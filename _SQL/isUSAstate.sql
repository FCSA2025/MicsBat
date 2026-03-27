USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[isUSAstate]    Script Date: 5/19/2021 11:24:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns a BIT value indicating if the prescribed two-letter state string is 
-- a valid U.S. state abbreviation, or not. 
ALTER FUNCTION [hulme].[isUSAstate](@state VARCHAR(2))
RETURNS BIT
BEGIN

	DECLARE @ret BIT; 

	IF (@state IN ('AK', 'AL', 'AR', 'AS', 'AZ', 'CA', 'CO', 'CT', 'DC', 'DE', 'FL', 'GA', 'GU', 'HI', 'IA', 'ID', 'IL', 'IN', 'KS', 'KY', 'LA', 'MA', 'MD', 'ME', 'MI', 'MN', 'MO', 'MP', 'MS', 'MT', 'NC', 'ND', 'NE', 'NH', 'NJ', 'NM', 'NV', 'NY', 'OH', 'OK', 'OR', 'PA', 'PR', 'RI', 'SC', 'SD', 'TN', 'TX', 'UT', 'VA', 'VI', 'VT', 'WA', 'WI', 'WV', 'WY') ) 
		SET @ret = 1; 
	ELSE 
		SET @ret = 0;

	RETURN @ret;

END