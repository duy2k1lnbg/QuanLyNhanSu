# Kế hoạch cho Antigravity IDE: chuyển TryHardAgain sang giao diện rừng xanh

Ngày lập: 02/10/2026. Đặc tả này dựa trên hai ảnh người dùng cung cấp và mã nguồn thực tế tại D:/QL_NS/QuanLyNhanSu.

## 1. Mục tiêu và quyết định triển khai

Chuyển landing page HRMS Enterprise sang phong cách cá nhân TryHardAgain theo ảnh số 2: rừng xanh, ánh nắng ấm, chữ serif màu kem, khoảng trắng thoáng và các phần nối tiếp tự nhiên.

Điểm nhận diện bắt buộc: **mỗi slide một không khí**, chỉ có **một hiệu ứng môi trường chính** trên mỗi slide. Chuyển màu nhấn và thanh thời gian carousel hỗ trợ hiệu ứng đó.

Ảnh tham chiếu:

- UI hiện tại: C:/Users/duyth/AppData/Local/Temp/codex-clipboard-8c2619c0-6a49-4f6b-9344-424d77cc403e.png.
- UI đích: D:/User/Downloads/Website cá nhân giữa rừng xanh.png.
- Dùng ảnh số 2 để tham chiếu bố cục, tỷ lệ, chất liệu và ánh sáng. Xây từng phần bằng HTML/CSS; không dùng nguyên ảnh chụp làm nền toàn trang.

Phạm vi: landing page trong HRMS.Web. Tái sử dụng đăng nhập, tải ứng dụng, thông tin tác giả, đa ngôn ngữ và theme hiện có. Giữ nguyên nghiệp vụ nhân sự, chấm công, tiền lương, API và cơ sở dữ liệu.

Các quyết định mặc định:

- Giữ đủ 10 slide, thứ tự và câu chữ hiện có. Mở trang ở hero-05 — PATIENCE; bộ đếm là 05 / 10.
- Thay typewriter bằng hiển thị trọn câu và fade nhẹ để nội dung đọc được ngay.
- Theme tối là xanh rừng như ảnh mẫu; theme sáng là kem và xanh sage. Đọc lựa chọn từ useAppTheme, giữ quy tắc lưu hiện tại.
- Khung tác giả mặc định dùng monogram ND như ảnh mẫu; hỗ trợ cấu hình dùng ảnh thật AUTHOR_INFO.avatarUrl.
- Âm thanh tắt mỗi lần tải trang; chỉ phát sau thao tác bật trực tiếp.
- Sau thao tác chọn/kéo/đổi slide bằng tay, dừng autoplay cho đến khi bấm “Tiếp tục”.

## 2. Khảo sát code và các điểm cần chú ý

Các đường dẫn trong bảng là tương đối với D:/QL_NS/QuanLyNhanSu.

| Thành phần | File hiện tại | Cách sử dụng |
| --- | --- | --- |
| Stack React 19, TypeScript, Vite, Ant Design | HRMS.Web/package.json | Giữ stack hiện có, ưu tiên CSS/SVG/API trình duyệt |
| Trang landing và state modal | HRMS.Web/src/pages/Login.tsx | Ghép bố cục mới, giữ callback đăng nhập và tải |
| Header | HRMS.Web/src/pages/login/components/LandingHeader.tsx | Đổi hình thức, thêm Về mình/menu mobile |
| Carousel 10 slide | HRMS.Web/src/components/CinematicHeroGallery.tsx | Tái sử dụng dữ liệu; thay controller và giao diện |
| CSS gallery | HRMS.Web/src/components/CinematicHeroGallery.css | Thay hero tối mạnh, coverflow 3D và tỷ lệ cũ |
| Ảnh slide | HRMS.Web/public/images/inspirational/hero-01.jpg đến hero-10.jpg | Tạo bản responsive và thumbnail riêng |
| Tác giả/link tải | HRMS.Web/src/pages/login/types.ts | Nguồn dữ liệu chính cho email, Facebook, GitHub, avatar, tải |
| Đăng nhập/tải | HRMS.Web/src/pages/login/components/LoginModal.tsx, DownloadModal.tsx | Giữ logic và các callback hiện tại |
| Theme | HRMS.Web/src/theme/ThemeContext.tsx | Dùng useAppTheme và data-theme; không tạo provider theme thứ hai |
| Ngôn ngữ | HRMS.Web/src/locales/{vi,en,ja,ko,zh-CN}.ts | Thêm nội dung mới cho cả 5 ngôn ngữ |
| Reveal | HRMS.Web/src/components/ScrollReveal.tsx, src/hooks/useScrollReveal.ts | Tái sử dụng, sửa phụ thuộc intro cho landing |
| Màn chào | HRMS.Web/src/App.tsx, src/hooks/useWelcomeIntro.ts | Hiện khoảng 4,5 giây, khóa cuộn; cần bỏ ở nhánh landing |
| Kiểm tra | HRMS.Web/tests và scripts build/lint/test | Chạy baseline và kiểm tra sau thay đổi |

Các phát hiện cần đưa vào implementation:

