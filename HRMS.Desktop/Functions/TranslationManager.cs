using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace QLyNSu.Functions
{
    public static class TranslationManager
    {
        private static readonly string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_settings.json");
        private static string _currentLanguage = "Tiếng Việt";

        // Memory-safe cache of original texts to support infinite dynamic switching
        private static readonly ConditionalWeakTable<object, string> _originalTexts = new ConditionalWeakTable<object, string>();

        #region Win32 Hook to automatically translate all forms and message boxes

        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static HookProc _hookProc;
        private static IntPtr _hHook = IntPtr.Zero;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        public static void InitializeHook()
        {
            if (_hHook == IntPtr.Zero)
            {
                _hookProc = new HookProc(CbtHookCallback);
                _hHook = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());
            }
        }

        public static void ShutdownHook()
        {
            if (_hHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hHook);
                _hHook = IntPtr.Zero;
            }
        }

        private static IntPtr CbtHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE)
            {
                IntPtr hWnd = wParam;

                // Get class name
                StringBuilder className = new StringBuilder(256);
                GetClassName(hWnd, className, className.Capacity);

                if (className.ToString() == "#32770") // Native Win32 dialog (MessageBox)
                {
                    // Translate dialog title
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(hWnd, title, title.Capacity);
                    string translatedTitle = Translate(title.ToString());
                    if (translatedTitle != title.ToString())
                    {
                        SetWindowText(hWnd, translatedTitle);
                    }

                    // Translate text and buttons
                    EnumChildWindows(hWnd, (childHwnd, lp) =>
                    {
                        StringBuilder childClass = new StringBuilder(256);
                        GetClassName(childHwnd, childClass, childClass.Capacity);
                        string cls = childClass.ToString();

                        if (cls == "Static" || cls == "Button")
                        {
                            StringBuilder text = new StringBuilder(512);
                            GetWindowText(childHwnd, text, text.Capacity);
                            string textStr = text.ToString();
                            if (!string.IsNullOrEmpty(textStr))
                            {
                                string translated = Translate(textStr);
                                if (translated != textStr)
                                {
                                    SetWindowText(childHwnd, translated);
                                }
                            }
                        }
                        return true;
                    }, IntPtr.Zero);
                }
                else
                {
                    // Check if it is a .NET Form
                    Form form = Form.FromHandle(hWnd) as Form;
                    if (form != null)
                    {
                        if (QLyNSu.Program.AppIcon != null && form.Icon != QLyNSu.Program.AppIcon)
                        {
                            try
                            {
                                form.Icon = QLyNSu.Program.AppIcon;
                            }
                            catch { }
                        }
                        try
                        {
                            Translate(form);
                        }
                        catch { }
                    }
                }
            }
            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        #endregion

        // Translation dictionary mapping Vietnamese to English, Japanese, Chinese, and Korean
        private static readonly Dictionary<string, (string English, string Japanese, string Chinese, string Korean)> _dictionary = 
            new Dictionary<string, (string, string, string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            { "CCCD:", ("Citizen ID:", "国民ID:", "身份证:", "주민등록번호:") },
            { "Loại nhân sự:", ("Employee Type:", "従業員タイプ:", "员工类型:", "직원 유형:") },
            { "HRM SYSTEM", ("HRM SYSTEM", "人事管理システム", "人力资源管理系统", "인사 관리 시스템") },
            { "Enterprise Management Platform", ("Enterprise Management Platform", "企業管理プラットフォーム", "企业管理平台", "기업 관리 플랫폼") },
            { "⚡   Local AI Assistant Integrated\r\n⚡   Oracle 19c Enterprise DB\r\n⚡    High-Security Architecture", ("⚡   Local AI Assistant Integrated\r\n⚡   Oracle 19c Enterprise DB\r\n⚡    High-Security Architecture", "⚡   ローカルAIアシスタント統合\r\n⚡   Oracle 19c エンタープライズDB\r\n⚡    高セキュリティアーキテクチャ", "⚡   集成人工智能助手\r\n⚡   Oracle 19c 企业级数据库\r\n⚡    高安全性架构", "⚡   로컬 AI 어시스턴트 통합\r\n⚡   Oracle 19c 엔터프라이즈 DB\r\n⚡    고보안 아키텍처") },
            { "Nhân viên (Office)", ("Office Worker", "オフィスワーカー", "办公室职员", "사무직") },
            { "Lái xe (Driver)", ("Driver", "運転手", "司机", "운전기사") },
            { "Công nhân (Worker)", ("Worker", "作業員", "工人", "근로자") },
            { "Nam", ("Male", "男性", "男", "남성") },
            { "Nữ", ("Female", "女性", "女", "여성") },

            // Main menu tabs
            { "Hệ Thống", ("System", "システム", "系统", "시스템") },
            { "Nhân Sự", ("HR", "人事", "人事", "인사") },
            { "Chấm Công", ("Timekeeping", "勤怠", "考勤", "근태") },
            { "Báo Biểu", ("Reports", "レポート", "报表", "보고서") },

            // Ribbon groups
            { "Tài Khoản", ("Account", "アカウント", "账户", "계정") },
            { "Danh Mục Dùng Chung", ("Common Categories", "共通カテゴリ", "公共目录", "공통 카테고리") },
            { "Nghiệp Vụ", ("Operations", "業務", "业务", "업무") },
            { "Quản Lý Chấm Công", ("Timekeeping Management", "勤怠管理", "考勤管理", "근태 관리") },

            // Ribbon buttons (Hệ Thống)
            { "Đăng Nhập", ("Login", "ログイン", "登录", "로그인") },
            { "Đăng Xuất", ("Logout", "ログアウト", "注销", "로그아웃") },
            { "Đổi Mật Khẩu", ("Change Password", "パスワード変更", "修改密码", "비밀번호 변경") },
            { "Nhóm Người Dùng", ("User Groups", "ユーザーグループ", "用户组", "사용자 그룹") },
            { "Người Dùng", ("Users", "ユーザー", "用户", "사용자") },
            { "Cập Nhật Thông Tin", ("Update Info", "情報更新", "更新信息", "정보 업데이트") },
            { "Phân Quyền Chức Năng", ("Function Permissions", "機能権限", "功能权限", "기능 권한") },
            { "Phân Quyền Báo Cáo", ("Report Permissions", "レポート権限", "报表权限", "보고서 권한") },
            { "Sao Lưu Dữ Liệu", ("Backup Data", "データバックアップ", "备份数据", "데이터 백업") },
            { "Phục Hồi Dữ Liệu", ("Restore Data", "データ復元", "恢复数据", "데이터 복원") },
            { "AI", ("AI", "AI", "AI", "AI") },
            { "Thoát", ("Exit", "終了", "退出", "종료") },
            { "Cài Đặt Hệ Thống", ("System Settings", "システム設定", "系统设置", "시스템 설정") },
            { "Cấu Hình Ngôn Ngữ", ("Language Settings", "言語設定", "语言设置", "언어 설정") },
            { "Cấu Hinh Ngôn Ngữ", ("Language Settings", "言語設定", "语言设置", "언어 설정") },

            // Ribbon buttons (Nhân Sự)
            { "Dân Tộc", ("Ethnicities", "民族", "民族", "민족") },
            { "Tôn Giáo", ("Religions", "宗教", "宗教", "종교") },
            { "Trình Độ", ("Qualifications", "学歴", "学历", "학력") },
            { "Nhân Viên", ("Employees", "社員", "员工", "직원") },
            { "Phòng Ban", ("Departments", "部署", "部门", "부서") },
            { "Bộ Phận", ("Sections", "部門", "科室", "부문") },
            { "Công Ty", ("Companies", "会社", "公司", "회사") },
            { "Chức Vụ", ("Positions", "役職", "职务", "직책") },
            { "Hợp Đồng", ("Contracts", "契約", "合同", "계약") },
            { "Lương", ("Salary", "給与", "工资", "급여") },
            { "Khen Thưởng", ("Rewards", "表彰", "表彰", "포상") },
            { "Kỷ Luật", ("Disciplines", "懲戒", "处分", "징계") },
            { "Điều Chuyển", ("Transfers", "異動", "调动", "전보") },
            { "Thôi Việc", ("Resignation", "退職", "离职", "퇴직") },

            // Ribbon buttons (Chấm Công)
            { "Loại Ca", ("Shift Types", "シフト種類", "班次类型", "교대 유형") },
            { "Loại Công", ("Work Types", "勤務種類", "工种类型", "근무 유형") },
            { "Phụ cấp", ("Allowances", "手当", "津贴", "수당") },
            { "Tăng Ca", ("Overtime", "残業", "加班", "연장 근무") },
            { "Ứng Lương", ("Salary Advance", "給与前払い", "预支工资", "가불") },
            { "Bảng Công", ("Timesheet", "勤怠表", "考勤表", "근태표") },
            { "Bảng Công Chi Tiết", ("Detailed Timesheet", "詳細勤怠表", "考勤明细表", "상세 근태표") },
            { "Bảng Lương", ("Payroll", "給与表", "工资表", "급여표") },

            // Ribbon buttons (Báo Biểu)
            { "Báo Cáo", ("Report", "レポート", "报告", "보고서") },

            // Right side dock
            { "Thông Báo", ("Notifications", "通知", "通知", "알림") },
            { "Sinh Nhật", ("Birthdays", "誕生日", "生日", "생일") },
            { "Tăng Lương", ("Salary Increments", "昇給", "加薪", "급여 인상") },

            // Common actions
            { "Lưu", ("Save", "保存", "保存", "저장") },
            { "Đóng", ("Close", "閉じる", "关闭", "닫기") },
            { "Hủy", ("Cancel", "キャンセル", "取消", "취소") },
            { "Xem", ("View", "閲覧", "查看", "보기") },
            { "Thêm", ("Add", "追加", "添加", "추가") },
            { "Sửa", ("Edit", "編集", "编辑", "수정") },
            { "Xóa", ("Delete", "削除", "删除", "삭제") },
            { "Làm Mới", ("Refresh", "更新", "刷新", "새로고침") },
            { "In", ("Print", "印刷", "打印", "인쇄") },
            { "Chức năng", ("Function", "機能", "功能", "기능") },
            { "Cảnh Báo Phân Quyền", ("Permission Warning", "権限警告", "权限警告", "권한 경고") },
            { "Bạn không có quyền truy cập chức năng này.", ("You do not have permission to access this function.", "この機能にアクセスする権限がありません。", "您没有访问此功能的权限。", "이 기능에 접근할 권한이 없습니다.") },
            { "Xuất Excel", ("Export Excel", "Excel出力", "导出Excel", "Excel 내보내기") },
            { "Tìm Kiếm", ("Search", "検索", "搜索", "검색") },

            // FrmSetting
            { "Cấu Hinh Hệ Thống", ("System Configuration", "システム構成", "系统配置", "시스템 구성") },
            { "Cấu Hình Hệ Thống", ("System Configuration", "システム構成", "系统配置", "시스템 구성") },
            { "Ngôn Ngữ Hệ Thống", ("System Language", "システム言語", "系统语言", "시스템 언어") },
            { "Cài Đặt Chung", ("General Settings", "一般設定", "常规设置", "일반 설정") },
            { "Công Cụ Quản Trị", ("Admin Tools", "管理ツール", "管理工具", "관리 도구") },
            { "Quản lý CSDL", ("Database Management", "データベース管理", "数据库管理", "데이터베이스 관리") },
            { "Cấu hình AI", ("AI Config", "AI設定", "AI配置", "AI 구성") },

            // FrmOllamaConfig & Messages
            { "Cấu Hình Kết Nối AI (Ollama)", ("AI Connection (Ollama)", "AI接続 (Ollama)", "AI连接 (Ollama)", "AI 연결 (Ollama)") },
            { "Kiểm Tra Kết Nối", ("Test Connection", "接続テスト", "测试连接", "연결 테스트") },
            { "Đang thử...", ("Testing...", "テスト中...", "测试中...", "테스트 중...") },
            { "Lưu Thiết Lập", ("Save Settings", "設定を保存", "保存设置", "설정 저장") },
            { "Kết nối đến Ollama Server thành công!", ("Connected to Ollama Server successfully!", "Ollama Serverに正常に接続しました！", "成功连接到Ollama服务器！", "Ollama 서버에 성공적으로 연결되었습니다!") },
            { "Kết nối thất bại. Mã lỗi:", ("Connection failed. Error code:", "接続に失敗しました。エラーコード:", "连接失败。错误代码:", "연결 실패. 오류 코드:") },
            { "Không thể kết nối. Vui lòng kiểm tra lại địa chỉ IP, port, mạng.", ("Cannot connect. Please check IP, port, network.", "接続できません。IP、ポート、ネットワークを確認してください。", "无法连接。请检查IP、端口、网络。", "연결할 수 없습니다. IP, 포트, 네트워크를 확인하세요.") },
            { "Chi tiết lỗi:", ("Error details:", "エラーの詳細:", "错误详情:", "오류 세부 정보:") },
            { "Vui lòng điền đầy đủ thông tin.", ("Please fill in all information.", "すべての情報を入力してください。", "请填写所有信息。", "모든 정보를 입력하십시오.") },
            { "Đã lưu thiết lập cấu hình AI.", ("AI configuration saved.", "AI設定が保存されました。", "AI配置已保存。", "AI 구성이 저장되었습니다.") },
            { "Lỗi khi lưu cấu hình:", ("Error saving config:", "設定の保存エラー:", "保存配置时出错:", "구성 저장 중 오류:") },

            // FrmDangNhap
            { "Vui lòng nhập tên đăng nhập và mật khẩu.", ("Please enter username and password.", "ユーザー名とパスワードを入力してください。", "请输入用户名和密码。", "사용자명과 비밀번호를 입력하십시오.") },
            { "Tên đăng nhập hoặc mật khẩu không đúng.", ("Incorrect username or password.", "ユーザー名またはパスワードが正しくありません。", "用户名或密码不正确。", "사용자명 또는 비밀번호가 올바르지 않습니다.") },
            { "Đang xử lý...", ("Processing...", "処理中...", "正在处理...", "처리 중...") },
            { "Lỗi kết nối cơ sở dữ liệu", ("Database connection error", "データベース接続エラー", "数据库连接错误", "데이터베이스 연결 오류") },
            { "CHÀO MỪNG", ("WELCOME", "ようこそ", "欢迎", "환영합니다") },
            { "Đăng nhập để tiếp tục truy cập hệ thống", ("Login to continue accessing the system", "ログインしてシステムへのアクセスを継続する", "登录以继续访问系统", "로그인하여 시스템 계속 접속") },
            { "Phiên bản v3.5.0", ("Version v3.5.0", "バージョン v3.5.0", "版本 v3.5.0", "버전 v3.5.0") },
            { "Ghi nhớ đăng nhập", ("Remember me", "ログイン情報を記憶", "记住登录", "로그인 기억하기") },
            { "Tên đăng nhập", ("Username", "ユーザー名", "用户名", "사용자명") },
            { "Mật khẩu", ("Password", "パスワード", "密码", "비밀번호") },
            { "Hệ thống Đăng nhập", ("Login System", "ログインシステム", "登录系统", "로그인 시스템") },

            // FrmChangePassword
            { "Mật khẩu cũ", ("Old Password", "旧パスワード", "旧密码", "기존 비밀번호") },
            { "Mật khẩu mới", ("New Password", "新パスワード", "新密码", "새 비밀번호") },
            { "Xác nhận mật khẩu", ("Confirm Password", "パスワード確認", "确认密码", "비밀번호 확인") },
            { "Thông tin tài khoản", ("Account Info", "アカウント정보", "账户信息", "계정 정보") },

            // MessageBox and dialogs
            { "Vui lòng chọn ngôn ngữ.", ("Please select a language.", "言語を選択してください。", "请选择语言。", "언어를 선택하십시오.") },
            { "Bạn có muốn đăng xuất không?", ("Do you want to log out?", "ログアウトしますか？", "您确定要退出登录吗？", "로그아웃하시겠습니까?") },
            { "Xác nhận đăng xuất", ("Confirm Logout", "ログアウト確認", "确认退出", "로그아웃 확인") },
            { "Bạn có chắc chắn muốn thoát không?", ("Are you sure you want to exit?", "終了してもよろしいですか？", "您确定要退出吗？", "정말로 종료하시겠습니까?") },
            { "Xác nhận thoát", ("Confirm Exit", "終了の確認", "确认退出", "종료 확인") },
            { "Hệ thống đang bận, vui lòng chờ một chút.", ("System is busy, please wait a moment.", "システムが混雑しています。しばらくお待ちください。", "系统忙，请稍候。", "시스템이 바쁩니다. 잠시만 기다려 주십시오.") },
            { "Vui lòng đợi", ("Please wait", "お待ちください", "请稍候", "잠시만 chờ...") },
            { "Đang xử lý dữ liệu...", ("Processing data...", "データを処理中...", "正在处理数据...", "데이터 처리 중...") },
            { "Có lỗi xảy ra", ("An error occurred", "エラーが発生しました", "发生错误", "오류가 발생했습니다") },
            { "Lỗi", ("Error", "エラー", "错误", "오류") },
            { "Cảnh báo", ("Warning", "警告", "警告", "경고") },
            { "Xác nhận", ("Confirm", "確認", "确认", "확인") },
            { "Hủy bỏ", ("Cancel", "キャンセル", "取消", "취소") },

            // Validation messages in CRUD forms
            { "Vui lòng điền tên trình độ.", ("Please enter qualification name.", "学歴名を入力してください。", "请输入学历名称。", "학력명을 입력하십시오.") },
            { "Vui lòng điền tên tôn giáo.", ("Please enter religion name.", "宗教名を入力してください。", "请输入宗教名称。", "종교명을 입력하십시오.") },
            { "Vui lòng điền tên phòng ban.", ("Please enter department name.", "部署名を入力してください。", "请输入部门名称。", "부서명을 입력하십시오.") },
            { "Bạn có chắc là xoá nó đi không?", ("Are you sure you want to delete this?", "本当に削除しますか？", "您确定要删除吗？", "정말로 삭제하시겠습니까?") },
            { "Lưu dữ liệu thành công!", ("Data saved successfully!", "データが正常に保存されました！", "数据保存成功！", "데이터가 성공적으로 저장되었습니다!") },
            { "Lưu thành công!", ("Saved successfully!", "正常に保存されました！", "保存成功！", "성공적으로 저장되었습니다!") },
            { "Lỗi khi lưu dữ liệu:", ("Error saving data:", "データの保存中にエラーが発生しました:", "保存数据时出错：", "데이터 저장 중 오류 발생:") },
            { "Vui lòng chọn một hàng để xóa.", ("Please select a row to delete.", "削除する行を選択してください。", "请选择要删除的行。", "삭제할 행을 선택하십시오.") },
            { "Xóa thành công.", ("Deleted successfully.", "正常に削除されました。", "删除成功。", "성공적으로 삭제되었습니다.") },
            { "Xóa thành công", ("Deleted successfully", "正常に削除されました", "删除成功", "성공적으로 삭제되었습니다") },
            { "Thêm thành công.", ("Added successfully.", "正常に追加されました。", "添加成功。", "성공적으로 추가되었습니다.") },
            { "Cập nhật thành công.", ("Updated successfully.", "正常に更新されました。", "更新成功。", "성공적으로 업데이트되었습니다.") },
            { "Vui lòng chọn", ("Please select", "選択してください", "请选择", "선택해 주십시오") },
            { "Hiển thị mật khẩu", ("Show password", "パスワードを表示", "显示密码", "비밀번호 표시") },
            { "Giám Sát", ("Monitor", "監視", "监控", "모니터링") },
            { "Thông Báo Mới", ("New Notification", "新着通知", "新通知", "새 알림") },
            { "Phần Mềm Quản Lý Nhân Sự", ("Human Resources Management System", "人事管理システム", "人力资源管理系统", "인사 관리 시스템") },
            { "Vui lòng điền", ("Please enter", "入力してください", "请输入", "입력해 주십시오") },
            { "Xác nhận xóa", ("Confirm Delete", "削除の確認", "确认删除", "삭제 확인") },

            // Database column headers and label texts
            { "Họ Tên", ("Full Name", "氏名", "姓名", "성명") },
            { "Ngày Sinh", ("Birth Date", "生年月日", "出生日期", "생년월일") },
            { "NGAYSINH", ("BIRTH DATE", "生年月日", "出生日期", "생년월일") },
            { "Giới Tính", ("Gender", "性別", "性别", "성별") },
            { "Điện Thoại", ("Phone", "電話番号", "电话号码", "전화번호") },
            { "Địa Chỉ", ("Address", "住所", "住所", "주소") },
            { "NGAYBATDAU", ("START DATE", "開始日", "开始日期", "시작일") },
            { "Tiếng Việt", ("Vietnamese", "ベトナム語", "越南语", "베트남어") },
            { "Tiếng Anh", ("English", "英語", "英语", "영어") },
            { "Tiếng Nhật", ("Japanese", "日本語", "日语", "일본어") },
            { "Tiếng Trung", ("Chinese", "中国語", "中文", "중국어") },
            { "Tiếng Hàn", ("Korean", "韓国語", "韩语", "한국어") },
            { "Đã lưu cài đặt ngôn ngữ hệ thống", ("Saved system language setting", "システム言語設定を保存しました", "已保存系统语言设置", "시스템 언어 설정이 저장되었습니다") },

            // Quản lý Loại Hợp Đồng
            { "Loại Hợp Đồng", ("Contract Type", "契約種別", "合同类型", "계약 유형") },
            { "Quản Lý Loại Hợp Đồng", ("Contract Type Management", "契約種別管理", "合同类型管理", "계약 유형 관리") },
            { "Tên Loại Hợp Đồng:", ("Contract Type Name:", "契約種別名:", "合同类型名称:", "계약 유형명:") },
            { "Tên Loại Hợp Đồng", ("Contract Type Name", "契約種別名", "合同类型名称", "계약 유형명") },
            { "Mã Loại HĐ", ("Type ID", "種別コード", "类型编号", "유형 코드") },
            { "LOAIHD", ("Type ID", "種別コード", "类型编号", "유형 코드") },
            { "TENLOAIHD", ("Contract Type Name", "契約種別名", "合同类型名称", "계약 유형명") },
            { "Vui lòng nhập tên loại hợp đồng.", ("Please enter contract type name.", "契約種別名を入力してください。", "请输入合同类型名称。", "계약 유형명을 입력해 주세요.") },
            { "Tên loại hợp đồng này đã tồn tại.", ("This contract type name already exists.", "この契約種別名はすでに存在します。", "此合同类型名称已存在。", "이 계약 유형명은 이미 존재합니다.") },

            // Quản lý Ngày Lễ
            { "Ngày Lễ", ("Holiday", "祝日", "节假日", "공휴일") },
            { "Quản Lý Ngày Lễ", ("Holiday Management", "祝日管理", "节假日管理", "공휴일 관리") },
            { "Tên Ngày Lễ:", ("Holiday Name:", "祝日名:", "节假日名称:", "공휴일명:") },
            { "Tên Ngày Lễ", ("Holiday Name", "祝日名", "节假日名称", "공휴일명") },
            { "Ngày Lễ:", ("Holiday Date:", "祝日日付:", "节假日日期:", "공휴일 날짜:") },
            { "Năm:", ("Year:", "年:", "年份:", "연도:") },
            { "Hệ Số:", ("Coefficient:", "係数:", "系数:", "계수:") },
            { "Lọc theo năm:", ("Filter by year:", "年で絞り込み:", "按年份筛选:", "연도별 필터:") },
            { "Mã Lễ", ("Holiday ID", "祝日コード", "节假日编号", "공휴일 코드") },
            { "Loại Ngày", ("Day Type", "日種別", "日期类型", "날짜 유형") },
            { "Tất cả các năm", ("All years", "すべての年", "所有年份", "모든 연도") },
            { "Vui lòng nhập tên ngày lễ.", ("Please enter holiday name.", "祝日名を入力してください。", "请输入节假日名称。", "공휴일명을 입력하세요.") },
            { "Ngày lễ này đã tồn tại trong danh sách.", ("This holiday already exists in the list.", "この祝日はすでにリストに存在します。", "此节假日已存在于列表中。", "이 공휴일은 이미 목록에 존재합니다.") },

            // Quản lý Tăng Ca
            { "Quản Lý Tăng Ca", ("Overtime Management", "残業管理", "加班管理", "초과근무 관리") },
            { "Ngày TC:", ("OT Date:", "残業日:", "加班日期:", "초과근무일:") },
            { "Loại Ca:", ("Shift Type:", "シフト種別:", "班次类型:", "교대 유형:") },
            { "Loại Công:", ("Work Type:", "勤務種別:", "工时类型:", "근무 유형:") },
            { "Giờ BĐ:", ("Start Time:", "開始時刻:", "开始时间:", "시작 시간:") },
            { "Giờ KT:", ("End Time:", "終了時刻:", "结束时间:", "종료 시간:") },
            { "Số Giờ:", ("Hours:", "時間数:", "小时数:", "시간:") },
            { "Trạng Thái NV:", ("Emp Status:", "従業員状態:", "员工状态:", "직원 상태:") },
            { "Quy Định OT:", ("OT Policy:", "残業規定:", "加班规定:", "초과근무 규정:") },
            { "Đơn Giá 1 Giờ:", ("Hourly Rate:", "時間単価:", "每小时单价:", "시간당 단가:") },
            { "Thành Tiền:", ("Total Amount:", "合計金額:", "总金额:", "총 금액:") },
            { "Ghi Chú:", ("Note:", "備考:", "备注:", "비고:") },
            { "Thử việc (85%)", ("Probation (85%)", "試用期間 (85%)", "试用期 (85%)", "수습 (85%)") },
            { "Chính thức (100%)", ("Official (100%)", "正社員 (100%)", "正式 (100%)", "정규직 (100%)") },
            { "Chưa chọn NV", ("No employee selected", "従業員未選択", "未选择员工", "직원 미선택") },
            { "Chưa xác định", ("Undefined", "未確定", "未确定", "미정") },
            { "Giờ BĐ", ("Start Time", "開始時刻", "开始时间", "시작 시간") },
            { "Giờ KT", ("End Time", "終了時刻", "结束时间", "종료 시간") },
            { "Số Giờ", ("Hours", "時間数", "小时数", "시간") },
            { "Quy Định", ("Policy", "規定", "规定", "규정") },
            { "Đơn Giá", ("Unit Price", "単価", "单价", "단가") },
            { "Thành Tiền", ("Total Amount", "合計金額", "总金额", "총 금액") },
            { "Vui lòng chọn nhân viên!", ("Please select an employee!", "従業員を選択してください！", "请选择员工！", "직원을 선택해 주세요!") },
            { "Giờ bắt đầu không thể lớn hơn hoặc bằng giờ kết thúc!", ("Start time cannot be later than or equal to end time!", "開始時刻は終了時刻以降にできません！", "开始时间不能大于或等于结束时间！", "시작 시간은 종료 시간보다 늦거나 같을 수 없습니다!") },
            { "Cập nhật thành công thông tin tăng ca!", ("Overtime information updated successfully!", "残業情報を正常に更新しました！", "加班信息更新成功！", "초과근무 정보가 성공적으로 업데이트되었습니다!") },
            { "Thêm mới thành công thông tin tăng ca!", ("New overtime record added successfully!", "新規残業レコードを正常に追加しました！", "新增加班记录成功！", "새 초과근무 기록이 성공적으로 추가되었습니다!") },

            // --- Phân hệ và Quản lý Phân quyền Chức năng ---
            { "Phân hệ", ("Subsystem", "サブシステム", "子系统", "하위 시스템") },
            { "Quản Lý Nhân Sự", ("HR Management", "人事管理", "人事管理", "인사 관리") },
            { "Chấm Công & Lương", ("Timekeeping & Payroll", "勤怠・給与", "考勤与薪资", "근태 및 급여") },
            { "Dashboard & Báo Cáo", ("Dashboard & Reports", "ダッシュボード・レポート", "仪表板与报表", "대시보드 및 보고서") },
            { "Mobile App", ("Mobile App", "モバイルアプリ", "移动端App", "모바일 앱") },
            { "Danh Mục", ("Master Data", "マスターデータ", "基础档案", "기초 데이터") },
            { "Mã chức năng", ("Function Code", "機能コード", "功能代码", "기능 코드") },
            { "Tên chức năng", ("Function Name", "機能名", "功能名称", "기능명") },
            { "Mã báo cáo", ("Report Code", "レポートコード", "报表代码", "보고서 코드") },
            { "Tên báo cáo", ("Report Name", "レポート名", "报表名称", "보고서명") },
            { "Cho phép xem", ("Allow View", "閲覧許可", "允许查看", "조회 허용") },
            { "Tài khoản/Nhóm", ("Account/Group", "アカウント/グループ", "账户/用户组", "계정/그룹") },
            { "Họ và tên / Mô tả", ("Full Name / Description", "氏名/説明", "姓名/说明", "성명 / 설명") },
            { "Quyền xem", ("View Permission", "閲覧権限", "查看权限", "조회 권한") },
            { "Quyền thêm", ("Add Permission", "追加権限", "添加权限", "추가 권한") },
            { "Quyền sửa", ("Edit Permission", "編集権限", "编辑权限", "수정 권한") },
            { "Quyền xóa", ("Delete Permission", "削除権限", "删除权限", "삭제 권한") },
            { "Quyền in", ("Print Permission", "印刷権限", "打印权限", "인쇄 권한") },
            { "Chọn tất cả", ("Select All", "すべて選択", "全选", "모두 선택") },
            { "Bỏ tất cả", ("Deselect All", "すべて解除", "取消全选", "모두 해제") },
            { "Sửa quyền", ("Edit Rights", "権限編集", "编辑权限", "권한 편집") },
            { "Tên nhóm", ("Group Name", "グループ名", "用户组名", "그룹명") },
            { "Mô tả nhóm", ("Group Description", "グループ説明", "用户组说明", "그룹 설명") },
            { "Tên tài khoản", ("Username", "ユーザー名", "用户名", "사용자명") },
            { "Họ và tên", ("Full Name", "氏名", "姓名", "성명") },
            { "In ấn", ("Print", "印刷", "打印", "인쇄") },
            { "Tìm kiếm chức năng / phân hệ...", ("Search function / subsystem...", "機能・サブシステムを検索...", "搜索功能/子系统...", "기능 / 하위 시스템 검색...") },
            { "Vui lòng chọn người dùng hoặc nhóm để sửa quyền.", ("Please select a user or group to edit permissions.", "権限を編集するユーザーまたはグループを選択してください。", "请选择用户或组以编辑权限。", "권한을 편집할 사용자 또는 그룹을 선택하십시오.") },
            { "Lưu phân quyền thành công.", ("Permissions saved successfully.", "権限を正常に保存しました。", "权限保存成功。", "권한이 성공적으로 저장되었습니다.") },
            { "Lỗi khi lưu phân quyền:", ("Error saving permissions:", "権限保存エラー:", "保存权限出错：", "권한 저장 오류:") },
            { "Đã làm mới dữ liệu chức năng và phân quyền thành công!", ("Refreshed functions and permissions successfully!", "機能と権限のデータを正常に更新しました！", "功能与权限数据刷新成功！", "기능 및 권한 데이터가 성공적으로 새로고침되었습니다!") },
            { "Lỗi khi làm mới:", ("Error refreshing:", "更新エラー:", "刷新出错：", "새로고침 오류:") },
            { "Dữ liệu phân quyền đang thay đổi chưa được lưu. Bạn có chắc chắn muốn đóng và hủy thay đổi không?", ("Unsaved permission changes exist. Are you sure you want to close and discard changes?", "未保存の権限変更があります。閉じて変更を破棄してもよろしいですか？", "存在未保存的权限更改。确定要关闭并放弃更改吗？", "저장되지 않은 권한 변경 사항이 있습니다. 닫고 변경 사항을 취소하시겠습니까?") },

            // --- Ribbon Buttons & Chức năng mới ---
            { "Phê Duyệt", ("Approvals", "承認", "审批", "결재") },
            { "Phê Duyệt Yêu Cầu", ("Request Approvals", "申請承認", "申请审批", "신청 결재") },
            { "Phê Duyệt Yêu Cầu (Online)", ("Online Request Approvals", "オンライン申請承認", "在线申请审批", "온라인 신청 결재") },
            { "Thông Báo Hệ Thống", ("System Announcements", "システム通知", "系统公告", "시스템 공지") },
            { "Cấp Tài Khoản Hàng Loạt", ("Bulk Account Provisioning", "一括アカウント発行", "批量账号分配", "일괄 계정 발급") },
            { "Cấu Hình Kết Nối CSDL", ("Database Connection Config", "DB接続設定", "数据库连接配置", "데이터베이스 연결 설정") },
            { "In Bảng Công Nhân Viên", ("Print Employee Timesheet", "従業員勤怠印刷", "打印员工考勤", "직원 근태 인쇄") },
            { "Cập Nhật Ngày Công", ("Update Workdays", "勤務日数更新", "更新工作日", "근무일 업데이트") },
            { "Khóa/Mở Khóa Tài Khoản", ("Lock/Unlock Account", "アカウントのロック/ロック解除", "锁定/解锁账号", "계정 잠금/해제") },

            // --- 58 Chức năng hệ thống (TB_SYS_FUNCTION) ---
            { "Đăng nhập hệ thống", ("System Login", "システムログイン", "系统登录", "시스템 로그인") },
            { "Đổi mật khẩu tài khoản", ("Change Password", "パスワード変更", "修改密码", "비밀번호 변경") },
            { "Quản lý người dùng", ("User Management", "ユーザー管理", "用户管理", "사용자 관리") },
            { "Quản lý danh mục Dân tộc", ("Ethnicity Management", "民族管理", "民族管理", "민족 관리") },
            { "Quản lý danh mục Tôn giáo", ("Religion Management", "宗教管理", "宗教管理", "종교 관리") },
            { "Quản lý danh mục Trình độ", ("Qualification Management", "学歴管理", "学历管理", "학력 관리") },
            { "Quản lý danh mục Chức vụ", ("Position Management", "役職管理", "职务管理", "직책 관리") },
            { "Quản lý danh mục Công ty", ("Company Management", "会社管理", "公司管理", "회사 관리") },
            { "Quản lý danh mục Phòng ban", ("Department Management", "部署管理", "部门管理", "부서 관리") },
            { "Quản lý danh mục Bộ phận", ("Section Management", "部門管理", "科室管理", "부문 관리") },
            { "Quản lý danh sách Nhân viên", ("Employee List Management", "社員一覧管理", "员工名册管理", "직원 목록 관리") },
            { "Quản lý Hợp đồng lao động", ("Labor Contract Management", "労働契約管理", "劳动合同管理", "근로계약 관리") },
            { "Quản lý Khen thưởng", ("Reward Management", "表彰管理", "表彰管理", "포상 관리") },
            { "Quản lý Kỷ luật", ("Discipline Management", "懲戒管理", "处分管理", "징계 관리") },
            { "Quản lý Điều chuyển nhân sự", ("Transfer Management", "人事異動管理", "人事调动管理", "인사 이동 관리") },
            { "Quản lý Thôi việc", ("Resignation Management", "退職管理", "离职管理", "퇴직 관리") },
            { "Quản lý Loại ca làm việc", ("Shift Type Management", "シフト種別管理", "班次类型管理", "교대 유형 관리") },
            { "Quản lý Loại công làm việc", ("Work Type Management", "勤務種別管理", "工时类型管理", "근무 유형 관리") },
            { "Quản lý Phụ cấp nhân viên", ("Allowance Management", "社員手当管理", "员工津贴管理", "직원 수당 관리") },
            { "Quản lý Tăng ca làm thêm", ("Overtime Management", "残業管理", "加班管理", "초과근무 관리") },
            { "Quản lý Ứng lương", ("Salary Advance Management", "給与前払い管理", "预支工资管理", "가불 관리") },
            { "Quản lý Bảng công tháng", ("Monthly Timesheet Management", "月間勤怠管理", "月度考勤管理", "월간 근태 관리") },
            { "Quản lý Bảng công chi tiết", ("Detailed Timesheet Management", "詳細勤怠管理", "考勤明细管理", "상세 근태 관리") },
            { "Quản lý Bảng tính lương", ("Payroll Management", "給与計算管理", "薪资核算管理", "급여 계산 관리") },
            { "Báo cáo danh sách nhân viên", ("Employee List Report", "社員一覧レポート", "员工名单报表", "직원 명부 보고서") },
            { "Báo cáo nâng lương", ("Salary Increment Report", "昇給レポート", "加薪报表", "급여 인상 보고서") },
            { "Báo cáo hợp đồng lao động", ("Labor Contract Report", "労働契約レポート", "劳动合同报表", "근로계약 보고서") },
            { "Phê duyệt yêu cầu từ Mobile App", ("Approve Requests from Mobile App", "モバイル申請の承認", "审批移动端申请", "모바일 신청 승인") },
            { "Quản lý và đăng thông báo", ("Announcements Management", "通知管理・投稿", "通知管理与发布", "공지사항 관리 및 등록") },
            { "Cấp phát tài khoản hàng loạt", ("Bulk Account Provisioning", "一括アカウント発行", "批量分配账号", "일괄 계정 발급") },
            { "Cấu hình kết nối CSDL Oracle", ("Oracle DB Connection Config", "Oracle DB接続設定", "Oracle数据库连接配置", "Oracle DB 연결 설정") },
            { "In ấn bảng công nhân viên", ("Print Employee Timesheet", "社員勤怠表印刷", "打印员工考勤表", "직원 근태표 인쇄") },
            { "Cập nhật ngày công nhân viên", ("Update Employee Workdays", "社員勤務日数更新", "更新员工工作日", "직원 근무일 업데이트") },
            { "Khóa hoặc mở khóa tài khoản", ("Lock or Unlock Account", "アカウントのロック/解除", "锁定或解锁账号", "계정 잠금 또는 해제") },
            { "Phân quyền chức năng hệ thống", ("System Function Authorization", "システム機能権限設定", "系统功能权限分配", "시스템 기능 권한 설정") },
            { "Phân quyền báo cáo hệ thống", ("System Report Authorization", "システムレポート権限設定", "系统报表权限分配", "시스템 보고서 권한 설정") },
            { "Quản lý danh mục ngày lễ", ("Holiday Management", "祝日管理", "节假日管理", "공휴일 관리") },
            { "Cấu hình AI và Trợ lý ảo", ("AI & Assistant Config", "AI・仮想アシスタント設定", "AI与智能助手配置", "AI 및 가상 비서 설정") },
            { "Sao lưu cơ sở dữ liệu", ("Backup Database", "データベースバックアップ", "备份数据库", "데이터베이스 백업") },
            { "Phục hồi cơ sở dữ liệu", ("Restore Database", "データベース復元", "恢复数据库", "데이터베이스 복원") },
            { "Xem lịch sử thao tác hệ thống", ("View System Audit Logs", "システム操作履歴閲覧", "查看系统操作日志", "시스템 작업 로그 조회") },
            { "Xem danh sách thiết bị mobile", ("View Mobile Device List", "モバイル端末一覧閲覧", "查看移动设备列表", "모바일 기기 목록 조회") },

            // --- 10 Chức năng Mobile App ---
            { "Xem hồ sơ cá nhân trên Mobile", ("View Personal Profile on Mobile", "モバイルで個人プロフィール閲覧", "在移动端查看个人档案", "모바일에서 개인 프로필 조회") },
            { "Chấm công GPS/Wifi trên Mobile", ("GPS/Wifi Timekeeping on Mobile", "モバイルGPS/Wifi勤怠打刻", "移动端GPS/Wifi考勤打卡", "모바일 GPS/Wifi 출퇴근 체크") },
            { "Gửi đơn xin nghỉ phép trên Mobile", ("Submit Leave Request on Mobile", "モバイルで休暇申請提出", "在移动端提交请假申请", "모바일에서 휴가 신청서 제출") },
            { "Yêu cầu bổ sung công trên Mobile", ("Attendance Adjustment Request on Mobile", "モバイルで打刻補正申請", "在移动端申请补卡", "모바일에서 근태 보정 신청") },
            { "Đăng ký làm thêm giờ trên Mobile", ("Overtime Registration on Mobile", "モバイルで残業申請", "在移动端登记加班", "모바일에서 초과근무 신청") },
            { "Tra cứu bảng công tháng trên Mobile", ("Check Monthly Timesheet on Mobile", "モバイルで月間勤怠照会", "在移动端查询月度考勤", "모바일에서 월간 근태 조회") },
            { "Tra cứu phiếu lương trên Mobile", ("Check Payslip on Mobile", "モバイルで給与明細照会", "在移动端查询工资条", "모바일에서 급여명세서 조회") },
            { "Nhận và xem thông báo trên Mobile", ("Receive Notifications on Mobile", "モバイルで通知受信・閲覧", "在移动端接收与查看通知", "모바일에서 공지 수신 및 확인") },
            { "Đổi mật khẩu tài khoản Mobile", ("Change Mobile Password", "モバイルパスワード変更", "修改移动端密码", "모바일 계정 비밀번호 변경") },
            { "Nhận thông báo đẩy (Push Notifications)", ("Push Notifications", "プッシュ通知受信", "接收推送通知", "푸시 알림 수신") },

            // --- FrmPheDuyetYeuCau ---
            { "Đơn Nghỉ Phép", ("Leave Requests", "休暇申請", "请假申请", "휴가 신청") },
            { "Chấm Công Bổ Sung", ("Attendance Adjustments", "打刻補正申請", "补卡申请", "출근 보정") },
            { "Đơn Tăng Ca", ("Overtime Requests", "残業申請", "加班申请", "초과근무 신청") },
            { "Chờ duyệt (PENDING)", ("Pending (PENDING)", "承認待ち (PENDING)", "待审批 (PENDING)", "대기 중 (PENDING)") },
            { "Đã duyệt (APPROVED)", ("Approved (APPROVED)", "承認済み (APPROVED)", "已审批 (APPROVED)", "승인됨 (APPROVED)") },
            { "Đã từ chối (REJECTED)", ("Rejected (REJECTED)", "却下済み (REJECTED)", "已拒绝 (REJECTED)", "반려됨 (REJECTED)") },
            { "Tất cả trạng thái", ("All Statuses", "すべてのステータス", "所有状态", "모든 상태") },
            { "Chờ duyệt", ("Pending", "承認待ち", "待审批", "대기 중") },
            { "Đã duyệt", ("Approved", "承認済み", "已审批", "승인됨") },
            { "Từ chối", ("Rejected", "却下", "已拒绝", "반려") },
            { "Mã Đơn", ("Request ID", "申請コード", "申请编号", "신청 번호") },
            { "Mã NV", ("Emp ID", "社員ID", "员工ID", "직원 ID") },
            { "Mã Nhân Sự", ("Employee Code", "社員コード", "工号", "사원 번호") },
            { "Loại Phép", ("Leave Type", "休暇種別", "假期类型", "휴가 유형") },
            { "Từ Ngày", ("From Date", "開始日", "开始日期", "시작일") },
            { "Đến Ngày", ("To Date", "終了日", "结束日期", "종료일") },
            { "Số Ngày", ("Days", "日数", "天数", "일수") },
            { "Lý Do Xin Nghỉ", ("Reason for Leave", "休暇理由", "请假理由", "휴가 사유") },
            { "Thời Gian Gửi", ("Submission Time", "提出時刻", "提交时间", "제출 시간") },
            { "Người Duyệt", ("Approver", "承認者", "审批人", "결재자") },
            { "Ghi Chú Duyệt", ("Approval Note", "承認備考", "审批备注", "결재 비고") },
            { "Ngày Bổ Sung", ("Adjustment Date", "補正日", "补卡日期", "보정 날짜") },
            { "Giờ Vào Gốc", ("Original Check-in", "元出勤時刻", "原签到时间", "기존 출근 시간") },
            { "Giờ Ra Gốc", ("Original Check-out", "元退勤時刻", "原签退时间", "기존 퇴근 시간") },
            { "Giờ Vào Đề Xuất", ("Proposed Check-in", "提案出勤時刻", "拟定签到时间", "제안 출근 시간") },
            { "Giờ Ra Đề Xuất", ("Proposed Check-out", "提案退勤時刻", "拟定签退时间", "제안 퇴근 시간") },
            { "Lý Do Bổ Sung", ("Adjustment Reason", "補正理由", "补卡原因", "보정 사유") },
            { "Ngày Tăng Ca", ("OT Date", "残業日", "加班日期", "초과근무일") },
            { "Giờ Bắt Đầu", ("Start Time", "開始時刻", "开始时间", "시작 시간") },
            { "Giờ Kết Thúc", ("End Time", "終了時刻", "结束时间", "종료 시간") },
            { "Số Giờ TC", ("OT Hours", "残業時間", "加班工时", "초과근무 시간") },
            { "Lý Do Tăng Ca", ("OT Reason", "残業理由", "加班原因", "초과근무 사유") },
            { "Phê Duyệt Đơn", ("Approve", "承認", "审批", "결재 승인") },
            { "Từ Chối Đơn", ("Reject", "却下", "拒绝", "결재 반려") },
            { "Vui lòng chọn một đơn xin nghỉ phép trên bảng.", ("Please select a leave request from the table.", "テーブルから休暇申請を選択してください。", "请在列表中选择一个请假申请。", "테이블에서 휴가 신청서를 선택해 주십시오.") },
            { "Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt).", ("Only requests in PENDING status can be processed.", "承認待ち (PENDING) の申請のみ処理できます。", "只能处理待审批 (PENDING) 状态的申请。", "대기 중 (PENDING) 상태의 신청만 처리할 수 있습니다.") },
            { "Nhập lý do từ chối đơn nghỉ phép:", ("Enter reason for rejecting leave request:", "休暇申請の却下理由を入力:", "输入拒绝请假申请的理由：", "휴가 신청 반려 사유 입력:") },
            { "Lý Do Từ Chối", ("Rejection Reason", "却下理由", "拒绝理由", "반려 사유") },
            { "Đã phê duyệt đơn nghỉ phép thành công!", ("Approved leave request successfully!", "休暇申請を正常に承認しました！", "请假申请审批成功！", "휴가 신청이 성공적으로 승인되었습니다!") },
            { "Đã từ chối đơn nghỉ phép thành công!", ("Rejected leave request successfully!", "休暇申請を正常に却下しました！", "请假申请已成功拒绝！", "휴가 신청이 성공적으로 반려되었습니다!") },
            { "Vui lòng chọn một bản ghi giải trình chấm công trên bảng.", ("Please select an attendance adjustment record from the table.", "テーブルから打刻補正記録を選択してください。", "请在列表中选择一条补卡记录。", "테이블에서 근태 보정 기록을 선택해 주십시오.") },
            { "Nhập lý do từ chối giải trình chấm công:", ("Enter reason for rejecting attendance adjustment:", "打刻補正の却下理由を入力:", "输入拒绝补卡的理由：", "근태 보정 반려 사유 입력:") },
            { "Đã phê duyệt giải trình chấm công thành công!", ("Approved attendance adjustment successfully!", "打刻補正を正常に承認しました！", "补卡申请审批成功！", "근태 보정이 성공적으로 승인되었습니다!") },
            { "Đã từ chối giải trình chấm công thành công!", ("Rejected attendance adjustment successfully!", "打刻補正を正常に却下しました！", "补卡申请已成功拒绝！", "근태 보정이 성공적으로 반려되었습니다!") },
            { "Vui lòng chọn một đơn đăng ký tăng ca trên bảng.", ("Please select an overtime request from the table.", "テーブルから残業申請を選択してください。", "请在列表中选择一个加班申请。", "테이블에서 초과근무 신청서를 선택해 주십시오.") },
            { "Nhập lý do từ chối đăng ký làm thêm giờ:", ("Enter reason for rejecting overtime request:", "残業申請の却下理由を入力:", "输入拒绝加班的理由：", "초과근무 신청 반려 사유 입력:") },
            { "Không tìm thấy đề xuất tăng ca.", ("Overtime request not found.", "残業申請が見つかりません。", "未找到加班申请。", "초과근무 신청을 찾을 수 없습니다.") },
            { "Bạn không thể tự phê duyệt hoặc từ chối đề xuất tăng ca của chính mình!", ("You cannot approve or reject your own overtime request!", "自分自身の残業申請を承認または却下することはできません！", "您不能审批或拒绝自己的加班申请！", "자신의 초과근무 신청을 승인하거나 반려할 수 없습니다!") },
            { "Đề xuất không còn ở trạng thái chờ duyệt hoặc đã có người khác xử lý.", ("The request is no longer pending or has been processed by someone else.", "申請は承認待ちではないか、他のユーザーによって処理されました。", "该申请已不再处于待审批状态或已被他人处理。", "해당 신청은 더 이상 대기 상태가 아니거나 다른 사용자가 처리했습니다.") },
            { "Đã phê duyệt đăng ký làm thêm giờ thành công!", ("Approved overtime request successfully!", "残業申請を正常に承認しました！", "加班申请审批成功！", "초과근무 신청이 성공적으로 승인되었습니다!") },
            { "Đã từ chối đăng ký làm thêm giờ thành công!", ("Rejected overtime request successfully!", "残業申請を正常に却下しました！", "加班申请已成功拒绝！", "초과근무 신청이 성공적으로 반려되었습니다!") },
            { "Xuất file Excel thành công:\n", ("Excel file exported successfully:\n", "Excelファイルのエクスポートに成功しました:\n", "Excel文件导出成功：\n", "Excel 파일 내보내기 성공:\n") },
            { "Lý Do Giải Trình", ("Explanation Reason", "補正理由", "说明原因", "보정 사유") },
            { "Nội Dung Công Việc", ("Job Description", "業務内容", "工作内容", "업무 내용") },
            { "Ngày Công", ("Workday", "勤務日", "工作日", "근무일") },
            { "Ngày Làm Thêm", ("Overtime Date", "残業日", "加班日期", "초과근무일") },

            // --- FrmThongBao ---
            { "Thông báo chung", ("General Announcement", "一般通知", "一般公告", "일반 공지") },
            { "Chính sách mới", ("New Policy", "新ポリシー", "新政策", "새로운 정책") },
            { "Tin khẩn cấp", ("Urgent News", "緊急ニュース", "紧急通知", "긴급 소식") },
            { "Quy chế công ty", ("Company Regulations", "社内規程", "公司规章", "회사 규정") },
            { "Bản nháp", ("Draft", "下書き", "草稿", "임시 보관") },
            { "Đã đăng", ("Published", "公開済み", "已发布", "게시됨") },
            { "Đã ẩn", ("Hidden", "非表示", "已隐藏", "숨김") },
            { "Ghim", ("Pinned", "ピン留め", "置顶", "고정") },
            { "Bình thường", ("Normal", "通常", "普通", "일반") },
            { "Tiêu đề", ("Title", "タイトル", "标题", "제목") },
            { "Nội dung", ("Content", "内容", "内容", "내용") },
            { "Người đăng", ("Publisher", "投稿者", "发布人", "작성자") },
            { "Ngày đăng", ("Publish Date", "投稿日", "发布日期", "게시일") },
            { "Loại thông báo", ("Notice Type", "通知種別", "公告类型", "공지 유형") },
            { "Ghim thông báo", ("Pin Notice", "通知を固定", "置顶公告", "공지 고정") },
            { "Tên Công ty", ("Company Name", "会社名", "公司名称", "회사명") },
            { "Tên Phòng ban", ("Department Name", "部署名", "部门名称", "부서명") },

            // --- FrmQuanLyTaiKhoan ---
            { "Danh sách tài khoản", ("Account List", "アカウント一覧", "账户列表", "계정 목록") },
            { "Chi tiết tài khoản", ("Account Details", "アカウント詳細", "账户详情", "계정 상세") },
            { "Tạo tài khoản tự động cho nhân viên chưa có tài khoản", ("Auto-create accounts for employees without accounts", "未登録社員向けアカウント自動発行", "为未开通账号的员工自动创建账号", "미생성 직원을 위한 자동 계정 생성") },
            { "Thực thi cấp phát", ("Execute Provisioning", "発行を実行", "执行分配", "발급 실행") },
            { "Mật khẩu mặc định:", ("Default Password:", "デフォルトパスワード:", "默认密码：", "기본 비밀번호:") },
            { "Tên Người Dùng", ("User Name", "ユーザー名", "用户姓名", "사용자 이름") },
            { "Mã Nhân Viên", ("Employee Code", "社員コード", "员工工号", "사원 번호") },
            { "Họ và Tên Nhân Sự", ("Full Name", "社員氏名", "员工姓名", "직원 성명") },
            { "Loại Tài Khoản", ("Account Type", "アカウント種別", "账户类型", "계정 유형") },
            { "Trạng Thái Mobile", ("Mobile Status", "モバイルステータス", "移动端状态", "모바일 상태") },
            { "Client Cho Phép", ("Allowed Client", "許可クライアント", "允许的客户端", "허용된 클라이언트") },
            { "Hoạt động", ("Active", "有効", "活动", "활성") },
            { "Bị tạm khóa", ("Suspended", "一時停止", "已停用", "일시 중지") },
            { "Khóa tài khoản", ("Lock Account", "アカウントをロック", "锁定账户", "계정 잠금") },
            { "Mở khóa", ("Unlock", "ロック解除", "解锁", "잠금 해제") },
            { "Đặt lại mật khẩu", ("Reset Password", "パスワード初期化", "重置密码", "비밀번호 초기화") },
            { "Đồng bộ Mobile", ("Sync Mobile", "モバイル同期", "同步移动端", "모바일 동기화") },
            { "Kết Quả", ("Result", "結果", "结果", "결과") },
            { "Thông Điệp Chi Tiết", ("Detail Message", "詳細メッセージ", "详细消息", "상세 메시지") },
            { "Thành công", ("Success", "成功", "成功", "성공") },
            { "Bỏ qua (Đã tồn tại)", ("Skipped (Already exists)", "スキップ (既存)", "跳过（已存在）", "건너뜀 (이미 존재)") },
            { "Thất bại", ("Failed", "失敗", "失败", "실패") },
            { "-- Tất cả phòng ban --", ("-- All Departments --", "-- すべての部署 --", "-- 所有部门 --", "-- 모든 부서 --") },
            { "Tất cả trạng thái TK", ("All Account Statuses", "すべてのアカウント状態", "所有账户状态", "모든 계정 상태") },
            { "Đang hoạt động", ("Active", "アクティブ", "正常活跃", "활성 상태") },
            { "Đã có mã nhân sự", ("Has Employee Code", "社員コードあり", "已关联工号", "사원 번호 있음") },
            { "Chưa gắn nhân sự", ("Unassigned", "社員未紐付け", "未关联员工", "직원 미연결") },
            { "Tất cả Mobile", ("All Mobile", "すべてのモバイル", "所有移动端", "모든 모바일") },
            { "Mobile Kích Hoạt", ("Mobile Enabled", "モバイル有効", "移动端已启用", "모바일 활성화") },
            { "Mobile Đang Tắt", ("Mobile Disabled", "モバイル無効", "移动端已禁用", "모바일 비활성화") },
            { "Mobile Bị Chặn (ADMIN)", ("Mobile Blocked (ADMIN)", "モバイルブロック (ADMIN)", "移动端已拦截 (ADMIN)", "모바일 차단됨 (ADMIN)") },
            { "Chọn", ("Select", "選択", "选择", "선택") },
            { "Họ và Tên Nhân Viên", ("Employee Name", "社員氏名", "员工姓名", "직원 성명") },
            { "Tài Khoản Hiện Có", ("Existing Account", "既存アカウント", "现有账户", "기존 계정") },
            { "Username Đề Xuất", ("Suggested Username", "推奨ユーザー名", "建议用户名", "추천 사용자명") },
            { "Hợp Lệ", ("Eligible", "適格", "符合条件", "적격") },
            { "Ghi Chú Đánh Giá", ("Evaluation Notes", "評価備考", "评估备注", "평가 비고") },
            { "(Chưa có)", ("(None)", "(なし)", "(无)", "(없음)") },
            { "Tự động kích hoạt Mobile cho tài khoản mới tạo", ("Auto-activate Mobile for newly created accounts", "新規アカウントのモバイル自動有効化", "自动为新账号启用移动端", "새 계정에 대해 모바일 자동 활성화") },
            { "Chỉ lọc nhân sự chưa có tài khoản", ("Only filter employees without accounts", "アカウント未所持社員のみ絞り込み", "仅筛选未开通账号的员工", "계정이 없는 직원만 필터링") },
            { "Tải lại trang", ("Reload Page", "再読み込み", "重新加载", "페이지 새로고침") },

            // --- FrmDatabaseConfig ---
            { "Cấu Hình Kết Nối Cơ Sở Dữ Liệu", ("Database Connection Configuration", "データベース接続構成", "数据库连接配置", "데이터베이스 연결 구성") },
            { "Danh Sách Cấu Hình (Profiles)", ("Configuration Profiles", "構成プロファイル一覧", "配置配置文件列表", "구성 프로필 목록") },
            { "Thông Tin Kết Nối", ("Connection Details", "接続詳細情報", "连接详细信息", "연결 세부 정보") },
            { "Kiểu kết nối:", ("Connection Type:", "接続形式:", "连接类型：", "연결 유형:") },
            { "Kiểu DB:", ("DB Type:", "DB種別:", "数据库类型：", "DB 유형:") },
            { "Màu nhận diện:", ("Profile Color:", "識別カラー:", "识别颜色：", "프로필 색상:") },
            { "Địa chỉ Máy chủ (IP / Host):", ("Host / IP Address:", "ホスト/IPアドレス:", "主机/IP地址：", "호스트 / IP 주소:") },
            { "Cổng kết nối (Port):", ("Port:", "ポート番号:", "端口号：", "포트:") },
            { "Loại định danh:", ("Identifier Type:", "識別子種別:", "标识符类型：", "식별자 유형:") },
            { "Phương thức xác thực:", ("Authentication Method:", "認証方式:", "认证方式：", "인증 방식:") },
            { "Tài khoản kết nối:", ("Username:", "接続アカウント:", "连接用户名：", "연결 사용자:") },
            { "Mật khẩu kết nối:", ("Password:", "パスワード:", "连接密码：", "연결 비밀번호:") },
            { "Ghi nhớ mật khẩu", ("Remember Password", "パスワードを保存", "记住密码", "비밀번호 저장") },
            { "Thử kết nối", ("Test Connection", "接続テスト", "测试连接", "연결 테스트") },
            { "Áp dụng Profile", ("Apply Profile", "プロファイルを適用", "应用配置文件", "프로필 적용") },
            { "Kết nối cơ sở dữ liệu thành công!", ("Connected to database successfully!", "データベースに正常に接続しました！", "成功连接到数据库！", "데이터베이스에 성공적으로 연결되었습니다!") },
            { "Kết nối cơ sở dữ liệu thất bại!", ("Database connection failed!", "データベース接続に失敗しました！", "数据库连接失败！", "데이터베이스 연결에 실패했습니다!") }
        };

        static TranslationManager()
        {
            LoadLanguage();
        }

        
        public static string GetCurrentLanguageCode()
        {
            if (_currentLanguage == null) return "vi";
            if (_currentLanguage.Contains("Anh") || _currentLanguage.Contains("English")) return "en";
            if (_currentLanguage.Contains("Nh") || _currentLanguage.Contains("Japanese") || _currentLanguage.Contains("Nihongo")) return "ja";
            if (_currentLanguage.Contains("Trung") || _currentLanguage.Contains("Chinese")) return "zh";
            if (_currentLanguage.Contains("H") || _currentLanguage.Contains("Korean") || _currentLanguage.Contains("Han")) return "ko";
            return "vi";
        }

        public static void TranslateAllOpenForms()
        {
            if (System.Windows.Forms.Application.OpenForms == null) return;
            var forms = new System.Collections.ArrayList(System.Windows.Forms.Application.OpenForms);
            foreach (System.Windows.Forms.Form form in forms)
            {
                Translate(form);
            }
        }

        public static void LoadDictionaryFromDB()
        {
            try
            {
                using (var db = new DA.MyEntities())
                {
                    var records = System.Linq.Enumerable.ToList(System.Linq.Queryable.Where(db.TB_TRANSLATIONS, t => t.TABLE_NAME == "UI_LABEL"));
                    var grouped = System.Linq.Enumerable.GroupBy(records, t => t.COLUMN_NAME);
                    foreach (var group in grouped)
                    {
                        string viText = group.Key;
                        if (string.IsNullOrEmpty(viText)) continue;

                        string en = viText, ja = viText, zh = viText, ko = viText;
                        
                        // Láº¥y dá»¯ liá»u cÅ© tá»« fix cá»©ng Äá» KHÃNG lÃ m máº¥t cÃ¡c ngÃ´n ngá»¯ khÃ´ng cÃ³ trong DB
                        if (_dictionary.TryGetValue(viText, out var existing))
                        {
                            en = existing.English;
                            ja = existing.Japanese;
                            zh = existing.Chinese;
                            ko = existing.Korean;
                        }

                        foreach (var r in group)
                        {
                            if (r.LANGUAGE_CODE != null && !string.IsNullOrWhiteSpace(r.VALUE) && r.VALUE != viText)
                            {
                                if (r.LANGUAGE_CODE.ToUpper() == "EN") en = r.VALUE;
                                else if (r.LANGUAGE_CODE.ToUpper() == "JA") ja = r.VALUE;
                                else if (r.LANGUAGE_CODE.ToUpper() == "ZH") zh = r.VALUE;
                                else if (r.LANGUAGE_CODE.ToUpper() == "KO") ko = r.VALUE;
                            }
                        }

                        // Override or append database translation onto the hardcoded dictionary base
                        _dictionary[viText] = (en, ja, zh, ko);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading DB translations: " + ex.Message);
            }
        }

        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                SaveLanguage(value);
            }
        }

        public static string Translate(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (_currentLanguage == "Tiếng Việt") return text;

            string trimmed = text.Trim();
            bool endsWithColon = trimmed.EndsWith(":");
            string lookupKey = endsWithColon ? trimmed.Substring(0, trimmed.Length - 1).Trim() : trimmed;

            if (_dictionary.TryGetValue(lookupKey, out var val))
            {
                string translated = val.English;
                if (_currentLanguage == "Tiếng Anh") translated = val.English;
                else if (_currentLanguage == "Tiếng Nhật") translated = val.Japanese;
                else if (_currentLanguage == "Tiếng Trung") translated = val.Chinese;
                else if (_currentLanguage == "Tiếng Hàn") translated = val.Korean;

                // Match case of input string
                if (IsAllUpper(lookupKey))
                {
                    translated = translated.ToUpper();
                }

                return endsWithColon ? translated + ":" : translated;
            }

            return text;
        }

        private static bool IsAllUpper(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            bool hasLetter = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetter(input[i]))
                {
                    hasLetter = true;
                    if (char.IsLower(input[i])) return false;
                }
            }
            return hasLetter;
        }

        private static string GetOriginalText(object obj, string currentText)
        {
            if (!_originalTexts.TryGetValue(obj, out string original))
            {
                original = currentText;
                _originalTexts.Add(obj, original);
            }
            return original;
        }

        public static void Translate(Form form)
        {
            if (form == null) return;

            // Translate form title
            string origFormText = GetOriginalText(form, form.Text);
            string translatedFormText = Translate(origFormText);
            if (form.Text != translatedFormText)
            {
                form.Text = translatedFormText;
            }

            // Translate controls recursively
            TranslateControls(form.Controls);
        }

        private static void TranslateControls(Control.ControlCollection controls)
        {
            if (controls == null) return;

            foreach (Control ctrl in controls)
            {
                // Skip translating text of ComboBox and LookUpEdit controls to prevent selection change re-entrancy
                string typeName = ctrl.GetType().Name;
                if (typeName.EndsWith("LookUpEdit") || typeName.EndsWith("ComboBoxEdit") || typeName == "ComboBox" || ctrl is ComboBox || typeName.EndsWith("SpinEdit"))
                {
                    if (ctrl.Controls != null && ctrl.Controls.Count > 0)
                    {
                        TranslateControls(ctrl.Controls);
                    }
                    continue;
                }

                // KHÔNG dịch thuộc tính Text của các ô nhập liệu vì đó là nội dung người dùng gõ vào
                bool isInputControl = ctrl is TextBox || ctrl is DevExpress.XtraEditors.TextEdit || ctrl is DevExpress.XtraEditors.MemoEdit || ctrl is DevExpress.XtraEditors.BaseEdit;

                // Translate control text
                if (!isInputControl && !string.IsNullOrEmpty(ctrl.Text))
                {
                    string origText = GetOriginalText(ctrl, ctrl.Text);
                    string translatedText = Translate(origText);
                    if (ctrl.Text != translatedText)
                    {
                        ctrl.Text = translatedText;
                    }
                }

                // Handle specialized DevExpress components
                if (ctrl is DevExpress.XtraBars.Ribbon.RibbonControl ribbon)
                {
                    TranslateRibbon(ribbon);
                }
                else if (ctrl is DevExpress.XtraGrid.GridControl grid)
                {
                    TranslateGrid(grid);
                }
                else if (ctrl is DevExpress.XtraTab.XtraTabControl tabControl)
                {
                    foreach (DevExpress.XtraTab.XtraTabPage page in tabControl.TabPages)
                    {
                        string origPageText = GetOriginalText(page, page.Text);
                        string translatedPageText = Translate(origPageText);
                        if (page.Text != translatedPageText)
                        {
                            page.Text = translatedPageText;
                        }
                    }
                }
                else if (ctrl is DevExpress.XtraEditors.GroupControl groupControl)
                {
                    string origGroupText = GetOriginalText(groupControl, groupControl.Text);
                    string translatedGroupText = Translate(origGroupText);
                    if (groupControl.Text != translatedGroupText)
                    {
                        groupControl.Text = translatedGroupText;
                    }
                }
                else if (ctrl is DevExpress.XtraBars.BarDockControl barDockControl && barDockControl.Manager != null)
                {
                    TranslateBarManager(barDockControl.Manager);
                }
                else if (ctrl is DevExpress.XtraBars.Docking.DockPanel dockPanel)
                {
                    string origDockText = GetOriginalText(dockPanel, dockPanel.Text);
                    string translatedDockText = Translate(origDockText);
                    if (dockPanel.Text != translatedDockText)
                    {
                        dockPanel.Text = translatedDockText;
                    }
                }
                else if (ctrl is DevExpress.XtraEditors.CheckEdit checkEdit)
                {
                    if (checkEdit.Properties != null && !string.IsNullOrEmpty(checkEdit.Properties.Caption))
                    {
                        string origCaption = GetOriginalText(checkEdit.Properties, checkEdit.Properties.Caption);
                        string translatedCaption = Translate(origCaption);
                        if (checkEdit.Properties.Caption != translatedCaption)
                        {
                            checkEdit.Properties.Caption = translatedCaption;
                        }
                    }
                }
                else if (ctrl is DevExpress.XtraEditors.TextEdit textEdit)
                {
                    if (textEdit.Properties != null && !string.IsNullOrEmpty(textEdit.Properties.NullValuePrompt))
                    {
                        string origPrompt = GetOriginalText(textEdit.Properties, textEdit.Properties.NullValuePrompt);
                        string translatedPrompt = Translate(origPrompt);
                        if (textEdit.Properties.NullValuePrompt != translatedPrompt)
                        {
                            textEdit.Properties.NullValuePrompt = translatedPrompt;
                        }
                    }
                }
                else if (ctrl is DevExpress.XtraEditors.SearchLookUpEdit searchLookUp)
                {
                    if (searchLookUp.Properties != null && searchLookUp.Properties.PopupView != null)
                    {
                        TranslateGridView(searchLookUp.Properties.PopupView);
                    }
                }
                else if (ctrl is DevExpress.XtraEditors.GridLookUpEdit gridLookUp)
                {
                    if (gridLookUp.Properties != null && gridLookUp.Properties.PopupView != null)
                    {
                        TranslateGridView(gridLookUp.Properties.PopupView);
                    }
                }

                // Recurse children
                if (ctrl.Controls != null && ctrl.Controls.Count > 0)
                {
                    TranslateControls(ctrl.Controls);
                }
            }
        }

        
        private static void TranslateBarManager(DevExpress.XtraBars.BarManager manager)
        {
            if (manager == null) return;
            foreach (DevExpress.XtraBars.BarItem item in manager.Items)
            {
                if (!string.IsNullOrEmpty(item.Caption))
                {
                    string origCaption = GetOriginalText(item, item.Caption);
                    string translatedCaption = Translate(origCaption);
                    if (item.Caption != translatedCaption)
                    {
                        item.Caption = translatedCaption;
                    }
                }
            }
        }

        private static void TranslateRibbon(DevExpress.XtraBars.Ribbon.RibbonControl ribbon)
        {
            if (ribbon == null) return;

            // Translate Pages
            foreach (DevExpress.XtraBars.Ribbon.RibbonPage page in ribbon.Pages)
            {
                string origPageText = GetOriginalText(page, page.Text);
                string translatedPageText = Translate(origPageText);
                if (page.Text != translatedPageText)
                {
                    page.Text = translatedPageText;
                }

                // Translate Ribbon Page Groups
                foreach (DevExpress.XtraBars.Ribbon.RibbonPageGroup group in page.Groups)
                {
                    string origGroupText = GetOriginalText(group, group.Text);
                    string translatedGroupText = Translate(origGroupText);
                    if (group.Text != translatedGroupText)
                    {
                        group.Text = translatedGroupText;
                    }
                }
            }

            // Translate Items
            foreach (DevExpress.XtraBars.BarItem item in ribbon.Items)
            {
                if (!string.IsNullOrEmpty(item.Caption))
                {
                    string origCaption = GetOriginalText(item, item.Caption);
                    string translatedCaption = Translate(origCaption);
                    if (item.Caption != translatedCaption)
                    {
                        item.Caption = translatedCaption;
                    }
                }
            }
        }

        private static void TranslateGridView(DevExpress.XtraGrid.Views.Base.ColumnView gridView)
        {
            if (gridView == null) return;
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gridView.Columns)
            {
                if (!string.IsNullOrEmpty(col.Caption))
                {
                    string origCaption = GetOriginalText(col, col.Caption);
                    string translatedCaption = Translate(origCaption);
                    if (col.Caption != translatedCaption)
                    {
                        col.Caption = translatedCaption;
                    }
                }
            }

            if (gridView is DevExpress.XtraGrid.Views.Grid.GridView gv)
            {
                if (!string.IsNullOrEmpty(gv.OptionsFind.FindNullPrompt))
                {
                    string origPrompt = GetOriginalText(gv, gv.OptionsFind.FindNullPrompt);
                    string translatedPrompt = Translate(origPrompt);
                    if (gv.OptionsFind.FindNullPrompt != translatedPrompt)
                    {
                        gv.OptionsFind.FindNullPrompt = translatedPrompt;
                    }
                }
            }
        }

        private static void TranslateGrid(DevExpress.XtraGrid.GridControl grid)
        {
            if (grid == null) return;

            foreach (DevExpress.XtraGrid.Views.Base.BaseView view in grid.ViewCollection)
            {
                if (view is DevExpress.XtraGrid.Views.Grid.GridView gridView)
                {
                    TranslateGridView(gridView);
                }
            }
        }

        public static void LoadLanguage()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var settings = JsonConvert.DeserializeObject<SystemSettings>(json);
                    if (settings != null && !string.IsNullOrEmpty(settings.Language))
                    {
                        _currentLanguage = settings.Language;
                        return;
                    }
                }
            }
            catch
            {
                // Ignore load error, fallback to default
            }
            _currentLanguage = "Tiếng Việt";
        }

        private static void SaveLanguage(string language)
        {
            try
            {
                var settings = new SystemSettings { Language = language };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Ignore save error
            }
        }

        public static string GetCanonicalLanguage(string translatedName)
        {
            if (string.IsNullOrEmpty(translatedName)) return "Tiếng Việt";
            if (translatedName.Equals("Tiếng Việt", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("Vietnamese", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("ベトナム語", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("越南语", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("베트남어", StringComparison.OrdinalIgnoreCase))
            {
                return "Tiếng Việt";
            }
            if (translatedName.Equals("Tiếng Anh", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("English", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("英語", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("英语", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("영어", StringComparison.OrdinalIgnoreCase))
            {
                return "Tiếng Anh";
            }
            if (translatedName.Equals("Tiếng Nhật", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("Japanese", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("日本語", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("日语", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("일본어", StringComparison.OrdinalIgnoreCase))
            {
                return "Tiếng Nhật";
            }
            if (translatedName.Equals("Tiếng Trung", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("Chinese", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("中国語", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("中文", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("중국어", StringComparison.OrdinalIgnoreCase))
            {
                return "Tiếng Trung";
            }
            if (translatedName.Equals("Tiếng Hàn", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("Korean", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("韓国語", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("韩语", StringComparison.OrdinalIgnoreCase) ||
                translatedName.Equals("한국어", StringComparison.OrdinalIgnoreCase))
            {
                return "Tiếng Hàn";
            }
            return translatedName;
        }

        private class SystemSettings
        {
            public string Language { get; set; }
        }
    }
}
