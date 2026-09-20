import { useState, useEffect } from 'react';

export type AppLanguage = 'vi' | 'en' | 'ja';

const LANGUAGE_STORAGE_KEY = 'app_language';
const LANGUAGE_EVENT_NAME = 'hrms_language_change';

export const I18N_CONFIG = {
  vi: {
    name: 'Tiếng Việt',
    short: 'VI',
    flag: '🇻🇳',
  },
  en: {
    name: 'English',
    short: 'EN',
    flag: '🇬🇧',
  },
  ja: {
    name: '日本語',
    short: 'JA',
    flag: '🇯🇵',
  },
};

export const I18N_DICTIONARY = {
  vi: {
    // Top Landing Header & Landing Page
    landing: {
      systemReady: 'Hệ Thống Sẵn Sàng',
      galleryFilm: 'Gallery Phim',
      downloadApp: 'Tải Ứng Dụng',
      signIn: 'Đăng Nhập',
      subHeader: 'Giải Pháp Quản Trị Nhân Sự & Phân Quyền Doanh Nghiệp',
      badge: 'NỀN TẢNG QUẢN TRỊ NHÂN LỰC THẾ HỆ MỚI • TIÊU CHUẨN DOANH NGHIỆP',
      heroTitle: 'Hệ Thống Quản Trị Nhân Sự, Chấm Công & Tiền Lương',
      heroTitleHighlight: 'Toàn Diện',
      heroDesc: 'Hệ sinh thái phần mềm kết nối dữ liệu trực tiếp và xuyên suốt giữa ứng dụng Windows Desktop (WinForms) cho khối văn phòng, cổng Web Quản Trị Trực Tuyến và Ứng Dụng Di Động dành cho toàn thể cán bộ công nhân viên.',
      btnSignIn: 'Đăng Nhập Quản Trị',
      btnDownload: 'Tải Bộ Cài Đặt Ứng Dụng',

      // 4 Live Stats
      stat1_label: 'Độ sẵn sàng dịch vụ',
      stat2_label: 'Chuẩn hóa quy trình HR',
      stat3_label: 'Chấm công & tính lương',
      stat4_label: 'Mã hóa bảo mật đa tầng',

      // Ecosystem Multi-Platform
      ecoBadge: 'HỆ SINH THÁI ĐA NỀN TẢNG',
      ecoTitle: 'Sẵn Sàng Cho Mọi Thiết Bị Của Doanh Nghiệp',
      ecoSubtitle: 'Lựa chọn phương thức làm việc linh hoạt, tối ưu năng suất cho từng phòng ban.',
      
      winCardTitle: 'Bản Windows Desktop',
      winCardSub: 'C# .NET • Windows Desktop Enterprise',
      winCardDesc: 'Ứng dụng máy tính chuyên dụng cho Ban Giám đốc, Kế toán trưởng và Phòng Nhân sự. Hỗ trợ import/export Excel hàng loạt, in phiếu lương và phân quyền chức năng chi tiết.',
      winCardBtn: 'Tải Bộ Cài Đặt (v3.5.0 .zip)',
      winTagReady: 'v3.5.0 Sẵn sàng',

      webCardTitle: 'Cổng Web Trực Tuyến',
      webCardSub: 'React 19 • HTTPS SSL 256-bit',
      webCardDesc: 'Truy cập từ bất kỳ trình duyệt nào trên máy tính hoặc điện thoại. Xem biểu đồ trực quan, tính lương, quản lý hồ sơ và trò chuyện cùng Trợ lý ảo AI Copilot.',
      webCardBtn: 'Mở Cổng Quản Trị',
      webTagLive: 'Trực Tuyến (Live)',

      mobileCardTitle: 'HRMS Mobile App',
      mobileCardSub: 'iOS & Android (Cross-Platform)',
      mobileCardDesc: 'Chấm công bằng định vị vệ tinh GPS, nhận diện khuôn mặt AI, gửi đơn xin nghỉ phép tức thì và nhận thông báo phiếu lương trực tiếp về điện thoại nhân viên.',
      mobileCardBtn: 'App Store / CH Play',
      mobileTagComing: 'Sắp ra mắt',

      // Features Section
      featuresBadge: 'TÍNH NĂNG VƯỢT TRỘI',
      featuresTitle: 'Chuẩn Hóa Mọi Nghiệp Vụ Nhân Sự',
      f1_title: 'Quản Lý Hồ Sơ 360°',
      f1_desc: 'Quản lý đầy đủ sơ yếu lý lịch, hợp đồng lao động, bằng cấp, điều chuyển phòng ban, nâng lương và khen thưởng kỷ luật.',
      f2_title: 'Chấm Công Linh Hoạt',
      f2_desc: 'Hỗ trợ phân ca linh hoạt, chấm công chi tiết 31 ngày trong kỳ, ghi nhận tăng ca (OT) và tạm ứng lương tức thời.',
      f3_title: 'Tính Lương Tự Động',
      f3_desc: 'Động cơ tính lương tự động từ dữ liệu chấm công, khấu trừ BHXH, tạm ứng và hỗ trợ in phiếu thanh toán lương chuẩn.',
      f4_title: 'Phân Quyền Ma Trận RBAC',
      f4_desc: 'Đồng bộ chuẩn 100% theo kiến trúc kiểm soát quyền hạn theo vai trò. Hỗ trợ tạo nhóm quyền và khóa tài khoản an toàn.',
      f5_title: 'Trợ Lý AI Copilot',
      f5_desc: 'Tích hợp AI Copilot thông minh, hỗ trợ tra cứu luật lao động, chính sách nhân sự và tổng hợp số liệu bằng ngôn ngữ tự nhiên.',
      f6_title: 'Khen Thưởng & Kỷ Luật',
      f6_desc: 'Theo dõi lịch sử quyết định nâng lương, ban hành khen thưởng thành tích và xử lý vi phạm kỷ luật chính xác, minh bạch.',

      // Architecture Section
      archBadge: 'KIẾN TRÚC KỸ THUẬT & NHÀ PHÁT TRIỂN',
      archTitle: 'Hệ Thống Được Phát Triển Theo Tiêu Chuẩn Doanh Nghiệp',
      archDesc: 'Dự án được xây dựng dựa trên kiến trúc phân tầng (Multi-tier Enterprise Architecture), đảm bảo tính toàn vẹn dữ liệu, bảo mật tuyệt đối thông tin nhân sự và sẵn sàng mở rộng quy mô cho hàng nghìn nhân sự.',
      archReadyTitle: 'Sẵn sàng trải nghiệm?',
      archReadyDesc: 'Đăng nhập để vào ngay Cổng Quản Trị Hệ Thống.',
      archReadyBtn: 'Đăng Nhập Ngay',

      // Author / Marketing Section
      authorConnectBadge: 'KẾT NỐI & TRAO ĐỔI DỰ ÁN',
      authorConnectTitle: 'Liên Hệ Hợp Tác & Trao Đổi Kỹ Thuật',
      authorConnectDesc: 'Dự án HRMS được tôi nghiên cứu và hoàn thiện với đầy đủ các phân hệ quản lý nhân sự, chấm công, tính lương và cổng Web/Desktop. Nếu bạn có nhu cầu trao đổi kỹ thuật, tham khảo mã nguồn hoặc có dự án cần cộng tác phát triển, rất vui lòng được kết nối qua các kênh dưới đây:',
      authorEmailDirect: 'Email Trực Tiếp',
      authorFbProfile: 'Facebook Profile',
      authorGithubSource: 'GitHub Mã Nguồn',
      authorRoleTitle: 'Kỹ Sư Phát Triển Phần Mềm',
      authorBadgeText: 'Tác Giả Dự Án',
      authorBio: 'Lập trình viên phát triển phần mềm quản trị nhân sự và hệ thống doanh nghiệp với C# .NET, React và CSDL quan hệ.',

      // Footer
      footerLine1: '© 2026 HRMS ENTERPRISE SOLUTION • HỆ THỐNG QUẢN TRỊ NHÂN SỰ DOANH NGHIỆP',
      footerLine2: 'Cổng Thông Tin Doanh Nghiệp Hoạt Động Trên Nền Tảng Đám Mây An Toàn • SSL 256-bit Encrypted',

      // Login Modal
      loginModalTitle: 'ĐĂNG NHẬP HỆ THỐNG',
      loginModalSubtitle: 'Cổng Quản Trị Nhân Sự Trực Tuyến',
      loginModalSSLTag: 'Cổng Xác Thực Bảo Mật SSL',
      usernameLabel: 'Tên đăng nhập',
      usernamePlaceholder: 'Nhập tên đăng nhập...',
      usernameRequired: 'Vui lòng nhập tên tài khoản!',
      passwordLabel: 'Mật khẩu',
      passwordPlaceholder: 'Nhập mật khẩu...',
      passwordRequired: 'Vui lòng nhập mật khẩu!',
      btnLoginSubmit: 'Đăng nhập hệ thống',
      loggingIn: 'Đang xác thực...',
      loginSuccess: 'Đăng nhập thành công!',
      loginError: 'Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản hoặc mật khẩu.',
      loginSecurityFooter: '🔒 Bảo mật mã hóa SSL 256-bit • Tiêu chuẩn RFC 7519 JWT',

      // Download Modal
      downloadModalHeader: 'Thông Báo Bộ Cài Đặt Ứng Dụng',
      downloadWinAlertTitle: 'Bộ Cài Đặt Windows Desktop v3.5.0 Đã Sẵn Sàng',
      downloadWinAlertDesc: 'Gói cài đặt HRMS_Setup_v3.5.0.zip đã sẵn sàng. Bạn có thể nhấn tải trực tiếp về máy tính làm việc.',
      downloadMobileAlertTitle: 'Ứng dụng di động đang trong quá trình phát triển',
      downloadMobileAlertDesc: 'Phiên bản ứng dụng di động cho iOS & Android đang trong lộ trình phát triển và sẽ sớm có mặt trên App Store & Google Play.',
      downloadGeneralAlertTitle: 'Hệ Sinh Thái Ứng Dụng Doanh Nghiệp HRMS',
      downloadGeneralAlertDesc: 'Bản cài đặt Windows v3.5.0 đã sẵn sàng tải về. Ứng dụng di động đang trong lộ trình phát triển.',
      downloadWinSectionTitle: '🖥️ Ứng dụng Windows Desktop (.NET Enterprise)',
      downloadWinSectionDesc: 'Gói cài đặt chính thức HRMS_Setup_v3.5.0.zip đã sẵn sàng. Tương thích Windows 10, Windows 11 và Windows Server.',
      downloadWinBtnText: 'Tải Về Ngay: HRMS_Setup_v3.5.0.zip',
      downloadMobileSectionTitle: '📱 Ứng dụng Di Động (HRMS Mobile iOS / Android)',
      downloadMobileSectionDesc: 'Ứng dụng di động đang trong giai đoạn xây dựng. Hiện tại bạn có thể truy cập mượt mà trên trình duyệt điện thoại qua địa chỉ:',
      downloadMobileResponsiveNote: '(Giao diện đã tối ưu 100% cho màn hình cảm ứng)',
      downloadModalUnderstood: 'Đã hiểu',
    },

    // In-App Main Layout (Dashboard, Sidebar, Header, User Menu)
    app: {
      // Menu Sidebar
      menuDashboard: 'Bảng điều khiển',
      menuEmployees: 'Quản lý Nhân sự',
      menuAttendance: 'Chấm công & Ca làm',
      menuPayroll: 'Tính lương & Thuế',
      menuContracts: 'Hợp đồng lao động',
      menuRewards: 'Khen thưởng & Kỷ luật',
      menuPromotions: 'Nâng lương & Chuyển phòng',
      menuOvertime: 'Tăng ca & Ứng lương',
      menuPermissions: 'Người dùng & Phân quyền',
      menuAiCopilot: 'AI Copilot',
      aiBadge: 'AI Trợ Lý',

      // Header Page Titles
      titleDashboard: '📊 Tổng quan Doanh nghiệp',
      titleEmployees: '👥 Danh mục & Hồ sơ Nhân viên',
      titleAttendance: '🕒 Chấm công & Phân ca làm việc',
      titlePayroll: '💵 Quản lý Bảng lương & Thuế TNCN',
      titleContracts: '📄 Hồ sơ Hợp đồng lao động',
      titleRewards: '🏆 Khen thưởng & Xử lý Kỷ luật',
      titlePromotions: '📈 Quyết định Nâng lương & Điều chuyển',
      titleOvertime: '💸 Tăng ca & Tạm ứng lương',
      titlePermissions: '🔐 Quản trị Hệ thống & Phân quyền',

      // Toolbar Actions & Tooltips
      homeTooltip: 'Về trang chủ Dashboard (Home)',
      online: 'Trực tuyến',
      offline: 'Ngoại tuyến',
      systemOnlineTooltip: (periods: number) => `Hệ thống Trực tuyến (${periods} Kỳ công)`,
      systemOfflineTooltip: 'Hệ thống Ngoại tuyến',
      searchPlaceholder: 'Tìm kiếm HRMS...',
      searchTooltip: 'Tìm kiếm nhanh nhân sự, hợp đồng, kỳ lương (Phím tắt: Ctrl + K hoặc /)',
      quickAdd: 'Thao tác nhanh',
      addEmployee: 'Thêm nhân viên mới',
      createContract: 'Tạo hợp đồng lao động',
      createLeave: 'Tạo đơn nghỉ phép',
      enterTimesheet: 'Nhập bảng chấm công',
      calculateSalary: 'Tính bảng lương tự động',
      askAiCopilot: 'Hỏi AI Copilot',
      notificationsTooltip: 'Trung tâm Thông báo khẩn',
      refreshTooltip: 'Làm mới dữ liệu',
      userAccount: (username: string) => `Tài khoản: ${username}`,
      roleAdmin: 'Quyền: Super Admin',
      roleStaff: (count: number) => `Quyền: ${count} chức năng`,
      changePassword: 'Đổi mật khẩu cá nhân',
      logout: 'Đăng xuất',
      tagSuperAdmin: 'SUPER ADMIN',
      tagStaff: 'NHÂN VIÊN',
      languageSwitchTooltip: 'Đổi ngôn ngữ giao diện',
    },
  },

  en: {
    landing: {
      systemReady: 'System Ready',
      galleryFilm: 'Cinema Gallery',
      downloadApp: 'Download App',
      signIn: 'Sign In',
      subHeader: 'Enterprise HR Management & Authorization Platform',
      badge: 'NEXT-GENERATION WORKFORCE PLATFORM • ENTERPRISE GRADE',
      heroTitle: 'Human Resource Management, Timekeeping & Payroll',
      heroTitleHighlight: 'End-to-End',
      heroDesc: 'Unified software ecosystem connecting Windows Desktop (WinForms) for corporate offices, Web Management Portal, and Mobile Apps for all workforce members.',
      btnSignIn: 'Management Sign In',
      btnDownload: 'Download Software Suite',

      stat1_label: 'Service Availability',
      stat2_label: 'HR Workflow Standardization',
      stat3_label: 'Real-time Attendance & Pay',
      stat4_label: 'Multi-Tier Encryption',

      ecoBadge: 'MULTI-PLATFORM ECOSYSTEM',
      ecoTitle: 'Ready for Every Enterprise Device',
      ecoSubtitle: 'Flexible working methods designed to maximize departmental productivity.',

      winCardTitle: 'Windows Desktop Edition',
      winCardSub: 'C# .NET • Windows Desktop Enterprise',
      winCardDesc: 'Workstation software tailored for Executive Leadership, Chief Accountants, and HR departments. Supports mass Excel import/export, payslip printing, and granular RBAC.',
      winCardBtn: 'Download Installer (v3.5.0 .zip)',
      winTagReady: 'v3.5.0 Ready',

      webCardTitle: 'Web Management Portal',
      webCardSub: 'React 19 • HTTPS SSL 256-bit',
      webCardDesc: 'Instant browser access on desktops and phones. Live analytical charts, payroll processing, digital personnel archives, and conversational AI Copilot assistance.',
      webCardBtn: 'Launch Web Portal',
      webTagLive: 'Online (Live)',

      mobileCardTitle: 'HRMS Mobile App',
      mobileCardSub: 'iOS & Android (Cross-Platform)',
      mobileCardDesc: 'GPS satellite attendance check-in, AI face verification, instant leave request filing, and automated mobile payslip push notifications.',
      mobileCardBtn: 'App Store / Google Play',
      mobileTagComing: 'Coming Soon',

      featuresBadge: 'CORE CAPABILITIES',
      featuresTitle: 'Standardize Every Human Resources Workflow',
      f1_title: '360° Employee Profiles',
      f1_desc: 'Comprehensive resumes, contracts, certifications, departmental reassignments, salary adjustments, and disciplinary records.',
      f2_title: 'Flexible Attendance',
      f2_desc: 'Dynamic shift allocation, 31-day detailed attendance matrix, overtime (OT) tracking, and on-demand salary advances.',
      f3_title: 'Automated Payroll Engine',
      f3_desc: 'Automated payroll computation from live attendance logs, social insurance deductions, advances, and standard payslip printing.',
      f4_title: 'RBAC Matrix Security',
      f4_desc: 'Role-based access control architecture. Granular permission groups, security delegation, and safe user locking.',
      f5_title: 'AI Copilot Assistant',
      f5_desc: 'Integrated AI Copilot for labor law advisory, enterprise policy queries, and natural-language personnel data analytics.',
      f6_title: 'Rewards & Discipline',
      f6_desc: 'Transparent salary raise history, official recognition certificates, and disciplined compliance management.',

      archBadge: 'TECHNICAL ARCHITECTURE & DEVELOPER',
      archTitle: 'Engineered According to Enterprise Industry Standards',
      archDesc: 'Multi-tier enterprise architecture ensuring strict data integrity, absolute confidential personnel security, and scale readiness for thousands of employees.',
      archReadyTitle: 'Ready to experience?',
      archReadyDesc: 'Sign in to access the Enterprise Management Portal immediately.',
      archReadyBtn: 'Sign In Now',

      authorConnectBadge: 'CONNECT & COLLABORATE',
      authorConnectTitle: 'Contact for Technical Collaboration',
      authorConnectDesc: 'The HRMS platform has been developed with complete modules for personnel, time attendance, payroll, and unified Web/Desktop portals. For technical inquiries, source code review, or collaborative development, feel free to reach out via:',
      authorEmailDirect: 'Direct Email',
      authorFbProfile: 'Facebook Profile',
      authorGithubSource: 'GitHub Repository',
      authorRoleTitle: 'Software Engineer',
      authorBadgeText: 'Project Creator',
      authorBio: 'Software engineer building enterprise workforce systems and cloud solutions with C# .NET, React, and relational databases.',

      footerLine1: '© 2026 HRMS ENTERPRISE SOLUTION • ENTERPRISE WORKFORCE MANAGEMENT PLATFORM',
      footerLine2: 'Secure Enterprise Portal Running on Protected Cloud Infrastructure • SSL 256-bit Encrypted',

      loginModalTitle: 'ENTERPRISE SIGN IN',
      loginModalSubtitle: 'Online Human Resources Management Portal',
      loginModalSSLTag: 'SSL Encrypted Authentication Gate',
      usernameLabel: 'Username',
      usernamePlaceholder: 'Enter your username...',
      usernameRequired: 'Please input your username!',
      passwordLabel: 'Password',
      passwordPlaceholder: 'Enter your password...',
      passwordRequired: 'Please input your password!',
      btnLoginSubmit: 'Sign In to Portal',
      loggingIn: 'Authenticating...',
      loginSuccess: 'Sign-in successful!',
      loginError: 'Sign-in failed. Please verify your username and password.',
      loginSecurityFooter: '🔒 SSL 256-bit Encryption • RFC 7519 JWT Standard',

      downloadModalHeader: 'Client Application Setup Package',
      downloadWinAlertTitle: 'Windows Desktop v3.5.0 Setup Package Ready',
      downloadWinAlertDesc: 'HRMS_Setup_v3.5.0.zip is ready for download. You can download and run it directly on your PC workstation.',
      downloadMobileAlertTitle: 'Mobile application is currently in development',
      downloadMobileAlertDesc: 'Mobile app editions for iOS & Android are under development and will be available soon on App Store & Google Play.',
      downloadGeneralAlertTitle: 'HRMS Enterprise Multi-Platform Ecosystem',
      downloadGeneralAlertDesc: 'Windows v3.5.0 installer is available now. Mobile apps are actively in progress.',
      downloadWinSectionTitle: '🖥️ Windows Desktop App (.NET Enterprise)',
      downloadWinSectionDesc: 'Official release HRMS_Setup_v3.5.0.zip is ready. Compatible with Windows 10, Windows 11, and Windows Server.',
      downloadWinBtnText: 'Download Now: HRMS_Setup_v3.5.0.zip',
      downloadMobileSectionTitle: '📱 Mobile Application (HRMS Mobile iOS / Android)',
      downloadMobileSectionDesc: 'Mobile apps are in active development. You can seamlessly access the full responsive portal via phone browser:',
      downloadMobileResponsiveNote: '(Interface is 100% touch-optimized for mobile screens)',
      downloadModalUnderstood: 'Understood',
    },

    app: {
      menuDashboard: 'Dashboard',
      menuEmployees: 'Employee Directory',
      menuAttendance: 'Time & Attendance',
      menuPayroll: 'Payroll & Tax',
      menuContracts: 'Labor Contracts',
      menuRewards: 'Rewards & Discipline',
      menuPromotions: 'Promotions & Transfers',
      menuOvertime: 'Overtime & Advances',
      menuPermissions: 'Users & Permissions',
      menuAiCopilot: 'AI Copilot',
      aiBadge: 'AI Assistant',

      titleDashboard: '📊 Enterprise Overview',
      titleEmployees: '👥 Staff Directory & 360° Profiles',
      titleAttendance: '🕒 Attendance & Shift Scheduling',
      titlePayroll: '💵 Payroll & Personal Income Tax',
      titleContracts: '📄 Labor Contracts Archive',
      titleRewards: '🏆 Rewards & Disciplinary Actions',
      titlePromotions: '📈 Salary Adjustments & Transfers',
      titleOvertime: '💸 Overtime & Salary Advances',
      titlePermissions: '🔐 System Administration & RBAC',

      homeTooltip: 'Return to Dashboard (Home)',
      online: 'Online',
      offline: 'Offline',
      systemOnlineTooltip: (periods: number) => `System Online (${periods} Payroll Periods)`,
      systemOfflineTooltip: 'System Offline',
      searchPlaceholder: 'Search HRMS...',
      searchTooltip: 'Quick search for employees, contracts, payroll (Shortcut: Ctrl + K or /)',
      quickAdd: 'Quick Action',
      addEmployee: 'Add New Employee',
      createContract: 'Create Employment Contract',
      createLeave: 'Submit Leave Request',
      enterTimesheet: 'Input Timesheet Data',
      calculateSalary: 'Calculate Payroll Automatically',
      askAiCopilot: 'Ask AI Copilot',
      notificationsTooltip: 'Urgent Notification Center',
      refreshTooltip: 'Refresh System Data',
      userAccount: (username: string) => `Account: ${username}`,
      roleAdmin: 'Role: Super Admin',
      roleStaff: (count: number) => `Role: ${count} Permissions`,
      changePassword: 'Change Personal Password',
      logout: 'Log Out',
      tagSuperAdmin: 'SUPER ADMIN',
      tagStaff: 'STAFF',
      languageSwitchTooltip: 'Switch interface language',
    },
  },

  ja: {
    landing: {
      systemReady: 'システム稼働中',
      galleryFilm: 'シネマギャラリー',
      downloadApp: 'アプリ取得',
      signIn: 'ログイン',
      subHeader: 'エンタープライズ人事管理＆権限管理ソリューション',
      badge: '次世代人事労務プラットフォーム • エンタープライズ標準規格',
      heroTitle: '人事労務管理・勤怠打刻・給与計算システム',
      heroTitleHighlight: '総合ソリューション',
      heroDesc: 'オフィス専用 Windows デスクトップアプリ (WinForms)、Web管理ポータル、全従業員向けモバイルアプリを完全統合したシームレスな業務エコシステム。',
      btnSignIn: '管理ポータルにログイン',
      btnDownload: 'クライアントアプリ取得',

      stat1_label: 'サービス稼働率',
      stat2_label: '人事プロセスの標準化',
      stat3_label: 'リアルタイム勤怠＆給与',
      stat4_label: '多層暗号化セキュリティ',

      ecoBadge: 'マルチプラットフォーム・エコシステム',
      ecoTitle: 'あらゆる企業端末に対応',
      ecoSubtitle: '部門ごとの業務形態に合わせて最適な生産性向上を実現。',

      winCardTitle: 'Windows デスクトップ版',
      winCardSub: 'C# .NET • Windows デスクトップ Enterprise',
      winCardDesc: '経営幹部、総務・経理、人事部門向けの専用ワークステーション。Excel一括入出力、給与明細印刷、細やかな権限設定に対応。',
      winCardBtn: 'インストーラー取得 (v3.5.0 .zip)',
      winTagReady: 'v3.5.0 提供中',

      webCardTitle: 'Web 管理ポータル',
      webCardSub: 'React 19 • HTTPS SSL 256-bit',
      webCardDesc: 'PCやスマートフォンのブラウザから即時利用可能。動的グラフ分析、給与計算、人事台帳管理、AIコパイロット対話支援。',
      webCardBtn: 'Webポータルを開く',
      webTagLive: 'オンライン稼働中',

      mobileCardTitle: 'HRMS モバイルアプリ',
      mobileCardSub: 'iOS & Android (クロスプラットフォーム)',
      mobileCardDesc: 'GPS衛星位置情報による勤怠打刻、AI顔認証、有給申請、給与明細通知をスマートフォンへ直接配信。',
      mobileCardBtn: 'App Store / Google Play',
      mobileTagComing: '近日公開',

      featuresBadge: '主要機能一覧',
      featuresTitle: 'あらゆる人事労務業務を標準化',
      f1_title: '360° 人事台帳管理',
      f1_desc: '履歴書、労働契約書、保有資格、部署異動辞令、昇給履歴、表彰・懲戒処分を一元管理。',
      f2_title: 'スマート勤怠・シフト',
      f2_desc: '柔軟なシフト編成、月間31日勤怠マトリクス、残業(OT)実績、給与前払い申請に対応。',
      f3_title: '高精度自動給与計算',
      f3_desc: '勤怠実績からの自動給与算出、社会保険料控除、源泉所得税計算、標準明細書の印刷出力。',
      f4_title: '多層RBACアクセス制御',
      f4_desc: '役割ベースの厳格なアクセス制御。権限グループ設定、安全なアカウントロック機能を完備。',
      f5_title: 'AIコパイロット支援',
      f5_desc: '労働法規や就業規則の照会、自然言語による人事データ要約・集計を支援するインテリジェントAI。',
      f6_title: '表彰・懲戒・昇給異動',
      f6_desc: '定期昇給決定、優秀社員表彰、社内規律違反の公正かつ透明なコンプライアンス管理。',

      archBadge: '技術アーキテクチャ＆開発者',
      archTitle: 'エンタープライズ標準規格に準拠したシステム設計',
      archDesc: '多層エンタープライズ設計に基づき構築。完全なデータ整合性、機密人事情報の徹底保護、数千名規模へのスケーラビリティを担保。',
      archReadyTitle: '体験してみませんか？',
      archReadyDesc: '今すぐログインして管理ポータルをご利用ください。',
      archReadyBtn: '今すぐログイン',

      authorConnectBadge: 'プロジェクト連携・お問い合わせ',
      authorConnectTitle: '技術交流・開発協力のお問い合わせ',
      authorConnectDesc: '本HRMSプロジェクトは、人事、勤怠、給与計算、Web/Desktopポータルを完全網羅して独自開発されています。技術的な情報交換、ソースコードの参照、開発協力等のご相談は下記よりお気軽にご連絡ください：',
      authorEmailDirect: 'ダイレクトメール',
      authorFbProfile: 'Facebook プロファイル',
      authorGithubSource: 'GitHub リポジトリ',
      authorRoleTitle: 'ソフトウェアエンジニア',
      authorBadgeText: 'プロジェクト開発者',
      authorBio: 'C# .NET、React、リレーショナルDBを活用した人事労務管理システムおよび企業基幹システムの専門開発者。',

      footerLine1: '© 2026 HRMS ENTERPRISE SOLUTION • エンタープライズ人事労務管理システム',
      footerLine2: 'セキュアなクラウド基盤で稼働する企業専用ポータル • SSL 256-bit 暗号化保護',

      loginModalTitle: 'システムログイン',
      loginModalSubtitle: 'オンライン人事管理ポータル',
      loginModalSSLTag: 'SSL 暗号化認証ゲート',
      usernameLabel: 'ユーザー名',
      usernamePlaceholder: 'ユーザー名を入力...',
      usernameRequired: 'ユーザー名を入力してください！',
      passwordLabel: 'パスワード',
      passwordPlaceholder: 'パスワードを入力...',
      passwordRequired: 'パスワードを入力してください！',
      btnLoginSubmit: 'ログインする',
      loggingIn: '認証中...',
      loginSuccess: 'ログインに成功しました！',
      loginError: '認証に失敗しました。ユーザー名とパスワードをご確認ください。',
      loginSecurityFooter: '🔒 SSL 256-bit 暗号化 • RFC 7519 JWT 標準規格',

      downloadModalHeader: 'アプリケーション導入パッケージ',
      downloadWinAlertTitle: 'Windows デスクトップ版 v3.5.0 提供準備完了',
      downloadWinAlertDesc: 'HRMS_Setup_v3.5.0.zip のダウンロードが可能です。PCワークステーションに保存して直接実行できます。',
      downloadMobileAlertTitle: 'モバイルアプリは現在開発中です',
      downloadMobileAlertDesc: 'iOS & Android 版モバイルアプリは開発ロードマップに基づき進行中であり、まもなく公開予定です。',
      downloadGeneralAlertTitle: 'HRMS エンタープライズ マルチプラットフォーム',
      downloadGeneralAlertDesc: 'Windows版インストーラーは即時取得可能です。モバイル版は現在開発進行中です。',
      downloadWinSectionTitle: '🖥️ Windows デスクトップアプリ (.NET Enterprise)',
      downloadWinSectionDesc: '公式リリース HRMS_Setup_v3.5.0.zip。Windows 10、Windows 11、Windows Server に完全対応。',
      downloadWinBtnText: '今すぐダウンロード: HRMS_Setup_v3.5.0.zip',
      downloadMobileSectionTitle: '📱 モバイルアプリケーション (HRMS Mobile iOS / Android)',
      downloadMobileSectionDesc: '専用アプリを開発中です。現在はスマートフォンのブラウザから完全レスポンシブでご利用いただけます：',
      downloadMobileResponsiveNote: '(タッチ操作に100%最適化されたモバイルUI)',
      downloadModalUnderstood: '了解しました',
    },

    app: {
      menuDashboard: 'ダッシュボード',
      menuEmployees: '人事台帳・社員管理',
      menuAttendance: '勤怠・シフト管理',
      menuPayroll: '給与・所得税計算',
      menuContracts: '労働契約管理',
      menuRewards: '表彰・懲戒処分',
      menuPromotions: '昇給・部署異动',
      menuOvertime: '残業・給与前払い',
      menuPermissions: 'ユーザー・権限管理',
      menuAiCopilot: 'AIコパイロット',
      aiBadge: 'AIアシスタント',

      titleDashboard: '📊 企業ダッシュボード',
      titleEmployees: '👥 社員名簿・360°人事台帳',
      titleAttendance: '🕒 勤怠打刻・シフト編成',
      titlePayroll: '💵 給与計算・源泉所得税',
      titleContracts: '📄 労働契約書管理',
      titleRewards: '🏆 表彰・懲戒処分記録',
      titlePromotions: '📈 昇給辞令・人事異動',
      titleOvertime: '💸 時間外労働・前払い申請',
      titlePermissions: '🔐 システム管理＆RBAC権限設定',

      homeTooltip: 'ダッシュボードへ戻る (Home)',
      online: 'オンライン',
      offline: 'オフライン',
      systemOnlineTooltip: (periods: number) => `システムオンライン (${periods} 給与計算期間)`,
      systemOfflineTooltip: 'システムオフライン',
      searchPlaceholder: 'HRMSを検索...',
      searchTooltip: '社員、契約書、給与期間を高速検索 (ショートカット: Ctrl + K または /)',
      quickAdd: 'クイック作成',
      addEmployee: '新規社員を登録',
      createContract: '労働契約書を作成',
      createLeave: '休暇申請を起票',
      enterTimesheet: '勤怠実績を入力',
      calculateSalary: '給与自動計算を実行',
      askAiCopilot: 'AIコパイロットに質問',
      notificationsTooltip: '重要通知センター',
      refreshTooltip: 'データを最新化',
      userAccount: (username: string) => `アカウント: ${username}`,
      roleAdmin: '権限: システム管理者 (Super Admin)',
      roleStaff: (count: number) => `権限: ${count} 機能利用可能`,
      changePassword: '個人パスワード変更',
      logout: 'ログアウト',
      tagSuperAdmin: 'SUPER ADMIN',
      tagStaff: '一般社員',
      languageSwitchTooltip: '表示言語の切り替え',
    },
  },
};

