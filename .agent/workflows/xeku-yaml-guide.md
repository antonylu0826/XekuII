---
description: XekuII 實體定義 YAML 完整撰寫指南
globs: entities/**/*.xeku.yaml
---

# XekuII YAML 實體定義指南

## 概要

在 `entities/` 目錄下建立 `.xeku.yaml` 檔案來定義業務實體。Generator 會讀取這些檔案，自動生成 C# XPO 業務物件、REST API 控制器，以及 React 前端頁面（TypeScript 型別、Zod Schema、API Client、List/Form/Detail 頁面）。

---

## 完整範例

以下展示一個包含所有支援功能的完整實體定義：

```yaml
entity: Product
caption: 產品
icon: BO_Product
dbTable: Products
description: Product inventory management

fields:
  # 基本字串欄位（必填、有長度限制、有驗證）
  - name: Code
    type: string
    required: true
    length: 20
    label: 產品代碼
    description: Unique product identifier
    validations:
      - regex: "^[A-Z]{2}-\\d{4}$"
        message: "Code format must be like AB-1234"

  # 基本字串欄位
  - name: Name
    type: string
    required: true
    label: 產品名稱
    description: Product display name

  # 數值欄位（有範圍驗證）
  - name: Price
    type: decimal
    label: 單價
    default: "0"
    description: Unit selling price
    validations:
      - range: ">=0"
        message: "Price cannot be negative"

  # 整數欄位
  - name: Quantity
    type: int
    label: 庫存數量
    default: "0"
    validations:
      - range: "0-999999"
        message: "Quantity must be between 0 and 999999"

  # 布林欄位
  - name: IsActive
    type: bool
    label: 啟用
    default: "true"

  # 日期時間欄位
  - name: CreatedDate
    type: datetime
    label: 建立日期
    default: "now"
    readonly: true

  # 列舉欄位（使用本實體定義的 enum）
  - name: Status
    type: ProductStatus
    label: 產品狀態
    default: "Active"

  # 計算欄位（PersistentAlias）
  - name: TotalValue
    type: decimal
    label: 庫存總值
    description: Total inventory value
    formula: "[Price] * [Quantity]"
    calculationType: persistent

  # 計算欄位（Getter）
  - name: DisplayName
    type: string
    label: 顯示名稱
    formula: "Code + \" - \" + Name"
    calculationType: getter

relations:
  # 多對一（外鍵）
  - name: Category
    type: reference
    target: ProductCategory
    required: true
    label: 產品分類
    lookupField: Name
    description: Product classification

  # 一對多（聚合子項目）
  - name: PriceHistory
    type: detail
    target: ProductPriceRecord
    label: 價格歷程
    cascade: delete
    description: Historical price changes

enums:
  - name: ProductStatus
    description: Product lifecycle status
    members:
      - name: Draft
        value: 0
        label: 草稿
        description: Product not yet published
      - name: Active
        value: 1
        label: 上架中
        description: Product is available for sale
      - name: Discontinued
        value: 2
        label: 已停售
        description: Product no longer available

rules:
  - trigger: BeforeSave
    script: ValidateProduct

# 前端 UI 配置（選用，省略則使用預設推斷）
ui:
  list:
    columns: [Code, Name, Category, Price, Quantity, Status]
    defaultSort: Name
    defaultSortDir: asc
    searchable: [Code, Name]
    filterable: [Status, Category]
    pageSize: 25
  form:
    layout:
      - row: [Code, Name]
      - row: [Category, Status]
      - row: [Price, Quantity]
      - row: [IsActive]
  detail:
    sections:
      - title: 基本資訊
        fields: [Code, Name, Category, Status, IsActive]
      - title: 定價與庫存
        fields: [Price, Quantity, TotalValue]
      - title: 價格歷程
        relation: PriceHistory

# 權限配置（選用，省略則使用預設值）
permissions:
  read: Default
  create: Default
  update: Default
  delete: Administrators
```

---

## UI 配置速查表

### ui.list（列表頁）

```yaml
ui:
  list:
    columns: [Field1, Field2, Ref1]     # 顯示哪些欄位，省略則全部非計算欄位
    defaultSort: Field1                   # 預設排序欄位
    defaultSortDir: desc                  # asc 或 desc
    searchable: [Field1, Field2]          # 可搜尋的欄位，省略則取前兩個字串欄位
    filterable: [Status]                  # 可篩選的欄位
    pageSize: 25                          # 每頁筆數，預設 20
```

### ui.form（表單頁）

```yaml
ui:
  form:
    layout:
      - row: [Field1, Field2]            # 同一列放多個欄位
      - row: [Field3]                     # 單獨一列
      - row: [RefField]                   # Reference 欄位自動用 ReferenceSelect
```

> 省略 `layout` 則每個欄位獨立一列。

### ui.detail（詳情頁）

```yaml
ui:
  detail:
    sections:
      - title: 區段標題
        fields: [Field1, Field2, Ref1]   # 顯示欄位
      - title: 明細項目
        relation: Items                   # 顯示 detail 關聯的表格
```

> 省略 `sections` 則所有欄位放在單一「Information」區段。

### permissions（權限）

