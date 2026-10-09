-- Safe rollback: disable AI and invalidate tickets; preserve policies and source data.
-- DDL auto-commits in Oracle. A successful application build is not proof of migration success.
WHENEVER SQLERROR EXIT FAILURE ROLLBACK
UPDATE HR.TB_AI_CAPABILITY SET IS_ENABLED=0,UPDATED_AT=SYSDATE;
UPDATE HR.TB_AI_REVISION SET REVISION_NUMBER=REVISION_NUMBER+1,UPDATED_AT=SYSDATE WHERE REVISION_KEY='POLICY_GLOBAL';
DELETE FROM HR.TB_AI_READ_TICKET;
COMMIT;
-- Keep context-protected views and minimal grants while restoring the reviewed app version.
-- Never re-grant raw tables or restore unprotected V1_2 views to regain functionality.
-- Restore individual definitions from preflight DDL exports only after a separate DBA review.