$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Add-Type -Path 'D:\Oracle\ODP.NET\managed\common\Oracle.ManagedDataAccess.dll'
[xml]$cfg=Get-Content "$root\HRMS.Api\Web.config" -Raw
$entry=$cfg.configuration.connectionStrings.add | Where-Object {$_.name -eq 'MyEntities'} | Select-Object -First 1
$outer=New-Object System.Data.Common.DbConnectionStringBuilder
$outer.set_ConnectionString([string]$entry.connectionString)
$builder=New-Object Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder
$builder.set_ConnectionString([string]$outer['provider connection string'])
$builder.set_UserID('HR_UAT_200')
$secure=Get-Content "$root\artifacts\synthetic200\oracle-password.dpapi" | ConvertTo-SecureString
$builder.set_Password(([System.Net.NetworkCredential]::new('',$secure)).Password)
$builder.set_DataSource('(DESCRIPTION=(CONNECT_TIMEOUT=5)(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl)))')
$builder.set_Pooling($false)
$env:HRMS_UAT_CONNECTION=$builder.ConnectionString
try { & "$root\artifacts\synthetic200\payroll\UatPayroll.exe"; if($LASTEXITCODE -ne 0){throw 'UAT payroll failed'} }
finally { Remove-Item Env:HRMS_UAT_CONNECTION -ErrorAction SilentlyContinue }