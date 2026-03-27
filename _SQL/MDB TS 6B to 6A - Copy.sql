-- '6B' to '6A'.

--===========================================================================================
-- This T-SQL script implements the updates required to main.mt_ante and main.mt_chan tables
-- to change records whose bndcde is '6B' to be '6A'.
--
-- Note that, in the bigger picture, MDB records using bndcde 6B will be transferred to use
-- bndcdes 6A, 6D or 6E DEPENDING THE RECORD'S TX AND RX FREQUENCIES.
--                      =============================================
--
-- As a consequence, we need to test that a record whose bndcde is being changed uses
-- the correct frequency range for the 'target' bndcde 6A.
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
--      mt_ante record key set (call1,call2,bndce,anum):  'mycall1', 'mycall2', '6B', 24
--
--      the update of this record to have key set         'mycall1', 'mycall2', '6A', 24
--      could fail if this key set already exists.
--
-- A similar problem exists when updating the bndcde of mt_chan records.
--
-- The solution is to detect when an updated key set 'clash' will occur and to modify the
-- anum (or chid) fields until the changed record does not a clash with an existing record.
--
-- Note, however, that changes to the anum of an ante records will also require changes to
-- any mt_chan records associated with the updated antenna.
--
--===========================================================================================

USE fcsa
GO

-----------------------------------------------------------------------------------------
-- Create a table that captures the '6B' antenna records whose anum needs to be changed
-- to avoid a call1, call2, '6A', anum key set clash.
-----------------------------------------------------------------------------------------

-- Create an intermediate table containing all mt_ante records where the bndcde update
-- would create a key set clash with an existing record.
IF OBJECT_ID('hulme.temp1') IS NOT NULL DROP TABLE hulme.temp1
SELECT AY.call1, AY.call2, AY.bndcde, AY.anum, 0 AS newAnum
    INTO hulme.temp1
    FROM hulme.mt_ante AS AX
	INNER JOIN hulme.mt_ante AS AY ON (AX.call1=AY.call1 AND AX.call2=AY.call2 AND AX.bndcde='6A' AND AY.bndcde='6B' AND AX.anum=AY.anum)

-- For existing '6A' ante records, identify all the existing anums for which a clash could occur.
IF OBJECT_ID('hulme.temp2') IS NOT NULL DROP TABLE hulme.temp2
SELECT A.call1 AS call1, A.call2 AS call2, T1.bndcde AS bndcde, T1.anum AS anum, A.anum AS anumX
    INTO hulme.temp2
    FROM hulme.temp1 AS T1
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T1.call1 AND A.call2=T1.call2 AND A.bndcde='6A')
	WHERE T1.bndcde='6B'

-- Calculate the maximum (+1) of the '6A' anums and set this as the anum of the '6B' records.
IF OBJECT_ID('hulme.temp3') IS NOT NULL DROP TABLE hulme.temp3
SELECT MIN(T2.call1) AS call1, MIN(T2.call2) AS call2, MIN(T2.bndcde) AS bndcde, MIN(T2.anum) AS anum, MAX(T2.anumX)+MIN(T2.anum) AS newAnum
    INTO hulme.temp3
    FROM hulme.temp2 AS T2
	GROUP BY call1, call2, bndcde, anum

SELECT * FROM hulme.temp3

-------------------------------------------------------------------------------------
-- Use the table hulme.temp3 to update the anum fields in hulme.mt_ante, as required.
-------------------------------------------------------------------------------------
UPDATE hulme.mt_ante
	SET anum = T.newAnum
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.temp3 AS T ON (T.call1=A.call1 AND T.call2=A.call2 AND T.bndcde='6B' AND T.anum=A.anum)
	WHERE A.bndcde='6B'

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
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='6B')
	WHERE C.bndcde='6B' AND (((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)))

/*
UPDATE hulme.mt_chan SET antnumbtx1 = (CASE WHEN (C.antnumbtx1 = T.anum) THEN T.newAnum ELSE antnumbtx1 END)
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='6B')
	WHERE C.bndcde='6B' AND (((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)))

UPDATE hulme.mt_chan SET antnumbtx2 = (CASE WHEN (C.antnumbtx2 = T.anum) THEN T.newAnum ELSE antnumbtx2 END)
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='6B')
	WHERE C.bndcde='6B' AND (((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)))

UPDATE hulme.mt_chan SET antnumbrx1 = (CASE WHEN (C.antnumbrx1 = T.anum) THEN T.newAnum ELSE antnumbrx1 END)
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='6B')
	WHERE C.bndcde='6B' AND (((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)))

UPDATE hulme.mt_chan SET antnumbrx2 = (CASE WHEN (C.antnumbrx2 = T.anum) THEN T.newAnum ELSE antnumbrx2 END)
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='6B')
	WHERE C.bndcde='6B' AND (((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)))

UPDATE hulme.mt_chan SET antnumbrx3 = (CASE WHEN (C.antnumbrx3 = T.anum) THEN T.newAnum ELSE antnumbrx3 END)
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde='6B')
	WHERE C.bndcde='6B' AND (((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)))
*/
----------------------------------------------------------------------------------------------------------
-- For those mt_chan records that changing bndcde from '6B' to '6A' would result in a duplicate key conflict
-- change the chid to create an (as yet) unused key set.
--
-- This will be achieved by the simple expedient of changing the first character of a chid string to 'X'.
----------------------------------------------------------------------------------------------------------
UPDATE CY
	SET chid = 'X' + RIGHT(CY.chid, 3) /*RIGHT('000' + CONVERT(VARCHAR(3), @i), 3)*/
    FROM hulme.mt_chan AS CX
	INNER JOIN hulme.mt_chan AS CY ON (CX.call1=CY.call1 AND CX.call2=CY.call2 AND CX.bndcde='6A' AND CY.bndcde='6B' AND CX.chid=CY.chid)
	WHERE (((hulme.IsInRangeFloat(5850000, 6425000, CY.freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, CY.freqrx) = 1)))
