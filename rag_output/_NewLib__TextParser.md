# Documented File: TextParser.cs
**Repository Path:** `_NewLib\TextParser.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// Provides methods that parse a string character-by-character;
    /// it is designed to handle some of the more mundane tasks involved in 
    /// processing a string of text from start to finish.
    /// </summary>
    /// <remarks>
    /// This code is open source, see:  http://www.blackbeltcoder.com/Articles/strings/a-text-parsing-helper-class
    /// </remarks>
    public class TextParser
    {
        private string _text;
        private int _pos;

        public string Text { get { return _text; } }
        public int Position { get { return _pos; } }
        public int Remaining { get { return _text.Length - _pos; } }
        public static char NullChar = (char)0;

        /// <summary>
        /// Default constructor; this sets the working document (private string) to null
        /// and character position to zero.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TextParser()
        {
            Reset(null);
        }

        /// <summary>
        /// This constructor sets the working document equal to the prescribed string
        /// and the character position to zero.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TextParser(string text)
        {
            Reset(text);
        }

        /// <summary>
        /// This method resets the current position to the start of the working document.
        /// </summary>
        public void Reset()
        {
            _pos = 0;
        }

        /// <summary>
        /// This method sets the working document to a prescribed string and resets the 
        /// character position the start.
        /// </summary>
        /// <param name="text"></param>
        public void Reset(string text)
        {
            _text = (text != null) ? text : String.Empty;
            _pos = 0;
        }

        /// <summary>
        /// This method returns true if the current position is at the end of 
        /// the working document.
        /// </summary>
        public bool EndOfText
        {
            get { return (_pos >= _text.Length); }
        }

        /// <summary>
        /// This method returns the character at the current position, or a null character if we're
        /// at the end of the document.
        /// </summary>
        /// <returns>The character at the current position</returns>
        public char Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// This method returns the character at the specified number of characters beyond the current
        /// position, or a null character if the specified position is at the end of the
        /// document.
        /// </summary>
        /// <param name="ahead">The number of characters beyond the current position</param>
        /// <returns>The character at the specified position</returns>
        public char Peek(int ahead)
        {
            int pos = (_pos + ahead);
            if (pos < _text.Length)
                return _text[pos];
            return NullChar;
        }

        /// <summary>
        /// This method returns a substring from the specified position to the end of the document.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public string Extract(int start)
        {
            return Extract(start, _text.Length);
        }

        /// <summary>
        /// This method returns a substring of the document comprising all the
        /// characters between prescribed start and end positions.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string Extract(int start, int end)
        {
            return _text.Substring(start, end - start);
        }

        /// <summary>
        /// This method moves the current position ahead one character.
        /// </summary>
        public void MoveAhead()
        {
            MoveAhead(1);
        }

        /// <summary>
        /// This method moves the current position ahead by the specified number of characters.
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        public void MoveAhead(int ahead)
        {
            _pos = Math.Min(_pos + ahead, _text.Length);
        }

        /// <summary>
        /// This method moves to the next occurrence of the prescribed string.
        /// </summary>
        /// <param name="s">String to find</param>
        /// <param name="ignoreCase">Indicates if case-insensitive comparisons
        /// are used</param>
        public void MoveTo(string s, bool ignoreCase = false)
        {
            _pos = _text.IndexOf(s, _pos, ignoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (_pos < 0)
                _pos = _text.Length;
        }

        /// <summary>
        /// This method moves to the next occurrence of the prescribed character.
        /// </summary>
        /// <param name="c">Character to find</param>
        public void MoveTo(char c)
        {
            _pos = _text.IndexOf(c, _pos);
            if (_pos < 0)
                _pos = _text.Length;
        }

        /// <summary>
        /// This method moves to the next occurrence of any one of the prescribed
        /// characters.
        /// </summary>
        /// <param name="chars">Array of characters to find</param>
        public void MoveTo(char[] chars)
        {
            _pos = _text.IndexOfAny(chars, _pos);
            if (_pos < 0)
                _pos = _text.Length;
        }

        /// <summary>
        /// This method moves to the next occurrence of any character that is not one
        /// of the prescribed characters.
        /// </summary>
        /// <param name="chars">Array of characters to move past</param>
        public void MovePast(char[] chars)
        {
            while (IsInArray(Peek(), chars))
                MoveAhead();
        }

        /// <summary>
        /// This method returns true if the prescribed character exists in the specified
        /// character array.
        /// </summary>
        /// <param name="c">Character to find</param>
        /// <param name="chars">Character array to search</param>
        /// <returns></returns>
        protected bool IsInArray(char c, char[] chars)
        {
            foreach (char ch in chars)
            {
                if (c == ch)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This method moves the current position to the first character that is part of a newline.
        /// </summary>
        public void MoveToEndOfLine()
        {
            char c = Peek();
            while (c != '\r' && c != '\n' && !EndOfText)
            {
                MoveAhead();
                c = Peek();
            }
        }

        /// <summary>
        /// This method moves the current position to the next character that is not whitespace.
        /// </summary>
        public void MovePastWhitespace()
        {
            while (Char.IsWhiteSpace(Peek()))
                MoveAhead();
        }
    }
}

```
