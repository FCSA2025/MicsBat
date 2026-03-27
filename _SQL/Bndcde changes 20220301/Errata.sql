--------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------
--
-- The purpose of this T-SQL script is to correct the few instances of antenna and/or 
-- channels records that were 'missed' by the previous changes. 
--
-- The most common type of 'misses' occur when a channel references both a main antenna and 
-- one or two diversity antennas. In this case, the channel record is 'associated' with
-- multiple antenna records.
--
-- It would be possible to enhance the T-SQL code in the previous scripts to handle the
-- complications described above. However, this would add significant additional complexity.
--
-- It is important to note that, after running the other T-SQL scripts, there are very few 
-- 'missed' antenna or channels:
--
--      a. qty. 2 instances of antenna records that erroneously have no channels assigned
--          to them. (Different ends of the same single link.)
--
--      b. qty. 26 instances of channel records that erroneously have no antennas assigned
--         to them. (Different ends of 13 links.)
--
-- Consider (say) the MDB records that were assigned to bndcde 6B: there are qty. 17,796 
-- antennas and qty. 26,668 channels previously using 6B. Out of these, the previous T-SQL
-- scripts failed to correctly handle just 26 channels records - approx. 0.1% (1 in 1000).
--
-- Consequently, the few instances of failed bndcde reassignment left after executing the
-- other scripts are best handled 'manually' in this errata script. 
--------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------

USE fcsa
GO

DECLARE @theCall1 CHAR(9)
DECLARE @theCall2 CHAR(9)
DECLARE @theBndcde CHAR(4) = '6B'
DECLARE @theAnum SMALLINT
DECLARE @theChid CHAR(4)

----------------------------------------------------------------------------------------------
-- Errata:  VCU853, XOA725.
----------------------------------------------------------------------------------------------
UPDATE hulme.mt_ante SET bndcde='6A' WHERE call1='VCU853' AND call2='XOA725' AND bndcde='6B' AND anum='12'
UPDATE hulme.mt_ante SET bndcde='6A' WHERE call1='XOA725' AND call2='VCU853' AND bndcde='6B' AND anum='11'

----------------------------------------------------------------------------------------------
-- Errata:  CJL824, VEL486.
----------------------------------------------------------------------------------------------

SET @theCall1 = 'CJL824'
SET @theCall2 = 'VEL486'
SET @theAnum = 11
SET @theChid = '1003'

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

----------------------------------------------------------------------------------------------
-- Errata:  CYY967, XLU791.
----------------------------------------------------------------------------------------------

SET @theCall1 = 'CYY967'
SET @theCall2 = 'XLU791'
SET @theAnum = 11
SET @theChid = '1001'

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

SET @theAnum = 12
SET @theChid = '1002'
INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

----------------------------------------------------------------------------------------------
-- Errata:  VAD429, VEL878.
----------------------------------------------------------------------------------------------
SET @theCall1 = 'VAD429'
SET @theCall2 = 'VEL878'
SET @theAnum = 11
SET @theChid = '1003'

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 
SET @theChid = '1004'
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid

SET @theChid = '1003'
INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid 
SET @theChid = '1004'
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

----------------------------------------------------------------------------------------------
-- Errata:  VEL384, VEL389.
----------------------------------------------------------------------------------------------
SET @theCall1 = 'VEL384'
SET @theCall2 = 'VEL389'
SET @theAnum = 11
SET @theChid = '1001'

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

SET @theAnum = 12
SET @theChid = '1003'
INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

----------------------------------------------------------------------------------------------
-- Errata:  VEL389, VEL387.
----------------------------------------------------------------------------------------------
SET @theCall1 = 'VEL389'
SET @theCall2 = 'VEL387'
SET @theAnum = 11
SET @theChid = '1001'

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

SET @theAnum = 12
SET @theChid = '1001'
INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6A' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6A' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

----------------------------------------------------------------------------------------------
-- Errata:  VFG382, VFG386.
----------------------------------------------------------------------------------------------
SET @theCall1 = 'VFG382'
SET @theCall2 = 'VFG386'
SET @theAnum = 11
SET @theChid = '1001'

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

SET @theAnum = 12
SET @theChid = '1002'
INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall1 AND call2=@theCall2 AND bndcde=@theBndcde AND chid=@theChid 

INSERT INTO hulme.mt_ante SELECT * FROM fcsaSQL3.fcsa.main.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum  
UPDATE hulme.mt_ante SET bndcde='6D' FROM hulme.mt_ante WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND anum=@theAnum 
UPDATE hulme.mt_chan SET bndcde='6D' FROM hulme.mt_chan WHERE call1=@theCall2 AND call2=@theCall1 AND bndcde=@theBndcde AND chid=@theChid

SELECT 'Correction of errata has been completed.'