1. Dữ liệu thực tế có 10 slide, dù một số comment/CSS ghi 9. Bộ đếm, dots và loop lấy từ slides.length.
2. 10 JPG chính hiện khoảng 644 KB–1.033 KB/ảnh, tổng khoảng 8,1 MB. Không tải đồng thời tất cả ảnh nền lớn khi mở trang.
3. hero-05.jpg là nụ hoa giữa nền xanh; khác bông hoa nở và tia nắng trong ảnh mẫu. Có thể dùng để dựng trước, nhưng cần asset phù hợp để đạt mức tương đồng cao.
4. useScrollReveal kiểm tra isIntroActiveGlobal(). Chỉ bỏ WelcomeIntro sẽ có thể làm các section vẫn chờ một sự kiện không được phát.
5. App.tsx hiện dùng hash route dạng #/dashboard. Anchor landing không được đổi hash thành #about.
6. Working tree đang có nhiều thay đổi, gồm App.tsx. Đọc diff trước khi sửa, bảo toàn các thay đổi đó; không reset/checkout ghi đè.

## 3. Bố cục và nội dung đích

Thứ tự trang: **Header + Hero → Về mình → Những điều đang tập trung → Dự án HRMS → Kết nối → Footer**.

### 3.1. Header

- Header trên hero, nền trong suốt ở đầu trang; cuộn xuống thì nền xanh rừng bán trong suốt, blur nhẹ và đường chia mảnh.
- Brand “HRMS Enterprise” bằng serif; cạnh đó chấm xanh và nhãn trạng thái hiện có. Không phát sinh cơ chế kiểm tra API mới chỉ để phục vụ UI.
- Menu: Gallery Phim, Về mình, Tải ứng dụng, lựa chọn ngôn ngữ, theme, âm thanh, Đăng nhập.
- “TryHardAgain” xuất hiện ở dòng phụ hero và footer, tạo nhận diện cá nhân.
- Desktop cao khoảng 64px; mobile khoảng 56px.
- Mobile giữ brand và đăng nhập, các mục phụ vào menu; vẫn truy cập được ngôn ngữ/theme/audio/tải.
- Anchor dùng cuộn trực tiếp tới element, với scroll-margin-top. Không sửa hash route.
- Không đặt overflow: hidden lên vùng header khiến dropdown bị cắt.

### 3.2. Hero và thumbnail

- Ảnh toàn chiều rộng, chủ thể về bên phải, vùng chữ bên trái đủ tối nhưng vẫn nhìn rõ rừng.
- Chiều cao desktop khởi điểm clamp(400px, 37vw, 620px), hiệu chỉnh qua ảnh so sánh. Mobile cao theo nội dung.
- Cụm chữ và thumbnail cùng nằm trong flow của khối nội dung, tránh khoảng trống lớn giữa hai cụm.
- Nhãn PATIENCE chữ nhỏ, tracking rộng, cạnh có nét ngang mảnh.
- Quote serif màu kem, chữ đứng, cỡ khoảng 40–58px desktop, 30–38px mobile.
- Câu dài và tiếng Nhật có biến thể cỡ chữ nhỏ hơn; hero được nở theo nội dung, không cắt quote hoặc ellipsis.
- Dòng phụ: “TryHardAgain — một góc nhỏ để nhắc mình tiếp tục.”
- Thay coverflow 3D bằng hàng 5 thumbnail phẳng trên desktop, quanh slide đang chọn; mỗi ảnh khoảng 74×52 đến 96×64px.
- Nút trước/sau tròn, viền kem mảnh; dots và bộ đếm bên dưới; timeline 2px riêng.
- Có nút Tạm dừng/Tiếp tục và Âm thanh dễ thấy; không đặt trùm lên quote.
- Mobile hiển thị khoảng 3 thumbnail, track cuộn ngang. Có thể ẩn dots để ưu tiên bộ đếm và nút điều khiển lớn.
- Giảm vignette hiện tại; dùng gradient theo focal point để không làm toàn ảnh gần như đen.

### 3.3. Về mình

- Nền xanh rừng trầm, hai mép có lá/bóng lá rất mờ.
- Desktop hai cột: khung tác giả trái, giới thiệu phải; mobile một cột.
- Nội dung: “Chào bạn, mình là Duy.”, Nguyễn Thọ Duy, hai đoạn ngắn về C# .NET, React, hệ thống nhân sự, kết nối dữ liệu/nghiệp vụ/trải nghiệm.
- Dòng kỹ năng: C# .NET · React · Cơ sở dữ liệu.
- Khung tỷ lệ khoảng 4:5, chất liệu sage và monogram ND; tạo monogram bằng SVG/text.
- Cấu hình hỗ trợ ảnh /myavt.png. Nếu dùng ảnh thật, crop giữ khuôn mặt, không kéo giãn ảnh dọc.

### 3.4. Những điều mình đang tập trung

- Tiêu đề giữa trang, dưới có đường ngang ngắn.
- Ba mục: 01 · Hiểu nghiệp vụ; 02 · Xây dựng hệ thống; 03 · Cải thiện trải nghiệm.
- Icon nét mảnh: lá, kết nối, khiên; stroke thống nhất.
- Desktop ba cột có đường chia dọc; mobile xếp dọc, chia ngang.
- Nội dung ngắn theo ảnh mẫu; chuyển khỏi lưới 6 thẻ tính năng dày đặc của landing cũ.

### 3.5. Dự án đang phát triển

- Nền rừng toàn chiều rộng; nội dung trái, mockup desktop/mobile phải.
- Tiêu đề “Hệ thống quản lý nhân sự”; mô tả hồ sơ, chấm công, tiền lương trên Desktop/Web/Mobile.
- CTA Xem GitHub dùng AUTHOR_INFO.github; Đăng nhập dùng callback hiện có; Tải ứng dụng mở DownloadModal.
- Dùng screenshot HRMS đã loại thông tin riêng tư trong frame thiết bị bằng HTML/CSS hoặc asset riêng.
- Các nút và chữ là thành phần thật, không nhúng vào ảnh.
- Mobile chữ trước, mockup sau, CTA wrap; Windows/Android vẫn được chọn trong modal.

