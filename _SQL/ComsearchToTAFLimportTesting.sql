SELECT * FROM fcsa.web.user_tables WHERE file_name = 'isedess22b' AND micsID = 'hulme1'

SELECT * FROM FCSASQL3.fcsa.main.mt_site


SELECT SCHEMA_NAME(SCHEMA_ID), name FROM fcsaSQL3.fcsa.sys.tables  WHERE SCHEMA_NAME(SCHEMA_ID) LIKE '%venn%' ORDER BY name --name LIKE '%comsou%'
SELECT SCHEMA_NAME(SCHEMA_ID), name FROM fcsaSQL5D.fcsa.sys.tables WHERE SCHEMA_NAME(SCHEMA_ID) LIKE '%venn%' ORDER BY name --name LIKE '%comsou%'

SELECT SCHEMA_NAME(SCHEMA_ID), name FROM fcsaSQL3.fcsa.sys.tables  WHERE name LIKE '%comsou%' ORDER BY name
SELECT SCHEMA_NAME(SCHEMA_ID), name FROM fcsaSQL5D.fcsa.sys.tables WHERE name LIKE '%comsou%' ORDER BY name
SELECT SCHEMA_NAME(SCHEMA_ID), name FROM fcsaSQL5D.test.sys.tables WHERE name LIKE '%comsou%' ORDER BY name

SELECT * FROM test.venn.ft_comsoutC22A_titl
SELECT * FROM test.venn.ft_comsoutC22A_site ORDER BY latit, longit
SELECT * FROM test.venn.ft_comsoutC22A_ante
SELECT * FROM test.venn.ft_comsoutC22A_chan

SELECT * FROM hulme.ft_comsearch_titl
SELECT * FROM hulme.ft_comsearch_site
SELECT * FROM hulme.ft_comsearch_ante
SELECT * FROM hulme.ft_comsearch_chan

SELECT * 
	FROM hulme.ft_comsearch_ante AS A
	INNER JOIN hulme.ft_comsearch_site AS S ON (S.call1 = A.call1)
	WHERE S.stats = '5'
	ORDER BY A.call1, call2, bndcde, anum

SELECT * 
	FROM hulme.ft_comsearch_chan AS C
	INNER JOIN hulme.ft_comsearch_site AS S ON (S.call1 = C.call1)
	WHERE S.stats = '5'
	ORDER BY C.call1, call2, bndcde, chid

