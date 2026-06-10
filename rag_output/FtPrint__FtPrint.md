# Documented File: FtPrint.cs
**Repository Path:** `FtPrint\FtPrint.cs`
**Primary Layer:** `FtPrint`
**Namespace:** `FtPrint`

## Source Code Representation
```csharp
﻿using _Configuration;
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
/// This application fetches a data set from the database associated
/// with a previously imported TS PDF text file and writes it either 
/// to Console.Out or to a prescribed file path.
/// </summary>
/// <description>
/// Suppose that we have previously imported an TS PDF text file using the
/// command-line invocation:
/// <para>
/// <code>
/// FtImport -f fcsa hulme1_0 tsMyPDF "D:\Users\ahulme\tsMyPDF.txt"
/// </code>
/// </para>
/// There will be qty. 6 tables in the database that were created by this import:
/// <list type="bullet">
/// <item>fcsa.hulme.ft_tsMyPDF_ante</item>
/// <item>fcsa.hulme.ft_tsMyPDF_chan</item>
/// <item>fcsa.hulme.ft_tsMyPDF_chng</item>
/// <item>fcsa.hulme.ft_tsMyPDF_shrl</item>
/// <item>fcsa.hulme.ft_tsMyPDF_site</item>
/// <item>fcsa.hulme.ft_tsMyPDF_titl</item>
/// </list>
/// There will also be a record associated with tsMyPDF in the database
/// table <b>web.user_tables</b> that is used by WebMICS to determine whether the
/// tables associated with esMyPDF already exist, or not.
/// <para>
/// The FtPrint application fetches all the data from these qty. 6 database tables. 
/// It arranges that data in the correct sequence (title -> changes -> site -> antenna -> channel).
/// Finally, it formats the data as strings written to Console.Out .
/// </para>
/// 
/// </description>
namespace FtPrint
{
    /// <summary>
    /// This class provides the Main() method for the FtPrint application that is used by WebMICS to 
    /// 'export' a prescribed TS table set in the form of a text file that uses 
    /// identical syntax as that for PDF 'import' files.
    /// </summary>
    /// <remarks>
    /// The command-line invocation of FtPrint.exe is described in the figure below:
    /// \image html "Usage - FtPrint.PNG" "Usage - FtPrint"
    /// </remarks>
    public class FtPrint
    {
        public const string COMMENT_LINE = "*===========================================================================\r\n";
        private const int USER_SESSION = 1;

        private static bool mPrintLong = true;
        private static bool mPrintFull = false;

        private static TextWriter mTW = Console.Out;

        /// <summary>
        /// This is the Main() method for the FtPrint application that provides
        /// top-level control of the processing required to parse the command line
        /// arguments, fetch records from the DB, format the information as
        /// text and writing the results out to Console.Out .
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        static void Main(string[] args)
        {
            int rc;
            int nRet;

            try
            {
                // Enable or disable developmental run-time logging.
#if true
                string mLog2FilePath = @"d:\MicsBatchLogs\FtPrint.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.BuildMetaData);
                }
                else
                {
                    Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
                }
#endif
                // Disable the 'spoofing' mode.
                // The spoofing mode replaces the MDB schema 'main' with the schema of the
                // user that executed this program (e.g. 'hulme'). Consequently, hulme.mt_chan
                // acts as a surrogate for main.mt_chan etc.
                // Spoofing mode can be turned on for test purposes using a command line flag.
                // The default value of this parameter is 'true' so the following line is just
                // for emphasis of its initial state.
                Info.SpoofModeIsOff = true;

                // Parse the command line arguments and set values in Info static class.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariables();

                //...Log2.v(Info.ToString());

                if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)) != 0)
                {
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, mTW);
                    Application.ExitQuietly(100);
                }

                // Start a MICS user session (connects to the database).
                rc = Ssutil.UtConnect(Info.DbName, USER_SESSION);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    ErrMsg.UtPrintMessage(Error.NODATABASE, Info.DbName);
                    Application.ExitQuietly(rc);
                }

                BiUtil.BiBillingRec("FTPRINT", Info.PdfName);

                // Set the operating state of the classes that provide high-level read/write
                // methods that access TS MDB tables.
                // We must do this after the call to Ssutil.UtConnect() because only then is
                // the user's schema (Info.GlobalSchema) set to its correct value.
                DynMdbAntenna.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbChannel.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynMdbSite.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbTown.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);
                DynSdbRout.SetState(Info.MdbWriteEnabled, Info.SpoofModeIsOff, Info.GlobalSchema);

                // Write the initial output header.
                string curDate, curTime;

                GenUtil.UtGetDateTime(out curDate, out curTime);

                mTW.Write("* TS-PDF: {0}, By FtPrint ({1}) at {2}\r\n*\r\n",
                    Info.PdfName, Info.BuildMetaData, Info.GetDateTimeNow());

                if (!Ssutil.UtTableExist(Constant.FT, Info.PdfName))
                {
                    Log2.e("\nFtPrint.Main(): Ssutil.UtTableExist() returned false for: " + Info.PdfName);
                    ErrMsg.UtPrintMessage(Error.FILENOTEXIST, "EXPORT/PRINT");
                }
                else
                {
                    // Commence the FtPrint operation on the prescribed table destination name.

                    FtTitl pTitle;
                    FtChng[] pChanges;
                    int nRet__;
                    string cLine;
                    FtSiteStr ptSite;
                    FtSiteStrNulls ptNulls;
                    int nInd;
                    bool AreRecords = false;

                    //	Put in the title, whether there is one there or not.
                    if ((nRet__ = FtUtils.FtGetTitle(out pTitle, Info.PdfName)) == 0)
                    {
                        FtPrintFw.PrintTitl(pTitle, out cLine, mPrintFull);
                        AreRecords = true;
                    }
                    else
                    {
                        cLine = String.Format("TT,N,{0},,,,", Info.PdfName);
                    }
                    mTW.Write("{0}\r\n", cLine);

                    //	Insert any change of call signs
                    nRet__ = FtUtils.FtGetChgCalls(out pChanges, Info.PdfName);    //	Get the change of call signs
                    if (nRet__ > 0)
                    {
                        for (nInd = 0; nInd < nRet__; nInd++)
                        {
                            FtPrintFw.PrintChnge(pChanges[nInd], out cLine);
                            mTW.Write("{0}\r\n", cLine);
                        }
                        AreRecords = true;
                    }

                    //	Now go through the sites in the file.
                    //cNextCall = "";

                    foreach (string call1 in FtUtils.FtGetCallSigns(Info.PdfName))
                    {
                        AreRecords = true;
                        //...Log2.v("\nFtPrint.Main(): FtUtils.FtNextCall() returned cNextCall = " + cNextCall);

                        // This method call fetches all of the site, ante and chan data from the DB
                        // for the prescribed call1.
                        nRet__ = FtUtils.FtGetSiteWN(call1, out ptSite, 3, Info.PdfName, out ptNulls);

                        if (nRet__ == 0)
                        {
                            // Now writes to Console.Out all of the site, ante and chan data from the DB
                            // for the prescribed call1.
                            FtPrintFw.FtPrintSite(ptSite, ptNulls, mPrintLong, mPrintFull, out cLine);
                            mTW.Write("{0}", cLine);
                        }
                        else
                        {
                            mTW.Write("**** ERROR reading {0}. *****\r\n{1}\r\n", call1, GenUtil.GetUserMess());
                            break;
                        }

                    }
                    if (!AreRecords)
                    {
                        mTW.Write("There is no data in file {0}\r\n", Info.PdfName);
                    }

                }

                // Prepare to exit normally.

               // if (mTW != Console.Out) mTW.Close();  // Just being extra cautious.

                BiUtil.BiBillingRec(Constant.BI_END, "");

                Ssutil.UtDisconnect(USER_SESSION);

                Qutils.ExitQueue(Info.DbName, "READ");

                Application.ExitQuietly(rc);

            }
            catch (Exception e)
            {
                Qutils.ExitQueue(Info.DbName, "READ");

                Log2.e("\n\nFtPrint.Main(): exception caught: " + e.Message);
                Log2.e("\n\nFtPrint.Main(): stack trace: \n\n" + e.StackTrace);

                Application.Exit(Error.FATAL_EXCEPTION);
            }
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute FtImport; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
            }

            // Get the user's password from the environment.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
            if (String.IsNullOrWhiteSpace(Info.Password))
            {
                // Password just has to be set to something; its value is never used.
                Info.Password = "Bananarama";
            }
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when FtImport
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program exports the record data found in a prescribed user 'ft_' table set  +");
            Console.Write("\r\n as text using the same format as an TS import PDF.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: FtPrint <dbname> <projectCode> [-o<outFilePath>] <printFlag> <tableName>");
            Console.Write("\r\n");
            Console.Write("\r\n        dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode     : project code, e.g. 'hulme1_34'");
            Console.Write("\r\n        outFilePath     : full path to prescribed output file.");
            Console.Write("\r\n        printFlag       : 'S' for short output (optional fields are not printed);");
            Console.Write("\r\n                          'F' for  full output (syntax comments are inserted between lines);");
            Console.Write("\r\n                          any other character: optional fields are printed, no syntax comments.");
            Console.Write("\r\n        tableName       : root name of the user's ft_ DB table set to be printed.");
            Console.Write("\r\n");
            Console.Write("\r\n <...> indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...] indicates an optional argument.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the command line arguments and uses their values to set
        /// program-specific internal variables and some of the fields of the static 
        /// class Info.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        private static void ParseCommandLineArgs(string[] args)
        {
            string printFlag;

            List<string> mandatoryArgs = new List<string>();
            List<string> optionalArgs = new List<string>();

            // Check for too few or too many command-line paramters.
            if ((args.Length < 4) || (args.Length > 5))
            {
                Console.Write("\r\nToo few or too many arguments: should be 4 or 5.");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Separate the mandatory and optional arguments into two lists.
            for (int argno = 0; argno < args.Length; argno++)
            {
                string arg = args[argno];

                if (arg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    optionalArgs.Add(arg);
                }
                else
                {
                    mandatoryArgs.Add(arg);
                }
            }

            // Parse the mandatory arguments.
            if (mandatoryArgs.Count != 4)
            {
                string str = "\r\n\r\nInvalid command line - incorrect number of mandatory arguments.";
                Console.WriteLine(str);
                WriteUsageToConsole();
                Log2.e(str);
                Application.ExitQuietly(Constant.FAILURE);
            }
            else
            {
                // Harvest the mandatory arguments.
                Info.DbName = mandatoryArgs[0];
                Info.ProjectCode = mandatoryArgs[1];
                printFlag = mandatoryArgs[2].ToUpper();
                Info.PdfName = mandatoryArgs[3];

                // As of 20190108 the WebMICS 'export' operation on a DB
                // TS table set invokes the command-line call:
                //
                // D:\prod\bin\ftPrint fcsa hulme1_0 -oD:\Inetpub\micstest\mics\userdirs\hulme\hulme1\tafloutv2.txt L tafloutv2
                //
                // where 'tafloutv2' is the root name of the TS table set to be printed.
                // However, the legacy C/C++ code does not recognize the command-line
                // argument 'L' and it is ignored. The legacy C/C++ code *does* recognize
                // 'S' or 'F' meaning produce a 'short' or a 'full' text export file.
                if (printFlag.Equals("S"))
                {
                    mPrintLong = false;
                }
                else if (printFlag.Equals("F"))
                {
                    mPrintFull = true;
                }


            }

            // Now parse the optional arguments.
            foreach (string cla in optionalArgs)
            {
                string arg = cla.ToUpper();

                // Redirect Console.Out to file option?
                if (arg.StartsWith("-O", StringComparison.OrdinalIgnoreCase))
                {
                    string outFilePath = arg.Substring(2);

                    Info.OutFilePath = outFilePath;

                    try
                    {
                        // Instantiate the 'redirection' TextWriter object.
                        mTW = File.CreateText(outFilePath);
                    }
                    catch
                    {
                        string str = "\r\nFtPrint: could not open file: " + arg;
                        Log2.e(str);
                        Console.Write(str);
                        Application.ExitQuietly(99);
                    }

                }
                else if (arg.StartsWith("-S", StringComparison.OrdinalIgnoreCase))
                {
                    // Spoofing mode has been prescribed.
                    Info.SpoofModeIsOff = false;
                }
                else
                {
                    string str = "\r\n\r\nFtPrint: invalid option: " + arg;
                    Log2.e(str);
                    Console.Write(str);
                    WriteUsageToConsole();
                    Application.ExitQuietly(120);
                }
            }
        }



    }
}

```
