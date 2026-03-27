---------------------------------------------------------------------------------------------------
--
-- This function returns a table containing the names of all of the files
-- in a Windows directory at a prescribed full network path. The returned 
-- table has just one column:
--
--       Column 1:  'fileName'    - the short name of a file, e.g. 'tsip_sk8812_v1_1.EXEC'
--
-- Example:       
--           DECLARE @filePath  VARCHAR(MAX) = '\\10.1.1.12\D$\Inetpub\wwwroot\mics\userdirs\xci\xci10';
--
--           SELECT fileName FROM hulme.IOreadfileAsTableFP(@filePath)
--
-- Source:   https://www.red-gate.com/simple-talk/wp-content/uploads/2017/07/uftReadFileAsTable.txt
--
-- See also: https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/reading-and-writing-files-in-sql-server-using-t-sql/
--           https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/the-tsql-of-text-files/
---------------------------------------------------------------------------------------------------

ALTER FUNCTION [hulme].[IOlistFilesinDirAsTable]
(
    @fullNetworkFilePath VARCHAR(255)      -- The full network path of the text file to be read from.
)
RETURNS 
@listOfFiles TABLE
(
-- The following line declares the structure of the table as having a single columns called 'fileName'.
fileName varchar(256)) 

AS
BEGIN
	DECLARE @cmd				VARCHAR(8000)	-- used to construct the Windows command to be executed.
	DECLARE @localIPaddress		VARCHAR(15)		-- e.g. '10.1.1.13'
	DECLARE @tempFilePath		VARCHAR(256)    -- the full network file path to a temporary text file.

	-- Get the IP address of this SQL Server.
	SELECT @localIPaddress = LOCAL_NET_ADDRESS FROM SYS.DM_EXEC_CONNECTIONS WHERE SESSION_ID = @@SPID

	-- Construct the full network file path to a temporary file.
	SET @tempFilePath = '\\' + @localIPaddress + '\C$\Windows\Temp\temp.txt'

	-- We will execute the Windows command 'DIR' with the /B (bare format) option we stream the result 
	-- out to a temporary text file that we can read from later.
	SET @cmd = 'DIR ' + @fullNetworkFilePath + ' /B > ' + @tempFilePath

	-- Execute the complete Windows command from within T-SQL.
	EXEC xp_cmdshell @cmd

	-- Now read back from the temporary text file and populate the table @listOfFiles.
	INSERT INTO @listOfFiles SELECT line FROM hulme.IOreadfileAsTableFP(@tempFilePath)

	-- Clean up.
	SET @cmd = 'DEL ' + @tempFilePath
	EXEC xp_cmdshell @cmd

	RETURN 
END