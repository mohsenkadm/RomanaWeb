-- =============================================================================
-- Rumana Platform - Modernization schema upgrade
-- Covers Sections 2.1, 2.2, 3, 5.1, 6 of the spec.
-- Idempotent: safe to re-run. Targets SQL Server.
-- =============================================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY

    -------------------------------------------------------------------------------
    -- Section 3: PromoCode - new fields
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.PromoCode', 'DiscountType') IS NULL
        ALTER TABLE dbo.PromoCode ADD DiscountType nvarchar(20) NULL;

    IF COL_LENGTH('dbo.PromoCode', 'MaxDiscountAmount') IS NULL
        ALTER TABLE dbo.PromoCode ADD MaxDiscountAmount decimal(18,2) NOT NULL
            CONSTRAINT DF_PromoCode_MaxDiscountAmount DEFAULT(0);

    IF COL_LENGTH('dbo.PromoCode', 'FirstUsedAt') IS NULL
        ALTER TABLE dbo.PromoCode ADD FirstUsedAt datetime NULL;

    -- Best-effort backfill of DiscountType for existing rows.
    UPDATE dbo.PromoCode
       SET DiscountType = CASE
                              WHEN DiscountAmount > 0 THEN 'Fixed'
                              WHEN Percentage     > 0 THEN 'Percentage'
                              ELSE NULL
                          END
     WHERE DiscountType IS NULL;

    -------------------------------------------------------------------------------
    -- Section 5.1: Stars - link to order + user
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.Stars', 'OrderId') IS NULL
        ALTER TABLE dbo.Stars ADD OrderId int NULL;

    IF COL_LENGTH('dbo.Stars', 'UserId') IS NULL
        ALTER TABLE dbo.Stars ADD UserId int NULL;

    -- Helpful index for "one rating per order" lookup.
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stars_OrderId' AND object_id = OBJECT_ID('dbo.Stars'))
        CREATE INDEX IX_Stars_OrderId ON dbo.Stars(OrderId);

    -------------------------------------------------------------------------------
    -- Section 5.1: DriverStars already has OrderId in the entity; add if missing.
    -------------------------------------------------------------------------------
    IF OBJECT_ID('dbo.DriverStars', 'U') IS NOT NULL
       AND COL_LENGTH('dbo.DriverStars', 'OrderId') IS NULL
        ALTER TABLE dbo.DriverStars ADD OrderId int NULL;

    IF OBJECT_ID('dbo.DriverStars', 'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DriverStars_OrderId' AND object_id = OBJECT_ID('dbo.DriverStars'))
        CREATE INDEX IX_DriverStars_OrderId ON dbo.DriverStars(OrderId);

    -------------------------------------------------------------------------------
    -- Section 2.1 / 2.2: AppSettings - pricing engine config
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.AppSettings', 'MinChargeKmThreshold') IS NULL
        ALTER TABLE dbo.AppSettings ADD MinChargeKmThreshold decimal(18,2) NOT NULL
            CONSTRAINT DF_AppSettings_MinChargeKmThreshold DEFAULT(1.5);

    IF COL_LENGTH('dbo.AppSettings', 'MinChargeAmount') IS NULL
        ALTER TABLE dbo.AppSettings ADD MinChargeAmount decimal(18,2) NOT NULL
            CONSTRAINT DF_AppSettings_MinChargeAmount DEFAULT(500);

    IF COL_LENGTH('dbo.AppSettings', 'RoundingMode') IS NULL
        ALTER TABLE dbo.AppSettings ADD RoundingMode nvarchar(10) NOT NULL
            CONSTRAINT DF_AppSettings_RoundingMode DEFAULT('Ceil');

    IF COL_LENGTH('dbo.AppSettings', 'ZoneMaxKm') IS NULL
        ALTER TABLE dbo.AppSettings ADD ZoneMaxKm decimal(18,2) NOT NULL
            CONSTRAINT DF_AppSettings_ZoneMaxKm DEFAULT(0);

    IF COL_LENGTH('dbo.AppSettings', 'ZoneMinKm') IS NULL
        ALTER TABLE dbo.AppSettings ADD ZoneMinKm decimal(18,2) NOT NULL
            CONSTRAINT DF_AppSettings_ZoneMinKm DEFAULT(0);

    -- Seed a default row if AppSettings is empty.
    IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings)
    BEGIN
        INSERT INTO dbo.AppSettings (PricePerKm, DefaultOrderCost, MinChargeKmThreshold, MinChargeAmount, RoundingMode, ZoneMinKm, ZoneMaxKm)
        VALUES (500, 3000, 1.5, 500, 'Ceil', 0, 0);
    END

    -------------------------------------------------------------------------------
    -- Section 6: SaleMan - driver location heartbeat
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.SaleMan', 'Lat') IS NULL
        ALTER TABLE dbo.SaleMan ADD Lat nvarchar(50) NULL;

    IF COL_LENGTH('dbo.SaleMan', 'Long') IS NULL
        ALTER TABLE dbo.SaleMan ADD [Long] nvarchar(50) NULL;

    IF COL_LENGTH('dbo.SaleMan', 'LocationUpdatedAt') IS NULL
        ALTER TABLE dbo.SaleMan ADD LocationUpdatedAt datetime NULL;

    -------------------------------------------------------------------------------
    -- Section 2.2: Zone (polygon) and ZonePrice (matrix)
    -------------------------------------------------------------------------------
    IF OBJECT_ID('dbo.Zone', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Zone
        (
            ZoneId   int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_Zone PRIMARY KEY,
            Name     nvarchar(200) NOT NULL,
            GeoJson  nvarchar(max) NOT NULL,
            IsActive bit           NOT NULL CONSTRAINT DF_Zone_IsActive DEFAULT(1)
        );
    END

    IF OBJECT_ID('dbo.ZonePrice', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ZonePrice
        (
            ZonePriceId int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_ZonePrice PRIMARY KEY,
            FromZoneId  int           NOT NULL,
            ToZoneId    int           NOT NULL,
            Price       decimal(18,2) NOT NULL
        );

        CREATE UNIQUE INDEX UX_ZonePrice_From_To
            ON dbo.ZonePrice(FromZoneId, ToZoneId);
    END

    -------------------------------------------------------------------------------
    -- (Optional) Foreign keys for ZonePrice -> Zone. Uncomment if you want them.
    -------------------------------------------------------------------------------
    -- IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ZonePrice_FromZone')
    --     ALTER TABLE dbo.ZonePrice
    --         ADD CONSTRAINT FK_ZonePrice_FromZone FOREIGN KEY (FromZoneId) REFERENCES dbo.Zone(ZoneId);
    -- IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ZonePrice_ToZone')
    --     ALTER TABLE dbo.ZonePrice
    --         ADD CONSTRAINT FK_ZonePrice_ToZone   FOREIGN KEY (ToZoneId)   REFERENCES dbo.Zone(ZoneId);

    COMMIT TRANSACTION;
    PRINT 'Modernization schema upgrade applied successfully.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @msg nvarchar(4000) = ERROR_MESSAGE();
    DECLARE @line int           = ERROR_LINE();
    RAISERROR('Migration failed at line %d: %s', 16, 1, @line, @msg);
END CATCH;
GO
