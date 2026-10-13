param([switch]$Live)
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$buildTool = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $buildTool) { throw 'Install Visual Studio Build Tools with .NET Framework 4.8 targeting pack.' }
& $buildTool ScrapeX.sln /p:Configuration=Release /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$compiler = Join-Path (Split-Path $buildTool) 'Roslyn\csc.exe'
& $compiler /nologo /out:ScrapeX\bin\Release\RegressionTests.exe /r:ScrapeX\bin\Release\ScrapeX.exe /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Net.Http.dll tools\RegressionTests.cs
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
& .\ScrapeX\bin\Release\RegressionTests.exe (Join-Path (Get-Location) 'reports\ui-preview.png') | Tee-Object reports\tests.txt
if ($LASTEXITCODE -ne 0) { throw 'Regression test failed.' }
& $compiler /nologo /out:ScrapeX\bin\Release\PerformanceTests.exe /r:ScrapeX\bin\Release\ScrapeX.exe tools\PerformanceTests.cs
if ($LASTEXITCODE -ne 0) { throw 'Performance test compilation failed.' }
& .\ScrapeX\bin\Release\PerformanceTests.exe | Tee-Object reports\performance-tests.txt
if ($LASTEXITCODE -ne 0) { throw 'Performance test failed.' }
foreach ($suite in @('CountryTests','SchedulerTests','ResilienceTests')) {
    & $compiler /nologo "/out:ScrapeX\bin\Release\$suite.exe" /r:ScrapeX\bin\Release\ScrapeX.exe /r:System.Net.Http.dll "tools\$suite.cs"
    if ($LASTEXITCODE -ne 0) { throw "$suite compilation failed." }
    & ".\ScrapeX\bin\Release\$suite.exe" | Tee-Object "reports\$suite.txt"
    if ($LASTEXITCODE -ne 0) { throw "$suite failed." }
}
& $compiler /nologo /out:ScrapeX\bin\Release\CatalogTests.exe /r:ScrapeX\bin\Release\ScrapeX.exe /r:ScrapeX\bin\Release\Newtonsoft.Json.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll tools\CatalogTests.cs
if ($LASTEXITCODE -ne 0) { throw 'Catalog test compilation failed.' }
& .\ScrapeX\bin\Release\CatalogTests.exe | Tee-Object reports\catalog-tests.txt
if ($LASTEXITCODE -ne 0) { throw 'Catalog/export tests failed.' }
if ($Live) {
    & $compiler /nologo /out:ScrapeX\bin\Release\LiveSmoke.exe /r:ScrapeX\bin\Release\ScrapeX.exe tools\LiveSmoke.cs
    if ($LASTEXITCODE -ne 0) { throw 'Live test compilation failed.' }
    & .\ScrapeX\bin\Release\LiveSmoke.exe ScrapeX\sources.txt | Tee-Object reports\live-smoke.txt
    if ($LASTEXITCODE -ne 0) { throw 'Live test failed.' }
}
