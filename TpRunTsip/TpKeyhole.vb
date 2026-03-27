Imports System.Math

Namespace TpRunTsip
    ''' <summary>
    ''' This class has just one method, WithinRange(), that helps with
    ''' the 'keyhole' calculations.
    ''' </summary>
    Public Class TpKeyhole
        ''' <summary>
        ''' This method returns true if the absolute value of 'value' is less than 
        ''' or equal to a boundary threshold.  
        ''' </summary>
        ''' <paramname="value"> - value to be be tested.</param>
        ''' <paramname="boundary"> - threshold value.</param>
        ''' <returns></returns>
        Public Shared Function WithinRange(value As Double, boundary As Double) As Boolean
            Return Abs(value) <= boundary
        End Function


    End Class
End Namespace
