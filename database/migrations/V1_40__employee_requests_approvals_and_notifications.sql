-- ==============================================================================
-- Migration V1_40: Employee Requests, Approvals, Profile Quota & Notifications
-- File: database/migrations/V1_40__employee_requests_approvals_and_notifications.sql
-- Description:
--   1. Tạo bảng TB_YEUCAU_UNGLUONG: Lưu đơn xin tạm ứng lương của nhân viên,
--      chỉ ghi nhận vào TB_UNGLUONG khi có thao tác xác nhận chi của Kế toán.
--   2. Tạo bảng TB_EMPLOYEE_EDIT_QUOTA: Quản lý số lần sửa trực tiếp theo (MANV, GROUP_CODE).
--      Lần đầu lưu trực tiếp, lần 2 trở đi chuyển sang đơn đề xuất chờ duyệt.
--   3. Tạo bảng TB_YEUCAU_THAYDOI_HOSO: Lưu đơn đề xuất thay đổi thông tin cá nhân,
--      bảo hiểm, người phụ thuộc và ảnh đại diện (avatar preview blob).
--   4. Tạo bảng TB_THONGBAO_NGUOIDUNG: Quản lý trạng thái đọc/chưa đọc thông báo theo User.
--   5. Bổ sung các mã quyền mới vào TB_SYS_FUNCTION, TB_SYS_RIGHT, TB_SYS_RIGHT_CHANNEL.
--   6. Cập nhật SCHEMA_VERSION lên 1.40.
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

PROMPT [MIGRATION V1_40] Starting Employee Requests, Approvals & Notifications Migration...

