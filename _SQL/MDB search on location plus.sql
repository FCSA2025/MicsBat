-- Create and populate a table containing MDB TS TX Link-end data.
IF OBJECT_ID('hulme.temp_MDB_TS_TX_Links') IS NOT NULL DROP TABLE hulme.temp_MDB_TS_TX_Links
SELECT DISTINCT
    S.oper,
    S.latit/360000.0 as mdbLat, S.longit/360000.0 AS mdbLng,
	S.prov,
	C.call1,  C.call2, C.bndcde, A.anum, C.chid, C.freqtx, A.azmth, A.aht
INTO hulme.temp_MDB_TS_TX_Links
FROM main.mt_chan as C
INNER JOIN main.mt_ante as A ON (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbtx1) AND (A.ause IN ('TX', 'TR'))
INNER JOIN main.mt_site as S On (S.call1 = C.call1) AND (S.oprtyp = 'FT')

DECLARE @lat AS REAL= 42.892964;						;
DECLARE @lng AS REAL= -78.87629;	

SELECT [hulme].[distance_deg](L.mdblat, L.mdblng, @lat, @lng) AS dist, *  
	FROM hulme.temp_MDB_TS_TX_Links AS L
	WHERE 
			([hulme].[distance_less_than_deg](L.mdblat, L.mdblng, @lat, @lng, 0.3) = 1)
	ORDER BY dist, azmth, freqtx