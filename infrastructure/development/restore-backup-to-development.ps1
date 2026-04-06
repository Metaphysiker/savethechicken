# PowerShell script to restore a database backup to the development environment
# Usage: .\restore-backup-to-development.ps1 [PathToDumpFile]
# If no path is provided, it will use the most recent backup from the default backup folder

param(
    [Parameter(Mandatory=$false)]
    [string]$PathToDumpFile
)

$ErrorActionPreference = "Stop"

$defaultBackupFolder = "C:\Users\sraes\savethechicken-backups"
$containerName = "savethechicken-development-postgres-1"

# If no dump file is provided, find the most recent one
if ([string]::IsNullOrEmpty($PathToDumpFile)) {
    Write-Host "No dump file specified. Looking for the most recent backup in: $defaultBackupFolder" -ForegroundColor Yellow
    
    if (-not (Test-Path $defaultBackupFolder)) {
        Write-Error "Default backup folder does not exist: $defaultBackupFolder"
        exit 1
    }
    
    $latestBackup = Get-ChildItem -Path $defaultBackupFolder -Filter "dump_*.dump" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($null -eq $latestBackup) {
        Write-Error "No backup files found in: $defaultBackupFolder"
        exit 1
    }
    
    $PathToDumpFile = $latestBackup.FullName
    Write-Host "Found most recent backup: $($latestBackup.Name)" -ForegroundColor Cyan
    Write-Host "Created: $($latestBackup.LastWriteTime)" -ForegroundColor Gray
}

# Validate the dump file exists
if (-not (Test-Path $PathToDumpFile)) {
    Write-Error "The specified dump file does not exist: $PathToDumpFile"
    exit 1
}

Write-Host "`nRestoring backup to development database..." -ForegroundColor Cyan
Write-Host "Dump file: $PathToDumpFile" -ForegroundColor Gray
Write-Host "Container: $containerName" -ForegroundColor Gray

# Check if the container is running
Write-Host "`nChecking if Docker container is running..." -ForegroundColor Yellow
$containerStatus = docker ps --filter "name=$containerName" --format "{{.Names}}"
if ($containerStatus -ne $containerName) {
    Write-Error "Container '$containerName' is not running. Please start the development environment first with 'docker compose up' in the infrastructure/development folder."
    exit 1
}
Write-Host "Container is running." -ForegroundColor Green

# Copy the dump file to the container
Write-Host "`nCopying dump file to container..." -ForegroundColor Yellow
docker cp $PathToDumpFile ${containerName}:/dump.dump
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to copy dump file to container"
    exit 1
}
Write-Host "Dump file copied successfully." -ForegroundColor Green

# Restore the dump
Write-Host "`nRestoring database..." -ForegroundColor Yellow
Write-Host "Note: You may see some warnings about existing objects being dropped - this is normal." -ForegroundColor Gray
docker exec -it $containerName pg_restore -U savethechicken -c -d savethechicken --no-owner --role=savethechicken /dump.dump

if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 1) {
    # Exit code 1 can occur with warnings but successful restore
    Write-Host "`nDatabase restore completed!" -ForegroundColor Green
    Write-Host "Your development database has been updated with the backup data." -ForegroundColor Cyan
} else {
    Write-Error "Database restore failed with exit code: $LASTEXITCODE"
    exit 1
}

# Clean up the dump file from the container
Write-Host "`nCleaning up..." -ForegroundColor Yellow
docker exec $containerName rm /dump.dump
Write-Host "Done!" -ForegroundColor Green
