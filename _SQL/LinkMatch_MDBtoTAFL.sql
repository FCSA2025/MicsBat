--==============================================================================================================
-- The purpose of this T-SQL script is, for every TS Tx Link in the MDB belonging to an FCSA member, to
-- search the ISED/TAFL data looking for a 'reasonable' match on the Link parameters 'local site location',
-- 'azimuth' and 'Tx frequency' and to identify and record the licence number of the matching ISED/TAFL record.
--
-- There are two essential inputs to this script:
--      [a] a table providing a full set of TAFL import data; and
--      [b] the extant MDB tables.
--
-- When all of the member's MDB links with valid licences have been identified individual tables of
-- licenced Tx Links are created for each member.
--
-- Terminology:
--               LAF   :   Location, Azimuth and Frequency.
--               LA-   :   Location and Azimuth only.
--               L-F   :   Location and Frequency only.
--
-- The matching algorithm uses 3-parameter matching (LAF) to identify EXCELLENT, LIKELY and POSSIBLE matches
-- of a member's MDB mt_chan record with a TAFL record uniquely identified by its Venn 'keyField' index.
-- 
-- The matching algorithm also identifies 'near miss' matches between a mt_chan record and a unique TAFL record,
-- using 2-parameter matching LA- and L-F.
--
-- Note:  the algorithm does not apply any matching criteria involving MICSoper and/or TAFL LicenseeName.
--		  It finds what it finds!
--
-- Before running this script, find and replace the following string to update the TAFL source table name
-- to the most recent version:
--
--                                  IsedTafl_20250226
--
--==============================================================================================================

USE micsdev;

-- Define the required 3-parameter matching tolerances.
DECLARE @d_kms_tol_3PM FLOAT = 0.300    -- kms     difference between MDB location  and TAFL location.
DECLARE @d_deg_tol_3PM FLOAT = 3.0      -- degrees difference between MDB azimuth   and TAFL azimuth.
DECLARE @d_MHz_tol_3PM FLOAT = 2        -- MHz     difference between MDB frequency and TAFL frequency. 

-- Define the required 2-parameter matching tolerances (for 'near miss' analysis).
DECLARE @d_kms_tol_2PM FLOAT = 0.050    -- kms     difference between MDB location  and TAFL location.
DECLARE @d_deg_tol_2PM FLOAT = 1.0      -- degrees difference between MDB azimuth   and TAFL azimuth.
DECLARE @d_MHz_tol_2PM FLOAT = 0.1      -- MHz     difference between MDB frequency and TAFL frequency. 

-- Create and populate a table containing relevant MDB TS TX Link-end data.
-- Also, create columns for later use in the  MDB-to-ISED Link matching analysis.
IF OBJECT_ID('hulme.LinkMatch_MdbToTafl') IS NOT NULL DROP TABLE hulme.LinkMatch_MdbToTafl

-- Create a table variable of the required user-defined table type.
DECLARE @myTable AS hulme.LinkMatchTableType

--Create a new permanent/physical LAML working table by selecting into from the table variable @myTable.
SELECT *
	INTO hulme.LinkMatch_MdbToTafl
	FROM @myTable
	WHERE 1 = 2

-- Populate the working table with member's TX-end MDB link data.
INSERT INTO hulme.LinkMatch_MdbToTafl (x_Direction, m_TxRx, m_Oper, m_Call1, m_Call2, m_Bndcde, m_Chid, m_Ause, m_Anum, m_Lat, m_Lng, m_Prov, m_Azmth, m_FreqTx, m_Aht, m_FreqTxRx, m_StatTxRx, m_OrdinalKey, m_SiteName, m_Region) 
	SELECT 'MdbToTafl', 'TX', S.oper, C.call1, C.call2, C.bndcde, C.chid, A.ause, C.antnumbtx1,
			S.latit/360000.0, -S.longit/360000.0,       -- Note that longitudes are negative, same as TAFL.
			S.prov, A.azmth, C.freqtx, A.aht, C.freqtx, C.stattx,
			hulme.ordinalKey(C.call1, C.call2), S.name, S.reg
	FROM micstest.main.mt_chan as C
	INNER JOIN micstest.main.mt_ante as A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbtx1)
	INNER JOIN micstest.main.mt_site as S ON (S.call1 = C.call1) AND (S.oprtyp = 'FT')
	--WHERE (S.oper = 'ALIANT')

