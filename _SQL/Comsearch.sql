USE fcsa

select  srspID, span, _of, freqLoKHz, freqHiKHz, title, applicMICS, pubDate, html_url, pdf_url  from SRSPbands

SELECT * FROM hulme.sd_band

UPDATE hulme.SRSPbands SET srspID = '371.0' FROM hulme.SRSPbands WHERE srspID = '371'

SELECT TXRX, FrequencyMhz, Heightabovegroundlevelm, Azimuthofmainlobedeg, Typeofstation, ITUclassofstation, LatitudeWGS84, LongitudeWGS84, Antennastructureheightabovegroundlevelm, Satellitename, MICSoper, keyfield
FROM [fcsaSQL5D].[fcsa].[venn].[ISEDCleanedI21C]
WHERE ITUclassofstation != 'FX'

SELECT MICSoper, COUNT(MICSoper)
FROM [fcsaSQL5D].[fcsa].[venn].[ISEDCleanedI21C]
WHERE TXRX = 'TX' AND ITUclassofstation = 'FX' AND hulme.isMember(MICSoper) = 1
GROUP BY Micsoper ORDER BY Micsoper

SELECT * FROM fcsa.sys.tables WHERE name LIKE '%comsearch%'


SELECT * FROM fcsaSQL3.fcsa.main.sd_eqpt
SELECT * FROM fcsaSQL3.fcsa.main.sd_oper WHERE nameop LIKE '%LIGHTPOINT%'

SELECT antMod1, COUNT(antMod1) FROM hulme.comsearch GROUP BY antMod1 ORDER BY antMod1
SELECT antMod2, COUNT(antMod1) FROM hulme.comsearch GROUP BY antMod2 ORDER BY antMod2
SELECT eqptMod1, COUNT(eqptMod1) FROM hulme.comsearch GROUP BY eqptMod1 ORDER BY eqptMod1
SELECT eqptMod2, COUNT(eqptMod2) FROM hulme.comsearch GROUP BY eqptMod2 ORDER BY eqptMod2

SELECT COUNT(A.acode) AS incidence, A.acode, MIN(X.amanu) AS manuf, MIN(X.amodel) AS model, MIN(X.again) AS gain, MIN(X.abw) AS bw, MIN(X.adesc) as descr
	FROM fcsaSQL3.fcsa.main.mt_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.sd_ante AS X ON (X.acode = A.acode)
	where bndcde = '78A' 
	GROUP BY A.acode 
	ORDER BY incidence DESC

SELECT * FROM fcsaSQL3.fcsa.main.mt_ante where bndcde = '78A'
SELECT * FROM fcsaSQL3.fcsa.main.sd_band

SELECT * FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '78A'

SELECT * 
	FROM fcsaSQL3.fcsa.main.mt_site AS S
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1 = S.call1)
	WHERE (A.bndcde = '78A')
		AND hulme.isCanadianProvince(S.prov) = 1
	ORDER BY S.call1

SELECT * 
	FROM fcsaSQL3.fcsa.main.mt_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS S ON (A.call1 = S.call1)
	WHERE (A.bndcde = '78A')
		AND hulme.isCanadianProvince(S.prov) = 1
	ORDER BY A.call1

SELECT * 
	FROM fcsaSQL3.fcsa.main.mt_chan AS C
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND ((A.anum = C.antnumbtx1) OR (A.anum = C.antnumbrx1))
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS S ON (A.call1 = S.call1)
	WHERE (A.bndcde = '78A')
		AND hulme.isCanadianProvince(S.prov) = 1
	ORDER BY C.call1

SELECT * 
	FROM fcsaSQL3.fcsa.main.mt_chan AS C
	INNER JOIN fcsaSQL3.fcsa.main.mt_ante AS A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND ((A.anum = C.antnumbtx1) OR (A.anum = C.antnumbrx1))
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS S ON (A.call1 = S.call1)
	WHERE (A.bndcde = '78A')
		AND hulme.isCanadianProvince(S.prov) = 0
	ORDER BY C.call1

