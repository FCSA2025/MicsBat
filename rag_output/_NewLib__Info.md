# Documented File: Info.cs
**Repository Path:** `_NewLib\Info.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    using Microsoft.Win32;
    using System.Globalization;
    using System.Runtime.Versioning;
    using System.Text.RegularExpressions;
    using SQLHDBC = IntPtr;

    /// <summary>
    /// This class encapsulates information about the user, system time and 
    /// the currently executing MICS program.
    /// </summary>
    public static class Info
    {

        private static string mDBname;
        private static string mMicsUserName;
        private static string mPassword;
        private static string mProjectCode;
        private static string mMicsRootDir;
        private static string mFcsaDisk;
        private static string mBuildMetaData;
        private static string mManagedBuildInfo;
        private static string mManagedBuildConf;
        private static int mProcessID;
        private static string mMicsCtxCalc;
        private static string mFcsaMaps50K;
        private static string mFcsaMaps250K;
        private static string mWorkDir;
        private static string mGlobalSchema;
        private static string mTsipReportsDir;
        private static string mDestName;
        private static string mPdfName;
        private static string mSdfName;
        private static string mInFilePath;
        private static string mOutFilePath;
        private static bool mMdbWriteEnabled = true;
        private static bool mSpoofModeIsOff = true;
        private static string mText = "";
        private static string mUltrixID;
        private static string mDate;
        private static string mTime;
        private static DateTime mDateTime;
        private static string mApplicationName;
        private static string mRunID = "";
        // Store the static ODBC connection handle created by a call to Ssutil.NewConn().
        private static SQLHDBC mStaticConnHandle = SQLHDBC.Zero;

        /// <summary>
        /// Static constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static Info()
        {
            mBuildMetaData = Info.CollateExeMetaData();
            mProcessID = Process.GetCurrentProcess().Id;
            mManagedBuildInfo = Info.ManagedBuildInfo();
            mManagedBuildConf = BuildConfig();

            string appName = AppDomain.CurrentDomain.FriendlyName;
            mApplicationName = appName.Substring(0, appName.IndexOf('.'));
        }

        /// <summary>
        /// This method returns a string that describes the Visual Studio build
        /// configuration used for the build configuration; it returns one of 
        /// "RELEASE", "DEBUG" or "N/A".
        /// </summary>
        /// <returns>"RELEASE", "DEBUG" or "N/A"</returns>
        public static string BuildConfig()
        {
            string str = "";

            char lastChar = Strings.LastChar(mBuildMetaData);
            if (lastChar == 'R')
            {
                str = "RELEASE";
            }
            else if (lastChar == 'D')
            {
                str = "DEBUG";
            }
            else
            {
                str = "N/A";
            }

            return str;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string
        /// that provides the values of all of Info's private member variables.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static new string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n\nInfo:\n====");

            sb.Append("\nDBname           = " + mDBname);
            sb.Append("\nMicsUserName     = " + mMicsUserName);
            sb.Append("\nPassword         = " + mPassword);
            sb.Append("\nProjectCode      = " + mProjectCode);
            sb.Append("\nMicsRootDir      = " + mMicsRootDir);
            sb.Append("\nFcsaDisk         = " + mFcsaDisk);
            sb.Append("\nBuildMetaData    = " + mBuildMetaData);
            sb.Append("\nmManagedBuildInfo= " + mManagedBuildInfo);
            sb.Append("\nmManagedBuildConf= " + mManagedBuildConf);
            sb.Append("\nProcessID        = " + mProcessID);
            sb.Append("\nMicsCtxCalc      = " + mMicsCtxCalc);
            sb.Append("\nFcsaMaps50K      = " + mFcsaMaps50K);
            sb.Append("\nFcsaMaps250K     = " + mFcsaMaps250K);
            sb.Append("\nWorkDir          = " + mWorkDir);
            sb.Append("\nmGlobalSchema    = " + mGlobalSchema);
            sb.Append("\nmTsipReportsDir  = " + mTsipReportsDir);
            sb.Append("\nmDestName        = " + mDestName);
            sb.Append("\nmPdfName         = " + mPdfName);
            sb.Append("\nmSdfName         = " + mSdfName);
            sb.Append("\nmInFilePath      = " + mInFilePath);
            sb.Append("\nmOutFilePath     = " + mOutFilePath);
            sb.Append("\nmMdbWriteEnabled = " + mMdbWriteEnabled);
            sb.Append("\nmSpoofModeIsOff  = " + mSpoofModeIsOff);
            sb.Append("\nmText            = " + mText);
            sb.Append("\nmUltrixID        = " + mUltrixID);
            sb.Append("\nmDate            = " + mDate);
            sb.Append("\nmTime            = " + mTime);
            sb.Append("\nmDateTime        = " + mDateTime.ToString());
            sb.Append("\nmApplicationName  = " + mApplicationName);
            sb.Append("\nmRunID  = " + mRunID);
            sb.Append("\nmStaticConnHandle = " + mStaticConnHandle.ToString("X"));
            sb.Append("\n");
            return sb.ToString();
        }

        //public static string TBD { get { return mTBD; } set { mTBD = value; } }

        public static string CsharpBuildConf { get { return mManagedBuildConf; } set { mManagedBuildConf = value; } }

        public static string CsharpBuildInfo { get { return mManagedBuildInfo; } set { mManagedBuildInfo = value; } }

        public static DateTime Datetime { get { return mDateTime; } set { mDateTime = value; } }

        public static string Date { get { return mDate; } set { mDate = value; } }

        public static string Time { get { return mTime; } set { mTime = value; } }

        public static string UltrixID
        {
            get { return mUltrixID; }
            set { mUltrixID = value; }
        }

        public static string MicsCtxCalc
        {
            get { return mMicsCtxCalc; }
            set { mMicsCtxCalc = value; }
        }

        public static string FcsaMaps50K
        {
            get { return mFcsaMaps50K; }
            set { mFcsaMaps50K = value; }
        }

        public static string WorkDir
        {
            get { return mWorkDir; }
            set { mWorkDir = value; }
        }

        public static string FcsaMaps250K
        {
            get { return mFcsaMaps250K; }
            set { mFcsaMaps250K = value; }
        }

        public static string BuildMetaData
        {
            get { return mBuildMetaData; }
            set { mBuildMetaData = value; }
        }

        public static string FcsaDisk
        {
            get { return mFcsaDisk; }
            set { mFcsaDisk = value; }
        }

        public static string MicsRootDir
        {
            get { return mMicsRootDir; }
            set { mMicsRootDir = value; }
        }

        public static string DbName
        {
            get { return mDBname; }
            set { mDBname = value; }
        }

        public static string MicsUserName
        {
            get { return mMicsUserName; }
            set { mMicsUserName = value; }
        }

        public static string Password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }

        public static string ProjectCode
        {
            get { return mProjectCode; }
            set { mProjectCode = value; }
        }

        public static int ProcessID
        {
            get { return mProcessID; }
            set { mProcessID = value; }
        }

        public static string GlobalSchema
        {
            get { return mGlobalSchema; }
            set { mGlobalSchema = value; }
        }

        public static string TsipReportsDir
        {
            get { return mTsipReportsDir; }
            set { mTsipReportsDir = value; }
        }

        public static string DestName
        {
            get { return mDestName; }
            set { mDestName = value; }
        }

        public static string PdfName
        {
            get { return mPdfName; }
            set { mPdfName = value; }
        }

        public static string SdfName
        {
            get { return mSdfName; }
            set { mSdfName = value; }
        }

        public static string InFilePath
        {
            get { return mInFilePath; }
            set { mInFilePath = value; }
        }

        public static string OutFilePath
        {
            get { return mOutFilePath; }
            set { mOutFilePath = value; }
        }

        public static bool MdbWriteEnabled
        {
            get { return mMdbWriteEnabled; }
            set { mMdbWriteEnabled = value; }
        }

        public static bool SpoofModeIsOff
        {
            get { return mSpoofModeIsOff; }
            set { mSpoofModeIsOff = value; }
        }

        public static string Text
        {
            get { return mText; }
            set { mText = value; }
        }

        public static string ApplicationName
        {
            get { return mApplicationName; }
            set { mApplicationName = value; }
        }

        public static string RunID
        {
            get { return mRunID; }
            set { mRunID = value; }
        }

        public static SQLHDBC StaticConnHandle { get => mStaticConnHandle; set => mStaticConnHandle = value; }


        /// <summary>
        /// This method returns a string representing a prescribed date
        /// and time using the format YYMMDD_HHMMSS.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string YYMMDD_HHMMSS(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            string yearStr = dateTime.Year.ToString().Substring(2, 2);
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0:00}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);
            string secondStr = String.Format("{0:00}", dateTime.Second);
            sb.Append(yearStr);
            sb.Append(monthStr);
            sb.Append(dayStr);
            sb.Append("-");
            sb.Append(hourStr);
            sb.Append(minuteStr);
            sb.Append(secondStr);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string representing a prescribed date
        /// and time using the format YYMMDD_HHMM.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string YYMMDD_HHMM(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            string yearStr = dateTime.Year.ToString().Substring(2, 2);
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0:00}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);
            sb.Append(yearStr);
            sb.Append(monthStr);
            sb.Append(dayStr);
            sb.Append("-");
            sb.Append(hourStr);
            sb.Append(minuteStr);
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string representing a prescribed date
        /// and time using a format like: "Wed Aug  8 12:27:08 2018".
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToFormatA(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            string dayName = dateTime.DayOfWeek.ToString();
            int monthNum = dateTime.Month;
            string yearStr = dateTime.Year.ToString();
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0,2}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);
            string secondStr = String.Format("{0:00}", dateTime.Second);

            sb.Append(dayName.Substring(0, 3));
            sb.Append(" ");
            sb.Append(DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(monthNum));
            sb.Append(" ");
            sb.Append(dayStr);
            sb.Append(" ");
            sb.Append(hourStr);
            sb.Append(":");
            sb.Append(minuteStr);
            sb.Append(":");
            sb.Append(secondStr);
            sb.Append(" ");
            sb.Append(yearStr);

            return sb.ToString();
        }

        /// <summary>
        /// This method sets the Info date and time fields to the current system time:
        /// the date format is dd-mmm-yyyy and the time format is hh:mm:ss (24-hr clock).
        /// </summary>
        public static void SetDateTimeNow()
        {
            // Get current date and time from the system.
            mDateTime = DateTime.Now;

            // Extract the infomation that we need.
            string yearStr = mDateTime.Year.ToString();
            string monthStr = String.Format("{0:00}", mDateTime.Month);
            string dayStr = String.Format("{0:00}", mDateTime.Day);
            string hourStr = String.Format("{0:00}", mDateTime.Hour);
            string minuteStr = String.Format("{0:00}", mDateTime.Minute);

            // Construct the date string: YYYY.MM.DD
            StringBuilder sbDate = new StringBuilder();
            sbDate.Append(yearStr);
            sbDate.Append(".");
            sbDate.Append(monthStr);
            sbDate.Append(".");
            sbDate.Append(dayStr);
            mDate = sbDate.ToString();

            // Construct the date string: HH:MM
            StringBuilder sbTime = new StringBuilder();
            sbTime.Append(hourStr);
            sbTime.Append(":");
            sbTime.Append(minuteStr);
            mTime = sbTime.ToString();
        }

        /// <summary>
        /// Returns the current system date/time in the string format "YYYY.MM.DD HH:MM:SS".
        /// </summary>
        /// <returns>"YYYY.MM.DD HH:MM:SS"</returns>
        public static string GetDateTimeNow()
        {
            DateTime dateTime = DateTime.Now;

            string yearStr = dateTime.Year.ToString();
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0:00}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);
            string secondStr = String.Format("{0:00}", dateTime.Second);

            StringBuilder sb = new StringBuilder();

            sb.Append(yearStr);
            sb.Append(".");
            sb.Append(monthStr);
            sb.Append(".");
            sb.Append(dayStr);
            sb.Append(" ");
            sb.Append(hourStr);
            sb.Append(":");
            sb.Append(minuteStr);
            sb.Append(":");
            sb.Append(secondStr);

            return sb.ToString();
        }

        /// <summary>
        /// Returns a string giving the date and time at which the currently executing
        /// MICS program was compiled, 32 or 64-bit executable and Release or Debug build.
        /// e.g. "171019-1057/64-R"  =  Compiled at  10:57am on 19th Nov. 2017, 64-bit, Release version.
        /// </summary>
        /// <returns></returns>
        public static string CollateExeMetaData()
        {
            string result = "";

            Assembly assembly = Assembly.GetEntryAssembly();
            TimeZoneInfo target = null;

            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;       //The offset to PE signature is given as an Int32 starting at byte 60.
            const int c_SignatureOffset = 4;       //The letters P and E followed by two null bytes.
            const int c_MachineOffset = 0;         //Two bytes that encode the Machine type (x86 or x64).
            const int c_LinkerTimestampOffset = 4; //Offset from Signature.
            const UInt16 c_x64 = 0x8664;           //PE machine code for x86-64.
            const UInt16 c_x32 = 0x014c;           //PE machine code for x86.

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            //Find the offset for the start of the PE header.
            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);

            //Get the Machine code as Int16.
            var machineCode = BitConverter.ToUInt16(buffer, offset + c_SignatureOffset + c_MachineOffset);
            string machine = "??";
            switch ((Int32)machineCode)
            {
                case (c_x32):
                    machine = "32";
                    break;
                case (c_x64):
                    machine = "64";
                    break;
            }

            //Get the linker's time-stamp.
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_SignatureOffset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            //The ?? operator is called the null-coalescing operator. 
            //It returns the left-hand operand if the operand is not null; 
            //otherwise it returns the right hand operand.
            var tz = target ?? TimeZoneInfo.Local;

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            //string yearStr = localTime.Year.ToString().Substring(2, 2);
            //string monthStr = String.Format("{0:00}", localTime.Month);
            //string dayStr = String.Format("{0:00}", localTime.Day);
            //string hourStr = String.Format("{0:00}", localTime.Hour);
            //string minuteStr = String.Format("{0:00}", localTime.Minute);

            //result += yearStr + monthStr + dayStr + "-" + hourStr + minuteStr;
            result = Info.YYMMDD_HHMM(localTime);
            result += "/" + machine;
#if DEBUG
            result += "-D";
#else
            result += "-R";
#endif
            return result;
        }

        /// <summary>
        /// Returns a string that collates all relevant user information"
        /// MICS user name, password, database name, project code, MICS root
        /// directory, FCSA disk, meta data for executable and Windows process ID.
        /// </summary>
        /// <returns>string comprising all relevant user information.</returns>
        public static string UserInfoAsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\nmMicsUserName   = " + mMicsUserName);
            sb.Append("\r\nmPassword       = " + mPassword);
            sb.Append("\r\nmDBname         = " + mDBname);
            sb.Append("\r\nmProjectCode    = " + mProjectCode);
            sb.Append("\r\nmMicsRootDir    = " + mMicsRootDir);
            sb.Append("\r\nmFcsaDisk       = " + mFcsaDisk);
            sb.Append("\r\nmBuildMetaData  = " + mBuildMetaData);
            sb.Append("\r\nmProcessID      = " + mProcessID);
            sb.Append("\r\nmTsipReportsDir = " + mTsipReportsDir);
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string comprising platform and executables' build information,
        /// e.g. "Build:  64-bit, Debug."
        /// </summary>
        /// <param name="buildConfig"> - BuildConfig.RELEASE or BuildConfig.DEBUG</param>
        /// <returns></returns>
        public static string BuildConfig_A(Enums.BuildConfig buildConfig)
        {
            string platform;
            string config;

            if (Environment.Is64BitProcess)
            {
                platform = "Build:  64-bit, ";
            }
            else
            {
                platform = "Build:  32-bit, ";
            }

            if (buildConfig == Enums.BuildConfig.RELEASE)
            {
                config = "Release.";
            }
            else
            {
                config = "Debug.";
            }


            return platform + config;
        }

        /// <summary>
        /// Returns a string comprising platform and executables' build information,
        /// e.g. "64-R".
        /// </summary>
        /// <param name="buildConfig"> - BuildConfig.RELEASE or BuildConfig.DEBUG</param>
        /// <returns></returns>
        public static string BuildConfig_B(Enums.BuildConfig buildConfig)
        {
            string platform;
            string config;

            if (Environment.Is64BitProcess)
            {
                platform = "64";
            }
            else
            {
                platform = "32";
            }

            if (buildConfig == Enums.BuildConfig.RELEASE)
            {
                config = "R";
            }
            else
            {
                config = "D";
            }


            return platform + "-" + config;
        }

        /// <summary>
        /// This method mimics the ANSI.ISO C/C++ macro <b>__FILE__</b> and returns
        /// the path of the source file being compiled.
        /// </summary>
        /// <remarks>
        /// The ANSI/ISO standard for C/C++ prescribes the standard predefined macro 
        /// <b>__FILE__</b> that expands to the (path) name of the source file being compiled, in 
        /// the form of a C string constant.
        /// <para>
        /// This is the path by which the preprocessor opened the file, not the short name 
        /// specified in #include or as the input file name argument. For example, 
        /// "/usr/local/include/myheader.h" is a possible expansion of this macro. 
        /// </para>
        /// </remarks>
        /// <param name="fileName"> - an optional argument; ignore this an just call as __FILE__()</param>
        /// <returns></returns>
        public static string __FILE__([System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
        {
            return fileName;
        }

        /// <summary>
        /// This method returns a string that identifies the target .NET framework
        /// version at the time of compilation, e.g. ".NET Framework 4.5.2"; the
        /// 'out' argument provides just the version number, e.g. "4.5.2".
        /// </summary>
        /// <param name="version"> - returns just the .NET version number, e.g. "4.5.2".</param>
        /// <returns>a string like ".NET Framework 4.5.2".</returns>
        public static string TargetFrameworkVersion(out string version)
        {
            // 'out' requirement.
            version = "N/A";

            string frmWrkDispName = ".NET version N/A.";

            // Get the currently executing Assembly object;
            Assembly entryAssembly = Assembly.GetEntryAssembly();

            // Get the TargetFrameworkAttribute for this assembly.
            object[] tgtFrmWrkAtts = entryAssembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), false);

            // Guard against the previous call returning null.
            if (tgtFrmWrkAtts != null)
            {
                // Get the zeroth element of the obj[] and cast it to TargetFrameworkAttribute.
                TargetFrameworkAttribute tgtFrmWrkAtt = (TargetFrameworkAttribute)tgtFrmWrkAtts[0];

                // Get the TargetFrameworkAttribute objects' FrameworkDisplayName.
                // This is a string formatted like: ".NET Framework 4.5.2"
                frmWrkDispName = tgtFrmWrkAtt.FrameworkDisplayName;
            }

            // Isolate the actual X.Y.Z version string.
            string pattern = @"\d+\.\d+\.\d+";
            Match m = Regex.Match(frmWrkDispName, pattern);
            if (m.Success)
            {
                version = m.Value;
            }

            return frmWrkDispName;
        }

        /// <summary>
        /// This method returns a string that identifies the programming language,
        /// .NET version, compilation date and time and the Visual Studio build
        /// configuration (RELEASE or DEBUG), e.g. "C#, .NET 4.5.2, 191024-1231/64-R"
        /// </summary>
        /// <returns>a string like "C#, .NET 4.5.2, 191024-1231/64-R"</returns>
        public static string ManagedBuildInfo()
        {
            string str = "";
            string version = "";

            TargetFrameworkVersion(out version);

            str = String.Format("C#, .NET {0}, {1}", version, BuildMetaData);

            return str;
        }

        /// <summary>
        /// This method returns an annotated multi-line string that shows details
        /// of the currently installed Windows operating system.
        /// </summary>
        /// <returns></returns>
        public static string GetOSInfo()
        {
            // For this method to give the correct results for Windows major version = 10
            // (i.e. Windows 10 or 11, or Server 2016, 2019 or 2022), the application
            // that calls this method must be 'manifested' for Windows 10.
            //
            // Instructions:
            // =============
            //    1. In Visual Studio, right click on the application project and select ADD.
            //    2.  Select NEW ITEM ...
            //    3.  Select 'Visual C# Items' on the left and 'Application Manifest File' in the middle panel.
            //    4.  Visual Studio will create a manifest file for the application and fill it with default text.
            //    5.  The manifest file uses XML format.
            //    6.  Under the elements Compatibility -- > Application remove the comment around the line
            //        <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
            //    7.  This will 'manifest' the application to know its running under Windows 10 if 
            //        Windows 10 is actually the installed OS.
            //    8.  If you don't do this, the method will return major version = 6, minor version = 2 which is 
            //        Windows 8 or Server 2012.

            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        switch (vs.Minor)
                        {
                            case 0:
                                operatingSystem = "2000";
                                break;
                            case 1:
                                operatingSystem = "XP";
                                break;
                            case 2:
                                operatingSystem = "Server 2003 or Server 2003 R2 or XP 64-bit edition";
                                break;
                        }
                        break;
                    case 6:
                        switch (vs.Minor)
                        {
                            case 0:
                                operatingSystem = "Server 2008 or Vista";
                                break;
                            case 1:
                                operatingSystem = "Server 2008 R2 or Windows 7";
                                break;
                            case 2:
                                operatingSystem = "Server 2012 or Windows 8";
                                break;
                            case 3:
                                operatingSystem = "Server 2012 R2 or Windows 8.1";
                                break;
                        }
                        break;
                    case 10:
                        switch (vs.Minor)
                        {
                            case 0:
                                operatingSystem = "Windows 10 or 11, or Server 2016, 2019 or 2022";
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                //operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }


        /// <summary>
        /// This method returns the highest (most recent) version of the currently
        /// installed dot NET frameworks.
        /// </summary>
        /// <returns></returns>
        public static string GetHighestInstalledNETversion()
        {
            string result = "N/A";

            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    int releaseKey = (int)ndpKey.GetValue("Release");

                    if (releaseKey >= 528040)
                        result = "4.8 or later";
                    else if (releaseKey >= 461808)
                        result = "4.7.2";
                    else if (releaseKey >= 461308)
                        result = "4.7.1";
                    else if (releaseKey >= 460798)
                        result = "4.7";
                    else if (releaseKey >= 394802)
                        result = "4.6.2";
                    else if (releaseKey >= 394254)
                        result = "4.6.1";
                    else if (releaseKey >= 393295)
                        result = "4.6";
                    else if (releaseKey >= 379893)
                        result = "4.5.2";
                    else if (releaseKey >= 378675)
                        result = "4.5.1";
                    else if (releaseKey >= 378389)
                        result = "4.5";
                    // This code should never execute. A non-null release key should mean
                    // that 4.5 or later is installed.
                    else result = "No 4.5 or later version detected";
                }
                else
                {
                    result = ".NET Framework Version 4.5 or later is not detected.";
                }
            }

            return result;
        }




    } //class
} //namespace

```
