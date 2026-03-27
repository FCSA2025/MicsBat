--*********************************************************************
-- Description  : Creates a comma separated list of all columns in a
--                table, adding square brackets if a column
--                name contains spaces.
-- Author/Owner : © SQL Matters 2011-13
--*********************************************************************
 
DECLARE @TABLE_NAME VARCHAR(128)
DECLARE @SCHEMA_NAME VARCHAR(128)
 
-----------------------------------------------------------------------
-- *** User Customisation
 
-- Set up the name of the table here :
SET @TABLE_NAME = 'sd_eqpt'
-- Set up the name of the schema here, or just leave set to 'dbo' :
SET @SCHEMA_NAME = 'main'
 
-----------------------------------------------------------------------
 
DECLARE @vvc_ColumnName VARCHAR(128)
DECLARE @vvc_ColumnList VARCHAR(MAX)

IF @SCHEMA_NAME =''
  BEGIN
  PRINT 'Error : No schema defined!'
  RETURN
  END
 
IF NOT EXISTS (SELECT * FROM sys.tables T JOIN sys.schemas S
          ON T.schema_id=S.schema_id
          WHERE T.Name=@TABLE_NAME AND S.name=@SCHEMA_NAME)
  BEGIN
  PRINT 'Error : The table '''+@TABLE_NAME+''' in schema '''+
        @SCHEMA_NAME+''' does not exist in this database!'
  RETURN
  END
 
DECLARE TableCursor CURSOR FAST_FORWARD FOR
SELECT   CASE WHEN PATINDEX('% %',C.name) > 0
         THEN '['+ C.name +']'
         ELSE C.name
         END
FROM     sys.columns C
JOIN     sys.tables T
ON       C.object_id  = T.object_id
JOIN     sys.schemas S
ON       S.schema_id  = T.schema_id
WHERE    T.name    = @TABLE_NAME
AND      S.name    = @SCHEMA_NAME
ORDER BY column_id


SET @vvc_ColumnList=''
 
OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @vvc_ColumnName

WHILE @@FETCH_STATUS=0
  BEGIN
  SET @vvc_ColumnList = @vvc_ColumnList + @vvc_ColumnName

  -- get the details of the next column
  FETCH NEXT FROM TableCursor INTO @vvc_ColumnName

  -- add a comma if we are not at the end of the row
  IF @@FETCH_STATUS=0
    SET @vvc_ColumnList = @vvc_ColumnList + ','
  END

CLOSE TableCursor
DEALLOCATE TableCursor

PRINT 'Here is the comma separated list of column names :'
PRINT '--------------------------------------------------'
PRINT @vvc_ColumnList