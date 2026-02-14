using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

public static class DbInitializer
{
    // Flag to enable demo seed data (set via environment or config)
    public static bool UseDemoSeed { get; set; } = true;

    /// <summary>
    /// Apply incremental schema changes for columns added after initial EnsureCreated.
    /// Uses IF NOT EXISTS to be idempotent (safe to run multiple times).
    /// </summary>
    public static async Task ApplySchemaUpdatesAsync(DataTouchDbContext context)
    {
        var updates = new[]
        {
            // QuoteSettingsJson on Cards (added for quote-request template)
            @"IF COL_LENGTH('Cards', 'QuoteSettingsJson') IS NULL
              ALTER TABLE [Cards] ADD [QuoteSettingsJson] NVARCHAR(MAX) NULL;",

            // CustomFieldsJson on QuoteRequests (added for quote-request metadata)
            @"IF COL_LENGTH('QuoteRequests', 'CustomFieldsJson') IS NULL
              ALTER TABLE [QuoteRequests] ADD [CustomFieldsJson] NVARCHAR(MAX) NULL;",

            // Make ServiceId nullable on QuoteRequests (for quote-request template quotes without service)
            // Note: This alters the column type; the FK already allows NULL via SetNull delete behavior
            @"IF EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'QuoteRequests' AND COLUMN_NAME = 'ServiceId' AND IS_NULLABLE = 'NO'
              )
              BEGIN
                  -- Drop FK constraint first
                  DECLARE @fkName NVARCHAR(256);
                  SELECT @fkName = fk.name
                  FROM sys.foreign_keys fk
                  INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                  INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                  WHERE OBJECT_NAME(fk.parent_object_id) = 'QuoteRequests' AND c.name = 'ServiceId';

                  IF @fkName IS NOT NULL
                      EXEC('ALTER TABLE [QuoteRequests] DROP CONSTRAINT [' + @fkName + ']');

                  ALTER TABLE [QuoteRequests] ALTER COLUMN [ServiceId] UNIQUEIDENTIFIER NULL;

                  -- Re-add FK
                  ALTER TABLE [QuoteRequests] ADD CONSTRAINT [FK_QuoteRequests_Services_ServiceId]
                      FOREIGN KEY ([ServiceId]) REFERENCES [Services]([Id]) ON DELETE SET NULL;
              END;",

            // Make CustomerEmail nullable on QuoteRequests (phone-only contact allowed)
            @"IF EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'QuoteRequests' AND COLUMN_NAME = 'CustomerEmail' AND IS_NULLABLE = 'NO'
              )
              ALTER TABLE [QuoteRequests] ALTER COLUMN [CustomerEmail] NVARCHAR(255) NULL;",

            // AvailabilityRule: add BreakStartTime, BreakEndTime columns
            @"IF COL_LENGTH('AvailabilityRules', 'BreakStartTime') IS NULL
              ALTER TABLE [AvailabilityRules] ADD [BreakStartTime] TIME NULL;",

            @"IF COL_LENGTH('AvailabilityRules', 'BreakEndTime') IS NULL
              ALTER TABLE [AvailabilityRules] ADD [BreakEndTime] TIME NULL;",

            // AvailabilityRule: add ServiceId for per-service schedule override
            @"IF COL_LENGTH('AvailabilityRules', 'ServiceId') IS NULL
              ALTER TABLE [AvailabilityRules] ADD [ServiceId] UNIQUEIDENTIFIER NULL;",

            // Drop old unique index (CardId, DayOfWeek) and recreate as (CardId, DayOfWeek, ServiceId)
            @"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AvailabilityRules_CardId_DayOfWeek' AND object_id = OBJECT_ID('AvailabilityRules'))
              DROP INDEX [IX_AvailabilityRules_CardId_DayOfWeek] ON [AvailabilityRules];",

            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AvailabilityRules_CardId_DayOfWeek_ServiceId' AND object_id = OBJECT_ID('AvailabilityRules'))
              CREATE UNIQUE INDEX [IX_AvailabilityRules_CardId_DayOfWeek_ServiceId] ON [AvailabilityRules] ([CardId], [DayOfWeek], [ServiceId]);",

            // Service: add UseGlobalSchedule
            @"IF COL_LENGTH('Services', 'UseGlobalSchedule') IS NULL
              ALTER TABLE [Services] ADD [UseGlobalSchedule] BIT NOT NULL DEFAULT 1;",

