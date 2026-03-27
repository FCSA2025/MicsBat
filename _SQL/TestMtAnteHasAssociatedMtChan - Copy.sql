USE [fcsa]

------------------------------------------------------------------
-- The following T-SQL detects all mt_ante records that have no
-- associated mt_chan records.
------------------------------------------------------------------

USE [fcsa]
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO

IF OBJECT_ID('hulme.temp') IS NOT NULL DROP TABLE hulme.temp

CREATE TABLE [hulme].[temp](
	[call1] [char](9) NOT NULL,
	[call2] [char](9) NOT NULL,
	[bndcde] [char](4) NOT NULL,
	[anum] [smallint] NOT NULL,
	[chid] [char](4) NULL
 CONSTRAINT [PK_mt_ante_abcdef] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[anum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
GO

INSERT INTO hulme.temp SELECT call1, call2, bndcde, anum, NULL FROM hulme.mt_ante

UPDATE hulme.temp
	SET
		chid = C.chid
	FROM hulme.temp AS T
	INNER JOIN hulme.mt_chan AS C ON (C.call1=T.call1 AND C.call2=T.call2 AND C.bndcde=T.bndcde)
	WHERE (T.anum = C.antnumbtx1) OR (T.anum = C.antnumbtx2) OR (T.anum = C.antnumbrx1) OR (T.anum = C.antnumbrx2) OR (T.anum = C.antnumbrx3)

-- Display all the mt_ante records that have no associated mt_chan records.
SELECT * FROM hulme.temp WHERE chid IS NULL

