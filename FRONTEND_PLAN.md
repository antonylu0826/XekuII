# XekuII 前端架構計畫書

> 版本：2026-02-05 v1.0
> 狀態：草案，待審核

---

## 一、目標與原則

### 1.1 核心目標

延伸現有的 **YAML 驅動開發**模式，讓同一份 `.xeku.yaml` 同時生成後端（C# XPO BO + API Controller）與前端（TypeScript 類型 + API Client + React CRUD 頁面），實現：

- **一份 YAML，前後端全生成**
- **AI 寫 YAML → 執行 Generator → 完整應用即刻可用**
- **零手動同步**：YAML 變更後重新生成，前後端自動一致

### 1.2 設計原則

| 原則 | 說明 |
|------|------|
| **YAML 即規格** | 不引入額外規格層；YAML 本身承載資料結構、驗證、UI 提示、權限等所有語意 |
| **生成與手寫分離** | `*.generated.*` 由 Generator 產出，不可手動修改；客製化邏輯放在獨立檔案中 |
| **AI 友善** | 選擇 AI 訓練資料最豐富的技術棧，確保生成品質與可維護性 |
| **最小複雜度** | 不過度設計，不為假設性需求預留架構；當前需要什麼就做什麼 |
| **漸進式增強** | 先生成能用的 CRUD，再逐步加入進階功能（Dashboard、圖表、工作流等） |

---

## 二、技術棧選型

### 2.1 前端技術棧

| 層級 | 技術 | 版本 | 選型理由 |
|------|------|------|----------|
| **語言** | TypeScript | 5.x | 型別安全、AI 生成品質最佳 |
| **框架** | React | 19.x | AI 訓練資料量最大，生態系最完整 |
| **建置工具** | Vite | 6.x | 零配置、秒級 HMR、原生 ESM |
| **路由** | TanStack Router | 1.x | 型別安全、檔案式路由、與 TanStack 生態整合 |
| **資料查詢** | TanStack Query | 5.x | 自動快取、樂觀更新、背景重新取得 |
| **表格** | TanStack Table | 8.x | Headless、排序/篩選/分頁、與 shadcn/ui 整合 |
| **UI 元件** | shadcn/ui | latest | 原始碼即元件，AI 可完全理解與修改，非黑箱套件 |
| **樣式** | Tailwind CSS | 4.x | Utility-first、AI 友善、與 shadcn/ui 搭配 |
| **表單** | React Hook Form | 7.x | 效能優、API 簡潔 |
| **驗證** | Zod | 3.x | TypeScript-first schema，與 YAML validation 直接映射 |
| **HTTP Client** | Axios | 1.x | Interceptor 支援 JWT、錯誤攔截 |
| **全域狀態** | Zustand | 5.x | 極簡、AI 友善（僅用於認證等全域狀態） |
| **圖示** | Lucide React | latest | shadcn/ui 預設圖示庫 |

### 2.2 排除方案說明

| 方案 | 排除理由 |
|------|----------|
| **DevExtreme (JS)** | 封裝度高，AI 難以理解內部實作；元件客製化受限 |
| **Blazor** | AI 對 Blazor 的生成品質遠不如 React/TS；社群資源有限 |
| **Vue / Angular** | AI 訓練資料量不如 React；TanStack 生態以 React 為主 |
| **Next.js** | SSR 對管理後台無必要，增加部署複雜度 |
| **Ant Design / MUI** | 高度封裝的元件庫，不如 shadcn/ui 對 AI 透明 |

---

## 三、YAML Schema 擴展

在現有 `.xeku.yaml` 基礎上，新增 `ui` 和 `permissions` 區塊，讓 Generator 有足夠資訊生成前端頁面。

### 3.1 擴展後的完整 YAML 結構

