# 🚂 Railway Config as Code - Guía de Uso

## 📋 Archivo: `railway.toml`

Este archivo define la configuración completa de Railway usando **Config as Code**.

---

## 🏗️ Servicios Configurados

### 1. **Production** (`datatouch-production`)
- **Branch:** `main`
- **Environment:** Production
- **Auto-Deploy:** ✅ Enabled

### 2. **Staging** (`datatouch-staging`)
- **Branch:** `develop`  
- **Environment:** Staging
- **Auto-Deploy:** ✅ Enabled

---

## ⚙️ Configuración Común

```toml
[build]
builder = "DOCKERFILE"
dockerfilePath = "Dockerfile"

[deploy]
startCommand = "dotnet DataTouch.Web.dll"
healthcheckPath = "/health"
healthcheckTimeout = 30
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 3
```

---

## 🔐 Variables de Entorno

### En Railway Dashboard

Debes configurar estas variables **manualmente** en Railway (no se pueden poner en `railway.toml` por seguridad):

#### Production Service
```bash
ConnectionStrings__DefaultConnection=Server=162.248.54.184;Database=DataTouch;User Id=sia;Password=***;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

#### Staging Service (opcional - misma DB o diferente)
```bash
ConnectionStrings__DefaultConnection=Server=162.248.54.184;Database=DataTouch_Staging;User Id=sia;Password=***;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

---

## 🚀 Cómo Funciona

### Flujo de Deployment

1. **Push a `main`:**
   ```bash
   git push origin main
   ```
   → Railway despliega automáticamente a `datatouch-production`

2. **Push a `develop`:**
   ```bash
   git push origin develop
   ```
   → Railway despliega automáticamente a `datatouch-staging`

3. **Pull Requests:**
   - Railway puede crear **Preview Deployments** automáticamente
   - Configurable en Railway Dashboard → Settings → Deployments

---

## 📊 Ventajas de Config as Code

| Aspecto | Sin TOML | Con TOML |
|---------|----------|----------|
| **Versionado** | ❌ Manual en UI | ✅ En Git |
| **Replicable** | ❌ Difícil | ✅ Fácil |
| **Multi-ambiente** | ⚠️ Complejo | ✅ Simple |
| **Auditable** | ❌ No | ✅ Sí |
| **Rollback** | ❌ Manual | ✅ Git revert |

---

## 🔧 Modificar Configuración

### Agregar Nuevo Servicio

```toml
[[services]]
name = "datatouch-preview"
source = "feature/*"  # Todas las feature branches

[services.env]
ASPNETCORE_ENVIRONMENT = "Development"
```

### Cambiar Health Check

```toml
[deploy]
healthcheckPath = "/api/health"
healthcheckTimeout = 60
```

### Agregar Variables de Entorno Públicas

```toml
[services.env]
ASPNETCORE_ENVIRONMENT = "Production"
ASPNETCORE_URLS = "http://0.0.0.0:$PORT"
LOG_LEVEL = "Information"
```

**⚠️ NUNCA pongas secretos aquí** (connection strings, API keys, etc.)

---

## 📝 Próximos Pasos

1. **Commit el `railway.toml`:**
   ```bash
   git add railway.toml
   git commit -m "feat(railway): Add config as code with production and staging services"
   git push
   ```

2. **Verificar en Railway Dashboard:**
   - Railway detectará automáticamente el `railway.toml`
   - Creará los servicios definidos
   - Aplicará la configuración

3. **Configurar Variables de Entorno:**
   - Ve a cada servicio en Railway
   - Agrega `ConnectionStrings__DefaultConnection`

---

## 🔗 Referencias

- [Railway Config as Code Docs](https://docs.railway.app/reference/config-as-code)
- [Railway TOML Reference](https://docs.railway.app/reference/config-as-code#railwaytoml-reference)
- [Environment Variables](https://docs.railway.app/guides/variables)

---

**✅ Con este archivo, tu configuración de Railway está completamente versionada y replicable.**
