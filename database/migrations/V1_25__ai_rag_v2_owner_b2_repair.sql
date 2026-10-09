-- ============================================================================
-- SCRIPT: V1_25__ai_rag_v2_owner_b2_repair.sql
-- Kich ban Repair Toan Dien cho AI_OWNER (Nhanh B2)
-- Muc tieu:
-- 1. Dong toan bo 13 Protected Views khi chua bind ve (V_AI_ORG_LOOKUP va V_AI_PERIOD tra ve 0 dong).
-- 2. Nang cap PKG_AI_AUTH: kiem tra F_SYSTEM_AI, active actor, capability function right, va scope grant.
-- 3. Bo sung GET_ACTOR_IDENTITY nhe cho greeting/identity check ma khong can load full policy.
-- 4. Nang cap PKG_AI_READER: Atomic ticket consume, kiem tra expiry/freshness, ROW_ALLOWED hoan chinh (ALL/DEPT/SELF/COMPANY/DENY).
-- 5. Dong bo column contract: TOTAL_COUNT trong V_AI_EMPLOYEE_COUNT; ENTITY_TYPE/ID/NAME trong V_AI_ORG_LOOKUP; BIRTHDAY_DAY/MONTH trong V_AI_EMPLOYEE; SOQD/NGAYKY trong V_AI_SALARY_CHANGE.
-- 6. Seed day du Field Policies cho 11 enabled capabilities va Scope Grants khoi diem.
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;
SET DEFINE OFF;

PROMPT [1/6] Cap nhat Seed Field Policies cho 11 Capabilities B2 trong AI_OWNER...

-- Seed Field Policies
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'MANV' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'HOTEN' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'IDPB' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'MACTY' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'TEN_PHONGBAN' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'TEN_CHUCVU' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'DATHOIVIEC' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'DATHOIVIEC' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');

-- Profile SENSITIVE: Masking cac truong nhay cam
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'MANV' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'HOTEN' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'IDPB' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'TEN_PHONGBAN' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'NGAYSINH' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'MASK');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'DIENTHOAI' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'MASK');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'DIACHI' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'MASK');

-- Count & Overtime policies
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_COUNT' cap,'TOTAL_COUNT' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'OVERTIME_VIEW' cap,'SOGIO' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'OVERTIME_SUM' cap,'TONG_SOGIO' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'ALLOWANCE_VIEW' cap,'SOTIEN' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'ADVANCE_VIEW' cap,'SOTIEN' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'CONTRACT_VIEW' cap,'HESOLUONG' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'SALARY_CHANGE_VIEW' cap,'HESOLUONGMOI' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'ATTENDANCE_SUMMARY' cap,'TONGNGAYCONG' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'FULL');

-- Cam tuyet doi mat khau va token trong moi capability
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'PASSWORD' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'DENY');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_LOOKUP' cap,'MATKHAU' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'DENY');
MERGE INTO TB_AI_FIELD_POLICY t USING (SELECT 'EMPLOYEE_PROFILE' cap,'PASSWORD' field FROM DUAL) s ON (t.CAPABILITY_CODE=s.cap AND t.LOGICAL_FIELD=s.field) WHEN NOT MATCHED THEN INSERT(CAPABILITY_CODE,LOGICAL_FIELD,ACCESS_MODE) VALUES(s.cap,s.field,'DENY');
COMMIT;

PROMPT [2/6] Khoi tao Scope Grants khoi diem cho Admin va cac User mau...
-- Admin (IDUSER = 1) duoc cap quyen ALL tren 11 enabled capabilities
MERGE INTO TB_AI_SCOPE_GRANT t USING (
    SELECT 1 AS usr_id, 'USER' AS stype, 'EMPLOYEE_LOOKUP' AS cap, 'ALL' AS scope FROM DUAL UNION ALL
    SELECT 1, 'USER', 'EMPLOYEE_PROFILE', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'EMPLOYEE_COUNT', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'ATTENDANCE_SUMMARY', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'OVERTIME_VIEW', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'OVERTIME_SUM', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'ALLOWANCE_VIEW', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'INSURANCE_SELF', 'SELF' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'ADVANCE_VIEW', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'CONTRACT_VIEW', 'ALL' FROM DUAL UNION ALL
    SELECT 1, 'USER', 'SALARY_CHANGE_VIEW', 'ALL' FROM DUAL
) s ON (t.SUBJECT_ID = s.usr_id AND t.SUBJECT_TYPE = s.stype AND t.CAPABILITY_CODE = s.cap)
WHEN NOT MATCHED THEN INSERT (SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, EFFECT, IS_ENABLED)
VALUES (s.stype, s.usr_id, s.cap, s.scope, 'ALLOW', 1);

-- Default grant cho nhan vien tu tra cuu ho so cua minh (SELF)
MERGE INTO TB_AI_SCOPE_GRANT t USING (
    SELECT 1 AS grp_id, 'GROUP' AS stype, 'EMPLOYEE_LOOKUP' AS cap, 'SELF' AS scope FROM DUAL UNION ALL
    SELECT 1, 'GROUP', 'INSURANCE_SELF', 'SELF' FROM DUAL
) s ON (t.SUBJECT_ID = s.grp_id AND t.SUBJECT_TYPE = s.stype AND t.CAPABILITY_CODE = s.cap)
WHEN NOT MATCHED THEN INSERT (SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, EFFECT, IS_ENABLED)
VALUES (s.stype, s.grp_id, s.cap, s.scope, 'ALLOW', 1);
COMMIT;

