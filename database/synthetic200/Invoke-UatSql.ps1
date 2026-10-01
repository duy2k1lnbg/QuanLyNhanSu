param([string]$Path,[string]$Query)
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
$uatDb=New-Object Oracle.ManagedDataAccess.Client.OracleConnection($builder.ConnectionString)
$uatDb.Open()
try {
 $cmd=$uatDb.CreateCommand();$cmd.CommandText='SELECT USER FROM DUAL'
 if($cmd.ExecuteScalar() -ne 'HR_UAT_200'){throw 'Refusing to run outside HR_UAT_200'}
 if($Query){
  $cmd=$uatDb.CreateCommand();$cmd.CommandText=$Query;$cmd.CommandTimeout=120
  $r=$cmd.ExecuteReader();$rows=@()
  while($r.Read()){$row=[ordered]@{};for($i=0;$i -lt $r.FieldCount;$i++){$row[$r.GetName($i)]=if($r.IsDBNull($i)){$null}else{[string]$r.GetValue($i)}};$rows+=[pscustomobject]$row}
  $r.Close();ConvertTo-Json -InputObject $rows -Compress -Depth 5
 } elseif($Path) {
  $sql=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
  $blocks=[regex]::Split($sql,'(?m)^\s*/\s*$')
  foreach($block in $blocks){if([string]::IsNullOrWhiteSpace($block)){continue}
   $cmd=$uatDb.CreateCommand();$cmd.CommandTimeout=600;$cmd.CommandText=$block.Trim()
   if($cmd.CommandText -notmatch '^(?s)\s*(--[^\r\n]*\r?\n\s*)*(DECLARE|BEGIN|CREATE\s+(OR\s+REPLACE\s+)?(TRIGGER|PROCEDURE|FUNCTION|PACKAGE|TYPE))\b'){$cmd.CommandText=$cmd.CommandText.TrimEnd(';')}
   [void]$cmd.ExecuteNonQuery()
  }
  Write-Output ('APPLIED: '+$Path)
 } else {throw 'Provide Path or Query'}
} finally {$uatDb.Close();$uatDb.Dispose()}
