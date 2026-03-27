USE [micsdev]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

-- Delete the main.sd_licn table if it already exists:
IF OBJECT_ID('main.sd_licn') IS NOT NULL DROP TABLE main.sd_licn

CREATE TABLE [main].[sd_licn](

    -- The essential keys to uniquely define an mt_chan record.
	[call1] [varchar](9) NOT NULL,                     -- MDB: mt_site.call1
	[call2] [varchar](9) NOT NULL,                     -- MDB: mt_ante.call2
	[bndcde] [varchar](4) NOT NULL,                    -- MDB: mt_ante.bndcde
	[chid] [varchar](4) NOT NULL,                      -- MDB: mt_chan.chid

	-- Convenience data.
	[oper] [varchar](6) NULL,                          -- MDB: mt_site.oper
	[oprtyp] [varchar](2) NULL,                        -- 'FT' or 'CT'.
	[antnumbtx1] [tinyint] NULL,                       -- MDB: mt_chan.antnumbtx1
	[antnumbrx1] [tinyint] NULL,                       -- MDB: mt_chan.antnumbrx1

	-- The associated licence data and qualifiers.
	[tx_licence] [varchar](63) NULL,                   -- licence number from MDB or TAFL data.
	[tx_provenance] [varchar](22) NULL,                -- One of:
	                                                   --			'NON_MEMBER_MT_ANTE',
													   --			'MEMBER_MT_ANTE', 
													   --			'TAFL_LAML_MATCH',
													   --			'WEBMICS_MEMBER_ENTERED'
	[tx_mdate] [varchar](10) NULL,                     -- Date associated with the source of Tx licence data (YYYY.MM.DD).
	[rx_licence] [varchar](63) NULL,                   -- licence number from MDB or TAFL data.
	[rx_provenance] [varchar](22) NULL,                -- As per tx_provenance.
	[rx_mdate] [varchar](10) NULL,                     -- Date associated with the source of Rx licence data (YYYY.MM.DD).

	-- Details of the matching TX record in the TAFL data.
	[tx_AuthorizationNumber] [varchar](63) NULL,       -- TAFL: authorization number (Tx).
	[tx_AccountNumber] [varchar](255) NULL,            -- TAFL: account number (Tx).
	[tx_licenceeName] [varchar](255) NULL,             -- TAFL: licencee name (Tx).                     
	[tx_Freq] [float] NULL,    				           -- TAFL: frequency (Tx).
	[tx_Lat] [float] NULL,    					       -- TAFL: latitude (Tx).
	[tx_Lng] [float] NULL,    						   -- TAFL: longitude (Tx).
	[tx_Azmth] [float] NULL,    					   -- TAFL: azimuth (Tx).
	[tx_Aht] [float] NULL,    						   -- TAFL: antenna's height above ground level (Tx).
	[tx_AuthorizationStatus] [varchar](2) NULL,        -- TAFL: Authorization status (Tx).
	[tx_Callsign] [varchar](255) NULL,                 -- TAFL: callsign (Tx).
	[tx_InserviceDate] [varchar](10) NULL,             -- TAFL: inService date (Tx) as (YYYY.MM.DD).
	[tx_FOM] [int] NULL,                               -- TAFL: FOM (Tx).
	-- Details of the matching RX record in the TAFL data.
	[rx_AuthorizationNumber] [varchar](63) NULL,       -- TAFL: authorization number (Rx).
	[rx_AccountNumber] [varchar](255) NULL,            -- TAFL: account number (Rx).
	[rx_licenceeName] [varchar](255) NULL,             -- TAFL: licencee name (Rx).                     
	[rx_Freq] [float] NULL,    				           -- TAFL: frequency (Rx).
	[rx_Lat] [float] NULL,    					       -- TAFL: latitude (Rx).
	[rx_Lng] [float] NULL,    						   -- TAFL: longitude (Rx).
	[rx_Azmth] [float] NULL,    					   -- TAFL: azimuth (Rx).
	[rx_Aht] [float] NULL,    						   -- TAFL: antenna's height above ground level (Rx).
	[rx_AuthorizationStatus] [varchar](2) NULL,        -- TAFL: Authorization status (Rx).
	[rx_Callsign] [varchar](255) NULL,                 -- TAFL: callsign (Rx).
	[rx_InserviceDate] [varchar](10) NULL,             -- TAFL: inService date (Rx) as (YYYY.MM.DD).
	[rx_FOM] [int] NULL,                               -- TAFL: FOM (Rx).
	-- Metadata describing the specific TAFL source data used for the matching. 
	[TaflDate] [varchar](10) NULL,                     -- TAFL: date of CSV file inside downloaded zip file as (YYYY.MM.DD).

	-- Date time of this record's insertion.
	[mdate] [varchar](10) NULL,                        -- Date of this record's inserstion into the table as (YYYY.MM.DD).
	[mtime] [varchar](5) NULL,						   -- Time of this record's inserstion into the table as (HH:MM).
 CONSTRAINT [PK_sd_licn_20230818] PRIMARY KEY CLUSTERED 
(
	[call1] ASC,
	[call2] ASC,
	[bndcde] ASC,
	[chid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


