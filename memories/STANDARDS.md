# 📏 STANDARDS.md - Estándares y Convenciones de Código

## Límites de Complejidad

### Blazor (.razor)

| Elemento | Máximo | Acción si excede |
|----------|--------|------------------|
| **Página** | 800 líneas | Extraer a componentes |
| **Componente** | 300 líneas | Split en subcomponents |
| **Métodos en @code** | 10 | Mover a Service |
| **Parámetros [Parameter]** | 5 | Crear ViewModel |
| **Niveles de anidación** | 3 | Extraer a método |

### C# Services

| Elemento | Máximo | Acción si excede |
|----------|--------|------------------|
| **Service** | 500 líneas | Split por responsabilidad |
| **Métodos por service** | 15 | Crear sub-services |
| **Parámetros por método** | 5 | Crear DTO |
| **Complejidad ciclomática** | 10 | Refactorizar |

### Entidades

| Elemento | Máximo | Acción si excede |
|----------|--------|------------------|
| **Propiedades** | 30 | Evaluar Value Objects |
| **Navigation Properties** | 10 | Split de entidad |

---

## Convenciones de Nomenclatura

### C#

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Clases | PascalCase | `AppointmentService` |
| Interfaces | IPascalCase | `IQuoteRepository` |
| Métodos públicos | PascalCase | `GetByIdAsync()` |
| Métodos privados | _camelCase | `_calculateSlots()` |
| Parámetros | camelCase | `cardId`, `startDate` |
| Constantes | UPPER_SNAKE | `MAX_APPOINTMENTS` |

### Blazor

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Páginas | PascalCase.razor | `MyCard.razor` |
| Componentes | PascalCase.razor | `CardPreview.razor` |
| Variables privadas | _camelCase | `_isLoading` |
| Parámetros | PascalCase | `[Parameter] public Guid CardId` |

### Archivos

| Tipo | Convención | Ejemplo |
|------|------------|---------|
| Entidades | Singular.cs | `Card.cs`, `Appointment.cs` |
| Servicios | NombreService.cs | `QuoteService.cs` |
| DTOs | NombreDto.cs | `CreateAppointmentDto.cs` |

---

## Patrones Requeridos

### Servicios

```csharp
// ✅ CORRECTO: Método con resultado estructurado
public async Task<(bool Success, T? Data, string? Error)> OperationAsync()
{
    try {
        // lógica
        return (true, result, null);
    } catch (Exception ex) {
        return (false, default, ex.Message);
    }
}
```

### Componentes Blazor

```razor
@* ✅ CORRECTO: Inyección de servicios *@
@inject AppointmentService AppointmentService
@inject ISnackbar Snackbar

@* ❌ INCORRECTO: DbContext directo *@
@inject DataTouchDbContext DbContext
```

---

## Commits

```
tipo(scope): descripción corta

Tipos válidos:
- feat: Nueva funcionalidad
- fix: Corrección de bug
- refactor: Refactorización sin cambio de funcionalidad  
- docs: Documentación
- test: Tests
- agent(nombre): Cambio hecho por agente específico
```

---

*Última actualización: Enero 2026*
