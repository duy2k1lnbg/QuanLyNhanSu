-- ============================================================================
-- SCRIPT 4: V1_24_ai_owner_rollback.sql
-- Kich ban Rollback an toan cho Kien truc AI V2 (3-Schema Architecture)
-- Nguoi chay: AI_OWNER va/hoac DBA (tuy cap do rollback)
-- NGUYEN TAC AN TOAN BAT BUOC:
-- 1. Rollback tap trung vao schema AI_OWNER; KHONG khoi phuc cac view reader cu chua duoc bao ve trong HR.
-- 2. KHONG cap lai bat ky quyen truc tiep nao tren cac bang goc HR cho AI_READONLY.
-- 3. KHONG quay ve trang thai "122 test" thieu an ninh.
-- 4. Khi kiem tra bao mat chua dat hoac gap su co, he thong AI phai duoc GIU O TRANG THAI TAT (DISABLED).
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT ============================================================================
PROMPT CAP DO 1: ROLLBACK MEM (EMERGENCY KILL-SWITCH / VO HIEU HOA TOAN BO AI)
PROMPT Thuc hien boi: AI_OWNER hoac DBA
PROMPT Muc dich: Vo hieu hoa lap tuc tinh nang AI ma khong mat du lieu cau hinh
PROMPT ============================================================================

-- 1. Tat toan bo Capabilities trong AI_OWNER
UPDATE AI_OWNER.TB_AI_CAPABILITY 
SET IS_ENABLED = 0;
DBMS_OUTPUT.PUT_LINE('-> Da tat toan bo (' || SQL%ROWCOUNT || ') Capabilities trong AI_OWNER.');

-- 2. Tang Policy Revision de vo hieu hoa moi cache phia C# Backend
UPDATE AI_OWNER.TB_AI_REVISION 
SET REVISION_NUMBER = REVISION_NUMBER + 1, UPDATED_AT = SYSDATE 
WHERE REVISION_KEY = 'POLICY_GLOBAL';
DBMS_OUTPUT.PUT_LINE('-> Da tang POLICY_GLOBAL Revision de lam mat hieu luc cache backend.');

-- 3. Xoa toan bo Ve doc (Ticket) con ton tai
DELETE FROM AI_OWNER.TB_AI_READ_TICKET;
DBMS_OUTPUT.PUT_LINE('-> Da xoa toan bo (' || SQL%ROWCOUNT || ') ve doc (Read Tickets).');

-- 4. Xoa toan bo Nonces
DELETE FROM AI_OWNER.TB_AI_PROOF_NONCE;
DBMS_OUTPUT.PUT_LINE('-> Da xoa toan bo (' || SQL%ROWCOUNT || ') nonces.');

-- 5. Xoa Context phien hien tai
BEGIN
    AI_OWNER.PKG_AI_READER.CLEAR_REQUEST();
    DBMS_OUTPUT.PUT_LINE('-> Da xoa trang Context phien.');
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

COMMIT;
PROMPT [HOAN TAT CAP DO 1] Toan bo yeu cau AI se bi tu choi tu dong (Fail-Closed). AI_READONLY khong the doc duoc bat ky du lieu nao.

PROMPT ============================================================================
PROMPT CAP DO 2: ROLLBACK CUNG (TEARDOWN TOAN BO DOI TUONG AI_OWNER & QUYEN AI_READONLY)
PROMPT Chi thuc hien khi can go bo hoan toan Module AI V2
PROMPT Nguoi chay: DBA (SYS hoac user co quyen DBA)
PROMPT ============================================================================

-- Doan code duoi day duoc de mac dinh COMMENT de tranh vo tinh chay go bo hoan toan.
-- De thuc hien Teardown triet de, bo comment va chay duoi quyen DBA.

/*
PROMPT [DBA TEARDOWN] Thu hoi quyen cua AI_READONLY tren AI_OWNER...
BEGIN
    FOR p IN (SELECT OBJECT_NAME FROM DBA_OBJECTS WHERE OWNER = 'AI_OWNER' AND OBJECT_TYPE IN ('VIEW', 'PACKAGE')) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'REVOKE ALL ON AI_OWNER.' || p.OBJECT_NAME || ' FROM AI_READONLY';
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('-> Da thu hoi toan bo quyen cua AI_READONLY tren AI_OWNER.');
END;
/

PROMPT [DBA TEARDOWN] Xoa Trusted Context HRMS_AI_CTX...
BEGIN
    EXECUTE IMMEDIATE 'DROP CONTEXT HRMS_AI_CTX';
    DBMS_OUTPUT.PUT_LINE('-> Da drop Context HRMS_AI_CTX.');
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

PROMPT [DBA TEARDOWN] Xoa cac Views va Packages trong schema AI_OWNER...
BEGIN
    FOR v IN (SELECT VIEW_NAME FROM DBA_VIEWS WHERE OWNER = 'AI_OWNER') LOOP
        EXECUTE IMMEDIATE 'DROP VIEW AI_OWNER.' || v.VIEW_NAME;
    END LOOP;
    
    FOR p IN (SELECT OBJECT_NAME FROM DBA_OBJECTS WHERE OWNER = 'AI_OWNER' AND OBJECT_TYPE = 'PACKAGE') LOOP
        EXECUTE IMMEDIATE 'DROP PACKAGE AI_OWNER.' || p.OBJECT_NAME;
    END LOOP;
    
    FOR t IN (SELECT TABLE_NAME FROM DBA_TABLES WHERE OWNER = 'AI_OWNER') LOOP
        EXECUTE IMMEDIATE 'DROP TABLE AI_OWNER.' || t.TABLE_NAME || ' CASCADE CONSTRAINTS';
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('-> Da xoa sach doi tuong trong schema AI_OWNER.');
END;
/

PROMPT [DBA TEARDOWN] Thu hoi WITH GRANT OPTION tu HR tren cac V_AI_SRC_*...
BEGIN
    FOR v IN (SELECT TABLE_NAME FROM DBA_VIEWS WHERE OWNER = 'HR' AND VIEW_NAME LIKE 'V_AI_SRC_%') LOOP
        BEGIN
            EXECUTE IMMEDIATE 'REVOKE SELECT ON HR.' || v.TABLE_NAME || ' FROM AI_OWNER';
        EXCEPTION
            WHEN OTHERS THEN NULL;
        END;
    END LOOP;
    DBMS_OUTPUT.PUT_LINE('-> Da thu hoi quyen tren cac view nguon HR.V_AI_SRC_*.');
END;
/
*/

PROMPT ============================================================================
PROMPT CANH BAO QUAN TRONG:
PROMPT - KHONG CAP LAI BAT KY QUYEN NAO TREN CAC BANG GOC HR.TB_* CHO AI_READONLY.
PROMPT - KHONG TAO LAI CAC VIEW READER KHONG BAO VE TRONG HR SCHEMA.
PROMPT - AI SE DUOC GIU TAT DUNG NGUYEN TAC AN TOAN NGHIEP VU.
PROMPT ============================================================================
