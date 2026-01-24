-- ═══════════════════════════════════════════════════════════════════════════════
-- DataTouch - SQL Server DDL Script (CORRECTED)
-- Version: 1.0.1
-- Fixes: Cascade path conflicts and filtered index syntax
-- ═══════════════════════════════════════════════════════════════════════════════

USE master;
GO

-- Create DataTouch database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DataTouch')
BEGIN
    CREATE DATABASE DataTouch
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Created database: DataTouch';
END
ELSE
BEGIN
    PRINT 'Database DataTouch already exists';
END
GO

USE DataTouch;
GO

PRINT 'Starting DataTouch schema creation...';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- Drop existing tables if they exist (for clean reinstall)
-- ═══════════════════════════════════════════════════════════════════════════════

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Activities') DROP TABLE Activities;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CardAnalytics') DROP TABLE CardAnalytics;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LeadNotes') DROP TABLE LeadNotes;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Leads') DROP TABLE Leads;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BookingSettings') DROP TABLE BookingSettings;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuoteRequests') DROP TABLE QuoteRequests;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvailabilityExceptions') DROP TABLE AvailabilityExceptions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvailabilityRules') DROP TABLE AvailabilityRules;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Appointments') DROP TABLE Appointments;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Services') DROP TABLE Services;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CardComponents') DROP TABLE CardComponents;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CardStyles') DROP TABLE CardStyles;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Cards') DROP TABLE Cards;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CardTemplates') DROP TABLE CardTemplates;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users') DROP TABLE Users;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Organizations') DROP TABLE Organizations;
GO

PRINT 'Dropped existing tables (if any)';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Organizations
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Organizations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_Organizations_Slug UNIQUE (Slug)
);

CREATE INDEX IX_Organizations_Slug ON Organizations(Slug);
CREATE INDEX IX_Organizations_IsActive ON Organizations(IsActive);
GO

PRINT 'Created table: Organizations';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Users
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'OrgUser',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Users_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT UQ_Users_OrgId_Email UNIQUE (OrganizationId, Email)
);

CREATE INDEX IX_Users_OrganizationId ON Users(OrganizationId);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
GO

PRINT 'Created table: Users';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: CardTemplates
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE CardTemplates (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(100) NOT NULL,
    Industry NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    ThumbnailUrl NVARCHAR(500) NOT NULL,
    DefaultStyleJson NVARCHAR(MAX) NULL,
    DefaultComponentsJson NVARCHAR(MAX) NULL,
    IsSystemTemplate BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_CardTemplates_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE NO ACTION  -- Changed from CASCADE to avoid multiple cascade paths
);

CREATE INDEX IX_CardTemplates_Industry ON CardTemplates(Industry);
CREATE INDEX IX_CardTemplates_IsSystemTemplate ON CardTemplates(IsSystemTemplate);
CREATE INDEX IX_CardTemplates_IsActive ON CardTemplates(IsActive);
GO

PRINT 'Created table: CardTemplates';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Cards
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Cards (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TemplateId UNIQUEIDENTIFIER NULL,
    StyleId UNIQUEIDENTIFIER NULL,
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
    ShowSaveContact BIT NOT NULL DEFAULT 1,
    ShowWhatsApp BIT NOT NULL DEFAULT 1,
    ShowCall BIT NOT NULL DEFAULT 1,
    ShowEmail BIT NOT NULL DEFAULT 1,
    SocialLinksJson NVARCHAR(MAX) NULL,
    WebsiteLinksJson NVARCHAR(MAX) NULL,
    PasswordHash NVARCHAR(500) NULL,
    ActiveFrom DATETIME2 NULL,
    ActiveUntil DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    PrimaryCardGoal NVARCHAR(20) NULL,
    
    CONSTRAINT FK_Cards_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_Cards_Users 
        FOREIGN KEY (UserId) 
        REFERENCES Users(Id) 
        ON DELETE NO ACTION,
    
    CONSTRAINT FK_Cards_Templates 
        FOREIGN KEY (TemplateId) 
        REFERENCES CardTemplates(Id) 
        ON DELETE NO ACTION,  -- Changed from SET NULL to avoid cascade conflicts
    
    CONSTRAINT UQ_Cards_OrgId_Slug UNIQUE (OrganizationId, Slug)
);

