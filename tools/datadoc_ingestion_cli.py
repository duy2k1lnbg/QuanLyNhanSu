#!/usr/bin/env python3
"""Validated DataDoc ingestion. apply -> verify -> publish, with immutable staging."""
import argparse
import collections
import datetime as dt
import hashlib
import json
import math
import os
from pathlib import Path
import re
import sys
import urllib.error
import urllib.request
import uuid

INDEX_FIELDS = [(f, 'keyword') for f in ['tenantId', 'sourceId', 'clauseId', 'section', 'domain', 'tag',
    'documentType', 'sourceKind', 'status', 'accessProfile', 'topics', 'version', 'corpusRevision',
    'companyId', 'allowedRoleIds', 'verificationStatus', 'applicabilityMode', 'embeddingModel', 'embeddingDigest']]
INDEX_FIELDS += [('effectiveFrom', 'datetime'), ('effectiveTo', 'datetime'), ('allowedDepartmentIds', 'integer')]
INPUT_VERSION = 'clause-text-v1'
DIMENSION = 1024
MODEL_DEFAULT = 'bge-m3:latest'

def generate_point_id(logical_identity):
    b = bytearray(hashlib.sha1(('HRMS_QDRANT_NAMESPACE:' + logical_identity).encode()).digest()[:16])
    b[6] = (b[6] & 15) | 80
    b[8] = (b[8] & 63) | 128
    return str(uuid.UUID(bytes=bytes(b)))

def http_json_request(url, method='GET', data=None, timeout=30):
    req = urllib.request.Request(url, method=method, headers={'Content-Type': 'application/json'},
        data=None if data is None else json.dumps(data).encode())
    try:
        with urllib.request.urlopen(req, timeout=timeout) as response:
            return response.status, json.load(response)
    except urllib.error.HTTPError as error:
        return error.code, json.loads(error.read())

def api(url, method='GET', data=None, timeout=30):
    status, response = http_json_request(url, method, data, timeout)
    if status != 200:
        raise ValueError(f'{method} {url}: HTTP {status}: {response}')
    return response.get('result', response)

def load_json(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))

def atomic_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding='utf-8')
    os.replace(temporary, path)

def load_document_titles(workspace_dir):
    titles = {}
    for path in (Path(workspace_dir) / 'DB_Doc').rglob('*.md'):
        text = path.read_text(encoding='utf-8-sig')
        match = re.match(r'^---\s*\n(.*?)\n---', text, re.S)
        if match:
            values = {}
            for line in match.group(1).splitlines():
                if ':' in line:
                    key, value = line.split(':', 1)
                    values[key.strip()] = value.strip().strip('"\'')
            if values.get('doc_id') and values.get('title'):
                if values['doc_id'] in titles:
                    raise ValueError('Duplicate document id: ' + values['doc_id'])
                titles[values['doc_id']] = values['title']
    return titles

def frontmatter(path):
    match = re.match(r'^---\s*\n(.*?)\n---', Path(path).read_text(encoding='utf-8-sig'), re.S)
    if not match:
        raise ValueError('Missing frontmatter: ' + str(path))
    values, list_key = {}, None
    def scalar(text):
        try:
            return json.loads(text)
        except json.JSONDecodeError:
            return text.strip('"\'')
    for raw in match.group(1).splitlines():
        line = raw.strip()
        if not line or line.startswith('#'):
            continue
        if line.startswith('- ') and list_key:
            values[list_key].append(scalar(line[2:].strip()))
        elif ':' in line and not raw.startswith(' '):
            key, value = line.split(':', 1)
            list_key = key if not value.strip() else None
            values[key] = [] if list_key else scalar(value.strip())
        else:
            raise ValueError('Unsupported frontmatter syntax: ' + str(path))
    return values

