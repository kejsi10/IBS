#!/usr/bin/env powershell
<#
.SYNOPSIS
    Installs the Playwright Chromium browser required for PDF generation.

.DESCRIPTION
    Builds the API project (which pulls in all runtime DLLs including Playwright)
    then invokes the Playwright browser install via DLL reflection — works with
    both Windows PowerShell 5.x and pwsh 7+.

.PARAMETER Configuration
    Build configuration (Debug or Release). Defaults to Debug.
#>
param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$TargetFramework = "net8.0"
$ApiProject    = "src/IBS.Api/IBS.Api.csproj"
$BinDir        = "src/IBS.Api/bin/$Configuration/$TargetFramework"
$PlaywrightDll = Join-Path $BinDir "Microsoft.Playwright.dll"

Write-Host "Building API project to resolve all runtime dependencies..." -ForegroundColor Cyan
dotnet build $ApiProject -c $Configuration --nologo -q

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Fix compilation errors and try again."
    exit 1
}

if (-not (Test-Path $PlaywrightDll)) {
    Write-Error "Microsoft.Playwright.dll not found at: $PlaywrightDll`nMake sure the build completed successfully."
    exit 1
}

Write-Host "Installing Playwright Chromium browser..." -ForegroundColor Cyan

# Set PLAYWRIGHT_DRIVER_SEARCH_PATH so Playwright locates its native driver
$Env:PLAYWRIGHT_DRIVER_SEARCH_PATH = (Resolve-Path $BinDir).Path

# Load the assembly and invoke Main via reflection — compatible with PowerShell 5.x and 7+
$absoluteDllPath = (Resolve-Path $PlaywrightDll).Path
$assembly = [System.Reflection.Assembly]::LoadFrom($absoluteDllPath)
$type = $assembly.GetType("Microsoft.Playwright.Program")

if ($null -eq $type) {
    Write-Error "Could not find Microsoft.Playwright.Program type in the DLL."
    exit 1
}

$exitCode = $type::Main([string[]]@("install", "chromium"))

if ($exitCode -ne 0) {
    Write-Error "Playwright browser installation failed (exit code: $exitCode)."
    exit $exitCode
}

Write-Host "Playwright Chromium installed successfully." -ForegroundColor Green
Write-Host "You can now generate PDF documents via the Documents API." -ForegroundColor Green
