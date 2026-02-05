---
description: 清理 XekuII 專案生成的代碼與暫存檔 (修復版)
---
1. 移除實體定義
// turbo
Remove-Item -Path "entities/*.xeku.yaml" -Force -ErrorAction SilentlyContinue

2. 移除生成的 Business Objects
// turbo
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue

3. 移除生成的 API Controllers
// turbo
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

4. 移除生成的 Extended Controllers (若有)
// turbo
Remove-Item -Path "XekuII.ApiHost/API/*ExtendedController.cs" -Force -ErrorAction SilentlyContinue

5. 遞迴移除殘留的控制器
// turbo
Get-ChildItem -Path "XekuII.ApiHost/API" -Include "*Controller.Generated.cs" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue

6. 移除測試腳本與日誌
// turbo
Remove-Item -Path "OrderSystemTest.ps1", "RuntimeTest.ps1", "db_update.log", "start_debug.log", "temp.cs" -Force -ErrorAction SilentlyContinue

7. 清理建置產物
// turbo
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
