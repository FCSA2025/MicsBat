using _Configuration;
using _NewLib;
using _OHloss;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>

/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - ImportAnte.PNG" ""
/// </remarks>
namespace ImportAnte
{
    using _DataStructures;
    using SQLLEN = Int64;

    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class ImportAnte
    {
        private static string mImportFile;
        private static bool mForceDropTable = false;

        /// <summary>
        /// This is the Main() method for the MICS program 
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            TextReader mTR = null;
            string inputBuffer;
            string tab1Name;
            string tab2Name;
            string msgBuf;
            int lineNum;
            int anteHandle = -1;
            int antdHandle = -1;
            int rc;
            int tableType;
            int numFields;
            int recType;
            short importOK;
            string[] fields = null;

            SuAntd antdStruct = new SuAntd();
            SQLLEN[] antdNulls = NullHelper.CreateArrayOfNullInd(SuAntd.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            tableType = Constant.SU_ANTE;

            importOK = Constant.SUCCESS;
            lineNum = 0;

            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\users\ahulme\temp\ImportAnte.log";
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

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                // UtConnect() expects to receive the user's MICS ID and password
                // from the static class Info. Parse the environmental variables
                // to get and set the Info fields for MICS ID and password.
                GetEnvVariablesForUtConnect();

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Log2.e("\nImportAnte.Main(): ERROR: call to Ssutil.UtConnect() failed.");
                    ErrMsg.UtPrintMessage(Error.NODATABASE, Info.DbName);

                    Application.ExitQuietly(Constant.FAILURE);
                }

                // e.g. tab1Name = hulme.su_tablename_ante
                // e.g. tab2Name = hulme.su_tablename_antd
                GenUtil.UtCvtName(Constant.SU_ANTE, Info.DestName, out tab1Name);
                GenUtil.UtCvtName(Constant.SU_ANTD, Info.DestName, out tab2Name);

                //...Log2.v("\nImportAnte.Main(): tab1Name = " + tab1Name);
                //...Log2.v("\nImportAnte.Main(): tab2Name = " + tab2Name);

                // If the table already exists we flag the error and exit the program.
                if (Ssutil.UtTableExist(tableType, Info.DestName))
                {
                    if (mForceDropTable)
                    {
                        rc = Ssutil.UtDropTable(tableType, Info.DestName);
                        //...Log2.v("\nImportAnte.Main(): Ssutil.UtDropTable() returned: " + rc);
                    }
                    else
                    {
                        Log2.e("\nImportAnte.Main(): ERROR: call to Ssutil.UtTableExist() returned true.");
                        ErrMsg.UtPrintMessage(Error.TABLEEXIST, Info.DestName);
                        importOK = Error.TABLEEXIST;
                        Ssutil.UtDisconnect(1);
                        Application.ExitQuietly(importOK);
                    }
                }


                // The table does not already exist.


                rc = Ssutil.UtCreateTable(tableType, Info.DestName);

                //...Log2.v("\nImportAnte.Main(): call to Ssutil.UtCreateTable() returned: " + rc);

                // DB Table: fcsa.tabledef.master_su_ante

                if ((anteHandle = SuDynAnte.SuSelectAnte(tab1Name, "", "")) < 0)
                {
                    ErrMsg.UtPrintMessage(anteHandle);
                    importOK = Constant.FAILURE;
                    Ssutil.UtDropTable(tableType, Info.DestName);
                    Ssutil.UtDisconnect(1);
                    Application.ExitQuietly(importOK);
                }

                if ((antdHandle = SuDynAntd.SuSelectAntd(tab2Name, "", "")) < 0)
                {
                    ErrMsg.UtPrintMessage(antdHandle);
                    importOK = Constant.FAILURE;
                    Ssutil.UtDropTable(tableType, Info.DestName);
                    Ssutil.UtDisconnect(1);
                    Application.ExitQuietly(importOK);
                }




                // Verify that the prescribed input file is a valid path and is readable.
                try
                {
                    // If mImportFile is a relative path then it is assumed
                    // to be relative to the User's current working directory,
                    // i.e. the directory in which the program was launched.
                    mTR = File.OpenText(mImportFile);
                }
                catch (Exception e)
                {
                    Log2.e("\nImportAnte.Main(): ERROR: call to File.OpenText() threw an exception:\n" + e.Message);
                    ErrMsg.UtPrintMessage(Error.IMPNOTEXIST);
                    importOK = Constant.FAILURE;
                    Application.ExitQuietly(importOK);
                }


                // Read lines from the input file; stop looping on EOF.
                while ((inputBuffer = mTR.ReadLine()) != null)
                {
                    lineNum++;
                    //...Log2.v("\n\nImportAnte.Main(): lineNum = " + lineNum);
                    //...Log2.v("\nImportAnte.Main(): inputBuffer = " + Strings.AddBars(inputBuffer));

                    if (Strings.FirstCharIs(inputBuffer.Trim(), Constant.COMMENT_CHAR))
                    {
                        continue;
                    }

                    SuAnte anteStruct = new SuAnte();
                    SQLLEN[] anteNulls = NullHelper.CreateArrayOfNullInd(SuAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    /*
                     * TASK 499: Count the number of fields
                     * so that later we will know whether
                     * we should check for the dtilt field
                     */
                    numFields = CSV.ParseForFields(inputBuffer, out fields);
                    //...Log2.v("\n\nImportAnte.Main(): numFields = " + numFields);

                    if ((rc = CSV.ParseFieldAsInt(fields, 0, out recType)) == Constant.UT_INV_CONV)
                    {
                        msgBuf = String.Format("Error - line #{0}, import Ante RECORD TYPE field, invalid conversion\n",
                                        lineNum);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                        importOK = Constant.FAILURE;
                    }
                    else if (rc == Constant.UT_EOLN)
                    {
                        msgBuf = String.Format("Error - line #{0}, import Ante RECORD TYPE field, end of data found\n",
                                        lineNum);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                        importOK = Constant.FAILURE;
                    }

                    //...Log2.v("\nImportAnte.Main(): recType = " + recType);

                    if (recType == Constant.MASTERREC)
                    {
                        //...Log2.v("\n\nImporteAnte.Main(): lineNum = " + lineNum);
                        //...Log2.v("\nImporteAnte.Main(): ===== DETECTED START OF MASTER RECORD =====\n\n");

                        MasterRecord.ParseFirstLine(lineNum, fields, out anteStruct, out anteNulls, ref importOK);

                        //...Log2.v("\nMaster Record: 1st line" + anteStruct.ToStringWN(anteNulls));


                        // Try to read the following line.
                        if ((inputBuffer = mTR.ReadLine()) == null)
                        {
                            break;
                        }

                        lineNum++;

                        /*
                         * TASK 499: Count the number of fields
                         * so that later we will know whether
                         * we should check for the dtilt field
                         */
                        numFields = CSV.ParseForFields(inputBuffer, out fields);



                        if ((rc = CSV.ParseFieldAsInt(fields, 0, out recType)) == Constant.UT_INV_CONV)
                        {
                            msgBuf = String.Format("Error - line #{0}, import Ante RECORD TYPE field, invalid conversion\n", lineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }
                        else if (rc == Constant.UT_EOLN)
                        {
                            msgBuf = String.Format("Error - line #{0}, import Ante RECORD TYPE field, end of data found\n", lineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }

                        MasterRecord.ParseSecondLine(lineNum, fields, ref anteStruct, ref anteNulls, ref importOK);

                        //...Log2.v("\nMaster Record: 2nd line" + anteStruct.ToStringWN(anteNulls));


                        // Try to read the following line.
                        if ((inputBuffer = mTR.ReadLine()) == null)
                        {
                            break;
                        }

                        lineNum++;

                        numFields = CSV.ParseForFields(inputBuffer, out fields);

                        if (Strings.FirstCharIs(inputBuffer.Trim(), Constant.COMMENT_CHAR))
                        {
                            msgBuf = String.Format("Warning - third Master Record expected; moving on to detail.\r\n");
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            continue;
                        }


                        if ((rc = CSV.ParseFieldAsInt(fields, 0, out recType)) == Constant.UT_INV_CONV)
                        {
                            msgBuf = String.Format("Error - line #{0}, import Ante RECORD TYPE field, invalid conversion\n", lineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }
                        else if (rc == Constant.UT_EOLN)
                        {
                            msgBuf = String.Format("Error - line #{0}, import Ante RECORD TYPE field, end of data found\n", lineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }

                        MasterRecord.ParseThirdLine(lineNum, fields, ref anteStruct, ref anteNulls, ref importOK);

                        //...Log2.v("\nMaster Record: 3rd line" + anteStruct.ToStringWN(anteNulls));

                        /* Task 1061a: New code */


                        /* Task 1061a: End new code */

                        /* check to see if it exists */
                        if ((rc = SuDynAnte.SuAnteExist(tab1Name, anteStruct)) == Constant.FOUND)
                        {
                            //...Log2.v("\n\nImporteAnte.Main(): record already exists for acode = " + anteStruct.acode);
                            msgBuf = String.Format("");
                            msgBuf = String.Format("Error - line #{0}, duplicate record\n", lineNum);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                            importOK = Constant.FAILURE;
                        }
                        else if (rc != Constant.NOT_FOUND)
                        {
                            // Some type of DB query error must have happened.
                            Log2.e("\n\nImporteAnte.Main(): ERROR: call to SuDynAnte.SuAnteExist() returned rc = " + rc);
                            ErrMsg.UtPrintMessage(rc);
                            importOK = Constant.FAILURE;
                        }

                        if (importOK == Constant.SUCCESS)
                        {
                            //...Log2.v("\n\nImporteAnte.Main(): no existing record with  acode = " + anteStruct.acode);

                            /* add to ingres file */
                            if ((rc = SuDynAnte.SuInsertAnte(anteHandle, anteStruct, anteNulls)) != Constant.SUCCESS)
                            {
                                Log2.e("\n\nImporteAnte.Main(): ERROR: call to SuDynAnte.SuInsertAnte() returned rc = " + rc);
                                importOK = Constant.FAILURE;
                                ErrMsg.UtPrintMessage(rc);
                            }
                        }

                    }
                    else if (recType == Constant.DETAILREC)
                    {
                        /*
                         * TASK 499: Processing for dtilt field added for
                         * this task, for PCS Antenna tilt calculation.
                         * Look for the dtilt field ONLY if the number
                         * of fields, as indicated by the number of comma
                         * delimiters, is more than SuAntd.MTIME.
                         */
                        //...Log2.v("\n\nImporteAnte.Main(): lineNum = " + lineNum);
                        //...Log2.v("\nImporteAnte.Main(): ===== DETECTED START OF DETAIL RECORD =====\n\n");

                        DetailRecord.ParseLine(lineNum, fields, out antdStruct, out antdNulls, ref importOK);

                        //...Log2.v(String.Format("\nLine# {0,6}: Detail Record\n{1}", lineNum, antdStruct.ToStringWN(antdNulls)));


                        if (importOK == Constant.SUCCESS)
                        {
                            /* add to ingres file */
                            if ((rc = SuDynAntd.SuInsertAntd(antdHandle, antdStruct, antdNulls)) != Constant.SUCCESS)
                            {
                                importOK = Constant.FAILURE;
                                ErrMsg.UtPrintMessage(rc);
                            }
                        }

                    }
                    else
                    { /* unknown record type */
                        msgBuf = String.Format("Error - line #{0}, unknown Ante record type\n", lineNum);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                        importOK = Constant.FAILURE;
                    }

                } // while read lines from input file

                mTR.Close();

                SuDynAnte.SuCloseAnte(anteHandle);
                SuDynAntd.SuCloseAntd(antdHandle);

                if (importOK == Constant.FAILURE)
                {
                    Ssutil.UtDropTable(tableType, Info.DestName);
                }

                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(importOK);

            }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nImportAnte.Main(): exception caught: " + e.Message);
                Log2.e("\r\n\r\nImportAnte.Main(): stack trace: \r\n\r\n" + e.StackTrace);
                Ssutil.UtDisconnect(1);
                Application.ExitQuietly(Error.FATAL_EXCEPTION);
            }

        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariablesForUtConnect()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\r\nImportAnte.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n ImportAnte reads 'master' and 'detail' antenna records from");
            Console.Write("\r\n an import text file, parses the fields, creates su_XXX_ante");
            Console.Write("\r\n and su_XXX_antd tables and populates these with records.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: ImportAnte <dbName> <tableName> <importFile> [-f]");
            Console.Write("\r\n =====");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        tableName        : table root name.");
            Console.Write("\r\n        importFile       : path of import file.");
            Console.Write("\r\n        -f               : force drop of existing tables");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     ImportAnte  fcsa  fxa  d:\\temp\\fxaV1.txt");
            Console.Write("\r\n     ");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (!((args.Length == 3) || (args.Length == 4)))
            {
                Log2.e("\nImportAnte.ParseCommandLineArgs(): ERROR: invalid args: " + Strings.OneLine(args));
                WriteUsageToConsole();
                Application.ExitQuietly(Constant.FAILURE);
            }

            Info.DbName = args[0];
            Info.DestName = args[1];
            mImportFile = args[2];

            if ((args.Length == 4) && args[3].ToUpper().Equals("-F"))
            {
                mForceDropTable = true;
            }

            //...Log2.v("\nImportAnte.ParseCommandLineArgs(): Info.DbName     = " + Info.DbName);
            //...Log2.v("\nImportAnte.ParseCommandLineArgs(): Info.DestName   = " + Info.DestName);
            //...Log2.v("\nImportAnte.ParseCommandLineArgs(): mImportFile     = " + mImportFile);
            //...Log2.v("\nImportAnte.ParseCommandLineArgs(): mForceDropTable = " + mForceDropTable);
        }




    }
}


