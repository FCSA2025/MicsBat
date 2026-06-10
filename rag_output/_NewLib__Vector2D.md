# Documented File: Vector2D.cs
**Repository Path:** `_NewLib\Vector2D.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates a point in 2-D cartesian space and provides
    /// static and instance-specific methods for vector addition, subtraction,
    /// modulus (length) and scalar dot product etc.
    /// </summary>
    public class Vector2D
    {
        private double mX = double.MinValue;
        private double mY = double.MinValue;

        public double X { get { return mX; } set { mX = value; } }
        public double Y { get { return mY; } set { mY = value; } }

        /// <summary>
        /// The default constructor of object of this class.
        /// </summary>
        public Vector2D() 
        { }

        /// <summary>
        /// Constructs a Vector2D object for a prescribed (X, Y) point.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="y"></param>
        public Vector2D(double X, double y)
        {
            mX = X;
            mY = y;
        }

        /// <summary>
        /// This method returns the vector v1 + v2.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector2D Add(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.mX + v2.mX, v1.mY + v2.mY);
        }

        /// <summary>
        /// This method returns the vector v1 - v2.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector2D Sub(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.mX - v2.mX, v1.mY - v2.mY);
        }

        /// <summary>
        /// This method returns a vector multiplied by a scalar.
        /// </summary>
        /// <param name="scalar"> the prescribed scalar value.</param>
        /// <param name="v1"> the prescribed vector.</param>
        /// <returns></returns>
        public static Vector2D ScalarMul(double scalar, Vector2D v1)
        {
            return new Vector2D(v1.mX * scalar, v1.mY * scalar);
        }

        /// <summary>
        /// This method returns the vector from P1 to P2 = v2 - v1.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector2D FromTo(Vector2D v1, Vector2D v2)
        {
            return Sub(v2, v1);
        }

        /// <summary>
        /// This method returns the square of the modulus (length) of a vector.
        /// </summary>
        /// <returns></returns>
        public double ModSqrd()
        {
            return mX * mX + mY * mY;
        }

        /// <summary>
        /// This method returns the modulus (length) of a vector.
        /// </summary>
        /// <returns></returns>
        public double Mod()
        {
            double modSqrd = mX * mX + mY * mY;

            return modSqrd > double.Epsilon ? Math.Sqrt(modSqrd) : 0.0;
        }

        /// <summary>
        /// This method returns the scalar 'dot' product of two vectors.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double DotProd(Vector2D v1, Vector2D v2)
        {
            return v1.mX * v2.mX + v1.mY * v2.mY;
        }

        /// <summary>
        /// This method returns the parameterized relative position, R, of the 
        /// foot of the perpendicular (FOTP) from point P0 to the line from P1 to P2 
        /// (extended to infinity); if R = 0 the FOTP is at point P1; if R = 1 the 
        /// FOTP is at point P2; if 0 < R < 1 the FOTP lies between P1 and P2; if
        /// R < 0 then the FOTP lies on the extension of the line closest to P1; if
        /// R > 1 then the FOTP lies on the extension of the line closest to P2.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double R(Vector2D v0, Vector2D v1, Vector2D v2)
        {
            return DotProd(FromTo(v1, v2), FromTo(v1, v0)) / FromTo(v1, v2).ModSqrd();
        }

        /// <summary>
        /// This method returns the result of a FOTP analysis comprising the Rvalue,
        /// the vector position of the foot of the perpendicular from v0 to the line
        /// v2 - v1, and the perpendicular distance from v0 to the FOTP point.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="Rvalue"></param>
        /// <param name="fotp"></param>
        /// <param name="perpendicularDistance"></param>
        public static void FOTP(Vector2D v0, Vector2D v1, Vector2D v2, out double Rvalue, out Vector2D fotp, out double perpendicularDistance)
        {
            Rvalue = R(v0, v1, v2);

            fotp = Add(v1, ScalarMul(Rvalue, FromTo(v1, v2)));

            perpendicularDistance = Math.Sqrt(FromTo(v0, v1).ModSqrd() - (Rvalue * Rvalue* FromTo(v1, v2).ModSqrd()));

            return ;
        }

        /// <summary>
        /// This method provides the value of a Vector2D object as a formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1})", mX, mY);
        }

        /// <summary>
        /// This method runs several simple tests and writes the results to stdout.
        /// </summary>
        public static void RunTest()
        {
            Vector2D v0 = new Vector2D(0, 0);
            Vector2D v1 = new Vector2D(1, 0);
            Vector2D v2 = new Vector2D(3, 0);
            Vector2D v3 = new Vector2D(1, 2);
            Vector2D v4 = new Vector2D(4, 5);
            Vector2D v5 = new Vector2D(7, 8);
            Vector2D v6 = new Vector2D(8, 0);
            Vector2D v7 = new Vector2D(5.5, 6.5);

            Console.Write("\n{0} : Mod = {1} : ModSqrd = {2}", v3, v3.Mod(), v3.ModSqrd());
            Console.Write("\nDotProd({0}, {1}) = {2}", v3, v4, DotProd(v3, v4));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v4, v4, v5, R(v4, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v5, v4, v5, R(v5, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v7, v4, v5, R(v7, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v3, v4, v5, R(v3, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v0, v1, v2, R(v0, v1, v2));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v6, v1, v2, R(v6, v1, v2));

            v0 = new Vector2D(1, 0);
            v1 = new Vector2D(0, 2);
            v2 = new Vector2D(2, 2);
            double Rvalue;
            Vector2D fotp;
            double perpendicularDistance;
            FOTP(v0, v1, v2, out Rvalue, out fotp, out perpendicularDistance);
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}, vp = {4}, perpDist = {5}", v0, v1, v2, Rvalue, fotp, perpendicularDistance);

            v0 = new Vector2D(1, 0);
            v1 = new Vector2D(0, 2);
            v2 = new Vector2D(0, 3);
            FOTP(v0, v1, v2, out Rvalue, out fotp, out perpendicularDistance);
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}, vp = {4}, perpDist = {5}", v0, v1, v2, Rvalue, fotp, perpendicularDistance);

            v0 = new Vector2D(1, 0);
            v1 = new Vector2D(0, 0);
            v2 = new Vector2D(2, 2);
            FOTP(v0, v1, v2, out Rvalue, out fotp, out perpendicularDistance);
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}, vp = {4}, perpDist = {5}", v0, v1, v2, Rvalue, fotp, perpendicularDistance);
        }


    }
}

```
