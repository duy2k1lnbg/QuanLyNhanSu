# ==============================================================================
# SCRIPT BUILD TỰ ĐỘNG THẲNG VÀO D:\QL_NS\deploy_vps
# - Build Frontend (React Vite) & Backend (ASP.NET Web API 2)
# - BẢO TOÀN TUYỆT ĐỐI ẢNH CỦA BẠN: myavt.png
# - Tự động nén sẵn file ZIP D:\QL_NS\deploy_vps.zip để chỉ việc copy lên VPS
# ==============================================================================

param(
    [switch]$SkipZip = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  BAT DAU BUILD TOAN BO VA DONG GOI VAO D:\QL_NS\deploy_vps" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$root = "D:\QL_NS\QuanLyNhanSu"
$deployRoot = "D:\QL_NS\deploy_vps"
$deployFrontend = Join-Path $deployRoot "frontend"
$deployBackend = Join-Path $deployRoot "backend"
$myavtBackup = "D:\QL_NS\myavt_backup.png"

# ------------------------------------------------------------------------------
# 1. BẢO TOÀN TUYỆT ĐỐI ẢNH CỦA BẠN (myavt.png)
# ------------------------------------------------------------------------------
Write-Host "`n[1/5] Kiem tra va bao ve anh myavt.png..." -ForegroundColor Yellow

# Neu co anh goc trong frontend hoac backup, sao luu chac chan
$deployAvt = Join-Path $deployFrontend "myavt.png"
$publicAvt = Join-Path $root "HRMS_Web\public\myavt.png"

if (Test-Path $deployAvt) {
    Copy-Item $deployAvt $myavtBackup -Force
} elseif (Test-Path $publicAvt) {
    Copy-Item $publicAvt $myavtBackup -Force
}

if (Test-Path $myavtBackup) {
    $avtSize = (Get-Item $myavtBackup).Length
    Write-Host "  -> [OK] Da bao toan anh myavt.png (Kich thuoc: $([Math]::Round($avtSize / 1MB, 2)) MB)" -ForegroundColor Green
    # Dam bao file goc luon nam trong public/ cua web de khi vite build luon lay dung anh nay
    Copy-Item $myavtBackup $publicAvt -Force
} else {
    Write-Host "  -> [Canh bao] Khong tim thay file myavt.png de backup!" -ForegroundColor Red
}

# Xóa vĩnh viễn author.jpg (không bao giờ dùng)
Remove-Item (Join-Path $root "HRMS_Web\public\author.jpg") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $root "HRMS_Web\src\assets\author.jpg") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $deployFrontend "author.jpg") -Force -ErrorAction SilentlyContinue

# ------------------------------------------------------------------------------
# 2. BUILD FRONTEND (HRMS_Web)
# ------------------------------------------------------------------------------
Write-Host "`n[2/5] Build Frontend (React + Vite)..." -ForegroundColor Yellow
Set-Location (Join-Path $root "HRMS_Web")
npm run build
if ($LASTEXITCODE -ne 0) {
    throw "Build Frontend that bai!"
}
Write-Host "  -> [OK] Build Frontend thanh cong!" -ForegroundColor Green

# Don dep assets cu trong deploy_vps\frontend\assets de khong bi rac file cu
if (-not (Test-Path $deployFrontend)) {
    New-Item -ItemType Directory -Path $deployFrontend -Force | Out-Null
}
$deployAssets = Join-Path $deployFrontend "assets"
if (Test-Path $deployAssets) {
    Remove-Item (Join-Path $deployAssets "*") -Force -Recurse -ErrorAction SilentlyContinue
}

# Copy ket qua build dist vao deploy_vps\frontend
Copy-Item -Recurse -Force (Join-Path $root "HRMS_Web\dist\*") $deployFrontend

# Xóa vĩnh viễn author.jpg nếu vô tình lọt vào dist
Remove-Item (Join-Path $deployFrontend "author.jpg") -Force -ErrorAction SilentlyContinue

# Khoi phuc va dam bao 100% myavt.png luon dung la anh cua ban
if (Test-Path $myavtBackup) {
    Copy-Item $myavtBackup $deployAvt -Force
    Write-Host "  -> [OK] Da giu nguyen anh myavt.png trong $deployFrontend" -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# 3. BUILD BACKEND (HRMS_API)
# ------------------------------------------------------------------------------
Write-Host "`n[3/5] Build Backend (ASP.NET Web API 2)..." -ForegroundColor Yellow
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = $null

if (Test-Path $vswhere) {
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
}

if (-not $msbuild -or -not (Test-Path $msbuild)) {
    $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
}

if (-not (Test-Path $msbuild)) {
    throw "Khong tim thay MSBuild.exe tai: $msbuild"
}

Set-Location $root
& $msbuild (Join-Path $root "HRMS_API\HRMS_API.csproj") /p:Configuration=Debug /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
    throw "Build Backend that bai!"
}
Write-Host "  -> [OK] Build Backend thanh cong!" -ForegroundColor Green

