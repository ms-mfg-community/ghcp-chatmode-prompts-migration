#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the App Modernization Wizard — installs prerequisites, configures
    GitHub OAuth, and starts the application.

.DESCRIPTION
    This script automates the first-time setup:
    1. Checks prerequisites (.NET 10 SDK, GitHub CLI, Copilot CLI)
    2. Guides you through GitHub OAuth App registration (or skips if already done)
    3. Stores Client ID/Secret in dotnet user-secrets
    4. Optionally authenticates the Copilot CLI
    5. Starts the application

.EXAMPLE
    ./setup.ps1
    ./setup.ps1 -SkipOAuth    # Skip OAuth setup, use PAT only
    ./setup.ps1 -StartOnly    # Just start the app (already configured)
#>

param(
    [switch]$SkipOAuth,
    [switch]$StartOnly,
    [switch]$Help
)

$ErrorActionPreference = "Stop"
$projectPath = Join-Path $PSScriptRoot "src" "AppModernization.Web"

# --- Helpers ---
function Write-Step($step, $message) {
    Write-Host "`n[$step] $message" -ForegroundColor Cyan
}

function Write-Ok($message) {
    Write-Host "  ✅ $message" -ForegroundColor Green
}

function Write-Warn($message) {
    Write-Host "  ⚠️  $message" -ForegroundColor Yellow
}

function Write-Fail($message) {
    Write-Host "  ❌ $message" -ForegroundColor Red
}

function Test-Command($cmd) {
    try { Get-Command $cmd -ErrorAction SilentlyContinue | Out-Null; return $true }
    catch { return $false }
}

# --- Help ---
if ($Help) {
    Get-Help $MyInvocation.MyCommand.Path -Detailed
    exit 0
}

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║    🚀 App Modernization Wizard — Setup          ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Magenta

# --- Start Only ---
if ($StartOnly) {
    Write-Step "1/1" "Starting the application..."
    Push-Location $projectPath
    dotnet run
    Pop-Location
    exit 0
}

# ============================================================
# Step 1: Check Prerequisites
# ============================================================
Write-Step "1/5" "Checking prerequisites..."

# .NET SDK
if (Test-Command "dotnet") {
    $dotnetVersion = (dotnet --version 2>$null)
    Write-Ok ".NET SDK $dotnetVersion"
} else {
    Write-Fail ".NET SDK not found. Install from: https://dot.net/download"
    exit 1
}

# GitHub CLI
if (Test-Command "gh") {
    $ghVersion = (gh --version 2>$null | Select-Object -First 1)
    Write-Ok "GitHub CLI: $ghVersion"

    # Check if logged in
    $ghStatus = gh auth status 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Ok "GitHub CLI authenticated"
    } else {
        Write-Warn "GitHub CLI not authenticated. Run: gh auth login"
    }
} else {
    Write-Warn "GitHub CLI not found (optional but recommended). Install from: https://cli.github.com"
}

# Copilot CLI
if (Test-Command "copilot") {
    Write-Ok "Copilot CLI found"
    $copilotAuth = copilot auth status 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Ok "Copilot CLI authenticated"
    } else {
        Write-Warn "Copilot CLI not authenticated"
        $authNow = Read-Host "  Authenticate Copilot CLI now? (y/N)"
        if ($authNow -eq 'y') {
            copilot auth login
        }
    }
} else {
    Write-Warn "Copilot CLI not found. Install from: https://docs.github.com/en/copilot/how-tos/set-up/install-copilot-cli"
    Write-Warn "The app will start but Copilot chat features won't work until the CLI is installed."
}

# ============================================================
# Step 2: Restore .NET packages
# ============================================================
Write-Step "2/5" "Restoring .NET packages..."
Push-Location $projectPath
dotnet restore --quiet
if ($LASTEXITCODE -eq 0) { Write-Ok "Packages restored" }
else { Write-Fail "Package restore failed"; Pop-Location; exit 1 }
Pop-Location

# ============================================================
# Step 3: Initialize user-secrets
# ============================================================
Write-Step "3/5" "Configuring user secrets..."
Push-Location $projectPath

