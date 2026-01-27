-- Temporary seed script for Cards, Services, etc.
USE DataTouch;

DECLARE @TechCorpId UNIQUEIDENTIFIER;
DECLARE @AdminUserId UNIQUEIDENTIFIER;
DECLARE @CardId UNIQUEIDENTIFIER = NEWID();
DECLARE @StyleId UNIQUEIDENTIFIER = NEWID();

SELECT @TechCorpId = Id FROM Organizations WHERE Slug = 'techcorp';
SELECT @AdminUserId = Id FROM Users WHERE Email = 'admin@techcorp.com';

PRINT 'TechCorpId: ' + CAST(ISNULL(@TechCorpId, '00000000-0000-0000-0000-000000000000') AS VARCHAR(36));
PRINT 'AdminUserId: ' + CAST(ISNULL(@AdminUserId, '00000000-0000-0000-0000-000000000000') AS VARCHAR(36));

-- Insert Card (with TemplateType which is required)
IF NOT EXISTS (SELECT 1 FROM Cards WHERE Slug = 'leonel-alvarenga')
BEGIN
    INSERT INTO Cards (
        Id, OrganizationId, UserId, Slug, FullName, Title, CompanyName, Bio,
        Phone, PhoneCountryCode, WhatsAppNumber, WhatsAppCountryCode, Email, ProfileImageUrl,
        ShowSaveContact, ShowWhatsApp, ShowCall, ShowEmail, 
        TemplateType, PrimaryCardGoal,
        SocialLinksJson, WebsiteLinksJson, IsActive, CreatedAt
    )
    VALUES (
        @CardId, @TechCorpId, @AdminUserId,
        'leonel-alvarenga', 'Leonel Alvarenga', 'CEO & Founder', 'TechCorp Solutions',
        'Experto en transformacion digital y desarrollo de software empresarial.',
        '7000-0000', '+503', '7000-0000', '+503', 'leonel@techcorp.com',
        'https://ui-avatars.com/api/?name=Leonel+Alvarenga&background=6366F1&color=fff&size=256',
        1, 1, 1, 1,
        'professional', 'booking',
        N'{"linkedin":"https://linkedin.com/in/leonel"}',
        N'[{"title":"Servicios","url":"https://techcorp.com"}]',
        1, GETUTCDATE()
    );
    PRINT 'Card created: leonel-alvarenga with ID ' + CAST(@CardId AS VARCHAR(36));

    -- Insert Services
    INSERT INTO Services (Id, CardId, OrganizationId, Name, Description, CategoryName, DurationMinutes, PriceFrom, ConversionType, DisplayOrder, IsActive, CreatedAt)
    VALUES 
        (NEWID(), @CardId, @TechCorpId, 'Consultoria Estrategica', 'Sesion de consultoria para definir estrategia tecnologica', 'Consultoria', 60, 150.00, 'booking', 1, 1, GETUTCDATE()),
        (NEWID(), @CardId, @TechCorpId, 'Desarrollo de Software', 'Desarrollo de aplicaciones web y moviles a medida', 'Desarrollo', 0, 5000.00, 'quote', 2, 1, GETUTCDATE()),
        (NEWID(), @CardId, @TechCorpId, 'Auditoria de Codigo', 'Revision completa de codigo y arquitectura', 'Consultoria', 120, 300.00, 'booking', 3, 1, GETUTCDATE());
    PRINT 'Services created: 3';

    -- Insert AvailabilityRules
    INSERT INTO AvailabilityRules (Id, CardId, DayOfWeek, StartTime, EndTime, IsActive)
    VALUES 
        (NEWID(), @CardId, 1, '09:00:00', '17:00:00', 1),
        (NEWID(), @CardId, 2, '09:00:00', '17:00:00', 1),
        (NEWID(), @CardId, 3, '09:00:00', '17:00:00', 1),
        (NEWID(), @CardId, 4, '09:00:00', '17:00:00', 1),
        (NEWID(), @CardId, 5, '09:00:00', '17:00:00', 1);
    PRINT 'Availability rules created';

    -- Insert BookingSettings
    INSERT INTO BookingSettings (
        Id, CardId, TimeZoneId, SlotIntervalMinutes, BufferBeforeMinutes, BufferAfterMinutes,
        MaxAppointmentsPerDay, MinNoticeMinutes, MaxAdvanceDays, CreatedAt
    )
    VALUES (
        NEWID(), @CardId, 'America/El_Salvador', 30, 15, 15, 10, 120, 30, GETUTCDATE()
    );
    PRINT 'Booking settings created';

    -- Insert Sample Analytics
    DECLARE @i INT = 1;
    WHILE @i <= 20
    BEGIN
        INSERT INTO CardAnalytics (Id, CardId, EventType, Timestamp, UserAgent, Country, CountryCode, City, DeviceType, SessionId, Channel)
        VALUES (
            NEWID(), @CardId,
            CASE (@i % 4) WHEN 0 THEN 'page_view' WHEN 1 THEN 'qr_scan' WHEN 2 THEN 'cta_click' ELSE 'link_click' END,
            DATEADD(DAY, -(@i % 30), GETUTCDATE()),
            'Mozilla/5.0', 'El Salvador', 'SV', 'San Salvador',
            CASE (@i % 3) WHEN 0 THEN 'mobile' WHEN 1 THEN 'desktop' ELSE 'tablet' END,
            NEWID(), 'qr'
        );
        SET @i = @i + 1;
    END
    PRINT 'Analytics events created: 20';
END
ELSE
BEGIN
    PRINT 'Card already exists';
END

PRINT 'Seed completed!';
