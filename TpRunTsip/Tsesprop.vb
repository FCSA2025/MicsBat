Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports System.IO
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods used to generate the Station Summary Report (.STATSUM) 
    ''' for the case of a proposed site being a Ts-Es interferer.
    ''' </summary>
    Public Class Tsesprop
        Private Shared mPrevInt As String = ""
        Private Shared mglbCount As Integer = 0

        ''' <summary>
        ''' This class extends the PrintLine base class to provide page header text
        ''' specific to the report type.
        ''' </summary>
        Private Class PrintLinerp2
            Inherits TpRunTsip.PrintLine
            ''' <summary>
            ''' This method overrides that of the PrintLine base class to provide
            ''' page header text specific to the report type.
            ''' </summary>
            ''' <paramname=""></param>
            Public Overrides Sub PageHeader()
                MyBase.PageBefore()
                TpRunTsip.Tsesprop.TsEsPropPage(Me)
            End Sub
        End Class

        ''' <summary>
        ''' This method retrieves TSIP results data from the DB and produces
        ''' the bulk of the text for the STATSUM report for Ts-Es interference.
        ''' </summary>
        ''' <paramname="tw">- the TextWriter object assigned to the .STATSUM report.</param>
        ''' <paramname="cTableName"> - the unique ID substring of the Te DB table names.</param>
        ''' <returns></returns>
        Public Shared Function TsEsProp(tw As TextWriter, cTableName As String) As Integer
            '...Log2.v("\nTsesprop.TsEsProp(): Entry: cTableName = " + cTableName);

            Dim nRet As Integer
            Dim interferer As String
            Dim call1 As String
            Dim name As String
            Dim oper As String
            Dim latit As Integer
            Dim longit As Integer
            Dim grnd As Double
            Dim nullInd As SQLLEN
            Dim nCount As Integer

            Dim p As TpRunTsip.Tsesprop.PrintLinerp2 = New TpRunTsip.Tsesprop.PrintLinerp2()
            p.OutFile(tw)

            TpRunTsip.Tsesprop.TsEsPropPage(p)

            '	Go through the table, this is the only place it is used, so a separate 
            '	access routine is not generated.
            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim sqlRet As SQLRETURN = 0
            Dim hStmt As SQLHANDLE

            Dim cBuff As String

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)
            If Not ODBC.IsOK(sqlRet) Then
            End If

            cBuff = SQLCHARPTR.Format("select tmpinter, tmpcall1, tmpname, tmpoper, tmplatit, tmplongit, tmpgrnd from {0} order by tmpinter, tmpcall1", cTableName)

            sqlRet = ODBC.SQLExecDirect(hStmt, cBuff, cBuff.Length)

            Const INTERFERER_SZ = 2
            Const CALL1_SZ = 10
            Const NAME_SZ = 33
            Const OPER_SZ = 7

            nCount = 0
            While True
                sqlRet = ODBC.SQLFetch(hStmt)
                If Not ODBC.IsOK(sqlRet) Then
                    Exit While
                End If

                ' This sets up the automatic column number indexing.
                Ssutil.DbStartGets()

                Ssutil.DbGetString(hStmt, 0, "interferer", interferer, INTERFERER_SZ, nullInd)
                Ssutil.DbGetString(hStmt, 0, "call1", call1, CALL1_SZ, nullInd)
                Ssutil.DbGetString(hStmt, 0, "name", name, NAME_SZ, nullInd)
                Ssutil.DbGetString(hStmt, 0, "oper", oper, OPER_SZ, nullInd)
                Ssutil.DbGetInt(hStmt, 0, "latit", latit, nullInd)
                Ssutil.DbGetInt(hStmt, 0, "longit", longit, nullInd)
                Ssutil.DbGetDouble(hStmt, 0, "grnd", grnd, nullInd)

                nCount += 1

                nRet = TpRunTsip.Tsesprop.TsEsPropDet(p, interferer, call1, name, oper, latit, longit, grnd)
            End While

            p.Output()
            p.IntAt(0, "{0} Records.", nCount)
            p.Output()

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nTsesprop.TsEsProp(): Exit");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method produces text used for a Ts-Es page header.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <returns></returns>
        Private Shared Function TsEsPropPage(pl As TpRunTsip.Tsesprop.PrintLinerp2) As Integer
            Dim cCurrDate As String
            Dim cCurrTime As String


            GenUtil.UtGetDateTime(cCurrDate, cCurrTime)

            pl.LeftAt(0, "FREQUENCY COORDINATION SYSTEM ASSOCIATION")
            pl.LeftAt(65, "DATE: {0}", cCurrDate)
            pl.Output()
            pl.LeftAt(0, "MASTER DATA BASE -- Interference Study Report - ")
            pl.LeftAt(72, "Page:")
            pl.IntAt(78, "{0,3:D}", pl.PageNumber)
            pl.Output()
            pl.LeftAt(0, "                    Proposed as Interferer - Station Summary")
            pl.Output()
            pl.LeftAt(0, "Project Code [{0}]", Info.ProjectCode)
            pl.Output()
            pl.Output()
            pl.LeftAt(0, "Stations Passed To Analysis")
            pl.Output()
            pl.LeftAt(0, "Call Sign")
            pl.LeftAt(10, "Station Name")
            pl.LeftAt(43, "Oper")
            pl.LeftAt(50, "Latitude")
            pl.LeftAt(63, "Longitude")
            pl.LeftAt(77, "Grnd")
            pl.Output()

            pl.LeftAt(78, "(m)")
            pl.Output()

            pl.LeftAt(0, "---------")
            pl.LeftAt(-1, " --------------------------------")
            pl.LeftAt(-1, " ------")
            pl.LeftAt(-1, " -----------")
            pl.LeftAt(-1, "  ------------")
            pl.LeftAt(-1, " -----")
            pl.Output()

            Return 0
        End Function

        ''' <summary>
        ''' This method produces text for the 'detail' line results data for a Ts-Es interference case.  
        ''' </summary>
        ''' <paramname="pl"> - PrintLine object with header override.</param>
        ''' <paramname="interferer"> - string indicating interferer ("I"), or not.</param>
        ''' <paramname="call1"> - call sign of the site.</param>
        ''' <paramname="name"> -the operator's assigned name for the site.</param>
        ''' <paramname="oper"> - the operator code.</param>
        ''' <paramname="latit"> - site's latitude.</param>
        ''' <paramname="longit"> - site's longitude.</param>
        ''' <paramname="grnd"> - ground height at site.</param>
        ''' <returns></returns>
        Private Shared Function TsEsPropDet(pl As TpRunTsip.Tsesprop.PrintLinerp2, interferer As String, call1 As String, name As String, oper As String, latit As Integer, longit As Integer, grnd As Double) As Integer
            Dim cLat As String
            Dim cLatSense As String
            Dim cLong As String
            Dim cLongSense As String

            If Not interferer.Equals(TpRunTsip.Tsesprop.mPrevInt) Then
                TpRunTsip.Tsesprop.mPrevInt = interferer
                pl.Output()
                pl.Output()
                If interferer.Equals("I") Then
                    pl.LeftAt(10, "=== FROM PROPOSED FILE AS INTERFERER ===")
                Else
                    pl.LeftAt(10, "=== FROM ENVIRONMENT FILE AS VICTIM ===")
                End If
                pl.Output()
                pl.Output()
            End If

            TpRunTsip.Tsesprop.mglbCount += 1

            GenUtil.UtLatConvStr(latit, cLat, cLatSense)
            GenUtil.UtLongConvStr(longit, cLong, cLongSense)

            pl.LeftAt(0, call1)
            pl.LeftAt(10, name)
            pl.LeftAt(43, oper)
            pl.LeftAt(50, cLat)
            pl.LeftAt(-1, "  ")
            pl.LeftAt(-1, cLong)
            pl.DoubleAt(-1, " {0,5:F0}", grnd)
            pl.Output()

            Return 0
        End Function



    End Class
End Namespace
