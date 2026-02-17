# ❌ ANTI_PATTERNS.md - Patrones a Evitar

## Blazor

### 1. 🔴 Mega-Razor Files

```razor
@* ❌ MAL: MyCard.razor (5275 líneas) *@
@* Todo en un solo archivo gigante *@

@* ✅ BIEN: Extraer secciones a componentes *@
<AppearanceSection @bind-Style="_style" />
<IdentitySection @bind-Card="_card" />
<ContactSection @bind-Card="_card" />
<SocialLinksSection @bind-Links="_socialLinks" />
```

**Archivos afectados actualmente**:
- `MyCard.razor` (5275 líneas) 🔴
- `PublicCard.razor` (2501 líneas) 🔴
- `TemplateLibrary.razor` (2000+ líneas) 🟡

---

### 2. 🔴 DbContext Directo en Páginas

```razor
@* ❌ MAL: Queries directas *@
@inject DataTouchDbContext DbContext

@code {
    var cards = await DbContext.Cards.ToListAsync();
}

@* ✅ BIEN: Usar servicio *@
@inject CardService CardService

@code {
    var cards = await CardService.GetAllAsync();
}
```

---

### 3. 🟡 50+ Variables Privadas

```razor
@* ❌ MAL: Estado disperso *@
@code {
    private bool _isLoading;
    private bool _isSaving;
    private string _error;
    private Card _card;
    private List<Service> _services;
    // ... 45 más
}

@* ✅ BIEN: ViewModel *@
@code {
    private MyCardViewModel _vm = new();
}
```

---

## C# Services

### 1. 🔴 God Service

```csharp
// ❌ MAL: DashboardService.cs tiene 1010 líneas y 37 métodos

// ✅ BIEN: Split por responsabilidad
public class KpiService { ... }
public class ChartDataService { ... }
public class InsightsService { ... }
public class LocationAnalyticsService { ... }
```

---

### 2. 🟡 DTOs al Final del Archivo

```csharp
// ❌ MAL: DTOs definidos al final del servicio
public class QuoteService
{
    // 450 líneas de lógica...
}

// DTOs aquí abajo
public class CreateQuoteDto { }
public class QuoteResult { }

// ✅ BIEN: DTOs en carpeta Models/
// Models/CreateQuoteDto.cs
// Models/QuoteResult.cs
```

---

### 3. 🟡 Catch Genérico

```csharp
// ❌ MAL: Catch todo
catch (Exception ex)
{
    return (false, null, "Error inesperado");
}

// ✅ BIEN: Preservar información
catch (Exception ex)
{
    _logger.LogError(ex, "Error en operación X");
    return (false, null, ex.Message);
}
```

---

## Entidades

### 1. 🟡 Enums en Archivo de Entidad

```csharp
// ❌ MAL: Enum dentro de Appointment.cs
public class Appointment { ... }
public enum AppointmentStatus { ... }

// ✅ MEJOR: Archivo separado (cuando crezca)
// Domain/Enums/AppointmentStatus.cs
```

---

---

## EF Core Blazor

### 1. 🔴 Fire-and-Forget DB Operations

```csharp
// ❌ MAL: Fire-and-forget causa concurrency crash
_ = DbContext.SaveChangesAsync();

// ✅ BIEN: Await o usar flag para flush posterior
await DbContext.SaveChangesAsync();
// O usar _pendingDbSave flag (patrón MyCard.razor)
```

### 2. 🔴 DB Operations sin SemaphoreSlim

```csharp
// ❌ MAL: Operaciones DB sin gate en MyCard.razor
await DbContext.Cards.FirstOrDefaultAsync(c => c.Id == id);

// ✅ BIEN: Usar _dbGate
await _dbGate.WaitAsync();
try {
    await DbContext.Cards.FirstOrDefaultAsync(c => c.Id == id);
} finally { _dbGate.Release(); }
```

### 3. 🔴 Inline Theme Tokens

```razor
@* ❌ MAL: Inline _themeTokens styles *@
<div style="color: @_themeTokens.TextPrimary">

@* ✅ BIEN: CSS vars *@
<div class="my-text">
/* CSS: */ color: var(--dt-text-primary);
```

---

## Registro de Detección

| Fecha | Anti-Pattern | Archivo | Estado |
|-------|--------------|---------|--------|
| 2026-01-20 | Mega-Razor | MyCard.razor | 🔴 Pendiente refactor |
| 2026-01-20 | God Service | DashboardService | 🔴 Pendiente split |
| 2026-01-20 | Mega-Razor | PublicCard.razor | 🔴 Pendiente refactor |
| 2026-02-11 | Fire-and-forget DB | MyCard.razor | ✅ Resuelto (_dbGate + _pendingDbSave) |
| 2026-02-11 | Inline _themeTokens | MyCard.razor | ✅ Resuelto (migrado a --dt-*) |
| 2026-02-11 | CardStyleModel duplicado | MyCard/PublicCard | ✅ Resuelto (extraído a Models/) |
| 2026-02-11 | Social icons markup 3x | MyCard/PublicCard/TemplateLib | ✅ Resuelto (SocialLinksRow.razor) |
| 2026-02-11 | Hex hardcodeado en preview | TemplateLibrary.razor | ✅ Resuelto (var(--dt-*)) |
| 2026-02-11 | Dual CSS var systems | MyCard/ThemeHelper | ✅ Resuelto (bridge aliases) |

---

*Última actualización: 2026-02-16*