--SELECT * FROM hulme.LinkMatch_MdbToTafl

-- Populate the working table with member's RX-end MDB link data.
INSERT INTO hulme.LinkMatch_MdbToTafl (m_TxRx, m_Oper, m_Call1, m_Call2, m_Bndcde, m_Chid, m_Ause, m_Anum, m_Lat, m_Lng, m_Prov, m_Azmth, m_FreqRx, m_Aht, m_FreqTxRx, m_StatTxRx, m_OrdinalKey, m_SiteName, m_Region) 
	SELECT 'RX', S.oper, C.call1, C.call2, C.bndcde, C.chid, A.ause, C.antnumbtx1,
			S.latit/360000.0, -S.longit/360000.0,       -- Note that longitudes are negative, same as TAFL.
			S.prov, A.azmth, C.freqrx, A.aht, C.freqrx, C.statrx,
			hulme.ordinalKey(C.call1, C.call2), S.name, S.reg
	FROM micstest.main.mt_chan as C
	INNER JOIN micstest.main.mt_ante as A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbrx1)
	INNER JOIN micstest.main.mt_site as S ON (S.call1 = C.call1) AND (S.oprtyp = 'FT')
	--WHERE (S.oper = 'ALIANT')

--SELECT * FROM hulme.LinkMatch_MdbToTafl ORDER BY call1, call2, bndcde, chid, TXRX

-- Set the LAF column to its default value '---'.
UPDATE hulme.LinkMatch_MdbToTafl
	SET x_LAF = '---'

-- The following query performs the 3-parameter MDB TS Link-end to ISED/TAFL record matching.
-- The matching criteria is that location/azimuth/TxFrequency components agree within prescribed tolerances.
-- If no Link-end match is found in the TAFL data then 'keyfield' remains NULL.
-- Remember to update the name of the TAFL data table to be used, e.g. IsedTafl_20250226
UPDATE hulme.LinkMatch_MdbToTafl
SET 
	x_LAF = 'LAF',
	t_KeyField = T.keyfield
FROM 
	hulme.LinkMatch_MdbToTafl AS L
	INNER JOIN hulme.IsedTafl_20250226 AS T ON 
			(T.TXRX = L.m_TxRx)
		AND ABS(0.001*L.m_FreqTxRx - T.FrequencyMhz) < @d_MHz_tol_3PM
		AND ABS(hulme.azimuth_delta_deg(L.m_Azmth, T.Azimuthofmainlobedeg)) < @d_deg_tol_3PM
		AND ([hulme].[distance_less_than_deg](L.m_Lat, L.m_Lng, T.LatitudeWGS84, T.LongitudeWGS84, @d_kms_tol_3PM) = 1)

--SELECT * FROM hulme.LinkMatch_MdbToTafl ORDER BY  LAF, Authorizationnumber,  TXRX DESC, call1, call2, bndcde, anum, chid

