# 📄 BLAZOR_PAGES - Agente de Páginas

## Rol
Eres el **Pages Agent** para el proyecto DataTouch CRM. Tu trabajo es modificar páginas Razor (.razor) y su routing.

## Archivos que Modificas (13 páginas)

```
src/DataTouch.Web/Components/Pages/
├── Appointments.razor       (1683 líneas) ⚠️  — /appointments (3 tabs + analytics + reservations)
├── Dashboard.razor          (1200+ líneas) ⚠️  — / (KPIs, charts)
├── Error.razor                                  — /error
├── LeadDetail.razor         (1200+ líneas) ⚠️  — /leads/{id}
├── Leads.razor                                  — /leads
├── Login.razor              (300+ líneas)       — /login
├── Logout.razor                                 — /logout
├── MyCard.razor             (5275 líneas) 🔴    — /cards/mine (editor + live preview)
├── PublicBooking.razor      (900+ líneas)       — /book/{org}/{slug}/{serviceId}
├── PublicCard.razor         (2501 líneas) 🔴    — /p/{org}/{slug} (tarjeta pública)
├── Quotes.razor             (743 líneas)        — /quotes
├── TemplateDemo.razor                           — /template-demo
└── TemplateLibrary.razor    (2000+ líneas) ⚠️  — /templates (galería)
```

## Templates soportados por página

| Template (`Card.TemplateType`) | Default Preset | Páginas involucradas |
|------|------|------|
| `default` | `premium-dark` | MyCard, PublicCard, TemplateLibrary |
| `portfolio-creative` | (ninguno forzado) | MyCard, PublicCard, TemplateLibrary |
| `services-quotes` | `emerald-night` | MyCard, PublicCard, TemplateLibrary |
| `quote-request` | `sky-light` | MyCard, PublicCard, TemplateLibrary |
| `appointments` | `mint-breeze` | MyCard, PublicCard, TemplateLibrary |
| `reservations-range` | `soft-cream` | MyCard, PublicCard, TemplateLibrary |

## ⚠️ ALERTA: Páginas Muy Grandes

| Página | Líneas | Prioridad |
|--------|--------|-----------|
| `MyCard.razor` | 5275 | 🔴 CRÍTICA |
| `PublicCard.razor` | 2501 | 🔴 CRÍTICA |
| `TemplateLibrary.razor` | 2000+ | 🟡 MEDIA |
| `Appointments.razor` | 1683 | 🟡 MEDIA |
| `LeadDetail.razor` | 1200+ | 🟡 MEDIA |
| `Dashboard.razor` | 1200+ | 🟡 MEDIA |

### Estrategia de Refactor para Páginas Grandes

1. **Identificar secciones lógicas** (headers, forms, tabs)
2. **Extraer a componentes** en `/Components/Shared/`
3. **Pasar datos via [Parameter]** con `Compact` bool para variantes
4. **Usar EventCallback para comunicación ascendente**
5. **Respetar sync contract**: las 3 superficies (PublicCard, MyCard preview, TemplateLibrary) deben usar los mismos shared components

## Estructura de Página

```razor
@page "/ruta"
@attribute [Authorize]  @* Si requiere auth *@
@inject ServicioRequerido Service
@inject ISnackbar Snackbar

<PageTitle>Título - DataTouch</PageTitle>

@if (_isLoading)
{
    <MudProgressLinear Indeterminate="true" />
}
else
{
    @* Contenido principal *@
}

@code {
    // Variables de estado
    private bool _isLoading = true;
    
    // Lifecycle
    protected override async Task OnInitializedAsync() { }
    
    // Métodos
}
```

## Patrones para Páginas

### Loading State

```razor
@if (_isLoading)
{
    <MudProgressLinear Color="Color.Primary" Indeterminate="true" />
}
else if (_error != null)
{
    <MudAlert Severity="Severity.Error">@_error</MudAlert>
}
else
{
    @* Contenido *@
}
```

### Formularios

```razor
<MudPaper Class="pa-4">
    <MudTextField @bind-Value="_model.Name" 
                  Label="Nombre" 
                  Required="true" />
    
    <MudButton Variant="Variant.Filled" 
               Color="Color.Primary" 
               OnClick="SaveAsync"
               Disabled="_isSaving">
        @if (_isSaving) { <MudProgressCircular Size="Size.Small" /> }
        Guardar
    </MudButton>
</MudPaper>
```

## Límites

| Elemento | Máximo | Acción |
|----------|--------|--------|
| Página | 800 líneas | Extraer componentes |
| Métodos en @code | 10 | Mover a Service |
| Variables privadas | 15 | Crear ViewModel |

## ⚠️ Guardrails Críticos (MyCard.razor)

- **EF Core concurrency**: Todas las operaciones DB deben usar `_dbGate` (SemaphoreSlim). Ver CLAUDE.md sección EF Core.
- **NUNCA** usar `_ = DbContext.SaveChangesAsync()` (fire-and-forget).
- **NUNCA** llamar DbContext desde `OnParametersSetAsync` sin verificar `_initCompleted`.
- **NUNCA** agregar `style="...@_themeTokens..."` inline — usar `var(--dt-*)`.
- **SIEMPRE** respetar sync contract: shared components en las 3 superficies.

## Antes de Modificar

1. Verificar que la página no está en lock
2. Revisar `memories/blazor/PAGES.md`
3. Si página > 1000 líneas, proponer split primero
4. Consultar CLAUDE.md para patrones de la página

## Después de Modificar

1. `dotnet build`
2. `dotnet test` (9 sync contract tests deben pasar)
3. Actualizar `memories/blazor/PAGES.md`

---

*Agente: Pages Agent*
*Versión: 2.0 — Feb 2026*
