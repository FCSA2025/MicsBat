using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    public static class Build
    {
        public static string GetExeInfo(this Assembly assembly, TimeZoneInfo target = null)
        {
            string result = "";
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;       //The offset to PE signature is given as an Int32 starting at byte 60.
            const int c_SignatureOffset = 4;       //The letters P and E followed by two null bytes.
            const int c_MachineOffset = 0;         //Two bytes that encode the Machine type (x86 or x64).
            const int c_LinkerTimestampOffset = 4; //Offset from Signature.
            const UInt16 c_x64 = 0x8664;              //PE machine code for x86-64.
            const UInt16 c_x32 = 0x014c;               //PE machine code for x86.

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

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            //string yearStr = localTime.Year.ToString().Substring(2, 2);
            //string monthStr = String.Format("{0:00}", localTime.Month);
            //string dayStr = String.Format("{0:00}", localTime.Day);
            //string hourStr = String.Format("{0:00}", localTime.Hour);
            //string minuteStr = String.Format("{0:00}", localTime.Minute);

            //result += yearStr + monthStr + dayStr + "-" + hourStr + minuteStr;
            result = TimeStamp.StandardFormat(localTime);
            result += "/" + machine;
#if DEBUG
            result += "-D";
#else
    result += "-R";
#endif
            return result;
        }
    }
}