```yaml
entity: Product
caption: 產品                    # UI 顯示名稱
icon: BO_Product                # 圖示（前端映射為 Lucide icon）
description: Product inventory management

# ─── 欄位定義（現有）───
fields:
  - name: Code
    type: string
    required: true
    label: 產品代碼              # 表單 label
    description: Unique product code
    length: 50
    validations:
      - regex: "^[A-Z]{2}-\\d{4}$"
        message: "Code must be like AB-1234"

  - name: Name
    type: string
    required: true
    label: 產品名稱

  - name: Price
    type: decimal
    label: 單價
    default: "0"
    validations:
      - range: ">=0"
        message: "Price cannot be negative"

  - name: IsActive
    type: bool
    label: 啟用
    default: "true"

  - name: Total
    type: decimal
    label: 小計
    formula: "[Quantity] * [Price]"
    calculationType: persistent

# ─── 關聯定義（現有）───
relations:
  - name: Category
    type: reference
    target: ProductCategory
    required: true
    label: 產品分類
    lookupField: Name

  - name: OrderItems
    type: detail
    target: OrderItem
    label: 訂單明細

# ─── 列舉定義（現有）───
enums:
  - name: ProductStatus
    description: Product lifecycle status
    members:
      - name: Draft
        label: 草稿
        value: 0
      - name: Active
        label: 上架
        value: 1
      - name: Discontinued
        label: 停售
        value: 2

# ─── 業務規則（現有）───
rules:
  - trigger: BeforeSave
    script: ValidateProduct

# ═══ 以下為新增區塊 ═══

# ─── UI 配置（新增）───
ui:
  list:
    columns: [Code, Name, Price, Category, IsActive]  # 列表頁顯示欄位
    defaultSort: Code                                   # 預設排序欄位
    defaultSortDir: asc                                 # 排序方向
    searchable: [Code, Name]                            # 可搜尋欄位
    filterable: [Category, IsActive]                    # 可篩選欄位
    pageSize: 20                                        # 預設分頁大小

  form:
    layout:                                              # 表單佈局
      - row: [Code, Name]                               # 同一行的欄位
      - row: [Price, Category]
      - row: [IsActive]
    # 未列出的欄位依序排列在最後

  detail:
    sections:                                            # 詳情頁分區
      - title: 基本資訊
        fields: [Code, Name, Price, Category, IsActive]
      - title: 訂單明細
        relation: OrderItems                             # 顯示 detail 關聯為子表格

# ─── 權限配置（新增）───
permissions:
  read: Default                    # 需要的最低角色
  create: Default
  update: Default
  delete: Administrators           # 僅管理員可刪除
```

### 3.2 新增 Model 類別

需要在 `XekuII.Generator/Models/` 新增以下類別：

```csharp
// Models/UiDefinition.cs
public class UiDefinition
{
    public ListUiDefinition? List { get; set; }
    public FormUiDefinition? Form { get; set; }
    public DetailUiDefinition? Detail { get; set; }
}

public class ListUiDefinition
{
    public List<string> Columns { get; set; } = new();
    public string? DefaultSort { get; set; }
    public string? DefaultSortDir { get; set; }
    public List<string> Searchable { get; set; } = new();
    public List<string> Filterable { get; set; } = new();
    public int PageSize { get; set; } = 20;
}

public class FormUiDefinition
{
    public List<FormRow> Layout { get; set; } = new();
}

public class FormRow
{
    public List<string> Row { get; set; } = new();
}

public class DetailUiDefinition
{
    public List<DetailSection> Sections { get; set; } = new();
}

public class DetailSection
{
    public string Title { get; set; } = string.Empty;
    public List<string>? Fields { get; set; }
    public string? Relation { get; set; }
}

// Models/PermissionsDefinition.cs
public class PermissionsDefinition
{
    public string Read { get; set; } = "Default";
    public string Create { get; set; } = "Default";
    public string Update { get; set; } = "Default";
    public string Delete { get; set; } = "Administrators";
}
```

並在 `EntityDefinition` 中加入：

```csharp
public UiDefinition? Ui { get; set; }
public PermissionsDefinition? Permissions { get; set; }
```

