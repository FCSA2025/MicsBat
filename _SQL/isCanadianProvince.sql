USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[isCanadianProvince]    Script Date: 5/19/2021 11:24:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns a BIT value indicating if the prescribed province string is 
-- Canadian, or not. 
ALTER FUNCTION [hulme].[isCanadianProvince](@prov VARCHAR(2))
RETURNS BIT
BEGIN

	DECLARE @ret BIT; 

	IF (@prov IN ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP')) 
		SET @ret = 1; 
	ELSE 
		SET @ret = 0;

	RETURN @ret;

END