### 3.6. Kết nối và footer

- Nền rừng/suối tối; dương xỉ hai mép, vùng giữa sạch cho nội dung.
- Tiêu đề “Kết nối & trao đổi”; mô tả ngắn.
- Email, Facebook, GitHub là các hàng icon trái, thông tin giữa, mũi tên phải, phân cách 1px.
- Email lấy từ AUTHOR_INFO.email; click email mở mailto:, button copy riêng.
- Facebook giữ URL hiện có. Hàng GitHub liên hệ dùng profile tác giả, CTA dự án dùng repository; thêm githubProfile riêng nếu cần.
- Footer “TryHardAgain · Nguyễn Thọ Duy”, có thể thêm năm hiện hành.

## 4. Thiết kế màu, chữ và khoảng cách

| Token | Theme tối | Theme sáng | Công dụng |
| --- | --- | --- | --- |
| --landing-bg | #14211B | #F3F0E5 | Nền chính |
| --landing-surface | #1C2C23 | #E6EBDD | Khối nội dung |
| --landing-text | #F4EEDC | #26382C | Chữ chính |
| --landing-muted | #C5CCBF | #52624E | Chữ phụ |
| --landing-border | rgba(239,232,212,.20) | rgba(48,70,49,.22) | Đường chia |
| --slide-accent | Theo slide | Theo slide | Thumbnail, nút, timeline |

Đây là màu khởi điểm, cần đo tương phản trên ảnh thật. Đổi slide chỉ đổi accent và hero, không đổi toàn bộ nền/chữ các section.

- Scope toàn bộ CSS dưới .tryhard-landing; không override toàn app các selector .ant-btn, .ant-card, h1, section.
- Modal/dropdown dùng portal cần class riêng hoặc theme cục bộ; chúng có thể nằm ngoài wrapper và không nhận variables của landing.
- Serif ưu tiên Lora hiện có, weight 500–600, chữ đứng. Body dùng system sans hiện có, 16–18px, line-height 1.6–1.7.
- Nếu tự host font, giữ bộ ký tự tiếng Việt, license và font-display: swap. Kiểm tra fallback tiếng Nhật.
- Container khoảng 1120–1200px; padding ngang clamp(20px, 6vw, 88px); khoảng section 48–80px.
- Bo góc 4–8px, border mảnh, shadow nhẹ. Bỏ glow xanh điện, gradient tím và shadow dày của UI cũ.
- Ảnh hero giữ chữ kem có nền bảo đảm tương phản ở cả hai theme; theme sáng chủ yếu đổi các section/controls.

## 5. Mỗi slide một không khí

Tách dữ liệu slide ra file riêng. Bổ sung environment, accent, accentStrong, focalPoint, durationMs, intensity và nguồn ảnh responsive. environment là một giá trị đơn, không phải mảng hiệu ứng.

| Slide | Môi trường chính | Cách thể hiện | Accent tham chiếu | Thời gian đọc |
| --- | --- | --- | --- | --- |
| 01 COMEBACK | Ánh bình minh | Gradient sáng lên nhẹ rồi giữ, không chớp | #CCA776 | 8 giây |
| 02 START AGAIN | Ánh đèn bàn | Vùng sáng ấm gần đèn, gần như tĩnh; không hạt/sương | #CBA36A | 8 giây |
| 03 SELF ACCEPTANCE | Bụi nắng | 8–12 hạt nhỏ trong vùng cửa sổ, tốc độ chậm | #D5C19A | 8 giây |
| 04 FORGIVE THE PAST | Sương | Hai dải mờ cùng loại hiệu ứng, trôi ngang 25–40 giây | #AAC0B0 | 10 giây |
| 05 PATIENCE | Bụi nắng | 10–16 hạt 1–3px trong vùng nắng, ánh ấm tĩnh trong ảnh | #DCC58E | 8 giây |
| 06 THE CHOSEN PATH | Mây/sương xa | Một dải gần chân trời, tránh vùng chữ | #AFB793 | 8 giây |
| 07 DECISION | Quầng đèn | Glow tĩnh nhẹ quanh ánh đèn thành phố | #C3AE86 | 12 giây |
| 08 MOVE FORWARD | Mưa | 8–14 vệt 1px, xiên nhẹ, opacity thấp | #A8BAC2 | 10 giây |
| 09 WHY NOT YOU? | Ánh chân trời | Gradient ấm hiện nhẹ, không hạt | #D9B486 | 14 giây |
| 10 TRY HARD AGAIN | Ánh bình minh | Tăng nhẹ trong 1,5–2 giây rồi giữ ổn định | #D9C28A | 12 giây |

5 slide người dùng chỉ định là yêu cầu chính. Mapping cho các slide khác là đề xuất để giữ đủ dữ liệu; đối chiếu từng ảnh thực tế khi chỉnh vị trí ánh sáng. Thời gian đọc là cấu hình khởi điểm, cần kiểm tra cả quoteSub/câu dài.

Quy tắc môi trường:

