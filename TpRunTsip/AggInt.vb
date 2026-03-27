Imports _DataStructures
Imports System.IO
Imports System.Math
Imports SQLLEN = System.Int64
Imports SQLHANDLE = System.IntPtr
Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods that support the production of the
    ''' Aggregate Interference Report (.AGGINT).
    ''' </summary>
    Public Class AggInt
        ''' <summary>
        ''' This method manages the production of the Aggregate Interference Report (.AGGINT).  
        ''' </summary>
        ''' <paramname="tw"> - TextWriter object that the report is written to.</param>
        ''' <paramname="viewName"> - the name of the DB table that is the source for the data.</param>
        ''' <paramname="cSphereCalc"> - the type of the loss calculation.</param>
        ''' <paramname="dCull"> - the culling margin (dB).</param>
        Public Shared Sub AggIntRep(tw As TextWriter, viewName As String, cSphereCalc As String, dCull As Double)                  ' 	The output file handle		
            ' 	The name of the table 		
            ' 	The loss calculation 			
            ' 	Culling Margin						
            '...Log2.v("\nAggInt.AggIntRep(): Entry");

            tw.Write("                  FREQUENCY CO-ORDINATION SYSTEM ASSOCIATION")
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "                       Aggregate Interference Report")
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "                       for {0}, Culling Margin: {1,6:F1}", viewName, dCull)
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "Victim    From        Ant  Frequency Rx Pwr C   AggInt   RqCo Margin")
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)

            TpRunTsip.AggInt.AggIntRecs(tw, viewName, dCull, cSphereCalc, 1)  ' The 1 is for a report 

            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)
            tw.Flush()

            '...Log2.v("\nAggInt.AggIntRep(): Exit");
            Return
        End Sub

        ''' <summary>
        ''' This method tests whether a record survives the 'cull', or not. 
        ''' </summary>
        ''' <paramname="tRec"> - TtChan object to be tested.</param>
        ''' <paramname="cSphereCalc"> - the type of loss calculation to be used.</param>
        ''' <paramname="dCull"> - the margin to be used for culling (dB).</param>
        ''' <returns> - true or false. </returns>
        Public Shared Function PassCull(tRec As TtChan, cSphereCalc As String, dCull As Double) As Boolean
            Dim result As Boolean

            If tRec.resti > dCull Then
                ' 	The LOS results fail the cull.  Check to OHloss if necessary 
                If cSphereCalc.Equals("5") Then
                    ' 	Ohloss calculations.  
                    If tRec.ohresult <= 0 OrElse tRec.ohresult >= 100 Then
                        result = False
                    Else
                        ' 	It is an ohloss condition.  
                        If tRec.resti80 > dCull Then
                            ' 	Failed the cull, try the 99. We alter the culling criteria  
                            If tRec.calctype.Equals("-I") OrElse tRec.calctype.Equals("I") Then
                                ' It is -I calculations 
                                If tRec.resti99 > dCull + 10.0 Then
                                    ' -I okay  
                                    result = False
                                Else
                                    result = True
                                End If
                            Else
                                ' C/I 
                                If tRec.resti99 > dCull - 10.0 Then
                                    ' C/I okay  
                                    result = False
                                Else
                                    result = True
                                End If
                            End If
                        Else
                            result = True
                        End If
                    End If
                Else
                    ' 	Spherical earth, and fails the cull 
                    result = False
                End If
            Else
                result = True        ' 	Passes the cull.  We want it.  
            End If

            Return result
        End Function

        ''' <summary>
        ''' This method produces the Aggregate Interference (.AGGINT) reports.
        ''' </summary>
        ''' <paramname="tw"> - TextWriter object assigned to the report.</param>
        ''' <paramname="tLine"> - AggIntLine object providing the data for the report.</param>
        ''' <paramname="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        Public Shared Sub AgIntOutRec(tw As TextWriter, tLine As AggIntLine, nRepType As Integer)
            If tLine Is Nothing Then
                Return
            End If

            ' 	Calculate the display values of the aggregate interference 
            Dim dAIN = 0.0
            Dim dInt = 0.0
            Dim dAIN80 = 0.0
            Dim dAIN99 = 0.0
            Dim dInt80 = 0.0
            Dim dInt99 = 0.0
            Dim dMargin = 0.0
            Dim dMargin80 = 0.0
            Dim dMargin99 = 0.0

            If tLine.dAin > 0.0 Then
                dAIN = 10.0 * Log10(tLine.dAin)
            End If
            dInt = dAIN

            If tLine.calctype.Equals("C/I") OrElse tLine.calctype.Equals("C") Then
                dInt = tLine.vicpwrrx - dAIN
                dMargin = dInt - tLine.reqdcalc
            Else
                ' 	The margin calculation depends on -I or C/I. 
                dMargin = tLine.reqdcalc - dInt
            End If

            ' 	For ohloss, we have some extra lines 
            If tLine.cSphereCalc.Equals("5") Then
                If tLine.dAin80 > 0.0 Then
                    dAIN80 = 10.0 * Log10(tLine.dAin80)
                End If
                If tLine.dAin99 > 0.0 Then
                    dAIN99 = 10.0 * Log10(tLine.dAin99)
                End If
                dInt80 = dAIN80
                dInt99 = dAIN99

                If tLine.calctype.Equals("C/I") OrElse tLine.calctype.Equals("C") Then
                    If tLine.dAin80 > 0.0 Then
                        dInt80 = tLine.vicpwrrx - dAIN80
                        dMargin80 = dInt80 - tLine.reqdcalc
                    End If
                    If tLine.dAin99 > 0.0 Then
                        dInt99 = tLine.vicpwrrx - dAIN99
                        dMargin99 = dInt99 - (tLine.reqdcalc - 10.0)  ' For C/I subtract 10
                    End If
                Else
                    If tLine.dAin80 > 0.0 Then
                        dMargin80 = tLine.reqdcalc - dInt80
                    End If
                    If tLine.dAin99 > 0.0 Then
                        dMargin99 = tLine.reqdcalc + 10.0 - dInt99   ' for -I add 10  
                    End If
                End If
            End If

            Select Case nRepType
                Case 1 ' 	Report 
                    tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "{0,-10}{1,-10}{2,5:D}{3,11:F0} {4,6:F1} {5,-3}", tLine.viccall1, tLine.viccall2, tLine.vicanum, tLine.vicfreqrx, tLine.vicpwrrx, tLine.calctype)
                    If tLine.dAin > 0.0 Then
                        ' 	There were some cases  
                        tw.Write("{0,7:F1}{1,7:F1}{2,7:F1}", dInt, tLine.reqdcalc, dMargin)
                    Else
                        tw.Write("         - No Cases -")
                    End If

                    If tLine.cSphereCalc.Equals("5") Then
                        If tLine.dAin80 > 0.0 Then
                            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "{0,47}{1,7:F1}       {2,7:F1}", "80%", dInt80, dMargin80)
                        Else
                            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "{0,47}         - No Cases -", "80%")
                        End If
                        If tLine.dAin99 > 0.0 Then
                            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "{0,47}{1,7:F1}       {2,7:F1}", "99%", dInt99, dMargin99)
                        Else
                            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "{0,47}         - No Cases -", "99%")
                        End If
                    End If

                Case 2 ' 	CSV file 
                    tw.Write("{0},{1},{2},{3:F6},{4:F6},{5},{6:F6},{7:F6},{8:F6}", tLine.viccall1, tLine.viccall2, tLine.vicanum, tLine.vicfreqrx, tLine.vicpwrrx, tLine.calctype, dInt, tLine.reqdcalc, dMargin)
                    If tLine.cSphereCalc.Equals("5") Then
                        If tLine.dAin80 > 0.0 Then
                            tw.Write(",{0:F6},{1:F6}", dInt80, dMargin80)
                        Else
                            tw.Write(",0,0")
                        End If
                        If tLine.dAin99 > 0.0 Then
                            tw.Write(",{0:F6},{1:F6}", dInt99, dMargin99)
                        Else
                            tw.Write(",0,0")
                        End If
                    End If
                    tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)
                Case Else
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*ERROR* Invalid Report Type in Report." & Microsoft.VisualBasic.Constants.vbCrLf)
            End Select

            Return
        End Sub

        ''' <summary>
        ''' This method creates and populates a new AggIntLine object that is
        ''' used later to produce the aggregate interference report (.AGGINT).  
        ''' </summary>
        ''' <paramname="tRec"> - TtChan object providing source data.</param>
        ''' <paramname="tLine"> - AggIntLine object to be used for .AGGINT report generation.</param>
        ''' <paramname="cSphereCalc"> - the type of loss calaculation to be used.</param>
        ''' <paramname="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        Public Shared Sub AgIntNewRec(tRec As TtChan, <Out> ByRef tLine As AggIntLine, cSphereCalc As String, nRepType As Integer)
            ' 'out' requirement.
            tLine = Nothing

            If tRec Is Nothing Then
                Return
            End If

            tLine = New AggIntLine()

            tLine.viccall1 = tRec.viccall1
            tLine.viccall2 = tRec.viccall2
            tLine.vicanum = tRec.vicanum
            tLine.vicfreqrx = tRec.vicfreqrx
            tLine.vicpwrrx = tRec.vicpwrrx
            tLine.calctype = tRec.calctype
            tLine.ctxinttraftx = tRec.ctxinttraftx
            tLine.ctxvictrafrx = tRec.ctxvictrafrx
            tLine.ctxeqpt = tRec.ctxeqpt
            tLine.reqdcalc = tRec.rqco
            tLine.resti = tRec.resti

            tLine.dEINCO = 0.0
            tLine.dEINCO80 = 0.0
            tLine.dEINCO99 = 0.0
            tLine.dAin = 0.0
            tLine.dAin80 = 0.0
            tLine.dAin99 = 0.0

            tLine.cSphereCalc = cSphereCalc

            Return
        End Sub

        ''' <summary>
        ''' This method further processes a partially-populated AggIntLine object to fill the remaining field values needed to produce aggregate interference report (.AGGINT).  
        ''' </summary>
        ''' <paramname="tRec"> - TtChan object to be tested.</param>
        ''' <paramname="tLine"> - AggIntLine object providing the data for the report.</param>
        ''' <paramname="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        ''' <paramname="dCull"> - the culling margin (dB).</param>
        Public Shared Sub AgIntProcRec(tRec As TtChan, ByRef tLine As AggIntLine, nRepType As Integer, dCull As Double)
            If tRec Is Nothing Then
                Return
            End If

            Dim dIN As Double
            Dim dIN80 As Double
            Dim dIN99 As Double
            Dim dICF As Double
            Dim deinco As Double

            If tRec.calctype.Equals("-I") OrElse tRec.calctype.Equals("I") Then
                ' 	It's -I 
                dICF = tRec.reqdcalc - tRec.rqco
                tLine.dEINCO = tRec.calcico - dICF
                If tLine.cSphereCalc.Equals("5") AndAlso tRec.ohresult > 0 AndAlso tRec.ohresult < 100 Then
                    ' 	Handle the ohloss calcs 
                    tLine.dEINCO80 = tRec.calcico80 - dICF
                    tLine.dEINCO99 = tRec.calcico99 - dICF
                Else
                    tLine.dEINCO80 = 0.0
                    tLine.dEINCO99 = 0.0
                End If

                ' 	Store the worst case requirement in the required calc field 
                If tLine.reqdcalc > tRec.rqco Then
                    tLine.reqdcalc = tRec.rqco
                End If
            Else
                ' It's C/I 
                dICF = tRec.rqco - tRec.reqdcalc
                dIN = tRec.vicpwrrx - tRec.calcico
                tLine.dEINCO = dIN - dICF
                If tLine.cSphereCalc.Equals("5") AndAlso tRec.ohresult > 0 AndAlso tRec.ohresult < 100 Then
                    dIN80 = tRec.vicpwrrx - tRec.calcico80
                    tLine.dEINCO80 = dIN80 - dICF
                    dIN99 = tRec.vicpwrrx - tRec.calcico99
                    tLine.dEINCO99 = dIN99 - dICF
                Else
                    tLine.dEINCO80 = 0.0
                    tLine.dEINCO99 = 0.0
                End If
                ' 	Store the worst case requirement in the required calc field 
                If tLine.reqdcalc < tRec.rqco Then
                    tLine.reqdcalc = tRec.rqco
                End If
            End If

            ' 	Accumulate the einco only for those values that fail the cull. 
            If tRec.resti <= dCull Then
                deinco = Pow(10.0, tLine.dEINCO / 10.0)
                tLine.dAin += deinco
            End If
            If tLine.cSphereCalc.Equals("5") AndAlso tRec.ohresult > 0 AndAlso tRec.ohresult < 100 Then
                If tRec.resti80 <= dCull Then
                    deinco = Pow(10.0, tLine.dEINCO80 / 10.0)
                    tLine.dAin80 += deinco
                End If
                If tRec.resti99 <= dCull + 10.0 Then
                    deinco = Pow(10.0, tLine.dEINCO99 / 10.0)
                    tLine.dAin99 += deinco
                End If
            End If

            Return
        End Sub

        ''' <summary>
        ''' This method produces the records for the aggregate interference report - 1181 - GJS.  
        ''' </summary>
        ''' <paramname="tw"></param>
        ''' <paramname="viewName"> - The name of the table</param>
        ''' <paramname="dCull"> - Culling Margin</param>
        ''' <paramname="cSphereCalc"> - The type of loss calc.</param>
        ''' <paramname="nRepType"> - 1: Report, 2: CSV</param>
        ''' <returns></returns>
        Public Shared Function AggIntRecs(tw As TextWriter, viewName As String, dCull As Double, cSphereCalc As String, nRepType As Integer) As Integer          ' 	The output file handle		
            ' 	The name of the table 		
            ' 	Culling Margin						
            ' 	The type of loss calc.		
            ' 	1: Report, 2: CSV					
            Dim nRet As Integer
            Dim cLastCall As String
            Dim dLastFrx As Double
            Dim tLine As AggIntLine = New AggIntLine() ' null;      /*	The output line 
            Dim tRec As TtChan
            Dim chanNulls As SQLLEN()
            Dim cTable As String
            Dim nRead As Integer
            Dim nPassed As Integer
            Dim hConn As SQLHANDLE
            Dim hStmt As SQLHANDLE

            ' 	Set up the actual channel table name 
            cTable = String.Format("tt_{0}_chan", viewName)

            TpRunTsip.TtDynChan.TtChanPrepareRead(cTable, "viccall1,viccall2,vicanum,vicfreqrx", Nothing, hConn, hStmt)

            cLastCall = ""
            dLastFrx = 0.0
            nRead = 0
            nPassed = 0
            While True
                nRet = TpRunTsip.TtDynChan.TtChanRead(hStmt, tRec, chanNulls)
                If nRet <> 0 Then
                    Exit While
                End If
                nRead += 1

                ' 	Perform the culling 
                If Not TpRunTsip.AggInt.PassCull(tRec, cSphereCalc, dCull) Then
                    Continue While
                End If
                nPassed += 1

                ' Do a breakpoint sort on the call1 and the freqrx 
                If Not tRec.viccall1.Equals(cLastCall) Then
                    If Not cLastCall.Equals("") Then
                        ' 	Output the last record 
                        TpRunTsip.AggInt.AgIntOutRec(tw, tLine, nRepType)
                    End If
                    ' agintnewrec(fRep, &tRec, &tLine, nRepType); 
                    cLastCall = tRec.viccall1
                    dLastFrx = -1.0
                End If

                If tRec.vicfreqrx <> dLastFrx Then
                    If dLastFrx > 0.0 Then
                        TpRunTsip.AggInt.AgIntOutRec(tw, tLine, nRepType)
                    End If
                    TpRunTsip.AggInt.AgIntNewRec(tRec, tLine, cSphereCalc, nRepType)
                    dLastFrx = tRec.vicfreqrx
                End If

                ' 	Process the record 
                TpRunTsip.AggInt.AgIntProcRec(tRec, tLine, nRepType, dCull)
            End While
            If dLastFrx <> 0.0 OrElse Not cLastCall.Equals("") Then
                TpRunTsip.AggInt.AgIntOutRec(tw, tLine, nRepType)        ' 	Output the last one 
            End If

            TpRunTsip.AggInt.AggIntStatRec(tw, nRepType, nRead, nPassed)

            ' Release ODBC resources.
            TpRunTsip.TtDynChan.TtChanClose(hConn, hStmt)

            Return 0
        End Function

        ''' <summary>
        ''' This method produces the statistics message for the .AGGINT report.  
        ''' </summary>
        ''' <paramname="tw"> - TextWriter object that the report is written to.</param>
        ''' <paramname="nRepType"> - prescribes the type of AGGINT report to be produced, either a human-readable text file or an EXCEL-readable .csv file.</param>
        ''' <paramname="nRead"> - number of records read.</param>
        ''' <paramname="nPassed"> - number of records that survived culling.</param>
        Public Shared Sub AggIntStatRec(tw As TextWriter, nRepType As Integer, nRead As Integer, nPassed As Integer)
            Select Case nRepType
                Case 1
                    tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & Microsoft.VisualBasic.Constants.vbCrLf & "Records Read........: {0,5:D}" & Microsoft.VisualBasic.Constants.vbCrLf & "Records Passing Cull: {1,5:D}" & Microsoft.VisualBasic.Constants.vbCrLf, nRead, nPassed)

                Case 2
                Case Else
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*ERROR* Invalid Report Type in Statistics." & Microsoft.VisualBasic.Constants.vbCrLf)
            End Select
        End Sub

        ''' <summary>
        ''' This method produces the Aggregate Interference report encoded in CSV format.  
        ''' </summary>
        ''' <paramname="tw"> - TextWriter object that the report is written to.</param>
        ''' <paramname="viewName"> - the name of the DB table that is the source for the data.</param>
        ''' <paramname="cSphereCalc"> - the type of the loss calculation.</param>
        ''' <paramname="dCull"> - the culling margin (dB).</param>
        Public Shared Sub AggIntCSV(tw As TextWriter, viewName As String, cSphereCalc As String, dCull As Double)                  ' 	The output file handle		
            ' 	The name of the table 		
            ' 	The loss calculation 			
            ' 	Culling Margin						
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf & "viccall1,viccall2,vicanum,vicfreqrx,vicpwrrx,calctype,aggint,rqco,margin")
            If cSphereCalc.Equals("5") Then
                tw.Write(",aggint80,margin80,aggint99,margin99")
            End If
            tw.Write(Microsoft.VisualBasic.Constants.vbCrLf)

            TpRunTsip.AggInt.AggIntRecs(tw, viewName, dCull, cSphereCalc, 2)  ' 	The 2 is for a CSV file 

            Return
        End Sub







    End Class
End Namespace
