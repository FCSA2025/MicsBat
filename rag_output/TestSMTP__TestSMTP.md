# Documented File: TestSMTP.cs
**Repository Path:** `TestSMTP\TestSMTP.cs`
**Primary Layer:** `TestSMTP`
**Namespace:** `TestSMTP`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestSMTP
{
    class TestSMTP
    {
        private static string mSmtpHostUrl = "";
        private static int mSmtpPort = 0;
        private static bool mEnableSsl = true;
        private static string mSmtpAccount = "";
        private static string mSmtpPassword = "";
        private static string mFromAddress = "";
        private static string mToAddress = "";

        static void Main(string[] args)
        {
            try
            {
                ParseCommandLineArgs(args);

                SmtpClient smtpClient = new SmtpClient(mSmtpHostUrl);

                //smtpClient.UseDefaultCredentials = false;

                MailAddress from = new MailAddress(mFromAddress);
                MailAddress to = new MailAddress(mToAddress);

                MailMessage message = new MailMessage(from, to);

                message.Body = "This is the body text.";
                message.Subject = "Test";

                smtpClient.Port = mSmtpPort;
                smtpClient.EnableSsl = mEnableSsl;
                smtpClient.Credentials = new NetworkCredential(mSmtpAccount, mSmtpPassword);

                smtpClient.Send(message);

                Console.Write("\n\nSuccessful completion - No Exceptions thrown.");

            }
            catch (Exception e)
            {
                Console.Write("\nERROR: Exception: " + e.Message);
            }
        }

        private static void ParseCommandLineArgs(string[] args)
        {
            switch (args.Length)
            {
                case 6:
                    mSmtpHostUrl = args[0];
                    mSmtpPort = Convert.ToInt32(args[1]);
                    mEnableSsl = Convert.ToBoolean(args[2]);
                    mSmtpAccount = args[3];
                    mSmtpPassword = args[4];
                    mToAddress = args[5];
                    mFromAddress = mSmtpAccount;
                    break;
                default:
                    WriteUsageToConsole();

                    Environment.Exit(666);
                    break;
            }

            Console.Write("\n");
            Console.Write("\n          SmtpHostUrl  = {0}", mSmtpHostUrl);
            Console.Write("\n          SmtpPort     = {0}", mSmtpPort);
            Console.Write("\n          EnableSsl    = {0}", mEnableSsl);
            Console.Write("\n          SmtpAccount  = {0}", mSmtpAccount);
            Console.Write("\n          SmtpPassword = {0}", mSmtpPassword);
            Console.Write("\n          FromAddress  = {0}", mFromAddress);
            Console.Write("\n          ToAddress    = {0}", mToAddress);
            Console.Write("\n");
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program sends a test email to a prescribed email address using a   +");
            Console.Write("\r\n prescribed SMTP server and sender email account.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: TestSMTP <SmtpHostUrl> <SmtpPort> <EnableSsl> <SenderMailBox> <SenderPassword> <ToAddress>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        SmtpHostUrl      : URL of SMTP host, e.g. smtp.office365.com or 10.1.1.26");
            Console.Write("\r\n        SmtpPort         : SMTP port number to be used.");
            Console.Write("\r\n        EnableSsl        : Set to true is Tsl/Ssl is required by the SMTP server, otherwise false.");
            Console.Write("\r\n        SenderMailBox    : Email address of the sender's email account on the SMTP server.");
            Console.Write("\r\n        SenderPassword   : Password for the sender's email account.");
            Console.Write("\r\n        ToAddress        : The recipient's email address.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     TestSMTP smtp.office365.com 587 true mics@fcsa.ca myPassword chris_p.bacon@rogers.com");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with all qty. 6 command-line arguments.");
            Console.Write("\r\n       2. The 'from' address is set equal to the SenderMailBox address.");
            Console.Write("\r\n\r\n Build: {0}\r\n", CollateExeMetaData());
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
            result = YYMMDD_HHMM(localTime);
            result += "/" + machine;
#if DEBUG
            result += "-D";
#else
            result += "-R";
#endif
            return result;
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

    }
}

```
