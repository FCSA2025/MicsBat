---------------------------------------------------------------------------------------------------
--
-- This function reads text, one line at a time, and inserts it into a table  where each line of 
-- text is a row, with a primary key based on the line number. The table has two columns:
--
--       Column 1:  'lineNum'    - the number of the line in the text file starting at 1.
--       Column 2:  'line'       - the line of text excluding \r and \n characters.
--
-- Example:       
--           DECLARE @filePath  VARCHAR(MAX) = '\\10.1.1.46\D$\Users\ahulme\temp\Bill_WebMICS\DailyStorage\MyFile.txt';
--
--           SELECT lineNum, line FROM acct.IOreadfileAsTable(@filePath)
--
-- Source:   https://www.red-gate.com/simple-talk/wp-content/uploads/2017/07/uftReadFileAsTable.txt
--
-- See also: https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/reading-and-writing-files-in-sql-server-using-t-sql/
--           https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/the-tsql-of-text-files/
---------------------------------------------------------------------------------------------------

CREATE FUNCTION [acct].[IOreadfileAsTable]
(
    @FilePath VARCHAR(255)      -- The full network path of the text file to be read from.
)
RETURNS 
@File TABLE
(
-- The following two lines declare the structure of the table as having two columns called
-- 'lineNum' and 'line'.
lineNum int identity(1,1),  -- This declares an auto-incrementing column, start at 1, incrementing by 1.
line varchar(8000)) 

AS
BEGIN

DECLARE  @objFileSystem int
        ,@objTextStream int,
		@objErrorObject int,
		@strErrorMessage Varchar(1000),
	    @Command varchar(1000),
	    @hr int,
		@String VARCHAR(8000),
		@YesOrNo INT

select @strErrorMessage='opening the File System Object'
EXECUTE @hr = sp_OACreate  'Scripting.FileSystemObject' , @objFileSystem OUT


if @HR=0 Select @objErrorObject=@objFileSystem, @strErrorMessage='Opening file "'+@FilePath+'"',@command=@FilePath

if @HR=0 execute @hr = sp_OAMethod   @objFileSystem  , 'OpenTextFile'
	, @objTextStream OUT, @command,1,false,0--for reading, FormatASCII

WHILE @hr=0
	BEGIN
	if @HR=0 Select @objErrorObject=@objTextStream, 
		@strErrorMessage='finding out if there is more to read in "'+@filePath+'"'
	if @HR=0 execute @hr = sp_OAGetProperty @objTextStream, 'AtEndOfStream', @YesOrNo OUTPUT

	IF @YesOrNo<>0  break
	if @HR=0 Select @objErrorObject=@objTextStream, 
		@strErrorMessage='reading from the output file "'+@filePath+'"'
	if @HR=0 execute @hr = sp_OAMethod  @objTextStream, 'Readline', @String OUTPUT
	INSERT INTO @file(line) SELECT @String
	END

if @HR=0 Select @objErrorObject=@objTextStream, 
	@strErrorMessage='closing the output file "'+@filePath+'"'
if @HR=0 execute @hr = sp_OAMethod  @objTextStream, 'Close'


if @hr<>0
	begin
	Declare 
		@Source varchar(255),
		@Description Varchar(255),
		@Helpfile Varchar(255),
		@HelpID int
	
	EXECUTE sp_OAGetErrorInfo  @objErrorObject, 
		@source output,@Description output,@Helpfile output,@HelpID output
	Select @strErrorMessage='Error whilst '
			+coalesce(@strErrorMessage,'doing something')
			+', '+coalesce(@Description,'')
	insert into @File(line) select @strErrorMessage
	end
EXECUTE  sp_OADestroy @objTextStream
	-- Fill the table variable with the rows for your result set

-----------------------------------------------------------------------------------
-- AH:    If present, remove the UTF-8 Byte Order Mark (BOM) from the first line. 
-----------------------------------------------------------------------------------
UPDATE @File
	SET line = CASE 
					WHEN ((SUBSTRING(line, 1, 1) = 0xEF) AND 
					      (SUBSTRING(line, 2, 1) = 0xBB) AND 
						  (SUBSTRING(line, 3, 1) = 0xBF)) 
						  THEN RIGHT(line, LEN(line) - 3)
					ELSE line
			   END
	WHERE lineNum = 1


	RETURN 
END