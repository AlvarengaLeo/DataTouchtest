# CLAUDE.md - Documentación Técnica del Proyecto DataTouch CRM

---

## 1. RESUMEN EJECUTIVO DEL PROYECTO

| Campo | Descripción |
|-------|-------------|
| **Nombre del proyecto** | DataTouch CRM - Plataforma de Tarjetas Digitales Profesionales |
| **Propósito y objetivo principal** | Sistema CRM para gestión de tarjetas digitales profesionales con reservas, cotizaciones y leads. Incluye editor visual en tiempo real, sistema de citas, solicitud de cotizaciones y CRM integrado. |
| **Stack tecnológico** | **.NET 9.0**, Blazor Server, MudBlazor 8.x, Entity Framework Core 9.x, MySQL (Pomelo), InMemory DB (desarrollo) |
| **Estado actual del desarrollo** | En desarrollo con funcionalidades core implementadas. Editor de tarjetas, templates, booking y quotes funcionando. Dashboard operativo. |
| **Nivel de criticidad** | **7/10** - Sistema empresarial con módulos de reservas y cotizaciones. Requiere estabilidad para uso profesional. |

---

## 2. ARQUITECTURA DEL PROYECTO

### 2.1 Estructura de Carpetas

```
DataTouch/
├── /docs                           # Documentación
│   └── HANDOFF.md                  # Handoff documentation
│
├── /sql                            # Scripts SQL
│   └── /migrations                 # Migraciones manuales
│       └── 20260113_AddBookingModule.sql
│
├── /src
│   ├── /DataTouch.Api              # API REST (futuro)
│   │   └── Program.cs
│   │
│   ├── /DataTouch.Domain           # Entidades de dominio
│   │   └── /Entities
│   │       ├── Activity.cs         # Timeline de eventos
│   │       ├── Appointment.cs      # Citas con estados
│   │       ├── AvailabilityException.cs  # Bloqueos de horario
│   │       ├── AvailabilityRule.cs # Reglas de disponibilidad
│   │       ├── BookingSettings.cs  # Configuración de reservas
│   │       ├── Card.cs             # Tarjeta principal (TemplateId, StyleId)
│   │       ├── CardAnalytics.cs    # Métricas de tarjeta
│   │       ├── CardComponent.cs    # Componentes dinámicos
│   │       ├── CardStyle.cs        # Estilos personalizados
│   │       ├── CardTemplate.cs     # Plantillas por industria
│   │       ├── Lead.cs             # Leads/Prospectos
│   │       ├── LeadNote.cs         # Notas de leads
│   │       ├── Organization.cs     # Organizaciones
│   │       ├── QuoteRequest.cs     # Solicitudes de cotización (8 estados)
│   │       ├── Service.cs          # Servicios (booking/quote)
│   │       └── User.cs             # Usuarios
│   │
│   ├── /DataTouch.Infrastructure   # Data Access Layer
│   │   └── /Data
│   │       └── DataTouchDbContext.cs  # DbContext con 15+ DbSets
│   │
│   └── /DataTouch.Web              # Blazor Server App
│       ├── /Components
│       │   ├── /Layout
│       │   │   ├── MainLayout.razor      # Layout con sidebar
│       │   │   └── MainLayout.razor.css
│       │   ├── /Pages
│       │   │   ├── Appointments.razor    # CRM de citas (1683 líneas)
│       │   │   ├── Dashboard.razor       # Panel KPIs
│       │   │   ├── Error.razor
│       │   │   ├── Home.razor
│       │   │   ├── Leads.razor           # Gestión de leads
│       │   │   ├── Login.razor
│       │   │   ├── MyCard.razor          # Editor tarjeta (4000+ líneas)
│       │   │   ├── PublicBooking.razor   # Página pública de reserva
│       │   │   ├── PublicCard.razor      # Tarjeta pública (2700+ líneas)
│       │   │   ├── Quotes.razor          # CRM cotizaciones (743 líneas)
│       │   │   └── TemplateLibrary.razor # Carrusel de plantillas
│       │   ├── /Shared
│       │   │   ├── AppointmentDetailsDrawer.razor
│       │   │   ├── CardPreview.razor     # Live preview
│       │   │   ├── CountryPhoneInput.razor
│       │   │   ├── CreateAppointmentDialog.razor  # Wizard 3 pasos
│       │   │   └── QuoteRequestModal.razor
│       │   └── /Templates
│       │       └── PortfolioCreativeTemplate.razor
│       ├── /Models
│       │   ├── PresetRegistry.cs         # Presets de apariencia
│       │   ├── QuoteFormConfig.cs
│       │   ├── ThemeHelper.cs
│       │   └── ThemeTokens.cs
│       ├── /Services
│       │   ├── AppointmentService.cs     # CRUD citas
│       │   ├── AuthService.cs            # Autenticación cookie
│       │   ├── AvailabilityService.cs    # Slots disponibles
│       │   ├── CardAnalyticsService.cs   # Métricas
│       │   ├── CardTemplateSeeder.cs     # Seed de plantillas
│       │   ├── CountryPhoneService.cs    # Códigos de país
│       │   ├── CustomAuthStateProvider.cs
│       │   ├── DashboardService.cs       # KPIs dashboard
│       │   ├── DbInitializer.cs          # Seed data
│       │   ├── GeoLocationService.cs
│       │   ├── QuoteAutomationService.cs # Background SLA alerts
│       │   ├── QuoteService.cs           # CRUD cotizaciones
│       │   └── ThemeService.cs
│       ├── /wwwroot
│       │   └── /uploads                   # Archivos subidos
│       │       ├── /backgrounds
│       │       └── /gallery
│       └── Program.cs                     # Entry point + DI
│
└── /tests
    └── /DataTouch.Tests
        ├── DataTouch.Tests.csproj
        └── UnitTest1.cs                   # Tests placeholder
```

