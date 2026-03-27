using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace _DataStructures
{
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with DB tables of the type: 
    /// <b>&lt;userID&gt;.te_&lt;pdfName&gt;_chan</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TeChan
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTERFERER_SZ)]
        public string interferer;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRCALL1_SZ)]
        public string terrcall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRCALL2_SZ)]
        public string terrcall2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRBNDCDE_SZ)]
        public string terrbndcde;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short terranum;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRCHID_SZ)]
        public string terrchid;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHLOCATION_SZ)]
        public string earthlocation;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHCALL1_SZ)]
        public string earthcall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHCHID_SZ)]
        public string earthchid;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTTRAFTX_SZ)]
        public string inttraftx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICTRAFRX_SZ)]
        public string victrafrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTEQPTTX_SZ)]
        public string inteqpttx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICEQPTRX_SZ)]
        public string viceqptrx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intfreqtx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double inttxpwr;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double inttxpwr2;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double inttxafls;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double inttxafls2;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicrxafls;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicfreqrx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicpwrrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATTX_SZ)]
        public string stattx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = STATRX_SZ)]
        public string statrx;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double energy;

        /* calculated fields */
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short etreport;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short tereport;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXINTTRAFTX_SZ)]
        public string ctxinttraftx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXVICTRAFRX_SZ)]
        public string ctxvictrafrx;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXEQPT_SZ)]
        public string ctxeqpt;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALCTYPE_SZ)]
        public string calctype; /* C/I or -I */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double earthmdsc; /* MDSC = Net Ante Gain */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double terrmdsc;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double eartheirp; /* only used for ES->TS */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double terreirp; /* only used for TS->ES */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqsep;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double scang;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double loss20mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double loss01mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double loss01mode2;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calci20mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calci01mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calci01mode2;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd20mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd01mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd01mode2;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double marg20mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double marg01mode1;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double marg01mode2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = REMTERRACODE_SZ)]
        public string remterracode;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double remterragain;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short terrant;

        //--------------------------------------------------------------------------

        public const int NUM_COLUMNS = 52;

        public const int INTERFERER_SZ = 2;
        public const int TERRCALL1_SZ = 10;
        public const int TERRCALL2_SZ = 10;
        public const int TERRBNDCDE_SZ = 5;
        public const int TERRCHID_SZ = 5;
        public const int EARTHLOCATION_SZ = 11;
        public const int EARTHCALL1_SZ = 10;
        public const int EARTHCHID_SZ = 5;
        public const int INTTRAFTX_SZ = 7;
        public const int VICTRAFRX_SZ = 7;
        public const int INTEQPTTX_SZ = 9;
        public const int VICEQPTRX_SZ = 9;
        public const int STATTX_SZ = 2;
        public const int STATRX_SZ = 2;
        public const int CTXINTTRAFTX_SZ = 7;
        public const int CTXVICTRAFRX_SZ = 7;
        public const int CTXEQPT_SZ = 9;
        public const int CALCTYPE_SZ = 4;
        public const int REMTERRACODE_SZ = 13;

        public const int INTERFERER = 0;
        public const int TERRCALL1 = 1;
        public const int TERRCALL2 = 2;
        public const int TERRBNDCDE = 3;
        public const int TERRANUM = 4;
        public const int TERRCHID = 5;
        public const int EARTHLOCATION = 6;
        public const int EARTHCALL1 = 7;
        public const int EARTHCHID = 8;
        public const int INTTRAFTX = 9;
        public const int VICTRAFRX = 10;
        public const int INTEQPTTX = 11;
        public const int VICEQPTRX = 12;
        public const int INTFREQTX = 13;
        public const int INTTXPWR = 14;
        public const int INTTXPWR2 = 15;
        public const int INTTXAFLS = 16;
        public const int INTTXAFLS2 = 17;
        public const int VICRXAFLS = 18;
        public const int VICFREQRX = 19;
        public const int VICPWRRX = 20;
        public const int STATTX = 21;
        public const int STATRX = 22;
        public const int ENERGY = 23;
        public const int ETREPORT = 24;
        public const int TEREPORT = 25;
        public const int CTXINTTRAFTX = 26;
        public const int CTXVICTRAFRX = 27;
        public const int CTXEQPT = 28;
        public const int CALCTYPE = 29;
        public const int EARTHMDSC = 30;
        public const int TERRMDSC = 31;
        public const int EARTHEIRP = 32;
        public const int TERREIRP = 33;
        public const int FREQSEP = 34;
        public const int SCANG = 35;
        public const int LOSS20MODE1 = 36;
        public const int LOSS01MODE1 = 37;
        public const int LOSS01MODE2 = 38;
        public const int CALCI20MODE1 = 39;
        public const int CALCI01MODE1 = 40;
        public const int CALCI01MODE2 = 41;
        public const int REQD20MODE1 = 42;
        public const int REQD01MODE1 = 43;
        public const int REQD01MODE2 = 44;
        public const int MARG20MODE1 = 45;
        public const int MARG01MODE1 = 46;
        public const int MARG01MODE2 = 47;
        public const int REMTERRACODE = 48;
        public const int REMTERRAGAIN = 49;
        public const int PROCESSED = 50;
        public const int TERRANT = 51;

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of float and/or double into native memory using P/Invoke marshalling.
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[INTERFERER] = Marshal.StringToHGlobalAnsi(interferer);

            parameterValuePtr[TERRCALL1] = Marshal.StringToHGlobalAnsi(terrcall1);

            parameterValuePtr[TERRCALL2] = Marshal.StringToHGlobalAnsi(terrcall2);

            parameterValuePtr[TERRBNDCDE] = Marshal.StringToHGlobalAnsi(terrbndcde);

            parameterValuePtr[TERRANUM] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TERRANUM], terranum);

            parameterValuePtr[TERRCHID] = Marshal.StringToHGlobalAnsi(terrchid);

            parameterValuePtr[EARTHLOCATION] = Marshal.StringToHGlobalAnsi(earthlocation);

            parameterValuePtr[EARTHCALL1] = Marshal.StringToHGlobalAnsi(earthcall1);

            parameterValuePtr[EARTHCHID] = Marshal.StringToHGlobalAnsi(earthchid);

            parameterValuePtr[INTTRAFTX] = Marshal.StringToHGlobalAnsi(inttraftx);

            parameterValuePtr[VICTRAFRX] = Marshal.StringToHGlobalAnsi(victrafrx);

            parameterValuePtr[INTEQPTTX] = Marshal.StringToHGlobalAnsi(inteqpttx);

            parameterValuePtr[VICEQPTRX] = Marshal.StringToHGlobalAnsi(viceqptrx);

            parameterValuePtr[INTFREQTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intfreqtx;
            Marshal.Copy(D, 0, parameterValuePtr[INTFREQTX], 1);

            parameterValuePtr[INTTXPWR] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = inttxpwr;
            Marshal.Copy(D, 0, parameterValuePtr[INTTXPWR], 1);

            parameterValuePtr[INTTXPWR2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = inttxpwr2;
            Marshal.Copy(D, 0, parameterValuePtr[INTTXPWR2], 1);

            parameterValuePtr[INTTXAFLS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = inttxafls;
            Marshal.Copy(D, 0, parameterValuePtr[INTTXAFLS], 1);

            parameterValuePtr[INTTXAFLS2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = inttxafls2;
            Marshal.Copy(D, 0, parameterValuePtr[INTTXAFLS2], 1);

            parameterValuePtr[VICRXAFLS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicrxafls;
            Marshal.Copy(D, 0, parameterValuePtr[VICRXAFLS], 1);

            parameterValuePtr[VICFREQRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicfreqrx;
            Marshal.Copy(D, 0, parameterValuePtr[VICFREQRX], 1);

            parameterValuePtr[VICPWRRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicpwrrx;
            Marshal.Copy(D, 0, parameterValuePtr[VICPWRRX], 1);

            parameterValuePtr[STATTX] = Marshal.StringToHGlobalAnsi(stattx);

            parameterValuePtr[STATRX] = Marshal.StringToHGlobalAnsi(statrx);

            parameterValuePtr[ENERGY] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = energy;
            Marshal.Copy(D, 0, parameterValuePtr[ENERGY], 1);

            parameterValuePtr[ETREPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ETREPORT], etreport);

            parameterValuePtr[TEREPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TEREPORT], tereport);

            parameterValuePtr[CTXINTTRAFTX] = Marshal.StringToHGlobalAnsi(ctxinttraftx);

            parameterValuePtr[CTXVICTRAFRX] = Marshal.StringToHGlobalAnsi(ctxvictrafrx);

            parameterValuePtr[CTXEQPT] = Marshal.StringToHGlobalAnsi(ctxeqpt);

            parameterValuePtr[CALCTYPE] = Marshal.StringToHGlobalAnsi(calctype);

            parameterValuePtr[EARTHMDSC] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = earthmdsc;
            Marshal.Copy(D, 0, parameterValuePtr[EARTHMDSC], 1);

            parameterValuePtr[TERRMDSC] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = terrmdsc;
            Marshal.Copy(D, 0, parameterValuePtr[TERRMDSC], 1);

            parameterValuePtr[EARTHEIRP] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = eartheirp;
            Marshal.Copy(D, 0, parameterValuePtr[EARTHEIRP], 1);

            parameterValuePtr[TERREIRP] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = terreirp;
            Marshal.Copy(D, 0, parameterValuePtr[TERREIRP], 1);

            parameterValuePtr[FREQSEP] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = freqsep;
            Marshal.Copy(D, 0, parameterValuePtr[FREQSEP], 1);

            parameterValuePtr[SCANG] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = scang;
            Marshal.Copy(D, 0, parameterValuePtr[SCANG], 1);

            parameterValuePtr[LOSS20MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = loss20mode1;
            Marshal.Copy(D, 0, parameterValuePtr[LOSS20MODE1], 1);

            parameterValuePtr[LOSS01MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = loss01mode1;
            Marshal.Copy(D, 0, parameterValuePtr[LOSS01MODE1], 1);

            parameterValuePtr[LOSS01MODE2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = loss01mode2;
            Marshal.Copy(D, 0, parameterValuePtr[LOSS01MODE2], 1);

            parameterValuePtr[CALCI20MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calci20mode1;
            Marshal.Copy(D, 0, parameterValuePtr[CALCI20MODE1], 1);

            parameterValuePtr[CALCI01MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calci01mode1;
            Marshal.Copy(D, 0, parameterValuePtr[CALCI01MODE1], 1);

            parameterValuePtr[CALCI01MODE2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calci01mode2;
            Marshal.Copy(D, 0, parameterValuePtr[CALCI01MODE2], 1);

            parameterValuePtr[REQD20MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = reqd20mode1;
            Marshal.Copy(D, 0, parameterValuePtr[REQD20MODE1], 1);

            parameterValuePtr[REQD01MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = reqd01mode1;
            Marshal.Copy(D, 0, parameterValuePtr[REQD01MODE1], 1);

            parameterValuePtr[REQD01MODE2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = reqd01mode2;
            Marshal.Copy(D, 0, parameterValuePtr[REQD01MODE2], 1);

            parameterValuePtr[MARG20MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = marg20mode1;
            Marshal.Copy(D, 0, parameterValuePtr[MARG20MODE1], 1);

            parameterValuePtr[MARG01MODE1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = marg01mode1;
            Marshal.Copy(D, 0, parameterValuePtr[MARG01MODE1], 1);

            parameterValuePtr[MARG01MODE2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = marg01mode2;
            Marshal.Copy(D, 0, parameterValuePtr[MARG01MODE2], 1);

            parameterValuePtr[REMTERRACODE] = Marshal.StringToHGlobalAnsi(remterracode);

            parameterValuePtr[REMTERRAGAIN] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = remterragain;
            Marshal.Copy(D, 0, parameterValuePtr[REMTERRAGAIN], 1);

            parameterValuePtr[PROCESSED] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt32(parameterValuePtr[PROCESSED], processed);

            parameterValuePtr[TERRANT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TERRANT], terrant);

            return parameterValuePtr;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("interferer = " + interferer);
            sb.AppendLine("terrcall1 = " + terrcall1);
            sb.AppendLine("terrcall2 = " + terrcall2);
            sb.AppendLine("terrbndcde = " + terrbndcde);
            sb.AppendLine("terranum = " + terranum);
            sb.AppendLine("terrchid = " + terrchid);
            sb.AppendLine("earthlocation = " + earthlocation);
            sb.AppendLine("earthcall1 = " + earthcall1);
            sb.AppendLine("earthchid = " + earthchid);
            sb.AppendLine("inttraftx = " + inttraftx);
            sb.AppendLine("victrafrx = " + victrafrx);
            sb.AppendLine("inteqpttx = " + inteqpttx);
            sb.AppendLine("viceqptrx = " + viceqptrx);
            sb.AppendLine("intfreqtx = " + intfreqtx);
            sb.AppendLine("inttxpwr = " + inttxpwr);
            sb.AppendLine("inttxpwr2 = " + inttxpwr2);
            sb.AppendLine("inttxafls = " + inttxafls);
            sb.AppendLine("inttxafls2 = " + inttxafls2);
            sb.AppendLine("vicrxafls = " + vicrxafls);
            sb.AppendLine("vicfreqrx = " + vicfreqrx);
            sb.AppendLine("vicpwrrx = " + vicpwrrx);
            sb.AppendLine("stattx = " + stattx);
            sb.AppendLine("statrx = " + statrx);
            sb.AppendLine("energy = " + energy);
            sb.AppendLine("etreport = " + etreport);
            sb.AppendLine("tereport = " + tereport);
            sb.AppendLine("ctxinttraftx = " + ctxinttraftx);
            sb.AppendLine("ctxvictrafrx = " + ctxvictrafrx);
            sb.AppendLine("ctxeqpt = " + ctxeqpt);
            sb.AppendLine("calctype = " + calctype);
            sb.AppendLine("earthmdsc = " + earthmdsc);
            sb.AppendLine("terrmdsc = " + terrmdsc);
            sb.AppendLine("eartheirp = " + eartheirp);
            sb.AppendLine("terreirp = " + terreirp);
            sb.AppendLine("freqsep = " + freqsep);
            sb.AppendLine("scang = " + scang);
            sb.AppendLine("loss20mode1 = " + loss20mode1);
            sb.AppendLine("loss01mode1 = " + loss01mode1);
            sb.AppendLine("loss01mode2 = " + loss01mode2);
            sb.AppendLine("calci20mode1 = " + calci20mode1);
            sb.AppendLine("calci01mode1 = " + calci01mode1);
            sb.AppendLine("calci01mode2 = " + calci01mode2);
            sb.AppendLine("reqd20mode1 = " + reqd20mode1);
            sb.AppendLine("reqd01mode1 = " + reqd01mode1);
            sb.AppendLine("reqd01mode2 = " + reqd01mode2);
            sb.AppendLine("marg20mode1 = " + marg20mode1);
            sb.AppendLine("marg01mode1 = " + marg01mode1);
            sb.AppendLine("marg01mode2 = " + marg01mode2);
            sb.AppendLine("remterracode = " + remterracode);
            sb.AppendLine("remterragain = " + remterragain);
            sb.AppendLine("processed = " + processed);
            sb.AppendLine("terrant = " + terrant);

            return sb.ToString();
        }


    }
}
