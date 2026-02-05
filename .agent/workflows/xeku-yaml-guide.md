---
description: XekuII 實體定義 YAML 撰寫指南
---

# XekuII YAML 指南

在 `entities/` 目錄下建立 `.xeku.yaml` 檔案。

## 範例：Customer.xeku.yaml

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

## 支援型別
- string
- int
- decimal
- double
- bool
- datetime
- guid

## 關聯 (Relations)
- type: reference (Many-to-One)
- type: detail (One-to-Many)