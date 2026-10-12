-- ==============================================================================
-- Rollback Migration V1_46: Revert Standardize Approval & Disbursement Functions
-- File: database/migrations/V1_46_rollback__standardize_approval_and_disbursement_functions.sql
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

PROMPT [ROLLBACK V1_46] Reverting V1_46 changes...

-- Xóa bản ghi trong SCHEMA_VERSION
DELETE FROM SCHEMA_VERSION WHERE VERSION = '1.46';

COMMIT;
PROMPT [ROLLBACK V1_46] Completed successfully.
