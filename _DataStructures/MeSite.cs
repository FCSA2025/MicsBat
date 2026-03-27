using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace _DataStructures
{
    using _NewLib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with the DB table <b>main.me_site</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class MeSite
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOCATION_SZ)]
        public string location;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NAME_SZ)]
        public string name;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = PROV_SZ)]
        public string prov;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPER_SZ)]
        public string oper;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int latit;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLATIT_SZ)]
        public string strlatit;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLATITS_SZ)]
        public string strlatits;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int longit;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLONGIT_SZ)]
        public string strlongit;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STRLONGITS_SZ)]
        public string strlongits;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float grnd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RADIO_SZ)]
        public string radio;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rain;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SDATE_SZ)]
        public string sdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATS_SZ)]
        public string stats;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NOTS_SZ)]
        public string nots;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = OPRTYP_SZ)]
        public string oprtyp;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = REG_SZ)]
        public string reg;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;

        //-----------------------------------------------------------------------

        public const int NUM_COLUMNS = 21;

        //-----------------------------------------------------------------------

        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int NAME_SZ = 17;
        public const int PROV_SZ = 3;
        public const int OPER_SZ = 7;
        public const int STRLATIT_SZ = 12;
        public const int STRLATITS_SZ = 2;
        public const int STRLONGIT_SZ = 13;
        public const int STRLONGITS_SZ = 2;
        public const int RADIO_SZ = 3;
        public const int SDATE_SZ = 26;
        public const int STATS_SZ = 2;
        public const int NOTS_SZ = 5;
        public const int OPRTYP_SZ = 3;
        public const int REG_SZ = 3;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        public const int USERID_SZ = 13;

        //-----------------------------------------------------------------------

        public const int LOCATION = 0;
        public const int NAME = 1;
        public const int PROV = 2;
        public const int OPER = 3;
        public const int LATIT = 4;
        public const int STRLATIT = 5;
        public const int STRLATITS = 6;
        public const int LONGIT = 7;
        public const int STRLONGIT = 8;
        public const int STRLONGITS = 9;
        public const int GRND = 10;
        public const int RADIO = 11;
        public const int RAIN = 12;
        public const int SDATE = 13;
        public const int STATS = 14;
        public const int NOTS = 15;
        public const int OPRTYP = 16;
        public const int REG = 17;
        public const int MDATE = 18;
        public const int MTIME = 19;
        public const int USERID = 20;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "location", "name", "prov", "oper", "latit", "strlatit", "strlatits", "longit", "strlongit", "strlongits", "grnd", "radio", "rain", "sdate", "stats", "nots", "oprtyp", "reg", "mdate", "mtime", "userid" };

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

            sb.AppendLine("========== MeSite:");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location =   " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "name =       " + name);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "prov =       " + prov);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oper =       " + oper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "latit =      " + latit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "strlatit =   " + strlatit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "strlatits =  " + strlatits);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "longit =     " + longit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "strlongit =  " + strlongit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "strlongits = " + strlongits);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "grnd =       " + grnd);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "radio =      " + radio);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rain =       " + rain);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sdate =      " + sdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "stats =      " + stats);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "nots =       " + nots);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "oprtyp =     " + oprtyp);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reg =        " + reg);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =      " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =      " + mtime);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "userid =     " + userid);

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
            // The field 'name' could contain single quotes that will cause an SQL error.
            // The solution is to 'escape' the single quote by 'doubling up',
            // e.g. O'Brian becomes O''Brian.
            string nameEscaped = name.Replace("'", "''");

            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[MeSite.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',");
            sb.Append(nullInds[MeSite.NAME] == Constant.DB_NULL ? "NULL," : "'" + nameEscaped + "',");
            sb.Append(nullInds[MeSite.PROV] == Constant.DB_NULL ? "NULL," : "'" + prov.ToString() + "',");
            sb.Append(nullInds[MeSite.OPER] == Constant.DB_NULL ? "NULL," : "'" + oper.ToString() + "',");
            sb.Append(nullInds[MeSite.LATIT] == Constant.DB_NULL ? "NULL," : "'" + latit.ToString() + "',");
            sb.Append(nullInds[MeSite.STRLATIT] == Constant.DB_NULL ? "NULL," : "'" + strlatit.ToString() + "',");
            sb.Append(nullInds[MeSite.STRLATITS] == Constant.DB_NULL ? "NULL," : "'" + strlatits.ToString() + "',");
            sb.Append(nullInds[MeSite.LONGIT] == Constant.DB_NULL ? "NULL," : "'" + longit.ToString() + "',");
            sb.Append(nullInds[MeSite.STRLONGIT] == Constant.DB_NULL ? "NULL," : "'" + strlongit.ToString() + "',");
            sb.Append(nullInds[MeSite.STRLONGITS] == Constant.DB_NULL ? "NULL," : "'" + strlongits.ToString() + "',");
            sb.Append(nullInds[MeSite.GRND] == Constant.DB_NULL ? "NULL," : "'" + grnd.ToString() + "',");
            sb.Append(nullInds[MeSite.RADIO] == Constant.DB_NULL ? "NULL," : "'" + radio.ToString() + "',");
            sb.Append(nullInds[MeSite.RAIN] == Constant.DB_NULL ? "NULL," : "'" + rain.ToString() + "',");
            sb.Append(nullInds[MeSite.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',");
            sb.Append(nullInds[MeSite.STATS] == Constant.DB_NULL ? "NULL," : "'" + stats.ToString() + "',");
            sb.Append(nullInds[MeSite.NOTS] == Constant.DB_NULL ? "NULL," : "'" + nots.ToString() + "',");
            sb.Append(nullInds[MeSite.OPRTYP] == Constant.DB_NULL ? "NULL," : "'" + oprtyp.ToString() + "',");
            sb.Append(nullInds[MeSite.REG] == Constant.DB_NULL ? "NULL," : "'" + reg.ToString() + "',");
            sb.Append(nullInds[MeSite.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MeSite.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MeSite.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'");

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
            // The field 'name' could contain single quotes that will cause an SQL error.
            // The solution is to 'escape' the single quote by 'doubling up',
            // e.g. O'Brian becomes O''Brian.
            string nameEscaped = name.Replace("'", "''");

            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[MeSite.LOCATION] + "=" + (nullInds[MeSite.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',"));
            sb.Append(columnNames[MeSite.NAME] + "=" + (nullInds[MeSite.NAME] == Constant.DB_NULL ? "NULL," : "'" + nameEscaped + "',"));
            sb.Append(columnNames[MeSite.PROV] + "=" + (nullInds[MeSite.PROV] == Constant.DB_NULL ? "NULL," : "'" + prov.ToString() + "',"));
            sb.Append(columnNames[MeSite.OPER] + "=" + (nullInds[MeSite.OPER] == Constant.DB_NULL ? "NULL," : "'" + oper.ToString() + "',"));
            sb.Append(columnNames[MeSite.LATIT] + "=" + (nullInds[MeSite.LATIT] == Constant.DB_NULL ? "NULL," : "'" + latit.ToString() + "',"));
            sb.Append(columnNames[MeSite.STRLATIT] + "=" + (nullInds[MeSite.STRLATIT] == Constant.DB_NULL ? "NULL," : "'" + strlatit.ToString() + "',"));
            sb.Append(columnNames[MeSite.STRLATITS] + "=" + (nullInds[MeSite.STRLATITS] == Constant.DB_NULL ? "NULL," : "'" + strlatits.ToString() + "',"));
            sb.Append(columnNames[MeSite.LONGIT] + "=" + (nullInds[MeSite.LONGIT] == Constant.DB_NULL ? "NULL," : "'" + longit.ToString() + "',"));
            sb.Append(columnNames[MeSite.STRLONGIT] + "=" + (nullInds[MeSite.STRLONGIT] == Constant.DB_NULL ? "NULL," : "'" + strlongit.ToString() + "',"));
            sb.Append(columnNames[MeSite.STRLONGITS] + "=" + (nullInds[MeSite.STRLONGITS] == Constant.DB_NULL ? "NULL," : "'" + strlongits.ToString() + "',"));
            sb.Append(columnNames[MeSite.GRND] + "=" + (nullInds[MeSite.GRND] == Constant.DB_NULL ? "NULL," : "'" + grnd.ToString() + "',"));
            sb.Append(columnNames[MeSite.RADIO] + "=" + (nullInds[MeSite.RADIO] == Constant.DB_NULL ? "NULL," : "'" + radio.ToString() + "',"));
            sb.Append(columnNames[MeSite.RAIN] + "=" + (nullInds[MeSite.RAIN] == Constant.DB_NULL ? "NULL," : "'" + rain.ToString() + "',"));
            sb.Append(columnNames[MeSite.SDATE] + "=" + (nullInds[MeSite.SDATE] == Constant.DB_NULL ? "NULL," : "'" + sdate.ToString() + "',"));
            sb.Append(columnNames[MeSite.STATS] + "=" + (nullInds[MeSite.STATS] == Constant.DB_NULL ? "NULL," : "'" + stats.ToString() + "',"));
            sb.Append(columnNames[MeSite.NOTS] + "=" + (nullInds[MeSite.NOTS] == Constant.DB_NULL ? "NULL," : "'" + nots.ToString() + "',"));
            sb.Append(columnNames[MeSite.OPRTYP] + "=" + (nullInds[MeSite.OPRTYP] == Constant.DB_NULL ? "NULL," : "'" + oprtyp.ToString() + "',"));
            sb.Append(columnNames[MeSite.REG] + "=" + (nullInds[MeSite.REG] == Constant.DB_NULL ? "NULL," : "'" + reg.ToString() + "',"));
            sb.Append(columnNames[MeSite.MDATE] + "=" + (nullInds[MeSite.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MeSite.MTIME] + "=" + (nullInds[MeSite.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MeSite.USERID] + "=" + (nullInds[MeSite.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'"));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a GeoPoint object corresponding to the (lat, long) location
        /// of this MeSite object.
        /// </summary>
        /// <returns></returns>
        public GeoPoint PositionAsGeoPoint()
        {
            double lat;
            double lng;

            // Latitude.
            lat = latit * Constant.CENTISECONDS_TO_DEGREES;
            if (strlatits == "S") lat = -lat;

            // Longitude.
            lng = longit * Constant.CENTISECONDS_TO_DEGREES;
            if (strlongits == "W") lng = -lng;

            return new GeoPoint(lat, lng);
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[me_site] " +
"(" +
    "[location] [char](10) NOT NULL," +
    "[name] [char](16) NULL," +
    "[prov] [char](2) NULL," +
    "[oper] [char](6) NULL," +
    "[latit] [int] NULL," +
    "[strlatit] [char](11) NULL," +
    "[strlatits] [char](1) NULL," +
    "[longit] [int] NULL," +
    "[strlongit] [char](12) NULL," +
    "[strlongits] [char](1) NULL," +
    "[grnd] [real] NULL," +
    "[radio] [char](2) NULL," +
    "[rain] [smallint] NULL," +
    "[sdate] [char](10) NULL," +
    "[stats] [char](1) NULL," +
    "[nots] [char](4) NULL," +
    "[oprtyp] [char](2) NULL," +
    "[reg] [char](2) NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "[userid] [char](12) NULL," +
    "CONSTRAINT [PK_me_site] PRIMARY KEY CLUSTERED " +
    "(" +
        "[location] ASC" +
    ") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";

        /*
                location
                name
                prov
                oper
                latit
                strlatit
                strlatits
                longit
                strlongit
                strlongits
                grnd
                radio
                rain
                sdate
                stats
                nots
                oprtyp
                reg
                mdate
                mtime
                userid
        */


    }
}
