# CLAUDE.md - Documentación Técnica del Proyecto DataTouch CRM

---

## 1. RESUMEN EJECUTIVO DEL PROYECTO

| Campo | Descripción |
|-------|-------------|
| **Nombre del proyecto** | DataTouch CRM - Plataforma de Tarjetas Digitales Profesionales |
| **Propósito y objetivo principal** | Sistema CRM SaaS que convierte tarjetas NFC/QR en un punto de entrada digital para captura de leads. Incluye editor visual en tiempo real, sistema de citas (booking), solicitud de cotizaciones (quotes), gestión de leads y analytics completos. |
| **Stack tecnológico** | **.NET 9.0**, Blazor Server, MudBlazor 8.x, Entity Framework Core 9.x, MySQL 8 (Pomelo), InMemory DB (desarrollo) |
| **Estado actual del desarrollo** | En desarrollo activo (MVP 0.1). Editor de tarjetas, templates, booking, quotes, dashboard y analytics funcionando. |
| **Nivel de criticidad** | **7/10** - Sistema empresarial con módulos de reservas, cotizaciones y CRM. Requiere estabilidad para uso profesional. |

---

## 2. ARQUITECTURA DEL PROYECTO

### 2.1 Estructura de Carpetas Completa

```
DataTouch/
├── .git/                           # Control de versiones
├── .gitignore
├── AGENT_INSTRUCTIONS.md           # Instrucciones para agentes IA
├── CLAUDE.md                       # Este documento de documentación técnica
├── DATABASE.md                     # Esquema de BD completo con diagrama ER (715 líneas)
├── DataTouch.sln                   # Solución de Visual Studio
├── README.md                       # Documentación principal (179 líneas)
├── SETUP.md                        # Guía de instalación (376 líneas)
│
├── /docs
│   └── HANDOFF.md                  # Documentación de handoff (257 líneas)
│
├── /sql
│   └── /migrations
│       └── 20260113_AddBookingModule.sql
│
├── /src
│   ├── /DataTouch.Api              # API REST (Minimal APIs - futuro)
│   │   ├── DataTouch.Api.csproj
│   │   └── Program.cs
│   │
│   ├── /DataTouch.Domain           # Capa de Dominio - 16 Entidades
│   │   ├── DataTouch.Domain.csproj
│   │   └── /Entities
│   │       ├── Activity.cs         # Timeline de eventos CRM (73 líneas, 12 tipos)
│   │       ├── Appointment.cs      # Citas con estados (72 líneas, 5 estados)
│   │       ├── AvailabilityException.cs  # Bloqueos de horario (32 líneas)
│   │       ├── AvailabilityRule.cs # Reglas de disponibilidad semanal (28 líneas)
│   │       ├── BookingSettings.cs  # Configuración de reservas (40 líneas)
│   │       ├── Card.cs             # Tarjeta principal (96 líneas)
│   │       ├── CardAnalytics.cs    # Eventos de analytics (115 líneas, 10 tipos)
│   │       ├── CardComponent.cs    # Componentes dinámicos (65 líneas)
│   │       ├── CardStyle.cs        # Estilos personalizados QR (130 líneas)
│   │       ├── CardTemplate.cs     # Plantillas por industria (45 líneas)
│   │       ├── Lead.cs             # Leads/Prospectos (27 líneas)
│   │       ├── LeadNote.cs         # Notas de leads (12 líneas)
│   │       ├── Organization.cs     # Organizaciones multi-tenant (15 líneas)
│   │       ├── QuoteRequest.cs     # Cotizaciones enterprise (111 líneas, 8 estados)
│   │       ├── Service.cs          # Servicios booking/quote (65 líneas)
│   │       └── User.cs             # Usuarios (18 líneas)
│   │
│   ├── /DataTouch.Infrastructure   # Data Access Layer
│   │   ├── DataTouch.Infrastructure.csproj
│   │   └── /Data
│   │       └── DataTouchDbContext.cs  # DbContext (385 líneas, 16 DbSets)
│   │
│   └── /DataTouch.Web              # Blazor Server App
│       ├── DataTouch.Web.csproj
│       ├── Program.cs              # Entry point + DI (131 líneas)
│       ├── /Components
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── _Imports.razor
│       │   ├── /Layout
│       │   │   ├── EmptyLayout.razor     # Layout sin sidebar (público)
│       │   │   ├── MainLayout.razor      # Layout con sidebar
│       │   │   ├── MainLayout.razor.css
│       │   │   └── NavMenu.razor
│       │   ├── /Pages
│       │   │   ├── Appointments.razor    # CRM de citas (1683 líneas)
│       │   │   ├── Dashboard.razor       # Panel KPIs (1200+ líneas)
│       │   │   ├── Dashboard.razor.css   # Estilos dashboard (1000+ líneas)
│       │   │   ├── Error.razor
│       │   │   ├── LeadDetail.razor      # Detalle de lead (1200+ líneas)
│       │   │   ├── Leads.razor           # Lista de leads
│       │   │   ├── Login.razor           # Página de login (300+ líneas)
│       │   │   ├── Logout.razor
│       │   │   ├── MyCard.razor          # Editor tarjeta (5275 líneas)
│       │   │   ├── PublicBooking.razor   # Página pública de reserva (900+ líneas)
│       │   │   ├── PublicCard.razor      # Tarjeta pública (2501 líneas)
│       │   │   ├── Quotes.razor          # CRM cotizaciones (743 líneas)
│       │   │   ├── TemplateDemo.razor
│       │   │   └── TemplateLibrary.razor # Biblioteca plantillas (2000+ líneas)
│       │   ├── /Shared
│       │   │   ├── AppointmentDetailsDrawer.razor  # Drawer detalle cita
│       │   │   ├── CardPreview.razor               # Live preview (350+ líneas)
│       │   │   ├── CountryPhoneInput.razor         # Input teléfono intl
│       │   │   ├── CreateAppointmentDialog.razor   # Wizard 3 pasos (835 líneas)
│       │   │   ├── DesignCustomizer.razor          # Personalizador diseño
│       │   │   ├── IconRegistry.razor              # Registro de íconos
│       │   │   ├── IconRegistry.razor.css
│       │   │   ├── QrCustomizer.razor              # Personalizador QR
│       │   │   ├── QuoteRequestModal.razor         # Modal cotización
│       │   │   └── TemplateSelector.razor          # Selector plantillas
│       │   └── /Templates
│       │       └── PortfolioCreativeTemplate.razor
│       ├── /Models
│       │   ├── PresetRegistry.cs         # Presets de apariencia (700+ líneas)
│       │   ├── QuoteFormConfig.cs        # Config formulario cotización
│       │   ├── ThemeHelper.cs            # Helpers de tema
│       │   └── ThemeTokens.cs            # Tokens CSS dinámicos
│       ├── /Services
│       │   ├── AppointmentService.cs     # CRUD citas (377 líneas, 10 métodos)
│       │   ├── AuthService.cs            # Autenticación cookie (90 líneas)
│       │   ├── AvailabilityService.cs    # Slots disponibles (263 líneas, 9 métodos)
│       │   ├── CardAnalyticsService.cs   # Métricas tarjeta (200+ líneas)
│       │   ├── CardTemplateSeeder.cs     # Seed de plantillas (150 líneas)
│       │   ├── CountryPhoneService.cs    # Códigos de país (300 líneas, 240 países)
│       │   ├── CustomAuthStateProvider.cs
│       │   ├── DashboardService.cs       # KPIs dashboard (1010 líneas, 37 métodos)
│       │   ├── DbInitializer.cs          # Seed data (420 líneas)
│       │   ├── GeoLocationService.cs     # Geolocalización IP (250 líneas)
│       │   ├── QuoteAutomationService.cs # Background SLA alerts (150 líneas)
│       │   ├── QuoteService.cs           # CRUD cotizaciones (499 líneas, 12 métodos)
│       │   └── ThemeService.cs
│       └── /wwwroot
│           ├── app.css
│           ├── design-tokens.css
│           └── /uploads
│               ├── /backgrounds
│               └── /gallery
│
└── /tests
    └── /DataTouch.Tests
        ├── DataTouch.Tests.csproj
        └── UnitTest1.cs              # Tests placeholder
```

