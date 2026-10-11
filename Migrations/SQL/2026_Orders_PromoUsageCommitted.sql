-- =============================================================================
-- Orders: IsPromoUsageCommitted — promo usage is counted only after restaurant
-- approval. Idempotent. Safe to re-run. Targets SQL Server.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.Orders', 'IsPromoUsageCommitted') IS NULL
        ALTER TABLE dbo.Orders ADD IsPromoUsageCommitted bit NOT NULL
            CONSTRAINT DF_Orders_IsPromoUsageCommitted DEFAULT(0);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
