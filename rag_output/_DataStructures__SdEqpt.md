# Documented File: SdEqpt.cs
**Repository Path:** `_DataStructures\SdEqpt.cs`
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
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLHANDLE = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with the SDF table 
    /// <b>main.sd_eqpt</b>  .
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class SdEqpt
    {
        // IMPORTANT!
        // =========
        // The following qty. 15 member values correspond to the legacy native 
        // structure sdEqpt_; member names are prescribed to be identical.
        // The order of appearance of these qty. 15 field members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

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
        public const int NUM_COLUMNS = 15;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "ecode", "estab", "exref", "emanu", "emodel", "edesc", "etype", "etraf", "emission", "e1stif", "e2ndif", "thhold", "ebndcde", "mdate", "mtime" };

        public const int ECODE = 0;
        public const int ESTAB = 1;
        public const int EXREF = 2;
        public const int EMANU = 3;
        public const int EMODEL = 4;
        public const int EDESC = 5;
        public const int ETYPE = 6;
        public const int ETRAF = 7;
        public const int EMISSION = 8;
        public const int E1STIF = 9;
        public const int E2NDIF = 10;
        public const int THHOLD = 11;
        public const int EBNDCDE = 12;
        public const int MDATE = 13;
        public const int MTIME = 14;

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

        //---------------------------------------------------------------------------------


        /// <summary>
        /// Per-instance default constructor.
        /// </summary>
        public SdEqpt()
        {
            ecode = "";
            estab = 0.0f;
            exref = "";
            emanu = "";
            emodel = "";
            edesc = "";
            etype = "";
            etraf = "";
            emission = "";
            e1stif = 0.0f;
            e2ndif = 0.0f;
            thhold = 0.0f;
            ebndcde = "";
            mdate = "";
            mtime = "";
        }

        /// <summary>
        /// This method returns an annotated, formatted, single-line string that
        /// provides the current values of this object's ecode and edesc.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string ToStringTerse()
        {
            return String.Format("ecode = {0,-8}  ;   edesc = {1}", ecode, edesc);
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
            sb.Append("\n ===== SdEqpt ===== ");

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
        /// This method returns the member values of an object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSV(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[SdEqpt.ECODE] == Constant.DB_NULL ? "NULL, " : "'" + ecode.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.ESTAB] == Constant.DB_NULL ? "NULL, " : "'" + estab.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.EXREF] == Constant.DB_NULL ? "NULL, " : "'" + exref.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.EMANU] == Constant.DB_NULL ? "NULL, " : "'" + emanu.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.EMODEL] == Constant.DB_NULL ? "NULL, " : "'" + emodel.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.EDESC] == Constant.DB_NULL ? "NULL, " : "'" + edesc.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.ETYPE] == Constant.DB_NULL ? "NULL, " : "'" + etype.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.ETRAF] == Constant.DB_NULL ? "NULL, " : "'" + etraf.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.EMISSION] == Constant.DB_NULL ? "NULL, " : "'" + emission.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.E1STIF] == Constant.DB_NULL ? "NULL, " : "'" + e1stif.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.E2NDIF] == Constant.DB_NULL ? "NULL, " : "'" + e2ndif.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.THHOLD] == Constant.DB_NULL ? "NULL, " : "'" + thhold.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.EBNDCDE] == Constant.DB_NULL ? "NULL, " : "'" + ebndcde.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', ");
            sb.Append(nullInds[SdEqpt.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' ");

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

            sb.Append(columnNames[SdEqpt.ECODE] + " = " + (nullInds[SdEqpt.ECODE] == Constant.DB_NULL ? "NULL, " : "'" + ecode.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.ESTAB] + " = " + (nullInds[SdEqpt.ESTAB] == Constant.DB_NULL ? "NULL, " : "'" + estab.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.EXREF] + " = " + (nullInds[SdEqpt.EXREF] == Constant.DB_NULL ? "NULL, " : "'" + exref.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.EMANU] + " = " + (nullInds[SdEqpt.EMANU] == Constant.DB_NULL ? "NULL, " : "'" + emanu.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.EMODEL] + " = " + (nullInds[SdEqpt.EMODEL] == Constant.DB_NULL ? "NULL, " : "'" + emodel.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.EDESC] + " = " + (nullInds[SdEqpt.EDESC] == Constant.DB_NULL ? "NULL, " : "'" + edesc.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.ETYPE] + " = " + (nullInds[SdEqpt.ETYPE] == Constant.DB_NULL ? "NULL, " : "'" + etype.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.ETRAF] + " = " + (nullInds[SdEqpt.ETRAF] == Constant.DB_NULL ? "NULL, " : "'" + etraf.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.EMISSION] + " = " + (nullInds[SdEqpt.EMISSION] == Constant.DB_NULL ? "NULL, " : "'" + emission.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.E1STIF] + " = " + (nullInds[SdEqpt.E1STIF] == Constant.DB_NULL ? "NULL, " : "'" + e1stif.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.E2NDIF] + " = " + (nullInds[SdEqpt.E2NDIF] == Constant.DB_NULL ? "NULL, " : "'" + e2ndif.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.THHOLD] + " = " + (nullInds[SdEqpt.THHOLD] == Constant.DB_NULL ? "NULL, " : "'" + thhold.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.EBNDCDE] + " = " + (nullInds[SdEqpt.EBNDCDE] == Constant.DB_NULL ? "NULL, " : "'" + ebndcde.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.MDATE] + " = " + (nullInds[SdEqpt.MDATE] == Constant.DB_NULL ? "NULL, " : "'" + mdate.ToString() + "', "));
            sb.Append(columnNames[SdEqpt.MTIME] + " = " + (nullInds[SdEqpt.MTIME] == Constant.DB_NULL ? "NULL, " : "'" + mtime.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns instantiated SdEqpt and SQLLEN[] nullInds objects whose
        /// members are set i.a.w. a prescribed SuEqpt object and its associated nullInds; 
        /// SdEqpt has identical members to SuEqpt except for 'cmd' and 'recstat'.
        /// </summary>
        /// <param name="suEqpt"></param>
        /// <param name="suEqptNullInds"></param>
        /// <param name="sdEqpt"></param>
        /// <param name="sdEqptNullInds"></param>
        public static void MakeSdFromSu(SuEqpt suEqpt, SQLLEN[] suEqptNullInds, out SdEqpt sdEqpt, out SQLLEN[] sdEqptNullInds)
        {
            sdEqpt = new SdEqpt();
            sdEqptNullInds = new SQLLEN[NUM_COLUMNS];

            sdEqpt.ecode = suEqpt.ecode;
            sdEqptNullInds[SdEqpt.ECODE] = suEqptNullInds[SuEqpt.ECODE];

            sdEqpt.estab = suEqpt.estab;
            sdEqptNullInds[SdEqpt.ESTAB] = suEqptNullInds[SuEqpt.ESTAB];

            sdEqpt.exref = suEqpt.exref;
            sdEqptNullInds[SdEqpt.EXREF] = suEqptNullInds[SuEqpt.EXREF];

            sdEqpt.emanu = suEqpt.emanu;
            sdEqptNullInds[SdEqpt.EMANU] = suEqptNullInds[SuEqpt.EMANU];

            sdEqpt.emodel = suEqpt.emodel;
            sdEqptNullInds[SdEqpt.EMODEL] = suEqptNullInds[SuEqpt.EMODEL];

            sdEqpt.edesc = suEqpt.edesc;
            sdEqptNullInds[SdEqpt.EDESC] = suEqptNullInds[SuEqpt.EDESC];

            sdEqpt.etype = suEqpt.etype;
            sdEqptNullInds[SdEqpt.ETYPE] = suEqptNullInds[SuEqpt.ETYPE];

            sdEqpt.etraf = suEqpt.etraf;
            sdEqptNullInds[SdEqpt.ETRAF] = suEqptNullInds[SuEqpt.ETRAF];

            sdEqpt.emission = suEqpt.emission;
            sdEqptNullInds[SdEqpt.EMISSION] = suEqptNullInds[SuEqpt.EMISSION];

            sdEqpt.e1stif = suEqpt.e1stif;
            sdEqptNullInds[SdEqpt.E1STIF] = suEqptNullInds[SuEqpt.E1STIF];

            sdEqpt.e2ndif = suEqpt.e2ndif;
            sdEqptNullInds[SdEqpt.E2NDIF] = suEqptNullInds[SuEqpt.E2NDIF];

            sdEqpt.thhold = suEqpt.thhold;
            sdEqptNullInds[SdEqpt.THHOLD] = suEqptNullInds[SuEqpt.THHOLD];

            sdEqpt.ebndcde = suEqpt.ebndcde;
            sdEqptNullInds[SdEqpt.EBNDCDE] = suEqptNullInds[SuEqpt.EBNDCDE];

            sdEqpt.mdate = suEqpt.mdate;
            sdEqptNullInds[SdEqpt.MDATE] = suEqptNullInds[SuEqpt.MDATE];

            sdEqpt.mtime = suEqpt.mtime;
            sdEqptNullInds[SdEqpt.MTIME] = suEqptNullInds[SuEqpt.MTIME];

        }

        /// <summary>
        /// This method returns a 'deep copy' of the current object; changes to the 
        /// field values of this object do not cause changes to the deep copy.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SdEqpt DeepCopy()
        {
            // A deep copy object has deep copies of the nonstatic fields of the current object.
            //    - If a field is a value type then a bit-by-bit copy of the field is created. 
            //    - If a field is a reference type then a deep copy of the referred object is created.

            SdEqpt sdEqpt = new SdEqpt();

            sdEqpt.ecode = this.ecode;
            sdEqpt.estab = this.estab;
            sdEqpt.exref = this.exref;
            sdEqpt.emanu = this.emanu;
            sdEqpt.emodel = this.emodel;
            sdEqpt.edesc = this.edesc;
            sdEqpt.etype = this.etype;
            sdEqpt.etraf = this.etraf;
            sdEqpt.emission = this.emission;
            sdEqpt.e1stif = this.e1stif;
            sdEqpt.e2ndif = this.e2ndif;
            sdEqpt.thhold = this.thhold;
            sdEqpt.ebndcde = this.ebndcde;
            sdEqpt.mdate = this.mdate;
            sdEqpt.mtime = this.mtime;

            return sdEqpt;
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

            tgtValPtrs[ECODE] = Marshal.AllocHGlobal(ECODE_SZ + 1);
            nullIndPtrs[ECODE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ECODE + 1, tgtValPtrs[ECODE], SdEqpt.ECODE_SZ, nullIndPtrs[ECODE]);

            tgtValPtrs[ESTAB] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[ESTAB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, ESTAB + 1, tgtValPtrs[ESTAB], nullIndPtrs[ESTAB]);

            tgtValPtrs[EXREF] = Marshal.AllocHGlobal(EXREF_SZ + 1);
            nullIndPtrs[EXREF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EXREF + 1, tgtValPtrs[EXREF], SdEqpt.EXREF_SZ, nullIndPtrs[EXREF]);

            tgtValPtrs[EMANU] = Marshal.AllocHGlobal(EMANU_SZ + 1);
            nullIndPtrs[EMANU] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EMANU + 1, tgtValPtrs[EMANU], SdEqpt.EMANU_SZ, nullIndPtrs[EMANU]);

            tgtValPtrs[EMODEL] = Marshal.AllocHGlobal(EMODEL_SZ + 1);
            nullIndPtrs[EMODEL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EMODEL + 1, tgtValPtrs[EMODEL], SdEqpt.EMODEL_SZ, nullIndPtrs[EMODEL]);

            tgtValPtrs[EDESC] = Marshal.AllocHGlobal(EDESC_SZ + 1);
            nullIndPtrs[EDESC] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EDESC + 1, tgtValPtrs[EDESC], SdEqpt.EDESC_SZ, nullIndPtrs[EDESC]);

            tgtValPtrs[ETYPE] = Marshal.AllocHGlobal(ETYPE_SZ + 1);
            nullIndPtrs[ETYPE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ETYPE + 1, tgtValPtrs[ETYPE], SdEqpt.ETYPE_SZ, nullIndPtrs[ETYPE]);

            tgtValPtrs[ETRAF] = Marshal.AllocHGlobal(ETRAF_SZ + 1);
            nullIndPtrs[ETRAF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ETRAF + 1, tgtValPtrs[ETRAF], SdEqpt.ETRAF_SZ, nullIndPtrs[ETRAF]);

            tgtValPtrs[EMISSION] = Marshal.AllocHGlobal(EMISSION_SZ + 1);
            nullIndPtrs[EMISSION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EMISSION + 1, tgtValPtrs[EMISSION], SdEqpt.EMISSION_SZ, nullIndPtrs[EMISSION]);

            tgtValPtrs[E1STIF] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[E1STIF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, E1STIF + 1, tgtValPtrs[E1STIF], nullIndPtrs[E1STIF]);

            tgtValPtrs[E2NDIF] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[E2NDIF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, E2NDIF + 1, tgtValPtrs[E2NDIF], nullIndPtrs[E2NDIF]);

            tgtValPtrs[THHOLD] = Marshal.AllocHGlobal(sizeof(float));
            nullIndPtrs[THHOLD] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToFloat(hStmt, THHOLD + 1, tgtValPtrs[THHOLD], nullIndPtrs[THHOLD]);

            tgtValPtrs[EBNDCDE] = Marshal.AllocHGlobal(EBNDCDE_SZ + 1);
            nullIndPtrs[EBNDCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EBNDCDE + 1, tgtValPtrs[EBNDCDE], SdEqpt.EBNDCDE_SZ, nullIndPtrs[EBNDCDE]);

            tgtValPtrs[MDATE] = Marshal.AllocHGlobal(MDATE_SZ + 1);
            nullIndPtrs[MDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MDATE + 1, tgtValPtrs[MDATE], SdEqpt.MDATE_SZ, nullIndPtrs[MDATE]);

            tgtValPtrs[MTIME] = Marshal.AllocHGlobal(MTIME_SZ + 1);
            nullIndPtrs[MTIME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MTIME + 1, tgtValPtrs[MTIME], SdEqpt.MTIME_SZ, nullIndPtrs[MTIME]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdEqpt object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="sdEqpt"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out SdEqpt sdEqpt, out SQLLEN[] nullInds)
        {
            sdEqpt = new SdEqpt();
            nullInds = NullHelper.CreateArrayOfNullInd(SdEqpt.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[ECODE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.ecode = "";
                nullInds[ECODE] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.ecode = Marshal.PtrToStringAnsi(tgtValPtrs[ECODE]).Trim();
                nullInds[ECODE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ESTAB]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ESTAB], F, 0, 1);
                sdEqpt.estab = F[0];
                nullInds[ESTAB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EXREF]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.exref = "";
                nullInds[EXREF] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.exref = Marshal.PtrToStringAnsi(tgtValPtrs[EXREF]).Trim();
                nullInds[EXREF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EMANU]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.emanu = "";
                nullInds[EMANU] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.emanu = Marshal.PtrToStringAnsi(tgtValPtrs[EMANU]).Trim();
                nullInds[EMANU] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EMODEL]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.emodel = "";
                nullInds[EMODEL] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.emodel = Marshal.PtrToStringAnsi(tgtValPtrs[EMODEL]).Trim();
                nullInds[EMODEL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EDESC]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.edesc = "";
                nullInds[EDESC] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.edesc = Marshal.PtrToStringAnsi(tgtValPtrs[EDESC]).Trim();
                nullInds[EDESC] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ETYPE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.etype = "";
                nullInds[ETYPE] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.etype = Marshal.PtrToStringAnsi(tgtValPtrs[ETYPE]).Trim();
                nullInds[ETYPE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ETRAF]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.etraf = "";
                nullInds[ETRAF] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.etraf = Marshal.PtrToStringAnsi(tgtValPtrs[ETRAF]).Trim();
                nullInds[ETRAF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EMISSION]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.emission = "";
                nullInds[EMISSION] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.emission = Marshal.PtrToStringAnsi(tgtValPtrs[EMISSION]).Trim();
                nullInds[EMISSION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[E1STIF]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[E1STIF], F, 0, 1);
                sdEqpt.e1stif = F[0];
                nullInds[E1STIF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[E2NDIF]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[E2NDIF], F, 0, 1);
                sdEqpt.e2ndif = F[0];
                nullInds[E2NDIF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[THHOLD]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[THHOLD], F, 0, 1);
                sdEqpt.thhold = F[0];
                nullInds[THHOLD] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EBNDCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.ebndcde = "";
                nullInds[EBNDCDE] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.ebndcde = Marshal.PtrToStringAnsi(tgtValPtrs[EBNDCDE]).Trim();
                nullInds[EBNDCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.mdate = "";
                nullInds[MDATE] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.mdate = Marshal.PtrToStringAnsi(tgtValPtrs[MDATE]).Trim();
                nullInds[MDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MTIME]);
            if (nullInd == Constant.DB_NULL)
            {
                sdEqpt.mtime = "";
                nullInds[MTIME] = Constant.DB_NULL;
            }
            else
            {
                sdEqpt.mtime = Marshal.PtrToStringAnsi(tgtValPtrs[MTIME]).Trim();
                nullInds[MTIME] = Constant.DB_NOT_NULL;
            }

        }







        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[sd_eqpt](" +
"[ecode] [char](8) NOT NULL," +
"[estab] [real] NULL," +
"[exref] [char](8) NULL," +
"[emanu] [char](10) NULL," +
"[emodel] [char](20) NULL," +
"[edesc] [char](32) NULL," +
"[etype] [char](2) NULL," +
"[etraf] [char](6) NULL," +
"[emission] [char](10) NULL," +
"[e1stif] [real] NULL," +
"[e2ndif] [real] NULL," +
"[thhold] [real] NULL," +
"[ebndcde] [char](4) NULL," +
"[mdate] [char](10) NULL," +
"[mtime] [char](8) NULL," +
" CONSTRAINT [PK_sd_eqpt] PRIMARY KEY CLUSTERED " +
"(" +
"[ecode] ASC" +
") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}

```
