USE ItemManagementDb;
GO

SET NOCOUNT ON;
GO

/* =========================================================
   1) Roles (Admin/User)
   ========================================================= */
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        RoleName     NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
    );
END
GO

/* =========================================================
   2) Users (stores Username + PasswordHash/Salt)
   ========================================================= */
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username      NVARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,

        PasswordHash  VARBINARY(256) NOT NULL,
        PasswordSalt  VARBINARY(128) NOT NULL,

        IsActive      BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
        CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_Users_Username ON dbo.Users(Username);
END
GO

/* =========================================================
   3) UserRoles (assign roles to users)
   ========================================================= */
IF OBJECT_ID('dbo.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId  INT NOT NULL,
        RoleId  INT NOT NULL,

        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
    );

    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles(RoleId);
END
GO

/* =========================================================
   4) Items (actual items - only Admin creates)
   ========================================================= */
IF OBJECT_ID('dbo.Items', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Items
    (
        ItemId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Items PRIMARY KEY,
        Name             NVARCHAR(200) NOT NULL,
        Description      NVARCHAR(1000) NULL,

        CreatedByUserId  INT NOT NULL,
        CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Items_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_Items_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId)
    );

    CREATE INDEX IX_Items_Name ON dbo.Items(Name);
END
GO

/* =========================================================
   5) ItemRequests (user requests, pending/approved/denied)
   ========================================================= */
IF OBJECT_ID('dbo.ItemRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemRequests
    (
        RequestId               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemRequests PRIMARY KEY,

        RequestedName           NVARCHAR(200) NOT NULL,
        RequestedDescription    NVARCHAR(1000) NULL,

        Status                  NVARCHAR(20) NOT NULL CONSTRAINT DF_ItemRequests_Status DEFAULT('Pending'),
        RejectionReason         NVARCHAR(1000) NULL,

        RequestedByUserId       INT NOT NULL,
        DecisionByAdminUserId   INT NULL,
        DecidedAt               DATETIME2(0) NULL,

        CreatedAt               DATETIME2(0) NOT NULL CONSTRAINT DF_ItemRequests_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_ItemRequests_RequestedBy FOREIGN KEY (RequestedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_ItemRequests_DecisionBy FOREIGN KEY (DecisionByAdminUserId) REFERENCES dbo.Users(UserId),

        CONSTRAINT CK_ItemRequests_Status CHECK (Status IN ('Pending','Approved','Denied')),

        -- if status is Denied, rejection reason must be filled
        CONSTRAINT CK_ItemRequests_DeniedReason CHECK (
            (Status <> 'Denied') OR (RejectionReason IS NOT NULL AND LTRIM(RTRIM(RejectionReason)) <> '')
        )
    );

    CREATE INDEX IX_ItemRequests_Status ON dbo.ItemRequests(Status);
    CREATE INDEX IX_ItemRequests_RequestedBy ON dbo.ItemRequests(RequestedByUserId);
END
GO

/* =========================================================
   6) Appeals (user response after denial)
   ========================================================= */
IF OBJECT_ID('dbo.ItemRequestAppeals', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemRequestAppeals
    (
        AppealId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemRequestAppeals PRIMARY KEY,
        RequestId         INT NOT NULL,
        AppealMessage     NVARCHAR(2000) NOT NULL,

        CreatedByUserId   INT NOT NULL,
        CreatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_ItemRequestAppeals_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_Appeals_Request FOREIGN KEY (RequestId) REFERENCES dbo.ItemRequests(RequestId),
        CONSTRAINT FK_Appeals_User FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(UserId)
    );

    CREATE INDEX IX_ItemRequestAppeals_RequestId ON dbo.ItemRequestAppeals(RequestId);
END
GO

/* =========================================================
   7) AuditLogs (track every action)
   ========================================================= */
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        ActorUserId   INT NOT NULL,
        ActionType    NVARCHAR(100) NOT NULL,
        EntityType    NVARCHAR(100) NOT NULL,
        EntityId      INT NULL,
        MetadataJson  NVARCHAR(MAX) NULL,
        CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_AuditLogs_Actor FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(UserId)
    );

    CREATE INDEX IX_AuditLogs_CreatedAt ON dbo.AuditLogs(CreatedAt);
    CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs(EntityType, EntityId);
END
GO

/* =========================================================
   8) Insert default roles (Admin/User)
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Admin')
    INSERT INTO dbo.Roles(RoleName) VALUES ('Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'User')
    INSERT INTO dbo.Roles(RoleName) VALUES ('User');
GO
SELECT * FROM dbo.Roles;
SELECT name FROM sys.tables ORDER BY name;
SELECT
    AuditId,
    ActorUserId,
    ActionType,
    EntityType,
    EntityId,
    MetadataJson,
    CreatedAt
FROM dbo.AuditLogs
ORDER BY CreatedAt DESC;
SELECT
    a.AuditId,
    u.Username AS ActorUsername,
    a.ActorUserId,
    a.ActionType,
    a.EntityType,
    a.EntityId,
    a.MetadataJson,
    a.CreatedAt
FROM dbo.AuditLogs a
JOIN dbo.Users u ON u.UserId = a.ActorUserId
ORDER BY a.CreatedAt DESC;

SELECT
    a.AuditId,
    u.Username AS ActorUsername,
    a.ActionType,
    a.EntityType,
    a.EntityId,
    a.CreatedAt
FROM dbo.AuditLogs a
JOIN dbo.Users u ON u.UserId = a.ActorUserId
ORDER BY a.CreatedAt DESC;
SELECT COUNT(*) AS TotalRows
FROM dbo.AuditLogs;
SELECT DISTINCT ActionType
FROM dbo.AuditLogs
ORDER BY ActionType;
SELECT TOP 20 *
FROM dbo.AuditLogs
ORDER BY CreatedAt DESC;
SELECT *
FROM dbo.AuditLogs
ORDER BY CreatedAt DESC;
SELECT DB_NAME() AS CurrentDatabase;
SELECT DB_NAME() AS CurrentDatabase;
SELECT COUNT(*) AS TotalAuditRows
FROM dbo.AuditLogs;
SELECT TOP 50 *
FROM dbo.AuditLogs
ORDER BY CreatedAt DESC;
SELECT TOP 20 RequestId, RequestedName, Status, RejectionReason, CreatedAt
FROM dbo.ItemRequests
ORDER BY CreatedAt DESC;
