-- ============================================================================
-- SCRIPT: V1_33_rollback__seed_admin_scope_dynamic.sql
-- Muc dich: Rollback Scope Grants cua ADMIN trong AI_OWNER.TB_AI_SCOPE_GRANT.
-- Nguoi chay: DBA
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT Rollback 12 Scope Grants cua ADMIN...
DECLARE
    v_admin_id NUMBER;
    v_deleted  NUMBER := 0;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    DELETE FROM AI_OWNER.TB_AI_SCOPE_GRANT
     WHERE SUBJECT_TYPE = 'USER'
       AND SUBJECT_ID = v_admin_id
       AND CAPABILITY_CODE IN (
            'EMPLOYEE_LOOKUP', 'EMPLOYEE_PROFILE', 'EMPLOYEE_COUNT',
            'ATTENDANCE_SUMMARY', 'OVERTIME_VIEW', 'OVERTIME_SUM',
            'ALLOWANCE_VIEW', 'ADVANCE_VIEW', 'CONTRACT_VIEW',
            'SALARY_CHANGE_VIEW', 'PAYROLL_VIEW', 'PAYROLL_SUMMARY'
       );
    v_deleted := SQL%ROWCOUNT;

    UPDATE AI_OWNER.TB_AI_REVISION
       SET REVISION_NUMBER = REVISION_NUMBER + 1,
           UPDATED_AT = CURRENT_TIMESTAMP
     WHERE REVISION_KEY = 'POLICY_GLOBAL';

    DBMS_OUTPUT.PUT_LINE('-> Rollback hoan tat: Da xoa ' || v_deleted || ' dong Scope Grants cua ADMIN.');
END;
/

COMMIT;
PROMPT Hoan tat Rollback V1_33.
