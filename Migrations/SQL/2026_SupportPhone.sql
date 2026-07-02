-- =============================================================================
-- Support phone numbers per mobile app (customer / restaurant / driver)
-- Idempotent. Safe to re-run.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF OBJECT_ID('dbo.SupportPhone', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SupportPhone (
            SupportPhoneId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            AppType        int NOT NULL,
            Phone          nvarchar(20) NOT NULL,
            Label          nvarchar(200) NULL,
            IsActive       bit NOT NULL CONSTRAINT DF_SupportPhone_IsActive DEFAULT(1),
            CONSTRAINT UQ_SupportPhone_AppType UNIQUE (AppType)
        );
    END

    -- Permission master row (if PermissionName table exists in your DB)
    IF OBJECT_ID('dbo.PermissionName', 'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.PermissionName WHERE ControlName = N'SupportPhone')
            INSERT INTO dbo.PermissionName (PermissionName, ControlName)
            VALUES (N'أرقام الدعم الفني', N'SupportPhone');
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
