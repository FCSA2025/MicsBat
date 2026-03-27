--================================================================================================================
-- The purpose of this T-SQL script is, for every Tx and Rx record in the ISED TAFL data belonging to an FCSA 
-- member, to search the FCSA MDB data looking for a 'reasonable' match on the link parameters 'local site location',
-- 'azimuth' and 'Tx frequency'.
--
-- There are three essential inputs to this script:
--
--      [a] a table providing a full set of TAFL import data; and
--      [b] a table providing a lookup of a unique FCSA 'oper' assignment for a set
--          of ISED Licensee Names, used in the TAFL data, that have been determined
--          to belong to FCSA members. E.g. 'HYDRO-QUÉBEC' is FCSA oper 'HYQU'.
--      [c] the current MDB TS tables.
--
-- If a TAFL record has a matching channel record in the MDB then an entry is made into a working table
-- that records:
--                [d] the unique line index (Venn 'keyfield') of the TAFL record; and the matching
--                [e] {m_call1, m_call2, m_bndcde, m_chid, m_anum} MDB table keys.
--
-- Terminology:
--               LAF   :   Location, Azimuth and Frequency.
--               LA-   :   Location and Azimuth only.
--               L-F   :   Location and Frequency only.
--
-- The matching algorithm uses 3-parameter matching (x_LAF) to identify EXCELLENT, LIKELY and POSSIBLE matches
-- of a member's TAFL record and a MDB mt_chan record.
-- 
-- The matching algorithm also identifies 'near miss' matches between a TAFL record and a unique mt_chan record,
-- using 2-parameter matching LA- and L-F.
--
-- It often happens that a TAFL record is 'owned' by a member (as inferred from its t_LicenseeName) but no
-- 'good' 3-parameter match nor 'near miss' 2-parameter match can be found. These are referred to as
-- 'missing' links, i.e. a member's TAFL record that is missing from the MDB.
--
-- Note:  the algorithm does not apply any matching criteria involving MICSoper and/or TAFL t_LicenseeName.
--		  It finds what it finds!
--
-- Special provision is made for the FCSA member oper 'ALIANT' which shares the same TAFL t_LicenseeName with NTTEL.
--
-- The t_LicenseeName to FCSA oper lookup table must be named:
--
--                                  hulme.Taflt_LicenseeNameToFcsaOper
--
-- Before running this script, find and replace the following string to update the TAFL source table name
-- to the most recent version:
--
--                                  hulme.IsedTafl_20250226
--
--================================================================================================================
RAISERROR('StartingLinkMatch_TaflToMdb ...', 0, 1) WITH NOWAIT

USE micsdev;

-- Define the required 3-parameter matching tolerances.
DECLARE @d_kms_tol_3PM FLOAT = 0.300    -- kms     difference between MDB location  and TAFL location.
DECLARE @d_deg_tol_3PM FLOAT = 3.0      -- degrees difference between MDB azimuth   and TAFL azimuth.
DECLARE @d_MHz_tol_3PM FLOAT = 2        -- MHz     difference between MDB frequency and TAFL frequency. 

-- Define the required 2-parameter matching tolerances (for 'near miss' analysis).
DECLARE @d_kms_tol_2PM FLOAT = 0.050    -- kms     difference between MDB location  and TAFL location.
DECLARE @d_deg_tol_2PM FLOAT = 1.0      -- degrees difference between MDB azimuth   and TAFL azimuth.
DECLARE @d_MHz_tol_2PM FLOAT = 0.1      -- MHz     difference between MDB frequency and TAFL frequency. 

-- Create a 'working' table that will record the intermediate and final link-matching data.
IF OBJECT_ID('hulme.LinkMatch_TaflToMdb') IS NOT NULL DROP TABLE hulme.LinkMatch_TaflToMdb

-- Create a table variable of the required user-defined table type.
DECLARE @myTable AS hulme.LinkMatchTableType

--Create a new permanent/physical LAML working table by selecting into from the table variable @myTable.
SELECT *
	INTO hulme.LinkMatch_TaflToMdb
	FROM @myTable
	WHERE 1 = 2

