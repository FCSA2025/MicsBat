Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports _OHloss
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Runtime.InteropServices
Imports SQLLEN = System.Int64

''' <summary>
''' This Terrestial Station (TS) and Earth Station (ES) interference analysis program is
''' the core of the MICS system. It inputs data describing new or modified radio systems
''' and identifies cases of interference with existing radio systems. TSIP provides
''' detailed analysis reports for TS-TS, TS-ES and ES-TS interference cases.
''' </summary>
''' <remarks>
''' The command-line usage is:
''' \image html "Usage - TpRunTsip.PNG" ""
''' </remarks>
Namespace TpRunTsip

    ''' <summary>
    ''' Provides the 'Main()' method that provides top-level 
    ''' control of the TSIP setup, microwave system calculations and
    ''' the production of reports.
    ''' </summary>
    Public Class TpRunTsip
        Private Shared mGlbIsCtxCalc As Integer
        Public Shared mReports As TpRunTsip.TsipReportHelper
        Public Shared mMicsUserViaCommandLine As String = ""

        Public Shared ReadOnly Property GlbIsCtxCalc As Integer
            Get
                Return TpRunTsip.TpRunTsip.mGlbIsCtxCalc
            End Get
        End Property

        ' The following pair of variables were previously combined in the native code
        ' struct glbParmparm.
        Public Shared mdCull As Double = 0.0
        Public Shared mdArcStep As Double

        ' Make a copy of the initial value of the stream Console.out
        ' so that it can be restored at a later time.
        Private Shared mStandardConsoleOut As TextWriter = Console.Out

        ' TextWriters for the error stream; make it globally accessible.
        Public Shared mTW_ERR As TextWriter = Nothing

        ' TextWriters for the reports.
        Private Shared mTW_AGGINTCSV As TextWriter = Nothing
        Private Shared mTW_AGGINTREP As TextWriter = Nothing
        Private Shared mTW_CASEDET As TextWriter = Nothing
        Private Shared mTW_CASEOHL As TextWriter = Nothing
        Private Shared mTW_CASESUM As TextWriter = Nothing
        Private Shared mTW_EXEC As TextWriter = Nothing
        Private Shared mTW_EXPORT As TextWriter = Nothing
        Private Shared mTW_HILO As TextWriter = Nothing
        Public Shared mTW_ORBIT As TextWriter = Nothing
        Private Shared mTW_STATSUM As TextWriter = Nothing
        Private Shared mTW_STUDY As TextWriter = Nothing

        '	ES to TS coordination distances.  Saved here for the final report. 
        Private Shared mdTxTro As Double = Double.MinValue
        Private Shared mdTxPre As Double = Double.MinValue
        Private Shared mdRxTro As Double = Double.MinValue
        Private Shared mdRxPre As Double = Double.MinValue

        Public Class ParmTableWN
            Public parmStruct As TpParm = New TpParm()
            Public parmNulls As SQLLEN() = NullHelper.CreateArrayOfNullInd(TpParm.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            ''' <summary>
            ''' This method returns a string that lists the field values
            ''' of this instance of ParmTableWN.
            ''' </summary>
            ''' <returns></returns>
            Public Overrides Function ToString() As String
                Return parmStruct.ToString()
            End Function

            ''' <summary>
            ''' This method returns a string that lists the field values
            ''' of this instance of ParmTableWN together with the field null
            ''' indicators.
            ''' </summary>
            ''' <paramname=""></param>
            ''' <returns></returns>
            Public Function ToStringWN() As String
                Return parmStruct.ToStringWN(parmNulls)
            End Function
        End Class

        ''' <summary>
        ''' This method that provides top-level 
        ''' control of the TSIP setup, microwave system calculations and
        ''' the production of reports.
        ''' </summary>
        ''' <paramname="args"></param>
        Private Shared Sub Main(args As String())
            Dim exitCode As Integer
            Dim nRet As Integer
            Dim rc As Integer
            Dim numStnGroups = 0, TsEsStnGroups = 0, EsTsStnGroups = 0
            Dim userSession = 1
            Dim userInfo As UserInfoData
            Dim parmTables As List(Of TpRunTsip.TpRunTsip.ParmTableWN) = New List(Of TpRunTsip.TpRunTsip.ParmTableWN)()
            Dim parmTableCount = 0
            Dim cProNameTrimmed As String
            Dim cRunNameTrimmed As String
            Dim cLockFile As String
            Dim numIntCases As Integer
            Dim numTeIntCases As Integer
            Dim startDate As String
            Dim startTime As String
            Dim isTS = False
            Dim viewName, parmName, siteName, endDate, endTime As String
            Dim anteName As String = Nothing, chanName As String = Nothing
            Dim sqlCommand As String
            Dim clockTime As Integer
            Dim cUnique, cUniqueEnv As String

            '----------------------------------------------------------------------

            Try
#If True
                Dim mLog2FilePath = "d:\users\ahulme\temp\TpRunTsip.log"
                If Log2.SetLogFilePath(mLog2FilePath) Then
                    Log2.Erase()
                    Log2.Set(Log2.FileOpenClose.PER_SESSION)
                    Log2.Set(Log2.WriteMode.ENABLED)
                    Log2.Set(Log2.Level.VERBOSE)
                    '...Log2.v("\nBuild: " + Info.BuildMetaData);
                    Info.BuildMetaData = Info.CollateExeMetaData()
                Else
                    Console.Error.Write(Microsoft.VisualBasic.Constants.vbCrLf & "ERROR: could not open Log2 file: " & mLog2FilePath)
                End If
#End If
                exitCode = Constant.SUCCESS

                ' Set our priority class to normal.  This will allow tsipInitiator to continue executing
                ' while it updates the tsip queue after we have started.
                Application.SetPriority(ProcessPriorityClass.Normal)

                ' Parse the command line.
                TpRunTsip.TpRunTsip.ParseCommandLineArguments(args)

                ' Report the version.
                Console.Write(Microsoft.VisualBasic.Constants.vbLf & "tpRunTsip Build {0} Process {1}" & Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf, Info.ManagedBuildInfo(), Info.ProcessID)

                ' Get all remaining Windows environment variables and system data 
                ' used by TpRunTsip.
                Call TpRunTsip.TpRunTsip.GetEnvVarsAndSysData()

                ' Now queue for access.
                nRet = Qutils.EnterQueue(Info.DbName, "READ", 30)
                If nRet <> Constant.SUCCESS Then
                    Qutils.ExplainQueue(Info.DbName, "READ", nRet, Nothing)    '	Explain any error message.
                    Application.Exit([Error].UNABLE_TO_ENTER_QUEUE)
                End If

                ' This tests for the environment variable MICS_CTX_CALC. If it is present
                ' and non-zero, then all ctx values are the result of calculations.
                If Not Equals(Info.MicsCtxCalc, Nothing) Then
                    TpRunTsip.TpRunTsip.mGlbIsCtxCalc = Convert.ToInt32(Info.MicsCtxCalc)
                    If TpRunTsip.TpRunTsip.mGlbIsCtxCalc <> 0 Then
                        Console.Write("All CTX values will be obtained by calculation ({0})." & Microsoft.VisualBasic.Constants.vbLf, TpRunTsip.TpRunTsip.mGlbIsCtxCalc)
                    End If
                Else
                    TpRunTsip.TpRunTsip.mGlbIsCtxCalc = 0
                End If

                ' Initialize the directories that will be used in ohloss.
                Dim dir250k As String
                Dim dir50k As String

                If Equals(Info.FcsaMaps50K, Nothing) Then
                    dir50k = "d:\dted50\data"
                Else
                    dir50k = Info.FcsaMaps50K
                End If

                If Equals(Info.FcsaMaps250K, Nothing) Then
                    dir250k = "d:\dted250\data"
                Else
                    dir250k = Info.FcsaMaps250K
                End If

                CTEfunctions.Init_Directories(dir250k, dir50k)

                Console.Write("dir250k, dir50k initialized" & Microsoft.VisualBasic.Constants.vbLf)

                ' Connect to the database and start the processing.
                rc = Ssutil.UtConnect(Info.DbName, userSession)

                If rc <> 0 Then
                    ' Can't connect to database 
                    ErrMsg.UtPrintMessage([Error].NODATABASE, Info.DbName)
                    Qutils.ExitQueue(Info.DbName, "READ")
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): Ssutil.UtConnect(): ERROR: Can't connect to database]n")
                    Application.Exit(Constant.FAILURE)
                End If
                Console.Write("Connected to Database" & Microsoft.VisualBasic.Constants.vbLf)

                ' AH: unsolicited bug fix; dated 20210222
                ' Do an early check that the user has defined some TSIP 'run' records,
                ' i.e. the PARM table has one or more records in it.
                Dim parmTablename As String
                GenUtil.UtCvtName(Constant.TP_PARM, Info.PdfName, parmTablename)

                nRet = Ssutil.DbCountRows(parmTablename, "")
                If nRet = [Error].ODBC_EXECDIRECT_FAILED Then
                    Dim str = String.Format("ERROR: The TSIP run parameters table {0} does not exist.", parmTablename)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): " & str)
                    Application.Exit([Error].PARMTABLEDOESNOTEXIST)
                ElseIf nRet < 1 Then
                    Dim str = String.Format("ERROR: The TSIP run parameters table {0} contains no records.", parmTablename)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): " & str)
                    Application.Exit([Error].PARMTABLEHASNORECORDS)
                End If
                Console.Write(Microsoft.VisualBasic.Constants.vbLf & nRet.ToString() & " runs found" & Microsoft.VisualBasic.Constants.vbLf)

                BiUtil.BiBillingRec("TPRUNTSIP", Info.PdfName)

                If _Utillib.UserInfo.UtGetUserInfo(userInfo) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].PERMISSIONDENIED)
                    Ssutil.UtDisconnect(userSession)
                    Qutils.ExitQueue(Info.DbName, "READ")
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): UtGetUserInfo(): ERROR: call failed.")
                    Application.Exit(Constant.FAILURE)
                End If
                Console.Write("User name: " & userInfo.micsUser.micsid & Microsoft.VisualBasic.Constants.vbLf)

                ' A single error file is created for all the tsip parameter records. 
                ' The null argument indicates that the file is to be created and truncated.
                ' This call instantiates the TextWriter mTW_ERR.
                TpRunTsip.TpRunTsip.CreateErrorFile(Info.DestName, Info.PdfName, Nothing)

                ' Tell ErrMsg where to stream its output.
                ErrMsg.SetDefaultOutputStream(TpRunTsip.TpRunTsip.mTW_ERR)

                ' load all records from the current parm file into memory.
                rc = TpRunTsip.TpRunTsip.ParmFileInit(Info.PdfName, parmTableCount, parmTables)
                If rc <> Constant.SUCCESS Then
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "Invalid Parm File, probably doesn't exist ({0})." & Microsoft.VisualBasic.Constants.vbCrLf, rc)
                    Call TpRunTsip.TpRunTsip.mTW_ERR.Close()
                    Qutils.ExitQueue(Info.DbName, "READ")
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): ParmFileInit(): ERROR: call failed.")
                    Application.Exit(Constant.FAILURE)
                End If

                ' Get the startDate and startTime to be used as fields in the TsipReports table.
                GenUtil.UtGetDateTime(startDate, startTime)
                Info.Date = startDate
                Info.Time = startTime
                Console.Write("Starttime retrieved" & Microsoft.VisualBasic.Constants.vbLf)

                ' All the parameter records have now been read in; now start to process them.

                ' For each parameter record in the current parameter file ...
                For Each currParm As TpRunTsip.TpRunTsip.ParmTableWN In parmTables
                    Log2.v(Microsoft.VisualBasic.Constants.vbLf & currParm.ToStringWN().ToString())

                    ' Determine which reports types have been requested.
                    TpRunTsip.TpRunTsip.mReports = New TpRunTsip.TsipReportHelper(currParm.parmStruct.reports, currParm.parmStruct.tsorbout)

                    ' Enhancement 180301A.
                    '             =======
                    ' Attach TS or ES export data file for each run to the email sent to the user.
                    ' 'Export' data is produced by FtPrint and/or FePrint.
                    ' To implement this enhancement the simplest approach is to invent two new
                    ' TSIP report types whose file name ends with .TS_EXPORT and/or ES_EXPORT.
                    ' WebMICS will remain unchanged so this is not an optional report that the
                    ' user can select. Instead, we will force the EXPORT report to be written for
                    ' each distinct run.
                    TpRunTsip.TpRunTsip.mReports.RequestExportReport(True)

                    '...Log2.v("\n" + mReports.ToString());

                    '	Clear out the study files in case of a rerun.
                    TpRunTsip.TpRunTsip.RemoveStudyFiles(Info.DestName, Info.PdfName, currParm.parmStruct.runname)
                    Console.Write("Study files cleared" & Microsoft.VisualBasic.Constants.vbLf)

                    ' Make the runname available via global static.
                    Info.RunID = currParm.parmStruct.runname
                    Console.Write("Runname:" & Info.RunID & Microsoft.VisualBasic.Constants.vbLf)

                    ' AH: New
                    ' The following method assigns a TextWriter stream for every report type.
                    TpRunTsip.TpRunTsip.OpenReportStreams(Info.PdfName, currParm.parmStruct.runname, currParm.parmStruct.protype)

                    ' Make sure that there is no other tsip running this file.  
                    ' First create a unique file name.
                    cProNameTrimmed = currParm.parmStruct.proname.Trim()
                    cRunNameTrimmed = currParm.parmStruct.runname.Trim()
                    cLockFile = String.Format("{0}_{1}_{2}_LOCK", Info.DbName, cProNameTrimmed, cRunNameTrimmed)
                    Console.Write("cLockFile:" & cLockFile & Microsoft.VisualBasic.Constants.vbLf)

                    If GenUtil.FileGate(cLockFile) <> Constant.SUCCESS Then
                        '	We have a locking situation.  Tell the user to come back later
                        Console.Write("*ERROR* - TSIP is currently being run on this combination. Try again later." & Microsoft.VisualBasic.Constants.vbCrLf)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write("*ERROR* - TSIP is currently being run on this combination. Try again later." & Microsoft.VisualBasic.Constants.vbCrLf)
                        Log2.w(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): FileGate(): WARNING: call failed.")
                        Continue For ' Process the next parameter table.
                    End If
                    Console.Write(Microsoft.VisualBasic.Constants.vbLf & "No locks in place - process run" & Microsoft.VisualBasic.Constants.vbLf)

                    ' Re-open the Error file for this Tsip parameter record.
                    ' This is necessary for multiple records since the output is
                    ' redirected for reports after each Tsip parameter record is
                    ' processed.
                    TpRunTsip.TpRunTsip.CreateErrorFile(Info.DestName, Info.PdfName, currParm.parmStruct.runname)

                    ' The following code is here to provide compatability between the
                    ' old TSIP parameter files with Propogation Loss Method selection
                    ' for the ES side only, and the new files which permit up to 5
                    ' propogation loss methods for the TS side. Old TSIP parameter
                    ' files may have NULL spherecalc fields--this must be caught and
                    ' converted here.
                    If currParm.parmNulls(TpParm.SPHERECALC) = Constant.DB_NULL Then
                        currParm.parmStruct.spherecalc = "3"
                        currParm.parmNulls(TpParm.SPHERECALC) = Constant.DB_NOT_NULL
                    Else
                        If Strings.FirstCharIs(currParm.parmStruct.spherecalc, "N"c) Then
                            currParm.parmStruct.spherecalc = "1"
                        End If
                        If Strings.FirstCharIs(currParm.parmStruct.spherecalc, "Y"c) Then
                            currParm.parmStruct.spherecalc = "2"
                        End If
                    End If
                    Console.Write(Microsoft.VisualBasic.Constants.vbLf & "spherecalc:" & currParm.parmStruct.spherecalc.ToString().ToString() & Microsoft.VisualBasic.Constants.vbLf)

                    '	If this is an ohloss calculation, print the directories...
                    If currParm.parmStruct.spherecalc(0) = "5"c Then
                        Dim str = String.Format(Microsoft.VisualBasic.Constants.vbCrLf & "Over Horizon Loss Calculation Directories:-" & Microsoft.VisualBasic.Constants.vbCrLf & "1:250K - {0}" & Microsoft.VisualBasic.Constants.vbCrLf & "1:50K  - {1}" & Microsoft.VisualBasic.Constants.vbCrLf, dir250k, dir50k)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    End If

                    ' Check whether this is a PLAN mode analysis. If it is,
                    ' generate a temporaray PDF with all the PLAN channels in it. The
                    ' name of this new PDF becomes the PDf name in the parmStruct. The
                    ' PDF will be deleted at the end of the tsip run.
                    If currParm.parmStruct.analopt.Equals("PLAN") AndAlso currParm.parmStruct.protype.Equals("T") Then
                        If TpRunTsip.TpPlanChan.GenChan(currParm.parmStruct) <> Constant.SUCCESS Then
                            ErrMsg.UtPrintMessage([Error].NOPLANPDF)
                            exitCode = Constant.FAILURE
                            Continue For
                        End If

                        _Utillib.UserInfo.UtUpdateCentralTable("A", currParm.parmStruct.proname, Constant.FT, "T", "N")
                        _Utillib.UserInfo.UtUpdateCentralTable("U", currParm.parmStruct.proname, Constant.FT, "T", "N")
                    End If


                    ' Initialize the number of interference cases found for this tsip run.
                    numIntCases = 0
                    numTeIntCases = 0

                    ' Start a stop watch to calculate elapsed real-time.
                    Dim stopWatch As Stopwatch = Stopwatch.StartNew()

                    ' Get the startDate and startTime to include in the exec report.
                    GenUtil.UtGetDateTime(startDate, startTime)

                    ' ParmRecInit() write the current parm rec to mTW_ERR.
                    ' Also, check the validation status of the PDFs.
                    '
                    ' Note:
                    ' ====
                    ' This method also changes the value of the field currParm.coord from that read in 
                    ' from the Parm table in the DB. However, the corresponding nullInd is NOT changed. 
                    ' This is important when the updated Parm object is inserted back into the DB 
                    ' because a coordist that is read in as NULL gets written back to the Parm table as NULL.
                    If CSharpImpl.__Assign(rc, TpRunTsip.TpRunTsip.ParmRecInit(currParm)) <> Constant.SUCCESS Then
                        If rc <> Constant.FAILURE Then
                            ErrMsg.UtPrintMessage(rc)
                            ErrMsg.UtPrintMessage([Error].DYN_MS_SQL_SERVER_ERR)
                        End If

                        Call TpRunTsip.TpRunTsip.CloseReportStreams()
                        Call TpRunTsip.TpRunTsip.DeleteUnwantedReportFiles()

                        exitCode = Constant.FAILURE
                        Continue For  ' skip to the next run.
                    End If

                    ' AH: Supercedes original comment text.
                    ' The call below is a 'legacy' subroutine name retained for convenience.
                    ' Although the call's name says 'Create', the Orbit report has already
                    ' been created, above, along with all the other report types.
                    ' This method just writes a text 'header' into the (so far) empty
                    ' Orbit report file.
                    ' We do this here because we will be writing to the ORBIT reports an
                    ' unknown number of times.
                    rc = TpRunTsip.TpRunTsip.CreateOrbitFile(Info.DestName, Info.PdfName, currParm.parmStruct.runname)
                    Console.Write(Microsoft.VisualBasic.Constants.vbLf & "TS_Orbit file created" & Microsoft.VisualBasic.Constants.vbLf)

                    viewName = String.Format("{0}_{1}", Info.PdfName, currParm.parmStruct.runname)

                    If currParm.parmStruct.protype.Equals("E") OrElse currParm.parmStruct.envtype.Equals("PDF_ES") OrElse currParm.parmStruct.envtype.Equals("MDB_ES") Then
                        Console.Write(Microsoft.VisualBasic.Constants.vbLf & "Processing ES TSIP" & Microsoft.VisualBasic.Constants.vbLf)
                        isTS = False
                        GenUtil.UtCvtName(Constant.TE_PARM, viewName, parmName)
                        GenUtil.UtCvtName(Constant.TE_SITE, viewName, siteName)
                        GenUtil.UtCvtName(Constant.TE_ANTE, viewName, anteName)

                        ' Cull, build and populate the ES SH Tables.
                        ' This is the call that begins the TSIP computation for ES cases.
                        rc = TpRunTsip.TeBuildSH.TeBuildSHTable(viewName, Info.PdfName, currParm.parmStruct, currParm.parmNulls, numIntCases, numTeIntCases, startDate, startTime)

                        Log2.v(currParm.parmStruct.ToStringWN(currParm.parmNulls))

                        If rc = Constant.FAILURE Then
                            TpRunTsip.TpRunTsip.mTW_ERR.Write("FATAL ES ERROR({0}): PROCESSING TERMINATED" & Microsoft.VisualBasic.Constants.vbCrLf, rc)
                            GenUtil.UtGetDateTime(endDate, endTime)
                            sqlCommand = String.Format("Time: {0}" & Microsoft.VisualBasic.Constants.vbLf, endTime.PadRight(12))
                            ErrMsg.UtPrintMessage([Error].GENERROR, sqlCommand)
                            If Not String.IsNullOrWhiteSpace(GenUtil.GetUserMess()) Then
                                Call TpRunTsip.TpRunTsip.mTW_ERR.Write("*ERROR : {0}" & Microsoft.VisualBasic.Constants.vbCrLf, GenUtil.GetUserMess())
                            End If

                            exitCode = Constant.FAILURE
                            Continue For
                        End If
                        Console.Write(Microsoft.VisualBasic.Constants.vbLf & "ES TSIP Calculations completed" & Microsoft.VisualBasic.Constants.vbLf)

                        ' We have completed the TSIP calculations, now start on the reports.  
                        ' Print out the times.
                        TpRunTsip.TpMdbPdfGet.UtGetInterferenceGroups(currParm.parmStruct.runname, siteName, anteName, TsEsStnGroups, EsTsStnGroups)

                        numStnGroups = TsEsStnGroups + EsTsStnGroups
                    Else
                        Console.Write(Microsoft.VisualBasic.Constants.vbLf & "Processing TS TSIP" & Microsoft.VisualBasic.Constants.vbLf)
                        isTS = True
                        GenUtil.UtCvtName(Constant.TT_PARM, viewName, parmName)
                        GenUtil.UtCvtName(Constant.TT_SITE, viewName, siteName)

                        ' Cull, build and populate the TS SH Tables.
                        ' This is the call that begins the TSIP computation for TS cases.
                        rc = TpRunTsip.TtBuildSH.TtBuildSHTable(viewName, currParm.parmStruct, currParm.parmNulls, numIntCases, startDate, startTime)

                        If rc <> Constant.SUCCESS Then
                            Console.Write(Microsoft.VisualBasic.Constants.vbLf & "Build TtBuildSHTable FAILED" & Microsoft.VisualBasic.Constants.vbLf)
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): ERROR: TtBuildSH.TtBuildSHTable() returned " & rc.ToString())

                            TpRunTsip.TpRunTsip.mTW_ERR.Write("FATAL TS ERROR({0}): PROCESSING TERMINATED" & Microsoft.VisualBasic.Constants.vbCrLf, rc)
                            GenUtil.UtGetDateTime(endDate, endTime)
                            sqlCommand = String.Format(" Time: {0}" & Microsoft.VisualBasic.Constants.vbLf, endTime.PadRight(12))
                            ErrMsg.UtPrintMessage([Error].GENERROR, sqlCommand)
                            If Not String.IsNullOrWhiteSpace(GenUtil.GetUserMess()) Then
                                Call TpRunTsip.TpRunTsip.mTW_ERR.Write("*ERROR : {0}" & Microsoft.VisualBasic.Constants.vbCrLf, GenUtil.GetUserMess())
                            End If

                            exitCode = Constant.FAILURE
                            Continue For
                        End If

                        numStnGroups = Ssutil.DbCountRows(siteName, Nothing)
                    End If

                    ' Delete the temporary PDF.
                    If currParm.parmStruct.analopt.Equals("PLAN") AndAlso currParm.parmStruct.protype.Equals("T") Then
                        Ssutil.UtDropTable(Constant.FT, currParm.parmStruct.proname)
                    End If

                    ' Update parm rec with the number of cases where interference was found.
                    If CSharpImpl.__Assign(rc, TpRunTsip.TpRunTsip.UpdateParmRec(numIntCases, numTeIntCases, parmName, currParm)) <> Constant.SUCCESS Then
                        ErrMsg.UtPrintMessage(rc)
                        exitCode = Constant.FAILURE
                        Continue For
                    End If

                    'AH: HERE   
                    Console.Out.Flush()

                    ' Get the total elapsed real-time.
                    Dim timeDiff = stopWatch.Elapsed.TotalSeconds

                    ' Get the sum of the 'User' and 'Kernel' CPU times for the current process.
                    clockTime = GenUtil.WinClock()

                    ' Get the end date and time.
                    GenUtil.UtGetDateTime(endDate, endTime)

                    '================================
                    ' Start of report production.   =
                    '================================

                    If CSharpImpl.__Assign(rc, TpRunTsip.TpRunTsip.ReportStudy(Info.DbName, Info.PdfName, Info.ProjectCode, currParm, parmName, siteName, anteName, chanName, clockTime, timeDiff, numStnGroups, TsEsStnGroups, EsTsStnGroups, Info.DestName)) <> Constant.SUCCESS Then
                        ErrMsg.UtPrintMessage(rc)
                        exitCode = Constant.FAILURE
                        Continue For
                    ElseIf numTeIntCases < 0 Then
                        ErrMsg.UtPrintMessage([Error].GENERROR, "No interference cases to report")
                    End If

                    If isTS Then

                        GenUtil.UtCvtName(Constant.TT_ANTE, viewName, anteName)
                        GenUtil.UtCvtName(Constant.TT_CHAN, viewName, chanName)

                        TpReport.CreateTTStatRep(currParm.parmStruct.protype, siteName, cUnique)
                        '	cUnique is the name of the temporary table created in this routine, used later.

                        cUniqueEnv = ""     '	Only used in ES 
                    Else

                        GenUtil.UtCvtName(Constant.TE_SITE, viewName, siteName)
                        GenUtil.UtCvtName(Constant.TE_ANTE, viewName, anteName)
                        GenUtil.UtCvtName(Constant.TE_CHAN, viewName, chanName)

                        TpReport.CreateETStatRep(currParm.parmStruct.protype, siteName, anteName, cUnique, cUniqueEnv)
                    End If

                    rc = TpRunTsip.TpRunTsip.ReportNew(Info.DbName, Info.PdfName, currParm, numIntCases, numTeIntCases, numStnGroups, viewName, Info.DestName, cUnique, cUniqueEnv, isTS)

                    If rc <> Constant.SUCCESS Then
                        ErrMsg.UtPrintMessage(rc)
                        exitCode = Constant.FAILURE
                        Continue For
                    Else
                        If numTeIntCases < 0 Then
                            ErrMsg.UtPrintMessage([Error].GENERROR, "No interference cases to report")
                        End If
                    End If

                    ' if user requested an execution report, write it 
                    If TpRunTsip.TpRunTsip.mReports.Exec Then
                        'AH: the following P/Invoke call can be removed once all code is in C#.
                        'mdArcStep = getGlbParmParmArcStep();

                        TpRunTsip.TpRunTsip.mReports.ExecWritten = True

                        TpRunTsip.TpRunTsip.TpExecRpt(TpRunTsip.TpRunTsip.mTW_EXEC, Info.PdfName, currParm.parmStruct, startTime, startDate, endTime, endDate, isTS, numStnGroups, TsEsStnGroups, EsTsStnGroups, numIntCases, numTeIntCases, currParm.parmStruct.tsorbout, Info.DestName)
                    End If

                    ' Write the EXPORT report 
                    If TpRunTsip.TpRunTsip.mReports.Export Then
                        TpRunTsip.TpRunTsip.mReports.ExportWritten = True

                        TpRunTsip.TpRunTsip.TpExportRpt(currParm.parmStruct.proname, currParm.parmStruct.protype)
                    End If

                    Log2.v(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): mReports = " & TpRunTsip.TpRunTsip.mReports.ToString())

                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    ' We can now remove the temporary table tsip_stat_rep
                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                    Ssutil.KillTable(cUnique)

                    GenUtil.FileGateClose(cLockFile)  ' Allow others to run this combo 

                    ' Report sets are 'per-record' so we need to close the TextWriter
                    ' streams and tidy-up.
                    Console.Write(Microsoft.VisualBasic.Constants.vbLf & "{0}", TpRunTsip.TpRunTsip.mReports.ToString())
                    Call TpRunTsip.TpRunTsip.CloseReportStreams()
                    Call TpRunTsip.TpRunTsip.DeleteUnwantedReportFiles()

                    ' Calculate and write normalized report content MD5 checksums into a table.
                    Call TpRunTsip.TpRunTsip.mReports.WritePerRunReportsToDbTable()

                Next ' end for (each parameter record) 

                ' Close the error stream.
                Call TpRunTsip.TpRunTsip.mTW_ERR.Close()

                ' There is one ERR report created that encompasses multiple runs.
                ' We can only calculate the normalized MD5 checksum of the ERR report
                ' after all runs have been completed and its textwriter has been closed.
                TpRunTsip.TpRunTsip.mReports.WriteRunReportToDbTable(TpRunTsip.TsipReportHelper.ErrFilePath)

                ' Insert a final record in the TsipReports table that provides a
                ' "checksum of all checksums".
                Call TpRunTsip.TpRunTsip.mReports.InsertFinalMD5allRunsandReports()

                ' We have a normal completion of Main() - perform final housekeeping and exit.
                BiUtil.BiBillingRec(Constant.BI_END, "")

                Ssutil.UtDisconnect(userSession)

                Qutils.ExitQueue(Info.DbName, "READ")

                Console.Out.Flush()

                Application.Exit("Successful normal exit from TpRunTsip.Main()", exitCode)
            Catch e As Exception
                Qutils.ExitQueue(Info.DbName, "READ")

                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): exception caught: " & e.Message)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.Main(): stack trace: " & Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & e.StackTrace)

                Application.Exit([Error].FATAL_EXCEPTION)
            End Try

        End Sub ' End of Main()

        ''' <summary>
        ''' This method writes a 'usage' message to Console.Out that provides a 
        ''' succinct summary of mandatory and optional arguments when the program
        ''' is run from the Windows command line.
        ''' </summary>
        Public Shared Sub WriteUsageToConsole()
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " This Terrestial Station (TS) and Earth Station (ES) interference analysis program is  +  ")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " the core of the MICS system. It inputs data describing new or modified radio systems  +")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " and identifies cases of interference with existing radio systems. TSIP provides ")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " detailed analysis reports for TS-TS, TS-ES and ES-TS interference cases.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " USAGE: TpRunTsip <dbName> <projCode> <paramTableName> [-o<prefix>] [-u<micsUser>] [-t]")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " ===== ")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        dbName           : database name, e.g. 'fcsa'.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        projCode         : user's project 'charge' code.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        paramTableName   : the XXX in TSIP parameter table tp_XXX_parm listed by WebMICS using:")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "                                --> Interference Analysis (TSIP)")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "                                    --> Open TSIP Parameter Files")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        --- Options ---------------------------------------------------------------------------")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        -o<prefix>       : the output reports are named <prefix>_<tableName>.CASEDET etc.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        -u<micsUser>     : overrides the environment variable 'MICSUSER'.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        -t               : writes all reports, for all runs, into a single DB table, ")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "                         : if <paramTableName> is ""tstest0183"" the table name is ""tstest0183_tsip_reports"" .")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " <...>  indicates a mandatory argument." & Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " [...]  indicates an optional argument.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " e.g.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "        TpRunTsip  fcsa  hulme1_0  es300km  -oTSIP -t")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " Notes:")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "       1. This program *must* be called with qty. 3 mandatory command-line arguments.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "       2. The default is that all reports are written to the console.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "       3. If the -o<prefix> option is used then reports are written to files.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "       4. Multiple files are written, one for each report type.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " TSIP reports directory:")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "      1. If TpRunTsip is launched by TsipInitiator then the required TSIP reports")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "         directory is passed via the environment variable TARGETDIRFORTSIPREPORTS.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "      2. TpRunTsip.exe checks to see if TARGETDIRFORTSIPREPORTS has been set, or not:")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "            - if TARGETDIRFORTSIPREPORTS is set then its value provides the ")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "              target directory for the TSIP reports.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "            - if TARGETDIRFORTSIPREPORTS is not set then the TSIP reports ")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & "              are written to the Windows environment's current directory.")
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf & " Build: {0}" & Microsoft.VisualBasic.Constants.vbCrLf, Info.ManagedBuildInfo())
        End Sub

        ''' <summary>
        ''' This method parses the command line arguments prescribed by the user.
        ''' </summary>
        ''' <paramname="args"> - User prescribed command line arguments.</param>
        Private Shared Sub ParseCommandLineArguments(args As String())
            If args.Length = 0 Then
                Call TpRunTsip.TpRunTsip.WriteUsageToConsole()
                Application.ExitQuietly([Error].COMMAND_LINE_ERROR)
            End If

            ' Create separate lists of 'flag' args prefixed with '-' and those that
            ' are not.
            Dim flagArgs As List(Of String) = New List(Of String)()
            Dim regularArgs As List(Of String) = New List(Of String)()

            For Each arg In args
                Console.Write(Microsoft.VisualBasic.Constants.vbLf & " args" & arg & Microsoft.VisualBasic.Constants.vbLf)
                If Strings.IsNumeric(arg) Then
                    regularArgs.Add(arg)
                ElseIf arg.StartsWith("-") Then
                    flagArgs.Add(arg)
                Else
                    regularArgs.Add(arg)
                End If
            Next

            ' Parse the flags.
            For Each arg In flagArgs
                Console.Write(Microsoft.VisualBasic.Constants.vbLf & " flagArgs" & arg & Microsoft.VisualBasic.Constants.vbLf)

                ' Check that we don't just have a minus character.
                If arg.Length = 1 Then
                    Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " Invalid flag: '{0}'", arg)
                    Call TpRunTsip.TpRunTsip.WriteUsageToConsole()
                    Application.ExitQuietly([Error].COMMAND_LINE_ERROR)
                End If

                ' Process the flags.
                Dim flag As String = arg.Substring(0, 2).ToUpper()
                Select Case flag
                    Case "-O"
                        Info.DestName = arg.Substring(2)

                        If Strings.HasValidFileNameChars(Info.DestName) Then
                            TpRunTsip.TsipReportHelper.OutputToFiles = True
                        Else
                            Dim str = " ERROR: string following -o contains invalid characters."
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.ParseCommandLineArguments(): " & str)
                            Application.Exit(str, 1)
                        End If
                    Case "-U"
                        TpRunTsip.TpRunTsip.mMicsUserViaCommandLine = arg.Substring(2)

                        If String.IsNullOrWhiteSpace(TpRunTsip.TpRunTsip.mMicsUserViaCommandLine) Then
                            Dim str = " ERROR: string following -u must be a valid MICSUSER name."
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.ParseCommandLineArguments(): " & str)
                            Application.Exit(str, 1)
                        End If
                    Case "-T"
                        If Equals(arg.ToUpper(), "-T") Then TpRunTsip.TsipReportHelper.OutputToTable = True
                    Case Else
                        Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " ERROR: Invalid flag: {0}" & Microsoft.VisualBasic.Constants.vbCrLf, arg)
                        Call TpRunTsip.TpRunTsip.WriteUsageToConsole()
                        Application.ExitQuietly([Error].COMMAND_LINE_ERROR)
                End Select
            Next

            ' Parse the mandatory arguments; there must be qty. 3 of them.
            If regularArgs.Count <> 3 Then
                Console.Write(Microsoft.VisualBasic.Constants.vbCrLf & " ERROR: Invalid number of arguments." & Microsoft.VisualBasic.Constants.vbCrLf)
                Call TpRunTsip.TpRunTsip.WriteUsageToConsole()
                Application.ExitQuietly([Error].COMMAND_LINE_ERROR)
            End If

            ' Parse the args.
            Console.Write(Microsoft.VisualBasic.Constants.vbLf & " regularArgs" & regularArgs(0) & ":" & regularArgs(1) & ":" & regularArgs(2) & Microsoft.VisualBasic.Constants.vbLf)
            Info.DbName = regularArgs(0)
            Info.ProjectCode = regularArgs(1)

            ' This is more precisely the XXX in the parameter table schema.tp_XXX_parm
            Info.PdfName = regularArgs(2)

            Log2.v(Microsoft.VisualBasic.Constants.vbLf & "Info.DbName      = " & Info.DbName)
            Log2.v(Microsoft.VisualBasic.Constants.vbLf & "Info.ProjectCode = " & Info.ProjectCode)
            Log2.v(Microsoft.VisualBasic.Constants.vbLf & "Info.DestName    = " & Info.DestName)
            Log2.v(Microsoft.VisualBasic.Constants.vbLf & "Info.PdfName     = " & Info.PdfName)
        End Sub

        ''' <summary>
        ''' This method attempts to get the values of a prescribed set of
        ''' environmental variables that the USER may have set in the Windows
        ''' environment in which this application is being executed. </summary>
        ''' </summary>
        ''' <remarks>
        ''' Only two 
        ''' variables *must* be set in the User's environment: 'MICSUSER' and 
        ''' 'PASSWORD'. The actual value of PASSWORD can be anything; TSIP does 
        ''' not use its value.
        ''' </remarks>
        ''' <param name=""></param>
        Private Shared Sub GetEnvVarsAndSysData()
            ' Windows environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER")     ' REQUIRED.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD")         ' REQUIRED (but can be anything).
            Info.MicsCtxCalc = Environment.GetEnvironmentVariable("MICS_CTX_CALC")
            Info.FcsaMaps50K = Environment.GetEnvironmentVariable("FCSAMAPS50K")
            Info.FcsaMaps250K = Environment.GetEnvironmentVariable("FCSAMAPS250K")
            Info.TsipReportsDir = Environment.GetEnvironmentVariable("TARGETDIRFORTSIPREPORTS")
            Info.WorkDir = Environment.GetEnvironmentVariable("WORK_DIR")

            ' If TpRunTsip is launched using WebMICS, as a process spawned by 
            ' TsipInitiator, then WebMICS sets MICSUSER and PASSWORD as per
            ' the user's WebMICS login paramaters.

            ' Check if the value to be used for MICSUSER was prescribed on the command-line.
            If Not String.IsNullOrWhiteSpace(TpRunTsip.TpRunTsip.mMicsUserViaCommandLine) Then
                Info.MicsUserName = TpRunTsip.TpRunTsip.mMicsUserViaCommandLine
            End If

            ' MICSUSER *must* be set in the user's environment.
            If Equals(Info.MicsUserName, Nothing) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.GetEnvVarsAndSysData(): ERROR: Environment variable MICSUSER is not set.")
                Application.Exit(387)
            End If

            ' PASSWORD *should* be set in the user's environment but it can have 
            ' any non-blank value.
            If Equals(Info.Password, Nothing) Then
                '...Log2.v("\nTpRunTsip.GetEnvVarsAndSysData(): Environment variable PASSWORD is not set.");
                Info.Password = "TheUserDidNotSetApassword"
            End If

            ' Set the TSIP reports directory.
            ' If this instance of TpRunTsip was spawned by TsipInitiator then
            ' the required TSIP reports directory is passed via the environment
            ' variable TARGETDIRFORTSIPREPORTS.
            '
            ' TpRunTsip.exe checks to see if TARGETDIRFORTSIPREPORTS has been set.
            '     - If TARGETDIRFORTSIPREPORTS is set then its value provides the 
            '       target directory for the TSIP reports.
            '     - If TARGETDIRFORTSIPREPORTS is not set then the TSIP reports 
            '       are written to the Windows environment's current directory.

            If Equals(Info.TsipReportsDir, Nothing) Then
                ' The environment variable TARGETDIRFORTSIPREPORTS has not been set.
                ' We need to provide an alternative destination for the TSIP report files.
                ' Use the directory Windows defaults to for File i/0 for relative file paths;
                ' If TpRunTsip is run in a Windows command prompt window, the default relative
                ' directory is the current directory.
                '...Log2.v("\nTpRunTsip.GetEnvVarsAndSysData(): Environment variable TARGETDIRFORTSIPREPORTS is not set.");

                Info.TsipReportsDir = ""
            Else
                ' Ensure that the last char is a backslash.
                If Not Strings.LastCharIs(Info.TsipReportsDir, "\"c) Then
                    Info.TsipReportsDir += "\"
                End If
            End If

            '...Log2.v("\nInfo:\n" + Info.ToString());

        End Sub


        ''' <summary>
        ''' This method creates the TSIP error file (truncating it if runname is
        ''' NULL), and redirects output to it before each Tsip parameter record is
        ''' processed. For each Tsip parameter record, a header is created that
        ''' includes the runname. This is neccessary because the reports redirect
        ''' the output, and the errors would be written to the EXEC report if this
        ''' were not done.
        ''' </summary>
        ''' <paramname="destname"> - the destination name given by the user.</param>
        ''' <paramname="tempName"> - the name of the Parameter file.</param>
        ''' <paramname="runname"> - the run name given within the Parameter record 
        ''' (null indicates that the file is to be created and truncated for the beginning of the TSIP run.</param>
        ''' <returns>Constant.SUCCESS or Constant.FAILURE</returns>
        Private Shared Function CreateErrorFile(destname As String, tempName As String, runname As String) As Integer
            '...Log2.v("\nTpRunTsip.CreateErrorFile(): Entry: " + destname + "   " + tempName);

            Dim errorrep As String
            Dim shortrunname As String
            Dim [cDate] As String
            Dim cTime As String

            ' The first call to CreateErrorFile() should have runname set to null to
            ' indicate that the error report file is to be created and truncated for 
            ' for the beginning of the TSIP run.
            ' Subsequent calls to CreateErrorFile() should set runname to a non-null 
            ' value indicating that we will append to the existing contents of the 
            ' error report file.
            If Equals(runname, Nothing) Then
                '...Log2.v("\nTpRunTsip.CreateErrorFile(): runname is NULL.");

                ' If redirection of output to file was NOT prescribed on the command line
                ' then we just have to set the mTW_ERR TextWriter stream to standard Console.Out
                If TpRunTsip.TsipReportHelper.OutputToFiles Then
                    ' Construct the full name of the error report file.
                    errorrep = Info.TsipReportsDir & String.Format("{0}_{1}.ERR", destname, tempName)

                    ' There is only one ERR report created even if the parameter file comprises multiple runs.
                    ' Hence the following are statics.
                    TpRunTsip.TsipReportHelper.ErrWritten = True
                    TpRunTsip.TsipReportHelper.ErrFilePath = errorrep

                    Try
                        File.Delete(destname)
                        Dim path = IO.Path.Combine(Info.TsipReportsDir, errorrep)

                        '...Log2.v("\nTpRunTsip.CreateErrorFile(): error report = " + path);
                        TpRunTsip.TpRunTsip.mTW_ERR = New StreamWriter(path, False)  ' Truncate.
                    Catch e As Exception
                        ' Could not open error report file for writing.
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.CreateErrorFile(): ERROR: " & e.Message)
                        Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "ERROR OPENING ERROR FILE - '{0}'" & Microsoft.VisualBasic.Constants.vbLf, errorrep)
                        Log2.e(str)
                        Return Constant.FAILURE
                    End Try
                Else
                    '...Log2.v("\nTpRunTsip.CreateErrorFile(): error report: to stdout");
                    TpRunTsip.TpRunTsip.mTW_ERR = TpRunTsip.TpRunTsip.mStandardConsoleOut
                End If  ' not the first call
            Else
                '...Log2.v("\nTpRunTsip.CreateErrorFile(): runname is not NULL.");

                GenUtil.UtGetDateTime([cDate], cTime)
                shortrunname = runname.Trim()
                Dim sb As StringBuilder = New StringBuilder()
                sb.Append(Microsoft.VisualBasic.Constants.vbCrLf & "FREQUENCY COORDINATION SYSTEM ASSOCIATION" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf)
                sb.Append(String.Format("TSIP build {0} Error Report for Run Name '{1}', at {2} {3}" & Microsoft.VisualBasic.Constants.vbCrLf & "Project Code [{4}] Process ID {5}" & Microsoft.VisualBasic.Constants.vbCrLf, Info.BuildMetaData, shortrunname, [cDate], cTime, Info.ProjectCode, Info.ProcessID))
                TpRunTsip.TpRunTsip.mTW_ERR.Write(sb)
                Call TpRunTsip.TpRunTsip.mTW_ERR.Flush()
            End If

            '...Log2.v("\nTpRunTsip.CreateErrorFile(): runname = " + runname);
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' ParmFileInit reads the parameter records from a DB 'tt_*_parm' table, and stores 
        ''' them in an internal array of parameter records.  
        ''' </summary>
        ''' <paramname="tempName"> - prescribed unique file name substring.</param>
        ''' <paramname="parmTableCount"> - provides a cummulative 'running' count of parm tables.</param>
        ''' <paramname="currParmTableList"> - list of ParmTableWN objects; WN = 'with nulls'.</param>
        ''' <returns></returns>
        Private Shared Function ParmFileInit(tempName As String, ByRef parmTableCount As Integer, <Out> ByRef currParmTableList As List(Of TpRunTsip.TpRunTsip.ParmTableWN)) As Integer
            '...Log2.v("\nTpRunTsip.ParmFileInit(): Entry");

            Dim tableName As String
            Dim parmHandle As Integer
            Dim rc As Integer
            currParmTableList = Nothing

            GenUtil.UtCvtName(Constant.TP_PARM, tempName, tableName)

            ' initialize 
            Dim str = String.Format(Microsoft.VisualBasic.Constants.vbCrLf & "{0}" & Microsoft.VisualBasic.Constants.vbCrLf & "{0} TSIP PARAMETER FILE NAME: {1}" & Microsoft.VisualBasic.Constants.vbCrLf & "{0} Project Code [{2}]" & Microsoft.VisualBasic.Constants.vbCrLf, Constant.COMMENT_CHAR, tempName, Info.ProjectCode)
            TpRunTsip.TpRunTsip.mTW_ERR.Write(str)

            ' Read all parameter records from file and populate the parameter table.
            parmHandle = TpDynParm.TpSelectParm(tableName, "", "runname")

            If parmHandle < 0 Then
                Call TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*ERROR* parmFileInit: Could not open parameter table ({0}):-" & Microsoft.VisualBasic.Constants.vbCrLf & "{1}" & Microsoft.VisualBasic.Constants.vbCrLf, parmHandle, GenUtil.GetUserMess())
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.ParmFileInit(): call to TpDynParm.TpSelectParm() FAILED for tableName = " & tableName)
                Return parmHandle
            End If

            '...Log2.v("\n\nTpRunTsip.ParmFileInit(): call to TpDynParm.TpSelectParm() SUCCEEDED for tableName = " + tableName);

            Dim tpParm As TpParm
            Dim nullInd As SQLLEN()
            currParmTableList = New List(Of TpRunTsip.TpRunTsip.ParmTableWN)()

            While CSharpImpl.__Assign(rc, TpDynParm.TpFetchParm(parmHandle, tpParm, nullInd)) = Constant.SUCCESS
                Dim parmTable As TpRunTsip.TpRunTsip.ParmTableWN = New TpRunTsip.TpRunTsip.ParmTableWN()
                parmTable.parmStruct = tpParm
                parmTable.parmNulls = nullInd
                currParmTableList.Add(parmTable)

                parmTableCount += 1
                If parmTableCount >= Constant.MAXPARMREC Then
                    ErrMsg.UtPrintMessage([Error].MAXPARMS, Constant.MAXPARMREC.ToString())
                    Exit While
                End If
            End While

            If rc <> Constant.NOMORERECS Then
                Call TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*ERROR* parmFileInit: Could not fetch parameter record({0}):-" & Microsoft.VisualBasic.Constants.vbCrLf & "{1}" & Microsoft.VisualBasic.Constants.vbCrLf, rc, GenUtil.GetUserMess())
                Return rc
            End If

            TpDynParm.TpCloseParm(parmHandle)

            '...Log2.v("\nTpRunTsip.ParmFileInit(): Exit");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method deletes any existing reports produced by previous runs
        ''' of TSIP with the same destination directory argument as the current run.
        ''' </summary>
        ''' <paramname="destName"> - directory that new TSIP reports shall be written to.</param>
        ''' <paramname="tempName"> - PDF table name</param>
        ''' <paramname="runName"> - name that User prescribed to identify this run.</param>
        Public Shared Sub RemoveStudyFiles(destName As String, tempName As String, runName As String)
            If Equals(destName, Nothing) OrElse Equals(tempName, Nothing) OrElse Equals(runName, Nothing) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.RemoveStudyFiles(): Error: at least one of the three input strings was null.")
                Return
            End If

            ' Just in case ...
            destName = destName.Trim()
            tempName = tempName.Trim()
            runName = runName.Trim()

            Dim fileStub = String.Format("{0}_{1}_{2}", destName, tempName, runName)

            ' Behaviour of File.Delete(): 
            ' If the file to be deleted does not exist, no exception is thrown.
            ' There are many reasons why an exception *could* be thrown, 
            ' e.g. the file path is invalid, the user does not have sufficient permissions,
            ' the specified file has been openned for use by another process etc.
            Try
                '	If there is a destName, then these files may have been created 
                File.Delete(fileStub & ".EXEC")

                File.Delete(fileStub & ".STATSUM")

                File.Delete(fileStub & ".CASEDET")

                File.Delete(fileStub & ".CASESUM")

                File.Delete(fileStub & ".STUDY")

                File.Delete(fileStub & ".ORBIT")

                File.Delete(fileStub & ".AGGINTREP")

                File.Delete(fileStub & ".HILO")

                File.Delete(fileStub & "_ts.EXPORT")

                File.Delete(fileStub & "_es.EXPORT")

                '	If this is created, it always created as a file 
                File.Delete(fileStub & ".CSV")
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.RemoveStudyFiles(): Error: File.Delete() threw an exception: " & e.Message)
            End Try

            '...Log2.v("\nTpRunTsip.RemoveStudyFiles(): fileStub = " + fileStub);
            Return
        End Sub

        ''' <summary>
        ''' This method prints a flat-file representation of the current record, and 
        ''' copies the record to a 'tt_*_parm' or 'te_*_parm' table to be saved as part of 
        ''' the SH Tables.  
        ''' </summary>
        ''' <paramname="currParm"> - ParmTableWN object containing parameter values and associated ODBC nullInds. </param>
        ''' <returns></returns>
        Public Shared Function ParmRecInit(currParm As TpRunTsip.TpRunTsip.ParmTableWN) As Integer
            '...Log2.v("\nTpRunTsip.ParmRecInit(): Entry");

            Dim isValid As Boolean         ' return code from TsipValid 
            Dim type As String    ' ES or TS - type of pdf 

            '	Zero the maximums for the calculations and the report 
            TpRunTsip.TpRunTsip.mdTxTro = 0.0
            TpRunTsip.TpRunTsip.mdRxTro = 0.0
            TpRunTsip.TpRunTsip.mdTxPre = 0.0
            TpRunTsip.TpRunTsip.mdRxPre = 0.0

            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("Proposed type...........: {0}" & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("Environment type........: {1}" & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("Proposed Name...........: {2}" & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("Environment Name........: {3}" & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("TSORB Report............: {4}" & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("Loss Calculation Type...: {5}" & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("Frequency Separation....: {6:#.00}")

            Call TpRunTsip.TpRunTsip.mTW_ERR.Write(sb.ToString(), currParm.parmStruct.protype.Trim(), currParm.parmStruct.envtype.Trim(), currParm.parmStruct.proname.Trim(), currParm.parmStruct.envname.Trim(), currParm.parmStruct.tsorbout.Trim(), currParm.parmStruct.spherecalc.Trim(), currParm.parmStruct.fsep)

            '	Set the frequency separation for the band adjacency seaisValidh
            Suutils.SuSetAdjFreq(currParm.parmStruct.fsep)

            If currParm.parmStruct.protype.Equals("E") Then
                '	Es Coordination distance is taken from the antenna records 
                Dim cLastLoc = ""
                Dim cNextLoc As String
                Dim feSiteStr As FeSiteStr
                Dim nInd As Integer

                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "Coordination Distance...:- (by site)")
                While FeUtils.FeNextSite(cLastLoc, cNextLoc, feSiteStr, 2, currParm.parmStruct.proname) = 0
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "             {0}   Call1            Tropo Precip", feSiteStr.stSite.location.PadRight(10))
                    For nInd = 0 To feSiteStr.nNumAnts - 1
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "                        : {0} TX {1,6:#.} {1,6:#.}" & Microsoft.VisualBasic.Constants.vbCrLf & "                                       RX {2,6:#.} {3,6:#.}", feSiteStr.stAnts(nInd).call1.PadRight(12), feSiteStr.stAnts(nInd).txtro, feSiteStr.stAnts(nInd).txpre, feSiteStr.stAnts(nInd).rxtro, feSiteStr.stAnts(nInd).rxpre)

                        '	Now save the maximums for the Study report 
                        If feSiteStr.stAnts(nInd).txtro > TpRunTsip.TpRunTsip.mdTxTro Then
                            TpRunTsip.TpRunTsip.mdTxTro = feSiteStr.stAnts(nInd).txtro
                        End If
                        If feSiteStr.stAnts(nInd).txpre > TpRunTsip.TpRunTsip.mdTxPre Then
                            TpRunTsip.TpRunTsip.mdTxPre = feSiteStr.stAnts(nInd).txpre
                        End If
                        If feSiteStr.stAnts(nInd).rxtro > TpRunTsip.TpRunTsip.mdRxTro Then
                            TpRunTsip.TpRunTsip.mdRxTro = feSiteStr.stAnts(nInd).rxtro
                        End If
                        If feSiteStr.stAnts(nInd).rxpre > TpRunTsip.TpRunTsip.mdRxPre Then
                            TpRunTsip.TpRunTsip.mdRxPre = feSiteStr.stAnts(nInd).rxpre
                        End If
                    Next

                    cLastLoc = cNextLoc  '	Get the next after this. 
                End While

                ' Store the max for final printout.
                ' We intentionally do not set the corresponding nullInd for the coordist column
                ' so that, if it is initially NULL in the Parm table, then it is written back to
                ' the Parm table as a NULL.
                currParm.parmStruct.coordist = Math.Max(Math.Max(TpRunTsip.TpRunTsip.mdTxTro, TpRunTsip.TpRunTsip.mdTxPre), Math.Max(TpRunTsip.TpRunTsip.mdRxTro, TpRunTsip.TpRunTsip.mdRxPre))
            Else
                '	Tx coordination distance is taken from the input parameters. 
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "Coordination Distance...: {0:#.00}", currParm.parmStruct.coordist)
            End If

            Call TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "Analysis Option.........: {0}" & Microsoft.VisualBasic.Constants.vbCrLf & "Margin Requested........: {1:0.00}" & Microsoft.VisualBasic.Constants.vbCrLf & "Channel Status Codes....: ({2}) {3}" & Microsoft.VisualBasic.Constants.vbCrLf & "Country Selection.......: {4}" & Microsoft.VisualBasic.Constants.vbCrLf & "Site Selection..........: {5}" & Microsoft.VisualBasic.Constants.vbCrLf & "Codes...................: ({6}) {7}" & Microsoft.VisualBasic.Constants.vbCrLf & "Run Name................: {8}" & Microsoft.VisualBasic.Constants.vbCrLf & "Keyword Parameter Field.: ""{9}""" & Microsoft.VisualBasic.Constants.vbCrLf & "Date....................: {10}" & Microsoft.VisualBasic.Constants.vbCrLf & "Time....................: {11}" & Microsoft.VisualBasic.Constants.vbCrLf, currParm.parmStruct.analopt.Trim(), currParm.parmStruct.margin, currParm.parmStruct.numchan, currParm.parmStruct.chancodes.Trim(), currParm.parmStruct.country.Trim(), currParm.parmStruct.selsites.Trim(), currParm.parmStruct.numcodes, currParm.parmStruct.codes.Trim(), currParm.parmStruct.runname.Trim(), currParm.parmStruct.parmparm.Trim(), currParm.parmStruct.mdate.Trim(), currParm.parmStruct.mtime.Trim())

            TpRunTsip.TpRunTsip.mTW_ERR.Write([Error].MsgForCode([Error].WORKING) & Microsoft.VisualBasic.Constants.vbCrLf)
            Call TpRunTsip.TpRunTsip.mTW_ERR.Flush()

            ' --- This section checks validation status -----------------------------------------

            ' check validation of proposed pdf 
            Dim retCode As Integer

            If currParm.parmStruct.protype.Equals("E") Then
                type = "ES"
                isValid = TpRunTsip.TpRunTsip.TsipValid(Constant.FE, currParm.parmStruct.proname, retCode)
            Else
                type = "TS"
                isValid = TpRunTsip.TpRunTsip.TsipValid(Constant.FT, currParm.parmStruct.proname, retCode)
            End If

            If Not isValid Then
                If retCode = Constant.FAILURE Then
                    ErrMsg.UtPrintMessage([Error].NOTTSIPVALID, type, currParm.parmStruct.proname)
                End If
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.ParmRecInit(): ERROR: TsipValid() returned false: A")
                Return retCode
            End If

            ' check validation of environment pdf ------------------------------------------------
            If currParm.parmStruct.envtype.Equals("PDF_ES") Then
                type = "ES"
                isValid = TpRunTsip.TpRunTsip.TsipValid(Constant.FE, currParm.parmStruct.envname, retCode)
            ElseIf currParm.parmStruct.envtype.Equals("PDF_TS") Then
                type = "TS"
                isValid = TpRunTsip.TpRunTsip.TsipValid(Constant.FT, currParm.parmStruct.envname, retCode)
            End If

            If Not isValid Then
                If retCode = Constant.FAILURE Then
                    ErrMsg.UtPrintMessage([Error].NOTTSIPVALID, type, currParm.parmStruct.envname)
                End If
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.ParmRecInit(): ERROR: TsipValid() returned false: B")
                Return retCode
            End If

            '...Log2.v("\nTpRunTsip.ParmRecInit(): Exit: SUCCESS");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' TsipValid checks the validation status of the proposed file, and the 
        ''' environment file for proper validation to run TSIP.  
        ''' </summary>
        ''' <paramname="tableType"> - eponym</param>
        ''' <paramname="tableName"> - eponym</param>
        ''' /// <paramname="retCode"> - return code.</param>
        ''' <returns></returns>
        Private Shared Function TsipValid(tableType As Integer, tableName As String, <Out> ByRef retCode As Integer) As Boolean
            ' 'out' requirement.
            retCode = Integer.MinValue

            Dim validatedFor As String
            Dim result = False

            If FilewUtil.UtFilewValidated(tableType, tableName, validatedFor) <> Constant.SUCCESS Then
                ' False.
                retCode = [Error].FILEWVAL
                Return False
            End If

            Select Case validatedFor(0)
                Case Constant.TSIP_VALIDATED, Constant.UPDATE_VALIDATED, Constant.UPDATE_POSTED
                    retCode = Constant.SUCCESS
                    result = True
                Case Constant.M_UPDATE_VALIDATED, Constant.M_TSIP_VALIDATED
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "Some sites are missing.  Links to them will be ignored in processing." & Microsoft.VisualBasic.Constants.vbLf)
                    retCode = Constant.SUCCESS
                    result = True
                Case Else
                    ' Must be false.
                    retCode = Constant.FAILURE
                    result = False
            End Select

            Return result
        End Function

        ''' <summary>
        ''' This methods constructs a textual .ORBIT report and
        ''' writes it to a file.
        ''' </summary>
        ''' <paramname="destname"> - target directory.</param>
        ''' <paramname="tempName"> - name of PDF file.</param>
        ''' <paramname="runname"> - User prescribed ID for this run.</param>
        ''' <returns></returns>
        Private Shared Function CreateOrbitFile(destname As String, tempName As String, runname As String) As Integer
            ' The Orbit textWriter is already instantiated.

            Dim projCode As String
            GenUtil.GetProjectCode(projCode)

            Dim sb As StringBuilder = New StringBuilder()

            sb.Append("                    ")
            sb.Append("FREQUENCY COORDINATION SYSTEM ASSOCIATION" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf)
            sb.Append("                                 ")
            sb.Append("MICS Orbit Report" & Microsoft.VisualBasic.Constants.vbCrLf & "Project Code ")
            sb.Append("[" & projCode & "]" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf)

            TpRunTsip.TpRunTsip.mTW_ORBIT.Write(sb)
            Call TpRunTsip.TpRunTsip.mTW_ORBIT.Flush()

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method updates the parameter table with the value of the current 
        ''' parameter record.  
        ''' </summary>
        ''' <paramname="numCases"> - number of cases.</param>
        ''' <paramname="numTeCases"> - number of TE cases.</param>
        ''' <paramname="parmName"> - name of paramater file.</param>
        ''' <paramname="currParm"> - ParmTableWN object containing paramter values and associated ODBC nullInds.</param>
        ''' <returns> - Constant.SUCCESS or non-zero failure code.</returns>
        Private Shared Function UpdateParmRec(numCases As Integer, numTeCases As Integer, parmName As String, currParm As TpRunTsip.TpRunTsip.ParmTableWN) As Integer
            Dim rc, parmHandle As Integer
            Dim [select] As String

            Dim tmpParm As TpParm
            Dim tmpNulls = New SQLLEN(26) {}

            ' Update parm rec w/ numCases.
            [select] = String.Format("runname = '{0}'", currParm.parmStruct.runname.PadRight(5))

            If CSharpImpl.__Assign(parmHandle, TpDynParm.TpSelectParm(parmName, [select], "")) >= 0 Then
                If CSharpImpl.__Assign(rc, TpDynParm.TpFetchParm(parmHandle, tmpParm, tmpNulls)) <> Constant.SUCCESS Then
                    Return rc
                End If
                tmpParm.numcases = numCases
                tmpParm.numtecases = numTeCases
                tmpNulls(TpParm.NUMCASES) = Constant.DB_NOT_NULL
                tmpNulls(TpParm.NUMTECASES) = Constant.DB_NOT_NULL
                currParm.parmStruct.numcases = numCases
                currParm.parmStruct.numtecases = numTeCases

                If CSharpImpl.__Assign(rc, TpDynParm.TpUpdateParm(parmHandle, tmpParm, tmpNulls)) <> Constant.SUCCESS Then
                    Return rc
                End If

                TpDynParm.TpCloseParm(parmHandle)
            Else
                Return parmHandle
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method creates a '.STUDY' report and writes it out to file.
        ''' </summary>
        ''' <paramname="dBaseName"> - name of DB.</param>
        ''' <paramname="tempname"> - name of PDF file.</param>
        ''' <paramname="pcode"> - project code.</param>
        ''' <paramname="currParm"> - ParmTableWN object providing parameter values and associated nullInds.</param>
        ''' <paramname="parmName"> - name of '_parm' parameter file.</param>
        ''' <paramname="siteName"> - eponym.</param>
        ''' <paramname="anteName"> - eponym.</param>
        ''' <paramname="chanName"> - eponym.</param>
        ''' <paramname="clocktime"> - eponym.</param>
        ''' <paramname="timediff"> - eponym.</param>
        ''' <paramname="numStns"> - number of stations.</param>
        ''' <paramname="tsesStns"> - number of TsEs stations.</param>
        ''' <paramname="estsStns"> - number of EsTs stations.</param>
        ''' <paramname="destname"> - name of directory that reports are to be written to.</param>
        ''' <returns></returns>
        Private Shared Function ReportStudy(dBaseName As String, tempname As String, pcode As String, currParm As TpRunTsip.TpRunTsip.ParmTableWN, parmName As String, siteName As String, anteName As String, chanName As String, clocktime As Integer, timediff As Double, numStns As Integer, tsesStns As Integer, estsStns As Integer, destname As String) As Integer
            '...Log2.v("\nTpRunTsip.ReportStudy(): Entry");

            Dim hours, minutes, seconds, cpuhour, cpumins, cpusecs, cpumsec, tempdiff As Integer
            Dim cDistStr As String
            Dim rc As Integer
            Dim cTableName As String

            Dim q As String
            Dim r As String
            Dim s As String
            Dim t As String
            Dim u As String
            Dim v As String
            Dim w As String
            Dim x As String
            Dim y As String
            Dim et = ""
            Dim IsTS As Boolean

            tempdiff = CInt(timediff)
            minutes = CInt(timediff) / 60
            hours = minutes / 60
            minutes = minutes Mod 60
            seconds = tempdiff Mod 60
            r = String.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds)

            cpumsec = clocktime Mod 1000
            cpusecs = clocktime / 1000 Mod 60
            cpumins = clocktime / 60000 Mod 60
            cpuhour = clocktime / 3600000
            q = String.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", cpuhour, cpumins, cpusecs, cpumsec)

            If currParm.parmStruct.protype.Equals("E") OrElse currParm.parmStruct.envtype.Equals("PDF_ES") OrElse currParm.parmStruct.envtype.Equals("MDB_ES") Then
                s = String.Format("{0}", tsesStns)
                et = String.Format("{0}", estsStns)
                IsTS = False   '	It's an es study
            Else
                s = String.Format("{0}", numStns)
                IsTS = True
            End If
            If TpRunTsip.TpRunTsip.mReports.Exec Then
                t = "Y"
            Else
                t = "N"
            End If
            If TpRunTsip.TpRunTsip.mReports.TtStudy OrElse TpRunTsip.TpRunTsip.mReports.TeStudy OrElse TpRunTsip.TpRunTsip.mReports.EtStudy Then
                u = "Y"
            Else
                u = "N"
            End If

            '	Pass in the max coordination distances for an ETSTUDY  
            If TpRunTsip.TpRunTsip.mReports.EtStudy Then
                cDistStr = String.Format(""",txtro=""{0:F0}"",txpre=""{1:F0}"",rxtro=""{2:F0}"",rxpre=""{3:F0}", TpRunTsip.TpRunTsip.mdTxTro, TpRunTsip.TpRunTsip.mdTxPre, TpRunTsip.TpRunTsip.mdRxTro, TpRunTsip.TpRunTsip.mdRxPre)
            End If

            If TpRunTsip.TpRunTsip.mReports.TsTsStn OrElse TpRunTsip.TpRunTsip.mReports.TsEsStn OrElse TpRunTsip.TpRunTsip.mReports.EsTsStn Then
                v = "Y"
            Else
                v = "N"
            End If

            If TpRunTsip.TpRunTsip.mReports.TsTsDet OrElse TpRunTsip.TpRunTsip.mReports.TsEsCase OrElse TpRunTsip.TpRunTsip.mReports.EsTsCase Then
                w = "Y"
            Else
                w = "N"
            End If

            If TpRunTsip.TpRunTsip.mReports.TsTsSum OrElse TpRunTsip.TpRunTsip.mReports.EsTsSum OrElse TpRunTsip.TpRunTsip.mReports.EsTsSum Then
                x = "Y"
            Else
                x = "N"
            End If

            '	The aggregate interference reports are carried in a two character field
            If TpRunTsip.TpRunTsip.mReports.AggIntRep Then
                y = "Y"
            Else
                y = "N"
            End If
            If TpRunTsip.TpRunTsip.mReports.AggIntCsv Then
                y += "Y"
            Else
                y += "N"
            End If

            cTableName = String.Format("{0}_{1}", tempname, currParm.parmStruct.runname)
            If cTableName.Length > Constant.MAX_DISP_TAB_LEN Then
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "reportStudy01: Output table name {0} is too long.  Max length is {1}" & Microsoft.VisualBasic.Constants.vbCrLf, cTableName, Constant.MAX_DISP_TAB_LEN)
                Return Constant.FAILURE
            End If

            If IsTS AndAlso TpRunTsip.TpRunTsip.mReports.TtStudy Then
                TpRunTsip.TpRunTsip.mReports.StudyWritten = True

                rc = TpRunTsip.Tstsrp1.TsTsRp1(TpRunTsip.TpRunTsip.mTW_STUDY, currParm.parmStruct, cTableName, pcode, q, r, s, t, u, v, w, x, y)
                If rc <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_STUDY, [Error].REPORT_ERROR)
                End If

            End If

            If Not IsTS AndAlso TpRunTsip.TpRunTsip.mReports.TeStudy Then
                TpRunTsip.TpRunTsip.mReports.StudyWritten = True

                rc = TpRunTsip.Tsesrp1.TsEsRp1(TpRunTsip.TpRunTsip.mTW_STUDY, currParm.parmStruct, cTableName, pcode, q, r, s, t, u, v, w, x, et)
                If rc <> 0 Then
                    ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_STUDY, [Error].REPORT_ERROR)
                End If
            End If

            If Not IsTS AndAlso TpRunTsip.TpRunTsip.mReports.EtStudy Then
                TpRunTsip.TpRunTsip.mReports.StudyWritten = True

                rc = TpRunTsip.Estsrp1.EsTsRp1(TpRunTsip.TpRunTsip.mTW_STUDY, currParm.parmStruct, cTableName, pcode, q, r, s, t, u, v, w, x, et, TpRunTsip.TpRunTsip.mdTxTro, TpRunTsip.TpRunTsip.mdRxTro, TpRunTsip.TpRunTsip.mdTxPre, TpRunTsip.TpRunTsip.mdRxPre, currParm.parmStruct.coordist)
                If rc <> 0 Then
                    ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_STUDY, [Error].REPORT_ERROR)
                End If
            End If

            '...Log2.v("\nTpRunTsip.ReportStudy(): Exit");
            Return Constant.SUCCESS
        End Function


        ''' <summary>
        ''' This method instantiates a set of TextWriter objects, one for each of the
        ''' different report types.
        ''' </summary>
        ''' <paramname="tableName"> - name of PDF.</param>
        ''' <paramname="runname"> - User prescribed unique ID for this run.</param>
        ''' <paramname="protype"> - User prescribed run type: either "T" for Terrestial Station or "E"" for Earth Station.</param>
        Private Shared Sub OpenReportStreams(tableName As String, runname As String, protype As String)
            Dim aggintCsv As String
            Dim aggintRep As String
            Dim caseDet As String
            Dim caseOhl As String
            Dim caseSum As String
            Dim exec As String
            Dim hilo As String
            Dim orbit As String
            Dim statSum As String
            Dim study As String
            Dim export As String

            Dim tsOrEs = If(Equals(protype, "T"), "TS", "ES")

            If TpRunTsip.TsipReportHelper.OutputToFiles Then
                aggintCsv = String.Format("{0}_{1}_{2}.AGGINT.csv", Info.DestName, tableName, runname)
                aggintRep = String.Format("{0}_{1}_{2}.AGGINTREP", Info.DestName, tableName, runname)
                caseDet = String.Format("{0}_{1}_{2}.CASEDET", Info.DestName, tableName, runname)
                caseOhl = String.Format("{0}_{1}_{2}.CASEOHL", Info.DestName, tableName, runname)
                caseSum = String.Format("{0}_{1}_{2}.CASESUM", Info.DestName, tableName, runname)
                exec = String.Format("{0}_{1}_{2}.EXEC", Info.DestName, tableName, runname)
                export = String.Format("{0}_{1}_{2}.{3}_EXPORT", Info.DestName, tableName, runname, tsOrEs)
                hilo = String.Format("{0}_{1}_{2}.HILO", Info.DestName, tableName, runname)
                orbit = String.Format("{0}_{1}_{2}.ORBIT", Info.DestName, tableName, runname)
                statSum = String.Format("{0}_{1}_{2}.STATSUM", Info.DestName, tableName, runname)
                study = String.Format("{0}_{1}_{2}.STUDY", Info.DestName, tableName, runname)

                ' Set the report file paths.
                TpRunTsip.TpRunTsip.mReports.AggIntCsvFilePath = Info.TsipReportsDir & aggintCsv
                TpRunTsip.TpRunTsip.mReports.AggIntRepFilePath = Info.TsipReportsDir & aggintRep
                TpRunTsip.TpRunTsip.mReports.CaseDetFilePath = Info.TsipReportsDir & caseDet
                TpRunTsip.TpRunTsip.mReports.CaseOhlFilePath = Info.TsipReportsDir & caseOhl
                TpRunTsip.TpRunTsip.mReports.CaseSumFilePath = Info.TsipReportsDir & caseSum
                TpRunTsip.TpRunTsip.mReports.ExecFilePath = Info.TsipReportsDir & exec
                TpRunTsip.TpRunTsip.mReports.ExportFilePath = Info.TsipReportsDir & export
                TpRunTsip.TpRunTsip.mReports.HiloFilePath = Info.TsipReportsDir & hilo
                TpRunTsip.TpRunTsip.mReports.OrbitFilePath = Info.TsipReportsDir & orbit
                TpRunTsip.TpRunTsip.mReports.StatSumFilePath = Info.TsipReportsDir & statSum
                TpRunTsip.TpRunTsip.mReports.StudyFilePath = Info.TsipReportsDir & study


                '...Log2.v("\n\nTpRunTsip.OpenReportStreams(): mReports: \n" + mReports.ToString());

                Try
                    ' Instantiate all the StreamWriters.
                    TpRunTsip.TpRunTsip.mTW_AGGINTCSV = New StreamWriter(TpRunTsip.TpRunTsip.mReports.AggIntCsvFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_AGGINTREP = New StreamWriter(TpRunTsip.TpRunTsip.mReports.AggIntRepFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_CASEDET = New StreamWriter(TpRunTsip.TpRunTsip.mReports.CaseDetFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_CASEOHL = New StreamWriter(TpRunTsip.TpRunTsip.mReports.CaseOhlFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_CASESUM = New StreamWriter(TpRunTsip.TpRunTsip.mReports.CaseSumFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_EXEC = New StreamWriter(TpRunTsip.TpRunTsip.mReports.ExecFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_EXPORT = New StreamWriter(TpRunTsip.TpRunTsip.mReports.ExportFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_HILO = New StreamWriter(TpRunTsip.TpRunTsip.mReports.HiloFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_ORBIT = New StreamWriter(TpRunTsip.TpRunTsip.mReports.OrbitFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_STATSUM = New StreamWriter(TpRunTsip.TpRunTsip.mReports.StatSumFilePath, False)  ' Truncate.
                    TpRunTsip.TpRunTsip.mTW_STUDY = New StreamWriter(TpRunTsip.TpRunTsip.mReports.StudyFilePath, False)  ' Truncate.
                Catch e As Exception
                    ' Could not open an output file for writing.
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.OpenReportStreams(): ERROR: failed to open file for writing: " & Microsoft.VisualBasic.Constants.vbLf & e.Message)
                    '...Log2.v("\nERROR OPENING OUTPUT FILE - (STUDY)\n");
                    Application.Exit(666)
                End Try
            Else
                ' All output streams are routed to Console.Out except the .csv file that
                ' is always written to a disk file.
                TpRunTsip.TpRunTsip.mTW_AGGINTREP = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_CASEDET = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_CASEOHL = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_CASESUM = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_EXEC = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_EXPORT = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_HILO = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_ORBIT = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_STATSUM = TpRunTsip.TpRunTsip.mStandardConsoleOut
                TpRunTsip.TpRunTsip.mTW_STUDY = TpRunTsip.TpRunTsip.mStandardConsoleOut

                ' Handle the special case of the AGGINT.csv report
                aggintCsv = String.Format("{0}_{1}_{2}.AGGINT.csv", Info.DestName, tableName, runname)
                TpRunTsip.TpRunTsip.mReports.AggIntCsvFilePath = aggintCsv

                Try
                    TpRunTsip.TpRunTsip.mTW_AGGINTCSV = New StreamWriter(aggintCsv, False)  ' Truncate.
                Catch e As Exception
                    ' Could not open an output file for writing.
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.OpenReportStreams(): ERROR: failed to open file for writing: " & Microsoft.VisualBasic.Constants.vbLf & e.Message)
                    '...Log2.v("\nERROR OPENING OUTPUT FILE - (STUDY)\n");
                    Application.Exit(666)
                End Try
            End If
        End Sub

        ''' <summary>
        ''' This method flushes and closes all of the TextWriter objects that were previously
        ''' created for each of the different report types.
        ''' </summary>
        ''' <paramname=""></param>
        Private Shared Sub CloseReportStreams()
            If TpRunTsip.TsipReportHelper.OutputToFiles Then
                Call TpRunTsip.TpRunTsip.mTW_AGGINTCSV.Close()
                Call TpRunTsip.TpRunTsip.mTW_AGGINTREP.Close()
                Call TpRunTsip.TpRunTsip.mTW_CASEDET.Close()
                Call TpRunTsip.TpRunTsip.mTW_CASEOHL.Close()
                Call TpRunTsip.TpRunTsip.mTW_CASESUM.Close()
                Call TpRunTsip.TpRunTsip.mTW_EXEC.Close()
                Call TpRunTsip.TpRunTsip.mTW_EXPORT.Close()
                Call TpRunTsip.TpRunTsip.mTW_HILO.Close()
                Call TpRunTsip.TpRunTsip.mTW_ORBIT.Close()
                Call TpRunTsip.TpRunTsip.mTW_STATSUM.Close()
                Call TpRunTsip.TpRunTsip.mTW_STUDY.Close()
            End If
        End Sub

        ''' <summary>
        ''' This method deletes any of the repertoir of report files that were
        ''' written to disk that have no text content, i.e. file size is zero bytes. 
        ''' </summary>
        ''' <remarks>
        ''' A report file is created but remains empty during a TSIP run if a report
        ''' of that type is not explicitely requested by the MICS User. Thus, this method
        ''' 'cleans up' unwanted report types.
        ''' </remarks>
        ''' <paramname=""></param>
        Private Shared Sub DeleteUnwantedReportFiles()
            '...Log2.v("\n" + mReports);

            If Not TpRunTsip.TpRunTsip.mReports.AggIntCsvWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.AggIntCsvFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.AggIntRepWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.AggIntRepFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.CaseDetWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.CaseDetFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.CaseOhlWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.CaseOhlFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.CaseSumWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.CaseSumFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.ExecWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.ExecFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.ExportWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.ExportFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.HiloWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.HiloFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.OrbitWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.OrbitFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.StatSumWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.StatSumFilePath)
            End If

            If Not TpRunTsip.TpRunTsip.mReports.StudyWritten Then
                TpRunTsip.TpRunTsip.DeleteFile(TpRunTsip.TpRunTsip.mReports.StudyFilePath)
            End If

        End Sub

        ''' <summary>
        ''' This method deletes an individual report file that was
        ''' written to disk but has no text content, i.e. file size is zero bytes. 
        ''' </summary>
        ''' <remarks>
        ''' A report file is created but remains empty during a TSIP run if a report
        ''' of that type is not explicitely requested by the MICS User. Thus, this method
        ''' 'cleans up' unwanted report types.
        ''' </remarks>
        ''' <paramname="fileName"> - eponym.</param>
        Private Shared Sub DeleteEmptyReports(fileName As String)
            If TpRunTsip.TsipReportHelper.OutputToFiles Then
                If File.Exists(fileName) Then
                    Dim fileInfo As FileInfo = New FileInfo(fileName)
                    If fileInfo.Length = 0 Then
                        Try
                            File.Delete(fileName)
                        Catch e As Exception
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.DeleteFileIfExistsAndEmpty(): ERROR: File.Delete()")
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & e.Message)
                        End Try
                    End If
                End If
            End If

        End Sub

        ''' <summary>
        ''' This method checks whether the precribed file exists and,
        ''' if so, deletes it.
        ''' </summary>
        ''' <paramname="fileName"> - eponym.</param>
        Private Shared Sub DeleteFile(fileName As String)
            If File.Exists(fileName) Then
                Dim fileInfo As FileInfo = New FileInfo(fileName)

                Try
                    File.Delete(fileName)
                Catch e As Exception
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.DeleteFileIfExistsAndEmpty(): ERROR: File.Delete()")
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & e.Message)
                End Try

            End If
        End Sub

        ''' <summary>
        ''' This method deletes the '.ORBIT' file from the repertoir of reports
        ''' previously written to disk if the MICS User did not explicitely request
        ''' an '.ORBIT' report.
        ''' </summary>
        ''' <paramname=""></param>
        Private Shared Sub DeleteOrbitReportIfNotRequested()
            If Not TpRunTsip.TpRunTsip.mReports.Orbit Then
                If File.Exists(TpRunTsip.TpRunTsip.mReports.OrbitFilePath) Then
                    Try
                        File.Delete(TpRunTsip.TpRunTsip.mReports.OrbitFilePath)
                    Catch e As Exception
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpRunTsip.DeleteOrbitReportIfNotRequested(): ERROR: File.Delete()")
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & e.Message)
                    End Try
                End If
            End If
        End Sub

        ''' <summary>
        ''' Set up the various report types to be produced.  
        ''' </summary>
        ''' <paramname="dBaseName"> - name of DB.</param>
        ''' <paramname="tempName"> - PDF name.</param>
        ''' <paramname="currParm"> - ParmTableWN object providing paramater values and associated ODBC nullInds.</param>
        ''' <paramname="numCases"> - number of cases.</param>
        ''' <paramname="numteCases"> - number of Te cases.</param>
        ''' <paramname="numStats"> - number of statistics to be reported.</param>
        ''' <paramname="viewName"> - name of the Tt PDF file.</param>
        ''' <paramname="destname"> - directory to which the reports are to be written.</param>
        ''' <paramname="cUnique"> - common DB report table name tag.</param>
        ''' <paramname="cUniqueEnv"> - DB report table name tag for '.STATSUM' report.</param>
        ''' <paramname="isTS"> - eponym.</param>
        ''' <returns></returns>
        Private Shared Function ReportNew(dBaseName As String, tempName As String, currParm As TpRunTsip.TpRunTsip.ParmTableWN, numCases As Integer, numteCases As Integer, numStats As Integer, viewName As String, destname As String, cUnique As String, cUniqueEnv As String, isTS As Boolean) As Integer ' stat rep table 
            '...Log2.v("\nTpRunTsip.ReportNew(): Entry");

            Dim nRet As Integer
            Dim IsTSTSDet = False
            Dim IsTSESDet = False
            Dim IsESTSDet = False
            Dim IsES As Boolean
            Dim lIsTS As Boolean
            Dim nDist = 0

            lIsTS = isTS
            IsES = Not lIsTS

            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ' Set up the reports for the TS/TS TSIP reports
            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            If TpRunTsip.TpRunTsip.mReports.TsTsStn OrElse TpRunTsip.TpRunTsip.mReports.TsEsStn OrElse TpRunTsip.TpRunTsip.mReports.EsTsStn Then
                '	One of the station lists is requested.  They all go to the same output
                '		file (.STATSUM) -- 1149 - GJS - 2007.08 

                TpRunTsip.TpRunTsip.mReports.StatSumWritten = True

                If lIsTS AndAlso TpRunTsip.TpRunTsip.mReports.TsTsStn Then
                    TpRunTsip.Tstsrp2.TsTsRp2(TpRunTsip.TpRunTsip.mTW_STATSUM, cUnique)

                    Call TpRunTsip.TpRunTsip.mTW_STATSUM.Flush()
                End If


                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ' Set up the reports for the TS/ES proposed TSIP reports
                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                If IsES AndAlso (currParm.parmStruct.protype.Equals("T") AndAlso TpRunTsip.TpRunTsip.mReports.TsEsStn) OrElse currParm.parmStruct.protype.Equals("E") AndAlso TpRunTsip.TpRunTsip.mReports.EsTsStn Then
                    If numStats > 0 Then
                        nRet = TpRunTsip.Tsesprop.TsEsProp(TpRunTsip.TpRunTsip.mTW_STATSUM, cUnique)

                        Call TpRunTsip.TpRunTsip.mTW_STATSUM.Flush()
                    End If
                End If

                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ' Set up the reports for the TS/ES proposed TSIP reports
                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                If IsES AndAlso (currParm.parmStruct.protype.Equals("T") AndAlso TpRunTsip.TpRunTsip.mReports.TsEsStn) OrElse currParm.parmStruct.protype.Equals("E") AndAlso TpRunTsip.TpRunTsip.mReports.EsTsStn Then
                    If numStats > 0 Then
                        TpRunTsip.TpRunTsip.mTW_STATSUM.Write(Microsoft.VisualBasic.Constants.vbFormFeed)  '	Start on a new page.

                        nRet = TpRunTsip.Tsesenv.TsEsEnv(TpRunTsip.TpRunTsip.mTW_STATSUM, cUniqueEnv)

                        Call TpRunTsip.TpRunTsip.mTW_STATSUM.Flush()
                    End If
                End If

            End If

            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '		Set up the reports for the TSIP CASEDET reports
            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            IsTSTSDet = TpRunTsip.TpRunTsip.mReports.TsTsDet
            '	Set both if even one is present.
            IsESTSDet = TpRunTsip.TpRunTsip.mReports.EsTsCase OrElse TpRunTsip.TpRunTsip.mReports.TsEsCase
            IsTSESDet = IsESTSDet

            ErrMsg.SetDefaultOutputStream(TpRunTsip.TpRunTsip.mTW_CASEDET)

            If IsTSTSDet OrElse IsTSESDet OrElse IsESTSDet Then
                If lIsTS AndAlso IsTSTSDet Then
                    TpRunTsip.TpRunTsip.mReports.CaseDetWritten = True

                    If numCases > 0 Then
                        '...Log2.v("\nTpRunTsip.ReportNew(): CASEDET, before call to TsTsRp3");

                        If TpRunTsip.Tstsrp3.TsTsRp3(TpRunTsip.TpRunTsip.mTW_CASEDET, viewName, currParm.parmStruct, False) <> Constant.SUCCESS Then
                            ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_CASEDET, [Error].REPORT_ERROR)
                        End If
                    Else
                        '...Log2.v("\nTpRunTsip.ReportNew(): No TS-TS interference cases to report");
                        ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_CASEDET, [Error].GENERROR, "No TS-TS interference cases to report")
                    End If
                End If
            End If
            Call TpRunTsip.TpRunTsip.mTW_CASEDET.Flush()

            ' The Case detail ohloss only report.
            If lIsTS AndAlso TpRunTsip.TpRunTsip.mReports.TsTsOhl Then
                If IsTSTSDet Then
                    ' The following line was originally inside the the following if {} block
                    ' so when there are no interference cases the CASEOHL report would not persist.
                    ' Peter requested that a CASEOHL report always be generated even if there are 
                    ' no interference cases.
                    TpRunTsip.TpRunTsip.mReports.CaseOhlWritten = True

                    If numCases > 0 Then
                        If TpRunTsip.Tstsrp3.TsTsRp3(TpRunTsip.TpRunTsip.mTW_CASEOHL, viewName, currParm.parmStruct, True) <> Constant.SUCCESS Then
                            ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_CASEOHL, [Error].REPORT_ERROR)
                        End If
                    Else
                        ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_CASEOHL, [Error].GENERROR, "No TS-TS interference cases to report")
                    End If
                End If
            End If
            Call TpRunTsip.TpRunTsip.mTW_CASEOHL.Flush()

            If IsES AndAlso IsESTSDet Then
                TpRunTsip.TpRunTsip.mReports.CaseDetWritten = True

                nRet = TpRunTsip.Estsrp3.EsTsRp3(TpRunTsip.TpRunTsip.mTW_CASEDET, viewName, currParm.parmStruct)
            End If

            If IsES AndAlso IsTSESDet Then
                TpRunTsip.TpRunTsip.mReports.CaseDetWritten = True

                nRet = TpRunTsip.Tsesrp3.TsEsRp3(TpRunTsip.TpRunTsip.mTW_CASEDET, viewName, currParm.parmStruct)
            End If

            Call TpRunTsip.TpRunTsip.mTW_CASEDET.Flush()

            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '		Set up the TSIP CASESUM report.
            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            If lIsTS AndAlso TpRunTsip.TpRunTsip.mReports.TsTsSum Then
                TpRunTsip.TpRunTsip.mReports.CaseSumWritten = True

                If numCases > 0 Then
                    If TpRunTsip.Tstsrp4.TsTsRp4(TpRunTsip.TpRunTsip.mTW_CASESUM, viewName) <> Constant.SUCCESS Then
                        ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_CASESUM, [Error].REPORT_ERROR)
                    End If
                Else
                    ErrMsg.UtPrintMessage(TpRunTsip.TpRunTsip.mTW_CASESUM, [Error].GENERROR, "No interference cases to report")
                End If
            End If
            Call TpRunTsip.TpRunTsip.mTW_CASESUM.Flush()

            '	Produce the Aggregate Interference report for TS - 1181 - GJS - 2006.01
            '		Ensure that AggInt reps are not run for BAND analysis, as we don't have
            '		the values stored. - GJS - 1219 - 2006.06.23 
            If lIsTS AndAlso TpRunTsip.TpRunTsip.mReports.AggIntRep AndAlso Not currParm.parmStruct.analopt.Equals("BAND") Then
                Dim parms As ParmStrct
                Dim cCull As String

                'WAS_FPRINTF("\n\f");    //	Print a form feed 

                '	Retrieve the Culling Margin from the parmparm field.  It is
                '	entered as CM=<margin> 
                parms = GenUtil.ParmBreakOut(currParm.parmStruct.parmparm)
                If parms IsNot Nothing Then
                    cCull = GenUtil.ParmByName(parms, "CM")
                    If Not Equals(cCull, Nothing) AndAlso cCull.Length > 0 Then
                        ' The following conversion could throw an exception.
                        TpRunTsip.TpRunTsip.mdCull = Convert.ToDouble(cCull)
                    End If
                End If

                TpRunTsip.TpRunTsip.mReports.AggIntRepWritten = True

                TpRunTsip.AggInt.AggIntRep(TpRunTsip.TpRunTsip.mTW_AGGINTREP, viewName, currParm.parmStruct.spherecalc, TpRunTsip.TpRunTsip.mdCull)

            End If

            Call TpRunTsip.TpRunTsip.mTW_AGGINTREP.Flush()


            '	Produce the Aggregate Interference CSV for TS - 1181 - GJS - 2006.01
            '		Ensure that AggInt reps are not run for BAND analysis, as we don't have
            '		the values stored. - GJS - 1219 - 2006.06.23 
            If lIsTS AndAlso TpRunTsip.TpRunTsip.mReports.AggIntCsv AndAlso Not currParm.parmStruct.analopt.Equals("BAND") Then
                Dim parms As ParmStrct
                Dim cCull As String

                '	Retrieve the Culling Margin from the parmparm field.  It is
                '		entered as CM=<margin> 
                parms = GenUtil.ParmBreakOut(currParm.parmStruct.parmparm)
                If parms IsNot Nothing Then
                    cCull = GenUtil.ParmByName(parms, "CM")
                    If Not Equals(cCull, Nothing) AndAlso cCull.Length > 0 Then
                        ' The following conversion could throw an exception.
                        TpRunTsip.TpRunTsip.mdCull = Convert.ToDouble(cCull)
                    End If
                End If

                TpRunTsip.TpRunTsip.mReports.AggIntCsvWritten = True

                TpRunTsip.AggInt.AggIntCSV(TpRunTsip.TpRunTsip.mTW_AGGINTCSV, viewName, currParm.parmStruct.spherecalc, TpRunTsip.TpRunTsip.mdCull)

                TpRunTsip.TpRunTsip.mTW_STUDY.Write(Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf & "Aggregate Interference CSV report is in file: {0} ... " & Microsoft.VisualBasic.Constants.vbCrLf, TpRunTsip.TpRunTsip.mReports.AggIntCsvFilePath)
                Call TpRunTsip.TpRunTsip.mTW_STUDY.Flush()

                Call TpRunTsip.TpRunTsip.mTW_AGGINTCSV.Flush()
            End If

            ' ***************************************************************************
            '
            '		If this is a TS (and if it is asked for) do the hilo check.
            '
            ' ***************************************************************************

            If TpRunTsip.TpRunTsip.mReports.HiloCheck AndAlso lIsTS Then
                Dim dDistKm As Double
                Dim [cDate] As String
                Dim cTime As String
                Dim parms As ParmStrct
                Dim cCull As String

                '	Retrieve the hilo check distance from the parmparm field.  It is
                '		entered as HILO=<distance in seconds> 
                parms = GenUtil.ParmBreakOut(currParm.parmStruct.parmparm)

                If parms IsNot Nothing Then
                    cCull = GenUtil.ParmByName(parms, "HILO")
                    If Not Equals(cCull, Nothing) AndAlso cCull.Length > 0 Then
                        ' The following conversion could throw an exception.
                        nDist = Convert.ToInt32(cCull)
                    End If
                End If

                If nDist >= 0 Then
                    '	A negative nDist also means no report. 
                    '	Open the report file 

                    If nDist = 0 Then
                        nDist = 7
                    End If
                    dDistKm = nDist * 0.03087  '	Convert from seconds at the earth's surface to Km.

                    GenUtil.UtGetDateTime([cDate], cTime)

                    TpRunTsip.TpRunTsip.mReports.HiloWritten = True

                    TpRunTsip.TpRunTsip.mTW_HILO.Write("HiLoCheck Report for {0} for distance: {1:F2}Km ({2} Seconds)" & Microsoft.VisualBasic.Constants.vbCrLf & "At {3}, {4}" & Microsoft.VisualBasic.Constants.vbCrLf, currParm.parmStruct.proname, dDistKm, nDist, [cDate], cTime)

                    nRet = HiLoAnalysis2021.HiloCheckFunc(currParm.parmStruct.proname, False, dDistKm, TpRunTsip.TpRunTsip.mTW_HILO)

                    If nRet < 0 Then
                        TpRunTsip.TpRunTsip.mTW_HILO.Write("WARNING - There were hilo processing errors in the file." & Microsoft.VisualBasic.Constants.vbCrLf)
                    Else
                        TpRunTsip.TpRunTsip.mTW_HILO.Write("There were {0} hilo violations." & Microsoft.VisualBasic.Constants.vbCrLf, nRet)
                    End If
                End If
            End If

            '...Log2.v("\nTpRunTsip.ReportNew(): Exit, final.");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method manages the output of the TSIP execution report ('.EXEC').  
        ''' </summary>
        ''' <paramname="tw"> - TextWriter object for .EXEC report.</param>
        ''' <paramname="tempName"> - PDF name.</param>
        ''' <paramname="currParm"> - TpParm object providing parameter values.</param>
        ''' <paramname="startTime"> - eponym.</param>
        ''' <paramname="startDate"> - eponym.</param>
        ''' <paramname="endTime"> - eponym.</param>
        ''' <paramname="endDate"> - eponym.</param>
        ''' <paramname="isTS"> - eponym.</param>
        ''' <paramname="numStnGroups"> - eponym.</param>
        ''' <paramname="tsesNumStns"> - eponym.</param>
        ''' <paramname="estsNumStns"> - eponym.</param>
        ''' <paramname="numIntCases"> - eponym.</param>
        ''' <paramname="numTeIntCases"> - eponym.</param>
        ''' <paramname="tsoCalc"> - NOT USED.</param>
        ''' <paramname="destname"> - directory into which the .EXEC report is to be written.</param>
        Private Shared Sub TpExecRpt(tw As TextWriter, tempName As String, currParm As TpParm, startTime As String, startDate As String, endTime As String, endDate As String, isTS As Boolean, numStnGroups As Integer, tsesNumStns As Integer, estsNumStns As Integer, numIntCases As Integer, numTeIntCases As Integer, tsoCalc As String, destname As String)
            tw.Write("                    FREQUENCY COORDINATION SYSTEM ASSOCIATION {0,16}" & Microsoft.VisualBasic.Constants.vbCrLf, Info.BuildMetaData)
            If isTS = True Then
                tw.Write("                      MICS TSIP Ts-Ts Execution Information" & Microsoft.VisualBasic.Constants.vbCrLf)
            Else
                tw.Write("                  MICS TSIP Es-Ts / Ts-Es Execution Information" & Microsoft.VisualBasic.Constants.vbCrLf)
            End If
            tw.Write("   Inputs:" & Microsoft.VisualBasic.Constants.vbCrLf)
            tw.Write("             Project Code:         {0}" & Microsoft.VisualBasic.Constants.vbCrLf, Info.ProjectCode)
            tw.Write("             Parameter File Name:  {0}" & Microsoft.VisualBasic.Constants.vbCrLf, tempName)
            tw.Write("             TSIP Run Name:        {0}" & Microsoft.VisualBasic.Constants.vbCrLf, currParm.runname)
            tw.Write("   Outputs:" & Microsoft.VisualBasic.Constants.vbCrLf)

            tw.Write("             Interference Tables" & Microsoft.VisualBasic.Constants.vbCrLf)

            ' The original C/C++ code used the printf format "%.10s" that means
            ' write a string but truncate it if it is over 10 characters long.
            Dim tName As String
            If tempName.Length > 10 Then
                tName = tempName.Substring(0, 10)
            Else
                tName = tempName
            End If

            If isTS = True Then
                tw.Write("                 tt_{0}_{1}_parm Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
                tw.Write("                 tt_{0}_{1}_site Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
                tw.Write("                 tt_{0}_{1}_ante Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
                tw.Write("                 tt_{0}_{1}_chan Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
            Else
                tw.Write("                 te_{0}_{1}_parm Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
                tw.Write("                 te_{0}_{1}_site Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
                tw.Write("                 te_{0}_{1}_ante Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
                tw.Write("                 te_{0}_{1}_chan Table" & Microsoft.VisualBasic.Constants.vbCrLf, tName, currParm.runname)
            End If

            If Not destname.Equals(Microsoft.VisualBasic.Constants.vbNullChar) Then
                tw.Write("             Report files" & Microsoft.VisualBasic.Constants.vbCrLf)
                If TpRunTsip.TpRunTsip.mReports.Exec Then
                    tw.Write("                 {0}_{1}_{2}.EXEC (Execution Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If TpRunTsip.TpRunTsip.mReports.TtStudy OrElse TpRunTsip.TpRunTsip.mReports.TeStudy OrElse TpRunTsip.TpRunTsip.mReports.EtStudy Then
                    tw.Write("                 {0}_{1}_{2}.STUDY (TSIP Study Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If TpRunTsip.TpRunTsip.mReports.TsTsStn OrElse TpRunTsip.TpRunTsip.mReports.TsEsStn OrElse TpRunTsip.TpRunTsip.mReports.EsTsStn Then
                    tw.Write("                 {0}_{1}_{2}.STATSUM (Station Summary Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If TpRunTsip.TpRunTsip.mReports.TsTsDet OrElse TpRunTsip.TpRunTsip.mReports.TsEsCase OrElse TpRunTsip.TpRunTsip.mReports.EsTsCase Then
                    tw.Write("                 {0}_{1}_{2}.CASEDET (Case Detail Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If TpRunTsip.TpRunTsip.mReports.TsTsSum OrElse TpRunTsip.TpRunTsip.mReports.TsTsSum OrElse TpRunTsip.TpRunTsip.mReports.TsTsSum Then
                    tw.Write("                 {0}_{1}_{2}.CASESUM (Case Summary Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If TpRunTsip.TpRunTsip.mReports.AggIntRep Then
                    tw.Write("                 {0}_{1}_{2}.AGGINTREP (Aggregate Interference Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If TpRunTsip.TpRunTsip.mReports.AggIntCsv Then
                    tw.Write("                 {0}_{1}_{2}_AGGINT.CSV (Aggregate Interference CSV)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If

                If currParm.tsorbout.Equals("Y") Then
                    tw.Write("                 {0}_{1}_{2}.ORBIT (Orbit Report)" & Microsoft.VisualBasic.Constants.vbCrLf, destname, tempName, currParm.runname)
                End If
            End If

            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)

            ' TASK 421: Additional line to show user the Propogation Loss Model

            tw.Write("   Propagation Loss Calculation Model:  ")
            Select Case currParm.spherecalc
                Case "1"
                    tw.Write("TSIP CCIR-SJM" & Microsoft.VisualBasic.Constants.vbCrLf)
                Case "2"
                    tw.Write("Spherical Earth" & Microsoft.VisualBasic.Constants.vbCrLf)
                Case "3"
                    tw.Write("Free Space" & Microsoft.VisualBasic.Constants.vbCrLf)
                Case "4"
                    tw.Write("PCS" & Microsoft.VisualBasic.Constants.vbCrLf)
                Case "5"
                    tw.Write("Over Horizon Loss" & Microsoft.VisualBasic.Constants.vbCrLf)
            End Select


            ' TASK 466: Extra line to show Frequency Separation

            tw.Write("   Maximum Frequency Separation      :  {0,7:F1}" & Microsoft.VisualBasic.Constants.vbCrLf, currParm.fsep)
            tw.Write("   Coordination Distance             :  {0,7:F1}" & Microsoft.VisualBasic.Constants.vbCrLf, currParm.coordist)
            If isTS Then
                tw.Write("   Culling Margin used               :  {0,7:F1}" & Microsoft.VisualBasic.Constants.vbCrLf, TpRunTsip.TpRunTsip.mdCull)
            Else
                tw.Write("   Arc Step Value used               :  {0,7:F1}" & Microsoft.VisualBasic.Constants.vbCrLf, TpRunTsip.TpRunTsip.mdArcStep)
            End If


            If isTS = True Then
                tw.Write("   Number of Station Groups Passed to Analysis:  {0}" & Microsoft.VisualBasic.Constants.vbCrLf, numStnGroups)
            Else
                tw.Write("   Number of TS->ES Station Groups Passed to Analysis:  {0}" & Microsoft.VisualBasic.Constants.vbCrLf, tsesNumStns)
                tw.Write("   Number of ES->TS Station Groups Passed to Analysis:  {0}" & Microsoft.VisualBasic.Constants.vbCrLf, estsNumStns)
            End If

            If isTS = True Then
                tw.Write("   Number of Interference Cases Reported:        {0}" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf, numIntCases)
            Else
                tw.Write("   Number of TS->ES Interference Cases Reported      :  {0}" & Microsoft.VisualBasic.Constants.vbCrLf, numTeIntCases)
                tw.Write("   Number of ES->TS Interference Cases Reported      :  {0}" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf, numIntCases)
            End If
            tw.Write("   Start of TSIP Run                                End of TSIP Run" & Microsoft.VisualBasic.Constants.vbCrLf)
            tw.Write("    Date: {0,-12}                               Date: {1,-12}" & Microsoft.VisualBasic.Constants.vbCrLf, startDate, endDate)
            tw.Write("    Time: {0,-12}                               Time: {1,-12}" & Microsoft.VisualBasic.Constants.vbCrLf, startTime, endTime)

            '  Display caching information 
            If True Then
                '  Antenna Cache 
                Dim nCalls As Integer
                Dim nHits As Integer
                Dim nSize As Integer
                Dim nUsed As Integer

                tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "Cache info:-" & Microsoft.VisualBasic.Constants.vbCrLf & "Cache       Calls  Hits  Size  Used" & Microsoft.VisualBasic.Constants.vbCrLf)

                Suutils.GetAntCacheInfo(nCalls, nHits, nSize, nUsed)
                tw.Write("Antennas.: {0,6:D}{1,6:D}{2,6:D}{3,6:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nCalls, nHits, nSize, nUsed)

                Suutils.GetEqptCacheInfo(nCalls, nHits, nSize, nUsed)
                tw.Write("Equipment: {0,6:D}{1,6:D}{2,6:D}{3,6:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nCalls, nHits, nSize, nUsed)

                Suutils.GetCTXCacheInfo(nCalls, nHits, nSize, nUsed)
                tw.Write("CTX Curve: {0,6:D}{1,6:D}{2,6:D}{3,6:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nCalls, nHits, nSize, nUsed)

                Suutils.GetCTXxCacheInfo(nCalls, nHits, nSize, nUsed)
                tw.Write("CTX Xref.: {0,6:D}{1,6:D}{2,6:D}{3,6:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nCalls, nHits, nSize, nUsed)

                CtxUtil.GetAnalogCacheInfo(nCalls, nHits, nSize, nUsed)
                tw.Write("Analog...: {0,6:D}{1,6:D}{2,6:D}{3,6:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nCalls, nHits, nSize, nUsed)

                CtxUtil.GetDigitalCacheInfo(nCalls, nHits, nSize, nUsed)
                tw.Write("Digital..: {0,6:D}{1,6:D}{2,6:D}{3,6:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nCalls, nHits, nSize, nUsed)

                tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            End If

            tw.Flush()
        End Sub

        ''' <summary>
        ''' This method creates and writes the content of the TSIP EXPORT report to file; the
        ''' actual content creation is performed by executing FtPrint.exe or FePrint.exe in a
        ''' Windows command shell.
        ''' </summary>
        ''' <paramname="pdfName"> - the name of a DB PDF table set.</param>
        ''' <paramname="pdfType"> - the type of the PDF table set; "T" for TS or "E" for ES.</param>
        Private Shared Sub TpExportRpt(pdfName As String, pdfType As String)
            ' USAGE: FtPrint <dbname> <projectCode> [-o<outFilePath>] <printFlag> <pdfName>
            ' USAGE: FePrint <dbname> <projectCode>                               <pdfName>

            Dim programName = If(Equals(pdfType, "T"), "FtPrint", "FePrint")

            Dim programPath = Ssutil.GetBinPath(programName, Info.DbName)

            Dim clArgs = New String(3) {}
            clArgs(0) = Info.DbName
            clArgs(1) = Info.ProjectCode
            clArgs(2) = If(Equals(pdfType, "T"), "X", "")
            clArgs(3) = pdfName

            ' Run FtPrint or FePrint in a Windows  command shell with output to the shell's stdout.

            Dim stdout As String
            Dim stderr As String
            Dim exitCode As Integer
            Dim retVal As Integer

            retVal = WindowsShell.RunCommand(programPath, clArgs, stdout, stderr, exitCode)

            If retVal <> Constant.SUCCESS Then
                stdout = String.Format(Microsoft.VisualBasic.Constants.vbCrLf & "TpRunTsip.TpExportRpt(): ERROR: call to WindowsShell.RunCommand() failed for:" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbTab & "{0} {1} {2} {3} {4}" & Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbTab & "ExitCode = {5}" & Microsoft.VisualBasic.Constants.vbCrLf, programPath, clArgs(0), clArgs(1), clArgs(2), clArgs(3), exitCode)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & stdout)

                TpRunTsip.TpRunTsip.mTW_ERR.Write(stdout)
                Call TpRunTsip.TpRunTsip.mTW_ERR.Flush()
            End If

            ' Write the output of the FtPrint or FePrint to the assigned TextWriter.
            TpRunTsip.TpRunTsip.mTW_EXPORT.Write(stdout)
            Call TpRunTsip.TpRunTsip.mTW_EXPORT.Flush()
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class








    End Class ' class
End Namespace ' namespace

