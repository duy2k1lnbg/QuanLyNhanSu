-- ============================================================================
-- SCRIPT: V1_33__seed_admin_scope_dynamic.sql
-- Muc dich: Cap Scope Grant cho ADMIN voi SCOPE_TYPE = 'ALL' tren 12 capability
--           toan cong ty duoc ho tro, tim kiem dong theo USERNAME = 'ADMIN'.
-- Nguoi chay: DBA / Quan tri he thong co quyen tren AI_OWNER
-- Luu y: KHONG hardcode ID=80/89. KHONG tu chay tren Production.
--        PAYROLL_SELF va INSURANCE_SELF khong cap boi vi ADMIN khong co MANV.
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT [1/3] Preflight: Tim kiem dong tai khoan ADMIN...
DECLARE
    v_admin_count NUMBER;
    v_admin_id    NUMBER;
    v_admin_name  VARCHAR2(100);
BEGIN
    SELECT COUNT(*) INTO v_admin_count
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    IF v_admin_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20010, 'Preflight that bai: Tim thay ' || v_admin_count || ' tai khoan ADMIN (yeu cau dung 1).');
    END IF;

    SELECT IDUSER, USERNAME INTO v_admin_id, v_admin_name
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    DBMS_OUTPUT.PUT_LINE('-> Preflight thanh cong: Tai khoan ADMIN ID=' || v_admin_id || ' (' || v_admin_name || ')');
END;
/

PROMPT [2/3] Seed Scope Grants dong cho ADMIN tren 12 Capability toan cong ty...
DECLARE
    v_admin_id NUMBER;
    v_inserted NUMBER := 0;
    v_exists   NUMBER := 0;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    FOR r IN (
        SELECT COLUMN_VALUE AS CAP FROM TABLE(SYS.ODCIVARCHAR2LIST(
            'EMPLOYEE_LOOKUP',
            'EMPLOYEE_PROFILE',
            'EMPLOYEE_COUNT',
            'ATTENDANCE_SUMMARY',
            'OVERTIME_VIEW',
            'OVERTIME_SUM',
            'ALLOWANCE_VIEW',
            'ADVANCE_VIEW',
            'CONTRACT_VIEW',
            'SALARY_CHANGE_VIEW',
            'PAYROLL_VIEW',
            'PAYROLL_SUMMARY'
        ))
    ) LOOP
        SELECT COUNT(*) INTO v_exists
        FROM AI_OWNER.TB_AI_SCOPE_GRANT
        WHERE SUBJECT_ID = v_admin_id AND SUBJECT_TYPE = 'USER' AND CAPABILITY_CODE = r.CAP;

        IF v_exists = 0 THEN
            INSERT INTO AI_OWNER.TB_AI_SCOPE_GRANT (
                SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, EFFECT, IS_ENABLED, CREATED_AT
            ) VALUES (
                'USER', v_admin_id, r.CAP, 'ALL', 'ALLOW', 1, SYSDATE
            );
            v_inserted := v_inserted + 1;
        END IF;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('-> Da khoi tao ' || v_inserted || ' Scope Grants moi cho ADMIN ID=' || v_admin_id || ' (khong ghi de cac grant hien co).');
END;
/

PROMPT [3/3] Bump Revision chinh sach de he thong AI reload cache...
BEGIN
    UPDATE AI_OWNER.TB_AI_REVISION
       SET REVISION_NUMBER = REVISION_NUMBER + 1,
           UPDATED_AT = CURRENT_TIMESTAMP
     WHERE REVISION_KEY = 'POLICY_GLOBAL';
     
    DBMS_OUTPUT.PUT_LINE('-> Da tang REVISION_NUMBER cho POLICY_GLOBAL.');
END;
/

COMMIT;
PROMPT Hoan tat Script V1_33.
EXIT;

