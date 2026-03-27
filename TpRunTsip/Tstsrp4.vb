Imports System
Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports _DataStructures
Imports System.IO
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods used to generate the Case Summary (.CASESUM) for Ts-Ts.
    ''' </summary>
    Public Class Tstsrp4
        ''' <summary>
        ''' This class extends the PrintLine base class to provide page header text
        ''' specific to the report type.
        ''' </summary>
        Public Class PrintLine4
            Inherits TpRunTsip.PrintLine
            ''' <summary>
            ''' This method overrides that of the PrintLine base class to provide
            ''' page header text specific to the report type.
            ''' </summary>
            ''' <paramname=""></param>
            Public Overrides Sub PageHeader()
                '...Log2.v("\nTstsrp4.PrintLine4.PageHeader()");

                'PageBefore();

                TpRunTsip.Tstsrp4.TsTsRp4Page(Me)
            End Sub
        End Class

        Private Shared mLastCase As Integer = -1

        ''' <summary>
        ''' This method retrieves TSIP results data from the DB and produces
        ''' the bulk of the text for the CASESUM report.
        ''' </summary>
        ''' <paramname="tw"> - the TextWriter object assigned to the .CASESUM report.</param>
        ''' <paramname="ttName"> - the unique ID substring of the Ts DB table names.</param>
        ''' <returns></returns>
        Public Shared Function TsTsRp4(tw As TextWriter, ttName As String) As Integer
            '...Log2.v("\nTstsrp4.TsTsRp4(): Entry");

            Dim pl As TpRunTsip.Tstsrp4.PrintLine4 = New TpRunTsip.Tstsrp4.PrintLine4()
            pl.OutFile(tw)

            pl.FormFeed()
            pl.PageHeader()

            Dim rr As TsTsSelRec4 = New TsTsSelRec4()

            Dim IsNull As SQLLEN

            Dim cSQL As String
            Dim cNameBuf As String
            Dim nNumCases As Integer

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN

            cSQL = SQLCHARPTR.Format("SELECT a.caseno, subcaseno, intname1, a.interferer, vicname1, int1vic1dist, intoffax, vicoffax, vicpwrrx, calctype, intpolar, vicpolar, intfreqtx, freqsep, calcico," & Microsoft.VisualBasic.Constants.vbTab & "calcixp, reqdcalc, resti, intname2, vicname2" & Microsoft.VisualBasic.Constants.vbTab & "FROM" & Microsoft.VisualBasic.Constants.vbTab & "{0}.tt_{1}_site a, {0}.tt_{1}_ante b, {0}.tt_{1}_chan c WHERE" & Microsoft.VisualBasic.Constants.vbTab & "a.intcall1 = b.intcall1 and a.intcall2 = b.intcall2 and a.viccall1 = b.viccall1 and a.viccall2 = b.viccall2 and a.caseno   = b.caseno and a.interferer = b.interferer and c.intcall1 = b.intcall1 and c.intcall2 = b.intcall2 and c.viccall1 = b.viccall1 and c.viccall2 = b.viccall2 and c.caseno   = b.caseno and c.interferer = b.interferer and c.intanum = b.intanum and c.vicanum = b.vicanum and c.intbndcde = b.intbndcde and c.vicbndcde = b.vicbndcde ORDER BY caseno, subcaseno, resti ", Info.GlobalSchema, ttName)
            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp4.TsTsRp4(): SQL:" & Microsoft.VisualBasic.Constants.vbLf & cSQL)

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp4.TsTsRp4(): ERROR: SQLExecDirect() failed on query:" & Microsoft.VisualBasic.Constants.vbLf & cSQL)
                Ssutil.DbGetDiagStmt(hStmt, "tstsrp401: Could not execute statement:-" & Microsoft.VisualBasic.Constants.vbLf & cSQL)
                Return [Error].ODBC_EXECDIRECT_FAILED
            End If

            nNumCases = 0
            While True
                sqlRet = ODBC.SQLFetch(hStmt)
                If sqlRet = Constant.NOMORERECS Then
                    Exit While
                ElseIf Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp4.TsTsRp4()(): ERROR: SQLFetch() failed.")
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp402: Failed Fetch:")
                    Return [Error].ODBC_FETCH_FAILED
                End If

                nNumCases += 1

                '	Get the record...
                Try
                    Ssutil.DbStartGets()
                    Ssutil.DbGetInt(hStmt, 0, "caseno", rr.caseno, IsNull)
                    Ssutil.DbGetInt(hStmt, 0, "subcaseno", rr.subcaseno, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "intname1", rr.intname1, TsTsSelRec4.INTNAME1, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "interferer", rr.interferer, TsTsSelRec4.INTERFERER, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "vicname1", rr.vicname1, TsTsSelRec4.VICNAME1, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "int1vic1dist", rr.int1vic1dist, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "intoffax", rr.intoffax, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "vicoffax", rr.vicoffax, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "vicpwrrx", rr.vicpwrrx, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "calctype", rr.calctype, TsTsSelRec4.CALCTYPE, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "intpolar", rr.intpolar, TsTsSelRec4.INTPOLAR, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "vicpolar", rr.vicpolar, TsTsSelRec4.VICPOLAR, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "intfreqtxd", rr.intfreqtxd, IsNull)
                    rr.intfreqtxd /= 1000.0
                    Ssutil.DbGetDouble(hStmt, 0, "freqsepd", rr.freqsepd, IsNull)
                    rr.freqsepd /= 1000.0
                    Ssutil.DbGetDouble(hStmt, 0, "calcico", rr.calcico, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "calcixp", rr.calcixp, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "reqdcalc", rr.reqdcalc, IsNull)
                    Ssutil.DbGetDouble(hStmt, 0, "resti", rr.resti, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "intname2", rr.intname2, TsTsSelRec4.INTNAME2, IsNull)
                    Ssutil.DbGetString(hStmt, 0, "vicname2", rr.vicname2, TsTsSelRec4.VICNAME2, IsNull)
                Catch e As Exception
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "Tstsrp4.TsTsRp4()(): ERROR: ODBC 'Get' attempt failed: " & e.Message)
                    Ssutil.DbGetDiagStmt(hStmt, "tstsrp402: Error retrieving field " & e.Message & ".")
                    Return [Error].ODBC_GET_FAILED
                End Try

                If rr.caseno <> TpRunTsip.Tstsrp4.mLastCase Then
                    If TpRunTsip.Tstsrp4.mLastCase > 0 Then
                        pl.Output()
                    End If
                    pl.IntAt(0, "{0,3:D}", rr.caseno)
                    TpRunTsip.Tstsrp4.mLastCase = rr.caseno
                End If
                pl.IntAt(4, "{0,4:D}", rr.subcaseno)
                cNameBuf = rr.intname1
                cNameBuf += " ("
                cNameBuf += rr.interferer
                cNameBuf += ")"
                pl.LeftAt(9, cNameBuf)
                pl.DoubleAt(46, "{0,6:F2}", rr.int1vic1dist)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F1}", rr.intoffax)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F1}", rr.vicoffax)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F2}", rr.vicpwrrx)
                pl.LeftAt(-1, " ")
                pl.LeftAt(-1, rr.calctype)
                pl.LeftAt(-1, "  ")
                pl.LeftAt(-1, rr.intpolar)
                pl.LeftAt(-1, " ")
                pl.LeftAt(-1, rr.vicpolar)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,11:F3}", rr.freqsepd)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F1}", rr.calcico)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F1}", rr.calcixp)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F1}", rr.reqdcalc)
                pl.LeftAt(-1, " ")
                pl.DoubleAt(-1, "{0,6:F1}", rr.resti)
                pl.Output()

                cNameBuf = rr.vicname1.Trim()
                If rr.interferer.Equals("P") Then
                    cNameBuf += " (E)"
                Else
                    cNameBuf += " (P)"
                End If
                pl.LeftAt(9, "--> ")
                pl.LeftAt(-1, cNameBuf)
                pl.Output()

            End While
            pl.Output()
            pl.IntAt(0, "Number of reporting cases: {0}", nNumCases)
            pl.Output()

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))


            '...Log2.v("\nTstsrp4.TsTsRp4(): Exit");
            Return 0
        End Function

        ''' <summary>
        ''' This method produces a page of text giving a summary of the details
        ''' for an interference case.
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <returns></returns>
        Private Shared Function TsTsRp4Page(pl As TpRunTsip.Tstsrp4.PrintLine4) As Integer
            Dim [cDate] As String
            Dim cTime As String

            GenUtil.UtGetDateTime([cDate], cTime)

            pl.PageBefore()
            pl.LeftAt(0, "Frequency Coordination System Association")
            pl.IntAt(111, "Page:{0,3:D}", pl.PageNumber)
            pl.Output()

            pl.LeftAt(0, "MASTER DATA BASE -- TSTS Interference Study Report - Case Summary")
            pl.LeftAt(111, "Date:{0}", [cDate])
            pl.Output()

            pl.LeftAt(0, "Project Code [{0}]", Info.ProjectCode)
            pl.Output()

            pl.LeftAt(0, "-")
            pl.RightAt(125, "-")
            pl.Output()

            pl.LeftAt(0, "Case/Sub")
            pl.LeftAt(9, "Interferer Station TX")
            pl.LeftAt(48, "I->V")
            pl.LeftAt(-1, "    Off")
            pl.LeftAt(-1, "    Off")
            pl.LeftAt(-1, "   RX")
            pl.LeftAt(-1, "    Ana")
            pl.LeftAt(-1, " P")
            pl.LeftAt(-1, " P")
            pl.LeftAt(-1, "   TX ->  RX")
            pl.LeftAt(-1, "       Calc.")
            pl.LeftAt(-1, "    Reqd.")
            pl.LeftAt(-1, "   Margn")
            pl.Output()

            pl.LeftAt(9, " --> Victim Station RX")
            pl.LeftAt(48, "Dist")
            pl.LeftAt(-1, "    AX>")
            pl.LeftAt(-1, "    AX>")
            pl.LeftAt(-1, "   RSL")
            pl.LeftAt(-1, "   Typ")
            pl.LeftAt(-1, " O")
            pl.LeftAt(-1, " O")
            pl.LeftAt(-1, "   Freq.Sep.")
            pl.LeftAt(-1, "    -I or C/I")
            pl.LeftAt(-1, "    -I")
            pl.LeftAt(124, "Check")
            pl.Output()

            pl.LeftAt(48, "(km)")
            pl.LeftAt(-1, "    TX")
            pl.LeftAt(-1, "     RX")
            pl.LeftAt(-1, "   (dBm)")
            pl.LeftAt(-1, "      L")
            pl.LeftAt(-1, " L")
            pl.LeftAt(-1, "   (MHz)")
            pl.LeftAt(-1, "       COPL")
            pl.LeftAt(-1, "   XPOL")
            pl.LeftAt(-1, "   C/I")
            pl.LeftAt(-1, "    (dBm)")
            pl.LeftAt(124, "Detail")
            pl.Output()

            pl.LeftAt(0, "---")
            pl.LeftAt(-1, " ---")
            pl.LeftAt(9, "---------------------------------")
            pl.LeftAt(46, "------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, " ---")
            pl.LeftAt(-1, "  I>V")
            pl.LeftAt(-1, " -----------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, "  -----")
            pl.LeftAt(-1, "  ------")
            pl.Output()

            Return 0
        End Function





    End Class
End Namespace
