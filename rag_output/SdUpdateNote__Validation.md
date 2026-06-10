# Documented File: Validation.cs
**Repository Path:** `SdUpdateNote\Validation.cs`
**Primary Layer:** `SdUpdateNote`
**Namespace:** `SdUpdateNote`

## Source Code Representation
```csharp
﻿using System;
using System.IO;

namespace SdUpdateNote
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the method that performs validation of a prescribed user
    /// SDB table su_XXX_note; this is achieved by running the existing
    /// WebMICS program 'sdfValidate.exe' with the appropriate command line arguments
    /// in a separate process.
    /// </summary>
    public class Validation
    {
        /// <summary>
        /// This method performs the validation of a prescribed user SDB table su_XXX_note; 
        /// this is achieved by running the existing WebMICS program 'sdfValidate.exe' with 
        /// the appropriate command line arguments in a separate process.
        /// </summary>
        /// <param name="isValidIfAlreadyPosted"></param>
        /// <param name="userTables"></param>
        /// <param name="userTablesNullInds"></param>
        /// <returns></returns>
        public static int Perform(bool isValidIfAlreadyPosted, out UserTables userTables, out SQLLEN[] userTablesNullInds)
        {
            // 'out' requirement.
            userTables = null;
            userTablesNullInds = null;

            int status = Constant.SUCCESS;
            string TEMP_DIR = Ssutil.GetFcsaTemp();

            string exePath = Ssutil.GetMicsRoot(Info.DbName) + @"bin\sdfValidate.exe";

            string args = String.Format("{0} {1} {2} {3} {4}",
                                            Info.DbName,
                                            TEMP_DIR,
                                            "note",
                                            Info.SdfName,
                                            Info.ProjectCode
                                            );
            string stdOut;
            string stdErr;
            int exitCode;

            WindowsShell.RunCommand(exePath, args, out stdOut, out stdErr, out exitCode);

            //...Log2.v("\nValidation.Perform(): exePath  = " + exePath);
            //...Log2.v("\nValidation.Perform(): args     = " + args);
            //...Log2.v("\nValidation.Perform(): exitCode = " + exitCode);
            //...Log2.v("\nValidation.Perform(): stdOut = \n" + stdOut);
            //...Log2.v("\nValidation.Perform(): stdErr = \n" + stdErr);

            // The MICS program sdfvalidate.exe writes textual validation results to
            // a file whode path is TEMP_DIR + Info.SdfName + ".txt"  .
            // Read the contents of this validation results file and write it out
            // to the console.
            string validationResultsFilePath = TEMP_DIR + Info.SdfName + ".txt";

            //...Log2.v("\nValidation.Perform(): validationResultsFilePath = " + validationResultsFilePath);

            try
            {
                string contents = File.ReadAllText(validationResultsFilePath);
                //Console.Write("\r\n{0}", contents);
                Console.Write("\r\n");
                File.Delete(validationResultsFilePath);
            }
            catch (Exception e)
            {
                Log2.e("\n\nValidation.Perform(): ERROR: exception: Could not read or delete validation output file: " + validationResultsFilePath + "\n" + e.Message);
                Console.Write("\r\nCould not read validation output file {0}.", validationResultsFilePath);
            }

            // Check the exitCode and process accordingly.
            if (exitCode != Constant.SUCCESS)
            {
                // The execution of sdfvalidate.exe failed.
                Console.Write("\r\nSubsidiary file failed Validation.\r\nRevalidate this file.");
                status = 2;
            }
            else
            {
                // sdfvalidate.exe completed successfully.

                // Verify that the SDF file is valid by checking the 'validstat' field
                // from the relevant record in the DB table web.user_tables.

                // Set up the keys to define a unique record in web.user_tables.
                string oper;
                Suutils.GetUltrixID(out oper);
                int tabletype = Constant.SU_NOTE;
                string file_name = Info.SdfName;

                //...Log2.v(String.Format("\nValidation.Perform(): keys:  oper = {0}; tabletype = {1}; file_name = {2}", oper, tabletype, file_name));

                bool exists = DynUserTables.FetchRecordWithKeys(oper, tabletype, file_name,
                                                                out userTables, out userTablesNullInds);

                if (!exists)
                {
                    // Could not get the record from web.user_tables
                    Log2.e("\n\nValidation.Perform(): ERROR: call to DynUserTables.FetchRecordWithKeys() failed.");
                    Console.Write("\r\nCould not get validation status.");
                    status = 3;
                }
                else
                {
                    string validstat = userTables.validstat.ToUpper();

                    if (userTablesNullInds[UserTables.VALIDSTAT] == Constant.DB_NULL)
                    {
                        // validstat field is NULL.
                        Log2.e("\n\nValidation.Perform(): ERROR: user_tables record is FOUND but validstat is NULL.");
                        Console.Write("\r\n\t\tSDF failed validation.\r\n");
                        status = 1;
                    }
                    else if (validstat == "Y")
                    {
                        // The user_tables record is found and marked as validated "Y".
                        // Thus, we have a successful validation of the prescribed SDF file.
                        //...Log2.v("\nValidation.Perform(): user_tables record FOUND: validation SUCCEEDED, validstat = " + validstat);

                        status = Constant.SUCCESS;
                    }
                    else if (validstat == "P" && isValidIfAlreadyPosted)
                    {
                        // The user_tables record is found and marked as "P" for POSTED.
                        // The legacy C/C++ version of SdUpdateCtx fails validation in this scenario.
                        // The user used the -p option to prevent failure of validation.
                        //...Log2.v("\nValidation.Perform(): user_tables record FOUND: validation SUCCEEDED, validstat = " + validstat);

                        status = Constant.SUCCESS;
                    }
                    else
                    {
                        // validstat is is not "Y".
                        Log2.e("\n\nValidation.Perform(): ERROR: user_tables record is FOUND but validstat = " + validstat);
                        Console.Write("\r\n\t\tSDF failed validation.\r\n");
                        status = 1;
                    }

                }

            }

            return status;
        }





    }
}



```
