using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates a point in 3-D cartesian space and provides
    /// static and instance-specific methods for vector addition, subtraction,
    /// modulus (length) and scalar dot product etc.
    /// </summary>
    public class Vector3D
    {
        private double mX = double.MinValue;
        private double mY = double.MinValue;
        private double mZ = double.MinValue;

        public double X { get { return mX; } set { mX = value; } }
        public double Y { get { return mY; } set { mY = value; } }
        public double Z { get { return mZ; } set { mZ = value; } }

        private Vector3D() { }

        /// <summary>
        /// This method is the fundamental constructor.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3D(double X, double y, double z)
        {
            mX = X;
            mY = y;
            mZ = z;
        }

        /// <summary>
        /// This method returns the vector v1 + v2.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3D Add(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.mX + v2.mX, v1.mY + v2.mY, v1.mZ + v2.mZ);
        }

                /// <summary>
        /// This method returns the vector v1 - v2.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3D Sub(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.mX - v2.mX, v1.mY - v2.mY, v1.mZ - v2.mZ);
        }

        /// <summary>
        /// This method returns the vector from P1 to P2 = v2 - v1.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3D FromTo(Vector3D v1, Vector3D v2)
        {
            return Sub(v2, v1);
        }

        /// <summary>
        /// This method returns the square of the modulus (length) of a vector.
        /// </summary>
        /// <returns></returns>
        public double ModSqrd()
        {
            return mX * mX + mY * mY + mZ * mZ;
        }

        /// <summary>
        /// This method returns the modulus (length) of a vector.
        /// </summary>
        /// <returns></returns>
        public double Mod()
        {
            double modSqrd = mX * mX + mY * mY + mZ * mZ;

            return modSqrd > double.Epsilon ? Math.Sqrt(modSqrd) : 0.0;
        }

        /// <summary>
        /// This method returns the scalar 'dot' product of two vectors.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double DotProd(Vector3D v1, Vector3D v2)
        {
            return v1.mX * v2.mX + v1.mY * v2.mY + v1.mZ * v2.mZ;
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
        public static double R(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            return DotProd(FromTo(v1, v2), FromTo(v1, v0)) / FromTo(v1, v2).ModSqrd();
        }

        /// <summary>
        /// This method provides the value of a Vector3D object as a formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", mX, mY, mZ);
        }

        /// <summary>
        /// This method runs several simple tests and writes the results to stdout.
        /// </summary>
        public static void RunTest()
        {
            Vector3D v0 = new Vector3D(0, 0, 0);
            Vector3D v1 = new Vector3D(1, 0, 0);
            Vector3D v2 = new Vector3D(3, 0, 0);
            Vector3D v3 = new Vector3D(1, 2, 3);
            Vector3D v4 = new Vector3D(4, 5, 6);
            Vector3D v5 = new Vector3D(7, 8, 9);
            Vector3D v6 = new Vector3D(8, 0, 0);
            Vector3D v7 = new Vector3D(5.5, 6.5, 7.5);

            Console.Write("\n{0} : Mod = {1} : ModSqrd = {2}", v3, v3.Mod(), v3.ModSqrd());
            Console.Write("\nDotProd({0}, {1}) = {2}", v3, v4, DotProd(v3, v4));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v4, v4, v5, R(v4, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v5, v4, v5, R(v5, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v7, v4, v5, R(v7, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v3, v4, v5, R(v3, v4, v5));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v0, v1, v2, R(v0, v1, v2));
            Console.Write("\nv0 = {0}, v1 = {1}, v2 = {2}, R = {3}", v6, v1, v2, R(v6, v1, v2));

        }


    }
}
