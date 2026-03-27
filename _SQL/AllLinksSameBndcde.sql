SELECT S1.call1, C.call2, C.bndcde, C.chid, A.anum, S1.prov, S1.name, hulme.distance_spherical_cs(S1.latit, S1.longit, S2.latit, S2.longit) AS dist, hulme.latitToFloat(S1.latit), hulme.longitToFloat(S1.longit), hulme.latitToFloat(S2.latit), hulme.longitToFloat(S2.longit), C.freqtx, C.freqrx, C.antnumbtx1, C.antnumbrx1
	FROM fcsa.main.mt_chan AS C 
    INNER JOIN fcsa.main.mt_ante AS A ON (A.call1=C.call1 AND A.call2=C.call2 AND A.bndcde=C.bndcde)
	INNER JOIN fcsa.main.mt_site AS S1 ON (S1.call1=C.call1)
	INNER JOIN fcsa.main.mt_site AS S2 ON (S2.call1=C.call2)
	WHERE C.bndcde='78A'
	ORDER BY dist DESC, C.call1, C.call2, A.anum, C.chid
	 
SELECT * FROM main.mt_site
SELECT * FROM main.mt_ante
SELECT * FROM main.mt_chan