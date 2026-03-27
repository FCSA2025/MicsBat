-------------------------------------------------------------------
-- This T-SQL script makes a 'local' copy of the MDB table sd_band.
-------------------------------------------------------------------

USE [fcsa]
GO

/****** Object:  Table [main].[sd_band]    Script Date: 2/7/2022 1:32:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF OBJECT_ID('hulme.sd_band') IS NOT NULL DROP TABLE hulme.sd_band

CREATE TABLE [hulme].[sd_band](
	[bndcde] [char](4) NOT NULL,
	[bandbitpos] [smallint] NULL,
	[blo] [float] NULL,
	[bmidf] [float] NULL,
	[bhi] [float] NULL,
	[badj] [char](99) NULL,
	[mdate] [char](10) NULL,
	[mtime] [char](8) NULL,
 CONSTRAINT [PK_sd_band] PRIMARY KEY CLUSTERED 
(
	[bndcde] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO hulme.sd_band SELECT * FROM main.sd_band
GO

SET ANSI_PADDING OFF
GO

--SELECT * FROM hulme.sd_band ORDER BY blo
SELECT 'Local copy of sd_band created.'




