-- =============================================================================
-- Product Sizes & Ingredients + OrderDetail size/ingredient columns
-- Idempotent. Targets SQL Server.
-- =============================================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY

    -------------------------------------------------------------------------------
    -- ProductSize table
    -------------------------------------------------------------------------------
    IF OBJECT_ID('dbo.ProductSize', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProductSize (
            ProductSizeId   int          NOT NULL IDENTITY(1,1),
            ProductsId      int          NOT NULL,
            SizeName        nvarchar(200) NOT NULL,
            SizePrice       float        NOT NULL CONSTRAINT DF_ProductSize_SizePrice DEFAULT(0),
            CONSTRAINT PK_ProductSize PRIMARY KEY (ProductSizeId)
        );

        CREATE INDEX IX_ProductSize_ProductsId ON dbo.ProductSize(ProductsId);
    END

    -------------------------------------------------------------------------------
    -- ProductIngredient table
    -------------------------------------------------------------------------------
    IF OBJECT_ID('dbo.ProductIngredient', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProductIngredient (
            ProductIngredientId int           NOT NULL IDENTITY(1,1),
            ProductsId          int           NOT NULL,
            IngredientName      nvarchar(300) NOT NULL,
            CONSTRAINT PK_ProductIngredient PRIMARY KEY (ProductIngredientId)
        );

        CREATE INDEX IX_ProductIngredient_ProductsId ON dbo.ProductIngredient(ProductsId);
    END

    -------------------------------------------------------------------------------
    -- OrderDetail — size & ingredients columns
    -------------------------------------------------------------------------------
    IF COL_LENGTH('dbo.OrderDetail', 'SelectedSizeId') IS NULL
        ALTER TABLE dbo.OrderDetail ADD SelectedSizeId int NULL;

    IF COL_LENGTH('dbo.OrderDetail', 'SelectedSizeName') IS NULL
        ALTER TABLE dbo.OrderDetail ADD SelectedSizeName nvarchar(200) NULL;

    IF COL_LENGTH('dbo.OrderDetail', 'SelectedSizePrice') IS NULL
        ALTER TABLE dbo.OrderDetail ADD SelectedSizePrice float NULL;

    IF COL_LENGTH('dbo.OrderDetail', 'SelectedIngredients') IS NULL
        ALTER TABLE dbo.OrderDetail ADD SelectedIngredients nvarchar(1000) NULL;

    COMMIT TRANSACTION;
    PRINT 'Product sizes & ingredients migration applied successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Migration failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH
