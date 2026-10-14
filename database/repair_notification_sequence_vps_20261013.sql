-- Forward-only repair for the confirmed TB_THONGBAO.ID collision on VPS.
-- Run after reviewing approval_request_preflight_vps_20261013.sql.
-- Expected target: XEPDB1; HR-owned sequence only. No request data is updated/deleted.
-- Oracle 19c+; DDL commits and cannot be undone by ROLLBACK.
SET SERVEROUTPUT ON SIZE UNLIMITED;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
DECLARE
 v_container VARCHAR2(128); v_max NUMBER; v_next NUMBER;
 v_increment NUMBER; v_cache NUMBER; v_cycle VARCHAR2(1);
 v_key_count NUMBER;
BEGIN
 v_container := SYS_CONTEXT('USERENV','CON_NAME');
 IF v_container <> 'XEPDB1' THEN
   RAISE_APPLICATION_ERROR(-20082,'Wrong container; this repair is scoped to VPS XEPDB1.');
 END IF;
 SELECT COUNT(*) INTO v_key_count FROM ALL_CONSTRAINTS c JOIN ALL_CONS_COLUMNS l
  ON c.OWNER=l.OWNER AND c.CONSTRAINT_NAME=l.CONSTRAINT_NAME
  WHERE c.OWNER='HR' AND c.TABLE_NAME='TB_THONGBAO' AND c.CONSTRAINT_TYPE='P' AND l.COLUMN_NAME='ID';
 IF v_key_count<>1 THEN RAISE_APPLICATION_ERROR(-20083,'Unexpected notification primary key contract.'); END IF;
 SELECT NVL(MAX(ID),0) INTO v_max FROM HR.TB_THONGBAO;
 SELECT LAST_NUMBER,INCREMENT_BY,CACHE_SIZE,CYCLE_FLAG INTO v_next,v_increment,v_cache,v_cycle
   FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER='HR' AND SEQUENCE_NAME='SEQ_THONGBAO';
 IF v_increment<>1 OR v_cache<>0 OR v_cycle<>'N' THEN
   RAISE_APPLICATION_ERROR(-20084,'Sequence configuration needs separate review; no DDL applied.');
 END IF;
 DBMS_OUTPUT.PUT_LINE('BEFORE container='||v_container||'; max_id='||v_max||'; next_id='||v_next);
 IF v_next<=v_max THEN
   EXECUTE IMMEDIATE 'ALTER SEQUENCE HR.SEQ_THONGBAO RESTART START WITH '||
       TO_CHAR(v_max+1,'FM99999999999999999999999999999999999999');
   DBMS_OUTPUT.PUT_LINE('Advanced notification sequence to '||(v_max+1));
 ELSE DBMS_OUTPUT.PUT_LINE('No drift; no DDL applied.');
 END IF;
END;
/
SELECT SEQUENCE_NAME,LAST_NUMBER,INCREMENT_BY,CACHE_SIZE,CYCLE_FLAG
 FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER='HR' AND SEQUENCE_NAME='SEQ_THONGBAO';

