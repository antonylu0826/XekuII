# XekuII 開發技能手冊

> AI 代理在操作 XekuII 專案時的完整參考。涵蓋 YAML 驅動架構、代碼生成、擴展機制與疑難排解。

---

## 1. 核心開發哲學

XekuII 遵循 **YAML 驅動開發（YAML-Driven Development）** 模式：

```
定義 YAML → 執行 Generator → 使用生成的代碼 → 透過 partial class 擴展
```

**關鍵原則：**
- `*.Generated.cs` 檔案由 Generator 產出，**嚴禁手動修改**
- 所有客製化邏輯透過 `partial class` 在獨立檔案中實作
- YAML 是唯一的真相來源（Single Source of Truth）
- 重新生成前務必清除舊的 Generated 檔案

---

## 2. YAML 實體定義規範

請參閱專屬技能文件：**[Entity Generation Standards](skills/ENTITY_GENERATION.md)**

該文件包含：
- **命名規範**：標準駝峰式命名 (Standard CamelCase) 與縮寫詞處理
- **實體結構**：Fields, Relations, Enums, Rules, UI 設定
- **欄位型別**：C# / TypeScript / Zod 型別映射表
- **驗證與預設值**：Validation Rules 與 Default Values 語法
- **計算欄位**：PersistentAlias 與 Getter 處理
- **關聯類型**：Reference 與 Detail 的最佳實踐

> **核心提示**：定義欄位名稱時，請優先使用 `Sku`, `PoNumber` 等首字母大寫的標準命名，避免使用 `SKU`, `PONumber` 等全大寫命名，以確保前後端資料綁定的一致性。

---

## 3. 常用指令

### 3.1 代碼生成

```powershell
# 完整生成（BO + API Controller）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API

# 完整生成（BO + API Controller + 前端）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API `
  --frontend ./xekuii-web/src/generated

# 僅生成 BO（不含 Controller 或前端）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects
```

參數說明：
- 第一個位置參數：實體 YAML 目錄路徑
- `--output`：BO 輸出目錄
- `--controllers`：Controller 輸出目錄（省略則不生成）
- `--frontend`：前端 generated 輸出目錄（省略則不生成，指向 `xekuii-web/src/generated`）
- `--namespace`：目標命名空間（預設 `XekuII.ApiHost.BusinessObjects`）

### 3.2 資料庫更新

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```

### 3.3 服務管理（XekuII CLI）

使用 CLI 統一管理前後端服務，類似 .NET Aspire 的服務編排：

```powershell
# 啟動所有服務
dotnet run --project XekuII.CLI -- start

# 僅啟動後端 API
dotnet run --project XekuII.CLI -- start --backend
dotnet run --project XekuII.CLI -- start -b

# 僅啟動前端 Web
dotnet run --project XekuII.CLI -- start --frontend
dotnet run --project XekuII.CLI -- start -f

# 停止所有服務
dotnet run --project XekuII.CLI -- stop

# 停止指定服務
dotnet run --project XekuII.CLI -- stop --backend
dotnet run --project XekuII.CLI -- stop --frontend

# 查詢服務狀態
dotnet run --project XekuII.CLI -- status
```

**服務端點：**
| 服務 | 端口 | URL |
|------|------|-----|
| Backend API | 5000 | http://localhost:5000 |
| Swagger UI | 5000 | http://localhost:5000/swagger |
| Frontend Web | 5173 | http://localhost:5173 |

**狀態說明：**
| 狀態 | 意義 |
|------|------|
| Stopped | 服務未運行 |
| Starting | 服務啟動中 |
| Running | 服務正常運行 |
| Unhealthy | 進程存在但健康檢查失敗 |
| Error | 啟動失敗或異常終止 |

### 3.4 手動啟動（無 CLI）

若不使用 CLI，可手動分別啟動：

```powershell
# 後端
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj

# 前端（另一個終端）
cd xekuii-web
npm run dev
```

### 3.5 清除生成檔案

```powershell
# 清除後端 BO
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue

# 清除後端 Controller
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

# 清除前端生成檔案
Remove-Item -Path "xekuii-web/src/generated/types/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/schemas/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/api/*.generated.ts" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/pages" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/routes.generated.tsx" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "xekuii-web/src/generated/navigation.generated.ts" -Force -ErrorAction SilentlyContinue

# 清除 .NET 建置產物
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
```

### 3.6 XekuII CLI（代碼生成）

```powershell
# 使用 CLI 生成
XekuII.CLI generate --entities=./entities --output=./XekuII.ApiHost/BusinessObjects --api --api-output=./XekuII.ApiHost/API

# 位置參數形式
XekuII.CLI generate ./entities ./XekuII.ApiHost/BusinessObjects XekuII.ApiHost.BusinessObjects
```