-- The essential information fabric has now been constructed by identifying a 
-- candidate match between each MDB link and a line number (keyField) in the TAFL data.
-- We can now populate almost all of the remaining columns.
UPDATE hulme.LinkMatch_MdbToTafl
	SET 
		t_TxRx = T.TXRX,
		t_AuthorizationNumber = T.Authorizationnumber, 
		t_Callsign =T.Callsign, 
		t_InserviceDate = T.Inservicedate, 
		t_AccountNumber = T.Accountnumber, 
		t_LicenseeName = T.Licenseename,
		t_ReferenceIdentifier = T.ReferenceIdentifier,
		t_Lat = T.LatitudeWGS84,
		t_Lng = T.LongitudeWGS84,
		t_Azmth = T.Azimuthofmainlobedeg,
		t_Aht = T.Heightabovegroundlevelm,
		t_FreqTxRx = T.FrequencyMhz,
		t_AuthorizationStatus = T.AuthorizationStatus,
		d_meters = 1000*[hulme].[distance_deg](m_Lat, m_Lng, T.LatitudeWGS84, T.LongitudeWGS84),
		d_degrees = ABS(hulme.azimuth_delta_deg(m_Azmth, T.Azimuthofmainlobedeg)),
		d_MHz = ABS(0.001*m_FreqTxRx - T.FrequencyMhz)
	FROM hulme.IsedTafl_20250226 AS T
	WHERE (x_LAF != '---') AND (t_KeyField = T.keyfield)

-- Calculate the 3-parameter matching confidence Figure of Merit (FOM), on a scale 0 to 100.
UPDATE hulme.LinkMatch_MdbToTafl
	SET x_FOM = hulme.TAFLmatchFOM(300.0, 3.0, 2.0, d_meters, d_degrees, d_MHz)
	WHERE (x_LAF = 'LAF')

-- Set the 3-parameter matching 'Confidence' field to one of EXCELLENT, LIKELY, POSSIBLE depending on FOM.
UPDATE hulme.LinkMatch_MdbToTafl
	SET x_Confidence =	CASE
							WHEN (x_FOM >= 85)				      THEN 'EXCELLENT'
							WHEN ((x_FOM >= 50) AND (x_FOM < 85)) THEN 'LIKELY'
							WHEN (x_FOM < 50)					  THEN 'POSSIBLE'
						END
	WHERE (x_LAF = 'LAF')	

-- As a basis for further screening, set the LicenseeName_oper column.
UPDATE hulme.LinkMatch_MdbToTafl
	SET t_Licenseename_Oper = N.MICSoper
	FROM hulme.TaflLicenseeNameToFcsaOper AS N
	WHERE t_LicenseeName = N.ISEDLicenseeName

-----------------------------------------------------------------------------------------------
--
--             NEAR MISS ANALYSIS
--
-----------------------------------------------------------------------------------------------
-- At this stage we have completed the population of the working table with matched / license data.
-- We now begin the analysis of the member's phantom MDB links for which we have found no match in the TAFL data.
-- We look for "near misses" between the MDB links and a links in the TAFL.

-- Determine whether there is a location and azimuth 'near miss' for each of the 'bad' Links.
-- A 'near miss' is defined as either a LA- or L-F 2-parameter match.

-- Identify and populate LA- near misses found using 2-parameter matching.
UPDATE hulme.LinkMatch_MdbToTafl
	SET 
		x_LAF = 'LA-',
		t_TxRx = T.TXRX,
		t_AuthorizationNumber = T.Authorizationnumber, 
		t_Callsign =T.Callsign, 
		t_InserviceDate = T.Inservicedate, 
		t_AccountNumber = T.Accountnumber, 
		t_LicenseeName = T.Licenseename,
		t_Lat = T.LatitudeWGS84,
		t_Lng = T.LongitudeWGS84,
		t_Azmth = T.Azimuthofmainlobedeg,
		t_Aht = T.Heightabovegroundlevelm,
		t_FreqTxRx = T.FrequencyMhz,
		t_AuthorizationStatus = T.AuthorizationStatus,
		d_meters = 1000*[hulme].[distance_deg](m_Lat, m_Lng, T.LatitudeWGS84, T.LongitudeWGS84),
		d_degrees = ABS(hulme.azimuth_delta_deg(m_Azmth, T.Azimuthofmainlobedeg)),
		d_MHz = ABS(0.001*m_FreqTxRx - T.FrequencyMhz)
	FROM hulme.LinkMatch_MdbToTafl AS L
	INNER JOIN hulme.IsedTafl_20250226 AS T ON
		ABS(hulme.azimuth_delta_deg(L.m_Azmth, T.Azimuthofmainlobedeg)) < @d_deg_tol_2PM
		AND ([hulme].[distance_less_than_deg](L.m_Lat, L.m_Lng, T.LatitudeWGS84, T.LongitudeWGS84, @d_kms_tol_2PM) = 1)
		AND (L.m_TxRx = T.TXRX)
	WHERE 
		(L.x_LAF = '---') 

