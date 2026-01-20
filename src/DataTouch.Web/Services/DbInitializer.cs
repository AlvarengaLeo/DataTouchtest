using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

public static class DbInitializer
{
    // Flag to enable demo seed data (set via environment or config)
    public static bool UseDemoSeed { get; set; } = true;

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
            },
            new Service
            {
                Id = Guid.NewGuid(),
                CardId = activeCard.Id,
                OrganizationId = organization.Id,
                Name = "Análisis de Proyectos",
                Description = "Evaluación detallada de viabilidad",
                DurationMinutes = 90,
                PriceFrom = 75m,
                DisplayOrder = 1,
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

        // Create sample leads with varied dates
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

        // Generate ~60 page views over 7 days
        for (int i = 0; i < 60; i++)
        {
            var daysAgo = random.Next(0, 7);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            events.Add(CreateEvent(cardId, "page_view", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random));
        }

        // Generate ~15 QR scans
        for (int i = 0; i < 15; i++)
        {
            var daysAgo = random.Next(0, 7);
            var hoursAgo = random.Next(0, 23);
            var loc = locations[random.Next(locations.Length)];
            
            events.Add(CreateEvent(cardId, "qr_scan", now.AddDays(-daysAgo).AddHours(-hoursAgo), loc.City, loc.Country, loc.CC, loc.Lat, loc.Lng, random));
        }

        // Generate link clicks with specific types for Top Links widget
        var linkTypes = new[] { "whatsapp", "email", "linkedin", "portfolio", "website", "instagram" };
        var clickCounts = new[] { 35, 12, 8, 4, 3, 2 }; // WhatsApp most clicked

        for (int linkIndex = 0; linkIndex < linkTypes.Length; linkIndex++)
        {
            for (int i = 0; i < clickCounts[linkIndex]; i++)
            {
                var daysAgo = random.Next(0, 7);
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
