USE [fcsa]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF OBJECT_ID(N'hulme.ofm_feecalalltot',N'U') IS NOT NULL DROP TABLE hulme.ofm_feecalxreplace


	CREATE TABLE hulme.ofm_feecalalltot(
		[date] [date] NULL,
		[admin] [char](12) NULL,
		[oper] [char](6) NULL,
		[tstot] [int] NULL,
		[estot] [int] NOT NULL
	) ON [PRIMARY]
GO

CREATE TABLE hulme.ofm_feecalxreplace(
	[date] [date] NULL,
	[admin] [char](12) NULL,
	[oper] [char](6) NULL,
	[txfee] [decimal](18, 0) NULL,
	[rxfee] [decimal](18, 0) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