- SlideAtmosphere chỉ mount đúng một loại environment.
- Hai dải sương vẫn thuộc một môi trường. PATIENCE không thêm animation bình minh bên cạnh bụi nắng.
- Khi đổi slide, effect cũ fade về 0 khoảng 150–200ms rồi unmount; effect mới hiện sau đó. Không giữ hai hệ hạt cùng lúc.
- Bóng lá chỉ ở About/mép section, không phủ thêm lên hero.
- Các lớp môi trường dùng pointer-events: none, aria-hidden=true và nằm sau vùng chữ/controls.
- Mask giảm mật độ/opacity ở vùng chữ. Dùng transform/opacity cho hạt, mưa; sương dùng gradient/asset mờ sẵn.
- Tránh blur động toàn màn hình, flicker đèn và exposure mạnh làm mất chữ.
- Mưa chỉ dùng vệt mảnh; không thêm giọt nước trên kính ở cùng slide.
- Mobile giảm số hạt/vệt khoảng 50%; opacity môi trường tham chiếu 0.05–0.18.

Accent đổi trong 400–600ms trên border-color/background-color/color thật. Không chỉ transition một custom property chưa đăng ký rồi kỳ vọng màu tự nội suy. Foreground của button có biến thể được kiểm tra tương phản theo theme/slide.

## 6. Controller carousel và thanh thời gian

### 6.1. Một nguồn thời gian

- Bỏ timer phụ thuộc typewriter.
- durationMs điều khiển chu kỳ; bắt đầu sau khi ảnh sẵn sàng và intro ngắn kết thúc.
- Một controller dùng performance.now() cho autoplay và progress.
- Thời gian đã chạy/điểm bắt đầu lưu bằng ref; không cập nhật React state 60 lần/giây.
- Timeline dùng transform: scaleX(progress), origin trái, cập nhật bằng RAF/ref hoặc CSS variable.
- Đạt 100% chỉ commit chuyển slide một lần.
- Đổi slide tay: reset progress và dừng; bấm Tiếp tục bắt đầu chu kỳ đầy đủ.
- Pause tạm rồi resume cùng slide: giữ thời gian còn lại, không reset về 0.
- Ảnh kế tiếp chưa decode: giữ ảnh hiện tại, không chuyển sang khung trắng; tải lỗi có fallback và không làm điều hướng kẹt.

### 6.2. Quy tắc pause

| Sự kiện | Hành vi |
| --- | --- |
| Hover hero/controls trên thiết bị có hover | Pause tạm, rời vùng thì tiếp tục nếu không còn lý do dừng |
| Focus bàn phím vào carousel | Pause để đọc/điều hướng; tiếp tục bằng nút rõ ràng |
| Thumbnail/dots/trước/sau/phím trái phải | Chuyển theo yêu cầu, reset, dừng đến khi bấm Tiếp tục |
| Bắt đầu drag | Dừng ngay; commit thì giữ dừng; hủy thì khôi phục trạng thái trước nếu được phép |
| Bấm Tạm dừng | Giữ progress và dừng chủ động |
| Mở modal/menu tương tác | Pause theo lý do riêng |
| Hero ngoài viewport hoặc tab ẩn | Dừng timer/effect, giữ thời gian còn lại |
| prefers-reduced-motion | Autoplay mặc định dừng; điều hướng tay vẫn dùng được |

Dùng tập lý do dừng: hover, focus, drag, modal, documentHidden, offscreen, cộng trạng thái dừng chủ động. Không dùng một boolean để mouseleave vô tình ghi đè modal/pause tay. Nút Tiếp tục là thao tác explicit cho phép khởi chạy khi focus đang trên chính nút đó; không để focus khóa trạng thái phát vô hạn.

### 6.3. Kéo theo tay

- Pointer Events dùng chung chuột/touch/pen; touch-action: pan-y để giữ cuộn dọc.
- Sau khoảng 8px xác định hướng; chỉ capture pointer khi kéo ngang rõ hơn dọc.
- Render current và hai slide liền kề trong panel có translateX; vị trí cập nhật liên tục theo deltaX.
- Không chờ pointerup rồi chỉ crossfade: ảnh phải thật sự đi theo tay.
- Ngưỡng commit tham chiếu: 20% bề rộng, cap khoảng 100px, hoặc vận tốc ngang > 0,45px/ms với khoảng kéo tối thiểu hợp lý.
- Settle 280–420ms về slide mới hoặc ảnh cũ; mỗi gesture tối đa một slide.
- Loop 10→1 và 1→10 không nhảy vị trí.
- Khóa autoplay khi drag. Nếu drag bắt đầu giữa transition, chuẩn hóa frame hiện tại rồi nhận gesture.
- Xử lý pointercancel, lostpointercapture và resize; reset an toàn.
- Chặn click sau drag thật; không bắt drag từ button/link/input/vùng chữ đang chọn.
- Chỉ vùng ảnh/track dùng user-select: none, không khóa chọn quote toàn hero.
- Tách wrapper drag, parallax và scale ảnh để không ghi đè cùng transform.
- Quote giữ ổn định lúc kéo; cập nhật quote/accent/environment sau commit và fade chữ ngắn.

## 7. Các hiệu ứng và tiện ích còn lại

### 7.1. Mở trang dưới một giây

| Timeline | Thành phần |
| --- | --- |
| 0–240ms | Nền fallback/ảnh hero xuất hiện |
| 120–440ms | Header và nhãn slide |
| 220–660ms | Toàn bộ quote và dòng phụ; translateY tối đa 8px |
| 380–820ms | Thumbnail/controls; stagger khoảng 35–45ms |
| 820–900ms | Bắt đầu carousel nếu không bị pause |

