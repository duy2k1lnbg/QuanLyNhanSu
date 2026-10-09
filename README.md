# Quáº£n lÃ½ nhÃ¢n sá»± (HRMS)

[Tiáº¿ng Viá»‡t](README.md) | [English](README.en.md) | [æ—¥æœ¬èªž](README.ja.md)

Há»‡ thá»‘ng quáº£n lÃ½ nhÃ¢n sá»± Ä‘a ná»n táº£ng gá»“m Desktop (Windows WinForms), Web (React) vÃ  Mobile (React Native Expo), sá»­ dá»¥ng cÆ¡ sá»Ÿ dá»¯ liá»‡u Oracle Database lÃ m nguá»“n lÆ°u trá»¯ nghiá»‡p vá»¥ táº­p trung, káº¿t há»£p dá»‹ch vá»¥ tÃ¬m kiáº¿m vector Qdrant vÃ  mÃ´ hÃ¬nh ngÃ´n ngá»¯ Ollama phá»¥c vá»¥ trá»£ lÃ½ tra cá»©u ná»™i bá»™.

TÃ i liá»‡u Ä‘Æ°á»£c cáº­p nháº­t ngÃ y **09/10/2026** dá»±a trÃªn mÃ£ nguá»“n, cáº¥u hÃ¬nh vÃ  káº¿t quáº£ kiá»ƒm tra thá»±c táº¿ trong repository.

---

## Bá»‘n má»©c tráº¡ng thÃ¡i Ä‘Ã¡nh giÃ¡ trong tÃ i liá»‡u

Äá»ƒ Ä‘áº£m báº£o tÃ­nh trung thá»±c vÃ  khÃ¡ch quan, cÃ¡c thÃ´ng tin trong tÃ i liá»‡u nÃ y tuÃ¢n thá»§ 4 má»©c tráº¡ng thÃ¡i:
1. **CÃ³ trong mÃ£ nguá»“n:** ThÃ nh pháº§n, mÃ n hÃ¬nh, controller, service hoáº·c báº£ng dá»¯ liá»‡u Ä‘Ã£ tá»“n táº¡i trong mÃ£ nguá»“n; chÆ°a Ä‘á»“ng nghÄ©a Ä‘Ã£ Ä‘Æ°á»£c kiá»ƒm thá»­ toÃ n diá»‡n trÃªn mÃ´i trÆ°á»ng váº­n hÃ nh thá»±c táº¿.
2. **ÄÃ£ kiá»ƒm tra:** ÄÃ£ cháº¡y thá»­ nghiá»‡m vá»›i ngÃ y ghi nháº­n, pháº¡m vi lá»‡nh vÃ  káº¿t quáº£ cá»¥ thá»ƒ; chá»‰ káº¿t luáº­n trong giá»›i háº¡n bÃ i kiá»ƒm tra Ä‘Ã³.
3. **ChÆ°a xÃ¡c minh / cÃ²n háº¡n cháº¿:** ChÆ°a cÃ³ mÃ´i trÆ°á»ng thá»­ nghiá»‡m E2E Ä‘áº§y Ä‘á»§, schema mÃ´i trÆ°á»ng Ä‘Ã­ch chÆ°a Ä‘á»“ng bá»™ hoáº·c cÃ²n tá»“n táº¡i lá»—i Ä‘Ã£ Ä‘Æ°á»£c ghi nháº­n trong cÃ¡c Ä‘á»£t rÃ  soÃ¡t.
4. **HÆ°á»›ng phÃ¡t triá»ƒn:** TÃ­nh nÄƒng, kiáº¿n trÃºc hoáº·c cáº£i tiáº¿n Ä‘Æ°á»£c hoáº¡ch Ä‘á»‹nh trong tÆ°Æ¡ng lai; chÆ°a triá»ƒn khai trong mÃ£ nguá»“n hiá»‡n hÃ nh.

---

## Má»¥c lá»¥c

