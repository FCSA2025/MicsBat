--------------------------------------------------------------
-- This script updates an already created main.sd_licn table.
--------------------------------------------------------------
--
-- This script should be run after every LAML re-calculation or TS non-member import from ISED/TAFL.

-- Provenance codes: 'NON_MEMBER_MT_ANTE','MEMBER_MT_ANTE', 'TAFL_LAML_MATCH', 'WEBMICS_MEMBER_ENTERED'.
--
-- Global replace the LAML table date tag '20231004' with that of the latest available TAFL data.

USE micsdev
GO


-- It is essential to first save any licence data entered by a member using WebMICS.

-- If it already exists, delete the temporary table used to save WebMICS-entered licence data.
IF OBJECT_ID('tempdb..#WebMicsEnteredLicences') IS NOT NULL DROP TABLE #WebMicsEnteredLicences

-- Copy any existing WebMICS-entered licence data into the temporary table.
SELECT *
	INTO #WebMicsEnteredLicences
	FROM main.sd_licn
	WHERE (tx_provenance = 'WEBMICS_MEMBER_ENTERED') OR (rx_provenance = 'WEBMICS_MEMBER_ENTERED')

-- Record the current date and time.
DECLARE @MDATE AS CHAR(10) = FORMAT (getdate(), 'yyyy.MM.dd')
DECLARE @MTIME AS CHAR( 5) = FORMAT (getdate(), 'HH:mm')

-- Synthesize a proper date from the LAML table date tag.
DECLARE @str CHAR(8) = '20231004'
DECLARE @TAFL_DATE CHAR(10) = CONCAT(SUBSTRING(@str, 1, 4), '.', SUBSTRING(@str, 5, 2), '.', SUBSTRING(@str, 7, 2))

-- Truncate the main.sd_licn table if it already exists:
IF OBJECT_ID('main.sd_licn') IS NOT NULL TRUNCATE TABLE main.sd_licn

-- Populate the main.sd_licn table with the full set of Canadian operator channel keys.
-- Also, populate the 'convenience' fields while this data is easily accessible.
INSERT INTO main.sd_licn
		(call1, call2, bndcde, chid, oper, oprtyp, antnumbtx1, antnumbrx1, mdate, mtime)
	SELECT DISTINCT
		C.call1, C.call2, C.bndcde, C.chid, S.oper, S.oprtyp, C.antnumbtx1, C.antnumbrx1, @MDATE, @MTIME
	FROM micsprod.main.mt_chan AS C
	INNER JOIN micsprod.main.mt_ante AS A ON (C.call1 = A.call1) AND (C.call2 = A.call2) AND (C.bndcde = A.bndcde) AND ((C.antnumbtx1 = A.anum) OR (C.antnumbrx1 = A.anum))
	INNER JOIN micsprod.main.mt_site AS S ON (C.call1 = S.call1)
	WHERE
		S.oprtyp IN ('FT', 'CT')  -- FT = FCSA member; CT = Canadian non-member.
EXECUTE Log2 'LinkMatch_Populate_sd_licn', 'Completed: INSERT INTO main.sd_licn (initial)'

-- Restore the previously saved WebMICS-entered licence data.
-- We need to do the Tx and Rx licence components separately.
UPDATE main.sd_licn 
	SET 
		tx_licence = W.tx_licence,
		tx_provenance = W.tx_provenance,
		tx_mdate = W.tx_mdate
	FROM #WebMicsEnteredLicences AS W
	INNER JOIN main.sd_licn AS L ON (W.call1 = L.call1) AND (W.call2 = L.call2) AND (W.bndcde = L.bndcde) AND (W.chid = L.chid)
	WHERE
		W.tx_provenance = 'WEBMICS_MEMBER_ENTERED'

UPDATE main.sd_licn 
	SET 
		rx_licence = W.rx_licence,
		rx_provenance = W.rx_provenance,
		rx_mdate = W.rx_mdate
	FROM #WebMicsEnteredLicences AS W
	INNER JOIN main.sd_licn AS L ON (W.call1 = L.call1) AND (W.call2 = L.call2) AND (W.bndcde = L.bndcde) AND (W.chid = L.chid)
	WHERE
		W.rx_provenance = 'WEBMICS_MEMBER_ENTERED'

--SELECT * FROM #WebMicsEnteredLicences
--SELECT * FROM main.sd_licn WHERE (tx_provenance = 'WEBMICS_MEMBER_ENTERED') OR (rx_provenance = 'WEBMICS_MEMBER_ENTERED')

-- Populate the table with Canadian non-member licence data.
-- The TS TAFL import process assigns the correct ISED licence number to every non-member mt_ante record.
-- Put another way, every non-member mt_ante record should already have a licence number.
-- Do the Tx component first.
UPDATE main.sd_licn
	SET 
		tx_licence = A.licence,
		tx_provenance = 'NON_MEMBER_MT_ANTE',
		tx_mdate = A.mdate
	FROM main.sd_licn AS L
	INNER JOIN micsprod.main.mt_ante AS A ON (L.call1 = A.call1) AND (L.call2 = A.call2) AND (L.bndcde = A.bndcde) AND (L.antnumbtx1 = A.anum)
	WHERE
		L.oprtyp = 'CT'
EXECUTE Log2 'LinkMatch_Populate_sd_licn',  'Completed: UPDATE main.sd_licn (Non-member, Tx)'

