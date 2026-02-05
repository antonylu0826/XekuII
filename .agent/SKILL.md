# XekuII Next Gen 技能開發手冊 (SKILL.md)

> 此文件定義了 XekuII 專案的核心開發技能、YAML 驅動架構及其自動化工作流。

## 1. 核心開發哲學：YAML 驅動 (YAML-Driven Development)

XekuII 的開發遵循「模型先行」原則：
1. **定義**: 在 `entities/` 目錄下撰寫 YAML 實體定義。
2. **生成**: 透過 `XekuII.Generator` 自動產生 Business Objects (XPO) 與 Web API Controllers。
3. **擴充**: 透過 `partial class` 在非生成檔案中擴充業務邏輯、動作 (Action) 或自定義行為。

---

## 2. YAML 實體定義指南

所有實體定義檔案必須以 `.xeku.yaml` 為副檔名，存放於 `entities/`。

### 基本結構範例
```yaml
entity: Product          # 實體名稱（對應 C# 類別名）
icon: BO_Product        # XAF 使用的圖示
fields:                 # 欄位清單
  - name: Code
    type: string
    required: true
    indexed: true
  - name: Price
    type: decimal
    defaultValue: 0
  - name: Quantity
    type: int
```

### 支援欄位型別 (Types)
| YAML 型別 | C# 型別 | 說明 |
| :--- | :--- | :--- |
| `string` | `string` | 字串，預設長度為 100 (可在 XAF 屬性自定義) |
| `int` | `int` | 整數 |
| `decimal` | `decimal` | 金額、高精度數值 |
| `bool` | `bool` | 布林值 |
| `datetime` | `DateTime` | 日期時間 |
| `guid` | `Guid` | 全域唯一識別碼 |

### 關聯性 (Relations)
- **Many-to-One (引用)**:
  ```yaml
  - name: Category
    type: reference
    entity: ProductCategory
  ```
- **One-to-Many (明細份項目)**:
  ```yaml
  - name: Items
    type: detail
    entity: OrderItem
  ```

---

## 3. 自動化指令集 (Workflows)

### 3.1 代碼生成與同步
當 YAML 變更時，立即重新產生代碼：
```powershell
dotnet run --project XekuII.Generator -- ./entities --output ./XekuII.ApiHost/BusinessObjects --controllers ./XekuII.ApiHost/API
```

### 3.2 資料庫更新
當生成代碼導致 XPO 結構變更時，執行資料庫同步：
```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj -- --updateDatabase --forceUpdate --silent
```

### 3.3 專案清理 (高度推薦)
在重新生成前，清理舊有的 `.Generated.cs` 以避免殘留：
- `Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force`
- `Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force`

---

## 4. 進階開發技巧

### Partial Class 運用
產生的產物通常帶有 `.Generated.cs` 標記。不應該手動修改這些檔案。
若要增加驗證邏輯或方法，請建立不帶 `.Generated` 的同名類別檔案：
```csharp
// 在 XekuII.ApiHost/BusinessObjects/Product.cs
public partial class Product {
    protected override void OnSaving() {
        base.OnSaving();
        // 自定義存檔邏輯
    }
}
```

### 命名空間修復
若 API Controller 找不到，請檢查 `XekuII.Generator` 是否輸出了正確的命名空間。預設應為 `Xeku.ApiHost.Controllers` (或與專案啟動類別一致)。

---

## 5. 常見問題 (Troubleshooting)

- **SQL 衝突**: 若更新資料庫失敗，通常是欄位型別變更導致。請使用 `--forceUpdate` 或手動清理資料庫。
- **Swagger 未顯示**: 檢查 Controller 是否繼承自正確的內容，且具備 `[Route("api/[controller]")]`。
- **重複產生**: 確保 `entities/` 下沒有內容重複、名稱相同的 YAML 檔案。
