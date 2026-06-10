# Documented File: FileScanner.cs
**Repository Path:** `_NewLib\FileScanner.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class is a light wrapper around an embedded StringReader object to read multi-line
    /// data from a text file; this class provides Open(), HasNextLine(), GetNextLine() and Close()
    /// functionality.
    /// </summary>
    public class FileScanner
    {
        string _currentLine;
        StringReader _stringReader;

        /// <summary>
        /// The primitive constructor.
        /// </summary>
        public FileScanner() 
        {
            _currentLine = null;
            _stringReader = null;
        }

        /// <summary>
        /// This method instantiates a StringReader object that contains the entire contents of a
        /// prescribed text file and sets the _currentLine as the first line.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public int Open(string sourcePath)
        {
            int retVal = -1;

            try
            {
                StreamReader streamReader = new StreamReader(sourcePath, Encoding.UTF8);
                string fileAsOneString = streamReader.ReadToEnd();
                streamReader.Close();

                _stringReader = new StringReader(fileAsOneString);
                _currentLine = _stringReader.ReadLine();

                retVal = 0;
            }
            catch
            {
                // Exception raised trying to read the file.
            }

            return retVal;
        }

        /// <summary>
        /// This method returns true if the embedded StringReader object has at least one more
        /// non-null line of text after the current line.
        /// </summary>
        /// <returns></returns>
        public bool HasNextLine()
        {
            if (_currentLine == null) return false;
            else return true;
        }

        /// <summary>
        /// This method returns the current line of text from the embedded StringReader object.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentLine()
        {
            string result = _currentLine;
            _currentLine = _stringReader.ReadLine();
            return result;
        }

        /// <summary>
        /// This method disposes of the embedded StringReader object.
        /// </summary>
        public void Close() { _stringReader.Close(); }
    } // class  

} // namespace

```
