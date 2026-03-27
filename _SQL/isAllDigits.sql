USE [micsdev]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Returns BIT value 1 if the prescribed string contains only digits [0-9]; otherwise 0.

ALTER FUNCTION [hulme].[isAllDigits](@string as VARCHAR(MAX))
RETURNS BIT
BEGIN
	DECLARE @retVal BIT = 0;      -- initialized to false.
	DECLARE @i int;			      -- string index counter.
	DECLARE @isAllDigits BIT = 1; -- boolean: 1 = all characters in string are digits [0-9]; else 0.

	-- Need to guard against the pathological case.
	IF (LEN(@string) = 0)
		SET @retVal = 0
	ELSE
		-- So the string actually has some characters.
		-- Check that each character is a digit [0-9].
		BEGIN
			SET @i = 1
			SET @isAllDigits = 1;
			WHILE (@i <= LEN(@string)) 
				BEGIN
					IF ((ASCII(SUBSTRING(@string, @i, 1)) < 48) OR (ASCII(SUBSTRING(@string, @i, 1)) > 57))	
						SET @isAllDigits = 0

					SET @i = @i + 1
				END

			IF (@isAllDigits = 0) 
				SET @retVal = 0;
			ELSE                          
				SET @retVal = 1;
		END

	RETURN @retVal;
END