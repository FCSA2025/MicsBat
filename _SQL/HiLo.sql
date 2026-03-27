SELECT * FROM hulme.ft_testhilo_site ORDER BY call1



SELECT 
	C.call1, C.bndcde, C.call2, C.chid, B.bmidf, 
	C.freqtx, IIF(C.freqtx IS NULL, 'NotSet', IIF(C.freqtx > B.bmidf, 'Hi', 'Lo'))  As TxHiLo, 
	C.freqrx, IIF(C.freqrx IS NULL, 'NotSet', IIF(C.freqrx > B.bmidf, 'Hi', 'Lo')) As RxHiLo,
	B.badj
	FROM main.mt_site AS S
	INNER JOIN main.mt_chan AS C ON C.call1 = S.call1
	INNER JOIN main.sd_band AS B ON B.bndcde = C.bndcde
	WHERE 
		tsip.distance_hs(17524840, 44222480, S.latit, S.longit) <= 0.154 --and (bandwd1 & 0x001f0008 != 0 or bandwd2 & 0x01000000 != 0)
		--call1='=CGUARD1A'
		--call1='=CGUARD1B'
		--call1='=CGUARD1C'
	ORDER BY call1, bndcde, call2, chid



	SELECT * FROM main.sd_band ORDER BY blo --ORDER BY bhi DESC

	SELECT * FROM [FCSASQL3].fcsa.web.dblogger ORDER BY logstarttime DESC

    SELECT * FROM [FCSASQL5D].fcsa.web.dblogger ORDER BY logstarttime DESC

	SELECT * FROM adm.account_id

	SELECT * FROM main.sd_oper ORDER BY oper

	SELECT * FROM main.mt_chan 
		WHERE 
				(hulme.isInRangeFloat(7250000, 7300000, freqtx) = 1)
		     OR (hulme.isInRangeFloat(7250000, 7300000, freqrx) = 1)


Select * from web.tsip_queue --where TQ_Status not in ('D','F') order by TQ_Job