PROMPT [3/6] Nang cap Package PKG_AI_AUTH: Xac thuc Proof, Active User, Function Rights, Scope Grants...
CREATE OR REPLACE PACKAGE PKG_AI_AUTH AUTHID DEFINER AS
    PROCEDURE VERIFY_AND_ISSUE_TICKET(
        p_actor_id    IN NUMBER,
        p_capability  IN VARCHAR2,
        p_audience    IN VARCHAR2,
        p_nonce       IN VARCHAR2,
        p_exp         IN NUMBER,
        p_signature   IN VARCHAR2,
        p_ticket      OUT VARCHAR2
    );

    PROCEDURE GET_ACTOR_SNAPSHOT(
        p_actor_id    IN NUMBER,
        p_audience    IN VARCHAR2,
        p_nonce       IN VARCHAR2,
        p_exp         IN NUMBER,
        p_signature   IN VARCHAR2,
        p_user_cur    OUT SYS_REFCURSOR,
        p_rights_cur  OUT SYS_REFCURSOR,
        p_caps_cur    OUT SYS_REFCURSOR,
        p_fields_cur  OUT SYS_REFCURSOR,
        p_grants_cur  OUT SYS_REFCURSOR,
        p_revs_cur    OUT SYS_REFCURSOR
    );

    PROCEDURE GET_ACTOR_IDENTITY(
        p_actor_id    IN NUMBER,
        p_audience    IN VARCHAR2,
        p_nonce       IN VARCHAR2,
        p_exp         IN NUMBER,
        p_signature   IN VARCHAR2,
        p_user_cur    OUT SYS_REFCURSOR,
        p_has_ai_right OUT NUMBER
    );
END PKG_AI_AUTH;
/