CREATE INDEX IX_Cards_OrganizationId ON Cards(OrganizationId);
CREATE INDEX IX_Cards_UserId ON Cards(UserId);
CREATE INDEX IX_Cards_Slug ON Cards(Slug);
CREATE INDEX IX_Cards_IsActive ON Cards(IsActive);
GO

PRINT 'Created table: Cards';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: CardStyles
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE CardStyles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NULL,
    CardId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(100) NOT NULL DEFAULT 'Custom',
    PrimaryColor NVARCHAR(20) NOT NULL DEFAULT '#6366F1',
    SecondaryColor NVARCHAR(20) NOT NULL DEFAULT '#EC4899',
    TextColor NVARCHAR(20) NOT NULL DEFAULT '#1F2937',
    BackgroundColor NVARCHAR(20) NOT NULL DEFAULT '#FFFFFF',
    BackgroundType NVARCHAR(20) NOT NULL DEFAULT 'gradient',
    BackgroundValue NVARCHAR(1000) NULL,
    FontFamily NVARCHAR(100) NOT NULL DEFAULT 'Inter',
    HeadingSize NVARCHAR(20) NOT NULL DEFAULT '1.5rem',
    QrShape NVARCHAR(20) NOT NULL DEFAULT 'square',
    QrForeground NVARCHAR(20) NOT NULL DEFAULT '#000000',
    QrBackground NVARCHAR(20) NOT NULL DEFAULT '#FFFFFF',
    QrLogoUrl NVARCHAR(500) NULL,
    CardBorderRadius NVARCHAR(20) NOT NULL DEFAULT '16px',
    CardShadow NVARCHAR(200) NOT NULL DEFAULT '0 4px 20px rgba(0,0,0,0.1)',
    LoadingAnimation NVARCHAR(20) NOT NULL DEFAULT 'fade',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_CardStyles_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE NO ACTION,  -- Changed from CASCADE
    
    CONSTRAINT FK_CardStyles_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE NO ACTION  -- Changed from CASCADE to avoid circular reference with Cards.StyleId
);

CREATE INDEX IX_CardStyles_CardId ON CardStyles(CardId);
GO

-- Add StyleId FK to Cards
ALTER TABLE Cards
ADD CONSTRAINT FK_Cards_Styles 
    FOREIGN KEY (StyleId) 
    REFERENCES CardStyles(Id) 
    ON DELETE NO ACTION;  -- Changed from SET NULL
GO

PRINT 'Created table: CardStyles';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: CardComponents
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE CardComponents (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    ConfigJson NVARCHAR(MAX) NULL,
    DataJson NVARCHAR(MAX) NULL,
    IsVisible BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_CardComponents_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IX_CardComponents_CardId ON CardComponents(CardId);
CREATE INDEX IX_CardComponents_CardId_Order ON CardComponents(CardId, DisplayOrder);
GO

PRINT 'Created table: CardComponents';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Services
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Services (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CategoryName NVARCHAR(50) NULL,
    DurationMinutes INT NOT NULL DEFAULT 30,
    PriceFrom DECIMAL(10,2) NULL,
    ConversionType NVARCHAR(20) NOT NULL DEFAULT 'booking',
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Services_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_Services_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE NO ACTION
);

CREATE INDEX IX_Services_CardId ON Services(CardId);
CREATE INDEX IX_Services_CardId_Order ON Services(CardId, DisplayOrder);
CREATE INDEX IX_Services_IsActive ON Services(IsActive);
GO

PRINT 'Created table: Services';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Appointments
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Appointments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    ServiceId UNIQUEIDENTIFIER NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerEmail NVARCHAR(255) NOT NULL,
    CustomerPhone NVARCHAR(50) NULL,
    CustomerPhoneCountryCode NVARCHAR(10) NULL,
    CustomerNotes NVARCHAR(1000) NULL,
    InternalNotes NVARCHAR(2000) NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    Timezone NVARCHAR(50) NOT NULL DEFAULT 'America/El_Salvador',
    Source NVARCHAR(50) NOT NULL DEFAULT 'PUBLIC_BOOKING',
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Appointments_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_Appointments_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE NO ACTION,
    
    CONSTRAINT FK_Appointments_Services 
        FOREIGN KEY (ServiceId) 
        REFERENCES Services(Id) 
        ON DELETE NO ACTION  -- Changed from SET NULL
);

