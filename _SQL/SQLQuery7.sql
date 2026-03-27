SELECT SERVERPROPERTY('Collation')

SELECT '¿¡¬√ƒ≈'

SELECT * FROM fcsa.hulme.ft_problemchars_site

SELECT ASCII(SUBSTRING(name,1,1)) FROM fcsa.hulme.ft_problemchars_site

SELECT CHAR(14)

SELECT * FROM main.sd_note

SELECT * FROM main.mt_site ORDER BY call1

SELECT * FROM main.mt_chan ORDER BY call1

SELECT call1, name FROM fcsa.main.mt_site WHERE prov = 'QC'

SELECT * FROM fcsa.main.mt_site WHERE prov = 'QC'

SELECT * FROM main.mt_site

SELECT hulme.allCharsBelow128('nnn nnnn'), SUBSTRING('o¿', 2, 1), LEN('o¿'), ASCII('ñ')

SELECT call1, oper, name, loc FROM main.mt_site WHERE hulme.allChars32to127(call1) = 0

SELECT oper, nonum, note FROM main.sd_note WHERE hulme.allCharsBelow128(note) = 0

SELECT * FROM hulme.ft_NonASCIIchars_site