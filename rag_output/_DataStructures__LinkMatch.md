# Documented File: LinkMatch.cs
**Repository Path:** `_DataStructures\LinkMatch.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class encapsulates the set of data fields used to define matches between
    /// MDB channel records and TAFL records in either direction (TAFL -> MDB and MDB -> TAFL);
    /// this class also provides methods that help build SQL queries to INSERT / UPDATE / SELECT
    /// LinkMatch objects to/from a SQL database table.
    /// </summary>
    public class LinkMatch
    {
        public string x_Direction;              //  Must be either 'TaflToMdb' or 'MdbToTafl'.
        public string m_TxRx;                   //  MDB: 'TX', 'RX' or NULL. (Analogous to TAFL TXRX.)
        public string m_Oper;                   //  MDB:  mt_site.oper
        public string m_Call1;                  //  MDB:  mt_site.m_call1
        public string m_Call2;                  //  MDB:  mt.ante.m_call2
        public string m_Bndcde;                 //  MDB:  mt_ante.m_bndcde
        public string m_Chid;                   //  MDB:  mt_chan.m_chid
        public string m_Ause;                   //  MDB:  mt_ante.m_ause
        public int m_Anum;                      //  MDB:  mt_ante.m_anum (= mt_chan.antnumbtx1 for a Tx Link)
        public double m_Lat;                    //  MDB:  mt_site.latit  / 360000.0
        public double m_Lng;                    //  MDB:  mt_site.longit / 360000.0
        public string m_Prov;                   //  MDB:  mt_site.prov
        public double m_Azmth;                  //  MDB:  mt.ante.m_Azmth
        public double m_FreqTx;                 //  MDB:  mt_chan.freqtx
        public double m_FreqRx;                 //  MDB:  mt_chan.freqrx
        public double m_Aht;                    //  MDB:  mt_ante.aht
        public double m_FreqTxRx;               //  MDB:  either freqtx or freqrx depending on the value of m_TxRx.
        public string m_StatTxRx;               //  MDB:  either stattx or statrx depending on the value of m_TxRx.
        public string m_OrdinalKey;             //  MDB:  string used as a key to perform call1-call2 pair ordering.
        public string m_SiteName;               //  MDB:  mt_site.name
        public string m_Region;                 //  MDB:  mt_site.reg
        public int t_KeyField;                  //  TAFL: Venn keyfield column value.
        public string t_TxRx;                   //  TAFL: TXRX ('TX', 'RX' or NULL).
        public double t_Lat;                    //  TAFL: latitude.
        public double t_Lng;                    //  TAFL: longitude.
        public double t_Azmth;                  //  TAFL: azimuth.
        public double t_Aht;                    //  TAFL: antenna's height above ground level.
        public double t_FreqTxRx;               //  TAFL: frequency.
        public string t_AuthorizationNumber;    //  TAFL: Authorization number.
        public string t_AuthorizationStatus;    //  TAFL: Authorization status.
        public string t_Callsign;               //  TAFL: Callsign.
        public string t_InserviceDate;          //  TAFL: InService date.
        public string t_AccountNumber;          //  TAFL: Account number.
        public string t_LicenseeName;           //  TAFL: Licensee name.
        public string t_ReferenceIdentifier;    //  TAFL: Reference identifier.
        public string t_LicenseeName_oper;      //  TAFL: FCSA 'oper' associated with the Licensee Name.
        public double d_Meters;                 //  the distance between sites being matched.
        public double d_Degrees;                //  the angular difference between azimuths being matched.
        public double d_MHz;                    //  the difference between frequencies being matched.
        public int x_FOM;                       //  the 'Figure of Merit' for the closeness of the match 0, 100.
        public string x_Confidence;             //  an emperical description of the 'closeness of the match'.
        public string x_LAF;                    //  the location, azimuth and/or frequency match characteristics.
        public string x_Comment;                //  a comment (e.g. recommended remedial action).

        //----------------------------------------------------------------

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 43;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "x_Direction", "m_TxRx", "m_Oper", "m_Call1", "m_Call2", "m_Bndcde", "m_Chid", "m_Ause", "m_Anum", "m_Lat", "m_Lng", "m_Prov", "m_Azmth", "m_FreqTx", "m_FreqRx", "m_Aht", "m_FreqTxRx", "m_StatTxRx", "m_OrdinalKey", "m_SiteName", "m_Region", "t_KeyField", "t_TxRx", "t_Lat", "t_Lng", "t_Azmth", "t_Aht", "t_FreqTxRx", "t_AuthorizationNumber", "t_AuthorizationStatus", "t_Callsign", "t_InserviceDate", "t_AccountNumber", "t_LicenseeName", "t_ReferenceIdentifier", "t_LicenseeName_oper", "d_Meters", "d_Degrees", "d_MHz", "x_FOM", "x_Confidence", "x_LAF", "x_Comment" };

        //----------------------------------------------------------------

        public const int X_DIRECTION = 0;               
        public const int M_TXRX = 1;
        public const int M_OPER = 2;
        public const int M_CALL1 = 3;
        public const int M_CALL2 = 4;
        public const int M_BNDCDE = 5;
        public const int M_CHID = 6;
        public const int M_AUSE = 7;
        public const int M_ANUM = 8;
        public const int M_LAT = 9;
        public const int M_LNG = 10;
        public const int M_PROV = 11;
        public const int M_AZMTH = 12;
        public const int M_FREQTX = 13;
        public const int M_FREQRX = 14;
        public const int M_AHT = 15;
        public const int M_FREQTXRX = 16;
        public const int M_STATTXRX = 17;
        public const int M_ORDINALKEY = 18;
        public const int M_SITENAME = 19;
        public const int M_REGION = 20;
        public const int T_KEYFIELD = 21;
        public const int T_TXRX = 22;
        public const int T_LAT = 23;
        public const int T_LNG = 24;
        public const int T_AZMTH = 25;
        public const int T_AHT = 26;
        public const int T_FREQTXRX = 27;
        public const int T_AUTHORIZATIONNUMBER = 28;
        public const int T_AUTHORIZATIONSTATUS = 29;
        public const int T_CALLSIGN = 30;
        public const int T_INSERVICEDATE = 31;
        public const int T_ACCOUNTNUMBER = 32;
        public const int T_LICENSEENAME = 33;
        public const int T_REFERENCEIDENTIFIER = 34;
        public const int T_LICENSEENAME_OPER = 35;
        public const int D_METERS = 36;
        public const int D_DEGREES = 37;
        public const int D_MHZ = 38;
        public const int X_FOM = 39;
        public const int X_CONFIDENCE = 40;
        public const int X_LAF = 41;
        public const int X_COMMENT = 42;

        //----------------------------------------------------------------

        public const int X_DIRECTION_SZ = 9 + 1;
        public const int M_TXRX_SZ = 2 + 1;
        public const int M_OPER_SZ = Constant.OPER_SZ;
        public const int M_CALL1_SZ = Constant.CALL_SZ;
        public const int M_CALL2_SZ = Constant.CALL_SZ;
        public const int M_BNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int M_CHID_SZ = Constant.CHID_SZ;
        public const int M_AUSE_SZ = Constant.AUSE_SZ;
        public const int M_PROV_SZ = Constant.PROV_SZ;
        public const int M_STATTXRX_SZ = Constant.STATS_SZ;
        public const int M_ORDINALKEY_SZ = M_CALL1_SZ + M_CALL2_SZ + 1 + 1;
        public const int M_SITENAME_SZ = 32 + 1;
        public const int M_REGION_SZ = 2 + 1;
        public const int T_TXRX_SZ = 2 + 1;
        public const int T_AUTHORIZATIONNUMBER_SZ = 63 + 1;
        public const int T_AUTHORIZATIONSTATUS_SZ = 40 + 1;
        public const int T_CALLSIGN_SZ = 255 + 1;
        public const int T_INSERVICEDATE_SZ = 10 + 1;
        public const int T_ACCOUNTNUMBER_SZ = 255 + 1;
        public const int T_LICENSEENAME_SZ = 255 + 1;
        public const int T_REFERENCEIDENTIFIER_SZ = 63 + 1;
        public const int T_LICENSEENAME_OPER_SZ = Constant.OPER_SZ;
        public const int X_CONFIDENCE_SZ = 9 + 1;
        public const int X_LAF_SZ = 3 + 1;
        public const int X_COMMENT_SZ = 255 + 1;

        //----------------------------------------------------------------

        /// <summary>
        /// A primitive constructor.
        /// </summary>
        public LinkMatch()
        {
            const string STRING_INIT_VAL = "";
            const int INT_INIT_VAL = Int32.MinValue;
            const double DOUBLE_INIT_VAL = Double.MinValue;

            x_Direction = STRING_INIT_VAL;
            m_TxRx = STRING_INIT_VAL;
            m_Oper = STRING_INIT_VAL;
            m_Call1 = STRING_INIT_VAL;
            m_Call2 = STRING_INIT_VAL;
            m_Bndcde = STRING_INIT_VAL;
            m_Chid = STRING_INIT_VAL;
            m_Ause = STRING_INIT_VAL;
            m_Anum = INT_INIT_VAL;
            m_Lat = DOUBLE_INIT_VAL;
            m_Lng = DOUBLE_INIT_VAL;
            m_Prov = STRING_INIT_VAL;
            m_Azmth = DOUBLE_INIT_VAL;
            m_FreqTx = DOUBLE_INIT_VAL;
            m_FreqRx = DOUBLE_INIT_VAL;
            m_Aht = DOUBLE_INIT_VAL;
            m_FreqTxRx = DOUBLE_INIT_VAL;
            m_StatTxRx = STRING_INIT_VAL;
            m_OrdinalKey = STRING_INIT_VAL;
            m_SiteName = STRING_INIT_VAL;
            m_Region = STRING_INIT_VAL;
            t_KeyField = INT_INIT_VAL;
            t_TxRx = STRING_INIT_VAL;
            t_Lat = DOUBLE_INIT_VAL;
            t_Lng = DOUBLE_INIT_VAL;
            t_Azmth = DOUBLE_INIT_VAL;
            t_Aht = DOUBLE_INIT_VAL;
            t_FreqTxRx = DOUBLE_INIT_VAL;
            t_AuthorizationNumber = STRING_INIT_VAL;
            t_AuthorizationStatus = STRING_INIT_VAL;
            t_Callsign = STRING_INIT_VAL;
            t_InserviceDate = STRING_INIT_VAL;
            t_AccountNumber = STRING_INIT_VAL;
            t_LicenseeName = STRING_INIT_VAL;
            t_ReferenceIdentifier = STRING_INIT_VAL;
            t_LicenseeName_oper = STRING_INIT_VAL;
            d_Meters = DOUBLE_INIT_VAL;
            d_Degrees = DOUBLE_INIT_VAL;
            d_MHz = DOUBLE_INIT_VAL;
            x_FOM = INT_INIT_VAL;
            x_Confidence = STRING_INIT_VAL;
            x_LAF = STRING_INIT_VAL;
            x_Comment = STRING_INIT_VAL;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields of this object together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== LinkMatch ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "x_Direction = " + x_Direction);
            sb.Append("\n" + nullInds[i++] + "      " + "m_TxRx = " + m_TxRx);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Oper = " + m_Oper);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Call1 = " + m_Call1);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Call2 = " + m_Call2);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Bndcde = " + m_Bndcde);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Chid = " + m_Chid);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Ause = " + m_Ause);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Anum = " + m_Anum);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Lat = " + m_Lat);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Lng = " + m_Lng);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Prov = " + m_Prov);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Azmth = " + m_Azmth);
            sb.Append("\n" + nullInds[i++] + "      " + "m_FreqTx = " + m_FreqTx);
            sb.Append("\n" + nullInds[i++] + "      " + "m_FreqRx = " + m_FreqRx);
            sb.Append("\n" + nullInds[i++] + "      " + "m_Aht = " + m_Aht);
            sb.Append("\n" + nullInds[i++] + "      " + "m_FreqTxRx = " + m_FreqTxRx);
            sb.Append("\n" + nullInds[i++] + "      " + "m_StatTxRx = " + m_StatTxRx);
            sb.Append("\n" + nullInds[i++] + "      " + "m_OrdinalKey = " + m_OrdinalKey);
            sb.Append("\n" + nullInds[i++] + "      " + "m_SiteName = " + m_SiteName);
            sb.Append("\n" + nullInds[i++] + "      " + "m_m_Region = " + m_Region);
            sb.Append("\n" + nullInds[i++] + "      " + "t_KeyField = " + t_KeyField);
            sb.Append("\n" + nullInds[i++] + "      " + "t_TxRx = " + t_TxRx);
            sb.Append("\n" + nullInds[i++] + "      " + "t_Lat = " + t_Lat);
            sb.Append("\n" + nullInds[i++] + "      " + "t_Lng = " + t_Lng);
            sb.Append("\n" + nullInds[i++] + "      " + "t_Azmth = " + t_Azmth);
            sb.Append("\n" + nullInds[i++] + "      " + "t_Aht = " + t_Aht);
            sb.Append("\n" + nullInds[i++] + "      " + "t_FreqTxRx = " + t_FreqTxRx);
            sb.Append("\n" + nullInds[i++] + "      " + "t_AuthorizationNumber = " + t_AuthorizationNumber);
            sb.Append("\n" + nullInds[i++] + "      " + "t_AuthorizationStatus = " + t_AuthorizationStatus);
            sb.Append("\n" + nullInds[i++] + "      " + "t_Callsign = " + t_Callsign);
            sb.Append("\n" + nullInds[i++] + "      " + "t_InserviceDate = " + t_InserviceDate);
            sb.Append("\n" + nullInds[i++] + "      " + "t_AccountNumber = " + t_AccountNumber);
            sb.Append("\n" + nullInds[i++] + "      " + "t_LicenseeName = " + t_LicenseeName);
            sb.Append("\n" + nullInds[i++] + "      " + "t_ReferenceIdentifier = " + t_ReferenceIdentifier);
            sb.Append("\n" + nullInds[i++] + "      " + "t_LicenseeName_oper = " + t_LicenseeName_oper);
            sb.Append("\n" + nullInds[i++] + "      " + "d_Meters = " + d_Meters);
            sb.Append("\n" + nullInds[i++] + "      " + "d_Degrees = " + d_Degrees);
            sb.Append("\n" + nullInds[i++] + "      " + "d_MHz = " + d_MHz);
            sb.Append("\n" + nullInds[i++] + "      " + "x_FOM = " + x_FOM);
            sb.Append("\n" + nullInds[i++] + "      " + "x_Confidence = " + x_Confidence);
            sb.Append("\n" + nullInds[i++] + "      " + "x_LAF = " + x_LAF);
            sb.Append("\n" + nullInds[i++] + "      " + "x_Comment = " + x_Comment);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns
        /// an array of IntPtr objects that point to the start addresses of non-contiguous blocks of global
        /// (heap) memory, each of a sufficient size to hold the value of a specific member field 
        /// of this object; similarly for the nullInd array associated with this object;
        /// thus, this method creates the parameter bindings prior to an SQL/ODBC UPDATE or 
        /// INSERT query.
        /// </summary>
        /// <param name="hStmt"></param>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>

        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[X_DIRECTION] = Marshal.AllocHGlobal(X_DIRECTION_SZ + 1);
            nullIndPtrs[X_DIRECTION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, X_DIRECTION + 1, tgtValPtrs[X_DIRECTION], LinkMatch.X_DIRECTION_SZ, nullIndPtrs[X_DIRECTION]);

            tgtValPtrs[M_TXRX] = Marshal.AllocHGlobal(M_TXRX_SZ + 1);
            nullIndPtrs[M_TXRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_TXRX + 1, tgtValPtrs[M_TXRX], LinkMatch.M_TXRX_SZ, nullIndPtrs[M_TXRX]);

            tgtValPtrs[M_OPER] = Marshal.AllocHGlobal(M_OPER_SZ + 1);
            nullIndPtrs[M_OPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_OPER + 1, tgtValPtrs[M_OPER], LinkMatch.M_OPER_SZ, nullIndPtrs[M_OPER]);

            tgtValPtrs[M_CALL1] = Marshal.AllocHGlobal(M_CALL1_SZ + 1);
            nullIndPtrs[M_CALL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_CALL1 + 1, tgtValPtrs[M_CALL1], LinkMatch.M_CALL1_SZ, nullIndPtrs[M_CALL1]);

            tgtValPtrs[M_CALL2] = Marshal.AllocHGlobal(M_CALL2_SZ + 1);
            nullIndPtrs[M_CALL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_CALL2 + 1, tgtValPtrs[M_CALL2], LinkMatch.M_CALL2_SZ, nullIndPtrs[M_CALL2]);

            tgtValPtrs[M_BNDCDE] = Marshal.AllocHGlobal(M_BNDCDE_SZ + 1);
            nullIndPtrs[M_BNDCDE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_BNDCDE + 1, tgtValPtrs[M_BNDCDE], LinkMatch.M_BNDCDE_SZ, nullIndPtrs[M_BNDCDE]);

            tgtValPtrs[M_CHID] = Marshal.AllocHGlobal(M_CHID_SZ + 1);
            nullIndPtrs[M_CHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_CHID + 1, tgtValPtrs[M_CHID], LinkMatch.M_CHID_SZ, nullIndPtrs[M_CHID]);

            tgtValPtrs[M_AUSE] = Marshal.AllocHGlobal(M_AUSE_SZ + 1);
            nullIndPtrs[M_AUSE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_AUSE + 1, tgtValPtrs[M_AUSE], LinkMatch.M_AUSE_SZ, nullIndPtrs[M_AUSE]);

            tgtValPtrs[M_ANUM] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[M_ANUM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, M_ANUM + 1, tgtValPtrs[M_ANUM], nullIndPtrs[M_ANUM]);

            tgtValPtrs[M_LAT] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_LAT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_LAT + 1, tgtValPtrs[M_LAT], nullIndPtrs[M_LAT]);

            tgtValPtrs[M_LNG] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_LNG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_LNG + 1, tgtValPtrs[M_LNG], nullIndPtrs[M_LNG]);

            tgtValPtrs[M_PROV] = Marshal.AllocHGlobal(M_PROV_SZ + 1);
            nullIndPtrs[M_PROV] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_PROV + 1, tgtValPtrs[M_PROV], LinkMatch.M_PROV_SZ, nullIndPtrs[M_PROV]);

            tgtValPtrs[M_AZMTH] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_AZMTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_AZMTH + 1, tgtValPtrs[M_AZMTH], nullIndPtrs[M_AZMTH]);

            tgtValPtrs[M_FREQTX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_FREQTX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_FREQTX + 1, tgtValPtrs[M_FREQTX], nullIndPtrs[M_FREQTX]);

            tgtValPtrs[M_FREQRX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_FREQRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_FREQRX + 1, tgtValPtrs[M_FREQRX], nullIndPtrs[M_FREQRX]);

            tgtValPtrs[M_AHT] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_AHT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_AHT + 1, tgtValPtrs[M_AHT], nullIndPtrs[M_AHT]);

            tgtValPtrs[M_FREQTXRX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[M_FREQTXRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, M_FREQTXRX + 1, tgtValPtrs[M_FREQTXRX], nullIndPtrs[M_FREQTXRX]);

            tgtValPtrs[M_STATTXRX] = Marshal.AllocHGlobal(M_STATTXRX_SZ + 1);
            nullIndPtrs[M_STATTXRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_STATTXRX + 1, tgtValPtrs[M_STATTXRX], LinkMatch.M_STATTXRX_SZ, nullIndPtrs[M_STATTXRX]);

            tgtValPtrs[M_ORDINALKEY] = Marshal.AllocHGlobal(M_ORDINALKEY_SZ + 1);
            nullIndPtrs[M_ORDINALKEY] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_ORDINALKEY + 1, tgtValPtrs[M_ORDINALKEY], LinkMatch.M_ORDINALKEY_SZ, nullIndPtrs[M_ORDINALKEY]);

            tgtValPtrs[M_SITENAME] = Marshal.AllocHGlobal(M_SITENAME_SZ + 1);
            nullIndPtrs[M_SITENAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_SITENAME + 1, tgtValPtrs[M_SITENAME], LinkMatch.M_SITENAME_SZ, nullIndPtrs[M_SITENAME]);

            tgtValPtrs[M_REGION] = Marshal.AllocHGlobal(M_REGION_SZ + 1);
            nullIndPtrs[M_REGION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, M_REGION + 1, tgtValPtrs[M_REGION], LinkMatch.M_REGION_SZ, nullIndPtrs[M_REGION]);

            tgtValPtrs[T_KEYFIELD] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[T_KEYFIELD] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, T_KEYFIELD + 1, tgtValPtrs[T_KEYFIELD], nullIndPtrs[T_KEYFIELD]);

            tgtValPtrs[T_TXRX] = Marshal.AllocHGlobal(T_TXRX_SZ + 1);
            nullIndPtrs[T_TXRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_TXRX + 1, tgtValPtrs[T_TXRX], LinkMatch.T_TXRX_SZ, nullIndPtrs[T_TXRX]);

            tgtValPtrs[T_LAT] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[T_LAT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, T_LAT + 1, tgtValPtrs[T_LAT], nullIndPtrs[T_LAT]);

            tgtValPtrs[T_LNG] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[T_LNG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, T_LNG + 1, tgtValPtrs[T_LNG], nullIndPtrs[T_LNG]);

            tgtValPtrs[T_AZMTH] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[T_AZMTH] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, T_AZMTH + 1, tgtValPtrs[T_AZMTH], nullIndPtrs[T_AZMTH]);

            tgtValPtrs[T_AHT] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[T_AHT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, T_AHT + 1, tgtValPtrs[T_AHT], nullIndPtrs[T_AHT]);

            tgtValPtrs[T_FREQTXRX] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[T_FREQTXRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, T_FREQTXRX + 1, tgtValPtrs[T_FREQTXRX], nullIndPtrs[T_FREQTXRX]);

            tgtValPtrs[T_AUTHORIZATIONNUMBER] = Marshal.AllocHGlobal(T_AUTHORIZATIONNUMBER_SZ + 1);
            nullIndPtrs[T_AUTHORIZATIONNUMBER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_AUTHORIZATIONNUMBER + 1, tgtValPtrs[T_AUTHORIZATIONNUMBER], LinkMatch.T_AUTHORIZATIONNUMBER_SZ, nullIndPtrs[T_AUTHORIZATIONNUMBER]);

            tgtValPtrs[T_AUTHORIZATIONSTATUS] = Marshal.AllocHGlobal(T_AUTHORIZATIONSTATUS_SZ + 1);
            nullIndPtrs[T_AUTHORIZATIONSTATUS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_AUTHORIZATIONSTATUS + 1, tgtValPtrs[T_AUTHORIZATIONSTATUS], LinkMatch.T_AUTHORIZATIONSTATUS_SZ, nullIndPtrs[T_AUTHORIZATIONSTATUS]);

            tgtValPtrs[T_CALLSIGN] = Marshal.AllocHGlobal(T_CALLSIGN_SZ + 1);
            nullIndPtrs[T_CALLSIGN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_CALLSIGN + 1, tgtValPtrs[T_CALLSIGN], LinkMatch.T_CALLSIGN_SZ, nullIndPtrs[T_CALLSIGN]);

            tgtValPtrs[T_INSERVICEDATE] = Marshal.AllocHGlobal(T_INSERVICEDATE_SZ + 1);
            nullIndPtrs[T_INSERVICEDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_INSERVICEDATE + 1, tgtValPtrs[T_INSERVICEDATE], LinkMatch.T_INSERVICEDATE_SZ, nullIndPtrs[T_INSERVICEDATE]);

            tgtValPtrs[T_ACCOUNTNUMBER] = Marshal.AllocHGlobal(T_ACCOUNTNUMBER_SZ + 1);
            nullIndPtrs[T_ACCOUNTNUMBER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_ACCOUNTNUMBER + 1, tgtValPtrs[T_ACCOUNTNUMBER], LinkMatch.T_ACCOUNTNUMBER_SZ, nullIndPtrs[T_ACCOUNTNUMBER]);

            tgtValPtrs[T_LICENSEENAME] = Marshal.AllocHGlobal(T_LICENSEENAME_SZ + 1);
            nullIndPtrs[T_LICENSEENAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_LICENSEENAME + 1, tgtValPtrs[T_LICENSEENAME], LinkMatch.T_LICENSEENAME_SZ, nullIndPtrs[T_LICENSEENAME]);

            tgtValPtrs[T_REFERENCEIDENTIFIER] = Marshal.AllocHGlobal(T_REFERENCEIDENTIFIER_SZ + 1);
            nullIndPtrs[T_REFERENCEIDENTIFIER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_REFERENCEIDENTIFIER + 1, tgtValPtrs[T_REFERENCEIDENTIFIER], LinkMatch.T_REFERENCEIDENTIFIER_SZ, nullIndPtrs[T_REFERENCEIDENTIFIER]);

            tgtValPtrs[T_LICENSEENAME_OPER] = Marshal.AllocHGlobal(T_LICENSEENAME_OPER_SZ + 1);
            nullIndPtrs[T_LICENSEENAME_OPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, T_LICENSEENAME_OPER + 1, tgtValPtrs[T_LICENSEENAME_OPER], LinkMatch.T_LICENSEENAME_OPER_SZ, nullIndPtrs[T_LICENSEENAME_OPER]);

            tgtValPtrs[D_METERS] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[D_METERS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, D_METERS + 1, tgtValPtrs[D_METERS], nullIndPtrs[D_METERS]);

            tgtValPtrs[D_DEGREES] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[D_DEGREES] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, D_DEGREES + 1, tgtValPtrs[D_DEGREES], nullIndPtrs[D_DEGREES]);

            tgtValPtrs[D_MHZ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[D_MHZ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, D_MHZ + 1, tgtValPtrs[D_MHZ], nullIndPtrs[D_MHZ]);

            tgtValPtrs[X_FOM] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[X_FOM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, X_FOM + 1, tgtValPtrs[X_FOM], nullIndPtrs[X_FOM]);

            tgtValPtrs[X_CONFIDENCE] = Marshal.AllocHGlobal(X_CONFIDENCE_SZ + 1);
            nullIndPtrs[X_CONFIDENCE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, X_CONFIDENCE + 1, tgtValPtrs[X_CONFIDENCE], LinkMatch.X_CONFIDENCE_SZ, nullIndPtrs[X_CONFIDENCE]);

            tgtValPtrs[X_LAF] = Marshal.AllocHGlobal(X_LAF_SZ + 1);
            nullIndPtrs[X_LAF] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, X_LAF + 1, tgtValPtrs[X_LAF], LinkMatch.X_LAF_SZ, nullIndPtrs[X_LAF]);

            tgtValPtrs[X_COMMENT] = Marshal.AllocHGlobal(X_COMMENT_SZ + 1);
            nullIndPtrs[X_COMMENT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, X_COMMENT + 1, tgtValPtrs[X_COMMENT], LinkMatch.X_COMMENT_SZ, nullIndPtrs[X_COMMENT]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set has been written to parameter-binding buffers in global (heap)
        /// memory; the method returns a fully populated MtSite object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="linkMatch"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out LinkMatch linkMatch, out SQLLEN[] nullInds)
        {
            linkMatch = new LinkMatch();
            nullInds = NullHelper.CreateArrayOfNullInd(LinkMatch.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[X_DIRECTION]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.x_Direction = "";
                nullInds[X_DIRECTION] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.x_Direction = Marshal.PtrToStringAnsi(tgtValPtrs[X_DIRECTION]).Trim();
                nullInds[X_DIRECTION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_TXRX]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_TxRx = "";
                nullInds[M_TXRX] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_TxRx = Marshal.PtrToStringAnsi(tgtValPtrs[M_TXRX]).Trim();
                nullInds[M_TXRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Oper = "";
                nullInds[M_OPER] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Oper = Marshal.PtrToStringAnsi(tgtValPtrs[M_OPER]).Trim();
                nullInds[M_OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_CALL1]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Call1 = "";
                nullInds[M_CALL1] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Call1 = Marshal.PtrToStringAnsi(tgtValPtrs[M_CALL1]).Trim();
                nullInds[M_CALL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_CALL2]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Call2 = "";
                nullInds[M_CALL2] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Call2 = Marshal.PtrToStringAnsi(tgtValPtrs[M_CALL2]).Trim();
                nullInds[M_CALL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_BNDCDE]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Bndcde = "";
                nullInds[M_BNDCDE] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Bndcde = Marshal.PtrToStringAnsi(tgtValPtrs[M_BNDCDE]).Trim();
                nullInds[M_BNDCDE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_CHID]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Chid = "";
                nullInds[M_CHID] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Chid = Marshal.PtrToStringAnsi(tgtValPtrs[M_CHID]).Trim();
                nullInds[M_CHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_AUSE]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Ause = "";
                nullInds[M_AUSE] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Ause = Marshal.PtrToStringAnsi(tgtValPtrs[M_AUSE]).Trim();
                nullInds[M_AUSE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_ANUM]);
            if (nullInd != Constant.DB_NULL)
            {
                linkMatch.m_Anum = Marshal.ReadInt32(tgtValPtrs[M_ANUM]);
                nullInds[M_ANUM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_LAT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_LAT], D, 0, 1);
                linkMatch.m_Lat = D[0];
                nullInds[M_LAT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_LNG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_LNG], D, 0, 1);
                linkMatch.m_Lng = D[0];
                nullInds[M_LNG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_PROV]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Prov = "";
                nullInds[M_PROV] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Prov = Marshal.PtrToStringAnsi(tgtValPtrs[M_PROV]).Trim();
                nullInds[M_PROV] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_AZMTH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_AZMTH], D, 0, 1);
                linkMatch.m_Azmth = D[0];
                nullInds[M_AZMTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_FREQTX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_FREQTX], D, 0, 1);
                linkMatch.m_FreqTx = D[0];
                nullInds[M_FREQTX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_FREQRX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_FREQRX], D, 0, 1);
                linkMatch.m_FreqRx = D[0];
                nullInds[M_FREQRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_AHT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_AHT], D, 0, 1);
                linkMatch.m_Aht = D[0];
                nullInds[M_AHT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_FREQTXRX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[M_FREQTXRX], D, 0, 1);
                linkMatch.m_FreqTxRx = D[0];
                nullInds[M_FREQTXRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_STATTXRX]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_StatTxRx = "";
                nullInds[M_STATTXRX] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_StatTxRx = Marshal.PtrToStringAnsi(tgtValPtrs[M_STATTXRX]).Trim();
                nullInds[M_STATTXRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_ORDINALKEY]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_OrdinalKey = "";
                nullInds[M_ORDINALKEY] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_OrdinalKey = Marshal.PtrToStringAnsi(tgtValPtrs[M_ORDINALKEY]).Trim();
                nullInds[M_ORDINALKEY] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_SITENAME]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_SiteName = "";
                nullInds[M_SITENAME] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_SiteName = Marshal.PtrToStringAnsi(tgtValPtrs[M_SITENAME]).Trim();
                nullInds[M_SITENAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[M_REGION]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.m_Region = "";
                nullInds[M_REGION] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.m_Region = Marshal.PtrToStringAnsi(tgtValPtrs[M_REGION]).Trim();
                nullInds[M_REGION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_KEYFIELD]);
            if (nullInd != Constant.DB_NULL)
            {
                linkMatch.t_KeyField = Marshal.ReadInt32(tgtValPtrs[T_KEYFIELD]);
                nullInds[T_KEYFIELD] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_TXRX]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_TxRx = "";
                nullInds[T_TXRX] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_TxRx = Marshal.PtrToStringAnsi(tgtValPtrs[T_TXRX]).Trim();
                nullInds[T_TXRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_LAT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[T_LAT], D, 0, 1);
                linkMatch.t_Lat = D[0];
                nullInds[T_LAT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_LNG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[T_LNG], D, 0, 1);
                linkMatch.t_Lng = D[0];
                nullInds[T_LNG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_AZMTH]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[T_AZMTH], D, 0, 1);
                linkMatch.t_Azmth = D[0];
                nullInds[T_AZMTH] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_AHT]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[T_AHT], D, 0, 1);
                linkMatch.t_Aht = D[0];
                nullInds[T_AHT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_FREQTXRX]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[T_FREQTXRX], D, 0, 1);
                linkMatch.t_FreqTxRx = D[0];
                nullInds[T_FREQTXRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_AUTHORIZATIONNUMBER]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_AuthorizationNumber = "";
                nullInds[T_AUTHORIZATIONNUMBER] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_AuthorizationNumber = Marshal.PtrToStringAnsi(tgtValPtrs[T_AUTHORIZATIONNUMBER]).Trim();
                nullInds[T_AUTHORIZATIONNUMBER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_AUTHORIZATIONSTATUS]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_AuthorizationStatus = "";
                nullInds[T_AUTHORIZATIONSTATUS] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_AuthorizationStatus = Marshal.PtrToStringAnsi(tgtValPtrs[T_AUTHORIZATIONSTATUS]).Trim();
                nullInds[T_AUTHORIZATIONSTATUS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_CALLSIGN]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_Callsign = "";
                nullInds[T_CALLSIGN] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_Callsign = Marshal.PtrToStringAnsi(tgtValPtrs[T_CALLSIGN]).Trim();
                nullInds[T_CALLSIGN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_INSERVICEDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_InserviceDate = "";
                nullInds[T_INSERVICEDATE] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_InserviceDate = Marshal.PtrToStringAnsi(tgtValPtrs[T_INSERVICEDATE]).Trim();
                nullInds[T_INSERVICEDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_ACCOUNTNUMBER]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_AccountNumber = "";
                nullInds[T_ACCOUNTNUMBER] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_AccountNumber = Marshal.PtrToStringAnsi(tgtValPtrs[T_ACCOUNTNUMBER]).Trim();
                nullInds[T_ACCOUNTNUMBER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_LICENSEENAME]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_LicenseeName = "";
                nullInds[T_LICENSEENAME] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_LicenseeName = Marshal.PtrToStringAnsi(tgtValPtrs[T_LICENSEENAME]).Trim();
                nullInds[T_LICENSEENAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_REFERENCEIDENTIFIER]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_ReferenceIdentifier = "";
                nullInds[T_REFERENCEIDENTIFIER] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_ReferenceIdentifier = Marshal.PtrToStringAnsi(tgtValPtrs[T_REFERENCEIDENTIFIER]).Trim();
                nullInds[T_REFERENCEIDENTIFIER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[T_LICENSEENAME_OPER]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.t_LicenseeName_oper = "";
                nullInds[T_LICENSEENAME_OPER] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.t_LicenseeName_oper = Marshal.PtrToStringAnsi(tgtValPtrs[T_LICENSEENAME_OPER]).Trim();
                nullInds[T_LICENSEENAME_OPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[D_METERS]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[D_METERS], D, 0, 1);
                linkMatch.d_Meters = D[0];
                nullInds[D_METERS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[D_DEGREES]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[D_DEGREES], D, 0, 1);
                linkMatch.d_Degrees = D[0];
                nullInds[D_DEGREES] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[D_MHZ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[D_MHZ], D, 0, 1);
                linkMatch.d_MHz = D[0];
                nullInds[D_MHZ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[X_FOM]);
            if (nullInd != Constant.DB_NULL)
            {
                linkMatch.x_FOM = Marshal.ReadInt32(tgtValPtrs[X_FOM]);
                nullInds[X_FOM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[X_CONFIDENCE]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.x_Confidence = "";
                nullInds[X_CONFIDENCE] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.x_Confidence = Marshal.PtrToStringAnsi(tgtValPtrs[X_CONFIDENCE]).Trim();
                nullInds[X_CONFIDENCE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[X_LAF]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.x_LAF = "";
                nullInds[X_LAF] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.x_LAF = Marshal.PtrToStringAnsi(tgtValPtrs[X_LAF]).Trim();
                nullInds[X_LAF] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[X_COMMENT]);
            if (nullInd == Constant.DB_NULL)
            {
                linkMatch.x_Comment = "";
                nullInds[X_COMMENT] = Constant.DB_NULL;
            }
            else
            {
                linkMatch.x_Comment = Marshal.PtrToStringAnsi(tgtValPtrs[X_COMMENT]).Trim();
                nullInds[X_COMMENT] = Constant.DB_NOT_NULL;
            }

        }


















    }
}

```
