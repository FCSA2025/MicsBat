USE [fcsa]

------------------------------------------------------------------
-- The following T-SQL detects all mt_chan records whose
--
--		antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3
-- 
-- does not have a matching anum in its local mt_ante table.
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
	[chid] [char](4) NOT NULL,
	[freqtx] [float] NULL,
	[freqrx] [float] NULL,
	[antnumbtx1] [tinyint] NULL,
	[antnumbtx2] [tinyint] NULL,
	[antnumbrx1] [tinyint] NULL,
	[antnumbrx2] [tinyint] NULL,
	[antnumbrx3] [tinyint] NULL,
	[foundtx1] [tinyint] NULL,
	[foundtx2] [tinyint] NULL,
	[foundrx1] [tinyint] NULL,
	[foundrx2] [tinyint] NULL,
	[foundrx3] [tinyint] NULL,
	[in6A] [BIT] NULL,
	[in6D] [BIT] NULL,
	[in6E] [BIT] NULL,
 CONSTRAINT [PK_mt_chan_orphans_xyz] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[chid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
GO

INSERT INTO hulme.orphans
	SELECT call1, call2, bndcde, chid, freqtx, freqrx, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, 0, 0, 0, 0, 0, 0, 0, 0
	FROM fcsaSQL3.fcsa.main.mt_chan

UPDATE hulme.orphans
	SET	
		foundtx1 = CASE WHEN antnumbtx1 IS NULL THEN NULL ELSE 0 END,
		foundtx2 = CASE WHEN antnumbtx2 IS NULL THEN NULL ELSE 0 END,
		foundrx1 = CASE WHEN antnumbrx1 IS NULL THEN NULL ELSE 0 END,
		foundrx2 = CASE WHEN antnumbrx2 IS NULL THEN NULL ELSE 0 END,
		foundrx3 = CASE WHEN antnumbrx3 IS NULL THEN NULL ELSE 0 END
	FROM hulme.orphans

UPDATE hulme.orphans
	SET
		foundtx1 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbtx1 = A.anum

UPDATE hulme.orphans
	SET
		foundtx2 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbtx2 = A.anum

UPDATE hulme.orphans
	SET
		foundrx1 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbrx1 = A.anum

UPDATE hulme.orphans
	SET
		foundrx2 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbrx2 = A.anum

UPDATE hulme.orphans
	SET
		foundrx3 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbrx3 = A.anum

UPDATE hulme.orphans
	SET
		in6A = hulme.IsInRangeFloatTxRx(5850000, 6425000, T.freqtx, T.freqrx),
		in6D = hulme.IsInRangeFloatTxRx(6425000, 6930000, T.freqtx, T.freqrx),
		in6E = hulme.IsInRangeFloatTxRx(6590000, 7125000, T.freqtx, T.freqrx)
	FROM hulme.orphans AS T

-- Identify the mt_chan records that have no matching anum in the mt_ante table.
IF OBJECT_ID('hulme.orphansBefore') IS NOT NULL DROP TABLE hulme.orphansBefore

SELECT * INTO hulme.orphansBefore
	FROM hulme.orphans
	WHERE	(foundtx1 != antnumbtx1) OR (foundtx2 != antnumbtx2) OR (foundrx1 != antnumbrx1) OR (foundrx2 != antnumbrx2) OR (foundrx3 != antnumbrx3)

-- Display the 'before' findings.
SELECT * FROM hulme.orphansBefore ORDER BY call1, call2

--============================ BEFORE ^^^    AFTER vvv ===================

TRUNCATE TABLE hulme.orphans

-- Intentionally fault an mt_chan record to demonstrate that the test algorithm is correct.
UPDATE hulme.mt_chan
	SET antnumbrx3 = 255
	WHERE call1='=FAULT1' AND call2='=FAULT1' AND bndcde='11A' AND chid='1001'

INSERT INTO hulme.orphans
	SELECT call1, call2, bndcde, chid, freqtx, freqrx, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, 0, 0, 0, 0, 0, 0, 0, 0
	FROM hulme.mt_chan

UPDATE hulme.orphans
	SET	
		foundtx1 = CASE WHEN antnumbtx1 IS NULL THEN NULL ELSE 0 END,
		foundtx2 = CASE WHEN antnumbtx2 IS NULL THEN NULL ELSE 0 END,
		foundrx1 = CASE WHEN antnumbrx1 IS NULL THEN NULL ELSE 0 END,
		foundrx2 = CASE WHEN antnumbrx2 IS NULL THEN NULL ELSE 0 END,
		foundrx3 = CASE WHEN antnumbrx3 IS NULL THEN NULL ELSE 0 END
	FROM hulme.orphans

UPDATE hulme.orphans
	SET
		foundtx1 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbtx1 = A.anum

UPDATE hulme.orphans
	SET
		foundtx2 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbtx2 = A.anum

UPDATE hulme.orphans
	SET
		foundrx1 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbrx1 = A.anum

UPDATE hulme.orphans
	SET
		foundrx2 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbrx2 = A.anum

UPDATE hulme.orphans
	SET
		foundrx3 = A.anum
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1 AND A.call2=T.call2 AND A.bndcde=T.bndcde)
	WHERE T.antnumbrx3 = A.anum

UPDATE hulme.orphans
	SET
		in6A = hulme.IsInRangeFloatTxRx(5850000, 6425000, T.freqtx, T.freqrx),
		in6D = hulme.IsInRangeFloatTxRx(6425000, 6930000, T.freqtx, T.freqrx),
		in6E = hulme.IsInRangeFloatTxRx(6590000, 7125000, T.freqtx, T.freqrx)
	FROM hulme.orphans AS T

-- Identify any mt_chan records that have no matching anum in the mt_ante table.
IF OBJECT_ID('hulme.orphansAfter') IS NOT NULL DROP TABLE hulme.orphansAfter

SELECT * INTO hulme.orphansAfter
	FROM hulme.orphans
	WHERE	(foundtx1 != antnumbtx1) OR (foundtx2 != antnumbtx2) OR (foundrx1 != antnumbrx1) OR (foundrx2 != antnumbrx2) OR (foundrx3 != antnumbrx3)

-- Display the 'after' findings.
SELECT * FROM hulme.orphansAfter ORDER BY call1, call2

--=====================================================================================

--Identify the differences between the 'before' records and the 'after' records.
IF OBJECT_ID('hulme.orphansDelta') IS NOT NULL DROP TABLE hulme.orphansDelta
SELECT * INTO hulme.orphansDelta FROM hulme.orphansAfter

DELETE hulme.orphansDelta
	FROM hulme.orphansDelta AS D
    JOIN hulme.orphansBefore AS B ON (D.call1 = B.call1) AND (D.call2 = B.call2) AND (D.bndcde = B.bndcde) AND (D.chid = B.chid)

SELECT * FROM hulme.orphansDelta ORDER BY call1, call2

--SELECT * FROM hulme.orphansDelta WHERE bndcde = '6E'  ORDER BY call1, call2

-----------------------------------------------------------
-- Clean up.
-----------------------------------------------------------
DROP TABLE hulme.orphans
DROP TABLE hulme.orphansBefore
DROP TABLE hulme.orphansAfter
DROP TABLE hulme.orphansDelta
GO
