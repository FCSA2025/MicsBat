using _DataStructures;
using System;
using System.Collections.Generic;
using static _NewLib.Enums.FtImpQual;
using _NewLib;
using _Configuration;
using System.IO;

/// <summary>
/// This application imports the contents of an TS data file, performs syntactic
/// validation and, if no errors are found, inserts new tables into the database.
/// </summary>
/// <description>
/// As an example, for the MICS user name 'hulme' and destination name
/// 'destname' the following qty. 6 distinct tables can be inserted
/// into the database by a successful FtImport operation:
/// <list type="bullet">
/// <item>hulme.ft_destname_ante</item>
/// <item>hulme.ft_destname_chan</item>
/// <item>hulme.ft_destname_chng</item>
/// <item>hulme.ft_destname_shrl</item>
/// <item>hulme.ft_destname_site</item>
/// <item>hulme.ft_destname_titl</item>
/// </list>
/// </description>
namespace FtImport
{
    using SQLLEN = Int64;
    using _Utillib;
    using static Enums;

    /// <summary>
    /// This class contains the Main() method that performs the
    /// top-level processing and control of the
    /// FtImport functionality together with its supporting methods.
    /// </summary>
    public class FtImport
    {
        // The full DB table names.
        private static string mTableName_titl;
        private static string mTableName_chng;
        private static string mTableName_site;
        private static string mTableName_ante;
        private static string mTableName_chan;

        // Handles to cursors that 'SELECT' a
        // DB table prior to record 'INSERT'. 
        private static int mTitlInsertHandle;
        private static int mSiteInsertHandle;
        private static int mAnteInsertHandle;
        private static int mChanInsertHandle;
        private static int mChngInsertHandle;

        private static bool mIsVerbose = false;

