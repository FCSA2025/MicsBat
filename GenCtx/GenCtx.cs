using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program manages the calculation of
/// a set of (x, y) data points for a CTX curve and their storage as 
/// records in the user's <b>returnvalues</b> table.
/// </summary>
namespace GenCtx
{

    /// <summary>
    /// This class has a Main() method that manages the calculation of
    /// a set of (x, y) data points for a CTX curve and their storage as 
    /// records in the user's <b>returnvalues</b> table.
    /// </summary>
    public class GenCtx
    {
        /// <summary>
        /// This Main() method manages the calculation of
        /// a set of (x, y) data points for a CTX curve and their storage as 
        /// records in the user's <b>returnvalues</b> table.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
#if false
                // Turn on developmental logging.
                string mLog2FilePath = @"d:\MicsBatchLogs\GenCtx.log";
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
                int nRet = -666;    /* return code from access check */
                int rc;
                string commandLineArgs = null;
                const int PREFIX = 7;

                string cRxEquip;
                string cRxTraf;
                string cTxEquip;
                string cTxTraf;
                string cKey;  /*	Unique key for this run. */

                TcTxAnalog tVicAnalog;
                TcTxDigital tVicDigital;
                TcTxAnalog tIntAnalog;
                TcTxDigital tIntDigital;
                char cVicType;
                char cIntType;
                int offset = 0;

                /*	These are the frequency separations for a digital victim */
                double[] aDFS = new double[]
                             {
                            0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8,
                            0.9,   1, 1.2, 1.4, 1.6, 1.8,   2, 2.2, 2.4, 2.6,
                            2.8,   3, 3.2, 3.4, 3.6, 3.8,   4, 4.2, 4.4, 4.6,
                            4.8,   5, 5.2, 5.4, 5.6, 5.8,   6, 6.2, 6.4, 6.6,
                            6.8,   7, 7.2, 7.4, 7.6, 7.8,   8, 8.2, 8.4, 8.6,
                            8.8,   9, 9.2, 9.4, 9.6, 9.8,  10,  11,  12,  13,
                            14,  15,  16,  17,  18,  19,  20,  21,  22,  23,    24,
                            25,  26,  27,  28,  29,  30,  31,  32,  33,  34, 35.01,
                            36,  37,  38,  39,  40,  41,  42,  43,  44,  45,    46,
                            47,  48,  49,  50,  51,  52,  53,  54,  55,  56,
                            58,  60,  62,  64,  66,  68,  70,  72,  74,  76,
                            78,  80,  82,  84,  86,  88, 100, 110, 120, 130,
                            140, 150, 160, 170, 180, 190, 200, 210, 220, 230,
                            240, 250, 260, 270, 280, 290, 300, 320, 340, 360, 
					        // AH: Change request #180118A.
					        //380, 400, 2000, -1}; // original line.
					        380, 400, 2000, 3000, 4000, 5000
                        };

                double[] aAFS = new double[]
                             {
                            0.0,
                            0.10, 0.12, 0.14, 0.16, 0.18,
                            0.20, 0.22, 0.24, 0.26, 0.28,
                            0.30, 0.32, 0.34, 0.36, 0.38,
                            0.40, 0.42, 0.44, 0.46, 0.48,
                            0.50, 0.52, 0.54, 0.56, 0.58,
                            0.60, 0.62, 0.64, 0.66, 0.68,
                            0.70, 0.72, 0.74, 0.76, 0.78,
                            0.80, 0.82, 0.84, 0.86, 0.88,
                            0.90, 0.92, 0.94, 0.96, 0.98,
                            1.0, 1.2, 1.4, 1.6, 1.8,
                            2.0, 2.2, 2.4, 2.6, 2.8,
                            3.0, 3.2, 3.4, 3.6, 3.8,
                            4.0, 4.2, 4.4, 4.6, 4.8,
                            5.0, 5.2, 5.4, 5.6, 5.8,
                            6.0, 6.2, 6.4, 6.6, 6.8,
                            7.0, 7.2, 7.4, 7.6, 7.8,
                            8.0, 8.2, 8.4, 8.6, 8.8,
                            9.0, 9.2, 9.4, 9.6, 9.8,
                            10.0, 12.0, 14.0, 16.0, 18.0,
                            20.0, 22.0, 24.0, 26.0, 28.0,
                            30.0, 32.0, 34.0, 36.0, 38.0,
                            40.0, 42.0, 44.0, 46.0, 48.0,
                            50.0, 52.0, 54.0, 56.0, 58.0,
                            60.0, 62.0, 64.0, 66.0, 68.0,
                            70.0, 72.0, 74.0, 76.0, 78.0,
                            80.0, 82.0, 84.0, 86.0, 88.0,
                            90.0, 92.0, 94.0, 96.0, 98.0,
                            100.0, 120.0, 140.0, 160.0, 180.0,
                            200.0, 220.0, 240.0, 260.0, 280.0,
                            300.0, 320.0, 340.0, 360.0, 380.0,
					        // AH: Change request #180118A.
					        //400.0, 2000, -1.0,  // original line.
					        400.0, 2000, 3000, 4000, 5000
                         };

