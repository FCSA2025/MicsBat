-- '6B' to '6E'.

--===========================================================================================
-- This T-SQL script implements the updates required to main.mt_ante and main.mt_chan tables
-- to change records whose bndcde is '6B' to be '6E'.
--
-- Note that, in the bigger picture, MDB records using bndcde 6B will be transferred to use
-- bndcdes 6A, 6D or 6E DEPENDING THE RECORD'S TX AND RX FREQUENCIES.
--                      =============================================
--
-- As a consequence, we need to test that a record whose bndcde is being changed uses
-- the correct frequency range for the 'target' bndcde 6E.
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
--      the update of this record to have key set         'mycall1', 'mycall2', '6E', 24
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
-- so as to avoid a {call1, call2, '6E', anum} key set CLASH.
-----------------------------------------------------------------------------------------

-- Create an intermediate table containing all mt_ante records where the bndcde update
-- would create a key set CLASH with an existing antenna record.
IF OBJECT_ID('hulme.temp1') IS NOT NULL DROP TABLE hulme.temp1
SELECT AY.call1, AY.call2, AY.bndcde, AY.anum, 0 AS affected, 0 AS newAnum
    INTO hulme.temp1
    FROM hulme.mt_ante AS AX
	INNER JOIN hulme.mt_ante AS AY ON (AX.call1=AY.call1 AND AX.call2=AY.call2 AND AX.bndcde='6E' AND AY.bndcde='6B' AND AX.anum=AY.anum)

--SELECT * FROM hulme.temp1

-- Mark as 'affected' only those antenna records which have an associated channel whose (tx, rx) is within 6E.
UPDATE hulme.temp1
	SET affected = 1
	FROM hulme.temp1 AS T1
	INNER JOIN hulme.mt_chan AS C ON ((C.call1 = T1.call1) AND (C.call2 = T1.call2) AND (C.bndcde = T1.bndcde))
	WHERE   (hulme.IsInRangeFloatTxRx(6590000, 7125000, C.freqtx, C.freqrx) = 1)
		AND ((T1.anum = C.antnumbtx1) OR (T1.anum = C.antnumbtx2) OR (T1.anum = C.antnumbrx1) OR (T1.anum = C.antnumbrx2) OR (T1.anum = C.antnumbrx3))

--SELECT * FROM hulme.temp1

-- For existing '6E' ante records, identify all the existing anums for which a clash could occur.
IF OBJECT_ID('hulme.temp2') IS NOT NULL DROP TABLE hulme.temp2
SELECT A.call1 AS call1, A.call2 AS call2, T1.bndcde AS bndcde, T1.anum AS anum, A.anum AS anumX
    INTO hulme.temp2
    FROM hulme.temp1 AS T1
	INNER JOIN hulme.mt_ante AS A ON (A.call1=T1.call1 AND A.call2=T1.call2 AND A.bndcde='6E')
	WHERE (T1.bndcde='6B') AND (T1.affected = 1)

--SELECT * FROM hulme.temp2

-- Calculate the maximum (+1) of the '6E' anums and set this as the anum of the '6B' records.
IF OBJECT_ID('hulme.temp3') IS NOT NULL DROP TABLE hulme.temp3
SELECT MIN(T2.call1) AS call1, MIN(T2.call2) AS call2, MIN(T2.bndcde) AS bndcde, MIN(T2.anum) AS anum, MAX(T2.anumX)+MIN(T2.anum) AS newAnum
    INTO hulme.temp3
    FROM hulme.temp2 AS T2
	GROUP BY call1, call2, bndcde, anum

--SELECT * FROM hulme.temp3

-------------------------------------------------------------------------------------
-- Use the table hulme.temp3 to update the anum fields in hulme.mt_ante, as required.
-- If the previous CLASH-detection-avoidance was successful the following T-SQL query
-- will execute without error.
-------------------------------------------------------------------------------------
UPDATE hulme.mt_ante
	SET anum = T.newAnum
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.temp3 AS T ON (T.call1=A.call1 AND T.call2=A.call2 AND T.bndcde='6B' AND T.anum=A.anum)
	WHERE A.bndcde='6B'

--SELECT call1, call2, bndcde, anum, ause FROM hulme.mt_ante

-----------------------------------------------------------------------------------
-- Now update the mt_chan table to reflect the anum changes in the mt_ante records.
-----------------------------------------------------------------------------------

UPDATE hulme.mt_chan SET antnumbtx1 = T.newAnum
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde=C.bndcde)
	WHERE (C.antnumbtx1 = T.anum) AND (hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) = 1)

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp3

UPDATE hulme.mt_chan SET antnumbtx2 = T.newAnum
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde=C.bndcde)
	WHERE (C.antnumbtx2 = T.anum) AND (hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) = 1)

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp3

