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
ALTER function [hulme].[IOgetFileSizeBytes]
(
	@fullNetworkFilePath  AS VARCHAR(MAX)   -- e.g. \\10.1.1.12\D$\Inetpub\wwwroot\mics\userdirs\xci\xci10\tsip_0078_1.STATSUM
)
RETURNS INT
BEGIN

	DECLARE @cmd           VARCHAR(8000)		-- used to construct the Windows command to be executed.
	DECLARE @fileSizeBytes INT                  -- stores the file size in bytes.

	-- Construct the complete Windows command.
	-- We will execute the Windows command 'DIR' with specific command-line arguments;
	-- this is then piped into a 'FIND' command that retains only lines that contain the word 'File';
	-- finally, we steam the result out to a temporary text file that we can read from later.
	SET @cmd = 'DIR ' + @fullNetworkFilePath + ' /N /A:-D /-C  | FIND "File" > \\10.1.1.46\C$\Windows\Temp\andrew.txt'

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
	SELECT @fileSizeBytes = CAST(SUBSTRING(line, 25, 15) AS INT) FROM  fcsa.hulme.IOreadfileAsTableFP('\\10.1.1.46\C$\Windows\Temp\andrew.txt')

	-- And we're done.
	RETURN @fileSizeBytes
END
GO