def schema_errors(value, schema, prefix='clause'):
    """Validate the draft-07 keywords used by the repository schemas; no optional dependency."""
    errors = []
    types = {'object': lambda v: isinstance(v, dict), 'array': lambda v: isinstance(v, list),
        'string': lambda v: isinstance(v, str), 'null': lambda v: v is None,
        'boolean': lambda v: isinstance(v, bool), 'integer': lambda v: isinstance(v, int) and not isinstance(v, bool)}
    if 'type' in schema:
        accepted = schema['type'] if isinstance(schema['type'], list) else [schema['type']]
        if not any(types[t](value) for t in accepted):
            return [prefix + ': invalid type']
    if 'enum' in schema and value not in schema['enum']:
        errors.append(prefix + ': invalid enum')
    if 'const' in schema and (value != schema['const'] or type(value) is not type(schema['const'])):
        errors.append(prefix + ': invalid constant')
    if isinstance(value, str):
        if len(value) < schema.get('minLength', 0):
            errors.append(prefix + ': too short')
        if 'pattern' in schema and not re.search(schema['pattern'], value):
            errors.append(prefix + ': invalid pattern')
        if schema.get('format') == 'date':
            try:
                dt.date.fromisoformat(value)
            except ValueError:
                errors.append(prefix + ': invalid date')
    if isinstance(value, list):
        if len(value) < schema.get('minItems', 0):
            errors.append(prefix + ': too few items')
        for index, item in enumerate(value):
            errors += schema_errors(item, schema.get('items', {}), f'{prefix}[{index}]')
    if isinstance(value, dict):
        for field in schema.get('required', []):
            if field not in value:
                errors.append(prefix + ': missing ' + field)
        props = schema.get('properties', {})
        for field, item in value.items():
            if schema.get('additionalProperties') is False and field not in props:
                errors.append(prefix + ': unexpected ' + field)
            if field in props:
                errors += schema_errors(item, props[field], prefix + '.' + field)
    for rule in schema.get('allOf', []):
        if 'if' in rule and not schema_errors(value, rule['if'], prefix):
            errors += schema_errors(value, rule.get('then', {}), prefix)
        elif 'if' not in rule:
            errors += schema_errors(value, rule, prefix)
    return errors

def tenant_for(c):
    if c['source_kind'] == 'VIETNAM_LAW':
        if c.get('company_id'):
            raise ValueError('National law must not own a company tenant')
        return 'GLOBAL'
    company = c.get('company_id')
    if not isinstance(company, str) or not company.strip() or company.strip().upper() == 'GLOBAL':
        raise ValueError('Company document requires a non-GLOBAL company_id')
    return company.strip()

def normalized_clause_id(c):
    return re.sub(r'_chunk_\d+$', '', c['clause_id'])

def identity_for(c):
    return f"{tenant_for(c)}:{c['doc_id']}:{c['version']}:{normalized_clause_id(c)}:{c.get('chunk_index', 0)}"

def acl_values(c, snake, camel):
    if snake in c and camel in c and c[snake] != c[camel]:
        raise ValueError('Conflicting ACL names: ' + snake)
    return c.get(snake, c.get(camel, []))

def gate_reasons(c):
    reasons = []
    for field, expected in [('status', 'ACTIVE_OFFICIAL'), ('verification_status', 'OFFICIAL_VERIFIED'),
        ('ingest_enabled', True), ('applies_to_current_company', True)]:
        if c.get(field) != expected or (isinstance(expected, bool) and c.get(field) is not expected):
            reasons.append(field + ' does not permit ingestion')
    if c.get('unresolved_fields') != []:
        reasons.append('Unresolved or missing approval fields')
    if c.get('applicability_mode') == 'UNRESOLVED':
        reasons.append('Unresolved applicability')
    return reasons

