# Run in elevated Windows PowerShell 5.1 (not PowerShell 7).
[CmdletBinding()]
param(
    [ValidateSet('Install','Start','Stop','Restart','Status','Uninstall')][string]$Action = 'Status',
    [string]$ProfilePath,
    [string]$AppDirectory = (Split-Path $PSScriptRoot -Parent)
)
$ErrorActionPreference = 'Stop'
$serviceName = 'ScrapeXServer'
$installDirectory = Join-Path $env:ProgramFiles 'ScrapeX\Service'
$dataDirectory = Join-Path $env:ProgramData 'ScrapeX\Service'
$registryPath = 'HKLM:\SOFTWARE\ScrapeXServiceInstaller'
$binaryPath = '"' + (Join-Path $installDirectory 'ScrapeX.exe') + '" --service'
$existing = Get-CimInstance Win32_Service -Filter "Name='ScrapeXServer'"
if ($existing -and $existing.PathName -ne $binaryPath) { throw 'ScrapeXServer belongs to a different installation. No changes were made.' }
if ($existing -and $existing.StartName -ne 'NT AUTHORITY\LocalService') { throw 'Existing service is not LocalService. Review its account before updating.' }
if ($Action -eq 'Status') {
    if ($existing) { $existing | Select-Object Name, State, StartMode, StartName, PathName }
    else { Write-Output 'ScrapeXServer is not installed.' }
    Write-Output "Data and logs: $dataDirectory"
    return
}
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Open Windows PowerShell as administrator using the same account that exported the profile.' }
function Invoke-Native([string]$File, [string[]]$Arguments) {
    & $File @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$File failed with exit code $LASTEXITCODE." }
}
function Stop-OwnedService {
    $service = Get-Service $serviceName -ErrorAction SilentlyContinue
    if ($service -and $service.Status -ne 'Stopped') {
        Stop-Service $serviceName
        $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(40))
    }
}
function Assert-PlainDirectory([string]$Path) {
    while (-not (Test-Path -LiteralPath $Path)) { $Path = Split-Path $Path -Parent }
    $item = Get-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
    while ($item) {
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Refusing redirected installation directory: $($item.FullName)" }
        $item = $item.Parent
    }
}
Assert-PlainDirectory $installDirectory
Assert-PlainDirectory $dataDirectory
if ($Action -eq 'Stop') { Stop-OwnedService; return }
if ($Action -eq 'Start') { Start-Service $serviceName; return }
if ($Action -eq 'Restart') { Stop-OwnedService; Start-Service $serviceName; return }
if ($Action -eq 'Uninstall') {
    Stop-OwnedService
    if ($existing) { Invoke-Native sc.exe @('delete', $serviceName) }
    $ownedUrls = @((Get-ItemProperty -LiteralPath $registryPath -Name OwnedUrls -ErrorAction SilentlyContinue).OwnedUrls)
    foreach ($url in $ownedUrls) {
        if ($url -match '^http://(127\.0\.0\.1|\+):[0-9]{4,5}/$') {
            $reservation = (& netsh.exe http show urlacl "url=$url") -join "`n"
            if ($LASTEXITCODE -eq 0 -and $reservation -match 'D:\(A;;GX;;;(?:LS|S-1-5-19)\)') {
                Invoke-Native netsh.exe @('http','delete','urlacl',"url=$url")
            }
        }
    }
    if (Test-Path -LiteralPath $registryPath) { Remove-Item -LiteralPath $registryPath }
    Write-Output 'Service removed. Binaries, results and settings are retained; no user data was deleted.'
    return
}
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Installation requires Windows PowerShell 5.1 for .NET Framework profile encryption.' }
$AppDirectory = [IO.Path]::GetFullPath($AppDirectory)
if ($AppDirectory.TrimEnd('\') -eq $installDirectory.TrimEnd('\')) { throw 'Run the installer from an extracted release, not the installed service directory.' }
foreach ($file in @('ScrapeX.exe','ScrapeX.exe.config','Newtonsoft.Json.dll','sources.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $AppDirectory $file) -PathType Leaf)) { throw "Missing $file in AppDirectory. For a source build use -AppDirectory .\ScrapeX\bin\Release." }
}
Add-Type -Path (Join-Path $AppDirectory 'Newtonsoft.Json.dll')
[void][Reflection.Assembly]::LoadFrom((Join-Path $AppDirectory 'ScrapeX.exe'))
$installedProfile = Join-Path $dataDirectory 'service-profile.json'
if ($ProfilePath) { $profile = [ScrapeX.ServiceProfile]::Load([IO.Path]::GetFullPath($ProfilePath), $false) }
elseif (Test-Path -LiteralPath $installedProfile) { $profile = [ScrapeX.ServiceProfile]::Load($installedProfile, $true) }
else { throw 'Export a Windows service profile in Server & API first, then supply -ProfilePath.' }
$prefix = $profile.Server.Prefix
$reservation = (& netsh.exe http show urlacl "url=$prefix") -join "`n"
$reservationExists = $LASTEXITCODE -eq 0 -and $reservation.Contains($prefix)
if ($reservationExists -and $reservation -notmatch '\(A;;GX;;;(?:LS|S-1-5-19)\)') { throw "The URL reservation for $prefix belongs to another account. Choose another port or review it manually; it was not changed." }
Stop-OwnedService
New-Item -ItemType Directory -Path $installDirectory, $dataDirectory -Force | Out-Null
# Restrict machine-encrypted secrets to administrators, SYSTEM and LocalService.
$acl = New-Object Security.AccessControl.DirectorySecurity
$acl.SetAccessRuleProtection($true, $false)
foreach ($entry in @(@('S-1-5-18','FullControl'),@('S-1-5-32-544','FullControl'),@('S-1-5-19','Modify'))) {
    $sid = New-Object Security.Principal.SecurityIdentifier($entry[0])
    $rule = New-Object Security.AccessControl.FileSystemAccessRule($sid, $entry[1], 'ContainerInherit,ObjectInherit', 'None', 'Allow')
    $acl.AddAccessRule($rule)
}
Set-Acl -LiteralPath $dataDirectory -AclObject $acl
foreach ($file in @('ScrapeX.exe','ScrapeX.exe.config','Newtonsoft.Json.dll','sources.txt')) {
    Copy-Item -LiteralPath (Join-Path $AppDirectory $file) -Destination (Join-Path $installDirectory $file) -Force
}
$profile.Save($installedProfile, $true)
if (-not $reservationExists) {
    Invoke-Native netsh.exe @('http','add','urlacl',"url=$prefix",'sddl=D:(A;;GX;;;LS)')
    New-Item -Path $registryPath -Force | Out-Null
    $owned = @((Get-ItemProperty -LiteralPath $registryPath -Name OwnedUrls -ErrorAction SilentlyContinue).OwnedUrls) | Where-Object { $_ }
    New-ItemProperty -LiteralPath $registryPath -Name OwnedUrls -PropertyType MultiString -Value @($owned + $prefix | Select-Object -Unique) -Force | Out-Null
}
if (-not $existing) {
    $credential = New-Object Management.Automation.PSCredential('NT AUTHORITY\LocalService', (New-Object Security.SecureString))
    New-Service -Name $serviceName -DisplayName 'ScrapeX Proxy Feed' -BinaryPathName $binaryPath -StartupType Automatic -Credential $credential -Description 'Scheduled proxy discovery, HTTP/TLS verification and filtered API feeds.' | Out-Null
} elseif ($existing.StartName -ne 'NT AUTHORITY\LocalService') { throw 'Existing service is not LocalService. Review its account before updating.' }
Invoke-Native sc.exe @('config',$serviceName,'start=','delayed-auto')
Invoke-Native sc.exe @('failure',$serviceName,'reset=','86400','actions=','restart/5000/restart/15000/restart/60000')
Invoke-Native sc.exe @('failureflag',$serviceName,'1')
Start-Service $serviceName
# SCM Running means OnStart returned; verify the actual API as well.
$ready = $false
for ($attempt = 0; $attempt -lt 30; $attempt++) {
    try {
        $stats = Invoke-RestMethod -Uri "http://127.0.0.1:$($profile.Server.Port)/api/v1/stats" -Headers @{'X-Api-Key'=$profile.Server.ApiKey} -TimeoutSec 2
        if ($stats.refresh.mode -eq 'windows-service') { $ready = $true; break }
    } catch { }
    Start-Sleep -Seconds 1
}
if (-not $ready) { throw "Service installed but API readiness failed. Stop the desktop listener if it uses this port; inspect $dataDirectory\service.log." }
Write-Output "ScrapeX service is ready. It starts after reboot and runs without the desktop app. Logs: $dataDirectory\service.log"
Write-Output 'No firewall rule was added. Use a reverse proxy or explicitly allow the chosen port for remote clients.'
