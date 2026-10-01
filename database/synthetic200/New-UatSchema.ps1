$ErrorActionPreference='Stop'
$taskRoot='D:\QL_NS\QuanLyNhanSu'
[void](New-Item -ItemType Directory -Force "$taskRoot\artifacts\synthetic200")
$taskSecretPath="$taskRoot\artifacts\synthetic200\oracle-password.dpapi"
if(Test-Path -LiteralPath $taskSecretPath){$taskSavedSecret=Get-Content -LiteralPath $taskSecretPath | ConvertTo-SecureString}
$taskPassword=if($taskSavedSecret){([System.Net.NetworkCredential]::new('',$taskSavedSecret)).Password.Substring(0,27)}else{'Uat'+[Guid]::NewGuid().ToString('N').Substring(0,24)}
$taskSql=@'
WHENEVER SQLERROR EXIT SQL.SQLCODE
SET SERVEROUTPUT ON ECHO OFF
DECLARE
 n NUMBER;
 h NUMBER;
 state VARCHAR2(30);
BEGIN
 SELECT COUNT(*) INTO n FROM DBA_USERS WHERE USERNAME='HR_UAT_200';
 IF n=0 THEN
 EXECUTE IMMEDIATE 'CREATE USER HR_UAT_200 IDENTIFIED BY "__PASSWORD__" DEFAULT TABLESPACE USERS QUOTA 1G ON USERS';
 EXECUTE IMMEDIATE 'GRANT CREATE SESSION, CREATE TABLE, CREATE VIEW, CREATE SEQUENCE, CREATE PROCEDURE, CREATE TRIGGER TO HR_UAT_200';
 END IF;
 SELECT COUNT(*) INTO n FROM DBA_TABLES WHERE OWNER='HR_UAT_200';
 IF n<>0 THEN RAISE_APPLICATION_ERROR(-20002,'UAT schema is not empty'); END IF;
 h := DBMS_DATAPUMP.OPEN('IMPORT','SCHEMA',job_name=>'HR_SYN200_META_'||TO_CHAR(SYSDATE,'HH24MISS'));
 DBMS_DATAPUMP.ADD_FILE(h,'HR_before_synthetic200_20260928.dmp','DATA_PUMP_DIR');
 DBMS_DATAPUMP.ADD_FILE(h,'HR_synthetic200_metadata_20260928.log','DATA_PUMP_DIR',filetype=>DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE);
 DBMS_DATAPUMP.METADATA_FILTER(h,'SCHEMA_EXPR','= ''HR''');
 DBMS_DATAPUMP.METADATA_REMAP(h,'REMAP_SCHEMA','HR','HR_UAT_200');
 DBMS_DATAPUMP.DATA_FILTER(h,'INCLUDE_ROWS',0);
 DBMS_DATAPUMP.METADATA_FILTER(h,'EXCLUDE_PATH_EXPR','IN (''USER'',''GRANT'',''PROCEDURE'',''TRIGGER'',''STATISTICS'')');
 DBMS_DATAPUMP.START_JOB(h);
 DBMS_DATAPUMP.WAIT_FOR_JOB(h,state);
 DBMS_OUTPUT.PUT_LINE('IMPORT_STATE='||state);
 IF state<>'COMPLETED' THEN RAISE_APPLICATION_ERROR(-20003,'Import incomplete'); END IF;
END;
/
EXIT
'@
$taskSql=$taskSql.Replace('__PASSWORD__',$taskPassword)
$taskPassword | ConvertTo-SecureString -AsPlainText -Force | ConvertFrom-SecureString | Set-Content -LiteralPath $taskSecretPath
$taskSql | & D:/Oracle/bin/sqlplus.exe -L -S '/ as sysdba'
if($LASTEXITCODE -ne 0){throw 'UAT schema setup failed. Inspect state before retrying.'}
