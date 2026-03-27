Imports _DataStructures
Imports _Utillib
Imports System.IO

Namespace TpRunTsip
    ''' <summary>
    ''' Provides methods used to generate the Study Report (.STUDY) for Ts-Ts.
    ''' </summary>
    Public Class Tstsrp1

        ''' <summary>
        ''' This method generates the Study Report fragment  for a single case of Ts-Ts interference.
        ''' </summary>
        ''' <paramname="tw"> - the TextWriter object assigned to the .STUDY report.</param>
        ''' <paramname="tpParm"> - TpParm object containing user-defined parameters.</param>
        ''' <paramname="cTableName"> - the unique ID substring of the Tt DB table names.</param>
        ''' <paramname="p"> - see the source code for context.</param>
        ''' <paramname="q"> - see the source code for context.</param>
        ''' <paramname="r"> - see the source code for context.</param>
        ''' <paramname="s"> - see the source code for context.</param>
        ''' <paramname="t"> - see the source code for context.</param>
        ''' <paramname="u"> - see the source code for context.</param>
        ''' <paramname="v"> - see the source code for context.</param>
        ''' <paramname="w"> - see the source code for context.</param>
        ''' <paramname="x"> - see the source code for context.</param>
        ''' <paramname="y"> - see the source code for context.</param>
        ''' <returns></returns>
        Public Shared Function TsTsRp1(tw As TextWriter, tpParm As TpParm, cTableName As String, p As String, q As String, r As String, s As String, t As String, u As String, v As String, w As String, x As String, y As String) As Integer
            '...Log2.v("\nTstsrp1.TstSRP1(): Entry: cTableName = " + cTableName);

            Dim pl As TpRunTsip.PrintLine = New TpRunTsip.PrintLine()
            pl.OutFile(tw)

            Dim nPage = 1
            Dim [cDate] As String
            Dim cTime As String
            Dim cAggRep As String

            pl.Output()
            pl.LeftAt(5, "FREQUENCY COORDINATION SYSTEM ASSOCIATION")
            pl.IntAt(70, "PAGE: {0,3:D}", nPage)
            pl.Output()

            GenUtil.UtGetDateTime([cDate], cTime)
            pl.LeftAt(5, "MASTER DATA BASE -- Interference Study Summary ")
            pl.LeftAt(74, cTime)
            pl.Output()

            pl.LeftAt(10, "Project Code              : ")
            pl.LeftAt(-1, p)
            pl.Output()
            pl.LeftAt(10, "PDF File Name             : ")
            pl.LeftAt(-1, tpParm.proname)
            pl.Output()
            pl.LeftAt(10, "Run Name                  : ")
            pl.LeftAt(-1, tpParm.runname)
            pl.Output()
            pl.LeftAt(10, "PDF Type                  : ")
            pl.LeftAt(-1, tpParm.protype)
            pl.Output()
            pl.LeftAt(10, "Environment Type          : ")
            pl.LeftAt(-1, tpParm.envtype)
            pl.Output()
            pl.LeftAt(10, "Environment Name          : ")
            pl.LeftAt(-1, tpParm.envname)
            pl.Output()
            pl.LeftAt(10, "Country (CAN/USA/ALL)     : ")
            pl.LeftAt(-1, tpParm.country)
            pl.Output()
            pl.LeftAt(10, "Selected Environment Sites: ")
            pl.LeftAt(-1, tpParm.selsites)
            pl.Output()
            If tpParm.selsites.Equals("CALL SIGN") Then
                pl.LeftAt(10, "Operator Codes            : ALL")
                pl.Output()
                pl.LeftAt(10, "Call Signs                : ")
                pl.LeftAt(-1, tpParm.codes)
                pl.Output()
            ElseIf tpParm.selsites.Equals("OPERATOR CODE") Then
                pl.LeftAt(10, "Call Sign Codes           : ALL")
                pl.Output()
                pl.LeftAt(10, "Operator Codes            : ")
                pl.LeftAt(-1, tpParm.codes)
                pl.Output()
            Else
                pl.LeftAt(10, "Operator Codes            : ALL")
                pl.Output()
                pl.LeftAt(10, "Call Sign Codes           : ALL")
                pl.Output()
            End If

            pl.Output()
            pl.LeftAt(10, "Study Date                : ")
            pl.LeftAt(-1, tpParm.mdate)
            pl.Output()
            pl.LeftAt(10, "Study Time                : ")
            pl.LeftAt(-1, tpParm.mtime)
            pl.Output()
            pl.LeftAt(10, "CPU Time                  : ")
            pl.LeftAt(-1, q)
            pl.Output()
            pl.LeftAt(10, "Elapsed Time              : ")
            pl.LeftAt(-1, r)
            pl.Output()
            pl.Output()
            pl.LeftAt(10, "Geostationary Orbit Study  (Y/N): ")
            pl.LeftAt(-1, tpParm.tsorbout)
            pl.Output()
            pl.LeftAt(10, "Propagation Loss Model          : ")
            Select Case tpParm.spherecalc(0)
                Case "1"c
                    pl.LeftAt(-1, "TSIP CCIR-SJM")

                Case "2"c
                    pl.LeftAt(-1, "Spherical Earth")

                Case "3"c
                    pl.LeftAt(-1, "Free Space")

                Case "4"c
                    pl.LeftAt(-1, "PCS-HATA")

                Case "5"c
                    pl.LeftAt(-1, "OH-LOSS")
                Case Else
            End Select
            pl.Output()

            pl.LeftAt(10, "Maximum Frequency Separation : ")
            pl.DoubleAt(-1, "{0,5:F0}", tpParm.fsep)
            pl.Output()
            pl.LeftAt(10, "Coordination Distance (km)   : ")
            pl.DoubleAt(-1, "{0,5:F0}", tpParm.coordist)
            pl.Output()
            pl.LeftAt(10, "Analysis type                :  ")
            pl.LeftAt(-1, tpParm.analopt)
            pl.Output()
            pl.LeftAt(10, "Margin (dB)                  : ")
            pl.DoubleAt(-1, "{0,5:F1}", tpParm.margin)
            pl.Output()
            pl.LeftAt(10, "Channel Status Codes (0 - 9) : ")
            pl.LeftAt(-1, tpParm.chancodes)
            pl.Output()
            pl.Output()

            pl.LeftAt(10, "Reports run:")
            pl.Output()
            pl.LeftAt(10, "Execution Report             : ")
            pl.LeftAt(-1, t)
            pl.Output()
            pl.LeftAt(10, "Study Summary Report         : ")
            pl.LeftAt(-1, u)
            pl.Output()
            pl.LeftAt(10, "Stations Analyzed Report     : ")
            pl.LeftAt(-1, v)
            pl.Output()
            pl.LeftAt(10, "Case Detail Report           : ")
            pl.LeftAt(-1, w)
            pl.Output()
            pl.LeftAt(10, "Case Summary Report          : ")
            pl.LeftAt(-1, x)
            pl.Output()
            pl.LeftAt(10, "Aggregate Interference Report: ")
            cAggRep = y(0).ToString()
            pl.LeftAt(-1, cAggRep)
            pl.Output()
            pl.LeftAt(10, "Aggregate Interference CSV   : ")
            cAggRep = y(1).ToString()
            pl.LeftAt(-1, cAggRep)
            pl.Output()

            If tpParm.tsorbout.Equals("Y") Then
                pl.LeftAt(10, "Orbit Report             : Y")
                pl.Output()
            End If
            pl.Output()

            pl.LeftAt(10, "Number of Stations Passed : ")
            pl.RightAt(41, s)
            pl.Output()
            pl.LeftAt(10, "Number of TS Cases        : ")
            pl.IntAt(37, "{0,5}", tpParm.numcases)
            pl.Output()

            pl.LeftAt(10, "Number of ES Cases        : ")
            pl.IntAt(37, "{0,5}", tpParm.numtecases)
            pl.Output()
            pl.LeftAt(10, "     Interference Tables:-")
            pl.Output()
            pl.LeftAt(10, "          tt_{0}_parm Table", cTableName)
            pl.Output()
            pl.LeftAt(10, "          tt_{0}_erro Table", cTableName)
            pl.Output()
            pl.LeftAt(10, "          tt_{0}_site Table", cTableName)
            pl.Output()
            pl.LeftAt(10, "          tt_{0}_ante Table", cTableName)
            pl.Output()
            pl.LeftAt(10, "          tt_{0}_chan Table", cTableName)
            pl.Output()

            '...Log2.v("\nTstsrp1.TstSRP1(): Exit");
            Return 0
        End Function


    End Class
End Namespace
