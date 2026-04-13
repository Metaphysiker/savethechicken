# PowerShell script to create a backup from Infomaniak production server
# Usage: .\create-infomaniak-backup.ps1 [-BackupPath C:\path\to\backups]
# NOTE: This script checks if a backup already exists for today.
#       If yes, it exits without downloading a duplicate.
#       This allows running it every hour without creating unnecessary backups.
#       Backups are kept indefinitely - manually delete old backups as needed.
#
# This script wraps create-backup-from-infomaniak-server.ps1 with a "check if exists" layer.

param(
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "C:\Users\sraes\savethechicken-backups"
)

$ErrorActionPreference = "Stop"

# Create automatic-backups subfolder
$automaticBackupPath = Join-Path $BackupPath "automatic-backups"
if (-not (Test-Path $automaticBackupPath)) {
    Write-Host "Creating automatic backup directory: $automaticBackupPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $automaticBackupPath -Force | Out-Null
}

# Check if a backup already exists for today
$today = Get-Date -Format "yyyy-MM-dd"
$existingBackup = Get-ChildItem -Path $automaticBackupPath -Filter "dump_$today*.dump" -ErrorAction SilentlyContinue | Select-Object -First 1

if ($existingBackup) {
    Write-Host "Backup already exists for today: $($existingBackup.Name)" -ForegroundColor Green
    Write-Host "  Created: $($existingBackup.LastWriteTime)" -ForegroundColor Gray
    Write-Host "  Size: $([math]::Round($existingBackup.Length / 1MB, 2)) MB" -ForegroundColor Gray
    Write-Host "`nNo backup needed. Exiting." -ForegroundColor Cyan
    exit 0
}

# No backup exists for today - create one using the main backup script
Write-Host "No backup found for today. Creating new backup..." -ForegroundColor Yellow
$backupScriptPath = Join-Path $PSScriptRoot "create-backup-from-infomaniak-server.ps1"

if (-not (Test-Path $backupScriptPath)) {
    Write-Error "Backup script not found: $backupScriptPath"
    exit 1
}

& $backupScriptPath -PathToSaveDump $automaticBackupPath
