----------------------------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------
--
-- This is the main 'program' that implements all of the changes required to the
-- MDB sd_band, mt_ante, and mt_chan tables i.a.w. the requirements defined below:
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
--     mt_ante, mt_chan:
--							a.	Reassign records with bndcde  7A to  6E;
--                          b.  Reassign records with bndcde 11B to 11A;
--                          c.  Reassign records with bndcde 18A to 18B;
--                          d.  Reassign records with bndcde  6B to  6A (depending on frequency);
--                          e.  Reassign records with bndcde  6B to  6D (depending on frequency);
--                          f.  Reassign records with bndcde  6B to  6E (depending on frequency);
--
--     NOTES:
--            1. The number of lines of T-SQL queries runs into several hundred and so the task
--               is decomposed into the execution of several individual T-SQL scripts each one
--               having a well-defined function.
--			  2. To avoid incorrect changes being made to the MDB sd_band table we first copy the 
--				 contents into 'local working' tables hulme.sd_band. The MDB tables can then be 
--				 updated from the local working tables once the changes have been inspected and approved.		     
--			  3. Similarly for the tables MDB TS mt_ante and mt_chan tables we first copy the contents 
--				 into 'local working' tables hulme.mt_ante and hulme.mt_chan. The MDB tables can then be
--				 updated from the local working tables once the changes have been inspected and approved.
--			     
----------------------------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------

USE fcsa
GO

-----------------------------------------------------------------------
-- sd_band changes.
-----------------------------------------------------------------------

-- Make a local 'working' copy of the MDB sd_band table.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\Copy sd_band.sql"

-- Implement the required changes to the local copy of sd_band.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\Sd_band changes.sql"

-----------------------------------------------------------------------
-- mt_ante and mt_chan changes.
-----------------------------------------------------------------------

-- Make local 'working' copies of the MDB TS mt_ante and mt_chan tables.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\Copy mt_ante & mt_chan.sql"

-- Reassign MDB TS records using bndcde 7A to 6E.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\TS 7A to 6E.sql"

-- Reassign MDB TS records using bndcde 11B to 11A.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\TS 11B to 11A.sql"

-- Reassign MDB TS records using bndcde 18A to 18B.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\TS 18A to 18B.sql"

-- Reassign MDB TS records using bndcde 6B to 6A, depending on frequency.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\TS 6B to 6A.sql"

-- Reassign MDB TS records using bndcde 6B to 6D, depending on frequency.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\TS 6B to 6D.sql"

-- Reassign MDB TS records using bndcde 6B to 6E, depending on frequency.
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\TS 6B to 6E.sql"

--
-- Correct the few instances of antenna and/or channels records that were 'missed'
-- by the previous changes. (The most common type of 'misses' are to do with a 
-- channel using both a main antenna and a diversity antenna.)
:r "D:\Users\ahulme\MICS#\_SQL\Bndcde changes 20220301\Errata.sql"
