# DataTouch CRM - Handoff Documentation

> **Última actualización**: 2026-01-20  
> **Branch**: `main`  
> **Versión**: 1.0.0-booking-enterprise

---

## 🎯 Resumen Ejecutivo

DataTouch CRM es una plataforma de gestión de tarjetas digitales profesionales con sistema integrado de **reservas**, **cotizaciones** y **CRM**. Esta versión incluye el módulo completo de **Booking System** y **Quote Request Flow** con arquitectura enterprise.

---

## 📁 Archivos Críticos para Revisar

### Punto de Entrada
| Archivo | Descripción |
|---------|-------------|
| `src/DataTouch.Web/Program.cs` | Configuración de servicios y middleware |
| `src/DataTouch.Web/Components/App.razor` | Root component |
| `src/DataTouch.Infrastructure/Data/DataTouchDbContext.cs` | DbContext con todas las entidades |
| `src/DataTouch.Web/Services/DbInitializer.cs` | Seed data inicial |

### Módulo de Reservas (Booking)
| Archivo | Descripción |
|---------|-------------|
| `src/DataTouch.Domain/Entities/Appointment.cs` | Entidad de citas con estados |
| `src/DataTouch.Domain/Entities/Service.cs` | Servicios con ConversionType (Cita/Cotización) |
| `src/DataTouch.Domain/Entities/AvailabilityRule.cs` | Reglas de disponibilidad semanal |
| `src/DataTouch.Domain/Entities/BookingSettings.cs` | Configuración de reservas |
| `src/DataTouch.Web/Services/AvailabilityService.cs` | Lógica de slots disponibles |
| `src/DataTouch.Web/Services/AppointmentService.cs` | CRUD de citas |
| `src/DataTouch.Web/Components/Pages/Appointments.razor` | Admin CRM de citas (2000+ líneas) |
| `src/DataTouch.Web/Components/Pages/PublicBooking.razor` | Página pública de reserva |
| `src/DataTouch.Web/Components/Shared/CreateAppointmentDialog.razor` | Wizard 3 pasos |

### Módulo de Cotizaciones (Quotes)
| Archivo | Descripción |
|---------|-------------|
| `src/DataTouch.Domain/Entities/QuoteRequest.cs` | Entidad con 8 estados enterprise |
| `src/DataTouch.Domain/Entities/Activity.cs` | Timeline de eventos |
| `src/DataTouch.Web/Services/QuoteService.cs` | Lógica con idempotency y lead dedup |
| `src/DataTouch.Web/Services/QuoteAutomationService.cs` | BackgroundService SLA alerts |
| `src/DataTouch.Web/Components/Pages/Quotes.razor` | Admin CRM de cotizaciones |
| `src/DataTouch.Web/Components/Shared/QuoteRequestModal.razor` | Modal público 3 pasos |

### UI Principal
| Archivo | Descripción |
|---------|-------------|
| `src/DataTouch.Web/Components/Pages/MyCard.razor` | Editor de tarjeta (4000+ líneas) |
| `src/DataTouch.Web/Components/Pages/PublicCard.razor` | Tarjeta pública renderizada |
| `src/DataTouch.Web/Components/Pages/TemplateLibrary.razor` | Biblioteca de plantillas |
| `src/DataTouch.Web/Components/Shared/CardPreview.razor` | Live Preview sync |

---

## 🛠️ Stack Tecnológico

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 9.0 | Framework principal |
| Blazor Server | 9.0 | UI interactiva |
| MudBlazor | 8.x | Componentes Material Design |
| Entity Framework Core | 9.x | ORM |
| InMemory Database | (dev) | Base de datos de desarrollo |
| MySQL/Pomelo | 9.x | Base de datos producción |

### Paquetes Clave
```xml
<PackageReference Include="MudBlazor" Version="8.0.0" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
```

---

## 🚀 Cómo Correr el Proyecto Local

### Prerequisitos
- .NET 9 SDK instalado
- Visual Studio 2022 / VS Code / Rider
- Git

### Pasos

```bash
# 1. Clonar repositorio
git clone https://github.com/[tu-usuario]/DataTouch.git
cd DataTouch

# 2. Restaurar dependencias
dotnet restore

# 3. Correr proyecto web
cd src/DataTouch.Web
dotnet run

# 4. Abrir en navegador
# https://localhost:5001 o http://localhost:5000
```

### Credenciales por Defecto (Seed Data)
| Email | Password | Rol |
|-------|----------|-----|
| `admin@techcorp.com` | `admin123` | Admin |

---

## 🔑 Puntos Críticos de Contexto

### 1. Lógica de CTA Principal

Los servicios tienen un campo `ConversionType`:
- `"booking"` → CTA "Reservar Cita"
- `"quote"` → CTA "Solicitar Cotización"

