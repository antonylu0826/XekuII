---
description: 執行 XekuII 代碼生成器的完整工作流程
---

# XekuII 代碼生成工作流程

## 前置檢查

確認環境就緒：

```powershell
# 檢查 .NET SDK
dotnet --version

# 確認 entities 目錄存在且有 YAML 檔案
Get-ChildItem -Path entities -Filter "*.xeku.yaml" -Recurse
```

---

## 步驟一：清除舊的生成檔案

**重要**：每次重新生成前都應清除，避免殘留的 Generated 檔案導致衝突。

```powershell
# 清除生成的業務物件
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue

# 清除生成的 API 控制器
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

# 清除生成的前端檔案（若使用 --frontend）
Remove-Item -Path "xekuii-web/src/generated/types/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/schemas/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/api/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/pages" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/routes.generated.tsx" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/navigation.generated.ts" -Force -ErrorAction SilentlyContinue
```

---

## 步驟二：執行 Generator

### 使用 XekuII.Generator 專案（推薦）

```powershell
# 完整生成：BO + API Controller
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API

# 完整生成：BO + API Controller + 前端
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API `
  --frontend ./xekuii-web/src/generated

# 僅生成 BO（不含 Controller 或前端）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects

# 指定命名空間
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API `
  --namespace XekuII.ApiHost.BusinessObjects
```

### 使用 XekuII.CLI

```powershell
# 完整生成
XekuII.CLI generate --entities=./entities `
  --output=./XekuII.ApiHost/BusinessObjects `
  --api --api-output=./XekuII.ApiHost/API

# 位置參數形式
XekuII.CLI generate ./entities ./XekuII.ApiHost/BusinessObjects XekuII.ApiHost.BusinessObjects
```

### Generator 參數說明

| 參數 | 說明 | 預設值 |
|------|------|--------|
| 第一個位置參數 | 實體 YAML 目錄 | （必填） |
| `--output` | BO 輸出目錄 | `../XekuII.ApiHost/BusinessObjects` |
| `--controllers` | Controller 輸出目錄（省略則不生成） | （無） |
| `--frontend` | 前端 generated 輸出目錄（省略則不生成） | （無） |
| `--namespace` | 目標命名空間 | `XekuII.ApiHost.BusinessObjects` |

### 預期輸出

```
🚀 Running XekuII Generator
=================
📂 Entities:     ./entities
📁 BO Output:   ./XekuII.ApiHost/BusinessObjects
📁 Controllers: ./XekuII.ApiHost/API
🌐 Frontend:    ./xekuii-web/src/generated
📦 Namespace:   XekuII.ApiHost.BusinessObjects

🔍 Scanning entities from: ./entities
📁 Output directory: ./XekuII.ApiHost/BusinessObjects
🌐 Frontend output: ./xekuii-web/src/generated

✅Generated BO: Product.Generated.cs
✅Generated API: ProductsController.Generated.cs
  🌐 types/product.types.generated.ts
  🌐 schemas/product.schema.generated.ts
  🌐 api/product.api.generated.ts
  🌐 pages/product/ProductListPage.generated.tsx
  🌐 pages/product/ProductFormPage.generated.tsx
  🌐 pages/product/ProductDetailPage.generated.tsx
✅Generated: routes.generated.tsx
✅Generated: navigation.generated.ts

📊 Total: 1 entities generated.
```

---

## 步驟三：驗證生成結果

```powershell
# 檢查生成的 BO 檔案
Get-ChildItem -Path "XekuII.ApiHost/BusinessObjects" -Filter "*.Generated.cs"

# 檢查生成的 Controller 檔案
Get-ChildItem -Path "XekuII.ApiHost/API" -Filter "*Controller.Generated.cs"

# 檢查生成的前端檔案（若使用 --frontend）
Get-ChildItem -Path "xekuii-web/src/generated" -Recurse -Filter "*.generated.*"

# 建置後端確認無編譯錯誤
dotnet build XekuII.ApiHost/XekuII.ApiHost.csproj

# 建置前端確認無型別錯誤（若使用 --frontend）
cd xekuii-web && npm run build && cd ..
```

---

## 步驟四：更新資料庫

當實體結構有變更時（新增/修改欄位、新增實體），需更新資料庫 Schema：

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```

| 參數 | 說明 |
|------|------|
| `--updateDatabase` | 啟動資料庫更新模式 |
| `--forceUpdate` | 強制更新（即使有衝突也嘗試） |
| `--silent` | 靜默模式（不顯示確認提示） |

---

## 步驟五：啟動並測試

```powershell
# 啟動應用程式
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj

# 在另一個終端測試 API
$body = '{"userName":"Admin","password":""}'
$auth = Invoke-RestMethod -Uri "http://localhost:5000/api/Authentication/Authenticate" `
  -Method Post -ContentType "application/json" -Body $body

$headers = @{ Authorization = "Bearer $($auth.token)" }
Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Headers $headers
```

或直接開啟 Swagger UI 進行測試：`http://localhost:5000/swagger`

---

## 完整一鍵流程

將以上步驟合併為單一指令序列：

### 後端 only

```powershell
# 清除 → 生成 → 建置 → 更新資料庫
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API

dotnet build XekuII.ApiHost/XekuII.ApiHost.csproj

dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```

### 全端（後端 + 前端）

```powershell
# 清除後端
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

# 清除前端
Remove-Item -Path "xekuii-web/src/generated/types/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/schemas/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/api/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/pages" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/routes.generated.tsx" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/navigation.generated.ts" -Force -ErrorAction SilentlyContinue

# 生成全部
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API `
  --frontend ./xekuii-web/src/generated

# 建置驗證
dotnet build XekuII.ApiHost/XekuII.ApiHost.csproj
cd xekuii-web && npm run build && cd ..

# 更新資料庫
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```

---

## 疑難排解

| 問題 | 解決方式 |
|------|----------|
| `Entities directory not found` | 確認 entities 目錄路徑正確，且包含 `.xeku.yaml` 檔案 |
| 建置錯誤：找不到型別 | 確保所有被引用的目標實體都有對應的 YAML 定義 |
| Controller 路由衝突 | 檢查是否有手動建立的 Controller 與生成的路由重複 |
| 資料庫更新失敗 | 嘗試 `--forceUpdate`，或刪除 LocalDB 資料庫後重建 |
| 命名空間不符 | 使用 `--namespace` 參數指定正確的命名空間 |
