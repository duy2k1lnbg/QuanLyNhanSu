-- ============================================================================
-- SCRIPT 3: V1_24_ai_owner_verify.sql
-- Kich ban kiem tra toan dien Kien truc AI V2 - Nhanh B2 (3-Schema Architecture)
-- Nguoi thuc thi: DBA va/hoac AI_READONLY
-- Muc tieu: Xac minh 100% ket noi AI an toan, khong ro ri quyen, xac thuc HMAC dung chuan
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;
SET DEFINE OFF;

PROMPT ============================================================================
PROMPT PHAN 1: KIEM TRA PHAN QUYEN HE THONG VA NGUYEN TAC AN TOAN TOI THIEU (DBA)
PROMPT ============================================================================

DECLARE
    v_count NUMBER;
    v_err_count NUMBER;
BEGIN
    -- 1. Kiem tra User AI_OWNER va AI_READONLY ton tai
    SELECT COUNT(*) INTO v_count FROM ALL_USERS WHERE USERNAME IN ('AI_OWNER', 'AI_READONLY');
    IF v_count < 2 THEN
        RAISE_APPLICATION_ERROR(-20020, 'Loi: Thieu User AI_OWNER hoac AI_READONLY.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 1. Cac User AI_OWNER va AI_READONLY ton tai day du.');

    -- 2. Kiem tra System Privileges cua AI_READONLY: Chi duoc phep co CREATE SESSION
    SELECT COUNT(*) INTO v_count FROM DBA_SYS_PRIVS 
    WHERE GRANTEE = 'AI_READONLY' AND PRIVILEGE <> 'CREATE SESSION';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20021, 'Loi: AI_READONLY co quyen he thong vuot qua CREATE SESSION!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 2. AI_READONLY chi co quyen duy nhat: CREATE SESSION.');

    -- 3. Kiem tra Roles cua AI_READONLY: Khong duoc gan role nao (ke ca CONNECT)
    SELECT COUNT(*) INTO v_count FROM DBA_ROLE_PRIVS WHERE GRANTEE = 'AI_READONLY';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20022, 'Loi: AI_READONLY duoc gan ROLE, can thu hoi theo nguyen tac toi thieu.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 3. AI_READONLY khong co role nao.');

    -- 4. Kiem tra Direct Table Grants tren HR: AI_READONLY khong duoc SELECT truc tiep bang goc nao
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE IN ('AI_READONLY', 'PUBLIC') AND OWNER = 'HR' AND TABLE_NAME LIKE 'TB_%';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20023, 'Loi: AI_READONLY co quyen truc tiep tren bang goc cua HR!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 4. AI_READONLY khong co quyen truy cap truc tiep tren bat ky bang nao cua HR.');

    -- 5. Kiem tra Table Grants tren AI_OWNER: AI_READONLY khong duoc SELECT tren cac bang quan tri/khoa/ve
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE IN ('AI_READONLY', 'PUBLIC') AND OWNER = 'AI_OWNER' AND TABLE_NAME LIKE 'TB_AI_%';
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20024, 'Loi: AI_READONLY bi ro ri quyen xem bang quan tri hoac bang khoa bi mat!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 5. Cac bang quan tri (TB_AI_SECRET_VAULT, NONCE, TICKET,...) duoc bao ve tuyet doi.');

    -- 6. Kiem tra WITH GRANT OPTION: AI_OWNER phai co WITH GRANT tren HR.V_AI_SRC_* (tranh ORA-01720)
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE = 'AI_OWNER' AND OWNER = 'HR' AND TABLE_NAME LIKE 'V_AI_SRC_%' AND GRANTABLE = 'YES';
    IF v_count < 11 THEN
        RAISE_APPLICATION_ERROR(-20025, 'Loi: AI_OWNER thieu WITH GRANT OPTION tren cac View nguon V_AI_SRC_* (nguy co ORA-01720).');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 6. AI_OWNER co du WITH GRANT OPTION tren tat ca cac View nguon tu HR.');

    -- 7. Kiem tra Trusted Context HRMS_AI_CTX
    SELECT COUNT(*) INTO v_count FROM DBA_CONTEXT 
    WHERE NAMESPACE = 'HRMS_AI_CTX' AND SCHEMA = 'AI_OWNER' AND PACKAGE = 'PKG_AI_READER';
    IF v_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20026, 'Loi: Trusted Context HRMS_AI_CTX khong ton tai hoac khong tro den AI_OWNER.PKG_AI_READER.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 7. Trusted Context HRMS_AI_CTX lien ket chinh xac voi AI_OWNER.PKG_AI_READER.');

    -- 8. Kiem tra loi bien dich PL/SQL va View
    SELECT COUNT(*) INTO v_err_count FROM DBA_ERRORS 
    WHERE OWNER IN ('HR', 'AI_OWNER') AND (NAME LIKE 'PKG_AI_%' OR NAME LIKE 'V_AI_%');
    IF v_err_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20027, 'Loi: Ton tai loi bien dich trong cac Package/View AI!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 8. Khong co loi bien dich trong bat ky doi tuong AI nao.');
