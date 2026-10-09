# Hiá»‡n tráº¡ng dá»± Ã¡n

Cáº­p nháº­t ná»™i dung: 01/10/2026.

**Pháº¡m vi:** Káº¿t quáº£ Ä‘Ã£ quan sÃ¡t vÃ  nháº­n xÃ©t tá»« mÃ£ nguá»“n; khÃ´ng pháº£i biÃªn báº£n nghiá»‡m thu.

## CÃ¡ch Ä‘á»c káº¿t quáº£

TÃ i liá»‡u nÃ y phÃ¢n biá»‡t káº¿t quáº£ lá»‡nh Ä‘Ã£ cháº¡y, nháº­n xÃ©t tá»« mÃ£ nguá»“n vÃ  cÃ¡c ghi chÃ©p cÅ©. Nhá»¯ng káº¿t quáº£ dÆ°á»›i Ä‘Ã¢y Ä‘Æ°á»£c quan sÃ¡t trong phiÃªn rÃ  soÃ¡t ngÃ y 01/10/2026 trÆ°á»›c khi viáº¿t láº¡i tÃ i liá»‡u. KhÃ´ng cháº¡y láº¡i cÃ¡c phÃ©p tÃ­nh lÆ°Æ¡ng hoáº·c test ghi database trong Ä‘á»£t chá»‰nh tÃ i liá»‡u nÃ y.

## Kiá»ƒm tra Ä‘Ã£ quan sÃ¡t

| Kiá»ƒm tra | Káº¿t quáº£ | Giá»›i háº¡n |
| --- | --- | --- |
| Build `HRMS.Api` báº±ng MSBuild | ThÃ nh cÃ´ng | CÃ²n cáº£nh bÃ¡o quyá»n ghi cache vÃ  xung Ä‘á»™t `System.Memory`; khÃ´ng pháº£i kiá»ƒm thá»­ endpoint |
| Web `npm run test` | 7 nhÃ³m kiá»ƒm tra Ä‘áº¡t | Chá»§ yáº¿u KPI, diá»…n giáº£i kÃ½ hiá»‡u cÃ´ng, intro vÃ  ngÃ´n ngá»¯; khÃ´ng pháº£i E2E |
| Web `npm run lint` | MÃ£ thoÃ¡t 0, cÃ³ cáº£nh bÃ¡o | CÃ³ cáº£nh bÃ¡o React/hooks; mÃ£ thoÃ¡t 0 khÃ´ng cÃ³ nghÄ©a khÃ´ng cÃ²n váº¥n Ä‘á» |
| Web `npm run build` | Build Ä‘áº¡t vá»›i output riÃªng | `dist` máº·c Ä‘á»‹nh gáº·p lá»—i quyá»n truy cáº­p; output kiá»ƒm tra náº±m trong `artifacts/project-analysis-web-20261001` |
| Bundle JavaScript Web | Khoáº£ng 2,27 MB, gzip khoáº£ng 684 KB | Má»™t bundle lá»›n; cáº§n Ä‘Ã¡nh giÃ¡ táº£i trang trÃªn thiáº¿t bá»‹ thá»±c táº¿ |
| Mobile `npm run test` | 8 test Ä‘áº¡t | Kiá»ƒm tra ngÃ´n ngá»¯ vÃ  tiá»‡n Ã­ch; chÆ°a xÃ¡c minh á»©ng dá»¥ng trÃªn thiáº¿t bá»‹ |
| Mobile TypeScript `--noEmit` | ThÃ nh cÃ´ng | KhÃ´ng thay cho kiá»ƒm thá»­ chá»©c nÄƒng |
| `Build-IsolatedTests.ps1 -OnlyPure` | Lá»—i biÃªn dá»‹ch | CÃ³ tham chiáº¿u thiáº¿u; suite cÃ²n chá»©a test dÃ¹ng database |

## Äiá»ƒm cÃ²n má»Ÿ trong mÃ£ nguá»“n