SELECT * FROM test.venn.ft_comsoutC22A_site ORDER BY name --latit, longit
SELECT * FROM hulme.ft_comsearch_site ORDER BY name--latit, longit
SELECT * FROM fcsa.hulme.comsearch
IF OBJECT_ID('hulme.comsearch') IS NOT NULL DROP TABLE hulme.comsearch
CREATE TABLE fcsa.hulme.comsearch([pathID] [int] NULL, [regID] [varchar](MAX) NULL, [pathStat] [varchar](MAX) NULL, [regdate] [varchar](MAX) NULL, [conDate] [varchar](MAX) NULL, [site1] [varchar](MAX) NULL, [state1] [varchar](MAX) NULL, [location1] [varchar](MAX) NULL, [site2] [varchar](MAX) NULL, [state2] [varchar](MAX) NULL, [location2] [varchar](MAX) NULL, [callsign] [varchar](MAX) NULL, [compName] [varchar](MAX) NULL, [lat1] [float] NULL, [lng1] [float] NULL, [lat2] [float] NULL, [lng2] [float] NULL, [pathLen] [float] NULL, [grndElev1] [float] NULL, [grndElev2] [float] NULL, [antMfr1] [varchar](MAX) NULL, [antMod1] [varchar](MAX) NULL, [gain1] [float] NULL, [bw1] [float] NULL, [rcagl1] [float] NULL, [antMfr2] [varchar](MAX) NULL, [antMod2] [varchar](MAX) NULL, [gain2] [float] NULL, [bw2] [float] NULL, [rcagl2] [float] NULL, [eqptMfr1] [varchar](MAX) NULL, [eqptMod1] [varchar](MAX) NULL, [eqptModDesc1] [varchar](MAX) NULL, [emDes1] [varchar](MAX) NULL, [mod1] [varchar](MAX) NULL, [acmMinMod1] [varchar](MAX) NULL, [acmMaxMod1] [varchar](MAX) NULL, [dataRate1] [float] NULL, [eqptMfr2] [varchar](MAX) NULL, [eqptMod2] [varchar](MAX) NULL, [eqptModDesc2] [varchar](MAX) NULL, [emDes2] [varchar](MAX) NULL, [mod2] [varchar](MAX) NULL, [acmMinMod2] [varchar](MAX) NULL, [acmMaxMod2] [varchar](MAX) NULL, [dataRate2] [float] NULL, [atpcNomPwr1] [float] NULL, [pwr1] [float] NULL, [atpcMaxPwr1] [float] NULL, [acmMinModPwr1] [float] NULL, [acmMaxModPwr1] [float] NULL, [atpcNomPwr2] [float] NULL, [pwr2] [float] NULL, [atpcMaxPwr2] [float] NULL, [acmMinModPwr2] [float] NULL, [acmMaxModPwr2] [float] NULL, [txLoss1] [float] NULL, [rxLoss1] [float] NULL, [cmnLoss1] [float] NULL, [txLoss2] [float] NULL, [rxLoss2] [float] NULL, [cmnLoss2] [float] NULL, [atpcNomRSL1] [float] NULL, [selectRSL1] [float] NULL, [atpcMaxRSL1] [float] NULL, [acmMinModRSL1] [float] NULL, [acmMaxModRSL1] [float] NULL, [atpcNomRSL2] [float] NULL, [selectRSL2] [float] NULL, [atpcMaxRSL2] [float] NULL, [acmMinModRSL2] [float] NULL, [acmMaxModRSL2] [float] NULL, [centerFreq1] [float] NULL, [polar1] [varchar](MAX) NULL, [centerFreq2] [float] NULL, [polar2] [varchar](MAX) NULL, [uniqueID] [int] NOT NULL, CONSTRAINT[PK_TBD] PRIMARY KEY CLUSTERED ([uniqueID] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]) ON[PRIMARY]

------------------------------------------------------------------------------------------

-- Prescribe the Lat/Lng degrees that we are filtering on.
DECLARE @latDeg FLOAT = 42.392861;
DECLARE @lngDeg FLOAT = -83.146278;
--DECLARE @latDeg FLOAT = 42.0935;
--DECLARE @lngDeg FLOAT = -80.13;

-- Prescribe the tolerance (degrees) of the Lat/Lng match.
DECLARE @TOL FLOAT = 0.00001;

DECLARE @DEGTOCS FLOAT = 360000.0;

SELECT 'Lat/Lng converted to 1/100 ths of seconds of arc ->', @latDeg AS lat, @lngDeg AS lng, ROUND(@latDeg*@DEGTOCS, 0) AS latit, ROUND(-@lngDeg*@DEGTOCS, 0) AS longit

SELECT 'RAW RECORD ->', pathID, callsign, site1, site2, lat1, lng1, rcagl1 AS aht1, lat2, lng2, rcagl2 AS aht2, conDate AS sdate, centerFreq1, polar1, centerFreq2, polar2
	FROM fcsa.hulme.comsearch50km
	WHERE (ABS(@latDeg - lat1) < @TOL AND ABS(@lngDeg - lng1) < @TOL) OR (ABS(@latDeg - lat2) < @TOL AND ABS(@lngDeg - lng2) < @TOL)

 SELECT	'BILL2->',S.call1, S.name, S2.name, S.latit, S.longit, S.sdate,
		A.call2, A.anum, A.azmth, A.elvtn, A.aht, C.chid, C.freqtx, C.poltx, C.freqrx, C.polrx
	FROM test.venn.ft_comsoutC22A_chan AS C
	LEFT JOIN test.venn.ft_comsoutC22A_ante AS A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.anum = C.antnumbtx1)
	LEFT JOIN test.venn.ft_comsoutC22A_site AS S ON (S.call1 = C.call1)
	LEFT JOIN test.venn.ft_comsoutC22A_site AS S2 ON (S2.call1 = C.call2)
	WHERE s.call1 in ('WQEZ616AP','WQEZ616AQ','WQEZ616AR')
	ORDER BY S.call1, A.azmth, C.freqtx
