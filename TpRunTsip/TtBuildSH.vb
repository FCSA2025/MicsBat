Imports _DataStructures
Imports System
Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports System.Runtime.InteropServices
Imports SQLLEN = System.Int64
Imports _Auxlib

Namespace TpRunTsip

    ''' <summary>
    ''' Provides a large number of methods
    ''' used to create and populate the TS SH Tables with data.
    ''' </summary>
    Public Class TtBuildSH
#If PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int ttBuildSHTable([In] string tsipName, [In, Out] TpParm tpParm, [In, Out] SQLLEN[] tpParmNulls, [In, Out] ref int numCases, [In] string startDate, [In] string startTime);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int vic2DimTable([In] TpParm tpParm,
                                                [In] FtSiteStr_OLD ftSiteStr_OLD,
                                                [In] FtSiteStrNulls_OLD ftSiteStrNull_OLD,
                                                [In] IntPtr tLinks_OLD_IntPtr,
                                                [In] int nLinkNum,
                                                [In] string envName,
                                                [In] string proName,
                                                [In] int isMDBInt,
                                                [In] string selCommand,
                                                [In, Out] ref int numCases);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int createAnteSHTable([In, Out] TpParm tpParmStruct,
                                                    [In, Out] TtSite proSiteStruct,
                                                    [In, Out] SQLLEN[] proSiteNulls,
                                                    [In, Out] TtSite envSiteStruct,
                                                    [In, Out] SQLLEN[] envSiteNulls,
                                                    [In] string proName,
                                                    [In] string envName,
                                                    [In, Out] FtSiteStr_OLD pProSite,
                                                    [In, Out] FtSiteStrNulls_OLD pProSiteNulls,
                                                    [In, Out] TLink_OLD pProLink,
                                                    [In, Out] FtSiteStr_OLD pEnvSite,
                                                    [In, Out] FtSiteStrNulls_OLD pEnvSiteNulls,
                                                    [In, Out] TLink_OLD pEnvLink,
                                                    [In] string envCall1,
                                                    [In] string envCall2,
                                                    [In] string envBndCode,
                                                    [In, Out] ref int numProAnte,
                                                    [In, Out] ref int numEnvAnte,
                                                    [In, Out] ref int numCases);


        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int createChanSHTable([In, Out] TpParm tpParmStruct,
                                                    [In, Out] TtSite proSiteStruct,
                                                    [In, Out] SQLLEN[] proSiteNulls,
                                                    [In, Out] TtSite envSiteStruct,
                                                    [In, Out] SQLLEN[] envSiteNulls,
                                                    [In, Out] TtAnte proAnteStruct,
                                                    [In, Out] SQLLEN[] proAnteNulls,
                                                    [In, Out] TtAnte envAnteStruct,
                                                    [In, Out] SQLLEN[] envAnteNulls,
                                                    [In] string proName,
                                                    [In] string envName,
                                                    [In, Out] ref int numProChan,
                                                    [In, Out] ref int numEnvChan,
                                                    [In, Out] ref int numCases,
                                                    [In] double envMBnd,
                                                    [In] double proMBnd,
                                                    [In] double envPatLoss,
                                                    [In] double proPatLoss);

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int CreateChanSHTable_NATIVE(TpParm tpParmStruct,
                                            TtSite proSiteStruct,
                                            SQLLEN[] proSiteNulls,
                                            TtSite envSiteStruct,
                                            SQLLEN[] envSiteNulls,
                                            TtAnte proAnteStruct,
                                            SQLLEN[] proAnteNulls,
                                            TtAnte envAnteStruct,
                                            SQLLEN[] envAnteNulls,
                                            string proName,
                                            string envName,
                                            ref int numProChan,
                                            ref int numEnvChan,
                                            ref int numCases,
                                            double envMBnd,
                                            double proMBnd,
                                            double envPatLoss,
                                            double proPatLoss)
        {
            //...Log2.v("\nTtBuildSH.CreateChanSHTable_NATIVE(): Entry");

            // 'out' requirements.
            //numProChan = 0;
            //numEnvChan = 0;

            int rc = Constant.FAILURE;

            try
            {
                rc = createChanSHTable(tpParmStruct,
                                            proSiteStruct,
                                            proSiteNulls,
                                            envSiteStruct,
                                            envSiteNulls,
                                            proAnteStruct,
                                            proAnteNulls,
                                            envAnteStruct,
                                            envAnteNulls,
                                            proName,
                                            envName,
                                            ref numProChan,
                                            ref numEnvChan,
                                            ref numCases,
                                            envMBnd,
                                            proMBnd,
                                            envPatLoss,
                                            proPatLoss);
            }
            catch (Exception e)
            {
                //...Log2.v("\nTtBuildSH.CreateChanSHTable_NATIVE(): ERROR: then Exit: " + e.Message + "\n" + e.StackTrace);
            }

            //...Log2.v("\nTtBuildSH.CreateChanSHTable_NATIVE(): Exit");
            return rc;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int CreateAnteSHTable_NATIVE(ref TpParm tpParmStruct,
                                                    ref TtSite proSiteStruct,
                                                    ref SQLLEN[] proSiteNulls,
                                                    ref TtSite envSiteStruct,
                                                    ref SQLLEN[] envSiteNulls,
                                                    string proName,
                                                    string envName,
                                                    ref FtSiteStr pProSite,
                                                    ref FtSiteStrNulls pProSiteNulls,
                                                    ref TLink pProLink,
                                                    ref FtSiteStr pEnvSite,
                                                    ref FtSiteStrNulls pEnvSiteNulls,
                                                    ref TLink pEnvLink,
                                                    string envCall1,
                                                    string envCall2,
                                                    string envBndCode,
                                                    ref int numProAnte,
                                                    ref int numEnvAnte,
                                                    ref int numCases)
        {
            //...Log2.v("\nTtBuildSH.CreateAnteSHTable_NATIVE(): Entry.");
            int rv = Constant.FAILURE;

            try
            {
                // Need to use the 'old' version of FtSiteStr that uses pointers not arrays.
                FtSiteStr_OLD pProSite_OLD = new FtSiteStr_OLD(pProSite);
                FtSiteStr_OLD pEnvSite_OLD = new FtSiteStr_OLD(pEnvSite);

                // Need to use the 'old' version of FtSiteStrNulls that uses pointers not arrays.
                FtSiteStrNulls_OLD pProSiteNulls_OLD = new FtSiteStrNulls_OLD(pProSiteNulls);
                FtSiteStrNulls_OLD pEnvSiteNulls_OLD = new FtSiteStrNulls_OLD(pEnvSiteNulls);

                // Need to use the 'old' version of TLink that uses pointers not arrays.
                TLink_OLD pProLink_OLD = new TLink_OLD(pProLink);
                TLink_OLD pEnvLink_OLD = new TLink_OLD(pEnvLink);

                // Make the call to the native C/C++ function.
                rv = createAnteSHTable(tpParmStruct,
                                                    proSiteStruct,
                                                    proSiteNulls,
                                                    envSiteStruct,
                                                    envSiteNulls,
                                                    proName,
                                                    envName,
                                                    pProSite_OLD,
                                                    pProSiteNulls_OLD,
                                                    pProLink_OLD,
                                                    pEnvSite_OLD,
                                                    pEnvSiteNulls_OLD,
                                                    pEnvLink_OLD,
                                                    envCall1,
                                                    envCall2,
                                                    envBndCode,
                                                    ref numProAnte,
                                                    ref numEnvAnte,
                                                    ref numCases);

                // Need to convert back to 'new' structure objects.
                pProSite = new FtSiteStr(pProSite_OLD);
                pEnvSite = new FtSiteStr(pEnvSite_OLD);

                pProLink = new TLink(pProLink_OLD);
                pEnvLink = new TLink(pEnvLink_OLD);

                pProSiteNulls = new FtSiteStrNulls(pProSiteNulls_OLD, pProSite_OLD.nNumAnts, pProSite_OLD.nNumChans);
                pEnvSiteNulls = new FtSiteStrNulls(pEnvSiteNulls_OLD, pProSite_OLD.nNumAnts, pProSite_OLD.nNumChans);
            }
            catch (Exception e)
            {
                Log2.e("\nTtBuildSH.CreateAnteSHTable_NATIVE(): ERROR: exception:\n" + e.Message + "\n" + e.StackTrace);
                Application.Exit("Forced exit from: TtBuildSH.CreateAnteSHTable_NATIVE()");
            }

            //...Log2.v("\nTtBuildSH.CreateAnteSHTable_NATIVE(): Exit: successful.");
            return rv;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int Vic2DimTable_NATIVE(TpParm tpParm,
                                FtSiteStr ftSiteStr,   //	Full proposed site
                                FtSiteStrNulls ftSiteStrNulls,
                                TLink[] tLinks,
                                int nLinkNum,       //	This is the link we are processing.
                                string envName,
                                string proName,
                                bool isMDB,
                                string selCommand,  //	Selection from the environment.
                                ref int numCases)
        {
            //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): called.");
            int rv = Constant.FAILURE;

            try
            {

                int isMDBInt = isMDB ? 1 : 0;

                // [In] FtSiteStr.
                FtSiteStr_OLD ftSiteStr_OLD = new FtSiteStr_OLD(ftSiteStr);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): A");

                FtSiteStrNulls_OLD ftSiteStrNull_OLD = new FtSiteStrNulls_OLD(ftSiteStrNulls);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): B");

                TLink_OLD[] tLinks_OLD = TLink_OLD.ConvertArray(tLinks);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): C");
                IntPtr tLinks_OLD_IntPtr = TLink_OLD.CopyArrayToGlobalMemory(tLinks_OLD);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): D");

                rv = vic2DimTable(tpParm,
                                   ftSiteStr_OLD,
                                   ftSiteStrNull_OLD,
                                   tLinks_OLD_IntPtr,
                                   nLinkNum,
                                   envName,
                                   proName,
                                   isMDBInt,
                                   selCommand,
                                   ref numCases
                                   );

                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): E");
            }
            catch (Exception e)
            {
                Log2.e("\nTtBuildSH.Vic2DimTable_NATIVE(): ERROR: exception:\n" + e.Message + "\n" + e.StackTrace);
                Application.Exit("Forced exit from: TtBuildSH.Vic2DimTable_NATIVE()");
            }
            return rv;
        }

        public static int TtBuildSHTable_NATIVE(string tsipName,
                                    ref TpParm tpParm,
                                    ref SQLLEN[] tpParmNulls,
                                    ref int numCases,
                                    string startDate,
                                    string startTime)
        {
            //...Log2.v("\nTtBuildSH.TtBuildSHTable_NATIVE(): Entry");

            int rc = -666;

            // Native call.
            rc = ttBuildSHTable(tsipName, tpParm, tpParmNulls, ref numCases, startDate, startTime);

            //...Log2.v("\nTtBuildSH.TtBuildSHTable_NATIVE(): Exit, rc = " + rc);
            return rc;
        }
