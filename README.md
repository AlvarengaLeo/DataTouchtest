# 🎴 DataTouch CRM

**Plataforma SaaS de Tarjetas Digitales Profesionales con CRM Integrado**

DataTouch convierte tarjetas NFC/QR en puntos de entrada digital para captura de leads, gestión de citas y cotizaciones. Diseñado para profesionales y empresas que buscan modernizar su networking y automatizar su proceso de ventas.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-8.15.0-594AE2)](https://mudblazor.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

---

## 📋 Tabla de Contenidos

- [Características](#-características)
- [Stack Tecnológico](#-stack-tecnológico)
- [Requisitos del Sistema](#-requisitos-del-sistema)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Ejecución](#-ejecución)
- [Arquitectura](#-arquitectura)
- [Base de Datos](#-base-de-datos)
- [GitFlow](#-gitflow)
- [Deployment](#-deployment)

---

## ✨ Características

### 🎴 Tarjetas Digitales
- **Editor Visual** con live preview en tiempo real
- **Plantillas** por industria (Tecnología, Negocios, Creativos)
- **Personalización completa** de colores, fuentes y estilos
- **Componentes modulares** (galería, video, enlaces personalizados)
- **QR Code dinámico** con branding personalizado
- **Compatible con NFC** para tap-to-share

### 📅 Sistema de Reservas (Booking)
- **Calendario inteligente** con disponibilidad configurable
- **Reservas públicas** desde la tarjeta digital
- **Gestión de servicios** con duración y precios
- **Zonas horarias** automáticas
- **Estados de citas**: Pending, Confirmed, Completed, Cancelled, NoShow

### 💼 Cotizaciones (Quotes)
- **Solicitudes de cotización** desde tarjeta pública
- **8 estados enterprise**: New → InReview → Quoted → Won/Lost
- **Timeline de actividades** con auditoría completa
- **Conversión automática** de cotizaciones a citas

### 📊 Analytics & CRM
- **Dashboard en tiempo real** con KPIs
- **Geolocalización** de visitantes
- **Tracking de eventos**: page views, QR scans, CTA clicks
- **Gestión de leads** capturados desde formularios

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología | Versión |
|------|------------|---------|
| **Framework** | .NET | 9.0 |
| **UI** | Blazor Server | 9.0 |
| **Componentes** | MudBlazor | 8.15.0 |
| **ORM** | Entity Framework Core | 9.0.0 |
| **Base de Datos** | SQL Server | 2019+ |
| **Autenticación** | Cookie Authentication | ASP.NET Core |

---

## 💻 Requisitos del Sistema

| Software | Versión Mínima | Descarga |
|----------|---------------|----------|
| **.NET SDK** | 9.0+ | [Descargar](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **SQL Server** | 2019+ / Express / Developer | [Descargar](https://www.microsoft.com/sql-server/sql-server-downloads) |
| **Git** | 2.40+ | [Descargar](https://git-scm.com/downloads) |

---

## 🚀 Instalación

### 1. Clonar el Repositorio

```bash
git clone https://github.com/AlvarengaLeo/DataTouch.git
cd DataTouch
```

### 2. Verificar .NET

```bash
dotnet --version
# Debe mostrar: 9.0.x o superior
```

### 3. Configurar SQL Server

**Crear Base de Datos:**

```bash
# Opción 1: Con SSMS (GUI)
# 1. Abrir SQL Server Management Studio
# 2. Ejecutar: sql/migrations/001_InitialCreate_SQLServer.sql
# 3. Ejecutar: sql/migrations/002_SeedData_SQLServer.sql

# Opción 2: Con sqlcmd (CLI)
cd sql/migrations
sqlcmd -S localhost -d master -i 001_InitialCreate_SQLServer.sql
sqlcmd -S localhost -d DataTouch -i 002_SeedData_SQLServer.sql
```

### 4. Restaurar Dependencias

```bash
dotnet restore
dotnet build
```

---

## ⚙️ Configuración

### Connection String

Editar `src/DataTouch.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DataTouch;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Variantes:**

```bash
# Windows Authentication
Server=localhost;Database=DataTouch;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true

# SQL Server Authentication
Server=localhost;Database=DataTouch;User Id=sa;Password=TuPassword;TrustServerCertificate=True;MultipleActiveResultSets=true

# Named Instance (SQL Express)
Server=localhost\SQLEXPRESS;Database=DataTouch;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

---

## 🏃 Ejecución

```bash
cd src/DataTouch.Web
dotnet run
# O con hot reload:
dotnet watch run
```

**URLs:**
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

**Credenciales:**
- Email: `admin@techcorp.com`
- Password: `admin123`

**Páginas:**
- Dashboard: `/`
- Mi Tarjeta: `/cards/mine`
- Citas: `/appointments`
- Cotizaciones: `/quotes`
- Tarjeta Pública: `/p/techcorp/leonel-alvarenga`

---

## 🏗️ Arquitectura

```
DataTouch/
├── src/
│   ├── DataTouch.Domain/              # Entidades (16 clases)
│   ├── DataTouch.Infrastructure/      # DbContext, EF Core
│   ├── DataTouch.Api/                 # API REST (futuro)
│   └── DataTouch.Web/                 # Blazor Server
│       ├── Components/Pages/          # 14 páginas
│       ├── Components/Shared/         # Componentes
│       └── Services/                  # 13 servicios
├── sql/migrations/                    # Scripts SQL
└── docs/                              # Documentación
```

---

## 🗄️ Base de Datos

**16 Entidades:**
- Core: `Organization`, `User`, `Card`, `Lead`
- Templates: `CardTemplate`, `CardStyle`, `CardComponent`
- Booking: `Service`, `Appointment`, `AvailabilityRule`, `QuoteRequest`
- Analytics: `CardAnalytics`, `Activity`

**Scripts:**
- `sql/migrations/001_InitialCreate_SQLServer.sql` - Creación de tablas
- `sql/migrations/002_SeedData_SQLServer.sql` - Datos de demostración

Ver [`docs/DATABASE.md`](docs/DATABASE.md) para más detalles.

---

## 🌿 GitFlow

### Branches

- `main` - Producción (protegido)
- `develop` - Desarrollo (protegido)
- `feature/*` - Nuevas funcionalidades
- `fix/*` - Correcciones de bugs
- `refactor/*` - Refactorización
- `hotfix/*` - Fixes urgentes a producción
- `docs/*` - Documentación

### Workflow

```bash
# Crear feature branch desde develop
git checkout develop
git pull origin develop
git checkout -b feature/nueva-funcionalidad

# Commits
git add .
git commit -m "feat: agregar nueva funcionalidad"

# Push y crear PR
git push origin feature/nueva-funcionalidad
# Crear Pull Request a develop (requiere 1 aprobador)
```

### Reglas de Protección

**Branch `develop`:**
- ❌ No commits directos
- ✅ Solo via Pull Request
- ✅ Requiere 1 aprobador
- ✅ Solo acepta: `feature/*`, `fix/*`, `refactor/*`, `docs/*`

**Branch `main`:**
- ❌ No commits directos
- ✅ Solo merge desde `develop` via PR
- ✅ Requiere 1 aprobador

### Convenciones de Commits

```bash
feat: Nueva funcionalidad
fix: Corrección de bug
refactor: Refactorización
docs: Documentación
test: Tests
chore: Mantenimiento
```

---

## 🚀 Deployment

### Plataformas Recomendadas

| Plataforma | Costo | CLI | Recomendado Para |
|------------|-------|-----|------------------|
| **Railway.app** | $5-20/mes | ✅ | Startups |
| **Render.com** | $7-25/mes | ✅ | Startups |
| **Azure App Service** | $55+/mes | ✅ | Enterprise |

### Railway.app (Recomendado)

```bash
npm i -g @railway/cli
railway login
railway init
railway up
```

### Dominio Personalizado

Todas las plataformas soportan dominios custom con SSL gratis (Let's Encrypt).

---

## 📄 Licencia

Proyecto privado - TechCorp Solutions

---

## 📞 Soporte

- **Issues**: [GitHub Issues](https://github.com/AlvarengaLeo/DataTouch/issues)
- **Docs**: Ver carpeta `/docs`

---

**Hecho con ❤️ por TechCorp Solutions**
