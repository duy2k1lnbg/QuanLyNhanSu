@echo off
chcp 65001 >nul
title Khoi phuc Database Oracle HRMS (.DMP)
echo ==============================================================================
echo        SCRIPT IMPORT ORACLE DATABASE (.DMP) CHO HRMS TREN VPS
echo        Ho tro Oracle 21c XE (Pluggable DB: XEPDB1) / 19c / 11g-12c
echo ==============================================================================
echo.

set DB_USER=HR
set DB_PASS=hr
set DB_SVC=localhost:1521/XEPDB1
set SYS_PASS=Duy123

echo [LUU Y QUAN TRONG CHO ORACLE 21c XE]:
echo   - KHONG nhap "xe" vi "xe" la Container Root (CDB$ROOT) - se loi ORA-65096!
echo   - BAT BUOC dung Pluggable Database (PDB): localhost:1521/XEPDB1
echo.

set /p INPUT_SVC="1. Nhap Host/Service [Mac dinh: %DB_SVC%]: "
if not "%INPUT_SVC%"=="" set DB_SVC=%INPUT_SVC%

set /p INPUT_USER="2. Nhap Database User can import [Mac dinh: %DB_USER%]: "
if not "%INPUT_USER%"=="" set DB_USER=%INPUT_USER%

set /p INPUT_PASS="3. Nhap Password cua user %DB_USER% [Mac dinh: %DB_PASS%]: "
if not "%INPUT_PASS%"=="" set DB_PASS=%INPUT_PASS%

echo.
set /p INPUT_SYS="4. Nhap Password cua SYS [Mac dinh: %SYS_PASS%]: "
if not "%INPUT_SYS%"=="" set SYS_PASS=%INPUT_SYS%

echo.
echo ==============================================================================
echo   Thong tin ket noi: %DB_USER% / **** @ %DB_SVC%
echo ==============================================================================
echo.

:: Xac dinh vi tri thu muc chua file DMP
set DUMP_DIR=%~dp0
if exist "%~dp0HR_BACKUP.DMP" set DUMP_DIR=%~dp0
if exist "%~dp0..\HR_BACKUP.DMP" set DUMP_DIR=%~dp0..
for %%i in ("%DUMP_DIR%") do set DUMP_DIR=%%~fi

echo Thu muc chua file DMP: %DUMP_DIR%
echo.

if not "%SYS_PASS%"=="" (
    echo [1/3] Don sach schema %DB_USER% va thiet lap directory bang SYSDBA...
    (
        echo CREATE OR REPLACE DIRECTORY HRMS_RESTORE_DIR AS '%DUMP_DIR%';
        echo GRANT READ, WRITE ON DIRECTORY HRMS_RESTORE_DIR TO PUBLIC;
        echo BEGIN
        echo   EXECUTE IMMEDIATE 'DROP USER %DB_USER% CASCADE';
        echo EXCEPTION WHEN OTHERS THEN NULL;
        echo END;
        echo /
        echo BEGIN
        echo   EXECUTE IMMEDIATE 'DROP USER AI_OWNER CASCADE';
        echo EXCEPTION WHEN OTHERS THEN NULL;
        echo END;
        echo /
        echo CREATE USER %DB_USER% IDENTIFIED BY %DB_PASS% DEFAULT TABLESPACE USERS TEMPORARY TABLESPACE TEMP QUOTA UNLIMITED ON USERS;
        echo GRANT CONNECT, RESOURCE, DBA TO %DB_USER%;
        echo GRANT UNLIMITED TABLESPACE TO %DB_USER%;
        echo GRANT READ, WRITE ON DIRECTORY HRMS_RESTORE_DIR TO %DB_USER%;
        echo EXIT;
    ) > "%temp%\prep_import.sql"

    sqlplus "sys/%SYS_PASS%@%DB_SVC% as sysdba" @"%temp%\prep_import.sql"
    echo.
    echo [2/2] Dang chay impdp cho ca HR va AI_OWNER bang SYSDBA tu HR_BACKUP.DMP...
    impdp 'sys/%SYS_PASS%@%DB_SVC% as sysdba' schemas=%DB_USER%,AI_OWNER directory=HRMS_RESTORE_DIR dumpfile=HR_BACKUP.DMP logfile=HR_RESTORE_IMPDP.log table_exists_action=replace

    if exist "%DUMP_DIR%\AI_OWNER_BACKUP.DMP" (
        echo.
        echo [Kiem tra] AI_OWNER_BACKUP.DMP du phong co san trong thu muc.
    )
) else (
    echo [1/2] Tao directory bang tai khoan %DB_USER%...
    (
        echo CREATE OR REPLACE DIRECTORY HRMS_RESTORE_DIR AS '%DUMP_DIR%';
        echo GRANT READ, WRITE ON DIRECTORY HRMS_RESTORE_DIR TO %DB_USER%;
        echo EXIT;
    ) > "%temp%\prep_import.sql"
    sqlplus "%DB_USER%/%DB_PASS%@%DB_SVC%" @"%temp%\prep_import.sql"
    
    echo.
    echo [2/2] Dang chay impdp voi ACCESS_METHOD=CONVENTIONAL...
    impdp "%DB_USER%/%DB_PASS%@%DB_SVC%" schemas=%DB_USER% directory=HRMS_RESTORE_DIR dumpfile=HR_BACKUP.DMP logfile=HR_RESTORE_IMPDP.log table_exists_action=replace access_method=conventional
)

echo.
echo ==============================================================================
echo   HOAN TAT! Vui long kiem tra ket qua import tai: %DUMP_DIR%\HR_RESTORE_IMPDP.log
echo ==============================================================================
pause