# Copy backend files vao deploy_vps\backend
if (-not (Test-Path $deployBackend)) {
    New-Item -ItemType Directory -Path $deployBackend -Force | Out-Null
}
$deployBin = Join-Path $deployBackend "bin"
if (-not (Test-Path $deployBin)) {
    New-Item -ItemType Directory -Path $deployBin -Force | Out-Null
}

Copy-Item -Recurse -Force (Join-Path $root "HRMS_API\bin\*") $deployBin
$deployWebConfig = Join-Path $deployBackend "Web.config"
Copy-Item -Force (Join-Path $root "HRMS_API\Web.config") $deployWebConfig
# Tu dong cau hinh cho Oracle XE tren VPS (localhost:1521/xe)
(Get-Content $deployWebConfig -Raw) -replace '1521/orcl', '1521/xe' | Set-Content $deployWebConfig -Encoding UTF8
Write-Host "  -> [OK] Da tu dong cau hinh connectionString = 1521/xe cho VPS!" -ForegroundColor Green
Copy-Item -Force (Join-Path $root "HRMS_API\Global.asax") (Join-Path $deployBackend "Global.asax")

# Copy ai_prompts.json vao ca root va bin cua backend
$aiPromptsSrc = Join-Path $root "HRMS_API\ai_prompts.json"
if (Test-Path $aiPromptsSrc) {
    Copy-Item -Force $aiPromptsSrc (Join-Path $deployBackend "ai_prompts.json")
    Copy-Item -Force $aiPromptsSrc (Join-Path $deployBin "ai_prompts.json")
    Write-Host "  -> [OK] Da copy ai_prompts.json vao deploy_vps\backend va bin!" -ForegroundColor Green
}

if (Test-Path (Join-Path $root "HRMS_API\Views")) {
    Copy-Item -Recurse -Force (Join-Path $root "HRMS_API\Views") $deployBackend
}
if (Test-Path (Join-Path $root "HRMS_API\Content")) {
    Copy-Item -Recurse -Force (Join-Path $root "HRMS_API\Content") $deployBackend
}
if (Test-Path (Join-Path $root "HRMS_API\Scripts")) {
    Copy-Item -Recurse -Force (Join-Path $root "HRMS_API\Scripts") $deployBackend
}

# ------------------------------------------------------------------------------
# 4. TAO CAC FILE CHAY TU DONG TRONG D:\QL_NS\deploy_vps
# ------------------------------------------------------------------------------
Write-Host "`n[4/6] Tao cac file chay tu dong va huong dan vao $deployRoot..." -ForegroundColor Yellow

# 4.1 Setup-VPS-IIS.ps1
$setupScript = @'
# ==============================================================================
# SCRIPT TU DONG CAI DAT IIS & CAU HINH HRMS TREN WINDOWS SERVER 2022
# Chay script nay bang PowerShell voi quyen Administrator tren VPS
# ==============================================================================

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  BAT DAU CAI DAT IIS VA CAU HINH HRMS TREN VPS..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Cai dat Web Server (IIS) va ASP.NET 4.8 Features
Write-Host "[1/5] Kiem tra va cai dat IIS + ASP.NET 4.8..." -ForegroundColor Yellow
$features = @(
    "Web-Server",
    "Web-WebServer",
    "Web-Common-Http",
    "Web-Default-Doc",
    "Web-Dir-Browsing",
    "Web-Http-Errors",
    "Web-Static-Content",
    "Web-Health",
    "Web-Http-Logging",
    "Web-Performance",
    "Web-Stat-Compression",
    "Web-Security",
    "Web-Filtering",
    "Web-App-Dev",
    "Web-Net-Ext45",
    "Web-Asp-Net45",
    "Web-ISAPI-Ext",
    "Web-ISAPI-Filter",
    "Web-Mgmt-Tools",
    "Web-Mgmt-Console"
)

