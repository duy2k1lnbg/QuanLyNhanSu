# Báo cáo kiểm kê kỹ thuật và khoảng trống tích hợp RAG (11/10/2026)

Tài liệu này ghi nhận kết quả rà soát thực tế mã nguồn backend, cấu hình vector store và dữ liệu CSDL cho luồng tra cứu chính sách (`POLICY_LOOKUP`).

---

## 1. Kết quả kiểm kê từ mã nguồn (Source Code Inspection)

Dưới đây là các bằng chứng trực tiếp từ mã nguồn C# trong repository `D:\QL_NS\QuanLyNhanSu`:

### 1.1. Lệch kiểu dữ liệu `companyId` trong QdrantService
- **Tập tin:** `HRMS.Business\Services\AI_Services\Retrieval\Vector\QdrantService.cs`
- **Bằng chứng tại dòng 164:**
  ```csharp
  if (!string.IsNullOrWhiteSpace(companyId)) payload["companyId"] = companyId;
  ```
  Khi lưu payload vào Qdrant qua `AddAsync`, `companyId` được lưu dưới dạng chuỗi (`string`, ví dụ: `"1"`).
- **Bằng chứng tại dòng 400–405:**
  ```csharp
  int compIdInt;
  if (int.TryParse(securityFilter.AllowedCompanyId, out compIdInt))
  {
      mustConditions.Add(new { key = "companyId", match = new { value = compIdInt } });
  }
  else
  {
      mustConditions.Add(new { key = "companyId", match = new { value = securityFilter.AllowedCompanyId } });
  }
  ```
  Khi thực hiện tìm kiếm có phân quyền (`SearchWithScopeAsync`), nếu `AllowedCompanyId` là số (ví dụ `"1"`), bộ lọc Qdrant tạo match theo kiểu số nguyên (`integer: 1`).
- **Hậu quả kỹ thuật:** Qdrant coi `1` (integer) và `"1"` (string) là hai kiểu khác nhau trong payload schema. Việc này dẫn đến hiện tượng **nạp thành công nhưng truy vấn trả về 0 kết quả**. Cần chuẩn hóa toàn bộ `companyId` trong payload và filter về cùng kiểu chuỗi (`string`).

---

### 1.2. Nguy cơ ghi đè Point ID khi nạp nhiều điều khoản
- **Tập tin:** `HRMS.Business\Services\AI_Services\Retrieval\Vector\QdrantService.cs`
- **Bằng chứng tại dòng 43–45:**
  ```csharp
  public static string GeneratePointId(string sourceCode, string entityKey, string visibilityProfile = "PUBLIC", string chunkKey = "0")
  {
      string input = $"{sourceCode}:{entityKey}:{visibilityProfile}:{chunkKey}";
      // Sinh UUID v5 theo namespace HRMS_QDRANT_NAMESPACE
  ```
- **Bằng chứng tại dòng 152:**
  ```csharp
  var pointId = GeneratePointId(tag, employeeId.HasValue ? employeeId.Value.ToString() : (sourceId ?? text.GetHashCode().ToString()));
  ```
- **Hậu quả kỹ thuật:** Khi nạp tài liệu quy chế văn bản có nhiều điều khoản cùng chung `sourceId` (hoặc cùng `tag`), phương thức `AddAsync` không truyền `chunkKey` mà nhận giá trị mặc định `"0"`. Do đó, tất cả các điều khoản hoặc các đoạn chunk của cùng một văn bản sẽ sinh ra **cùng một UUID pointId**, điều này dẫn đến việc **điều khoản sau ghi đè điều khoản trước trong Qdrant**, làm mất mát hầu hết các điều khoản trong văn bản.

---

### 1.3. Lệch Tag lọc dữ liệu trong AiExecutionService
- **Tập tin:** `HRMS.Business\Services\AI_Services\Chat\AiExecutionService.cs`
- **Bằng chứng tại dòng 365 và dòng 498:**
  ```csharp
  // Dòng 365:
  var bizFilter = new Bu.Services.AI_Services.Vector.VectorBusinessFilter { Tag = plan.Domain == "POLICY" ? "REGULATION" : plan.Domain, Domain = plan.Domain };
  // Dòng 498 (nhánh Hybrid):
  var bizFilter = new Bu.Services.AI_Services.Vector.VectorBusinessFilter { Tag = "REGULATION", Domain = "POLICY" };
  ```
- **Hậu quả kỹ thuật:** Bộ lọc server của `AiExecutionService` luôn ép `Tag = "REGULATION"` và `Domain = "POLICY"` khi tra cứu văn bản quy chế. Nếu dữ liệu nạp vào Qdrant đặt `tag = "POLICY"` hoặc `tag = "LEGAL"` mà không có cơ chế tương thích, reader hiện tại sẽ lọc bỏ hoàn toàn các vector này.

---

### 1.4. Trạng thái phân quyền và Capability `POLICY_LOOKUP`
- **Tập tin:** `HRMS.Business\Services\AI_Services\Security\Authorization\AiCapabilityCatalog.cs` (dòng 214–226):
  Đã khai báo capability `POLICY_LOOKUP` trong catalog C#.
