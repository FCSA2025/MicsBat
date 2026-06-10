# Documented File: SuOper.cs
**Repository Path:** `_DataStructures\SuOper.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _Configuration;

namespace _DataStructures
{
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with the SDB table <b>main.sd_oper</b> 
    /// except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuOper
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPER_SZ)]
        public string oper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NAMEOP_SZ)]
        public string nameop;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = COOPER_SZ)]
        public string cooper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDBM_SZ)]
        public string mdbm;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ADDR_SZ)]
        public string addr;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CITY_SZ)]
        public string city;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PRSTAT_SZ)]
        public string prstat;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ZIPPC_SZ)]
        public string zippc;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = DEPT_SZ)]
        public string dept;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NAMEP_SZ)]
        public string namep;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PHONEP_SZ)]
        public string phonep;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = FAXNUM_SZ)]
        public string faxnum;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TELECOM_SZ)]
        public string telecom;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPNOTE_SZ)]
        public string opnote;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ADMIN_SZ)]
        public string admin;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EMAIL_SZ)]
        public string email;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;


        //-----------------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 20;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "oper", "nameop", "cooper", "mdbm", "addr", "city", "prstat", "zippc", "dept", "namep", "phonep", "faxnum", "telecom", "opnote", "admin", "email", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int OPER = 2;
        public const int NAMEOP = 3;
        public const int COOPER = 4;
        public const int MDBM = 5;
        public const int ADDR = 6;
        public const int CITY = 7;
        public const int PRSTAT = 8;
        public const int ZIPPC = 9;
        public const int DEPT = 10;
        public const int NAMEP = 11;
        public const int PHONEP = 12;
        public const int FAXNUM = 13;
        public const int TELECOM = 14;
        public const int OPNOTE = 15;
        public const int ADMIN = 16;
        public const int EMAIL = 17;
        public const int MDATE = 18;
        public const int MTIME = 19;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int OPER_SZ = Constant.OPER_SZ;
        public const int NAMEOP_SZ = Constant.NAMEOP_SZ;
        public const int COOPER_SZ = Constant.OPERCODE_SZ;
        public const int MDBM_SZ = Constant.OPERCODE_SZ;
        public const int ADDR_SZ = Constant.ADDR_SZ;
        public const int CITY_SZ = Constant.CITY_SZ;
        public const int PRSTAT_SZ = Constant.PRSTAT_SZ;
        public const int ZIPPC_SZ = Constant.ZIPPC_SZ;
        public const int DEPT_SZ = Constant.DEPT_SZ;
        public const int NAMEP_SZ = Constant.NAMEP_SZ;
        public const int PHONEP_SZ = Constant.PHONEP_SZ;
        public const int FAXNUM_SZ = Constant.FAXNUM_SZ;
        public const int TELECOM_SZ = Constant.TELECOM_SZ;
        public const int OPNOTE_SZ = Constant.OPNOTE_SZ;
        public const int ADMIN_SZ = Constant.ADMIN_SZ;
        public const int EMAIL_SZ = Constant.EMAIL_SIZE_;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //-----------------------------------------------------------------------------------

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

        //-------------------------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SuOper()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuOper()
        {
            cmd = "";
            recstat = "";
            oper = "";
            nameop = "";
            cooper = "";
            mdbm = "";
            addr = "";
            city = "";
            prstat = "";
            zippc = "";
            dept = "";
            namep = "";
            phonep = "";
            faxnum = "";
            telecom = "";
            opnote = "";
            admin = "";
            email = "";
            mdate = "";
            mtime = "";
        }

        //-----------------------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.AppendLine("cmd =     " + cmd);
            sb.AppendLine("recstat = " + recstat);
            sb.AppendLine("oper =    " + oper);
            sb.AppendLine("nameop =  " + nameop);
            sb.AppendLine("cooper =  " + cooper);
            sb.AppendLine("mdbm =    " + mdbm);
            sb.AppendLine("addr =    " + addr);
            sb.AppendLine("city =    " + city);
            sb.AppendLine("prstat =  " + prstat);
            sb.AppendLine("zippc =   " + zippc);
            sb.AppendLine("dept =    " + dept);
            sb.AppendLine("namep =   " + namep);
            sb.AppendLine("phonep =  " + phonep);
            sb.AppendLine("faxnum =  " + faxnum);
            sb.AppendLine("telecom = " + telecom);
            sb.AppendLine("opnote =  " + opnote);
            sb.AppendLine("admin =   " + admin);
            sb.AppendLine("email =   " + email);
            sb.AppendLine("mdate =   " + mdate);
            sb.AppendLine("mtime =   " + mtime);

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
            sb.Append("\n ===== SuOper ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "oper = " + oper);
            sb.Append("\n" + nullInds[i++] + "      " + "nameop = " + nameop);
            sb.Append("\n" + nullInds[i++] + "      " + "cooper = " + cooper);
            sb.Append("\n" + nullInds[i++] + "      " + "mdbm = " + mdbm);
            sb.Append("\n" + nullInds[i++] + "      " + "addr = " + addr);
            sb.Append("\n" + nullInds[i++] + "      " + "city = " + city);
            sb.Append("\n" + nullInds[i++] + "      " + "prstat = " + prstat);
            sb.Append("\n" + nullInds[i++] + "      " + "zippc = " + zippc);
            sb.Append("\n" + nullInds[i++] + "      " + "dept = " + dept);
            sb.Append("\n" + nullInds[i++] + "      " + "namep = " + namep);
            sb.Append("\n" + nullInds[i++] + "      " + "phonep = " + phonep);
            sb.Append("\n" + nullInds[i++] + "      " + "faxnum = " + faxnum);
            sb.Append("\n" + nullInds[i++] + "      " + "telecom = " + telecom);
            sb.Append("\n" + nullInds[i++] + "      " + "opnote = " + opnote);
            sb.Append("\n" + nullInds[i++] + "      " + "admin = " + admin);
            sb.Append("\n" + nullInds[i++] + "      " + "email = " + email);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SuOper.CMD] == Constant.DB_NULL ? "NULL, " : "'" + cmd.ToString() + "', ");
            sb.Append(nullInds[SuOper.RECSTAT] == Constant.DB_NULL ? "NULL, " : "'" + recstat.ToString() + "', ");
            sb.Append(nullInds[SuOper.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', ");
            sb.Append(nullInds[SuOper.NAMEOP] == Constant.DB_NULL ? "NULL, " : "'" + nameop.ToString() + "', ");
            sb.Append(nullInds[SuOper.COOPER] == Constant.DB_NULL ? "NULL, " : "'" + cooper.ToString() + "', ");
            sb.Append(nullInds[SuOper.MDBM] == Constant.DB_NULL ? "NULL, " : "'" + mdbm.ToString() + "', ");
            sb.Append(nullInds[SuOper.ADDR] == Constant.DB_NULL ? "NULL, " : "'" + addr.ToString() + "', ");
            sb.Append(nullInds[SuOper.CITY] == Constant.DB_NULL ? "NULL, " : "'" + city.ToString() + "', ");
            sb.Append(nullInds[SuOper.PRSTAT] == Constant.DB_NULL ? "NULL, " : "'" + prstat.ToString() + "', ");
            sb.Append(nullInds[SuOper.ZIPPC] == Constant.DB_NULL ? "NULL, " : "'" + zippc.ToString() + "', ");
            sb.Append(nullInds[SuOper.DEPT] == Constant.DB_NULL ? "NULL, " : "'" + dept.ToString() + "', ");
            sb.Append(nullInds[SuOper.NAMEP] == Constant.DB_NULL ? "NULL, " : "'" + namep.ToString() + "', ");
            sb.Append(nullInds[SuOper.PHONEP] == Constant.DB_NULL ? "NULL, " : "'" + phonep.ToString() + "', ");
            sb.Append(nullInds[SuOper.FAXNUM] == Constant.DB_NULL ? "NULL, " : "'" + faxnum.ToString() + "', ");
            sb.Append(nullInds[SuOper.TELECOM] == Constant.DB_NULL ? "NULL, " : "'" + telecom.ToString() + "', ");
            sb.Append(nullInds[SuOper.OPNOTE] == Constant.DB_NULL ? "NULL, " : "'" + opnote.ToString() + "', ");
            sb.Append(nullInds[SuOper.ADMIN] == Constant.DB_NULL ? "NULL, " : "'" + admin.ToString() + "', ");
            sb.Append(nullInds[SuOper.EMAIL] == Constant.DB_NULL ? "NULL, " : "'" + email.ToString() + "', ");
            sb.Append(nullInds[SuOper.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SuOper.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[SuOper.CMD] + " = " + (nullInds[SuOper.CMD] == Constant.DB_NULL ? "NULL, " : "'" + cmd.ToString() + "', "));
            sb.Append(columnNames[SuOper.RECSTAT] + " = " + (nullInds[SuOper.RECSTAT] == Constant.DB_NULL ? "NULL, " : "'" + recstat.ToString() + "', "));
            sb.Append(columnNames[SuOper.OPER] + " = " + (nullInds[SuOper.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', "));
            sb.Append(columnNames[SuOper.NAMEOP] + " = " + (nullInds[SuOper.NAMEOP] == Constant.DB_NULL ? "NULL, " : "'" + nameop.ToString() + "', "));
            sb.Append(columnNames[SuOper.COOPER] + " = " + (nullInds[SuOper.COOPER] == Constant.DB_NULL ? "NULL, " : "'" + cooper.ToString() + "', "));
            sb.Append(columnNames[SuOper.MDBM] + " = " + (nullInds[SuOper.MDBM] == Constant.DB_NULL ? "NULL, " : "'" + mdbm.ToString() + "', "));
            sb.Append(columnNames[SuOper.ADDR] + " = " + (nullInds[SuOper.ADDR] == Constant.DB_NULL ? "NULL, " : "'" + addr.ToString() + "', "));
            sb.Append(columnNames[SuOper.CITY] + " = " + (nullInds[SuOper.CITY] == Constant.DB_NULL ? "NULL, " : "'" + city.ToString() + "', "));
            sb.Append(columnNames[SuOper.PRSTAT] + " = " + (nullInds[SuOper.PRSTAT] == Constant.DB_NULL ? "NULL, " : "'" + prstat.ToString() + "', "));
            sb.Append(columnNames[SuOper.ZIPPC] + " = " + (nullInds[SuOper.ZIPPC] == Constant.DB_NULL ? "NULL, " : "'" + zippc.ToString() + "', "));
            sb.Append(columnNames[SuOper.DEPT] + " = " + (nullInds[SuOper.DEPT] == Constant.DB_NULL ? "NULL, " : "'" + dept.ToString() + "', "));
            sb.Append(columnNames[SuOper.NAMEP] + " = " + (nullInds[SuOper.NAMEP] == Constant.DB_NULL ? "NULL, " : "'" + namep.ToString() + "', "));
            sb.Append(columnNames[SuOper.PHONEP] + " = " + (nullInds[SuOper.PHONEP] == Constant.DB_NULL ? "NULL, " : "'" + phonep.ToString() + "', "));
            sb.Append(columnNames[SuOper.FAXNUM] + " = " + (nullInds[SuOper.FAXNUM] == Constant.DB_NULL ? "NULL, " : "'" + faxnum.ToString() + "', "));
            sb.Append(columnNames[SuOper.TELECOM] + " = " + (nullInds[SuOper.TELECOM] == Constant.DB_NULL ? "NULL, " : "'" + telecom.ToString() + "', "));
            sb.Append(columnNames[SuOper.OPNOTE] + " = " + (nullInds[SuOper.OPNOTE] == Constant.DB_NULL ? "NULL, " : "'" + opnote.ToString() + "', "));
            sb.Append(columnNames[SuOper.ADMIN] + " = " + (nullInds[SuOper.ADMIN] == Constant.DB_NULL ? "NULL, " : "'" + admin.ToString() + "', "));
            sb.Append(columnNames[SuOper.EMAIL] + " = " + (nullInds[SuOper.EMAIL] == Constant.DB_NULL ? "NULL, " : "'" + email.ToString() + "', "));
            sb.Append(columnNames[SuOper.MDATE] + " = " + (nullInds[SuOper.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SuOper.MTIME] + " = " + (nullInds[SuOper.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

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
