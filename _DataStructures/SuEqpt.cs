using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with the 'subupt' table 
    /// <b>main.sd_eqpt</b> except for the additional fields 'cmd' and 'recstat'.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SuEqpt
    {
        // IMPORTANT!
        // =========
        // The following qty. 17 member values correspond to the legacy native 
        // structure suEqpt_; member names are prescribed to be identical.
        // The order of appearance of these qty. 17 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CMD_SZ)]
        public string cmd;    //#00
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RECSTAT_SZ)]
        public string recstat;    //#01
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ECODE_SZ)]
        public string ecode;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float estab;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EXREF_SZ)]
        public string exref;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EMANU_SZ)]  //was 11
        public string emanu;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EMODEL_SZ)]  //was 21
        public string emodel;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EDESC_SZ)]  //was 33
        public string edesc;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ETYPE_SZ)]  //was 3
        public string etype;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ETRAF_SZ)]  //was 7
        public string etraf;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EMISSION_SZ)]  //was 11
        public string emission;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float e1stif;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float e2ndif;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float thhold;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EBNDCDE_SZ)]
        public string ebndcde;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------
        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 17;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "cmd", "recstat", "ecode", "estab", "exref", "emanu", "emodel", "edesc", "etype", "etraf", "emission", "e1stif", "e2ndif", "thhold", "ebndcde", "mdate", "mtime" };

        public const int CMD = 0;
        public const int RECSTAT = 1;
        public const int ECODE = 2;
        public const int ESTAB = 3;
        public const int EXREF = 4;
        public const int EMANU = 5;
        public const int EMODEL = 6;
        public const int EDESC = 7;
        public const int ETYPE = 8;
        public const int ETRAF = 9;
        public const int EMISSION = 10;
        public const int E1STIF = 11;
        public const int E2NDIF = 12;
        public const int THHOLD = 13;
        public const int EBNDCDE = 14;
        public const int MDATE = 15;
        public const int MTIME = 16;

        public const int CMD_SZ = Constant.CMD_SZ;
        public const int RECSTAT_SZ = Constant.RECSTAT_SZ;
        public const int ECODE_SZ = Constant.ECODE_SZ;
        public const int EXREF_SZ = Constant.ECODE_SZ;
        public const int EMANU_SZ = Constant.EMANU_SZ;
        public const int EMODEL_SZ = Constant.EMODEL_SZ;
        public const int EDESC_SZ = Constant.EDESC_SZ;
        public const int ETYPE_SZ = Constant.ETYPE_SZ;
        public const int ETRAF_SZ = Constant.ETRAF_SZ;
        public const int EMISSION_SZ = Constant.EMISSION_SZ;
        public const int EBNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //----------------------------------------------------------------------------

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
        static SuEqpt()
        {
            mAllFieldsAsCSV = ListOfColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SuEqpt()
        {
            cmd = "";
            recstat = "";
            ecode = "";
            exref = "";
            emanu = "";
            emodel = "";
            edesc = "";
            etype = "";
            etraf = "";
            emission = "";
            ebndcde = "";
            mdate = "";
            mtime = "";
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\r\ncmd =      " + cmd);
            sb.Append("\r\nrecstat =  " + recstat);
            sb.Append("\r\necode =    " + ecode);
            sb.Append("\r\nestab =    " + estab);
            sb.Append("\r\nexref =    " + exref);
            sb.Append("\r\nemanu =    " + emanu);
            sb.Append("\r\nemodel =   " + emodel);
            sb.Append("\r\nedesc =    " + edesc);
            sb.Append("\r\netype =    " + etype);
            sb.Append("\r\netraf =    " + etraf);
            sb.Append("\r\nemission = " + emission);
            sb.Append("\r\ne1stif =   " + e1stif);
            sb.Append("\r\ne2ndif =   " + e2ndif);
            sb.Append("\r\nthhold =   " + thhold);
            sb.Append("\r\nebndcde =  " + ebndcde);
            sb.Append("\r\nmdate =    " + mdate);
            sb.Append("\r\nmtime =    " + mtime);

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
            sb.Append("\n ===== SuEqpt ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "cmd = " + cmd);
            sb.Append("\n" + nullInds[i++] + "      " + "recstat = " + recstat);
            sb.Append("\n" + nullInds[i++] + "      " + "ecode = " + ecode);
            sb.Append("\n" + nullInds[i++] + "      " + "estab = " + estab);
            sb.Append("\n" + nullInds[i++] + "      " + "exref = " + exref);
            sb.Append("\n" + nullInds[i++] + "      " + "emanu = " + emanu);
            sb.Append("\n" + nullInds[i++] + "      " + "emodel = " + emodel);
            sb.Append("\n" + nullInds[i++] + "      " + "edesc = " + edesc);
            sb.Append("\n" + nullInds[i++] + "      " + "etype = " + etype);
            sb.Append("\n" + nullInds[i++] + "      " + "etraf = " + etraf);
            sb.Append("\n" + nullInds[i++] + "      " + "emission = " + emission);
            sb.Append("\n" + nullInds[i++] + "      " + "e1stif = " + e1stif);
            sb.Append("\n" + nullInds[i++] + "      " + "e2ndif = " + e2ndif);
            sb.Append("\n" + nullInds[i++] + "      " + "thhold = " + thhold);
            sb.Append("\n" + nullInds[i++] + "      " + "ebndcde = " + ebndcde);
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
        public SuEqpt DeepCopy()
        {
            // A deep copy object has deep copies of the nonstatic fields of the current object.
            //    - If a field is a value type then a bit-by-bit copy of the field is created. 
            //    - If a field is a reference type then a deep copy of the referred object is created.

            SuEqpt suEqpt = new SuEqpt();

            suEqpt.cmd = this.cmd;
            suEqpt.recstat = this.recstat;
            suEqpt.ecode = this.ecode;
            suEqpt.estab = this.estab;
            suEqpt.exref = this.exref;
            suEqpt.emanu = this.emanu;
            suEqpt.emodel = this.emodel;
            suEqpt.edesc = this.edesc;
            suEqpt.etype = this.etype;
            suEqpt.etraf = this.etraf;
            suEqpt.emission = this.emission;
            suEqpt.e1stif = this.e1stif;
            suEqpt.e2ndif = this.e2ndif;
            suEqpt.thhold = this.thhold;
            suEqpt.ebndcde = this.ebndcde;
            suEqpt.mdate = this.mdate;
            suEqpt.mtime = this.mtime;

            return suEqpt;
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
