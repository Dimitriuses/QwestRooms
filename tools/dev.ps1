<#
.SYNOPSIS
    Developer entry point for QwestRooms: build, test, run, reseed and capture screenshots.

.DESCRIPTION
    One definition of how this repository builds and runs, callable from a terminal, from VS Code
    tasks and from CI. Written for Windows PowerShell 5.1 and PowerShell 7 alike -- no &&, no
    ternary, no null-coalescing -- so it works on Windows out of the box and on Linux and macOS
    under pwsh.

.EXAMPLE
    ./tools/dev.ps1 build
    ./tools/dev.ps1 test
    ./tools/dev.ps1 run
    ./tools/dev.ps1 reseed
    ./tools/dev.ps1 screenshots
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('restore', 'build', 'test', 'run', 'reseed', 'screenshots')]
    [string]$Command,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [int]$Port = 5188
)

$ErrorActionPreference = 'Stop'

# $PSScriptRoot is empty while PowerShell 5.1 evaluates param() defaults, so these are filled in
# here rather than in the parameter list.
$RepoRoot   = Split-Path -Parent $PSScriptRoot
$Solution   = Join-Path $RepoRoot 'QwestRooms.sln'
$WebProject = Join-Path $RepoRoot 'src/QwestRooms.UI/QwestRooms.UI.csproj'
$Database   = Join-Path $RepoRoot 'src/QwestRooms.UI/qwestrooms.db'

function Invoke-Dotnet {
    param([string[]]$DotnetArgs)

    Write-Host "dotnet $($DotnetArgs -join ' ')" -ForegroundColor Cyan
    & dotnet @DotnetArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($DotnetArgs -join ' ') failed with exit code $LASTEXITCODE." }
}

function Invoke-Restore {
    Invoke-Dotnet @('restore', $Solution)
}

function Invoke-Build {
    Invoke-Dotnet @('build', $Solution, '-c', $Configuration, '--nologo')
}

function Invoke-Test {
    Invoke-Dotnet @('test', $Solution, '-c', $Configuration, '--nologo')
}

function Start-Site {
    Write-Host "Starting http://localhost:$Port/ -- the first run creates and seeds the database." -ForegroundColor Cyan
    Invoke-Dotnet @('run', '--project', $WebProject, '-c', $Configuration, '--urls', "http://localhost:$Port")
}

function Reset-Database {
    # The seed loads only into an empty catalogue, so deleting the file is how you reload it after
    # editing MockData/*.sql or re-running generate-seed.ps1.
    foreach ($suffix in @('', '-shm', '-wal')) {
        $path = $Database + $suffix
        if (Test-Path $path) {
            Remove-Item $path -Force
            Write-Host "Deleted $path" -ForegroundColor Yellow
        }
    }

    Write-Host "Run './tools/dev.ps1 run' to recreate and reseed it." -ForegroundColor Green
}

function Invoke-Screenshots {
    $script = Join-Path $PSScriptRoot 'capture-screenshots.js'
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        throw "node was not found on PATH; it is needed to drive the browser that takes the screenshots."
    }

    & node $script --port $Port
    if ($LASTEXITCODE -ne 0) { throw "Screenshot capture failed." }
}

switch ($Command) {
    'restore'     { Invoke-Restore }
    'build'       { Invoke-Build }
    'test'        { Invoke-Test }
    'run'         { Start-Site }
    'reseed'      { Reset-Database }
    'screenshots' { Invoke-Screenshots }
}
