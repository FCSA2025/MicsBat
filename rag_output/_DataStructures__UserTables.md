# Documented File: UserTables.cs
**Repository Path:** `_DataStructures\UserTables.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class has members that are isomorphic with the column names of the
    /// DB table web.user_tables.
    /// </summary>
    public class UserTables
    {
        public string oper;     // This member name is an oddball.
                                // The corresponding column name in the table web.user_tables
                                // is named 'operator'.
                                // However, 'operator' is a reserved keyword in the C# language specification.
                                // Consequently, we will name this member 'oper' but remember in the
                                // list of column names that it should be 'operator'.

        public int tabletype;
        public string file_name;
        public string micsid;
        public string project_code;
        public string validstat;
        public ODBC.TIMESTAMP_STRUCT create_date;

        //-------------------------------------------------------------------------

        public const int NUM_COLUMNS = 7;

        public const int OPER = 0;
        public const int TABLETYPE = 1;
        public const int FILE_NAME = 2;
        public const int MICSID = 3;
        public const int PROJECT_CODE = 4;
        public const int VALIDSTAT = 5;
        public const int CREATE_DATE = 6;

        public const int OPER_SZ = Constant.OPERCODE_SZ;
        public const int FILE_NAME_SZ = Constant.DISP_NM_SZ;
        public const int MICSID_SZ = Constant.ID_SZ;
        public const int PROJECT_CODE_SZ = Constant.PCODE_SZ;
        public const int VALIDSTAT_SZ = 2;

        //--------------------------------------------------------------------------

        //Array of strings providing the class-member / database-column names.
        //Note: the order is that of columns in the table main.mt_ante.
        private static string[] columnNames = new string[NUM_COLUMNS] { "operator", "tabletype", "file_name", "micsid", "project_code", "validstat", "create_date" };

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public UserTables()
        {
            oper = "";
            tabletype = Int32.MinValue;
            file_name = "";
            micsid = "";
            project_code = "";
            validstat = "";
            create_date = new ODBC.TIMESTAMP_STRUCT();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\noper         = " + oper);
            sb.Append("\ntabletype    = " + tabletype);
            sb.Append("\nfile_name    = " + file_name);
            sb.Append("\nmicsid       = " + micsid);
            sb.Append("\nproject_code = " + project_code);
            sb.Append("\nvalidstat    = " + validstat);
            sb.Append("\ncreate_date  = " + create_date.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            int n = 0;

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oper         = " + oper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tabletype    = " + tabletype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "file_name    = " + file_name);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "micsid       = " + micsid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "project_code = " + project_code);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "validstat    = " + validstat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "create_date  = " + create_date);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated string for the key fields 'operator',
        /// 'tabletype' and 'file_name'.
        /// </summary>
        /// <returns></returns>
        public string KeysToString()
        {
            return String.Format("oper = {0}; tabletype = {1}; file_name = {2}",
                                    oper, tabletype, file_name);
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[UserTables.OPER] + "=" + (nullInds[UserTables.OPER] == Constant.DB_NULL ? "NULL," : "'" + oper.ToString() + "',"));
            sb.Append(columnNames[UserTables.TABLETYPE] + "=" + (nullInds[UserTables.TABLETYPE] == Constant.DB_NULL ? "NULL," : "'" + tabletype.ToString() + "',"));
            sb.Append(columnNames[UserTables.FILE_NAME] + "=" + (nullInds[UserTables.FILE_NAME] == Constant.DB_NULL ? "NULL," : "'" + file_name.ToString() + "',"));
            sb.Append(columnNames[UserTables.MICSID] + "=" + (nullInds[UserTables.MICSID] == Constant.DB_NULL ? "NULL," : "'" + micsid.ToString() + "',"));
            sb.Append(columnNames[UserTables.PROJECT_CODE] + "=" + (nullInds[UserTables.PROJECT_CODE] == Constant.DB_NULL ? "NULL," : "'" + project_code.ToString() + "',"));
            sb.Append(columnNames[UserTables.VALIDSTAT] + "=" + (nullInds[UserTables.VALIDSTAT] == Constant.DB_NULL ? "NULL," : "'" + validstat.ToString() + "',"));
            sb.Append(columnNames[UserTables.CREATE_DATE] + "=" + (nullInds[UserTables.CREATE_DATE] == Constant.DB_NULL ? "NULL" : "'" + create_date.ToString() + "'"));

            return sb.ToString();
        }


    }
}

```
