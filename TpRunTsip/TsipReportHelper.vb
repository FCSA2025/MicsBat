Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Namespace TpRunTsip

    Public Class TsipReportHelper
        ' Static members.
        Private Shared mOutputToFiles As Boolean = False
        Private Shared mOutputToReportsTable As Boolean = False
        Private Shared mReportsTableAlreadyCreated As Boolean = False
        Private Shared mReportsTableName As String = ""

        Private Shared mListOfAllMD5 As List(Of String) = New List(Of String)()

        Public Shared Property OutputToFiles As Boolean
            Get
                Return TpRunTsip.TsipReportHelper.mOutputToFiles
            End Get
            Set(value As Boolean)
                TpRunTsip.TsipReportHelper.mOutputToFiles = value
            End Set
        End Property
        Public Shared Property OutputToTable As Boolean
            Get
                Return TpRunTsip.TsipReportHelper.mOutputToReportsTable
            End Get
            Set(value As Boolean)
                TpRunTsip.TsipReportHelper.mOutputToReportsTable = value
            End Set
        End Property

        '-----------------------------------------------------------------

        ' Requested file content flags.
        Private mExec As Boolean = False
        Private mTtStudy As Boolean = False
        Private mTeStudy As Boolean = False
        Private mEtStudy As Boolean = False
        Private mTsTsStn As Boolean = False
        Private mTsEsStn As Boolean = False
        Private mEsTsStn As Boolean = False
        Private mTsTsDet As Boolean = False
        Private mTsTsSum As Boolean = False
        Private mTsEsCase As Boolean = False
        Private mEsTsCase As Boolean = False
        Private mTsEsSum As Boolean = False
        Private mEsTsSum As Boolean = False
        Private mAggIntRep As Boolean = False
        Private mAggIntCsv As Boolean = False
        Private mTsTsOhl As Boolean = False
        Private mHiloCheck As Boolean = False
        Private mOrbit As Boolean = False
        Private mExport As Boolean = False

        '----------------------------------------------------------------------------

        ' Set to true when a report file is actually written to.
        Private mAggIntCsvWritten As Boolean = False
        Private mAggIntRepWritten As Boolean = False
        Private mCaseDetWritten As Boolean = False
        Private mCaseOhlWritten As Boolean = False
        Private mCaseSumWritten As Boolean = False
        Private mExecWritten As Boolean = False
        Private mHiloWritten As Boolean = False
        Private mOrbitWritten As Boolean = False
        Private mStatSumWritten As Boolean = False
        Private mStudyWritten As Boolean = False
        Private mExportWritten As Boolean = False

        Private Shared mErrWritten As Boolean = False

        '----------------------------------------------------------------------------

        ' Report file paths.
        Private mAggIntCsvFilePath As String
        Private mAggIntRepFilePath As String
        Private mCaseDetFilePath As String
        Private mCaseOhlFilePath As String
        Private mCaseSumFilePath As String
        Private mExecFilePath As String
        Private mHiloFilePath As String
        Private mOrbitFilePath As String
        Private mStatSumFilePath As String
        Private mStudyFilePath As String
        Private mExportFilePath As String

        Private Shared mErrFilePath As String

        '----------------------------------------------------------------------------

        ' MD5 checksum of normalized report content as hexadecimal strings.
        Private mAggIntCsvFileMD5 As String
        Private mAggIntRepFileMD5 As String
        Private mCaseDetFileMD5 As String
        Private mCaseOhlFileMD5 As String
        Private mCaseSumFileMD5 As String
        Private mExecFileMD5 As String
        Private mHiloFileMD5 As String
        Private mOrbitFileMD5 As String
        Private mStatSumFileMD5 As String
        Private mStudyFileMD5 As String
        Private mExportFileMD5 As String

        Private Shared mErrFileMD5 As String

        '----------------------------------------------------------------------------
        Public ReadOnly Property Exec As Boolean
            Get
                Return mExec
            End Get
        End Property
        Public ReadOnly Property TtStudy As Boolean
            Get
                Return mTtStudy
            End Get
        End Property
        Public ReadOnly Property TeStudy As Boolean
            Get
                Return mTeStudy
            End Get
        End Property
        Public ReadOnly Property EtStudy As Boolean
            Get
                Return mEtStudy
            End Get
        End Property
        Public ReadOnly Property TsTsStn As Boolean
            Get
                Return mTsTsStn
            End Get
        End Property
        Public ReadOnly Property TsEsStn As Boolean
            Get
                Return mTsEsStn
            End Get
        End Property
        Public ReadOnly Property EsTsStn As Boolean
            Get
                Return mEsTsStn
            End Get
        End Property
        Public ReadOnly Property TsTsDet As Boolean
            Get
                Return mTsTsDet
            End Get
        End Property
        Public ReadOnly Property TsTsSum As Boolean
            Get
                Return mTsTsSum
            End Get
        End Property
        Public ReadOnly Property TsEsCase As Boolean
            Get
                Return mTsEsCase
            End Get
        End Property
        Public ReadOnly Property EsTsCase As Boolean
            Get
                Return mEsTsCase
            End Get
        End Property
        Public ReadOnly Property TsEsSum As Boolean
            Get
                Return mTsEsSum
            End Get
        End Property
        Public ReadOnly Property EsTsSum As Boolean
            Get
                Return mEsTsSum
            End Get
        End Property
        Public ReadOnly Property AggIntRep As Boolean
            Get
                Return mAggIntRep
            End Get
        End Property
        Public ReadOnly Property AggIntCsv As Boolean
            Get
                Return mAggIntCsv
            End Get
        End Property
        Public ReadOnly Property TsTsOhl As Boolean
            Get
                Return mTsTsOhl
            End Get
        End Property
        Public ReadOnly Property HiloCheck As Boolean
            Get
                Return mHiloCheck
            End Get
        End Property
        Public ReadOnly Property Orbit As Boolean
            Get
                Return mOrbit
            End Get
        End Property
        Public ReadOnly Property Export As Boolean
            Get
                Return mExport
            End Get
        End Property

        '----------------------------------------------------------------------------
        Public Property AggIntCsvFilePath As String
            Get
                Return mAggIntCsvFilePath
            End Get
            Set(value As String)
                mAggIntCsvFilePath = value
            End Set
        End Property
        Public Property AggIntRepFilePath As String
            Get
                Return mAggIntRepFilePath
            End Get
            Set(value As String)
                mAggIntRepFilePath = value
            End Set
        End Property
        Public Property CaseDetFilePath As String
            Get
                Return mCaseDetFilePath
            End Get
            Set(value As String)
                mCaseDetFilePath = value
            End Set
        End Property
        Public Property CaseOhlFilePath As String
            Get
                Return mCaseOhlFilePath
            End Get
            Set(value As String)
                mCaseOhlFilePath = value
            End Set
        End Property
        Public Property CaseSumFilePath As String
            Get
                Return mCaseSumFilePath
            End Get
            Set(value As String)
                mCaseSumFilePath = value
            End Set
        End Property
        Public Property ExecFilePath As String
            Get
                Return mExecFilePath
            End Get
            Set(value As String)
                mExecFilePath = value
            End Set
        End Property
        Public Property HiloFilePath As String
            Get
                Return mHiloFilePath
            End Get
            Set(value As String)
                mHiloFilePath = value
            End Set
        End Property
        Public Property OrbitFilePath As String
            Get
                Return mOrbitFilePath
            End Get
            Set(value As String)
                mOrbitFilePath = value
            End Set
        End Property
        Public Property StatSumFilePath As String
            Get
                Return mStatSumFilePath
            End Get
            Set(value As String)
                mStatSumFilePath = value
            End Set
        End Property
        Public Property StudyFilePath As String
            Get
                Return mStudyFilePath
            End Get
            Set(value As String)
                mStudyFilePath = value
            End Set
        End Property
        Public Property ExportFilePath As String
            Get
                Return mExportFilePath
            End Get
            Set(value As String)
                mExportFilePath = value
            End Set
        End Property
        Public Shared Property ErrFilePath As String
            Get
                Return TpRunTsip.TsipReportHelper.mErrFilePath
            End Get
            Set(value As String)
                TpRunTsip.TsipReportHelper.mErrFilePath = value
            End Set
        End Property

        Public Property AggIntCsvFileMD5 As String
            Get
                Return mAggIntCsvFileMD5
            End Get
            Set(value As String)
                mAggIntCsvFileMD5 = value
            End Set
        End Property
        Public Property AggIntRepFileMD5 As String
            Get
                Return mAggIntRepFileMD5
            End Get
            Set(value As String)
                mAggIntRepFileMD5 = value
            End Set
        End Property
        Public Property CaseDetFileMD5 As String
            Get
                Return mCaseDetFileMD5
            End Get
            Set(value As String)
                mCaseDetFileMD5 = value
            End Set
        End Property
        Public Property CaseOhlFileMD5 As String
            Get
                Return mCaseOhlFileMD5
            End Get
            Set(value As String)
                mCaseOhlFileMD5 = value
            End Set
        End Property
        Public Property CaseSumFileMD5 As String
            Get
                Return mCaseSumFileMD5
            End Get
            Set(value As String)
                mCaseSumFileMD5 = value
            End Set
        End Property
        Public Property ExecFileMD5 As String
            Get
                Return mExecFileMD5
            End Get
            Set(value As String)
                mExecFileMD5 = value
            End Set
        End Property
        Public Property HiloFileMD5 As String
            Get
                Return mHiloFileMD5
            End Get
            Set(value As String)
                mHiloFileMD5 = value
            End Set
        End Property
        Public Property OrbitFileMD5 As String
            Get
                Return mOrbitFileMD5
            End Get
            Set(value As String)
                mOrbitFileMD5 = value
            End Set
        End Property
        Public Property StatSumFileMD5 As String
            Get
                Return mStatSumFileMD5
            End Get
            Set(value As String)
                mStatSumFileMD5 = value
            End Set
        End Property
        Public Property StudyFileMD5 As String
            Get
                Return mStudyFileMD5
            End Get
            Set(value As String)
                mStudyFileMD5 = value
            End Set
        End Property
        Public Property ExportFileMD5 As String
            Get
                Return mExportFileMD5
            End Get
            Set(value As String)
                mExportFileMD5 = value
            End Set
        End Property
        Public Shared Property ErrFileMD5 As String
            Get
                Return TpRunTsip.TsipReportHelper.mErrFileMD5
            End Get
            Set(value As String)
                TpRunTsip.TsipReportHelper.mErrFileMD5 = value
            End Set
        End Property

        Public Property AggIntCsvWritten As Boolean
            Get
                Return mAggIntCsvWritten
            End Get
            Set(value As Boolean)
                mAggIntCsvWritten = value
            End Set
        End Property
        Public Property AggIntRepWritten As Boolean
            Get
                Return mAggIntRepWritten
            End Get
            Set(value As Boolean)
                mAggIntRepWritten = value
            End Set
        End Property
        Public Property CaseDetWritten As Boolean
            Get
                Return mCaseDetWritten
            End Get
            Set(value As Boolean)
                mCaseDetWritten = value
            End Set
        End Property
        Public Property CaseOhlWritten As Boolean
            Get
                Return mCaseOhlWritten
            End Get
            Set(value As Boolean)
                mCaseOhlWritten = value
            End Set
        End Property
        Public Property CaseSumWritten As Boolean
            Get
                Return mCaseSumWritten
            End Get
            Set(value As Boolean)
                mCaseSumWritten = value
            End Set
        End Property
        Public Property ExecWritten As Boolean
            Get
                Return mExecWritten
            End Get
            Set(value As Boolean)
                mExecWritten = value
            End Set
        End Property
        Public Property HiloWritten As Boolean
            Get
                Return mHiloWritten
            End Get
            Set(value As Boolean)
                mHiloWritten = value
            End Set
        End Property
        Public Property OrbitWritten As Boolean
            Get
                Return mOrbitWritten
            End Get
            Set(value As Boolean)
                mOrbitWritten = value
            End Set
        End Property
        Public Property StatSumWritten As Boolean
            Get
                Return mStatSumWritten
            End Get
            Set(value As Boolean)
                mStatSumWritten = value
            End Set
        End Property
        Public Property StudyWritten As Boolean
            Get
                Return mStudyWritten
            End Get
            Set(value As Boolean)
                mStudyWritten = value
            End Set
        End Property
        Public Property ExportWritten As Boolean
            Get
                Return mExportWritten
            End Get
            Set(value As Boolean)
                mExportWritten = value
            End Set
        End Property
        Public Shared Property ErrWritten As Boolean
            Get
                Return TpRunTsip.TsipReportHelper.mErrWritten
            End Get
            Set(value As Boolean)
                TpRunTsip.TsipReportHelper.mErrWritten = value
            End Set
        End Property
        '----------------------------------------------------------------------------

        ' Bit-position versus report type.
        Public Const EXECField As Integer = 0
        Public Const TTSTUDYField As Integer = 1
        Public Const TESTUDYField As Integer = 2
        Public Const ETSTUDYField As Integer = 3
        Public Const TSTSSTNField As Integer = 4
        Public Const TSESSTNField As Integer = 5
        Public Const ESTSSTNField As Integer = 6
        Public Const TSTSDETField As Integer = 7
        Public Const TSTSSUMField As Integer = 8
        Public Const TSESCASEField As Integer = 9
        Public Const ESTSCASEField As Integer = 10
        Public Const TSESSUMField As Integer = 11
        Public Const ESTSSUMField As Integer = 12
        Public Const AGGINTREPField As Integer = 13
        Public Const AGGINTCSVField As Integer = 14
        Public Const TSTSOHLField As Integer = 15
        Public Const HILOCHECKField As Integer = 16
        Public Const EXPORTField As Integer = 17

        ' 32-bit integer value corresponding to just the 0th bit being set.
        Private Const BIT_ZERO As UInteger = &H80000000UI

        '---------------------------------------------------------------------------

        ''' <summary>
        ''' This constructor method sets all report type bits to zero (i.e. 'not requested').
        ''' </summary>
        ''' <paramname=""></param>
        ''' <returns></returns>
        Public Sub New()
            SetAll(False)
        End Sub

        ''' <summary>
        ''' This constructor method sets the private class bit flags using a prescribed
        ''' integer 'mask'; it also tests whether tsorbout equals "Y" and, if so, 
        ''' sets mOrbit to true..
        ''' </summary>
        ''' <paramname=""></param>
        ''' <returns></returns>
        Public Sub New(bitMask As Integer, tsorbout As String)
            SetUsingBitMask(bitMask)

            If tsorbout.Equals("Y") Then mOrbit = True
        End Sub

        ''' <summary>
        ''' This method sets all the internal (private) boolean flags
        ''' to the prescribed true or false value. (true = report is requested).
        ''' </summary>
        ''' <paramname="setting"></param>
        Public Sub SetAll(setting As Boolean)
            mExec = setting
            mTtStudy = setting
            mTeStudy = setting
            mEtStudy = setting
            mTsTsStn = setting
            mTsEsStn = setting
            mEsTsStn = setting
            mTsTsDet = setting
            mTsTsSum = setting
            mTsEsCase = setting
            mEsTsCase = setting
            mTsEsSum = setting
            mEsTsSum = setting
            mAggIntRep = setting
            mAggIntCsv = setting
            mTsTsOhl = setting
            mHiloCheck = setting
            mOrbit = setting
            mExport = setting
        End Sub

        ''' <summary>
        ''' This method sets all the internal (private) boolean
        ''' 'report requested' flags i.a.w. a prescribed bitmap.
        ''' </summary>
        ''' <paramname="bitMask"></param>
        Public Sub SetUsingBitMask(bitMask As Integer)
            mExec = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.EXECField)
            mTtStudy = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TTSTUDYField)
            mTeStudy = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TESTUDYField)
            mEtStudy = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.ETSTUDYField)
            mTsTsStn = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSTSSTNField)
            mTsEsStn = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSESSTNField)
            mEsTsStn = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.ESTSSTNField)
            mTsTsDet = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSTSDETField)
            mTsTsSum = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSTSSUMField)
            mTsEsCase = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSESCASEField)
            mEsTsCase = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.ESTSCASEField)
            mTsEsSum = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSESSUMField)
            mEsTsSum = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.ESTSSUMField)
            mAggIntRep = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.AGGINTREPField)
            mAggIntCsv = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.AGGINTCSVField)
            mTsTsOhl = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.TSTSOHLField)
            mHiloCheck = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.HILOCHECKField)
            mExport = TpRunTsip.TsipReportHelper.TestBit(bitMask, TpRunTsip.TsipReportHelper.EXPORTField)
        End Sub

        ''' <summary>
        ''' This method returns true if a prescribed bit position in a
        ''' bitmap is set to 1.
        ''' </summary>
        ''' <paramname="bitMap"></param>
        ''' <paramname="pseudoBitNum"></param>
        ''' <returns></returns>
        Private Shared Function TestBit(bitMap As Integer, pseudoBitNum As Integer) As Boolean
            Dim mask As UInteger

            ' Check for valid bitnum 
            If pseudoBitNum < 0 OrElse pseudoBitNum >= 1 << Constant.NUM_SHIFT Then
                Return False
            End If

            mask = TpRunTsip.TsipReportHelper.BIT_ZERO >> pseudoBitNum
            ' Test the bit.
            If (bitMap And mask) > 0 Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' The TSIP EXPORT report cannot be specifically requested by the user via WebMICS; instead
        ''' the decision to create a TSIP EXPORT report is made in the TpRunTsip Main() code by calling
        ''' this method.
        ''' </summary>
        ''' <paramname="isRequested"> - true to request the creation of the TSIP EXPORT report.</param>
        Public Sub RequestExportReport(isRequested As Boolean)
            If isRequested Then
                mExport = True
            Else
                mExport = False
            End If
        End Sub

        ''' <summary>
        ''' This method provides an annotated, formatted, multi-line
        ''' string providing the current values of all private member
        ''' boolean flags.
        ''' </summary>
        ''' <paramname=""></param>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mExec = " & mExec.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTtStudy   = " & mTtStudy.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTeStudy   = " & mTeStudy.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mEtStudy   = " & mEtStudy.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsTsStn   = " & mTsTsStn.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsEsStn   = " & mTsEsStn.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mEsTsStn   = " & mEsTsStn.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsTsDet   = " & mTsTsDet.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsTsSum   = " & mTsTsSum.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsEsCase  = " & mTsEsCase.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mEsTsCase  = " & mEsTsCase.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsEsSum   = " & mTsEsSum.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mEsTsSum   = " & mEsTsSum.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mAggIntRep = " & mAggIntRep.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mAggIntCsv = " & mAggIntCsv.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mTsTsOhl = " & mTsTsOhl.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mHiloCheck = " & mHiloCheck.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mOrbit     = " & mOrbit.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mExport    = " & mExport.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mAggIntCsvRequired = " & mAggIntCsvWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mAggIntRepRequired = " & mAggIntRepWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mCaseDetRequired   = " & mCaseDetWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mCaseOhlRequired   = " & mCaseOhlWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mCaseSumRequired   = " & mCaseSumWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mExecRequired      = " & mExecWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mHiloRequired      = " & mHiloWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mOrbitRequired     = " & mOrbitWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mStatSumRequired   = " & mStatSumWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mStudyRequired     = " & mStudyWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mExportRequired    = " & mExportWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mErrRequired       = " & TpRunTsip.TsipReportHelper.mErrWritten.ToString())
            sb.Append(Microsoft.VisualBasic.Constants.vbLf)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mAggIntCsvFilePath = " & mAggIntCsvFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mAggIntRepFilePath = " & mAggIntRepFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mCaseDetFilePath   = " & mCaseDetFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mCaseOhlFilePath   = " & mCaseOhlFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mCaseSumFilePath   = " & mCaseSumFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mExecFilePath      = " & mExecFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mHiloFilePath      = " & mHiloFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mOrbitFilePath     = " & mOrbitFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mStatSumFilePath   = " & mStatSumFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mStudyFilePath     = " & mStudyFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf & "mExportFilePath    = " & mExportFilePath)
            sb.Append(Microsoft.VisualBasic.Constants.vbLf)

            Return sb.ToString()
        End Function

        Private Shared Sub CreateTsipReportsTable()
            Console.Write(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TsipReportHelper.CreateTsipReportsTable(): Entry")
            Console.Write(Microsoft.VisualBasic.Constants.vbLf & "CreateTsipReportsTable Info.GlobalSchema: {0} Info.PdfName: {1}" & Microsoft.VisualBasic.Constants.vbLf, Info.GlobalSchema, Info.PdfName)

            If Not TpRunTsip.TsipReportHelper.mOutputToReportsTable Then Return

            If Not TpRunTsip.TsipReportHelper.mReportsTableAlreadyCreated Then
                TpRunTsip.TsipReportHelper.mReportsTableName = String.Format("{0}.{1}_tsip_reports", Info.GlobalSchema, Info.PdfName)
                'mReportsTableName = String.Format("{0}.{1}_tsip_reports", "venn", "bill1");
                DynTsipReports.DropTableIfExists(TpRunTsip.TsipReportHelper.mReportsTableName)
                DynTsipReports.CreateTable(TpRunTsip.TsipReportHelper.mReportsTableName)

                TpRunTsip.TsipReportHelper.mReportsTableAlreadyCreated = True
            End If
        End Sub

        Public Sub WritePerRunReportsToDbTable()
            If Not TpRunTsip.TsipReportHelper.mOutputToReportsTable Then Return

            Call TpRunTsip.TsipReportHelper.CreateTsipReportsTable()

            If mAggIntCsvWritten Then
                CalculateMD5ofReport(mAggIntCsvFilePath)
            End If

            If mAggIntRepWritten Then
                CalculateMD5ofReport(mAggIntRepFilePath)
            End If

            If mCaseDetWritten Then
                CalculateMD5ofReport(mCaseDetFilePath)
            End If

            If mCaseOhlWritten Then
                CalculateMD5ofReport(mCaseOhlFilePath)
            End If

            If mCaseSumWritten Then
                CalculateMD5ofReport(mCaseSumFilePath)
            End If

            If mExecWritten Then
                CalculateMD5ofReport(mExecFilePath)
            End If

            If mExportWritten Then
                CalculateMD5ofReport(mExportFilePath)
            End If

            If mHiloWritten Then
                CalculateMD5ofReport(mHiloFilePath)
            End If

            If mOrbitWritten Then
                CalculateMD5ofReport(mOrbitFilePath)
            End If

            If mStatSumWritten Then
                CalculateMD5ofReport(mStatSumFilePath)
            End If

            If mStudyWritten Then
                CalculateMD5ofReport(mStudyFilePath)
            End If

        End Sub

        Public Sub WriteRunReportToDbTable(reportFilePath As String)
            If Not TpRunTsip.TsipReportHelper.mOutputToReportsTable Then Return

            Call TpRunTsip.TsipReportHelper.CreateTsipReportsTable()

            CalculateMD5ofReport(reportFilePath)
        End Sub

        Public Function CalculateMD5ofReport(reportFilePath As String) As String
            Dim md5 = ""

            Dim sb As StringBuilder = New StringBuilder()

            Try
                Dim lines = File.ReadAllLines(reportFilePath)
                Dim normalizedLines As List(Of String) = New List(Of String)()

                For Each line In lines
                    Dim normalizedLine As String = TpRunTsip.TsipReportHelper.NormalizedLine(line)
                    sb.AppendLine(normalizedLine)
                    normalizedLines.Add(normalizedLine)
                Next

                md5 = TpRunTsip.TsipReportHelper.CalculateMD5ofString(sb.ToString())

                TpRunTsip.TsipReportHelper.mListOfAllMD5.Add(md5)

                Console.Write(sb.ToString())
                Console.Write("MD5 = {0}" & Microsoft.VisualBasic.Constants.vbLf, md5)

                Dim runID = Info.RunID

                Dim reportType = Path.GetExtension(reportFilePath).Replace(".", "")
                If reportType.ToLower().Equals("csv") Then reportType = "AGGINT.csv"
                If reportType.ToLower().Equals("err") Then runID = ""
                Console.Write("reportType = {0}" & Microsoft.VisualBasic.Constants.vbLf, reportType)

                Dim tsipReports As TsipReports = New TsipReports()

                tsipReports.date = Info.Date
                tsipReports.time = Info.Time
                tsipReports.paramFile = Info.PdfName
                tsipReports.runID = runID
                tsipReports.reportType = reportType
                tsipReports.lineNum = 0
                tsipReports.line = md5

                Dim nullInds = NullHelper.CreateArrayOfNullInd(TsipReports.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL)

                DynTsipReports.Insert(TpRunTsip.TsipReportHelper.mReportsTableName, tsipReports, nullInds)

                Dim lineNum = 1
                For Each normalizedLine In normalizedLines
                    tsipReports = New TsipReports()

                    tsipReports.date = Info.Date
                    tsipReports.time = Info.Time
                    tsipReports.paramFile = Info.PdfName
                    tsipReports.runID = runID
                    tsipReports.reportType = reportType
                    tsipReports.lineNum = Math.Min(Threading.Interlocked.Increment(lineNum), lineNum - 1)
                    tsipReports.line = normalizedLine

                    DynTsipReports.Insert(TpRunTsip.TsipReportHelper.mReportsTableName, tsipReports, nullInds)
                Next
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TsipReportHelper.CalculateNormalizedMD5(): ERROR: exception: {0}" & Microsoft.VisualBasic.Constants.vbLf & "{1}" & Microsoft.VisualBasic.Constants.vbLf & "reportFilePath = {2}", e.Message, reportFilePath, e.StackTrace)
            End Try

            Return md5
        End Function

        Public Shared Function NormalizedLine(line As String) As String
            Dim lNormalizedLine = line

            ' Single quotation characters in the report wreak havoc with SQL queries.
            If line.Contains("'") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "'", "`")
            End If

            ' STUDY report.
            If line.Contains("Interference Study Summary") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d:\d\d", "XX:XX")
            End If
            If line.Contains("Project Code") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \w+", ": XXX")
            End If
            If line.Contains("PDF File Name") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \w+", ": XXX")
            End If
            If line.Contains("Environment Name") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \w+", ": XXX")
            End If
            If line.Contains("Study Date") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \d\d\d\d\.\d\d\.\d\d", ": XXXX.XX.XX")
            End If
            If line.Contains("Study Time") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \d\d:\d\d", ": XX:XX")
            End If
            If line.Contains("CPU Time") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \d\d:\d\d:\d\d\.\d\d\d", ": XX:XX:XX.XXX")
            End If
            If line.Contains("Elapsed Time") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \d\d:\d\d:\d\d", ": XX:XX:XX")
            End If
            If line.Contains("Aggregate Interference CSV report is in file") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": .*", ": X.AGGINT.csv ...")
            End If

            ' STATSUM report.
            If line.Contains("FREQUENCY COORDINATION SYSTEM ASSOCIATION") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, ": \d\d\d\d\.\d\d\.\d\d", ": XXXX.XX.XX")
            End If
            If line.Contains("Project Code") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "\[\w+\]", "[XXX]")
            End If

            ' HILO report.
            If line.Contains("HiLoCheck Report for") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "HiLoCheck Report for \w+", "HiLoCheck Report for XXX")
            End If
            If line.StartsWith("At ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d\d\d\.\d\d\.\d\d, \d\d:\d\d", "XXXX.XX.XX, XX:XX")
            End If


            ' EXEC report.
            If line.Contains("FREQUENCY COORDINATION SYSTEM ASSOCIATION") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d\d\d\d\d-\d\d\d\d\/\d\d-\w", "XXXXXX-XXXX/XX-X")
            End If
            If line.Contains("Project Code:         ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Project Code:         (\w+)", "Project Code:         XXX")
            End If
            If line.Contains("Parameter File Name:  ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Parameter File Name:  (\w+)", "Parameter File Name:  XXX")
            End If
            If line.Contains("_parm Table") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)_parm Table", "XXX_parm Table")
            End If
            If line.Contains("_site Table") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)_site Table", "XXX_site Table")
            End If
            If line.Contains("_ante Table") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)_ante Table", "XXX_ante Table")
            End If
            If line.Contains("_chan Table") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)_chan Table", "XXX_chan Table")
            End If
            If line.Contains(".EXEC (Execution Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.EXEC \(Execution Report\)", "XXX.EXEC (Execution Report)")
            End If
            If line.Contains(".STUDY (TSIP Study Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.STUDY \(TSIP Study Report\)", "XXX.STUDY (TSIP Study Report)")
            End If
            If line.Contains(".STATSUM (Station Summary Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.STATSUM \(Station Summary Report\)", "XXX.STATSUM (Station Summary Report)")
            End If
            If line.Contains(".CASEDET (Case Detail Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.CASEDET \(Case Detail Report\)", "XXX.CASEDET (Case Detail Report)")
            End If
            If line.Contains(".CASESUM (Case Summary Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.CASESUM \(Case Summary Report\)", "XXX.CASESUM (Case Summary Report)")
            End If
            If line.Contains(".AGGINTREP (Aggregate Interference Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.AGGINTREP \(Aggregate Interference Report\)", "XXX.AGGINTREP (Aggregate Interference Report)")
            End If
            If line.Contains("AGGINT.CSV (Aggregate Interference CSV)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)AGGINT\.CSV \(Aggregate Interference CSV\)", "XXX_AGGINT.CSV (Aggregate Interference CSV)")
            End If
            If line.Contains(".EXEC (Execution Report)") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "(\w+)\.EXEC \(Execution Report\)", "XXX.EXEC (Execution Report)")
            End If
            If line.Contains("Date: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Date: \d\d\d\d\.\d\d\.\d\d", "Date: XXXX.XX.XX")
            End If
            If line.Contains("Time: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Time: \d\d:\d\d", "Time: XX:XX")
            End If
            If line.Contains("Antennas.:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Antennas\.:.+", "Antennas.:")
            End If
            If line.Contains("Equipment:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Equipment:.+", "Equipment:")
            End If
            If line.Contains("CTX Curve:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "CTX Curve:.*", "CTX Curve:")
            End If
            If line.Contains("CTX Xref.:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "CTX Xref\.:.*", "CTX Xref.:")
            End If
            If line.Contains("Analog...:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Analog\.\.\.:.*", "Analog...:")
            End If
            If line.Contains("Digital..:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Digital\.\.:.*", "Digital..:")
            End If

            ' CASESUM report.
            If line.Contains("Date:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Date:\d\d\d\d\.\d\d\.\d\d", "Date:XXXX.XX.XX")
            End If

            ' CASEDET and CASOHL reports.
            If line.Contains("Time:      ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Time:      \d\d:\d\d", "Time:      XX:XX")
            End If
            If line.Contains("Time:     ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Time:     \d\d:\d\d", "Time:     XX:XX")
            End If

            ' AGGINTREP report.
            If line.Contains(", Culling Margin:") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "for \w+, Culling Margin:", "for XXX, Culling Margin:")
            End If

            ' AGGINT.csv report.

            ' ERR report.
            If line.Contains("* TSIP PARAMETER FILE NAME: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "\* TSIP PARAMETER FILE NAME: \w+", "* TSIP PARAMETER FILE NAME: XXX")
            End If
            If line.Contains("Error Report for Run Name") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "TSIP build \d\d\d\d\d\d-\d\d\d\d\/\d\d-\w Error Report for Run Name `.+`, at \d\d\d\d\.\d\d\.\d\d \d\d:\d\d", "TSIP build XXXXXX-XXXX/XX-X Error Report for Run Name `XXX`, at XXXX.XX.XX XX:XX")
            End If
            If line.Contains("Process ID ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Process ID \d+", "Process ID XXX")
            End If
            If line.Contains("Proposed Name...........: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Proposed Name\.{11}: \w+", "Proposed Name...........: XXX")
            End If
            If line.Contains("Date....................: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Date\.{20}: \d\d\d\d\.\d\d\.\d\d", "Date....................: XXXX.XX.XX")
            End If
            If line.Contains("Time....................: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "Time\.{20}: \d\d:\d\d", "Time....................: XX:XX")
            End If

            ' TS_EXPORT report.
            If line.StartsWith("* TS-PDF: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "TS-PDF: \w+", "TS-PDF: XXX")

                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d\d\d\d\d-\d\d\d\d\/\d\d-\w", "XXXXXX-XXXX/XX-X")

                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d\d\d\.\d\d\.\d\d \d\d:\d\d:\d\d", "XXXX.XX.XX XX:XX:XX")
            End If

            ' ES_EXPORT report.
            If line.Contains("ES-PDF NAME: ") Then
                lNormalizedLine = Regex.Replace(lNormalizedLine, "ES-PDF NAME: \w+", "ES-PDF NAME: XXX")

                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d\d\d\d\d-\d\d\d\d\/\d\d-\w", "XXXXXX-XXXX/XX-X")

                lNormalizedLine = Regex.Replace(lNormalizedLine, "\d\d\d\d\.\d\d\.\d\d \d\d:\d\d:\d\d", "XXXX.XX.XX XX:XX:XX")
            End If



            Return lNormalizedLine
        End Function

        Public Function InsertFinalMD5allRunsandReports() As String
            Dim sb As StringBuilder = New StringBuilder()

            For Each md5 As String In TpRunTsip.TsipReportHelper.mListOfAllMD5
                sb.Append(md5)
            Next

            Dim md5AllRunsandReports As String = TpRunTsip.TsipReportHelper.CalculateMD5ofString(sb.ToString())

            Try
                Console.Write("md5AllRunsandReports = {0}" & Microsoft.VisualBasic.Constants.vbLf, md5AllRunsandReports)

                Dim tsipReports As TsipReports = New TsipReports()

                tsipReports.date = Info.Date
                tsipReports.time = Info.Time
                tsipReports.paramFile = Info.PdfName
                tsipReports.runID = ""
                tsipReports.reportType = ""
                tsipReports.lineNum = 0
                tsipReports.line = md5AllRunsandReports

                Dim nullInds = NullHelper.CreateArrayOfNullInd(TsipReports.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL)

                DynTsipReports.Insert(TpRunTsip.TsipReportHelper.mReportsTableName, tsipReports, nullInds)
            Catch e As Exception
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TsipReportHelper.InsertFinalMD5allRunsandReports(): ERROR: exception: {0}" & Microsoft.VisualBasic.Constants.vbLf & "{1}", e.Message, e.StackTrace)
            End Try

            Return md5AllRunsandReports
        End Function

        ''' <summary>
        ''' This method returns the MD5 checksum of the concatanation of all current field values of
        ''' all of the records used in this application as a 32-digit hexadecimal string; this
        ''' is an expedient way of determining if any field values have changed compared to a
        ''' previous time.
        ''' </summary>
        ''' <returns></returns>
        Private Shared Function CalculateMD5ofString(content As String) As String
            Dim checkSum = ""

            Using md5 As Cryptography.MD5 = Cryptography.MD5.Create()
                Dim inputBytes = Encoding.ASCII.GetBytes(content)
                Dim hashBytes = md5.ComputeHash(inputBytes)

                checkSum = TpRunTsip.TsipReportHelper.BytesToHexString(hashBytes)
            End Using

            Return checkSum
        End Function

        Public Shared Function BytesToHexString(arrInput As Byte()) As String
            Dim i As Integer
            Dim sOutput As StringBuilder = New StringBuilder(arrInput.Length)
            For i = 0 To arrInput.Length - 1
                sOutput.Append(arrInput(i).ToString("X2"))
            Next

            Return sOutput.ToString()
        End Function

    End Class
End Namespace
