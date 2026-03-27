/* extract relevant info from main tables */
DROP TABLE hulme.TempISEDm

SELECT DISTINCT 
	A.dist AS dist, 
	C.call1, C.call2, C.bndcde, A.anum, C.chid, C.freqtx, A.aht, A.azmth, A.elvtn, 
	S.latit/360000.0 AS latDegtx, S.longit/360000.0 AS lonDegtx, S2.latit/360000.0 AS latDegrx, S2.longit/360000.0 AS lonDegrx 
	INTO hulme.TempISEDm
	FROM main.mt_chan as C 
	LEFT JOIN main.mt_ante as A ON (C.call1 = A.call1) AND (C.call2 = A.call2) AND (C.bndcde = A.bndcde) 
	LEFT JOIN main.mt_site as S ON (C.call1 = S.call1)
	LEFT JOIN main.mt_site as S2 ON (C.call2 = S2.call1)
	WHERE 
		S.oprtyp = 'FT'
		AND A.ause IN ('TX', 'TR')
		AND C.freqtx IS NOT NULL
		AND S.call1 NOT LIKE '=%'

/* Extract relevant info from Intermediate table (this is closest match to Mics) but you may want to revert to Cleaned and adjust field names */
DROP TABLE hulme.TempISEDi
SELECT CONVERT(float,0.0) as dist, callsign, FrequencyMhz as freqMHz, LatitudeWGS84 as lat, LongitudeWGS84 as lon, Heightabovegroundlevelm, Azimuthofmainlobedeg, 
CONVERT(float,0.0) AS latDegtx, CONVERT(float,0.0) as lonDegtx
		INTO hulme.TempISEDi
	FROM venn.[ISEDCleanedI20A] 
		WHERE TXRX = 'T'
		AND LatitudeWGS84 IS NOT NULL
		AND LongitudeWGS84 IS NOT NULL

/* convert lat/longs to decimal degree format for comparisons */
UPDATE hulme.TempISEDi SET latDegtx = hulme.strToFloatLat(lat), lonDegtx = hulme.strToFloatLong(lon)

/* calculate distances (there appears to be a bug in your calculatons but I didn't look into it */
UPDATE hulme.TempISEDi SET dist = hulme.distance_cs_deg( latDegTx, lonDegTx, latDegrx, lonDegrx)

/* link the two tables to find matches (you can tune the criteria) */
SELECT m.call1, M.call2, M.freqtx, M.latDegtx, I.latDegtx, M.lonDegtx, I.lonDegtx, M.aht, I.ahttx, M.azmth, I.aztx 
	FROM hulme.TempISEDm M LEFT JOIN hulme.TempISEDi I
	ON 	M.freqtx  = I.freq
		AND ABS(ROUND(M.latDegtx,3) - ROUND(I.latDegtx,3)) < .005
		AND ABS(ROUND(M.lonDegtx,3) - ROUND(I.lonDegtx,3)) < .005	
		AND ABS(M.aht - I.ahttx) < 2.0
		AND ABS(M.azmth - I.aztx) < 1.0
	ORDER BY M.latDegtx, M.lonDegtx
	

SELECT TOP 10 * FROM hulme.TempISEDm ORDER BY latDegtx, lonDegtx
SELECT TOP 10 * FROM hulme.TempISEDi	ORDER BY latDegtx, lonDegtx
SELECT TOP 10 * FROM venn.ISEDFullIntermediateI20A ORDER BY lat, lon