1. [Giá»›i thiá»‡u vÃ  má»¥c Ä‘Ã­ch](#1-giá»›i-thiá»‡u-vÃ -má»¥c-Ä‘Ã­ch)
2. [Äá»‘i tÆ°á»£ng sá»­ dá»¥ng](#2-Ä‘á»‘i-tÆ°á»£ng-sá»­-dá»¥ng)
3. [Vai trÃ² cá»§a Desktop, Web vÃ  Mobile](#3-vai-trÃ²-cá»§a-desktop-web-vÃ -mobile)
4. [Chá»©c nÄƒng theo nghiá»‡p vá»¥](#4-chá»©c-nÄƒng-theo-nghiá»‡p-vá»¥)
5. [Kiáº¿n trÃºc tá»•ng thá»ƒ](#5-kiáº¿n-trÃºc-tá»•ng-thá»ƒ)
6. [CÃ´ng nghá»‡ vÃ  vai trÃ²](#6-cÃ´ng-nghá»‡-vÃ -vai-trÃ²)
7. [Cáº¥u trÃºc repository](#7-cáº¥u-trÃºc-repository)
8. [CÃ¡c luá»“ng nghiá»‡p vá»¥](#8-cÃ¡c-luá»“ng-nghiá»‡p-vá»¥)
9. [ÄÄƒng nháº­p vÃ  phÃ¢n quyá»n](#9-Ä‘Äƒng-nháº­p-vÃ -phÃ¢n-quyá»n)
10. [AI, SQL vÃ  RAG](#10-ai-sql-vÃ -rag)
11. [Dá»¯ liá»‡u vÃ  Ä‘á»“ng bá»™ vector](#11-dá»¯-liá»‡u-vÃ -Ä‘á»“ng-bá»™-vector)
12. [Chuáº©n bá»‹ vÃ  cháº¡y cá»¥c bá»™](#12-chuáº©n-bá»‹-vÃ -cháº¡y-cá»¥c-bá»™)
13. [Kiá»ƒm tra vÃ  káº¿t quáº£ Ä‘Ã£ ghi nháº­n](#13-kiá»ƒm-tra-vÃ -káº¿t-quáº£-Ä‘Ã£-ghi-nháº­n)
14. [Triá»ƒn khai vÃ  váº­n hÃ nh](#14-triá»ƒn-khai-vÃ -váº­n-hÃ nh)
15. [KhÃ³ khÄƒn vÃ  giá»›i háº¡n hiá»‡n táº¡i](#15-khÃ³-khÄƒn-vÃ -giá»›i-háº¡n-hiá»‡n-táº¡i)
16. [HÆ°á»›ng phÃ¡t triá»ƒn](#16-hÆ°á»›ng-phÃ¡t-triá»ƒn)
17. [TÃ i liá»‡u liÃªn quan](#17-tÃ i-liá»‡u-liÃªn-quan)
18. [Báº£o trÃ¬ tÃ i liá»‡u vÃ  giáº¥y phÃ©p](#18-báº£o-trÃ¬-tÃ i-liá»‡u-vÃ -giáº¥y-phÃ©p)

---

## 1. Giá»›i thiá»‡u vÃ  má»¥c Ä‘Ã­ch

Dá»± Ã¡n quáº£n lÃ½ nhÃ¢n sá»± gá»“m Desktop, Web vÃ  Mobile, sá»­ dá»¥ng Oracle cho dá»¯ liá»‡u nghiá»‡p vá»¥. Desktop khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng; Web vÃ  Mobile cung cáº¥p cÃ¡c chá»©c nÄƒng quáº£n lÃ½ hoáº·c tra cá»©u theo quyá»n. Pháº§n AI há»— trá»£ tra cá»©u dá»¯ liá»‡u vÃ  tÃ i liá»‡u, hiá»‡n váº«n cÃ³ cÃ¡c luá»“ng cáº§n hoÃ n thiá»‡n.

Há»‡ thá»‘ng Ä‘Æ°á»£c xÃ¢y dá»±ng nháº±m giáº£i quyáº¿t cÃ¡c bÃ i toÃ¡n váº­n hÃ nh nhÃ¢n sá»± cá»‘t lÃµi:
- **Táº­p trung hÃ³a há»“ sÆ¡ vÃ  vÃ²ng Ä‘á»i nhÃ¢n sá»±:** Quáº£n lÃ½ thÃ´ng tin á»©ng viÃªn, tuyá»ƒn dá»¥ng, há»“ sÆ¡ nhÃ¢n viÃªn, há»£p Ä‘á»“ng lao Ä‘á»™ng, khen thÆ°á»Ÿng, ká»· luáº­t, nÃ¢ng lÆ°Æ¡ng, Ä‘iá»u chuyá»ƒn vÃ  thÃ´i viá»‡c.
- **Tá»± Ä‘á»™ng hÃ³a cháº¥m cÃ´ng vÃ  phÃ¢n Ä‘oáº¡n giá» cÃ´ng:** Thu tháº­p dá»¯ liá»‡u vÃ o/ra, tÃ­nh toÃ¡n cÃ´ng chuáº©n, cÃ´ng thá»±c táº¿, lÃ m Ä‘Ãªm, nghá»‰ lá»…, phÃ©p nÄƒm vÃ  phÃ¡t hiá»‡n báº¥t thÆ°á»ng giá» lÃ m.
- **TÃ­nh toÃ¡n tiá»n lÆ°Æ¡ng theo chÃ­nh sÃ¡ch:** Thá»±c thi quy táº¯c tÃ­nh lÆ°Æ¡ng dá»±a trÃªn cÃ´ng thá»±c táº¿, phá»¥ cáº¥p, tÄƒng ca, cÃ¡c khoáº£n trÃ­ch ná»™p báº¯t buá»™c (BHXH, BHYT, BHTN, Kinh phÃ­ CÃ´ng Ä‘oÃ n), thuáº¿ thu nháº­p cÃ¡ nhÃ¢n (TNCN), giáº£m trá»« gia cáº£nh vÃ  táº¡m á»©ng lÆ°Æ¡ng.
- **Tá»± phá»¥c vá»¥ nhÃ¢n viÃªn (ESS) vÃ  phÃª duyá»‡t Ä‘a cáº¥p:** Cho phÃ©p nhÃ¢n viÃªn gá»­i Ä‘Æ¡n xin nghá»‰ phÃ©p, tÄƒng ca, bá»• sung cÃ´ng vÃ  á»©ng lÆ°Æ¡ng trá»±c tuyáº¿n; há»— trá»£ cáº¥p quáº£n lÃ½ xÃ©t duyá»‡t trÃªn Web vÃ  Mobile.
- **Há»— trá»£ há»i Ä‘Ã¡p ná»™i bá»™ báº±ng AI:** TÃ­ch há»£p mÃ´ hÃ¬nh ngÃ´n ngá»¯ cá»¥c bá»™ vÃ  tÃ¬m kiáº¿m ngá»¯ nghÄ©a theo pháº¡m vi báº£o máº­t dá»¯ liá»‡u, cho phÃ©p tra cá»©u quy cháº¿ vÃ  thÃ´ng tin nhÃ¢n sá»± trong giá»›i háº¡n quyá»n háº¡n Ä‘Æ°á»£c cáº¥p.

---

## 2. Äá»‘i tÆ°á»£ng sá»­ dá»¥ng

1. **ChuyÃªn viÃªn NhÃ¢n sá»± (HR Specialist):**
   - Quáº£n lÃ½ cÆ¡ cáº¥u tá»• chá»©c (CÃ´ng ty, PhÃ²ng ban, Bá»™ pháº­n, Chá»©c vá»¥).
   - Tiáº¿p nháº­n há»“ sÆ¡, láº­p vÃ  kÃ½ há»£p Ä‘á»“ng lao Ä‘á»™ng, theo dÃµi quÃ¡ trÃ¬nh cÃ´ng tÃ¡c, biáº¿n Ä‘á»™ng nhÃ¢n sá»±, khen thÆ°á»Ÿng, ká»· luáº­t vÃ  quyáº¿t Ä‘á»‹nh thÃ´i viá»‡c.
   - Thao tÃ¡c chá»§ yáº¿u trÃªn giao diá»‡n Desktop vÃ  cá»•ng Web quáº£n trá»‹.
2. **ChuyÃªn viÃªn Cháº¥m cÃ´ng â€“ Tiá»n lÆ°Æ¡ng (Payroll Specialist):**
   - Thiáº¿t láº­p ca lÃ m viá»‡c, loáº¡i cÃ´ng, lá»‹ch nghá»‰ lá»… vÃ  quy táº¯c cháº¥m cÃ´ng.
   - Kiá»ƒm tra báº£ng cháº¥m cÃ´ng chi tiáº¿t, xá»­ lÃ½ báº¥t thÆ°á»ng vÃ  chá»‘t/cÃ´ng bá»‘ ká»³ cÃ´ng.
   - Khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng trÃªn Desktop, kiá»ƒm tra cÃ¡c khoáº£n trÃ­ch ná»™p báº£o hiá»ƒm, thuáº¿, phá»¥ cáº¥p, phÃ¡t sinh, Ä‘á»‘i soÃ¡t báº£ng lÆ°Æ¡ng vÃ  quáº£n lÃ½ tráº¡ng thÃ¡i cÃ´ng bá»‘ chi tráº£.
3. **Cáº¥p Quáº£n lÃ½ / TrÆ°á»Ÿng bá»™ pháº­n (Department Head / Manager):**
   - GiÃ¡m sÃ¡t tÃ¬nh hÃ¬nh cháº¥m cÃ´ng, nhÃ¢n sá»± trong bá»™ pháº­n phá»¥ trÃ¡ch.
   - Tiáº¿p nháº­n vÃ  phÃª duyá»‡t hoáº·c tá»« chá»‘i cÃ¡c yÃªu cáº§u nghá»‰ phÃ©p, lÃ m thÃªm giá», á»©ng lÆ°Æ¡ng vÃ  giáº£i trÃ¬nh cháº¥m cÃ´ng cá»§a cáº¥p dÆ°á»›i qua Web hoáº·c Mobile.
4. **NhÃ¢n viÃªn (Employee):**
   - Tra cá»©u há»“ sÆ¡ cÃ¡ nhÃ¢n, há»£p Ä‘á»“ng hiá»‡n hÃ nh, dá»¯ liá»‡u cháº¥m cÃ´ng hÃ ng ngÃ y vÃ  phiáº¿u lÆ°Æ¡ng cÃ¡ nhÃ¢n cá»§a cÃ¡c ká»³ Ä‘Ã£ Ä‘Æ°á»£c cÃ´ng bá»‘.
   - Gá»­i yÃªu cáº§u xin nghá»‰ phÃ©p, Ä‘Äƒng kÃ½ tÄƒng ca, xin á»©ng lÆ°Æ¡ng vÃ  theo dÃµi tráº¡ng thÃ¡i phÃª duyá»‡t qua á»©ng dá»¥ng Web hoáº·c Mobile.
5. **Quáº£n trá»‹ viÃªn Há»‡ thá»‘ng (System Administrator):**
   - Quáº£n trá»‹ tÃ i khoáº£n, nhÃ³m ngÆ°á»i dÃ¹ng, ma tráº­n phÃ¢n quyá»n 3 ná»n táº£ng (Desktop, Web, Mobile) theo 5 hÃ nh vi thao tÃ¡c (Xem, ThÃªm, Sá»­a, XÃ³a, In).
   - Quáº£n trá»‹ pháº¡m vi truy cáº­p dá»¯ liá»‡u AI (Self, Department, Company, All), giÃ¡m sÃ¡t phiÃªn Ä‘Äƒng nháº­p vÃ  cáº¥u hÃ¬nh tham sá»‘ há»‡ thá»‘ng.

---

## 3. Vai trÃ² cá»§a Desktop, Web vÃ  Mobile

Ba ná»n táº£ng Ä‘Æ°á»£c phÃ¢n Ä‘á»‹nh trÃ¡ch nhiá»‡m rÃµ rÃ ng nháº±m Ä‘Ã¡p á»©ng cÃ¡c mÃ´i trÆ°á»ng lÃ m viá»‡c Ä‘áº·c thÃ¹:

```
+-----------------------------------------------------------------------------------+
|                               PHÃ‚N Äá»ŠNH VAI TRÃ’ Ná»€N Táº¢NG                          |
+-----------------------------------------------------------------------------------+
|  HRMS.Desktop (Windows WinForms)                                                  |
|  - Váº­n hÃ nh nghiá»‡p vá»¥ chuyÃªn sÃ¢u cá»§a phÃ²ng NhÃ¢n sá»± & Káº¿ toÃ¡n lÆ°Æ¡ng               |
|  - Nháº­p liá»‡u há»“ sÆ¡, há»£p Ä‘á»“ng, danh má»¥c tá»• chá»©c, biá»ƒu máº«u in áº¥n DevExpress         |
|  - NÆ I DUY NHáº¤T KHá»žI PHÃT TÃNH TOÃN Ká»² LÆ¯Æ NG (FrmBangLuong -> PayrollEngine)       |
|  - Káº¿t ná»‘i trá»±c tiáº¿p C# Business Library & DataAccess tá»›i Oracle                  |
|  - Chat AI qua giao diá»‡n tÃ­ch há»£p gá»i REST API (AiApiClient)                      |
+-----------------------------------------------------------------------------------+
|  HRMS.Web (React 19 + TypeScript + Ant Design)                                    |
|  - Cá»•ng thÃ´ng tin quáº£n trá»‹ vÃ  váº­n hÃ nh dÃ nh cho quáº£n lÃ½ vÃ  nhÃ¢n sá»±                |
|  - Báº£ng Ä‘iá»u khiá»ƒn (Dashboard KPI), danh sÃ¡ch nhÃ¢n viÃªn, há»£p Ä‘á»“ng, báº£ng cÃ´ng      |
|  - Trung tÃ¢m phÃª duyá»‡t Ä‘Æ¡n tá»« (ApprovalCenterPage)                                |
|  - PhÃ¢n quyá»n kÃªnh & thao tÃ¡c (PhanQuyenModal), quáº£n trá»‹ pháº¡m vi AI               |
|  - Xem báº£ng lÆ°Æ¡ng Ä‘Ã£ cÃ´ng bá»‘ (chá»‰ Ä‘á»c); KHÃ”NG khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng                |
|  - Trá»£ lÃ½ AI Copilot Drawer (há»i Ä‘Ã¡p theo quyá»n Ä‘Äƒng nháº­p)                        |
|  - 100% giao tiáº¿p qua REST API (HRMS.Api) cÃ³ xÃ¡c thá»±c JWT                         |
+-----------------------------------------------------------------------------------+
|  HRMS.Mobile (React Native + Expo)                                                |
|  - á»¨ng dá»¥ng tá»± phá»¥c vá»¥ nhÃ¢n viÃªn (ESS) trÃªn thiáº¿t bá»‹ di Ä‘á»™ng cÃ¡ nhÃ¢n              |
|  - Xem há»“ sÆ¡ cÃ¡ nhÃ¢n, báº£ng cÃ´ng thÃ¡ng, phiáº¿u lÆ°Æ¡ng cÃ¡ nhÃ¢n Ä‘Ã£ duyá»‡t (api/me)      |
|  - Gá»­i Ä‘Æ¡n xin nghá»‰ phÃ©p, tÄƒng ca, á»©ng lÆ°Æ¡ng                                      |
|  - Quáº£n lÃ½ duyá»‡t nhanh Ä‘Æ¡n tá»« cáº¥p dÆ°á»›i (ManagerApprovalsScreen)                   |
|  - KHÃ”NG cÃ³ tÃ­nh nÄƒng khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng hoáº·c quáº£n trá»‹ há»‡ thá»‘ng                 |
|  - 100% giao tiáº¿p qua REST API (HRMS.Api) cÃ³ xÃ¡c thá»±c JWT                         |
+-----------------------------------------------------------------------------------+
```

---

## 4. Chá»©c nÄƒng theo nghiá»‡p vá»¥

### 4.1. Ma tráº­n chá»©c nÄƒng trÃªn cÃ¡c ná»n táº£ng

| PhÃ¢n há»‡ nghiá»‡p vá»¥ | Desktop | Web | Mobile | Quyá»n háº¡n yÃªu cáº§u | Tráº¡ng thÃ¡i ká»¹ thuáº­t |
| :--- | :---: | :---: | :---: | :---: | :--- |
| **Há»“ sÆ¡ nhÃ¢n sá»±** | Quáº£n lÃ½ toÃ n diá»‡n | Quáº£n lÃ½ & tra cá»©u | Xem há»“ sÆ¡ cÃ¡ nhÃ¢n | `F_DM_NHANVIEN` | CÃ³ trong mÃ£ nguá»“n, Web/Mobile qua API |
| **Há»£p Ä‘á»“ng lao Ä‘á»™ng** | Soáº¡n tháº£o, in áº¥n, kÃ½ | Quáº£n lÃ½ danh sÃ¡ch | Xem há»£p Ä‘á»“ng cá»§a mÃ¬nh | `F_NV_HOPDONG` | CÃ³ trong mÃ£ nguá»“n, tÃ­nh lÆ°Æ¡ng Ä‘á»c trá»±c tiáº¿p |
| **Danh má»¥c tá»• chá»©c/ca** | Thiáº¿t láº­p toÃ n bá»™ | Tra cá»©u danh má»¥c | KhÃ´ng há»— trá»£ | `F_DM_*`, `F_CC_*` | CÃ³ trong mÃ£ nguá»“n trÃªn Desktop & API |
| **Quáº£n lÃ½ cháº¥m cÃ´ng** | Import mÃ¡y cháº¥m cÃ´ng, chá»‘t | Xem báº£ng cÃ´ng, KPI | Xem cÃ´ng cÃ¡ nhÃ¢n | `F_CC_BANGCONG` | CÃ³ trong mÃ£ nguá»“n; engine phÃ¢n Ä‘oáº¡n giá» |
| **Khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng** | **Khá»Ÿi phÃ¡t tÃ­nh toÃ¡n** | KhÃ´ng há»— trá»£ | KhÃ´ng há»— trá»£ | `F_CC_BANGLUONG` (Desktop) | ÄÃ£ kiá»ƒm tra qua NUnit; WinForms gá»i Engine |
| **Tra cá»©u báº£ng lÆ°Æ¡ng** | Xem chi tiáº¿t, in phiáº¿u | Tra cá»©u theo quyá»n | Xem phiáº¿u lÆ°Æ¡ng cÃ¡ nhÃ¢n | `F_CC_BANGLUONG`, `/me` | CÃ³ trong mÃ£ nguá»“n; Web/Mobile Ä‘á»c qua API |
| **PhÃ¡t sinh & Ä‘iá»u chá»‰nh lÆ°Æ¡ng** | Nháº­p phÃ¡t sinh, phá»¥ cáº¥p | Tra cá»©u theo ká»³ | KhÃ´ng há»— trá»£ | `F_CC_PHUCAP`, `F_CC_UNGLUONG` | CÃ³ trong mÃ£ nguá»“n trÃªn Desktop |
| **Khen thÆ°á»Ÿng, ká»· luáº­t** | Ban hÃ nh quyáº¿t Ä‘á»‹nh | Xem danh sÃ¡ch | Xem quyáº¿t Ä‘á»‹nh cÃ¡ nhÃ¢n | `F_NV_KHENTHUONG`, `KYLUAT` | CÃ³ trong mÃ£ nguá»“n |
| **Tá»± phá»¥c vá»¥ nhÃ¢n viÃªn (ESS)** | Giá»›i háº¡n | Ná»™p Ä‘Æ¡n, xem káº¿t quáº£ | Ná»™p Ä‘Æ¡n, xem káº¿t quáº£ | Token ngÆ°á»i dÃ¹ng (`/api/me`) | CÃ³ trong mÃ£ nguá»“n; Web & Mobile hoáº¡t Ä‘á»™ng |
| **PhÃª duyá»‡t Ä‘Æ¡n tá»«** | KhÃ´ng Æ°u tiÃªn | Trung tÃ¢m phÃª duyá»‡t | PhÃª duyá»‡t nhanh | Quyá»n quáº£n lÃ½ duyá»‡t Ä‘Æ¡n | CÃ³ trong mÃ£ nguá»“n trÃªn API, Web, Mobile |
| **BÃ¡o cÃ¡o & Thá»‘ng kÃª** | In áº¥n DevExpress | Biá»ƒu Ä‘á»“ Dashboard | Dashboard thu gá»n | `F_BC_BAOCAO`, `F_DB_*` | CÃ³ trong mÃ£ nguá»“n; Desktop in qua Reports |
| **PhÃ¢n quyá»n & TÃ i khoáº£n** | Quáº£n lÃ½ tÃ i khoáº£n | Ma tráº­n quyá»n 3 kÃªnh | KhÃ´ng há»— trá»£ | `F_SYSTEM_USER`, `PQ_CHUCNANG` | ÄÃ£ kiá»ƒm tra 32/32 tests; batch API atomic |
| **Trá»£ lÃ½ AI & RAG** | Chat Form (qua API) | Chat Drawer (qua API) | ChÆ°a tÃ­ch há»£p UI | `F_SYSTEM_AI` + Scope Grant | ÄÃ£ kiá»ƒm tra 234 tests; cutover v2 |

### 4.2. Chi tiáº¿t cÃ¡c nhÃ³m nghiá»‡p vá»¥ chÃ­nh

#### A. Há»“ sÆ¡ nhÃ¢n sá»± vÃ  Há»£p Ä‘á»“ng lao Ä‘á»™ng
- **Má»¥c Ä‘Ã­ch:** LÆ°u trá»¯ lÃ½ lá»‹ch nhÃ¢n viÃªn, há»“ sÆ¡ cÄƒn cÆ°á»›c, trÃ¬nh Ä‘á»™, quÃ¡ trÃ¬nh cÃ´ng tÃ¡c; quáº£n lÃ½ thá»i háº¡n há»£p Ä‘á»“ng, má»©c lÆ°Æ¡ng cÆ¡ báº£n, há»‡ sá»‘ lÆ°Æ¡ng vÃ  cÃ¡c thá»a thuáº­n phá»¥ cáº¥p.
- **Dá»¯ liá»‡u Ä‘áº§u vÃ o:** ThÃ´ng tin cÃ¡ nhÃ¢n, phÃ²ng ban, bá»™ pháº­n, chá»©c vá»¥, loáº¡i há»£p Ä‘á»“ng, lÆ°Æ¡ng Ä‘Ã³ng báº£o hiá»ƒm, ngÃ y hiá»‡u lá»±c vÃ  ngÃ y háº¿t háº¡n.
- **Thao tÃ¡c chÃ­nh:** ThÃªm má»›i há»“ sÆ¡, quÃ©t cÄƒn cÆ°á»›c, kÃ½ há»£p Ä‘á»“ng má»›i, gia háº¡n há»£p Ä‘á»“ng, ban hÃ nh quyáº¿t Ä‘á»‹nh Ä‘iá»u chuyá»ƒn, nÃ¢ng lÆ°Æ¡ng, khen thÆ°á»Ÿng, ká»· luáº­t, cháº¥m dá»©t há»£p Ä‘á»“ng.
- **Káº¿t quáº£:** Báº£n ghi nhÃ¢n viÃªn trong `TB_NHANVIEN`, há»“ sÆ¡ há»£p Ä‘á»“ng trong `TB_HOPDONG`, dá»¯ liá»‡u cung cáº¥p trá»±c tiáº¿p cho `EmployeeProfileResolver` phá»¥c vá»¥ tÃ­nh lÆ°Æ¡ng.

#### B. Cháº¥m cÃ´ng vÃ  PhÃ¢n Ä‘oáº¡n giá»
- **Má»¥c Ä‘Ã­ch:** Quáº£n lÃ½ ká»³ cÃ´ng hÃ ng thÃ¡ng, ghi nháº­n thá»i gian lÃ m viá»‡c thá»±c táº¿, chia tÃ¡ch chÃ­nh xÃ¡c cÃ´ng ngÃ y, cÃ´ng Ä‘Ãªm, tÄƒng ca ngÃ y thÆ°á»ng, tÄƒng ca chá»§ nháº­t vÃ  ngÃ y lá»….
- **Dá»¯ liá»‡u Ä‘áº§u vÃ o:** Dá»¯ liá»‡u quáº¹t tháº» tá»« mÃ¡y cháº¥m cÃ´ng, Ä‘Æ¡n xin nghá»‰ phÃ©p, Ä‘Æ¡n lÃ m thÃªm giá» Ä‘Ã£ Ä‘Æ°á»£c duyá»‡t.
- **Thao tÃ¡c chÃ­nh:** Cáº­p nháº­t ngÃ y cÃ´ng, xá»­ lÃ½ ngoáº¡i lá»‡ Ä‘i trá»…/vá» sá»›m, tÃ­nh toÃ¡n theo cÃ´ng thá»©c chuáº©n thÃ´ng qua `TimeSegmentationEngine`, cÃ´ng bá»‘ dá»¯ liá»‡u báº£ng cÃ´ng chi tiáº¿t qua `AttendancePublishingService`.
- **Káº¿t quáº£:** Báº£ng cÃ´ng tá»•ng há»£p `TB_BANGCONG` vÃ  chi tiáº¿t ngÃ y `TB_BANGCONG_NHANVIEN_CHITIET`.

#### C. TÃ­nh toÃ¡n tiá»n lÆ°Æ¡ng (Quy trÃ¬nh cá»‘t lÃµi)
- **Äáº·c táº£ khá»Ÿi phÃ¡t:** QuÃ¡ trÃ¬nh tÃ­nh toÃ¡n lÆ°Æ¡ng **báº¯t buá»™c Ä‘Æ°á»£c khá»Ÿi phÃ¡t tá»« Desktop** (`FrmBangLuong` -> gá»i `BANGLUONG.TinhLuongKyCong` -> gá»i `PayrollEngine`).
- **Dá»¯ liá»‡u Ä‘áº§u vÃ o:** Báº£ng cÃ´ng Ä‘Ã£ chá»‘t, há»£p Ä‘á»“ng lao Ä‘á»™ng hiá»‡u lá»±c, chÃ­nh sÃ¡ch báº£o hiá»ƒm vÃ  thuáº¿ trong `TB_CHINH_SACH_LUONG`, cÃ¡c khoáº£n phÃ¡t sinh lÆ°Æ¡ng (`TB_PHATSINH_LUONG`), táº¡m á»©ng (`TB_UNGLUONG`), phá»¥ cáº¥p (`TB_PHUCAP`).
- **Thao tÃ¡c chÃ­nh:**
  1. Kiá»ƒm tra tráº¡ng thÃ¡i ká»³ cÃ´ng (Ä‘Ã£ chá»‘t cÃ´ng chÆ°a).
  2. Náº¡p chÃ­nh sÃ¡ch thuáº¿/báº£o hiá»ƒm hiá»‡u lá»±c thÃ´ng qua `PolicyResolver`.
  3. TÃ­nh toÃ¡n cÃ´ng nháº­t chuáº©n, lÆ°Æ¡ng cÃ´ng thá»±c táº¿, phá»¥ cáº¥p theo ngÃ y cÃ´ng.
  4. TÃ­nh tiá»n lÃ m thÃªm giá» (ngÃ y thÆ°á»ng, nghá»‰ tuáº§n, lá»…/táº¿t).
  5. TÃ­nh toÃ¡n cÃ¡c khoáº£n trÃ­ch theo lÆ°Æ¡ng cá»§a ngÆ°á»i lao Ä‘á»™ng: BHXH (8%), BHYT (1.5%), BHTN (1%), Kinh phÃ­ cÃ´ng Ä‘oÃ n (theo quy Ä‘á»‹nh).
  6. TÃ­nh giáº£m trá»« gia cáº£nh (báº£n thÃ¢n, ngÆ°á»i phá»¥ thuá»™c) vÃ  thuáº¿ TNCN theo biá»ƒu thuáº¿ lÅ©y tiáº¿n tá»«ng pháº§n.
  7. Trá»« táº¡m á»©ng vÃ  cÃ¡c khoáº£n giáº£m trá»« khÃ¡c; xÃ¡c Ä‘á»‹nh Sá»‘ tiá»n thá»±c lÄ©nh (Net Pay).
- **Káº¿t quáº£:** Dá»¯ liá»‡u lÆ°u vÃ o báº£ng `TB_BANGLUONG`. Web vÃ  Mobile chá»‰ Ä‘á»c dá»¯ liá»‡u nÃ y qua `BangLuongController` vÃ  `MeController` khi ká»³ lÆ°Æ¡ng Ä‘Ã£ Ä‘Æ°á»£c Ä‘Ã¡nh dáº¥u cÃ´ng bá»‘ (`APPROVED` hoáº·c `PUBLISHED`).

---

## 5. Kiáº¿n trÃºc tá»•ng thá»ƒ

### 5.1. SÆ¡ Ä‘á»“ tÆ°Æ¡ng tÃ¡c giá»¯a cÃ¡c thÃ nh pháº§n

```mermaid
flowchart LR
    subgraph Clients["Táº§ng Client Giao Diá»‡n"]
        D["Desktop App\n(WinForms .NET 4.7.2)\n(DevExpress 24.1)"]
        W["Web Portal\n(React 19 + Vite)\n(Ant Design 6)"]
        M["Mobile App\n(React Native 0.86)\n(Expo 57)"]
    end

    subgraph DesktopLogic["Xá»­ lÃ½ nghiá»‡p vá»¥ Desktop"]
        D_Ops["Nghiá»‡p vá»¥, BÃ¡o cÃ¡o & TÃ­nh lÆ°Æ¡ng"]
        D_AI["Giao diá»‡n Chat AI\n(FrmAI_Chat)"]
        D_Client["AiApiClient\n(HTTP REST Client)"]
    end

    subgraph ApiGateway["Táº§ng API Gateway"]
        API["HRMS.Api\n(ASP.NET Web API 2)\n(IIS / IIS Express :55463)"]
        JWT["XÃ¡c thá»±c JWT &\nKiá»ƒm soÃ¡t kÃªnh truy cáº­p"]
        RateLimit["Rate Limiter &\nConcurrency Leaser"]
    end

    subgraph CoreLibraries["ThÆ° viá»‡n dÃ¹ng chung"]
        BUS["HRMS.Business (Bu)\n- Nghiá»‡p vá»¥ NhÃ¢n sá»±, CÃ´ng, LÆ°Æ¡ng\n- Engine TÃ­nh lÆ°Æ¡ng (PayrollEngine)\n- 14 ThÆ° má»¥c AI_Services"]
        DA["HRMS.DataAccess (DA)\n- Entity Framework 6.5.1 (EDMX)\n- MyEntities / AiEntities"]
    end

    subgraph DataStorage["CÆ¡ sá»Ÿ dá»¯ liá»‡u & Dá»‹ch vá»¥ ngoáº¡i vi"]
        Oracle[("Oracle Database 19c\n- Báº£ng nghiá»‡p vá»¥ nhÃ¢n sá»± (HR)\n- Báº£ng chÃ­nh sÃ¡ch & phÃ¢n quyá»n AI\n- GÃ³i Package Reader an toÃ n")]
        Qdrant[("Qdrant Vector DB :6333\n- Collection: hrms_vectors_v2\n- 197 Employee Points (1024-dim)\n- 7 Payload Indexes")]
        Ollama["Ollama Engine :11434\n- Model Chat: qwen2.5:7b-instruct\n- Model Embedding: bge-m3"]
    end

    subgraph SyncTool["CÃ´ng cá»¥ Ä‘á»“ng bá»™ ná»n"]
        Sync["HRMS.VectorDataSync\n(Console CLI C#)"]
    end

    %% Luá»“ng Desktop nghiá»‡p vá»¥
    D --> D_Ops
    D_Ops --> BUS
    D_Ops --> DA

    %% Luá»“ng Desktop AI
    D --> D_AI
    D_AI --> D_Client
    D_Client --> API

    %% Luá»“ng Web & Mobile
    W --> API
    M --> API

    %% Xá»­ lÃ½ táº¡i API
    API --> JWT
    API --> RateLimit
    API --> BUS
    API --> DA

    %% Táº§ng dá»¯ liá»‡u
    BUS --> DA
    DA --> Oracle

    %% AI Integrations
    BUS --> Ollama
    BUS --> Qdrant

    %% Sync Tool
    Sync --> DA
    Sync --> Ollama
    Sync --> Qdrant
```

### 5.2. NguyÃªn lÃ½ kiáº¿n trÃºc
- **MÃ´ hÃ¬nh lai n-Tier:** KhÃ´ng pháº£i kiáº¿n trÃºc microservice phÃ¢n tÃ¡n mÃ  lÃ  mÃ´ hÃ¬nh n-tier truyá»n thá»‘ng cÃ³ chia sáº» thÆ° viá»‡n. Desktop chia sáº» trá»±c tiáº¿p cÃ¡c assembly nghiá»‡p vá»¥ (`Bu.dll`, `DA.dll`), trong khi Web vÃ  Mobile báº¯t buá»™c Ä‘i qua cá»•ng kiá»ƒm soÃ¡t táº­p trung `HRMS.Api`.
- **PhÃ¢n tÃ¡ch luá»“ng AI cá»§a Desktop:** Giao diá»‡n Chat AI trÃªn Desktop (`FrmAI_Chat`) **khÃ´ng** tá»± má»Ÿ káº¿t ná»‘i Oracle hay Qdrant Ä‘á»ƒ truy váº¥n mÃ  Ä‘i qua `AiApiClient` gá»­i request HTTP tá»›i `HRMS.Api`, Ä‘áº£m báº£o má»i cÃ¢u há»i Ä‘á»u chá»‹u sá»± kiá»ƒm soÃ¡t cá»§a Rate Limiting, xÃ¡c thá»±c JWT vÃ  kiá»ƒm tra Scope Grant nhÆ° Web.

---

## 6. CÃ´ng nghá»‡ vÃ  vai trÃ²

| CÃ´ng nghá»‡ | PhiÃªn báº£n cáº¥u hÃ¬nh | Vai trÃ² ká»¹ thuáº­t trong dá»± Ã¡n | RÃ ng buá»™c & Ghi chÃº |
| :--- | :--- | :--- | :--- |
| **C# / .NET Framework** | `v4.7.2` | Ná»n táº£ng backend cho API, Business, DataAccess, Desktop vÃ  VectorDataSync | Cháº¡y trÃªn Windows; biÃªn dá»‹ch qua MSBuild cá»§a Visual Studio 2022 |
| **WinForms** | .NET 4.7.2 | Giao diá»‡n á»©ng dá»¥ng mÃ¡y tráº¡m Desktop cho cÃ¡n bá»™ nhÃ¢n sá»± | ÄÃ²i há»i mÃ´i trÆ°á»ng Windows; xá»­ lÃ½ sá»± kiá»‡n client trá»±c tiáº¿p |
| **DevExpress** | `24.1` | Bá»™ giao diá»‡n nÃ¢ng cao cho Desktop: GridView, TreeList, Ribbon, XtraReports | Cáº§n bá»™ cÃ i Ä‘áº·t vÃ  báº£n quyá»n DevExpress 24.1 khi phÃ¡t triá»ƒn |
| **ASP.NET Web API 2** | `5.2.9` | Cung cáº¥p RESTful API cho Web, Mobile vÃ  Desktop AI Client | LÆ°u trá»¯ trÃªn IIS / IIS Express (cá»•ng máº·c Ä‘á»‹nh `55463`) |
| **Entity Framework** | `6.5.1` | ORM Database-First káº¿t ná»‘i Oracle thÃ´ng qua tá»‡p EDMX | `MyEntities` (nghiá»‡p vá»¥) vÃ  `AiEntities` (chá»‰ Ä‘á»c view AI) |
| **Oracle Client** | `23.7.0` (Managed) | Driver .NET káº¿t ná»‘i trá»±c tiáº¿p Oracle Database 19c | Sá»­ dá»¥ng thÆ° viá»‡n `Oracle.ManagedDataAccess` chÃ­nh hÃ£ng |
| **Oracle Database** | 19c Enterprise / XE | Nguá»“n lÆ°u trá»¯ nghiá»‡p vá»¥ chÃ­nh (HR schema) vÃ  chÃ­nh sÃ¡ch báº£o máº­t AI | Äá»™c láº­p vá»›i Qdrant; má»i sá»‘ liá»‡u cÃ´ng - lÆ°Æ¡ng chuáº©n náº±m táº¡i Ä‘Ã¢y |
| **React** | `19.2.0` | ThÆ° viá»‡n UI xÃ¢y dá»±ng á»©ng dá»¥ng Web SPA quáº£n trá»‹ | Quáº£n lÃ½ tráº¡ng thÃ¡i báº±ng React Hooks; chia sáº» Theme Context |
| **TypeScript** | `~5.9.3` / TS 6 | Äáº£m báº£o tÃ­nh an toÃ n kiá»ƒu dá»¯ liá»‡u cho toÃ n bá»™ frontend Web | BiÃªn dá»‹ch nghiÃªm ngáº·t qua `tsc -b` khÃ´ng phÃ¡t sinh lá»—i |
| **Vite** | `^8.0.0` | CÃ´ng cá»¥ build frontend vÃ  development server cho Web | Cá»•ng máº·c Ä‘á»‹nh `5173`; proxy `/api` sang backend IIS Express |
| **Ant Design** | `^6.3.0` | Bá»™ component giao diá»‡n doanh nghiá»‡p cho Web (Table, Modal, Drawer, Form) | TÃ­ch há»£p cháº¿ Ä‘á»™ Dark/Light vÃ  há»— trá»£ chuyá»ƒn Ä‘á»•i Ä‘a ngÃ´n ngá»¯ |
| **Expo** | `~57.0.0` | Framework phÃ¡t triá»ƒn vÃ  Ä‘Ã³ng gÃ³i á»©ng dá»¥ng di Ä‘á»™ng Ä‘a ná»n táº£ng | Há»— trá»£ phÃ¡t triá»ƒn vÃ  cháº¡y giáº£ láº­p qua Expo CLI |
| **React Native** | `0.86.0` | Ná»n táº£ng di Ä‘á»™ng cho Mobile Client | á»¨ng dá»¥ng táº­p trung vÃ o tÃ­nh nÄƒng ESS cho ngÆ°á»i lao Ä‘á»™ng |
| **Qdrant Vector DB** | `v1.12.1` / `v1.19.0` | CÆ¡ sá»Ÿ dá»¯ liá»‡u vector lÆ°u trá»¯ ngá»¯ nghÄ©a há»“ sÆ¡ nhÃ¢n sá»± vÃ  tÃ i liá»‡u ná»™i bá»™ | Collection má»¥c tiÃªu: `hrms_vectors_v2` (1024 chiá»u, cosine) |
| **Ollama** | Local runtime | Cung cáº¥p mÃ´ hÃ¬nh ngÃ´n ngá»¯ lá»›n (LLM) vÃ  mÃ´ hÃ¬nh vector hÃ³a (Embedding) | Model chat: `qwen2.5:7b-instruct`; Model embedding: `bge-m3` |
| **NUnit** | `3.14.0` | Framework kiá»ƒm thá»­ tá»± Ä‘á»™ng cho há»‡ thá»‘ng backend .NET | TÃ­ch há»£p bá»™ cháº¡y NUnit3TestAdapter cho Visual Studio & CLI |

---

## 7. Cáº¥u trÃºc repository

```text
QuanLyNhanSu/
â”œâ”€â”€ HRMS.sln                                # Solution chÃ­nh chá»©a cÃ¡c project .NET
â”œâ”€â”€ README.md                               # HÆ°á»›ng dáº«n chÃ­nh (Tiáº¿ng Viá»‡t)
â”œâ”€â”€ README.en.md                            # HÆ°á»›ng dáº«n Tiáº¿ng Anh (English)
â”œâ”€â”€ README.ja.md                            # HÆ°á»›ng dáº«n Tiáº¿ng Nháº­t (æ—¥æœ¬èªž)
â”‚
â”œâ”€â”€ HRMS.Desktop/                           # á»¨ng dá»¥ng Windows Forms (.NET Framework 4.7.2)
â”‚   â”œâ”€â”€ FORM_NHANSU/                        # MÃ n hÃ¬nh quáº£n lÃ½ há»“ sÆ¡, há»£p Ä‘á»“ng, khen thÆ°á»Ÿng
â”‚   â”œâ”€â”€ FORM_CHAMCONG/                      # MÃ n hÃ¬nh báº£ng cÃ´ng, tÃ­nh lÆ°Æ¡ng (FrmBangLuong), phá»¥ cáº¥p
â”‚   â”œâ”€â”€ FORM_SYSTEM/                        # MÃ n hÃ¬nh tÃ i khoáº£n, phÃ¢n quyá»n, cáº¥u hÃ¬nh AI (FrmOllamaConfig)
â”‚   â”œâ”€â”€ FORM_BAOCAO/ & Reports/             # Máº«u in áº¥n phiáº¿u lÆ°Æ¡ng, báº£ng cÃ´ng DevExpress
â”‚   â””â”€â”€ Functions/                          # AiApiClient, AiBootstrap, cáº¥u hÃ¬nh tráº¡m
â”‚
â”œâ”€â”€ HRMS.Api/                               # Backend REST API (ASP.NET Web API 2)
â”‚   â”œâ”€â”€ Controllers/                        # AiChat, BangLuong, ChamCong, Me, User, ScopeAdmin...
â”‚   â”œâ”€â”€ Filters/                            # JwtAuthorize, RateLimitAttribute...
â”‚   â”œâ”€â”€ Services/                           # RateLimiterService, JwtService...
â”‚   â””â”€â”€ App_Start/                          # WebApiConfig, RouteConfig, CorsHandler...
â”‚
â”œâ”€â”€ HRMS.Business/                          # ThÆ° viá»‡n nghiá»‡p vá»¥ dÃ¹ng chung (Namespace: Bu)
â”‚   â”œâ”€â”€ CLASS_NHANSU/                       # Nghiá»‡p vá»¥ nhÃ¢n viÃªn, há»£p Ä‘á»“ng, phÃ²ng ban
â”‚   â”œâ”€â”€ CLASS_CHAMCONG/                     # BANGLUONG (facade tÃ­nh lÆ°Æ¡ng), TimeSegmentationEngine...
â”‚   â”œâ”€â”€ CLASS_PAYROLL/                      # PayrollEngine, PolicyResolver, EmployeeProfileResolver...
â”‚   â”œâ”€â”€ CLASS_SECURITY/                     # ChannelCapabilityRegistry, PlatformAccessGuard...
â”‚   â”œâ”€â”€ CLASS_SYSTEM/                       # UserSession, SYS_USER, SYS_CONFIG...
â”‚   â”œâ”€â”€ DTO/                                # Äá»‘i tÆ°á»£ng truyá»n dá»¯ liá»‡u (Data Transfer Objects)
â”‚   â””â”€â”€ Services/AI_Services/               # 53 file C# phÃ¢n bá»• trong 14 thÆ° má»¥c chá»©c nÄƒng:
â”‚       â”œâ”€â”€ Bootstrap/                      # AiServiceLocator
â”‚       â”œâ”€â”€ Configuration/                  # AiConfigurationCoordinator
â”‚       â”œâ”€â”€ Chat/                           # AiExecutionService, ChatboxManager
â”‚       â”œâ”€â”€ Understanding/                  # QueryUnderstandingService, EntityResolver, ClarificationPolicy...
â”‚       â”œâ”€â”€ Planning/                       # QueryPlanner, ExecutionPlan
â”‚       â”œâ”€â”€ Retrieval/                      # ScopedSqlExecutor, QdrantService, HybridRagService...
â”‚       â”œâ”€â”€ Responses/                      # DeterministicResponseRenderer, RagSynthesizer, FastResponseService
â”‚       â”œâ”€â”€ Providers/                      # OllamaService (LLM & Embedding HTTP client)
â”‚       â”œâ”€â”€ Prompts/                        # JsonPromptManager (náº¡p prompt á»©ng dá»¥ng)
â”‚       â”œâ”€â”€ Memory/                         # AiCacheCoordinator, ConversationStateManager...
â”‚       â”œâ”€â”€ Security/                       # AiAuthorizationService, AiScopeEvaluator, ScopeGrantManagement...
â”‚       â”œâ”€â”€ Indexing/                       # AiDataSyncHub, QdrantOutboxManager
â”‚       â”œâ”€â”€ Interfaces/                     # Há»£p Ä‘á»“ng IVectorService, ILlmService, IScopedSqlExecutor...
â”‚       â””â”€â”€ Runtime/                        # SystemClockProvider, FakeClockProvider
â”‚
â”œâ”€â”€ HRMS.DataAccess/                        # Táº§ng káº¿t ná»‘i dá»¯ liá»‡u Entity Framework 6 (Namespace: DA)
â”‚   â”œâ”€â”€ QLNhanSu.edmx                       # Database-First Model (Oracle)
â”‚   â”œâ”€â”€ MyEntities.cs                       # Context nghiá»‡p vá»¥ chÃ­nh
â”‚   â””â”€â”€ MyEntities.ChannelRights.cs         # PhÃ¢n quyá»n 3 kÃªnh vÃ  báº£ng TB_SYS_RIGHT_CHANNEL
â”‚
â”œâ”€â”€ HRMS.Web/                               # á»¨ng dá»¥ng Web Quáº£n trá»‹ (React 19 + TypeScript + Vite)
â”‚   â”œâ”€â”€ src/pages/                          # DashboardPage, NhanVienPage, BangLuongPage, ApprovalCenterPage...
â”‚   â”œâ”€â”€ src/components/                     # PhanQuyenModal, AiChatDrawer, CommandPaletteModal...
â”‚   â””â”€â”€ src/locales/                        # Báº£n dá»‹ch Ä‘a ngÃ´n ngá»¯: vi, en, ja, ko, zh-CN
â”‚
â”œâ”€â”€ HRMS.Mobile/                            # á»¨ng dá»¥ng Di Ä‘á»™ng Tá»± phá»¥c vá»¥ (React Native Expo 57)
â”‚   â””â”€â”€ src/                                # Screens (Profile, Attendance, Payroll), Navigation, Api...
â”‚
â”œâ”€â”€ HRMS.VectorDataSync/                    # Console CLI quáº£n trá»‹ vÃ  Ä‘á»“ng bá»™ vector Qdrant
â”‚   â””â”€â”€ Program.cs                          # CLI: preflight, verify, rebuild, activate, reconcile...
â”‚
â”œâ”€â”€ HRMS.Tests/                             # Dá»± Ã¡n kiá»ƒm thá»­ NUnit 3 (net472)
â”‚   â”œâ”€â”€ PlatformAccessAndSessionEnforcementTests.cs # Kiá»ƒm thá»­ 32 ká»‹ch báº£n phÃ¢n quyá»n ná»n táº£ng
â”‚   â”œâ”€â”€ PostReviewRemediationVerificationTests.cs    # Kiá»ƒm thá»­ cáº¯t chuyá»ƒn Qdrant v2 & coordinator
â”‚   â””â”€â”€ AntigravityUnifiedAiAndPermissionsVerificationTests.cs # Kiá»ƒm thá»­ tÃ­ch há»£p AI & Scope
â”‚
â”œâ”€â”€ database/                               # Script cÆ¡ sá»Ÿ dá»¯ liá»‡u
â”‚   â”œâ”€â”€ migrations/                         # Migration V1_0 Ä‘áº¿n V1_33 (DDL, DML, Rollback, Verify)
â”‚   â”œâ”€â”€ realistic200/                       # Bá»™ dá»¯ liá»‡u máº«u 200 nhÃ¢n sá»± thá»±c táº¿
â”‚   â””â”€â”€ backups/                            # Báº£n sao lÆ°u cáº¥u hÃ¬nh vÃ  metadata
â”‚
â”œâ”€â”€ docs/                                   # TÃ i liá»‡u ká»¹ thuáº­t dá»± Ã¡n (13 tÃ i liá»‡u hoáº¡t Ä‘á»™ng)
â”‚   â”œâ”€â”€ README.md                           # Má»¥c lá»¥c hÆ°á»›ng dáº«n chi tiáº¿t
â”‚   â”œâ”€â”€ ai-services-guide.md                # Sá»• tay chi tiáº¿t 53 file C# AI trong 14 thÆ° má»¥c
â”‚   â”œâ”€â”€ ai-rag-and-account-permissions-guide.md # HÆ°á»›ng dáº«n RAG & PhÃ¢n quyá»n ná»n táº£ng
â”‚   â””â”€â”€ archive/                            # NÆ¡i lÆ°u trá»¯ tÃ i liá»‡u ká»¹ thuáº­t vÃ  bÃ¡o cÃ¡o lá»‹ch sá»­
â”‚
â”œâ”€â”€ docker-compose.yml                      # Cáº¥u hÃ¬nh container dá»‹ch vá»¥: Oracle, Qdrant, Ollama, Web
â”œâ”€â”€ start_local_backend.bat                 # Script cháº¡y IIS Express backend cá»•ng 55463
â””â”€â”€ build_deploy.ps1                        # Script build vÃ  Ä‘Ã³ng gÃ³i triá»ƒn khai cá»¥c bá»™
```

---

## 8. CÃ¡c luá»“ng nghiá»‡p vá»¥

### 8.1. Luá»“ng tÃ­nh lÆ°Æ¡ng vÃ  Ä‘á»‘i soÃ¡t cÃ´ng bá»‘

```mermaid
flowchart TD
    subgraph AttendancePhase["1. Cháº¥m cÃ´ng & Chá»‘t cÃ´ng"]
        Raw["Dá»¯ liá»‡u cháº¥m cÃ´ng thÃ´ / Quáº¹t tháº»"] --> Seg["TimeSegmentationEngine\nPhÃ¢n Ä‘oáº¡n ca, cÃ´ng Ä‘Ãªm, tÄƒng ca"]
        Seg --> Detail["Báº£ng cÃ´ng chi tiáº¿t nhÃ¢n viÃªn\n(TB_BANGCONG_NHANVIEN_CHITIET)"]
        Detail --> Pub["AttendancePublishingService\nChá»‘t & CÃ´ng bá»‘ ká»³ cÃ´ng"]
    end

    subgraph DesktopPayroll["2. Khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng (Desktop Ä‘á»™c quyá»n)"]
        Pub --> FrmBL["Giao diá»‡n Desktop: FrmBangLuong\n(ChuyÃªn viÃªn chá»n Ká»³ cÃ´ng & Báº¥m TÃ­nh lÆ°Æ¡ng)"]
        FrmBL --> BL_Facade["BANGLUONG.TinhLuongKyCong"]
        BL_Facade --> Engine["PayrollEngine.CalculatePayroll"]

        Profile["EmployeeProfileResolver\n(Há»“ sÆ¡ nhÃ¢n viÃªn, Há»£p Ä‘á»“ng hiá»‡u lá»±c)"] --> Engine
        Policy["PolicyResolver\n(TB_CHINH_SACH_LUONG: Tá»· lá»‡ BH, Biá»ƒu thuáº¿)"] --> Engine
        Occur["PayrollOccurrenceService\n(PhÃ¡t sinh lÆ°Æ¡ng, Phá»¥ cáº¥p, Táº¡m á»©ng)"] --> Engine
    end

    subgraph CalculationCore["3. TÃ­nh toÃ¡n chi tiáº¿t tá»«ng khoáº£n"]
        Engine --> Cal1["TÃ­nh lÆ°Æ¡ng cÃ´ng thá»±c táº¿ & Phá»¥ cáº¥p cÃ´ng"]
        Cal1 --> Cal2["TÃ­nh tiá»n lÃ m thÃªm giá» (OT thÆ°á»ng, Ä‘Ãªm, lá»…)"]
        Cal2 --> Cal3["TrÃ­ch ná»™p báº¯t buá»™c: BHXH (8%), BHYT (1.5%), BHTN (1%), KPCÄ"]
        Cal3 --> Cal4["TÃ­nh giáº£m trá»« gia cáº£nh & Thuáº¿ TNCN lÅ©y tiáº¿n"]
        Cal4 --> Cal5["Trá»« táº¡m á»©ng & XÃ¡c Ä‘á»‹nh Thá»±c lÄ©nh (Net Pay)"]
    end

    subgraph PersistenceAndPublishing["4. LÆ°u trá»¯ & Quáº£n lÃ½ tráº¡ng thÃ¡i"]
        Cal5 --> SaveDB[("LÆ°u káº¿t quáº£ vÃ o Oracle\nTB_BANGLUONG\n(Tráº¡ng thÃ¡i: CALCULATED)")]
        SaveDB --> Approve["CÃ¡n bá»™ cÃ³ tháº©m quyá»n duyá»‡t\n(Tráº¡ng thÃ¡i chuyá»ƒn: APPROVED / PUBLISHED)"]
    end

    subgraph LookupPhase["5. Tra cá»©u káº¿t quáº£ lÆ°Æ¡ng (Web & Mobile)"]
        Approve --> API_BL["BangLuongController\n(REST API :55463)"]
        API_BL --> Web_BL["HRMS.Web: BangLuongPage\n(Xem báº£ng lÆ°Æ¡ng theo pháº¡m vi phÃ¢n quyá»n)"]
        API_BL --> Mob_BL["HRMS.Mobile: PayrollScreen\n(NhÃ¢n viÃªn xem phiáº¿u lÆ°Æ¡ng cÃ¡ nhÃ¢n qua /api/me/payroll)"]
    end
```

### 8.2. Quy trÃ¬nh xá»­ lÃ½ yÃªu cáº§u vÃ  phÃª duyá»‡t (ESS)
1. **NhÃ¢n viÃªn láº­p Ä‘Æ¡n:** NgÆ°á»i dÃ¹ng má»Ÿ Web (`ApprovalCenterPage`) hoáº·c Mobile (`LeaveRequestScreen`, `OvertimeRequestScreen`, `AdvanceSalaryScreen`), nháº­p thÃ´ng tin (ngÃ y nghá»‰, lÃ½ do, sá»‘ tiá»n á»©ng).
2. **Tiáº¿p nháº­n táº¡i API:** `MeController` tiáº¿p nháº­n request, kiá»ƒm tra danh tÃ­nh tá»« JWT, lÆ°u báº£n ghi vÃ o `TB_YEUCAU` vá»›i tráº¡ng thÃ¡i `PENDING`.
3. **ThÃ´ng bÃ¡o quáº£n lÃ½:** NgÆ°á»i quáº£n lÃ½ trá»±c tiáº¿p nháº­n thÃ´ng tin yÃªu cáº§u trong danh sÃ¡ch chá» duyá»‡t.
4. **PhÃª duyá»‡t / Tá»« chá»‘i:** Quáº£n lÃ½ báº¥m Duyá»‡t hoáº·c Tá»« chá»‘i kÃ¨m lÃ½ do. `ApprovalController` kiá»ƒm tra quyá»n phÃª duyá»‡t, ghi nháº­n `APPROVED` hoáº·c `REJECTED`, ghi log audit vÃ  cáº­p nháº­t dá»¯ liá»‡u liÃªn quan (cÃ´ng, phÃ©p hoáº·c táº¡m á»©ng).

---

## 9. ÄÄƒng nháº­p vÃ  phÃ¢n quyá»n

### 9.1. Ma tráº­n báº£o máº­t 3 táº§ng

```mermaid
flowchart TD
    Login["YÃªu cáº§u ÄÄƒng nháº­p\n(TÃ i khoáº£n + Máº­t kháº©u + KÃªnh káº¿t ná»‘i)"] --> Auth["XÃ¡c thá»±c tÃ i khoáº£n & KhÃ³a báº£o máº­t (BCrypt)"]
    Auth --> ChkDisable{"TÃ i khoáº£n bá»‹\nvÃ´ hiá»‡u hÃ³a (DISABLED=1)?"}
    ChkDisable -- CÃ³ --> DenyLogin["Tá»« chá»‘i Ä‘Äƒng nháº­p (401 / Account Disabled)"]

    ChkDisable -- KhÃ´ng --> ChkGate{"Kiá»ƒm tra Cá»•ng ná»n táº£ng:\nF_LOGIN_DESKTOP / F_LOGIN_WEB / F_LOGIN_MOBILE"}
    ChkGate -- KhÃ´ng cÃ³ quyá»n --> DenyPlatform["Tá»« chá»‘i truy cáº­p ná»n táº£ng\n(PLATFORM_ACCESS_DENIED)"]

    ChkGate -- ÄÆ°á»£c phÃ©p --> GenToken["Cáº¥p JWT Token (Web/Mobile)\nHoáº·c táº¡o UserSession (Desktop)\nKÃ¨m SessionId, Jti, SecurityVersion"]

    GenToken --> ReqAction["NgÆ°á»i dÃ¹ng thá»±c hiá»‡n thao tÃ¡c trÃªn chá»©c nÄƒng (F_*)"]

    ReqAction --> ChkChannelSupport{"ChannelCapabilityRegistry:\nKÃªnh cÃ³ há»— trá»£ thao tÃ¡c nÃ y khÃ´ng?"}
    ChkChannelSupport -- KhÃ´ng há»— trá»£ --> DenyCap["VÃ´ hiá»‡u hÃ³a / Tá»« chá»‘i thao tÃ¡c trÃªn kÃªnh"]

    ChkChannelSupport -- CÃ³ há»— trá»£ --> ChkActionRight{"Chi tiáº¿t quyá»n 5 thao tÃ¡c:\nVIEW, ADD, EDIT, DELETE, PRINT"}
    ChkActionRight -- KhÃ´ng cÃ³ quyá»n --> DenyAction["Tá»« chá»‘i thao tÃ¡c (403 Forbidden)"]

    ChkActionRight -- Há»£p lá»‡ --> EvalScope{"ÄÃ¡nh giÃ¡ Pháº¡m vi dá»¯ liá»‡u (Data Scope):\nSELF | DEPARTMENT | COMPANY | ALL"}
    EvalScope --> ExecSQL["Thá»±c thi truy váº¥n lá»c chÃ­nh xÃ¡c báº£n ghi theo pháº¡m vi"]
```

### 9.2. CÃ¡c nguyÃªn táº¯c phÃ¢n quyá»n cá»‘t lÃµi
- **Quyá»n cha cá»§a kÃªnh (Platform Gate):** `F_LOGIN_DESKTOP`, `F_LOGIN_WEB`, `F_LOGIN_MOBILE` Ä‘Ã³ng vai trÃ² lÃ  "cá»•ng kiá»ƒm soÃ¡t" vÃ o há»‡ thá»‘ng. Náº¿u tÃ i khoáº£n bá»‹ táº¯t quyá»n cha cá»§a kÃªnh nÃ o, toÃ n bá»™ quyá»n con trÃªn kÃªnh Ä‘Ã³ Ä‘á»u bá»‹ vÃ´ hiá»‡u hÃ³a láº­p tá»©c, ká»ƒ cáº£ Ä‘á»‘i vá»›i quáº£n trá»‹ viÃªn.
- **Fail-Closed Security (An toÃ n khi lá»—i):** Tuyá»‡t Ä‘á»‘i khÃ´ng kiá»ƒm tra quyá»n báº±ng so sÃ¡nh chuá»—i tÃªn tÃ i khoáº£n `USERNAME == "admin"`. Má»i ngÆ°á»i dÃ¹ng (ká»ƒ cáº£ Admin) Ä‘á»u pháº£i cÃ³ báº£n ghi quyá»n há»£p lá»‡ trong `TB_SYS_RIGHT_CHANNEL` hoáº·c `TB_AI_SCOPE_GRANT`.
- **Zero-Trust Session Guard:** Má»i request Ä‘á»u kiá»ƒm tra tÃ­nh há»£p lá»‡ cá»§a phiÃªn lÃ m viá»‡c (`SessionId`, `Jti`, `TokenVersion`). Khi quyá»n bá»‹ thay Ä‘á»•i hoáº·c tÃ i khoáº£n bá»‹ khÃ³a, `TokenVersion` tÄƒng lÃªn lÃ m máº¥t hiá»‡u lá»±c token ngay láº­p tá»©c.
- **Atomic Batch Save:** Giao diá»‡n phÃ¢n quyá»n `PhanQuyenModal` gá»i endpoint lÆ°u gá»™p Ä‘á»“ng thá»i 3 kÃªnh (`SaveBatchChannelPermissions`) trong má»™t Transaction CSDL duy nháº¥t, trÃ¡nh tÃ¬nh tráº¡ng báº¥t Ä‘á»“ng bá»™ tráº¡ng thÃ¡i khi lÆ°u tá»«ng kÃªnh.

---

## 10. AI, SQL vÃ  RAG

### 10.1. SÆ¡ Ä‘á»“ xá»­ lÃ½ há»™i thoáº¡i Copilot

```mermaid
flowchart TD
    UserQuery["NgÆ°á»i dÃ¹ng gá»­i cÃ¢u há»i\n(Web Drawer hoáº·c Desktop FrmAI_Chat)"] --> API_Chat["AiChatController: POST /api/ai/chat"]
    API_Chat --> RateGuard["RateLimiterService:\n- Rate Limit theo IP & User\n- Concurrency Leaser (giá»¯ slot cho tá»›i khi hoÃ n táº¥t)"]

    RateGuard --> ExecService["AiExecutionService.ProcessChatAsync"]
    ExecService --> StateMgr["ConversationStateManager:\nKiá»ƒm tra ngá»¯ cáº£nh há»™i thoáº¡i Ä‘a lÆ°á»£t"]
    ExecService --> AuthCtx["AiPolicyProvider:\nDá»±ng AiAuthorizationContext tá»« JWT (khÃ´ng tin client)"]

    ExecService --> NLP["QueryUnderstandingService:\n- QueryPreprocessor: chuáº©n hÃ³a Unicode\n- EntityResolver: phÃ¢n giáº£i tÃªn nhÃ¢n viÃªn/phÃ²ng ban\n- ClarificationPolicy: phÃ¡t hiá»‡n trÃ¹ng tÃªn/mÆ¡ há»“"]

    NLP --> NeedClarify{"Cáº§n lÃ m rÃµ\n(TrÃ¹ng tÃªn, thiáº¿u ká»³)?"}
    NeedClarify -- CÃ³ --> ReturnClarify["Tráº£ vá» danh sÃ¡ch lá»±a chá»n lÃ m rÃµ\n(ChÆ°a truy váº¥n cÆ¡ sá»Ÿ dá»¯ liá»‡u)"]

    NeedClarify -- KhÃ´ng --> Planner["QueryPlanner.PlanQuery:\nChá»n chiáº¿n lÆ°á»£c (ExecutionStrategy)"]

    Planner --> StrategySwitch{"Chiáº¿n lÆ°á»£c thá»±c thi?"}

    StrategySwitch -- DeterministicDirect --> FastResp["FastResponseService:\nTráº£ lá»i chÃ o há»i / Giá»›i thiá»‡u tÃ­nh nÄƒng"]
    StrategySwitch -- SqlTemplate --> SqlExec["ScopedSqlExecutor:\nThá»±c thi SQL Template Ä‘Æ°á»£c duyá»‡t qua OraclePackage"]
    StrategySwitch -- VectorSearch --> VecExec["QdrantService.SearchScopedAsync:\nLá»c vector theo Tag & Security Filter"]
    StrategySwitch -- Hybrid --> HybridExec["Hybrid Flow:\nCháº¡y song song SqlTemplate + Scoped Vector"]
    StrategySwitch -- Forbidden / Unsupported --> DenyResp["Tá»« chá»‘i truy váº¥n / ChÆ°a há»— trá»£ nghiá»‡p vá»¥"]

    SqlExec --> EvalPerm{"Kiá»ƒm tra quyá»n SQL:\nAuthorizationDenied?"}
    EvalPerm -- Bá»‹ tá»« chá»‘i --> RespForbidden["Tráº£ vá» lá»—i 403 Forbidden ngay láº­p tá»©c\n(KHÃ”NG chuyá»ƒn sang 200 answered)"]

    EvalPerm -- ThÃ nh cÃ´ng --> Renderer["DeterministicResponseRenderer:\nÄá»‹nh dáº¡ng báº£ng sá»‘ liá»‡u chÃ­nh xÃ¡c"]
    VecExec --> Synthesizer["RagSynthesizer:\nTÃ³m táº¯t tÃ i liá»‡u trÃ­ch dáº«n qua Ollama LLM"]
    HybridExec --> MergeResp["Tá»•ng há»£p Báº±ng chá»©ng:\nSá»‘ liá»‡u chuáº©n tá»« SQL + TrÃ­ch dáº«n tá»« Vector"]

    Renderer --> FinalResp["Tráº£ káº¿t quáº£ chuáº©n xÃ¡c vá» UI"]
    Synthesizer --> FinalResp
    MergeResp --> FinalResp
    FastResp --> FinalResp
    ReturnClarify --> FinalResp
    DenyResp --> FinalResp
```

### 10.2. CÃ¡c quy táº¯c an toÃ n trong xá»­ lÃ½ AI
- **NgÄƒn cháº·n SQL Injection tuyá»‡t Ä‘á»‘i:** Loáº¡i bá» hoÃ n toÃ n viá»‡c cho LLM tá»± sinh cÃ¢u lá»‡nh SQL tÃ¹y Ã½. Há»‡ thá»‘ng chá»‰ sá»­ dá»¥ng cÃ¡c máº«u SQL cá»‘ Ä‘á»‹nh (`SqlTemplate`) Ä‘Ã£ Ä‘Äƒng kÃ½ trong `AiCapabilityCatalog`, truyá»n tham sá»‘ báº±ng `OracleParameter` cÃ³ Ä‘á»‹nh kiá»ƒu.
- **Fail-closed á»Ÿ chiáº¿n lÆ°á»£c Hybrid:** Trong luá»“ng truy váº¥n lai (káº¿t há»£p SQL sá»‘ liá»‡u vÃ  Vector tÃ i liá»‡u), náº¿u nhÃ¡nh SQL bá»‹ tá»« chá»‘i quyá»n hoáº·c gáº·p lá»—i thá»±c thi, há»‡ thá»‘ng tráº£ vá» ngay mÃ£ tráº¡ng thÃ¡i `forbidden` (403) hoáº·c `error` (500), tuyá»‡t Ä‘á»‘i khÃ´ng giáº¥u lá»—i Ä‘á»ƒ tráº£ vá» 200.
- **Lá»c Vector Ä‘a táº§ng (Security Filter):** TÃ¬m kiáº¿m vector trong Qdrant luÃ´n Ã¡p dá»¥ng bá»™ lá»c quyá»n hiá»‡u lá»±c:
  - `SELF`: Chá»‰ tÃ¬m tháº¥y dá»¯ liá»‡u vector cá»§a chÃ­nh nhÃ¢n viÃªn gá»i truy váº¥n (`employeeId`).
  - `DEPARTMENT`: Chá»‰ tÃ¬m tháº¥y nhÃ¢n viÃªn trong cÃ¡c phÃ²ng ban Ä‘Æ°á»£c phÃ©p (`departmentId`). NhÃ¢n viÃªn chÆ°a gÃ¡n phÃ²ng ban (`departmentId <= 0`) khÃ´ng thá»ƒ xem dá»¯ liá»‡u phÃ²ng ban cá»§a ngÆ°á»i khÃ¡c.
  - `COMPANY` / `ALL`: Giá»›i háº¡n theo cÃ´ng ty hoáº·c toÃ n quyá»n theo pháº¡m vi quáº£n trá»‹.
- **Báº£o toÃ n Slot Concurrency:** Há»‡ thá»‘ng giá»¯ chá»— thá»±c thi (concurrency lease) trong `RateLimiterService` suá»‘t thá»i gian LLM sinh vÄƒn báº£n vÃ  chá»‰ giáº£i phÃ³ng sau khi request hoÃ n táº¥t, báº£o vá»‡ tÃ i nguyÃªn mÃ¡y chá»§.

---

## 11. Dá»¯ liá»‡u vÃ  Ä‘á»“ng bá»™ vector

### 11.1. Hiá»‡n tráº¡ng kho lÆ°u trá»¯ Vector (Qdrant)

```mermaid
flowchart LR
    subgraph SourceDB["CÆ¡ sá»Ÿ dá»¯ liá»‡u Nguá»“n (Oracle)"]
        HR_NV["HR.TB_NHANVIEN\n(Há»“ sÆ¡ 200 nhÃ¢n sá»± thá»±c táº¿)"]
        RegDocs["Quy cháº¿ & Quy Ä‘á»‹nh Ná»™i bá»™\n(ChÆ°a náº¡p tÃ i liá»‡u chÃ­nh thá»©c)"]
    end

    subgraph SyncEngine["HRMS.VectorDataSync (Console CLI)"]
        CLI["Program.cs\nCommands: verify, rebuild, activate"]
        Chunker["Äá»‹nh dáº¡ng Text & Payload Metadata"]
        Embedder["Ollama: bge-m3\n(Sinh vector 1024 chiá»u)"]
    end

    subgraph QdrantTarget["Qdrant Vector Server :6333"]
        V2[("Collection: hrms_vectors_v2\n- 197 Äiá»ƒm vector nhÃ¢n sá»± (tag=EMPLOYEE)\n- 0 Äiá»ƒm quy Ä‘á»‹nh (tag=REGULATION)\n- Khoáº£ng cÃ¡ch: Cosine\n- 7 Payload Indexes (tag, departmentId, companyId, employeeId...)")]
        Legacy[("Collection: hrms_vectors (v1 legacy)\n- ÄÃ£ cáº¯t chuyá»ƒn thÃ nh cÃ´ng sang v2")]
    end

    HR_NV --> Chunker
    Chunker --> Embedder
    Embedder --> CLI
    CLI --> V2

    RegDocs -. "Káº¿ hoáº¡ch náº¡p tÃ i liá»‡u sau" .-> Chunker
```

### 11.2. Tráº¡ng thÃ¡i quan sÃ¡t thá»±c táº¿ (Ghi nháº­n 09/10/2026)
- **Collection hoáº¡t Ä‘á»™ng:** `hrms_vectors_v2` lÃ  collection máº·c Ä‘á»‹nh Ä‘Æ°á»£c cáº¥u hÃ¬nh trong `QdrantService.cs`.
- **Sá»‘ lÆ°á»£ng Ä‘iá»ƒm vector:** CÃ³ chÃ­nh xÃ¡c **197 Ä‘iá»ƒm vector** nhÃ¢n viÃªn há»£p lá»‡ (trÃªn tá»•ng sá»‘ 200 nhÃ¢n sá»±, do 2 nhÃ¢n viÃªn chÆ°a gÃ¡n phÃ²ng ban vÃ  1 nhÃ¢n viÃªn thá»­ viá»‡c chÆ°a kÃ­ch hoáº¡t há»“ sÆ¡ Ä‘áº§y Ä‘á»§).
- **Tráº¡ng thÃ¡i tÃ i liá»‡u quy cháº¿:** NhÃ£n `tag = "REGULATION"` hiá»‡n cÃ³ **0 Ä‘iá»ƒm vector** (chÆ°a náº¡p vÄƒn báº£n chÃ­nh sÃ¡ch Ä‘Ã£ Ä‘Æ°á»£c duyá»‡t). Khi ngÆ°á»i dÃ¹ng há»i vá» quy cháº¿, AI tráº£ lá»i trung thá»±c lÃ  thiáº¿u nguá»“n tÃ i liá»‡u thay vÃ¬ bá»‹a Ä‘áº·t quy Ä‘á»‹nh.
- **7 Payload Index tá»‘i Æ°u hÃ³a:** ÄÃ£ táº¡o index trÆ°á»ng trÃªn Qdrant cho: `tag` (keyword), `departmentId` (integer), `companyId` (integer), `employeeId` (integer), `domain` (keyword), `documentType` (keyword), `visibility_profile` (keyword).
- **NguyÃªn táº¯c Ä‘á»c báº¥t biáº¿n:** HÃ m tÃ¬m kiáº¿m `SearchScopedAsync` chá»‰ thá»±c hiá»‡n lá»‡nh `GET /collections/{name}` vÃ  khÃ´ng bao giá» tá»± Ã½ gá»­i lá»‡nh `PUT` Ä‘á»ƒ táº¡o collection khi Ä‘á»c.

---

## 12. Chuáº©n bá»‹ vÃ  cháº¡y cá»¥c bá»™

### 12.1. YÃªu cáº§u mÃ´i trÆ°á»ng
- **Há»‡ Ä‘iá»u hÃ nh:** Windows 10 / 11 hoáº·c Windows Server 2019 / 2022.
- **CÃ´ng cá»¥ phÃ¡t triá»ƒn:** Visual Studio 2022 (báº£n Community, Professional hoáº·c Enterprise) kÃ¨m workload *.NET desktop development* vÃ  *.NET Framework 4.7.2 targeting pack*.
- **Bá»™ thÆ° viá»‡n UI:** DevExpress V24.1 (báº¯t buá»™c Ä‘á»ƒ má»Ÿ vÃ  build project `HRMS.Desktop`).
- **CÆ¡ sá»Ÿ dá»¯ liá»‡u:** Oracle Database 19c (Local hoáº·c Docker) Ä‘Ã£ import schema `HR` vÃ  cÃ¡c báº£ng chÃ­nh sÃ¡ch AI.
- **Node.js & npm:** Node.js phiÃªn báº£n `>= 20.19.0` (khuyÃªn dÃ¹ng Node 20 LTS hoáº·c 22 LTS).
- **Dá»‹ch vá»¥ AI:**
  - Ollama cháº¡y cá»¥c bá»™ táº¡i `http://localhost:11434` (Ä‘Ã£ kÃ©o model: `ollama pull qwen2.5:7b-instruct` vÃ  `ollama pull bge-m3`).
  - Qdrant cháº¡y táº¡i `http://localhost:6333`.

### 12.2. Thá»© tá»± khá»Ÿi Ä‘á»™ng cÃ¡c thÃ nh pháº§n

#### BÆ°á»›c 1: Khá»Ÿi Ä‘á»™ng CÆ¡ sá»Ÿ dá»¯ liá»‡u vÃ  AI Engine
Khá»Ÿi Ä‘á»™ng container hoáº·c dá»‹ch vá»¥ Oracle, Ollama vÃ  Qdrant:
```powershell
# Kiá»ƒm tra dá»‹ch vá»¥ Ollama
curl http://localhost:11434/api/tags

# Kiá»ƒm tra dá»‹ch vá»¥ Qdrant
curl http://localhost:6333/collections
```

#### BÆ°á»›c 2: Cáº¥u hÃ¬nh vÃ  Khá»Ÿi Ä‘á»™ng Backend API
1. Má»Ÿ file [HRMS.Api/Web.config](HRMS.Api/Web.config), kiá»ƒm tra chuá»—i káº¿t ná»‘i Oracle trong `connectionStrings`:
   - `QLNhanSuEntities` (truy cáº­p schema chÃ­nh `HR`).
   - `AiEntities` (truy cáº­p view AI).
2. BiÃªn dá»‹ch solution qua Visual Studio hoáº·c cháº¡y lá»‡nh MSBuild:
   ```powershell
   & "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" HRMS.Api\HRMS.Api.csproj /p:Configuration=Debug /v:m
   ```
3. Cháº¡y backend báº±ng IIS Express thÃ´ng qua script cÃ³ sáºµn táº¡i thÆ° má»¥c gá»‘c:
   ```powershell
   .\start_local_backend.bat
   ```
   *Backend API sáº½ láº¯ng nghe táº¡i Ä‘á»‹a chá»‰:* `http://localhost:55463`

#### BÆ°á»›c 3: Khá»Ÿi Ä‘á»™ng Web Quáº£n trá»‹
Má»Ÿ cá»­a sá»• dÃ²ng lá»‡nh má»›i:
```powershell
cd HRMS.Web
npm ci
npm run dev
```
*Giao diá»‡n Web sáº½ cháº¡y táº¡i:* `http://localhost:5173`. Tá»‡p cáº¥u hÃ¬nh Vite tá»± Ä‘á»™ng proxy cÃ¡c request `/api` sang `http://localhost:55463`.

#### BÆ°á»›c 4: Khá»Ÿi Ä‘á»™ng Desktop App
1. Má»Ÿ `HRMS.sln` trong Visual Studio 2022.
2. Äáº·t `HRMS.Desktop` lÃ m Startup Project.
3. Nháº¥n **F5** hoáº·c **Ctrl + F5** Ä‘á»ƒ cháº¡y á»©ng dá»¥ng.
4. ÄÄƒng nháº­p báº±ng tÃ i khoáº£n Ä‘Æ°á»£c cáº¥p quyá»n Desktop (vÃ­ dá»¥: `ADMIN`).

#### BÆ°á»›c 5: Khá»Ÿi Ä‘á»™ng Mobile App (TÃ¹y chá»n)
Má»Ÿ cá»­a sá»• dÃ²ng lá»‡nh má»›i:
```powershell
cd HRMS.Mobile
npm ci
npm run start
```
Sá»­ dá»¥ng á»©ng dá»¥ng Expo Go trÃªn Ä‘iá»‡n thoáº¡i hoáº·c mÃ¡y áº£o Android/iOS Ä‘á»ƒ quÃ©t mÃ£ QR. LÆ°u Ã½ cáº­p nháº­t `API_BASE_URL` trong [HRMS.Mobile/src/config/index.ts](HRMS.Mobile/src/config/index.ts) thÃ nh IP máº¡ng LAN cá»§a mÃ¡y cháº¡y API (khÃ´ng dÃ¹ng `localhost` trÃªn thiáº¿t bá»‹ tháº­t).

---

## 13. Kiá»ƒm tra vÃ  káº¿t quáº£ Ä‘Ã£ ghi nháº­n

DÆ°á»›i Ä‘Ã¢y lÃ  cÃ¡c káº¿t quáº£ kiá»ƒm chá»©ng thá»±c táº¿ Ä‘Ã£ cháº¡y trÃªn repository vÃ o ngÃ y **09/10/2026**:

| Háº¡ng má»¥c kiá»ƒm tra | Lá»‡nh / CÃ´ng cá»¥ thá»±c hiá»‡n | Pháº¡m vi kiá»ƒm tra | Káº¿t quáº£ ghi nháº­n | Giá»›i háº¡n Ä‘Ã£ xÃ¡c Ä‘á»‹nh |
| :--- | :--- | :--- | :---: | :--- |
| **PhÃ¢n quyá»n ná»n táº£ng (3 kÃªnh)** | `dotnet test --filter "PlatformAccess..."` | 32 ká»‹ch báº£n phÃ¢n quyá»n ná»n táº£ng, token version, wildcard star | **32/32 PASSED (100%)** | Test cháº¡y offline trÃªn mock context |
| **AI Security & RBAC** | `dotnet test --filter "Ai|Antigravity|Rate..."` | 234 test kiá»ƒm thá»­ Prompt injection, Cache, Scope, Rate Limiter | **234/234 PASSED (100%)** | DÃ¹ng schema test vÃ  in-memory probe |
| **Qdrant v2 Cutover & Index** | `FollowupReviewProbe.exe` | 11 probe kiá»ƒm tra bá»™ lá»c báº£o máº­t, concurrency leaser, collection v2 | **11/11 PASSED (100%)** | Kiá»ƒm tra trá»±c tiáº¿p server Qdrant local |
| **Kiá»ƒm thá»­ Web Frontend** | `npm test` trong `HRMS.Web` | 5 test suite: KPI, Intro, Carousel, AI Chatbot, Permission Modal | **5/5 SUITES PASSED** | Kiá»ƒm thá»­ logic trÃªn node test runner |
| **BiÃªn dá»‹ch TypeScript** | `npx tsc -b` trong `HRMS.Web` | ToÃ n bá»™ codebase TypeScript cá»§a á»©ng dá»¥ng Web | **0 Lá»–I (Build Succeeded)** | Äáº£m báº£o tÃ­nh toÃ n váº¹n kiá»ƒu dá»¯ liá»‡u |
| **BiÃªn dá»‹ch Backend C#** | MSBuild cho Business, Api, Tests | MÃ£ nguá»“n C# toÃ n dá»± Ã¡n sau tÃ¡i cáº¥u trÃºc 14 thÆ° má»¥c AI | **0 Lá»–I (Build Succeeded)** | 53 file C# AI Ä‘Æ°á»£c báº£o toÃ n nguyÃªn váº¹n |
| **Kiá»ƒm tra dá»¯ liá»‡u Qdrant** | HTTP GET `/collections/hrms_vectors_v2` | Kiá»ƒm tra sá»‘ lÆ°á»£ng point vector vÃ  7 index trÆ°á»ng payload | **197 points nhÃ¢n sá»±, 0 point quy cháº¿** | Cáº§n quy trÃ¬nh náº¡p quy cháº¿ Ä‘Ã£ duyá»‡t |

---

## 14. Triá»ƒn khai vÃ  váº­n hÃ nh

- **MÃ´i trÆ°á»ng Backend:** MÃ¡y chá»§ Windows Server 2019/2022 cÃ i Ä‘áº·t IIS 10, cáº¥u hÃ¬nh Application Pool cháº¿ Ä‘á»™ `.NET CLR Version v4.0.30319`, Pipeline Mode: *Integrated*.
- **MÃ´i trÆ°á»ng Web:** BiÃªn dá»‹ch tÄ©nh qua `npm run build` sinh thÆ° má»¥c `dist/`, triá»ƒn khai trÃªn IIS (kÃ¨m URL Rewrite) hoáº·c Nginx lÃ m reverse proxy.
- **Biáº¿n mÃ´i trÆ°á»ng vÃ  Cáº¥u hÃ¬nh báº£o máº­t:**
  - `HRMS_JWT_SECRET`: KhÃ³a bÃ­ máº­t kÃ½ token JWT (tá»‘i thiá»ƒu 32 kÃ½ tá»±, Ä‘áº·t trong biáº¿n mÃ´i trÆ°á»ng mÃ¡y chá»§).
  - Chuá»—i káº¿t ná»‘i Oracle Ä‘áº·t trong file cáº¥u hÃ¬nh mÃ¡y chá»§, mÃ£ hÃ³a báº±ng `aspnet_regiis` khi Ä‘Æ°a lÃªn production.
  - Cáº¥u hÃ¬nh CORS: Giá»›i háº¡n chÃ­nh xÃ¡c domain cá»§a cá»•ng Web quáº£n trá»‹, khÃ´ng dÃ¹ng wildcard `*`.
- **Dá»¯ liá»‡u sao lÆ°u:**
  - Äá»‹nh ká»³ sao lÆ°u Oracle Database qua tiá»‡n Ã­ch `expdp` (tá»‡p `HR_BACKUP.DMP`).
  - Äá»‹nh ká»³ táº¡o snapshot dá»¯ liá»‡u Qdrant qua API `POST /collections/{name}/snapshots`.

---

## 15. KhÃ³ khÄƒn vÃ  giá»›i háº¡n hiá»‡n táº¡i

1. **Kiáº¿n trÃºc cÃ´ng nghá»‡ khÃ´ng Ä‘á»“ng nháº¥t:** Há»‡ thá»‘ng káº¿t há»£p giá»¯a .NET Framework 4.7.2 cÅ© (WinForms, Web API 2) vÃ  ngÄƒn xáº¿p hiá»‡n Ä‘áº¡i (React 19, TypeScript, Expo 57). KhÃ´ng cÃ³ má»™t cÃ´ng cá»¥ build duy nháº¥t cho toÃ n bá»™ há»‡ thá»‘ng; viá»‡c thiáº¿t láº­p mÃ´i trÆ°á»ng má»›i Ä‘Ã²i há»i cÃ i Ä‘áº·t cáº£ Visual Studio, DevExpress vÃ  Node.js.
2. **Khá»Ÿi phÃ¡t tÃ­nh lÆ°Æ¡ng phá»¥ thuá»™c mÃ¡y tráº¡m Desktop:** Do toÃ n bá»™ engine tÃ­nh lÆ°Æ¡ng sÃ¢u vÃ  cÃ¡c biá»ƒu máº«u bÃ¡o cÃ¡o DevExpress gáº¯n liá»n vá»›i `HRMS.Desktop`, viá»‡c tÃ­nh lÆ°Æ¡ng hiá»‡n táº¡i báº¯t buá»™c pháº£i thá»±c hiá»‡n trÃªn mÃ¡y tÃ­nh cÃ i Ä‘áº·t Windows cÃ³ báº£n quyá»n DevExpress.
3. **Cáº¥u hÃ¬nh phÃ¢n tÃ¡n:** CÃ¡c tham sá»‘ káº¿t ná»‘i náº±m ráº£i rÃ¡c á»Ÿ `Web.config`, `App.config`, `.env` vÃ  báº£ng `TB_CONFIG` trong CSDL, Ä‘Ã²i há»i quáº£n trá»‹ viÃªn pháº£i kiá»ƒm tra ká»¹ lÆ°á»¡ng khi Ä‘á»•i Ä‘á»‹a chá»‰ mÃ¡y chá»§.
4. **Kho tÃ i liá»‡u vector quy cháº¿ cÃ²n thiáº¿u:** Dá»¯ liá»‡u vector hiá»‡n táº¡i má»›i chá»‰ pháº£n Ã¡nh thÃ´ng tin nhÃ¢n sá»± (`tag=EMPLOYEE`), chÆ°a cÃ³ kho vÄƒn báº£n quy Ä‘á»‹nh, quy cháº¿ cÃ´ng ty chÃ­nh thá»©c (`tag=REGULATION=0`). AI sáº½ tá»« chá»‘i tráº£ lá»i hoáº·c bÃ¡o thiáº¿u nguá»“n khi há»i sÃ¢u vá» chÃ­nh sÃ¡ch ná»™i bá»™.
5. **Äá»“ng bá»™ sá»± kiá»‡n vector chÆ°a hoÃ n toÃ n bá»n vá»¯ng (Outbox):** CÆ¡ cháº¿ outbox hiá»‡n táº¡i cÃ²n dá»±a trÃªn bá»™ nhá»› RAM vÃ  hÃ ng Ä‘á»£i ná»™i bá»™; khi á»©ng dá»¥ng khá»Ÿi Ä‘á»™ng láº¡i Ä‘á»™t ngá»™t, má»™t sá»‘ sá»± kiá»‡n thay Ä‘á»•i dá»¯ liá»‡u nhÃ¢n sá»± cÃ³ thá»ƒ cáº§n lá»‡nh Ä‘á»“ng bá»™ thá»§ cÃ´ng (`HRMS.VectorDataSync --reconcile`).
6. **Má»©c Ä‘á»™ bao phá»§ Ä‘a ngÃ´n ngá»¯:** DÃ¹ cá»•ng Web Ä‘Ã£ cÃ³ tá»‡p ngÃ´n ngá»¯ hoÃ n chá»‰nh (Viá»‡t, Anh, Nháº­t, HÃ n, Trung), giao diá»‡n Desktop WinForms váº«n hiá»ƒn thá»‹ tiáº¿ng Viá»‡t lÃ  chá»§ Ä‘áº¡o; má»™t sá»‘ nhÃ£n chá»©c nÄƒng trong cÆ¡ sá»Ÿ dá»¯ liá»‡u cÅ© cÃ²n tá»“n táº¡i lá»—i mÃ£ hÃ³a kÃ½ tá»±.

---

## 16. HÆ°á»›ng phÃ¡t triá»ƒn

| Thá»© tá»± Æ°u tiÃªn | HÆ°á»›ng phÃ¡t triá»ƒn | Má»¥c tiÃªu & GiÃ¡ trá»‹ mang láº¡i | Äiá»u kiá»‡n nghiá»‡m thu |
| :---: | :--- | :--- | :--- |
| **P1** | **Sá»‘ hÃ³a & náº¡p kho vÄƒn báº£n quy cháº¿ (Approved Corpus Ingestion)** | Náº¡p tÃ i liá»‡u ná»™i bá»™ (ná»™i quy, quy cháº¿ lÆ°Æ¡ng, quy Ä‘á»‹nh phÃ©p) vÃ o Qdrant Ä‘á»ƒ AI tráº£ lá»i chÃ­nh xÃ¡c quy Ä‘á»‹nh. | Äiá»ƒm vector `tag=REGULATION` > 0; cÃ¢u há»i quy cháº¿ cÃ³ citation nguá»“n há»£p lá»‡. |
| **P1** | **HoÃ n thiá»‡n bá»n vá»¯ng hÃ³a Outbox (Durable Outbox)** | LÆ°u cÃ¡c sá»± kiá»‡n thay Ä‘á»•i nhÃ¢n sá»± vÃ o báº£ng outbox Oracle trong cÃ¹ng Transaction nghiá»‡p vá»¥. | Worker tá»± Ä‘á»™ng khÃ´i phá»¥c vÃ  Ä‘á»“ng bá»™ láº¡i vector sau khi tiáº¿n trÃ¬nh khá»Ÿi Ä‘á»™ng láº¡i mÃ  khÃ´ng máº¥t sá»± kiá»‡n. |
| **P2** | **Cá»•ng API kÃ­ch hoáº¡t tÃ­nh lÆ°Æ¡ng an toÃ n** | NghiÃªn cá»©u Ä‘Ã³ng gÃ³i `PayrollEngine` thÃ nh dá»‹ch vá»¥ Ä‘á»™c láº­p Ä‘á»ƒ Web cÃ³ thá»ƒ kÃ­ch hoáº¡t tÃ­nh lÆ°Æ¡ng cÃ³ giÃ¡m sÃ¡t. | CÃ³ endpoint POST tÃ­nh lÆ°Æ¡ng Ä‘Æ°á»£c kiá»ƒm soÃ¡t quyá»n cháº·t cháº½; audit Ä‘áº§y Ä‘á»§ vÃ  khÃ³a tranh cháº¥p ká»³ cÃ´ng. |
| **P2** | **Chuáº©n hÃ³a Ä‘a ngÃ´n ngá»¯ toÃ n diá»‡n trÃªn Desktop** | HoÃ n thiá»‡n cÆ¡ cháº¿ náº¡p resource Ä‘a ngÃ´n ngá»¯ trÃªn cÃ¡c form Desktop vÃ  lÃ m sáº¡ch dá»¯ liá»‡u mÃ£ hÃ³a nhÃ£n cÅ©. | Chuyá»ƒn Ä‘á»•i ngÃ´n ngá»¯ trÃªn Desktop khÃ´ng phÃ¡t sinh lá»—i hiá»ƒn thá»‹ kÃ½ tá»± (mojibake). |
| **P3** | **ÄÃ¡nh giÃ¡ hiá»‡n Ä‘áº¡i hÃ³a ná»n táº£ng (.NET Core / .NET 9)** | ÄÃ¡nh giÃ¡ tÃ­nh kháº£ thi chuyá»ƒn Ä‘á»•i backend ASP.NET Web API 2 sang ASP.NET Core Ä‘á»ƒ cháº¡y Ä‘Æ°á»£c trÃªn Linux container. | BÃ¡o cÃ¡o Ä‘Ã¡nh giÃ¡ tÆ°Æ¡ng thÃ­ch, káº¿ hoáº¡ch tÃ¡i cáº¥u trÃºc EF sang EF Core vÃ  lá»™ trÃ¬nh thá»±c thi khÃ´ng lÃ m giÃ¡n Ä‘oáº¡n há»‡ thá»‘ng. |

---

## 17. TÃ i liá»‡u liÃªn quan

Táº¥t cáº£ cÃ¡c tÃ i liá»‡u ká»¹ thuáº­t chi tiáº¿t Ä‘Æ°á»£c lÆ°u trá»¯ trong thÆ° má»¥c [docs/](docs/):
- [Má»¥c lá»¥c tÃ i liá»‡u](docs/README.md): Tá»•ng quan toÃ n bá»™ há»‡ thá»‘ng tÃ i liá»‡u.
- [Sá»• tay Kiáº¿n trÃºc & Váº­n hÃ nh AI Services](docs/ai-services-guide.md): Báº£n Ä‘á»“ chi tiáº¿t 53 file C# AI trong 14 thÆ° má»¥c chá»©c nÄƒng, sÆ¡ Ä‘á»“ vÃ  hÆ°á»›ng dáº«n má»Ÿ rá»™ng.
- [HÆ°á»›ng dáº«n RAG & PhÃ¢n quyá»n ná»n táº£ng](docs/ai-rag-and-account-permissions-guide.md): Chi tiáº¿t cÆ¡ cháº¿ phÃ¢n quyá»n 3 kÃªnh vÃ  an toÃ n AI.
- [HÆ°á»›ng dáº«n CÃ i Ä‘áº·t & Khá»Ÿi cháº¡y](docs/installation.md): Chi tiáº¿t cÃ¡c bÆ°á»›c thiáº¿t láº­p mÃ´i trÆ°á»ng cho láº­p trÃ¬nh viÃªn má»›i.
- [Kiáº¿n trÃºc há»‡ thá»‘ng](docs/architecture.md): Tham kháº£o cáº¥u trÃºc cÃ¡c táº§ng vÃ  luá»“ng dá»¯ liá»‡u.
- [Hiá»‡n tráº¡ng há»‡ thá»‘ng](docs/current-status.md): ÄÃ¡nh giÃ¡ hiá»‡n tráº¡ng vÃ  cÃ¡c Ä‘iá»ƒm cáº§n xá»­ lÃ½ ká»¹ thuáº­t.
- [Quy trÃ¬nh CÃ´ng â€“ LÆ°Æ¡ng](docs/payroll.md): Diá»…n giáº£i cÃ´ng thá»©c, chÃ­nh sÃ¡ch vÃ  Ä‘á»‘i soÃ¡t dá»¯ liá»‡u lÆ°Æ¡ng.
- [HÆ°á»›ng dáº«n á»¨ng dá»¥ng Di Ä‘á»™ng](docs/mobile.md): Thiáº¿t láº­p, cáº¥u hÃ¬nh API vÃ  Ä‘Ã³ng gÃ³i Mobile Expo.
- [ThÆ° má»¥c LÆ°u trá»¯ Lá»‹ch sá»­](docs/archive/): LÆ°u trá»¯ cÃ¡c bÃ¡o cÃ¡o kiá»ƒm thá»­ vÃ  tÃ i liá»‡u thiáº¿t káº¿ cÃ¡c giai Ä‘oáº¡n trÆ°á»›c.

---

## 18. Báº£o trÃ¬ tÃ i liá»‡u vÃ  giáº¥y phÃ©p

### 18.1. Quy táº¯c Ä‘á»“ng bá»™ 3 ngÃ´n ngá»¯
TÃ i liá»‡u hÆ°á»›ng dáº«n táº¡i thÆ° má»¥c gá»‘c repository Ä‘Æ°á»£c duy trÃ¬ Ä‘á»“ng bá»™ á»Ÿ ba phiÃªn báº£n ngÃ´n ngá»¯:
- [Tiáº¿ng Viá»‡t (README.md)](README.md) - TÃ i liá»‡u tham chiáº¿u gá»‘c.
- [English (README.en.md)](README.en.md) - PhiÃªn báº£n Tiáº¿ng Anh vá»›i cÃ¹ng cáº¥u trÃºc 18 má»¥c vÃ  báº£ng ká»¹ thuáº­t tÆ°Æ¡ng Ä‘Æ°Æ¡ng.
- [æ—¥æœ¬èªž (README.ja.md)](README.ja.md) - PhiÃªn báº£n Tiáº¿ng Nháº­t sá»­ dá»¥ng thuáº­t ngá»¯ nghiá»‡p vá»¥ tá»± nhiÃªn, giá»¯ nguyÃªn bá»‘i cáº£nh luáº­t lao Ä‘á»™ng vÃ  báº£o hiá»ƒm Viá»‡t Nam.

Má»i thay Ä‘á»•i lá»›n vá» kiáº¿n trÃºc, chá»©c nÄƒng hoáº·c káº¿t quáº£ kiá»ƒm thá»­ báº¯t buá»™c pháº£i Ä‘Æ°á»£c cáº­p nháº­t Ä‘á»“ng thá»i trÃªn cáº£ ba phiÃªn báº£n.

### 18.2. Báº£n quyá»n vÃ  Giáº¥y phÃ©p
MÃ£ nguá»“n dá»± Ã¡n lÃ  sáº£n pháº©m pháº§n má»m ná»™i bá»™ doanh nghiá»‡p. Há»‡ thá»‘ng cÃ³ sá»­ dá»¥ng cÃ¡c thÆ° viá»‡n mÃ£ nguá»“n má»Ÿ theo giáº¥y phÃ©p tÆ°Æ¡ng á»©ng (MIT, Apache 2.0) vÃ  thÃ nh pháº§n giao diá»‡n thÆ°Æ¡ng máº¡i DevExpress (yÃªu cáº§u giáº¥y phÃ©p thÆ°Æ¡ng máº¡i há»£p lá»‡ khi triá»ƒn khai sáº£n xuáº¥t). KhÃ´ng tá»± Ã½ sao chÃ©p hoáº·c phÃ¢n phá»‘i mÃ£ nguá»“n ra bÃªn ngoÃ i khi chÆ°a Ä‘Æ°á»£c sá»± cho phÃ©p báº±ng vÄƒn báº£n cá»§a chá»§ sá»Ÿ há»¯u dá»± Ã¡n.
