import sys
sys.dont_write_bytecode = True
import argparse, copy, importlib.util, json, unittest
from pathlib import Path
from unittest.mock import patch
ROOT = Path(__file__).resolve().parents[1] if Path(__file__).parent.name == 'tools' else Path(r'D:\QL_NS\QuanLyNhanSu')
spec = importlib.util.spec_from_file_location('cli', ROOT / 'tools/datadoc_ingestion_cli.py')
cli = importlib.util.module_from_spec(spec)
spec.loader.exec_module(cli)
SCHEMA = cli.load_json(ROOT / 'DB_Doc/06_RAG/clause.schema.json')
SAMPLE = json.loads((ROOT / 'DB_Doc/06_RAG/clauses.jsonl').read_text(encoding='utf-8').splitlines()[0])
class IngestionTests(unittest.TestCase):
    def inspect(self, changes=None, copies=1):
        row = copy.deepcopy(SAMPLE)
        row.update(changes or {})
        with patch.object(cli, 'load_json', return_value=SCHEMA), patch.object(Path, 'read_text', return_value='\n'.join(json.dumps(row) for _ in range(copies))):
            return cli.inspect_and_filter_clauses('offline-fixture', {row['doc_id']: 'Verified legal document'})
    def test_real_corpus(self):
        rows, eligible, excluded = cli.corpus(argparse.Namespace(workspace=str(ROOT)))
        self.assertEqual((len(rows), len(eligible), len(excluded)), (123, 26, 97))
    def test_disabled_ingestion_is_excluded(self):
        self.assertEqual(len(self.inspect({'ingest_enabled':False})[1]), 0)
    def test_unverified_is_excluded(self):
        self.assertEqual(len(self.inspect({'verification_status':'DRAFT_PENDING_CONFIRMATION'})[1]), 0)
    def test_unresolved_is_excluded(self):
        self.assertEqual(len(self.inspect({'unresolved_fields':['company_location']})[1]), 0)
    def test_not_applicable_is_excluded(self):
        self.assertEqual(len(self.inspect({'applies_to_current_company':False})[1]), 0)
    def test_invalid_content_is_fatal(self):
        with self.assertRaises(ValueError): self.inspect({'text':'','content':''})
    def test_invalid_hash_is_fatal(self):
        with self.assertRaises(ValueError): self.inspect({'content_hash':'0'*64})
    def test_missing_company_is_fatal(self):
        with self.assertRaises(ValueError): self.inspect({'source_kind':'COMPANY_POLICY','company_id':None})
    def test_empty_restricted_acl_is_fatal(self):
        with self.assertRaises(ValueError): self.inspect({'access_profile':'CONFIDENTIAL'})
    def test_duplicate_identity_is_fatal(self):
        with self.assertRaises(ValueError): self.inspect(copies=2)
    def test_missing_chunk_is_fatal(self):
        with self.assertRaises(ValueError): self.inspect({'total_chunks':2})
    def test_company_jurisdiction_does_not_become_global(self):
        row=copy.deepcopy(SAMPLE); row.update(source_kind='COMPANY_POLICY', company_id='CTY_1', jurisdiction='VN')
        self.assertEqual(cli.tenant_for(row), 'CTY_1')
        id1=cli.generate_point_id(cli.identity_for(row)); row['company_id']='CTY_2'
        self.assertNotEqual(id1, cli.generate_point_id(cli.identity_for(row)))
    def test_stable_legacy_identity(self):
        self.assertEqual(cli.generate_point_id(cli.identity_for(SAMPLE)), '7ce63aab-5d9a-5ac3-b3b7-0e4ef39a4ea8')
    def test_vietnam_effective_date_boundary(self):
        self.assertEqual(cli.utc_effective('2026-10-11'), '2026-10-10T17:00:00Z')
    def test_publish_before_verify_is_rejected_without_qdrant_write(self):
        args=argparse.Namespace(workspace=str(ROOT),receipt='offline-receipt')
        with patch.object(cli, 'load_json', return_value={'status':'STAGED'}), patch.object(cli, 'api') as api:
            with self.assertRaises(ValueError): cli.cmd_publish(args)
            api.assert_not_called()
    def test_active_collection_cannot_be_modified(self):
        args=argparse.Namespace(workspace=str(ROOT), collection='active_collection', qdrant_url='http://offline')
        with patch.object(cli, 'corpus', return_value=([],[SAMPLE],[])), patch.object(cli, 'model_descriptor', return_value={}), patch.object(cli, 'api', return_value={'aliases':[{'alias_name':'hrms_knowledge_active','collection_name':'active_collection'}]}) as api:
            with self.assertRaises(ValueError): cli.cmd_apply(args)
            self.assertEqual(api.call_count,1)
    def test_index_dimensions_and_model_mismatch_fail_verification(self):
        args=argparse.Namespace(workspace=str(ROOT),qdrant_url='http://offline')
        receipt={'physicalCollection':'staging'}
        with patch.object(cli,'corpus',return_value=([],[SAMPLE],[])), patch.object(cli,'api',return_value={'config':{'params':{'vectors':{'size':384,'distance':'Cosine'}}}}):
            with self.assertRaises(ValueError): cli.verify_run(args,receipt)

if __name__ == '__main__':
    unittest.main(verbosity=2)
