USE [fcsa]
GO

------------------------------------------------------------------
-- The following T-SQL detects all mt_ante records whose call1
-- does not present in mt_site.
------------------------------------------------------------------

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
	[call1Exists] [BIT] NOT NULL
 CONSTRAINT [PK_mt_site_orphans] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[anum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF

INSERT INTO hulme.orphans
	SELECT call1, call2, bndcde, anum, '0'
	FROM fcsaSQL3.fcsa.main.mt_ante

UPDATE hulme.orphans
	SET	call1Exists = 1
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansBefore') IS NOT NULL DROP TABLE hulme.orphansBefore
SELECT * INTO hulme.orphansBefore FROM hulme.orphans WHERE (call1Exists = 0)

-- Display all the mt_site records that have no associated mt_ante records.
SELECT * FROM hulme.orphansBefore

TRUNCATE TABLE hulme.orphans

INSERT INTO hulme.orphans
	SELECT call1, call2, bndcde, anum, '0'
	FROM hulme.mt_ante

-- Intentionally insert a faulty (has no site) mt_ante record to demonstrate that the test algorithm is correct.
INSERT INTO hulme.orphans (call1, call2, bndcde, anum, call1Exists ) 
	VALUES ('ZZZZZZ', 'AAAA', '99Z', '123', '0')

UPDATE hulme.orphans
	SET	call1Exists = 1
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansAfter') IS NOT NULL DROP TABLE hulme.orphansAfter
SELECT * INTO hulme.orphansAfter FROM hulme.orphans WHERE (call1Exists = 0)

-- Display all the mt_site records that have no associated mt_ante records.
SELECT * FROM hulme.orphansAfter

SELECT * INTO hulme.orphansDelta FROM hulme.orphansAfter 

DELETE hulme.orphansDelta
	FROM hulme.orphansDelta AS D
    JOIN hulme.orphansBefore AS B ON (D.call1 = B.call1)

SELECT * FROM hulme.orphansDelta

-----------------------------------------------------------
-- Clean up.
-----------------------------------------------------------
DROP TABLE hulme.orphans
DROP TABLE hulme.orphansBefore
DROP TABLE hulme.orphansAfter
DROP TABLE hulme.orphansDelta
GO
