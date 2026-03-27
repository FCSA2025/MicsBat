using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program inputs one or more KML-formatted text files, as provided by the Government of Canada, and parses them to 
/// find ISED Tier 5 Area polygon definitions and then inserts this geomatic 
/// data as records in a SQL Server table using MultiPolygon object definitions.
/// </summary>
/// <remarks>
/// Excellent KML reference:  https://developers.google.com/kml/documentation/kmlreference
/// 
/// The command-line usage is:
/// \image html "Usage - T5aPolygons.PNG" ""
/// </remarks>
namespace T5aPolygons
{
    using System.IO;
    using System.Text.RegularExpressions;
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
    /// This class provides the Main() method for this application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class T5aPolygons
    {
        public static string mDbTableName = "";
        public static string mKMLfilesDirectoryPath = "";
        public static string mFileNameWithWildCards = "";
        public static string[] mKMLfilePaths = null;

        /// <summary>
        /// This class provides the main method for the T5aPolygons application.
        /// </summary>
        /// <param name="args"> - command-line arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                // Enable or disable developmental run-time logging.
#if false
                string mLog2FilePath = @"d:\MicsBatchLog\T5aPolygons.log";
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
                int rc = 0;

                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);

                List<Tier5Area> allTier5Areas = new List<Tier5Area>();

                bool isSuccessful;
                SQLHDBC hConn = SQLHDBC.Zero;

                // Parse the ISED KML files and get a list of all the Tier5Area objects.
                Console.Write("\n\nImporting ISED Tier 5 Area data from the following file(s):\n");
                int n = 1;
                foreach (string filePath in mKMLfilePaths)
                {
                    Console.Write("\n     {0,2}    {1}", n++, filePath);
                }

                isSuccessful = ISEDkmlFiles.GetTier5Areas(mKMLfilePaths, out allTier5Areas);

                if (!isSuccessful)
                {
                    Console.Error.Write("\nERROR: call to ISEDkmlFiles.GetTier5Areas() failed.");
                    Application.ExitQuietly(666);
                }

                Console.Write("\n\nNumber of Tier 5 Area Polygons imported: {0}", allTier5Areas.Count);

                foreach (Tier5Area tier5Area in allTier5Areas)
                {
                    if (tier5Area.GeoBoundaries.Count > 1)
                    {
                        Console.Write("\n\nNote: Tier 5 Area {0} has {1} polygons.", tier5Area.ID, tier5Area.GeoBoundaries.Count);
                    }
                }

                // Identify any anomalies with the KML polygon data and mitigate
                // if possible.
                string anomalies = IdentifyAnomalies(allTier5Areas);

                if (!String.IsNullOrWhiteSpace(anomalies))
                {
                    Console.Write("\n\n{0}", anomalies);

                    // Fix any duplicate Tier 5 IDs.
                    int instances = FixReplicatedAreaIDs(allTier5Areas, out allTier5Areas);
                }

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                // Unlike most other MICS programs, the user's project charge code is not 
                // passed in through the command-line; instead we need to get it from the
                // WindowsShell environment variable MICS_PROJECT.
                string pc;
                GenUtil.GetProjectCode(out pc);
                Info.ProjectCode = pc;

                // Establish an FCSA user session with the database.
                rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                    Application.ExitQuietly(11);
                }

                // Create and populate a table in the database with user-prescribed
                // schema and table name.

                Console.Write("\n\nOutput to SQL DB table: {0}", mDbTableName);

                int retVal = PolygonsTable.CreateTable(mDbTableName);

                if (retVal != Constant.SUCCESS)
                {
                    Console.Error.Write("\n\nT5aPolygons.Main(): ERROR: call to PolygonsTable.CreateTable() returned {0}.", retVal);
                    Application.ExitQuietly(12);
                }
                else
                {
                    Console.Write("\n\n     - Table creation:   SUCCEEDED");
                    Console.Write("\n     - Record insertion: ");
                }

                retVal = PolygonsTable.InsertRecords(allTier5Areas, mDbTableName);

                if (retVal != Constant.SUCCESS)
                {
                    Console.Error.Write("\n\nT5aPolygons.Main(): ERROR: call to PolygonsTable.InsertRecords() returned {0}.", retVal);
                    Application.ExitQuietly(13);
                }
                else
                {
                    Console.Write("SUCCEEDED");
                }

#if false
                // Just for entertainment.
                GeoPoint geoPoint = new GeoPoint(50.03111111, -125.36638888);

                string t5aID = Tier5Area.LookUpTier5AreaID(allTier5Areas, geoPoint);

                Console.Write("\n\nQuick Test:");
                Console.Write("\n     - Tier 5 Area ID lookup for location {0}:   {1}", geoPoint.ToString(), t5aID);
