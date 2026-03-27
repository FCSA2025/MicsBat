USE fcsa

--SELECT * FROM main.sd_band ORDER BY bndcde

IF OBJECT_ID('hulme.tempTS') IS NOT NULL DROP TABLE hulme.tempTS
SELECT bndcde, COUNT(bndcde) AS instances
	INTO hulme.tempTS
	FROM main.mt_ante AS T
	GROUP BY bndcde
	ORDER BY bndcde

--SELECT * FROM hulme.tempTS ORDER BY bndcde

IF OBJECT_ID('hulme.tempEsTx') IS NOT NULL DROP TABLE hulme.tempEsTx
SELECT txband, COUNT(txband) AS instances
    INTO hulme.tempEsTx
	FROM main.me_ante AS E
	GROUP BY txband
	ORDER BY txband

--SELECT * FROM hulme.tempEsTx ORDER BY txband

IF OBJECT_ID('hulme.tempEsRx') IS NOT NULL DROP TABLE hulme.tempEsRx
SELECT rxband, COUNT(rxband) AS instances
    INTO hulme.tempEsRx
	FROM main.me_ante AS E
	GROUP BY rxband
	ORDER BY rxband

--SELECT * FROM hulme.tempEsRx ORDER BY rxband 

IF OBJECT_ID('hulme.tempTally') IS NOT NULL DROP TABLE hulme.tempTally
SELECT S.bndcde, TS.instances AS TS, ESTX.instances AS ESTX, ESRX.instances AS ESRX
	INTO hulme.tempTally
	FROM main.sd_band AS S
	FULL OUTER JOIN hulme.tempTS AS TS ON S.bndcde = TS.bndcde
	FULL OUTER JOIN hulme.tempEsTx AS ESTX ON S.bndcde = ESTX.txband
	FULL OUTER JOIN hulme.tempEsRx AS ESRX ON S.bndcde = ESRX.rxband
	WHERE S.bndcde IS NOT NULL
	ORDER BY bndcde

UPDATE hulme.tempTally SET TS = 0 WHERE TS IS NULL	
UPDATE hulme.tempTally SET ESTX = 0 WHERE ESTX IS NULL
UPDATE hulme.tempTally SET ESRX = 0 WHERE ESRX IS NULL

SELECT * FROM hulme.tempTally ORDER BY bndcde