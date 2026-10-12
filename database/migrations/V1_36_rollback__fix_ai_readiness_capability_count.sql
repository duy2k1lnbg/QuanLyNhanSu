-- ============================================================================
-- SCRIPT: V1_36_rollback__fix_ai_readiness_capability_count.sql
-- Muc dich: Khoi phuc lai logic IS_READY() ban dau (v_caps_count <> 11)
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT Khoi phuc PACKAGE BODY AI_OWNER.PKG_AI_READER ve ban V1_25...

CREATE OR REPLACE PACKAGE BODY AI_OWNER.PKG_AI_READER AS
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

        SELECT REVISION_NUMBER INTO v_cur_pol_rev FROM TB_AI_REVISION WHERE REVISION_KEY = 'POLICY_GLOBAL';

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

        v_exp_str := SYS_CONTEXT('HRMS_AI_CTX', 'TICKET_EXP');
        IF v_exp_str IS NOT NULL THEN
            IF SYSDATE > TO_DATE(v_exp_str, 'YYYYMMDDHH24MISS') THEN
                RETURN 0;
            END IF;
        END IF;

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

        IF v_self_only = 1 OR v_cap IN ('INSURANCE_SELF', 'PAYROLL_SELF') THEN
            IF p_manv IS NOT NULL AND v_actor_manv IS NOT NULL AND p_manv = v_actor_manv THEN
                RETURN 1;
            ELSE
                RETURN 0;
            END IF;
        END IF;

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
        SELECT COUNT(*) INTO v_views_count
          FROM ALL_VIEWS
         WHERE OWNER = 'AI_OWNER' AND VIEW_NAME LIKE 'V_AI_%';

        IF v_views_count < 13 THEN RETURN 0; END IF;

        SELECT COUNT(*) INTO v_pkgs_count
          FROM ALL_OBJECTS
         WHERE OWNER = 'AI_OWNER'
           AND OBJECT_TYPE = 'PACKAGE BODY'
           AND OBJECT_NAME IN ('PKG_AI_AUTH', 'PKG_AI_READER')
           AND STATUS = 'VALID';

        IF v_pkgs_count < 2 THEN RETURN 0; END IF;

        SELECT COUNT(*) INTO v_caps_count
          FROM TB_AI_CAPABILITY
         WHERE IS_ENABLED = 1;

        IF v_caps_count <> 11 THEN RETURN 0; END IF;

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
