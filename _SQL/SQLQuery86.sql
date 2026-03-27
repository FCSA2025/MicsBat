USE fcsa
GO

DECLARE @theCall1 CHAR(9)
DECLARE @theCall2 CHAR(9)
DECLARE @theBndcde CHAR(4) = '6B'
DECLARE @theAnum SMALLINT
DECLARE @theChid CHAR(4)

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
