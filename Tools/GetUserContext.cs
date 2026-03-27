using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class GetUserContext
    {
        public static void Go(string[] args)
        {
            // Write the Windows context parameters to the console.
            Console.Write("{0}", GetWindowsContext());

            // UtConnect() expects to receive the user's MICSUSER and PASSWORD
            // environment variables via the static class Info. Get the values 
            // of these two environmental variables and set Info.MicsUserName
            // and Info.Password.
            GetEnvVariablesForUtConnect();

            // Use the fcsa database.
            Info.DbName = "micsdev";

            // Establish an FCSA user session with the database.
            int rc = Ssutil.UtConnect(Info.DbName, 1);
            if (rc != 0)
            {
                /* Can't connect to database */
                Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                Application.ExitQuietly(11);
            }

            Console.Write("{0}\n", GetSqlServerContext());

            // Terminate the FCSA user's DB session.
            Ssutil.UtDisconnect(1);

            Application.ExitQuietly(0);
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

        public static string GetWindowsContext()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // Current Windows thread.
                string windowsUserName = Environment.UserName;
                string commandLine = Environment.CommandLine;
                string currentDirectory = Environment.CurrentDirectory;
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST. OR use Environment.MachineName;
                string thisIP = GetIPsByName(hostName, true, false)[0].ToString();
                string OSVersion = Info.GetOSInfo();
                bool is64BitOS = Environment.Is64BitOperatingSystem;
                bool is64BitProc = Environment.Is64BitProcess;
                int procCount = Environment.ProcessorCount;
                string userDomainName = Environment.UserDomainName;
                string tgtNETversion; Info.TargetFrameworkVersion(out tgtNETversion);
                string installedNETversion = Info.GetHighestInstalledNETversion();
                string programPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                // Get all the Windows environment variables referenced by the MICS# programs.
                string FCSADISK = Environment.GetEnvironmentVariable("FCSADISK");
                string FCSAMAPS50K = Environment.GetEnvironmentVariable("FCSAMAPS50K");
                string FCSAMAPS250K = Environment.GetEnvironmentVariable("FCSAMAPS250K");
                string MICSUSER = Environment.GetEnvironmentVariable("MICSUSER");
                string MICS_CTX_CALC = Environment.GetEnvironmentVariable("MICS_CTX_CALC");
                string MICS_NAD_FILE = Environment.GetEnvironmentVariable("MICS_NAD_FILE");
                string MICS_PROJECT = Environment.GetEnvironmentVariable("MICS_PROJECT");
                string MICS_REMOTE = Environment.GetEnvironmentVariable("MICS_REMOTE");
                string MICS_ROOT_DIR = Environment.GetEnvironmentVariable("MICS_ROOT_DIR");
                string MICS_STOPCASE = Environment.GetEnvironmentVariable("MICS_STOPCASE");
                string PASSWORD = Environment.GetEnvironmentVariable("PASSWORD");
                string TARGETDIRFORTSIPREPORTS = Environment.GetEnvironmentVariable("TARGETDIRFORTSIPREPORTS");
                string WORK_DIR = Environment.GetEnvironmentVariable("WORK_DIR");

                sb.Append("\n\nCurrent Application Environment:");
                sb.Append("\n==============================");
                sb.Append("\ndateTimeNow             = " + Info.GetDateTimeNow());
                sb.Append("\nhostName                = " + hostName);
                sb.Append("\nIPv4                    = " + thisIP);
                sb.Append("\nWindows OS Version      = " + OSVersion);
                sb.Append("\nInstalled .NET version  = " + installedNETversion);
                sb.Append("\nTarget    .NET version  = " + tgtNETversion);
                sb.Append("\n64-bit OS?              = " + is64BitOS);
                sb.Append("\n64-bit process?         = " + is64BitProc);
                sb.Append("\nAvailable cores         = " + procCount);
                sb.Append("\nwindowsUserName         = " + windowsUserName);
                sb.Append("\nNetwork Domain Name     = " + userDomainName);
                sb.Append("\ncurrentDirectory        = " + currentDirectory);
                sb.Append("\ncurrentProgram          = " + programPath);
                sb.Append("\ncommandLine             = " + commandLine);

                sb.Append("\n\nWindows Environment Variables:");
                sb.Append("\n==============================");
                sb.Append("\nFCSADISK                = " + FCSADISK);
                sb.Append("\nFCSAMAPS50K             = " + FCSAMAPS50K);
                sb.Append("\nFCSAMAPS250K            = " + FCSAMAPS250K);
                sb.Append("\nMICSUSER                = " + MICSUSER);
                sb.Append("\nMICS_CTX_CALC           = " + MICS_CTX_CALC);
                sb.Append("\nMICS_NAD_FILE           = " + MICS_NAD_FILE);
                sb.Append("\nMICS_PROJECT            = " + MICS_PROJECT);
                sb.Append("\nMICS_REMOTE             = " + MICS_REMOTE);
                sb.Append("\nMICS_ROOT_DIR           = " + MICS_ROOT_DIR);
                sb.Append("\nMICS_STOPCASE           = " + MICS_STOPCASE);
                sb.Append("\nPASSWORD                = " + PASSWORD);
                sb.Append("\nTARGETDIRFORTSIPREPORTS = " + TARGETDIRFORTSIPREPORTS);
                sb.Append("\nWORK_DIR                = " + WORK_DIR);

            }
            catch (Exception e)
            {
                sb.Append("\n\nGetUserContext(): ERROR: exception: " + e.Message);
                sb.Append("\n" + e.StackTrace);
                Log2.e("\n\nGetUserContext(): ERROR: exception: " + e.Message);
                Log2.e("\n" + e.StackTrace);
            }

            return sb.ToString();
        }

        public static string GetSqlServerContext()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // Get the SQL Server user-context for the current UtConnect() session.
                string sqlServerVersion = "";
                string sqlUserName = "";
                string sqlDefaultSchema = "";
                string sqlDbName = "";
                string sqlServerName = "";
                string sqlAuthMode = "";

                Ssutil.GetSQLsessionParams(out sqlServerVersion, out sqlUserName, out sqlDefaultSchema, out sqlDbName, out sqlServerName, out sqlAuthMode);

                string ultrixID;
                Suutils.GetUltrixID(out ultrixID);

                // This is the legacy method that gets the default schema for the current Windows user.
                string dbo_FCSASchema = Ssutil.GetFCSASchema();

                // This is the newer method that explicitely retrieves the schema for
                // the prescribed MICS user.
                string MICSUSER = Environment.GetEnvironmentVariable("MICSUSER");
                string getFCSASchema_A = Ssutil.GetFCSASchema_A(MICSUSER);

                sb.Append("\n\nSQL Server User-Context:");
                sb.Append("\n==============================");
                sb.Append("\n@@SERVERNAME       = " + sqlServerName);
                sb.Append("\nServer Version     = " + sqlServerVersion);
                sb.Append("\nAuthorization Mode = " + sqlAuthMode);
                sb.Append("\nDB_NAME()          = " + sqlDbName);
                sb.Append("\nUSER_NAME()        = " + sqlUserName);
                sb.Append("\nSCHEMA_NAME()      = " + sqlDefaultSchema);
                sb.Append("\ndbo.FCSASchema()   = " + dbo_FCSASchema);
                sb.Append("\nGetFCSASchema_A()  = " + getFCSASchema_A);
                sb.Append("\nUltrixID           = " + Info.UltrixID);

            }
            catch (Exception e)
            {
                sb.Append("\n\nGetUserContext(): ERROR: exception: " + e.Message);
                sb.Append("\n" + e.StackTrace);
                Log2.e("\n\nGetUserContext(): ERROR: exception: " + e.Message);
                Log2.e("\n" + e.StackTrace);
            }

            return sb.ToString();
        }

        public static IPAddress[] GetIPsByName(string hostName, bool ip4Wanted, bool ip6Wanted)
        {
            // Check if the hostname is already an IPAddress
            IPAddress outIpAddress;
            if (IPAddress.TryParse(hostName, out outIpAddress) == true)
                return new IPAddress[] { outIpAddress };
            //<----------

            IPAddress[] addresslist = Dns.GetHostAddresses(hostName);

            if (addresslist == null || addresslist.Length == 0)
                return new IPAddress[0];
            //<----------

            if (ip4Wanted && ip6Wanted)
                return addresslist;
            //<----------

            if (ip4Wanted)
                return addresslist.Where(o => o.AddressFamily == AddressFamily.InterNetwork).ToArray();
            //<----------

            if (ip6Wanted)
                return addresslist.Where(o => o.AddressFamily == AddressFamily.InterNetworkV6).ToArray();
            //<----------

            return new IPAddress[0];
        }

    }
}
