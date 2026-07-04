-- Permission for Driver Activity report page (PDF §2.3.2)
SET NOCOUNT ON;
BEGIN TRY
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'DriverActivity')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'نشاط المندوبين', N'DriverActivity');

        -- Grant to admins who already have Zones access
        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'Zones')
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'DriverActivity')
        BEGIN
            DECLARE @zonesId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'Zones');
            DECLARE @actId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'DriverActivity');

            INSERT INTO dbo.Permission (AdminId, PermissionNameId)
            SELECT p.AdminId, @actId
            FROM dbo.Permission p
            WHERE p.PermissionNameId = @zonesId
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.Permission x
                  WHERE x.AdminId = p.AdminId AND x.PermissionNameId = @actId);
        END
    END
END TRY
BEGIN CATCH
    THROW;
END CATCH
