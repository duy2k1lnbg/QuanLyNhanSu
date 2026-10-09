-- ==============================================================================
-- VERIFICATION SCRIPT FOR V1_27: PLATFORM LOGIN PERMISSIONS
-- File: database/migrations/V1_27_verify__platform_login_permissions.sql
-- Description:
--   Kiểm tra tính đúng đắn và an toàn sau khi áp dụng migration V1_27.
-- ==============================================================================

SET LINESIZE 250;
SET PAGESIZE 100;
SET FEEDBACK ON;

PROMPT ====================================================================
PROMPT 1. VERIFY FUNCTION CODES CREATED IN TB_SYS_FUNCTION
PROMPT ====================================================================
SELECT FUNCTION_CODE, DESCRIPTION, PARENT, SORT
FROM TB_SYS_FUNCTION
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
ORDER BY SORT;

PROMPT ====================================================================
PROMPT 2. VERIFY NO WRITE/PRINT ACTIONS ENABLED ON F_LOGIN_*
PROMPT ====================================================================
SELECT FUNCTION_CODE,
       SUM(CAN_ADD) AS SUM_ADD,
       SUM(CAN_EDIT) AS SUM_EDIT,
       SUM(CAN_DELETE) AS SUM_DELETE,
       SUM(CAN_PRINT) AS SUM_PRINT,
       SUM(CASE WHEN CAN_VIEW <> USER_RIGHT THEN 1 ELSE 0 END) AS MISMATCH_COUNT
FROM TB_SYS_RIGHT
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
GROUP BY FUNCTION_CODE;

PROMPT ====================================================================
PROMPT 3. VERIFY ROOT ADMIN PLATFORM RIGHTS
PROMPT ====================================================================
SELECT u.USERNAME, r.FUNCTION_CODE, r.CAN_VIEW, r.USER_RIGHT
FROM TB_SYS_RIGHT r
JOIN TB_SYS_USER u ON r.IDUSER = u.IDUSER
WHERE UPPER(TRIM(u.USERNAME)) = 'ADMIN'
  AND r.FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
ORDER BY r.FUNCTION_CODE;

PROMPT ====================================================================
PROMPT 4. VERIFY CUTOVER CONFIG FLAG
PROMPT ====================================================================
SELECT NAME, VALUE FROM TB_CONFIG WHERE NAME = 'PLATFORM_ACCESS_CUTOVER_APPLIED';
SELECT CONFIG_KEY, CONFIG_VALUE FROM TB_SYS_CONFIG WHERE CONFIG_KEY = 'PLATFORM_ACCESS_CUTOVER_APPLIED';

PROMPT ====================================================================
PROMPT 5. VERIFY OVERALL GRANT COUNTS BY PLATFORM
PROMPT ====================================================================
SELECT FUNCTION_CODE,
       SUM(CAN_VIEW) AS GRANTED_USERS,
       COUNT(*) - SUM(CAN_VIEW) AS DENIED_USERS
FROM TB_SYS_RIGHT
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
GROUP BY FUNCTION_CODE
ORDER BY FUNCTION_CODE;

PROMPT ====================================================================
PROMPT 6. VERIFY ENFORCEMENT TRIGGER STATUS
PROMPT ====================================================================
SELECT TRIGGER_NAME, STATUS FROM USER_TRIGGERS WHERE TRIGGER_NAME = 'TRG_ENFORCE_LOGIN_RIGHTS';

PROMPT ====================================================================
PROMPT VERIFICATION COMPLETED.
PROMPT ====================================================================
EXIT;
