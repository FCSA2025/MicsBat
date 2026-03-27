USE [fcsa]
GO

/****** Object:  Table [acct].[daily_storage]    Script Date: 11/4/2022 6:44:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [acct].[daily_storage](
	[storage_date] [date] NOT NULL,
	[operator] [char](8) NOT NULL,
	[micsid] [char](10) NOT NULL,
	[project_code] [char](10) NOT NULL,
	[filestorage] [float] NULL,
	[mdbstorage] [float] NULL,
	[operastorage] [float] NULL,
	[sitemdbstorage] [float] NULL,
	[siteoperastorage] [float] NULL,
	[chanmdbstorage] [float] NULL,
	[chanoperastorage] [float] NULL,
	[anazmdbstorage] [float] NULL,
	[anazoperastorage] [float] NULL,
	[miscoperastorage] [float] NULL,
	[create_time] [datetime] NULL,
 CONSTRAINT [PK_daily_storage] PRIMARY KEY CLUSTERED 
(
	[storage_date] ASC,
	[operator] ASC,
	[micsid] ASC,
	[project_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO


SELECT * FROM fcsaSQL3.[fcsa].[acct].[filestorage_temp]
SELECT * FROM fcsaSQL3.[fcsa].[acct].[daily_storage] ORDER BY storage_date DESC, operator, micsid

SELECT CONVERT(VARBINARY(1), SUBSTRING(line, 1, 1)), CONVERT(VARBINARY(1), SUBSTRING(line, 2, 1)), CONVERT(VARBINARY(1), SUBSTRING(line, 3, 1)),
	CASE WHEN ((SUBSTRING(line, 1, 1) = 0xEF) AND (SUBSTRING(line, 2, 1) = 0xBB) AND (SUBSTRING(line, 3, 1) = 0xBF)) THEN RIGHT(line, LEN(line) - 3) ELSE line END
	from
		hulme.IOuftReadfileAsTable('\\10.1.1.46\D$\Users\ahulme\temp\Bill_WebMICS\DailyStorage','Program.cs')
		where lineNum = 1
--where line doesnt begin with a hash

DECLARE @filePath  VARCHAR(MAX) = '\\10.1.1.46\D$\Users\ahulme\temp\Bill_WebMICS\DailyStorage\Program.cs';
SELECT lineNum, line FROM hulme.IOreadfileAsTableFP(@filePath)

DECLARE @dir  VARCHAR(MAX)     = '\\10.1.1.46\D$\Users\ahulme\temp';
DECLARE @file VARCHAR(MAX)     = 'temp.txt'
DECLARE @filePathX VARCHAR(MAX) = '\\10.1.1.46\D$\Users\ahulme\temp\myFile.txt';
DECLARE @LF CHAR(1) = CHAR(10)
DECLARE @message VARCHAR(MAX) = CONCAT('Hello World!', @LF, '2nd line.')
EXECUTE hulme.IOwriteStringToFileFP @message, @filePathX
DECLARE @message2 VARCHAR(MAX) = CONCAT(@LF, 'Alpha', @LF, 'Bravo.')
EXECUTE hulme.IOwriteStringToFileFPappend @message2, @filePathX

EXEC sp_configure 'Ole Automation Procedures';
GO

sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'Ole Automation Procedures', 1;
GO
RECONFIGURE;
GO

SELECT * FROM fcsaSQL3.fcsa.acct.daily_storage ORDER BY storage_date DESC

