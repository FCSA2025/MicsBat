--------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------
-- This script populates the following SQL Server tables that relate to ISED's New Fee Model (NFM)
-- for the cost of the spectral component of a TS transmission (Tx) Link.
--
--       1.  hulme.nfm_ts_all_tx_links				// The detailed intermediate 'workbook' table.
--       2.  hulme.nfm_ts_d_per_oper_all_tx_links   // Detailed NFM spectral costs per Tx Link.
--       3.  hulme.nfm_ts_d_per_oper_summary        // Summary of NFM costs per member.
--
-- Table Dependencies:
--
--        1.  main.mt_site
--        2.  main.mt_ante
--        3.  main.mt_chan
--        4.  hulme.nfm_callsign_t5a
--        5.  main.sd_eqpt
--        6.  hulme.ised_base_rates
--
-- User-Defined Functions:
--
--        1.  hulme.bandwidth_from(emission, edesc)
--        2.  hulme.tier5AreaCodeLogic(tx_br_code, rx_br_code)
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------

-- Set environment.
USE [micsdev];

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

-- Drop any existing intermediate 'workbook' table.
 IF OBJECT_ID('hulme.nfm_ts_all_tx_links') IS NOT NULL DROP TABLE hulme.nfm_ts_all_tx_links;

 -- Create a new intermediate 'workbook' table.
 CREATE TABLE hulme.nfm_ts_all_tx_links (
    [instances] INT,				-- Counts the number of identical Tx Link triplets {call1, call2, freqMHztx}.
 	[oper] CHAR(6) NOT NULL,		-- The operator of the site of the Tx Link-end.
	[call1] CHAR(9) NOT NULL,		-- The call sign of the (local) site ate the Tx Link-end.
	[call2] CHAR(9) NOT NULL,		-- The call sign of the (remote) site at Rx Link-end.
	[prov] CHAR(2) NOT NULL,		-- The province code of the site at the Tx (local) Link-end.
	[bndcde] CHAR(4) NOT NULL,		-- The band code of the antenna at the Tx (local) Link-end.
	[anum] SMALLINT NOT NULL,		-- The antenna number of the Tx (local) Link-end.
	[chid] CHAR(4) NOT NULL,		-- The channel ID code of the Tx (local) Link-end.
	[freqMHztx] FLOAT NOT NULL,		-- The transmit centre-frequency of the Link (MHz).
	[lat] FLOAT NOT NULL,			-- The latitude of the Tx (local) Link-end.
	[lng] FLOAT NOT NULL,			-- The longitude of the Tx (local) Link-end.
	[azimuth] FLOAT NOT NULL,		-- The azimuth of the antenna at the Tx (local) Link-end.
	[traftx] CHAR(6),				-- Traffic code of the channel at the Tx (local) Link-end.
	[trafrx] CHAR(6),				-- Traffic code of the channel at the Rx (remote) Link-end.
	[eqpttx] CHAR(8),				-- Equipment code of the channel at the Tx (local) Link-end.
	[edesc] CHAR(32),				-- Emission description of the channel at the Tx (local) Link-end.
	[emission] CHAR(10),			-- ITU emission designator of the transmission.
	[tx_t5a_ID] CHAR(7),			-- ISED Tier 5 Area ID of for the local site (Tx Link-end).
	[rx_t5a_ID] CHAR(7),			-- ISED Tier 5 Area ID of for the remote site (Rx Link-end).
	[tx_br_code] CHAR(6),			-- ISED base rate code (URBAN, RURAL, REMOTE) for the local site (Tx Link-end).
	[rx_br_code] CHAR(6),			-- ISED base rate code (URBAN, RURAL, REMOTE) for the remote site (Rx Link-end).
	[baseRateCode] CHAR(6),			-- Applicable ISED base rate code (a function of 'tx_br_code' and 'tx_br_code').
	[d_per_MHz] FLOAT,				-- ISED prescribed NFM dollars per MHz of occupied bandwidth.
	[bw_MHz] FLOAT,					-- Tx Link's occupied bandwidth (MHz).
	[bw_source] CHAR(32),			-- 'PARSED' means that the bandwidth is derived from 'emission' or 'edesc'.
	[d_per_year] FLOAT				-- Cost of the Tx Link in dollars per year.

) ON [PRIMARY];

