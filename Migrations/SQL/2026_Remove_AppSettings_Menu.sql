-- Redirect admin menu: AppSettings → Zones (pricing moved to Zones UI)
SET NOCOUNT ON;
BEGIN TRY
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'Zones')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'إدارة المناطق والتسعير', N'Zones');

        -- Repoint user permissions from AppSettings to Zones
        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'AppSettings')
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'Zones')
        BEGIN
            DECLARE @oldId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'AppSettings');
            DECLARE @newId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'Zones');

            INSERT INTO dbo.Permission (AdminId, PermissionNameId)
            SELECT p.AdminId, @newId
            FROM dbo.Permission p
            WHERE p.PermissionNameId = @oldId
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.Permission x
                  WHERE x.AdminId = p.AdminId AND x.PermissionNameId = @newId);

            DELETE FROM dbo.Permission WHERE PermissionNameId = @oldId;
            DELETE FROM dbo.PermissionName WHERE PermissionNameId = @oldId;
        END
    END
END TRY
BEGIN CATCH
    THROW;
END CATCH
