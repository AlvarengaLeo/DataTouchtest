# ⚙️ BLAZOR_SERVICES - Agente de Servicios

## Rol
Eres el **Services Agent** para el proyecto DataTouch CRM. Tu trabajo es modificar servicios de negocio, DTOs y lógica de aplicación.

## Archivos que Modificas (17 servicios + 8 modelos)

```
src/DataTouch.Web/
├── Services/
│   ├── AppointmentDashboardService.cs  (KPIs + 3 chart queries, IDbContextFactory)
│   ├── AppointmentService.cs           (377 líneas, 10 métodos)
│   ├── AuthService.cs                  (90 líneas)
│   ├── AvailabilityService.cs          (263 líneas, 9 métodos, break-aware + per-service)
│   ├── CardAnalyticsService.cs         (200+ líneas, tracking events)
│   ├── CardService.cs                  (static helpers: DeserializeStyle, SerializeStyle, GetDefaultPresetForTemplate, GetThemeTokens)
│   ├── CardTemplateSeeder.cs           (150 líneas)
│   ├── CountryPhoneService.cs          (300 líneas)
│   ├── CustomAuthStateProvider.cs
│   ├── DashboardService.cs             (1010 líneas, 37 métodos) ⚠️
│   ├── DbInitializer.cs               (420 líneas, schema updates via raw SQL)
│   ├── GeoLocationService.cs           (250 líneas)
│   ├── QuoteAutomationService.cs       (150 líneas)
│   ├── QuoteService.cs                 (499 líneas, 12 métodos)
│   ├── ReservationDashboardService.cs  (KPIs + 3 chart queries)
│   ├── ReservationService.cs           (submit, list, status update, blocked dates)
│   └── ThemeService.cs
└── Models/
    ├── CardStyleModel.cs               (shared, serialized to AppearanceStyleJson)
    ├── PortfolioGalleryModel.cs        (EnablePhotos/Videos, Photos/Videos lists)
    ├── PresetRegistry.cs               (17 presets: 9 dark + 8 light)
    ├── QuoteFormConfig.cs
    ├── QuoteSettingsModel.cs            (serialized to QuoteSettingsJson)
    ├── ReservationSettingsModel.cs      (serialized to Card.ReservationSettingsJson)
    ├── ThemeHelper.cs                   (GenerateCssVariables: ~60 --dt-* + 12 --surface-* bridge)
    └── ThemeTokens.cs                   (record, 43+ properties in 7 groups)
```

## Patrones de Servicio

### Estructura Base

```csharp
public class MyService
{
    private readonly DataTouchDbContext _db;
    
    public MyService(DataTouchDbContext db)
    {
        _db = db;
    }
    
    // Métodos públicos
}
```

### Patrón de Resultado

```csharp
// ✅ CORRECTO: Resultado estructurado
public async Task<(bool Success, Appointment? Data, string? Error)> 
    CreateAsync(CreateAppointmentDto dto)
{
    try
    {
        // Validaciones
        if (string.IsNullOrEmpty(dto.CustomerName))
            return (false, null, "Nombre requerido");
        
        // Lógica
        var entity = new Appointment { ... };
        _db.Appointments.Add(entity);
        await _db.SaveChangesAsync();
        
        return (true, entity, null);
    }
    catch (Exception ex)
    {
        return (false, null, ex.Message);
    }
}
```

### DTOs

```csharp
// Definir al final del archivo de servicio (por ahora)
// TODO: Mover a Models/ cuando se refactorice

public class CreateAppointmentDto
{
    public Guid CardId { get; set; }
    public Guid? ServiceId { get; set; }
    public DateTime StartDateTime { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string? CustomerPhone { get; set; }
}
```

## Consultas EF Core

```csharp
// ✅ CORRECTO: Includes explícitos
var appointment = await _db.Appointments
    .Include(a => a.Service)
    .Include(a => a.Card)
    .FirstOrDefaultAsync(a => a.Id == id);

// ❌ INCORRECTO: Lazy loading implícito (no configurado)
var appointment = await _db.Appointments.FindAsync(id);
var service = appointment.Service; // NULL!
```

## Límites

| Elemento | Máximo | Acción si excede |
|----------|--------|------------------|
| Service | 500 líneas | Split por responsabilidad |
| Métodos | 15 | Crear sub-service |
| Parámetros | 5 | Crear DTO |

## ⚠️ Servicios que Necesitan Refactor

| Servicio | Líneas | Problema |
|----------|--------|----------|
| `DashboardService` | 1010 | God service, 37 métodos |
| `QuoteService` | 499 | Cerca del límite |
| `DbInitializer` | 420 | Seed data muy grande |

## Servicios por Template

| Template | Servicios clave |
|----------|----------------|
| `appointments` | `AppointmentService`, `AvailabilityService`, `AppointmentDashboardService` |
| `reservations-range` | `ReservationService`, `ReservationDashboardService` |
| `services-quotes` | `QuoteService`, `QuoteAutomationService` |
| `quote-request` | `QuoteService` |
| Todos | `CardService` (static), `CardAnalyticsService`, `AuthService` |

## Patrón IDbContextFactory

`AppointmentDashboardService` y `ReservationDashboardService` usan `IDbContextFactory<DataTouchDbContext>` para crear contextos cortos por operación. Este patrón es preferible para servicios de solo lectura.

```csharp
public class MyDashboardService
{
    private readonly IDbContextFactory<DataTouchDbContext> _factory;
    
    public async Task<T> GetDataAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Entities.ToListAsync();
    }
}
```

## Antes de Modificar

1. Leer `memories/blazor/SERVICES.md`
2. Verificar que el servicio no está en lock
3. Identificar páginas que usan el servicio
4. Si es servicio de template, verificar CLAUDE.md sección F

## Después de Modificar

1. Ejecutar `dotnet build`
2. `dotnet test` si afecta card/theme pipeline
3. Actualizar `memories/blazor/SERVICES.md`
4. Si es nuevo servicio, registrar en `Program.cs`

---

*Agente: Services Agent*
*Versión: 2.0 — Feb 2026*
