---
description: XekuII YAML å¯¦é?å®šç¾©å®Œæ•´?‡å?ï¼Œå??«æ??‰æ”¯?´ç?æ¬„ä??é??¯ã€Enum?è?ç®—å±¬?§ã€é?è­‰è???
---

# XekuII YAML å¯¦é?å®šç¾©?‡å?

?¬æ??—èªª?å?ä½•ä½¿??`.XekuII.yaml` ?¼å?å®šç¾©å¯¦é?ï¼ŒGenerator ?ƒè‡ª?•ç”¢??C# Business Object ??REST API Controller??

---

## ?? å®Œæ•´ YAML çµæ?

```yaml
# å¯¦é??ºæœ¬è³‡è?
entity: EntityName           # å¯¦é??ç¨± (PascalCase)
caption: "é¡¯ç¤º?ç¨±"           # UI é¡¯ç¤º?ç¨±
icon: "BO_EntityName"        # XAF ?–ç¤º?ç¨±
description: "å¯¦é??¨é€”èªª??   # AI Agent èªæ??†è§£??

# Enum å®šç¾©ï¼ˆå¯?¸ï?
enums:
  - name: EnumName
    description: "Enum ?¨é€”èªª??
    members:
      - name: MemberName
        value: 0
        label: "é¡¯ç¤ºæ¨™ç±¤"
        description: "?¸é?èªªæ?"

# æ¬„ä?å®šç¾©
fields:
  - name: FieldName
    type: string
    # ... æ¬„ä?å±¬æ€?

# ?œè¯å®šç¾©
relations:
  - name: RelationName
    type: reference
    target: TargetEntity
    # ... ?œè¯å±¬æ€?
```

---

## ?? æ¬„ä?é¡å?å°ç…§

| YAML Type | C# Type | èªªæ? |
|:---|:---|:---|
| `string` | `string` | ?‡å? |
| `int` | `int` | ?´æ•¸ |
| `decimal` | `decimal` | ç²¾ç¢ºå°æ•¸ï¼ˆé?é¡ç”¨ï¼?|
| `datetime` | `DateTime` | ?¥æ??‚é? |
| `bool` | `bool` | å¸ƒæ???|
| `guid` | `Guid` | ?¯ä?è­˜åˆ¥ç¢?|
| `{EnumName}` | `{EnumName}` | ?ªè? Enum é¡å? |

---

## ?? æ¬„ä?å±¬æ€§å??´å?è¡?

```yaml
fields:
  - name: FieldName          # å¿…å¡«ï¼šæ?ä½å?ç¨?(PascalCase)
    type: string             # å¿…å¡«ï¼šè??™é???

    # ?ºæœ¬å±¬æ€?
    length: 100              # å­—ä¸²?·åº¦?åˆ¶
    required: true           # ?¯å¦å¿…å¡« ??[RuleRequiredField]
    readonly: true           # ?¯å¦?¯è? ??[ModelDefault("AllowEdit", "False")]
    label: "é¡¯ç¤ºæ¨™ç±¤"         # UI æ¨™ç±¤ ??[XafDisplayName]
    description: "æ¬„ä?èªªæ?"   # AI èªæ? ??[Description]
    default: "value"         # ?è¨­????AfterConstruction()

    # è¨ˆç?å±¬æ€§ï??‡ä?ï¼?
    formula: "[Quantity] * [UnitPrice]"  # è¨ˆç??¬å?
    calculation_type: persistent         # persistent=è³‡æ?åº«å±¤, getter=?‰ç”¨å±?

    # é©—è?è¦å?
    validations:
      - min: 0               # ?€å°å€?
        max: 100             # ?€å¤§å€?
        message: "?¯èª¤è¨Šæ¯"
      - range: ">=0"         # ç¯„å?è¡¨é?å¼?(>N, >=N, <N, <=N, N-M)
        message: "?¯èª¤è¨Šæ¯"
      - regex: "^[A-Z]{2}[0-9]{8}$"  # æ­??è¡¨é?å¼?
        message: "?¼å??¯èª¤"
      - criteria: "[Status] != 'Cancelled' OR [CancelReason] IS NOT NULL"
        message: "æ¢ä»¶?ªæ»¿è¶?
```

---

## ?? ?œè¯é¡å?

### Referenceï¼ˆå?å°ä?ï¼?

```yaml
relations:
  - name: Customer           # å±¬æ€§å?ç¨?
    type: reference          # ?œè¯é¡å?
    target: Customer         # ?®æ?å¯¦é?
    required: true           # ?¯å¦å¿…å¡«
    label: "å®¢æˆ¶"            # UI æ¨™ç±¤
    description: "ä¸‹è??®ç?å®¢æˆ¶"  # AI èªæ?
```

