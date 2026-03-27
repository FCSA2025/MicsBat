------------------------------------------------------------------------------
------------------------------------------------------------------------------
-- This script creates and populates the SQL Server table:
--
--       [fcsa].[hulme].[nfm_callsign_t5a]
--
-- Using its call sign, this table can be used to lookup a TS site's:
--
--        - location (latitude, longitude); 
--        - ISED Tier 5 Area ID (e.g. '5-234'); and 
--        - ISED base rate code (URBAN, RURAL, REMOTE);
--
-- Table Dependencies:
--
--        1. [hulme].[nfm_t5a_polygons]
--
-- User-Defined Functions:
--
--        None.
-----------------------------------------------------------------------------
------------------------------------------------------------------------------

-- Set environment.
USE [micsdev];

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

-- Drop any existing table with this name.
IF OBJECT_ID('hulme.nfm_callsign_t5a') IS NOT NULL DROP TABLE hulme.nfm_callsign_t5a

-- Create the target table.
CREATE TABLE hulme.nfm_callsign_t5a(
	[call1] CHAR(9) NOT NULL,
	[lat] REAL NOT NULL,
	[lng] REAL NOT NULL,
	[tier5AreaID] CHAR(7) NOT NULL,
	[baseRateCode] CHAR(6) NOT NULL
	PRIMARY KEY CLUSTERED 
	(
		[call1]
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY];

-- Populate the target table.
INSERT INTO hulme.nfm_callsign_t5a
	SELECT 
		call1, latit/360000.0, -longit/360000.0, tier5AreaID, baseRateCode 
	FROM micsprod.main.mt_site
	JOIN hulme.nfm_t5a_polygons ON 
		boundary.STContains('POINT(' + CONVERT (VARCHAR(50), latit/360000.0, 128) + ' ' + CONVERT (VARCHAR(50), -longit/360000.0, 128) + ')') = 1
	WHERE 
		-- The call1 Link-end is in Canada or St. Perre & Miquelon.
		prov IN ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP')
		-- The call1 Link-end is a FCSA member.
		AND oprtyp = 'FT';

-- Display the target table.
--SELECT * FROM hulme.nfm_callsign_t5a ORDER BY call1;

-- Verify the targt table's modify_date.
--SELECT name, [modify_date] FROM sys.tables WHERE name = 'nfm_callsign_t5a'
