# 🚀 DataTouch MVP 0.1

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 9">
  <img src="https://img.shields.io/badge/Blazor-Server-512BD4?style=for-the-badge&logo=blazor" alt="Blazor Server">
  <img src="https://img.shields.io/badge/MudBlazor-8.15.0-594AE2?style=for-the-badge" alt="MudBlazor">
  <img src="https://img.shields.io/badge/MySQL-8.x-4479A1?style=for-the-badge&logo=mysql&logoColor=white" alt="MySQL">
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="MIT License">
</p>

Una plataforma SaaS que convierte tarjetas NFC/QR en un punto de entrada digital para captura de leads.

---

## 📚 Documentación

| Documento | Descripción |
|-----------|-------------|
| [📖 SETUP.md](./docs/SETUP.md) | Guía completa de instalación y configuración |
| [🗄️ DATABASE.md](./docs/DATABASE.md) | Esquema de base de datos y scripts SQL |
| [📋 HANDOFF.md](./docs/HANDOFF.md) | Documento de handoff del proyecto |
| [🧠 CLAUDE.md](./CLAUDE.md) | Documentación técnica exhaustiva para IA |

---

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, para MySQL)
- [Git](https://git-scm.com/downloads)

### Instalación

```bash
# 1. Clonar el repositorio
git clone https://github.com/AlvarengaLeo/DataTouch.git
cd DataTouch

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la aplicación (usa base de datos en memoria)
cd src/DataTouch.Web
dotnet run
```

### 🌐 URLs de Acceso

| Página | URL | Credenciales |
|--------|-----|--------------|
| **CRM Panel** | https://localhost:5001/login | `admin@demo.com` / `admin123` |
| **Tarjeta Pública** | https://localhost:5001/p/demo-company/admin-demo | Acceso público |

---

## 📁 Estructura del Proyecto

```
DataTouch/
├── 📁 src/
│   ├── DataTouch.Domain/        # 🎯 Entidades de dominio
│   ├── DataTouch.Infrastructure/  # 🔧 EF Core, DbContext
│   ├── DataTouch.Api/           # 🌐 API endpoints (Minimal API)
│   └── DataTouch.Web/           # 🖥️ Blazor Server UI
└── 📁 tests/
    └── DataTouch.Tests/         # 🧪 Unit tests
```

---

## 🎯 Funcionalidades MVP 0.1

### Dashboard
- ✅ Panel de control con KPIs en tiempo real
- ✅ Gráficos de interacciones (vistas, clics, leads)
- ✅ Analytics geográficos y por dispositivo
- ✅ Top enlaces más clickeados

### CRM del Cliente (ARISTA 2)
- ✅ Login con autenticación por cookies
- ✅ Gestión de Leads (lista, detalle, edición de estado)
- ✅ Edición de tarjeta personal
- ✅ Biblioteca de plantillas por industria

### Landing Pública (ARISTA 3)
- ✅ Vista de tarjeta pública `/p/{orgSlug}/{cardSlug}`
- ✅ Botones de contacto (Llamar, WhatsApp, Email, Guardar Contacto)
- ✅ Formulario de contacto que crea Leads
- ✅ Tracking de analytics automático

---

## 🔧 Stack Tecnológico

| Capa | Tecnología |
|------|------------|
| **Backend** | .NET 9, Minimal APIs |
| **Frontend** | Blazor Server |
| **UI Library** | MudBlazor 8.15.0 |
| **ORM** | Entity Framework Core 9.0 |
| **Base de datos** | MySQL 8 (producción) / InMemory (desarrollo) |
| **Autenticación** | Cookie Authentication |

---

## 🐳 Ejecutar con MySQL (Producción)

```bash
# 1. Iniciar MySQL con Docker
docker run --name datatouch-mysql \
  -e MYSQL_ROOT_PASSWORD=datatouch123 \
  -e MYSQL_DATABASE=datatouch \
  -p 3306:3306 \
  -d mysql:8

# 2. Modificar Program.cs para usar MySQL (ver SETUP.md)

# 3. Ejecutar la aplicación
cd src/DataTouch.Web
dotnet run
```

---

## 🧪 Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 Modelo de Datos

El sistema maneja las siguientes entidades principales:

| Entidad | Descripción |
|---------|-------------|
| `Organization` | Empresas/clientes (multi-tenant) |
| `User` | Usuarios del CRM |
| `Card` | Tarjetas digitales NFC/QR |
| `Lead` | Leads capturados |
| `CardAnalytics` | Eventos de interacción |
| `CardTemplate` | Plantillas de diseño |
| `CardStyle` | Estilos personalizados |

Para más detalles, ver [DATABASE.md](./docs/DATABASE.md).

---

## 🤝 Contribuir

1. Fork el repositorio
2. Crear una rama feature (`git checkout -b feature/amazing-feature`)
3. Commit los cambios (`git commit -m 'Add amazing feature'`)
4. Push a la rama (`git push origin feature/amazing-feature`)
5. Abrir un Pull Request

---

## 📝 Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo `LICENSE` para más detalles.

---

## 👨‍💻 Autor

**Leonardo Alvarenga**
- GitHub: [@AlvarengaLeo](https://github.com/AlvarengaLeo)

---

*DataTouch MVP 0.1 - Transformando conexiones en oportunidades* 🚀
