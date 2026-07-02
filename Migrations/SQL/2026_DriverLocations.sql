-- =============================================================================
-- Rumana Platform - Driver live location tracking
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF OBJECT_ID('dbo.DriverLocations', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DriverLocations (
            SaleManId       int NOT NULL PRIMARY KEY,
            Lat             float NOT NULL,
            Lng             float NOT NULL,
            UpdatedAt       datetime2 NOT NULL CONSTRAINT DF_DriverLocations_UpdatedAt DEFAULT(SYSUTCDATETIME()),
            ActiveOrderId   int NULL
        );
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
