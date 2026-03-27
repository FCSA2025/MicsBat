USE [fcsa]
GO
/****** Object:  UserDefinedFunction [hulme].[distance_spherical_deg]    Script Date: 12/24/2021 1:58:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*  
	Returns the distance in Kilometres between two points on a spherical Earth, 
    given their latitude/longitude coordinates in degrees.

    The algorithm implements the Haversine formula and is exact for a spherical earth. 
*/
CREATE function [hulme].[distance_spherical_cs](@lat1 as bigint, @lng1 as bigint, @lat2 as bigint, @lng2 as bigint)
returns float
begin
	declare @latrad1 as float = RADIANS(@lat1 / 360000.0);
	declare @latrad2 as float = RADIANS(@lat2 / 360000.0);
	declare @longrad1 as float = RADIANS(@lng1 / 360000.0);
	declare @longrad2 as float = RADIANS(@lng2 / 360000.0);

	declare @latdiff as float;
	if (@latrad1 = @latrad2) 
		set @latdiff = 1.e-8; 
	else 
		set @latdiff = (@latrad1 - @latrad2);

	declare @longdiff as float;
	if (@longrad1 - @longrad2 = 0.0) 
		set @longdiff = 1.e-8;
	else
		set @longdiff = @longrad1 - @longrad2;

    declare @temp as float;
	set @temp = POWER(SIN(0.5 * (@longdiff)), 2);
	set @temp *= COS(@latrad1) * COS(@latrad2);
	set @temp += POWER(SIN(0.5 * (@latdiff)), 2);

	if (@temp < 0) set @temp = 1.e-8;

	set @temp = SQRT(@temp);

	declare @distanceKm as float = 2 * 6371 * ASIN(@temp)

	if (@distanceKm < 0.001) set @distanceKm = 0.001; 

	return @distanceKm;
end