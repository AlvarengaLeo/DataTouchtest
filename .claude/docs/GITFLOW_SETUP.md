# 🚀 Script de Inicialización de GitFlow

Este script configura la estructura de branches de GitFlow para DataTouch.

---

## 📋 Pasos de Ejecución

### 1. Verificar Estado Actual

```bash
# Ver branches actuales
git branch -a

# Ver branch actual
git branch --show-current

# Ver commits recientes
git log --oneline -5
```

---

### 2. Crear Branch `main` desde `develop`

```bash
# Asegurarse de estar en develop actualizado
git checkout develop
git pull origin develop

# Crear branch main desde develop
git checkout -b main

# Push main al remoto
git push -u origin main

# Volver a develop
git checkout develop
```

---

### 3. Configurar `main` como Default Branch en GitHub

**Opción A: GitHub UI**
1. Ir a: `https://github.com/AlvarengaLeo/DataTouch/settings/branches`
2. En "Default branch", click en el ícono de cambio (⇄)
3. Seleccionar `main`
4. Click "Update"
5. Confirmar el cambio

**Opción B: GitHub CLI**
```bash
gh repo edit --default-branch main
```

---

### 4. Proteger Branches (Seguir GITHUB_SETUP.md)

Ver instrucciones detalladas en [`docs/GITHUB_SETUP.md`](./GITHUB_SETUP.md)

**Resumen rápido:**
1. Ir a Settings → Branches
2. Add rule para `main`:
   - Require PR
   - Require 1 approval
   - Require status checks
   - Include administrators
3. Add rule para `develop`:
   - Require PR
   - Require 1 approval
   - Require status checks
   - Include administrators

---

### 5. Crear Primera Feature Branch (Ejemplo)

```bash
# Desde develop
git checkout develop
git pull origin develop

# Crear feature branch
git checkout -b feature/sql-server-migration

# Hacer cambios...
# (Agregar paquetes NuGet, actualizar Program.cs, etc.)

# Commit
git add .
git commit -m "feat: migrar de InMemory a SQL Server"

# Push
git push -u origin feature/sql-server-migration

# Crear PR en GitHub:
# Base: develop
# Compare: feature/sql-server-migration
```

---

## 🔄 Workflow Completo de Ejemplo

### Escenario: Agregar nueva funcionalidad

```bash
# 1. Actualizar develop
git checkout develop
git pull origin develop

# 2. Crear feature branch
git checkout -b feature/analytics-export

# 3. Desarrollar
# ... hacer cambios en el código ...

# 4. Commits frecuentes
git add src/DataTouch.Web/Services/AnalyticsExportService.cs
git commit -m "feat: agregar servicio de exportación de analytics"

git add src/DataTouch.Web/Components/Pages/Analytics.razor
git commit -m "feat: agregar botón de exportar en página de analytics"

git add tests/DataTouch.Tests/AnalyticsExportServiceTests.cs
git commit -m "test: agregar tests para AnalyticsExportService"

# 5. Push
git push -u origin feature/analytics-export

# 6. Crear Pull Request en GitHub
# - Ir a https://github.com/AlvarengaLeo/DataTouch/pulls
# - Click "New pull request"
# - Base: develop
# - Compare: feature/analytics-export
# - Llenar título y descripción
# - Asignar reviewer
# - Click "Create pull request"

# 7. Esperar aprobación y merge

# 8. Después del merge, limpiar
git checkout develop
git pull origin develop
git branch -d feature/analytics-export
```

---

## 🔥 Hotfix de Emergencia

```bash
# 1. Crear hotfix desde main
git checkout main
git pull origin main
git checkout -b hotfix/critical-security-fix

# 2. Hacer fix
# ... corregir el problema ...

# 3. Commit
git add .
git commit -m "hotfix: corregir vulnerabilidad XSS en formulario de contacto"

# 4. Push
git push -u origin hotfix/critical-security-fix

# 5. Crear 2 PRs:
# PR 1: hotfix/critical-security-fix → main
# PR 2: hotfix/critical-security-fix → develop

# 6. Después de ambos merges, limpiar
git checkout develop
git pull origin develop
git branch -d hotfix/critical-security-fix
```

---

## 📦 Release Flow (develop → main)

```bash
# 1. Verificar que develop está estable
git checkout develop
git pull origin develop

# 2. Crear PR en GitHub
# Base: main
# Compare: develop
# Título: "Release v1.0.0"
# Descripción: Changelog completo

# 3. Después del merge, crear tag
git checkout main
git pull origin main
git tag -a v1.0.0 -m "Release version 1.0.0 - Initial production release"
git push origin v1.0.0

# 4. Crear GitHub Release
gh release create v1.0.0 \
  --title "v1.0.0 - Initial Release" \
  --notes "Primera versión de producción de DataTouch CRM"
```

---

## ✅ Verificación de Configuración

### Test 1: Intentar Push Directo a `main` (Debe Fallar)

```bash
git checkout main
echo "test" >> test.txt
git add test.txt
git commit -m "test: verificar protección"
git push origin main

# Resultado esperado:
# remote: error: GH006: Protected branch update failed
# ❌ Este push debe ser rechazado
```

### Test 2: Intentar Push Directo a `develop` (Debe Fallar)

```bash
git checkout develop
echo "test" >> test.txt
git add test.txt
git commit -m "test: verificar protección"
git push origin develop

# Resultado esperado:
# remote: error: GH006: Protected branch update failed
# ❌ Este push debe ser rechazado
```

### Test 3: Crear Feature Branch y PR (Debe Funcionar)

```bash
git checkout develop
git pull origin develop
git checkout -b feature/test-gitflow
echo "# Test GitFlow" >> test-gitflow.md
git add test-gitflow.md
git commit -m "docs: agregar test de gitflow"
git push -u origin feature/test-gitflow

# Crear PR en GitHub
# ✅ Debe permitir crear PR
# ✅ Debe requerir 1 aprobación
# ✅ Debe permitir merge después de aprobación
```

---

## 🛠️ Comandos Útiles

### Ver Branches

```bash
# Locales
git branch

# Remotos
git branch -r

# Todos
git branch -a

# Con último commit
git branch -v
```

### Limpiar Branches Eliminados

```bash
# Eliminar referencias a branches remotos eliminados
git fetch --prune

# Eliminar branches locales ya mergeados
git branch --merged | grep -v "\*" | grep -v "main" | grep -v "develop" | xargs -n 1 git branch -d
```

### Ver Estado del Repositorio

```bash
# Status
git status

# Log gráfico
git log --oneline --graph --all --decorate

# Ver PRs abiertos
gh pr list

# Ver último release
gh release view
```

---

## 📚 Referencias

- [GitFlow Workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow)
- [GitHub Flow](https://docs.github.com/en/get-started/quickstart/github-flow)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

*Script de Inicialización de GitFlow para DataTouch - Enero 2026*
