# Páginas Blazor (13)

Actualizado: 2026-02-16

| Página | Ruta | Líneas | Estado | Layout |
|--------|------|--------|--------|--------|
| Appointments | `/appointments` | 1683 | ⚠️ Crítico | MainLayout |
| Dashboard | `/` | 1200+ | ⚠️ Crítico | MainLayout |
| Error | `/error` | 29 | ✅ OK | — |
| LeadDetail | `/leads/{id}` | 1200+ | ⚠️ Crítico | MainLayout |
| Leads | `/leads` | 133 | ✅ OK | MainLayout |
| Login | `/login` | 300+ | ✅ OK | EmptyLayout |
| Logout | `/logout` | 6 | ✅ OK | — |
| MyCard | `/cards/mine` | 5275 | 🔴 Crítico | MainLayout |
| PublicBooking | `/book/{org}/{slug}/{serviceId}` | 900+ | ⚠️ Grande | EmptyLayout |
| PublicCard | `/p/{org}/{slug}` | 2501 | 🔴 Crítico | EmptyLayout |
| Quotes | `/quotes` | 743 | ✅ OK | MainLayout |
| TemplateDemo | `/template-demo` | ~77 | ✅ OK | — |
| TemplateLibrary | `/templates` | 2000+ | ⚠️ Crítico | MainLayout |

## Páginas con Templates

Las siguientes páginas renderizan contenido específico por `Card.TemplateType`:

- **MyCard.razor** — Editor + live preview (flags: `_isServicesTemplate`, `_isQuoteRequestTemplate`, `_isPortfolioTemplate`, `_isAppointmentsTemplate`, `_isReservationsTemplate`)
- **PublicCard.razor** — Tarjeta pública (mismos flags)
- **TemplateLibrary.razor** — Galería de templates (in-memory `_templates` list)

## Relación con Shared Components

| Componente synced | PublicCard | MyCard Preview | TemplateLibrary |
|---|---|---|---|
| `SocialLinksRow` | ✅ | ✅ Compact | ✅ Compact |
| `QuoteRequestBlock` | ✅ | ✅ Compact | ✅ Compact |
| `AppointmentBookingBlock` | ✅ | ✅ Compact | ✅ Compact |
| `ReservationBookingBlock` | ✅ | ✅ Compact | ✅ Compact |
| `PortfolioGalleryBlock` | ✅ | ✅ | ✅ |

