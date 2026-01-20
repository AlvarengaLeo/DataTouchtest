-- ═══════════════════════════════════════════════════════════════════════════════
-- DataTouch Migration: AddBookingModule
-- Date: 2026-01-13
-- Description: Creates tables for the booking/appointments module
-- ═══════════════════════════════════════════════════════════════════════════════

USE DataTouch;

-- ─────────────────────────────────────────────────────────────────────────────
-- Add TemplateType column to Cards table
-- ─────────────────────────────────────────────────────────────────────────────
ALTER TABLE Cards 
ADD COLUMN IF NOT EXISTS TemplateType VARCHAR(50) NOT NULL DEFAULT 'default';

-- ─────────────────────────────────────────────────────────────────────────────
-- Table: Services
-- Stores the services offered by each card holder
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS Services (
    Id CHAR(36) PRIMARY KEY,
    CardId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Description TEXT NULL,
    DurationMinutes INT NOT NULL DEFAULT 30,
    PriceFrom DECIMAL(10,2) NULL,
    PriceTo DECIMAL(10,2) NULL,
    Currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    
    INDEX IX_Services_CardId (CardId),
    INDEX IX_Services_IsActive (IsActive),
    
    CONSTRAINT FK_Services_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Table: Appointments
-- Stores customer appointments/bookings
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS Appointments (
    Id CHAR(36) PRIMARY KEY,
    CardId CHAR(36) NOT NULL,
    ServiceId CHAR(36) NULL,
    StartDateTime DATETIME NOT NULL,
    EndDateTime DATETIME NOT NULL,
    Status TINYINT NOT NULL DEFAULT 0,  -- 0=Pending, 1=Confirmed, 2=Completed, 3=Cancelled, 4=NoShow
    CustomerName VARCHAR(200) NOT NULL,
    CustomerEmail VARCHAR(254) NOT NULL,
    CustomerPhone VARCHAR(30) NULL,
    CustomerNotes TEXT NULL,
    InternalNotes TEXT NULL,
    Source VARCHAR(50) NOT NULL DEFAULT 'PUBLIC',  -- PUBLIC, CRM, IMPORT
    ReminderSent BOOLEAN NOT NULL DEFAULT FALSE,
    CancellationReason TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    
    INDEX IX_Appointments_CardId (CardId),
    INDEX IX_Appointments_ServiceId (ServiceId),
    INDEX IX_Appointments_StartDateTime (StartDateTime),
    INDEX IX_Appointments_Status (Status),
    INDEX IX_Appointments_CustomerEmail (CustomerEmail),
    
    CONSTRAINT FK_Appointments_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Appointments_Services FOREIGN KEY (ServiceId) 
        REFERENCES Services(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Table: AvailabilityRules
-- Stores weekly recurring availability for each card holder
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS AvailabilityRules (
    Id CHAR(36) PRIMARY KEY,
    CardId CHAR(36) NOT NULL,
    DayOfWeek TINYINT NOT NULL,  -- 0=Sunday, 1=Monday, ..., 6=Saturday
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    
    INDEX IX_AvailabilityRules_CardId (CardId),
    INDEX IX_AvailabilityRules_DayOfWeek (DayOfWeek),
    
    CONSTRAINT FK_AvailabilityRules_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_AvailabilityRules_CardDay UNIQUE (CardId, DayOfWeek)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Table: AvailabilityExceptions
-- Stores exceptions to normal availability (time off, extra hours, holidays)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS AvailabilityExceptions (
    Id CHAR(36) PRIMARY KEY,
    CardId CHAR(36) NOT NULL,
    Date DATE NOT NULL,
    StartTime TIME NULL,  -- NULL means all day
    EndTime TIME NULL,
    ExceptionType TINYINT NOT NULL DEFAULT 0,  -- 0=Unavailable, 1=ExtraHours
    Reason VARCHAR(500) NULL,
    
    INDEX IX_AvailabilityExceptions_CardId (CardId),
    INDEX IX_AvailabilityExceptions_Date (Date),
    
    CONSTRAINT FK_AvailabilityExceptions_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Table: QuoteRequests
-- Stores quote/cotizacion requests from customers
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS QuoteRequests (
    Id CHAR(36) PRIMARY KEY,
    CardId CHAR(36) NOT NULL,
    CustomerName VARCHAR(200) NOT NULL,
    CustomerEmail VARCHAR(254) NOT NULL,
    CustomerPhone VARCHAR(30) NULL,
    Description TEXT NOT NULL,
    Status TINYINT NOT NULL DEFAULT 0,  -- 0=New, 1=InReview, 2=Sent, 3=Accepted, 4=Rejected
    ResponseNotes TEXT NULL,
    QuoteAmount DECIMAL(10,2) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    RespondedAt DATETIME NULL,
    
    INDEX IX_QuoteRequests_CardId (CardId),
    INDEX IX_QuoteRequests_Status (Status),
    INDEX IX_QuoteRequests_CustomerEmail (CustomerEmail),
    
    CONSTRAINT FK_QuoteRequests_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Table: BookingSettings
-- Stores booking configuration per card (slot intervals, buffers, limits)
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS BookingSettings (
    Id CHAR(36) PRIMARY KEY,
    CardId CHAR(36) NOT NULL,
    TimeZoneId VARCHAR(100) NOT NULL DEFAULT 'America/El_Salvador',
    SlotIntervalMinutes INT NOT NULL DEFAULT 30,
    BufferBeforeMinutes INT NOT NULL DEFAULT 0,
    BufferAfterMinutes INT NOT NULL DEFAULT 0,
    MaxAppointmentsPerDay INT NOT NULL DEFAULT 0,
    MinNoticeMinutes INT NOT NULL DEFAULT 0,
    MaxAdvanceDays INT NOT NULL DEFAULT 60,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    
    CONSTRAINT UQ_BookingSettings_CardId UNIQUE (CardId),
    
    CONSTRAINT FK_BookingSettings_Cards FOREIGN KEY (CardId) 
        REFERENCES Cards(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Migration Tracking
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20260113_AddBookingModule', '10.0.2')
ON DUPLICATE KEY UPDATE ProductVersion = '10.0.2';

-- ═══════════════════════════════════════════════════════════════════════════════
-- END OF MIGRATION
-- ═══════════════════════════════════════════════════════════════════════════════
