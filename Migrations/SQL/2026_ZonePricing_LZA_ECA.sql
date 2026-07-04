-- Rumana — Zone LZA/ECA pricing + related tables
-- Idempotent SQL Server migration

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.Zone', 'BaseDeliveryPrice') IS NULL
        ALTER TABLE dbo.Zone ADD BaseDeliveryPrice decimal(18,0) NULL;

    IF COL_LENGTH('dbo.Zone', 'LzaKm') IS NULL
        ALTER TABLE dbo.Zone ADD LzaKm decimal(6,2) NOT NULL
            CONSTRAINT DF_Zone_LzaKm DEFAULT(3);

    IF COL_LENGTH('dbo.Zone', 'EcaPricePerKm') IS NULL
        ALTER TABLE dbo.Zone ADD EcaPricePerKm decimal(18,0) NOT NULL
            CONSTRAINT DF_Zone_EcaPricePerKm DEFAULT(250);

    IF COL_LENGTH('dbo.Zone', 'MaxEcaFee') IS NULL
        ALTER TABLE dbo.Zone ADD MaxEcaFee decimal(18,0) NOT NULL
            CONSTRAINT DF_Zone_MaxEcaFee DEFAULT(2500);

    IF COL_LENGTH('dbo.Zone', 'NearRestaurantPrice') IS NULL
        ALTER TABLE dbo.Zone ADD NearRestaurantPrice decimal(18,0) NULL;

    IF COL_LENGTH('dbo.Zone', 'NearRestaurantKm') IS NULL
        ALTER TABLE dbo.Zone ADD NearRestaurantKm decimal(4,2) NOT NULL
            CONSTRAINT DF_Zone_NearRestaurantKm DEFAULT(1);

    IF COL_LENGTH('dbo.Zone', 'MaxTotalDeliveryFee') IS NULL
        ALTER TABLE dbo.Zone ADD MaxTotalDeliveryFee decimal(18,0) NULL;

    IF COL_LENGTH('dbo.Orders', 'PricingSource') IS NULL
        ALTER TABLE dbo.Orders ADD PricingSource nvarchar(30) NULL;

    IF COL_LENGTH('dbo.Orders', 'PricingFromZone') IS NULL
        ALTER TABLE dbo.Orders ADD PricingFromZone nvarchar(200) NULL;

    IF COL_LENGTH('dbo.Orders', 'PricingToZone') IS NULL
        ALTER TABLE dbo.Orders ADD PricingToZone nvarchar(200) NULL;

    IF COL_LENGTH('dbo.Orders', 'RouteDistanceKm') IS NULL
        ALTER TABLE dbo.Orders ADD RouteDistanceKm decimal(8,2) NULL;

    IF COL_LENGTH('dbo.Orders', 'PricingZoneFee') IS NULL
        ALTER TABLE dbo.Orders ADD PricingZoneFee decimal(18,0) NULL;

    IF COL_LENGTH('dbo.Orders', 'PricingEcaFee') IS NULL
        ALTER TABLE dbo.Orders ADD PricingEcaFee decimal(18,0) NULL;

    IF OBJECT_ID('dbo.RestaurantZone', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.RestaurantZone (
            RestaurantZoneId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            RestaurantId int NOT NULL,
            ZoneId int NOT NULL,
            CONSTRAINT UQ_RestaurantZone UNIQUE (RestaurantId, ZoneId)
        );
        CREATE INDEX IX_RestaurantZone_RestaurantId ON dbo.RestaurantZone(RestaurantId);
        CREATE INDEX IX_RestaurantZone_ZoneId ON dbo.RestaurantZone(ZoneId);
    END

    IF OBJECT_ID('dbo.SaleManZone', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SaleManZone (
            SaleManZoneId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            SaleManId int NOT NULL,
            ZoneId int NOT NULL,
            CONSTRAINT UQ_SaleManZone UNIQUE (SaleManId, ZoneId)
        );
        CREATE INDEX IX_SaleManZone_SaleManId ON dbo.SaleManZone(SaleManId);
        CREATE INDEX IX_SaleManZone_ZoneId ON dbo.SaleManZone(ZoneId);
    END

    IF OBJECT_ID('dbo.ServiceCoverageRequest', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ServiceCoverageRequest (
            ServiceCoverageRequestId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            Name nvarchar(200) NOT NULL,
            Phone nvarchar(20) NOT NULL,
            Address nvarchar(500) NULL,
            Lat float NOT NULL,
            Lng float NOT NULL,
            CreatedAt datetime NOT NULL CONSTRAINT DF_ServiceCoverageRequest_CreatedAt DEFAULT(GETDATE()),
            IsProcessed bit NOT NULL CONSTRAINT DF_ServiceCoverageRequest_IsProcessed DEFAULT(0)
        );
    END

    IF COL_LENGTH('dbo.AppSettings', 'AllowBusyDriverDispatch') IS NULL
        ALTER TABLE dbo.AppSettings ADD AllowBusyDriverDispatch bit NOT NULL
            CONSTRAINT DF_AppSettings_AllowBusyDriverDispatch DEFAULT(0);

    IF COL_LENGTH('dbo.AppSettings', 'IqdRoundingStep') IS NULL
        ALTER TABLE dbo.AppSettings ADD IqdRoundingStep int NOT NULL
            CONSTRAINT DF_AppSettings_IqdRoundingStep DEFAULT(250);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
