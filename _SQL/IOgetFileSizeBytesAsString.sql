USE [fcsa]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

----------------------------------------------------------------------------------
-- This function returns the file size in bytes of a Windows file at a prescribed,
-- fully-qualified network file path, e.g.
--
--    '\\10.1.1.12\D$\Inetpub\wwwroot\mics\userdirs\xci\xci10\tsip_0078_1.STATSUM'
----------------------------------------------------------------------------------
ALTER function [hulme].[IOgetFileSizeBytesAsString]
(
	@fullNetworkFilePath  AS VARCHAR(MAX)   -- e.g. \\10.1.1.12\D$\Inetpub\wwwroot\mics\userdirs\xci\xci10\tsip_0078_1.STATSUM
)
RETURNS VARCHAR(MAX)
BEGIN
	DECLARE @cmd           VARCHAR(8000)		-- used to construct the Windows command to be executed.
	DECLARE @fileSizeStr   VARCHAR(8000)        -- used to capture the substring that contains the file size.
	DECLARE @fileSizeBytes INT                  -- stores the file size in bytes.

	DECLARE @uniqueTag VARCHAR(20)
	SELECT @uniqueTag = LTRIM(CAST(CONVERT(BIGINT, CAST(GETDATE() AS FLOAT) * 10000000000) AS VARCHAR(20)))

	DECLARE @tempFilePath VARCHAR(256)
	SET @tempFilePath = '\\10.1.1.46\C$\Windows\Temp\andrew' + @uniqueTag + '.txt'

	-- Construct the complete Windows command.
	-- We will execute the Windows command 'DIR' with specific command-line arguments;
	-- this is then piped into a 'FIND' command that retains only lines that contain the word 'File';
	-- finally, we steam the result out to a temporary text file that we can read from later.
	--SET @cmd = 'DIR ' + @fullNetworkFilePath + ' /N /A:-D /-C  | FIND "File" > ' + @tempFilePath

	-- Execute the complete Windows command from within T-SQL.
	EXEC xp_cmdshell @cmd

	-- We now use the UDF fcsa.hulme.IOreadfileAsTableFP() to read the contents of a temporary file into table.
	-- This table should only contain a single record and its 'line' column should be something like:
	--
	--                "               1 File(s)          62369 bytes"
	--                                         |             |
	--                 123456789012345678901234567890123456789012345
    --                                         123456789012345
	--
	-- The file size string that we need to get starts at character 25 and has max length 15 characters.

	DECLARE @lineStr VARCHAR(8000)
	SELECT @lineStr = line FROM fcsa.hulme.IOreadfileAsTableFP(@tempFilePath)
	--SET @tempStr = CONCAT('ECHO ', @tempStr, ' > ' + @tempFilePath)
	--EXEC xp_cmdshell @tempStr
/*	
	DECLARE @tryCount INT = 0;
	WHILE (@tryCount < 100)
		BEGIN
		 SET @tryCount = @tryCount + 1
		 --SELECT @fileSizeStr = line  FROM  fcsa.hulme.IOreadfileAsTableFP(@tempFilePath)
		 /*
		 SELECT @fileSizeBytes = 
			CASE
				WHEN TRY_CAST(SUBSTRING(@fileSizeStr, 25, 15) AS INT) IS NULL THEN -1
				ELSE TRY_CAST(SUBSTRING(@fileSizeStr, 25, 15) AS INT)
			END
		 IF @fileSizeBytes = -1
			BEGIN
		 		SET @tempStr = CONCAT('ECHO ', 'ERROR: IOgetFileSizeBytesAsString(): ', @tryCount, '   ', @fullNetworkFilePath, ' >> ' + @tempFilePath)
				EXEC xp_cmdshell @tempStr
				CONTINUE
			END
		 ELSE 
			SET @tryCount = 666
		*/
		END
*/
	--SELECT @fileSizeBytes = TRY_CAST(SUBSTRING(line, 25, 15) AS INT) FROM  fcsa.hulme.IOreadfileAsTableFP('\\10.1.1.46\C$\Windows\Temp\andrew.txt')

	-- Extract the substring that provides the file size in bytes.
	DECLARE @string VARCHAR(MAX) = ''
	--SELECT @string = line FROM fcsa.hulme.IOreadfileAsTableFP(@tempFilePath)
	SET @string = SUBSTRING(@lineStr, 25, 15)

	-- Clean up.
	SET @cmd = 'DEL ' + @tempFilePath
	EXEC xp_cmdshell @cmd

	RETURN @string
END
GO

