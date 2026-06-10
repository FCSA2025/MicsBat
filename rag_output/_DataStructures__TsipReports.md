# Documented File: TsipReports.cs
**Repository Path:** `_DataStructures\TsipReports.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    public class TsipReports
    {
        public string date = "";
        public string time = "";
        public string paramFile = "";
        public string runID = "";
        public string reportType = "";
        public int lineNum = -666;
        public string line = "";

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 7;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "date", "time", "paramFile", "runID", "reportType", "lineNum", "line" };

        public const int DATE = 0;
        public const int TIME = 1;
        public const int PARAMFILE = 2;
        public const int RUNID = 3;
        public const int REPORTTYPE = 4;
        public const int LINENUM = 5;
        public const int LINE = 6;

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static TsipReports()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
        }

        //List of all column names for SQL 'select' command.
        private static string mAllFieldsAsCSV;
        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[TsipReports.DATE] == Constant.DB_NULL ? "NULL, " : "'" + date.ToString() + "', ");
            sb.Append(nullInds[TsipReports.TIME] == Constant.DB_NULL ? "NULL, " : "'" + time.ToString() + "', ");
            sb.Append(nullInds[TsipReports.PARAMFILE] == Constant.DB_NULL ? "NULL, " : "'" + paramFile.ToString() + "', ");
            sb.Append(nullInds[TsipReports.RUNID] == Constant.DB_NULL ? "NULL, " : "'" + runID.ToString() + "', ");
            sb.Append(nullInds[TsipReports.REPORTTYPE] == Constant.DB_NULL ? "NULL, " : "'" + reportType.ToString() + "', ");
            sb.Append(nullInds[TsipReports.LINENUM] == Constant.DB_NULL ? "NULL, " : "'" + lineNum.ToString() + "', ");
            sb.Append(nullInds[TsipReports.LINE] == Constant.DB_NULL ? "NULL, " : "'" + line.ToString() + "' ");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all column names for use in a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfAllColumnNamesForSQLSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append(columnNames[i]);
                if (i != NUM_COLUMNS - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" ");
            return sb.ToString();
        }

        public const string CREATE_TABLE = "" +
            "CREATE TABLE {0} ( " +
                "[date] [char](10) NOT NULL, " +
                "[time] [char](5) NOT NULL, " +
                "[paramFile] [varchar](64) NOT NULL, " +
                "[runID] [varchar](8) NOT NULL, " +
                "[reportType] [varchar](64) NOT NULL, " +
                "[lineNum] [int] NOT NULL, " +
                "[line] [varchar](MAX) NOT NULL " +
                    "PRIMARY KEY CLUSTERED " +
                    "( " +
                        "[date] ASC, " +
                        "[time] ASC, " +
                        "[paramFile] ASC, " +
                        "[runID] ASC, " +
                        "[reportType] ASC, " +
                        "[lineNum] ASC " +
                    ")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY] " +
            ") ON [PRIMARY] ";



    }
}

```
