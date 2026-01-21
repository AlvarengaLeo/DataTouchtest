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

## Registro de Detección

| Fecha | Anti-Pattern | Archivo | Acción |
|-------|--------------|---------|--------|
| 2026-01-20 | Mega-Razor | MyCard.razor | Pendiente refactor |
| 2026-01-20 | God Service | DashboardService | Pendiente split |
| 2026-01-20 | Mega-Razor | PublicCard.razor | Pendiente refactor |

---

*Última actualización: 2026-01-20*
