USE fcsa

-- For test/investigation purposes only.
DECLARE @theCall1 CHAR(9) = 'VAD429'
DECLARE @theCall2 CHAR(9) = 'VEL878'
DECLARE @theBndcde CHAR(4) = '6B'

SET @theCall1 = 'VEL878'
SET @theCall2 = 'VAD429'

SELECT call1, call2, bndcde, anum, ause 
	FROM fcsaSQL3.fcsa.main.mt_ante
	WHERE call1=@theCall1 AND call2=@theCall2

SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx,
		 hulme.IsInRangeFloatTxRx(5850000, 6425000, freqtx, freqrx) AS inBand6A,
		 hulme.IsInRangeFloatTxRx(6425000, 6930000, freqtx, freqrx) AS inBand6D,
		 hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E
	FROM fcsaSQL3.fcsa.main.mt_chan
	WHERE call1=@theCall1 AND call2=@theCall2


SELECT call1, call2, bndcde, anum, ause 
	FROM hulme.mt_ante
	WHERE call1=@theCall1 AND call2=@theCall2

SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx,
		 hulme.IsInRangeFloatTxRx(5850000, 6425000, freqtx, freqrx) AS inBand6A,
		 hulme.IsInRangeFloatTxRx(6425000, 6930000, freqtx, freqrx) AS inBand6D,
		 hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E
	FROM hulme.mt_chan
	WHERE call1=@theCall1 AND call2=@theCall2
