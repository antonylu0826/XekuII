---
description: XekuII Generator ?‹ç™¼?‡å?ï¼Œå???API ?¢ç??å¯¦é«”å?ç¾©ã€relations ?•ç?
---

# XekuII Generator å®Œæ•´å·¥ä?æµç?

?¬æ??—èªª?å?ä½•å??¶é?å§‹ä½¿??XekuII.Generator å»ºç?å®Œæ•´??CRUD ?‰ç”¨ç¨‹å???

---

## ?? å·¥ä?æµç?ç¸½è¦½

1. **å®šç¾©å¯¦é?** - å»ºç? `.XekuII.yaml` æª”æ?
2. **?¢ç?ç¨‹å?ç¢?* - ?·è? Generator ?¢ç? BO ??Controller
3. **å®‰å…¨?ç½®** - ç¢ºè? JWT èªè??‡æ?æ¬Šè¨­å®?
4. **?´å? API** - ?ºæ–¼ IObjectSpaceFactory ?´å??Ÿèƒ½
5. **é©—è?æ¸¬è©¦** - å»ºç½®ä¸¦æ¸¬è©¦å???

---

## Step 1: å®šç¾©å¯¦é?

??`entities/` è³‡æ?å¤¾å»ºç«?`.XekuII.yaml` æª”æ?ï¼?

```yaml
# entities/Customer.XekuII.yaml
entity: Customer
caption: "å®¢æˆ¶"
icon: "BO_Customer"

fields:
  - name: Code
    type: string
    length: 20
    required: true
    label: "å®¢æˆ¶ç·¨è?"
  - name: Name
    type: string
    length: 100
    required: true
    label: "å®¢æˆ¶?ç¨±"
  - name: IsActive
    type: bool
    label: "?Ÿç”¨"
```

### æ¬„ä?é¡å?å°ç…§

| YAML Type | C# Type |
|:---|:---|
| `string` | `string` |
| `int` | `int` |
| `decimal` | `decimal` |
| `datetime` | `DateTime` |
| `bool` | `bool` |
| `guid` | `Guid` |

### ?œè¯é¡å?

```yaml
relations:
  # å¤–éµ?ƒè€ƒï?å¤šå?ä¸€ï¼?
  - name: Customer
    type: reference
    target: Customer
    required: true
    label: "å®¢æˆ¶"

  # Master-Detailï¼ˆä?å°å?ï¼?
  - name: Items
    type: detail
    target: OrderItem
    label: "?ç´°"
    cascade: delete
```

---

## Step 2: ?·è? Generator

// turbo
```bash
cd XekuII.Generator
dotnet run -- entities --output XekuII.ApiHost/BusinessObjects --controllers XekuII.ApiHost/API --namespace XekuII.ApiHost.BusinessObjects
```

Generator ?ƒç”¢?Ÿï?
- `XekuII.ApiHost/BusinessObjects/{Entity}.Generated.cs` - XAF/XPO Business Object
- `XekuII.ApiHost/API/{Entity}sController.Generated.cs` - REST API Controller

---

## Step 3: å®‰å…¨?‡åŸºç¤é?ç½?

### 3.1 JWT å®‰å…¨æ©Ÿåˆ¶ (?è?)

?¢ç???Controller ?è¨­æ¨™è?äº?`[Authorize]` å±¬æ€§ã€‚ç‚ºäº†è?æ­????`IObjectSpaceFactory` ?‹ä?ï¼Œå??ˆç¢ºä¿å‘¼??API ?‚å¸¶?‰æ??ˆç? JWT Token??

> [!IMPORTANT]
> **?ºä?éº¼é?è¦?[Authorize]ï¼?*
> XAF ??`IObjectSpaceFactory.CreateObjectSpace<T>()` ?€è¦?Security Context ?èƒ½?‹ä??‚å??œæ???`[Authorize]` ?–æœª?ä? Tokenï¼Œæ??ºç¾ `400 Error: The user name must not be empty`??

### 3.2 JSON ?½å????

ç¢ºè? `Startup.cs` è¨­å???camelCase ä»¥åŒ¹??Web ?ç«¯???ï¼?

```csharp
services.Configure<JsonOptions>(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

---

## Step 4: ?•ç? Relations ?‡å??½æ“´??

### 4.1 ä½¿ç”¨ IObjectSpaceFactory

?¨æ“´??Controller ?‚ï??‰æ³¨?¥ä¸¦ä½¿ç”¨ `IObjectSpaceFactory`??

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomController : ControllerBase {
    private readonly IObjectSpaceFactory _objectSpaceFactory;

    public CustomController(IObjectSpaceFactory objectSpaceFactory) {
        _objectSpaceFactory = objectSpaceFactory;
    }

    [HttpGet]
    public IActionResult MyAction() {
        using var objectSpace = _objectSpaceFactory.CreateObjectSpace<MyEntity>();
        // ... ?·è??è¼¯
        return Ok();
    }
}
```

### 4.2 Detail é¡å?ï¼ˆMaster-Detailï¼?

?¢ç???Controller å·²ç??…å«äº†ç´°ç¯€è³‡æ???CRUD ç«¯é?ï¼?

```
GET  /api/{entity}/{id}/details     - ?–å?å®Œæ•´è³‡æ??«å??…ç›®
GET  /api/{entity}/{id}/{relation}  - ?–å?å­é??®å?è¡?
POST /api/{entity}/{id}/{relation}  - ?°å?å­é???
PUT  /api/{entity}/{id}/{relation}/{itemId} - ?´æ–°å­é???
DELETE /api/{entity}/{id}/{relation}/{itemId} - ?ªé™¤å­é???
```

---

## Step 5: é©—è?æ¸¬è©¦

### 5.1 ?·è?å»ºç½®é©—è?
// turbo
```bash
# API Server å»ºç½®
dotnet build XekuII.ApiHost/XekuII.ApiHost.csproj
```

---

## ?? ?ƒè€ƒæ?æ¡?

| é¡å? | è·¯å? |
|:---|:---|
| Generator ä¸»ç?å¼?| `XekuII.Generator/Program.cs` (CLI Entry) |
| Controller ?¢ç???| `XekuII.Generator/Generators/ControllerCodeGenerator.cs` |
| BO ?¢ç???| `XekuII.Generator/Generators/CSharpCodeGenerator.cs` |
| æ¸…ç??‡ä»¤ | `.agent/workflows/XekuII-cleanup.md` |

---

## ? ï? å·²çŸ¥?åˆ¶

| ?…ç›® | ?€??| èªªæ? |
|:---|:---:|:---|
| Generator relations (reference) | ??| å·²æ”¯?´è‡ª?•ç”¢??(??ID/Name) |
| Generator relations (detail) | ??| å·²æ”¯?´ç”¢??CRUD ç«¯é???DTO |
| é©—è?è¦å? | ??| å·²æ”¯?´åŸº??Validation Rules (Required, Range, Regex ç­? |
| ?è¨­??| ??| å·²æ”¯?´é€é? `default:` è¨­å? (??`DateTime.Now`) |

---

## ?? ?ªä?å¢å¼·

- [ ] ?¯æ´?ªè?æ¬„ä?é¡å?
- [ ] ?¢ç? OpenAPI / Swagger ?‡ä»¶ç´°ç??´å?
- [ ] ?¯æ´ YAML å®šç¾©è¨ˆç?æ¬„ä??¬å? (PersistentAlias)
