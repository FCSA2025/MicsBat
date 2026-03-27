USE [fcsa];
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

-- Drop any existing table.
IF OBJECT_ID('hulme.ofm_spectral_fee') IS NOT NULL DROP TABLE hulme.ofm_spectral_fee

CREATE TABLE hulme.ofm_spectral_fee(
	[date] [date] NULL,
	[oper] [char](6) NULL,
	[feetx] [char](2) NULL,
	[traftx] [char](6) NULL,
	[txcount] [smallint] NULL,
	[txamt] [int] NULL,
	[feerx] [char](2) NULL,
	[trafrx] [char](6) NULL,
	[rxcount] [smallint] NULL,
	[rxamt] [int] NULL,
	[mycol] [uniqueidentifier] ROWGUIDCOL  NULL
) ON [PRIMARY];

ALTER TABLE hulme.ofm_spectral_fee ADD  DEFAULT (newid()) FOR [mycol];

-- Initial population of the master table using records from mt_chan.
INSERT INTO hulme.ofm_spectral_fee
	SELECT 
		getdate(),
		oper,
		feetx, traftx, 0, 0,
		feerx, trafrx, 0, 0,
		newid()
	FROM main.mt_chan
	JOIN main.mt_site ON mt_site.call1 = mt_chan.call1
	WHERE main.mt_site.oprtyp = 'FT'

-- Update the records in hulme.nfm_ts_links that have feetx = 'X'.
-- Use the record's traftx value to lookup the fee code from the table acct.traf_fee.
-- Do the same for feerx.
UPDATE hulme.ofm_spectral_fee
	SET feetx = LookUp.fee_factor
    FROM acct.traf_fee AS LookUp
    WHERE 
		feetx = 'X'
        AND hulme.ofm_spectral_fee.traftx = LookUp.traffic;

UPDATE hulme.ofm_spectral_fee
	SET feerx = LookUp.fee_factor
    FROM acct.traf_fee AS LookUp
    WHERE 
		feerx = 'X'
        AND hulme.ofm_spectral_fee.trafrx = LookUp.traffic

-- Populate the txamt and rxamt columns using the fee table techdef.fee_codes.
UPDATE hulme.ofm_spectral_fee
	SET txamt = LookUp.fee
    FROM techdef.fee_codes AS LookUp
    WHERE 
		hulme.ofm_spectral_fee.feetx = LookUp.code;

UPDATE hulme.ofm_spectral_fee
	SET rxamt = LookUp.fee
    FROM techdef.fee_codes AS LookUp
    WHERE 
		hulme.ofm_spectral_fee.feerx = LookUp.code;

SELECT * FROM hulme.ofm_spectral_fee

-- Create a roll-up of fees per FCSA member / operator.

-- Drop any existing roll-up table.
IF OBJECT_ID('hulme.ofm_spectral_fee_rollup') IS NOT NULL DROP TABLE hulme.ofm_spectral_fee_rollup;

-- Create a new roll-up table.
CREATE TABLE hulme.ofm_spectral_fee_rollup(
	[date] [date] NULL,
	[oper] [char](6) NOT NULL,
	[txamt] [int] NULL,
	[rxamt] [int] NULL,
	[total_amt] [int] NULL,
) ON [PRIMARY];

-- Populate the roll-up table.
INSERT INTO hulme.ofm_spectral_fee_rollup
	SELECT
		date,
		oper,
		SUM(txamt),
		SUM(rxamt),
		total_amt = SUM(txamt) + SUM(rxamt)
	FROM hulme.ofm_spectral_fee
		GROUP BY date,oper
		ORDER BY oper;

SELECT * FROM hulme.ofm_spectral_fee_rollup ORDER BY date, oper;

SELECT
	date, 
	A.oper, B.urbanLinks, B.ruralLinks, B.remoteLinks, B.numLinks, B.bw_MHz, 
	total_amt AS old_ts_fee, 
	d_per_year AS new_ts_fee
	FROM hulme.ofm_spectral_fee_rollup AS A
	JOIN hulme.nfm_ts_d_per_oper AS B ON B.oper = A.oper
	ORDER BY date, oper
--oper, urbanLinks, ruralLinks, remoteLinks, numLinks, bw_MHz, d_per_year


SET ANSI_PADDING OFF;


