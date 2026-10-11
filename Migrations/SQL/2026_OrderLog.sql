-- =============================================================================
-- Order audit log: every create/update/delete on Orders + OrderDetail
-- Idempotent. Safe to re-run. Targets SQL Server.
-- =============================================================================
SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF OBJECT_ID('dbo.OrderLog', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.OrderLog (
            OrderLogId  int IDENTITY(1,1) NOT NULL PRIMARY KEY,
            OrderId     int NOT NULL,
            OrderNo     int NULL,
            Action      nvarchar(60)  NOT NULL,
            Title       nvarchar(200) NOT NULL,
            Details     nvarchar(max) NULL,
            ActorType   nvarchar(20)  NOT NULL,
            ActorId     int NULL,
            ActorName   nvarchar(200) NULL,
            CreatedAt   datetime NOT NULL CONSTRAINT DF_OrderLog_CreatedAt DEFAULT(GETDATE())
        );

        CREATE INDEX IX_OrderLog_OrderId ON dbo.OrderLog(OrderId, CreatedAt);
        CREATE INDEX IX_OrderLog_CreatedAt ON dbo.OrderLog(CreatedAt DESC);
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
