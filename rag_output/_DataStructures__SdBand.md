# Documented File: SdBand.cs
**Repository Path:** `_DataStructures\SdBand.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace _DataStructures
{
    using _NewLib;
    using System.Collections.Generic;
    using System.Linq;
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class encapsulates the dataset of a frequency band and is isomorphic
    /// to the DB table <b>main.sd_band</b> .
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdBand
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BNDCDE_SZ)]
        public string bndcde;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short bandbitpos;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double blo;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double bmidf;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double bhi;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = BADJ)]
        public string badj;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        //--------------------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 8;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "bndcde", "bandbitpos", "blo", "bmidf", "bhi", "badj", "mdate", "mtime" };

        public const int BNDCDE = 0;
        public const int BANDBITPOS = 1;
        public const int BLO = 2;
        public const int BMIDF = 3;
        public const int BHI = 4;
        public const int BADJ = 5;
        public const int MDATE = 6;
        public const int MTIME = 7;

        public const int BNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int BADJ_SZ = Constant.BADJ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;

        //--------------------------------------------------------------------------

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
            sb.AppendLine("bndcde =     " + bndcde);
            sb.AppendLine("bandbitpos = " + bandbitpos);
            sb.AppendLine("blo =        " + blo);
            sb.AppendLine("bmidf =      " + bmidf);
            sb.AppendLine("bhi =        " + bhi);
            sb.AppendLine("badj =       " + badj);
            sb.AppendLine("mdate =      " + mdate);
            sb.AppendLine("mtime =      " + mtime);
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
            sb.Append("\n ===== SdBand ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "bndcde = " + bndcde);
            sb.Append("\n" + nullInds[i++] + "      " + "bandbitpos = " + bandbitpos);
            sb.Append("\n" + nullInds[i++] + "      " + "blo = " + blo);
            sb.Append("\n" + nullInds[i++] + "      " + "bmidf = " + bmidf);
            sb.Append("\n" + nullInds[i++] + "      " + "bhi = " + bhi);
            sb.Append("\n" + nullInds[i++] + "      " + "badj = " + badj);
            sb.Append("\n" + nullInds[i++] + "      " + "mdate = " + mdate);
            sb.Append("\n" + nullInds[i++] + "      " + "mtime = " + mtime);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an array of IntPtr that point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.sd_band.
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

            tgtValPtrs[BNDCDE] = Marshal.AllocHGlobal(BNDCDE_SZ + 1);
            nullIndPtrs[BNDCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, BNDCDE + 1, tgtValPtrs[BNDCDE], SdBand.BNDCDE_SZ, nullIndPtrs[BNDCDE]);

            tgtValPtrs[BANDBITPOS] = Marshal.AllocHGlobal(sizeof(short));
            nullIndPtrs[BANDBITPOS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToShort(hStmt, BANDBITPOS + 1, tgtValPtrs[BANDBITPOS], nullIndPtrs[BANDBITPOS]);

            tgtValPtrs[BLO] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[BLO] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, BLO + 1, tgtValPtrs[BLO], nullIndPtrs[BLO]);

            tgtValPtrs[BMIDF] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[BMIDF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, BMIDF + 1, tgtValPtrs[BMIDF], nullIndPtrs[BMIDF]);

            tgtValPtrs[BHI] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[BHI] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, BHI + 1, tgtValPtrs[BHI], nullIndPtrs[BHI]);

            tgtValPtrs[BADJ] = Marshal.AllocHGlobal(BADJ_SZ + 1);
            nullIndPtrs[BADJ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, BADJ + 1, tgtValPtrs[BADJ], SdBand.BADJ_SZ, nullIndPtrs[BADJ]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdBand.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdBand.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdBand object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdBand"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdBand sdBand, out SQLLEN[] nullInds)
        {
            sdBand = new SdBand();
            nullInds = NullHelper.CreateArrayOfNullInd(SdBand.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[BNDCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdBand.bndcde = "";
                nullInds[BNDCDE] = Constant.DB_NULL;
            }
            else
            {
                sdBand.bndcde = Marshal.PtrToStringAnsi(tgtValPtrs[BNDCDE]).Trim();
                nullInds[BNDCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BANDBITPOS]);
            if (nullInd != Constant.DB_NULL)
            {
                sdBand.bandbitpos = Marshal.ReadInt16(tgtValPtrs[BANDBITPOS]);
                nullInds[BANDBITPOS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BLO]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[BLO], D, 0, 1);
                sdBand.blo = D[0];
                nullInds[BLO] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BMIDF]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[BMIDF], D, 0, 1);
                sdBand.bmidf = D[0];
                nullInds[BMIDF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BHI]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[BHI], D, 0, 1);
                sdBand.bhi = D[0];
                nullInds[BHI] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BADJ]);
            if (nullInd == Constant.DB_NULL)
            {
                sdBand.badj = "";
                nullInds[BADJ] = Constant.DB_NULL;
            }
            else
            {
                sdBand.badj = Marshal.PtrToStringAnsi(tgtValPtrs[BADJ]).Trim();
                nullInds[BADJ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdBand.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdBand.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdBand.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdBand.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

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

            sb.Append(nullInds[SdBand.BNDCDE] == Constant.DB_NULL ? "NULL, " : "'" + bndcde.ToString() + "', ");
            sb.Append(nullInds[SdBand.BANDBITPOS] == Constant.DB_NULL ? "NULL, " : "'" + bandbitpos.ToString() + "', ");
            sb.Append(nullInds[SdBand.BLO] == Constant.DB_NULL ? "NULL, " : "'" + blo.ToString() + "', ");
            sb.Append(nullInds[SdBand.BMIDF] == Constant.DB_NULL ? "NULL, " : "'" + bmidf.ToString() + "', ");
            sb.Append(nullInds[SdBand.BHI] == Constant.DB_NULL ? "NULL, " : "'" + bhi.ToString() + "', ");
            sb.Append(nullInds[SdBand.BADJ] == Constant.DB_NULL ? "NULL, " : "'" + badj.ToString() + "', ");
            sb.Append(nullInds[SdBand.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdBand.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of an object formatted as a CSV string..
        /// </summary>
        /// <returns></returns>
        public string ToStringAsCSV()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(bndcde.ToString() + ", ");
            sb.Append(bandbitpos.ToString() + ", ");
            sb.Append(blo.ToString() + ", ");
            sb.Append(bmidf.ToString() + ", ");
            sb.Append(bhi.ToString() + ", ");
            sb.Append(badj.ToString() + ", ");
            sb.Append(mdate.ToString() + ", ");
            sb.Append(mtime.ToString() + " ");

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

            sb.Append(columnNames[SdBand.BNDCDE] + " = " + (nullInds[SdBand.BNDCDE] == Constant.DB_NULL ? "NULL, " : "'" + bndcde.ToString() + "', "));
            sb.Append(columnNames[SdBand.BANDBITPOS] + " = " + (nullInds[SdBand.BANDBITPOS] == Constant.DB_NULL ? "NULL, " : "'" + bandbitpos.ToString() + "', "));
            sb.Append(columnNames[SdBand.BLO] + " = " + (nullInds[SdBand.BLO] == Constant.DB_NULL ? "NULL, " : "'" + blo.ToString() + "', "));
            sb.Append(columnNames[SdBand.BMIDF] + " = " + (nullInds[SdBand.BMIDF] == Constant.DB_NULL ? "NULL, " : "'" + bmidf.ToString() + "', "));
            sb.Append(columnNames[SdBand.BHI] + " = " + (nullInds[SdBand.BHI] == Constant.DB_NULL ? "NULL, " : "'" + bhi.ToString() + "', "));
            sb.Append(columnNames[SdBand.BADJ] + " = " + (nullInds[SdBand.BADJ] == Constant.DB_NULL ? "NULL, " : "'" + badj.ToString() + "', "));
            sb.Append(columnNames[SdBand.MDATE] + " = " + (nullInds[SdBand.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdBand.MTIME] + " = " + (nullInds[SdBand.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdBand and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuBand object and its associated nullInds; 
        /// SdBand has identical members to SuBand except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suBand"></param>
        /// <param name="suBandNullInds"></param>
        /// <param name="sdBand"></param>
        /// <param name="sdBandNullInds"></param>
        public static void MakeSdFromSu(SuBand suBand, SQLLEN[] suBandNullInds, out SdBand sdBand, out SQLLEN[] sdBandNullInds)
        {
            sdBand = new SdBand();
            sdBandNullInds = new SQLLEN[NUM_COLUMNS];

            sdBand.bndcde = suBand.bndcde;
            sdBandNullInds[SdBand.BNDCDE] = suBandNullInds[SuBand.BNDCDE];

            sdBand.bandbitpos = suBand.bandbitpos;
            sdBandNullInds[SdBand.BANDBITPOS] = suBandNullInds[SuBand.BANDBITPOS];

            sdBand.blo = suBand.blo;
            sdBandNullInds[SdBand.BLO] = suBandNullInds[SuBand.BLO];

            sdBand.bmidf = suBand.bmidf;
            sdBandNullInds[SdBand.BMIDF] = suBandNullInds[SuBand.BMIDF];

            sdBand.bhi = suBand.bhi;
            sdBandNullInds[SdBand.BHI] = suBandNullInds[SuBand.BHI];

            sdBand.badj = suBand.badj;
            sdBandNullInds[SdBand.BADJ] = suBandNullInds[SuBand.BADJ];

            sdBand.mdate = suBand.mdate;
            sdBandNullInds[SdBand.MDATE] = suBandNullInds[SuBand.MDATE];

            sdBand.mtime = suBand.mtime;
            sdBandNullInds[SdBand.MTIME] = suBandNullInds[SuBand.MTIME];
        }

        /// <summary>
        /// This method returns a list of adjacent bndcde string values for this SdBand
        /// object, as fetched from the badj column of a main.sd_band table record.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAdjBandCodeList()
        {
            List<string> result = new List<string>();

            string[] badjBandCodeArray = this.badj.Split(';');

            // The final badj bndcde is right padded with spaces so Trim().
            // Also, it is possible that two or more ';' appear with no text between them, 
            // and is parsed (split) as an 'empty' field that could cause havoc later.
            for (int i = 0; i < badjBandCodeArray.Length; i++)
            {
                // Only add non-empty fields to the returned list.
                if (!String.IsNullOrWhiteSpace(badjBandCodeArray[i])) result.Add(badjBandCodeArray[i].Trim());
            }

            return result;
        }

        /// <summary>
        /// This method returns true if a prescribed frequency (in KHz) lies inside this 
        /// SdBand object's frequency interval [blo, bhi]; else false.
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        public bool IsInBand(double freq)
        {
            return (freq >= blo) && (freq <= bhi);
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_band](" +
    "[bndcde] [char](4) NOT NULL," +
    "[bandbitpos] [smallint] NULL," +
    "[blo] [float] NULL," +
    "[bmidf] [float] NULL," +
    "[bhi] [float] NULL," +
    "[badj] [char](99) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_band] PRIMARY KEY CLUSTERED " +
"(" +
    "[bndcde] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]" +
") ON [PRIMARY]";



    }
}


```
