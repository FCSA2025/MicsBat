# Documented File: TtSite.cs
**Repository Path:** `_DataStructures\TtSite.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
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
    /// <b>&lt;userID&gt;.tt_&lt;pdfName&gt;_site</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TtSite
    {

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTERFERER_SZ)]
        public string interferer;  /* P - proposed, E - environment */
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTCALL1_SZ)]
        public string intcall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTCALL2_SZ)]
        public string intcall2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCALL1_SZ)]
        public string viccall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICCALL2_SZ)]
        public string viccall2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTNAME1_SZ)]
        public string intname1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTNAME2_SZ)]
        public string intname2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICNAME1_SZ)]
        public string vicname1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICNAME2_SZ)]
        public string vicname2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTOPER_SZ)]
        public string intoper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTOPER2_SZ)]
        public string intoper2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICOPER_SZ)]
        public string vicoper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = VICOPER2_SZ)]
        public string vicoper2;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int intlatit;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int intlongit;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intgrnd;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int viclatit;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int viclongit;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicgrnd;

        /* calculated fields */
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short report;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int caseno;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int subcases;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double int1int2dist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vic1vic2dist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double int1vic1dist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double distadv;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intoffax;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicoffax;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double intvicaz;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double vicintaz;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;

        //----------------------------------------------------------------------------------------

        public const int NUM_COLUMNS = 31;

        public const int INTERFERER_SZ = 2;
        public const int INTCALL1_SZ = 10;
        public const int INTCALL2_SZ = 10;
        public const int VICCALL1_SZ = 10;
        public const int VICCALL2_SZ = 10;
        public const int INTNAME1_SZ = 33;
        public const int INTNAME2_SZ = 33;
        public const int VICNAME1_SZ = 33;
        public const int VICNAME2_SZ = 33;
        public const int INTOPER_SZ = 7;
        public const int INTOPER2_SZ = 7;
        public const int VICOPER_SZ = 7;
        public const int VICOPER2_SZ = 7;

        public const int INTERFERER = 0;
        public const int INTCALL1 = 1;
        public const int INTCALL2 = 2;
        public const int VICCALL1 = 3;
        public const int VICCALL2 = 4;
        public const int INTNAME1 = 5;
        public const int INTNAME2 = 6;
        public const int VICNAME1 = 7;
        public const int VICNAME2 = 8;
        public const int INTOPER = 9;
        public const int INTOPER2 = 10;
        public const int VICOPER = 11;
        public const int VICOPER2 = 12;
        public const int INTLATIT = 13;
        public const int INTLONGIT = 14;
        public const int INTGRND = 15;
        public const int VICLATIT = 16;
        public const int VICLONGIT = 17;
        public const int VICGRND = 18;
        public const int REPORT = 19;
        public const int CASENO = 20;
        public const int SUBCASES = 21;
        public const int INT1INT2DIST = 22;
        public const int VIC1VIC2DIST = 23;
        public const int INT1VIC1DIST = 24;
        public const int DISTADV = 25;
        public const int INTOFFAX = 26;
        public const int VICOFFAX = 27;
        public const int INTVICAZ = 28;
        public const int VICINTAZ = 29;
        public const int PROCESSED = 30;

        //----------------------------------------------------------------------------------------

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            //Can only copy arrays of double into native memory using Marshal method.
            double[] D = new double[1];

            parameterValuePtr[INTERFERER] = Marshal.StringToHGlobalAnsi(interferer);

            parameterValuePtr[INTCALL1] = Marshal.StringToHGlobalAnsi(intcall1);

            parameterValuePtr[INTCALL2] = Marshal.StringToHGlobalAnsi(intcall2);

            parameterValuePtr[VICCALL1] = Marshal.StringToHGlobalAnsi(viccall1);

            parameterValuePtr[VICCALL2] = Marshal.StringToHGlobalAnsi(viccall2);

            parameterValuePtr[INTNAME1] = Marshal.StringToHGlobalAnsi(intname1);

            parameterValuePtr[INTNAME2] = Marshal.StringToHGlobalAnsi(intname2);

            parameterValuePtr[VICNAME1] = Marshal.StringToHGlobalAnsi(vicname1);

            parameterValuePtr[VICNAME2] = Marshal.StringToHGlobalAnsi(vicname2);

            parameterValuePtr[INTOPER] = Marshal.StringToHGlobalAnsi(intoper);

            parameterValuePtr[INTOPER2] = Marshal.StringToHGlobalAnsi(intoper2);

            parameterValuePtr[VICOPER] = Marshal.StringToHGlobalAnsi(vicoper);

            parameterValuePtr[VICOPER2] = Marshal.StringToHGlobalAnsi(vicoper2);

            parameterValuePtr[INTLATIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[INTLATIT], intlatit);

            parameterValuePtr[INTLONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[INTLONGIT], intlongit);

            parameterValuePtr[INTGRND] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intgrnd;
            Marshal.Copy(D, 0, parameterValuePtr[INTGRND], 1);

            parameterValuePtr[VICLATIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[VICLATIT], viclatit);

            parameterValuePtr[VICLONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[VICLONGIT], viclongit);

            parameterValuePtr[VICGRND] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicgrnd;
            Marshal.Copy(D, 0, parameterValuePtr[VICGRND], 1);

            parameterValuePtr[REPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[REPORT], report);

            parameterValuePtr[CASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[CASENO], caseno);

            parameterValuePtr[SUBCASES] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[SUBCASES], subcases);

            parameterValuePtr[INT1INT2DIST] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = int1int2dist;
            Marshal.Copy(D, 0, parameterValuePtr[INT1INT2DIST], 1);

            parameterValuePtr[VIC1VIC2DIST] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vic1vic2dist;
            Marshal.Copy(D, 0, parameterValuePtr[VIC1VIC2DIST], 1);

            parameterValuePtr[INT1VIC1DIST] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = int1vic1dist;
            Marshal.Copy(D, 0, parameterValuePtr[INT1VIC1DIST], 1);

            parameterValuePtr[DISTADV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = distadv;
            Marshal.Copy(D, 0, parameterValuePtr[DISTADV], 1);

            parameterValuePtr[INTOFFAX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intoffax;
            Marshal.Copy(D, 0, parameterValuePtr[INTOFFAX], 1);

            parameterValuePtr[VICOFFAX] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicoffax;
            Marshal.Copy(D, 0, parameterValuePtr[VICOFFAX], 1);

            parameterValuePtr[INTVICAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = intvicaz;
            Marshal.Copy(D, 0, parameterValuePtr[INTVICAZ], 1);

            parameterValuePtr[VICINTAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = vicintaz;
            Marshal.Copy(D, 0, parameterValuePtr[VICINTAZ], 1);

            parameterValuePtr[PROCESSED] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[PROCESSED], processed);

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

            sb.Append("\n\n----- TtSite -----");

            sb.Append("\ninterferer = " + interferer);
            sb.Append("\nintcall1 = " + intcall1);
            sb.Append("\nintcall2 = " + intcall2);
            sb.Append("\nviccall1 = " + viccall1);
            sb.Append("\nviccall2 = " + viccall2);
            sb.Append("\nintname1 = " + intname1);
            sb.Append("\nintname2 = " + intname2);
            sb.Append("\nvicname1 = " + vicname1);
            sb.Append("\nvicname2 = " + vicname2);
            sb.Append("\nintoper = " + intoper);
            sb.Append("\nintoper2 = " + intoper2);
            sb.Append("\nvicoper = " + vicoper);
            sb.Append("\nvicoper2 = " + vicoper2);
            sb.Append("\nintlatit = " + intlatit);
            sb.Append("\nintlongit = " + intlongit);
            sb.Append("\nintgrnd = " + intgrnd);
            sb.Append("\nviclatit = " + viclatit);
            sb.Append("\nviclongit = " + viclongit);
            sb.Append("\nvicgrnd = " + vicgrnd);
            sb.Append("\nreport = " + report);
            sb.Append("\ncaseno = " + caseno);
            sb.Append("\nsubcases = " + subcases);
            sb.Append("\nint1int2dist = " + int1int2dist);
            sb.Append("\nvic1vic2dist = " + vic1vic2dist);
            sb.Append("\nint1vic1dist = " + int1vic1dist);
            sb.Append("\ndistadv = " + distadv);
            sb.Append("\nintoffax = " + intoffax);
            sb.Append("\nvicoffax = " + vicoffax);
            sb.Append("\nintvicaz = " + intvicaz);
            sb.Append("\nvicintaz = " + vicintaz);
            sb.Append("\nprocessed = " + processed);

            return sb.ToString();
        }



    }
}

```
