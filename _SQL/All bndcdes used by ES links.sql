USE fcsa

SELECT A.location, A.call1, A.txband, A.rxband, A.az, A.el, C.chid, C.freqtx, C.freqrx
	FROM fcsaSQL3.fcsa.main.me_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.me_chan AS C ON (C.location = A.location) AND (C.call1 = A.call1)

SELECT A.txband, COUNT(A.txband) AS numChannels
	FROM fcsaSQL3.fcsa.main.me_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.me_chan AS C ON (C.location = A.location) AND (C.call1 = A.call1)
	GROUP BY txband
	ORDER BY txband

SELECT A.rxband, COUNT(A.rxband) AS numChannels
    INTO ES_RX
	FROM fcsaSQL3.fcsa.main.me_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.me_chan AS C ON (C.location = A.location) AND (C.call1 = A.call1)
	GROUP BY rxband
	ORDER BY rxband

IF OBJECT_ID('tempdb..#ES_TX') IS NOT NULL DROP TABLE #ES_TX
CREATE TABLE #ES_TX (
    txband CHAR(4) NOT NULL PRIMARY KEY,
	numChannels INT NULL
)
IF OBJECT_ID('tempdb..#ES_RX') IS NOT NULL DROP TABLE #ES_RX
CREATE TABLE #ES_RX (
    rxband CHAR(4) NOT NULL PRIMARY KEY,
	numChannels INT NULL
)

INSERT INTO #ES_TX
	SELECT A.txband, COUNT(A.txband) AS numChannels
	FROM fcsaSQL3.fcsa.main.me_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.me_chan AS C ON (C.location = A.location) AND (C.call1 = A.call1)
	WHERE A.txband IS NOT NULL
	GROUP BY txband
	ORDER BY txband

INSERT INTO #ES_RX
	SELECT A.rxband, COUNT(A.rxband) AS numChannels
	FROM fcsaSQL3.fcsa.main.me_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.me_chan AS C ON (C.location = A.location) AND (C.call1 = A.call1)
	WHERE A.rxband IS NOT NULL
	GROUP BY rxband
	ORDER BY rxband

IF OBJECT_ID('tempdb..#ES_temp') IS NOT NULL DROP TABLE #ES_temp
CREATE TABLE #ES_temp (
    bndcde CHAR(4) NOT NULL
)

INSERT INTO #ES_temp SELECT txband FROM #ES_TX
INSERT INTO #ES_temp SELECT rxband FROM #ES_RX

IF OBJECT_ID('tempdb..#ES_all_bndcdes') IS NOT NULL DROP TABLE #ES_all_bndcdes
CREATE TABLE #ES_all_bndcdes (
    bndcde CHAR(4) NOT NULL,
)

INSERT INTO #ES_all_bndcdes SELECT * FROM #ES_temp GROUP BY bndcde ORDER BY bndcde

SELECT * FROM #ES_all_bndcdes ORDER BY bndcde

IF OBJECT_ID('tempdb..#ES_TxRx') IS NOT NULL DROP TABLE #ES_TxRx
CREATE TABLE #ES_TxRx (
    bndcde CHAR(4) NOT NULL PRIMARY KEY,
	countTx INT NULL,
	countRx INT NULL,
	blo INT NULL,
	bhi INT NULL
)

INSERT INTO #ES_TxRx (bndcde) SELECT * FROM #ES_all_bndcdes

UPDATE #ES_TxRx SET #ES_TxRx.countTx = #ES_TX.numChannels FROM #ES_TxRx INNER JOIN #ES_TX ON #ES_TxRx.bndcde = #ES_TX.txband
UPDATE #ES_TxRx SET #ES_TxRx.countRx = #ES_RX.numChannels FROM #ES_TxRx INNER JOIN #ES_RX ON #ES_TxRx.bndcde = #ES_RX.rxband

UPDATE #ES_TxRx SET #ES_TxRx.blo = S.blo, #ES_TxRx.bhi = S.bhi FROM #ES_TxRx INNER JOIN fcsaSQL3.fcsa.main.sd_band AS S ON #ES_TxRx.bndcde = S.bndcde

SELECT * FROM #ES_TxRx ORDER BY blo