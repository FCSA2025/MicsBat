USE FCSA;
GO

-- Source table is adm.account_details
SELECT ultrixID 
	FROM fcsaSQL3.fcsa.adm.account_details 
	GROUP BY ultrixID 
	ORDER BY ultrixID

-- Source table is acct.daily_storage
SELECT operator 
	FROM fcsaSQL3.fcsa.acct.daily_storage 
	GROUP BY operator 
	ORDER BY operator

-- Source tables are main.mt_site and main.me_site
IF OBJECT_ID('hulme.temp') IS NOT NULL DROP TABLE hulme.temp
SELECT oper AS operator
	INTO fcsa.hulme.temp
	FROM fcsaSQL3.fcsa.main.mt_site
	WHERE oprtyp IN ('FT', 'FE')
	GROUP BY oper 
INSERT
	INTO fcsa.hulme.temp
	(operator)
	SELECT oper
	FROM fcsaSQL3.fcsa.main.me_site
	WHERE oprtyp IN ('FT', 'FE')
	GROUP BY oper 
SELECT DISTINCT * FROM hulme.temp ORDER BY operator

-- Source table is main.sd_oper
SELECT oper
	FROM fcsaSQL3.fcsa.main.sd_oper
	WHERE opnote = 'FC' AND email IS NOT NULL
	ORDER BY oper

-- Source table is ised.ISEDTStomicsoperBUI22A
SELECT MICSoper
	FROM fcsa.ised.ISEDTStomicsoperBUI22A
	WHERE MICSoprtyp IN ('FT', 'FE')
	GROUP BY MICSoper 
	ORDER BY MICSoper