- Tổng animation vào trang ≤ 900ms. Đây là thời lượng animation, không phải cam kết tải mạng dưới 1 giây.
- Nền fallback xuất hiện ngay, chữ/nút hoạt động nếu ảnh/font chưa tải xong.
- Bỏ WelcomeIntro ở nhánh chưa đăng nhập trong App.tsx; giữ scope hành vi nhánh quản trị hiện tại.
- Thêm waitForIntro?: boolean vào useScrollReveal/ScrollReveal, mặc định true. Landing truyền false.
- Khi false: reveal theo viewport/focus, không chờ global intro hoặc sự kiện hoàn tất.
- Không gọi notifyIntroFinished chỉ để bỏ intro landing, vì hàm đó đánh dấu session đã xem và có thể ảnh hưởng intro quản trị.
- Reduced motion hiện ngay, không stagger/delay/scroll lock.

### 7.2. Chiều sâu khi cuộn

- Một listener scroll thụ động và một RAF dùng chung.
- Background hero/project dịch khoảng 12–36px; lá mép dịch 6–16px với tốc độ khác.
- Tính theo tiến độ local của section, clamp hai đầu; không nhân scrollY không giới hạn.
- Nội dung/controls ở flow bình thường; ảnh có overscan đủ để không lộ mép.
- Tắt với reduced motion; mobile chỉ giữ một lớp nhẹ nếu phù hợp.
- Section ngoài viewport không tiếp tục cập nhật animation.

### 7.3. Bóng lá Về mình

- Hai cụm bóng lá ở mép, opacity khoảng 0.035–0.075, mask mềm.
- Rotate tối đa ±1,2°, translate 2–5px; chu kỳ 16–24 giây, lệch pha.
- Wrapper sway tách wrapper parallax; chữ và khối giới thiệu đứng yên.
- Không thêm bóng lá động vào hero; reduced motion dùng bóng tĩnh.

### 7.4. Đường nối các section

- SVG nét 1px theo mép trong container, nối About→Focus→Project→Contact.
- 3–4 điểm như hạt giống ở mốc section, hiện nhẹ khi cuộn tới.
- stroke-dasharray/stroke-dashoffset theo độ dài path và tiến độ cuộn.
- Không đi qua chữ/nút/khung ảnh; không hardcode chiều cao cho mọi màn hình.
- Resize/font/đổi ngôn ngữ: đo lại mốc bằng ResizeObserver hoặc cơ chế có cleanup.
- Mobile dùng nét dọc ngắn ở lề hoặc divider; reduced motion giữ đường tĩnh.

### 7.5. Khung tác giả

- SVG rect overlay 1px, vẽ border 650–800ms khi vào viewport lần đầu.
- Hover scale nội dung 1.025–1.035 trong 300ms; frame clip, border không scale.
- Chỉ hover với thiết bị hỗ trợ hover.
- Reduced motion có border hoàn chỉnh, nội dung đứng yên.

### 7.6. Copy email

- Button riêng, nhãn truy cập “Sao chép email”.
- Gọi navigator.clipboard.writeText(AUTHOR_INFO.email) trong thao tác click.
- Chỉ sau promise thành công mới hiện “Đã sao chép” sát nút 1,5–2 giây.
- Thông báo role=status/aria-live=polite.
- Clipboard lỗi/không hỗ trợ: báo “Không thể sao chép, hãy chọn địa chỉ email”, cho chọn chữ.
- Dọn timeout khi unmount; thao tác nhiều lần cập nhật một thông báo, không xếp chồng.

### 7.7. Audio tùy chọn

- Một track rừng, một track mưa nhẹ, có quyền sử dụng.
- Nút có trạng thái Bật/Tắt và aria-pressed.
- Mặc định preload=none, chưa fetch/phát trước khi bật; không autoplay, không tự bật lại khi tải trang.
- Âm lượng tham chiếu 0.15–0.25; fade-in 600–1000ms, tắt thì fade-out ngắn rồi pause.
- MOVE FORWARD dùng mưa; slide thiên nhiên dùng rừng. START AGAIN/DECISION có thể im lặng, chế độ bật của người dùng vẫn được giữ.
- Đổi track fade-out rồi fade-in, tránh tăng âm lượng do chồng nguồn.
- Mỗi play() xử lý promise; trình duyệt từ chối thì báo cần bấm bật lại, không thể hiện đang phát giả.
- Tab ẩn pause; quay lại chỉ resume nếu vẫn bật và được phép phát.
- Cleanup audio/timer khi rời landing/đăng nhập thành công.
- Thiếu file audio: dev hiển thị trạng thái chưa có asset; không tạo URL giả. Nghiệm thu cần file thật.

### 7.8. Theme mềm 300ms

- Dùng useAppTheme và variables riêng của landing.
- Transition background-color/color/border-color đồng thời khoảng 300ms.
- Không transition: all toàn cây; gradient đổi bằng hai layer crossfade opacity nếu cần.
- Đọc theme đã lưu sớm để tránh flash; kiểm tra lần đầu vì provider hiện mặc định light.
- Controls trên ảnh có token riêng giữ tương phản ở theme sáng.
- Reduced motion bỏ/rút ngắn transition, không flash trắng.

## 8. Cấu trúc file dự kiến

Tất cả đường dẫn dưới đây thuộc D:/QL_NS/QuanLyNhanSu.

