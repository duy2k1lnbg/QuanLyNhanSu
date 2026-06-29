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
                        Translate(form);
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
            { "Thêm", ("Add", "追加", "添加", "추가") },
            { "Sửa", ("Edit", "編集", "编辑", "수정") },
            { "Xóa", ("Delete", "削除", "删除", "삭제") },
            { "Làm Mới", ("Refresh", "更新", "刷新", "새로고침") },
            { "In", ("Print", "印刷", "打印", "인쇄") },
            { "Xuất Excel", ("Export Excel", "Excel出力", "导出Excel", "Excel 내보내기") },
            { "Tìm Kiếm", ("Search", "検索", "搜索", "검색") },

            // FrmSetting
            { "Cấu Hinh Hệ Thống", ("System Configuration", "システム構成", "系统配置", "시스템 구성") },
            { "Cấu Hình Hệ Thống", ("System Configuration", "システム構成", "系统配置", "시스템 구성") },
            { "Ngôn Ngữ Hệ Thống", ("System Language", "システム言語", "系统语言", "시스템 언어") },

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
            { "Đã lưu cài đặt ngôn ngữ hệ thống", ("Saved system language setting", "システム言語設定を保存しました", "已保存系统语言设置", "시스템 언어 설정이 저장되었습니다") }
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
                if (typeName.EndsWith("LookUpEdit") || typeName.EndsWith("ComboBoxEdit") || typeName == "ComboBox" || ctrl is ComboBox)
                {
                    if (ctrl.Controls != null && ctrl.Controls.Count > 0)
                    {
                        TranslateControls(ctrl.Controls);
                    }
                    continue;
                }

                // Translate control text
                if (!string.IsNullOrEmpty(ctrl.Text))
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
