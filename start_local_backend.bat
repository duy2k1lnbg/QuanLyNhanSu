@echo off
title HRMS_API Local Server (Port 55463)
echo ==========================================================
echo   DANG KHOI DONG BACKEND HRMS_API TREN PORT 55463...
echo   Url: http://localhost:55463/api/auth/login
echo ==========================================================
echo.
"C:\Program Files\IIS Express\iisexpress.exe" /path:"%~dp0HRMS.Api" /port:55463
pause
