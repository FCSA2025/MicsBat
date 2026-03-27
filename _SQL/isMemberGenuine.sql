USE [micsdev]
GO
/****** Object:  UserDefinedFunction [hulme].[isMemberGenuine]    Script Date: 5/19/2021 11:24:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns a BIT value indicating if the prescribed micsID string belongs to an FCSA member.
ALTER FUNCTION [hulme].[isMemberGenuine](@micsID VARCHAR(10))
RETURNS BIT
BEGIN

	DECLARE @ret BIT; 

	SET @ret = CASE
			       WHEN @micsID IN ('ABCCOM', 'ALIANT', 'BCHY', 'BELL', 'BMCE', 'BRAGG', 'DND', 'GLW', 
				                    'HYONE', 'HYQU', 'MTS', 'NAVI', 'NTTEL', 'NWT', 'ONT', 'RCTL', 
									'SHAW', 'STEL', 'TBAY', 'TEKSAV', 'TELS', 'TERAGO', 'TLUSAB', 'TLUSBC', 
									'TLUSMC', 'TLUSQC', 'VDTR', 'WIREIE', 'XCI', 'ZAYO') THEN 1
		           ELSE  0
	           END

	RETURN @ret;

END