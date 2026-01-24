-- ═══════════════════════════════════════════════════════════════════════════════
-- DataTouch - Seed Data Script for SQL Server
-- Version: 1.0.0
-- Description: Inserts demo/initial data for development and testing
-- ═══════════════════════════════════════════════════════════════════════════════

USE DataTouch;
GO

PRINT 'Starting seed data insertion...';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: Organizations
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @TechCorpId UNIQUEIDENTIFIER = NEWID();
DECLARE @DemoCompanyId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Organizations WHERE Slug = 'techcorp')
BEGIN
    INSERT INTO Organizations (Id, Name, Slug, Country, IsActive, CreatedAt)
    VALUES 
        (@TechCorpId, 'TechCorp Solutions', 'techcorp', 'El Salvador', 1, GETUTCDATE()),
        (@DemoCompanyId, 'Demo Company', 'demo-company', 'El Salvador', 1, GETUTCDATE());
    
    PRINT 'Organizations created: 2';
END
ELSE
BEGIN
    SELECT @TechCorpId = Id FROM Organizations WHERE Slug = 'techcorp';
    SELECT @DemoCompanyId = Id FROM Organizations WHERE Slug = 'demo-company';
    PRINT 'Organizations already exist, using existing IDs';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: Users
-- Password: admin123 (SHA256 hash)
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @TechCorpId UNIQUEIDENTIFIER;
DECLARE @DemoCompanyId UNIQUEIDENTIFIER;
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @DemoUserId UNIQUEIDENTIFIER = NEWID();

SELECT @TechCorpId = Id FROM Organizations WHERE Slug = 'techcorp';
SELECT @DemoCompanyId = Id FROM Organizations WHERE Slug = 'demo-company';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@techcorp.com')
BEGIN
    INSERT INTO Users (Id, OrganizationId, Email, PasswordHash, FullName, Role, IsActive, CreatedAt)
    VALUES 
        (
            @AdminUserId,
            @TechCorpId,
            'admin@techcorp.com',
            'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', -- admin123 (Base64)
            'Leonel Alvarenga',
            'OrgAdmin',
            1,
            GETUTCDATE()
        ),
        (
            @DemoUserId,
            @DemoCompanyId,
            'admin@demo.com',
            'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', -- admin123 (Base64)
            'Admin Demo',
            'OrgAdmin',
            1,
            GETUTCDATE()
        );
    
    PRINT 'Users created: 2';
    PRINT 'Login credentials:';
    PRINT '  - admin@techcorp.com / admin123';
    PRINT '  - admin@demo.com / admin123';
END
ELSE
BEGIN
    SELECT @AdminUserId = Id FROM Users WHERE Email = 'admin@techcorp.com';
    SELECT @DemoUserId = Id FROM Users WHERE Email = 'admin@demo.com';
    PRINT 'Users already exist';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: CardTemplates (System Templates)
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @TemplateModernId UNIQUEIDENTIFIER = NEWID();
DECLARE @TemplateMinimalId UNIQUEIDENTIFIER = NEWID();
DECLARE @TemplateCreativeId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM CardTemplates WHERE Name = 'Modern Professional')
BEGIN
    INSERT INTO CardTemplates (Id, OrganizationId, Name, Industry, Description, ThumbnailUrl, IsSystemTemplate, IsActive, CreatedAt)
    VALUES 
        (
            @TemplateModernId,
            NULL,
            'Modern Professional',
            'Technology',
            'Clean and modern design perfect for tech professionals',
            'https://via.placeholder.com/400x600/6366F1/FFFFFF?text=Modern',
            1,
            1,
            GETUTCDATE()
        ),
        (
            @TemplateMinimalId,
            NULL,
            'Minimal Elegance',
            'Business',
            'Minimalist design for business professionals',
            'https://via.placeholder.com/400x600/1F2937/FFFFFF?text=Minimal',
            1,
            1,
            GETUTCDATE()
        ),
        (
            @TemplateCreativeId,
            NULL,
            'Creative Bold',
            'Creative',
            'Bold and vibrant design for creative professionals',
            'https://via.placeholder.com/400x600/EC4899/FFFFFF?text=Creative',
            1,
            1,
            GETUTCDATE()
        );
    
    PRINT 'Card templates created: 3';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: Cards
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @TechCorpId UNIQUEIDENTIFIER;
DECLARE @AdminUserId UNIQUEIDENTIFIER;
DECLARE @CardId UNIQUEIDENTIFIER = NEWID();
DECLARE @StyleId UNIQUEIDENTIFIER = NEWID();