SELECT eqpttx, COUNT(eqpttx) FROM fcsaSQL3.fcsa.main.mt_chan where bndcde = '78A' GROUP BY eqpttx
---------------------------------------------------------------------------------
IF OBJECT_ID('#tempEcodes') IS NOT NULL DROP TABLE #tempEcodes
SELECT eqpttx AS eqpt INTO #tempEcodes FROM fcsaSQL3.fcsa.main.mt_chan where bndcde = '78A' GROUP BY eqpttx
UNION
SELECT eqptrx AS eqpt FROM fcsaSQL3.fcsa.main.mt_chan where bndcde = '78A' GROUP BY eqptrx 

SELECT * 
	FROM fcsa.main.sd_eqpt AS A
	INNER JOIN #tempEcodes AS T ON T.eqpt = A.ecode
	ORDER BY ecode

-----------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#tempAcodes') IS NOT NULL DROP TABLE #tempAcodes
SELECT acode AS acodeX INTO #tempAcodes FROM fcsaSQL3.fcsa.main.mt_ante where bndcde = '78A' GROUP BY acode

SELECT * 
	INTO hulme.temptemp
	FROM fcsa.main.sd_ante AS A
	INNER JOIN #tempAcodes AS T ON T.acodeX = A.acode
	ORDER BY A.acode

-------------------------------------------------------------------------------------
SELECT * FROM fcsa.main.sd_ante WHERE bandcodes LIKE '%78A%' ORDER BY amanu, amodel

SELECT * FROM fcsa.main.sd_ante WHERE amanu = 'MTI' ORDER BY amanu, amodel

SELECT axtype, COUNT(axtype) FROM fcsa.main.sd_ante GROUP BY axtype ORDER BY axtype
SELECT axref, COUNT(axref) AS kount FROM fcsa.main.sd_ante GROUP BY axref ORDER BY kount desc
SELECT arms, COUNT(arms) FROM fcsa.main.sd_ante GROUP BY arms ORDER BY arms
SELECT adesc, COUNT(adesc) AS kount FROM fcsa.main.sd_ante GROUP BY adesc ORDER BY kount desc

DROP TABLE hulme.comsearch
SELECT * FROM hulme.comsearch
SELECT * FROM hulme.comsearch WHERE centerFreq2 IS NULL
SELECT * FROM hulme.comsearch WHERE (location1 NOT LIKE '' OR location2 NOT LIKE '')
SELECT MAX(LEN(location1)) FROM hulme.comsearch 
SELECT MAX(LEN(location2)) FROM hulme.comsearch 



SELECT * FROM hulme.ft_comsearch_titl
SELECT * FROM hulme.ft_comsearch_site ORDER BY stats
SELECT * FROM hulme.ft_comsearch_ante
SELECT * FROM hulme.ft_comsearch_chan

SELECT * FROM hulme.comsearch WHERE site1 LIKE 'CROWN' OR site2 LIKE 'CROWN'
SELECT * FROM hulme.ft_comsearch_site 
SELECT call1, COUNT(call1) AS numAnte FROM hulme.ft_comsearch_ante GROUP BY call1 ORDER BY numAnte DESC, call1
SELECT * FROM hulme.ft_comsearch_chan WHERE call1 = 'Z00003173' ORDER BY chid

SELECT * FROM hulme.sd_ante
SELECT * FROM hulme.sd_eqpt
SELECT * FROM hulme.sd_oper
SELECT * FROM hulme.sd_traf

SELECT * FROM hulme.comsearch_tafl_Intermediate

SELECT call1, call2, bndcde, anum, aht, azmth, elvtn FROM hulme.ft_comsearch_ante


DROP TABLE fcsa.hulme.mt_site
SELECT * INTO fcsa.hulme.mt_site FROM fcsaSQL3.fcsa.main.mt_site
DROP TABLE fcsa.hulme.mt_ante
SELECT * INTO fcsa.hulme.mt_ante FROM fcsaSQL3.fcsa.main.mt_ante
DROP TABLE fcsa.hulme.mt_chan
SELECT * INTO fcsa.hulme.mt_chan FROM fcsaSQL3.fcsa.main.mt_chan

SELECT * FROM hulme.mt_site
SELECT * FROM hulme.mt_ante
SELECT * FROM hulme.mt_chan

