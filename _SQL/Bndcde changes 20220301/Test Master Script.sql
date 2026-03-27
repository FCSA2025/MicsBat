USE fcsa
GO

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO

IF OBJECT_ID('hulme.testResults') IS NOT NULL DROP TABLE hulme.testResults
CREATE TABLE [hulme].[testResults](
	[testID] [int] NOT NULL DEFAULT 0,
	[testDesc] [char](500) NOT NULL DEFAULT 'TBD',
	[beforeCount] [int] NOT NULL DEFAULT 0,
	[afterCount] [int] NOT NULL DEFAULT 0,
	[changeBIT] [BIT] NOT NULL DEFAULT 0
 CONSTRAINT [PK_testResults] PRIMARY KEY CLUSTERED 
(
	[testDesc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

IF OBJECT_ID('hulme.orphans') IS NOT NULL DROP TABLE hulme.orphans
CREATE TABLE [hulme].[orphans](
	[call1] [char](9) NOT NULL,
	[call2] [char](9) NULL,
	[bndcde] [char](4) NULL,
	[anum] [smallint] NULL,
	[chid] [char](4) NULL,
	[flag] [BIT] DEFAULT 0
 CONSTRAINT [PK_mt_site_orphans] PRIMARY KEY CLUSTERED 
(
	[call1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
GO

DECLARE @countBefore INT;
DECLARE @countAfter INT;
DECLARE @testID INT = 0;

---------------------------------------------------------------------------------------------
-- Number of MDB antennas, before vs after.
---------------------------------------------------------------------------------------------
SET @testID = @testID + 1;
SET @countBefore = (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante);
SET @countAfter = (SELECT COUNT(*) FROM hulme.mt_ante);
INSERT INTO hulme.testResults SELECT 
	@testID, 'Number of MDB TS antennas.', @countBefore, @countAfter, (CASE WHEN (@countBefore = @countAfter) THEN 0 ELSE 1 END)

---------------------------------------------------------------------------------------------
-- Number of MDB TS channels, before vs after.
---------------------------------------------------------------------------------------------
SET @testID = @testID + 1;
SET @countBefore = (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan);
SET @countAfter = (SELECT COUNT(*) FROM hulme.mt_chan);
INSERT INTO hulme.testResults SELECT 
	@testID, 'Number of MDB TS channels.', @countBefore, @countAfter, (CASE WHEN (@countBefore = @countAfter) THEN 0 ELSE 1 END)

-------------------------------------------------------------------------
-- The number of TS sites that have no antenna.
-------------------------------------------------------------------------
SET @testID = @testID + 1;
TRUNCATE TABLE hulme.orphans
INSERT 	INTO hulme.orphans
	SELECT call1, NULL, NULL, NULL, NULL, 0
	FROM fcsaSQL3.fcsa.main.mt_site

UPDATE hulme.orphans
	SET	call2 = A.call2, bndcde = A.bndcde, anum = A.anum, flag = 1
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansBefore') IS NOT NULL DROP TABLE hulme.orphansBefore
SELECT * INTO hulme.orphansBefore FROM hulme.orphans WHERE (call2 IS NULL)

TRUNCATE TABLE hulme.orphans
INSERT 	INTO hulme.orphans
	SELECT call1, NULL, NULL, NULL, NULL, 0
	FROM fcsaSQL3.fcsa.main.mt_site

-- Intentionally insert a faulty (has no antennas) mt_site record to demonstrate that the test algorithm is correct.
--INSERT INTO hulme.orphans (call1, call2, bndcde, anum) VALUES ('ZZZZZZ', NULL, NULL, NULL)

UPDATE hulme.orphans
	SET	call2 = A.call2, bndcde = A.bndcde, anum = A.anum, flag = 1
	FROM hulme.orphans AS T
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansAfter') IS NOT NULL DROP TABLE hulme.orphansAfter
SELECT * INTO hulme.orphansAfter FROM hulme.orphans WHERE call2 IS NULL

SET @countBefore = (SELECT COUNT(*) FROM hulme.orphansbefore);
SET @countAfter = (SELECT COUNT(*) FROM hulme.orphansAfter);
INSERT INTO hulme.testResults SELECT 
	@testID, 'Number of TS sites that have no antenna.', @countBefore, @countAfter, (CASE WHEN (@countBefore = @countAfter) THEN 0 ELSE 1 END)

-------------------------------------------------------------------------
-- The number of TS antennas that have no site.
-------------------------------------------------------------------------
SET @testID = @testID + 1;
TRUNCATE TABLE hulme.orphans
INSERT INTO hulme.orphans
	SELECT call1, NULL, NULL, NULL, NULL, 0
	FROM fcsaSQL3.fcsa.main.mt_ante
	GROUP BY call1

UPDATE hulme.orphans
	SET	flag = 1
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansBefore') IS NOT NULL DROP TABLE hulme.orphansBefore
SELECT * INTO hulme.orphansBefore FROM hulme.orphans WHERE (flag != 1)

TRUNCATE TABLE hulme.orphans
INSERT INTO hulme.orphans
	SELECT call1, NULL, NULL, NULL, NULL, 0
	FROM hulme.mt_ante
	GROUP BY call1

-- Intentionally insert a faulty (has no site) mt_ante record to demonstrate that the test algorithm is correct.
--INSERT INTO hulme.orphans (call1, call2, bndcde, anum, call1Exists ) VALUES ('ZZZZZZ', 'AAAA', '99Z', '123', '0')

UPDATE hulme.orphans
	SET	flag = 1
	FROM hulme.orphans AS T
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS A ON (A.call1=T.call1)

IF OBJECT_ID('hulme.orphansAfter') IS NOT NULL DROP TABLE hulme.orphansAfter
SELECT * INTO hulme.orphansAfter FROM hulme.orphans WHERE (flag != 1)

SET @countBefore = (SELECT COUNT(*) FROM hulme.orphansbefore);
SET @countAfter = (SELECT COUNT(*) FROM hulme.orphansAfter);
INSERT INTO hulme.testResults SELECT 
	@testID, 'Number of TS antennas that have no site.', @countBefore, @countAfter, (CASE WHEN (@countBefore = @countAfter) THEN 0 ELSE 1 END)




-----------------------------------------------------------
-- Clean up.
-----------------------------------------------------------
DROP TABLE hulme.orphans
DROP TABLE hulme.orphansBefore
DROP TABLE hulme.orphansAfter




SELECT * FROM hulme.testResults ORDER BY testID