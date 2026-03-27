using _Configuration;
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
/// with a previously imported ES PDF text file and writes it to Console.Out .
/// </summary>
/// <description>
/// Suppose that we have previously imported an ES PDF text file using the
/// command-line invocation:
/// <para>
/// <code>
/// feimport -D fcsa hulme1_0 esMyPDF "D:\Users\ahulme\esMyPDF.txt"
/// </code>
/// </para>
/// There will be qty. 8 tables in the database that were created by this import:
/// <list type="bullet">
/// <item>fcsa.hulme.fe_esMyPDF_ante</item>
/// <item>fcsa.hulme.fe_esMyPDF_azim</item>
/// <item>fcsa.hulme.fe_esMyPDF_ccal</item>
/// <item>fcsa.hulme.fe_esMyPDF_chan</item>
/// <item>fcsa.hulme.fe_esMyPDF_cloc</item>
/// <item>fcsa.hulme.fe_esMyPDF_shrl</item>
/// <item>fcsa.hulme.fe_esMyPDF_site</item>
/// <item>fcsa.hulme.fe_esMyPDF_titl</item>
/// </list>
/// There will also be a record associated with esMyPDF in the database
/// table <b>web.user_tables</b> that is used by WebMICS to determine whether the
/// tables associated with esMyPDF already exist, or not.
/// <para>
/// The FePrint application fetches all the data from these qty. 8 database tables. 
/// It arranges that data in the correct sequence (title -> changes -> site -> antenna -> azimuth -> channel).
/// Finally, it formats the data as strings written to Console.Out .
/// </para>
/// 
/// </description>
namespace FePrint
{
    /// <summary>
    /// This class provides the Main() method for the FePrint application
    /// together with supporting methods that parse the command line arguments.
    /// </summary>
    public class FePrint
    {
        public const string COMMENT_LINE = "*===========================================================================\r\n";
        private const int USER_SESSION = 1;

        private static TextWriter mTW = Console.Out;

        /// <summary>
        /// This is the Main() method for the FePrint application that provides
        /// all of the top-level control.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string printLine = "";
            int rc;
            int nRet;

            // Parse the command line arguments and set values in Info static class.
            // Note that this method call sets mTW to either Console.Out or a user-
            // prescribed file path.
            ParseCommandLineArgs(args);

            // UtConnect() expects to receive the user's MICS ID and password
            // from the static class Info. Parse the environmental variables
            // to get and set the Info fields for MICS ID and password.
            GetEnvVariables();

            if ((nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)) != 0)
            {
                Qutils.ExplainQueue(Info.DbName, "READ", nRet, null);
                Application.Exit(100);
            }

            // Start a MICS user session (connects to the database).
            rc = Ssutil.UtConnect(Info.DbName, USER_SESSION);
            if (rc != 0)
            {
                /* Can't connect to database */
                ErrMsg.UtPrintMessage(Error.NODATABASE, Info.DbName);
                Application.Exit(rc);
            }

            BiUtil.BiBillingRec("FEPRINT", args[2]);

            mTW.Write("{0}{1} ES-PDF NAME: {2}, Produced by fePrint build {3}\r\n{0}",
                COMMENT_LINE, Constant.COMMENT_CHAR, Info.PdfName, Info.GetDateTimeNow());

            if (!Ssutil.UtTableExist(Constant.FE, Info.PdfName))
            {
                ErrMsg.UtPrintMessage(Error.FILENOTEXIST, "EXPORT/PRINT", mTW);
            }
            else
            {
                /* main loop to print flat PDF lines */
                while ((rc = FePrintUtils.FePrintFw(Info.PdfName, ref printLine)) == Constant.SUCCESS)
                {
                    mTW.Write("{0}\r\n", printLine);
                }
                /* an error occured so print it */
                if (rc != Constant.NOMORERECS)
                {
                    ErrMsg.UtPrintMessage(rc, "", mTW);
                }
                else
                {
                    rc = Constant.SUCCESS;
                }

                string str = null;
                FePrintUtils.FePrintFw(null, ref str);
            }

            if (mTW != Console.Out) mTW.Close();

            BiUtil.BiBillingRec(Constant.BI_END, "");

            Ssutil.UtDisconnect(USER_SESSION);

            Qutils.ExitQueue(Info.DbName, "READ");

            Application.ExitQuietly(rc);


        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute FeImport; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariables()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when FtImport
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program exports the record data found in a prescribed user 'fe_' table set  +");
            Console.Write("\r\n as text using the same format as an ES import PDF.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: FePrint <dbname> <projectCode> [-o<outFilePath>] <tableName>");
            Console.Write("\r\n");
            Console.Write("\r\n        dbname          : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        projectCode     : project code, e.g. 'hulme1_34'");
            Console.Write("\r\n        outFilePath     : full path to prescribed output file.");
            Console.Write("\r\n        tableName       : root name of the user's fe_ DB table set to be printed.");
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
            List<string> mandatoryArgs = new List<string>();
            List<string> optionalArgs = new List<string>();

            // Check for too few or too many command-line paramters.
            if ((args.Length < 3) || (args.Length > 4))
            {
                Console.Write("\r\nToo few or too many arguments: should be 3 or 4.");
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
            if (mandatoryArgs.Count != 3)
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
                Info.PdfName = mandatoryArgs[2];
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
                        string str = "\r\nFePrint: could not open file: " + arg;
                        Log2.e(str);
                        Console.Write(str);
                        Application.ExitQuietly(99);
                    }

                }
                else
                {
                    string str = "\r\n\r\nFePrint: invalid option: " + arg;
                    Log2.e(str);
                    Console.Write(str);
                    WriteUsageToConsole();
                    Application.ExitQuietly(120);
                }
            }
        }




    }
}
