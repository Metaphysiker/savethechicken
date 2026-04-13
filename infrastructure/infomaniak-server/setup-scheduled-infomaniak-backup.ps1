# PowerShell script to setup Windows Task Scheduler for automatic Infomaniak backups
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "C:\Users\sraes\savethechicken-backups"
)

$ErrorActionPreference = "Stop"

# Check if running as Administrator
$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator. Right-click PowerShell and select 'Run as Administrator'"
    exit 1
}

$taskName = "SaveTheChicken-InfomaniakBackup"
$scriptPath = Join-Path $PSScriptRoot "create-infomaniak-backup.ps1"

# Verify script exists
if (-not (Test-Path $scriptPath)) {
    Write-Error "Backup script not found at: $scriptPath"
    exit 1
}

Write-Host "Setting up scheduled task: $taskName" -ForegroundColor Cyan
Write-Host "Frequency: Every hour" -ForegroundColor Gray
Write-Host "Script: $scriptPath" -ForegroundColor Gray
Write-Host "Backup path: $BackupPath" -ForegroundColor Gray
Write-Host "`nNote: Script will only create ONE backup per day (skips if backup already exists)" -ForegroundColor Yellow

# Remove existing task if it exists
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Host "`nRemoving existing scheduled task..." -ForegroundColor Yellow
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

# Create the action
$action = New-ScheduledTaskAction `
    -Execute "PowerShell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`" -BackupPath `"$BackupPath`"" `
    -WorkingDirectory $PSScriptRoot

# Create the trigger - run every hour (indefinitely)
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours 1) -RepetitionDuration (New-TimeSpan -Days 9999)

# Create settings
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Hours 1)

# Create the principal (run as current user, only when logged on)
$principal = New-ScheduledTaskPrincipal `
    -UserId $env:USERNAME `
    -LogonType Interactive `
    -RunLevel Highest

# Register the task
Register-ScheduledTask `
    -TaskName $taskName `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -Principal $principal `
    -Description "Automatically backs up SaveTheChicken production database from Infomaniak server"

Write-Host "`nScheduled task created successfully!" -ForegroundColor Green
Write-Host "`nTask details:" -ForegroundColor Cyan
Write-Host "  Name: $taskName"
Write-Host "  Schedule: Every hour (starting now)"
Write-Host "  Script: $scriptPath"
Write-Host "  Backup directory: $BackupPath\automatic-backups"
Write-Host "  Logic: Only creates ONE backup per day (first run each day)"

Write-Host "`nYou can:" -ForegroundColor Yellow
Write-Host "  - View the task: taskschd.msc (Task Scheduler GUI)"
Write-Host "  - Run manually: Get-ScheduledTask -TaskName '$taskName' | Start-ScheduledTask"
Write-Host "  - Check status: Get-ScheduledTask -TaskName '$taskName' | Get-ScheduledTaskInfo"
Write-Host "  - Remove task: Unregister-ScheduledTask -TaskName '$taskName' -Confirm:`$false"

Write-Host "`nHow it works:" -ForegroundColor Cyan
Write-Host "  - Task runs every hour"
Write-Host "  - Checks if a backup was already created today"
Write-Host "  - If yes: exits immediately (no backup created)"
Write-Host "  - If no: downloads backup from Infomaniak server"
Write-Host "`nThis ensures you get ONE backup per day, whenever your PC is running!" -ForegroundColor Green
Write-Host "`nIMPORTANT: SSH access to deploy@84.234.19.192 must be configured (no password prompt)!" -ForegroundColor Yellow
