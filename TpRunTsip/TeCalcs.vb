Imports _DataStructures
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports _Configuration
Imports _Utillib
Imports _Auxlib
Imports _NewLib
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16
Imports System.Runtime.InteropServices
Imports System.Math
Imports _NewLib.Maths

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods that perform TSIP calculations for the ES case.
    ''' </summary>
    Public Class TeCalcs
#If PINVOKE
        [DllImport("tpruntsip.dll", CharSet = CharSet.Ansi)]
        private extern static int teAnteCalcs([In] TpParm parmStruct,
                                                [In, Out] TeAnte anteStruct,
                                                [In, Out] TeSite siteStruct,
                                                [In, Out] SQLLEN[] anteNulls,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);

        [DllImport("tpruntsip.dll", CharSet = CharSet.Ansi)]
        private extern static int teChanCalcs([In, Out] TeChan chanStruct,
                                                [In, Out] TeAnte anteStruct,
                                                [In, Out] TeSite siteStruct,
                                                [In] TpParm parmStruct,
                                                [In, Out] SQLLEN[] chanNulls,
                                                [In] string intPrintMsg,
                                                [In] string vicPrintMsg);


        public static int TeChanCalcs_NATIVE(ref TeChan chanStruct,
                                 ref TeAnte anteStruct,
                                 ref TeSite siteStruct,
                                 TpParm parmStruct,
                                 ref SQLLEN[] chanNulls,
                                 string intPrintMsg,
                                 string vicPrintMsg)
        {
            return teChanCalcs(chanStruct, anteStruct, siteStruct, parmStruct, chanNulls, intPrintMsg, vicPrintMsg);
        }

        public static int TeAnteCalcs_NATIVE(TpParm parmStruct,
                    ref TeAnte anteStruct,
                    ref TeSite siteStruct,
                    ref SQLLEN[] anteNulls,
                    string intPrintMsg,
                    string vicPrintMsg)
        {
            return teAnteCalcs(parmStruct, anteStruct, siteStruct, anteNulls, intPrintMsg, vicPrintMsg);
        }
#End If
        '-------------------------------------------------------------------------------

        Private Shared oldTrafTx As String = ""
        Private Shared oldTrafRx As String = ""
        Private Shared oldEqptTx As String = ""
        Private Shared oldEqptRx As String = ""
        Private Shared oldRc As Integer
        Private Shared curCtx As CtxStruct = New CtxStruct()
        Private Shared sf As ScanFormatted = New ScanFormatted()
        '-------------------------------------------------------------------------------


        ''' <summary>
        ''' This method performs calculations at the site level to populate the following fields in 
        ''' the ES SH Site Table: terrname2, earthoper, intreq, tudist, tuazim, utazim, 
        ''' etdist, etazim, teazim, eudist, euazim, ueazim.  
        ''' </summary>
        ''' <paramname="parmStruct"> - TpParm object providing parameter data.</param>
        ''' <paramname="siteStruct"> - TeSite object.</param>
        ''' <paramname="siteNulls"> - ODBC nullInds associated with siteStruct.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function TeSiteCalcs(parmStruct As TpParm, ByRef siteStruct As TeSite, ByRef siteNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String) As Integer
            '...Log2.v("\nTeCalcs.TeSiteCalcs(): Entry");

            Dim rc As Integer
            Dim terrMDB, earthMDB As Boolean
            Dim tmpSiteNulls As SQLLEN()  ' [FtSite.SIZE_ > FE_SITE_SIZE_ ? FtSite.SIZE_ : FE_SITE_SIZE_];
            Dim uLatit, uLongit As Integer
            Dim [select] = ""
            Dim terrTabName As String  ' [TABLE_NM_SZ]
            Dim earthTabName As String  ' [TABLE_NM_SZ]
            Dim junkName As String  ' [TABLE_NM_SZ]
            Dim terrOprTyp As String  ' [3]
            Dim earthOprTyp As String  ' [3]
            Dim ftTmpSite As FtSite
            Dim feTmpSite As FeSite

            TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, terrTabName, earthTabName, junkName, terrMDB, earthMDB)

            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetCall(siteStruct.terrcall2, terrTabName, terrMDB, ftTmpSite, tmpSiteNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", [select])
                ' can't find remote site - tell calling process to continue processing 
                Return Constant.CONT_PROCESSING
            End If

            siteStruct.terrname2 = ftTmpSite.name
            siteNulls(TeSite.TERRNAME2) = tmpSiteNulls(FtSite.NAME)
            siteStruct.terroper2 = ftTmpSite.oper
            siteNulls(TeSite.TERROPER2) = tmpSiteNulls(FtSite.NAME)

            uLatit = ftTmpSite.latit
            uLongit = ftTmpSite.longit

            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetCall(siteStruct.terrcall1, terrTabName, terrMDB, ftTmpSite, tmpSiteNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", [select])
                Return rc
            End If
            terrOprTyp = ftTmpSite.oprtyp

            [select] = SQLCHARPTR.Format("location = '{0}'", siteStruct.earthlocation)
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TeSiteGetLoc(siteStruct.earthlocation, earthTabName, earthMDB, feTmpSite, tmpSiteNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", [select])
                Return rc
            End If
            earthOprTyp = feTmpSite.oprtyp

            If Strings.FirstCharIs(earthOprTyp, "F"c) AndAlso Strings.FirstCharIs(terrOprTyp, "F"c) AndAlso Not siteStruct.earthoper.Equals("TGLB") Then
                siteStruct.intreq = "FCSA"
            Else
                siteStruct.intreq = "CCIR"
            End If

            siteNulls(TeSite.INTREQ) = Constant.DB_NOT_NULL

            AxSub2.AxDistan(siteStruct.terrlatit / 100.0, uLatit / 100.0, siteStruct.terrlongit / 100.0, uLongit / 100.0, siteStruct.tudist, siteStruct.tuazim, siteStruct.utazim)
            siteNulls(TeSite.TUDIST) = Constant.DB_NOT_NULL
            siteNulls(TeSite.TUAZIM) = Constant.DB_NOT_NULL
            siteNulls(TeSite.UTAZIM) = Constant.DB_NOT_NULL

            AxSub2.AxDistan(siteStruct.earthlatit / 100.0, siteStruct.terrlatit / 100.0, siteStruct.earthlongit / 100.0, siteStruct.terrlongit / 100.0, siteStruct.etdist, siteStruct.etazim, siteStruct.teazim)
            siteNulls(TeSite.ETDIST) = Constant.DB_NOT_NULL
            siteNulls(TeSite.ETAZIM) = Constant.DB_NOT_NULL
            siteNulls(TeSite.TEAZIM) = Constant.DB_NOT_NULL

            AxSub2.AxDistan(siteStruct.earthlatit / 100.0, uLatit / 100.0, siteStruct.earthlongit / 100.0, uLongit / 100.0, siteStruct.eudist, siteStruct.euazim, siteStruct.ueazim)
            siteNulls(TeSite.EUDIST) = Constant.DB_NOT_NULL
            siteNulls(TeSite.EUAZIM) = Constant.DB_NOT_NULL
            siteNulls(TeSite.UEAZIM) = Constant.DB_NOT_NULL

            '...Log2.v("\nTeCalcs.TeSiteCalcs(): Exit");

            Return Constant.SUCCESS
        End Function


        ''' <summary>
        ''' This method performs calculations at the antenna level to fill in the following fields in 
        ''' the ES SH Ante Table: terrht, earthht, teelev, etelev, tuelev, utelev, 
        ''' euelev, tdiscang, adisc_ute. It also calls the method TeAnteRainCalcs that populates 
        ''' additional antenna fields.  
        ''' </summary>
        ''' <paramname="parmStruct"> - TpParm object providing parameter data.</param>
        ''' <paramname="anteStruct"> - TeAnte object.</param>
        ''' <paramname="siteStruct"> - TeSite object.</param>
        ''' <paramname="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function TeAnteCalcs(parmStruct As TpParm, ByRef anteStruct As TeAnte, ByRef siteStruct As TeSite, ByRef anteNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String) As Integer
            '...Log2.v("\nTeCalcs.TeAnteCalcs(): Entry");

            Dim rc As Integer
            Dim earthMDB As Boolean
            Dim terrMDB As Boolean
            Dim tmpAnteNulls As SQLLEN()  '[FtAnte.SIZE_];
            Dim tmpSiteNulls As SQLLEN()  '[FtSite.SIZE_];
            Dim teAnteNulls As SQLLEN()  '[FE_ANTE_SIZE_];
            Dim nullADiscCTxV As SQLLEN
            Dim nullADiscXTxV As SQLLEN
            Dim nullADiscCTxH As SQLLEN
            Dim nullADiscXTxH As SQLLEN

            Dim antHtTKm As Double
            Dim antHtEKm As Double
            Dim antHtUKm As Double
            Dim minADisc As Double
            Dim tLnkAht As Double
            Dim tLnkGrnd As Double
            Dim tmpTeElev As Double
            Dim tmpEtElev As Double
            Dim tmpTuElev As Double
            Dim tmpUtElev As Double
            Dim tmpEuElev As Double
            Dim tmpUeElev As Double
            Dim tAht As Double
            Dim eAht As Double
            Dim junk As Double
            Dim disc As Double
            Dim adiscctxv As Double
            Dim adiscxtxv As Double
            Dim adiscctxh As Double
            Dim adiscxtxh As Double
            Dim terrMbnd As Double
            Dim dtAntaz As Double
            Dim dtAntel As Double

            Dim detElev As Double
            Dim deuElev As Double
            Dim etVec As Double()  '[3];    /*	Geometric E to T vector 
            Dim euVec As Double()  '[3];    /*	Geometric E to U vector 
            Dim tuVec As Double()  '[3];    /*	This is the geometric T to U vector, must be calculated
            ' 		here - GJS - 1108 - 2003.01 

            Dim terrTabName As String
            Dim earthTabName As String  'TABLE_NM_SZ]
            Dim terrSiteName As String  'TABLE_NM_SZ]
            Dim earthSiteName As String  'TABLE_NM_SZ]
            Dim azimTabName As String  'TABLE_NM_SZ]
            Dim anteTabName As String  'TABLE_NM_SZ]
            Dim antdTabName As String  'TABLE_NM_SZ]
            Dim junkName As String  'TABLE_NM_SZ]
            Dim [select] = ""  'SEL_STMT_SIZE];

            Dim ftTmpSite As FtSite
            Dim ftTmpAnte As FtAnte
            Dim feTmpAnte As FeAnte
            Dim curBand As SdBand

            Dim terrxtype As Integer

            Dim es As AxStation

            Dim pAnt As SuAntStr
            Dim nRet As Integer


            TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, terrSiteName, earthSiteName, junkName, terrMDB, earthMDB)

            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetCall(siteStruct.terrcall2, terrSiteName, terrMDB, ftTmpSite, tmpSiteNulls)) <> Constant.SUCCESS Then
                Log2.w(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): EXIT: WARNING: can't find remote site: TtSiteGetCall() returned " & rc.ToString())

                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", [select])
                ' can't find remote site - tell calling process to continue processing 
                Return Constant.CONT_PROCESSING
            End If
            tLnkGrnd = ftTmpSite.grnd

            TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_ANTE, terrTabName, earthTabName, azimTabName, terrMDB, earthMDB)

            If Strings.FirstCharIs(parmStruct.protype, "T"c) Then
                junkName = parmStruct.proname
            Else
                junkName = parmStruct.envname
            End If
            ' get the remote antenna's height 
            tLnkAht = TpRunTsip.TeCalcs.GetHighestAntenna(junkName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum)
            '...Log2.v("\nGetHighestAntenna: " + tLnkAht);
            '&&Console.Error.Write("\nTeAnteCalcs(): tLnkAht = " + tLnkAht);

            If tLnkAht < 0.0 Then
                ' Can't find remote antennae;
                '  not a fatal error, print message and continue w/ next ante 
                Log2.w(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): EXIT: WARNING: Call to GetHighestAntenna() returned " & tLnkAht.ToString())

                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, Microsoft.VisualBasic.Constants.vbTab & "WARNING: Could not find remote antenna to calculate antenna height")
                Return Constant.CONT_PROCESSING
            End If

            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtAnteGet(anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrTabName, terrMDB, ftTmpAnte, tmpAnteNulls)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: EXIT: call to TtAnteGet() returned " & rc.ToString())
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                Return rc
            End If
            tAht = ftTmpAnte.aht
            '&&Console.Error.Write("\nTeAnteCalcs(): tAht = " + tAht);

            [select] = SQLCHARPTR.Format("location = '{0}' and call1 = '{1}'", anteStruct.earthlocation, anteStruct.earthcall1)
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TeAnteGet([select], earthTabName, earthMDB, feTmpAnte, teAnteNulls)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): EXIT: ERROR: call to TeAnteGet() returned " & rc.ToString())
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                Return rc
            End If
            eAht = feTmpAnte.aht
            '&&Console.Error.Write("\nTeAnteCalcs(): eAht = " + eAht);

            ' ------ ------
            anteStruct.terrht = siteStruct.terrgrnd + tAht
            anteNulls(TeAnte.TERRHT) = Constant.DB_NOT_NULL

            anteStruct.earthht = siteStruct.earthgrnd + eAht
            anteNulls(TeAnte.EARTHHT) = Constant.DB_NOT_NULL

            antHtTKm = anteStruct.terrht / 1000.0
            antHtEKm = anteStruct.earthht / 1000.0
            antHtUKm = (tLnkGrnd + tLnkAht) / 1000.0


            AxSub3.AxElev(antHtTKm, antHtEKm, siteStruct.etdist, tmpTeElev, tmpEtElev)

            detElev = tmpEtElev    ' 	Save this for the geometric vectors 
            '&&Console.Error.Write("\nTeAnteCalcs(): detElev = " + detElev);

            TpRunTsip.TeCalcs.CalcRefElev(siteStruct.etdist, tmpTeElev, antHtTKm, antHtEKm, tmpTeElev)

            [select] = SQLCHARPTR.Format("location = '{0}' and call1 = '{1}'", anteStruct.earthlocation, anteStruct.earthcall1)

            If CSharpImpl.__Assign(rc, TpRunTsip.TeSubCalc.TeCheckRadioHoriz([select], earthMDB, azimTabName, siteStruct.etdist, siteStruct.teazim, tmpTeElev, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): EXIT: ERROR: A: call to TeCheckRadioHoriz() returned " & rc.ToString())
                Return rc
            End If

            '&&Console.Error.Write("\nTeAnteCalcs(): anteStruct.terracode = " + anteStruct.terracode);

            anteStruct.teelev = tmpTeElev
            anteNulls(TeAnte.TEELEV) = Constant.DB_NOT_NULL


            TpRunTsip.TeCalcs.CalcRefElev(siteStruct.etdist, tmpEtElev, antHtEKm, antHtTKm, tmpEtElev)

            If CSharpImpl.__Assign(rc, TpRunTsip.TeSubCalc.TeCheckRadioHoriz([select], earthMDB, azimTabName, siteStruct.etdist, siteStruct.etazim, tmpEtElev, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): EXIT: ERROR: B: call to TeCheckRadioHoriz() returned " & rc.ToString())
                Return rc
            End If

            anteStruct.etelev = tmpEtElev
            anteNulls(TeAnte.ETELEV) = Constant.DB_NOT_NULL

            AxSub3.AxElev(antHtTKm, antHtUKm, siteStruct.tudist, tmpTuElev, tmpUtElev)

            TpRunTsip.TeCalcs.CalcRefElev(siteStruct.tudist, tmpTuElev, antHtTKm, antHtUKm, tmpTuElev)

            anteStruct.tuelev = tmpTuElev
            anteNulls(TeAnte.TUELEV) = Constant.DB_NOT_NULL

            TpRunTsip.TeCalcs.CalcRefElev(siteStruct.tudist, tmpUtElev, antHtUKm, antHtTKm, tmpUtElev)

            anteStruct.utelev = tmpUtElev
            anteNulls(TeAnte.UTELEV) = Constant.DB_NOT_NULL


            AxSub3.AxElev(antHtEKm, antHtUKm, siteStruct.eudist, tmpEuElev, tmpUeElev)

            deuElev = tmpEuElev    ' 	Save for geometric vectors 

            TpRunTsip.TeCalcs.CalcRefElev(siteStruct.eudist, tmpEuElev, antHtEKm, antHtUKm, tmpEuElev)

            anteStruct.euelev = tmpEuElev
            anteNulls(TeAnte.EUELEV) = Constant.DB_NOT_NULL

            TpRunTsip.TeVects.TeBuildVector(siteStruct.etdist, detElev, siteStruct.etazim, etVec)
            ' euVec is the vector from the es site to the u site (u is other end of the ts link.
            TpRunTsip.TeVects.TeBuildVector(siteStruct.eudist, deuElev, siteStruct.euazim, euVec)

            ' tuVec is the vector (SEZ in the es frame of reference) from the ts site to u.
            TpRunTsip.TeVects.TeVectorSub(euVec, etVec, tuVec)

            '&&Console.Error.Write("\nteAnteCalcs(): dog");
            If CSharpImpl.__Assign(rc, TpRunTsip.TeSubCalc.TeFillEsStnStr(siteStruct, earthMDB, anteStruct, earthTabName, es)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: call to TeFillEsStnStr() returned " & rc.ToString())
                Return rc
            End If

            '&&Console.Error.Write("\nteAnteCalcs(): cat"); Console.Error.Flush();
            If CSharpImpl.__Assign(rc, TpRunTsip.TeSubCalc.TeCalcSET(es, anteStruct, siteStruct, parmStruct, anteNulls, Constant.REF_INDEX, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: call to TeCalcSET() returned " & rc.ToString())

                If rc = [Error].NOTEMPDATA Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "teAnteCalcs(): fox")
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write("Temporary Antenna code: {0}" & Microsoft.VisualBasic.Constants.vbLf, anteStruct.earthacode)
                    ErrMsg.UtPrintMessage([Error].NOTEMPDATA)
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf)
                    Return Constant.CONT_PROCESSING
                End If
                Return rc
            End If

            '&&Console.Error.Write("\nTeAnteCalcs(): anteStruct.terracode = " + anteStruct.terracode);

            ' 	Calculate the ute angle.  This is used for the reports if we have an
            ' 		off-axis angle situation. 
            SepAng.AxSepAng(siteStruct.tuazim, anteStruct.tuelev, siteStruct.teazim, anteStruct.teelev, anteStruct.angleute)
            anteNulls(TeAnte.ANGLEUTE) = Constant.DB_NOT_NULL

            ' 	If the antenna is an offaxis antenna on the terrestrial side, use the
            ' 		offaxis az and el for the input parameters - GJS - 1108 - 2002.12 
            If Strings.FirstCharIs(anteStruct.tsoffaxis, "Y"c) Then
                ' 	Offaxis antenna 
                dtAntaz = anteStruct.tstrueaz
                dtAntel = anteStruct.tstrueel
            Else
                dtAntaz = siteStruct.tuazim
                dtAntel = anteStruct.tuelev
            End If


            SepAng.AxSepAng(dtAntaz, dtAntel, siteStruct.teazim, anteStruct.teelev, anteStruct.tdiscang)

            anteNulls(TeAnte.TDISCANG) = Constant.DB_NOT_NULL

            ' check if local TS site is a passive reflector:
            '  if call1 begins with '%'				 
            If Strings.FirstCharIs(anteStruct.terrcall1, "%"c) Then
                ' get midband freq from SDB for terr site 
                If Suutils.SdGetBand(anteStruct.terrbndcde, curBand) <> 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: call to SdGetBand() returned " & rc.ToString())
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, anteStruct.terrbndcde)
                    Return Constant.FAILURE
                End If
                terrMbnd = curBand.bmidf

                ' calculate terrestrial's passive reflector  discrimination 
                If Strings.FirstCharIs(anteStruct.interferer, "T"c) Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, terrMDB, terrTabName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, anteStruct.tdiscang, disc, junk, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, terrMDB, terrTabName, anteStruct.terrcall1, anteStruct.terrcall2, anteStruct.terrbndcde, anteStruct.terranum, terrMbnd, anteStruct.tdiscang, disc, junk, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: call to TtCalcPassive() returned " & rc.ToString())
                    Return rc
                End If
                adiscctxv = disc
                adiscxtxv = disc
                adiscctxh = disc
                adiscxtxh = disc
                nullADiscCTxV = Constant.DB_NOT_NULL
                nullADiscXTxV = Constant.DB_NOT_NULL
                nullADiscCTxH = Constant.DB_NOT_NULL
                nullADiscXTxH = Constant.DB_NOT_NULL
            Else
                ' TS is not a passive repeater; get antenna pattern and
                '  calculate antenna discriminations for the TS antenna
                ' 
                ' get full table names for subsidiary ante and antd info 
                If (Not parmStruct.tempant.Trim().Equals("")) AndAlso anteStruct.terracode.StartsWith("$") Then
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, parmStruct.tempant, antdTabName)
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, parmStruct.tempant, anteTabName)
                Else
                    antdTabName = "sd_antd"
                    anteTabName = "sd_ante"
                End If

                ' get the antenna model, gain and xref for this ante 
                '&&Console.Error.Write("\nTgt05");
                nRet = Suutils.SuGetAnt(anteStruct.terracode, pAnt)
                If nRet <> 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: A: SuGetAnt returned " & nRet.ToString())
                End If

                If pAnt.acDscPtr Is Nothing Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: A: SuGetAnt returned with pAnt.acDscPtr == null")
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & pAnt.ToString())
                End If

                If nRet = 0 Then
                    If pAnt.acAnt.again <> 0.0 Then
                        anteStruct.terragain = pAnt.acAnt.again
                        anteNulls(TeAnte.TERRAGAIN) = Constant.DB_NOT_NULL
                    End If

                    If pAnt.acAnt.amodel.Length > 0 Then
                        anteStruct.terramodel = pAnt.acAnt.amodel
                        anteNulls(TeAnte.TERRAMODEL) = Constant.DB_NOT_NULL
                    End If

                    'if (!pAnt.acAnt.acode.Equals(anteStruct.terracode)) {
                    anteStruct.terraxref = pAnt.acAnt.axref
                    anteNulls(TeAnte.TERRAXREF) = Constant.DB_NOT_NULL
                    '}

                    terrxtype = pAnt.acAnt.axtype
                    If (terrxtype = 1 OrElse terrxtype = 2) AndAlso anteStruct.tdiscang < 0 Then
                        anteStruct.tdiscang = 360.0 + anteStruct.tdiscang
                    End If
                End If

                ' get antenna pattern and calculate antenna discriminations
                '  for the terrestrial antenna
                ' 
                If CSharpImpl.__Assign(rc, TpGetDat.CalcDisc(pAnt, anteStruct.tdiscang, adiscctxv, adiscxtxv, adiscctxh, adiscxtxh, nullADiscCTxV, nullADiscXTxV, nullADiscCTxH, nullADiscXTxH, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: A: call to CalcDisc() returned " & rc.ToString())
                    Return rc
                End If
            End If

            minADisc = Constant.MAXADISC
            If nullADiscCTxV <> Constant.DB_NULL Then
                minADisc = If(minADisc < adiscctxv, minADisc, adiscctxv)
            End If
            If nullADiscXTxV <> Constant.DB_NULL Then
                minADisc = If(minADisc < adiscxtxv, minADisc, adiscxtxv)
            End If
            If nullADiscCTxH <> Constant.DB_NULL Then
                minADisc = If(minADisc < adiscctxh, minADisc, adiscctxh)
            End If
            If nullADiscXTxH <> Constant.DB_NULL Then
                minADisc = If(minADisc < adiscxtxh, minADisc, adiscxtxh)
            End If

            If minADisc = Constant.MAXADISC Then
                anteNulls(TeAnte.ADISC_UTE) = Constant.DB_NULL
            Else
                anteStruct.adisc_ute = minADisc
                anteNulls(TeAnte.ADISC_UTE) = Constant.DB_NOT_NULL
            End If

            anteTabName = "sd_ante"

            ' get the antenna model, gain and xref for this ante 
            '&&Console.Error.Write("\nTgt06");
            nRet = Suutils.SuGetAnt(anteStruct.earthacode, pAnt)
            If nRet = Constant.SUCCESS Then
                If pAnt.acAnt.again <> 0.0 Then
                    anteStruct.earthagain = pAnt.acAnt.again
                    anteNulls(TeAnte.EARTHAGAIN) = Constant.DB_NOT_NULL
                End If

                If pAnt.acAnt.amodel.Length > 0 Then
                    anteStruct.earthamodel = pAnt.acAnt.amodel
                    anteNulls(TeAnte.EARTHAMODEL) = Constant.DB_NOT_NULL
                End If

                '	We have returned a cross reference code
                anteStruct.earthaxref = pAnt.acAnt.axref
                anteNulls(TeAnte.EARTHAXREF) = Constant.DB_NOT_NULL
            End If

            ' Rain scatter volume calculations 
            TpRunTsip.TeSubCalc.TeAnteRainCalcs(anteStruct, anteNulls, siteStruct, tuVec)

            ' 	We now should have the discrimination angle to the volume from the
            ' 		all the antenna directions.  If it is an off-axis antenna, we must
            ' 		calculate the off-axis discrimination (adisc_atv) 
            If Strings.FirstCharIs(anteStruct.tsoffaxis, "Y"c) AndAlso anteNulls(TeAnte.ANGLEATV) <> Constant.DB_NULL Then
                If CSharpImpl.__Assign(rc, TpGetDat.CalcDisc(pAnt, anteStruct.angleatv, adiscctxv, adiscxtxv, adiscctxh, adiscxtxh, nullADiscCTxV, nullADiscXTxV, nullADiscCTxH, nullADiscXTxH, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.TeAnteCalcs(): ERROR: B: call to CalcDisc() returned " & rc.ToString())
                    Return rc
                End If

                minADisc = Constant.MAXADISC
                If nullADiscCTxV <> Constant.DB_NULL Then
                    minADisc = If(minADisc < adiscctxv, minADisc, adiscctxv)
                End If
                If nullADiscXTxV <> Constant.DB_NULL Then
                    minADisc = If(minADisc < adiscxtxv, minADisc, adiscxtxv)
                End If
                If nullADiscCTxH <> Constant.DB_NULL Then
                    minADisc = If(minADisc < adiscctxh, minADisc, adiscctxh)
                End If
                If nullADiscXTxH <> Constant.DB_NULL Then
                    minADisc = If(minADisc < adiscxtxh, minADisc, adiscxtxh)
                End If

                If minADisc = Constant.MAXADISC Then
                    anteNulls(TeAnte.ADISC_ATV) = Constant.DB_NULL
                Else
                    anteStruct.adisc_atv = minADisc
                    anteNulls(TeAnte.ADISC_ATV) = Constant.DB_NOT_NULL
                End If
            End If

            '...Log2.v("\nTeCalcs.TeAnteCalcs(): Exit");
            Return Constant.SUCCESS
        End Function


        ''' <summary>
        ''' This method returns the height of the highest antenna, on the remote side of
        ''' a given local antenna, that the local antenna communicates with. 
        ''' </summary>
        ''' <remarks>
        ''' All the remote antennae from the MDB 
        ''' are determined along with all the remote antennae from within the PDF. The 
        ''' two lists are then merged with the antennae in the PDF taking precedence 
        ''' over those in the MDB. The reason for this is that the user may be adding a 
        ''' new antenna that is higher than any in the MDB or may be deleting the 
        ''' highest antennae in the MDB.  
        ''' </remarks>
        ''' <paramname="pdf"> - name of the PDF.</param>
        ''' <paramname="call1"> - local callsign.</param>
        ''' <paramname="call2"> - remote callsign.</param>
        ''' <paramname="bndcde"> - band code.</param>
        ''' <paramname="anum"> - antenna number.</param>
        ''' <returns></returns>
        Public Shared Function GetHighestAntenna(pdf As String, call1 As String, call2 As String, bndcde As String, anum As Short) As Double
            Dim dHt = -1.0F
            Dim dHt1 = -1.0F
            Dim cSelect As String
            Dim pdfName As String

            Dim sqlRet As SQLRETURN
            Dim hStmt As SQLHANDLE

            Dim hConn As SQLHDBC = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            ' 	Just get the highest antenna on this link at the other end 
            cSelect = SQLCHARPTR.Format("select max(aht) from main.mt_ante where call1='{0}' and call2='{1}' and bndcde='{2}' group by call1, call2, bndcde ", call2, call1, bndcde)

            sqlRet = ODBC.SQLExecDirect(hStmt, cSelect, cSelect.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Qutils.ExitQueue(Info.DbName, "READ")
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.GetHighestAntenna(): ERROR: call to SQLExecDirect() failed for:" & Microsoft.VisualBasic.Constants.vbLf & cSelect)
                Application.Exit(666)
            End If

            sqlRet = ODBC.SQLFetch(hStmt)

            If Not ODBC.IsOK(sqlRet) Then
                dHt = -1.0F
            Else
                Dim temp As Long
                Ssutil.DbGetFloat(hStmt, 1, "max(aht)", dHt, temp)
            End If

            ' We must close the cursor on the statement handle before reusing it.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE)

            ' It may also be in the pdf 
            If Not pdf.Equals("mt_ante") AndAlso pdf.Length > 0 Then
                ' Get the name of the antenna table 
                GenUtil.UtCvtName(Constant.FT_ANTE, pdf, pdfName)
                cSelect = SQLCHARPTR.Format("select max(aht) from {0} where call1='{1}' and call2='{2}' and bndcde='{3}' group by call1, call2, bndcde ", pdfName, call2, call1, bndcde)

                sqlRet = ODBC.SQLExecDirect(hStmt, cSelect, cSelect.Length)
                If Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.GetHighestAntenna(): ERROR: SQLExecDirect() failed for:" & Microsoft.VisualBasic.Constants.vbLf & cSelect)
                    Dim str = SQLCHARPTR.Format("getHighestAntenna02: Could not select from {0} for {1} {2} {3}.", pdfName, call2, call1, bndcde)
                    Ssutil.DbGetDiagStmt(hStmt, str)
                    Return -2.0
                End If

                sqlRet = ODBC.SQLFetch(hStmt)
                If ODBC.IsOK(sqlRet) Then
                    Dim temp As Long
                    Ssutil.DbGetFloat(hStmt, 1, "max(aht)", dHt1, temp)
                Else
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeCalcs.GetHighestAntenna(): ERROR: SQLFetch failed.")
                    dHt1 = -1.0F
                End If
            End If

            ' 	Return the highest antenna 
            If dHt1 > dHt Then
                dHt = dHt1
            End If

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            Ssutil.DisConn(hConn)

            Return dHt
        End Function

        ''' <summary>
        ''' Calculates the refracted elevation between two 
        ''' antennae given the distance between the sites, the elevation angle between 
        ''' the sites, and the heights of both antennae.  
        ''' </summary>
        ''' <paramname="dist"> - distance between sites.</param>
        ''' <paramname="elevAng"> - elevation angle.</param>
        ''' <paramname="antHt1Km"> - height of 1st. antenna in Km.</param>
        ''' <paramname="antHt2Km"> - height of 2nd. antenna in Km.</param>
        ''' <paramname="tmpElev"> - calculated refracted elevation.</param>
        Public Shared Sub CalcRefElev(dist As Double, elevAng As Double, antHt1Km As Double, antHt2Km As Double, <Out> ByRef tmpElev As Double)
            Dim phi As Double
            Dim rho As Double
            Dim chi As Double
            Dim sig As Double
            Dim del As Double
            Dim arf As Double         ' apparent earth radius factor 

            phi = AsinD(dist * CosD(elevAng) / (Constant.ERTHRD + antHt2Km))

            rho = Log(Constant.REF_INDEX / (Constant.REF_INDEX + -7.31 * Exp(0.005577 * Constant.REF_INDEX)))

            arf = Pow((1.0 - rho * Constant.REF_INDEX * Constant.ERTHRD * 0.000001) / (1.0 + Constant.REF_INDEX * 0.000001), -1.0)

            chi = rho * (arf - 1) * (Constant.ERTHRD + antHt1Km) / (12.0 * arf) + 1.0 / 3.0 * Pow((2.0 * arf + 1.0) / (2.0 * arf), Constant.SQUARE) - 0.5

            phi = phi * DEG_TO_RAD

            sig = 2.0 * arf * (antHt2Km - antHt1Km) - (Constant.ERTHRD + antHt1Km) * Pow(phi, Constant.SQUARE) * (1.0 + chi * Pow(phi, Constant.SQUARE))

            del = 2.0 * (Constant.ERTHRD + antHt1Km) * phi * arf * (1.0 + 2.0 * chi * Pow(phi, Constant.SQUARE))

            tmpElev = AtanD(sig / del)
        End Sub

        ' TOP

        ''' <summary>
        ''' This method performs calculations at the channel level to populate the following fields in 
        ''' the ES SH Chan Table: etreport, tereport, intfreqtx, vicfreqrx, 
        ''' reqd20mode1, reqd01mode1, reqd01mode2, terrmdsc, earthmdsc, terreirp, 
        ''' eartheirp, scang, loss20mode1, calci20mode1, loss01mode1, calci01mode1, 
        ''' loss01mode2, calci01mode2, marg20mode1, marg01mode1, marg01mode2, calctype. 
        ''' In addition, for P/C calculations, the following are also populated: ctxinttraftx, 
        ''' ctxvictrafrx, ctxeqpt, freqsep,.  
        ''' </summary>
        ''' <paramname="chanStruct"> - TeChan object.</param>
        ''' <paramname="anteStruct"> - TeAnte object.</param>
        ''' <paramname="siteStruct"> - TeSite object.</param>
        ''' <paramname="parmStruct"> - TpParm object providing parameter data.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function TeChanCalcs(ByRef chanStruct As TeChan, ByRef anteStruct As TeAnte, ByRef siteStruct As TeSite, parmStruct As TpParm, ByRef chanNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String) As Integer
            '...Log2.v("\nTeCalcs.TeChanCalcs(): Entry");
            '&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): eagle: anteStruct.earthacode = " + anteStruct.earthacode);

            Dim [select] = ""
            Dim remTerrACode As String
            Dim terrAnteTabName As String
            Dim earthAnteTabName As String
            Dim terrChanTabName As String
            Dim earthChanTabName As String
            Dim terrSiteTabName As String
            Dim earthSiteTabName As String
            Dim anteTabName As String
            Dim junkName As String

            Dim tmpG = 0
            Dim tmpGain = 0
            Dim rc As Integer

            Dim ttmpChanNulls As SQLLEN()  '[FtChan.SIZE_];
            Dim etmpChanNulls As SQLLEN()  '[FE_CHAN_SIZE_];
            Dim tmpAnteNulls As SQLLEN()   '[FE_ANTE_SIZE_];
            Dim ttAnteNulls As SQLLEN()    '[FtAnte.SIZE_];
            Dim direction As Short
            Dim nullRemTPowTx As SQLLEN = 0
            Dim isSDB As Short
            Dim terrMDB As Boolean
            Dim earthMDB As Boolean
            Dim ctxFlag As Short
            Dim tmpAnum As Short

            Dim nullEarthAfslr As SQLLEN
            Dim nullEarthAfslt As SQLLEN
            Dim nullEarthPowTx As SQLLEN
            Dim nullL01M2 As SQLLEN
            Dim nullTerrPowTx As SQLLEN
            Dim nullEarthPowRx As SQLLEN

            Dim terrAfsl As Double
            Dim terrPowTx As Double
            Dim earthPowTx = 0.0
            Dim earthPowRx As Double
            Dim remTerrPowTx As Double
            Dim earthAfslr As Double
            Dim earthAfslt As Double
            Dim patLossLink As Double
            Dim value As Double
            Dim junk As Double
            Dim remTerrAfslTx As Double
            Dim loss01mode2 As Double
            Dim fsepHi As Double
            Dim fsepMid As Double
            Dim fsepLo As Double
            Dim fsepMax As Double
            Dim cdist As Double
            Dim earthAGain As Double
            Dim terrAGain As Double
            Dim intMbnd As Double
            Dim vicMbnd As Double
            Dim intEqptStab As Double
            Dim vicEqptStab As Double
            Dim remTerrAGain As Double

            Dim ftTmpChan As FtChan
            Dim feTmpChan As FeChan
            Dim ftTmpAnte As FtAnte
            Dim feTmpAnte As FeAnte

            Dim cVicType As Char              ' 	Either 'A' or 'D' Victim 
            Dim cIntType As Char              ' 	Either 'A' or 'D' Interferor 
            ' 	Equipments.  Only two will be used.

            Dim tVicAnalog As TcTxAnalog
            Dim tVicDigital As TcTxDigital
            Dim tIntAnalog As TcTxAnalog
            Dim tIntDigital As TcTxDigital

            Dim reqLo As Double
            Dim reqMid As Double
            Dim reqHi As Double

            Dim tempeqpt As String
            Dim cTypeOfInt As Char

            Dim pAnt As SuAntStr
            Dim nRet As Integer

            Dim ecalctype As Enums.Ecalctype

            If Strings.FirstCharIs(chanStruct.interferer, "T"c) Then
                direction = Constant.TS_ES
            Else
                direction = Constant.ES_TS
            End If

            chanStruct.etreport = Constant.FALSE
            chanStruct.tereport = Constant.FALSE
            chanNulls(TeChan.ETREPORT) = Constant.DB_NOT_NULL
            chanNulls(TeChan.TEREPORT) = Constant.DB_NOT_NULL


            TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_SITE, terrSiteTabName, earthSiteTabName, junkName, terrMDB, earthMDB)


            TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_ANTE, terrAnteTabName, earthAnteTabName, junkName, terrMDB, earthMDB)


            TpRunTsip.TeSubCalc.TeTableNames(parmStruct, Constant.FT_CHAN, terrChanTabName, earthChanTabName, junkName, terrMDB, earthMDB)


            If direction = Constant.TS_ES Then
                If CSharpImpl.__Assign(rc, TpGetDat.TpCalcMbnds(chanStruct.terrbndcde, anteStruct.earthband, intMbnd, vicMbnd, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Return rc
                End If
            Else
                If CSharpImpl.__Assign(rc, TpGetDat.TpCalcMbnds(anteStruct.earthband, chanStruct.terrbndcde, intMbnd, vicMbnd, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                    Return rc
                End If
            End If

            ' check if local TS site is a passive reflector:
            '  if call1 begins with '%'
            ' 
            If Strings.FirstCharIs(anteStruct.terrcall1, "%"c) Then

                ' calculate terrestrial's passive reflector gain 
                If direction = Constant.TS_ES Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, terrMDB, terrAnteTabName, chanStruct.terrcall1, chanStruct.terrcall2, chanStruct.terrbndcde, chanStruct.terranum, intMbnd, anteStruct.tdiscang, junk, terrAGain, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, terrMDB, terrAnteTabName, chanStruct.terrcall1, chanStruct.terrcall2, chanStruct.terrbndcde, chanStruct.terranum, vicMbnd, anteStruct.tdiscang, junk, terrAGain, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Return rc
                End If ' terrestrial is not a passive repeater 
            Else

                anteTabName = "sd_ante"
                isSDB = Constant.TRUE

                rc = TpRunTsip.TeCalcs.SuAnteGet(anteStruct.terracode, anteTabName, isSDB, terrAGain)
                '&&Console.Error.Write("\nteCalcs.teChanCalcs(): apple");
                If rc <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                    Return rc
                End If
            End If

            If anteStruct.earthacode.StartsWith("CCIR") Then
                Dim nRes As Integer = TpRunTsip.TeCalcs.sf.Parse(anteStruct.earthacode, "%4s%2d%2d")
                Dim results As List(Of Object) = TpRunTsip.TeCalcs.sf.Results
                junkName = CStr(results(0))

                If direction = Constant.ES_TS Then
                    '&&Console.Error.Write("\nteCalcs.teChanCalcs(): bee");
                    ' positions 7&8 are the againTx 
                    tmpG = CInt(results(1))
                    tmpGain = CInt(results(2))  ' TS-ES 
                Else
                    '&&Console.Error.Write("\nteCalcs.teChanCalcs(): cow");
                    ' positions 5&6 are the againRx 
                    tmpG = CInt(results(2))
                    tmpGain = CInt(results(1))
                End If

                '&&Console.Error.Write("\njunkName = " + junkName);
                '&&Console.Error.Write("\ntmpG = " + tmpG);
                '&&Console.Error.Write("\ntmpGain = " + tmpGain);
                earthAGain = tmpGain
            Else
                '&&Console.Error.Write("\nteCalcs.teChanCalcs(): dingo");
                anteTabName = "sd_ante"
                isSDB = Constant.TRUE

                If CSharpImpl.__Assign(rc, TpRunTsip.TeCalcs.SuAnteGet(anteStruct.earthacode, anteTabName, isSDB, earthAGain)) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                    Return rc
                End If
            End If

            '&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): ferret: anteStruct.earthacode = " + anteStruct.earthacode);

            ' get local antenna's AFSL information, tx and rx 
            [select] = SQLCHARPTR.Format("location = '{0}' and call1 = '{1}'", anteStruct.earthlocation, anteStruct.earthcall1)

            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TeAnteGet([select], earthAnteTabName, earthMDB, feTmpAnte, tmpAnteNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                Return rc
            End If


            earthAfslr = feTmpAnte.afslr
            earthAfslt = feTmpAnte.afslt
            nullEarthAfslr = tmpAnteNulls(FeAnte.AFSLR)
            nullEarthAfslt = tmpAnteNulls(FeAnte.AFSLT)

            If direction = Constant.TS_ES Then
                chanStruct.vicrxafls = feTmpAnte.afslr
                chanNulls(TeChan.VICRXAFLS) = Constant.DB_NOT_NULL
            Else
                chanStruct.inttxafls = feTmpAnte.afslt
                chanNulls(TeChan.INTTXAFLS) = Constant.DB_NOT_NULL
            End If


            ' Get local Channel information
            '  NOTE: using the first channel fetched (arbitrary) since there are no
            '        default values & for BAND calcs there is no Chan level info
            ' 
            [select] = SQLCHARPTR.Format("location = '{0}' and call1 = '{1}'", chanStruct.earthlocation, chanStruct.earthcall1)
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TeChanGet([select], earthChanTabName, earthMDB, feTmpChan, etmpChanNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "CHAN", [select])
                Return rc
            End If


            ' BAND (ie no Chan info) use midband freq's for tx & rx frequencies 
            If parmStruct.analopt.Trim().Equals("BAND") Then
                chanStruct.intfreqtx = intMbnd
                chanStruct.vicfreqrx = vicMbnd
                chanStruct.remterracode = anteStruct.terracode
                chanStruct.energy = feTmpChan.p4khz
                chanNulls(TeChan.INTFREQTX) = Constant.DB_NOT_NULL
                chanNulls(TeChan.VICFREQRX) = Constant.DB_NOT_NULL
                chanNulls(TeChan.REMTERRACODE) = Constant.DB_NOT_NULL
                chanNulls(TeChan.ENERGY) = Constant.DB_NOT_NULL

                ' get the remote antenna model, gain and xref for this ante 
                nRet = Suutils.SuGetAnt(chanStruct.remterracode, pAnt)

                If nRet = 0 Then
                    If pAnt.acAnt.again <> 0.0 Then
                        chanStruct.remterragain = pAnt.acAnt.again
                        chanNulls(TeChan.REMTERRAGAIN) = Constant.DB_NOT_NULL
                    End If
                End If
            End If


            If etmpChanNulls(FeChan.MAXTXPOWER) <> Constant.DB_NULL AndAlso etmpChanNulls(FeChan.P4KHZ) <> Constant.DB_NULL Then
                earthPowTx = feTmpChan.maxtxpower - feTmpChan.p4khz
                nullEarthPowTx = Constant.DB_NOT_NULL
            Else
                nullEarthPowTx = Constant.DB_NULL
            End If
            terrPowTx = 13.0

            ' Get Rem.Terr.Stn Channel info 
            If Not parmStruct.analopt.Equals("BAND") Then

                If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtChanGet(chanStruct.terrcall2, chanStruct.terrcall1, chanStruct.terrbndcde, chanStruct.terrchid, terrChanTabName, terrMDB, ftTmpChan, ttmpChanNulls)) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "CHAN", [select])
                    Return rc
                End If


                If chanStruct.terrant = 1 Then
                    If ttmpChanNulls(FtChan.AFSLTX1) = Constant.DB_NULL Then
                        remTerrAfslTx = 0.0
                    Else
                        remTerrAfslTx = ftTmpChan.afsltx1
                    End If
                Else
                    If ttmpChanNulls(FtChan.AFSLTX2) = Constant.DB_NULL Then
                        remTerrAfslTx = 0.0
                    Else
                        remTerrAfslTx = ftTmpChan.afsltx2
                    End If
                End If

                remTerrPowTx = ftTmpChan.pwrtx
                nullRemTPowTx = ttmpChanNulls(FtChan.PWRTX)
                If direction = Constant.TS_ES Then
                    If ttmpChanNulls(FtChan.PWRTX) <> Constant.DB_NULL Then
                        chanStruct.inttxpwr2 = ftTmpChan.pwrtx
                        chanNulls(TeChan.INTTXPWR2) = Constant.DB_NOT_NULL
                        chanStruct.inttxafls2 = remTerrAfslTx
                        chanNulls(TeChan.INTTXAFLS2) = Constant.DB_NOT_NULL
                    End If
                End If
            End If

            '	Count for debugging.
            'glbTimesThrough++;

            If direction = Constant.ES_TS Then
                If siteStruct.intreq.Equals("FCSA") Then
                    chanStruct.reqd20mode1 = -158.0
                    chanStruct.reqd01mode1 = -138.0
                    chanStruct.reqd01mode2 = -138.0
                Else
                    chanStruct.reqd20mode1 = -151.0
                    chanStruct.reqd01mode1 = -137.0
                    chanStruct.reqd01mode2 = -137.0
                End If
                chanNulls(TeChan.REQD20MODE1) = Constant.DB_NOT_NULL
                chanNulls(TeChan.REQD01MODE1) = Constant.DB_NOT_NULL
                chanNulls(TeChan.REQD01MODE2) = Constant.DB_NOT_NULL
            Else

                '	If ES is receiving the interference, then the band analysis uses the
                '	i20, it01, and ip01 fields as the initial filter.  If these are absent
                '	as they can be, then a zero here means it will fail the filter and 
                '	never get to the full channel analysis.  So if these values are absent
                '	we set them to their defaults before using them. -- OEL - 2014-07-03
                '
                If etmpChanNulls(FeChan.I20) = Constant.DB_NULL Then
                    feTmpChan.i20 = -160.0F
                    etmpChanNulls(FeChan.I20) = Constant.DB_NOT_NULL
                End If
                If etmpChanNulls(FeChan.IT01) = Constant.DB_NULL Then
                    feTmpChan.it01 = -153.0F
                    etmpChanNulls(FeChan.IT01) = Constant.DB_NOT_NULL
                End If
                If etmpChanNulls(FeChan.IP01) = Constant.DB_NULL Then
                    feTmpChan.ip01 = -153.0F
                    etmpChanNulls(FeChan.IP01) = Constant.DB_NOT_NULL
                End If
                chanStruct.reqd20mode1 = feTmpChan.i20
                '...Log2.v(String.Format("\nA: reqd20mode1 = {0}", chanStruct.reqd20mode1));
                chanStruct.reqd01mode1 = feTmpChan.it01
                chanStruct.reqd01mode2 = feTmpChan.ip01
                chanNulls(TeChan.REQD20MODE1) = etmpChanNulls(FeChan.I20)
                chanNulls(TeChan.REQD01MODE1) = etmpChanNulls(FeChan.IT01)
                chanNulls(TeChan.REQD01MODE2) = etmpChanNulls(FeChan.IP01)
            End If

            ' --------------------
            ' MDSC && EIRP Calcs 

            chanStruct.terrmdsc = terrAGain - anteStruct.adisc_ute
            chanNulls(TeChan.TERRMDSC) = Constant.DB_NOT_NULL

            If direction = Constant.TS_ES Then
                If nullEarthAfslr <> Constant.DB_NULL Then
                    chanStruct.earthmdsc = earthAGain - anteStruct.adisc_set - earthAfslr
                    chanNulls(TeChan.EARTHMDSC) = Constant.DB_NOT_NULL
                Else
                    chanNulls(TeChan.EARTHMDSC) = Constant.DB_NULL
                End If
                chanStruct.terreirp = terrPowTx + chanStruct.terrmdsc
                chanNulls(TeChan.TERREIRP) = Constant.DB_NOT_NULL
            Else
                If nullEarthAfslt <> Constant.DB_NULL Then
                    chanStruct.earthmdsc = earthAGain - anteStruct.adisc_set - earthAfslt
                    chanNulls(TeChan.EARTHMDSC) = Constant.DB_NOT_NULL
                    If nullEarthPowTx <> Constant.DB_NULL Then
                        chanStruct.eartheirp = earthPowTx + chanStruct.earthmdsc
                        chanNulls(TeChan.EARTHEIRP) = Constant.DB_NOT_NULL
                    Else
                        chanNulls(TeChan.EARTHEIRP) = Constant.DB_NULL
                    End If
                Else
                    chanNulls(TeChan.EARTHMDSC) = Constant.DB_NULL
                    chanNulls(TeChan.EARTHEIRP) = Constant.DB_NULL
                End If
            End If

            '&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): bear: anteStruct.earthacode = " + anteStruct.earthacode);
            If CSharpImpl.__Assign(rc, TpRunTsip.TeSubCalc.TeCalcScang(parmStruct, siteStruct, anteStruct, chanStruct, chanNulls, earthAGain, terrAGain, earthAfslt, nullEarthAfslt, earthAfslr, nullEarthAfslr, 0.0, loss01mode2, nullL01M2, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                Return rc
            End If


            If anteStruct.mode1 = Constant.TRUE Then

                ' freq stored in KHz, CalcL20M1 uses MHz 
                TpRunTsip.TeSubCalc.TeCalcL20M1(siteStruct.etdist, siteStruct.radiozone, chanStruct.intfreqtx / 1000.0, anteStruct.earthht, anteStruct.terrht, parmStruct.spherecalc, chanStruct.loss20mode1)

                chanNulls(TeChan.LOSS20MODE1) = Constant.DB_NOT_NULL

                If chanNulls(TeChan.EARTHMDSC) <> Constant.DB_NULL Then
                    If direction = Constant.TS_ES Then
                        chanStruct.calci20mode1 = chanStruct.terreirp + chanStruct.earthmdsc - chanStruct.loss20mode1
                        chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NOT_NULL
                    Else
                        If chanNulls(TeChan.EARTHEIRP) <> Constant.DB_NULL Then
                            chanStruct.calci20mode1 = chanStruct.terrmdsc + chanStruct.eartheirp - chanStruct.loss20mode1
                            chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NOT_NULL
                        Else
                            chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                        End If
                    End If
                Else
                    chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                End If

                ' freq stored in KHz, CalcL01M1 uses MHz 
                TpRunTsip.TeSubCalc.TeCalcL01M1(siteStruct.radiozone, chanStruct.loss20mode1, chanStruct.intfreqtx / 1000.0, anteStruct.etelev, siteStruct.etdist, direction, chanStruct.loss01mode1)
                chanNulls(TeChan.LOSS01MODE1) = Constant.DB_NOT_NULL


                If chanNulls(TeChan.EARTHMDSC) <> Constant.DB_NULL Then
                    If direction = Constant.TS_ES Then
                        chanStruct.calci01mode1 = chanStruct.terreirp + chanStruct.earthmdsc - chanStruct.loss01mode1
                        chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NOT_NULL
                    Else
                        If chanNulls(TeChan.EARTHEIRP) <> Constant.DB_NULL Then
                            chanStruct.calci01mode1 = chanStruct.terrmdsc + chanStruct.eartheirp - chanStruct.loss01mode1
                            chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NOT_NULL
                        Else
                            chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                        End If
                    End If
                Else
                    chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                End If
            End If

            If anteStruct.mode2 = Constant.TRUE Then

                chanStruct.loss01mode2 = loss01mode2
                chanNulls(TeChan.LOSS01MODE2) = nullL01M2

                If chanNulls(TeChan.LOSS01MODE2) <> Constant.DB_NULL Then
                    If direction = Constant.TS_ES Then
                        chanStruct.calci01mode2 = terrPowTx - chanStruct.loss01mode2
                        chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NOT_NULL
                    Else
                        If nullEarthPowTx <> Constant.DB_NULL Then
                            chanStruct.calci01mode2 = earthPowTx - chanStruct.loss01mode2
                            chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NOT_NULL
                        Else
                            chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                        End If
                    End If
                Else
                    chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                End If
            End If

            If anteStruct.mode1 = Constant.TRUE Then
                If chanNulls(TeChan.CALCI20MODE1) <> Constant.DB_NULL AndAlso chanNulls(TeChan.REQD20MODE1) <> Constant.DB_NULL Then
                    chanStruct.marg20mode1 = chanStruct.reqd20mode1 - chanStruct.calci20mode1
                    chanNulls(TeChan.MARG20MODE1) = Constant.DB_NOT_NULL
                Else
                    chanNulls(TeChan.MARG20MODE1) = Constant.DB_NULL
                End If

                If chanNulls(TeChan.CALCI01MODE1) <> Constant.DB_NULL AndAlso chanNulls(TeChan.REQD01MODE1) <> Constant.DB_NULL Then
                    chanStruct.marg01mode1 = chanStruct.reqd01mode1 - chanStruct.calci01mode1
                    chanNulls(TeChan.MARG01MODE1) = Constant.DB_NOT_NULL
                Else
                    chanNulls(TeChan.MARG01MODE1) = Constant.DB_NULL
                End If
            End If

            If anteStruct.mode2 = Constant.TRUE Then
                If chanNulls(TeChan.CALCI01MODE2) <> Constant.DB_NULL AndAlso chanNulls(TeChan.REQD01MODE2) <> Constant.DB_NULL Then
                    chanStruct.marg01mode2 = chanStruct.reqd01mode2 - chanStruct.calci01mode2
                    chanNulls(TeChan.MARG01MODE2) = Constant.DB_NOT_NULL
                Else
                    chanNulls(TeChan.MARG01MODE2) = Constant.DB_NULL
                End If
            End If


            ' if anyone one of  the margins is less than the paramter.margin
            '  then continue with PC calcs
            '  	ie. if mode1 and (either marg20mode1<parm.margin or
            ' 	    marg01mode1<parm.margin)
            ' 	    or mode2 and (marg01mode2<parm.margin)
            ' 	then continue with P/C calcs
            ' 	else return and go on to the next channel
            ' 


            ' BAND interference was found; continue with P/C calculations
            If chanNulls(TeChan.MARG20MODE1) <> Constant.DB_NULL AndAlso chanStruct.marg20mode1 < parmStruct.margin AndAlso anteStruct.mode1 = Constant.TRUE OrElse chanNulls(TeChan.MARG01MODE1) <> Constant.DB_NULL AndAlso chanStruct.marg01mode1 < parmStruct.margin AndAlso anteStruct.mode1 = Constant.TRUE OrElse chanNulls(TeChan.MARG01MODE2) <> Constant.DB_NULL AndAlso chanStruct.marg01mode2 < parmStruct.margin AndAlso anteStruct.mode2 = Constant.TRUE Then
            Else
                ' no BAND interference; no P/C calculations will be done 
                Return Constant.PC_SKIP
            End If

            If parmStruct.analopt.Equals("BAND") Then
                If direction = Constant.TS_ES Then
                    chanStruct.tereport = Constant.TRUE
                Else
                    chanStruct.etreport = Constant.TRUE
                End If
                chanStruct.calctype = "-I"
                chanNulls(TeChan.CALCTYPE) = Constant.DB_NOT_NULL
                Return Constant.PC_SKIP
            End If

            ' @@@@@@@@@@@@@  Begin Plan/Channel Calculations @@@@@@@@@@@@@@@@@@

            ' Get Terr.Stn Channel info needed for calcs from pdf or mdb tables 
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtChanGet(chanStruct.terrcall1, chanStruct.terrcall2, chanStruct.terrbndcde, chanStruct.terrchid, terrChanTabName, terrMDB, ftTmpChan, ttmpChanNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                If rc = Constant.FAILURE Then
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "CHAN", [select])
                End If
                Return rc
            End If


            If direction = Constant.TS_ES Then
                If chanStruct.terrant = 1 Then
                    terrAfsl = ftTmpChan.afsltx1
                    If ttmpChanNulls(FtChan.AFSLTX1) = Constant.DB_NULL Then
                        terrAfsl = 0.0
                    End If
                    tmpAnum = ftTmpChan.antnumbtx1
                Else
                    terrAfsl = ftTmpChan.afsltx2
                    If ttmpChanNulls(FtChan.AFSLTX2) = Constant.DB_NULL Then
                        terrAfsl = 0.0
                    End If
                    tmpAnum = ftTmpChan.antnumbtx2
                End If   ' ES-TS 
            Else
                If chanStruct.terrant = 1 Then
                    terrAfsl = ftTmpChan.afslrx1
                    If ttmpChanNulls(FtChan.AFSLRX1) = Constant.DB_NULL Then
                        terrAfsl = 0.0
                    End If
                    tmpAnum = ftTmpChan.antnumbrx1
                ElseIf chanStruct.terrant = 2 Then
                    terrAfsl = ftTmpChan.afslrx2
                    If ttmpChanNulls(FtChan.AFSLRX2) = Constant.DB_NULL Then
                        terrAfsl = 0.0
                    End If
                    tmpAnum = ftTmpChan.antnumbrx2
                Else
                    terrAfsl = ftTmpChan.afslrx3
                    If ttmpChanNulls(FtChan.AFSLRX3) = Constant.DB_NULL Then
                        terrAfsl = 0.0
                    End If
                    tmpAnum = ftTmpChan.antnumbrx3
                End If
            End If
            terrPowTx = ftTmpChan.pwrtx
            nullTerrPowTx = ttmpChanNulls(FtChan.PWRTX)

            ' 	Now get the remote end antenna to get the values for the
            ' recalculation of the RX power at this end (gain and fsl)
            ' GJS (1042) 
            [select] = SQLCHARPTR.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and ( ause = 'TX ' or ause = 'TR ')", ftTmpChan.call2, ftTmpChan.call1, ftTmpChan.bndcde)
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtAnteGetCond([select], terrAnteTabName, terrMDB, ftTmpAnte, ttAnteNulls)) <> Constant.SUCCESS Then
                ' If we fail it is probably because there are only RX antennas
                ' at the other end.  Get any antenna. 
                [select] = SQLCHARPTR.Format("call1 = '{0,-9}' and call2 = '{1,-9}' and bndcde = '{2,-4}' ", ftTmpChan.call2, ftTmpChan.call1, ftTmpChan.bndcde)
                If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtAnteGetCond([select], terrAnteTabName, terrMDB, ftTmpAnte, ttAnteNulls)) <> Constant.SUCCESS Then
                    ' True Failure.  Message out. 
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                    Return rc
                End If
            End If


            remTerrACode = ftTmpAnte.acode
            chanStruct.remterracode = ftTmpAnte.acode
            chanNulls(TeChan.REMTERRACODE) = Constant.DB_NOT_NULL

            If chanNulls(TeChan.REMTERRACODE) <> Constant.DB_NULL Then
                '&&Console.Error.Write("\nTgt08");
                ' get the remote antenna model, gain and xref for this ante 
                nRet = Suutils.SuGetAnt(chanStruct.remterracode, pAnt)
                If nRet = 0 Then
                    If pAnt.acAnt.again <> 0.0 Then
                        chanStruct.remterragain = pAnt.acAnt.again
                        chanNulls(TeChan.REMTERRAGAIN) = Constant.DB_NOT_NULL
                    End If
                End If
            End If

            ' check if remote TS site is a passive reflector:
            '  if call2 begins with '%'	 
            If Strings.FirstCharIs(anteStruct.terrcall2, "%"c) Then

                ' calculate terrestrial's remote passive reflector gain 
                If direction = Constant.TS_ES Then
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(True, terrMDB, terrAnteTabName, chanStruct.terrcall2, chanStruct.terrcall1, chanStruct.terrbndcde, 0, intMbnd, anteStruct.tdiscang, junk, remTerrAGain, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TtCalkPassive.TtCalcPassive(False, terrMDB, terrAnteTabName, chanStruct.terrcall2, chanStruct.terrcall1, chanStruct.terrbndcde, 0, vicMbnd, anteStruct.tdiscang, junk, remTerrAGain, intPrintMsg, vicPrintMsg)
                End If
                If rc <> Constant.SUCCESS Then
                    Return rc
                End If ' terrestrial is not a passive repeater 
            Else


                anteTabName = "sd_ante"
                isSDB = Constant.TRUE

                If CSharpImpl.__Assign(rc, TpRunTsip.TeCalcs.SuAnteGet(remTerrACode, anteTabName, isSDB, remTerrAGain)) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "ANTE", [select])
                    Return rc
                End If
            End If

            ' Get Earth Stn Channel info needed for calcs from pdf or mdb tables 
            [select] = SQLCHARPTR.Format("location = '{0}' and call1 = '{1}' and chid = '{2}'", chanStruct.earthlocation, chanStruct.earthcall1, chanStruct.earthchid)
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TeChanGet([select], earthChanTabName, earthMDB, feTmpChan, etmpChanNulls)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "CHAN", [select])
                Return rc
            End If

            ' 	Check to see if the powers are null before assigning them	|OEL TSIP ES Bug 1 
            If CSharpImpl.__Assign(nullEarthPowTx, etmpChanNulls(FeChan.PWRTX)) = Constant.DB_NULL Then
                earthPowTx = 0.0
            Else
                earthPowTx = feTmpChan.pwrtx
            End If
            If CSharpImpl.__Assign(nullEarthPowRx, etmpChanNulls(FeChan.PWRRX)) = Constant.DB_NULL Then
                earthPowRx = 0.0
            Else
                earthPowRx = feTmpChan.pwrrx
            End If


            If CSharpImpl.__Assign(rc, TpGetDat.TpGetEqptStab(chanStruct.inteqpttx, chanStruct.viceqptrx, parmStruct.tempequip, intEqptStab, vicEqptStab, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                If rc <> Constant.FAILURE Then
                    Return rc
                End If
                ctxFlag = Constant.FALSE
            Else
                ctxFlag = Constant.TRUE
            End If

            fsepMid = Math.Abs(chanStruct.intfreqtx - chanStruct.vicfreqrx)

            fsepHi = fsepMid + (intEqptStab * intMbnd + vicEqptStab * vicMbnd)

            fsepLo = fsepMid - (intEqptStab * intMbnd + vicEqptStab * vicMbnd)
            fsepLo = Max(0.0, fsepLo)  ' 	If negative, assume zero 
            ' 
            '  TASK 466: Maximum frequency separation is now set in the
            '  parameters by the user, and a choice between this and the
            '  default is made by tpMaxFSep.
            ' 

            cdist = anteStruct.txpre
            If cdist < anteStruct.txtro Then cdist = anteStruct.txtro
            If cdist < anteStruct.rxpre Then cdist = anteStruct.rxpre
            If cdist < anteStruct.rxtro Then cdist = anteStruct.rxtro


            fsepMax = TpGetDat.TpMaxFSep(parmStruct.fsep, cdist, siteStruct.etdist, chanStruct.inttraftx, chanStruct.victrafrx)

            If fsepHi >= fsepMax AndAlso fsepLo >= fsepMax Then
                ' not a fatal error but this case not in contention 
                Return Constant.SUCCESS
            End If

            If TpRunTsip.TpRunTsip.GlbIsCtxCalc = 0 Then
                If Not TpRunTsip.TeCalcs.oldTrafTx.Equals(chanStruct.inttraftx) OrElse Not TpRunTsip.TeCalcs.oldTrafRx.Equals(chanStruct.victrafrx) OrElse Not TpRunTsip.TeCalcs.oldEqptTx.Equals(chanStruct.inteqpttx) OrElse Not TpRunTsip.TeCalcs.oldEqptRx.Equals(chanStruct.viceqptrx) Then
                    TpRunTsip.TeCalcs.oldTrafTx = chanStruct.inttraftx
                    TpRunTsip.TeCalcs.oldTrafRx = chanStruct.victrafrx
                    TpRunTsip.TeCalcs.oldEqptTx = chanStruct.inteqpttx
                    TpRunTsip.TeCalcs.oldEqptRx = chanStruct.viceqptrx



                    '''...Log2.v(curCtx.ToString());

                    rc = TpGetDat.TpGetCtxInfo(chanStruct.inttraftx, chanStruct.victrafrx, chanStruct.viceqptrx, parmStruct.tempequip, TpRunTsip.TeCalcs.curCtx, intPrintMsg, vicPrintMsg)
                Else
                    rc = TpRunTsip.TeCalcs.oldRc
                End If

                TpRunTsip.TeCalcs.oldRc = rc
                If rc <> Constant.SUCCESS Then
                    If rc = [Error].DYN_MS_SQL_SERVER_ERR Then
                        Return [Error].DYN_MS_SQL_SERVER_ERR
                    End If
                    rc = Constant.FAILURE
                End If
            Else
                ' 	This will cause the calculation routines to be used 
                rc = Constant.FAILURE
            End If

            If rc = Constant.SUCCESS Then
                ' 	CTX curve found.  use it for the parameters. 
                chanStruct.ctxinttraftx = TpRunTsip.TeCalcs.curCtx.ctxtraftx
                chanStruct.ctxvictrafrx = TpRunTsip.TeCalcs.curCtx.ctxtrafrx
                chanStruct.ctxeqpt = TpRunTsip.TeCalcs.curCtx.ctxeqpt
                chanStruct.calctype = TpRunTsip.TeCalcs.curCtx.calcType
                chanNulls(TeChan.CTXINTTRAFTX) = Constant.DB_NOT_NULL
                chanNulls(TeChan.CTXVICTRAFRX) = Constant.DB_NOT_NULL
                chanNulls(TeChan.CTXEQPT) = Constant.DB_NOT_NULL
                chanNulls(TeChan.CALCTYPE) = Constant.DB_NOT_NULL

                If chanStruct.calctype.Equals("-I") Then
                    ecalctype = Enums.Ecalctype.eMINUSI
                Else
                    ecalctype = Enums.Ecalctype.eCOVERI
                End If

                If ctxFlag = Constant.TRUE Then
                    ' 	If the equipments were found ... 
                    TpGetDat.TpCalcSepReqd(chanStruct.calctype, TpRunTsip.TeCalcs.curCtx.ctxPts, fsepHi, fsepMid, fsepLo, TpRunTsip.TeCalcs.curCtx.numPts, chanStruct.freqsep, value)
                    If ecalctype = Enums.Ecalctype.eMINUSI Then
                        ' convert to dbW 
                        value = value - 30.0
                        '...Log2.v(String.Format("\nA: value = {0}", value));
                    End If
                Else
                    chanStruct.freqsep = fsepHi / 100.0

                    If chanStruct.freqsep / 1000.0 <= 500.0 Then
                        value = 95.0
                    Else
                        value = 0.0
                    End If
                End If
                '...Log2.v(String.Format("\nB: value = {0}", value));

                chanNulls(TeChan.FREQSEP) = Constant.DB_NOT_NULL
            Else
                ' 	Could not find the CTX curve, use the calculations.  First get the
                ' 		appropriate equipment/traffic crossreferences. 

                rc = GetCtx.GetCtxEqpt(chanStruct.victrafrx, chanStruct.viceqptrx, chanStruct.inttraftx, chanStruct.inteqpttx, cVicType, cIntType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital)

                '''...Log2.v("\n\n{0}  {1}", cVicType, cIntType);
                '''...Log2.v(tVicAnalog.ToString());
                '''...Log2.v(tVicDigital.ToString());
                '''...Log2.v(tIntAnalog.ToString());
                '''...Log2.v(tIntDigital.ToString());

                If rc = 0 Then
                    ' 	We got the crossreferences, first store the crossreference codes,
                    ' 		then calculate the requirements 
                    GetCtx.XrefCodes(chanStruct.inttraftx, chanStruct.inteqpttx, cIntType, tIntAnalog, tIntDigital, chanStruct.ctxinttraftx, tempeqpt)

                    GetCtx.XrefCodes(chanStruct.victrafrx, chanStruct.viceqptrx, cVicType, tVicAnalog, tVicDigital, chanStruct.ctxvictrafrx, chanStruct.ctxeqpt)

                    chanNulls(TeChan.CTXINTTRAFTX) = Constant.DB_NOT_NULL
                    chanNulls(TeChan.CTXVICTRAFRX) = Constant.DB_NOT_NULL
                    chanNulls(TeChan.CTXEQPT) = Constant.DB_NOT_NULL

                    ' 	Get the three requirements for the three frequencies centered around
                    ' 		the nominal frequency separation (+ / - the stabilities). 
                    GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, fsepHi / 1000.0, direction = Constant.TS_ES, cTypeOfInt, reqHi)

                    GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, fsepMid / 1000.0, direction = Constant.TS_ES, cTypeOfInt, reqMid)

                    GetCtx.GetCtxReq(cIntType, cVicType, tVicAnalog, tVicDigital, tIntAnalog, tIntDigital, fsepLo / 1000.0, direction = Constant.TS_ES, cTypeOfInt, reqLo)

                    '...Log2.v(String.Format("\nreqHi = {0}", reqHi));
                    '...Log2.v(String.Format("\nreqMid = {0}", reqMid));
                    '...Log2.v(String.Format("\nreqLo = {0}", reqLo));

                    ' 	Choose which of these is used depending on the type of interference 
                    If cTypeOfInt = "I"c Then
                        ' 	Interference only.  Choose the most negative 
                        If reqHi <= reqMid AndAlso reqHi <= reqLo Then
                            chanStruct.freqsep = fsepHi
                            value = reqHi
                        ElseIf reqLo <= reqMid AndAlso reqLo <= reqHi Then
                            chanStruct.freqsep = fsepLo
                            value = reqLo
                        Else
                            chanStruct.freqsep = fsepMid
                            value = reqMid
                        End If
                        ' convert to dbW 
                        value = value - 30.0

                        chanStruct.calctype = "I"
                        '...Log2.v(String.Format("\nC: value = {0}", value));
                        ecalctype = Enums.Ecalctype.eMINUSI
                    Else
                        If reqHi >= reqMid AndAlso reqHi >= reqLo Then
                            chanStruct.freqsep = fsepHi
                            '...Log2.v(String.Format("\nD-1: value = {0}", value));
                            value = reqHi
                        ElseIf reqLo >= reqMid AndAlso reqLo >= reqHi Then
                            chanStruct.freqsep = fsepLo
                            '...Log2.v(String.Format("\nD-2: value = {0}", value));
                            value = reqLo
                        Else
                            chanStruct.freqsep = fsepMid
                            value = reqMid
                            '...Log2.v(String.Format("\nD-3: value = {0}", value));
                        End If
                        chanStruct.calctype = "C"
                        ecalctype = Enums.Ecalctype.eCOVERI
                    End If

                    chanNulls(TeChan.CALCTYPE) = Constant.DB_NOT_NULL
                    chanNulls(TeChan.FREQSEP) = Constant.DB_NOT_NULL
                Else
                    ' 	Could not get the equipment 
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "Could not retrieve traffic/equipment cross reference ({0}):-" & Microsoft.VisualBasic.Constants.vbLf & "  %s into %s" & Microsoft.VisualBasic.Constants.vbLf & "%s/%s into %s/%s" & Microsoft.VisualBasic.Constants.vbLf, rc, intPrintMsg, vicPrintMsg, chanStruct.inttraftx, chanStruct.inteqpttx, chanStruct.victrafrx, chanStruct.viceqptrx)
                    Return Constant.SUCCESS
                End If
            End If


            ' ----------------------------------------
            ' have all the data - start Calculations 

            ' --------------------
            ' MDSC && EIRP Calcs 
            chanStruct.terrmdsc = terrAGain - anteStruct.adisc_ute - terrAfsl
            chanNulls(TeChan.TERRMDSC) = Constant.DB_NOT_NULL

            If direction = Constant.TS_ES Then
                If nullTerrPowTx <> Constant.DB_NULL Then
                    chanStruct.terreirp = terrPowTx - 30.0 + chanStruct.terrmdsc
                    chanNulls(TeChan.TERREIRP) = Constant.DB_NOT_NULL
                Else
                    chanNulls(TeChan.TERREIRP) = Constant.DB_NULL
                End If
                chanNulls(TeChan.EARTHEIRP) = Constant.DB_NULL
            Else
                If chanNulls(TeChan.EARTHMDSC) <> Constant.DB_NULL AndAlso nullEarthPowTx <> Constant.DB_NULL Then
                    chanStruct.eartheirp = earthPowTx + chanStruct.earthmdsc
                    chanNulls(TeChan.EARTHEIRP) = Constant.DB_NOT_NULL
                Else
                    chanNulls(TeChan.EARTHEIRP) = Constant.DB_NULL
                End If
                chanNulls(TeChan.TERREIRP) = Constant.DB_NULL
            End If

            '&&Console.Error.Write("\nTeCalcs.TeChanCalcs(): crab: anteStruct.earthacode = " + anteStruct.earthacode);
            If CSharpImpl.__Assign(rc, TpRunTsip.TeSubCalc.TeCalcScang(parmStruct, siteStruct, anteStruct, chanStruct, chanNulls, earthAGain, terrAGain, earthAfslt, nullEarthAfslt, earthAfslr, nullEarthAfslr, terrAfsl, loss01mode2, nullL01M2, intPrintMsg, vicPrintMsg)) <> Constant.SUCCESS Then
                Return rc
            End If


            GenUtil.FreeSpacePathLoss(siteStruct.tudist, chanStruct.intfreqtx / 1000.0, patLossLink)

            If anteStruct.mode1 = Constant.TRUE Then
                ' freq's stored in KHz, CalcL20M1 uses MHz 
                TpRunTsip.TeSubCalc.TeCalcL20M1(siteStruct.etdist, siteStruct.radiozone, chanStruct.intfreqtx / 1000.0, anteStruct.earthht, anteStruct.terrht, parmStruct.spherecalc, chanStruct.loss20mode1)

                chanNulls(TeChan.LOSS20MODE1) = Constant.DB_NOT_NULL

                ' freq's stored in KHz, CalcL01M1 uses MHz 
                TpRunTsip.TeSubCalc.TeCalcL01M1(siteStruct.radiozone, chanStruct.loss20mode1, chanStruct.intfreqtx / 1000.0, anteStruct.etelev, siteStruct.etdist, direction, chanStruct.loss01mode1)

                chanNulls(TeChan.LOSS01MODE1) = Constant.DB_NOT_NULL

                If chanNulls(TeChan.EARTHMDSC) <> Constant.DB_NULL Then
                    If direction = Constant.TS_ES Then
                        If nullTerrPowTx <> Constant.DB_NULL AndAlso chanNulls(TeChan.TERREIRP) <> Constant.DB_NULL Then
                            chanStruct.calci20mode1 = chanStruct.terreirp + chanStruct.earthmdsc - chanStruct.loss20mode1
                            chanStruct.calci01mode1 = chanStruct.terreirp + chanStruct.earthmdsc - chanStruct.loss01mode1
                            If ecalctype = Enums.Ecalctype.eCOVERI Then
                                If nullEarthPowRx <> Constant.DB_NULL Then
                                    chanStruct.calci20mode1 = earthPowRx - chanStruct.calci20mode1

                                    chanStruct.calci01mode1 = earthPowRx - chanStruct.calci01mode1
                                    chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NOT_NULL
                                    chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NOT_NULL
                                Else
                                    chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                                    chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                                End If
                            Else
                                chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NOT_NULL
                                chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NOT_NULL
                            End If
                        Else
                            chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                            chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                        End If
                    Else
                        If chanNulls(TeChan.EARTHEIRP) <> Constant.DB_NULL AndAlso chanNulls(TeChan.TERRMDSC) <> Constant.DB_NULL Then
                            chanStruct.calci20mode1 = chanStruct.terrmdsc + chanStruct.eartheirp - chanStruct.loss20mode1
                            chanStruct.calci01mode1 = chanStruct.terrmdsc + chanStruct.eartheirp - chanStruct.loss01mode1

                            If ecalctype = Enums.Ecalctype.eCOVERI Then
                                If nullRemTPowTx <> Constant.DB_NULL Then
                                    chanStruct.calci20mode1 = chanStruct.vicpwrrx - 30.0 - chanStruct.calci20mode1
                                    chanStruct.calci01mode1 = chanStruct.vicpwrrx - 30.0 - chanStruct.calci01mode1
                                    chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NOT_NULL
                                    chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NOT_NULL
                                Else
                                    chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                                    chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                                End If
                            Else
                                chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NOT_NULL
                                chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NOT_NULL
                            End If
                        Else
                            chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                            chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                        End If
                    End If
                Else
                    chanNulls(TeChan.CALCI20MODE1) = Constant.DB_NULL
                    chanNulls(TeChan.CALCI01MODE1) = Constant.DB_NULL
                End If


                If ecalctype = Enums.Ecalctype.eCOVERI Then
                    If siteStruct.intreq.Equals("FCSA") Then
                        If direction = Constant.TS_ES Then
                            chanStruct.reqd20mode1 = value - 17.47
                            chanStruct.reqd01mode1 = value - 35.54
                        Else
                            chanStruct.reqd20mode1 = value - 13.49
                            chanStruct.reqd01mode1 = value - 33.49
                        End If
                    Else
                        If direction = Constant.TS_ES Then
                            chanStruct.reqd20mode1 = value - 21.73
                            chanStruct.reqd01mode1 = value - 40.50
                        Else
                            chanStruct.reqd20mode1 = value - 23.49
                            chanStruct.reqd01mode1 = value - 37.49
                        End If
                    End If
                Else
                    chanStruct.reqd20mode1 = value
                    chanStruct.reqd01mode1 = value
                End If
                chanNulls(TeChan.REQD20MODE1) = Constant.DB_NOT_NULL
                chanNulls(TeChan.REQD01MODE1) = Constant.DB_NOT_NULL

                If chanNulls(TeChan.CALCI20MODE1) <> Constant.DB_NULL Then
                    If ecalctype = Enums.Ecalctype.eCOVERI Then
                        chanStruct.marg20mode1 = chanStruct.calci20mode1 - chanStruct.reqd20mode1
                        chanNulls(TeChan.MARG20MODE1) = Constant.DB_NOT_NULL
                        If chanStruct.marg20mode1 < parmStruct.margin Then
                            If direction = Constant.TS_ES Then
                                chanStruct.tereport = Constant.TRUE
                            Else
                                chanStruct.etreport = Constant.TRUE
                            End If
                        End If
                    Else
                        chanStruct.marg20mode1 = chanStruct.reqd20mode1 - chanStruct.calci20mode1
                        chanNulls(TeChan.MARG20MODE1) = Constant.DB_NOT_NULL
                        If chanStruct.marg20mode1 < parmStruct.margin Then
                            If direction = Constant.TS_ES Then
                                chanStruct.tereport = Constant.TRUE
                            Else
                                chanStruct.etreport = Constant.TRUE
                            End If
                        End If
                    End If
                Else
                    chanNulls(TeChan.MARG20MODE1) = Constant.DB_NULL
                End If

                If chanNulls(TeChan.CALCI01MODE1) <> Constant.DB_NULL Then
                    If ecalctype = Enums.Ecalctype.eCOVERI Then
                        chanStruct.marg01mode1 = chanStruct.calci01mode1 - chanStruct.reqd01mode1
                        chanNulls(TeChan.MARG01MODE1) = Constant.DB_NOT_NULL
                        If chanStruct.marg01mode1 < parmStruct.margin Then
                            If direction = Constant.TS_ES Then
                                chanStruct.tereport = Constant.TRUE
                            Else
                                chanStruct.etreport = Constant.TRUE
                            End If
                        End If
                    Else
                        chanStruct.marg01mode1 = chanStruct.reqd01mode1 - chanStruct.calci01mode1
                        chanNulls(TeChan.MARG01MODE1) = Constant.DB_NOT_NULL
                        If chanStruct.marg01mode1 < parmStruct.margin Then
                            If direction = Constant.TS_ES Then
                                chanStruct.tereport = Constant.TRUE
                            Else
                                chanStruct.etreport = Constant.TRUE
                            End If
                        End If
                    End If
                Else
                    chanNulls(TeChan.MARG01MODE1) = Constant.DB_NULL
                End If
            End If


            If anteStruct.mode2 = Constant.TRUE Then
                chanStruct.loss01mode2 = loss01mode2
                chanNulls(TeChan.LOSS01MODE2) = nullL01M2

                If chanNulls(TeChan.LOSS01MODE2) <> Constant.DB_NULL Then
                    If direction = Constant.TS_ES Then
                        If nullTerrPowTx <> Constant.DB_NULL Then
                            chanStruct.calci01mode2 = terrPowTx - 30.0 - chanStruct.loss01mode2
                            If ecalctype = Enums.Ecalctype.eCOVERI Then
                                If nullEarthPowRx <> Constant.DB_NULL Then
                                    chanStruct.calci01mode2 = earthPowRx - chanStruct.calci01mode2
                                    chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NOT_NULL
                                Else
                                    chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                                End If
                            Else
                                chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NOT_NULL
                            End If
                        Else
                            chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                        End If
                    Else
                        If nullEarthPowTx <> Constant.DB_NULL Then
                            chanStruct.calci01mode2 = earthPowTx - chanStruct.loss01mode2
                            If ecalctype = Enums.Ecalctype.eCOVERI Then
                                If nullRemTPowTx <> Constant.DB_NULL Then
                                    chanStruct.calci01mode2 = chanStruct.vicpwrrx - chanStruct.calci01mode2 ' GJS - 150723A - 2015-12-23
                                    chanStruct.calci01mode2 += Constant.DBM_TO_DBW_CONVERSION                          ' AH  - 160627A - 2016-06-27
                                    chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NOT_NULL
                                Else
                                    chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                                End If
                            Else
                                chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NOT_NULL
                            End If
                        Else
                            chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                        End If
                    End If
                Else
                    chanNulls(TeChan.CALCI01MODE2) = Constant.DB_NULL
                End If

                If ecalctype = Enums.Ecalctype.eCOVERI Then
                    If siteStruct.intreq.Equals("FCSA") Then
                        If direction = Constant.TS_ES Then
                            chanStruct.reqd01mode2 = value - 35.54
                        Else
                            chanStruct.reqd01mode2 = value - 33.49
                        End If
                    Else
                        If direction = Constant.TS_ES Then
                            chanStruct.reqd01mode2 = value - 40.50
                        Else
                            chanStruct.reqd01mode2 = value - 37.49
                        End If
                    End If
                Else
                    chanStruct.reqd01mode2 = value
                End If
                chanNulls(TeChan.REQD01MODE2) = Constant.DB_NOT_NULL

                If chanNulls(TeChan.CALCI01MODE2) <> Constant.DB_NULL Then
                    If ecalctype = Enums.Ecalctype.eCOVERI Then
                        chanStruct.marg01mode2 = chanStruct.calci01mode2 - chanStruct.reqd01mode2
                        chanNulls(TeChan.MARG01MODE2) = Constant.DB_NOT_NULL
                        If chanStruct.marg01mode2 < parmStruct.margin Then
                            If direction = Constant.TS_ES Then
                                chanStruct.tereport = Constant.TRUE
                            Else
                                chanStruct.etreport = Constant.TRUE
                            End If
                        End If
                    Else
                        chanStruct.marg01mode2 = chanStruct.reqd01mode2 - chanStruct.calci01mode2
                        chanNulls(TeChan.MARG01MODE2) = Constant.DB_NOT_NULL
                        If chanStruct.marg01mode2 < parmStruct.margin Then
                            If direction = Constant.TS_ES Then
                                chanStruct.tereport = Constant.TRUE
                            Else
                                chanStruct.etreport = Constant.TRUE
                            End If
                        End If
                    End If
                Else
                    chanNulls(TeChan.MARG01MODE2) = Constant.DB_NULL
                End If
            End If



            '...Log2.v("\nTeCalcs.TeChanCalcs(): Exit");
            Return Constant.SUCCESS
        End Function

        ' BOTTOM

        ''' <summary>
        ''' This method retrieves the antenna gain from the prescribed 
        ''' antenna table in the DB, i.a.w. with the given selection criteria.  
        ''' </summary>
        ''' <paramname="acode"> - antenna code.</param>
        ''' <paramname="tabName"> - name of antenna table.</param>
        ''' <paramname="isSDB"> - indicates whether the tables are in the secondary DB table set, or not.</param>
        ''' <paramname="suAgain"> - the retrieved antenna gain.</param>
        ''' <returns></returns>
        Public Shared Function SuAnteGet(acode As String, tabName As String, isSDB As Short, <Out> ByRef suAgain As Double) As Integer
            ' 'out' requirement.
            suAgain = 0.0

            Dim pAnt As SuAntStr
            Dim nRet As Integer

            '&&Console.Error.Write("\nTgt09");
            nRet = Suutils.SuGetAnt(acode, pAnt)

            If nRet <> 0 Then
                Return Constant.FAILURE
            End If

            suAgain = pAnt.acAnt.again

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
