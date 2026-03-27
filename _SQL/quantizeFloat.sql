USE [micsdev]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- For a prescribed floating-point value, this function returns its quantized (rounded)
-- value w.r.t. a prescribed quantum size.
-- e.g. 
--       quantizeFloat(44.123456, 0.0005) RETURNS 44.1235
--       quantizeFloat(44.123456, 0.04  ) RETURNS 44.12
--       quantizeFloat(44.123456, 0.25  ) RETURNS 44.0
--
ALTER function [hulme].[quantizeFloat](@value AS FLOAT, @quantum AS FLOAT)
RETURNS FLOAT
BEGIN
	DECLARE @tempF AS FLOAT = 0;
	DECLARE @tempI AS BIGINT = 0;
	DECLARE @quantized AS FLOAT = 0;

    IF (@quantum < 2.23E-308) 
		SET @quantized = @value;
    ELSE
		BEGIN
			SET @tempF = @value / @quantum;

			SET @tempI = ROUND(@tempF, 0);

			SET @quantized = @tempI * @quantum ;
		END

    RETURN @quantized

END