            // ReservationSettingsJson on Cards (added for reservations-range template)
            @"IF COL_LENGTH('Cards', 'ReservationSettingsJson') IS NULL
              ALTER TABLE [Cards] ADD [ReservationSettingsJson] NVARCHAR(MAX) NULL;",

            // ReservationResources table
            @"IF OBJECT_ID('ReservationResources', 'U') IS NULL
              CREATE TABLE [ReservationResources] (
                  [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                  [CardId] UNIQUEIDENTIFIER NOT NULL,
                  [Name] NVARCHAR(200) NOT NULL,
                  [Description] NVARCHAR(1000) NULL,
                  [MaxGuests] INT NOT NULL DEFAULT 10,
                  [PricePerNight] DECIMAL(10,2) NULL,
                  [IsActive] BIT NOT NULL DEFAULT 1,
                  [DisplayOrder] INT NOT NULL DEFAULT 0,
                  [BlockedDatesJson] NVARCHAR(MAX) NULL,
                  [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  [UpdatedAt] DATETIME2 NULL,
                  CONSTRAINT [FK_ReservationResources_Cards] FOREIGN KEY ([CardId]) REFERENCES [Cards]([Id]) ON DELETE CASCADE
              );",

            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationResources_CardId' AND object_id = OBJECT_ID('ReservationResources'))
              CREATE INDEX [IX_ReservationResources_CardId] ON [ReservationResources] ([CardId]);",

            // ReservationRequests table
            @"IF OBJECT_ID('ReservationRequests', 'U') IS NULL
              CREATE TABLE [ReservationRequests] (
                  [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                  [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
                  [CardId] UNIQUEIDENTIFIER NOT NULL,
                  [ResourceId] UNIQUEIDENTIFIER NULL,
                  [LeadId] UNIQUEIDENTIFIER NULL,
                  [RequestNumber] NVARCHAR(20) NOT NULL,
                  [FromDate] DATETIME2 NOT NULL,
                  [ToDate] DATETIME2 NOT NULL,
                  [Nights] INT NOT NULL DEFAULT 1,
                  [GuestsAdults] INT NOT NULL DEFAULT 1,
                  [GuestsChildren] INT NOT NULL DEFAULT 0,
                  [ExtrasJson] NVARCHAR(MAX) NULL,
                  [Notes] NVARCHAR(2000) NULL,
                  [ContactName] NVARCHAR(200) NOT NULL,
                  [ContactPhone] NVARCHAR(50) NULL,
                  [ContactPhoneCountryCode] NVARCHAR(10) NULL,
                  [ContactEmail] NVARCHAR(255) NULL,
                  [Status] NVARCHAR(20) NOT NULL DEFAULT 'New',
                  [StatusReason] NVARCHAR(500) NULL,
                  [Source] NVARCHAR(50) NOT NULL DEFAULT 'PublicCard',
                  [TemplateKey] NVARCHAR(50) NOT NULL DEFAULT 'ReservationsRange',
                  [IpAddress] NVARCHAR(50) NULL,
                  [UserAgent] NVARCHAR(500) NULL,
                  [IdempotencyKey] NVARCHAR(100) NULL,
                  [InternalNotes] NVARCHAR(2000) NULL,
                  [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  [UpdatedAt] DATETIME2 NULL,
                  CONSTRAINT [FK_ReservationRequests_Cards] FOREIGN KEY ([CardId]) REFERENCES [Cards]([Id]) ON DELETE CASCADE,
                  CONSTRAINT [FK_ReservationRequests_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations]([Id]),
                  CONSTRAINT [FK_ReservationRequests_Resources] FOREIGN KEY ([ResourceId]) REFERENCES [ReservationResources]([Id]) ON DELETE NO ACTION
              );",

            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationRequests_CardId' AND object_id = OBJECT_ID('ReservationRequests'))
              CREATE INDEX [IX_ReservationRequests_CardId] ON [ReservationRequests] ([CardId]);",

            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationRequests_Status' AND object_id = OBJECT_ID('ReservationRequests'))
              CREATE INDEX [IX_ReservationRequests_Status] ON [ReservationRequests] ([Status]);",

            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReservationRequests_CardId_FromDate' AND object_id = OBJECT_ID('ReservationRequests'))
              CREATE INDEX [IX_ReservationRequests_CardId_FromDate] ON [ReservationRequests] ([CardId], [FromDate]);",

            // PortfolioGalleryJson on Cards (photos + videos with enable flags)
            @"IF COL_LENGTH('Cards', 'PortfolioGalleryJson') IS NULL
              ALTER TABLE [Cards] ADD [PortfolioGalleryJson] NVARCHAR(MAX) NULL;",
        };

        foreach (var sql in updates)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Schema update FAILED: {ex.Message}");
                Console.WriteLine($"  SQL: {sql.Substring(0, Math.Min(sql.Length, 200))}...");
            }
        }

        Console.WriteLine("Schema updates applied successfully.");
    }

    /// <summary>
    /// Force seed demo analytics data to existing database (without deleting it).
    /// Call this to add demo data for dashboard visualization.
    /// </summary>
    public static async Task ForceSeedDemoAnalyticsAsync(DataTouchDbContext context)
    {
        // Get existing organization and card
        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null)
        {
            Console.WriteLine("No organization found. Run InitializeAsync first.");
            return;
        }

        var card = await context.Cards.FirstOrDefaultAsync(c => c.OrganizationId == organization.Id && c.IsActive);
        if (card == null)
        {
            Console.WriteLine("No active card found.");
            return;
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.OrganizationId == organization.Id);

        // Clear existing analytics for clean demo
        var existingAnalytics = await context.CardAnalytics.Where(a => a.CardId == card.Id).ToListAsync();
        if (existingAnalytics.Any())
        {
            context.CardAnalytics.RemoveRange(existingAnalytics);
            await context.SaveChangesAsync();
            Console.WriteLine($"Cleared {existingAnalytics.Count} existing analytics events.");
        }

        // Get or create leads
        var leads = await context.Leads.Where(l => l.OrganizationId == organization.Id).ToListAsync();
        if (!leads.Any() && user != null)
        {
            Console.WriteLine("No leads found. Creating demo leads...");
            leads = CreateDemoLeads(organization.Id, card.Id, user.Id);
            context.Leads.AddRange(leads);
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {leads.Count} demo leads.");
        }

        // Seed new demo events
        await SeedEngagementEventsAsync(context, organization.Id, card.Id, leads);
        Console.WriteLine("Demo analytics data seeded successfully!");
    }

