# XekuII Next Generation Framework

**XekuII** 是一套以 YAML 驅動、AI 原生的應用程式框架，建構於 **DevExpress XAF** 與 **.NET 8** 之上。只需定義 YAML 實體檔，執行 Generator，即可一步到位產生完整的 C# 業務物件、REST API 控制器與 DTO。

[English Documentation](README.md)

---

## 架構概覽

```
                                    ┌─── C# XPO 業務物件 (.Generated.cs)
entities/*.xeku.yaml ──→ Generator ─┤
                                    └─── ASP.NET Core API 控制器 (.Generated.cs)
                                              ├── CRUD 端點
                                              ├── Master-Detail 端點
                                              └── 型別安全的 DTO
```

**核心理念**：寫一份 YAML，生成全部。透過 `partial class` 擴展業務邏輯，永遠不動生成的檔案。

---

## 功能特色

- **YAML 驅動開發** — 在 `.xeku.yaml` 檔案中定義實體、欄位、關聯、驗證規則、列舉、計算欄位、預設值與業務規則。
- **智能代碼生成** — 自動產生帶有完整 Attribute 裝飾的 XPO 業務物件、具備 CRUD 與 Detail 端點的 REST API 控制器、以及型別安全的 DTO。
- **自動關聯分析** — 跨實體偵測配對的 reference/detail 關係，自動產生反向關聯並確保 `AssociationName` 正確配對。
- **豐富的驗證支援** — 範圍驗證（`>0`、`>=1`、`0-100`）、最小/最大值、正規表達式、XAF Criteria 表達式、自訂錯誤訊息 — 全部從 YAML 定義。
- **計算欄位** — 支援 PersistentAlias（XPO Criteria 語法）或 getter-only（C# 表達式）的計算屬性。
- **列舉生成** — 實體範圍的列舉型別，包含 `[Description]` 與 `[XafDisplayName]` 屬性，兼顧 AI 理解與 UI 顯示。
- **AI 原生設計** — 所有實體、欄位、關聯、列舉皆包含 `[Description]` 屬性，使 API 對 AI 代理自我說明。
- **企業級安全** — XAF 安全系統搭配 JWT Bearer 認證、角色型存取控制（RBAC）及物件層級權限。

---

## 專案結構

```
XekuII/
├── entities/                        # YAML 實體定義（輸入）
├── XekuII.Generator/               # 代碼生成引擎
│   ├── Models/                     #   EntityDefinition, FieldDefinition 等模型
│   ├── Parsers/                    #   YamlEntityParser
│   └── Generators/                 #   CSharpCodeGenerator, ControllerCodeGenerator
├── XekuII.ApiHost/                  # ASP.NET Core 後端（XAF + Web API）
│   ├── BusinessObjects/            #   生成的 XPO 實體 + 手動 partial class
│   ├── API/                        #   生成的控制器 + Security 控制器
│   └── DatabaseUpdate/             #   種子資料與遷移
├── XekuII.CLI/                      # Generator 的 CLI 包裝
├── .agent/                          # AI 代理技能指南與工作流程
│   ├── SKILL.md                    #   核心開發參考手冊
│   └── workflows/                  #   步驟式工作流程指南
└── CLAUDE.md                        # AI 助手專案指示
```

---

