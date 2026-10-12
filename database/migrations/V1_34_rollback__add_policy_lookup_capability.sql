-- ============================================================================
-- SCRIPT: V1_34_rollback__add_policy_lookup_capability.sql
-- Muc dich: Rollback Capability POLICY_LOOKUP va Scope Grant cua ADMIN.
-- ============================================================================

DELETE FROM AI_OWNER.TB_AI_SCOPE_GRANT WHERE CAPABILITY_CODE = 'POLICY_LOOKUP';
DELETE FROM AI_OWNER.TB_AI_CAPABILITY WHERE CAPABILITY_CODE = 'POLICY_LOOKUP';

UPDATE AI_OWNER.TB_AI_REVISION
   SET REVISION_NUMBER = REVISION_NUMBER + 1,
       UPDATED_AT = CURRENT_TIMESTAMP
 WHERE REVISION_KEY = 'POLICY_GLOBAL';

COMMIT;
PROMPT Hoan tat Rollback V1_34.
