# Documented File: CharParser.cs
**Repository Path:** `_NewLib\CharParser.cs`
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
    public class CharParser
    {
        private char[] mLine;
        private int mCurrentIndex;
        private int mFinalIndex;
        private char mCurrentChar;
        private char mPreviousChar;
        private char mNextChar;

        public char PreviousChar { get { return mPreviousChar; } }
        public char ThisChar { get { return mCurrentChar; } }
        public char NextChar { get { return mNextChar; } }

        private CharParser() { }

        public CharParser(string line)
        {
            mLine = line.ToCharArray();
            mCurrentIndex = 0;
            mFinalIndex = mLine.Length - 1;
            mCurrentChar = mLine[0];
            mPreviousChar = (char)0;
            mNextChar = (1 > mFinalIndex) ? (char)0 : mLine[1];
        }

        /// <summary>
        /// This method returns true if the current position is the EOL; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool EOL()
        {
            return (mCurrentIndex < mFinalIndex) ? false : true;
        }

        /// <summary>
        /// This method advances the current position by one character.
        /// </summary>
        /// <returns></returns>
        public bool Advance()
        {
            if (mCurrentIndex == mFinalIndex) return false;

            mCurrentIndex = (mCurrentIndex < mFinalIndex) ? mCurrentIndex + 1 : mCurrentIndex;

            mPreviousChar = mCurrentChar;

            mCurrentChar = mNextChar;

            mNextChar = (mCurrentIndex == mFinalIndex) ? (char)0 : mLine[mCurrentIndex + 1];

            return true;
        }

    }
}

```
