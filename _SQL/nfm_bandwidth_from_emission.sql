-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------
-- This scalar User-Defined Function (UDF) attempts to extract and return a
-- numerical value for the bandwidth (in MHz) by parsing a prescribed ITU
-- Emission Designator string as used in the 'emission' column of the SDB table sd_eqpt.
--
-- See:  https://en.wikipedia.org/wiki/Types_of_radio_emissions
-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------

USE [fcsa];
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

-- Delete the UDF if it already exists.
IF object_id('hulme.bandwidth_from_emission', 'FN') IS NOT NULL
    DROP FUNCTION hulme.bandwidth_from_emission
GO

CREATE FUNCTION hulme.bandwidth_from_emission(@emission as char(10))
returns FLOAT
AS
begin
	-- If @emission input is NULL then return a negative value (error code).
    if (@emission IS NULL) return -1;

	-- Tidy up the input @emission string to ensure it has a canonical format.
    declare @emsn as char(10) = RTRIM(LTRIM(UPPER(@emission)));

	-- Emission Designators must have at least 7 characters.
	-- See:  https://en.wikipedia.org/wiki/Types_of_radio_emissions
	if (LEN(@emsn) < 7) return -2;

	-- Parse the first 4 characters.
	declare @c1 as char(1) = SUBSTRING(@emsn, 1, 1); 
	declare @c2 as char(1) = SUBSTRING(@emsn, 2, 1); 
	declare @c3 as char(1) = SUBSTRING(@emsn, 3, 1); 
	declare @c4 as char(1) = SUBSTRING(@emsn, 4, 1); 

	-- Some Emission Designators erroneously have a 4th character that is 'O' rather than '0'.
	-- Test for this and fix it.
	if (@c4 = 'O') set @c4 = '0';

	-- Determine whether each of the first 4 characters is a numeric digit (0-9), or not.
	declare @c1_is_number as int = case when (ASCII(@c1) >= 48) AND (ASCII(@c1) <= 57) then 1 else 0 end;
	declare @c2_is_number as int = case when (ASCII(@c2) >= 48) AND (ASCII(@c2) <= 57) then 1 else 0 end;
	declare @c3_is_number as int = case when (ASCII(@c3) >= 48) AND (ASCII(@c3) <= 57) then 1 else 0 end;
	declare @c4_is_number as int = case when (ASCII(@c4) >= 48) AND (ASCII(@c4) <= 57) then 1 else 0 end;

	-- Determine whether each of the first 4 characters is alphabetic (A-Z), or not.
    declare @c1_is_letter as int = case when (ASCII(@c1) >= 65) AND (ASCII(@c1) <= 90) then 1 else 0 end;
	declare @c2_is_letter as int = case when (ASCII(@c2) >= 65) AND (ASCII(@c2) <= 90) then 1 else 0 end;
	declare @c3_is_letter as int = case when (ASCII(@c3) >= 65) AND (ASCII(@c3) <= 90) then 1 else 0 end;
	declare @c4_is_letter as int = case when (ASCII(@c4) >= 65) AND (ASCII(@c4) <= 90) then 1 else 0 end;

	-- Determine the pattern that the first 4 characters conform to.
	declare @pattern_is_000A as int = case when (@c1_is_number = 1 AND @c2_is_number = 1 AND @c3_is_number = 1 AND @c4_is_letter = 1) then 1 else 0 end;
	declare @pattern_is_00A0 as int = case when (@c1_is_number = 1 AND @c2_is_number = 1 AND @c3_is_letter = 1 AND @c4_is_number = 1) then 1 else 0 end;
	declare @pattern_is_0A00 as int = case when (@c1_is_number = 1 AND @c2_is_letter = 1 AND @c3_is_number = 1 AND @c4_is_number = 1) then 1 else 0 end;

	-- If the leading 4 character pattern is invalid the return an error code.
	if ((@pattern_is_000A = 0) AND (@pattern_is_00A0 = 0) AND (@pattern_is_0A00 = 0)) return -3;

	-- Now we know the pattern type we can extract the 'unit' letter.
	declare @unit as char(1) = 
		case
			when @pattern_is_000A = 1 then @c4
			when @pattern_is_00A0 = 1 then @c3
			when @pattern_is_0A00 = 1 then @c2
		end;

	-- Test that the 'unit' letter is valid, i.e. one of (H, K, M or G).
	if (@unit NOT in ('H', 'K', 'M', 'G')) return -4;

	-- Set the numeical 'multiplier' value i.a.w. the 'unit' letter.
	declare @multiplier as int =
		case
			when @unit = 'h' then '1'
			when @unit = 'K' then 1000
			when @unit = 'M' then 1000000
			when @unit = 'G' then 1000000000
		end;

	-- Get the numerical values of each of the first four characters.
	-- One of these will be a letter, not a number, but this is guarded
	-- against later.
	declare @c1_number as int = ASCII(@c1) - 48;
	declare @c2_number as int = ASCII(@c2) - 48;
	declare @c3_number as int = ASCII(@c3) - 48;
	declare @c4_number as int = ASCII(@c4) - 48;

	-- We can now determine the numerical value infered by the first 4
	-- characters of the input Emission Designator.
	declare @base_number as real =
		case
			when @pattern_is_000A = 1 then (@c1_number*100 + @c2_number*10 + @c3_number*1)
			when @pattern_is_00A0 = 1 then (@c1_number* 10 + @c2_number* 1 +                   @c4_number* 0.1)
			when @pattern_is_0A00 = 1 then (@c1_number*  1 +                 @c3_number* 0.1 + @c4_number*0.01)
		end;

    -- Construct the numerical value of the bandwidth and convert to MHz.
	return @base_number * @multiplier /1E6;
end