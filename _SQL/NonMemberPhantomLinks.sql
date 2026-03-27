--==============================================================================================================
-- The purpose of this T-SQL script is, for every TS Tx Link in the MDB belonging to an FCSA member, to
-- search the ISED/TAFL data looking for a 'reasonable' match on the Link parameters 'local site location',
-- 'azimuth' and 'Tx frequency'.
--
-- There are two essential inputs to this script:
--      [a] a table providing a full set of TAFL import data; and
--      [b] the extant MDB tables.
--
-- The specific function of this script is to identify, and perform a 'problem analysis', on every FCSA
-- member MDB Tx Link that has no reasonably-matching counterpart in the ISED/TAFL data - AKA a 'phantom' Link.
--
-- When all of the member's phantom MDB links have been identified and analysed individual tables of phantom
-- Tx Links are created for each member.
--
-- Before running this script, find and replace the following string to update the TAFL source table name
-- to the most recent version:
--
--                                  hulme.IsedTafl_20221207
--
--==============================================================================================================

USE fcsa;

-- Create and populate a table containing relevant MDB TS TX Link-end data.
-- Also, create columns for later use in the  MDB-to-ISED Link matching analysis.
IF OBJECT_ID('hulme.mdbTxLinkAnalysis') IS NOT NULL DROP TABLE hulme.mdbTxLinkAnalysis

