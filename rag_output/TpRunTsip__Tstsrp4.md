# Documented File: Tstsrp4.cs
**Repository Path:** `TpRunTsip\Tstsrp4.cs`
**Primary Layer:** `TpRunTsip`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;
using _NewLib;
using _Utillib;
using _DataStructures;
using System.IO;
using static System.Math;

namespace TpRunTsip
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

    /// <summary>
    /// Provides methods used to generate the Case Summary (.CASESUM) for Ts-Ts.
    /// </summary>
    public class Tstsrp4
    {
        /// <summary>
        /// This class extends the PrintLine base class to provide page header text
        /// specific to the report type.
        /// </summary>
        public class PrintLine4 : PrintLine
        {
            /// <summary>
            /// This method overrides that of the PrintLine base class to provide
            /// page header text specific to the report type.
            /// </summary>
            /// <param name=""></param>
            public override void PageHeader()
            {
                //...Log2.v("\nTstsrp4.PrintLine4.PageHeader()");

                //PageBefore();

                TsTsRp4Page(this);
            }
        }

        private static int mLastCase = -1;

        /// <summary>
        /// This method retrieves TSIP results data from the DB and produces
        /// the bulk of the text for the CASESUM report.
        /// </summary>
        /// <param name="tw"> - the TextWriter object assigned to the .CASESUM report.</param>
        /// <param name="ttName"> - the unique ID substring of the Ts DB table names.</param>
        /// <returns></returns>
        public static int TsTsRp4(TextWriter tw, string ttName)
        {
            Console.WriteLine("\nTstsrp4.TsTsRp4(): Entry");
            Console.WriteLine("\nTstsrp4.TsTsRp4(): TW:" + tw + " ttName:" + ttName);

            PrintLine4 pl = new PrintLine4();
            pl.OutFile(tw);

            pl.FormFeed();
            pl.PageHeader();

            TsTsSelRec4 rr = new TsTsSelRec4();

            SQLLEN IsNull;

            string cSQL;
            string cNameBuf;
            int nNumCases;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            cSQL = String.Format("SELECT a.caseno, subcaseno, intname1, a.interferer, vicname1, int1vic1dist, intoffax, vicoffax, " +
                "vicpwrrx, calctype, intpolar, vicpolar, intfreqtx, freqsep, calcico,	calcixp, reqdcalc, resti, intname2, vicname2	" +
                "FROM {0}.tt_{1}_site a,{0}.tt_{1}_ante b,{0}.tt_{1}_chan c " +
                "WHERE	a.intcall1 = b.intcall1 and a.intcall2 = b.intcall2 " +
                "and a.viccall1 = b.viccall1 and a.viccall2 = b.viccall2 and a.caseno   = b.caseno and a.interferer = b.interferer " +
                "and c.intcall1 = b.intcall1 and c.intcall2 = b.intcall2 and c.viccall1 = b.viccall1 and c.viccall2 = b.viccall2 " +
                "and c.caseno   = b.caseno and c.interferer = b.interferer and c.intanum = b.intanum and c.vicanum = b.vicanum " +
                "and c.intbndcde = b.intbndcde and c.vicbndcde = b.vicbndcde ORDER BY caseno, subcaseno, resti ",
                   Info.GlobalSchema, ttName);

            Console.Write(cSQL);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Console.WriteLine("\nTstsrp4.TsTsRp4(): ERROR: SQLExecDirect() failed on query:\n" + cSQL);
                Log2.e("\nTstsrp4.TsTsRp4(): ERROR: SQLExecDirect() failed on query:\n" + cSQL);
                Ssutil.DbGetDiagStmt(hStmt, "tstsrp401: Could not execute statement:-\n" + cSQL);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            nNumCases = 0;
            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (sqlRet == Constant.NOMORERECS)
                {
                    break;
                }
                else if (!ODBC.IsOK(sqlRet))
                {
                    Console.WriteLine("\nTstsrp4.TsTsRp4()(): ERROR: SQLFetch() failed.");
                    Log2.e("\nTstsrp4.TsTsRp4()(): ERROR: SQLFetch() failed.");
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp402: Failed Fetch:");
                    return Error.ODBC_FETCH_FAILED;
                }

                nNumCases++;

                //	Get the record...
                try
                {
                    Ssutil.DbStartGets();
                    Ssutil.DbGetInt(hStmt, 0, "caseno", out rr.caseno, out IsNull);
                    Ssutil.DbGetInt(hStmt, 0, "subcaseno", out rr.subcaseno, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "intname1", out rr.intname1, TsTsSelRec4.INTNAME1, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "interferer", out rr.interferer, TsTsSelRec4.INTERFERER, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "vicname1", out rr.vicname1, TsTsSelRec4.VICNAME1, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "int1vic1dist", out rr.int1vic1dist, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "intoffax", out rr.intoffax, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "vicoffax", out rr.vicoffax, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", out rr.vicpwrrx, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "calctype", out rr.calctype, TsTsSelRec4.CALCTYPE, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "intpolar", out rr.intpolar, TsTsSelRec4.INTPOLAR, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "vicpolar", out rr.vicpolar, TsTsSelRec4.VICPOLAR, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtxd", out rr.intfreqtxd, out IsNull);
                    rr.intfreqtxd /= 1000.0;
                    Ssutil.DbGetDouble(hStmt, 0, "freqsepd", out rr.freqsepd, out IsNull);
                    rr.freqsepd /= 1000.0;
                    Ssutil.DbGetDouble(hStmt, 0, "calcico", out rr.calcico, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp", out rr.calcixp, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "reqdcalc", out rr.reqdcalc, out IsNull);
                    Ssutil.DbGetDouble(hStmt, 0, "resti", out rr.resti, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "intname2", out rr.intname2, TsTsSelRec4.INTNAME2, out IsNull);
                    Ssutil.DbGetString(hStmt, 0, "vicname2", out rr.vicname2, TsTsSelRec4.VICNAME2, out IsNull);
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nTstsrp4.TsTsRp4()(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    Log2.e("\nTstsrp4.TsTsRp4()(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp402: Error retrieving field " + e.Message + ".");
                    return Error.ODBC_GET_FAILED;
                }

                if (rr.caseno != mLastCase)
                {
                    if (mLastCase > 0)
                    {
                        pl.Output();
                    }
                    pl.IntAt(0, "{0,3:D}", rr.caseno);
                    mLastCase = rr.caseno;
                }
                pl.IntAt(4, "{0,4:D}", rr.subcaseno);
                cNameBuf = rr.intname1;
                cNameBuf += " (";
                cNameBuf += rr.interferer;
                cNameBuf += ")";
                pl.LeftAt(9, cNameBuf);
                pl.DoubleAt(46, "{0,6:F2}", rr.int1vic1dist);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F1}", rr.intoffax);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F1}", rr.vicoffax);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F2}", rr.vicpwrrx);
                pl.LeftAt(-1, " ");
                pl.LeftAt(-1, rr.calctype);
                pl.LeftAt(-1, "  ");
                pl.LeftAt(-1, rr.intpolar);
                pl.LeftAt(-1, " ");
                pl.LeftAt(-1, rr.vicpolar);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,11:F3}", rr.freqsepd);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F1}", rr.calcico);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F1}", rr.calcixp);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F1}", rr.reqdcalc);
                pl.LeftAt(-1, " ");
                pl.DoubleAt(-1, "{0,6:F1}", rr.resti);
                pl.Output();

                cNameBuf = rr.vicname1.Trim();
                if (rr.interferer.Equals("P"))
                {
                    cNameBuf += " (E)";
                }
                else
                {
                    cNameBuf += " (P)";
                }
                pl.LeftAt(9, "--> ");
                pl.LeftAt(-1, cNameBuf);
                pl.Output();

            }
            pl.Output();
            pl.IntAt(0, "Number of reporting cases: {0}", nNumCases);
            pl.Output();

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);


            //...Log2.v("\nTstsrp4.TsTsRp4(): Exit");
            return 0;
        }

        /// <summary>
        /// This method produces a page of text giving a summary of the details
        /// for an interference case.
        /// </summary>
        /// <param name="pl"> - PrintLine object with header override.</param>
        /// <returns></returns>
        private static int TsTsRp4Page(PrintLine4 pl)
        {
            string cDate;
            string cTime;
            Console.WriteLine("In TsTsRp4Page");
            GenUtil.UtGetDateTime(out cDate, out cTime);

            pl.PageBefore();
            pl.LeftAt(0, "Frequency Coordination System Association");
            pl.IntAt(111, "Page:{0,3:D}", pl.PageNumber);
            pl.Output();

            pl.LeftAt(0, "MASTER DATA BASE -- TSTS Interference Study Report - Case Summary");
            pl.LeftAt(111, "Date:{0}", cDate);
            pl.Output();

            pl.LeftAt(0, "Project Code [{0}]", Info.ProjectCode);
            pl.Output();

            pl.LeftAt(0, "-");
            pl.RightAt(125, "-");
            pl.Output();

            pl.LeftAt(0, "Case/Sub");
            pl.LeftAt(9, "Interferer Station TX");
            pl.LeftAt(48, "I->V");
            pl.LeftAt(-1, "    Off");
            pl.LeftAt(-1, "    Off");
            pl.LeftAt(-1, "   RX");
            pl.LeftAt(-1, "    Ana");
            pl.LeftAt(-1, " P");
            pl.LeftAt(-1, " P");
            pl.LeftAt(-1, "   TX ->  RX");
            pl.LeftAt(-1, "       Calc.");
            pl.LeftAt(-1, "    Reqd.");
            pl.LeftAt(-1, "   Margn");
            pl.Output();

            pl.LeftAt(9, " --> Victim Station RX");
            pl.LeftAt(48, "Dist");
            pl.LeftAt(-1, "    AX>");
            pl.LeftAt(-1, "    AX>");
            pl.LeftAt(-1, "   RSL");
            pl.LeftAt(-1, "   Typ");
            pl.LeftAt(-1, " O");
            pl.LeftAt(-1, " O");
            pl.LeftAt(-1, "   Freq.Sep.");
            pl.LeftAt(-1, "    -I or C/I");
            pl.LeftAt(-1, "    -I");
            pl.LeftAt(124, "Check");
            pl.Output();

            pl.LeftAt(48, "(km)");
            pl.LeftAt(-1, "    TX");
            pl.LeftAt(-1, "     RX");
            pl.LeftAt(-1, "   (dBm)");
            pl.LeftAt(-1, "      L");
            pl.LeftAt(-1, " L");
            pl.LeftAt(-1, "   (MHz)");
            pl.LeftAt(-1, "       COPL");
            pl.LeftAt(-1, "   XPOL");
            pl.LeftAt(-1, "   C/I");
            pl.LeftAt(-1, "    (dBm)");
            pl.LeftAt(124, "Detail");
            pl.Output();

            pl.LeftAt(0, "---");
            pl.LeftAt(-1, " ---");
            pl.LeftAt(9, "---------------------------------");
            pl.LeftAt(46, "------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, " ---");
            pl.LeftAt(-1, "  I>V");
            pl.LeftAt(-1, " -----------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, "  -----");
            pl.LeftAt(-1, "  ------");
            pl.Output();

            return 0;
        }





    }
}

```
