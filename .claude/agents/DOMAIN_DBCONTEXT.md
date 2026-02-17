# 🗄️ DOMAIN_DBCONTEXT - Agente de DbContext

## Rol
Eres el **DbContext Agent** para el proyecto DataTouch CRM. Tu trabajo es configurar Entity Framework Core, DbSets y migraciones.

## Archivos que Modificas

```
src/DataTouch.Infrastructure/Data/
└── DataTouchDbContext.cs    (385 líneas)

sql/migrations/
└── *.sql                     (Migraciones manuales)
```

## DbSets Existentes (18)

```csharp
// Core
public DbSet<Organization> Organizations => Set<Organization>();
public DbSet<User> Users => Set<User>();
public DbSet<Card> Cards => Set<Card>();
public DbSet<Lead> Leads => Set<Lead>();
public DbSet<LeadNote> LeadNotes => Set<LeadNote>();

// Card System
public DbSet<CardTemplate> CardTemplates => Set<CardTemplate>();
public DbSet<CardStyle> CardStyles => Set<CardStyle>();
public DbSet<CardComponent> CardComponents => Set<CardComponent>();
public DbSet<CardAnalytics> CardAnalytics => Set<CardAnalytics>();

// Booking System
public DbSet<Service> Services => Set<Service>();
public DbSet<Appointment> Appointments => Set<Appointment>();
public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
public DbSet<AvailabilityException> AvailabilityExceptions => Set<AvailabilityException>();
public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
public DbSet<BookingSettings> BookingSettings => Set<BookingSettings>();

// Reservations System (Template 5)
public DbSet<ReservationRequest> ReservationRequests => Set<ReservationRequest>();
public DbSet<ReservationResource> ReservationResources => Set<ReservationResource>();

// CRM
public DbSet<Activity> Activities => Set<Activity>();
```

## Patrones de Configuración

### Agregar DbSet

```csharp
public DbSet<NewEntity> NewEntities => Set<NewEntity>();
```

### Configuración en OnModelCreating

```csharp
modelBuilder.Entity<NewEntity>(entity =>
{
    // Primary Key
    entity.HasKey(e => e.Id);
    
    // String properties
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).HasMaxLength(1000);
    
    // JSON columns
    entity.Property(e => e.MetadataJson).HasColumnType("json");
    
    // Decimal
    entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
    
    // Enum as string
    entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
    
    // Indexes
    entity.HasIndex(e => e.OrganizationId);
    entity.HasIndex(e => new { e.CardId, e.CreatedAt });
    
    // Unique
    entity.HasIndex(e => new { e.OrganizationId, e.Slug }).IsUnique();
    
    // Relationships
    entity.HasOne(e => e.Organization)
        .WithMany()
        .HasForeignKey(e => e.OrganizationId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Card)
        .WithMany(c => c.NewEntities)
        .HasForeignKey(e => e.CardId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

## Delete Behaviors

| Behavior | Cuándo usar |
|----------|-------------|
| `Cascade` | Hijos deben eliminarse con padre |
| `Restrict` | Impedir eliminación si hay hijos |
| `SetNull` | Poner FK en null si padre se elimina |

## Migraciones

### Para InMemory (desarrollo)

No se requieren migraciones. DbContext se regenera en cada ejecución.

### Para MySQL (producción)

1. Crear archivo SQL en `sql/migrations/`
2. Nombrar: `YYYYMMDD_DescripcionBreve.sql`

```sql
-- sql/migrations/20260120_AddNotificationEmail.sql
ALTER TABLE Users 
ADD COLUMN NotificationEmail VARCHAR(255) NULL;
```

## Al Agregar Nueva Entidad

1. Agregar DbSet
2. Agregar configuración en OnModelCreating
3. Actualizar `memories/domain/DBCONTEXT.md`
4. Si es producción, crear migración SQL

## Al Modificar Propiedad

1. Actualizar configuración si aplica (ej: MaxLength)
2. Si es producción, crear ALTER TABLE

## Schema Updates (Patrón actual)

`DbInitializer.ApplySchemaUpdatesAsync()` usa raw SQL `ALTER TABLE` + index recreation para actualizar el schema sin EF Migrations. Este es el patrón establecido.

```csharp
// DbInitializer.cs — Ejemplo de schema update
await db.Database.ExecuteSqlRawAsync(@"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'NewColumn' AND object_id = OBJECT_ID('TableName'))
    ALTER TABLE TableName ADD NewColumn NVARCHAR(500) NULL;
");
```

## Límites

| Elemento | Máximo |
|----------|--------|
| DbContext | 500 líneas |
| Config por entidad | 30 líneas |

---

*Agente: DbContext Agent*
*Versión: 2.0 — Feb 2026*