### 2.2 Flujo de Datos

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FLUJO DE DATOS                                  │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐                    ┌──────────────────┐
│   Browser        │     SignalR        │   Blazor Server  │
│   (Usuario)      │ ◄────────────────► │   (.NET 9.0)     │
│                  │  WebSocket         │                  │
└──────┬───────────┘                    └────────┬─────────┘
       │                                         │
       │ Cookies                                 │ EF Core 9.x
       │ (Auth)                                  │
       │                                         ▼
       │                                ┌──────────────────┐
       │                                │   DbContext      │
       │                                │   (15+ DbSets)   │
       │                                └────────┬─────────┘
       │                                         │
       │                                         ▼
       │                                ┌──────────────────┐
       │                                │   Database       │
       │                                │   InMemory (dev) │
       │                                │   MySQL (prod)   │
       │                                └──────────────────┘
       ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                          RUTAS PÚBLICAS                                      │
├──────────────────────────────────────────────────────────────────────────────┤
│  /p/{org-slug}/{card-slug}  →  PublicCard.razor (Tarjeta pública)           │
│  /book/{org-slug}/{card-slug}/{service-id}  →  PublicBooking.razor          │
└──────────────────────────────────────────────────────────────────────────────┘

FLUJO DE AUTENTICACIÓN:
1. Usuario ingresa credenciales en Login.razor
2. AuthService valida email/password contra DbContext
3. Se crea cookie de sesión con email
4. CustomAuthStateProvider proporciona claims
5. Páginas protegidas verifican AuthorizeView
```

### 2.3 Patrones de Arquitectura Identificados

| Patrón | Implementación | Ubicación |
|--------|----------------|-----------|
| **Clean Architecture** | Separación Domain/Infrastructure/Web | Proyectos separados |
| **Service Layer** | Lógica de negocio encapsulada | `Web/Services/` |
| **Repository (EF)** | DbContext como Unit of Work | `Infrastructure/Data/` |
| **Background Services** | Tareas asíncronas | `QuoteAutomationService` |
| **Dependency Injection** | Constructor injection | `Program.cs` |
| **Component-Based UI** | Blazor components | `/Components/` |
| **State Machine** | QuoteStatus enum (8 estados) | `QuoteRequest.cs` |

**Convenciones de código:**
- Entidades: `PascalCase` singular (`Card`, `Appointment`)
- Servicios: `PascalCase` + Service suffix (`QuoteService`)
- Páginas Blazor: `PascalCase.razor` (`MyCard.razor`)
- Variables C#: `_camelCase` para privadas, `PascalCase` para públicas
- Archivos: Kebab-case para uploads (`profile-image.jpg`)

---

## 3. COMPONENTES PRINCIPALES

### 3.1 Sistema de Tarjetas

#### Card Entity

**Ubicación**: `DataTouch.Domain/Entities/Card.cs`

```csharp
public class Card
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Slug { get; set; }           // URL amigable
    public string FullName { get; set; }
    public string? Title { get; set; }
    public string? CompanyName { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }
    
    // CTA Visibility
    public bool ShowSaveContact { get; set; }
    public bool ShowWhatsApp { get; set; }
    public bool ShowCall { get; set; }
    public bool ShowEmail { get; set; }
    
    // JSON Storage
    public string? SocialLinksJson { get; set; }      // {"linkedin":"url",...}
    public string? WebsiteLinksJson { get; set; }     // [{"title":"","url":""}]
    public string? GalleryImagesJson { get; set; }    // Portfolio images
    public string? AppearanceStyleJson { get; set; }  // Theme customization
    
    // Template & Style
    public Guid? TemplateId { get; set; }
    public Guid? StyleId { get; set; }
    
    // Security
    public string? PasswordHash { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveUntil { get; set; }
    
    // Navigation
    public ICollection<Lead> Leads { get; set; }
    public ICollection<CardAnalytics> Analytics { get; set; }
    public ICollection<CardComponent> Components { get; set; }
}
```

#### Editor de Tarjeta

**Página**: `MyCard.razor` (~4000 líneas)
- **Secciones**:
  - Apariencia (6 presets, configuración avanzada)
  - Identidad (nombre, título, empresa, bio)
  - Información de contacto (teléfono, WhatsApp, email)
  - Redes sociales (LinkedIn, Instagram, etc.)
  - Botones de acción (CTAs)
  - Galería de portfolio (para template creativo)
- **Live Preview**: Sincronización en tiempo real con `CardPreview.razor`
- **Presets disponibles**: Premium Dark, Soft Gradient, Glass Clean, High Contrast, Minimal White, Ocean Wave

---

### 3.2 Sistema de Reservas (Booking)

#### Entidades

| Entidad | Campos Clave | Propósito |
|---------|--------------|-----------|
| `Appointment` | Status, ServiceId, StartTime, CustomerName | Cita agendada |
| `Service` | ConversionType, DurationMinutes, PriceFrom | Servicio ofrecido |
| `AvailabilityRule` | DayOfWeek, StartTime, EndTime | Horario semanal |
| `AvailabilityException` | Date, IsBlocked, Reason | Días festivos/bloqueos |
| `BookingSettings` | MinAdvanceHours, MaxAdvanceDays | Configuración |

#### Appointment Status

```csharp
public enum AppointmentStatus
{
    Pending,    // Esperando confirmación
    Confirmed,  // Confirmada
    Cancelled,  // Cancelada
    Completed,  // Completada
    NoShow      // No asistió
}
```

#### Servicios

**AppointmentService** (`Services/AppointmentService.cs`)
- `CreatePublicAppointmentAsync()` - Crear cita desde público
- `GetAppointmentsByCardAsync()` - Listar citas
- `UpdateStatusAsync()` - Cambiar estado
- `GetAvailableSlotsAsync()` - Obtener horarios disponibles

**AvailabilityService** (`Services/AvailabilityService.cs`)
- `GetAvailableSlotsAsync()` - Calcula slots libres considerando reglas y excepciones
- Filtra citas existentes para evitar conflictos

#### Flujo de Reserva

```
Usuario visita tarjeta pública
    └── Click "Reservar Cita"
         └── PublicBooking.razor
              └── Selecciona Servicio
                   └── Selecciona Fecha
                        └── AvailabilityService.GetAvailableSlotsAsync()
                             └── Muestra slots disponibles
                                  └── Selecciona hora
                                       └── Ingresa datos
                                            └── AppointmentService.CreatePublicAppointmentAsync()
                                                 └── Appointment creado con status "Pending"
