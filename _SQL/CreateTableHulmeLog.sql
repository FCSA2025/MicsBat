USE [micsdev]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

-- Delete the table hulme.console if it already exists:
IF OBJECT_ID('hulme.log') IS NOT NULL DROP TABLE hulme.log

CREATE TABLE [hulme].[log](

    [dateTime] [DATETIME] NOT NULL,
	[identifier] [varchar](max) NULL,
	[text] [varchar](max) NULL
) 

GO

SET ANSI_PADDING OFF
GO