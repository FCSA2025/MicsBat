USE micsdev
GO

SELECT
state_desc + ' ' + permission_name +
' on ['+ ss.name + '].[' + so.name + ']
to [' + sdpr.name + ']'
COLLATE LATIN1_General_CI_AS as [Permissions T-SQL]
FROM SYS.DATABASE_PERMISSIONS AS sdp
JOIN sys.objects AS so
     ON sdp.major_id = so.OBJECT_ID
JOIN SYS.SCHEMAS AS ss
     ON so.SCHEMA_ID = ss.SCHEMA_ID
JOIN SYS.DATABASE_PRINCIPALS AS sdpr
     ON sdp.grantee_principal_id = sdpr.principal_id
where 1=1
  AND so.name = 'ItemStock'

UNION

SELECT
state_desc + ' ' + permission_name +
' on Schema::['+ ss.name + ']
to [' + sdpr.name + ']'
COLLATE LATIN1_General_CI_AS as [Permissions T-SQL]
FROM SYS.DATABASE_PERMISSIONS AS sdp
JOIN SYS.SCHEMAS AS ss
     ON sdp.major_id = ss.SCHEMA_ID
     AND sdp.class_desc = 'Schema'
JOIN SYS.DATABASE_PRINCIPALS AS sdpr
     ON sdp.grantee_principal_id = sdpr.principal_id
where 1=1

order by [Permissions T-SQL]
GO