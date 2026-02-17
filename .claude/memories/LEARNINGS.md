# 💡 LEARNINGS.md - Lecciones Aprendidas

## Enero 2026

### Setup Multi-Agente

| Aprendizaje | Detalle |
|-------------|---------|
| **Estructura primero** | Crear /memories antes de empezar a trabajar con agentes |
| **Auto-generar donde posible** | Scripts de PowerShell pueden generar ENTITIES.md, SERVICES.md |
| **Locks son críticos** | Múltiples agentes modificando el mismo archivo causa conflictos |

---

## Febrero 2026

### 2026-02-11 — EF Core Concurrency Crash en Blazor

**Contexto**: Seleccionar template desde `/templates` → navegar a `/cards/mine?template=...`

**Problema**: `InvalidOperationException: A second operation was started on this context instance` crasheaba el circuito Blazor. Causa: fire-and-forget `_ = DbContext.SaveChangesAsync()` + overlap de lifecycle methods.

**Solución**: `SemaphoreSlim _dbGate(1,1)` para serializar todas las operaciones DB. Flag `_pendingDbSave` en lugar de fire-and-forget. Guard `_initCompleted` en `OnParametersSetAsync`.

**Prevención**: NUNCA usar fire-and-forget con DbContext en Blazor. SIEMPRE wrappear operaciones DB en `_dbGate`. Ver CLAUDE.md sección "EF Core Concurrency Guardrails".

---

### 2026-02-11 — Sync Contract: 5 causas de desincronización visual

**Contexto**: Las 3 superficies de rendering (PublicCard, MyCard preview, TemplateLibrary) mostraban diferencias visuales.

**Problema**: (1) Markup duplicado para social icons, (2) Hex hardcodeados en template preview, (3) Inline `_themeTokens` styles, (4) Dos sistemas CSS vars, (5) `CardStyleModel` duplicado.

**Solución**: Shared components (`SocialLinksRow`, `QuoteRequestBlock`), migración a `var(--dt-*)`, extracción de `CardStyleModel` a `Models/`, bridge aliases `--surface-*`.

**Prevención**: Seguir guardrails de CLAUDE.md sección I. Ejecutar `dotnet test` (9 sync contract tests). Grep checks en debug checklist sección H.

---

### 2026-02-11/13 — Implementación de Templates 4 y 5

**Contexto**: Agregar templates "Citas (Agenda)" y "Reservas (Rango de Fechas)".

**Problema**: El checklist de 10 pasos (CLAUDE.md sección F) requiere cambios coordinados en 6+ archivos. Olvidar un paso causa desync.

**Solución**: Seguir el checklist de 10 pasos al pie de la letra. Crear shared component primero, luego registrar en las 3 superficies, luego servicios, luego tests.

**Prevención**: Siempre usar el checklist de CLAUDE.md sección F "HOW TO ADD A NEW TEMPLATE TYPE" como guía obligatoria.

---

### 2026-02-16 — Drift entre documentación y código

**Contexto**: La carpeta `.claude/` tenía memorias y agentes desactualizados (conteos de Enero 2026).

**Problema**: 2 entidades, 4 servicios, 5 componentes y 3 modelos no estaban documentados. Los agentes referenciaban conteos incorrectos.

**Solución**: Auditoría completa comparando archivos reales vs documentación. Actualización de los 11 agentes + 8 memorias + skills.

**Prevención**: Ejecutar Validation Agent después de cada feature mayor. Mantener CURRENT_SPRINT.md actualizado.

---

*Última actualización: 2026-02-16*