```

---

### 3.3 Sistema de Cotizaciones (Quotes)

#### QuoteRequest Entity

**Ubicación**: `DataTouch.Domain/Entities/QuoteRequest.cs`

```csharp
public class QuoteRequest
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; }     // "QR-20260120-001"
    public QuoteStatus Status { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Description { get; set; }
    public Guid? ServiceId { get; set; }
    public DateTime? SlaDeadlineAt { get; set; }   // Para alertas SLA
    public DateTime CreatedAt { get; set; }
}
```

#### Estado Machine (8 estados)

```
┌─────────────────────────────────────────────────────────────────┐
│                     QUOTE STATUS MACHINE                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   ┌───────┐    ┌──────────┐    ┌───────────┐    ┌────────┐     │
│   │  New  │───►│ InReview │───►│NeedsInfo  │───►│ Quoted │     │
│   └───────┘    └──────────┘    └───────────┘    └───┬────┘     │
│                                                      │          │
│                                              ┌───────▼───────┐  │
│                                              │  Negotiation  │  │
│                                              └───────┬───────┘  │
│                                                      │          │
│                              ┌──────────┬────────────┴────┐     │
│                              ▼          ▼                 ▼     │
│                         ┌───────┐  ┌────────┐      ┌──────────┐│
│                         │  Won  │  │  Lost  │      │ Archived ││
│                         └───────┘  └────────┘      └──────────┘│
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

