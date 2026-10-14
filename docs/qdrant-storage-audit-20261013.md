# Kiểm tra dữ liệu và dung lượng Qdrant local

Ngày 13/10/2026, múi giờ người dùng Asia/Tokyo. Chỉ kiểm tra đọc, chưa xóa collection/point/snapshot, chưa sửa cấu hình hay khởi động/dừng dịch vụ.

## Kết luận

Qdrant local thực tế chạy qdrant.exe trên port 6333, phiên bản 1.19.0, dữ liệu ở D:\Qdrant. Docker Desktop/WSL đang dừng; đây không phải container qdrant v1.12.1 trong docker-compose.yml. File Docker VHDX là dung lượng của hệ thống Docker khác, không dùng để kết luận dung lượng Qdrant hiện tại.

- D:\Qdrant\storage: 4.266.772.606 bytes = 4,267 GB = 3,974 GiB.
- D:\Qdrant\snapshots: 2.652.283.008 bytes = 2,652 GB = 2,470 GiB.
- Cả D:\Qdrant: 7.011.687.906 bytes = 7,012 GB = 6,530 GiB; gồm storage, snapshots, binaries và storage.rar.
- Có 570 points, tất cả vectors 1024 chiều Cosine. Raw float32 vectors lý thuyết chỉ 570 × 1024 × 4 = 2.334.720 bytes (~2,23 MiB).
- Text của 570 points đọc để hash có tổng khoảng 392.288 bytes, không phải hàng GB tài liệu.
- Có 3 collection tài liệu cũ không còn được alias hiện hành trỏ tới.
- Có 2 cặp snapshot trùng bytes theo SHA-256; giữ một bản mỗi cặp có thể bỏ khoảng 1,235 GiB bản sao dư.
- Collection nhân viên có 100 points trùng cả employee ID, text và vector với bản khác. Ngoài ra 97 points schema cũ không có counterpart domain EMPLOYEE; chưa đối chiếu Oracle nên chưa kết luận là nhân viên không còn tồn tại.

## Collection đang dùng, bản cũ và giữ lại rollback

| Collection | Points | Nội dung | Dung lượng file collection | Phân loại |
|---|---:|---|---:|---|
| hrms_knowledge_bgem3_20261012T054201_5cfefe2a | 112 | POLICY / REGULATION, 27 source IDs | 980.476.270 bytes; 935,06 MiB | Active: alias hrms_knowledge_active hiện trỏ tới đây |
| hrms_vectors_v2 | 297 | EMPLOYEE, nhiều schema cùng tồn tại | 563.509.422 bytes; 537,40 MiB | Collection nhân viên được cấu hình mặc định trong mã; không xóa toàn bộ |
| hrms_knowledge_bgem3_20261012T052439_051df452 | 109 | POLICY / REGULATION, 26 source IDs | 980.476.183 bytes; 935,05 MiB | PreviousCollection trong receipt PUBLISHED; giữ rollback theo retention |
| hrms_knowledge_bgem3_20261010T195635_1a9111c8 | 26 | POLICY / REGULATION, 5 source IDs | 980.473.769 bytes; 935,05 MiB | Bản cũ không còn alias; ứng viên dọn sau chốt retention/consumers |
| hrms_knowledge_bgem3_v1 | 26 | POLICY / REGULATION, 5 source IDs | 761.836.534 bytes; 726,54 MiB | Bản cũ không còn alias, receipt SUCCESS đời đầu; ứng viên dọn |

Alias hiện tại lấy trực tiếp GET /aliases, không suy từ tên collection:
hrms_knowledge_active → hrms_knowledge_bgem3_20261012T054201_5cfefe2a.

Receipt scratch/datadoc_ingestion_receipt_v2.json có status PUBLISHED, 112 points, previousCollection = hrms_knowledge_bgem3_20261012T052439_051df452, publishedAt 12/10/2026 14:49:56 +09:00.

So sánh point IDs với active:
- Bản 109 points: 109 IDs nằm trong active, active thêm 3 IDs.
- Hai bản 26 points: 26 IDs nằm trong active, active thêm 86 IDs.
- Không có point ID riêng của 3 bản cũ ngoài active.
- ID subset không chứng minh mọi version/text hiện hành giống hệt; bản cũ vẫn có thể có giá trị rollback.

Mã QdrantService mặc định KnowledgeCollection = hrms_knowledge_active, EmployeeCollection = hrms_vectors_v2; AppSettings có thể override nếu được triển khai. Các config nguồn API/Desktop/sync được đọc không có key override collection/QdrantUrl. QueryPlanner ưu tiên SQL cho tra cứu/thống kê nhân viên và vector cho POLICY. Không có authenticated live chat trong audit này, nên kết luận usage là alias/config/source-code, không phải thống kê traffic từng user. Không khẳng định không có consumer ngoài dự án đang gọi physical collection cũ.

