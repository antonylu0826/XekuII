---
name: Entity Generation Standards
description: Comprehensive guide for defining XekuII entities using YAML, including field types, validation rules, and best practices.
---

# XekuII 實體定義規範 (Entity Generation Standards)

本文件詳細規範了如何使用 YAML 定義 XekuII 系統中的實體 (Entity)，這是驅動整個系統生成的核心。

---

## 1. 命名規範 (Naming Conventions)

### 1.1 標準駝峰式命名 (Standard CamelCase)

**非常重要**：在定義實體欄位名稱 (Field Name) 時，必須嚴格遵守標準的 PascalCase (大駝峰) 命名規則。特別是對於縮寫詞 (Acronyms)，應視為一般單字處理，避免連續大寫。

系統生成器會依據 `System.Text.Json` 的標準駝峰式規則將其轉換為前端使用的 camelCase (小駝峰)。若違反此規則，將導致前端無法正確綁定後端回傳的 JSON 資料。

| 欄位概念 | 推薦命名 (PascalCase) | 生成結果 (camelCase) | 狀態 | 說明 |
|----------|----------------------|---------------------|------|------|
| 產品編號 | `Sku` | `sku` | ✅ 推薦 | 把縮寫視為單字，最安全 |
| 採購單號 | `PoNumber` | `poNumber` | ✅ 推薦 | 把縮寫視為單字 |
| IP位置 | `IpAddress` | `ipAddress` | ✅ 推薦 | 把縮寫視為單字 |
| 產品編號 | `SKU` | `sku` | ⭕ 接受 | 生成器已修正支援全大寫縮寫 (SKU -> sku) |
| 採購單號 | `PONumber` | `poNumber` | ⭕ 接受 | 生成器已修正支援首部縮寫 (PONumber -> poNumber) |

> **警告**：過去版本生成器曾將 `SKU` 轉換為 `sKU`，這是不標準的。雖然新版生成器已修復此問題，但為了長遠的可讀性與一致性，建議**新定義的實體盡量使用 `Sku`, `PoNumber`, `IpAddress` 這類首字母大寫、其餘小寫的命名方式**。

---

## 2. YAML 結構範例

所有定義檔必須以 `.xeku.yaml` 為副檔名，存放於 `entities/` 目錄。

```yaml
entity: Order
caption: 訂單
icon: BO_Order
dbTable: Orders                        # 選用：自訂表格名稱
description: Sales order management    # AI 可讀的用途描述

fields:
  - name: OrderNumber
    type: string
    required: true
    length: 20
    label: 訂單編號
    description: Unique order number
    validations:
      - regex: "^ORD-\\d{6}$"
        message: "Order number must be like ORD-000001"

  - name: OrderDate
    type: datetime
    required: true
    label: 訂單日期
    default: "now"

  - name: TotalAmount
    type: decimal
    label: 總金額
    formula: "[Items].Sum([Subtotal])"
    calculationType: persistent

  - name: Note
    type: string
    length: 500
    label: 備註

  - name: Status
    type: OrderStatus
    label: 訂單狀態
    default: "Draft"

  - name: IsUrgent
    type: bool
    label: 急件
    default: "false"

relations:
  - name: Customer
    type: reference
    target: Customer
    required: true
    label: 客戶
    lookupField: Name
    description: Customer who placed this order

  - name: Items
    type: detail
    target: OrderItem
    label: 訂單明細
    cascade: delete
    description: Line items of this order

enums:
  - name: OrderStatus
    description: Order processing lifecycle
    members:
      - name: Draft
        value: 0
        label: 草稿
        description: Order is being prepared
      - name: Confirmed
        value: 1
        label: 已確認
      - name: Shipped
        value: 2
        label: 已出貨
      - name: Completed
        value: 3
        label: 已完成
      - name: Cancelled
        value: 4
        label: 已取消

rules:
  - trigger: BeforeSave
    script: ValidateOrder

ui:
  list:
    columns: [OrderNumber, OrderDate, Customer, Status, TotalAmount]
    defaultSort: OrderDate
    defaultSortDir: desc
    searchable: [OrderNumber]
    filterable: [Status]
    pageSize: 25
  form:
    layout:
      - row: [OrderNumber, OrderDate]
      - row: [Customer, Status]
      - row: [IsUrgent]
      - row: [Note]
  detail:
    sections:
      - title: 基本資訊
        fields: [OrderNumber, OrderDate, Customer, Status, IsUrgent]
      - title: 金額
        fields: [TotalAmount]
      - title: 訂單明細
        relation: Items
      - title: 備註
        fields: [Note]

permissions:
  read: Default
  create: Default
  update: Default
  delete: Administrators
```

---

## 3. UI 區塊說明

`ui` 區塊控制前端頁面的生成行為：

| 區塊 | 說明 |
|------|------|
| `ui.list` | 列表頁：顯示欄位、排序、搜尋、篩選、每頁筆數 |
| `ui.form` | 表單頁：layout 定義每列包含哪些欄位 |
| `ui.detail` | 詳情頁：sections 定義分區，可包含欄位或關聯（relation） |
| `permissions` | CRUD 各動作所需角色（`read`、`create`、`update`、`delete`） |

> 若未定義 `ui` 區塊，Generator 會以合理預設值自動推斷（所有非計算欄位顯示於列表、一列一欄位的表單、單一分區的詳情頁）。

---

## 4. 欄位型別映射