def inspect_and_filter_clauses(workspace_dir, titles):
    root = Path(workspace_dir) / 'DB_Doc/06_RAG'
    schema = load_json(root / 'clause.schema.json')
    rows = [json.loads(line) for line in (root / 'clauses.jsonl').read_text(encoding='utf-8-sig').splitlines() if line.strip()]
    eligible, excluded, seen = [], [], set()
    for c in rows:
        reasons = gate_reasons(c)
        structural = schema_errors(c, schema)
        if structural:
            # Invalid eligible data must fail the run, not quietly reduce the published corpus.
            if not reasons:
                raise ValueError(f"{c.get('clause_id')}: " + '; '.join(structural))
            reasons += structural
        if not reasons:
            c['documentTitle'] = titles.get(c['doc_id'], '')
            if not c['documentTitle']:
                raise ValueError('Missing document title: ' + c['doc_id'])
            if c['content'] != c['text'] or c.get('content_hash') != hashlib.sha256(c['text'].encode()).hexdigest():
                raise ValueError('Invalid content/hash: ' + c['clause_id'])
            if not c.get('version') or not c.get('approved_by') or not c.get('effective_from'):
                raise ValueError('Missing version, approval or effective date: ' + c['clause_id'])
            tenant_for(c)
            roles = acl_values(c, 'allowed_role_ids', 'allowedRoleIds')
            departments = acl_values(c, 'allowed_department_ids', 'allowedDepartmentIds')
            if any(not isinstance(r, str) or not r.strip() for r in roles) or any(type(d) is not int or d <= 0 for d in departments):
                raise ValueError('Invalid ACL identifiers: ' + c['clause_id'])
            if (c['access_profile'] != 'PUBLIC_INTERNAL' or c['applicability_mode'] == 'RESTRICTED') and not (roles or departments):
                raise ValueError('Restricted document has empty ACL: ' + c['clause_id'])
            start = dt.date.fromisoformat(c['effective_from'])
            if c.get('effective_to') and dt.date.fromisoformat(c['effective_to']) <= start:
                raise ValueError('Invalid effective interval: ' + c['clause_id'])
            if type(c.get('chunk_index', 0)) is not int or type(c.get('total_chunks', 1)) is not int or not 0 <= c.get('chunk_index', 0) < c.get('total_chunks', 1):
                raise ValueError('Invalid chunk indexes: ' + c['clause_id'])
            identity = identity_for(c)
            if identity in seen:
                raise ValueError('Duplicate logical identity: ' + identity)
            seen.add(identity)
            eligible.append(c)
        else:
            excluded.append({'clause_id': c.get('clause_id'), 'doc_id': c.get('doc_id'), 'reasons': reasons})
    groups = collections.defaultdict(list)
    for c in eligible:
        groups[(tenant_for(c), c['doc_id'], c['version'], normalized_clause_id(c))].append(c)
    for group in groups.values():
        count = group[0].get('total_chunks', 1)
        if any(c.get('total_chunks', 1) != count for c in group) or {c.get('chunk_index', 0) for c in group} != set(range(count)):
            raise ValueError('Missing or inconsistent chunks: ' + group[0]['clause_id'])
    return rows, eligible, excluded

def validate_manifest(workspace, eligible):
    folder = Path(workspace) / 'DB_Doc'
    manifest = load_json(folder / '06_RAG/manifest.json')
    docs = {d['doc_id']: d for d in manifest['documents']}
    if len(docs) != len(manifest['documents']):
        raise ValueError('Duplicate manifest document ids')
    schema = load_json(folder / '06_RAG/document.schema.json')
    for doc_id in {c['doc_id'] for c in eligible}:
        doc = docs.get(doc_id)
        if not doc or doc.get('ingest_enabled') is not True:
            raise ValueError('Manifest disables or omits document: ' + doc_id)
        path = (folder / doc['relative_path']).resolve()
        if not path.is_relative_to(folder.resolve()):
            raise ValueError('Manifest escapes document folder')
        if hashlib.sha256(path.read_bytes()).hexdigest() != doc['sha256']:
            raise ValueError('Source file hash mismatch: ' + doc_id)
        metadata = frontmatter(path)
        errors = schema_errors(metadata, schema, doc_id)
        if errors or metadata.get('ingest_enabled') is not True:
            raise ValueError('Invalid/disabled source frontmatter: ' + doc_id + ': ' + '; '.join(errors))
        for c in [c for c in eligible if c['doc_id'] == doc_id]:
            for field in ['doc_id', 'version', 'status', 'source_kind', 'document_type', 'access_profile', 'company_id', 'effective_from', 'effective_to', 'applicability_mode', 'applies_to_current_company']:
                if metadata.get(field) != c.get(field):
                    raise ValueError('Source/clause metadata drift: ' + doc_id + ':' + field)
            for snake, camel in [('allowed_role_ids','allowedRoleIds'), ('allowed_department_ids','allowedDepartmentIds')]:
                if acl_values(metadata, snake, camel) != acl_values(c, snake, camel):
                    raise ValueError('Source/clause ACL drift: ' + doc_id + ':' + snake)

def corpus(args):
    rows, eligible, excluded = inspect_and_filter_clauses(args.workspace, load_document_titles(args.workspace))
    validate_manifest(args.workspace, eligible)
    if not eligible:
        raise ValueError('No eligible clauses')
    return rows, eligible, excluded

def utc_effective(value):
    if not value:
        return None
    # Vietnamese effective dates begin at 00:00 Asia/Ho_Chi_Minh, UTC+07.
    local = dt.datetime.combine(dt.date.fromisoformat(value), dt.time(), tzinfo=dt.timezone(dt.timedelta(hours=7)))
    return local.astimezone(dt.timezone.utc).isoformat().replace('+00:00', 'Z')