### 2.2 Flujo de Datos y Autenticación

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
       │ Cookies (Auth)                          │ EF Core 9.x
       │ POST /api/auth/login                    │ DbContext (16 DbSets)
       │ GET /api/auth/logout                    │
       │                                         ▼
       │                                ┌──────────────────┐
       ▼                                │   Database       │
       │                                │   InMemory (dev) │
       │                                │   MySQL 8 (prod) │
       │                                └──────────────────┘
       │
┌──────┴──────────────────────────────────────────────────────────────────────┐
│                          RUTAS                                               │
├──────────────────────────────────────────────────────────────────────────────┤
│ PÚBLICAS (EmptyLayout):                                                      │
│   /p/{org-slug}/{card-slug}  → PublicCard.razor (Tarjeta pública)           │
│   /book/{org-slug}/{card-slug}/{service-id}  → PublicBooking.razor          │
│   /login                      → Login.razor                                  │
│                                                                              │
│ PROTEGIDAS (MainLayout, [Authorize]):                                        │
│   /                           → Dashboard.razor                              │
│   /cards/mine                 → MyCard.razor (Editor)                        │
│   /leads                      → Leads.razor                                  │
│   /leads/{id}                 → LeadDetail.razor                             │
│   /appointments               → Appointments.razor (CRM Citas)               │
│   /quotes                     → Quotes.razor (CRM Cotizaciones)              │
│   /templates                  → TemplateLibrary.razor                        │
└──────────────────────────────────────────────────────────────────────────────┘

