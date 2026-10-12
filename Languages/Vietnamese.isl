;
; Vietnamese translation by memecoder (memecoder17@gmail.com)
; Last modification date: 2023-10-26
;
[LangOptions]
LanguageName=Tiếng Việt
LanguageID=$042A
LanguageCodePage=1258
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=
;DialogFontSize=9
;DialogFontBaseScaleWidth=7
;DialogFontBaseScaleHeight=15
;WelcomeFontName=Segoe UI
;WelcomeFontSize=14

[Messages]

; *** Application titles
SetupAppTitle=Cài đặt
SetupWindowTitle=Cài đặt - %1
UninstallAppTitle=Gỡ cài đặt
UninstallAppFullTitle=Gỡ cài đặt %1

; *** Misc. common
InformationTitle=Thông tin
ConfirmTitle=Xác nhận
ErrorTitle=Lỗi

; *** SetupLdr messages
SetupLdrStartupMessage=Chương trình sẽ tiến hành cài đặt %1. Bạn có muốn tiếp tục không?
LdrCannotCreateTemp=Không thể tạo tệp tạm thời. Đã hủy quá trình cài đặt
LdrCannotExecTemp=Không thể mở tệp trong thư mục tạm. Đã hủy quá trình cài đặt
HelpTextNote=

; *** Startup error messages
LastErrorMessage=%1.%n%nLỗi %2: %3
SetupFileMissing=Không tìm thấy tệp %1 trong thư mục cài đặt. Vui lòng kiểm tra lại hoặc tải một bản cài đặt mới.
SetupFileCorrupt=Các tệp cài đặt đã bị hỏng. Vui lòng tải lại một bản cài đặt mới.
SetupFileCorruptOrWrongVer=Các tệp cài đặt đã bị hỏng hoặc không tương thích với phiên bản hiện tại. Vui lòng tải lại một bản cài đặt mới.
InvalidParameter=Lệnh truyền vào không hợp lệ:%n%n%1
SetupAlreadyRunning=Chương trình cài đặt này đang được chạy rồi.
WindowsVersionNotSupported=Phần mềm này không hỗ trợ phiên bản Windows mà máy tính của bạn đang sử dụng.
WindowsServicePackRequired=Phần mềm này yêu cầu %1 Service Pack %2 hoặc mới hơn.
NotOnThisPlatform=Phần mềm này sẽ không hoạt động trên %1.
OnlyOnThisPlatform=Phần mềm này chỉ có thể hoạt động trên %1.
OnlyOnTheseArchitectures=Phần mềm này chỉ có thể cài đặt trên các phiên bản Windows hỗ trợ cấu trúc vi xử lý sau:%n%n%1
WinVersionTooLowError=Phần mềm này yêu cầu %1 phiên bản %2 hoặc mới hơn.
WinVersionTooHighError=Không thể cài phần mềm này trên %1 phiên bản %2 hoặc mới hơn.
AdminPrivilegesRequired=Bạn cần đăng nhập bằng tài khoản Quản trị viên (Administrator) để cài đặt phần mềm này.
PowerUserPrivilegesRequired=Bạn cần đăng nhập bằng tài khoản Quản trị viên (Admin) hoặc thuộc nhóm Power Users để cài đặt phần mềm này.
SetupAppRunningError=Hệ thống phát hiện %1 hiện đang chạy.%n%nVui lòng đóng tất cả các cửa sổ của ứng dụng đó lại, sau đó nhấn OK để tiếp tục, hoặc nhấn Hủy để thoát.
UninstallAppRunningError=Hệ thống phát hiện %1 hiện đang chạy.%n%nVui lòng đóng tất cả các cửa sổ của ứng dụng đó lại, sau đó nhấn OK để tiếp tục, hoặc nhấn Hủy để thoát.

; *** Startup questions
PrivilegesRequiredOverrideTitle=Chọn Chế độ Cài đặt
PrivilegesRequiredOverrideInstruction=Bạn muốn cài đặt theo chế độ nào?
PrivilegesRequiredOverrideText1=Bạn có thể cài đặt %1 cho tất cả mọi người dùng chung máy tính này (yêu cầu quyền Admin), hoặc chỉ cài cho riêng bạn.
PrivilegesRequiredOverrideText2=Bạn có thể cài đặt %1 cho riêng bạn, hoặc cài cho tất cả mọi người dùng chung máy tính này (yêu cầu quyền Admin).
PrivilegesRequiredOverrideAllUsers=Cài đặt cho &tất cả người dùng
PrivilegesRequiredOverrideAllUsersRecommended=Cài cho &tất cả người dùng (Khuyên dùng)
PrivilegesRequiredOverrideCurrentUser=Chỉ cài cho &riêng tôi
PrivilegesRequiredOverrideCurrentUserRecommended=Chỉ cài cho &riêng tôi (Khuyên dùng)