CREATE INDEX IX_Appointments_CardId ON Appointments(CardId);
CREATE INDEX IX_Appointments_CardId_StartDateTime ON Appointments(CardId, StartDateTime);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);
CREATE INDEX IX_Appointments_StartDateTime ON Appointments(StartDateTime);
GO

PRINT 'Created table: Appointments';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: AvailabilityRules
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE AvailabilityRules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek INT NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_AvailabilityRules_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT UQ_AvailabilityRules_CardId_DayOfWeek UNIQUE (CardId, DayOfWeek)
);

CREATE INDEX IX_AvailabilityRules_CardId ON AvailabilityRules(CardId);
GO

PRINT 'Created table: AvailabilityRules';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: AvailabilityExceptions
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE AvailabilityExceptions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    ExceptionDate DATE NOT NULL,
    ExceptionType NVARCHAR(20) NOT NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    Reason NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_AvailabilityExceptions_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IX_AvailabilityExceptions_CardId ON AvailabilityExceptions(CardId);
CREATE INDEX IX_AvailabilityExceptions_CardId_Date ON AvailabilityExceptions(CardId, ExceptionDate);
GO

PRINT 'Created table: AvailabilityExceptions';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: QuoteRequests
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE QuoteRequests (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerEmail NVARCHAR(255) NOT NULL,
    CustomerPhone NVARCHAR(50) NULL,
    CustomerPhoneCountryCode NVARCHAR(10) NULL,
    Description NVARCHAR(2000) NULL,
    InternalNotes NVARCHAR(2000) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'New',
    ConvertedAppointmentId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_QuoteRequests_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_QuoteRequests_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE NO ACTION,
    
    CONSTRAINT FK_QuoteRequests_Appointments 
        FOREIGN KEY (ConvertedAppointmentId) 
        REFERENCES Appointments(Id) 
        ON DELETE NO ACTION  -- Changed from SET NULL
);

CREATE INDEX IX_QuoteRequests_CardId ON QuoteRequests(CardId);
CREATE INDEX IX_QuoteRequests_Status ON QuoteRequests(Status);
CREATE INDEX IX_QuoteRequests_CreatedAt ON QuoteRequests(CreatedAt);
GO

PRINT 'Created table: QuoteRequests';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: BookingSettings
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE BookingSettings (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    TimeZoneId NVARCHAR(100) NOT NULL DEFAULT 'America/El_Salvador',
    SlotDurationMinutes INT NOT NULL DEFAULT 30,
    BufferMinutes INT NOT NULL DEFAULT 0,
    MinAdvanceBookingHours INT NOT NULL DEFAULT 2,
    MaxAdvanceBookingDays INT NOT NULL DEFAULT 30,
    AllowWeekendBooking BIT NOT NULL DEFAULT 1,
    RequireApproval BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_BookingSettings_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT UQ_BookingSettings_CardId UNIQUE (CardId)
);

CREATE INDEX IX_BookingSettings_CardId ON BookingSettings(CardId);
GO

