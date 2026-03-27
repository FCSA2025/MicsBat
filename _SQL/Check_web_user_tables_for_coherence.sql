-- This script relates only to the identification of 'phantom' TS tables sets (i.e ft_)
-- A 'phantom' entry is a record in web.user_tables that has no matching ft_ tables,
-- i.e. ft_XXX_titl, ft_XXX_site, ft_XXX_ante, ft_XXX_chan, ft_XXX_chng, ft_XXX_shrl
--


-- Create a temporary table used to collate 'does this table actually exist?' information.
-- Refer to this as the COLLATION TABLE.
IF OBJECT_ID('tempdb..#user_tables_type_0') IS NOT NULL DROP TABLE #user_tables_type_0
SELECT operator, file_name, 0 AS titl, 0 AS site, 0 AS ante, 0 AS chan, 0 AS chng, 0 AS shrl, 0 AS phantom
	INTO #user_tables_type_0
	FROM micsdev.web.user_tables 
	WHERE tabletype = 0 
	ORDER BY operator, file_name

--SELECT * FROM #user_tables_type_0 ORDER BY operator, file_name 

-- TITL
-- Create a working table that collates all the user tables of this type.
IF OBJECT_ID('tempdb..#exist_titl') IS NOT NULL DROP TABLE #exist_titl
SELECT TABLE_SCHEMA, TABLE_NAME 
	INTO #exist_titl
	FROM micsdev.INFORMATION_SCHEMA.TABLES  
	WHERE TABLE_NAME LIKE 'ft_%_titl'

-- Update the Collation Table to indicate that a user table with the prescribed name 
-- and type has been found in the gamut of user tables on the SQL Server.
UPDATE #user_tables_type_0
	SET titl = 1
	FROM #user_tables_type_0 AS S
	INNER JOIN #exist_titl AS A ON (A.TABLE_SCHEMA = S.operator) AND (A.TABLE_NAME = CONCAT('ft_', file_name, '_titl'))

-- SITE
-- Create a working table that collates all the user tables of this type.
IF OBJECT_ID('tempdb..#exist_site') IS NOT NULL DROP TABLE #exist_site
SELECT TABLE_SCHEMA, TABLE_NAME 
	INTO #exist_site
	FROM micsdev.INFORMATION_SCHEMA.TABLES  
	WHERE TABLE_NAME LIKE 'ft_%_site'

-- Update the Collation Table to indicate that a user table with the prescribed name 
-- and type has been found in the gamut of user tables on the SQL Server.
UPDATE #user_tables_type_0
	SET site = 1
	FROM #user_tables_type_0 AS S
	INNER JOIN #exist_site AS A ON (A.TABLE_SCHEMA = S.operator) AND (A.TABLE_NAME = CONCAT('ft_', file_name, '_site'))

-- ANTE
-- Create a working table that collates all the user tables of this type.
IF OBJECT_ID('tempdb..#exist_ante') IS NOT NULL DROP TABLE #exist_ante
SELECT TABLE_SCHEMA, TABLE_NAME 
	INTO #exist_ante
	FROM micsdev.INFORMATION_SCHEMA.TABLES  
	WHERE TABLE_NAME LIKE 'ft_%_ante'

-- Update the Collation Table to indicate that a user table with the prescribed name 
-- and type has been found in the gamut of user tables on the SQL Server.
UPDATE #user_tables_type_0
	SET ante = 1
	FROM #user_tables_type_0 AS S
	INNER JOIN #exist_ante AS A ON (A.TABLE_SCHEMA = S.operator) AND (A.TABLE_NAME = CONCAT('ft_', file_name, '_ante'))

-- CHAN
-- Create a working table that collates all the user tables of this type.
IF OBJECT_ID('tempdb..#exist_chan') IS NOT NULL DROP TABLE #exist_chan
SELECT TABLE_SCHEMA, TABLE_NAME 
	INTO #exist_chan
	FROM micsdev.INFORMATION_SCHEMA.TABLES  
	WHERE TABLE_NAME LIKE 'ft_%_chan'

