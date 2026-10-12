#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
HRMS RAG Retrieval Verification Matrix
Kiểm tra chi tiết luồng tra cứu vector qua Qdrant và Ollama với 10 câu hỏi nghiệp vụ.
"""

import sys
import json
import urllib.request
import time

sys.stdout.reconfigure(encoding='utf-8')

QDRANT_URL = "http://127.0.0.1:6333"
OLLAMA_URL = "http://127.0.0.1:11434"
ALIAS_NAME = "hrms_knowledge_active"
EMBED_MODEL = "bge-m3:latest"

TEST_CASES = [
    {
        "id": 1,
        "category": "Giờ làm việc",
        "query": "Thời giờ làm việc bình thường theo Bộ luật lao động quy định thế nào?",
        "expected_clauses": ["PL-01/DIEU-01"],
        "policy_context": "GLOBAL",
        "description": "Hỏi thời giờ làm việc bình thường tối đa 8 giờ/ngày, 48 giờ/tuần."
    },
    {
        "id": 2,
        "category": "Ca làm việc",
        "query": "Quy định về thời gian nghỉ giữa ca và ca làm việc?",
        "expected_clauses": ["PL-01/DIEU-03", "PL-01/DIEU-04"],
        "policy_context": "GLOBAL",
        "description": "Hỏi ca làm việc, phân chia thời gian và nghỉ giữa ca."
    },
    {
        "id": 3,
        "category": "Làm thêm giờ",
        "query": "Số giờ làm thêm của người lao động được quy định tối đa bao nhiêu?",
        "expected_clauses": ["PL-02/DIEU-01", "PL-02/DIEU-02"],
        "policy_context": "GLOBAL",
        "description": "Hỏi giới hạn giờ làm thêm trong ngày, tháng, năm."
    },
    {
        "id": 4,
        "category": "Lương làm thêm giờ & ca đêm",
        "query": "Quy định tiền lương làm thêm giờ vào ban đêm được tính như thế nào?",
        "expected_clauses": ["PL-02/DIEU-04", "PL-01/DIEU-02"],
        "policy_context": "GLOBAL",
        "description": "Hỏi phụ cấp làm thêm ban đêm ít nhất 30% và 20% phụ trội."
    },
    {
        "id": 5,
        "category": "Nghỉ phép năm",
        "query": "Quy định người lao động làm việc trong điều kiện bình thường được nghỉ bao nhiêu ngày phép năm?",
        "expected_clauses": ["PL-03/DIEU-01"],
        "policy_context": "GLOBAL",
        "description": "Hỏi số ngày nghỉ phép năm tiêu chuẩn (12 ngày)."
    },
    {
        "id": 6,
        "category": "Thâm niên nghỉ phép",
        "query": "Quy định thâm niên làm việc bao nhiêu năm thì được tăng thêm ngày nghỉ phép hàng năm?",
        "expected_clauses": ["PL-03/DIEU-02"],
        "policy_context": "GLOBAL",
        "description": "Hỏi tăng ngày phép theo thâm niên (cứ đủ 05 năm thêm 01 ngày)."
    },
    {
        "id": 7,
        "category": "Nghỉ lễ tết",
        "query": "Quy định các ngày nghỉ lễ tết hưởng nguyên lương theo luật gồm những ngày nào?",
        "expected_clauses": ["PL-03/DIEU-03"],
        "policy_context": "GLOBAL",
        "description": "Hỏi 11 ngày nghỉ lễ tết hưởng nguyên lương trong năm."
    },
    {
        "id": 8,
        "category": "Trình tự kỷ luật",
        "query": "Trình tự và thủ tục xử lý kỷ luật lao động được thực hiện như thế nào?",
        "expected_clauses": ["PL-06/DIEU-02", "PL-06/DIEU-01"],
        "policy_context": "GLOBAL",
        "description": "Hỏi quy trình thông báo, thành phần họp và biên bản xử lý kỷ luật."
    },
    {
        "id": 9,
        "category": "Ngoài corpus",
        "query": "Quy định về phụ cấp nguy hiểm trong lò phản ứng hạt nhân của công ty?",
        "expected_clauses": [],
        "policy_context": "GLOBAL",
        "description": "Chủ đề không có trong tài liệu đã nạp. Kết quả phải không bịa đặt hoặc score thấp."
    },
    {
        "id": 10,
        "category": "Dự thảo chưa duyệt (Draft)",
        "query": "Cho tôi xem nội dung bản dự thảo quy chế phúc lợi CT-01 năm 2027?",
        "expected_clauses": [],
        "policy_context": "COMPANY",
        "description": "Tài liệu CT-01 ở trạng thái DRAFT_NOT_APPROVED, bị cổng Gate loại, không xuất hiện trong index."
    }
]

def get_embedding(text):
    payload = json.dumps({"model": EMBED_MODEL, "prompt": text}).encode('utf-8')
    req = urllib.request.Request(f"{OLLAMA_URL}/api/embeddings", data=payload, headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=60) as resp:
        return json.loads(resp.read().decode('utf-8'))["embedding"]

def search_qdrant(vector, company_id="GLOBAL", limit=5):
    body = {
        "vector": vector,
        "limit": limit,
        "score_threshold": 0.6,
        "filter": {
            "must": [
                {"key": "domain", "match": {"value": "POLICY"}},
                {"key": "tag", "match": {"value": "REGULATION"}},
                {"key": "status", "match": {"value": "ACTIVE_OFFICIAL"}},
                {"key": "verificationStatus", "match": {"value": "OFFICIAL_VERIFIED"}},
                {"key": "accessProfile", "match": {"value": "PUBLIC_INTERNAL"}},
                {"key": "appliesToCurrentCompany", "match": {"value": True}},
                {"key": "effectiveFrom", "range": {"lte": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())}},
                {"should": [ {"is_empty": {"key": "effectiveTo"}}, {"key": "effectiveTo", "range": {"gt": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())}} ]}
            ],
            "should": [
                {"key": "tenantId", "match": {"value": "GLOBAL"}},
                {"key": "tenantId", "match": {"value": company_id}}
            ]
        },
        "with_payload": True
    }
    req = urllib.request.Request(
        f"{QDRANT_URL}/collections/{ALIAS_NAME}/points/search",
        data=json.dumps(body).encode('utf-8'),
        headers={"Content-Type": "application/json"}
    )
    with urllib.request.urlopen(req, timeout=30) as resp:
        return json.loads(resp.read().decode('utf-8'))["result"]

def run_matrix():
    print("==========================================================================")
    print("      HRMS RAG RETRIEVAL VERIFICATION MATRIX ON QDRANT SERVING ALIAS      ")
    print(f"      Endpoint: {QDRANT_URL} | Serving Alias: {ALIAS_NAME}                ")
    print(f"      Embedding Model: {EMBED_MODEL} (1024 dims)                         ")
    print("==========================================================================")

    results = []
    passed = 0
    total = len(TEST_CASES)

    for tc in TEST_CASES:
        print(f"\n[TC {tc['id']}] {tc['category']}: \"{tc['query']}\"")
        vec = get_embedding(tc['query'])
        hits = search_qdrant(vec, tc.get('policy_context', 'GLOBAL'), limit=3)

        hit_clauses = [h.get("payload", {}).get("clauseId") for h in hits]
        top_hit = hits[0] if hits else None
        top_clause = top_hit.get("payload", {}).get("clauseId") if top_hit else None
        top_score = top_hit.get("score") if top_hit else 0.0
        top_title = top_hit.get("payload", {}).get("title") if top_hit else "None"
        top_doc = top_hit.get("payload", {}).get("documentTitle") if top_hit else "None"

        matched = False
        if len(tc["expected_clauses"]) == 0:
            # Mong đợi không trúng hoặc trúng điểm thấp / không chứa nội dung dự thảo
            if not hits:
                matched = True
                print(f"  -> [PASS] Không tìm thấy hoặc điểm tương đồng thấp ({top_score:.4f}). Không có hit đạt ngưỡng. Đây là kiểm thử retrieval, chưa kiểm thử sinh câu trả lời.")
            else:
                print(f"  -> [FAIL] Tìm thấy ngoài dự kiến: {top_clause} (Score: {top_score:.4f})")
        else:
            if any(exp in hit_clauses for exp in tc["expected_clauses"]):
                matched = True
                print(f"  -> [PASS] Top Hit: [{top_clause}] {top_title}")
                print(f"     Doc Title: {top_doc}")
                print(f"     Cosine Score: {top_score:.4f} | Hits: {', '.join(hit_clauses)}")
            else:
                print(f"  -> [FAIL] Expected {tc['expected_clauses']}, Actual Hits: {hit_clauses}")

        if matched:
            passed += 1

        results.append({
            "testCaseId": tc["id"],
            "category": tc["category"],
            "query": tc["query"],
            "expectedClauses": tc["expected_clauses"],
            "actualTopClause": top_clause,
            "actualTopTitle": top_title,
            "actualTopDoc": top_doc,
            "cosineScore": top_score,
            "allHits": hit_clauses,
            "passed": matched
        })

    print("\n==========================================================================")
    print(f"KẾT QUẢ MA TRẬN RETRIEVAL: {passed}/{total} PASSED ({(passed/total)*100:.1f}%)")
    print("==========================================================================")

    out_file = r"D:\QL_NS\QuanLyNhanSu\scratch\qdrant_retrieval_matrix_results.json"
    with open(out_file, 'w', encoding='utf-8') as fp:
        json.dump({
            "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
            "endpoint": QDRANT_URL,
            "alias": ALIAS_NAME,
            "embeddingModel": EMBED_MODEL,
            "totalCases": total,
            "passedCases": passed,
            "cases": results
        }, fp, ensure_ascii=False, indent=2)
    print(f"Báo cáo chi tiết đã lưu tại: {out_file}")
    return 0 if passed == total else 1

if __name__ == "__main__":
    sys.exit(run_matrix())
