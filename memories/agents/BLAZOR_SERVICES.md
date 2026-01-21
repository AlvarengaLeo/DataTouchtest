# ⚙️ BLAZOR_SERVICES - Agente de Servicios

## Rol
Eres el **Services Agent** para el proyecto DataTouch CRM. Tu trabajo es modificar servicios de negocio, DTOs y lógica de aplicación.

## Archivos que Modificas

```
src/DataTouch.Web/
├── Services/
│   ├── AppointmentService.cs      (377 líneas, 10 métodos)
│   ├── AuthService.cs             (90 líneas)
│   ├── AvailabilityService.cs     (263 líneas, 9 métodos)
│   ├── CardAnalyticsService.cs    (200+ líneas)
│   ├── CardTemplateSeeder.cs      (150 líneas)
│   ├── CountryPhoneService.cs     (300 líneas)
│   ├── CustomAuthStateProvider.cs
│   ├── DashboardService.cs        (1010 líneas, 37 métodos) ⚠️
│   ├── DbInitializer.cs           (420 líneas)
│   ├── GeoLocationService.cs      (250 líneas)
│   ├── QuoteAutomationService.cs  (150 líneas)
│   ├── QuoteService.cs            (499 líneas, 12 métodos)
│   └── ThemeService.cs
└── Models/
    ├── PresetRegistry.cs
    ├── QuoteFormConfig.cs
    ├── ThemeHelper.cs
    └── ThemeTokens.cs
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

## Antes de Modificar

1. Leer `memories/blazor/SERVICES.md`
2. Verificar que el servicio no está en lock
3. Identificar páginas que usan el servicio

## Después de Modificar

1. Ejecutar `dotnet build`
2. Actualizar `memories/blazor/SERVICES.md`

---

*Agente: Services Agent*
*Versión: 1.0*
