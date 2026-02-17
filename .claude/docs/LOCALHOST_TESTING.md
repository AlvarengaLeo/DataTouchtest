# 🚀 Guía Completa: Ejecutar DataTouch en Localhost

## ✅ Estado Actual de Configuración

### Backend YA Configurado ✅
- ✅ `appsettings.json` - Connection string apunta a SQL Server DataTouch
- ✅ `Program.cs` - Usa `UseSqlServer()` en lugar de InMemory
- ⚠️ **FALTA:** Paquete NuGet `Microsoft.EntityFrameworkCore.SqlServer`

### Base de Datos ✅
- ✅ Base de datos `DataTouch` creada
- ✅ 16 tablas creadas
- ✅ Datos de prueba insertados

---

## 📦 Paso 1: Instalar Paquete SQL Server

```powershell
cd c:\src\DataTouch\DataTouch\src\DataTouch.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
```

**Resultado esperado:**
```
info : PackageReference for 'Microsoft.EntityFrameworkCore.SqlServer' version '9.0.0' added to file 'DataTouch.Infrastructure.csproj'.
```

---

## 🔧 Paso 2: Verificar Configuración

### 2.1 Verificar `appsettings.json`

**Archivo:** `src/DataTouch.Web/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=162.248.54.184;Database=DataTouch;User Id=sia;Password=fuGvDyHxN9k8JyR;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  }
}
```

✅ **Ya está configurado correctamente**

### 2.2 Verificar `Program.cs`

**Archivo:** `src/DataTouch.Web/Program.cs` (líneas 22-26)

```csharp
// Add DbContext - Use SQL Server for production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataTouchDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
```

✅ **Ya está configurado correctamente**

---

## 🚀 Paso 3: Ejecutar la Aplicación

```powershell
# Navegar al proyecto Web
cd c:\src\DataTouch\DataTouch\src\DataTouch.Web

# Restaurar dependencias (por si acaso)
dotnet restore

# Ejecutar la aplicación
dotnet run
```

**Resultado esperado:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 🧪 Paso 4: Probar la Aplicación

### 4.1 Acceder a la Aplicación

**URLs disponibles:**
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

### 4.2 Login con Credenciales Demo

**Credenciales:**
- **Email:** `admin@techcorp.com`
- **Password:** `admin123`

### 4.3 Verificar Funcionalidades

#### ✅ Dashboard
- URL: https://localhost:5001/dashboard
- Debe mostrar estadísticas de la organización

#### ✅ Tarjeta Pública
- URL: https://localhost:5001/p/techcorp/leonel-alvarenga
- Debe mostrar la tarjeta de Leonel Alvarenga
- Debe tener 3 servicios:
  1. Consultoría Estratégica (60 min, $150)
  2. Desarrollo de Software (Quote, desde $5000)
  3. Auditoría de Código (120 min, $300)

#### ✅ Gestión de Tarjetas
- URL: https://localhost:5001/cards
- Debe mostrar la tarjeta "leonel-alvarenga"

#### ✅ Analytics
- URL: https://localhost:5001/analytics
- Debe mostrar 50 eventos de prueba

---

## 🔍 Paso 5: Verificar Conexión a Base de Datos

### Opción A: Desde la Aplicación

1. Ejecutar `dotnet run`
2. Si la app inicia sin errores → ✅ Conexión exitosa
3. Si hay error de conexión → Ver troubleshooting abajo

### Opción B: Verificar en SQL Server

```sql
USE DataTouch;
GO

-- Verificar datos
SELECT COUNT(*) AS TotalOrganizations FROM Organizations;
SELECT COUNT(*) AS TotalUsers FROM Users;
SELECT COUNT(*) AS TotalCards FROM Cards;
SELECT COUNT(*) AS TotalServices FROM Services;
SELECT COUNT(*) AS TotalAnalytics FROM CardAnalytics;
```

**Resultado esperado:**
```
TotalOrganizations: 2
TotalUsers: 2
TotalCards: 1
TotalServices: 3
TotalAnalytics: 50
```

---

## 🐛 Troubleshooting

### Error: "A network-related or instance-specific error occurred"

**Causa:** No puede conectar al servidor SQL Server

**Solución:**
1. Verificar que el servidor `162.248.54.184` es accesible:
   ```powershell
   Test-NetConnection -ComputerName 162.248.54.184 -Port 1433
   ```

2. Verificar credenciales en `appsettings.json`

3. Verificar firewall del servidor SQL Server

### Error: "Login failed for user 'sia'"

**Causa:** Credenciales incorrectas o permisos insuficientes

**Solución:**
1. Verificar usuario y password en `appsettings.json`
2. Verificar que el usuario `sia` tiene permisos en la base de datos `DataTouch`

### Error: "Cannot open database 'DataTouch'"

**Causa:** La base de datos no existe

**Solución:**
```powershell
# Ejecutar script DDL
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d master -i sql/migrations/001_InitialCreate_SQLServer.sql
```

### Error: "The type initializer for 'Microsoft.Data.SqlClient.TdsParser' threw an exception"

**Causa:** Falta el paquete NuGet de SQL Server

**Solución:**
```powershell
cd src/DataTouch.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
```

### La aplicación inicia pero no muestra datos

**Causa:** Base de datos vacía

**Solución:**
```powershell
# Ejecutar seed data
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d DataTouch -i sql/migrations/002_SeedData_SQLServer.sql
```

---

## 📊 Checklist de Verificación

Antes de ejecutar, verifica:

- [ ] ✅ Paquete `Microsoft.EntityFrameworkCore.SqlServer` instalado
- [ ] ✅ Connection string en `appsettings.json` correcto
- [ ] ✅ `Program.cs` usa `UseSqlServer()`
- [ ] ✅ Base de datos `DataTouch` existe en servidor
- [ ] ✅ Tablas creadas (16 tablas)
- [ ] ✅ Datos de prueba insertados
- [ ] ✅ Servidor SQL Server accesible desde tu máquina

---

## 🎯 Flujo Completo desde Cero

Si quieres empezar desde cero:

```powershell
# 1. Instalar paquete SQL Server
cd c:\src\DataTouch\DataTouch\src\DataTouch.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0

# 2. Crear base de datos y tablas
cd c:\src\DataTouch\DataTouch
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d master -i sql/migrations/001_InitialCreate_SQLServer.sql

# 3. Insertar datos de prueba
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d DataTouch -i sql/migrations/002_SeedData_SQLServer.sql

# 4. Ejecutar aplicación
cd src/DataTouch.Web
dotnet run

# 5. Abrir navegador
start https://localhost:5001
```

---

## 🔄 Desarrollo: Limpiar y Reiniciar Datos

```powershell
# Limpiar todas las tablas
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d DataTouch -i sql/migrations/000_TruncateAll.sql

# Insertar datos frescos
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d DataTouch -i sql/migrations/002_SeedData_SQLServer.sql

# Reiniciar aplicación
# Ctrl+C para detener
dotnet run
```

---

## 📝 Notas Importantes

1. **Desarrollo vs Producción:**
   - Actualmente apunta al servidor remoto `162.248.54.184`
   - Para desarrollo local, considera usar SQL Server LocalDB o Docker

2. **Sensitive Data Logging:**
   - Está habilitado en Development para debugging
   - Se desactiva automáticamente en Production

3. **Connection String Seguro:**
   - En producción, usa User Secrets o Azure Key Vault
   - No commitear passwords en el código

4. **Migraciones:**
   - Actualmente usamos scripts SQL manuales
   - Considera usar EF Core Migrations para cambios futuros

---

*Guía de Ejecución Local - DataTouch v1.0*