SELECT * FROM main.mt_site
SELECT * FROM main.mt_ante
SELECT * FROM main.mt_chan --ORDER BY pwrtx

SELECT * FROM main.sd_ante
SELECT * FROM main.sd_eqpt
SELECT * FROM main.sd_oper WHERE opnote = 'AA'
SELECT * FROM main.sd_traf

SELECT * FROM fcsa.main.sd_eqpt WHERE ecode = 'ML321068' ORDER BY emanu, emodel

SELECT * FROM fcsa.main.sd_eqpt WHERE ecode LIKE 'DFLT%' ORDER BY ecode

SELECT * FROM fcsa.main.sd_eqpt WHERE ((emodel LIKE '%AL80%') OR (edesc LIKE '%AL80%')) ORDER BY emanu, emodel

SELECT * FROM fcsa.main.sd_traf 

SELECT * FROM fcsa.main.sd_oper WHERE hulme.isCanadianProvince(prstat) = 0
SELECT * FROM fcsa.main.sd_oper WHERE nameop LIKE 'TRANS%'

SELECT * FROM fcsa.main.sd_oper WHERE oper LIKE '#%'
SELECT * FROM fcsa.main.sd_ante WHERE acode LIKE '#%'



SELECT DISTINCT A.trafcode, A.ecode
	INTO #myTable
	FROM fcsa.main.sd_traf AS A
	INNER JOIN fcsa.main.sd_traf AS B ON (A.ecode = B.ecode)
	WHERE A.trafcode != B.trafcode
	ORDER BY ecode, trafcode

SELECT COUNT(ecode) AS 'Number of different trafcode', ecode FROM #myTable GROUP BY ecode ORDER BY ecode

DROP TABLE #myTable
SELECT acode, COUNT(acode) AS kount 
    INTO #myTable
	FROM fcsa.main.mt_ante 
	WHERE bndcde = '78A' 
	GROUP BY acode 
	ORDER BY acode

SELECT B.kount, A.*
	FROM fcsa.main.sd_ante as A
	INNER JOIN #myTable AS B ON (A.acode = B.acode)
	ORDER BY amanu, amodel

SELECT * FROM fcsa.main.sd_ante 
	WHERE /*aband = '80 GHZ'*/ bandcodes LIKE '%78A%'
	ORDER BY amanu, amodel
	
SELECT * FROM fcsa.main.sd_ante WHERE acode = 'WORST11T'


SELECT * FROM fcsa.main.mt_site WHERE prov = 'ON' AND oper = 'RCTL' AND name LIKE '%OTTAWA%'

SELECT * 
	FROM fcsa.web.user_tables 
	WHERE operator='hulme' AND tabletype=0 AND file_name='testhilo'

INSERT INTO
	web.user_tables
	SELECT operator, tabletype, 'comsearch', micsid, project_code, validstat, create_date
	FROM fcsa.web.user_tables 
	WHERE operator='hulme' AND tabletype=0 AND file_name='testhilo'

DELETE FROM web.user_tables WHERE file_name='comsearch' AND micsid='hulme1'

SELECT * FROM sys.database_principals ORDER BY name



SELECT call1 FROM fcsa.main.mt_site

SELECT C.*
	FROM main.mt_chan AS C
	INNER JOIN main.mt_site AS S ON (S.call1 = C.call1)
	WHERE S.prov = 'NY'

--===================================================================================
DECLARE @lat AS REAL= 42.901436;						;
DECLARE @lng AS REAL= 78.890103;
DECLARE @rad AS REAL = 20.0;

SELECT hulme.distance_cs_deg(S.latit, S.longit, @lat, @lng) AS dist, latit/360000.0 AS lat, -longit/360000.0 AS lng, *
	FROM fcsa.main.mt_site AS S
    WHERE 
		(hulme.distance_less_than_cs_deg(S.latit, S.longit, @lat, @lng, @rad) = 1)
	ORDER BY dist, call1

SELECT latit/360000.0 AS lat, -longit/360000.0 AS lng, * FROM fcsa.main.mt_site --WHERE call1 = 'WQWY80701'