--SELECT * FROM hulme.LinkMatch_MdbToTafl ORDER BY  LAF, Authorizationnumber,  TXRX DESC, call1, call2, bndcde, anum, chid

-- Identify and populate L-F near misses found using 2-parameter matching.
UPDATE hulme.LinkMatch_MdbToTafl
	SET 
		x_LAF = 'L-F',
		t_TxRx = T.TXRX,
		t_AuthorizationNumber = T.Authorizationnumber, 
		t_Callsign =T.Callsign, 
		t_InserviceDate = T.Inservicedate, 
		t_AccountNumber = T.Accountnumber, 
		t_LicenseeName = T.Licenseename,
		t_Lat = T.LatitudeWGS84,
		t_Lng = T.LongitudeWGS84,
		t_Azmth = T.Azimuthofmainlobedeg,
		t_Aht = T.Heightabovegroundlevelm,
		t_FreqTxRx = T.FrequencyMhz,
		t_AuthorizationStatus = T.AuthorizationStatus,
		d_meters = 1000*[hulme].[distance_deg](m_Lat, m_Lng, T.LatitudeWGS84, T.LongitudeWGS84),
		d_degrees = ABS(hulme.azimuth_delta_deg(m_Azmth, T.Azimuthofmainlobedeg)),
		d_MHz = ABS(0.001*m_FreqTxRx - T.FrequencyMhz)
	FROM hulme.LinkMatch_MdbToTafl AS L
	INNER JOIN hulme.IsedTafl_20250226 AS T ON
		ABS(0.001*L.m_FreqTxRx - T.FrequencyMhz) < @d_MHz_tol_2PM
		AND ([hulme].[distance_less_than_deg](L.m_Lat, L.m_Lng, T.LatitudeWGS84, T.LongitudeWGS84, @d_kms_tol_2PM) = 1)
		AND (L.m_TxRx = T.TXRX)
	WHERE 
		(L.x_LAF = '---')

-- Populate the CorrectiveAction column for the 2-parameter 'near-misses'.
UPDATE hulme.LinkMatch_MdbToTafl
	SET x_Comment =	CASE
								WHEN (x_LAF = 'LA-') AND (m_TxRx = 'TX') THEN 'NEAR-MISS: is the MDB Tx frequency correct?'
								WHEN (x_LAF = 'LA-') AND (m_TxRx = 'RX') THEN 'NEAR-MISS: is the MDB Rx frequency correct?'
								WHEN (x_LAF = 'L-F') THEN 'NEAR-MISS: is the MDB azimuth correct?'
							END
	WHERE (x_LAF IN ('LA-', 'L-F'))	

-- Provide a summary of total and missing TAFL links rolled-up by operator.
SELECT 
	m_oper AS oper, 
	COUNT(m_oper) AS totalRecords,
	SUM(CASE WHEN x_LAF != 'LAF' THEN 1 ELSE 0 END) AS totalPhantoms,
	SUM(CASE WHEN x_LAF  = 'LA-' THEN 1 ELSE 0 END) AS LA_,
	SUM(CASE WHEN x_LAF  = 'L-F' THEN 1 ELSE 0 END) AS L_F,
	SUM(CASE WHEN x_LAF  = '---' THEN 1 ELSE 0 END) AS ___	
	FROM hulme.LinkMatch_MdbToTafl
	GROUP BY m_oper
	ORDER BY m_oper


