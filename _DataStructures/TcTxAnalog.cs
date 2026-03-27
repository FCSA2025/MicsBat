using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates a subset of the columns of the DB table <b>TSIP.ctxaeqpt</b>
    /// that stores analog equipment data. Objects of this class are used to pass
    /// analog equipment data to the CTX calculation methods.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TcTxAnalog
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ECODE_SZ)]
        public string ecode;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFCODE_SZ)]
        public string trafcode;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int numchan;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double fmin;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double fm;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double sigma;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double nf;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double iffreq;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nFilter; /*	Size of the filter array */

#if INTPTR
        public IntPtr aFiltFS; /*	Array of frequency separations in the filter */

        public IntPtr aFiltVal;    /*	Array of discriminations in the filter */
#else
        [MarshalAsAttribute(UnmanagedType.LPStruct)]  // Be carefull!
        public double[] aFiltFS; /*	Array of frequency separations in the filter */

        [MarshalAsAttribute(UnmanagedType.LPStruct)]  // Be carefull!
        public double[] aFiltVal;    /*	Array of discriminations in the filter */
#endif

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double sif;  /*	IF response factor */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double irf;  /*	Image response Factor */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double ai70; /*	Attenuation at IF frequency */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double ai140;	/*	Attenuation at 2 * IF frequency */

        //------------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 15;

        //------------------------------------------------------------------------------------

        public const int ECODE_SZ = 9;
        public const int TRAFCODE_SZ = 7;

        //------------------------------------------------------------------------------------

        public const int ECODE = 0;
        public const int TRAFCODE = 1;
        public const int NUMCHAN = 2;
        public const int FMIN = 3;
        public const int FM = 4;
        public const int SIGMA = 5;
        public const int NF = 6;
        public const int IFFREQ = 7;
        public const int NFILTER = 8;
        public const int AFILTFS = 9;
        public const int DOUBLE = 10;
        public const int SIF = 11;
        public const int IRF = 12;
        public const int AI70 = 13;
        public const int AI140 = 14;

        //------------------------------------------------------------------------------------

        private static int count = 0;
        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            count++;
            sb.Append(String.Format("\n\n======= TcTxAnalog # {0} =======", count));

            sb.Append("\necode = " + ecode);
            sb.Append("\ntrafcode = " + trafcode);
            sb.Append("\nnumchan = " + numchan);
            sb.Append("\nfmin = " + fmin);
            sb.Append("\nfm = " + fm);
            sb.Append("\nsigma = " + sigma);
            sb.Append("\nnf = " + nf);
            sb.Append("\niffreq = " + iffreq);
            sb.Append("\nnFilter = " + nFilter);
            for (int i = 0; i < nFilter; i++)
            {
                sb.Append(String.Format("\naFiltFS[{0}] = {1}", i, aFiltFS[i]));
            }
            for (int i = 0; i < nFilter; i++)
            {
                sb.Append(String.Format("\naFiltVal[{0}] = {1}", i, aFiltVal[i]));
            }
            sb.Append("\nsif = " + sif);
            sb.Append("\nirf = " + irf);
            sb.Append("\nai70 = " + ai70);
            sb.Append("\nai140 = " + ai140);

            return sb.ToString();
        }



    }
}