---

## 4. 生成產物說明

### 4.1 產生器內部架構 (Generator Architecture)

為了保持代碼的可維護性，React 相關產生器採用 **Facade 模式**與**組件化設計**：

- **核心整合器**: `ReactPageGenerator` (Facade) 負責協調並調用子產生器。
- **專門子產生器**:
    - `ReactListPageGenerator`: 負責列表頁面 (`*ListPage.generated.tsx`)。
    - `ReactFormPageGenerator`: 負責表單、即時計算與關聯存檔頁面 (`*FormPage.generated.tsx`)。
    - `ReactDetailPageGenerator`: 負責詳情頁面 (`*DetailPage.generated.tsx`)。
    - `ReactMetadataGenerator`: 負責全局路由 (`routes.generated.tsx`) 與導航選單 (`navigation.generated.ts`)。
- **共用工具**: `Utilities/ReactGeneratorUtils.cs` 封裝了欄位映射、圖示轉換與欄位解析邏輯。

> **提示**: 若需優化特定頁面邏輯，應直接修改對應的子產生器檔案。

### 4.2 Business Object（`*.Generated.cs`）

每個實體生成一個 `partial class`，包含：
- XPO `BaseObject` 繼承與建構子
- 所有欄位的 backing field + property（含 `SetPropertyValue` 通知機制）
- `[Description]`、`[XafDisplayName]`、`[RuleRequiredField]` 等 Attribute
- 驗證規則 Attribute（`[RuleValueComparison]`、`[RuleRange]`、`[RuleRegularExpression]`、`[RuleCriteria]`）
- Reference 關聯（`[Association]`）
- Detail 集合（`[Association, Aggregated]` + `XPCollection`）
- 自動生成的反向關聯
- 列舉型別（在類別外、同命名空間中）
- `AfterConstruction()` 預設值覆寫
- `OnSaving()` 業務規則呼叫
- Partial method 宣告（供手動實作）

### 4.2 API Controller（`*Controller.Generated.cs`）

每個實體生成一個 Controller + 相關 DTO，包含：

**端點：**
| 方法 | 路由 | 說明 |
|------|------|------|
| GET | `/api/{entities}` | 列出全部（含 reference ID/Name） |
| GET | `/api/{entities}/{id}` | 依 ID 取得 |
| GET | `/api/{entities}/{id}/details` | 完整細節（含關聯物件） |
| POST | `/api/{entities}` | 建立（使用 DTO） |
| PUT | `/api/{entities}/{id}` | 更新（使用 DTO） |
| DELETE | `/api/{entities}/{id}` | 刪除 |

**Detail 端點（每個 detail 關聯一組）：**
| 方法 | 路由 | 說明 |
|------|------|------|
| GET | `/api/{entities}/{id}/{detailName}` | 列出明細 |
| POST | `/api/{entities}/{id}/{detailName}` | 新增明細 |
| PUT | `/api/{entities}/{id}/{detailName}/{itemId}` | 更新明細 |
| DELETE | `/api/{entities}/{id}/{detailName}/{itemId}` | 刪除明細 |

**DTO：**
- `{Entity}Dto` — 建立/更新用（排除計算欄位與唯讀欄位）
- `{Entity}DetailsDto` — 回應用（含所有欄位 + 關聯物件）
- `{Entity}{RefTarget}RefDto` — Reference 物件摘要（Oid + Name）
- `{Entity}{DetailTarget}ItemDto` — Detail 項目摘要
- `{Entity}Add{DetailTarget}Request` — 新增明細請求
- `{Entity}Update{DetailTarget}Request` — 更新明細請求

### 4.3 前端生成產物

使用 `--frontend` 參數時，每個實體會生成以下前端檔案（放置於指定的前端 generated 目錄）：

| 檔案 | 目錄 | 說明 |
|------|------|------|
| `{kebab}.types.generated.ts` | `types/` | TypeScript 介面：主介面、Ref DTO、Details 介面、Create/Update DTO、列舉 + Labels |
| `{kebab}.schema.generated.ts` | `schemas/` | Zod 驗證 schema：create schema（含驗證鏈）、update schema（partial）、推斷型別 |
| `{kebab}.api.generated.ts` | `api/` | 型別安全 API 客戶端：getAll、getById、getDetails、create、update、delete + detail 端點 |
| `{Entity}ListPage.generated.tsx` | `pages/{kebab}/` | 列表頁：TanStack Table、搜尋、排序、刪除確認、行操作按鈕（View/Edit/Delete） |
| `{Entity}FormPage.generated.tsx` | `pages/{kebab}/` | 表單頁：React Hook Form + Zod 驗證、建立/編輯雙模式、ReferenceSelect |
| `{Entity}DetailPage.generated.tsx` | `pages/{kebab}/` | 詳情頁：欄位顯示、分區佈局、Detail 關聯表格（含 reference 選擇）、編輯/刪除操作 |

