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
    /// This class has fields that are isomorphic with the DB table <b>main.me_azim</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class MeAzim
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LOCATION_SZ)]
        public string location;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALL1_SZ)]
        public string call1;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float azim;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float elev;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float dist;
        [MarshalAsAttribute(UnmanagedType.R4)]
        public float loss;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MDATE_SZ)]
        public string mdate;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = MTIME_SZ)]
        public string mtime;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = USERID_SZ)]
        public string userid;

        //----------------------------------------------------------------------
        public const int NUM_COLUMNS = 9;

        public const int LOCATION_SZ = Constant.LOCATION_SZ;
        public const int CALL1_SZ = Constant.CALLSIGN_SZ;
        public const int MDATE_SZ = Constant.DATE_SZ;
        public const int MTIME_SZ = Constant.TIME_SZ;
        public const int USERID_SZ = 13;

        public const int LOCATION = 0;
        public const int CALL1 = 1;
        public const int AZIM = 2;
        public const int ELEV = 3;
        public const int DIST = 4;
        public const int LOSS = 5;
        public const int MDATE = 6;
        public const int MTIME = 7;
        public const int USERID = 8;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "location", "call1", "azim", "elev", "dist", "loss", "mdate", "mtime", "userid" };

        //----------------------------------------------------------------------

        /// <summary>
        /// This method returns a string that concatenates the 'key'
        /// fields {location, call1, azim}.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string KeysToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("location = " + location);
            sb.Append("   call1 = " + call1);
            sb.Append("    azim = " + azim);
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

            sb.AppendLine("========== MeAzim:");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "location =  " + location);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "call1 =     " + call1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "azim =      " + azim);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "elev =      " + elev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "dist =      " + dist);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "loss =      " + loss);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mdate =     " + mdate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mtime =     " + mtime);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "userid =    " + userid);

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

            sb.Append(nullInds[MeAzim.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',");
            sb.Append(nullInds[MeAzim.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',");
            sb.Append(nullInds[MeAzim.AZIM] == Constant.DB_NULL ? "NULL," : "'" + azim.ToString() + "',");
            sb.Append(nullInds[MeAzim.ELEV] == Constant.DB_NULL ? "NULL," : "'" + elev.ToString() + "',");
            sb.Append(nullInds[MeAzim.DIST] == Constant.DB_NULL ? "NULL," : "'" + dist.ToString() + "',");
            sb.Append(nullInds[MeAzim.LOSS] == Constant.DB_NULL ? "NULL," : "'" + loss.ToString() + "',");
            sb.Append(nullInds[MeAzim.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',");
            sb.Append(nullInds[MeAzim.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',");
            sb.Append(nullInds[MeAzim.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'");

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

            sb.Append(columnNames[MeAzim.LOCATION] + "=" + (nullInds[MeAzim.LOCATION] == Constant.DB_NULL ? "NULL," : "'" + location.ToString() + "',"));
            sb.Append(columnNames[MeAzim.CALL1] + "=" + (nullInds[MeAzim.CALL1] == Constant.DB_NULL ? "NULL," : "'" + call1.ToString() + "',"));
            sb.Append(columnNames[MeAzim.AZIM] + "=" + (nullInds[MeAzim.AZIM] == Constant.DB_NULL ? "NULL," : "'" + azim.ToString() + "',"));
            sb.Append(columnNames[MeAzim.ELEV] + "=" + (nullInds[MeAzim.ELEV] == Constant.DB_NULL ? "NULL," : "'" + elev.ToString() + "',"));
            sb.Append(columnNames[MeAzim.DIST] + "=" + (nullInds[MeAzim.DIST] == Constant.DB_NULL ? "NULL," : "'" + dist.ToString() + "',"));
            sb.Append(columnNames[MeAzim.LOSS] + "=" + (nullInds[MeAzim.LOSS] == Constant.DB_NULL ? "NULL," : "'" + loss.ToString() + "',"));
            sb.Append(columnNames[MeAzim.MDATE] + "=" + (nullInds[MeAzim.MDATE] == Constant.DB_NULL ? "NULL," : "'" + mdate.ToString() + "',"));
            sb.Append(columnNames[MeAzim.MTIME] + "=" + (nullInds[MeAzim.MTIME] == Constant.DB_NULL ? "NULL," : "'" + mtime.ToString() + "',"));
            sb.Append(columnNames[MeAzim.USERID] + "=" + (nullInds[MeAzim.USERID] == Constant.DB_NULL ? "NULL" : "'" + userid.ToString() + "'"));

            return sb.ToString();
        }




        //----------------------------------------------------------------------

        public const string CREATE_TABLE = "" +
"CREATE TABLE [{0}].[me_azim](" +
    "[location] [char](10) NOT NULL," +
    "[call1] [char](9) NOT NULL," +
    "[azim] [real] NOT NULL," +
    "[elev] [real] NULL," +
    "[dist] [real] NULL," +
    "[loss] [real] NULL," +
    "[mdate] [char](10) NULL," +
    "[mtime] [char](8) NULL," +
    "[userid] [char](12) NULL," +
    "CONSTRAINT [PK_me_azim] PRIMARY KEY CLUSTERED " +
    "(" +
        "[location] ASC," +
        "[call1] ASC," +
        "[azim] ASC" +
    ")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";

        //----------------------------------------------------------------------

        //----------------------------------------------------------------------

        //----------------------------------------------------------------------

        /*
        location
        call1
        azim
        elev
        dist
        loss
        mdate
        mtime
        userid
        */
    }
}
