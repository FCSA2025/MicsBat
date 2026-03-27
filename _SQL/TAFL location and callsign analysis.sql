USE micsdev
GO

SELECT DISTINCT LatitudeWGS84, LongitudeWGS84, Callsign
	FROM IsedTafl_20230725 
	ORDER BY LatitudeWGS84, LongitudeWGS84, Callsign

SELECT /*TXRX, FrequencyMhz, */ DISTINCT LatitudeWGS84, LongitudeWGS84, Callsign
	FROM IsedTafl_20230725 

SELECT TXRX, FrequencyMhz, LatitudeWGS84, LongitudeWGS84, Azimuthofmainlobedeg, Callsign, Licenseename, Stationlocation, Keyfield
	FROM IsedTafl_20230725 
	WHERE hulme.distance_less_than_deg(43.643, -079.387, LatitudeWGS84, LongitudeWGS84, 0.050) = 1

-- Create and populate #TempTable to hold quantized location data from the TAFL.
-- Token is a string comprising Lat@Lng quantized to the prescribed quantum.
DECLARE @quantum AS FLOAT = 0.001
IF OBJECT_ID(N'tempdb..#TempTable') IS NOT NULL DROP TABLE #TempTable
SELECT DISTINCT
	Callsign, CONCAT(FORMAT(hulme.quantizeFloat(LatitudeWGS84, @quantum), '00.000'), '@', FORMAT(hulme.quantizeFloat(LongitudeWGS84, @quantum), '000.000')) AS Token
	INTO #TempTable
	FROM IsedTafl_20230725

SELECT * FROM #TempTable

-------------------------------------------------------------------------
IF OBJECT_ID(N'tempdb..#TempTable2') IS NOT NULL DROP TABLE #TempTable2
SELECT Callsign, COUNT(Token) AS kount
	INTO #TempTable2
	FROM #TempTable
	GROUP BY Callsign
	ORDER BY Callsign

SELECT * FROM #TempTable2 ORDER BY kount DESC

IF OBJECT_ID(N'tempdb..#TempTable21') IS NOT NULL DROP TABLE #TempTable21
SELECT kount, COUNT(Callsign) AS CallSignsWithKount
	INTO #TempTable21
	FROM #TempTable2
	GROUP BY kount
	ORDER BY kount DESC 

SELECT * FROM #TempTable21 ORDER BY kount --CallSignsWithKount DESC

SELECT * 
	FROM #TempTable AS A
	INNER JOIN #TempTable2 AS B ON (A.Callsign = B.Callsign)
	WHERE B.kount = 3
	ORDER BY A.Callsign

-------------------------------------------------------------------------
IF OBJECT_ID(N'tempdb..#TempTable3') IS NOT NULL DROP TABLE #TempTable3
SELECT Token, COUNT(Callsign) AS kount
	INTO #TempTable3
	FROM #TempTable
	GROUP BY Token
	ORDER BY Token

SELECT * FROM #TempTable3 ORDER BY kount DESC

SELECT kount, COUNT(Token)
	FROM #TempTable3
	GROUP BY kount
	ORDER BY kount 


SELECT DISTINCT * FROM #TempTable WHERE Token = '43.643@-079.387'