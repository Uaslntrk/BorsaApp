using System.Threading.Tasks;
using Dapper;
using WpfLibrary1;

namespace BorsaApp.DAL
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            // Veritabani yoksa olustur
            var csBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Db.ConnectionString);
            var dbName = csBuilder.InitialCatalog;
            
            if (!string.IsNullOrEmpty(dbName))
            {
                csBuilder.InitialCatalog = "master";
                using var masterConn = new Microsoft.Data.SqlClient.SqlConnection(csBuilder.ConnectionString);
                masterConn.Open();
                var exists = masterConn.ExecuteScalar<int?>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName });
                if (exists == null)
                {
                    masterConn.Execute($"CREATE DATABASE [{dbName}]");
                }
            }

            var sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Customers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [TcNo] [nvarchar](20) NULL,
        [RiskLevel] [nvarchar](50) NOT NULL DEFAULT ('Orta'),
        [IsActive] [bit] NOT NULL DEFAULT (1),
        [CashBalance] [decimal](18, 4) NOT NULL DEFAULT (100000.0000),
     CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assets]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Assets](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [nvarchar](50) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Sector] [nvarchar](100) NULL,
        [CurrentPrice] [decimal](18, 4) NOT NULL DEFAULT (0),
        [PreviousPrice] [decimal](18, 4) NOT NULL DEFAULT (0),
        [UpdatedAt] [datetime2](7) NULL,
     CONSTRAINT [PK_Assets] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    -- Seed dummy assets
    INSERT INTO [dbo].[Assets] ([Code], [Name], [Sector], [CurrentPrice], [UpdatedAt])
    VALUES ('THYAO', 'THY', 'Ulasim', 280.50, SYSUTCDATETIME()), 
           ('EREGL', 'Eregli', 'Demir Celik', 45.20, SYSUTCDATETIME()), 
           ('ASELS', 'Aselsan', 'Savunma', 52.10, SYSUTCDATETIME())
END

-- Ensure UpdatedAt exists for existing tables
-- Ensure UpdatedAt exists for existing tables
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Assets') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Assets ADD [UpdatedAt] [datetime2](7) NULL
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Trades]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Trades](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [CustomerId] [int] NOT NULL,
        [AssetId] [int] NOT NULL,
        [BuySell] [nvarchar](10) NOT NULL,
        [Quantity] [int] NOT NULL,
        [Price] [decimal](18, 4) NOT NULL,
        [TradeDate] [datetime2](7) NOT NULL,
        [IsCancelled] [bit] NOT NULL DEFAULT (0),
        [CancelledAt] [datetime2](7) NULL,
        [CancelReason] [nvarchar](max) NULL,
     CONSTRAINT [PK_Trades] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](50) NOT NULL,
        [PasswordHash] [varbinary](max) NOT NULL,
        [Role] [nvarchar](50) NOT NULL,
        [IsActive] [bit] NOT NULL,
     CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

END

-- Seed admin (pass: admin) and customer (pass: admin)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Role], [IsActive])
    VALUES ('admin', 0x8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918, 'Admin', 1)
END

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'user')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Role], [IsActive])
    VALUES ('user', 0x8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918, 'Customer', 1)
END


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [Actor] [nvarchar](100) NULL,
        [Action] [nvarchar](100) NOT NULL,
        [Entity] [nvarchar](100) NOT NULL,
        [EntityId] [int] NOT NULL,
        [Details] [nvarchar](max) NULL,
     CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceAlarms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PriceAlarms](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [AssetCode] [nvarchar](50) NOT NULL,
        [TargetPrice] [decimal](18, 4) NOT NULL,
        [Direction] [nvarchar](20) NOT NULL,
        [IsActive] [bit] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [TriggeredAt] [datetime2](7) NULL,
        [IsAutoSell] [bit] NOT NULL DEFAULT (0),
        [CustomerId] [int] NOT NULL DEFAULT (0),
     CONSTRAINT [PK_PriceAlarms] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END";
            using var connection = Db.Create();
            connection.Execute(sql);
        }
    }
}
