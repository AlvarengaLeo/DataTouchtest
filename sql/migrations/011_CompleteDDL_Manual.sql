-- ═══════════════════════════════════════════════════════════════════════════════
-- DataTouch - Complete DDL for SQL Server
-- Version: 1.0.0 FINAL
-- Description: Complete database schema with all 16 tables and columns
-- All CASCADE conflicts resolved with NO ACTION where needed
-- ═══════════════════════════════════════════════════════════════════════════════

USE DataTouch;
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- 1. Organizations (Base table)
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Organizations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(100) NOT NULL UNIQUE,
    Country NVARCHAR(100) NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 2. Users
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_Users_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_Users_OrgEmail ON Users(OrganizationId, Email);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 3. CardTemplates
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE CardTemplates (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(100) NOT NULL,
    Industry NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    ThumbnailUrl NVARCHAR(500) NOT NULL,
    DefaultStyleJson NVARCHAR(MAX) NOT NULL,
    DefaultComponentsJson NVARCHAR(MAX) NOT NULL,
    IsSystemTemplate BIT NOT NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_CardTemplates_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE CASCADE
);
CREATE INDEX IX_CardTemplates_Industry ON CardTemplates(Industry);
CREATE INDEX IX_CardTemplates_IsSystemTemplate ON CardTemplates(IsSystemTemplate);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 4. Cards
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Cards (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Title NVARCHAR(150) NULL,
    CompanyName NVARCHAR(200) NULL,
    Bio NVARCHAR(1000) NULL,
    Phone NVARCHAR(20) NULL,
    PhoneCountryCode NVARCHAR(10) NULL,
    WhatsAppNumber NVARCHAR(20) NULL,
    WhatsAppCountryCode NVARCHAR(10) NULL,
    Email NVARCHAR(255) NULL,
    ProfileImageUrl NVARCHAR(500) NULL,
    ShowSaveContact BIT NOT NULL,
    ShowWhatsApp BIT NOT NULL,
    ShowCall BIT NOT NULL,
    ShowEmail BIT NOT NULL,
    SocialLinksJson NVARCHAR(MAX) NULL,
    WebsiteLinksJson NVARCHAR(MAX) NULL,
    GalleryImagesJson NVARCHAR(MAX) NULL,
    AppearanceStyleJson NVARCHAR(MAX) NULL,
    TemplateId UNIQUEIDENTIFIER NULL,
    TemplateType NVARCHAR(MAX) NOT NULL,
    PrimaryCardGoal NVARCHAR(MAX) NOT NULL,
    StyleId UNIQUEIDENTIFIER NULL,
    PasswordHash NVARCHAR(500) NULL,
    ActiveFrom DATETIME2 NULL,
    ActiveUntil DATETIME2 NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Cards_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Cards_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Cards_CardTemplates FOREIGN KEY (TemplateId) 
        REFERENCES CardTemplates(Id) ON DELETE SET NULL
);
CREATE UNIQUE INDEX IX_Cards_OrgSlug ON Cards(OrganizationId, Slug);
CREATE INDEX IX_Cards_UserId ON Cards(UserId);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 5. CardStyles
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE CardStyles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NULL,
    CardId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(100) NOT NULL,
    PrimaryColor NVARCHAR(20) NOT NULL,
    SecondaryColor NVARCHAR(20) NOT NULL,
    TextColor NVARCHAR(20) NOT NULL,
    BackgroundColor NVARCHAR(20) NOT NULL,
    BackgroundType NVARCHAR(20) NOT NULL,
    BackgroundValue NVARCHAR(1000) NULL,
    FontFamily NVARCHAR(100) NOT NULL,
    HeadingSize NVARCHAR(20) NOT NULL,
    QrShape NVARCHAR(20) NOT NULL,
    QrForeground NVARCHAR(20) NOT NULL,
    QrBackground NVARCHAR(20) NOT NULL,
    QrLogoUrl NVARCHAR(500) NULL,
    CardBorderRadius NVARCHAR(20) NOT NULL,
    CardShadow NVARCHAR(200) NOT NULL,
    LoadingAnimation NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_CardStyles_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CardStyles_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
);

