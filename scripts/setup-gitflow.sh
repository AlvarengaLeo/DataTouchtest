#!/bin/bash
# ═══════════════════════════════════════════════════════════════════════════════
# Script para Configurar GitFlow con GitHub CLI
# ═══════════════════════════════════════════════════════════════════════════════

echo "🚀 Iniciando configuración de GitFlow para DataTouch..."
echo ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 1: Crear branch develop desde main
# ═══════════════════════════════════════════════════════════════════════════════

echo "📋 Paso 1: Creando branch 'develop' desde 'main'..."

# Asegurarse de estar en main actualizado
git checkout main
git pull origin main

# Crear develop desde main
git checkout -b develop

# Push develop al remoto
git push -u origin develop

echo "✅ Branch 'develop' creado y pusheado"
echo ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 2: Configurar develop como default branch
# ═══════════════════════════════════════════════════════════════════════════════

echo "📋 Paso 2: Configurando 'develop' como default branch..."

# Obtener el nombre del repositorio (formato: owner/repo)
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)

# Configurar develop como default branch
gh repo edit $REPO --default-branch develop

echo "✅ Branch 'develop' configurado como default"
echo ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 3: Proteger branch 'main'
# ═══════════════════════════════════════════════════════════════════════════════

echo "📋 Paso 3: Configurando protección para branch 'main'..."

gh api repos/$REPO/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":[]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":false}' \
  --field restrictions=null \
  --field required_linear_history=false \
  --field allow_force_pushes=false \
  --field allow_deletions=false \
  --field required_conversation_resolution=true

echo "✅ Branch 'main' protegido"
echo ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 4: Proteger branch 'develop'
# ═══════════════════════════════════════════════════════════════════════════════

echo "📋 Paso 4: Configurando protección para branch 'develop'..."

gh api repos/$REPO/branches/develop/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":[]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":false}' \
  --field restrictions=null \
  --field required_linear_history=false \
  --field allow_force_pushes=false \
  --field allow_deletions=false \
  --field required_conversation_resolution=true

echo "✅ Branch 'develop' protegido"
echo ""

# ═══════════════════════════════════════════════════════════════════════════════
# PASO 5: Verificar configuración
# ═══════════════════════════════════════════════════════════════════════════════

echo "📋 Paso 5: Verificando configuración..."
echo ""

echo "Branches remotos:"
git branch -r
echo ""

echo "Default branch:"
gh repo view --json defaultBranchRef -q .defaultBranchRef.name
echo ""

echo "Protección de 'main':"
gh api repos/$REPO/branches/main/protection --jq '.required_pull_request_reviews.required_approving_review_count'
echo ""

echo "Protección de 'develop':"
gh api repos/$REPO/branches/develop/protection --jq '.required_pull_request_reviews.required_approving_review_count'
echo ""

# ═══════════════════════════════════════════════════════════════════════════════
# RESUMEN
# ═══════════════════════════════════════════════════════════════════════════════

echo "═══════════════════════════════════════════════════════════════"
echo "✅ Configuración de GitFlow completada exitosamente!"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "📊 Resumen:"
echo "  ✅ Branch 'develop' creado desde 'main'"
echo "  ✅ Branch 'develop' configurado como default"
echo "  ✅ Branch 'main' protegido (requiere 1 aprobador)"
echo "  ✅ Branch 'develop' protegido (requiere 1 aprobador)"
echo ""
echo "📝 Reglas aplicadas:"
echo "  - ❌ No commits directos a main/develop"
echo "  - ✅ Solo merge via Pull Request"
echo "  - ✅ Requiere 1 aprobación mínima"
echo "  - ✅ Requiere resolución de conversaciones"
echo "  - ✅ Incluye administradores en restricciones"
echo ""
echo "🎯 Próximos pasos:"
echo "  1. Crear feature branch: git checkout -b feature/sql-server-migration"
echo "  2. Hacer cambios y commits"
echo "  3. Push: git push -u origin feature/sql-server-migration"
echo "  4. Crear PR en GitHub hacia 'develop'"
echo ""
echo "═══════════════════════════════════════════════════════════════"
