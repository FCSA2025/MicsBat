
DECLARE @pdf CHAR(9) = '%TLUSBCX1'
SELECT 'sadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'CGA276A'
SELECT 'sadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'XOA699A'
SELECT 'sadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '$INLB01X'
SELECT 'owsadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '%INLB02X'
SELECT 'owsadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'XOD250A'
SELECT 'owsadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '%TLUSBCX8'
SELECT 'dbleadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '%TLUSBCX9'
SELECT 'dbleadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'CGF981A'
SELECT 'dbleadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'CGF983A'
SELECT 'dbleadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '%DUQU04XA'
SELECT 'owdadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '%DUQU15XA'
SELECT 'owdadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'KZN80XA'
SELECT 'owdadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = 'KZN95XA'
SELECT 'owdadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '=TLUSMCR2'
SELECT 'regadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 

SET @pdf = '=TLUSMCZ2'
SELECT 'regadd.txt', @pdf, COUNT(call1) FROM main.mt_site WHERE call1 = @pdf 






