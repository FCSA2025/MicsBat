# Documented File: SuPlnd.cs
**Repository Path:** `_DataStructures\SuPlnd.cs`
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
    /// This class has fields that are isomorphic with the 'subupt' table <b>main.sd_plnd</b> 
    /// except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuPlnd
    {
        // IMPORTANT!
        // =========
        // The following qty. 15 member values correspond to the legacy native 
        // structure suPlnd_; member names are prescribed to be identical.
        // The order of appearance of these qty. 15 field members MUST be
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
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SBAND_SZ)]
        public string sband;    //#2
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SPLAN_SZ)]
        public string splan;    //#3
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short spno;    //#4
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set1;    //#5
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S1CHID_SZ)]
        public string s1chid;    //#6
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set2;    //#7
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S2CHID_SZ)]
        public string s2chid;    //#8
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set3;    //#9
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S3CHID_SZ)]
        public string s3chid;    //#10
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double set4;    //#11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = S4CHID_SZ)]
        public string s4chid;    //#12
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;    //#13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;    //#14

        //------------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 15;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "sband", "splan", "spno", "set1", "s1chid", "set2", "s2chid", "set3", "s3chid", "set4", "s4chid", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int SBAND = 2;
        public const int SPLAN = 3;
        public const int SPNO = 4;
        public const int SET1 = 5;
        public const int S1CHID = 6;
        public const int SET2 = 7;
        public const int S2CHID = 8;
        public const int SET3 = 9;
        public const int S3CHID = 10;
        public const int SET4 = 11;
        public const int S4CHID = 12;
        public const int MDATE = 13;
        public const int MTIME = 14;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int SBAND_SZ = Constant.BNDCDE_SZ;
        public const int SPLAN_SZ = Constant.PLAN_SZ;
        public const int S1CHID_SZ = Constant.CHID_SZ;
        public const int S2CHID_SZ = Constant.CHID_SZ;
        public const int S3CHID_SZ = Constant.CHID_SZ;
        public const int S4CHID_SZ = Constant.CHID_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //------------------------------------------------------------------------------

        //Provide read-only access to these members.
        private static string mAllFieldsAsCSV;
        private static string mSubsetOfFieldsAsCSV;
        //List of all column names with bindings for SQL 'update' command.
        private static string mAllBindingsAsCSV;

        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //-------------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SuPlnd()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SuPlnd()
        {
            cmd = "";
            recstat = "";
            sband = "";
            splan = "";
            spno = 0;
            set1 = 0.0;
            s1chid = "";
            set2 = 0.0;
            s2chid = "";
            set3 = 0.0;
            s3chid = "";
            set4 = 0.0;
            s4chid = "";
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

            sb.Append("\r\ncmd =     " + cmd);
            sb.Append("\r\nrecstat = " + recstat);
            sb.Append("\r\nsband =   " + sband);
            sb.Append("\r\nsplan =   " + splan);
            sb.Append("\r\nspno =    " + spno);
            sb.Append("\r\nset1 =    " + set1);
            sb.Append("\r\ns1chid =  " + s1chid);
            sb.Append("\r\nset2 =    " + set2);
            sb.Append("\r\ns2chid =  " + s2chid);
            sb.Append("\r\nset3 =    " + set3);
            sb.Append("\r\ns3chid =  " + s3chid);
            sb.Append("\r\nset4 =    " + set4);
            sb.Append("\r\ns4chid =  " + s4chid);
            sb.Append("\r\nmdate =   " + mdate);
            sb.Append("\r\nmtime =   " + mtime);

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
            sb.Append("\n ===== SuPlnd ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "sband = " + sband);
            sb.Append("\n" + nullInds[i++] + "      " + "splan = " + splan);
            sb.Append("\n" + nullInds[i++] + "      " + "spno = " + spno);
            sb.Append("\n" + nullInds[i++] + "      " + "set1 = " + set1);
            sb.Append("\n" + nullInds[i++] + "      " + "s1chid = " + s1chid);
            sb.Append("\n" + nullInds[i++] + "      " + "set2 = " + set2);
            sb.Append("\n" + nullInds[i++] + "      " + "s2chid = " + s2chid);
            sb.Append("\n" + nullInds[i++] + "      " + "set3 = " + set3);
            sb.Append("\n" + nullInds[i++] + "      " + "s3chid = " + s3chid);
            sb.Append("\n" + nullInds[i++] + "      " + "set4 = " + set4);
            sb.Append("\n" + nullInds[i++] + "      " + "s4chid = " + s4chid);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

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
