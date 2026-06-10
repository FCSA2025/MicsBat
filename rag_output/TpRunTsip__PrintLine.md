# Documented File: PrintLine.cs
**Repository Path:** `TpRunTsip\PrintLine.cs`
**Primary Layer:** `TpRunTsip`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TpRunTsip
{
    /// <summary>
    /// Provides methods that facilitate the production of textual reports
    /// by writing fields in a line-by-line sequence, in which individual fields are written
    /// to prescribed row start and/or end positions, and then passing the accumulated
    /// string to a TextWriter object, to be written to file.
    /// </summary>
    public class PrintLine
    {
        private const char SPACE = ' ';
        private int m_lineLen;
        private char[] m_lineBuff;
        private TextWriter m_outFile;
        private int m_page;
        private int m_line;
        private int m_linesPerPage;
        private int m_cursor;

        public int PageNumber
        {
            get { return m_page; }
        }

        /// <summary>
        /// The default constructor: sets the default line length.
        /// </summary>
        /// <returns></returns>
        public PrintLine() : this(Constant.DEFAULT_LINE_SIZE)
        {
        }

        /// <summary>
        /// Constructor: sets the line length to a prescribed number
        /// of characters.
        /// </summary>
        /// <param name="nLen"> - rescribed line length (number of characters).</param>
        /// <returns></returns>
        public PrintLine(int nLen)
        {

            m_lineLen = nLen;

            m_lineBuff = new char[m_lineLen];
            FillWithSpaces(ref m_lineBuff);


            m_outFile = Console.Out;

            m_page = 1;
            m_line = 0;
            m_linesPerPage = Constant.DEFAULT_LINES_PER_PAGE;
            m_cursor = 0;
        }

        /// <summary>
        /// Fills a prescribed character array with blank spaces.
        /// </summary>
        /// <param name="buffer"> - prescribed character array</param>
        private void FillWithSpaces(ref char[] buffer)
        {
            // Fill with space characters.
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = SPACE;
            }
        }

        /// <summary>
        /// Sets the class's private Textwriter object to be used for
        /// writing text to file.
        /// </summary>
        /// <param name="swNext"> - TextWriter object.</param>
        /// <returns></returns>
        public TextWriter OutFile(TextWriter swNext)
        {
            TextWriter swTemp = m_outFile;
            m_outFile = swNext;
            return swTemp;
        }


        /****************************************************************************\
        *
        *		Output a line buffer.
        *
        \****************************************************************************/

        /// <summary>
        /// Writes a single line of accumulated text to the class's private TextWriter
        /// object.
        /// </summary>
        /// <param name=""></param>
        public void Output()
        {
            Output(1);
        }

        /// <summary>
        /// Writes multiple lines of text to the class's private TextWriter
        /// object; the first line is the current accumulated text line, the 
        /// subsequent lines are all blank.
        /// </summary>
        /// <param name=""> - number of lines to be written to TextWriter.</param>
        public void Output(int nLines)
        {
            //	Trim:
            int nInd = m_lineLen;

            m_line++;
            if (m_line >= m_linesPerPage)
            {
                PageHeader();
            }

            string str = new string(m_lineBuff);
            str = str.TrimEnd(SPACE);


            if (str == "")
            {
                str = " ";
            }

            m_outFile.Write(str + "\r\n");
            m_outFile.Flush();
            FillWithSpaces(ref m_lineBuff);
            m_cursor = 0;

            //	If we specify more than one line, the following lines will be blank lines...
            while (--nLines > 0)
            {
                m_line++;
                if (m_line >= m_linesPerPage)
                {
                    PageHeader();
                    break;  //	If we go over a page end, then cancel further skips.
                }
                m_outFile.Write("\n");
                m_outFile.Flush();
            }

        }

        /// <summary>
        /// Place a string in the line buffer starting at a precribed position, If the position
        /// is -1, then place it adjacent to the end of the last placement.  Check for
        /// line overflow, and truncate if necessary.
        /// </summary>
        /// <param name="nLeft"> - leftmost character start-position.</param>
        /// <param name="cString"> - the string to be written.</param>
        public void LeftAt(int nLeft, string cString)
        {
            int nLen = cString.Length;

            if (nLeft == -1)
            {
                nLeft = m_cursor;
            }

            if (nLen + nLeft >= m_lineLen)
            {
                nLen = m_lineLen - nLeft;
            }

            for (int i = 0; i < nLen; i++)
            {
                m_lineBuff[nLeft + i] = cString[i];
            }

            m_cursor = nLeft + nLen;
        }

        /// <summary>
        /// Place a string so that it terminates as a given column in the line 
        /// buffer.  Check for underflow, and if the column is -1, place it at the
        /// end of the last string added.
        /// </summary>
        /// <param name="nRight"> - rightmost character end-position.</param>
        /// <param name="cString"> - the string to be written.</param>
        public void RightAt(int nRight, string cString)
        {
            int nLen = cString.Length;

            if (nRight == -1)
            {
                nRight = m_cursor + nLen;
            }

            if (nRight >= m_lineLen)
            {
                nRight = m_lineLen - 1;
            }

            if (nRight + 1 - nLen < 0)
            {
                nLen = nRight + 1;
            }

            for (int i = 0; i < nLen; i++)
            {
                m_lineBuff[i + nRight - nLen + 1] = cString[i];
            }

            m_cursor = nRight + 1;
        }

        /// <summary>
        /// Write a short integer (Int16) with a given format to the specified position.
        /// </summary>
        /// <param name="nPos"> - leftmost character start-position.</param>
        /// <param name="cFormat"> - format specifier, e.g {0} or {0,8F3} etc.</param>
        /// <param name="sValue"> integer value to be written.</param>
        public void ShortAt(int nPos, string cFormat, short sValue)
        {
            string cBuff;

            if (nPos >= m_lineLen)
            {
                return;
            }

            cBuff = String.Format(cFormat, sValue);

            if (cBuff.Length + nPos > m_lineLen)
            {
                m_lineBuff[nPos] = '*';
                return;
            }

            LeftAt(nPos, cBuff);
            return;
        }

        /// <summary>
        /// Write an integer (Int32) with a given format to the specified position.
        /// </summary>
        /// <param name="nPos"> - leftmost character start-position.</param>
        /// <param name="cFormat"> - format specifier, e.g {0} or {0,6} etc.</param>
        /// <param name="nValue"> - integer value to be written.</param>
        public void IntAt(int nPos, string cFormat, int nValue)
        {
            string cBuff;

            if (nPos >= m_lineLen)
            {
                return;
            }

            cBuff = String.Format(cFormat, nValue);

            if (cBuff.Length + nPos > m_lineLen)
            {
                m_lineBuff[nPos] = '*';
                return;
            }

            LeftAt(nPos, cBuff);
            return;
        }

        /// <summary>
        /// Write a float using a specified format to a specified position in the
        /// line buffer; checks for overflow; -1 means start writing adjacent to 
        /// the current column position.
        /// </summary>
        /// <param name="nPos"> - leftmost character start-position.</param>
        /// <param name="cFormat"> - format specifier, e.g {0} or {0,8F3} etc.</param>
        /// <param name="fValue"> - float value to be written.</param>
        public void FloatAt(int nPos, string cFormat, float fValue)
        {
            string cBuff;

            if (nPos >= m_lineLen)
            {
                return;
            }

            cBuff = String.Format(cFormat, fValue);

            // If dValue < 0 but formatting it rounds to zero printf() will write "-0.0"
            // whereas C# writes "0.0". For consistency of output between the
            // native and managed code we emulate this difference in formatting.
            if (fValue < 0)
            {
                string pattern = @" (0*)\.(0*)";
                Match match = Regex.Match(cBuff, pattern);
                if (match.Success)
                {
                    GroupCollection gc = match.Groups;
                    string substitute = "-" + gc[1] + "." + gc[2];
                    cBuff = Regex.Replace(cBuff, pattern, substitute);
                }
            }

            if (cBuff.Length + nPos > m_lineLen)
            {
                m_lineBuff[nPos] = '*';
                return;
            }

            LeftAt(nPos, cBuff);
            return;
        }

        /// <summary>
        /// Write a double using a specified format to a specified position in the
        /// line buffer; checks for overflow; -1 means start writing adjacent to 
        /// the current column position.
        /// </summary>
        /// <param name="nPos"> - leftmost character start-position.</param>
        /// <param name="cFormat"> - format specifier, e.g {0} or {0,8F3} etc.</param>
        /// <param name="dValue"> - double value to be written.</param>
        public void DoubleAt(int nPos, string cFormat, double dValue)
        {
            string cBuff;

            if (nPos >= m_lineLen)
            {
                return;
            }

            cBuff = String.Format(cFormat, dValue);

            // If dValue < 0 but formatting it rounds to zero printf() will write "-0.0"
            // whereas C# writes "0.0". For consistency of output between the
            // native and managed code we emulate this difference in formatting.
            if (dValue < 0)
            {
                string pattern = @" (0*)\.(0*)";
                Match match = Regex.Match(cBuff, pattern);
                if (match.Success)
                {
                    GroupCollection gc = match.Groups;
                    string substitute = "-" + gc[1] + "." + gc[2];
                    cBuff = Regex.Replace(cBuff, pattern, substitute);
                }
            }

            if (cBuff.Length + nPos > m_lineLen)
            {
                m_lineBuff[nPos] = '*';
                return;
            }

            LeftAt(nPos, cBuff);
            return;
        }

        /// <summary>
        /// Write the string created by <c>String.Format(cFormat, str)</c> starting at the
        /// prescribed character start position.
        /// </summary>
        /// <param name="nPos"> - leftmost character start position.</param>
        /// <param name="cFormat"> - format specifier to be applied to str, e.g. {0,6}</param>
        /// <param name="str"> - the string to be formatted and written to the current line buffer.</param>
        public void LeftAt(int nPos, string cFormat, string str)
        {
            string cBuff;

            if (nPos >= m_lineLen)
            {
                return;
            }

            cBuff = String.Format(cFormat, str);

            if (cBuff.Length + nPos > m_lineLen)
            {
                m_lineBuff[nPos] = '*';
                return;
            }

            LeftAt(nPos, cBuff);
            return;
        }

        /// <summary>
        /// Write the string created by <c>String.Format(cFormat, str)</c> ending at the
        /// prescribed rightmost character position.
        /// </summary>
        /// <param name="nPos"> - rightmost character end position.</param>
        /// <param name="cFormat"> - format specifier to be applied to str, e.g. {0,6}</param>
        /// <param name="str"> - the string to be formatted and written to the current line buffer.</param>
        public void RightAt(int nPos, string cFormat, string str)
        {
            string cBuff;

            if (nPos >= m_lineLen)
            {
                return;
            }

            cBuff = String.Format(cFormat, str);

            if (cBuff.Length + nPos > m_lineLen)
            {
                m_lineBuff[nPos] = '*';
                return;
            }

            RightAt(nPos, cBuff);
            return;
        }


        /****************************************************************************\
        *
        *		Skip a page, outputting the current buffer first.
        *
        \****************************************************************************/
        /// <summary>
        /// Skip forward a page after outputting the current line buffer.
        /// </summary>
        /// <param name=""></param>
        public void PageBefore()
        {
            if (m_page > 1 || m_line > 0)
            {
                m_page++;
            }

            m_line = 0;     //	To avoid an output loop at the end of page.

            if (m_cursor != 0)
            {
                Output();
            }

            // \f is the Form Feed character: it skips to the start of the next page. 
            // This applies mostly to terminals where the output device is a printer.
            if (m_page > 1)
            {
                LeftAt(0, "\f");
                Output();
                m_line = 0;
            }
        }

        /// <summary>
        /// Insert a form feed character at the current character position in the
        /// line buffer.
        /// </summary>
        /// <param name=""></param>
        public void FormFeed()
        {
            LeftAt(0, "\f");
            Output();
            m_line = 0;
        }


        /****************************************************************************\
        *
        *		Print the page header.
        *
        \****************************************************************************/
        /// <summary>
        /// This is a 'placeholder' method that should be overriden by one that provides 
        /// the specific header content for a specific TSIP report type.
        /// </summary>
        public virtual void PageHeader()
        {
            //...Log2.v("\nPrintLine:PageHeader()");

            PageBefore();
        }


        /// <summary>
        /// Returns the number of lines written to the current page.
        /// </summary>
        /// <returns></returns>
        public int LineNumber() { return m_line; }

        /// <summary>
        /// Returns the maximum number of lines allowed per page.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public int LastLine() { return m_linesPerPage; }

        /// <summary>
        /// Returns the maximum number of characters allowed per line.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public int LastPos() { return m_lineLen; }

        /// <summary>
        /// Returns the current character position within the line buffer.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public int currPos() { return m_cursor; }

        /// <summary>
        /// Sets the maximum number of lines per page.
        /// </summary>
        /// <param name="nLines"> - maximum number of lines per page.</param>
        /// <returns></returns>
        public int SetLinesPerPage(int nLines)
        {
            int nTemp = m_linesPerPage;
            m_linesPerPage = nLines;
            return nTemp;
        }


    }
}

```