FLUJO DE AUTENTICACIÓN:
1. Usuario ingresa credenciales en Login.razor
2. POST /api/auth/login con form data
3. Program.cs valida contra DbContext.Users (SHA256 hash)
4. HttpContext.SignInAsync() crea cookie con claims
5. CustomAuthStateProvider proporciona AuthenticationState
6. Blazor valida [Authorize] en cada página
```

### 2.3 Patrones de Arquitectura

| Patrón | Implementación | Ubicación |
|--------|----------------|-----------|
| **Clean Architecture** | Separación Domain/Infrastructure/Web | Proyectos separados |
| **Service Layer** | Lógica de negocio encapsulada | `Web/Services/` (13 servicios) |
| **Repository (EF)** | DbContext como Unit of Work | `Infrastructure/Data/` |
| **Background Services** | Tareas asíncronas | `QuoteAutomationService` |
| **State Machine** | Enums con transiciones | `QuoteStatus` (8), `AppointmentStatus` (5), `ActivityType` (12) |
| **Activity Logging** | Audit trail polimórfico | `Activity` entity |
| **Idempotency** | Prevención de duplicados | `QuoteRequest.IdempotencyKey` |
| **Lead Deduplication** | Merge por email | `QuoteService.FindOrCreateLeadAsync()` |

---

## 3. ENTIDADES DEL DOMINIO

### 3.1 Card (Tarjeta Digital)

```csharp
public class Card
{
    // Identificación
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Slug { get; set; }              // URL: /p/{org}/{slug}
    
    // Información personal
    public string FullName { get; set; }
    public string? Title { get; set; }
    public string? CompanyName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    
    // Contacto (con soporte multi-país)
    public string? Phone { get; set; }
    public string? PhoneCountryCode { get; set; }  // "+503"
    public string? WhatsAppNumber { get; set; }
    public string? WhatsAppCountryCode { get; set; }
    public string? Email { get; set; }
    
    // CTA Visibility Flags
    public bool ShowSaveContact { get; set; }
    public bool ShowWhatsApp { get; set; }
    public bool ShowCall { get; set; }
    public bool ShowEmail { get; set; }
    
    // JSON Storage (flexible)
    public string? SocialLinksJson { get; set; }      // {"linkedin":"url",...}
    public string? WebsiteLinksJson { get; set; }     // [{"title":"","url":""}]
    public string? GalleryImagesJson { get; set; }    // Portfolio images
    public string? AppearanceStyleJson { get; set; }  // Theme tokens
    
    // Template System
    public Guid? TemplateId { get; set; }
    public string TemplateType { get; set; } = "default";  // default, portfolio-creative, services-quotes
    public string PrimaryCardGoal { get; set; } = "booking";  // booking | quote
    public Guid? StyleId { get; set; }
    
