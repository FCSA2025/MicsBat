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

IF OBJECT_ID('hulme.orphans') IS NOT NULL DROP TABLE hulme.orphans
CREATE TABLE [hulme].[orphans](
	[call1] [char](9) NOT NULL,
	[call2] [char](9) NOT NULL,
	[bndcde] [char](4) NOT NULL,
	[anum] [smallint] NOT NULL,
	[chid] [char](4) NULL
 CONSTRAINT [PK_mt_ante_orphans] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[anum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
GO

INSERT 	INTO hulme.orphans
	SELECT call1, call2, bndcde, anum, NULL
	FROM fcsaSQL3.fcsa.main.mt_ante

UPDATE hulme.orphans
	SET	chid = C.chid
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_chan AS C ON (C.call1=T.call1 AND C.call2=T.call2 AND C.bndcde=T.bndcde)
	WHERE (T.anum = C.antnumbtx1) OR (T.anum = C.antnumbtx2) OR (T.anum = C.antnumbrx1) OR (T.anum = C.antnumbrx2) OR (T.anum = C.antnumbrx3)

IF OBJECT_ID('hulme.orphansBefore') IS NOT NULL DROP TABLE hulme.orphansBefore
SELECT * INTO hulme.orphansBefore FROM hulme.orphans WHERE chid IS NULL

-- Display all the mt_ante records that have no associated mt_chan records.
SELECT * FROM hulme.orphansBefore


TRUNCATE TABLE hulme.orphans

INSERT 	INTO hulme.orphans
	SELECT call1, call2, bndcde, anum, NULL
	FROM hulme.mt_ante

-- Intentionally insert a faulty (has no channels) mt_ante record to demonstrate that the test algorithm is correct.
INSERT INTO hulme.orphans (call1, call2, bndcde, anum) 
	VALUES ('=FAULTY1', '=FAULTY1', 'ZZZ', '255')

UPDATE hulme.orphans
	SET	chid = C.chid
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_chan AS C ON (C.call1=T.call1 AND C.call2=T.call2 AND C.bndcde=T.bndcde)
	WHERE (T.anum = C.antnumbtx1) OR (T.anum = C.antnumbtx2) OR (T.anum = C.antnumbrx1) OR (T.anum = C.antnumbrx2) OR (T.anum = C.antnumbrx3)

IF OBJECT_ID('hulme.orphansAfter') IS NOT NULL DROP TABLE hulme.orphansAfter
SELECT * INTO hulme.orphansAfter FROM hulme.orphans WHERE chid IS NULL

-- Display all the mt_ante records that have no associated mt_chan records.
SELECT * FROM hulme.orphansAfter

IF OBJECT_ID('hulme.orphansDelta') IS NOT NULL DROP TABLE hulme.orphansDelta
SELECT * INTO hulme.orphansDelta FROM hulme.orphansAfter

DELETE hulme.orphansDelta
	FROM hulme.orphansDelta AS D
    JOIN hulme.orphansBefore AS B ON (D.call1 = B.call1) AND (D.call2 = B.call2) AND (D.bndcde = B.bndcde) AND (D.anum = B.anum)

SELECT * FROM hulme.orphansDelta

/*
SELECT oper, S.call1, call2, bndcde, anum,
		(CASE WHEN hulme.isMember(oper) = 1 THEN 'yes' ELSE 'no' END) AS isMember, 
		(CASE WHEN hulme.isCanadianProvince(prov) = 1 THEN 'yes' ELSE 'no' END) AS isCanadian,
		mdate, userid
	FROM hulme.orphansBefore AS B
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS S ON S.call1 = B.call1
	ORDER BY oper, call1
*/
-----------------------------------------------------------
-- Clean up.
-----------------------------------------------------------
DROP TABLE hulme.orphans
DROP TABLE hulme.orphansBefore
DROP TABLE hulme.orphansAfter
DROP TABLE hulme.orphansDelta
GO



