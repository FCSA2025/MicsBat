# Documented File: TeVects.cs
**Repository Path:** `TpRunTsip\TeVects.cs`
**Primary Layer:** `TpRunTsip`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static _NewLib.Maths;

namespace TpRunTsip
{
    /// <summary>
    /// Provides methods that perform basic vector arithmetic in
    /// a 3-dimensional Euclidean space.
    /// </summary>

    public class TeVects
    {
        /// <summary>
        /// This method constructs an SEZ vector given its length and angles.  
        /// </summary>
        /// <param name="length"> - length (magnitude) of the vector.</param>
        /// <param name="elev"> - vector's elevation.</param>
        /// <param name="azim"> - vector's azimuth</param>
        /// <param name="vector"> - resulting vector as a double[3].</param>
        public static void TeBuildVector(double length,    /* input  - scale factor of the vector*/
                       double elev,             /* input  - vector elevation	*/
                       double azim,             /* input  - vector azimuth	*/
                       out double[] vector)     /* output - resulting representation */
        {
            vector = new double[3];

            vector[0] = length * -1.0 * CosD(elev) * CosD(azim);
            vector[1] = length * CosD(elev) * SinD(azim);
            vector[2] = length * SinD(elev);
        }

        /// <summary>
        /// This method returns (vector1) - (vector2).  
        /// </summary>
        /// <param name="vector1"> - vector to subtract from.</param>
        /// <param name="vector2"> - vector to subtract.</param>
        /// <param name="resVector"> - resulting vector.</param>
        public static void TeVectorSub(double[] vector1,      /* input  - vector to subtract from */
                     double[] vector2,      /* input  - vector to subtract */
                     out double[] resVector)    /* output - resulting vector */
        {
            resVector = new double[3];

            resVector[0] = vector1[0] - vector2[0];
            resVector[1] = vector1[1] - vector2[1];
            resVector[2] = vector1[2] - vector2[2];
        }

        /// <summary>
        /// This method computes the length (magnitude) of a given vector.  
        /// </summary>
        /// <param name="inVector"> - input vector.</param>
        /// <param name="length"> - length of vector.</param>
        public static void TeVectorLen(double[] inVector, /* input  - input vector */
                     out double length)    /* output - length of vector */
        {
            double sqrd;

            TeVectorLinMux(inVector, inVector, out sqrd);

            length = Sqrt(sqrd);
        }

        /// <summary>
        /// This method computes the scalar (dot) product of two vectors.  
        /// </summary>
        /// <param name="vector1"> - eponym.</param>
        /// <param name="vector2"> - eponym</param>
        /// <param name="result"> - resulting scalar dot product</param>
        public static void TeVectorLinMux(double[] vector1,   /* input  - vector 1 to be multiplied */
                        double[] vector2,   /* input  - vector 2 to be multiplied */
                        out double result) /* output - resulting dot product */
        {
            double[] tmpVec = new double[3];

            tmpVec[0] = vector1[0] * vector2[0];
            tmpVec[1] = vector1[1] * vector2[1];
            tmpVec[2] = vector1[2] * vector2[2];

            TeVectorLinAdd(tmpVec, out result);
        }

        /// <summary>
        /// This method computes the scalar sum of the elements of the vector.  
        /// </summary>
        /// <param name="inVector"> - input vector.</param>
        /// <param name="result"> - scalar sum of the vector's elements.</param>
        public static void TeVectorLinAdd(double[] inVector,  /* input  - input vector */
                        out double result) /* output - sum of vec's elements */
        {
            result = inVector[0] + inVector[1] + inVector[2];
        }

        /// <summary>
        /// This method returns the unit vector of the given vector.  
        /// </summary>
        /// <param name="inVector"> - input vector.</param>
        /// <param name="unitVector"> - a vector having the same direction cosines
        /// as the input veotor but having unit length (magnitude). </param>
        public static void TeVectorUnit(double[] inVector,    /* input  - input vector */
              out double[] unitVector)  /* output - unit represntn of vector */
        {
            // 'out' requirement.
            unitVector = new double[3];
            double length;

            TeVectorLen(inVector, out length);

            unitVector[0] = inVector[0] / length;
            unitVector[1] = inVector[1] / length;
            unitVector[2] = inVector[2] / length;
        }

        /// <summary>
        /// Calculates the multiplication of a vector by a scalar.  
        /// </summary>
        /// <param name="scalar"> - scalar to multiply by.</param>
        /// <param name="inVector"> - vector to be multiplied.</param>
        /// <param name="outVector"> - resulting vector.</param>
        public static void TeVectorSclMux(double scalar,  /* input  - scalar to multiply by */
                        double[] inVector,  /* input  - vector to be multiplied */
                        out double[] outVector) /* output - resulting vector */
        {
            // 'out' requirement.
            outVector = new double[3];

            outVector[0] = scalar * inVector[0];
            outVector[1] = scalar * inVector[1];
            outVector[2] = scalar * inVector[2];
        }

        /// <summary>
        /// This method returns the angle (in degrees) subtended between two vectors.
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static double Vangle(double[] vec1, double[] vec2)
        {
            double dDot;
            double dVec1Len;
            double dVec2Len;

            // cos(angle) = (dot product) / (length1 * length2).  

            TeVectorLinMux(vec1, vec2, out dDot);
            TeVectorLen(vec1, out dVec1Len);
            TeVectorLen(vec2, out dVec2Len);

            return (AcosD(dDot / (dVec1Len * dVec2Len)));
        }

    }
}

```
