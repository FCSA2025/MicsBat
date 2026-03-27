----------------------------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------
--
-- This T-SQL script implements all of the changes required to the MDB sd_band table 
-- i.a.w. the requirements defined below:
--
--    sd_band: deletions:	'1C', '1D', '3D', '5A', '6C'. 
--							(These are also deleted from the adjacent band list field.  
--
--    sd_band: changes:  
--							a.  '1A'  : bmidf = 1472500,  bhi = 1518000
--							b.  '2H'  : bmidf = 2595000,  bhi = 2690000
--							c.  '10A' : bmidf = 10590000, bhi = 10680000
--							d.  '13A' : bmidf = 12975000, bhi = 13250000
--							e.  '22A' : bmidf = 22700000
--
----------------------------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------

USE fcsa
GO

DECLARE @NUM_1A_ORPHANS INT;
DECLARE @NUM_10A_ORPHANS INT;
DECLARE @NUM_11B_ORPHANS INT;
DECLARE @NUM_13A_ORPHANS INT;
DECLARE @NUM_18A_ORPHANS INT;
DECLARE @NUM_7A_ORPHANS INT;

DECLARE @message VARCHAR(500);

-- Create a simple table to capture the actions and results.
IF OBJECT_ID('hulme.MDBupdateLog') IS NOT NULL DROP TABLE hulme.MDBupdateLog

CREATE TABLE hulme.MDBupdateLog(
	[sd_band_changes] [varchar](500) NOT NULL
)

---------------------------------------------------------------------
-- main.sd_band -----------------------------------------------------
---------------------------------------------------------------------

-- Perform the required sd_band record deletions.
DELETE FROM hulme.sd_band WHERE bndcde = '1C'
DELETE FROM hulme.sd_band WHERE bndcde = '1D'
DELETE FROM hulme.sd_band WHERE bndcde = '3D'
DELETE FROM hulme.sd_band WHERE bndcde = '5A'
DELETE FROM hulme.sd_band WHERE bndcde = '6C'

INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: deleted records whose bndcde in {1C, 1D, 3D, 5A, 6C}.')

-- We have to remove references to the deleted bands in the 'badj' fields.
-- First, insert a ';' at the start of every badj to 'regularize' the substring replacements.
UPDATE hulme.sd_band SET badj = ';' + badj
-- Now replace the deleted band tokens with empty strings.
UPDATE hulme.sd_band SET badj = REPLACE(badj, ';1C', '')
UPDATE hulme.sd_band SET badj = REPLACE(badj, ';1D', '')
UPDATE hulme.sd_band SET badj = REPLACE(badj, ';3D', '')
UPDATE hulme.sd_band SET badj = REPLACE(badj, ';5A', '')
UPDATE hulme.sd_band SET badj = REPLACE(badj, ';6C', '')
-- Remove the ';' at the start of every badj to restore the previous format.
UPDATE hulme.sd_band SET badj = SUBSTRING(badj, 2, LEN(badj) - 1)

INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: updated all badj fields to remove references to {1C, 1D, 3D, 5A, 6C}.')

-- Update bndcde 1A to match frequency bands defined in SRSP 301.4
SELECT @NUM_1A_ORPHANS = COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE (hulme.IsInOpenRangeFloat(1518000, 1525000, freqtx) = 1) OR (hulme.IsInOpenRangeFloat(1518000, 1525000, freqrx) = 1)
UPDATE hulme.sd_band SET bmidf = 1472500, bhi = 1518000 WHERE (@NUM_1A_ORPHANS = 0) AND bndcde = '1A'

IF (@NUM_1A_ORPHANS = 0) INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi updated for bndcde 1A.'); 
ELSE INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi update for bndcde 1A; orphan channels were found; FAILURE.');

-- Update bndcde 2H upper limit to 2,690 MHz to match with FCC.
UPDATE hulme.sd_band SET bmidf = 2595000, bhi = 2690000 WHERE bndcde = '2H';
INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi updated for bndcde 2H.');

-- Update bndcde 10A to match frequency bands defined in SRSP 310.5
SELECT @NUM_10A_ORPHANS = COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE (hulme.IsInOpenRangeFloat(10680000, 10700000, freqtx) = 1) OR (hulme.IsInOpenRangeFloat(10680000, 10700000, freqrx) = 1)
UPDATE hulme.sd_band SET blo = 10550000, bmidf = 10615000, bhi = 10680000 WHERE (@NUM_10A_ORPHANS = 0) AND bndcde = '10A'

IF (@NUM_10A_ORPHANS = 0) INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi updated for bndcde 10A.'); 
ELSE INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi update for bndcde 10A; orphan channels were found; FAILURE.');

-- Update bndcde 13A to match frequency bands defined in SRSP 312.7
SELECT @NUM_13A_ORPHANS = COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE (hulme.IsInOpenRangeFloat(13250000, 13750000, freqtx) = 1) OR (hulme.IsInOpenRangeFloat(13250000, 13750000, freqrx) = 1)
UPDATE hulme.sd_band SET bmidf = 12975000, bhi = 13250000 WHERE (@NUM_13A_ORPHANS = 0) AND bndcde = '13A'

IF (@NUM_13A_ORPHANS = 0) INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi updated for bndcde 13A.'); 
ELSE INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf, bhi update for bndcde 13A; orphan channels were found; FAILURE.');

-- Update bndcde 22A to match mid-frequency with that defined in SRSP 321.8
UPDATE hulme.sd_band SET bmidf = 22700000 WHERE bndcde = '22A'

INSERT INTO hulme.MDBupdateLog (sd_band_changes) VALUES ('sd_band: bmidf updated for bndcde 22A.');

/*
-- Test the updated contents of hulme.sd_band
SELECT * FROM hulme.sd_band
SELECT * FROM hulme.sd_band WHERE bndcde IN ('1C', '1D', '3D', '5A', '6C')
SELECT * FROM hulme.sd_band WHERE badj LIKE '%[^0-9]1C%'
SELECT * FROM hulme.sd_band WHERE badj LIKE '%[^0-9]1D%'
SELECT * FROM hulme.sd_band WHERE badj LIKE '%[^0-9]3D%'
SELECT * FROM hulme.sd_band WHERE badj LIKE '%[^0-9]5A%'
SELECT * FROM hulme.sd_band WHERE badj LIKE '%[^0-9]6C%'
*/
SELECT * FROM hulme.MDBupdateLog

