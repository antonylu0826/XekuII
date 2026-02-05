---
description: æ¸…ç©º?¢ç??„ç?å¼ç¢¼ï¼Œä??™æ??¶æ¶æ§‹ï??¨æ–¼?æ–°æ¸¬è©¦ Generator
---

# XekuII æ¸…ç©º?¢ç?æª”æ?

?¬æ??—èªª?å?ä½•æ?ç©?Generator ?¢ç??„æ?æ¡ˆï?ä¿ç?æ©Ÿåˆ¶?¶æ?ä»¥ä¾¿?æ–°æ¸¬è©¦??

---

## å¿«é€Ÿæ?ç©ºæ?ä»?

// turbo-all

### Step 1: æ¸…ç©ºå¯¦é?å®šç¾©
```powershell
Remove-Item -Path "entities/*.XekuII.yaml" -Force -ErrorAction SilentlyContinue
```

### Step 2: æ¸…ç©ºå¾Œç«¯ Business Objects
```powershell
Remove-Item -Path "XekuII.ApiHost/BusinessObjects/*.Generated.cs" -Force -ErrorAction SilentlyContinue
```

### Step 3: æ¸…ç©ºå¾Œç«¯ API Controllers
```powershell
Remove-Item -Path "XekuII.ApiHost/API/*Controller.Generated.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "XekuII.ApiHost/API/*ExtendedController.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "XekuII.ApiHost/API/StatsController.cs" -Force -ErrorAction SilentlyContinue
```

---

## ä¿ç??„æ??¶æ¶æ§?

æ¸…ç©ºå¾Œï?ä»¥ä?æª”æ?/è³‡æ?å¤¾æ?ä¿ç?ï¼?

| é¡å? | è·¯å? | èªªæ? |
|:---|:---|:---|
| å¾Œç«¯ | `XekuII.ApiHost/API/StatsController.cs` | çµ±è?ç«¯é? |
| å¾Œç«¯ | `XekuII.ApiHost/API/Security/` | èªè? API |
| å¾Œç«¯ | `XekuII.ApiHost/Startup.cs` | ?å?è¨­å? |
| å¾Œç«¯ | `XekuII.ApiHost/BusinessObjects/ApplicationUser*.cs` | ä½¿ç”¨??BO |

| Generator | `XekuII.Generator/` | ?´å€?Generator å°ˆæ? |

---

## é©—è?æ¸…ç©º

// turbo
```powershell
# é©—è?å¾Œç«¯å»ºç½®
dotnet build XekuII.ApiHost/XekuII.ApiHost.csproj --verbosity quiet
```

---

## ?å»ºæµç?

æ¸…ç©ºå¾Œï?ä½¿ç”¨ `/XekuII-generator` å·¥ä?æµç?å¾é›¶?‹å?å»ºç??°å¯¦é«”ã€?
