-- ============================================================================
-- SCRIPT: V1_26_ai_owner_index_sync_rollback.sql
-- Rollback cho migration V1_26 (Index sync & Qdrant foundation)
-- Nguoi thuc thi: sysdba
-- ============================================================================

WHENEVER SQLERROR CONTINUE;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT [Rollback 1/4] Chuyen session sang AI_OWNER...
ALTER SESSION SET CURRENT_SCHEMA = AI_OWNER;

PROMPT [Rollback 2/4] Thu hoi grant PKG_AI_INDEX tu AI_READONLY...
REVOKE EXECUTE ON AI_OWNER.PKG_AI_INDEX FROM AI_READONLY;

PROMPT [Rollback 3/4] Xoa Package PKG_AI_INDEX...
DROP PACKAGE BODY AI_OWNER.PKG_AI_INDEX;
DROP PACKAGE AI_OWNER.PKG_AI_INDEX;

PROMPT [Rollback 4/4] Xoa 4 bang metadata index...
DROP TABLE AI_OWNER.TB_AI_INDEX_CHECKPOINT CASCADE CONSTRAINTS;
DROP TABLE AI_OWNER.TB_AI_INDEX_OUTBOX CASCADE CONSTRAINTS;
DROP TABLE AI_OWNER.TB_AI_INDEX_REGISTRY CASCADE CONSTRAINTS;
DROP TABLE AI_OWNER.TB_AI_INDEX_SOURCE CASCADE CONSTRAINTS;

PROMPT Hoan tat Rollback V1_26.
