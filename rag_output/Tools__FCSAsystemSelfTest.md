# Documented File: FCSAsystemSelfTest.cs
**Repository Path:** `Tools\FCSAsystemSelfTest.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class FCSAsystemSelfTest
    {
        public const string FORMAT_FAIL = "\nTest : {0, -32} : {1, -14} : {2, -32} : FAIL {3}";
        public const string FORMAT_PASS = "\nTest : {0, -32} : {1, -14} : {2, -32} : PASS {3}";

        public static void Go(string[] args)
        {
            try
            {
                int passCount = 0;
                int failCount = 0;

                // d:\dted50 
                Banner(@"d:\dted50");
                DirExistsWithUserRights(@"d:\dted50", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\dted50\data", FileSystemRights.Read, ref passCount, ref failCount);
                DirHasNumSubDirs(@"d:\dted50", 91, ref passCount, ref failCount);
                DirHasNumTreeFiles(@"d:\dted50", 26618, ref passCount, ref failCount);
                DirHasSizeBytes(@"d:\dted50", 77175111244, ref passCount, ref failCount);

                // d:\dted250
                Banner(@"d:\dted250");
                DirExistsWithUserRights(@"d:\dted250", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\dted250\data", FileSystemRights.Read, ref passCount, ref failCount);
                DirHasNumSubDirs(@"d:\dted250", 103, ref passCount, ref failCount);
                DirHasNumTreeFiles(@"d:\dted250", 2521, ref passCount, ref failCount);
                DirHasSizeBytes(@"d:\dted250", 6143512642, ref passCount, ref failCount);

                // d:\perflogs
                Banner(@"d:\perflogs");
                DirExistsWithUserRights(@"d:\perflogs", FileSystemRights.Write, ref passCount, ref failCount);

                // d:\inetpub
                Banner(@"d:\inetpub");
                DirExistsWithUserRights(@"d:\inetpub", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\inetpub\micsprod", FileSystemRights.Read, ref passCount, ref failCount);

                // d:\prod
                Banner(@"d:\prod");
                DirExistsWithUserRights(@"d:\prod", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\bin", FileSystemRights.ReadAndExecute, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files", FileSystemRights.Read, ref passCount, ref failCount);

                // d:\prod\bin
                Banner(@"d:\prod\bin");
                foreach (string progName in AllProdBinProgNames)
                {
                    string filePath = Path.Combine(@"d:\prod\bin", progName);
                    FileExistsWithUserRights(filePath, FileSystemRights.ReadAndExecute, ref passCount, ref failCount);
                }

                // d:\prod\files
                Banner(@"d:\prod\files");
                DirExistsWithUserRights(@"d:\prod\files\read", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit", FileSystemRights.Read, ref passCount, ref failCount);
                FileExistsWithUserRights(@"D:\prod\files\tsiplog.log", FileSystemRights.Write, ref passCount, ref failCount);
                FileExistsWithUserRights(@"D:\prod\files\ntv2_0.bin", FileSystemRights.Read, ref passCount, ref failCount);
                FileExistsWithUserRights(@"D:\prod\files\ntv2_0.ind", FileSystemRights.Read, ref passCount, ref failCount);

                // d:\prod\files\audit
                Banner(@"d:\prod\files\audit");
                DirExistsWithUserRights(@"d:\prod\files\audit\ante", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\band", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\ctx", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\eqpt", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\es", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\FCC", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\FCSA coord", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\note", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\oper", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\plan", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\rout", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\TAFL", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\town", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\towr", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\traf", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit\ts", FileSystemRights.Read, ref passCount, ref failCount);

                Console.Write("\n\npassCount = {0}, failCount = {1}", passCount, failCount);
            }
            catch (Exception e)
            {
                Console.Write("\n\nGo(): ERROR: exception caught: {0}\n{1}", e.Message, e.StackTrace);
            }

        }

        public static void Banner(string message)
        {
            Console.Write("\n\n              =================== {0} ===================", message);
        }

        public static bool DirExistsWithUserRights(string dirPath, FileSystemRights fileSystemRights, ref int passCount, ref int failCount)
        {
            string testReportLine = "";

            bool result = false;
            string reason = "";

            // Determine whether the prescribed directory exists, or not.
            if (!Directory.Exists(dirPath))
            {
                reason = "(Directory does not exist)";
                testReportLine = String.Format(FORMAT_FAIL, "Directory Exists With Rights?", fileSystemRights, dirPath, reason);

                failCount++;
            }
            else
            {
                // The directory exists; now check the current user's access rights.

                // The DirectoryInfo object for the prescribed file directory path provides properties and
                // instance methods for creating, moving, and enumerating through directories and subdirectories,
                // including methods that provide the access control and audit security for the directory.
                DirectoryInfo dInfo = new DirectoryInfo(dirPath);

                // The DirectorySecurity object provided by the DirectoryInfo object represents the access control
                // and audit security for the directory.
                DirectorySecurity sac = dInfo.GetAccessControl();

                // Now get the collection of AuthorizationRule objects associated with the DirectorySecurity object that
                // determines the access to securable objects.
                AuthorizationRuleCollection authRules = sac.GetAccessRules(true, true, typeof(NTAccount));

                FileSystemRights fsRights = 0;

                foreach (AuthorizationRule rule in authRules)
                {
                    // FileSystemAccessRule objects determine the access to securable objects.
                    // We get this object by converting the AuthorizationRule object to type FileSystemAccessRule.
                    FileSystemAccessRule fsRule = rule as FileSystemAccessRule;

                    if (fsRule != null)
                    {
                        // An NTAccount object represents a Windows user or group account which we get from
                        // the FileSystemAccessRule object.
                        NTAccount ntAccount = rule.IdentityReference as NTAccount;

                        // We are only interested in the rights of the User Group members.
                        if ((ntAccount != null) && (ntAccount.Value == @"BUILTIN\Users"))
                        {
                            //Console.Write("\n      ntAccount = {0}", ntAccount.ToString());
                            fsRights = fsRights | fsRule.FileSystemRights;
                        }
                    }
                }

                // Determine whether the prescribed directory has the required access rights.
                if (fsRights != 0)
                {
                    if ((fsRights & fileSystemRights) == fileSystemRights) result = true;
                }

                // Report the test results.
                if (result == true)
                {
                    reason = "";
                    testReportLine = String.Format(FORMAT_PASS, "Directory Exists With Rights?", fileSystemRights, dirPath, reason);
                    passCount++;
                    result = true;
                }
                else
                {
                    reason = "(Insufficient access rights)";
                    testReportLine = String.Format(FORMAT_FAIL, "Directory Exists With Rights?", fileSystemRights, dirPath, reason);

                    failCount++;
                }
            }

            Console.Write("\n{0}", testReportLine);

            return result;
        }

        public static bool DirHasNumSubDirs(string dirPath, int numSubDirs, ref int passCount, ref int failCount)
        {
            bool result = false;

            string testType = "Number of subdirectories?";
            string testReportLine = "";
            string reason = "";

            if (String.IsNullOrWhiteSpace(dirPath))
            {
                reason = "(Test path is NULL)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numSubDirs, dirPath, reason);
                failCount++;
            }
            else if (!(File.Exists(dirPath) || Directory.Exists(dirPath)))
            {
                reason = "(Test path does not exist)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numSubDirs, dirPath, reason);
                failCount++;
            }
            else if (!Directory.Exists(dirPath))
            {
                reason = "(Test path is not a directory)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numSubDirs, dirPath, reason);
                failCount++;
            }
            else
            {
                int actualNumSubDirs = Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories).Count();

                if (actualNumSubDirs == numSubDirs)
                {
                    reason = "";
                    testReportLine = String.Format(FORMAT_PASS, testType, numSubDirs, dirPath, reason);
                    passCount++;
                }
                else
                {
                    reason = "(count is different)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, numSubDirs, dirPath, reason);
                    failCount++;
                }
            }

            Console.Write("\n{0}", testReportLine);

            return result;
        }

        public static bool DirHasNumTreeFiles(string dirPath, int numTreeFiles, ref int passCount, ref int failCount)
        {
            bool result = false;

            string testType = "Number of files in tree?";
            string testReportLine = "";
            string reason = "";

            if (String.IsNullOrWhiteSpace(dirPath))
            {
                reason = "(Test path is NULL)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numTreeFiles, dirPath, reason);
                failCount++;
            }
            else if (!(File.Exists(dirPath) || Directory.Exists(dirPath)))
            {
                reason = "(Test path does not exist)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numTreeFiles, dirPath, reason);
                failCount++;
            }
            else if (!Directory.Exists(dirPath))
            {
                reason = "(Test path is not a directory)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numTreeFiles, dirPath, reason);
                failCount++;
            }
            else
            {
                int actualNumTreeFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories).Count();

                if (actualNumTreeFiles == numTreeFiles)
                {
                    reason = "";
                    testReportLine = String.Format(FORMAT_PASS, testType, numTreeFiles, dirPath, reason);
                    passCount++;
                }
                else
                {
                    reason = "(count is different)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, numTreeFiles, dirPath, reason);
                    failCount++;
                }
            }

            Console.Write("\n{0}", testReportLine);

            return result;
        }

        public static bool DirHasSizeBytes(string dirPath, long numDirSizeBytes, ref int passCount, ref int failCount)
        {
            bool result = false;

            string testType = "Directory size in bytes?";
            string testReportLine = "";
            string reason = "";

            if (String.IsNullOrWhiteSpace(dirPath))
            {
                reason = "(Test path is NULL)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numDirSizeBytes, dirPath, reason);
                failCount++;
            }
            else if (!(File.Exists(dirPath) || Directory.Exists(dirPath)))
            {
                reason = "(Test path does not exist)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numDirSizeBytes, dirPath, reason);
                failCount++;
            }
            else if (!Directory.Exists(dirPath))
            {
                reason = "(Test path is not a directory)";
                testReportLine = String.Format(FORMAT_FAIL, testType, numDirSizeBytes, dirPath, reason);
                failCount++;
            }
            else
            {
                long actualDirSizeBytes = DirectorySizeBytes(new DirectoryInfo(dirPath));

                if (actualDirSizeBytes == numDirSizeBytes)
                {
                    reason = "";
                    testReportLine = String.Format(FORMAT_PASS, testType, numDirSizeBytes, dirPath, reason);
                    passCount++;
                }
                else
                {
                    reason = "(count is different)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, numDirSizeBytes, dirPath, reason);
                    failCount++;
                }
            }

            Console.Write("\n{0}", testReportLine);

            return result;
        }

        public static long DirectorySizeBytes(DirectoryInfo d)
        {
            long size = 0;

            // Accumulate file sizes in this directory.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            // Accumulate file sizes in all subdirectories using recursive call.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirectorySizeBytes(di);
            }
            return size;
        }

        public static bool FileExistsWithUserRights(string filePath, FileSystemRights reqdFileSystemRights, ref int passCount, ref int failCount)
        {
            string testReportLine = "";

            bool result = false;
            string testType = "File exists with rights?";
            string reason = "";

            if (!File.Exists(filePath))
            {
                reason = @"(File not exist)";
                testReportLine = String.Format(FORMAT_FAIL, testType, reqdFileSystemRights, filePath, reason);
                failCount++;
            }
            else
            {
                // The file exists; now check the current user's access rights.

                // The FileInfo object for the prescribed file path provides properties and instance methods
                // for the creation, copying, deletion, moving, and opening of files, including methods that
                // provide the access control and audit security for the file.
                FileInfo fInfo = new FileInfo(filePath);

                // The FileSecurity object provided by the FileInfo object represents the access control
                // and audit security for the file.
                FileSecurity sac = fInfo.GetAccessControl();

                // Now get the collection of AuthorizationRule objects associated with the FileSecurity object that
                // determines the access to securable objects.
                AuthorizationRuleCollection authRules = sac.GetAccessRules(true, true, typeof(NTAccount));

                FileSystemRights fsRights = 0;

                foreach (AuthorizationRule rule in authRules)
                {
                    // FileSystemAccessRule objects determine the access to securable objects.
                    // We get this object by converting the AuthorizationRule object to type FileSystemAccessRule.
                    FileSystemAccessRule fsRule = rule as FileSystemAccessRule;

                    if (fsRule != null)
                    {
                        // An NTAccount object represents a Windows user or group account which we get from
                        // the FileSystemAccessRule object.
                        NTAccount ntAccount = rule.IdentityReference as NTAccount;

                        // We are only interested in the rights of the User Group members.
                        if ((ntAccount != null) && (ntAccount.Value == @"BUILTIN\Users"))
                        {
                            fsRights = fsRights | fsRule.FileSystemRights;
                        }
                    }
                }

                // Determine whether the prescribed file has the required access rights.
                if ((fsRights & reqdFileSystemRights) == reqdFileSystemRights)
                {
                    reason = "";
                    testReportLine = String.Format(FORMAT_PASS, testType, reqdFileSystemRights, filePath, reason);
                    passCount++;
                }
                else
                {
                    reason = "(Insufficient access rights)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, reqdFileSystemRights, filePath, reason);
                    failCount++;
                }
            }

            Console.Write("\n{0}", testReportLine);

            return result;
        }


        public static List<string> AllProdBinProgNames = new List<string>()
        {
            "_Auxlib.dll",
            "_Configuration.dll",
            "_DataStructures.dll",
            "_NewLib.dll",
            "_OHloss.dll",
            "_Utillib.dll",
            "Chg_Mbr_Pass.exe",
            "CopyTable.exe",
            "CTE.dll",
            "DailyStorage.exe",
            "DBAccess.dll",
            "esImport.exe",
            "esPrint.exe",
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
            "KillTable.exe",
            "meUpdate.exe",
            "Microsoft.SqlServer.Dts.Design.dll",
            "Microsoft.SQLServer.ManagedDTS.dll",
            "Microsoft.SqlServer.PipelineHost.dll",
            "msvcr100d.dll",
            "MtUpdate.exe",
            "pcnscan.exe",
            "pfdcont.exe",
            "pl5Import.exe",
            "sdfImport.exe",
            "sdfPrint.exe",
            "sdfValidate.exe",
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
            "SQLtoFlat.exe",
            "TpRunTsip.exe",
            "TsipInitiator.exe",
            "TsipQdelete.exe",
            "Vch.exe",
            "wOrbit.exe",
            "Wpassive.exe",
            "writegate.exe",
            "WsatAze.exe",

        };

    }
}

```
