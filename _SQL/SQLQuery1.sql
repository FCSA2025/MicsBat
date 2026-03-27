SELECT COUNT(*) FROM micsdev.main.sd_traf

--The actual query
SELECT state_desc
    ,permission_name
    ,'ON'
    ,class_desc
    ,SCHEMA_NAME(major_id)
    ,'TO'
    ,USER_NAME(grantee_principal_id)
FROM sys.database_permissions AS PERM
JOIN sys.database_principals AS Prin
    ON PERM.major_ID = Prin.principal_id
        AND class_desc = 'SCHEMA'
WHERE major_id = SCHEMA_ID('web')
    AND grantee_principal_id = user_id('ROLwebusers')
    --AND    permission_name = 'SELECT'
GO

--cleanup
USE tempdb
GO

DROP DATABASE listschema

--The actual query
SELECT state_desc
    ,permission_name
    ,'ON'
    ,class_desc
    ,SCHEMA_NAME(major_id)
    ,'TO'
    ,USER_NAME(grantee_principal_id)
FROM sys.database_permissions AS PERM
JOIN sys.database_principals AS Prin
    ON PERM.major_ID = Prin.principal_id
        AND class_desc = 'SCHEMA'
WHERE major_id = SCHEMA_ID('web')
    AND grantee_principal_id = user_id('ROLwebusers')
    AND    permission_name = 'UPDATE'   -- 'SELECT' or 'UPDATE'
GO