USE [fcsa]
GO

/****** Object:  UserDefinedFunction [hulme].[SplitString_T]    Script Date: 11/3/2022 11:06:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-----------------------------------------------------------------------------------------------
-- This function returns a table with dimensions N rows by 2 columns where the row 
-- values are the result of 'splitting' the prescribed list string w.r.t. the prescribed 
-- delimitter string.
--                     1st column: 'field' is an integer row index from 1 to N.
--                     2nd column: 'value' is the split substring for the field value.
-----------------------------------------------------------------------------------------------
CREATE FUNCTION [hulme].[SplitString_T]
(
    @List NVARCHAR(MAX),
    @Delim VARCHAR(255)
)
RETURNS TABLE
AS
    RETURN ( SELECT ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS field, [Value] FROM 
      ( 
        SELECT 
          [Value] = LTRIM(RTRIM(SUBSTRING(@List, [Number],
          CHARINDEX(@Delim, @List + @Delim, [Number]) - [Number])))
        FROM (SELECT Number = ROW_NUMBER() OVER (ORDER BY name)
          FROM sys.all_objects) AS x
          WHERE Number <= LEN(@List)
          AND SUBSTRING(@Delim + @List, [Number], LEN(@Delim)) = @Delim
      ) AS y
    );
GO

