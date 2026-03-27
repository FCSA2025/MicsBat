USE [fcsa]
GO

/****** Object:  UserDefinedFunction [tsip].[azimuth_hs]    Script Date: 4/3/2019 7:10:49 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*  Return the azimuth in degrees given lat/long inputs in hundredths of a second. The longitudes are */
/*	the negatives of what we use, so ours must be negated on entry. */
create function [tsip].[azimuth_hs](@lat1 as bigint, @lng1 as bigint, @lat2 as bigint, @lng2 as bigint)
returns float
begin
	declare @latrad1 as float = RADIANS(@lat1 / 360000.0);
	declare @latrad2 as float = RADIANS(@lat2 / 360000.0);
	declare @latdiff as float;
	if (@latrad1 = @latrad2) 
		set @latdiff = 1.e-8;
	else 
		set @latdiff = (@latrad1 - @latrad2);
		
	declare @longrad1 as float = RADIANS(@lng1 / 360000.0);
	declare @longrad2 as float = RADIANS(@lng2 / 360000.0);
	declare @longdiff as float;
	if (@longrad1 - @longrad2 = 0.0) 
		set @longdiff = 1.e-8;
	else
		set @longdiff = @longrad1 - @longrad2;
		
	declare @latAvr as float = (@latrad1 + @latrad2) / 2.0;
	declare @c1 as float = 1.0 - (0.00669454 * (SQUARE(SIN(@latAvr))));
	/* declare @am as float = SQRT(@c1) / 30.9221917;*/
	declare @bearingMid as float = ABS(ATAN((@longdiff * COS(@latAvr) * @c1) / 
	                                        ((1.0 - 0.00669454) * @latdiff)));
	declare @bearingDiff as float = @longDiff * SIN(@latAvr);
	declare @bearing12 as float = @bearingMid - (@bearingDiff / 2.0);
	if (@latDiff >= 0.0) begin
		if (@longDiff < 0.0) begin
			set @bearing12 = PI() + @bearing12;
		end;
		else begin
			set @bearing12 = PI() - @bearing12 - @bearingDiff;
		end;
	end;
	else if (@longDiff <= 0.0) begin
		set @bearing12 = 2.0 * PI() - @bearing12 - @bearingDiff;
	end;
	
	return Degrees(@bearing12);
end

GO


