# Documented File: RegistryKey.cs
**Repository Path:** `Tools\RegistryKey.cs`
**Primary Layer:** `Tools`
**Namespace:** `Tools`

## Source Code Representation
```csharp
﻿using _NewLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class RegistryKey
    {
        private enum Mode {READ, WRITE};
        private static Mode mMode = Mode.READ;
        private static string mKeyValue = "";
        private static string mKeyFullPath = "";
        private static string mKeyDirPath = "";
        private static string mKeyName = "";

        public static void Go(string[] args)
        {
            try
            {
                ParseCommandLineArgs(args);

                switch (mMode)
                {
                    case Mode.READ:

                        mKeyValue = Read_HKEY_LOCAL_MACHINE(mKeyFullPath);
                        Console.Write("\nKey:   HKEY_LOCAL_MACHINE\\{0}\n\nValue: {1}\n", mKeyFullPath, mKeyValue);
                        break;

                    case Mode.WRITE:

                        Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(mKeyDirPath);
                        regKey.SetValue(mKeyName, mKeyValue);

                        //As a check, read the key that was just written to and compare with the command line arg.
                        string target = Read_HKEY_LOCAL_MACHINE(mKeyFullPath);

                        if (target.Equals(mKeyValue))
                        {
                            Console.Write("\nKey:   HKEY_LOCAL_MACHINE\\{0}\n\nValue: {1}\n", mKeyFullPath, mKeyValue);
                        }
                        else
                        {
                            Console.Write("\nERROR: attempt to assign value to the prescribed registry key failed.");
                        }

                        regKey.Close();
                        break;

                    default:
                        Console.Write("\n\nERROR: unknown Mode.");
                        break;
                }

            }
            catch (Exception e)  //just for demonstration...it's always best to handle specific exceptions
            {
                Console.Write("\n\nRead_HKEY_LOCAL_MACHINE : ERROR: exception: \n\n{0}", e.Message);

                if (e.Message.ToLower().StartsWith("access"))
                {
                    Console.Write("\n\nOpen a Windows Command Prompt as ADMINISTRATOR and try again.\n");
                }
            }

        }

        /// <summary>
        /// This method returns the string value of the registry key at the prescribed path
        /// below computer&#92;HKEY_LOCAL_MACHINE.
        /// </summary>
        /// <param name="keyFullPath"></param>
        /// <returns>
        /// The value of the string at the prescribed key location below HKEY_LOCAL_MACHINE; if
        /// the returned value is null the read attempt failed.
        /// </returns>
        public static string Read_HKEY_LOCAL_MACHINE(string keyFullPath)
        {
            string readValue = null;

            string keyDirPath = Path.GetDirectoryName(keyFullPath);
            string keyName = Path.GetFileName(keyFullPath);

            try
            {
                using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(keyDirPath))
                {
                    if (key != null)
                    {

                        Object obj = key.GetValue(keyName);
                        if (obj != null)
                        {
                            readValue = (string) obj;
                        }
                        else
                        {
                            Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR : obj == null");
                        }
                    }
                    else
                    {
                        Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR: key == null");
                    }
                }
            }
            catch (Exception e)  
            {
                Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR: exception: {0}", e.Message);
            }

            return readValue;
        }

        public static string UnEscapeForUpperCase(string arg)
        {
            string correctedArg = "";

                char[] chars = arg.ToCharArray();
                bool convertToUpperCase = false;
                string str = "";

                for (int j = 0; j < chars.Length; j++)
                {
                    if (chars[j] == '\\')
                    {
                        convertToUpperCase = true;
                    }
                    else
                    {
                        char c = chars[j];

                        if (convertToUpperCase)
                        {
                            c = Char.ToUpper(c);
                            convertToUpperCase = false;
                        }

                        str += c;
                    }
                }

                correctedArg = str;

            return correctedArg;
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program reads/writes a value from/to a registry key under HKEY_LOCAL_MACHINE.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: RegistryKey <-r|-w> <keyPath> [keyValue]");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        -r          : read the value stored at keyPath.");
            Console.Write("\r\n        -w          : write (set) the value of keyPath.");
            Console.Write("\r\n        keyPath     : the full path of the key under HKEY_LOCAL_MACHINE.");
            Console.Write("\r\n        keyValue    : for -w prescribes the value to be set.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n       Tools RegistryKey -w SOFTWARE\\foo\\bar \\Bananarama");
            Console.Write("\r\n       Tools RegistryKey -r SOFTWARE\\foo\\bar");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. For Windows console applications command line arguments are always passed");
            Console.Write("\r\n          to the program as lowercase. ");
            Console.Write("\r\n       2. To preserve an uppercase character (as might be required for a password)");
            Console.Write("\r\n          preceed it with the escape character '\\'. ");
            Console.Write("\r\n          e.g. 'PassWord' should be given on the command line as '\\Pass\\Word' .");
            Console.Write("\r\n\r\n Build: {0}\r\n", Info.BuildMetaData);
        }

        /// <summary>
        /// This method parses the user-prescribed command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseCommandLineArgs(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("\r\nERROR: Invalid command line args");
                WriteUsageToConsole();
                Application.ExitQuietly(1);
            }

            // Create separate lists of 'flag' args prefixed with '-' and those that
            // are not.
            List<string> flagArgs = new List<string>();
            List<string> regularArgs = new List<string>();

            foreach (string arg in args)
            {
                if (Strings.IsNumeric(arg))
                {
                    regularArgs.Add(arg);
                }
                else if (arg.StartsWith("-"))
                {
                    flagArgs.Add(arg);
                }
                else
                {
                    regularArgs.Add(arg);
                }
            }

            // Parse the flags.
            if (flagArgs.Count != 1)
            {
                Console.Write("\r\nERROR: Invalid command line args");
                WriteUsageToConsole();
                Application.ExitQuietly(6);
            }

            foreach (string arg in flagArgs)
            {
                // Check that we don't just have a minus character.
                if (arg.Length == 1)
                {
                    Console.Write("\r\nERROR: Invalid flag: '{0}'", arg);
                    WriteUsageToConsole();
                    Application.ExitQuietly(2);
                }

                // Process the flags.
                string flag = Strings.DropFirstChar(arg).ToUpper();
                switch (flag)
                {
                    case "R":
                        mMode = Mode.READ;
                        break;
                    case "W":
                        mMode = Mode.WRITE;
                        break;
                    default:
                        Console.Write("\r\nERROR: Invalid flag: {0}\r\n", arg);
                        WriteUsageToConsole();
                        Application.ExitQuietly(3);
                        break;
                }

            }

            // Parse the regular arguments.
            if (mMode == Mode.READ)
            {
                if (regularArgs.Count == 1)
                {
                    mKeyFullPath = regularArgs[0];
                }
                else
                {
                    Console.Write("\r\nERROR: Invalid command line args");
                    WriteUsageToConsole();
                    Application.ExitQuietly(4);
                }
            }
            else if (mMode == Mode.WRITE)
            {
                if (regularArgs.Count == 2)
                {
                    mKeyFullPath = regularArgs[0];
                    mKeyValue = UnEscapeForUpperCase(regularArgs[1]);
                }
                else
                {
                    Console.Write("\r\nERROR: Invalid number of arguments.\r\n");
                    WriteUsageToConsole();
                    Application.ExitQuietly(5);
                }
            }
            else
            {
                Console.Write("\r\nERROR: Parsing of command line failed.\r\n");
                WriteUsageToConsole();
                Application.ExitQuietly(5);
            }

            mKeyDirPath = Path.GetDirectoryName(mKeyFullPath);
            mKeyName = Path.GetFileName(mKeyFullPath);

            //Console.Write("\n");
            //Console.Write("\nmKeyFullPath = " + mKeyFullPath);
            //Console.Write("\nmKeyDirPath  = " + mKeyDirPath);
            //Console.Write("\nmKeyName     = " + mKeyName);
            //Console.Write("\nmKeyValue    = " + mKeyValue);
            //Console.Write("\nmMode        = " + mMode);
        }





    }
}

```