CREATE OR REPLACE PACKAGE BODY PKG_AI_AUTH AS
    FUNCTION GET_SECRET RETURN VARCHAR2 IS
        v_secret VARCHAR2(256);
    BEGIN
        SELECT SECRET_VAL INTO v_secret FROM TB_AI_SECRET_VAULT WHERE KEY_NAME = 'AI_PROOF_KEY';
        RETURN v_secret;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20010, 'Security configuration error: HMAC Secret key not found.');
    END;

    PROCEDURE VERIFY_PROOF_INTERNAL(
        p_actor_id   IN NUMBER,
        p_action     IN VARCHAR2,
        p_audience   IN VARCHAR2,
        p_nonce      IN VARCHAR2,
        p_exp        IN NUMBER,
        p_signature  IN VARCHAR2
    ) IS
        PRAGMA AUTONOMOUS_TRANSACTION;
        v_secret      VARCHAR2(256);
        v_payload     VARCHAR2(1000);
        v_computed    VARCHAR2(64);
        v_now_epoch   NUMBER;
        v_exp_date    DATE;
    BEGIN
        IF p_actor_id <= 0 OR p_action IS NULL OR p_nonce IS NULL OR p_signature IS NULL THEN
            RAISE_APPLICATION_ERROR(-20001, 'Invalid proof parameters.');
        END IF;

        IF p_audience <> 'HRMS_AI_ORACLE' THEN
            RAISE_APPLICATION_ERROR(-20001, 'Invalid audience in proof.');
        END IF;

        v_now_epoch := (CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS DATE) - DATE '1970-01-01') * 86400;
        IF p_exp < v_now_epoch THEN
            RAISE_APPLICATION_ERROR(-20002, 'Actor proof has expired.');
        END IF;

        v_secret := GET_SECRET();
        v_payload := p_actor_id || '|' || p_action || '|' || p_audience || '|' || p_nonce || '|' || p_exp;
        v_computed := RAWTOHEX(
            DBMS_CRYPTO.MAC(
                src => UTL_I18N.STRING_TO_RAW(v_payload, 'AL32UTF8'),
                typ => DBMS_CRYPTO.HMAC_SH256,
                key => UTL_I18N.STRING_TO_RAW(v_secret, 'AL32UTF8')
            )
        );

        IF UPPER(v_computed) <> UPPER(p_signature) THEN
            RAISE_APPLICATION_ERROR(-20001, 'Cryptographic proof signature mismatch.');
        END IF;

        v_exp_date := SYSDATE + ((p_exp - v_now_epoch) / 86400);
        BEGIN
            INSERT INTO TB_AI_PROOF_NONCE (NONCE, ACTOR_ID, EXPIRES_AT)
            VALUES (p_nonce, p_actor_id, v_exp_date);
            DELETE FROM TB_AI_PROOF_NONCE WHERE EXPIRES_AT < SYSDATE;
            COMMIT;
        EXCEPTION
            WHEN DUP_VAL_ON_INDEX THEN
                ROLLBACK;
                RAISE_APPLICATION_ERROR(-20004, 'Replay attack detected: Nonce has already been consumed.');
        END;
    END;

    PROCEDURE VERIFY_AND_ISSUE_TICKET(
        p_actor_id    IN NUMBER,
        p_capability  IN VARCHAR2,
        p_audience    IN VARCHAR2,
        p_nonce       IN VARCHAR2,
        p_exp         IN NUMBER,
        p_signature   IN VARCHAR2,
        p_ticket      OUT VARCHAR2
    ) IS
        v_pol_rev     NUMBER(19);
        v_sid         NUMBER;
        v_sessionid   NUMBER;
        v_cap_enabled NUMBER;
        v_req_func    VARCHAR2(50);
        v_has_ai      NUMBER := 0;
        v_has_func    NUMBER := 0;
        v_has_grant   NUMBER := 0;
        v_deny_grant  NUMBER := 0;
    BEGIN
        -- 1. Xac minh HMAC Proof
        VERIFY_PROOF_INTERNAL(p_actor_id, p_capability, p_audience, p_nonce, p_exp, p_signature);

        -- 2. Kiem tra User dang hoat dong (Disabled = 0, IsGroup = 0) va quyen F_SYSTEM_AI
        SELECT COUNT(*) INTO v_has_ai
          FROM HR.V_AI_SRC_AUTH
         WHERE IDUSER = p_actor_id
           AND NVL(DISABLED, 0) = 0
           AND NVL(ISGROUP, 0) = 0
           AND FUNCTION_CODE = 'F_SYSTEM_AI'
           AND (CAN_VIEW = 1 OR USER_RIGHT = 1);

        IF v_has_ai = 0 THEN
            RAISE_APPLICATION_ERROR(-20006, 'Actor is inactive or missing F_SYSTEM_AI authorization.');
        END IF;

        -- 3. Kiem tra Capability co ton tai va dang BAT khong
        SELECT COUNT(*), MAX(REQUIRED_FUNCTION_CODE)
          INTO v_cap_enabled, v_req_func
          FROM TB_AI_CAPABILITY
         WHERE CAPABILITY_CODE = p_capability AND IS_ENABLED = 1;

        IF v_cap_enabled = 0 THEN
            RAISE_APPLICATION_ERROR(-20003, 'Capability is disabled or not mapped in this deployment profile.');
        END IF;

        -- 4. Kiem tra Function Right bat buoc cua capability (neu co yeu cau)
        IF v_req_func IS NOT NULL THEN
            SELECT COUNT(*) INTO v_has_func
              FROM HR.V_AI_SRC_AUTH
             WHERE IDUSER = p_actor_id
               AND FUNCTION_CODE = v_req_func
               AND (CAN_VIEW = 1 OR USER_RIGHT = 1);

            IF v_has_func = 0 THEN
                RAISE_APPLICATION_ERROR(-20007, 'Actor is missing required function right: ' || v_req_func);
            END IF;
        END IF;

        -- 5. Kiem tra Scope Grant (DENY thang ALLOW)
        SELECT NVL(SUM(CASE WHEN EFFECT = 'ALLOW' THEN 1 ELSE 0 END), 0),
               NVL(SUM(CASE WHEN EFFECT = 'DENY' THEN 1 ELSE 0 END), 0)
          INTO v_has_grant, v_deny_grant
          FROM TB_AI_SCOPE_GRANT g
         WHERE g.CAPABILITY_CODE = p_capability
           AND g.IS_ENABLED = 1
           AND (g.VALID_FROM IS NULL OR g.VALID_FROM <= SYSDATE)
           AND (g.VALID_TO IS NULL OR g.VALID_TO > SYSDATE)
           AND (
               (g.SUBJECT_TYPE = 'USER' AND g.SUBJECT_ID = p_actor_id)
               OR (g.SUBJECT_TYPE = 'GROUP' AND g.SUBJECT_ID IN (
                   SELECT gm.ID_GROUP FROM HR.V_AI_SRC_AUTH gm
                    WHERE gm.IDUSER = p_actor_id AND gm.ID_GROUP IS NOT NULL
               ))
           );

        IF v_deny_grant > 0 THEN
            RAISE_APPLICATION_ERROR(-20008, 'Access denied: Explicit DENY scope policy matches actor.');
        END IF;

        -- Cho phep neu co grant hoac neu la self-capability
        IF v_has_grant = 0 AND p_capability NOT IN ('INSURANCE_SELF', 'PAYROLL_SELF') THEN
            RAISE_APPLICATION_ERROR(-20009, 'Access denied: No active scope grant matches actor for this capability.');
        END IF;

        -- 6. Lay phien ban chinh sach hien hanh
        SELECT REVISION_NUMBER INTO v_pol_rev FROM TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL';

        -- 7. Sinh Ticket ngau nhien va gan voi Session Oracle hien tai
        p_ticket := LOWER(RAWTOHEX(SYS_GUID()));
        v_sid := SYS_CONTEXT('USERENV', 'SID');
        v_sessionid := SYS_CONTEXT('USERENV', 'SESSIONID');

        INSERT INTO TB_AI_READ_TICKET (TICKET_ID, ACTOR_USER_ID, CAPABILITY_CODE, POLICY_REVISION, ORACLE_SID, ORACLE_SESSIONID, EXPIRES_AT)
        VALUES (p_ticket, p_actor_id, p_capability, v_pol_rev, v_sid, v_sessionid, SYSDATE + (40/86400));
    END;

    PROCEDURE GET_ACTOR_SNAPSHOT(
        p_actor_id    IN NUMBER,
        p_audience    IN VARCHAR2,
        p_nonce       IN VARCHAR2,
        p_exp         IN NUMBER,
        p_signature   IN VARCHAR2,
        p_user_cur    OUT SYS_REFCURSOR,
        p_rights_cur  OUT SYS_REFCURSOR,
        p_caps_cur    OUT SYS_REFCURSOR,
        p_fields_cur  OUT SYS_REFCURSOR,
        p_grants_cur  OUT SYS_REFCURSOR,
        p_revs_cur    OUT SYS_REFCURSOR
    ) IS
        v_pol_rev NUMBER(19);
    BEGIN
        VERIFY_PROOF_INTERNAL(p_actor_id, 'ACTOR_SNAPSHOT', p_audience, p_nonce, p_exp, p_signature);

        SELECT REVISION_NUMBER INTO v_pol_rev FROM TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL';

        -- User cursor
        OPEN p_user_cur FOR
            SELECT DISTINCT u.IDUSER, u.USERNAME, u.FULLNAME, u.MANV, u.MACTY, v_pol_rev as POLICY_VERSION
            FROM HR.V_AI_SRC_AUTH u
            WHERE u.IDUSER = p_actor_id AND NVL(u.DISABLED, 0) = 0 AND NVL(u.ISGROUP, 0) = 0;

        -- Rights cursor
        OPEN p_rights_cur FOR
            SELECT DISTINCT FUNCTION_CODE FROM HR.V_AI_SRC_AUTH
            WHERE IDUSER = p_actor_id AND FUNCTION_CODE IS NOT NULL AND (CAN_VIEW = 1 OR USER_RIGHT = 1);

        -- Capabilities cursor
        OPEN p_caps_cur FOR
            SELECT CAPABILITY_CODE, REQUIRED_FUNCTION_CODE, SOURCE_VIEW, IS_ENABLED
            FROM TB_AI_CAPABILITY;

        -- Field policies cursor
        OPEN p_fields_cur FOR
            SELECT CAPABILITY_CODE, LOGICAL_FIELD, ACCESS_MODE, ALLOWED_OPERATIONS
            FROM TB_AI_FIELD_POLICY;

        -- Scope grants cursor
        OPEN p_grants_cur FOR
            SELECT g.CAPABILITY_CODE, g.SCOPE_TYPE, g.SCOPE_KEY, g.EFFECT, g.VALID_TO
            FROM TB_AI_SCOPE_GRANT g
            WHERE g.IS_ENABLED = 1
              AND (g.VALID_FROM IS NULL OR g.VALID_FROM <= SYSDATE)
              AND (g.VALID_TO IS NULL OR g.VALID_TO > SYSDATE)
              AND (
                  (g.SUBJECT_TYPE = 'USER' AND g.SUBJECT_ID = p_actor_id)
                  OR (g.SUBJECT_TYPE = 'GROUP' AND g.SUBJECT_ID IN (
                      SELECT ID_GROUP FROM HR.V_AI_SRC_AUTH WHERE IDUSER = p_actor_id AND ID_GROUP IS NOT NULL
                  ))
              );

        -- Revisions cursor
        OPEN p_revs_cur FOR
            SELECT REVISION_KEY, REVISION_NUMBER FROM TB_AI_REVISION;
    END;

    PROCEDURE GET_ACTOR_IDENTITY(
        p_actor_id    IN NUMBER,
        p_audience    IN VARCHAR2,
        p_nonce       IN VARCHAR2,
        p_exp         IN NUMBER,
        p_signature   IN VARCHAR2,
        p_user_cur    OUT SYS_REFCURSOR,
        p_has_ai_right OUT NUMBER
    ) IS
    BEGIN
        VERIFY_PROOF_INTERNAL(p_actor_id, 'ACTOR_IDENTITY', p_audience, p_nonce, p_exp, p_signature);

        SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END INTO p_has_ai_right
          FROM HR.V_AI_SRC_AUTH
         WHERE IDUSER = p_actor_id
           AND NVL(DISABLED, 0) = 0
           AND NVL(ISGROUP, 0) = 0
           AND FUNCTION_CODE = 'F_SYSTEM_AI'
           AND (CAN_VIEW = 1 OR USER_RIGHT = 1);

        OPEN p_user_cur FOR
            SELECT DISTINCT u.IDUSER, u.USERNAME, u.FULLNAME, u.MANV, u.MACTY
              FROM HR.V_AI_SRC_AUTH u
             WHERE u.IDUSER = p_actor_id AND NVL(u.DISABLED, 0) = 0 AND NVL(u.ISGROUP, 0) = 0;
    END;
