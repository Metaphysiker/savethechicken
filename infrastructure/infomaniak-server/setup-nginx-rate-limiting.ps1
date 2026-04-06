# Setup nginx rate limiting on Infomaniak server

param(
    [string]$ServerHost = "rettet-das-huhn.ch",
    [string]$ServerUser = "deploy"
)

Write-Host "=================================================="
Write-Host "Setup Nginx Rate Limiting"
Write-Host "=================================================="
Write-Host ""

# Upload helper script
$scriptPath = Join-Path $PSScriptRoot "setup-nginx-rate-limiting.sh"
$configPath = Join-Path $PSScriptRoot "nginx-rate-limiting-config.conf"

if (-not (Test-Path $scriptPath)) {
    Write-Host "Error: setup-nginx-rate-limiting.sh not found" -ForegroundColor Red
    exit 1
}

Write-Host "Uploading scripts to server..." -ForegroundColor Cyan
scp $scriptPath "${ServerUser}@${ServerHost}:/tmp/setup-nginx-rate-limiting.sh" 2>$null
scp $configPath "${ServerUser}@${ServerHost}:/tmp/nginx-rate-limiting-config.conf" 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to upload scripts" -ForegroundColor Red
    exit 1
}

Write-Host "Running setup helper..." -ForegroundColor Cyan
Write-Host ""

ssh -t "${ServerUser}@${ServerHost}" "chmod +x /tmp/setup-nginx-rate-limiting.sh && /tmp/setup-nginx-rate-limiting.sh"

Write-Host ""
Write-Host "=================================================="
Write-Host "IMPORTANT: Manual Configuration Required"
Write-Host "=================================================="
Write-Host ""
Write-Host "The helper script has created a backup and checked your config." -ForegroundColor Yellow
Write-Host "Now you need to manually edit the nginx config on the server:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. SSH into the server:" -ForegroundColor White
Write-Host "   ssh $ServerUser@$ServerHost"
Write-Host ""
Write-Host "2. View the example config:" -ForegroundColor White
Write-Host "   cat /tmp/nginx-rate-limiting-config.conf"
Write-Host ""
Write-Host "3. Edit your nginx config:" -ForegroundColor White
Write-Host "   sudo nano /etc/nginx/sites-available/default"
Write-Host ""
Write-Host "4. Test and reload:" -ForegroundColor White
Write-Host "   sudo nginx -t"
Write-Host "   sudo systemctl reload nginx"
Write-Host ""
Write-Host "Reference file: infrastructure/infomaniak-server/nginx-rate-limiting-config.conf"
Write-Host ""
