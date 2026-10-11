-- كشف المندوبين (DriverActivity) + كشف المطاعم (GetReportRes)
-- يحدّث أسماء القائمة ويمنح الصلاحية لكل الأدمن الموجودين.
SET NOCOUNT ON;
BEGIN TRY
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        -- DriverActivity → كشف المندوبين
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'DriverActivity')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'كشف المندوبين', N'DriverActivity');
        ELSE
            UPDATE dbo.PermissionName
            SET PermissionName = N'كشف المندوبين'
            WHERE ControlName = N'DriverActivity';

        -- GetReportRes → كشف المطاعم
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'GetReportRes')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'كشف المطاعم', N'GetReportRes');
        ELSE
            UPDATE dbo.PermissionName
            SET PermissionName = N'كشف المطاعم'
            WHERE ControlName = N'GetReportRes';

        IF OBJECT_ID('dbo.Permission', 'U') IS NOT NULL
           AND OBJECT_ID('dbo.Admin', 'U') IS NOT NULL
        BEGIN
            DECLARE @driverActId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'DriverActivity');
            DECLARE @reportResId int = (SELECT TOP 1 PermissionNameId FROM dbo.PermissionName WHERE ControlName = N'GetReportRes');

            IF @driverActId IS NOT NULL
            BEGIN
                INSERT INTO dbo.Permission (AdminId, PermissionNameId)
                SELECT a.AdminId, @driverActId
                FROM dbo.Admin a
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Permission p
                    WHERE p.AdminId = a.AdminId AND p.PermissionNameId = @driverActId);
            END

            IF @reportResId IS NOT NULL
            BEGIN
                INSERT INTO dbo.Permission (AdminId, PermissionNameId)
                SELECT a.AdminId, @reportResId
                FROM dbo.Admin a
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.Permission p
                    WHERE p.AdminId = a.AdminId AND p.PermissionNameId = @reportResId);
            END
        END
    END
END TRY
BEGIN CATCH
    THROW;
END CATCH
