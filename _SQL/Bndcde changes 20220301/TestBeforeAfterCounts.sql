USE fcsa
GO

USE [fcsa]
GO

IF OBJECT_ID('hulme.beforeAfterCounts') IS NOT NULL DROP TABLE hulme.beforeAfterCounts
CREATE TABLE hulme.beforeAfterCounts(
	[tableName] [char](32) NOT NULL,
	[countBefore] [int],
	[countAfter] [int],
	[recNum] [int],
 CONSTRAINT [PK_beforeAfterCounts] PRIMARY KEY CLUSTERED 
(
	[tableName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

--
-- Antenna analysis.
--

INSERT INTO hulme.beforeAfterCounts VALUES ('sd_band', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.sd_band), '0', '1')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : total', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante), '0', '2')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  1C', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '1C'), '0', '3')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  1D', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '1D'), '0', '4')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  3D', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '3D'), '0', '5')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  5A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '5A'), '0', '6')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  6C', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '6C'), '0', '7')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  7A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '7A'), '0', '8')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde = 11A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '11A'), '0', '9')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde = 11B', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '11B'), '0', '10')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde = 18A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '18A'), '0', '11')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde = 18B', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '18B'), '0', '12')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  6B', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '6B'), '0', '13')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  6A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '6A'), '0', '14')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  6D', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '6D'), '0', '15')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_ante : bndcde =  6E', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_ante WHERE bndcde = '6E'), '0', '16')

UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.sd_band) FROM hulme.beforeAfterCounts WHERE recNum = 1
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante) FROM hulme.beforeAfterCounts WHERE recNum = 2
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '1C') FROM hulme.beforeAfterCounts WHERE recNum = 3
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '1D') FROM hulme.beforeAfterCounts WHERE recNum = 4
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '3D') FROM hulme.beforeAfterCounts WHERE recNum = 5
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '5A') FROM hulme.beforeAfterCounts WHERE recNum = 6
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '6C') FROM hulme.beforeAfterCounts WHERE recNum = 7
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '7A') FROM hulme.beforeAfterCounts WHERE recNum = 8
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '11A') FROM hulme.beforeAfterCounts WHERE recNum = 9
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '11B') FROM hulme.beforeAfterCounts WHERE recNum = 10
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '18A') FROM hulme.beforeAfterCounts WHERE recNum = 11
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '18B') FROM hulme.beforeAfterCounts WHERE recNum = 12
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '6B') FROM hulme.beforeAfterCounts WHERE recNum = 13
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '6A') FROM hulme.beforeAfterCounts WHERE recNum = 14
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '6D') FROM hulme.beforeAfterCounts WHERE recNum = 15
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_ante WHERE bndcde = '6E') FROM hulme.beforeAfterCounts WHERE recNum = 16

--
-- Channel analysis.
--

INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : total', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan), '0', '102')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  1C', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '1C'), '0', '103')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  1D', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '1D'), '0', '104')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  3D', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '3D'), '0', '105')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  5A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '5A'), '0', '106')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  6C', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '6C'), '0', '107')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  7A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '7A'), '0', '108')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde = 11A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '11A'), '0', '109')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde = 11B', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '11B'), '0', '110')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde = 18A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '18A'), '0', '111')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde = 18B', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '18B'), '0', '112')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  6B', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '6B'), '0', '113')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  6A', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '6A'), '0', '114')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  6D', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '6D'), '0', '115')
INSERT INTO hulme.beforeAfterCounts VALUES ('mt_chan : bndcde =  6E', (SELECT COUNT(*) FROM fcsaSQL3.fcsa.main.mt_chan WHERE bndcde = '6E'), '0', '116')

UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan) FROM hulme.beforeAfterCounts WHERE recNum = 102
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '1C') FROM hulme.beforeAfterCounts WHERE recNum = 103
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '1D') FROM hulme.beforeAfterCounts WHERE recNum = 104
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '3D') FROM hulme.beforeAfterCounts WHERE recNum = 105
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '5A') FROM hulme.beforeAfterCounts WHERE recNum = 106
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '6C') FROM hulme.beforeAfterCounts WHERE recNum = 107
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '7A') FROM hulme.beforeAfterCounts WHERE recNum = 108
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '11A') FROM hulme.beforeAfterCounts WHERE recNum = 109
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '11B') FROM hulme.beforeAfterCounts WHERE recNum = 110
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '18A') FROM hulme.beforeAfterCounts WHERE recNum = 111
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '18B') FROM hulme.beforeAfterCounts WHERE recNum = 112
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '6B') FROM hulme.beforeAfterCounts WHERE recNum = 113
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '6A') FROM hulme.beforeAfterCounts WHERE recNum = 114
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '6D') FROM hulme.beforeAfterCounts WHERE recNum = 115
UPDATE hulme.beforeAfterCounts SET countAfter = (SELECT COUNT(*) FROM fcsaSQL5D.fcsa.hulme.mt_chan WHERE bndcde = '6E') FROM hulme.beforeAfterCounts WHERE recNum = 116

SELECT * FROM hulme.beforeAfterCounts ORDER BY recNum