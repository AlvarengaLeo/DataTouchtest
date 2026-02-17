# 🔧 TECH_DEBT.md - Deuda Técnica

## Prioridad Alta 🔴

### 1. MyCard.razor - Componente Monolítico
- **Archivo**: `Components/Pages/MyCard.razor`
- **Líneas**: 5275
- **Problema**: Excede límite de 800 líneas por 6x
- **Impacto**: Difícil de mantener, tests imposibles
- **Solución propuesta**: 
  - Extraer AppearanceSection
  - Extraer IdentitySection
  - Extraer ContactSection
  - Extraer GallerySection
- **Estimación**: 4 horas

### 2. PublicCard.razor - Componente Grande
- **Archivo**: `Components/Pages/PublicCard.razor`
- **Líneas**: 2501
- **Problema**: Excede límite de 800 líneas por 3x
- **Impacto**: Lógica de temas mezclada con UI
- **Solución propuesta**:
  - Extraer lógica de temas a ThemeService
  - Crear componentes para secciones
- **Estimación**: 3 horas

### 3. DashboardService - God Service
- **Archivo**: `Services/DashboardService.cs`
- **Líneas**: 1010
- **Métodos**: 37
- **Problema**: Excede límite de 500 líneas por 2x
- **Solución propuesta**:
  - Crear KpiService
  - Crear ChartDataService
  - Crear InsightsService
- **Estimación**: 3 horas

---

## Prioridad Media 🟡

### 4. Sin Repository Pattern
- **Problema**: Services acceden a DbContext directamente
- **Impacto**: Tests más difíciles, acoplamiento alto
- **Solución**: Agregar IRepository<T> interfaces
- **Estimación**: 8 horas

### 5. DTOs Inline
- **Problema**: DTOs definidos al final de servicios
- **Impacto**: Difícil encontrar, no reutilizables
- **Solución**: Mover a carpeta Models/
- **Estimación**: 2 horas

### 6. Cobertura de Tests 0%
- **Problema**: Sin tests unitarios
- **Impacto**: Refactors riesgosos
- **Solución**: Agregar tests para servicios críticos
- **Estimación**: 16 horas

---

## Prioridad Baja 🟢

### 7. Warnings MudBlazor (17)
- **Problema**: Atributos obsoletos
- **Impacto**: Warnings en build
- **Solución**: Actualizar a nuevos atributos
- **Estimación**: 1 hora

### 8. Logging Estructurado
- **Problema**: Sin Serilog ni logging consistente
- **Impacto**: Debugging en producción difícil
- **Solución**: Agregar Serilog
- **Estimación**: 4 horas

---

## Deuda Técnica Aceptada (by design)

| Item | Razón | Ref CLAUDE.md |
|------|-------|---------------|
| Template registry desacoplado de DB | Fast iteration, `_templates` in-memory | Sección I, #9 |
| Preset forzado sobreescribe preferencia usuario | Templates requieren themes específicos | Sección I, #10 |
| CTA buttons/chips/avatar 3 implementaciones | Usan `var(--dt-*)`, baja divergencia visual | Sección I, #11 |
| Gallery chrome hardcoded hex | Solo afecta iPhone frame, no card content | Sección I, #12 |
| `SocialLinksModel` privado/duplicado | Simple DTO sin defaults | Sección I, #13 |

## Deuda Técnica Resuelta ✅

| Item | Fecha | Solución |
|------|-------|----------|
| CardStyleModel duplicado | 2026-02-11 | Extraído a `Models/CardStyleModel.cs` |
| Hex hardcodeados en TemplateLibrary | 2026-02-11 | Migrado a `var(--dt-*)` |
| Dos sistemas CSS vars en paralelo | 2026-02-11 | Bridge aliases en ThemeHelper |
| Inline styles bypass CSS vars | 2026-02-11 | Zero `style=".*_themeTokens"` |
| Sin CardService | 2026-02-11 | Creado `Services/CardService.cs` |
| Social icons shape distinto | 2026-02-11 | `SocialLinksRow.razor` compartido |
| Sin tests de sincronización | 2026-02-11 | `SyncContractTests.cs` (9 tests) |
| QuoteSettingsJson pérdida en switch | 2026-02-11 | SaveCard() siempre serializa |
| EF Core concurrency crash | 2026-02-11 | `_dbGate` SemaphoreSlim + `_pendingDbSave` |

---

## Resumen

| Prioridad | Items Pendientes | Horas Estimadas |
|-----------|-----------------|-----------------|
| 🔴 Alta | 3 | 10 horas |
| 🟡 Media | 3 | 26 horas |
| 🟢 Baja | 2 | 5 horas |
| ✅ Resueltos | 9 | — |
| **Total pendiente** | **8** | **41 horas** |

---

*Última actualización: 2026-02-16*
