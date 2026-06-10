# Documented File: SQLmissingTablesToCSVFiles.cs
**Repository Path:** `Tools\SQLmissingTablesToCSVFiles.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    using _DataStructures;
    using System.IO;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;

    public class SQLmissingTablesToCSVFiles
    {
        private static string mMissingsTableNameRoot = "hulme.ISEDmissingTxLinks_";
        private static string[] mMemberOpers = new string[]
        {
                        "ALIANT",
                        "BCHY",
                        "BELL",
                        "BMCE",
                        "BRAGG",
                        "DND",
                        "GLW",
                        "HYONE",
                        "HYQU",
                        "MTS",
                        "NAVI",
                        "NTTEL",
                        "NWT",
                        "ONT",
                        "RCTL",
                        "SHAW",
                        "STEL",
                        "TBAY",
                        "TEKSAV",
                        "TERAGO",
                        "TLUSAB",
                        "TLUSBC",
                        "TLUSMC",
                        "TLUSQC",
                        "VDTR",
                        "WIREIE",
                        "XCI",
                        "ZAYO"
        };

        private const string RUBRIC = @"This worksheet provides a list of TAFL records licensed to {0} that are missing from FCSA's MDB as of {1}.

Number of missing links = {2}

Column A (heading 'ProblemCode') provides an analysis of the problem using the following codes:
          L  =  No location match.
          A  =  Location match but no azimuth match.
          F  =  Location and azimuth match but no frequency match.

ISED TAFL field definitions:  https://sms-sgs.ic.gc.ca/eic/site/sms-sgs-prod.nsf/vwapj/tafl_description_ltaf.pdf/$file/tafl_description_ltaf.pdf
ISED TAFL data (CSV file)  :  http://www.ic.gc.ca/engineering/SMS_TAFL_Files/TAFL_LTAF_Fixe.zip


";

        public static void Go(string[] args)
        {
            //...Log2.v("\n\nSQLmissingTablesToCSVFiles.Go(): Entry");

            try
            {
                // Get environment variables required to begin a database session.
                // Use static object Info to collate environment variables, process IDs etc.
                Info.DbName = "fcsa";
                Info.MicsUserName = Environment.GetEnvironmentVariable("MicsUser");     // REQUIRED.
                Info.Password = Environment.GetEnvironmentVariable("Password");         // REQUIRED (but can be anything).

                /* Attempt connection to the prescribed database */
                int rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    string str = String.Format("\r\nCannot connect to database {0}, reason {1:D}\r\n", Info.DbName, rc);
                    Console.Write(str);
                    Log2.e("\n\nSQLmissingTablesToCSVFiles.Go(): ERROR: " + str);
                    Application.ExitQuietly(400);
                }
                //...Log2.v("\n\nSQLmissingTablesToCSVFiles.Go(): Successfully connected to database: " + Info.DbName);

                // The application-specific code follows.
 
                int count = 0;

                foreach (string oper in mMemberOpers)
                {
                    string tableName = mMissingsTableNameRoot + oper;

                    int handle = DynTAFL.SelectTAFL(tableName, "", "");

                    TAFL tafl;
                    SQLLEN[] nullInds;

                    List<string> csvLines = new List<string>();

                    count = Ssutil.DbCountRows(tableName, "");

                    Info.SetDateTimeNow();

                    csvLines.Add(String.Format(RUBRIC, oper, Info.Date, count));
                    csvLines.Add(TAFL.ColumnNamesSpecial());

                    while (DynTAFL.FetchTAFL(handle, out tafl, out nullInds) == ODBC.SQL_SUCCESS)
                    {
                        csvLines.Add(tafl.ToCSVstringSpecial());
                    }

                    DynTAFL.CloseTAFL(handle);

                    string outputFilePath = @"d:\MicsBatchLogs\" + "MissingLinks - " + oper + ".csv";
                    File.WriteAllLines(outputFilePath, csvLines);
                }

                //The following call frees up the ODBC handle to 'Environment' 
                //resources that were used during the session.
                Ssutil.UtDisconnect(1);

                //...Log2.v("\n\nSQLmissingTablesToCSVFiles.Go(): Disconnected from database: " + Info.DbName);
                //...Log2.v("\n\nSQLmissingTablesToCSVFiles.Go(): Exit");

                Application.ExitQuietly(0);
            }
            catch (Exception e)
            {
                string str = String.Format("\r\nSQLmissingTablesToCSVFiles.CreateSpoofMdbTables(): ERROR: exception: \n{0}\n{1}\n", e.Message, e.StackTrace);
                Log2.e(str);
                Console.Write(str);
                Application.ExitQuietly(666);
            }        
        }
    }
}

```
