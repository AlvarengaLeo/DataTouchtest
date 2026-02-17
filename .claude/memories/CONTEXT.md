# 📋 CONTEXT.md - Contexto del Proyecto DataTouch CRM

## Resumen Ejecutivo

**DataTouch CRM** es una plataforma SaaS de tarjetas digitales profesionales que convierte tarjetas NFC/QR en puntos de entrada digital para captura de leads, gestión de citas, cotizaciones y reservaciones.

## Stack Tecnológico

| Capa | Tecnología |
|------|------------|
| Framework | .NET 9.0 |
| UI | Blazor Server + MudBlazor 8.x |
| ORM | Entity Framework Core 9.x |
| Database | SQL Server (prod) / InMemory (dev) |
| Auth | Cookie Authentication |
| Theming | ThemeTokens → PresetRegistry → ThemeHelper → CSS vars `--dt-*` |

## Estructura del Proyecto

```
/src
├── DataTouch.Domain        → 18 Entidades
├── DataTouch.Infrastructure → DbContext (385+ líneas)
├── DataTouch.Web           → Blazor Server App
│   ├── Components/Pages    → 13 páginas
│   ├── Components/Shared   → 18 componentes
│   ├── Services            → 17 servicios
│   └── Models              → 8 modelos (ThemeTokens, PresetRegistry, CardStyleModel, etc.)
└── DataTouch.Api           → API REST (futuro)
```

## Templates de Tarjeta (5)

| Template (`Card.TemplateType`) | Preset forzado | Componente Shared |
|------|------|------|
| `default` | `premium-dark` | — |
| `portfolio-creative` | (ninguno) | `PortfolioGalleryBlock` |
| `services-quotes` | `emerald-night` | `QuoteRequestBlock` |
| `quote-request` | `sky-light` | `QuoteRequestBlock` |
| `appointments` | `mint-breeze` | `AppointmentBookingBlock` |
| `reservations-range` | `soft-cream` | `ReservationBookingBlock` |

## Entidades Principales

| Entidad | Propósito |
|---------|-----------|
| `Card` | Tarjeta digital con info de contacto + JSON columns |
| `Appointment` | Citas/reservas (5 estados) |
| `QuoteRequest` | Cotizaciones (8 estados) |
| `ReservationRequest` | Reservaciones de rango de fechas |
| `ReservationResource` | Recursos reservables |
| `Service` | Servicios ofrecidos (UseGlobalSchedule) |
| `Lead` | Leads capturados |
| `Activity` | Timeline de auditoría |
| `AvailabilityRule` | Reglas de disponibilidad (global + per-service, break-aware) |

## Páginas Críticas (por tamaño)

| Página | Líneas | Prioridad Refactor |
|--------|--------|-------------------|
| `MyCard.razor` | 5275 | 🔴 Alta |
| `PublicCard.razor` | 2501 | 🔴 Alta |
| `TemplateLibrary.razor` | 2000+ | 🟡 Media |
| `Appointments.razor` | 1683 | 🟡 Media |
| `Dashboard.razor` | 1200+ | 🟡 Media |

## Servicios Principales

| Servicio | Métodos | Responsabilidad |
|----------|---------|-----------------|
| `DashboardService` | 37 | KPIs y analytics |
| `QuoteService` | 12 | CRUD cotizaciones |
| `AppointmentService` | 10 | CRUD citas |
| `AvailabilityService` | 9 | Cálculo de slots (break-aware) |
| `ReservationService` | — | CRUD reservaciones |
| `CardService` | static | Serialization helpers, preset defaults |

## Rutas Importantes

| Ruta | Página | Tipo |
|------|--------|------|
| `/` | Dashboard | Protegida |
| `/cards/mine` | MyCard | Protegida |
| `/appointments` | Appointments | Protegida |
| `/quotes` | Quotes | Protegida |
| `/templates` | TemplateLibrary | Protegida |
| `/p/{org}/{slug}` | PublicCard | Pública |
| `/book/{org}/{slug}/{serviceId}` | PublicBooking | Pública |

## Fuente de Verdad Técnica

**`.claude/CLAUDE.md`** es el documento canónico con 798 líneas que contiene:
- Pipeline de rendering (3 superficies, 1 fuente de verdad)
- Sistema de templates y presets (17 presets)
- Sync contract y guardrails
- Debug checklist
- Changelog

---

*Última actualización: Febrero 2026*