    // Security
    public string? PasswordHash { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveUntil { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public Organization Organization { get; set; }
    public User User { get; set; }
    public CardTemplate? Template { get; set; }
    public CardStyle? Style { get; set; }
    public ICollection<Lead> Leads { get; set; }
    public ICollection<Service> Services { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
    public ICollection<QuoteRequest> QuoteRequests { get; set; }
    public ICollection<AvailabilityRule> AvailabilityRules { get; set; }
    public ICollection<AvailabilityException> AvailabilityExceptions { get; set; }
    public ICollection<CardComponent> Components { get; set; }
    public ICollection<CardAnalytics> Analytics { get; set; }
}
```

### 3.2 Service (Servicios)

```csharp
public class Service
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public decimal? PriceFrom { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // Tipo de conversión: "booking" | "quote" | "both"
    public string ConversionType { get; set; } = "booking";
    
    // Modalidad: "presencial" | "online" | "domicilio"
    public string? Modality { get; set; }
    
    // Overrides de configuración
    public int? BufferBeforeMinutes { get; set; }
    public int? BufferAfterMinutes { get; set; }
    public int? MinNoticeMinutes { get; set; }
    public int? MaxBookingsPerDay { get; set; }
    
    public string? QuoteFormConfigJson { get; set; }
}
```

### 3.3 Appointment (Citas)

```csharp
public enum AppointmentStatus
{
    Pending = 0,    // Esperando confirmación
    Confirmed = 1,  // Confirmada
    Completed = 2,  // Completada
    Cancelled = 3,  // Cancelada
    NoShow = 4      // No asistió
}

public class Appointment
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ServiceId { get; set; }
    
    // Tiempo
    public DateTime StartDateTime { get; set; }  // UTC
    public DateTime EndDateTime { get; set; }    // UTC
    public string Timezone { get; set; } = "America/El_Salvador";
    
    // Estado
    public AppointmentStatus Status { get; set; }
    
    // Cliente
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public string? CustomerNotes { get; set; }
    
    // CRM
    public string? InternalNotes { get; set; }
    public string Source { get; set; } = "PublicCard";  // PublicCard, Admin, Quote
    
    // Cancel tracking (undo support)
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancelReason { get; set; }
    public AppointmentStatus? PreviousStatus { get; set; }
}
```

### 3.4 QuoteRequest (Cotizaciones - 8 Estados)

```csharp
public enum QuoteStatus
{
    New = 0,         // Recién recibida, sin revisar
    InReview = 1,    // Owner la está analizando
    NeedsInfo = 2,   // Se pidió más info al cliente
    Quoted = 3,      // Cotización enviada
    Negotiation = 4, // En proceso de negociación
    Won = 5,         // Cliente aceptó, venta cerrada
    Lost = 6,        // Cliente rechazó o no respondió
    Archived = 7     // Movida a histórico
}

public class QuoteRequest
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid CardId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? LeadId { get; set; }  // CRM integration
    
    // Número humano: "QR-2026-0042"
    public string RequestNumber { get; set; }
    
    // Cliente (denormalizado)
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public string? CustomerCompany { get; set; }
    
    // Detalles
    public string? Description { get; set; }
    public string? AttachmentsJson { get; set; }
    public string? CustomFieldsJson { get; set; }
    
    // Estado
    public QuoteStatus Status { get; set; }
    public string? StatusReason { get; set; }  // Para lost/archived
    
    // Assignment
    public Guid? OwnerId { get; set; }
    public int Priority { get; set; } = 2;  // 1=Low, 2=Med, 3=High
    
    // SLA tracking
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? LastContactAt { get; set; }
    public DateTime? SlaDeadlineAt { get; set; }
    public bool SlaNotified { get; set; }
    
    // Outcome
    public decimal? QuotedAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public DateTime? WonAt { get; set; }
    public DateTime? LostAt { get; set; }
    
    // Idempotency
    public string? IdempotencyKey { get; set; }
    
    // Conversion
    public Guid? ConvertedAppointmentId { get; set; }
}
```

### 3.5 Activity (Audit Trail)

```csharp
public enum ActivityType
{
    Created = 0,
    StatusChange = 1,
    Note = 2,
    EmailSent = 3,
    EmailReceived = 4,
    Call = 5,
    Assignment = 6,
    Conversion = 7,
    AttachmentAdded = 8,
    SlaAlert = 9,
    Merge = 10,
    Automation = 11
}

public class Activity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    
    // Relación polimórfica
    public string EntityType { get; set; }  // "Lead", "QuoteRequest", "Appointment"
    public Guid EntityId { get; set; }
    
    // Datos
    public ActivityType Type { get; set; }
    public string Description { get; set; }
    public string? MetadataJson { get; set; }  // {old_status, new_status, note, etc.}
    
