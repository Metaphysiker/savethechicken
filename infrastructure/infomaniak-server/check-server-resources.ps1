# Check server resources for SaveTheChicken production environment
# This script connects to the Infomaniak server via SSH and runs resource checks

param(
    [string]$ServerHost = "rettet-das-huhn.ch",
    [string]$ServerUser = "deploy"
)

Write-Host "=================================================="
Write-Host "Connecting to $ServerHost as $ServerUser"
Write-Host "=================================================="
Write-Host ""

# Upload and execute the resource check script
$scriptPath = Join-Path $PSScriptRoot "check-server-resources.sh"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: check-server-resources.sh not found" -ForegroundColor Red
    exit 1
}

# Copy script to server and execute
Write-Host "Uploading resource check script..." -ForegroundColor Cyan
scp $scriptPath "${ServerUser}@${ServerHost}:/tmp/check-server-resources.sh"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to upload script to server" -ForegroundColor Red
    exit 1
}

Write-Host "Running resource check on server..." -ForegroundColor Cyan
Write-Host ""

ssh "${ServerUser}@${ServerHost}" "chmod +x /tmp/check-server-resources.sh && /tmp/check-server-resources.sh && rm /tmp/check-server-resources.sh"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error: Failed to execute resource check" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=================================================="
Write-Host "Would you like to add Docker resource limits?"
Write-Host "=================================================="
Write-Host ""
Write-Host "Your docker-compose.remote.yml currently has NO resource limits."
Write-Host "This means containers can use unlimited CPU and memory."
Write-Host ""
Write-Host "To add limits, edit: infrastructure/infomaniak-server/docker-compose.remote.yml"
Write-Host ""
Write-Host "Example resource limits:"
Write-Host "  webapi:"
Write-Host "    deploy:"
Write-Host "      resources:"
Write-Host "        limits:"
Write-Host "          cpus: '1.0'"
Write-Host "          memory: 512M"
Write-Host "        reservations:"
Write-Host "          memory: 256M"
Write-Host ""