-- Populate the intermediate table with FCSA member's Tx Link-end data drawn from the TS MDB tables
-- mt_site, mt_ante and mt_chan.
--
-- Each Tx Link-End has a unique {site, ante, chan} triplet where:
--      mt_site.call1 = mt_ante.call1 = mt_chan.call1; and
--      mt_ante.call2 = mt_chan.call2
--      mt_ante.bndcde = mt_chan.bndcde; and
--      mt_ante.anum = mt_chan.antnumbtx1
INSERT INTO hulme.nfm_ts_all_tx_links (instances, oper, call1, call2, prov, bndcde, anum, chid, freqMHztx, lat, lng, azimuth, traftx, trafrx, eqpttx)
	SELECT	
		ROW_NUMBER() OVER (PARTITION BY site.call1, ante.call2, chan.freqtx ORDER BY site.call1, ante.call2, chan.freqtx),
		site.oper, chan.call1, chan.call2, site.prov, chan.bndcde, ante.anum, chan.chid,
		chan.freqtx / 1000,
		site.latit/360000.0 AS lat, -site.longit/360000.0 AS lng,
		ante.azmth, 
		chan.traftx, chan.trafrx,
		chan.eqpttx
	FROM micsprod.main.mt_chan AS chan
	JOIN micsprod.main.mt_ante AS ante ON (ante.call1 = chan.call1) AND (ante.call2 = chan.call2) AND (ante.bndcde = chan.bndcde) AND (ante.anum = chan.antnumbtx1)
	JOIN micsprod.main.mt_site AS site ON (site.call1 = chan.call1)
	WHERE
		site.oprtyp='FT'                -- Only include Links belonging to FCSA members.
		AND ante.ause IN ('TR', 'TX')   -- Link-end must be the Tx part of Transmit-Receive ot Transmit-Only antenna.
		AND chan.freqtx IS NOT NULL
	ORDER BY 
		chan.call1, chan.call2, chan.bndcde, chan.chid;

-- The spectral component of ISED's new fee model depends on the Tx Link-end's
-- Tier 5 Area 'base rate code' (URBAN, RURAL, REMOTE) and the Link's Tx signal 
-- centre frequency and occupied bandwidth.
--
-- Currently, a Link's occupied bandwidth is not stored anywhere in the TS MDB tables.
--
-- To overcome this deficiency, we will attempt to infer a Link's occupied bandwidth via
-- its 'transmit equipment' code (eqpttx) that is a field in its mt_chan record.
-- We use 'eqpttx' to lookup the following quantities from the SDB table main.sd_eqpt:
--     edesc   : an FCSA-specific emission description string.
--     emission: an industry-standard emission description string.
UPDATE hulme.nfm_ts_all_tx_links
	SET
		edesc = E.edesc,
		emission = E.emission
    FROM hulme.nfm_ts_all_tx_links AS L
	JOIN micsprod.main.sd_eqpt AS E ON (E.ecode = L.eqpttx);
	
-- Attempt to populate the occupied bandwidth colum (bw_MHz) by parsing from 
-- the string fields 'emission' or 'edesc'. This functionality is provided
-- by the User-Defined Function (UDF):
--       hulme.bandwidth_from(emission, edesc)
UPDATE hulme.nfm_ts_all_tx_links
	SET 
		bw_MHz = hulme.bandwidth_from(emission, edesc);

-- If the bandwidth was successfully parsed from the 'emission' or 'edesc' fields
-- (in that order) we will set the 'bw_source' field to 'PARSED' so that later
-- we know how the bandwidth value was derived.
UPDATE hulme.nfm_ts_all_tx_links
	SET
		bw_source = case
						when bw_MHz > 0 then 'PARSED' else null
					end;

