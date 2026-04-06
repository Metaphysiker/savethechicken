# PowerShell script to check what version is currently deployed on the server

Write-Host "Checking current deployment on Infomaniak server..." -ForegroundColor Cyan
Write-Host ""

$deploymentInfo = ssh deploy@84.234.19.192 "cat /home/deploy/savethechicken/deployment.txt 2>/dev/null"

if ($LASTEXITCODE -eq 0 -and $deploymentInfo) {
    Write-Host "Currently Deployed:" -ForegroundColor Green
    Write-Host $deploymentInfo -ForegroundColor Gray

    # Parse and highlight key info
    $lines = $deploymentInfo -split "`n"
    Write-Host "`nQuick Summary:" -ForegroundColor Cyan
    foreach ($line in $lines) {
        if ($line -match "Commit Short: (.+)") {
            Write-Host "  Commit: $($matches[1])" -ForegroundColor Yellow
        }
        if ($line -match "Deployment Date: (.+)") {
            Write-Host "  Deployed: $($matches[1])" -ForegroundColor Yellow
        }
        if ($line -match "Branch: (.+)") {
            Write-Host "  From Branch: $($matches[1])" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "No deployment info found on server" -ForegroundColor Red
    Write-Host "This might be the first deployment, or the server doesn't have deployment tracking yet." -ForegroundColor Gray
}