#End If

        Private Const TEN As Integer = 10

        ''' <summary>
        ''' This method provides top-level management for the creation of TS SH Tables
        ''' and their population with results data. 
        ''' </summary>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <paramname="tpParmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="tpParmNulls"> - ODBC nullInds associated with tpParm.</param>
        ''' <paramname="numCases"> - accumulated count of the number of interference cases.</param>
        ''' <paramname="startDate"> - TSIP processing start date to be written to tpParm.</param>
        ''' <paramname="startTime"> - TSIP processing start date to be written to tpParm.</param>
        ''' <returns></returns>
        Public Shared Function TtBuildSHTable(tsipName As String, ByRef tpParmStruct As TpParm, ByRef tpParmNulls As SQLLEN(), ByRef numCases As Integer, startDate As String, startTime As String) As Integer
            '...Log2.v("\nTtBuildSH.TtBuildSHTable(): Entry");

            Dim rc As Integer
            Dim ttParmName As String  ' TSIP parm table name 

            ' create empty SH TABLES 
            rc = TpRunTsip.TtBuildSH.TtCreateTsipTables(tsipName)

            If rc <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtBuildSHTable(): ERROR: call to TtCreateTsipTables() failed, rc = " & rc.ToString() & ". Exit.")
                ErrMsg.UtPrintMessage(rc)
                Return Constant.FAILURE
            End If

            ' compose internal parameter table name 
            GenUtil.UtCvtName(Constant.TT_PARM, tsipName, ttParmName)

            ' insert current date and time into parm rec 
            tpParmStruct.mdate = startDate
            tpParmStruct.mtime = startTime
            tpParmStruct.numcases = -1
            tpParmStruct.numtecases = -1
            tpParmNulls(TpParm.MDATE) = Constant.DB_NOT_NULL
            tpParmNulls(TpParm.MTIME) = Constant.DB_NOT_NULL
            tpParmNulls(TpParm.NUMCASES) = Constant.DB_NOT_NULL
            tpParmNulls(TpParm.NUMTECASES) = Constant.DB_NOT_NULL

            ' insert parameter record into SH TABLE 
            rc = TpRunTsip.TsipUtils.UtInsertParmRecord(ttParmName, tpParmStruct, tpParmNulls)
            If rc <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtBuildSHTable(): ERROR: call to UtInsertParmRecord() failed. Exit.")
                ErrMsg.UtPrintMessage(rc)
                Return -10
            End If

            tpParmStruct.numcases = 0
            tpParmStruct.numtecases = 0

            ' perform initial rough cull to extract a set of affected sites,
            '  then fine cull, and create the temporary interferer and victim
            '  site pair tables.  Then from the two site pair tables, create
            '  the SH TABLES and complete with interference calculations 
            If CSharpImpl.__Assign(rc, TpRunTsip.TtBuildSH.TtCullNCreate(tpParmStruct, tsipName, numCases)) <> Constant.SUCCESS Then
                If rc <> Constant.FAILURE Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtBuildSHTable(): ERROR: call to TtCullNCreate() failed. Exit.")
                    ErrMsg.UtPrintMessage(rc)
                End If
                Return -11
            End If

            '...Log2.v("\nTtBuildSH.TtBuildSHTable(): Exit: final");
            Return Constant.SUCCESS

        End Function ' ----- end ttBuildSHTable ----- 

        ''' <summary>
        ''' This method creates the empty SH and Temporary 
        ''' tables to be populated by TSIP.   
        ''' </summary>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <returns></returns>
        Public Shared Function TtCreateTsipTables(tsipName As String) As Integer
            Dim rc As Integer     ' return code from drop or create 

            ' create TSIP SH TABLE 
            If Ssutil.UtTableExist(Constant.TT, tsipName) Then
                If CSharpImpl.__Assign(rc, Ssutil.UtDropTable(Constant.TT, tsipName)) <> Constant.SUCCESS Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCreateTsipTables(): ERROR: call to Ssutil.UtDropTable() failed, rc = " & rc.ToString())
                    Return rc
                End If
            End If

            If CSharpImpl.__Assign(rc, Ssutil.UtCreateTable(Constant.TT, tsipName)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCreateTsipTables(): ERROR: call to Ssutil.UtCreateTable() failed, rc = " & rc.ToString())
                Return rc
            End If

            Return Constant.SUCCESS

        End Function ' ---- end ttCreateTsipTables ----

        ''' <summary>
        ''' This method controls the selection of the sites, 
        ''' antennae, and channels from the environment that satisfy the User's
        ''' requirements and then populates the associated fields in the SH Tables. 
        ''' </summary>
        ''' <paramname="tpParmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <paramname="numCases"> - accumulated count of the number of interference cases.</param>
        ''' <returns></returns>
        Public Shared Function TtCullNCreate(tpParmStruct As TpParm, tsipName As String, ByRef numCases As Integer) As Integer
            '...Log2.v("\nTtBuildSH.TtCullNCreate(): Entry");

            Dim rc As Integer
            Dim isMDB = False
            Dim codesSelection As String      ' selected code selectn criteria 
            Dim sqlCommand As String          ' selected code selectn criteria 
            Dim ttSiteName As String          ' TSIP site table name 
            Dim ttAnteName As String          ' TSIP ante table name 
            Dim ttChanName As String          ' TSIP chan table name 
            Dim envSiteName As String         ' environment site table name
            Dim envAnteName As String         ' environment ante table name
            Dim envChanName As String         ' environment chan table name
            Dim proSiteName As String         ' proposed site table name 
            Dim proAnteName As String         ' proposed ante table name 
            Dim proChanName As String         ' proposed chan table name 

            Dim cCallFound As String
            Dim pSite As FtSiteStr
            Dim pSiteNulls As FtSiteStrNulls
            Dim nInd As Integer
            Dim pLinks As TLink()
            Dim nNumLinks As Integer

            TpRunTsip.TtBuildSH.TtTableNames(tsipName, tpParmStruct.envtype, tpParmStruct.proname, tpParmStruct.envname, ttSiteName, ttAnteName, ttChanName, envSiteName, envAnteName, envChanName, proSiteName, proAnteName, proChanName, isMDB)

            codesSelection = ""

            '...Log2.v("\nTtBuildSH.TtCullNCreate(): A");

            ' create selection criteria for culling of operator codes or call signs
            TpRunTsip.TsipUtils.UtOpCodesCallSigns(tpParmStruct, codesSelection)

            ' prepare SH SITE, ANTENNA, and CHANNEL tables for insert 
            If CSharpImpl.__Assign(rc, TpRunTsip.TtDynSite.TtPrepareSite(ttSiteName)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCullNCreate(): ERROR: call to TtPrepareSite() failed. Exit")
                GenUtil.SetError(-20, "Could not prepare site: " & rc.ToString())
                Return Constant.FAILURE
            End If
            '...Log2.v("\nTtBuildSH.TtCullNCreate(): B");
            If CSharpImpl.__Assign(rc, TpRunTsip.TtDynAnte.TtPrepareAnte(ttAnteName)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCullNCreate(): ERROR: call to TtPrepareAnte() failed. Exit")
                GenUtil.SetError(-21, "Could not prepare antenna: " & rc.ToString())
                Return Constant.FAILURE
            End If
            '...Log2.v("\nTtBuildSH.TtCullNCreate(): C");
            If CSharpImpl.__Assign(rc, TpRunTsip.TtDynChan.TtPrepareChan(ttChanName)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCullNCreate(): ERROR: call to TtPrepareChan() failed. Exit")
                GenUtil.SetError(-22, "Could not prepare channel: " & rc.ToString())
                Return Constant.FAILURE
            End If

            ' loop through all sites in the Proposed file and process each
            '  completely (ie. all its ante's and chan's) before doing next site 
            cCallFound = ""

            rc = FtUtils.FtEnumSite("cmd != 'D'", tpParmStruct.proname, cCallFound) '	Start the search.

            '...Log2.v("\nTtBuildSH.TtCullNCreate(): D");
            ' for (each site in the proposed file) 
            While rc = 0
                '...Log2.v("\nTtBuildSH.TtCullNCreate(): E");
                '	Got the next call sign in the enumeration.  Get the site.
                rc = FtUtils.FtGetSiteWN(cCallFound, pSite, 3, tpParmStruct.proname, pSiteNulls)

                If rc <> 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCullNCreate(): ERROR: call to FtGetSiteWN() failed. Exit")
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "ttBuildSH: Error getting a proposed site:-" & Microsoft.VisualBasic.Constants.vbLf & "{0}" & Microsoft.VisualBasic.Constants.vbLf, GenUtil.GetUserMess())
                    Return -25
                End If

                '	Get the links for this site.
                nNumLinks = FtUtils.FtMakeLinks(pSite, pLinks)

                '...Log2.v("\nTtBuildSH.TtCullNCreate(): F");
                If nNumLinks < 0 Then
                    '	error getting links
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCullNCreate(): ERROR: call to FtMakeLinks() failed. Exit.")
                    Dim str = String.Format("ttCullNCreate:  Error getting links for {0} ({1})", pSite.stSite.call1, pSite.stSite.name)
                    GenUtil.SetErr(str)
                    Return Constant.FAILURE
                End If

                '	Go through the proposed site's links.
                For nInd = 0 To nNumLinks - 1
                    '...Log2.v("\nTtBuildSH.TtCullNCreate(): G");
                    '	Generate the rough cull for this link by using the first antenna in the link.
                    TpRunTsip.TtBuildSH.GenRoughCull(tpParmStruct, pSite, pLinks(nInd), isMDB, codesSelection, sqlCommand)

                    '...Log2.v(pSiteNulls.ToString());

                    '...Log2.v("\nTtBuildSH.TtCullNCreate(): H");
                    '	Process this link against the environment in the rough cull...
                    'AH: avoid warning C4706: assignment within conditional expression
                    rc = TpRunTsip.TtBuildSH.Vic2DimTable(tpParmStruct, pSite, pSiteNulls, pLinks, nInd, tpParmStruct.envname, tpParmStruct.proname, isMDB, sqlCommand, numCases)

                    If rc <> Constant.SUCCESS Then
                        rc = -1
                        If rc <> 0 Then
                            '	Could not find the other end of the link.  Continue with next case
                            GenUtil.AddErr("Could not find the other end of the link, ignoring...")
                            Continue For
                        End If

                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.TtCullNCreate(): ERROR: call to Vic2DimTable() failed. Exit")
                        Return (GenUtil.AddErr("Could not create vic2Dim table: " & rc.ToString()))
                    End If

                    '...Log2.v("\nTtBuildSH.TtCullNCreate(): I");

                Next ' end of for loop

                FtUtils.FtFreeLinks(pLinks, nNumLinks)

                '...Log2.v("\nTtBuildSH.TtCullNCreate(): J");
                rc = FtUtils.FtEnumSite("cmd != 'D'", tpParmStruct.proname, cCallFound) '	continue the search.

            End While ' end for (each site) 

            TpRunTsip.TtDynChan.TtChanClose()
            TpRunTsip.TtDynAnte.TtCloseAnte()
            TpRunTsip.TtDynSite.TtSiteClose()

            If rc <> Constant.NOMORERECS Then
                Return rc
            End If

            '...Log2.v("\nTtBuildSH.TtCullNCreate(): Exit: final.");
            Return Constant.SUCCESS

        End Function ' ----- end ttCullNCreate ----- 

        ''' <summary>
        ''' This method builds the full SQL table names for the TS
        ''' SH Tables and temp tables. The names are built from the tsip parameter file 
        ''' name and the runname of the record within that file.   
        ''' </summary>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <paramname="envType"> - "MDB_TS" or "INTRA".</param>
        ''' <paramname="proName"> - table name of proposed sites.</param>
        ''' <paramname="envName"> - table name of sites in the environment.</param>
        ''' <paramname="ttSiteName"> - name of Tt site table.</param>
        ''' <paramname="ttAnteName"> - name of Tt ante table.</param>
        ''' <paramname="ttChanName"> - name of Tt chan table.</param>
        ''' <paramname="envSiteName"> - name of table for containing environment site.</param>
        ''' <paramname="envAnteName"> - name of table for containing environment ante.</param>
        ''' <paramname="envChanName"> - name of table for containing environment chan.</param>
        ''' <paramname="proSiteName"> - name of table for proposed site.</param>
        ''' <paramname="proAnteName"> - name of table for proposed ante.</param>
        ''' <paramname="proChanName"> - name of table for proposed chan.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        Public Shared Sub TtTableNames(tsipName As String, envType As String, proName As String, ByRef envName As String, <Out> ByRef ttSiteName As String, <Out> ByRef ttAnteName As String, <Out> ByRef ttChanName As String, <Out> ByRef envSiteName As String, <Out> ByRef envAnteName As String, <Out> ByRef envChanName As String, <Out> ByRef proSiteName As String, <Out> ByRef proAnteName As String, <Out> ByRef proChanName As String, ByRef isMDB As Boolean)
            GenUtil.UtCvtName(Constant.TT_SITE, tsipName, ttSiteName)
            GenUtil.UtCvtName(Constant.TT_ANTE, tsipName, ttAnteName)
            GenUtil.UtCvtName(Constant.TT_CHAN, tsipName, ttChanName)

            ' ENVIRONMENT TABLE NAMES 
            envType = envType.Trim()

            isMDB = False
            If envType.Equals("MDB_TS") Then
                isMDB = True
                envSiteName = "mt_site"
                envAnteName = "mt_ante"
                envChanName = "mt_chan"
            ElseIf envType.Equals("INTRA") Then
                envSiteName = proName
                envAnteName = proName
                envChanName = proName
                '	In the INTRA case, the environment name in the parm table will be blank.
                '	Change this to the proposed file.
                envName = proName
            Else
                envSiteName = envName
                envAnteName = envName
                envChanName = envName
            End If

            ' PROPOSED TABLE NAMES 
            proSiteName = proName
            proAnteName = proName
            proChanName = proName
        End Sub ' ---- end ttTableNames ----

        ''' <summary>
        ''' This method generates the text of the SQL SELECT query that culls 
        ''' sites from the environment for further processing. To increase real-time performance
        ''' use is made of user-defined functions that run on the SQL Server.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="ftIntSite"> - FtSiteStr object.</param>
        ''' <paramname="tLink"> - TLink object.</param>
        ''' <paramname="isMDB"> - boolean; true if the tables are in the main DB table set, otherwise false.</param>
        ''' <paramname="codesSelection"> - provides parameters to be used in user-defined functions on the SQL Server.</param>
        ''' <paramname="sqlCommand"> - text of an SQL query that performs culling using user-defined functions on the SQL Server.</param>
        'envName,
        Public Shared Sub GenRoughCull(tpParm As TpParm, ftIntSite As FtSiteStr, tLink As TLink, isMDB As Boolean, codesSelection As String, <Out> ByRef sqlCommand As String)
            ' 'out' requirement.
            sqlCommand = Nothing

            Dim coordDist As Double = tpParm.coordist
            Dim cDistString As String
            Dim cBands As String
            Dim cOr As String
            Dim cOneBand As String
            Dim sBandBits As BandBits

            Dim nNumBands As Integer = Suutils.SuNumberOfBands()  '	Get the number of bands
            Dim nNumBandWds As Integer = (nNumBands + Constant.BITS_PER_BANDWORD - 1) / Constant.BITS_PER_BANDWORD
            Dim nInd As Integer
            Dim nInd1 As Integer

            '	Check for a non-deleted antenna.
            For nInd = 0 To tLink.nNumAnts - 1
                If Not ftIntSite.stAntsPtr(tLink.aAnts(nInd)).cmd.Equals("D") Then
                    Exit For
                End If
            Next

            '	If all the antennas are deleted, no processing.
            If nInd < tLink.nNumAnts Then
                '	Not a deleted antenna
                If tpParm.selsites.Equals("CALL SIGN") Then
                    ' 	We don't pay attention to distance if we have call signs 
                    cDistString = "(1=1)"
                Else

                    Dim nAntNo = tLink.aAnts(nInd)

                    'Use the distance and keyhole routines at the server
                    cDistString = String.Format("tsip.keyhole_hs({0}, {1}, latit, longit, {2:F6}, {3:F6}) <= 2 ", ftIntSite.stSite.latit, ftIntSite.stSite.longit, ftIntSite.stAntsPtr(CInt(nAntNo)).azmth, coordDist)
                End If

                ' Fill in the bandBit array with the adjacent band bits. 
                TpRunTsip.TtBuildSH.SiteAdjBands(ftIntSite.stSite, sBandBits)
                '	Set the bandword search...
                cBands = ""
                cOr = ""
                For nInd1 = 0 To nNumBandWds - 1
                    cOneBand = String.Format("{0}bandwd{1} & 0x{2:x8} != 0", cOr, nInd1 + 1, sBandBits.bitArray(nInd1))
                    cOr = " or "
                    cBands += cOneBand
                Next

                ' set up the sql command to select (call1, call2, bndcde) from
                '  the proposed antenna table where the call1 is the same as the
                '  current proposed site's call1, the latitude and longitude of the
                '  call1's site falls within the culling box. This select statement
                '  is completed in genKeyholeCull, where another culling box is added,
                '  the bndcde is in the list of bands adj to the proposed site, and
                '  the call1 is one of those specified by the user (if any) 
                If isMDB = True Then
                    ' environment is MDB - no need to check command field

                    sqlCommand = String.Format("({0}) and ({1}) {2} ", cDistString, cBands, codesSelection)
                Else
                    ' environment is a filew - check command field & ignore DELETED records 

                    sqlCommand = String.Format("({0}) and ({1}) {2} and cmd != 'D' ", cDistString, cBands, codesSelection)

                End If

            End If

        End Sub ' ----- end genRoughCull ----- 

        ''' <summary>
        ''' This method checks if the interfering and victim 
        ''' sites operate in adjacent bands with results output
        ''' as a set of bits encoding true or false.
        ''' </summary>
        ''' <paramname="intSiteStruct"> - FtSite object.</param>
        ''' <paramname="pBitArray"> - BandBits object encapsulating the set of result bits.</param>
        Public Shared Sub SiteAdjBands(intSiteStruct As FtSite, <Out> ByRef pBitArray As BandBits)
            ' 'out' requirement.
            pBitArray = New BandBits()

            Dim aBitPos As Integer
            Dim curBand As SdBand
            Dim sBandAdj As BandBits
            Dim nBandCount As Integer = Suutils.SuNumberOfBands()

            pBitArray.Initialize() ' Zero the output array.

            ' Get the bandbits from the FtSite object.
            Dim bandBits As BandBits = New BandBits(intSiteStruct)

            ' for each interferer band bit position.  
            For aBitPos = 1 To nBandCount

                ' is band used 
                'if (GenUtil.UtTestBit(intSiteStruct.bandwd1, aBitPos - 1))
                If GenUtil.UtTestBit(bandBits.bitArray, aBitPos - 1) = Enums.BIT.SET Then
                    ' check status of select 
                    If TpRunTsip.TsipUtils.UtGetBandBit(aBitPos, curBand) = Constant.SUCCESS Then
                        '	We have the band for this bit position.  Get the adjacency 
                        Suutils.SuAdjBands(curBand.bndcde, sBandAdj)
                        Suutils.SuBandBitsOR(pBitArray, sBandAdj)
                    Else
                        ' band with this position doesn't exist 
                        ErrMsg.UtPrintMessage([Error].INVBNDBIT)
                    End If
                End If
            Next
        End Sub ' ---- end siteAdjBands ----

        ''' <summary>
        ''' This method uses the SQL query produced by GenRoughCull to select the 
        ''' (call1, call2, bndcode) sets from the environment and then process each of 
        ''' these environment sets against each proposed set. Processing includes 
        ''' additional culling as well as calculations.  
        ''' </summary>
        ''' <paramname="tpParmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="ftIntSite"> - FtSiteStr object for proposed site.</param>
        ''' <paramname="ftIntSiteNulls"> - array of ODBC nullInds associated with ftIntSite.</param>
        ''' <paramname="pLinks"> - array of TLink objects.</param>
        ''' <paramname="nLinkNum"> - number of the link currently being processed.</param>
        ''' <paramname="envName"> - table name of sites in the environment.</param>
        ''' <paramname="proName"> - table name of proposed site.</param>
        ''' <paramname="isMDB"> - boolean; true if the tables are in the main DB table set, otherwise false.</param>
        ''' <paramname="selCommand"> - text of an SQL query previously output by GenRoughCull().</param>
        ''' <paramname="numCases"> - cummulative count of the number of interference cases.</param>
        ''' <returns></returns>
        'int isMDB,
        Public Shared Function Vic2DimTable(tpParmStruct As TpParm, ftIntSite As FtSiteStr, ftIntSiteNulls As FtSiteStrNulls, pLinks As TLink(), nLinkNum As Integer, envName As String, proName As String, isMDB As Boolean, selCommand As String, ByRef numCases As Integer) As Integer   '	Full proposed site
            '	This is the link we are processing.
            '	Selection from the environment.
            '...Log2.v("\nTtBuild.Vic2DimTable(): Entry.");

            Dim intPrintMsg = ""
            Dim vicPrintMsg = ""
            Dim numProAnte = 0
            Dim numEnvAnte = 0
            Dim rc As Integer
            Dim dist As Double
            Dim azimIV As Double
            Dim azimVI As Double

            Dim proSiteStruct As TtSite
            Dim envSiteStruct As TtSite

            Dim proSiteNulls As SQLLEN()
            Dim envSiteNulls As SQLLEN()

            Dim pIntAnte As FtAnte

            Dim vicCall1 As String
            Dim vicCall2 As String
            Dim sqlCommand As String
            Dim vicBndCode As String

            Dim nInd As Integer
            Dim nRet As Integer
            Dim nLink As Integer

            Dim cCallFound As String
            Dim cRememberString As String

            Dim pIntSite2 As FtSiteStr
            Dim pIntSite2Nulls As FtSiteStrNulls
            Dim pVicSite As FtSiteStr = Nothing
            Dim pVicSiteNulls As FtSiteStrNulls = Nothing
            Dim vicSite2 As FtSiteStr
            Dim vicSite2Nulls As FtSiteStrNulls

            Dim pVicLinks As TLink()
            Dim nNumVicLinks As Integer

            Dim nAntNum = FtUtils.FtFirstNonDelAnte(ftIntSite, pLinks, nLinkNum)       '	First non deleted antenna of the link.

            '...Log2.v("\nTtBuild.Vic2DimTable(): A");

            pIntAnte = ftIntSite.stAntsPtr(nAntNum)

            sqlCommand = selCommand

            '	Get the other end of the interfering antenna
            'AH: avoid warning C4706: assignment within conditional expression
            nRet = TpRunTsip.TpMdbPdfGet.TtFullSiteGet(pIntAnte.call2, proName, False, pIntSite2, pIntSite2Nulls)
            If nRet <> Constant.SUCCESS Then
                Dim str = String.Format("Could not get other end for proposed antenna: {0} to {1} band {2}", pIntAnte.call1, pIntAnte.call2, pIntAnte.bndcde)
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.Vic2DimTable(): ERROR: " & str)
                GenUtil.SetErr("vic2DimTable: " & str)
                Return Constant.FAILURE
            End If

            '...Log2.v("\nTtBuild.Vic2DimTable(): B");

            If tpParmStruct.tsorbout.Equals("Y") Then
                '...Log2.v("\nTtBuild.Vic2DimTable(): C");

                '	Run the tsorb calcs between the site and its link.
                If Not GenUtil.Remember(TpRunTsip.TtBuildSH.RememberString(cRememberString, pIntAnte.call1, pIntAnte.call2)) AndAlso Not GenUtil.Remember(TpRunTsip.TtBuildSH.RememberString(cRememberString, pIntAnte.call2, pIntAnte.call1)) Then
                    '...Log2.v("\nTtBuild.Vic2DimTable(): D");
                    '	This pair hasn't been seen yet.
                    '...Log2.v("\ngamma");
                    nRet = TpRunTsip.TtCalcs.TtTsorbCalcs(tpParmStruct, pIntAnte.offazm, pIntAnte.aht, pIntAnte.call2, pIntAnte.bndcde, pIntAnte.tazmth, pIntAnte.telvtn, ftIntSite.stSite)
                End If
            End If

            cCallFound = ""

            While True
                '...Log2.v("\nTtBuild.Vic2DimTable(): E");

                '	Go through the victim sites as designated by the input where clause.
                nInd = TpRunTsip.TpMdbPdfGet.TtEnumSite(sqlCommand, If(isMDB, Nothing, envName), cCallFound)

                If nInd <> 0 Then
                    '	That's all there are.
                    '...Log2.v("\nTtBuild.Vic2DimTable(): F");
                    Exit While
                End If

                '...Log2.v("\nTtBuild.Vic2DimTable(): G");

                '	Get the victim full site.
                nInd = TpRunTsip.TpMdbPdfGet.TtFullSiteGet(cCallFound, envName, isMDB, pVicSite, pVicSiteNulls)
                If nInd <> 0 Then
                    '...Log2.v("\nTtBuild.Vic2DimTable(): H");
                    ' Call sign not there.  This is an error.
                    Dim str = String.Format("Victim call sign {0} not found.", cCallFound)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.Vic2DimTable(): ERROR: " & str)
                    GenUtil.SetErr("vic2DimTable01: " & str)
                    Exit While
                End If

                '...Log2.v("\nTtBuild.Vic2DimTable(): I: Victim = " + pVicSite.stSite.call1);

                If TpRunTsip.TtBuildSH.IntVicSiteCull(tpParmStruct, ftIntSite.stSite, pVicSite.stSite.oper, pVicSite.stSite.latit / 100.0, pVicSite.stSite.longit / 100.0, pVicSite.stSite.prov, dist, azimIV, azimVI) <> Constant.SUCCESS Then
                    '...Log2.v("\nTtBuild.Vic2DimTable(): J");
                    Continue While
                End If

                '...Log2.v("\nTtBuild.Vic2DimTable(): K");

                '	Site passes basic geometry cull, copy in the site call sign
                vicCall1 = pVicSite.stSite.call1

                '	Get the links in the victim site.
                nNumVicLinks = FtUtils.FtMakeLinks(pVicSite, pVicLinks)
                If nNumVicLinks < 0 Then
                    '...Log2.v("\nTtBuild.Vic2DimTable(): L");
                    '	Problem making the victims links.
                    Dim str = String.Format("vic2DimTable: Could not make the victims links for {0} ({1})", pVicSite.stSite.call1, pVicSite.stSite.name)
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.Vic2DimTable(): ERROR: " & str)
                    GenUtil.SetErr(str)
                    Continue While
                End If

                '...Log2.v("\nTtBuild.Vic2DimTable(): M: nNumVicLinks = " + nNumVicLinks);

                '	Go through the links in the victim site
                For nLink = 0 To nNumVicLinks - 1
                    '...Log2.v("\nTtBuild.Vic2DimTable(): N");

                    '	Check that the proposed and env links are in the same band.
                    If Not Suutils.SuIsBandAdjacent(pLinks(nLinkNum).bndcde, pVicLinks(nLink).bndcde) Then
                        Dim str = String.Format(Microsoft.VisualBasic.Constants.vbLf & "pLinks[nLinkNum].bndcde = {0}" & Microsoft.VisualBasic.Constants.vbLf & "pVicLink[nLink].bndcde = {1}", pLinks(CInt(nLinkNum)).bndcde, pVicLinks(CInt(nLink)).bndcde)
                        '...Log2.v("\nTtBuild.Vic2DimTable(): O: " + str);
                        '	Bands for these antennas aren't adjacent.
                        Continue For
                    End If

                    '...Log2.v("\nTtBuild.Vic2DimTable(): P");

                    vicCall2 = pVicLinks(nLink).call2
                    vicBndCode = pVicLinks(nLink).bndcde
                    If TpRunTsip.TtBuildSH.SitePairsCull(tpParmStruct.envtype, ftIntSite.stSite.call1, ftIntSite.stAntsPtr(nAntNum).call2, vicCall1, vicCall2) <> Constant.SUCCESS Then
                        '...Log2.v("\nTtBuild.Vic2DimTable(): Q");
                        Continue For
                    End If

                    '...Log2.v("\nTtBuild.Vic2DimTable(): R");

                    ' 
                    '  Read environment site's remote data from the	env. file.
                    '  A Constant.FAILURE from this function means only that the data was not
                    '  found and the remainder of the for loop should be skipped.
                    ' 
                    rc = TpRunTsip.TtBuildSH.SetRemData(vicCall2, envName, isMDB, vicSite2, vicSite2Nulls)
                    If rc <> 0 Then
                        If rc = Constant.FAILURE Then
                            '...Log2.v("\nTtBuild.Vic2DimTable(): S");
                            Continue For
                        Else
                            '...Log2.v("\nTtBuild.Vic2DimTable(): T");
                            Dim str = String.Format("***Error Remote error for {0}, {1}" & Microsoft.VisualBasic.Constants.vbLf, vicCall2, envName)
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.Vic2DimTable(): ERROR: " & str)
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & str)
                            Return rc
                        End If
                    End If

                    '...Log2.v("\nTtBuild.Vic2DimTable(): U");

                    ' 	set the values of the 2 TS Site SH records from the	pro. and env. files,  setBothSites() - calls
                    '  	ttSiteCalcs() to finish the population of the	TS Site SH records. 
                    TpRunTsip.TtBuildSH.SetBothSites(tpParmStruct.envtype, ftIntSite, ftIntSiteNulls, pLinks(nLinkNum), pVicSite, pVicSiteNulls, pVicLinks(nLink), dist, azimIV, azimVI, proSiteStruct, proSiteNulls, envSiteStruct, envSiteNulls, pIntSite2, vicSite2)

                    '...Log2.v("\nTtBuild.Vic2DimTable(): V");

                    ' 
                    '  Find distance cull: here we determine whether the site is within the coordination distance
                    '  or within the Keyhole area. If it is neither, no further processing is done for this site.
                    ' 
                    '	Check distance with azimuth for keyhole cull.
                    If dist > tpParmStruct.coordist Then
                        If Not (TpRunTsip.TpKeyhole.WithinRange(proSiteStruct.intoffax, 5.0) OrElse TpRunTsip.TpKeyhole.WithinRange(proSiteStruct.vicoffax, 5.0)) Then
                            '...Log2.v("\nTtBuild.Vic2DimTable(): W");
                            Continue For
                        Else
                            If dist > 2.0 * tpParmStruct.coordist Then
                                '...Log2.v("\nTtBuild.Vic2DimTable(): X");
                                '	Even in the keyhole, the distance must be less than twice coord.
                                Continue For
                            End If
                        End If
                    End If

                    '...Log2.v("\nTtBuild.Vic2DimTable(): Y, " + pVicSite.stSite.call1);

                    ' from SH SITE create SH ANTENNA 
                    'if ((rc = CreateAnteSHTable_NATIVE(ref tpParmStruct, ref proSiteStruct,
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtBuildSH.CreateAnteSHTable(tpParmStruct, proSiteStruct, proSiteNulls, envSiteStruct, envSiteNulls, proName, envName, ftIntSite, ftIntSiteNulls, pLinks(nLinkNum), pVicSite, pVicSiteNulls, pVicLinks(nLink), vicCall1, vicCall2, vicBndCode, numProAnte, numEnvAnte, numCases)) <> Constant.SUCCESS Then
                        Dim str = String.Format("***Error Could not create Ante Table for:-" & Microsoft.VisualBasic.Constants.vbLf & "Vic: {0} into {1} band {2}" & Microsoft.VisualBasic.Constants.vbLf, vicCall1, vicCall2, vicBndCode)
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.Vic2DimTable(): ERROR: " & str)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & str)
                        Return rc
                    End If

                    '...Log2.v("\nTtBuild.Vic2DimTable(): Z");

                    '	Add the records to the tt tables 
                    If numProAnte > 0 Then
                        '...Log2.v("\nTtBuild.Vic2DimTable(): AA");
                        '	Proposed site is interferer.
                        proSiteStruct.report = Constant.TRUE

                        rc = TpRunTsip.TtDynSite.TtInsertSite(proSiteStruct, proSiteNulls)
                        If rc <> Constant.SUCCESS Then
                            ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                            ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)

                            Dim str = String.Format("Error Inserting site returned {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.Vic2DimTable(): ERROR: " & str)
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(str)
                            Return rc
                        End If
                    End If

                    '...Log2.v("\nTtBuild.Vic2DimTable(): AB");
                    If numEnvAnte > 0 AndAlso Not tpParmStruct.envtype.Equals("INTRA") Then
                        '...Log2.v("\nTtBuild.Vic2DimTable(): AC");
                        '	Environment is the interferer.
                        envSiteStruct.report = Constant.TRUE

                        rc = TpRunTsip.TtDynSite.TtInsertSite(envSiteStruct, envSiteNulls)
                        If rc <> Constant.SUCCESS Then
                            '...Log2.v("\nTtBuild.Vic2DimTable(): AD");
                            ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                            ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                            TpRunTsip.TpRunTsip.mTW_ERR.Write("Error Inserting site(1) returned {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                            Return rc
                        End If
                    End If

                Next ' end for (each proposed set) 
            End While ' end while (endLoop) 

            '...Log2.v("\nTtBuild.Vic2DimTable(): Exit.");
            Return Constant.SUCCESS

        End Function ' ----- end vic2DimTable ----- 


        ''' <summary>
        ''' Construct a string from two call signs to use as input to the 
        ''' method GenUtil.Remember().  
        ''' </summary>
        ''' <paramname="cOutString"> - output string = call1 + " " + call2</param>
        ''' <paramname="call1"> - callsign of 1st site.</param>
        ''' <paramname="call2"> - callsign of 2nd site.</param>
        ''' <returns></returns>
        Public Shared Function RememberString(<Out> ByRef cOutString As String, call1 As String, call2 As String) As String
            cOutString = call1
            cOutString += " "
            cOutString += call2

            Return cOutString
        End Function

        ''' <summary>
        ''' This method culls environment sites based on ALL EXCEPT SELF, 
        ''' Country, and distance (fine distance cull).  
        ''' </summary>
        ''' <paramname="parmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="intSiteStruct"> - FtSite object for interfering site.</param>
        ''' <paramname="vicOper"> - victim's operator code.</param>
        ''' <paramname="vicLatit"> - victim's latitude.</param>
        ''' <paramname="vicLongit"> - victim's longitude.</param>
        ''' <paramname="vicProv"> - victim's Province or State.</param>
        ''' <paramname="distIV"> - calculated distance between victim and interferer.</param>
        ''' <paramname="azimIV"> - calculated bearing from interferer to victim.</param>
        ''' <paramname="azimVI"> - calculated bearing from victim to interferer.</param>
        ''' <returns></returns>
        Public Shared Function IntVicSiteCull(parmStruct As TpParm, intSiteStruct As FtSite, vicOper As String, vicLatit As Double, vicLongit As Double, vicProv As String, <Out> ByRef distIV As Double, <Out> ByRef azimIV As Double, <Out> ByRef azimVI As Double) As Integer
            ' 'out' requirements.
            distIV = 0.0
            azimIV = 0.0
            azimVI = 0.0

            Dim intLatit As Double
            Dim intLongit As Double
            Dim vicCountry As String

            ' identical operator codes
            '  if ((proposed oper is same as environment oper)
            '  and (selsites is "ALL EXCEPT SELF")) 
            If intSiteStruct.oper.Equals(vicOper) AndAlso parmStruct.selsites.Equals("ALL EXCEPT SELF") Then
                Return Constant.FAILURE
            End If

            ' culling distance (fine distance cull) -
            '  calculate the distance between the proposed site and environment
            '  site 
            intLatit = intSiteStruct.latit / 100.00
            intLongit = intSiteStruct.longit / 100.00

            AxSub2.AxDistan(intLatit, vicLatit, intLongit, vicLongit, distIV, azimIV, azimVI)

            ' if (the distance is greater than the coordination dist) 
            If distIV > 2 * parmStruct.coordist Then
                Return Constant.FAILURE
            End If

            ' within specified country -
            '  if ((the user specified a country) and (country is not "ALL")) 
            If Not parmStruct.country.Equals("") AndAlso Not parmStruct.country.Equals("ALL") Then
                ' check that the environment is in the specified country 
                GenUtil.UtGetCountry(vicProv, vicCountry)
                If Not vicCountry.Equals(parmStruct.country) Then
                    Return Constant.FAILURE
                End If
            End If

            Return Constant.SUCCESS

        End Function ' ----- end intVicSiteCull ----- 

        ''' <summary>
        ''' This method selects sites to test based on the site cull.
        ''' </summary>
        ''' <remarks>
        ''' The site pairs cull selects records that:
        ''' <listtype="bullet">
        ''' <item>are NOT co-located site pairs (x,y)-(A,B) where x=A & y=B; AND</item>
        ''' <item>are NOT cross-located site pairs (x,y)-(A,B) where x=B & y=A.</item>
        ''' </list>list> 
        ''' </remarks>
        ''' <paramname="envType"> - "MDB_TS" or "INTRA".</param>
        ''' <paramname="intCall1"> - interferer, 1st callsign.</param>
        ''' <paramname="intCall2"> - interferer, 2nd callsign.</param>
        ''' <paramname="vicCall1"> - victim, 1st callsign.</param>
        ''' <paramname="vicCall2"> - victim, 2nd callsign.</param>
        ''' <returns></returns>
        Public Shared Function SitePairsCull(envType As String, intCall1 As String, intCall2 As String, vicCall1 As String, vicCall2 As String) As Integer
            ' co-located site pairs (call1,call2)-(call1,call2) -
            '  if ((proposed call1 is same as environment call1)
            '  and (proposed call2 is same as environment call2) 
            If intCall1.Equals(vicCall1) AndAlso intCall2.Equals(vicCall2) Then
                Return Constant.FAILURE
            End If

            ' cross-located site pairs (call1,call2)-(call2,call1) -
            '  if ((proposed call1 is same as environment call2)
            '  and (proposed call2 is same as environment call1) 
            If intCall1.Equals(vicCall2) AndAlso intCall2.Equals(vicCall1) Then
                Return Constant.FAILURE
            End If

            ' for passive reflectors, PRs (their call signs start w/ %),
            '  ignore cases where call1 is a PR and
            '  	 (call1,call2)-(call1,call3)
            '  	 (call1,call2)-(call3,call1) -
            '  if ((proposed call1 starts with '%')
            '  and ((proposed call1 is same as environment call1)
            '  or (proposed call1 is same as environment call2))) 
            If Strings.FirstCharIs(intCall1, "%"c) AndAlso (intCall1.Equals(vicCall1) OrElse intCall1.Equals(vicCall2)) Then
                Return Constant.FAILURE
            End If

            ' PR - ignore cases where call2 is a PR and
            '  	 (call1,call2)-(call2,call3)
            '  	 (call1,call2)-(call3,call2) -
            '  if ((proposed call2 starts with '%')
            '  and ((proposed call2 is same as environment call1)
            '  or (proposed call2 is same as environment call2))) 
            If Strings.FirstCharIs(intCall2, "%"c) AndAlso (intCall2.Equals(vicCall1) OrElse intCall2.Equals(vicCall2)) Then
                Return Constant.FAILURE
            End If

            Return Constant.SUCCESS

        End Function ' ----- end sitePairsCull ----- 

        ''' <summary>
        ''' This method finds and stores necessary site data about 
        ''' the remote site of the input site.  
        ''' </summary>
        ''' <paramname="callTwo"> - callsign of remote site.</param>
        ''' <paramname="siteName"> - name of site table to be searched.</param>
        ''' <paramname="isMDB"> - boolean; true if the tables are in the main DB table set, otherwise false.</param>
        ''' <paramname="pSite2"> - FtSiteStr object for remote site.</param>
        ''' <paramname="pSiteNulls"> - ODBC nullInds for pSite2.</param>
        ''' <returns></returns>
        Public Shared Function SetRemData(callTwo As String, siteName As String, isMDB As Boolean, <Out> ByRef pSite2 As FtSiteStr, <Out> ByRef pSiteNulls As FtSiteStrNulls) As Integer
            Dim rc = Constant.FAILURE

            ' get interferer's remote name, lat & long 
            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtFullSiteGet(callTwo, siteName, isMDB, pSite2, pSiteNulls)) <> 0 Then
                If rc <> 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.SetRemData(): ERROR: call to TtFullSiteGet() failed, rc = " & rc.ToString())
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", callTwo)
                End If
                Return rc
            End If

            Return Constant.SUCCESS
        End Function ' ---- end setRemData ----

        ''' <summary>
        ''' This method sets the values of the TS Site SH Table from the 
        ''' proposed and environment files; it also calls the method TtSiteCalcs()
        ''' to complete the population of the TS Site record.  
        ''' </summary>
        ''' <paramname="envtype"> - "MDB_TS" or "INTRA".</param>
        ''' <paramname="pIntSite"> - FtSiteStr object describing the interfering site.</param>
        ''' <paramname="pIntSiteNulls"> - ODBC nullInds associated with pIntSite.</param>
        ''' <paramname="pIntLink"> - TLink object associated with the interfering site.</param>
        ''' <paramname="pVicSite"> - FtSiteStr object describing the victim site.</param>
        ''' <paramname="pVicSiteNulls"> - ODBC nullInds associated with pVicSite.</param>
        ''' <paramname="pVicLink"> - TLink object associated with the victim site.</param>
        ''' <paramname="dist"> - distance between interferer and victim sites.</param>
        ''' <paramname="azimIV"> - calculated bearing from interferer to victim.</param>
        ''' <paramname="azimVI"> - calculated bearing from victim to interferer.</param>
        ''' <paramname="proSite"> - TtSite object describing the proposed site.</param>
        ''' <paramname="proNulls"> - ODBC nullInds associated with proSite.</param>
        ''' <paramname="envSite"> - TtSite object describing an environment site.</param>
        ''' <paramname="envNulls"> - ODBC nullInds associated with envSite.</param>
        ''' <paramname="intRemote"> - FtSiteStr object describing the interferer's remote site.</param>
        ''' <paramname="vicRemote"> - FtSiteStr object describing the victim's remote site.</param>
        Public Shared Sub SetBothSites(envtype As String, pIntSite As FtSiteStr, pIntSiteNulls As FtSiteStrNulls, pIntLink As TLink, pVicSite As FtSiteStr, pVicSiteNulls As FtSiteStrNulls, pVicLink As TLink, dist As Double, azimIV As Double, azimVI As Double, <Out> ByRef proSite As TtSite, <Out> ByRef proNulls As SQLLEN(), <Out> ByRef envSite As TtSite, <Out> ByRef envNulls As SQLLEN(), intRemote As FtSiteStr, vicRemote As FtSiteStr)
            ' 'out' requirements.
            proSite = New TtSite()   ' TSIP site record structure 
            envSite = New TtSite()   ' TSIP site record structure 
            proNulls = NullHelper.CreateArrayOfNullInd(TtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)
            envNulls = NullHelper.CreateArrayOfNullInd(TtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            Dim intSite = pIntSite.stSite
            Dim vicSite = pVicSite.stSite

            NullHelper.FillArray(proNulls, Constant.DB_NULL)

            proSite.interferer = "P"
            proSite.intcall1 = intSite.call1
            proSite.intcall2 = pIntLink.call2
            proSite.viccall1 = vicSite.call1
            proSite.viccall2 = pVicLink.call2

            proSite.intname1 = intSite.name
            proSite.intoper = intSite.oper
            proSite.intlatit = intSite.latit
            proSite.intlongit = intSite.longit
            proSite.intgrnd = CDbl(intSite.grnd)

            proSite.vicname1 = vicSite.name
            proSite.vicoper = vicSite.oper
            proSite.viclatit = vicSite.latit
            proSite.viclongit = vicSite.longit
            proSite.vicgrnd = CDbl(vicSite.grnd)
            proSite.report = Constant.FALSE
            proSite.caseno = 0
            proSite.subcases = 0

            proSite.intname2 = intRemote.stSite.name
            proSite.intoper2 = intRemote.stSite.oper
            proSite.int1int2dist = pIntSite.stAntsPtr(pIntLink.aAnts(0)).dist ' Distance of first antenna in link.
            proSite.vicname2 = vicRemote.stSite.name
            proSite.vicoper2 = vicRemote.stSite.oper
            proSite.vic1vic2dist = pVicSite.stAntsPtr(pVicLink.aAnts(0)).dist
            proSite.int1vic1dist = dist

            ' 	Store the azimuths for the geometry on the report - GJS - 2005.10.04 
            proSite.intvicaz = azimIV
            proSite.vicintaz = azimVI

            proNulls(TtSite.INTERFERER) = Constant.DB_NOT_NULL
            proNulls(TtSite.INTCALL1) = Constant.DB_NOT_NULL
            proNulls(TtSite.INTCALL2) = Constant.DB_NOT_NULL
            proNulls(TtSite.VICCALL1) = Constant.DB_NOT_NULL
            proNulls(TtSite.VICCALL2) = Constant.DB_NOT_NULL

            proNulls(TtSite.INTNAME1) = pIntSiteNulls.anSiteNull(FtSite.NAME)
            proNulls(TtSite.INTOPER) = pIntSiteNulls.anSiteNull(FtSite.OPER)
            proNulls(TtSite.INTLATIT) = pIntSiteNulls.anSiteNull(FtSite.LATIT)
            proNulls(TtSite.INTLONGIT) = pIntSiteNulls.anSiteNull(FtSite.LONGIT)
            proNulls(TtSite.INTGRND) = pIntSiteNulls.anSiteNull(FtSite.GRND)

            proNulls(TtSite.VICNAME1) = pVicSiteNulls.anSiteNull(FtSite.NAME)
            proNulls(TtSite.VICOPER) = pVicSiteNulls.anSiteNull(FtSite.OPER)
            proNulls(TtSite.VICLATIT) = pVicSiteNulls.anSiteNull(FtSite.LATIT)
            proNulls(TtSite.VICLONGIT) = pVicSiteNulls.anSiteNull(FtSite.LONGIT)
            proNulls(TtSite.VICGRND) = pVicSiteNulls.anSiteNull(FtSite.GRND)

            proNulls(TtSite.REPORT) = Constant.DB_NOT_NULL
            proNulls(TtSite.CASENO) = Constant.DB_NOT_NULL
            proNulls(TtSite.SUBCASES) = Constant.DB_NOT_NULL

            proNulls(TtSite.INTNAME2) = Constant.DB_NOT_NULL
            proNulls(TtSite.INTOPER2) = Constant.DB_NOT_NULL
            proNulls(TtSite.INT1INT2DIST) = Constant.DB_NOT_NULL
            proNulls(TtSite.VICNAME2) = Constant.DB_NOT_NULL
            proNulls(TtSite.VICOPER2) = Constant.DB_NOT_NULL
            proNulls(TtSite.VIC1VIC2DIST) = Constant.DB_NOT_NULL
            proNulls(TtSite.INT1VIC1DIST) = Constant.DB_NOT_NULL

            proNulls(TtSite.INTVICAZ) = Constant.DB_NOT_NULL
            proNulls(TtSite.VICINTAZ) = Constant.DB_NOT_NULL

            TpRunTsip.TtCalcs.TtSiteCalcs(proSite, proNulls, pIntSite.stSite.grnd, pVicSite.stSite.grnd, azimIV, azimVI, pIntSite.stAntsPtr(pIntLink.aAnts(0)).azmth, pVicSite.stAntsPtr(pVicLink.aAnts(0)).azmth)


            If Not envtype.Equals("INTRA") Then
                For i = 0 To TtSite.NUM_COLUMNS - 1
                    envNulls(i) = proNulls(i)
                Next

                envSite.interferer = "E"
                envSite.intcall1 = vicSite.call1
                envSite.intcall2 = pVicLink.call2
                envSite.viccall1 = intSite.call1
                envSite.viccall2 = pIntLink.call2

                envSite.intname1 = vicSite.name
                envSite.intoper = vicSite.oper
                envSite.intlatit = vicSite.latit
                envSite.intlongit = vicSite.longit
                envSite.intgrnd = CDbl(vicSite.grnd)

                envSite.vicname1 = intSite.name
                envSite.vicoper = intSite.oper
                envSite.viclatit = intSite.latit
                envSite.viclongit = intSite.longit
                envSite.vicgrnd = CDbl(intSite.grnd)

                ' 	Store the azimuths for the geometry on the report - GJS - 2005.10.04 
                envSite.intvicaz = azimVI
                envSite.vicintaz = azimIV
                envNulls(TtSite.INTVICAZ) = Constant.DB_NOT_NULL
                envNulls(TtSite.VICINTAZ) = Constant.DB_NOT_NULL

                envSite.report = Constant.FALSE
                envSite.caseno = 0
                envSite.subcases = 0

                envSite.intname2 = vicRemote.stSite.name
                envSite.intoper2 = vicRemote.stSite.oper
                envSite.int1int2dist = pVicSite.stAntsPtr(pVicLink.aAnts(0)).dist '
                envSite.vicname2 = intRemote.stSite.name
                envSite.vicoper2 = intRemote.stSite.oper
                envSite.vic1vic2dist = pIntSite.stAntsPtr(pIntLink.aAnts(0)).dist
                envSite.int1vic1dist = dist
                envNulls(TtSite.INTNAME2) = Constant.DB_NOT_NULL
                envNulls(TtSite.INTOPER2) = Constant.DB_NOT_NULL
                envNulls(TtSite.VICNAME2) = Constant.DB_NOT_NULL
                envNulls(TtSite.VICOPER2) = Constant.DB_NOT_NULL

                TpRunTsip.TtCalcs.TtSiteCalcs(envSite, envNulls, pVicSite.stSite.grnd, pIntSite.stSite.grnd, azimVI, azimIV, pVicSite.stAntsPtr(pVicLink.aAnts(0)).azmth, pIntSite.stAntsPtr(pIntLink.aAnts(0)).azmth)
            End If

        End Sub ' ---- end setBothSites ----


        ''' <summary>
        ''' This method controls the culling and population of the TS Ante SH 
        ''' Table records.  
        ''' </summary>
        ''' <paramname="tpParmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="proSiteStruct"> - TtSite object describing the proposed site.</param>
        ''' <paramname="proSiteNulls"> - ODBC nullInds associated with proSiteStruct.</param>
        ''' <paramname="envSiteStruct"> - TtSite object describing an environment site.</param>
        ''' <paramname="envSiteNulls"> - ODBC nullInds associated with envSiteStruct.</param>
        ''' <paramname="proName"> - table name of proposed sites.</param>
        ''' <paramname="envName"> - table name of sites in the environment.</param>
        ''' <paramname="pProSite"> - FtSiteStr object describing the proposed site.</param>
        ''' <paramname="pProSiteNulls"> - ODBC nullInds associated with pProSite.</param>
        ''' <paramname="pProLink"> - TLink objected associated with proposed site.</param>
        ''' <paramname="pEnvSite"> - FtSiteStr object describing the environment site.</param>
        ''' <paramname="pEnvSiteNulls"> - ODBC nullInds associated with pEnvSite.</param>
        ''' <paramname="pEnvLink"> - TLink objected associated with environment site.</param>
        ''' <paramname="envCall1"> - callsign of environment site.</param>
        ''' <paramname="envCall2"> - callsign of environment remote site.</param>
        ''' <paramname="envBndCode"> - Band Code of environment site.</param>
        ''' <paramname="numProAnte"> - number of antennae processed for proposed site.</param>
        ''' <paramname="numEnvAnte"> - number of antennae processed for environment site.</param>
        ''' <paramname="numCases"> - cummulative count of number of interference cases considered.</param>
        ''' <returns></returns>
        Public Shared Function CreateAnteSHTable(ByRef tpParmStruct As TpParm, ByRef proSiteStruct As TtSite, ByRef proSiteNulls As SQLLEN(), ByRef envSiteStruct As TtSite, ByRef envSiteNulls As SQLLEN(), proName As String, envName As String, ByRef pProSite As FtSiteStr, ByRef pProSiteNulls As FtSiteStrNulls, ByRef pProLink As TLink, ByRef pEnvSite As FtSiteStr, ByRef pEnvSiteNulls As FtSiteStrNulls, ByRef pEnvLink As TLink, envCall1 As String, envCall2 As String, envBndCode As String, <Out> ByRef numProAnte As Integer, <Out> ByRef numEnvAnte As Integer, ByRef numCases As Integer) As Integer
            '...Log2.v("\n\n===== TtBuildSH.CreateAnteSHTable(): Entry");

            ' 'out' requirements.
            numProAnte = 0
            numEnvAnte = 0

            Dim numProChan = 0
            Dim numEnvChan = 0
            Dim rc As Integer
            Dim proMBnd As Double
            Dim envMBnd = 0.0
            Dim envPatLoss = 0.0
            Dim proPatLoss As Double
            Dim intPrintMsg As String
            Dim vicPrintMsg As String
            Dim oldEnvBndCode As String
            Dim pat360 = New Char(1) {"N"c, "N"c}

            Dim proAnteStruct As TtAnte = New TtAnte()   ' TSIP antenna record structure 
            Dim proAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            Dim envAnteStruct As TtAnte = New TtAnte()   ' TSIP antenna record structure 
            Dim envAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            Dim proAcode As String
            Dim envAcode As String
            Dim proAuse As String
            Dim envAuse As String
            Dim proGain As Single
            Dim envGain As Single

            Dim proht As Single
            Dim envht As Single

            Dim proAnum As Short
            Dim envAnum As Short

            Dim envOffax As String
            Dim envTazmth As Double
            Dim envTelvtn As Double
            Dim envTgain As Double
            Dim envAz As Double
            Dim envEl As Double

            Dim envAntAz As Double
            Dim envAntEl As Double
            Dim envDiscAng As Double

            Dim proOffax As String
            Dim proTazmth As Double
            Dim proTelvtn As Double
            Dim proTgain As Double
            Dim proAz As Double
            Dim proEl As Double

            Dim proAntAz As Double
            Dim proAntEl As Double
            Dim proDiscAng As Double

            Dim dDistIV As Double
            Dim dBearIV As Double
            Dim dBearVI As Double
            Dim dElevIV As Double
            Dim dElevVI As Double

            Dim nProInd As Integer
            Dim nEnvInd As Integer

            Dim pProAnte As FtAnte
            Dim pProAnteNulls As SQLLEN()
            Dim pEnvAnte As FtAnte
            Dim pEnvAnteNulls As SQLLEN()

            Dim nProAntNum As Integer
            Dim nEnvAntNum As Integer

            Dim curBand As SuBand

            ' for each site permutation produced in vic2DimTable(), find all
            '  possible bandCode/antennaNumber permutations of antenna information
            '  (x,y,bandCode,antennaNumber)-(A,B,bandCode,antennaNumber) 

            oldEnvBndCode = ""

            proAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)
            envAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            '	Go through proposed antennnas in link.
            For nProInd = 0 To pProLink.nNumAnts - 1
                '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): A");

                '	Assign the proposed variables...
                nProAntNum = pProLink.aAnts(nProInd)

                pProAnte = pProSite.stAntsPtr(nProAntNum) ' The link has the index of the ante.
                pProAnteNulls = pProSiteNulls.anAntsNullPtr(nProAntNum)

                proAnum = pProAnte.anum
                proAcode = pProAnte.acode
                proAuse = pProAnte.ause
                proGain = pProAnte.tgain
                proht = pProAnte.aht
                proOffax = pProAnte.offazm
                proTazmth = pProAnte.tazmth
                proTelvtn = pProAnte.telvtn
                proTgain = pProAnte.tgain
                proAz = pProAnte.azmth
                proEl = pProAnte.elvtn

                intPrintMsg = String.Format("INTERFERER:  {0,10}   {1,10}   {2,5}   {3,4:D}", pProAnte.call1, pProAnte.call2, pProAnte.bndcde, proAnum)

                For nEnvInd = 0 To pEnvLink.nNumAnts - 1
                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): B");

                    nEnvAntNum = pEnvLink.aAnts(nEnvInd)
                    pEnvAnte = pEnvSite.stAntsPtr(nEnvAntNum) ' The link has the index of the ante.
                    pEnvAnteNulls = pEnvSiteNulls.anAntsNullPtr(nEnvAntNum)

                    envAnum = pEnvAnte.anum
                    envAcode = pEnvAnte.acode
                    envAuse = pEnvAnte.ause
                    envGain = pEnvAnte.tgain
                    envht = pEnvAnte.aht
                    envOffax = pEnvAnte.offazm
                    envTazmth = pEnvAnte.tazmth
                    envTelvtn = pEnvAnte.telvtn
                    envTgain = pEnvAnte.tgain
                    envAz = pEnvAnte.azmth
                    envEl = pEnvAnte.elvtn

                    vicPrintMsg = String.Format("VICTIM:      {0,10}   {1,10}   {2,5}   {3,4:D}", pEnvAnte.call1, pEnvAnte.call2, pEnvAnte.bndcde, envAnum)

                    ' ***************************************************************\
                    ' 
                    ' 		Offaxis angle processing.	GJS - 1108 - 2002.11
                    ' 
                    ' 		The true (T) azimuth and elevation represent the antenna as it
                    ' 		really is, if they are present.  Otherwise, the antenna
                    ' 		boresight is assumed to be right down the hop line.
                    ' 
                    ' 		Set the default values for the offaxis values from the input
                    ' 
                    ' \****************************************************************
                    Dim s As String = envOffax.Trim().ToUpper()
                    Dim isYorT = s.Equals("Y") OrElse s.Equals("T")

                    If pEnvAnteNulls(FtAnte.OFFAZM) <> Constant.DB_NULL AndAlso isYorT Then
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): C");

                        ' 	An offaxis angle was present 
                        If pEnvAnteNulls(FtAnte.TAZMTH) <> Constant.DB_NULL Then
                            '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): D");

                            envAntAz = envTazmth
                            If pEnvAnteNulls(FtAnte.TELVTN) <> Constant.DB_NULL Then
                                envAntEl = envTelvtn
                            Else
                                envAntEl = 0.0 ' 	Assume zero elevation if missing 
                            End If
                        Else
                            '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): E");

                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "{0}" & Microsoft.VisualBasic.Constants.vbLf & "{1}" & Microsoft.VisualBasic.Constants.vbLf & "Offaxis angle indicated for environment, but angle not found." & Microsoft.VisualBasic.Constants.vbLf & "Hop azimuth is being used." & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)

                            envAntAz = envAz
                            envAntEl = envEl
                        End If
                        envOffax = "Y" ' 	Make sure it is not null. 
                    Else
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): F");

                        ' 	Just use the hop azimuth 
                        envAntAz = envAz
                        envAntEl = envEl
                        envOffax = "N"
                    End If

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): G");

                    ' 	Now the proposed angles 
                    s = proOffax.Trim().ToUpper()
                    isYorT = s.Equals("Y") OrElse s.Equals("T")

                    If pProAnteNulls(FtAnte.OFFAZM) <> Constant.DB_NULL AndAlso isYorT Then
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): H");

                        ' 	An offaxis angle was present 
                        If pProAnteNulls(FtAnte.TAZMTH) <> Constant.DB_NULL Then
                            proAntAz = proTazmth
                            If pProAnteNulls(FtAnte.TELVTN) <> Constant.DB_NULL Then
                                proAntEl = proTelvtn
                            Else
                                proAntEl = 0.0 ' 	Zero if missing 
                            End If
                        Else
                            TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "{0}" & Microsoft.VisualBasic.Constants.vbLf & "{1}" & Microsoft.VisualBasic.Constants.vbLf & "Offaxis angle indicated for proposed, but angle not found." & Microsoft.VisualBasic.Constants.vbLf & "Hop azimuth is being used." & Microsoft.VisualBasic.Constants.vbLf, intPrintMsg, vicPrintMsg)

                            proAntAz = proAz
                            proAntEl = proEl
                        End If
                        proOffax = "Y"
                    Else
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): I");

                        ' 	Just use the hop azimuth 
                        proAntAz = proAz
                        proAntEl = proEl
                        proOffax = "N"
                    End If

                    ' 	Now we have the actual antenna azimuths and elevations for both
                    ' 		env and pro sites.  We need to calculate the vector between them
                    ' 		and use that to calculate the offaxis angles. 
                    AxSub2.AxDistan(proSiteStruct.intlatit / 100.0, proSiteStruct.viclatit / 100.0, proSiteStruct.intlongit / 100.0, proSiteStruct.viclongit / 100.0, dDistIV, dBearIV, dBearVI)

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): J");

                    If dDistIV > 0.001 Then
                        ' 	Given the distance we can calculate the elevation 
                        AxSub3.AxElev((proSiteStruct.intgrnd + proht) / 1000.0, (proSiteStruct.vicgrnd + envht) / 1000.0, dDistIV, dElevIV, dElevVI)

                        ' 	Now we can calculate the offaxis angles for these antennas. 
                        proDiscAng = GenUtil.IncAngle(proAntAz, proAntEl, dBearIV, dElevIV)
                        envDiscAng = GenUtil.IncAngle(envAntAz, envAntEl, dBearVI, dElevVI)
                    Else
                        ' 	The antennas are colocated, set the offaxis angles to 90.0 
                        envDiscAng = 90.0
                        proDiscAng = 90.0
                    End If

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): K");

                    ' 	Set up the topology for the antennas.  This includes the off-axis
                    ' 		angles. First when Proposed is interferer, then environment. 
                    TpRunTsip.TtBuildSH.SetTopology(proAnteStruct, proAnteNulls, proOffax, proAz, proAntAz, proDiscAng, envOffax, envAz, envAntAz, envDiscAng)
                    TpRunTsip.TtBuildSH.SetTopology(envAnteStruct, envAnteNulls, envOffax, envAz, envAntAz, envDiscAng, proOffax, proAz, proAntAz, proDiscAng)

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): L");

                    ' set the values of the 2 TS Ante SH records from the pro. and env. files,
                    '  setBothAntes() - calls ttAnteCalcs() to finish the population of the
                    '  TS Ante SH records. 

                    rc = TpRunTsip.TtBuildSH.SetBothAntes(tpParmStruct, pProSite, pProSiteNulls, nProAntNum, pEnvSite, pEnvSiteNulls, nEnvAntNum, pProLink.bndcde, proAnum, proAcode, proAuse, pProAnteNulls(FtAnte.ACODE), pProAnteNulls(FtAnte.AUSE), proGain, pProAnteNulls(FtAnte.TGAIN), envBndCode, envAnum, envAcode, envAuse, pEnvAnteNulls(FtAnte.ACODE), pProAnteNulls(FtAnte.AUSE), envGain, pProAnteNulls(FtAnte.TGAIN), proSiteStruct, proAnteStruct, proAnteNulls, envAnteStruct, envAnteNulls, intPrintMsg, vicPrintMsg, pat360, proht, envht, proDiscAng, envDiscAng, proEl, envEl)

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): M");

                    If rc <> 0 Then
                        Return rc
                    End If

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): N");

                    ' if (oldEnvBndCode is different from curr env. bndcode) 
                    If Not oldEnvBndCode.Equals(envBndCode) Then
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): O");

                        oldEnvBndCode = envBndCode
                        ' get midband freq from SDB for environment 
                        If Suutils.SuGetBand(envBndCode, curBand) <> Constant.SUCCESS Then
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.CreateAnteSHTable(): ERROR: call to SuGetBand() failed.")
                            ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                            ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                            ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, envBndCode)
                            Return Constant.FAILURE
                        End If
                        envMBnd = curBand.bmidf

                        ' if (env. file type is not "INTRA") 
                        If Not tpParmStruct.envtype.Equals("INTRA") Then
                            ' ********************************************************************\
                            ' 
                            ' 		Calculate the path distance, not the ground distance for use later
                            ' 		on GJS - 1179 - 2004.08.23
                            ' 
                            ' \*********************************************************************
                            dDistIV = AxSub3.PathDist((proSiteStruct.intgrnd + proAnteStruct.intaht) / 1000.0, (proSiteStruct.vicgrnd + proAnteStruct.vicaht) / 1000.0, dDistIV)

                            ' calculate the env. path loss, envPatLoss 
                            GenUtil.FreeSpacePathLoss(dDistIV, envMBnd / 1000.0, envPatLoss)
                        End If
                    End If

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): P");

                    ' get midband freq, proMBnd, from SDB for proposed 
                    If Suutils.SuGetBand(pProLink.bndcde, curBand) <> Constant.SUCCESS Then
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.CreateAnteSHTable(): ERROR: call to SuGetBand() failed.")
                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                        ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, pProLink.bndcde)
                        Return Constant.FAILURE
                    End If
                    proMBnd = curBand.bmidf

                    ' calculate the pro. path loss, proPatLoss 
                    GenUtil.FreeSpacePathLoss(dDistIV, proMBnd / 1000.0, proPatLoss)

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): Q");

                    ' from SH ANTE create SH CHANNEL 
                    'if ((rc = CreateChanSHTable_NATIVE(tpParmStruct, proSiteStruct, proSiteNulls,
                    If CSharpImpl.__Assign(rc, TpRunTsip.TtBuildSH.CreateChanSHTable(tpParmStruct, proSiteStruct, proSiteNulls, envSiteStruct, envSiteNulls, proAnteStruct, proAnteNulls, envAnteStruct, envAnteNulls, proName, envName, numProChan, numEnvChan, numCases, envMBnd, proMBnd, envPatLoss, proPatLoss)) <> Constant.SUCCESS Then
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): Q-1");
                        Return rc
                    End If

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): R");

                    ' add proAnte to SH Table 
                    If numProChan > 0 Then
                        '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): R-1");
                        If proAnteStruct.report = 1 Then
                            '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): R-2");
                            ' Set the case number in the antenna table 
                            proAnteStruct.caseno = proSiteStruct.caseno
                            proAnteNulls(TtAnte.CASENO) = Constant.DB_NOT_NULL

                            ' insert the proposed ante data into the ante SH Table 
                            rc = TpRunTsip.TtDynAnte.TtInsertAnte(proAnteStruct, proAnteNulls)
                            If rc <> Constant.SUCCESS Then
                                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.CreateAnteSHTable(): ERROR: call to TtInsertAnte() failed.")
                                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                TpRunTsip.TpRunTsip.mTW_ERR.Write("Error: Inserting tt Antenna returned {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                                Return rc
                            End If
                        End If

                        numProAnte += 1
                    End If

                    '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): S");

                    If numEnvChan > 0 Then
                        If envAnteStruct.report = 1 Then
                            ' 	Set the antenna case 
                            envAnteStruct.caseno = envSiteStruct.caseno
                            envAnteNulls(TtAnte.CASENO) = Constant.DB_NOT_NULL

                            ' insert the environment ante data into the ante * SH Table 
                            rc = TpRunTsip.TtDynAnte.TtInsertAnte(envAnteStruct, envAnteNulls)
                            If rc <> Constant.SUCCESS Then
                                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                TpRunTsip.TpRunTsip.mTW_ERR.Write("Error: Inserting tt Antenna(2) returned {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                                Return rc
                            End If
                        End If

                        numEnvAnte += 1
                    End If
                Next
            Next

            '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): T");

            ' 
            '  Revision for 360 degree antenna pattern: if pat360[0] has been set to
            '  'Y' by setBothAntes, then add 360 to intoffax if it is less than 0.
            '  Likewise, if pat360[1] has been set to 'Y' by setBothAntes, then add
            '  360 to vicoffax if it is less than 0.
            ' 

            '...Log2.v("\nSIGMA: pat360[0] = " + pat360[0]);
            '...Log2.v("\nSIGMA: intoffax = " + proSiteStruct.intoffax);
            If pat360(0) = "Y"c AndAlso proSiteStruct.intoffax < 0 Then
                proSiteStruct.intoffax = 360 + proSiteStruct.intoffax
                '...Log2.v("\nZULU: intoffax = " + proSiteStruct.intoffax);

            End If

            '...Log2.v("\nSIGMA: pat360[1] = " + pat360[1]);
            '...Log2.v("\nSIGMA: vicoffax = " + proSiteStruct.vicoffax);
            If pat360(1) = "Y"c AndAlso proSiteStruct.vicoffax < 0 Then
                proSiteStruct.vicoffax = 360 + proSiteStruct.vicoffax
                '...Log2.v("\nZULU: vicoffax = " + proSiteStruct.vicoffax);

            End If

            '...Log2.v("\nTtBuildSH.CreateAnteSHTable(): Exit: final");
            Return Constant.SUCCESS

        End Function ' ----- end createAnteSHTable ----- 


        ''' <summary>
        ''' This method sets the azimuth and off-axis angles fields in a TtAnte object.  
        ''' </summary>
        ''' <paramname="pttAnte"> - TtAnte object to be partially populated.</param>
        ''' <paramname="nttAnteNulls"> - ODBC nullInds associated with pttAnte.</param>
        ''' <paramname="cIntOffax"> - TBD.</param>
        ''' <paramname="dIntHopAz"> - TBD.</param>
        ''' <paramname="dIntAntAz"> - TBD.</param>
        ''' <paramname="dIntOffax"> - TBD.</param>
        ''' <paramname="cVicOffax"> - TBD.</param>
        ''' <paramname="dVicHopAz"> - TBD.</param>
        ''' <paramname="dVicAntAz"> - TBD.</param>
        ''' <paramname="dVicOffax"> - TBD.</param>
        Public Shared Sub SetTopology(ByRef pttAnte As TtAnte, ByRef nttAnteNulls As SQLLEN(), cIntOffax As String, dIntHopAz As Double, dIntAntAz As Double, dIntOffax As Double, cVicOffax As String, dVicHopAz As Double, dVicAntAz As Double, dVicOffax As Double)
            pttAnte.intaoffax = cIntOffax
            pttAnte.inthopaz = dIntHopAz
            pttAnte.intantaz = dIntAntAz
            pttAnte.intoffantax = dIntOffax

            nttAnteNulls(TtAnte.INTAOFFAX) = Constant.DB_NOT_NULL
            nttAnteNulls(TtAnte.INTHOPAZ) = Constant.DB_NOT_NULL
            nttAnteNulls(TtAnte.INTANTAZ) = Constant.DB_NOT_NULL
            nttAnteNulls(TtAnte.INTOFFANTAX) = Constant.DB_NOT_NULL

            pttAnte.vicaoffax = cVicOffax
            pttAnte.vichopaz = dVicHopAz
            pttAnte.vicantaz = dVicAntAz
            pttAnte.vicoffantax = dVicOffax

            nttAnteNulls(TtAnte.VICAOFFAX) = Constant.DB_NOT_NULL
            nttAnteNulls(TtAnte.VICHOPAZ) = Constant.DB_NOT_NULL
            nttAnteNulls(TtAnte.VICANTAZ) = Constant.DB_NOT_NULL
            nttAnteNulls(TtAnte.VICOFFANTAX) = Constant.DB_NOT_NULL
        End Sub

        ''' <summary>
        ''' This method sets the values of the TS Ante SH Table from the 
        ''' proposed and environment files; it also calls the method TtAnteCalcs()
        ''' to complete the population of the TS Site record.    
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="pProSite"> - FtSiteStr object describing the proposed site.</param>
        ''' <paramname="pProSiteNulls"> - ODBC nullInds associated with pProSite.</param>
        ''' <paramname="nProAntNum"> - proposed antennae number.</param>
        ''' <paramname="pEnvSite"> - FtSiteStr object describing the environment site.</param>
        ''' <paramname="pEnvSiteNulls"> - ODBC nullInds associated with pEnvSite.</param>
        ''' <paramname="nEnvAntNum"> - proposed antennae number</param>
        ''' <paramname="intBndcde"> - interferer's Band Code.</param>
        ''' <paramname="intAnum"> - interferer's antenna number.</param>
        ''' <paramname="intAcode"> - interferer's antenna code.</param>
        ''' <paramname="intAuse"> - TBD.</param>
        ''' <paramname="intNull"> - ODBC nullInd.</param>
        ''' <paramname="intNullUse"> - ODBC nullInd.</param>
        ''' <paramname="intGain"> - inteferer's gain.</param>
        ''' <paramname="intGainNull"> - ODBC nullInd associated with intGain.</param>
        ''' <paramname="vicBndcde"> - victim's Band Code.</param>
        ''' <paramname="vicAnum"> - victim's antenna number.</param>
        ''' <paramname="vicAcode"> - victim's antenna code.</param>
        ''' <paramname="vicAuse"> - TBD.</param>
        ''' <paramname="vicNull"> - ODBC nullInd.</param>
        ''' <paramname="vicNullUse"> - ODBC nullInd.</param>
        ''' <paramname="vicGain"> - victim's gain.</param>
        ''' <paramname="vicGainNull"> - ODBC nullInd associated with vicGain.</param>
        ''' <paramname="ttSite"> - TtSite object.</param>
        ''' <paramname="proAnte"> - TtAnte object describing the proposed site's antenna.</param>
        ''' <paramname="proNulls"> - ODBC nullInd associated with proAnte.</param>
        ''' <paramname="envAnte"> - TtAnte object describing the environment site's antenna.</param>
        ''' <paramname="envNulls"> - ODBC nullInd associated with envAnte.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <paramname="pat360"> - TBD.</param>
        ''' <paramname="intaht"> - TBD.</param>
        ''' <paramname="vicaht"> - TBD.</param>
        ''' <paramname="proDiscAng"> - TBD.</param>
        ''' <paramname="envDiscAng"> - TBD.</param>
        ''' <paramname="proEl"> - TBD.</param>
        ''' <paramname="envEl"> - TBD.</param>
        ''' <returns></returns>
        Public Shared Function SetBothAntes(tpParm As TpParm, pProSite As FtSiteStr, pProSiteNulls As FtSiteStrNulls, nProAntNum As Integer, pEnvSite As FtSiteStr, pEnvSiteNulls As FtSiteStrNulls, nEnvAntNum As Integer, intBndcde As String, intAnum As Short, intAcode As String, intAuse As String, intNull As SQLLEN, intNullUse As SQLLEN, intGain As Single, intGainNull As SQLLEN, vicBndcde As String, vicAnum As Short, vicAcode As String, vicAuse As String, vicNull As SQLLEN, vicNullUse As SQLLEN, vicGain As Single, vicGainNull As SQLLEN, ttSite As TtSite, proAnte As TtAnte, proNulls As SQLLEN(), envAnte As TtAnte, envNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String, ByRef pat360 As Char(), intaht As Single, vicaht As Single, proDiscAng As Double, envDiscAng As Double, proEl As Double, envEl As Double) As Integer
            Dim rc As Integer
            Dim intaxref = ""
            Dim intamodel = ""
            Dim vicaxref = ""
            Dim vicamodel = ""
            Dim intagain = 0.0F
            Dim vicagain = 0.0F
            Dim axtype = 0
            Dim axtypenull As SQLLEN = 0
            Dim intmodnull As SQLLEN = 0
            Dim lIntgainnull As SQLLEN = 0
            Dim vicaxnull As SQLLEN = 0
            Dim vicmodnull As SQLLEN = 0
            Dim lVicgainnull As SQLLEN = 0
            Dim intaxnull As SQLLEN = 0

            Dim pAntStr As SuAntStr

            proAnte.interferer = "P"
            proAnte.intcall1 = ttSite.intcall1
            proAnte.intcall2 = ttSite.intcall2
            proAnte.viccall1 = ttSite.viccall1
            proAnte.viccall2 = ttSite.viccall2

            proAnte.intbndcde = intBndcde
            proAnte.intanum = intAnum
            proAnte.intacode = intAcode
            proAnte.intause = intAuse
            proAnte.intaht = intaht

            proAnte.vicbndcde = vicBndcde
            proAnte.vicanum = vicAnum
            proAnte.vicacode = vicAcode
            proAnte.vicause = vicAuse
            proAnte.vicaht = vicaht

            ' 	Store the elevations from site to other end 
            proAnte.intelev = proEl
            proAnte.vicelev = envEl

            '	Pull in the interfering antenna 
            rc = Suutils.SuGetAnt(proAnte.intacode, pAntStr)
            If rc = 0 Then
                ' Set the same variables as above.  
                intaxref = pAntStr.acAnt.axref
                NullHelper.BlankNull(pAntStr.acAnt.axref, intaxnull)

                intamodel = pAntStr.acAnt.amodel
                NullHelper.BlankNull(pAntStr.acAnt.axref, intmodnull)

                intagain = pAntStr.acAnt.again
                NullHelper.ZeroNull(pAntStr.acAnt.again, lIntgainnull)

                axtype = pAntStr.acAnt.axtype
                NullHelper.ZeroNull(pAntStr.acAnt.axtype, axtypenull)
            Else
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "*ERROR* getting antenna code {0}" & Microsoft.VisualBasic.Constants.vbLf, proAnte.intacode)
            End If

            If intaxnull <> Constant.DB_NULL Then
                proAnte.intaxref = intaxref
                proNulls(TtAnte.INTAXREF) = Constant.DB_NOT_NULL
            End If

            If intmodnull <> Constant.DB_NULL Then
                proAnte.intamodel = intamodel
                proNulls(TtAnte.INTAMODEL) = Constant.DB_NOT_NULL
            End If

            If lIntgainnull <> Constant.DB_NULL Then
                proAnte.intgain = intagain
                proNulls(TtAnte.INTGAIN) = Constant.DB_NOT_NULL
            End If

            If axtypenull <> Constant.DB_NULL Then
                If axtype = 1 OrElse axtype = 2 Then pat360(0) = "Y"c
            End If

            ' Now the victim antenna.  
            rc = Suutils.SuGetAnt(proAnte.vicacode, pAntStr)

            If rc = 0 Then
                ' Set the same variables as above.  
                vicaxref = pAntStr.acAnt.axref
                NullHelper.BlankNull(pAntStr.acAnt.axref, vicaxnull)

                vicamodel = pAntStr.acAnt.amodel
                NullHelper.BlankNull(pAntStr.acAnt.axref, vicmodnull)

                vicagain = pAntStr.acAnt.again
                NullHelper.ZeroNull(pAntStr.acAnt.again, lVicgainnull)

                axtype = pAntStr.acAnt.axtype
                NullHelper.ZeroNull(pAntStr.acAnt.axtype, axtypenull)
            Else
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbLf & "*ERROR* getting antenna code {0}" & Microsoft.VisualBasic.Constants.vbLf, proAnte.vicacode)
            End If

            If vicaxnull <> Constant.DB_NULL Then
                proAnte.vicaxref = vicaxref
                proNulls(TtAnte.VICAXREF) = Constant.DB_NOT_NULL
            End If

            If vicmodnull <> Constant.DB_NULL Then
                proAnte.vicamodel = vicamodel
                proNulls(TtAnte.VICAMODEL) = Constant.DB_NOT_NULL
            End If

            If lVicgainnull <> Constant.DB_NULL Then
                proAnte.vicgain = vicagain
                proNulls(TtAnte.VICGAIN) = Constant.DB_NOT_NULL
            End If

            If axtypenull <> Constant.DB_NULL Then
                If axtype = 1 OrElse axtype = 2 Then
                    pat360(1) = "Y"c
                End If
            End If


            proAnte.report = Constant.FALSE
            proAnte.subcaseno = 0

            proNulls(TtAnte.INTERFERER) = Constant.DB_NOT_NULL
            proNulls(TtAnte.INTCALL1) = Constant.DB_NOT_NULL
            proNulls(TtAnte.INTCALL2) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICCALL1) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICCALL2) = Constant.DB_NOT_NULL

            proNulls(TtAnte.INTBNDCDE) = Constant.DB_NOT_NULL
            proNulls(TtAnte.INTANUM) = Constant.DB_NOT_NULL
            proNulls(TtAnte.INTACODE) = intNull
            proNulls(TtAnte.INTAUSE) = intNullUse

            proNulls(TtAnte.VICBNDCDE) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICANUM) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICACODE) = vicNull
            proNulls(TtAnte.VICAUSE) = vicNullUse
            proNulls(TtAnte.INTAHT) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICAHT) = Constant.DB_NOT_NULL
            proNulls(TtAnte.INTELEV) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICELEV) = Constant.DB_NOT_NULL

            proNulls(TtAnte.REPORT) = Constant.DB_NOT_NULL
            proNulls(TtAnte.SUBCASENO) = Constant.DB_NOT_NULL

            ' *******************************************************\
            '  	Calculate the elevation angles between the two antennas.
            ' \********************************************************
            AxSub3.AxElev((ttSite.intgrnd + intaht) / 1000.0, (ttSite.vicgrnd + vicaht) / 1000.0, ttSite.int1vic1dist, proAnte.intvicel, proAnte.vicintel)

            proNulls(TtAnte.INTVICEL) = Constant.DB_NOT_NULL
            proNulls(TtAnte.VICINTEL) = Constant.DB_NOT_NULL

            rc = TpRunTsip.TtCalcs.TtAnteCalcs(tpParm, pProSite, pProSiteNulls, nProAntNum, pEnvSite, pEnvSiteNulls, nEnvAntNum, proAnte, ttSite, proNulls, intPrintMsg, vicPrintMsg, proDiscAng, envDiscAng)

            If rc <> Constant.SUCCESS Then
                Return rc
            End If

            If Not tpParm.envtype.Equals("INTRA") Then

                envAnte.interferer = "E"
                envAnte.intcall1 = ttSite.viccall1
                envAnte.intcall2 = ttSite.viccall2
                envAnte.viccall1 = ttSite.intcall1
                envAnte.viccall2 = ttSite.intcall2

                envAnte.intbndcde = vicBndcde
                envAnte.intanum = vicAnum
                envAnte.intacode = vicAcode
                envAnte.intause = vicAuse
                envAnte.intgain = vicGain
                envAnte.intaht = vicaht
                envAnte.intelev = envEl
                envAnte.vicelev = proEl

                envAnte.vicaxref = intaxref
                envAnte.intaxref = vicaxref
                envAnte.vicamodel = intamodel
                envAnte.intamodel = vicamodel

                envAnte.vicbndcde = intBndcde
                envAnte.vicanum = intAnum
                envAnte.vicacode = intAcode
                envAnte.vicause = intAuse
                envAnte.vicgain = intGain
                envAnte.vicaht = intaht

                ' 	Calculate the elevation angles for the Environment pair 
                AxSub3.AxElev((ttSite.vicgrnd + vicaht) / 1000.0, (ttSite.intgrnd + intaht) / 1000.0, ttSite.int1vic1dist, envAnte.intvicel, envAnte.vicintel)

                envNulls(TtAnte.INTVICEL) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICINTEL) = Constant.DB_NOT_NULL

                envAnte.report = Constant.FALSE
                envAnte.subcaseno = 0

                envNulls(TtAnte.INTERFERER) = Constant.DB_NOT_NULL
                envNulls(TtAnte.INTCALL1) = Constant.DB_NOT_NULL
                envNulls(TtAnte.INTCALL2) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICCALL1) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICCALL2) = Constant.DB_NOT_NULL
                envNulls(TtAnte.INTBNDCDE) = Constant.DB_NOT_NULL
                envNulls(TtAnte.INTANUM) = Constant.DB_NOT_NULL
                envNulls(TtAnte.INTACODE) = vicNull
                envNulls(TtAnte.INTAUSE) = vicNullUse
                envNulls(TtAnte.INTGAIN) = vicGainNull
                envNulls(TtAnte.VICBNDCDE) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICANUM) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICACODE) = intNull
                envNulls(TtAnte.VICAUSE) = intNullUse
                envNulls(TtAnte.INTAHT) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICAHT) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICGAIN) = intGainNull
                envNulls(TtAnte.REPORT) = Constant.DB_NOT_NULL
                envNulls(TtAnte.SUBCASENO) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICAXREF) = intaxnull
                envNulls(TtAnte.INTAXREF) = vicaxnull
                envNulls(TtAnte.VICAMODEL) = intmodnull
                envNulls(TtAnte.INTAMODEL) = vicmodnull
                envNulls(TtAnte.INTELEV) = Constant.DB_NOT_NULL
                envNulls(TtAnte.VICELEV) = Constant.DB_NOT_NULL

                envAnte.adiscctxh = proAnte.adisccrxh
                envAnte.adiscctxv = proAnte.adisccrxv
                envAnte.adisccrxh = proAnte.adiscctxh
                envAnte.adisccrxv = proAnte.adiscctxv

                envAnte.adiscxtxh = proAnte.adiscxrxh
                envAnte.adiscxtxv = proAnte.adiscxrxv
                envAnte.adiscxrxh = proAnte.adiscxtxh
                envAnte.adiscxrxv = proAnte.adiscxtxv

                envNulls(TtAnte.ADISCCTXH) = proNulls(TtAnte.ADISCCRXH)
                envNulls(TtAnte.ADISCCTXV) = proNulls(TtAnte.ADISCCRXV)
                envNulls(TtAnte.ADISCCRXH) = proNulls(TtAnte.ADISCCTXH)
                envNulls(TtAnte.ADISCCRXV) = proNulls(TtAnte.ADISCCTXV)

                envNulls(TtAnte.ADISCXTXH) = proNulls(TtAnte.ADISCXRXH)
                envNulls(TtAnte.ADISCXTXV) = proNulls(TtAnte.ADISCXRXV)
                envNulls(TtAnte.ADISCXRXH) = proNulls(TtAnte.ADISCXTXH)
                envNulls(TtAnte.ADISCXRXV) = proNulls(TtAnte.ADISCXTXV)
            End If
            Return Constant.SUCCESS

        End Function ' ---- end setBothAntes ----

        ''' <summary>
        ''' This method controls the culling and population 
        ''' of the TS Chan SH Table records.  
        ''' </summary>
        ''' <paramname="tpParmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="proSiteStruct"> - TtSite object describing the proposed site.</param>
        ''' <paramname="proSiteNulls"> - ODBC nullInds associated with pSiteStruct.</param>
        ''' <paramname="envSiteStruct"> - TtSite object describing the environment site.</param>
        ''' <paramname="envSiteNulls"> - ODBC nullInds associated with pSiteStruct.</param>
        ''' <paramname="proAnteStruct"> - TtAnte object describing the proposed site antenna.</param>
        ''' <paramname="proAnteNulls"> - ODBC nullInds associated with proAnteStruct.</param>
        ''' <paramname="envAnteStruct"> - TtAnte object describing the environment site antenna.</param></param>
        ''' <param name="envAnteNulls"> - ODBC nullInds associated with envAnteStruct.</param>
        ''' <param name="proName"> - table name of proposed site.</param>
        ''' <param name="envName"> - table name of environment site.</param>
        ''' <param name="numProChan"> - number of proposed site/antenna channels.</param>
        ''' <param name="numEnvChan"> - number of environment site/antenna channels.</param>
        ''' <param name="numCases"> - cummulative number of interference cases.</param>
        ''' <param name="envMBnd"> - TBD.</param>
        ''' <param name="proMBnd"> - TBD.</param>
        ''' <param name="envPatLoss"> - TBD.</param>
        ''' <param name="proPatLoss"> - TBD.</param>
        ''' <returns></returns>
        Public Shared Function CreateChanSHTable(tpParmStruct As TpParm, proSiteStruct As TtSite, proSiteNulls As SQLLEN(), envSiteStruct As TtSite, envSiteNulls As SQLLEN(), proAnteStruct As TtAnte, proAnteNulls As SQLLEN(), envAnteStruct As TtAnte, envAnteNulls As SQLLEN(), proName As String, envName As String, <Out> ByRef numProChan As Integer, <Out> ByRef numEnvChan As Integer, ByRef numCases As Integer, envMBnd As Double, proMBnd As Double, envPatLoss As Double, proPatLoss As Double) As Integer
            '...Log2.v(tpParmStruct.ToString());
            '...Log2.v(proSiteStruct.ToString());
            '...Log2.v(envSiteStruct.ToString());
            '...Log2.v(proAnteStruct.ToString());
            '...Log2.v("\nproName = " + proName);
            '...Log2.v("\nenvName = " + envName);
            '...Log2.v("\nnumCases = " + numCases);
            '...Log2.v("\nenvMBnd = " + envMBnd);
            '...Log2.v("\nproMBnd = " + proMBnd);
            '...Log2.v("\nenvPatLoss = " + envPatLoss);
            '...Log2.v("\nproPatLoss = " + proPatLoss);
            '...Log2.v("\n");

            '...Log2.v("\nTtBuildSH.CreateChanSHTable(): Entry");

            ' 'out' requirements.
            numProChan = 0
            numEnvChan = 0

            Dim ftProChanHandle As Integer    ' interferer pdf chan handle-dynamic 
            Dim ftEnvChanHandle As Integer    ' victim pdf chan handle - dynamic 
            Dim rc1 As Integer
            Dim rc As Integer
            Dim rc2 As Integer
            Dim proChanName = ""
            Dim envChanName = ""
            Dim proChanNulls As SQLLEN()
            Dim envChanNulls As SQLLEN()
            Dim ftProChanNulls As SQLLEN()   ' interferer pdf chan nulls
            Dim ftEnvChanNulls As SQLLEN()   ' victim pdf chan nulls 
            Dim proPatNulls As SQLLEN()
            Dim envPatNulls As SQLLEN()
            Dim intPcSkip = False
            Dim vicPcSkip = False
            Dim proAntDiscW = 0.0
            Dim envAntDiscW = 0.0
            Dim proSelection As String    ' interferer selectn criteria-dynamic
            Dim envSelection As String ' victim selection criteria - dynamic
            Dim intPrintMsg As String
            Dim vicPrintMsg As String
            Dim proChanStruct As TtChan   ' TSIP chan recort structure 
            Dim envChanStruct As TtChan   ' TSIP chan recort structure 
            Dim ftProChanStruct As FtChan ' interferer pdf chan record struct
            Dim ftEnvChanStruct As FtChan ' victim pdf chan record structure 

            ' Instantiate data structure objects and nullInd arrays.
            envChanStruct = New TtChan()
            envChanNulls = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            proChanStruct = New TtChan()
            proChanNulls = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            proPatNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)
            envPatNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            ' calculate the worst antenna discrimination for the proposed ante,
            '  this will be used in the calculations 
            rc = TpRunTsip.TtBuildSH.CalcADiscW(proAnteNulls, proAnteStruct, proAntDiscW)

            If rc <> Constant.SUCCESS Then
                '...Log2.v("\ncreateChanSHTable(): Exit: A");
                Dim str = String.Format("createChanSHTable01: Could not calculate proposed antenna discrimination. ({0})", rc)
                Log2.e(Microsoft.VisualBasic.Constants.vbVerticalTab & "TtBuildSH.CreateChanSHTable(): ERROR: call to CalcADiscW() failed: " & str)
                GenUtil.SetErr(str)
                Return Constant.FAILURE
            End If

            ' if (env. file type is not "INTRA") 
            If Not tpParmStruct.envtype.Equals("INTRA") Then

                ' calculate the worst antenna discrimination for the env. ante
                '  this will be used in the calculations 
                rc = TpRunTsip.TtBuildSH.CalcADiscW(envAnteNulls, envAnteStruct, envAntDiscW)

                If rc <> Constant.SUCCESS Then
                    '...Log2.v("\ncreateChanSHTable(): Exit: B");
                    Dim str = String.Format("createChanSHTable02: Could not calculate environment antenna discrimination. ({0})", rc)
                    Log2.e(Microsoft.VisualBasic.Constants.vbVerticalTab & "TtBuildSH.CreateChanSHTable(): ERROR: call to CalcADiscW() failed: " & str)
                    GenUtil.SetErr(str)
                    Return Constant.FAILURE
                End If
            End If

            '...Log2.v("\ncreateChanSHTable(): 1");

            ' proposed selection 
            proSelection = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbtx1 = {3} or antnumbtx2 = {4} or antnumbrx1 = {5} or antnumbrx2 = {6} or antnumbrx3 = {7}) and cmd!='D'", proAnteStruct.intcall1, proAnteStruct.intcall2, proAnteStruct.intbndcde, proAnteStruct.intanum, proAnteStruct.intanum, proAnteStruct.intanum, proAnteStruct.intanum, proAnteStruct.intanum)

            '...Log2.v("\nTtBuildSH.CreateChanSHTable(): proSelection = " + proSelection);

            ' environment selection 
            envSelection = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbrx1 = {3} or antnumbrx2 = {4} or antnumbrx3 = {5} or antnumbtx1 = {6} or antnumbtx2 = {7})", proAnteStruct.viccall1, proAnteStruct.viccall2, proAnteStruct.vicbndcde, proAnteStruct.vicanum, proAnteStruct.vicanum, proAnteStruct.vicanum, proAnteStruct.vicanum, proAnteStruct.vicanum)

            '...Log2.v("\nTtBuildSH.CreateChanSHTable(): envSelection = " + envSelection);

            proPatNulls(TtAnte.ADISCCTXV) = proAnteNulls(TtAnte.ADISCCTXV)
            proPatNulls(TtAnte.ADISCXTXV) = proAnteNulls(TtAnte.ADISCXTXV)
            proPatNulls(TtAnte.ADISCCTXH) = proAnteNulls(TtAnte.ADISCCTXH)
            proPatNulls(TtAnte.ADISCXTXH) = proAnteNulls(TtAnte.ADISCXTXH)
            proPatNulls(TtAnte.ADISCCRXV) = proAnteNulls(TtAnte.ADISCCRXV)
            proPatNulls(TtAnte.ADISCXRXV) = proAnteNulls(TtAnte.ADISCXRXV)
            proPatNulls(TtAnte.ADISCCRXH) = proAnteNulls(TtAnte.ADISCCRXH)
            proPatNulls(TtAnte.ADISCXRXH) = proAnteNulls(TtAnte.ADISCXRXH)

            ' if (env. file type is not "INTRA") 
            If Not tpParmStruct.envtype.Equals("INTRA") Then
                envPatNulls(TtAnte.ADISCCTXV) = envAnteNulls(TtAnte.ADISCCTXV)
                envPatNulls(TtAnte.ADISCXTXV) = envAnteNulls(TtAnte.ADISCXTXV)
                envPatNulls(TtAnte.ADISCCTXH) = envAnteNulls(TtAnte.ADISCCTXH)
                envPatNulls(TtAnte.ADISCXTXH) = envAnteNulls(TtAnte.ADISCXTXH)
                envPatNulls(TtAnte.ADISCCRXV) = envAnteNulls(TtAnte.ADISCCRXV)
                envPatNulls(TtAnte.ADISCXRXV) = envAnteNulls(TtAnte.ADISCXRXV)
                envPatNulls(TtAnte.ADISCCRXH) = envAnteNulls(TtAnte.ADISCCRXH)
                envPatNulls(TtAnte.ADISCXRXH) = envAnteNulls(TtAnte.ADISCXRXH)
            End If

            ' select from the proposed file all channels belonging to the current
            '  proposed ante. 
            ' 
            '  TASK 497: In order to allow users to access dba PDF's, we must specify
            '  that we are not opening this cursor for update, even though we have
            '  no ordering cirteria. This is what "NU" means.
            ' 


            GenUtil.SetFullChanName(proName, tpParmStruct.protype, proChanName)
            GenUtil.SetFullChanName(envName, tpParmStruct.envtype, envChanName)

            '...Log2.v("\nTtBuildSH.CreateChanSHTable(): proChanName = " + proChanName);
            '...Log2.v("\nTtBuildSH.CreateChanSHTable(): envChanName = " + envChanName);

            ftProChanHandle = DynChannel.FtSelectChannel(proChanName, proSelection, "NU")

            If ftProChanHandle < 0 Then
                '...Log2.v("\ncreateChanSHTable(): Exit: C");
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.CreateChanSHTable(): ERROR: call to FtSelectChannel() failed.")
                Return ftProChanHandle
            End If

            '...Log2.v("\ncreateChanSHTable(): 2");

            ' for (each channel selected) 
            While CSharpImpl.__Assign(rc, DynChannel.FtFetchChannel(ftProChanHandle, ftProChanStruct, ftProChanNulls)) = Constant.SUCCESS
                '...Log2.v("\ncreateChanSHTable(): 3");

                ' select from the environment file all channels belonging to
                '  the current environment ante. 
                If CSharpImpl.__Assign(ftEnvChanHandle, TpRunTsip.TpMdbPdfGet.TtTtSelectChannel(envChanName, envSelection, tpParmStruct.envtype)) < 0 Then
                    '...Log2.v("\ncreateChanSHTable(): Exit: D");
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.CreateChanSHTable(): ERROR: call to TtTtSelectChannel() failed")
                    Return ftEnvChanHandle
                End If

                '...Log2.v("\ncreateChanSHTable(): 4");

                ' for (each channel selected) 
                While CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtTtFetchChannel(ftEnvChanHandle, ftEnvChanStruct, ftEnvChanNulls, tpParmStruct.envtype)) = Constant.SUCCESS
                    '...Log2.v("\ncreateChanSHTable(): 5");

                    ' proposed is interferer 
                    If Not intPcSkip Then

                        ' if (pro. chan. antnumbtx1 is same as pro. ante. intanum
                        '  or pro. chan. antnumbtx2 is same as pro. ante. intanum) 
                        If ftProChanStruct.antnumbtx1 = proAnteStruct.intanum OrElse ftProChanStruct.antnumbtx2 = proAnteStruct.intanum Then

                            ' if (env. chan. antnumbrx1 is same as env. ante. intanum
                            '  or env. chan. antnumbrx2 is same as env. ante. intanum
                            '  or env. chan. antnumbrx3 is same as env. ante. intanum) 
                            If ftEnvChanStruct.antnumbrx1 = proAnteStruct.vicanum OrElse ftEnvChanStruct.antnumbrx2 = proAnteStruct.vicanum OrElse ftEnvChanStruct.antnumbrx3 = proAnteStruct.vicanum Then

                                intPrintMsg = String.Format("INTERFERER:  {0,10}   {1,10}   {2,5}   {3,4:D}   {4}", proAnteStruct.intcall1, proAnteStruct.intcall2, proAnteStruct.intbndcde, proAnteStruct.intanum, ftProChanStruct.chid)

                                vicPrintMsg = String.Format("VICTIM:      {0,10}   {1,10}   {2,5}   {3,4:D}   {4}", proAnteStruct.viccall1, proAnteStruct.viccall2, proAnteStruct.vicbndcde, proAnteStruct.vicanum, ftEnvChanStruct.chid)

                                ' check if the transmit status code is among the
                                '  status codes specified by the user 


                                rc2 = TpRunTsip.TtBuildSH.ChanCull(tpParmStruct, proAnteStruct.interferer, ftProChanStruct, ftEnvChanStruct)

                                If rc2 = Constant.SUCCESS Then
                                    '...Log2.v("\ncreateChanSHTable(): 6");

                                    numProChan += 1

                                    ' 	Set the value of the proChanStruct to be the values of the
                                    ' 		proposed ft channel into the environment ft channel and runs
                                    ' 		the interference calculation.  The value of 'report' in the
                                    ' 		proChanStruct will be 1 if there was interference 
                                    rc1 = TpRunTsip.TtBuildSH.TtSetPCChan(tpParmStruct, ftProChanStruct, ftProChanNulls, ftEnvChanStruct, ftEnvChanNulls, proSiteStruct, proAnteStruct, proAnteNulls, proChanStruct, proChanNulls, proPatNulls, intPrintMsg, vicPrintMsg, proAntDiscW, proMBnd, envMBnd, proPatLoss)


                                    If rc1 <> Constant.SUCCESS Then
                                        If rc1 = Constant.PC_SKIP Then
                                            intPcSkip = True
                                        Else
                                            TpRunTsip.TpMdbPdfGet.TtTtCloseChannel(ftEnvChanHandle, tpParmStruct.envtype)
                                            DynChannel.FtCloseChannel(ftProChanHandle)
                                            '...Log2.v("\ncreateChanSHTable(): Exit: E");
                                            Return rc1
                                        End If
                                    End If

                                    '...Log2.v("\ncreateChanSHTable(): 7");

                                    ' if (analysis option is "BAND") 
                                    If tpParmStruct.analopt.Equals("BAND") Then
                                        '...Log2.v("\ncreateChanSHTable(): 7-1");
                                        intPcSkip = True
                                    End If

                                    If proChanStruct.report = Constant.TRUE Then
                                        '...Log2.v("\ncreateChanSHTable(): 7-2");
                                        ' 	There was Proposed into Environment interference on these
                                        ' 		channels 
                                        If proSiteStruct.report <> Constant.TRUE Then
                                            '...Log2.v("\ncreateChanSHTable(): 7-3");
                                            ' 	This site has not yet had any interference.  Now it has 
                                            proSiteStruct.report = Constant.TRUE
                                            proSiteStruct.caseno = Threading.Interlocked.Increment(numCases)
                                            proSiteStruct.subcases = 1

                                            proSiteNulls(TtSite.REPORT) = Constant.DB_NOT_NULL
                                            proSiteNulls(TtSite.CASENO) = Constant.DB_NOT_NULL
                                            proSiteNulls(TtSite.SUBCASES) = Constant.DB_NOT_NULL
                                        ElseIf proAnteStruct.report <> Constant.TRUE Then
                                            '...Log2.v("\ncreateChanSHTable(): 7-4");
                                            ' 	This antenna has had no interference yet 
                                            proSiteStruct.subcases += 1
                                        End If

                                        If proAnteStruct.report <> Constant.TRUE Then
                                            '...Log2.v("\ncreateChanSHTable(): 7-5");
                                            proAnteStruct.report = Constant.TRUE
                                            proAnteStruct.caseno = proSiteStruct.caseno
                                            proAnteStruct.subcaseno = proSiteStruct.subcases
                                            proAnteNulls(TtAnte.REPORT) = Constant.DB_NOT_NULL
                                            proAnteNulls(TtAnte.SUBCASENO) = Constant.DB_NOT_NULL
                                        End If

                                        '...Log2.v("\ncreateChanSHTable(): 7-6");

                                        ' 	Store the case number in the channel as well. 
                                        proChanStruct.caseno = proSiteStruct.caseno
                                        proChanNulls(TtChan.CASENO) = Constant.DB_NOT_NULL

                                        ' insert the proposed chan data into
                                        '  the chan SH Table 
                                        rc = TpRunTsip.TtDynChan.TtInsertChan(proChanStruct, proChanNulls)

                                        If rc <> Constant.SUCCESS Then
                                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TtBuildSH.CreateChanSHTable(): ERROR: call to TtInsertChan() failed.")
                                            ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                            ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                            TpRunTsip.TpRunTsip.mTW_ERR.Write("Error: Inserting tt Channel returns {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                                            '...Log2.v("\ncreateChanSHTable(): Exit: F");
                                            Return rc
                                        End If

                                    End If  'if (proChanStruct.report == Constant.TRUE)
                                End If  'if (rc2 == Constant.SUCCESS)
                            End If  'if (ftEnvChanStruct.antnumbrx1 == proAnteStruct.vicanum ||
                        End If  'if (ftProChanStruct.antnumbtx1 == proAnteStruct.intanum ||
                    End If  'if (intPcSkip)

                    '...Log2.v("\ncreateChanSHTable(): 8");

                    ' if (env. file type is not "INTRA") 
                    If Not tpParmStruct.envtype.Equals("INTRA") Then
                        If vicPcSkip = False Then
                            ' if (pro. chan. antnumbrx1 is same as pro. ante. intanum
                            '  or pro. chan. antnumbrx2 is same as pro. ante. intanum
                            '  or pro. chan. antnumbrx3 is same as pro. ante. intanum) 
                            If ftProChanStruct.antnumbrx1 = proAnteStruct.intanum OrElse ftProChanStruct.antnumbrx2 = proAnteStruct.intanum OrElse ftProChanStruct.antnumbrx3 = proAnteStruct.intanum Then

                                ' if (env. chan. antnumbtx1 is same as env. ante. intanum
                                '  or env. chan. antnumbtx2 is same as env. ante. intanum) 
                                If ftEnvChanStruct.antnumbtx1 = envAnteStruct.intanum OrElse ftEnvChanStruct.antnumbtx2 = envAnteStruct.intanum Then
                                    ' environment is interferer 

                                    intPrintMsg = String.Format("INTERFERER:  {0,10}   {1,10}   {2,5}   {3,4:D}   {4}", envAnteStruct.intcall1, envAnteStruct.intcall2, envAnteStruct.intbndcde, envAnteStruct.intanum, ftEnvChanStruct.chid)

                                    vicPrintMsg = String.Format("VICTIM:      {0,10}   {1,10}   {2,5}   {3,4:D}   {4}", envAnteStruct.viccall1, envAnteStruct.viccall2, envAnteStruct.vicbndcde, envAnteStruct.vicanum, ftEnvChanStruct.chid)

                                    ' check if receive status code is among the
                                    '  status codes specified by the user 
                                    rc2 = TpRunTsip.TtBuildSH.ChanCull(tpParmStruct, envAnteStruct.interferer, ftEnvChanStruct, ftProChanStruct)


                                    If rc2 = Constant.SUCCESS Then
                                        '...Log2.v("\ncreateChanSHTable(): 9");

                                        numEnvChan += 1

                                        ' 	set the values of the TS Chan SH record from the pro. and env.
                                        ' 		files, ttSetPCChan() - calls ttChanCalcs() to finish the
                                        ' 		population of the TS Chan SH records - if its return code is
                                        '  	PC_SKIP then vicPcSkip = true 
                                        rc1 = TpRunTsip.TtBuildSH.TtSetPCChan(tpParmStruct, ftEnvChanStruct, ftEnvChanNulls, ftProChanStruct, ftProChanNulls, envSiteStruct, envAnteStruct, envAnteNulls, envChanStruct, envChanNulls, envPatNulls, intPrintMsg, vicPrintMsg, envAntDiscW, envMBnd, proMBnd, envPatLoss)

                                        If rc1 <> Constant.SUCCESS Then
                                            '...Log2.v("\ncreateChanSHTable(): 10");

                                            If rc1 = Constant.PC_SKIP Then
                                                vicPcSkip = True
                                            Else
                                                TpRunTsip.TpMdbPdfGet.TtTtCloseChannel(ftEnvChanHandle, tpParmStruct.envtype)
                                                DynChannel.FtCloseChannel(ftProChanHandle)

                                                '...Log2.v("\ncreateChanSHTable(): Exit: G");
                                                Return rc1
                                            End If
                                        End If


                                        ' if (analysis option is "BAND") 
                                        If tpParmStruct.analopt.Equals("BAND") Then
                                            vicPcSkip = True
                                        End If

                                        If envChanStruct.report = Constant.TRUE Then
                                            If envSiteStruct.report <> Constant.TRUE Then
                                                envSiteStruct.report = Constant.TRUE
                                                envSiteStruct.caseno = Threading.Interlocked.Increment(numCases)
                                                envSiteStruct.subcases = 1
                                                envSiteNulls(TtSite.REPORT) = Constant.DB_NOT_NULL
                                                envSiteNulls(TtSite.CASENO) = Constant.DB_NOT_NULL
                                                envSiteNulls(TtSite.SUBCASES) = Constant.DB_NOT_NULL
                                            ElseIf envAnteStruct.report <> Constant.TRUE Then
                                                envSiteStruct.subcases += 1
                                            End If
                                            If envAnteStruct.report <> Constant.TRUE Then
                                                envAnteStruct.report = Constant.TRUE
                                                envAnteStruct.caseno = envSiteStruct.caseno
                                                envAnteStruct.subcaseno = envSiteStruct.subcases
                                                envAnteNulls(TtAnte.REPORT) = Constant.DB_NOT_NULL
                                                envAnteNulls(TtAnte.SUBCASENO) = Constant.DB_NOT_NULL
                                            End If

                                            ' 	Store the case number in the channel as well 
                                            envChanStruct.caseno = envSiteStruct.caseno
                                            envChanNulls(TtChan.CASENO) = Constant.DB_NOT_NULL

                                            ' insert the proposed chan
                                            '  data into the chan SH Table
                                            rc = TpRunTsip.TtDynChan.TtInsertChan(envChanStruct, envChanNulls)
                                            '...Log2.v("\ncreateChanSHTable(): 11");

                                            If rc <> Constant.SUCCESS Then
                                                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                                TpRunTsip.TpRunTsip.mTW_ERR.Write("Error inserting tt Channel(3) returns {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                                                '...Log2.v("\ncreateChanSHTable(): Exit: H");
                                                Return rc
                                            End If
                                        End If  'if (envChanStruct.report == Constant.TRUE)
                                    End If  'if (chanCull(tpParmStruct, envAnteStruct.interferer,
                                End If  'if (ftEnvChanStruct.antnumbtx1 == envAnteStruct.intanum ||
                            End If  'if (ftProChanStruct.antnumbrx1 == proAnteStruct.intanum ||
                        End If  ' if (vicPcSkip == false)
                    End If  'if (!tpParmStruct.envtype.Equals("INTRA"))

                    If intPcSkip AndAlso vicPcSkip Then
                        Exit While ' end for (each channel) 
                    End If
                End While

                TpRunTsip.TpMdbPdfGet.TtTtCloseChannel(ftEnvChanHandle, tpParmStruct.envtype)
                If intPcSkip AndAlso vicPcSkip Then
                    Exit While ' end for (each channel) 
                End If

                '...Log2.v("\ncreateChanSHTable(): 12");

            End While 'while ((rc = DynChannel.FtFetchChannel    /* end for (each channel) 

            If rc = [Error].DYN_MS_SQL_SERVER_ERR Then
                '...Log2.v("\ncreateChanSHTable(): Exit: I");
                Return rc
            End If
            DynChannel.FtCloseChannel(ftProChanHandle)

            '...Log2.v("\nnumProChan = " + numProChan);
            '...Log2.v("\nnumEnvChan = " + numEnvChan);

            '...Log2.v("\ncreateChanSHTable(): Exit: final");
            '...Log2.v("\nTtBuildSH.CreateChanSHTable(): Exit: final");
            Return Constant.SUCCESS

        End Function ' ----- end createChanSHTable ----- 

        ''' <summary>
        ''' This method sets the values of the TS Chan SH Table from the 
        ''' proposed and environment files if the User specified Plan/Channel 
        ''' calculations; it also calls the TtChanCalcs() method to complete the 
        ''' population of the TS Chan record.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="intChanStruct"> - FtChan object describing the interferer's channel.</param>
        ''' <paramname="intNulls"> - ODBC nullInds associated with intChanStruct.</param>
        ''' <paramname="vicChanStruct"> - FtChan object describing the victim's channel.</param>
        ''' <paramname="vicNulls"> - ODBC nullInds associated with vicChanStruct.</param>
        ''' <paramname="ttSite"> - TtSite object.</param>
        ''' <paramname="ttAnte"> - TtAnte object.</param>
        ''' <paramname="ttAnteNulls"> - ODBC nullInds associated with ttAnte.</param>
        ''' <paramname="ttChan"> - TtChan object.</param>
        ''' <paramname="ttChanNulls"> - ODBC nullInds associated with ttChan.</param>
        ''' <paramname="patNulls"> - TBD.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <paramname="intADiscW"> - TBD.</param>
        ''' <paramname="intMBnd"> - TBD.</param>
        ''' <paramname="vicMBnd"> - TBD.</param>
        ''' <paramname="intPatLoss"> - TBD.</param>
        ''' <returns></returns>
        Public Shared Function TtSetPCChan(tpParm As TpParm, intChanStruct As FtChan, intNulls As SQLLEN(), vicChanStruct As FtChan, vicNulls As SQLLEN(), ttSite As TtSite, ttAnte As TtAnte, ttAnteNulls As SQLLEN(), <Out> ByRef ttChan As TtChan, <Out> ByRef ttChanNulls As SQLLEN(), patNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String, intADiscW As Double, intMBnd As Double, vicMBnd As Double, intPatLoss As Double) As Integer
            ' 'out' requirements.
            ttChan = New TtChan()
            ttChanNulls = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            '...Log2.v("\nTtBuildSH.TtSetPCChan(): Entry");

            Dim intAFSLtx = 0.0
            Dim nullIntAfslTx As SQLLEN

            If ttAnte.intanum = intChanStruct.antnumbtx1 Then
                ttChan.txant = 1
            Else
                ttChan.txant = 2
            End If
            ttChanNulls(_DataStructures.TtChan.TXANT) = Constant.DB_NOT_NULL

            ttChan.interferer = ttAnte.interferer
            ttChan.intcall1 = ttAnte.intcall1
            ttChan.intcall2 = ttAnte.intcall2
            ttChan.intbndcde = ttAnte.intbndcde
            ttChan.intanum = ttAnte.intanum
            ttChan.viccall1 = ttAnte.viccall1
            ttChan.viccall2 = ttAnte.viccall2
            ttChan.vicbndcde = ttAnte.vicbndcde
            ttChan.vicanum = ttAnte.vicanum

            ttChan.intchid = intChanStruct.chid
            ttChan.intpolar = intChanStruct.poltx
            ttChan.inttraftx = intChanStruct.traftx
            ttChan.inteqpttx = intChanStruct.eqpttx
            ttChan.intstattx = intChanStruct.stattx
            ttChan.intfreqtx = intChanStruct.freqtx
            ttChan.intpwrtx = CDbl(intChanStruct.pwrtx)
            ttChanNulls(_DataStructures.TtChan.INTPWRTX) = intNulls(FtChan.PWRTX)

            If ttChan.intanum = intChanStruct.antnumbtx1 Then
                ttChan.intafsltx = CDbl(intChanStruct.afsltx1)
                ttChanNulls(_DataStructures.TtChan.INTAFSLTX) = intNulls(FtChan.AFSLTX1)
            Else
                ttChan.intafsltx = CDbl(intChanStruct.afsltx2)
                ttChanNulls(_DataStructures.TtChan.INTAFSLTX) = intNulls(FtChan.AFSLTX2)
            End If

            ttChan.vicchid = vicChanStruct.chid
            ttChan.vicpolar = vicChanStruct.polrx
            ttChan.victrafrx = vicChanStruct.trafrx
            ttChan.viceqptrx = vicChanStruct.eqptrx
            ttChan.vicstatrx = vicChanStruct.statrx
            ttChan.vicfreqrx = vicChanStruct.freqrx

            If ttChan.vicanum = vicChanStruct.antnumbrx1 Then
                ttChan.vicpwrrx = CDbl(vicChanStruct.pwrrx1)
                ttChanNulls(_DataStructures.TtChan.VICPWRRX) = vicNulls(FtChan.PWRRX1)
                ttChan.vicafslrx = CDbl(vicChanStruct.afslrx1)
                ttChanNulls(_DataStructures.TtChan.VICAFSLRX) = vicNulls(FtChan.AFSLRX1)
                ttChan.rxant = 1
            ElseIf ttChan.vicanum = vicChanStruct.antnumbrx2 Then
                ttChan.vicpwrrx = CDbl(vicChanStruct.pwrrx2)
                ttChanNulls(_DataStructures.TtChan.VICPWRRX) = vicNulls(FtChan.PWRRX2)
                ttChan.vicafslrx = CDbl(vicChanStruct.afslrx2)
                ttChanNulls(_DataStructures.TtChan.VICAFSLRX) = vicNulls(FtChan.AFSLRX2)
                ttChan.rxant = 2
            Else
                ttChan.vicpwrrx = CDbl(vicChanStruct.pwrrx3)
                ttChanNulls(_DataStructures.TtChan.VICPWRRX) = vicNulls(FtChan.PWRRX3)
                ttChan.vicafslrx = CDbl(vicChanStruct.afslrx3)
                ttChanNulls(_DataStructures.TtChan.VICAFSLRX) = vicNulls(FtChan.AFSLRX3)
                ttChan.rxant = 3
            End If
            ttChanNulls(_DataStructures.TtChan.RXANT) = Constant.DB_NOT_NULL

            ttChan.report = Constant.FALSE
            ttChanNulls(_DataStructures.TtChan.REPORT) = Constant.DB_NOT_NULL

            ttChanNulls(_DataStructures.TtChan.INTERFERER) = ttAnteNulls(_DataStructures.TtAnte.INTERFERER)
            ttChanNulls(_DataStructures.TtChan.INTCALL1) = ttAnteNulls(_DataStructures.TtAnte.INTCALL1)
            ttChanNulls(_DataStructures.TtChan.INTCALL2) = ttAnteNulls(_DataStructures.TtAnte.INTCALL2)
            ttChanNulls(_DataStructures.TtChan.INTBNDCDE) = ttAnteNulls(_DataStructures.TtAnte.INTBNDCDE)
            ttChanNulls(_DataStructures.TtChan.INTANUM) = ttAnteNulls(_DataStructures.TtAnte.INTANUM)
            ttChanNulls(_DataStructures.TtChan.VICCALL1) = ttAnteNulls(_DataStructures.TtAnte.VICCALL1)
            ttChanNulls(_DataStructures.TtChan.VICCALL2) = ttAnteNulls(_DataStructures.TtAnte.VICCALL2)
            ttChanNulls(_DataStructures.TtChan.VICBNDCDE) = ttAnteNulls(_DataStructures.TtAnte.VICBNDCDE)
            ttChanNulls(_DataStructures.TtChan.VICANUM) = ttAnteNulls(_DataStructures.TtAnte.VICANUM)

            ttChanNulls(_DataStructures.TtChan.INTCHID) = intNulls(FtChan.CHID)
            ttChanNulls(_DataStructures.TtChan.INTPOLAR) = intNulls(FtChan.POLTX)
            ttChanNulls(_DataStructures.TtChan.INTEQPTTX) = intNulls(FtChan.EQPTTX)
            ttChanNulls(_DataStructures.TtChan.INTTRAFTX) = intNulls(FtChan.TRAFTX)
            ttChanNulls(_DataStructures.TtChan.INTFREQTX) = intNulls(FtChan.FREQTX)
            ttChanNulls(_DataStructures.TtChan.INTSTATTX) = intNulls(FtChan.STATTX)

            ttChanNulls(_DataStructures.TtChan.VICCHID) = vicNulls(FtChan.CHID)
            ttChanNulls(_DataStructures.TtChan.VICPOLAR) = vicNulls(FtChan.POLRX)
            ttChanNulls(_DataStructures.TtChan.VICTRAFRX) = vicNulls(FtChan.TRAFRX)
            ttChanNulls(_DataStructures.TtChan.VICEQPTRX) = vicNulls(FtChan.EQPTRX)
            ttChanNulls(_DataStructures.TtChan.VICFREQRX) = vicNulls(FtChan.FREQRX)
            ttChanNulls(_DataStructures.TtChan.VICSTATRX) = vicNulls(FtChan.STATRX)

            If Strings.FirstCharIs(intChanStruct.call1, "%"c) Then
                intAFSLtx = 0.0
                nullIntAfslTx = Constant.DB_NOT_NULL
            Else
                If ttChan.txant = 1 Then
                    intAFSLtx = intChanStruct.afsltx1
                    nullIntAfslTx = intNulls(FtChan.AFSLTX1)
                Else
                    intAFSLtx = intChanStruct.afsltx2
                    nullIntAfslTx = intNulls(FtChan.AFSLTX2)
                End If
            End If

            '...Log2.v("\nttSetPCChan(): before call to ttChanCalcs()...");

            Dim rc As Integer = TpRunTsip.TtCalcs.TtChanCalcs(ttChan, ttAnte, ttAnteNulls, ttSite, tpParm, patNulls, ttChanNulls, intChanStruct.pwrtx, intAFSLtx, nullIntAfslTx, intPrintMsg, vicPrintMsg, intADiscW, intMBnd, vicMBnd, intPatLoss)

            Dim test1 = ttChanNulls(_DataStructures.TtChan.INTERFERER) = Constant.DB_NULL
            Dim test2 = ttChanNulls(_DataStructures.TtChan.INTCALL1) = Constant.DB_NULL
            Dim test3 = ttChanNulls(_DataStructures.TtChan.INTCALL2) = Constant.DB_NULL

            If test1 OrElse test2 OrElse test3 Then
                '...Log2.v("\nTtBuildSH.TtSetPCChan(): THE TRAP IS SPRUNG");
                '...Log2.v("\nTtBuildSH.TtSetPCChan(): ttChan:\n" + ttChan.ToStringWN(ttChanNulls));
            End If

            '...Log2.v("\nTtBuildSH.TtSetPCChan(): Exit");
            Return rc
        End Function ' ---- end ttSetPCChan ----

        ''' <summary>
        ''' Calculates the worst Antenna Discrimination. 
        ''' </summary>
        ''' <remarks>
        ''' The worst Antenna Discrimination is the minimum of the following values:
        ''' <listtype="bullet">
        ''' <item>adiscctxh + adisccrxh;</item>
        ''' <item>adiscctxv + adisccrxv;</item> 
        ''' <item>adiscctxh + adiscxrxv;</item> 
        ''' <item>adiscxtxh + adisccrxv;</item> 
        ''' <item>adiscctxv + adiscxrxh;</item> 
        ''' <item>adiscxtxv + adisccrxh.</item> 
        ''' </list>
        ''' </remarks>
        ''' <paramname="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        ''' <paramname="anteStruct"> - TtAnte object.</param>
        ''' <paramname="totADiscW"> - calculated worst Antenna Discrimination.</param>
        ''' <returns></returns>
        Public Shared Function CalcADiscW(anteNulls As SQLLEN(), anteStruct As TtAnte, <Out> ByRef totADiscW As Double) As Integer
            totADiscW = Constant.DFLT_ADISCW

            If anteNulls(TtAnte.ADISCCTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXH) <> Constant.DB_NULL Then
                totADiscW = anteStruct.adiscctxh + anteStruct.adisccrxh
            End If
            If anteNulls(TtAnte.ADISCCTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXV) <> Constant.DB_NULL Then
                totADiscW = If(totADiscW < anteStruct.adiscctxv + anteStruct.adisccrxv, totADiscW, anteStruct.adiscctxv + anteStruct.adisccrxv)
            End If
            If anteNulls(TtAnte.ADISCCTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCXRXV) <> Constant.DB_NULL Then
                totADiscW = If(totADiscW < anteStruct.adiscctxh + anteStruct.adiscxrxv, totADiscW, anteStruct.adiscctxh + anteStruct.adiscxrxv)
            End If
            If anteNulls(TtAnte.ADISCXTXH) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXV) <> Constant.DB_NULL Then
                totADiscW = If(totADiscW < anteStruct.adiscxtxh + anteStruct.adisccrxv, totADiscW, anteStruct.adiscxtxh + anteStruct.adisccrxv)
            End If
            If anteNulls(TtAnte.ADISCCTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCXRXH) <> Constant.DB_NULL Then
                totADiscW = If(totADiscW < anteStruct.adiscctxv + anteStruct.adiscxrxh, totADiscW, anteStruct.adiscctxv + anteStruct.adiscxrxh)
            End If
            If anteNulls(TtAnte.ADISCXTXV) <> Constant.DB_NULL AndAlso anteNulls(TtAnte.ADISCCRXH) <> Constant.DB_NULL Then
                totADiscW = If(totADiscW < anteStruct.adiscxtxv + anteStruct.adisccrxh, totADiscW, anteStruct.adiscxtxv + anteStruct.adisccrxh)
            End If
            If totADiscW = Constant.DFLT_ADISCW Then
                Return [Error].WRST_ADISC
            End If

            Return Constant.SUCCESS

        End Function ' ----- end calcADiscW ----- 

        ''' <summary>
        ''' This method selects channels to test for interference based 
        ''' on the channel cull. 
        ''' </summary>
        ''' <remarks>
        ''' The channel cull consists of selecting records i.a.w. the following criteria:
        ''' <listtype="bullet">
        ''' <item>if the interferer is the environment, the transmit status code is among 
        ''' the specified codes.</item>
        ''' <item>if the victim is the environment, the receive status 
        ''' code is among the specified codes.</item>
        ''' </list>
        ''' </remarks>
        ''' <paramname="parmStruct"> - TpParm object providing paramater data.</param>
        ''' <paramname="interferer"> - TBD.</param>
        ''' <paramname="intChanStruct"> - FtChan object describing the interferer's channel.</param>
        ''' <paramname="vicChanStruct"> - FtChan object describing the victim's channel.</param>
        ''' <returns></returns>
        Public Shared Function ChanCull(parmStruct As TpParm, interferer As String, intChanStruct As FtChan, vicChanStruct As FtChan) As Integer
            Dim rc As Integer
            Dim getRc As Integer
            Dim codesList As String
            Dim aCode As String

            '...Log2.v("\nchanCull: Entry");

            If parmStruct.numchan = TpRunTsip.TtBuildSH.TEN Then
                Return Constant.SUCCESS
            End If

            rc = Constant.FAILURE
            codesList = parmStruct.chancodes

            getRc = GenUtil.UtGetInputString(codesList, aCode, 1)

            While rc <> Constant.SUCCESS AndAlso getRc >= 0
                '...Log2.v("\nchanCull: A");

                ' if the environ is the interferer use it's transmit status 
                If Strings.FirstCharIs(interferer, "E"c) Then
                    '...Log2.v("\nchanCull: B");
                    If aCode(0) = intChanStruct.stattx(0) Then
                        '...Log2.v("\nchanCull: C");
                        rc = Constant.SUCCESS
                    End If
                Else
                    '...Log2.v("\nchanCull: D");
                    ' if environment is victim use receive (rx) status 
                    If aCode(0) = vicChanStruct.statrx(0) Then
                        '...Log2.v("\nchanCull: E");
                        rc = Constant.SUCCESS
                    End If
                End If
                getRc = GenUtil.UtGetInputString(Nothing, aCode, 1)
            End While
            Return rc
        End Function ' ---- end chanCull 

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class





    End Class
End Namespace
