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
# 4. DONG GOI ZIP CHO VPS
# ------------------------------------------------------------------------------
if (-not $SkipZip) {
    Write-Host "`n[4/5] Dong goi file ZIP de copy sang VPS..." -ForegroundColor Yellow
    $zipPath = "D:\QL_NS\deploy_vps.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $deployRoot "*") -DestinationPath $zipPath -Force
    $zipSize = (Get-Item $zipPath).Length
    Write-Host "  -> [OK] Da tao: $zipPath ($([Math]::Round($zipSize / 1MB, 2)) MB)" -ForegroundColor Green
} else {
    Write-Host "`n[4/5] Bo qua buoc tao file ZIP." -ForegroundColor Gray
}

# ------------------------------------------------------------------------------
# 5. TONG KET KIEM TRA
# ------------------------------------------------------------------------------
Write-Host "`n[5/5] Kiem tra ket qua deploy:" -ForegroundColor Yellow
$finalAvt = Join-Path $deployFrontend "myavt.png"
if (Test-Path $finalAvt) {
    $len = (Get-Item $finalAvt).Length
    Write-Host "  -> [ANH MYAVT.PNG]: HOAN TOAN NGUYEN VEN ($len bytes)" -ForegroundColor Green
} else {
    Write-Host "  -> [LOI]: Khong thay myavt.png!" -ForegroundColor Red
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "  BUILD VA DONG GOI HOAN TAT!" -ForegroundColor Green
Write-Host "  1. Thu muc deploy : D:\QL_NS\deploy_vps" -ForegroundColor Cyan
Write-Host "  2. File ZIP copy  : D:\QL_NS\deploy_vps.zip" -ForegroundColor Cyan
Write-Host "  3. Anh avatar     : myavt.png da duoc bao toan!" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green
