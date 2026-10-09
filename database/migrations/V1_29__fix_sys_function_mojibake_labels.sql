-- =========================================================================================
-- Migration: V1_29__fix_sys_function_mojibake_labels.sql
-- Description: Chuẩn hóa nhãn tiếng Việt cho bảng HR.TB_SYS_FUNCTION, khắc phục lỗi mã hóa (mojibake).
-- Author: HRMS Enterprise Platform
-- Date: 2026-10-08
-- Guard: Chỉ cập nhật các dòng có ký tự lỗi hoặc sai lệch nhãn; kiểm tra row count trước và sau.
-- =========================================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

DECLARE
    v_total_updated NUMBER := 0;
    v_mojibake_count NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- Bắt đầu rà soát và khắc phục nhãn mojibake trong HR.TB_SYS_FUNCTION ---');

    -- Đếm số lượng dòng nghi vấn mojibake trước khi xử lý
    SELECT COUNT(*) INTO v_mojibake_count
    FROM HR.TB_SYS_FUNCTION
    WHERE DESCRIPTION LIKE '%Ä%'
       OR DESCRIPTION LIKE '%á»%'
       OR DESCRIPTION LIKE '%áº%'
       OR DESCRIPTION LIKE '%Ã%'
       OR DESCRIPTION LIKE '%Æ%';

    DBMS_OUTPUT.PUT_LINE('Số dòng phát hiện chuỗi ký tự lỗi encoding ban đầu: ' || v_mojibake_count);

    -- Cập nhật danh mục chuẩn hóa cho 58 chức năng hệ thống
    MERGE INTO HR.TB_SYS_FUNCTION tgt
    USING (
        SELECT 'F_LOGIN_DESKTOP' AS c, 'Đăng nhập Desktop' AS d, 'LOGIN' AS p FROM DUAL UNION ALL
        SELECT 'F_LOGIN_WEB', 'Đăng nhập Website', 'LOGIN' FROM DUAL UNION ALL
        SELECT 'F_LOGIN_MOBILE', 'Đăng nhập Mobile', 'LOGIN' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_GROUP', 'Nhóm Người Dùng', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_USER', 'Quản Lý Tài Khoản', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_CAPTAIKHOAN', 'Cấp Tài Khoản Hàng Loạt', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_LOCK_USER', 'Khóa/Mở Khóa Tài Khoản', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_PQ_CHUCNANG', 'Phân Quyền Chức Năng', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_PQ_BAOCAO', 'Phân Quyền Báo Cáo', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_THONGBAO', 'Thông Báo Hệ Thống', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_SAULUU', 'Sao Lưu Dữ Liệu', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_PHUCHOI', 'Phục Hồi Dữ Liệu', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_GIAMSAT', 'Giám Sát Đăng Nhập', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_AI', 'Trợ Lý AI & Chatbot', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_SETTING', 'Cấu Hình Ngôn Ngữ & Hệ Thống', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_AI_CONFIG', 'Cấu Hình AI Server (Ollama)', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_SYSTEM_DB_CONFIG', 'Cấu Hình Kết Nối CSDL', 'SYSTEM' FROM DUAL UNION ALL
        SELECT 'F_DB_NHANSU', 'Dashboard Nhân Sự', 'DASHBOARD' FROM DUAL UNION ALL
        SELECT 'F_DB_LUONG', 'Dashboard Lương', 'DASHBOARD' FROM DUAL UNION ALL
        SELECT 'F_BC_BAOCAO', 'Báo Cáo Tổng Hợp & Chi Tiết', 'DASHBOARD' FROM DUAL UNION ALL
        SELECT 'F_DM_DANTOC', 'Dân Tộc', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_TONGIAO', 'Tôn Giáo', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_TRINHDO', 'Trình Độ', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_NHANVIEN', 'Hồ Sơ Nhân Viên', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_PHONGBAN', 'Phòng Ban', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_BOPHAN', 'Bộ Phận', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_CONGTY', 'Công Ty', 'DM' FROM DUAL UNION ALL
        SELECT 'F_DM_CHUCVU', 'Chức Vụ', 'DM' FROM DUAL UNION ALL
        SELECT 'F_NV_HOPDONG', 'Hợp Đồng Lao Động', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_LOAIHOPDONG', 'Loại Hợp Đồng', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_NANGLUONG', 'Lên Lương Nhân Viên', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_KHENTHUONG', 'Khen Thưởng', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_KYLUAT', 'Kỷ Luật', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_DIEUCHUYEN', 'Điều Chuyển Nhân Viên', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_THOIVIEC', 'Thôi Việc', 'NV' FROM DUAL UNION ALL
        SELECT 'F_NV_PHEDUYET', 'Phê Duyệt Yêu Cầu (Online)', 'NV' FROM DUAL UNION ALL
        SELECT 'F_CC_LOAICA', 'Loại Ca', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_LOAICONG', 'Loại Công', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_NGAYLE', 'Ngày Lễ', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_PHUCAP', 'Phụ Cấp Nhân Viên', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_TANGCA', 'Tăng Ca', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_UNGLUONG', 'Ứng Lương', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_BANGCONG', 'Quản Lý Bảng Công', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_BCCT', 'Bảng Công Chi Tiết', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_BCCT_IN', 'In Bảng Công Nhân Viên', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_CAPNHATCONG', 'Cập Nhật Ngày Công', 'CC' FROM DUAL UNION ALL
        SELECT 'F_CC_BANGLUONG', 'Bảng Lương', 'CC' FROM DUAL UNION ALL
        SELECT 'MOBILE_ROOT', 'Phân Hệ Mobile App', 'MOBILE' FROM DUAL UNION ALL
        SELECT 'MOBILE_PROFILE_VIEW', 'Xem Hồ Sơ Cá Nhân Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_ATTENDANCE_VIEW', 'Xem Bảng Công Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_PAYROLL_VIEW', 'Xem Bảng Lương Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_CONTRACT_VIEW', 'Xem Hợp Đồng Lao Động Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_INSURANCE_VIEW', 'Xem Bảo Hiểm Xã Hội Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_NOTIFICATION_VIEW', 'Xem Thông Báo Nội Bộ Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_REQUEST_LEAVE', 'Gửi Đơn Nghỉ Phép Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_REQUEST_OVERTIME', 'Gửi Đơn Tăng Ca Mobile', 'MOBILE_ROOT' FROM DUAL UNION ALL
        SELECT 'MOBILE_REQUEST_ADVANCE', 'Gửi Yêu Cầu Ứng Lương Mobile', 'MOBILE_ROOT' FROM DUAL
    ) src
    ON (tgt.FUNCTION_CODE = src.c)
    WHEN MATCHED THEN
        UPDATE SET tgt.DESCRIPTION = src.d
        WHERE tgt.DESCRIPTION != src.d
           OR tgt.DESCRIPTION LIKE '%Ä%'
           OR tgt.DESCRIPTION LIKE '%á»%'
           OR tgt.DESCRIPTION LIKE '%áº%'
           OR tgt.DESCRIPTION LIKE '%Ã%';

    v_total_updated := SQL%ROWCOUNT;
    DBMS_OUTPUT.PUT_LINE('Số dòng đã cập nhật chuẩn hóa nhãn: ' || v_total_updated);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('--- Hoàn tất thành công migration V1_29 ---');
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('LỖI KHI THỰC HIỆN MIGRATION: ' || SQLERRM);
        RAISE;
END;
/
