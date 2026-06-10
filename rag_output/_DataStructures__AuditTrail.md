# Documented File: AuditTrail.cs
**Repository Path:** `_DataStructures\AuditTrail.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _Configuration;
    using SQLLEN = Int64;

    /// <summary>
    /// The members of this class are isomorphic with the columns of the
    /// table <b>adm.audit_trail</b>.
    /// </summary>
    public class AuditTrail
    {
        public string ultrixid = "";
        public string micsid = "";
        public string tabletype = "";
        public string filename = "";
        public string mdate = "";
        public string mtime = "";

        //----------------------------------------------------------------------

        public const int NUM_COLUMNS = 6;

        public const int ULTRIXID = 0;
        public const int MICSID = 1;
        public const int TABLETYPE = 2;
        public const int FILENAME = 3;
        public const int MDATE = 4;
        public const int MTIME = 5;

        public const int ULTRIXID_SZ = 9;
        public const int MICSID_SZ = 11;
        public const int TABLETYPE_SZ = 5;
        public const int FILENAME_SZ = 17;
        public const int MDATE_SZ = 11;
        public const int MTIME_SZ = 7;

        //----------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public new string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("========== AuditTrail:");
            sb.Append("\nultrixid  = " + ultrixid);
            sb.Append("\nmicsid    = " + micsid);
            sb.Append("\ntabletype = " + tabletype);
            sb.Append("\nfilename  = " + filename);
            sb.Append("\nmdate     = " + mdate);
            sb.Append("\nmtime     = " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// mdateides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"> - input array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int n = 0;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("========== AuditTrail:");
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ultrixid  = " + ultrixid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "micsid    = " + micsid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tabletype = " + tabletype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "filename  = " + filename);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate     = " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime     = " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[AuditTrail.ULTRIXID] == Constant.DB_NULL ? "NULL," : "'" + ultrixid.ToString() + "',");
            sb.Append(nullInds[AuditTrail.MICSID] == Constant.DB_NULL ? "NULL," : "'" + micsid.ToString() + "',");
            sb.Append(nullInds[AuditTrail.TABLETYPE] == Constant.DB_NULL ? "NULL," : "'" + tabletype.ToString() + "',");
            sb.Append(nullInds[AuditTrail.FILENAME] == Constant.DB_NULL ? "NULL," : "'" + filename.ToString() + "',");
            sb.Append(nullInds[AuditTrail.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[AuditTrail.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "'");

            return sb.ToString();
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[audit_trail] " +
"(" +
"[ultrixid] [char](8) NULL," +
"[micsid] [char](10) NULL," +
"[tabletype] [char](4) NULL," +
"[file_name] [char](16) NOT NULL," +
"[mdate] [datetime] NOT NULL," +
"[mtime] [char](8) NULL" +
") ON[PRIMARY]";

        /*
        ultrixid
        micsid
        tabletype
        filename
        mdate
        mtime
        */

    }
}

```
