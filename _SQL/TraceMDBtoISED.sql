DECLARE @cs_to_deg AS FLOAT = 1.0 / 360000.0;

DECLARE @tgt_lat_deg AS FLOAT =  45.339587;
--DECLARE @tgt_lng_deg AS FLOAT =  75.904349;

--DECLARE @tgt_lat_cs AS BIGINT =  @tgt_lat_deg * 360000.0;
--DECLARE @tgt_lng_cs AS BIGINT =  @tgt_lng_deg * 360000.0;

DROP TABLE hulme.temp1

SELECT DISTINCT 
	[hulme].[distance_cs_deg]( S.latit, S.longit, T.LatitudeWGS84, T.LongitudeWGS84) AS dist, 
	C.call1, C.call2, C.bndcde, A.anum, C.chid, C.freqtx, A.azmth, 
	S.latit/360000.0 AS latDeg, S.longit/360000.0 AS lngDeg, 
	T.OccupiedbandwidthkHz AS BW_Khz, T.LatitudeWGS84, T.LongitudeWGS84, A.aht, T.Heightabovegroundlevelm, 
	T.keyfield
	INTO hulme.temp1
	FROM main.mt_site as S
	INNER JOIN main.mt_ante as A ON (A.call1 = S.call1)
	INNER JOIN main.mt_chan as C ON (C.call1 = A.call1) AND (C.call2 = A.call2) AND (C.bndcde = A.bndcde) AND (C.antnumbtx1 = A.anum)
	LEFT OUTER JOIN [fcsa].[venn].[ISEDCleanedI20A] as T 
		ON
		    T.TXRX = 'TX'
		AND ABS(0.001*C.freqtx - T.FrequencyMhz) < 2
		AND ABS(hulme.azimuth_delta_deg(A.azmth, T.Azimuthofmainlobedeg)) < 3.0
		--AND ABS(A.aht - T.Heightabovegroundlevelm) < 10.0
		AND ([hulme].[distance_cs_deg]( S.latit, S.longit, T.LatitudeWGS84, T.LongitudeWGS84) < 0.300)
	WHERE 
		S.oprtyp = 'FT'
		AND A.ause IN ('TX', 'TR')
		AND C.freqtx IS NOT NULL
		--AND S.call1 LIKE 'CFA%'

		--AND  ([hulme].[distance_cs_deg]( @tgt_lat_cs, @tgt_lng_cs, T.LatitudeWGS84, T.LongitudeWGS84) < 1)
	--GROUP BY S.call1, T.LatitudeWGS84, T.LongitudeWGS84, T.TXRX, T.FrequencyMhz, T.OccupiedbandwidthkHz, T.Azimuthofmainlobedeg, T.keyfield 
	ORDER BY C.call1, C.call2, C.bndcde, A.anum, C.chid, C.freqtx

SELECT * FROM hulme.temp1 WHERE keyfield IS NULL

SELECT keyfield, COUNT(*) as kount FROM hulme.temp1 
		WHERE 
				LEFT(call1, 1) NOT IN ('=', '$', '%', ';')
			AND LEFT(call2, 1) NOT IN ('=', '$', '%', ';')
		GROUP BY keyfield
		HAVING COUNT(*) > 1
		ORDER BY keyfield


SELECT * FROM hulme.temp1
	WHERE 
			LEFT(call1, 1) NOT IN ('=', '$', '%', ';')
		AND LEFT(call2, 1) NOT IN ('=', '$', '%', ';')
		AND keyfield IS NULL
	 ORDER BY call1, call2, freqtx

SELECT Polarization, COUNT(Polarization) FROM [fcsa].[venn].[ISEDCleanedI20A] GROUP BY Polarization ORDER BY Polarization

SELECT poltx, COUNT(poltx) FROM main.mt_chan GROUP BY poltx ORDER BY poltx

------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------
SELECT * FROM main.mt_site WHERE call1 LIKE 'XJV%'


	DECLARE @ang1 AS FLOAT = 359;
	DECLARE @ang2 AS FLOAT = 1;

	SELECT hulme.azimuth_delta_deg(@ang1, @ang2);

SELECT call1, name, [hulme].[distance_cs_deg](latit, longit, @tgt_lat_deg, @tgt_lng_deg) AS dist  
	FROM main.mt_site WHERE [hulme].[distance_cs_deg](latit, longit, @tgt_lat_deg, @tgt_lng_deg) < 1

SELECT hulme.IsCloseLatLng(45.0, 180.1, 45.0, 180.0, 1);

SELECT * 
	FROM main.mt_site as A
	WHERE 
			A.prov in ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP')
		AND A.call1 NOT IN (
							SELECT B.CallSign 
							FROM venn.ISEDFullISEDCallInfoI20A as B
						   )
		AND LEFT(A.call1, 1) NOT IN ('=', '$', '%', ';')

SELECT * FROM [fcsa].[venn].[ISEDCleanedI20A] WHERE Callsign = 'CFN783';