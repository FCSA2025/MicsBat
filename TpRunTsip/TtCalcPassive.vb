Imports _Auxlib
Imports _DataStructures
Imports System
Imports System.Math
Imports _NewLib.Maths
Imports _Configuration
Imports _Utillib
Imports _NewLib
Imports System.Runtime.InteropServices

Namespace TpRunTsip
    ''' <summary>
    ''' Provides methods that perform the 'passive' calculations for TSIP.
    ''' </summary>
    ''' <remarks>
    ''' The 'k' in TtCalkPassive is not a typo. This class contains a legacy method
    ''' called TtCalcPassive(); a class can't have a method name identical to its class name.
    ''' </remarks>
    Public Class TtCalkPassive
        ''' <summary>
        ''' Calculates the discrimination and antenna gains 
        ''' for a passive antenna.  
        ''' </summary>
        ''' <paramname="isInt"> - not used.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="TableName"> - antenna input file name.</param>
        ''' <paramname="call1"> - passive Call Sign</param>
        ''' <paramname="call2"> - other end from passive.</param>
        ''' <paramname="bndcde"> - band code.</param>
        ''' <paramname="anum"> - antenna number.</param>
        ''' <paramname="freq"> - operating frequency.</param>
        ''' <paramname="offAxisAng"> off-axis angle.</param>
        ''' <paramname="disc"> - discrimination.</param>
        ''' <paramname="pGain"> - antenna gain.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function TtCalcPassive(isInt As Boolean, isMDB As Boolean, TableName As String, call1 As String, call2 As String, bndcde As String, anum As Short, freq As Double, offAxisAng As Double, <Out> ByRef disc As Double, <Out> ByRef pGain As Double, intPrintMsg As String, vicPrintMsg As String) As Integer           ' 	antenna input file name  
            ' 	Passive Call Sign				 
            ' 	Other end from Passive   
            '...Log2.v("\nTtCalcPassive.TtCalcPassive(): Entry");

            ' 'out' requirements.
            disc = 0.0
            pGain = 0.0

            Dim acodeP As String
            Dim rc As Integer
            Dim height As Double
            Dim width As Double
            Dim htB As Double
            Dim htA As Double
            Dim htP As Double
            Dim distAP As Double
            Dim azimAP As Double
            Dim azimPA As Double
            Dim clHtA As Double
            Dim clHtB As Double
            Dim distBP As Double
            Dim azimBP As Double
            Dim azimPB As Double
            Dim xPA As Double
            Dim yPA As Double
            Dim zPA As Double
            Dim xPB As Double
            Dim yPB As Double
            Dim zPB As Double
            Dim Y As Double
            Dim iAng As Double
            Dim clHtP As Double
            Dim elevAP As Double
            Dim elevPA As Double
            Dim elevBP As Double
            Dim elevPB As Double

            Dim siteA As FtSite
            Dim siteB As FtSite
            Dim siteP As FtSite

            Dim dTGain As Double
            Dim dAreaOrGain As Double

            ' first establish the geometry.  
            If CSharpImpl.__Assign(rc, TpRunTsip.TtCalkPassive.SitesAPB(isInt, isMDB, TableName, call1, call2, bndcde, anum, acodeP, htA, htB, htP, siteA, siteB, siteP, dTGain, dAreaOrGain, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                Return rc
            End If

            ' parse Passive's acode for height and width 
            GenUtil.TtPassiveAcode(acodeP, height, width)

            ' calc included angle 

            ' calc dist A-P, calc azim P-A 
            AxSub2.AxDistan(siteA.latit / 100.0, siteP.latit / 100.0, siteA.longit / 100.0, siteP.longit / 100.0, distAP, azimAP, azimPA)

            ' calc dist B-P, calc azim P-B 
            AxSub2.AxDistan(siteB.latit / 100.0, siteP.latit / 100.0, siteB.longit / 100.0, siteP.longit / 100.0, distBP, azimBP, azimPB)

            ' calculate the center line heights in km 
            clHtA = (siteA.grnd + htA) / 1000.0
            clHtP = (siteP.grnd + htP) / 1000.0
            clHtB = (siteB.grnd + htB) / 1000.0
            ' calc elev P-A 
            AxSub3.AxElev(clHtA, clHtP, distAP, elevAP, elevPA)
            ' calc elev P-B 
            AxSub3.AxElev(clHtB, clHtP, distBP, elevBP, elevPB)

            xPA = distAP * CosD(180.0 - azimPA) * CosD(elevPA)
            yPA = distAP * CosD(azimPA - 90.0) * CosD(elevPA)
            zPA = distAP * SinD(elevPA)

            xPB = distBP * CosD(180.0 - azimPB) * CosD(elevPB)
            yPB = distBP * CosD(azimPB - 90.0) * CosD(elevPB)
            zPB = distBP * SinD(elevPB)

            Y = (xPA * xPB + yPA * yPB + zPA * zPB) / (distAP * distBP)
            iAng = AcosD(Y)
            ' end of calc's for included angle 

            ' calc discrim for passive 
            ' 	Constant changed to 0.1 from 0.0001
            ' 	1204 - GJS - 2005.12.16 
            If offAxisAng >= -0.1 AndAlso offAxisAng <= 0.1 Then
                disc = 0.0
            Else
                If offAxisAng > 180 Then offAxisAng -= 360
                offAxisAng = Abs(offAxisAng)
                width = Abs(width)
                disc = 20.0 * Log10(width * CosD(0.5 * iAng)) + 20.0 * Log10(freq / 1000.0) + 20.0 * Log10(SinD(offAxisAng)) - 39.5995
            End If

            ' Changed to handle near field passive gains - 1222 - GJS - 2007.04.27 
            If dTGain <> 0.0 Then
                ' 	We have the gain already calculated. 
                pGain = dTGain
                rc = 0
            Else
                ' 	We have to calculate the gain ourselves.  
                If FtUtils.IsCallPassive(call2) Then
                    ' If the next site is passive, then use the passive to passive gain 
                    rc = Ax14.PassPassGain(width, height, iAng, freq / 1000.0, distAP, dAreaOrGain, pGain)        ' 	The width of the passive (m) 		
                    ' 	The height of the passive (m)  	
                    ' 	The included angle in degrees		
                    ' 	Frequency in MHz					
                    ' 	Distance to the next site (km) 	
                    ' 	The antenna area at the remote site	
                Else
                    ' use the normal passive gain calc. 
                    rc = Ax14.PassiveGain(width, height, iAng, freq / 1000.0, distAP, dAreaOrGain, pGain)         ' 	The width of the passive (m) 
                    ' 	The height of the passive (m)  
                    ' 	The included angle in degrees	
                    ' 	Frequency in MHz		
                    ' 	Distance to the next site (km) 
                    ' 	The antenna gain at the next or active
                    ' 																		*		site	
                    ' 	Output gain. 
                End If
            End If

            If rc = 0 Then
                ' disc limited to gain/2 and must be non-negative. 1204 - GJS - 2005.12 
                disc = If(disc > pGain, pGain, disc)
                disc = If(disc < 0.0, 0.0, disc)
            Else
                ' 	Problem with the passive gains.  
                disc = 0.0
            End If

            '...Log2.v("\nTtCalcPassive.TtCalcPassive(): Exit: " + disc + "  " + pGain);
            Return rc
        End Function

        ''' <summary>
        ''' This method searches for both ends of the passive path given the passive 
        ''' antenna. ie. the transmitting site A, the passive site P, and the receiving 
        ''' site B.  
        ''' </summary>
        ''' <paramname="isInt"></param>
        ''' <paramname="isMDB"></param>
        ''' <paramname="TableName"></param>
        ''' <paramname="call1"></param>
        ''' <paramname="call2"></param>
        ''' <paramname="bndcde"></param>
        ''' <paramname="anum"></param>
        ''' <paramname="acodeP"></param>
        ''' <paramname="htA"></param>
        ''' <paramname="htB"></param>
        ''' <paramname="htP"> - Antenna height of the passive</param>
        ''' <paramname="siteA"></param>
        ''' <paramname="siteB"></param>
        ''' <paramname="siteP"></param>
        ''' <paramname="dTGain"> - The tgain from siteP to siteA</param>
        ''' <paramname="dAreaOrGain"> - Effective area (passive) or gain</param>
        ''' <paramname="intPrintMsg"></param>
        ''' <paramname="vicPrintMsg"></param>
        ''' <returns></returns>
        Public Shared Function SitesAPB(isInt As Boolean, isMDB As Boolean, TableName As String, call1 As String, call2 As String, bndcde As String, anum As Short, <Out> ByRef acodeP As String, <Out> ByRef htA As Double, <Out> ByRef htB As Double, <Out> ByRef htP As Double, <Out> ByRef siteA As FtSite, <Out> ByRef siteB As FtSite, <Out> ByRef siteP As FtSite, <Out> ByRef dTGain As Double, <Out> ByRef dAreaOrGain As Double, intPrintMsg As String, vicPrintMsg As String) As Integer    ' 	Antenna height of the passive			
            ' 	The tgain from siteP to siteA			
            ' 	Effective area (passive) or gain	
            ' 'out' requirements.
            acodeP = ""
            htA = Double.MaxValue
            htB = Double.MaxValue
            htP = Double.MaxValue
            dTGain = Double.MaxValue
            dAreaOrGain = Double.MaxValue
            siteA = Nothing
            siteB = Nothing
            siteP = Nothing

            Dim [select] = ""
            Dim rc As Integer

            Dim pSite As FtSiteStr
            Dim pSiteNulls As FtSiteStrNulls

            Dim nInd As Integer
            Dim nAnt As Integer
            Dim cCallB As String
            Dim acodeA = ""

            Dim dAz1 = 0.0
            Dim dEl1 = 0.0
            Dim dAz2 = 0.0
            Dim dEl2 = 0.0

            ' 	Make sure this is a passive case. 
            If Strings.FirstCharIs(call1, "%"c) Then
                '	First get the passive site, and all its antennas.
                If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetAnte(call1, TableName, isMDB, pSite, pSiteNulls)) <> 0 Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "Site P", call1)
                    Return rc
                End If

                ' Got the site and antennas.
                siteP = pSite.stSite

                'We need the antenna that points to A (the link fed in) to get the true
                ' gain for the passive calculations.
                dTGain = 0.0

                For nInd = 0 To pSite.nNumAnts - 1
                    If call2.Equals(pSite.stAntsPtr(CInt(nInd)).call2) AndAlso bndcde.Equals(pSite.stAntsPtr(CInt(nInd)).bndcde.Trim()) Then
                        '	Found it
                        If pSite.stAntsPtr(nInd).offazm.Equals("P") AndAlso pSite.stAntsPtr(nInd).tgain <> 0.0 Then
                            dTGain = pSite.stAntsPtr(nInd).tgain
                        End If
                        Exit For
                    End If
                Next

                '	Now get the index of the antenna (B) in this site that points _AWAY_ from 
                '	the call2/bandcode fed in.  The index is nAnt.
                nInd = 0
                nAnt = -1

                rc = FtUtils.FtNextAnte(pSite, -1, Nothing, bndcde)

                While rc >= 0
                    If Not call2.Equals(pSite.stAntsPtr(CInt(rc)).call2) AndAlso bndcde.Equals(pSite.stAntsPtr(CInt(rc)).bndcde.Trim()) Then
                        '	Found a link pointing elsewhere, assume this is the other end.
                        nAnt = rc
                        Exit While
                    End If
                    rc = FtUtils.FtNextAnte(pSite, rc, Nothing, bndcde)
                End While
                If nAnt >= 0 Then
                    acodeP = pSite.stAntsPtr(nAnt).acode
                    htP = pSite.stAntsPtr(nAnt).aht
                    cCallB = pSite.stAntsPtr(nAnt).call2
                Else
                    Dim str = String.Format("Could not get away antenna for link {0}->{1}/{2}", call1, call2, bndcde)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcPassive.SitesAPB(): ERROR: " & str)
                    GenUtil.SetErr("SitesAPB: " & str)
                    Return rc
                End If

                ' 	Get the other end of this passive link and all its antennas. (Site A) 
                If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetAnte(call2, TableName, isMDB, pSite, pSiteNulls)) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE A", [select])
                    Return rc
                End If

                siteA = pSite.stSite

                '	Get the link pointing back to get the antenna height.
                For nInd = 0 To pSite.nNumAnts - 1
                    If pSite.stAntsPtr(nInd).call2.Equals(call1) AndAlso pSite.stAntsPtr(nInd).bndcde.Equals(bndcde) Then
                        '	Found it.
                        htA = pSite.stAntsPtr(nInd).aht
                        acodeA = pSite.stAntsPtr(nInd).acode
                        dAz1 = pSite.stAntsPtr(nInd).azmth
                        dEl1 = pSite.stAntsPtr(nInd).elvtn        ' 	Az and El back to passive. 
                    ElseIf Not pSite.stAntsPtr(nInd).call2.Equals(call1) AndAlso pSite.stAntsPtr(nInd).bndcde.Equals(bndcde) Then
                        '	This link may not exist, but if it does...
                        dAz2 = pSite.stAntsPtr(nInd).azmth
                        dEl2 = pSite.stAntsPtr(nInd).elvtn    '	Pointing away from the passive.
                    End If
                Next

                ' 	Get the other end of the bounced link. (Site B) 
                If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetAnte(cCallB, TableName, isMDB, pSite, pSiteNulls)) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE B", [select])
                    Return rc
                End If

                siteB = pSite.stSite

                '	Get the other end of the passive to B link to get the antenna height.
                For nInd = 0 To pSite.nNumAnts - 1
                    If pSite.stAntsPtr(nInd).call2.Equals(call1) AndAlso pSite.stAntsPtr(nInd).bndcde.Equals(bndcde) Then
                        '	Found it.
                        htB = pSite.stAntsPtr(nInd).aht
                        Exit For
                    End If
                Next

                If dTGain = 0.0 Then
                    If FtUtils.IsBillBoard(acodeA) Then
                        ' 	This is a passive antenna,  We need to calculate the effective
                        ' 		area going to the other end.  We assume that if we don't have
                        ' 		tgain, we don't have the normal stored.  
                        Dim dAzNormal As Double
                        Dim dElNormal As Double
                        Dim dIncAngle As Double
                        Dim dHeight As Double
                        Dim dWidth As Double
                        Dim nRet As Integer

                        ' 	Calculate the normal given the two rays.  
                        nRet = Ax14.CalcPassiveNormal(dAz1, dEl1, dAz2, dEl2, dAzNormal, dElNormal)

                        ' 	Calculate the included angle from the first azimuth 
                        dIncAngle = Ax14.IncludedAngle(dAzNormal, dElNormal, dAz1, dEl1)

                        ' 	Get the dimensions of the antenna.  
                        GenUtil.TtPassiveAcode(acodeA, dHeight, dWidth)

                        ' 	Now calculate the effective area  
                        dAreaOrGain = Ax14.EffectiveArea(dWidth, dHeight, dIncAngle)
                    Else
                        ' 	This is an active antenna.  Just retrieve it and return its gain. 
                        Dim pAnte As SuAntStr = Nothing
                        If Suutils.SuGetAnt(acodeA, pAnte) = 0 Then
                            dAreaOrGain = pAnte.acAnt.again
                        Else
                            dAreaOrGain = 0.0
                            Return -2  ' 	Could not find the antenna at A 
                        End If
                    End If
                End If
            Else
                ' 	Not a passive case.  Why are we here? 
                Return -1
            End If

            Return Constant.SUCCESS
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
