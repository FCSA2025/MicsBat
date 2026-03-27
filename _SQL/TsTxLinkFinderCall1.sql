 IF OBJECT_ID('tempdb..#AllFilteredLinks') IS NOT NULL DROP TABLE #AllFilteredLinks
 
 -- Create a temporary working table.
 CREATE TABLE #AllFilteredLinks (
	[call1] CHAR(9) NOT NULL,		-- The call sign of the (local) site ate the Tx Link-end.
	[call2] CHAR(9) NOT NULL,		-- The call sign of the (remote) site at Rx Link-end.
	[oper] CHAR(6) NOT NULL,		-- The operator of the site of the Tx Link-end.
	[prov] CHAR(2) NOT NULL,		-- The province code of the site at the Tx (local) Link-end.
	[bndcde] CHAR(4) NOT NULL,		-- The band code of the antenna at the Tx (local) Link-end.
	[anum] SMALLINT NOT NULL,		-- The antenna number of the Tx (local) Link-end.
	[chid] CHAR(4) NOT NULL,		-- The channel ID code of the Tx (local) Link-end.
	[ause] CHAR(3),                 -- The operational use of the antenna: TX, RX, TR, STX, DV1 or DV2.   
	[freqMHztx] FLOAT NOT NULL,		-- The transmit centre-frequency of the Link (MHz).
	[lat] FLOAT NOT NULL,			-- The latitude of the Tx (local) Link-end.
	[lng] FLOAT NOT NULL,			-- The longitude of the Tx (local) Link-end.
	[azmth] FLOAT NOT NULL,		-- The azimuth of the antenna at the Tx (local) Link-end.
	[azmthDiff] FLOAT NOT NULL,		-- The azimuth of the antenna at the Tx (local) Link-end.
/*	PRIMARY KEY CLUSTERED 
	(
        [call1],
		[call2],
		[bndcde],
		[chid],
		[freqtx]
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]*/
) ON [PRIMARY];


-- Find all the Tx Links that satisfy the prescribed conditions (filters).
INSERT INTO #AllFilteredLinks (call1, call2, oper, prov, bndcde, anum, chid, ause, freqMHztx, lat, lng, azmth, azmthDiff)
SELECT DISTINCT    chanA.call1, chanA.call2, siteA.oper, siteA.prov, chanA.bndcde, anteA.anum, chanA.chid, anteA.ause,
        chanA.freqtx / 1000,
        siteA.latit/360000.0 AS lat, siteA.longit/360000.0 AS lng,
		anteA.azmth,
        ABS(anteA.azmth - anteB.azmth) - 180 AS azmthDiff
    FROM main.mt_chan chanA
    JOIN main.mt_chan chanB ON (chanA.call1 = chanB.call2) AND (chanA.call2 = chanB.call1) AND (chanA.bndcde = chanB.bndcde) AND (chanA.chid = chanB.chid)
    JOIN main.mt_ante anteA ON (anteA.call1 = chanA.call1) AND (anteA.call2 = chanA.call2) AND (anteA.bndcde = chanA.bndcde)
    JOIN main.mt_ante anteB ON (anteB.call1 = chanB.call1) AND (anteB.call2 = chanB.call2) AND (anteB.bndcde = chanB.bndcde)
    JOIN main.mt_site siteA ON (siteA.call1 = chanA.call1)
    JOIN main.mt_site siteB ON (siteB.call1 = chanB.call1)
    WHERE
        -- A is the transmit end and B is the receive end.
        ABS (chanA.freqtx - chanB.freqrx) < 1
        -- The antenna ause fields support a Transmitter-Receiver Link pair,
        -- and diversity antennas are excluded.
        --AND (anteA.ause IN ('TX', 'TR')) AND (anteB.ause IN ('RX', 'TR'))
        -- No ficticious call signs.
        AND LEFT(chanA.call1, 1) NOT IN ('=', '$', '%', ';')
        AND LEFT(chanA.call2, 1) NOT IN ('=', '$', '%', ';')
        -- The call1 Link-end is in Canada or St. Perre & Miquelon.
        AND (siteA.prov IN ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP'))
        -- The call1 Link-end is a FCSA member.
        --AND ((siteA.oper IN (SELECT oper FROM adm.account_ids)))
        --AND NOT (siteB.prov IN ('YT','NW','BC','AB','SK','MB','ON','QC','NB','PE','NS','NL','NU', 'SP'))
        --AND (siteA.oper = siteB.oper)
        --AND NOT (chanA.chid = chanB.chid)
        --AND NOT (chanA.bndcde = chanB.bndcde)
        --AND NOT (chanA.feetx = chanB.feerx)
        --AND NOT (chanA.traftx = chanB.trafrx)
        --AND anteA.apoint = 'I19A'
        --AND anteB.apoint = 'I19A'
        --AND (chanA.traftx != chanB.trafrx)
        --AND (anteA.apoint IS NOT NULL)
        --AND (chanA.feetx = 'X' OR  chanA.feerx = 'X' OR  chanB.feetx = 'X' OR  chanB.feerx = 'X')
        --AND (chanA.feetx IS NULL)
        --AND ABS(anteA.azmth - anteB.azmth) - 180 > 0.5
        --AND (ABS(siteA.latit/360000.0 - 49.19333333) < 0.01) AND (ABS(siteA.longit/360000.0 - 122.91833333) < 0.01)
        --AND hulme.bandwidth_from_emission(emission) < 0
        --AND hulme.bandwidth_from(emission, edesc) <= 0
        --AND hulme.bandwidth_from(emission, edesc) IS NULL
        --AND ((chanA.traftx != chanB.trafrx) AND (chanA.feetx = 'X'))
		AND siteA.call1 = 'XJV283'
    ORDER BY chanA.call1, chanA.call2, chanA.bndcde, chanA.chid;

	SELECT * FROM #AllFilteredLinks ORDER BY call1, call2, bndcde, chid;