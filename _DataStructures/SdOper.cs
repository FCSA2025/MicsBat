using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _Configuration;

namespace _DataStructures
{
    using _NewLib;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLHANDLE = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the SDB table <b>main.sd_oper</b> .
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdOper
    {
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
        public const int NUM_COLUMNS = 18;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "oper", "nameop", "cooper", "mdbm", "addr", "city", "prstat", "zippc", "dept", "namep", "phonep", "faxnum", "telecom", "opnote", "admin", "email", "mdate", "mtime" };

        public const int OPER = 0;
        public const int NAMEOP = 1;
        public const int COOPER = 2;
        public const int MDBM = 3;
        public const int ADDR = 4;
        public const int CITY = 5;
        public const int PRSTAT = 6;
        public const int ZIPPC = 7;
        public const int DEPT = 8;
        public const int NAMEP = 9;
        public const int PHONEP = 10;
        public const int FAXNUM = 11;
        public const int TELECOM = 12;
        public const int OPNOTE = 13;
        public const int ADMIN = 14;
        public const int EMAIL = 15;
        public const int MDATE = 16;
        public const int MTIME = 17;

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
        static SdOper()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SdOper()
        {
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
        /// This method returns a CSV string that shows the current values of the 'oper' and 'nameop'
        /// member variables of this SdOper object.
        /// </summary>
        /// <returns></returns>
        public string ToStringTerse()
        {
            return String.Format("oper = {0,6}, nameop = {1}", oper, nameop);
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
            sb.Append("\n ===== SdOper ===== ");

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

            sb.Append(nullInds[SdOper.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', ");
            sb.Append(nullInds[SdOper.NAMEOP] == Constant.DB_NULL ? "NULL, " : "'" + nameop.ToString() + "', ");
            sb.Append(nullInds[SdOper.COOPER] == Constant.DB_NULL ? "NULL, " : "'" + cooper.ToString() + "', ");
            sb.Append(nullInds[SdOper.MDBM] == Constant.DB_NULL ? "NULL, " : "'" + mdbm.ToString() + "', ");
            sb.Append(nullInds[SdOper.ADDR] == Constant.DB_NULL ? "NULL, " : "'" + addr.ToString() + "', ");
            sb.Append(nullInds[SdOper.CITY] == Constant.DB_NULL ? "NULL, " : "'" + city.ToString() + "', ");
            sb.Append(nullInds[SdOper.PRSTAT] == Constant.DB_NULL ? "NULL, " : "'" + prstat.ToString() + "', ");
            sb.Append(nullInds[SdOper.ZIPPC] == Constant.DB_NULL ? "NULL, " : "'" + zippc.ToString() + "', ");
            sb.Append(nullInds[SdOper.DEPT] == Constant.DB_NULL ? "NULL, " : "'" + dept.ToString() + "', ");
            sb.Append(nullInds[SdOper.NAMEP] == Constant.DB_NULL ? "NULL, " : "'" + namep.ToString() + "', ");
            sb.Append(nullInds[SdOper.PHONEP] == Constant.DB_NULL ? "NULL, " : "'" + phonep.ToString() + "', ");
            sb.Append(nullInds[SdOper.FAXNUM] == Constant.DB_NULL ? "NULL, " : "'" + faxnum.ToString() + "', ");
            sb.Append(nullInds[SdOper.TELECOM] == Constant.DB_NULL ? "NULL, " : "'" + telecom.ToString() + "', ");
            sb.Append(nullInds[SdOper.OPNOTE] == Constant.DB_NULL ? "NULL, " : "'" + opnote.ToString() + "', ");
            sb.Append(nullInds[SdOper.ADMIN] == Constant.DB_NULL ? "NULL, " : "'" + admin.ToString() + "', ");
            sb.Append(nullInds[SdOper.EMAIL] == Constant.DB_NULL ? "NULL, " : "'" + email.ToString() + "', ");
            sb.Append(nullInds[SdOper.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdOper.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdOper.OPER] + " = " + (nullInds[SdOper.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', "));
            sb.Append(columnNames[SdOper.NAMEOP] + " = " + (nullInds[SdOper.NAMEOP] == Constant.DB_NULL ? "NULL, " : "'" + nameop.ToString() + "', "));
            sb.Append(columnNames[SdOper.COOPER] + " = " + (nullInds[SdOper.COOPER] == Constant.DB_NULL ? "NULL, " : "'" + cooper.ToString() + "', "));
            sb.Append(columnNames[SdOper.MDBM] + " = " + (nullInds[SdOper.MDBM] == Constant.DB_NULL ? "NULL, " : "'" + mdbm.ToString() + "', "));
            sb.Append(columnNames[SdOper.ADDR] + " = " + (nullInds[SdOper.ADDR] == Constant.DB_NULL ? "NULL, " : "'" + addr.ToString() + "', "));
            sb.Append(columnNames[SdOper.CITY] + " = " + (nullInds[SdOper.CITY] == Constant.DB_NULL ? "NULL, " : "'" + city.ToString() + "', "));
            sb.Append(columnNames[SdOper.PRSTAT] + " = " + (nullInds[SdOper.PRSTAT] == Constant.DB_NULL ? "NULL, " : "'" + prstat.ToString() + "', "));
            sb.Append(columnNames[SdOper.ZIPPC] + " = " + (nullInds[SdOper.ZIPPC] == Constant.DB_NULL ? "NULL, " : "'" + zippc.ToString() + "', "));
            sb.Append(columnNames[SdOper.DEPT] + " = " + (nullInds[SdOper.DEPT] == Constant.DB_NULL ? "NULL, " : "'" + dept.ToString() + "', "));
            sb.Append(columnNames[SdOper.NAMEP] + " = " + (nullInds[SdOper.NAMEP] == Constant.DB_NULL ? "NULL, " : "'" + namep.ToString() + "', "));
            sb.Append(columnNames[SdOper.PHONEP] + " = " + (nullInds[SdOper.PHONEP] == Constant.DB_NULL ? "NULL, " : "'" + phonep.ToString() + "', "));
            sb.Append(columnNames[SdOper.FAXNUM] + " = " + (nullInds[SdOper.FAXNUM] == Constant.DB_NULL ? "NULL, " : "'" + faxnum.ToString() + "', "));
            sb.Append(columnNames[SdOper.TELECOM] + " = " + (nullInds[SdOper.TELECOM] == Constant.DB_NULL ? "NULL, " : "'" + telecom.ToString() + "', "));
            sb.Append(columnNames[SdOper.OPNOTE] + " = " + (nullInds[SdOper.OPNOTE] == Constant.DB_NULL ? "NULL, " : "'" + opnote.ToString() + "', "));
            sb.Append(columnNames[SdOper.ADMIN] + " = " + (nullInds[SdOper.ADMIN] == Constant.DB_NULL ? "NULL, " : "'" + admin.ToString() + "', "));
            sb.Append(columnNames[SdOper.EMAIL] + " = " + (nullInds[SdOper.EMAIL] == Constant.DB_NULL ? "NULL, " : "'" + email.ToString() + "', "));
            sb.Append(columnNames[SdOper.MDATE] + " = " + (nullInds[SdOper.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdOper.MTIME] + " = " + (nullInds[SdOper.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

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

        /// <summary>
        /// This method returns instantiated SdOper and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuOper object and its associated nullInds; 
        /// SdOper has identical members to SuOper except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suOper"></param>
        /// <param name="suOperNullInds"></param>
        /// <param name="sdOper"></param>
        /// <param name="sdOperNullInds"></param>
        public static void MakeSdFromSu(SuOper suOper, SQLLEN[] suOperNullInds, out SdOper sdOper, out SQLLEN[] sdOperNullInds)
        {
            sdOper = new SdOper();
            sdOperNullInds = new SQLLEN[NUM_COLUMNS];

            sdOper.oper = suOper.oper;
            sdOperNullInds[SdOper.OPER] = suOperNullInds[SuOper.OPER];

            sdOper.nameop = suOper.nameop;
            sdOperNullInds[SdOper.NAMEOP] = suOperNullInds[SuOper.NAMEOP];

            sdOper.cooper = suOper.cooper;
            sdOperNullInds[SdOper.COOPER] = suOperNullInds[SuOper.COOPER];

            sdOper.mdbm = suOper.mdbm;
            sdOperNullInds[SdOper.MDBM] = suOperNullInds[SuOper.MDBM];

            sdOper.addr = suOper.addr;
            sdOperNullInds[SdOper.ADDR] = suOperNullInds[SuOper.ADDR];

            sdOper.city = suOper.city;
            sdOperNullInds[SdOper.CITY] = suOperNullInds[SuOper.CITY];

            sdOper.prstat = suOper.prstat;
            sdOperNullInds[SdOper.PRSTAT] = suOperNullInds[SuOper.PRSTAT];

            sdOper.zippc = suOper.zippc;
            sdOperNullInds[SdOper.ZIPPC] = suOperNullInds[SuOper.ZIPPC];

            sdOper.dept = suOper.dept;
            sdOperNullInds[SdOper.DEPT] = suOperNullInds[SuOper.DEPT];

            sdOper.namep = suOper.namep;
            sdOperNullInds[SdOper.NAMEP] = suOperNullInds[SuOper.NAMEP];

            sdOper.phonep = suOper.phonep;
            sdOperNullInds[SdOper.PHONEP] = suOperNullInds[SuOper.PHONEP];

            sdOper.faxnum = suOper.faxnum;
            sdOperNullInds[SdOper.FAXNUM] = suOperNullInds[SuOper.FAXNUM];

            sdOper.telecom = suOper.telecom;
            sdOperNullInds[SdOper.TELECOM] = suOperNullInds[SuOper.TELECOM];

            sdOper.opnote = suOper.opnote;
            sdOperNullInds[SdOper.OPNOTE] = suOperNullInds[SuOper.OPNOTE];

            sdOper.admin = suOper.admin;
            sdOperNullInds[SdOper.ADMIN] = suOperNullInds[SuOper.ADMIN];

            sdOper.email = suOper.email;
            sdOperNullInds[SdOper.EMAIL] = suOperNullInds[SuOper.EMAIL];

            sdOper.mdate = suOper.mdate;
            sdOperNullInds[SdOper.MDATE] = suOperNullInds[SuOper.MDATE];

            sdOper.mtime = suOper.mtime;
            sdOperNullInds[SdOper.MTIME] = suOperNullInds[SuOper.MTIME];
        }

        /// <summary>
        /// This method returns an IntPtr[] whose elements point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.sd_ctxd; the ODBC binding
        /// 'count' starts at zero.
        /// Thus, this method creates the parameter bindings prior to an SQL / ODBC 'fetch', 'update' or 
        /// 'insert' query.
        /// </summary>
        /// <param name="hStmt"></param>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[OPER] = Marshal.AllocHGlobal(OPER_SZ + 1);
            nullIndPtrs[OPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPER + 1, tgtValPtrs[OPER], SdOper.OPER_SZ, nullIndPtrs[OPER]);

            tgtValPtrs[NAMEOP] = Marshal.AllocHGlobal(NAMEOP_SZ + 1);
            nullIndPtrs[NAMEOP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NAMEOP + 1, tgtValPtrs[NAMEOP], SdOper.NAMEOP_SZ, nullIndPtrs[NAMEOP]);

            tgtValPtrs[COOPER] = Marshal.AllocHGlobal(COOPER_SZ + 1);
            nullIndPtrs[COOPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, COOPER + 1, tgtValPtrs[COOPER], SdOper.COOPER_SZ, nullIndPtrs[COOPER]);

            tgtValPtrs[MDBM] = Marshal.AllocHGlobal(MDBM_SZ + 1);
            nullIndPtrs[MDBM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDBM + 1, tgtValPtrs[MDBM], SdOper.MDBM_SZ, nullIndPtrs[MDBM]);

            tgtValPtrs[ADDR] = Marshal.AllocHGlobal(ADDR_SZ + 1);
            nullIndPtrs[ADDR] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ADDR + 1, tgtValPtrs[ADDR], SdOper.ADDR_SZ, nullIndPtrs[ADDR]);

            tgtValPtrs[CITY] = Marshal.AllocHGlobal(CITY_SZ + 1);
            nullIndPtrs[CITY] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CITY + 1, tgtValPtrs[CITY], SdOper.CITY_SZ, nullIndPtrs[CITY]);

            tgtValPtrs[PRSTAT] = Marshal.AllocHGlobal(PRSTAT_SZ + 1);
            nullIndPtrs[PRSTAT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PRSTAT + 1, tgtValPtrs[PRSTAT], SdOper.PRSTAT_SZ, nullIndPtrs[PRSTAT]);

            tgtValPtrs[ZIPPC] = Marshal.AllocHGlobal(ZIPPC_SZ + 1);
            nullIndPtrs[ZIPPC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ZIPPC + 1, tgtValPtrs[ZIPPC], SdOper.ZIPPC_SZ, nullIndPtrs[ZIPPC]);

            tgtValPtrs[DEPT] = Marshal.AllocHGlobal(DEPT_SZ + 1);
            nullIndPtrs[DEPT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, DEPT + 1, tgtValPtrs[DEPT], SdOper.DEPT_SZ, nullIndPtrs[DEPT]);

            tgtValPtrs[NAMEP] = Marshal.AllocHGlobal(NAMEP_SZ + 1);
            nullIndPtrs[NAMEP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NAMEP + 1, tgtValPtrs[NAMEP], SdOper.NAMEP_SZ, nullIndPtrs[NAMEP]);

            tgtValPtrs[PHONEP] = Marshal.AllocHGlobal(PHONEP_SZ + 1);
            nullIndPtrs[PHONEP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PHONEP + 1, tgtValPtrs[PHONEP], SdOper.PHONEP_SZ, nullIndPtrs[PHONEP]);

            tgtValPtrs[FAXNUM] = Marshal.AllocHGlobal(FAXNUM_SZ + 1);
            nullIndPtrs[FAXNUM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FAXNUM + 1, tgtValPtrs[FAXNUM], SdOper.FAXNUM_SZ, nullIndPtrs[FAXNUM]);

            tgtValPtrs[TELECOM] = Marshal.AllocHGlobal(TELECOM_SZ + 1);
            nullIndPtrs[TELECOM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TELECOM + 1, tgtValPtrs[TELECOM], SdOper.TELECOM_SZ, nullIndPtrs[TELECOM]);

            tgtValPtrs[OPNOTE] = Marshal.AllocHGlobal(OPNOTE_SZ + 1);
            nullIndPtrs[OPNOTE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPNOTE + 1, tgtValPtrs[OPNOTE], SdOper.OPNOTE_SZ, nullIndPtrs[OPNOTE]);

            tgtValPtrs[ADMIN] = Marshal.AllocHGlobal(ADMIN_SZ + 1);
            nullIndPtrs[ADMIN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ADMIN + 1, tgtValPtrs[ADMIN], SdOper.ADMIN_SZ, nullIndPtrs[ADMIN]);

            tgtValPtrs[EMAIL] = Marshal.AllocHGlobal(EMAIL_SZ + 1);
            nullIndPtrs[EMAIL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EMAIL + 1, tgtValPtrs[EMAIL], SdOper.EMAIL_SZ, nullIndPtrs[EMAIL]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdOper.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdOper.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdOper object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdOper"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdOper sdOper, out SQLLEN[] nullInds)
        {
            sdOper = new SdOper();
            nullInds = NullHelper.CreateArrayOfNullInd(SdOper.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.oper = "";
                nullInds[OPER] = Constant.DB_NULL;
            }
            else
            {
                sdOper.oper = Marshal.PtrToStringAnsi(tgtValPtrs[OPER]).Trim();
                nullInds[OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NAMEOP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.nameop = "";
                nullInds[NAMEOP] = Constant.DB_NULL;
            }
            else
            {
                sdOper.nameop = Marshal.PtrToStringAnsi(tgtValPtrs[NAMEOP]).Trim();
                nullInds[NAMEOP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[COOPER]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.cooper = "";
                nullInds[COOPER] = Constant.DB_NULL;
            }
            else
            {
                sdOper.cooper = Marshal.PtrToStringAnsi(tgtValPtrs[COOPER]).Trim();
                nullInds[COOPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDBM]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.mdbm = "";
                nullInds[MDBM] = Constant.DB_NULL;
            }
            else
            {
                sdOper.mdbm = Marshal.PtrToStringAnsi(tgtValPtrs[MDBM]).Trim();
                nullInds[MDBM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ADDR]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.addr = "";
                nullInds[ADDR] = Constant.DB_NULL;
            }
            else
            {
                sdOper.addr = Marshal.PtrToStringAnsi(tgtValPtrs[ADDR]).Trim();
                nullInds[ADDR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CITY]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.city = "";
                nullInds[CITY] = Constant.DB_NULL;
            }
            else
            {
                sdOper.city = Marshal.PtrToStringAnsi(tgtValPtrs[CITY]).Trim();
                nullInds[CITY] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PRSTAT]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.prstat = "";
                nullInds[PRSTAT] = Constant.DB_NULL;
            }
            else
            {
                sdOper.prstat = Marshal.PtrToStringAnsi(tgtValPtrs[PRSTAT]).Trim();
                nullInds[PRSTAT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ZIPPC]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.zippc = "";
                nullInds[ZIPPC] = Constant.DB_NULL;
            }
            else
            {
                sdOper.zippc = Marshal.PtrToStringAnsi(tgtValPtrs[ZIPPC]).Trim();
                nullInds[ZIPPC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DEPT]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.dept = "";
                nullInds[DEPT] = Constant.DB_NULL;
            }
            else
            {
                sdOper.dept = Marshal.PtrToStringAnsi(tgtValPtrs[DEPT]).Trim();
                nullInds[DEPT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NAMEP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.namep = "";
                nullInds[NAMEP] = Constant.DB_NULL;
            }
            else
            {
                sdOper.namep = Marshal.PtrToStringAnsi(tgtValPtrs[NAMEP]).Trim();
                nullInds[NAMEP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PHONEP]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.phonep = "";
                nullInds[PHONEP] = Constant.DB_NULL;
            }
            else
            {
                sdOper.phonep = Marshal.PtrToStringAnsi(tgtValPtrs[PHONEP]).Trim();
                nullInds[PHONEP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FAXNUM]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.faxnum = "";
                nullInds[FAXNUM] = Constant.DB_NULL;
            }
            else
            {
                sdOper.faxnum = Marshal.PtrToStringAnsi(tgtValPtrs[FAXNUM]).Trim();
                nullInds[FAXNUM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TELECOM]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.telecom = "";
                nullInds[TELECOM] = Constant.DB_NULL;
            }
            else
            {
                sdOper.telecom = Marshal.PtrToStringAnsi(tgtValPtrs[TELECOM]).Trim();
                nullInds[TELECOM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPNOTE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.opnote = "";
                nullInds[OPNOTE] = Constant.DB_NULL;
            }
            else
            {
                sdOper.opnote = Marshal.PtrToStringAnsi(tgtValPtrs[OPNOTE]).Trim();
                nullInds[OPNOTE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ADMIN]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.admin = "";
                nullInds[ADMIN] = Constant.DB_NULL;
            }
            else
            {
                sdOper.admin = Marshal.PtrToStringAnsi(tgtValPtrs[ADMIN]).Trim();
                nullInds[ADMIN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EMAIL]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.email = "";
                nullInds[EMAIL] = Constant.DB_NULL;
            }
            else
            {
                sdOper.email = Marshal.PtrToStringAnsi(tgtValPtrs[EMAIL]).Trim();
                nullInds[EMAIL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdOper.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdOper.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdOper.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }






        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_oper](" +
    "[oper] [char](6) NOT NULL," +
    "[nameop] [char](40) NULL," +
    "[cooper] [char](6) NULL," +
    "[mdbm] [char](6) NULL," +
    "[addr] [char](50) NULL," +
    "[city] [char](15) NULL," +
    "[prstat] [char](2) NULL," +
    "[zippc] [char](10) NULL," +
    "[dept] [char](40) NULL," +
    "[namep] [char](40) NULL," +
    "[phonep] [char](16) NULL," +
    "[faxnum] [char](16) NULL," +
    "[telecom] [char](1) NULL," +
    "[opnote] [char](2) NULL," +
    "[admin] [char](12) NULL," +
    "[email] [char](50) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_oper] PRIMARY KEY CLUSTERED " +
"(" +
    "[oper] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}