### 3.3 UI 配置的預設行為

當 YAML 未提供 `ui` 區塊時，Generator 應自動推導合理預設值：

| 屬性 | 預設行為 |
|------|----------|
| `list.columns` | 所有非計算、非 detail 的欄位 |
| `list.defaultSort` | 第一個 string 型別欄位，若無則為 Name |
| `list.searchable` | 所有 string 型別欄位 |
| `list.filterable` | 所有 reference 和 bool 欄位 |
| `form.layout` | 每行一個欄位，依定義順序 |
| `detail.sections` | 單一區塊包含所有欄位 + 各 detail 關聯各一區塊 |
| `permissions.*` | 全部為 Default |

---

## 四、Generator 擴展計畫

### 4.1 新增生成器

在 `XekuII.Generator/Generators/` 下新增四個前端代碼生成器：

```
XekuII.Generator/
├── Generators/
│   ├── CSharpCodeGenerator.cs          # 現有：C# BO
│   ├── ControllerCodeGenerator.cs      # 現有：C# API Controller
│   ├── TypeScriptTypeGenerator.cs      # 新增：TS 介面 + 列舉
│   ├── ZodSchemaGenerator.cs           # 新增：Zod 驗證 schema
│   ├── ApiClientGenerator.cs           # 新增：Typed API client
│   └── ReactPageGenerator.cs           # 新增：React CRUD 頁面
```

### 4.2 各生成器職責與輸出

#### TypeScriptTypeGenerator

**輸入**：`EntityDefinition`
**輸出**：`{entity}.types.generated.ts`

映射規則：

| YAML Type | TypeScript Type |
|-----------|----------------|
| `string` | `string` |
| `int` | `number` |
| `decimal` | `number` |
| `double` | `number` |
| `bool` | `boolean` |
| `datetime` | `string` (ISO 8601) |
| `guid` | `string` |
| `enum` | 生成的 TypeScript enum |
| `reference` | `{ id: string; [lookupField]: string } \| null` |
| `detail` | 不出現在 base type，出現在 DetailsType |

生成內容：

```typescript
// product.types.generated.ts

/** Product lifecycle status */
export enum ProductStatus {
  /** 草稿 */
  Draft = 0,
  /** 上架 */
  Active = 1,
  /** 停售 */
  Discontinued = 2,
}

/** Product inventory management */
export interface Product {
  id: string;
  code: string;
  name: string;
  price: number;
  isActive: boolean;
  total: number;                         // 計算欄位（唯讀）
  category: ProductCategoryRef | null;
}

export interface ProductCategoryRef {
  id: string;
  name: string;                          // 來自 lookupField
}

export interface ProductDetails extends Product {
  orderItems: OrderItemSummary[];        // detail 關聯
}

export interface CreateProductDto {
  code: string;
  name: string;
  price: number;
  isActive: boolean;
  categoryId: string | null;
}

export interface UpdateProductDto {
  code?: string;
  name?: string;
  price?: number;
  isActive?: boolean;
  categoryId?: string | null;
}
```

#### ZodSchemaGenerator

**輸入**：`EntityDefinition`（fields + validations）
**輸出**：`{entity}.schema.generated.ts`

驗證映射規則：

| YAML Validation | Zod Method |
|----------------|------------|
| `required: true` | `.min(1)` (string) 或不加 `.optional()` |
| `range: ">0"` | `.gt(0)` |
| `range: ">=1"` | `.gte(1)` |
| `range: "0-100"` | `.gte(0).lte(100)` |
| `min: 5` | `.gte(5)` |
| `max: 100` | `.lte(100)` |
| `regex: "^..."` | `.regex(/^.../)` |
| `length: 50` | `.max(50)` |
| `message: "..."` | 作為錯誤訊息參數 |

生成內容：

