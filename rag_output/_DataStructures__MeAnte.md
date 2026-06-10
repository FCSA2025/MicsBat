# Documented File: MeAnte.cs
**Repository Path:** `_DataStructures\MeAnte.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using _Configuration;

namespace _DataStructures
{
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with the DB table <b>main.me_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class MeAnte
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOCATION_SZ)]
        public string location;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TXBAND_SZ)]
        public string txband;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RXBAND_SZ)]
        public string rxband;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODETX_SZ)]
        public string acodetx;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ACODERX_SZ)]
        public string acoderx;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float g_t;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float lnat;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float aht;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslt;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float afslr;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txhgmax;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxhgmax;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int satlongit;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float satlong;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SATLONGS_SZ)]
        public string satlongs;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float az;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float el;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float sarc1;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float sarc2;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxpre;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txpre;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxtro;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txtro;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LICENCE_SZ)]
        public string licence;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SATNAME_SZ)]
        public string satname;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATA_SZ)]
        public string stata;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTA_SZ)]
        public string nota;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OP2_SZ)]
        public string op2;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int antref;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ORBIT_SZ)]
        public string orbit;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;

        //-------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 34;
        //-------------------------------------------------------------------------------

        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int TXBAND_SZ = 5;
        public const int RXBAND_SZ = 5;
        public const int ACODETX_SZ = 13;
        public const int ACODERX_SZ = 13;
        public const int SATLONGS_SZ = 2;
        public const int LICENCE_SZ = Constant.LICENCE_SZ;
        public const int SATNAME_SZ = 17;
        public const int STATA_SZ = 2;
        public const int NOTA_SZ = 5;
        public const int OP2_SZ = 3;
        public const int ORBIT_SZ = 3;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        public const int USERID_SZ = 13;

        //-------------------------------------------------------------------------------

        public const int LOCATION = 0;
        public const int CALL1 = 1;
        public const int TXBAND = 2;
        public const int RXBAND = 3;
        public const int ACODETX = 4;
        public const int ACODERX = 5;
        public const int G_T = 6;
        public const int LNAT = 7;
        public const int AHT = 8;
        public const int AFSLT = 9;
        public const int AFSLR = 10;
        public const int TXHGMAX = 11;
        public const int RXHGMAX = 12;
        public const int SATLONGIT = 13;
        public const int SATLONG = 14;
        public const int SATLONGS = 15;
        public const int AZ = 16;
        public const int EL = 17;
        public const int SARC1 = 18;
        public const int SARC2 = 19;
        public const int RXPRE = 20;
        public const int TXPRE = 21;
        public const int RXTRO = 22;
        public const int TXTRO = 23;
        public const int LICENCE = 24;
        public const int SATNAME = 25;
        public const int STATA = 26;
        public const int NOTA = 27;
        public const int OP2 = 28;
        public const int ANTREF = 29;
        public const int ORBIT = 30;
        public const int MDATE = 31;
        public const int MTIME = 32;
        public const int USERID = 33;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "location", "call1", "txband", "rxband", "acodetx", "acoderx", "g_t", "lnat", "aht", "afslt", "afslr", "txhgmax", "rxhgmax", "satlongit", "satlong", "satlongs", "az", "el", "sarc1", "sarc2", "rxpre", "txpre", "rxtro", "txtro", "licence", "satname", "stata", "nota", "op2", "antref", "orbit", "mdate", "mtime", "userid" };


        //-------------------------------------------------------------------------------

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields {location, call1}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("location = " + location);
            sb.Append("   call1 = " + call1);
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
            sb.AppendLine();

            sb.AppendLine("========== MeAnte:");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location =      " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 =      " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txband =      " + txband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxband =      " + rxband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acodetx =      " + acodetx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "acoderx =      " + acoderx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "g_t =      " + g_t);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "lnat =      " + lnat);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "aht =      " + aht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "afslt =      " + afslt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "afslr =      " + afslr);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txhgmax =      " + txhgmax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxhgmax =      " + rxhgmax);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlongit =      " + satlongit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlong =      " + satlong);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlongs =      " + satlongs);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "az =      " + az);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "el =      " + el);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sarc1 =      " + sarc1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sarc2 =      " + sarc2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxpre =      " + rxpre);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txpre =      " + txpre);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxtro =      " + rxtro);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txtro =      " + txtro);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "licence =      " + licence);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satname =      " + satname);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stata =      " + stata);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "nota =      " + nota);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "op2 =      " + op2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "antref =      " + antref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "orbit =      " + orbit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =      " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =      " + mtime);

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

            sb.Append(nullInds[MeAnte.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',");
            sb.Append(nullInds[MeAnte.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',");
            sb.Append(nullInds[MeAnte.TXBAND] == Constant.DB_NULL ? "NULL," : "'" + txband.ToString() + "',");
            sb.Append(nullInds[MeAnte.RXBAND] == Constant.DB_NULL ? "NULL," : "'" + rxband.ToString() + "',");
            sb.Append(nullInds[MeAnte.ACODETX] == Constant.DB_NULL ? "NULL," : "'" + acodetx.ToString() + "',");
            sb.Append(nullInds[MeAnte.ACODERX] == Constant.DB_NULL ? "NULL," : "'" + acoderx.ToString() + "',");
            sb.Append(nullInds[MeAnte.G_T] == Constant.DB_NULL ? "NULL," : "'" + g_t.ToString() + "',");
            sb.Append(nullInds[MeAnte.LNAT] == Constant.DB_NULL ? "NULL," : "'" + lnat.ToString() + "',");
            sb.Append(nullInds[MeAnte.AHT] == Constant.DB_NULL ? "NULL," : "'" + aht.ToString() + "',");
            sb.Append(nullInds[MeAnte.AFSLT] == Constant.DB_NULL ? "NULL," : "'" + afslt.ToString() + "',");
            sb.Append(nullInds[MeAnte.AFSLR] == Constant.DB_NULL ? "NULL," : "'" + afslr.ToString() + "',");
            sb.Append(nullInds[MeAnte.TXHGMAX] == Constant.DB_NULL ? "NULL," : "'" + txhgmax.ToString() + "',");
            sb.Append(nullInds[MeAnte.RXHGMAX] == Constant.DB_NULL ? "NULL," : "'" + rxhgmax.ToString() + "',");
            sb.Append(nullInds[MeAnte.SATLONGIT] == Constant.DB_NULL ? "NULL," : "'" + satlongit.ToString() + "',");
            sb.Append(nullInds[MeAnte.SATLONG] == Constant.DB_NULL ? "NULL," : "'" + satlong.ToString() + "',");
            sb.Append(nullInds[MeAnte.SATLONGS] == Constant.DB_NULL ? "NULL," : "'" + satlongs.ToString() + "',");
            sb.Append(nullInds[MeAnte.AZ] == Constant.DB_NULL ? "NULL," : "'" + az.ToString() + "',");
            sb.Append(nullInds[MeAnte.EL] == Constant.DB_NULL ? "NULL," : "'" + el.ToString() + "',");
            sb.Append(nullInds[MeAnte.SARC1] == Constant.DB_NULL ? "NULL," : "'" + sarc1.ToString() + "',");
            sb.Append(nullInds[MeAnte.SARC2] == Constant.DB_NULL ? "NULL," : "'" + sarc2.ToString() + "',");
            sb.Append(nullInds[MeAnte.RXPRE] == Constant.DB_NULL ? "NULL," : "'" + rxpre.ToString() + "',");
            sb.Append(nullInds[MeAnte.TXPRE] == Constant.DB_NULL ? "NULL," : "'" + txpre.ToString() + "',");
            sb.Append(nullInds[MeAnte.RXTRO] == Constant.DB_NULL ? "NULL," : "'" + rxtro.ToString() + "',");
            sb.Append(nullInds[MeAnte.TXTRO] == Constant.DB_NULL ? "NULL," : "'" + txtro.ToString() + "',");
            sb.Append(nullInds[MeAnte.LICENCE] == Constant.DB_NULL ? "NULL," : "'" + licence.ToString() + "',");
            sb.Append(nullInds[MeAnte.SATNAME] == Constant.DB_NULL ? "NULL," : "'" + satname.ToString() + "',");
            sb.Append(nullInds[MeAnte.STATA] == Constant.DB_NULL ? "NULL," : "'" + stata.ToString() + "',");
            sb.Append(nullInds[MeAnte.NOTA] == Constant.DB_NULL ? "NULL," : "'" + nota.ToString() + "',");
            sb.Append(nullInds[MeAnte.OP2] == Constant.DB_NULL ? "NULL," : "'" + op2.ToString() + "',");
            sb.Append(nullInds[MeAnte.ANTREF] == Constant.DB_NULL ? "NULL," : "'" + antref.ToString() + "',");
            sb.Append(nullInds[MeAnte.ORBIT] == Constant.DB_NULL ? "NULL," : "'" + orbit.ToString() + "',");
            sb.Append(nullInds[MeAnte.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MeAnte.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MeAnte.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'");

            return sb.ToString();
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

            sb.Append(columnNames[MeAnte.LOCATION] + "=" + (nullInds[MeAnte.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',"));
            sb.Append(columnNames[MeAnte.CALL1] + "=" + (nullInds[MeAnte.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',"));
            sb.Append(columnNames[MeAnte.TXBAND] + "=" + (nullInds[MeAnte.TXBAND] == Constant.DB_NULL ? "NULL," : "'" + txband.ToString() + "',"));
            sb.Append(columnNames[MeAnte.RXBAND] + "=" + (nullInds[MeAnte.RXBAND] == Constant.DB_NULL ? "NULL," : "'" + rxband.ToString() + "',"));
            sb.Append(columnNames[MeAnte.ACODETX] + "=" + (nullInds[MeAnte.ACODETX] == Constant.DB_NULL ? "NULL," : "'" + acodetx.ToString() + "',"));
            sb.Append(columnNames[MeAnte.ACODERX] + "=" + (nullInds[MeAnte.ACODERX] == Constant.DB_NULL ? "NULL," : "'" + acoderx.ToString() + "',"));
            sb.Append(columnNames[MeAnte.G_T] + "=" + (nullInds[MeAnte.G_T] == Constant.DB_NULL ? "NULL," : "'" + g_t.ToString() + "',"));
            sb.Append(columnNames[MeAnte.LNAT] + "=" + (nullInds[MeAnte.LNAT] == Constant.DB_NULL ? "NULL," : "'" + lnat.ToString() + "',"));
            sb.Append(columnNames[MeAnte.AHT] + "=" + (nullInds[MeAnte.AHT] == Constant.DB_NULL ? "NULL," : "'" + aht.ToString() + "',"));
            sb.Append(columnNames[MeAnte.AFSLT] + "=" + (nullInds[MeAnte.AFSLT] == Constant.DB_NULL ? "NULL," : "'" + afslt.ToString() + "',"));
            sb.Append(columnNames[MeAnte.AFSLR] + "=" + (nullInds[MeAnte.AFSLR] == Constant.DB_NULL ? "NULL," : "'" + afslr.ToString() + "',"));
            sb.Append(columnNames[MeAnte.TXHGMAX] + "=" + (nullInds[MeAnte.TXHGMAX] == Constant.DB_NULL ? "NULL," : "'" + txhgmax.ToString() + "',"));
            sb.Append(columnNames[MeAnte.RXHGMAX] + "=" + (nullInds[MeAnte.RXHGMAX] == Constant.DB_NULL ? "NULL," : "'" + rxhgmax.ToString() + "',"));
            sb.Append(columnNames[MeAnte.SATLONGIT] + "=" + (nullInds[MeAnte.SATLONGIT] == Constant.DB_NULL ? "NULL," : "'" + satlongit.ToString() + "',"));
            sb.Append(columnNames[MeAnte.SATLONG] + "=" + (nullInds[MeAnte.SATLONG] == Constant.DB_NULL ? "NULL," : "'" + satlong.ToString() + "',"));
            sb.Append(columnNames[MeAnte.SATLONGS] + "=" + (nullInds[MeAnte.SATLONGS] == Constant.DB_NULL ? "NULL," : "'" + satlongs.ToString() + "',"));
            sb.Append(columnNames[MeAnte.AZ] + "=" + (nullInds[MeAnte.AZ] == Constant.DB_NULL ? "NULL," : "'" + az.ToString() + "',"));
            sb.Append(columnNames[MeAnte.EL] + "=" + (nullInds[MeAnte.EL] == Constant.DB_NULL ? "NULL," : "'" + el.ToString() + "',"));
            sb.Append(columnNames[MeAnte.SARC1] + "=" + (nullInds[MeAnte.SARC1] == Constant.DB_NULL ? "NULL," : "'" + sarc1.ToString() + "',"));
            sb.Append(columnNames[MeAnte.SARC2] + "=" + (nullInds[MeAnte.SARC2] == Constant.DB_NULL ? "NULL," : "'" + sarc2.ToString() + "',"));
            sb.Append(columnNames[MeAnte.RXPRE] + "=" + (nullInds[MeAnte.RXPRE] == Constant.DB_NULL ? "NULL," : "'" + rxpre.ToString() + "',"));
            sb.Append(columnNames[MeAnte.TXPRE] + "=" + (nullInds[MeAnte.TXPRE] == Constant.DB_NULL ? "NULL," : "'" + txpre.ToString() + "',"));
            sb.Append(columnNames[MeAnte.RXTRO] + "=" + (nullInds[MeAnte.RXTRO] == Constant.DB_NULL ? "NULL," : "'" + rxtro.ToString() + "',"));
            sb.Append(columnNames[MeAnte.TXTRO] + "=" + (nullInds[MeAnte.TXTRO] == Constant.DB_NULL ? "NULL," : "'" + txtro.ToString() + "',"));
            sb.Append(columnNames[MeAnte.LICENCE] + "=" + (nullInds[MeAnte.LICENCE] == Constant.DB_NULL ? "NULL," : "'" + licence.ToString() + "',"));
            sb.Append(columnNames[MeAnte.SATNAME] + "=" + (nullInds[MeAnte.SATNAME] == Constant.DB_NULL ? "NULL," : "'" + satname.ToString() + "',"));
            sb.Append(columnNames[MeAnte.STATA] + "=" + (nullInds[MeAnte.STATA] == Constant.DB_NULL ? "NULL," : "'" + stata.ToString() + "',"));
            sb.Append(columnNames[MeAnte.NOTA] + "=" + (nullInds[MeAnte.NOTA] == Constant.DB_NULL ? "NULL," : "'" + nota.ToString() + "',"));
            sb.Append(columnNames[MeAnte.OP2] + "=" + (nullInds[MeAnte.OP2] == Constant.DB_NULL ? "NULL," : "'" + op2.ToString() + "',"));
            sb.Append(columnNames[MeAnte.ANTREF] + "=" + (nullInds[MeAnte.ANTREF] == Constant.DB_NULL ? "NULL," : "'" + antref.ToString() + "',"));
            sb.Append(columnNames[MeAnte.ORBIT] + "=" + (nullInds[MeAnte.ORBIT] == Constant.DB_NULL ? "NULL," : "'" + orbit.ToString() + "',"));
            sb.Append(columnNames[MeAnte.MDATE] + "=" + (nullInds[MeAnte.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MeAnte.MTIME] + "=" + (nullInds[MeAnte.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MeAnte.USERID] + "=" + (nullInds[MeAnte.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'"));

            return sb.ToString();
        }

        //-------------------------------------------------------------------------------

        public const string CREATE_TABLE = "" +
    "CREATE TABLE [{0}].[me_ante] " +
    "(" +
    "[location] [char](10) NOT NULL," +
    "[call1] [char](9) NOT NULL," +
    "[txband] [char](4) NULL," +
    "[rxband] [char](4) NULL," +
    "[acodetx] [char](12) NULL," +
    "[acoderx] [char](12) NULL," +
    "[g_t] [real] NULL," +
    "[lnat] [real] NULL," +
    "[aht] [real] NULL," +
    "[afslt] [real] NULL," +
    "[afslr] [real] NULL," +
    "[txhgmax] [real] NULL," +
    "[rxhgmax] [real] NULL," +
    "[satlongit] [int] NULL," +
    "[satlong] [real] NULL," +
    "[satlongs] [char](1) NULL," +
    "[az] [real] NULL," +
    "[el] [real] NULL," +
    "[sarc1] [real] NULL," +
    "[sarc2] [real] NULL," +
    "[rxpre] [real] NULL," +
    "[txpre] [real] NULL," +
    "[rxtro] [real] NULL," +
    "[txtro] [real] NULL," +
    "[licence] [char](13) NULL," +
    "[satname] [char](16) NULL," +
    "[stata] [char](1) NULL," +
    "[nota] [char](4) NULL," +
    "[op2] [char](2) NULL," +
    "[antref] [int] NULL," +
    "[orbit] [char](2) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "[userid] [char](12) NULL," +
    "CONSTRAINT [PK_me_ante] PRIMARY KEY CLUSTERED " +
    "(" +
        "[location] ASC," +
        "[call1] ASC" +
    ")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
    ") ON [PRIMARY]";


        /*
        location
        call1
        txband
        rxband
        acodetx
        acoderx
        g_t
        lnat
        aht
        afslt
        afslr
        txhgmax
        rxhgmax
        satlongit
        satlong
        satlongs
        az
        el
        sarc1
        sarc2
        rxpre
        txpre
        rxtro
        txtro
        licence
        satname
        stata
        nota
        op2
        antref
        orbit
        mdate
        mtime
        userid
                 */










    }
}

```
