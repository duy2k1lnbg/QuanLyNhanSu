-- ==============================================================================
-- Migration V1_12: Liên Kết Tài Khoản Người Dùng Với Nhân Viên & Phân Quyền Mobile
-- Version: V1_12
-- Description: Bổ sung cột MANV, CLIENT_TYPE vào TB_SYS_USER và thêm quyền Mobile
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR CONTINUE;

-- 1. Bổ sung cột MANV và CLIENT_TYPE vào TB_SYS_USER nếu chưa có
DECLARE
    v_col_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_col_count 
    FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'MANV';
    
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (MANV NUMBER)';
    END IF;

    SELECT COUNT(*) INTO v_col_count 
    FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'CLIENT_TYPE';
    
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (CLIENT_TYPE NVARCHAR2(20) DEFAULT ''ALL'')';
    END IF;
END;
/

-- 2. Thêm ràng buộc Khóa Ngoại và Ràng Buộc Duy Nhất (Unique Index) cho MANV
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_constraints WHERE constraint_name = 'FK_SYS_USER_NV';
    IF v_cnt = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD CONSTRAINT FK_SYS_USER_NV FOREIGN KEY (MANV) REFERENCES TB_NHANVIEN(MANV) ON DELETE SET NULL';
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_indexes WHERE index_name = 'UQ_SYS_USER_MANV';
    IF v_cnt = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UQ_SYS_USER_MANV ON TB_SYS_USER(CASE WHEN MANV IS NOT NULL THEN MANV END)';
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END IF;
END;
/

-- 3. Thêm các chức năng phân quyền Mobile vào TB_SYS_FUNCTION
DECLARE
    PROCEDURE add_func(p_code VARCHAR2, p_desc VARCHAR2, p_sort NUMBER, p_parent VARCHAR2) IS
        v_cnt NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cnt FROM TB_SYS_FUNCTION WHERE FUNCTION_CODE = p_code;
        IF v_cnt = 0 THEN
            INSERT INTO TB_SYS_FUNCTION (FUNCTION_CODE, DESCRIPTION, SORT, PARENT, ISGROUP, MENU, TIPS)
            VALUES (p_code, p_desc, p_sort, p_parent, 0, 1, p_desc);
        END IF;
    END;
BEGIN
    -- Nhóm chức năng Mobile
    add_func('MOBILE_ROOT',              'Phân hệ Mobile App',            200, NULL);
    add_func('MOBILE_PROFILE_VIEW',      'Xem hồ sơ cá nhân Mobile',      201, 'MOBILE_ROOT');
    add_func('MOBILE_ATTENDANCE_VIEW',   'Xem bảng công Mobile',          202, 'MOBILE_ROOT');
    add_func('MOBILE_PAYROLL_VIEW',      'Xem bảng lương Mobile',         203, 'MOBILE_ROOT');
    add_func('MOBILE_CONTRACT_VIEW',     'Xem hợp đồng lao động Mobile',  204, 'MOBILE_ROOT');
    add_func('MOBILE_INSURANCE_VIEW',    'Xem bảo hiểm xã hội Mobile',    205, 'MOBILE_ROOT');
    add_func('MOBILE_NOTIFICATION_VIEW', 'Xem thông báo nội bộ Mobile',   206, 'MOBILE_ROOT');
    COMMIT;
END;
/

-- 4. Đồng bộ quyền Mobile cho toàn bộ người dùng hiện có
MERGE INTO TB_SYS_RIGHT r
USING (
    SELECT u.IDUSER, f.FUNCTION_CODE
    FROM TB_SYS_USER u
    CROSS JOIN TB_SYS_FUNCTION f
    WHERE f.FUNCTION_CODE LIKE 'MOBILE_%'
) src
ON (r.IDUSER = src.IDUSER AND r.FUNCTION_CODE = src.FUNCTION_CODE)
WHEN NOT MATCHED THEN
    INSERT (IDUSER, FUNCTION_CODE, USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
    VALUES (src.IDUSER, src.FUNCTION_CODE, 1, 1, 0, 0, 0, 0);

COMMIT;
