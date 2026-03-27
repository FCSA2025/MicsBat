Imports _Configuration
Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports System
Imports System.Math
Imports System.Collections.Generic
Imports _NewLib.Maths
Imports _Auxlib
Imports System.Runtime.InteropServices
Imports SQLLEN = System.Int64

Namespace TpRunTsip
    Public Class TeSubCalc
#If PINVOKE
        [DllImport("tpruntsip.dll", CharSet = CharSet.Ansi)]
        private extern static int teCalcScang([In] TpParm parmStruct,
                                                [In, Out] TeSite siteStruct,
                                                [In, Out] TeAnte anteStruct,
                                                [In, Out] TeChan chanStruct,
                                                [In, Out] SQLLEN[] chanNulls,
                                                [In] double earthAGain,
                                                [In] double terrAGain,
                                                [In] double earthAfslt,
                                                [In] SQLLEN nullEarthAfslt,
                                                [In] double earthAfslr,
                                                [In] SQLLEN nullEarthAfslr,
                                                [In] double terrAfsl,
                                                [In, Out] ref double loss01mode2,
                                                [In, Out] ref SQLLEN nullL01M2,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);
        public static int TeCalcScang_NATIVE(TpParm parmStruct,
                                ref TeSite siteStruct,
                                ref TeAnte anteStruct,
                                ref TeChan chanStruct,
                                ref SQLLEN[] chanNulls,
                                double earthAGain,
                                double terrAGain,
                                double earthAfslt,
                                SQLLEN nullEarthAfslt,
                                double earthAfslr,
                                SQLLEN nullEarthAfslr,
                                double terrAfsl,
                                out double loss01mode2,
                                out SQLLEN nullL01M2,
                                string intPrintMsg,
                                string vicPrintMsg)
        {
            // 'out' requirements.
            loss01mode2 = 0.0;
            nullL01M2 = Constant.DB_NULL;

            int retVal;

            retVal = teCalcScang(parmStruct, siteStruct, anteStruct, chanStruct, chanNulls,
                                    earthAGain, terrAGain, earthAfslt, nullEarthAfslt,
                                    earthAfslr, nullEarthAfslr, terrAfsl,
                                    ref loss01mode2, ref nullL01M2,
                                    intPrintMsg, vicPrintMsg);



            return retVal;
        }
