Imports _Configuration
Imports System
Imports System.IO
Imports System.Text.RegularExpressions

Namespace TpRunTsip
    ''' <summary>
    ''' Provides methods that facilitate the production of textual reports
    ''' by writing fields in a line-by-line sequence, in which individual fields are written
    ''' to prescribed row start and/or end positions, and then passing the accumulated
    ''' string to a TextWriter object, to be written to file.
    ''' </summary>
    Public Class PrintLine
        Private Const SPACE As Char = " "c
        Private m_lineLen As Integer
        Private m_lineBuff As Char()
        Private m_outFile As TextWriter
        Private m_page As Integer
        Private m_line As Integer
        Private m_linesPerPage As Integer
        Private m_cursor As Integer

        Public ReadOnly Property PageNumber As Integer
            Get
                Return m_page
            End Get
        End Property

        ''' <summary>
        ''' The default constructor: sets the default line length.
        ''' </summary>
        ''' <returns></returns>
        Public Sub New()
            Me.New(Constant.DEFAULT_LINE_SIZE)
        End Sub

        ''' <summary>
        ''' Constructor: sets the line length to a prescribed number
        ''' of characters.
        ''' </summary>
        ''' <paramname="nLen"> - rescribed line length (number of characters).</param>
        ''' <returns></returns>
        Public Sub New(nLen As Integer)

            m_lineLen = nLen

            m_lineBuff = New Char(m_lineLen - 1) {}
            FillWithSpaces(m_lineBuff)


            m_outFile = Console.Out

            m_page = 1
            m_line = 0
            m_linesPerPage = Constant.DEFAULT_LINES_PER_PAGE
            m_cursor = 0
        End Sub

        ''' <summary>
        ''' Fills a prescribed character array with blank spaces.
        ''' </summary>
        ''' <paramname="buffer"> - prescribed character array</param>
        Private Sub FillWithSpaces(ByRef buffer As Char())
            ' Fill with space characters.
            For i = 0 To buffer.Length - 1
                buffer(i) = TpRunTsip.PrintLine.SPACE
            Next
        End Sub

        ''' <summary>
        ''' Sets the class's private Textwriter object to be used for
        ''' writing text to file.
        ''' </summary>
        ''' <paramname="swNext"> - TextWriter object.</param>
        ''' <returns></returns>
        Public Function OutFile(swNext As TextWriter) As TextWriter
            Dim swTemp = m_outFile
            m_outFile = swNext
            Return swTemp
        End Function


        ' **************************************************************************\
        ' 
        ' 		Output a line buffer.
        ' 
        ' \***************************************************************************

        ''' <summary>
        ''' Writes a single line of accumulated text to the class's private TextWriter
        ''' object.
        ''' </summary>
        ''' <paramname=""></param>
        Public Sub Output()
            Output(1)
        End Sub

        ''' <summary>
        ''' Writes multiple lines of text to the class's private TextWriter
        ''' object; the first line is the current accumulated text line, the 
        ''' subsequent lines are all blank.
        ''' </summary>
        ''' <paramname=""> - number of lines to be written to TextWriter.</param>
        Public Sub Output(nLines As Integer)
            '	Trim:
            Dim nInd = m_lineLen

            m_line += 1
            If m_line >= m_linesPerPage Then
                PageHeader()
            End If

            Dim str As String = New String(m_lineBuff)
            str = str.TrimEnd(TpRunTsip.PrintLine.SPACE)


            If Equals(str, "") Then
                str = " "
            End If

            m_outFile.Write(str & Microsoft.VisualBasic.Constants.vbCrLf)
            m_outFile.Flush()
            FillWithSpaces(m_lineBuff)
            m_cursor = 0

            '	If we specify more than one line, the following lines will be blank lines...
            While Threading.Interlocked.Decrement(nLines) > 0
                m_line += 1
                If m_line >= m_linesPerPage Then
                    PageHeader()
                    Exit While  '	If we go over a page end, then cancel further skips.
                End If
                m_outFile.Write(Microsoft.VisualBasic.Constants.vbLf)
                m_outFile.Flush()
            End While

        End Sub

        ''' <summary>
        ''' Place a string in the line buffer starting at a precribed position, If the position
        ''' is -1, then place it adjacent to the end of the last placement.  Check for
        ''' line overflow, and truncate if necessary.
        ''' </summary>
        ''' <paramname="nLeft"> - leftmost character start-position.</param>
        ''' <paramname="cString"> - the string to be written.</param>
        Public Sub LeftAt(nLeft As Integer, cString As String)
            Dim nLen = cString.Length

            If nLeft = -1 Then
                nLeft = m_cursor
            End If

            If nLen + nLeft >= m_lineLen Then
                nLen = m_lineLen - nLeft
            End If

            For i = 0 To nLen - 1
                m_lineBuff(nLeft + i) = cString(i)
            Next

            m_cursor = nLeft + nLen
        End Sub

        ''' <summary>
        ''' Place a string so that it terminates as a given column in the line 
        ''' buffer.  Check for underflow, and if the column is -1, place it at the
        ''' end of the last string added.
        ''' </summary>
        ''' <paramname="nRight"> - rightmost character end-position.</param>
        ''' <paramname="cString"> - the string to be written.</param>
        Public Sub RightAt(nRight As Integer, cString As String)
            Dim nLen = cString.Length

            If nRight = -1 Then
                nRight = m_cursor + nLen
            End If

            If nRight >= m_lineLen Then
                nRight = m_lineLen - 1
            End If

            If nRight + 1 - nLen < 0 Then
                nLen = nRight + 1
            End If

            For i = 0 To nLen - 1
                m_lineBuff(i + nRight - nLen + 1) = cString(i)
            Next

            m_cursor = nRight + 1
        End Sub

        ''' <summary>
        ''' Write a short integer (Int16) with a given format to the specified position.
        ''' </summary>
        ''' <paramname="nPos"> - leftmost character start-position.</param>
        ''' <paramname="cFormat"> - format specifier, e.g {0} or {0,8F3} etc.</param>
        ''' <paramname="sValue"> integer value to be written.</param>
        Public Sub ShortAt(nPos As Integer, cFormat As String, sValue As Short)
            Dim cBuff As String

            If nPos >= m_lineLen Then
                Return
            End If

            cBuff = String.Format(cFormat, sValue)

            If cBuff.Length + nPos > m_lineLen Then
                m_lineBuff(nPos) = "*"c
                Return
            End If

            LeftAt(nPos, cBuff)
            Return
        End Sub

        ''' <summary>
        ''' Write an integer (Int32) with a given format to the specified position.
        ''' </summary>
        ''' <paramname="nPos"> - leftmost character start-position.</param>
        ''' <paramname="cFormat"> - format specifier, e.g {0} or {0,6} etc.</param>
        ''' <paramname="nValue"> - integer value to be written.</param>
        Public Sub IntAt(nPos As Integer, cFormat As String, nValue As Integer)
            Dim cBuff As String

            If nPos >= m_lineLen Then
                Return
            End If

            cBuff = String.Format(cFormat, nValue)

            If cBuff.Length + nPos > m_lineLen Then
                m_lineBuff(nPos) = "*"c
                Return
            End If

            LeftAt(nPos, cBuff)
            Return
        End Sub

        ''' <summary>
        ''' Write a float using a specified format to a specified position in the
        ''' line buffer; checks for overflow; -1 means start writing adjacent to 
        ''' the current column position.
        ''' </summary>
        ''' <paramname="nPos"> - leftmost character start-position.</param>
        ''' <paramname="cFormat"> - format specifier, e.g {0} or {0,8F3} etc.</param>
        ''' <paramname="fValue"> - float value to be written.</param>
        Public Sub FloatAt(nPos As Integer, cFormat As String, fValue As Single)
            Dim cBuff As String

            If nPos >= m_lineLen Then
                Return
            End If

            cBuff = String.Format(cFormat, fValue)

            ' If dValue < 0 but formatting it rounds to zero printf() will write "-0.0"
            ' whereas C# writes "0.0". For consistency of output between the
            ' native and managed code we emulate this difference in formatting.
            If fValue < 0 Then
                Dim pattern = " (0*)\.(0*)"
                Dim match = Regex.Match(cBuff, pattern)
                If match.Success Then
                    Dim gc = match.Groups
                    Dim substitute As String = "-" & gc(1).ToString() & "." & gc(2).ToString()
                    cBuff = Regex.Replace(cBuff, pattern, substitute)
                End If
            End If

            If cBuff.Length + nPos > m_lineLen Then
                m_lineBuff(nPos) = "*"c
                Return
            End If

            LeftAt(nPos, cBuff)
            Return
        End Sub

        ''' <summary>
        ''' Write a double using a specified format to a specified position in the
        ''' line buffer; checks for overflow; -1 means start writing adjacent to 
        ''' the current column position.
        ''' </summary>
        ''' <paramname="nPos"> - leftmost character start-position.</param>
        ''' <paramname="cFormat"> - format specifier, e.g {0} or {0,8F3} etc.</param>
        ''' <paramname="dValue"> - double value to be written.</param>
        Public Sub DoubleAt(nPos As Integer, cFormat As String, dValue As Double)
            Dim cBuff As String

            If nPos >= m_lineLen Then
                Return
            End If

            cBuff = String.Format(cFormat, dValue)

            ' If dValue < 0 but formatting it rounds to zero printf() will write "-0.0"
            ' whereas C# writes "0.0". For consistency of output between the
            ' native and managed code we emulate this difference in formatting.
            If dValue < 0 Then
                Dim pattern = " (0*)\.(0*)"
                Dim match = Regex.Match(cBuff, pattern)
                If match.Success Then
                    Dim gc = match.Groups
                    Dim substitute As String = "-" & gc(1).ToString() & "." & gc(2).ToString()
                    cBuff = Regex.Replace(cBuff, pattern, substitute)
                End If
            End If

            If cBuff.Length + nPos > m_lineLen Then
                m_lineBuff(nPos) = "*"c
                Return
            End If

            LeftAt(nPos, cBuff)
            Return
        End Sub

        ''' <summary>
        ''' Write the string created by <c>String.Format(cFormat, str)</c> starting at the
        ''' prescribed character start position.
        ''' </summary>
        ''' <paramname="nPos"> - leftmost character start position.</param>
        ''' <paramname="cFormat"> - format specifier to be applied to str, e.g. {0,6}</param>
        ''' <paramname="str"> - the string to be formatted and written to the current line buffer.</param>
        Public Sub LeftAt(nPos As Integer, cFormat As String, str As String)
            Dim cBuff As String

            If nPos >= m_lineLen Then
                Return
            End If

            cBuff = String.Format(cFormat, str)

            If cBuff.Length + nPos > m_lineLen Then
                m_lineBuff(nPos) = "*"c
                Return
            End If

            LeftAt(nPos, cBuff)
            Return
        End Sub

        ''' <summary>
        ''' Write the string created by <c>String.Format(cFormat, str)</c> ending at the
        ''' prescribed rightmost character position.
        ''' </summary>
        ''' <paramname="nPos"> - rightmost character end position.</param>
        ''' <paramname="cFormat"> - format specifier to be applied to str, e.g. {0,6}</param>
        ''' <paramname="str"> - the string to be formatted and written to the current line buffer.</param>
        Public Sub RightAt(nPos As Integer, cFormat As String, str As String)
            Dim cBuff As String

            If nPos >= m_lineLen Then
                Return
            End If

            cBuff = String.Format(cFormat, str)

            If cBuff.Length + nPos > m_lineLen Then
                m_lineBuff(nPos) = "*"c
                Return
            End If

            RightAt(nPos, cBuff)
            Return
        End Sub


        ' **************************************************************************\
        ' 
        ' 		Skip a page, outputting the current buffer first.
        ' 
        ' \***************************************************************************
        ''' <summary>
        ''' Skip forward a page after outputting the current line buffer.
        ''' </summary>
        ''' <paramname=""></param>
        Public Sub PageBefore()
            If m_page > 1 OrElse m_line > 0 Then
                m_page += 1
            End If

            m_line = 0     '	To avoid an output loop at the end of page.

            If m_cursor <> 0 Then
                Output()
            End If

            ' \f is the Form Feed character: it skips to the start of the next page. 
            ' This applies mostly to terminals where the output device is a printer.
            If m_page > 1 Then
                Me.LeftAt(0, Microsoft.VisualBasic.Constants.vbFormFeed)
                Output()
                m_line = 0
            End If
        End Sub

        ''' <summary>
        ''' Insert a form feed character at the current character position in the
        ''' line buffer.
        ''' </summary>
        ''' <paramname=""></param>
        Public Sub FormFeed()
            Me.LeftAt(0, Microsoft.VisualBasic.Constants.vbFormFeed)
            Output()
            m_line = 0
        End Sub


        ' **************************************************************************\
        ' 
        ' 		Print the page header.
        ' 
        ' \***************************************************************************
        ''' <summary>
        ''' This is a 'placeholder' method that should be overriden by one that provides 
        ''' the specific header content for a specific TSIP report type.
        ''' </summary>
        Public Overridable Sub PageHeader()
            '...Log2.v("\nPrintLine:PageHeader()");

            PageBefore()
        End Sub


        ''' <summary>
        ''' Returns the number of lines written to the current page.
        ''' </summary>
        ''' <returns></returns>
        Public Function LineNumber() As Integer
            Return m_line
        End Function

        ''' <summary>
        ''' Returns the maximum number of lines allowed per page.
        ''' </summary>
        ''' <paramname=""></param>
        ''' <returns></returns>
        Public Function LastLine() As Integer
            Return m_linesPerPage
        End Function

        ''' <summary>
        ''' Returns the maximum number of characters allowed per line.
        ''' </summary>
        ''' <paramname=""></param>
        ''' <returns></returns>
        Public Function LastPos() As Integer
            Return m_lineLen
        End Function

        ''' <summary>
        ''' Returns the current character position within the line buffer.
        ''' </summary>
        ''' <paramname=""></param>
        ''' <returns></returns>
        Public Function currPos() As Integer
            Return m_cursor
        End Function

        ''' <summary>
        ''' Sets the maximum number of lines per page.
        ''' </summary>
        ''' <paramname="nLines"> - maximum number of lines per page.</param>
        ''' <returns></returns>
        Public Function SetLinesPerPage(nLines As Integer) As Integer
            Dim nTemp = m_linesPerPage
            m_linesPerPage = nLines
            Return nTemp
        End Function


    End Class
End Namespace