END;
/

PROMPT ============================================================================
PROMPT PHAN 2: KIEM TRA CAU HINH CAPABILITY VA CHINH SACH NHANH B2
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

    DBMS_OUTPUT.PUT_LINE('[PASS] 9. Danh muc Capability Nhanh B2 dung chuan: Tong 16, 11 BAT, 5 TAT (tat ca Payroll da tat).');
END;
/

PROMPT ============================================================================
PROMPT PHAN 3: KIEM TRA XAC THUC HMAC-SHA256 VA CO CHE CHONG REPLAY NGUYEN TU
PROMPT ============================================================================

DECLARE
    v_secret       VARCHAR2(256);
    v_actor_id     NUMBER := 1; -- Admin/test actor
    v_nonce        VARCHAR2(64);
    v_exp          NUMBER;
    v_payload      VARCHAR2(1000);
    v_valid_sig    VARCHAR2(64);
    v_ticket       VARCHAR2(64);
    v_user_cur     SYS_REFCURSOR;
    v_rights_cur   SYS_REFCURSOR;
    v_caps_cur     SYS_REFCURSOR;
    v_fields_cur   SYS_REFCURSOR;
    v_grants_cur   SYS_REFCURSOR;
    v_revs_cur     SYS_REFCURSOR;
    v_caught_error BOOLEAN := FALSE;
