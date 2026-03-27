USE [fcsa]
GO

/****** Object:  Table [hulme].[TaflLicenseeNameToFcsaOper]    Script Date: 11/30/2022 9:48:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [hulme].[TaflLicenseeNameToFcsaOper](
	[MICSoper] [nvarchar](6) NOT NULL,
	[LicenseeName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_TaflLicenseeNameToFcsaOper] PRIMARY KEY CLUSTERED 
(
	[LicenseeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

