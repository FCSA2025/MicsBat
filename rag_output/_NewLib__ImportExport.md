# Documented File: ImportExport.cs
**Repository Path:** `_NewLib\ImportExport.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    public class ImportExport
    {



        /// <summary>
        /// This method verifies that a file with the prescribed path can 
        /// be created and written to; any existing file is overwritten, resulting
        /// in an empty (zero-bytes) file; any directories that need to be created 
        /// are created first; problems encountered in attempting to create the
        /// file throw exceptions that are caught within the method and succinct, 
        /// informative error messages are provided to the caller using the 'out' 
        /// string argument.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool CanWriteToFile(String filePath, out String errorMessage)
        {
            bool result = false;
            String dirPath;
            String fileName;
            errorMessage = "ImportExport.CanWriteToFile(): ERROR.\n";
            errorMessage += "Requested filePath:  " + filePath + "\n";

            //Parse the filePath into a directory path and an 'end' file name.
            try
            {
                dirPath = Path.GetDirectoryName(filePath);
                fileName = Path.GetFileName(filePath);
            }
            catch (ArgumentException)
            {
                errorMessage += "The path contains invalid characters, is empty, or contains only white spaces.\n";
                return result;
            }
            catch (PathTooLongException)
            {
                errorMessage += "The path parameter is longer than the system-defined maximum length.\n";
                return result;
            }

            //The directory path has to exist in its entirity before a file
            //can be created in its 'end' directory. The following system method
            //creates the full directory path if it does not already exist.
            try
            {
                Directory.CreateDirectory(dirPath);
            }
            catch
            {
                errorMessage += "Directory.CreateDirectory() failed.\n";
                return result;
            }

            //We are now sure of having an 'end' directory to write into.
            //Create the file. This opens a FileStream that we then have to close
            //before attempting to access the file again.
            try
            {
                FileStream fs = File.Create(filePath);
                fs.Close();
            }
            catch (IOException)
            {
                errorMessage += "System.IO.File.Create() threw an exception.\n";
                return result;
            }

            //Now overwrite the contents of the file with zero-bytes.
            //This is essential to fully create a new file!
            try
            {
                File.WriteAllText(filePath, "");
                result = true;
            }
            catch (IOException)
            {
                errorMessage += "System.IO.File.WriteAllText() threw an IOException.\n";
                return result;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage += "Path specified a file that is read-only or the caller does not have the required permission.\n";
                return result;
            }
            catch (SecurityException)
            {
                errorMessage += "The caller does not have the required permission.\n";
                return result;
            }

            //If we reach here, the file creation attempt succeeded.
            errorMessage = "No errors detected.";
            return result;
        }

        /// <summary>
        /// This method verifies that a file with the prescribed path exists and 
        /// can be read from; any problems encountered will throw exceptions that 
        /// are caught within the method and succinct, informative error messages,
        /// are provided to the caller using the 'out' string argument.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool CanReadFromFile(String filePath, out String errorMessage)
        {
            bool result = false;
            String dirPath;
            String fileName;
            errorMessage = "ImportExport.CanReadFromFile(): ERROR.\n";
            errorMessage += "Requested filePath:  " + filePath + "\n";

            //Check for invalid syntanx and path length.
            try
            {
                dirPath = Path.GetDirectoryName(filePath);
                fileName = Path.GetFileName(filePath);
            }
            catch (ArgumentException)
            {
                errorMessage += "The import path contains invalid characters, is empty, or contains only white spaces.\n";
                return result;
            }
            catch (PathTooLongException)
            {
                errorMessage += "The import path parameter is longer than the system-defined maximum length.\n";
                return result;
            }

            //Check that the prescribed path actually exists and points to a file.

            if (!File.Exists(filePath))
            {
                errorMessage += "The import file path does not exist.\n";
                return result;
            }

            //Now try to open the file for reading.
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(filePath);
            }
            catch (Exception e)
            {
                errorMessage += "System.IO.File.OpenRead() threw an exception: " + e.Message + "\n";
                return result;
            }

            //Read the first byte.
            try
            {
                int b = fs.ReadByte();

                if (b == -1)
                {
                    errorMessage += "The import file is empty.";
                    return result;
                }
                result = true;
            }
            catch
            {
                errorMessage += "System.IO.FileStream.ReadByte() threw an IOException.\n";
                return result;
            }

            fs.Close();

            //If we reach here, the file creation attempt succeeded.
            errorMessage = "No errors detected.";
            return result;
        }




    }
}

```