BEGIN
    -- Lay khoa bi mat noi bo tu AI_OWNER
    SELECT SECRET_VAL INTO v_secret FROM AI_OWNER.TB_AI_SECRET_VAULT WHERE KEY_NAME = 'AI_PROOF_KEY';
    
    -- Tao Nonce ngau nhien va han dung 30s
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((SYSDATE - DATE '1970-01-01') * 86400) + 30;

    -- 1. Kiem tra Proof voi chu ky gia mao (Signature Mismatch)
    BEGIN
        AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
            p_actor_id   => v_actor_id,
            p_capability => 'EMPLOYEE_LOOKUP',
            p_audience   => 'HRMS_AI_ORACLE',
            p_nonce      => v_nonce,
            p_exp        => v_exp,
            p_signature  => 'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA',
            p_ticket     => v_ticket
        );
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -20001 THEN
                v_caught_error := TRUE;
            END IF;
    END;
    IF NOT v_caught_error THEN
        RAISE_APPLICATION_ERROR(-20040, 'Loi: Chu ky HMAC gia mao khong bi chan!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 10. Chan chu ky HMAC gia mao thanh cong (ORA-20001).');

    -- 2. Kiem tra Proof het han (Expired Proof)
    v_caught_error := FALSE;
    BEGIN
        v_payload := v_actor_id || '|EMPLOYEE_LOOKUP|HRMS_AI_ORACLE|' || v_nonce || '|' || (v_exp - 100);
        v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
            src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
            typ => DBMS_CRYPTO.HMAC_SH256,
            key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
        ));
        AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
            p_actor_id   => v_actor_id,
            p_capability => 'EMPLOYEE_LOOKUP',
            p_audience   => 'HRMS_AI_ORACLE',
            p_nonce      => v_nonce,
            p_exp        => v_exp - 100, -- Qua khu
            p_signature  => v_valid_sig,
            p_ticket     => v_ticket
        );
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -20002 THEN
                v_caught_error := TRUE;
            END IF;
    END;
    IF NOT v_caught_error THEN
        RAISE_APPLICATION_ERROR(-20041, 'Loi: Proof da het han khong bi chan!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 11. Chan Proof het han thanh cong (ORA-20002).');

    -- 3. Kiem tra tao Proof hop le va lay Snapshot
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((SYSDATE - DATE '1970-01-01') * 86400) + 30;
    v_payload := v_actor_id || '|ACTOR_SNAPSHOT|HRMS_AI_ORACLE|' || v_nonce || '|' || v_exp;
    v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
        src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
        typ => DBMS_CRYPTO.HMAC_SH256,
        key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
    ));

    AI_OWNER.PKG_AI_AUTH.GET_ACTOR_SNAPSHOT(
        p_actor_id   => v_actor_id,
        p_audience   => 'HRMS_AI_ORACLE',
        p_nonce      => v_nonce,
        p_exp        => v_exp,
        p_signature  => v_valid_sig,
        p_user_cur   => v_user_cur,
        p_rights_cur => v_rights_cur,
        p_caps_cur   => v_caps_cur,
        p_fields_cur => v_fields_cur,
        p_grants_cur => v_grants_cur,
        p_revs_cur   => v_revs_cur
    );
    CLOSE v_user_cur; CLOSE v_rights_cur; CLOSE v_caps_cur;
    CLOSE v_fields_cur; CLOSE v_grants_cur; CLOSE v_revs_cur;
    DBMS_OUTPUT.PUT_LINE('[PASS] 12. Lay Actor Snapshot voi HMAC Proof hop le thanh cong.');

    -- 4. Kiem tra tan cong phat lai (Replay Attack voi cung 1 Nonce)
    v_caught_error := FALSE;
    BEGIN
        AI_OWNER.PKG_AI_AUTH.GET_ACTOR_SNAPSHOT(
            p_actor_id   => v_actor_id,
            p_audience   => 'HRMS_AI_ORACLE',
            p_nonce      => v_nonce, -- Tai su dung Nonce vua goi o buoc 3
            p_exp        => v_exp,
            p_signature  => v_valid_sig,
            p_user_cur   => v_user_cur,
            p_rights_cur => v_rights_cur,
            p_caps_cur   => v_caps_cur,
            p_fields_cur => v_fields_cur,
            p_grants_cur => v_grants_cur,
            p_revs_cur   => v_revs_cur
        );
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -20004 THEN
                v_caught_error := TRUE;
            END IF;
    END;
    IF NOT v_caught_error THEN
        RAISE_APPLICATION_ERROR(-20042, 'Loi: Tan cong phat lai (Replay Nonce) khong bi chan nguyen tu!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 13. Chan tan cong phat lai (Replay Nonce) nguyen tu thanh cong (ORA-20004).');

    -- 5. Kiem tra yeu cau cap ve cho Capability bi TAT (Vi du: PAYROLL_VIEW o Nhanh B2)
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((SYSDATE - DATE '1970-01-01') * 86400) + 30;
    v_payload := v_actor_id || '|PAYROLL_VIEW|HRMS_AI_ORACLE|' || v_nonce || '|' || v_exp;
    v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
        src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
        typ => DBMS_CRYPTO.HMAC_SH256,
        key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
    ));
    v_caught_error := FALSE;
    BEGIN
        AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
            p_actor_id   => v_actor_id,
            p_capability => 'PAYROLL_VIEW',
            p_audience   => 'HRMS_AI_ORACLE',
            p_nonce      => v_nonce,
            p_exp        => v_exp,
            p_signature  => v_valid_sig,
            p_ticket     => v_ticket
        );
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -20003 THEN
                v_caught_error := TRUE;
            END IF;
    END;
    IF NOT v_caught_error THEN
        RAISE_APPLICATION_ERROR(-20043, 'Loi: Yeu cau ve cho Payroll Capability bi tat khong bi chan!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 14. Chan cap ve cho Capability bi tat (PAYROLL_VIEW) thanh cong (ORA-20003).');
