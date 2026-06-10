# Documented File: SdAnte.cs
**Repository Path:** `_DataStructures\SdAnte.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _NewLib;
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
    /// This class has fields that are isomorphic with the columns of the SDB table 
    /// <b>main.sd_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdAnte
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODE_SZ)]
        public string acode;        //02
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int axtype;          //03
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AXREF_SZ)]
        public string axref;        //04
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float again;         //05
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float abw;           //06
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short arms;          //07
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ABAND_SZ)]
        public string aband;        //08
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AMANU_SZ)]
        public string amanu;        //09
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = APATTERN_SZ)]
        public string apattern;     //10
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = AMODEL_SZ)]
        public string amodel;       //11
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short anip;          //12
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float ax0;           //13
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ADESC_SZ)]
        public string adesc;        //14
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ANTYPE_SZ)]
        public string antype;       //15
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float aftbr;         //16
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lofreq;       //17
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double hifreq;       //18
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BANDCODES_SZ)]
        public string bandcodes;    //19
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;        //20
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;        //21

        //--------------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 20;

        public const int ACODE = 0;
        public const int AXTYPE = 1;
        public const int AXREF = 2;
        public const int AGAIN = 3;
        public const int ABW = 4;
        public const int ARMS = 5;
        public const int ABAND = 6;
        public const int AMANU = 7;
        public const int APATTERN = 8;
        public const int AMODEL = 9;
        public const int ANIP = 10;
        public const int AX0 = 11;
        public const int ADESC = 12;
        public const int ANTYPE = 13;
        public const int AFTBR = 14;
        public const int LOFREQ = 15;
        public const int HIFREQ = 16;
        public const int BANDCODES = 17;
        public const int MDATE = 18;
        public const int MTIME = 19;

        public const int ACODE_SZ = Constant.ACODE_SZ;
        public const int AXREF_SZ = Constant.ACODE_SZ;
        public const int ABAND_SZ = Constant.ABAND_SZ;
        public const int AMANU_SZ = Constant.AMANU_SZ;
        public const int APATTERN_SZ = Constant.ACODE_SZ;
        public const int AMODEL_SZ = Constant.ANTE_MODEL_SZ;
        public const int ADESC_SZ = Constant.ADESC_SZ;
        public const int ANTYPE_SZ = Constant.ANTYPE_SZ;
        public const int BANDCODES_SZ = Constant.BANDCODES_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "acode", "axtype", "axref", "again", "abw", "arms", "aband", "amanu", "apattern", "amodel", "anip", "ax0", "adesc", "antype", "aftbr", "lofreq", "hifreq", "bandcodes", "mdate", "mtime" };

        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        private static string mAllBindingsAsCSV;

        private static string mAllFieldsAsCSV;

        //Provide read-only access to specific members.
        public static string AllColumnsForSqlSelect { get { return mAllFieldsAsCSV; } }

        public static string AllColumnsForSqlUpdateAsBindings { get { return mAllBindingsAsCSV; } }

        //----------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static SdAnte()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();
        }

        /// <summary>
        /// Per-instance constructor.
        /// </summary>
        public SdAnte()
        {
            acode = "";
            axref = "";
            aband = "";
            amanu = "";
            apattern = "";
            amodel = "";
            adesc = "";
            antype = "";
            bandcodes = "";
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
            sb.Append("\r\n");
            sb.AppendLine("acode =    " + acode);
            sb.AppendLine("axtype =    " + axtype);
            sb.AppendLine("axref =   " + axref);
            sb.AppendLine("again =     " + again);
            sb.AppendLine("abw =     " + abw);
            sb.AppendLine("arms =    " + arms);
            sb.AppendLine("aband =      " + aband);
            sb.AppendLine("amanu =    " + amanu);
            sb.AppendLine("apattern =    " + apattern);
            sb.AppendLine("amodel =     " + amodel);
            sb.AppendLine("anip =   " + anip);
            sb.AppendLine("ax0 =   " + ax0);
            sb.AppendLine("adesc =   " + adesc);
            sb.AppendLine("antype =    " + antype);
            sb.AppendLine("aftbr = " + aftbr);
            sb.AppendLine("lofreq = " + lofreq);
            sb.AppendLine("hifreq = " + hifreq);
            sb.AppendLine("bandcodes = " + bandcodes);
            sb.AppendLine("mdate =    " + mdate);
            sb.AppendLine("mtime =    " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a CSV string that shows the current values of a subset
        /// of the member variables of this SdAnte object.
        /// </summary>
        /// <returns></returns>
        public string ToStringTerse()
        {
            return String.Format("{0,12}, {1,4:F1}, {2,3:F1}, {3,10}, {4,12}, {5,15}, {6,20}", acode, again, abw, amanu, apattern, amodel, adesc);
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
            sb.AppendLine("\r\n");

            int n = 0;
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acode =     " + acode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "axtype =    " + axtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "axref =     " + axref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "again =     " + again);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "abw =       " + abw);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "arms =      " + arms);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aband =     " + aband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "amanu =     " + amanu);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "apattern =  " + apattern);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "amodel =    " + amodel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "anip =      " + anip);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ax0 =       " + ax0);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adesc =     " + adesc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "antype =    " + antype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aftbr =     " + aftbr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "lofreq =    " + lofreq);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "hifreq =    " + hifreq);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bandcodes = " + bandcodes);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =     " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =     " + mtime);

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
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float and double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[ACODE] = Marshal.StringToHGlobalAnsi(acode);

            parameterValuePtr[AXTYPE] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[AXTYPE], axtype);

            parameterValuePtr[AXREF] = Marshal.StringToHGlobalAnsi(axref);

            parameterValuePtr[AGAIN] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = again;
            Marshal.Copy(F, 0, parameterValuePtr[AGAIN], 1);

            parameterValuePtr[ABW] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = abw;
            Marshal.Copy(F, 0, parameterValuePtr[ABW], 1);

            parameterValuePtr[ARMS] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ARMS], arms);

            parameterValuePtr[ABAND] = Marshal.StringToHGlobalAnsi(aband);

            parameterValuePtr[AMANU] = Marshal.StringToHGlobalAnsi(amanu);

            parameterValuePtr[APATTERN] = Marshal.StringToHGlobalAnsi(apattern);

            parameterValuePtr[AMODEL] = Marshal.StringToHGlobalAnsi(amodel);

            parameterValuePtr[ANIP] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ANIP], anip);

            parameterValuePtr[AX0] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = ax0;
            Marshal.Copy(F, 0, parameterValuePtr[AX0], 1);

            parameterValuePtr[ADESC] = Marshal.StringToHGlobalAnsi(adesc);

            parameterValuePtr[ANTYPE] = Marshal.StringToHGlobalAnsi(antype);

            parameterValuePtr[AFTBR] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = aftbr;
            Marshal.Copy(F, 0, parameterValuePtr[AFTBR], 1);

            parameterValuePtr[LOFREQ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = lofreq;
            Marshal.Copy(D, 0, parameterValuePtr[LOFREQ], 1);

            parameterValuePtr[HIFREQ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = hifreq;
            Marshal.Copy(D, 0, parameterValuePtr[HIFREQ], 1);

            parameterValuePtr[BANDCODES] = Marshal.StringToHGlobalAnsi(bandcodes);

            parameterValuePtr[MDATE] = Marshal.StringToHGlobalAnsi(mdate);

            parameterValuePtr[MTIME] = Marshal.StringToHGlobalAnsi(mtime);

            return parameterValuePtr;
        }

        /// <summary>
        /// This method returns an IntPtr[] whose elements point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.sd_ante; the ODBC binding
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

            tgtValPtrs[ACODE] = Marshal.AllocHGlobal(ACODE_SZ + 1);
            nullIndPtrs[ACODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACODE + 1, tgtValPtrs[ACODE], SdAnte.ACODE_SZ, nullIndPtrs[ACODE]);

            tgtValPtrs[AXTYPE] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[AXTYPE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, AXTYPE + 1, tgtValPtrs[AXTYPE], nullIndPtrs[AXTYPE]);

            tgtValPtrs[AXREF] = Marshal.AllocHGlobal(AXREF_SZ + 1);
            nullIndPtrs[AXREF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, AXREF + 1, tgtValPtrs[AXREF], SdAnte.AXREF_SZ, nullIndPtrs[AXREF]);

            tgtValPtrs[AGAIN] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AGAIN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AGAIN + 1, tgtValPtrs[AGAIN], nullIndPtrs[AGAIN]);

            tgtValPtrs[ABW] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[ABW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, ABW + 1, tgtValPtrs[ABW], nullIndPtrs[ABW]);

            tgtValPtrs[ARMS] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ARMS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ARMS + 1, tgtValPtrs[ARMS], nullIndPtrs[ARMS]);

            tgtValPtrs[ABAND] = Marshal.AllocHGlobal(ABAND_SZ + 1);
            nullIndPtrs[ABAND] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ABAND + 1, tgtValPtrs[ABAND], SdAnte.ABAND_SZ, nullIndPtrs[ABAND]);

            tgtValPtrs[AMANU] = Marshal.AllocHGlobal(AMANU_SZ + 1);
            nullIndPtrs[AMANU] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, AMANU + 1, tgtValPtrs[AMANU], SdAnte.AMANU_SZ, nullIndPtrs[AMANU]);

            tgtValPtrs[APATTERN] = Marshal.AllocHGlobal(APATTERN_SZ + 1);
            nullIndPtrs[APATTERN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, APATTERN + 1, tgtValPtrs[APATTERN], SdAnte.APATTERN_SZ, nullIndPtrs[APATTERN]);

            tgtValPtrs[AMODEL] = Marshal.AllocHGlobal(AMODEL_SZ + 1);
            nullIndPtrs[AMODEL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, AMODEL + 1, tgtValPtrs[AMODEL], SdAnte.AMODEL_SZ, nullIndPtrs[AMODEL]);

            tgtValPtrs[ANIP] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANIP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANIP + 1, tgtValPtrs[ANIP], nullIndPtrs[ANIP]);

            tgtValPtrs[AX0] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AX0] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AX0 + 1, tgtValPtrs[AX0], nullIndPtrs[AX0]);

            tgtValPtrs[ADESC] = Marshal.AllocHGlobal(ADESC_SZ + 1);
            nullIndPtrs[ADESC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ADESC + 1, tgtValPtrs[ADESC], SdAnte.ADESC_SZ, nullIndPtrs[ADESC]);

            tgtValPtrs[ANTYPE] = Marshal.AllocHGlobal(ANTYPE_SZ + 1);
            nullIndPtrs[ANTYPE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANTYPE + 1, tgtValPtrs[ANTYPE], SdAnte.ANTYPE_SZ, nullIndPtrs[ANTYPE]);

            tgtValPtrs[AFTBR] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AFTBR] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AFTBR + 1, tgtValPtrs[AFTBR], nullIndPtrs[AFTBR]);

            tgtValPtrs[LOFREQ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LOFREQ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LOFREQ + 1, tgtValPtrs[LOFREQ], nullIndPtrs[LOFREQ]);

            tgtValPtrs[HIFREQ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[HIFREQ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, HIFREQ + 1, tgtValPtrs[HIFREQ], nullIndPtrs[HIFREQ]);

            tgtValPtrs[BANDCODES] = Marshal.AllocHGlobal(BANDCODES_SZ + 1);
            nullIndPtrs[BANDCODES] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, BANDCODES + 1, tgtValPtrs[BANDCODES], SdAnte.BANDCODES_SZ, nullIndPtrs[BANDCODES]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdAnte.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdAnte.MTIME_SZ, nullIndPtrs[MTIME]);
        }

        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdAnte object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdAnte"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdAnte sdAnte, out SQLLEN[] nullInds)
        {
            sdAnte = new SdAnte();
            nullInds = NullHelper.CreateArrayOfNullInd(SdAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.acode = "";
                nullInds[ACODE] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.acode = Marshal.PtrToStringAnsi(tgtValPtrs[ACODE]).Trim();
                nullInds[ACODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AXTYPE]);
            if (nullInd != Constant.DB_NULL)
            {
                sdAnte.axtype = Marshal.ReadInt32(tgtValPtrs[AXTYPE]);
                nullInds[AXTYPE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AXREF]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.axref = "";
                nullInds[AXREF] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.axref = Marshal.PtrToStringAnsi(tgtValPtrs[AXREF]).Trim();
                nullInds[AXREF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AGAIN]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AGAIN], F, 0, 1);
                sdAnte.again = F[0];
                nullInds[AGAIN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ABW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ABW], F, 0, 1);
                sdAnte.abw = F[0];
                nullInds[ABW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ARMS]);
            if (nullInd != Constant.DB_NULL)
            {
                sdAnte.arms = Marshal.ReadInt16(tgtValPtrs[ARMS]);
                nullInds[ARMS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ABAND]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.aband = "";
                nullInds[ABAND] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.aband = Marshal.PtrToStringAnsi(tgtValPtrs[ABAND]).Trim();
                nullInds[ABAND] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AMANU]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.amanu = "";
                nullInds[AMANU] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.amanu = Marshal.PtrToStringAnsi(tgtValPtrs[AMANU]).Trim();
                nullInds[AMANU] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[APATTERN]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.apattern = "";
                nullInds[APATTERN] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.apattern = Marshal.PtrToStringAnsi(tgtValPtrs[APATTERN]).Trim();
                nullInds[APATTERN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AMODEL]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.amodel = "";
                nullInds[AMODEL] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.amodel = Marshal.PtrToStringAnsi(tgtValPtrs[AMODEL]).Trim();
                nullInds[AMODEL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANIP]);
            if (nullInd != Constant.DB_NULL)
            {
                sdAnte.anip = Marshal.ReadInt16(tgtValPtrs[ANIP]);
                nullInds[ANIP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AX0]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AX0], F, 0, 1);
                sdAnte.ax0 = F[0];
                nullInds[AX0] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ADESC]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.adesc = "";
                nullInds[ADESC] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.adesc = Marshal.PtrToStringAnsi(tgtValPtrs[ADESC]).Trim();
                nullInds[ADESC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTYPE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.antype = "";
                nullInds[ANTYPE] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.antype = Marshal.PtrToStringAnsi(tgtValPtrs[ANTYPE]).Trim();
                nullInds[ANTYPE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AFTBR]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AFTBR], F, 0, 1);
                sdAnte.aftbr = F[0];
                nullInds[AFTBR] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LOFREQ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LOFREQ], D, 0, 1);
                sdAnte.lofreq = D[0];
                nullInds[LOFREQ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[HIFREQ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[HIFREQ], D, 0, 1);
                sdAnte.hifreq = D[0];
                nullInds[HIFREQ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDCODES]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.bandcodes = "";
                nullInds[BANDCODES] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.bandcodes = Marshal.PtrToStringAnsi(tgtValPtrs[BANDCODES]).Trim();
                nullInds[BANDCODES] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdAnte.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdAnte.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

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

            sb.Append(columnNames[SdAnte.ACODE] + "=" + (nullInds[SdAnte.ACODE] == Constant.DB_NULL ? "NULL," : "'" + acode.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AXTYPE] + "=" + (nullInds[SdAnte.AXTYPE] == Constant.DB_NULL ? "NULL," : "'" + axtype.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AXREF] + "=" + (nullInds[SdAnte.AXREF] == Constant.DB_NULL ? "NULL," : "'" + axref.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AGAIN] + "=" + (nullInds[SdAnte.AGAIN] == Constant.DB_NULL ? "NULL," : "'" + again.ToString() + "',"));
            sb.Append(columnNames[SdAnte.ABW] + "=" + (nullInds[SdAnte.ABW] == Constant.DB_NULL ? "NULL," : "'" + abw.ToString() + "',"));
            sb.Append(columnNames[SdAnte.ARMS] + "=" + (nullInds[SdAnte.ARMS] == Constant.DB_NULL ? "NULL," : "'" + arms.ToString() + "',"));
            sb.Append(columnNames[SdAnte.ABAND] + "=" + (nullInds[SdAnte.ABAND] == Constant.DB_NULL ? "NULL," : "'" + aband.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AMANU] + "=" + (nullInds[SdAnte.AMANU] == Constant.DB_NULL ? "NULL," : "'" + amanu.ToString() + "',"));
            sb.Append(columnNames[SdAnte.APATTERN] + "=" + (nullInds[SdAnte.APATTERN] == Constant.DB_NULL ? "NULL," : "'" + apattern.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AMODEL] + "=" + (nullInds[SdAnte.AMODEL] == Constant.DB_NULL ? "NULL," : "'" + amodel.ToString() + "',"));
            sb.Append(columnNames[SdAnte.ANIP] + "=" + (nullInds[SdAnte.ANIP] == Constant.DB_NULL ? "NULL," : "'" + anip.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AX0] + "=" + (nullInds[SdAnte.AX0] == Constant.DB_NULL ? "NULL," : "'" + ax0.ToString() + "',"));
            sb.Append(columnNames[SdAnte.ADESC] + "=" + (nullInds[SdAnte.ADESC] == Constant.DB_NULL ? "NULL," : "'" + adesc.ToString() + "',"));
            sb.Append(columnNames[SdAnte.ANTYPE] + "=" + (nullInds[SdAnte.ANTYPE] == Constant.DB_NULL ? "NULL," : "'" + antype.ToString() + "',"));
            sb.Append(columnNames[SdAnte.AFTBR] + "=" + (nullInds[SdAnte.AFTBR] == Constant.DB_NULL ? "NULL," : "'" + aftbr.ToString() + "',"));
            sb.Append(columnNames[SdAnte.LOFREQ] + "=" + (nullInds[SdAnte.LOFREQ] == Constant.DB_NULL ? "NULL," : "'" + lofreq.ToString() + "',"));
            sb.Append(columnNames[SdAnte.HIFREQ] + "=" + (nullInds[SdAnte.HIFREQ] == Constant.DB_NULL ? "NULL," : "'" + hifreq.ToString() + "',"));
            sb.Append(columnNames[SdAnte.BANDCODES] + "=" + (nullInds[SdAnte.BANDCODES] == Constant.DB_NULL ? "NULL," : "'" + bandcodes.ToString() + "',"));
            sb.Append(columnNames[SdAnte.MDATE] + "=" + (nullInds[SdAnte.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[SdAnte.MTIME] + "=" + (nullInds[SdAnte.MTIME] == Constant.DB_NULL ? "NULL" : "'" + mtime.ToString() + "'"));

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

            sb.Append(nullInds[SdAnte.ACODE] == Constant.DB_NULL ? "NULL," : "'" + acode.ToString() + "',");
            sb.Append(nullInds[SdAnte.AXTYPE] == Constant.DB_NULL ? "NULL," : "'" + axtype.ToString() + "',");
            sb.Append(nullInds[SdAnte.AXREF] == Constant.DB_NULL ? "NULL," : "'" + axref.ToString() + "',");
            sb.Append(nullInds[SdAnte.AGAIN] == Constant.DB_NULL ? "NULL," : "'" + again.ToString() + "',");
            sb.Append(nullInds[SdAnte.ABW] == Constant.DB_NULL ? "NULL," : "'" + abw.ToString() + "',");
            sb.Append(nullInds[SdAnte.ARMS] == Constant.DB_NULL ? "NULL," : "'" + arms.ToString() + "',");
            sb.Append(nullInds[SdAnte.ABAND] == Constant.DB_NULL ? "NULL," : "'" + aband.ToString() + "',");
            sb.Append(nullInds[SdAnte.AMANU] == Constant.DB_NULL ? "NULL," : "'" + amanu.ToString() + "',");
            sb.Append(nullInds[SdAnte.APATTERN] == Constant.DB_NULL ? "NULL," : "'" + apattern.ToString() + "',");
            sb.Append(nullInds[SdAnte.AMODEL] == Constant.DB_NULL ? "NULL," : "'" + amodel.ToString() + "',");
            sb.Append(nullInds[SdAnte.ANIP] == Constant.DB_NULL ? "NULL," : "'" + anip.ToString() + "',");
            sb.Append(nullInds[SdAnte.AX0] == Constant.DB_NULL ? "NULL," : "'" + ax0.ToString() + "',");
            sb.Append(nullInds[SdAnte.ADESC] == Constant.DB_NULL ? "NULL," : "'" + adesc.ToString() + "',");
            sb.Append(nullInds[SdAnte.ANTYPE] == Constant.DB_NULL ? "NULL," : "'" + antype.ToString() + "',");
            sb.Append(nullInds[SdAnte.AFTBR] == Constant.DB_NULL ? "NULL," : "'" + aftbr.ToString() + "',");
            sb.Append(nullInds[SdAnte.LOFREQ] == Constant.DB_NULL ? "NULL," : "'" + lofreq.ToString() + "',");
            sb.Append(nullInds[SdAnte.HIFREQ] == Constant.DB_NULL ? "NULL," : "'" + hifreq.ToString() + "',");
            sb.Append(nullInds[SdAnte.BANDCODES] == Constant.DB_NULL ? "NULL," : "'" + bandcodes.ToString() + "',");
            sb.Append(nullInds[SdAnte.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[SdAnte.MTIME] == Constant.DB_NULL ? "NULL" : "'" + mtime.ToString() + "'");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdAnte and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuAnte object and its associated nullInds; 
        /// SdAnte has identical members to SuAntd except for 'cmd'  and 'recstat'.
        /// </summary>
        /// <param name="suAnte"></param>
        /// <param name="suAnteNullInds"></param>
        /// <param name="sdAnte"></param>
        /// <param name="sdAnteNullInds"></param>
        public static void MakeSdFromSu(SuAnte suAnte, SQLLEN[] suAnteNullInds, out SdAnte sdAnte, out SQLLEN[] sdAnteNullInds)
        {
            // SdAnte has identical members to SuAnte except for 'cmd' and 'recstat'.

            sdAnte = new SdAnte();
            sdAnteNullInds = new SQLLEN[SdAnte.NUM_COLUMNS];

            sdAnte.acode = suAnte.acode;
            sdAnteNullInds[SdAnte.ACODE] = suAnteNullInds[SuAnte.ACODE];

            sdAnte.axtype = suAnte.axtype;
            sdAnteNullInds[SdAnte.AXTYPE] = suAnteNullInds[SuAnte.AXTYPE];

            sdAnte.axref = suAnte.axref;
            sdAnteNullInds[SdAnte.AXREF] = suAnteNullInds[SuAnte.AXREF];

            sdAnte.again = suAnte.again;
            sdAnteNullInds[SdAnte.AGAIN] = suAnteNullInds[SuAnte.AGAIN];

            sdAnte.abw = suAnte.abw;
            sdAnteNullInds[SdAnte.ABW] = suAnteNullInds[SuAnte.ABW];

            sdAnte.arms = suAnte.arms;
            sdAnteNullInds[SdAnte.ARMS] = suAnteNullInds[SuAnte.ARMS];

            sdAnte.aband = suAnte.aband;
            sdAnteNullInds[SdAnte.ABAND] = suAnteNullInds[SuAnte.ABAND];

            sdAnte.amanu = suAnte.amanu;
            sdAnteNullInds[SdAnte.AMANU] = suAnteNullInds[SuAnte.AMANU];

            sdAnte.apattern = suAnte.apattern;
            sdAnteNullInds[SdAnte.APATTERN] = suAnteNullInds[SuAnte.APATTERN];

            sdAnte.amodel = suAnte.amodel;
            sdAnteNullInds[SdAnte.AMODEL] = suAnteNullInds[SuAnte.AMODEL];

            sdAnte.anip = suAnte.anip;
            sdAnteNullInds[SdAnte.ANIP] = suAnteNullInds[SuAnte.ANIP];

            sdAnte.ax0 = suAnte.ax0;
            sdAnteNullInds[SdAnte.AX0] = suAnteNullInds[SuAnte.AX0];

            sdAnte.adesc = suAnte.adesc;
            sdAnteNullInds[SdAnte.ADESC] = suAnteNullInds[SuAnte.ADESC];

            sdAnte.antype = suAnte.antype;
            sdAnteNullInds[SdAnte.ANTYPE] = suAnteNullInds[SuAnte.ANTYPE];

            sdAnte.aftbr = suAnte.aftbr;
            sdAnteNullInds[SdAnte.AFTBR] = suAnteNullInds[SuAnte.AFTBR];

            sdAnte.lofreq = suAnte.lofreq;
            sdAnteNullInds[SdAnte.LOFREQ] = suAnteNullInds[SuAnte.LOFREQ];

            sdAnte.hifreq = suAnte.hifreq;
            sdAnteNullInds[SdAnte.HIFREQ] = suAnteNullInds[SuAnte.HIFREQ];

            sdAnte.bandcodes = suAnte.bandcodes;
            sdAnteNullInds[SdAnte.BANDCODES] = suAnteNullInds[SuAnte.BANDCODES];

            sdAnte.mdate = suAnte.mdate;
            sdAnteNullInds[SdAnte.MDATE] = suAnteNullInds[SuAnte.MDATE];

            sdAnte.mtime = suAnte.mtime;
            sdAnteNullInds[SdAnte.MTIME] = suAnteNullInds[SuAnte.MTIME];
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_ante](" +
    "[acode] [char](12) NOT NULL," +
    "[axtype] [int] NULL," +
    "[axref] [char](12) NULL," +
    "[again] [real] NULL," +
    "[abw] [real] NULL," +
    "[arms] [tinyint] NULL," +
    "[aband] [char](10) NULL," +
    "[amanu] [char](10) NULL," +
    "[apattern] [char](12) NULL," +
    "[amodel] [char](15) NULL," +
    "[anip] [smallint] NULL," +
    "[ax0] [real] NULL," +
    "[adesc] [char](20) NULL," +
    "[antype] [char](8) NULL," +
    "[aftbr] [real] NULL," +
    "[lofreq] [float] NULL," +
    "[hifreq] [float] NULL," +
    "[bandcodes] [char](99) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
 "CONSTRAINT [PK_sd_ante] PRIMARY KEY CLUSTERED " +
"(" +
    "[acode] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";






    }
}


```
