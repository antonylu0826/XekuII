---
description: 清理 XekuII 專案的生成代碼、暫存檔與建置產物
---

# XekuII 專案清理工作流程

## 場景說明

在以下情況需要執行清理：

- **重新生成前**：避免已刪除/重新命名的實體殘留舊的 Generated 檔案
- **實體重構後**：移除不再需要的 Generated 代碼
- **完整重置**：清除所有生成物與建置產物，回到乾淨狀態

---

## 層級一：清除生成的代碼（最常用）

重新生成前執行此層級即可。

```powershell
# 移除生成的 Business Objects
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue

# 移除生成的 API Controllers
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue
```

驗證清除結果：

```powershell
# 確認無殘留
Get-ChildItem -Path "XekuII.ApiHost/BusinessObjects" -Filter "*.Generated.cs"
Get-ChildItem -Path "XekuII.ApiHost/API" -Filter "*Controller.Generated.cs"
```

---

## 層級二：清除實體定義

移除所有 YAML 實體定義檔（回到空白狀態）。

```powershell
# 移除所有實體定義
Remove-Item -Path "entities/*.xeku.yaml" -Force -ErrorAction SilentlyContinue

# 遞迴搜尋子目錄（若有）
Get-ChildItem -Path "entities" -Include "*.xeku.yaml" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
```

> **警告**：此操作不可逆。請確認已備份需要的 YAML 定義。

---

## 層級三：清除暫存檔與測試腳本

移除開發過程中產生的暫存檔案。

```powershell
# 測試腳本
Remove-Item -Path "OrderSystemTest.ps1" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "RuntimeTest.ps1" -Force -ErrorAction SilentlyContinue

# 日誌與暫存
Remove-Item -Path "db_update.log" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "start_debug.log" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "temp.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "build_error.log" -Force -ErrorAction SilentlyContinue
```

---

## 層級四：清除建置產物

移除 .NET 建置產物（bin/obj 目錄）。

```powershell
# 清除所有專案的 bin/obj
Get-ChildItem -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
```

驗證：

```powershell
# 確認已清除
Get-ChildItem -Include bin,obj -Recurse -Directory
```

---

## 完整重置（所有層級）

一次執行所有清理步驟：

```powershell
Write-Host "=== XekuII Full Cleanup ===" -ForegroundColor Cyan

# 層級一：生成的代碼
Write-Host "Removing generated code..." -ForegroundColor Yellow
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path "XekuII.ApiHost/API" -Include "*Controller.Generated.cs" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue

# 層級二：實體定義（選用，取消註解以啟用）
# Write-Host "Removing entity definitions..." -ForegroundColor Yellow
# Remove-Item -Path "entities/*.xeku.yaml" -Force -ErrorAction SilentlyContinue

# 層級三：暫存檔
Write-Host "Removing temp files..." -ForegroundColor Yellow
Remove-Item -Path "OrderSystemTest.ps1","RuntimeTest.ps1","db_update.log","start_debug.log","temp.cs","build_error.log" -Force -ErrorAction SilentlyContinue

# 層級四：建置產物
Write-Host "Removing build artifacts..." -ForegroundColor Yellow
Get-ChildItem -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cleanup complete." -ForegroundColor Green
```

---

## 清理後的下一步

清理完成後，典型的接續操作：

```powershell
# 重新生成（若有 YAML 定義）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API

# 還原套件（若清除了 bin/obj）
dotnet restore

# 建置
dotnet build

# 更新資料庫
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```