UPDATE hulme.mt_chan SET antnumbrx1 = T.newAnum
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde=C.bndcde)
	WHERE (C.antnumbrx1 = T.anum) AND (hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) = 1)

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp3

UPDATE hulme.mt_chan SET antnumbrx2 = T.newAnum
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde=C.bndcde)
	WHERE (C.antnumbrx2 = T.anum) AND (hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) = 1)

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp3

UPDATE hulme.mt_chan SET antnumbrx3 = T.newAnum
	FROM hulme.mt_chan AS C
	INNER JOIN hulme.temp3 AS T ON (T.call1=C.call1 AND T.call2=C.call2 AND T.bndcde=C.bndcde)
	WHERE (C.antnumbrx3 = T.anum) AND (hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) = 1)

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp3

---------------------------------------------------------------------------------------
-- We are now ready to perform the updates to the mt_ante to change bndcde '6B' to '6E' 
-- because we have ensured that there will be no key set CLASHes with existing records.
---------------------------------------------------------------------------------------

-- Create a table that identifies those antenna records that are qualified to have
-- their bndcde changed from 6B to 6A.
IF OBJECT_ID('hulme.temp4') IS NOT NULL DROP TABLE hulme.temp4
SELECT A.call1, A.call2, A.bndcde, A.anum, C.chid,
    hulme.IsInRangeFloatTxRx(5850000, 6425000, C.freqtx, C.freqrx) AS isIn6A,
	hulme.IsInRangeFloatTxRx(6425000, 6930000, C.freqtx, C.freqrx) AS isIn6D,
	hulme.IsInRangeFloatTxRx(6590000, 7125000, C.freqtx, C.freqrx) AS isIn6E
	INTO hulme.temp4
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.mt_chan AS C ON ((C.call1 = A.call1) AND (C.call2 = A.call2) AND (C.bndcde = A.bndcde))
	WHERE A.bndcde='6B' AND
			((A.anum = C.antnumbtx1) OR (A.anum = C.antnumbtx2) OR (A.anum = C.antnumbrx1) OR (A.anum = C.antnumbrx2) OR (A.anum = C.antnumbrx3))

UPDATE hulme.mt_ante 
	SET bndcde = '6E' 
	FROM hulme.mt_ante AS A
	INNER JOIN hulme.temp4 AS T4 ON ((T4.call1 = A.call1) AND (T4.call2 = A.call2) AND (T4.bndcde = A.bndcde) AND (T4.anum = A.anum))
	WHERE (A.bndcde='6B') AND (isIn6E = 1)

--SELECT call1, call2, bndcde, anum FROM hulme.mt_ante
--SELECT * FROM hulme.temp3

----------------------------------------------------------------------------------------------------------
-- For those mt_chan records where changing bndcde from '6B' to '6E' would result in a duplicate key CLASH
-- change the chid so as to create an (as yet) unused key set.
--
-- This will be achieved by the simple expedient of changing the first character of a chid string to 'X'.
----------------------------------------------------------------------------------------------------------
UPDATE CY
	SET chid = 'X' + RIGHT(CY.chid, 3)
    FROM hulme.mt_chan AS CX
	INNER JOIN hulme.mt_chan AS CY ON (CX.call1=CY.call1 AND CX.call2=CY.call2 AND CX.bndcde='6E' AND CY.bndcde='6B' AND CX.chid=CY.chid)
	WHERE (hulme.IsInRangeFloatTxRx(6590000, 7125000, CY.freqtx, CY.freqrx) = 1) 

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp3

---------------------------------------------------------------------------------------
-- We are now ready to perform the updates to the mt_chan to change bndcde '6B' to '6E' 
-- because we have ensured that there will be no key set CLASHes with existing records.
---------------------------------------------------------------------------------------
UPDATE hulme.mt_chan 
	SET bndcde = '6E' 
	FROM hulme.mt_chan
	WHERE (bndcde='6B') AND (hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) = 1)

--SELECT call1, call2, bndcde, chid, antnumbtx1, antnumbtx2, antnumbrx1, antnumbrx2, antnumbrx3, freqtx, freqrx, hulme.IsInRangeFloatTxRx(6590000, 7125000, freqtx, freqrx) AS inBand6E FROM hulme.mt_chan
--SELECT * FROM hulme.temp1

-----------------------------------------------------------
-- Clean up.
-----------------------------------------------------------
DROP TABLE hulme.temp1
DROP TABLE hulme.temp2
DROP TABLE hulme.temp3
DROP TABLE hulme.temp4
GO

SELECT 'Reassignment of MDB TS records using bndcde 6B to 6E has been completed.'