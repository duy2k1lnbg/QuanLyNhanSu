; Script generated for HRMS Enterprise Setup
; Supports 5 languages with interactive selection and non-destructive preference mapping

#define MyAppName "HRMS - Phần mềm Quản lý Nhân sự"
#define MyAppVersion "3.5.0"
#define MyAppPublisher "HRMS Enterprise"
#define MyAppExeName "QLyNSu.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{5C16C978-B549-4D88-90D1-168A0E418A32}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\HRMS
DisableProgramGroupPage=yes
; Hỗ trợ chạy trên Windows 64-bit và 32-bit
ArchitecturesInstallIn64BitMode=x64compatible
; Output setup configuration
OutputDir=D:\User\Desktop\HRMS_Release
OutputBaseFilename=HRMS_Setup_v3.5.0
SetupIconFile=HRMS.Desktop\Resources\hrm_icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; Enable interactive language selection dialog on install
ShowLanguageDialog=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "vietnamese"; MessagesFile: "compiler:Languages\Vietnamese.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[CustomMessages]
english.CreateDesktopIcon=Create a &desktop shortcut
english.AdditionalIcons=Additional shortcuts:
english.LaunchApp=Launch HRMS Enterprise

vietnamese.CreateDesktopIcon=Tạo biểu tượng trên &màn hình nền (Desktop)
vietnamese.AdditionalIcons=Biểu tượng bổ sung:
vietnamese.LaunchApp=Khởi chạy phần mềm HRMS Enterprise

japanese.CreateDesktopIcon=デスクトップにショートカットを作成する(&D)
japanese.AdditionalIcons=追加のショートカット:
japanese.LaunchApp=HRMS Enterprise を起動する

chinesesimplified.CreateDesktopIcon=创建桌面快捷方式(&D)
chinesesimplified.AdditionalIcons=其他快捷方式:
chinesesimplified.LaunchApp=启动 HRMS 企业管理系统

korean.CreateDesktopIcon=바탕 화면에 바로가기 만들기(&D)
korean.AdditionalIcons=추가 바로가기:
korean.LaunchApp=HRMS Enterprise 실행

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Lấy file thực thi chính từ thư mục Release
Source: "HRMS.Desktop\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; Lấy toàn bộ dependency trong Release, loại trừ file settings máy dev/build để không ghi đè cấu hình người dùng
Source: "HRMS.Desktop\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "system_settings.json,*.user,*.pdb"

; File cấu hình prompts AI
Source: "HRMS.Desktop\ai_prompts.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[Code]
// Helper returning the Vietnamese canonical name matching Desktop TranslationManager
function GetSelectedCanonicalLanguage(): string;
begin
  if ActiveLanguage = 'vietnamese' then
    Result := 'Tiếng Việt'
  else if ActiveLanguage = 'english' then
    Result := 'Tiếng Anh'
  else if ActiveLanguage = 'japanese' then
    Result := 'Tiếng Nhật'
  else if ActiveLanguage = 'chinesesimplified' then
    Result := 'Tiếng Trung'
  else if ActiveLanguage = 'korean' then
    Result := 'Tiếng Hàn'
  else
    Result := 'Tiếng Việt';
end;

// Non-destructively initialize language on first run
procedure CurStepChanged(CurStep: TSetupStep);
var
  SettingsFile: string;
  JsonContent: string;
begin
  if CurStep = ssPostInstall then
  begin
    SettingsFile := ExpandConstant('{app}\system_settings.json');
    // Only write default language if settings file does not already exist
    // This preserves existing user preferences on application upgrades
    if not FileExists(SettingsFile) then
    begin
      JsonContent := '{' + #13#10 +
                     '  "Language": "' + GetSelectedCanonicalLanguage() + '"' + #13#10 +
                     '}';
      SaveStringToFile(SettingsFile, JsonContent, False);
    end;
  end;
end;
