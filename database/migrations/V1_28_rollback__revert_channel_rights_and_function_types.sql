-- ==============================================================================
-- Rollback V1_28: Revert Channel Rights & Function Types (Hardened & Audited)
-- File: database/migrations/V1_28_rollback__revert_channel_rights_and_function_types.sql
-- Description:
--   Rollback an toàn cho Migration V1_28.
--   Tự động backup dữ liệu phân quyền vào TB_SYS_RIGHT_CHANNEL_ROLLBACK_BAK
--   trước khi thu hồi bảng, tránh mất mát dữ liệu quản trị đã cấu hình.
-- ==============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR CONTINUE;

PROMPT [ROLLBACK V1_28] Starting audited rollback of Channel Rights and Function Types...

-- 1. Backup dữ liệu hiện tại trước khi thu hồi (Phòng ngừa mất dữ liệu phân quyền)
DECLARE
    v_cnt NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM ALL_TABLES WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_RIGHT_CHANNEL';
    IF v_cnt > 0 THEN
        EXECUTE IMMEDIATE 'CREATE TABLE HR.TB_SYS_RIGHT_CHANNEL_ROLLBACK_BAK AS SELECT * FROM HR.TB_SYS_RIGHT_CHANNEL';
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Backup table HR.TB_SYS_RIGHT_CHANNEL_ROLLBACK_BAK created successfully.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Backup notice: ' || SQLERRM);
END;
/

-- 2. Drop trigger TRG_ENFORCE_RIGHT_CHANNEL
DECLARE
    v_trg_cnt NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_trg_cnt FROM ALL_TRIGGERS WHERE OWNER = 'HR' AND TRIGGER_NAME = 'TRG_ENFORCE_RIGHT_CHANNEL';
    IF v_trg_cnt > 0 THEN
        EXECUTE IMMEDIATE 'DROP TRIGGER HR.TRG_ENFORCE_RIGHT_CHANNEL';
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Dropped trigger TRG_ENFORCE_RIGHT_CHANNEL.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Trigger TRG_ENFORCE_RIGHT_CHANNEL did not exist.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Trigger drop error: ' || SQLERRM);
END;
/

-- 3. Drop table TB_SYS_RIGHT_CHANNEL
DECLARE
    v_tbl_cnt NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_tbl_cnt FROM ALL_TABLES WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_RIGHT_CHANNEL';
    IF v_tbl_cnt > 0 THEN
        EXECUTE IMMEDIATE 'DROP TABLE HR.TB_SYS_RIGHT_CHANNEL CASCADE CONSTRAINTS';
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Dropped table TB_SYS_RIGHT_CHANNEL.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Table TB_SYS_RIGHT_CHANNEL did not exist.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Table drop error: ' || SQLERRM);
END;
/

-- 4. Drop column RIGHT_TYPE from TB_SYS_FUNCTION
DECLARE
    v_col_cnt NUMBER := 0;
BEGIN
    SELECT COUNT(*) INTO v_col_cnt FROM ALL_TAB_COLS WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_FUNCTION' AND COLUMN_NAME = 'RIGHT_TYPE';
    IF v_col_cnt > 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_SYS_FUNCTION DROP COLUMN RIGHT_TYPE';
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Dropped column RIGHT_TYPE from TB_SYS_FUNCTION.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Column RIGHT_TYPE did not exist.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Column drop error: ' || SQLERRM);
END;
/

-- 5. Delete cutover flag from TB_CONFIG
BEGIN
    DELETE FROM HR.TB_CONFIG WHERE NAME = 'CHANNEL_RIGHTS_V1_28_APPLIED';
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Cleared cutover flag CHANNEL_RIGHTS_V1_28_APPLIED from TB_CONFIG.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('[ROLLBACK V1_28] Config flag clear error: ' || SQLERRM);
END;
/

PROMPT [ROLLBACK V1_28] Rollback script completed.
