-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------
-- This scalar User-Defined Function (UDF) attempts to extract and return a
-- numerical value for the bandwidth (in MHz) by parsing a prescribed industry-standard
-- Emission Designator string as used in the 'emission' column of the SDB table sd_eqpt;
-- 
-- if this fails, this UDF attempts to parse the bandwidth from the prescribed 'edesc' string,
-- as used in the SDB table sd_eqpt. 
--
-- If both of these parsing attempts fail then the UDF returns -1.
--
-- Dependencies:
--					1.  UDF: hulme.bandwidth_from_emission(@emission)
--					2.  UDF: hulme.bandwidth_from_edesc(@edesc) 
-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------

USE [fcsa];
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

-- Delete the UDF if it already exists.
IF object_id('hulme.bandwidth_from', 'FN') IS NOT NULL
    DROP FUNCTION hulme.bandwidth_from
GO

CREATE FUNCTION hulme.bandwidth_from(@emission as char(10), @edesc as char(32))
returns FLOAT
AS
begin

    declare @retVal as real;

	-- First attempt to get the bandwidth (MHz) from the prescribed 'emmision' string.
	set @retVal = hulme.bandwidth_from_emission(@emission);
	if (@retVal > 0) return @retVal;

	-- Next, attempt to get the bandwidth (MHz) from the prescribed 'edesc' string.
	set @retVal = hulme.bandwidth_from_edesc(@edesc);
	if (@retVal > 0) return @retVal;

	-- Both attempts failed.
	return -1;
	
end