using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T5aPolygons
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
    using _NewLib;
    using _Utillib;

    /// <summary>
    /// This class provides methods that create and populate a SQL Server table whose
    /// columns are: Tier5Area ID, ISED base rate code (URBAN, RURAL, REMOTE) and 
    /// MultiPolygon.
    /// </summary>
    public class PolygonsTable
    {
        /// <summary>
        /// This method creates a SQL Server table whose columns are: Tier5Area ID, 
        /// ISED base rate code (URBAN, RURAL, REMOTE) and MultiPolygon. If a table
        /// with the prescribed name already exists then it is dropped and recreated.
        /// </summary>
        /// <param name="polygonsTableName"></param>
        /// <returns></returns>
        public static int CreateTable(string polygonsTableName)
        {
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            DoQuery(hStmt, "SET ANSI_NULLS ON");

            DoQuery(hStmt, "SET QUOTED_IDENTIFIER ON");

            DoQuery(hStmt, "SET ANSI_PADDING ON");

            DoQuery(hStmt, String.Format("IF OBJECT_ID('{0}') IS NOT NULL DROP TABLE {0}", polygonsTableName));         

            sqlRet = DoQuery(hStmt, String.Format(CREATE_POLYGONS_TABLE, polygonsTableName));

            DoQuery(hStmt, "SET ANSI_PADDING OFF");

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            return sqlRet;
        }

        /// <summary>
        /// This method inserts prescribed Tier5Area objects as records in the prescribed SQL Server table.
        /// </summary>
        /// <param name="allTier5Areas"></param>
        /// <param name="polygonsTableName"></param>
        /// <returns></returns>
        public static int InsertRecords(List<Tier5Area> allTier5Areas, string polygonsTableName)
        {
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn;
            SQLHANDLE hStmt;

            hConn = Ssutil.NewConn();
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            StringBuilder sb = new StringBuilder();

            foreach (Tier5Area t5a in allTier5Areas)
            {
                sb.Clear();

                sb.Append("\nDECLARE @multipolygon GEOMETRY = 'MULTIPOLYGON("); 

                foreach (GeoBoundary geoBoundary in t5a.GeoBoundaries)
                {
                    string apostrophe = (geoBoundary == t5a.GeoBoundaries.First()) ? "" : ",";

                    sb.Append(apostrophe + "((");

                    foreach (GeoPoint point in geoBoundary.Points)
                    {
                        string comma = (point == geoBoundary.Points.First()) ? "" : ",";

                        // X-axis: point.Lat
                        // Y-axis: point.Lng
                        // So the 'map of Canada' is rotated by 90 degrees from the conventional.
                        sb.Append(String.Format(" {0} {1} {2}", comma, point.Lat, point.Lng));
                    }

                    sb.Append("))");
                }

                sb.Append(")';");

                sb.Append(String.Format("\nINSERT INTO {0} (tier5AreaID, baseRateCode, boundary) VALUES", polygonsTableName));

                sb.Append(String.Format(" ( '{0}', '{1}', @multipolygon);", t5a.ID, t5a.BaseRateCode));

                DoQuery(hStmt, sb.ToString());
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return sqlRet;
        }

        /// <summary>
        /// This method encapsulates the submission of a query
        /// to the ODBC / SQL Server.
        /// </summary>
        /// <param name="hStmt"> - an open ODBC statement handle</param>
        /// <param name="query"> - SQL query to be submitted.</param>
        /// <returns></returns>
        private static SQLRETURN DoQuery(SQLHANDLE hStmt, string query)
        {
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            //...Log2.v("\n" + ODBC.GetDiagnostics(hStmt, query));

            return sqlRet;
        }

        public const string CREATE_POLYGONS_TABLE = "" +
"CREATE TABLE {0} (" +
    "[tier5AreaID] [char](7) NOT NULL," +
    "[baseRateCode] [char](6) NOT NULL," +
    "[boundary] [geometry] NOT NULL," +
    " PRIMARY KEY CLUSTERED " +
    "(" +
        "[tier5AreaID] ASC" +
    ") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";




    }
}
