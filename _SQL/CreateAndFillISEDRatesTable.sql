USE [fcsa]
GO

DROP TABLE [hulme].[ised_base_rates]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [hulme].[ised_base_rates](
	[freqMHzLo] [real] NOT NULL,
	[freqMHzHi] [real] NOT NULL,
	[urban_$perMHz] [money] NOT NULL,
	[rural_$perMHz] [money] NOT NULL,
	[remote_$perMHz] [money] NOT NULL,

	CONSTRAINT [PK_ised_base_rates] PRIMARY KEY CLUSTERED 
	(
		[freqMHzLo] ASC,
		[freqMHzHi] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO [hulme].[ised_base_rates]
	(freqMHzLo, freqMHzHi, urban_$perMHz, rural_$perMHz, remote_$perMHz)
	VALUES
		(0, 890, 2750.00, 2200.00, 1375.00),
		(890, 960, 138.00, 110.40, 69.00),
		(960, 4200, 45.00, 36.00, 22.50),
		(4200, 8500, 34.00, 27.20, 17.00),
		(8500, 15350, 24.00, 19.2, 12.00),
		(15350, 24250, 16.00, 12.80, 8.00),
		(24250, 52600, 10.00, 8.00, 5.00),
		(52600, 92000, 0.50, 0.40, 0.25),
		(92000, 3.40E+38, 0.50, 0.40, 0.25)

SET ANSI_PADDING OFF
GO
