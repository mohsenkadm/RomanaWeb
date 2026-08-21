-- =============================================================================
-- Problem reports (driver → admin) linked to orders
-- Idempotent. Safe to re-run. Targets SQL Server.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF OBJECT_ID('dbo.ProblemReport', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProblemReport (
            ProblemReportId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            OrderId         int NOT NULL,
            SaleManId       int NOT NULL,
            Message         nvarchar(2000) NOT NULL,
            Status          int NOT NULL CONSTRAINT DF_ProblemReport_Status DEFAULT(0),
            CreatedAt       datetime NOT NULL CONSTRAINT DF_ProblemReport_CreatedAt DEFAULT(GETDATE()),
            UpdatedAt       datetime NULL,
            AdminNote       nvarchar(1000) NULL
        );

        CREATE INDEX IX_ProblemReport_CreatedAt ON dbo.ProblemReport(CreatedAt DESC);
        CREATE INDEX IX_ProblemReport_Status ON dbo.ProblemReport(Status);
        CREATE INDEX IX_ProblemReport_OrderId ON dbo.ProblemReport(OrderId);
        CREATE INDEX IX_ProblemReport_SaleManId ON dbo.ProblemReport(SaleManId);
    END

    -- Menu permission
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'ProblemReports')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'إبلاغات المشاكل', N'ProblemReports');

        -- Grant to admins who already have Orders permission
        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'Orders')
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'ProblemReports')
        BEGIN
            DECLARE @ordersId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'Orders');
            DECLARE @prId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'ProblemReports');

            INSERT INTO dbo.Permission (AdminId, PermissionNameId)
            SELECT p.AdminId, @prId
            FROM dbo.Permission p
            WHERE p.PermissionNameId = @ordersId
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.Permission x
                  WHERE x.AdminId = p.AdminId AND x.PermissionNameId = @prId);
        END
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
