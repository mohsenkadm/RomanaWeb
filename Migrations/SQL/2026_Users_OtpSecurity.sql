-- =============================================================================
-- Users OTP security: expiry + rate-limit timestamps
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.Users', 'CodeExpiresAt') IS NULL
        ALTER TABLE dbo.Users ADD CodeExpiresAt datetime2 NULL;

    IF COL_LENGTH('dbo.Users', 'LastOtpSentAt') IS NULL
        ALTER TABLE dbo.Users ADD LastOtpSentAt datetime2 NULL;

    IF COL_LENGTH('dbo.Users', 'OtpVerifyFailCount') IS NULL
        ALTER TABLE dbo.Users ADD OtpVerifyFailCount int NOT NULL
            CONSTRAINT DF_Users_OtpVerifyFailCount DEFAULT (0);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
