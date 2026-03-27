-- Find all back-to-back repeaters.
SELECT * FROM main.mt_site WHERE call1 LIKE ';%'
SELECT * FROM main.mt_ante WHERE call1 LIKE ';%'
SELECT * FROM main.mt_chan WHERE call1 LIKE ';%'

-- Find all passive repeaters.
SELECT * FROM main.mt_site WHERE call1 LIKE '[%]%'
SELECT * FROM main.mt_ante WHERE call1 LIKE '[%]%'
SELECT * FROM main.mt_chan WHERE call1 LIKE '[%]%'

-- Find all TS passive repeaters and count how many antennas per site.
SELECT S.call1, COUNT(S.call1) AS numAnte
	FROM main.mt_site AS S
	JOIN main.mt_ante AS A ON ((A.call1 = S.call1))
	WHERE
		S.call1 LIKE '[%]%'
	GROUP BY S.call1
	ORDER BY numAnte, call1

-- Find all TS passive repeaters and count how many channels per antenna.
SELECT S.call1, A.call2, A.bndcde, A.anum, COUNT(C.chid) AS numChan --C.chid, C.antnumbtx1, C.antnumbrx1
	FROM main.mt_site AS S
	JOIN main.mt_ante AS A ON ((A.call1 = S.call1))
	JOIN main.mt_chan AS C ON ((C.call1 = S.call1) AND (C.call2 = A.call2) AND (C.bndcde = A.bndcde) AND ((C.antnumbtx1 = A.anum) OR (C.antnumbrx1 = A.anum)))
	WHERE
		S.call1 LIKE '[%]%'
		--S.call1 = '%HYQU12'
		GROUP BY S.call1, A.call2, A.bndcde, A.anum
	--GROUP BY S.call1
	ORDER BY numChan DESC, S.call1, A.call2, A.bndcde, A.anum