    // Actor (null = system)
    public Guid? UserId { get; set; }
    public string? SystemSource { get; set; }  // "automation", "api", "webhook"
}
```

### 3.6 CardAnalytics (Eventos)

**Tipos de eventos soportados:**

| EventType | Descripción | Channel |
|-----------|-------------|---------|
| `page_view` | Vista de la página | - |
| `qr_scan` | Escaneo del código QR | - |
| `nfc_tap` | Tap de NFC | - |
| `cta_click` | Clic en botón de acción | whatsapp, call, email, calendar |
| `link_click` | Clic en enlace/red social | linkedin, instagram, website |
| `contact_save` | Descarga de vCard | - |
| `form_submit` | Envío de formulario | - |
| `meeting_click` | Clic en calendario | calendly |
| `directions_click` | Clic en mapa | - |
| `share` | Tarjeta compartida | - |

---

## 4. SERVICIOS PRINCIPALES

### 4.1 AppointmentService (377 líneas)

| Método | Descripción |
|--------|-------------|
| `GetPublicServicesAsync()` | Obtiene servicios activos de una tarjeta |
| `GetAvailableSlotsAsync()` | Calcula slots disponibles para una fecha |
| `CreatePublicAppointmentAsync()` | Crea cita con concurrency check |
| `GetAppointmentsAsync()` | Lista citas con filtros para CRM |
| `UpdateStatusAsync()` | Cambia estado de cita |
| `RescheduleAsync()` | Reprogramar cita |
| `UpdateNotesAsync()` | Agregar notas internas |
| `CancelWithReasonAsync()` | Cancelar con razón (guarda estado previo) |
| `RestoreAsync()` | Restaurar cita cancelada |

### 4.2 QuoteService (499 líneas)

| Método | Descripción |
|--------|-------------|
| `CreatePublicQuoteAsync()` | Crea cotización con idempotency y lead dedup |
| `FindOrCreateLeadAsync()` | Busca lead por email o crea nuevo |
| `GenerateRequestNumberAsync()` | Genera número QR-YYYY-NNNN |
| `GetQuotesAsync()` | Lista cotizaciones con includes |
| `GetStatusCountsAsync()` | Conteo por estado para dashboard |
| `UpdateStatusAsync()` | Transición de estado + activity log |
| `AssignOwnerAsync()` | Asignar responsable |
| `AddNoteAsync()` | Agregar nota interna |
| `GetTimelineAsync()` | Timeline de actividades |
| `ConvertToAppointmentAsync()` | Convertir cotización a cita |

### 4.3 DashboardService (1010 líneas)

| Método | Descripción |
|--------|-------------|
| `GetDashboardKpisAsync()` | KPIs principales con comparación periódica |
| `GetTotalInteractionsAsync()` | Total de interacciones (page_view + link_click + cta_click) |
| `GetLeadsCapturedAsync()` | Leads capturados en rango |
| `GetMeetingsBookedAsync()` | Citas agendadas |
| `GetInteractionsVsLeadsChartAsync()` | Datos para gráfico de tendencias |
| `GetTopLocationsAsync()` | Top países/ciudades con mapa |
| `GetTopLinksAsync()` | Enlaces más clickeados |
| `GetInsightsAsync()` | Insights automáticos |
| `GetHighIntentDataAsync()` | Actividades de alta intención |

### 4.4 AvailabilityService (263 líneas)

| Método | Descripción |
|--------|-------------|
| `CalculateSlotsForDateAsync()` | Calcula slots basado en reglas |
| `HasAvailabilityAsync()` | Verifica disponibilidad en fecha |
| `GetRulesAsync()` | Obtiene reglas de tarjeta |
| `SaveRulesAsync()` | Guarda/actualiza reglas |
| `CreateDefaultRulesAsync()` | Crea reglas por defecto (L-V 9-17) |
| `GetExceptionsAsync()` | Obtiene excepciones en rango |
| `SaveExceptionAsync()` | Guarda excepción |
| `DeleteExceptionAsync()` | Elimina excepción |

---

## 5. PÁGINAS BLAZOR PRINCIPALES

### 5.1 MyCard.razor (5275 líneas)

**Ruta:** `/cards/mine`

**Secciones del editor:**
1. **Header** - Título "Perfil Público", estado de guardado
2. **Apariencia** (colapsable)
   - Presets: 6 categorías (Dark, Light, Gradient, Glass, Bold, Minimal)
   - Configuración avanzada: colores, tipografía, fondos
3. **Identidad** - Nombre, título, empresa, bio
4. **Contacto** - Teléfonos con selector de país, email
5. **Redes Sociales** - 8 plataformas soportadas
6. **Botones CTA** - Visibilidad de cada botón
7. **Galería** - Portfolio de imágenes (drag & drop)
8. **Live Preview** - `CardPreview.razor` sincronizado

### 5.2 PublicCard.razor (2501 líneas)

**Ruta:** `/p/{OrgSlug}/{CardSlug}`

**Renderizado dinámico:**
- Detecta tipo de template (default, portfolio, services)
- Aplica tokens CSS dinámicos
- Muestra servicios con CTAs inteligentes
- Formulario de contacto → Lead
- Modal de cotización → QuoteRequest

### 5.3 Appointments.razor (1683 líneas)

**Ruta:** `/appointments`

**Tabs:**
1. **Citas** - Tabla con filtros, drawer de detalle
2. **Servicios** - CRUD de servicios
3. **Disponibilidad** - Reglas semanales, excepciones

### 5.4 Dashboard.razor (1200+ líneas)

**Ruta:** `/` (home)

**Métricas:**
- KPIs con comparación periódica
- Gráfico de interacciones vs leads
- Mapa de ubicaciones
- Top enlaces
- Actividad reciente
- Insights automáticos

---

## 6. CONFIGURACIÓN

### 6.1 Program.cs - Servicios Registrados

```csharp
// Database
builder.Services.AddDbContext<DataTouchDbContext>(options =>
    options.UseInMemoryDatabase("DataTouchDb")
           .EnableSensitiveDataLogging());