#endif
                Console.Write("\n");

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                Application.ExitQuietly(0);

            }
            catch (Exception e)
            {
                Log2.e("\r\r\n\nT5aPolygons.Main(): exception caught: " + e.Message);
                Log2.e("\r\r\n\nT5aPolygons.Main(): stack trace: \r\r\n\n" + e.StackTrace);
                BiUtil.BiBillingRec(Constant.BI_END, "");
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
                Log2.e("\r\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
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
            Console.Write("\r\n");
            Console.Write("\r\n This program inputs one or more KML-formatted text files, parses them to   +");
            Console.Write("\r\n find ISED Tier 5 Area polygon definitions and then inserts this geomatic   +"); 
            Console.Write("\r\n data as records in a SQL Server table using MultiPolygon object definitions.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: T5aPolygons <dbName> <projCode> <tableName> <directory> <KMLfileName>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        tableName        : prescribed SQL table name as [schema].[name]");
            Console.Write("\r\n        directory        : prescribed folder containing the KML files.");
            Console.Write("\r\n        KMLfileName      : prescribed name of the KML file. Multiple names");
            Console.Write("\r\n                           can be defined using '*' and '?' wildcards.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     T5aPolygons fcsa hulme1_0 hulme.nfm_t5a_polygons D:\\_Data *.kml");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 5 command-line arguments.");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length == 0)
            {
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
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

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 5 of them.
            if (regularArgs.Count != 5)
            {
                Console.Write("\r\n ERROR: Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Parse the args.
            Info.DbName = regularArgs[0];
            Info.ProjectCode = regularArgs[1];
            string schema_tableName = regularArgs[2];
            mKMLfilesDirectoryPath = regularArgs[3];
            mFileNameWithWildCards = regularArgs[4];

            // Check that schema_tableName is a valid (short) SQL Server table name. 
            const string template = @"\w+\.\w+";
            if (!Regex.IsMatch(schema_tableName, template))
            {
                Console.Write("\r\n ERROR: Invalid SQL table identifier: must have the form [schema].[tableName]\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Construct the fully qualified SQL table name identifier.
            mDbTableName = Info.DbName + "." + schema_tableName;

            // Check if mKMLfilesDirectoryPath is a valid directory path.
            if (!Directory.Exists(mKMLfilesDirectoryPath))
            {
                Console.Write("\r\n ERROR: Directory path is invalid or does not exist: {0}\r\n", mKMLfilesDirectoryPath);
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            // Check that there is at least one KML file in the prescribed directory 
            // that matches the prescribed filename template. The file name can be
            // a combination of valid literal path and wildcard (* and ?) characters, 
            // but it doesn't support regular expressions.
            mKMLfilePaths = Directory.EnumerateFiles(mKMLfilesDirectoryPath, mFileNameWithWildCards).ToArray();

            if ((mKMLfilePaths == null) || (mKMLfilePaths.Length < 1))
            {
                Console.Write("\r\n ERROR: No files found in prescribed directory that match: {0}\r\n", mFileNameWithWildCards);
                WriteUsageToConsole();
                Application.ExitQuietly(Error.COMMAND_LINE_ERROR);
            }

            //...Log2.v("\nT5aPolygons:" + Info.ToString());
            //...Log2.v("\n");
            //...Log2.v("\nmDbTableName           : " + mDbTableName);
            //...Log2.v("\nmKMLfilesDirectoryPath : " + mKMLfilesDirectoryPath);
            //...Log2.v("\nmFileNameWithWildCards : " + mFileNameWithWildCards);
        }

        /// <summary>
        /// This method identifies any replicated Tier 5 Area IDs found
        /// in the input KML files.
        /// </summary>
        /// <param name="allTier5Areas"></param>
        /// <returns></returns>
        private static string IdentifyAnomalies(List<Tier5Area> allTier5Areas)
        {
            string str = "KML Polygon Anomalies:\n";

            // Identify and count boundaries that have the same (replicated) Tier 5 Area ID number.
            bool noAnomalies = true;
            for (int i = 0; i < allTier5Areas.Count; i++)
            {
                Tier5Area tier5Area = allTier5Areas[i];
                int hitCount = 0;

                // Avoid double counting by starting at j = i.
                for (int j = i; j < allTier5Areas.Count; j++)
                {
                    Tier5Area t5A = allTier5Areas[j];
                    if (tier5Area.ID == t5A.ID) hitCount++;
                }

                if (hitCount > 1)
                {
                    str += String.Format("\n     - Tier5Area ID {0} has {1} instances.", tier5Area.ID, hitCount);
                    noAnomalies = false;
                }
            }

            if (noAnomalies) str += String.Format("\n     - None.");

            return str;
        }

        /// <summary>
        /// This method provides a 'fix' for all instances of replicated Tier 5 Area IDs
        /// by appending a unique digit to the ID string.
        /// </summary>
        /// <param name="allTier5Areas"></param>
        /// <param name="fixedTier5Areas"></param>
        /// <returns></returns>
        private static int FixReplicatedAreaIDs(List<Tier5Area> allTier5Areas, out List<Tier5Area> fixedTier5Areas)
        {
            // 'out' requirement.
            fixedTier5Areas = new List<Tier5Area>();

            int instance = 0;
            string fixedID = "";

            Console.Write("\n\nAnomalies Fixed:\n");

            bool noAnomaliesFixed = true;

            // Identify and count boundaries that have the same (duplicated) Tier 5 Area ID number.
            foreach (Tier5Area tier5Area in allTier5Areas)
            {
                int hitCount = 0;

                foreach (Tier5Area t5A in allTier5Areas)
                {
                    if (tier5Area.ID == t5A.ID)
                    {
                        hitCount++;
                    }
                }

                Tier5Area fixedT5A;

                if (hitCount > 1)
                {
                    noAnomaliesFixed = false;

                    instance++;

                    if (instance > 9)
                    {
                        Console.Error.Write("\n\nNFM_T5A_Polygons.FixDuplicateAreaIDs(): ERROR: instance = " + instance);
                        Application.ExitQuietly(666);
                    }

                    fixedID = String.Format("{0}-{1,1}", tier5Area.ID, instance);

                    fixedT5A = new Tier5Area(fixedID, tier5Area.BaseRateCode, tier5Area.GeoBoundaries);

                    Console.Write("\n     - Replicated Area ID {0} changed to {1}", tier5Area.ID, fixedID);
                }
                else
                {
                    instance = 0;

                    fixedT5A = new Tier5Area(tier5Area.ID, tier5Area.BaseRateCode, tier5Area.GeoBoundaries);
                }

                fixedTier5Areas.Add(fixedT5A);
            }

            if (noAnomaliesFixed) Console.Write("\n     - None.");

            return instance;
        }


    }
}