```yaml
permissions:
  read: Default           # 讀取所需角色
  create: Default         # 建立所需角色
  update: Default         # 更新所需角色
  delete: Administrators  # 刪除所需角色
```

> 預設值：read/create/update = `Default`，delete = `Administrators`。

---

## 欄位型別速查表

| YAML 型別 | C# 型別 | 範例值 |
|-----------|---------|--------|
| `string` | `string` | `"hello"` |
| `int` | `int` | `42` |
| `decimal` | `decimal` | `99.99` |
| `double` | `double` | `3.14159` |
| `bool` | `bool` | `true` / `false` |
| `datetime` | `DateTime` | `2024-01-01` |
| `guid` | `Guid` | `550e8400-e29b-41d4-a716-446655440000` |
| `{EnumName}` | `{EnumName}` | 對應 enums 區塊定義的名稱 |

---

## 驗證規則速查表

```yaml
# 數值範圍
- range: ">0"           # 大於
- range: ">=0"          # 大於等於
- range: "<100"         # 小於
- range: "<=100"        # 小於等於
- range: "1-100"        # 範圍（含兩端）

# 最小 / 最大
- min: 0
- max: 9999

# 正規表達式
- regex: "^[A-Z0-9]+$"

# XAF Criteria
- criteria: "[EndDate] >= [StartDate]"

# 自訂錯誤訊息
- range: ">=0"
  message: "不可為負數"
```

---

## 預設值速查表

| 型別 | YAML default | 生成結果 |
|------|-------------|----------|
| string | `"NEW"` | `"NEW"` |
| int | `"0"` | `0` |
| decimal | `"100"` | `100m` |
| bool | `"true"` | `true` |
| datetime | `"now"` | `DateTime.Now` |
| datetime | `"today"` | `DateTime.Today` |
| datetime | `"utcnow"` | `DateTime.UtcNow` |
| guid | `"new"` | `Guid.NewGuid()` |
| guid | `"empty"` | `Guid.Empty` |
| enum | `"Draft"` | `{EnumName}.Draft` |

---

## 關聯定義

### Reference（多對一）

最常見的關聯類型。在資料庫中對應外鍵欄位。

```yaml
relations:
  - name: Department       # 屬性名稱
    type: reference         # 關聯類型
    target: Department      # 目標實體名稱
    required: true          # 是否必填（可選）
    label: 所屬部門         # UI 顯示名稱（可選）
    lookupField: Name       # 查詢時顯示的欄位（可選）
    description: ...        # AI 描述（可選）
```

### Detail（一對多，聚合）

主從關係。子項目隨主項目刪除而刪除（`[Aggregated]`）。

```yaml
relations:
  - name: LineItems         # 屬性名稱
    type: detail            # 關聯類型
    target: OrderLineItem   # 目標實體名稱
    label: 明細項目         # UI 顯示名稱（可選）
    cascade: delete         # 級聯行為（可選）
    description: ...        # AI 描述（可選）
```

### 關聯配對

若兩個實體互相引用，Generator 會自動偵測配對：

```yaml
# Order.xeku.yaml
relations:
  - name: Items
    type: detail
    target: OrderItem

# OrderItem.xeku.yaml
relations:
  - name: Order
    type: reference
    target: Order
```

Generator 確保兩端使用相同的 `AssociationName`。若只有單端定義關聯，另一端會自動生成反向屬性。

---

## 列舉定義

```yaml
enums:
  - name: Priority              # 列舉型別名稱（PascalCase）
    description: Task priority   # AI 描述（可選）
    members:
      - name: Low               # 成員名稱（PascalCase）
        value: 0                # 數值（可選，省略則自動遞增）
        label: 低               # UI 顯示（可選）
        description: Low prio   # AI 描述（可選）
      - name: Medium
        value: 1
        label: 中
      - name: High
        value: 2
        label: 高
      - name: Critical
        value: 3
        label: 緊急
```

使用方式：在 field 的 `type` 中填入列舉名稱。

```yaml
fields:
  - name: Priority
    type: Priority        # 對應上方 enums 中定義的名稱
    default: "Medium"     # 使用成員名稱作為預設值
```

---

## 業務規則

```yaml
rules:
  - trigger: BeforeSave       # 觸發時機（目前支援 BeforeSave）
    script: ValidateProduct   # Partial method 名稱
```

Generator 會：
1. 宣告 `partial void ValidateProduct();`
2. 在 `OnSaving()` 中呼叫 `ValidateProduct();`

手動實作：

```csharp
// BusinessObjects/Product.cs
public partial class Product
{
    partial void ValidateProduct()
    {
        if (Price <= 0 && IsActive)
            throw new UserFriendlyException("Active products must have a positive price.");
    }
}
```

---

## 常見錯誤

| 錯誤 | 原因 | 修正 |
|------|------|------|
| 列舉型別找不到 | field type 與 enum name 大小寫不一致 | 確保完全一致（PascalCase） |
| 反向關聯未生成 | 目標實體沒有 YAML 定義 | 建立目標實體的 `.xeku.yaml` |
| 驗證未生效 | `validations` 拼錯或格式不正確 | 檢查是否為 list 格式（`- range: "..."`） |
| 預設值格式錯誤 | 未用引號包裹 | YAML 中所有 default 值都用引號：`default: "true"` |
