CREATE TABLE hulme.TsipReports (
	[date] [char](10) NOT NULL,
	[time] [char](5) NOT NULL, 
	[paramFile] [varchar](64) NOT NULL,
	[runID] [varchar](8) NOT NULL,
	[reportType] [varchar](64) NOT NULL,
	[lineNum] [int] NOT NULL,
	[line] [varchar](MAX) NOT NULL
	 PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[time] ASC,
	[paramFile] ASC,
	[runID] ASC,
	[reportType] ASC,
	[lineNum] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO


SELECT * FROM hulme.TsipReports
DROP TABLE hulme.TsipReports
TRUNCATE TABLE hulme.TsipReports

SELECT * FROM hulme.w3_fil599v1_tsip_reports
DROP TABLE hulme.w3_fil599v1_tsip_reports
TRUNCATE TABLE hulme.w3_fil599v1_tsip_reports

SELECT * FROM hulme.tstest0183_tsip_reports
DROP TABLE hulme.tstest0183_tsip_reports
TRUNCATE TABLE hulme.tstest0183_tsip_reports

SELECT * FROM hulme.tstest0183_tsip_reports WHERE lineNum = 0
SELECT * FROM hulme.tstest0183_tsip_reports_reference WHERE lineNum = 0

SELECT R.paramFile, CASE WHEN (R.line = T.line) THEN 'PASS' ELSE 'FAIL' END AS TEST
	FROM hulme.tstest0183_tsip_reports_reference AS R
	INNER JOIN hulme.tstest0183_tsip_reports AS T ON ((R.paramFile = T.paramFile) AND (R.runID = T.runID) AND (R.reportType = T.reportType) AND (R.paramFile = T.paramFile) AND (R.lineNum = T.lineNum))
	WHERE R.lineNum = 0 AND R.reportType = ''

SELECT R.* , T.line AS lineUnderTest, CASE WHEN (R.line = T.line) THEN 'OK' ELSE '< DIFFERENCE' END AS MATCH
	FROM hulme.tstest0183_tsip_reports_reference AS R
	INNER JOIN hulme.tstest0183_tsip_reports AS T ON ((R.paramFile = T.paramFile) AND (R.runID = T.runID) AND (R.reportType = T.reportType) AND (R.paramFile = T.paramFile) AND (R.lineNum = T.lineNum))
	WHERE R.lineNum = 0

SELECT R.paramFile, R.runID, R.reportType , CASE WHEN (R.line = T.line) THEN '' ELSE 'X' END AS MATCH, R.lineNum, R.line, T.line
	FROM hulme.tstest0183_tsip_reports_reference AS R
	INNER JOIN hulme.tstest0183_tsip_reports AS T ON ((R.paramFile = T.paramFile) AND (R.runID = T.runID) AND (R.reportType = T.reportType) AND (R.paramFile = T.paramFile) AND (R.lineNum = T.lineNum))
	WHERE 
		R.runID = 'R03' AND R.reportType = 'CASEDET'

--============================

SELECT * FROM hulme.comtec2202_tsip_reports WHERE lineNum = 0

SELECT * 
	FROM micstest.hulme.comtec2202_tsip_reports 
	WHERE reportType = 'CASEDET' AND runID = 'TS1'
	ORDER BY lineNum

SELECT * INTO micsproddev.hulme.comtec2202_tsip_reports_reference FROM micsdev.hulme.comtec2202_tsip_reports_reference
SELECT * INTO micstest.hulme.comtec2202_tsip_reports FROM micsdev.hulme.comtec2202_tsip_reports_reference

DROP TABLE micstest.hulme.comtec2202_tsip_reports

-- FAULT -----------------
--  1 11,325.0000  B 11,305.0000  B 1 5   19.552 24.00  0.0  0.5 -39.90  0.00 85.66 H FSL    105.5   -87.9  (-101.6)   -97.8    -9.9
--report--E820BC028401DD00D58D855F9EBC5C2A
--global--DE4812A73D51FD3BD8B4D2422C492453
UPDATE micstest.hulme.comtec2202_tsip_reports
	SET line = '  1 11,325.0000  B 11,305.0000  B 1 5   19.552 24.00  0.0  0.5 -31.26  0.00 85.66 H FSL    105.5   -87.9  (-101.6)   -97.8    -9.9'
	WHERE runID = 'TS1' AND reportType = 'CASEDET' AND lineNum = 37
UPDATE micstest.hulme.comtec2202_tsip_reports
	SET line = 'C5C2AE820BC028401DD00D58D855F9EB'
	WHERE runID = 'TS1' AND reportType = 'CASEDET' AND lineNum = 0
UPDATE micstest.hulme.comtec2202_tsip_reports
	SET line = 'C5C2AE820BC028401DD00D58D855F9EB'
	WHERE runID = '' AND reportType = '' AND lineNum = 0

-- REPAIR -----------------
UPDATE micstest.hulme.comtec2202_tsip_reports
	SET line = '  1 11,325.0000  B 11,305.0000  B 1 5   19.552 24.00  0.0  0.5 -39.90  0.00 85.66 H FSL    105.5   -87.9  (-101.6)   -97.8    -9.9'
	WHERE runID = 'TS1' AND reportType = 'CASEDET' AND lineNum = 37
UPDATE micstest.hulme.comtec2202_tsip_reports
	SET line = 'E820BC028401DD00D58D855F9EBC5C2A'
	WHERE runID = 'TS1' AND reportType = 'CASEDET' AND lineNum = 0
UPDATE micstest.hulme.comtec2202_tsip_reports
	SET line = 'DE4812A73D51FD3BD8B4D2422C492453'
	WHERE runID = '' AND reportType = '' AND lineNum = 0

-- Fully automated, PASS/FAIL
SELECT R.paramFile, CASE WHEN (R.line = T.line) THEN 'PASS' ELSE 'FAIL' END AS TEST
	FROM micstest.hulme.comtec2202_tsip_reports AS R
	INNER JOIN micsdev.hulme.comtec2202_tsip_reports AS T 
		ON ((R.paramFile = T.paramFile) AND (R.runID = T.runID) 
		AND (R.reportType = T.reportType) AND (R.paramFile = T.paramFile) 
		AND (R.lineNum = T.lineNum))
	WHERE R.lineNum = 0 AND R.reportType = ''
	ORDER BY R.paramFile, R.runID, R.reportType, R.lineNum

-- Show checksums for all reports + global.
SELECT R.* , T.line AS line_Reference, CASE WHEN (R.line = T.line) THEN 'OK' ELSE '< DIFFERENCE' END AS MATCH
	FROM micstest.hulme.comtec2202_tsip_reports AS R
	INNER JOIN micsdev.hulme.comtec2202_tsip_reports AS T 
		ON ((R.paramFile = T.paramFile) AND (R.runID = T.runID) 
		AND (R.reportType = T.reportType) AND (R.paramFile = T.paramFile) 
		AND (R.lineNum = T.lineNum))
	WHERE R.lineNum = 0
	ORDER BY paramFile, runID, reportType, lineNum

-- Compare two different instances of the same report type.
SELECT R.paramFile, R.runID, R.reportType , 
	   CASE WHEN (R.line = T.line) THEN 'OK' ELSE 'DIFFERENT >' END AS MATCH, R.lineNum, R.line, T.line AS line_Reference
	FROM micstest.hulme.comtec2202_tsip_reports AS R
	INNER JOIN micsdev.hulme.comtec2202_tsip_reports AS T 
		ON ((R.paramFile = T.paramFile) AND (R.runID = T.runID) 
		AND (R.reportType = T.reportType) AND (R.paramFile = T.paramFile) 
		AND (R.lineNum = T.lineNum))
	WHERE 
		R.runID = 'TS1' AND R.reportType = 'CASEDET'
	ORDER BY paramFile, runID, reportType, lineNum

SELECT * 
	FROM micstest.hulme.comtec2202_tsip_reports AS R
	WHERE 
		R.runID = 'TS1' AND R.reportType = 'CASEDET'
	ORDER BY paramFile, runID, reportType, lineNum
SELECT * 
	FROM micsdev.hulme.comtec2202_tsip_reports 
	ORDER BY paramFile, runID, reportType, lineNum