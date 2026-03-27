USE [fcsa]
GO

/****** Object:  UserDefinedFunction [hulme].[CsvToTable]    Script Date: 11/3/2022 11:06:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-----------------------------------------------------------------------------------------------
-- This function returns a table with dimensions N rows by 2 columns where the row 
-- values are the result of 'splitting' the prescribed list string w.r.t. the prescribed 
-- delimitter character.
--                     1st column: 'field' is an integer row index from 1 to N.
--                     2nd column: 'value' is the split substring for the field value.
-----------------------------------------------------------------------------------------------
CREATE FUNCTION [hulme].[CsvToTable]
(
    @list NVARCHAR(MAX),      -- The string to be split.
    @delim CHAR(1)            -- The delimitter to be used.
)
RETURNS TABLE
AS
	RETURN 
		SELECT * FROM hulme.SplitString_T(@list, @delim)

