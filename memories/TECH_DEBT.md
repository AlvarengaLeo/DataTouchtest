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

## Resumen

| Prioridad | Items | Horas Estimadas |
|-----------|-------|-----------------|
| 🔴 Alta | 3 | 10 horas |
| 🟡 Media | 3 | 26 horas |
| 🟢 Baja | 2 | 5 horas |
| **Total** | **8** | **41 horas** |

---

*Última actualización: 2026-01-20*
