-- ==============================================================================
-- Migration V1_28: Channel Rights & Function Types (Sections 13-14 Specification)
-- File: database/migrations/V1_28__channel_rights_and_function_types.sql
-- Description:
--   1. Thêm cột RIGHT_TYPE vào TB_SYS_FUNCTION để phân biệt loại quyền:
--      - 'LOGIN': Cho 3 mã quyền đăng nhập nền tảng (F_LOGIN_DESKTOP, F_LOGIN_WEB, F_LOGIN_MOBILE)
--      - 'FUNCTION': Quyền chức năng nghiệp vụ có thể cấp phát action
--      - 'CATEGORY': Nhóm/tiêu đề danh mục chức năng (chỉ dùng tổ chức hiển thị)
--   2. Tạo bảng mới TB_SYS_RIGHT_CHANNEL có khóa 3 cột (IDUSER, CLIENT_TYPE, FUNCTION_CODE)
--      cho phép phân quyền độc lập theo từng nền tảng (Desktop, Web, Mobile).
--   3. Backfill dữ liệu quyền con an toàn từ TB_SYS_RIGHT theo kênh và khả năng kênh.
--   4. Thiết lập Trigger bảo vệ tầng DB bảo đảm quy tắc bảo mật theo kênh (ví dụ Web cấm tính lương).
--   5. Ghi nhận dấu mốc hoàn tất vào TB_CONFIG (CHANNEL_RIGHTS_V1_28_APPLIED = 'TRUE').
-- ==============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

PROMPT [MIGRATION V1_28] Starting Channel Rights and Function Types migration...

-- ------------------------------------------------------------------------------
-- 1. Bổ sung cột RIGHT_TYPE vào TB_SYS_FUNCTION
-- ------------------------------------------------------------------------------
DECLARE
    v_col_exists NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_col_exists
    FROM ALL_TAB_COLUMNS
    WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_FUNCTION' AND COLUMN_NAME = 'RIGHT_TYPE';

    IF v_col_exists = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_SYS_FUNCTION ADD RIGHT_TYPE VARCHAR2(20) DEFAULT ''FUNCTION'' NOT NULL';
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_SYS_FUNCTION ADD CONSTRAINT CK_SYS_FUNC_RIGHT_TYPE CHECK (RIGHT_TYPE IN (''LOGIN'', ''FUNCTION'', ''CATEGORY''))';
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Added column RIGHT_TYPE to TB_SYS_FUNCTION.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Column RIGHT_TYPE already exists.');
    END IF;
END;
/

-- Cập nhật RIGHT_TYPE cho các mã hiện có
UPDATE HR.TB_SYS_FUNCTION
SET RIGHT_TYPE = 'LOGIN'
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE');

UPDATE HR.TB_SYS_FUNCTION
SET RIGHT_TYPE = 'CATEGORY'
WHERE NVL(ISGROUP, 0) = 1;

COMMIT;
PROMPT [MIGRATION V1_28] Updated RIGHT_TYPE metadata.

-- ------------------------------------------------------------------------------
-- 2. Tạo bảng TB_SYS_RIGHT_CHANNEL (Khóa ba cột IDUSER, CLIENT_TYPE, FUNCTION_CODE)
-- ------------------------------------------------------------------------------
DECLARE
    v_tbl_exists NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_tbl_exists
    FROM ALL_TABLES
    WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_RIGHT_CHANNEL';

    IF v_tbl_exists = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE HR.TB_SYS_RIGHT_CHANNEL (
            IDUSER NUMBER NOT NULL,
            CLIENT_TYPE VARCHAR2(20) NOT NULL,
            FUNCTION_CODE NVARCHAR2(100) NOT NULL,
            CAN_VIEW NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_ADD NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_EDIT NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_DELETE NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_PRINT NUMBER(1) DEFAULT 0 NOT NULL,
            CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT PK_TB_SYS_RIGHT_CHANNEL PRIMARY KEY (IDUSER, CLIENT_TYPE, FUNCTION_CODE),
            CONSTRAINT FK_SRC_USER FOREIGN KEY (IDUSER) REFERENCES HR.TB_SYS_USER(IDUSER) ON DELETE CASCADE,
            CONSTRAINT CK_SRC_CLIENT_TYPE CHECK (CLIENT_TYPE IN (''DESKTOP'', ''WEB'', ''MOBILE'')),
            CONSTRAINT CK_SRC_CAN_VIEW CHECK (CAN_VIEW IN (0, 1)),
            CONSTRAINT CK_SRC_CAN_ADD CHECK (CAN_ADD IN (0, 1)),
            CONSTRAINT CK_SRC_CAN_EDIT CHECK (CAN_EDIT IN (0, 1)),
            CONSTRAINT CK_SRC_CAN_DELETE CHECK (CAN_DELETE IN (0, 1)),
            CONSTRAINT CK_SRC_CAN_PRINT CHECK (CAN_PRINT IN (0, 1))
        )';
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Created table TB_SYS_RIGHT_CHANNEL.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Table TB_SYS_RIGHT_CHANNEL already exists.');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 3. Backfill dữ liệu quyền con an toàn theo kênh (Idempotent)
-- ------------------------------------------------------------------------------
DECLARE
    v_is_applied NUMBER := 0;
