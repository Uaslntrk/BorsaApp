USE BorsaAppDb;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceAlarms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PriceAlarms](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [AssetCode] [nvarchar](50) NOT NULL,
        [TargetPrice] [decimal](18, 4) NOT NULL,
        [Direction] [nvarchar](20) NOT NULL, -- 'Above' or 'Below'
        [IsActive] [bit] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [TriggeredAt] [datetime2](7) NULL,
     CONSTRAINT [PK_PriceAlarms] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )
    )
END
GO
