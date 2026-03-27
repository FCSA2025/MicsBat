USE [fcsa]
GO

/****** Object:  UserDefinedFunction [hulme].[CsvGetFieldAsInt]    Script Date: 11/3/2022 11:06:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-----------------------------------------------------------------------------------------------
-- This function returns the integer value given by a substring (field) of a prescribed string 
-- that corresponds to splitting that string w.r.t. a prescribed delimitter character. 
-- The fields are indexed starting at 1.
-----------------------------------------------------------------------------------------------
CREATE FUNCTION [hulme].[CsvGetFieldAsInt]
(
    @list NVARCHAR(MAX),      -- The string to be split.
    @delim CHAR(1),           -- The delimitter to be used.
	@fieldNum INT             -- The index of the field to be returned (>= 1).
)
RETURNS INT
BEGIN
	DECLARE @retVal INT

	SELECT @retVal = CAST(hulme.CsvGetField(@list, @delim, @fieldNum) AS INT)

	RETURN @retVal
END