CREATE TABLE hulme.mdbTxLinkAnalysis(
    [problemCode] CHAR(1) NULL,			-- This is a single character 'problem code' (L, A or F) that gives the FCSA member
	                                    -- some information about the nature of the Phantom Tx Link. A NULL value indicates
										-- that this MDB Tx Link has a reasonable match in the ISED/TAFL data.
	[oper] [char](6) NULL,				-- mt_site.oper
    [call1] [char](9) NOT NULL,			-- mt_site.call1
	[call2] [char](9) NOT NULL,			-- mt.ante.call2
	[bndcde] [char](4) NOT NULL,		-- mt_ante.bndcde
	[chid] [char](4) NOT NULL,			-- mt_chan.chid
	[anum] [smallint] NULL,				-- mt_ante.anum (= mt_chan.antnumbtx1 for a Tx Link)
	[mdbLat] [float] NULL,				-- mt_site.latit  / 36000.0
    [mdbLng] [float] NULL,				-- mt_site.longit / 36000.0
	[prov] [char](2) NULL,				-- mt_site.prov
	[azmth] [real] NULL,				-- mt.ante.azmth
	[freqtx] [float] NULL,				-- mt_chan.freqtx
	[aht] [real] NULL,					-- mt_ante.aht
	[locationHit] [BIT] NULL,			-- bit flag: 1 = 'reasonable location match in ISED/TAFL data'; otherwise 0
	[azimuthHit] [BIT] NULL,			-- bit flag: 1 = 'reasonable location and azimuth match in ISED/TAFL data'; otherwise 0	
	[taflKeyField] [int] NULL,			-- the Venn keyfield column value in the ISED/TAFL table; a NULL indicates 'no match found'
 CONSTRAINT [PK_ft_zayodata_chan] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[chid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO hulme.mdbTxLinkAnalysis
	SELECT DISTINCT
		NULL,
		S.oper,
		C.call1, C.call2, C.bndcde, C.chid, C.antnumbtx1,
		S.latit/360000.0, -S.longit/360000.0,              -- Note that longitudes are negative, same as TAFL.
		S.prov,
		A.azmth, C.freqtx, A.aht,
		NULL, NULL, NULL
FROM main.mt_chan as C
INNER JOIN main.mt_ante as A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbtx1) AND (A.ause IN ('TX', 'TR'))
INNER JOIN main.mt_site as S ON (S.call1 = C.call1) AND (S.oprtyp = 'CT')
--WHERE S.oper = 'ABCCOM'

-- The following query performs the MDB TS TX Link-end to ISED/TAFL TX record matching.
-- The matching criteria is that location/azimuth/TxFrequency components agree within prescribed tolerances.
-- This query takes about 9 minutes to complete.
-- It is more efficient to perform the location matching last as it is the most computationally expensive.
-- If no TX Link-end match is found in the ISED data then 'keyfield' will be NULL.
-- Remember to update the name of the ISED data table to be used, e.g. hulme.IsedTafl_20221207
UPDATE hulme.mdbTxLinkAnalysis
SET 
	taflKeyField = T.keyfield
FROM 
	hulme.mdbTxLinkAnalysis AS L
	LEFT OUTER JOIN hulme.IsedTafl_20221207 AS T ON
	        ABS(0.001*L.freqtx - T.FrequencyMhz) < 2
		AND ABS(hulme.azimuth_delta_deg(L.azmth, T.Azimuthofmainlobedeg)) < 3.0
		AND ([hulme].[distance_less_than_deg](L.mdblat, L.mdblng, T.LatitudeWGS84, T.LongitudeWGS84, 0.300) = 1)
		AND T.TXRX = 'TX'

-- Create and populate a table that counts 'good' MDB TS Tx Link-ends,
-- rolled-up on a per-operator basis.
-- A 'good' MDB TS TX Link-end is one that has a match in the IESD/TAFL data,
-- i.e. 'keyfield' is not NULL.
IF OBJECT_ID('hulme.links_vs_tafl_by_oper') IS NOT NULL DROP TABLE hulme.links_vs_tafl_by_oper;
SELECT 
		oper, 
		COUNT(oper) AS totalLinks,
		COUNT(taflKeyField) AS goodLinks,
		CONVERT(INT,0) AS badLinks,
		CONVERT(FLOAT,0) AS badLinks_pc
	INTO hulme.links_vs_tafl_by_oper
	FROM hulme.mdbTxLinkAnalysis --WHERE LEFT(call1, 1) NOT IN ('=', '$', '%', ';')
	GROUP BY oper
	ORDER BY oper;

-- Now calculate the number of 'bad' Links per operator, both as an absolute
-- count and as a percentage of the total number of Links.
UPDATE hulme.links_vs_tafl_by_oper
	SET
		badLinks = totalLinks - goodLinks 
	FROM hulme.links_vs_tafl_by_oper
UPDATE hulme.links_vs_tafl_by_oper
	SET
		badLinks_pc = 100.0 * (CONVERT(FLOAT, badLinks)/CONVERT(FLOAT, totalLinks))
	FROM hulme.links_vs_tafl_by_oper

-- Test whether there is a location 'hit' for each of the 'bad' Links.
-- This query takes 15 minutes to complete.
-- Remember to update the name of the ISED data table to be used, e.g. hulme.IsedTafl_20221207
UPDATE hulme.mdbTxLinkAnalysis 
	SET 
		locationHit = 1
	FROM hulme.mdbTxLinkAnalysis AS L
	INNER JOIN hulme.IsedTafl_20221207 AS T ON
		([hulme].[distance_less_than_deg](L.mdblat, L.mdblng, T.LatitudeWGS84, T.LongitudeWGS84, 0.300) = 1)
    WHERE (L.taflKeyField IS NULL) AND (L.oper IS NOT NULL)

-- Test whether there is a location and azimuth 'hit' for each of the 'bad' Links.
-- This query takes 24 minutes to complete.
-- Remember to update the name of the ISED data table to be used, e.g. hulme.IsedTafl_20221207
UPDATE hulme.mdbTxLinkAnalysis
	SET 
		azimuthHit = 1
	FROM hulme.mdbTxLinkAnalysis AS L
	INNER JOIN hulme.IsedTafl_20221207 AS T ON
			ABS(hulme.azimuth_delta_deg(L.azmth, T.Azimuthofmainlobedeg)) < 3.0
		AND ([hulme].[distance_less_than_deg](L.mdblat, L.mdblng, T.LatitudeWGS84, T.LongitudeWGS84, 0.300) = 1)
	WHERE 
			L.locationHit = 1
		AND (L.taflKeyField IS NULL) AND (L.oper IS NOT NULL)

UPDATE hulme.member_bad_links SET summary = NULL FROM hulme.member_bad_links

-- Given the previous 'hit' results, provide a textual summary for each 'bad' Link.
UPDATE hulme.mdbTxLinkAnalysis 
	SET 
		problemCode =	CASE
							WHEN (locationHit IS NULL) THEN 'L'                        /* No location match. */
							WHEN ((locationHit = 1) AND (azimuthHit IS NULL)) THEN 'A' /* Location match but no azimuth match. */
							WHEN ((locationHit = 1) AND (azimuthHit = 1)) THEN 'F'     /* Location and azimuth match but no frequency match. */
						END
	FROM hulme.mdbTxLinkAnalysis AS L
	WHERE (L.taflKeyField IS NULL) AND (L.oper IS NOT NULL)

-- Create a table to fill with 'bad' link analysis data, rolled up by operator.
IF OBJECT_ID('hulme.member_bad_links_by_oper') IS NOT NULL DROP TABLE hulme.member_bad_links_by_oper;
SELECT 
		L.oper, 
		COUNT(L.oper) AS totalBadLinks,
		SUM(CASE WHEN (L.problemCode = 'L') THEN 1 ELSE 0 END) AS badLocation,
		SUM(CASE WHEN (L.problemCode = 'A') THEN 1 ELSE 0 END) AS badAzimuth,
		SUM(CASE WHEN (L.problemCode = 'F') THEN 1 ELSE 0 END) AS badFrequency
	INTO hulme.member_bad_links_by_oper
	FROM hulme.mdbTxLinkAnalysis AS L --WHERE LEFT(call1, 1) NOT IN ('=', '$', '%', ';')
	WHERE (L.taflKeyField IS NULL) AND (L.oper IS NOT NULL)
	GROUP BY oper
	ORDER BY oper;

SELECT * FROM hulme.member_bad_links_by_oper ORDER BY oper

-- Provide a summary of totalMDBlinks, matchedMDBlinks and phantomMDBlinks rolled-up by operator.
IF OBJECT_ID('hulme.phantomRollUpByOper') IS NOT NULL DROP TABLE hulme.phantomRollUpByOper

CREATE TABLE hulme.phantomRollUpByOper(
	[oper] [char](6) NOT NULL,				
    [totalMDBLinks] [int] NULL,
	[matchingMDBLinks] [int] NULL,
	[phantomMDBLinks] [int] NULL,
 CONSTRAINT [PK_AH20221026A] PRIMARY KEY CLUSTERED 
(
	[oper] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT INTO hulme.phantomRollUpByOper
	(oper, totalMDBLinks, matchingMDBLinks)
SELECT oper, COUNT(*), SUM(CASE WHEN problemCode IS NULL THEN 1 ELSE 0 END)
	FROM hulme.mdbTxLinkAnalysis GROUP BY oper

UPDATE hulme.phantomRollUpByOper
	SET phantomMDBLinks = totalMDBLinks - matchingMDBLinks

SELECT * FROM hulme.phantomRollUpByOper


SELECT SUM(totalMDBlinks), SUM(phantomMDBLinks) FROM hulme.phantomRollUpByOper

SELECT * FROM hulme.mdbTxLinkAnalysis WHERE problemCode != 'NULL'