-- Populate the working table with the essential fields from the FCSA member's TAFL data.
RAISERROR('Populating working table with fields from the FCSA members TAFL data ...', 0, 1) WITH NOWAIT
INSERT INTO hulme.LinkMatch_TaflToMdb
	(x_Direction, t_KeyField, t_TxRx, t_lat, t_lng, t_Azmth, t_Aht, t_FreqTxRx, t_AuthorizationNumber, t_AuthorizationStatus, t_Callsign, t_InserviceDate, t_AccountNumber, t_LicenseeName, t_ReferenceIdentifier, t_LicenseeName_oper)
	SELECT 'TaflToMdb', T.Keyfield, T.TXRX, T.LatitudeWGS84, T.LongitudeWGS84, T.Azimuthofmainlobedeg, T.Antennastructureheightabovegroundlevelm, T.FrequencyMhz, T.AuthorizationNumber, T.AuthorizationStatus, T.Callsign, T.InserviceDate, T.AccountNumber, T.LicenseeName, T.ReferenceIdentifier, M.MICSoper
	FROM hulme.IsedTafl_20250226 AS T
	INNER JOIN [hulme].[TaflLicenseeNameToFcsaOper] AS M ON (M.ISEDLicenseeName = LTRIM(RTRIM(T.LicenseeName)))
	WHERE 	
			(T.LatitudeWGS84        IS NOT NULL)
		AND (T.LongitudeWGS84       IS NOT NULL)
		AND (T.Azimuthofmainlobedeg IS NOT NULL)
		--AND (M.MICSoper = 'BMCE')

--SELECT * FROM hulme.LinkMatch_TaflToMdb

-- Fix the snaffu w.r.t. multiple TELUS operator IDs not matching the province recorded in the TAFL record.
UPDATE hulme.LinkMatch_TaflToMdb
	SET t_LicenseeName_oper = CASE
								WHEN ((W.t_LicenseeName_oper LIKE 'TLUS%') AND (T.Provinces = 'AB')) THEN 'TLUSAB'
							    WHEN ((W.t_LicenseeName_oper LIKE 'TLUS%') AND (T.Provinces = 'BC')) THEN 'TLUSBC'
								WHEN ((W.t_LicenseeName_oper LIKE 'TLUS%') AND (T.Provinces = 'QC')) THEN 'TLUSQC'
								WHEN ((W.t_LicenseeName_oper LIKE 'TLUS%') AND (T.Provinces IN ('NL', 'PE', 'NS', 'NB', 'ON', 'MB', 'SK', 'YT', 'NT', 'NU'))) THEN 'TLUSMC'
							    ELSE   W.t_LicenseeName_oper
							END
	FROM hulme.LinkMatch_TaflToMdb AS W
	INNER JOIN hulme.IsedTafl_20250226 AS T ON (T.Keyfield = W.t_KeyField)

--SELECT * FROM hulme.LinkMatch_TaflToMdb

-- Both ALIANT and NTTEL have the same TAFL t_LicenseeName, i.e. 'BELL CANADA'.
-- The lookup table [hulme].[Taflt_LicenseeNameToFcsaOper] has an entry for 'NTTEL' but not for ALIANT. 
-- ALIANT can be differentiated in the TAFL data using the criteria:
-- (t_LicenseeName = 'BELL CANADA') AND (t_ReferenceIdentifier = 'BELL ALIANT')
-- We will now correct the LinkMatch_TaflToMdb table to correctly identify
-- which 'BELL CANADA' TAFL records have t_LicenseeName_oper 'ALIANT or 'NTTEL'.
UPDATE hulme.LinkMatch_TaflToMdb
	SET 
		t_LicenseeName_oper = 'ALIANT'
	FROM hulme.LinkMatch_TaflToMdb
	WHERE (t_LicenseeName = 'BELL CANADA') AND (t_ReferenceIdentifier = 'BELL ALIANT')

--SELECT * FROM hulme.LinkMatch_TaflToMdb WHERE t_LicenseeName = 'BELL CANADA' ORDER BY t_LicenseeName_oper

-- To perform the matching of a TAFL record to a channel in the MDB it is first expedient to
-- extract a subset of member data from the MDB and 'flatten' it to resemble a TAFL record.
-- Let's select the member TX channel data first and insert it into a temporary table.
IF OBJECT_ID('tempdb..#MDBmemberFlat') IS NOT NULL DROP TABLE #MDBmemberFlat

