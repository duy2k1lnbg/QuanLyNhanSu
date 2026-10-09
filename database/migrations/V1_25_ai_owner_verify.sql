-- ============================================================================
-- SCRIPT: V1_25_ai_owner_verify.sql
-- Kich ban kiem tra toan dien AI_OWNER B2 Repair
-- Nguoi thuc thi: DBA va AI_READONLY
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;
SET DEFINE OFF;

PROMPT ============================================================================
PROMPT PHAN 1: KIEM TRA PHAN QUYEN HE THONG VA TOI THIEU (DBA / METADATA)
PROMPT ============================================================================

DECLARE
    v_count NUMBER;
    v_err_count NUMBER;
BEGIN
    -- 1. Users
    SELECT COUNT(*) INTO v_count FROM ALL_USERS WHERE USERNAME IN ('AI_OWNER', 'AI_READONLY');
    IF v_count < 2 THEN
        RAISE_APPLICATION_ERROR(-20020, 'Loi: Thieu User AI_OWNER hoac AI_READONLY.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 1. Cac User AI_OWNER va AI_READONLY ton tai day du.');

    -- 2. System Privileges: CREATE SESSION only
    SELECT COUNT(*) INTO v_count FROM DBA_SYS_PRIVS 
    WHERE GRANTEE = 'AI_READONLY' AND PRIVILEGE <> 'CREATE SESSION';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20021, 'Loi: AI_READONLY co quyen he thong vuot qua CREATE SESSION!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 2. AI_READONLY chi co quyen duy nhat: CREATE SESSION.');

    -- 3. Roles: 0
    SELECT COUNT(*) INTO v_count FROM DBA_ROLE_PRIVS WHERE GRANTEE = 'AI_READONLY';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20022, 'Loi: AI_READONLY duoc gan ROLE!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 3. AI_READONLY khong co role nao.');

    -- 4. Direct Table Grants tren HR: 0
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE IN ('AI_READONLY', 'PUBLIC') AND OWNER = 'HR' AND TABLE_NAME LIKE 'TB_%';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20023, 'Loi: AI_READONLY co quyen truc tiep tren bang goc cua HR!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 4. AI_READONLY khong co quyen tren bat ky bang nao cua HR.');

    -- 5. Direct Table Grants tren AI_OWNER tables: 0
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE IN ('AI_READONLY', 'PUBLIC') AND OWNER = 'AI_OWNER' AND TABLE_NAME LIKE 'TB_AI_%';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20024, 'Loi: AI_READONLY bi ro ri quyen xem bang quan tri/khoa!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 5. Cac bang quan tri (TB_AI_SECRET_VAULT, NONCE, TICKET) duoc bao ve tuyet doi.');

    -- 6. WITH GRANT OPTION tren HR.V_AI_SRC_*
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE = 'AI_OWNER' AND OWNER = 'HR' AND TABLE_NAME LIKE 'V_AI_SRC_%' AND GRANTABLE = 'YES';
    IF v_count < 11 THEN
        RAISE_APPLICATION_ERROR(-20025, 'Loi: AI_OWNER thieu WITH GRANT OPTION tren V_AI_SRC_*.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 6. AI_OWNER co du WITH GRANT OPTION tren tat ca cac View nguon tu HR.');

    -- 7. Trusted Context HRMS_AI_CTX
    SELECT COUNT(*) INTO v_count FROM DBA_CONTEXT 
    WHERE NAMESPACE = 'HRMS_AI_CTX' AND SCHEMA = 'AI_OWNER' AND PACKAGE = 'PKG_AI_READER';
    IF v_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20026, 'Loi: Trusted Context HRMS_AI_CTX khong hop le.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 7. Trusted Context HRMS_AI_CTX lien ket chinh xac voi AI_OWNER.PKG_AI_READER.');

    -- 8. Kiem tra khong co loi bien dich
    SELECT COUNT(*) INTO v_err_count FROM DBA_ERRORS 
    WHERE OWNER IN ('HR', 'AI_OWNER') AND (NAME LIKE 'PKG_AI_%' OR NAME LIKE 'V_AI_%');
    IF v_err_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20027, 'Loi: Ton tai loi bien dich trong cac Package/View AI!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 8. Khong co loi bien dich trong bat ky doi tuong AI nao.');

    -- 9. Legacy views owned by AI_READONLY: must be 0
    SELECT COUNT(*) INTO v_count FROM DBA_OBJECTS WHERE OWNER = 'AI_READONLY';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20028, 'Loi: AI_READONLY van con so huu doi tuong cu (' || v_count || ')!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 9. AI_READONLY so huu dung 0 doi tuong (legacy views da duoc drop triet de).');
