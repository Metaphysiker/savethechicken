# Start only the test environment without running tests
# Usage: .\start-test-env.ps1

Write-Host "🚀 Starting SaveTheChicken test environment..." -ForegroundColor Green

Push-Location "$PSScriptRoot\infrastructure\testing"

Write-Host "📦 Starting Docker services..." -ForegroundColor Cyan
docker-compose up -d

Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "📊 Service status:" -ForegroundColor Cyan
docker-compose ps

Pop-Location

Write-Host ""
Write-Host "✅ Test environment is ready!" -ForegroundColor Green
Write-Host "   Frontend: http://localhost:8080" -ForegroundColor Cyan
Write-Host "   Backend:  http://localhost:8081" -ForegroundColor Cyan
Write-Host "   Database: localhost:5433" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run tests manually:" -ForegroundColor Yellow
Write-Host "   cd playwright" -ForegroundColor Gray
Write-Host "   npm install" -ForegroundColor Gray
Write-Host "   npm test" -ForegroundColor Gray
Write-Host ""
Write-Host "To stop the environment:" -ForegroundColor Yellow
Write-Host "   cd infrastructure\testing" -ForegroundColor Gray
Write-Host "   docker-compose down" -ForegroundColor Gray
