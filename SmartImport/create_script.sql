USE [DBName]
GO

/****** Object:  Table [dbo].[ImportSource]    Script Date: 6/6/2017 1:46:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportSource](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NOT NULL,
	[Source] [varchar](max) NOT NULL,
	[Desto] [varchar](max) NOT NULL,
	[MoveProc] [varchar](max) NOT NULL,
	[archiveLocation] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO



/****** Object:  Table [dbo].[ImportSourceErrorLog]    Script Date: 6/6/2017 1:46:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportSourceErrorLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[dt] [datetime] NOT NULL,
	[Error] [varchar](500) NOT NULL,
	[Filename] [varchar](500) NULL
) ON [PRIMARY]

GO



/****** Object:  Table [dbo].[ImportSourceLog]    Script Date: 6/6/2017 1:47:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportSourceLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DateRun] [datetime] NOT NULL,
	[Message] [varchar](500) NOT NULL,
	[Filename] [varchar](250) NOT NULL
) ON [PRIMARY]

GO




USE [ImportData]
GO

/****** Object:  Table [dbo].[ImportTriggers]    Script Date: 7/11/2017 10:51:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportTriggers](
	[ImportStart] [datetime] NOT NULL
) ON [PRIMARY]

GO

