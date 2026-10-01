param([switch]$OnlyPure)
$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$out=if($OnlyPure){"$root\artifacts\synthetic200\pure"}else{"$root\artifacts\synthetic200\bin"}
[void](New-Item -ItemType Directory -Force $out)
foreach($name in @('Newtonsoft.Json.dll','nunit.framework.dll','DA.dll','EntityFramework.dll','System.ValueTuple.dll')){
 Copy-Item "$root\HRMS.Tests\bin\Debug\net472\$name" $out -Force
}
Copy-Item 'D:\Oracle\ODP.NET\managed\common\Oracle.ManagedDataAccess.dll' $out -Force
$tests=Get-Content "$root\HRMS.Tests\TimeSegmentationEngineTests.cs" -Raw -Encoding UTF8
$mark=$tests.IndexOf('public void Test10_OracleDatabase_ConstraintsAcceptEngineOutputs')
$cut=$tests.LastIndexOf('[Test]',$mark)
$safe=$tests.Substring(0,$cut).Replace('using Oracle.ManagedDataAccess.Client;','')+"    } }"
[IO.File]::WriteAllText("$out\PureTests.cs",$safe)
$csc="$root\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\csc.exe"
& $csc /nologo /langversion:latest /target:exe "/out:$out/PureTests.exe" "/r:$out/Newtonsoft.Json.dll" "/r:$out/nunit.framework.dll" "$root/HRMS.Business/CLASS_CHAMCONG/TimeSegmentationEngine.cs" "$out/PureTests.cs" "$root/HRMS.Tests/AttendanceRegressionTests.cs" "$root/database/synthetic200/PureRunner.cs"
if($LASTEXITCODE -ne 0){throw 'Pure test compilation failed'}
& "$out/PureTests.exe"
if($LASTEXITCODE -ne 0){throw 'Pure tests failed'}
if($OnlyPure){return}
& $csc /nologo /langversion:latest /target:exe "/out:$out/UatAttendance.exe" "/r:$out/Newtonsoft.Json.dll" "/r:$out/Oracle.ManagedDataAccess.dll" "/r:$out/EntityFramework.dll" "/r:$out/DA.dll" "/r:$out/System.ValueTuple.dll" "$root/HRMS.Business/CLASS_CHAMCONG/TimeSegmentationEngine.cs" "$root/HRMS.Business/CLASS_CHAMCONG/AttendancePublishingService.cs" "$root/database/synthetic200/UatAttendance.cs"
if($LASTEXITCODE -ne 0){throw 'Attendance service compilation failed'}
