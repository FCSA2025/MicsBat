USE [micsdev]

SET ANSI_NULLS ON          -- When ANSI_NULLS is ON, all comparisons against a null value evaluate to UNKNOWN. 
                           -- When SET ANSI_NULLS is OFF, comparisons of all data against a null value evaluate 
						   -- to TRUE if the data value is NULL. 
						   -- If SET ANSI_NULLS is not specified, the setting of the ANSI_NULLS option of the 
						   -- current database applies.
SET QUOTED_IDENTIFIER ON   -- SET QUOTED_IDENTIFIER must be ON when you are creating or changing indexes on 
                           -- computed columns or indexed views. 
                           -- If SET QUOTED_IDENTIFIER is OFF, then CREATE, UPDATE, INSERT, and DELETE statements 
						   -- will fail on tables with indexes on computed columns, or tables with indexed views.
SET ANSI_PADDING ON        -- ANSI_PADDING must be ON when you are creating or changing indexes on computed columns 
                           -- or indexed views.
GO                         -- This is a signal to SQL Server to process the current batch of Transact-SQL statements. 
                           -- The current batch of statements is composed of all statements entered since the last GO 
                           -- or since the start of the ad hoc session or script if this is the first GO.

----------------------------------------------------------------------------------------
-- LinkMatch_MdbToTafl
----------------------------------------------------------------------------------------
-- 20230831
IF OBJECT_ID('hulme.LinkMatch_MdbToTafl_20230831') IS NOT NULL DROP TABLE hulme.LinkMatch_MdbToTafl_20230831

-- Create a table variable of the required user-defined table type.
DECLARE @myTable AS hulme.LinkMatchTableType

--Create a new permanent/physical LAML working table by selecting into from the table variable @myTable.
SELECT *
	INTO hulme.LinkMatch_MdbToTafl_20230831
	FROM @myTable
	WHERE 1 = 2

-- Populate the working table with member's TX-end MDB link data.
INSERT INTO hulme.LinkMatch_MdbToTafl_20230831 
	SELECT *
	FROM hulme.LinkMatch_MdbToTafl

--SELECT * FROM hulme.LinkMatch_MdbToTafl_20230831

----------------------------------------------------------------------------------------
-- LinkMatch_TaflToMdb
----------------------------------------------------------------------------------------
-- 20230831
IF OBJECT_ID('hulme.LinkMatch_TaflToMdb_20230831') IS NOT NULL DROP TABLE hulme.LinkMatch_TaflToMdb_20230831

-- Create a table variable of the required user-defined table type.
--DECLARE @myTable AS hulme.LinkMatchTableType

--Create a new permanent/physical LAML working table by selecting into from the table variable @myTable.
SELECT *
	INTO hulme.LinkMatch_TaflToMdb_20230831
	FROM @myTable
	WHERE 1 = 2

-- Populate the working table with member's TX-end MDB link data.
INSERT INTO hulme.LinkMatch_TaflToMdb_20230831 
	SELECT *
	FROM hulme.LinkMatch_TaflToMdb

--SELECT * FROM hulme.LinkMatch_TaflToMdb_20230831