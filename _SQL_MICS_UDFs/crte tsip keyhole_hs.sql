USE [fcsa]
GO

/****** Object:  UserDefinedFunction [tsip].[keyhole_hs]    Script Date: 4/3/2019 7:11:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*  Return the position relative to the keyhole for two sites and an antenna azimuth */
create function [tsip].[keyhole_hs](@lat1 as bigint, @lng1 as bigint, @lat2 as bigint, @lng2 as bigint, 
                                    @antenna_az as float, @coorddist as float)
returns int
begin
	declare @ret as int = 0;
	declare @dist as float = [tsip].[distance_hs](@lat1, @lng1, @lat2, @lng2);
	if @dist > 2 * @coorddist begin
		set @ret = 4;	/* The two sites are too far appart for consideration */
	end;
	else if @dist <= @coorddist begin
		set @ret = 1;	/* The two sites are close enough that they _must_ be considered */
	end;
	else begin
		/*	Check the keyhole.  If the azimuth is within 6 degrees (5 degrees plus one for using spherical earth)
		*	then we have a keyhole return */
		declare @az as float = [tsip].[azimuth_hs](@lat1, @lng1, @lat2, @lng2);
		declare @asdiff as float = ABS(@antenna_az - @az);
		
		if @asdiff <= 6.0 or @asdiff >= 354 begin
			set @ret = 2; /* In the keyhole */
		end;
		else begin
			set @ret = 3; /* Not in the keyhole */
		end;
	end;
	
	return @ret;
end

GO