-- ------------------------------------------------------------------------------
-- 1. BẢNG TB_YEUCAU_UNGLUONG VÀ SEQUENCE
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_YEUCAU_UNGLUONG';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE SEQUENCE HR.SEQ_YEUCAU_UNGLUONG START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_YEUCAU_UNGLUONG');
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'TB_YEUCAU_UNGLUONG';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE HR.TB_YEUCAU_UNGLUONG (
            ID              NUMBER NOT NULL,
            MANV            NUMBER NOT NULL,
            SOTIEN          NUMBER(15, 2) NOT NULL,
            NGAY_DENGHI     DATE NOT NULL,
            THANG           NUMBER(2) NOT NULL,
            NAM             NUMBER(4) NOT NULL,
            LYDO            NVARCHAR2(1000) NOT NULL,
            TRANGTHAI       VARCHAR2(30) DEFAULT ''PENDING'' NOT NULL,
            NGUOIDUYET      NUMBER,
            NGAYDUYET       DATE,
            GHICHUDUYET     NVARCHAR2(1000),
            NGUOI_CHI       NUMBER,
            NGAY_CHI        DATE,
            SO_CHUNG_TU     NVARCHAR2(100),
            GHI_CHU_CHI     NVARCHAR2(1000),
            IDUL_LINK       NUMBER,
            CREATED_AT      DATE DEFAULT SYSDATE NOT NULL,
            CREATED_BY      NUMBER,
            CONSTRAINT PK_YEUCAU_UNGLUONG PRIMARY KEY (ID),
            CONSTRAINT CK_YC_UNGLUONG_STATUS CHECK (TRANGTHAI IN (''PENDING'', ''APPROVED'', ''DISBURSED'', ''REJECTED'', ''CANCELLED'')),
            CONSTRAINT CK_YC_UNGLUONG_SOTIEN CHECK (SOTIEN > 0),
            CONSTRAINT UQ_YC_UNGLUONG_LINK UNIQUE (IDUL_LINK)
        )';
        DBMS_OUTPUT.PUT_LINE('Created TABLE TB_YEUCAU_UNGLUONG');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 2. BẢNG TB_EMPLOYEE_EDIT_QUOTA VÀ SEQUENCE
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_EMPLOYEE_EDIT_QUOTA';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE SEQUENCE HR.SEQ_EMPLOYEE_EDIT_QUOTA START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_EMPLOYEE_EDIT_QUOTA');
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'TB_EMPLOYEE_EDIT_QUOTA';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE HR.TB_EMPLOYEE_EDIT_QUOTA (
            ID                  NUMBER NOT NULL,
            MANV                NUMBER NOT NULL,
            GROUP_CODE          VARCHAR2(30) NOT NULL,
            DIRECT_EDIT_COUNT   NUMBER DEFAULT 0 NOT NULL,
            FIRST_EDIT_DATE     DATE,
            LAST_EDIT_DATE      DATE,
            CONSTRAINT PK_EMPLOYEE_EDIT_QUOTA PRIMARY KEY (ID),
            CONSTRAINT UQ_EMPLOYEE_EDIT_QUOTA UNIQUE (MANV, GROUP_CODE),
            CONSTRAINT CK_EMP_QUOTA_GROUP CHECK (GROUP_CODE IN (''PERSONAL_INFO'', ''INSURANCE_INFO'', ''DEPENDENTS'', ''AVATAR''))
        )';
        DBMS_OUTPUT.PUT_LINE('Created TABLE TB_EMPLOYEE_EDIT_QUOTA');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 3. BẢNG TB_YEUCAU_THAYDOI_HOSO VÀ SEQUENCE
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_YEUCAU_THAYDOI_HOSO';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE SEQUENCE HR.SEQ_YEUCAU_THAYDOI_HOSO START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_YEUCAU_THAYDOI_HOSO');
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'TB_YEUCAU_THAYDOI_HOSO';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE HR.TB_YEUCAU_THAYDOI_HOSO (
            ID              NUMBER NOT NULL,
            MANV            NUMBER NOT NULL,
            GROUP_CODE      VARCHAR2(30) NOT NULL,
            REQ_TYPE        VARCHAR2(50) NOT NULL,
            CURRENT_DATA    NCLOB,
            PROPOSED_DATA   NCLOB,
            PROPOSED_BLOB   BLOB,
            BASE_VERSION    VARCHAR2(100),
            STATUS          VARCHAR2(30) DEFAULT ''PENDING'' NOT NULL,
            REASON          NVARCHAR2(1000),
            REJECT_REASON   NVARCHAR2(1000),
            RESOLVED_BY     NUMBER,
            RESOLVED_AT     DATE,
            APPLIED_AT      DATE,
            APPLIED_DATA    NCLOB,
            CREATED_AT      DATE DEFAULT SYSDATE NOT NULL,
            CREATED_BY      NUMBER,
            CONSTRAINT PK_YEUCAU_THAYDOI_HOSO PRIMARY KEY (ID),
            CONSTRAINT CK_YC_HOSO_GROUP CHECK (GROUP_CODE IN (''PERSONAL_INFO'', ''INSURANCE_INFO'', ''DEPENDENTS'', ''AVATAR'')),
            CONSTRAINT CK_YC_HOSO_STATUS CHECK (STATUS IN (''PENDING'', ''APPROVED'', ''REJECTED'', ''APPLIED'', ''CANCELLED''))
        )';
        DBMS_OUTPUT.PUT_LINE('Created TABLE TB_YEUCAU_THAYDOI_HOSO');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 4. BẢNG TB_THONGBAO_NGUOIDUNG VÀ SEQUENCE
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_THONGBAO_NGUOIDUNG';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE SEQUENCE HR.SEQ_THONGBAO_NGUOIDUNG START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_THONGBAO_NGUOIDUNG');
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'TB_THONGBAO_NGUOIDUNG';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE HR.TB_THONGBAO_NGUOIDUNG (
            ID              NUMBER NOT NULL,
            ID_THONGBAO     NUMBER NOT NULL,
            IDUSER          NUMBER NOT NULL,
            MANV            NUMBER,
            IS_READ         NUMBER(1) DEFAULT 0 NOT NULL,
            READ_AT         DATE,
            CONSTRAINT PK_THONGBAO_NGUOIDUNG PRIMARY KEY (ID),
            CONSTRAINT UQ_THONGBAO_NGUOIDUNG UNIQUE (ID_THONGBAO, IDUSER),
            CONSTRAINT CK_TB_NGUOIDUNG_READ CHECK (IS_READ IN (0, 1)),
            CONSTRAINT FK_TB_NGUOIDUNG_TB FOREIGN KEY (ID_THONGBAO) REFERENCES HR.TB_THONGBAO(ID) ON DELETE CASCADE
        )';
        DBMS_OUTPUT.PUT_LINE('Created TABLE TB_THONGBAO_NGUOIDUNG');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 5. BỔ SUNG CÁC QUYỀN MỚI VÀO REGISTRY PHÂN QUYỀN
