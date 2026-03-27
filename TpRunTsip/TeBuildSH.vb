Imports System
Imports System.Runtime.InteropServices
Imports System.Collections.Generic
Imports _DataStructures
Imports _Configuration
Imports _NewLib
Imports _Utillib
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16
Imports _Auxlib

Namespace TpRunTsip
    ''' <summary>
    ''' Provides a large number of methods
    ''' used to create and populate the ES SH Tables with data.
    ''' </summary>
    Public Class TeBuildSH

        '----------------------------------------------------------------------

        Private Shared temp1Table As String()

        ' Note: the length of this array can go up to 20,000 !
        Private Shared temp2Table As TtTemp2()

        '-----------------------------------------------------------------------

        ''' <summary>
        ''' This method provides top-level management for the creation of ES SH Tables
        ''' and their population with results data.
        ''' </summary>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <paramname="paramName"> - name of paramater file.</param>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="tpParmNulls"> - ODBC nullInds associated with tpParm.</param>
        ''' <paramname="numEtCases"> - accumulated count of the number of Et cases.</param>
        ''' <paramname="numTeCases"> - accumulated count of the number of Te cases.</param>
        ''' <paramname="startDate"> - TSIP processing start date to be written to tpParm.</param>
        ''' <paramname="startTime"> - TSIP processing start date to be written to tpParm.</param>
        ''' <returns></returns>
        Public Shared Function TeBuildSHTable(tsipName As String, paramName As String, ByRef tpParm As TpParm, ByRef tpParmNulls As SQLLEN(), ByRef numEtCases As Integer, ByRef numTeCases As Integer, startDate As String, startTime As String) As Integer
            '...Log2.v("\nTeBuildSH.TeBuildSHTable(): Entry");

            Dim rc As Integer
            Dim teParmTableName As String

            ' create empty SH TABLES 
            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.TeCreateTsipTables(paramName, tsipName)) <> Constant.SUCCESS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.TeBuildSHTable(): ERROR: call to TeCreateTsipTables() failed, rc = " & rc.ToString())
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*** Could not create TSIP Tables ({0})" & Microsoft.VisualBasic.Constants.vbCrLf, rc)
                Return Constant.FAILURE
            End If

            ' store date and time 
            tpParm.mdate = startDate
            tpParm.mtime = startTime
            tpParm.numcases = -1
            tpParm.numtecases = -1
            tpParmNulls(_DataStructures.TpParm.MDATE) = Constant.DB_NOT_NULL
            tpParmNulls(_DataStructures.TpParm.MTIME) = Constant.DB_NOT_NULL
            tpParmNulls(_DataStructures.TpParm.NUMCASES) = Constant.DB_NOT_NULL
            tpParmNulls(_DataStructures.TpParm.NUMTECASES) = Constant.DB_NOT_NULL

            ' compose internal parameter table name 
            GenUtil.UtCvtName(Constant.TE_PARM, tsipName, teParmTableName)

            ' insert parameter record into SH TABLE 
            If CSharpImpl.__Assign(rc, TpRunTsip.TsipUtils.UtInsertParmRecord(teParmTableName, tpParm, tpParmNulls)) <> Constant.SUCCESS Then
                TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*** Could not insert Parameter Record into SH table ({0}).", rc)
                Return Constant.FAILURE
            End If
            tpParm.numcases = 0
            tpParm.numtecases = 0

            ' perform initial rough cull to extract a set of affected sites,
            '  then fine cull, and create the temporary ts and es site pair tables.
            '  Then from the two site pair tables, create the SH TABLES and
            '  complete with interference calculations 

            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.TeCullNCreate(tpParm, paramName, tsipName, numEtCases, numTeCases)) <> Constant.SUCCESS Then
                If rc <> Constant.FAILURE Then
                    TpRunTsip.TpRunTsip.mTW_ERR.Write(Microsoft.VisualBasic.Constants.vbCrLf & "*** Error culling the table ({0})." & Microsoft.VisualBasic.Constants.vbCrLf, rc)
                    ErrMsg.UtPrintMessage(rc, "", "", "", "", "")
                End If

                ' Drop the temporary DB tables.
                TpRunTsip.TeBuildSH.RemoveTempTables(paramName)
                Return Constant.FAILURE
            End If

            ' Drop the temporary DB tables.
            TpRunTsip.TeBuildSH.RemoveTempTables(paramName)

            '...Log2.v("\nTeBuildSH.TeBuildSHTable(): Exit");
            Return Constant.SUCCESS

        End Function ' ---- end teBuildSHTable ----

        ''' <summary>
        ''' This method creates the empty SH and Temporary 
        ''' tables to be populated by TSIP.  
        ''' </summary>
        ''' <paramname="paramName"> - name of paramater file.</param>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <returns></returns>
        Public Shared Function TeCreateTsipTables(paramName As String, tsipName As String) As Integer
            '...Log2.v("\nTeBuildSH.TeCreateTsipTables(): Entry");

            Dim rc As Integer

            If Ssutil.UtTableExist(Constant.TE, tsipName) Then
                If CSharpImpl.__Assign(rc, Ssutil.UtDropTable(Constant.TE, tsipName)) <> Constant.SUCCESS Then
                    Return -666
                End If
            End If

            If CSharpImpl.__Assign(rc, Ssutil.UtCreateTable(Constant.TE, tsipName)) <> Constant.SUCCESS Then
                Return -667
            End If

            ' create temporary tables 
            ' if the (TS) temporary table exists drop it 
            If Ssutil.UtTableExist(Constant.TT_TEMP1, paramName) Then
                If CSharpImpl.__Assign(rc, Ssutil.UtDropTable(Constant.TT_TEMP1, paramName)) <> Constant.SUCCESS Then
                    Return -668
                End If
            End If
            ' create a one dimensional temporary table 
            If CSharpImpl.__Assign(rc, Ssutil.UtCreateTable(Constant.TT_TEMP1, paramName)) <> Constant.SUCCESS Then
                Return -669
            End If

            ' if the temporary (TS) table exists drop it 
            If Ssutil.UtTableExist(Constant.TT_TEMP2, paramName) Then
                If CSharpImpl.__Assign(rc, Ssutil.UtDropTable(Constant.TT_TEMP2, paramName)) <> Constant.SUCCESS Then
                    Return -670
                End If
            End If
            ' create a two dimensional temporary environment (TS) table 
            If CSharpImpl.__Assign(rc, Ssutil.UtCreateTable(Constant.TT_TEMP2, paramName)) <> Constant.SUCCESS Then
                Return -671
            End If

            ' if the (ES) temporary table exists drop it 
            If Ssutil.UtTableExist(Constant.TE_TEMP1, paramName) Then
                If CSharpImpl.__Assign(rc, Ssutil.UtDropTable(Constant.TE_TEMP1, paramName)) <> Constant.SUCCESS Then
                    Return -672
                End If
            End If
            ' create an (ES) temporary table 
            If CSharpImpl.__Assign(rc, Ssutil.UtCreateTable(Constant.TE_TEMP1, paramName)) <> Constant.SUCCESS Then
                Return -673
            End If

            '...Log2.v("\nTeBuildSH.TeCreateTsipTables(): Exit");

            Return Constant.SUCCESS

        End Function ' ---- end teCreateTsipTables ----

        ''' <summary>
        ''' This method deletes the temporary tables when they are no longer needed.  
        ''' </summary>
        ''' <paramname="paramName"> - name of paramater file.</param>
        Public Shared Sub RemoveTempTables(paramName As String)
            '...Log2.v("\nTeBuildSH.RemoveTempTables(): Entry");

            Ssutil.UtDropTable(Constant.TT_TEMP1, paramName)
            Ssutil.UtDropTable(Constant.TT_TEMP2, paramName)
            Ssutil.UtDropTable(Constant.TE_TEMP1, paramName)

            '...Log2.v("\nTeBuildSH.RemoveTempTables(): Exit");

        End Sub ' ---- end removeTempTables ----

        ''' <summary>
        ''' This method controls the selection of the sites, 
        ''' antennae, and channels from the environment that satisfy the User's
        ''' requirements and then populates the associated fields in the SH Tables.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="paramName"> - name of paramater file.</param>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <paramname="numEtCases"> - accumulated count of the number of Et cases.</param>
        ''' <paramname="numTeCases"> - accumulated count of the number of Te cases.</param>
        ''' <returns></returns>
        Public Shared Function TeCullNCreate(ByRef tpParm As TpParm, paramName As String, tsipName As String, ByRef numEtCases As Integer, ByRef numTeCases As Integer) As Integer
            '...Log2.v("\nTeBuildSH.TeCullNCreate(): Entry");

            Dim ttTemp1Name As String = Nothing
            Dim ttTemp2Name As String = Nothing
            Dim teTemp1Name As String = Nothing
            Dim teSiteTableName As String = Nothing
            Dim teAnteTableName As String = Nothing
            Dim teChanTableName As String = Nothing
            Dim tsSiteTableName As String = Nothing
            Dim tsAnteTableName As String = Nothing
            Dim tsChanTableName As String = Nothing
            Dim esSiteTableName As String = Nothing
            Dim esAnteTableName As String = Nothing
            Dim esChanTableName As String = Nothing
            Dim esAzimTableName As String = Nothing
            Dim codesSelection As String = Nothing
            Dim isMDB As Boolean
            Dim rc As Integer

            TpRunTsip.TeBuildSH.TeBuildShTableNames(tpParm, paramName, tsipName, ttTemp1Name, ttTemp2Name, teTemp1Name, teSiteTableName, teAnteTableName, teChanTableName, tsSiteTableName, tsAnteTableName, tsChanTableName, esSiteTableName, esAnteTableName, esChanTableName, esAzimTableName, isMDB)

            '...Log2.v("\nTeBuildSH.TeCullNCreate(): A");

            ' create selection criteria for culling of oper. codes or call signs 
            TpRunTsip.TsipUtils.UtOpCodesCallSigns(tpParm, codesSelection)

            '...Log2.v("\nTeBuildSH.TeCullNCreate(): B");

            If Equals(tpParm.protype, "E") Then
                '...Log2.v("\nTeBuildSH.TeCullNCreate(): C");

                ' if (proposed file type is "ES") 

                ' perform initial rough cull to extract TS sites which are
                '  roughly within the coordination distance, and create the
                '  earth location table. Also, create the terrestrial call1
                '  table of the sites in the proposed file 
                If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.TeRoughCull(tpParm, esSiteTableName, tsSiteTableName, esAnteTableName, esAzimTableName, ttTemp1Name, teTemp1Name, codesSelection)) <> Constant.SUCCESS Then
                    Return rc
                End If
            Else
                '...Log2.v("\nTeBuildSH.TeCullNCreate(): D");

                ' (proposed file type is "TS") 

                ' perform initial rough cull to extract affected ES sites,
                '  and create the terrestrial call1 table.  Also, create the
                '  earth location table of the sites in the proposed file 
                If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.TtRoughCull(tpParm, isMDB, tsSiteTableName, tsAnteTableName, esSiteTableName, esAnteTableName, ttTemp1Name, teTemp1Name, codesSelection)) <> Constant.SUCCESS Then
                    Return rc
                End If
            End If

            ' create the terrestrial site pair table from the terrestrial call1
            '  table created in teRoughCull() or ttRoughCull() 
            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.Terr2DimTable(tpParm, tsSiteTableName, tsAnteTableName, ttTemp1Name, ttTemp2Name)) <> Constant.SUCCESS Then
                Return rc
            End If

            '...Log2.v("\nTeBuildSH.TeCullNCreate(): E");

            ' from the terrestrial site pair table and earth location table,
            '  create the SH TABLES 
            ' AH: HERE WIP
            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.CreateSHTables(tpParm, isMDB, teSiteTableName, esSiteTableName, tsSiteTableName, teAnteTableName, esAnteTableName, tsAnteTableName, teChanTableName, esChanTableName, esAzimTableName, tsChanTableName, teTemp1Name, ttTemp2Name, numEtCases, numTeCases)) <> Constant.SUCCESS Then
                Return rc
            End If

            '...Log2.v("\nTeBuildSH.TeCullNCreate(): Exit");
            Return Constant.SUCCESS
        End Function ' ---- end teCullNCreate ----

        ''' <summary>
        ''' This method builds the full SQL table names for the 
        ''' SH Tables and temp tables. The names are built from the tsip parameter file 
        ''' name and the runname of the record within that file.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="paramName"> - name of paramater file.</param>
        ''' <paramname="tsipName"> - name of PDF file.</param>
        ''' <paramname="ttTemp1Name"> - temporary unique filename to be used.</param>
        ''' <paramname="ttTemp2Name"> - temporary unique filename to be used.</param>
        ''' <paramname="teTemp1Name"> - temporary unique filename to be used.</param>
        ''' <paramname="teSiteTableName"> - name of Te site table.</param>
        ''' <paramname="teAnteTableName"> - name of Te ante table.</param>
        ''' <paramname="teChanTableName"> - name of Te chan table.</param>
        ''' <paramname="tsSiteTableName"> - name of Ts site table.</param>
        ''' <paramname="tsAnteTableName"> - name of Ts ante table.</param>
        ''' <paramname="tsChanTableName"> - name of Ts chan table.</param>
        ''' <paramname="esSiteTableName"> - name of Es site table.</param>
        ''' <paramname="esAnteTableName"> - name of Es ante table.</param>
        ''' <paramname="esChanTableName"> - name of Es chan table.</param>
        ''' <paramname="esAzimTableName"> - </param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        Public Shared Sub TeBuildShTableNames(tpParm As TpParm, paramName As String, tsipName As String, <Out> ByRef ttTemp1Name As String, <Out> ByRef ttTemp2Name As String, <Out> ByRef teTemp1Name As String, <Out> ByRef teSiteTableName As String, <Out> ByRef teAnteTableName As String, <Out> ByRef teChanTableName As String, <Out> ByRef tsSiteTableName As String, <Out> ByRef tsAnteTableName As String, <Out> ByRef tsChanTableName As String, <Out> ByRef esSiteTableName As String, <Out> ByRef esAnteTableName As String, <Out> ByRef esChanTableName As String, <Out> ByRef esAzimTableName As String, <Out> ByRef isMDB As Boolean)
            '...Log2.v("\nTeBuildSH.TeBuildShTableNames(): Entry");

            ' ENVIRONMENT & PROPOSED TABLE NAMES 
            GenUtil.UtCvtName(Constant.TT_TEMP1, paramName, ttTemp1Name)
            GenUtil.UtCvtName(Constant.TT_TEMP2, paramName, ttTemp2Name)
            GenUtil.UtCvtName(Constant.TE_TEMP1, paramName, teTemp1Name)

            '...Log2.v("\nTeBuildSH.TeBuildShTableNames(): ttTemp1Name = " + ttTemp1Name);
            '...Log2.v("\nTeBuildSH.TeBuildShTableNames(): ttTemp2Name = " + ttTemp2Name);
            '...Log2.v("\nTeBuildSH.TeBuildShTableNames(): teTemp1Name = " + teTemp1Name);

            GenUtil.UtCvtName(Constant.TE_SITE, tsipName, teSiteTableName)
            GenUtil.UtCvtName(Constant.TE_ANTE, tsipName, teAnteTableName)
            GenUtil.UtCvtName(Constant.TE_CHAN, tsipName, teChanTableName)

            tpParm.envtype.Trim()
            isMDB = False
            If tpParm.protype.Equals("E") Then
                If tpParm.envtype.Equals("MDB_TS") Then
                    isMDB = True
                    tsSiteTableName = "main.mt_site"
                    tsAnteTableName = "main.mt_ante"
                    tsChanTableName = "main.mt_chan"
                Else
                    GenUtil.UtCvtName(Constant.FT_SITE, tpParm.envname, tsSiteTableName)
                    GenUtil.UtCvtName(Constant.FT_ANTE, tpParm.envname, tsAnteTableName)
                    GenUtil.UtCvtName(Constant.FT_CHAN, tpParm.envname, tsChanTableName)
                End If
                GenUtil.UtCvtName(Constant.FE_SITE, tpParm.proname, esSiteTableName)
                GenUtil.UtCvtName(Constant.FE_ANTE, tpParm.proname, esAnteTableName)
                GenUtil.UtCvtName(Constant.FE_CHAN, tpParm.proname, esChanTableName)
                GenUtil.UtCvtName(Constant.FE_AZIM, tpParm.proname, esAzimTableName)
            Else
                If tpParm.envtype.Equals("MDB_ES") Then
                    isMDB = True
                    esSiteTableName = "main.me_site"
                    esAnteTableName = "main.me_ante"
                    esChanTableName = "main.me_chan"
                    esAzimTableName = "main.me_azim"
                Else
                    GenUtil.UtCvtName(Constant.FE_SITE, tpParm.envname, esSiteTableName)
                    GenUtil.UtCvtName(Constant.FE_ANTE, tpParm.envname, esAnteTableName)
                    GenUtil.UtCvtName(Constant.FE_CHAN, tpParm.envname, esChanTableName)
                    GenUtil.UtCvtName(Constant.FE_AZIM, tpParm.envname, esAzimTableName)
                End If
                GenUtil.UtCvtName(Constant.FT_SITE, tpParm.proname, tsSiteTableName)
                GenUtil.UtCvtName(Constant.FT_ANTE, tpParm.proname, tsAnteTableName)
                GenUtil.UtCvtName(Constant.FT_CHAN, tpParm.proname, tsChanTableName)
            End If

            '...Log2.v("\nTeBuildSH.TeBuildShTableNames(): Exit");
        End Sub ' ---- end teBuildShTableNames 

        ''' <summary>
        ''' This method controls the culling of the environment 
        ''' when the environment is either the ES MDB or a ES PDF.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="esSiteTableName"> - name of Es site table.</param>
        ''' <paramname="tsSiteTableName"> - name of Ts site table.</param>
        ''' <paramname="esAnteTableName"> - name of Es ante table.</param>
        ''' <paramname="esAzimTableName"> - name of Es azim table</param>
        ''' <paramname="ttTemp1Name"> - temporary unique filename to be used.</param>
        ''' <paramname="teTemp1Name"> - temporary unique filename to be used.</param>
        ''' <paramname="codesSelection"> - the operator code or call sign selection criteria from the specified codes to be used in the SQL query.</param>
        ''' <returns></returns>
        Public Shared Function TeRoughCull(tpParm As TpParm, esSiteTableName As String, tsSiteTableName As String, esAnteTableName As String, esAzimTableName As String, ttTemp1Name As String, teTemp1Name As String, codesSelection As String) As Integer
            '...Log2.v("\nTeBuildSH.TeRoughCull(): Entry");

            Dim sqlCommand As String
            Dim feSiteNulls As SQLLEN()
            Dim feAnteNulls As SQLLEN()
            Dim feSiteHandle As Integer
            Dim feAnteHandle As Integer
            Dim rc As Integer
            Dim rc2 As Integer
            Dim longMinBorder As Integer
            Dim longMaxBorder As Integer
            Dim deltaLong As Integer
            Dim deltaLat As Integer
            Dim maxCoordDistTX As Double
            Dim maxCoordDistRX As Double
            Dim maxCoordDist As Double
            Dim selectionCriteria As String
            Dim feSite As FeSite
            Dim feAnte As FeAnte
            Dim cDistSel As String

            ' Use this list to accumulate call1 strings.
            Dim call1List As List(Of String) = New List(Of SQLCHARPTR)()

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN = 0

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            ' ONE DIMENSIONAL TABLE - ROUGH CULL
            '  Walk through each record in earth table selecting records from the
            '  environment that fall within desired lat/long range. Selected
            '  records are inserted into the TE_TEMP1 temporary table
            ' 

            '	Clean table
            Ssutil.DbDeleteRows(teTemp1Name, Nothing)

            If CSharpImpl.__Assign(feSiteHandle, DynFeSite.FeSelectSite(esSiteTableName, "cmd != 'D'", "")) < 0 Then
                Return feSiteHandle
            End If

            ' Loop over each Earth Station location in the user's fe_XXX_site table.
            While CSharpImpl.__Assign(rc, DynFeSite.FeFetchSite(feSiteHandle, feSite, feSiteNulls)) = Constant.SUCCESS

                ' check each site's azimuth records for loss value 
                rc2 = TpRunTsip.TeBuildSH.CheckPDFAzimuthLoss(esAzimTableName, feSite.location)

                ' add the current location from earth table to the
                '  temporary earth location table 
                sqlCommand = SQLCHARPTR.Format("insert into {0} (location) values ('{1}')", teTemp1Name, feSite.location)
                sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)
                If Not ODBC.IsOK(sqlRet) Then
                    Ssutil.DbGetDiagStmt(hStmt, "teRoughCull02: Could not insert location (" & feSite.location & ") into temp table.")
                    Return -2
                End If


                selectionCriteria = SQLCHARPTR.Format("location = '{0}'", feSite.location)
                If CSharpImpl.__Assign(feAnteHandle, DynFeAnte.FeSelectAnte(esAnteTableName, selectionCriteria, "")) < 0 Then
                    Return feAnteHandle
                End If

                ' Loop over all antennae in the user's fe_XXX_ante table that are associated with
                ' the current Earth Station location.
                While CSharpImpl.__Assign(rc, DynFeAnte.FeFetchAnte(feAnteHandle, feAnte, feAnteNulls)) = Constant.SUCCESS
                    If tpParm.selsites.Equals("CALL SIGN") Then
                        ' 	We do not worry about the lat/long box or coordination distance if
                        ' 		we are selecting by call sign. GJS - 2002.05.20 
                        cDistSel = "(1=1)"
                    Else
                        ' calculate the delta latitude and longitude from max
                        ' of feAnte.txtro and feAnte.txpre ranges

                        maxCoordDistTX = 0.0
                        maxCoordDistRX = 0.0

                        If feAnteNulls(_DataStructures.FeAnte.TXBAND) <> Constant.DB_NULL Then
                            ' use the max of txtro and txpre for the
                            '  coordination distance 
                            maxCoordDistTX = If(feAnte.txtro > feAnte.txpre, feAnte.txtro, feAnte.txpre)
                        End If

                        If feAnteNulls(_DataStructures.FeAnte.RXBAND) <> Constant.DB_NULL Then
                            maxCoordDistRX = If(feAnte.rxtro > feAnte.rxpre, feAnte.rxtro, feAnte.rxpre)
                        End If

                        maxCoordDist = If(maxCoordDistTX > maxCoordDistRX, maxCoordDistTX, maxCoordDistRX)

                        ' using this coordination distance and location of the
                        '  current proposed ES site, calculate the latitudes
                        '  and longitudes for a rough culling box (adjustments
                        '  are also made when the longitude is close to 180 deg
                        '  (E/W)), utDeltaLatLong() and addl calcs 

                        GenUtil.UtDeltaLatLong(feSite.latit, maxCoordDist, deltaLat, deltaLong)
                        longMinBorder = feSite.longit - deltaLong
                        longMaxBorder = feSite.longit + deltaLong

                        ' adjustement when the longitude is close to
                        '  180 degrees (E/W) 
                        If longMinBorder < -Constant.LONG_MAXIMUM Then
                            longMinBorder += 2 * Constant.LONG_MAXIMUM
                        End If
                        If longMaxBorder > Constant.LONG_MAXIMUM Then
                            longMaxBorder -= 2 * Constant.LONG_MAXIMUM
                        End If

                        cDistSel = SQLCHARPTR.Format("(a.latit >= {0} and a.latit <= {1} and a.longit >= {2} and a.longit <= {3})", feSite.latit - deltaLat, feSite.latit + deltaLat, longMinBorder, longMaxBorder)
                    End If

                    ' This is the non-keyhole cull version. 
                    sqlCommand = SQLCHARPTR.Format("insert into {0} (call1) select call1 from {1} a  where {2} {3}", ttTemp1Name, tsSiteTableName, cDistSel, codesSelection)

                    ' if environment is not MDB, exclude deleted records 
                    If Not tpParm.envtype.Equals("MDB_TS") Then
                        sqlCommand += " and a.cmd != 'D'"
                    End If

                    '...Log2.v("\nTeBuildSH.TeRoughCull(): sqlCommand = " + sqlCommand);

                    sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)
                    If Not (ODBC.IsOK(sqlRet) OrElse sqlRet = ODBC.SQL_NO_DATA) Then
                        Ssutil.DbGetDiagStmt(hStmt, "teRoughCull03: Could not insert call signs into temp table." & Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbTab & "SQL is:" & Microsoft.VisualBasic.Constants.vbLf & sqlCommand)
                        Return -3
                    End If

                End While ' End of loop over all antennae in the user's fe_XXX_ante table that are associated with
                ' the current Earth Station location.

                If rc <> Constant.NOMORERECS Then
                    Return rc
                End If

                DynFeAnte.FeCloseAnte(feAnteHandle)

            End While ' End of loop over each Earth Station location in the user's fe_XXX_site table. 

            If rc <> Constant.NOMORERECS Then
                Return rc
            End If

            DynFeSite.FeCloseSite(feSiteHandle)

            ' AH:
            ' Date: 22-Aug-2019.
            ' Bug Fix b190626A : tsip ests duplicate cases.
            ' We need to delete the duplicate rows in table ttTemp1Name that have the same call1.
            Dim SQL = SQLCHARPTR.Format("WITH TableBWithRowID AS ( SELECT ROW_NUMBER() OVER (ORDER BY call1) AS RowID, call1  FROM {0} )  DELETE o FROM TableBWithRowID o WHERE RowID < (SELECT MAX(rowID) FROM TableBWithRowID i WHERE i.call1=o.call1 GROUP BY call1)", ttTemp1Name)
            Ssutil.DbExecute(SQL)

            '...Log2.v("\nTeBuildSH.TeRoughCull(): ttTemp1Name = " + ttTemp1Name);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nTeBuildSH.TeRoughCull(): Exit");

            Return Constant.SUCCESS
        End Function ' ---- end teRoughCull ----

        ''' <summary>
        ''' This method tests if an azimuth loss value is non-zero and 
        ''' writes out a warning message to the .ERR report.
        ''' </summary>
        ''' <paramname="azimname"> - name of the azim table in the DB.</param>
        ''' <paramname="location"> - name of location to be used in SQL select clause.</param>
        ''' <returns></returns>
        Public Shared Function CheckPDFAzimuthLoss(azimname As String, location As String) As Integer
            Dim [select] As String
            Dim feAzim As FeAzim
            Dim azimNulls As SQLLEN()
            Dim handle, rc As Integer

            [select] = SQLCHARPTR.Format("location = '{0}'", location.Trim())

            If CSharpImpl.__Assign(handle, DynFeAzim.FeSelectAzim(azimname, [select], "")) < 0 Then
                Return handle
            End If

            ' for (each record in the earth proposed file) 
            rc = DynFeAzim.FeFetchAzim(handle, feAzim, azimNulls)
            While rc = Constant.SUCCESS
                If azimNulls(_DataStructures.FeAzim.LOSS) <> Constant.DB_NULL AndAlso feAzim.loss <> 0.0 Then
                    TpRunTsip.TpRunTsip.mTW_ERR.Write("WARNING - Azimuth Record: {0} {1} {2,5:F2} has a loss value of {3,5:F2}" & Microsoft.VisualBasic.Constants.vbCrLf, feAzim.location.Trim(), feAzim.call1.Trim(), feAzim.azim, feAzim.loss)
                End If
                rc = DynFeAzim.FeFetchAzim(handle, feAzim, azimNulls)
            End While

            DynFeAzim.FeCloseAzim(handle)

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method controls the culling of the environment when the environment 
        ''' is either the TS MDB or a TS PDF. A one dimensional TS table an a one 
        ''' dimensional ES table are created. To increase real-time performance
        ''' use is made of user-defined functions that run on the SQL Server.
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="tsSiteTableName"> - name of Ts site table.</param>
        ''' <paramname="tsAnteTableName"> - name of Ts ante table.</param>
        ''' <paramname="esSiteTableName"> - name of Es site table.</param>
        ''' <paramname="esAnteTableName"> - name of Es ante table.</param>
        ''' <paramname="ttTemp1Name"> - temporary unique filename.</param>
        ''' <paramname="teTemp1Name"> - temporary unique filename.</param>
        ''' <paramname="codesSelection">- the operator code or call sign selection criteria from the specified codes to be used in the SQL query.</param>
        ''' <returns></returns>
        Public Shared Function TtRoughCull(tpParm As TpParm, isMDB As Boolean, tsSiteTableName As String, tsAnteTableName As String, esSiteTableName As String, esAnteTableName As String, ttTemp1Name As String, teTemp1Name As String, codesSelection As String) As Integer
            '...Log2.v("\nTeBuildSH.TtRoughCull(): Entry");

            Dim sqlCommand As String
            Dim lAvLat As Long
            Dim lAvLng As Long
            Dim nNull As SQLLEN
            Dim fRadius As Single

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN = 0

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            ' ONE DIMENSIONAL TABLE - ROUGH CULL 

            '	Clean out the temporary table.
            Ssutil.DbDeleteRows(ttTemp1Name, Nothing)
            Ssutil.DbDeleteRows(teTemp1Name, Nothing)

            ' **************************************************************************\
            ' 
            ' 		Calculate the centroid of the ts table and the radius.
            ' 
            ' \***************************************************************************
            sqlCommand = SQLCHARPTR.Format("Select AVG(CAST(s.latit AS bigint)), AVG(CAST(s.longit AS bigint))   From {0} s  Where cmd != 'D' ", tsSiteTableName)

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.TtRoughCull(): ERROR: SQLExecDirect() failed for sqlCommand = {0}", sqlCommand)
            End If

            sqlRet = ODBC.SQLFetch(hStmt)

            sqlRet = CShort(Ssutil.DbGetLong(hStmt, 1, "AvLat", lAvLat, nNull))
            sqlRet = CShort(Ssutil.DbGetLong(hStmt, 2, "AvLng", lAvLng, nNull))

            ' To reuse the statement handle we must first close the open cursor.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE)

            '	This gives us the centroid, now find the maximum radius from this centroid in the
            '	ts file.
            sqlCommand = SQLCHARPTR.Format("SELECT Max([tsip].[distance_hs]({0}, {1}, s.latit, s.longit))   FROM {2} s  WHERE cmd != 'D' ", lAvLat, lAvLng, tsSiteTableName)

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Ssutil.DbGetDiagStmt(hStmt, "ttRoughCull10: Could not get radius")
                Return -12
            End If
            sqlRet = ODBC.SQLFetch(hStmt)

            sqlRet = CShort(Ssutil.DbGetFloat(hStmt, 1, "MaxRadius", fRadius, nNull))

            ' To reuse the statement handle we must first close the open cursor.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE)

            '	We now have the centroid and the maximum distance from the centroid of all the
            '	sites in the proposed ts file.  Now we go through the environment and select 
            '	all those sites that are within the distance specified on their antenna records
            '	to the centroid plus the radius plus 2km (because we are using great circle math
            '	here on an ellipsoidal earth).
            sqlCommand = SQLCHARPTR.Format("INSERT INTO {0} SELECT DISTINCT s.location, 0, 0, ''   FROM {1} s JOIN {2} a ON s.location = a.location  WHERE [tsip].[distance_hs]({3}, {4}, s.latit, s.longit) <= [tsip].[maxnum]({5},([tsip].[maxnum]([tsip].[maxnum](a.txtro, a.txpre), [tsip].[maxnum](a.rxtro, a.rxpre)) + {6})) + 2.0 ", teTemp1Name, esSiteTableName, esAnteTableName, lAvLat, lAvLng, tpParm.coordist, fRadius)

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)

            ' To reuse the statement handle we must first close the open cursor.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE)

            '	Now add the ts call signs to the tt_tmp1 table.
            sqlCommand = SQLCHARPTR.Format("INSERT INTO {0} SELECT s.call1, 0, 0, ''   FROM {1} s  WHERE s.cmd != 'D' ", ttTemp1Name, tsSiteTableName)
            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)

            ' Release ODBC resources.
            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nTeBuildSH.TtRoughCull(): Exit");
            Return Constant.SUCCESS
        End Function ' ---- end ttRoughCull ----

        ''' <summary>
        ''' This method populates the temporary table 
        ''' containing all pairs of communicating TS sites withing the culling range 
        ''' or proposed PDF.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="tsSiteTableName"> - name of Ts site table.</param>
        ''' <paramname="tsAnteTableName"> - name of Ts ante table.</param>
        ''' <paramname="ttTemp1Name"> - temporary unique filename.</param>
        ''' <paramname="ttTemp2Name"> - temporary unique filename.</param>
        ''' <returns></returns>
        Public Shared Function Terr2DimTable(tpParm As TpParm, tsSiteTableName As String, tsAnteTableName As String, ttTemp1Name As String, ttTemp2Name As String) As Integer
            '...Log2.v("\nTeBuildSH.Terr2DimTable(): Entry");

            Dim counter = 0
            Dim sqlCommand As String

            Dim tsAnteNulls As SQLLEN()
            Dim ftSiteNulls As SQLLEN()
            Dim tsAnteHandle, ttTemp1Handle, rc As Integer
            Dim oldCallTwo As String
            Dim ftSite As FtSite
            Dim ftAnte As FtAnte
            Dim nCount = 0
            Dim call1 As String
            Dim call1NullInd As SQLLEN

            Dim hConn As SQLHDBC = Ssutil.NewConn()
            Dim hStmt As SQLHANDLE
            Dim sqlRet As SQLRETURN = 0

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            ' TWO DIMENSIONAL VICTIM TABLE
            '  for each site falling into the culling range, identify all it's
            '  communicating sites and insert the site pair into the terrestrial
            '  temporary table 
            '	Clean table
            Ssutil.DbDeleteRows(ttTemp2Name, Nothing)

            If CSharpImpl.__Assign(ttTemp1Handle, TpRunTsip.TtDynTemp1.TtSelectTemp1(ttTemp1Name, "", "")) < 0 Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.Terr2DimTable(): ODBC 'Select' failed, return code = " & ttTemp1Handle.ToString())
                Return ttTemp1Handle
            End If

            ' for (each site in the temporary TS table) 
            While CSharpImpl.__Assign(rc, TpRunTsip.TtDynTemp1.TtFetchTemp1(ttTemp1Handle, call1, call1NullInd)) = Constant.SUCCESS
                oldCallTwo = ""

                ' find all antennas from the TS table whose call1 is the
                '  same as the current call1 from the temp. TS table
                '  (ordered by call2) 

                sqlCommand = SQLCHARPTR.Format("call1 = '{0}'", call1)

                If CSharpImpl.__Assign(tsAnteHandle, TpRunTsip.TpMdbPdfGet.SelectTerrAnte(tsAnteTableName, sqlCommand, tpParm.envtype)) < 0 Then
                    Return tsAnteHandle
                End If

                ' for (each temporary TS site) 
                While CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.FetchTerrAnte(tsAnteHandle, ftAnte, tsAnteNulls, tpParm.envtype)) = Constant.SUCCESS
                    nCount += 1
                    If Not oldCallTwo.Equals(ftAnte.call2) Then
                        ' if (oldCall2 is different from curr call2) 

                        ' insert the (call1, call2) pair into the temporary TS pairs table 
                        sqlCommand = SQLCHARPTR.Format("insert into {0} (call1, call2) values ('{1}', '{2}')", ttTemp2Name, ftAnte.call1, ftAnte.call2)

                        sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length)
                        If Not ODBC.IsOK(sqlRet) Then
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.Terr2DimTable(): ERROR: SQLExecDirect() failed for:" & Microsoft.VisualBasic.Constants.vbLf & sqlCommand)
                            Dim str = SQLCHARPTR.Format("terr2DimTable02: Could not insert link ({0}-{1}) into temp table.", ftAnte.call1, ftAnte.call2)
                            Ssutil.DbGetDiagStmt(hStmt, str)
                            Return [Error].ODBC_EXECDIRECT_FAILED
                        End If

                        ' Now do Orbit calcs if the user requested them.
                        '  Orbit calcs are done once for each pair combination
                        '  of TS sites 
                        If Strings.FirstCharIs(tpParm.protype, "T"c) AndAlso Strings.FirstCharIs(tpParm.tsorbout, "Y"c) Then
                            sqlCommand = SQLCHARPTR.Format("call1 = '{0}' and call2 = '{1}'", ftAnte.call2, ftAnte.call1)

                            counter = Ssutil.DbCountRows(ttTemp2Name, sqlCommand)

                            If counter = 0 OrElse Strings.FirstCharIs(ftAnte.offazm, "Y"c) Then
                                ' get local site Info 
                                If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetCall(ftAnte.call1, tsSiteTableName, False, ftSite, ftSiteNulls)) <> Constant.SUCCESS Then
                                    Return rc
                                End If

                                ' perform Orbit calcs 
                                If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.TtTsorbCalcs(tpParm, ftAnte.offazm, ftAnte.aht, ftAnte.call2, ftAnte.bndcde, ftAnte.tazmth, ftAnte.telvtn, ftSite)) <> Constant.SUCCESS Then
                                    Return rc
                                End If
                            End If
                        End If

                        oldCallTwo = ftAnte.call2
                    ElseIf Strings.FirstCharIs(ftAnte.offazm, "Y"c) Then
                        ' if (true azim/elev values are set in the ES ante rec) 

                        ' orbit calcs are also done for all antennas with
                        '  true azim/elev values 
                        If Strings.FirstCharIs(tpParm.protype, "T"c) AndAlso Strings.FirstCharIs(tpParm.tsorbout, "Y"c) Then
                            ' get local site Info 
                            If CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.TtSiteGetCall(ftAnte.call1, tsSiteTableName, False, ftSite, ftSiteNulls)) <> Constant.SUCCESS Then
                                Return rc
                            End If

                            If CSharpImpl.__Assign(rc, TpRunTsip.TtCalcs.TtTsorbCalcs(tpParm, ftAnte.offazm, ftAnte.aht, ftAnte.call2, ftAnte.bndcde, ftAnte.tazmth, ftAnte.telvtn, ftSite)) <> Constant.SUCCESS Then
                                Return rc
                            End If
                        End If
                    End If
                End While ' end for (each temporary TS site) 

                If rc <> Constant.NOMORERECS Then
                    Return rc
                End If

                TpRunTsip.TpMdbPdfGet.CloseTerrAnte(tsAnteHandle, tpParm.envtype)
            End While

            TpRunTsip.TtDynTemp1.TtCloseTemp1(ttTemp1Handle)

            If rc <> Constant.NOMORERECS Then
                Return rc
            End If

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            sqlRet = CShort(Ssutil.DisConn(hConn))

            '...Log2.v("\nTeBuildSH.Terr2DimTable(): Exit");
            Return Constant.SUCCESS
        End Function ' ---- end terr2DimTable ----

        ''' <summary>
        ''' This method manages the population of the SH 
        ''' Tables with data once the rough cull has been completed.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="teSiteTableName"> - name of Te site table.</param>
        ''' <paramname="esSiteTableName"> - name of Es site table.</param>
        ''' <paramname="tsSiteTableName"> - name of Ts site table.</param>
        ''' <paramname="teAnteTableName"> - name of Te ante table.</param>
        ''' <paramname="esAnteTableName"> - name of Es ante table.</param>
        ''' <paramname="tsAnteTableName"> - name of Ts ante table.</param>
        ''' <paramname="teChanTableName"> - name of Te chan table.</param>
        ''' <paramname="esChanTableName"> - name of Es chan table.</param>
        ''' <paramname="esAzimTableName"> - name of Es azim table.</param>
        ''' <paramname="tsChanTableName"> - name of Ts azim table.</param>
        ''' <paramname="teTemp1Name"> - temporary unique filename</param>
        ''' <paramname="ttTemp2Name"> - temporary unique filename</param>
        ''' <paramname="numEtCases"> - accumulated count of the number of Et cases.</param>
        ''' <paramname="numTeCases"> - accumulated count of the number of Te cases.</param>
        ''' <returns></returns>
        Public Shared Function CreateSHTables(tpParm As TpParm, isMDB As Boolean, teSiteTableName As String, esSiteTableName As String, tsSiteTableName As String, teAnteTableName As String, esAnteTableName As String, tsAnteTableName As String, teChanTableName As String, esChanTableName As String, esAzimTableName As String, tsChanTableName As String, teTemp1Name As String, ttTemp2Name As String, ByRef numEtCases As Integer, ByRef numTeCases As Integer) As Integer
            '...Log2.v("\nTeBuildSH.CreateSHTables(): Entry");

            Dim esSiteNulls As SQLLEN()  '[ME_SITE_SIZE_],
            Dim tsSiteNulls As SQLLEN() = Nothing  '[FtSite.SIZE_],
            Dim teSiteNulls As SQLLEN()  '[TE_SITE_SIZE_],
            Dim teTemp1Nulls As SQLLEN  '[TE_TEMP1_SIZE_];

            Dim teTemp1Handle As Integer
            Dim ttTemp2Count As Integer
            Dim distFail = False
            Dim numAnte As Integer
            Dim i As Integer
            Dim rc As Integer
            Dim oldTtCallOne As String
            Dim [select] = ""
            Dim intPrintMsg As String
            Dim vicPrintMsg As String

            Dim tsSite As FtSite = Nothing
            Dim esSite As FeSite
            Dim teSite As TeSite

            Dim teTemp1 As String

            Dim nTemp1Count = 0
            Dim nTemp1Counter As Integer

            ' read all records from the temporary TS pairs table into memory 
            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.InitTemp2(TpRunTsip.TeBuildSH.temp2Table, ttTemp2Count, ttTemp2Name)) <> Constant.SUCCESS Then
                Return rc
            End If

            '...Log2.v("\nTeBuildSH.CreateSHTables(): ttTemp2Count = " + ttTemp2Count);
            ' AH: REMOVE!
            'ttTemp2Count = 1;

            ' prepare SH SITE, ANTENNA, and CHANNEL tables for insert 

            If CSharpImpl.__Assign(rc, TpRunTsip.TeDynSite.TePrepareSite(teSiteTableName)) <> Constant.SUCCESS Then
                Return rc
            End If

            If CSharpImpl.__Assign(rc, TpRunTsip.TeDynAnte.TePrepareAnte(teAnteTableName)) <> Constant.SUCCESS Then
                Return rc
            End If

            If CSharpImpl.__Assign(rc, TpRunTsip.TeDynChan.TePrepareChan(teChanTableName)) <> Constant.SUCCESS Then
                Return rc
            End If

            ' read the EARTH records 
            ' GJS 07/12/99 In order to commit at the end of each item in the
            '    temp1 table, we cannot keep a cursor open over the whole table,
            '    since a commit closes all cursors.  Keeping this outer loop cursor
            '    open til the end results in an unacceptably large transaction file.
            ' 
            '    What we must do then is read the Temp1 table into memory and use it
            '    as the outer loop.
            ' 
            '    Start by getting its size. 
            nTemp1Count = Ssutil.DbCountRows(teTemp1Name, Nothing)

            '...Log2.v("\nTeBuildSH.CreateSHTables(): teTemp1Name = " + teTemp1Name);
            '...Log2.v("\nTeBuildSH.CreateSHTables(): nTemp1Count = " + nTemp1Count);

            ' Now load up the teTemp1 array with the table 
            If CSharpImpl.__Assign(teTemp1Handle, TpRunTsip.TeDynTemp1.TeSelectTemp1(teTemp1Name, "", "location")) < 0 Then
                Return teTemp1Handle
            End If

            Dim teTemp1List As List(Of String) = New List(Of String)()

            nTemp1Counter = 0
            While TpRunTsip.TeDynTemp1.TeFetchTemp1(teTemp1Handle, teTemp1, teTemp1Nulls) = Constant.SUCCESS
                teTemp1List.Add(teTemp1)
            End While

            TpRunTsip.TeBuildSH.temp1Table = teTemp1List.ToArray()

            nTemp1Counter = TpRunTsip.TeBuildSH.temp1Table.Length

            TpRunTsip.TeDynTemp1.TeCloseTemp1(teTemp1Handle)

            ' Do a sanity check. 
            If nTemp1Counter <> nTemp1Count Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateSHTables(): ERROR: temp1Table inconsistency.")
                TpRunTsip.TpRunTsip.mTW_ERR.Write("/n*ERROR* Temp1 table inconsistent. /n")
                Return Constant.FAILURE
            End If

            ' Now go through the internal array of temp1 elements
            '    for (each record in the earth location table) 
            nTemp1Counter = 0
            While nTemp1Counter < nTemp1Count
                teTemp1 = teTemp1List(nTemp1Counter)

                nTemp1Counter += 1

                intPrintMsg = SQLCHARPTR.Format("INTERFERER:   {0,11}", teTemp1)

                ' retrieve the earth record for the current location from
                '  the PDF or MDB 
                If Strings.FirstCharIs(tpParm.protype, "E"c) Then
                    rc = TpRunTsip.TpMdbPdfGet.TeSiteGetLoc(teTemp1, esSiteTableName, False, esSite, esSiteNulls)
                Else
                    rc = TpRunTsip.TpMdbPdfGet.TeSiteGetLoc(teTemp1, esSiteTableName, isMDB, esSite, esSiteNulls)
                End If


                If rc <> Constant.SUCCESS Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateSHTables(): ERROR: TeSiteGetLoc failed, rc = " & rc.ToString())
                    ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                    ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", [select])
                    Return rc
                End If

                '...Log2.v("\nTeBuildSH.CreateSHTables(): esSite = " + esSite.KeysToString());

                oldTtCallOne = ""
                Dim ttTemp2 As TtTemp2

                ' for (each record in the temporary TS pairs table) 
                For i = 0 To ttTemp2Count - 1
                    ttTemp2 = TpRunTsip.TeBuildSH.temp2Table(i)

                    '...Log2.v("\nTeBuildSH.CreateSHTables(): i = " + i);
                    '...Log2.v("\nTeBuildSH.CreateSHTables(): ttTemp2 = \n" + ttTemp2.ToString());

                    vicPrintMsg = SQLCHARPTR.Format("VICTIM:   {0,10}     {0,10}", ttTemp2.call1, ttTemp2.call2)

                    ''' *	check and set the debugging case switch */
                    'if (glbStopCase != 0)
                    '{
                    '    glbStopCase = teDbgCheckCase("", "", ttTemp2.temp2.call1, ttTemp2.temp2.call2);
                    '}
                    'if (glbStopCase > 0)
                    '{
                    '    //int nRet = 1;
                    '}

                    ' if (oldTtCallOne is different from curr. temp TS call1) 
                    If Not ttTemp2.call1.Equals(oldTtCallOne) Then
                        ' retrieve the terrestrial record for the
                        '  current site from the PDF or MDB 
                        If Strings.FirstCharIs(tpParm.protype, "T"c) Then
                            rc = TpRunTsip.TpMdbPdfGet.TtSiteGetCall(ttTemp2.call1, tsSiteTableName, False, tsSite, tsSiteNulls)
                        Else
                            rc = TpRunTsip.TpMdbPdfGet.TtSiteGetCall(ttTemp2.call1, tsSiteTableName, isMDB, tsSite, tsSiteNulls)
                        End If

                        If rc <> Constant.SUCCESS Then
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateSHTables(): ERROR: TtSiteGetCall failed, rc = " & rc.ToString())
                            ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                            ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                            ErrMsg.UtPrintMessage([Error].FETCH_FAIL, "SITE", [select])
                            Return rc
                        End If
                        oldTtCallOne = ttTemp2.call1
                    ElseIf distFail = True Then
                        ' same call1 as last and distCull failed
                        '  so go on to next temp. TS pairs record 
                        Continue For
                    End If

                    ' cull the environment information based on operator
                    '  codes (if the user specified any) and country 
                    If TpRunTsip.TeBuildSH.SiteCull(tpParm, tsSite, esSite) = Constant.SUCCESS Then
                        distFail = False

                        ' set the values for an ES Site SH record
                        '  from the pro. and env. files, setTeSite() -
                        '  calls teSiteCalcs() to finish the
                        '  population of the ES Site SH record. 

                        rc = TpRunTsip.TeBuildSH.SetTeSite(tpParm, tsSite, tsSiteNulls, esSite, esSiteNulls, teSite, teSiteNulls, ttTemp2.call2, intPrintMsg, vicPrintMsg)

                        If rc <> Constant.SUCCESS AndAlso rc <> Constant.CONT_PROCESSING Then
                            Return rc
                        End If

                        ' CONT_PROCESSING indicates that TS remote site data could
                        '  not be located - simply skip the record 
                        numAnte = 0

                        If rc = Constant.SUCCESS Then
                            ' from SH Site create SH ANTENNA 
                            ' AH: HERE 20180209
                            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.CreateAnteTable(tpParm, teSite, teSiteNulls, esAnteTableName, tsAnteTableName, esChanTableName, tsChanTableName, numAnte, numEtCases, numTeCases)) <> Constant.SUCCESS Then
                                Return rc
                            End If
                        End If

                        If numAnte > 0 Then
                            '...Log2.v("\n\nTeBuildSH.CreateSHTables(): inserting TeSite: " + teSite.ToStringA());

                            ' add record to the ES SH site table 
                            If CSharpImpl.__Assign(rc, TpRunTsip.TeDynSite.TeInsertSite(teSite, teSiteNulls)) <> Constant.SUCCESS Then
                                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateSHTables(): ERROR: TeInsertSite() failed, rc = " & rc.ToString())
                                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                TpRunTsip.TpRunTsip.mTW_ERR.Write("Error inserting te Site: {0}." & Microsoft.VisualBasic.Constants.vbCrLf, rc)
                                Return rc
                            End If
                        End If
                    ElseIf rc = Constant.SITE_FAIL Then
                        distFail = True
                    End If

                Next ' end for (each temp. TS pairs record) 

            End While ' while (nTemp1Counter < nTemp1Count) : for each earth location record.


            TpRunTsip.TeDynChan.TeCloseChan()

            TpRunTsip.TeDynAnte.TeCloseAnte()

            TpRunTsip.TeDynSite.CloseTESite()

            If rc <> 0 AndAlso rc <> Constant.NOMORERECS Then
                Return rc
            End If

            rc = TpRunTsip.TeBuildSH.CheckMDBAzimuthLoss(esAzimTableName, teAnteTableName)

            '...Log2.v("\nTeBuildSH.CreateSHTables(): Exit");

            Return Constant.SUCCESS

        End Function ' ---- end createSHTables ----


        ''' <summary>
        ''' This method reads all records from the temporary TS pairs DB table into application memory.  
        ''' </summary>
        ''' <paramname="currTemp2"> - a TtTemp2 object to be populated with data.</param>
        ''' <paramname="temp2TableCount"> - the number of tables read from the DB.</param>
        ''' <paramname="temp2Name"> - name of temporary TS pairs DB table.</param>
        ''' <returns></returns>
        Private Shared Function InitTemp2(<Out> ByRef currTemp2 As TtTemp2(), <Out> ByRef temp2TableCount As Integer, temp2Name As String) As Integer
            '...Log2.v("\nTeBuildSH.InitTemp2(): Entry");

            ' 'out' requirement
            currTemp2 = Nothing
            temp2TableCount = -666

            Dim temp2Nulls As SQLLEN()
            Dim rc As Integer
            Dim tmpMaxTemp2 As String
            Dim temp2Handle = 0

            ' move all parameter records to the parameter table 
            If CSharpImpl.__Assign(temp2Handle, TpRunTsip.TtDynTemp2.TtSelectTemp2(temp2Name, "", "call1")) < 0 Then
                Return temp2Handle
            End If

            Dim ttTemp2List As List(Of TtTemp2) = New List(Of TtTemp2)()
            Dim ttTemp2 As TtTemp2
            temp2TableCount = 0

            While CSharpImpl.__Assign(rc, TpRunTsip.TtDynTemp2.TtFetchTemp2(temp2Handle, ttTemp2, temp2Nulls)) = Constant.SUCCESS
                ttTemp2List.Add(ttTemp2)

                temp2TableCount += 1

                If temp2TableCount = Constant.MAXTMP2REC Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.InitTemp2(): ERROR: TtTemp2[] exceeds maximum array size.")
                    tmpMaxTemp2 = SQLCHARPTR.Format("{0}", Constant.MAXTMP2REC)
                    ErrMsg.UtPrintMessage([Error].MAXTMP2S, tmpMaxTemp2)
                    Exit While
                End If

            End While

            ' If it crashed out of the while-loop with an error then just return.
            If rc = [Error].DYN_MS_SQL_SERVER_ERR Then
                Return rc
            End If

            ' If we get here all is well; we can set the 'out' parameters.
            currTemp2 = ttTemp2List.ToArray()
            temp2TableCount = currTemp2.Length

            TpRunTsip.TtDynTemp2.TtCloseTemp2(temp2Handle)

            '...Log2.v("\nTeBuildSH.InitTemp2(): Exit");
            Return Constant.SUCCESS

        End Function ' ---- end initTemp2 ----

        ''' <summary>
        ''' This method determines if an ES ante in the SH table list has a 
        ''' non-zero loss value.  
        ''' </summary>
        ''' <paramname="azimTable"> - name of _azim table in DB.</param>
        ''' <paramname="anteTable"> - name of _ante table in DB.</param>
        ''' <returns></returns>
        Public Shared Function CheckMDBAzimuthLoss(azimTable As String, anteTable As String) As Integer
            Dim sel1 As String
            Dim location As String  '[LOCATION_SZ];
            Dim call1 As String  '[CALLSIGN_SZ];
            Dim azimuth As Single
            Dim loss As Single
            Dim locanull As SQLLEN
            Dim call1null As SQLLEN
            Dim azimnull As SQLLEN
            Dim lossnull As SQLLEN

            Dim sqlRet As SQLRETURN
            Dim hStmt As SQLHANDLE

            Dim hConn As SQLHDBC = Ssutil.NewConn()

            If azimTable.StartsWith("me") Then ' environment is MDB 
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

                sel1 = SQLCHARPTR.Format("select distinct {0}.earthlocation, {1}.earthcall1, {2}.azim, {3}.loss from {4}, {5} where {6}.earthlocation = {7}.location and {8}.earthcall1 = {9}.call1 and {10}.loss != 0.0", anteTable, anteTable, azimTable, azimTable, anteTable, azimTable, anteTable, azimTable, anteTable, azimTable, azimTable)

                sqlRet = ODBC.SQLExecDirect(hStmt, sel1, sel1.Length)

                If Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CheckMDBAzimuthLoss(): ERROR: SQLExecDirect failed.")
                    Return [Error].ODBC_EXECDIRECT_FAILED
                End If

                While True
                    sqlRet = ODBC.SQLFetch(hStmt)

                    If Not ODBC.IsOK(sqlRet) Then
                        If sqlRet = ODBC.SQL_NO_DATA Then
                            ' It is EOF, break out of the while-loop.
                            Exit While
                        Else
                            ' Something is wrong.
                            Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CheckMDBAzimuthLoss(): ERROR: SQLFetch failed, return value = " & sqlRet.ToString())
                            Return [Error].ODBC_FETCH_FAILED
                        End If
                    End If

                    Try
                        ' Initialize auto column indexing.
                        Ssutil.DbStartGets()

                        ' Get the DB column values from the fetched table.
                        Ssutil.DbGetString(hStmt, 0, "earthlocation", location, Constant.LOCATION_SZ, locanull)

                        Ssutil.DbGetString(hStmt, 0, "earthcall1", call1, Constant.CALLSIGN_SZ, call1null)

                        Ssutil.DbGetFloat(hStmt, 0, "azim", azimuth, azimnull)

                        Ssutil.DbGetFloat(hStmt, 0, "loss", loss, lossnull)

                        TpRunTsip.TpRunTsip.mTW_ERR.Write("WARNING - Azimuth Record:{0} {1} {0,5:F2} has a loss value of {0,5:F2}" & Microsoft.VisualBasic.Constants.vbCrLf, location.Trim(), call1.Trim(), azimuth, loss)
                    Catch e As Exception
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CheckMDBAzimuthLoss(): ERROR: ODBC 'Get' failed: " & e.Message)

                        Return [Error].ODBC_GET_FAILED
                    End Try

                End While

                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            End If

            Ssutil.DisConn(hConn)

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method culls the sites from the environment based on 
        ''' the User's inputs via the MICS GUI, ie. Country and ALL EXCEPT SELF.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="tsSite"> - FtSite object.</param>
        ''' <paramname="esSite"> - FeSite object.</param>
        ''' <returns></returns>
        Public Shared Function SiteCull(tpParm As TpParm, tsSite As FtSite, esSite As FeSite) As Integer
            '...Log2.v("\nTeBuildSH.SiteCull(): Entry");

            Dim province As String
            Dim country As String  '[4];

            ' cull OPERATOR CODES 

            ' if ((ts. oper is same as es. oper) and (user specified "ALL EXCEPT SELF")) 
            If tsSite.oper.Equals(esSite.oper) AndAlso tpParm.selsites.Equals("ALL EXCEPT SELF") Then
                Log2.w(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.SiteCull(): WARNING: returned Constant.FAILURE")

                Return Constant.FAILURE
            End If

            ' cull for COUNTRY 

            ' if (proposed file type) is "ES") 
            If Strings.FirstCharIs(tpParm.protype, "E"c) Then
                ' ES - TS 
                province = tsSite.prov
            Else
                ' TS - ES 
                province = esSite.prov
            End If

            ' if ((the user specified a country) and (country is not "ALL")) 
            If Not tpParm.country.Equals("") AndAlso Not tpParm.country.Equals("ALL") Then
                ' check that the environment is in the specified country 
                GenUtil.UtGetCountry(province, country)
                If Not country.Equals(tpParm.country) Then
                    Return Constant.FAILURE
                End If
            End If

            '...Log2.v("\nTeBuildSH.SiteCull(): Entry");

            Return Constant.SUCCESS

        End Function ' ---- end siteCull ----

        ''' <summary>
        ''' This method fills in the values of the SH Site record 
        ''' using source data from the prescribed PDF, the environment file (MDB or PDF) as well as results data calculated by TSIP.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="tsSite"> - FtSite object.</param>
        ''' <paramname="tsNulls"> - ODBC nullInds for tsSite.</param>
        ''' <paramname="esSite"> - FeSite object.</param>
        ''' <paramname="esNulls"> - ODBC nullInds for esSite.</param>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="teNulls"> - ODBC nullInds for teSite.</param>
        ''' <paramname="call2"> - call sign for 2nd site.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function SetTeSite(tpParm As TpParm, tsSite As FtSite, tsNulls As SQLLEN(), esSite As FeSite, esNulls As SQLLEN(), <Out> ByRef teSite As TeSite, <Out> ByRef teNulls As SQLLEN(), call2 As String, intPrintMsg As String, vicPrintMsg As String) As Integer
            '...Log2.v("\nTeBuildSH.SetTeSite(): Entry");

            teSite = New TeSite()
            teNulls = NullHelper.CreateArrayOfNullInd(TeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            teSite.terrcall1 = tsSite.call1
            teSite.terrcall2 = call2
            teSite.terrname1 = tsSite.name
            teSite.terroper = tsSite.oper
            teSite.terrlatit = tsSite.latit
            teSite.terrlongit = tsSite.longit
            teSite.terrgrnd = tsSite.grnd
            teSite.earthlocation = esSite.location
            teSite.earthname = esSite.name
            teSite.earthoper = esSite.oper
            teSite.earthlatit = esSite.latit
            teSite.earthlongit = esSite.longit
            teSite.earthgrnd = esSite.grnd
            teSite.radiozone = esSite.radio
            teSite.rainzone = esSite.rain
            teSite.processed = 0  ' false;
            teSite.etreport = 0   ' false;
            teSite.tereport = 0   ' false;
            teSite.etcaseno = 0
            teSite.tecaseno = 0
            teSite.etsubcases = 0
            teSite.tesubcases = 0

            teNulls(_DataStructures.TeSite.TERRCALL1) = tsNulls(FtSite.CALL1)
            teNulls(_DataStructures.TeSite.TERRCALL2) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.TERRNAME1) = tsNulls(FtSite.NAME)
            teNulls(_DataStructures.TeSite.TERROPER) = tsNulls(FtSite.OPER)
            teNulls(_DataStructures.TeSite.TERRLATIT) = tsNulls(FtSite.LATIT)
            teNulls(_DataStructures.TeSite.TERRLONGIT) = tsNulls(FtSite.LONGIT)
            teNulls(_DataStructures.TeSite.TERRGRND) = tsNulls(FtSite.GRND)
            teNulls(_DataStructures.TeSite.EARTHLOCATION) = esNulls(FeSite.LOCATION)
            teNulls(_DataStructures.TeSite.EARTHNAME) = esNulls(FeSite.NAME)
            teNulls(_DataStructures.TeSite.EARTHOPER) = esNulls(FeSite.OPER)
            teNulls(_DataStructures.TeSite.EARTHLATIT) = esNulls(FeSite.LATIT)
            teNulls(_DataStructures.TeSite.EARTHLONGIT) = esNulls(FeSite.LONGIT)
            teNulls(_DataStructures.TeSite.EARTHGRND) = esNulls(FeSite.GRND)
            teNulls(_DataStructures.TeSite.RADIOZONE) = esNulls(FeSite.RADIO)
            teNulls(_DataStructures.TeSite.RAINZONE) = esNulls(FeSite.RAIN)
            teNulls(_DataStructures.TeSite.PROCESSED) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.ETREPORT) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.TEREPORT) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.ETCASENO) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.TECASENO) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.ETSUBCASES) = Constant.DB_NOT_NULL
            teNulls(_DataStructures.TeSite.TESUBCASES) = Constant.DB_NOT_NULL

            Dim rc As Integer = TpRunTsip.TeCalcs.TeSiteCalcs(tpParm, teSite, teNulls, intPrintMsg, vicPrintMsg)

            '...Log2.v("\nTeBuildSH.SetTeSite(): Exit");

            Return rc

        End Function ' ---- end setTeSite ----

        ''' <summary>
        ''' This method controls the culling of antennae and the population of 
        ''' the Antenna SH Table with data.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="teSiteNulls"> - ODBC nullInds for teSite.</param>
        ''' <paramname="esAnteTableName"> - name of Es ante table.</param>
        ''' <paramname="tsAnteTableName"> - name of Ts ante table.</param>
        ''' <paramname="esChanTableName"> - name of Es chan table.</param>
        ''' <paramname="tsChanTableName"> - name of Ts ante table.</param>
        ''' <paramname="numAnte"> - number of ante records read from the DB.</param>
        ''' <paramname="numEtCases"> - accumulated count of the number of Et cases.</param>
        ''' <paramname="numTeCases"> - accumulated count of the number of Te cases.</param>
        ''' <returns></returns>
        Public Shared Function CreateAnteTable(tpParm As TpParm, ByRef teSite As TeSite, ByRef teSiteNulls As SQLLEN(), esAnteTableName As String, tsAnteTableName As String, esChanTableName As String, tsChanTableName As String, ByRef numAnte As Integer, ByRef numEtCases As Integer, ByRef numTeCases As Integer) As Integer
            '...Log2.v("\nTeBuildSH.CreateAnteTable(): Entry");

            Dim mode1 As Boolean
            Dim mode2 As Boolean

            ' AH : HERE
            Dim teAnteNulls = NullHelper.CreateArrayOfNullInd(TeAnte.NUM_COLUMNS, _NewLib.NullHelper.ColumnStatus.NULL)
            Dim esAnteNulls As SQLLEN()  '[FE_ANTE_SIZE_];

            Dim esAnteHandle As Integer
            Dim anteCount As Integer
            Dim chanCount As Integer
            Dim i As Integer
            Dim rc As Integer

            Dim esCriteria As String
            Dim tsCriteria As String
            Dim intPrintMsg As String
            Dim vicPrintMsg As String

            Dim teAnte As TeAnte = New TeAnte()
            Dim esAnte As FeAnte

            'AnteTable tsAnte;
            Dim ftAnteWNarray As FtAnteWN()

            'tsAnte = anteTable;
            '*numAnte = 0;

            tsCriteria = SQLCHARPTR.Format("call1 = '{0}' and call2 = '{1}'", teSite.terrcall1, teSite.terrcall2)

            ' read all antenna records belonging to the current TS (call1, call2)
            '  pair from TS table into memory 
            If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.InitTsAnte(tsCriteria, ftAnteWNarray, anteCount, tsAnteTableName, tpParm.envtype)) <> Constant.SUCCESS Then
                Return rc
            End If

            esCriteria = SQLCHARPTR.Format("location = '{0}'", teSite.earthlocation)

            If CSharpImpl.__Assign(esAnteHandle, TpRunTsip.TpMdbPdfGet.SelectEarthAnte(esAnteTableName, esCriteria, tpParm.envtype)) < 0 Then
                Return esAnteHandle
            End If

            Dim tsAnteWN As FtAnteWN

            ' for (each antenna record belonging to the current ES location) 
            While CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.FetchEarthAnte(esAnteHandle, esAnte, esAnteNulls, tpParm.envtype)) = Constant.SUCCESS
                ' for (each TS antenna record) 
                For i = 0 To anteCount - 1
                    tsAnteWN = ftAnteWNarray(i)

                    ' If (the current es antenna transmits)
                    '  do the first fine Cull for double distance and identify
                    '  the calculation modes. The NULL indicates that teAnte
                    '  information is not yet available.
                    ' 

                    If esAnteNulls(FeAnte.ACODETX) <> Constant.DB_NULL AndAlso TpRunTsip.TeBuildSH.DistanceCull(teSite, esAnte, Nothing, "E", mode1, mode2) = Constant.SUCCESS Then

                        intPrintMsg = SQLCHARPTR.Format("INTERFERER:   {0,11}     {0,10}    {0,5}", esAnte.location, esAnte.call1, esAnte.txband)
                        vicPrintMsg = SQLCHARPTR.Format("VICTIM:       {0,10}     {0,10}    {0,5}    {0,4:D}", tsAnteWN.ante.call1, tsAnteWN.ante.call2, tsAnteWN.ante.bndcde, tsAnteWN.ante.anum)

                        ' cull ante info for adjancent band and call sign. 
                        If TpRunTsip.TeBuildSH.AnteCull(tpParm, esAnte, tsAnteWN.ante, "E") = Constant.SUCCESS Then

                            ' set the values for an ES Ante SH record from the pro. and env. files,
                            '  ES is interferer, setTeAnte() - calls teAnteCalcs() to finish the
                            '  population of the ES Ante SH record

                            rc = TpRunTsip.TeBuildSH.SetTeAnte(tpParm, True, tsAnteWN.ante, tsAnteWN.anteNulls, esAnte, esAnteNulls, teSite, teAnte, teAnteNulls, mode1, mode2, intPrintMsg, vicPrintMsg)

                            'AH: REMOVE
                            Dim filter As Boolean = teAnte.terrcall1.Equals("CHX576")
                            filter = filter And teAnte.terrcall2.Equals("VEL885")
                            If filter Then
                                Dim str = SQLCHARPTR.Format(Microsoft.VisualBasic.Constants.vbLf & "FILTER-A: {0}  {1}  {2}  {3}", teAnte.terrcall1, teAnte.terrcall2, teAnte.tvdistes, teAnteNulls(_DataStructures.TeAnte.TVDISTES))
                                '...Log2.v(str);
                            End If

                            If rc <> Constant.SUCCESS AndAlso rc <> Constant.CONT_PROCESSING Then
                                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateAnteTable(): ERROR: A: call to SetTeAnte() failed: TS remote antenna or site data could not be located.")
                                Return rc
                            End If
                            ' CONT_PROCESSING indicates that TS remote antenna or site data could
                            '  not be located - simply skip the record 

                            chanCount = 0

                            ' 
                            '  Do a second distance cull with the azimuths stored in teAnte--this cull includes only
                            '  areas within the coordination distance and the Keyhole area.
                            ' 	---- OEL 970709 ----- fix the distance cull so that
                            '                         interferer is ES.
                            ' 
                            If rc = Constant.SUCCESS AndAlso TpRunTsip.TeBuildSH.DistanceCull(teSite, esAnte, teAnte, "E", mode1, mode2) = Constant.SUCCESS Then
                                'AH: REMOVE
                                filter = teAnte.terrcall1.Equals("CHX576")
                                filter = filter And teAnte.terrcall2.Equals("VEL885")
                                If filter Then
                                    Dim str = SQLCHARPTR.Format(Microsoft.VisualBasic.Constants.vbLf & "FILTER-B: {0}  {1}  {2}  {3}", teAnte.terrcall1, teAnte.terrcall2, teAnte.tvdistes, teAnteNulls(_DataStructures.TeAnte.TVDISTES))
                                    '...Log2.v(str);
                                End If

                                ' from SH Ante create SH	CHANNEL 
                                ' AH: HERE 20180219
                                If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.CreateChanTable(tpParm, teSite, teSiteNulls, teAnte, teAnteNulls, esChanTableName, tsChanTableName, chanCount, numEtCases, numTeCases)) <> Constant.SUCCESS Then
                                    Return rc
                                End If

                                If chanCount > 0 Then
                                    ' add record to the ES SH ante table where ES is the
                                    ' 	interferer 

                                    'AH: REMOVE
                                    filter = teAnte.terrcall1.Equals("CHX576")
                                    filter = filter And teAnte.terrcall2.Equals("VEL885")
                                    If filter Then
                                        Dim str = SQLCHARPTR.Format(Microsoft.VisualBasic.Constants.vbLf & "FILTER-Z: {0}  {1}  {2}  {3}", teAnte.terrcall1, teAnte.terrcall2, teAnte.tvdistes, teAnteNulls(_DataStructures.TeAnte.TVDISTES))
                                        '...Log2.v(str);
                                    End If

                                    If CSharpImpl.__Assign(rc, TpRunTsip.TeDynAnte.TeInsertAnte(teAnte, teAnteNulls)) <> Constant.SUCCESS Then
                                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                        TpRunTsip.TpRunTsip.mTW_ERR.Write("Attempt to insert Antenna record returned {0}" & Microsoft.VisualBasic.Constants.vbLf, rc)
                                        Return rc
                                    End If
                                    numAnte += 1
                                End If
                            End If   ' end if (distanceCull...	
                        End If       ' 	end if (anteCull...	
                    End If           ' end if ((esAnteNulls...	

                    ' if (the current es antenna receives) 
                    ' do the fine Cull for double distance and identify
                    '  the calculation modes 
                    If esAnteNulls(FeAnte.ACODERX) <> Constant.DB_NULL AndAlso TpRunTsip.TeBuildSH.DistanceCull(teSite, esAnte, Nothing, "T", mode1, mode2) = Constant.SUCCESS Then
                        vicPrintMsg = SQLCHARPTR.Format("VICTIM:       {0,11}     {0,10}    {0,5}", esAnte.location, esAnte.call1, esAnte.txband)
                        intPrintMsg = SQLCHARPTR.Format("INTERFERER:   {0,10}     {0,10}    {0,5}    {0,4:D}", tsAnteWN.ante.call1, tsAnteWN.ante.call2, tsAnteWN.ante.bndcde, tsAnteWN.ante.anum)

                        ' cull ante info 
                        If TpRunTsip.TeBuildSH.AnteCull(tpParm, esAnte, tsAnteWN.ante, "T") = Constant.SUCCESS Then

                            ' 	set the values for an ES Ante SH record from the pro. and env.
                            ' 		files, ES is victim, setTeAnte() - calls teAnteCalcs() to finish
                            ' 		the population of the ES Ante SH record
                            rc = TpRunTsip.TeBuildSH.SetTeAnte(tpParm, False, tsAnteWN.ante, tsAnteWN.anteNulls, esAnte, esAnteNulls, teSite, teAnte, teAnteNulls, mode1, mode2, intPrintMsg, vicPrintMsg)
                            If rc <> Constant.SUCCESS AndAlso rc <> Constant.CONT_PROCESSING Then
                                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateAnteTable(): ERROR: B: call to SetTeAnte() failed: TS remote antenna or site data could not be located.")
                                Return rc
                            End If
                            ' CONT_PROCESSING indicates that TS remote antenna or site data could
                            '  not be located - simply skip the record 

                            chanCount = 0
                            ' 
                            '  	Do a second distance cull with the azimuths stored in teAnte
                            ' 	-- this cull includes only areas within the coordination distance
                            ' 	and the Keyhole area.
                            ' 
                            If rc = Constant.SUCCESS AndAlso TpRunTsip.TeBuildSH.DistanceCull(teSite, esAnte, teAnte, "T", mode1, mode2) = Constant.SUCCESS Then
                                ' from SH Ante create SH	CHANNEL 
                                If CSharpImpl.__Assign(rc, TpRunTsip.TeBuildSH.CreateChanTable(tpParm, teSite, teSiteNulls, teAnte, teAnteNulls, esChanTableName, tsChanTableName, chanCount, numEtCases, numTeCases)) <> Constant.SUCCESS Then
                                    Return rc
                                End If

                                If chanCount > 0 Then
                                    ' add record to the ES SH ante table where ES is the victim 
                                    If CSharpImpl.__Assign(rc, TpRunTsip.TeDynAnte.TeInsertAnte(teAnte, teAnteNulls)) <> Constant.SUCCESS Then
                                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                        TpRunTsip.TpRunTsip.mTW_ERR.Write("Inserting Antenna returned {0}" & Microsoft.VisualBasic.Constants.vbLf, rc)
                                        Return rc
                                    End If
                                    numAnte += 1
                                End If

                            End If       ' end if (distanceCull... 

                        End If           ' end if (anteCull...  	

                    End If               ' end if ((esAnteNulls... 


                Next ' end for (each TS antenna record) 

            End While ' end for (each ES antenna record) 

            If rc <> Constant.NOMORERECS Then
                Return rc
            End If

            TpRunTsip.TpMdbPdfGet.CloseEarthAnte(esAnteHandle, tpParm.envtype)

            '...Log2.v("\nTeBuildSH.CreateAnteTable(): Exit");

            Return Constant.SUCCESS

        End Function ' ---- end createAnteTable ----

        ''' <summary>
        ''' This method populates the fields of the SH Site record with source data
        ''' from the PDF, the environment file (MDB or PDF) as well as results data
        ''' calculated by TSIP.  
        ''' </summary>
        ''' <paramname="select"> - provides the text of an SQL 'where' clause used to selectively read Ts ante records from the DB.</param>
        ''' <paramname="ftAnteWNarray"> - FtAnteWN object combining fields for FtAnte and its associated ODBC nulInds.</param>
        ''' <paramname="anteTableCount"> - the number of FtAnte records read from the DB.</param>
        ''' <paramname="tsTableName"> - name of the DB table containing the FtAnte records.</param>
        ''' <paramname="envtype"></param>
        ''' <returns></returns>
        Public Shared Function InitTsAnte([select] As String, <Out> ByRef ftAnteWNarray As FtAnteWN(), <Out> ByRef anteTableCount As Integer, tsTableName As String, envtype As String) As Integer
            '...Log2.v("\nTeBuildSH.InitTsAnte(): Entry");

            ' 'out' requirement.
            ftAnteWNarray = Nothing
            anteTableCount = -666

            Dim anteHandle As Integer
            Dim rc As Integer
            Dim count = 0

            ' move all parameter records to the parameter table 
            If CSharpImpl.__Assign(anteHandle, TpRunTsip.TpMdbPdfGet.SelectTerrAnte(tsTableName, [select], envtype)) < 0 Then
                Return anteHandle
            End If

            Dim ftAnte As FtAnte
            Dim ftAnteNullInds As SQLLEN()
            Dim ftAnteWNList As List(Of FtAnteWN) = New List(Of FtAnteWN)()

            While CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.FetchTerrAnte(anteHandle, ftAnte, ftAnteNullInds, envtype)) = Constant.SUCCESS
                'currAnte++;
                Dim ftAnteWN As FtAnteWN = New FtAnteWN()
                ftAnteWN.ante = ftAnte
                ftAnteWN.anteNulls = ftAnteNullInds

                ftAnteWNList.Add(ftAnteWN)

                count += 1

                If count >= Constant.MAXANTEREC Then
                    ErrMsg.UtPrintMessage([Error].MAXANTES, Constant.MAXANTEREC.ToString())
                    Exit While
                End If
            End While

            If rc <> Constant.NOMORERECS Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.InitTsAnte():  ERROR: Too many FtAnteWN array objects.")
                Return rc
            End If

            TpRunTsip.TpMdbPdfGet.CloseTerrAnte(anteHandle, envtype)

            ' Finally, create and return the 'out' FtAnteWN[].
            ftAnteWNarray = ftAnteWNList.ToArray()
            anteTableCount = ftAnteWNarray.Length

            '...Log2.v("\nTeBuildSH.InitTsAnte(): Exit");

            Return Constant.SUCCESS

        End Function ' ---- end initTsAnte 

        ''' <summary>
        ''' This method culls the sites from the environment based on the 
        ''' User's specifications ie. coordination distance. 
        ''' </summary>
        ''' <remarks>
        ''' This method is used to do two such culls. The first is for double
        ''' the coordination distance, and is done when teAnte is NULL. The 
        ''' second is a fine cull which removes everything beyond the 
        ''' coordination distance except for what lies within the Keyhole 
        ''' Coodination Zone. This second cull requires values from teAnte, 
        ''' thus teAnte is always required for this sort of cull. 
        ''' </remarks>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="esAnte"> - FeAnte object.</param>
        ''' <paramname="teAnte"> - TeAnte object.</param>
        ''' <paramname="interferer"> - name of interferer.</param>
        ''' <paramname="mode1"> - see the code for a full definition.</param>
        ''' <paramname="mode2"> - see the code for a full definition.</param>
        ''' <returns></returns>
        Public Shared Function DistanceCull(teSite As TeSite, esAnte As FeAnte, teAnte As TeAnte, interferer As String, <Out> ByRef mode1 As Boolean, <Out> ByRef mode2 As Boolean) As Integer
            '...Log2.v("\nTeBuildSH.DistanceCull(): Entry");

            ' 'out' requirement;
            mode1 = False
            mode2 = False

            Dim rc As Integer
            Dim esLatit As Double
            Dim esLongit As Double
            Dim tsLatit As Double
            Dim tsLongit As Double
            Dim distKm As Double
            Dim bearingMid As Double
            Dim bearingDiff As Double

            esLatit = teSite.earthlatit / 100.00
            esLongit = teSite.earthlongit / 100.00
            tsLatit = teSite.terrlatit / 100.00
            tsLongit = teSite.terrlongit / 100.00

            ' calculate the distance between ES and TS stations 
            AxSub2.AxDistan(esLatit, tsLatit, esLongit, tsLongit, distKm, bearingMid, bearingDiff)

            ' MODE 1 --> ES-TS txtro ; TS-ES rxtro 
            rc = Constant.FAILURE

            ' allow double distance cull for first distance cull, indicated
            ' by a null value for teAnte 

            If teAnte Is Nothing Then distKm = .5 * distKm
            ' 
            '  In the first distance cull, all sites more than twice the maximum
            '  distance are disallowed. In the second cull, we have the discrimination
            '  angles, and we are able to remove the sites which are not within
            '  the maximum distance or not within the keyhole area.
            ' 

            ' if (((interferer is "ES") and ((distKm <= es. txtro)
            ' 		or (2nd distance cull and abs(teAnte.ediscang < 5))))
            '  or ((interferer is "TS") and ((distKm <= es. rxtro)
            ' 		or (2nd distance cull and abs(teAnte.tdiscang < 5))))
            ' 
            If Strings.FirstCharIs(interferer, "E"c) AndAlso (distKm <= esAnte.txtro OrElse teAnte IsNot Nothing AndAlso TpRunTsip.TpKeyhole.WithinRange(teAnte.ediscang, 5.0)) OrElse Strings.FirstCharIs(interferer, "T"c) AndAlso (distKm <= esAnte.rxtro OrElse teAnte IsNot Nothing AndAlso TpRunTsip.TpKeyhole.WithinRange(teAnte.tdiscang, 5.0)) Then
                mode1 = True
                rc = Constant.SUCCESS
            Else
                mode1 = False
            End If

            ' MODE 2 --> ES-TS txpre ; TS-ES rxpre 

            ' if ((interferer is "ES" and distKm <= es. txpre)
            ' 		or (2nd distance cull and abs(teAnte.ediscang < 5))))
            '  or (interferer is "TS" and distKm <= es. rxpre))
            ' 		or (2nd distance cull and abs(teAnte.tdiscang < 5))))
            ' 
            If Strings.FirstCharIs(interferer, "E"c) AndAlso (distKm <= esAnte.txpre OrElse teAnte IsNot Nothing AndAlso TpRunTsip.TpKeyhole.WithinRange(teAnte.ediscang, 5.0)) OrElse Strings.FirstCharIs(interferer, "T"c) AndAlso (distKm <= esAnte.rxpre OrElse teAnte IsNot Nothing AndAlso TpRunTsip.TpKeyhole.WithinRange(teAnte.tdiscang, 5.0)) Then
                mode2 = True
                rc = Constant.SUCCESS
            Else
                mode2 = False
            End If

            '...Log2.v("\nTeBuildSH.DistanceCull(): Exit");

            Return rc

        End Function ' ---- end distanceCull ----

        ''' <summary>
        ''' This method culls the antennae from the environment based on 
        ''' adjacent bands.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="esAnte"> - FeAnte object.</param>
        ''' <paramname="tsAnte"> - FtAnte object.</param>
        ''' <paramname="interferer"> - name of inteferer.</param>
        ''' <returns></returns>
        Public Shared Function AnteCull(tpParm As TpParm, esAnte As FeAnte, tsAnte As FtAnte, interferer As String) As Integer
            '...Log2.v("\nTeBuildSH.AnteCull(): Entry");

            Dim rc As Integer

            ' if (the curr. ES and TS bands are not adjacent 
            If Strings.FirstCharIs(interferer, "E"c) Then
                ' ES-TS 
                If CSharpImpl.__Assign(rc, TpRunTsip.TsipUtils.UtAnteAdjBands(esAnte.txband, tsAnte.bndcde)) <> Constant.SUCCESS Then
                    Return rc
                End If
            Else
                ' TS-ES 
                If CSharpImpl.__Assign(rc, TpRunTsip.TsipUtils.UtAnteAdjBands(tsAnte.bndcde, esAnte.rxband)) <> Constant.SUCCESS Then
                    Return rc
                End If
            End If

            ' if (proposed file type is "TS") 
            If Strings.FirstCharIs(tpParm.protype, "T"c) Then
                ' if (user specified "CALL SIGN" culling) 
                If tpParm.selsites.Equals("CALL SIGN") Then
                    ' check if es. call1 is in the list supplied by
                    '  the user to call signs 
                    Return TpRunTsip.TeBuildSH.CallInList(esAnte.call1, tpParm.codes)
                End If
            End If

            '...Log2.v("\nTeBuildSH.AnteCull(): Exit");

            Return Constant.SUCCESS

        End Function ' ---- end anteCull ----

        ''' <summary>
        ''' This method determines whether the given call sign is in the 
        ''' list of call signs specified by the User for culling.  
        ''' </summary>
        ''' <paramname="callSign"> - call sign of site to be tested.</param>
        ''' <paramname="list"> - string containing a list of User' prescribed list of call signs to be culled.</param>
        ''' <returns></returns>
        Public Shared Function CallInList(callSign As String, list As String) As Integer
            Dim rc As Integer
            Dim callSignList As String ' [150]
            Dim aCode As String  ' [9];

            callSignList = list

            rc = GenUtil.UtGetInputString(callSignList, aCode, 9)
            While rc >= 0
                If callSign.Trim().Equals(aCode.Trim()) Then
                    Return Constant.SUCCESS
                End If
                rc = GenUtil.UtGetInputString(Nothing, aCode, 9)
            End While
            Return Constant.FAILURE

        End Function ' ---- end callInList ----

        ''' <summary>
        ''' This method populates the fields of the SH Ante record with source data
        ''' from the PDF, the environment file (MDB or PDF) and results data 
        ''' calculated by TSIP.  
        ''' </summary>
        ''' <paramname="tpParm"> - name of paramater file.</param>
        ''' <paramname="isTX"> - see code for definition.</param>
        ''' <paramname="tsAnte"> - FtAnte object.</param>
        ''' <paramname="tsAnteNulls"> - ODBC nullInds associated with tsAnte.</param>
        ''' <paramname="esAnte"> - FeAnte object.</param>
        ''' <paramname="esAnteNulls"> - ODBC nullInds associated with esAnte.</param>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="teAnte"> - TeAnte object.</param>
        ''' <paramname="teAnteNulls"> - ODBC nullInds associated with teAnte.</param>
        ''' <paramname="mode1"> - see code for definition.</param>
        ''' <paramname="mode2"> - see code for definition.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function SetTeAnte(tpParm As TpParm, isTX As Boolean, tsAnte As FtAnte, tsAnteNulls As SQLLEN(), esAnte As FeAnte, esAnteNulls As SQLLEN(), ByRef teSite As TeSite, ByRef teAnte As TeAnte, ByRef teAnteNulls As SQLLEN(), mode1 As Boolean, mode2 As Boolean, intPrintMsg As String, vicPrintMsg As String) As Integer
            '...Log2.v("\nTeBuildSH.SetTeAnte(): Entry");
#If False
            //...Log2.v("\nTeBuildSH.SetTeAnte(): tsAnte: " + tsAnte.KeysToString());
            //...Log2.v("\nTeBuildSH.SetTeAnte(): esAnte: " + esAnte.KeysToString());
#End If

            ' insert record into TSIP ANTE table 

            NullHelper.FillArray(teAnteNulls, Constant.DB_NULL)

            teAnte.terrcall1 = tsAnte.call1
            teAnte.terrcall2 = tsAnte.call2
            teAnte.terrbndcde = tsAnte.bndcde
            teAnte.terranum = tsAnte.anum
            teAnte.terracode = tsAnte.acode
            teAnte.intause = tsAnte.ause

            teAnte.earthlocation = esAnte.location
            teAnte.earthcall1 = esAnte.call1
            teAnte.satname = esAnte.satname
            teAnte.satoper = esAnte.op2
            teAnte.satlongit = esAnte.satlongit
            teAnte.txpre = esAnte.txpre
            teAnte.txtro = esAnte.txtro
            teAnte.rxpre = esAnte.rxpre
            teAnte.rxtro = esAnte.rxtro
            teAnte.sarc1 = esAnte.sarc1
            teAnte.sarc2 = esAnte.sarc2
            teAnte.tvazim = 0.0 ' need to initialize this value 
            teAnte.processed = 0
            teAnte.mode1 = If(mode1, 1, 0)
            teAnte.mode2 = If(mode2, 1, 0)
            teAnte.etreport = 0
            teAnte.tereport = 0
            teAnte.etsubcaseno = 0
            teAnte.tesubcaseno = 0

            ' 	Copy in the offaxis angle fields from the ts. GJS - 1108 - 2002.12 
            If tsAnteNulls(FtAnte.OFFAZM) = Constant.DB_NULL OrElse Not tsAnte.offazm.Equals("Y") Then
                teAnte.tsoffaxis = "N"
                ' 	Set all the offaxis angle fields to null.  They are not used. 
                teAnte.tstrueaz = tsAnte.azmth
                teAnte.tstrueel = tsAnte.elvtn
                teAnte.angleuta = -1.0
                teAnte.angleeta = -1.0
                teAnte.angleatv = -1.0
                teAnte.adisc_atv = 0.0
                teAnteNulls(_DataStructures.TeAnte.ANGLEUTA) = Constant.DB_NULL
                teAnteNulls(_DataStructures.TeAnte.ANGLEETA) = Constant.DB_NULL
                teAnteNulls(_DataStructures.TeAnte.ANGLEATV) = Constant.DB_NULL
                teAnteNulls(_DataStructures.TeAnte.ADISC_ATV) = Constant.DB_NULL
            Else
                teAnte.tsoffaxis = "Y" ' 'Y' if offaxis 
                teAnte.tstrueaz = tsAnte.tazmth
                teAnte.tstrueel = tsAnte.telvtn
                ' 	Others will be filled in after the geometry has been calculated. 
            End If
            teAnteNulls(_DataStructures.TeAnte.TSTRUEAZ) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.TSTRUEEL) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.TSOFFAXIS) = Constant.DB_NOT_NULL

            teAnteNulls(_DataStructures.TeAnte.TERRCALL1) = tsAnteNulls(FtAnte.CALL1)
            teAnteNulls(_DataStructures.TeAnte.TERRCALL2) = tsAnteNulls(FtAnte.CALL2)
            teAnteNulls(_DataStructures.TeAnte.TERRBNDCDE) = tsAnteNulls(FtAnte.BNDCDE)
            teAnteNulls(_DataStructures.TeAnte.TERRANUM) = tsAnteNulls(FtAnte.ANUM)
            teAnteNulls(_DataStructures.TeAnte.TERRACODE) = tsAnteNulls(FtAnte.ACODE)
            teAnteNulls(_DataStructures.TeAnte.INTAUSE) = tsAnteNulls(FtAnte.AUSE)
            teAnteNulls(_DataStructures.TeAnte.EARTHLOCATION) = esAnteNulls(FeAnte.LOCATION)
            teAnteNulls(_DataStructures.TeAnte.EARTHCALL1) = esAnteNulls(FeAnte.CALL1)
            teAnteNulls(_DataStructures.TeAnte.SATNAME) = esAnteNulls(FeAnte.SATNAME)
            teAnteNulls(_DataStructures.TeAnte.SATOPER) = esAnteNulls(FeAnte.OP2)
            teAnteNulls(_DataStructures.TeAnte.SATLONGIT) = esAnteNulls(FeAnte.SATLONGIT)
            teAnteNulls(_DataStructures.TeAnte.TXPRE) = esAnteNulls(FeAnte.TXPRE)
            teAnteNulls(_DataStructures.TeAnte.TXTRO) = esAnteNulls(FeAnte.TXTRO)
            teAnteNulls(_DataStructures.TeAnte.RXPRE) = esAnteNulls(FeAnte.RXPRE)
            teAnteNulls(_DataStructures.TeAnte.RXTRO) = esAnteNulls(FeAnte.RXTRO)
            teAnteNulls(_DataStructures.TeAnte.SARC1) = esAnteNulls(FeAnte.SARC1)
            teAnteNulls(_DataStructures.TeAnte.SARC2) = esAnteNulls(FeAnte.SARC2)
            teAnteNulls(_DataStructures.TeAnte.PROCESSED) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.MODE1) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.MODE2) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.INTERFERER) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.ETREPORT) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.TEREPORT) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.ETSUBCASENO) = Constant.DB_NOT_NULL
            teAnteNulls(_DataStructures.TeAnte.TESUBCASENO) = Constant.DB_NOT_NULL

            If isTX Then
                teAnte.interferer = "E"
                teAnte.earthband = esAnte.txband
                teAnte.earthacode = esAnte.acodetx
                teAnteNulls(_DataStructures.TeAnte.EARTHBAND) = esAnteNulls(FeAnte.TXBAND)
                teAnteNulls(_DataStructures.TeAnte.EARTHACODE) = esAnteNulls(FeAnte.ACODETX)
            Else
                teAnte.interferer = "T"
                teAnte.earthband = esAnte.rxband
                teAnte.earthacode = esAnte.acoderx
                teAnteNulls(_DataStructures.TeAnte.EARTHBAND) = esAnteNulls(FeAnte.RXBAND)
                teAnteNulls(_DataStructures.TeAnte.EARTHACODE) = esAnteNulls(FeAnte.ACODERX)
            End If

            Dim rc As Integer = TpRunTsip.TeCalcs.TeAnteCalcs(tpParm, teAnte, teSite, teAnteNulls, intPrintMsg, vicPrintMsg)

            '...Log2.v("\nTeBuildSH.SetTeAnte(): Exit");

            Return rc

        End Function ' ---- end setTeAnte ----

        ''' <summary>
        ''' This method controls the channel culling and population of 
        ''' the Channel SH Table with data.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="teSiteNulls"> - ODBC nullInds for teSite.</param>
        ''' <paramname="teAnte"> - TeAnte object.</param>
        ''' <paramname="teAnteNulls"> - ODBC nullInds for teAnte.</param>
        ''' <paramname="esChanTableName"> - name of Es chan table.</param>
        ''' <paramname="tsChanTableName"> - name of Ts chan table.</param>
        ''' <paramname="chanCount"> - channel count.</param>
        ''' <paramname="numEtCases"> - accumulated count of the number of Et cases.</param>
        ''' <paramname="numTeCases"> - accumulated count of the number of Te cases.</param>
        ''' <returns></returns>
        Public Shared Function CreateChanTable(tpParm As TpParm, ByRef teSite As TeSite, ByRef teSiteNulls As SQLLEN(), ByRef teAnte As TeAnte, ByRef teAnteNulls As SQLLEN(), esChanTableName As String, tsChanTableName As String, ByRef chanCount As Integer, ByRef numEtCases As Integer, ByRef numTeCases As Integer) As Integer  ' Check all these 'ref'
            '...Log2.v("\nTeBuildSH.CreateChanTable(): Entry");

            Dim esChanNulls As SQLLEN()  '[FE_CHAN_SIZE_],
            Dim tsChanNulls As SQLLEN()  '[FtChan.SIZE_];
            Dim esChanHandle, tsChanHandle, rc, rc1 As Integer
            Dim intPrintMsg = ""
            Dim vicPrintMsg = ""
            Dim teChan As TeChan
            Dim esChan As FeChan
            Dim tsChan As FtChan

            teChan = New TeChan()
            Dim teChanNulls = NullHelper.CreateArrayOfNullInd(TeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            Dim esCriteria As String
            Dim tsCriteria As String
            Dim tmppwr = 0.0F
            Dim pwrnull As SQLLEN = Constant.DB_NULL

            Dim sqlRet As SQLRETURN
            Dim hStmt As SQLHANDLE
            Dim hConn As SQLHDBC = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)

            chanCount = 0

            'setSqlNulls(teChanNulls, TE_CHAN_SIZE_, Constant.DB_NULL);
            'setSqlNulls(esChanNulls, FE_CHAN_SIZE_, Constant.DB_NULL);
            'setSqlNulls(tsChanNulls, FtChan.SIZE_, Constant.DB_NULL);

            teChanNulls = NullHelper.CreateArrayOfNullInd(TeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)
            esChanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)
            tsChanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

            ' if (analysis option is "BAND") 
            If tpParm.analopt.Equals("BAND") Then

                ' BAND - add a single CHANNEL record FOR ES INTERFERER 
                If teAnte.interferer.Equals("E") Then
                    esCriteria = SQLCHARPTR.Format("select max(pwrtx) from {0} where location = '{1}' and call1 = '{2}'", esChanTableName, teAnte.earthlocation, teAnte.earthcall1)

                    sqlRet = ODBC.SQLExecDirect(hStmt, esCriteria, esCriteria.Length)
                    If ODBC.IsOK(sqlRet) Then
                        Try
                            Ssutil.DbGetFloat(hStmt, 1, "max(pwrtx)", tmppwr, pwrnull)

                            If pwrnull = Constant.DB_NULL Then
                                ' Ensure that tmppwr is set to zero regardless of 
                                ' value of Ssutil.dbGetMode (Enums.DbGetForNullMode).
                                tmppwr = 0.0F
                            End If
                        Catch e As Exception
                            Log2.w(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateChanTable(): WARNING: A: DbGetFloat() threw an exception: " & e.Message)
                            ' If there are no pwrtxs at all, then we will still return
                            ' ODBC.IsOK, but the result will be null, and DbGetFloat will
                            ' throw an exception.
                            ' Note: determine if this is actually ever needed?
                            tmppwr = 0.0F
                            pwrnull = Constant.DB_NULL
                        End Try
                    Else
                        tmppwr = 0.0F
                        pwrnull = Constant.DB_NULL
                    End If

                    teChan.inttxpwr = tmppwr
                    teChanNulls(_DataStructures.TeChan.INTTXPWR) = pwrnull

                    ' set the values for an ES Chan SH record from the
                    '  pro. and env. files, ES is interferer, setBandChan()
                    '  calls teChanCalcs() to finish the population of the
                    '  ES Chan SH record 
                    rc = TpRunTsip.TeBuildSH.SetBandChan("E", tpParm, teSite, teAnte, teAnteNulls, teChan, teChanNulls, chanCount, intPrintMsg, vicPrintMsg)

                    If rc <> Constant.SUCCESS AndAlso rc <> Constant.PC_SKIP AndAlso rc <> Constant.CONT_PROCESSING Then
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                        Ssutil.DisConn(hConn)
                        Return rc
                    End If

                    TpRunTsip.TeBuildSH.TeDump(teChan, teAnte, teAnteNulls, teSite, teSiteNulls, numEtCases, numTeCases)

                    ' add record to the ES SH Chan table where ES is the
                    '  interferer 
                    If CSharpImpl.__Assign(rc, TpRunTsip.TeDynChan.TeInsertChan(teChan, teChanNulls)) <> Constant.SUCCESS Then
                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write("Inserting Channel returned: {0}." & Microsoft.VisualBasic.Constants.vbLf, rc)
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                        Ssutil.DisConn(hConn)
                        Return rc
                    End If
                End If

                ' FOR TS AS THE INTERFERER 

                ' set the values for an ES Chan SH record from the pro. and
                '  env. files, ES is victim, setBandChan() - calls
                '  teChanCalcs() to finish the population of the ES Chan
                '  SH record 

                If teAnte.interferer.Equals("T") Then
                    tsCriteria = SQLCHARPTR.Format("select max(pwrtx) from {0} where call1 = '{1}' and call2 = '{2}' and bndcde = '{3}' and (antnumbtx1 = {4} or antnumbtx2 = {5})", tsChanTableName, teAnte.terrcall1, teAnte.terrcall2, teAnte.terrbndcde, teAnte.terranum, teAnte.terranum)

                    sqlRet = ODBC.SQLExecDirect(hStmt, tsCriteria, tsCriteria.Length)
                    If ODBC.IsOK(sqlRet) Then
                        Try
                            Ssutil.DbGetFloat(hStmt, 1, "max(pwrtx)", tmppwr, pwrnull)
                        Catch e As Exception
                            Log2.w(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateChanTable(): WARNING: B: DbGetFloat() threw an exception: " & e.Message)
                            ' If there are no pwrtxs at all, then we will still return
                            ' ODBC.IsOK, but the result will be null, and DbGetFloat will
                            ' throw an exception.
                            ' Note: determine if this is actually ever needed?
                            tmppwr = 0.0F
                            pwrnull = Constant.DB_NULL
                        End Try
                    Else
                        tmppwr = 0.0F
                        pwrnull = Constant.DB_NULL
                    End If

                    teChan.inttxpwr = tmppwr
                    teChanNulls(_DataStructures.TeChan.INTTXPWR) = pwrnull

                    rc = TpRunTsip.TeBuildSH.SetBandChan("T", tpParm, teSite, teAnte, teAnteNulls, teChan, teChanNulls, chanCount, intPrintMsg, vicPrintMsg)

                    If rc <> Constant.SUCCESS AndAlso rc <> Constant.PC_SKIP AndAlso rc <> Constant.CONT_PROCESSING Then
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                        Ssutil.DisConn(hConn)
                        Return rc
                    End If

                    TpRunTsip.TeBuildSH.TeDump(teChan, teAnte, teAnteNulls, teSite, teSiteNulls, numEtCases, numTeCases)

                    ' add record to the ES SH Chan table where ES is the
                    '  victim 
                    If CSharpImpl.__Assign(rc, TpRunTsip.TeDynChan.TeInsertChan(teChan, teChanNulls)) <> Constant.SUCCESS Then
                        ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                        ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                        TpRunTsip.TpRunTsip.mTW_ERR.Write("Inserting channel returned {0}" & Microsoft.VisualBasic.Constants.vbLf, rc)
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                        Ssutil.DisConn(hConn)
                        Return rc
                    End If
                End If
            Else
                ' P/C-add a record for each chan permutation 
                esCriteria = SQLCHARPTR.Format("location = '{0}' and call1 = '{1}'", teAnte.earthlocation, teAnte.earthcall1)

                '&&Console.Error.Write("\nteBuildSH.createChanTable(): rhino");
                ' read earth channel info 
                If CSharpImpl.__Assign(esChanHandle, TpRunTsip.TpMdbPdfGet.SelectEarthChan(esChanTableName, esCriteria, tpParm.envtype)) < 0 Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TeBuildSH.CreateChanTable(): ERROR: SelectEarthChan() failed")
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                    Ssutil.DisConn(hConn)
                    Return esChanHandle
                End If

                ' for (each channel from the ES table which belongs to
                '  the curr. ES Antenna) 
                '&&Console.Error.Write("\nteBuildSH.createChanTable(): whale");
                While CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.FetchEarthChan(esChanHandle, esChan, esChanNulls, tpParm.envtype)) = Constant.SUCCESS
                    If Strings.FirstCharIs(teAnte.interferer, "E"c) Then
                        ' 	Earth site is the interferer, check if this channel is tx, i.e. has
                        ' 		a non-zero freqtx.  -- GJS - 1267 - 2008.05 
                        If esChan.freqtx = 0.0 OrElse esChanNulls(FeChan.FREQTX) = Constant.DB_NULL Then
                            Continue While
                        End If

                        ' 	Prepare the error message strings 
                        intPrintMsg = SQLCHARPTR.Format("INTERFERER:   {0,11}     {1,10}    {2,5}", esChan.location, esChan.call1, esChan.chid)
                        ' ES-TS 
                        tsCriteria = SQLCHARPTR.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbrx1 = {3} or antnumbrx2 = {4} or antnumbrx3 = {5})", teAnte.terrcall1, teAnte.terrcall2, teAnte.terrbndcde, teAnte.terranum, teAnte.terranum, teAnte.terranum)
                    Else
                        ' 	Earth site is the victim, check if this channel is rx, i.e. has
                        ' 		a non-zero freqrx.  -- GJS - 1267 - 2008.05 
                        If esChan.freqrx = 0.0 OrElse esChanNulls(FeChan.FREQRX) = Constant.DB_NULL Then
                            Continue While
                        End If

                        vicPrintMsg = SQLCHARPTR.Format("VICTIM:       {0,11}     {1,10}    {2,5}", esChan.location, esChan.call1, esChan.chid)
                        ' TS-ES 
                        tsCriteria = SQLCHARPTR.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbtx1 = {3} or antnumbtx2 = {4})", teAnte.terrcall1, teAnte.terrcall2, teAnte.terrbndcde, teAnte.terranum, teAnte.terranum)
                    End If

                    ' read terrestrial channel info 
                    If CSharpImpl.__Assign(tsChanHandle, TpRunTsip.TpMdbPdfGet.SelectTerrChan(tsChanTableName, tsCriteria, tpParm.envtype)) < 0 Then
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                        Ssutil.DisConn(hConn)
                        Return tsChanHandle
                    End If

                    ' for (each channel from the TS table which belongs to
                    '  the curr. TS Antenna) 
                    While CSharpImpl.__Assign(rc, TpRunTsip.TpMdbPdfGet.FetchTerrChan(tsChanHandle, tsChan, tsChanNulls, tpParm.envtype)) = Constant.SUCCESS
                        If Strings.FirstCharIs(teAnte.interferer, "E"c) Then
                            vicPrintMsg = SQLCHARPTR.Format("VICTIM:       {0,10}     {1,10}    {2,5}    {3}", tsChan.call1, tsChan.call2, tsChan.bndcde, tsChan.chid)
                        Else
                            intPrintMsg = SQLCHARPTR.Format("INTERFERER:   {0,10}     {1,10}    {2,5}    {3}", tsChan.call1, tsChan.call2, tsChan.bndcde, tsChan.chid)
                        End If

                        ' cull chan info 
                        If TpRunTsip.TeBuildSH.CullChan(tpParm, teAnte.interferer, esChan, tsChan) = Constant.SUCCESS Then

                            chanCount += 1

                            ' 	set the values for an ES Chan SH record from the pro. and env.
                            ' 		files, teSetPCChan() - calls teChanCalcs() to finish the population
                            ' 		of the ES Chan SH record 
                            rc1 = TpRunTsip.TeBuildSH.TeSetPCChan(tpParm, teSite, teAnte, teAnteNulls, teChan, teChanNulls, tsChan, tsChanNulls, esChan, esChanNulls, intPrintMsg, vicPrintMsg)

                            If rc1 <> Constant.SUCCESS AndAlso rc1 <> Constant.PC_SKIP AndAlso rc1 <> Constant.CONT_PROCESSING Then
                                GenUtil.SetErr("Error %d on " & Microsoft.VisualBasic.Constants.vbLf & "%s into" & Microsoft.VisualBasic.Constants.vbLf & "%s" & Microsoft.VisualBasic.Constants.vbLf, rc1.ToString(), intPrintMsg, vicPrintMsg)
                                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                                Ssutil.DisConn(hConn)
                                Return rc1
                            End If

                            TpRunTsip.TeBuildSH.TeDump(teChan, teAnte, teAnteNulls, teSite, teSiteNulls, numEtCases, numTeCases)

                            ' add record to the ES SH Chan table 
                            If CSharpImpl.__Assign(rc, TpRunTsip.TeDynChan.TeInsertChan(teChan, teChanNulls)) <> Constant.SUCCESS Then
                                ErrMsg.UtPrintMessage([Error].GENERROR, intPrintMsg)
                                ErrMsg.UtPrintMessage([Error].GENERROR, vicPrintMsg)
                                TpRunTsip.TpRunTsip.mTW_ERR.Write("Inserting channel returned {0}" & Microsoft.VisualBasic.Constants.vbLf, rc)
                                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                                Ssutil.DisConn(hConn)
                                Return rc
                            End If
                        End If
                    End While ' end for (each TS channel) 

                    If rc <> Constant.NOMORERECS Then
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                        Ssutil.DisConn(hConn)
                        Return rc
                    End If

                    TpRunTsip.TpMdbPdfGet.CloseTerrChan(tsChanHandle, tpParm.envtype)

                End While ' end for (each ES channel) 

                If rc <> Constant.NOMORERECS Then
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
                    Ssutil.DisConn(hConn)
                    Return rc
                End If

                TpRunTsip.TpMdbPdfGet.CloseEarthChan(esChanHandle, tpParm.envtype)
            End If

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            Ssutil.DisConn(hConn)

            '...Log2.v("\nTeBuildSH.CreateChanTable(): Exit");

            Return Constant.SUCCESS

        End Function ' ---- end createChanTable ----

        ''' <summary>
        ''' This method sets the values for an ES Chan SH record from the PDF and 
        ''' environment files; ES is the interferer. This method calls TeChanCalcs()
        ''' to complete the population of the ES Chan SH record with data.  
        ''' </summary>
        ''' <paramname="interf"> - name of the interferer.</param>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="teAnte"> - TeAnte object.</param>
        ''' <paramname="teAnteNulls"> - ODBC nullInds for teAnte.</param>
        ''' <paramname="teChan"> - TeChan object.</param>
        ''' <paramname="teChanNulls"> - ODBC nullInds for teChan.</param>
        ''' <paramname="chanCount"> - channel count.</param>
        ''' <paramname="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <paramname="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function SetBandChan(interf As String, tpParm As TpParm, ByRef teSite As TeSite, ByRef teAnte As TeAnte, ByRef teAnteNulls As SQLLEN(), ByRef teChan As TeChan, ByRef teChanNulls As SQLLEN(), <Out> ByRef chanCount As Integer, <Out> ByRef intPrintMsg As String, <Out> ByRef vicPrintMsg As String) As Integer  ' Check ref?
            ' Check ref?
            '...Log2.v("\nTeBuildSH.SetBandChan(): Entry");

            chanCount = 1

            teChan.interferer = interf
            teChan.terrcall1 = teAnte.terrcall1
            teChan.terrcall2 = teAnte.terrcall2
            teChan.terrbndcde = teAnte.terrbndcde
            teChan.terranum = teAnte.terranum
            teChan.earthlocation = teAnte.earthlocation
            teChan.earthcall1 = teAnte.earthcall1
            teChan.processed = 1

            teChanNulls(_DataStructures.TeChan.INTERFERER) = Constant.DB_NOT_NULL
            teChanNulls(_DataStructures.TeChan.TERRCALL1) = teAnteNulls(_DataStructures.TeAnte.TERRCALL1)
            teChanNulls(_DataStructures.TeChan.TERRCALL2) = teAnteNulls(_DataStructures.TeAnte.TERRCALL2)
            teChanNulls(_DataStructures.TeChan.TERRBNDCDE) = teAnteNulls(_DataStructures.TeAnte.TERRBNDCDE)
            teChanNulls(_DataStructures.TeChan.TERRANUM) = teAnteNulls(_DataStructures.TeAnte.TERRANUM)
            teChanNulls(_DataStructures.TeChan.EARTHLOCATION) = teAnteNulls(_DataStructures.TeAnte.EARTHLOCATION)
            teChanNulls(_DataStructures.TeChan.EARTHCALL1) = teAnteNulls(_DataStructures.TeAnte.EARTHCALL1)
            teChanNulls(_DataStructures.TeChan.PROCESSED) = Constant.DB_NOT_NULL

            If Strings.FirstCharIs(interf, "E"c) Then
                intPrintMsg = SQLCHARPTR.Format("INTERFERER:   {0,11}     {1,10}", teAnte.earthlocation, teAnte.earthcall1)
                vicPrintMsg = SQLCHARPTR.Format("VICTIM:       {0,10}     {1,10}    {2,5}    {3,4:D}", teAnte.terrcall1, teAnte.terrcall2, teAnte.terrbndcde, teAnte.terranum)
            Else
                vicPrintMsg = SQLCHARPTR.Format("VICTIM:       {0,11}     {1,10}", teAnte.earthlocation, teAnte.earthcall1)
                intPrintMsg = SQLCHARPTR.Format("INTEFERER:    {0,10}     {1,10}    {2,5}    {3,4:D}", teAnte.terrcall1, teAnte.terrcall2, teAnte.terrbndcde, teAnte.terranum)
            End If

            Dim rv As Integer = TpRunTsip.TeCalcs.TeChanCalcs(teChan, teAnte, teSite, tpParm, teChanNulls, intPrintMsg, vicPrintMsg)

            '...Log2.v("\nTeBuildSH.SetBandChan(): Exit");

            Return rv

        End Function ' ---- end setBandChan ----

        ''' <summary>
        ''' This method sets various report flags and case/subcase numbers in 
        ''' the site, ante and chan records.  
        ''' </summary>
        ''' <paramname="teChan"> - TeChan object.</param>
        ''' <paramname="teAnte"> - TeAnte object.</param>
        ''' <paramname="anteNulls"> - ODBC nullInds for teAnte</param>
        ''' <paramname="teSite"> - TeSite object.</param>
        ''' <paramname="siteNulls"> - ODBC nullInds for teSite</param>
        ''' <paramname="numETSitesRptd"> - number of Et sites to be reported.</param>
        ''' <paramname="numTESitesRptd"> - number of Te sites to be reported.</param>
        Public Shared Sub TeDump(teChan As TeChan, ByRef teAnte As TeAnte, ByRef anteNulls As SQLLEN(), ByRef teSite As TeSite, ByRef siteNulls As SQLLEN(), ByRef numETSitesRptd As Integer, ByRef numTESitesRptd As Integer)
            '...Log2.v("\nTeBuildSH.TeDump(): Entry");

            If teChan.etreport = Constant.TRUE Then
                If teSite.etreport <> Constant.TRUE Then
                    teSite.etreport = Constant.TRUE
                    teSite.etcaseno = Threading.Interlocked.Increment(numETSitesRptd)
                    teSite.etsubcases = 1
                    siteNulls(_DataStructures.TeSite.ETREPORT) = Constant.DB_NOT_NULL
                    siteNulls(_DataStructures.TeSite.ETCASENO) = Constant.DB_NOT_NULL
                    siteNulls(_DataStructures.TeSite.ETSUBCASES) = Constant.DB_NOT_NULL
                ElseIf teAnte.etreport <> Constant.TRUE Then
                    teSite.etsubcases += 1
                End If
                If teAnte.etreport <> Constant.TRUE Then
                    teAnte.etreport = Constant.TRUE
                    teAnte.etsubcaseno = teSite.etsubcases
                    anteNulls(_DataStructures.TeAnte.ETREPORT) = Constant.DB_NOT_NULL
                    anteNulls(_DataStructures.TeAnte.ETSUBCASENO) = Constant.DB_NOT_NULL
                End If
            End If
            If teChan.tereport = Constant.TRUE Then
                If teSite.tereport <> Constant.TRUE Then
                    teSite.tereport = Constant.TRUE
                    teSite.tecaseno = Threading.Interlocked.Increment(numTESitesRptd)
                    teSite.tesubcases = 1
                    siteNulls(_DataStructures.TeSite.TEREPORT) = Constant.DB_NOT_NULL
                    siteNulls(_DataStructures.TeSite.TECASENO) = Constant.DB_NOT_NULL
                    siteNulls(_DataStructures.TeSite.TESUBCASES) = Constant.DB_NOT_NULL
                ElseIf teAnte.tereport <> Constant.TRUE Then
                    teSite.tesubcases += 1
                End If
                If teAnte.tereport <> Constant.TRUE Then
                    teAnte.tereport = Constant.TRUE
                    teAnte.tesubcaseno = teSite.tesubcases
                    anteNulls(_DataStructures.TeAnte.TEREPORT) = Constant.DB_NOT_NULL
                    anteNulls(_DataStructures.TeAnte.TESUBCASENO) = Constant.DB_NOT_NULL
                End If
            End If

            '...Log2.v("\nTeBuildSH.TeDump(): Exit");
        End Sub ' ---- end teDump ----

        ''' <summary>
        ''' This method culls the channels from the environment based 
        ''' on channel status precribed by the User.  
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param>
        ''' <paramname="interferer"> - name of interferer.</param>
        ''' <paramname="esChan"> - FeChan object.</param>
        ''' <paramname="tsChan"> - FtChan object.</param>
        ''' <returns></returns>
        Public Shared Function CullChan(tpParm As TpParm, interferer As String, esChan As FeChan, tsChan As FtChan) As Integer
            '...Log2.v("\nTeBuildSH.CullChan(): Entry");

            Dim rc As Integer
            Dim getRc As Integer
            Dim codesList As String
            Dim aCode As String

            rc = Constant.FAILURE
            codesList = tpParm.chancodes

            getRc = GenUtil.UtGetInputString(codesList, aCode, 1)

            While rc <> Constant.SUCCESS AndAlso getRc >= 0

                ' if (proposed file type is "ES") 
                If Strings.FirstCharIs(tpParm.protype, "E"c) Then
                    ' ES (proposed) - TS (environment) 

                    ' if the environment(TS) is the interferer use it's
                    '  transmit status 
                    ' and (stattx of curr TS channel is in the list of
                    '  channel codes to cull) 
                    If Strings.FirstCharIs(interferer, "T"c) AndAlso aCode.Equals(tsChan.stattx) Then
                        rc = Constant.SUCCESS
                    End If

                    ' if the environment(TS) is the victim use it's
                    '  recieve status 
                    ' and (statrx of curr TS channel is in the list of
                    '  channel codes to cull) 
                    If Strings.FirstCharIs(interferer, "E"c) AndAlso aCode.Equals(tsChan.statrx) Then
                        rc = Constant.SUCCESS
                    End If
                Else
                    ' TS (proposed) - ES (environment) 

                    ' if the environment(ES) is the interferer use it's
                    '  transmit status 
                    ' and (stattx of curr ES channel is in the list of
                    '  channel codes to cull) 
                    If Strings.FirstCharIs(interferer, "E"c) AndAlso aCode.Equals(esChan.stattx) Then
                        rc = Constant.SUCCESS
                    End If

                    ' if the environment(ES) is the victim use it's
                    '  recieve status 
                    ' and (statrx of curr ES channel is in the list of
                    '  channel codes to cull) 
                    If Strings.FirstCharIs(interferer, "T"c) AndAlso aCode.Equals(esChan.statrx) Then
                        rc = Constant.SUCCESS
                    End If
                End If

                getRc = GenUtil.UtGetInputString(Nothing, aCode, 1)
            End While

            '...Log2.v("\nTeBuildSH.CullChan(): Exit");

            Return rc

        End Function ' ---- end cullChan ----

        ''' <summary>
        ''' This method sets the values for an ES Chan SH record using source data from the PDF and environment files; it calls TeChanCalcs() to finish the
        ''' population of the ES Chan SH record with data.
        ''' </summary>
        ''' <paramname="tpParm"> - TpParm object providing paramater data.</param></param>
        ''' <param name="teSite"> - TeSite object.</param>
        ''' <param name="teAnte"> - TeAnte object.</param>
        ''' <param name="teAnteNulls"> - ODBC nullInds for teAnte.</param>
        ''' <param name="teChan"> - TeChan object.</param>
        ''' <param name="teChanNulls"> - ODBC nullInds for teChan.</param>
        ''' <param name="tsChan"> - FtChan object.</param>
        ''' <param name="tsChanNulls"> - ODBC nullInds for tsChan.</param>
        ''' <param name="esChan"> - FeChan object.</param>
        ''' <param name="esChanNulls"> - ODBC nullInds for esChan.</param>
        ''' <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        ''' <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        ''' <returns></returns>
        Public Shared Function TeSetPCChan(tpParm As TpParm, teSite As TeSite, teAnte As TeAnte, teAnteNulls As SQLLEN(), ByRef teChan As TeChan, ByRef teChanNulls As SQLLEN(), tsChan As FtChan, tsChanNulls As SQLLEN(), esChan As FeChan, esChanNulls As SQLLEN(), intPrintMsg As String, vicPrintMsg As String) As Integer
            '...Log2.v("\nTeBuildSH.TeSetPCChan(): Entry");

            teChan.interferer = teAnte.interferer
            teChan.terrcall1 = teAnte.terrcall1
            teChan.terrcall2 = teAnte.terrcall2
            teChan.terrbndcde = teAnte.terrbndcde
            teChan.terranum = teAnte.terranum
            teChan.terrchid = tsChan.chid
            teChan.earthlocation = teAnte.earthlocation
            teChan.earthcall1 = teAnte.earthcall1
            teChan.earthchid = esChan.chid
            teChan.processed = Constant.TRUE

            teChanNulls(_DataStructures.TeChan.INTERFERER) = Constant.DB_NOT_NULL
            teChanNulls(_DataStructures.TeChan.TERRCALL1) = teAnteNulls(_DataStructures.TeAnte.TERRCALL1)
            teChanNulls(_DataStructures.TeChan.TERRCALL2) = teAnteNulls(_DataStructures.TeAnte.TERRCALL2)
            teChanNulls(_DataStructures.TeChan.TERRBNDCDE) = teAnteNulls(_DataStructures.TeAnte.TERRBNDCDE)
            teChanNulls(_DataStructures.TeChan.TERRANUM) = teAnteNulls(_DataStructures.TeAnte.TERRANUM)
            teChanNulls(_DataStructures.TeChan.TERRCHID) = tsChanNulls(FtChan.CHID)
            teChanNulls(_DataStructures.TeChan.EARTHLOCATION) = teAnteNulls(_DataStructures.TeAnte.EARTHLOCATION)
            teChanNulls(_DataStructures.TeChan.EARTHCALL1) = teAnteNulls(_DataStructures.TeAnte.EARTHCALL1)
            teChanNulls(_DataStructures.TeChan.EARTHCHID) = esChanNulls(FeChan.CHID)
            teChanNulls(_DataStructures.TeChan.PROCESSED) = Constant.DB_NOT_NULL

            If Strings.FirstCharIs(teAnte.interferer, "E"c) Then
                teChan.inttraftx = esChan.traftx
                teChan.victrafrx = tsChan.trafrx
                teChan.inteqpttx = esChan.eqpttx
                teChan.viceqptrx = tsChan.eqptrx
                teChan.stattx = esChan.stattx
                teChan.statrx = tsChan.statrx
                teChan.intfreqtx = esChan.freqtx
                teChan.inttxpwr = esChan.pwrtx
                teChan.energy = esChan.p4khz
                teChan.vicfreqrx = tsChan.freqrx

                teChanNulls(_DataStructures.TeChan.INTTRAFTX) = esChanNulls(FeChan.TRAFTX)
                teChanNulls(_DataStructures.TeChan.VICTRAFRX) = tsChanNulls(FtChan.TRAFRX)
                teChanNulls(_DataStructures.TeChan.INTEQPTTX) = esChanNulls(FeChan.EQPTTX)
                teChanNulls(_DataStructures.TeChan.VICEQPTRX) = tsChanNulls(FtChan.EQPTRX)
                teChanNulls(_DataStructures.TeChan.INTFREQTX) = esChanNulls(FeChan.FREQTX)
                teChanNulls(_DataStructures.TeChan.INTTXPWR) = esChanNulls(FeChan.PWRTX)
                teChanNulls(_DataStructures.TeChan.VICFREQRX) = tsChanNulls(FtChan.FREQRX)
                teChanNulls(_DataStructures.TeChan.STATTX) = esChanNulls(FeChan.STATTX)
                teChanNulls(_DataStructures.TeChan.ENERGY) = esChanNulls(FeChan.P4KHZ)
                teChanNulls(_DataStructures.TeChan.STATRX) = tsChanNulls(FtChan.STATRX)

                If teChan.terranum = tsChan.antnumbrx1 Then
                    teChan.vicpwrrx = CDbl(tsChan.pwrrx1)
                    teChan.terrant = 1
                    teChan.vicrxafls = tsChan.afslrx1
                    teChanNulls(_DataStructures.TeChan.VICRXAFLS) = Constant.DB_NOT_NULL
                    teChanNulls(_DataStructures.TeChan.VICPWRRX) = tsChanNulls(FtChan.PWRRX1)
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NOT_NULL
                ElseIf teChan.terranum = tsChan.antnumbrx2 Then
                    teChan.vicpwrrx = CDbl(tsChan.pwrrx2)
                    teChan.terrant = 2
                    teChan.vicrxafls = tsChan.afslrx2
                    teChanNulls(_DataStructures.TeChan.VICRXAFLS) = Constant.DB_NOT_NULL
                    teChanNulls(_DataStructures.TeChan.VICPWRRX) = tsChanNulls(FtChan.PWRRX2)
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NOT_NULL
                ElseIf teChan.terranum = tsChan.antnumbrx3 Then
                    teChan.vicpwrrx = CDbl(tsChan.pwrrx3)
                    teChan.terrant = 3
                    teChan.vicrxafls = tsChan.afslrx3
                    teChanNulls(_DataStructures.TeChan.VICRXAFLS) = Constant.DB_NOT_NULL
                    teChanNulls(_DataStructures.TeChan.VICPWRRX) = tsChanNulls(FtChan.PWRRX3)
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NOT_NULL
                Else
                    teChanNulls(_DataStructures.TeChan.VICRXAFLS) = Constant.DB_NULL
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NULL
                End If
            Else
                teChan.inttraftx = tsChan.traftx
                teChan.victrafrx = esChan.trafrx
                teChan.inteqpttx = tsChan.eqpttx
                teChan.viceqptrx = esChan.eqptrx
                teChan.stattx = tsChan.stattx
                teChan.statrx = esChan.statrx
                teChan.intfreqtx = tsChan.freqtx
                teChan.inttxpwr = tsChan.pwrtx
                teChan.vicfreqrx = esChan.freqrx
                teChan.energy = esChan.p4khz
                teChan.vicpwrrx = esChan.pwrrx

                teChanNulls(_DataStructures.TeChan.INTTRAFTX) = tsChanNulls(FtChan.TRAFTX)
                teChanNulls(_DataStructures.TeChan.VICTRAFRX) = esChanNulls(FeChan.TRAFRX)
                teChanNulls(_DataStructures.TeChan.INTEQPTTX) = tsChanNulls(FtChan.EQPTTX)
                teChanNulls(_DataStructures.TeChan.VICEQPTRX) = esChanNulls(FeChan.EQPTRX)
                teChanNulls(_DataStructures.TeChan.INTFREQTX) = tsChanNulls(FtChan.FREQTX)
                teChanNulls(_DataStructures.TeChan.INTTXPWR) = tsChanNulls(FtChan.PWRTX)
                teChanNulls(_DataStructures.TeChan.VICFREQRX) = esChanNulls(FeChan.FREQRX)
                teChanNulls(_DataStructures.TeChan.VICPWRRX) = esChanNulls(FeChan.PWRRX)
                teChanNulls(_DataStructures.TeChan.STATTX) = tsChanNulls(FtChan.STATTX)
                teChanNulls(_DataStructures.TeChan.ENERGY) = esChanNulls(FeChan.P4KHZ)
                teChanNulls(_DataStructures.TeChan.STATRX) = esChanNulls(FeChan.STATRX)

                If teChan.terranum = tsChan.antnumbtx1 Then
                    teChan.terrant = 1
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NOT_NULL
                    teChan.inttxafls = tsChan.afsltx1
                    teChanNulls(_DataStructures.TeChan.INTTXAFLS) = Constant.DB_NOT_NULL
                ElseIf teChan.terranum = tsChan.antnumbtx2 Then
                    teChan.terrant = 2
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NOT_NULL
                    teChan.inttxafls = tsChan.afsltx2
                    teChanNulls(_DataStructures.TeChan.INTTXAFLS) = Constant.DB_NOT_NULL
                Else
                    teChanNulls(_DataStructures.TeChan.TERRANT) = Constant.DB_NULL
                    teChanNulls(_DataStructures.TeChan.INTTXAFLS) = Constant.DB_NULL
                End If
            End If

            Dim rv As Integer = TpRunTsip.TeCalcs.TeChanCalcs(teChan, teAnte, teSite, tpParm, teChanNulls, intPrintMsg, vicPrintMsg)

            '...Log2.v("\nTeBuildSH.TeSetPCChan(): Exit");

            Return rv

        End Function ' ---- end teSetPCChan ----

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class










    End Class
End Namespace
