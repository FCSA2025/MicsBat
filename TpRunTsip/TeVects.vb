Imports System.Math
Imports _NewLib.Maths
Imports System.Runtime.InteropServices

Namespace TpRunTsip
    ''' <summary>
    ''' Provides methods that perform basic vector arithmetic in
    ''' a 3-dimensional Euclidean space.
    ''' </summary>

    Public Class TeVects
        ''' <summary>
        ''' This method constructs an SEZ vector given its length and angles.  
        ''' </summary>
        ''' <paramname="length"> - length (magnitude) of the vector.</param>
        ''' <paramname="elev"> - vector's elevation.</param>
        ''' <paramname="azim"> - vector's azimuth</param>
        ''' <paramname="vector"> - resulting vector as a double[3].</param>
        Public Shared Sub TeBuildVector(length As Double, elev As Double, azim As Double, <Out> ByRef vector As Double())    ' input  - scale factor of the vector
            ' input  - vector elevation	
            ' input  - vector azimuth	
            ' output - resulting representation 
            vector = New Double(2) {}

            vector(0) = length * -1.0 * CosD(elev) * CosD(azim)
            vector(1) = length * CosD(elev) * SinD(azim)
            vector(2) = length * SinD(elev)
        End Sub

        ''' <summary>
        ''' This method returns (vector1) - (vector2).  
        ''' </summary>
        ''' <paramname="vector1"> - vector to subtract from.</param>
        ''' <paramname="vector2"> - vector to subtract.</param>
        ''' <paramname="resVector"> - resulting vector.</param>
        Public Shared Sub TeVectorSub(vector1 As Double(), vector2 As Double(), <Out> ByRef resVector As Double())      ' input  - vector to subtract from 
            ' input  - vector to subtract 
            ' output - resulting vector 
            resVector = New Double(2) {}

            resVector(0) = vector1(0) - vector2(0)
            resVector(1) = vector1(1) - vector2(1)
            resVector(2) = vector1(2) - vector2(2)
        End Sub

        ''' <summary>
        ''' This method computes the length (magnitude) of a given vector.  
        ''' </summary>
        ''' <paramname="inVector"> - input vector.</param>
        ''' <paramname="length"> - length of vector.</param>
        Public Shared Sub TeVectorLen(inVector As Double(), <Out> ByRef length As Double) ' input  - input vector 
            ' output - length of vector 
            Dim sqrd As Double

            TpRunTsip.TeVects.TeVectorLinMux(inVector, inVector, sqrd)

            length = Sqrt(sqrd)
        End Sub

        ''' <summary>
        ''' This method computes the scalar (dot) product of two vectors.  
        ''' </summary>
        ''' <paramname="vector1"> - eponym.</param>
        ''' <paramname="vector2"> - eponym</param>
        ''' <paramname="result"> - resulting scalar dot product</param>
        Public Shared Sub TeVectorLinMux(vector1 As Double(), vector2 As Double(), <Out> ByRef result As Double)   ' input  - vector 1 to be multiplied 
            ' input  - vector 2 to be multiplied 
            ' output - resulting dot product 
            Dim tmpVec = New Double(2) {}

            tmpVec(0) = vector1(0) * vector2(0)
            tmpVec(1) = vector1(1) * vector2(1)
            tmpVec(2) = vector1(2) * vector2(2)

            TpRunTsip.TeVects.TeVectorLinAdd(tmpVec, result)
        End Sub

        ''' <summary>
        ''' This method computes the scalar sum of the elements of the vector.  
        ''' </summary>
        ''' <paramname="inVector"> - input vector.</param>
        ''' <paramname="result"> - scalar sum of the vector's elements.</param>
        Public Shared Sub TeVectorLinAdd(inVector As Double(), <Out> ByRef result As Double)  ' input  - input vector 
            ' output - sum of vec's elements 
            result = inVector(0) + inVector(1) + inVector(2)
        End Sub

        ''' <summary>
        ''' This method returns the unit vector of the given vector.  
        ''' </summary>
        ''' <paramname="inVector"> - input vector.</param>
        ''' <paramname="unitVector"> - a vector having the same direction cosines
        ''' as the input veotor but having unit length (magnitude). </param>
        Public Shared Sub TeVectorUnit(inVector As Double(), <Out> ByRef unitVector As Double())    ' input  - input vector 
            ' output - unit represntn of vector 
            ' 'out' requirement.
            unitVector = New Double(2) {}
            Dim length As Double

            TpRunTsip.TeVects.TeVectorLen(inVector, length)

            unitVector(0) = inVector(0) / length
            unitVector(1) = inVector(1) / length
            unitVector(2) = inVector(2) / length
        End Sub

        ''' <summary>
        ''' Calculates the multiplication of a vector by a scalar.  
        ''' </summary>
        ''' <paramname="scalar"> - scalar to multiply by.</param>
        ''' <paramname="inVector"> - vector to be multiplied.</param>
        ''' <paramname="outVector"> - resulting vector.</param>
        Public Shared Sub TeVectorSclMux(scalar As Double, inVector As Double(), <Out> ByRef outVector As Double())  ' input  - scalar to multiply by 
            ' input  - vector to be multiplied 
            ' output - resulting vector 
            ' 'out' requirement.
            outVector = New Double(2) {}

            outVector(0) = scalar * inVector(0)
            outVector(1) = scalar * inVector(1)
            outVector(2) = scalar * inVector(2)
        End Sub

        ''' <summary>
        ''' This method returns the angle (in degrees) subtended between two vectors.
        ''' </summary>
        ''' <paramname="vec1"></param>
        ''' <paramname="vec2"></param>
        ''' <returns></returns>
        Public Shared Function Vangle(vec1 As Double(), vec2 As Double()) As Double
            Dim dDot As Double
            Dim dVec1Len As Double
            Dim dVec2Len As Double

            ' cos(angle) = (dot product) / (length1 * length2).  

            TpRunTsip.TeVects.TeVectorLinMux(vec1, vec2, dDot)
            TpRunTsip.TeVects.TeVectorLen(vec1, dVec1Len)
            TpRunTsip.TeVects.TeVectorLen(vec2, dVec2Len)

            Return AcosD(dDot / (dVec1Len * dVec2Len))
        End Function

    End Class
End Namespace
