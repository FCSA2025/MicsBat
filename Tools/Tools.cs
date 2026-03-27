using _Auxlib;
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
/// This program provides automated command-line testing of the following MICS# applications: 
/// FeImport, FeValidate, FtImport, FtValidate, TpRunTsip and TsipInitiator; it enables two different
/// versions of a MICS executable to be run using the same input data sets and the output results
/// can be checked for similarities and differences using (say) Winmerge; examples of different
/// versions are the legacy C/C++ executable, a previous release of the C# version and the current
/// C# version; it also provides a framework from which to call a repertoir of sundry test programs
/// and utilities without needing a separate VS Project for each one.
/// </summary>
namespace Tools
{
    using SQLRETURN = Int16;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that perform automated command-line testing of the following MICS# applications: 
    /// FeImport, FeValidate, FtImport, FtValidate, TpRunTsip and TsipInitiator; it enables two different
    /// versions of a MICS executable to be run using the same input data sets and the output results
    /// can be checked for similarities and differences using (say) Winmerge; examples of different
    /// versions are the legacy C/C++ executable, a previous release of the C# version and the current
    /// C# version.
    /// </summary>
    public class Tools
    {
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                //TAFL tafl = new TAFL();
                //SQLLEN[] nullInds = NullHelper.CreateArrayOfNullInd(TAFL.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL);
                //nullInds[TAFL.FREQUENCYMHZ] = Constant.DB_NULL;

