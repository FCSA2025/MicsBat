using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// This class has fields that are isomorphic with the PDF table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_titl</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FtTitl
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_VALIDATED_SZ)]
        public string validated;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_NAMEF_SZ)]
        public string namef;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_SOURCE_SZ)]
        public string source;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.FTTITLE_DESCR_SZ)]
        public string descr;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------

        public const int NUM_COLUMNS = 6;

        public const int VALIDATED = 0;
        public const int NAMEF = 1;
        public const int SOURCE = 2;
        public const int DESCR = 3;
        public const int MDATE = 4;
        public const int MTIME = 5;

        public const int VALIDATED_SZ = Constant.FTTITLE_VALIDATED_SZ;
        public const int NAMEF_SZ = Constant.FTTITLE_NAMEF_SZ;
        public const int SOURCE_SZ = Constant.FTTITLE_SOURCE_SZ;
        public const int DESCR_SZ = Constant.FTTITLE_DESCR_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "validated", "namef", "source", "descr", "mdate", "mtime" };

        //List of all column names for SQL 'select' command.
        //Provide read-only access to this 'constant'.
        private static string mAllFieldsAsCSV;
        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        //Provide read-only access to this 'constant'.
        private static string mAllBindingsAsCSV;
        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //----------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static FtTitl()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FtTitl()
        {
            //Set all column members to prescribed initial values.
            Initialize();
        }

        /// <summary>
        /// This method initializes the value of all member fields.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            const string STRING_INIT_VAL = "";

            validated = STRING_INIT_VAL;
            namef = STRING_INIT_VAL;
            source = STRING_INIT_VAL;
            descr = STRING_INIT_VAL;
            mdate = STRING_INIT_VAL;
            mtime = STRING_INIT_VAL;
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
            sb.Append("\r\n");
            sb.AppendLine("validated = " + validated);
            sb.AppendLine("namef     = " + namef);
            sb.AppendLine("source    = " + source);
            sb.AppendLine("descr     = " + descr);
            sb.AppendLine("mdate     = " + mdate);
            sb.AppendLine("mtime     = " + mtime);
            return sb.ToString();
        }

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[0] = Marshal.StringToHGlobalAnsi(validated);  // #0  string  validated 
            parameterValuePtr[1] = Marshal.StringToHGlobalAnsi(namef);      // #1  string  namef
            parameterValuePtr[2] = Marshal.StringToHGlobalAnsi(source);     // #2  string  source
            parameterValuePtr[3] = Marshal.StringToHGlobalAnsi(descr);      // #3  string  descr
            parameterValuePtr[4] = Marshal.StringToHGlobalAnsi(mdate);      // #4  string  mdate
            parameterValuePtr[5] = Marshal.StringToHGlobalAnsi(mtime);      // #5  string  mtime

            return parameterValuePtr;
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
        /// of all column names for use in a SQL 'update' query using
        /// bound parameter values
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
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float into native memory using Marshal method.
            float[] F = new float[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[VALIDATED] = Marshal.StringToHGlobalAnsi(validated);
            parameterValuePtr[NAMEF] = Marshal.StringToHGlobalAnsi(namef);
            parameterValuePtr[SOURCE] = Marshal.StringToHGlobalAnsi(source);
            parameterValuePtr[DESCR] = Marshal.StringToHGlobalAnsi(descr);
            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);
            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }

        /// <summary>
        /// This method returns a string comprising a complete SQL 'insert' query using
        /// bound parameter values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string BuildSqlInsertString(string tableName)
        {
            StringBuilder valuesSB = new StringBuilder();
            valuesSB.Append("(?");
            for (int i = 1; i < NUM_COLUMNS; i++)
            {
                valuesSB.Append(", ?");
            }
            valuesSB.Append(")");

            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(tableName);
            sb.Append(" (");
            sb.Append(AllColumnsForSqlSelect);
            sb.Append(") VALUES ");
            sb.Append(valuesSB);

            return sb.ToString();
        }

        public const string CREATE_TABLE = "" +
        "CREATE TABLE [{0}].[{1}]" +
        "(" +
            "[validated] [char](1) NULL," +
            "[namef] [char](16) NULL," +
            "[source] [char](6) NULL," +
            "[descr] [char](40) NULL," +
            "[mdate] [char](10) NULL," +
            "[mtime] [char](8) NULL," +
        ") ON[PRIMARY]";



    }
}