SELECT @TechCorpId = Id FROM Organizations WHERE Slug = 'techcorp';
SELECT @AdminUserId = Id FROM Users WHERE Email = 'admin@techcorp.com';

IF NOT EXISTS (SELECT 1 FROM Cards WHERE Slug = 'leonel-alvarenga')
BEGIN
    -- Step 1: Create Card WITHOUT StyleId first
    INSERT INTO Cards (
        Id, OrganizationId, UserId, TemplateId, StyleId, Slug, FullName, Title, CompanyName, Bio,
        Phone, PhoneCountryCode, WhatsAppNumber, WhatsAppCountryCode, Email, ProfileImageUrl,
        ShowSaveContact, ShowWhatsApp, ShowCall, ShowEmail, PrimaryCardGoal,
        SocialLinksJson, WebsiteLinksJson, IsActive, CreatedAt
    )
    VALUES (
        @CardId,
        @TechCorpId,
        @AdminUserId,
        NULL,
        NULL,  -- StyleId is NULL initially
        'leonel-alvarenga',
        'Leonel Alvarenga',
        'CEO & Founder',
        'TechCorp Solutions',
        'Experto en transformación digital y desarrollo de software empresarial. Ayudamos a empresas a escalar con tecnología.',
        '7000-0000',
        '+503',
        '7000-0000',
        '+503',
        'leonel@techcorp.com',
        'https://ui-avatars.com/api/?name=Leonel+Alvarenga&background=6366F1&color=fff&size=256',
        1, 1, 1, 1,
        'booking',
        N'{"linkedin":"https://linkedin.com/in/leonel-alvarenga","instagram":"https://instagram.com/techcorp","twitter":"https://twitter.com/techcorp"}',
        N'[{"title":"Nuestros Servicios","url":"https://techcorp.com/services"},{"title":"Agendar Consulta","url":"https://calendly.com/techcorp"}]',
        1,
        GETUTCDATE()
    );
    
    -- Step 2: Create CardStyle (now Card exists)
    INSERT INTO CardStyles (
        Id, OrganizationId, CardId, Name, PrimaryColor, SecondaryColor, TextColor, BackgroundColor,
        BackgroundType, FontFamily, HeadingSize, QrShape, QrForeground, QrBackground,
        CardBorderRadius, CardShadow, LoadingAnimation, CreatedAt
    )
    VALUES (
        @StyleId,
        @TechCorpId,
        @CardId,
        'TechCorp Brand',
        '#6366F1',
        '#EC4899',
        '#1F2937',
        '#FFFFFF',
        'gradient',
        'Inter',
        '1.5rem',
        'square',
        '#000000',
        '#FFFFFF',
        '16px',
        '0 4px 20px rgba(0,0,0,0.1)',
        'fade',
        GETUTCDATE()
    );
    
    -- Step 3: Update Card with StyleId (now CardStyle exists)
    UPDATE Cards
    SET StyleId = @StyleId
    WHERE Id = @CardId;
    
    PRINT 'Card created: leonel-alvarenga';
    PRINT 'Public URL: /p/techcorp/leonel-alvarenga';
END
ELSE
BEGIN
    SELECT @CardId = Id FROM Cards WHERE Slug = 'leonel-alvarenga';
    SELECT @StyleId = StyleId FROM Cards WHERE Slug = 'leonel-alvarenga';
    PRINT 'Card already exists';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: Services
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @TechCorpId UNIQUEIDENTIFIER;
DECLARE @CardId UNIQUEIDENTIFIER;

SELECT @TechCorpId = Id FROM Organizations WHERE Slug = 'techcorp';
SELECT @CardId = Id FROM Cards WHERE Slug = 'leonel-alvarenga';

