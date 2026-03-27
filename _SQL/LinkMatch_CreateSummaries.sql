USE micsdev

-- Provide a summary of total matched and missing TAFL links rolled-up by operator.
SELECT 
	t_LicenseeName_oper AS oper, 
	COUNT(t_LicenseeName_oper) AS totalRecords,
	SUM(CASE WHEN x_LAF != 'LAF' THEN 1 ELSE 0 END) AS totalMissing,
	SUM(CASE WHEN x_LAF  = 'LA-' THEN 1 ELSE 0 END) AS LA_,
	SUM(CASE WHEN x_LAF  = 'L-F' THEN 1 ELSE 0 END) AS L_F,
	SUM(CASE WHEN x_LAF  = '---' THEN 1 ELSE 0 END) AS ___	
	FROM hulme.LinkMatch_TaflToMdb_20231004
	GROUP BY t_LicenseeName_oper
	ORDER BY t_LicenseeName_oper

-- Provide a summary of total matched and phantom MDB links rolled-up by operator.
SELECT 
	m_oper AS oper, 
	COUNT(m_oper) AS totalRecords,
	SUM(CASE WHEN x_LAF != 'LAF' THEN 1 ELSE 0 END) AS totalPhantoms,
	SUM(CASE WHEN x_LAF  = 'LA-' THEN 1 ELSE 0 END) AS LA_,
	SUM(CASE WHEN x_LAF  = 'L-F' THEN 1 ELSE 0 END) AS L_F,
	SUM(CASE WHEN x_LAF  = '---' THEN 1 ELSE 0 END) AS ___	
	FROM hulme.LinkMatch_MdbToTafl_20231004
	GROUP BY m_oper
	ORDER BY m_oper