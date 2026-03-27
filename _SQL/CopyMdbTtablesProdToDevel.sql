--------------------------------------------------------------------------------
--
-- This T-SQL script copies the contents of the qty. 22 MDB tables in the fcsa 
-- database on the production server (fcsaSQL3.fcsa.main.*) over to the 
-- development server (fcsaSQL5D).
--
--------------------------------------------------------------------------------

-- SSMS knows which SQL Server this script is being run on and is assumed to
-- be the 'target' of the copy.

-- Specify the database to be copied into on this SQL Server.
USE Andrew;

-- Use the following key words to do macro substitutions on the following text, 
-- using Find and Replace.
-- #SOURCE_SCHEMA		e.g. replace by 'fcsaSQL3.fcsa.main'
-- #TARGET_SCHEMA	    e.g. replace by 'main'

TRUNCATE TABLE #TARGET_SCHEMA.control;
INSERT INTO #TARGET_SCHEMA.control SELECT * FROM #SOURCE_SCHEMA.control

TRUNCATE TABLE #TARGET_SCHEMA.me_ante;
INSERT INTO #TARGET_SCHEMA.me_ante SELECT * FROM #SOURCE_SCHEMA.me_ante

TRUNCATE TABLE #TARGET_SCHEMA.me_azim;
INSERT INTO #TARGET_SCHEMA.me_azim SELECT * FROM #SOURCE_SCHEMA.me_azim

TRUNCATE TABLE #TARGET_SCHEMA.me_chan;
INSERT INTO #TARGET_SCHEMA.me_chan SELECT * FROM #SOURCE_SCHEMA.me_chan

TRUNCATE TABLE #TARGET_SCHEMA.me_site;
INSERT INTO #TARGET_SCHEMA.me_site SELECT * FROM #SOURCE_SCHEMA.me_site

TRUNCATE TABLE #TARGET_SCHEMA.mt_ante;
INSERT INTO #TARGET_SCHEMA.mt_ante SELECT * FROM #SOURCE_SCHEMA.mt_ante

TRUNCATE TABLE #TARGET_SCHEMA.mt_chan;
INSERT INTO #TARGET_SCHEMA.mt_chan SELECT * FROM #SOURCE_SCHEMA.mt_chan

TRUNCATE TABLE #TARGET_SCHEMA.mt_site;
INSERT INTO #TARGET_SCHEMA.mt_site SELECT * FROM #SOURCE_SCHEMA.mt_site

TRUNCATE TABLE #TARGET_SCHEMA.sd_antd;
INSERT INTO #TARGET_SCHEMA.sd_antd SELECT * FROM #SOURCE_SCHEMA.sd_antd

TRUNCATE TABLE #TARGET_SCHEMA.sd_ante;
INSERT INTO #TARGET_SCHEMA.sd_ante SELECT * FROM #SOURCE_SCHEMA.sd_ante

TRUNCATE TABLE #TARGET_SCHEMA.sd_band;
INSERT INTO #TARGET_SCHEMA.sd_band SELECT * FROM #SOURCE_SCHEMA.sd_band

TRUNCATE TABLE #TARGET_SCHEMA.sd_ctx;
INSERT INTO #TARGET_SCHEMA.sd_ctx SELECT * FROM #SOURCE_SCHEMA.sd_ctx

TRUNCATE TABLE #TARGET_SCHEMA.sd_ctxd;
INSERT INTO #TARGET_SCHEMA.sd_ctxd SELECT * FROM #SOURCE_SCHEMA.sd_ctxd

TRUNCATE TABLE #TARGET_SCHEMA.sd_eqpt;
INSERT INTO #TARGET_SCHEMA.sd_eqpt SELECT * FROM #SOURCE_SCHEMA.sd_eqpt

TRUNCATE TABLE #TARGET_SCHEMA.sd_note;
INSERT INTO #TARGET_SCHEMA.sd_note SELECT * FROM #SOURCE_SCHEMA.sd_note

TRUNCATE TABLE #TARGET_SCHEMA.sd_oper;
INSERT INTO #TARGET_SCHEMA.sd_oper SELECT * FROM #SOURCE_SCHEMA.sd_oper

TRUNCATE TABLE #TARGET_SCHEMA.sd_plan;
INSERT INTO #TARGET_SCHEMA.sd_plan SELECT * FROM #SOURCE_SCHEMA.sd_plan

TRUNCATE TABLE #TARGET_SCHEMA.sd_plnd;
INSERT INTO #TARGET_SCHEMA.sd_plnd SELECT * FROM #SOURCE_SCHEMA.sd_plnd

TRUNCATE TABLE #TARGET_SCHEMA.sd_rout;
INSERT INTO #TARGET_SCHEMA.sd_rout SELECT * FROM #SOURCE_SCHEMA.sd_rout

TRUNCATE TABLE #TARGET_SCHEMA.sd_town;
INSERT INTO #TARGET_SCHEMA.sd_town SELECT * FROM #SOURCE_SCHEMA.sd_town

TRUNCATE TABLE #TARGET_SCHEMA.sd_towr;
INSERT INTO #TARGET_SCHEMA.sd_towr SELECT * FROM #SOURCE_SCHEMA.sd_towr

TRUNCATE TABLE #TARGET_SCHEMA.sd_traf;
INSERT INTO #TARGET_SCHEMA.sd_traf SELECT * FROM #SOURCE_SCHEMA.sd_traf


