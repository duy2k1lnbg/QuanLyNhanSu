-- ==============================================================================
-- PREFLIGHT INSPECTION SCRIPT: PLATFORM ACCESS PERMISSIONS & SESSION STATE
-- File: database/migrations/preflight_platform_access.sql
-- Mode: READ-ONLY (No DDL, No DML, Safe to run anytime)
-- ==============================================================================

SET LINESIZE 250;
SET PAGESIZE 100;
SET FEEDBACK ON;

PROMPT ====================================================================
PROMPT 1. CHECK EXISTING TABLES AND OBJECTS
PROMPT ====================================================================
SELECT table_name, status, num_rows, last_analyzed
FROM user_tables
WHERE table_name IN (
    'TB_SYS_USER', 'TB_SYS_FUNCTION', 'TB_SYS_RIGHT', 'TB_SYS_GROUP',
    'TB_USER_EMPLOYEE_MAPPING', 'TB_AUTH_SESSION', 'TB_AUTH_POLICY',
    'TB_AUTH_LOGIN_ATTEMPT', 'TB_AUTH_AUDIT', 'TB_NHANVIEN'
)
ORDER BY table_name;

PROMPT ====================================================================
PROMPT 2. CHECK RELEVANT COLUMNS IN TB_SYS_USER
PROMPT ====================================================================
SELECT column_name, data_type, data_length, nullable, data_default
FROM user_tab_cols
WHERE table_name = 'TB_SYS_USER'
  AND column_name IN (
    'IDUSER', 'USERNAME', 'PASSWORD', 'DISABLED', 'ISGROUP', 'MACTY', 'MADVI',
    'MANV', 'CLIENT_TYPE', 'TOKEN_VERSION', 'FAILED_LOGIN_COUNT', 'LOCKOUT_END',
    'FIRST_FAILED_LOGIN_AT', 'LAST_FAILED_LOGIN_AT', 'LAST_SUCCESS_LOGIN_AT', 'LOCK_REASON'
  )
ORDER BY column_id;

PROMPT ====================================================================
PROMPT 3. CHECK UNIQUE CONSTRAINTS ON MAPPING AND SESSION TABLES
PROMPT ====================================================================
SELECT constraint_name, table_name, constraint_type, status
FROM user_constraints
WHERE table_name IN ('TB_USER_EMPLOYEE_MAPPING', 'TB_AUTH_SESSION', 'TB_SYS_RIGHT')
  AND constraint_type IN ('P', 'U', 'R')
ORDER BY table_name, constraint_type;

PROMPT ====================================================================
PROMPT 4. INSPECT USER DISTRIBUTION BY CLIENT_TYPE
PROMPT ====================================================================
SELECT NVL(CLIENT_TYPE, '<NULL>') AS LEGACY_CLIENT_TYPE,
       COUNT(*) AS USER_COUNT,
       SUM(CASE WHEN ISGROUP = 1 THEN 1 ELSE 0 END) AS GROUP_COUNT,
       SUM(CASE WHEN DISABLED = 1 THEN 1 ELSE 0 END) AS DISABLED_COUNT,
       SUM(CASE WHEN MANV IS NOT NULL AND MANV > 0 THEN 1 ELSE 0 END) AS HAS_MANV_COUNT
FROM TB_SYS_USER
GROUP BY CLIENT_TYPE
ORDER BY USER_COUNT DESC;

PROMPT ====================================================================
PROMPT 5. INSPECT ROOT ADMIN RECORD (ISGROUP, MEMBERSHIP, STATUS)
PROMPT ====================================================================
SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, CLIENT_TYPE, MANV, TOKEN_VERSION
FROM TB_SYS_USER
WHERE UPPER(TRIM(USERNAME)) = 'ADMIN';

PROMPT ====================================================================
PROMPT 6. CHECK GROUP INTEGRITY (TB_SYS_GROUP)
PROMPT ====================================================================
SELECT COUNT(*) AS TOTAL_MEMBERSHIPS,
       COUNT(DISTINCT ID_GROUP) AS DISTINCT_GROUPS,
       COUNT(DISTINCT MEMBER) AS DISTINCT_MEMBERS
FROM TB_SYS_GROUP;

-- Kiểm tra xem có membership nào trỏ tới nhóm không có ISGROUP = 1 hoặc bị disabled
SELECT g.ID_GROUP, u_grp.USERNAME AS GROUP_NAME, u_grp.ISGROUP, u_grp.DISABLED, COUNT(*) AS MEMBER_COUNT
FROM TB_SYS_GROUP g
LEFT JOIN TB_SYS_USER u_grp ON g.ID_GROUP = u_grp.IDUSER
GROUP BY g.ID_GROUP, u_grp.USERNAME, u_grp.ISGROUP, u_grp.DISABLED
ORDER BY g.ID_GROUP;

PROMPT ====================================================================
PROMPT 7. CHECK FUNCTION CODES (CHECK IF F_LOGIN_* ALREADY EXIST)
PROMPT ====================================================================
SELECT FUNCTION_CODE, DESCRIPTION, PARENT, SORT
FROM TB_SYS_FUNCTION
WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE', 'F_SYSTEM_AI', 'F_DB_NHANSU')
ORDER BY SORT, FUNCTION_CODE;

PROMPT ====================================================================
PROMPT 8. INSPECT ACTIVE SESSIONS IN TB_AUTH_SESSION
PROMPT ====================================================================
SELECT CLIENT_TYPE,
       COUNT(*) AS TOTAL_SESSIONS,
       SUM(CASE WHEN REVOKED_AT IS NULL AND EXPIRES_AT > CURRENT_TIMESTAMP THEN 1 ELSE 0 END) AS ACTIVE_SESSIONS,
       SUM(CASE WHEN REVOKED_AT IS NOT NULL THEN 1 ELSE 0 END) AS REVOKED_SESSIONS,
       SUM(CASE WHEN EXPIRES_AT <= CURRENT_TIMESTAMP THEN 1 ELSE 0 END) AS EXPIRED_SESSIONS
FROM TB_AUTH_SESSION
GROUP BY CLIENT_TYPE
ORDER BY CLIENT_TYPE;

PROMPT ====================================================================
PROMPT 9. INSPECT USER EMPLOYEE MAPPING STATUS
PROMPT ====================================================================
SELECT COUNT(*) AS TOTAL_MAPPINGS,
       SUM(CASE WHEN IS_MOBILE_ENABLED = 1 THEN 1 ELSE 0 END) AS MOBILE_ENABLED_COUNT,
       SUM(CASE WHEN IS_MOBILE_ENABLED = 0 THEN 1 ELSE 0 END) AS MOBILE_DISABLED_COUNT
FROM TB_USER_EMPLOYEE_MAPPING;

PROMPT ====================================================================
PROMPT PREFLIGHT COMPLETE. NO CHANGES COMMITTED.
PROMPT ====================================================================
EXIT;
