# Servicios de la Aplicación (17)

Actualizado: 2026-02-16

| Servicio | Líneas | Estado | Patrón DB |
|----------|--------|--------|-----------|
| AppointmentDashboardService | — | ✅ OK | IDbContextFactory |
| AppointmentService | 377 | ✅ OK | Scoped DbContext |
| AuthService | 90 | ✅ OK | Scoped DbContext |
| AvailabilityService | 263 | ✅ OK | Scoped DbContext |
| CardAnalyticsService | 200+ | ✅ OK | Scoped DbContext |
| CardService | — | ✅ OK | Static (no DB) |
| CardTemplateSeeder | 150 | ✅ OK | Scoped DbContext |
| CountryPhoneService | 300 | ✅ OK | Static data |
| CustomAuthStateProvider | ~17 | ✅ OK | — |
| DashboardService | 1010 | 🔴 Excede | IDbContextFactory |
| DbInitializer | 420 | ⚠️ Grande | Scoped DbContext |
| GeoLocationService | 250 | ✅ OK | External API |
| QuoteAutomationService | 150 | ✅ OK | Scoped DbContext |
| QuoteService | 499 | ⚠️ Límite | Scoped DbContext |
| ReservationDashboardService | — | ✅ OK | IDbContextFactory |
| ReservationService | — | ✅ OK | Scoped DbContext |
| ThemeService | ~20 | ✅ OK | — |

## Servicios por Template

| Template | Servicios involucrados |
|----------|----------------------|
| `appointments` | AppointmentService, AvailabilityService, AppointmentDashboardService |
| `reservations-range` | ReservationService, ReservationDashboardService |
| `services-quotes` | QuoteService, QuoteAutomationService |
| `quote-request` | QuoteService |
| Todos | CardService (static), CardAnalyticsService, AuthService |

## Modelos (8)

| Modelo | Propósito |
|--------|-----------|
| CardStyleModel | Shared, serialized → AppearanceStyleJson |
| PortfolioGalleryModel | EnablePhotos/Videos + listas |
| PresetRegistry | 17 presets (9 dark + 8 light) |
| QuoteFormConfig | Config de formulario de cotización |
| QuoteSettingsModel | Serialized → QuoteSettingsJson |
| ReservationSettingsModel | Serialized → ReservationSettingsJson |
| ThemeHelper | GenerateCssVariables (~60 --dt-* + 12 --surface-*) |
| ThemeTokens | Record, 43+ propiedades en 7 grupos |

