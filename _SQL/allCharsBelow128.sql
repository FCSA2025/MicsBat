USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[allCharsBelow128]    Script Date: 11/19/2021 1:57:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns a BIT value indicating if the prescribed character string comprises
-- characters whose ASCII code lies below 128.
-- BIT value 1 means all character byte codes are below 128.
-- BIT value 0 means some character byte codes are 128 or above.

ALTER FUNCTION [hulme].[allCharsBelow128](@string VARCHAR(1000))
RETURNS BIT
BEGIN

	DECLARE @numChars INT = LEN(@string);
	DECLARE @index INT = 1;
	DECLARE @char CHAR(1);

	IF @numChars = 0 RETURN 1;

	WHILE @index <= @numChars
	BEGIN
		SET @char = SUBSTRING(@string, @index, 1)
	    IF ASCII(@char) >= 128 RETURN 0
        SET @index = @index + 1
	END

	RETURN 1;

END