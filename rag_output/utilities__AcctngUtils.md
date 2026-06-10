# Documented File: AcctngUtils.cs
**Repository Path:** `utilities\AcctngUtils.cs`
**Primary Layer:** `utilities`
**Namespace:** `AcctngUtilities`

## Source Code Representation
```csharp
﻿using System;
using System.Data.Odbc;
using System.Diagnostics;

namespace AcctngUtilities
{
    public class AcctngUtils
    {
        public static int log_billing1(Process inProc, string cn_str, String company, String user, String project, String module, String info)
        {
            // this version uses odbc connections and call is not part of a transaction
            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception)
            {
                return 1;  // could not open connection
            }
            // create sql to insert billing info  

            TimeSpan elapsed;
            try
            {
                DateTime ExitTime = DateTime.Now;
                elapsed = ExitTime.Subtract(inProc.StartTime);
            }
            catch (Exception)
            {
                return 2;   // could not calculate elapsed time
            }
            //SQL:VIEW:web.daily_usage_view 
            string strSql = "insert into web.daily_usage_view values ('" +
                company + "','" + user + "',GetDate(),'" + project + "','" + module + "'," +
                elapsed.TotalSeconds + "," +
                inProc.UserProcessorTime.TotalMilliseconds + "," +
                inProc.TotalProcessorTime.TotalMilliseconds + ",'" + info + "')";
            //sw.WriteLine(strSql);
            //sw.Flush();

            // insert mics_billing
            OdbcCommand insert1 = new OdbcCommand(strSql, cn);

            try
            {
                insert1.ExecuteNonQuery();
                return 0;
            }
            catch (Exception)
            {
                return 3;   // could not insert billing record
            }

        }
        public static int log_billing2(Process inProc, OdbcConnection cn, OdbcTransaction otr, String company, String user, String project, String module, String info)
        {
            // this version uses odbc connections and call IS part of a transaction

            // get elapsed time for process
            TimeSpan elapsed;
            try
            {
                DateTime ExitTime = DateTime.Now;
                elapsed = ExitTime.Subtract(inProc.StartTime);
            }
            catch (Exception)
            {
                return 2;
            }

            // create sql to insert billing info  
            //SQL:VIEW:web.daily_usage_view 
            string strSql = "insert into web.daily_usage_view values ('" +
                company + "','" + user + "',GetDate(),'" + project + "','" + module + "'," +
                elapsed.TotalSeconds + "," +
                inProc.UserProcessorTime.TotalMilliseconds + "," +
                inProc.TotalProcessorTime.TotalMilliseconds + ",'" + info + "')";

            // insert mics_billing
            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
                return 0;
            }
            catch (Exception)
            {
                return 3;
            }

        }

    }
}

```
