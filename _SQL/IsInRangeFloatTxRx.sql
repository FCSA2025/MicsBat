USE [micsdev]
GO
/****** Object:  UserDefinedFunction [hulme].[isInRangeFloatTxRx]    Script Date: 6/9/2021 11:08:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Returns BIT value 1 if a prescribed freqtx and freqrx values both lie in the closed interval [lower, upper]; otherwise 0.
ALTER function [hulme].[isInRangeFloatTxRx](@lower as float, @upper as float, @freqtx as float, @freqrx as float)
RETURNS BIT
BEGIN
	
	declare @freqtxIsInRange as BIT = 0; 	
	if 
			(@freqtx IS NULL) set @freqtxIsInRange = 1
	else if 
			((@freqtx >= @lower) AND (@freqtx <= @upper)) SET @freqtxIsInRange = 1;

	declare @freqrxIsInRange as BIT = 0; 	
	if 
			(@freqrx IS NULL) set @freqrxIsInRange = 1
	else if 
			((@freqrx >= @lower) AND (@freqrx <= @upper)) SET @freqrxIsInRange = 1;

    declare @isInRange as BIT = 0;
	if (@freqtxIsInRange = 1 AND @freqrxIsInRange = 1) set @isInRange = 1;

	return @isInRange ;
END