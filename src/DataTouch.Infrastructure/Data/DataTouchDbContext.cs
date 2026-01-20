using DataTouch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Infrastructure.Data;

public class DataTouchDbContext : DbContext
{
    public DataTouchDbContext(DbContextOptions<DataTouchDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadNote> LeadNotes => Set<LeadNote>();
    
    // NEW - Card Template System
    public DbSet<CardTemplate> CardTemplates => Set<CardTemplate>();
    public DbSet<CardStyle> CardStyles => Set<CardStyle>();
    public DbSet<CardComponent> CardComponents => Set<CardComponent>();
    public DbSet<CardAnalytics> CardAnalytics => Set<CardAnalytics>();
    
    // ═══════════════════════════════════════════════════════════════
    // BOOKING SYSTEM
    // ═══════════════════════════════════════════════════════════════
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<AvailabilityException> AvailabilityExceptions => Set<AvailabilityException>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<BookingSettings> BookingSettings => Set<BookingSettings>();
    
    // CRM / Timeline
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Organization
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Country).HasMaxLength(100);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.OrganizationId, e.Email }).IsUnique();

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Card
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.WhatsAppNumber).HasMaxLength(20);
            entity.Property(e => e.WhatsAppCountryCode).HasMaxLength(10);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.SocialLinksJson).HasColumnType("json");
            entity.Property(e => e.WebsiteLinksJson).HasColumnType("json");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.HasIndex(e => new { e.OrganizationId, e.Slug }).IsUnique();
            entity.HasIndex(e => e.OrganizationId);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Cards)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Cards)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // NEW - Template & Style relationships
            entity.HasOne(e => e.Template)
                .WithMany(t => t.Cards)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Style)
                .WithMany()
                .HasForeignKey(e => e.StyleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Lead
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.PhoneE164).HasMaxLength(20);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.InternalNotes).HasMaxLength(2000);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Card)
                .WithMany(c => c.Leads)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.OwnerUser)
                .WithMany()
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeadNote
        modelBuilder.Entity<LeadNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Notes)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ═══════════════════════════════════════════════════════════════
        // NEW ENTITIES - Card Template System
        // ═══════════════════════════════════════════════════════════════
        
        // CardTemplate
        modelBuilder.Entity<CardTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Industry).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DefaultStyleJson).HasColumnType("json");
            entity.Property(e => e.DefaultComponentsJson).HasColumnType("json");
            entity.HasIndex(e => e.Industry);
            entity.HasIndex(e => e.IsSystemTemplate);
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // CardStyle
        modelBuilder.Entity<CardStyle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PrimaryColor).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TextColor).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BackgroundColor).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BackgroundType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BackgroundValue).HasMaxLength(1000);
            entity.Property(e => e.FontFamily).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HeadingSize).IsRequired().HasMaxLength(20);
            entity.Property(e => e.QrShape).IsRequired().HasMaxLength(20);
            entity.Property(e => e.QrForeground).IsRequired().HasMaxLength(20);
            entity.Property(e => e.QrBackground).IsRequired().HasMaxLength(20);
            entity.Property(e => e.QrLogoUrl).HasMaxLength(500);
            entity.Property(e => e.CardBorderRadius).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CardShadow).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LoadingAnimation).IsRequired().HasMaxLength(20);
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Card)
                .WithOne(c => c.Style)
                .HasForeignKey<CardStyle>(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // CardComponent
        modelBuilder.Entity<CardComponent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConfigJson).HasColumnType("json");
            entity.Property(e => e.DataJson).HasColumnType("json");
            entity.HasIndex(e => e.CardId);
            entity.HasIndex(e => new { e.CardId, e.DisplayOrder });
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.Components)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // CardAnalytics
        modelBuilder.Entity<CardAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Referrer).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.DeviceType).HasMaxLength(20);
            entity.Property(e => e.MetadataJson).HasColumnType("json");
            entity.HasIndex(e => e.CardId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => new { e.CardId, e.Timestamp });
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.Analytics)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ═══════════════════════════════════════════════════════════════
        // BOOKING SYSTEM ENTITIES
        // ═══════════════════════════════════════════════════════════════
        
        // Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.PriceFrom).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.CardId);
            entity.HasIndex(e => new { e.CardId, e.DisplayOrder });
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.Services)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.CustomerPhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.CustomerNotes).HasMaxLength(1000);
            entity.Property(e => e.InternalNotes).HasMaxLength(2000);
            entity.Property(e => e.Timezone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => e.CardId);
            entity.HasIndex(e => new { e.CardId, e.StartDateTime });
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.Appointments)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // AvailabilityRule
        modelBuilder.Entity<AvailabilityRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CardId, e.DayOfWeek }).IsUnique();
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.AvailabilityRules)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // AvailabilityException
        modelBuilder.Entity<AvailabilityException>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExceptionType).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => new { e.CardId, e.ExceptionDate });
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.AvailabilityExceptions)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // QuoteRequest
        modelBuilder.Entity<QuoteRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.CustomerPhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.InternalNotes).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => e.CardId);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Card)
                .WithMany(c => c.QuoteRequests)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ConvertedAppointment)
                .WithMany()
                .HasForeignKey(e => e.ConvertedAppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // BookingSettings
        modelBuilder.Entity<BookingSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeZoneId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.CardId).IsUnique(); // One settings per card
            
            entity.HasOne(e => e.Card)
                .WithMany()
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