PRINT 'Created table: BookingSettings';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Leads
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Leads (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    CardId UNIQUEIDENTIFIER NOT NULL,
    OwnerUserId UNIQUEIDENTIFIER NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(50) NULL,
    PhoneCountryCode NVARCHAR(10) NULL,
    PhoneE164 NVARCHAR(20) NULL,
    Message NVARCHAR(2000) NULL,
    Source NVARCHAR(50) NOT NULL DEFAULT 'CARD_CONTACT_FORM',
    Status NVARCHAR(50) NOT NULL DEFAULT 'New',
    InternalNotes NVARCHAR(2000) NULL,
    NotesUpdatedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Leads_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_Leads_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE NO ACTION,
    
    CONSTRAINT FK_Leads_Users 
        FOREIGN KEY (OwnerUserId) 
        REFERENCES Users(Id) 
        ON DELETE NO ACTION
);

CREATE INDEX IX_Leads_OrganizationId ON Leads(OrganizationId);
CREATE INDEX IX_Leads_CardId ON Leads(CardId);
CREATE INDEX IX_Leads_Status ON Leads(Status);
CREATE INDEX IX_Leads_CreatedAt ON Leads(CreatedAt);
GO

PRINT 'Created table: Leads';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: LeadNotes
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE LeadNotes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    LeadId UNIQUEIDENTIFIER NOT NULL,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_LeadNotes_Leads 
        FOREIGN KEY (LeadId) 
        REFERENCES Leads(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_LeadNotes_Users 
        FOREIGN KEY (CreatedByUserId) 
        REFERENCES Users(Id) 
        ON DELETE NO ACTION
);

CREATE INDEX IX_LeadNotes_LeadId ON LeadNotes(LeadId);
CREATE INDEX IX_LeadNotes_CreatedAt ON LeadNotes(CreatedAt);
GO

PRINT 'Created table: LeadNotes';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: CardAnalytics
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE CardAnalytics (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UserAgent NVARCHAR(500) NULL,
    IpHash NVARCHAR(50) NULL,
    Referrer NVARCHAR(500) NULL,
    Country NVARCHAR(100) NULL,
    CountryCode NVARCHAR(10) NULL,
    Region NVARCHAR(100) NULL,
    City NVARCHAR(100) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    GeoSource NVARCHAR(20) NULL,
    DeviceType NVARCHAR(20) NULL,
    SessionId NVARCHAR(50) NULL,
    Channel NVARCHAR(50) NULL,
    MetadataJson NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    
    CONSTRAINT FK_CardAnalytics_Cards 
        FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IX_CardAnalytics_CardId ON CardAnalytics(CardId);
CREATE INDEX IX_CardAnalytics_Timestamp ON CardAnalytics(Timestamp);
CREATE INDEX IX_CardAnalytics_EventType ON CardAnalytics(EventType);
CREATE INDEX IX_CardAnalytics_CardId_Timestamp ON CardAnalytics(CardId, Timestamp);
GO

PRINT 'Created table: CardAnalytics';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- TABLE: Activities
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE Activities (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    ActivityType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    PerformedByUserId UNIQUEIDENTIFIER NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    MetadataJson NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_Activities_Organizations 
        FOREIGN KEY (OrganizationId) 
        REFERENCES Organizations(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IX_Activities_OrganizationId ON Activities(OrganizationId);
CREATE INDEX IX_Activities_EntityType_EntityId ON Activities(EntityType, EntityId);
CREATE INDEX IX_Activities_Timestamp ON Activities(Timestamp);
GO

PRINT 'Created table: Activities';
GO

-- ═══════════════════════════════════════════════════════════════════════════════
-- Summary
-- ═══════════════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT 'DataTouch database schema created successfully!';
PRINT 'Total tables created: 16';
PRINT '';
PRINT 'Using database: DataTouch';
PRINT '═══════════════════════════════════════════════════════════════';
GO