    /// <summary>
    /// Creates demo leads for testing
    /// </summary>
    private static List<Lead> CreateDemoLeads(Guid organizationId, Guid cardId, Guid userId)
    {
        return new List<Lead>
        {
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Juan Pérez",
                Email = "juan@example.com",
                Phone = "+503 7123 4567",
                Message = "Me interesa conocer más sobre sus servicios",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-3)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "María García",
                Email = "maria@example.com",
                Phone = "+503 7890 1234",
                Message = "Quisiera agendar una reunión",
                Source = "CARD_CONTACT_FORM",
                Status = "Contacted",
                CreatedAt = DateTime.UtcNow.AddDays(-5).AddHours(-5)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Carlos López",
                Email = "carlos@example.com",
                Phone = "+503 7456 7890",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddDays(-8).AddHours(-2)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Sarah Johnson",
                Email = "sarah@example.com",
                Phone = "+1 555 123 4567",
                Message = "Looking for partnership opportunities",
                Source = "CARD_CONTACT_FORM",
                Status = "Qualified",
                CreatedAt = DateTime.UtcNow.AddDays(-12).AddHours(-8)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Roberto Martínez",
                Email = "roberto@example.com",
                Phone = "+52 55 1234 5678",
                Source = "CARD_CONTACT_FORM",
                Status = "Closed",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Ana Fernández",
                Email = "ana@example.com",
                Phone = "+503 7111 2222",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddDays(-18).AddHours(-10)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Pedro Sánchez",
                Email = "pedro@example.com",
                Phone = "+503 7333 4444",
                Source = "CARD_CONTACT_FORM",
                Status = "Contacted",
                CreatedAt = DateTime.UtcNow.AddDays(-22).AddHours(-14)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CardId = cardId,
                OwnerUserId = userId,
                FullName = "Laura Torres",
                Email = "laura@example.com",
                Phone = "+503 7555 6666",
                Source = "CARD_CONTACT_FORM",
                Status = "Qualified",
                CreatedAt = DateTime.UtcNow.AddDays(-25).AddHours(-9)
            }
        };
    }

    public static async Task InitializeAsync(DataTouchDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if already seeded
        if (await context.Organizations.AnyAsync())
        {
            return;
        }

        // Create demo organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Demo Company",
            Slug = "demo-company",
            Country = "El Salvador",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(organization);

        // Create demo user
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            Email = "admin@demo.com",
            PasswordHash = AuthService.HashPassword("admin123"),
            FullName = "Admin Demo",
            Role = "OrgAdmin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        // Create demo cards (one active, one inactive for testing)
        var activeCard = new Card
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserId = user.Id,
            Slug = "admin-demo",
            FullName = "Admin Demo",
            Title = "Gerente General",
            CompanyName = "Demo Company",
            Bio = "Experto en transformación digital y desarrollo de negocios. Más de 10 años de experiencia en el sector tecnológico.",
            Phone = "555-0100",
            PhoneCountryCode = "+503",
            WhatsAppNumber = "7000-0000",
            WhatsAppCountryCode = "+503",
            Email = "admin@demo.com",
            ProfileImageUrl = "https://ui-avatars.com/api/?name=Admin+Demo&background=5D3FD3&color=fff&size=256",
            ShowSaveContact = true,
            ShowWhatsApp = true,
            ShowCall = true,
            ShowEmail = true,
            SocialLinksJson = "{\"linkedin\":\"https://linkedin.com/in/demo\",\"instagram\":\"https://instagram.com/demo\"}",
            WebsiteLinksJson = "[{\"title\":\"Nuestros Servicios\",\"url\":\"https://example.com/services\"},{\"title\":\"Agendar Consulta\",\"url\":\"https://example.com/book\"}]",
            TemplateType = "services-quotes", // Use services template for demo
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var inactiveCard = new Card
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserId = user.Id,
            Slug = "inactive-demo",
            FullName = "Tarjeta Inactiva",
            Title = "Test Card",
            CompanyName = "Demo Company",
            Email = "inactive@demo.com",
            IsActive = true, // Card is active but has no interactions
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        context.Cards.Add(activeCard);
        context.Cards.Add(inactiveCard);
        
        // Create sample services for services template demo
        var services = new List<Service>
        {
            new Service
            {
                Id = Guid.NewGuid(),
                CardId = activeCard.Id,
                OrganizationId = organization.Id,
                Name = "Consultoría Empresarial",
                Description = "Asesoría estratégica para tu negocio",
                DurationMinutes = 60,
                PriceFrom = 50m,
                DisplayOrder = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Services.AddRange(services);
        
        // Create availability rules for booking system (Monday-Friday, 9am-5pm)
        var availabilityRules = new List<AvailabilityRule>();
        for (int day = 1; day <= 5; day++) // Monday (1) to Friday (5)
        {
            availabilityRules.Add(new AvailabilityRule
            {
                Id = Guid.NewGuid(),
                CardId = activeCard.Id,
                DayOfWeek = day, // int: 0=Sunday, 1=Monday, etc.
                StartTime = new TimeSpan(9, 0, 0),  // 9:00 AM
                EndTime = new TimeSpan(17, 0, 0),   // 5:00 PM
                IsActive = true
            });
        }
        context.AvailabilityRules.AddRange(availabilityRules);
        
        // Create booking settings for demo card
        var bookingSettings = new BookingSettings
        {
            Id = Guid.NewGuid(),
            CardId = activeCard.Id,
            TimeZoneId = "America/El_Salvador",
            SlotIntervalMinutes = 30,
            BufferBeforeMinutes = 0,
            BufferAfterMinutes = 15,
            MaxAppointmentsPerDay = 10,
            MinNoticeMinutes = 60,
            MaxAdvanceDays = 30,
            CreatedAt = DateTime.UtcNow
        };
        context.BookingSettings.Add(bookingSettings);

        // Create sample leads with varied dates over 30 days
        var leads = new List<Lead>
        {
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Juan Pérez",
                Email = "juan@example.com",
                Phone = "+503 7123 4567",
                Message = "Me interesa conocer más sobre sus servicios",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-3)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "María García",
                Email = "maria@example.com",
                Phone = "+503 7890 1234",
                Message = "Quisiera agendar una reunión",
                Source = "CARD_CONTACT_FORM",
                Status = "Contacted",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-5)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Carlos López",
                Email = "carlos@example.com",
                Phone = "+503 7456 7890",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Sarah Johnson",
                Email = "sarah@example.com",
                Phone = "+1 555 123 4567",
                Message = "Looking for partnership opportunities",
                Source = "CARD_CONTACT_FORM",
                Status = "Qualified",
                CreatedAt = DateTime.UtcNow.AddDays(-3).AddHours(-8)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Roberto Martínez",
                Email = "roberto@example.com",
                Phone = "+52 55 1234 5678",
                Source = "CARD_CONTACT_FORM",
                Status = "Closed",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            // Additional leads distributed over 30 days for weekday aggregation demo
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Ana Fernández",
                Email = "ana@example.com",
                Phone = "+503 7111 2222",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddDays(-7).AddHours(-10)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Pedro Sánchez",
                Email = "pedro@example.com",
                Phone = "+503 7333 4444",
                Source = "CARD_CONTACT_FORM",
                Status = "Contacted",
                CreatedAt = DateTime.UtcNow.AddDays(-10).AddHours(-14)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Laura Torres",
                Email = "laura@example.com",
                Phone = "+503 7555 6666",
                Source = "CARD_CONTACT_FORM",
                Status = "Qualified",
                CreatedAt = DateTime.UtcNow.AddDays(-14).AddHours(-9)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Miguel Ángel Ruiz",
                Email = "miguel@example.com",
                Phone = "+503 7777 8888",
                Source = "CARD_CONTACT_FORM",
                Status = "New",
                CreatedAt = DateTime.UtcNow.AddDays(-18).AddHours(-11)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Carmen Díaz",
                Email = "carmen@example.com",
                Phone = "+503 7999 0000",
                Source = "CARD_CONTACT_FORM",
                Status = "Contacted",
                CreatedAt = DateTime.UtcNow.AddDays(-21).AddHours(-15)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "David Morales",
                Email = "david@example.com",
                Phone = "+1 555 987 6543",
                Source = "CARD_CONTACT_FORM",
                Status = "Qualified",
                CreatedAt = DateTime.UtcNow.AddDays(-25).AddHours(-8)
            },
            new Lead
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CardId = activeCard.Id,
                OwnerUserId = user.Id,
                FullName = "Patricia Hernández",
                Email = "patricia@example.com",
                Phone = "+52 55 9999 8888",
                Source = "CARD_CONTACT_FORM",
                Status = "Closed",
                CreatedAt = DateTime.UtcNow.AddDays(-28).AddHours(-12)
            }
        };

        context.Leads.AddRange(leads);

        await context.SaveChangesAsync();

        // Seed engagement events for dashboard (only if UseDemoSeed is true)
        if (UseDemoSeed)
        {
            await SeedEngagementEventsAsync(context, organization.Id, activeCard.Id, leads);
        }
        
        // Seed card templates (system templates)
        await CardTemplateSeeder.SeedTemplatesAsync(context);
    }

    private static async Task SeedEngagementEventsAsync(
        DataTouchDbContext context, 
        Guid organizationId, 
        Guid cardId,
        List<Lead> leads)
    {
        var random = new Random(42); // Fixed seed for reproducible data
        var events = new List<CardAnalytics>();
        var now = DateTime.UtcNow;

        // Location distribution with coordinates (for real map rendering)
        var locations = new (string City, string Country, string CC, double Lat, double Lng)[]
        {
            ("San Salvador", "El Salvador", "SV", 13.6929, -89.2182),
            ("San Salvador", "El Salvador", "SV", 13.6929, -89.2182),
            ("San Salvador", "El Salvador", "SV", 13.6929, -89.2182), // More weight
            ("Santa Ana", "El Salvador", "SV", 13.9942, -89.5597),
            ("San Miguel", "El Salvador", "SV", 13.4833, -88.1833),
            ("Boston", "United States", "US", 42.3601, -71.0589),
            ("Miami", "United States", "US", 25.7617, -80.1918),
            ("Ciudad de México", "Mexico", "MX", 19.4326, -99.1332)
        };

        // Generate ~200 page views over 30 days (to show weekday aggregation)
        for (int i = 0; i < 200; i++)
        {
            var daysAgo = random.Next(0, 30);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            events.Add(CreateEvent(cardId, "page_view", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random));
        }

        // Generate ~50 QR scans over 30 days
        for (int i = 0; i < 50; i++)
        {
            var daysAgo = random.Next(0, 30);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            events.Add(CreateEvent(cardId, "qr_scan", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random));
        }

        // Generate link clicks with specific types for Top Links widget (over 30 days)
        var linkTypes = new[] { "whatsapp", "email", "linkedin", "portfolio", "website", "instagram" };
        var clickCounts = new[] { 80, 30, 20, 15, 10, 8 }; // More clicks distributed over 30 days

        for (int linkIndex = 0; linkIndex < linkTypes.Length; linkIndex++)
        {
            for (int i = 0; i < clickCounts[linkIndex]; i++)
            {
                var daysAgo = random.Next(0, 30);
                var hoursAgo = random.Next(0, 23);
                var loc = locations[random.Next(locations.Length)];
                
                var evt = CreateEvent(cardId, "cta_click", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random);
                evt.MetadataJson = $"{{\"button\":\"{linkTypes[linkIndex]}\"}}";
                evt.Channel = linkTypes[linkIndex];
                events.Add(evt);
            }
        }

        // Generate contact saves (high-intent) - linked to leads
        for (int i = 0; i < Math.Min(leads.Count, 8); i++)
        {
            var lead = leads[i % leads.Count];
            var loc = locations[random.Next(3)]; // Mostly local
            
            // Create contact save event shortly before lead creation
            var eventTime = lead.CreatedAt.AddMinutes(-random.Next(1, 30));
            var evt = CreateEvent(cardId, "contact_save", eventTime, loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random);
            events.Add(evt);
        }

        // Generate form submits - linked to leads
        foreach (var lead in leads)
        {
            var loc = locations[random.Next(3)];
            var evt = CreateEvent(cardId, "form_submit", lead.CreatedAt.AddSeconds(-random.Next(10, 60)), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random);
            evt.MetadataJson = $"{{\"lead_id\":\"{lead.Id}\"}}";
            events.Add(evt);
        }

        // Generate meeting booked events (high-intent)
        for (int i = 0; i < 3; i++)
        {
            var daysAgo = random.Next(0, 5);
            var hoursAgo = random.Next(8, 18); // Business hours
            var loc = locations[random.Next(locations.Length)];
            
            var evt = CreateEvent(cardId, "cta_click", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random);
            evt.MetadataJson = "{\"button\":\"calendar\",\"type\":\"meeting_booked\"}";
            evt.Channel = "calendar";
            events.Add(evt);
        }

        // Generate additional call clicks (high-intent)
        for (int i = 0; i < 5; i++)
        {
            var daysAgo = random.Next(0, 6);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            var evt = CreateEvent(cardId, "cta_click", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random);
            evt.MetadataJson = "{\"button\":\"call\"}";
            evt.Channel = "call";
            events.Add(evt);
        }

        // Add some previous period events for comparison (8-14 days ago)
        for (int i = 0; i < 35; i++) // Fewer events = growth trend
        {
            var daysAgo = random.Next(8, 14);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            events.Add(CreateEvent(cardId, "page_view", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random));
        }

        for (int i = 0; i < 8; i++)
        {
            var daysAgo = random.Next(8, 14);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            var evt = CreateEvent(cardId, "cta_click", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random);
            evt.MetadataJson = "{\"button\":\"whatsapp\"}";
            evt.Channel = "whatsapp";
            events.Add(evt);
        }

        context.CardAnalytics.AddRange(events);
        await context.SaveChangesAsync();
    }

    private static CardAnalytics CreateEvent(
        Guid cardId, 
        string eventType, 
        DateTime timestamp, 
        string city, 
        string country,
        string countryCode,
        double lat,
        double lng,
        Random random)
    {
        var deviceRoll = random.NextDouble();
        var device = deviceRoll < 0.65 ? "mobile" : (deviceRoll < 0.95 ? "desktop" : "tablet");

        return new CardAnalytics
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            EventType = eventType,
            Timestamp = timestamp,
            City = city,
            Country = country,
            CountryCode = countryCode,
            Latitude = lat,
            Longitude = lng,
            GeoSource = "seed",
            DeviceType = device,
            IpHash = GeoLocationService.HashIpAddress($"192.168.{random.Next(1, 255)}.{random.Next(1, 255)}"),
            SessionId = Guid.NewGuid().ToString()[..8],
            UserAgent = device switch
            {
                "mobile" => "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X)",
                "tablet" => "Mozilla/5.0 (iPad; CPU OS 16_0 like Mac OS X)",
                _ => "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
            }
        };
    }
}