def model_descriptor(args):
    tags = api(args.ollama_url.rstrip('/') + '/api/tags')['models']
    requested = args.model if ':' in args.model else args.model + ':latest'
    model = next((m for m in tags if m['name'] == requested), None)
    if not model or not model.get('digest'):
        raise ValueError('Configured embedding model is not installed; no automatic model selection')
    return {'model': requested, 'digest': model['digest'], 'dimension': DIMENSION, 'distance': 'Cosine', 'inputVersion': INPUT_VERSION}

def payload_for(c, descriptor, revision, run_id, now):
    clause = normalized_clause_id(c)
    payload = {
        'tenantId': tenant_for(c), 'sourceId': c['doc_id'], 'clauseId': clause, 'section': clause,
        'chunkId': f"{clause}_chunk_{c.get('chunk_index', 0)}", 'chunkIndex': c.get('chunk_index', 0),
        'totalChunks': c.get('total_chunks', 1), 'parentClauseId': c.get('parent_clause_id', c.get('parentClauseId')),
        'relatedClauseIds': c.get('related_clause_ids', c.get('relatedClauseIds', [])),
        'version': c['version'], 'text': c['text'], 'title': c['clause_title'], 'documentTitle': c['documentTitle'],
        'sourceKind': c['source_kind'], 'documentType': c['document_type'], 'domain': 'POLICY', 'tag': 'REGULATION',
        'topics': c['topics'], 'accessProfile': c['access_profile'],
        'allowedRoleIds': acl_values(c, 'allowed_role_ids', 'allowedRoleIds'),
        'allowedDepartmentIds': acl_values(c, 'allowed_department_ids', 'allowedDepartmentIds'),
        'applicabilityMode': c['applicability_mode'], 'applicableLocation': c['applicable_location'],
        'appliesToCurrentCompany': c['applies_to_current_company'], 'jurisdiction': c['jurisdiction'], 'companyId': c.get('company_id'),
        'status': c['status'], 'effectiveFrom': utc_effective(c.get('effective_from')), 'effectiveTo': utc_effective(c.get('effective_to')),
        'approvedBy': c['approved_by'], 'verificationStatus': c['verification_status'], 'supersedes': c.get('supersedes'),
        'unresolvedFields': c['unresolved_fields'], 'sourceUrls': c.get('source_urls', []), 'sourcePages': c.get('source_pages', []),
        'legalBasis': c.get('legal_basis', []), 'contentHash': hashlib.sha256(c['text'].encode()).hexdigest(),
        'embeddingModel': descriptor['model'], 'embeddingDigest': descriptor['digest'],
        'embeddingDimension': DIMENSION, 'embeddingInputVersion': INPUT_VERSION,
        'corpusRevision': revision, 'ingestionRunId': run_id, 'importedAt': now, 'updatedAt': now}
    # Stable metadata hash excludes import timestamps/run id.
    payload['metadataHash'] = hashlib.sha256(json.dumps({k: v for k, v in payload.items() if k not in ['importedAt', 'updatedAt', 'ingestionRunId']}, sort_keys=True, ensure_ascii=False).encode()).hexdigest()
    return payload

def scroll(args, collection):
    points, offset = [], None
    while True:
        body = {'limit': 100, 'with_payload': True, 'with_vector': True}
        if offset is not None:
            body['offset'] = offset
        result = api(f'{args.qdrant_url}/collections/{collection}/points/scroll', 'POST', body)
        points += result['points']
        offset = result.get('next_page_offset')
        if offset is None:
            return points

def valid_vector(vector):
    return isinstance(vector, list) and len(vector) == DIMENSION and all(type(v) in [int, float] and math.isfinite(v) for v in vector) and any(vector)

def receipt_path(args):
    return Path(args.receipt or (Path(args.workspace) / 'scratch/datadoc_ingestion_receipt_v2.json'))