if (Get-Command Get-WindowsFeature -ErrorAction SilentlyContinue) {
    foreach ($f in $features) {
        $status = Get-WindowsFeature -Name $f -ErrorAction SilentlyContinue
        if ($status -and -not $status.Installed) {
            Write-Host "  -> Installing $f..." -ForegroundColor Gray
            Install-WindowsFeature -Name $f -IncludeManagementTools | Out-Null
        }
    }
    Write-Host "[OK] IIS va ASP.NET 4.8 da san sang!" -ForegroundColor Green
} else {
    Write-Host "[OK] IIS da san sang tren may nay!" -ForegroundColor Green
}

# Kiem tra va cai dat URL Rewrite Module 2.1 neu chua co
$rewriteInstalled = Test-Path "HKLM:\SOFTWARE\Microsoft\IIS Extensions\URL Rewrite"
if (-not $rewriteInstalled) {
    Write-Host "  -> Dang tai va cai dat IIS URL Rewrite Module 2.1 cho SPA React..." -ForegroundColor Gray
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $msiUrl = "https://download.microsoft.com/download/1/2/8/128E2E03-C533-4BB4-A56E-E2DE65868F90/rewrite_amd64_en-US.msi"
        $msiFile = "$env:TEMP\rewrite_amd64_en-US.msi"
        Invoke-WebRequest -Uri $msiUrl -OutFile $msiFile -UseBasicParsing
        Start-Process msiexec.exe -ArgumentList "/i `"$msiFile`" /quiet /norestart" -Wait
        Write-Host "  -> [OK] Da cai dat IIS URL Rewrite 2.1!" -ForegroundColor Green
    } catch {
        Write-Host "  -> [Luu y] Khong the tai tu dong URL Rewrite, ban co the cai thu cong neu can." -ForegroundColor Yellow
    }
} else {
    Write-Host "  -> [OK] IIS URL Rewrite 2.1 da duoc cai dat." -ForegroundColor Green
}

# 2. Tao thu muc chua web va backend
Write-Host "[2/5] Tao thu muc ung dung C:\HRMS..." -ForegroundColor Yellow
$hrmsRoot = "C:\HRMS"
$apiPath = "C:\HRMS\backend"
$webPath = "C:\HRMS\frontend"

# Tam dung IIS de tranh khoa DLL khi chep de
iisreset /stop | Out-Null
Start-Sleep -Seconds 1

if (-not (Test-Path $hrmsRoot)) { New-Item -ItemType Directory -Path $hrmsRoot | Out-Null }

# Tu dong copy tu thu muc hien tai neu script dang nam trong thu muc deploy
$currentDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ($currentDir.TrimEnd('\') -ne $hrmsRoot.TrimEnd('\')) {
    if (Test-Path (Join-Path $currentDir "backend")) {
        Write-Host "  -> Copying backend to $apiPath..." -ForegroundColor Gray
        Copy-Item -Recurse -Force (Join-Path $currentDir "backend") $hrmsRoot
    }
    if (Test-Path (Join-Path $currentDir "frontend")) {
        Write-Host "  -> Copying frontend to $webPath..." -ForegroundColor Gray
        Copy-Item -Recurse -Force (Join-Path $currentDir "frontend") $hrmsRoot
    }
} else {
    Write-Host "  -> Thu muc chay trung C:\HRMS, bo qua copy de tranh loi trung lap!" -ForegroundColor Green
}

# Dam bao file ai_prompts.json co mat
$srcPrompts = Join-Path $currentDir "backend\ai_prompts.json"
if (Test-Path $srcPrompts) {
    Copy-Item -Force $srcPrompts (Join-Path $apiPath "ai_prompts.json")
    Copy-Item -Force $srcPrompts (Join-Path $apiPath "bin\ai_prompts.json")
}

# Cap quyen doc/thuc thi cho IIS_IUSRS
Write-Host "  -> Cap quyen truy cap cho IIS_IUSRS..." -ForegroundColor Gray
$acl = Get-Acl $hrmsRoot
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl $hrmsRoot $acl

# 3. Mo Firewall Port 80, 5000, 1521
Write-Host "[3/5] Mo Windows Firewall cho Port 80 (Web), Port 5000 (API)..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName "HRMS Web (Port 80)" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow -ErrorAction SilentlyContinue | Out-Null
New-NetFirewallRule -DisplayName "HRMS API (Port 5000)" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow -ErrorAction SilentlyContinue | Out-Null
Write-Host "[OK] Firewall ports opened!" -ForegroundColor Green

# 4. Cau hinh IIS
Write-Host "[4/5] Cau hinh IIS Application Pools & Sites..." -ForegroundColor Yellow
Import-Module WebAdministration

# Tao AppPool cho HRMS API (.NET v4.0, Integrated, Enable 32-bit: False)
$apiPoolName = "HRMS_API_Pool"
if (-not (Test-Path "IIS:\AppPools\$apiPoolName")) {
    $pool = New-Item "IIS:\AppPools\$apiPoolName"
    $pool.managedRuntimeVersion = "v4.0"
    $pool.managedPipelineMode = "Integrated"
    $pool | Set-Item
} else {
    Set-ItemProperty "IIS:\AppPools\$apiPoolName" -Name "managedRuntimeVersion" -Value "v4.0"
    Set-ItemProperty "IIS:\AppPools\$apiPoolName" -Name "managedPipelineMode" -Value "Integrated"
}

# Cau hinh Default Web Site hoac HRMS_Web tren Port 80
$webSiteName = "Default Web Site"
if (Test-Path "IIS:\Sites\$webSiteName") {
    Set-ItemProperty "IIS:\Sites\$webSiteName" -Name "physicalPath" -Value $webPath
    Restart-WebItem "IIS:\Sites\$webSiteName"
    Write-Host "  -> Da gan $webSiteName (Port 80) tro den $webPath" -ForegroundColor Green
} else {
    New-Website -Name "HRMS_Web" -Port 80 -PhysicalPath $webPath -ApplicationPool "DefaultAppPool"
    Write-Host "  -> Da tao site HRMS_Web (Port 80)" -ForegroundColor Green
}

# Cau hinh HRMS_API tren Port 5000
$apiSiteName = "HRMS_API"
if (Test-Path "IIS:\Sites\$apiSiteName") {
    Stop-Website -Name $apiSiteName -ErrorAction SilentlyContinue
    Remove-Website -Name $apiSiteName
}
New-Website -Name $apiSiteName -Port 5000 -PhysicalPath $apiPath -ApplicationPool $apiPoolName

# Cau hinh Sub-Application /api tren Port 80 (chay truc tiep tren port 80 khong can mo firewall port 5000)
if (Test-Path "IIS:\Sites\$webSiteName\api") {
    Remove-WebApplication -Site $webSiteName -Name "api" -ErrorAction SilentlyContinue
}
New-WebApplication -Site $webSiteName -Name "api" -PhysicalPath $apiPath -ApplicationPool $apiPoolName
Write-Host "  -> Da tao sub-app /api tren Port 80 (http://localhost/api)" -ForegroundColor Green

Write-Host "  -> Da tao site HRMS_API (Port 5000) voi AppPool $apiPoolName tro den $apiPath" -ForegroundColor Green

# 5. Khoi dong lai IIS
Write-Host "[5/5] Khoi dong lai IIS..." -ForegroundColor Yellow
iisreset /restart

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  HOAN TAT CAI DAT!" -ForegroundColor Green
Write-Host "  1. Backend API: http://localhost:5000/api/nhanvien" -ForegroundColor Cyan
Write-Host "  2. Frontend Web: http://localhost" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green
'@
$setupScript | Set-Content (Join-Path $deployRoot "Setup-VPS-IIS.ps1") -Encoding UTF8
Write-Host "  -> [OK] Da tao Setup-VPS-IIS.ps1" -ForegroundColor Green

# 4.2 Setup-VPS-IIS.bat (Chay truc tiep Setup-VPS-IIS.ps1 voi quyen Administrator)
$batSetup = @'
@echo off
chcp 65001 >nul
title CAI DAT VA CAU HINH HRMS TREN VPS

:: Kiem tra va tu dong xin quyen Administrator
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [THONG BAO] Dang yeu cau quyen Administrator de cai dat IIS...
    powershell -Command "Start-Process '%~0' -Verb RunAs"
    exit /b
)

cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup-VPS-IIS.ps1"

echo.
echo ==========================================================
echo Nhan phim bat ky de thoat...
pause >nul
'@
$batSetup | Set-Content (Join-Path $deployRoot "Setup-VPS-IIS.bat") -Encoding ASCII
Write-Host "  -> [OK] Da tao Setup-VPS-IIS.bat" -ForegroundColor Green

# Don dep cac file cu/thua neu co
Remove-Item (Join-Path $deployRoot "1-CAI-DAT-VA-CHAY-TU-DONG.bat") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $deployRoot "2-KIEM-TRA-TRANG-THAI.bat") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $deployRoot "3-RESTART-IIS.bat") -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $deployRoot "Check-Status.ps1") -Force -ErrorAction SilentlyContinue

# 4.3 HUONG_DAN_SU_DUNG_VPS.txt
$guide = @'
================================================================================
          HUONG DAN TRIEN KHAI HRMS LEN VPS WINDOWS SERVER
================================================================================

1. BUOC 1: COPY LEN VPS
   - Copy toan bo thu muc 'deploy_vps' (hoac giai nen file 'deploy_vps.zip') 
     vao bat ky thu muc nao tren VPS (vi du: C:\deploy_vps).

2. BUOC 2: CAI DAT & CHAY TU DONG CHI VOI 1 CLICK
   - Click dup vao file:
     >>> [ Setup-VPS-IIS.bat ] <<<
   - File se tu dong xin quyen Administrator va thuc hien theo 5 buoc chuan:
     + [1/5] Cai dat IIS Web Server, ASP.NET 4.8 va IIS URL Rewrite Module 2.1.
     + [2/5] Sao chep frontend, backend vao C:\HRMS va phan quyen IIS_IUSRS.
     + [3/5] Mo Windows Firewall Port 80 (Web) va Port 5000 (API).
     + [4/5] Cau hinh AppPool HRMS_API_Pool, Website Port 80 va Sub-App /api.
     + [5/5] Khoi dong lai IIS va in ket qua hoan tat.

3. BUOC 3: TRUY CAP HE THONG
   - Giao dien Web : http://localhost (hoac http://<IP_VPS>)
   - Backend API   : http://localhost/api (hoac http://localhost:5000)

4. LUU Y VE KET NOI ORACLE DATABASE
   - Mac dinh ket noi Database tro toi: localhost:1521/xe (Oracle XE).
   - Neu VPS cua ban dung ten Service khac (vi du: 'orcl' hoac 'ORCLPDB'):
     Mo file: C:\HRMS\backend\Web.config
     Tim dong "localhost:1521/xe" va thay doi thanh ten Service cua ban.
     Sau do mo CMD/PowerShell chay "iisreset" de ap dung.

================================================================================
'@
$guide | Set-Content (Join-Path $deployRoot "HUONG_DAN_SU_DUNG_VPS.txt") -Encoding UTF8
Write-Host "  -> [OK] Da tao HUONG_DAN_SU_DUNG_VPS.txt" -ForegroundColor Green

# ------------------------------------------------------------------------------
# 5. DONG GOI ZIP CHO VPS
# ------------------------------------------------------------------------------
if (-not $SkipZip) {
    Write-Host "`n[5/6] Dong goi file ZIP de copy sang VPS..." -ForegroundColor Yellow
    $zipPath = "D:\QL_NS\deploy_vps.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $deployRoot "*") -DestinationPath $zipPath -Force
    $zipSize = (Get-Item $zipPath).Length
    Write-Host "  -> [OK] Da tao: $zipPath ($([Math]::Round($zipSize / 1MB, 2)) MB)" -ForegroundColor Green
} else {
    Write-Host "`n[5/6] Bo qua buoc tao file ZIP." -ForegroundColor Gray
}