#### Quote Services

**QuoteService** (`Services/QuoteService.cs`)
- `CreateQuoteRequestAsync()` - Crear solicitud con idempotency key
- `UpdateStatusAsync()` - Transición de estado + activity log
- `GetTimelineAsync()` - Historial de actividades
- `AddNoteAsync()` - Agregar nota a la cotización

**QuoteAutomationService** (`Services/QuoteAutomationService.cs`)
- BackgroundService que corre cada 5 minutos
- Detecta cotizaciones próximas a vencer SLA
- Potencial para enviar alertas por email

---

### 3.4 Sistema de Templates

#### CardTemplate Entity

```csharp
public class CardTemplate
{
    public Guid Id { get; set; }
    public Guid? OrganizationId { get; set; }  // null = sistema
    public string Name { get; set; }
    public string Industry { get; set; }        // "Sales", "Tech", etc.
    public string? Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public string DefaultStyleJson { get; set; }
    public string DefaultComponentsJson { get; set; }
    public bool IsSystemTemplate { get; set; }
}
```

#### Templates Disponibles

| Nombre | Tipo | Características |
|--------|------|-----------------|
| Creative | Standard | Diseño moderno, colores vibrantes |
| Minimal | Standard | Limpio, minimalista |
| Professional | Standard | Orientado a ventas |
| Portafolio Creativo | Portfolio | Galería de imágenes |
| Corporate | Standard | Branding empresarial |
| Perfil Profesional | Default | CTAs y formulario de contacto |

---

## 4. GUÍA DE TRABAJO POR COMPONENTE

### 4.1 Sistema de Autenticación

#### ¿Cómo funciona actualmente?

**Flujo:**
1. Usuario ingresa email y password en `Login.razor`
2. `AuthService.Login()` valida contra `DbContext.Users`
3. Se crea cookie con `HttpContext.SignInAsync()`
4. `CustomAuthStateProvider` lee claims de la cookie
5. Componentes usan `AuthorizeView` para proteger contenido

**Archivos involucrados:**
- `Components/Pages/Login.razor`
- `Services/AuthService.cs`
- `Services/CustomAuthStateProvider.cs`
- `Program.cs` (configuración auth)

#### Cómo modificar/extender

1. **Agregar roles**:
   - Modificar `User.cs` con campo `Role`
   - Agregar claim en `AuthService.Login()`
   - Usar `[Authorize(Roles = "Admin")]` en páginas

2. **Agregar OAuth (Google)**:
   - Instalar `Microsoft.AspNetCore.Authentication.Google`
   - Configurar en `Program.cs`
   - Agregar botón en `Login.razor`

---

### 4.2 Editor de Tarjeta (MyCard.razor)

#### ¿Cómo funciona actualmente?

**Estructura:**
1. Carga tarjeta del usuario autenticado
2. Deserializa JSON fields (socialLinks, appearance, gallery)
3. Renderiza editor con secciones colapsables
4. Live Preview sincroniza en tiempo real via `StateHasChanged()`
5. `SaveCard()` serializa viewmodels a JSON y guarda

**Secciones:**
- Apariencia (líneas 1-600)
- Identidad (líneas 600-1000)
- Contacto (líneas 1000-1400)
- Redes Sociales (líneas 1400-1800)
- Botones CTA (líneas 1800-2200)
- Portfolio (líneas 2200-2600)
- Live Preview (líneas 2600-3200)
- ViewModels (líneas 3200-3800)

#### Archivos involucrados

- `Components/Pages/MyCard.razor` (editor principal)
- `Components/Shared/CardPreview.razor` (preview)
- `Models/PresetRegistry.cs` (presets de apariencia)
- `Models/ThemeTokens.cs` (tokens CSS)
- `Domain/Entities/Card.cs` (entidad)

#### Cómo agregar nueva sección

1. Agregar campo a `Card.cs` (o nuevo JSON field)
2. Crear ViewModel class en `MyCard.razor` (`@code`)
3. Agregar UI section con `MudPaper` + header colapsable
4. Deserializar en `DeserializeJsonFields()`
5. Serializar en `SaveCard()`
6. Actualizar preview en `CardPreview.razor`

