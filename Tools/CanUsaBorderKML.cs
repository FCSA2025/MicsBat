using _NewLib;
using System;

namespace Tools
{
    public class CanUsaBorderKML
    {
        public static void Go(string[] args)
        {
            // Enable or disable developmental run-time logging.
#if true
            string mLog2FilePath = @"d:\MicsBatchLogs\CanUsaBorderKml.log";
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

            CanUsaBorder.Go(args);
            Application.ExitQuietly(0);
        }


    }
}