-- If the previous call to the UDF:
--     hulme.bandwidth_from(emission, edesc)
-- failed it would have set the field 'bw_MHz' to a negative value.
--  
-- For these (hopefully few) rogue cases we will set the bandwidth equal
-- to a default value of 30 MHz. 
--
-- Also, we will set the 'bw_source' field
-- to 'DEFAULT' so that we can remember how the bandwidth was derived.
UPDATE hulme.nfm_ts_all_tx_links
	SET 
		bw_MHz = 30.0, 
		bw_source = 'DEFAULT'
	WHERE bw_MHz < 0;

-- Populate the 'tier5AreaID' and 'base_rate' columns for the Tx Link's local end
-- via the look-up table [hulme].[nfm_callsign_t5a]
-- keyed on the Tx Link's local call sign (call1).
UPDATE hulme.nfm_ts_all_tx_links
	SET 
		tx_t5a_ID = tier5AreaID,
		tx_br_code = nfm_callsign_t5a.baseRateCode
	FROM nfm_callsign_t5a
	WHERE nfm_callsign_t5a.call1 = hulme.nfm_ts_all_tx_links.call1;

-- Populate the 'tier5AreaID' and 'base_rate' columns for the Tx Link's remote end
-- via the look-up table [hulme].[nfm_callsign_t5a]
-- keyed on the Tx Link's remote call sign (call2).
UPDATE hulme.nfm_ts_all_tx_links
	SET 
		rx_t5a_ID = tier5AreaID,
		rx_br_code = nfm_callsign_t5a.baseRateCode
	FROM nfm_callsign_t5a
	WHERE nfm_callsign_t5a.call1 = hulme.nfm_ts_all_tx_links.call2;

-- ISED's applicable base rate code is a function of the base rate codes
-- at both ends of the Link. This functionality is provided by the UDF:
--      hulme.tier5AreaCodeLogic(tx_br_code, rx_br_code)
UPDATE hulme.nfm_ts_all_tx_links
		SET 
			baseRateCode = hulme.tier5AreaCodeLogic(tx_br_code, rx_br_code);

-- Populate the $ per MHz column using the lookup table hulme.ised_base_rates
-- keyed on the Tx Link's transmit centre frequency. 
UPDATE hulme.nfm_ts_all_tx_links
	SET d_per_MHz =	case
						when baseRateCode = 'URBAN' then R.urban_$perMHz
						when baseRateCode = 'RURAL' then R.rural_$perMHz
						when baseRateCode = 'REMOTE' then R.remote_$perMHz
					end
	FROM hulme.nfm_ts_all_tx_links
	JOIN hulme.ised_base_rates AS R ON (freqMHzLo < freqMHztx) AND (freqMHztx <= freqMHzHi);

-- Populate the $ per annum column (d_per_year).
UPDATE hulme.nfm_ts_all_tx_links
	SET 
		d_per_year = bw_MHz * d_per_MHz;