END PKG_AI_AUTH;
/

PROMPT [4/6] Nang cap Package PKG_AI_READER: Atomic Consume, Context Enforcement, Dynamic Readiness...
CREATE OR REPLACE PACKAGE PKG_AI_READER AUTHID DEFINER AS
    PROCEDURE BIND_TICKET(p_ticket IN VARCHAR2, p_self_only IN NUMBER DEFAULT 0);
    PROCEDURE CLEAR_REQUEST;
    FUNCTION VIEW_ALLOWED(p_view_name IN VARCHAR2) RETURN NUMBER;
    FUNCTION ROW_ALLOWED(p_manv IN NUMBER, p_idpb IN NUMBER, p_macty IN VARCHAR2) RETURN NUMBER;
    FUNCTION FIELD_MODE(p_field_name IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION IS_READY RETURN NUMBER;
    PROCEDURE BUMP_REVISION(p_key IN VARCHAR2);
END PKG_AI_READER;
/

CREATE OR REPLACE PACKAGE BODY PKG_AI_READER AS
    PROCEDURE BIND_TICKET(p_ticket IN VARCHAR2, p_self_only IN NUMBER DEFAULT 0) IS
        v_actor_id    NUMBER;
        v_cap         VARCHAR2(50);
        v_manv        NUMBER;
        v_idpb        NUMBER;
        v_macty       VARCHAR2(50);
        v_src_view    VARCHAR2(100);
        v_sid         NUMBER;
        v_sessionid   NUMBER;
        v_exp         DATE;
        v_cur_pol_rev NUMBER(19);
        v_ticket_rev  NUMBER(19);
    BEGIN
        v_sid := SYS_CONTEXT('USERENV', 'SID');
        v_sessionid := SYS_CONTEXT('USERENV', 'SESSIONID');

        -- Kiem tra Policy revision hien hanh
        SELECT REVISION_NUMBER INTO v_cur_pol_rev FROM TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL';

        -- 1. Chuyen trang thai nguyen tu (Atomic Consume)
        UPDATE TB_AI_READ_TICKET
           SET IS_CONSUMED = 1
         WHERE TICKET_ID = p_ticket
           AND IS_CONSUMED = 0
           AND EXPIRES_AT > SYSDATE
           AND ORACLE_SID = v_sid
           AND ORACLE_SESSIONID = v_sessionid
           AND POLICY_REVISION = v_cur_pol_rev
        RETURNING ACTOR_USER_ID, CAPABILITY_CODE, EXPIRES_AT, POLICY_REVISION
             INTO v_actor_id, v_cap, v_exp, v_ticket_rev;

        IF SQL%ROWCOUNT = 0 THEN
            CLEAR_REQUEST();
            RAISE_APPLICATION_ERROR(-20005, 'Security violation: Ticket is invalid, expired, session mismatch, or revision stale.');
        END IF;

        SELECT SOURCE_VIEW INTO v_src_view FROM TB_AI_CAPABILITY WHERE CAPABILITY_CODE = v_cap;
        SELECT MANV, IDPB, MACTY INTO v_manv, v_idpb, v_macty FROM HR.V_AI_SRC_AUTH WHERE IDUSER = v_actor_id AND ROWNUM = 1;

        -- 2. Thiet lap Context an toan
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'BOUND', '1');
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'ACTOR_ID', TO_CHAR(v_actor_id));
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'CAPABILITY', v_cap);
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'ALLOWED_VIEW', v_src_view);
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'ACTOR_MANV', NVL(TO_CHAR(v_manv), '0'));
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'ACTOR_IDPB', NVL(TO_CHAR(v_idpb), '0'));
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'ACTOR_MACTY', NVL(v_macty, '0'));
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'SELF_ONLY', TO_CHAR(p_self_only));
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'TICKET_EXP', TO_CHAR(v_exp, 'YYYYMMDDHH24MISS'));
        DBMS_SESSION.SET_CONTEXT('HRMS_AI_CTX', 'POLICY_REV', TO_CHAR(v_ticket_rev));
    EXCEPTION
        WHEN OTHERS THEN
            CLEAR_REQUEST();
            RAISE;
    END;

    PROCEDURE CLEAR_REQUEST IS
    BEGIN
        DBMS_SESSION.CLEAR_CONTEXT('HRMS_AI_CTX');
    END;

    FUNCTION VIEW_ALLOWED(p_view_name IN VARCHAR2) RETURN NUMBER IS
        v_bound       VARCHAR2(10);
        v_src_view    VARCHAR2(100);
        v_exp_str     VARCHAR2(30);
        v_pol_rev_str VARCHAR2(30);
        v_cur_pol_rev NUMBER(19);
        v_cap         VARCHAR2(50);
    BEGIN
        v_bound := SYS_CONTEXT('HRMS_AI_CTX', 'BOUND');
        IF v_bound <> '1' THEN RETURN 0; END IF;

        -- Kiem tra ve het han trong context
        v_exp_str := SYS_CONTEXT('HRMS_AI_CTX', 'TICKET_EXP');
        IF v_exp_str IS NOT NULL THEN
            IF SYSDATE > TO_DATE(v_exp_str, 'YYYYMMDDHH24MISS') THEN
                RETURN 0;
            END IF;
        END IF;

        -- Kiem tra Policy revision con tuoi khong
        v_pol_rev_str := SYS_CONTEXT('HRMS_AI_CTX', 'POLICY_REV');
        IF v_pol_rev_str IS NOT NULL THEN
            BEGIN
                SELECT REVISION_NUMBER INTO v_cur_pol_rev FROM TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL';
                IF v_cur_pol_rev <> TO_NUMBER(v_pol_rev_str) THEN
                    RETURN 0;
                END IF;
            EXCEPTION
                WHEN OTHERS THEN RETURN 0;
            END;
        END IF;

        v_src_view := UPPER(SYS_CONTEXT('HRMS_AI_CTX', 'ALLOWED_VIEW'));
        IF UPPER(p_view_name) = v_src_view THEN
            RETURN 1;
        END IF;

        -- Helper views cho phep doc khi co bat ky ve hop le nao dang bind
        v_cap := SYS_CONTEXT('HRMS_AI_CTX', 'CAPABILITY');
        IF UPPER(p_view_name) IN ('V_AI_ORG_LOOKUP', 'V_AI_PERIOD') THEN
            RETURN 1;
        END IF;

        IF UPPER(p_view_name) = 'V_AI_EMPLOYEE_LOOKUP' AND v_cap NOT IN ('EMPLOYEE_COUNT', 'PAYROLL_SUMMARY') THEN
            RETURN 1;
        END IF;

        RETURN 0;
    EXCEPTION
        WHEN OTHERS THEN RETURN 0;
    END;

    FUNCTION ROW_ALLOWED(p_manv IN NUMBER, p_idpb IN NUMBER, p_macty IN VARCHAR2) RETURN NUMBER IS
        v_bound       VARCHAR2(10);
        v_actor_id    NUMBER;
        v_actor_manv  NUMBER;
        v_self_only   NUMBER;
        v_cap         VARCHAR2(50);
        v_allow       NUMBER := 0;
        v_deny        NUMBER := 0;
    BEGIN
        v_bound := SYS_CONTEXT('HRMS_AI_CTX', 'BOUND');
        IF v_bound <> '1' THEN RETURN 0; END IF;

        v_actor_id := TO_NUMBER(SYS_CONTEXT('HRMS_AI_CTX', 'ACTOR_ID'));
        v_actor_manv := TO_NUMBER(SYS_CONTEXT('HRMS_AI_CTX', 'ACTOR_MANV'));
        v_self_only := TO_NUMBER(SYS_CONTEXT('HRMS_AI_CTX', 'SELF_ONLY'));
        v_cap := SYS_CONTEXT('HRMS_AI_CTX', 'CAPABILITY');

        -- 1. Bat buoc self-only
        IF v_self_only = 1 OR v_cap IN ('INSURANCE_SELF', 'PAYROLL_SELF') THEN
            IF p_manv IS NOT NULL AND v_actor_manv IS NOT NULL AND p_manv = v_actor_manv THEN
                RETURN 1;
            ELSE
                RETURN 0;
            END IF;
        END IF;

        -- 2. Danh gia Scope Grants
        SELECT NVL(SUM(CASE WHEN EFFECT = 'ALLOW' THEN 1 ELSE 0 END), 0),
               NVL(SUM(CASE WHEN EFFECT = 'DENY' THEN 1 ELSE 0 END), 0)
          INTO v_allow, v_deny
          FROM TB_AI_SCOPE_GRANT g
         WHERE g.CAPABILITY_CODE = v_cap
           AND g.IS_ENABLED = 1
           AND (g.VALID_FROM IS NULL OR g.VALID_FROM <= SYSDATE)
           AND (g.VALID_TO IS NULL OR g.VALID_TO > SYSDATE)
           AND (
               (g.SUBJECT_TYPE = 'USER' AND g.SUBJECT_ID = v_actor_id)
               OR (g.SUBJECT_TYPE = 'GROUP' AND g.SUBJECT_ID IN (
                   SELECT gm.ID_GROUP FROM HR.V_AI_SRC_AUTH gm
                    WHERE gm.IDUSER = v_actor_id AND gm.ID_GROUP IS NOT NULL
               ))
           )
           AND (
               g.SCOPE_TYPE = 'ALL'
               OR (g.SCOPE_TYPE = 'SELF' AND p_manv = v_actor_manv)
               OR (g.SCOPE_TYPE = 'DEPARTMENT' AND g.SCOPE_KEY = TO_CHAR(p_idpb, 'TM9'))
               OR (g.SCOPE_TYPE = 'COMPANY' AND g.SCOPE_KEY = p_macty)
           );

        -- Quy tac: DENY thang ALLOW
        IF v_deny > 0 THEN RETURN 0; END IF;
        IF v_allow > 0 THEN RETURN 1; END IF;

        RETURN 0;
    EXCEPTION
        WHEN OTHERS THEN RETURN 0;
    END;

    FUNCTION FIELD_MODE(p_field_name IN VARCHAR2) RETURN VARCHAR2 IS
        v_bound VARCHAR2(10);
        v_cap   VARCHAR2(50);
        v_mode  VARCHAR2(20);
    BEGIN
        v_bound := SYS_CONTEXT('HRMS_AI_CTX', 'BOUND');
        IF v_bound <> '1' THEN RETURN 'DENY'; END IF;

        v_cap := SYS_CONTEXT('HRMS_AI_CTX', 'CAPABILITY');
        SELECT ACCESS_MODE INTO v_mode
          FROM TB_AI_FIELD_POLICY
         WHERE CAPABILITY_CODE = v_cap
           AND LOGICAL_FIELD = UPPER(p_field_name);
        RETURN v_mode;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            IF UPPER(p_field_name) IN ('PASSWORD', 'MATKHAU', 'TOKEN', 'REFRESH_TOKEN', 'CCCD') THEN
                RETURN 'DENY';
            END IF;
            RETURN 'FULL';
        WHEN OTHERS THEN RETURN 'DENY';
    END;

    FUNCTION IS_READY RETURN NUMBER IS
        v_views_count  NUMBER := 0;
        v_pkgs_count   NUMBER := 0;
        v_caps_count   NUMBER := 0;
        v_rev_count    NUMBER := 0;
    BEGIN
        -- 1. Kiem tra du 13 Views trong AI_OWNER o trang thai VALID
        SELECT COUNT(*) INTO v_views_count
          FROM ALL_VIEWS
         WHERE OWNER = 'AI_OWNER' AND VIEW_NAME LIKE 'V_AI_%';

        IF v_views_count < 13 THEN RETURN 0; END IF;

        -- 2. Kiem tra 2 Package Bodies o trang thai VALID
        SELECT COUNT(*) INTO v_pkgs_count
          FROM ALL_OBJECTS
         WHERE OWNER = 'AI_OWNER'
           AND OBJECT_TYPE = 'PACKAGE BODY'
           AND OBJECT_NAME IN ('PKG_AI_AUTH', 'PKG_AI_READER')
           AND STATUS = 'VALID';

        IF v_pkgs_count < 2 THEN RETURN 0; END IF;

        -- 3. Kiem tra dung 11 Capabilities BẬT o Nhanh B2
        SELECT COUNT(*) INTO v_caps_count
          FROM TB_AI_CAPABILITY
         WHERE IS_ENABLED = 1;

        IF v_caps_count <> 11 THEN RETURN 0; END IF;

        -- 4. Kiem tra Revisions
        SELECT COUNT(*) INTO v_rev_count
          FROM TB_AI_REVISION
         WHERE REVISION_KEY = 'POLICY_GLOBAL';

        IF v_rev_count <> 1 THEN RETURN 0; END IF;

        RETURN 1;
    EXCEPTION
        WHEN OTHERS THEN RETURN 0;
    END;

    PROCEDURE BUMP_REVISION(p_key IN VARCHAR2) IS
        PRAGMA AUTONOMOUS_TRANSACTION;
    BEGIN
        UPDATE TB_AI_REVISION
           SET REVISION_NUMBER = REVISION_NUMBER + 1,
               UPDATED_AT = SYSDATE
         WHERE REVISION_KEY = p_key;
        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN ROLLBACK;
    END;
