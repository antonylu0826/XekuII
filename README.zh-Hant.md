# XekuII Next Generation Framework

XekuII 是一個基於 **DevExpress XAF** 與 **.NET 8** 的現代化、AI 原生網頁應用程式框架。它透過 Yaml 定義檔驅動與自動化代碼生成技術，簡化了後端開發流程，讓開發者能專注於核心業務邏輯，而將繁瑣的 boilerplate 交由框架處理。

[English Documentation](README.md)

## 主要功能

- **YAML 驅動開發**: 使用簡潔易讀的 YAML 檔案定義資料模型 (Entity)、驗證規則與關聯性。
- **自動化代碼生成**: `XekuII.Generator` 工具能根據您的 YAML 定義，自動生成強型別的 C# 業務物件 (XPO) 與 ASP.NET Core Web API 控制器。
- **AI 就緒架構**: 專為整合人工智慧而設計，促進智慧化應用程式的開發。
- **現代化技術堆疊**: 建立在最新的 .NET 8 與 DevExpress XAF Blazor/WebAPI 技術之上。

## 專案結構

- **`XekuII.ApiHost`**: 主要的 ASP.NET Core 後端應用程式，託管 XAF 安全系統、Web API 與業務物件。
- **`XekuII.Generator`**: 負責解析 YAML 定義並生成 C# 代碼的命令列工具 (CLI)。
- **`XekuII.CLI`**: (選用) 用於管理 XekuII 環境的命令列介面。
- **`entities/`**: 建議存放 `.xeku.yaml` 實體定義檔的目錄。

## 系統需求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [DevExpress Universal Subscription](https://www.devexpress.com/) (XAF 與 XPO 需授權)
- 文字編輯器或 IDE (VS Code, Visual Studio 2022)

## 快速開始

### 1. 定義實體 (Define Entities)

在 `entities/` 目錄中建立一個 `.xeku.yaml` 檔案 (例如 `entities/Customer.xeku.yaml`)：

```yaml
entity: Customer
icon: BO_Customer
fields:
  - name: Name
    type: string
    required: true
  - name: Email
    type: string
```

### 2. 生成代碼 (Generate Code)

在專案根目錄下執行 Generator：

```powershell
# 語法: dotnet run --project XekuII.Generator -- <實體目錄> [選項]
dotnet run --project XekuII.Generator -- ./entities --output ./XekuII.ApiHost/BusinessObjects --controllers ./XekuII.ApiHost/API
```

### 3. 執行應用程式 (Run the Application)

啟動後端伺服器：

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj
```

API 服務將於 `http://localhost:5000` (或 `launchSettings.json` 設定的連接埠) 啟動。

## 資料庫更新

若您修改了資料庫結構 (Schema)，可能需要執行資料庫更新指令：

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj -- --updateDatabase --forceUpdate --silent
```

## 授權

Private Project. All rights reserved.
