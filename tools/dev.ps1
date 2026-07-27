<#
.SYNOPSIS
    Developer helper for QwestRooms: restore, build, test, run and reseed.

.DESCRIPTION
    This project targets .NET Framework 4.8 and ASP.NET MVC 5, so it needs full MSBuild and
    IIS Express rather than the `dotnet` CLI. This script locates both via vswhere so the same
    commands work from VS Code, a plain terminal, or CI.

.EXAMPLE
    .\tools\dev.ps1 restore
    .\tools\dev.ps1 build -Configuration Release
    .\tools\dev.ps1 test
    .\tools\dev.ps1 run
    .\tools\dev.ps1 stop
    .\tools\dev.ps1 reseed
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('restore', 'build', 'test', 'run', 'stop', 'reseed')]
    [string]$Command,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$RepoRoot   = Split-Path -Parent $PSScriptRoot
$Solution   = Join-Path $RepoRoot 'QwestRooms.sln'
$SiteFolder = Join-Path $RepoRoot 'QwestRooms.UI'
$Port       = 58221
$Database   = 'QwestRooms.DAL.RoomsContext'
$LocalDb    = '(LocalDb)\MSSQLLocalDB'

function Find-VsWhere {
    $p = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $p)) {
        throw "vswhere.exe not found. Visual Studio 2022 or the Build Tools are required."
    }
    return $p
}

function Find-MSBuild {
    $vswhere = Find-VsWhere
    $path = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild `
                       -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $path) { throw "MSBuild not found. Install the .NET desktop build tools." }
    return $path
}

function Find-VsTest {
    $vswhere = Find-VsWhere
    $path = & $vswhere -latest -products * -find '**\TestPlatform\vstest.console.exe' |
            Select-Object -First 1
    if (-not $path) { throw "vstest.console.exe not found." }
    return $path
}

function Find-IisExpress {
    $candidates = @(
        (Join-Path $env:ProgramFiles 'IIS Express\iisexpress.exe'),
        (Join-Path ${env:ProgramFiles(x86)} 'IIS Express\iisexpress.exe')
    )
    $path = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $path) { throw "IIS Express not found. It installs with Visual Studio." }
    return $path
}

function Get-NuGet {
    # packages.config projects cannot be restored by `msbuild -t:restore` or `dotnet restore`,
    # so a real nuget.exe is required. Fetch it once into tools/ if it is not already on PATH.
    $onPath = Get-Command nuget -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $local = Join-Path $PSScriptRoot 'nuget.exe'
    if (-not (Test-Path $local)) {
        Write-Host 'Downloading nuget.exe into tools/ ...' -ForegroundColor Cyan
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' `
                          -OutFile $local
    }
    return $local
}

function Invoke-Restore {
    $nuget = Get-NuGet
    Write-Host "Restoring packages..." -ForegroundColor Cyan
    & $nuget restore $Solution -NonInteractive
    if ($LASTEXITCODE -ne 0) { throw "Restore failed." }
}

function Invoke-Build {
    $msbuild = Find-MSBuild
    Write-Host "Building ($Configuration)..." -ForegroundColor Cyan
    & $msbuild $Solution "/p:Configuration=$Configuration" /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed." }
}

function Invoke-Test {
    $vstest = Find-VsTest
    $assembly = Join-Path $RepoRoot "QwestRooms.Tests\bin\$Configuration\net48\QwestRooms.Tests.dll"
    if (-not (Test-Path $assembly)) { throw "Test assembly not found; build first." }
    & $vstest $assembly '/Logger:console;verbosity=minimal'
    if ($LASTEXITCODE -ne 0) { throw "Tests failed." }
}

function Stop-Site {
    Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "IIS Express stopped." -ForegroundColor Yellow
}

function Start-Site {
    Stop-Site
    $iis = Find-IisExpress
    Start-Process -FilePath $iis -ArgumentList "/path:`"$SiteFolder`"", "/port:$Port" -WindowStyle Hidden
    $url = "http://localhost:$Port/"
    Write-Host "Starting $url" -ForegroundColor Cyan
    Write-Host "First request creates and seeds the database, so it can take a minute." -ForegroundColor DarkGray

    for ($i = 0; $i -lt 150; $i++) {
        try {
            Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 600 | Out-Null
            Write-Host "Ready: $url" -ForegroundColor Green
            Start-Process $url
            return
        }
        catch { Start-Sleep -Seconds 2 }
    }
    throw "The site did not respond. Check that it built, and that LocalDB is available."
}

function Reset-Database {
    # The initializer only rebuilds when the *model* changes, so editing the seed scripts alone
    # will not reload them. Dropping the database makes the next request recreate and reseed it.
    Stop-Site
    $sql = @"
if db_id('$Database') is not null
begin
    alter database [$Database] set single_user with rollback immediate;
    drop database [$Database];
end
"@
    & sqlcmd -S $LocalDb -d master -Q $sql -b
    if ($LASTEXITCODE -ne 0) { throw "Could not drop the database." }
    Write-Host "Database dropped. Run './tools/dev.ps1 run' to recreate and reseed it." -ForegroundColor Green
}

switch ($Command) {
    'restore' { Invoke-Restore }
    'build'   { Invoke-Restore; Invoke-Build }
    'test'    { Invoke-Restore; Invoke-Build; Invoke-Test }
    'run'     { Invoke-Restore; Invoke-Build; Start-Site }
    'stop'    { Stop-Site }
    'reseed'  { Reset-Database }
}