/*
SELECT	'BILL->',S.call1, S.name, S2.name, S.latit, S.longit, S.sdate,
		A.call2, A.anum, A.azmth, A.elvtn, A.aht, C.chid, C.freqtx, C.poltx, C.freqrx, C.polrx
	FROM test.venn.ft_comsoutC22A_site AS S
	INNER JOIN test.venn.ft_comsoutC22A_ante AS A ON (S.call1 = A.call1)
	INNER JOIN test.venn.ft_comsoutC22A_chan AS C ON (C.call1 = A.call1) AND (C.call2 = A.call2) AND ((C.antnumbtx1 = A.anum) OR (C.antnumbrx1 = A.anum))
	INNER JOIN test.venn.ft_comsoutC22A_site AS S2 ON (S2.call1 = A.call2)
	WHERE ABS(@latDeg - S.latit/360000.0) < @TOL AND ABS(-@lngDeg - S.longit/360000.0) < @TOL
	ORDER BY S.call1, A.azmth, C.freqtx
*/

SELECT	'Andrew ->',S.call1, S.name, S2.name, S.latit, S.longit, S.sdate,
		A.call2, A.anum, A.azmth, A.elvtn, A.aht, C.chid, C.freqtx, C.poltx, C.freqrx, C.polrx
	FROM fcsa.hulme.ft_comsearch_chan AS C
	LEFT JOIN fcsa.hulme.ft_comsearch_ante AS A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.anum = C.antnumbtx1)
	LEFT JOIN fcsa.hulme.ft_comsearch_site AS S ON (S.call1 = C.call1)
	LEFT JOIN fcsa.hulme.ft_comsearch_site AS S2 ON (S2.call1 = C.call2)
	WHERE s.call1 in ('WQEZ616AP','WQEZ616AQ','WQEZ616AR')
	ORDER BY S.call1, A.azmth, C.freqtx
/*
SELECT	'Andrew ->', S.call1, S.name, S2.name, S.latit, S.longit, S.sdate,
		A.call2, A.anum, A.azmth, A.elvtn, A.aht, C.chid, C.freqtx, C.poltx, C.freqrx, C.polrx
	FROM fcsa.hulme.ft_comsearch_site AS S
	INNER JOIN fcsa.hulme.ft_comsearch_ante AS A ON (S.call1 = A.call1)
	INNER JOIN fcsa.hulme.ft_comsearch_chan AS C ON (C.call1 = A.call1) AND (C.call2 = A.call2) AND ((C.antnumbtx1 = A.anum) OR (C.antnumbrx1 = A.anum))
	INNER JOIN fcsa.hulme.ft_comsearch_site AS S2 ON (S2.call1 = A.call2)
	WHERE ABS(@latDeg - S.latit/360000.0) < @TOL AND ABS(-@lngDeg - S.longit/360000.0) < @TOL
	ORDER BY S.call1, A.azmth, C.freqtx
*/
SELECT * FROM test.venn.ft_comsoutC22A_site WHERE call1 = 'WQEZ616AQ'
SELECT * FROM test.venn.ft_comsoutC22A_ante WHERE call1 = 'WQEZ616AQ'
SELECT * FROM test.venn.ft_comsoutC22A_chan WHERE call1 = 'WQEZ616AQ'

SELECT * FROM fcsa.hulme.ft_comsearch_site WHERE call1 = 'Z00000008'
SELECT * FROM fcsa.hulme.ft_comsearch_ante WHERE call1 = 'Z00000008'
SELECT * FROM fcsa.hulme.ft_comsearch_chan WHERE call1 = 'Z00000008'