-- Populate the table with Canadian non-member licence data.
-- Now do the Rx component.
UPDATE main.sd_licn
	SET 
		rx_licence = A.licence,
		rx_provenance = 'NON_MEMBER_MT_ANTE',
		rx_mdate = A.mdate
	FROM main.sd_licn AS L
	INNER JOIN micsprod.main.mt_ante AS A ON (L.call1 = A.call1) AND (L.call2 = A.call2) AND (L.bndcde = A.bndcde) AND (L.antnumbrx1 = A.anum)
	WHERE
		L.oprtyp = 'CT'
EXECUTE Log2 'LinkMatch_Populate_sd_licn', 'Completed: UPDATE main.sd_licn (Non-member, Rx)'


-- Populate the table with member licence data.
-- Not all member antenna records have a licence number.
-- Assume that the same-end Tx and Rx channel components are on the same licence.
UPDATE main.sd_licn
	SET 
		tx_licence = A.licence,
		tx_provenance = 'MEMBER_MT_ANTE',
		tx_mdate = A.mdate,
		rx_licence = A.licence,
		rx_provenance = 'MEMBER_MT_ANTE',
		rx_mdate = A.mdate
	FROM main.sd_licn AS L
	INNER JOIN micsprod.main.mt_ante AS A ON (L.call1 = A.call1) AND (L.call2 = A.call2) AND (L.bndcde = A.bndcde) AND (L.antnumbrx1 = A.anum)
	WHERE
		L.oprtyp = 'FT'
		AND hulme.isValidIsedAuthorizationNumber(A.licence) = 1
EXECUTE Log2 'LinkMatch_Populate_sd_licn', 'Completed: UPDATE main.sd_licn (Member, Tx and Rx)'

-- Now update the records to include the TAFL-derived licence data for members.

-- It will simplify the following T-SQL queries if we first create a temporary table 
-- that contains the 'high-FOM' TAFL record matches between the member's MDB channels 
-- and TAFL records.

-- Delete the temporary table if it already exists:
IF OBJECT_ID('tempdb..#goodMatches') IS NOT NULL DROP TABLE #goodMatches

-- Populate the temporary table using a subset of the latest available LAML analysis 
-- results corresponding to mt_chan records for which a licence has been found
-- and the FOM exceeds a prescribed value over [0, 100].
SELECT *
	INTO #goodMatches
	FROM hulme.LinkMatch_MdbToTafl_20231004 AS R
	WHERE R.x_LAF = 'LAF' AND (R.x_FOM >= 50)

-- Now update the sd_licn table with licence data derived from TAFL matches.
-- Remember that #WebMicsEnteredLicences has TX and RX data in separate records.
-- First do the TX type records.
UPDATE main.sd_licn
	SET 
		tx_licence =  G.t_AuthorizationNumber,
	    tx_provenance = 'TAFL_LAML_MATCH',
		tx_mdate = @TAFL_DATE,

		tx_AuthorizationNumber = G.t_AuthorizationNumber,
		tx_AccountNumber = G.t_AccountNumber,
		tx_licenceeName = G.t_LicenseeName,
		tx_Freq = G.t_FreqTxRx,
		tx_Lat = G.t_Lat,
		tx_Lng = G.t_Lng,
		tx_Azmth = G.t_Azmth,
		tx_Aht = G.t_Aht,
		tx_AuthorizationStatus = G.t_AuthorizationStatus,
		tx_Callsign = G.t_Callsign,
		tx_InserviceDate = REPLACE(G.t_InserviceDate, '-', '.'),	
		tx_FOM = G.x_FOM,

		TaflDate = @TAFL_DATE
	FROM main.sd_licn AS L
	INNER JOIN #goodMatches AS G ON ((G.m_Call1 = L.call1) AND (G.m_Call2 = L.call2) AND (G.m_bndcde = L.bndcde) AND (G.m_chid = L.chid))
	WHERE 
			G.t_TxRx = 'TX'
		AND L.tx_AuthorizationNumber IS NULL

-- Now do the RX records.	
UPDATE main.sd_licn
	SET 
		rx_licence =  G.t_AuthorizationNumber,
	    rx_provenance = 'TAFL_LAML_MATCH',
		rx_mdate = @TAFL_DATE,

		rx_AuthorizationNumber = G.t_AuthorizationNumber,
		rx_AccountNumber = G.t_AccountNumber,
		rx_licenceeName = G.t_LicenseeName,
		rx_Freq = G.t_FreqTxRx,
		rx_Lat = G.t_Lat,
		rx_Lng = G.t_Lng,
		rx_Azmth = G.t_Azmth,
		rx_Aht = G.t_Aht,
		rx_AuthorizationStatus = G.t_AuthorizationStatus,
		rx_Callsign = G.t_Callsign,
		rx_InserviceDate = REPLACE(G.t_InserviceDate, '-', '.'),	
		rx_FOM = G.x_FOM,

		TaflDate = @TAFL_DATE
	FROM main.sd_licn AS L
	INNER JOIN #goodMatches AS G ON ((G.m_Call1 = L.call1) AND (G.m_Call2 = L.call2) AND (G.m_bndcde = L.bndcde) AND (G.m_chid = L.chid))
	WHERE 
			G.t_TxRx = 'RX'
		AND L.rx_AuthorizationNumber IS NULL

-- Clean up.
DROP TABLE #WebMicsEnteredLicences
DROP TABLE #goodMatches

SELECT * FROM main.sd_licn
	--WHERE oper = 'RCTL' AND rx_licence IS NULL
	WHERE tx_provenance = 'WEBMICS_MEMBER_ENTERED' OR rx_provenance = 'WEBMICS_MEMBER_ENTERED'