; *** Misc. errors
ErrorCreatingDir=Không thể tạo thư mục "%1"
ErrorTooManyFilesInDir=Không thể tạo tệp trong thư mục "%1" vì bên trong đã có quá nhiều tệp

; *** Setup common messages
ExitSetupTitle=Thoát Cài đặt
ExitSetupMessage=Quá trình cài đặt chưa hoàn tất. Nếu bạn thoát bây giờ, phần mềm sẽ không được cài vào máy.%n%nBạn có thể mở lại trình cài đặt này vào lúc khác.%n%nBạn có chắc chắn muốn thoát không?
AboutSetupMenuItem=&Thông tin về trình cài đặt...
AboutSetupTitle=Thông tin về trình cài đặt
AboutSetupMessage=%1 phiên bản %2%n%3%n%nTrang chủ %1:%n%4
AboutSetupNote=
TranslatorNote=Vietnamese translation by memecoder

; *** Buttons
ButtonBack=< &Quay lại
ButtonNext=&Tiếp theo >
ButtonInstall=&Cài đặt
ButtonOK=OK
ButtonCancel=Hủy
ButtonYes=&Có
ButtonYesToAll=Có với &Tất cả
ButtonNo=&Không
ButtonNoToAll=Khôn&g với Tất cả
ButtonFinish=&Hoàn tất
ButtonBrowse=&Duyệt...
ButtonWizardBrowse=D&uyệt...
ButtonNewFolder=&Tạo thư mục mới

; *** "Select Language" dialog messages
SelectLanguageTitle=Chọn ngôn ngữ cài đặt
SelectLanguageLabel=Vui lòng chọn ngôn ngữ bạn muốn sử dụng trong quá trình cài đặt.

; *** Common wizard text
ClickNext=Nhấn 'Tiếp theo' để tiếp tục, hoặc 'Hủy' để thoát.
BeveledLabel=
BrowseDialogTitle=Chọn thư mục
BrowseDialogLabel=Vui lòng chọn một thư mục trong danh sách bên dưới, sau đó nhấn OK.
NewFolderName=Thư mục mới

; *** "Welcome" wizard page
WelcomeLabel1=Chào mừng bạn đến với Cài đặt [name]
WelcomeLabel2=Chương trình sẽ cài đặt [name/ver] lên máy tính của bạn.%n%nBạn nên đóng tất cả các ứng dụng khác trước khi tiếp tục để đảm bảo quá trình cài đặt diễn ra suôn sẻ.

; *** "Password" wizard page
WizardPassword=Mật khẩu
PasswordLabel1=Bộ cài đặt này được bảo vệ bằng mật khẩu.
PasswordLabel3=Vui lòng nhập mật khẩu, sau đó nhấn 'Tiếp theo'. Lưu ý: Mật khẩu có phân biệt chữ hoa và chữ thường.
PasswordEditLabel=&Mật khẩu:
IncorrectPassword=Mật khẩu bạn nhập chưa đúng. Vui lòng thử lại nhé.

; *** "License Agreement" wizard page
WizardLicense=Thỏa thuận Sử dụng
LicenseLabel=Vui lòng đọc kỹ các thông tin quan trọng dưới đây trước khi tiếp tục.
LicenseLabel3=Vui lòng đọc Thỏa thuận Sử dụng sau đây. Bạn cần đồng ý với các điều khoản này để tiếp tục cài đặt.
LicenseAccepted=Tôi &đồng ý với các điều khoản trên
LicenseNotAccepted=Tôi &không đồng ý

; *** "Information" wizard pages
WizardInfoBefore=Thông tin
InfoBeforeLabel=Vui lòng đọc kỹ các thông tin quan trọng dưới đây trước khi tiếp tục.
InfoBeforeClickLabel=Khi bạn đã sẵn sàng tiếp tục, hãy nhấn 'Tiếp theo'.
WizardInfoAfter=Thông tin
InfoAfterLabel=Vui lòng đọc kỹ các thông tin quan trọng dưới đây trước khi tiếp tục.
InfoAfterClickLabel=Khi bạ