                double[] adFsep;
                double dReq;
                int nSize;
                int nInd;
                int nFirstExtra = -1;

                bool IsEs;
                char cTypeOfInt;      // This is 'C' for C/I and 'I' for -I.
                string[] aRet;
                int nNumArgs;
                int nExtras;
                string fileName = "";

                // Instantiate a TextWriter object that sends written text into oblivion.
                // This avoids always having to check whether fPrint is null prior to 
                // writing to it.
                // This TextWriter object will be reinstantiated to something more useful
                // if the user included the -p option in the command line.
                TextWriter fPrint = new StreamWriter(Stream.Null);

                commandLineArgs = Strings.CommandLineFromArgs(args);
                //...Log2.v("\nGenCtx.Main(): commandLineArgs = " + commandLineArgs);

                //	Check for the optional arguments.  They must appear before all the others.
                //	First set the default values as if they were absent.
                IsEs = false;
                nNumArgs = 6;

                for (nInd = 0; nInd < args.Length && args[nInd][0] == '-'; nInd++)
                {
                    string arg = args[nInd];

                    if (arg.ToUpper()[1] == 'E')
                    {
                        IsEs = true;
                    }
                    else if (arg.ToUpper()[1] == 'P')
                    {
                        fileName = arg.Substring(2);
                        try
                        {
                            //	The file name is adjacent to the -p. i.e. -poutfile.prn, or stdout if absent.
                            int nLen = arg.Length;
                            if (nLen > 2)
                            {
                                fPrint = new StreamWriter(fileName);
                            }
                            else
                            {
                                fPrint = Console.Out;
                            }
                        }
                        catch (Exception e)
                        {
                            Log2.e("\nGenCtx.Main(): ERROR: could not open StreamWriter: exception: " + e.Message);
                            Console.Write("\nCould not open file: {0}\n", fileName);
                            Application.Exit(9);
                        }
                    }
                    else
                    {
                        Log2.w("\nGenCtx.Main(): ERROR: invalid command line option: " + arg);
                        Console.Write("\nUnidentified option: {0}", arg);
                    }

                    nNumArgs++;
                }

                //...Log2.v("\nGenCtx.Main(): A");

                if (args.Length < nNumArgs)
                {
                    Log2.e("\nGenCtx.Main(): ERROR: invalid command line: " + commandLineArgs);
                    Console.Write("\nUsage: genctx [-e] [-p<outfile>] <dbname> <RX Equip> <RX Traffic> <TX Equip> <TX Traffic> <key> <further frequency separations>");
                    Console.Write("\n       Where -e is present for an ES calculation.\n");
                    Application.Exit(10);
                }

                // Harvest the key parameter values for this run of the program.
                Info.DbName = args[nInd++];

                cRxEquip = args[nInd++].ToUpper();
                cRxTraf = args[nInd++].ToUpper();
                cTxEquip = args[nInd++].ToUpper();
                cTxTraf = args[nInd++].ToUpper();

                cKey = args[nInd++];

                nExtras = args.Length - nNumArgs;  // Count of the extra frequencies.
                nFirstExtra = nInd;

                //...Log2.v("\nGenCtx.Main(): B-1");

                // For developmental testing.
                //...Log2.v("\n");
                //...Log2.v("\nTsipQdelete.Main(): args.Length = " + args.Length);
                //...Log2.v("\nTsipQdelete.Main(): IsES        = " + IsEs);
                //...Log2.v("\nTsipQdelete.Main(): fileName    = " + fileName);
                //...Log2.v("\nTsipQdelete.Main(): Info.DbName = " + Info.DbName);
                //...Log2.v("\nTsipQdelete.Main(): cRxEquip    = " + cRxEquip);
                //...Log2.v("\nTsipQdelete.Main(): cRxTraf     = " + cRxTraf);
                //...Log2.v("\nTsipQdelete.Main(): cTxEquip    = " + cTxEquip);
                //...Log2.v("\nTsipQdelete.Main(): cTxTraf     = " + cTxTraf);
                //...Log2.v("\nTsipQdelete.Main(): cKey        = " + cKey);
                //...Log2.v("\nTsipQdelete.Main(): nFirstExtra = " + nFirstExtra);
                //...Log2.v("\nTsipQdelete.Main(): nExtras     = " + nExtras);
                //...Log2.v("\nTsipQdelete.Main(): aDFS.Length = " + aDFS.Length);
                //...Log2.v("\nTsipQdelete.Main(): aAFS.Length = " + aAFS.Length);
                //...Log2.v("\n");