---

### 4.3 Tarjeta Pública (PublicCard.razor)

#### ¿Cómo funciona actualmente?

**Ruta**: `/p/{orgSlug}/{cardSlug}`

**Flujo:**
1. `OnInitializedAsync()` busca tarjeta por slugs
2. Deserializa appearance y social links
3. Renderiza tarjeta con estilos dinámicos
4. CTAs llaman a acciones (WhatsApp, Tel, Email)
5. Formulario de contacto crea Lead

**Secciones:**
- Hero (avatar, nombre, título, bio)
- Status chips (Disponible, Responde < 1h, etc.)
- CTAs principales (Guardar Contacto, WhatsApp, Llamar, Email)
- Galería (si template portfolio)
- Formulario de contacto
- Footer DataTouch

---

## 5. CONVENCIONES Y ESTÁNDARES DEL PROYECTO

### Nomenclatura

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Entidades | PascalCase singular | `Card`, `Appointment`, `QuoteRequest` |
| Servicios | PascalCase + Service | `QuoteService`, `AvailabilityService` |
| Páginas Blazor | PascalCase.razor | `MyCard.razor`, `Appointments.razor` |
| Componentes Shared | PascalCase.razor | `CardPreview.razor`, `QuoteRequestModal.razor` |
| ViewModels | PascalCase + Model | `CardStyleModel`, `SocialLinksModel` |
| Campos privados | _camelCase | `_isLoading`, `_selectedQuote` |
| Propiedades | PascalCase | `FullName`, `CreatedAt` |
| JSON storage | snake_case | `social_links_json` → C# `SocialLinksJson` |

### Estructura de Respuestas

**DbContext Queries:**
```csharp
// Patrón de query con includes
var card = await DbContext.Cards
    .Include(c => c.Organization)
    .Include(c => c.Leads)
    .FirstOrDefaultAsync(c => c.Email == email);
```

**Service Methods:**
```csharp
// Patrón de resultado con success/error
public async Task<(bool Success, Appointment? Appointment, string? Error)> 
    CreatePublicAppointmentAsync(CreateAppointmentDto dto)
{
    // Validaciones
    // Crear entidad
    // Guardar
    return (Success: true, Appointment: appointment, Error: null);
}
```

### Códigos de Estado (Blazor)

| Estado | Manejo |
|--------|--------|
| Loading | `_isLoading = true` + `<MudProgressLinear>` |
| Error | `Snackbar.Add(message, Severity.Error)` |
| Success | `Snackbar.Add(message, Severity.Success)` |
| Empty | Mostrar empty state con ilustración |

---

## 6. CONFIGURACIÓN Y VARIABLES DE ENTORNO

### Variables en Program.cs

```csharp
// Base de datos (actualmente InMemory para desarrollo)
builder.Services.AddDbContext<DataTouchDbContext>(options =>
    options.UseInMemoryDatabase("DataTouchDb")
           .EnableSensitiveDataLogging());

// Para producción MySQL:
// options.UseMySql(connectionString, 
//     ServerVersion.AutoDetect(connectionString));

// Autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Background Services
builder.Services.AddHostedService<QuoteAutomationService>();
```

### Servicios Registrados

| Servicio | Tipo | Propósito |
|----------|------|-----------|
| `DataTouchDbContext` | Scoped | Entity Framework DbContext |
| `AuthService` | Scoped | Autenticación de usuarios |
| `DashboardService` | Scoped | KPIs para dashboard |
| `AppointmentService` | Scoped | CRUD de citas |
| `AvailabilityService` | Scoped | Slots disponibles |
| `QuoteService` | Scoped | CRUD de cotizaciones |
| `QuoteAutomationService` | Hosted | Background SLA alerts |
| `CardAnalyticsService` | Scoped | Métricas de tarjeta |

---

## 7. TESTING

### Tests Existentes

**Ubicación**: `tests/DataTouch.Tests/`
- Framework: xUnit
- Cobertura: **Mínima** (solo placeholder)

```csharp
// UnitTest1.cs
public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Vacío
    }
}
```

### Cómo Ejecutar Tests