export const getSavedLanguage = (): AppLanguage => {
  if (typeof window === 'undefined') return 'vi';
  const saved = localStorage.getItem(LANGUAGE_STORAGE_KEY);
  if (saved === 'vi' || saved === 'en' || saved === 'ja') return saved;
  return 'vi';
};

export const setSavedLanguage = (lang: AppLanguage): void => {
  if (typeof window === 'undefined') return;
  localStorage.setItem(LANGUAGE_STORAGE_KEY, lang);
  window.dispatchEvent(new CustomEvent(LANGUAGE_EVENT_NAME, { detail: lang }));
};

export function useAppLanguage() {
  const [lang, setLang] = useState<AppLanguage>(getSavedLanguage);

  useEffect(() => {
    const handleLangChange = (e: Event) => {
      const customEvent = e as CustomEvent<AppLanguage>;
      if (customEvent.detail) {
        setLang(customEvent.detail);
      } else {
        setLang(getSavedLanguage());
      }
    };

    window.addEventListener(LANGUAGE_EVENT_NAME, handleLangChange);
    window.addEventListener('storage', handleLangChange);

    return () => {
      window.removeEventListener(LANGUAGE_EVENT_NAME, handleLangChange);
      window.removeEventListener('storage', handleLangChange);
    };
  }, []);

  const changeLanguage = (newLang: AppLanguage) => {
    setLang(newLang);
    setSavedLanguage(newLang);
  };

  return {
    lang,
    setLang: changeLanguage,
    tLanding: I18N_DICTIONARY[lang].landing,
    tApp: I18N_DICTIONARY[lang].app,
    config: I18N_CONFIG[lang],
    allConfigs: I18N_CONFIG,
  };
}
