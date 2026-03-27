USE fcsa;

SELECT * FROM fcsasql5D.fcsa.hulme.srspBands

-----------------------------------------------------------------------
IF OBJECT_ID('hulme.temp') IS NOT NULL DROP TABLE hulme.temp

SELECT Distinct M.bndcde AS 'bndcde', CONCAT(S.SRSP, '-',span) AS 'SRSPref'
	INTO hulme.temp
	FROM main.sd_band AS M
	INNER JOIN hulme.srspBands AS S 
		ON (hulme.isInRangeFloat(M.blo, M.bhi, S.freqLoKHz + 1) = 1) OR (hulme.isInRangeFloat(M.blo, M.bhi, S.freqHiKHz - 1) = 1)
	ORDER BY M.bndcde

--SELECT * FROM hulme.temp

-----------------------------------------------------------------------
----- Aggregate over SRSPref ------------------------------------------
-----------------------------------------------------------------------
DECLARE @counter INT;
SET @counter = 1;

set nocount on;

;WITH Partitioned AS
(
    SELECT 
        bndcde,
        SRSPref,
        ROW_NUMBER() OVER (PARTITION BY bndcde ORDER BY SRSPref) AS SRSPrefNumber,
        COUNT(*) OVER (PARTITION BY bndcde) AS NameCount
    FROM hulme.temp
),
Concatenated AS
(
    SELECT bndcde, CAST(SRSPref AS nvarchar(MAX)) AS SRSPrefFullName, SRSPref, SRSPrefNumber, NameCount FROM Partitioned WHERE SRSPrefNumber = 1

    UNION ALL

    SELECT 
        P.bndcde, CAST(SRSPrefFullName + '; ' + P.SRSPref AS nvarchar(MAX)), P.SRSPref, P.SRSPrefNumber, P.NameCount
    FROM Partitioned AS P
        INNER JOIN Concatenated AS C ON P.bndcde = C.bndcde AND P.SRSPrefNumber = C.SRSPrefNumber + 1
)
SELECT 
    bndcde,
    SRSPrefFullName
FROM Concatenated
WHERE SRSPrefNumber = NameCount
ORDER BY bndcde

-----------------------------------------------------------------------
----- Aggregate over bndcde --------------------------------------------
-----------------------------------------------------------------------

SET @counter = 1;

set nocount on;

;WITH Partitioned AS
(
    SELECT 
        SRSPref,
        bndcde,
        ROW_NUMBER() OVER (PARTITION BY SRSPref ORDER BY bndcde) AS bndcdeNumber,
        COUNT(*) OVER (PARTITION BY SRSPref) AS NameCount
    FROM hulme.temp
),
Concatenated AS
(
    SELECT SRSPref, CAST(bndcde AS nvarchar(MAX)) AS bndcdeFullName, bndcde, bndcdeNumber, NameCount FROM Partitioned WHERE bndcdeNumber = 1

    UNION ALL

    SELECT 
        P.SRSPref, CAST(bndcdeFullName + '; ' + P.bndcde AS nvarchar(MAX)), P.bndcde, P.bndcdeNumber, P.NameCount
    FROM Partitioned AS P
        INNER JOIN Concatenated AS C ON P.SRSPref = C.SRSPref AND P.bndcdeNumber = C.bndcdeNumber + 1
)

SELECT 
    SRSPref,
    bndcdeFullName
FROM Concatenated
WHERE bndcdeNumber = NameCount
ORDER BY SRSPref