-- Update the Collation Table to indicate that a user table with the prescribed name 
-- and type has been found in the gamut of user tables on the SQL Server.
UPDATE #user_tables_type_0
	SET chan = 1
	FROM #user_tables_type_0 AS S
	INNER JOIN #exist_chan AS A ON (A.TABLE_SCHEMA = S.operator) AND (A.TABLE_NAME = CONCAT('ft_', file_name, '_chan'))

-- CHNG
-- Create a working table that collates all the user tables of this type.
IF OBJECT_ID('tempdb..#exist_chng') IS NOT NULL DROP TABLE #exist_chng
SELECT TABLE_SCHEMA, TABLE_NAME 
	INTO #exist_chng
	FROM micsdev.INFORMATION_SCHEMA.TABLES  
	WHERE TABLE_NAME LIKE 'ft_%_chng'

-- Update the Collation Table to indicate that a user table with the prescribed name 
-- and type has been found in the gamut of user tables on the SQL Server.
UPDATE #user_tables_type_0
	SET chng = 1
	FROM #user_tables_type_0 AS S
	INNER JOIN #exist_chng AS A ON (A.TABLE_SCHEMA = S.operator) AND (A.TABLE_NAME = CONCAT('ft_', file_name, '_chng'))

-- SHRL
-- Create a working table that collates all the user tables of this type.
IF OBJECT_ID('tempdb..#exist_shrl') IS NOT NULL DROP TABLE #exist_shrl
SELECT TABLE_SCHEMA, TABLE_NAME 
	INTO #exist_shrl
	FROM micsdev.INFORMATION_SCHEMA.TABLES  
	WHERE TABLE_NAME LIKE 'ft_%_shrl'

-- Update the Collation Table to indicate that a user table with the prescribed name 
-- and type has been found in the gamut of user tables on the SQL Server.
UPDATE #user_tables_type_0
	SET shrl = 1
	FROM #user_tables_type_0 AS S
	INNER JOIN #exist_shrl AS A ON (A.TABLE_SCHEMA = S.operator) AND (A.TABLE_NAME = CONCAT('ft_', file_name, '_shrl'))
--
--Identify the user_table records that are PHANTOMS.
UPDATE #user_tables_type_0
	SET phantom = 1
	FROM #user_tables_type_0
	WHERE (titl = 0) OR (site = 0) OR (ante = 0) OR (chan = 0) OR (chng = 0) OR (shrl = 0)     

-- Have a look at the raw results.
SELECT * FROM #user_tables_type_0 ORDER BY operator, file_name

--
-- Start of the roll-up analysis.
IF OBJECT_ID('tempdb..#user_tables_tally') IS NOT NULL DROP TABLE #user_tables_tally
SELECT DISTINCT operator, NULL AS totalTableSets, NULL AS phantomTableSets
	INTO #user_tables_tally
	FROM #user_tables_type_0

-- Tally up all of the ft_ records in web.user_tables.
UPDATE #user_tables_tally
	SET totalTableSets = (SELECT COUNT(operator) FROM #user_tables_type_0 AS X WHERE X.operator = Y.operator GROUP BY operator)
	FROM #user_tables_tally AS Y

-- Tally up all of the ft_ records in web.user_tables that are marked as being PHANTOMS.
UPDATE #user_tables_tally
	SET phantomTableSets = (SELECT COUNT(operator) FROM #user_tables_type_0 AS X WHERE (X.operator = Y.operator) AND X.phantom = 1 GROUP BY operator)
	FROM #user_tables_tally AS Y

-- Deal with the null entries.
UPDATE #user_tables_tally
	SET phantomTableSets = 0
	FROM #user_tables_tally
	WHERE phantomTableSets IS NULL

-- Inspect the rolled-up results per operator.
SELECT * FROM #user_tables_tally ORDER BY operator