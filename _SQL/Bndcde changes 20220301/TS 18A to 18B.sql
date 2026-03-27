-- '18A' to '18B'.

--===========================================================================================
-- This T-SQL script implements the updates required to main.mt_ante and main.mt_chan tables
-- to change records whose bndcde is '18A' to be '18B'.
--
-- To avoid incorrect changes being made to the MDB TS mt_ante and mt_chan tables we first 
-- copy the contents into 'local working' tables hulme.mt_ante and hulme.mt_chan.
-- The MDB tables can then be updated from the local working tables once the changes have
-- been inspected and approved.
--
-- Changing the bndcde of existing MDB ante and chan records is not a trivial endeavour because 
-- the obvious update query can often fail because a record with the proposed/changed key set 
-- already exits. E.g. consider:
--
--      mt_ante record key set (call1,call2,bndce,anum):  'mycall1', 'mycall2', '18A', 24
--
--      the update of this record to have key set         'mycall1', 'mycall2', '18B', 24
--      could fail if this key set already exists.
--
-- A similar problem exists when updating the bndcde of mt_chan records.
--
-- The solution is to detect when an updated key set 'clash' will occur and to modify the
-- anum (or chid) fields until the changed record does not a clash with an existing record.
--
-- Note,also, that changes to the anum of an ante records will also require changes to
-- any mt_chan records associated with the updated antenna.
--
--===========================================================================================

USE fcsa
GO

-----------------------------------------------------------------------------------------
-- Create a table that captures the '18A' antenna records whose anum needs to be changed
-- to avoid a call1, call2, bndcde, anum key set clash.
-----------------------------------------------------------------------------------------

-- Create an intermediate table containing all mt_ante records where the bndcde update
-- would create a key set clash with an existing record.
IF OBJECT_ID('hulme.temp1') IS NOT NULL DROP TABLE hulme.temp1
SELECT AY.call1, AY.call2, AY.bndcde, AY.anum, 0 AS newAnum
    INTO hulme.temp1
    FROM hulme.mt_ante AS AX
	INNER JOIN hulme.mt_ante AS AY ON (AX.call1=AY.call1 AND AX.call2=AY.call2 AND AX.bndcde='18B' AND AY.bndcde='18A' AND AX.anum=AY.anum)

-- For existing '18B' ante records, identify all the existing anums for which a clash could occur.
IF OBJECT_ID('hulme.temp2') IS NOT NULL DROP TABLE hulme.temp2
SELECT A.call1 AS call1, A.call2 AS call2, T1.bndcde AS bndcde, T1.anum AS anum, A.anum AS anumX
    INTO hulme.temp2
    FROM hulme.temp1 AS T1
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T1.call1 AND A.call2=T1.call2 AND A.bndcde='18B')
	WHERE T1.bndcde='18A'

-- Calculate the maximum (+1) of the '18B' anums and set this as the anum of the '18A' records.
IF OBJECT_ID('hulme.temp3') IS NOT NULL DROP TABLE hulme.temp3
SELECT MIN(T2.call1) AS call1, MIN(T2.call2) AS call2, MIN(T2.bndcde) AS bndcde, MIN(T2.anum) AS anum, MAX(T2.anumX)+1 AS newAnum
    INTO hulme.temp3
    FROM hulme.temp2 AS T2
	GROUP BY call1, call2, bndcde, anum

-------------------------------------------------------------------------------------
-- Use the table hulme.temp2 to update the anum fields in hulme.mt_ante, as required.
-------------------------------------------------------------------------------------
UPDATE hulme.mt_ante
	SET anum = T.newAnum
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.temp3 AS T ON (T.call1=A.call1 AND T.call2=A.call2 AND T.bndcde='18A' AND T.anum=A.anum)
	WHERE A.bndcde='18A'

