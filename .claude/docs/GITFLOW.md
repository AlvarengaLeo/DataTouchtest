  # 🌿 GitFlow Configuration Guide

  Este documento describe la configuración de GitFlow para el proyecto DataTouch, incluyendo la estructura de branches, reglas de protección y workflow de desarrollo.

  ---

  ## 📊 Estructura de Branches

  ### Branches Principales

  | Branch | Propósito | Protección | Deploy |
  |--------|-----------|------------|--------|
  | `main` | Producción estable | 🔒 Protegido | ✅ Auto-deploy a producción |
  | `develop` | Desarrollo activo | 🔒 Protegido | ✅ Auto-deploy a staging |

  ### Branches de Trabajo

  | Prefijo | Propósito | Ejemplo | Merge a |
  |---------|-----------|---------|---------|
  | `feature/` | Nuevas funcionalidades | `feature/booking-system` | `develop` |
  | `fix/` | Corrección de bugs | `fix/login-validation` | `develop` |
  | `refactor/` | Refactorización de código | `refactor/dashboard-service` | `develop` |
  | `hotfix/` | Fixes urgentes a producción | `hotfix/security-patch` | `main` y `develop` |
  | `docs/` | Cambios en documentación | `docs/update-readme` | `develop` |

  ---

  ## 🔒 Reglas de Protección de Branches

  ### Branch `main`

  ```yaml
  Protección:
    - ❌ No commits directos
    - ✅ Solo merge via Pull Request
    - ✅ Requiere 1 aprobador mínimo
    - ✅ Requiere que CI/CD pase
    - ✅ Solo acepta merge desde: develop
    - ✅ Requiere branch actualizado antes de merge
    - ✅ Incluye administradores en restricciones
  ```

  ### Branch `develop`

  ```yaml
  Protección:
    - ❌ No commits directos
    - ✅ Solo merge via Pull Request
    - ✅ Requiere 1 aprobador mínimo
    - ✅ Requiere que CI/CD pase
    - ✅ Solo acepta merge desde: feature/*, fix/*, refactor/*, docs/*
    - ✅ Requiere branch actualizado antes de merge
    - ✅ Incluye administradores en restricciones
  ```

  ---

  ## 🚀 Workflow de Desarrollo

  ### 1. Crear Feature Branch

  ```bash
  # Asegurarse de estar en develop actualizado
  git checkout develop
  git pull origin develop

  # Crear nueva feature branch
  git checkout -b feature/nombre-descriptivo

  # Ejemplos:
  git checkout -b feature/quote-request-flow
  git checkout -b feature/analytics-dashboard
  git checkout -b fix/appointment-timezone
  ```

  ### 2. Desarrollo y Commits

  ```bash
  # Hacer cambios en el código
  # ...

  # Agregar archivos
  git add .

  # Commit con mensaje descriptivo (seguir convenciones)
  git commit -m "feat: agregar sistema de cotizaciones"

  # Más commits según sea necesario
  git commit -m "test: agregar tests para QuoteService"
  git commit -m "docs: actualizar README con quotes"
  ```

  ### 3. Push y Pull Request

  ```bash
  # Push de la branch al remoto
  git push origin feature/nombre-descriptivo

  # Si es el primer push:
  git push -u origin feature/nombre-descriptivo
  ```

  **En GitHub:**
  1. Ir a la página del repositorio
  2. Click en "Compare & pull request"
  3. **Base branch**: `develop`
  4. **Compare branch**: `feature/nombre-descriptivo`
  5. Llenar título y descripción del PR
  6. Asignar reviewer (mínimo 1)
  7. Click "Create pull request"

  ### 4. Code Review

  **Reviewer debe:**
  - ✅ Revisar código línea por línea
  - ✅ Verificar que sigue estándares del proyecto
  - ✅ Verificar que los tests pasan
  - ✅ Dejar comentarios constructivos
  - ✅ Aprobar o solicitar cambios

  **Autor debe:**
  - ✅ Responder a comentarios
  - ✅ Hacer cambios solicitados
  - ✅ Push de cambios adicionales a la misma branch

  ### 5. Merge

  Una vez aprobado:
  1. **Squash and merge** (recomendado para features pequeños)
  2. **Merge commit** (para features grandes con historia importante)
  3. **Rebase and merge** (para mantener historia lineal)

  ```bash
  # Después del merge, eliminar branch local
  git checkout develop
  git pull origin develop
  git branch -d feature/nombre-descriptivo
  ```

  ---

  ## 🔥 Hotfixes (Fixes Urgentes a Producción)

  ```bash
  # Crear hotfix desde main
  git checkout main
  git pull origin main
  git checkout -b hotfix/descripcion-urgente

  # Hacer fix
  git add .
  git commit -m "hotfix: corregir vulnerabilidad de seguridad"

  # Push
  git push origin hotfix/descripcion-urgente

  # Crear 2 PRs:
  # 1. hotfix/descripcion-urgente → main
  # 2. hotfix/descripcion-urgente → develop
  ```

  ---

  ## 📝 Convenciones de Commits

  Seguir el formato **Conventional Commits**:

  ```
  <tipo>: <descripción corta>

  [cuerpo opcional]

  [footer opcional]
  ```

  ### Tipos de Commits

  | Tipo | Descripción | Ejemplo |
  |------|-------------|---------|
  | `feat` | Nueva funcionalidad | `feat: agregar sistema de reservas` |
  | `fix` | Corrección de bug | `fix: corregir validación de email` |
  | `refactor` | Refactorización | `refactor: extraer DashboardService` |
  | `docs` | Documentación | `docs: actualizar guía de instalación` |
  | `test` | Tests | `test: agregar tests para AuthService` |
  | `chore` | Mantenimiento | `chore: actualizar dependencias` |
  | `style` | Formato de código | `style: aplicar formato con prettier` |
  | `perf` | Mejora de performance | `perf: optimizar query de analytics` |

  ### Ejemplos de Buenos Commits

  ```bash
  feat: agregar módulo de cotizaciones con 8 estados

  Implementa el flujo completo de cotizaciones:
  - Estados: New, InReview, Quoted, Won, Lost
  - Timeline de actividades
  - Conversión a citas

  Closes #123
  ```

  ```bash
  fix: corregir timezone en citas públicas

  Las citas creadas desde la tarjeta pública no respetaban
  el timezone configurado en BookingSettings.

  Fixes #456
  ```

  ---

  ## 🔄 Release Flow (develop → main)

  ```bash
  # 1. Asegurarse que develop está estable
  git checkout develop
  git pull origin develop

  # 2. Crear PR de develop → main
  # En GitHub: Create Pull Request
  # Base: main
  # Compare: develop

  # 3. Título del PR: "Release v1.2.0"
  # 4. Descripción: Changelog de features/fixes incluidos
  # 5. Requiere aprobación de al menos 1 reviewer
  # 6. Merge a main

  # 7. Crear tag de versión
  git checkout main
  git pull origin main
  git tag -a v1.2.0 -m "Release version 1.2.0"
  git push origin v1.2.0
  ```

  ---

  ## 🛠️ Configurar Branch Protection en GitHub

  ### Para `main`:

  1. Ir a **Settings** → **Branches**
  2. Click **Add rule**
  3. Branch name pattern: `main`
  4. Configurar:
    - ✅ Require a pull request before merging
      - ✅ Require approvals: 1
      - ✅ Dismiss stale pull request approvals when new commits are pushed
    - ✅ Require status checks to pass before merging
      - ✅ Require branches to be up to date before merging
    - ✅ Require conversation resolution before merging
    - ✅ Include administrators
    - ✅ Restrict who can push to matching branches
      - Agregar: `develop` branch
  5. Click **Create**

  ### Para `develop`:

  1. Repetir pasos anteriores
  2. Branch name pattern: `develop`
  3. Configurar igual que `main`
  4. En "Restrict who can push":
    - Agregar patterns: `feature/*`, `fix/*`, `refactor/*`, `docs/*`

  ---

  ## 🤖 GitHub Actions (CI/CD)

  El proyecto incluye workflows automáticos:

  ### On Pull Request a `develop`:
  - ✅ Build del proyecto
  - ✅ Ejecutar tests
  - ✅ Análisis de código con SonarCloud
  - ✅ Verificar convenciones de commits

  ### On Merge a `develop`:
  - ✅ Build
  - ✅ Tests
  - ✅ Deploy automático a staging

  ### On Merge a `main`:
  - ✅ Build
  - ✅ Tests
  - ✅ Deploy automático a producción
  - ✅ Crear release en GitHub

  ---

  ## 📚 Recursos

  - [Git Flow Cheatsheet](https://danielkummer.github.io/git-flow-cheatsheet/)
  - [Conventional Commits](https://www.conventionalcommits.org/)
  - [GitHub Branch Protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)

  ---

  ## ❓ FAQ

  **¿Puedo hacer commits directos a `develop`?**
  No. Todos los cambios deben pasar por Pull Request.

  **¿Cuántos aprobadores necesito?**
  Mínimo 1 aprobador para merge a `develop` o `main`.

  **¿Qué hago si mi PR tiene conflictos?**
  ```bash
  git checkout tu-branch
  git pull origin develop
  # Resolver conflictos manualmente
  git add .
  git commit -m "chore: resolver conflictos con develop"
  git push origin tu-branch
  ```

  **¿Puedo hacer merge de mi propio PR?**
  Sí, pero solo después de tener al menos 1 aprobación.

  **¿Qué pasa si rompo algo en `develop`?**
  Crear un `fix/` branch inmediatamente y hacer PR para corregir.

  ---

  *Configuración de GitFlow para DataTouch - Enero 2026*
