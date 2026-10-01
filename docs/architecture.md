# Kiến trúc và nguồn mã tham khảo

Cập nhật nội dung: 01/10/2026.

**Phạm vi:** Tài liệu tham khảo theo mã nguồn hiện có.

## Các luồng truy cập

- Desktop → Business/DataAccess → Oracle.
- Web/Mobile → API → Business/DataAccess → Oracle.
- VectorDataSync → các thư viện dùng chung và Qdrant.

Namespace `Bu` và `DA` khác tên thư mục project. Model dùng Database-First/EDMX. Ngoài truy vấn EF, nhiều service/controller dùng SQL trực tiếp; cần đọc luồng thực tế thay vì giả định mọi thao tác đều qua lớp Business.

## Nguồn mã theo chức năng

| Nhóm | Nguồn |
| --- | --- |
| Nhân sự | [CLASS_NHANSU](../HRMS.Business/CLASS_NHANSU), [NhanVienController](../HRMS.Api/Controllers/NhanVienController.cs) |
| Chấm công | [CLASS_CHAMCONG](../HRMS.Business/CLASS_CHAMCONG), [ChamCongController](../HRMS.Api/Controllers/ChamCongController.cs) |
| Phân đoạn/công bố công | [TimeSegmentationEngine](../HRMS.Business/CLASS_CHAMCONG/TimeSegmentationEngine.cs), [AttendancePublishingService](../HRMS.Business/CLASS_CHAMCONG/AttendancePublishingService.cs) |
| Tính lương | [PayrollEngine](../HRMS.Business/CLASS_PAYROLL/PayrollEngine.cs), [BANGLUONG](../HRMS.Business/CLASS_CHAMCONG/BANGLUONG.cs) |
| Chính sách và hồ sơ | [PolicyResolver](../HRMS.Business/CLASS_PAYROLL/PolicyResolver.cs), [EmployeeProfileResolver](../HRMS.Business/CLASS_PAYROLL/EmployeeProfileResolver.cs) |
| Xác thực | [AuthController](../HRMS.Api/Controllers/AuthController.cs), [CLASS_SECURITY](../HRMS.Business/CLASS_SECURITY), [JwtAuthorizeAttribute](../HRMS.Api/Filters/JwtAuthorizeAttribute.cs) |
| Tự phục vụ | [MeController](../HRMS.Api/Controllers/MeController.cs), [API Mobile](mobile.md) |
| Phê duyệt | [ApprovalController](../HRMS.Api/Controllers/ApprovalController.cs), [approvalsApi](../HRMS.Mobile/src/api/approvalsApi.ts) |
| AI | [AI services](../HRMS.Business/Services/AI_Services), [AiChatController](../HRMS.Api/Controllers/AiChatController.cs) |
| Audit EF | [MyEntities.Audit](../HRMS.DataAccess/MyEntities.Audit.cs) |

## Database và cấu hình

`MyEntities` phục vụ dữ liệu nghiệp vụ. `AiEntities` phục vụ các view AI; quyền database cần được kiểm tra trên tài khoản thực tế. Truy vấn chỉ đọc không có nghĩa dữ liệu đã được giới hạn theo quyền người hỏi.

Xem [danh mục thuộc tính model](model-fields.md), [từ điển lương](payroll.md) và [migration runbook](database.md). Không suy khóa/ràng buộc database chỉ từ kiểu dữ liệu CLR của entity.

## Những điểm ảnh hưởng bảo trì

`App.tsx` đang chứa state và thao tác của nhiều trang. `MeController`, `PayrollEngine` và service công bố công có nhiều trách nhiệm trong cùng file. Có thể tách dần sau khi xác định test phù hợp; việc này không nằm trong đợt chỉnh tài liệu.

Quyền ở giao diện và ở server cần đối chiếu riêng. Những điểm đã nhận diện nằm trong [hiện trạng](current-status.md).
