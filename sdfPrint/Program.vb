Imports System
Imports System.IO
Imports System.Data
Imports System.Data.Odbc
Imports System.Diagnostics
Imports ImpExpstructs
Imports AcctngUtilities

Namespace sdfPrint
    Friend Class Program
        Private Shared sw As StreamWriter
        Private Shared swrep As StreamWriter
        Private Shared sesSchema As String

        Private Shared Function Main(ByVal args As String()) As Integer
            Dim retval As String
            Dim cn_str As String

            ' get process info
            Dim thisProc As Process = Process.GetCurrentProcess()

            ' get arguments passed in
            Dim dbase = args(0)         ' database
            Dim out_dir = args(1)       ' output directory
            Dim filetype = args(2)      ' sdf type
            Dim filename = args(3)      ' PDF name
            Dim projectCode = args(4)   ' project code

            Dim userid = Environment.GetEnvironmentVariable("MicsUser")
            Dim webdrive = Environment.GetEnvironmentVariable("webdrive")
            Dim odbc = Environment.GetEnvironmentVariable("odbc")

            Dim dbgfile = webdrive & "\extractlogs\" & userid & "sdfPrint-" & filetype & ".txt"

            sw = New StreamWriter(dbgfile, False)

            sw.WriteLine(dbase & " " & out_dir & " " & filetype & " " & filename & " " & projectCode)
            Call sw.Flush()

            cn_str = "DSN=" & odbc & ";DATABASE=" & dbase & ";Trusted_Connection = True;MARS_Connection=yes"
            sw.WriteLine(cn_str)
            Call sw.Flush()

            Dim cn As OdbcConnection = New OdbcConnection(cn_str)

            ' try to open sql connection
            Try
                cn.Open()
            Catch e1 As Exception
                sw.WriteLine(e1.Message)
                Call sw.Close()
                Return 2  ' could not open connection
            End Try
            ' get default schema
            Dim getschema As OdbcCommand = New OdbcCommand("SELECT RTrim(dbo.user_schema2022('" & userid & "'))", cn)
            getschema.CommandType = CommandType.Text

            sesSchema = CStr(getschema.ExecuteScalar())
            sw.WriteLine("New schema:" & sesSchema & ":")
            Call sw.Flush()
            cn.Close()

            ' delete output file if present

            Dim outfile = out_dir & filename & ".txt"
            sw.WriteLine(outfile)
            Call sw.Flush()
            Try
                If File.Exists(outfile) Then
                    File.Delete(outfile)
                End If
            Catch ex As Exception
                sw.WriteLine(ex.Message)
                Call sw.Close()
                cn.Close()
                Return 2
            End Try

            swrep = New StreamWriter(outfile)

            Select Case filetype
                Case "band"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFbandPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING BAND INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "ante"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFantePrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING ANTE INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "ctx"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFctxPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING CTX INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "eqpt"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFeqptPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING EQPT INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "note"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFnotePrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING NOTE INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "oper"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFoperPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING OPER INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "plan"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFplanPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING PLAN INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "rout"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFroutPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING ROUT INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "town"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFtownPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING TOWN INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "towr"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFtowrPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING TOWR INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If
                Case "traf"
                    If Not Equals(CSharpImpl.__Assign(retval, ImpExp.SDFtrafPrint(swrep, sesSchema, cn_str, filename)), "OK") Then
                        sw.WriteLine("ERROR SELECTING TRAF INFO: " & retval)
                        Call sw.Flush()
                        Call swrep.Flush()
                        Call sw.Close()
                        Call swrep.Close()
                        Return 2
                    End If

                Case Else
            End Select

            Dim intret = AcctngUtils.log_billing1(thisProc, cn_str, sesSchema, userid, projectCode, "SD_EXPORT", filename)
            If intret <> 0 Then
                Call sw.Close()
                Call swrep.Close()
                Return 2
            End If

            Call sw.Close()
            Call swrep.Close()
            Return 0

        End Function

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Namespace
