using _NewLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// TBD
    /// </summary>
    public class TestTsipInitiator
    {
        private const string TSIPINITIATOR = @"D:\Users\ahulme\MICS#\_bin\Release\tsipInitiator.exe";
        private const string COMMON_ARGS = @" fcsa hulme1_0 -oZulu ";

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="args"></param>
        public static void Test_TsipInitiator(string[] args)
        {
            string[] commandLineArgs = new string[11]
            {
                COMMON_ARGS + "agt2w",
                COMMON_ARGS + "25hopgtaswdown",
                COMMON_ARGS + "keyhole",
                COMMON_ARGS + "milangll",
                COMMON_ARGS + "es2subcases",
                COMMON_ARGS + "tstest0183",
                COMMON_ARGS + "esdnd4_env",
                COMMON_ARGS + "passive",
                COMMON_ARGS + "testorbit",
                COMMON_ARGS + "esdnd4",
                COMMON_ARGS + "agginttest"
            };

            foreach (string cla in commandLineArgs)
            {
                // Spawn another process that will be used to run TpRunTsip.
                Process tsipProcess = new Process();

                // Configure the process using the StartInfo properties.
                tsipProcess.StartInfo.FileName = TSIPINITIATOR;
                tsipProcess.StartInfo.Arguments = cla;
                tsipProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Console.Write("\n\ncommand line = " + TSIPINITIATOR + cla);

                // Start the tpRunTsip process.
                tsipProcess.Start();

                int tsipProcessID = tsipProcess.Id;

                Console.Write("\nTsipQ.StartTsip(): TpRunTsip execution has started, process ID = " + tsipProcessID);

                Thread.Sleep(3000);

            }

        }






    }
}
