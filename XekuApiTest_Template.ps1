# Xeku API 整合測試範本 (PowerShell)
# 使用方法: 修改 $entityName 與 CRUD 內容後執行。

$baseUrl = "http://localhost:5000"
$authUrl = "$baseUrl/api/Authentication/Authenticate"
$entityName = "YourEntity" # 例如: Customers

# --- 1. 登入與取得 Token ---
Write-Host "Authenticating as Admin..." -ForegroundColor Cyan
$loginBody = @{ userName = "Admin"; password = "" } | ConvertTo-Json
$authResponse = Invoke-RestMethod -Uri $authUrl -Method Post -Body $loginBody -ContentType "application/json"
$token = if ($authResponse.token) { $authResponse.token } else { $authResponse }
$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }
Write-Host "Login Successful.`n" -ForegroundColor Green

# --- 2. 建立資料 (POST) ---
Write-Host "--- Step 1: Create $entityName ---" -ForegroundColor Yellow
$createBody = @{
    code = "TEST001"
    name = "Template Test Item"
    # 添加其他欄位...
} | ConvertTo-Json
$item = Invoke-RestMethod -Uri "$baseUrl/api/$entityName" -Method Post -Body $createBody -Headers $headers
$oid = $item.oid
Write-Host "Created $entityName: $oid" -ForegroundColor Green

# --- 3. 讀取資料 (GET) ---
Write-Host "`n--- Step 2: Read $entityName ---" -ForegroundColor Yellow
$readItem = Invoke-RestMethod -Uri "$baseUrl/api/$entityName/$oid" -Method Get -Headers $headers
Write-Host "Read successfully: $($readItem.name)"

# --- 4. 更新資料 (PUT) ---
Write-Host "`n--- Step 3: Update $entityName ---" -ForegroundColor Yellow
$updateBody = @{
    code = "TEST001-UPD"
    name = "Updated Item"
} | ConvertTo-Json
Invoke-RestMethod -Uri "$baseUrl/api/$entityName/$oid" -Method Put -Body $updateBody -Headers $headers
Write-Host "Update successful."

# --- 5. 清理資料 (DELETE) ---
Write-Host "`n--- Step 4: Cleanup ---" -ForegroundColor Yellow
Invoke-RestMethod -Uri "$baseUrl/api/$entityName/$oid" -Method Delete -Headers $headers
Write-Host "Delete successful. Cleanup Complete." -ForegroundColor Green

Write-Host "`n--- Test Completed Successfully! ---" -ForegroundColor Yellow