```typescript
// product.schema.generated.ts
import { z } from "zod";

export const createProductSchema = z.object({
  code: z.string()
    .min(1, "Code is required")
    .max(50)
    .regex(/^[A-Z]{2}-\d{4}$/, "Code must be like AB-1234"),
  name: z.string().min(1, "Name is required"),
  price: z.number().gte(0, "Price cannot be negative"),
  isActive: z.boolean(),
  categoryId: z.string().uuid().nullable(),
});

export const updateProductSchema = createProductSchema.partial();

export type CreateProductInput = z.infer<typeof createProductSchema>;
export type UpdateProductInput = z.infer<typeof updateProductSchema>;
```

#### ApiClientGenerator

**輸入**：`EntityDefinition`（entity name + relations）
**輸出**：`{entity}.api.generated.ts`

根據 `ControllerCodeGenerator` 生成的端點結構，產生對應的 typed client：

```typescript
// product.api.generated.ts
import { apiClient } from "@/lib/api-client";
import type {
  Product,
  ProductDetails,
  CreateProductDto,
  UpdateProductDto,
} from "./product.types.generated";
import type { PaginatedResult, QueryParams } from "@/lib/types";

const BASE = "/api/products";

export const productApi = {
  /** GET /api/products */
  getAll: (params?: QueryParams) =>
    apiClient.get<PaginatedResult<Product>>(BASE, { params }),

  /** GET /api/products/:id */
  getById: (id: string) =>
    apiClient.get<Product>(`${BASE}/${id}`),

  /** GET /api/products/:id/details */
  getDetails: (id: string) =>
    apiClient.get<ProductDetails>(`${BASE}/${id}/details`),

  /** POST /api/products */
  create: (data: CreateProductDto) =>
    apiClient.post<Product>(BASE, data),

  /** PUT /api/products/:id */
  update: (id: string, data: UpdateProductDto) =>
    apiClient.put<Product>(`${BASE}/${id}`, data),

  /** DELETE /api/products/:id */
  delete: (id: string) =>
    apiClient.delete(`${BASE}/${id}`),

  // ── Detail 關聯端點 ──

  /** GET /api/products/:id/orderItems */
  getOrderItems: (id: string) =>
    apiClient.get<OrderItemSummary[]>(`${BASE}/${id}/orderItems`),

  /** POST /api/products/:id/orderItems */
  addOrderItem: (id: string, data: AddOrderItemRequest) =>
    apiClient.post(`${BASE}/${id}/orderItems`, data),

  /** PUT /api/products/:id/orderItems/:itemId */
  updateOrderItem: (id: string, itemId: string, data: UpdateOrderItemRequest) =>
    apiClient.put(`${BASE}/${id}/orderItems/${itemId}`, data),

  /** DELETE /api/products/:id/orderItems/:itemId */
  deleteOrderItem: (id: string, itemId: string) =>
    apiClient.delete(`${BASE}/${id}/orderItems/${itemId}`),
};
```

#### ReactPageGenerator

**輸入**：`EntityDefinition`（含 `ui` 配置）
**輸出**：三個頁面檔案

**列表頁** (`ProductListPage.generated.tsx`)：
- DataTable 使用 TanStack Table + shadcn/ui
- 欄位依 `ui.list.columns` 排列
- 搜尋框對 `ui.list.searchable` 欄位做模糊查詢
- 篩選器對 `ui.list.filterable` 欄位做精確篩選
- 分頁使用 `ui.list.pageSize`
- 排序預設 `ui.list.defaultSort`
- 每行有「檢視」「編輯」「刪除」操作按鈕
- 「新增」按鈕導向表單頁
- 使用 TanStack Query 管理資料

**表單頁** (`ProductFormPage.generated.tsx`)：
- React Hook Form + Zod schema 驗證
- 佈局依 `ui.form.layout` 排列（多欄位同行）
- Reference 欄位渲染為 Select/Combobox（從 API 取得選項）
- Bool 欄位渲染為 Switch
- Enum 欄位渲染為 Select
- 計算欄位不出現在表單
- 區分新增/編輯模式（透過 URL 參數）
- 提交後導向列表頁

