# PowerShell deployment script for Infomaniak server
# Deploys Docker images and creates a git branch to track deployment history

$ErrorActionPreference = "Stop"

# Get current branch name
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "Current branch: $currentBranch" -ForegroundColor Cyan

# Create deployment tracking branch
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$deploymentBranch = "deployment/$timestamp"
Write-Host "Creating deployment tracking branch: $deploymentBranch" -ForegroundColor Green

git branch $deploymentBranch
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create deployment branch"
    exit 1
}

Write-Host "`nBuilding Docker images..." -ForegroundColor Yellow
docker compose --file docker-compose.build.yml build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed"
    exit 1
}

Write-Host "`nTransferring WebAPI image to server..." -ForegroundColor Yellow
docker save savethechicken-production-webapi | ssh deploy@84.234.19.192 docker load
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to transfer WebAPI image"
    exit 1
}

Write-Host "`nTransferring Web image to server..." -ForegroundColor Yellow
docker save savethechicken-production-web | ssh deploy@84.234.19.192 docker load
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to transfer Web image"
    exit 1
}

Write-Host "`nCopying configuration files to server..." -ForegroundColor Yellow
scp docker-compose.remote.yml deploy@84.234.19.192:/home/deploy/savethechicken
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to copy docker-compose.remote.yml"
    exit 1
}

scp .env deploy@84.234.19.192:/home/deploy/savethechicken
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to copy .env file"
    exit 1
}

scp appsettings.Production.json deploy@84.234.19.192:/home/deploy/savethechicken
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to copy appsettings.Production.json"
    exit 1
}

Write-Host "`nDeploying on remote server..." -ForegroundColor Yellow
$sshCommands = @'
cd /home/deploy/savethechicken
docker compose --file docker-compose.remote.yml down
docker compose --file docker-compose.remote.yml up -d
docker system prune -f
'@

ssh deploy@84.234.19.192 $sshCommands
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to deploy on remote server"
    exit 1
}

Write-Host "`nDeployment completed successfully!" -ForegroundColor Green
Write-Host "Deployment tracked in branch: $deploymentBranch" -ForegroundColor Cyan
Write-Host "Switching back to: $currentBranch" -ForegroundColor Cyan

# Return to original branch
git checkout $currentBranch
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to switch back to $currentBranch - you are still on deployment branch"
    exit 1
}

Write-Host "`nDone! Deployment branch '$deploymentBranch' created for tracking." -ForegroundColor Green
Write-Host "To view deployment history: git branch --list 'deployment/*'" -ForegroundColor Gray