END PKG_AI_READER;
/

PROMPT [5/6] Tai tao toan bo 13 Protected Views voi Column Contract va Row Guards chuan...

-- 1. V_AI_EMPLOYEE_LOOKUP
CREATE OR REPLACE VIEW V_AI_EMPLOYEE_LOOKUP AS
SELECT e.MANV, e.HOTEN, e.IDPB, e.TEN_PHONGBAN, e.IDCV, e.TEN_CHUCVU, e.MACTY,
       NVL(e.DATHOIVIEC, 0) AS DATHOIVIEC
FROM HR.V_AI_SRC_EMPLOYEE e
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_EMPLOYEE_LOOKUP') = 1
  AND PKG_AI_READER.ROW_ALLOWED(e.MANV, e.IDPB, e.MACTY) = 1
  AND e.DELETED_DATE IS NULL AND NVL(e.DATHOIVIEC, 0) = 0;

-- 2. V_AI_EMPLOYEE
CREATE OR REPLACE VIEW V_AI_EMPLOYEE AS
SELECT e.MANV,
       e.HOTEN,
       CASE WHEN PKG_AI_READER.FIELD_MODE('NGAYSINH') = 'FULL' THEN e.NGAYSINH ELSE NULL END AS NGAYSINH,
       CASE WHEN PKG_AI_READER.FIELD_MODE('DIACHI') = 'FULL' THEN e.DIACHI ELSE NULL END AS DIACHI,
       CASE WHEN PKG_AI_READER.FIELD_MODE('DIENTHOAI') = 'FULL' THEN e.DIENTHOAI ELSE NULL END AS DIENTHOAI,
       e.IDPB,
       e.TEN_PHONGBAN,
       e.IDCV,
       e.TEN_CHUCVU,
       e.MACTY,
       NVL(e.DATHOIVIEC, 0) AS DATHOIVIEC,
       EXTRACT(DAY FROM e.NGAYSINH) AS BIRTHDAY_DAY,
       EXTRACT(MONTH FROM e.NGAYSINH) AS BIRTHDAY_MONTH
