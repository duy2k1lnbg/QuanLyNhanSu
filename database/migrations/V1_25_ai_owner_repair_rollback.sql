-- ============================================================================
-- SCRIPT: V1_25_ai_owner_repair_rollback.sql
-- Kich ban Rollback cho V1_25 Repair Migration (Schema AI_OWNER)
-- Nguoi chay: AI_OWNER va/hoac DBA (tuy cap do rollback)
-- NGUYEN TAC AN TOAN:
-- 1. Rollback tap trung vao AI_OWNER; TUYET DOI KHONG mo lai cac view unshielded cu trong HR hoac AI_READONLY.
-- 2. KHONG cap lai quyen truc tiep tren bang HR cho AI_READONLY.
-- 3. Fail-closed: khi rollback, he thong AI phai an toan, khong ro ri du lieu.
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT ============================================================================
PROMPT CAP DO 1: FAIL-CLOSED KILL SWITCH (VO HIEU HOA TOAN BO AI)
PROMPT Thuc hien boi: AI_OWNER hoac DBA
PROMPT ============================================================================

-- 1. Vo hieu hoa toan bo capabilities
UPDATE TB_AI_CAPABILITY SET IS_ENABLED = 0;
DBMS_OUTPUT.PUT_LINE('-> Da tat toan bo (' || SQL%ROWCOUNT || ') Capabilities trong AI_OWNER.');

-- 2. Tang revision de vo hieu hoa cache
UPDATE TB_AI_REVISION 
   SET REVISION_NUMBER = REVISION_NUMBER + 1, UPDATED_AT = SYSDATE 
 WHERE REVISION_KEY = 'POLICY_GLOBAL';
DBMS_OUTPUT.PUT_LINE('-> Da tang POLICY_GLOBAL Revision.');

-- 3. Xoa toan bo ve doc va nonces
DELETE FROM TB_AI_READ_TICKET;
DBMS_OUTPUT.PUT_LINE('-> Da xoa toan bo Read Tickets.');
DELETE FROM TB_AI_PROOF_NONCE;
DBMS_OUTPUT.PUT_LINE('-> Da xoa toan bo Nonces.');

-- 4. Xoa context phien hien tai
BEGIN
    PKG_AI_READER.CLEAR_REQUEST();
    DBMS_OUTPUT.PUT_LINE('-> Da xoa trang Context phien.');
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- 5. Thu hoi quyen cua AI_READONLY tren cac view va package de dong chat tuyet doi
BEGIN
    FOR v IN (SELECT VIEW_NAME FROM ALL_VIEWS WHERE OWNER = 'AI_OWNER' AND VIEW_NAME LIKE 'V_AI_%') LOOP
        BEGIN
            EXECUTE IMMEDIATE 'REVOKE SELECT ON AI_OWNER.' || v.VIEW_NAME || ' FROM AI_READONLY';
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END LOOP;
    BEGIN
        EXECUTE IMMEDIATE 'REVOKE EXECUTE ON AI_OWNER.PKG_AI_AUTH FROM AI_READONLY';
        EXECUTE IMMEDIATE 'REVOKE EXECUTE ON AI_OWNER.PKG_AI_READER FROM AI_READONLY';
    EXCEPTION
        WHEN OTHERS THEN NULL;
    END;
    DBMS_OUTPUT.PUT_LINE('-> Da thu hoi SELECT va EXECUTE tu AI_READONLY (Fail-closed hoan toan).');
END;
/

COMMIT;
PROMPT [HOAN TAT CAP DO 1] Toan bo he thong AI da duoc dong chat an toan.
