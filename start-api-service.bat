@echo off
echo ===================================================
echo [HRMS Mobile] Starting Backend Services...
echo ===================================================

REM 1. Start IIS Express on port 5001
start "IIS Express Backend (Port 5001)" "C:\Program Files\IIS Express\iisexpress.exe" /path:%~dp0HRMS_API /port:5001

REM 2. Wait 2 seconds
timeout /t 2 >nul

REM 3. Start Node.js Reverse Proxy on port 5000
start "HRMS Reverse Proxy (Port 5000)" node "%~dp0proxy.js"

echo ===================================================
echo Services started!
echo - IIS Express: http://localhost:5001/
echo - LAN Proxy:   http://0.0.0.0:5000/
echo Mobile app can now connect directly over Wi-Fi.
echo ===================================================