- **Tập tin:** `HRMS.Business\Services\AI_Services\Planning\QueryPlanner.cs` (dòng 33, dòng 73–78, dòng 191–194):
  Đã định tuyến câu hỏi có ý định quy định/chính sách sang chiến lược `ExecutionStrategy.VectorSearch` hoặc `ExecutionStrategy.Hybrid` với `VectorCapability = "POLICY_LOOKUP"`.
- **Khoảng trống CSDL:** Bảng danh mục `TB_AI_CAPABILITY` trong Oracle ở snapshot kiểm tra ngày 11/10/2026 chỉ có 16 capability gốc và chưa đăng ký `POLICY_LOOKUP`. Cần chạy script migration đăng ký capability này vào Oracle trước khi kích hoạt trên môi trường thực tế.

---

## 2. Kiểm kê trạng thái dữ liệu thực tế (Snapshot ngày 11/10/2026)

| Thành phần | Trạng thái quan sát | Đánh giá |
|---|---|---|
| **Qdrant Collection `hrms_vectors_v2`** | 297 points, vector size 1024, distance Cosine. 100% points có `tag = "EMPLOYEE"`. 0 point có `tag = "REGULATION"`, 0 point có `domain = "POLICY"`. | Chưa có bất kỳ văn bản chính sách/quy chế nào được nạp vào vector store. |
| **Oracle `TB_HESO_TANGCA`** | 7 bản ghi cấu hình hệ số tăng ca theo khung giờ và loại ngày. | Có dữ liệu cấu hình có cấu trúc, nhưng chưa có văn bản quy chế đi kèm. |
| **Oracle `TB_YEUCAU_TANGCA`** | 5 bản ghi yêu cầu tăng ca thực tế. | Dữ liệu giao dịch phát sinh, không phải quy chế. |
| **Oracle `TB_NGAYLE`** | 11 bản ghi ngày lễ năm 2026. | Danh mục ngày nghỉ lễ hợp pháp; không có quy chế thưởng lễ. |
| **Oracle `TB_KHENTHUONG_KYLUAT`** | 9 bản ghi, 100% có `LOAI = 1` (khen thưởng). Không có bản ghi kỷ luật (`LOAI = 2`). | Chưa có quy chế xử lý kỷ luật lao động và chưa có quyết định xử phạt. |
| **Oracle `TB_NANGLUONG_NHANVIEN`** | 0 bản ghi. | Chưa có quyết định nâng lương thực tế và chưa có văn bản tiêu chuẩn nâng bậc. |

---

## 3. Khoảng trống giữa Metadata thiết kế và Bộ lọc Server hiện tại

| Trường Metadata thiết kế | Hỗ trợ trong reader hiện tại (`QdrantService.cs`) | Khoảng trống cần xử lý khi tích hợp |
|---|---|---|
| `companyId` | Có hỗ trợ filter trong QdrantService nhưng bị lỗi kiểu `string` vs `int` | Cần sửa QdrantService đồng nhất `string` và cập nhật payload schema. |
| `departmentId` | Có hỗ trợ filter theo danh sách phòng ban được cấp quyền (`AllowedDepartmentIds`) | Đạt yêu cầu cho văn bản cấp phòng ban. |
| `tag` / `domain` | Có hỗ trợ, nhưng reader đang hardcode `Tag = "REGULATION"`, `Domain = "POLICY"` | Khi nạp tài liệu mới, payload phải duy trì `tag: "REGULATION"` và `domain: "POLICY"` để tương thích ngược. |
| `status` (`DRAFT_NOT_APPROVED`, `APPROVED`) | **Chưa có trong reader hiện tại** | Reader hiện tại chưa lọc theo trường `status`. Do đó, **không được nạp các bản nháp (`DRAFT_NOT_APPROVED`) vào collection sản xuất** cho đến khi reader được nâng cấp bộ lọc `status == "APPROVED"`. |
| `effective_from`, `effective_to` | **Chưa có trong reader hiện tại** | Reader chưa có bộ lọc range timestamp. Việc lọc hiệu lực thời gian hiện cần thực hiện ở tầng hậu kiểm hoặc nâng cấp Qdrant range filter. |
| `access_profile` (`PUBLIC_INTERNAL`, `CONFIDENTIAL`) | **Chưa có trong reader hiện tại** | Reader chỉ lọc theo scope nhân viên/phòng ban/công ty. Cần bổ sung filter `access_profile` trước khi đưa tài liệu mật vào kho. |

---

## 4. Kết luận phân định ranh giới công việc

1. **Phạm vi nhiệm vụ hiện tại:** Tạo lập bộ tài liệu nguồn chuẩn mực (DataDoc), hệ thống JSON Schema, danh mục điều khoản (`clauses.jsonl`), manifest, bộ 50+ test case và tài liệu kỹ thuật tích hợp tại `D:\QL_NS\QuanLyNhanSu\DB_Doc`.
2. **Không thực hiện trong nhiệm vụ này:** Không sửa đổi code C# đang vận hành, không chạy script chỉnh sửa CSDL Oracle, không nạp trực tiếp vào Qdrant đang chạy để đảm bảo an toàn tuyệt đối cho hệ thống production.