def verify_run(args, receipt):
    _, eligible, _ = corpus(args)
    collection = receipt['physicalCollection']
    info = api(f'{args.qdrant_url}/collections/{collection}')
    vectors = info['config']['params']['vectors']
    if vectors.get('size') != DIMENSION or vectors.get('distance') != 'Cosine':
        raise ValueError('Collection vector descriptor mismatch')
    missing_indexes = {f for f, _ in INDEX_FIELDS} - set(info.get('payload_schema', {}))
    if missing_indexes:
        raise ValueError('Missing payload indexes: ' + ', '.join(sorted(missing_indexes)))
    actual = {p['id']: p for p in scroll(args, collection)}
    expected = {generate_point_id(identity_for(c)): payload_for(c, receipt['descriptor'], receipt['corpusRevision'], receipt['ingestionRunId'], receipt['timestamp']) for c in eligible}
    if set(actual) != set(expected) or set(receipt['pointIds']) != set(expected):
        raise ValueError('Point identity set mismatch; orphan/missing data')
    for point_id, payload in expected.items():
        if actual[point_id]['payload'] != payload or not valid_vector(actual[point_id]['vector']):
            raise ValueError('Invalid payload/hash/vector: ' + point_id)
    employee = api(f'{args.qdrant_url}/collections/hrms_vectors_v2')['points_count']
    if employee != receipt['employeePointsBefore']:
        raise ValueError('Employee collection count changed; investigate before publishing')
    model = model_descriptor(args)
    if model != receipt['descriptor']:
        raise ValueError('Embedding model descriptor changed during ingestion')
    embed = api(args.ollama_url + '/api/embeddings', 'POST', {'model': model['model'], 'prompt': 'Thời giờ làm việc bình thường'}, timeout=60)['embedding']
    if not valid_vector(embed):
        raise ValueError('Invalid query vector')
    hits = api(f'{args.qdrant_url}/collections/{collection}/points/search', 'POST', {
        'vector': embed, 'limit': 5, 'score_threshold': 0.6, 'with_payload': True,
        'filter': {'must': [{'key': 'domain', 'match': {'value': 'POLICY'}}, {'key': 'status', 'match': {'value': 'ACTIVE_OFFICIAL'}}]}})
    if 'PL-01' in {c['doc_id'] for c in eligible} and not any(h['payload']['clauseId'] == 'PL-01/DIEU-01' for h in hits):
        raise ValueError('Expected legal evidence missing in retrieval smoke test')
    if not hits:
        raise ValueError('Empty retrieval smoke test')
    return len(actual)

def cmd_validate(args):
    rows, eligible, excluded = corpus(args)
    print(json.dumps({'total': len(rows), 'eligible': len(eligible), 'excluded': len(excluded)}, ensure_ascii=False))
    return 0

def cmd_dry_run(args):
    rows, eligible, excluded = corpus(args)
    result = {'totalClauses': len(rows), 'eligibleClauses': len(eligible), 'excludedClauses': len(excluded),
        'eligible': [{'clauseId': c['clause_id'], 'tenantId': tenant_for(c), 'pointId': generate_point_id(identity_for(c)), 'contentHash': c['content_hash']} for c in eligible], 'excluded': excluded}
    atomic_json(Path(args.workspace) / 'scratch/datadoc_dry_run_report.json', result)
    print(f"Dry-run: {len(eligible)} eligible; {len(excluded)} excluded. No Qdrant write.")
    return 0

def cmd_apply(args):
    _, eligible, _ = corpus(args)
    descriptor = model_descriptor(args)
    aliases = api(args.qdrant_url + '/aliases')['aliases']
    run_id = str(uuid.uuid4())
    now = dt.datetime.now(dt.timezone.utc).isoformat().replace('+00:00', 'Z')
    collection = args.collection or 'hrms_knowledge_bgem3_' + dt.datetime.now(dt.timezone.utc).strftime('%Y%m%dT%H%M%S') + '_' + run_id[:8]
    if collection == 'hrms_vectors_v2' or any(a['collection_name'] == collection for a in aliases):
        raise ValueError('Refusing to write employee or active collection')
    status, _ = http_json_request(f'{args.qdrant_url}/collections/{collection}')
    if status != 404:
        raise ValueError('Staging collection must be new; response=' + str(status))
    employee_count = api(args.qdrant_url + '/collections/hrms_vectors_v2')['points_count']
    receipt = {'status': 'INGESTING', 'physicalCollection': collection, 'alias': args.alias, 'descriptor': descriptor,
        'embeddingModel': descriptor['model'], 'corpusRevision': args.revision or run_id,
        'ingestionRunId': run_id, 'timestamp': now, 'employeePointsBefore': employee_count,
        'pointIds': [generate_point_id(identity_for(c)) for c in eligible], 'pointsUpserted': 0}
    atomic_json(receipt_path(args), receipt)
    api(f'{args.qdrant_url}/collections/{collection}', 'PUT', {'vectors': {'size': DIMENSION, 'distance': 'Cosine'}})
    for field, kind in INDEX_FIELDS:
        api(f'{args.qdrant_url}/collections/{collection}/index?wait=true', 'PUT', {'field_name': field, 'field_schema': kind})
    # A complete fresh staging corpus removes orphans without deleting the serving collection.
    for index, c in enumerate(eligible, 1):
        embedding = api(args.ollama_url + '/api/embeddings', 'POST', {'model': descriptor['model'], 'prompt': c['text']}, timeout=120)['embedding']
        if not valid_vector(embedding):
            raise ValueError('Embedding dimension/value mismatch: ' + c['clause_id'])
        api(f'{args.qdrant_url}/collections/{collection}/points?wait=true', 'PUT', {'points': [{
            'id': generate_point_id(identity_for(c)), 'vector': embedding,
            'payload': payload_for(c, descriptor, receipt['corpusRevision'], run_id, now)}]})
        receipt['pointsUpserted'] = index
        atomic_json(receipt_path(args), receipt)
        print(f"[{index}/{len(eligible)}] {c['clause_id']}", flush=True)
    receipt['status'] = 'STAGED'
    atomic_json(receipt_path(args), receipt)
    print('Staged successfully. Run verify, then publish; serving alias unchanged.')
    return 0

