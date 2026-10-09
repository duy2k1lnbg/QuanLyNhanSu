-- ==============================================================================
-- ROLLBACK SCRIPT FOR V1_27: PLATFORM LOGIN PERMISSIONS
-- File: database/migrations/V1_27_rollback__revert_platform_login_permissions.sql
-- Description:
--   1. Xóa Trigger TRG_ENFORCE_LOGIN_RIGHTS.
--   2. Xóa các dòng TB_SYS_RIGHT thuộc 3 mã F_LOGIN_*.
--   3. Xóa 3 mã F_LOGIN_* khỏi TB_SYS_FUNCTION.
--   4. Xóa cấu hình PLATFORM_ACCESS_CUTOVER_APPLIED trong TB_SYS_CONFIG.
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR CONTINUE;

PROMPT [ROLLBACK V1_27] Starting rollback of platform login permissions...

-- 1. Xóa Trigger
BEGIN
    EXECUTE IMMEDIATE 'DROP TRIGGER TRG_ENFORCE_LOGIN_RIGHTS';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- 2. Xóa dữ liệu quyền trong TB_SYS_RIGHT
DELETE FROM TB_SYS_RIGHT 
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE');
COMMIT;

-- 3. Xóa các mã chức năng trong TB_SYS_FUNCTION
DELETE FROM TB_SYS_FUNCTION 
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE');
COMMIT;

-- 4. Xóa cờ cutover
DELETE FROM TB_SYS_CONFIG WHERE KEY = 'PLATFORM_ACCESS_CUTOVER_APPLIED';
COMMIT;

PROMPT [ROLLBACK V1_27] ROLLBACK COMPLETED SUCCESSFULLY.
EXIT;