**詳情頁** (`ProductDetailPage.generated.tsx`)：
- 依 `ui.detail.sections` 分區顯示
- 一般欄位以 Label + Value 格式顯示
- Detail 關聯以子 DataTable 顯示（支援行內新增/編輯/刪除）
- Reference 欄位顯示為可點擊連結
- 「編輯」「刪除」「返回列表」按鈕

### 4.3 路由與導覽自動生成

Generator 另外產生：

**`routes.generated.ts`**：自動註冊所有實體的路由

```typescript
// routes.generated.ts
import { ProductListPage } from "./pages/product/ProductListPage.generated";
import { ProductFormPage } from "./pages/product/ProductFormPage.generated";
import { ProductDetailPage } from "./pages/product/ProductDetailPage.generated";
// ... 其他實體

export const generatedRoutes = [
  { path: "/products",       component: ProductListPage,   label: "產品",   icon: "package" },
  { path: "/products/new",   component: ProductFormPage },
  { path: "/products/:id",   component: ProductDetailPage },
  { path: "/products/:id/edit", component: ProductFormPage },
  // ... 其他實體
];
```

**`navigation.generated.ts`**：側邊欄選單資料

```typescript
// navigation.generated.ts
export const generatedNavItems = [
  { label: "產品",       path: "/products",        icon: "package" },
  { label: "產品分類",   path: "/product-categories", icon: "folder" },
  // ... 其他實體，依 entity caption 和 icon 生成
];
```

---

## 五、前端專案結構

### 5.1 目錄配置

```
xekuii-web/                              # 前端專案根目錄（與 XekuII.ApiHost 同級）
├── public/
│   └── favicon.ico
│
├── src/
│   ├── generated/                       # ⚡ Generator 產出，禁止手動修改
│   │   ├── types/                       # TypeScript 介面
│   │   │   ├── product.types.generated.ts
│   │   │   ├── product-category.types.generated.ts
│   │   │   └── index.ts                 # Re-export all
│   │   │
│   │   ├── schemas/                     # Zod 驗證 schema
│   │   │   ├── product.schema.generated.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── api/                         # Typed API client
│   │   │   ├── product.api.generated.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── pages/                       # CRUD 頁面元件
│   │   │   ├── product/
│   │   │   │   ├── ProductListPage.generated.tsx
│   │   │   │   ├── ProductDetailPage.generated.tsx
│   │   │   │   └── ProductFormPage.generated.tsx
│   │   │   └── product-category/
│   │   │       └── ...
│   │   │
│   │   ├── routes.generated.ts          # 路由自動註冊
│   │   └── navigation.generated.ts      # 側邊欄選單資料
│   │
│   ├── components/                      # 手動維護的共用元件
│   │   ├── ui/                          # shadcn/ui 元件（初始化時安裝）
│   │   │   ├── button.tsx
│   │   │   ├── input.tsx
│   │   │   ├── select.tsx
│   │   │   ├── data-table.tsx           # 封裝 TanStack Table
│   │   │   ├── dialog.tsx
│   │   │   ├── form.tsx
│   │   │   ├── toast.tsx
│   │   │   └── ...
│   │   │
│   │   ├── layout/                      # 佈局元件
│   │   │   ├── AppLayout.tsx            # 主佈局（Sidebar + Header + Content）
│   │   │   ├── Sidebar.tsx              # 側邊欄（消費 navigation.generated.ts）
│   │   │   ├── Header.tsx               # 頂部列（使用者資訊、登出）
│   │   │   └── Breadcrumb.tsx           # 麵包屑導覽
│   │   │
│   │   └── shared/                      # 共用業務元件
│   │       ├── EntityDataTable.tsx       # 通用實體表格 wrapper
│   │       ├── EntityForm.tsx            # 通用實體表單 wrapper
│   │       ├── ReferenceSelect.tsx       # Reference 欄位下拉選擇器
│   │       ├── ConfirmDialog.tsx         # 刪除確認對話框
│   │       └── LoadingSpinner.tsx
│   │
│   ├── lib/                             # 基礎設施
│   │   ├── api-client.ts               # Axios instance + JWT interceptor
│   │   ├── auth.ts                     # 認證狀態管理（Zustand store）
│   │   ├── types.ts                    # 共用型別（PaginatedResult, QueryParams）
│   │   ├── utils.ts                    # 工具函式（cn, formatDate, etc.）
│   │   └── query-client.ts            # TanStack Query 配置
│   │
│   ├── pages/                           # 手動維護的特殊頁面
│   │   ├── LoginPage.tsx
│   │   ├── DashboardPage.tsx
│   │   └── NotFoundPage.tsx
│   │
│   ├── hooks/                           # 自定義 Hooks
│   │   ├── useAuth.ts                  # 認證 hook
│   │   └── useEntityQuery.ts           # 通用實體 CRUD query hook
│   │
│   ├── App.tsx                          # 根元件（路由 + 佈局）
│   ├── main.tsx                         # 進入點
│   └── index.css                        # Tailwind 入口
│
├── package.json
├── vite.config.ts
├── tailwind.config.ts
├── tsconfig.json
├── tsconfig.app.json
├── components.json                      # shadcn/ui 配置
└── .eslintrc.cjs
```