**?¢ç?çµæ?ï¼?*
- `private Customer _customer;`
- `[Association("Order-Customer")]`
- `public Customer Customer { get; set; }`

### Detailï¼ˆä?å°å? / Master-Detailï¼?

```yaml
relations:
  - name: Items              # ?†å?å±¬æ€§å?ç¨?
    type: detail             # ?œè¯é¡å?
    target: OrderItem        # å­é??®å¯¦é«?
    label: "?ç´°"
    description: "è¨‚å–®?…å«?„ç”¢?æ?ç´?
```

**?¢ç?çµæ?ï¼?*
- `[Association("Order-Items"), Aggregated]`
- `public XPCollection<OrderItem> Items { get; }`

---

## ?¯ Enum å®šç¾©

```yaml
enums:
  - name: OrderStatus
    description: "è¨‚å–®?•ç??€??
    members:
      - name: Draft
        value: 0
        label: "?‰ç¨¿"
        description: "è¨‚å–®å°šæœª?äº¤ï¼Œå¯?ªç”±ç·¨è¼¯"
      - name: Submitted
        value: 1
        label: "å·²æ?äº?
        description: "è¨‚å–®å·²æ?äº¤ï?ç­‰å?å¯©æ ¸"
      - name: Approved
        value: 2
        label: "å·²æ ¸??
        description: "è¨‚å–®å·²é€šé?å¯©æ ¸"
      - name: Cancelled
        value: 9
        label: "å·²å?æ¶?
        description: "è¨‚å–®å·²å?æ¶?

fields:
  - name: Status
    type: OrderStatus        # ?´æ¥ä½¿ç”¨ Enum ?ç¨±
    required: true
    label: "?€??
```

**?¢ç?çµæ?ï¼?*
```csharp
[Description("è¨‚å–®?•ç??€??)]
public enum OrderStatus
{
    [Description("è¨‚å–®å°šæœª?äº¤ï¼Œå¯?ªç”±ç·¨è¼¯")]
    [XafDisplayName("?‰ç¨¿")]
    Draft = 0,
    // ...
}
```

---

## ?? è¨ˆç?å±¬æ€?

### Persistentï¼ˆè??™åº«å±¤ç?ï¼?

```yaml
- name: Amount
  type: decimal
  label: "?‘é?"
  description: "?¸é?ä¹˜ä»¥?®åƒ¹?„é?é¡?
  formula: "[Quantity] * [UnitPrice]"
  calculation_type: persistent
```

**?¢ç?çµæ?ï¼?*
```csharp
[PersistentAlias("[Quantity] * [UnitPrice]")]
public decimal Amount => (decimal)EvaluateAlias(nameof(Amount));
```

- ???¯åœ¨è³‡æ?åº«å±¤ç´šæ?åºã€ç¯©??
- ???ˆèƒ½è¼ƒå¥½

### Getterï¼ˆæ??¨å±¤ç´šï?

```yaml
- name: DisplayName
  type: string
  label: "é¡¯ç¤º?ç¨±"
  formula: "FirstName + \" \" + LastName"
  calculation_type: getter
```

**?¢ç?çµæ?ï¼?*
```csharp
[NonPersistent]
public string DisplayName => FirstName + " " + LastName;
```

- ???¯ä½¿?¨ä»»??C# è¡¨é?å¼?
- ???¡æ??¨è??™åº«å±¤ç??ä?

---

## ??é©—è?è¦å?

### ?¸å€¼ç???

```yaml
validations:
  # ä½¿ç”¨ min/max
  - min: 0.01
    message: "?¸é?å¿…é?å¤§æ–¼ 0"
  - max: 9999999.99
    message: "?‘é?è¶…å‡ºç¯„å?"

  # ä½¿ç”¨ range è¡¨é?å¼?
  - range: ">=0"
    message: "ä¸å¯?ºè???
  - range: "0-100"
    message: "å¿…é???0-100 ä¹‹é?"
```

### æ­??è¡¨é?å¼?

```yaml
validations:
  - regex: "^[A-Z]{2}[0-9]{8}$"
    message: "ç·¨è??¼å??¯èª¤ï¼ˆç?ä¾‹ï?SO20260101ï¼?
  - regex: "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$"
    message: "è«‹è¼¸?¥æ??ˆç? Email"
```

### XAF Criteria

```yaml
validations:
  - criteria: "[EndDate] >= [StartDate]"
    message: "çµæ??¥æ?å¿…é??šæ–¼?‹å??¥æ?"
```

---

## ?¨ ?è¨­??

ä½¿ç”¨ `default` å±¬æ€§è¨­å®šæ?ä½ç??å??¼ï??ƒåœ¨ `AfterConstruction()` ?¹æ?ä¸­è‡ª?•è¨­å®šã€?

