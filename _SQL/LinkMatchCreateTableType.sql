USE micsdev
GO

CREATE TYPE hulme.LinkMatchTableType AS TABLE
(
    [x_Direction] [char](9) NULL,                   -- Must be either 'TaflToMdb' or 'MdbToTafl'.
	[m_TxRx] [char](2) NULL,                        -- MDB: 'TX', 'RX' or NULL. (Analogous to TAFL TXRX.)
	[m_Oper] [char](6) NULL,				        -- MDB:  mt_site.oper
    [m_Call1] [char](9) NULL,			            -- MDB:  mt_site.m_call1
	[m_Call2] [char](9) NULL,			            -- MDB:  mt.ante.m_call2
	[m_Bndcde] [char](4) NULL,		                -- MDB:  mt_ante.m_bndcde
	[m_Chid] [char](4) NULL,			            -- MDB:  mt_chan.m_chid
	[m_Ause] [char](3) NULL,                        -- MDB:  mt_ante.m_ause
	[m_Anum] [int] NULL,				            -- MDB:  mt_ante.m_anum (= mt_chan.antnumbtx1 for a Tx Link)
	[m_Lat] [float] NULL,				            -- MDB:  mt_site.latit  / 360000.0
    [m_Lng] [float] NULL,				            -- MDB:  mt_site.longit / 360000.0
	[m_Prov] [char](2) NULL,				        -- MDB:  mt_site.prov
	[m_Azmth] [float] NULL,				            -- MDB:  mt.ante.m_Azmth
	[m_FreqTx] [float] NULL,				        -- MDB:  mt_chan.freqtx
	[m_FreqRx] [float] NULL,				        -- MDB:  mt_chan.freqrx
	[m_Aht] [float] NULL,					        -- MDB:  mt_ante.aht
	[m_FreqTxRx] [float] NULL,                      -- MDB:  either freqtx or freqrx depending on the value of m_TxRx.
	[m_StatTxRx] [char](1) NULL,                    -- MDB:  either stattx or statrx depending on the value of m_TxRx.
	[m_OrdinalKey] [char](19) NULL,                 -- MDB:  string used as a key to perform call1-call2 pair ordering.
	[m_SiteName] [char](32) NULL,                   -- MDB:  mt_site.name
	[m_Region] [char](2) NULL,                      -- MDB:  mt_site.reg
	[t_KeyField] [int] NULL,    					-- TAFL: Venn keyfield column value.
	[t_TxRx] [char](2) NULL,    					-- TAFL: TXRX ('TX', 'RX' or NULL).
	[t_Lat] [float] NULL,    						-- TAFL: latitude.
	[t_Lng] [float] NULL,    						-- TAFL: longitude.
	[t_Azmth] [float] NULL,    						-- TAFL: azimuth.
	[t_Aht] [float] NULL,    						-- TAFL: antenna's height above ground level.
	[t_FreqTxRx] [float] NULL,    					-- TAFL: frequency.
	[t_AuthorizationNumber] [char](63) NULL,        -- TAFL: Authorization number.
	[t_AuthorizationStatus] [char](2) NULL,         -- TAFL: Authorization status.
	[t_Callsign] [char](255) NULL,                  -- TAFL: Callsign.
	[t_InserviceDate] [char](10) NULL,              -- TAFL: InService date.
	[t_AccountNumber] [char](255) NULL,             -- TAFL: Account number.
	[t_LicenseeName] [char](255) NULL,              -- TAFL: Licensee name.
	[t_ReferenceIdentifier] [char](63) NULL,        -- TAFL: Reference identifier.
	[t_LicenseeName_oper] [char](6) NULL,           -- TAFL: FCSA 'oper' associated with the Licensee Name.
	[d_Meters] [float] NULL,					    -- the distance between sites being matched.
	[d_Degrees] [float] NULL,                       -- the angular difference between azimuths being matched.
	[d_MHz] [float] NULL,						    -- the difference between frequencies being matched.
	[x_FOM] [int] NULL,                             -- the 'Figure of Merit' for the closeness of the match [0, 100].
	[x_Confidence] [char](9) NULL,                  -- an emperical description of the 'closeness of the match'.
	[x_LAF] [char](3) NULL,                         -- the location, azimuth and/or frequency match characteristics.
	[x_Comment] [char](255) NULL                    -- a comment (e.g. recommended remedial action)
);
GO