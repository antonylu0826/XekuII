# XekuII Next Generation Framework

**XekuII** is a YAML-driven, AI-native application framework built on **DevExpress XAF** and **.NET 8**. Define your data models in YAML, run the generator, and get production-ready C# Business Objects, REST API Controllers, and DTOs — all in one step.

[繁體中文說明](README.zh-Hant.md)

---

## Architecture

```
                                    ┌─── C# XPO Business Objects (.Generated.cs)
entities/*.xeku.yaml ──→ Generator ─┤
                                    └─── ASP.NET Core API Controllers (.Generated.cs)
                                              ├── CRUD Endpoints
                                              ├── Detail (Master-Detail) Endpoints
                                              └── Typed DTOs
```

**Core philosophy**: Write YAML once, generate everything. Customize via `partial class` — never touch generated files.

---

## Features

- **YAML-Driven Development** — Define entities, fields, relations, validations, enums, calculated fields, default values, and business rules in `.xeku.yaml` files.
- **Intelligent Code Generation** — Automatically generates XPO Business Objects with full attribute decoration, REST API Controllers with CRUD + detail endpoints, and typed DTOs.
- **Smart Relationship Analysis** — Detects paired reference/detail relationships across entities and auto-generates reverse associations with correct `AssociationName` pairing.
- **Rich Validation Support** — Range (`>0`, `>=1`, `0-100`), min/max, regex, XAF criteria expressions, and custom error messages — all from YAML.
- **Calculated Fields** — PersistentAlias (XPO criteria) or getter-only (C# expression) computed properties.
- **Enum Generation** — Entity-scoped enums with `[Description]` and `[XafDisplayName]` attributes for AI and UI readability.
- **AI-Native Design** — All entities, fields, relations, and enums include `[Description]` attributes, making the API self-documenting for AI agents.
- **Enterprise Security** — XAF Security System with JWT Bearer authentication, role-based access control (RBAC), and object-level permissions.

---

## Project Structure

```
XekuII/
├── entities/                        # YAML entity definitions (input)
├── XekuII.Generator/               # Code generation engine
│   ├── Models/                     #   EntityDefinition, FieldDefinition, etc.
│   ├── Parsers/                    #   YamlEntityParser
│   └── Generators/                 #   CSharpCodeGenerator, ControllerCodeGenerator
├── XekuII.ApiHost/                  # ASP.NET Core backend (XAF + Web API)
│   ├── BusinessObjects/            #   Generated XPO entities + manual partial classes
│   ├── API/                        #   Generated controllers + Security controllers
│   └── DatabaseUpdate/             #   Seed data and migrations
├── XekuII.CLI/                      # CLI wrapper for the generator
├── .agent/                          # AI agent skill guides and workflows
│   ├── SKILL.md                    #   Core development reference
│   └── workflows/                  #   Step-by-step workflow guides
└── CLAUDE.md                        # AI assistant project instructions
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ApiHost) / [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Generator, CLI)
- [DevExpress Universal Subscription](https://www.devexpress.com/) v25.2.3 (XAF, XPO)
- SQL Server LocalDB (default) or any SQL Server instance
- A text editor or IDE (VS Code, Visual Studio, Rider)

---

## Quick Start

### 1. Define an Entity

Create `entities/Customer.xeku.yaml`:

```yaml
entity: Customer
caption: Customer
icon: BO_Customer
description: Customer contact information management

fields:
  - name: Name
    type: string
    required: true
    label: Customer Name
    description: Full name of the customer

  - name: Email
    type: string
    label: Email Address
    validations:
      - regex: "^[\\w.-]+@[\\w.-]+\\.\\w+$"
        message: "Invalid email format"

  - name: Phone
    type: string
    label: Phone Number

  - name: IsActive
    type: bool
    label: Active
    default: "true"
    description: Whether the customer account is active

  - name: CreditLimit
    type: decimal
    label: Credit Limit
    default: "0"
    validations:
      - range: ">=0"
        message: "Credit limit cannot be negative"

relations:
  - name: Category
    type: reference
    target: CustomerCategory
    label: Category
    description: Customer classification

  - name: Orders
    type: detail
    target: Order
    label: Orders
    description: Orders placed by this customer
```

### 2. Generate Code

```powershell
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API
```

This produces:
- `XekuII.ApiHost/BusinessObjects/Customer.Generated.cs` — XPO Business Object
- `XekuII.ApiHost/API/CustomersController.Generated.cs` — REST API Controller + DTOs

### 3. Update Database

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj `
  -- --updateDatabase --forceUpdate --silent
```

### 4. Run the Application

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj
```

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

---

## YAML Schema Reference

### Entity-Level Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `entity` | string | Yes | Entity name (becomes C# class name, PascalCase) |
| `caption` | string | No | UI display name (`[XafDisplayName]`) |
| `icon` | string | No | XAF icon name (`[ImageName]`) |
| `dbTable` | string | No | Custom database table name |
| `description` | string | No | AI-readable description (`[Description]`) |
| `fields` | list | Yes | Field definitions |
| `relations` | list | No | Relation definitions |
| `rules` | list | No | Business rule triggers |
| `enums` | list | No | Enum type definitions |

### Field Definition

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `name` | string | Yes | Field name (PascalCase) |
| `type` | string | Yes | Data type (see type mapping below) |
| `required` | bool | No | Adds `[RuleRequiredField]` |
| `readonly` | bool | No | `[ModelDefault("AllowEdit", "False")]` |
| `length` | int | No | String max length (`[Size(n)]`) |
| `label` | string | No | UI display name (`[XafDisplayName]`) |
| `default` | string | No | Default value (set in `AfterConstruction`) |
| `description` | string | No | AI-readable description (`[Description]`) |
| `formula` | string | No | Calculated field expression |
| `calculationType` | string | No | `persistent` (PersistentAlias) or `getter` (C# property) |
| `validations` | list | No | Validation rule definitions |

### Type Mapping

| YAML Type | C# Type | Description |
|-----------|---------|-------------|
| `string` | `string` | Text (default length 100) |
| `int` | `int` | Integer |
| `decimal` | `decimal` | High-precision number |
| `double` | `double` | Floating-point number |
| `bool` | `bool` | Boolean |
| `datetime` | `DateTime` | Date and time |
| `guid` | `Guid` | Globally unique identifier |
| `{EnumName}` | `{EnumName}` | Entity-scoped enum type |

### Validation Rules

```yaml
validations:
  - range: ">0"                    # Greater than 0
  - range: ">=1"                   # Greater than or equal to 1
  - range: "<=100"                 # Less than or equal to 100
  - range: "0-100"                 # Between 0 and 100 (inclusive)
  - min: 5                         # Minimum value
  - max: 999                       # Maximum value
  - regex: "^[A-Z]{2}-\\d{4}$"    # Regex pattern
  - criteria: "[Status] != 'Closed'"  # XAF criteria expression
  - message: "Custom error text"   # Custom error message (per rule)
```

### Relation Definition

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `name` | string | Yes | Relation property name |
| `type` | string | Yes | `reference` (Many-to-One) or `detail` (One-to-Many, aggregated) |
| `target` | string | Yes | Target entity name |
| `required` | bool | No | Adds `[RuleRequiredField]` (reference only) |
| `label` | string | No | UI display name |
| `lookupField` | string | No | Field displayed in lookups |
| `cascade` | string | No | Cascade behavior |
| `description` | string | No | AI-readable description |

### Enum Definition

```yaml
enums:
  - name: OrderStatus
    description: Order processing lifecycle status
    members:
      - name: Pending
        value: 0
        label: Pending
        description: Order awaiting processing
      - name: Confirmed
        value: 1
        label: Confirmed
      - name: Shipped
        value: 2
        label: Shipped
      - name: Completed
        value: 3
        label: Completed
```

### Calculated Fields

```yaml
fields:
  # PersistentAlias (XPO criteria syntax, stored in DB)
  - name: Total
    type: decimal
    formula: "[Quantity] * [UnitPrice]"
    calculationType: persistent

  # Getter (C# expression, computed at runtime)
  - name: FullName
    type: string
    formula: "FirstName + \" \" + LastName"
    calculationType: getter
```

### Default Values

```yaml
fields:
  - name: CreatedDate
    type: datetime
    default: "now"           # DateTime.Now (also: today, utcnow)

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

### Business Rules

```yaml
rules:
  - trigger: BeforeSave
    script: ValidateOrder     # Generates: partial void ValidateOrder();
                              # Called from OnSaving() override
```

Implement the partial method in a non-generated file:

```csharp
// BusinessObjects/Order.cs
public partial class Order
{
    partial void ValidateOrder()
    {
        if (Items.Count == 0)
            throw new UserFriendlyException("Order must have at least one item.");
    }
}
```

---

## Generated API Endpoints

For each entity (e.g., `Customer`), the generator creates:

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/customers` | List all (with reference IDs and names) |
| `GET` | `/api/customers/{id}` | Get by ID |
| `GET` | `/api/customers/{id}/details` | Get with full relation data |
| `POST` | `/api/customers` | Create |
| `PUT` | `/api/customers/{id}` | Update |
| `DELETE` | `/api/customers/{id}` | Delete |

For each `detail` relation (e.g., `Orders`):

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/customers/{id}/orders` | List detail items |
| `POST` | `/api/customers/{id}/orders` | Add detail item |
| `PUT` | `/api/customers/{id}/orders/{itemId}` | Update detail item |
| `DELETE` | `/api/customers/{id}/orders/{itemId}` | Delete detail item |

All endpoints require JWT Bearer authentication (`[Authorize]`).

---

## Authentication

```powershell
# Get JWT token
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/Authentication/Authenticate" `
  -Method Post -ContentType "application/json" `
  -Body '{"userName":"Admin","password":""}'

$token = $response.token

# Use token in requests
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/customers" -Headers $headers
```

Default users (created by `DatabaseUpdate/Updater.cs`):
- **Admin** (password: empty) — `Administrators` role
- **User** (password: empty) — `Default` role

---

## Customization via Partial Classes

Generated files (`*.Generated.cs`) must never be edited manually. Extend behavior by creating a separate file with the same class name:

```csharp
// BusinessObjects/Customer.cs (manual, not generated)
public partial class Customer
{
    protected override void OnSaving()
    {
        base.OnSaving();
        // Custom save logic
    }

    public string GetFormattedAddress()
    {
        return $"{City}, {Country}";
    }
}
```

---

## Clean & Regenerate

Before regenerating, remove stale generated files:

```powershell
# Remove generated Business Objects
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue

# Remove generated Controllers
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue

# Regenerate
dotnet run --project XekuII.Generator -- ./entities `
  --output ./XekuII.ApiHost/BusinessObjects `
  --controllers ./XekuII.ApiHost/API
```

---

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | .NET 8.0 / .NET 10.0 | 8.0 (ApiHost) / 10.0 (Generator, CLI) |
| ORM | DevExpress XPO | 25.2.3 |
| Application Framework | DevExpress XAF | 25.2.3 |
| API | ASP.NET Core Web API | 8.0 |
| Authentication | JWT + XAF Security System | — |
| YAML Parsing | YamlDotNet | 16.3.0 |
| API Docs | Swagger / OpenAPI (Swashbuckle) | 6.9.0 |
| Database | SQL Server LocalDB | — |

---

## License

Private Project. All rights reserved.
