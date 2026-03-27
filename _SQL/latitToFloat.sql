USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[latitToFloat]    Script Date: 7/13/2021 5:19:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [hulme].[latitToFloat] 
(
	@latit INT    -- units: hundreths of a second of arc.
)
RETURNS [float]
AS
BEGIN

    -- We assume the latitude is in the northern hemisphere, i.e. is positive.
	RETURN @latit / 360000.0 
			
END