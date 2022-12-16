USE [SpeedWebAPI]
GO

/****** Object:  Table [dbo].[SpeedLimit]    Script Date: 12/15/2022 2:15:42 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpeedLimit]') AND type in (N'U'))
DROP TABLE [dbo].[SpeedLimit]
GO

/****** Object:  Table [dbo].[SpeedLimit]    Script Date: 12/15/2022 2:15:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SpeedLimit](
	[Lat] [decimal](18, 10) NOT NULL,
	[Lng] [decimal](18, 10) NOT NULL,
	[ProviderType] [int] NOT NULL,
	[Position] [nvarchar](50) NOT NULL,
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
	[UpdateCount] [int] NULL,
 CONSTRAINT [PK_SpeedLimit] PRIMARY KEY CLUSTERED 
(
	[Lat] ASC,
	[Lng] ASC,
	[ProviderType] ASC,
	[Position] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


