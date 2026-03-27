using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class CallSignsFromPDF
    {
        public static void Go(string[] args)
        {
            try
            {
                string[] allLines;
                List<string> callSigns = new List<string>();

                // There must be a single argument that provides the path to the PDF file.
                if (args.Length != 1)
                {
                    Console.Write("\n\nERROR: missing path to PDF file.");
                    Application.ExitQuietly(666);
                }

                // Attempt to read from the PDF file.
                allLines = File.ReadAllLines(args[0]);

                // Parse through the lines of text to find one that starts with "AK".
                foreach (string line in allLines)
                {
                    if (line.StartsWith("AK"))
                    {
                        // Parse the CSV fields.
                        string[] fields = line.Split(',');

                        // Check that the AK record has exactly 9 fields.
                        if (fields.Length != 9)
                        {
                            Console.Write("\n\nERROR: invalid AK record syntax : incorrect number of fields (should be 9):\n{0}", line);
                            Application.ExitQuietly(666);
                        }

                        // local  call1 is field 3 of [0, 8].
                        // remote call1 is field 4 of [0, 8].
                        for (int i = 3; i <= 4; i++)
                        {
                            string callSign = fields[i].Trim().ToUpper();

                            if (String.IsNullOrWhiteSpace(callSign))
                            {
                                Console.Write("\n\nERROR: invalid AK record : mandatory call sign is missing:\n{0}", line);
                                Application.ExitQuietly(666);
                            }

                            // Accumulate the call sign in the list.
                            callSigns.Add(callSign.Trim());
                        }
                    }
                }

                // Remove duplicate entries and sort the remaining call signs.
                callSigns = Strings.RemoveDuplicates(callSigns);
                callSigns.Sort(StringComparer.Ordinal);

                int lineNum = 1;
                foreach (string callSign in callSigns)
                {
                    Console.Write("\n{0}.     {1}", lineNum++, callSign);
                }
            }
            catch (Exception e)
            {
                Console.Write("\n\nERROR: exception: {0}", e.Message);
                Application.ExitQuietly(666);
            }

        }





    }
}