| Thao tác | File | Trách nhiệm |
| --- | --- | --- |
| Sửa | HRMS.Web/src/pages/Login.tsx | Ghép section, giữ modal/callback, truyền pause do modal |
| Sửa | HRMS.Web/src/pages/login/components/LandingHeader.tsx | Header, menu, anchor, theme/audio |
| Sửa | HRMS.Web/src/components/CinematicHeroGallery.tsx | Hero, thumbnail, progress, điều hướng và drag |
| Sửa | HRMS.Web/src/components/CinematicHeroGallery.css | Layout/lớp ảnh/controls/responsive |
| Thêm | HRMS.Web/src/pages/login/LandingPage.css | Tokens và CSS scope các section |
| Thêm | HRMS.Web/src/pages/login/data/cinematicSlides.ts | 10 slide và metadata môi trường/ảnh |
| Thêm | HRMS.Web/src/pages/login/components/LandingAbout.tsx | Bio, kỹ năng, khung tác giả |
| Thêm | HRMS.Web/src/pages/login/components/LandingFocus.tsx | Ba hướng đang tập trung |
| Thêm | HRMS.Web/src/pages/login/components/LandingProject.tsx | HRMS, mockup, CTA |
| Thêm | HRMS.Web/src/pages/login/components/LandingContact.tsx | Liên hệ/copy email |
| Thêm | HRMS.Web/src/pages/login/components/SlideAtmosphere.tsx | Đúng một môi trường |
| Thêm | HRMS.Web/src/pages/login/components/LandingJourneyLine.tsx | Đường nối SVG |
| Thêm | HRMS.Web/src/pages/login/components/AuthorFrame.tsx | Viền vẽ/crop/monogram |
| Thêm | HRMS.Web/src/pages/login/components/AmbientAudioToggle.tsx | Control audio |
| Thêm | HRMS.Web/src/pages/login/hooks/useHeroCarousel.ts | Autoplay/progress/pause/chuyển cảnh |
| Thêm | HRMS.Web/src/pages/login/hooks/useCarouselDrag.ts | Pointer/axis lock/velocity/settle |
| Thêm | HRMS.Web/src/pages/login/hooks/useLandingParallax.ts | Scroll RAF và visibility |
| Thêm | HRMS.Web/src/pages/login/hooks/useAmbientAudio.ts | Tải/phát/fade/pause/cleanup |
| Sửa nhỏ | HRMS.Web/src/App.tsx | Bỏ intro dài ở nhánh landing, bảo toàn diff hiện có |
| Sửa nhỏ | HRMS.Web/src/hooks/useScrollReveal.ts, src/components/ScrollReveal.tsx | Thêm waitForIntro |
| Sửa | HRMS.Web/src/locales/{vi,en,ja,ko,zh-CN}.ts | Nội dung/controls/thông báo mới |
| Sửa nếu cần | HRMS.Web/src/pages/login/types.ts | githubProfile/cấu hình khung tác giả |
| Thêm | HRMS.Web/public/images/landing/, public/audio/ | Nền/lá/mockup/ảnh tối ưu/audio |
| Thêm | HRMS.Web/tests/heroCarousel.test.ts | Logic thời gian/pause/wrap/drag/cancel |

Các LandingHero/LandingEcosystem/LandingFeatures/LandingArchitecture cũ ngừng được render sau khi chuyển nội dung và link cần thiết. Kiểm tra nơi sử dụng trước khi dọn file.

Không thêm Framer Motion, GSAP, particles hay thư viện carousel chỉ để làm các hiệu ứng trên. Nếu phát sinh nhu cầu thực sự, đánh giá phần stack hiện tại làm được và nêu lý do.

## 9. Asset và hiệu năng

### 9.1. Asset cần chuẩn bị

1. 10 ảnh có sẵn: bản responsive khoảng 768/1280/1920px; thumbnail riêng 160–240px; WebP/AVIF và fallback.
2. PATIENCE gần ảnh mẫu: hoa nở bên phải, ánh ấm, vùng chữ trái. Ghi rõ khác biệt nếu tạm dùng nụ hoa hiện có.
3. Nền project và rừng/suối contact, lá/dương xỉ trong suốt hoặc SVG.
4. Monogram ND, screenshot desktop/mobile, frame thiết bị.
5. Hai track rừng/mưa loop êm, license phù hợp.

Lập manifest nguồn/license/kích thước/dung lượng. Không hotlink nguồn thiếu ổn định. Placeholder có thể dùng để dựng, cần ghi trong báo cáo dev và thay trước nghiệm thu.

### 9.2. Tải và render

- Ảnh mở đầu ưu tiên cao, không lazy-load ảnh LCP.
- Dùng img/picture positioned như background để có srcset, sizes, fetchpriority và kích thước rõ ràng.
- Sau ảnh đầu, preload/decode hai ảnh liền kề theo index vòng lặp; ảnh khác tải khi gần được dùng hoặc idle có điều kiện.
- Chỉ mount 2–3 lớp hero cần thiết; thumbnail dùng file nhỏ. Không render backgroundImage của cả 10 JPG lớn ngay từ đầu.
- Ảnh/nền dưới fold lazy-load; audio sau click.
- Asset có aspect ratio/kích thước và placeholder màu để tránh layout shift.
- Budget khởi điểm: hero desktop 200–450 KB, mobile 100–220 KB, thumbnail <20 KB; initial load mục tiêu <1,5 MB trước audio. Đây là mục tiêu cần đo, chưa phải kết quả thực tế.

### 9.3. Animation

