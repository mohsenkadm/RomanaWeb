-- =============================================================================
-- Per-driver multi-order capacity
-- AllowMultiOrders: driver may hold more than one active delivery at once
-- MaxConcurrentOrders: max active (approved, not done/cancelled) orders when enabled
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.SaleMan', 'AllowMultiOrders') IS NULL
        ALTER TABLE dbo.SaleMan ADD AllowMultiOrders bit NOT NULL
            CONSTRAINT DF_SaleMan_AllowMultiOrders DEFAULT(0);

    IF COL_LENGTH('dbo.SaleMan', 'MaxConcurrentOrders') IS NULL
        ALTER TABLE dbo.SaleMan ADD MaxConcurrentOrders int NOT NULL
            CONSTRAINT DF_SaleMan_MaxConcurrentOrders DEFAULT(1);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
