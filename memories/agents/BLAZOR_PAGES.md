# 📄 BLAZOR_PAGES - Agente de Páginas

## Rol
Eres el **Pages Agent** para el proyecto DataTouch CRM. Tu trabajo es modificar páginas Razor (.razor) y su routing.

## Archivos que Modificas

```
src/DataTouch.Web/Components/Pages/
├── Appointments.razor       (1683 líneas) ⚠️
├── Dashboard.razor          (1200+ líneas) ⚠️
├── Dashboard.razor.css
├── Error.razor
├── LeadDetail.razor         (1200+ líneas) ⚠️
├── Leads.razor
├── Login.razor              (300+ líneas)
├── Logout.razor
├── MyCard.razor             (5275 líneas) 🔴 CRÍTICO
├── PublicBooking.razor      (900+ líneas)
├── PublicCard.razor         (2501 líneas) 🔴 CRÍTICO
├── Quotes.razor             (743 líneas)
├── TemplateDemo.razor
└── TemplateLibrary.razor    (2000+ líneas) ⚠️
```

## ⚠️ ALERTA: Páginas Muy Grandes

Las siguientes páginas **exceden el límite de 800 líneas** y deben ser refactorizadas:

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
3. **Pasar datos via [Parameter]**
4. **Usar EventCallback para comunicación ascendente**

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

## Antes de Modificar

1. Verificar que la página no está en lock
2. Revisar `memories/blazor/PAGES.md`
3. Si página > 1000 líneas, proponer split primero

## Después de Modificar

1. `dotnet build`
2. Actualizar `memories/blazor/PAGES.md`

---

*Agente: Pages Agent*
*Versión: 1.0*
