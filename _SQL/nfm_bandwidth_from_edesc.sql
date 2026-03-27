-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------
-- This scalar User-Defined Function (UDF) attempts to extract and return a
-- numerical value for the bandwidth (in MHz) by parsing a prescribed string
-- used in the 'edesc' column of the SDB table sd_eqpt.
-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------

USE [fcsa]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Delete the UDF if it already exists.
IF object_id('hulme.bandwidth_from_edesc', 'FN') IS NOT NULL
    DROP FUNCTION hulme.bandwidth_from_edesc
GO

CREATE FUNCTION hulme.bandwidth_from_edesc(@edesc_in as char(32))
returns FLOAT
AS
begin
	-- If @edesc input is NULL then return a negative value (error code).
    if (@edesc_in IS NULL) return -1;

	-- Tidy up the input @edesc string to ensure it has a canonical format.
    declare @edesc as char(32) = RTRIM(LTRIM(UPPER(@edesc_in)));

	-- Insert a space character at the start of the string to simplify pattern matching.
	set @edesc = ' ' + @edesc;

	declare @pos as int;

	---------------------------------------------------------------------------------------------------
	---------------------------------- MHz ------------------------------------------------------------
	---------------------------------------------------------------------------------------------------

	declare @bandwidthMHz as real = -666.0;

	-- edesc string contains a substring like: ' 4MHZ'
	set @pos = PATINDEX('% [1-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 1);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 40MHZ'
	set @pos = PATINDEX('% [1-9][0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 2);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 405MHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthMHz;
	
	-- edesc string contains a substring like: ' 4057MHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9][0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 7.5MHZ'
	set @pos = PATINDEX('% [0-9].[0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 7.53MHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthMHz;
	
	-- edesc string contains a substring like: ' 7.537MHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9][0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 27.5MHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 27.54MHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9][0-9]MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthMHz;

	--------------------------------------------------------------------------------
	
	-- edesc string contains a substring like: ' 4 MHZ'
	set @pos = PATINDEX('% [1-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 1);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 40 MHZ'
	set @pos = PATINDEX('% [1-9][0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 2);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 405 MHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 4057 MHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9][0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthMHz;
	-- edesc string contains a substring like: ' 7.5 MHZ'
	set @pos = PATINDEX('% [0-9].[0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 7.53 MHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthMHz;
	
	-- edesc string contains a substring like: ' 7.537 MHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9][0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 27.5 MHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthMHz;

	-- edesc string contains a substring like: ' 27.54 MHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9][0-9] MHZ%', @edesc);
	if (@pos > 0) set @bandwidthMHz = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthMHz;

	---------------------------------------------------------------------------------------------------
	---------------------------------- KHz ------------------------------------------------------------
	---------------------------------------------------------------------------------------------------

	declare @bandwidthKHZ as real = -66.6;

-- edesc string contains a substring like: ' 4KHZ'
	set @pos = PATINDEX('% [1-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 1);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 40KHZ'
	set @pos = PATINDEX('% [1-9][0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 2);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 405KHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;
	
	-- edesc string contains a substring like: ' 4057KHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9][0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 7.5KHZ'
	set @pos = PATINDEX('% [0-9].[0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 7.53KHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;
	
	-- edesc string contains a substring like: ' 7.537KHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9][0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 27.5KHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 27.54KHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9][0-9]KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	--------------------------------------------------------------------------------
	
	-- edesc string contains a substring like: ' 4 KHZ'
	set @pos = PATINDEX('% [1-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 1);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 40 KHZ'
	set @pos = PATINDEX('% [1-9][0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 2);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 405 KHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 4057 KHZ'
	set @pos = PATINDEX('% [1-9][0-9][0-9][0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;
	-- edesc string contains a substring like: ' 7.5 KHZ'
	set @pos = PATINDEX('% [0-9].[0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 3);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 7.53 KHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;
	
	-- edesc string contains a substring like: ' 7.537 KHZ'
	set @pos = PATINDEX('% [0-9].[0-9][0-9][0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 27.5 KHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 4);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-- edesc string contains a substring like: ' 27.54 KHZ'
	set @pos = PATINDEX('% [1-9][0-9].[0-9][0-9] KHZ%', @edesc);
	if (@pos > 0) set @bandwidthKHZ = substring(@edesc, @pos + 1, 5);
	if (@pos > 0) return @bandwidthKHZ / 1000.0;

	-----------------------------------------------------------------------------------
	-------------------- No match found -----------------------------------------------
	-----------------------------------------------------------------------------------
	if (@pos > 0) return @bandwidthMHz;

	return @bandwidthMHz;

end