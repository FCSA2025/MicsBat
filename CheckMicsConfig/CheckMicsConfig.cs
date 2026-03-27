using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CheckMicsConfig
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

    public class CheckMicsConfig
    {
        public const string FORMAT_FAIL = "\nTest : {0, -32} : {1, -14} : {2, -32} : FAIL {3}";
        public const string FORMAT_PASS = "\nTest : {0, -32} : {1, -14} : {2, -32} : PASS {3}";

        public static string mDbName = "";
        public static string mMicsUserName = "";
        public static string mPassword = "";
        public static string mGlobalSchema = "";
        public static string mUltrixID = "";

        public enum DbAccess { SELECT_FETCH, INSERT_UPDATE }

        public static void Main(string[] args)
        {
            try
            {
                // Parse, process and sanitize the command-line arguments.
                ParseCommandLineArgs(args);
#if false
                // d:\dted50 
                WriteBanner(@"Check that data files for '50-scale' OHLOSS calculations exist with required access rights for BUILTIN\Users.");
                DirExistsWithUserRights(@"d:\dted50", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\dted50\data", FileSystemRights.Read, ref passCount, ref failCount);
                DirHasNumSubDirs(@"d:\dted50", 91, ref passCount, ref failCount);
                DirHasNumTreeFiles(@"d:\dted50", 26618, ref passCount, ref failCount);
                DirHasSizeBytes(@"d:\dted50", 77175111244, ref passCount, ref failCount);

                // d:\dted250
                WriteBanner(@"Check that data files for '250-scale' OHLOSS calculations exist with required access rights for BUILTIN\Users.");
                DirExistsWithUserRights(@"d:\dted250", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\dted250\data", FileSystemRights.Read, ref passCount, ref failCount);
                DirHasNumSubDirs(@"d:\dted250", 103, ref passCount, ref failCount);
                DirHasNumTreeFiles(@"d:\dted250", 2521, ref passCount, ref failCount);
                DirHasSizeBytes(@"d:\dted250", 6143512642, ref passCount, ref failCount);

                // d:\perflogs
                WriteBanner(@"Check that all essential directories exist with required access rights for BUILTIN\Users.");
                DirExistsWithUserRights(@"d:\perflogs", FileSystemRights.Write, ref passCount, ref failCount);

                // d:\inetpub
                DirExistsWithUserRights(@"d:\inetpub", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\inetpub\" + mDbName, FileSystemRights.Read, ref passCount, ref failCount);

                // d:\prod
                DirExistsWithUserRights(@"d:\prod", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\bin", FileSystemRights.ReadAndExecute, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files", FileSystemRights.Read, ref passCount, ref failCount);

                DirExistsWithUserRights(@"d:\prod\files\read", FileSystemRights.Read, ref passCount, ref failCount);
                DirExistsWithUserRights(@"d:\prod\files\audit", FileSystemRights.Read, ref passCount, ref failCount);

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

                // d:\prod\bin
                WriteBanner(@"Check all essential .exe and .dll files under d:prod\bin exist with required access rights for BUILTIN\Users.");
                foreach (string progName in AllProdBinProgNames)
                {
                    string filePath = Path.Combine(@"d:\prod\bin", progName);
                    FileExistsWithUserRights(filePath, FileSystemRights.ReadAndExecute, ref passCount, ref failCount);
                }

                // d:\prod\files
                WriteBanner(@"Check that all essential files under d:prod\files exist with required access rights for BUILTIN\Users.");
                FileExistsWithUserRights(@"D:\prod\files\tsiplog.log", FileSystemRights.Write, ref passCount, ref failCount);
                FileExistsWithUserRights(@"D:\prod\files\ntv2_0.bin", FileSystemRights.Read, ref passCount, ref failCount);
                FileExistsWithUserRights(@"D:\prod\files\ntv2_0.ind", FileSystemRights.Read, ref passCount, ref failCount);

                //====================================================================================================

                WriteBanner(@"Check that a connection to the database " + mDbName + " can be established using ODBC.");

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect(ref passCount, ref failCount);

                // Establish an FCSA user session with the database.
                ODBCconnect.UtConnect(mDbName, ref passCount, ref failCount);

                WriteBanner("Check permissions that group ROLwebusers have on SQL tables they need to READ from.");

                TableExistsWithUserReadAccess(mDbName + ".main.mt_site", 50000, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.mt_ante", 80000, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.mt_chan", 100000, ref passCount, ref failCount);

                TableExistsWithUserReadAccess(mDbName + ".main.me_site", 1200, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.me_ante", 1500, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.me_azim", 3200, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.me_chan", 5000, ref passCount, ref failCount);

                TableExistsWithUserReadAccess(mDbName + ".main.sd_antd", 150000, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_ante", 3000, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_band", 50, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_ctx", 2100, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_ctxd", 130000, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_eqpt", 3500, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_note", 700, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_oper", 9200, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_plan", 120, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_plnd", 920, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_rout", 1400, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_town", 1600, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_towr", 15, ref passCount, ref failCount);
                TableExistsWithUserReadAccess(mDbName + ".main.sd_traf", 3400, ref passCount, ref failCount);

                TableExistsWithUserReadAccess(mDbName + ".adm.account_details", 150, ref passCount, ref failCount); 
                TableExistsWithUserReadAccess(mDbName + ".adm.account_ids", 40, ref passCount, ref failCount);

                WriteBanner("Check permissions that group ROLwebusers have on SQL tables they need to WRITE to.");

                TableExistsWithUserWriteAccess(mDbName + ".web.daily_usage", "ISU", ref passCount, ref failCount);
                TableExistsWithUserWriteAccess(mDbName + ".web.dblogger", "DISU", ref passCount, ref failCount);
                TableExistsWithUserWriteAccess(mDbName + ".web.tsip_queue", "DISU", ref passCount, ref failCount);
                TableExistsWithUserWriteAccess(mDbName + ".web.user_tables", "DISU", ref passCount, ref failCount);

                // Terminate the FCSA user's DB session.
                ODBCconnect.UtDisconnect();

                Console.Write("\n\npassCount = {0}, failCount = {1}", passCount, failCount);
#endif
                int exitCode;
                string stdOut;
                string stdErr;

                WindowsShell.RunCommand("hulme1", @"d:\prod\bin\FtImport.exe", @"-f micsdev hulme1_0 zulu D:\users\ahulme\MICS#\_Testing\TS\TS_MIN_PDF_TEMPLATE.txt",
                    out stdOut, out stdErr, out exitCode);

                Console.Write("\nExitCode = {0}", exitCode);
                Console.Write("\nstdOut = \n{0}", stdOut);
                Console.Write("\nstdErr = \n{0}", stdErr);

                WindowsShell.RunCommand("hulme1", @"d:\prod\bin\FtValidate.exe", @"micsdev hulme1_0 zulu ",
    out stdOut, out stdErr, out exitCode);

                Console.Write("\nExitCode = {0}", exitCode);
                Console.Write("\nstdOut = \n{0}", stdOut);
                Console.Write("\nstdErr = \n{0}", stdErr);

                WindowsShell.RunCommand("hulme1", @"D:\prod\bin\TpRunTsip.exe", @"micsdev hulme1_0 tstest0183 -oTSIP ",
out stdOut, out stdErr, out exitCode);

                Console.Write("\nExitCode = {0}", exitCode);
                Console.Write("\nstdOut = \n{0}", stdOut);
                Console.Write("\nstdErr = \n{0}", stdErr);

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.Write("\n\nCheckMicsConfig(): ERROR: exception caught: {0}\n{1}", e.Message, e.StackTrace);
            }

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


        public static bool TableExists(string twoDotTableName, string testType, DbAccess dbAccess)
        {
            const string SQL_TABLE_EXISTS_TEMPLATE = "SELECT TABLE_TYPE AS tableType FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME = '{2}'";

            string reason = "";
            string testReportLine = "";
            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = SQLHDBC.Zero;

            if (String.IsNullOrWhiteSpace(twoDotTableName))
            {
                reason = "(prescribed table name is NULL or whitespace)";
                testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                Console.Write("\n{0}", testReportLine);
                return false;
            }

            string[] fields = twoDotTableName.Split('.');
            if (fields.Count() != 3)
            {
                reason = "(prescribed table name is not 2-dot)";
                testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                Console.Write("\n{0}", testReportLine);
                return false;
            }

            string dbName = fields[0].Trim();
            string schema = fields[1].Trim();
            string shortTableName = fields[2].Trim();

            string sql = String.Format(SQL_TABLE_EXISTS_TEMPLATE, dbName, schema, shortTableName);

            try
            {
                hConn = ODBCconnect.NewConn();

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(call to ODBC.SQLAllocHandle() failed)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                sqlRet = ODBC.SQLExecDirect(hStmt, sql, sql.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(ODBC.SQLExecDirect() failed)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(table does not exist)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                SQLLEN nullInd;
                string tableType;
                ODBCconnect.DbGetString(hStmt, 1, "tableType", out tableType, 64, out nullInd);
                if (String.IsNullOrWhiteSpace(tableType))
                {
                    reason = "(query on INFORMATION_SCHEMA.TABLES returned unexpected result)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Count")
                {
                    reason = "(ODBCconnect.DbGetInt() threw exception)";
                }
                else
                {
                    reason = "(exception thrown for " + sql + ")";
                }

                testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                Console.Write("\n{0}", testReportLine);
                return false;
            }
            finally
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                ODBCconnect.DisConn(hConn);
            }

            // If we get here the table must exist in the prescribed database.
            return true;
        }

        public static bool TableExistsWithUserReadAccess(string tableName, DbAccess reqdDbAccess, int reqdCount, string permissions, ref int passCount, ref int failCount)
        {
            bool result = false;

            switch (reqdDbAccess)
            {
                case DbAccess.SELECT_FETCH:
                    result = TableExistsWithUserReadAccess(tableName, reqdCount, ref passCount, ref failCount);
                    break;

                case DbAccess.INSERT_UPDATE:
                    result = TableExistsWithUserWriteAccess(tableName, permissions, ref passCount, ref failCount);
                    break;
            }

            return result;
        }

        public static bool TableExistsWithUserReadAccess(string twoDotTableName, int reqdCount, ref int passCount, ref int failCount)
        {
            string testReportLine = "";

            bool result = false;
            string testType = "SQL table exists and accessible?";
            string reason = "";

            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = SQLHDBC.Zero;
            string sql = "SELECT COUNT(*) FROM " + twoDotTableName;

            DbAccess dbAccess = DbAccess.SELECT_FETCH;

            if (!TableExists(twoDotTableName, testType, dbAccess))
            {
                failCount++;
                return false;
            }

            try
            {
                hConn = ODBCconnect.NewConn();

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(call to ODBC.SQLAllocHandle() failed)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    failCount++;
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                sqlRet = ODBC.SQLExecDirect(hStmt, sql, sql.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(ODBC.SQLExecDirect() failed for " + sql + ")";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    failCount++;
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(ODBC.SQLFetch() failed for " + sql + ")";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    failCount++;
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                SQLLEN nullInd;
                int nCount;
                ODBCconnect.DbGetInt(hStmt, 1, "Count", out nCount, out nullInd);
                if (nCount == 0) nCount = -1; // Just for the convenience of the following test when reqdCount is zero.
                if (nCount < reqdCount)
                {
                    reason = "(too few records, should be >= " + reqdCount.ToString("#,##0") + ")";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    failCount++;
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Count")
                {
                    reason = "(ODBCconnect.DbGetInt() threw exception)";
                }
                else
                {
                    reason = "(exception thrown for " + sql + ")";
                }

                testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                failCount++;
                Console.Write("\n{0}", testReportLine);
                return false;
            }
            finally
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                ODBCconnect.DisConn(hConn);
            }

            testReportLine = String.Format(FORMAT_PASS, testType, dbAccess.ToString(), twoDotTableName, reason);
            passCount++;
            Console.Write("\n{0}", testReportLine);

            return result;
        }

        private const string SQL_PERMISSION_QUERY_TEMPLATE = "" +
            "SELECT COUNT(*) AS numMatches " +
            "FROM sys.database_permissions AS dp " +
            "INNER JOIN {0}.sys.objects AS o ON dp.major_id=o.object_id " +
            "INNER JOIN {0}.sys.schemas AS s ON o.schema_id = s.schema_id " +
            "INNER JOIN {0}.sys.database_principals AS dpr ON dp.grantee_principal_id=dpr.principal_id " +
            "WHERE 1=1 " +
            "AND o.name IN ('{1}') " +
            "AND s.name = '{2}' " +
            "AND dpr.name = '{3}' " +
            "AND dp.permission_name='{4}' " +
            "GROUP BY dp.permission_name";


        public static bool TableExistsWithUserWriteAccess(string twoDotTableName, string permissions, ref int passCount, ref int failCount)
        {
            string testReportLine = "";

            bool result = false;
            string testType = "SQL table exists and accessible?";
            string reason = "";

            DbAccess dbAccess = DbAccess.INSERT_UPDATE;

            if (!TableExists(twoDotTableName, testType, dbAccess))
            {
                failCount++;
                return false;
            }

            string[] fields = twoDotTableName.Split('.');
            if (fields.Count() != 3)
            {
                reason = "(prescribed table name is not 2-dot)";
                testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                failCount++;
                Console.Write("\n{0}", testReportLine);
                return false;
            }

            string dbName = fields[0].Trim();
            string schema = fields[1].Trim();
            string shortTableName = fields[2].Trim();

            const string ROLE = "ROLwebusers";

            permissions = permissions.Trim().ToUpper();

            // Check that the prescribed table has 'SELECT' permission granted.
            if (permissions.Contains("S") && !TablePermissionsWorker(twoDotTableName, dbName, shortTableName, schema, ROLE, dbAccess, "SELECT"))
            {
                failCount++;
                return false;
            }

            // Check that the prescribed table has 'INSERT' permission granted.
            if (permissions.Contains("I") && !TablePermissionsWorker(twoDotTableName, dbName, shortTableName, schema, ROLE, dbAccess, "INSERT"))
            {
                failCount++;
                return false;
            }

            // Check that the prescribed table has 'UPDATE' permission granted.
            if (permissions.Contains("U") && !TablePermissionsWorker(twoDotTableName, dbName, shortTableName, schema, ROLE, dbAccess, "UPDATE"))
            {
                failCount++;
                return false;
            }

            // Check that the prescribed table has 'DELETE' permission granted.
            if (permissions.Contains("D") && !TablePermissionsWorker(twoDotTableName, dbName, shortTableName, schema, ROLE, dbAccess, "DELETE"))
            {
                failCount++;
                return false;
            }

            // To get here, the test must have passed.
            testReportLine = String.Format(FORMAT_PASS, testType, DbAccess.INSERT_UPDATE.ToString(), twoDotTableName, reason);
            passCount++;
            Console.Write("\n{0}", testReportLine);

            return result;
        }

        public static bool TablePermissionsWorker(string twoDotTableName, string dbName, string shortTableName, string schema, string role, DbAccess dbAccess, string permission)
        {
            string testReportLine = "";
            string testType = "SQL table exists and accessible?";
            string reason = "";

            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = SQLHDBC.Zero;

            string sql = String.Format(SQL_PERMISSION_QUERY_TEMPLATE, dbName, shortTableName, schema, role, permission);

            try
            {
                hConn = ODBCconnect.NewConn();

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(call to ODBC.SQLAllocHandle() failed)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                sqlRet = ODBC.SQLExecDirect(hStmt, sql, sql.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(ODBC.SQLExecDirect() failed)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(" + permission + " permission not granted)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }

                SQLLEN nullInd;
                int nCount;
                ODBCconnect.DbGetInt(hStmt, 1, "numMatches", out nCount, out nullInd);
                if (nCount != 1)
                {
                    reason = "(query on permissions returned unexpected value)";
                    testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                    Console.Write("\n{0}", testReportLine);
                    return false;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Count")
                {
                    reason = "(ODBCconnect.DbGetInt() threw exception)";
                }
                else
                {
                    reason = "(exception thrown for " + sql + ")";
                }

                testReportLine = String.Format(FORMAT_FAIL, testType, dbAccess.ToString(), twoDotTableName, reason);
                Console.Write("\n{0}", testReportLine);
                return false;
            }
            finally
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                ODBCconnect.DisConn(hConn);
            }

            return true;
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program updates the Subsidiary Database (SDB) table main.sd_traf using field   +");
            Console.Write("\r\n values provided in the prescribed user SDF table [user].su_[sdfName]_traf.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: SdUpdateTraf <dbName> <projCode> <sdfName> [-c] [-d] [-p] [-s] [-t]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        dbName           : database name, e.g. 'fcsa'.");
            Console.Write("\r\n        project          : user's project 'charge' code.");
            Console.Write("\r\n        sdfName          : the XXX in user table su_XXX_traf.");
            Console.Write("\r\n");
            Console.Write("\r\n        --- For Testing -------------------------------------------------------------");
            Console.Write("\r\n        -c               : create empty TS spoof tables in the user's schema.");
            Console.Write("\r\n        -d               : disable attempts to insert/update/delete MDB table records.");
            Console.Write("\r\n        -p               : force validation if already posted (web.user_tables_view: validstat = 'P').");
            Console.Write("\r\n        -s               : enable spoof mode - schema 'main' is replaced by user's schema.");
            Console.Write("\r\n        -t               : create empty TS spoof tables and populate with simple test data.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     SdUpdateTraf fcsa HULME1_0 test_combo0");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with qty. 3 command-line arguments.");
            Console.Write("\r\n\r\n");
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
                Environment.Exit(666);
            }

            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
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
                    Console.Error.Write("\r\n Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Environment.Exit(666);
                }

                // Process the flags.
                string flag = arg.Substring(1, arg.Length - 1).ToUpper();
                switch (flag)
                {
                    case "D":
                        ;
                        break;
                    default:
                        Console.Write("\r\n Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Environment.Exit(666);
                        break;
                }

            }

            // Parse the regular arguments; there must be qty. 3 of them.
            if (regularArgs.Count != 1)
            {
                Console.Write("\r\n Invalid number of arguments.\r\n");
                WriteUsageToConsole();
                Environment.Exit(666);
            }

            // Parse the args.
            mDbName = regularArgs[0];

        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static bool GetEnvVariablesForUtConnect(ref int passCount, ref int failCount)
        {
            // Get the user's MICS ID from the environment.
            mMicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.

            // Get the user's password from the environment.
            mPassword = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).

            return true;
        }

        public static void WriteBanner(string message)
        {
            message = message.Trim();

            const int totalWidth = 120;
            const int sideWidth = 3;
            const int messageOffset = sideWidth + 2;

            const int maxMessageLength = totalWidth - 2 * sideWidth - 2;

            if (message.Length > maxMessageLength)
            {
                message = message.Substring(0, maxMessageLength);
            }

            string wall = RepeatedChar('=', sideWidth);
            string longEdge = RepeatedChar('=', totalWidth);

            string emptyLine = wall +
                        RepeatedChar(' ', totalWidth - 2*sideWidth) +
                        wall;

            string messageLine = wall +
                                    RepeatedChar(' ', messageOffset - sideWidth) +
                                    message +
                                    RepeatedChar(' ', totalWidth - messageOffset - message.Length - sideWidth) +
                                    wall;

            Console.Write("\n\n{0}", longEdge);
            Console.Write("\n{0}", emptyLine);
            Console.Write("\n{0}", messageLine);
            Console.Write("\n{0}", emptyLine);
            Console.Write("\n{0}", longEdge);
        }

        /// <summary>
        /// Return a string comprising a prescribed character repeated a prescribed number of times.
        /// </summary>
        /// <param name="c"> - character.</param>
        /// <param name="nRepeat"> - length of string.</param>
        /// <returns>Repeated characters.</returns>
        public static string RepeatedChar(char c, int nRepeat)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nRepeat; i++)
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method creates a string of digits of the repeating form
        /// 01234567890123...
        /// </summary>
        /// <param name="numChars"> - the prescribed number of digits to be in the string.</param>
        /// <returns></returns>
        public static string MakeRuler(int numChars)
        {
            string ruler = "";

            if (numChars < 1)
            {
                return "";
            }

            for (int i = 0; i < numChars; i++)
            {
                int digit = i % 10;

                ruler += Convert.ToString(digit);
            }

            return ruler;
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
            "WsatAze.exe"
        };

    }
}
