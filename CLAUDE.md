# XekuII Next Gen 開發指南

> 此文件為 AI 助手在開啟新對話時提供關鍵專案資訊與常用指令。

## 專案資訊
- **名稱**: XekuII Next Gen
- **框架**: .NET 8, DevExpress XAF (Blazor/WebAPI)
- **資料庫**: LocalDB (`XekuII.ApiHost`) / InMemory (測試用)
- **架構原則**: YAML 驅動開發 (Entity Definitions) -> 自動化代碼生成 (Generator)

## 常用指令

### 1. 產生程式碼 (Code Generation)
執行 Generator 以將 `entities/*.xeku.yaml` 轉換為 C# 代碼：
```powershell
dotnet run --project XekuII.Generator -- ./entities --output ./XekuII.ApiHost/BusinessObjects --controllers ./XekuII.ApiHost/API
```

### 2. 建置與執行 (Build & Run)
啟動後端 API 伺服器：
```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj
```
- API Swagger: http://localhost:5000/swagger

### 3. 資料庫更新 (Database Update)
當實體結構變更時，執行此指令更新資料庫 Schema：
```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj -- --updateDatabase --forceUpdate --silent
```

### 4. 測試 (Testing)
- **單元/整合測試**: (尚無專用專案，目前使用 PowerShell 腳本)
- **Runtime 驗證**: 執行 `RuntimeTest.ps1` 進行端對端 API 測試。
```powershell
powershell -ExecutionPolicy Bypass -File .\RuntimeTest.ps1
```

### 5. 清理專案 (Cleanup)
移除所有生成代碼與暫存檔 (需搭配技能使用):
- 刪除 `entities/*.xeku.yaml`
- 刪除 `XekuII.ApiHost/BusinessObjects/*.Generated.cs`
- 刪除 `XekuII.ApiHost/API/*.Generated.cs`

## 主要目錄結構
- `XekuII.ApiHost/`: 後端主程式 (XAF Security, Web API)
  - `BusinessObjects/`: 生成的 XPO 實體 (partial class)
  - `API/`: 生成的 Controller
- `XekuII.Generator/`: 代碼生成工具 (Console App)
- `entities/`: YAML 實體定義檔存放處
