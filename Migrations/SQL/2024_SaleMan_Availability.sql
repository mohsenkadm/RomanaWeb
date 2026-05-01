-- =============================================================================
-- Adds the working/stopped flag to dbo.SaleMan (Section 6).
-- Idempotent. Targets SQL Server.
-- =============================================================================

SET NOCOUNT ON;

IF COL_LENGTH('dbo.SaleMan', 'IsAvailable') IS NULL
BEGIN
    ALTER TABLE dbo.SaleMan
        ADD IsAvailable bit NOT NULL
            CONSTRAINT DF_SaleMan_IsAvailable DEFAULT(1);
END

IF COL_LENGTH('dbo.SaleMan', 'AvailabilityChangedAt') IS NULL
BEGIN
    ALTER TABLE dbo.SaleMan
        ADD AvailabilityChangedAt datetime NULL;
END

PRINT 'SaleMan availability columns ensured.';
GO
