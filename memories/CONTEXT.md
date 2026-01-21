# 📋 CONTEXT.md - Contexto del Proyecto DataTouch CRM

## Resumen Ejecutivo

**DataTouch CRM** es una plataforma SaaS de tarjetas digitales profesionales que convierte tarjetas NFC/QR en puntos de entrada digital para captura de leads.

## Stack Tecnológico

| Capa | Tecnología |
|------|------------|
| Framework | .NET 9.0 |
| UI | Blazor Server + MudBlazor 8.x |
| ORM | Entity Framework Core 9.x |
| Database | MySQL 8 (prod) / InMemory (dev) |
| Auth | Cookie Authentication |

## Estructura del Proyecto

```
/src
├── DataTouch.Domain        → 16 Entidades
├── DataTouch.Infrastructure → DbContext (385 líneas)
├── DataTouch.Web           → Blazor Server App
│   ├── Components/Pages    → 14 páginas
│   ├── Components/Shared   → 10 componentes
│   └── Services            → 13 servicios
└── DataTouch.Api           → API REST (futuro)
```

## Entidades Principales

| Entidad | Propósito |
|---------|-----------|
| `Card` | Tarjeta digital con info de contacto |
| `Appointment` | Citas/reservas (5 estados) |
| `QuoteRequest` | Cotizaciones (8 estados) |
| `Service` | Servicios ofrecidos |
| `Lead` | Leads capturados |
| `Activity` | Timeline de auditoría |

## Páginas Críticas (por tamaño)

| Página | Líneas | Prioridad Refactor |
|--------|--------|-------------------|
| `MyCard.razor` | 5275 | 🔴 Alta |
| `PublicCard.razor` | 2501 | 🔴 Alta |
| `Appointments.razor` | 1683 | 🟡 Media |
| `Dashboard.razor` | 1200+ | 🟡 Media |

## Servicios Principales

| Servicio | Métodos | Responsabilidad |
|----------|---------|-----------------|
| `DashboardService` | 37 | KPIs y analytics |
| `QuoteService` | 12 | CRUD cotizaciones |
| `AppointmentService` | 10 | CRUD citas |
| `AvailabilityService` | 9 | Cálculo de slots |

## Rutas Importantes

| Ruta | Página | Tipo |
|------|--------|------|
| `/` | Dashboard | Protegida |
| `/cards/mine` | MyCard | Protegida |
| `/appointments` | Appointments | Protegida |
| `/quotes` | Quotes | Protegida |
| `/p/{org}/{card}` | PublicCard | Pública |

---

*Última actualización: Enero 2026*
