USE [fcsa]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns the distance in Kilometres given lat/long inputs in which the first pair
-- is in centi-seconds and the second pair in degrees.
-- The algorithm follows that used in the MICS# AxDistan() function, to give the same result. */
ALTER function [hulme].[distance_cs_deg](@lat1 as bigint, @lng1 as bigint, @lat2 as float, @lng2 as float)
returns float
begin
	declare @latrad1 as float = RADIANS(@lat1 / 360000.0);
	declare @latrad2 as float = RADIANS(@lat2);
	declare @latdiff as float;
	if (@latrad1 = @latrad2) 
		set @latdiff = 1.e-8; 
	else 
		set @latdiff = (@latrad1 - @latrad2);
	declare @longrad1 as float = RADIANS(@lng1 / 360000.0);
	declare @longrad2 as float = RADIANS(@lng2);
	declare @longdiff as float;
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
	return @distanceKm;
end