# ------------------------------------------------------------------------------
# 6. TONG KET KIEM TRA
# ------------------------------------------------------------------------------
Write-Host "`n[6/6] Kiem tra ket qua deploy:" -ForegroundColor Yellow
$finalAvt = Join-Path $deployFrontend "myavt.png"
if (Test-Path $finalAvt) {
    $len = (Get-Item $finalAvt).Length
    Write-Host "  -> [ANH MYAVT.PNG]: HOAN TOAN NGUYEN VEN ($len bytes)" -ForegroundColor Green
} else {
    Write-Host "  -> [LOI]: Khong thay myavt.png!" -ForegroundColor Red
}

$finalPrompts = Join-Path $deployBackend "ai_prompts.json"
if (Test-Path $finalPrompts) {
    Write-Host "  -> [AI_PROMPTS.JSON]: HOAN TAT ($((Get-Item $finalPrompts).Length) bytes)" -ForegroundColor Green
} else {
    Write-Host "  -> [LOI]: Khong thay ai_prompts.json trong backend!" -ForegroundColor Red
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "  BUILD VA DONG GOI HOAN TAT!" -ForegroundColor Green
Write-Host "  1. Thu muc deploy : D:\QL_NS\deploy_vps" -ForegroundColor Cyan
Write-Host "  2. File ZIP copy  : D:\QL_NS\deploy_vps.zip" -ForegroundColor Cyan
Write-Host "  3. Anh avatar     : myavt.png da duoc bao toan!" -ForegroundColor Cyan
Write-Host "  4. File chay tu dong: Setup-VPS-IIS.bat" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green
