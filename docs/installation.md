# CÃ i Ä‘áº·t vÃ  cháº¡y dá»± Ã¡n cá»¥c bá»™

Cáº­p nháº­t ná»™i dung: 01/10/2026.

**Pháº¡m vi:** TÃ i liá»‡u tham kháº£o theo mÃ£ nguá»“n hiá»‡n cÃ³.

## Chuáº©n bá»‹

| ThÃ nh pháº§n | Cáº§n cho |
| --- | --- |
| Windows, Visual Studio/MSBuild vÃ  .NET Framework 4.7.2 Developer Pack | API, Business, DataAccess vÃ  Desktop |
| DevExpress 24.1 theo tham chiáº¿u project | Build/cháº¡y Desktop vÃ  bÃ¡o cÃ¡o |
| Oracle cÃ³ schema phÃ¹ há»£p | CÃ¡c chá»©c nÄƒng dá»¯ liá»‡u |
| Node.js/npm tÆ°Æ¡ng thÃ­ch package Ä‘Ã£ khÃ³a | Web/Mobile |
| Ollama, Qdrant | CÃ¡c luá»“ng AI cÃ³ sá»­ dá»¥ng dá»‹ch vá»¥ nÃ y |

Vite cÃ i trong workspace khai bÃ¡o Node `^20.19.0 || >=22.12.0`. Kiá»ƒm tra thÃªm yÃªu cáº§u package Mobile trÆ°á»›c khi chá»n runtime chung. KhÃ´ng suy tá»« viá»‡c API dÃ¹ng .NET Framework ráº±ng `dotnet run` sáº½ khá»Ÿi Ä‘á»™ng Ä‘Æ°á»£c API nÃ y.

## Database

1. XÃ¡c Ä‘á»‹nh host, port, service name vÃ  schema thá»±c táº¿. Service name Oracle khÃ´ng Ä‘Æ°á»£c suy tá»« tÃªn thÆ° má»¥c hay giÃ¡ trá»‹ máº«u `xe`.
2. Sao lÆ°u cáº¥u hÃ¬nh hiá»‡n cÃ³ trÆ°á»›c khi chá»‰nh connection string.
3. Äá»‘i chiáº¿u [migration runbook](database.md). Repository hiá»‡n cÃ³ cÃ¡c script migration tá»« V1_0 Ä‘áº¿n V1_33 trong `database/migrations/`, nhÆ°ng khÃ´ng cÃ³ xÃ¡c nháº­n trong tÃ i liá»‡u nÃ y ráº±ng táº¥t cáº£ Ä‘Ã£ Ä‘Æ°á»£c Ã¡p dá»¥ng trÃªn mÃ´i trÆ°á»ng cá»¥ thá»ƒ.
4. DÃ¹ng schema riÃªng cho test. KhÃ´ng tá»± cháº¡y seed/cutover hoáº·c toÃ n bá»™ thÆ° má»¥c migration trÃªn dá»¯ liá»‡u Ä‘ang dÃ¹ng.

`docker-compose.yml` cÃ³ service Oracle thá»­ nghiá»‡m. NÃ³ mount cáº£ thÆ° má»¥c migration vÃ o thÆ° má»¥c init; cáº§n tÃ¡ch forward/rollback/draft trÆ°á»›c khi dÃ¹ng cÆ¡ cháº¿ Ä‘Ã³. Viá»‡c táº¡o `.env` khÃ´ng tá»± cáº¥u hÃ¬nh cÃ¡c connection string cá»§a á»©ng dá»¥ng .NET.

## Backend

- Má»Ÿ `HRMS.sln` táº¡i gá»‘c repository, restore NuGet khi cáº§n.
- Cáº¥u hÃ¬nh connection string trong `HRMS.Api/Web.config` theo `MyEntities` vÃ  `AiEntities`. KhÃ´ng tá»± thay metadata EDMX báº±ng chuá»—i vÃ­ dá»¥ rÃºt gá»n.
- Desktop/console/test cÃ³ file cáº¥u hÃ¬nh riÃªng; kiá»ƒm tra chÃºng khi cháº¡y thÃ nh pháº§n tÆ°Æ¡ng á»©ng.
- Build `HRMS.Api` báº±ng Visual Studio hoáº·c MSBuild cá»§a Visual Studio.
- Cháº¡y `start_local_backend.bat`: IIS Express Ä‘Æ°á»£c gá»i vá»›i `/path:...HRMS.Api` vÃ  `/port:55463`.

ÄÆ°á»ng dáº«n cÃ i IIS Express/MSBuild khÃ¡c nhau theo mÃ¡y; sá»­a lá»‡nh cá»¥c bá»™ cho Ä‘Ãºng mÃ´i trÆ°á»ng.

## Web

```powershell
cd HRMS.Web
npm ci
npm run dev
```

Vite má»Ÿ cá»•ng `5173`, proxy `/api` tá»›i `http://localhost:55463`. Kiá»ƒm tra `HRMS.Web/.env` náº¿u request Ä‘i sang cá»•ng khÃ¡c: `VITE_API_BASE_URL` Ä‘Æ°á»£c Æ°u tiÃªn vÃ  file máº«u hiá»‡n Ä‘áº·t `http://localhost:5000/api`.

Äá»ƒ dÃ¹ng proxy máº·c Ä‘á»‹nh, bá» override URL khÃ´ng phÃ¹ há»£p hoáº·c Ä‘áº·t `VITE_API_BASE_URL=/api` trong cáº¥u hÃ¬nh cá»¥c bá»™. Khá»Ÿi Ä‘á»™ng láº¡i Vite sau khi Ä‘á»•i biáº¿n mÃ´i trÆ°á»ng.

## Mobile

Xem [build Mobile](mobile.md). `start-api-service.bat` lÃ  cÃ¡ch cháº¡y khÃ¡c: IIS Express cá»•ng `5001` cÃ¹ng `proxy.js` á»Ÿ cá»•ng `5000`. Proxy cÃ²n biáº¿n Ä‘á»•i response cho Mobile; khÃ´ng xem hai cÃ¡ch cháº¡y lÃ  tÆ°Æ¡ng Ä‘Æ°Æ¡ng náº¿u chÆ°a thá»­ endpoint.

## Kiá»ƒm tra tá»‘i thiá»ƒu

- Web táº£i Ä‘Æ°á»£c trang vÃ  request Ä‘áº¿n Ä‘Ãºng API.
- ÄÄƒng nháº­p báº±ng tÃ i khoáº£n Ä‘Æ°á»£c cáº¥p trÃªn database kiá»ƒm thá»­; khÃ´ng dÃ¹ng tÃ i khoáº£n/máº­t kháº©u viáº¿t trong tÃ i liá»‡u cÅ©.
- Kiá»ƒm tra dá»¯ liá»‡u cÃ¡ nhÃ¢n, quyá»n xem vÃ  trÆ°á»ng há»£p bá»‹ tá»« chá»‘i truy cáº­p.
- Cháº¡y cÃ¡c lá»‡nh kiá»ƒm tra frontend trong README. KhÃ´ng cháº¡y má»i test backend trÆ°á»›c khi tÃ¡ch nhÃ³m cÃ³ ghi database.

Káº¿t quáº£ build/test Ä‘Ã£ quan sÃ¡t náº±m trong [hiá»‡n tráº¡ng](current-status.md). HÆ°á»›ng dáº«n nÃ y chÆ°a Ä‘Æ°á»£c thá»­ tá»« Ä‘áº§u trÃªn má»™t mÃ¡y má»›i.
