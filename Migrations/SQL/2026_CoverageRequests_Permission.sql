-- Permission for Service Coverage Requests admin page
SET NOCOUNT ON;
BEGIN TRY
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'CoverageRequests')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'طلبات توفير الخدمة', N'CoverageRequests');

        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'Zones')
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'CoverageRequests')
        BEGIN
            DECLARE @zonesId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'Zones');
            DECLARE @covId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'CoverageRequests');

            INSERT INTO dbo.Permission (AdminId, PermissionNameId)
            SELECT p.AdminId, @covId
            FROM dbo.Permission p
            WHERE p.PermissionNameId = @zonesId
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.Permission x
                  WHERE x.AdminId = p.AdminId AND x.PermissionNameId = @covId);
        END
    END
END TRY
BEGIN CATCH
    THROW;
END CATCH