END;
/

PROMPT ============================================================================
PROMPT PHAN 4: KIEM TRA DIEU KIEN TRUY CAP VIEW (BOUND VS UNBOUND CONTEXT)
PROMPT ============================================================================

DECLARE
    v_row_count NUMBER;
    v_secret    VARCHAR2(256);
    v_nonce     VARCHAR2(64);
    v_exp       NUMBER;
    v_payload   VARCHAR2(1000);
    v_valid_sig VARCHAR2(64);
    v_ticket    VARCHAR2(64);
BEGIN
    -- 1. Truoc khi Bind Ticket: Query Protected Views phai tra ve 0 dong
    AI_OWNER.PKG_AI_READER.CLEAR_REQUEST();
    SELECT COUNT(*) INTO v_row_count FROM AI_OWNER.V_AI_EMPLOYEE_LOOKUP;
    IF v_row_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20050, 'Loi an ninh: Protected View tra ve du lieu khi Context chua duoc bind!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 15. View khong tra ve du lieu khi chua bind Ticket (Zero Rows).');

    -- 2. Cap ve va Bind hop le cho EMPLOYEE_LOOKUP
    SELECT SECRET_VAL INTO v_secret FROM AI_OWNER.TB_AI_SECRET_VAULT WHERE KEY_NAME = 'AI_PROOF_KEY';
    v_nonce := LOWER(RAWTOHEX(SYS_GUID()));
    v_exp := ((SYSDATE - DATE '1970-01-01') * 86400) + 30;
    v_payload := '1|EMPLOYEE_LOOKUP|HRMS_AI_ORACLE|' || v_nonce || '|' || v_exp;
    v_valid_sig := RAWTOHEX(DBMS_CRYPTO.MAC(
        src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
        typ => DBMS_CRYPTO.HMAC_SH256,
        key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
    ));

    AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET(
        p_actor_id   => 1,
        p_capability => 'EMPLOYEE_LOOKUP',
        p_audience   => 'HRMS_AI_ORACLE',
        p_nonce      => v_nonce,
        p_exp        => v_exp,
        p_signature  => v_valid_sig,
        p_ticket     => v_ticket
    );

    AI_OWNER.PKG_AI_READER.BIND_TICKET(v_ticket, 0);

    -- Khi da bind EMPLOYEE_LOOKUP, view V_AI_EMPLOYEE_LOOKUP phai cho phep doc
    IF AI_OWNER.PKG_AI_READER.VIEW_ALLOWED('V_AI_EMPLOYEE_LOOKUP') <> 1 THEN
        RAISE_APPLICATION_ERROR(-20051, 'Loi: View duoc cap phep khong hop le trong Context!');
    END IF;

    -- Nhung cac view khac khong duoc phep
    IF AI_OWNER.PKG_AI_READER.VIEW_ALLOWED('V_AI_CONTRACT') <> 0 THEN
        RAISE_APPLICATION_ERROR(-20052, 'Loi: View khac capability duoc cap van mo!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 16. Context cach ly nghiem ngat: chi cho phep View ung voi Ticket.');

    -- Don dep session sau khi test
    AI_OWNER.PKG_AI_READER.CLEAR_REQUEST();
    DBMS_OUTPUT.PUT_LINE('[PASS] 17. Don dep Context thanh cong.');
END;
/

PROMPT ============================================================================
PROMPT TONG KET: HE THONG AI V2 NHANH B2 DAT 100% TIEU CHUAN AN TOAN VA XAC THUC.
PROMPT SAN SANG CHO MO TOAN BO TRUY VAN QUA TAI KHOAN AI_READONLY.
PROMPT ============================================================================