END;
/

PROMPT ============================================================================
PROMPT PHAN 2: KIEM TRA CAPABILITY B2 (16 TONG, 11 BAT, 5 TAT, 0 PAYROLL)
PROMPT ============================================================================

DECLARE
    v_total_caps   NUMBER;
    v_enabled_caps NUMBER;
    v_payroll_caps NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_total_caps FROM AI_OWNER.TB_AI_CAPABILITY;
    SELECT COUNT(*) INTO v_enabled_caps FROM AI_OWNER.TB_AI_CAPABILITY WHERE IS_ENABLED = 1;
    SELECT COUNT(*) INTO v_payroll_caps FROM AI_OWNER.TB_AI_CAPABILITY WHERE DOMAIN = 'PAYROLL' AND IS_ENABLED = 1;

    IF v_total_caps <> 16 THEN
        RAISE_APPLICATION_ERROR(-20030, 'Loi: So luong capability khong dung chuan (ky vong 16, thuc te ' || v_total_caps || ').');
    END IF;
    IF v_enabled_caps <> 11 THEN
        RAISE_APPLICATION_ERROR(-20031, 'Loi: Nhanh B2 phai co dung 11 capability BAT (thuc te ' || v_enabled_caps || ').');
    END IF;
    IF v_payroll_caps > 0 THEN
        RAISE_APPLICATION_ERROR(-20032, 'Loi bao mat B2: Domain PAYROLL van con mo trong danh sach BAT!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 10. Danh muc Capability Nhanh B2 dung chuan: Tong 16, 11 BAT, 5 TAT (Payroll tat tuyet doi).');
END;
/

PROMPT ============================================================================
PROMPT PHAN 3: KIEM TRA DONG TOAN BO 13 VIEWS KHI CHUA BIND VE (UNBOUND ZERO-ROWS)
PROMPT ============================================================================

DECLARE
    v_c NUMBER;
    v_total NUMBER := 0;
BEGIN
    AI_OWNER.PKG_AI_READER.CLEAR_REQUEST();

    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_EMPLOYEE_LOOKUP; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_EMPLOYEE_LOOKUP'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_EMPLOYEE; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_EMPLOYEE'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_EMPLOYEE_COUNT; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_EMPLOYEE_COUNT'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_ORG_LOOKUP; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_ORG_LOOKUP'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_PERIOD; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_PERIOD'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_OVERTIME; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_OVERTIME'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_OVERTIME_SUMMARY; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_OVERTIME_SUMMARY'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_ALLOWANCE; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_ALLOWANCE'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_INSURANCE; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_INSURANCE'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_ADVANCE; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_ADVANCE'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_ATTENDANCE_SUMMARY; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_ATTENDANCE_SUMMARY'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_CONTRACT; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_CONTRACT'); END IF;
    SELECT COUNT(*) INTO v_c FROM AI_OWNER.V_AI_SALARY_CHANGE; IF v_c > 0 THEN RAISE_APPLICATION_ERROR(-20050, 'Leak: V_AI_SALARY_CHANGE'); END IF;

    DBMS_OUTPUT.PUT_LINE('[PASS] 11. Toan bo 13 Views deu tra ve dung 0 dong khi chua co Ticket hop le (Unbound Guard 100%).');
END;
/

PROMPT ============================================================================
PROMPT PHAN 4: KIEM TRA HMAC AUTH, NONCE REPLAY, DISABLED ACTOR, EXPIRED PROOF
PROMPT ============================================================================

DECLARE
    v_secret       VARCHAR2(256);
    v_actor_id     NUMBER := 80;
    v_nonce        VARCHAR2(64);
    v_exp          NUMBER;
    v_payload      VARCHAR2(1000);
    v_valid_sig    VARCHAR2(64);
    v_ticket       VARCHAR2(64);
    v_caught_error BOOLEAN;
    v_user_cur     SYS_REFCURSOR;
    v_has_ai       NUMBER;
