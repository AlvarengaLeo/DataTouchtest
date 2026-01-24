-- ═══════════════════════════════════════════════════════════════════════════════
-- DataTouch - TRUNCATE All Tables Script
-- Version: 1.0.0
-- Description: Cleans all data from tables (for development/testing only)
-- WARNING: This will delete ALL data from the database!
-- ═══════════════════════════════════════════════════════════════════════════════

USE DataTouch;
GO

PRINT 'WARNING: This will delete ALL data from DataTouch database!';
PRINT 'Starting table cleanup...';
GO

-- Disable all foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Delete all data from tables in reverse dependency order
DELETE FROM Activities;
DELETE FROM CardAnalytics;
DELETE FROM LeadNotes;
DELETE FROM Leads;
DELETE FROM BookingSettings;
DELETE FROM QuoteRequests;
DELETE FROM AvailabilityExceptions;
DELETE FROM AvailabilityRules;
DELETE FROM Appointments;
DELETE FROM Services;
DELETE FROM CardComponents;
DELETE FROM CardStyles;
DELETE FROM Cards;
DELETE FROM CardTemplates;
DELETE FROM Users;
DELETE FROM Organizations;
GO

-- Re-enable all foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO

-- Reset identity seeds (if any tables use IDENTITY)
-- DBCC CHECKIDENT ('TableName', RESEED, 0);
GO

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT 'All tables cleaned successfully!';
PRINT 'Database is now empty and ready for fresh seed data.';
PRINT '═══════════════════════════════════════════════════════════════';
GO
