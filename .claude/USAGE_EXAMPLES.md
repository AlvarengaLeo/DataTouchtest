# 🎯 Ejemplos de Uso de Agentes y Skills

> Guía práctica con prompts reales para utilizar cada agente y skill del sistema multi-agente DataTouch CRM.

---

## 1. Orchestrator Agent — Coordinar tareas complejas

### Ejemplo: Agregar un nuevo template "Eventos"

```
Prompt al Orchestrator:
"Necesito agregar un nuevo template 'Eventos' (key: events) con preset ocean-mist. 
Debe tener un componente EventBookingBlock con lista de próximos eventos y botón de registro."
```

**Lo que hace el Orchestrator:**
1. Delega a Planning Agent para descomponer
2. Recibe plan de 9 subtareas (ver CLAUDE.md sección F)
3. Asigna en orden: Entity → DbContext → UI (shared component) → Pages (3 superficies) → Services → Testing → Docs
4. Verifica locks antes de cada asignación
5. Al final, ejecuta `dotnet build` y `dotnet test`

---

## 2. Planning Agent — Descomponer tareas

### Ejemplo: Descomponer "Agregar sistema de notificaciones"

```
Prompt al Planning Agent:
"Analiza y descompone: Agregar sistema de notificaciones por email cuando un visitante 
solicita una cita desde la tarjeta pública."
```

**Output esperado:**
```markdown
## Plan de Ejecución: Notificaciones Email

### Análisis Inicial
- Complejidad: 4/5
- Archivos afectados: 6
- Agentes requeridos: Entity, DbContext, Services, Pages, Testing

### Subtareas
1. Entity Agent: Agregar NotificationPreferences a User
2. DbContext Agent: Config + migración SQL
3. Services Agent: Crear EmailNotificationService
4. Pages Agent: Toggle en MyCard.razor (sección configuración)
5. Services Agent: Hook en AppointmentService.CreatePublicAppointmentAsync
6. Testing Agent: Tests para NotificationService
7. Validation Agent: Verificar consistencia
```

---

## 3. Pages Agent — Modificar páginas Razor

### Ejemplo: Agregar sección al editor MyCard

```
Prompt al Pages Agent:
"Agregar una nueva sección 'Horarios Especiales' en MyCard.razor para el template 
appointments. Debe aparecer después de la sección de servicios y permitir configurar 
días festivos bloqueados."
```

**Checklist que sigue el agente:**
1. Verifica lock en CURRENT_SPRINT.md
2. Lee CLAUDE.md para patrones de MyCard
3. Agrega `_isAppointmentsTemplate` guard
4. Usa `var(--dt-*)` para colores
5. Wrappea DB ops en `_dbGate`
6. Ejecuta `dotnet build` + `dotnet test`

### Ejemplo: Agregar nueva ruta pública

```
Prompt al Pages Agent:
"Crear página PublicEvents.razor en /p/{org}/{slug}/events que muestre la lista 
de eventos del template 'events'. Usar EmptyLayout."
```

---

## 4. UI Agent — Crear/modificar componentes compartidos

### Ejemplo: Crear componente synced

```
Prompt al UI Agent:
"Crear EventBookingBlock.razor en Components/Shared/ que se renderice en las 3 superficies.
Debe aceptar parámetro Compact (bool) y Events (List<EventData>). 
Usar var(--dt-*) para colores."
```

**Patrón que sigue:**
```razor
@* Components/Shared/EventBookingBlock.razor *@
<div class="event-booking-block @(Compact ? "event-booking-compact" : "")" 
     style="@ContainerStyle">
    @foreach (var evt in Events)
    {
        <div class="event-card">...</div>
    }
</div>

@code {
    [Parameter] public bool Compact { get; set; }
    [Parameter] public string? ContainerStyle { get; set; }
    [Parameter] public List<EventData> Events { get; set; } = new();
    [Parameter] public EventCallback<EventData> OnRegisterClick { get; set; }
}
```