| YAML 型別 | C# 型別 | TypeScript 型別 | Zod 型別 | 說明 |
|-----------|---------|----------------|----------|------|
| `string` | `string` | `string` | `z.string()` | 字串，可用 `length` 設定最大長度 |
| `int` | `int` | `number` | `z.number().int()` | 整數 |
| `decimal` | `decimal` | `number` | `z.number()` | 高精度數值（金額等） |
| `double` | `double` | `number` | `z.number()` | 浮點數 |
| `bool` | `bool` | `boolean` | `z.boolean()` | 布林值 |
| `datetime` | `DateTime` | `string` (ISO) | `z.string()` | 日期時間 |
| `guid` | `Guid` | `string` | `z.string().uuid()` | 全域唯一識別碼 |
| `{EnumName}` | `{EnumName}` | `enum (string)` | `z.nativeEnum()` | 實體範圍列舉 (API 傳輸與前端皆為 String) |

---

## 5. 驗證規則語法

```yaml
validations:
  # 範圍驗證
  - range: ">0"          # ValueComparisonType.GreaterThan
  - range: ">=1"         # ValueComparisonType.GreaterThanOrEqual
  - range: "<100"        # ValueComparisonType.LessThan
  - range: "<=99"        # ValueComparisonType.LessThanOrEqual
  - range: "1-100"       # RuleRange (含兩端)

  # 最小/最大值
  - min: 0               # GreaterThanOrEqual
  - max: 999             # LessThanOrEqual

  # 正規表達式
  - regex: "^[A-Z]{2}-\\d{4}$"

  # XAF Criteria 表達式
  - criteria: "[EndDate] > [StartDate]"

  # 自訂錯誤訊息（附加在任一規則上）
  - range: ">=0"
    message: "金額不可為負數"
```

---

## 6. 預設值語法

| 型別 | YAML 值 | 生成的 C# |
|------|---------|-----------|
| `string` | `"NEW"` | `"NEW"` |
| `int` | `"0"` | `0` |
| `decimal` | `"100.5"` | `100.5m` |
| `bool` | `"true"` / `"false"` | `true` / `false` |
| `datetime` | `"now"` | `DateTime.Now` |
| `datetime` | `"today"` | `DateTime.Today` |
| `datetime` | `"utcnow"` | `DateTime.UtcNow` |
| `guid` | `"new"` | `Guid.NewGuid()` |
| `guid` | `"empty"` | `Guid.Empty` |
| `{Enum}` | `"Draft"` | `{EnumName}.Draft` |

**日期型別注意事項**：
前端 HTML5 `<input type="datetime-local">` 僅接受 `yyyy-MM-ddTHH:mm` 格式（不含時區 Z）。
生成程式碼時，若預設值為 `now`/`today`，Generator 會自動轉換為本地時間的 ISO String (由 `ReactGeneratorUtils` 處理)。

---

## 7. 計算欄位

**PersistentAlias**（XPO Criteria 語法，可參與資料庫查詢）：
```yaml
- name: Total
  type: decimal
  formula: "[Quantity] * [UnitPrice]"
  calculationType: persistent    # 預設值，可省略
```

**Getter**（C# 表達式，僅在記憶體中計算）：
```yaml
- name: FullName
  type: string
  formula: "FirstName + \" \" + LastName"
  calculationType: getter
```

> **注意：前端表單處理**
> 帶有 `formula` 的欄位在生成前端代碼時，必須注意：
> 1. **Schema 驗證**：Zod Schema 必須將此類欄位設為 `.optional()`，因為它們通常由後端計算，前端表單不會送出此值。若設為必填會導致表單提交靜默失敗。
> 2. **表單顯示**：通常以 `readonly` 或 `disabled` 方式顯示，或僅在詳情頁顯示。

---

## 8. 關聯類型

**Reference（多對一）** — 產生外鍵屬性 + `[Association]`：
```yaml
relations:
  - name: Category
    type: reference
    target: ProductCategory
    required: true           # 必填外鍵
    lookupField: Name        # 查詢時顯示的欄位
```

**Detail（一對多，聚合）** — 產生 `XPCollection` + `[Aggregated]`：
```yaml
relations:
  - name: Items
    type: detail
    target: OrderItem
    cascade: delete          # 級聯刪除
```

> **最佳實踐**：
> - 僅在「強聚合 (Strong Aggregation)」關係中使用 `detail`（即子物件生命週期完全依賴父物件，如訂單明細）。
> - 對於獨立的主檔實體（如採購單），即使是供應商的「紀錄」，也應使用 `reference` 關係搭配查詢，避免將其降級為附屬物件導致導航隱藏。

---

## 9. 自動反向關聯

Generator 會自動分析所有實體之間的關係：

- **配對關係**：若 A 定義 `reference → B` 且 B 定義 `detail → A`，兩端共用同一個 `AssociationName`
- **單向關係**：若只有一端定義關聯，Generator 自動在另一端生成反向屬性
  - `reference → TargetEntity` → 在 Target 生成 `XPCollection<SourceEntity>`
  - `detail → TargetEntity` → 在 Target 生成 `SourceEntity` reference 屬性

---

## 10. Detail 子實體的 Reference 關聯

Detail 子實體（如 `OrderLine`）可以擁有自己的 reference 關聯（如參照 `Product`）：

```yaml
# OrderLine.xeku.yaml
entity: OrderLine
relations:
  - name: Product
    type: reference
    target: Product
    required: true
    label: 產品
    lookupField: Name
```

Generator 會自動處理：

1. **後端 API**：`OrderAddOrderLineRequest` 包含 `ProductId` 欄位
2. **前端表單**：Detail item 新增對話框顯示 `ReferenceSelect` 下拉選單
3. **導航排除**：Detail 子實體（如 OrderLine）不會出現在側邊導航選單，只能從父實體詳情頁存取

> **重要**：Detail 子實體的 reference 會自動排除對父實體的反向參照。例如 OrderLine 的 Product reference 會包含在表單中，但 Order reference 不會（由系統自動設定）。