| Má»©c Æ°u tiÃªn | Äiá»ƒm cáº§n xá»­ lÃ½ | Nguá»“n vÃ  cÄƒn cá»© |
| --- | --- | --- |
| P0 | GET AI chat cho phÃ©p áº©n danh *(ÄÃ£ kháº¯c phá»¥c 09/10/2026)* | [AiChatController](../HRMS.Api/Controllers/AiChatController.cs): ÄÃ£ bá» ChatGet; Ã¡p dá»¥ng `[JwtAuthorize]` toÃ n controller, GET chá»‰ tráº£ probe readiness tá»‘i thiá»ƒu khÃ´ng chá»©a dá»¯ liá»‡u nhÃ¢n viÃªn |
| P1 | API danh sÃ¡ch lÆ°Æ¡ng vÃ  khÃ³a/má»Ÿ khÃ³a ká»³ thiáº¿u kiá»ƒm tra quyá»n riÃªng | [BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs): cÃ¡c action nÃ y chá»‰ káº¿ thá»«a kiá»ƒm tra Ä‘Äƒng nháº­p; chÆ°a lá»c pháº¡m vi cÃ´ng ty á»Ÿ danh sÃ¡ch |
| P1 | Danh tÃ­nh audit cÃ³ thá»ƒ bá»‹ request khÃ¡c ghi Ä‘Ã¨ | [MyEntities.Audit](../HRMS.DataAccess/MyEntities.Audit.cs) dÃ¹ng thuá»™c tÃ­nh `static`; filter API gÃ¡n láº¡i chÃºng |
| P1 | Lá»‹ch sá»­ AI Ä‘ang Ä‘Æ°á»£c chia sáº» trong process | [ChatboxManager](../HRMS.Business/Services/AI_Services/Chat/ChatboxManager.cs) giá»¯ RAG tÄ©nh; dá»‹ch vá»¥ Ä‘Ã³ cÃ³ lá»‹ch sá»­ há»™i thoáº¡i |
| P1 | Lá»c Ä‘Ã£ chi tráº£ chÆ°a thá»‘ng nháº¥t vá»›i Ä‘Ã£ duyá»‡t | [BangLuongController](../HRMS.Api/Controllers/BangLuongController.cs) nháº­n `APPROVED` trong bá»™ lá»c `paid`; V1_20 cÃ²n ghi lÃ  draft |
| P1 | Test Ä‘á»™c láº­p chÆ°a tÃ¡ch khá»i test database | [Script](../database/synthetic200/Build-IsolatedTests.ps1), [AttendanceRegressionTests](../HRMS.Tests/AttendanceRegressionTests.cs) |
| P2 | Cáº¥u hÃ¬nh CORS/chi tiáº¿t lá»—i cáº§n Ä‘á»‘i chiáº¿u mÃ´i trÆ°á»ng | [CorsHandler](../HRMS.Api/App_Start/CorsHandler.cs), [WebApiConfig](../HRMS.Api/App_Start/WebApiConfig.cs): CORS wildcard vÃ  chi tiáº¿t lá»—i Always |
| P2 | Cáº¥u hÃ¬nh khá»Ÿi táº¡o Docker cáº§n rÃ  soÃ¡t | [Compose](../docker-compose.yml) mount cáº£ thÆ° má»¥c cÃ³ rollback vÃ o init directory; API cháº¡y riÃªng |

ÄÃ¢y lÃ  nháº­n xÃ©t tá»« mÃ£ nguá»“n, chÆ°a pháº£i káº¿t quáº£ thá»­ khai thÃ¡c trÃªn mÃ´i trÆ°á»ng triá»ƒn khai. Má»©c Æ°u tiÃªn dÃ¹ng Ä‘á»ƒ sáº¯p xáº¿p cÃ´ng viá»‡c ná»™i bá»™. CÃ¡c Ä‘iá»ƒm Ä‘Ã£ kháº¯c phá»¥c Ä‘Æ°á»£c ghi chÃº vá»›i ngÃ y xÃ¡c minh.

## ChÆ°a xÃ¡c minh trong phiÃªn rÃ  soÃ¡t

- Build/cháº¡y toÃ n bá»™ Desktop vÃ  cÃ¡c bÃ¡o cÃ¡o DevExpress.
- á»¨ng dá»¥ng Mobile trÃªn Android/iOS, Ä‘Äƒng nháº­p vÃ  cÃ¡c luá»“ng phÃª duyá»‡t trÃªn thiáº¿t bá»‹.
- Test tÃ­ch há»£p vÃ  E2E vá»›i schema Oracle riÃªng.
- PhiÃªn báº£n migration Ä‘Ã£ Ã¡p dá»¥ng trÃªn database Ä‘ang sá»­ dá»¥ng.
- ToÃ n bá»™ cÃ´ng thá»©c lÆ°Æ¡ng vÃ  giÃ¡ trá»‹ chÃ­nh sÃ¡ch theo quy Ä‘á»‹nh Ã¡p dá»¥ng cá»§a Ä‘Æ¡n vá»‹.
- XÃ¡c nháº­n thanh toÃ¡n thá»±c táº¿, cáº¥u hÃ¬nh IIS/TLS vÃ  khÃ´i phá»¥c backup trÃªn mÃ¡y triá»ƒn khai.

## TÃ¬nh tráº¡ng tÃ i liá»‡u

`docs` hiá»‡n cÃ³ 13 tÃ i liá»‡u Markdown hoáº¡t Ä‘á»™ng theo chá»§ Ä‘á» cÃ¹ng thÆ° má»¥c `archive/` lÆ°u trá»¯ tÃ i liá»‡u lá»‹ch sá»­. CÃ¡c hÆ°á»›ng dáº«n Mobile, Web, cÃ´ngâ€“lÆ°Æ¡ng vÃ  kiá»ƒm thá»­ Ä‘Ã£ Ä‘Æ°á»£c gá»™p; hai tÃ i liá»‡u má»›i vá» AI Services (`ai-services-guide.md` vÃ  `ai-rag-and-account-permissions-guide.md`) Ä‘Ã£ Ä‘Æ°á»£c bá»• sung. Prompt náº±m riÃªng trong `prompts/` vÃ  bá»‹ Git bá» qua. Snapshot/backup dá»¯ liá»‡u náº±m dÆ°á»›i `database/`; JSON Ä‘Æ°á»£c giá»¯ nguyÃªn khi di chuyá»ƒn.

Trong Ä‘á»£t dá»n tÃ i liá»‡u chá»‰ kiá»ƒm tra Ä‘Æ°á»ng dáº«n, Ä‘á»‹nh dáº¡ng, báº£n sao vÃ  quy táº¯c Git ignore. KhÃ´ng cháº¡y láº¡i build/test á»©ng dá»¥ng, SQL, seed hoáº·c tÃ­nh lÆ°Æ¡ng. Káº¿t quáº£ á»Ÿ báº£ng Ä‘áº§u lÃ  káº¿t quáº£ cá»§a phiÃªn rÃ  soÃ¡t trÆ°á»›c Ä‘Ã³.
