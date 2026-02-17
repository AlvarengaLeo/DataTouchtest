# 🚂 Guía de Despliegue en Railway

## Requisitos Previos

1. Cuenta en [Railway](https://railway.app)
2. Repositorio en GitHub conectado
3. Railway CLI (opcional pero recomendado)

---

## Opción 1: Despliegue desde GitHub (Recomendado)

### Paso 1: Crear Proyecto en Railway
1. Ve a [railway.app](https://railway.app) y haz login
2. Click en **"New Project"**
3. Selecciona **"Deploy from GitHub repo"**
4. Autoriza Railway para acceder a tu repositorio
5. Selecciona `DataTouch-Team/DataTouch`

### Paso 2: Configurar Variables de Entorno
En Railway Dashboard → Tu Proyecto → Variables:

```
DATABASE_URL=Server=162.248.54.184;Database=DataTouch;User Id=sia;Password=fuGvDyHxN9k8JyR;TrustServerCertificate=True;MultipleActiveResultSets=True;
ASPNETCORE_ENVIRONMENT=Production
```

### Paso 3: Configurar Dominio
1. Ve a Settings → Domains
2. Railway te dará un dominio como `datatouch-production.up.railway.app`
3. O configura tu dominio personalizado

---

## Opción 2: Railway CLI

### Instalación
```bash
# Con npm
npm install -g @railway/cli

# Con Homebrew (macOS)
brew install railway
```

### Comandos Básicos
```bash
# Login
railway login

# Vincular proyecto existente
railway link

# Ver logs
railway logs

# Deploy manual
railway up

# Abrir dashboard
railway open

# Variables de entorno
railway variables
railway variables set DATABASE_URL="tu-connection-string"
```

---

## Opción 3: GitHub Actions (CI/CD Automático)

El archivo `.github/workflows/railway-deploy.yml` ya está configurado.

### Configurar Secret en GitHub
1. Ve a tu repo en GitHub → Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Nombre: `RAILWAY_TOKEN`
4. Valor: Tu token de Railway (obtenerlo desde Railway Dashboard → Account Settings → Tokens)

### Flujo de CI/CD
- **Push a `develop`** → Deploy a Staging
- **Push a `main`** → Deploy a Production
- **Pull Request** → Solo Build & Test (sin deploy)

---

## Variables de Entorno Requeridas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DATABASE_URL` | Connection string SQL Server | `Server=...;Database=DataTouch;...` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ejecución | `Production` |
| `PORT` | Puerto (Railway lo asigna automáticamente) | `8080` |

---

## Estructura de Archivos para Railway

```
DataTouch/
├── Dockerfile              # Configuración de contenedor
├── railway.json            # Configuración de Railway
├── .dockerignore           # Archivos a excluir del build
├── .github/
│   └── workflows/
│       └── railway-deploy.yml  # CI/CD Pipeline
└── src/
    └── DataTouch.Web/
        ├── appsettings.json
        └── appsettings.Production.json
```

---

## Troubleshooting

### Error: "Port already in use"
Railway asigna el puerto dinámicamente via `$PORT`. El Dockerfile ya está configurado para usar esta variable.

### Error: "Database connection failed"
1. Verifica que `DATABASE_URL` esté configurado correctamente
2. Asegúrate que el servidor SQL permite conexiones externas
3. Verifica que `TrustServerCertificate=True` esté en el connection string

### Error: "Health check failed"
El endpoint `/health` debe responder con 200 OK. Verifica que la app inicie correctamente.

### Ver Logs
```bash
railway logs --tail
```

---

## Comandos Útiles

```bash
# Ver estado del servicio
railway status

# Reiniciar servicio
railway service restart

# Ver métricas
railway metrics

# Conectar a shell del contenedor
railway shell
```

---

## Costos Estimados

Railway tiene un plan gratuito con:
- $5 USD de crédito mensual
- Suficiente para apps pequeñas/medianas

Para producción, considera el plan Pro ($20/mes) que incluye:
- Sin límites de ejecución
- Dominios personalizados
- Mejor soporte

---

## Próximos Pasos

1. [ ] Crear cuenta en Railway
2. [ ] Conectar repositorio de GitHub
3. [ ] Configurar variables de entorno
4. [ ] Hacer deploy inicial
5. [ ] Configurar dominio personalizado (opcional)
6. [ ] Configurar GitHub Actions para CI/CD
