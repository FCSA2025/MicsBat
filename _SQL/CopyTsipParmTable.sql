USE [micsdev]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF OBJECT_ID('micsproddev.hulme.tp_fil599v1_parm') IS NOT NULL DROP TABLE micsproddev.hulme.tp_fil599v1_parm
CREATE TABLE micsproddev.hulme.tp_fil599v1_parm(
	[protype] [char](1) NULL,
	[envtype] [char](8) NULL,
	[proname] [char](16) NULL,
	[envname] [char](16) NULL,
	[tsorbout] [char](1) NULL,
	[spherecalc] [char](1) NULL,
	[fsep] [float] NULL,
	[coordist] [float] NULL,
	[analopt] [char](4) NULL,
	[margin] [float] NULL,
	[numchan] [smallint] NULL,
	[chancodes] [char](19) NULL,
	[country] [char](3) NULL,
	[selsites] [char](15) NULL,
	[numcodes] [smallint] NULL,
	[codes] [char](164) NULL,
	[runname] [char](5) NOT NULL,
	[reports] [int] NULL,
	[numcases] [int] NULL,
	[numtecases] [int] NULL,
	[parmparm] [char](50) NULL,
	[mdate] [char](10) NULL,
	[mtime] [char](8) NULL,
 CONSTRAINT [PK_tp_fil599v1_parm] PRIMARY KEY CLUSTERED 
(
	[runname] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

INSERT INTO micsproddev.hulme.tp_fil599v1_parm SELECT * FROM micsdev.hulme.tp_fil599v1_parm
GO

INSERT INTO micsproddev.web.user_tables
	VALUES ('hulme', '417', 'fil599v1', 'hulme1', 'hulme1_0', 'N', GETDATE())