CREATE TABLE #MDBmemberFlat(
	[TxRx] [char](2),               -- same as for the TAFL TXRX filed but applied to the MDB channel data.
	[oper] [char](6),				-- mt_site.oper
    [call1] [char](9),			    -- mt_site.call1
	[call2] [char](9),			    -- mt.ante.call2
	[bndcde] [char](4),		        -- mt_ante.bndcde
	[chid] [char](4),			    -- mt_chan.chid
	[ause] [char](3),               -- mt_ante.ause
	[anum] [smallint],				-- mt_ante.anum (= mt_chan.antnumbtx1 for a Tx Link)
	[lat] [float],				    -- mt_site.latit  / 360000.0
    [lng] [float],				    -- mt_site.longit / 360000.0
	[prov] [char](2),				-- mt_site.prov
	[azmth] [real],				    -- mt.ante.Azmth
	[freqtx] [float],				-- mt_chan.freqtx
	[freqrx] [float],				-- mt_chan.freqrx
	[aht] [real],					-- mt_ante.aht
	[freqtxrx] [float],             -- mt_chan.freqtx or mt_chan.freqrx depending on TxRx     
	[stattxrx] [char](1),           -- mt_chan.stattx or mt_chan.statrx depending on TxRx 
	[name] [char](32) NULL,			-- MDB:  mt_site.name
	[reg] [char](2) NULL,           -- MDB:  mt_site.reg
 CONSTRAINT [PK_MDBmemberFlat_20250226] PRIMARY KEY CLUSTERED 
