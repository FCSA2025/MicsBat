# Documented File: LineOfChars.cs
**Repository Path:** `_NewLib\LineOfChars.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates the data and methods that model the notion
    /// of a single line (sequence) of characters that has a starting character, an
    /// end character and the concept of 'advance', 'last', 'this' and 'next'
    /// character positions.
    /// </summary>
    public class LineOfChars
    {
        public char[] mLine;
        public bool mGoodLine = false;
        public int mLength;
        public int mPos;
        public char mLast;
        public char mThis;
        public char mNext;

        public const char NULL_CHAR = (char)0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private LineOfChars() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="str"></param>
        public LineOfChars(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                mGoodLine = false;
            }
            else
            {
                mLine = str.ToCharArray();
                mGoodLine = true;
                mLength = mLine.Length;
                mPos = 0;
                mLast = NULL_CHAR;
                mThis = mLine[0];
                mNext = mLength > 1 ? mLine[1] : NULL_CHAR;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="chars"></param>
        public LineOfChars(char[] chars)
        {
            if (chars == null || chars.Length == 0)
            {
                mGoodLine = false;
            }
            else
            {
                // Need to deep-copy the array of chars to prevent corruption.
                mLine = new char[chars.Length];
                chars.CopyTo(mLine, 0);
                mGoodLine = true;
                mLength = mLine.Length;
                mPos = 0;
                mLast = NULL_CHAR;
                mThis = mLine[0];
                mNext = mLength > 1 ? mLine[1] : NULL_CHAR;
            }
        }

        /// <summary>
        /// This method advances (increments by one) the current position within
        /// this line of characters; if the advance succeeds the method returns
        /// true but if the current position is already the end character then
        /// it returns false.
        /// </summary>
        /// <returns></returns>
        public bool Advance()
        {
            bool result = false;

            if (mPos == mLength - 1)
            {
                result = false;
            }
            else
            {
                mPos++;
                mLast = mThis;
                mThis = mNext;
                mNext = mPos <= mLength - 1 ? mLine[mLength - 1] : NULL_CHAR;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This method returns a string comprising 3 characters equal to
        /// "last" + "current" + "next".
        /// </summary>
        /// <returns></returns>
        public string LastAndThisAndNext()
        {
            if (!mGoodLine) return "";

            string str = "";
            str += mLast;
            str += mThis;
            str += mNext;
            return str;
        }

        /// <summary>
        /// This method returns a string comprising 2 characters equal to
        /// "last" + "current".
        /// </summary>
        /// <returns></returns>
        public string LastAndThis()
        {
            if (!mGoodLine) return "";

            string str = "";
            str += mLast;
            str += mThis;
            return str;
        }

        /// <summary>
        /// This method returns a string comprising 2 characters equal to
        /// "current" + "next".
        /// </summary>
        /// <returns></returns>
        public string ThisAndNext()
        {
            if (!mGoodLine) return "";

            string str = "";
            str += mThis;
            str += mNext;
            return str;
        }


    }
}

```