## Snapshot nào dư?

| Collection | Snapshot | Bytes | Trạng thái |
|---|---|---:|---|
| hrms_knowledge_bgem3_v1 | hrms_knowledge_bgem3_v1-4885704794424873-2026-10-12-07-48-05.snapshot | 762.356.224 | Cùng SHA-256 với file tên ngắn bên dưới |
| hrms_knowledge_bgem3_v1 | hrms_knowledge_bgem3_v1.snapshot | 762.356.224 | Bản sao byte-identical |
| hrms_vectors_v2 | hrms_vectors_v2-4885704794424873-2026-10-12-07-47-39.snapshot | 563.785.216 | Cùng SHA-256 với file tên ngắn bên dưới |
| hrms_vectors_v2 | hrms_vectors_v2.snapshot | 563.785.216 | Bản sao byte-identical |

SHA-256:
- Cặp knowledge: CB807EBC0BB7F1792840F9AF663403C82E2DB54C2EC85875FEF3758F2EC3644D.
- Cặp employee: 6197B8B778DC107289F6C1529F39AE26617147DBA631CED8CCCEB5F1CF4811EF.

Đây là bằng chứng bản sao dư, không chỉ suy từ kích thước. Bỏ một file mỗi cặp tiết kiệm 1.326.141.440 bytes = 1,326 GB = 1,235 GiB. Chưa kiểm tra backup job có phụ thuộc tên file hay restore được không; nếu dọn cần kiểm tra reference trước, giữ bản có tên/timestamp phù hợp retention. Các snapshot không phải dữ liệu phục vụ chat live. Không coi backup duy nhất là rác.

Snapshot collection cũ không phải backup của active 112-point collection mới. Không xóa toàn bộ snapshots và tự coi active đã được bảo vệ.

## Trùng/rác tiềm năng bên trong collection nhân viên

hrms_vectors_v2 có 297 points thuộc 197 employee IDs khác nhau:
- 100 IDs có 2 points mỗi ID.
- 97 IDs có 1 point mỗi ID.
- 100 points có domain = EMPLOYEE.
- 197 points thiếu domain.
- Với 100 nhóm 2 points, text hash và vector hash float32 giống hệt.
- Trong 197 points thiếu domain, 100 points có counterpart domain EMPLOYEE, còn 97 points không có counterpart đó.
- Cả 297 points đều chưa có visibility_profile; không lấy field đó làm điều kiện xóa ngay vì contract hiện đang pha trộn schema.

Kết luận:
- 100 bản sao thiếu domain là ứng viên deduplicate đã xác minh nội dung/vector trùng, nhưng trước xóa cần chọn canonical point theo schema/ID/current scope metadata và bảo đảm writer không tạo lại.
- 97 bản còn lại phải đối chiếu nguồn Oracle hiện hành và trạng thái nghỉ việc/xóa/scope, không xóa vì không có domain.
- Không kết luận tất cả 197 points thiếu domain là rác.
- Xóa 100 points sẽ không tự giải phóng ngay hàng trăm MiB vì collection đang có file chunk/index cấp phát và optimizer threshold. Số points và dung lượng file không tỷ lệ tuyến tính ở quy mô này.

Đọc code hiện tại SyncEmployeeDataAsync vẫn dùng MyEntities/raw employee tables, không đúng thiết kế toàn AI qua reader. Đây là phát hiện riêng từ code, chưa sửa; cần xử lý nguồn đồng bộ/projection trong đợt repair để tránh duy trì dữ liệu index sai nguồn/quyền. Không dùng audit này làm bằng chứng tất cả security gates đã đạt.

## Vì sao ít points mà gần 4 GiB?

Active collection 112 points chiếm ~935 MiB, thành phần đo từ file:
- payload indexes: 636.404.968 bytes (~606,92 MiB), 816 files.
- payload storage: 138.544.328 bytes (~132,13 MiB).
- vector storage: 138.412.456 bytes (~132,00 MiB).
- WAL: 67.108.881 bytes (~64,00 MiB).
- metadata khác: 5.637 bytes.

Mỗi collection có 4 segments. Các file vector/payload chunk ban đầu lớn (nhiều file 32 MiB); WAL gồm các segment 32 MiB. Nhiều payload index được tạo trên mỗi segment; active có 23 indexed fields. Khi giữ các collection cũ, overhead này nhân lên dù chỉ có 26–112 points.

