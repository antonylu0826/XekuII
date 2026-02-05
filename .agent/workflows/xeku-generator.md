---
description: 執行 XekuII 代碼生成器 (修復版)
---
1. 確保已安裝 .NET 8 SDK
// turbo
dotnet --version

2. 執行生成器 (預設路徑)
// turbo
dotnet run --project XekuII.Generator -- ./entities --output ./XekuII.ApiHost/BusinessObjects --controllers ./XekuII.ApiHost/API
