------------------------------------------------------------------
-- The following T-SQL detects all mt_site records that have no
-- associated mt_ante records.
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
	[call2] [char](9) NULL,
	[bndcde] [char](4) NULL,
	[anum] [smallint] NULL,
	[chid] [char](4) NULL
 CONSTRAINT [PK_mt_site_orphans] PRIMARY KEY CLUSTERED 
(
	[call1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
GO

INSERT 	INTO hulme.orphans
	SELECT call1, NULL, NULL, NULL, NULL
	FROM fcsaSQL3.fcsa.main.mt_site

UPDATE hulme.orphans
	SET	call2 = A.call2, bndcde = A.bndcde, anum = A.anum
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansBefore') IS NOT NULL DROP TABLE hulme.orphansBefore
SELECT * INTO hulme.orphansBefore FROM hulme.orphans WHERE (call2 IS NULL)

-- Display all the mt_site records that have no associated mt_ante records.
SELECT * FROM hulme.orphansBefore

TRUNCATE TABLE hulme.orphans

INSERT 	INTO hulme.orphans
	SELECT call1, NULL, NULL, NULL, NULL
	FROM fcsaSQL3.fcsa.main.mt_site

-- Intentionally insert a faulty (has no antennas) mt_site record to demonstrate that the test algorithm is correct.
INSERT INTO hulme.orphans (call1, call2, bndcde, anum) 
	VALUES ('ZZZZZZ', NULL, NULL, NULL)

UPDATE hulme.orphans
	SET	call2 = A.call2, bndcde = A.bndcde, anum = A.anum
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansAfter') IS NOT NULL DROP TABLE hulme.orphansAfter
SELECT * INTO hulme.orphansAfter FROM hulme.orphans WHERE call2 IS NULL

-- Display all the mt_site records that have no associated mt_ante records.
SELECT * FROM hulme.orphansAfter

IF OBJECT_ID('hulme.orphansDelta') IS NOT NULL DROP TABLE hulme.orphansDelta
SELECT * INTO hulme.orphansDelta FROM hulme.orphansAfter

DELETE hulme.orphansDelta
	FROM hulme.orphansDelta AS D
    JOIN hulme.orphansBefore AS B ON (D.call1 = B.call1)

SELECT * FROM hulme.orphansDelta

/*
   SELECT S.call1, S.oper,
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



