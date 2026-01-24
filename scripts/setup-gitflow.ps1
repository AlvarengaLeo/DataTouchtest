# ═══════════════════════════════════════════════════════════════════════════════
# Script PowerShell para Configurar GitFlow con GitHub CLI
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "🚀 Iniciando configuración de GitFlow para DataTouch..." -ForegroundColor Cyan
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 1: Crear branch develop desde main
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "📋 Paso 1: Creando branch 'develop' desde 'main'..." -ForegroundColor Yellow

# Asegurarse de estar en main actualizado
git checkout main
git pull origin main

# Crear develop desde main
git checkout -b develop

# Push develop al remoto
git push -u origin develop

Write-Host "✅ Branch 'develop' creado y pusheado" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 2: Configurar develop como default branch
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "📋 Paso 2: Configurando 'develop' como default branch..." -ForegroundColor Yellow

# Obtener el nombre del repositorio (formato: owner/repo)
$REPO = gh repo view --json nameWithOwner -q .nameWithOwner

# Configurar develop como default branch
gh repo edit $REPO --default-branch develop

Write-Host "✅ Branch 'develop' configurado como default" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 3: Proteger branch 'main'
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "📋 Paso 3: Configurando protección para branch 'main'..." -ForegroundColor Yellow

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

Write-Host "✅ Branch 'main' protegido" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 4: Proteger branch 'develop'
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "📋 Paso 4: Configurando protección para branch 'develop'..." -ForegroundColor Yellow

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

Write-Host "✅ Branch 'develop' protegido" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 5: Verificar configuración
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "📋 Paso 5: Verificando configuración..." -ForegroundColor Yellow
Write-Host ""

Write-Host "Branches remotos:"
git branch -r
Write-Host ""

Write-Host "Default branch:"
gh repo view --json defaultBranchRef -q .defaultBranchRef.name
Write-Host ""

Write-Host "Protección de 'main':"
gh api repos/$REPO/branches/main/protection --jq '.required_pull_request_reviews.required_approving_review_count'
Write-Host ""

Write-Host "Protección de 'develop':"
gh api repos/$REPO/branches/develop/protection --jq '.required_pull_request_reviews.required_approving_review_count'
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════════
# RESUMEN
# ═══════════════════════════════════════════════════════════════════════════════

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Configuración de GitFlow completada exitosamente!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Resumen:" -ForegroundColor Yellow
Write-Host "  ✅ Branch 'develop' creado desde 'main'" -ForegroundColor Green
Write-Host "  ✅ Branch 'develop' configurado como default" -ForegroundColor Green
Write-Host "  ✅ Branch 'main' protegido (requiere 1 aprobador)" -ForegroundColor Green
Write-Host "  ✅ Branch 'develop' protegido (requiere 1 aprobador)" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Reglas aplicadas:" -ForegroundColor Yellow
Write-Host "  - ❌ No commits directos a main/develop"
Write-Host "  - ✅ Solo merge via Pull Request"
Write-Host "  - ✅ Requiere 1 aprobación mínima"
Write-Host "  - ✅ Requiere resolución de conversaciones"
Write-Host "  - ✅ Incluye administradores en restricciones"
Write-Host ""
Write-Host "🎯 Próximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Crear feature branch: git checkout -b feature/sql-server-migration"
Write-Host "  2. Hacer cambios y commits"
Write-Host "  3. Push: git push -u origin feature/sql-server-migration"
Write-Host "  4. Crear PR en GitHub hacia 'develop'"
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
