# Run Docker cleanup on Infomaniak server
# This script connects via SSH and runs the cleanup script

param(
    [string]$ServerHost = "rettet-das-huhn.ch",
    [string]$ServerUser = "deploy"
)

Write-Host "=================================================="
Write-Host "Docker Cleanup - Infomaniak Server"
Write-Host "=================================================="
Write-Host ""

$scriptPath = Join-Path $PSScriptRoot "cleanup-docker.sh"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: cleanup-docker.sh not found" -ForegroundColor Red
    exit 1
}

Write-Host "Uploading cleanup script..." -ForegroundColor Cyan
scp $scriptPath "${ServerUser}@${ServerHost}:/tmp/cleanup-docker.sh"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to upload script" -ForegroundColor Red
    exit 1
}

Write-Host "Running cleanup on server..." -ForegroundColor Yellow
Write-Host ""

ssh -t "${ServerUser}@${ServerHost}" "chmod +x /tmp/cleanup-docker.sh && /tmp/cleanup-docker.sh && rm /tmp/cleanup-docker.sh"

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
