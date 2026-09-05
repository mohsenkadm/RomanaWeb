-- =============================================================================
-- Restaurant display sort order (admin-controlled sequence)
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.Restaurant', 'SortOrder') IS NULL
        ALTER TABLE dbo.Restaurant ADD SortOrder int NOT NULL
            CONSTRAINT DF_Restaurant_SortOrder DEFAULT(0);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
