USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[TAFLmatchFOM]    Script Date: 1/25/2023 8:33:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*  
	Returns a heuristic Figure of Merit (FOM), between 0 and 100 for the goodness of the match
	with a TAFL record. 100 = very good match.
*/
ALTER FUNCTION [hulme].[TAFLmatchFOM](
							@distLimit AS FLOAT, 
							@azmthLimit AS FLOAT,
							@mhzLimit AS FLOAT,
							@distActual AS FLOAT, 
							@azmthActual AS FLOAT,
							@mhzActual AS FLOAT
							)
RETURNS INT
BEGIN
	DECLARE @TAFLmatchFOM AS INT = 100;
	DECLARE @min AS FLOAT = 1.0;

	DECLARE @distOK  AS FLOAT = 0.33 * @distLimit;
	DECLARE @azmthOK AS FLOAT = 0.33 * @azmthLimit;
	DECLARE @mhzOK   AS FLOAT = 0.33 * @mhzLimit;

    DECLARE @distFOM  AS FLOAT = 1.0;
	DECLARE @azmthFOM AS FLOAT = 1.0;
	DECLARE @mhzFOM   AS FLOAT = 1.0;

	IF (@distActual > @distOK) SET @distFOM = 1.0 - (@distActual - @distOK) / (@distLimit - @distOK);
	--IF (@distFOM < 0.5) SET @distFOM = 0.5;

	IF (@azmthActual > @azmthOK) SET @azmthFOM = 1.0 - (@azmthActual - @azmthOK) / (@azmthLimit - @azmthOK);
	--IF (@azmthFOM < 0.5) SET @azmthFOM = 0.5;

	IF (@mhzActual > @mhzOK) SET @mhzFOM = 1.0 - (@mhzActual - @mhzOK) / (@mhzLimit - @mhzOK);
	--IF (@mhzFOM < 0.5) SET @mhzFOM = 0.5;

	IF (@distFOM  < @min) SET @min = @distFOM;
	IF (@azmthFOM < @min) SET @min = @azmthFOM;
	IF (@mhzFOM   < @min) SET @min = @mhzFOM;

	SET @TAFLmatchFOM = 100.0 * @min;

	RETURN @TAFLmatchFOM
END 