-- =============================================================================
-- Rumana Platform - Order dropoff coordinates + backfill from Users
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF COL_LENGTH('dbo.Orders', 'Lat') IS NULL
        ALTER TABLE dbo.Orders ADD Lat nvarchar(50) NULL;

    IF COL_LENGTH('dbo.Orders', 'Long') IS NULL
        ALTER TABLE dbo.Orders ADD [Long] nvarchar(50) NULL;

    -- Backfill order dropoff coords from customer profile when missing/zero.
    UPDATE o
    SET o.Lat = u.Lat,
        o.Long = u.Long
    FROM dbo.Orders o
    INNER JOIN dbo.Users u ON u.UserId = o.UserId
    WHERE (o.Lat IS NULL OR LTRIM(RTRIM(o.Lat)) IN ('', '0', '0.0'))
      AND u.Lat IS NOT NULL
      AND LTRIM(RTRIM(u.Lat)) NOT IN ('', '0', '0.0');

    UPDATE o
    SET o.Long = u.Long
    FROM dbo.Orders o
    INNER JOIN dbo.Users u ON u.UserId = o.UserId
    WHERE (o.Long IS NULL OR LTRIM(RTRIM(o.Long)) IN ('', '0', '0.0'))
      AND u.Long IS NOT NULL
      AND LTRIM(RTRIM(u.Long)) NOT IN ('', '0', '0.0');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH

-- Restaurants still missing valid coordinates (review manually):
-- SELECT RestaurantId, Name, Lat, [Long]
-- FROM dbo.Restaurant
-- WHERE Lat IS NULL OR [Long] IS NULL
--    OR TRY_CAST(Lat AS float) IS NULL OR ABS(TRY_CAST(Lat AS float)) < 0.0001
--    OR TRY_CAST([Long] AS float) IS NULL OR ABS(TRY_CAST([Long] AS float)) < 0.0001;