### ?ºæœ¬é¡å?

```yaml
fields:
  # å¸ƒæ???
  - name: IsActive
    type: bool
    default: "true"

  # ?´æ•¸
  - name: Quantity
    type: int
    default: "1"

  # å°æ•¸
  - name: Discount
    type: decimal
    default: "0"

  # å­—ä¸²
  - name: Status
    type: string
    default: "New"
```

### DateTime ?è¨­??

```yaml
fields:
  # ?¶å??‚é?
  - name: CreatedDate
    type: datetime
    default: "DateTime.Now"

  # ä»Šå¤©?¥æ?
  - name: OrderDate
    type: datetime
    default: "DateTime.Today"

  # UTC ?‚é?
  - name: Timestamp
    type: datetime
    default: "DateTime.UtcNow"
```

### Enum ?è¨­??

```yaml
fields:
  - name: Status
    type: OrderStatus
    default: "Draft"          # ?ªå?è½‰æ???OrderStatus.Draft
```

**?¢ç?çµæ?ï¼?*
```csharp
public override void AfterConstruction()
{
    base.AfterConstruction();
    IsActive = true;
    CreatedDate = DateTime.Now;
    Status = OrderStatus.Draft;
}
```

---

## ?? å®Œæ•´ç¯„ä?

### Order.XekuII.yaml

```yaml
entity: Order
caption: "?·å”®è¨‚å–®"
icon: "BO_Order"
description: "?·å”®è¨‚å–®ä¸»æ?ï¼Œè??„å®¢?¶è?è³¼è?è¨?

enums:
  - name: OrderStatus
    description: "è¨‚å–®?•ç??€??
    members:
      - name: Draft
        value: 0
        label: "?‰ç¨¿"
        description: "è¨‚å–®å°šæœª?äº¤"
      - name: Submitted
        value: 1
        label: "å·²æ?äº?
        description: "è¨‚å–®å·²æ?äº?
      - name: Approved
        value: 2
        label: "å·²æ ¸??
        description: "è¨‚å–®å·²æ ¸??
      - name: Cancelled
        value: 9
        label: "å·²å?æ¶?
        description: "è¨‚å–®å·²å?æ¶?

fields:
  - name: OrderNo
    type: string
    length: 20
    required: true
    label: "è¨‚å–®ç·¨è?"
    description: "?¯ä??„è??®è??¥ç·¨??
    validations:
      - regex: "^[A-Z]{2}[0-9]{8}$"
        message: "?¼å??¯èª¤ï¼ˆç?ä¾‹ï?SO20260101ï¼?

  - name: OrderDate
    type: datetime
    required: true
    label: "è¨‚å–®?¥æ?"
    description: "è¨‚å–®å»ºç??¥æ?"
    default: "DateTime.Now"

  - name: Status
    type: OrderStatus
    required: true
    label: "?€??
    description: "?®å?è¨‚å–®?•ç??€??
    default: "Draft"

  - name: TotalAmount
    type: decimal
    label: "ç¸½é?é¡?
    description: "è¨‚å–®?ç´°?‘é?ç¸½å?"
    formula: "Items.Sum(Amount)"
    calculation_type: persistent

  - name: Notes
    type: string
    length: 500
    label: "?™è¨»"
    description: "è¨‚å–®è£œå?èªªæ?"

relations:
  - name: Customer
    type: reference
    target: Customer
    required: true
    label: "å®¢æˆ¶"
    description: "ä¸‹è??®ç?å®¢æˆ¶"

  - name: Items
    type: detail
    target: OrderItem
    label: "?ç´°"
    description: "è¨‚å–®?¢å??ç´°"
```

---

## ?? ?¢ç?ç¨‹å?ç¢?

```bash
dotnet run --project XekuII.Generator -- ./entities --output ./XekuII.Generated --controllers ./XekuII.Backend/API
```

**?¢ç?çµæ?ï¼?*
- `XekuII.Generated/Order.Generated.cs` - Business Object + Enum
- `XekuII.Backend/API/OrdersController.Generated.cs` - REST API + DTO

---

## ?? ?¸é?è³‡æ?

| è³‡æ? | è·¯å? |
|:---|:---|
| Generator ä¸»ç?å¼?| `XekuII.Generator/Program.cs` |
| BO ?¢ç???| `XekuII.Generator/Generators/CSharpCodeGenerator.cs` |
| Controller ?¢ç???| `XekuII.Generator/Generators/ControllerCodeGenerator.cs` |
| ?¾æ?å¯¦é?ç¯„ä? | `entities/*.XekuII.yaml` |
| å°ˆæ?è¦æ ¼??| `XekuIINextGen_Plan.md` |
