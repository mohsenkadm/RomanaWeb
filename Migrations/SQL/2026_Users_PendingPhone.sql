-- =============================================================================
-- Users pending phone for profile change OTP verification
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.Users', 'PendingPhone') IS NULL
        ALTER TABLE dbo.Users ADD PendingPhone nvarchar(20) NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
