-- ============================================================================
-- SCRIPT: V1_34__add_policy_lookup_capability.sql
-- Muc dich: Bo sung Capability POLICY_LOOKUP cho Qdrant DataDoc RAG Collection
--           (hrms_knowledge_active) va cap Scope Grant cho quan tri vien ADMIN.
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT [1/3] Kiem tra va cap nhat TB_AI_CAPABILITY cho POLICY_LOOKUP...
MERGE INTO AI_OWNER.TB_AI_CAPABILITY t USING (
    SELECT 'POLICY_LOOKUP' AS code FROM DUAL
) s ON (t.CAPABILITY_CODE = s.code)
WHEN MATCHED THEN
    UPDATE SET t.DOMAIN = 'POLICY',
               t.OPERATION = 'LOOKUP',
               t.REQUIRED_FUNCTION_CODE = NULL,
               t.SOURCE_VIEW = 'hrms_knowledge_active',
               t.FIELD_PROFILE = 'DEFAULT',
               t.IS_ENABLED = 1,
               t.UPDATED_AT = SYSDATE
WHEN NOT MATCHED THEN
    INSERT (CAPABILITY_CODE, DOMAIN, OPERATION, REQUIRED_FUNCTION_CODE, SOURCE_VIEW, FIELD_PROFILE, IS_ENABLED)
    VALUES ('POLICY_LOOKUP', 'POLICY', 'LOOKUP', NULL, 'hrms_knowledge_active', 'DEFAULT', 1);

PROMPT [2/3] Cap Scope Grant ALL cho tai khoan ADMIN...
DECLARE
    v_admin_id NUMBER;
    v_exists   NUMBER;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    SELECT COUNT(*) INTO v_exists
    FROM AI_OWNER.TB_AI_SCOPE_GRANT
    WHERE SUBJECT_ID = v_admin_id AND SUBJECT_TYPE = 'USER' AND CAPABILITY_CODE = 'POLICY_LOOKUP';

    IF v_exists = 0 THEN
        INSERT INTO AI_OWNER.TB_AI_SCOPE_GRANT (
            SUBJECT_TYPE, SUBJECT_ID, CAPABILITY_CODE, SCOPE_TYPE, EFFECT, IS_ENABLED, CREATED_AT
        ) VALUES (
            'USER', v_admin_id, 'POLICY_LOOKUP', 'ALL', 'ALLOW', 1, SYSDATE
        );
        DBMS_OUTPUT.PUT_LINE('-> Da cap Scope Grant POLICY_LOOKUP cho ADMIN ID=' || v_admin_id);
    ELSE
        DBMS_OUTPUT.PUT_LINE('-> Scope Grant POLICY_LOOKUP cho ADMIN da ton tai.');
    END IF;
END;
/

PROMPT [3/3] Bump Revision chinh sach POLICY_GLOBAL...
UPDATE AI_OWNER.TB_AI_REVISION
   SET REVISION_NUMBER = REVISION_NUMBER + 1,
       UPDATED_AT = CURRENT_TIMESTAMP
 WHERE REVISION_KEY = 'POLICY_GLOBAL';

COMMIT;
PROMPT Hoan tat Migration V1_34.
