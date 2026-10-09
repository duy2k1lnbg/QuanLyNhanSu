# Kiáº¿n trÃºc vÃ  nguá»“n mÃ£ tham kháº£o

Cáº­p nháº­t ná»™i dung: 01/10/2026.

**Pháº¡m vi:** TÃ i liá»‡u tham kháº£o theo mÃ£ nguá»“n hiá»‡n cÃ³.

## CÃ¡c luá»“ng truy cáº­p

- Desktop: nghiá»‡p vá»¥ vÃ  tÃ­nh lÆ°Æ¡ng gá»i trá»±c tiáº¿p Business/DataAccess â†’ Oracle. RiÃªng giao diá»‡n chat AI (`FrmAI_Chat`) gá»i qua `AiApiClient` â†’ `HRMS.Api` â†’ `AiExecutionService`.
- Web / Mobile: gá»i REST API (`HRMS.Api`) cÃ³ xÃ¡c thá»±c JWT vÃ  kiá»ƒm tra quyá»n ná»n táº£ng â†’ Business/DataAccess â†’ Oracle.
- VectorDataSync: Console CLI Ä‘á»c dá»¯ liá»‡u tá»« DataAccess/Oracle, gá»i Ollama táº¡o embedding vÃ  Ä‘á»“ng bá»™ sang collection `hrms_vectors_v2` trÃªn Qdrant.

Namespace `Bu` vÃ  `DA` khÃ¡c tÃªn thÆ° má»¥c project. Model dÃ¹ng Database-First/EDMX. Dá»‹ch vá»¥ AI Ä‘Æ°á»£c quy hoáº¡ch thÃ nh 14 thÆ° má»¥c chá»©c nÄƒng dÆ°á»›i `HRMS.Business/Services/AI_Services`. NgoÃ i truy váº¥n EF, nhiá»u service/controller dÃ¹ng SQL trá»±c tiáº¿p; cáº§n Ä‘á»c luá»“ng thá»±c táº¿ thay vÃ¬ giáº£ Ä‘á»‹nh má»i thao tÃ¡c Ä‘á»u qua lá»›p Business.

## Nguá»“n mÃ£ theo chá»©c nÄƒng

| NhÃ³m | Nguá»“n |
| --- | --- |
| NhÃ¢n sá»± | [CLASS_NHANSU](../HRMS.Business/CLASS_NHANSU), [NhanVienController](../HRMS.Api/Controllers/NhanVienController.cs) |
| Cháº¥m cÃ´ng | [CLASS_CHAMCONG](../HRMS.Business/CLASS_CHAMCONG), [ChamCongController](../HRMS.Api/Controllers/ChamCongController.cs) |
| PhÃ¢n Ä‘oáº¡n/cÃ´ng bá»‘ cÃ´ng | [TimeSegmentationEngine](../HRMS.Business/CLASS_CHAMCONG/TimeSegmentationEngine.cs), [AttendancePublishingService](../HRMS.Business/CLASS_CHAMCONG/AttendancePublishingService.cs) |
| TÃ­nh lÆ°Æ¡ng | [PayrollEngine](../HRMS.Business/CLASS_PAYROLL/PayrollEngine.cs), [BANGLUONG](../HRMS.Business/CLASS_CHAMCONG/BANGLUONG.cs) |
| ChÃ­nh sÃ¡ch vÃ  há»“ sÆ¡ | [PolicyResolver](../HRMS.Business/CLASS_PAYROLL/PolicyResolver.cs), [EmployeeProfileResolver](../HRMS.Business/CLASS_PAYROLL/EmployeeProfileResolver.cs) |
| XÃ¡c thá»±c | [AuthController](../HRMS.Api/Controllers/AuthController.cs), [CLASS_SECURITY](../HRMS.Business/CLASS_SECURITY), [JwtAuthorizeAttribute](../HRMS.Api/Filters/JwtAuthorizeAttribute.cs) |
| Tá»± phá»¥c vá»¥ | [MeController](../HRMS.Api/Controllers/MeController.cs), [API Mobile](mobile.md) |
| PhÃª duyá»‡t | [ApprovalController](../HRMS.Api/Controllers/ApprovalController.cs), [approvalsApi](../HRMS.Mobile/src/api/approvalsApi.ts) |
| AI | [AI services](../HRMS.Business/Services/AI_Services), [AiChatController](../HRMS.Api/Controllers/AiChatController.cs) |
| Audit EF | [MyEntities.Audit](../HRMS.DataAccess/MyEntities.Audit.cs) |

## Database vÃ  cáº¥u hÃ¬nh

`MyEntities` phá»¥c vá»¥ dá»¯ liá»‡u nghiá»‡p vá»¥. `AiEntities` phá»¥c vá»¥ cÃ¡c view AI; quyá»n database cáº§n Ä‘Æ°á»£c kiá»ƒm tra trÃªn tÃ i khoáº£n thá»±c táº¿. Truy váº¥n chá»‰ Ä‘á»c khÃ´ng cÃ³ nghÄ©a dá»¯ liá»‡u Ä‘Ã£ Ä‘Æ°á»£c giá»›i háº¡n theo quyá»n ngÆ°á»i há»i.

Xem [danh má»¥c thuá»™c tÃ­nh model](model-fields.md), [tá»« Ä‘iá»ƒn lÆ°Æ¡ng](payroll.md) vÃ  [migration runbook](database.md). KhÃ´ng suy khÃ³a/rÃ ng buá»™c database chá»‰ tá»« kiá»ƒu dá»¯ liá»‡u CLR cá»§a entity.

## Nhá»¯ng Ä‘iá»ƒm áº£nh hÆ°á»Ÿng báº£o trÃ¬

`App.tsx` Ä‘ang chá»©a state vÃ  thao tÃ¡c cá»§a nhiá»u trang. `MeController`, `PayrollEngine` vÃ  service cÃ´ng bá»‘ cÃ´ng cÃ³ nhiá»u trÃ¡ch nhiá»‡m trong cÃ¹ng file. CÃ³ thá»ƒ tÃ¡ch dáº§n sau khi xÃ¡c Ä‘á»‹nh test phÃ¹ há»£p; viá»‡c nÃ y khÃ´ng náº±m trong Ä‘á»£t chá»‰nh tÃ i liá»‡u.

Quyá»n á»Ÿ giao diá»‡n vÃ  á»Ÿ server cáº§n Ä‘á»‘i chiáº¿u riÃªng. Nhá»¯ng Ä‘iá»ƒm Ä‘Ã£ nháº­n diá»‡n náº±m trong [hiá»‡n tráº¡ng](current-status.md).
