# 🚀 DataTouch - Guía de Instalación y Configuración

## Tabla de Contenidos

1. [Descripción General](#descripción-general)
2. [Requisitos del Sistema](#requisitos-del-sistema)
3. [Instalación Paso a Paso](#instalación-paso-a-paso)
4. [Configuración de Base de Datos](#configuración-de-base-de-datos)
5. [Variables de Configuración](#variables-de-configuración)
6. [Ejecución del Proyecto](#ejecución-del-proyecto)
7. [Estructura del Proyecto](#estructura-del-proyecto)
8. [Troubleshooting](#troubleshooting)

---

## Descripción General

**DataTouch** es una plataforma SaaS que convierte tarjetas NFC/QR en un punto de entrada digital para captura de leads. Permite a profesionales y empresas compartir información de contacto de forma moderna y hacer seguimiento de interacciones.

### Funcionalidades Principales

| Módulo | Descripción |
|--------|-------------|
| **Dashboard** | Panel de control con KPIs, gráficos de interacciones y analytics |
| **Mi Tarjeta** | Editor de tarjeta digital personal con plantillas |
| **Leads** | Gestión de leads capturados desde tarjetas públicas |
| **Biblioteca de Plantillas** | Catálogo de plantillas por industria |
| **Tarjeta Pública** | Landing page pública por tarjeta `/p/{org}/{card}` |

---

## Requisitos del Sistema

### 🖥️ Software Requerido

| Software | Versión Mínima | Descarga |
|----------|---------------|----------|
| **.NET SDK** | 9.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **Docker Desktop** | 4.x+ | [Download](https://www.docker.com/products/docker-desktop/) |
| **Git** | 2.40+ | [Download](https://git-scm.com/downloads) |
| **Visual Studio 2022** o **VS Code** | Opcional | [VS](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) |

### 🛠️ Extensiones Recomendadas (VS Code)

```
C# for Visual Studio Code (ms-dotnettools.csharp)
C# Dev Kit (ms-dotnettools.csdevkit)
Docker (ms-azuretools.vscode-docker)
```

---

## Instalación Paso a Paso

### Paso 1: Clonar el Repositorio

```powershell
# Clonar el repositorio
git clone https://github.com/AlvarengaLeo/DataTouch.git

# Navegar al directorio
cd DataTouch
```

### Paso 2: Verificar Instalación de .NET

```powershell
# Verificar versión de .NET
dotnet --version
# Debe mostrar: 9.0.x o superior

# Listar SDKs instalados
dotnet --list-sdks
```

Si no tienes .NET 9:
```powershell
# Windows (con winget)
winget install Microsoft.DotNet.SDK.9

# O descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
```

### Paso 3: Restaurar Dependencias

```powershell
# Desde la raíz del proyecto
dotnet restore

# Verificar que no haya errores
dotnet build
```

---

## Configuración de Base de Datos

DataTouch soporta **dos modos de base de datos**:

### 🟢 Opción A: InMemory (Development - Por Defecto)

**No requiere configuración**. La aplicación usa una base de datos en memoria con datos de demostración precargados.

> ⚠️ **IMPORTANTE**: Los datos se pierden al reiniciar la aplicación.

### 🔵 Opción B: MySQL (Production)

#### 1. Iniciar MySQL con Docker

```powershell
# Crear y ejecutar contenedor MySQL
docker run --name datatouch-mysql `
  -e MYSQL_ROOT_PASSWORD=datatouch123 `
  -e MYSQL_DATABASE=datatouch `
  -p 3306:3306 `
  -d mysql:8
```

#### 2. Verificar que MySQL está corriendo

```powershell
docker ps

# Debe mostrar algo como:
# CONTAINER ID   IMAGE     STATUS          PORTS
# abc123...      mysql:8   Up 2 minutes    0.0.0.0:3306->3306/tcp
```

#### 3. Modificar el Connection String

Editar el archivo `src/DataTouch.Web/Program.cs` (líneas 22-24):

```csharp
// ANTES (InMemory - Development)
builder.Services.AddDbContext<DataTouchDbContext>(options =>
    options.UseInMemoryDatabase("DataTouchDb"));

// DESPUÉS (MySQL - Production)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataTouchDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

#### 4. Connection String en appsettings.json

El archivo `src/DataTouch.Web/appsettings.json` ya contiene la configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=datatouch;User=root;Password=datatouch123;"
  }
}
```

**Parámetros del Connection String:**

| Parámetro | Valor por Defecto | Descripción |
|-----------|------------------|-------------|
| `Server` | `localhost` | Host del servidor MySQL |
| `Port` | `3306` | Puerto de MySQL |
| `Database` | `datatouch` | Nombre de la base de datos |
| `User` | `root` | Usuario de MySQL |
| `Password` | `datatouch123` | Contraseña del usuario |

> 🔒 **NOTA DE SEGURIDAD**: En producción, usar User Secrets o variables de entorno para las credenciales.

---

## Variables de Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=datatouch;User=root;Password=datatouch123;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### Configuración Avanzada (Opcional)

Para producción, se recomienda usar **User Secrets**:

```powershell
# Inicializar User Secrets
cd src/DataTouch.Web
dotnet user-secrets init

# Guardar connection string de forma segura
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=prod-server;Database=datatouch;User=prod_user;Password=SECURE_PASSWORD;"
```

---

## Ejecución del Proyecto

### Modo Development (Recomendado para Primera Ejecución)

```powershell
# Navegar al proyecto Web
cd src/DataTouch.Web

# Ejecutar en modo desarrollo
dotnet run

# O con hot reload
dotnet watch run
```

### URLs de Acceso

| Tipo | URL | Descripción |
|------|-----|-------------|
| **HTTPS** | `https://localhost:5001` | URL segura (recomendada) |
| **HTTP** | `http://localhost:5000` | URL no segura |

### Credenciales de Demostración

| Email | Contraseña | Rol |
|-------|-----------|-----|
| `admin@demo.com` | `admin123` | OrgAdmin |

### Páginas Principales

| Página | Ruta | Descripción |
|--------|------|-------------|
| Login | `/login` | Página de inicio de sesión |
| Dashboard | `/` | Panel de control (requiere auth) |
| Mi Tarjeta | `/my-card` | Editor de tarjeta personal |
| Leads | `/leads` | Lista de leads capturados |
| Plantillas | `/templates` | Biblioteca de plantillas |
| Tarjeta Pública | `/p/demo-company/admin-demo` | Vista pública de la tarjeta demo |

---

## Estructura del Proyecto

```
DataTouch/
├── 📁 src/
│   ├── 📁 DataTouch.Domain/          # 🎯 Capa de Dominio
│   │   └── 📁 Entities/              # Entidades del negocio
│   │       ├── Card.cs               # Tarjeta digital
│   │       ├── CardAnalytics.cs      # Eventos de analytics
│   │       ├── CardComponent.cs      # Componentes modulares
│   │       ├── CardStyle.cs          # Estilos personalizados
│   │       ├── CardTemplate.cs       # Plantillas de diseño
│   │       ├── Lead.cs               # Lead capturado
│   │       ├── LeadNote.cs           # Notas sobre leads
│   │       ├── Organization.cs       # Organización/empresa
│   │       └── User.cs               # Usuario del sistema
│   │
│   ├── 📁 DataTouch.Infrastructure/  # 🔧 Capa de Infraestructura
│   │   └── 📁 Data/
│   │       └── DataTouchDbContext.cs # DbContext de EF Core
│   │
│   ├── 📁 DataTouch.Api/             # 🌐 API REST (Minimal APIs)
│   │   └── Program.cs                # Configuración de la API
│   │
│   └── 📁 DataTouch.Web/             # 🖥️ Aplicación Blazor Server
│       ├── 📁 Components/
│       │   ├── 📁 Layout/            # Layout principal y navegación
│       │   ├── 📁 Pages/             # Páginas Razor
│       │   │   ├── Dashboard.razor   # Dashboard con KPIs
│       │   │   ├── MyCard.razor      # Editor de tarjeta
│       │   │   ├── Leads.razor       # Lista de leads
│       │   │   ├── LeadDetail.razor  # Detalle de lead
│       │   │   ├── Login.razor       # Página de login
│       │   │   ├── PublicCard.razor  # Tarjeta pública
│       │   │   └── TemplateLibrary.razor
│       │   ├── 📁 Shared/            # Componentes compartidos
│       │   └── 📁 Templates/         # Plantillas de tarjetas
│       ├── 📁 Services/              # Servicios de la aplicación
│       │   ├── AuthService.cs        # Autenticación
│       │   ├── DashboardService.cs   # Datos del dashboard
│       │   ├── CardAnalyticsService.cs
│       │   ├── DbInitializer.cs      # Seed de datos demo
│       │   └── GeoLocationService.cs
│       ├── 📁 wwwroot/               # Archivos estáticos
│       │   ├── app.css               # Estilos globales
│       │   └── design-tokens.css     # Tokens de diseño
│       ├── Program.cs                # Entry point
│       └── appsettings.json          # Configuración
│
└── 📁 tests/
    └── 📁 DataTouch.Tests/           # 🧪 Tests unitarios
```

---

## Troubleshooting

### ❌ Error: "The term 'dotnet' is not recognized"

**Causa**: .NET SDK no está instalado o no está en el PATH.

**Solución**:
```powershell
# Verificar instalación
winget install Microsoft.DotNet.SDK.9

# Reiniciar PowerShell después de instalar
```

### ❌ Error: "Port 5001 is already in use"

**Solución**:
```powershell
# Encontrar el proceso usando el puerto
netstat -ano | findstr :5001

# Terminar el proceso (reemplazar PID con el número encontrado)
taskkill /PID <PID> /F

# O cambiar el puerto en Properties/launchSettings.json
```

### ❌ Error: "Connection refused" (MySQL)

**Causa**: MySQL no está corriendo o el puerto está bloqueado.

**Solución**:
```powershell
# Verificar que Docker está corriendo
docker ps

# Si el contenedor no está, reiniciarlo
docker start datatouch-mysql

# Ver logs del contenedor
docker logs datatouch-mysql
```

### ❌ Error: "SSL certificate problem"

**Solución para desarrollo**:
```powershell
# Confiar en el certificado de desarrollo de .NET
dotnet dev-certs https --trust
```

---

## 📞 Soporte

Para problemas o preguntas:
- Crear un **Issue** en el repositorio de GitHub
- Revisar la documentación en `/docs`

---

*Documentación generada para DataTouch MVP 0.1*
