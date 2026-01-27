# 🗑️ Archivos a Eliminar - Migración SQL Server

## Scripts de Migración Obsoletos (Iteraciones Fallidas)

Estos archivos fueron intentos intermedios que fallaron o fueron reemplazados por `011_CompleteDDL_Manual.sql`:

### ❌ Scripts EF Core (No funcionaron por CASCADE conflicts)
- `sql/migrations/006_EFCore_Complete.sql` - Primer intento EF Core, incompleto
- `sql/migrations/007_EFCore_Final.sql` - Segundo intento EF Core, CASCADE errors
- `sql/migrations/010_CompleteDDL_SQLServer.sql` - Basado en EF Core, QUOTED_IDENTIFIER errors

### ❌ Scripts ALTER Incrementales (Reemplazados por DDL completo)
- `sql/migrations/003_AlterQuoteRequests.sql` - ALTER para QuoteRequests (ya en 011)
- `sql/migrations/004_FixPasswordHashes.sql` - Fix de hashes (ya en 002_SeedData)
- `sql/migrations/005_AlterCards.sql` - ALTER para Cards (ya en 011)
- `sql/migrations/008_AlterServices.sql` - ALTER para Services (ya en 011)
- `sql/migrations/009_AlterBookingSettings.sql` - ALTER para BookingSettings (ya en 011)

### ❌ Documentación Temporal
- `sql/migrations/current_schema.txt` - Análisis temporal del schema
- `sql/migrations/README_EXECUTION.md` - Duplicado de MANUAL_EXECUTION.md

### ❌ Migraciones EF Core (No usadas)
- `src/DataTouch.Infrastructure/Data/Migrations/` - Carpeta completa de EF Core migrations

## ✅ Archivos a MANTENER

### Scripts Finales (Producción)
- `sql/migrations/000_TruncateAll.sql` - Limpieza de datos
- `sql/migrations/001_InitialCreate_SQLServer.sql` - DDL inicial (referencia histórica)
- `sql/migrations/002_SeedData_SQLServer.sql` - Datos de prueba
- `sql/migrations/011_CompleteDDL_Manual.sql` - **DDL FINAL Y FUNCIONAL**
- `sql/migrations/MANUAL_EXECUTION.md` - Instrucciones de ejecución
- `sql/migrations/MIGRATION_ANALYSIS.md` - Análisis de problemas encontrados

### Scripts Auxiliares
- `sql/migrations/generate-password-hash.ps1` - Utilidad para generar hashes

### Documentación
- `docs/LOCALHOST_TESTING.md` - Guía de testing
- `docs/GITFLOW*.md` - Documentación de GitFlow
- `docs/GITHUB_SETUP.md` - Setup de GitHub

### Scripts de Automatización
- `scripts/` - Scripts de deployment y utilidades

---

## 📋 Resumen

**Total a eliminar:** 12 archivos  
**Total a mantener:** 11 archivos + carpetas de docs y scripts

**Razón:** Los archivos obsoletos fueron intentos iterativos que fallaron. El DDL final (`011_CompleteDDL_Manual.sql`) contiene TODO lo necesario para crear la base de datos completa y funcional.
