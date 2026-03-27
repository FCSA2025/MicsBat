Imports _DataStructures
Imports _NewLib
Imports _Utillib
Imports System
Imports System.Runtime.InteropServices
Imports _Configuration
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLHANDLE = System.IntPtr
Imports SQLHDBC = System.IntPtr
Imports SQLLEN = System.Int64
Imports SQLRETURN = System.Int16

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods that select, fetch, copy and insert records to/from
    ''' the main database tables.
    ''' </summary>
    Public Class TpMdbPdfGet
#If PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static void utGetInterferenceGroups([In] string runname, [In] string siteName, [In] string anteName, [In, Out] ref int TsEsStnGroups, [In, Out] ref int EsTsStnGroups);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int ttChanGet([In] string cCall1,
                            [In] string cCall2,
                            [In] string bndcde,
                            [In] string chid,
                            [In] string tabName,
                            [In] short isMDB,
                            [In, Out] FtChan chanStruct,
                            [In, Out] SQLLEN[] chanNulls);
        public static void UtGetInterferenceGroups_NATIVE(string runname,
                                                             string siteName,
                                                             string anteName,
                                                             out int TsEsStnGroups,
                                                             out int EsTsStnGroups)
        {
            //...Log2.v("\nTpMdbPdfGet.UtGetInterferenceGroups_NATIVE(): Entry");

            // out.
            TsEsStnGroups = 0;
            EsTsStnGroups = 0;

            // Native call.
            utGetInterferenceGroups(runname, siteName, anteName, ref TsEsStnGroups, ref EsTsStnGroups);

            //...Log2.v("\nTpMdbPdfGet.UtGetInterferenceGroups_NATIVE(): Exit");
        }

        public static int TtChanGet_NATIVE(string cCall1,
                            string cCall2,
                            string bndcde,
                            string chid,
                            string tabName,
                            bool isMDB,
                            out FtChan chanStruct,
                            out SQLLEN[] chanNulls)
        {
            // 'out' requirements;
            chanStruct = new FtChan();
            chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            short isMDB_short = Constant.FALSE;
            if (isMDB)
            {
                isMDB_short = Constant.TRUE;
            }

            int retVal = ttChanGet(cCall1, cCall2, bndcde, chid, tabName, isMDB_short, chanStruct, chanNulls);

            return retVal;
        }
