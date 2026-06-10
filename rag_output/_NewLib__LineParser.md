# Documented File: LineParser.cs
**Repository Path:** `_NewLib\LineParser.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class provides methods that parse an array of strings line-by-line;
    /// it is designed to handle some of the more mundane tasks involved in 
    /// processing lines of text (e.g. read from a document) from start to finish.
    /// </summary>
    public class LineParser
    {
        private string[] mLines;
        private int mIndex;

        public string Line { get { return EndOfText ? null : mLines[mIndex]; } }
        public int Index { get { return mIndex; } }
        public int Remaining { get { return mLines.Length - mIndex; } }
        public static char NullChar = (char)0;

        /// <summary>
        /// Default constructor; this sets the working document (private string[]) to null
        /// and line index to zero.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public LineParser()
        {
            Reset(null);
        }

        /// <summary>
        /// This constructor sets the working document equal to the prescribed string[]
        /// and the line index to zero.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public LineParser(string[] lines)
        {
            Reset(lines);
        }

        /// <summary>
        /// This method resets the current index to the start of the working document.
        /// </summary>
        public void Reset()
        {
            mIndex = 0;
        }

        /// <summary>
        /// This method sets the working document to a prescribed array of strings (lines) and resets the 
        /// line index to the start.
        /// </summary>
        /// <param name="lines"></param>
        public void Reset(string[] lines)
        {
            mIndex = 0;
            mLines = lines;
        }

        /// <summary>
        /// This method returns true if the current index is at the end of 
        /// the working document.
        /// </summary>
        public bool EndOfText
        {
            get { return (mIndex >= mLines.Length); }
        }

        /// <summary>
        /// This method returns the line at the current index, or a null line if we're
        /// at the end of the document.
        /// </summary>
        /// <returns>The line at the current index</returns>
        public string Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// This method returns the line at the specified number of lines beyond the current
        /// index, or a null line if the specified index is at the end of the
        /// document.
        /// </summary>
        /// <param name="ahead">The number of lines beyond the current index</param>
        /// <returns>The line at the specified index</returns>
        public string Peek(int ahead)
        {
            int pos = (mIndex + ahead);

            return pos < mLines.Length ? mLines[pos] : null;
        }

        /// <summary>
        /// This method moves the current index ahead one line.
        /// </summary>
        public void MoveAhead()
        {
            MoveAhead(1);
        }

        /// <summary>
        /// This method moves the current index ahead by the specified number of lines.
        /// </summary>
        /// <param name="ahead">The number of lines to move ahead</param>
        public void MoveAhead(int ahead)
        {
            mIndex = Math.Min(mIndex + ahead, mLines.Length);
        }

        /// <summary>
        /// This method moves to the next occurrence of the prescribed string.
        /// </summary>
        /// <param name="s">String to find</param>
        /// <param name="ignoreCase">Indicates if case-insensitive comparisons are to be used.</param>
        public void MoveTo(string s, bool ignoreCase = false)
        {

        }

        /// <summary>
        /// This method returns true if the prescribed line exists, in whole or as a substring, in the specified
        /// line array.
        /// </summary>
        /// <param name="str">line to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <param name="lineIndex">the index of the line that the search string was found at</param>
        /// <param name="charPos">the character position in the line in which the search string was found</param>
        /// <returns></returns>
        public bool IsInLines(string str, bool caseSensitive, out int lineIndex, out int charPos)
        {
            // 'out'.
            lineIndex = -1;
            charPos = -1;

            if (String.IsNullOrEmpty(str)) return false;

            bool result = false;

            for (int index = 0; index < mLines.Length; index++)
            {
                string line = mLines[index];
                lineIndex = index;

                result = LineContains(line, str, caseSensitive, out charPos);

                if (result == true) break;
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the prescribed line exists in the specified
        /// line array.
        /// </summary>
        /// <param name="str">line to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <param name="lineIndex">the index of the line that the search string was found at</param>
        /// <returns></returns>
        public bool IsInLines(string str, bool caseSensitive, out int lineIndex)
        {
            // 'out'.
            lineIndex = -1;

            if (String.IsNullOrEmpty(str)) return false;

            bool result = false;

            for (int index = 0; index < mLines.Length; index++)
            {
                string line = mLines[index];
                lineIndex = index;

                int charPos;
                result = LineContains(line, str, caseSensitive, out charPos);

                if (result == true) break;
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the prescribed line exists in the specified
        /// line array.
        /// </summary>
        /// <param name="str">line to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <returns></returns>
        public bool IsInLines(string str, bool caseSensitive)
        {
            if (String.IsNullOrEmpty(str)) return false;

            bool result = false;

            for (int index = 0; index < mLines.Length; index++)
            {
                string line = mLines[index];

                int charPos;
                result = LineContains(line, str, caseSensitive, out charPos);

                if (result == true) break;
            }

            return result;
        }

        /// <summary>
        /// This method returns true if a prescribed line contains a prescribed substring otherwise false; the
        /// substring start character index is also returned.
        /// </summary>
        /// <param name="line">line to be searched</param>
        /// <param name="str">string to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <param name="charPos">the character position in the line in which the search string was found</param>
        /// <returns></returns>
        public static bool LineContains(string line, string str, bool caseSensitive, out int charPos)
        {
            // 'out'.
            charPos = -1;

            bool result = false;

            if (String.IsNullOrEmpty(line)) return false;
            if (String.IsNullOrEmpty(str)) return false;

            if (caseSensitive)
            {
                result = line.Contains(str);

                if (result == true) charPos = line.IndexOf(str);
            }
            else // case insensitive.
            {
                str = str.ToUpper();
                line = line.ToUpper();

                result = line.Contains(str);

                if (result == true) charPos = line.IndexOf(str);
            }

            return result;
        }

        /// <summary>
        /// This method returns true if a prescribed line contains a prescribed substring otherwise false.
        /// </summary>
        /// <param name="line">prescribed line to be searched</param>
        /// <param name="str">string to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <returns></returns>
        public static bool LineContains(string line, string str, bool caseSensitive)
        {
            bool result = false;

            if (String.IsNullOrEmpty(line)) return false;
            if (String.IsNullOrEmpty(str)) return false;

            if (caseSensitive)
            {
                result = line.Contains(str);
            }
            else // case insensitive.
            {
                str = str.ToUpper();
                line = line.ToUpper();

                result = line.Contains(str);
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the current line contains a prescribed substring otherwise false; the
        /// substring start character index is also returned.
        /// </summary>
        /// <param name="str">line to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <param name="charPos">the character position in the line in which the search string was found</param>
        /// <returns></returns>
        public bool LineContains(string str, bool caseSensitive, out int charPos)
        {
            return LineContains(Line, str, caseSensitive, out charPos);
        }

        /// <summary>
        /// This method returns true if the current line contains a prescribed substring otherwise false; the
        /// substring start character index is also returned.
        /// </summary>
        /// <param name="str">line to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <returns></returns>
        public bool LineContains(string str, bool caseSensitive)
        {
            int charPos;
            return LineContains(Line, str, caseSensitive, out charPos);
        }

        /// <summary>
        /// This method returns true if the current line contains a prescribed substring otherwise false; 
        /// the comparison ignores whitespace characters in the current line and the substring being searched for.
        /// </summary>
        /// <param name="str">line to find</param>
        /// <param name="caseSensitive">prescribed whether the search is to be case sensitive, or not</param>
        /// <returns></returns>
        public bool LineContainsIgnoreWhitespace(string str, bool caseSensitive)
        {
            if (String.IsNullOrEmpty(str)) return false;
            if (String.IsNullOrEmpty(Line)) return false;

            int charPos;

            string strIWS = Regex.Replace(str, @"\s", "");
            string lineIWS = Regex.Replace(Line, @"\s", "");

            return LineContains(lineIWS, strIWS, caseSensitive, out charPos);
        }

        /// <summary>
        /// This method moves the current index to the next line that is not all whitespace.
        /// </summary>
        public void MovePastWhitespace()
        {
            while (String.IsNullOrWhiteSpace(Peek())) MoveAhead();
        }
    }
}

```