-- Now add FK from Cards to CardStyles
ALTER TABLE Cards ADD CONSTRAINT FK_Cards_CardStyles 
    FOREIGN KEY (StyleId) REFERENCES CardStyles(Id) ON DELETE NO ACTION;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 6. CardComponents
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE CardComponents (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    DisplayOrder INT NOT NULL,
    ConfigJson NVARCHAR(MAX) NOT NULL,
    DataJson NVARCHAR(MAX) NOT NULL,
    IsVisible BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_CardComponents_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
);
CREATE INDEX IX_CardComponents_CardId_DisplayOrder ON CardComponents(CardId, DisplayOrder);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 7. CardAnalytics
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE CardAnalytics (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    UserAgent NVARCHAR(500) NULL,
    IpHash NVARCHAR(MAX) NULL,
    Referrer NVARCHAR(500) NULL,
    Country NVARCHAR(100) NULL,
    CountryCode NVARCHAR(MAX) NULL,
    Region NVARCHAR(MAX) NULL,
    City NVARCHAR(100) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    GeoSource NVARCHAR(MAX) NULL,
    DeviceType NVARCHAR(20) NULL,
    SessionId NVARCHAR(MAX) NULL,
    Channel NVARCHAR(MAX) NULL,
    MetadataJson NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    CONSTRAINT FK_CardAnalytics_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
);
CREATE INDEX IX_CardAnalytics_CardId_Timestamp ON CardAnalytics(CardId, Timestamp);
CREATE INDEX IX_CardAnalytics_EventType ON CardAnalytics(EventType);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 8. Leads
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Leads (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    CardId UNIQUEIDENTIFIER NOT NULL,
    OwnerUserId UNIQUEIDENTIFIER NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(50) NULL,
    PhoneCountryCode NVARCHAR(10) NULL,
    PhoneE164 NVARCHAR(20) NULL,
    Message NVARCHAR(2000) NULL,
    Source NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    InternalNotes NVARCHAR(2000) NULL,
    NotesUpdatedAt DATETIME2 NULL,
    LastActivityAt DATETIME2 NULL,
    CONSTRAINT FK_Leads_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Leads_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Leads_Users FOREIGN KEY (OwnerUserId) 
        REFERENCES Users(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_Leads_CardId ON Leads(CardId);
CREATE INDEX IX_Leads_Status ON Leads(Status);
CREATE INDEX IX_Leads_CreatedAt ON Leads(CreatedAt);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 9. LeadNotes
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE LeadNotes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    LeadId UNIQUEIDENTIFIER NOT NULL,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_LeadNotes_Leads FOREIGN KEY (LeadId) 
        REFERENCES Leads(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LeadNotes_Users FOREIGN KEY (CreatedByUserId) 
        REFERENCES Users(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_LeadNotes_LeadId ON LeadNotes(LeadId);
CREATE INDEX IX_LeadNotes_CreatedAt ON LeadNotes(CreatedAt);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 10. Services
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Services (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DurationMinutes INT NOT NULL,
    PriceFrom DECIMAL(10,2) NULL,
    CategoryName NVARCHAR(50) NULL,
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL,
    ConversionType NVARCHAR(MAX) NOT NULL,
    Modality NVARCHAR(MAX) NULL,
    BufferBeforeMinutes INT NULL,
    BufferAfterMinutes INT NULL,
    MinNoticeMinutes INT NULL,
    MaxBookingsPerDay INT NULL,
    QuoteFormConfigJson NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_Services_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Services_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_Services_CardId_DisplayOrder ON Services(CardId, DisplayOrder);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 11. Appointments
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Appointments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    ServiceId UNIQUEIDENTIFIER NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    Timezone NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerEmail NVARCHAR(255) NOT NULL,
    CustomerPhone NVARCHAR(50) NULL,
    CustomerPhoneCountryCode NVARCHAR(10) NULL,
    CustomerNotes NVARCHAR(1000) NULL,
    InternalNotes NVARCHAR(2000) NULL,
    Source NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CancelledAt DATETIME2 NULL,
    CancelledByUserId UNIQUEIDENTIFIER NULL,
    CancelReason NVARCHAR(MAX) NULL,
    PreviousStatus INT NULL,
    CONSTRAINT FK_Appointments_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Appointments_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Appointments_Services FOREIGN KEY (ServiceId) 
        REFERENCES Services(Id) ON DELETE SET NULL
);
CREATE INDEX IX_Appointments_CardId_StartDateTime ON Appointments(CardId, StartDateTime);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 12. AvailabilityRules
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE AvailabilityRules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek INT NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BIT NOT NULL,
    CONSTRAINT FK_AvailabilityRules_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_AvailabilityRules_CardId_DayOfWeek ON AvailabilityRules(CardId, DayOfWeek);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 13. AvailabilityExceptions
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE AvailabilityExceptions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    ExceptionDate DATE NOT NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    ExceptionType NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_AvailabilityExceptions_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AvailabilityExceptions_CardId_ExceptionDate ON AvailabilityExceptions(CardId, ExceptionDate);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 14. BookingSettings
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE BookingSettings (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CardId UNIQUEIDENTIFIER NOT NULL,
    TimeZoneId NVARCHAR(100) NOT NULL,
    SlotIntervalMinutes INT NOT NULL,
    BufferBeforeMinutes INT NOT NULL,
    BufferAfterMinutes INT NOT NULL,
    MaxAppointmentsPerDay INT NOT NULL,
    MinNoticeMinutes INT NOT NULL,
    MaxAdvanceDays INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_BookingSettings_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_BookingSettings_CardId ON BookingSettings(CardId);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 15. QuoteRequests
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE QuoteRequests (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    CardId UNIQUEIDENTIFIER NOT NULL,
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    LeadId UNIQUEIDENTIFIER NULL,
    RequestNumber NVARCHAR(MAX) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerEmail NVARCHAR(255) NOT NULL,
    CustomerPhone NVARCHAR(50) NULL,
    CustomerPhoneCountryCode NVARCHAR(10) NULL,
    CustomerCompany NVARCHAR(MAX) NULL,
    Description NVARCHAR(2000) NULL,
    AttachmentsJson NVARCHAR(MAX) NULL,
    CustomFieldsJson NVARCHAR(MAX) NULL,
    Status NVARCHAR(20) NOT NULL,
    StatusReason NVARCHAR(MAX) NULL,
    OwnerId UNIQUEIDENTIFIER NULL,
    Priority INT NOT NULL,
    FirstResponseAt DATETIME2 NULL,
    LastContactAt DATETIME2 NULL,
    SlaDeadlineAt DATETIME2 NULL,
    SlaNotified BIT NOT NULL,
    QuotedAmount DECIMAL(18,2) NULL,
    FinalAmount DECIMAL(18,2) NULL,
    WonAt DATETIME2 NULL,
    LostAt DATETIME2 NULL,
    IdempotencyKey NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(MAX) NULL,
    UserAgent NVARCHAR(MAX) NULL,
    Referrer NVARCHAR(MAX) NULL,
    InternalNotes NVARCHAR(2000) NULL,
    ConvertedAppointmentId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_QuoteRequests_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_QuoteRequests_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE,
    CONSTRAINT FK_QuoteRequests_Services FOREIGN KEY (ServiceId) 
        REFERENCES Services(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_QuoteRequests_Appointments FOREIGN KEY (ConvertedAppointmentId) 
        REFERENCES Appointments(Id) ON DELETE SET NULL,
    CONSTRAINT FK_QuoteRequests_Users FOREIGN KEY (OwnerId) 
        REFERENCES Users(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_QuoteRequests_CardId ON QuoteRequests(CardId);
CREATE INDEX IX_QuoteRequests_Status ON QuoteRequests(Status);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 16. Activities
-- ═══════════════════════════════════════════════════════════════════════════════
CREATE TABLE Activities (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(MAX) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    Type INT NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    MetadataJson NVARCHAR(MAX) NULL,
    UserId UNIQUEIDENTIFIER NULL,
    SystemSource NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_Activities_Organizations FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Activities_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_Activities_OrganizationId ON Activities(OrganizationId);

GO

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT 'Database schema created successfully!';
PRINT '16 tables created with all columns and relationships';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT '';
