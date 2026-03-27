---------------------------------------------------------------------------------------------------
--
-- This procedure writes a prescribed T-SQL string value to a Windows text file at a 
-- fully-qualified, network path. Two modes of writing are supported: OVERWRITE and APPEND.
--
--    OVERWRITE:  deletes the file if it already exists, creates a new file and writes to it.
--
--    APPEND:     the file MUST already exist; text is appended at the end of current content.
--
-- Example:       
--          DECLARE @filepath  VARCHAR(MAX) = '\\10.1.1.46\D$\Users\ahulme\temp\myFile.txt';
--          DECLARE @LF CHAR(1) = CHAR(10)
--          DECLARE @message VARCHAR(MAX)   = CONCAT('Hello World!', @LF, '2nd line.')
--
--          EXECUTE acct.IOwriteStringToFile @message, @filePath, 0
--
-- Source:   https://www.red-gate.com/simple-talk/wp-content/uploads/2017/07/spWriteStringTofile.txt
--
-- See also: https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/reading-and-writing-files-in-sql-server-using-t-sql/
--           https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/the-tsql-of-text-files/
---------------------------------------------------------------------------------------------------

CREATE PROCEDURE acct.IOwriteStringToFile
 (
    @String Varchar(max),     -- T-SQL string value, up to 2 Gigabytes in SQL Server 2012.
    @FilePath VARCHAR(255),   -- The full network path of the text file to be writen to.
	@WriteMode BIT            -- 0 for OVERWRITE, 1 for APPEND.  
)
AS
DECLARE @objFileSystem int,
        @objTextStream int,
		@objErrorObject int,
		@strErrorMessage Varchar(1000),
	    @Command varchar(1000),
	    @hr int

set nocount on

select @strErrorMessage='opening the File System Object'
EXECUTE @hr = sp_OACreate  'Scripting.FileSystemObject' , @objFileSystem OUT

if @WriteMode = 0      -- OVERWRITE
	BEGIN
		if @HR=0 Select @objErrorObject=@objFileSystem , @strErrorMessage='Creating file "'+@FilePath+'"'
		if @HR=0 execute @hr = sp_OAMethod   @objFileSystem   , 'CreateTextFile', @objTextStream OUT, @FilePath,2, False
		
		if @HR=0 Select @objErrorObject=@objTextStream, 
			@strErrorMessage='writing to the file "'+@FilePath+'"'
		if @HR=0 execute @hr = sp_OAMethod  @objTextStream, 'Write', Null, @String
		
		if @HR=0 Select @objErrorObject=@objTextStream, @strErrorMessage='closing the file "'+@FilePath+'"'
		if @HR=0 execute @hr = sp_OAMethod  @objTextStream, 'Close'
	END
else                   -- APPEND
	BEGIN
		if @hr=0 Select @objErrorObject=@objFileSystem , @strErrorMessage='Appending to file "'+@FilePath+'"'
		if @hr=0 execute @hr = sp_OAMethod   @objFileSystem   , 'OpenTextFile', @objTextStream OUT, @FilePath, 8, true, 0
		
			if @hr<>0
			begin
			raiserror ('ALPHA',16,1)
			end
		
		if @hr=0 Select @objErrorObject=@objTextStream, 
			@strErrorMessage='appending to the file "'+@FilePath+'"'
		if @hr=0 execute @hr = sp_OAMethod  @objTextStream, 'Write', Null, @String
		
		if @hr=0 Select @objErrorObject=@objTextStream, @strErrorMessage='closing the file "'+@FilePath+'"'
		if @hr=0 execute @hr = sp_OAMethod  @objTextStream, 'Close'
	END

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
	raiserror (@strErrorMessage,16,1)
	end
EXECUTE  sp_OADestroy @objTextStream
EXECUTE  sp_OADestroy @objFileSystem

