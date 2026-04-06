# PowerShell deployment script for Infomaniak server
# Deploys Docker images and creates a git branch to track deployment history

$ErrorActionPreference = "Stop"

Write-Host "=== Pre-Deployment Checks ===" -ForegroundColor Cyan

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Warning "You have uncommitted changes:"
    Write-Host $gitStatus -ForegroundColor Yellow
    $response = Read-Host "Continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Deployment cancelled." -ForegroundColor Red
        exit 0
    }
}

# Get current deployment info
$currentBranch = git rev-parse --abbrev-ref HEAD
$commitHash = git rev-parse HEAD
$commitShort = git rev-parse --short HEAD
$commitMessage = git log -1 --pretty=%B

Write-Host "`nCurrent branch: $currentBranch" -ForegroundColor Cyan
Write-Host "Commit: $commitShort - $commitMessage" -ForegroundColor Cyan

# Check what's currently deployed on server
Write-Host "`nChecking current deployment on server..." -ForegroundColor Yellow
$currentDeployment = ssh deploy@84.234.19.192 "cat /home/deploy/savethechicken/deployment.txt 2>/dev/null || echo 'No deployment info found'"
if ($currentDeployment -ne "No deployment info found") {
    Write-Host "Currently deployed:" -ForegroundColor Yellow
    Write-Host $currentDeployment -ForegroundColor Gray
} else {
    Write-Host "No previous deployment info found on server" -ForegroundColor Gray
}

# Create deployment tracking branch and tag
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$deploymentBranch = "deployment/$timestamp"
$deploymentTag = "deploy-$timestamp"

Write-Host "`nCreating deployment tracking:" -ForegroundColor Green
Write-Host "  Branch: $deploymentBranch" -ForegroundColor Gray
Write-Host "  Tag: $deploymentTag" -ForegroundColor Gray

git branch $deploymentBranch
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create deployment branch"
    exit 1
}

git tag -a $deploymentTag -m "Deployment at $timestamp from $currentBranch ($commitShort)"
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to create deployment tag (continuing anyway)"
}

# Create deployment info file
$deploymentInfo = @"
Deployment Date: $timestamp
Branch: $currentBranch
Commit: $commitHash
Commit Short: $commitShort
Message: $commitMessage
Deployed By: $env:USERNAME
Deployed From: $env:COMPUTERNAME
"@

Set-Content -Path "deployment.txt" -Value $deploymentInfo
Write-Host "`nDeployment info created" -ForegroundColor Green

Write-Host "`nBuilding Docker images..." -ForegroundColor Yellow
docker compose --file docker-compose.build.yml build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed"
    exit 1
}

# Create temp directory for image archives
$tempDir = Join-Path $env:TEMP "savethechicken-deploy"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

try {
    Write-Host "`nExporting WebAPI image..." -ForegroundColor Yellow
    $webApiTar = Join-Path $tempDir "webapi.tar"
    docker save savethechicken-production-webapi -o $webApiTar
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to export WebAPI image"
    }

    Write-Host "Transferring WebAPI image to server..." -ForegroundColor Yellow
    scp $webApiTar deploy@84.234.19.192:/tmp/savethechicken-webapi.tar
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to transfer WebAPI image"
    }

    Write-Host "Loading WebAPI image on server..." -ForegroundColor Yellow
    ssh deploy@84.234.19.192 "docker load -i /tmp/savethechicken-webapi.tar && rm /tmp/savethechicken-webapi.tar"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to load WebAPI image on server"
    }

    Write-Host "`nExporting Web image..." -ForegroundColor Yellow
    $webTar = Join-Path $tempDir "web.tar"
    docker save savethechicken-production-web -o $webTar
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to export Web image"
    }

    Write-Host "Transferring Web image to server..." -ForegroundColor Yellow
    scp $webTar deploy@84.234.19.192:/tmp/savethechicken-web.tar
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to transfer Web image"
    }

    Write-Host "Loading Web image on server..." -ForegroundColor Yellow
    ssh deploy@84.234.19.192 "docker load -i /tmp/savethechicken-web.tar && rm /tmp/savethechicken-web.tar"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to load Web image on server"
    }
}
finally {
    # Cleanup temp files
    Write-Host "`nCleaning up temporary files..." -ForegroundColor Gray
    Remove-Item -Recurse -Force $tempDir -ErrorAction SilentlyContinue
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

scp deployment.txt deploy@84.234.19.192:/home/deploy/savethechicken
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to copy deployment.txt"
    exit 1
}

Write-Host "`nDeploying on remote server..." -ForegroundColor Yellow

# Stop and remove old containers
Write-Host "Stopping old containers..." -ForegroundColor Gray
ssh deploy@84.234.19.192 "cd /home/deploy/savethechicken && docker compose --file docker-compose.remote.yml down"
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to stop old containers (they may not exist yet)"
}

# Start new containers
Write-Host "Starting new containers..." -ForegroundColor Gray
ssh deploy@84.234.19.192 "cd /home/deploy/savethechicken && docker compose --file docker-compose.remote.yml up -d"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to start containers on remote server"
    exit 1
}

# Clean up unused Docker resources
Write-Host "Cleaning up unused Docker resources..." -ForegroundColor Gray
ssh deploy@84.234.19.192 "docker system prune -f"

Write-Host "`nDeployment completed successfully!" -ForegroundColor Green
Write-Host "Deployment tracked in:" -ForegroundColor Cyan
Write-Host "  Branch: $deploymentBranch" -ForegroundColor Gray
Write-Host "  Tag: $deploymentTag" -ForegroundColor Gray
Write-Host "  Commit: $commitShort" -ForegroundColor Gray
Write-Host "Switching back to: $currentBranch" -ForegroundColor Cyan

# Cleanup local deployment file
Remove-Item deployment.txt -ErrorAction SilentlyContinue

# Return to original branch
git checkout $currentBranch
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to switch back to $currentBranch - you are still on deployment branch"
    exit 1
}

Write-Host "`nTo check what's deployed on server, run:" -ForegroundColor Gray
Write-Host "  ssh deploy@84.234.19.192 cat /home/deploy/savethechicken/deployment.txt" -ForegroundColor DarkGray
Write-Host "`nTo view deployment history:" -ForegroundColor Gray
Write-Host "  git branch --list 'deployment/*'" -ForegroundColor DarkGray
Write-Host "  git tag --list 'deploy-*'" -ForegroundColor DarkGray