Đã đọc GetCompressedFileSizeW/attributes: files hiện không SparseFile; reported compressed sizes bằng logical lengths, không có bằng chứng vùng sparse làm số hiển thị lớn nhưng không chiếm disk. Chưa tính riêng cluster allocation/directory overhead của filesystem; các số trên là tổng dung lượng file và reported compressed sizes.

Không có HNSW indexed vectors (indexed_vectors_count = 0); với collection nhỏ điều này có thể bình thường, không phải tự chứng minh Qdrant hỏng. Lớn nhất ở đây là payload structures/preallocated storage/old collections/snapshots, không phải hàng triệu vector.

Nguồn kỹ thuật:
- [Qdrant storage](https://qdrant.tech/documentation/manage-data/storage/): WAL và segments là thành phần dữ liệu, không xóa thủ công.
- [Qdrant capacity planning](https://qdrant.tech/documentation/operations/capacity-planning/): payload indexes thêm storage/RAM; chỉ index field thực sự dùng filter.
- [Qdrant config](https://github.com/qdrant/qdrant/blob/master/config/config.yaml): WAL segment config, auto segment count.
- [Qdrant small collection indexing](https://qdrant.tech/documentation/production-operations/bulk-data-import/): ít dữ liệu có thể chưa đạt HNSW indexing threshold.
- [Qdrant snapshots](https://qdrant.tech/documentation/operations/snapshots/): snapshot files và API quản lý độc lập.

## Phương án dọn theo mức độ, chưa thực hiện

1. Giữ active 112-point collection, hrms_vectors_v2 và previous 109-point collection phục vụ rollback.
2. Chốt retention/consumers, loại hai bản tài liệu cũ: hrms_knowledge_bgem3_v1 và hrms_knowledge_bgem3_20261010T195635_1a9111c8. Collection file tổng ~1,623 GiB.
3. Kiểm tra snapshot references và bỏ một snapshot byte-identical mỗi cặp; giảm thêm ~1,235 GiB.
4. Tổng ứng viên dọn ở mức giữ previous + một snapshot mỗi cặp: khoảng 3.068.451.743 bytes = 3,068 GB = 2,858 GiB; còn khoảng 3,943 GB = 3,673 GiB cho toàn thư mục theo baseline này. Đây là estimate riêng cho targets, cần đo lại sau API cleanup; không bảo đảm metadata/free-space tức thời.
5. Dọn 100 points trùng sau canonical/source/writer verification; 97 legacy IDs phải đối chiếu Oracle trước.
6. Nếu cần giảm sâu: evaluate segment count thấp hơn và chỉ giữ payload indexes thực sự dùng ở query/ACL/verify. Giữ security indexes, không delete index chỉ vì lớn; thử trên versioned staging collection, đo retrieval/ACL/latency, rồi cutover. Không đổi vector dimension/model chỉ để giảm MB khi raw vectors đã nhỏ.
7. Source docs/chunks/manifests/receipt không phải rác; storage.rar 7,74 MiB ngày 13/09 là backup cũ chưa kiểm tra, không ưu tiên xóa để xử lý hàng GiB.

Thực hiện cleanup sau khi được yêu cầu: recheck alias/point counts/targets, dùng Qdrant API đúng collection/snapshot/point IDs, xác minh sau thao tác. Không xóa WAL/segments/chunk_0.mmap/payload index directories hoặc cả storage trong Explorer khi Qdrant đang chạy. Không docker prune/xóa Docker VHDX để chữa Qdrant Windows service.

## Phạm vi và bằng chứng

Đã dùng GET root/collections/aliases/collection details/snapshots/metrics; POST points/scroll là đọc, payload hạn chế và hashing text/vector trong process, không in nội dung nhân sự/tài liệu. Không tạo snapshot hay POST write/delete. Đã đo D:\Qdrant và checksum 4 snapshot; đọc config source và ingestion receipts.

Metadata audit tạm trong AppData Local Temp, không đưa raw text/vectors/employee IDs vào báo cáo:
- hrms-qdrant-audit-20261013-summary.json
- hrms-qdrant-audit-20261013-file-sizes.json
- hrms-qdrant-audit-20261013-point-fingerprints.json

Chưa đăng nhập chat thật để đo usage, chưa đọc Oracle hiện hành để kết luận 97 legacy IDs, chưa test restore snapshot, chưa kiểm tra consumers/backup jobs ngoài repo. Vì vậy phân biệt “đang được alias/code dùng”, “bản cũ có thể dọn”, “bản rollback”, “byte-identical duplicate” và “chưa đủ bằng chứng rác”.