### 5.2 共用基礎設施說明

#### `lib/api-client.ts`

- Axios instance，baseURL 指向 `http://localhost:5000`
- Request interceptor：自動附加 JWT token
- Response interceptor：401 自動導向登入頁、統一錯誤處理
- 支援 CORS

#### `lib/auth.ts`

- Zustand store 管理認證狀態
- `login(username, password)` → 呼叫 XAF JWT 端點
- `logout()` → 清除 token、導向登入頁
- `isAuthenticated` → 判斷登入狀態
- Token 存於 `localStorage`

#### `lib/types.ts`

```typescript
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface QueryParams {
  page?: number;
  pageSize?: number;
  sort?: string;
  sortDir?: "asc" | "desc";
  search?: string;
  [key: string]: unknown;
}
```

#### `components/shared/EntityDataTable.tsx`

通用的 DataTable wrapper，生成的 ListPage 會使用它：

- 接收 column 定義、data、pagination 等 props
- 內建排序、搜尋、篩選 UI
- 內建分頁控制
- 行操作按鈕（檢視、編輯、刪除）

#### `components/shared/EntityForm.tsx`

通用的 Form wrapper，生成的 FormPage 會使用它：

- 接收 Zod schema、defaultValues、onSubmit 等 props
- 自動渲染各類型欄位
- 支援多欄佈局
- 內建錯誤顯示

---

## 六、生成指令整合

### 6.1 擴展 XekuGenerator

修改 `XekuGenerator.cs` 的 `GenerateAll` 方法，新增 `--frontend` 參數：

```csharp
public void GenerateAll(
    string entitiesDirectory,
    string targetNamespace = "XekuII.Generated",
    bool generateControllers = false,
    string? controllersOutputDir = null,
    bool generateFrontend = false,           // 新增
    string? frontendOutputDir = null)         // 新增
```

### 6.2 統一指令

```powershell
# 完整生成（後端 + 前端）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API `
  --frontend ./xekuii-web/src/generated

# 僅生成後端（向後相容）
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API

# 僅生成前端（開發時快速迭代）
dotnet run --project XekuII.Generator -- ./entities `
  --frontend ./xekuii-web/src/generated
```

### 6.3 CLI 擴展

```powershell
# XekuII.CLI
XekuII.CLI generate ./entities --all       # 後端 + 前端
XekuII.CLI generate ./entities --web       # 僅前端
XekuII.CLI generate ./entities --api       # 僅後端
```

---

## 七、完整開發工作流

