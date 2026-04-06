# Check container health on Infomaniak server

param(
    [string]$ServerHost = "rettet-das-huhn.ch",
    [string]$ServerUser = "deploy"
)

Write-Host "=================================================="
Write-Host "Container Health Check - Infomaniak Server"
Write-Host "=================================================="
Write-Host ""

$scriptPath = Join-Path $PSScriptRoot "check-container-health.sh"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: check-container-health.sh not found" -ForegroundColor Red
    exit 1
}

Write-Host "Uploading health check script..." -ForegroundColor Cyan
scp $scriptPath "${ServerUser}@${ServerHost}:/tmp/check-container-health.sh" 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to upload script" -ForegroundColor Red
    exit 1
}

Write-Host "Running health check..." -ForegroundColor Cyan
Write-Host ""

ssh "${ServerUser}@${ServerHost}" "chmod +x /tmp/check-container-health.sh && /tmp/check-container-health.sh && rm /tmp/check-container-health.sh"

Write-Host ""
Write-Host "=================================================="
Write-Host "Run this regularly to catch issues early!"
Write-Host "=================================================="
