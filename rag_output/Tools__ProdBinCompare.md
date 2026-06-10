# Documented File: ProdBinCompare.cs
**Repository Path:** `Tools\ProdBinCompare.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    using SQLLEN = Int64;

    public class ProdBinCompare
    {
        private static Dictionary<string, ProdBinConfig> pbcDictionary = new Dictionary<string, ProdBinConfig>();
        public static void Go(string[] args)
        {
            if (args.Length != 1) 
            {
                Console.Write("\n\nUsage:  ProdBinCompare  <pathToDirectory>\n\n");
                Application.ExitQuietly(666);
            }

            string pathToDir = args[0];

            if (!Directory.Exists(pathToDir))
            {
                Console.Write("\n\nERROR: directory does not exist: {0}\n\n", pathToDir);
                Application.ExitQuietly(667);
            }

            // UtConnect() expects to receive the user's MICSUSER and PASSWORD
            // environment variables via the static class Info. Get the values 
            // of these two environmental variables and set Info.MicsUserName
            // and Info.Password.
            GetEnvVariablesForUtConnect();

            // Use the fcsa database.
            Info.DbName = "fcsa";

            // Establish an FCSA user session with the database.
            int rc = Ssutil.UtConnect(Info.DbName, 1);
            if (rc != 0)
            {
                /* Can't connect to database */
                Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                Application.ExitQuietly(11);
            }

            ProdBinConfig pbc;
            SQLLEN[] nulls;

            int cursor = DynProdBinConfig.Select("", "fileName");

            while (DynProdBinConfig.Fetch(cursor, out pbc, out nulls) == Constant.SUCCESS)
            {
                //Console.Write("\n{0}", pbc.ToString());

                // Update the dictionary.
                string candidateKey = pbc.FileName.ToLower();
                if (pbcDictionary.ContainsKey(candidateKey))
                {
                    // Replace the value if the pbc just fetched has a more recent sortDate.
                    int existingSortDate = Convert.ToInt32(pbcDictionary[candidateKey].SortDate);
                    int candidateSortDate = Convert.ToInt32(pbc.SortDate);

                    if (candidateSortDate > existingSortDate) pbcDictionary[candidateKey] = pbc;
                }
                else
                {
                    pbcDictionary.Add(pbc.FileName.ToLower(), pbc);
                }
            }

            DynProdBinConfig.Close(cursor);

            foreach (KeyValuePair<string, ProdBinConfig> kvp in pbcDictionary)
            {
                string filePath = Path.Combine(pathToDir, kvp.Key);
                string fileSortDate = File.GetLastWriteTime(filePath).ToString("yyyyMMdd");
                string difference = (kvp.Value.SortDate != fileSortDate) ? "<==========": "";
                Console.Write("\n{0,-40}, {1,8}, {2,8} {3}", kvp.Value.FileName, kvp.Value.SortDate, fileSortDate, difference);
            }

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


    }
}

```
