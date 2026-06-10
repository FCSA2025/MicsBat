# Documented File: TtAnte.cs
**Repository Path:** `_DataStructures\TtAnte.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with DB tables of the type: 
    /// <b>&lt;userID&gt;.tt_&lt;pdfName&gt;_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class TtAnte
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

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCALL1_SZ)]
        public string viccall1;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCALL2_SZ)]
        public string viccall2;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICBNDCDE_SZ)]
        public string vicbndcde;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short vicanum;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTACODE_SZ)]
        public string intacode;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICACODE_SZ)]
        public string vicacode;

        /* calculated fields */

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short report;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int subcaseno;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscctxh;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscctxv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisccrxh;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisccrxv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxtxh;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxtxv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxrxh;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adiscxrxv;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTAUSE_SZ)]
        public string intause;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICAUSE_SZ)]
        public string vicause;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float intgain;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float vicgain;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTAXREF_SZ)]
        public string intaxref;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTAMODEL_SZ)]
        public string intamodel;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICAXREF_SZ)]
        public string vicaxref;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICAMODEL_SZ)]
        public string vicamodel;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTAOFFAX_SZ)]
        public string intaoffax;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double inthopaz;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intantaz;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intoffantax;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICAOFFAX_SZ)]
        public string vicaoffax;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vichopaz;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicantaz;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicoffantax;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float intaht;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float vicaht;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intvicel;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicintel;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intelev;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicelev;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int caseno;

        //----------------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 45;

        //----------------------------------------------------------------------------------------

        public const int INTERFERER_SZ = 2;
        public const int INTCALL1_SZ = Constant.CALLSIGN_SZ;
        public const int INTCALL2_SZ = Constant.CALLSIGN_SZ;
        public const int INTBNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int VICCALL1_SZ = Constant.CALLSIGN_SZ;
        public const int VICCALL2_SZ = Constant.CALLSIGN_SZ;
        public const int VICBNDCDE_SZ = Constant.BNDCDE_SZ;
        public const int INTACODE_SZ = Constant.ACODE_SZ;
        public const int VICACODE_SZ = Constant.ACODE_SZ;
        public const int INTAUSE_SZ = 4;
        public const int VICAUSE_SZ = 4;
        public const int INTAXREF_SZ = Constant.ACODE_SZ;
        public const int INTAMODEL_SZ = Constant.ANTE_MODEL_SZ;
        public const int VICAXREF_SZ = Constant.ACODE_SZ;
        public const int VICAMODEL_SZ = Constant.ANTE_MODEL_SZ;
        public const int INTAOFFAX_SZ = 2;
        public const int VICAOFFAX_SZ = 2;

        //----------------------------------------------------------------------------------------

        public const int INTERFERER = 0;
        public const int INTCALL1 = 1;
        public const int INTCALL2 = 2;
        public const int INTBNDCDE = 3;
        public const int INTANUM = 4;
        public const int VICCALL1 = 5;
        public const int VICCALL2 = 6;
        public const int VICBNDCDE = 7;
        public const int VICANUM = 8;
        public const int INTACODE = 9;
        public const int VICACODE = 10;
        public const int REPORT = 11;
        public const int SUBCASENO = 12;
        public const int ADISCCTXH = 13;
        public const int ADISCCTXV = 14;
        public const int ADISCCRXH = 15;
        public const int ADISCCRXV = 16;
        public const int ADISCXTXH = 17;
        public const int ADISCXTXV = 18;
        public const int ADISCXRXH = 19;
        public const int ADISCXRXV = 20;
        public const int PROCESSED = 21;
        public const int INTAUSE = 22;
        public const int VICAUSE = 23;
        public const int INTGAIN = 24;
        public const int VICGAIN = 25;
        public const int INTAXREF = 26;
        public const int INTAMODEL = 27;
        public const int VICAXREF = 28;
        public const int VICAMODEL = 29;
        public const int INTAOFFAX = 30;
        public const int INTHOPAZ = 31;
        public const int INTANTAZ = 32;
        public const int INTOFFANTAX = 33;
        public const int VICAOFFAX = 34;
        public const int VICHOPAZ = 35;
        public const int VICANTAZ = 36;
        public const int VICOFFANTAX = 37;
        public const int INTAHT = 38;
        public const int VICAHT = 39;
        public const int INTVICEL = 40;
        public const int VICINTEL = 41;
        public const int INTELEV = 42;
        public const int VICELEV = 43;
        public const int CASENO = 44;

        //-----------------------------------------------------------------------------
        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of float and/or double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[INTERFERER] = Marshal.StringToHGlobalAnsi(interferer);

            parameterValuePtr[INTCALL1] = Marshal.StringToHGlobalAnsi(intcall1);

            parameterValuePtr[INTCALL2] = Marshal.StringToHGlobalAnsi(intcall2);

            parameterValuePtr[INTBNDCDE] = Marshal.StringToHGlobalAnsi(intbndcde);

            parameterValuePtr[INTANUM] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[INTANUM], intanum);

            parameterValuePtr[VICCALL1] = Marshal.StringToHGlobalAnsi(viccall1);

            parameterValuePtr[VICCALL2] = Marshal.StringToHGlobalAnsi(viccall2);

            parameterValuePtr[VICBNDCDE] = Marshal.StringToHGlobalAnsi(vicbndcde);

            parameterValuePtr[VICANUM] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[VICANUM], vicanum);

            parameterValuePtr[INTACODE] = Marshal.StringToHGlobalAnsi(intacode);

            parameterValuePtr[VICACODE] = Marshal.StringToHGlobalAnsi(vicacode);

            parameterValuePtr[REPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[REPORT], report);

            parameterValuePtr[SUBCASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[SUBCASENO], subcaseno);

            parameterValuePtr[ADISCCTXH] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adiscctxh;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCCTXH], 1);

            parameterValuePtr[ADISCCTXV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adiscctxv;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCCTXV], 1);

            parameterValuePtr[ADISCCRXH] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adisccrxh;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCCRXH], 1);

            parameterValuePtr[ADISCCRXV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adisccrxv;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCCRXV], 1);

            parameterValuePtr[ADISCXTXH] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adiscxtxh;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCXTXH], 1);

            parameterValuePtr[ADISCXTXV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adiscxtxv;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCXTXV], 1);

            parameterValuePtr[ADISCXRXH] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adiscxrxh;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCXRXH], 1);

            parameterValuePtr[ADISCXRXV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adiscxrxv;
            Marshal.Copy(D, 0, parameterValuePtr[ADISCXRXV], 1);

            parameterValuePtr[PROCESSED] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[PROCESSED], processed);

            parameterValuePtr[INTAUSE] = Marshal.StringToHGlobalAnsi(intause);

            parameterValuePtr[VICAUSE] = Marshal.StringToHGlobalAnsi(vicause);

            parameterValuePtr[INTGAIN] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = intgain;
            Marshal.Copy(F, 0, parameterValuePtr[INTGAIN], 1);

            parameterValuePtr[VICGAIN] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = vicgain;
            Marshal.Copy(F, 0, parameterValuePtr[VICGAIN], 1);

            parameterValuePtr[INTAXREF] = Marshal.StringToHGlobalAnsi(intaxref);

            parameterValuePtr[INTAMODEL] = Marshal.StringToHGlobalAnsi(intamodel);

            parameterValuePtr[VICAXREF] = Marshal.StringToHGlobalAnsi(vicaxref);

            parameterValuePtr[VICAMODEL] = Marshal.StringToHGlobalAnsi(vicamodel);

            parameterValuePtr[INTAOFFAX] = Marshal.StringToHGlobalAnsi(intaoffax);

            parameterValuePtr[INTHOPAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = inthopaz;
            Marshal.Copy(D, 0, parameterValuePtr[INTHOPAZ], 1);

            parameterValuePtr[INTANTAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intantaz;
            Marshal.Copy(D, 0, parameterValuePtr[INTANTAZ], 1);

            parameterValuePtr[INTOFFANTAX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intoffantax;
            Marshal.Copy(D, 0, parameterValuePtr[INTOFFANTAX], 1);

            parameterValuePtr[VICAOFFAX] = Marshal.StringToHGlobalAnsi(vicaoffax);

            parameterValuePtr[VICHOPAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vichopaz;
            Marshal.Copy(D, 0, parameterValuePtr[VICHOPAZ], 1);

            parameterValuePtr[VICANTAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicantaz;
            Marshal.Copy(D, 0, parameterValuePtr[VICANTAZ], 1);

            parameterValuePtr[VICOFFANTAX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicoffantax;
            Marshal.Copy(D, 0, parameterValuePtr[VICOFFANTAX], 1);

            parameterValuePtr[INTAHT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = intaht;
            Marshal.Copy(F, 0, parameterValuePtr[INTAHT], 1);

            parameterValuePtr[VICAHT] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = vicaht;
            Marshal.Copy(F, 0, parameterValuePtr[VICAHT], 1);

            parameterValuePtr[INTVICEL] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intvicel;
            Marshal.Copy(D, 0, parameterValuePtr[INTVICEL], 1);

            parameterValuePtr[VICINTEL] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicintel;
            Marshal.Copy(D, 0, parameterValuePtr[VICINTEL], 1);

            parameterValuePtr[INTELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intelev;
            Marshal.Copy(D, 0, parameterValuePtr[INTELEV], 1);

            parameterValuePtr[VICELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicelev;
            Marshal.Copy(D, 0, parameterValuePtr[VICELEV], 1);

            parameterValuePtr[CASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[CASENO], caseno);

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

            sb.Append("\n\n----- TtAnte -----");

            sb.Append("\ninterferer = " + interferer);
            sb.Append("\nintcall1 = " + intcall1);
            sb.Append("\nintcall2 = " + intcall2);
            sb.Append("\nintbndcde = " + intbndcde);
            sb.Append("\nintanum = " + intanum);
            sb.Append("\nviccall1 = " + viccall1);
            sb.Append("\nviccall2 = " + viccall2);
            sb.Append("\nvicbndcde = " + vicbndcde);
            sb.Append("\nvicanum = " + vicanum);
            sb.Append("\nintacode = " + intacode);
            sb.Append("\nvicacode = " + vicacode);
            sb.Append("\nreport = " + report);
            sb.Append("\nsubcaseno = " + subcaseno);
            sb.Append("\nadiscctxh = " + adiscctxh);
            sb.Append("\nadiscctxv = " + adiscctxv);
            sb.Append("\nadisccrxh = " + adisccrxh);
            sb.Append("\nadisccrxv = " + adisccrxv);
            sb.Append("\nadiscxtxh = " + adiscxtxh);
            sb.Append("\nadiscxtxv = " + adiscxtxv);
            sb.Append("\nadiscxrxh = " + adiscxrxh);
            sb.Append("\nadiscxrxv = " + adiscxrxv);
            sb.Append("\nprocessed = " + processed);
            sb.Append("\nintause = " + intause);
            sb.Append("\nvicause = " + vicause);
            sb.Append("\nintgain = " + intgain);
            sb.Append("\nvicgain = " + vicgain);
            sb.Append("\nintaxref = " + intaxref);
            sb.Append("\nintamodel = " + intamodel);
            sb.Append("\nvicaxref = " + vicaxref);
            sb.Append("\nvicamodel = " + vicamodel);
            sb.Append("\nintaoffax = " + intaoffax);
            sb.Append("\ninthopaz = " + inthopaz);
            sb.Append("\nintantaz = " + intantaz);
            sb.Append("\nintoffantax = " + intoffantax);
            sb.Append("\nvicaoffax = " + vicaoffax);
            sb.Append("\nvichopaz = " + vichopaz);
            sb.Append("\nvicantaz = " + vicantaz);
            sb.Append("\nvicoffantax = " + vicoffantax);
            sb.Append("\nintaht = " + intaht);
            sb.Append("\nvicaht = " + vicaht);
            sb.Append("\nintvicel = " + intvicel);
            sb.Append("\nvicintel = " + vicintel);
            sb.Append("\nintelev = " + intelev);
            sb.Append("\nvicelev = " + vicelev);
            sb.Append("\ncaseno = " + caseno);

            return sb.ToString();
        }



    }
}

```