## 系統需求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（ApiHost）/ [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（Generator、CLI）
- [DevExpress Universal Subscription](https://www.devexpress.com/) v25.2.3（XAF、XPO）
- SQL Server LocalDB（預設）或任何 SQL Server 實例
- 文字編輯器或 IDE（VS Code、Visual Studio、Rider）

---

## 快速開始

### 1. 定義實體

建立 `entities/Customer.xeku.yaml`：

```yaml
entity: Customer
caption: 客戶
icon: BO_Customer
description: Customer contact information management

fields:
  - name: Name
    type: string
    required: true
    label: 客戶名稱
    description: Full name of the customer

  - name: Email
    type: string
    label: 電子郵件
    validations:
      - regex: "^[\\w.-]+@[\\w.-]+\\.\\w+$"
        message: "電子郵件格式不正確"

  - name: Phone
    type: string
    label: 電話號碼

  - name: IsActive
    type: bool
    label: 啟用
    default: "true"
    description: Whether the customer account is active

  - name: CreditLimit
    type: decimal
    label: 信用額度
    default: "0"
    validations:
      - range: ">=0"
        message: "信用額度不可為負數"

relations:
  - name: Category
    type: reference
    target: CustomerCategory
    label: 客戶分類
    description: Customer classification

  - name: Orders
    type: detail
    target: Order
    label: 訂單
    description: Orders placed by this customer
```

### 2. 執行代碼生成

```powershell
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API
```

生成產物：
- `XekuII.ApiHost/BusinessObjects/Customer.Generated.cs` — XPO 業務物件
- `XekuII.ApiHost/API/CustomersController.Generated.cs` — REST API 控制器 + DTO

### 3. 更新資料庫

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```

### 4. 啟動應用程式

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj
```

- **API 位址**：http://localhost:5000
- **Swagger UI**：http://localhost:5000/swagger

---

## YAML Schema 參考

### 實體層級屬性

| 屬性 | 型別 | 必填 | 說明 |
|------|------|------|------|
| `entity` | string | 是 | 實體名稱（對應 C# 類別名，PascalCase） |
| `caption` | string | 否 | UI 顯示名稱（`[XafDisplayName]`） |
| `icon` | string | 否 | XAF 圖示名稱（`[ImageName]`） |
| `dbTable` | string | 否 | 自訂資料庫表格名稱 |
| `description` | string | 否 | AI 可讀的描述（`[Description]`） |
| `fields` | list | 是 | 欄位定義清單 |
| `relations` | list | 否 | 關聯定義清單 |
| `rules` | list | 否 | 業務規則觸發器 |
| `enums` | list | 否 | 列舉型別定義 |

### 欄位定義

| 屬性 | 型別 | 必填 | 說明 |
|------|------|------|------|
| `name` | string | 是 | 欄位名稱（PascalCase） |
| `type` | string | 是 | 資料型別（見型別映射表） |
| `required` | bool | 否 | 新增 `[RuleRequiredField]` |
| `readonly` | bool | 否 | `[ModelDefault("AllowEdit", "False")]` |
| `length` | int | 否 | 字串最大長度（`[Size(n)]`） |
| `label` | string | 否 | UI 顯示名稱（`[XafDisplayName]`） |
| `default` | string | 否 | 預設值（在 `AfterConstruction` 中設定） |
| `description` | string | 否 | AI 可讀的描述（`[Description]`） |
| `formula` | string | 否 | 計算欄位表達式 |
| `calculationType` | string | 否 | `persistent`（PersistentAlias）或 `getter`（C# 屬性） |
| `validations` | list | 否 | 驗證規則定義 |

### 型別映射

| YAML 型別 | C# 型別 | 說明 |
|-----------|---------|------|
| `string` | `string` | 字串（預設長度 100） |
| `int` | `int` | 整數 |
| `decimal` | `decimal` | 高精度數值 |
| `double` | `double` | 浮點數 |
| `bool` | `bool` | 布林值 |
| `datetime` | `DateTime` | 日期時間 |
| `guid` | `Guid` | 全域唯一識別碼 |
| `{EnumName}` | `{EnumName}` | 實體範圍的列舉型別 |

### 驗證規則

```yaml
validations:
  - range: ">0"                    # 大於 0
  - range: ">=1"                   # 大於等於 1
  - range: "<=100"                 # 小於等於 100
  - range: "0-100"                 # 介於 0 到 100（含）
  - min: 5                         # 最小值
  - max: 999                       # 最大值
  - regex: "^[A-Z]{2}-\\d{4}$"    # 正規表達式
  - criteria: "[Status] != 'Closed'"  # XAF Criteria 表達式
  - message: "自訂錯誤訊息"          # 自訂錯誤訊息（每條規則）
```

### 關聯定義

| 屬性 | 型別 | 必填 | 說明 |
|------|------|------|------|
| `name` | string | 是 | 關聯屬性名稱 |
| `type` | string | 是 | `reference`（多對一）或 `detail`（一對多，聚合） |
| `target` | string | 是 | 目標實體名稱 |
| `required` | bool | 否 | 新增 `[RuleRequiredField]`（僅 reference） |
| `label` | string | 否 | UI 顯示名稱 |
| `lookupField` | string | 否 | 查詢時顯示的欄位 |
| `cascade` | string | 否 | 級聯行為 |
| `description` | string | 否 | AI 可讀的描述 |

### 列舉定義

```yaml
enums:
  - name: OrderStatus
    description: Order processing lifecycle status
    members:
      - name: Pending
        value: 0
        label: 待處理
        description: Order awaiting processing
      - name: Confirmed
        value: 1
        label: 已確認
      - name: Shipped
        value: 2
        label: 已出貨
      - name: Completed
        value: 3
        label: 已完成
```

### 計算欄位

```yaml
fields:
  # PersistentAlias（XPO Criteria 語法，存入資料庫）
  - name: Total
    type: decimal
    formula: "[Quantity] * [UnitPrice]"
    calculationType: persistent

  # Getter（C# 表達式，執行時計算）
  - name: FullName
    type: string
    formula: "FirstName + \" \" + LastName"
    calculationType: getter
```

### 預設值

```yaml
fields:
  - name: CreatedDate
    type: datetime
    default: "now"           # DateTime.Now（另有 today、utcnow）

  - name: IsActive
    type: bool
    default: "true"

  - name: Status
    type: OrderStatus
    default: "Pending"       # OrderStatus.Pending

  - name: Code
    type: string
    default: "NEW"

  - name: TrackingId
    type: guid
    default: "new"           # Guid.NewGuid()
```

### 業務規則

```yaml
rules:
  - trigger: BeforeSave
    script: ValidateOrder     # 生成：partial void ValidateOrder();
                              # 在 OnSaving() 中被呼叫
```

在非生成檔案中實作 partial method：

```csharp
// BusinessObjects/Order.cs
public partial class Order
{
    partial void ValidateOrder()
    {
        if (Items.Count == 0)
            throw new UserFriendlyException("訂單至少需要一個項目。");
    }
}
```

---

## 生成的 API 端點

以 `Customer` 實體為例，Generator 生成以下端點：

| 方法 | 路由 | 說明 |
|------|------|------|
| `GET` | `/api/customers` | 列出全部（含 reference 的 ID 與名稱） |
| `GET` | `/api/customers/{id}` | 依 ID 取得 |
| `GET` | `/api/customers/{id}/details` | 取得完整關聯資料 |
| `POST` | `/api/customers` | 建立 |
| `PUT` | `/api/customers/{id}` | 更新 |
| `DELETE` | `/api/customers/{id}` | 刪除 |

對每個 `detail` 關聯（例如 `Orders`）：

| 方法 | 路由 | 說明 |
|------|------|------|
| `GET` | `/api/customers/{id}/orders` | 列出明細項目 |
| `POST` | `/api/customers/{id}/orders` | 新增明細項目 |
| `PUT` | `/api/customers/{id}/orders/{itemId}` | 更新明細項目 |
| `DELETE` | `/api/customers/{id}/orders/{itemId}` | 刪除明細項目 |

所有端點皆需 JWT Bearer 認證（`[Authorize]`）。

---

## 認證方式

```powershell
# 取得 JWT Token
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/Authentication/Authenticate" `
  -Method Post -ContentType "application/json" `
  -Body '{"userName":"Admin","password":""}'

$token = $response.token

# 在請求中使用 Token
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/customers" -Headers $headers
```

預設帳號（由 `DatabaseUpdate/Updater.cs` 建立）：
- **Admin**（密碼：空白）— `Administrators` 角色
- **User**（密碼：空白）— `Default` 角色

---

## 透過 Partial Class 客製化

生成的檔案（`*.Generated.cs`）不可手動修改。建立同名類別的獨立檔案來擴展行為：

```csharp
// BusinessObjects/Customer.cs（手動建立，非生成）
public partial class Customer
{
    protected override void OnSaving()
    {
        base.OnSaving();
        // 自訂存檔邏輯
    }

    public string GetFormattedAddress()
    {
        return $"{City}, {Country}";
    }
}
```

---

## 清除與重新生成

重新生成前，移除過期的生成檔案：

```powershell
# 移除生成的業務物件
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue

# 移除生成的控制器
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

# 重新生成
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API
```

---

## 技術棧

| 層級 | 技術 | 版本 |
|------|------|------|
| 框架 | .NET 8.0 / .NET 10.0 | 8.0（ApiHost）/ 10.0（Generator、CLI） |
| ORM | DevExpress XPO | 25.2.3 |
| 應用框架 | DevExpress XAF | 25.2.3 |
| API | ASP.NET Core Web API | 8.0 |
| 認證 | JWT + XAF Security System | — |
| YAML 解析 | YamlDotNet | 16.3.0 |
| API 文件 | Swagger / OpenAPI (Swashbuckle) | 6.9.0 |
| 資料庫 | SQL Server LocalDB | — |

---

## 授權

Private Project. All rights reserved.