```
┌─────────────────────────────────────────────────────────┐
│  AI（或開發者）定義實體                                    │
│  → entities/Product.xeku.yaml                           │
└───────────────┬─────────────────────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────────────────────┐
│  執行 Generator                                         │
│  dotnet run --project XekuII.Generator -- ./entities    │
│    --output ... --controllers ... --frontend ...        │
└───────────────┬─────────────────────────────────────────┘
                │
        ┌───────┴───────┐
        ▼               ▼
┌──────────────┐ ┌──────────────────┐
│ 後端生成      │ │ 前端生成          │
│ BO.cs        │ │ types.ts         │
│ Controller.cs│ │ schema.ts        │
│ DTO          │ │ api.ts           │
│              │ │ ListPage.tsx     │
│              │ │ FormPage.tsx     │
│              │ │ DetailPage.tsx   │
│              │ │ routes.ts        │
│              │ │ navigation.ts    │
└──────┬───────┘ └────────┬─────────┘
       │                  │
       ▼                  ▼
┌──────────────┐ ┌──────────────────┐
│ 更新資料庫    │ │ Vite dev server  │
│ --updateDB   │ │ 自動 HMR 更新     │
└──────┬───────┘ └────────┬─────────┘
       │                  │
       └────────┬─────────┘
                ▼
┌─────────────────────────────────────────────────────────┐
│  完整 CRUD 應用即刻可用                                   │
│  後端: http://localhost:5000/swagger                     │
│  前端: http://localhost:5173                             │
└─────────────────────────────────────────────────────────┘
```

---

## 八、實作階段

### P0：前端專案骨架（基礎設施）

**目標**：建立可運行的前端專案，包含佈局、認證、共用元件

- [ ] 初始化 Vite + React + TypeScript 專案
- [ ] 安裝與配置 Tailwind CSS v4
- [ ] 安裝與配置 shadcn/ui
- [ ] 安裝 TanStack Query、TanStack Router、React Hook Form、Zod、Axios、Zustand
- [ ] 實作 `lib/api-client.ts`（Axios + JWT interceptor）
- [ ] 實作 `lib/auth.ts`（Zustand 認證 store）
- [ ] 實作 `lib/types.ts`（共用型別）
- [ ] 實作 `pages/LoginPage.tsx`
- [ ] 實作 `components/layout/`（AppLayout, Sidebar, Header）
- [ ] 實作 `components/shared/EntityDataTable.tsx`
- [ ] 實作 `components/shared/EntityForm.tsx`
- [ ] 實作 `components/shared/ReferenceSelect.tsx`
- [ ] 配置 proxy 到 localhost:5000

### P1：Generator 擴展 — 型別與驗證

**目標**：YAML → TypeScript Types + Zod Schema

- [ ] 新增 `Models/UiDefinition.cs` 和 `Models/PermissionsDefinition.cs`
- [ ] 擴展 `EntityDefinition.cs` 加入 `Ui` 和 `Permissions` 屬性
- [ ] 實作 `Generators/TypeScriptTypeGenerator.cs`
- [ ] 實作 `Generators/ZodSchemaGenerator.cs`
- [ ] 擴展 `XekuGenerator.cs` 支援 `--frontend` 參數
- [ ] 單元測試：驗證各型別映射正確

### P2：Generator 擴展 — API Client 與列表頁

**目標**：YAML → API Client + ListPage

- [ ] 實作 `Generators/ApiClientGenerator.cs`
- [ ] 實作 `Generators/ReactPageGenerator.cs`（先做 ListPage）
- [ ] 生成 `routes.generated.ts` 和 `navigation.generated.ts`
- [ ] 端對端驗證：定義一個測試 YAML → 生成 → 前端列表頁可正常顯示資料

### P3：Generator 擴展 — 表單頁與詳情頁

**目標**：YAML → FormPage + DetailPage