### Ejemplo: Modificar componente existente

```
Prompt al UI Agent:
"Agregar parámetro ShowPrice (bool, default true) a AppointmentBookingBlock.razor. 
Cuando es false, ocultar el chip de precio en cada servicio."
```

---

## 5. Services Agent — Crear/modificar servicios

### Ejemplo: Crear servicio nuevo

```
Prompt al Services Agent:
"Crear EventService.cs con métodos:
- GetUpcomingEventsAsync(Guid cardId) → List<Event>
- RegisterAttendeeAsync(Guid eventId, string name, string email) → (bool, Event?, string?)
- GetEventStatsAsync(Guid cardId) → EventStats
Registrar en Program.cs."
```

### Ejemplo: Agregar método a servicio existente

```
Prompt al Services Agent:
"Agregar método GetReservationsByDateRangeAsync(Guid cardId, DateTime from, DateTime to) 
a ReservationService.cs. Incluir eager loading de ReservationResource."
```

### Ejemplo: Crear dashboard service

```
Prompt al Services Agent:
"Crear EventDashboardService.cs usando IDbContextFactory (patrón de AppointmentDashboardService).
Métodos: GetKpisAsync, GetEventsByStatusChart, GetAttendanceByMonthChart."
```

---

## 6. Entity Agent — Crear/modificar entidades

### Ejemplo: Crear entidad nueva

```
Prompt al Entity Agent:
"Crear entidad Event en Domain/Entities/Event.cs con:
- Id (Guid), CardId (Guid FK), Title, Description, Location
- StartDateTime, EndDateTime
- MaxAttendees (int), IsActive (bool)
- CreatedAt, UpdatedAt
- Navigation: Card, ICollection<EventAttendee>"
```

### Ejemplo: Agregar propiedad a entidad existente

```
Prompt al Entity Agent:
"Agregar propiedad CancellationPolicy (string?, max 1000) a ReservationRequest.cs.
Notificar a DbContext Agent para configuración."
```

---

## 7. DbContext Agent — Configurar EF Core

### Ejemplo: Agregar nueva entidad al DbContext

```
Prompt al DbContext Agent:
"Agregar DbSet<Event> Events al DataTouchDbContext. Configurar en OnModelCreating:
- PK: Id, String props con MaxLength
- FK a Card con Cascade delete
- Index en (CardId, StartDateTime)
- Crear migración SQL en sql/migrations/"
```

### Ejemplo: Schema update (patrón establecido)

```
Prompt al DbContext Agent:
"Agregar columna CancellationPolicy a tabla ReservationRequests usando el patrón 
DbInitializer.ApplySchemaUpdatesAsync (raw SQL, no EF Migrations)."
```

---

## 8. Testing Agent — Crear tests

### Ejemplo: Tests para servicio nuevo

```
Prompt al Testing Agent:
"Crear tests para ReservationService:
1. SubmitReservationAsync_WithValidData_ReturnsSuccess
2. SubmitReservationAsync_WithBlockedDate_ReturnsFalse
3. GetReservationsByCardAsync_ReturnsOrderedList
4. UpdateStatusAsync_WhenNotFound_ReturnsFalse
Usar InMemory DB + xUnit + FluentAssertions."
```

### Ejemplo: Ejecutar tests existentes

