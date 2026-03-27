Imports _Auxlib
Imports _Configuration
Imports _DataStructures
Imports _OHloss
Imports _NewLib
Imports _Utillib
Imports System
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Math
Imports _NewLib.Maths
Imports SQLLEN = System.Int64

Namespace TpRunTsip
    ''' <summary>
    ''' Provides methods that perform the mathematical calculations
    ''' required for TSIP analysis of Tt cases.
    ''' </summary>
    Public Class TtCalcs

#If PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]

        private extern static int ttTsorbCalcs([In] TpParm parmStruct,
                      [In] string offazm,
                      [In] float aht,
                      [In] string call2,
                      [In] string bndcde,
                      [In] float tazmth,
                      [In] float telvtn,
                      [In] FtSite siteStruct);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]

        private extern static int calcTotADisc([In, Out] SQLLEN[] anteNulls,
                                                    [In, Out] TtChan chanStruct,
                                                    [In, Out] TtAnte anteStruct,
                                                    [In, Out] ref double totAdiscX,
                                                    [In, Out] ref double totAdiscC,
                                                    [In, Out] ref short copolar,
                                                    [In, Out] ref SQLLEN nullTotAdX,
                                                    [In, Out] ref double totAdisc);

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int CalcTotADisc_NATIVE(SQLLEN[] anteNulls,
                                                    TtChan chanStruct,
                                                    TtAnte anteStruct,
                                                    ref double totAdiscX,
                                                    ref double totAdiscC,
                                                    ref short copolar,
                                                    ref SQLLEN nullTotAdX,
                                                    ref double totAdisc)
        {
            //...Log2.v("\nTtCalcs.CalcTotADisc_NATIVE(): Entry");
            int rc = Constant.FAILURE;

            try
            {
                rc = calcTotADisc(anteNulls,
                                  chanStruct,
                                  anteStruct,
                                  ref totAdiscX,
                                  ref totAdiscC,
                                  ref copolar,
                                  ref nullTotAdX,
                                  ref totAdisc);
            }
            catch (Exception e)
            {
                Log2.e("\nTtCalcs.CalcTotADisc_NATIVE(): ERROR: " + e.Message);
                Log2.e("\nTtCalcs.CalcTotADisc_NATIVE(): ERROR: " + e.StackTrace);
            }

            //...Log2.v("\nTtCalcs.CalcTotADisc_NATIVE(): Exit");
            return rc;
        }

        public static int TtTsorbCalcs_NATIVE(TpParm parmStruct,
                      string offazm,
                      float aht,
                      string call2,
                      string bndcde,
                      float tazmth,
                      float telvtn,
                      FtSite siteStruct)
        {
            //...Log2.v("\nTtCalcs.TtTsorbCalcs_NATIVE(): Entry");

            int result;

            result = ttTsorbCalcs(parmStruct, offazm, aht, call2, bndcde, tazmth, telvtn, siteStruct);

            //...Log2.v("\nTtCalcs.TtTsorbCalcs_NATIVE(): Exit");
            return result;
        }