- [ ] `ReactPageGenerator` 加入 FormPage 生成
- [ ] `ReactPageGenerator` 加入 DetailPage 生成
- [ ] 表單與 Zod schema 整合驗證
- [ ] Detail 子表格行內 CRUD
- [ ] 端對端驗證：完整 CRUD 流程（列表→新增→檢視→編輯→刪除）

### P4：進階功能

**目標**：產品化增強

- [ ] Dashboard 頁面（顯示各實體統計數量）
- [ ] 權限控制 UI（依 `permissions` 配置隱藏/停用按鈕）
- [ ] 全域搜尋
- [ ] 資料匯出（CSV）
- [ ] 深色模式（Tailwind dark: 前綴）
- [ ] 響應式佈局（行動裝置適配）

### P5：開發體驗優化

**目標**：提升 AI 開發效率

- [ ] Generator watch mode（YAML 變更自動重新生成）
- [ ] 前端 hot reload 與 generated 檔案整合
- [ ] 錯誤提示改善（YAML 語法錯誤、型別不符等）
- [ ] 生成代碼的 lint 與格式化

---

## 九、風險與應對

| 風險 | 影響 | 應對策略 |
|------|------|----------|
| C# Generator 生成 TS/React 可能語法不夠自然 | 生成的前端代碼可讀性差 | 使用模板引擎（Scriban），模板本身可由 AI 優化 |
| shadcn/ui API 變更 | 生成的元件可能不相容 | shadcn/ui 是原始碼複製，版本鎖定在專案中 |
| 複雜表單需求超出 YAML 表達能力 | 無法純生成 | 支援 partial override：生成基礎版，手動檔案覆蓋特定行為 |
| 前後端 API 合約不一致 | 前端呼叫失敗 | 同一個 Generator 生成兩端，結構性保證一致 |

---

## 十、未來展望（不在當前範圍）

以下功能不在本計畫範圍內，但架構上應不阻礙未來擴展：

- **Mobile App 生成**（React Native，使用相同 types + api client）
- **即時通知**（WebSocket / SignalR）
- **工作流引擎**（YAML 定義業務流程 → 生成前端審批 UI）
- **多語系**（i18n，YAML 中的 label/caption 擴展為多語系 key）
- **AI Chat 介面**（直接用自然語言操作 CRUD）

---

## 附錄 A：YAML 欄位型別與前端元件映射

| YAML Type | TS Type | 表單元件 | 列表欄位 |
|-----------|---------|----------|----------|
| `string` | `string` | `<Input>` | 文字 |
| `string` (length > 500) | `string` | `<Textarea>` | 截斷文字 |
| `int` | `number` | `<Input type="number">` | 數字 |
| `decimal` | `number` | `<Input type="number" step="0.01">` | 格式化金額 |
| `bool` | `boolean` | `<Switch>` | Badge (是/否) |
| `datetime` | `string` | `<DatePicker>` | 格式化日期 |
| `guid` | `string` | 不顯示 | 不顯示 |
| `enum` | `enum` | `<Select>` | Badge (label) |
| `reference` | `Ref` | `<ReferenceSelect>` (Combobox) | 連結文字 |
| `detail` | — | 不在主表單 | 不在列表 |
| `formula` | readonly | 不在表單 | 格式化數值 |

---

## 附錄 B：Icon 映射表（XAF → Lucide）

Generator 需要一份 XAF icon 到 Lucide icon 的映射：

| XAF Icon | Lucide Icon |
|----------|-------------|
| `BO_Product` | `package` |
| `BO_Customer` | `users` |
| `BO_Order` | `shopping-cart` |
| `BO_Invoice` | `file-text` |
| `BO_Category` | `folder` |
| `BO_Report` | `bar-chart` |
| `BO_Settings` | `settings` |
| `BO_Calendar` | `calendar` |
| `BO_Task` | `check-square` |
| `BO_Note` | `sticky-note` |
| （預設） | `file` |

此映射表將內嵌於 `ReactPageGenerator` 中，可隨時擴充。
