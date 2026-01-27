# 🚀 Comandos para Configurar GitFlow - Paso a Paso

## ✅ Cambios Completados

1. ✅ **Connection String actualizado** en `appsettings.json`
2. ✅ **Program.cs actualizado** de InMemory a SQL Server
3. ✅ **Scripts creados** para automatizar GitFlow

---

## 📋 Opción 1: Ejecutar Script Automático (Recomendado)

### PowerShell (Windows)

```powershell
# Navegar al proyecto
cd c:\src\DataTouch\DataTouch

# Ejecutar script
.\scripts\setup-gitflow.ps1
```

---

## 📋 Opción 2: Comandos Manuales Paso a Paso

### Paso 1: Crear Branch `develop` desde `main`

```powershell
# Asegurarse de estar en main actualizado
git checkout main
git pull origin main

# Crear develop desde main
git checkout -b develop

# Push develop al remoto
git push -u origin develop
```

**Resultado esperado:**
```
Switched to a new branch 'develop'
Total 0 (delta 0), reused 0 (delta 0)
To https://github.com/AlvarengaLeo/DataTouch.git
 * [new branch]      develop -> develop
Branch 'develop' set up to track remote branch 'develop' from 'origin'.
```

---

### Paso 2: Configurar `develop` como Default Branch

```powershell
# Opción A: Con GitHub CLI
gh repo edit --default-branch develop

# Opción B: Manual en GitHub UI
# 1. Ir a: https://github.com/AlvarengaLeo/DataTouch/settings/branches
# 2. En "Default branch", click en el ícono de cambio (⇄)
# 3. Seleccionar 'develop'
# 4. Click "Update"
```

---

### Paso 3: Proteger Branch `main` con GitHub CLI

```powershell
# Obtener nombre del repo
$REPO = gh repo view --json nameWithOwner -q .nameWithOwner

# Aplicar protección a main
gh api repos/$REPO/branches/main/protection `
  --method PUT `
  --field required_status_checks='{"strict":true,"contexts":[]}' `
  --field enforce_admins=true `
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":false}' `
  --field restrictions=null `
  --field required_linear_history=false `
  --field allow_force_pushes=false `
  --field allow_deletions=false `
  --field required_conversation_resolution=true
```

**Reglas aplicadas a `main`:**
- ❌ No commits directos
- ✅ Solo merge via Pull Request
- ✅ Requiere 1 aprobador
- ✅ Requiere resolución de conversaciones
- ✅ Incluye administradores

---

### Paso 4: Proteger Branch `develop` con GitHub CLI

```powershell
# Aplicar protección a develop
gh api repos/$REPO/branches/develop/protection `
  --method PUT `
  --field required_status_checks='{"strict":true,"contexts":[]}' `
  --field enforce_admins=true `
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":false}' `
  --field restrictions=null `
  --field required_linear_history=false `
  --field allow_force_pushes=false `
  --field allow_deletions=false `
  --field required_conversation_resolution=true
```

**Reglas aplicadas a `develop`:**
- ❌ No commits directos
- ✅ Solo merge via Pull Request
- ✅ Requiere 1 aprobador
- ✅ Requiere resolución de conversaciones
- ✅ Incluye administradores

---

### Paso 5: Verificar Configuración

```powershell
# Ver branches remotos
git branch -r

# Ver default branch
gh repo view --json defaultBranchRef -q .defaultBranchRef.name

# Ver protección de main
gh api repos/$REPO/branches/main/protection --jq '.required_pull_request_reviews.required_approving_review_count'

# Ver protección de develop
gh api repos/$REPO/branches/develop/protection --jq '.required_pull_request_reviews.required_approving_review_count'
```

**Resultado esperado:**
```
origin/develop
origin/main

develop

1

1
```

---

## 🧪 Probar Configuración

### Test 1: Intentar Push Directo a `main` (Debe Fallar)

```powershell
git checkout main
echo "test" >> test.txt
git add test.txt
git commit -m "test: verificar protección"
git push origin main
```

**Resultado esperado:**
```
remote: error: GH006: Protected branch update failed for refs/heads/main.
❌ CORRECTO - El push debe ser rechazado
```

### Test 2: Crear Feature Branch y PR (Debe Funcionar)

```powershell
git checkout develop
git checkout -b feature/test-gitflow
echo "# Test" >> test.md
git add test.md
git commit -m "docs: test gitflow"
git push -u origin feature/test-gitflow

