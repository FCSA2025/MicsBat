using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates a subset of the columns of the DB table <b>TSIP.ctxdeqpt</b>
    /// that stores digital equipment data. Objects of this class are used to pass
    /// digital equipment data to the CTX calculation methods.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TcTxDigital
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = ECODE_SZ)]
        public string ecode;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TRAFCODE_SZ)]
        public string trafcode;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double bandwidth;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double nf;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double iffreq;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double ai70;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double ai140;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double thdcrit;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double sif;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double irf;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double nth;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nFilter;

#if INTPTR
        public IntPtr aFiltFS;

        public IntPtr aFiltVal;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nSpect;

        public IntPtr aSpectFS;

        public IntPtr aSpectVal;
#else
        [MarshalAsAttribute(UnmanagedType.LPStruct)]  // Be carefull!
        public double[] aFiltFS;

        [MarshalAsAttribute(UnmanagedType.LPStruct)]  // Be carefull!
        public double[] aFiltVal;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int nSpect;

        [MarshalAsAttribute(UnmanagedType.LPStruct)]  // Be carefull!
        public double[] aSpectFS;

        [MarshalAsAttribute(UnmanagedType.LPStruct)]  // Be carefull!
        public double[] aSpectVal;
#endif


        //-----------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 17;

        //-----------------------------------------------------------------------------------

        public const int ECODE_SZ = 9;
        public const int TRAFCODE_SZ = 7;

        //-----------------------------------------------------------------------------------

        public const int ECODE = 0;
        public const int TRAFCODE = 1;
        public const int BANDWIDTH = 2;
        public const int NF = 3;
        public const int IFFREQ = 4;
        public const int AI70 = 5;
        public const int AI140 = 6;
        public const int THDCRIT = 7;
        public const int SIF = 8;
        public const int IRF = 9;
        public const int NTH = 10;
        public const int NFILTER = 11;
        public const int AFILTFSPTR = 12;
        public const int AFILTVALPTR = 13;
        public const int NSPECT = 14;
        public const int ASPECTFSPTR = 15;
        public const int ASPECTVALPTR = 16;

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
            sb.Append(String.Format("\n\n======= TcTxDigital # {0} =======", count));

            sb.Append("\necode = " + ecode);
            sb.Append("\ntrafcode = " + trafcode);
            sb.Append("\nbandwidth = " + bandwidth);
            sb.Append("\nnf = " + nf);
            sb.Append("\niffreq = " + iffreq);
            sb.Append("\nai70 = " + ai70);
            sb.Append("\nai140 = " + ai140);
            sb.Append("\nthdcrit = " + thdcrit);
            sb.Append("\nsif = " + sif);
            sb.Append("\nirf = " + irf);
            sb.Append("\nnth = " + nth);
            sb.Append("\nnFilter = " + nFilter);
            for (int i = 0; i < nFilter; i++)
            {
                sb.Append(String.Format("\naFiltFS[{0}] = {1}", i, aFiltFS[i]));
            }
            for (int i = 0; i < nFilter; i++)
            {
                sb.Append(String.Format("\naFiltVal[{0}] = {1}", i, aFiltVal[i]));
            }
            sb.Append("\nnSpect = " + nSpect);
            for (int i = 0; i < nSpect; i++)
            {
                sb.Append(String.Format("\naSpectFS[{0}] = {1}", i, aSpectFS[i]));
            }
            for (int i = 0; i < nSpect; i++)
            {
                sb.Append(String.Format("\naSpectVal[{0}] = {1}", i, aSpectVal[i]));
            }

            return sb.ToString();
        }

    }
}