                // Write a preamble message to the fPrint TextWriter object.
                //...Log2.v("\nGenCtx.Main(): B");

                DateTime dateTime = DateTime.Now;

                fPrint.Write("\ngenctx build {0} at {1}\n\n{2}/{3} into {4}/{5}\n",
                          Info.BuildMetaData, Info.ToFormatA(dateTime),
                                cTxEquip, cTxTraf, cRxEquip, cRxTraf);
                fPrint.Flush();

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the MICS Id and password.

                // Get the user's MICS ID from the environment.
                Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
                if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                {
                    Log2.e("\nTsipQdelete.Main(): ERROR: Windows environment variable MicsUser is not set.");
                    Application.Exit(99);
                }

                // Get the user's password from the environment.
                Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
                if (String.IsNullOrWhiteSpace(Info.Password))
                {
                    // Password just has to be set to something; its value is never used.
                    Info.Password = "Bananarama";
                }

                // Attempt to start a MICS user session. This instantiates an ODBC 
                // connection handle that is used throughout the session.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != Constant.SUCCESS)
                {
                    /* Can't connect to database */
                    Log2.e("\nGenCtx.Main(): ERROR: call to Ssutil.UtConnect() failed.");
                    Console.Write("genctx -- Could not connect to database: {0}\n", rc);
                    Application.Exit(11);
                }

                // The project code is not given in the command line; get it from the
                // user's MICS_PROJECT Windows environment variable.
                string dummy;
                Info.ProjectCode = GenUtil.GetProjectCode(out dummy);

                // Initialize the billing.
                BiUtil.BiBillingRec("GENCTX", cKey);

                //...Log2.v("\nGenCtx.Main(): A");

                // Get the equipment cross references.
                nRet = GetCtx.GetCtxEqpt(cRxTraf, cRxEquip, cTxTraf, cTxEquip,
                                            out cVicType, out cIntType,
                                            out tVicAnalog, out tVicDigital,
                                            out tIntAnalog, out tIntDigital);
                if (nRet != Constant.SUCCESS)
                {
                    //...Log2.v("\nGenCtx.Main(): C");

                    Console.Write("\n*ERROR* - Could not retrieve the equipment crossreferences: {0}\n", nRet);
                    Ssutil.UtDisconnect(1);
                    Application.Exit(127);
                }

                //...Log2.v("\nGenCtx.Main(): D");

                fPrint.Write("Resolving the cross reference:-\n{0} into {1}\n", cIntType, cVicType);
                if (cIntType == 'A')
                {
                    fPrint.Write("Analog {0}/{1} into ", tIntAnalog.ecode, tIntAnalog.trafcode);
                }
                else
                {
                    fPrint.Write("Digital {0}/{1} into ", tIntDigital.ecode, tIntDigital.trafcode);
                }
                if (cVicType == 'A')
                {
                    fPrint.Write("Analog {0}/{1}.\n", tVicAnalog.ecode, tVicAnalog.trafcode);
                }
                else
                {
                    fPrint.Write("Digital {0}/{1}.\n", tVicDigital.ecode, tVicDigital.trafcode);
                }

                // Now get the frequency separations that will be used in producing the
                // requirements table.
                if (cVicType == 'A')
                {
                    //...Log2.v("\nGenCtx.Main(): E");
                    adFsep = aAFS;
                    nSize = aAFS.Length + nExtras;
                }
                else
                {
                    //...Log2.v("\nGenCtx.Main(): F");
                    adFsep = aDFS;
                    nSize = aDFS.Length + nExtras;
                }

                // Add in the cross-reference information that was used.
                // PREFIX, viceqpt, victraf, inteqpt, inttraf, vicbwchan, intbwchan
                nSize += PREFIX;

                aRet = new String[nSize];

                //...Log2.v("\nGenCtx.Main(): F1: aRet.Length = " + aRet.Length);

                aRet[0] = String.Format("{0}", PREFIX);      /*	Size of the prefix */

                if (cVicType == 'A')
                {
                    //...Log2.v("\nGenCtx.Main(): G");
                    aRet[1] = tVicAnalog.ecode;
                    aRet[2] = tVicAnalog.trafcode;
                    aRet[5] = String.Format("CHANNELS: {0}", tVicAnalog.numchan);
                }
                else
                {
                    //...Log2.v("\nGenCtx.Main(): H");
                    aRet[1] = tVicDigital.ecode;
                    aRet[2] = tVicDigital.trafcode;
                    aRet[5] = String.Format("BANDWIDTH:{0:F2}MHz", tVicDigital.bandwidth);
                }