        /// <summary>
        /// This is the Main() method that performs the top-level processing 
        /// and control of the FtImport functionality.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        static void Main(string[] args)
        {
            // Enable developmental logging.
#if true
            string mLog2FilePath = @"D:\MicsBatchLogs\FtImport.log";
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
            try
            {
                string str = "";
                string[] lines = null;
                short importOK;
                FileInfo fileInfo = null;
                string msgBuf;
                //string projectCode;
                string importFilePath;

                int nRet;
                bool dropTablesRequested = false;
                bool dropTablesThenExit = false;

                // Parse the command line arguments.
                ParseCommandLineArgs(args,
                                        out dropTablesThenExit,
                                        out dropTablesRequested,
                                        out importFilePath
                                    );

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariables();

                // Queue for database access.
                if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)) != 0)
                {
                    Log2.e("\nFtImport.Main(): ERROR: call to Qutils.EnterQueue() failed.");
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, null);
                    Application.ExitQuietly(Error.UNABLE_TO_ENTER_QUEUE);
                }

                // Start a MICS user session.
                nRet = Ssutil.UtConnect(Info.DbName, 1);
                if (nRet != 0)
                {
                    // Can't connect to database.
                    str = String.Format("\r\nImport.Main(): ERROR: Cannot connect to database {0}, reason {1:D}\r\n", Info.DbName, nRet);
                    Log2.e(str);
                    Console.Write(str);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.Exit(Error.ODBC_SQLCONNECT_FAILED);
                }

                // Initialize the billing record.
                BiUtil.BiBillingRec("FTIMPORT", Info.DestName);

                // If '-f' or '-x' option is prescribed on the command line drop all 
                // existing tables associated with the destination name. 
                if (dropTablesRequested)
                {
                    DropAllAssociatedTables(Info.DestName);
                }

                // If '-x' option is prescribed on the command line then exit without 
                // importing a PDF.
                if (dropTablesThenExit)
                {
                    // Check that all TS tables for PDF destName were successfully dropped.
                    if (Ssutil.UtTableExist(Constant.FT, Info.DestName))
                    {
                        Console.Write("\r\nAttempt to drop all TS tables for PDF '{0}' FAILED.", Info.DestName);
                    }
                    else
                    {
                        Console.Write("\r\nAll TS tables for PDF '{0}' were successfull dropped.", Info.DestName);

                    }

                    Ssutil.UtDisconnect(1);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    Application.ExitQuietly(0);
                }

                // Now proceed as if the '-f' option was not called so as to mimic
                // the flow of the legacy C++ code.

                // Check if any of the Ft tables associated with Info.DestName already exists; 
                // if so, the policy is that we must exit.
                bool someTablesAlreadyExist = Ssutil.UtTableExist(Constant.FT, Info.DestName);

                if (someTablesAlreadyExist)
                {
                    Ssutil.UtDisconnect(1);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    importOK = Error.TABLEEXIST;
                    ErrMsg.UtPrintMessage(importOK, Info.DestName);
                    Application.ExitQuietly(importOK);
                }
                else
                {
                    //...Log2.v("\nFtImport.Main(): no tables currently exist for: " + Info.DestName);
                }

                // Create the _titl, _chng, _site, _ante, _chan and _shrl tables.
                nRet = Ssutil.UtCreateTable(Constant.FT, Info.DestName);
                if (nRet != Constant.SUCCESS)
                {
                    Log2.e("\nFtImport.Main(): ERROR: call to Ssutil.UtCreateTable() failed.");
                    Ssutil.UtDisconnect(1);
                    Qutils.ExitQueue(Info.DbName, "READ");
                    importOK = 100;
                    Console.Write("\r\nftImport: Error creating table {0}.\r\n\t{1}", Info.DestName, GenUtil.GetUserMess());
                    Application.ExitQuietly(importOK);
                }

                // Initialize parse sequence-tracking variables.
                importOK = Constant.SUCCESS;

                // Get the fully-qualified SQL database table names associated
                // with the destName.
                CreateAssocTableNames(Info.DestName);

                // Select the DB tables into which the imported data will be inserted.
                CreateCursorsForDbTableInserts(ref importOK);
                //...Log2.v("\nFtImport.Main(): CreateCursorsForDbTableInserts() returned importOK =  " + importOK);

                // The static class 'State' handles the writing of accumulated Site,
                // Ante and Chan data to the DB. 
                // Consequently, we must pass the associated table names and
                // ODBC insert handles to it.
                State.SiteTableName = mTableName_site;
                State.AnteTableName = mTableName_ante;
                State.ChanTableName = mTableName_chan;

                State.SiteInsertHandle = mSiteInsertHandle;
                State.AnteInsertHandle = mAnteInsertHandle;
                State.ChanInsertHandle = mChanInsertHandle;

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
                    catch (Exception e)
                    {
                        Log2.e("\nFtImport.Main(): ERROR: attempt to read from import file failed: " + e.Message);
                        DropAllAssociatedTables(Info.DestName);
                        Ssutil.UtDisconnect(1);
                        Qutils.ExitQueue(Info.DbName, "READ");
                        importOK = Error.IMPNOTEXIST;
                        ErrMsg.UtPrintMessage(importOK);
                        Application.ExitQuietly(importOK);
                    }

                    // Report if file is empty then exit.
                    if (fileInfo.Length == 0)
                    {
                        //...Log2.v("\nFtImport.Main(): import file is empty: " + importFilePath);
                        DropAllAssociatedTables(Info.DestName);
                        Ssutil.UtDisconnect(1);
                        Qutils.ExitQueue(Info.DbName, "READ");
                        Application.ExitQuietly(Constant.SUCCESS);
                    }

                    //...Log2.v("\n\nFtImport.Main(): successfully read from file: " + importFilePath + "\n");

                    // Perform an initial validation of the import file's contents.
                    // A 'qualified' line is one that begins with a known token, i.e.
                    // TT, TD, GK, SK, SD, AK, AQ, AO, CK, CT, CR, CQ or CO
                    // and has the correct number of CSV token/fields for its type.
                    // Parse through all the lines read from the import file and produce
                    // an array of qualified lines. This will discard any comment or 
                    // blank lines. In addition, qualified lines will be checked for
                    // correct sequence and for 'missing' lines in Site, Antenna and 
                    // Channel records. No validation of a line's CSV field-content 
                    // is performed yet - that will be done in the next stage of parsing.
                    QualLine[] qualLines;

                    importOK = FtParsing.ParseForQualLines(lines, out qualLines);

                    // Validate the fields contents of each qualified line, assemble
                    // complete records and write them to the appropriate database table.
                    for (int index = 0; index < qualLines.Length; index++)
                    {
                        QualLine qualLine = qualLines[index];

                        //...Log2.v("\n\nFtImport.Main(): Line:  " + qualLine.ToString());

                        State.PushQualLine(qualLine);

                        // Show progress if verbose.
                        if (mIsVerbose)
                        {
                            Console.Error.Write(String.Format("\r\nParsing Line {0}: {1}", qualLine.LineNum, qualLine.LineText));
                        }

                        // Parse each line according to its type, determined by the first 2 chars.
                        switch (qualLine.Qualifier)
                        {
                            //=======================================================================
                            //======================= Title Record ==================================
                            //=======================================================================

                            // Title Terrestial Station.
                            case TT:
                                TitleRecord.Parse_TT(ref importOK, mTableName_titl, qualLine, mTitlInsertHandle);
                                break;

                            //=======================================================================
                            //======================= Change of Call Sign ===========================
                            //=======================================================================

                            case GK:
                                ChangeRecord.Parse_GK(ref importOK, mTableName_chng, qualLine, mChngInsertHandle);
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
                                SiteRecord.Parse_SD(ref importOK, mTableName_site, qualLine, mSiteInsertHandle);
                                break;

                            //=======================================================================
                            //======================= Antenna Records ===============================
                            //=======================================================================

                            // Antenna Keys.
                            case AK:
                                AnteRecord.Parse_AK(ref importOK, qualLine);
                                break;

                            //// Antenna Required.
                            case AQ:
                                AnteRecord.Parse_AQ(ref importOK, qualLine);
                                break;

                            //// Antenna Optional.
                            case AO:
                                AnteRecord.Parse_AO(ref importOK, qualLine, mAnteInsertHandle);
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
                                ChanRecord.Parse_CR(ref importOK, qualLine);
                                break;

                            // Channel Required.
                            case CQ:
                                ChanRecord.Parse_CQ(ref importOK, qualLine);
                                break;

                            // Channel Optional.
                            case CO:
                                ChanRecord.Parse_CO(ref importOK, qualLine);
                                break;

                            //=======================================================================
                            // Unknown record type.
                            //=======================================================================
                            case UNKNOWN:
                                //...Log2.v("\nFtImport.Main(): unknown TS record type: \n" + qualLine.ToString());
                                UnknownRecord.Parse_UNKNOWN(qualLine);
                                importOK = Constant.FAILURE;
                                msgBuf = String.Format("Error - line #{0}, unknown TS record type\r\n", qualLine.LineNum);
                                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                                break;

                        } // switch.

                        // Peek at the next qualLine.
                        QualLine nextQualLine = PeekNextQualLine(qualLines, index);

                        // If the next qualLine is EOF check the state and provide
                        // errors and warnings as necessary.
                        State.CheckStateIfJustBeforeEOF(nextQualLine, ref importOK);

                        // If ready, add an FtAnte to State's accumulator list and 
                        // check for duplicates.
                        State.AddAnteIfReady(nextQualLine, ref importOK);

                        // If ready, add an FtChan to State's accumulator list and 
                        // check for duplicates.
                        State.AddChanIfReady(nextQualLine, ref importOK);

                        // SK and SD records can appear in any order.
                        // This could be the start of a new FtSite.
                        // If so, we need to write the previously accumulated FtSite,
                        // FtAnte and FtChan objects out to the DB.
                        // This is handled by the State class method WriteToDBifReady().
                        // This method also checks for a 'duplicate Site record' scenario.
                        State.WriteRecordsToDBifReady(nextQualLine, ref importOK);

                    } // end-of-loop: qualLine.

                    // The TS data file specification
                    // ensures that the TS import file must have a first qualified line of type 'TT'. 
                    // Optionally, this can be followed by a qualified line of type 'TD'. It is the parsing
                    // of the 'TD' line that triggers the insertion of a _titl record into the database.
                    // If no 'TD' line is present, no _titl record will have been inserted yet. 
                    // Consequently, we need to add a _titl record if none already exists and set its validated field to 'N'.
                    AddTitleRecordIfNoneExists(ref importOK);

                    // If something went wrong we drop all the DB tables that may have been
                    // created for this PDF; this ensures the integrity of the import
                    // processing.
                    if (importOK != Constant.SUCCESS)
                    {
                        Ssutil.UtDropTable(Constant.FT, Info.DestName);
                    }

                }

                // Free the ODBC resources.
                CloseODBChandles();

                // Complete the billing record.
                BiUtil.BiBillingRec(Constant.BI_END, "");

                // Close the MICS ODBC session.
                Ssutil.UtDisconnect(1);

                // Exit the DB access queue.
                Qutils.ExitQueue(Info.DbName, "READ");

                // In the event of a failed import, WebMICS expects the
                // exit code to be -1 (and only -1).
                if (importOK != 0)
                {
                    importOK = -1;
                }

                // We're done.
                Application.ExitQuietly(importOK);

            }
            catch (Exception e)
            {
                Log2.e("\n\nFtImport.Main(): exception caught: " + e.Message);
                Log2.e("\n\nFtImport.Main(): stack trace: \n\n" + e.StackTrace);
                DropAllAssociatedTables(Info.DestName);
                Ssutil.UtDisconnect(1);
                Console.Write("\n\nFATAL ERROR:");
                Console.Write("\n\nFtImport.Main(): exception caught: " + e.Message);
                Console.Write("\n\nFtImport.Main(): stack trace: \n\n" + e.StackTrace);
                Qutils.ExitQueue(Info.DbName, "READ");
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        } // Main()

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when FtImport        
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program reads a prescribed TS PDF text file, checks the syntax and parameter");
            Console.Write("\r\n values, and creates and populates a set of user tables in the prescribed database.");
            Console.Write("\r\n");
            Console.Write("\r\n\r\n USAGE: FtImport <dbname> <projectCode> <rootName> <importFilePath> [-f] [-x] [-v] [-!<schema>] ");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n       dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n       projectCode     : project code, e.g. 'hulme1_34'");
            Console.Write("\r\n       rootName        : if this is 'XXX' the the user tables are ft_XXX_site etc.");
            Console.Write("\r\n       importFilePath  : full Windows path name of the TS text file to be imported.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- Options ---------------------------------------------------------------------");
            Console.Write("\r\n       -f              : forces any existing associated DB tables to be dropped.");
            Console.Write("\r\n       -x              : same as -f but program exits after tables are dropped;");
            Console.Write("\r\n                                         (for importFilePath use a junk string).");
            Console.Write("\r\n       -v              : verbose mode (shows line-by-line).");
            Console.Write("\r\n       -!<schema>      : creates and populates user tables with the precribed schema name.");
            Console.Write("\r\n                         Usage restricted to users who are members of the Administrator Group.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     FtImport -f micsprod hulme1_0 aardvark D: \\users\\ahulme\\TS_MIN_PDF_TEMPLATE.txt");
            Console.Write("\r\n");
            Console.Write("\r\n     FtImport -!fmda2 micsprod hulme1_0 aardvark D: \\users\\ahulme\\TS_MIN_PDF_TEMPLATE.txt");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 4 manadatory command-line arguments.");
            Console.Write("\r\n       2. Options can be placed anywhere, in any order, on the command-line.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method drops all existing ft_ tables associated with the prescribed destination name. 
        /// </summary>
        public static void DropAllAssociatedTables(string destName)
        {

            //...Log2.v("\nFtImport.DropAllAssociatedTables(): dropping all tables associated with destName: " + destName);
            Ssutil.UtCleanupTables(Constant.FT, destName);

            // Delete any entry for destName in the 'central table' web.user_tables.
            string cSQL = String.Format("DELETE FROM web.user_tables WHERE operator='{0}' AND tabletype='{1}' AND file_name='{2}'",
                                            Info.GlobalSchema, Constant.FT, destName);

            if (Ssutil.DbExecute(cSQL))
            {
                //...Log2.v("\nFtImport.DropAllAssociatedTables(): successfully dropped central table record for: " + destName);
            }
            else
            {
                Log2.e("\nFtImport.DropAllAssociatedTables(): ERROR: call to DbExecute() failed for cSQL = " + cSQL);
                Application.ExitQuietly(Constant.FAILURE);
            }
        }

        /// <summary>
        /// This method parses the command line arguments and uses their values to set
        /// various associated internal variables and flags.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        /// <param name="dropTablesThenExit"> - indicates whether a 'forced' drop of existing tables then program exit is to be performed.</param>
        /// <param name="dropTablesRequested"> - indicates whether a 'forced' drop of existing tables is to be performed.</param>
        /// <param name="pdfImportFile"> - full Windows path of the TS data file to be imported.</param>
        public static void ParseCommandLineArgs(string[] args, out bool dropTablesThenExit, out bool dropTablesRequested, out string pdfImportFile)
        {
            // 'out' requirements.
            dropTablesThenExit = false;
            dropTablesRequested = false;
            pdfImportFile = null;

            List<string> importFileList = new List<string>();

            // No command line args?
            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Environment.Exit(Error.COMMAND_LINE_ERROR);
            }

            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (Strings.IsNumeric(arg))
                {
                    regularArgs.Add(arg);
                }
                else if (arg.StartsWith("-"))
                {
                    flagArgs.Add(arg);
                }
                else
                {
                    regularArgs.Add(arg);
                }
            }

            // Parse the flags.
            foreach (string arg in flagArgs)
            {
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                }

                // Parse the character after the '-'.
                char secondChar = arg.ToUpper()[1];
                switch (secondChar)
                {
                    case 'F':
                        dropTablesRequested = true;
                        break;
                    case 'X':
                        dropTablesRequested = true;
                        dropTablesThenExit = true;
                        break;
                    case 'V':
                        mIsVerbose = true;
                        break;
                    case '!':
                        if (arg.Length > 2)
                        {
                            string schemaName = arg.Substring(2);

                            // If we don't check the user's permissions now the program may fail at
                            // run time with an SQL error.
                            if (User.IsInAdministratorGroup())
                            {
                                // Save the schema to the Info static structure member.
                                // This has a global effect throughout the code so that all synthesized SQL
                                // queries will explicitely include the schema name 'forced' on the command-line.
                                Info.GlobalSchema = schemaName;
                                Ssutil.Glb_Schema = schemaName;
                            }
                            else
                            {
                                Console.Write("\n The -! option is restricted to users who are members of the Administrators Group.");
                                WriteUsageToConsole();
                                Environment.Exit(98);
                            }
                        }
                        else
                        {
                            Console.Write("\n Invalid syntax for option -!");
                            WriteUsageToConsole();
                            Environment.Exit(98);
                        }
                        break;
                    default:
                        //...Log2.e("\nFtImport.Main(): ERROR: invalid option flag: " + arg);
                        Console.Write("\r\n Invalid option: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Environment.Exit(98);
                        break;
                }
            }

            // Count the regular arguments; there must be qty. 4 of them.
            if (regularArgs.Count != 4)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the regular command-line arguments.
            pdfImportFile = regularArgs[3];

            // Set the associated static properties of the Info class.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            Info.DestName = regularArgs[2];
            Info.PdfName = regularArgs[2];

            // Truncate the pdf name to the first 16 characters when creating
            // associated tables in the database.
            if (Info.PdfName.Length > Constant.DISP_NM_SZ - 1)
            {
                Info.PdfName = Info.PdfName.Substring(0, Constant.DISP_NM_SZ - 1);
                Info.DestName = Info.PdfName;
            }
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute FtImport; specifically these are 'MICSUSER'
        /// and 'PASSWORD'.
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                //...Log2.e("\nFtImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
        /// This method creates the full set of SQL Server table names that 
        /// can be inserted into the database i.a.w. the contents of the
        /// TS data and the prescribed destination name ('infix').
        /// </summary>
        /// <description>
        /// As an example, for the MICS user name 'hulme' and destination name
        /// 'destname' the following qty. 8 distinct tables can be inserted
        /// into the database by a successful FtImport operation:
        /// <list type="bullet">
        /// <item>hulme.ft_destname_ante</item>
        /// <item>hulme.ft_destname_chan</item>
        /// <item>hulme.ft_destname_chng</item>
        /// <item>hulme.ft_destname_shrl</item>
        /// <item>hulme.ft_destname_site</item>
        /// <item>hulme.ft_destname_titl</item>
        /// </list>
        /// </description>
        /// <param name="destName"> - the destination name prescribed in the command line.</param>
        public static void CreateAssocTableNames(string destName)
        {
            GenUtil.UtCvtName(Constant.FT_TITL, destName, out mTableName_titl);
            GenUtil.UtCvtName(Constant.FT_CHNG_CALL, destName, out mTableName_chng);
            GenUtil.UtCvtName(Constant.FT_SITE, destName, out mTableName_site);
            GenUtil.UtCvtName(Constant.FT_ANTE, destName, out mTableName_ante);
            GenUtil.UtCvtName(Constant.FT_CHAN, destName, out mTableName_chan);
        }

        /// <summary>
        /// This method initializes a set of MICS 'cursor handles' that prescribe 
        /// the SQL SELECT operations (and subsequent SQL INSERT) for each of the 
        /// qty. 6 table names derived from the destination name.
        /// </summary>
        /// <param name="importOK"> - only changed if errors occur.</param>
        public static void CreateCursorsForDbTableInserts(ref short importOK)
        {
            if ((mTitlInsertHandle = DynTitle.FtSelectTitle(mTableName_titl, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mTitlInsertHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFtImport.Main().CreateCursorsForDbTableInserts(): A: importOK = Constant.FAILURE");
            }
            else if ((mSiteInsertHandle = DynSite.FtSelectSite(mTableName_site, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mSiteInsertHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFtImport.Main().CreateCursorsForDbTableInserts(): B: importOK = Constant.FAILURE");
            }
            else if ((mAnteInsertHandle = DynAntenna.FtSelectAntenna(mTableName_ante, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mAnteInsertHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFtImport.Main().CreateCursorsForDbTableInserts(): C: importOK = Constant.FAILURE");
            }
            else if ((mChanInsertHandle = DynChannel.FtSelectChannel(mTableName_chan, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mChanInsertHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFtImport.Main().CreateCursorsForDbTableInserts(): D: importOK = Constant.FAILURE");

            }
            else if ((mChngInsertHandle = DynChange.FtSelectChngCall(mTableName_chng, "", "")) < 0)
            {
                ErrMsg.UtPrintMessage(mChngInsertHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFtImport.Main().CreateCursorsForDbTableInserts(): E: importOK = Constant.FAILURE");
            }

            return;
        }

        /// <summary>
        /// This methods closes (releases) the set of MICS cursor handles that were
        /// initialized using the method CreateCursorsForDbTableInserts().
        /// </summary>
        public static void CloseODBChandles()
        {
            DynTitle.FtCloseTitle(mTitlInsertHandle);
            DynSite.FtCloseSite(mSiteInsertHandle);
            DynAntenna.FtCloseAntenna(mAnteInsertHandle);
            DynChannel.FtCloseChannel(mChanInsertHandle);
            DynChange.FtCloseChngCall(mChngInsertHandle);
        }

        /// <summary>
        /// This method inserts a title record into the associated _titl DB table 
        /// if one does not already exist.
        /// </summary>
        /// <param name="importOK"> - value returns unchanged if the operation is successful.</param>
        public static void AddTitleRecordIfNoneExists(ref short importOK)
        {
            int rc;
            mTitlInsertHandle = DynTitle.FtSelectTitle(mTableName_titl, "", "");
            if (mTitlInsertHandle < 0)
            {
                ErrMsg.UtPrintMessage(mTitlInsertHandle);
                importOK = Constant.FAILURE;
                //...Log2.v("\nFtImport.Main(): L: importOK = Constant.FAILURE");
            }
            else
            {
                FtTitl titlStruct;
                SQLLEN[] titlNulls;

                if ((rc = DynTitle.FtFetchTitle(mTitlInsertHandle, out titlStruct, out titlNulls)) != Constant.SUCCESS)
                {
                    if (rc == Constant.NOMORERECS)
                    {
                        // Add a record to the table.
                        titlStruct = new FtTitl();
                        titlNulls = NullHelper.CreateArrayOfNullInd(FtTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                        titlStruct.validated = Constant.NOT_VALIDATED.ToString();
                        titlNulls[FtTitl.VALIDATED] = Constant.DB_NOT_NULL;

                        /* add to ingres file */
                        if ((rc = DynTitle.FtInsertTitl(mTitlInsertHandle, titlStruct, titlNulls)) != Constant.SUCCESS)
                        {
                            ErrMsg.UtPrintMessage(rc);
                            importOK = Constant.FAILURE;
                            //...Log2.v("\nFtImport.Main(): M: importOK = Constant.FAILURE");
                        }
                    }
                    else
                    {
                        // Something bad happened.
                        ErrMsg.UtPrintMessage(rc);
                        importOK = Constant.FAILURE;
                        //...Log2.v("\nFtImport.Main(): N: importOK = Constant.FAILURE");
                    }
                }

            }
        }

        /// <summary>
        /// This method returns the next qualified line in the PDF import 
        /// text file that immediately follows a line prescribed by its list index
        /// number.
        /// </summary>
        /// <param name="qualLines"> - a list containing all qualified lines.</param>
        /// <param name="index"> - the index of the position in the list that we want to 'peek' beyond.</param>
        /// <returns></returns>
        private static QualLine PeekNextQualLine(QualLine[] qualLines, int index)
        {
            QualLine nextQualLine = null;

            // Check if there is actually a next line.
            if (qualLines != null)
            {
                if ((index + 1) < qualLines.Length)
                {
                    nextQualLine = qualLines[index + 1];
                }
                else
                {
                    // Phoney up a qualLine as an 'end of file'.
                    nextQualLine = new QualLine(-1, "", FtImpQual.EOF, null);
                }
            }

            return nextQualLine;
        }


    }
}