IF NOT EXISTS (SELECT 1 FROM Services WHERE CardId = @CardId)
BEGIN
    INSERT INTO Services (Id, CardId, OrganizationId, Name, Description, CategoryName, DurationMinutes, PriceFrom, ConversionType, DisplayOrder, IsActive, CreatedAt)
    VALUES 
        (
            NEWID(),
            @CardId,
            @TechCorpId,
            'Consultoría Estratégica',
            'Sesión de consultoría para definir estrategia tecnológica',
            'Consultoría',
            60,
            150.00,
            'booking',
            1,
            1,
            GETUTCDATE()
        ),
        (
            NEWID(),
            @CardId,
            @TechCorpId,
            'Desarrollo de Software',
            'Desarrollo de aplicaciones web y móviles a medida',
            'Desarrollo',
            0,  -- 0 minutes for quote-based services (no fixed duration)
            5000.00,
            'quote',
            2,
            1,
            GETUTCDATE()
        ),
        (
            NEWID(),
            @CardId,
            @TechCorpId,
            'Auditoría de Código',
            'Revisión completa de código y arquitectura',
            'Consultoría',
            120,
            300.00,
            'booking',
            3,
            1,
            GETUTCDATE()
        );
    
    PRINT 'Services created: 3';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: AvailabilityRules (Monday to Friday, 9 AM - 5 PM)
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @CardId UNIQUEIDENTIFIER;
SELECT @CardId = Id FROM Cards WHERE Slug = 'leonel-alvarenga';

IF NOT EXISTS (SELECT 1 FROM AvailabilityRules WHERE CardId = @CardId)
BEGIN
    INSERT INTO AvailabilityRules (Id, CardId, DayOfWeek, StartTime, EndTime, IsAvailable, CreatedAt)
    VALUES 
        (NEWID(), @CardId, 1, '09:00:00', '17:00:00', 1, GETUTCDATE()), -- Monday
        (NEWID(), @CardId, 2, '09:00:00', '17:00:00', 1, GETUTCDATE()), -- Tuesday
        (NEWID(), @CardId, 3, '09:00:00', '17:00:00', 1, GETUTCDATE()), -- Wednesday
        (NEWID(), @CardId, 4, '09:00:00', '17:00:00', 1, GETUTCDATE()), -- Thursday
        (NEWID(), @CardId, 5, '09:00:00', '17:00:00', 1, GETUTCDATE()); -- Friday
    
    PRINT 'Availability rules created: Monday-Friday 9AM-5PM';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: BookingSettings
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @CardId UNIQUEIDENTIFIER;
SELECT @CardId = Id FROM Cards WHERE Slug = 'leonel-alvarenga';

IF NOT EXISTS (SELECT 1 FROM BookingSettings WHERE CardId = @CardId)
BEGIN
    INSERT INTO BookingSettings (
        Id, CardId, TimeZoneId, SlotDurationMinutes, BufferMinutes,
        MinAdvanceBookingHours, MaxAdvanceBookingDays, AllowWeekendBooking,
        RequireApproval, CreatedAt
    )
    VALUES (
        NEWID(),
        @CardId,
        'America/El_Salvador',
        30,
        15,
        2,
        30,
        0,
        0,
        GETUTCDATE()
    );
    
    PRINT 'Booking settings created';
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- SEED: Sample Analytics Events
-- ═══════════════════════════════════════════════════════════════════════════════

DECLARE @CardId UNIQUEIDENTIFIER;
SELECT @CardId = Id FROM Cards WHERE Slug = 'leonel-alvarenga';

IF NOT EXISTS (SELECT 1 FROM CardAnalytics WHERE CardId = @CardId)
BEGIN
    DECLARE @Counter INT = 1;
    WHILE @Counter <= 50
    BEGIN
        INSERT INTO CardAnalytics (
            Id, CardId, EventType, Timestamp, UserAgent, Country, CountryCode,
            City, DeviceType, SessionId, Channel
        )
        VALUES (
            NEWID(),
            @CardId,
            CASE (@Counter % 4)
                WHEN 0 THEN 'page_view'
                WHEN 1 THEN 'qr_scan'
                WHEN 2 THEN 'cta_click'
                ELSE 'link_click'
            END,
            DATEADD(DAY, -(@Counter % 30), GETUTCDATE()),
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64)',
            'El Salvador',
            'SV',
            'San Salvador',
            CASE (@Counter % 3)
                WHEN 0 THEN 'mobile'
                WHEN 1 THEN 'desktop'
                ELSE 'tablet'
            END,
            NEWID(),
            'qr'
        );
        
        SET @Counter = @Counter + 1;
    END
    
    PRINT 'Sample analytics events created: 50';
END
GO

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT 'Seed data insertion completed successfully!';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'Demo Credentials:';
PRINT '  Email: admin@techcorp.com';
PRINT '  Password: admin123';
PRINT '';
PRINT 'Public Card URL: /p/techcorp/leonel-alvarenga';
PRINT '';
GO