-- ------------------------------------------------------------------------------
DECLARE
    PROCEDURE add_func(p_code VARCHAR2, p_name NVARCHAR2, p_right_type VARCHAR2 DEFAULT 'FUNCTION') IS
        v_exists NUMBER := 0;
    BEGIN
        SELECT COUNT(*) INTO v_exists FROM HR.TB_SYS_FUNCTION WHERE FUNCTION_CODE = p_code;
        IF v_exists = 0 THEN
            INSERT INTO HR.TB_SYS_FUNCTION (FUNCTION_CODE, DESCRIPTION, RIGHT_TYPE, ISGROUP)
            VALUES (p_code, p_name, p_right_type, 0);
            DBMS_OUTPUT.PUT_LINE('Added Function: ' || p_code);
        END IF;
    END;
BEGIN
    add_func('F_APPROVE_ADVANCE',           N'Phê duyệt đơn tạm ứng lương',            'FUNCTION');
    add_func('F_PAYROLL_DISBURSE_ADVANCE',  N'Xác nhận chi tiền tạm ứng lương',       'FUNCTION');
    add_func('F_APPROVE_PROFILE',           N'Phê duyệt thay đổi hồ sơ nhân viên',     'FUNCTION');
    add_func('MOBILE_PROFILE_EDIT',         N'Đề xuất / Cập nhật hồ sơ cá nhân Mobile','FUNCTION');
    add_func('MOBILE_REQUEST_INSURANCE',    N'Đề xuất thay đổi bảo hiểm Mobile',       'FUNCTION');
    add_func('MOBILE_REQUEST_DEPENDENT',    N'Đề xuất người phụ thuộc Mobile',         'FUNCTION');
    COMMIT;
END;
/

-- Cấp quyền mặc định cho tài khoản quản trị (ADMIN, IDUSER = 1 hoặc USERNAME = 'admin')
DECLARE
    v_admin_id NUMBER := 1;
BEGIN
    SELECT MIN(IDUSER) INTO v_admin_id FROM HR.TB_SYS_USER WHERE LOWER(USERNAME) = 'admin';
    IF v_admin_id IS NULL THEN
        v_admin_id := 1;
    END IF;

    -- Cấp toàn quyền trên TB_SYS_RIGHT
    FOR r IN (
        SELECT FUNCTION_CODE FROM HR.TB_SYS_FUNCTION 
        WHERE FUNCTION_CODE IN (
            'F_APPROVE_ADVANCE', 'F_PAYROLL_DISBURSE_ADVANCE', 'F_APPROVE_PROFILE',
            'MOBILE_REQUEST_ADVANCE', 'MOBILE_PROFILE_EDIT', 'MOBILE_REQUEST_INSURANCE', 'MOBILE_REQUEST_DEPENDENT'
        )
    ) LOOP
        MERGE INTO HR.TB_SYS_RIGHT tgt
        USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID FROM DUAL) src
        ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID)
        WHEN MATCHED THEN
            UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1, USER_RIGHT = 1
        WHEN NOT MATCHED THEN
            INSERT (FUNCTION_CODE, IDUSER, USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.FC, src.U_ID, 1, 1, 1, 1, 1, 1);
            
        -- Kênh DESKTOP
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID, 'DESKTOP' AS CT FROM DUAL) src
        ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
        WHEN MATCHED THEN
            UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.U_ID, src.CT, src.FC, 1, 1, 1, 1, 1);

        -- Kênh WEB
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID, 'WEB' AS CT FROM DUAL) src
        ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
        WHEN MATCHED THEN
            UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.U_ID, src.CT, src.FC, 1, 1, 1, 1, 1);

        -- Kênh MOBILE
        IF r.FUNCTION_CODE LIKE 'MOBILE_%' THEN
            MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
            USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID, 'MOBILE' AS CT FROM DUAL) src
            ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
            WHEN MATCHED THEN
                UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1
            WHEN NOT MATCHED THEN
                INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
                VALUES (src.U_ID, src.CT, src.FC, 1, 1, 1, 1, 1);
        END IF;
    END LOOP;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Granted default permissions to ADMIN.');
END;
/

-- ------------------------------------------------------------------------------
-- 6. GHI NHẬN MIGRATION VERSION 1.40
-- ------------------------------------------------------------------------------
INSERT INTO SCHEMA_VERSION (VERSION, DESCRIPTION, TYPE, SCRIPT, INSTALLED_BY, INSTALLED_ON, SUCCESS)
SELECT '1.40', 'Employee requests, approvals, profile edit quota, and notifications', 'SQL', 'V1_40__employee_requests_approvals_and_notifications.sql', USER, SYSTIMESTAMP, 1
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM SCHEMA_VERSION WHERE VERSION = '1.40' AND SUCCESS = 1);

COMMIT;
PROMPT [MIGRATION V1_40] Migration completed successfully!
EXIT;
