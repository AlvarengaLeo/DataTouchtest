# 🔧 Configuración de GitHub - Branch Protection

Este documento contiene los comandos y configuraciones necesarias para establecer las reglas de protección de branches en GitHub.

---

## 📋 Pasos para Configurar Branch Protection

### Opción 1: Configuración Manual en GitHub UI

#### 1. Proteger Branch `main`

1. Ir a tu repositorio en GitHub
2. Click en **Settings** (⚙️)
3. En el menú lateral, click en **Branches**
4. Click en **Add branch protection rule**
5. Configurar:

```yaml
Branch name pattern: main

☑️ Require a pull request before merging
  ☑️ Require approvals: 1
  ☑️ Dismiss stale pull request approvals when new commits are pushed
  ☑️ Require review from Code Owners (opcional)

☑️ Require status checks to pass before merging
  ☑️ Require branches to be up to date before merging
  Status checks: (agregar cuando tengas CI/CD configurado)
    - build
    - test
    - sonarcloud

☑️ Require conversation resolution before merging

☑️ Require signed commits (opcional, recomendado)

☑️ Require linear history (opcional)

☑️ Include administrators
  (Esto aplica las reglas incluso a admins del repo)

☑️ Restrict who can push to matching branches
  (Dejar vacío para que nadie pueda push directo)

☑️ Allow force pushes: NO
☑️ Allow deletions: NO
```

6. Click **Create** o **Save changes**

---

#### 2. Proteger Branch `develop`

Repetir los pasos anteriores con estas configuraciones:

```yaml
Branch name pattern: develop

☑️ Require a pull request before merging
  ☑️ Require approvals: 1
  ☑️ Dismiss stale pull request approvals when new commits are pushed

☑️ Require status checks to pass before merging
  ☑️ Require branches to be up to date before merging
  Status checks:
    - build
    - test

☑️ Require conversation resolution before merging

☑️ Include administrators

☑️ Restrict who can push to matching branches
  (Dejar vacío)

☑️ Allow force pushes: NO
☑️ Allow deletions: NO
```

---

### Opción 2: Configuración con GitHub CLI

Si prefieres usar la línea de comandos:

```bash
# Instalar GitHub CLI (si no lo tienes)
# Windows:
winget install GitHub.cli

# Autenticarse
gh auth login

# Navegar a tu repositorio
cd c:\src\DataTouch\DataTouch

# Proteger branch main
gh api repos/:owner/:repo/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["build","test"]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true}' \
  --field restrictions=null

# Proteger branch develop
gh api repos/:owner/:repo/branches/develop/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["build","test"]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true}' \
  --field restrictions=null
```

---

## 🔄 Crear Branch `main` desde `develop`

Si actualmente solo tienes `develop`, necesitas crear `main`:

```bash
# Asegurarte de estar en develop actualizado
git checkout develop
git pull origin develop

# Crear branch main localmente
git checkout -b main

# Push main al remoto
git push -u origin main

# Configurar main como default branch en GitHub:
# Settings → Branches → Default branch → Cambiar a 'main'
```

---

## ✅ Verificar Configuración

### Prueba 1: Intentar Push Directo (Debe Fallar)

```bash
git checkout main
echo "test" >> test.txt
git add test.txt
git commit -m "test: verificar protección"
git push origin main

# Resultado esperado:
# remote: error: GH006: Protected branch update failed
```

### Prueba 2: Crear PR (Debe Funcionar)

```bash
git checkout develop
git checkout -b feature/test-protection
echo "test" >> test.txt
git add test.txt
git commit -m "feat: test branch protection"
git push origin feature/test-protection

# Crear PR en GitHub UI
# Debe permitir crear PR a develop
# Debe requerir 1 aprobación antes de merge
```

---

## 🎯 Reglas Resumidas

### ✅ Permitido

| Acción | `main` | `develop` |
|--------|--------|-----------|
| Ver código | ✅ | ✅ |
| Crear PR | ✅ | ✅ |
| Aprobar PR | ✅ | ✅ |
| Merge PR (con aprobación) | ✅ | ✅ |

### ❌ No Permitido

| Acción | `main` | `develop` |
|--------|--------|-----------|
| Push directo | ❌ | ❌ |
| Force push | ❌ | ❌ |
| Eliminar branch | ❌ | ❌ |
| Merge sin aprobación | ❌ | ❌ |
| Merge con CI fallando | ❌ | ❌ |

---

## 🔐 Configuración Adicional Recomendada

### 1. Configurar CODEOWNERS

Crear archivo `.github/CODEOWNERS`:

```bash
# Owners globales
* @AlvarengaLeo

# Owners por área
/src/DataTouch.Domain/ @AlvarengaLeo
/src/DataTouch.Infrastructure/ @AlvarengaLeo
/docs/ @AlvarengaLeo
```

### 2. Configurar PR Template

Crear archivo `.github/pull_request_template.md`:

```markdown
## Descripción
<!-- Describe los cambios realizados -->

## Tipo de cambio
- [ ] 🐛 Bug fix
- [ ] ✨ Nueva funcionalidad
- [ ] 🔨 Refactorización
- [ ] 📝 Documentación
- [ ] 🧪 Tests

## Checklist
- [ ] El código compila sin errores
- [ ] Los tests pasan
- [ ] Agregué tests para los cambios
- [ ] Actualicé la documentación
- [ ] Seguí las convenciones de código del proyecto

## Screenshots (si aplica)
<!-- Agregar screenshots de cambios visuales -->

## Issues relacionados
Closes #
```

### 3. Configurar Issue Templates

Crear archivo `.github/ISSUE_TEMPLATE/bug_report.md`:

```markdown
---
name: Bug Report
about: Reportar un bug
title: '[BUG] '
labels: bug
assignees: ''
---

## Descripción del Bug
<!-- Descripción clara del problema -->

## Pasos para Reproducir
1. 
2. 
3. 

## Comportamiento Esperado
<!-- Qué debería pasar -->

## Comportamiento Actual
<!-- Qué está pasando -->

## Screenshots
<!-- Si aplica -->

## Entorno
- OS: [e.g. Windows 11]
- .NET Version: [e.g. 9.0]
- Browser: [e.g. Chrome 120]
```

---

## 📊 Monitoreo

### Ver Reglas Actuales

```bash
# Con GitHub CLI
gh api repos/:owner/:repo/branches/main/protection

# O visitar:
# https://github.com/AlvarengaLeo/DataTouch/settings/branches
```

### Ver PRs Pendientes

```bash
gh pr list --state open
```

### Ver Status de CI

```bash
gh run list --branch develop
```

---

## 🆘 Troubleshooting

### Error: "Required status check is not enabled"

**Solución:** Primero debes tener al menos un workflow de GitHub Actions ejecutado antes de poder requerirlo en branch protection.

### Error: "You need admin access"

**Solución:** Solo los admins del repositorio pueden configurar branch protection.

### No puedo hacer merge aunque tengo aprobación

**Solución:** Verifica que:
1. Los status checks (CI) estén pasando
2. La branch esté actualizada con la base
3. Todos los comentarios estén resueltos

---

*Configuración de Branch Protection para DataTouch - Enero 2026*