# Init secrets if not already
dotnet user-secrets init 2>$null

# Check if OAuth is already configured
$existingClientId = dotnet user-secrets list 2>$null | Select-String "GitHub:ClientId"
if ($existingClientId -and -not $SkipOAuth) {
    Write-Ok "GitHub OAuth already configured"
    $reconfigure = Read-Host "  Reconfigure? (y/N)"
    if ($reconfigure -ne 'y') {
        $SkipOAuth = $true
    }
}

Pop-Location

# ============================================================
# Step 4: GitHub OAuth Setup
# ============================================================
if (-not $SkipOAuth) {
    Write-Step "4/5" "GitHub OAuth App setup..."
    Write-Host ""
    Write-Host "  You need a GitHub OAuth App (one-time setup)." -ForegroundColor White
    Write-Host "  This lets users click 'Sign in with GitHub' instead of managing PATs." -ForegroundColor Gray
    Write-Host ""
    Write-Host "  ┌─────────────────────────────────────────────────────┐" -ForegroundColor DarkGray
    Write-Host "  │  1. Open: https://github.com/settings/developers   │" -ForegroundColor DarkGray
    Write-Host "  │  2. Click 'New OAuth App'                          │" -ForegroundColor DarkGray
    Write-Host "  │  3. Fill in:                                       │" -ForegroundColor DarkGray
    Write-Host "  │     App name:     App Modernization Wizard         │" -ForegroundColor DarkGray
    Write-Host "  │     Homepage:     https://localhost:7292            │" -ForegroundColor DarkGray
    Write-Host "  │     Callback URL: https://localhost:7292/signin-github │" -ForegroundColor DarkGray
    Write-Host "  │  4. Click 'Register application'                   │" -ForegroundColor DarkGray
    Write-Host "  │  5. Copy the Client ID                             │" -ForegroundColor DarkGray
    Write-Host "  │  6. Click 'Generate a new client secret'           │" -ForegroundColor DarkGray
    Write-Host "  │  7. Copy the Client Secret                         │" -ForegroundColor DarkGray
    Write-Host "  └─────────────────────────────────────────────────────┘" -ForegroundColor DarkGray
    Write-Host ""

    # Try to open the browser
    $openBrowser = Read-Host "  Open GitHub developer settings in your browser? (Y/n)"
    if ($openBrowser -ne 'n') {
        Start-Process "https://github.com/settings/developers"
    }

    Write-Host ""
    $clientId = Read-Host "  Paste your Client ID"
    $clientSecret = Read-Host "  Paste your Client Secret"

    if ([string]::IsNullOrWhiteSpace($clientId) -or [string]::IsNullOrWhiteSpace($clientSecret)) {
        Write-Warn "Skipping OAuth — no credentials provided. You can use PAT login instead."
    } else {
        Push-Location $projectPath
        dotnet user-secrets set "GitHub:ClientId" $clientId | Out-Null
        dotnet user-secrets set "GitHub:ClientSecret" $clientSecret | Out-Null
        Pop-Location
        Write-Ok "OAuth credentials saved to user-secrets (not in source code)"
    }
} else {
    Write-Step "4/5" "Skipping OAuth setup (--SkipOAuth or already configured)"
    Write-Ok "You can use PAT login in the app, or run this script again to configure OAuth"
}

# ============================================================
# Step 5: Build and Start
# ============================================================
Write-Step "5/5" "Building and starting the application..."

Push-Location $projectPath
dotnet build --quiet
if ($LASTEXITCODE -ne 0) {
    Write-Fail "Build failed"
    Pop-Location
    exit 1
}
Write-Ok "Build succeeded"

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║    ✅ Setup complete!                            ║" -ForegroundColor Green
Write-Host "║                                                  ║" -ForegroundColor Green
Write-Host "║    The app will start at:                        ║" -ForegroundColor Green
Write-Host "║    https://localhost:7292                        ║" -ForegroundColor Green
Write-Host "║                                                  ║" -ForegroundColor Green
Write-Host "║    Press Ctrl+C to stop the server.              ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

dotnet run
Pop-Location
