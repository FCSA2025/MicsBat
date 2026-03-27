USE fcsa
GO

	DECLARE @cmd				VARCHAR(8000)	-- used to construct the Windows command to be executed.
	DECLARE @localIPaddress		VARCHAR(15)		-- e.g. '10.1.1.13'
	DECLARE @tempFilePath		VARCHAR(256)    -- the full network file path to a temporary text file.

	SET @cmd = '<nul (set/p z=) > \\10.1.1.46\d$\users\ahulme\temp\lion.txt'
	EXEC XP_CMDSHELL @cmd, NO_OUTPUT

	SET @cmd = 'FOR /f %a in (''DIR \\10.1.1.46\d$\Inetpub\wwwroot\mics\userdirs /S /B /A:-D'') do echo %~fa %~za %~ta  >> \\10.1.1.46\d$\users\ahulme\temp\lion.txt'

	-- Execute the complete Windows command from within T-SQL.
    EXEC XP_CMDSHELL @cmd, NO_OUTPUT

	SELECT * FROM hulme.IOreadfileAsTableFP('\\10.1.1.46\d$\users\ahulme\temp\lion.txt')

	DECLARE @line VARCHAR(999) = '\\10.1.1.46\d$\Inetpub\wwwroot\mics\userdirs\abccom\abccom2\tsip_nh_dragonmtn.ERR 1659 10/01/2021 06:01 PM  '  

	DECLARE @dateTime DATETIME;

	SET @dateTime = CAST ('10/01/2021 06:01 PM  ' AS DATETIME)

	SELECT @dateTime

	DECLARE @charIndexFirstSpace INT
	DECLARE @charIndexSecondSpace INT

	SET @charIndexFirstSpace = CHARINDEX(' ', @line)
	SET @charIndexSecondSpace = CHARINDEX(' ', @line, @charIndexFirstSpace + 1)

	SELECT @charIndexFirstSpace, @charIndexSecondSpace 

	DECLARE @filePath VARCHAR(256)
	DECLARE @fileSizeStr VARCHAR(256)
	DECLARE @dateTimeStr VARCHAR(256)

	SET @filePath = '#' + SUBSTRING(@line, 1, @charIndexFirstSpace - 1) + '#'
	SET @fileSizeStr = '#' + SUBSTRING(@line, @charIndexFirstSpace + 1, @charIndexSecondSpace - @charIndexFirstSpace - 1) + '#'
	SET @dateTimeStr = '#' + SUBSTRING(@line, @charIndexSecondSpace + 1, LEN(@line) - @charIndexSecondSpace) + '#'
	SELECT @filePath, @fileSizeStr, @dateTimeStr

IF OBJECT_ID('tempdb..#MyTempTable') IS NOT NULL DROP TABLE #MyTempTable
CREATE TABLE #MyTempTable (
    line VARCHAR(512) PRIMARY KEY,
	charIndexStartOperName INT NOT NULL,
	charIndexStartMicsID INT NOT NULL,
	charIndexEndMicsID INT NOT NULL,
	charIndexFirstSpace INT NOT NULL,
	charIndexSecondSpace INT NOT NULL
);

INSERT INTO #MyTempTable
	SELECT 
		T.line, 
		CHARINDEX('userdirs', T.line) + 9,
		CHARINDEX('\', T.line, CHARINDEX('userdirs\', T.line) + 9) + 1,
		CHARINDEX('\', T.line, CHARINDEX('\', T.line, CHARINDEX('userdirs', T.line) + 9) + 1) - 1,
		CHARINDEX(' ', T.line), 
		CHARINDEX(' ', T.line, CHARINDEX(' ', T.line) + 1)
	FROM hulme.IOreadfileAsTableFP('\\10.1.1.46\d$\users\ahulme\temp\lion.txt') AS T

SELECT * FROM #MyTempTable

SELECT 
	SUBSTRING(line, 1, charIndexFirstSpace - 1) AS filePath,
	SUBSTRING(line, charIndexFirstSpace + 1, charIndexSecondSpace - charIndexFirstSpace - 1) AS fileSizeStr,
	SUBSTRING(line, charIndexSecondSpace + 1, LEN(line) - charIndexSecondSpace) AS dateTimeStr,
	SUBSTRING(line, charIndexStartOperName, charIndexStartMicsID - charIndexStartOperName - 1) AS oper,
	SUBSTRING(line, charIndexStartMicsID, charIndexEndMicsID - charIndexStartMicsID + 1) AS micsid
	FROM #MyTempTable
/*
SET @cmd = 'FOR /f %a in (''DIR \\10.1.1.46\d$\Inetpub\wwwroot\mics\userdirs /S /B /A:-D'') do echo %~fa %~za'
create table #output (output varchar(255) null)
insert #output exec master..xp_cmdshell @cmd
select * from #output where output is not null
--drop table #output
*/