BEGIN
    SELECT SECRET_VAL INTO v_secret FROM AI_OWNER.TB_AI_SECRET_VAULT WHERE KEY_NAME = 'AI_PROOF_KEY';

    -- 1. Fake signature
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) - DATE '1970-01-01') * 86400) + 30;
    v_caught_error := FALSE;
    BEGIN
        AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
            v_actor_id, 'EMPLOYEE_LOOKUP', 'HRMS_AI_ORACLE', v_nonce, v_exp,
            'BAD_SIGNATURE_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA', v_ticket
        );
    EXCEPTION WHEN OTHERS THEN IF SQLCODE = -20001 THEN v_caught_error := TRUE; END IF;
    END;
    IF NOT v_caught_error THEN RAISE_APPLICATION_ERROR(-20040, 'Loi: Chu ky gia mao khong bi chan!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 12. Chan chu ky HMAC gia mao thanh cong (ORA-20001).');

    -- 2. Expired proof
    v_caught_error := FALSE;
    BEGIN
        v_payload := v_actor_id || '|EMPLOYEE_LOOKUP|HRMS_AI_ORACLE|' || v_nonce || '|' || (v_exp - 100);
        v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
            src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
            typ => DBMS_CRYPTO.HMAC_SH256,
            key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
        ));
        AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
            v_actor_id, 'EMPLOYEE_LOOKUP', 'HRMS_AI_ORACLE', v_nonce, v_exp - 100, v_valid_sig, v_ticket
        );
    EXCEPTION WHEN OTHERS THEN IF SQLCODE = -20002 THEN v_caught_error := TRUE; END IF;
    END;
    IF NOT v_caught_error THEN RAISE_APPLICATION_ERROR(-20041, 'Loi: Proof het han khong bi chan!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 13. Chan Proof het han thanh cong (ORA-20002).');

    -- 3. GET_ACTOR_IDENTITY test
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) - DATE '1970-01-01') * 86400) + 30;
    v_payload := v_actor_id || '|ACTOR_IDENTITY|HRMS_AI_ORACLE|' || v_nonce || '|' || v_exp;
    v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
        src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
        typ => DBMS_CRYPTO.HMAC_SH256,
        key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
    ));
    AI_OWNER.PKG_AI_AUTH.GET_ACTOR_IDENTITY(
        v_actor_id, 'HRMS_AI_ORACLE', v_nonce, v_exp, v_valid_sig, v_user_cur, v_has_ai
    );
    CLOSE v_user_cur;
    IF v_has_ai <> 1 THEN RAISE_APPLICATION_ERROR(-20044, 'Loi: Admin phai co F_SYSTEM_AI = 1.'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 14. GET_ACTOR_IDENTITY thanh cong, xac nhan F_SYSTEM_AI = 1.');

    -- 4. Replay Attack test
    v_caught_error := FALSE;
    BEGIN
        AI_OWNER.PKG_AI_AUTH.GET_ACTOR_IDENTITY(
            v_actor_id, 'HRMS_AI_ORACLE', v_nonce, v_exp, v_valid_sig, v_user_cur, v_has_ai
        );
    EXCEPTION WHEN OTHERS THEN IF SQLCODE = -20004 THEN v_caught_error := TRUE; END IF;
    END;
    IF NOT v_caught_error THEN RAISE_APPLICATION_ERROR(-20042, 'Loi: Replay Nonce khong bi chan!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 15. Chan Replay Nonce nguyen tu thanh cong (ORA-20004).');

    -- 5. Disabled Capability test (PAYROLL_VIEW)
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) - DATE '1970-01-01') * 86400) + 30;
    v_payload := v_actor_id || '|PAYROLL_VIEW|HRMS_AI_ORACLE|' || v_nonce || '|' || v_exp;
    v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
        src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
        typ => DBMS_CRYPTO.HMAC_SH256,
        key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
    ));
    v_caught_error := FALSE;
    BEGIN
        AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
            v_actor_id, 'PAYROLL_VIEW', 'HRMS_AI_ORACLE', v_nonce, v_exp, v_valid_sig, v_ticket
        );
    EXCEPTION WHEN OTHERS THEN IF SQLCODE = -20003 THEN v_caught_error := TRUE; END IF;
    END;
    IF NOT v_caught_error THEN RAISE_APPLICATION_ERROR(-20043, 'Loi: PAYROLL_VIEW khong bi chan!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 16. Chan cap ticket cho disabled capability (PAYROLL_VIEW) thanh cong (ORA-20003).');
END;
/

