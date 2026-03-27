USE [micsdev]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Returns BIT value 1 if the prescribed string is a valid ISED AuthorizationNumber; otherwise 0.
-- A valid AuthorizationNumber is a string like '010756239-002' or '010756239'.

ALTER FUNCTION [hulme].[isValidIsedAuthorizationNumber](@AuthNumb as VARCHAR(MAX))
RETURNS BIT
BEGIN
	DECLARE @retVal BIT = 0;      -- initialized to false.
	DECLARE @i int;			      -- string index counter.
	DECLARE @allDigits BIT = 1;   -- boolean: 1 = all characters in string are digits [0, 9]; else 0.

	-- A valid AuthorizationNumber has either qty. 9 or 13 characters.
	-- Check for the case of 9 characters.
	IF (LEN(@AuthNumb) = 9)
		BEGIN
			IF (hulme.isAllDigits(@AuthNumb) = 1) SET @retVal = 1;
		END

	-- Check for the case of 13 characters.
	ELSE IF (LEN(@AuthNumb) = 13) 
		BEGIN		
			-- Character 10 (index starting at 1) must be a hyphen.
			IF (SUBSTRING(@AuthNumb, 10, 1) = '-')
				BEGIN
					-- Remove the hyphen.
					DECLARE @tempStr VARCHAR(MAX) = CONCAT(SUBSTRING(@AuthNumb, 1, 9), SUBSTRING(@AuthNumb, 11, 3))

				    -- Check that the remaining characters are all digits [0, 9].
					IF (hulme.isAllDigits(@tempStr) = 1) SET @retVal = 1;
				END
		END

	RETURN @retVal;
END