                if (cIntType == 'A')
                {
                    //...Log2.v("\nGenCtx.Main(): I");
                    aRet[3] = tIntAnalog.ecode;
                    aRet[4] = tIntAnalog.trafcode;
                    aRet[6] = String.Format("CHANNELS: {0}", tIntAnalog.numchan);
                }
                else
                {
                    //...Log2.v("\nGenCtx.Main(): J");
                    aRet[3] = tIntDigital.ecode;
                    aRet[4] = tIntDigital.trafcode;
                    aRet[6] = String.Format("BANDWIDTH:{0:F2}MHz", tIntDigital.bandwidth);
                }

                // Now go through the frequency separation array filling in the requirements
                // for each element.
                offset = PREFIX - 1;

                for (nInd = 0; nInd < adFsep.Length; nInd++)
                {
                    offset++;  // enumerates the elements of aRet[].

                    nRet = GetCtx.GetCtxReq(cIntType, cVicType,
                                            tVicAnalog, tVicDigital,
                                            tIntAnalog, tIntDigital,
                                            adFsep[nInd], IsEs,
                                            out cTypeOfInt, out dReq);
                    if (nRet != 0)
                    {
                        //...Log2.v("\nGenCtx.Main(): K");
                        break;
                    }
                    else
                    {
                        /*	Store both the frequency separation and the requirement in the
                        *		returned value array */
                        aRet[offset] = String.Format("{0,8:F2},{1,8:F3}", adFsep[nInd], dReq);
                        //...Log2.v(String.Format("\nGenCtx.Main(): K1: aRet[{0}] = {1}", offset, aRet[offset]));
                    }
                }

                //...Log2.v("\nGenCtx.Main(): L0");

                // After processing the list of frequencies we must process any additional
                // frequencies provided in the command line argument list.
                for (nInd = 0; nInd < nExtras; nInd++)
                {
                    //...Log2.v("\nGenCtx.Main(): L");

                    int nArg = nInd + nFirstExtra;

                    offset++;  // Carried on from before

                    double dFS = Convert.ToDouble(args[nArg]);

                    nRet = GetCtx.GetCtxReq(cIntType, cVicType,
                                            tVicAnalog, tVicDigital,
                                            tIntAnalog, tIntDigital,
                                            dFS, IsEs,
                                            out cTypeOfInt, out dReq);
                    if (nRet != Constant.SUCCESS)
                    {
                        Log2.e("\nGenCtx.Main(): ERROR: problem retrieving requirements for FS: {0,6:F3}, returned: {1}\n", adFsep[nInd], nRet);
                        Console.Write("\n*ERROR* Problem retrieving requirements for FS: {0,6:F3}, returned: {1}\n", adFsep[nInd], nRet);
                        break;
                    }
                    else
                    {
                        /*	Store both the frequency separation and the requirement in the
                        *		returned value array */
                        aRet[offset] = String.Format("{0,8:F2},{1,8:F3}", dFS, dReq);
                        //...Log2.v(String.Format("\nGenCtx.Main(): M1: aRet[{0}] = {1}", offset, aRet[offset]));
                    }
                }

                if (nRet == Constant.SUCCESS)
                {
                    //...Log2.v("\nGenCtx.Main(): N");

                    // We now have the output array of frequency separations and returned
                    // values in aRet.  Store this in the database for the web server to access.
                    // First, though, we sort everything after the PREFIX by frequency separation.
                    string[] aSort = new string[aRet.Length - PREFIX];

                    for (int i = 0; i < aSort.Length; i++)
                    {
                        aSort[i] = aRet[i + PREFIX];
                    }

                    Array.Sort(aSort, StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < aSort.Length; i++)
                    {
                        aRet[i + PREFIX] = aSort[i];
                    }

                    // Insert the record into the returnvalues table.
                    nRet = Retval.RetVal(cKey, ref aRet);
                    if (nRet != Constant.SUCCESS)
                    {
                        Log2.e("\nGenCtx.Main(): ERROR: call to RetVal() failed: " + nRet);
                        Application.Exit(666);
                    }

                    // Write out all of the aRet[] element values.
                    for (int i = 0; i < aRet.Length; i++)
                    {
                        //...Log2.v(String.Format("\nGenCtx.Main(): M: aRet[{0}] = {1}", i, aRet[i]));

                        fPrint.Write("{0}\n", aRet[i]);
                        fPrint.Flush();
                    }
                }

                // Finalize the billing.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Terminate the MICS user's session.
                Ssutil.UtDisconnect(1);

                //...Log2.v("\nGenCtx.Main(): end of Main().");

                Environment.Exit((nRet < 0) ? (127 + nRet) : nRet);
            }
            catch (Exception e)
            {
                Log2.e("\n\nGenCtx.Main(): exception caught: " + e.Message);
                Log2.e("\n\nGenCtx.Main(): stack trace: \n\n" + e.StackTrace);

                Application.Exit(Error.FATAL_EXCEPTION);
            }


        } // Main()











    }
}