- Ưu tiên transform/opacity, không animate width/left/top để kéo ảnh.
- RAF scroll dùng chung; RAF progress chỉ chạy khi carousel đang phát/hiển thị.
- Không rerender React mỗi frame; không will-change mọi slide suốt thời gian trang tồn tại.
- Pause CSS effect khi tab ẩn/hero offscreen; unmount effect inactive, cleanup RAF/listener/observer/timer.
- Không video nền/WebGL. Máy yếu giảm hiệu ứng, vẫn giữ bố cục/chữ/điều hướng/accent.
- Mục tiêu đo: CLS ≤0.1, LCP ≤2.5s, INP ≤200ms trong điều kiện ghi rõ. Dùng trace lab khi chưa có dữ liệu người dùng thực; không coi điểm Lighthouse là bằng chứng đầy đủ cho INP.

## 10. Responsive và khả năng sử dụng

| Viewport | Hành vi |
| --- | --- |
| ≥1200px | Header đầy đủ, 5 thumbnail, About 2 cột, Focus 3 cột, Project 2 cột |
| 768–1199px | Header gọn, giảm spacing, 3–5 thumbnail, mockup không chồng chữ |
| <768px | Menu mobile, hero cao theo nội dung, khoảng 3 thumbnail, section 1 cột, CTA wrap |

- Kiểm tra 360, 390, 768, 1024, 1440, 1920px; zoom 200%; câu dài ở 5 ngôn ngữ.
- Main/section có heading theo thứ tự, một H1 rõ ràng; câu trích dẫn dùng blockquote.
- Quote tự đổi không liên tục phát qua live region.
- Carousel có nhãn; thumbnail là button, slide hiện tại dùng aria-current và viền/hình dạng.
- Phím trái/phải chỉ điều hướng khi focus trong carousel phù hợp; không bắt toàn window/input/modal.
- Vùng chạm chính ≥44×44px. Dots desktop có vùng bấm lớn hơn nét nhìn thấy; mobile ưu tiên bộ đếm và nút lớn nếu dots không vừa.
- Focus-visible rõ; focus vào phần đang reveal làm nội dung hiện ngay.
- Menu thu gọn vẫn cho truy cập download/ngôn ngữ/theme/audio.
- Tương phản mục tiêu: chữ thường ≥4.5:1; chữ lớn và ranh giới controls cần thiết ≥3:1. Đo trên ảnh sáng nhất và cả hai theme.
- Reduced motion tắt môi trường động, parallax, sway, border draw, stagger, autoplay mặc định; giữ nội dung đầy đủ và điều hướng tay.
- Drag/trang trí không khóa cuộn dọc; modal thật vẫn quản lý scroll/focus theo hành vi hiện tại.

## 11. Giai đoạn triển khai

### Giai đoạn 1 — Baseline

- Đọc diff và các phụ thuộc intro/hash.
- Chạy npm run build, npm run lint, npm test từ HRMS.Web; ghi lỗi có sẵn.
- Chụp desktop/mobile hiện tại, kiểm tra login/download/language/theme.
- Lập danh mục asset và phần thiếu.

Đầu ra: baseline, danh sách file sửa, manifest asset, ảnh đối chiếu.

### Giai đoạn 2 — UI tĩnh theo ảnh 2

- Xây token, header, hero, About, Focus, Project, Contact, footer.
- Giữ callback/modal/dữ liệu, thêm nội dung 5 ngôn ngữ.
- Mở PATIENCE, thumbnail phẳng, bỏ typewriter/coverflow.
- Hiệu chỉnh crop/font/gradient/tỷ lệ/spacing desktop rồi mobile.

Qua bước khi: trang tĩnh gần ảnh mẫu, chữ không tràn, CTA hoạt động.

### Giai đoạn 3 — Carousel và thời gian

- Tách data/controller/pause reasons.
- Next/prev/thumbnail/dots/loop/timeline/pause-play.
- Xử lý modal/tab hidden/offscreen/ảnh chưa decode.
- Kiểm tra progress và thời gian còn lại.

Qua bước khi: timer/progress đồng bộ, pause không bị ghi đè, không double transition.

### Giai đoạn 4 — Drag và môi trường

- Kéo theo tay, axis lock, cancel/settle/loop.
- Thêm atmosphere và accent, làm 5 slide chủ đạo trước rồi 5 slide còn lại.
- Hiệu chỉnh focal point/mobile/câu dài/mask.

Qua bước khi: ảnh theo tay, vẫn cuộn dọc tự nhiên, đúng một môi trường trên mỗi slide.

### Giai đoạn 5 — Cuộn và tiện ích

- Parallax, bóng lá, đường nối, frame tác giả, copy email.
- Bỏ intro dài landing và dùng waitForIntro=false.
- Theme 300ms và entry ≤900ms.

Qua bước khi: section reveal trên tab mới, không scroll lock/flash, copy báo đúng kết quả.

### Giai đoạn 6 — Audio và tối ưu

- Audio click-to-play, fade, lỗi, tab hidden, cleanup khi login.
- Ảnh responsive/preload liền kề, font, giảm effect mobile/reduced motion.
- Đo mạng/trace và xử lý jank/tải dư.

Qua bước khi: audio mặc định tắt, không leak, không tải cả 10 JPG, budget được đo.

### Giai đoạn 7 — Nghiệm thu

- Chạy lại build/lint/test, bổ sung test logic có rủi ro.
- Chụp đủ viewport/hai theme, so bố cục/font/độ sáng/crop/spacing với ảnh mẫu.
- Kiểm tra bàn phím/reduced motion/mạng chậm/asset lỗi/clipboard lỗi/login/download.
- Sửa lỗi rồi chạy lại phần liên quan.
- Bàn giao file thay đổi, ảnh trước/sau, kết quả và phần còn thiếu.