-----------------------------------------------------------------------------------
-- Now update the mt_chan table to reflect the anum changes in the mt_ante records.
-----------------------------------------------------------------------------------
UPDATE hulme.mt_chan
	SET antnumbtx1 = (CASE WHEN (C.antnumbtx1 = T.anum) THEN T.newAnum ELSE antnumbtx1 END),
	    antnumbtx2 = (CASE WHEN (C.antnumbtx2 = T.anum) THEN T.newAnum ELSE antnumbtx2 END),
	    antnumbrx1 = (CASE WHEN (C.antnumbrx1 = T.anum) THEN T.newAnum ELSE antnumbrx1 END),
	    antnumbrx2 = (CASE WHEN (C.antnumbrx2 = T.anum) THEN T.newAnum ELSE antnumbrx2 END),
		antnumbrx3 = (CASE WHEN (C.antnumbrx3 = T.anum) THEN T.newAnum ELSE antnumbrx3 END)
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='18A')
	WHERE C.bndcde='18A'

----------------------------------------------------------------------------------------------------------
-- For those mt_chan records that changing bndcde from 11B to 11A would result in a duplicate key conflict
-- change the chid to create an (as yet) unused key set.
--
-- This will be achieved by the simple expedient of changing the first character of a chid string to 'X'.
----------------------------------------------------------------------------------------------------------
UPDATE CY
	SET chid = 'X' + RIGHT(CY.chid, 3) /*RIGHT('000' + CONVERT(VARCHAR(3), @i), 3)*/
    FROM hulme.mt_chan AS CX
	INNER JOIN hulme.mt_chan AS CY ON (CX.call1=CY.call1 AND CX.call2=CY.call2 AND CX.bndcde='18B' AND CY.bndcde='18A' AND CX.chid=CY.chid)

-------------------------------------------------------------------------------------
-- We are now ready to perform the 'simple' updates to the mt_ante and mt_chan tables
-- to change bndcde '18A' to '18B' because we have now ensured that there will be no
-- key set clashes with existing records.
---------------------------------------------------------------------------
UPDATE hulme.mt_ante SET bndcde = '18B' WHERE bndcde = '18A'

UPDATE hulme.mt_chan SET bndcde = '18B' WHERE bndcde = '18A'

-----------------------------------------------------------
-- Clean up.
-----------------------------------------------------------
DROP TABLE hulme.temp1
DROP TABLE hulme.temp2
DROP TABLE hulme.temp3
GO

SELECT 'Reassignment of MDB TS records using bndcde 18A to 18B has been completed.'

-----------------------------------
-- Inspect and approve the results.
-----------------------------------
/*
SELECT * FROM hulme.mt_ante WHERE bndcde = '18A'

SELECT * FROM hulme.temp2
SELECT * FROM hulme.temp3

SELECT A.call1, A.call2, A.bndcde, A.anum, C.chid, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE C.bndcde='18B' AND ((antnumbtx1 IS NOT NULL) OR (antnumbtx2 IS NOT NULL) OR (antnumbrx1 IS NOT NULL) OR (antnumbrx2 IS NOT NULL) OR (antnumbrx3 IS NOT NULL))--WHERE A.call1='WPVL20301' AND A.call2='WPVL20303'
	ORDER BY antnumbrx1 DESC, antnumbtx1 DESC, call1, call2, bndcde, anum

SELECT AY.call1, AY.call2, AY.bndcde, AY.anum, AX.bndcde, AX.anum
    FROM hulme.mt_ante AS AX
	INNER JOIN hulme.mt_ante AS AY ON (AX.call1=AY.call1 AND AX.call2=AY.call2 AND AX.bndcde='18B' AND AY.bndcde='18A')

SELECT * FROM hulme.mt_chan WHERE call1='CGV837' AND call2='CGY624'
SELECT * FROM hulme.mt_chan WHERE chid LIKE 'X%'
SELECT * FROM fcsaSQL3.fcsa.main.mt_ante ORDER BY mdate DESC, mtime DESC
*/

