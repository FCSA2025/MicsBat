using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace _DataStructures
{
    using SQLPOINTER = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with DB tables of the type: 
    /// <b>&lt;userID&gt;.tt_&lt;pdfName&gt;_chan</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TtChan
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTERFERER_SZ)]
        public string interferer;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTCALL1_SZ)]
        public string intcall1;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTCALL2_SZ)]
        public string intcall2;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTBNDCDE_SZ)]
        public string intbndcde;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short intanum;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTCHID_SZ)]
        public string intchid;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCALL1_SZ)]
        public string viccall1;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCALL2_SZ)]
        public string viccall2;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICBNDCDE_SZ)]
        public string vicbndcde;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short vicanum;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCHID_SZ)]
        public string vicchid;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTPOLAR_SZ)]
        public string intpolar;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICPOLAR_SZ)]
        public string vicpolar;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTSTATTX_SZ)]
        public string intstattx;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICSTATRX_SZ)]
        public string vicstatrx;

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
        public double vicfreqrx;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicpwrrx;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intpwrtx;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intafsltx;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicafslrx;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rxant;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short txant;

        /* 	calculated fields 	*/
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXINTTRAFTX_SZ)]
        public string ctxinttraftx;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXVICTRAFRX_SZ)]
        public string ctxvictrafrx;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXEQPT_SZ)]
        public string ctxeqpt;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CALCTYPE_SZ)]
        public string calctype;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short report;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double totantdisc;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freqsep;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqdcalc;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double patloss;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcico;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcixp;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double resti;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double eirpadv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tiltdisc;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double pathloss80;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcico80;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcixp80;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd80;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double resti80;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double pathloss99;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcico99;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double calcixp99;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double reqd99;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double resti99;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int ohresult;

        /*	Added for the Aggregate Interference Reports - 1181 - GJS - 2006.02 */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double rqco;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int caseno;

        /*	Added to display xref info for CTX Calcs - 1215 - GJS - 2006.04.13 */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = CTXINTEQPT_SZ)]
        public string ctxinteqpt;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTEQTYPE_SZ)]
        public string inteqtype;
        /* 	A for analog equipment and D for Digital */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICEQTYPE_SZ)]
        public string viceqtype;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intbwchans;

        /*	If int is A this is number of channels, if D this is the bandwidth */
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicbwchans;


        //----------------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 60;

        //----------------------------------------------------------------------------------------

        // The following constants enumerate the order of declaration of the public fields.
        public const int INTERFERER = 0;
        public const int INTCALL1 = 1;
        public const int INTCALL2 = 2;
        public const int INTBNDCDE = 3;
        public const int INTANUM = 4;
        public const int INTCHID = 5;
        public const int VICCALL1 = 6;
        public const int VICCALL2 = 7;
        public const int VICBNDCDE = 8;
        public const int VICANUM = 9;
        public const int VICCHID = 10;
        public const int INTPOLAR = 11;
        public const int VICPOLAR = 12;
        public const int INTSTATTX = 13;
        public const int VICSTATRX = 14;
        public const int INTTRAFTX = 15;
        public const int VICTRAFRX = 16;
        public const int INTEQPTTX = 17;
        public const int VICEQPTRX = 18;
        public const int INTFREQTX = 19;
        public const int VICFREQRX = 20;
        public const int VICPWRRX = 21;
        public const int INTPWRTX = 22;
        public const int INTAFSLTX = 23;
        public const int VICAFSLRX = 24;
        public const int RXANT = 25;
        public const int TXANT = 26;
        public const int CTXINTTRAFTX = 27;
        public const int CTXVICTRAFRX = 28;
        public const int CTXEQPT = 29;
        public const int CALCTYPE = 30;
        public const int REPORT = 31;
        public const int TOTANTDISC = 32;
        public const int FREQSEP = 33;
        public const int REQDCALC = 34;
        public const int PATLOSS = 35;
        public const int CALCICO = 36;
        public const int CALCIXP = 37;
        public const int RESTI = 38;
        public const int EIRPADV = 39;
        public const int TILTDISC = 40;
        public const int PATHLOSS80 = 41;
        public const int CALCICO80 = 42;
        public const int CALCIXP80 = 43;
        public const int REQD80 = 44;
        public const int RESTI80 = 45;
        public const int PATHLOSS99 = 46;
        public const int CALCICO99 = 47;
        public const int CALCIXP99 = 48;
        public const int REQD99 = 49;
        public const int RESTI99 = 50;
        public const int OHRESULT = 51;
        public const int RQCO = 52;
        public const int PROCESSED = 53;
        public const int CASENO = 54;
        public const int CTXINTEQPT = 55;
        public const int INTEQTYPE = 56;
        public const int VICEQTYPE = 57;
        public const int INTBWCHANS = 58;
        public const int VICBWCHANS = 59;

        //------------------------------------------------------------------------------------------

        // The following constants prescribe the 'legacy' size of the string fields
        public const int INTERFERER_SZ = 2;
        public const int INTCALL1_SZ = Constant.CALLSIGN_SZ;
        public const int INTCALL2_SZ = Constant.CALLSIGN_SZ;
        public const int INTBNDCDE_SZ = 5;
        public const int INTCHID_SZ = 5;
        public const int VICCALL1_SZ = Constant.CALLSIGN_SZ;
        public const int VICCALL2_SZ = Constant.CALLSIGN_SZ;
        public const int VICBNDCDE_SZ = 5;
        public const int VICCHID_SZ = 5;
        public const int INTPOLAR_SZ = 2;
        public const int VICPOLAR_SZ = 2;
        public const int INTSTATTX_SZ = 2;
        public const int VICSTATRX_SZ = 2;
        public const int INTTRAFTX_SZ = 7;
        public const int VICTRAFRX_SZ = 7;
        public const int INTEQPTTX_SZ = 9;
        public const int VICEQPTRX_SZ = 9;
        public const int CTXINTTRAFTX_SZ = 7;
        public const int CTXVICTRAFRX_SZ = 7;
        public const int CTXEQPT_SZ = 9;
        public const int CALCTYPE_SZ = 4;
        public const int CTXINTEQPT_SZ = 9;
        public const int INTEQTYPE_SZ = 2;
        public const int VICEQTYPE_SZ = 2;

        //------------------------------------------------------------------------------------

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of float and/or double into native memory using Marshal method.
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[INTERFERER] = Marshal.StringToHGlobalAnsi(interferer);

            parameterValuePtr[INTCALL1] = Marshal.StringToHGlobalAnsi(intcall1);

            parameterValuePtr[INTCALL2] = Marshal.StringToHGlobalAnsi(intcall2);

            parameterValuePtr[INTBNDCDE] = Marshal.StringToHGlobalAnsi(intbndcde);

            parameterValuePtr[INTANUM] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[INTANUM], intanum);

            parameterValuePtr[INTCHID] = Marshal.StringToHGlobalAnsi(intchid);

            parameterValuePtr[VICCALL1] = Marshal.StringToHGlobalAnsi(viccall1);

            parameterValuePtr[VICCALL2] = Marshal.StringToHGlobalAnsi(viccall2);

            parameterValuePtr[VICBNDCDE] = Marshal.StringToHGlobalAnsi(vicbndcde);

            parameterValuePtr[VICANUM] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[VICANUM], vicanum);

            parameterValuePtr[VICCHID] = Marshal.StringToHGlobalAnsi(vicchid);

            parameterValuePtr[INTPOLAR] = Marshal.StringToHGlobalAnsi(intpolar);

            parameterValuePtr[VICPOLAR] = Marshal.StringToHGlobalAnsi(vicpolar);

            parameterValuePtr[INTSTATTX] = Marshal.StringToHGlobalAnsi(intstattx);

            parameterValuePtr[VICSTATRX] = Marshal.StringToHGlobalAnsi(vicstatrx);

            parameterValuePtr[INTTRAFTX] = Marshal.StringToHGlobalAnsi(inttraftx);

            parameterValuePtr[VICTRAFRX] = Marshal.StringToHGlobalAnsi(victrafrx);

            parameterValuePtr[INTEQPTTX] = Marshal.StringToHGlobalAnsi(inteqpttx);

            parameterValuePtr[VICEQPTRX] = Marshal.StringToHGlobalAnsi(viceqptrx);

            parameterValuePtr[INTFREQTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intfreqtx;
            Marshal.Copy(D, 0, parameterValuePtr[INTFREQTX], 1);

            parameterValuePtr[VICFREQRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicfreqrx;
            Marshal.Copy(D, 0, parameterValuePtr[VICFREQRX], 1);

            parameterValuePtr[VICPWRRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicpwrrx;
            Marshal.Copy(D, 0, parameterValuePtr[VICPWRRX], 1);

            parameterValuePtr[INTPWRTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intpwrtx;
            Marshal.Copy(D, 0, parameterValuePtr[INTPWRTX], 1);

            parameterValuePtr[INTAFSLTX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intafsltx;
            Marshal.Copy(D, 0, parameterValuePtr[INTAFSLTX], 1);

            parameterValuePtr[VICAFSLRX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicafslrx;
            Marshal.Copy(D, 0, parameterValuePtr[VICAFSLRX], 1);

            parameterValuePtr[RXANT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[RXANT], rxant);

            parameterValuePtr[TXANT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TXANT], txant);

            parameterValuePtr[CTXINTTRAFTX] = Marshal.StringToHGlobalAnsi(ctxinttraftx);

            parameterValuePtr[CTXVICTRAFRX] = Marshal.StringToHGlobalAnsi(ctxvictrafrx);

            parameterValuePtr[CTXEQPT] = Marshal.StringToHGlobalAnsi(ctxeqpt);

            parameterValuePtr[CALCTYPE] = Marshal.StringToHGlobalAnsi(calctype);

            parameterValuePtr[REPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[REPORT], report);

            parameterValuePtr[TOTANTDISC] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = totantdisc;
            Marshal.Copy(D, 0, parameterValuePtr[TOTANTDISC], 1);

            parameterValuePtr[FREQSEP] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = freqsep;
            Marshal.Copy(D, 0, parameterValuePtr[FREQSEP], 1);

            parameterValuePtr[REQDCALC] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = reqdcalc;
            Marshal.Copy(D, 0, parameterValuePtr[REQDCALC], 1);

            parameterValuePtr[PATLOSS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = patloss;
            Marshal.Copy(D, 0, parameterValuePtr[PATLOSS], 1);

            parameterValuePtr[CALCICO] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calcico;
            Marshal.Copy(D, 0, parameterValuePtr[CALCICO], 1);

            parameterValuePtr[CALCIXP] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calcixp;
            Marshal.Copy(D, 0, parameterValuePtr[CALCIXP], 1);

            parameterValuePtr[RESTI] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = resti;
            Marshal.Copy(D, 0, parameterValuePtr[RESTI], 1);

            parameterValuePtr[EIRPADV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = eirpadv;
            Marshal.Copy(D, 0, parameterValuePtr[EIRPADV], 1);

            parameterValuePtr[TILTDISC] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tiltdisc;
            Marshal.Copy(D, 0, parameterValuePtr[TILTDISC], 1);

            parameterValuePtr[PATHLOSS80] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = pathloss80;
            Marshal.Copy(D, 0, parameterValuePtr[PATHLOSS80], 1);

            parameterValuePtr[CALCICO80] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calcico80;
            Marshal.Copy(D, 0, parameterValuePtr[CALCICO80], 1);

            parameterValuePtr[CALCIXP80] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calcixp80;
            Marshal.Copy(D, 0, parameterValuePtr[CALCIXP80], 1);

            parameterValuePtr[REQD80] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = reqd80;
            Marshal.Copy(D, 0, parameterValuePtr[REQD80], 1);

            parameterValuePtr[RESTI80] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = resti80;
            Marshal.Copy(D, 0, parameterValuePtr[RESTI80], 1);

            parameterValuePtr[PATHLOSS99] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = pathloss99;
            Marshal.Copy(D, 0, parameterValuePtr[PATHLOSS99], 1);

            parameterValuePtr[CALCICO99] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calcico99;
            Marshal.Copy(D, 0, parameterValuePtr[CALCICO99], 1);

            parameterValuePtr[CALCIXP99] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = calcixp99;
            Marshal.Copy(D, 0, parameterValuePtr[CALCIXP99], 1);

            parameterValuePtr[REQD99] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = reqd99;
            Marshal.Copy(D, 0, parameterValuePtr[REQD99], 1);

            parameterValuePtr[RESTI99] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = resti99;
            Marshal.Copy(D, 0, parameterValuePtr[RESTI99], 1);

            parameterValuePtr[OHRESULT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[OHRESULT], ohresult);

            parameterValuePtr[RQCO] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = rqco;
            Marshal.Copy(D, 0, parameterValuePtr[RQCO], 1);

            parameterValuePtr[PROCESSED] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[PROCESSED], processed);

            parameterValuePtr[CASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[CASENO], caseno);

            parameterValuePtr[CTXINTEQPT] = Marshal.StringToHGlobalAnsi(ctxinteqpt);

            parameterValuePtr[INTEQTYPE] = Marshal.StringToHGlobalAnsi(inteqtype);

            parameterValuePtr[VICEQTYPE] = Marshal.StringToHGlobalAnsi(viceqtype);

            parameterValuePtr[INTBWCHANS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intbwchans;
            Marshal.Copy(D, 0, parameterValuePtr[INTBWCHANS], 1);

            parameterValuePtr[VICBWCHANS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicbwchans;
            Marshal.Copy(D, 0, parameterValuePtr[VICBWCHANS], 1);

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

            sb.AppendLine("\n----- TtChan -----");

            sb.AppendLine("interferer =      " + interferer);
            sb.AppendLine("intcall1 =      " + intcall1);
            sb.AppendLine("intcall2 =      " + intcall2);
            sb.AppendLine("intbndcde =      " + intbndcde);
            sb.AppendLine("intanum =      " + intanum);
            sb.AppendLine("intchid =      " + intchid);
            sb.AppendLine("viccall1 =      " + viccall1);
            sb.AppendLine("viccall2 =      " + viccall2);
            sb.AppendLine("vicbndcde =      " + vicbndcde);
            sb.AppendLine("vicanum =      " + vicanum);
            sb.AppendLine("vicchid =      " + vicchid);
            sb.AppendLine("intpolar =      " + intpolar);
            sb.AppendLine("vicpolar =      " + vicpolar);
            sb.AppendLine("intstattx =      " + intstattx);
            sb.AppendLine("vicstatrx =      " + vicstatrx);
            sb.AppendLine("inttraftx =      " + inttraftx);
            sb.AppendLine("victrafrx =      " + victrafrx);
            sb.AppendLine("inteqpttx =      " + inteqpttx);
            sb.AppendLine("viceqptrx =      " + viceqptrx);
            sb.AppendLine("intfreqtx =      " + intfreqtx);
            sb.AppendLine("vicfreqrx =      " + vicfreqrx);
            sb.AppendLine("vicpwrrx =      " + vicpwrrx);
            sb.AppendLine("intpwrtx =      " + intpwrtx);
            sb.AppendLine("intafsltx =      " + intafsltx);
            sb.AppendLine("vicafslrx =      " + vicafslrx);
            sb.AppendLine("rxant =      " + rxant);
            sb.AppendLine("txant =      " + txant);
            sb.AppendLine("ctxinttraftx =      " + ctxinttraftx);
            sb.AppendLine("ctxvictrafrx =      " + ctxvictrafrx);
            sb.AppendLine("ctxeqpt =      " + ctxeqpt);
            sb.AppendLine("calctype =      " + calctype);
            sb.AppendLine("report =      " + report);
            sb.AppendLine("totantdisc =      " + totantdisc);
            sb.AppendLine("freqsep =      " + freqsep);
            sb.AppendLine("reqdcalc =      " + reqdcalc);
            sb.AppendLine("patloss =      " + patloss);
            sb.AppendLine("calcico =      " + calcico);
            sb.AppendLine("calcixp =      " + calcixp);
            sb.AppendLine("resti =      " + resti);
            sb.AppendLine("eirpadv =      " + eirpadv);
            sb.AppendLine("tiltdisc =      " + tiltdisc);
            sb.AppendLine("pathloss80 =      " + pathloss80);
            sb.AppendLine("calcico80 =      " + calcico80);
            sb.AppendLine("calcixp80 =      " + calcixp80);
            sb.AppendLine("reqd80 =      " + reqd80);
            sb.AppendLine("resti80 =      " + resti80);
            sb.AppendLine("pathloss99 =      " + pathloss99);
            sb.AppendLine("calcico99 =      " + calcico99);
            sb.AppendLine("calcixp99 =      " + calcixp99);
            sb.AppendLine("reqd99 =      " + reqd99);
            sb.AppendLine("resti99 =      " + resti99);
            sb.AppendLine("ohresult =      " + ohresult);
            sb.AppendLine("rqco =      " + rqco);
            sb.AppendLine("processed =      " + processed);
            sb.AppendLine("caseno =      " + caseno);
            sb.AppendLine("ctxinteqpt =      " + ctxinteqpt);
            sb.AppendLine("inteqtype =      " + inteqtype);
            sb.AppendLine("viceqtype =      " + viceqtype);
            sb.AppendLine("intbwchans =      " + intbwchans);
            sb.AppendLine("vicbwchans =      " + vicbwchans);

            return sb.ToString();
        }
        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();
            int n = 0;

            sb.AppendLine("\n----- TtChan -----");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "interferer =      " + interferer);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intcall1 =      " + intcall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intcall2 =      " + intcall2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intbndcde =      " + intbndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intanum =      " + intanum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intchid =      " + intchid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viccall1 =      " + viccall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viccall2 =      " + viccall2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicbndcde =      " + vicbndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicanum =      " + vicanum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicchid =      " + vicchid);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intpolar =      " + intpolar);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicpolar =      " + vicpolar);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intstattx =      " + intstattx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicstatrx =      " + vicstatrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inttraftx =      " + inttraftx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "victrafrx =      " + victrafrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inteqpttx =      " + inteqpttx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viceqptrx =      " + viceqptrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intfreqtx =      " + intfreqtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicfreqrx =      " + vicfreqrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicpwrrx =      " + vicpwrrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intpwrtx =      " + intpwrtx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intafsltx =      " + intafsltx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicafslrx =      " + vicafslrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxant =      " + rxant);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txant =      " + txant);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxinttraftx =      " + ctxinttraftx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxvictrafrx =      " + ctxvictrafrx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxeqpt =      " + ctxeqpt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calctype =      " + calctype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "report =      " + report);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "totantdisc =      " + totantdisc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "freqsep =      " + freqsep);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reqdcalc =      " + reqdcalc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "patloss =      " + patloss);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcico =      " + calcico);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcixp =      " + calcixp);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "resti =      " + resti);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eirpadv =      " + eirpadv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tiltdisc =      " + tiltdisc);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pathloss80 =      " + pathloss80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcico80 =      " + calcico80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcixp80 =      " + calcixp80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reqd80 =      " + reqd80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "resti80 =      " + resti80);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "pathloss99 =      " + pathloss99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcico99 =      " + calcico99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "calcixp99 =      " + calcixp99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "reqd99 =      " + reqd99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "resti99 =      " + resti99);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ohresult =      " + ohresult);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rqco =      " + rqco);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "processed =      " + processed);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "caseno =      " + caseno);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ctxinteqpt =      " + ctxinteqpt);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "inteqtype =      " + inteqtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "viceqtype =      " + viceqtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intbwchans =      " + intbwchans);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "vicbwchans =      " + vicbwchans);

            return sb.ToString();
        }




    }
}
