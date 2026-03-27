using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    using _DataStructures;
    public class ProdBinConfiguration
    {
        private static string[] mProdBinHulme = new string[44]
{
"_Auxlib.dll",
"_Configuration.dll",
"_DataStructures.dll",
"_NewLib.dll",
"_OHloss.dll",
"_Utillib.dll",
"CTE.dll",
"FeImport.exe",
"FePrint.exe",
"FeValidate.exe",
"FtImport.exe",
"FtPrint.exe",
"FtValidate.exe",
"GenCtx.exe",
"GetCoords.exe",
"GetOHLrep.exe",
"getprofrep.exe",
"HiLoCheck.exe",
"hold.exe",
"Interpol.exe",
"meUpdate.exe",
"MtUpdate.exe",
"pcnscan.exe",
"pfdcont.exe",
"sdUpdateAnte.exe",
"sdUpdateBand.exe",
"sdUpdateCtx.exe",
"sdUpdateEqpt.exe",
"sdUpdateNote.exe",
"sdUpdateOper.exe",
"sdUpdatePlan.exe",
"sdUpdateRout.exe",
"sdUpdateTown.exe",
"sdUpdateTowr.exe",
"sdUpdateTraf.exe",
"SetEmailPassword.exe",
"TpRunTsip.exe",
"TsipInitiator.exe",
"TsipQdelete.exe",
"Vch.exe",
"wOrbit.exe",
"Wpassive.exe",
"writegate.exe",
"WsatAze.exe"
};

        private static string[] mProdBinVenn = new string[16]
{
"Chg_Mbr_Pass.exe",
"CopyTable.exe",
"CopyTable.exe.config",
"DailyStorage.exe",
"DailyStorage.exe.config",
"DBAccess.dll",
"esImport.exe",
"esPrint.exe",
"KillTable.exe",
"KillTable.exe.config",
"pl5Import.exe",
"sdfImport.exe",
"sdfPrint.exe",
"sdfValidate.exe",
"SQLtoFlat.exe",
"SQLtoFlat.exe.config"
};
        private static string[] mProdBinMicrosoft = new string[4]
{
"Microsoft.SqlServer.Dts.Design.dll",
"Microsoft.SQLServer.ManagedDTS.dll",
"Microsoft.SqlServer.PipelineHost.dll",
"msvcr100d.dll"
};

        private static string[] mMICSexeNames = new string[36]
{
"FeImport.exe",
"FePrint.exe",
"FeValidate.exe",
"FtImport.exe",
"FtPrint.exe",
"FtValidate.exe",
"GenCtx.exe",
"GetCoords.exe",
"GetOHLrep.exe",
"getprofrep.exe",
"HiLoCheck.exe",
"hold.exe",
"Interpol.exe",
"meUpdate.exe",
"MtUpdate.exe",
"pcnscan.exe",
"pfdcont.exe",
"sdUpdateAnte.exe",
"sdUpdateBand.exe",
"sdUpdateCtx.exe",
"sdUpdateEqpt.exe",
"sdUpdateNote.exe",
"sdUpdateOper.exe",
"sdUpdatePlan.exe",
"sdUpdateRout.exe",
"sdUpdateTown.exe",
"sdUpdateTowr.exe",
"sdUpdateTraf.exe",
"TpRunTsip.exe",
"TsipInitiator.exe",
"TsipQdelete.exe",
"Vch.exe",
"wOrbit.exe",
"Wpassive.exe",
"writegate.exe",
"WsatAze.exe"
};

        //private const string WEB_6D_PROD_BIN_DIR = @"\\fcsaWEB6D\d$\prod\bin";
        //private const string WEB_6D_PROD_BIN_DIR = @"\\fcsaWEB6D\d$\prod\bin_20220920(configuration controlled)";
        //private const string WEB_3_PROD_BIN_DIR = @"\\fcsaWEB3\d$\prod\bin";
        private const string PROD_BIN_A = @"D:\prod\bin";
        private const string PROD_BIN_B = @"D:\MicsBatchLog\CloudMicsProd_Prod_Bin";
        //private const string PROD_BIN_CONFIG_REFERENCE = @"d:\prod\bin_20220920(configuration controlled)";
        //private const string KOZA_PROD_BIN = @"D:\MicsBatchLog\prod_bin_KOZA_production_server\bin";

        public static void Go(string[] args)
        {
            try
            {

                // Create a list of all required prod\bin programs and libraries.
                List<string> requiredFileNames = new List<string>();

                requiredFileNames.AddRange(mProdBinHulme);
                requiredFileNames.AddRange(mProdBinVenn);
                requiredFileNames.AddRange(mProdBinMicrosoft);

                List<string> allFileNamesInProdBin_A = GetAllFilePathsInDir(PROD_BIN_A);
                List<string> allFileNamesInProdBin_B = GetAllFilePathsInDir(PROD_BIN_B);

                AuditFileNameList(requiredFileNames, PROD_BIN_A, allFileNamesInProdBin_A);
                AuditFileNameList(requiredFileNames, PROD_BIN_B, allFileNamesInProdBin_B);

                List<ProdBinConfig> fileMetaDataProdBin_A = GetFileMetaData(PROD_BIN_A, requiredFileNames);
                List<ProdBinConfig> fileMetaDataProdBin_B = GetFileMetaData(PROD_BIN_B, requiredFileNames);

                //List<ProdBinConfig> differences = IdentifyNewerDifferences(requiredFileNames, CLOUDMICSPROD_PROD_BIN, fileMetaDataProdBin, CLOUDMICSDEV_PROD_BIN, fileMetaDataProdBinConfig);
                List<ProdBinConfig> differences = IdentifyNewerDifferences(requiredFileNames, PROD_BIN_A, fileMetaDataProdBin_A, PROD_BIN_B, fileMetaDataProdBin_B);

                MakeSQLinsertQueries(differences, "hulme.prod_bin_config", PROD_BIN_A);

            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nERROR: exception: {0}", e.Message);
                Console.Error.Write("\n\n{0}", e.StackTrace);
                Application.ExitQuietly(666);
            }
        }

        /// <summary>
        /// This method returns the MD5 hash string for the contents of a prescribed file path.
        /// </summary>
        /// <returns></returns>
        public static string GetMD5OfFile(string filePath)
        {
            try
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = File.ReadAllBytes(filePath);

                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }

            }
            catch (Exception)
            {
            }
            return "File not found.";
        }

        private static string GetCustodian(string requiredFileName)
        {
            string owner = "???";

            if (Strings.IsInList(mProdBinHulme.ToList(), requiredFileName)) return "HULME";

            if (Strings.IsInList(mProdBinVenn.ToList(), requiredFileName)) return "VENN";

            if (Strings.IsInList(mProdBinMicrosoft.ToList(), requiredFileName)) return "MICROSOFT";

            return owner;
        }

        private static string GetMicsProgramBuildInfo(string exePath)
        {
            string buildInfo = "";

            // Check that the exe file actually exists at the prescribed location.
            if (!File.Exists(exePath))
            {
                Console.Error.Write("\nERROR: File not found: {0}", exePath);
                return "";
            }

            string stdOut = "";
            string stdErr = "";
            int exitCode = 0;

            int retVal = WindowsShell.RunCommand(exePath, "", out stdOut, out stdErr, out exitCode);

            if (retVal != Constant.SUCCESS)
            {
                Console.Error.Write("\n\nERROR: call to WindowsShell.RunCommand() failed for exePath = {0}.\n", exePath);
                return "";
            }

            // Strip out the build info-tag from the stdOut text.
            // Build: 210329-1218/64-R
            string pattern = @"(\d\d\d\d\d\d-\d\d\d\d/\d\d-\w)";

            Match match = Regex.Match(stdOut.ToUpper(), pattern);

            if (match.Success)
            {
                buildInfo = match.Groups[1].Value.ToString();
            }
            else
            {
                Console.Error.Write("\nCould not get BuildInfo for MICS program: {0}", Path.GetFileName(exePath));
            }

            return buildInfo;
        }

        private static List<ProdBinConfig> GetFileMetaData(string prodBinPath, List<string> requiredFileNames)
        {
            List<ProdBinConfig> fileMetaDataList = new List<ProdBinConfig>();

            foreach (string requiredFileName in requiredFileNames)
            {
                string filePath = Path.Combine(prodBinPath, requiredFileName);

                if (File.Exists(filePath))
                {
                    ProdBinConfig fileMetaData = new ProdBinConfig(requiredFileName, GetMD5OfFile(filePath));

                    // If the filePath does not exist, these GetLastWriteTime() methods return
                    // 12:00 midnight, January 1, 1601 A.D. (C.E.) Coordinated Universal Time (UTC), 
                    // adjusted to local time.
                    fileMetaData.LastModified = File.GetLastWriteTime(filePath).ToString("g");
                    fileMetaData.SortDate = File.GetLastWriteTime(filePath).ToString("yyyyMMdd");
                    fileMetaData.Custodian = GetCustodian(requiredFileName);

                    fileMetaDataList.Add(fileMetaData);
                }
            }

            string header = String.Format("Meta data for required files in {0}", prodBinPath);
            Console.Write("\n\n{0}", header);
            Console.Write("\n{0}", Strings.RepeatedChar('=', header.Length));

            Console.Write("\n{0}", ProdBinConfig.CSVheader());
            foreach (ProdBinConfig fileMetaData in fileMetaDataList)
            {
                Console.Write("\n{0}", fileMetaData.ToString());
            }

            return fileMetaDataList;
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

        private static List<string> GetAllFilePathsInDir(string dirPath)
        {
            List<string> allFileNamesInDir = new List<string>();

            try
            {
                List<string> allFilePathsInDir = Directory.EnumerateFiles(dirPath).ToList();

                foreach (string filePath in allFilePathsInDir)
                {
                    allFileNamesInDir.Add(Path.GetFileName(filePath));
                }
            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nGetAllFilePathsInDir(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
            }

            return allFileNamesInDir;
        }

        private static void AuditFileNameList(List<string> requiredFileNames, string auditDir, List<string> fileNames)
        {
            string header = String.Format("Configuration Audit for {0}", auditDir);
            Console.Write("\n\n{0}", header);
            Console.Write("\n{0}", Strings.RepeatedChar('=', header.Length));

            Console.Write("\n\n    Required files that are missing:");

            int count = 0;
            foreach (string requiredFileName in requiredFileNames)
            {

                if (!Strings.IsInListCaseInsensitive(fileNames, requiredFileName))
                {
                    count++;
                    Console.Write("\n        {0}", requiredFileName);
                }

            }

            if (count == 0) Console.Write("\n        None.");
            Console.Write("\n    There are {0} missing files.\n", count);

            Console.Write("\n\n    Files that are not required:");

            count = 0;
            foreach (string path in fileNames)
            {
                string fileName = Path.GetFileName(path);

                if (!Strings.IsInListCaseInsensitive(requiredFileNames.ToList(), fileName))
                {
                    count++;
                    Console.Write("\n        {0}", Path.GetFileName(path));
                }

            }

            if (count == 0) Console.Write("\n        None.");
            Console.Write("\n    There are {0} files that are not required.\n", count);

            return;
        }

        private static List<ProdBinConfig> IdentifyNewerDifferences(List<string> requiredFileNames, string dirPathA, List<ProdBinConfig> pbcListA, string dirPathB, List<ProdBinConfig> pbcListB)
        {
            string header = String.Format("Comparison of required files in {0} versus {1}", dirPathA, dirPathB);
            Console.Write("\n\n{0}", header);
            Console.Write("\n{0}", Strings.RepeatedChar('=', header.Length));

            List<ProdBinConfig> differencesA = new List<ProdBinConfig>();

            foreach (string fileName in requiredFileNames)
            {
                ProdBinConfig fmd5D = ProdBinConfig.GetProdBinConfigByName(fileName, pbcListA);

                ProdBinConfig fmd3 = ProdBinConfig.GetProdBinConfigByName(fileName, pbcListB);

                string result = "";

                if ((fmd5D == null) && (fmd3 == null)) result = "==========> Not found in either directory";
                if (fmd5D == null) result = "==========> Not found in " + dirPathA;
                if (fmd3 == null) result = "==========> Not found in " + dirPathB;

                if ((fmd5D != null) && (fmd3 != null))
                {
                    if (fmd5D.MD5hash == fmd3.MD5hash)
                    {
                        result = "IDENTICAL";
                    }
                    else
                    {
                        result = "==========> DIFFERENT";

                        if (String.Compare(fmd5D.SortDate, fmd3.SortDate, true) > 0)
                        {
                            differencesA.Add(fmd5D);
                        }

                    }
                }

                Console.Write("\n{0,36} : {1}", fileName, result);
            }

            return differencesA;
        }

        private static void MakeSQLinsertQueries(List<ProdBinConfig> pbcList, string tableName, string dirPath)
        {
            string header = String.Format("T-SQL INSERT queries for differencies in {0}:", dirPath);
            Console.Write("\n\n{0}", header);
            Console.Write("\n{0}", Strings.RepeatedChar('=', header.Length));

            foreach (ProdBinConfig pbc in pbcList)
            {
                string query = String.Format("\nINSERT INTO {0} VALUES ( {1} )",  tableName, pbc.ToStringAsQuotedCSV());

                Console.Write(query);
            }
        }

    }
}