def cmd_verify(args):
    receipt = load_json(receipt_path(args))
    if receipt['status'] not in ['STAGED', 'VERIFIED', 'PUBLISHED']:
        raise ValueError('Ingestion is incomplete')
    count = verify_run(args, receipt)
    if receipt['status'] != 'PUBLISHED':
        receipt['status'] = 'VERIFIED'
    receipt['verifiedAt'] = dt.datetime.now(dt.timezone.utc).isoformat()
    atomic_json(receipt_path(args), receipt)
    print(f'Verified {count} IDs, full payloads, hashes, vectors, indexes, model digest and retrieval.')
    return 0

def cmd_publish(args):
    receipt = load_json(receipt_path(args))
    if receipt.get('status') not in ['VERIFIED', 'PUBLISHED'] or not receipt.get('verifiedAt'):
        raise ValueError('Publish requires a verified receipt')
    verify_run(args, receipt)  # Recheck corpus/model immediately before promotion.
    aliases = api(args.qdrant_url + '/aliases')['aliases']
    previous = next((a['collection_name'] for a in aliases if a['alias_name'] == receipt['alias']), None)
    if previous != receipt['physicalCollection']:
        actions = ([{'delete_alias': {'alias_name': receipt['alias']}}] if previous else [])
        actions += [{'create_alias': {'alias_name': receipt['alias'], 'collection_name': receipt['physicalCollection']}}]
        api(args.qdrant_url + '/collections/aliases', 'POST', {'actions': actions})
    after = api(args.qdrant_url + '/aliases')['aliases']
    if not any(a['alias_name'] == receipt['alias'] and a['collection_name'] == receipt['physicalCollection'] for a in after):
        raise ValueError('Alias publication verification failed')
    receipt.update(status='PUBLISHED', previousCollection=receipt.get('previousCollection', previous),
        publishedAt=dt.datetime.now(dt.timezone.utc).isoformat())
    atomic_json(receipt_path(args), receipt)
    print(f"Published {receipt['alias']} -> {receipt['physicalCollection']}; previous collection retained.")
    return 0

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=['validate', 'dry-run', 'apply', 'verify', 'publish'])
    parser.add_argument('--workspace', default=r'D:\QL_NS\QuanLyNhanSu')
    parser.add_argument('--qdrant-url', default=os.environ.get('QDRANT_URL', 'http://127.0.0.1:6333'))
    parser.add_argument('--ollama-url', default=os.environ.get('OLLAMA_URL', 'http://127.0.0.1:11434'))
    parser.add_argument('--model', default=MODEL_DEFAULT)
    parser.add_argument('--collection', help='New staging collection only; unique name generated by default')
    parser.add_argument('--alias', default='hrms_knowledge_active')
    parser.add_argument('--revision')
    parser.add_argument('--receipt')
    args = parser.parse_args()
    args.qdrant_url = args.qdrant_url.rstrip('/')
    args.ollama_url = args.ollama_url.rstrip('/')
    try:
        return globals()['cmd_' + args.command](args)
    except (ValueError, KeyError, OSError) as error:
        print('FAILED: ' + str(error), file=sys.stderr)
        return 1

if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    sys.exit(main())