FROM HR.V_AI_SRC_EMPLOYEE e
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_EMPLOYEE') = 1
  AND PKG_AI_READER.ROW_ALLOWED(e.MANV, e.IDPB, e.MACTY) = 1
  AND e.DELETED_DATE IS NULL AND NVL(e.DATHOIVIEC, 0) = 0;

-- 3. V_AI_EMPLOYEE_COUNT: Cung cap ca TOTAL_COUNT va TOTAL_EMPLOYEES
CREATE OR REPLACE VIEW V_AI_EMPLOYEE_COUNT AS
SELECT e.IDPB, e.TEN_PHONGBAN, e.MACTY,
       COUNT(*) AS TOTAL_COUNT,
       COUNT(*) AS TOTAL_EMPLOYEES
FROM HR.V_AI_SRC_EMPLOYEE e
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_EMPLOYEE_COUNT') = 1
  AND PKG_AI_READER.ROW_ALLOWED(e.MANV, e.IDPB, e.MACTY) = 1
  AND e.DELETED_DATE IS NULL AND NVL(e.DATHOIVIEC, 0) = 0
GROUP BY e.IDPB, e.TEN_PHONGBAN, e.MACTY;

-- 4. V_AI_ORG_LOOKUP: Dong khi chua co ve, cung cap ENTITY_TYPE, ENTITY_ID, ENTITY_NAME
CREATE OR REPLACE VIEW V_AI_ORG_LOOKUP AS
SELECT DISTINCT TO_NCHAR('DEPARTMENT') AS ENTITY_TYPE,
       TO_NCHAR(TO_CHAR(pb.IDPB, 'TM9')) AS ENTITY_ID,
       TO_NCHAR(pb.TENPB) AS ENTITY_NAME,
       pb.IDPB,
       TO_NCHAR(pb.TENPB) AS TEN_PHONGBAN,
       TO_NCHAR(e.MACTY) AS MACTY
