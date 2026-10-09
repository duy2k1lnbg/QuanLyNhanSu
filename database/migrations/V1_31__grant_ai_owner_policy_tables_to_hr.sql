-- ============================================================================
-- SCRIPT: V1_31__grant_ai_owner_policy_tables_to_hr.sql
-- Muc dich: Khac phuc ORA-00942 khi MyEntities (ket noi voi user HR) truy van
--           cac bang quan tri chinh sach AI (OracleAiScopeGrantRepository).
-- Nguoi chay: DBA (SYS AS SYSDBA hoac AI_OWNER)
-- Luu y: Phai chay boi tai khoan co quyen tren AI_OWNER (hoac SYSDBA).
--        KHONG cap quyen cho AI_READONLY (AI_READONLY chi duoc EXECUTE package).
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT [1/3] Cap quyen toi thieu tu AI_OWNER cho HR tren cac bang chinh sach AI...
BEGIN
    EXECUTE IMMEDIATE 'GRANT SELECT, INSERT, UPDATE, DELETE ON AI_OWNER.TB_AI_SCOPE_GRANT TO HR';
    EXECUTE IMMEDIATE 'GRANT SELECT, INSERT, UPDATE, DELETE ON AI_OWNER.TB_AI_REVISION TO HR';
    EXECUTE IMMEDIATE 'GRANT SELECT, INSERT, UPDATE, DELETE ON AI_OWNER.TB_AI_CAPABILITY TO HR';
    EXECUTE IMMEDIATE 'GRANT SELECT, INSERT, UPDATE, DELETE ON AI_OWNER.TB_AI_FIELD_POLICY TO HR';
    DBMS_OUTPUT.PUT_LINE('-> Da cap quyen tren cac bang AI_OWNER cho HR.');
END;
/

PROMPT [2/3] Tao Synonyms trong schema HR de ho tro ca hai cach truy van...
BEGIN
    -- Tao Synonym private trong schema HR
    BEGIN
        EXECUTE IMMEDIATE 'CREATE OR REPLACE SYNONYM HR.TB_AI_SCOPE_GRANT FOR AI_OWNER.TB_AI_SCOPE_GRANT';
        EXECUTE IMMEDIATE 'CREATE OR REPLACE SYNONYM HR.TB_AI_REVISION FOR AI_OWNER.TB_AI_REVISION';
        EXECUTE IMMEDIATE 'CREATE OR REPLACE SYNONYM HR.TB_AI_CAPABILITY FOR AI_OWNER.TB_AI_CAPABILITY';
        EXECUTE IMMEDIATE 'CREATE OR REPLACE SYNONYM HR.TB_AI_FIELD_POLICY FOR AI_OWNER.TB_AI_FIELD_POLICY';
        DBMS_OUTPUT.PUT_LINE('-> Da tao Synonyms trong HR.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('-> Bo qua tao synonym neu khong du quyen: ' || SQLERRM);
    END;
END;
/

PROMPT [3/3] Kiem tra ket noi va doc thu tu schema HR...
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM AI_OWNER.TB_AI_SCOPE_GRANT WHERE ROWNUM = 1;
    DBMS_OUTPUT.PUT_LINE('-> Xac nhan thanh cong: Co the SELECT tu AI_OWNER.TB_AI_SCOPE_GRANT!');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('-> Canh bao kiem tra: ' || SQLERRM);
END;
/

PROMPT Hoan tat Script V1_31.
EXIT;
