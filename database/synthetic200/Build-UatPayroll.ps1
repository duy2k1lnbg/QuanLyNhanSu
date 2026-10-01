$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$out="$root\artifacts\synthetic200\payroll"
[void](New-Item -ItemType Directory -Force $out)
Copy-Item "$root\HRMS.Business\bin\Debug\*.dll" $out -Force
Copy-Item "$root\HRMS.DataAccess\bin\Debug\DA.dll" $out -Force
[xml]$config=Get-Content "$root\HRMS.Tests\App.config" -Raw
[void]$config.configuration.RemoveChild($config.configuration.connectionStrings)
[void]$config.configuration.RemoveChild($config.configuration.applicationSettings)
foreach($binding in $config.SelectNodes("//*[local-name()='dependentAssembly']")){
 $dll=Join-Path $out ($binding.assemblyIdentity.name+'.dll')
 if(Test-Path -LiteralPath $dll){
  $version=[Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString()
  $binding.bindingRedirect.newVersion=$version
  $binding.bindingRedirect.oldVersion='0.0.0.0-'+$version
 }
}
$config.Save("$out\UatPayroll.exe.config")
$csc="$root\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\csc.exe"
& $csc /nologo /langversion:latest /target:exe "/out:$out/UatPayroll.exe" "/r:$out/Bu.dll" "/r:$out/DA.dll" "/r:$out/EntityFramework.dll" "/r:$out/Oracle.ManagedDataAccess.dll" "$root/database/synthetic200/UatPayroll.cs"
if($LASTEXITCODE -ne 0){throw 'UAT payroll compilation failed'}
