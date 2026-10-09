-- ============================================================================
-- SCRIPT: V1_32__verify_admin_ai_scope_and_status.sql
-- Muc dich: Kiem tra chi doc (SELECT-only) toan bo trang thai AI Scope Grant,
--           Capability va Field Policy lien quan den tai khoan ADMIN tren Oracle.
-- Nguoi chay: DBA hoac User co quyen SELECT tren AI_OWNER va HR
-- ============================================================================

SET SERVEROUTPUT ON SIZE UNLIMITED;
SET LINESIZE 200;
SET PAGESIZE 100;

PROMPT ============================================================================
PROMPT 1. THONG TIN TAI KHOAN ADMIN VA LIEN KET NHAN VIEN
PROMPT ============================================================================
SELECT IDUSER, USERNAME, FULLNAME, MANV, DISABLED, ISGROUP, TOKEN_VERSION
FROM HR.TB_SYS_USER
WHERE UPPER(TRIM(USERNAME)) = 'ADMIN';

PROMPT ============================================================================
PROMPT 2. QUYEN DANG NHAP NEN TANG (TB_SYS_RIGHT) VA QUYEN THEO KENH (TB_SYS_RIGHT_CHANNEL)
PROMPT ============================================================================
PROMPT 2.1. Quyen dang nhap nen tang trong HR.TB_SYS_RIGHT:
SELECT FUNCTION_CODE, USER_RIGHT, CAN_VIEW
FROM HR.TB_SYS_RIGHT
WHERE IDUSER = (SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1)
  AND FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE');

PROMPT 2.2. Quyen chuc nang nghiep vu theo kenh trong HR.TB_SYS_RIGHT_CHANNEL:
SELECT CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT
FROM HR.TB_SYS_RIGHT_CHANNEL
WHERE IDUSER = (SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1)
  AND FUNCTION_CODE IN ('F_SYSTEM_AI', 'F_SYSTEM_AI_CONFIG', 'F_CC_BANGLUONG', 'F_DM_NHANVIEN')
ORDER BY CLIENT_TYPE, FUNCTION_CODE;

PROMPT ============================================================================
PROMPT 3. CAC GRANT AI SCOPE CUA ADMIN TRONG AI_OWNER.TB_AI_SCOPE_GRANT
PROMPT ============================================================================
SELECT g.GRANT_ID, g.SUBJECT_TYPE, g.SUBJECT_ID, g.CAPABILITY_CODE, 
       g.SCOPE_TYPE, g.EFFECT, g.IS_ENABLED,
       TO_CHAR(g.VALID_FROM, 'YYYY-MM-DD HH24:MI') AS VALID_FROM,
       TO_CHAR(g.VALID_TO, 'YYYY-MM-DD HH24:MI') AS VALID_TO
FROM AI_OWNER.TB_AI_SCOPE_GRANT g
WHERE (g.SUBJECT_TYPE = 'USER' AND g.SUBJECT_ID = (SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1))
   OR (g.SUBJECT_TYPE = 'GROUP' AND g.SUBJECT_ID IN (
       SELECT ID_GROUP FROM HR.TB_SYS_GROUP WHERE MEMBER = (SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1)
   ))
ORDER BY g.CAPABILITY_CODE;

PROMPT ============================================================================
PROMPT 4. TRANG THAI HOAT DONG CUA CAC CAPABILITY AI TRONG CATALOGUE (TB_AI_CAPABILITY)
PROMPT ============================================================================
SELECT CAPABILITY_CODE, DESCRIPTION, IS_ENABLED, REQUIRED_FUNCTION_CODE
FROM AI_OWNER.TB_AI_CAPABILITY
ORDER BY CAPABILITY_CODE;

PROMPT ============================================================================
PROMPT 5. CHINH SACH TRUONG DU LIEU (FIELD POLICY) CHO TOAN HE THONG
PROMPT ============================================================================
SELECT POLICY_ID, CAPABILITY_CODE, LOGICAL_FIELD, ACCESS_MODE, ALLOWED_OPERATIONS
FROM AI_OWNER.TB_AI_FIELD_POLICY
ORDER BY CAPABILITY_CODE, LOGICAL_FIELD;

PROMPT ============================================================================
PROMPT GHI CHU NGHIEP VU:
PROMPT - ADMIN chua co MANV (MANV is null), do do cac capability tu phuc vu ca nhan
PROMPT   (nhu PAYROLL_SELF, INSURANCE_SELF) se khong tim thay ho so nhan vien.
PROMPT - De tra cuu du lieu toan cong ty, ADMIN can duoc cap SCOPE_TYPE = 'ALL' tren
PROMPT   cac capability nhu EMPLOYEE_LOOKUP, PAYROLL_SUMMARY, ATTENDANCE_SUMMARY, v.v.
PROMPT ============================================================================
EXIT;
