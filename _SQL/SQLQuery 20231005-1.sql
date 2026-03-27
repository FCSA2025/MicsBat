SELECT * FROM hulme.IsedTafl_20230831

SELECT Authorizationnumber FROM hulme.IsedTafl_20230831 GROUP BY Authorizationnumber

SELECT SUBSTRING(Authorizationnumber, 1, 9) AS Main, COUNT(Authorizationnumber) FROM hulme.IsedTafl_20230831 GROUP BY SUBSTRING(Authorizationnumber, 1, 9) ORDER BY Main

SELECT TXRX, FrequencyMhz, LatitudeWGS84, LongitudeWGS84, Azimuthofmainlobedeg, Callsign, Authorizationnumber, Authorizationstatus, Inservicedate 
	FROM hulme.IsedTafl_20230831 
	WHERE Authorizationnumber LIKE '%010032293%' 
-----------------------------------------------------------------
SELECT TXRX, FrequencyMhz, LatitudeWGS84, LongitudeWGS84, Azimuthofmainlobedeg, Callsign, Authorizationnumber, Authorizationstatus, Inservicedate  
	FROM hulme.IsedTafl_20230725 
	WHERE Authorizationnumber LIKE '010019099%'
	ORDER BY Authorizationnumber

SELECT TXRX, FrequencyMhz, LatitudeWGS84, LongitudeWGS84, Azimuthofmainlobedeg, Callsign, Authorizationnumber, Authorizationstatus, Inservicedate  
	FROM hulme.IsedTafl_20230831 
	WHERE Authorizationnumber LIKE '010019099%'
	ORDER BY Authorizationnumber

SELECT TXRX, FrequencyMhz, LatitudeWGS84, LongitudeWGS84, Azimuthofmainlobedeg, Callsign, Authorizationnumber, Authorizationstatus, Inservicedate  
	FROM hulme.IsedTafl_20231004 
	WHERE Authorizationnumber LIKE '010019099%'
	ORDER BY Authorizationnumber
-----------------------------------------------------------------------


SELECT * FROM hulme.IsedTafl_20230831 WHERE keyfield IN (13350, 13351)

SELECT TXRX, FrequencyMhz, LatitudeWGS84, LongitudeWGS84, Azimuthofmainlobedeg, Antennastructureheightabovegroundlevelm, Callsign, Authorizationnumber, Authorizationstatus, Inservicedate, Keyfield  
	FROM hulme.IsedTafl_20230831 
	--WHERE Authorizationnumber LIKE '010019099%'
	WHERE Keyfield IN (64600, 64601)

SELECT * 
	FROM hulme.IsedTafl_20230831 
	--WHERE Authorizationnumber LIKE '010019099%'
	WHERE Keyfield IN (64600, 64601)

DROP TABLE #myTempTable_A
SELECT Authorizationnumber, COUNT(Authorizationnumber) AS Kount
INTO 
	#myTempTable_A
	FROM hulme.IsedTafl_20230831
	GROUP BY Authorizationnumber
	ORDER BY Kount DESC, Authorizationnumber

SELECT Kount, Count(Kount) AS Frequency 
	FROM #myTempTable_A
	GROUP BY Kount
	ORDER BY Frequency DESC

DROP TABLE #myTempTable_B
SELECT DISTINCT SUBSTRING(Authorizationnumber, 1, 9) AS main, SUBSTRING(Authorizationnumber, 11, 3) AS Rev
	INTO #myTempTable_B
	FROM hulme.IsedTafl_20230831
	ORDER BY Main, Rev

SELECT * FROM #myTempTable_B ORDER BY Main, Rev DESC

SELECT main, COUNT(Rev) AS RevCount
	FROM #myTempTable_B
	GROUP BY main
	ORDER BY RevCount DESC, main

DROP TABLE #myTempTable_C
SELECT DISTINCT SUBSTRING(Authorizationnumber, 1, 9) As Main, MIN(SUBSTRING(Authorizationnumber, 11, 3)) AS Min, MAX(SUBSTRING(Authorizationnumber, 11, 3)) AS Max
	INTO #myTempTable_C
	FROM hulme.IsedTafl_20230831
	GROUP BY SUBSTRING(Authorizationnumber, 1, 9)
	ORDER BY Main

SELECT * FROM #myTempTable_C ORDER BY Main

SELECT main, COUNT(Rev) AS RevCount
	FROM #myTempTable_C
	GROUP BY main
	ORDER BY RevCount DESC, main



SELECT A.TXRX, A.FrequencyMhz, A.LatitudeWGS84, A.LongitudeWGS84, A.Azimuthofmainlobedeg, A.Antennastructureheightabovegroundlevelm, A.Callsign, A.Authorizationnumber, A.Authorizationstatus, A.Inservicedate, A.Keyfield, B.FrequencyMhz, B.Callsign, B.Authorizationnumber, B.Authorizationstatus, B.Inservicedate, B.Keyfield 
	FROM hulme.IsedTafl_20230831 AS A
	INNER JOIN hulme.IsedTafl_20230831 AS B ON
		(A.TXRX = B.TXRX) AND (A.LatitudeWGS84 = B.LatitudeWGS84) AND (A.LatitudeWGS84 = B.LatitudeWGS84) AND (A.Azimuthofmainlobedeg = B.Azimuthofmainlobedeg) AND  (A.Antennastructureheightabovegroundlevelm = B.Antennastructureheightabovegroundlevelm) 
		AND (SUBSTRING(A.Authorizationnumber, 1, 9) = SUBSTRING(B.Authorizationnumber, 1, 9))
		AND (SUBSTRING(A.Authorizationnumber, 11, 3) != SUBSTRING(B.Authorizationnumber, 11, 3))
		AND (A.FrequencyMhz != B.FrequencyMhz)
	ORDER BY A.Authorizationnumber, A.TXRX, A.FrequencyMhz

