-- =============================================================================
-- PromoCode: CreatedBy admin + CreatedAt, and ensure legacy columns exist.
-- Idempotent. Safe to re-run. Targets SQL Server.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    -------------------------------------------------------------------------------
    -- Ensure columns required by the PromoCode entity exist
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.PromoCode', 'DiscountAmount') IS NULL
        ALTER TABLE dbo.PromoCode ADD DiscountAmount decimal(18,2) NOT NULL
            CONSTRAINT DF_PromoCode_DiscountAmount DEFAULT(0);

    IF COL_LENGTH('dbo.PromoCode', 'IsActive') IS NULL
        ALTER TABLE dbo.PromoCode ADD IsActive bit NOT NULL
            CONSTRAINT DF_PromoCode_IsActive DEFAULT(1);

    IF COL_LENGTH('dbo.PromoCode', 'IsForAllStores') IS NULL
        ALTER TABLE dbo.PromoCode ADD IsForAllStores bit NOT NULL
            CONSTRAINT DF_PromoCode_IsForAllStores DEFAULT(0);

    IF COL_LENGTH('dbo.PromoCode', 'MaxOrders') IS NULL
        ALTER TABLE dbo.PromoCode ADD MaxOrders int NOT NULL
            CONSTRAINT DF_PromoCode_MaxOrders DEFAULT(0);

    IF COL_LENGTH('dbo.PromoCode', 'UsedOrders') IS NULL
        ALTER TABLE dbo.PromoCode ADD UsedOrders int NOT NULL
            CONSTRAINT DF_PromoCode_UsedOrders DEFAULT(0);

    IF COL_LENGTH('dbo.PromoCode', 'MaxUsagePerUser') IS NULL
        ALTER TABLE dbo.PromoCode ADD MaxUsagePerUser int NOT NULL
            CONSTRAINT DF_PromoCode_MaxUsagePerUser DEFAULT(1);

    IF COL_LENGTH('dbo.PromoCode', 'DiscountType') IS NULL
        ALTER TABLE dbo.PromoCode ADD DiscountType nvarchar(20) NULL;

    IF COL_LENGTH('dbo.PromoCode', 'MaxDiscountAmount') IS NULL
        ALTER TABLE dbo.PromoCode ADD MaxDiscountAmount decimal(18,2) NOT NULL
            CONSTRAINT DF_PromoCode_MaxDiscountAmount DEFAULT(0);

    IF COL_LENGTH('dbo.PromoCode', 'FirstUsedAt') IS NULL
        ALTER TABLE dbo.PromoCode ADD FirstUsedAt datetime NULL;

    -------------------------------------------------------------------------------
    -- CreatedBy admin tracking
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.PromoCode', 'CreatedByAdminId') IS NULL
        ALTER TABLE dbo.PromoCode ADD CreatedByAdminId int NULL;

    IF COL_LENGTH('dbo.PromoCode', 'CreatedAt') IS NULL
        ALTER TABLE dbo.PromoCode ADD CreatedAt datetime NULL
            CONSTRAINT DF_PromoCode_CreatedAt DEFAULT(GETDATE());

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
