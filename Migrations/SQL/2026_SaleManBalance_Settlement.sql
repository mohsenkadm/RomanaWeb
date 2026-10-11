-- =============================================================================
-- SaleMan dual balances (food net + delivery fees) + settlement (تصفير) ledger
-- Idempotent. Safe to re-run. Targets SQL Server.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    -- Current running balances per driver
    IF OBJECT_ID('dbo.SaleManBalance', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SaleManBalance (
            SaleManId            int            NOT NULL PRIMARY KEY,
            FoodNetBalance       decimal(18,2)  NOT NULL CONSTRAINT DF_SaleManBalance_Food DEFAULT(0),
            DeliveryFeeBalance   decimal(18,2)  NOT NULL CONSTRAINT DF_SaleManBalance_Fee DEFAULT(0),
            UpdatedAt            datetime       NOT NULL CONSTRAINT DF_SaleManBalance_UpdatedAt DEFAULT(GETDATE())
        );
    END

    -- Settlement (تصفير) transactions
    IF OBJECT_ID('dbo.SaleManSettlement', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SaleManSettlement (
            SettlementId         int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            SaleManId            int            NOT NULL,
            FoodNetAmount        decimal(18,2)  NOT NULL CONSTRAINT DF_SaleManSettlement_Food DEFAULT(0),
            DeliveryFeeAmount    decimal(18,2)  NOT NULL CONSTRAINT DF_SaleManSettlement_Fee DEFAULT(0),
            SettledAt            datetime       NOT NULL CONSTRAINT DF_SaleManSettlement_SettledAt DEFAULT(GETDATE()),
            SettledByAdminId     int            NULL,
            Note                 nvarchar(500)  NULL
        );

        CREATE INDEX IX_SaleManSettlement_SaleManId ON dbo.SaleManSettlement(SaleManId);
        CREATE INDEX IX_SaleManSettlement_SettledAt ON dbo.SaleManSettlement(SettledAt DESC);
    END

    -- Per-order accrual entries (linked to a settlement when cleared)
    IF OBJECT_ID('dbo.SaleManBalanceEntry', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SaleManBalanceEntry (
            EntryId              int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            SaleManId            int            NOT NULL,
            OrderId              int            NOT NULL,
            FoodNetAmount        decimal(18,2)  NOT NULL CONSTRAINT DF_SaleManBalanceEntry_Food DEFAULT(0),
            DeliveryFeeAmount    decimal(18,2)  NOT NULL CONSTRAINT DF_SaleManBalanceEntry_Fee DEFAULT(0),
            CreatedAt            datetime       NOT NULL CONSTRAINT DF_SaleManBalanceEntry_CreatedAt DEFAULT(GETDATE()),
            SettlementId         int            NULL,
            CONSTRAINT UQ_SaleManBalanceEntry_OrderId UNIQUE (OrderId)
        );

        CREATE INDEX IX_SaleManBalanceEntry_SaleManId ON dbo.SaleManBalanceEntry(SaleManId);
        CREATE INDEX IX_SaleManBalanceEntry_SettlementId ON dbo.SaleManBalanceEntry(SettlementId);
        CREATE INDEX IX_SaleManBalanceEntry_Open
            ON dbo.SaleManBalanceEntry(SaleManId)
            WHERE SettlementId IS NULL;
    END

    -- Menu permission: تقرير تصفير المندوبين
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'DriverSettlements')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'تصفير حسابات المندوبين', N'DriverSettlements');

        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'SaleMan')
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'DriverSettlements')
        BEGIN
            DECLARE @saleManPermId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'SaleMan');
            DECLARE @settlePermId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'DriverSettlements');

            INSERT INTO dbo.Permission (AdminId, PermissionNameId)
            SELECT p.AdminId, @settlePermId
            FROM dbo.Permission p
            WHERE p.PermissionNameId = @saleManPermId
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.Permission x
                  WHERE x.AdminId = p.AdminId AND x.PermissionNameId = @settlePermId);
        END
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
