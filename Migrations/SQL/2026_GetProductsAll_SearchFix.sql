-- =============================================================================
-- Fix GetProductsAll: restaurant/subcategory search must not require IsActive/IsClosed
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.GetProductsAll', 'P') IS NOT NULL
BEGIN
    EXEC('
ALTER PROCEDURE [dbo].[GetProductsAll]
    @Name nvarchar(500),
    @RestaurantName nvarchar(500) = NULL,
    @SubCategoriesName nvarchar(500) = NULL,
    @index int
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Name = '''') SET @Name = NULL;
    IF (@RestaurantName = '''') SET @RestaurantName = NULL;
    IF (@SubCategoriesName = '''') SET @SubCategoriesName = NULL;

    ;WITH C AS
    (
        SELECT
            ROW_NUMBER() OVER (ORDER BY p.ProductsId) AS _RowCount,
            p.*,
            r.Name AS RestaurantName,
            s.SubCategoriesName,
            ProductsImageFirst.ImagePath AS ProductsImageFirst
        FROM Products p
        INNER JOIN Restaurant r ON r.RestaurantId = p.RestaurantId
        INNER JOIN SubCategories s ON s.SubCategoriesId = p.SubCategoriesId
        OUTER APPLY (
            SELECT TOP (1) i.ImagePath
            FROM Images i
            WHERE i.ProductsId = p.ProductsId
        ) AS ProductsImageFirst
        WHERE
            (@Name IS NULL OR p.ProductsName LIKE ''%'' + @Name + ''%'' OR p.ProductsDetails LIKE ''%'' + @Name + ''%'')
            AND r.IsDelete = 0
            AND (
                @RestaurantName IS NOT NULL
                OR @SubCategoriesName IS NOT NULL
                OR @Name IS NOT NULL
                OR (r.IsActive = 1 AND ISNULL(r.IsClosed, 0) = 0)
            )
            AND (@RestaurantName IS NULL OR r.Name LIKE ''%'' + @RestaurantName + ''%'')
            AND (@SubCategoriesName IS NULL OR s.SubCategoriesName LIKE ''%'' + @SubCategoriesName + ''%'')
    )
    SELECT *
    FROM C
    WHERE C._RowCount BETWEEN ((@index - 1) * 100 + 1) AND (((@index - 1) * 100 + 1) + 100) - 1
    ORDER BY C.RestaurantName;
END
    ');
END
GO