FROM HR.V_AI_SRC_PHONGBAN pb
JOIN HR.V_AI_SRC_EMPLOYEE e ON e.IDPB = pb.IDPB
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_ORG_LOOKUP') = 1
  AND PKG_AI_READER.ROW_ALLOWED(e.MANV, e.IDPB, e.MACTY) = 1
UNION ALL
SELECT DISTINCT TO_NCHAR('COMPANY') AS ENTITY_TYPE,
       TO_NCHAR(e.MACTY) AS ENTITY_ID,
       TO_NCHAR('Công ty ') || TO_NCHAR(e.MACTY) AS ENTITY_NAME,
       0 AS IDPB,
       TO_NCHAR(NULL) AS TEN_PHONGBAN,
       TO_NCHAR(e.MACTY) AS MACTY
FROM HR.V_AI_SRC_EMPLOYEE e
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_ORG_LOOKUP') = 1
  AND PKG_AI_READER.ROW_ALLOWED(e.MANV, e.IDPB, e.MACTY) = 1;

-- 5. V_AI_PERIOD: Dong khi chua co ve
CREATE OR REPLACE VIEW V_AI_PERIOD AS
SELECT DISTINCT att.MAKYCONG, att.THANG, att.NAM, att.MACTY
FROM HR.V_AI_SRC_ATTENDANCE att
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_PERIOD') = 1
  AND PKG_AI_READER.ROW_ALLOWED(att.MANV, att.IDPB, att.MACTY) = 1;

-- 6. V_AI_OVERTIME
CREATE OR REPLACE VIEW V_AI_OVERTIME AS
SELECT tc.IDTCA, tc.MANV, tc.HOTEN, tc.NGAY, tc.THANG, tc.NAM, tc.SOGIO, tc.IDLOAICA, tc.IDPB, tc.TEN_PHONGBAN, tc.MACTY
FROM HR.V_AI_SRC_TANGCA tc
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_OVERTIME') = 1
  AND PKG_AI_READER.ROW_ALLOWED(tc.MANV, tc.IDPB, tc.MACTY) = 1
  AND tc.DELETED_DATE IS NULL;

-- 7. V_AI_OVERTIME_SUMMARY
CREATE OR REPLACE VIEW V_AI_OVERTIME_SUMMARY AS
SELECT tc.MANV, tc.HOTEN, tc.THANG, tc.NAM, tc.IDPB, tc.TEN_PHONGBAN, tc.MACTY, SUM(tc.SOGIO) AS TONG_SOGIO
FROM HR.V_AI_SRC_TANGCA tc
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_OVERTIME_SUMMARY') = 1
  AND PKG_AI_READER.ROW_ALLOWED(tc.MANV, tc.IDPB, tc.MACTY) = 1
  AND tc.DELETED_DATE IS NULL
GROUP BY tc.MANV, tc.HOTEN, tc.THANG, tc.NAM, tc.IDPB, tc.TEN_PHONGBAN, tc.MACTY;

-- 8. V_AI_ALLOWANCE
CREATE OR REPLACE VIEW V_AI_ALLOWANCE AS
SELECT pc.MANV, pc.HOTEN, pc.IDPC, pc.TENPC, pc.SOTIEN, pc.TU_NGAY, pc.DEN_NGAY, pc.IDPB, pc.TEN_PHONGBAN, pc.MACTY
FROM HR.V_AI_SRC_PHUCAP pc
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_ALLOWANCE') = 1
  AND PKG_AI_READER.ROW_ALLOWED(pc.MANV, pc.IDPB, pc.MACTY) = 1
  AND pc.DELETED_DATE IS NULL;

