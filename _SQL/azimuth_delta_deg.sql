USE [fcsa]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Returns the angle (degrees) subtended by two radii with prescribed azimuths
-- over the range [0, 360). Positive values indicate a clockwise rotation from
-- @azim_deg_1 to @azim_deg_2.
-- The returned value is normalized to lie in the range (-180, 180].
ALTER function [hulme].[azimuth_delta_deg](@azim_deg_1 as float, @azim_deg_2 as float)
returns float
begin
	declare @retVal as float = 0.0;
	declare @delta as float = 0.0;
	declare @sign as int = +1;

	-- Sanitize the inputs.
	if ((@azim_deg_1 is NULL) OR (@azim_deg_2 is NULL)) return NULL;

	if (@azim_deg_1 > @azim_deg_2) set  @sign = -1.0;

	set @delta = abs(@azim_deg_2 - @azim_deg_1);

	if (@delta >  180.0) set @delta = @delta - 360.0;
	if (@delta < -180.0) set @delta = @delta + 360.0;

	set @retVal = @sign * @delta;

	return @retVal;
end