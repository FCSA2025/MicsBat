--  replace the name of the antenna su table in this script before executing.
-- only execute to end of CASE statement, then interpolate and execute the last statement

USE FCSA
GO

IF EXISTS (
   SELECT 1 FROM INFORMATION_SCHEMA.TABLES
   WHERE TABLE_NAME = 'chkinterpolate' AND TABLE_SCHEMA = 'dbo'
)
BEGIN
  DROP TABLE dbo.chkinterpolate
  PRINT 'Dropped dbo.chkinterpolate'
END
GO

CREATE TABLE dbo.chkinterpolate (acode char(12) NOT NULL,
antang real NOT NULL, 
dcov real NULL,
dxpv real NULL,
dcoh real NULL,
dxph real NULL,
interpstat INT NOT NULL)
GO

ALTER TABLE dbo.chkinterpolate
ADD PRIMARY KEY (acode,antang)
GO


INSERT INTO dbo.chkinterpolate (acode,antang,dcov, dxpv, dcoh, dxph, interpstat)
SELECT acode,antang,dcov,dxpv,dcoh,dxph,interpstat FROM fmda2.su_da1265interp_antd
GO

select * from dbo.chkinterpolate
GO

UPDATE dbo.chkinterpolate SET interpstat = CASE
	WHEN dcov>=0.0 and dxpv IS NULL and dcoh>=0.0 and dxph>=0.0 THEN 1
	WHEN dcov IS NULL and dxpv>=0.0 and dcoh>=0.0 and dxph>=0.0 THEN 2
	WHEN dcov IS NULL and dxpv IS NULL and dcoh>=0.0 and dxph>=0.0 THEN 3
	WHEN dcov>=0.0 and dxpv>=0.0 and dcoh>=0.0 and dxph IS NULL THEN 4
	WHEN dcov>=0.0 and dxpv IS NULL and dcoh>=0.0 and dxph IS NULL THEN 5
	WHEN dcov IS NULL and dxpv>=0.0 and dcoh>=0.0 and dxph IS NULL THEN 6
	WHEN dcov IS NULL and dxpv IS NULL and dcoh>=0.0 and dxph IS NULL THEN 7
	WHEN dcov>=0.0 and dxpv>=0.0 and dcoh IS NULL and dxph>=0.0 THEN 8
	WHEN dcov>=0.0 and dxpv IS NULL and dcoh IS NULL and dxph>=0.0 THEN 9
	WHEN dcov IS NULL and dxpv>=0.0 and dcoh IS NULL and dxph>=0.0 THEN 10
	WHEN dcov IS NULL and dxpv IS NULL and dcoh IS NULL and dxph>=0.0 THEN 11
	WHEN dcov>=0.0 and dxpv>=0.0 and dcoh IS NULL and dxph IS NULL THEN 12
	WHEN dcov>=0.0 and dxpv IS NULL and dcoh IS NULL and dxph IS NULL THEN 13
	WHEN dcov IS NULL and dxpv>=0.0 and dcoh IS NULL and dxph IS NULL THEN 14
	WHEN dcov IS NULL and dxpv IS NULL and dcoh IS NULL and dxph IS NULL THEN 15
	ELSE 0
END
GO



-- Now interpolate the antenna

-- Compare the interpstat values between the two tables

select * from dbo.chkinterpolate where EXISTS (select acode,antang,interpstat from fmda2.su_da1265interp_antd where
fmda2.su_da1265interp_antd.acode=dbo.chkinterpolate.acode and
fmda2.su_da1265interp_antd.antang=dbo.chkinterpolate.antang and
fmda2.su_da1265interp_antd.interpstat!=dbo.chkinterpolate.interpstat)
GO

-- Execute if you want to see the antang and interpstat rows from the two tables side-by-side

select dbo.chkinterpolate.acode AS myacode,dbo.chkinterpolate.antang AS myantang, dbo.chkinterpolate.interpstat AS myinterpstat,
fmda2.su_da1265interp_antd.antang as fileantang, fmda2.su_da1265interp_antd.interpstat from dbo.chkinterpolate,fmda2.su_da1265interp_antd
where fmda2.su_da1265interp_antd.acode=dbo.chkinterpolate.acode and
fmda2.su_da1265interp_antd.antang=dbo.chkinterpolate.antang
GO

-- Two check that uninterpolate works correctly, run uninterpolate and Save, then go back to editor
-- and interpolate and Save.  Then run the second last statement.  If the uninterpolate worked
-- correctly, you should have the same values in the subsidiary file once again and they should match
-- the values in chkinterpolate.