## 12. Tiêu chí nghiệm thu

| Yêu cầu | Cách xác nhận |
| --- | --- |
| Bố cục ảnh 2 | Đúng thứ tự, tone rừng/kem, hero đủ sáng, thumbnail ngang, section có tỷ lệ/spacing phù hợp |
| Mỗi slide một không khí | 5 slide chủ đạo đúng mapping, không mount thêm môi trường khác |
| Accent | Viền/nút/timeline đổi đồng bộ, nhẹ, đủ tương phản |
| Timeline | Đúng chu kỳ, freeze/resume chính xác, pause theo mọi lý do |
| Drag | Ảnh theo tay; commit/snap/cancel/loop/resize không kẹt |
| Entry | ≤900ms; nền trước, chữ, thumbnail; không còn overlay 4,5 giây ở landing |
| Parallax | Nền chậm hơn nội dung, lá khác tốc độ; không lộ mép/layout shift |
| Bóng lá | About rất mờ, sway nhẹ, reduced motion tĩnh |
| Đường nối | Theo tiến độ cuộn, mốc đúng sau resize/ngôn ngữ, không che nội dung |
| Frame tác giả | Border vẽ một lần, hover nhẹ, crop đúng, reduced motion đầy đủ |
| Copy | Clipboard nhận đúng email, báo thành công thực, lỗi có fallback |
| Audio | Mặc định tắt, sau click mới tải/phát, xử lý lỗi/tab hidden/rời trang |
| Theme | Khoảng 300ms, nền/chữ/viền đồng bộ, lưu lựa chọn, không flash |
| Chức năng cũ | Login callback, URL tải Windows/APK, ngôn ngữ/theme/link tác giả đúng |
| Responsive | Không tràn từ 360px, zoom/câu dài đọc được, keyboard/focus/reduced motion đầy đủ |
| Hiệu năng | Không tải bộ JPG lớn ngay đầu, không animation ngoài viewport/tab ẩn, báo cáo đo có điều kiện rõ |

Test tự động cần thiết cho controller tương tác mới:

- Nhiều pause reasons: bỏ hover nhưng còn modal thì vẫn dừng.
- Pause/resume giữ đúng thời gian; đổi ảnh tay reset và giữ dừng.
- Next/prev wrap ở hai đầu; thao tác nhanh không double commit.
- Drag ngang commit theo distance/velocity, kéo dọc không đổi ảnh, cancel an toàn.
- Unmount hủy timer/RAF; không chuyển ảnh sau khi rời landing.

Dùng runner Node hiện có cho logic thuần, nối test mới vào script test nếu phù hợp. Kiểm tra UI thật trên trình duyệt; không thay bằng test tìm chuỗi trong source. Không thêm framework kiểm thử mới khi chưa cần.

Nếu baseline lỗi do thay đổi ngoài scope, báo lỗi trước/sau và phần chưa xác minh; không sửa nghiệp vụ để làm đẹp kết quả UI.

## 13. Prompt dán vào Antigravity IDE

```text
Hãy triển khai landing page TryHardAgain/HRMS Enterprise theo kế hoạch trong
docs/antigravity-tryhardagain-ui-plan.md, repo D:/QL_NS/QuanLyNhanSu.

Ảnh mục tiêu:
D:/User/Downloads/Website cá nhân giữa rừng xanh.png
Ảnh UI hiện tại:
C:/Users/duyth/AppData/Local/Temp/codex-clipboard-8c2619c0-6a49-4f6b-9344-424d77cc403e.png

Đọc code và diff trước khi sửa. Thực hiện tuần tự các giai đoạn: UI tĩnh,
carousel/timeline, drag/môi trường, hiệu ứng cuộn/tiện ích, audio/tối ưu,
nghiệm thu. Bảo toàn các thay đổi đang có.

Chỉ thay landing HRMS.Web, giữ login/download callback và URL, ngôn ngữ/theme.
Chỉnh App.tsx tối thiểu để landing không chạy WelcomeIntro dài; thêm
waitForIntro cho ScrollReveal để các section không chờ vô hạn.
Anchor landing không được phá hash route.

Giữ 10 slide và câu chữ hiện có, mở PATIENCE (hero-05), bỏ typewriter và
coverflow 3D. Mỗi slide chỉ có một môi trường chính; accent và timeline
đồng bộ. Kéo chuột/touch làm ảnh đi theo tay, vẫn cuộn dọc tự nhiên.
Dừng autoplay khi tương tác; đổi ảnh tay chỉ tiếp tục khi bấm Tiếp tục.
Audio mặc định tắt, chỉ phát sau click. Entry tối đa 900ms, theme khoảng
300ms. Hoàn thiện mobile và prefers-reduced-motion.

Ưu tiên React/TypeScript/CSS/SVG hiện có. Không dùng nguyên ảnh mẫu làm
nền trang, không URL asset giả, không sửa nghiệp vụ/API/database.
Ghi rõ asset thiếu, thay placeholder trước nghiệm thu.
Ưu tiên ảnh đầu, preload ảnh liền kề, không tải cả bộ JPG lớn.

Chạy build/lint/test, kiểm tra trình duyệt ở các viewport trong kế hoạch,
chụp ảnh trước/sau, báo kết quả thực tế và phần chưa xác minh.
Hoàn tất khi bố cục, khả năng đọc, tương tác, chức năng cũ và hiệu năng
cùng đạt tiêu chí, không chỉ khi đã thêm đủ hiệu ứng.
```