-------------------------------------------------------------------------------------
-- We are now ready to perform the 'simple' updates to the mt_ante and mt_chan tables
-- to change bndcde '6B' to '6A' because we have now ensured that there will be no
-- key set clashes with existing records.
---------------------------------------------------------------------------

IF OBJECT_ID('hulme.temp4') IS NOT NULL DROP TABLE hulme.temp4
SELECT call1, call2, bndcde, antnumbtx1 AS anum, chid
	INTO hulme.temp4
	FROM hulme.mt_chan AS C
	WHERE C.bndcde = '6B' AND (antnumbtx1 IS NOT NULL) AND (	((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)) )
UNION
SELECT call1, call2, bndcde, antnumbtx2 AS anum, chid
	FROM hulme.mt_chan AS C
	WHERE C.bndcde = '6B' AND (antnumbtx2 IS NOT NULL) AND (	((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)) )
UNION
SELECT call1, call2, bndcde, antnumbrx1 AS anum, chid
	FROM hulme.mt_chan AS C
	WHERE C.bndcde = '6B' AND (antnumbrx1 IS NOT NULL) AND (	((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)) )
UNION
SELECT call1, call2, bndcde, antnumbrx2 AS anum, chid
	FROM hulme.mt_chan AS C
	WHERE C.bndcde = '6B' AND (antnumbrx2 IS NOT NULL) AND (	((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)) )
UNION
SELECT call1, call2, bndcde, antnumbrx3 AS anum, chid
	FROM hulme.mt_chan AS C
	WHERE C.bndcde = '6B' AND (antnumbrx3 IS NOT NULL) AND (	((hulme.IsInRangeFloat(5850000, 6425000, freqtx) = 1) AND (hulme.IsInRangeFloat(5850000, 6425000, freqrx) = 1)) )

SELECT * FROM hulme.temp4

UPDATE hulme.mt_ante 
	SET bndcde = '6A' 
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.temp4 AS T ON ((T.call1 = A.call1) AND (T.call2 = A.call2) AND (T.bndcde = A.bndcde) AND (T.anum = A.anum))
	WHERE A.bndcde = '6B'

-----------------------------------
-- Inspect and approve the results.
-----------------------------------
/*
SELECT * FROM hulme.mt_ante WHERE bndcde = '6B'

SELECT * FROM hulme.temp2
SELECT * FROM hulme.temp3

SELECT A.call1, A.call2, A.bndcde, A.anum, C.chid, C.antnumbtx1, C.antnumbtx2, C.antnumbrx1, C.antnumbrx2, C.antnumbrx3
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.mt_chan AS C ON (C.call1=A.call1 AND C.call2=A.call2 AND C.bndcde=A.bndcde)
	WHERE C.bndcde='6A' AND ((antnumbtx1 IS NOT NULL) OR (antnumbtx2 IS NOT NULL) OR (antnumbrx1 IS NOT NULL) OR (antnumbrx2 IS NOT NULL) OR (antnumbrx3 IS NOT NULL))--WHERE A.call1='WPVL20301' AND A.call2='WPVL20303'
	ORDER BY antnumbrx1 DESC, antnumbtx1 DESC, call1, call2, bndcde, anum

SELECT AY.call1, AY.call2, AY.bndcde, AY.anum, AX.bndcde, AX.anum
    FROM hulme.mt_ante AS AX
	INNER JOIN hulme.mt_ante AS AY ON (AX.call1=AY.call1 AND AX.call2=AY.call2 AND AX.bndcde='6A' AND AY.bndcde='6B')

SELECT * FROM hulme.mt_chan WHERE call1='CGV837' AND call2='CGY624'
SELECT * FROM hulme.mt_chan WHERE chid LIKE 'X%'
SELECT * FROM fcsaSQL3.fcsa.main.mt_ante ORDER BY mdate DESC, mtime DESC
*/