(
    [TxRx] ASC,
    [call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[chid] ASC,
	[anum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

-- Select the member TX channel data from the MDB and insert it into the temporary flat table.
RAISERROR('Populating temporary ''flat'' table with member MDB data to resemble a TAFL record ...', 0, 1) WITH NOWAIT
INSERT INTO #MDBmemberFlat 
	SELECT
		'TX', 
		S.oper,
		C.call1, C.call2, C.bndcde, C.chid, A.ause, A.anum, 
		S.latit/360000.0, -S.longit/360000.0,
		S.prov, A.azmth, 
		C.freqtx*0.001, 0.0,
		A.aht, 
		C.freqtx*0.001, C.stattx, S.name, S.reg
	FROM micstest.main.mt_chan AS C
	INNER JOIN micstest.main.mt_ante as A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbtx1)
	INNER JOIN micstest.main.mt_site AS S ON (S.call1 = C.call1)
	WHERE (C.antnumbtx1 IS NOT NULL) AND (hulme.isMemberGenuine(S.oper) = 1) 

--SELECT * FROM #MDBmemberFlat

-- Now select the member RX channel data and insert it into the temporary table.
INSERT INTO #MDBmemberFlat 
	SELECT
		'RX', 
		S.oper,
		C.call1, C.call2, C.bndcde, C.chid, A.ause, A.anum, 
		S.latit/360000.0, -S.longit/360000.0,
		S.prov, A.azmth, 
		0.0, C.freqrx*0.001, 
		A.aht,
		C.freqrx*0.001, C.statrx, S.name, S.reg
	FROM micstest.main.mt_chan AS C
	INNER JOIN micstest.main.mt_ante as A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbtx1)
	INNER JOIN micstest.main.mt_site AS S ON (S.call1 = C.call1)
	WHERE (C.antnumbtx1 IS NOT NULL) AND (hulme.isMemberGenuine(S.oper) = 1) 

--SELECT * FROM #MDBmemberFlat

-- Set the x_LAF column to its default value '---'.
UPDATE hulme.LinkMatch_TaflToMdb
	SET x_LAF = '---'

-- For each TAFL record 'owned' by a member search for a 3-parameter fit in the MDB data.
RAISERROR('Starting 3-parameter matching: TAFL -> MDB ...', 0, 1) WITH NOWAIT
UPDATE hulme.LinkMatch_TaflToMdb
		SET 
		    x_LAF = 'LAF',
		    m_TxRx = M.TXRX,
			m_Oper = M.oper,
			m_Call1 = M.call1,
			m_Call2 = M.call2,
			m_Bndcde = M.bndcde,
			m_Chid = M.chid,
			m_Ause = M.ause,
			m_Anum = M.anum,
			m_Lat = M.Lat,
			m_Lng = M.Lng,
			m_Prov = M.prov,
			m_Azmth = M.azmth,
			m_FreqTx = CASE WHEN (M.TXRX = 'TX') THEN M.freqtx ELSE 0.0 END,
			m_FreqRx = CASE WHEN (M.TXRX = 'RX') THEN M.freqrx ELSE 0.0 END,
			m_Aht = M.aht,
			m_FreqTxRx = CASE WHEN (M.TXRX = 'TX') THEN M.freqtx ELSE M.freqrx END,
			m_StatTxRx = M.stattxrx,
			m_OrdinalKey = [hulme].[OrdinalKey](M.call1, M.call2),
			m_SiteName = M.name,
			m_Region = M.reg,
			d_Meters = 1000*[hulme].[distance_deg](M.Lat, M.Lng, T.t_lat, T.t_lng),
		    d_Degrees = ABS(hulme.azimuth_delta_deg(M.Azmth, T.t_Azmth)),
		    d_MHz = ABS(M.FreqTxRx - T.t_FreqTxRx)
	FROM hulme.LinkMatch_TaflToMdb AS T
	INNER JOIN #MDBmemberFlat AS M ON 
				(M.TXRX = T.t_TxRx)
		AND ABS(M.freqTxRx - T.t_FreqTxRx) < @d_MHz_tol_3PM
		AND ABS(hulme.azimuth_delta_deg(M.azmth, T.t_Azmth)) < @d_deg_tol_3PM
		AND ([hulme].[distance_less_than_deg](M.lat, M.lng, T.t_lat, T.t_lng, @d_kms_tol_3PM) = 1)

--SELECT * FROM hulme.LinkMatch_TaflToMdb WHERE t_KeyField = 27--ORDER BY TXRX, t_LicenseeName_oper

-- Calculate the 3-parameter matching Confidence Figure of Merit (FOM), on a scale 0 to 100.
UPDATE hulme.LinkMatch_TaflToMdb
	SET x_FOM = hulme.TAFLmatchFOM(300.0, 3.0, 2.0, d_Meters, d_Degrees, d_MHz)
	WHERE (x_LAF = 'LAF')

-- Set the 3-parameter matching 'Confidence' field to one of EXCELLENT, LIKELY, POSSIBLE depending on FOM.
UPDATE hulme.LinkMatch_TaflToMdb
	SET x_Confidence =	CASE
							WHEN (x_FOM >= 85)				      THEN 'EXCELLENT'
							WHEN ((x_FOM >= 50) AND (x_FOM < 85)) THEN 'LIKELY'
							WHEN (x_FOM < 50)					  THEN 'POSSIBLE'
						END
	WHERE (x_LAF = 'LAF')	

--SELECT * FROM hulme.LinkMatch_TaflToMdb ORDER BY TXRX, t_LicenseeName_oper

-----------------------------------------------------------------------------------------------
--
--             NEAR MISS ANALYSIS
--
-----------------------------------------------------------------------------------------------
-- Determine whether there is a location and azimuth 'near miss' for any of the remaining '---' 
-- links using 2-parameter matching.
RAISERROR('Starting 2-parameter ''LA-'' matching: TAFL -> MDB ...', 0, 1) WITH NOWAIT
UPDATE hulme.LinkMatch_TaflToMdb
	SET 
		    x_LAF = 'LA-',
		    m_TxRx = M.TxRx,
			m_Oper = M.oper,
			m_Call1 = M.call1,
			m_Call2 = M.call2,
			m_Bndcde = M.bndcde,
			m_Chid = M.chid,
			m_Ause = M.ause,
			m_Anum = M.anum,
			m_Lat = M.lat,
			m_Lng = M.lng,
			m_Prov = M.prov,
			m_Azmth = M.azmth,
			m_FreqTx = CASE WHEN (M.TXRX = 'TX') THEN M.freqtx ELSE 0.0 END,
			m_FreqRx = CASE WHEN (M.TXRX = 'RX') THEN M.freqrx ELSE 0.0 END,
			m_Aht = M.aht,
			m_FreqTxRx = CASE WHEN (M.TxRx = 'TX') THEN M.freqtx ELSE M.freqrx END,
			m_StatTxRx = M.stattxrx,
			m_OrdinalKey = [hulme].[OrdinalKey](M.call1, M.call2),
			m_SiteName = M.name,
			m_Region = M.reg,
			d_Meters = 1000*[hulme].[distance_deg](M.lat, M.lng, T.t_lat, T.t_lng),
		    d_Degrees = ABS(hulme.azimuth_delta_deg(M.azmth, T.t_Azmth)),
		    d_MHz = ABS(M.FreqTxRx - T.t_FreqTxRx)
	FROM hulme.LinkMatch_TaflToMdb AS T
	INNER JOIN #MDBmemberFlat AS M ON 
				(M.TxRx = T.t_TxRx)
		AND ABS(hulme.azimuth_delta_deg(M.azmth, T.t_Azmth)) < @d_deg_tol_2PM
		AND ([hulme].[distance_less_than_deg](M.lat, M.lng, T.t_lat, T.t_lng, @d_kms_tol_2PM) = 1)
	WHERE 
		(T.x_LAF = '---') 

-- Determine whether there is a location and azimuth 'near miss' for each of the 'bad' Links
-- using 2-parameter matching.
RAISERROR('Starting 2-parameter ''L-F'' matching: TAFL -> MDB ...', 0, 1) WITH NOWAIT
UPDATE hulme.LinkMatch_TaflToMdb
	SET 
		    x_LAF = 'L-F',
		    m_TxRx = M.TXRX,
			m_Oper = M.oper,
			m_call1 = M.call1,
			m_call2 = M.call2,
			m_bndcde = M.bndcde,
			m_chid = M.chid,
			m_ause = M.ause,
			m_anum = M.anum,
			m_Lat = M.lat,
			m_Lng = M.lng,
			m_Prov = M.prov,
			m_Azmth = M.azmth,
			m_FreqTx = CASE WHEN (M.TxRx = 'TX') THEN M.freqtx ELSE 0.0 END,
			m_FreqRx = CASE WHEN (M.TxRx = 'RX') THEN M.freqrx ELSE 0.0 END,
			m_Aht = M.aht,
			m_FreqTxRx = CASE WHEN (M.TxRx = 'TX') THEN M.freqtx ELSE M.freqrx END,
		    m_StatTxRx = M.stattxrx,
			m_OrdinalKey = [hulme].[OrdinalKey](M.call1, M.call2),
			m_SiteName = M.name,
			m_Region = M.reg,
			d_Meters = 1000*[hulme].[distance_deg](M.lat, M.lng, T.t_lat, T.t_lng),
		    d_Degrees = ABS(hulme.azimuth_delta_deg(M.azmth, T.t_Azmth)),
		    d_MHz = ABS(M.freqTxRx - T.t_FreqTxRx)
	FROM hulme.LinkMatch_TaflToMdb AS T
	INNER JOIN #MDBmemberFlat AS M ON 
				(M.TxRx = T.t_TxRx)
		AND ABS(M.freqTxRx - T.t_FreqTxRx) < @d_MHz_tol_2PM
		AND ([hulme].[distance_less_than_deg](M.lat, M.lng, T.t_lat, T.t_lng, @d_kms_tol_2PM) = 1)
	WHERE 
		(T.x_LAF = '---') 

-- Provide a summary of total and missing TAFL links rolled-up by operator.
RAISERROR('Creating summary of total matching and missing TAFL links rolled-up by operator ...', 0, 1) WITH NOWAIT
SELECT 
	t_LicenseeName_oper AS oper, 
	COUNT(t_LicenseeName_oper) AS totalRecords,
	SUM(CASE WHEN x_LAF != 'LAF' THEN 1 ELSE 0 END) AS totalMissing,
	SUM(CASE WHEN x_LAF  = 'LA-' THEN 1 ELSE 0 END) AS LA_,
	SUM(CASE WHEN x_LAF  = 'L-F' THEN 1 ELSE 0 END) AS L_F,
	SUM(CASE WHEN x_LAF  = '---' THEN 1 ELSE 0 END) AS ___	
	FROM hulme.LinkMatch_TaflToMdb
	GROUP BY t_LicenseeName_oper
	ORDER BY t_LicenseeName_oper