-- Create a new table 'hulme.nfm_ts_d_per_oper_all_tx_links' that will be used
-- to store a reduced set of 'cost per Tx Link' data. This table can be used to 
-- provide a detailed accounting of the spectral component cost to an individual
-- FCSA member down to the level of individual Tx Links operated by that member. 
-- This table will later be used to provide a roll-up of the NFM spectral component
-- cost per operator.
IF OBJECT_ID('hulme.nfm_ts_d_per_oper_all_tx_links') IS NOT NULL DROP TABLE hulme.nfm_ts_d_per_oper_all_tx_links;
CREATE TABLE hulme.nfm_ts_d_per_oper_all_tx_links (
 	[oper] CHAR(6) NOT NULL,        -- Counts the number of identical Tx Link triplets {call1, call2, freqMHztx}.
	[call1] CHAR(9) NOT NULL,       -- The call sign of the (local) site ate the Tx Link-end.
	[call2] CHAR(9) NOT NULL,       -- The call sign of the (remote) site at Rx Link-end.
	[bndcde] CHAR(4) NOT NULL,      -- The band code of the antenna at the Tx (local) Link-end.
	[anum] SMALLINT NOT NULL,       -- The antenna number of the Tx (local) Link-end.
	[chid] CHAR(4) NOT NULL,        -- The channel ID code of the Tx (local) Link-end.
	[lat] FLOAT NOT NULL,           -- The latitude of the Tx (local) Link-end.
	[lng] FLOAT NOT NULL,           -- The longitude of the Tx (local) Link-end.
	[azimuth] FLOAT NOT NULL,       -- The azimuth of the antenna at the Tx (local) Link-end.
	[freqMHztx] FLOAT NOT NULL,     -- The transmit centre-frequency of the Link (MHz).
	[bw_MHz] FLOAT,                 -- Tx Link's occupied bandwidth (MHz).
	[baseRateCode] CHAR(6),         -- Applicable ISED Link Base Rate Code (a function of 'tx_br_code' and 'tx_br_code').
	[d_per_MHz] FLOAT,              -- ISED prescribed NFM dollars per MHz of Link's occupied bandwidth.
	[d_per_year] FLOAT,             -- Cost of the Tx Link in dollars per year.
);

-- Populate the table 'hulme.nfm_ts_d_per_oper_all_tx_links' from the previous table
-- hulme.nfm_ts_all_tx_links
INSERT INTO hulme.nfm_ts_d_per_oper_all_tx_links (oper, call1, call2, bndcde, anum, chid, lat , lng, azimuth , freqMHztx, bw_MHz, baseRateCode, d_per_MHz, d_per_year)
	SELECT 
		oper, call1, call2, bndcde, anum, chid, lat , lng, azimuth , freqMHztx, bw_MHz, baseRateCode, d_per_MHz, ROUND(d_per_year, 2)	
	FROM hulme.nfm_ts_all_tx_links AS L;

-- We will now create a summary table of total spectral component cost per FCSA member.
IF OBJECT_ID('hulme.nfm_ts_d_per_oper_summary')      IS NOT NULL DROP TABLE hulme.nfm_ts_d_per_oper_summary
CREATE TABLE hulme.nfm_ts_d_per_oper_summary (
 	[oper] CHAR(6) NOT NULL,        -- The operator of the site of the Tx Link-end.
	[urbanLinks] INT NOT NULL,		-- The number of the operator's Tx Links whose applicable base rate code is 'URBAN'.
	[ruralLinks] INT NOT NULL,		-- The number of the operator's Tx Links whose applicable base rate code is 'RURAL'.
	[remoteLinks] INT NOT NULL,		-- The number of the operator's Tx Links whose applicable base rate code is 'REMOTE'.
	[numLinks] INT NOT NULL,		-- The total number of Tx Links operated (the sum of the previous three values).
	[bw_MHz] FLOAT NOT NULL,        -- Tx Link's occupied bandwidth (MHz).
	[d_per_year] FLOAT              -- Cost of the Tx Link in dollars per year.
	PRIMARY KEY CLUSTERED 
	(
		oper
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
); --ON [PRIMARY]

-- Populate the summary table of total spectral component cost per FCSA member by summing
-- over the Tx Links that they individually operate.
INSERT INTO hulme.nfm_ts_d_per_oper_summary (oper, urbanLinks, ruralLinks, remoteLinks, numLinks, bw_MHz, d_per_year)
	SELECT 
		oper,
		SUM(case when baseRateCode='URBAN' then 1 else 0 end),
		SUM(case when baseRateCode='RURAL' then 1 else 0 end),
		SUM(case when baseRateCode='REMOTE' then 1 else 0 end),
		COUNT(*),
		SUM(bw_MHz),
		ROUND(SUM(d_per_year), 0)
	FROM hulme.nfm_ts_d_per_oper_all_tx_links
	GROUP BY oper
	ORDER BY oper;

-- Browse the results.
SELECT * FROM hulme.nfm_ts_d_per_oper_summary ORDER BY oper