-- 9. V_AI_INSURANCE
CREATE OR REPLACE VIEW V_AI_INSURANCE AS
SELECT bh.IDBH, bh.MANV, bh.HOTEN, bh.SOBH, bh.NGAYCAP, bh.NOICAP, bh.NOIKHAMBENH, bh.IDPB, bh.TEN_PHONGBAN, bh.MACTY
FROM HR.V_AI_SRC_BAOHIEM bh
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_INSURANCE') = 1
  AND PKG_AI_READER.ROW_ALLOWED(bh.MANV, bh.IDPB, bh.MACTY) = 1
  AND bh.DELETED_DATE IS NULL;

-- 10. V_AI_ADVANCE
CREATE OR REPLACE VIEW V_AI_ADVANCE AS
SELECT ul.IDUL, ul.MANV, ul.HOTEN, ul.NGAY, ul.THANG, ul.NAM, ul.SOTIENUNG AS SOTIEN, ul.IDPB, ul.TEN_PHONGBAN, ul.MACTY
FROM HR.V_AI_SRC_UNGLUONG ul
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_ADVANCE') = 1
  AND PKG_AI_READER.ROW_ALLOWED(ul.MANV, ul.IDPB, ul.MACTY) = 1
  AND ul.DELETED_DATE IS NULL;

-- 11. V_AI_ATTENDANCE_SUMMARY
CREATE OR REPLACE VIEW V_AI_ATTENDANCE_SUMMARY AS
SELECT att.MAKYCONG, att.MANV, att.HOTEN, att.THANG, att.NAM, att.TONGNGAYCONG, att.NGAYPHEP, att.NGHIKHONGPHEP, att.CONGNGAYLE, att.CONGCHUNHAT, att.IDPB, att.TEN_PHONGBAN, att.MACTY
FROM HR.V_AI_SRC_ATTENDANCE att
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_ATTENDANCE_SUMMARY') = 1
  AND PKG_AI_READER.ROW_ALLOWED(att.MANV, att.IDPB, att.MACTY) = 1
  AND att.DELETED_DATE IS NULL AND att.TRANGTHAI = 1 AND att.CONG_PUBLISH_REV > 0;

-- 12. V_AI_CONTRACT
CREATE OR REPLACE VIEW V_AI_CONTRACT AS
SELECT hd.SOHD, hd.MANV, hd.HOTEN, hd.NGAYBATDAU, hd.NGAYKETTHUC, hd.NGAYKY, hd.LANKY, hd.HESOLUONG, hd.IDPB, hd.TEN_PHONGBAN, hd.MACTY
FROM HR.V_AI_SRC_HOPDONG hd
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_CONTRACT') = 1
  AND PKG_AI_READER.ROW_ALLOWED(hd.MANV, hd.IDPB, hd.MACTY) = 1
  AND hd.DEL_DATE IS NULL;

-- 13. V_AI_SALARY_CHANGE: Cung cap alias dong bo ca SOQD va SOQDNL, NGAYKY va NGAYKYNL
CREATE OR REPLACE VIEW V_AI_SALARY_CHANGE AS
SELECT nl.SOQDNL AS SOQD,
       nl.SOQDNL,
       nl.SOHD,
       nl.MANV,
       nl.HOTEN,
       nl.NGAYKYNL AS NGAYKY,
       nl.NGAYKYNL,
       nl.NGAYLENLUONG,
       nl.HESOLUONG_NOW AS HESOLUONGHIENTAI,
       nl.HESOLUONG_NOW,
       nl.HESOLUONG_NEW AS HESOLUONGMOI,
       nl.HESOLUONG_NEW,
       nl.IDPB,
       nl.TEN_PHONGBAN,
       nl.MACTY
FROM HR.V_AI_SRC_NANGLUONG nl
WHERE PKG_AI_READER.VIEW_ALLOWED('V_AI_SALARY_CHANGE') = 1
  AND PKG_AI_READER.ROW_ALLOWED(nl.MANV, nl.IDPB, nl.MACTY) = 1
  AND nl.DELETED_DATE IS NULL;

PROMPT [6/6] Cap quyen su dung cho AI_READONLY va cap BUMP_REVISION cho HR...
GRANT EXECUTE ON PKG_AI_AUTH TO AI_READONLY;
GRANT EXECUTE ON PKG_AI_READER TO AI_READONLY;
GRANT EXECUTE ON PKG_AI_READER TO HR;

GRANT SELECT ON V_AI_EMPLOYEE_LOOKUP TO AI_READONLY;
GRANT SELECT ON V_AI_EMPLOYEE TO AI_READONLY;
GRANT SELECT ON V_AI_EMPLOYEE_COUNT TO AI_READONLY;
GRANT SELECT ON V_AI_ORG_LOOKUP TO AI_READONLY;
GRANT SELECT ON V_AI_PERIOD TO AI_READONLY;
GRANT SELECT ON V_AI_OVERTIME TO AI_READONLY;
GRANT SELECT ON V_AI_OVERTIME_SUMMARY TO AI_READONLY;
GRANT SELECT ON V_AI_ALLOWANCE TO AI_READONLY;
GRANT SELECT ON V_AI_INSURANCE TO AI_READONLY;
GRANT SELECT ON V_AI_ADVANCE TO AI_READONLY;
GRANT SELECT ON V_AI_ATTENDANCE_SUMMARY TO AI_READONLY;
GRANT SELECT ON V_AI_CONTRACT TO AI_READONLY;
GRANT SELECT ON V_AI_SALARY_CHANGE TO AI_READONLY;

-- Cap nhat Policy Revision
UPDATE TB_AI_REVISION SET REVISION_NUMBER = REVISION_NUMBER + 1, UPDATED_AT = SYSDATE WHERE REVISION_KEY = 'POLICY_GLOBAL';
COMMIT;

PROMPT ============================================================================
PROMPT HOAN TAT V1_25 REPAIR CHO AI_OWNER (13 VIEWS CLOSED WHEN UNBOUND, PKGS UPGRADED).
PROMPT ============================================================================

EXIT;
