Imports _Configuration
Imports _DataStructures
Imports System
Imports _NewLib
Imports _Utillib
Imports SQLCHARPTR = System.String            'Invented to mimic (char *) for [In]  only.
Imports SQLLEN = System.Int64
Imports System.Runtime.InteropServices

Namespace TpRunTsip

    ''' <summary>
    ''' Provides methods to generate the channels for a plan.
    ''' </summary>
    Public Class TpPlanChan
        Private Shared mPolarity As String() = New String(3) {"VVHH", "HHVV", "VHHV", "HVVH"}

        Private Shared mCall1 As String
        Private Shared mCall2 As String
        Private Shared mBndcde As String
        Private Shared mFreq As Single() = New Single(49) {}

        '-----------------------------------------------------------------------

        ''' <summary>
        ''' This is the principal method for the generation of channels; its
        ''' function is to oversee the generation of channel records for PLAN mode
        ''' analysis.
        ''' </summary>
        ''' <remarks>
        ''' <listtype="bullet">
        ''' <item>The first step is to copy the PDF over to a temporary PDF.</item>
        ''' <item>For convenience of testing, we also ensure that a title record is
        ''' included when the temporary PDF is created.</item>
        ''' <item>Two channel cursors are opened, one for the old PDF and one for the new.</item> 
        ''' <item>For each channel record in the old PDF, we check if we are dealing with a new
        ''' call1-call2 pair, and reset the chid (which becomes the seed for the
        ''' new channel ids, which are always preceded by 'pl').</item> 
        ''' <item>After checking that hl is valid, we intialise the structure which serves as the
        ''' basis for new channels, and then add those channels (at least, those
        ''' which are valid) to the temporary PDF.</item> 
        ''' <item>If any of this fails, the method returns an error condition.</item>
        ''' </list>
        ''' </remarks>
        ''' <paramname="tpParm"> - prescribed TpParm object.</param>
        ''' <returns></returns>
        ''' <para> - Constant.SUCCESS:	method exited normally.</para>
        ''' <para> - Constant.FAILURE:	invalid fields?</para>
        Public Shared Function GenChan(tpParm As TpParm) As Integer
            ' Local variables 
            Dim newChan As NewChan()  ' [Constant.MAXPLANPTS] 99 plans max 			
            Dim row As Integer                                            ' counter for # channels 							
            Dim call1, call2 As String ' present call1 and call2
            Dim bndcde As String              ' band code 														
            Dim fwName As String          ' source PDF name											
            Dim tempName As String   ' temporary PDF name										
            Dim intTableNm As String ' full table names for channel tables	
            Dim ftChan As FtChan                ' space for Channel record 						
            Dim chanNullInd As SQLLEN() ' [FtChan.NUM_COLUMNS] null indicators for channel 	
            Dim chanHandle As Integer                             ' Handle for source channel table			
            Dim newChanHandle As Integer                      ' Handle for temporary channel table		
            Dim chid = 0                                           ' integer portion of plxx chids				
            Dim rc As Integer

            fwName = tpParm.proname.Trim()
            tempName = SQLCHARPTR.Format("pl_{0,13}", tpParm.proname)
            tpParm.proname = tempName


            Ssutil.UtDropTable(Constant.FT, tempName)

            ' COPY THE PDF TO THE NEW PDF HERE 
            If CSharpImpl.__Assign(rc, Ssutil.UtCopyTable(Constant.FT, fwName, tempName, False)) <> Constant.SUCCESS Then
                ErrMsg.UtPrintMessage(rc)
                Return rc
            End If


            Ssutil.CreateNewTitleRec(Constant.FT_TITL, tempName)

            ' 
            '  Now we open two cursors, one for the new PDF, one for the old. The
            '  order in which these are opened is important, since the value of
            '  intTableNm is used later in the call to addChannels, and must
            '  contain the name of the old PDF.
            ' 
            ' Convert temp PDF name to internal Channel table name 
            GenUtil.UtCvtName(Constant.FT_CHAN, tempName, intTableNm)

            newChanHandle = DynChannel.FtSelectChannel(intTableNm, Nothing, "call1, call2, bndcde, chid")
            If newChanHandle < 0 Then
                ErrMsg.UtPrintMessage(newChanHandle)
                Return newChanHandle
            End If

            ' Convert PDF name to internal Channel table name 
            GenUtil.UtCvtName(Constant.FT_CHAN, fwName, intTableNm)
            chanHandle = DynChannel.FtSelectChannel(intTableNm, Nothing, "call1, call2, bndcde, chid")
            If chanHandle < 0 Then
                ErrMsg.UtPrintMessage(chanHandle)
                Return chanHandle
            End If

            ' Get the channel record from the screen 

            ' 
            '  Initialise the static variables in validateFreq, and the variables
            '  used to determine if a new call1/call2/bandcode set is being used.
            ' 
            TpRunTsip.TpPlanChan.ValidateFreq(Nothing, Nothing, 0, 0)
            call1 = ""
            call2 = ""
            bndcde = ""

            ' START CHANNEL CURSOR LOOP HERE  
            While DynChannel.FtFetchChannel(chanHandle, ftChan, chanNullInd) = Constant.SUCCESS
                If Not call1.Equals(ftChan.call1) OrElse Not call2.Equals(ftChan.call2) OrElse Not bndcde.Equals(ftChan.bndcde) Then
                    call1 = ftChan.call1
                    call2 = ftChan.call2
                    bndcde = ftChan.bndcde
                    chid = 1
                End If

                If ftChan.hl <= 0 OrElse ftChan.hl > 6 Then
                    ' Invalid or unknown Hl code 
                    ErrMsg.UtPrintMessage([Error].INVHILO)
                    DynChannel.FtCloseChannel(chanHandle)
                    DynChannel.FtCloseChannel(newChanHandle)
                    chanHandle = Constant.DB_NULL
                    Return Constant.FAILURE
                End If


                ' 
                '  Create an array of details for generated channels, which will be
                '  combined with the details from the existing channels in addChannels
                '  to create full channel records in the temporary PDF
                ' 
                If TpRunTsip.TpPlanChan.InitialiseNewChan(ftChan.bndcde, ftChan.splan, ftChan.hl, ftChan.vh, row, newChan) <> Constant.SUCCESS Then
                    ErrMsg.UtPrintMessage([Error].INVBAND)
                    DynChannel.FtCloseChannel(chanHandle)
                    DynChannel.FtCloseChannel(newChanHandle)
                    chanHandle = Constant.DB_NULL
                    Return Constant.FAILURE
                End If

                If TpRunTsip.TpPlanChan.AddChannels(intTableNm, newChanHandle, ftChan, chanNullInd, newChan, row, chid) = Constant.FAILURE Then
                    DynChannel.FtCloseChannel(chanHandle)
                    DynChannel.FtCloseChannel(newChanHandle)
                    Return Constant.FAILURE
                End If
            End While

            DynChannel.FtCloseChannel(chanHandle)
            DynChannel.FtCloseChannel(newChanHandle)

            '...Log2.v("\nTpPlanChan.GetChan(): succeeded");
            Return Constant.SUCCESS
        End Function

        ''' <summary>
        ''' This method uses the current bndcde & plan to calculate the channel 
        ''' records with this plan. These values can then be entered into an internal table 
        ''' that will be the basis for the new channels.  
        ''' </summary>
        ''' <paramname="bndcde"> - band code.</param>
        ''' <paramname="pplan"> - plan name.</param>
        ''' <paramname="hl"> - hilo case indicator.</param>
        ''' <paramname="vh"> - polarization case indicator.</param>
        ''' <paramname="row"> - row indicator for newChan struct</param>
        ''' <paramname="newChan"> - initialized NewChan object.</param>
        ''' <returns></returns>
        Public Shared Function InitialiseNewChan(bndcde As String, pplan As String, hl As Short, vh As Short, <Out> ByRef row As Integer, <Out> ByRef newChan As NewChan()) As Integer      ' band code 
            ' plan name 
            ' hilo 
            ' row indicator for newChan struct 
            ' newChan Struct 
            ' out
            row = 0
            newChan = Arrays.CreateArrayUsingDefaultElementConstructor(Of NewChan)(Constant.MAXPLANPTS)

            ' Local variables 
            Dim i, j As Integer              ' loop counters 
            Dim sdPlan As SdPlan  ' Space for BAND/PLAN row 
            Dim sdPlndArray As SdPlnd()
            Dim nNumData As Integer

            Dim nRet As Integer

            ' First initialise the internal structure 
            ' ps = NewChan;


            For i = 0 To newChan.Length - 1
                newChan(i).rxInd = Constant.DB_NULL
                newChan(i).txInd = Constant.DB_NULL
                newChan(i).chidInd = Constant.DB_NULL
            Next

            nRet = Suutils.SdGetPlan(pplan, bndcde, sdPlan, sdPlndArray, nNumData)

            ' Now initialise the row variable
            row = 0

            ' The following loop will initialise the internal newChan structure
            ' with all of the automatically generated channels. The channels
            ' rx & tx frequencies come from the plan.  After each set of
            ' frequencies is extracted bump the newChan struct to the next entry.
            i = 0
            j = 0

            While i < nNumData
                Dim plan = sdPlndArray(i)

                Select Case hl
                    Case 1
                        newChan(j).txfreq = CDbl(plan.set1)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 1, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set2)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 2, newChan(j).polrx)

                    Case 2
                        newChan(j).txfreq = CDbl(plan.set2)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 2, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set1)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 1, newChan(j).polrx)

                    Case 3
                        newChan(j).txfreq = CDbl(plan.set3)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 3, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set4)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 4, newChan(j).polrx)

                    Case 4
                        newChan(j).txfreq = CDbl(plan.set4)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 4, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set3)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 3, newChan(j).polrx)

                    Case 5
                        newChan(j).txfreq = CDbl(plan.set1)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 1, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set2)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 2, newChan(j).polrx)

                        j += 1
                        row += 1

                        newChan(j).txfreq = CDbl(plan.set3)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 3, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set4)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 4, newChan(j).polrx)

                    Case 6
                        newChan(j).txfreq = CDbl(plan.set2)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 2, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set1)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 1, newChan(j).polrx)

                        j += 1
                        row += 1

                        newChan(j).txfreq = CDbl(plan.set4)
                        newChan(j).txInd = Constant.DB_NOT_NULL
                        newChan(j).txstatus = "1"
                        ' Set the TX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 4, newChan(j).poltx)

                        newChan(j).rxfreq = CDbl(plan.set3)
                        newChan(j).rxInd = Constant.DB_NOT_NULL
                        newChan(j).rxstatus = "1"
                        ' Set the RX polarization 
                        TpRunTsip.TpPlanChan.GetPol(vh, 3, newChan(j).polrx)
                    Case Else
                End Select

                j += 1
                row += 1
                i += 1

            End While

            Return 0
        End Function

        ''' <summary>
        ''' This method determines the channel's polarization for the prescribed vh and SET indicators.
        ''' (See page B-72 of TSIP reference Guide.) 
        ''' </summary>
        ''' <paramname="vh"> - polarization code indicator.</param>
        ''' <paramname="set"> - selects the character position within a 4-char polarization code.</param>
        ''' <paramname="pol"></param>
        Public Shared Sub GetPol(vh As Short, [set] As Integer, <Out> ByRef pol As String)
            ' vh  = 1, 2, 3, or 4
            ' set = 1, 2, 3, or 4.
            If vh <= 0 OrElse vh > 4 OrElse [set] <= 0 OrElse [set] > 4 Then
                ' Invalid combination. Should be impossible to get here! 
                pol = " "
            Else
                ' mPolarity[4] = { "VVHH", "HHVV", "VHHV", "HVVH" }
                pol = TpRunTsip.TpPlanChan.mPolarity(vh - 1).Substring([set] - 1, 1)
            End If
        End Sub

        ''' <summary>
        ''' This method checks whether a given set of frequencies have already 
        ''' been used as TX or RX in a channel record for this {call1, call2, band code} 
        ''' in this PDF or in the original channels of the old PDF.  
        ''' </summary>
        ''' <paramname="fwName"> - name of PDF.</param>
        ''' <paramname="chan"> - FtChan object.</param>
        ''' <paramname="freqtx"> - 2 band plan transmit frequency.</param>
        ''' <paramname="freqrx"> - 2 band plan receive frequency.</param>
        ''' <returns></returns>
        Public Shared Function ValidateFreq(fwName As String, chan As FtChan, freqtx As Double, freqrx As Double) As Integer    ' PDF name 
            ' chan structure 
            ' 2 band plan frequencies 
            Dim searchClause As String

            Dim i As Integer
            Dim ret = Constant.SUCCESS

            If SQLCHARPTR.IsNullOrWhiteSpace(fwName) Then
                TpRunTsip.TpPlanChan.mCall1 = ""
                TpRunTsip.TpPlanChan.mCall2 = ""
                TpRunTsip.TpPlanChan.mBndcde = ""

                For i = 0 To 49
                    TpRunTsip.TpPlanChan.mFreq(i) = 0.0F
                Next

                Return Constant.SUCCESS
            End If

            If Not TpRunTsip.TpPlanChan.mCall1.Equals(chan.call1) OrElse Not TpRunTsip.TpPlanChan.mCall2.Equals(chan.call2) OrElse Not TpRunTsip.TpPlanChan.mBndcde.Equals(chan.bndcde) Then
                TpRunTsip.TpPlanChan.mCall1 = chan.call1
                TpRunTsip.TpPlanChan.mCall2 = chan.call2
                TpRunTsip.TpPlanChan.mBndcde = chan.bndcde
                i = 0
            Else
                i = 0

                While i < 50 AndAlso TpRunTsip.TpPlanChan.mFreq(i) > 0.0
                    If freqtx = TpRunTsip.TpPlanChan.mFreq(i) OrElse freqrx = TpRunTsip.TpPlanChan.mFreq(i) Then
                        Return [Error].FREQUENCYUSED
                    End If

                    i += 1
                End While
            End If

            If freqtx > 0.0 Then TpRunTsip.TpPlanChan.mFreq(Math.Min(Threading.Interlocked.Increment(i), i - 1)) = freqtx
            If freqrx > 0.0 Then TpRunTsip.TpPlanChan.mFreq(Math.Min(Threading.Interlocked.Increment(i), i - 1)) = freqrx
            TpRunTsip.TpPlanChan.mFreq(Math.Min(Threading.Interlocked.Increment(i), i - 1)) = 0.0F

            ' set up search clause to search PDF 

            searchClause = SQLCHARPTR.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (freqtx = {3} or freqtx = {4} or freqrx = {5} or freqrx = {6})", chan.call1, chan.call2, chan.bndcde, freqtx, freqrx, freqtx, freqrx)

            ret = Ssutil.DbCountRows(fwName, searchClause)

            Return ret
        End Function

        ''' <summary>
        ''' This method adds the channels to the temporary PDF. 
        ''' </summary>
        ''' <remarks>
        ''' <listtype="bullet">
        ''' <item>In order for TX 
        ''' channels to be added, the txpow field must be defined.</item> 
        ''' <item>For RX channels, the 
        ''' antnumbrx1 must exist.</item> 
        ''' <item>Finally, validateFreq checks that the frequency has 
        ''' not been used already, and that is does not already exist in the original 
        ''' channels. </item>
        ''' <item>If all of these tests are passed, the channel information from 
        ''' the generated channel info is combined with the original channel data to 
        ''' create new records which are added to the channel table of the temporary 
        ''' PDF.</item>
        ''' </list>
        ''' </remarks>
        ''' <paramname="pdfName"> - name of PDF.</param>
        ''' <paramname="chanHandle"> - handle (index) of a cursor previously returned by a call to DynChannel.FtSelectChannel().</param>
        ''' <paramname="chan"> - FtChan object.</param>
        ''' <paramname="nullsChan"> - ODBC nullInds associated with chan.</param>
        ''' <paramname="newChans"> - array of NewChan objects.</param>
        ''' <paramname="rows"> - number of array elements in newChans.</param>
        ''' <paramname="chid"> - channel ID.</param>
        ''' <returns>Constant.SUCCESS or Constant.FAILURE.</returns>
        Public Shared Function AddChannels(pdfName As String, chanHandle As Integer, chan As FtChan, nullsChan As SQLLEN(), newChans As NewChan(), rows As Integer, ByRef chid As Integer) As Integer                    ' channel structure 
            ' newChan structure
            ' rows indicator 
            Dim recCount As Integer
            Dim i, rc As Integer

            recCount = 0

            For i = 0 To rows
                Dim newChanStruct = newChans(i)
                'AH: CHECK THIS HERE!
                If (newChanStruct.rxInd <> Constant.DB_NULL AndAlso nullsChan(FtChan.ANTNUMBRX1) <> Constant.DB_NULL OrElse newChanStruct.txInd <> Constant.DB_NULL AndAlso nullsChan(FtChan.PWRTX) <> Constant.DB_NULL) AndAlso TpRunTsip.TpPlanChan.ValidateFreq(pdfName, chan, newChanStruct.txfreq, newChanStruct.rxfreq) = 0 Then
                    ' extract the information from the structure 

                    chan.chid = SQLCHARPTR.Format("pl{0:00}", chid) ' "%02d" format = zero fill, minimum of 2 digits, integer
                    chid += 1
                    newChanStruct.chidInd = Constant.DB_NOT_NULL
                    If newChanStruct.txInd <> Constant.DB_NULL AndAlso nullsChan(FtChan.PWRTX) <> Constant.DB_NULL Then
                        ' Tx part of channel is defined 
                        chan.freqtx = newChanStruct.txfreq
                        nullsChan(FtChan.FREQTX) = Constant.DB_NOT_NULL
                        chan.stattx = newChanStruct.txstatus
                        nullsChan(FtChan.STATTX) = Constant.DB_NOT_NULL
                        chan.poltx = newChanStruct.poltx
                        nullsChan(FtChan.POLTX) = Constant.DB_NOT_NULL
                    Else
                        ' Come here if RX only channel 
                        nullsChan(FtChan.FREQTX) = Constant.DB_NULL
                        nullsChan(FtChan.STATTX) = Constant.DB_NULL
                        nullsChan(FtChan.POLTX) = Constant.DB_NULL
                    End If

                    If newChanStruct.rxInd <> Constant.DB_NULL AndAlso nullsChan(FtChan.ANTNUMBRX1) <> Constant.DB_NULL Then
                        ' Rx part of channel is defined 
                        chan.freqrx = newChanStruct.rxfreq
                        nullsChan(FtChan.FREQRX) = Constant.DB_NOT_NULL
                        chan.statrx = newChanStruct.rxstatus
                        nullsChan(FtChan.STATRX) = Constant.DB_NOT_NULL
                        chan.polrx = newChanStruct.polrx
                        nullsChan(FtChan.POLRX) = Constant.DB_NOT_NULL
                    Else
                        ' Come here if TX only channel 
                        nullsChan(FtChan.FREQRX) = Constant.DB_NULL
                        nullsChan(FtChan.STATRX) = Constant.DB_NULL
                        nullsChan(FtChan.POLRX) = Constant.DB_NULL
                    End If

                    rc = DynChannel.FtInsertChannel(chanHandle, chan, nullsChan)
                    If rc = Constant.SUCCESS Then
                        ' Insert worked ok 
                        recCount += 1     ' bump records added 
                    Else
                        ' Insert failed 
                        ErrMsg.UtPrintMessage(rc)
                        Return Constant.FAILURE
                    End If
                End If

            Next ' for (i = 0; i <= rows; i++)

            Return recCount
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
