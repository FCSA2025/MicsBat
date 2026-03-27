DECLARE @lat1 AS FLOAT = 45;
DECLARE @lng1 AS FLOAT = 45;

DECLARE @lat2 AS FLOAT = 45.1;
DECLARE @lng2 AS FLOAT = 45.1;

SELECT 
	[hulme].[distance_deg](@lat1, @lng1, @lat2, @lng2) AS IAUellipsoid1968Dist,
	[hulme].[distance_spherical_deg](@lat1, @lng1, @lat2, @lng2) AS sphericalDist