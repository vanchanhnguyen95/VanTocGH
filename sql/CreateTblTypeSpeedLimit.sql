--USE [Dev_SpeedWebAPI]
--GO

/****** Object:  UserDefinedTableType [dbo].[SpeedLimit]    Script Date: 12/15/2022 2:20:20 PM ******/
--DROP TYPE [dbo].[SpeedLimit]
--GO

/****** Object:  UserDefinedTableType [dbo].[SpeedLimit]    Script Date: 12/15/2022 2:20:20 PM ******/
CREATE TYPE [dbo].[SpeedLimit] AS TABLE(
	[Lat] [decimal](18, 10) NOT NULL,
	[Lng] [decimal](18, 10) NOT NULL,
	[ProviderType] [int] NOT NULL,
	[Position] [nvarchar](50) NULL,
	[MinSpeed] [int] NULL,
	[MaxSpeed] [int] NULL,
	[PointError] [bit] NULL,
	[SegmentID] [bigint] NULL,
	[IsUpdateSpeed] [bit] NULL,
	[CreatedDate] [datetime2](7) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[DeleteFlag] [int] NULL,
	[UpdateCount] [int] NULL
)
GO


