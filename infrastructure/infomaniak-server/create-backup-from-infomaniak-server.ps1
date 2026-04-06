# PowerShell script to create a backup from Infomaniak server
# Usage: .\create-backup-from-infomaniak-server.ps1 C:\path\to\save\backup

param(
    [Parameter(Mandatory=$true)]
    [string]$PathToSaveDump
)

$ErrorActionPreference = "Stop"

# Validate the local path exists
if (-not (Test-Path $PathToSaveDump)) {
    Write-Error "The specified path does not exist: $PathToSaveDump"
    exit 1
}

# Create timestamped backup name
$timestamp = Get-Date -Format "yyyy-MM-dd_HH_mm_ss"
$backupName = "dump_$timestamp.dump"

Write-Host "Creating database backup: $backupName" -ForegroundColor Cyan
Write-Host "Saving to: $PathToSaveDump" -ForegroundColor Cyan

# SSH commands to execute on remote server (using semicolons to avoid line ending issues)
$sshCommands = "cd /home/deploy/savethechicken; docker exec savethechicken-production-postgres-1 bash -c 'pg_dump -Fc -U savethechicken savethechicken > db.dump'; docker cp savethechicken-production-postgres-1:/db.dump $backupName; mkdir -p /home/deploy/backups/savethechicken; mv $backupName /home/deploy/backups/savethechicken"

Write-Host "`nExecuting backup on remote server..." -ForegroundColor Yellow
ssh deploy@84.234.19.192 $sshCommands
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create backup on remote server"
    exit 1
}

Write-Host "`nDownloading backup to local machine..." -ForegroundColor Yellow
$localBackupPath = Join-Path $PathToSaveDump $backupName
scp "deploy@84.234.19.192:/home/deploy/backups/savethechicken/$backupName" $localBackupPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to download backup file"
    exit 1
}

Write-Host "`nBackup completed successfully!" -ForegroundColor Green
Write-Host "Backup saved to: $localBackupPath" -ForegroundColor Cyan
Write-Host "Backup size: $((Get-Item $localBackupPath).Length / 1MB) MB" -ForegroundColor Gray
