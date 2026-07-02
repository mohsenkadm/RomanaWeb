-- =============================================================================
-- Rumana Platform - Feature upgrade (OTP login, order statuses, products, splash)
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    -- PromoCode: per-user usage limit
    IF COL_LENGTH('dbo.PromoCode', 'MaxUsagePerUser') IS NULL
        ALTER TABLE dbo.PromoCode ADD MaxUsagePerUser int NOT NULL
            CONSTRAINT DF_PromoCode_MaxUsagePerUser DEFAULT(1);

    -- Track promo usage per user
    IF OBJECT_ID('dbo.PromoCodeUsage', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PromoCodeUsage (
            PromoCodeUsageId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            PromoCodeId      int NOT NULL,
            UserId           int NOT NULL,
            UsedCount        int NOT NULL CONSTRAINT DF_PromoCodeUsage_UsedCount DEFAULT(0),
            CONSTRAINT UQ_PromoCodeUsage UNIQUE (PromoCodeId, UserId)
        );
        CREATE INDEX IX_PromoCodeUsage_User ON dbo.PromoCodeUsage(UserId);
    END

    -- Orders: extended delivery workflow flags
    IF COL_LENGTH('dbo.Orders', 'IsPreparing') IS NULL
        ALTER TABLE dbo.Orders ADD IsPreparing bit NOT NULL CONSTRAINT DF_Orders_IsPreparing DEFAULT(0);
    IF COL_LENGTH('dbo.Orders', 'IsDriverEnRouteToPickup') IS NULL
        ALTER TABLE dbo.Orders ADD IsDriverEnRouteToPickup bit NOT NULL CONSTRAINT DF_Orders_IsDriverEnRouteToPickup DEFAULT(0);
    IF COL_LENGTH('dbo.Orders', 'IsPickedUpFromRestaurant') IS NULL
        ALTER TABLE dbo.Orders ADD IsPickedUpFromRestaurant bit NOT NULL CONSTRAINT DF_Orders_IsPickedUpFromRestaurant DEFAULT(0);
    IF COL_LENGTH('dbo.Orders', 'IsOutForDelivery') IS NULL
        ALTER TABLE dbo.Orders ADD IsOutForDelivery bit NOT NULL CONSTRAINT DF_Orders_IsOutForDelivery DEFAULT(0);
    IF COL_LENGTH('dbo.Orders', 'IsDeliveryConfirmed') IS NULL
        ALTER TABLE dbo.Orders ADD IsDeliveryConfirmed bit NOT NULL CONSTRAINT DF_Orders_IsDeliveryConfirmed DEFAULT(0);

    -- Products: preparation time + availability
    IF COL_LENGTH('dbo.Products', 'PreparationTimeMinutes') IS NULL
        ALTER TABLE dbo.Products ADD PreparationTimeMinutes int NOT NULL CONSTRAINT DF_Products_PreparationTimeMinutes DEFAULT(15);
    IF COL_LENGTH('dbo.Products', 'IsAvailable') IS NULL
        ALTER TABLE dbo.Products ADD IsAvailable bit NOT NULL CONSTRAINT DF_Products_IsAvailable DEFAULT(1);

    -- App splash screen (single row enforced in application layer)
    IF OBJECT_ID('dbo.AppSplash', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.AppSplash (
            AppSplashId   int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            ImageUrl      nvarchar(500) NOT NULL,
            Details       nvarchar(1000) NULL,
            IsVisible     bit NOT NULL CONSTRAINT DF_AppSplash_IsVisible DEFAULT(0),
            UpdatedAt     datetime NOT NULL CONSTRAINT DF_AppSplash_UpdatedAt DEFAULT(GETDATE())
        );
    END

    -- Carousel: target app (1=customer, 2=restaurant, 3=delivery)
    IF COL_LENGTH('dbo.Carousel', 'AppType') IS NULL
        ALTER TABLE dbo.Carousel ADD AppType int NOT NULL
            CONSTRAINT DF_Carousel_AppType DEFAULT(1);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
