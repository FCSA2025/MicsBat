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