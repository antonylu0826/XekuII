# Test-Permissions.ps1
# Usage: .\Test-Permissions.ps1 -UserName "Admin" -Password ""

param (
    [string]$UserName = "Admin",
    [string]$Password = ""
)

$baseUrl = "http://localhost:5000/api"

Write-Host "--- Authenticating as $UserName ---" -ForegroundColor Cyan
$authBody = @{ userName = $UserName; password = $Password } | ConvertTo-Json
try {
    Write-Host "Calling Authenticate endpoint..." -NoNewline
    $authResponse = Invoke-RestMethod -Uri "$baseUrl/Authentication/Authenticate" -Method Post -Headers @{ "Content-Type" = "application/json" } -Body $authBody -TimeoutSec 10
    $token = $authResponse
    Write-Host " Done." -ForegroundColor Green
    
    Write-Host "--- Getting My Permissions ---" -ForegroundColor Cyan
    $headers = @{ 
        "Authorization" = "Bearer $token"
        "Accept" = "application/json"
    }
    Write-Host "Calling my-permissions endpoint..." -NoNewline
    $perms = Invoke-RestMethod -Uri "$baseUrl/Permissions/my-permissions" -Method Get -Headers $headers -TimeoutSec 10
    Write-Host " Done." -ForegroundColor Green
    
    $perms | ConvertTo-Json
} catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.InnerException) {
        Write-Host "Inner Error: $($_.Exception.InnerException.Message)" -ForegroundColor Yellow
    }
}
