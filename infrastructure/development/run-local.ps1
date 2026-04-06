# Start SaveTheChicken Development Environment
Write-Host "Starting SaveTheChicken Development Environment..." -ForegroundColor Green

# Get the root directory (two levels up from infrastructure/development)
$rootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

# Track spawned processes
$script:spawnedProcesses = @()

# Function to stop a process and all its child processes
function Stop-ProcessTree {
    param([int]$targetPid)  # renamed to avoid conflict with built-in $pid
    $children = Get-CimInstance Win32_Process | Where-Object { $_.ParentProcessId -eq $targetPid }
    foreach ($child in $children) { Stop-ProcessTree -targetPid $child.ProcessId }
    try { Stop-Process -Id $targetPid -Force -ErrorAction SilentlyContinue } catch {}
}

# Function to cleanup spawned processes
function Stop-Servers {
    Write-Host "`nStopping servers..." -ForegroundColor Yellow
    foreach ($proc in $script:spawnedProcesses) {
        if ($proc -and !$proc.HasExited) {
            Stop-ProcessTree -targetPid $proc.Id
        }
    }
    Write-Host "Servers stopped." -ForegroundColor Green
}

# Register cleanup on script exit
Register-EngineEvent PowerShell.Exiting -Action { Stop-Servers } | Out-Null

# Start WebApi in a new window
Write-Host "Starting WebApi on http://localhost:8081..." -ForegroundColor Cyan
$webApiProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$rootDir\WebApi'; dotnet watch run --launch-profile http --urls http://localhost:8081" -PassThru
$script:spawnedProcesses += $webApiProcess

# Wait a bit for WebApi to start
Start-Sleep -Seconds 3

# Start Web frontend in a new window
Write-Host "Starting Web Frontend on http://localhost:8080..." -ForegroundColor Cyan
$frontendProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$rootDir\MauiBlazorWeb\MauiBlazorWeb.Web'; dotnet watch run --launch-profile http --urls http://localhost:8080" -PassThru
$script:spawnedProcesses += $frontendProcess

Write-Host ""
Write-Host "Development servers starting..." -ForegroundColor Green
Write-Host "WebApi:   http://localhost:8081" -ForegroundColor Yellow
Write-Host "Frontend: http://localhost:8080" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key or close this window to stop servers..." -ForegroundColor Gray

# Wait for key press
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Stop the servers when key is pressed
Stop-Servers
