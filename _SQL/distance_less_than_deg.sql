USE [fcsa]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*  Returns 1 (true) if the distance in Kilometres between two prescribed (lat/long) point inputs (in degrees)
    is less than a prescribed distance (in km). */
/*	The algorithm follows that used in the MICS# AxDistan() function, to give the same result. */
ALTER FUNCTION [hulme].[distance_less_than_deg](@lat1 as float, @lng1 as float, @lat2 as float, @lng2 as float, @radius_kms as float)
returns BIT
begin
    declare @result as BIT = 0;

	-- Try for a quick return based on latitude difference.
	-- Earth's radius is 6,371 km.
	-- A 1 degree change in latitude (at constant longitude) spans 111.2 km
	-- of circumference.
	declare @quickDist as float = 2 * @radius_kms
	if (ABS(@lat1 - @lat2) * 111.2 > @quickDist) return 0;

	declare @latrad1 as float = RADIANS(@lat1);
	declare @latrad2 as float = RADIANS(@lat2);
	declare @latdiff as float;

	if (@latrad1 = @latrad2) 
		set @latdiff = 1.e-8; 
	else 
		set @latdiff = (@latrad1 - @latrad2);

	declare @longrad1 as float = RADIANS(@lng1);
	declare @longrad2 as float = RADIANS(@lng2);
	declare @longdiff as float;

	-- Try for a quick return based on based on longitude difference.
	-- Assume all latitudes are between 0 and 90 degrees.
	declare @s1 as float = 6371 * COS(@latrad1) * ABS(@longdiff);
	declare @s2 as float = 6371 * COS(@latrad2) * ABS(@longdiff);	
	if ((@s1 > @quickDist) and (@s2 > @quickDist)) return 0

	if (@longrad1 - @longrad2 = 0.0) 
		set @longdiff = 1.e-8;
	else
		set @longdiff = @longrad1 - @longrad2;

	declare @latAvr as float = (@latrad1 + @latrad2) / 2.0;

	declare @c1 as float = 1.0 - (0.00669454 * (SQUARE(SIN(@latAvr))));
	declare @am as float = SQRT(@c1) / 30.9221917;
	declare @bearingMid as float = ABS(ATAN((@longdiff * COS(@latAvr) * @c1) / 
	                                        ((1.0 - 0.00669454) * @latdiff)));
	declare @distanceKm as float = ABS((@longdiff * COS(@latAvr)) / 
	                                   (@am * SIN(@bearingMid) * 0.0048481368));
	if (@distanceKm < 0.001)
		set @distanceKm = 0.001; 

	if (@distanceKm < @radius_kms) set @result = 1;

	return @result;
end