# Crear PR
gh pr create --base develop --head feature/test-gitflow --title "Test GitFlow" --body "Testing branch protection"
```

**Resultado esperado:**
```
✅ PR creado exitosamente
✅ Requiere 1 aprobación antes de merge
```

---

## 📝 Próximos Pasos Después de Configurar GitFlow

### 1. Commit de Cambios de SQL Server

```powershell
# Volver a develop
git checkout develop

# Crear feature branch
git checkout -b feature/sql-server-migration

# Agregar cambios
git add src/DataTouch.Web/appsettings.json
git add src/DataTouch.Web/Program.cs
git add sql/migrations/
git add docs/
git add scripts/
git add README.md

# Commit
git commit -m "feat: migrar de InMemory a SQL Server

- Actualizar connection string a SQL Server remoto
- Cambiar DbContext de UseInMemoryDatabase a UseSqlServer
- Agregar scripts DDL para SQL Server
- Agregar scripts de seed data
- Actualizar README con instrucciones de SQL Server
- Agregar documentación de GitFlow
- Crear scripts de automatización de GitFlow"

# Push
git push -u origin feature/sql-server-migration

# Crear PR
gh pr create --base develop --head feature/sql-server-migration --title "feat: Migración a SQL Server" --body "Migración completa de InMemory a SQL Server con scripts DDL y documentación"
```

### 2. Ejecutar Scripts SQL en el Servidor

```powershell
# Opción A: Con sqlcmd (si tienes acceso directo)
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d master -i sql/migrations/001_InitialCreate_SQLServer.sql
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d BodaDB -i sql/migrations/002_SeedData_SQLServer.sql

# Opción B: Con SSMS
# 1. Conectar a 162.248.54.184 con usuario 'sia'
# 2. Abrir y ejecutar sql/migrations/001_InitialCreate_SQLServer.sql
# 3. Abrir y ejecutar sql/migrations/002_SeedData_SQLServer.sql
```

### 3. Probar la Aplicación

```powershell
cd src/DataTouch.Web
dotnet run

# Abrir navegador en: https://localhost:5001
# Login con: admin@techcorp.com / admin123
```

---

## ❓ FAQ

**¿Necesito instalar GitHub CLI?**
Sí, si quieres usar los comandos `gh`. Instalar con:
```powershell
winget install GitHub.cli
```

**¿Puedo configurar branch protection sin GitHub CLI?**
Sí, manualmente en GitHub UI:
1. Settings → Branches → Add rule
2. Seguir instrucciones en `docs/GITHUB_SETUP.md`

**¿Qué pasa si ya tengo cambios sin commit?**
Primero haz commit o stash:
```powershell
# Opción 1: Commit
git add .
git commit -m "wip: cambios en progreso"

# Opción 2: Stash
git stash
# ... hacer setup de gitflow ...
git stash pop
```

**¿Cómo verifico que las reglas están aplicadas?**
```powershell
# Ver en GitHub UI
# https://github.com/AlvarengaLeo/DataTouch/settings/branches

# O con CLI
gh api repos/AlvarengaLeo/DataTouch/branches/main/protection
```

---

## 🎯 Resumen de Comandos Rápidos

```powershell
# Setup completo automático
cd c:\src\DataTouch\DataTouch
.\scripts\setup-gitflow.ps1

# O manual:
git checkout main && git pull
git checkout -b develop && git push -u origin develop
gh repo edit --default-branch develop
gh api repos/$(gh repo view --json nameWithOwner -q .nameWithOwner)/branches/main/protection --method PUT --field required_pull_request_reviews='{"required_approving_review_count":1}'
gh api repos/$(gh repo view --json nameWithOwner -q .nameWithOwner)/branches/develop/protection --method PUT --field required_pull_request_reviews='{"required_approving_review_count":1}'
```

---

*Guía de Comandos GitFlow - Enero 2026*
