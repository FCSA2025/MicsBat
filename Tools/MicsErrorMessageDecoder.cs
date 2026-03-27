using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class MicsErrorMessageDecoder
    {
        public static void Go(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Write("\n\nERROR: incorrect number of command-line arguments.");
                WriteUsageToConsole();
                Environment.Exit(-666);
            }

            int micsErrorCode = Int32.MinValue;

            try
            {
                micsErrorCode = Convert.ToInt32(args[0]);
            }
            catch
            {
                Console.Write("\n\nERROR: command-line argument is not an integer: \"{0}\".", args[0]);
                WriteUsageToConsole();
                Environment.Exit(-666);
            }

            Console.Write("\n\nMICS# error message for code {0} is \"{1}\".", micsErrorCode, Error.MsgForCode(micsErrorCode));

            Environment.Exit(0);
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program returns the error message associated with a MICS# program error code (number).");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: MicsErrorMessageDecoder  <errorCode>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        errorCode        : the MICS# error code (number).");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     MicsErrorMessageDecoder  -2228");
            Console.Write("\r\n");
        }

    }
}