SELECT * FROM fcsa.main.sd_oper WHERE oper = 'C99631'

SELECT * FROM main.mt_site WHERE call1 LIKE '%WQTX959%'

SELECT A.bndcde, *
	FROM fcsa.main.mt_site AS S
	INNER JOIN fcsa.main.mt_ante as A ON (S.call1 = A.call1)
	WHERE S.call1 IN ('WQII65905', 'WQNI57501')

---------------------------------------------------------------------------------

USE [fcsa]
GO

/****** Object:  Table [venn].[ISEDIntermediateI22B]    Script Date: 6/6/2022 9:55:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [venn].[ISEDIntermediateI22B](
	[TXRX] [varchar](2) NOT NULL,
	[callsign] [varchar](10) NOT NULL,
	[call2] [varchar](10) NULL,
	[bndcde] [varchar](6) NULL,
	[sitename] [varchar](32) NULL,
	[anum] [int] NULL,
	[chid] [varchar](5) NULL,
	[lat] [varchar](12) NULL,
	[lon] [varchar](13) NULL,
	[grnd] [float] NULL,
	[prov] [varchar](2) NULL,
	[oper] [varchar](10) NULL,
	[opnote] [varchar](2) NULL,
	[stats] [varchar](1) NULL,
	[icaccount] [varchar](13) NULL,
	[licence] [varchar](20) NULL,
	[sdate] [varchar](12) NULL,
	[Total_losses_dB] [float] NULL,
	[acodetx] [varchar](40) NULL,
	[ahttx] [float] NULL,
	[aztx] [float] NULL,
	[elevtx] [float] NULL,
	[freq] [float] NULL,
	[pol] [varchar](1) NULL,
	[pwrtx] [float] NULL,
	[losstx] [float] NULL,
	[gaintx] [float] NULL,
	[eqpttx] [varchar](10) NULL,
	[traftx] [varchar](10) NULL,
	[namerx] [varchar](255) NULL,
	[anumrx] [int] NULL,
	[latrx] [varchar](12) NULL,
	[lonrx] [varchar](13) NULL,
	[grndrx] [float] NULL,
	[provrx] [varchar](2) NULL,
	[operrx] [varchar](255) NULL,
	[acoderx] [varchar](40) NULL,
	[ahtrx] [float] NULL,
	[azrx] [float] NULL,
	[elevrx] [float] NULL,
	[lossrx] [float] NULL,
	[gainrx] [float] NULL,
	[eqptrx] [varchar](10) NULL,
	[trafrx] [varchar](10) NULL,
	[RecordAction] [int] NULL,
	[MICSanum] [int] NULL
) ON [PRIMARY]
GO

SELECT * FROM fcsa.hulme.comsearch_tafl_Intermediate
DROP TABLE fcsa.hulme.comsearch_tafl_Intermediate


SELECT * FROM hulme.ft_apple_site

SELECT * FROM web.user_tables ORDER BY create_date DESC

SELECT * FROM main.sd_oper

DELETE FROM web.user_tables WHERE file_name='apple'

SELECT * FROM fcsa.main.mt_ante WHERE call1='VAD429'

SELECT  call1 AS CALL1, COUNT(anum) AS AnumCount INTO #tempThur FROM fcsa.main.mt_ante GROUP BY call1 ORDER BY call1

SELECT * FROM #tempThur ORDER BY AnumCount DESC

SELECT * FROM fcsa.main.mt_site

SELECT * FROM fcsa.main.mt_chan WHERE bndcde = '78A'

SELECT * FROM fcsa.venn.ISEDIntermediateI22A WHERE bndcde = '78A'

SELECT fcsa.hulme.isCanadianProvince(S.prov) ,S.oper, S.prov, * 
	FROM fcsa.main.mt_chan AS C
	INNER JOIN fcsa.main.mt_site S ON (S.call1 = C.call1)
	WHERE bndcde = '78A'

SELECT * FROM fcsa.main.sd_eqpt\

SELECT * FROM hulme.comsearch WHERE compName LIKE '%AMG%' ORDER BY compName

SELECT * FROM fcsa.main.sd_oper WHERE nameop LIKE '%MST%' ORDER BY nameop