#End If

        ''' <summary>
        ''' This method selects TS Ante data, that matches the seleciton criteria, 
        ''' from the appropriate TS table, based on the TSIP proposed and environment 
        ''' files specified. It returns a handle (index) to a cursor object that can
        ''' be used in subsequent calls to FetchTerrAnte().
        ''' </summary>
        ''' <paramname="tsAnteTableName"> - eponym.</param>
        ''' <paramname="selection"> - SQL 'WHERE' clause.</param>
        ''' <paramname="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        ''' <returns></returns>
        Public Shared Function SelectTerrAnte(tsAnteTableName As String, selection As String, envType As String) As Integer
            Dim anteHandle As Integer
            Dim selectClause As String

            If envType.Equals("MDB_TS") Then
                ' MDB is used as environment 
                anteHandle = DynMdbAntenna.MtSelectAntenna(selection, "")
            Else
                ' a proposed or an environment pdf is used 
                ' exclude deleted records 
                selection.Trim()
                If SQLCHARPTR.IsNullOrWhiteSpace(selection) Then
                    selectClause = "cmd != 'D'"
                Else
                    selectClause = SQLCHARPTR.Format("{0} and cmd != 'D'", selection)
                End If

                anteHandle = DynAntenna.FtSelectAntenna(tsAnteTableName, selectClause, "")
            End If
            Return anteHandle
        End Function

        ''' <summary>
        ''' This method populates an FtAnte object by fetching TS Ante data from the DB, 
        ''' using a cursor previously prepared by SelectTerrAnte(). 
        ''' </summary>
        ''' <paramname="anteHandle"> - handle (index) of cursor returned by SelectTerrAnte().</param>
        ''' <paramname="ftAnte"> - FtAnte object to be populated.</param>
        ''' <paramname="ftAnteNullInds"> - ODBC nullInds associated with ftAnte.</param>
        ''' <paramname="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        ''' <returns></returns>
        Public Shared Function FetchTerrAnte(anteHandle As Integer, <Out> ByRef ftAnte As FtAnte, <Out> ByRef ftAnteNullInds As SQLLEN(), envType As String) As Integer
            ' 'out' requirement.
            ftAnte = Nothing
            ftAnteNullInds = Nothing

            Dim rc As Integer

            If envType.Equals("MDB_TS") Then
                Dim mdbAnteNulls As SQLLEN()
                Dim mdbAnteStruct As MtAnte

                ' the mdb is used as environment 
                If CSharpImpl.__Assign(rc, DynMdbAntenna.MtFetchAntenna(anteHandle, mdbAnteStruct, mdbAnteNulls)) = Constant.SUCCESS Then
                    ftAnte = New FtAnte()
                    ftAnteNullInds = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    ' copy mdb Structure into the pdf structure 
                    FtValCopy.FtCopyAnte(ftAnte, mdbAnteStruct, ftAnteNullInds, mdbAnteNulls)
                End If
            Else
                ' a pdf (environment or proposed) is used 
                rc = DynAntenna.FtFetchAntenna(anteHandle, ftAnte, ftAnteNullInds)
            End If
            Return rc
        End Function

        ''' <summary>
        ''' This method closes the TS Antenna cursor opened by SelectTerrAnte() and releases
        ''' the associated ODBC connection and statement handles.  
        ''' </summary>
        ''' <paramname="anteHandle"> - cursor handle (index) to be closed.</param>
        ''' <paramname="envType"> - "MDB_TS" or other.</param>
        Public Shared Sub CloseTerrAnte(anteHandle As Integer, envType As String)
            If envType.Equals("MDB_TS") Then
                ' the mdb is used as environment 
                DynMdbAntenna.MtCloseAntenna(anteHandle)
            Else
                ' a pdf (environment or proposed) is used 
                DynAntenna.FtCloseAntenna(anteHandle)
            End If
        End Sub

        ''' <summary>
        ''' This method populates a FtSite object with data from the appropriate TS 
        ''' table and having the prescribed call sign.  
        ''' </summary>
        ''' <paramname="cCallSign"> - call sign.</param>
        ''' <paramname="tabName"> - name of table to read from if not an MDB table.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="ftSite"> - FtSite object to be populated.</param>
        ''' <paramname="ftSiteNullInds"> - ODBC nullInds associated with ftSite.</param>
        ''' <returns></returns>
        Public Shared Function TtSiteGetCall(cCallSign As String, tabName As String, isMDB As Boolean, <Out> ByRef ftSite As FtSite, <Out> ByRef ftSiteNullInds As SQLLEN()) As Integer         '	Expanded table name for the site.
            ' 'out' requirement.
            ftSite = Nothing
            ftSiteNullInds = Nothing

            Dim nRet As SQLRETURN
            Dim nHandle As Integer

            Dim cSelection As String

            If isMDB = True Then
                cSelection = SQLCHARPTR.Format("call1='{0}'", cCallSign)

                nHandle = DynMdbSite.MtSelectSite(cSelection, Nothing)

                Dim mtSite As MtSite
                Dim mtSiteNullInds As SQLLEN()

                nRet = CShort(DynMdbSite.MtFetchSite(nHandle, mtSite, mtSiteNullInds))

                DynMdbSite.MtCloseSite(nHandle)

                If ODBC.IsOK(nRet) Then
                    ftSite = New FtSite()
                    ftSiteNullInds = NullHelper.CreateArrayOfNullInd(FtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    ' copy mdb info into pdf structure 
                    FtValCopy.FtCopySite(ftSite, mtSite, ftSiteNullInds, mtSiteNullInds)
                End If
                DynMdbSite.MtCloseSite(nHandle)
            Else
                cSelection = SQLCHARPTR.Format("call1='{0}' and cmd != 'D' ", cCallSign)

                nHandle = DynSite.FtSelectSite(tabName, cSelection, Nothing)

                nRet = CShort(DynSite.FtFetchSite(nHandle, ftSite, ftSiteNullInds))

                DynSite.FtCloseSite(nHandle)
            End If

            Return nRet
        End Function

        ''' <summary>
        ''' This method populates an FeSite object with data fetched from the appropriate 
        ''' DB ES table that matches the prescribed selection criteria.  
        ''' </summary>
        ''' <paramname="location"> - prescribed location field/column value for selection.</param>
        ''' <paramname="tabName"> - name of table if not a main table.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="feSite"> - FeSite object to be populated.</param>
        ''' <paramname="feSiteNullInds"> - ODBC nullInds associated with feSite.</param>
        ''' <returns></returns>
        Public Shared Function TeSiteGetLoc(location As String, tabName As String, isMDB As Boolean, <Out> ByRef feSite As FeSite, <Out> ByRef feSiteNullInds As SQLLEN()) As Integer
            ' 'out' requirement
            feSite = Nothing
            feSiteNullInds = Nothing

            Dim meNulls As SQLLEN()  '[ME_SITE_SIZE_];
            Dim meSite As MeSite
            Dim sqlCommand As String

            Dim nCursor As Integer
            Dim nRet As Integer

            If isMDB Then
                sqlCommand = SQLCHARPTR.Format("location='{0}'", location)

                nCursor = DynMeSite.MeSelectSite(sqlCommand, "")

                nRet = DynMeSite.MeFetchSite(nCursor, meSite, meNulls)

                If nRet <> 0 Then
                    If nRet <> Constant.NOMORERECS Then
                        Return [Error].DYN_MS_SQL_SERVER_ERR
                    Else
                        Return Constant.FAILURE
                    End If
                Else
                    DynMeSite.MeCloseSite(nCursor)

                    ' copy mdb info into pdf structure 
                    ' AH: HERE
                    feSite = New FeSite()
                    feSiteNullInds = NullHelper.CreateArrayOfNullInd(FeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    FeValCopy.FeCopySite(feSite, meSite, feSiteNullInds, meNulls)
                End If
            Else
                sqlCommand = SQLCHARPTR.Format("location = '{0}' and cmd != 'D'", location)

                nCursor = DynFeSite.FeSelectSite(tabName, sqlCommand, "")

                nRet = DynFeSite.FeFetchSite(nCursor, feSite, feSiteNullInds)

                If nRet <> 0 Then
                    If nRet = Constant.NOMORERECS Then
                        Return [Error].DYN_MS_SQL_SERVER_ERR
                    Else
                        Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.TeSiteGetLoc(): ERROR: FeFetchSite() failed.")
                        Return Constant.FAILURE
                    End If
                End If

                DynFeSite.FeCloseSite(nCursor)
            End If
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method selects ES Ante data, that matches the seleciton criteria, 
        ''' from the appropriate ES table, based on the TSIP proposed and environment 
        ''' files specified. It returns a handle (index) to a cursor object that can
        ''' be used in subsequent calls to FetchEarthAnte().
        ''' </summary>
        ''' <paramname="tsAnteTableName"> - name of table to be used if not a main table.</param>
        ''' <paramname="selection"> - part of an SQL 'WHERE' clause used for record selection.</param>
        ''' <paramname="envType"> - "MDB_ES" or other.</param>
        ''' <returns></returns>
        Public Shared Function SelectEarthAnte(tsAnteTableName As String, selection As String, envType As String) As Integer
            Dim anteHandle As Integer
            Dim selectClause As String

            If envType.Equals("MDB_ES") Then
                ' MDB is used as environment 
                anteHandle = DynMeAnte.MeSelectAnte(selection, "")
            Else
                ' a proposed or an environment pdf is used 
                ' exclude deleted records 
                selection.Trim()
                If SQLCHARPTR.IsNullOrWhiteSpace(selection) Then
                    selectClause = "cmd != 'D'"
                Else
                    selectClause = SQLCHARPTR.Format("{0} and cmd != 'D'", selection)
                End If

                anteHandle = DynFeAnte.FeSelectAnte(tsAnteTableName, selectClause, "")
            End If
            Return anteHandle
        End Function

        ''' <summary>
        ''' This method closes the ES Antenna cursor opened by SelectTerrAnte() and releases
        ''' the associated ODBC connection and statement handles.
        ''' </summary>
        ''' <paramname="anteHandle"> - handle (index) of handle to be closed.</param>
        ''' <paramname="envType"> - "MDB_ES" or other.</param>
        Public Shared Sub CloseEarthAnte(anteHandle As Integer, envType As String)
            If envType.Equals("MDB_ES") Then
                ' the mdb is used as environment 
                DynMeAnte.MeCloseAnte(anteHandle)
            Else
                ' a pdf (environment or proposed) is used 
                DynFeAnte.FeCloseAnte(anteHandle)
            End If
        End Sub

        ''' <summary>
        ''' This method populates an FeAnte object by fetching ES Ante data from the DB, 
        ''' using a cursor previously prepared by SelectEarthAnte().
        ''' </summary>
        ''' <paramname="anteHandle"> - handle (index) of cursor returned by SelectTerrAnte().</param>
        ''' <paramname="feAnte"> - FeAnte object to be populated.</param>
        ''' <paramname="feAnteNullInds"> - ODBC nullInds associated with ftAnte.</param>
        ''' <paramname="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        ''' <returns></returns>
        Public Shared Function FetchEarthAnte(anteHandle As Integer, <Out> ByRef feAnte As FeAnte, <Out> ByRef feAnteNullInds As SQLLEN(), envType As String) As Integer
            ' 'out' requirement.
            feAnte = Nothing
            feAnteNullInds = Nothing

            Dim mdbAnteStruct As MeAnte
            Dim mdbAnteNulls As SQLLEN()  '[ME_ANTE_SIZE_];
            Dim rc As Integer

            If envType.Equals("MDB_ES") Then
                ' the mdb is used as environment 
                If CSharpImpl.__Assign(rc, DynMeAnte.MeFetchAnte(anteHandle, mdbAnteStruct, mdbAnteNulls)) = Constant.SUCCESS Then
                    ' copy mdb Structure into the pdf structure 
                    feAnte = New FeAnte()
                    feAnteNullInds = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    FeValCopy.FeCopyAnte(feAnte, mdbAnteStruct, feAnteNullInds, mdbAnteNulls)
                End If
            Else
                ' a pdf (environment or proposed) is used 
                rc = DynFeAnte.FeFetchAnte(anteHandle, feAnte, feAnteNullInds)
            End If
            Return rc
        End Function

        ''' <summary>
        ''' This method populates an FtAnte object with data from 
        ''' the appropriate TS table that matches the prescribed selection criteria: 
        ''' {call1, call2, bndcde & anum}.  
        ''' </summary>
        ''' <paramname="cCall1"> - local call sign.</param>
        ''' <paramname="cCall2"> - remote call sign.</param>
        ''' <paramname="cBand"> - band code.</param>
        ''' <paramname="anum"> - antenna number.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="ftAnte"> - FtAnte object to be populated.</param>
        ''' <paramname="ftAnteNulls"> - ODBC nullInds associated with ftAnte.</param>
        ''' <returns></returns>
        Public Shared Function TtAnteGet(cCall1 As String, cCall2 As String, cBand As String, anum As Integer, tabName As String, isMDB As Boolean, <Out> ByRef ftAnte As FtAnte, <Out> ByRef ftAnteNulls As SQLLEN()) As Integer
            ' 'out' requirement.
            ftAnte = Nothing
            ftAnteNulls = Nothing

            Dim mtNulls As SQLLEN()  '[MtAnte.SIZE_];
            Dim mtAnte As MtAnte
            Dim nRet As Integer

            If isMDB Then
                ' BUG FIX PENDING.
                ' If the MDB tables are incoherent (i.e. channels can't find their antennas etc)
                ' MtReadAnte() will return mtAnte as null with a non-zero nRet.
                ' We need some 'guard' code to detect and handle this pathological situation.
                nRet = MtUtils.MtReadAnte(cCall1, cCall2, cBand, anum, mtAnte, mtNulls)

                If nRet <> Constant.SUCCESS Then Log2.e(Microsoft.VisualBasic.Constants.vbLf & Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.TtAnteGet(): ERROR: nRet = " & nRet.ToString())

                ' copy mdb info into pdf structure 
                ftAnte = New FtAnte()
                ftAnteNulls = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                FtValCopy.FtCopyAnte(ftAnte, mtAnte, ftAnteNulls, mtNulls)
            Else
                '	memset(&ftAnte, 0, sizeof(ftAnte));
                nRet = FtUtils.FtReadAnte(cCall1, cCall2, cBand, anum, tabName, ftAnte, ftAnteNulls)
                If ftAnte.cmd.Equals("D") Then
                    nRet = Constant.NOMORERECS
                End If
            End If

            Return nRet
        End Function

        ''' <summary> 
        ''' This method populates an FeAnte object with data from 
        ''' the appropriate ES table that matches the prescribed selection criteria. 
        ''' </summary>
        ''' <paramname="select"> - part of an SQL SELECT query WHERE clause.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="feAnte">- FeAnte object to be populated.</param>
        ''' <paramname="feAnteNulls"> - ODBC nullInds associated with feAnte.</param>
        ''' <returns></returns>
        Public Shared Function TeAnteGet([select] As String, tabName As String, isMDB As Boolean, <Out> ByRef feAnte As FeAnte, <Out> ByRef feAnteNulls As SQLLEN()) As Integer
            ' 'out' requirement.
            feAnte = Nothing
            feAnteNulls = Nothing

            Dim meNulls As SQLLEN()  ' [ME_ANTE_SIZE_];
            Dim meAnte As MeAnte

            Dim curHandle As Integer
            Dim nRet As Integer

            If isMDB Then
                curHandle = DynMeAnte.MeSelectAnte([select], "")
                If curHandle < 0 Then
                    Return curHandle
                End If
                nRet = DynMeAnte.MeFetchAnte(curHandle, meAnte, meNulls)
                If nRet <> 0 Then
                    Return Constant.FAILURE
                End If

                ' copy mdb info into pdf structure 
                feAnte = New FeAnte()
                feAnteNulls = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                FeValCopy.FeCopyAnte(feAnte, meAnte, feAnteNulls, meNulls)

                DynMeAnte.MeCloseAnte(curHandle)
            Else
                curHandle = DynFeAnte.FeSelectAnte(tabName, [select], "")
                If curHandle < 0 Then
                    Return curHandle
                End If
                nRet = DynFeAnte.FeFetchAnte(curHandle, feAnte, feAnteNulls)
                If nRet <> 0 Then
                    Return Constant.FAILURE
                End If

                DynFeAnte.FeCloseAnte(curHandle)
            End If

            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method populates an FtSiteStr object with data from 
        ''' the appropriate TS tables that matches the prescribed selection criteria; FtSiteStr is a 
        ''' class type that encapsulates an FtSite and its associated FtAntes.
        ''' </summary>
        ''' <paramname="cCall1"> - local call sign.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="pSite">- FtSiteStr object to be populated.</param>
        ''' <paramname="siteNulls"> - FtSiteStrNulls object providing ODBC nullInd data for the contents of pSite.</param>
        ''' <returns></returns>
        Public Shared Function TtSiteGetAnte(cCall1 As String, tabName As String, isMDB As Boolean, <Out> ByRef pSite As FtSiteStr, <Out> ByRef siteNulls As FtSiteStrNulls) As Integer
            ' 'out' requirements.
            pSite = Nothing
            siteNulls = Nothing

            Dim nRet As Integer
            Dim pMtSite As MtSiteStr
            Dim pMtNulls As MtSiteStrNulls


            If isMDB = True Then
                nRet = MtUtils.MtGetSiteWN(cCall1, pMtSite, 2, pMtNulls)  ' Just to the antenna level 
                If nRet = 0 Then
                    ' copy mdb info into pdf structure 
                    MtUtils.MtToFtWN(pSite, siteNulls, pMtSite, pMtNulls)    ' Ptr to input mt site          

                End If
            Else
                nRet = FtUtils.FtGetSiteWN(cCall1, pSite, 2, tabName, siteNulls)
            End If

            Return nRet
        End Function

        ''' <summary>
        ''' This method selects ES Chan data, that matches the seleciton criteria, 
        ''' from the appropriate ES table, based on the TSIP proposed and environment 
        ''' files specified. It returns a handle (index) to a cursor object that can
        ''' be used in subsequent calls to FetchEarthChan(). 
        ''' </summary>
        ''' <paramname="tsChanTableName"> - name of table to be used if not a main table.</param>
        ''' <paramname="selection"> - part of an SQL 'WHERE' clause used for record selection.</param>
        ''' <paramname="envType"> - "MDB_ES" or other.</param>
        ''' <returns></returns>
        Public Shared Function SelectEarthChan(tsChanTableName As String, selection As String, envType As String) As Integer
            ' Beware: there are two distinct species of chanHandle: one from DynMeChan
            ' and the other from DynFeChan.
            Dim chanHandle As Integer

            Dim selectClause As String

            If envType.Equals("MDB_ES") Then
                '&&Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): molusc");
                ' MDB is used as environment 
                chanHandle = DynMeChan.MeSelectChan(selection, "")
            Else
                ' a proposed or an environment pdf is used 
                ' exclude deleted records 
                selection.Trim()
                If SQLCHARPTR.IsNullOrWhiteSpace(selection) Then
                    '&&Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): snail");
                    selectClause = "cmd != 'D'"
                Else
                    '&&Console.Error.Write("\ntpMdbPdfGet.selectEarthChan(): eel");
                    selectClause = SQLCHARPTR.Format("{0} and cmd != 'D'", selection)
                End If

                chanHandle = DynFeChan.FeSelectChan(tsChanTableName, selectClause, "")
            End If
            Return chanHandle
        End Function

        ''' <summary>
        ''' This method populates an FeChan object by fetching ES Chan data from the DB, 
        ''' using a cursor previously prepared by SelectEarthChan().
        ''' </summary>
        ''' <paramname="chanHandle"> - handle (index) of cursor returned by SelectEarthChan().</param>
        ''' <paramname="chanStruct"> - FeChan object to be populated.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <paramname="envType"> - if envType == "MDB_ES" then the MDB is used as the environment.</param>
        ''' <returns></returns>
        Public Shared Function FetchEarthChan(chanHandle As Integer, <Out> ByRef chanStruct As FeChan, <Out> ByRef chanNulls As SQLLEN(), envType As String) As Integer
            ' 'out' requirements.
            chanStruct = Nothing
            chanNulls = Nothing

            Dim rc As Integer

            If envType.Equals("MDB_ES") Then
                Dim mdbChanStruct As MeChan
                Dim mdbChanNulls As SQLLEN()

                ' the mdb is used as environment 
                '&&Console.Error.Write("\ntpMdbPdfGet.teChanGet(): tiger");
                If CSharpImpl.__Assign(rc, DynMeChan.MeFetchChan(chanHandle, mdbChanStruct, mdbChanNulls)) = Constant.SUCCESS Then
                    chanStruct = New FeChan()
                    chanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    ' copy mdb Structure into the pdf structure 
                    FeValCopy.FeCopyChan(chanStruct, mdbChanStruct, chanNulls, mdbChanNulls)
                End If
            Else
                ' a pdf (environment or proposed) is used 
                rc = DynFeChan.FeFetchChan(chanHandle, chanStruct, chanNulls)
            End If
            Return rc
        End Function

        ''' <summary>
        ''' This method selects TS Chan data, that matches the selection criteria, 
        ''' from the appropriate TS table, based on the TSIP proposed and environment 
        ''' files specified. It returns a handle (index) to a cursor object that can
        ''' be used in subsequent calls to FetchTerrChan().  
        ''' </summary>
        ''' <paramname="tsChanTableName"> - name of table to be used if not a main table.</param>
        ''' <paramname="selection"> - part of an SQL 'WHERE' clause used for record selection.</param>
        ''' <paramname="envType"> - "MDB_TS" or other.</param>
        ''' <returns></returns>
        Public Shared Function SelectTerrChan(tsChanTableName As String, selection As String, envType As String) As Integer
            Dim chanHandle As Integer
            Dim selectClause As String

            If envType.Equals("MDB_TS") Then
                ' MDB is used as environment 
                chanHandle = DynMdbChannel.MtSelectChannel(selection, "")
            Else
                ' a proposed or an environment pdf is used 

                ' exclude deleted records 
                If SQLCHARPTR.IsNullOrWhiteSpace(selection) Then
                    selectClause = "cmd != 'D'"
                Else
                    selectClause = SQLCHARPTR.Format("{0} and cmd != 'D'", selection)
                End If

                chanHandle = DynChannel.FtSelectChannel(tsChanTableName, selectClause, "call1")
            End If
            Return chanHandle
        End Function

        ''' <summary>
        ''' This method populates an FtChan object by fetching TS Chan data from the DB, 
        ''' using a cursor previously prepared by SelectTerrChan(). 
        ''' </summary>
        ''' <paramname="chanHandle"> - handle (index) of cursor returned by SelectTerrChan().</param>
        ''' <paramname="chanStruct"> - FtChan object to be populated.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <paramname="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        ''' <returns></returns>
        Public Shared Function FetchTerrChan(chanHandle As Integer, <Out> ByRef chanStruct As FtChan, <Out> ByRef chanNulls As SQLLEN(), envType As String) As Integer
            ' 'out' requirements.
            chanStruct = Nothing
            chanNulls = Nothing

            Dim rc As Integer

            If envType.Equals("MDB_TS") Then
                ' the mdb is used as environment 
                Dim mdbChanStruct As MtChan
                Dim mdbChanNulls As SQLLEN()

                If CSharpImpl.__Assign(rc, DynMdbChannel.MtFetchChannel(chanHandle, mdbChanStruct, mdbChanNulls)) = Constant.SUCCESS Then
                    ' copy mdb Structure into the pdf structure 
                    chanStruct = New FtChan()
                    chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    FtValCopy.FtCopyChan(chanStruct, mdbChanStruct, chanNulls, mdbChanNulls)
                End If
            Else
                ' a pdf (environment or proposed) is used 
                rc = DynChannel.FtFetchChannel(chanHandle, chanStruct, chanNulls)
            End If
            Return rc
        End Function

        ''' <summary>
        ''' This method closes the TS Channel cursor opened by SelectTerrChan() and releases
        ''' the associated ODBC connection and statement handles. 
        ''' </summary>
        ''' <paramname="chanHandle"> - cursor handle (index) to be closed.</param>
        ''' <paramname="envType"> - "MDB_TS" or other.</param>
        Public Shared Sub CloseTerrChan(chanHandle As Integer, envType As String)
            If envType.Equals("MDB_TS") Then
                ' the mdb is used as environment 
                DynMdbChannel.MtCloseChannel(chanHandle)
            Else
                ' a pdf (environment or proposed) is used 
                DynChannel.FtCloseChannel(chanHandle)
            End If
        End Sub

        ''' <summary>
        ''' This method closes the ES Channel cursor opened by SelectEarthChan() and releases
        ''' the associated ODBC connection and statement handles. 
        ''' </summary>
        ''' <paramname="chanHandle"> - cursor handle (index) to be closed.</param>
        ''' <paramname="envType"> - "MDB_ES" or other.</param>
        Public Shared Sub CloseEarthChan(chanHandle As Integer, envType As String)
            If envType.Equals("MDB_ES") Then
                ' the mdb is used as environment 
                DynMeChan.MeCloseChan(chanHandle)
            Else
                ' a pdf (environment or proposed) is used 
                DynFeChan.FeCloseChan(chanHandle)
            End If
        End Sub

        ''' <summary>
        ''' This method populates an FtSiteStr object with data from 
        ''' the appropriate TS tables that matches the prescribed selection criteria (call sign);
        ''' FtSiteStr is a class type that encapsulates an FtSite and its associated 
        ''' FtAntes and FtChans. 
        ''' </summary>
        ''' <paramname="cCallSign"> - local call sign.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="pSite">- FtSiteStr object to be populated.</param>
        ''' <paramname="siteNulls"> - FtSiteStrNulls object providing ODBC nullInd data for the contents of pSite.</param>
        ''' <returns></returns>
        Public Shared Function TtFullSiteGet(cCallSign As String, tabName As String, isMDB As Boolean, <Out> ByRef pSite As FtSiteStr, <Out> ByRef siteNulls As FtSiteStrNulls) As Integer
            ' 'out' requirements.
            pSite = Nothing
            siteNulls = Nothing

            Dim nRet As Integer
            Dim pMtSite As MtSiteStr
            Dim pMtNulls As MtSiteStrNulls

            If isMDB Then
                nRet = MtUtils.MtGetSiteWN(cCallSign, pMtSite, 3, pMtNulls)
                If nRet = 0 Then
                    ' Copy mdb info into pdf structure 
                    MtUtils.MtToFtWN(pSite, siteNulls, pMtSite, pMtNulls)
                End If
            Else
                nRet = FtUtils.FtGetSiteWN(cCallSign, pSite, 3, tabName, siteNulls)
            End If

            Return nRet
        End Function

        ''' <summary>
        ''' This method populates an FeChan object with data from 
        ''' the appropriate ES table that matches the prescribed selection criteria. 
        ''' </summary>
        ''' <paramname="select"> - part of an SQL SELECT query WHERE clause.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="chanStruct">- FeChan object to be populated.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <returns></returns>
        Public Shared Function TeChanGet([select] As String, tabName As String, isMDB As Boolean, <Out> ByRef chanStruct As FeChan, <Out> ByRef chanNulls As SQLLEN()) As Integer
            ' 'out' requirements.
            chanStruct = Nothing
            chanNulls = Nothing

            Dim meNulls As SQLLEN()  '[MeChan.NUM_COLUMNS];
            Dim meChan As MeChan
            Dim nHandle As Integer
            Dim nRet As Integer

            If isMDB = True Then
                nHandle = DynMeChan.MeSelectChan([select], "location,call1,chid")
                '!!Console.Error.Write("\nTpMdbPdfGet.teChanGet(): horse, nHandle = " + nHandle);
                If nHandle < 0 Then
                    Return nHandle
                End If
                '&&Console.Error.Write("\ntpMdbPdfGet.teChanGet(): Gnat");
                nRet = DynMeChan.MeFetchChan(nHandle, meChan, meNulls)
                '!!Console.Error.Write("\nTpMdbPdfGet.teChanGet(): horse, nHandle = " + nHandle);
                DynMeChan.MeCloseChan(nHandle)

                If nRet = 0 Then
                    chanStruct = New FeChan()
                    chanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    ' copy mdb info into pdf structure 
                    FeValCopy.FeCopyChan(chanStruct, meChan, chanNulls, meNulls)
                End If
            Else
                nHandle = DynFeChan.FeSelectChan(tabName, [select], "location,call1,chid")
                If nHandle < 0 Then
                    Return nHandle
                End If

                nRet = DynFeChan.FeFetchChan(nHandle, chanStruct, chanNulls)

                DynFeChan.FeCloseChan(nHandle)
            End If

            Return nRet
        End Function


        ''' <summary>
        ''' This method populates an FtAnte object with data from 
        ''' the appropriate TS table that matches the prescribed selection criteria.  
        ''' </summary>
        ''' <paramname="selection"> - part of an SQL SELECT query WHERE clause.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        ''' <paramname="anteStruct"> - FtAnte object to be populated.</param>
        ''' <paramname="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        ''' <returns></returns>
        Public Shared Function TtAnteGetCond(selection As String, tabName As String, isMDB As Boolean, <Out> ByRef anteStruct As FtAnte, <Out> ByRef anteNulls As SQLLEN()) As Integer
            ' 'out' requirements.
            anteStruct = Nothing
            anteNulls = Nothing

            Dim mtNulls As SQLLEN()  '[MtAnte.SIZE_];
            Dim mtAnte As MtAnte
            Dim nRet As Integer
            Dim nHandle As Integer

            If isMDB Then
                'memset(&mtAnte, 0, sizeof(mtAnte));  /*	Zero to handle nulls. 
                'sqlCommand = String.Format("select * from {0} where {1}", tabName, select);
                'exec sql execute immediate :sqlCommand into :mtAnte:mtNulls;
                'if (sqlca.sqlcode == Constant.NOMORERECS) {
                '	return(Constant.FAILURE);
                '} else if ((sqlca.sqlcode != Constant.SUCCESS)	&& (sqlca.sqlcode != SEVERALRECS)) {
                '	return(Constant.DYN_MS_SQL_SERVER_ERR);
                '}
                nHandle = DynMdbAntenna.MtSelectAntenna(selection, "")
                If nHandle < 0 Then
                    Return nHandle
                End If

                nRet = DynMdbAntenna.MtFetchAntenna(nHandle, mtAnte, mtNulls)

                DynMdbAntenna.MtCloseAntenna(nHandle)

                If nRet = 0 Then
                    anteStruct = New FtAnte()
                    anteNulls = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    ' copy mdb info into pdf structure 
                    FtValCopy.FtCopyAnte(anteStruct, mtAnte, anteNulls, mtNulls)
                End If
            Else
                nHandle = DynAntenna.FtSelectAntenna(tabName, selection, "")

                nRet = DynAntenna.FtFetchAntenna(nHandle, anteStruct, anteNulls)

                DynAntenna.FtCloseAntenna(nHandle)
            End If

            Return nRet
        End Function

        ''' <summary>
        ''' This method populates an FtChan object with data from 
        ''' the appropriate TS table that matches the prescribed selection criteria: 
        ''' {call1, call2, bndcde & chid}. 
        ''' </summary>
        ''' <paramname="cCall1"> - local call sign.</param>
        ''' <paramname="cCall2"> - remote call sign.</param>
        ''' <paramname="bndcde"> - band code.</param>
        ''' <paramname="chid"> - channel ID.</param>
        ''' <paramname="tabName"> - name of DB table to use if not MDB.</param>
        ''' <paramname="isMDB"> - indicates whether the tables are in the main DB table set, or not</param>
        ''' <paramname="chanStruct"> - FtChan object to be populated.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <returns></returns>
        Public Shared Function TtChanGet(cCall1 As String, cCall2 As String, bndcde As String, chid As String, tabName As String, isMDB As Boolean, <Out> ByRef chanStruct As FtChan, <Out> ByRef chanNulls As SQLLEN()) As Integer
            ' 'out' requirements.
            chanStruct = Nothing
            chanNulls = Nothing

            Dim mtNulls As SQLLEN()   ' [MtChan.SIZE_];
            Dim mtChan As MtChan

            Dim nRet = 0

            If isMDB = True Then
                nRet = MtUtils.MtReadChan(cCall1, cCall2, bndcde, chid, mtChan, mtNulls)

                If nRet = Constant.NOMORERECS Then
                    Return Constant.FAILURE
                ElseIf nRet <> 0 Then
                    Return [Error].DYN_MS_SQL_SERVER_ERR
                End If

                chanStruct = New FtChan()
                chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                ' copy mdb info into pdf structure 
                FtValCopy.FtCopyChan(chanStruct, mtChan, chanNulls, mtNulls)
            Else
                nRet = FtUtils.FtReadChan(cCall1, cCall2, bndcde, chid, tabName, chanStruct, chanNulls)

                If nRet = Constant.NOMORERECS Then
                    Return Constant.FAILURE
                ElseIf chanStruct.cmd.Equals("D") Then
                    Return Constant.FAILURE
                ElseIf nRet <> 0 Then
                    Return [Error].DYN_MS_SQL_SERVER_ERR
                End If
            End If

            Return nRet
        End Function

        ''' <summary>
        ''' This method returns the counts of the Es-Ts and Ts-Es interference cases for ES.  
        ''' </summary>
        ''' <paramname="runname"> - User-defined unique run ID.</param>
        ''' <paramname="siteName"> - name of site table.</param>
        ''' <paramname="anteName"> - name of ante table.</param>
        ''' <paramname="TsEsStnGroups"> - count of the number of Ts-Es interference cases for ES.</param>
        ''' <paramname="EsTsStnGroups"> - count of the number of Es-Ts interference cases for ES.</param>
        ''' <returns></returns>
        Public Shared Function UtGetInterferenceGroups(runname As String, siteName As String, anteName As String, <Out> ByRef TsEsStnGroups As Integer, <Out> ByRef EsTsStnGroups As Integer) As Integer
            ' 'out' requirement.
            TsEsStnGroups = 0
            EsTsStnGroups = 0

            Dim rv = Constant.FAILURE
            Dim selcount As String
            Dim cUnique As String

            Dim sqlRet As SQLRETURN
            Dim hStmt As SQLHANDLE
            Dim hConn As SQLHDBC = Ssutil.NewConn()

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, hStmt)
            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.UtGetInterferenceGroups(): ERROR: call to SQLAllocHandle() failed.")
                Return [Error].ODBC_SQLALLOCHANDLE_FAILED
            End If

            GenUtil.MkUnique(cUnique, Info.GlobalSchema, "", "stat_cnt_", "", runname)
            ' 	In ingres 2.5 it looks like dropping a table that does not exist
            ' 		will cause an error message. So remove this. GJS - 2002.07.02
            ' selcount = String.Format("drop table {0}", cUnique);
            ' exec sql execute immediate :selcount;
            ' 

            selcount = SQLCHARPTR.Format("create view {0} as select distinct a.terrcall1, a.earthlocation, b.earthcall1, b.earthband from {1} a, {2} b", cUnique, siteName, anteName)
            selcount += " where a.terrcall1 = b.terrcall1 and a.earthlocation = b.earthlocation and interferer = 'T'"

            '	exec sql execute immediate :selcount;
            sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.UtGetInterferenceGroups(): ERROR: A: call to SQLExecDirect() failed: " & Microsoft.VisualBasic.Constants.vbLf & selcount)
                TsEsStnGroups = -1
                rv = [Error].ODBC_EXECDIRECT_FAILED
            Else
                TsEsStnGroups = Ssutil.DbCountRows(cUnique, Nothing)
                If TsEsStnGroups < 0 Then
                    TsEsStnGroups = 0
                End If

                rv = Constant.SUCCESS

                selcount = SQLCHARPTR.Format("drop view {0}", cUnique)
                sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length)

                If Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.UtGetInterferenceGroups(): ERROR: B: call to SQLExecDirect() failed: " & Microsoft.VisualBasic.Constants.vbLf & selcount)
                    rv = [Error].ODBC_EXECDIRECT_FAILED
                End If
            End If

            selcount = SQLCHARPTR.Format("create view {0} as select distinct a.earthlocation, b.earthcall1, b.earthband, a.terrcall1 from {1} a, {2} b", cUnique, siteName, anteName)
            selcount += " where a.earthlocation = b.earthlocation and a.terrcall1 = b.terrcall1 and interferer = 'E'"

            'exec sql execute immediate :selcount;
            sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length)

            If Not ODBC.IsOK(sqlRet) Then
                Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.UtGetInterferenceGroups(): ERROR: C: call to SQLExecDirect() failed: " & Microsoft.VisualBasic.Constants.vbLf & selcount)
                EsTsStnGroups = -1
                rv = [Error].ODBC_EXECDIRECT_FAILED
            Else
                EsTsStnGroups = Ssutil.DbCountRows(cUnique, Nothing)
                If EsTsStnGroups < 0 Then
                    EsTsStnGroups = 0
                End If

                rv = Constant.SUCCESS

                selcount = SQLCHARPTR.Format("drop view {0}", cUnique)

                sqlRet = ODBC.SQLExecDirect(hStmt, selcount, selcount.Length)

                If Not ODBC.IsOK(sqlRet) Then
                    Log2.e(Microsoft.VisualBasic.Constants.vbLf & "TpMdbPdfGet.UtGetInterferenceGroups(): ERROR: D: call to SQLExecDirect() failed: " & selcount)
                    rv = [Error].ODBC_EXECDIRECT_FAILED
                End If

            End If

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt)
            Ssutil.DisConn(hConn)

            Return rv
        End Function

        ''' <summary>
        ''' This method enumerates sites in the <b>main.mt_site</b> table or a 
        ''' caller-defined table that match a prescribed selection criteria.  
        ''' </summary>
        ''' <paramname="cSearch"> - part of an SQL SELECT query WHERE clause.</param>
        ''' <paramname="cTable"> - name of table to be searched; if null or blank then the main table is used.</param>
        ''' <paramname="cCallFound"> - on input this prescribes that any found call sign must 
        ''' be alphabetically greater than cCallFound; if a record that matches all the search 
        ''' criteria is found, cCallFound returns its call sign.</param>
        ''' <returns></returns>
        Public Shared Function TtEnumSite(cSearch As String, cTable As String, ByRef cCallFound As String) As Integer
            If SQLCHARPTR.IsNullOrWhiteSpace(cTable) Then
                '	mdb.
                Return MtUtils.MtEnumSite(cSearch, cCallFound)
            Else
                '	pdf
                Return FtUtils.FtEnumSite(cSearch, cTable, cCallFound)
            End If
        End Function

        ''' <summary>
        ''' This method selects TS Chan data, that matches the selection criteria, 
        ''' from the appropriate TS table, based on the TSIP proposed and environment 
        ''' files specified. It returns a handle (index) to a cursor object that can
        ''' be used in subsequent calls to TtTtFetchChannel(). 
        ''' </summary>
        ''' <paramname="chanTableName"> - name of table to be used.</param>
        ''' <paramname="select"> - part of an SQL 'WHERE' clause used for record selection.</param>
        ''' <paramname="envType"> - "MDB_TS" or other.</param>
        ''' <returns></returns>
        Public Shared Function TtTtSelectChannel(chanTableName As String, [select] As String, envType As String) As Integer
            Dim chanHandle As Integer
            Dim selectClause As String

            If envType.Equals("MDB_TS") Then   ' MDB is used 
                chanHandle = DynMdbChannel.MtSelectChannel([select], "")   ' an environment pdf is used 
            Else
                [select] = [select].Trim()
                selectClause = [select]
                If SQLCHARPTR.IsNullOrWhiteSpace([select]) Then
                    selectClause = "cmd != 'D'"
                Else
                    selectClause += " and cmd != 'D'"
                End If

                chanHandle = DynChannel.FtSelectChannel(chanTableName, selectClause, "call1")
            End If
            Return chanHandle
        End Function

        ''' <summary>
        ''' This method populates an FtChan object by fetching TS Chan data from the DB, 
        ''' using a cursor previously prepared by TtTtSelectChannel().
        ''' </summary>
        ''' <paramname="chanHandle"> - handle (index) of cursor returned by TtTtSelectChannel()</param>
        ''' <paramname="chanStruct"> - FtChan object to be populated.</param>
        ''' <paramname="chanNulls"> - ODBC nullInds associated with chanStruct.</param>
        ''' <paramname="envType"> - if envType == "MDB_TS" then the MDB is used as the environment.</param>
        ''' <returns></returns>
        Public Shared Function TtTtFetchChannel(chanHandle As Integer, <Out> ByRef chanStruct As FtChan, <Out> ByRef chanNulls As SQLLEN(), envType As String) As Integer
            ' 'out' requirements.
            chanStruct = Nothing
            chanNulls = Nothing

            Dim rc As Integer
            Dim mdbChanNulls As SQLLEN()  '[MtChan.NUM_COLUMNS];
            Dim mdbChanStruct As MtChan

            If envType.Equals("MDB_TS") Then   ' the mdb is used 
                rc = DynMdbChannel.MtFetchChannel(chanHandle, mdbChanStruct, mdbChanNulls)

                If rc = Constant.SUCCESS Then
                    ' copy mdb Structure into the pdf structure 
                    chanStruct = New FtChan()
                    chanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL)

                    FtValCopy.FtCopyChan(chanStruct, mdbChanStruct, chanNulls, mdbChanNulls)

                    chanStruct.cmd = "N"
                End If   ' a pdf is used 
            Else
                rc = DynChannel.FtFetchChannel(chanHandle, chanStruct, chanNulls)
            End If

            Return rc
        End Function

        ''' <summary>
        ''' This method closes the TS Channel cursor opened by TtTtSelectChannel() and releases
        ''' the associated ODBC connection and statement handles.  
        ''' </summary>
        ''' <paramname="chanHandle"> - cursor handle (index) to be closed.</param>
        ''' <paramname="envType"> - "MDB_TS" or other.</param>
        Public Shared Sub TtTtCloseChannel(chanHandle As Integer, envType As String)
            If envType.Equals("MDB_TS") Then
                ' the mdb is used 
                DynMdbChannel.MtCloseChannel(chanHandle)
            Else
                ' a pdf is used 
                DynChannel.FtCloseChannel(chanHandle)
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
