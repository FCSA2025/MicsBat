USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[longitToFloat]    Script Date: 7/13/2021 5:19:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [hulme].[longitToFloat] 
(
	@longit INT    -- units: hundreths of a second of arc.
)
RETURNS [float]
AS
BEGIN

    -- We assume the longitude is west of the Greenwich meridian, i.e. is negative.
	RETURN - @longit / 360000.0 
			
END