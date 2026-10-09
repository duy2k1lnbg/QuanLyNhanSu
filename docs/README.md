# TÃ i liá»‡u HRMS

Cáº­p nháº­t: 01/10/2026. TÃ i liá»‡u Ä‘Æ°á»£c gá»™p theo chá»§ Ä‘á» Ä‘á»ƒ giáº£m cÃ¡c báº£n trÃ¹ng vÃ  dá»… tra cá»©u.

## Báº¯t Ä‘áº§u

1. [README dá»± Ã¡n](../README.md) ([English](../README.en.md) / [æ—¥æœ¬èªž](../README.ja.md)): thÃ nh pháº§n, kiáº¿n trÃºc vÃ  cÃ¡ch báº¯t Ä‘áº§u.
2. [CÃ i Ä‘áº·t](installation.md): mÃ´i trÆ°á»ng, cáº¥u hÃ¬nh vÃ  URL cá»¥c bá»™.
3. [Kiáº¿n trÃºc](architecture.md): luá»“ng truy cáº­p vÃ  nguá»“n mÃ£ chÃ­nh.
4. [Hiá»‡n tráº¡ng](current-status.md): káº¿t quáº£ Ä‘Ã£ quan sÃ¡t, giá»›i háº¡n vÃ  viá»‡c cÃ²n má»Ÿ.

## HÆ°á»›ng dáº«n theo chá»§ Ä‘á»

| TÃ i liá»‡u | Ná»™i dung |
| --- | --- |
| [Triá»ƒn khai](deployment.md) | IIS/API, Web, cáº¥u hÃ¬nh vÃ  cÃ¡c bÆ°á»›c cáº§n kiá»ƒm tra |
| [Mobile](mobile.md) | Kiáº¿n trÃºc, API, cháº¡y/build, mapping vÃ  pháº¡m vi dá»¯ liá»‡u |
| [CÃ´ngâ€“lÆ°Æ¡ng](payroll.md) | Luá»“ng tÃ­nh, dá»¯ liá»‡u, tráº¡ng thÃ¡i/quyá»n, chÃ­nh sÃ¡ch vÃ  Ä‘á»‘i soÃ¡t giao diá»‡n |
| [Database](database.md) | Migration, kiá»ƒm tra dá»¯ liá»‡u vÃ  cÃ¡c bá»™ dá»¯ liá»‡u mÃ´ phá»ng |
| [Web](web.md) | Theme/component, KPI, kÃ½ hiá»‡u cÃ´ng vÃ  intro |
| [Kiá»ƒm thá»­](testing.md) | Nguá»“n test, ká»‹ch báº£n cáº§n cháº¡y vÃ  báº±ng chá»©ng cáº§n ghi |
| [RAG vÃ  phÃ¢n quyá»n tÃ i khoáº£n](ai-rag-and-account-permissions-guide.md) | ÄÆ°á»ng gá»i AI tháº­t, quyá»n ná»n táº£ng/nghiá»‡p vá»¥/pháº¡m vi, cÃ¡ch cáº¥p quyá»n vÃ  cháº©n Ä‘oÃ¡n UI/ngÃ´n ngá»¯ |
| [Thuá»™c tÃ­nh model](model-fields.md) | Danh má»¥c scalar C#; khÃ´ng thay metadata Oracle hoáº·c káº¿t quáº£ coverage UI |
| [Kiáº¿n trÃºc & Váº­n hÃ nh AI Services](ai-services-guide.md) | Báº£n Ä‘á»“ 53 file AI, cÃ¡c nhÃ³m chá»©c nÄƒng (Bootstrap, Config, Chat, NLP, Planning, Retrieval, Security, Indexing) vÃ  hÆ°á»›ng dáº«n báº£o trÃ¬ |

Script vÃ  snapshot dá»¯ liá»‡u náº±m trong `database/`. TÃ i liá»‡u ká»¹ thuáº­t lá»‹ch sá»­ vÃ  bÃ¡o cÃ¡o test trÆ°á»›c Ä‘Ã¢y Ä‘Æ°á»£c lÆ°u trá»¯ trong thÆ° má»¥c [archive/](archive/).

## Quy Æ°á»›c cáº­p nháº­t

- Sá»­a tÃ i liá»‡u chÃ­nh cá»§a chá»§ Ä‘á» thay vÃ¬ thÃªm bÃ¡o cÃ¡o tá»•ng káº¿t trÃ¹ng ná»™i dung.
- Káº¿t quáº£ cháº¡y cáº§n ghi ngÃ y, source, lá»‡nh, mÃ´i trÆ°á»ng vÃ  giá»›i háº¡n; tÃªn test hoáº·c migration khÃ´ng chá»©ng minh Ä‘Ã£ cháº¡y.
- PhÃ¢n biá»‡t mÃ´ táº£ theo source, káº¿t quáº£ Ä‘Ã£ quan sÃ¡t, káº¿ hoáº¡ch vÃ  dá»¯ liá»‡u lá»‹ch sá»­.
- Chá»‰ thÃªm tÃ i liá»‡u khi cÃ³ má»™t chá»§ Ä‘á» má»›i cáº§n hÆ°á»›ng dáº«n riÃªng.

Prompt Ä‘Æ°á»£c lÆ°u cá»¥c bá»™ trong `prompts/` á»Ÿ gá»‘c repository vÃ  bá»‹ Git bá» qua. CÃ¡c tÃ i liá»‡u ká»¹ thuáº­t/bÃ¡o cÃ¡o cÅ© Ä‘Æ°á»£c lÆ°u trá»¯ trong `docs/archive/` vÃ  `artifacts/docs-maintenance/` Ä‘á»ƒ báº£o toÃ n lá»‹ch sá»­ mÃ  khÃ´ng lÃ m bá»«a bá»™n tÃ i liá»‡u chÃ­nh.
