# P4 權限控制 UI 實作計畫

## Context

目前 XekuII 前端已完成 P0-P3（CRUD 頁面生成），但所有 UI 操作按鈕（新增/編輯/刪除）對所有登入用戶無差別顯示。後端使用 XAF Security 的 `AddSecuredXpo`（Secured ObjectSpace 在資料層阻擋無權限操作），但前端不知道用戶的有效權限，用戶可能點了按鈕才收到錯誤。

**目標**：前端根據當前用戶的 XAF 有效權限，自動隱藏無權限的操作按鈕。Generator 自動生成權限感知 UI 程式碼。

**權限管理方式**：Default Role 的實體權限由系統管理員透過 RolesController API 手動設定，Generator 不自動生成 Updater 權限程式碼。

---

## 架構設計

```
XAF Security System (truth source)
        │
        ▼
GET /api/Permissions/my-permissions  ← 新增 API
        │  { "Product": { read, create, write, delete }, ... }
        ▼
usePermissionsStore (Zustand)  ← 新增
        │  登入後 fetch, 存入 store
        ▼
生成的頁面 (Generator 產出)  ← 修改
        │  useEntityPermissions("Product") → 條件渲染按鈕
        ▼
使用者看到符合其權限的 UI
```

---

## 步驟一：後端 - 新增 My Permissions API

### 新增 `XekuII.ApiHost/API/Security/PermissionsController.cs`

- 注入 `ISecurityProvider` + `IObjectSpaceFactory`
- 端點 `GET /api/Permissions/my-permissions`
- 掃描 `XekuII.Generated` namespace 下所有繼承 `XPBaseObject` 的 BO 類型
- 對每個類型呼叫 XAF `IsGrantedExtensions` 的 `CanRead/CanCreate/CanWrite/CanDelete`
- 回傳格式：`{ "Product": { read: true, create: true, write: true, delete: false }, ... }`

---

## 步驟二：前端 - 權限狀態管理

### 2.1 新增 `xekuii-web/src/lib/permissions.ts`

Zustand store，包含：
- `permissions: Record<string, EntityPermissions>` - 權限快取
- `loaded: boolean` - 是否已載入
- `fetchPermissions()` - 呼叫 API 取得權限
- `clear()` - 清除（登出時用）
- `can(entity, action)` - 便捷查詢方法

### 2.2 新增 `xekuii-web/src/hooks/usePermissions.ts`

便捷 hook：
```typescript
export function useEntityPermissions(entity: string) {
  const can = usePermissionsStore((s) => s.can);
  return {
    canRead: can(entity, "read"),
    canCreate: can(entity, "create"),
    canUpdate: can(entity, "write"),
    canDelete: can(entity, "delete"),
  };
}
```

### 2.3 修改 `xekuii-web/src/lib/auth.ts`

- `login()` 成功後呼叫 `usePermissionsStore.getState().fetchPermissions()`
- `logout()` 時呼叫 `usePermissionsStore.getState().clear()`

### 2.4 修改 `xekuii-web/src/lib/api-client.ts`

- Response interceptor 新增 403 處理（toast 提示，不登出）

### 2.5 修改 `xekuii-web/src/lib/types.ts`

NavItem 新增可選 `entity` 欄位：
```typescript
export interface NavItem {
  label: string;
  path: string;
  icon: string;
  entity?: string;  // 新增
}
```

---

## 步驟三：前端 - Sidebar 權限過濾 & App 初始化

### 3.1 修改 `xekuii-web/src/components/layout/Sidebar.tsx`

- import `usePermissionsStore`
- 過濾 navItems：只顯示無 `entity` 的項目（手動項目）或 `can(entity, "read")` 為 true 的項目

### 3.2 修改 `xekuii-web/src/App.tsx`

- `useEffect` 中：如果 token 存在，同時 `fetchPermissions()`

---

## 步驟四：Generator 修改 - 生成權限感知頁面

### 4.1 修改 `ReactListPageGenerator.cs`

- 新增 import `useEntityPermissions`
- 元件開頭：`const { canCreate, canUpdate, canDelete } = useEntityPermissions("Product");`
- `New` 按鈕：`{canCreate && <Button>...New</Button>}`
- RowActions `Edit`：`{canUpdate && <Tooltip>...Edit</Tooltip>}`
- RowActions `Delete`：`{canDelete && <Tooltip>...Delete</Tooltip>}`
- `deleteMutation` + `ConfirmDialog`：用 `canDelete` 條件包裹

### 4.2 修改 `ReactFormPageGenerator.cs`

- 新增 import `useEntityPermissions` + `useEffect`
- 權限路由保護：新增模式檢查 `canCreate`、編輯模式檢查 `canUpdate`
- 無權限時 `useEffect` 自動 `navigate` 回列表頁

### 4.3 修改 `ReactDetailPageGenerator.cs`

- 新增 import `useEntityPermissions`
- `Edit` 按鈕：`{canUpdate && ...}`
- `Delete` 按鈕：`{canDelete && ...}`

### 4.4 修改 `ReactMetadataGenerator.cs`

`GenerateNavigation()` 方法中，每個 navItem 加入 `entity` 欄位：
```typescript
{ label: "產品", path: "/products", icon: "Circle", entity: "Product" },
```

---

## 涉及的檔案清單

| 分類 | 檔案 | 動作 |
|------|------|------|
| 後端 | `XekuII.ApiHost/API/Security/PermissionsController.cs` | **新增** |
| 前端 | `xekuii-web/src/lib/permissions.ts` | **新增** |
| 前端 | `xekuii-web/src/hooks/usePermissions.ts` | **新增** |
| 前端 | `xekuii-web/src/lib/auth.ts` | 修改 |
| 前端 | `xekuii-web/src/lib/api-client.ts` | 修改 |
| 前端 | `xekuii-web/src/lib/types.ts` | 修改 |
| 前端 | `xekuii-web/src/components/layout/Sidebar.tsx` | 修改 |
| 前端 | `xekuii-web/src/App.tsx` | 修改 |
| Generator | `XekuII.Generator/Generators/ReactListPageGenerator.cs` | 修改 |
| Generator | `XekuII.Generator/Generators/ReactFormPageGenerator.cs` | 修改 |
| Generator | `XekuII.Generator/Generators/ReactDetailPageGenerator.cs` | 修改 |
| Generator | `XekuII.Generator/Generators/ReactMetadataGenerator.cs` | 修改 |

---

## 驗證計畫

1. **建置後端**：`dotnet build XekuII.ApiHost`
2. **重新生成**：
   ```powershell
   dotnet run --project XekuII.Generator -- ./entities `
     --output ./XekuII.ApiHost/BusinessObjects `
     --controllers ./XekuII.ApiHost/API `
     --frontend ./xekuii-web/src/generated
   ```
3. **前端建置**：`cd xekuii-web && npm run build`（確認無 TS 錯誤）
4. **啟動服務**：`dotnet run --project XekuII.CLI -- start`
5. **API 測試**：
   - Admin 登入 → `GET /api/Permissions/my-permissions` → 所有權限皆 true
   - User 登入 → 確認回傳依 Default Role 的實際權限
6. **UI 驗證**：
   - Admin：看到所有操作按鈕、所有導航項目
   - 權限受限的用戶：列表頁不顯示 Delete/Edit 按鈕、詳情頁不顯示 Delete/Edit 按鈕
   - 直接訪問無 create 權限的 `/products/new` → 自動重導至列表頁
