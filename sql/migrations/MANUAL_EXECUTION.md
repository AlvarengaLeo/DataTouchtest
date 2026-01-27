# 🚀 Instrucciones de Ejecución Manual - SQL Server Migration

## 📋 Scripts Listos para Ejecutar

### 1️⃣ Eliminar y Recrear Base de Datos
```powershell
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -Q "DROP DATABASE IF EXISTS DataTouch; CREATE DATABASE DataTouch;"
```

### 2️⃣ Crear Todas las Tablas (DDL Completo)
```powershell
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d DataTouch -i sql/migrations/011_CompleteDDL_Manual.sql
```

### 3️⃣ Insertar Datos de Prueba
```powershell
sqlcmd -S 162.248.54.184 -U sia -P fuGvDyHxN9k8JyR -d DataTouch -i sql/migrations/002_SeedData_SQLServer.sql
```

### 4️⃣ Iniciar Aplicación
```powershell
cd src/DataTouch.Web
dotnet run
```

---

## ✅ Verificación del DDL

### Tablas Creadas (16 total)

| # | Tabla | Columnas Clave | Status |
|---|-------|----------------|--------|
| 1 | **Organizations** | Id, Name, Slug, Country | ✅ Base |
| 2 | **Users** | Id, Email, PasswordHash, Role | ✅ Completo |
| 3 | **CardTemplates** | Id, DefaultStyleJson, DefaultComponentsJson | ✅ Completo |
| 4 | **Cards** | Id, SocialLinksJson, GalleryImagesJson, AppearanceStyleJson, TemplateType | ✅ Completo |
| 5 | **CardStyles** | Id, PrimaryColor, BackgroundType, QrShape | ✅ Completo |
| 6 | **CardComponents** | Id, ConfigJson, DataJson, DisplayOrder | ✅ Completo |
| 7 | **CardAnalytics** | Id, EventType, MetadataJson, IpAddress | ✅ Completo |
| 8 | **Leads** | Id, FullName, Email, Status, Source | ✅ Completo |
| 9 | **LeadNotes** | Id, Content, CreatedByUserId | ✅ Completo |
| 10 | **Services** | Id, Modality, BufferBeforeMinutes, QuoteFormConfigJson | ✅ Completo |
| 11 | **Appointments** | Id, StartDateTime, Status, CustomerName | ✅ Completo |
| 12 | **AvailabilityRules** | Id, DayOfWeek, StartTime, EndTime | ✅ Completo |
| 13 | **AvailabilityExceptions** | Id, ExceptionDate, ExceptionType | ✅ Completo |
| 14 | **BookingSettings** | Id, SlotIntervalMinutes, BufferBeforeMinutes, MaxAppointmentsPerDay | ✅ Completo |
| 15 | **QuoteRequests** | Id, ServiceId, QuotedAmount, SlaDeadlineAt, AttachmentsJson | ✅ Completo |
| 16 | **Activities** | Id, EntityType, Type, MetadataJson | ✅ Completo |

---

## 🔧 Configuración de CASCADE

### CASCADE (Elimina hijos automáticamente)
- Organizations → Users
- Organizations → CardTemplates  
- Organizations → CardStyles
- Organizations → Activities
- Cards → CardStyles
- Cards → CardComponents
- Cards → CardAnalytics
- Cards → AvailabilityRules
- Cards → AvailabilityExceptions
- Cards → BookingSettings
- Cards → Services
- Cards → QuoteRequests
- Leads → LeadNotes

### NO ACTION (Requiere eliminación manual)
- Organizations → Cards
- Organizations → Leads
- Organizations → Services
- Organizations → Appointments
- Organizations → QuoteRequests
- Cards → Leads
- Cards → Appointments
- Users → Cards
- Users → Leads
- Users → LeadNotes
- Users → QuoteRequests
- Users → Activities
- Services → Appointments
- Services → QuoteRequests

### SET NULL (Limpia referencia)
- CardTemplates → Cards
- CardStyles → Cards
- Services → Appointments
- Appointments → QuoteRequests

---

## 📊 Columnas Especiales Verificadas

### JSON Columns (NVARCHAR(MAX))
✅ Cards: `SocialLinksJson`, `WebsiteLinksJson`, `GalleryImagesJson`, `AppearanceStyleJson`  
✅ CardTemplates: `DefaultStyleJson`, `DefaultComponentsJson`  
✅ CardComponents: `ConfigJson`, `DataJson`  
✅ CardAnalytics: `MetadataJson`  
✅ Services: `QuoteFormConfigJson`  
✅ QuoteRequests: `AttachmentsJson`, `CustomFieldsJson`  

### Decimal Columns
✅ Services: `PriceFrom` DECIMAL(10,2)  
✅ QuoteRequests: `QuotedAmount` DECIMAL(18,2), `FinalAmount` DECIMAL(18,2)  

### Missing Columns Previously Identified
✅ Cards: `GalleryImagesJson`, `AppearanceStyleJson`, `TemplateType`  
✅ Services: `Modality`, `BufferBeforeMinutes`, `BufferAfterMinutes`, `MinNoticeMinutes`, `MaxBookingsPerDay`, `QuoteFormConfigJson`  
✅ BookingSettings: `SlotIntervalMinutes`, `BufferBeforeMinutes`, `BufferAfterMinutes`, `MaxAppointmentsPerDay`, `MinNoticeMinutes`, `MaxAdvanceDays`  
✅ QuoteRequests: Todas las 34 columnas incluidas  

---

## 🎯 Credenciales Demo

```
Email: admin@techcorp.com
Password: admin123
```

## 🌐 URLs

- **Aplicación:** http://localhost:5233
- **Login:** http://localhost:5233/login
- **Dashboard:** http://localhost:5233/dashboard
- **Tarjeta Pública:** http://localhost:5233/p/techcorp/leonel-alvarenga

---

## ⚠️ Notas Importantes

1. **Orden de Ejecución:** Ejecutar los scripts en el orden indicado (1→2→3→4)
2. **CASCADE Conflicts:** Todos resueltos usando NO ACTION donde era necesario
3. **Idempotencia:** El script DROP DATABASE asegura inicio limpio
4. **Verificación:** Después de ejecutar, verificar que las 16 tablas existan

---

**Listo para ejecutar!** 🚀
