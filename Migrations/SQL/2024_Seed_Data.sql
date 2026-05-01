-- =============================================================================
-- Rumana Platform - Seed data (idempotent).
-- Run AFTER 2024_Modernization_Upgrade.sql.
-- Inserts only when the row does not already exist.
-- =============================================================================

SET NOCOUNT ON;

-------------------------------------------------------------------------------
-- AppSettings: ensure at least one configuration row exists.
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings)
BEGIN
    INSERT INTO dbo.AppSettings
        (PricePerKm, DefaultOrderCost, MinChargeKmThreshold, MinChargeAmount, RoundingMode, ZoneMinKm, ZoneMaxKm)
    VALUES
        (500, 3000, 1.5, 500, 'Ceil', 0, 0);
END

-------------------------------------------------------------------------------
-- Zones: two demo polygons over Baghdad (Karada and Mansour bounding boxes).
-- Replace with real polygons when admin uploads the actual Excel.
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Zone WHERE Name = N'Karada')
BEGIN
    INSERT INTO dbo.Zone (Name, GeoJson, IsActive)
    VALUES (
        N'Karada',
        N'{"type":"Polygon","coordinates":[[[44.40,33.30],[44.45,33.30],[44.45,33.34],[44.40,33.34],[44.40,33.30]]]}',
        1
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.Zone WHERE Name = N'Mansour')
BEGIN
    INSERT INTO dbo.Zone (Name, GeoJson, IsActive)
    VALUES (
        N'Mansour',
        N'{"type":"Polygon","coordinates":[[[44.32,33.30],[44.38,33.30],[44.38,33.34],[44.32,33.34],[44.32,33.30]]]}',
        1
    );
END

-------------------------------------------------------------------------------
-- ZonePrice matrix: A->B and B->A for the two demo zones.
-------------------------------------------------------------------------------
DECLARE @karada int = (SELECT TOP 1 ZoneId FROM dbo.Zone WHERE Name = N'Karada');
DECLARE @mansour int = (SELECT TOP 1 ZoneId FROM dbo.Zone WHERE Name = N'Mansour');

IF @karada IS NOT NULL AND @mansour IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.ZonePrice WHERE FromZoneId = @karada AND ToZoneId = @mansour)
        INSERT INTO dbo.ZonePrice (FromZoneId, ToZoneId, Price) VALUES (@karada, @mansour, 4000);

    IF NOT EXISTS (SELECT 1 FROM dbo.ZonePrice WHERE FromZoneId = @mansour AND ToZoneId = @karada)
        INSERT INTO dbo.ZonePrice (FromZoneId, ToZoneId, Price) VALUES (@mansour, @karada, 4000);
END

-------------------------------------------------------------------------------
-- Demo promo codes (only inserted if missing).
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.PromoCode WHERE PromoName = N'WELCOME10')
BEGIN
    INSERT INTO dbo.PromoCode
        (PromoName, Percentage, RestaurantId, MaxOrders, UsedOrders, IsActive, IsForAllStores, DiscountAmount, DiscountType, MaxDiscountAmount)
    VALUES
        (N'WELCOME10', 10, 0, 1000, 0, 1, 1, 0, N'Percentage', 5000);
END

IF NOT EXISTS (SELECT 1 FROM dbo.PromoCode WHERE PromoName = N'STORE500')
BEGIN
    -- STORE-scoped; assumes a restaurant exists with id = (SELECT TOP 1 ...)
    DECLARE @anyRes int = (SELECT TOP 1 RestaurantId FROM dbo.Restaurant ORDER BY RestaurantId);
    IF @anyRes IS NOT NULL
    BEGIN
        INSERT INTO dbo.PromoCode
            (PromoName, Percentage, RestaurantId, MaxOrders, UsedOrders, IsActive, IsForAllStores, DiscountAmount, DiscountType, MaxDiscountAmount)
        VALUES
            (N'STORE500', 0, @anyRes, 200, 0, 1, 0, 500, N'Fixed', 0);
    END
END

PRINT 'Seed data applied.';
GO
