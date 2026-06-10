# Documented File: MtPhantomTxLink.cs
**Repository Path:** `_DataStructures\MtPhantomTxLink.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _NewLib;
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
    using SQLUINTEGER = UInt32;
    using _Configuration;
    public class MtPhantomTxLink
    {
        //==================================================================================
        // Non-static members.
        //==================================================================================
        public string problemCode;      // This is a single character 'problem code' (L, A or F)
        public string oper;             // mt_site.oper
        public string call1;            // mt_site.call1
        public string call2;            // mt.ante.call2
        public string bndcde;           // mt_ante.bndcde
        public string chid;             // mt_chan.chid
        public short anum;              // mt_ante.anum (= mt_chan.antnumbtx1 for a Tx Link)
        public double mdbLat;           // mt_site.latit  / 36000.0
        public double mdbLng;           // mt_site.longit / 36000.0
        public string prov;             // mt_site.prov
        public float azmth;             // mt.ante.azmth
        public double freqtx;           // mt_chan.freqtx
        public float aht;               // mt_ante.aht

        //==================================================================================
        // The total number of fields corresponding to database columns.
        //==================================================================================
        public const int NUM_COLUMNS = 13;

        //==================================================================================
        // Array of strings providing the class-member / database-column names.
        //
        // Note: 
        //       - the member name 'TxRx' corresponds to the SQL table column name 'TXRX';
        //       - the member name 'Keyfield' corresponds to the SQL table column name 'keyfield';
        //       - all the other member names are identical to their SQL table column name.
        //==================================================================================
        private static string[] columnNames = new string[NUM_COLUMNS] {
                    "problemCode",
                    "oper",
                    "call1",
                    "call2",
                    "bndcde",
                    "chid",
                    "anum",
                    "mdbLat",
                    "mdbLng",
                    "prov",
                    "azmth",
                    "freqtx",
                    "aht"
            };

        //List of all column names for SQL 'select' command, i.e. " cmd, recstat, call1, call2, bndcde, anum, ... , mtime " 
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
        static MtPhantomTxLink()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public MtPhantomTxLink()
        {
        }


        //==================================================================================
        // Zero-based index.
        //==================================================================================

        public const int PROBLEMCODE = 0;
        public const int OPER = 1;
        public const int CALL1 = 2;
        public const int CALL2 = 3;
        public const int BNDCDE = 4;
        public const int CHID = 5;
        public const int ANUM = 6;
        public const int MDBLAT = 7;
        public const int MDBLNG = 8;
        public const int PROV = 9;
        public const int AZMTH = 10;
        public const int FREQTX = 11;
        public const int AHT = 12;

        //==================================================================================
        // String sizes.
        //==================================================================================
        public const int PROBLEMCODE_SZ = 2;
        public const int OPER_SZ = 7;
        public const int CALL1_SZ = 10;
        public const int CALL2_SZ = 10;
        public const int BNDCDE_SZ = 5;
        public const int CHID_SZ = 5;
        public const int PROV_SZ = 3;

        //==================================================================================
        // Methods.
        //==================================================================================

        /// <summary>
        /// This method returns an array of IntPtr that point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.mt_ante.
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

            tgtValPtrs[PROBLEMCODE] = Marshal.AllocHGlobal(PROBLEMCODE_SZ + 1);
            nullIndPtrs[PROBLEMCODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PROBLEMCODE + 1, tgtValPtrs[PROBLEMCODE], MtPhantomTxLink.PROBLEMCODE_SZ, nullIndPtrs[PROBLEMCODE]);

            tgtValPtrs[OPER] = Marshal.AllocHGlobal(OPER_SZ + 1);
            nullIndPtrs[OPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPER + 1, tgtValPtrs[OPER], MtPhantomTxLink.OPER_SZ, nullIndPtrs[OPER]);

            tgtValPtrs[CALL1] = Marshal.AllocHGlobal(CALL1_SZ + 1);
            nullIndPtrs[CALL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALL1 + 1, tgtValPtrs[CALL1], MtPhantomTxLink.CALL1_SZ, nullIndPtrs[CALL1]);

            tgtValPtrs[CALL2] = Marshal.AllocHGlobal(CALL2_SZ + 1);
            nullIndPtrs[CALL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALL2 + 1, tgtValPtrs[CALL2], MtPhantomTxLink.CALL2_SZ, nullIndPtrs[CALL2]);

            tgtValPtrs[BNDCDE] = Marshal.AllocHGlobal(BNDCDE_SZ + 1);
            nullIndPtrs[BNDCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, BNDCDE + 1, tgtValPtrs[BNDCDE], MtPhantomTxLink.BNDCDE_SZ, nullIndPtrs[BNDCDE]);

            tgtValPtrs[CHID] = Marshal.AllocHGlobal(CHID_SZ + 1);
            nullIndPtrs[CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CHID + 1, tgtValPtrs[CHID], MtPhantomTxLink.CHID_SZ, nullIndPtrs[CHID]);

            tgtValPtrs[ANUM] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[ANUM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, ANUM + 1, tgtValPtrs[ANUM], nullIndPtrs[ANUM]);

            tgtValPtrs[MDBLAT] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[MDBLAT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, MDBLAT + 1, tgtValPtrs[MDBLAT], nullIndPtrs[MDBLAT]);

            tgtValPtrs[MDBLNG] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[MDBLNG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, MDBLNG + 1, tgtValPtrs[MDBLNG], nullIndPtrs[MDBLNG]);

            tgtValPtrs[PROV] = Marshal.AllocHGlobal(PROV_SZ + 1);
            nullIndPtrs[PROV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PROV + 1, tgtValPtrs[PROV], MtPhantomTxLink.PROV_SZ, nullIndPtrs[PROV]);

            tgtValPtrs[AZMTH] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AZMTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AZMTH + 1, tgtValPtrs[AZMTH], nullIndPtrs[AZMTH]);

            tgtValPtrs[FREQTX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[FREQTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, FREQTX + 1, tgtValPtrs[FREQTX], nullIndPtrs[FREQTX]);

            tgtValPtrs[AHT] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[AHT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, AHT + 1, tgtValPtrs[AHT], nullIndPtrs[AHT]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned MtAnte object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="mtPhantomTxLink"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out MtPhantomTxLink mtPhantomTxLink, out SQLLEN[] nullInds)
        {
            mtPhantomTxLink = new MtPhantomTxLink();
            nullInds = NullHelper.CreateArrayOfNullInd(MtPhantomTxLink.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[PROBLEMCODE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.problemCode = "";
                nullInds[PROBLEMCODE] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.problemCode = Marshal.PtrToStringAnsi(tgtValPtrs[PROBLEMCODE]).Trim();
                nullInds[PROBLEMCODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.oper = "";
                nullInds[OPER] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.oper = Marshal.PtrToStringAnsi(tgtValPtrs[OPER]).Trim();
                nullInds[OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL1]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.call1 = "";
                nullInds[CALL1] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.call1 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL1]).Trim();
                nullInds[CALL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALL2]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.call2 = "";
                nullInds[CALL2] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.call2 = Marshal.PtrToStringAnsi(tgtValPtrs[CALL2]).Trim();
                nullInds[CALL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BNDCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.bndcde = "";
                nullInds[BNDCDE] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.bndcde = Marshal.PtrToStringAnsi(tgtValPtrs[BNDCDE]).Trim();
                nullInds[BNDCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.chid = "";
                nullInds[CHID] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.chid = Marshal.PtrToStringAnsi(tgtValPtrs[CHID]).Trim();
                nullInds[CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANUM]);
            if (nullInd != Constant.DB_NULL)
            {
                mtPhantomTxLink.anum = Marshal.ReadInt16(tgtValPtrs[ANUM]);
                nullInds[ANUM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDBLAT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[MDBLAT], D, 0, 1);
                mtPhantomTxLink.mdbLat = D[0];
                nullInds[MDBLAT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDBLNG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[MDBLNG], D, 0, 1);
                mtPhantomTxLink.mdbLng = D[0];
                nullInds[MDBLNG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PROV]);
            if (nullInd == Constant.DB_NULL)
            {
                mtPhantomTxLink.prov = "";
                nullInds[PROV] = Constant.DB_NULL;
            }
            else
            {
                mtPhantomTxLink.prov = Marshal.PtrToStringAnsi(tgtValPtrs[PROV]).Trim();
                nullInds[PROV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AZMTH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AZMTH], F, 0, 1);
                mtPhantomTxLink.azmth = F[0];
                nullInds[AZMTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FREQTX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[FREQTX], D, 0, 1);
                mtPhantomTxLink.freqtx = D[0];
                nullInds[FREQTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AHT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AHT], F, 0, 1);
                mtPhantomTxLink.aht = F[0];
                nullInds[AHT] = Constant.DB_NOT_NULL;
            }

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
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== MtPhantomTxLink =====");

            sb.Append("\nproblemCode = " + problemCode);
            sb.Append("\noper = " + oper);
            sb.Append("\ncall1 = " + call1);
            sb.Append("\ncall2 = " + call2);
            sb.Append("\nbndcde = " + bndcde);
            sb.Append("\nchid = " + chid);
            sb.Append("\nanum = " + anum);
            sb.Append("\nmdbLat = " + mdbLat);
            sb.Append("\nmdbLng = " + mdbLng);
            sb.Append("\nprov = " + prov);
            sb.Append("\nazmth = " + azmth);
            sb.Append("\nfreqtx = " + freqtx);
            sb.Append("\naht = " + aht);

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
            int n = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== MtPhantomTxLink =====");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "problemCode =      " + problemCode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oper =      " + oper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 =      " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call2 =      " + call2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "bndcde =      " + bndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "chid =      " + chid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "anum =      " + anum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdbLat =      " + mdbLat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdbLng =      " + mdbLng);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "prov =      " + prov);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "azmth =      " + azmth);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqtx =      " + freqtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aht =      " + aht);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a CSV formatted string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public string ToCSVstring()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(problemCode);
            sb.Append("," + oper);
            sb.Append("," + call1);
            sb.Append("," + call2);
            sb.Append("," + bndcde);
            sb.Append("," + chid);
            sb.Append("," + anum);
            sb.Append("," + mdbLat);
            sb.Append("," + mdbLng);
            sb.Append("," + prov);
            sb.Append("," + azmth);
            sb.Append("," + freqtx);
            sb.Append("," + aht);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string providing a list of the column names
        /// as can be used in a SQL SELECT query.
        /// </summary>
        /// <returns></returns>
        public static string ColumnNames()
        {
            return AllColumnsForSqlSelect;
        }


        /*
        problemCode
        oper
        call1
        call2
        bndcde
        chid
        anum
        mdbLat
        mdbLng
        prov
        azmth
        freqtx
        aht
        */




    }
}

```