另外生成跨實體的共用檔案：

| 檔案 | 說明 |
|------|------|
| `routes.generated.tsx` | 路由定義：列表、新增、詳情、編輯路由（含 detail 子實體） |
| `navigation.generated.ts` | 導航選單項目：label、path、icon（自動排除 detail 子實體） |

**導航選單排除規則**：

被其他實體透過 `type: detail` 關聯的子實體（如 `OrderLine`）不會出現在側邊導航選單。這些實體的頁面仍存在（透過 URL 可直接存取），但使用者應從父實體的詳情頁管理這些子項目。

> **注意**：`*.generated.*` 檔案由 Generator 完全管理，嚴禁手動修改。前端客製化應在 `xekuii-web/src/components/` 或 `xekuii-web/src/pages/` 中實作。

---

## 5. 擴展機制：Partial Class

### 5.1 擴展業務邏輯

```csharp
// BusinessObjects/Order.cs（手動建立）
public partial class Order
{
    // 實作 Generator 宣告的 partial method
    partial void ValidateOrder()
    {
        if (Items.Count == 0)
            throw new UserFriendlyException("訂單至少需要一個項目。");
    }

    // 自訂方法
    public void CalculateDiscount(decimal rate)
    {
        Discount = TotalAmount * rate;
    }

    // 覆寫生命週期方法
    protected override void OnSaving()
    {
        base.OnSaving();  // 重要：保留 Generator 生成的 BeforeSave 邏輯
        ModifiedDate = DateTime.Now;
    }
}
```

### 5.2 注意事項

- 如果 Generator 已生成 `OnSaving()` override（因為 YAML 有 `BeforeSave` 規則），手動覆寫時必須呼叫 `base.OnSaving()` 以保留自動邏輯
- Partial method 的簽名由 Generator 宣告，手動檔案只需提供實作
- 命名空間必須與 Generator 輸出一致（預設 `XekuII.ApiHost.BusinessObjects`）

---

## 6. 認證與安全

### 6.1 JWT 認證

```powershell
# 取得 Token
$body = '{"userName":"Admin","password":""}'
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/Authentication/Authenticate" `
  -Method Post -ContentType "application/json" -Body $body

# 使用 Token
$headers = @{ Authorization = "Bearer $($response.token)" }
Invoke-RestMethod -Uri "http://localhost:5000/api/customers" -Headers $headers
```

### 6.2 預設帳號

| 帳號 | 密碼 | 角色 | 權限 |
|------|------|------|------|
| Admin | （空白） | Administrators | 完全存取 |
| User | （空白） | Default | 受限存取 |

---

## 7. 疑難排解

| 問題 | 原因 | 解決方式 |
|------|------|----------|
| 資料庫更新失敗 | 欄位型別變更導致 SQL 衝突 | 使用 `--forceUpdate`，或手動刪除資料庫重建 |
| Swagger 未顯示 Controller | Controller 命名空間或路由設定錯誤 | 確認 Controller 繼承 `ControllerBase` 且有 `[Route("api/[controller]")]` |
| 生成檔案殘留 | 重命名/刪除實體後舊檔案未清除 | 重新生成前執行清除指令（見 3.4） |
| 反向關聯衝突 | 兩個實體對同一目標有相同屬性名 | Generator 的 `AnalyzeRelationships` 會避免重複，若仍衝突請檢查 YAML 命名 |
| 列舉型別找不到 | 欄位 type 與 enum name 大小寫不一致 | enum name 使用 PascalCase，欄位 type 完全對應 |
| 建置錯誤：找不到型別 | 目標實體尚未定義 YAML | 確保所有被 reference/detail 引用的實體都有對應的 `.xeku.yaml` |

---

## 8. 開發工作流程摘要

```
1. 建立/修改 entities/*.xeku.yaml
2. 清除舊的 Generated 檔案（後端 + 前端，避免殘留）
3. 執行 Generator（BO + Controller + Frontend）
4. 更新資料庫（若結構有變）
5. 建置後端驗證編譯
6. 建置前端驗證編譯（npm run build）
7. 啟動應用程式並驗證
8. 如需客製化：
   - 後端：建立 partial class 擴展
   - 前端：在 src/components/ 或 src/pages/ 中實作
```

**詳細步驟請參考 `.agent/workflows/` 目錄下的工作流程文件。**
