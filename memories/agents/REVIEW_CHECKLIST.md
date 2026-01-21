# 🔍 REVIEW_CHECKLIST - Checklist de Review

## Para Review Agent

Este documento define los criterios de revisión de código para DataTouch CRM.

---

## Checklist General

### ✅ Build y Compilación
- [ ] `dotnet build` pasa sin errores
- [ ] No hay nuevos warnings introducidos
- [ ] Todos los imports/usings son necesarios

### ✅ Convenciones
- [ ] Nomenclatura sigue STANDARDS.md
- [ ] No hay magic strings (usar constantes)
- [ ] Comentarios en español o inglés (consistente)

### ✅ Límites de Complejidad
- [ ] Páginas < 800 líneas
- [ ] Componentes < 300 líneas
- [ ] Services < 500 líneas
- [ ] Métodos < 50 líneas
- [ ] Parámetros < 5 por método

---

## Checklist por Tipo de Archivo

### 📄 Para Páginas Blazor (.razor)

- [ ] Usa servicios inyectados, no DbContext directo
- [ ] Loading state manejado
- [ ] Error handling con Snackbar
- [ ] No lógica de negocio en @code (mover a service)
- [ ] Menos de 10 métodos en @code

### ⚙️ Para Servicios (.cs)

- [ ] Constructor injection para DbContext
- [ ] Métodos async usan `Async` suffix
- [ ] Resultado estructurado (Success, Data, Error)
- [ ] Includes explícitos en queries EF
- [ ] Sin lógica en catch (solo logging/return)

### 📦 Para Entidades

- [ ] Primary key es `Id` (Guid)
- [ ] FKs siguen patrón `{Entity}Id`
- [ ] Navigation properties inicializadas
- [ ] XML documentation en propiedades no obvias

---

## Red Flags 🚩

### Rechazar si:
- [ ] Commit de archivos > 1000 líneas sin split
- [ ] DbContext inyectado directamente en página
- [ ] Passwords en código (sin hash)
- [ ] Queries sin paginación en listas públicas
- [ ] No hay manejo de null en navigation properties

### Advertir si:
- [ ] Servicio cerca del límite (>400 líneas)
- [ ] Más de 3 niveles de anidación
- [ ] Múltiples responsabilidades en un método
- [ ] Tests faltantes para lógica nueva

---

## Feedback Template

```markdown
## 📋 Code Review: [Nombre del cambio]

### ✅ Aspectos Positivos
- [Qué está bien]

### ⚠️ Sugerencias
- **Archivo**: [path]
- **Línea**: [N]
- **Issue**: [descripción]
- **Sugerencia**: [cómo mejorar]

### 🚩 Bloqueantes (si aplica)
- **Issue**: [descripción]
- **Razón**: [por qué no puede pasar]

### Veredicto
- [ ] ✅ Aprobado
- [ ] ⚠️ Aprobado con comentarios
- [ ] ❌ Requiere cambios
```

---

*Versión: 1.0*