#End If

        Private Shared ecalctype As Enums.Ecalctype

        ''' <summary>
        ''' This method performs ORBIT or TsOrb calculations if the User requested 
        ''' TsOrb calcs in the parameter file.  
        ''' </summary>
        ''' <paramname="parmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="offazm"> - TBD.</param>
        ''' <paramname="aht"> - TBD.</param>
        ''' <paramname="call2"> - TBD.</param>
        ''' <paramname="bndcde"> - TBD.</param>
        ''' <paramname="tazmth"> - TBD.</param>
        ''' <paramname="telvtn"> - TBD.</param>
        ''' <paramname="siteStruct"> - FtSite object encapsulating the site information.</param>
        ''' <returns></returns>
        Public Shared Function TtTsorbCalcs(parmStruct As TpParm, offazm As String, aht As Single, call2 As String, bndcde As String, tazmth As Single, telvtn As Single, siteStruct As FtSite) As Integer
            Dim dist = 0.0
            Dim azim = 0.0
            Dim elevAng = 0.0
            Dim insL = 0.0
            Dim minAngSep = 0.0
            Dim e1_10 = 0.0
            Dim e10_15 = 0.0
            Dim eGt15 = 0.0
            Dim crAzSt = 0.0
            Dim crAzEnd = 0.0
            Dim crLSt = 0.0
            Dim crLEnd = 0.0
            Dim trueAzim = 0.0
            Dim trueElev = 0.0

            Dim insLSen = ""
            Dim crLStSen = ""
            Dim crLEndSen = ""
            Dim siteSelect = ""

            Dim rc As Integer
            Dim nInd As Integer
            Dim trueVals = Constant.FALSE
            Dim trans = 0

            'FILE fp;

            Dim pSite As FtSiteStr
            Dim pSiteNulls As FtSiteStrNulls

            Dim s1 As AxStation = New AxStation()
            Dim s2 As AxStation = New AxStation()

            ' Copy proposed's call1 information into Orbit's station 1
            ' structure and the porposed's call2 information into station 2 
            ' structure - both site and antenna information are needed

            s1.elevM = siteStruct.grnd
            s1.name = siteStruct.name

            TpSub.TpLoadLat(siteStruct.latit, s1.LL.latSens, s1.LL.latDeg, s1.LL.latMin, s1.LL.latSec)

            TpSub.TpLoadLong(siteStruct.longit, s1.LL.longSens, s1.LL.longDeg, s1.LL.longMin, s1.LL.longSec)

            s1.antHtM = aht

            ' Now fill in s2 struct with call2's info.
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtFullSiteGet(call2, parmStruct.proname, False, pSite, pSiteNulls)) <> 0 Then
                If rc < 0 Then
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", siteSelect)
                End If
                Return rc
            End If

            ' A record was found for selected data /
            s2.name = pSite.stSite.name
            s2.elevM = pSite.stSite.grnd

            TpSub.TpLoadLat(pSite.stSite.latit, s2.LL.latSens, s2.LL.latDeg, s2.LL.latMin, s2.LL.latSec)

            TpSub.TpLoadLong(pSite.stSite.longit, s2.LL.longSens, s2.LL.longDeg, s2.LL.longMin, s2.LL.longSec)

            ' Need to get the call2's antenna height /
            nInd = FtUtils.FtGetMainAnte(pSite, siteStruct.call1, bndcde)
            If nInd < 0 Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtTsorbCalcs(): ERROR: FtGetMainAnte() failed to find a main antenna at opposite end.")
                GenUtil.SetErr("ttTsorbCalcs: No main antenna at opposite end: %s, %s, %s", siteStruct.call1, call2, bndcde)
                Return rc
            End If

            ' A record was found for selected data /
            s2.antHtM = pSite.stAntsPtr(nInd).aht


            ' -------Perform ORBIT or tsOrb calculations-------/

            If Strings.FirstCharIs(offazm, "Y"c) Then
                trueVals = Constant.TRUE
                trueAzim = tazmth
                trueElev = telvtn
            End If
            rc = AxOrbitSupp.AxOrbit(Constant.METERS, s1, s2, trueAzim, trueElev, trueVals, dist, azim, elevAng, insL, insLSen, minAngSep, e1_10, e10_15, eGt15, crAzSt, crAzEnd, crLSt, crLStSen, crLEnd, crLEndSen, trans)


            ' Write to the Orbit report.

            TpRunTsip.TpRunTsip.mReports.OrbitWritten = True

            AxOrbitSupp.AxRptOrb(TpRunTsip.TpRunTsip.mTW_ORBIT, dist, azim, elevAng, insL, insLSen, minAngSep, e1_10, e10_15, eGt15, crAzSt, crAzEnd, crLSt, crLStSen, crLEnd, crLEndSen, trans, rc, trueVals, s1, s2)

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method performs the calculations for a TSIP TS-TS case
        ''' at the site level.  
        ''' </summary>
        ''' <paramname="ttSite"> - TtSite object describing the current site.</param>
        ''' <paramname="siteNulls"> - ODBC nullInds associated with ttSite.</param>
        ''' <paramname="intLnkGrnd"> - TBD.</param>
        ''' <paramname="vicLnkGrnd"> - TBD.</param>
        ''' <paramname="azimIV"> - TBD.</param>
        ''' <paramname="azimVI"> - TBD.</param>
        ''' <paramname="azimXY"> - azimuth bearing from interferer -> remote.</param>
        ''' <paramname="azimAB"> - azimuth bearing from victim -> remote.</param>
        Public Shared Sub TtSiteCalcs(ByRef ttSite As TtSite, ByRef siteNulls As SQLLEN(), intLnkGrnd As Double, vicLnkGrnd As Double, azimIV As Double, azimVI As Double, azimXY As Double, azimAB As Double)            ' 	Interferor . remote 
            ' 	Victim . remote 
            Dim offax As Double

            ' compute distance advantage	 
            ttSite.distadv = 20.0 * Math.Log10(ttSite.int1vic1dist / ttSite.vic1vic2dist)
            siteNulls(_DataStructures.TtSite.DISTADV) = Constant.DB_NOT_NULL

            ' compute interferer's off-axis angle 
            TpRunTsip.TtCalcs.OffAngle(ttSite.int1vic1dist, ttSite.int1int2dist, ttSite.intgrnd, ttSite.vicgrnd, intLnkGrnd, azimIV, azimXY, offax)
            ttSite.intoffax = offax
            siteNulls(_DataStructures.TtSite.INTOFFAX) = Constant.DB_NOT_NULL

            ' compute victims's off-axis angle 
            TpRunTsip.TtCalcs.OffAngle(ttSite.int1vic1dist, ttSite.vic1vic2dist, ttSite.vicgrnd, ttSite.intgrnd, vicLnkGrnd, azimVI, azimAB, offax)
            ttSite.vicoffax = offax
            siteNulls(_DataStructures.TtSite.VICOFFAX) = Constant.DB_NOT_NULL

        End Sub

        ''' <summary>
        ''' Calculates the offaxis angles between the victim and interferer and corrects 
        ''' for sign using the azimuths. The call parameters describe a victim 'V' pointing to 
        ''' its other end 'A' and interfered with from 'I'. The method returns the offaxis angle 
        ''' from azimVA.   
        ''' </summary>
        ''' <paramname="IVdist"> - distance from Interferer to Victim.</param>
        ''' <paramname="VAdist"> - distance from Victim to its other end.</param>
        ''' <paramname="Igrnd"> - ground height at Interferer site.</param>
        ''' <paramname="Vgrnd"> - ground height at Victim site.</param>
        ''' <paramname="AGrnd"> - ground height of Victim's other end.</param>
        ''' <paramname="azimVI"> - azimuth bearing from Victim to Interferer.</param>
        ''' <paramname="azimVA"> - azimuth bearing frim Victim to its other end.</param>
        ''' <paramname="offax"> - offaxis angle from azimVA.</param>
        Public Shared Sub OffAngle(IVdist As Double, VAdist As Double, Igrnd As Double, Vgrnd As Double, AGrnd As Double, azimVI As Double, azimVA As Double, <Out> ByRef offax As Double)
            If IVdist <= 0.001 Then
                offax = 90.0
            Else
                TpRunTsip.TtCalcs.TtCalcIAng(Igrnd, Vgrnd, AGrnd, IVdist, VAdist, azimVI, azimVA, offax)
            End If
        End Sub

        ''' <summary>
        ''' Calculates the subtended angle XAY between 3 points given the
        ''' ground altitude and distance, and the azimuths between the two sites and 
        ''' the common apex.  
        ''' </summary>
        ''' <paramname="Xgrnd"> - ground height of site X.</param>
        ''' <paramname="Agrnd"> - ground height of site A.</param>
        ''' <paramname="Ygrnd"> - ground height of site Y.</param>
        ''' <paramname="XAdist"> - distance between X and A.</param>
        ''' <paramname="XYdist"> - distance between X and Y.</param>
        ''' <paramname="azimXA"> - azimuth bearing from X to A.</param>
        ''' <paramname="azimXY"> - azimuth bearing from X to Y.</param>
        ''' <paramname="iAng"> - the calculated subtended angle in degrees.</param>
        Public Shared Sub TtCalcIAng(Xgrnd As Double, Agrnd As Double, Ygrnd As Double, XAdist As Double, XYdist As Double, azimXA As Double, azimXY As Double, <Out> ByRef iAng As Double)
            ' 'out' requirement.
            iAng = 0.0

            Dim elevXA, elevAX, elevXY, elevYX As Double
            Dim xXA, yXA, zXA, xXY, yXY, zXY, Y As Double
            Dim dCheck As Double

            ' calc elev X-A 
            AxSub3.AxElev(Xgrnd / 1000.0, Agrnd / 1000.0, XAdist, elevXA, elevAX)
            ' calc elev X-Y 
            AxSub3.AxElev(Xgrnd / 1000.0, Ygrnd / 1000.0, XYdist, elevXY, elevYX)

            xXA = XAdist * CosD(180.0 - azimXA) * CosD(elevXA)
            yXA = XAdist * CosD(azimXA - 90.0) * CosD(elevXA)
            zXA = XAdist * SinD(elevXA)

            xXY = XYdist * CosD(180.0 - azimXY) * CosD(elevXY)
            yXY = XYdist * CosD(azimXY - 90.0) * CosD(elevXY)
            zXY = XYdist * SinD(elevXY)

            Y = (xXA * xXY + yXA * yXY + zXA * zXY) / (XAdist * XYdist)

            If Y > 0.99999 Then
                iAng = 0.0
            Else
                iAng = AcosD(Y)
            End If

            ' 	Get the sign of the offaxis angle.  The sign of the angle will be the
            ' 		same as the sign of the sine (?!?) of the offaxis azimuth minus the
            ' 		boresight azimuth. 
            dCheck = SinD(azimXA - azimXY)
            If dCheck < 0.0 Then
                iAng = -iAng
            End If
        End Sub

        ''' <summary>
        ''' Thismethod performs TSIP calculations for a TS-TS case at the 
        ''' antenna level.  
        ''' </summary>
        ''' <paramname="parmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="pProSite"> - FtSiteStr object describing the proposed site.</param>
        ''' <paramname="pProSiteNulls"> - ODBC nullInds associated with pProSite.</param>
        ''' <paramname="nProAntNum"> number of proposed antennae.</param>
        ''' <paramname="pEnvSite"> - FtSiteStr object describing an environment site.</param>
        ''' <paramname="pEnvSiteNulls"> - ODBC nullInds associated with pEnvSite.</param>
        ''' <paramname="nEnvAntNum"> - number of antennae at environment site.</param>
        ''' <paramname="anteStruct"> - TtAnte object providing current antenna data.</param>
        ''' <paramname="siteStruct"> - TtSite object providing current site pair data</param>
        ''' <paramname="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <paramname="dIntOffax"> - Inteferer antenna off-axis angle.</param>
        ''' <paramname="dVicOffax"> - Victim antenna off-axis angle.</param>
        ''' <returns></returns>
        Public Shared Function TtAnteCalcs(parmStruct As TpParm, pProSite As FtSiteStr, pProSiteNulls As FtSiteStrNulls, nProAntNum As Integer, pEnvSite As FtSiteStr, pEnvSiteNulls As FtSiteStrNulls, nEnvAntNum As Integer, anteStruct As TtAnte, siteStruct As TtSite, anteNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String, dIntOffax As Double, dVicOffax As Double) As Integer ' current parameter record data  
            ' current antenna data           
            ' current site pair data         
            ' (NOT_)NULL for each ante field 
            ' 	Antenna off-axis angles				
            Dim rc As Integer
            Dim isMDB As Boolean
            Dim disc As Double
            Dim junk As Double
            Dim intMbnd As Double
            Dim vicMbnd As Double
            Dim intTableName As String
            Dim vicTableName As String
            Dim pBand As SuBand
            Dim nMdbCheck As Boolean

            ' get full names of victim site and antenna tables 
            TpSub.TtTableName(siteStruct.interferer, parmStruct, intTableName, vicTableName, isMDB)

            ' check if local site is a passive reflector (interferer):
            '  if call1 begins with '%'
            ' 
            If Strings.FirstCharIs(anteStruct.intcall1, "%"c) Then
                ' get midband freq from SDB for interferer 
                If Suutils.SuGetBand(anteStruct.intbndcde, pBand) <> 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtAnteCalcs(): ERROR: call to SuGetBand() failed.")
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, anteStruct.intbndcde)
                    Return Constant.FAILURE
                End If

                intMbnd = pBand.bmidf
                pBand = Nothing

                ' calculate interfer's passive reflector and discrimination 
                If Strings.FirstCharIs(siteStruct.interferer, "P"c) Then
                    nMdbCheck = False
                Else
                    nMdbCheck = isMDB
                End If
                rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, nMdbCheck, intTableName, anteStruct.intcall1, anteStruct.intcall2, anteStruct.intbndcde, anteStruct.intanum, intMbnd, dIntOffax, disc, junk, intPrintMsg, vicPrintMsg)
                If rc <> Constant.SUCCESS Then
                    Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in Passive Calculation:-" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtAnteCalcs(): ERROR: call to TtCalcPassive() failed: " & str)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    Return rc
                End If

                anteStruct.adiscctxv = disc
                anteStruct.adiscxtxv = disc
                anteStruct.adiscctxh = disc
                anteStruct.adiscxtxh = disc
                anteNulls(TtAnte.ADISCCTXV) = Constant.DB_NOT_NULL
                anteNulls(TtAnte.ADISCXTXV) = Constant.DB_NOT_NULL
                anteNulls(TtAnte.ADISCCTXH) = Constant.DB_NOT_NULL
                anteNulls(TtAnte.ADISCXTXH) = Constant.DB_NOT_NULL
            Else
                ' interferer is not a passive repeater
                '  get antenna pattern and calculate antenna discriminations for
                '  the interferer or transmit (tx) antenna
                ' 
                '  get full table names for subsidiary ante and antd info 
                ' not temp. Ante table so Ante data from SDB 

                rc = TpGetDat.TpCalcDisc(anteStruct.intacode, dIntOffax, anteStruct.adiscctxv, anteStruct.adiscxtxv, anteStruct.adiscctxh, anteStruct.adiscxtxh, anteNulls(TtAnte.ADISCCTXV), anteNulls(TtAnte.ADISCXTXV), anteNulls(TtAnte.ADISCCTXH), anteNulls(TtAnte.ADISCXTXH), intPrintMsg, vicPrintMsg)

                If rc <> Constant.SUCCESS Then
                    If rc = Constant.CONT_PROCESSING Then
                        ' not a fatal error, continue with next ante 
                        Return Constant.SUCCESS
                    End If
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "***Error in Interferor Discrim. Calc:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                    Return rc
                End If
            End If
            ' check if local site is a passive reflector (victim):
            '  if call1 begins with '%'
            ' 
            If Strings.FirstCharIs(anteStruct.viccall1, "%"c) Then

                ' get midband freq from SDB for victim 
                If Suutils.SuGetBand(anteStruct.vicbndcde, pBand) <> 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtAnteCalcs(): ERROR: call to SuGetBand() failed.")
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, anteStruct.vicbndcde)
                    Return Constant.FAILURE
                End If

                vicMbnd = pBand.bmidf
                pBand = Nothing

                ' calculate victim's passive reflector and discrimination 
                If Strings.FirstCharIs(siteStruct.interferer, "P"c) Then
                    nMdbCheck = isMDB
                Else
                    nMdbCheck = False
                End If
                rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, nMdbCheck, vicTableName, anteStruct.viccall1, anteStruct.viccall2, anteStruct.vicbndcde, anteStruct.vicanum, vicMbnd, dVicOffax, disc, junk, intPrintMsg, vicPrintMsg)

                If rc <> Constant.SUCCESS Then
                    Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in Proposed Passive Calculation:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtAnteCalcs(): ERROR: call to TtCalcPassive() failed: " & str)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    Return rc
                End If
                anteStruct.adisccrxv = disc
                anteStruct.adiscxrxv = disc
                anteStruct.adisccrxh = disc
                anteStruct.adiscxrxh = disc

                anteNulls(TtAnte.ADISCCRXV) = Constant.DB_NOT_NULL
                anteNulls(TtAnte.ADISCXRXV) = Constant.DB_NOT_NULL
                anteNulls(TtAnte.ADISCCRXH) = Constant.DB_NOT_NULL
                anteNulls(TtAnte.ADISCXRXH) = Constant.DB_NOT_NULL ' victim is not a passive repeater
                ' 		 * get antenna pattern and calculate antenna discriminations for
                ' 		 * the victim or receive (rx) antenna
                ' 		 
            Else
                ' not temp. Ante table so Ante data from SDB 

                rc = TpGetDat.TpCalcDisc(anteStruct.vicacode, dVicOffax, anteStruct.adisccrxv, anteStruct.adiscxrxv, anteStruct.adisccrxh, anteStruct.adiscxrxh, anteNulls(TtAnte.ADISCCRXV), anteNulls(TtAnte.ADISCXRXV), anteNulls(TtAnte.ADISCCRXH), anteNulls(TtAnte.ADISCXRXH), intPrintMsg, vicPrintMsg)

                If rc <> Constant.SUCCESS Then
                    If rc = Constant.CONT_PROCESSING Then
                        ' not a fatal error, continue with next ante 
                        Return Constant.SUCCESS
                    End If


                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "***Error in Victim Discrimination Calc:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                    Return rc
                End If
            End If

            Return Constant.SUCCESS
        End Function

        'TOP

        Private Shared oldTrafTx As String = ""
        Private Shared oldTrafRx As String = ""
        Private Shared oldEqptTx As String = ""
        Private Shared oldEqptRx As String = ""
        Private Shared oldRc As Integer = 0
        Private Shared curCtx As CtxStruct = New CtxStruct()

        ''' <summary>
        ''' This method performs the TSIP calculations for a TS-TS case at the 
        ''' channel level. Calculations are done for the worst case first (called Band 
        ''' Calculations). If no interference is found then the rest of the channels on 
        ''' this antenna are not processed. If Band Calculations produce interferece 
        ''' and the User requested P/C (Plan/Channel) calculations then each channel is 
        ''' processed individually with actual, as opposed to worst case, data.  
        ''' </summary>
        ''' <paramname="chanStruct"> - TtChan object providing current channel data.</param>
        ''' <paramname="anteStruct"> - TtAnte object providing current antenna data.</param>
        ''' <paramname="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        ''' <paramname="siteStruct"> - TtSite object providing current site data.</param>
        ''' <paramname="parmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="patNulls"> - ODBC nullInds associated with antenna pattern values.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <paramname="intPowrTx"> - Interferer's transmit power level.</param>
        ''' <paramname="intAFSLtx"> - antenna feed system loss.</param>
        ''' <paramname="nullIntAfslTx"> - ODBC nullInd associated with intAFSLtx.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <paramname="totADiscW"> - total antenna discrimination.</param>
        ''' <paramname="intMbnd"> - interferer's mid-band frequency.</param>
        ''' <paramname="vicMbnd"> - victim's mid-band frequency.</param>
        ''' <paramname="intPatLoss"> - transmission path loss for the interfering signal.</param>
        ''' <returns></returns>
        Public Shared Function TtChanCalcs(chanStruct As TtChan, anteStruct As TtAnte, anteNulls As SQLLEN(), siteStruct As TtSite, parmStruct As TpParm, patNulls As SQLLEN(), chanNulls As SQLLEN(), intPowrTx As Double, intAFSLtx As Double, nullIntAfslTx As SQLLEN, intPrintMsg As String, vicPrintMsg As String, totADiscW As Double, intMbnd As Double, vicMbnd As Double, intPatLoss As Double) As Integer ' current channel data 
            ' current antenna data 
            ' Nulls for the antenna struct 
            ' current site data 
            ' current parameter record data 
            ' nulls for pattern values 
            ' (NOT_)NULL for each chan field 
            '...Log2.v("\nTtCalcs.TtChanCalcs(): Entry");

            Dim bandTmp As Double
            Dim totAdiscC = 0.0
            Dim totAdiscX As Double
            Dim vicAFSLrx As Double
            Dim viclnkAFSLtx As Double
            Dim viclnkPowTx As Double
            Dim intEqptStab As Double
            Dim vicEqptStab As Double
            Dim intAGain As Double
            Dim vicAGain As Double
            Dim totADisc = 0.0
            Dim fsepHi As Double
            Dim fsepLo As Double
            Dim fsepMid As Double
            Dim fsepMax As Double
            Dim viclnkAGain As Double
            Dim cCalcType As Char        ' The type of loss calculation selected 

            Dim intTableName As String
            Dim vicTableName As String
            Dim [select] As String
            Dim vicChanTable As String
            Dim rc As Integer
            Dim nRet As Integer
            Dim vicChanNulls As SQLLEN()  '[FT_CHAN_SIZE_];
            Dim copolar As Short = 0
            Dim viclnkANumTx As Short = 0
            Dim nullViclnkAfslTx As SQLLEN = Constant.DB_NOT_NULL
            Dim nullAntNumTx As SQLLEN = Constant.DB_NOT_NULL
            Dim nullPowTx As SQLLEN = Constant.DB_NOT_NULL
            Dim nullVicAfslRx As SQLLEN = Constant.DB_NOT_NULL
            Dim nullTotAdX As SQLLEN = Constant.DB_NOT_NULL
            Dim nullTotAdC As SQLLEN = Constant.DB_NOT_NULL
            Dim isMDB As Boolean

            Dim ftTmpChan As FtChan

            Dim intTiltDisc As Double
            Dim vicTiltDisc As Double

            Dim cVicType As Char          ' 	Either 'A' or 'D' Victim 
            Dim cIntType As Char          ' 	Either 'A' or 'D' Interferor 
            Dim tVicAnalog As TcTxAnalog      ' 	Equipments.  Only two will be 
            Dim tVicDigital As TcTxDigital    ' 	used. 
            Dim tIntAnalog As TcTxAnalog
            Dim tIntDigital As TcTxDigital

            Dim reqLo As Double
            Dim reqMid As Double
            Dim reqHi As Double

            Dim cTypeOfInt As Char

            Dim dActualDist As Double

            nullViclnkAfslTx = -1
            viclnkAFSLtx = 0.0
            totAdiscX = 0.0
            viclnkAFSLtx = 0.0
            viclnkPowTx = 0.0
            viclnkAGain = 0.0

            TpSub.TtTableName(siteStruct.interferer, parmStruct, intTableName, vicTableName, isMDB)

            GenUtil.UtCvtName(Constant.FT_CHAN, vicTableName, vicChanTable)

            ' calculate interferer and victim gains for antenna or passive 
            rc = TpRunTsip.TtCalcs.CalcGains(chanStruct, anteStruct, siteStruct, parmStruct, intAGain, vicAGain, intPrintMsg, vicPrintMsg)

            '...Log2.v("\nttChanCalcs(): 1");

            If rc <> Constant.SUCCESS Then
                Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in Calculating Victim Gain:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtChanCalcs(): ERROR: call to CalcGains() failed, the Exit: " & str)
                Console.Out.Write(str)
                Return rc
            End If

            ' Because the gain is not stored (the value stored in the gain column
            '    is the tgain from the antenna record -- which is never used) in the
            '    ttAnte structure, we need to store it here so that it prints out on
            '    the reports.
            ' 
            anteStruct.intgain = intAGain
            anteNulls(TtAnte.INTGAIN) = Constant.DB_NOT_NULL
            anteStruct.vicgain = vicAGain
            anteNulls(TtAnte.VICGAIN) = Constant.DB_NOT_NULL

            rc = TpRunTsip.TtCalcs.GetVicLnkInfo(chanStruct, anteStruct, siteStruct, parmStruct, vicTableName, isMDB, vicMbnd, viclnkAFSLtx, viclnkANumTx, viclnkPowTx, viclnkAGain, nullViclnkAfslTx, nullAntNumTx, nullPowTx, intPrintMsg, vicPrintMsg)

            If rc <> Constant.SUCCESS Then
                Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in getting victim Link Info:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtChanCalcs(): ERROR: call to GetVicLnkInfo() failed then Exit: " & str)
                TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                Return rc
            End If

            '...Log2.v("\nttChanCalcs(): 2");

            ' **************************************************************************\
            ' 
            ' 		Do the multipoint calculations, then start on the basic BAND preliminary
            ' 		calculations.  If we fail these we don't need to go further.
            ' 
            ' \***************************************************************************
            rc = TpRunTsip.TtCalcs.MultiPointCalcs(chanStruct)

            If rc <> [Error].CONTINUE Then
                Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in Multipoint Calculations:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {1}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtChanCalcs(): ERROR: call to MultiPointCalcs() failed the Exit: " & str)
                TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                Return rc
            End If

            '...Log2.v("\nttChanCalcs(): 3");

            chanStruct.patloss = intPatLoss
            chanNulls(TtChan.PATLOSS) = Constant.DB_NOT_NULL

            chanStruct.totantdisc = totADiscW
            chanNulls(TtChan.TOTANTDISC) = Constant.DB_NOT_NULL

            chanStruct.reqdcalc = 110.0
            chanNulls(TtChan.REQDCALC) = Constant.DB_NOT_NULL

            If nullAntNumTx <> Constant.DB_NULL AndAlso nullPowTx <> Constant.DB_NULL AndAlso nullIntAfslTx <> Constant.DB_NULL Then
                chanStruct.eirpadv = viclnkPowTx + viclnkAGain - viclnkAFSLtx - (40.0 + intAGain - intAFSLtx)
                chanNulls(TtChan.EIRPADV) = Constant.DB_NOT_NULL

                chanStruct.calcico = siteStruct.distadv + chanStruct.totantdisc + chanStruct.eirpadv
                chanNulls(TtChan.CALCICO) = Constant.DB_NOT_NULL

                chanStruct.resti = chanStruct.calcico - chanStruct.reqdcalc
                chanNulls(TtChan.RESTI) = Constant.DB_NOT_NULL

                If chanStruct.resti < parmStruct.margin Then
                    If parmStruct.analopt.Equals("BAND") Then
                        ' BAND (ie no Chan info) use midband freq's
                        '  for tx & rx frequencies 
                        chanStruct.intfreqtx = intMbnd
                        chanStruct.vicfreqrx = vicMbnd
                        chanNulls(TtChan.INTFREQTX) = Constant.DB_NOT_NULL
                        chanNulls(TtChan.VICFREQRX) = Constant.DB_NOT_NULL

                        chanStruct.report = Constant.TRUE
                        chanStruct.calctype = "C/I"
                        chanNulls(TtChan.CALCTYPE) = Constant.DB_NOT_NULL
                        '	Band report ...
                        Return Constant.PC_SKIP
                    End If
                Else
                    Return Constant.PC_SKIP
                End If
            Else
                Return Constant.PC_SKIP
            End If

            '...Log2.v("\nTtCalcs.TtChanCalcs(): 3-1");

            ' @@@@@@@@@@@@@  Begin Plan/Channel Calculations @@@@@@@@@@@@@@@@@@
            rc = TpGetDat.TpGetEqptStab(chanStruct.inteqpttx, chanStruct.viceqptrx, parmStruct.tempequip, intEqptStab, vicEqptStab, intPrintMsg, vicPrintMsg)

            If rc <> Constant.SUCCESS Then
                Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error getting eqpt stability for:-" & Microsoft.VisualBasic.Constants.vbLf & "Tx Equip {0} into Rx Equip: {1}" & Microsoft.VisualBasic.Constants.vbLf & "Int: {2}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {3}" & Microsoft.VisualBasic.Constants.vbLf, chanStruct.inteqpttx, chanStruct.viceqptrx, intPrintMsg, vicPrintMsg)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtChanCalcs(): ERROR: call to TpGetEqptStab() failed the Exit: " & str)
                TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                Return rc
            End If

            '...Log2.v("\nttChanCalcs(): 4");

            fsepMid = Math.Abs(chanStruct.intfreqtx - chanStruct.vicfreqrx)
            fsepHi = fsepMid + (intEqptStab * intMbnd + vicEqptStab * vicMbnd)
            fsepLo = fsepMid - (intEqptStab * intMbnd + vicEqptStab * vicMbnd)
            fsepLo = Max(0.0, fsepLo)

            ' 
            '  TASK 466: Maximum frequency separation is now set in the
            '  parameters by the user, and a choice between this and the
            '  default is set by tpMaxFSep.
            ' 
            fsepMax = TpGetDat.TpMaxFSep(parmStruct.fsep, parmStruct.coordist, siteStruct.int1vic1dist, chanStruct.inttraftx, chanStruct.victrafrx)
            If fsepHi >= fsepMax AndAlso fsepLo >= fsepMax Then
                ' not a fatal error but this case not in contention 
                Return Constant.SUCCESS
            End If

            '...Log2.v("\nttChanCalcs(): 5");

            [select] = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'", chanStruct.viccall1, chanStruct.viccall2, chanStruct.vicbndcde, chanStruct.vicchid)

            If Strings.FirstCharIs(siteStruct.interferer, "E"c) Then
                rc = TpRunTsip.TpMdbPdfGet.TtChanGet(chanStruct.viccall1, chanStruct.viccall2, chanStruct.vicbndcde, chanStruct.vicchid, vicChanTable, False, ftTmpChan, vicChanNulls)
            Else
                rc = TpRunTsip.TpMdbPdfGet.TtChanGet(chanStruct.viccall1, chanStruct.viccall2, chanStruct.vicbndcde, chanStruct.vicchid, vicChanTable, isMDB, ftTmpChan, vicChanNulls)
            End If
            If rc <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "CHAN", [select])
                If rc = Constant.FAILURE Then
                    If Strings.FirstCharIs(siteStruct.interferer, "E"c) Then
                        ' if we looked for the channel in the PDF and didn't find it this is
                        '  simply a warning - ie don't return a hard and fast stop 
                        Return Constant.SUCCESS
                    End If
                    Return rc
                End If
            End If
            If Strings.FirstCharIs(chanStruct.intcall2, "%"c) Then
                vicAFSLrx = 0.0
                nullVicAfslRx = Constant.DB_NOT_NULL
            Else
                If chanStruct.rxant = 1 Then
                    If vicChanNulls(FtChan.AFSLRX1) = Constant.DB_NULL Then
                        vicAFSLrx = 0.0
                        nullVicAfslRx = Constant.DB_NOT_NULL
                    Else
                        vicAFSLrx = ftTmpChan.afslrx1
                        nullVicAfslRx = vicChanNulls(FtChan.AFSLRX1)
                    End If
                ElseIf chanStruct.rxant = 2 Then
                    If vicChanNulls(FtChan.AFSLRX2) = Constant.DB_NULL Then
                        vicAFSLrx = 0.0
                        nullVicAfslRx = Constant.DB_NOT_NULL
                    Else
                        vicAFSLrx = ftTmpChan.afslrx2
                        nullVicAfslRx = vicChanNulls(FtChan.AFSLRX2)
                    End If
                Else
                    If vicChanNulls(FtChan.AFSLRX3) = Constant.DB_NULL Then
                        vicAFSLrx = 0.0
                        nullVicAfslRx = Constant.DB_NOT_NULL
                    Else
                        vicAFSLrx = ftTmpChan.afslrx3
                        nullVicAfslRx = vicChanNulls(FtChan.AFSLRX3)
                    End If
                End If
            End If

            '...Log2.v("\nTtCalcs.TtChanCalcs(): 5-1-2");

            If TpRunTsip.TpRunTsip.GlbIsCtxCalc = 0 Then
                '...Log2.v("\nttChanCalcs(): 5-2");

                ' 	We will be using the ctx curves when we can 
                If Not TpRunTsip.TtCalcs.oldTrafTx.Equals(chanStruct.inttraftx) OrElse Not TpRunTsip.TtCalcs.oldTrafRx.Equals(chanStruct.victrafrx) OrElse Not TpRunTsip.TtCalcs.oldEqptTx.Equals(chanStruct.inteqpttx) OrElse Not TpRunTsip.TtCalcs.oldEqptRx.Equals(chanStruct.viceqptrx) Then
                    '...Log2.v("\nttChanCalcs(): 5-3");

                    ' 	This is a new case.  Store the information and get the CTX info 
                    TpRunTsip.TtCalcs.oldTrafTx = chanStruct.inttraftx
                    TpRunTsip.TtCalcs.oldTrafRx = chanStruct.victrafrx
                    TpRunTsip.TtCalcs.oldEqptTx = chanStruct.inteqpttx
                    TpRunTsip.TtCalcs.oldEqptRx = chanStruct.viceqptrx

                    rc = TpGetDat.TpGetCtxInfo(chanStruct.inttraftx, chanStruct.victrafrx, chanStruct.viceqptrx, parmStruct.tempequip, TpRunTsip.TtCalcs.curCtx, intPrintMsg, vicPrintMsg)
                Else
                    '...Log2.v("\nttChanCalcs(): 5-4");
                    rc = TpRunTsip.TtCalcs.oldRc
                End If

                '...Log2.v("\nttChanCalcs(): 5-5");

                TpRunTsip.TtCalcs.oldRc = rc
                If rc = [Error].DYN_MS_SQL_SERVER_ERR Then
                    Return [Error].DYN_MS_SQL_SERVER_ERR ' 	Pass it back up in this case 
                End If
            Else
                '...Log2.v("\nttChanCalcs(): 5-6");
                ' 	This will cause the calculations to be used. 
                rc = Constant.FAILURE
            End If

            '...Log2.v("\nttChanCalcs(): 5-7");

            ' ************************************************************************\
            ' 
            ' 		If we did not find the ctx curve, then we will attempt to calculate the
            ' 		ctx requirements using the CTX... programs (converted to subroutines)
            ' 		1083 - GJS - 2003.12
            ' 
            ' \*************************************************************************
            If rc = Constant.SUCCESS Then
                ' 	CTX curve found.  Get the requirements from the ctx curve returned 
                chanStruct.ctxinttraftx = TpRunTsip.TtCalcs.curCtx.ctxtraftx
                chanStruct.ctxvictrafrx = TpRunTsip.TtCalcs.curCtx.ctxtrafrx
                chanStruct.ctxeqpt = TpRunTsip.TtCalcs.curCtx.ctxeqpt
                chanStruct.calctype = TpRunTsip.TtCalcs.curCtx.calcType
                chanNulls(TtChan.CTXINTTRAFTX) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CTXVICTRAFRX) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CTXEQPT) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CALCTYPE) = Constant.DB_NOT_NULL

                ' 	Store the co-channel requirement for aggregate int. repoorts
                ' 		- 1181 - GJS - 2006.02.04 
                chanStruct.rqco = TpRunTsip.TtCalcs.curCtx.rqco
                chanNulls(TtChan.RQCO) = Constant.DB_NOT_NULL

                ' 	Set the local enumeration variable for later 
                If chanStruct.calctype.Equals("-I") Then
                    TpRunTsip.TtCalcs.ecalctype = Enums.Ecalctype.eMINUSI
                Else
                    TpRunTsip.TtCalcs.ecalctype = Enums.Ecalctype.eCOVERI
                End If

                TpGetDat.TpCalcSepReqd(chanStruct.calctype, TpRunTsip.TtCalcs.curCtx.ctxPts, fsepHi, fsepMid, fsepLo, TpRunTsip.TtCalcs.curCtx.numPts, chanStruct.freqsep, chanStruct.reqdcalc)

                '...Log2.v("\nttChanCalcs(): 6");

                chanNulls(TtChan.FREQSEP) = Constant.DB_NOT_NULL
                chanNulls(TtChan.REQDCALC) = Constant.DB_NOT_NULL
            Else
                ' 	Could not find the CTX curve, use the calculations.  First get the
                ' 		appropriate equipment/traffic crossreferences. 
                rc = GetCtx.GetCtxEqpt(chanStruct.victrafrx, chanStruct.viceqptrx, chanStruct.inttraftx, chanStruct.inteqpttx, cVicType, cIntType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital)

                '...Log2.v("\nttChanCalcs(): 7");

                If rc = 0 Then
                    ' 	We got the crossreferences, first store the crossreference codes,
                    ' 		then calculate the requirements 
                    GetCtx.XrefCodes(chanStruct.inttraftx, chanStruct.inteqpttx, cIntType, tIntAnalog, tIntDigital, chanStruct.ctxinttraftx, chanStruct.ctxinteqpt)
                    GetCtx.XrefCodes(chanStruct.victrafrx, chanStruct.viceqptrx, cVicType, tVicAnalog, tVicDigital, chanStruct.ctxvictrafrx, chanStruct.ctxeqpt)

                    '...Log2.v("\nttChanCalcs(): 8");

                    chanNulls(TtChan.CTXINTTRAFTX) = Constant.DB_NOT_NULL
                    chanNulls(TtChan.CTXINTEQPT) = Constant.DB_NOT_NULL
                    chanNulls(TtChan.CTXVICTRAFRX) = Constant.DB_NOT_NULL
                    chanNulls(TtChan.CTXEQPT) = Constant.DB_NOT_NULL

                    ' 	As soon as we have retrieved the ctx?eqpt tables, we check for
                    ' 		the presence of the curves we will need and flag the record if they
                    ' 		are not there.  The curves themselves are passed automatically to
                    ' 		the calculation routines.  GJS - 1215 - 2006.04.13 
                    chanStruct.inteqtype = If(cIntType = "D"c, "D", "A")
                    chanStruct.viceqtype = If(cVicType = "D"c, "D", "A")

                    chanNulls(TtChan.INTEQTYPE) = Constant.DB_NOT_NULL
                    chanNulls(TtChan.VICEQTYPE) = Constant.DB_NOT_NULL

                    If cIntType = "D"c Then
                        ' 	Digital interferer, if there is no power spectrum store the
                        ' 		bandwidth 
                        If tIntDigital.nSpect <= 0 Then
                            ' 	There is no power spectrum, use the bandwidth 
                            chanStruct.intbwchans = tIntDigital.bandwidth
                            chanNulls(TtChan.INTBWCHANS) = Constant.DB_NOT_NULL
                        End If
                    End If

                    ' 	Check the filter response 
                    If cVicType = "D"c Then
                        ' 	The victim is digital 
                        If tVicDigital.nFilter <= 0 Then
                            chanStruct.vicbwchans = tVicDigital.bandwidth
                            chanNulls(TtChan.VICBWCHANS) = Constant.DB_NOT_NULL
                        End If
                    Else
                        ' 	The victim is analog 
                        If tVicAnalog.nFilter <= 0 Then
                            chanStruct.vicbwchans = CDbl(tVicAnalog.numchan)
                            chanNulls(TtChan.VICBWCHANS) = Constant.DB_NOT_NULL
                        End If
                    End If

                    ' 	Get the three requirements for the three frequencies centered around
                    ' 		the nominal frequency separation (+ / - the stabilities). 
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, fsepHi / 1000.0, False, cTypeOfInt, reqHi)

                    '...Log2.v("\nttChanCalcs(): 9");

                    If nRet <> 0 Then
                        ' 	If this is going to be a problem, it will occur the first time
                        ' 		and we bail out here.  We do not need to check the rest of the
                        ' 		times.
                        ' 
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "*** Error {0}. CTX curve too long or separation (%.0f) too great for" & Microsoft.VisualBasic.Constants.vbLf & "    %s into %s" & Microsoft.VisualBasic.Constants.vbLf & "    Subcase aborted." & Microsoft.VisualBasic.Constants.vbLf, nRet, fsepHi / 1000.0, intPrintMsg, vicPrintMsg)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write("    Int. Type: %c, Vic. Type: %c", cIntType, cVicType)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "    Int Traff/Equip: {0}/{1}", chanStruct.inttraftx, chanStruct.inteqpttx)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "    Vic Traff/Equip: {0}/{1}", chanStruct.victrafrx, chanStruct.viceqptrx)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "    CTX req (tfci/tfcr/eqpr): %s / %s / %s" & Microsoft.VisualBasic.Constants.vbLf, chanStruct.ctxinttraftx, chanStruct.ctxvictrafrx, chanStruct.ctxeqpt)
                        Return Constant.SUCCESS  ' 	Return success to ignore this error and carry on 
                    End If
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, fsepMid / 1000.0, False, cTypeOfInt, reqMid)
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, fsepLo / 1000.0, False, cTypeOfInt, reqLo)

                    '...Log2.v(String.Format("\nttChanCalcs(): 10, cTypeOfInt = |{0}|", cTypeOfInt));

                    ' 	Choose which of these is used depending on the type of interference 
                    If cTypeOfInt = "I"c Then
                        ' 	Interference only.  Choose the most negative 
                        '...Log2.v("\nttChanCalcs(): 10-1");
                        If reqHi <= reqMid AndAlso reqHi <= reqLo Then
                            chanStruct.freqsep = fsepHi
                            chanStruct.reqdcalc = reqHi
                        ElseIf reqLo <= reqMid AndAlso reqLo <= reqHi Then
                            chanStruct.freqsep = fsepLo
                            chanStruct.reqdcalc = reqLo
                        Else
                            chanStruct.freqsep = fsepMid
                            chanStruct.reqdcalc = reqMid
                        End If
                        chanStruct.calctype = "I"
                        TpRunTsip.TtCalcs.ecalctype = Enums.Ecalctype.eMINUSI
                    Else
                        '...Log2.v("\nttChanCalcs(): 10-2");
                        '	C/I
                        If reqHi >= reqMid AndAlso reqHi >= reqLo Then
                            chanStruct.freqsep = fsepHi
                            chanStruct.reqdcalc = reqHi
                        ElseIf reqLo >= reqMid AndAlso reqLo >= reqHi Then
                            chanStruct.freqsep = fsepLo
                            chanStruct.reqdcalc = reqLo
                        Else
                            chanStruct.freqsep = fsepMid
                            chanStruct.reqdcalc = reqMid
                        End If
                        chanStruct.calctype = "C"
                        TpRunTsip.TtCalcs.ecalctype = Enums.Ecalctype.eCOVERI
                    End If

                    chanNulls(TtChan.CALCTYPE) = Constant.DB_NOT_NULL
                    chanNulls(TtChan.FREQSEP) = Constant.DB_NOT_NULL
                    chanNulls(TtChan.REQDCALC) = Constant.DB_NOT_NULL

                    ' 	We also need to store the co-channel requirement for the Aggregate
                    ' 		Interference reports. - 1181 - GJS - 2006.02.04 
                    nRet = GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, 0.0, False, cTypeOfInt, chanStruct.rqco)

                    '...Log2.v("\nttChanCalcs(): 11");

                    chanNulls(TtChan.RQCO) = Constant.DB_NOT_NULL
                Else

                    ' 	Could not get the equipment 
                    Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "Could not retrieve equipment/traffic cross reference:-" & Microsoft.VisualBasic.Constants.vbLf & "{0} into {1}" & Microsoft.VisualBasic.Constants.vbLf & "{2}/{3} into {4}/{5}" & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg, chanStruct.inttraftx, chanStruct.inteqpttx, chanStruct.victrafrx, chanStruct.viceqptrx)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtChanCalcs(): ERROR: Exit: " & str)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    Return Constant.SUCCESS
                End If
            End If

            '...Log2.v("\nTtCalcs.TtChanCalcs(): 3");

            ' ----------------------------------------
            ' have all the data - start Calculations 
            ' ----------------------------------------

            rc = TpRunTsip.TtCalcs.CalcTotADisc(patNulls, chanStruct, anteStruct, totAdiscX, totAdiscC, copolar, nullTotAdX, totADisc)

            If rc <> Constant.SUCCESS Then
                ' not a fatal error print message and cont with next chan 
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage(rc)
                Return Constant.SUCCESS
            End If

            '...Log2.v("\nttChanCalcs(): 12");

            ' 
            '  TASK 499: Check the first two letters of the traffic code for
            '  both the interferer and the victim. If either one begins with
            '  PS, the antenna is a PCS antenna and the PCS antenna tilt
            '  discrimination has to be calculated. The order of the ground
            '  heights provided to this function is determined by which
            '  antenna is the PCS antenna.
            ' 
            '   Modified 99.03 by GJS (1021) to calculate the tilt discrimination for
            '   any antenna.
            ' 
            rc = TpRunTsip.TtCalcs.TtPcsTilt(anteStruct.intacode, siteStruct.intgrnd, anteStruct.intaht, siteStruct.vicgrnd, anteStruct.vicaht, siteStruct.int1vic1dist, intTiltDisc)

            rc = TpRunTsip.TtCalcs.TtPcsTilt(anteStruct.vicacode, siteStruct.vicgrnd, anteStruct.vicaht, siteStruct.intgrnd, anteStruct.intaht, siteStruct.int1vic1dist, vicTiltDisc)

            '...Log2.v("\nttChanCalcs(): 13");

            chanStruct.tiltdisc = vicTiltDisc + intTiltDisc
            chanStruct.totantdisc = totADisc + chanStruct.tiltdisc
            ' 	Also add them to the co and cross polar 
            totAdiscX += chanStruct.tiltdisc
            totAdiscC += chanStruct.tiltdisc
            chanNulls(TtChan.TILTDISC) = Constant.DB_NOT_NULL
            chanNulls(TtChan.TOTANTDISC) = Constant.DB_NOT_NULL

            ' 
            '  TASK 421: This method selects the proper propagation loss model
            '  according to the user's choice, indicated by the first character
            '  in parmStruct.spherecalc.
            ' 
            cCalcType = parmStruct.spherecalc(0)

            If cCalcType = "5"c Then
                ' ************************************************************************\
                '      If this is an OH-loss calculation, then we first run the pathloss
                '      as if it were Free Space Loss type '3'.  Only if it fails this
                '      do we run it as an OH-Loss case. (Task 1050)
                ' \*************************************************************************
                cCalcType = "3"c
            End If

            If TpRunTsip.TtCalcs.TtPathLoss(siteStruct, anteStruct, chanStruct, cCalcType, dActualDist) Then
                chanNulls(TtChan.PATLOSS) = Constant.DB_NOT_NULL
            Else
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.TtChanCalcs(): ERROR: Pathloss calculation failed, the Exit.")
                TpRunTsip.TpRunTsip.mTW_ERR.Write("Pathloss calculation failed" & Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf)
                Return Constant.FAILURE
            End If

            '...Log2.v("\nttChanCalcs(): 14");

            If nullViclnkAfslTx <> Constant.DB_NULL AndAlso nullAntNumTx <> Constant.DB_NULL AndAlso nullIntAfslTx <> Constant.DB_NULL Then
                chanStruct.eirpadv = viclnkPowTx - viclnkAFSLtx + viclnkAGain - (intPowrTx - intAFSLtx + intAGain)
                chanNulls(TtChan.EIRPADV) = Constant.DB_NOT_NULL
            End If

            If TpRunTsip.TtCalcs.ecalctype = Enums.Ecalctype.eMINUSI Then
                '...Log2.v("\nttChanCalcs(): 14-1");
                If nullIntAfslTx <> Constant.DB_NULL AndAlso nullTotAdC <> Constant.DB_NULL Then
                    '...Log2.v("\nttChanCalcs(): 14-1-1");
                    bandTmp = intPowrTx + intAGain - chanStruct.patloss + vicAGain - intAFSLtx - totAdiscC

                    If nullVicAfslRx <> Constant.DB_NULL Then
                        '...Log2.v("\nttChanCalcs(): 14-1-2");
                        chanStruct.calcico = bandTmp - vicAFSLrx
                        chanNulls(TtChan.CALCICO) = Constant.DB_NOT_NULL
                    End If
                End If

                '...Log2.v("\nttChanCalcs(): 14-1-3");

                If nullIntAfslTx <> Constant.DB_NULL AndAlso nullTotAdX <> Constant.DB_NULL Then
                    '...Log2.v("\nttChanCalcs(): 14-1-4");

                    bandTmp = intPowrTx + intAGain - chanStruct.patloss + vicAGain - intAFSLtx - totAdiscX

                    If nullVicAfslRx <> Constant.DB_NULL Then
                        '...Log2.v("\nttChanCalcs(): 14-1-5");

                        chanStruct.calcixp = bandTmp - vicAFSLrx
                        chanNulls(TtChan.CALCIXP) = Constant.DB_NOT_NULL
                    End If
                End If

                '...Log2.v("\nttChanCalcs(): 14-1-6");

                If copolar = Constant.TRUE Then
                    '...Log2.v("\nttChanCalcs(): 14-1-7");

                    If chanNulls(TtChan.CALCICO) <> Constant.DB_NULL Then
                        '...Log2.v("\nttChanCalcs(): 14-1-8");

                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcico
                        chanNulls(TtChan.RESTI) = Constant.DB_NOT_NULL
                    End If
                Else
                    '...Log2.v("\nttChanCalcs(): 14-1-9");

                    If chanNulls(TtChan.CALCIXP) <> Constant.DB_NULL Then
                        '...Log2.v("\nttChanCalcs(): 14-1-10");

                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcixp
                        chanNulls(TtChan.RESTI) = Constant.DB_NOT_NULL
                    End If
                End If

                '...Log2.v("\nttChanCalcs(): 14-1-11");

                If chanNulls(TtChan.RESTI) <> Constant.DB_NULL Then
                    '...Log2.v("\nttChanCalcs(): 14-1-12");

                    chanStruct.report = If(chanStruct.resti < parmStruct.margin, Constant.TRUE, Constant.FALSE)

                    '...Log2.v("\nttChanCalcs(): 14-1-13");
                End If    ' calc type is C/I 
            Else

                '...Log2.v("\nttChanCalcs(): 14-2");

                If chanNulls(TtChan.EIRPADV) <> Constant.DB_NULL Then
                    If nullTotAdC <> Constant.DB_NULL Then

                        bandTmp = intPowrTx - intAFSLtx + intAGain - chanStruct.patloss + vicAGain - totAdiscC - vicAFSLrx

                        chanStruct.calcico = chanStruct.vicpwrrx - bandTmp
                        chanNulls(TtChan.CALCICO) = Constant.DB_NOT_NULL
                    End If
                    If nullTotAdX <> Constant.DB_NULL Then
                        bandTmp = intPowrTx - intAFSLtx + intAGain - chanStruct.patloss + vicAGain - totAdiscX - vicAFSLrx

                        chanStruct.calcixp = chanStruct.vicpwrrx - bandTmp
                        chanNulls(TtChan.CALCIXP) = Constant.DB_NOT_NULL
                    End If
                End If

                '...Log2.v("\nttChanCalcs(): 15");

                If copolar = Constant.TRUE Then
                    If chanNulls(TtChan.CALCICO) <> Constant.DB_NULL Then
                        chanStruct.resti = chanStruct.calcico - chanStruct.reqdcalc
                        chanNulls(TtChan.RESTI) = Constant.DB_NOT_NULL
                    End If
                Else
                    If chanNulls(TtChan.CALCIXP) <> Constant.DB_NULL Then
                        chanStruct.resti = chanStruct.calcixp - chanStruct.reqdcalc
                        chanNulls(TtChan.RESTI) = Constant.DB_NOT_NULL
                    End If
                End If


                If chanNulls(TtChan.RESTI) <> Constant.DB_NULL Then
                    chanStruct.report = If(chanStruct.resti < parmStruct.margin, Constant.TRUE, Constant.FALSE)
                End If
            End If

            '...Log2.v("\nttChanCalcs(): 15-A");
            ' **********************************************************************\
            ' 
            '    If we have a hit (report is true) and the selection for loss
            '    calculation was '5' (for OH-loss), then we run the actual OH-Loss
            '    calculation.  We have previously run this as a type '3' (FSL) and
            '    are only concerned if it fails this. (Task 1050)
            ' 
            ' \***********************************************************************
            If chanStruct.report = Constant.TRUE AndAlso Strings.FirstCharIs(parmStruct.spherecalc, "5"c) Then
                '...Log2.v("\nttChanCalcs(): 15-B");
                ' This was OH-Loss, First we recalculate the path loss using the
                '    oh-loss routines. 
                If TpRunTsip.TtCalcs.TtPathLoss(siteStruct, anteStruct, chanStruct, "5"c, dActualDist) Then
                Else
                    TpRunTsip.TpRunTsip.mTW_ERR.Write("Pathloss calculation failed in OH-Loss on {0} into {1}" & Microsoft.VisualBasic.Constants.vbLf & "Result: {2}, using Freespace loss." & Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf, siteStruct.intcall1, siteStruct.viccall1, chanStruct.ohresult)
                End If

                '...Log2.v("\nttChanCalcs(): 16");

                chanNulls(TtChan.PATLOSS) = Constant.DB_NOT_NULL

                ' The structure here is a bit different, there are three lines to
                '    display 
                If TpRunTsip.TtCalcs.ecalctype = Enums.Ecalctype.eMINUSI Then
                    ' Get the I value in calcico and calcixp.
                    '    First get the common terms 
                    chanStruct.calcico80 = CSharpImpl.__Assign(chanStruct.calcico99, CSharpImpl.__Assign(chanStruct.calcico, intPowrTx + intAGain - intAFSLtx + vicAGain - vicAFSLrx))
                    chanStruct.calcico -= chanStruct.patloss
                    chanStruct.calcixp = chanStruct.calcico - totAdiscX
                    chanStruct.calcico -= totAdiscC
                    ' ****  80% of the time at the 95% CF *****
                    chanStruct.calcico80 -= chanStruct.pathloss80
                    chanStruct.calcixp80 = chanStruct.calcico80 - totAdiscX
                    chanStruct.calcico80 -= totAdiscC
                    ' ****  99.99% of the time at the 95% CF *****
                    chanStruct.calcico99 -= chanStruct.pathloss99
                    chanStruct.calcixp99 = chanStruct.calcico99 - totAdiscX
                    chanStruct.calcico99 -= totAdiscC

                    ' Derive the Required values from the FSL value 
                    chanStruct.reqd80 = chanStruct.reqdcalc
                    chanStruct.reqd99 = chanStruct.reqdcalc + 10.0

                    ' Calculate the margin. 
                    If copolar = Constant.TRUE Then
                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcico
                        chanStruct.resti80 = chanStruct.reqd80 - chanStruct.calcico80
                        chanStruct.resti99 = chanStruct.reqd99 - chanStruct.calcico99
                    Else
                        chanStruct.resti = chanStruct.reqdcalc - chanStruct.calcixp
                        chanStruct.resti80 = chanStruct.reqd80 - chanStruct.calcixp80
                        chanStruct.resti99 = chanStruct.reqd99 - chanStruct.calcixp99
                    End If
                Else
                    ' This is a C/I ctx curve calculate the margins
                    '    First get the common terms 
                    chanStruct.calcico80 = CSharpImpl.__Assign(chanStruct.calcico99, CSharpImpl.__Assign(chanStruct.calcico, chanStruct.vicpwrrx - (intPowrTx + intAGain - intAFSLtx + vicAGain - vicAFSLrx)))
                    ' For C/I we subtract the path loss and the antenna discrimination
                    '    from the denominator (i.e. Add it to the result) 
                    chanStruct.calcico += chanStruct.patloss
                    chanStruct.calcixp = chanStruct.calcico + totAdiscX
                    chanStruct.calcico += totAdiscC
                    ' ****  80% of the time at the 95% CF *****
                    chanStruct.calcico80 += chanStruct.pathloss80
                    chanStruct.calcixp80 = chanStruct.calcico80 + totAdiscX
                    chanStruct.calcico80 += totAdiscC
                    ' ****  99.99% of the time at the 95% CF *****
                    chanStruct.calcico99 += chanStruct.pathloss99
                    chanStruct.calcixp99 = chanStruct.calcico99 + totAdiscX
                    chanStruct.calcico99 += totAdiscC

                    ' Derive the Required values from the FSL value 
                    chanStruct.reqd80 = chanStruct.reqdcalc
                    chanStruct.reqd99 = chanStruct.reqdcalc - 10.0

                    ' Calculate the margin. 
                    If copolar = Constant.TRUE Then
                        chanStruct.resti = chanStruct.calcico - chanStruct.reqdcalc
                        chanStruct.resti80 = chanStruct.calcico80 - chanStruct.reqd80
                        chanStruct.resti99 = chanStruct.calcico99 - chanStruct.reqd99
                    Else
                        chanStruct.resti = chanStruct.calcixp - chanStruct.reqdcalc
                        chanStruct.resti80 = chanStruct.calcixp80 - chanStruct.reqd80
                        chanStruct.resti99 = chanStruct.calcixp99 - chanStruct.reqd99
                    End If
                End If ' -I or C/I check 

                ' Decide if we have a case 
                If chanStruct.resti < parmStruct.margin OrElse chanStruct.resti80 < parmStruct.margin OrElse chanStruct.resti99 < parmStruct.margin Then
                    chanStruct.report = Constant.TRUE
                Else
                    chanStruct.report = Constant.TRUE ' Turn the report off 
                End If
                ' Now set the not null flags for ingres 
                chanNulls(TtChan.CALCICO) = Constant.DB_NOT_NULL
                chanNulls(TtChan.PATHLOSS80) = Constant.DB_NOT_NULL
                chanNulls(TtChan.PATHLOSS99) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CALCICO80) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CALCICO99) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CALCIXP) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CALCIXP80) = Constant.DB_NOT_NULL
                chanNulls(TtChan.CALCIXP99) = Constant.DB_NOT_NULL
                chanNulls(TtChan.REQD80) = Constant.DB_NOT_NULL
                chanNulls(TtChan.REQD99) = Constant.DB_NOT_NULL
                chanNulls(TtChan.RESTI) = Constant.DB_NOT_NULL
                chanNulls(TtChan.RESTI80) = Constant.DB_NOT_NULL
                chanNulls(TtChan.RESTI99) = Constant.DB_NOT_NULL
                chanNulls(TtChan.REPORT) = Constant.DB_NOT_NULL
                chanNulls(TtChan.OHRESULT) = Constant.DB_NOT_NULL

            End If

            '...Log2.v("\nTtCalcs.TtChanCalcs(): Exit, final");
            Return Constant.SUCCESS
        End Function

        'END

        ''' <summary>
        ''' Calculates the interferer's and victim's antenna 
        ''' gains including the special case of an antenna being  a 
        ''' passive reflector.  
        ''' </summary>
        ''' <paramname="chanStruct"> - TtChan object describing a channel.</param>
        ''' <paramname="anteStruct"> - TtAnte object describing an antenna.</param>
        ''' <paramname="siteStruct"> - TtSite object describing a site.</param>
        ''' <paramname="parmStruct"> - TpParm object providing parameter data.</param>
        ''' <paramname="interfererGain"> - the calculated gain of the interfer's antenna.</param>
        ''' <paramname="victimGain"> - the calculated gain of the victim's antenna.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function CalcGains(chanStruct As TtChan, anteStruct As TtAnte, siteStruct As TtSite, parmStruct As TpParm, <Out> ByRef interfererGain As Double, <Out> ByRef victimGain As Double, intPrintMsg As String, vicPrintMsg As String) As Integer
            ' 'out' requirements.
            interfererGain = 0.0
            victimGain = 0.0

            Dim acode As String

            Dim intTableName As String
            Dim vicTableName As String
            Dim rc As Integer
            Dim isMDB As Boolean
            Dim junk As Double

            ' Subsidiary antenna structure 
            Dim pSubAnt As SuAntStr


            TpSub.TtTableName(siteStruct.interferer, parmStruct, intTableName, vicTableName, isMDB)

            If Strings.FirstCharIs(anteStruct.intcall1, "%"c) Then
                ' use interfer passive reflector gain 
                If Strings.FirstCharIs(siteStruct.interferer, "P"c) Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, False, intTableName, anteStruct.intcall1, anteStruct.intcall2, anteStruct.intbndcde, anteStruct.intanum, chanStruct.intfreqtx, siteStruct.intoffax, junk, interfererGain, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, isMDB, intTableName, anteStruct.intcall1, anteStruct.intcall2, anteStruct.intbndcde, anteStruct.intanum, chanStruct.intfreqtx, siteStruct.intoffax, junk, interfererGain, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in Int Passive Calc. for {0}:" & Microsoft.VisualBasic.Constants.vbLf & "Int: {1}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {2}" & Microsoft.VisualBasic.Constants.vbLf, anteStruct.intcall1, intPrintMsg, vicPrintMsg)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.CalcGains(): ERROR: call to TtCalcPassive() failed: " & str)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    Return rc
                End If
            Else
                ' calc interfer antenna gain 
                acode = anteStruct.intacode
                ' get antenna gain from SDB for interferer 
                ' use the subupt routine, hopefully the antenna is in the cache 
                If CSharpImpl.__Assign(rc, Suutils.SuGetAnt(acode, pSubAnt)) <> 0 Then
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "*ERROR* Can't get interferer Antenna Code [{0}], reason {1}." & Microsoft.VisualBasic.Constants.vbLf, acode, rc)
                    Return Constant.FAILURE
                End If

                interfererGain = pSubAnt.acAnt.again
            End If

            If Strings.FirstCharIs(anteStruct.viccall1, "%"c) Then
                ' use victim passive reflector gain 
                If Strings.FirstCharIs(siteStruct.interferer, "P"c) Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, isMDB, vicTableName, anteStruct.viccall1, anteStruct.viccall2, anteStruct.vicbndcde, anteStruct.vicanum, chanStruct.vicfreqrx, siteStruct.vicoffax, junk, victimGain, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, False, vicTableName, anteStruct.viccall1, anteStruct.viccall2, anteStruct.vicbndcde, anteStruct.vicanum, chanStruct.vicfreqrx, siteStruct.vicoffax, junk, victimGain, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "***Error in Vic Passive Calc for {0}:-" & Microsoft.VisualBasic.Constants.vbLf & "Int: {1}" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {2}" & Microsoft.VisualBasic.Constants.vbLf, anteStruct.viccall1, intPrintMsg, vicPrintMsg)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.CalcGains(): ERROR: call to TtCalcPassive() failed: " & str)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    Return rc
                End If
            Else
                acode = anteStruct.vicacode
                If CSharpImpl.__Assign(rc, Suutils.SuGetAnt(acode, pSubAnt)) <> 0 Then
                    Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "*ERROR* Can't get victim Antenna Code [{0}], reason {1}." & Microsoft.VisualBasic.Constants.vbLf, acode, rc)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtCalcs.CalcGains(): ERROR: call to SuGetAnt() failed: " & str)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                    Return Constant.FAILURE
                End If

                victimGain = pSubAnt.acAnt.again
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method performs the Antenna Tilt modification and returns the calculated
        ''' output discrimination.
        ''' </summary>
        ''' <remarks>
        ''' <listtype="bullet">
        ''' <item>For the given 
        ''' inteferer and victim ground height, and their distance, the elevation is 
        ''' calculated between the two sites.</item> 
        ''' <item>Using the elevation, we calculate the 
        ''' tilt angle of the antenna, and look this up in the sd_antd table along with 
        ''' the antenna code provided as the first argument.</item> 
        ''' <item>The dtilt value derived 
        ''' from that table is then added to the current total antenna discrimination.</item> 
        ''' <item>The program looks first for the temporary antenna table and tries here 
        ''' first, and then it tries the sd_antd table if there is no temporary table 
        ''' or the query fails.</item>
        ''' </list>
        ''' </remarks>
        ''' <paramname="acode"> - antenna code</param>
        ''' <paramname="Xgrnd"> - X ground height in meters.</param>
        ''' <paramname="Xht"> - X antenna height in meters.</param>
        ''' <paramname="Ygrnd"> - Y ground height in meters.</param>
        ''' <paramname="Yht"> - Y antenna height in meters.</param>
        ''' <paramname="XYdist"> - distance between X and Y in meters.</param>
        ''' <paramname="disc"> - calculated output discrimination.</param>
        ''' <returns></returns>
        Public Shared Function TtPcsTilt(acode As String, Xgrnd As Double, Xht As Double, Ygrnd As Double, Yht As Double, XYdist As Double, <Out> ByRef disc As Double) As Integer     ' 	Antenna code 
            ' 	X Ground in m. 
            ' 	X Antenna Height 
            ' 	Y Ground 
            ' 	Y Antenna Height 
            ' 	Distance 
            ' 	Output discrimination 
            ' 'out' requirement.
            disc = 0.0

            Dim tiltang As Single

            Dim elevXY, elevYX As Double
            Dim rc = -1
            Dim pstrAntenna As SuAntStr = Nothing
            Dim strDisc As SuAntd

            If CSharpImpl.__Assign(rc, Suutils.SuGetAnt(acode, pstrAntenna)) >= 0 Then  ' Read in the antenna 
                ' Check that this antenna has discrimination entries 
                If pstrAntenna.acAnt.anip > 0 Then
                    ' Get the elevation angle 
                    AxSub3.AxElev((Xgrnd + Xht) / 1000.0, (Ygrnd + Yht) / 1000.0, XYdist, elevXY, elevYX)
                    If elevXY >= 0 Then
                        tiltang = 0
                    Else
                        tiltang = CSng(-elevXY)
                    End If
                    ' This will return the interpolated value 
                    rc = Suutils.InterpADisc(pstrAntenna.acDscPtr, tiltang, pstrAntenna.acAnt.anip, strDisc)

                    If rc >= 0 Then
                        disc = strDisc.dtilt
                    Else
                        disc = 0.0
                    End If
                Else
                    ' This antenna has no discrimination curves (like a passive) 
                    disc = 0.0
                    rc = 0
                End If
            Else
                ' Error in reading in the antenna 
                disc = 0.0
            End If

            Return rc
        End Function

        ''' <summary>
        ''' Calculates propagation path loss and saves the results in the
        ''' TtChan object. Any one of four distinct path loss models can be applied,
        ''' as prescribed by the User in the input parameter file.  
        ''' </summary>
        ''' <remarks>
        ''' Five path loss models are currently available:
        ''' <listtype="bullet">
        ''' <item>the standard CCIR-SJM method;  (ID '1');</item>
        ''' <item>the Spherical Earth model (ID '2');</item>
        ''' <item>the Free Space model (ID '3'); and</item>
        ''' <item>the PCS Extension to Hata ModelCOST-231 model (ID '4').</item> 
        ''' <item>the OH-Loss model provided by CTE (ID '5').</item> 
        ''' </list>
        ''' The height of the interfering and victim antennas are obtained, and the model is selected. 
        ''' For the PCS model, the heights are matched up with the PCS or MW antenna by checking 
        ''' which one has a traffic code beginning with "PC". 
        ''' <para>With the exception of calcPatLoss, 
        ''' which does its own internal conversion, the frequencies are converted from KHz into 
        ''' MHz by dividing them by 1000 as they are passed in to the functions.</para>
        ''' </remarks>
        ''' <paramname="siteStruct"> - TtSite object.</param>
        ''' <paramname="anteStruct"> - TtAnte object</param>
        ''' <paramname="chanStruct"> - TtChan object.</param>
        ''' <paramname="spherecalc"> - character ID code of the path loss model to be used (see remarks).</param>
        ''' <paramname="dDistUsed"> - the actual distance used.</param>
        ''' <returns></returns>
        Public Shared Function TtPathLoss(siteStruct As TtSite, anteStruct As TtAnte, chanStruct As TtChan, spherecalc As Char, <Out> ByRef dDistUsed As Double) As Boolean   ' Site structure      	
            ' Antenna structure   	
            ' current channel data	
            ' prop. loss model    	
            ' The actual dist used 
            '...Log2.v("\nTtCalcs.TtPathLoss(): Entry");

            Dim pcsHeight As Double
            Dim mwHeight As Double
            Dim txHeight As Double
            Dim rxHeight As Double
            Dim dActualDistKm As Double

            Dim sctOHLoss As OhLossXfer  ' Structure for the OH Loss transfer 
            ' The following are used in the OH-Loss calculations to convert NAD 27
            '    (used by MICS) to NAD 83 used by the OHLoss database 

            txHeight = anteStruct.intaht
            rxHeight = anteStruct.vicaht
            ' ************************************************************************\
            ' 
            ' 		Instead of the surface distance between two sites, we use the path
            ' 		distance.  1179 - GJS - 2004.08.24
            ' 
            ' \*************************************************************************
            dActualDistKm = AxSub3.PathDist((txHeight + siteStruct.intgrnd) / 1000.0, (rxHeight + siteStruct.vicgrnd) / 1000.0, siteStruct.int1vic1dist)
            '...Log2.v("\nTtCalcs.TtPathLoss(): A");

            dDistUsed = dActualDistKm

            Select Case spherecalc
                Case "1"c
                    '...Log2.v("\nTtCalcs.TtPathLoss(): B");
                    GenUtil.CalcPatLoss(dActualDistKm, chanStruct.intfreqtx, chanStruct.patloss)

                Case "2"c
                    '...Log2.v("\nTtCalcs.TtPathLoss(): C");
                    txHeight += siteStruct.intgrnd
                    rxHeight += siteStruct.vicgrnd

                    TpRunTsip.TtCalcs.SphericalPathLoss(rxHeight, txHeight, dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)

                Case "3"c
                    '...Log2.v("\nTtCalcs.TtPathLoss(): D");
                    GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)

                Case "4"c
                    '...Log2.v("\nTtCalcs.TtPathLoss(): E");
                    If Not chanStruct.inttraftx.StartsWith("PS") Then
                        '...Log2.v("\nTtCalcs.TtPathLoss(): F");
                        ' 
                        '    If the first two letters in inttraftx are "PS", then
                        '    the PCS antenna is the interferer and the MW antenna is
                        '    the victim.
                        ' 
                        pcsHeight = txHeight + siteStruct.intgrnd
                        mwHeight = rxHeight + siteStruct.vicgrnd

                        TpRunTsip.TtCalcs.PCSPropLoss(pcsHeight, mwHeight, dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)
                    ElseIf Not chanStruct.victrafrx.StartsWith("PS") Then
                        '...Log2.v("\nTtCalcs.TtPathLoss(): G");
                        ' 
                        '  Otherwise, the first two letters in victraftx are "PS"
                        '  and the PCS antenna is the victim and the MW antenna is
                        '  the interferer.
                        ' 
                        pcsHeight = rxHeight + siteStruct.vicgrnd
                        mwHeight = txHeight + siteStruct.intgrnd

                        TpRunTsip.TtCalcs.PCSPropLoss(pcsHeight, mwHeight, dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)
                    Else
                        '...Log2.v("\nTtCalcs.TtPathLoss(): H");
                        GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)
                    End If

                Case "5"c
                    ' OH-Loss model, using CTE's path profile program. 
                    '...Log2.v("\nTtCalcs.TtPathLoss(): I");
                    ' Store the FSL for the display 
                    GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)

                    chanStruct.pathloss80 = CSharpImpl.__Assign(chanStruct.pathloss99, chanStruct.patloss)

                    ' If the interferer and victim are colocated, the skip this
                    '    processing 
                    If siteStruct.int1vic1dist <= 0.001 Then
                        '...Log2.v("\nTtCalcs.TtPathLoss(): J");
                        ' if the sites are co-located, we can't just set the distance to
                        '    .001 or something, because we are dealing with coordinates, so
                        '    we set the type of result to colocation and the loss to zero by
                        '    not changing the initial value at all. 
                        chanStruct.ohresult = 100
                    Else
                        '...Log2.v("\nTtCalcs.TtPathLoss(): K");
                        ' Now set up the OHLoss structure for the routine 
                        sctOHLoss = New OhLossXfer()

                        sctOHLoss.lat_1 = LatLong.FsecsToDeg(siteStruct.intlatit)
                        sctOHLoss.lng_1 = LatLong.FsecsToDeg(siteStruct.intlongit)
                        sctOHLoss.lat_2 = LatLong.FsecsToDeg(siteStruct.viclatit)
                        sctOHLoss.lng_2 = LatLong.FsecsToDeg(siteStruct.viclongit)

                        ' a zero antenna height will break the ohloss routine 
                        If anteStruct.intaht > 0.0 Then
                            sctOHLoss.s1_anthght = anteStruct.intaht
                        Else
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "** WARNING ** {0}-{1}: Interfering antenna height of zero. 10m used." & Microsoft.VisualBasic.Constants.vbLf, siteStruct.intcall1, siteStruct.intname1)
                            sctOHLoss.s1_anthght = 10.0
                        End If

                        If anteStruct.vicaht > 0.0 Then
                            sctOHLoss.s2_anthght = anteStruct.vicaht
                        Else
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "** WARNING ** {0}-{1}: Victim antenna height of zero. 10m used." & Microsoft.VisualBasic.Constants.vbLf, siteStruct.viccall1, siteStruct.vicname1)
                            sctOHLoss.s2_anthght = 10.0
                        End If

                        sctOHLoss.freq = chanStruct.intfreqtx / 1000.0

                        If chanStruct.intpolar.Equals("H") Then
                            sctOHLoss.polarization = 0             ' Horizontal polarization 
                        Else
                            sctOHLoss.polarization = 1             ' Anything else we assume Vertical 
                        End If

                        sctOHLoss.clim_region = 0         ' Continental temperate 
                        sctOHLoss.K_median = 1.333333  ' K  

                        '...Log2.v("\nTtCalcs.TtPathLoss(): L");
                        CTEfunctions.Calc_OhLoss(sctOHLoss)
                        '...Log2.v("\nTtCalcs.TtPathLoss(): L-1");

                        If sctOHLoss.error_status = Enums.PRF.OK Then

                            chanStruct.pathloss80 += sctOHLoss.ohloss_95(1)
                            chanStruct.pathloss99 += sctOHLoss.ohloss_95(5)
                            chanStruct.ohresult = sctOHLoss.calc_type

                            dDistUsed = sctOHLoss.eff_dist        ' 	The distance used is the
                            ' 					                                    *		effective distance      
                            '	Tell the report that 50,000:1 maps used, without changing the calling
                            '	sequence or table structure.
                            If sctOHLoss.cded50_files_used Then
                                chanStruct.ohresult += 1000
                            End If
                        ElseIf sctOHLoss.error_status = Enums.PRF.NOMAP Then
                            '...Log2.v("\nTtCalcs.TtPathLoss(): M");
                            ' 	No map was present.  This should not be fatal.  Print an error
                            ' 		message and do free space loss 
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "** WARNING ** Map ({0}) for path from %s to %s " & Microsoft.VisualBasic.Constants.vbLf & "              is missing. Using FSL." & Microsoft.VisualBasic.Constants.vbLf, sctOHLoss.map_name, siteStruct.intcall1, siteStruct.viccall1)

                            GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)

                            chanStruct.pathloss80 += chanStruct.patloss
                            chanStruct.pathloss99 += chanStruct.patloss
                            chanStruct.ohresult = -1   ' 	Non-terminal error 

                            Return False
                        Else
                            '...Log2.v("\nTtCalcs.TtPathLoss(): N");
                            ' Store the actual error in the ohresult field.  Use free space
                            ' 		loss. 
                            GenUtil.FreeSpacePathLoss(dActualDistKm, chanStruct.intfreqtx / 1000, chanStruct.patloss)

                            chanStruct.pathloss80 += chanStruct.patloss
                            chanStruct.pathloss99 += chanStruct.patloss
                            chanStruct.ohresult = -sctOHLoss.error_status

                            Return False
                        End If
                    End If

                Case Else
                    Return False
            End Select

            '...Log2.v("\nTtCalcs.TtPathLoss(): Exit");
            Return True
        End Function

        ''' <summary>
        ''' This method performs a Ts-Ts propagation path loss calculation 
        ''' i.a.w. the Spherical Earth model.  
        ''' </summary>
        ''' <paramname="rxHeight"> - recieve antenna's height.</param>
        ''' <paramname="txHeight"> - transmit antenna's height.</param>
        ''' <paramname="plength"> - distance between recieve and transmit antennae.</param>
        ''' <paramname="freq"> - transmission frequency.</param>
        ''' <paramname="ploss"> - calculated path loss.</param>
        Public Shared Sub SphericalPathLoss(rxHeight As Double, txHeight As Double, plength As Double, freq As Double, <Out> ByRef ploss As Double)
            Dim transDist As Double                   ' Transition distance in km   
            Dim theta, H, lH, N, FSL As Double ' Temporary variables for calculation 

            transDist = 4.123 * (Sqrt(rxHeight) + Sqrt(txHeight))
            FSL = 32.45 + 20.0 * Log10(plength) + 20.0 * Log10(freq)

            If plength > transDist Then
                theta = (plength - transDist) / 8.5
                H = theta * plength / 4000
                lH = 1.063 * (theta * theta) * .001
                N = 20.0 * Log10(5.0 + .27 * H) + 1.17261 * lH
                ploss = 29.73 + 30.0 * Log10(freq) + 10.0 * Log10(plength) + 30.0 * Log10(theta) + N
                If ploss < FSL Then ploss = FSL
            Else
                ploss = FSL
            End If
        End Sub

        ''' <summary>
        ''' Calculates and returns the propagation path loss i.a.w. the PCS Extension
        ''' to the Hata ModelCOST-231 Model. 
        ''' </summary>
        ''' <remarks>
        ''' Note the use of explicit multiplications instead of using the math library pow() 
        ''' function; early, and even recent, versions of the UNIX, Linux and VC++ standard
        ''' math library had problems with the pow(x, n) function when n was an integer and
        ''' especially for n= 2.
        ''' </remarks>
        ''' <paramname="pcsHeight"> - TBD.</param>
        ''' <paramname="mwHeight"> - TBD.</param>
        ''' <paramname="plength"> - TBD.</param>
        ''' <paramname="freq"> - operating frequency.</param>
        ''' <paramname="pLoss"> - calculated path loss.</param>
        Public Shared Sub PCSPropLoss(pcsHeight As Double, mwHeight As Double, plength As Double, freq As Double, <Out> ByRef pLoss As Double)
            ' 'out' requirement.
            pLoss = 0.0

            Dim transDist, theta, H, lH, N, FSL, alpha As Double                   ' Transition distance in km	
            ' Temporary variables for calculation 

            transDist = 4.123 * (Sqrt(pcsHeight) + Sqrt(mwHeight))
            FSL = 32.45 + 20.0 * Log10(plength) + 20.0 * Log10(freq)

            If plength > transDist Then
                ' 
                '  propagation Loss: pLoss
                '  (Hourly median transmission loss 50% of the time)
                ' 
                theta = (plength - transDist) / 8.5    ' in milliradians	
                H = theta * plength / 4000
                lH = 1.063 * theta * theta * .001
                N = 20.0 * Log10(5 + .27 * H) + 1.17261 * lH
                pLoss = 29.73 + 30.0 * Log10(freq) + 10.0 * Log10(plength) + 30.0 * Log10(theta) + N
                If pLoss < FSL Then pLoss = FSL
            Else
                If pcsHeight > 60.0 Then
                    pLoss = FSL
                Else
                    If pcsHeight <= 9.0 Then
                        alpha = (1.1 * Log10(freq) - 0.7) * pcsHeight - (1.56 * Log10(freq) - 0.8)
                    ElseIf pcsHeight <= 28.0 Then
                        alpha = 2.68 * pcsHeight - 3.53 - 0.1017 * pcsHeight * pcsHeight + .00152 * pcsHeight * pcsHeight * pcsHeight
                    Else
                        alpha = 25.49 + 19.92 * Log10(pcsHeight / 28.0)
                    End If
                    pLoss = 69.55 + 26.16 * Log10(freq) - 13.82 * Log10(mwHeight) + (44.9 - 6.55 * Log10(mwHeight)) * Log10(plength) - 2.0 * Log10(freq / 28) * Log10(freq / 28) - 11.4 - alpha
                End If
            End If
        End Sub

        ''' <summary>
        ''' This method retrieves remote site data (for a victim site) from the DB. 
        ''' </summary>
        ''' <paramname="chanStruct"> - TtChan object.</param>
        ''' <paramname="anteStruct"> - TtAnte object.</param>
        ''' <paramname="siteStruct"> - TtSite object.</param>
        ''' <paramname="parmStruct"> - TpParm object providing parameter data.</param>
        ''' <paramname="vicTableName"> - unique part of victim table name.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="vicMbnd"> - victim's mid-band frequency.</param>
        ''' <paramname="viclnkAFSLtx"> - victim's antenna feed system loss.</param>
        ''' <paramname="viclnkANumTx"> - victim's lik antenna number.</param>
        ''' <paramname="viclnkPowTx"> - victim's link transmit power.</param>
        ''' <paramname="viclnkAntGain"> - victim's link antenna gain.</param>
        ''' <paramname="nullViclnkAfslTx"> - ODBC nullInds associated with viclnkAFSLtx.</param>
        ''' <paramname="nullAntNumTx"> - ODBC nullInds associated with viclnkANumTx.</param>
        ''' <paramname="nullPowTx"> - ODBC nullInds associated with viclnkPowTx.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function GetVicLnkInfo(chanStruct As TtChan, anteStruct As TtAnte, siteStruct As TtSite, parmStruct As TpParm, vicTableName As String, isMDB As Boolean, vicMbnd As Double, ByRef viclnkAFSLtx As Double, ByRef viclnkANumTx As Short, ByRef viclnkPowTx As Double, ByRef viclnkAntGain As Double, ByRef nullViclnkAfslTx As SQLLEN, ByRef nullAntNumTx As SQLLEN, ByRef nullPowTx As SQLLEN, intPrintMsg As String, vicPrintMsg As String) As Integer
            Dim tmpACode As String
            Dim pAnt As SuAntStr

            Dim rc As Integer
            Dim viclnkChanNulls As SQLLEN()  '[FtChan.SIZE_];
            Dim tmpAnteNulls As SQLLEN()  '[FtAnte.SIZE_];
            Dim ftTmpChan As FtChan
            Dim ftTmpAnte As FtAnte
            Dim junk As Double
            Dim [select] = ""
            Dim vicChanTable As String
            Dim vicAnteTable As String

            GenUtil.UtCvtName(Constant.FT_CHAN, vicTableName, vicChanTable)
            GenUtil.UtCvtName(Constant.FT_ANTE, vicTableName, vicAnteTable)

            [select] = String.Format("call2='{0}' and call1='{1}' and bndcde='{2}' and chid='{3}'", chanStruct.viccall2, chanStruct.viccall1, chanStruct.vicbndcde, chanStruct.vicchid)
            If Strings.FirstCharIs(siteStruct.interferer, "E"c) Then
                rc = TpRunTsip.TpMdbPdfGet.TtChanGet(chanStruct.viccall2, chanStruct.viccall1, chanStruct.vicbndcde, chanStruct.vicchid, vicChanTable, False, ftTmpChan, viclnkChanNulls)
            Else
                rc = TpRunTsip.TpMdbPdfGet.TtChanGet(chanStruct.viccall2, chanStruct.viccall1, chanStruct.vicbndcde, chanStruct.vicchid, vicChanTable, isMDB, ftTmpChan, viclnkChanNulls)
            End If
            If rc <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "CHAN", [select])
                If rc = Constant.FAILURE Then
                    If Strings.FirstCharIs(siteStruct.interferer, "E"c) Then
                        ' if we looked for the remote channel in the PDF and didn't find it this is
                        '  simply a warning - ie don;t return a hard and fast stop 
                        Return Constant.SUCCESS
                    End If
                End If
                Return rc
            End If

            If Strings.FirstCharIs(chanStruct.viccall2, "%"c) Then
                viclnkAFSLtx = 0.0
                nullViclnkAfslTx = Constant.DB_NOT_NULL
                nullAntNumTx = Constant.DB_NULL
            Else
                If chanStruct.txant = 1 Then
                    viclnkAFSLtx = ftTmpChan.afsltx1
                    nullViclnkAfslTx = viclnkChanNulls(FtChan.AFSLTX1)
                    viclnkANumTx = ftTmpChan.antnumbtx1
                    nullAntNumTx = viclnkChanNulls(FtChan.ANTNUMBTX1)
                Else
                    viclnkAFSLtx = ftTmpChan.afsltx2
                    nullViclnkAfslTx = viclnkChanNulls(FtChan.AFSLTX2)
                    viclnkANumTx = ftTmpChan.antnumbtx2
                    nullAntNumTx = viclnkChanNulls(FtChan.ANTNUMBTX2)
                End If
            End If
            viclnkPowTx = ftTmpChan.pwrtx
            nullPowTx = viclnkChanNulls(FtChan.PWRTX)


            If nullAntNumTx <> Constant.DB_NULL Then
                ' need to get the victim link's antenna information 
                [select] = String.Format("call1 = '{0,-9:S}' and call2 = '{1,-9:S}' and bndcde = '{2,-4:S}' and anum = {3}", anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde, viclnkANumTx)

                If Strings.FirstCharIs(siteStruct.interferer, "E"c) Then
                    rc = TpRunTsip.TpMdbPdfGet.TtAnteGet(anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde, viclnkANumTx, vicAnteTable, False, ftTmpAnte, tmpAnteNulls)
                Else
                    rc = TpRunTsip.TpMdbPdfGet.TtAnteGet(anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde, viclnkANumTx, vicAnteTable, isMDB, ftTmpAnte, tmpAnteNulls)
                End If
                If rc <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])

                    If rc = Constant.FAILURE Then
                        If Strings.FirstCharIs(siteStruct.interferer, "E"c) Then
                            ' can't find remote antenna in the PDF return a success message
                            '  rather than an error	 
                            Return Constant.SUCCESS
                        End If
                    End If
                    Return rc
                End If

                ' record found for this antenna - store info 
                tmpACode = ftTmpAnte.acode

                [select] = String.Format("acode = '%-12s'", tmpACode)

                If CSharpImpl.__Assign(rc, Suutils.SuGetAnt(tmpACode, pAnt)) <> 0 Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", tmpACode)
                    If rc <> Constant.NOMORERECS Then
                        Return [Error].DYN_MS_SQL_SERVER_ERR
                    Else
                        Return Constant.FAILURE
                    End If
                End If

                viclnkAntGain = pAnt.acAnt.again

                rc = Constant.SUCCESS
            ElseIf Strings.FirstCharIs(chanStruct.viccall2, "%"c) Then
                ' 	Other end is a passive.  
                ' calculate victim's passive reflector and discrimination at call2 
                If Strings.FirstCharIs(siteStruct.interferer, "P"c) Then
                    ' anteStruct.vicanum 
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, isMDB, vicTableName, anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde, 0, vicMbnd, siteStruct.vicoffax, junk, viclnkAntGain, intPrintMsg, vicPrintMsg)
                Else
                    ' anteStruct.vicanum 
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, False, vicTableName, anteStruct.viccall2, anteStruct.viccall1, anteStruct.vicbndcde, 0, vicMbnd, siteStruct.vicoffax, junk, viclnkAntGain, intPrintMsg, vicPrintMsg)
                End If
                If rc = Constant.SUCCESS Then
                    nullAntNumTx = Constant.DB_NOT_NULL
                End If
            End If
            Return rc
        End Function

        ''' <summary>
        ''' This method identifies extraneous point-to-multipoint cases 
        ''' that do not need to be reported.  
        ''' </summary>
        ''' <paramname="chanStruct"> - TtChan object to be vetted.</param>
        ''' <returns>
        ''' Either:
        ''' <listtype="bullet">
        ''' <item>Error.CONTINUE; or</item>
        ''' <item>Constant.PC_SKIP.</item>
        ''' </list>
        ''' </returns>
        Public Shared Function MultiPointCalcs(chanStruct As TtChan) As Integer
            If Not chanStruct.inttraftx.StartsWith("PM") AndAlso Not chanStruct.inttraftx.StartsWith("PS") Then
                Return [Error].CONTINUE

            ElseIf Not chanStruct.inttraftx.Equals(chanStruct.victrafrx) Then
                Return [Error].CONTINUE

            ElseIf chanStruct.intfreqtx <> chanStruct.vicfreqrx Then
                Return [Error].CONTINUE

            ElseIf chanStruct.intcall2.Equals(chanStruct.viccall1) OrElse chanStruct.intcall1.Equals(chanStruct.viccall2) Then
                Return Constant.PC_SKIP
            Else
                Return [Error].CONTINUE
            End If
        End Function

        ''' <summary>
        ''' Calculates the total discrimination for the given 
        ''' antenna.  
        ''' </summary>
        ''' <paramname="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        ''' <paramname="chanStruct"> - TtChan object.</param>
        ''' <paramname="anteStruct"> - TtAnte object.</param>
        ''' <paramname="totAdiscX"> - TBD.</param>
        ''' <paramname="totAdiscC"> - TBD.</param>
        ''' <paramname="copolar"> - TBD.</param>
        ''' <paramname="nullTotAdX"> - ODBC nullInd associated with totAdiscX.</param>
        ''' <paramname="totAdisc"> - calculated total antenna discrimination.</param>
        ''' <returns></returns>
        Public Shared Function CalcTotADisc(anteNulls As SQLLEN(), chanStruct As TtChan, anteStruct As TtAnte, ByRef totAdiscX As Double, ByRef totAdiscC As Double, ByRef copolar As Short, ByRef nullTotAdX As SQLLEN, ByRef totAdisc As Double) As Integer
            Dim horizCopolar = 0.0
            Dim vertCopolar = 0.0
            Dim hvXpolar1 = 0.0
            Dim hvXpolar2 = 0.0
            Dim vhXpolar1 = 0.0
            Dim vhXpolar2 = 0.0
            Dim tmpTotAdiscX = 0.0
            Dim nullHorizCo As SQLLEN
            Dim nullVertCo As SQLLEN
            Dim nullTotAdC As SQLLEN = Constant.DB_NOT_NULL
            Dim tempNull As SQLLEN = 0
            Dim rc As Integer
            Dim cIntPol As String
            Dim cVicPol As String

            ' 
            ' 		We handle circular polarizations like B for now.  So we check for the
            ' 		circular polarization and convert it to B if found.
            ' 
            TpRunTsip.TtCalcs.ConvertPol(cIntPol, chanStruct.intpolar)
            TpRunTsip.TtCalcs.ConvertPol(cVicPol, chanStruct.vicpolar)

            If anteNulls(TtAnte.ADISCCTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXH) <> Constant.DB_NULL Then
                horizCopolar = anteStruct.adiscctxh + anteStruct.adisccrxh
                nullHorizCo = Constant.DB_NOT_NULL
            Else
                nullHorizCo = Constant.DB_NULL
            End If
            If anteNulls(TtAnte.ADISCCTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXV) <> Constant.DB_NULL Then
                vertCopolar = anteStruct.adiscctxv + anteStruct.adisccrxv
                nullVertCo = Constant.DB_NOT_NULL
            Else
                nullVertCo = Constant.DB_NULL
            End If

            ' ------------------------*
            '  for intpolar is H or B *
            ' ------------------------
            If Strings.FirstCharIs(cIntPol, "H"c) OrElse Strings.FirstCharIs(cIntPol, "B"c) Then
                nullTotAdX = Constant.DB_NOT_NULL
                If anteNulls(TtAnte.ADISCXTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXV) <> Constant.DB_NULL Then
                    hvXpolar1 = anteStruct.adiscxtxh + anteStruct.adisccrxv
                    If anteNulls(TtAnte.ADISCCTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCXRXV) <> Constant.DB_NULL Then
                        hvXpolar2 = anteStruct.adiscctxh + anteStruct.adiscxrxv
                        totAdiscX = If(hvXpolar1 < hvXpolar2, hvXpolar1, hvXpolar2)
                    Else
                        totAdiscX = hvXpolar1
                    End If
                Else
                    If anteNulls(TtAnte.ADISCCTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCXRXV) <> Constant.DB_NULL Then
                        totAdiscX = anteStruct.adiscctxh + anteStruct.adiscxrxv
                    Else
                        nullTotAdX = Constant.DB_NULL
                    End If
                End If
                If Strings.FirstCharIs(cIntPol, "B"c) Then
                    tmpTotAdiscX = totAdiscX
                    tempNull = nullTotAdX
                End If
            End If

            ' ------------------------*
            '  for intpolar is V or B *
            ' ------------------------
            If Strings.FirstCharIs(cIntPol, "V"c) OrElse Strings.FirstCharIs(cIntPol, "B"c) Then
                nullTotAdX = Constant.DB_NOT_NULL
                If anteNulls(TtAnte.ADISCXTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXH) <> Constant.DB_NULL Then
                    vhXpolar1 = anteStruct.adiscxtxv + anteStruct.adisccrxh
                    If anteNulls(TtAnte.ADISCCTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCXRXH) <> Constant.DB_NULL Then
                        vhXpolar2 = anteStruct.adiscctxv + anteStruct.adiscxrxh
                        totAdiscX = If(vhXpolar1 < vhXpolar2, vhXpolar1, vhXpolar2)
                    Else
                        totAdiscX = vhXpolar1
                    End If
                Else
                    If anteNulls(TtAnte.ADISCCTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCXRXH) <> Constant.DB_NULL Then
                        totAdiscX = anteStruct.adiscctxv + anteStruct.adiscxrxh
                    Else
                        nullTotAdX = Constant.DB_NULL
                    End If
                End If
                If Strings.FirstCharIs(cIntPol, "B"c) Then
                    If nullTotAdX = Constant.DB_NULL Then
                        nullTotAdX = tempNull
                        totAdiscX = tmpTotAdiscX
                    ElseIf tempNull <> Constant.DB_NULL Then
                        totAdiscX = If(tmpTotAdiscX < totAdiscX, tmpTotAdiscX, totAdiscX)
                    End If
                End If
            End If

            ' ----------------------------------------------------
            If Strings.FirstCharIs(cIntPol, "H"c) Then
                If Strings.FirstCharIs(cVicPol, "H"c) Then
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfSingleDirAnt(nullHorizCo, horizCopolar, Constant.TRUE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                ElseIf Strings.FirstCharIs(cVicPol, "V"c) Then
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfSingleDirAnt(nullTotAdX, totAdiscX, Constant.FALSE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If   ' chanStruct.vicpolar ==  "B" 
                Else
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfMultiDirAnt(nullTotAdX, nullHorizCo, totAdiscX, horizCopolar, Constant.FALSE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                End If
                If nullHorizCo = Constant.DB_NULL Then
                    nullTotAdC = Constant.DB_NULL
                Else
                    totAdiscC = horizCopolar
                End If
            ElseIf Strings.FirstCharIs(cIntPol, "V"c) Then
                If Strings.FirstCharIs(cVicPol, "H"c) Then
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfSingleDirAnt(nullTotAdX, totAdiscX, Constant.FALSE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                ElseIf Strings.FirstCharIs(cVicPol, "V"c) Then
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfSingleDirAnt(nullVertCo, vertCopolar, Constant.TRUE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If   ' chanStruct.vicpolar ==  "B" 
                Else
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfMultiDirAnt(nullTotAdX, nullVertCo, totAdiscX, vertCopolar, Constant.FALSE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                End If
                If nullVertCo = Constant.DB_NULL Then
                    nullTotAdC = Constant.DB_NULL
                Else
                    totAdiscC = vertCopolar
                End If   ' chanStruct.intpolar == "B"  or circular 
            Else
                ' 
                '  TASK 504: Added the second part of the following conditions, dealing
                '  with the victim polarization. Previously, this section of the code seemed
                '  to assume that the victim polarization was 'B'.
                ' 
                If nullHorizCo = Constant.DB_NULL OrElse Strings.FirstCharIs(cVicPol, "V"c) Then
                    If nullVertCo = Constant.DB_NULL Then
                        nullTotAdC = Constant.DB_NULL
                    Else
                        totAdiscC = vertCopolar
                    End If
                Else
                    If nullVertCo = Constant.DB_NULL OrElse Strings.FirstCharIs(cVicPol, "H"c) Then
                        totAdiscC = horizCopolar
                    Else
                        totAdiscC = If(horizCopolar < vertCopolar, horizCopolar, vertCopolar)
                    End If
                End If
                If Strings.FirstCharIs(cVicPol, "H"c) Then
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfMultiDirAnt(nullTotAdX, nullHorizCo, totAdiscX, horizCopolar, Constant.FALSE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                ElseIf Strings.FirstCharIs(cVicPol, "V"c) Then
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfMultiDirAnt(nullTotAdX, nullVertCo, totAdiscX, vertCopolar, Constant.FALSE, totAdisc, copolar)) <> Constant.SUCCESS Then
                        Return rc
                    End If   ' chanStruct.vicpolar ==  "B" or circular 
                Else
                    If nullTotAdX = Constant.DB_NULL Then
                        If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfSingleDirAnt(nullTotAdC, totAdiscC, Constant.TRUE, totAdisc, copolar)) <> Constant.SUCCESS Then
                            Return rc
                        End If
                    Else
                        If nullTotAdC = Constant.DB_NULL Then
                            totAdisc = totAdiscX
                            copolar = Constant.FALSE
                        Else
                            If totAdiscC < totAdiscX Then
                                totAdisc = totAdiscC
                                copolar = Constant.TRUE
                            Else
                                totAdisc = totAdiscX
                                copolar = Constant.FALSE
                            End If
                        End If
                    End If
                End If
            End If
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method checks an input polarization code for circular polarization and, if found,
        ''' converts/outputs it as "B"; otherwise the output polarization code is the same as the input.
        ''' </summary>
        ''' <paramname="outpol"> - output polarization code.</param>
        ''' <paramname="inpol"> - input polarization code.</param>
        Public Shared Sub ConvertPol(<Out> ByRef outpol As String, inpol As String)
            ' 'out' requirement.
            outpol = ""

            If Not "CLR".Contains(inpol.ToUpper()(0)) Then
                '	The input is not circular polarized
                outpol = inpol.ToUpper()
            Else
                outpol = "B"
            End If

            Return
        End Sub

        ''' <summary>
        ''' This method determines the total antenna 
        ''' discrimination between two single directional antennae. The orientation of 
        ''' the two antennae may be copolar or crosspolar.  
        ''' </summary>
        ''' <paramname="nullDisc"> - ODBC nullInd associated with disc.</param>
        ''' <paramname="disc"> - calculated discrimination.</param>
        ''' <paramname="samePolar"> - flag denoting the orientation.</param>
        ''' <paramname="totantdisc"> - resulting discrimination</param>
        ''' <paramname="copolar"> - resulting orientation.</param>
        ''' <returns></returns>
        Public Shared Function DiscOfSingleDirAnt(nullDisc As SQLLEN, disc As Double, samePolar As Short, <Out> ByRef totantdisc As Double, <Out> ByRef copolar As Short) As Integer  ' null value of disc 
            ' calc'd discrimination 
            ' flag for orientation 
            ' resulting discrim 
            ' resulting orientation 
            ' 'out' requirements.
            totantdisc = 0.0
            copolar = -666

            If nullDisc = Constant.DB_NULL Then
                Return [Error].SINGLEDIR_ADISC
            End If

            totantdisc = disc
            copolar = samePolar

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method determines the total antenna 
        ''' discrimination between one single and one multi-directional antenna. The 
        ''' orientation of the two antenna is also determined to be copolar or 
        ''' crosspolar.  
        ''' </summary>
        ''' <paramname="nullXpDisc"> - ODBC nullInd associated with discXp.</param>
        ''' <paramname="nullCpDisc"> - ODBC nullInd associated with discCo.</param>
        ''' <paramname="discXp"> - calculated discrimination of crosspolar orientation.</param>
        ''' <paramname="discCo"> - calculated discrimination of copolar orientation</param>
        ''' <paramname="samePolar"> - flag indicating the xpolar orientation.</param>
        ''' <paramname="totantdisc"> - resulting total antenna discrimination.</param>
        ''' <paramname="copolar"> - flag indicating the resulting orientation.</param>
        ''' <returns></returns>
        Public Shared Function DiscOfMultiDirAnt(nullXpDisc As SQLLEN, nullCpDisc As SQLLEN, discXp As Double, discCo As Double, samePolar As Short, <Out> ByRef totantdisc As Double, <Out> ByRef copolar As Short) As Integer ' null value of crosspolar antenna disc 
            ' null value of copolar antenna disc 
            ' calc'd discrimination of cross polar orientation 
            ' calc'd discrimination of co polar orientation 
            ' flag for xpolar orientation
            ' resulting discrim 
            ' resulting orientation 
            ' 'out' requirements.
            totantdisc = 0.0
            copolar = -666

            Dim rc As Integer

            If nullCpDisc = Constant.DB_NULL Then
                If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.DiscOfSingleDirAnt(nullXpDisc, discXp, samePolar, totantdisc, copolar)) <> Constant.SUCCESS Then
                    Return rc
                End If
            Else
                If nullXpDisc = Constant.DB_NULL Then
                    totantdisc = discCo
                    copolar = Constant.TRUE
                Else
                    If discCo < discXp Then
                        totantdisc = discCo
                        copolar = Constant.TRUE
                    Else
                        totantdisc = discXp
                        copolar = Constant.FALSE
                    End If
                End If
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