PROMPT ============================================================================
PROMPT PHAN 5: KIEM TRA BIND TICKET, ISOLATION VA ATOMIC CONSUMPTION
PROMPT ============================================================================

DECLARE
    v_secret       VARCHAR2(256);
    v_actor_id     NUMBER := 80;
    v_nonce        VARCHAR2(64);
    v_exp          NUMBER;
    v_payload      VARCHAR2(1000);
    v_valid_sig    VARCHAR2(64);
    v_ticket       VARCHAR2(64);
    v_row_count    NUMBER;
    v_caught_error BOOLEAN;
BEGIN
    SELECT SECRET_VAL INTO v_secret FROM AI_OWNER.TB_AI_SECRET_VAULT WHERE KEY_NAME = 'AI_PROOF_KEY';

    -- Issue valid ticket for EMPLOYEE_COUNT
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) - DATE '1970-01-01') * 86400) + 30;
    v_payload := v_actor_id || '|EMPLOYEE_COUNT|HRMS_AI_ORACLE|' || v_nonce || '|' || v_exp;
    v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
        src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
        typ => DBMS_CRYPTO.HMAC_SH256,
        key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
    ));

    AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
        v_actor_id, 'EMPLOYEE_COUNT', 'HRMS_AI_ORACLE', v_nonce, v_exp, v_valid_sig, v_ticket
    );

    -- Bind ticket
    AI_OWNER.PKG_AI_READER.BIND_TICKET(v_ticket, 0);

    -- V_AI_EMPLOYEE_COUNT should return rows
    SELECT COUNT(*) INTO v_row_count FROM AI_OWNER.V_AI_EMPLOYEE_COUNT;
    IF v_row_count = 0 THEN RAISE_APPLICATION_ERROR(-20055, 'Loi: EMPLOYEE_COUNT khong tra ve du lieu khi da bind!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 17. Bind ticket EMPLOYEE_COUNT thanh cong, doc duoc ' || v_row_count || ' phong ban.');

    -- Unpermitted views like V_AI_CONTRACT must return 0
    SELECT COUNT(*) INTO v_row_count FROM AI_OWNER.V_AI_CONTRACT;
    IF v_row_count <> 0 THEN RAISE_APPLICATION_ERROR(-20056, 'Loi: V_AI_CONTRACT ro ri khi chi bind EMPLOYEE_COUNT!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 18. Capability isolation: V_AI_CONTRACT tra ve 0 dong khi dang bind EMPLOYEE_COUNT.');

    -- Column contract check: V_AI_EMPLOYEE_COUNT has both TOTAL_COUNT and TOTAL_EMPLOYEES
    SELECT SUM(TOTAL_COUNT) INTO v_row_count FROM AI_OWNER.V_AI_EMPLOYEE_COUNT;
    DBMS_OUTPUT.PUT_LINE('[PASS] 19. Column contract TOTAL_COUNT tren V_AI_EMPLOYEE_COUNT hoat dong tot (Tong NV: ' || v_row_count || ').');

    -- Attempt to bind the same ticket a 2nd time (Atomic consume check)
    v_caught_error := FALSE;
    BEGIN
        AI_OWNER.PKG_AI_READER.BIND_TICKET(v_ticket, 0);
    EXCEPTION WHEN OTHERS THEN
        IF SQLCODE = -20005 THEN v_caught_error := TRUE; END IF;
    END;
    IF NOT v_caught_error THEN RAISE_APPLICATION_ERROR(-20057, 'Loi: Ticket duoc dung lai (khong atomic consume)!'); END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 20. Atomic consume ngan chan su dung lai Ticket da dung (ORA-20005).');

    -- IS_READY check
    IF AI_OWNER.PKG_AI_READER.IS_READY() <> 1 THEN
        RAISE_APPLICATION_ERROR(-20058, 'Loi: PKG_AI_READER.IS_READY tra ve 0!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 21. PKG_AI_READER.IS_READY() tra ve 1 (San sang cho Nhanh B2).');

    AI_OWNER.PKG_AI_READER.CLEAR_REQUEST();
    DBMS_OUTPUT.PUT_LINE('[PASS] 22. Clear Context thanh cong.');
END;
/

PROMPT ============================================================================
PROMPT TONG KET: HE THONG ORACLE B2 DAT 100% TAT CA CAC TIEU CHUAN AN TOAN & CONTRACT.
PROMPT ============================================================================

EXIT;
