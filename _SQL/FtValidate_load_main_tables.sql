USE [Regression]
GO

/****** Object:  StoredProcedure [dbo].[ftvalidate_load_main_tables]    Script Date: 1/11/2021 5:40:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[ftvalidate_load_main_tables]
as 
begin

-- Delete any extant MDB and/or SDB tables.
IF OBJECT_ID('main.mt_ante') IS NOT NULL DROP TABLE regression.main.mt_ante
IF OBJECT_ID('main.mt_chan') IS NOT NULL DROP TABLE regression.main.mt_chan
IF OBJECT_ID('main.mt_site') IS NOT NULL DROP TABLE regression.main.mt_site
IF OBJECT_ID('main.sd_antd') IS NOT NULL DROP TABLE regression.main.sd_antd
IF OBJECT_ID('main.sd_ante') IS NOT NULL DROP TABLE regression.main.sd_ante
IF OBJECT_ID('main.sd_band') IS NOT NULL DROP TABLE regression.main.sd_band
IF OBJECT_ID('main.sd_eqpt') IS NOT NULL DROP TABLE regression.main.sd_eqpt
IF OBJECT_ID('main.sd_note') IS NOT NULL DROP TABLE regression.main.sd_note
IF OBJECT_ID('main.sd_oper') IS NOT NULL DROP TABLE regression.main.sd_oper
IF OBJECT_ID('main.sd_plan') IS NOT NULL DROP TABLE regression.main.sd_plan
IF OBJECT_ID('main.sd_plnd') IS NOT NULL DROP TABLE regression.main.sd_plnd
IF OBJECT_ID('main.sd_rout') IS NOT NULL DROP TABLE regression.main.sd_rout
IF OBJECT_ID('main.sd_town') IS NOT NULL DROP TABLE regression.main.sd_town
IF OBJECT_ID('main.sd_traf') IS NOT NULL DROP TABLE regression.main.sd_traf
IF OBJECT_ID('techdef.fee_codes') IS NOT NULL DROP TABLE regression.techdef.fee_codes

-- Create the main tables needed for regression testing.
-- The code below is somewhat devious. It creates the 'into' table
-- with the identical columns as the 'from' table; however, the 'WHERE 0 = 1'
-- term always evaluates to false and so no data is actually written to
-- the 'into' table.
SELECT * INTO regression.main.mt_ante FROM micsdev.main.mt_ante WHERE 0=1
SELECT * INTO regression.main.mt_chan FROM micsdev.main.mt_chan WHERE 0=1
SELECT * INTO regression.main.mt_site FROM micsdev.main.mt_site WHERE 0=1
SELECT * INTO regression.main.sd_antd FROM micsdev.main.sd_antd WHERE 0=1
SELECT * INTO regression.main.sd_ante FROM micsdev.main.sd_ante WHERE 0=1
SELECT * INTO regression.main.sd_band FROM micsdev.main.sd_band WHERE 0=1
SELECT * INTO regression.main.sd_eqpt FROM micsdev.main.sd_eqpt WHERE 0=1
SELECT * INTO regression.main.sd_note FROM micsdev.main.sd_note WHERE 0=1
SELECT * INTO regression.main.sd_oper FROM micsdev.main.sd_oper WHERE 0=1
SELECT * INTO regression.main.sd_plan FROM micsdev.main.sd_plan WHERE 0=1
SELECT * INTO regression.main.sd_plnd FROM micsdev.main.sd_plnd WHERE 0=1
SELECT * INTO regression.main.sd_rout FROM micsdev.main.sd_rout WHERE 0=1
SELECT * INTO regression.main.sd_town FROM micsdev.main.sd_town WHERE 0=1
SELECT * INTO regression.main.sd_traf FROM micsdev.main.sd_traf WHERE 0=1
SELECT * INTO regression.techdef.fee_codes FROM micsdev.techdef.fee_codes WHERE 0=1

-- Handle the exceptional case of regression.mt_site.
-- The field FtValidate.SiteCoords is computed.
-- However the SELECT * INTO FROM WHERE query will not create regression.SiteCoords
-- as a computed field - just as a standard 'value' field.
-- To remedy this we must drop then recreate the regression.SiteCoords as the
-- computed field that we require.
ALTER TABLE main.mt_site DROP COLUMN SiteCoords
ALTER TABLE main.mt_site ADD SiteCoords AS (([strlatit]+'-')+[strlongit])

-- Populate the main working tables.
-- Again, we have to handle the exceptional case of main.mt_site - we can't use '*' 
-- because that would attempt to insert into the computed field which would cause an error.
-- Instead we have to explicitely select all of the columns except the computed field (SiteCoords).
INSERT INTO regression.main.mt_ante SELECT * FROM regression.ftvalidate.mt_ante
INSERT INTO regression.main.mt_chan SELECT * FROM regression.ftvalidate.mt_chan

INSERT INTO regression.main.mt_site SELECT call1,name,prov,oper,latit,strlatit,strlatits,longit,strlongit,strlongits,grnd,stats,sdate,loc,icaccount,reg,spoint,nots,oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4,bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime,userid 
                                    FROM regression.ftvalidate.mt_site

INSERT INTO regression.main.sd_antd SELECT * FROM regression.ftvalidate.sd_antd
INSERT INTO regression.main.sd_ante SELECT * FROM regression.ftvalidate.sd_ante
INSERT INTO regression.main.sd_band SELECT * FROM regression.ftvalidate.sd_band
INSERT INTO regression.main.sd_eqpt SELECT * FROM regression.ftvalidate.sd_eqpt
INSERT INTO regression.main.sd_note SELECT * FROM regression.ftvalidate.sd_note
INSERT INTO regression.main.sd_oper SELECT * FROM regression.ftvalidate.sd_oper
INSERT INTO regression.main.sd_plan SELECT * FROM regression.ftvalidate.sd_plan
INSERT INTO regression.main.sd_plnd SELECT * FROM regression.ftvalidate.sd_plnd
INSERT INTO regression.main.sd_rout SELECT * FROM regression.ftvalidate.sd_rout
INSERT INTO regression.main.sd_town SELECT * FROM regression.ftvalidate.sd_town
INSERT INTO regression.main.sd_traf SELECT * FROM regression.ftvalidate.sd_traf
INSERT INTO regression.techdef.fee_codes SELECT * FROM regression.ftvalidate.fee_codes

end;

GO