BEGIN
    BEGIN
        SELECT COUNT(*) INTO v_is_applied
        FROM HR.TB_CONFIG
        WHERE NAME = 'CHANNEL_RIGHTS_V1_28_APPLIED' AND UPPER(TRIM(VALUE)) IN ('1', 'TRUE');
    EXCEPTION
        WHEN OTHERS THEN v_is_applied := 0;
    END;

    IF v_is_applied = 0 THEN
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] First-time channel rights backfill starting...');

        -- 3.1. Backfill DESKTOP: Cho các user có quyền F_LOGIN_DESKTOP = 1
        -- Sao chép nguyên trạng quyền các chức năng nghiệp vụ (không bao gồm F_LOGIN_*)
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (
            SELECT r.IDUSER, 'DESKTOP' AS CLIENT_TYPE, r.FUNCTION_CODE,
                   NVL(r.CAN_VIEW, 0) AS CAN_VIEW,
                   NVL(r.CAN_ADD, 0) AS CAN_ADD,
                   NVL(r.CAN_EDIT, 0) AS CAN_EDIT,
                   NVL(r.CAN_DELETE, 0) AS CAN_DELETE,
                   NVL(r.CAN_PRINT, 0) AS CAN_PRINT
            FROM HR.TB_SYS_RIGHT r
            JOIN HR.TB_SYS_RIGHT parent_r ON parent_r.IDUSER = r.IDUSER AND parent_r.FUNCTION_CODE = 'F_LOGIN_DESKTOP'
            WHERE parent_r.CAN_VIEW = 1
              AND r.FUNCTION_CODE NOT IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
        ) src
        ON (tgt.IDUSER = src.IDUSER AND tgt.CLIENT_TYPE = src.CLIENT_TYPE AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
        WHEN MATCHED THEN
            UPDATE SET tgt.CAN_VIEW = src.CAN_VIEW, tgt.CAN_ADD = src.CAN_ADD, tgt.CAN_EDIT = src.CAN_EDIT,
                       tgt.CAN_DELETE = src.CAN_DELETE, tgt.CAN_PRINT = src.CAN_PRINT, tgt.UPDATED_AT = SYSTIMESTAMP
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.IDUSER, src.CLIENT_TYPE, src.FUNCTION_CODE, src.CAN_VIEW, src.CAN_ADD, src.CAN_EDIT, src.CAN_DELETE, src.CAN_PRINT);

        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Backfilled DESKTOP channel rights.');

        -- 3.2. Backfill WEB: Cho các user có quyền F_LOGIN_WEB = 1
        -- Giới hạn: Bảng lương (F_CC_BANGLUONG) trên Web chỉ cấp CAN_VIEW, toàn bộ action tính/ghi/xóa/in = 0
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (
            SELECT r.IDUSER, 'WEB' AS CLIENT_TYPE, r.FUNCTION_CODE,
                   NVL(r.CAN_VIEW, 0) AS CAN_VIEW,
                   CASE WHEN r.FUNCTION_CODE = 'F_CC_BANGLUONG' THEN 0 ELSE NVL(r.CAN_ADD, 0) END AS CAN_ADD,
                   CASE WHEN r.FUNCTION_CODE = 'F_CC_BANGLUONG' THEN 0 ELSE NVL(r.CAN_EDIT, 0) END AS CAN_EDIT,
                   CASE WHEN r.FUNCTION_CODE = 'F_CC_BANGLUONG' THEN 0 ELSE NVL(r.CAN_DELETE, 0) END AS CAN_DELETE,
                   CASE WHEN r.FUNCTION_CODE = 'F_CC_BANGLUONG' THEN 0 ELSE NVL(r.CAN_PRINT, 0) END AS CAN_PRINT
            FROM HR.TB_SYS_RIGHT r
            JOIN HR.TB_SYS_RIGHT parent_r ON parent_r.IDUSER = r.IDUSER AND parent_r.FUNCTION_CODE = 'F_LOGIN_WEB'
            WHERE parent_r.CAN_VIEW = 1
              AND r.FUNCTION_CODE NOT IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
        ) src
        ON (tgt.IDUSER = src.IDUSER AND tgt.CLIENT_TYPE = src.CLIENT_TYPE AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
        WHEN MATCHED THEN
            UPDATE SET tgt.CAN_VIEW = src.CAN_VIEW, tgt.CAN_ADD = src.CAN_ADD, tgt.CAN_EDIT = src.CAN_EDIT,
                       tgt.CAN_DELETE = src.CAN_DELETE, tgt.CAN_PRINT = src.CAN_PRINT, tgt.UPDATED_AT = SYSTIMESTAMP
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.IDUSER, src.CLIENT_TYPE, src.FUNCTION_CODE, src.CAN_VIEW, src.CAN_ADD, src.CAN_EDIT, src.CAN_DELETE, src.CAN_PRINT);

        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Backfilled WEB channel rights.');

        -- 3.3. Backfill MOBILE: Cho các user có quyền F_LOGIN_MOBILE = 1
        -- Chỉ sao chép các mã capability Mobile chuyên biệt (bắt đầu bằng MOBILE_)
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (
            SELECT r.IDUSER, 'MOBILE' AS CLIENT_TYPE, r.FUNCTION_CODE,
                   NVL(r.CAN_VIEW, 0) AS CAN_VIEW,
                   NVL(r.CAN_ADD, 0) AS CAN_ADD,
                   NVL(r.CAN_EDIT, 0) AS CAN_EDIT,
                   NVL(r.CAN_DELETE, 0) AS CAN_DELETE,
                   NVL(r.CAN_PRINT, 0) AS CAN_PRINT
            FROM HR.TB_SYS_RIGHT r
            JOIN HR.TB_SYS_RIGHT parent_r ON parent_r.IDUSER = r.IDUSER AND parent_r.FUNCTION_CODE = 'F_LOGIN_MOBILE'
            WHERE parent_r.CAN_VIEW = 1
              AND r.FUNCTION_CODE LIKE 'MOBILE_%'
        ) src
        ON (tgt.IDUSER = src.IDUSER AND tgt.CLIENT_TYPE = src.CLIENT_TYPE AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
        WHEN MATCHED THEN
            UPDATE SET tgt.CAN_VIEW = src.CAN_VIEW, tgt.CAN_ADD = src.CAN_ADD, tgt.CAN_EDIT = src.CAN_EDIT,
                       tgt.CAN_DELETE = src.CAN_DELETE, tgt.CAN_PRINT = src.CAN_PRINT, tgt.UPDATED_AT = SYSTIMESTAMP
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.IDUSER, src.CLIENT_TYPE, src.FUNCTION_CODE, src.CAN_VIEW, src.CAN_ADD, src.CAN_EDIT, src.CAN_DELETE, src.CAN_PRINT);

        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Backfilled MOBILE channel rights.');

        -- Đánh dấu cờ hoàn tất
        MERGE INTO HR.TB_CONFIG tgt
        USING (SELECT 'CHANNEL_RIGHTS_V1_28_APPLIED' AS NAME, 'TRUE' AS VALUE FROM DUAL) src
        ON (tgt.NAME = src.NAME)
        WHEN MATCHED THEN UPDATE SET tgt.VALUE = src.VALUE
        WHEN NOT MATCHED THEN INSERT (ID_CF, NAME, VALUE) 
            VALUES ((SELECT NVL(MAX(ID_CF), 0) + 1 FROM HR.TB_CONFIG), src.NAME, src.VALUE);

        COMMIT;
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Channel rights migration completed.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_28] Channel rights migration was already applied.');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 4. Trigger bảo vệ toàn vẹn dữ liệu quyền theo kênh
-- ------------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER HR.TRG_ENFORCE_RIGHT_CHANNEL
BEFORE INSERT OR UPDATE ON HR.TB_SYS_RIGHT_CHANNEL
FOR EACH ROW
BEGIN
    -- Quy tắc 1: Web Portal cấm tuyệt đối action thêm/sửa/xóa/in đối với Bảng lương
    IF :NEW.CLIENT_TYPE = 'WEB' AND :NEW.FUNCTION_CODE = 'F_CC_BANGLUONG' THEN
        :NEW.CAN_ADD := 0;
        :NEW.CAN_EDIT := 0;
        :NEW.CAN_DELETE := 0;
        :NEW.CAN_PRINT := 0;
    END IF;

    -- Quy tắc 2: Mobile App không chấp nhận các chức năng quản trị hệ thống Desktop
    IF :NEW.CLIENT_TYPE = 'MOBILE' AND :NEW.FUNCTION_CODE IN ('F_SYSTEM_DB_CONFIG', 'F_SYSTEM_SAULUU', 'F_SYSTEM_PHUCHOI', 'F_SYSTEM_SETTING') THEN
        :NEW.CAN_VIEW := 0;
        :NEW.CAN_ADD := 0;
        :NEW.CAN_EDIT := 0;
        :NEW.CAN_DELETE := 0;
        :NEW.CAN_PRINT := 0;
    END IF;

    :NEW.UPDATED_AT := SYSTIMESTAMP;
END;
/

PROMPT [MIGRATION V1_28] Trigger TRG_ENFORCE_RIGHT_CHANNEL created/updated.
PROMPT [MIGRATION V1_28] Migration script prepared successfully!
