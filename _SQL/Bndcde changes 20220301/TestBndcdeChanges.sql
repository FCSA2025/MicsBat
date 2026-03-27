USE fcsa

SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('6E')
SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('7A')
SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('6E','7A')

SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('6E')
SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('7A')
SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('6E','7A')

---------------------------------------------------------------------

SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('6E')
SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('7A')
SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('6E','7A')

SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('6E')
SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('7A')
SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('6E','7A')

----------------------------------------------------------------------

SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('18A')
SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('18B')
SELECT COUNT(*) FROM hulme.mt_ante WHERE bndcde IN ('18A','18B')

SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('18A')
SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('18B')
SELECT COUNT(*) FROM hulme.mt_chan WHERE bndcde IN ('18A','18B')

/*
SELECT call1, call2, bndcde, anum FROM hulme.mt_ante WHERE bndcde='6E'
UNION
SELECT call1, call2, bndcde, anum FROM hulme.mt_ante WHERE bndcde='7A'
ORDER BY call1, call2, bndcde, anum
*/
SELECT A.call1, A.call2, A.bndcde, A.anum, C.chid, MAX(C.antnumbtx1), MAX(C.antnumbtx2), MAX(C.antnumbrx1), MAX(C.antnumbrx2), MAX(C.antnumbrx3)
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE (C.call1='XOA517' AND C.call2='XOA519') AND (C.bndcde='6E' OR C.bndcde='7A') AND ((antnumbtx1 = A.anum) OR (antnumbtx2 = A.anum) OR (antnumbrx1 = A.anum) OR (antnumbrx2 = A.anum) OR (antnumbrx3 = A.anum))--WHERE A.call1='WPVL20301' AND A.call2='WPVL20303'
	GROUP BY A.call1, A.call2, A.bndcde, A.anum, C.chid, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	ORDER BY antnumbrx1 DESC, antnumbtx1 DESC, call1, call2, bndcde, anum, chid
/*
SELECT call1, call2, bndcde, chid FROM hulme.mt_chan ORDER BY call1, call2, bndcde, chid
*/
DECLARE @theCall1 CHAR(9) = 'CFA719'
DECLARE @theCall2 CHAR(9) = 'CFA720'
DECLARE @theBndcde CHAR(4) = '6B'

SELECT call1, call2, bndcde, anum
     FROM hulme.mt_ante
	 WHERE (call1=@theCall1 AND call2=@theCall2 /*AND bndcde=@theBndcde*/)
SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(5850000, 6425000, freqtx, freqrx) AS inBand6A 
	FROM hulme.mt_chan
	 WHERE (call1=@theCall1 AND call2=@theCall2 /*AND bndcde=@theBndcde*/)


SELECT A.call1, A.call2, A.bndcde, A.anum, C.chid, C.freqtx, C.freqrx, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE (C.call1=@theCall1 AND C.call2=@theCall2) AND C.bndcde=@theBndcde--(C.bndcde='6E' OR C.bndcde='7A') AND ((antnumbtx1 = A.anum) OR (antnumbtx2 = A.anum) OR (antnumbrx1 = A.anum) OR (antnumbrx2 = A.anum) OR (antnumbrx3 = A.anum))--WHERE A.call1='WPVL20301' AND A.call2='WPVL20303'
	--GROUP BY A.call1, A.call2, A.bndcde, A.anum, C.chid, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	ORDER BY call1, call2, bndcde, anum, chid

SELECT A.call1, A.call2, A.bndcde, A.anum, C.chid,  C.freqtx, C.freqrx, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE  C.bndcde='6B'
	ORDER BY call1, call2, bndcde, anum, chid