// Para producción MySQL:
// options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CardAnalyticsService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<QuoteService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<CountryPhoneService>();
builder.Services.AddScoped<GeoLocationService>();

// Background Services
builder.Services.AddQuoteAutomations();  // Extension method
```

### 6.2 API Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/auth/login` | Login con form data |
| `GET` | `/api/auth/logout` | Logout (redirect a /login) |

---

## 7. DOCUMENTACIÓN ADICIONAL

| Archivo | Contenido |
|---------|-----------|
| [README.md](./README.md) | Documentación principal, quick start |
| [SETUP.md](./SETUP.md) | Guía de instalación completa |
| [DATABASE.md](./DATABASE.md) | Esquema de BD, diagrama ER, scripts SQL |
| [docs/HANDOFF.md](./docs/HANDOFF.md) | Contexto para continuidad |

---

## 8. TESTING

### Estado Actual

- **Framework:** xUnit
- **Cobertura:** Mínima (placeholder)
- **Ubicación:** `tests/DataTouch.Tests/`

### Ejecución

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

### Prioridades de Testing

| Área | Prioridad | Tipo |
|------|-----------|------|
| `AppointmentService` | 🔴 Alta | Unit tests con mock DbContext |
| `QuoteService` | 🔴 Alta | Unit tests con state transitions |
| `AvailabilityService` | 🔴 Alta | Unit tests con cálculo de slots |
| `DashboardService` | 🟡 Media | Integration tests |
| Blazor Components | 🟡 Media | bUnit |
| E2E | 🟢 Baja | Playwright |

---

## 9. DEUDA TÉCNICA

### 🔴 Crítico (Resuelto)

1. **MudSelect InvalidCastException** ✅
   - Ubicación: `Quotes.razor:77`, `CreateAppointmentDialog.razor:87`
   - Solución: Agregar `T="Guid?"` y cast `(Guid?)service.Id`

2. **SDK Version** ✅
   - Problema: Proyectos referenciaban `net10.0`
   - Solución: Cambiar a `net9.0`

### 🟡 Importante

1. **Páginas muy grandes**
   - `MyCard.razor` (5275 líneas) - Extraer a componentes

2. **Sin validación robusta**
   - Implementar FluentValidation o DataAnnotations

3. **Warnings MudBlazor**
   - 17 warnings `MUD0002` sobre atributos obsoletos

4. **Auth básica**
   - Password hash SHA256 simple
   - Implementar ASP.NET Identity para producción

### 🟢 Mejoras

1. Email notifications (SendGrid/SMTP)
2. Google Calendar integration
3. Multi-idioma (i18n)
4. Repository pattern
5. Logging estructurado (Serilog)

---

## 10. INICIO RÁPIDO

```bash
# 1. Clonar
git clone https://github.com/AlvarengaLeo/DataTouch.git
cd DataTouch

# 2. Verificar SDK
dotnet --version  # 9.0+

# 3. Restaurar y build
dotnet restore
dotnet build

# 4. Run
dotnet run --project src/DataTouch.Web

# 5. Abrir
# https://localhost:5001
# Login: admin@demo.com / admin123
```

---

*Documento actualizado el 20 de Enero de 2026*
*Versión: 2.0 (análisis exhaustivo)*
