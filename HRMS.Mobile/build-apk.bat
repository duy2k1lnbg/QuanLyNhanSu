@echo off
chcp 65001 >nul
echo ==============================================================================
echo        SCRIPT BUILD APK CHO HRMS MOBILE (Ket noi VPS: 103.200.22.79)
echo ==============================================================================
echo.
cd /d "%~dp0android"

echo [1/2] Dang tien hanh dong goi APK (assembleDebug)...
call gradlew.bat assembleDebug

if errorlevel 1 (
    echo.
    echo [LOI] Bien dich APK that bai! Vui long kiem tra thong bao loi o tren.
    pause
    exit /b 1
)

echo.
echo [2/2] Dang sao chep file APK...
if not exist "%~dp0..\..\deploy_vps\mobile" mkdir "%~dp0..\..\deploy_vps\mobile"
copy /y "%~dp0android\app\build\outputs\apk\debug\app-debug.apk" "%~dp0HRMS_Mobile.apk" >nul
copy /y "%~dp0android\app\build\outputs\apk\debug\app-debug.apk" "%~dp0..\..\deploy_vps\mobile\HRMS_Mobile.apk" >nul

echo.
echo ==============================================================================
echo   [HOAN TAT] File APK da duoc tao va copy thanh cong tai:
echo   1. %~dp0HRMS_Mobile.apk
echo   2. D:\QL_NS\deploy_vps\mobile\HRMS_Mobile.apk
echo ==============================================================================
pause