                //Console.Write("\n{0}", tafl.ToStringAsCSVexport(nullInds));
                //Application.ExitQuietly(0);
#if false
                string mLog2FilePath = @"d:\MicsBatchLog\Tools.log";
                if (Log2.SetLogFilePath(mLog2FilePath))
                {
                    Log2.Erase();
                    Log2.Set(Log2.FileOpenClose.PER_SESSION);
                    Log2.Set(Log2.WriteMode.ENABLED);
                    Log2.Set(Log2.Level.VERBOSE);
                    Info.BuildMetaData = Info.CollateExeMetaData();
                    Log2.v("\nBuild: " + Info.ManagedBuildInfo());
                }
#endif
                if (args.Length == 0)
                {
                    WriteCommandLineUsage();
                    Application.ExitQuietly(666);
                }
                else
                {
                    // To get here, there must be at least one command-line argument.
                    // Verify that the first argument is the name of a MICS programs 
                    // and process accordingly.

                    string micsProgName = args[0].ToLower();

                    string[] remainingArgs = new string[args.Length - 1];

                    for (int i = 0; i < remainingArgs.Length; i++)
                    {
                        remainingArgs[i] = args[i + 1].ToLower();
                        //...Log2.v(String.Format("\nTools.Main(): remainingArgs[{0}] = {1}", i, remainingArgs[i]));
                    }

                    switch (micsProgName)
                    {
                        case "blockdbreadwrite":
                            BlockDbReadWrite.Go(remainingArgs);
                            break;
                        case "bndcdeoverlaps":
                            BndcdeOverlaps.Go(remainingArgs);
                            break;
                        case "callsignsfrompdf":
                            CallSignsFromPDF.Go(remainingArgs);
                            break;
                        case "canusaborderkml":
                            CanUsaBorderKML.Go(remainingArgs);
                            break;
                        case "cleantemp":
                            CleanTemp.Go(remainingArgs);
                            break;
                        case "convertlatlng":
                            ConvertLatLng.Go(remainingArgs);
                            break;
                        case "createtabletafllicenseenametofcsaoper":
                            CreateTableTaflLicenseeNameToFcsaOper.Go(remainingArgs);
                            break;
                        case "directoryconfig":
                            DirectoryConfig.Go(remainingArgs);
                            break;
                        case "fcsasystemselftest":
                            FCSAsystemSelfTest.Go(remainingArgs);
                            break;
                        case "feimport":
                            TestFeImport.Test_FeImport(remainingArgs);
                            break;
                        case "fevalidate":
                            TestFeValidate.Test_FeValidate(remainingArgs);
                            break;
                        case "filecharsscan":
                            FileCharsScan.Go(remainingArgs);
                            break;
                        case "fileattributes":
                            FileAttributesReport.Go(remainingArgs);
                            break;
                        case "ftimport":
                            TestFtImport.Test_FtImport(remainingArgs);
                            break;
                        case "ftprint":
                            TestFtPrint.Test_FtPrint(remainingArgs);
                            break;
                        case "ftvalidate":
                            TestFtValidate.Test_FtValidate(remainingArgs);
                            break;
                        case "getmicsbuildinfo":
                            GetMicsBuildInfo.Go(remainingArgs);
                            break;
                        case "getusercontext":
                            GetUserContext.Go(remainingArgs);
                            break;
                        case "googlemaps":
                            GoogleMaps.Go(remainingArgs);
                            break;
                        case "hilocheck":
                            HiLoCheck.Go(remainingArgs);
                            break;
                        case "kml":
                            KML.Go(remainingArgs);
                            break;
                        case "micserrormessagedecoder":
                            MicsErrorMessageDecoder.Go(remainingArgs);
                            break;
                        case "micssolnsourcecodehashes":
                            MicsSolnSourceCodeHashes.Go(remainingArgs);
                            break;
                        case "parsectfa":
                            ParseCTFA.Go(remainingArgs);
                            break;
                        case "populateregressiondb":
                            PopulateRegressionDB.Go(remainingArgs);
                            break;
                        case "prodbincompare":
                            ProdBinCompare.Go(remainingArgs);
                            break;
                        case "prodbinconfiguration":
                            ProdBinConfiguration.Go(remainingArgs);
                            break;
                        case "registrykey":
                            RegistryKey.Go(remainingArgs);
                            break;
                        case "sandbox":
                            SandBox.Go(remainingArgs);
                            break;
                        case "scrapectfa":
                            ScrapeCTFA.Go(remainingArgs);
                            break;
                        case "spooftablesforftvalidate":
                            SpoofTablesForFtValidate.Go();
                            break;
                        case "sqlmissingtablestocsvfiles":
                            SQLmissingTablesToCSVFiles.Go(remainingArgs);
                            break;
                        case "sqlphantomtablestocsvfiles":
                            SQLphantomTablesToCSVFiles.Go(remainingArgs);
                            break;
                        case "testdatabaseconnection":
                            TestDatabaseConnection.Go(remainingArgs);
                            break;
                        case "testfeimport":
                            TestFeImport.Test_FeImport(remainingArgs);
                            break;
                        case "tpruntsip":
                            TestTpRunTsip.Test_TpRunTsip(remainingArgs);
                            break;
                        case "tsipinitiator":
                            TestTsipInitiator.Test_TsipInitiator(remainingArgs);
                            break;

                        default:
                            Console.WriteLine("\n1st. command-line argument is not a valid mode.\n");
                            WriteCommandLineUsage();
                            Application.ExitQuietly(666);
                            break;

                    }
                }


            }
            catch (Exception e)
            {
                Log2.e("\n\nTools.Main(): ERROR: exception: " + e.Message);
                Log2.e("\n\nTools.Main(): ERROR: stack trace: \n" + e.StackTrace);
            }

        } // main() method

        private static void WriteCommandLineUsage()
        {
            Console.WriteLine("\n\nUsage:  Tools <mode> [p1] [p2] [p3] ....");

            List<string> modes = new List<string>()
            {
                    "BlockDbReadWrite",
                    "BndcdeOverlaps",
                    "CallSignsFromPDF",
                    "CanUsaBorderKML",
                    "CleanTemp",
                    "ConvertLatLng",
                    "CreateTableTaflLicenseeNameToFcsaOper",
                    "ConvertLatLng",
                    "DirectoryConfig",
                    "FCSAsystemSelfTest",
                    "FeValidate",
                    "FileAttributes",
                    "FileCharsScan",
                    "FtImport",
                    "FtPrint",
                    "FtValidate",
                    "GetMicsBuildInfo",
                    "GetUserContext",
                    "GoogleMaps",
                    "HiLoCheck",
                    "KML",
                    "MicsErrorMessageDecoder",
                    "MicsSolnSourceCodeHashes",
                    "ParseCTFA",
                    "remainingArgs",
                    "PopulateRegressionDB",
                    "ProdBinCompare",
                    "ProdBinConfiguration",
                    "RegistryKey",
                    "ScrapeCTFA",
                    "SQLmissingTablesToCSVFiles",
                    "SQLphantomTablesToCSVFiles",
                    "SandBox",
                    "SpoofTablesForFtValidate",
                    "TestDatabaseConnection",
                    "TestFeImport",
                    "TestMicsEmail",
                    "TpRunTsip",
                    "tsipinitiator"
            };

            Console.WriteLine("\n\nSupported modes:");

            foreach (string mode in modes) Console.Write("\n                  - {0}", mode);
        }


    } // class
} // namespace
