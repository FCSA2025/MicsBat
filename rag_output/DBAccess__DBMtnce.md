# Documented File: DBMtnce.cs
**Repository Path:** `DBAccess\DBMtnce.cs`
**Primary Layer:** `DBAccess`
**Namespace:** `DBAccess`

## Source Code Representation
```csharp
﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Web;
using System.Text;
using System.Data.Odbc;

namespace DBAccess
{
    public class DBMtnce
    {
        public static DataTable GetPermissions()
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "EXEC dbo.GetPermissions";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }

        public static DataTable GetDailyUsage()
        {
            dbconnect oCn = new dbconnect();
            //SQL:VIEW:web.daily_usage_view 
            string cSQL = "SELECT * FROM web.daily_usage_view ORDER BY create_date DESC";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }

        public static DataTable GetMthlyConnect()
        {
            dbconnect oCn = new dbconnect();
            //SQL:TABLE:web.mthly_connect
            string cSQL = "SELECT * FROM web.mthly_connect ORDER BY sessionid";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }

        public static DataTable GetUserTables()
        {
            dbconnect oCn = new dbconnect();
            //SQL:VIEW:web.user_tables_view
            string cSQL = "SELECT * FROM web.user_tables_view ORDER BY tabletype, file_name";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
        public static DataTable GetNonMicsUserTables(string schema)
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = '" +
                        schema + "' AND dbo.micstable(table_name) = 0 ORDER BY table_name";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;
        }
    }
}

```
