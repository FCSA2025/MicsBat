-- Create a table to contain matching Tx and Rx Link-ends.
IF OBJECT_ID('hulme.MDB_TS_Link_Pairs') IS NOT NULL DROP TABLE hulme.MDB_TS_Link_Pairs

SELECT
		CONVERT(CHAR(9), NULL) AS txCall1,
		CONVERT(CHAR(9), NULL) AS txCall2,
		CONVERT(CHAR(4), NULL) AS txBndcde,
		CONVERT(SMALLINT, NULL) AS txAnum,
		CONVERT(CHAR(4), NULL) AS txChid,
		CONVERT(CHAR(6), NULL) AS txOper,
		CONVERT(FLOAT, NULL) AS txFreqtx,
		CONVERT(FLOAT, NULL) AS txAzmth,
		CONVERT(CHAR(4), NULL) AS rxBndcde,
		CONVERT(SMALLINT, NULL) AS rxAnum,
		CONVERT(CHAR(4), NULL) AS rxChid,
		CONVERT(CHAR(6), NULL) AS rxOper,
		CONVERT(FLOAT, NULL) AS rxFreqrx,
		CONVERT(FLOAT, NULL) AS rxAzmth
	INTO hulme.MDB_TS_Link_Pairs
	WHERE 0 = 1

-- Populate the table with Tx Link-ends.
-- Restrict this to FCSA member operators for now.
INSERT INTO hulme.MDB_TS_Link_Pairs (txCall1,txCall2,txBndcde,txAnum,txChid,txOper,txFreqtx,txAzmth,rxBndcde,rxAnum,rxChid,rxOper,rxFreqrx,rxAzmth)
	SELECT
		C.call1,
		C.call2,
		C.bndcde,
		C.antnumbtx1,
		C.chid,
		S.oper,
		C.freqtx,
		A.azmth,
		NULL,--rxBndcde
		NULL,--rxAnum
		NULL,--rxChid
		NULL,--rxOper
		NULL,--txFreqtx
		NULL --txAzmth
	FROM main.mt_chan AS C
	INNER JOIN main.mt_ante AS A ON ((A.ause IN ('TX', 'TR')) AND (A.call1 = C.call1) AND (A.call2 = C.call2) AND (A.bndcde = C.bndcde) AND (A.anum = C.antnumbtx1))
	INNER JOIN main.mt_site as S ON (S.call1 = A.call1)
	WHERE
		S.oprtyp = 'FT'

-- Now find the matching Rx Link-end for every Tx Link-end.
UPDATE hulme.MDB_TS_Link_Pairs
	SET
		rxBndcde = C.bndcde,
		rxAnum = C.antnumbrx1,
		rxChid = C.chid,
		rxOper = S.oper,
		rxFreqrx = C.freqrx,
		rxAzmth = A.azmth
	FROM hulme.MDB_TS_Link_Pairs AS L
	INNER JOIN main.mt_site as S ON (S.call1 = L.txCall2)
	INNER JOIN main.mt_ante AS A ON ((A.ause IN ('RX', 'TR')) AND (A.call1 = S.call1) AND (A.call2 = L.txCall1))
	INNER JOIN main.mt_chan AS C ON ((C.call1 = A.call1) AND (C.call2 = C.call2) AND (C.bndcde = A.bndcde) AND (C.antnumbrx1 = A.anum))
	WHERE
			--S.oprtyp = 'FT'
		 (ABS(L.txFreqtx - C.freqrx) < 2)
		AND ABS(ABS(hulme.azimuth_delta_deg(L.txAzmth, A.azmth)) - 180.0) < 2.0

SELECT * FROM hulme.MDB_TS_Link_Pairs 
	WHERE rxOper IS NULL
	ORDER BY txOper, rxOper, txCall1, txCall2, txBndcde, txFreqtx

SELECT * FROM main.mt_chan WHERE call1 = 'VX9NKA' AND call2 = 'VX9EQT' ORDER BY bndcde, freqrx
SELECT * FROM main.mt_ante WHERE call1 = 'VX9NKA' AND call2 = 'VX9EQT' ORDER BY bndcde

SELECT ABS(ABS(hulme.azimuth_delta_deg(178.45182800293, 1.548173)) - 180)

SELECT latit/360000.0, -longit/300600.0 FROM main.mt_site WHERE call1 = 'VX9NKA'