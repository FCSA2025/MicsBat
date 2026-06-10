# Documented File: AggInt.cs
**Repository Path:** `TpRunTsip20260126\AggInt.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace TpRunTsip
{
    using SQLLEN = Int64;
    using SQLHANDLE = IntPtr;
    using _NewLib;

    /// <summary>
    /// Provides methods that support the production of the
    /// Aggregate Interference Report (.AGGINT).
    /// </summary>
    public class AggInt
    {
        /// <summary>
        /// This method manages the production of the Aggregate Interference Report (.AGGINT).  
        /// </summary>
        /// <param name="tw"> - TextWriter object that the report is written to.</param>
        /// <param name="viewName"> - the name of the DB table that is the source for the data.</param>
        /// <param name="cSphereCalc"> - the type of the loss calculation.</param>
        /// <param name="dCull"> - the culling margin (dB).</param>
        public static void AggIntRep(TextWriter tw,                  /*	The output file handle		*/
                             string viewName,               /*	The name of the table 		*/
                             string cSphereCalc,        /*	The loss calculation 			*/
                             double dCull)                  /*	Culling Margin						*/
        {
            //...Log2.v("\nAggInt.AggIntRep(): Entry");

            tw.Write("                  FREQUENCY CO-ORDINATION SYSTEM ASSOCIATION");
            tw.Write("\r\n                       Aggregate Interference Report");
            tw.Write("\r\n                       for {0}, Culling Margin: {1,6:F1}",
                          viewName, dCull);
            tw.Write("\r\n");
            tw.Write("\r\nVictim    From        Ant  Frequency Rx Pwr C   AggInt   RqCo Margin");
            tw.Write("\r\n");

            AggIntRecs(tw, viewName, dCull, cSphereCalc, 1);  /*The 1 is for a report */

            tw.Write("\r\n");
            tw.Flush();

            //...Log2.v("\nAggInt.AggIntRep(): Exit");
            return;
        }

        /// <summary>
        /// This method tests whether a record survives the 'cull', or not. 
        /// </summary>
        /// <param name="tRec"> - TtChan object to be tested.</param>
        /// <param name="cSphereCalc"> - the type of loss calculation to be used.</param>
        /// <param name="dCull"> - the margin to be used for culling (dB).</param>
        /// <returns> - true or false. </returns>
        public static bool PassCull(TtChan tRec, string cSphereCalc, double dCull)
        {
            bool result;

            if (tRec.resti > dCull)
            {
                /*	The LOS results fail the cull.  Check to OHloss if necessary */
                if (cSphereCalc.Equals("5"))
                {
                    /*	Ohloss calculations.  */
                    if (tRec.ohresult <= 0 || tRec.ohresult >= 100)
                    {
                        result = false;
                    }
                    else
                    {
                        /*	It is an ohloss condition.  */
                        if (tRec.resti80 > dCull)
                        {
                            /*	Failed the cull, try the 99. We alter the culling criteria  */
                            if (tRec.calctype.Equals("-I") ||
                                 tRec.calctype.Equals("I"))
                            {
                                /* It is -I calculations */
                                if (tRec.resti99 > dCull + 10.0)
                                {
                                    /* -I okay  */
                                    result = false;
                                }
                                else
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                /* C/I */
                                if (tRec.resti99 > dCull - 10.0)
                                {
                                    /* C/I okay  */
                                    result = false;
                                }
                                else
                                {
                                    result = true;
                                }
                            }
                        }
                        else
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    /*	Spherical earth, and fails the cull */
                    result = false;
                }
            }
            else
            {
                result = true;        /*	Passes the cull.  We want it.  */
            }

            return result;
        }

        /// <summary>
        /// This method produces the Aggregate Interference (.AGGINT) reports.
        /// </summary>
        /// <param name="tw"> - TextWriter object assigned to the report.</param>
        /// <param name="tLine"> - AggIntLine object providing the data for the report.</param>
        /// <param name="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        public static void AgIntOutRec(TextWriter tw, AggIntLine tLine, int nRepType)
        {
            if (tLine == null)
            {
                return;
            }

            /*	Calculate the display values of the aggregate interference */
            double dAIN = 0.0;
            double dInt = 0.0;
            double dAIN80 = 0.0;
            double dAIN99 = 0.0;
            double dInt80 = 0.0;
            double dInt99 = 0.0;
            double dMargin = 0.0;
            double dMargin80 = 0.0;
            double dMargin99 = 0.0;

            if (tLine.dAin > 0.0)
            {
                dAIN = 10.0 * Log10(tLine.dAin);
            }
            dInt = dAIN;

            if (tLine.calctype.Equals("C/I") ||
                tLine.calctype.Equals("C"))
            {
                dInt = tLine.vicpwrrx - dAIN;
                dMargin = dInt - tLine.reqdcalc;
            }
            else
            {
                /*	The margin calculation depends on -I or C/I. */
                dMargin = tLine.reqdcalc - dInt;
            }

            /*	For ohloss, we have some extra lines */
            if (tLine.cSphereCalc.Equals("5"))
            {
                if (tLine.dAin80 > 0.0)
                {
                    dAIN80 = 10.0 * Log10(tLine.dAin80);
                }
                if (tLine.dAin99 > 0.0)
                {
                    dAIN99 = 10.0 * Log10(tLine.dAin99);
                }
                dInt80 = dAIN80;
                dInt99 = dAIN99;

                if (tLine.calctype.Equals("C/I") ||
                    tLine.calctype.Equals("C"))
                {
                    if (tLine.dAin80 > 0.0)
                    {
                        dInt80 = tLine.vicpwrrx - dAIN80;
                        dMargin80 = dInt80 - tLine.reqdcalc;
                    }
                    if (tLine.dAin99 > 0.0)
                    {
                        dInt99 = tLine.vicpwrrx - dAIN99;
                        dMargin99 = dInt99 - (tLine.reqdcalc - 10.0);  /* For C/I subtract 10*/
                    }
                }
                else
                {
                    if (tLine.dAin80 > 0.0)
                    {
                        dMargin80 = tLine.reqdcalc - dInt80;
                    }
                    if (tLine.dAin99 > 0.0)
                    {
                        dMargin99 = (tLine.reqdcalc + 10.0) - dInt99;   /* for -I add 10  */
                    }
                }
            }

            switch (nRepType)
            {
                case 1: /*	Report */
                    tw.Write("\r\n{0,-10}{1,-10}{2,5:D}{3,11:F0} {4,6:F1} {5,-3}",
                        tLine.viccall1,
                        tLine.viccall2,
                        tLine.vicanum,
                        tLine.vicfreqrx,
                        tLine.vicpwrrx,
                        tLine.calctype
                                    );
                    if (tLine.dAin > 0.0)
                    {
                        /*	There were some cases  */
                        tw.Write("{0,7:F1}{1,7:F1}{2,7:F1}", dInt, tLine.reqdcalc, dMargin);
                    }
                    else
                    {
                        tw.Write("         - No Cases -");
                    }

                    if (tLine.cSphereCalc.Equals("5"))
                    {
                        if (tLine.dAin80 > 0.0)
                        {
                            tw.Write("\r\n{0,47}{1,7:F1}       {2,7:F1}",
                                            "80%",
                                            dInt80,
                                            dMargin80
                                         );
                        }
                        else
                        {
                            tw.Write("\r\n{0,47}         - No Cases -", "80%");
                        }
                        if (tLine.dAin99 > 0.0)
                        {
                            tw.Write("\r\n{0,47}{1,7:F1}       {2,7:F1}",
                                         "99%",
                                         dInt99,
                                         dMargin99
                                         );
                        }
                        else
                        {
                            tw.Write("\r\n{0,47}         - No Cases -", "99%");
                        }
                    }
                    break;

                case 2: /*	CSV file */
                    tw.Write("{0},{1},{2},{3:F6},{4:F6},{5},{6:F6},{7:F6},{8:F6}",
                                    tLine.viccall1,
                                    tLine.viccall2,
                                    tLine.vicanum,
                                    tLine.vicfreqrx,
                                    tLine.vicpwrrx,
                                    tLine.calctype,
                                    dInt,
                                    tLine.reqdcalc,
                                    dMargin
                                    );
                    if (tLine.cSphereCalc.Equals("5"))
                    {
                        if (tLine.dAin80 > 0.0)
                        {
                            tw.Write(",{0:F6},{1:F6}",
                                            dInt80,
                                            dMargin80
                                         );
                        }
                        else
                        {
                            tw.Write(",0,0");
                        }
                        if (tLine.dAin99 > 0.0)
                        {
                            tw.Write(",{0:F6},{1:F6}",
                                            dInt99,
                                            dMargin99
                                            );
                        }
                        else
                        {
                            tw.Write(",0,0");
                        }
                    }
                    tw.Write("\r\n");
                    break;

                default:
                    TpRunTsip.mTW_ERR.Write("\r\n*ERROR* Invalid Report Type in Report.\r\n");
                    break;
            }

            return;
        }

        /// <summary>
        /// This method creates and populates a new AggIntLine object that is
        /// used later to produce the aggregate interference report (.AGGINT).  
        /// </summary>
        /// <param name="tRec"> - TtChan object providing source data.</param>
        /// <param name="tLine"> - AggIntLine object to be used for .AGGINT report generation.</param>
        /// <param name="cSphereCalc"> - the type of loss calaculation to be used.</param>
        /// <param name="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        public static void AgIntNewRec(TtChan tRec,
                                        out AggIntLine tLine,
                                        string cSphereCalc,
                                        int nRepType)
        {
            // 'out' requirement.
            tLine = null;

            if (tRec == null)
            {
                return;
            }

            tLine = new AggIntLine();

            tLine.viccall1 = tRec.viccall1;
            tLine.viccall2 = tRec.viccall2;
            tLine.vicanum = tRec.vicanum;
            tLine.vicfreqrx = tRec.vicfreqrx;
            tLine.vicpwrrx = tRec.vicpwrrx;
            tLine.calctype = tRec.calctype;
            tLine.ctxinttraftx = tRec.ctxinttraftx;
            tLine.ctxvictrafrx = tRec.ctxvictrafrx;
            tLine.ctxeqpt = tRec.ctxeqpt;
            tLine.reqdcalc = tRec.rqco;
            tLine.resti = tRec.resti;

            tLine.dEINCO = 0.0;
            tLine.dEINCO80 = 0.0;
            tLine.dEINCO99 = 0.0;
            tLine.dAin = 0.0;
            tLine.dAin80 = 0.0;
            tLine.dAin99 = 0.0;

            tLine.cSphereCalc = cSphereCalc;

            return;
        }

        /// <summary>
        /// This method further processes a partially-populated AggIntLine object to fill the remaining field values needed to produce aggregate interference report (.AGGINT).  
        /// </summary>
        /// <param name="tRec"> - TtChan object to be tested.</param>
        /// <param name="tLine"> - AggIntLine object providing the data for the report.</param>
        /// <param name="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        /// <param name="dCull"> - the culling margin (dB).</param>
        public static void AgIntProcRec(TtChan tRec,
                          ref AggIntLine tLine,
                          int nRepType,
                          double dCull)
        {
            if (tRec == null)
            {
                return;
            }

            double dIN;
            double dIN80;
            double dIN99;
            double dICF;
            double deinco;

            if (tRec.calctype.Equals("-I") ||
                tRec.calctype.Equals("I"))
            {
                /*	It's -I */
                dICF = tRec.reqdcalc - tRec.rqco;
                tLine.dEINCO = tRec.calcico - dICF;
                if (tLine.cSphereCalc.Equals("5") &&
                        tRec.ohresult > 0 &&
                        tRec.ohresult < 100)
                {
                    /*	Handle the ohloss calcs */
                    tLine.dEINCO80 = tRec.calcico80 - dICF;
                    tLine.dEINCO99 = tRec.calcico99 - dICF;
                }
                else
                {
                    tLine.dEINCO80 = 0.0;
                    tLine.dEINCO99 = 0.0;
                }

                /*	Store the worst case requirement in the required calc field */
                if (tLine.reqdcalc > tRec.rqco)
                {
                    tLine.reqdcalc = tRec.rqco;
                }
            }
            else
            {
                /* It's C/I */
                dICF = tRec.rqco - tRec.reqdcalc;
                dIN = tRec.vicpwrrx - tRec.calcico;
                tLine.dEINCO = dIN - dICF;
                if (tLine.cSphereCalc.Equals("5") &&
                        tRec.ohresult > 0 &&
                        tRec.ohresult < 100)
                {
                    dIN80 = tRec.vicpwrrx - tRec.calcico80;
                    tLine.dEINCO80 = dIN80 - dICF;
                    dIN99 = tRec.vicpwrrx - tRec.calcico99;
                    tLine.dEINCO99 = dIN99 - dICF;
                }
                else
                {
                    tLine.dEINCO80 = 0.0;
                    tLine.dEINCO99 = 0.0;
                }
                /*	Store the worst case requirement in the required calc field */
                if (tLine.reqdcalc < tRec.rqco)
                {
                    tLine.reqdcalc = tRec.rqco;
                }
            }

            /*	Accumulate the einco only for those values that fail the cull. */
            if (tRec.resti <= dCull)
            {
                deinco = Pow(10.0, tLine.dEINCO / 10.0);
                tLine.dAin += deinco;
            }
            if (tLine.cSphereCalc.Equals("5") &&
                    tRec.ohresult > 0 &&
                    tRec.ohresult < 100)
            {
                if (tRec.resti80 <= dCull)
                {
                    deinco = Pow(10.0, tLine.dEINCO80 / 10.0);
                    tLine.dAin80 += deinco;
                }
                if (tRec.resti99 <= (dCull + 10.0))
                {
                    deinco = Pow(10.0, tLine.dEINCO99 / 10.0);
                    tLine.dAin99 += deinco;
                }
            }

            return;
        }

        /// <summary>
        /// This method produces the records for the aggregate interference report - 1181 - GJS.  
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="viewName"> - The name of the table</param>
        /// <param name="dCull"> - Culling Margin</param>
        /// <param name="cSphereCalc"> - The type of loss calc.</param>
        /// <param name="nRepType"> - 1: Report, 2: CSV</param>
        /// <returns></returns>
        public static int AggIntRecs(TextWriter tw,          /*	The output file handle		*/
                             string viewName,      /*	The name of the table 		*/
                             double dCull,       /*	Culling Margin						*/
                               string cSphereCalc,/*	The type of loss calc.		*/
                             int nRepType)     /*	1: Report, 2: CSV					*/
        {
            int nRet;
            string cLastCall;
            double dLastFrx;
            AggIntLine tLine = new AggIntLine(); // null;      /*	The output line */
            TtChan tRec;
            SQLLEN[] chanNulls;
            string cTable;
            int nRead;
            int nPassed;
            SQLHANDLE hConn;
            SQLHANDLE hStmt;

            /*	Set up the actual channel table name */
            cTable = String.Format("tt_{0}_chan", viewName);

            TtDynChan.TtChanPrepareRead(cTable, "viccall1,viccall2,vicanum,vicfreqrx", null, out hConn, out hStmt);

            cLastCall = "";
            dLastFrx = 0.0;
            nRead = 0;
            nPassed = 0;
            while (true)
            {
                nRet = TtDynChan.TtChanRead(hStmt, out tRec, out chanNulls);
                if (nRet != 0)
                {
                    break;
                }
                nRead++;

                /*	Perform the culling */
                if (!AggInt.PassCull(tRec, cSphereCalc, dCull))
                {
                    continue;
                }
                nPassed++;

                /* Do a breakpoint sort on the call1 and the freqrx */
                if (!tRec.viccall1.Equals(cLastCall))
                {
                    if (!cLastCall.Equals(""))
                    {
                        /*	Output the last record */
                        AggInt.AgIntOutRec(tw, tLine, nRepType);
                    }
                    /* agintnewrec(fRep, &tRec, &tLine, nRepType); */
                    cLastCall = tRec.viccall1;
                    dLastFrx = -1.0;
                }

                if (tRec.vicfreqrx != dLastFrx)
                {
                    if (dLastFrx > 0.0)
                    {
                        AggInt.AgIntOutRec(tw, tLine, nRepType);
                    }
                    AggInt.AgIntNewRec(tRec, out tLine, cSphereCalc, nRepType);
                    dLastFrx = tRec.vicfreqrx;
                }

                /*	Process the record */
                AggInt.AgIntProcRec(tRec, ref tLine, nRepType, dCull);
            }
            if (dLastFrx != 0.0 || !cLastCall.Equals(""))
            {
                AggInt.AgIntOutRec(tw, tLine, nRepType);        /*	Output the last one */
            }

            AggIntStatRec(tw, nRepType, nRead, nPassed);

            // Release ODBC resources.
            TtDynChan.TtChanClose(hConn, hStmt);

            return (0);
        }

        /// <summary>
        /// This method produces the statistics message for the .AGGINT report.  
        /// </summary>
        /// <param name="tw"> - TextWriter object that the report is written to.</param>
        /// <param name="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        /// <param name="nRead"> - number of records read.</param>
        /// <param name="nPassed"> - number of records that survived culling.</param>
        public static void AggIntStatRec(TextWriter tw,
                       int nRepType,
                                     int nRead,
                                     int nPassed)
        {
            switch (nRepType)
            {
                case 1:
                    tw.Write("\r\n\r\nRecords Read........: {0,5:D}\r\nRecords Passing Cull: {1,5:D}\r\n",
                                                nRead, nPassed);
                    break;

                case 2:
                    break;

                default:
                    TpRunTsip.mTW_ERR.Write("\r\n*ERROR* Invalid Report Type in Statistics.\r\n");
                    break;
            }
        }

        /// <summary>
        /// This method produces the Aggregate Interference report encoded in CSV format.  
        /// </summary>
        /// <param name="tw"> - TextWriter object that the report is written to.</param>
        /// <param name="viewName"> - the name of the DB table that is the source for the data.</param>
        /// <param name="cSphereCalc"> - the type of the loss calculation.</param>
        /// <param name="dCull"> - the culling margin (dB).</param>
        public static void AggIntCSV(TextWriter tw,                  /*	The output file handle		*/
                             string viewName,               /*	The name of the table 		*/
                             string cSphereCalc,        /*	The loss calculation 			*/
                             double dCull)                  /*	Culling Margin						*/
        {
            tw.Write("\r\nviccall1,viccall2,vicanum,vicfreqrx,vicpwrrx,calctype,aggint,rqco,margin");
            if (cSphereCalc.Equals("5"))
            {
                tw.Write(",aggint80,margin80,aggint99,margin99");
            }
            tw.Write("\r\n");

            AggIntRecs(tw, viewName, dCull, cSphereCalc, 2);  /*	The 2 is for a CSV file */

            return;
        }







    }
}

```
