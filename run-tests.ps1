# Run the test environment and execute Playwright tests
# Usage: .\run-tests.ps1

Write-Host "🚀 Starting SaveTheChicken test environment..." -ForegroundColor Green

# Navigate to testing infrastructure
Push-Location "$PSScriptRoot\..\infrastructure\testing"

# Start Docker Compose services
Write-Host "📦 Starting Docker services..." -ForegroundColor Cyan
docker-compose up -d

# Wait for services to be healthy
Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service health
docker-compose ps

# Return to playwright folder
Pop-Location
Push-Location "$PSScriptRoot\playwright"

# Install dependencies if node_modules doesn't exist
if (-not (Test-Path "node_modules")) {
    Write-Host "📥 Installing npm dependencies..." -ForegroundColor Cyan
    npm install
    npx playwright install chromium
}

# Run tests
Write-Host "🧪 Running Playwright tests..." -ForegroundColor Green
npm test

# Store exit code
$TestExitCode = $LASTEXITCODE

# Return to original location
Pop-Location

# Cleanup (optional - comment out if you want to keep environment running)
# Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
# Push-Location "$PSScriptRoot\infrastructure\testing"
# docker-compose down
# Pop-Location

Write-Host ""
if ($TestExitCode -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed. Exit code: $TestExitCode" -ForegroundColor Red
}

exit $TestExitCode
