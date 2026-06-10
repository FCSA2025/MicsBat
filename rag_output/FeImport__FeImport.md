# Documented File: FeImport.cs
**Repository Path:** `FeImport\FeImport.cs`
**Primary Layer:** `FeImport`
**Namespace:** `FeImport`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums.FeImpQual;
using _NewLib;
using _Configuration;
using System.IO;

/// <summary>
/// This application imports the contents of an ES data file, performs syntactic
/// validation and, if no errors are found, inserts new tables into the database.
/// </summary>
/// <description>
/// As an example, for the MICS user name 'hulme' and destination name
/// 'destname' the following qty. 8 distinct tables can be inserted
/// into the database by a successful FeImport operation:
/// <list type="bullet">
/// <item>hulme.fe_destname_ante</item>
/// <item>hulme.fe_destname_azim</item>
/// <item>hulme.fe_destname_ccal</item>
/// <item>hulme.fe_destname_chan</item>
/// <item>hulme.fe_destname_cloc</item>
/// <item>hulme.fe_destname_shrl</item>
/// <item>hulme.fe_destname_site</item>
/// <item>hulme.fe_destname_titl</item>
/// </list>
/// </description>
namespace FeImport
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
    using _Utillib;

    /// <summary>
    /// This class contains the Main() method that performs the
    /// top-level processing and control of the
    /// FeImport functionality together with its supporting methods.
    /// </summary>
    public class FeImport
    {
        private static string mProgramName = "FeImport";

        private static string mTab1Name;
        private static string mTab2Name;
        private static string mTab3Name;
        private static string mTab4Name;
        private static string mTab5Name;
        private static string mTab6Name;
        private static string mTab7Name;

        private static int mTitlHandle;
        private static int mSiteHandle;
        private static int mAnteHandle;
        private static int mChanHandle;
        private static int mCCalHandle;
        private static int mCLocHandle;
        private static int mAzimHandle;

        private static bool mIsVerbose = false;

        /// <summary>
        /// This is the Main() method that performs the
        /// top-level processing and control of the
        /// FeImport functionality.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        static void Main(string[] args)
        {
            string destName = "";

            try
            {
#if false
            string mLog2FilePath = @"D:\MicsBatchLogs\FeImport.log";
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
                // Display build info.
                Console.Write("\r\n{0}.exe:  Build: {1}\r\n\r\n", mProgramName, Info.BuildMetaData);

                FeTitl titlStruct = new FeTitl();
                FeSite siteStruct = new FeSite(); ;
                FeAnte anteStruct = new FeAnte();
                FeChan chanStruct = new FeChan();
                FeCLoc cLocStruct = new FeCLoc();
                FeCCal cCalStruct = new FeCCal();
                FeAzim azimStruct = new FeAzim();

                //string inputBuffer;
                string[] lines = null;
                short importOK;
                FileInfo fileInfo = null;

                SQLLEN[] titlNulls = NullHelper.CreateArrayOfNullInd(FeTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
                SQLLEN[] siteNulls = NullHelper.CreateArrayOfNullInd(FeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
                SQLLEN[] anteNulls = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
                SQLLEN[] chanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
                SQLLEN[] cCalNulls = NullHelper.CreateArrayOfNullInd(FeCCal.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
                SQLLEN[] cLocNulls = NullHelper.CreateArrayOfNullInd(FeCLoc.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
                SQLLEN[] azimNulls = NullHelper.CreateArrayOfNullInd(FeAzim.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                string msgBuf;
                string dbName;
                string projectCode;
                string importFilePath;

                int nRet;
                bool dropTablesRequested = false;
                bool dropTablesThenExit = false;

                // Parse the command line arguments.
                ParseCommandLineArgs(args,
                                        out dropTablesThenExit,
                                        out dropTablesRequested,
                                        out dbName,
                                        out projectCode,
                                        out destName,
                                        out importFilePath
                                    );

                // Accumulate Info.
                Info.DbName = dbName;
                Info.ProjectCode = projectCode;
                Info.DestName = destName;

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariables();

                // Start a MICS user session.
                nRet = Ssutil.UtConnect(Info.DbName, 1);
                if (nRet != 0)
                {
                    // Can't connect to database.
                    Log2.e("\nFeImport.Main(): ERROR: call to Ssutil.UtConnect() failed, nRet = {0}", nRet);
                    importOK = Error.NODATABASE;
                    ErrMsg.UtPrintMessage(importOK, Info.DbName);
                    WriteSuccessOrFailMsg(importOK, mProgramName);
                    Application.ExitQuietly(importOK);

                }

                // This is a new feature, not present in the legacy C++ code.
                // If '-D' option is prescribed on the command line drop all 
                // existing tables associated with the destination name. 
                // Note: WebMICS never uses the -D option.
                if (dropTablesRequested)
                {
                    DropAllAssociatedTables(destName);
                }

                // Exit if user only wants to drop tables then exit without importing a PDF.
                if (dropTablesThenExit)
                {
                    // Check that all ES tables for PDF destName were successfully dropped.
                    if (Ssutil.UtTableExist(Constant.FE, destName))
                    {
                        Console.Write("\r\nAttempt to drop all ES tables for PDF '{0}' FAILED.", destName);
                    }
                    else
                    {
                        Console.Write("\r\nAll ES tables for PDF '{0}' were successfull dropped.", destName);

                    }

                    Ssutil.UtDisconnect(1);
                    Application.ExitQuietly(0);
                }

                // Now proceed as if the '-D' option was not called so as to mimic
                // the flow of the legacy C++ code.

                // Check if any of the Fe tables associated with destName already exists; 
                // if so, the policy is that we must exit.
                bool someTablesAlreadyExist = Ssutil.UtTableExist(Constant.FE, destName);

                if (someTablesAlreadyExist)
                {
                    Ssutil.UtDisconnect(1);
                    importOK = Error.TABLEEXIST;
                    ErrMsg.UtPrintMessage(importOK, destName);
                    WriteSuccessOrFailMsg(importOK, mProgramName);
                    Application.ExitQuietly(importOK);
                }
                else
                {
                    //...Log2.v("\nFeImport.Main(): no tables currently exist for: " + destName);
                }

                // We have to create the table.
                nRet = Ssutil.UtCreateTable(Constant.FE, destName);
                if (nRet != Constant.SUCCESS)
                {
                    //...Log2.e("\nFeImport.Main(): ERROR: call to Ssutil.UtCreateTable() failed.");
                    Ssutil.UtDisconnect(1);
                    importOK = Error.NOCREATETABLE;
                    ErrMsg.UtPrintMessage(importOK, destName);
                    WriteSuccessOrFailMsg(importOK, mProgramName);
                    Application.ExitQuietly(importOK);
                }

                // Initialize the billing record.
                BiUtil.BiBillingRec("FEIMPORT", destName);


                // Initialize parse sequence-tracking variables.
                importOK = Constant.SUCCESS;

                // Get the fully-qualified SQL database table names associated
                // with the destName.
                CreateAssocTableNames(Info.DestName);

                // Select the DB tables into which the imported data will be inserted.
                CreateCursorsForDbTableInserts(ref importOK);

                // If everything is OK so far we can begin to read lines of text
                // from the import file and parse them for syntax.
                if (importOK == Constant.SUCCESS)
                {
                    // Get all lines of text from the import files as a string[].
                    try
                    {
                        fileInfo = new FileInfo(importFilePath);

                        lines = File.ReadAllLines(importFilePath);
                    }
                    catch
                    {
                        //...Log2.e("\nFeImport.Main(): ERROR: attempt to read from import file failed: " + e.Message);
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        importOK = Error.IMPNOTEXIST;
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.Exit(importOK);
                    }

                    if (fileInfo.Length == 0)
                    {
                        //...Log2.v("\nFeImport.Main(): import file is empty: " + importFilePath);
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        importOK = Error.FILEISEMPTY;
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.Exit(importOK);
                    }

                    //...Log2.v("\n\nFeImport.Main(): successfully read from file: " + importFilePath + "\n");

                    // Perform an initial validation of the import file's contents.
                    // A 'qualified' line is one that begins with a known token, i.e.
                    // TE, TD, LK, GK, SK, SD, AK, AT, AR, AS, ZK, CK, CT or CR
                    // and has the correct number of CSV token/fields for its type.
                    // Parse through all the lines read from the import file and produce
                    // an array of qualified lines. This will discard any comment or 
                    // blank lines. In addition, qualified lines will be checked for
                    // correct sequence and for 'missing' lines in Site, Antenna and 
                    // Channel records. No validation of a line's CSV field-content 
                    // is performed yet - that will be done in the next stage of parsing.
                    QualLine[] qualLines;

                    importOK = FeValidation.ParseForQualLines(lines, out qualLines);

                    if (importOK == Error.FILEISEMPTY)
                    {
                        //...Log2.v("\nFeImport.Main(): A: call to ParseQualifiedLines() lines failed.");
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.ExitQuietly(importOK);


                    }
                    else if (importOK == Error.IMPORTFILEHASNOPARSABLECONTENT)
                    {
                        //...Log2.v("\nFeImport.Main(): B: call to ParseQualifiedLines() lines failed.");
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.ExitQuietly(importOK);
                    }
                    else if (importOK != Constant.SUCCESS)
                    {
                        //...Log2.v("\nFeImport.Main(): C: call to ParseQualifiedLines() lines failed.");
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.ExitQuietly(importOK);
                    }

                    importOK = FeValidation.CheckMandatedFields(qualLines);
                    if (importOK != Constant.SUCCESS)
                    {
                        //...Log2.v("\nFeImport.Main(): call to CheckMandatedFields() lines failed.");
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.ExitQuietly(importOK);
                    }

                    importOK = FeValidation.CheckMandatedIfAddFields(qualLines);
                    if (importOK != Constant.SUCCESS)
                    {
                        //...Log2.v("\nFeImport.Main(): call to CheckMandatedIfAddFields() lines failed.");
                        //...Log2.v("\nFeImport.Main(): H: importOK = Constant.FAILURE");
                        DropAllAssociatedTables(destName);
                        Ssutil.UtDisconnect(1);
                        ErrMsg.UtPrintMessage(importOK);
                        WriteSuccessOrFailMsg(importOK, mProgramName);
                        Application.ExitQuietly(importOK);
                    }

                    // Validate the fields contents of each qualified line, assemble
                    // complete records and write them to the appropriate database table.
                    foreach (QualLine qualLine in qualLines)
                    {
                        //...Log2.v(String.Format("\n\r\nFeImport.Main(): parsing line {0}: {1}", qualLine.LineNum, qualLine.LineText));

                        // Show progress if verbose.
                        if (mIsVerbose)
                        {
                            Console.Error.Write(String.Format("\r\nParsing Line {0}: {1}", qualLine.LineNum, qualLine.LineText));
                        }

                        // Parse each line according to its type, designated by the first 2 chars.
                        switch (qualLine.Qualifier)
                        {
                            //=======================================================================
                            //======================= Title Record ==================================
                            //=======================================================================

                            // Title Earth Station.
                            case TE:
                                TitleRecord.Parse_TE(ref importOK, qualLine);
                                break;

                            // Title Description.
                            case TD:
                                TitleRecord.Parse_TD(ref importOK, qualLine, mTitlHandle);
                                break;

                            //=======================================================================
                            //======================= Change of Location Code =======================
                            //=======================================================================

                            // Location Key.
                            case LK:
                                ChangeRecord.Parse_LK(ref importOK, mTab5Name, qualLine, mCLocHandle);
                                break;

                            //=======================================================================
                            //======================= Change of Call Sign ===========================
                            //=======================================================================

                            case GK:
                                ChangeRecord.Parse_GK(ref importOK, mTab7Name, qualLine, mCCalHandle);
                                break;

                            //=======================================================================
                            //======================= Site Records ==================================
                            //=======================================================================

                            // Site Key.
                            case SK:
                                SiteRecord.Parse_SK(ref importOK, qualLine);
                                break;

                            // Site Detail.
                            case SD:
                                SiteRecord.Parse_SD(ref importOK, mTab2Name, qualLine, mSiteHandle);
                                break;

                            //=======================================================================
                            //======================= Antenna Records ===============================
                            //=======================================================================

                            // Antenna Keys.
                            case AK:
                                AnteRecord.Parse_AK(ref importOK, qualLine);
                                break;

                            //// Antenna Transmit.
                            case AT:
                                AnteRecord.Parse_AT(ref importOK, qualLine);
                                break;

                            //// Antenna Receive.
                            case AR:
                                AnteRecord.Parse_AR(ref importOK, qualLine);
                                break;

                            //// Antenna Satellite.
                            case AS:
                                AnteRecord.Parse_AS(ref importOK, mTab3Name, qualLine, mAnteHandle);
                                break;

                            //=======================================================================
                            //======================= Azimuth Records ===============================
                            //=======================================================================

                            // aZimuth Keys.
                            case ZK:
                                AzimRecord.Parse_ZK(ref importOK, qualLine, mAzimHandle);
                                break;

                            //=======================================================================
                            //======================= Channel Records ===============================
                            //=======================================================================

                            // Channel Keys.
                            case CK:
                                ChanRecord.Parse_CK(ref importOK, qualLine);
                                break;

                            // Channel Transmit.
                            case CT:
                                ChanRecord.Parse_CT(ref importOK, qualLine);
                                break;

                            // Channel Receive.
                            case CR:

                                ChanRecord.Parse_CR(ref importOK, mTab4Name, qualLine, mChanHandle);
                                break;

                            // Unknown record type.
                            default:
                                //...Log2.v("\nFeImport.Main(): unknown ES record type/qualifier: " + qualLine.Qualifier);
                                //...Log2.v("\nFeImport.Main(): K: importOK = Constant.FAILURE");
                                DropAllAssociatedTables(destName);
                                Ssutil.UtDisconnect(1);
                                importOK = Error.UNKNOWNSECORDTYPE;
                                msgBuf = String.Format("Error - line #{0}, unknown ES record type/qualifier\n", qualLine.LineNum);
                                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                                Application.ExitQuietly(importOK);
                                break;

                        } // switch.

                    } // foreach line.

                    // The <a href="ES Data File - Text File Format.pdf">ES data file specification</a> 
                    // ensures that the ES import file must have a first qualified line of type 'TE'. 
                    // Optionally, this can be followed by a qualified line of type 'TD'. It is the parsing
                    // of the 'TD' line that triggers the insertion of a _titl record into the database.
                    // If no 'TD' line is present, no _titl record will have been inserted yet. 
                    // Consequently, we need to add a _titl record if none already exists and set its validated field to 'N'.
                    AddTitleRecordIfNoneExists(ref importOK);

                }

                // Free the ODBC resources.
                CloseODBChandles();

                // Complete the billing record.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Close the MICS ODBC session.
                Ssutil.UtDisconnect(1);

                // Final 'success' or 'fail' message to Console.Out
                WriteSuccessOrFailMsg(importOK, mProgramName);

                // We're done.
                Application.ExitQuietly(importOK);

            }
            catch
            {
                //...Log2.e("\n\nFeImport.Main(): exception caught: " + e.Message);
                //...Log2.e("\n\nFeImport.Main(): stack trace: \n\n" + e.StackTrace);
                DropAllAssociatedTables(destName);
                Ssutil.UtDisconnect(1);
                Application.Exit(Error.FATAL_EXCEPTION);
            }

        } // Main()

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when FeImport
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n\r\nUSAGE: feImport [-v] [-d] <dbname> <projectCode> <destName> <importFilePath>");
            Console.Write("\r\n");
            Console.Write("\r\n       -v              : optional: verbose mode (shows line-by-line).");
            Console.Write("\r\n       -d              : optional: forces any existing associated DB tables to be dropped.");
            Console.Write("\r\n       -x              : optional: same as -d but program exits after tables are dropped;");
            Console.Write("\r\n                                   (for importFilePath use a junk string).");
            Console.Write("\r\n       dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n       projectCode     : project code, e.g. 'hulme1_34'");
            Console.Write("\r\n       destName        : destination name 'infix' from which all associated DB table names are derived.");
            Console.Write("\r\n       importFilePath  : full Windows path name of the ES text file to be imported.");
        }

        /// <summary>
        /// This method drops all existing tables associated with the prescribed destination name. 
        /// </summary>
        public static void DropAllAssociatedTables(string destName)
        {

            //...Log2.v("\nFeImport.DropAllAssociatedTables(): dropping all tables associated with destName: " + destName);
            Ssutil.UtCleanupTables(Constant.FE, destName);

            // Delete any entry for destName in the 'central table' web.user_tables.
            string cSQL = String.Format("DELETE FROM web.user_tables WHERE operator='{0}' AND tabletype='{1}' AND file_name='{2}'",
                                            Info.GlobalSchema, Constant.FE, destName);

            if (Ssutil.DbExecute(cSQL))
            {
                //...Log2.v("\nFeImport.DropAllAssociatedTables(): successfully dropped central table record for: " + destName);
            }
            else
            {
                //...Log2.e("\nFeImport.DropAllAssociatedTables(): ERROR: call to DbExecute() failed for cSQL = " + cSQL);
                Application.Exit("failed to drop central table for: " + destName, Error.ODBC_EXECUTE_FAILED);
            }
        }

        /// <summary>
        /// This method parses the command line arguments and uses their values to set
        /// various associated internal variables and flags.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        /// <param name="dropTablesThenExit"> - indicates whether a 'forced' drop of existing tables then program exit is to be performed.</param>
        /// <param name="dropTablesRequested"> - indicates whether a 'forced' drop of existing tables is to be performed.</param>
        /// <param name="dbName"> - name of SQL Server database (e.g. 'fcsa').</param>
        /// <param name="projectCode"> - MICS user's project charge code.</param>
        /// <param name="destName"> - destination name (table names' lexical 'infix').</param>
        /// <param name="pdfImportFile"> - full Windows path of the ES data file to be imported.</param>
        public static void ParseCommandLineArgs(string[] args, out bool dropTablesThenExit, out bool dropTablesRequested, out string dbName, out string projectCode, out string destName, out string pdfImportFile)
        {
            // 'out' requirements.
            dropTablesThenExit = false;
            dropTablesRequested = false;
            dbName = null;
            projectCode = null;
            destName = null;
            pdfImportFile = null;

            List<string> argsWithNoFlags = new List<string>();
            List<string> importFileList = new List<string>();

            // No command line args?
            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Environment.Exit(Error.COMMAND_LINE_ERROR);
            }

            // Parse for, and strip out, options prefixed by '-' that can occur 
            // anywhere in the command line argument list.
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg.Trim().ToUpper())
                    {
                        case "-D":
                            dropTablesRequested = true;
                            break;
                        case "-X":
                            dropTablesRequested = true;
                            dropTablesThenExit = true;
                            break;
                        case "-V":
                            mIsVerbose = true;
                            break;
                        default:
                            //...Log2.e("\nFeImport.Main(): ERROR: invalid option flag: " + arg);
                            WriteUsageToConsole();
                            Environment.Exit(Error.COMMAND_LINE_ERROR);
                            break;
                    }
                }
                else
                {
                    argsWithNoFlags.Add(arg);
                }
            }

            //...Log2.v("\nFeImport.ParseCommandLineArgs: DropTables = " + DropTablesRequested);

            if (argsWithNoFlags.Count != 4)
            {
                // Missing parameters.
                WriteUsageToConsole();
                Environment.Exit(Error.COMMAND_LINE_ERROR);
            }

            dbName = argsWithNoFlags[0];
            projectCode = argsWithNoFlags[1];
            destName = argsWithNoFlags[2];

            pdfImportFile = argsWithNoFlags[3];
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute FeImport; specifically these are 'MICSUSER'
        /// and 'PASSWORD'.
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                //...Log2.e("\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                Application.Exit(Error.ENVVARMICSUSERNOTSET);
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
        /// This method creates the full set of SQL Server table names that 
        /// can be inserted into the database i.a.w. the contents of the
        /// ES data and the prescribed destination name ('infix').
        /// </summary>
        /// <description>
        /// As an example, for the MICS user name 'hulme' and destination name
        /// 'destname' the following qty. 8 distinct tables can be inserted
        /// into the database by a successful FeImport operation:
        /// <list type="bullet">
        /// <item>hulme.fe_destname_ante</item>
        /// <item>hulme.fe_destname_azim</item>
        /// <item>hulme.fe_destname_ccal</item>
        /// <item>hulme.fe_destname_chan</item>
        /// <item>hulme.fe_destname_cloc</item>
        /// <item>hulme.fe_destname_shrl</item>
        /// <item>hulme.fe_destname_site</item>
        /// <item>hulme.fe_destname_titl</item>
        /// </list>
        /// </description>
        /// <param name="destName"> - the destination name prescribed in the command line.</param>
        public static void CreateAssocTableNames(string destName)
        {
            GenUtil.UtCvtName(Constant.FE_TITL, destName, out mTab1Name);
            GenUtil.UtCvtName(Constant.FE_SITE, destName, out mTab2Name);
            GenUtil.UtCvtName(Constant.FE_ANTE, destName, out mTab3Name);
            GenUtil.UtCvtName(Constant.FE_CHAN, destName, out mTab4Name);
            GenUtil.UtCvtName(Constant.FE_CLOC, destName, out mTab5Name);
            GenUtil.UtCvtName(Constant.FE_CCAL, destName, out mTab7Name);
            GenUtil.UtCvtName(Constant.FE_AZIM, destName, out mTab6Name);
        }

        /// <summary>
        /// This method initializes a set of MICS 'cursor handles' that prescribe 
        /// the SQL SELECT operations (and subsequent SQL INSERT) for each of the 
        /// qty. 6 table names derived from the destination name.
        /// </summary>
        /// <param name="importOK"> - set to zero on output if no errors occur.</param>
        public static void CreateCursorsForDbTableInserts(ref short importOK)
        {
            if ((mTitlHandle = DynFeTitl.FeSelectTitl(mTab1Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mTitlHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): A: importOK = Constant.FAILURE");
            }
            else if ((mSiteHandle = DynFeSite.FeSelectSite(mTab2Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mSiteHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): B: importOK = Constant.FAILURE");
            }
            else if ((mAnteHandle = DynFeAnte.FeSelectAnte(mTab3Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mAnteHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): C: importOK = Constant.FAILURE");
            }
            else if ((mChanHandle = DynFeChan.FeSelectChan(mTab4Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mChanHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): D: importOK = Constant.FAILURE");

            }
            else if ((mCLocHandle = DynFeCLoc.FeSelectCLoc(mTab5Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mCLocHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): E: importOK = Constant.FAILURE");
            }
            else if ((mCCalHandle = DynFeCCal.FeSelectCCal(mTab7Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mCCalHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): F: importOK = Constant.FAILURE");
            }
            else if ((mAzimHandle = DynFeAzim.FeSelectAzim(mTab6Name, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mAzimHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): G: importOK = Constant.FAILURE");
            }

            return;
        }

        /// <summary>
        /// This methods closes (releases) the set of MICS cursor handles that were
        /// initialized using the method CreateCursorsForDbTableInserts().
        /// </summary>
        public static void CloseODBChandles()
        {
            DynFeTitl.FeCloseTitl(mTitlHandle);
            DynFeSite.FeCloseSite(mSiteHandle);
            DynFeAnte.FeCloseAnte(mAnteHandle);
            DynFeChan.FeCloseChan(mChanHandle);
            DynFeCLoc.FeCloseCLoc(mCLocHandle);
            DynFeCCal.FeCloseCCal(mCCalHandle);
            DynFeAzim.FeCloseAzim(mAzimHandle);
        }

        /// <summary>
        /// This method inserts a title record into the associated _titl DB table 
        /// if one does not already exist.
        /// </summary>
        /// <param name="importOK"> - returns zero if the operation is successful.</param>
        public static void AddTitleRecordIfNoneExists(ref short importOK)
        {
            int rc;
            mTitlHandle = DynFeTitl.FeSelectTitl(mTab1Name, "", "");
            if (mTitlHandle < 0)
            {
                ErrMsg.UtPrintMessage(mTitlHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFeImport.Main(): L: importOK = Constant.FAILURE");
            }
            else
            {
                FeTitl titlStruct;
                SQLLEN[] titlNulls;

                if ((rc = DynFeTitl.FeFetchTitl(mTitlHandle, out titlStruct, out titlNulls)) != Constant.SUCCESS)
                {
                    if (rc == Constant.NOMORERECS)
                    {
                        // Add a record to the table.
                        titlStruct = new FeTitl();
                        titlNulls = NullHelper.CreateArrayOfNullInd(FeTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                        titlStruct.validated = Constant.NOT_VALIDATED.ToString();
                        titlNulls[FeTitl.VALIDATED] = Constant.DB_NOT_NULL;

                        /* add to ingres file */
                        if ((rc = DynFeTitl.FeInsertTitl(mTitlHandle, titlStruct, titlNulls)) != Constant.SUCCESS)
                        {
                            ErrMsg.UtPrintMessage(rc);
                            importOK = Constant.FAILURE;
                            //...Log2.v("\nFeImport.Main(): M: importOK = Constant.FAILURE");
                        }
                    }
                    else
                    {
                        // Something bad happened.
                        ErrMsg.UtPrintMessage(rc);
                        importOK = Constant.FAILURE;
                        //...Log2.v("\nFeImport.Main(): N: importOK = Constant.FAILURE");
                    }
                }

            }
        }

        /// <summary>
        /// This method writes a simple 'success or fail" message to Console.Out tagged
        /// with the prescribed program name.
        /// </summary>
        /// <param name="importOK"> - zero = successful; otherwise fail.</param>
        /// <param name="programName"> - prescribed program name.</param>
        public static void WriteSuccessOrFailMsg(short importOK, string programName)
        {
            string finalMsg;

            if (importOK == 0)
            {
                finalMsg = programName + " attempt was SUCCESSFUL.";
            }
            else
            {
                finalMsg = programName + " attempt FAILED; exitCode = " + importOK;
            }
            Console.Write("\r\n" + finalMsg + "\r\n");
        }


    }
}

```
