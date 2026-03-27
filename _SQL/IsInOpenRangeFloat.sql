USE [micsdev]
GO
/****** Object:  UserDefinedFunction [hulme].[isInOpenRangeFloat]    Script Date: 6/9/2021 11:08:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Returns BIT value 1 if a prescribed test value lies in the open interval (lower, upper); otherwise 0.
-- If the @testVal is NULL this UDF returns 1.
ALTER function [hulme].[isInOpenRangeFloat](@lower as float, @upper as float, @testVal as float)
RETURNS BIT
BEGIN
	
	if (@testVal IS NULL) return 1;
    
	declare @isInRange as BIT = 0; 

	if ((@testVal > @lower) AND (@testVal < @upper)) SET @isInRange = 1;

	return @isInRange ;
END