**Reglas de CTA:**
- Si todos los servicios son del mismo tipo → auto-configura CTA
- Si hay servicios mixtos → muestra selector de chips en admin
- `PrimaryCardGoal` en `Card.cs` determina el CTA principal
- El CTA secundario aparece como link discreto ("o solicitar cotización")

**Archivos relevantes:**
- `MyCard.razor` líneas 650-720 (lógica `IsGoalSelectorVisible`, `HasMixedTypes`)
- `PublicCard.razor` líneas 180-225 (renderizado de CTAs)

### 2. Sistema de Servicios

```csharp
public class Service {
    public string ConversionType { get; set; } // "booking" | "quote"
    public int DurationMinutes { get; set; }
    public decimal? PriceFrom { get; set; }
    public int DisplayOrder { get; set; }
}
```

### 3. Estados de QuoteRequest (8-State Machine)

```
New → InReview → NeedsInfo → Quoted → Negotiation → Won
                                                   → Lost → Archived
```

### 4. Live Preview

El `CardPreview.razor` sincroniza en tiempo real con los cambios del editor.
- Usa `ThemeTokens` para colores dinámicos
- Template aplicado afecta estructura y estilos
- El preview refleja el CTA principal seleccionado

### 5. Disponibilidad de Citas

- `AvailabilityRule` define horarios semanales (ej: Lunes 9:00-17:00)
- `AvailabilityException` para días festivos/bloqueos
- `AvailabilityService.GetAvailableSlotsAsync()` calcula slots libres
- Filtra automáticamente citas existentes (no duplica horarios ocupados)

---

## 📋 Pendientes / Próximos Pasos

### Prioridad Alta
- [ ] Migración EF Core a MySQL producción (conflicto Pomelo v9 / EF Design v10)
- [ ] Notificaciones por email (citas confirmadas, cotizaciones recibidas)
- [ ] Dashboard con métricas reales (conectar a DashboardService)

### Prioridad Media
- [ ] Integración con Google Calendar (export/sync)
- [ ] Recordatorios automáticos 24h antes de cita por WhatsApp
- [ ] Formulario de cancelación con razón

### Prioridad Baja
- [ ] Multi-idioma (i18n)
- [ ] Tema claro/oscuro persistente por usuario
- [ ] Analytics avanzados (funnel de conversión)

---

## ✅ Checklist Post-Clone

Después de clonar el repo, verificar:

- [ ] `dotnet build` compila sin errores (solo warnings MUD0002 aceptables)
- [ ] `dotnet run` inicia servidor en localhost:5001
- [ ] Login con `admin@techcorp.com` / `admin123` funciona
- [ ] Navegar a `/cards/mine` muestra editor de tarjeta
- [ ] Navegar a `/appointments` muestra tabla de citas
- [ ] Navegar a `/quotes` muestra tabla de cotizaciones
- [ ] Navegar a `/p/techcorp/leonel-alvarenga` muestra tarjeta pública
- [ ] Click en "Solicitar Cotización" abre modal de 3 pasos
- [ ] Live Preview actualiza en tiempo real al editar

---

## 📊 Estructura de Carpetas

```
DataTouch/
├── src/
│   ├── DataTouch.Api/           # API REST (futuro)
│   ├── DataTouch.Domain/        # Entidades y modelos
│   │   └── Entities/
│   │       ├── Appointment.cs
│   │       ├── Service.cs
│   │       ├── QuoteRequest.cs
│   │       ├── Activity.cs
│   │       └── ...
│   ├── DataTouch.Infrastructure/ # DbContext, Data Access
│   │   └── Data/
│   │       └── DataTouchDbContext.cs
│   └── DataTouch.Web/           # Blazor Server App
│       ├── Components/
│       │   ├── Pages/           # Páginas principales
│       │   ├── Shared/          # Componentes reutilizables
│       │   └── Layout/          # MainLayout, NavMenu
│       ├── Services/            # Business logic
│       ├── Models/              # ViewModels, DTOs
│       └── wwwroot/             # Static assets
├── sql/
│   └── migrations/              # Scripts SQL
└── tests/
    └── DataTouch.Tests/         # Unit tests
```

---

## 🔧 Comandos Útiles

```bash
# Build completo
dotnet build

# Run con hot reload
dotnet watch run --project src/DataTouch.Web

# Limpiar y rebuild
dotnet clean && dotnet build

# Ver logs detallados
dotnet run --verbosity detailed
```

---

## 📞 Contacto / Soporte

Para dudas sobre la arquitectura o decisiones de diseño, revisar:
- Este documento `HANDOFF.md`
- Comentarios en código (especialmente en servicios)
- Artifacts en `.gemini/antigravity/brain/` (specs y walkthroughs)

---

*Documento generado automáticamente para continuidad de desarrollo.*
