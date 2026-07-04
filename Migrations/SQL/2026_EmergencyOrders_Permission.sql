-- Permission for Emergency Orders dashboard (PDF §2.3.1)
SET NOCOUNT ON;
BEGIN TRY
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'EmergencyOrders')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'لوحة الطوارئ', N'EmergencyOrders');

        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'Zones')
           AND EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'EmergencyOrders')
        BEGIN
            DECLARE @zonesId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'Zones');
            DECLARE @emId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'EmergencyOrders');

            INSERT INTO dbo.Permission (AdminId, PermissionNameId)
            SELECT p.AdminId, @emId
            FROM dbo.Permission p
            WHERE p.PermissionNameId = @zonesId
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.Permission x
                  WHERE x.AdminId = p.AdminId AND x.PermissionNameId = @emId);
        END
    END
END TRY
BEGIN CATCH
    THROW;
END CATCH
