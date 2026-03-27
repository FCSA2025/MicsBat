USE fcsa

SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante

SELECT ause, COUNT(ause) FROM fcsaSQL3.fcsa.main.mt_ante GROUP BY ause ORDER BY ause

SELECT A.call1, A.call2, A.bndcde, A.anum, A.ause, A.azmth, A.aht, C.chid, C.freqtx, C.freqrx, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	FROM fcsaSQL3.fcsa.main.mt_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE ((A.anum = C.antnumbtx1) OR (A.anum = C.antnumbtx2) OR (A.anum = C.antnumbrx1) OR (A.anum = C.antnumbrx2) OR (A.anum = C.antnumbrx3))
		AND A.ause = 'TR' AND ((C.antnumbtx1 != antnumbrx1) AND (C.antnumbtx1 != antnumbrx2))


SELECT S.oper, hulme.isMember(S.oper) AS member, hulme.isCanadianProvince(S.prov) AS canadian, A.call1, A.call2, A.bndcde, A.anum, A.ause, A.azmth, A.aht, C.chid, C.freqtx, C.freqrx, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	FROM fcsaSQL3.fcsa.main.mt_ante AS A
	INNER JOIN fcsaSQL3.fcsa.main.mt_site AS S ON S.call1 = A.call1
	INNER JOIN fcsaSQL3.fcsa.main.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE ((A.anum = C.antnumbtx1) OR (A.anum = C.antnumbtx2) OR (A.anum = C.antnumbrx1) OR (A.anum = C.antnumbrx2) OR (A.anum = C.antnumbrx3))
		AND A.ause = 'TR' AND ((C.antnumbtx1 != antnumbrx1) AND (C.antnumbtx1 != antnumbrx2))

-------------------------------------------------------------------------------------
DECLARE @theCall1 CHAR(9) = 'CGV867'
DECLARE @theCall2 CHAR(9) = 'CGF999'
--DECLARE @theBndcde CHAR(4) = '6B'
--DECLARE @theAnum SMALLINT = 11
--DECLARE @theChid CHAR(4) = ABCD

SELECT * FROM main.sd_band  ORDER BY blo, bhi
SELECT * FROM hulme.sd_band ORDER BY blo, bhi

SELECT * FROM fcsaSQL3.fcsa.main.mt_site WHERE call1=@theCall1
UNION
SELECT * FROM fcsaSQL3.fcsa.main.mt_site WHERE call1=@theCall2

SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2

SELECT call1, call2, bndcde, chid, freqtx, freqrx, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3 
	FROM fcsaSQL3.fcsa.main.mt_chan WHERE call1=@theCall1 AND call2=@theCall2

SELECT * FROM fcsasql3.fcsa.main.mt_chan WHERE 
	((freqtx IS NOT NULL) AND hulme.isInRangeFloat(10500000, 10550000, freqtx) = 1) OR ((freqrx IS NOT NULL) AND hulme.isInRangeFloat(10500000, 10550000, freqrx) = 1)

-------------------------------------------------------------------------------------

DECLARE @theCall1 CHAR(9) = '=HYONE01'
--DECLARE @theCall2 CHAR(9) = 'CGF999'
--DECLARE @theBndcde CHAR(4) = '6B'
--DECLARE @theAnum SMALLINT = 11
--DECLARE @theChid CHAR(4) = ABCD

SELECT * FROM fcsaSQL3.fcsa.main.mt_site WHERE call1=@theCall1

SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1

SELECT call1, call2, bndcde, chid, freqtx, freqrx, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3 
	FROM fcsaSQL3.fcsa.main.mt_chan WHERE call1=@theCall1
