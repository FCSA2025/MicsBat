using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _NewLib;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLHANDLE = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the SDF table 
    /// <b>main.sd_note</b>  .
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdNote
    {
        // IMPORTANT!
        // =========
        // The following qty. 5 member values correspond to the legacy native 
        // structure sdNote_; member names are prescribed to be identical.
        // The order of appearance of these qty. 5 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPER_SZ)]
        public string oper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NONUM_SZ)]
        public string nonum;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTE_SZ)]
        public string note;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //----------------------------------------------------------------
        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 5;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "oper", "nonum", "note", "mdate", "mtime" };

        public const int OPER = 0;
        public const int NONUM = 1;
        public const int NOTE = 2;
        public const int MDATE = 3;
        public const int MTIME = 4;

        public const int OPER_SZ = Constant.OPERCODE_SZ;
        public const int NONUM_SZ = Constant.SUNOTE_NONUM_SZ;
        public const int NOTE_SZ = Constant.SUNOTE_NOTE_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //---------------------------------------------------------------------------------

        /// <summary>
        /// Per-instance default constructor.
        /// </summary>
        public SdNote()
        {
            oper = "";
            nonum = "";
            note = "";
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
            sb.Append("\n ===== SdNote ===== ");

            sb.Append("\noper  = " + oper);
            sb.Append("\nnonum = " + nonum);
            sb.Append("\nnote  = " + note);
            sb.Append("\nmdate = " + mdate);
            sb.Append("\nmtime = " + mtime);

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
            sb.Append("\n ===== SdNote ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "oper = " + oper);
            sb.Append("\n" + nullInds[i++] + "      " + "nonum = " + nonum);
            sb.Append("\n" + nullInds[i++] + "      " + "note = " + note);
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

            sb.Append(nullInds[SdNote.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', ");
            sb.Append(nullInds[SdNote.NONUM] == Constant.DB_NULL ? "NULL, " : "'" + nonum.ToString() + "', ");
            sb.Append(nullInds[SdNote.NOTE] == Constant.DB_NULL ? "NULL, " : "'" + note.ToString() + "', ");
            sb.Append(nullInds[SdNote.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdNote.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdNote.OPER] + " = " + (nullInds[SdNote.OPER] == Constant.DB_NULL ? "NULL, " : "'" + oper.ToString() + "', "));
            sb.Append(columnNames[SdNote.NONUM] + " = " + (nullInds[SdNote.NONUM] == Constant.DB_NULL ? "NULL, " : "'" + nonum.ToString() + "', "));
            sb.Append(columnNames[SdNote.NOTE] + " = " + (nullInds[SdNote.NOTE] == Constant.DB_NULL ? "NULL, " : "'" + note.ToString() + "', "));
            sb.Append(columnNames[SdNote.MDATE] + " = " + (nullInds[SdNote.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdNote.MTIME] + " = " + (nullInds[SdNote.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdNote and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuNote object and its associated nullInds; 
        /// SdNote has identical members to SuNote except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suNote"></param>
        /// <param name="suNoteNullInds"></param>
        /// <param name="sdNote"></param>
        /// <param name="sdNoteNullInds"></param>
        public static void MakeSdFromSu(SuNote suNote, SQLLEN[] suNoteNullInds, out SdNote sdNote, out SQLLEN[] sdNoteNullInds)
        {
            sdNote = new SdNote();
            sdNoteNullInds = new SQLLEN[NUM_COLUMNS];

            sdNote.oper = suNote.oper;
            sdNoteNullInds[SdNote.OPER] = suNoteNullInds[SuNote.OPER];

            sdNote.nonum = suNote.nonum;
            sdNoteNullInds[SdNote.NONUM] = suNoteNullInds[SuNote.NONUM];

            sdNote.note = suNote.note;
            sdNoteNullInds[SdNote.NOTE] = suNoteNullInds[SuNote.NOTE];

            sdNote.mdate = suNote.mdate;
            sdNoteNullInds[SdNote.MDATE] = suNoteNullInds[SuNote.MDATE];

            sdNote.mtime = suNote.mtime;
            sdNoteNullInds[SdNote.MTIME] = suNoteNullInds[SuNote.MTIME];

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
            Bind.BindBufferToString(hStmt, OPER + 1, tgtValPtrs[OPER], SdNote.OPER_SZ, nullIndPtrs[OPER]);

            tgtValPtrs[NONUM] = Marshal.AllocHGlobal(NONUM_SZ + 1);
            nullIndPtrs[NONUM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NONUM + 1, tgtValPtrs[NONUM], SdNote.NONUM_SZ, nullIndPtrs[NONUM]);

            tgtValPtrs[NOTE] = Marshal.AllocHGlobal(NOTE_SZ + 1);
            nullIndPtrs[NOTE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, NOTE + 1, tgtValPtrs[NOTE], SdNote.NOTE_SZ, nullIndPtrs[NOTE]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdNote.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdNote.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdNote object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdNote"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdNote sdNote, out SQLLEN[] nullInds)
        {
            sdNote = new SdNote();
            nullInds = NullHelper.CreateArrayOfNullInd(SdNote.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                sdNote.oper = "";
                nullInds[OPER] = Constant.DB_NULL;
            }
            else
            {
                sdNote.oper = Marshal.PtrToStringAnsi(tgtValPtrs[OPER]).Trim();
                nullInds[OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NONUM]);
            if (nullInd == Constant.DB_NULL)
            {
                sdNote.nonum = "";
                nullInds[NONUM] = Constant.DB_NULL;
            }
            else
            {
                sdNote.nonum = Marshal.PtrToStringAnsi(tgtValPtrs[NONUM]).Trim();
                nullInds[NONUM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NOTE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdNote.note = "";
                nullInds[NOTE] = Constant.DB_NULL;
            }
            else
            {
                sdNote.note = Marshal.PtrToStringAnsi(tgtValPtrs[NOTE]).Trim();
                nullInds[NOTE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdNote.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdNote.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdNote.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdNote.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_note](" +
    "[oper] [char](6) NOT NULL," +
    "[nonum] [char](4) NOT NULL," +
    "[note] [char](60) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_note] PRIMARY KEY CLUSTERED " +
"(" +
    "[oper] ASC," +
    "[nonum] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}
