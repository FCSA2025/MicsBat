# Documented File: CodeParser.cs
**Repository Path:** `_NewLib\CodeParser.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace _NewLib
{
    /// <summary>
    /// This class provides a set of static methods that are useful for parsing
    /// C# source code.
    /// </summary>
    public class CodeParser
    {
        private enum State { FREE, C_COMMENT, CPP_COMMENT, STRING }
        private enum Event { C_START, C_END, CPP_START, BSP_STR, QUOTE, OTHER }
        private const char ENQ = (char)5;
        private const char ACK = (char)6;
        private const char BEL = (char)7;
        private const char BSP = (char)8;
        private const char QUOTE = '"';

        private static String ENQ_STR = Char.ToString(ENQ);
        private static String ACK_STR = Char.ToString(ACK);
        private static String BEL_STR = Char.ToString(BEL);
        private static String BSP_STR = Char.ToString(BSP);

        private const String TAG = "CodeParser: ";

        private static char previousChar;

        /// <summary>
        /// This method inputs a single character and returns an Event object that
        /// is used to simplify the detection and parsing of C and C++ comment
        /// strings using a 'state-machine' approach.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static Event IdentifyEvent(char c)
        {
            //Only worry about non-trivial character 'events'.
            Event eVent = Event.OTHER;

            switch (c)
            {
                case (ENQ):
                    eVent = Event.C_START;
                    break;
                case (ACK):
                    eVent = Event.C_END;
                    break;
                case (BEL):
                    eVent = Event.CPP_START;
                    break;
                case (BSP):
                    eVent = Event.BSP_STR;
                    break;
                case (QUOTE):
                    eVent = Event.QUOTE;
                    break;
                default:
                    break;
            }
            return eVent;
        }

        /// <summary>
        /// This method is called for each successive character in a block of valid C# source code
        /// and accumulates a list of characters that remain after all C and C++ comments have been
        /// removed; oit implements a state-machine using Event transitions detected in IdentifyEvent().
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="charList"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static State StripCommentsStateMachine(State currentState, ref List<char> charList, char c)
        {
            //Only worry about non-trivial state transitions
            State newState = currentState;

            //Identify the event type.
            Event eVent = IdentifyEvent(c);

            switch (currentState)
            {
                case (State.FREE):
                    switch (eVent)
                    {
                        case (Event.C_START):
                            newState = State.C_COMMENT;
                            break;
                        case (Event.C_END):
                            //This State/Event combination should never occur for
                            //a source file that can be successfully compiled.
                            charList.Add(c);
                            break;
                        case (Event.CPP_START):
                            newState = State.CPP_COMMENT;
                            break;
                        case (Event.QUOTE):
                            if (previousChar != '\'')
                            {
                                newState = State.STRING;
                            }
                            charList.Add(c);
                            break;
                        default:
                            charList.Add(c);
                            break;
                    }
                    break;
                case (State.C_COMMENT):
                    switch (eVent)
                    {
                        case (Event.C_END):
                            newState = State.FREE;
                            break;
                        default:
                            break;
                    }
                    break;
                case (State.CPP_COMMENT):
                    switch (eVent)
                    {
                        case (Event.BSP_STR):
                            newState = State.FREE;
                            charList.Add(c);
                            break;
                        default:
                            break;
                    }
                    break;
                case (State.STRING):
                    switch (eVent)
                    {
                        case (Event.QUOTE):
                            newState = State.FREE;
                            charList.Add(c);
                            break;
                        default:
                            charList.Add(c);
                            break;
                    }
                    break;
                default:
                    break;
            }

            return newState;
        }

        /// <summary>
        /// This method inputs a string of valid C# source code text and returns
        /// the same code fragment but with all C and C++ style comments removed.
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns></returns>
        public static String StripComments(String inStr)
        {
            String result = inStr;
            String pattern;

            //Convert instances of the dyad // to the single char BEL.
            //We need to do this first to avoid confusion handling //*
            pattern = "[/][/]";
            result = Regex.Replace(result, pattern, BEL_STR);
            //Convert instances of the dyad /* to the single char ENQ.
            pattern = "[/][*]";
            result = Regex.Replace(result, pattern, ENQ_STR);
            //Convert instances of the dyad */ to the single char ACK.
            pattern = "[*][/]";
            result = Regex.Replace(result, pattern, ACK_STR);
            //Convert instances of the dyad \r\n to the single char BSP.
            pattern = "[\r][\n]";
            result = Regex.Replace(result, pattern, BSP_STR);

            //Initialize the state to FREE.
            State state = State.FREE;

            //Create a char[] from the pre-processed string.
            char[] charArray = result.ToCharArray();

            //Create a List<char> to accumulate the results of comment stripping.
            List<char> charList = new List<char>();

            foreach (char c in charArray)
            {
                state = StripCommentsStateMachine(state, ref charList, c);
            }

            result = new string(charList.ToArray());

            //Recover instances of the dyad /* from the single char ENQ.
            result = result.Replace(ENQ_STR, "/*");
            //Recover instances of the dyad */ from the single char ACK.
            result = result.Replace(ACK_STR, "*/");
            //Recover instances of the dyad // from the single char BEL.
            result = result.Replace(BEL_STR, "//");
            //Recover instances of the dyad \r\n from the single char BSP.
            result = result.Replace(BSP_STR, "\r\n");

            result = TidyUpLineBreaks(result);

            return result;
        }

        /// <summary>
        /// This method is called for each successive character in a block of valid C# source code
        /// and accumulates a list of characters where any '{' or '}' characters embedded within 
        /// C and/or C++ comments and/or string literals have been converted to 'safe' the characters '<' and '>' 
        /// to facilitate robust parsing of curlyBracket code blocks.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="charList"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static State SanitizeCommentsStateMachine(State currentState, ref List<char> charList, char c)
        {
            //Only worry about non-trivial state transitions
            State newState = currentState;

            //Identify the event type.
            Event eVent = IdentifyEvent(c);

            //This will hold the santized character.
            char r = c;

            switch (currentState)
            {
                case (State.FREE):
                    switch (eVent)
                    {
                        case (Event.C_START):
                            newState = State.C_COMMENT;
                            break;
                        case (Event.C_END):
                            //This State/Event combination should never occur for
                            //a source file that can be successfully compiled.
                            break;
                        case (Event.CPP_START):
                            newState = State.CPP_COMMENT;
                            break;
                        case (Event.QUOTE):
                            if (previousChar != '\'')
                            {
                                newState = State.STRING;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case (State.C_COMMENT):
                    switch (eVent)
                    {
                        case (Event.C_END):
                            newState = State.FREE;
                            break;
                        default:
                            if (c == '{') r = '<';
                            if (c == '}') r = '>';
                            break;
                    }
                    break;
                case (State.CPP_COMMENT):
                    switch (eVent)
                    {
                        case (Event.BSP_STR):
                            newState = State.FREE;
                            break;
                        default:
                            if (c == '{') r = '<';
                            if (c == '}') r = '>';
                            break;
                    }
                    break;
                case (State.STRING):
                    switch (eVent)
                    {
                        case (Event.QUOTE):
                            newState = State.FREE;
                            break;
                        default:
                            if (c == '{') r = '<';
                            if (c == '}') r = '>';
                            break;
                    }
                    break;
                default:
                    break;
            }
            charList.Add(r);
            previousChar = c;
            return newState;
        }


        /// <summary>
        /// This method parses the input string and replaces any '{' or '}' characters
        /// that lurk inside comments and/or string literals that would
        /// confound curlyBracket block detection.
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns></returns>
        public static String SanitizeComments(String inStr)
        {
            String result = inStr;
            String pattern;

            previousChar = '\0';

            //Convert instances of the dyad // to the single char BEL.
            //We need to do this first to avoid confusion handling //*
            pattern = "[/][/]";
            result = Regex.Replace(result, pattern, BEL_STR);
            //Convert instances of the dyad /* to the single char ENQ.
            pattern = "[/][*]";
            result = Regex.Replace(result, pattern, ENQ_STR);
            //Convert instances of the dyad */ to the single char ACK.
            pattern = "[*][/]";
            result = Regex.Replace(result, pattern, ACK_STR);
            //Convert instances of the dyad \r\n to the single char BSP.
            pattern = "[\r][\n]";
            result = Regex.Replace(result, pattern, BSP_STR);

            //Initialize the state to FREE.
            State state = State.FREE;

            //Create a char[] from the pre-processed string.
            char[] charArray = result.ToCharArray();

            //Create a List<char> to accumulate the results of comment stripping.
            List<char> charList = new List<char>();

            foreach (char c in charArray)
            {
                state = SanitizeCommentsStateMachine(state, ref charList, c);
            }

            result = new string(charList.ToArray());

            //Recover instances of the dyad /* from the single char ENQ.
            result = result.Replace(ENQ_STR, "/*");
            //Recover instances of the dyad */ from the single char ACK.
            result = result.Replace(ACK_STR, "*/");
            //Recover instances of the dyad // from the single char BEL.
            result = result.Replace(BEL_STR, "//");
            //Recover instances of the dyad \r\n from the single char BSP.
            result = result.Replace(BSP_STR, "\r\n");

            result = TidyUpLineBreaks(result);

            return result;
        }

        /// <summary>
        /// This method inputs a string and returns a string in which any whitespace before
        /// end-of-line is removed, the maximum number of consecutive blank lines is two, and
        /// any blank lines at the start of the string are removed.
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns></returns>
        public static String TidyUpLineBreaks(String inStr)
        {
            String result = inStr;

            //Remove instances of spaces and tabs before \r\n.
            String pattern = "([ ]|\t)*\r\n";
            result = Regex.Replace(result, pattern, "\r\n");

            //Replace 3, or more, contiguous "\r\n" by "\r\n\r\n"
            //This preserves isolated single blank lines. 
            pattern = "\r\n\r\n(\r\n)+";
            result = Regex.Replace(result, pattern, "\r\n\r\n");

            //Tidy up any \r\n at start of the string.
            pattern = "^(\r\n)*";
            result = Regex.Replace(result, pattern, "");

            return result;
        }

        /*
		 * Assumes that any C++ style comments have already been removed.
		 */
        /// <summary>
        /// This method inputs a string and returns a string in which all non-printing
        /// characters are converted to space characters and multiple consecutive space characters
        /// are reduced to just one; this method assumes that any C or C++ style comments have already been removed.
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string RemoveNonPrintingChars(string inputStr)
        {
            string str = inputStr;
            string pattern = null;
            string lineSep = Environment.NewLine;

            //Replace all non-printing characters with the space character.
            //\s is equivalent to [ \f\n\r\t].
            //Consequently all line-breaks are removed.
            pattern = "\\s+";
            Regex R = new Regex(pattern);
            str = R.Replace(str, " ");

            //Replace multiple-spaces by single-space.
            pattern = "[ ]+";
            R = new Regex(pattern);
            str = R.Replace(str, " ");

            return str;
        }

        /// <summary>
        /// This method inputs a string and returns a string in which all tab
        /// characters are converted to space characters and multiple consecutive space characters
        /// are reduced to just one.
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string ReplaceTabsWithSpaces(string inputStr)
        {
            string str = inputStr;
            string pattern = "\\t";

            str = Regex.Replace(str, pattern, " ");

            //Replace multiple-spaces by single-space.
            pattern = "[ ]+";
            str = Regex.Replace(str, pattern, " ");

            return str;
        }

        /// <summary>
        /// This method inputs a string of valid C# source code text and detects and returns the first
        /// entire code block, delimited by two precribed characters, at or following a prescribed character start index.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="leftDelim"></param>
        /// <param name="rightDelim"></param>
        /// <returns></returns>
        public static String GetNextBlock(String str, int startIndex, char leftDelim, char rightDelim)
        {
            string result = null;

            bool foundFirstLB = false;
            bool foundLastRB = false;
            int index = startIndex;
            char[] charArray = str.ToCharArray();
            int level = 0;

            while ((!foundLastRB) && (index < str.Length))
            {
                char c = charArray[index++];

                if ((c == leftDelim) && (!foundFirstLB))
                {
                    foundFirstLB = true;
                    level = 1;
                    result = leftDelim.ToString();
                }
                else if ((level == 1) && (c == rightDelim))
                {
                    foundLastRB = true;
                    result += rightDelim.ToString();
                }
                else if (foundFirstLB && !foundLastRB)
                {
                    result += c.ToString();
                    if (c == leftDelim) level = level + 1;
                    if (c == rightDelim) level = level - 1;
                }

            } // while

            if (!(foundFirstLB && foundLastRB)) result = null;

            return result;
        }

        /// <summary>
        /// This method inputs a string of C# source code text and detects and returns a string in which all 
        /// code blocks, delimited by two precribed characters, have been removed.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="leftDelim"></param>
        /// <param name="rightDelim"></param>
        /// <returns></returns>
        public static String RemoveBlocks(String str, char leftDelim, char rightDelim)
        {
            string result = "";

            int index = 0;
            char[] charArray = str.ToCharArray();
            int level = 0;

            while (index < str.Length)
            {
                bool foundFirstLB = false;
                bool foundLastRB = false;

                while ((!foundLastRB) && (index < str.Length))
                {
                    char c = charArray[index++];

                    if ((c == leftDelim) && (!foundFirstLB))
                    {
                        foundFirstLB = true;
                        level = 1;
                    }
                    else if ((level == 1) && (c == rightDelim))
                    {
                        foundLastRB = true;
                    }
                    else if (foundFirstLB && !foundLastRB)
                    {
                        if (c == leftDelim) level = level + 1;
                        if (c == rightDelim) level = level - 1;
                    }
                    else
                    {
                        result += c;
                    }

                } // while
            } //while

            //if (!(foundFirstLB && foundLastRB)) result = null;

            return result;
        }

        /// <summary>
        /// This method inputs a string containing C# code and returns an intelligent
        /// estimate of Source Lines of Code (SLOC) count; all comment lines are counted
        /// but multiple consecutive blank lines are excluded from the count.
        /// </summary>
        /// <param name="curlyBlock"></param>
        /// <param name="slocBlock"></param>
        /// <returns></returns>
        public static int GetSlocCount(String curlyBlock, out string slocBlock)
        {
            int sloc = 0;
            slocBlock = "";

            if (curlyBlock == null)
            {
                // No further processing is possible.
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(curlyBlock))
                {
                    // Only allow isolated single blank lines.
                    slocBlock = CodeParser.TidyUpLineBreaks(curlyBlock);

                    // Count newline characters.
                    sloc = CodeParser.CountCharacters(slocBlock, '\n');

                    // Account for the final }.
                    sloc += 1;
                }
                else
                {
                    // Assume this is a single 'inline' function definition.
                    sloc = 1;
                }
            }

            return sloc;
        }


        /// <summary>
        /// This method inputs a string and returns a count of many instances of a 
        /// prescribed character are present within the string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="charToCount"></param>
        /// <returns></returns>
        public static int CountCharacters(string str, char charToCount)
        {
            int count = 0;
            if (!String.IsNullOrEmpty(str))
            {
                char[] charArray = str.ToArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    if (charArray[i] == charToCount) count++;
                }
            }
            return count;
        }


        /// <summary>
        /// This method returns the contents of a file as a single string formed by concatanating
        /// individually trimmed lines of text; blank lines are discarded; \\r\\n characters are retained.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFileAsTrimmedString(string path)
        {
            StringBuilder fileContents = new StringBuilder();
            string fileContentsStr = "";
            FileScanner scanner;
            string lineSeparator = Environment.NewLine;

            scanner = new FileScanner();

            if (scanner.Open(path) == Constant.SUCCESS)
            {
                try
                {
                    while (scanner.HasNextLine())
                    {
                        string str = scanner.GetCurrentLine().Trim();

                        fileContents.Append(str + lineSeparator);
                    }
                }
                catch { }
                finally
                {
                    scanner.Close();
                }

                fileContentsStr = fileContents.ToString();
            }

            return fileContentsStr;
        }



    } //class
} //namespace

```