#End If
        ' Switched when the message about the beam being below 0 is printed.
        Public Shared IsLowElevMessToPrint As Boolean = True
        'Same thing, western end.
        Public Shared IsDroppedBelowToPrint As Boolean = True

        Private Shared sf As ScanFormatted = New ScanFormatted()

        '----------------------------------------------------------------------
        ''' <summary>
        ''' This method generates the Terrestrial and Earth station table names based 
        ''' on the users input and tableType (ie. user entered ENV file is MDB_TS then 
        ''' the TS site table name is 'mt_site').  
        ''' </summary>
        ''' <paramname="parmStruct"></param>
        ''' <paramname="tabType"></param>
        ''' <paramname="terrTableName"></param>
        ''' <paramname="earthTableName"></param>
        ''' <paramname="azimTableName"></param>
        ''' <paramname="terrMDB"></param>
        ''' <paramname="earthMDB"></param>
        Public Shared Sub TeTableNames(parmStruct As TpParm, tabType As Integer, <Out> ByRef terrTableName As String, <Out> ByRef earthTableName As String, <Out> ByRef azimTableName As String, <Out> ByRef terrMDB As Boolean, <Out> ByRef earthMDB As Boolean)
            '...Log2.v("\nTeSubCalc.TeTableNames(): Entry");

            ' 'out' requirement.
            terrTableName = ""
            earthTableName = ""
            azimTableName = ""
            terrMDB = False
            earthMDB = False

            Dim esType As Integer

            parmStruct.envtype.Trim()
            If tabType = Constant.FT_SITE Then
                esType = Constant.FE_SITE
            ElseIf tabType = Constant.FT_ANTE Then
                esType = Constant.FE_ANTE
            Else
                esType = Constant.FE_CHAN
            End If

            If Strings.FirstCharIs(parmStruct.protype, "T"c) Then
                GenUtil.UtCvtName(tabType, parmStruct.proname, terrTableName)
                If parmStruct.envtype.Equals("PDF_ES") Then
                    GenUtil.UtCvtName(esType, parmStruct.envname, earthTableName)
                    GenUtil.UtCvtName(Constant.FE_AZIM, parmStruct.envname, azimTableName)   ' envtype = "MDB_ES" 
                Else
                    earthMDB = True
                    azimTableName = "me_azim"
                    If esType = Constant.FE_SITE Then
                        earthTableName = "me_site"
                    ElseIf esType = Constant.FE_ANTE Then
                        earthTableName = "me_ante"
                    Else
                        earthTableName = "me_chan"
                    End If
                End If   ' protype == 'E' 
            Else
                GenUtil.UtCvtName(esType, parmStruct.proname, earthTableName)
                GenUtil.UtCvtName(Constant.FE_AZIM, parmStruct.proname, azimTableName)
                If parmStruct.envtype.Equals("PDF_TS") Then
                    GenUtil.UtCvtName(tabType, parmStruct.envname, terrTableName)   ' envtype = "MDB_TS" 
                Else
                    terrMDB = True
                    If tabType = Constant.FT_SITE Then
                        terrTableName = "mt_site"
                    ElseIf tabType = Constant.FT_ANTE Then
                        terrTableName = "mt_ante"
                    Else
                        terrTableName = "mt_chan"
                    End If
                End If
            End If

            Dim str = String.Format("{0}  {1}  {2}  {3}  {4}", terrTableName, earthTableName, azimTableName, terrMDB, earthMDB)

            '...Log2.v("\nTeSubCalc.TeTableNames(): Exit: " + str);
        End Sub

        ''' <summary>
        ''' This method checks the horizon about an antenna and stores the azimuth 
        ''' elevation and distance to the point on that horizon where the azimuth is at 
        ''' its minimum point.  
        ''' </summary>
        ''' <paramname="select"></param>
        ''' <paramname="isMDB"></param>
        ''' <paramname="azimTabName"></param>
        ''' <paramname="dist"></param>
        ''' <paramname="azim"></param>
        ''' <paramname="elev"></param>
        ''' <paramname="intPrintMsg"></param>
        ''' <paramname="vicPrintMsg"></param>
        ''' <returns></returns>
        Public Shared Function TeCheckRadioHoriz([select] As String, isMDB As Boolean, azimTabName As String, dist As Double, azim As Double, ByRef elev As Double, intPrintMsg As String, vicPrintMsg As String) As Integer
            Dim tmpAzimHandle, rc As Integer
            Dim tmpmeAzimNulls As SQLLEN()  '[ME_AZIM_SIZE_];
            Dim tmpfeAzimNulls As SQLLEN()  '[FE_AZIM_SIZE_];

            Dim tempSel As String
            Dim savAzim, savElev, savDist, tableElev, tableDist As Single
            Dim feTmpAzim As FeAzim
            Dim meTmpAzim As MeAzim


            If isMDB = True Then
                If CSharpImpl.__Assign(tmpAzimHandle, DynMeAzim.MeSelectAzim([select], "azim")) < 0 Then
                    Return tmpAzimHandle
                End If
                If CSharpImpl.__Assign(rc, DynMeAzim.MeFetchAzim(tmpAzimHandle, meTmpAzim, tmpmeAzimNulls)) = Constant.SUCCESS Then
                    If meTmpAzim.azim > azim Then
                        ' 	Don't print the error message. Just set the default value. 
                        ' ErrMsg.UtPrintMessage(GENERROR, intPrintMsg);
                        ' ErrMsg.UtPrintMessage(GENERROR, vicPrintMsg);
                        ' ErrMsg.UtPrintMessage(AZIMDEFAULT, azimTabName, select);
                        ' 
                        tableElev = 0.0F
                        tableDist = 1.0F
                    ElseIf meTmpAzim.azim = azim Then
                        tableElev = CSng(meTmpAzim.elev)
                        tableDist = CSng(meTmpAzim.dist)
                    Else
                        savAzim = CSng(meTmpAzim.azim)
                        savElev = CSng(meTmpAzim.elev)
                        savDist = CSng(meTmpAzim.dist)

                        While CSharpImpl.__Assign(rc, DynMeAzim.MeFetchAzim(tmpAzimHandle, meTmpAzim, tmpmeAzimNulls)) = Constant.SUCCESS
                            If meTmpAzim.azim < azim Then
                                savAzim = CSng(meTmpAzim.azim)
                                savElev = CSng(meTmpAzim.elev)
                                savDist = CSng(meTmpAzim.dist)
                            Else
                                Exit While
                            End If
                        End While

                        GenUtil.Interp(CSng(azim), savAzim, CSng(meTmpAzim.azim), savDist, CSng(meTmpAzim.dist), tableDist)

                        GenUtil.Interp(CSng(azim), savAzim, CSng(meTmpAzim.azim), savElev, CSng(meTmpAzim.elev), tableElev)
                    End If
                ElseIf rc = Constant.NOMORERECS Then
                    ' 	Don't print the error message, just use default value 
                    ' ErrMsg.UtPrintMessage(GENERROR, intPrintMsg);
                    ' ErrMsg.UtPrintMessage(GENERROR, vicPrintMsg);
                    ' ErrMsg.UtPrintMessage(AZIMDEFAULT, azimTabName, select);
                    ' 
                    tableElev = 0.0F
                    tableDist = 1.0F
                Else
                    Return rc
                End If

                DynMeAzim.MeCloseAzim(tmpAzimHandle)
            Else
                tempSel = String.Format("{0} and cmd != 'D'", [select])

                If CSharpImpl.__Assign(tmpAzimHandle, DynFeAzim.FeSelectAzim(azimTabName, tempSel, "azim")) < 0 Then
                    Return tmpAzimHandle
                End If

                If CSharpImpl.__Assign(rc, DynFeAzim.FeFetchAzim(tmpAzimHandle, feTmpAzim, tmpfeAzimNulls)) = Constant.SUCCESS Then
                    If feTmpAzim.azim > azim Then
                        ' 	Don't print the error message.
                        ' ErrMsg.UtPrintMessage(GENERROR, intPrintMsg);
                        ' ErrMsg.UtPrintMessage(GENERROR, vicPrintMsg);
                        ' ErrMsg.UtPrintMessage(AZIMDEFAULT,azimTabName,tempSel);
                        ' 
                        tableElev = 0.0F
                        tableDist = 1.0F
                    ElseIf feTmpAzim.azim = azim Then
                        tableElev = CSng(feTmpAzim.elev)
                        tableDist = CSng(feTmpAzim.dist)
                    Else
                        savAzim = CSng(feTmpAzim.azim)
                        savElev = CSng(feTmpAzim.elev)
                        savDist = CSng(feTmpAzim.dist)

                        While CSharpImpl.__Assign(rc, DynFeAzim.FeFetchAzim(tmpAzimHandle, feTmpAzim, tmpfeAzimNulls)) = Constant.SUCCESS
                            If feTmpAzim.azim < azim Then
                                savAzim = CSng(feTmpAzim.azim)
                                savElev = CSng(feTmpAzim.elev)
                                savDist = CSng(feTmpAzim.dist)
                            Else
                                Exit While
                            End If
                        End While

                        GenUtil.Interp(CSng(azim), savAzim, CSng(feTmpAzim.azim), savDist, CSng(feTmpAzim.dist), tableDist)

                        GenUtil.Interp(CSng(azim), savAzim, CSng(feTmpAzim.azim), savElev, CSng(feTmpAzim.elev), tableElev)
                    End If
                ElseIf rc = Constant.NOMORERECS Then
                    tableElev = 0.0F
                    tableDist = 1.0F
                Else
                    Return rc
                End If

                DynFeAzim.FeCloseAzim(tmpAzimHandle)
            End If

            If tableDist < dist AndAlso tableElev > elev Then
                elev = tableElev
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' Module Global.  
        ''' </summary>
        ''' <paramname="siteStruct"> - struct of Es site info</param>
        ''' <paramname="isMDB"> - is ES from the MDB flag</param>
        ''' <paramname="anteStruct"> - struct of ES ante info</param>
        ''' <paramname="tableName"> - name of ES table</param>
        ''' <paramname="stnStruct"> - AUX type ES station struct</param>
        ''' <returns></returns>
        Public Shared Function TeFillEsStnStr(siteStruct As TeSite, isMDB As Boolean, anteStruct As TeAnte, tableName As String, <Out> ByRef stnStruct As AxStation) As Integer  ' struct of Es site info 
            ' is ES from the MDB flag 
            ' struct of ES ante info 
            ' name of ES table 
            ' AUX type ES station struct 
            ' 'out' requirement.
            stnStruct = New AxStation()

            Dim [select] As String
            Dim rc As Integer
            Dim tmpAnteNulls As SQLLEN()  '[FE_ANTE_SIZE_];
            Dim feTmpAnte As FeAnte

            stnStruct.elevM = siteStruct.earthgrnd
            stnStruct.name = siteStruct.earthname

            TpSub.TpLoadLat(siteStruct.earthlatit, stnStruct.LL.latSens, stnStruct.LL.latDeg, stnStruct.LL.latMin, stnStruct.LL.latSec)

            TpSub.TpLoadLong(siteStruct.earthlongit, stnStruct.LL.longSens, stnStruct.LL.longDeg, stnStruct.LL.longMin, stnStruct.LL.longSec)

            [select] = String.Format("location = '{0}'", anteStruct.earthlocation)

            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TeAnteGet([select], tableName, isMDB, feTmpAnte, tmpAnteNulls)) <> Constant.SUCCESS Then
                Return rc
            End If

            stnStruct.antHtM = feTmpAnte.aht

            Return Constant.SUCCESS
        End Function

        Private Shared incr As Double = -1.0

        ''' <summary>
        ''' This method calulates the angle SET (satellite, Earth Station, Terrestrial 
        ''' Station.  
        ''' </summary>
        ''' <paramname="es"></param>
        ''' <paramname="anteStruct"> - struct of Es site info</param>
        ''' <paramname="siteStruct"> - struct of Es site info</param>
        ''' <paramname="parmStruct"> - struct of Es site info</param>
        ''' <paramname="anteNulls"></param>
        ''' <paramname="refIndex"></param>
        ''' <paramname="intPrintMsg"></param>
        ''' <paramname="vicPrintMsg"></param>
        ''' <returns></returns>


        Public Shared Function TeCalcSET(es As AxStation, ByRef anteStruct As TeAnte, siteStruct As TeSite, parmStruct As TpParm, ByRef anteNulls As SQLLEN(), refIndex As Double, intPrintMsg As String, vicPrintMsg As String) As Integer   ' struct of Es site info 
            ' struct of Es site info 
            ' struct of Es site info 
            '...Log2.v("\nTeSubCalc.TeSubCalc.TeCalcSET()(): Entry");
            '&&Console.Error.Write("\nTeSubCalc.TeCalcSet():");

            Dim nlng As Double
            Dim tmp As Double
            Dim earthAGain As Double
            Dim tmpEsAzim = 0.0
            Dim tmpEsElev As Double
            Dim tmpSET As Double
            Dim savEsAzim = 0.0
            Dim savEsElev = 0.0
            Dim savSET = 0.0

            Dim minADisc As Single
            Dim tmpMin As Single
            Dim adiscctxv As Single
            Dim adiscxtxv As Single
            Dim adiscctxh As Single
            Dim adiscxtxh As Single

            Dim rc As Integer
            Dim tmpG As Integer
            Dim tmpGain As Integer

            Dim anteTableName As String
            Dim junk As String
            Dim [select] As String

            Dim ste As AxStation

            Dim patDat As PatternStruct = Nothing

            ' double  dArcStep;
            Dim pParms As ParmStrct     ' 	Pointer to the anonymous parm structure. 
            Dim cArcStep As String
            Dim nAboveHorizon = 0  ' 	0 starting, -1 starts below, +1 moved above 

            If TpRunTsip.TeSubCalc.incr < 0 Then
                ' Break out the arc step from the parm field (parmparm) 
                pParms = GenUtil.ParmBreakOut(parmStruct.parmparm)

                cArcStep = GenUtil.ParmByName(pParms, "ARCSTEP")

                If Equals(cArcStep, Nothing) OrElse CSharpImpl.__Assign(TpRunTsip.TeSubCalc.incr, Convert.ToDouble(cArcStep)) <= 0.0 Then
                    ' Default to the normal arcstep 
                    If anteStruct.sarc2 <= 9.5 Then
                        TpRunTsip.TeSubCalc.incr = 1.0
                    Else
                        TpRunTsip.TeSubCalc.incr = anteStruct.sarc2 / 9.5
                    End If
                End If

                TpRunTsip.TpRunTsip.mdArcStep = TpRunTsip.TeSubCalc.incr
                '...Log2.v("\nA: TpRunTsip.mdArcStep = " + TpRunTsip.mdArcStep);
            End If

            '...Log2.v("\nB: TpRunTsip.mdArcStep = " + TpRunTsip.mdArcStep);

            anteTableName = "sd_ante"

            ' get antenna pattern and calculate antenna discriminations for
            '  the earth antenna
            ' 
            '&&Console.Error.Write("\nanteStruct.earthacode = " + anteStruct.earthacode);
            If Not anteStruct.earthacode.StartsWith("CCIR") Then
                If CSharpImpl.__Assign(rc, TpGetDat.TpGetPattern(anteStruct.earthacode, patDat)) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    If rc = [Error].NOANTDFOUND OrElse rc = [Error].NOANTEFOUND Then
                        [select] = String.Format("acode = '{0}'", anteStruct.earthacode)
                        ErrMsg.UtPrintMessage(rc, anteTableName, [select])
                        Return Constant.CONT_PROCESSING
                    Else
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeSubCalc.TeCalcSET(): ERROR: call to TpGetPattern() failed")
                        TpRunTsip.TpRunTsip.mTW_ERR.Write("Error Getting antenna pattern: {0}" & Microsoft.VisualBasic.Constants.vbLf, rc)
                    End If
                    Return rc
                End If
            End If
            tmpMin = CSng(Constant.MAXADISC)

            ste = New AxStation()

            '&&Console.Error.Write("\nteSubCalc.teCalcSet(): baboon: anteStruct->sarc1 = {0}", anteStruct.sarc1);
            '&&Console.Error.Write("\nteSubCalc.teCalcSet(): baboon: anteStruct->sarc2 = {0}", anteStruct.sarc2);
            '&&Console.Error.Write("\nteSubCalc.teCalcSet(): baboon: incr = {0}", incr);
            nlng = anteStruct.sarc1 - anteStruct.sarc2
            While nlng <= anteStruct.sarc1 + anteStruct.sarc2

                ste.LL.longSeconds = nlng * 3600.0

                rc = SatAze.Sataze(es, ste, tmpEsAzim, tmp, tmpEsElev)
                '&&Console.Error.Write("\nteSubCalc.teCalcSet(): turtle: tmpEsAzim = {0}", tmpEsAzim);
                If rc < 0 Then
                    ' 	Error.  Ignore this longitude	
                    TpRunTsip.TpRunTsip.mTW_ERR.Write("Error in calculating Satellite elevation, Long: {0:F2} ({1})" & Microsoft.VisualBasic.Constants.vbLf, nlng, rc)
                    Continue While
                ElseIf rc > 0 Then
                    ' 	Beam is beneath the Horizon 
                    If nAboveHorizon = 0 Then
                        ' 	The beam starts out beneath the level.  Set this to -1 to indicate
                        ' 		this 
                        nAboveHorizon = -1
                        Continue While
                    ElseIf nAboveHorizon = +1 Then
                        ' 	If the beam is now beneath the horizon, and it was above it
                        ' 		we stop. 
                        If TpRunTsip.TeSubCalc.IsDroppedBelowToPrint Then
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "** WARNING ** The service arc has dropped below the level (0.0 elev)." & Microsoft.VisualBasic.Constants.vbLf & "This occurred at Longitude {0:F2} for site {1}." & Microsoft.VisualBasic.Constants.vbLf & "Beams below 0.0 degrees elevation are not processed." & Microsoft.VisualBasic.Constants.vbLf & "-- This message will only appear once. --" & Microsoft.VisualBasic.Constants.vbLf, nlng, anteStruct.earthlocation)
                            TpRunTsip.TeSubCalc.IsDroppedBelowToPrint = False
                        End If
                        Exit While
                    Else
                        ' 	Beam is below the horizon, but has never been above.  
                        Continue While
                    End If
                Else
                    ' 	Beam is above horizon.  Check if it just moved above 
                    If nAboveHorizon = -1 Then
                        If TpRunTsip.TeSubCalc.IsLowElevMessToPrint Then
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "** WARNING ** The service arc starts below the level (0.0 elev)." & Microsoft.VisualBasic.Constants.vbLf & "It first comes above the level at Longitude {0:F2} for site {1}." & Microsoft.VisualBasic.Constants.vbLf & "Beams below 0.0 degrees elevation are not processed." & Microsoft.VisualBasic.Constants.vbLf & "-- This message will only appear once. --" & Microsoft.VisualBasic.Constants.vbLf, nlng, anteStruct.earthlocation)
                            TpRunTsip.TeSubCalc.IsLowElevMessToPrint = False
                        End If
                    End If
                    nAboveHorizon = +1
                End If


                SepAng.AxSepAng(tmpEsAzim, tmpEsElev, siteStruct.etazim, anteStruct.etelev, tmpSET)

                minADisc = CSng(Constant.MAXADISC)
                If Not anteStruct.earthacode.StartsWith("CCIR") Then
                    TpSub.TpFindDisc(patDat.pattern, patDat.numPts, Abs(tmpSET), adiscctxv, adiscxtxv, adiscctxh, adiscxtxh)

                    If patDat.nulls(Constant.DCOV) <> Constant.DB_NULL Then
                        minADisc = If(minADisc < adiscctxv, minADisc, adiscctxv)
                    End If
                    If patDat.nulls(Constant.DXPV) <> Constant.DB_NULL Then
                        minADisc = If(minADisc < adiscxtxv, minADisc, adiscxtxv)
                    End If
                    If patDat.nulls(Constant.DCOH) <> Constant.DB_NULL Then
                        minADisc = If(minADisc < adiscctxh, minADisc, adiscctxh)
                    End If
                    If patDat.nulls(Constant.DXPH) <> Constant.DB_NULL Then
                        minADisc = If(minADisc < adiscxtxh, minADisc, adiscxtxh)
                    End If
                Else
                    If Strings.FirstCharIs(anteStruct.interferer, "E"c) Then
                        junk = anteStruct.earthacode.Substring(0, 4)
                        tmpG = Convert.ToInt32(anteStruct.earthacode.Substring(4, 2))
                        tmpGain = Convert.ToInt32(anteStruct.earthacode.Substring(6, 2))  ' TS-ES 
                    Else
                        ' positions 5&6 are the againRx 
                        junk = anteStruct.earthacode.Substring(0, 4)
                        tmpGain = Convert.ToInt32(anteStruct.earthacode.Substring(4, 2))
                        tmpG = Convert.ToInt32(anteStruct.earthacode.Substring(6, 2))
                    End If
                    earthAGain = tmpGain

                    If Abs(tmpSET) < 1.0 Then
                        minADisc = 0.0F
                    ElseIf Abs(tmpSET) >= 48.0 Then
                        minADisc = CSng(earthAGain + 10.0)
                    Else
                        minADisc = CSng(earthAGain - (32.0 - 25.0 * Log10(tmpSET)))
                    End If
                End If

                If minADisc < tmpMin Then
                    savSET = tmpSET
                    savEsAzim = tmpEsAzim
                    '&&Console.Error.Write("\nteSubCalc.teCalcSET(): goat: savEsAzim = {0}", savEsAzim);
                    savEsElev = tmpEsElev
                    tmpMin = minADisc
                ElseIf minADisc = tmpMin Then
                    tmpMin = minADisc

                    If savSET > tmpSET Then
                        savSET = tmpSET
                        savEsAzim = tmpEsAzim
                        '&&Console.Error.Write("\nteSubCalc.teCalcSET(): pig: savEsAzim = {0}", savEsAzim);
                        savEsElev = tmpEsElev
                    End If
                End If
                nlng += TpRunTsip.TeSubCalc.incr
            End While

            If tmpMin = Constant.MAXADISC Then
                anteNulls(TeAnte.EDISCANG) = Constant.DB_NULL
                anteNulls(TeAnte.ESAZIM) = Constant.DB_NULL
                anteNulls(TeAnte.ESELEV) = Constant.DB_NULL
                anteNulls(TeAnte.ADISC_SET) = Constant.DB_NULL
            Else
                anteStruct.ediscang = savSET
                anteStruct.esazim = savEsAzim
                '&&Console.Error.Write("\nteSubCalc.teCalcSET(): anteStruct.esazim = {0}", anteStruct.esazim);
                anteStruct.eselev = savEsElev
                anteStruct.adisc_set = tmpMin

                anteNulls(TeAnte.EDISCANG) = Constant.DB_NOT_NULL
                anteNulls(TeAnte.ESAZIM) = Constant.DB_NOT_NULL
                anteNulls(TeAnte.ESELEV) = Constant.DB_NOT_NULL
                anteNulls(TeAnte.ADISC_SET) = Constant.DB_NOT_NULL
            End If

            '...Log2.v("\nTeSubCalc.TeCalcSET(): Exit");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method performs rain calculations for the given site and antenna. The 
        ''' calculated values are as follows: evdistes - earth to rain volume dist on 
        ''' the ES vector tvdistes - terr. to rain volume dist on the ES vector tvazim 
        ''' - TS to rain volume azimuth tvelev - TS to rain volume elevation angleutv - 
        ''' angle UTV (remote TS, TS, rain volume) tvdisttu - TS to rain volume on the 
        ''' TU vector evdisttu - ES to rain volume on the TU vector evazim - earth to 
        ''' rain volume azimuth evelev - earth to rain volume elevation anglesev - 
        ''' angle SEV (satellite, earth, rain volume).  
        ''' </summary>
        ''' <paramname="anteStruct"> - struct of ES info</param>
        ''' <paramname="anteNulls"></param>
        ''' <paramname="siteStruct"> - struct of ES info</param>
        ''' <paramname="tuVec"> - Geometric T to U vect</param>
        Public Shared Sub TeAnteRainCalcs(ByRef anteStruct As TeAnte, ByRef anteNulls As SQLLEN(), siteStruct As TeSite, tuVec As Double())    ' struct of ES info 
            ' struct of ES info 
            ' Geometric T to U vect 
            Dim esVec = New Double(2) {}
            Dim teVec = New Double(2) {}
            Dim etVec = New Double(2) {}
            Dim euVec = New Double(2) {}
            Dim tu1Vec = New Double(2) {}
            Dim tvVec = New Double(2) {}
            Dim evVec = New Double(2) {}

            ' 	Offaxis antenna vector - GJS - 1108 - 2002.12 
            Dim taVec = New Double(2) {}

            Dim evAngVec = New Double(2) {}
            Dim lambda As Double
            Dim tmpVal As Double
            Dim HR As Double
            Dim tuDist As Double
            Dim tuElev As Double
            Dim nu As Double
            Dim RAD As Double
            Dim ettu1 As Double
            Dim eset As Double
            Dim et2 As Double
            Dim estu1 As Double
            Dim esDist As Double
            Dim h As Double
            Dim az As Double
            Dim ez As Double
            Dim te2 As Double
            Dim tu1es As Double
            Dim tees As Double
            Dim tu1te As Double
            Dim tu2 As Double
            Dim tute As Double
            Dim tvAngVec = New Double(2) {}
            Dim cosalp As Double
            Dim cosbet As Double

            RAD = 3.1415926536 / 180.0

            HR = TpRunTsip.TeSubCalc.CalcHR(siteStruct.earthlatit / 100.0)

            ' 	Build the geometry using SEZ vectors around the ES site 
            ' 	esVec is the unit vector from the ES site to the satellite 
            TpRunTsip.TeVects.TeBuildVector(1.0, anteStruct.eselev, anteStruct.esazim, esVec)
            ' 	etVec is the vector from the es site to the ts site 
            TpRunTsip.TeVects.TeBuildVector(siteStruct.etdist, anteStruct.etelev, siteStruct.etazim, etVec)
            ' 	euVec is the vector from the es site to the u site (u is other end of the
            ' 		ts link 
            TpRunTsip.TeVects.TeBuildVector(siteStruct.eudist, anteStruct.euelev, siteStruct.euazim, euVec)
            ' 	tuVec is the vector (SEZ in the es frame of reference) from the ts site
            ' 		to u. fed in because we need the geometric vector here, not the refracted
            ' 		vector or one using the radio horizon.  GJS - 1108 - 2003.01
            ' teVectorSub(euVec, etVec, tuVec); 
            TpRunTsip.TeVects.TeVectorLen(tuVec, tuDist)
            ' 	and tu1Vec is the unit vector from ts to u 
            TpRunTsip.TeVects.TeVectorUnit(tuVec, tu1Vec)

            tuElev = AtanD(tu1Vec(2) / Sqrt(1.0 - Pow(tu1Vec(2), 2.0)))

            If Strings.FirstCharIs(anteStruct.tsoffaxis, "Y"c) Then
                ' 	Off axis angle.  Create the unit vector along antenna boresight
                ' 		Note that we are creating this vector in the SEZ coordination system
                ' 		of the ES station, although the arguments are for the SEZ coordinates
                ' 		at the Terrestrial site.  It is felt that within 200km or so, this will
                ' 		not give material error.  GJS - 1108 - 2003.01 
                TpRunTsip.TeVects.TeBuildVector(1.0, anteStruct.tstrueel, anteStruct.tstrueaz, taVec)
            End If

            ' Rain Scatter Cell on ES vector 

            ' 	ettu1 is the dot product of es to ts on the ts to u unit vector. 
            TpRunTsip.TeVects.TeVectorLinMux(etVec, tu1Vec, ettu1)
            ' 	eset is the dot product of earth to sat on the earth to terrestrial vec.
            TpRunTsip.TeVects.TeVectorLinMux(esVec, etVec, eset)
            ' 	et2 is the square of the length of the earth to terrestrial vector 
            TpRunTsip.TeVects.TeVectorLinMux(etVec, etVec, et2)
            ' 	estu1 is the dot prod of the earth-sat vec on the terr. link vect. 
            TpRunTsip.TeVects.TeVectorLinMux(esVec, tu1Vec, estu1)

            ' 	lambda is the distance along the es vector that we have a rain cell.
            ' 		reference needed on this, as it is different from the Engineering specs 
            lambda = (-ettu1 * eset + et2 * estu1) / (-ettu1 + estu1 * eset)
            lambda = If(lambda < 0.1, 0.1, lambda)

            TpRunTsip.TeVects.TeVectorLen(esVec, esDist)
            anteStruct.evdistes = lambda * esDist
            anteNulls(TeAnte.EVDISTES) = Constant.DB_NOT_NULL

            ' 	evdistes is the distance along the es to Sat vector that the rain
            ' 		cell is found.  If this is above the maximum height (HR), then it is set
            ' 		to the distance to the maximum height 
            h = anteStruct.evdistes * Maths.SinD(anteStruct.eselev)
            If h > HR Then
                anteStruct.evdistes = HR / Maths.SinD(anteStruct.eselev)
                lambda = anteStruct.evdistes
            End If

            ' 	tvVec becomes the vector from the ts site to the rain volume 
            TpRunTsip.TeVects.TeVectorSclMux(lambda, esVec, tvVec)

            ' 	calculated form the es-V - es-TS vectors 
            TpRunTsip.TeVects.TeVectorSub(tvVec, etVec, tvVec)

            ' 	Get the distance (tvdistes) ts to volume 

            'AH: REMOVE
            Dim filter As Boolean = anteStruct.terrcall1.Equals("CHX576")
            filter = filter And anteStruct.terrcall2.Equals("VEL885")
            If filter Then
                Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "FILTER-alpha: {0}  {1}  {2}  {3}", anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.tvdistes, anteNulls(TeAnte.TVDISTES))
                '...Log2.v(str);
            End If

            TpRunTsip.TeVects.TeVectorLen(tvVec, anteStruct.tvdistes)
            anteNulls(TeAnte.TVDISTES) = Constant.DB_NOT_NULL

            'AH: REMOVE
            filter = anteStruct.terrcall1.Equals("CHX576")
            filter = filter And anteStruct.terrcall2.Equals("VEL885")
            If filter Then
                Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "FILTER-beta: {0}  {1}  {2}  {3}", anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.tvdistes, anteNulls(TeAnte.TVDISTES))
                '...Log2.v(str);
            End If

            ' 	Get the angles of the TV vector. 
            tvAngVec(0) = Abs(Atan(tvVec(1) / tvVec(0))) ' 0 is angle from South 
            tvAngVec(1) = Abs(Atan(tvVec(0) / tvVec(1))) ' 	1 is angle from East 
            ' 	2 is elevation angle.  All wrt the SEZ at the ES site 
            tvAngVec(2) = Math.Abs(Math.Atan(tvVec(2) / anteStruct.tvdistes / Math.Pow(1.0 - Math.Pow(tvVec(2) / anteStruct.tvdistes, 2.0), 0.5)))

            az = tvAngVec(0) / RAD

            If tvVec(0) >= 0.0 Then
                If tvVec(1) >= 0.0 Then
                    anteStruct.tvazim = 180.0 - az
                End If
            ElseIf tvVec(1) >= 0.0 Then
                anteStruct.tvazim = az
            Else
                anteStruct.tvazim = 360.0 - az
            End If
            anteNulls(TeAnte.TVAZIM) = Constant.DB_NOT_NULL

            anteStruct.tvelev = tvAngVec(2) / RAD
            anteNulls(TeAnte.TVELEV) = Constant.DB_NOT_NULL

            ' 	Why this is done, rather than remove the fabs call above,
            ' 		is not clear and might be historical 
            If tvVec(2) < 0.0 Then
                anteStruct.tvelev = -anteStruct.tvelev
            End If

            ' 	Get the projection of the Terrestrial to link vector on the terr. to
            ' 		volume vector 
            TpRunTsip.TeVects.TeVectorLinMux(tuVec, tvVec, tmpVal)
            ' 	cosalp is the distance along the TU vector as if both were unit vectors 
            cosalp = tmpVal / (tuDist * anteStruct.tvdistes)

            ' 	calculate utv from one side of a right unit triangle 
            anteStruct.angleutv = 90.0 - AtanD(cosalp / Pow(1.0 - Pow(cosalp, 2.0), 0.5))
            anteNulls(TeAnte.ANGLEUTV) = Constant.DB_NOT_NULL


            TpRunTsip.TeVects.TeVectorSclMux(-1.0, etVec, teVec) ' 	Get ready for the next section, but
            ' 																			*		this is needed for offaxis angles 

            If Strings.FirstCharIs(anteStruct.tsoffaxis, "Y"c) Then
                ' 	The antenna at the TS site is an offaxis angle antenna.
                ' 		Calculate the angles for its true azimuth (A) 
                anteStruct.angleuta = TpRunTsip.TeVects.Vangle(tuVec, taVec)
                anteNulls(TeAnte.ANGLEUTA) = Constant.DB_NOT_NULL

                anteStruct.angleeta = TpRunTsip.TeVects.Vangle(teVec, taVec)
                anteNulls(TeAnte.ANGLEETA) = Constant.DB_NOT_NULL

                anteStruct.angleatv = TpRunTsip.TeVects.Vangle(tvVec, taVec)
                anteNulls(TeAnte.ANGLEATV) = Constant.DB_NOT_NULL
            End If


            ' 	Rain Scatter Cell on TU vector 
            ' 	Exactly the same logic as above, on the TU vector, mutatis mutandi 

            TpRunTsip.TeVects.TeVectorLinMux(teVec, teVec, te2)
            TpRunTsip.TeVects.TeVectorLinMux(tu1Vec, esVec, tu1es)
            TpRunTsip.TeVects.TeVectorLinMux(teVec, esVec, tees)
            TpRunTsip.TeVects.TeVectorLinMux(tu1Vec, teVec, tu1te)
            TpRunTsip.TeVects.TeVectorLinMux(tuVec, tuVec, tu2)
            TpRunTsip.TeVects.TeVectorLinMux(tuVec, teVec, tute)

            nu = (te2 * tu1es - tees * tu1te) / (tu1es * tu1te - tees)
            nu = If(nu < 0.1, 0.1, nu)

            anteStruct.tvdisttu = nu
            anteNulls(TeAnte.TVDISTTU) = Constant.DB_NOT_NULL

            h = anteStruct.tvdisttu * SinD(tuElev)

            If h > HR Then
                anteStruct.tvdisttu = HR / SinD(tuElev)
                nu = anteStruct.tvdisttu
            End If
            TpRunTsip.TeVects.TeVectorSclMux(nu, tu1Vec, evVec)
            TpRunTsip.TeVects.TeVectorSub(evVec, teVec, evVec)

            TpRunTsip.TeVects.TeVectorLen(evVec, anteStruct.evdisttu)
            anteNulls(TeAnte.EVDISTTU) = Constant.DB_NOT_NULL

            evAngVec(0) = Abs(Atan(evVec(1) / evVec(0)))
            evAngVec(1) = Abs(Atan(evVec(0) / evVec(1)))
            evAngVec(2) = Math.Abs(Math.Atan(evVec(2) / anteStruct.evdisttu / Math.Pow(1.0 - Math.Pow(evVec(2) / anteStruct.evdisttu, 2.0), 0.5)))

            ez = evAngVec(0) / RAD

            If evVec(0) >= 0.0 Then
                If evVec(1) >= 0.0 Then
                    anteStruct.evazim = 180.0 - ez
                Else
                    anteStruct.evazim = 180.0 + ez
                End If
            ElseIf evVec(1) >= 0.0 Then
                anteStruct.evazim = ez
            Else
                anteStruct.evazim = 360.0 - ez
            End If
            anteNulls(TeAnte.EVAZIM) = Constant.DB_NOT_NULL

            anteStruct.evelev = evAngVec(2) / RAD
            anteNulls(TeAnte.EVELEV) = Constant.DB_NOT_NULL

            If evVec(2) < 0.0 Then
                anteStruct.evelev = -anteStruct.evelev
            End If

            TpRunTsip.TeVects.TeVectorLinMux(evVec, esVec, tmpVal)

            cosbet = tmpVal / anteStruct.evdisttu

            anteStruct.anglesev = -AtanD(cosbet / Sqrt(1.0 - Pow(cosbet, 2.0))) + 90.0
            anteNulls(TeAnte.ANGLESEV) = Constant.DB_NOT_NULL
        End Sub

        ''' <summary>
        ''' Calculates the HR value given the latitude of the earth 
        ''' station.  
        ''' </summary>
        ''' <paramname="earthLatSec"></param>
        ''' <returns></returns>
        Public Shared Function CalcHR(earthLatSec As Double) As Double
            Dim HR, earthLatDeg As Double

            earthLatDeg = earthLatSec / 3600.0
            If earthLatDeg <= 20.0 Then
                HR = 5.2
            ElseIf earthLatDeg <= 70.0 Then
                HR = 7.15 - 0.081 * earthLatDeg - 2450.0 / Pow(earthLatDeg, 3.0)
            Else
                HR = 1.47
            End If
            Return HR
        End Function

        ''' <summary>
        ''' Calculates the 20% loss for mode 1 (tropospheric scattering).  
        ''' </summary>
        ''' <paramname="dist"></param>
        ''' <paramname="radio"></param>
        ''' <paramname="freqMhz"></param>
        ''' <paramname="earthht"></param>
        ''' <paramname="terrht"></param>
        ''' <paramname="spherecalc"></param>
        ''' <paramname="loss"></param>
        Public Shared Sub TeCalcL20M1(dist As Double, radio As String, freqMhz As Double, earthht As Double, terrht As Double, spherecalc As String, <Out> ByRef loss As Double)
            Dim const1, const2 As Double, C1 = 17.6, C2 = 6374.82
            Dim x, fx, y, yt, ye, gyt, gye, fsl As Double


            If Strings.FirstCharIs(radio, "A"c) AndAlso (Strings.FirstCharIs(spherecalc, "Y"c) OrElse Strings.FirstCharIs(spherecalc, "2"c)) Then
                x = 2.2 * dist * Pow(freqMhz, 1.0 / 3.0) * Pow(Constant.K_FACTOR * C2, -2.0 / 3.0)
                fx = 11.0 + 10.0 * Log10(x) - C1 * x
                y = 9.600001 * Pow(freqMhz, 2.0 / 3.0) * Pow(Constant.K_FACTOR * C2, -1.0 / 3.0)
                yt = y * terrht
                ye = y * earthht
                If yt > 2.0 Then
                    gyt = C1 * Pow(yt - 1.1, 0.5) - 5.0 * Log10(yt - 1.1) - 8.0
                Else
                    gyt = 20.0 * Log10(yt + 0.1 * Pow(yt, 3.0))
                End If
                If ye > 2.0 Then
                    gye = C1 * Pow(ye - 1.1, 0.5) - 5.0 * Log10(ye - 1.1) - 8.0
                Else
                    gye = 20.0 * Log10(ye + 0.1 * Pow(ye, 3.0))
                End If
                fsl = 32.45 + 20.0 * Log10(dist) + 20.0 * Log10(freqMhz)
                loss = If(fsl - fx - gyt - gye < fsl, fsl, fsl - fx - gyt - gye)
            Else
                If dist <= 90.0 Then
                    const1 = 104.49
                    const2 = 20.0
                ElseIf dist <= 160.0 Then
                    If Strings.FirstCharIs(radio, "A"c) Then
                        const1 = -228.0
                        const2 = 190.0
                    ElseIf Strings.FirstCharIs(radio, "B"c) Then
                        const1 = -207.4
                        const2 = 179.57
                    Else
                        const1 = 191.75
                        const2 = 171.56
                    End If
                Else
                    If Strings.FirstCharIs(radio, "A"c) Then
                        const1 = 14.0
                        const2 = 80.0
                    ElseIf Strings.FirstCharIs(radio, "B"c) Then
                        const1 = 17.8
                        const2 = 77.4
                    Else
                        const1 = 15.36
                        const2 = 77.6
                    End If
                End If
                If freqMhz > 0 Then
                    loss = const1 + const2 * Log10(dist) + 20.0 * Log10(freqMhz / 4000.0)
                Else
                    loss = 0
                End If
            End If
        End Sub

        ''' <summary>
        ''' Calculates the . 01% loss for mode 1.  
        ''' </summary>
        ''' <paramname="radio"></param>
        ''' <paramname="loss20m1"></param>
        ''' <paramname="freqMhz"></param>
        ''' <paramname="elevAng"></param>
        ''' <paramname="dist"></param>
        ''' <paramname="type"></param>
        ''' <paramname="loss"></param>
        Public Shared Sub TeCalcL01M1(radio As String, loss20m1 As Double, freqMhz As Double, elevAng As Double, dist As Double, type As Short, <Out> ByRef loss As Double)
            ' 'out' requirement.
            loss = 0.0

            Dim lossOxygen As Double
            Dim lossVapour As Double
            Dim lossOther As Double
            Dim baseRate = 0.0
            Dim rateOfAtten = 0.0
            Dim lossHor As Double
            Dim freqGhz As Double

            If dist < 90.0 Then
                loss = loss20m1
                Return
            End If

            freqGhz = freqMhz / 1000.0
            If freqMhz > 0 Then
                baseRate = 120.0 + 20.0 * Log10(freqGhz)

                lossOxygen = 0.0068 * Pow(freqGhz, Constant.SQUARE) * (1.0 / Pow(60.0 - freqGhz, Constant.SQUARE) + 1.0 / Pow(60.0 + freqGhz, Constant.SQUARE) + 1.0 / (Pow(freqGhz, Constant.SQUARE) + 0.36))

                lossVapour = 0.000003 * Pow(freqGhz, Constant.SQUARE) + 0.00035 * (1.0 / (Pow(1.0 - 22.3 / freqGhz, Constant.SQUARE) + 9.0 / Pow(freqGhz, Constant.SQUARE)) + 1.0 / Pow(1.0 + 22.3 / freqGhz, Constant.SQUARE))


                If Strings.FirstCharIs(radio, "A"c) Then
                    lossOther = 0.154 * Pow(1.0 + 3.05 * Log10(freqGhz), 0.4) * Pow(0.9028 + 0.0486 * Log10(0.01), Constant.SQUARE)
                    rateOfAtten = lossOxygen + lossVapour + lossOther
                Else
                    lossOther = Pow(0.272 + 0.047 * Log10(0.01), Constant.SQUARE)
                    If Strings.FirstCharIs(radio, "B"c) Then
                        rateOfAtten = lossOxygen + 2.0 * lossVapour + lossOther
                    Else
                        rateOfAtten = lossOxygen + 5.0 * lossVapour + lossOther
                    End If
                End If
            End If

            If type = Constant.ES_TS Then
                HorLoss.AxHorLoss(freqGhz, elevAng, lossHor)
            Else
                lossHor = 0.0
            End If

            If freqMhz > 0 Then
                loss = baseRate + dist * rateOfAtten + lossHor
            Else
                loss = 0
            End If
        End Sub

        ''' <summary>
        ''' Calculates the rain scattering angle. The rain volume is first 
        ''' calculated to be on the ES vector and then the TU vector. The transmission 
        ''' loss for scattering propagation is calculated for both cases and the 
        ''' minimum loss determines the calculated placement of the volume. The angle 
        ''' is either SEV (satellite, earth, rain volume) or UTV (TS remote site, TS, 
        ''' rain volume).  
        ''' </summary>
        ''' <paramname="parmStruct"></param>
        ''' <paramname="siteStruct"></param>
        ''' <paramname="anteStruct"></param>
        ''' <paramname="chanStruct"></param>
        ''' <paramname="chanNulls"></param>
        ''' <paramname="earthAGain"></param>
        ''' <paramname="terrAGain"></param>
        ''' <paramname="earthAfslt"></param>
        ''' <paramname="nullEarthAfslt"></param>
        ''' <paramname="earthAfslr"></param>
        ''' <paramname="nullEarthAfslr"></param>
        ''' <paramname="terrAfsl"></param>
        ''' <paramname="loss01mode2"></param>
        ''' <paramname="nullL01M2"></param>
        ''' <paramname="intPrintMsg"></param>
        ''' <paramname="vicPrintMsg"></param>
        ''' <returns></returns>
        Public Shared Function TeCalcScang(parmStruct As TpParm, ByRef siteStruct As TeSite, ByRef anteStruct As TeAnte, ByRef chanStruct As TeChan, ByRef chanNulls As SQLLEN(), earthAGain As Double, terrAGain As Double, earthAfslt As Double, nullEarthAfslt As SQLLEN, earthAfslr As Double, nullEarthAfslr As SQLLEN, terrAfsl As Double, <Out> ByRef loss01mode2 As Double, <Out> ByRef nullL01M2 As SQLLEN, intPrintMsg As String, vicPrintMsg As String) As Integer
            ' 'out' requirements.
            loss01mode2 = 0.0
            nullL01M2 = Constant.DB_NULL

            Dim adisccv = 0.0
            Dim adiscxv = 0.0
            Dim adiscch = 0.0
            Dim adiscxh = 0.0
            Dim terrMbnd = 0.0
            Dim adiscSev = Constant.DFLT_ADISCW
            Dim adiscUtv = Constant.DFLT_ADISCW
            Dim adiscAlpha = Constant.DFLT_ADISCW
            Dim adiscBeta = Constant.DFLT_ADISCW
            Dim HR As Double
            Dim R As Double
            Dim alpha As Double
            Dim beta As Double
            Dim abtx As Double
            Dim abrx As Double
            Dim lrtx As Double
            Dim lrrx As Double
            Dim kltx As Double
            Dim klrx As Double
            Dim ettu1 As Double
            Dim eset As Double
            Dim et2 As Double
            Dim estu As Double
            Dim estu1 As Double
            Dim evDist As Double
            Dim tvDist As Double
            Dim junk As Double
            Dim tmpVal As Double
            Dim cond As Double
            Dim tuDist As Double
            Dim te2 As Double
            Dim tu1es As Double
            Dim tees As Double
            Dim tu1te As Double
            Dim tu2 As Double
            Dim tute As Double
            Dim lossVolAlpha As Double
            Dim lossVolBeta As Double
            Dim disc As Double
            Dim esVec = New Double(2) {}
            Dim etVec = New Double(2) {}
            Dim euVec = New Double(2) {}
            Dim teVec = New Double(2) {}
            Dim tuVec = New Double(2) {}
            Dim tu1Vec = New Double(2) {}
            Dim tmpVec = New Double(2) {}
            Dim tvVec = New Double(2) {}
            Dim evVec = New Double(2) {}
            Dim tvdiscang As Double

            Dim junkName = ""
            Dim terrSiteName As String
            Dim terrAnteName = ""

            Dim nullCv As SQLLEN = 0
            Dim nullXv As SQLLEN = 0
            Dim nullCh As SQLLEN = 0
            Dim nullXh As SQLLEN = 0
            Dim terrMDB = False
            Dim earthMDB = False

            Dim tmpG = 0
            Dim tmpGain = 0
            Dim rc As Integer

            Dim curBand As SdBand

            If anteStruct.earthacode.StartsWith("CCIR") Then
                Dim nRes As Integer = TpRunTsip.TeSubCalc.sf.Parse(anteStruct.earthacode, "%4s%2d%2d")
                Dim results As List(Of Object) = TpRunTsip.TeSubCalc.sf.Results
                junkName = CStr(results(0))

                If Strings.FirstCharIs(anteStruct.interferer, "E"c) Then
                    '&&Console.Error.Write("\nBolshoi");
                    tmpG = CInt(results(1))
                    tmpGain = CInt(results(2))  ' TS-ES 
                Else
                    '&&Console.Error.Write("\nVelvet");
                    ' positions 5&6 are the againRx 
                    tmpG = CInt(results(2))
                    tmpGain = CInt(results(1))
                End If

                '&&Console.Error.Write("\njunkName = " + junkName);
                '&&Console.Error.Write("\ntmpG = " + tmpG);
                '&&Console.Error.Write("\ntmpGain = " + tmpGain);
            End If

            '&&Console.Error.Write("\nteSubCalc.teCalc.Scang(): aardvark: earthacode = " +  anteStruct.earthacode);
            ' calc discrimination for angle SEV -> adiscSev 
            If Not anteStruct.earthacode.StartsWith("CCIR") Then
                If CSharpImpl.__Assign(rc, TpGetDat.TpCalcDisc(anteStruct.earthacode, Math.Abs(anteStruct.anglesev), adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Return rc
                End If

                TpRunTsip.TeSubCalc.FindMin(adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, adiscSev)
                If adiscSev = Constant.DFLT_ADISCW Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].CALCDISC, "SEV")
                    Return Constant.FAILURE
                End If
            Else
                If Math.Abs(anteStruct.anglesev) < 1.0 Then
                    adiscSev = 0.0
                ElseIf Math.Abs(anteStruct.anglesev) >= 48.0 Then
                    adiscSev = tmpGain + 10.0
                Else
                    adiscSev = tmpGain - (32.0 - 25.0 * Math.Log10(anteStruct.anglesev))
                End If
            End If

            ' check if local TS site is a passive reflector:
            '  if call1 begins with '%'
            ' 
            If Strings.FirstCharIs(anteStruct.terrcall1, "%"c) Then

                TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, terrSiteName, junkName, junkName, terrMDB, earthMDB)

                TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_ANTE, terrAnteName, junkName, junkName, terrMDB, earthMDB)

                ' get midband freq from SDB for terrestrial site 
                If TpRunTsip.TsipUtils.UtGetBand(anteStruct.terrbndcde, curBand) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, anteStruct.terrbndcde)
                    Return Constant.FAILURE
                End If
                terrMbnd = curBand.bmidf

                ' calculate terrestrials's passive reflector & discrim 
                If Strings.FirstCharIs(anteStruct.interferer, "T"c) Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, terrMDB, terrAnteName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, anteStruct.angleutv, disc, junk, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, terrMDB, terrAnteName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, anteStruct.angleutv, disc, junk, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Return rc
                End If
                adiscUtv = disc
            Else
                ' terrestrial site is not a passive repeater
                '  get antenna pattern and calculate antenna discriminations for
                '  the TS antenna
                ' 
                ' get full table names for subsidiary ante and antd info 

                ' 	Decide on the discrimination angle if this is off-axis - 1108
                ' 		GJS - 2002.12 
                If Strings.FirstCharIs(anteStruct.tsoffaxis, "Y"c) Then
                    tvdiscang = anteStruct.angleatv
                Else
                    tvdiscang = anteStruct.angleutv
                End If

                ' calc discrimination for angle UTV -> adiscUtv 
                If CSharpImpl.__Assign(rc, TpGetDat.TpCalcDisc(anteStruct.terracode, Abs(tvdiscang), adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Return rc
                End If

                TpRunTsip.TeSubCalc.FindMin(adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, adiscUtv)

                If adiscUtv = Constant.DFLT_ADISCW Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].CALCDISC, "UTV")
                    Return Constant.FAILURE
                End If
            End If


            ' -----------------------------
            ' set up the needed variables 

            HR = TpRunTsip.TeSubCalc.CalcHR(siteStruct.earthlatit / 100.0)

            TpRunTsip.TeVects.TeBuildVector(1.0, anteStruct.eselev, anteStruct.esazim, esVec)
            TpRunTsip.TeVects.TeBuildVector(siteStruct.etdist, anteStruct.etelev, siteStruct.etazim, etVec)
            TpRunTsip.TeVects.TeBuildVector(siteStruct.eudist, anteStruct.euelev, siteStruct.euazim, euVec)

            TpRunTsip.TeVects.TeVectorSub(euVec, etVec, tuVec)
            TpRunTsip.TeVects.TeVectorUnit(tuVec, tu1Vec)


            ' ------------------------
            ' Rain Cell on ES Vector 

            TpRunTsip.TeVects.TeVectorLinMux(etVec, tu1Vec, ettu1)
            TpRunTsip.TeVects.TeVectorLinMux(esVec, etVec, eset)
            TpRunTsip.TeVects.TeVectorLinMux(etVec, etVec, et2)
            TpRunTsip.TeVects.TeVectorLinMux(esVec, tuVec, estu)
            TpRunTsip.TeVects.TeVectorLinMux(esVec, tu1Vec, estu1)
            cond = (et2 * estu1 - ettu1 * eset) / (estu1 * eset - ettu1)

            cond = If(cond < 0.1, 0.1, cond)

            TpRunTsip.TeVects.TeVectorSclMux(cond, esVec, tmpVec)
            TpRunTsip.TeVects.TeVectorSub(tmpVec, etVec, tvVec)
            TpRunTsip.TeVects.TeVectorLen(tvVec, tvDist)
            TpRunTsip.TeVects.TeVectorLen(tuVec, tuDist)

            TpRunTsip.TeVects.TeVectorLinMux(tvVec, tuVec, tmpVal)
            alpha = AcosD(tmpVal / (tvDist * tuDist))

            ' check if local TS site is a passive reflector:
            '  if call1 begins with '%'
            ' 
            If Strings.FirstCharIs(anteStruct.terrcall1, "%"c) Then
                ' calculate terrestrials's passive reflector out  discrim 
                If Strings.FirstCharIs(anteStruct.interferer, "T"c) Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, terrMDB, terrAnteName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, alpha, disc, junk, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, terrMDB, terrAnteName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, alpha, disc, junk, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Return rc
                End If
                adiscAlpha = disc ' terrestrial site is not a passive repeater
                ' 		 * get antenna pattern and calculate antenna discriminations for
                ' 		 * the TS antenna	 
            Else
                ' calc discrimination for angle Alpha -> adiscAlpha 
                If CSharpImpl.__Assign(rc, TpGetDat.TpCalcDisc(anteStruct.terracode, Abs(alpha), adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Return rc
                End If

                TpRunTsip.TeSubCalc.FindMin(adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, adiscAlpha)
                If adiscAlpha = Constant.DFLT_ADISCW Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].CALCDISC, "Alpha")
                    Return Constant.FAILURE
                End If
            End If

            ' check Max Rain Cell Ht or check for minimum distance condition loss
            If anteStruct.evdistes * Maths.SinD(anteStruct.eselev) > HR OrElse 20.0 * Log10(tvDist) + adiscAlpha < 20.0 * Math.Log10(anteStruct.tvdistes) + adiscUtv Then
                cond = HR / Maths.SinD(anteStruct.eselev)
                TpRunTsip.TeVects.TeVectorSclMux(cond, esVec, tmpVec)
                TpRunTsip.TeVects.TeVectorSub(tmpVec, etVec, tvVec)
                TpRunTsip.TeVects.TeVectorLen(tvVec, tvDist)
                anteStruct.tvdistes = tvDist
                TpRunTsip.TeVects.TeVectorLinMux(tvVec, tuVec, tmpVal)
                alpha = AcosD(tmpVal / (tvDist * tuDist))

                ' check if local TS site is a passive reflector:
                '  if call1 begins with '%'
                ' 
                If Strings.FirstCharIs(anteStruct.terrcall1, "%"c) Then
                    ' calc terrestrials's passive reflector out  discrim 
                    If Strings.FirstCharIs(anteStruct.interferer, "T"c) Then
                        rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, terrMDB, terrAnteName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, alpha, disc, junk, intPrintMsg, vicPrintMsg)
                    Else
                        rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, terrMDB, terrAnteName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, alpha, disc, junk, intPrintMsg, vicPrintMsg)
                    End If
                    If rc <> Constant.SUCCESS Then
                        Return rc
                    End If
                    adiscAlpha = disc ' terrestrial site is not a passive repeater
                    ' 			 * get antenna pattern and calc antenna discrims for
                    ' 			 * the TS antenna
                    ' 			 
                Else
                    ' calc discrimination for angle Alpha -> adiscAlpha 
                    If CSharpImpl.__Assign(rc, TpGetDat.TpCalcDisc(anteStruct.terracode, Abs(alpha), adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                    adiscAlpha = Constant.DFLT_ADISCW

                    TpRunTsip.TeSubCalc.FindMin(adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, adiscAlpha)

                    If adiscAlpha = Constant.DFLT_ADISCW Then
                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                        ErrMsg.UtPrintMessage([Error].CALCDISC, "Alpha")
                        Return Constant.FAILURE
                    End If
                End If
            Else
                alpha = anteStruct.angleutv
                adiscAlpha = adiscUtv
            End If


            ' ------------------------
            ' Rain Cell on TU Vector 

            TpRunTsip.TeVects.TeVectorSclMux(-1.0, etVec, teVec)

            TpRunTsip.TeVects.TeVectorLinMux(teVec, teVec, te2)
            TpRunTsip.TeVects.TeVectorLinMux(tu1Vec, esVec, tu1es)
            TpRunTsip.TeVects.TeVectorLinMux(teVec, esVec, tees)
            TpRunTsip.TeVects.TeVectorLinMux(tu1Vec, teVec, tu1te)
            TpRunTsip.TeVects.TeVectorLinMux(tuVec, tuVec, tu2)
            TpRunTsip.TeVects.TeVectorLinMux(tuVec, teVec, tute)
            cond = (te2 * tu1es - tees * tu1te) / (tu1es * tu1te - tees)

            cond = If(cond < 0.1, 0.1, cond)

            TpRunTsip.TeVects.TeVectorSclMux(cond, tu1Vec, tmpVec)
            TpRunTsip.TeVects.TeVectorSub(tmpVec, teVec, evVec)
            TpRunTsip.TeVects.TeVectorLen(evVec, evDist)

            beta = AcosD((cond * tu1es - tees) / (Pow(cond, Constant.SQUARE) * tu2 + te2 - 2.0 * cond * tute))

            ' calc discrimination for angle Beta -> adiscBeta 
            If Not anteStruct.earthacode.StartsWith("CCIR") Then
                If CSharpImpl.__Assign(rc, TpGetDat.TpCalcDisc(anteStruct.earthacode, Abs(beta), adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Return rc
                End If
                TpRunTsip.TeSubCalc.FindMin(adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, adiscBeta)
                If adiscBeta = Constant.DFLT_ADISCW Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].CALCDISC, "Beta")
                    Return Constant.FAILURE
                End If
            Else
                If Abs(beta) < 1.0 Then
                    adiscBeta = 0.0
                ElseIf Abs(beta) >= 48.0 Then
                    adiscBeta = tmpGain + 10.0
                Else
                    adiscBeta = tmpGain - (32.0 - 25.0 * Log10(beta))
                End If
            End If

            ' check Max Rain Cell Ht 
            ' check for minimum distance condition loss 
            If anteStruct.tvdistes * Maths.SinD(anteStruct.tuelev) > HR OrElse 20.0 * Log10(evDist) + adiscBeta < 20.0 * Math.Log10(anteStruct.evdisttu) + adiscSev Then
                cond = HR / Maths.SinD(anteStruct.tuelev)
                TpRunTsip.TeVects.TeVectorSclMux(cond, tu1Vec, tmpVec)
                TpRunTsip.TeVects.TeVectorSub(tmpVec, teVec, evVec)
                TpRunTsip.TeVects.TeVectorLen(evVec, evDist)
                anteStruct.evdisttu = evDist
                beta = AcosD((cond * tu1es - tees) / (Pow(cond, Constant.SQUARE) * tu2 + te2 - 2.0 * cond * tute))

                ' calc discrimination for angle Beta -> adiscBeta 
                If Not anteStruct.earthacode.StartsWith("CCIR") Then
                    If CSharpImpl.__Assign(rc, TpGetDat.TpCalcDisc(anteStruct.earthacode, Abs(beta), adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                        Return rc
                    End If
                    adiscBeta = Constant.DFLT_ADISCW
                    TpRunTsip.TeSubCalc.FindMin(adisccv, adiscxv, adiscch, adiscxh, nullCv, nullXv, nullCh, nullXh, adiscBeta)
                    If adiscBeta = Constant.DFLT_ADISCW Then
                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                        ErrMsg.UtPrintMessage([Error].CALCDISC, "Beta")
                        Return Constant.FAILURE
                    End If
                Else
                    If Abs(beta) < 1.0 Then
                        adiscBeta = 0.0
                    ElseIf Abs(beta) >= 48.0 Then
                        adiscBeta = tmpGain + 10.0
                    Else
                        adiscBeta = tmpGain - (32.0 - 25.0 * Log10(beta))
                    End If
                End If
            Else
                beta = anteStruct.anglesev
                adiscBeta = adiscSev
            End If

            ' -----------------------------------------------
            ' Calculation of Rain Scatter Transmission Loss 

            ' values stored in KHz, ProvValues uses MHz 
            If TpSub.TePropValues(siteStruct.rainzone, chanStruct.intfreqtx / 1000.0, chanStruct.intfreqtx / 1000.0, R, abtx, lrtx) <> 0 Then
                ' 	The rain zone was invalid and has been set to 9 
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "*WARNING* Rain Zone {0} is invalid: {1},{2}. Using 9.", siteStruct.rainzone, siteStruct.terrcall1, siteStruct.earthlocation)
            End If
            ' No need to print an error here, as it will already have been printed 
            TpSub.TePropValues(siteStruct.rainzone, chanStruct.intfreqtx / 1000.0, chanStruct.vicfreqrx / 1000.0, R, abrx, lrrx)

            ' calculations use freq in GHz 
            If chanStruct.intfreqtx > 0 Then
                kltx = 168.0 - 20.0 * Math.Log10(chanStruct.intfreqtx / 1000000.0) - 13.2 * Log10(R) + lrtx + abtx
            Else
                ' intfreqtx is null or an invalid value 
                kltx = 9999999     ' max out kltx 
            End If

            If chanStruct.vicfreqrx > 0 Then
                klrx = 168.0 - 20.0 * Math.Log10(chanStruct.vicfreqrx / 1000000.0) - 13.2 * Log10(R) + lrrx + abrx
            Else
                ' vicfreqrx is null or an invalid value 
                klrx = 9999999     ' max out klrx 
            End If
            kltx = If(klrx < kltx, klrx, kltx)

            ' --------------------------------------------------------
            ' calculate transmission loss for scattering propagation 

            If nullEarthAfslr = Constant.DB_NULL Then
                earthAfslr = 0.0
            End If

            If nullEarthAfslt = Constant.DB_NULL Then
                earthAfslt = 0.0
            End If

            If Strings.FirstCharIs(chanStruct.interferer, "T"c) OrElse Strings.FirstCharIs(chanStruct.interferer, "E"c) Then
                If Strings.FirstCharIs(chanStruct.interferer, "T"c) Then
                    lossVolAlpha = kltx + 20.0 * Math.Log10(anteStruct.tvdistes) - (terrAGain - adiscAlpha) + earthAfslr + terrAfsl
                    lossVolBeta = kltx + 20.0 * Math.Log10(anteStruct.evdisttu) - (earthAGain - adiscBeta) + earthAfslr + terrAfsl
                Else
                    lossVolAlpha = kltx + 20.0 * Math.Log10(anteStruct.tvdistes) - (terrAGain - adiscAlpha) + earthAfslt + terrAfsl
                    lossVolBeta = kltx + 20.0 * Math.Log10(anteStruct.evdisttu) - (earthAGain - adiscBeta) + earthAfslt + terrAfsl
                End If

                If lossVolAlpha < lossVolBeta Then
                    loss01mode2 = lossVolAlpha
                    chanStruct.scang = alpha
                Else
                    loss01mode2 = lossVolBeta
                    chanStruct.scang = beta
                End If
                chanNulls(Constant.TE_CHAN_SCANG) = Constant.DB_NOT_NULL
                nullL01M2 = Constant.DB_NOT_NULL
            Else
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, Microsoft.VisualBasic.Constants.vbTab & "SCANG Calculations inconclusive for Rain Vol placement")
                chanNulls(Constant.TE_CHAN_SCANG) = Constant.DB_NULL
                nullL01M2 = Constant.DB_NULL
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' Calculates the minimum of the four antenna discriminations 
        ''' passed to it (note nulls for the discriminations are also checked) the 
        ''' adiscSev parameter must be initialized to something acceptable.  
        ''' </summary>
        ''' <paramname="adisccv"></param>
        ''' <paramname="adiscxv"></param>
        ''' <paramname="adiscch"></param>
        ''' <paramname="adiscxh"></param>
        ''' <paramname="nullCv"></param>
        ''' <paramname="nullXv"></param>
        ''' <paramname="nullCh"></param>
        ''' <paramname="nullXh"></param>
        ''' <paramname="adiscSev"></param>
        Public Shared Sub FindMin(adisccv As Double, adiscxv As Double, adiscch As Double, adiscxh As Double, nullCv As Short, nullXv As Short, nullCh As Short, nullXh As Short, ByRef adiscSev As Double)
            If nullCv <> Constant.DB_NULL Then
                adiscSev = If(adiscSev < adisccv, adiscSev, adisccv)
            End If
            If nullXv <> Constant.DB_NULL Then
                adiscSev = If(adiscSev < adiscxv, adiscSev, adiscxv)
            End If
            If nullCh <> Constant.DB_NULL Then
                adiscSev = If(adiscSev < adiscch, adiscSev, adiscch)
            End If
            If nullXh <> Constant.DB_NULL Then
                adiscSev = If(adiscSev < adiscxh, adiscSev, adiscxh)
            End If
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class





    End Class
End Namespace
