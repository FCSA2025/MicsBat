using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
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

    class BndcdeOverlaps
    {
        private struct BandTally
        {
            public SdBand sdBand;
            public List<SdBand> overlapLeftList;
            public List<SdBand> containsList;
            public List<SdBand> overlapRightList;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Go(string[] args)
        {
            int retVal = -666;

            try
            {
#if true
                // Turn on developmental logging.
                string mLog2FilePath = @"d:MicsBatchLogs\BndcdeOverlaps.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.i("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                // We need to access the database; establish a connection.
                // First, populate the Info fields required to make a connect.
                Info.DbName = "fcsa";
                Info.MicsUserName = "hulme1";
                Info.Password = "XXX";

                // Connect to the DB.
                retVal = Ssutil.UtConnect(Info.DbName, 1);
                if (retVal != Constant.SUCCESS)
                {
                    string msg = String.Format("\n\nBndcdeOverlaps.Go(): ERROR: Ssutil.UtConnect() failed, returned {0}", retVal);
                    Console.Write(msg);
                    Log2.e(msg);
                    Application.ExitQuietly(666);
                }

                //...Log2.v("\n\nBndcdeOverlaps.Go(): Ssutil.UtConnect() succeeded");

                List<BandTally> bandTallyList = new List<BandTally>();

                List<SdBand> sdBandList = GetSdBandList();

                Console.Write("\n\nsdBandList.Count = {0}", sdBandList.Count);

                List<SdBand> sdBandWithSubBandsList = new List<SdBand>();

                // Initialize the BandTally List.
                foreach (SdBand sdBand in sdBandList)
                {
                    BandTally bandTally = new BandTally();
                    bandTally.sdBand = sdBand;

                    bandTally.overlapLeftList = new List<SdBand>();
                    bandTally.containsList = new List<SdBand>();
                    bandTally.overlapRightList = new List<SdBand>();

                    bandTallyList.Add(bandTally);
                }

                foreach (BandTally bandTallyA in bandTallyList)
                {
                    foreach (BandTally bandTallyB in bandTallyList)
                    {
                        // Skip the test if object A is the same as object B.
                        if (bandTallyA.sdBand == bandTallyB.sdBand) continue;

                        bool AcontainsB = (bandTallyB.sdBand.blo >= bandTallyA.sdBand.blo) && (bandTallyB.sdBand.bhi <= bandTallyA.sdBand.bhi);

                        bool AoverlappedonTheLeft = (bandTallyB.sdBand.blo < bandTallyA.sdBand.blo) && (bandTallyB.sdBand.bhi <= bandTallyA.sdBand.bhi) && (bandTallyB.sdBand.bhi > bandTallyA.sdBand.blo);
                        bool AoverlappedonTheRight = (bandTallyB.sdBand.blo > bandTallyA.sdBand.blo) && (bandTallyB.sdBand.bhi > bandTallyA.sdBand.bhi) && (bandTallyB.sdBand.blo < bandTallyA.sdBand.bhi);

                        if (AcontainsB) bandTallyA.containsList.Add(bandTallyB.sdBand);
                        if (AoverlappedonTheLeft) bandTallyA.overlapLeftList.Add(bandTallyB.sdBand);
                        if (AoverlappedonTheRight) bandTallyA.overlapRightList.Add(bandTallyB.sdBand);
                    }
                }


                foreach (BandTally bandTally in bandTallyList)
                {
                    Console.Write("\n{0}\t{1:n0}\t{2:n0}\t{3:n0}", bandTally.sdBand.bndcde, 0.001 * bandTally.sdBand.blo, 0.001 * bandTally.sdBand.bmidf, 0.001 * bandTally.sdBand.bhi);

                    string comma = "";
                    Console.Write("\t");
                    //Console.Write("\n           Overlapped on left:  ");
                    foreach (SdBand sdBand in bandTally.overlapLeftList)
                    {
                        Console.Write("{0}{1}", comma, sdBand.bndcde);
                        comma = ", ";
                    }

                    comma = "";
                    Console.Write("\t");
                    //Console.Write("\n           Contains:            ");
                    foreach (SdBand sdBand in bandTally.containsList)
                    {
                        Console.Write("{0}{1}", comma, sdBand.bndcde);
                        comma = ", ";
                    }

                    comma = "";
                    Console.Write("\t");
                    //Console.Write("\n           Overlapped on right: ");
                    foreach (SdBand sdBand in bandTally.overlapRightList)
                    {
                        Console.Write("{0}{1}", comma, sdBand.bndcde);
                        comma = ", ";
                    }
                }

                // Find gaps.
                bool inBandbefore = true;
                for (int f = 700000; f <= 86000000; f += 1000)
                {
                    bool inBand = false;

                    foreach (BandTally bandTally in bandTallyList)
                    {
                        if (IsInBand(f, bandTally.sdBand))
                        {
                            inBand = true;
                            break;
                        }
                    }

                    if (inBandbefore && !inBand) Console.Write("\nTRIGGER====================");
                    inBandbefore = inBand;

                    if (!inBand) Console.Write("\n{0:n0}", f);
                }

                // Disconnect from the DB.
                Ssutil.UtDisconnect(1);

                Environment.Exit(666);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

        private static bool IsInBand(int f, SdBand sdBand)
        {
            return ((f >= sdBand.blo) && (f <= sdBand.bhi));
        }

        /// <summary>
        /// TBD
        /// </summary>
        public static List<SdBand> GetSdBandList()
        {
            List<SdBand> SdBandList = new List<SdBand>();

            bool morebandsToFetch = true;
            SdBand sctBand;
            SQLLEN sIsNull;

            string cSQL;
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("SELECT bndcde, bandbitpos, blo, bmidf, bhi, badj, mdate, mtime FROM main.sd_band ORDER BY blo, bhi ");

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            while (morebandsToFetch)
            {
                sctBand = new SdBand();

                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    morebandsToFetch = false;
                    break;
                }
                else
                {
                    try
                    {
                        Ssutil.DbGetString(hStmt, 1, "bndcde", out sctBand.bndcde, Constant.BNDCDE_SZ, out sIsNull);
                        Ssutil.DbGetShort(hStmt, 2, "bandbitpos", out sctBand.bandbitpos, out sIsNull);
                        Ssutil.DbGetDouble(hStmt, 3, "blo", out sctBand.blo, out sIsNull);
                        Ssutil.DbGetDouble(hStmt, 4, "bmidf", out sctBand.bmidf, out sIsNull);
                        Ssutil.DbGetDouble(hStmt, 5, "bhi", out sctBand.bhi, out sIsNull);
                        Ssutil.DbGetString(hStmt, 6, "badj", out sctBand.badj, Constant.BADJ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 7, "mdate", out sctBand.mdate, Constant.DATE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 8, "mtime", out sctBand.mtime, Constant.TIME_SZ, out sIsNull);
                    }
                    catch (Exception e)
                    {
                        string msg = String.Format("\n\nTestHiLo.GetSdBandList(): ERROR: DbGet failed: {0}", e.Message);
                        Log2.e(msg);
                        Application.ExitQuietly(667);
                        break;
                    }
                }

                //Trim the band code so we don't have to anywhere else.
                sctBand.bndcde = sctBand.bndcde.Trim();

                SdBandList.Add(sctBand);

            } // end of while loop.

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return SdBandList;
        }


    }
}
