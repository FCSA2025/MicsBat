# Documented File: SuTraf.cs
**Repository Path:** `_DataStructures\SuTraf.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with the subsidiary table <b>main.sd_traf</b> 
    /// except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuTraf
    {
        // IMPORTANT!
        // =========
        // The following qty. 9 member values correspond to the legacy native 
        // structure suTraf_; member names are prescribed to be identical.
        // The order of appearance of these qty. 9 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;    //#0
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;    //#1
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFCODE_SZ)]
        public string trafcode;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ECODE_SZ)]
        public string ecode;    //#3
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREFTRCDE_SZ)]
        public string xreftrcde;    //#4
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = XREFEQCDE_SZ)]
        public string xrefeqcde;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRDESC_SZ)]
        public string trdesc;    //#6
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#8

        //----------------------------------------------------------------
        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 9;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "trafcode", "ecode", "xreftrcde", "xrefeqcde", "trdesc", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int TRAFCODE = 2;
        public const int ECODE = 3;
        public const int XREFTRCDE = 4;
        public const int XREFEQCDE = 5;
        public const int TRDESC = 6;
        public const int MDATE = 7;
        public const int MTIME = 8;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int TRAFCODE_SZ = Constant.TRAFCODE_SZ;
        public const int ECODE_SZ = Constant.ECODE_SZ;
        public const int XREFTRCDE_SZ = Constant.TRAFCODE_SZ;
        public const int XREFEQCDE_SZ = Constant.ECODE_SZ;
        public const int TRDESC_SZ = Constant.TRDESC_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //---------------------------------------------------------------------------------

        //Provide read-only access to these members.
        private static string mAllFieldsAsCSV;
        private static string mSubsetOfFieldsAsCSV;
        //List of all column names with bindings for SQL 'update' command.
        private static string mAllBindingsAsCSV;

        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        public static string SubsetOfColumnsForSqlSelect
        {
            get { return mSubsetOfFieldsAsCSV; }
        }

        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //---------------------------------------------------------------------------------

        // <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SuTraf()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuTraf()
        {
            cmd = "";
            recstat = "";
            trafcode = "";
            ecode = "";
            xreftrcde = "";
            xrefeqcde = "";
            trdesc = "";
            mdate = "";
            mtime = "";
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== SuTraf ===== ");

            sb.Append("\r\ncmd =       " + cmd);
            sb.Append("\r\nrecstat =   " + recstat);
            sb.Append("\r\ntrafcode =  " + trafcode);
            sb.Append("\r\necode =     " + ecode);
            sb.Append("\r\nxreftrcde = " + xreftrcde);
            sb.Append("\r\nxrefeqcde = " + xrefeqcde);
            sb.Append("\r\ntrdesc =    " + trdesc);
            sb.Append("\r\nmdate =     " + mdate);
            sb.Append("\r\nmtime =     " + mtime);

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
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== SuTraf ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "trafcode = " + trafcode);
            sb.Append("\n" + nullInds[i++] + "      " + "ecode = " + ecode);
            sb.Append("\n" + nullInds[i++] + "      " + "xreftrcde = " + xreftrcde);
            sb.Append("\n" + nullInds[i++] + "      " + "xrefeqcde = " + xrefeqcde);
            sb.Append("\n" + nullInds[i++] + "      " + "trdesc = " + trdesc);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

            return sb.ToString();
        }


        /// <summary>
        /// This method returns a 'deep copy' of the current object; changes to the 
        /// field values of this object do not cause changes to the deep copy.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuTraf DeepCopy()
        {
            // A deep copy object has deep copies of the nonstatic fields of the current object.
            //    - If a field is a value type then a bit-by-bit copy of the field is created. 
            //    - If a field is a reference type then a deep copy of the referred object is created.

            SuTraf suTraf = new SuTraf();

            suTraf.cmd = this.cmd;
            suTraf.recstat = this.recstat;
            suTraf.trafcode = this.trafcode;
            suTraf.ecode = this.ecode;
            suTraf.xreftrcde = this.xreftrcde;
            suTraf.xrefeqcde = this.xrefeqcde;
            suTraf.trdesc = this.trdesc;
            suTraf.mdate = this.mdate;
            suTraf.mtime = this.mtime;

            return suTraf;
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

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all member field values except 'cmd' and 'recstat' for use in 
        /// a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfColumnNamesForSQLSelect()
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

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all column names for use in a SQL 'update' query using
        /// bound parameter values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfAllColumnNamesForSQLUpdate()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append(columnNames[i] + "=?");
                if (i != NUM_COLUMNS - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" ");
            return sb.ToString();
        }



    }
}

```