```bash
# Desde raíz del proyecto
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Áreas Sin Cobertura (Recomendaciones)

| Área | Prioridad | Tipo de Test |
|------|-----------|--------------|
| `AppointmentService` | 🔴 Alta | Unit tests con DbContext mock |
| `QuoteService` | 🔴 Alta | Unit tests con state transitions |
| `AvailabilityService` | 🔴 Alta | Unit tests con slots calculation |
| `AuthService` | 🟡 Media | Integration tests |
| Blazor Components | 🟡 Media | bUnit component tests |
| E2E Flows | 🟢 Baja | Playwright |

---

## 8. ISSUES Y DEUDA TÉCNICA IDENTIFICADA

### 🔴 Crítico

1. **MudSelect Type Mismatch (RESUELTO)**
   - **Ubicación**: `Quotes.razor:77`, `CreateAppointmentDialog.razor:87`
   - **Problema**: `InvalidCastException` al usar `MudSelect<Guid?>` con `MudSelectItem<Guid>`
   - **Solución aplicada**: Agregar `T="Guid?"` y cast explícito `(Guid?)service.Id`

2. **SDK Version Mismatch (RESUELTO)**
   - **Problema**: Proyectos referenciaban `net10.0` inexistente
   - **Solución aplicada**: Cambiar a `net9.0` en todos los .csproj

3. **Sin autenticación real**
   - **Problema**: Auth basada en cookies sin password hashing robusto
   - **Riesgo**: No apto para producción sin mejoras
   - **Solución**: Implementar ASP.NET Identity

### 🟡 Importante

1. **Seed data inconsistente**
   - **Ubicación**: `DbInitializer.cs`
   - **Problema**: Usuarios de seed tienen emails diferentes (`admin@demo.com` vs `admin@techcorp.com`)
   - **Solución**: Unificar datos de seed

2. **Páginas muy grandes**
   - **Problema**: `MyCard.razor` tiene 4000+ líneas
   - **Solución**: Extraer secciones a componentes separados

3. **Sin validación de formularios**
   - **Problema**: Validación manual, sin `EditForm` o `FluentValidation`
   - **Solución**: Implementar `DataAnnotationsValidator`

4. **Warnings de MudBlazor**
   - **Problema**: 17 warnings de `MUD0002` sobre atributos obsoletos
   - **Solución**: Actualizar patrones según documentación MudBlazor 8.x

### 🟢 Mejoras

1. **Implementar Repository Pattern**
   - Separar queries del DbContext
   - Facilitar testing con mocks

2. **Agregar logging estructurado**
   - Usar Serilog o similar
   - Logs de auditoría para acciones importantes

3. **Email notifications**
   - Notificar citas confirmadas
   - Alertas de cotizaciones nuevas

4. **Internacionalización**
   - La UI está en español hardcodeado
   - Implementar i18n para multi-idioma

---

## 9. GUÍA DE INICIO RÁPIDO

### Setup Inicial

```bash
# 1. Clonar repositorio
git clone https://github.com/AlvarengaLeo/DataTouch.git
cd DataTouch

# 2. Verificar .NET SDK
dotnet --version  # Requiere 9.0+

# 3. Restaurar dependencias
dotnet restore

# 4. Build
dotnet build

# 5. Run
dotnet run --project src/DataTouch.Web --urls "https://localhost:5001;http://localhost:5000"

# 6. Abrir en navegador
# https://localhost:5001
```

### Credenciales por Defecto

| Email | Password | Acceso |
|-------|----------|--------|
| `admin@demo.com` | `admin123` | Dashboard completo |

### Comandos Útiles

```bash
# Build con warnings
dotnet build --verbosity normal

# Run con hot reload
dotnet watch run --project src/DataTouch.Web

# Limpiar y rebuild
dotnet clean && dotnet build

# Ejecutar tests
dotnet test

# Ver estructura del proyecto
dir src/DataTouch.Domain/Entities/
```

---

## 10. CONTACTO Y RECURSOS

### Documentación Adicional

| Recurso | Ubicación |
|---------|-----------|
| Handoff Documentation | `docs/HANDOFF.md` |
| SQL Migrations | `sql/migrations/` |
| Artifacts (specs, walkthroughs) | `.gemini/antigravity/brain/` |

### URLs de Desarrollo

| Ambiente | URL |
|----------|-----|
| Local HTTPS | https://localhost:5001 |
| Local HTTP | http://localhost:5000 |
| Tarjeta Demo | /p/demo-company/admin-demo |
| Appointments CRM | /appointments |
| Quotes CRM | /quotes |

### Git Flow

- Branch principal: `main`
- Commits: Conventional commits (`feat:`, `fix:`, `docs:`)
- PR requeridos para merge

---

*Documento generado el 20 de Enero de 2026*
*Última actualización: v1.1 (fix SDK + MudSelect)*
