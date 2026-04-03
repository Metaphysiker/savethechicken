# Restore Farms from JSON Backup
# Usage: .\restore-farms-from-json.ps1 -JsonFile "path\to\json-betriebe.json" -ApiUrl "https://your-api-url" -Token "your-jwt-token"

param(
    [Parameter(Mandatory=$true)]
    [string]$JsonFile,
    
    [Parameter(Mandatory=$true)]
    [string]$ApiUrl,
    
    [Parameter(Mandatory=$true)]
    [string]$Token
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Farm Restoration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if file exists
if (-not (Test-Path $JsonFile)) {
    Write-Host "ERROR: JSON file not found: $JsonFile" -ForegroundColor Red
    exit 1
}

# Read JSON file
Write-Host "Reading JSON file: $JsonFile" -ForegroundColor Yellow
$jsonContent = Get-Content $JsonFile -Raw | ConvertFrom-Json

$farms = $jsonContent.data
$totalFarms = $farms.Count
Write-Host "Found $totalFarms farms to restore" -ForegroundColor Green
Write-Host ""

# Confirm before proceeding
$confirmation = Read-Host "Do you want to proceed with restoration? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Restoration cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Starting restoration..." -ForegroundColor Cyan
Write-Host ""

$headers = @{
    "Authorization" = "Bearer $Token"
    "Content-Type" = "application/json"
}

$successCount = 0
$errorCount = 0
$errors = @()

foreach ($farm in $farms) {
    $farmName = $farm.name
    Write-Host "Processing: $farmName" -ForegroundColor White
    
    try {
        # Prepare the DTO for API (without read-only fields like id, createdAt, updatedAt, genericName)
        $farmDto = @{
            name = $farm.name
            numberOfChickens = $farm.numberOfChickens
            numberOfRoosters = $farm.numberOfRoosters
            size = $farm.size
            color = $farm.color
            generalInformation = $farm.generalInformation
            datesForRescues = $farm.datesForRescues
            saveChickenActionId = $farm.saveChickenActionId
            contactId = 0  # Will be created from nested object
            addressId = 0  # Will be created from nested object
            contact = @{
                firstName = $farm.contact.firstName
                lastName = $farm.contact.lastName
                phoneNumber = $farm.contact.phoneNumber
                email = $farm.contact.email
                carMake = $farm.contact.carMake
                categories = $farm.contact.categories
                availableDates = $farm.contact.availableDates
            }
            address = @{
                street = $farm.address.street
                city = $farm.address.city
                postalCode = $farm.address.postalCode
                geoCoordinate = $farm.address.geoCoordinate
            }
        }
        
        $jsonBody = $farmDto | ConvertTo-Json -Depth 10
        
        # POST to API
        $apiEndpoint = "$ApiUrl/api/farm"
        $response = Invoke-RestMethod -Uri $apiEndpoint -Method Post -Headers $headers -Body $jsonBody -ErrorAction Stop
        
        Write-Host "  ✓ Success (New ID: $($response.id))" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "  ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
        $errors += @{
            Farm = $farmName
            Error = $_.Exception.Message
        }
    }
    
    # Small delay to avoid overwhelming the API
    Start-Sleep -Milliseconds 100
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Restoration Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total farms:    $totalFarms" -ForegroundColor White
Write-Host "Successful:     $successCount" -ForegroundColor Green
Write-Host "Failed:         $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "White" })
Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "Errors:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  - $($error.Farm): $($error.Error)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "Restoration script finished." -ForegroundColor Cyan
