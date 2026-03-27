Imports _DataStructures
Imports System
Imports _Configuration
Imports _Utillib
Imports SQLLEN = System.Int64
Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods that perform 'utility' functions that are
    ''' unique to TpRunTsip.
    ''' </summary>
    Public Class TsipUtils

        ''' <summary>
        ''' This method inserts a populated TpParm object into a parameter table in the DB.  
        ''' </summary>
        ''' <paramname="tableName"> - full SQL Server DB table name.</param>
        ''' <paramname="parmStruct"> - populated TpParm object.</param>
        ''' <paramname="parmNulls"> - ODBC nullInds associated with parmStruct.</param>
        ''' <returns></returns>
        Public Shared Function UtInsertParmRecord(tableName As String, parmStruct As TpParm, parmNulls As SQLLEN()) As Integer

            Dim parmHandle As Integer
            Dim rc As Integer

            ' insert TSIP parm record information 
            If CSharpImpl.__Assign(parmHandle, TpDynParm.TpSelectParm(tableName, "", "")) < 0 Then
                Return parmHandle
            End If

            rc = TpDynParm.TpInsertParm(parmHandle, parmStruct, parmNulls)


            TpDynParm.TpCloseParm(parmHandle)
            Return rc
        End Function

        ''' <summary>
        ''' This method constructs the operator code or call sign selection criteria 
        ''' from the specified codes; this can then be used as part of the 'WHERE' 
        ''' clause of an SQL query.  
        ''' </summary>
        ''' <paramname="parmStruct"> - TpParm object.</param>
        ''' <paramname="selection"> - part of the 'WHERE' clause of an SQL query </param>
        Public Shared Sub UtOpCodesCallSigns(parmStruct As TpParm, <Out> ByRef selection As String)
            '...Log2.v("\nTsipUtils.UtOpCodesCallSigns(): Entry");

            Dim codeList As String
            Dim oneSelection As String
            Dim aCode As String
            Dim rc As Integer
            Dim i As Integer

            selection = ""      '	Empty the string.

            codeList = parmStruct.codes

            If parmStruct.selsites.Equals("CALL SIGN") AndAlso Not parmStruct.envtype.Equals("MDB_ES") Then
                i = 0
                ' Initialize the GenUtil.UtGetInputString buffer and return first poke.
                rc = GenUtil.UtGetInputString(codeList, aCode, 9)
                While rc >= 0
                    If i = 0 Then
                        oneSelection = String.Format(" and (call1 = '{0}'", aCode.PadRight(9))
                        i += 1
                    Else
                        oneSelection = String.Format(" or call1 = '{0}'", aCode.PadRight(9))
                    End If
                    selection += oneSelection
                    ' Subsequent pokes from the GenUtil.UtGetInputString buffer.
                    rc = GenUtil.UtGetInputString(Nothing, aCode, 9)
                End While
                If i > 0 Then
                    selection += ")"
                End If
            End If

            If parmStruct.selsites.Equals("OPERATOR CODE") Then
                i = 0
                ' First poke; non-null first argument.
                rc = GenUtil.UtGetInputString(codeList, aCode, 6)
                While rc >= 0
                    If i = 0 Then
                        oneSelection = String.Format(" and (oper = '{0}'", aCode.PadRight(6))
                        i += 1
                    Else
                        oneSelection = String.Format(" or oper = '{0}'", aCode.PadRight(6))
                    End If
                    selection += oneSelection
                    ' Subsequent pokes; null first arguments.
                    rc = GenUtil.UtGetInputString(Nothing, aCode, 6)
                End While
                If i > 0 Then
                    selection += ")"
                End If
            End If

            '...Log2.v("\nTsipUtils.UtOpCodesCallSigns(): Exit: selection = " + selection);
        End Sub

        ''' <summary>
        ''' This determines whether the interferer's band, or its adjacent bands, are 
        ''' used by the victim.  
        ''' </summary>
        ''' <paramname="intBand"> - interferer's band code ID.</param>
        ''' <paramname="vicBand"> - victim's band code ID.</param>
        ''' <returns></returns>
        Public Shared Function UtAnteAdjBands(intBand As String, vicBand As String) As Integer
            Dim adjBands As String
            Dim curBand As SdBand
            Dim cIntBand As String
            Dim cVicBand As String

            cIntBand = intBand.Trim()
            cVicBand = vicBand.Trim()

            ' check to see if its the same band 
            If cIntBand.Equals(cVicBand) Then
                Return Constant.SUCCESS
            End If

            ' get the interferer's adjacent bands 
            If TpRunTsip.TsipUtils.UtGetBand(cIntBand, curBand) <> Constant.SUCCESS Then
                ' no such band code exists 
                ErrMsg.UtPrintMessage([Error].INVALIDBANDCODE, intBand)
                Return Constant.FAILURE
            End If
            adjBands = curBand.badj

            ' check each of the interferer's adjacent bands 
            Dim tokens = adjBands.Split(Constant.ADJ_BAND_DELIM_STR(0))

            For Each token In tokens
                If token.Trim().Equals(cVicBand) Then
                    Return Constant.SUCCESS
                End If
            Next

            Return Constant.FAILURE
        End Function

        ''' <summary>
        ''' This method returns an SdBand object populated with column values from the DB table
        ''' "main.sd_band" for the record with a prescribed bndcde. The method returns 0 if the
        ''' record was found; a non-zero return value indicates failure.  
        ''' </summary>
        ''' <remarks>
        ''' This method simply makes a pass-through call to Suutils.SdGetBand(). Consequently, 
        ''' the need for this method in TsipUtils could be eliminated.
        ''' </remarks>
        ''' <paramname="bndCde"> - prescribed bndcde.</param>
        ''' <paramname="curBand"> - populated SdBand object.</param>
        ''' <returns></returns>
        Public Shared Function UtGetBand(bndCde As String, <Out> ByRef curBand As SdBand) As Integer
            Return Suutils.SdGetBand(bndCde, curBand)
        End Function

        ''' <summary>
        ''' Creates a populated SdBand object that corresponds to a precribed band bit.
        ''' </summary>
        ''' <remarks>
        ''' This method simply makes a pass-through call to Suutils.SdGetBandfromBit(). Consequently, 
        ''' the need for this method in TsipUtils could be eliminated.
        ''' </remarks>
        ''' <paramname="bndBitPos"> - prescribed bit position.</param>
        ''' <paramname="curBand"> - populated SdBand object.</param>
        ''' <returns></returns>
        ''' <para> - Constant.SUCCESS - the method succeeded.</para>
        ''' <para> - Any other value - the attempt failed.</para>
        Public Shared Function UtGetBandBit(bndBitPos As Short, <Out> ByRef curBand As SdBand) As Integer
            Return Suutils.SdGetBandfromBit(bndBitPos, curBand)
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