```bash
# Todos los tests
dotnet test

# Solo sync contract
dotnet test --filter "FullyQualifiedName~SyncContract"

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## 9. Review Agent — Code review

### Ejemplo: Revisar cambio antes de merge

```
Prompt al Review Agent:
"Revisar los cambios en MyCard.razor para la nueva sección de horarios especiales. 
Verificar: sync contract, guardrails de CLAUDE.md, EF Core concurrency, CSS vars."
```

**Checklist que aplica (REVIEW_CHECKLIST.md):**
- Build pasa sin errores
- No hay hex hardcodeado en componentes synced
- No hay `style="...@_themeTokens..."` inline
- DB operations usan `_dbGate`
- 9 sync contract tests pasan

---

## 10. Docs Agent — Documentación

### Ejemplo: Documentar nuevo template

```
Prompt al Docs Agent:
"Documentar Template 6 'Eventos' en:
1. CLAUDE.md sección J (changelog)
2. memories/CONTEXT.md (agregar a tabla de templates)
3. memories/blazor/COMPONENTS.md (nuevo shared component)
4. memories/blazor/SERVICES.md (nuevo servicio)"
```

### Ejemplo: Actualizar memorias después de cambio

```
Prompt al Docs Agent:
"Se agregó propiedad UseGlobalSchedule a Service entity. Actualizar:
- memories/domain/ENTITIES.md
- memories/blazor/SERVICES.md (si afecta AvailabilityService)"
```

---

## 11. Validation Agent — Verificar consistencia

### Ejemplo: Validación completa post-sprint

```
Prompt al Validation Agent:
"Ejecutar validación completa:
1. Verificar 18 entidades vs ENTITIES.md
2. Verificar 17 servicios vs SERVICES.md
3. Verificar 18 componentes vs COMPONENTS.md
4. Ejecutar dotnet build + dotnet test
5. Verificar sync contract (grep checks)
6. Generar reporte"
```

### Ejemplo: Quick validation

```
Prompt al Validation Agent:
"Quick check: ¿Los conteos de entidades, servicios y componentes en las memorias 
coinciden con el código real?"
```

---

## Flujo Completo: Agregar Template 6 (paso a paso)

```
1. USUARIO → ORCHESTRATOR:
   "Agregar template 'Eventos' (key: events, preset: ocean-mist)"

2. ORCHESTRATOR → PLANNING:
   "Descomponer: nuevo template events"

3. PLANNING → ORCHESTRATOR:
   Plan de 9 subtareas (CLAUDE.md sección F checklist)

4. ORCHESTRATOR → ENTITY AGENT:
   "Crear Event + EventAttendee entities"

5. ORCHESTRATOR → DBCONTEXT AGENT:
   "Agregar DbSets + config + migración"

6. ORCHESTRATOR → UI AGENT:
   "Crear EventBookingBlock.razor (synced, Compact param)"

7. ORCHESTRATOR → PAGES AGENT:
   "Registrar en TemplateLibrary + MyCard (_isEventsTemplate) + PublicCard"

8. ORCHESTRATOR → SERVICES AGENT:
   "Crear EventService + EventDashboardService + actualizar CardService"

9. ORCHESTRATOR → TESTING AGENT:
   "Tests para EventService + sync contract"

10. ORCHESTRATOR → VALIDATION AGENT:
    "dotnet build + dotnet test + validar memorias"

11. ORCHESTRATOR → DOCS AGENT:
    "Actualizar CLAUDE.md, memorias, CURRENT_SPRINT.md"
```

---

## Skills: Uso del settings.local.json

El archivo `skills/settings.local.json` define permisos auto-aprobados para Claude Code:

```json
{
  "permissions": {
    "allow": [
      "Bash(dotnet build:*)",    // Build sin aprobación
      "Bash(dotnet test:*)",     // Tests sin aprobación
      "Bash(dotnet run:*)",      // Run sin aprobación
      "Bash(grep:*)",            // Búsquedas
      "Bash(git status:*)",      // Git status
      "Bash(git log:*)",         // Git log
      "Bash(git diff:*)"         // Git diff
    ]
  }
}
```

**Comandos frecuentes pre-aprobados:**
```bash
dotnet build                                    # Verificar compilación
dotnet test                                     # Ejecutar 9 sync contract tests
dotnet test --filter "SyncContract"             # Solo tests de sincronización
grep -r "class CardStyleModel" src/             # Verificar 1 sola definición
grep -r 'style=".*_themeTokens' src/            # Debe retornar 0 resultados
git status                                      # Ver cambios pendientes
git diff --name-only                            # Archivos modificados
```

---

*Versión: 1.0 — Feb 2026*
