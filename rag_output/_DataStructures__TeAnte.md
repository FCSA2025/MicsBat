# Documented File: TeAnte.cs
**Repository Path:** `_DataStructures\TeAnte.cs`
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
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    /// <summary>
    /// This class has fields that are isomorphic with DB tables of the type: 
    /// <b>&lt;userID&gt;.te_&lt;pdfName&gt;_ante</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TeAnte
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

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHLOCATION_SZ)]
        public string earthlocation;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHCALL1_SZ)]
        public string earthcall1;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHBAND_SZ)]
        public string earthband;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRACODE_SZ)]
        public string terracode;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHACODE_SZ)]
        public string earthacode;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SATNAME_SZ)]
        public string satname;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = SATOPER_SZ)]
        public string satoper;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int satlongit;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txpre;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float txtro;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxpre;

        [MarshalAsAttribute(UnmanagedType.R4)]
        public float rxtro;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double sarc1;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double sarc2;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTAUSE_SZ)]
        public string intause;

        // Calculated fields.

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short mode1;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short mode2;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short etreport;

        [MarshalAsAttribute(UnmanagedType.I2)]
        public short tereport;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int etsubcaseno;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int tesubcaseno;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double esazim; /* earth stn to satellite azimuth */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double eselev; /* earth stn to satellite elevation */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double teelev; /* terr stn to earth stn elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double etelev; /* earth stn to terr stn elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tuelev; /* terr stn to terr link stn elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double utelev; /* terr link stn to terr stn elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double euelev; /* earth stn to terr link stn elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double ediscang; /* earth stn discrimination angle */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tdiscang; /* terr stn discrimination angle */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisc_set; /* ante. discrim. at angle SET */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisc_ute; /* ante. discrim. at angle UTE */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double terrht; /* terr stn ht amsl = ground + antenna ht */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double earthht; /* earth stn ht amsl = ground + antenna ht */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tvazim; /* terr stn to rain scatt. vol (rsv) azim */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double evazim; /* earth stn to rsv azim */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tvelev; /* terr stn to rsv elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double evelev; /* earth stn to rsv elev */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tvdistes; /* terr stn to rsv distance for vol on ES vec */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tvdisttu; /* terr stn to rsv distance for vol on TU vec */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double evdistes; /* earth stn to rsv dist for vol on ES vec */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double evdisttu; /* earth stn to rsv dist for vol on TU vec */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angleutv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double anglesev;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TSOFFAXIS_SZ)]
        public string tsoffaxis; /* 'Y' if offaxis */

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tstrueaz;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tstrueel;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angleute;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angleuta;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angleeta;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angleatv;

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double adisc_atv;

        public double terragain;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRAMODEL_SZ)]
        public string terramodel;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRAXREF_SZ)]
        public string terraxref;
        public double earthagain;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHAMODEL_SZ)]
        public string earthamodel;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHAXREF_SZ)]
        public string earthaxref;

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;


        //-------------------------------------------------------------

        public const int NUM_COLUMNS = 64;

        //-------------------------------------------------------------

        public const int INTERFERER = 0;
        public const int TERRCALL1 = 1;
        public const int TERRCALL2 = 2;
        public const int TERRBNDCDE = 3;
        public const int TERRANUM = 4;
        public const int EARTHLOCATION = 5;
        public const int EARTHCALL1 = 6;
        public const int EARTHBAND = 7;
        public const int TERRACODE = 8;
        public const int EARTHACODE = 9;
        public const int SATNAME = 10;
        public const int SATOPER = 11;
        public const int SATLONGIT = 12;
        public const int TXPRE = 13;
        public const int TXTRO = 14;
        public const int RXPRE = 15;
        public const int RXTRO = 16;
        public const int SARC1 = 17;
        public const int SARC2 = 18;
        public const int INTAUSE = 19;
        public const int MODE1 = 20;
        public const int MODE2 = 21;
        public const int ETREPORT = 22;
        public const int TEREPORT = 23;
        public const int ETSUBCASENO = 24;
        public const int TESUBCASENO = 25;
        public const int ESAZIM = 26;
        public const int ESELEV = 27;
        public const int TEELEV = 28;
        public const int ETELEV = 29;
        public const int TUELEV = 30;
        public const int UTELEV = 31;
        public const int EUELEV = 32;
        public const int EDISCANG = 33;
        public const int TDISCANG = 34;
        public const int ADISC_SET = 35;
        public const int ADISC_UTE = 36;
        public const int TERRHT = 37;
        public const int EARTHHT = 38;
        public const int TVAZIM = 39;
        public const int EVAZIM = 40;
        public const int TVELEV = 41;
        public const int EVELEV = 42;
        public const int TVDISTES = 43;
        public const int TVDISTTU = 44;
        public const int EVDISTES = 45;
        public const int EVDISTTU = 46;
        public const int ANGLEUTV = 47;
        public const int ANGLESEV = 48;
        public const int TSOFFAXIS = 49;
        public const int TSTRUEAZ = 50;
        public const int TSTRUEEL = 51;
        public const int ANGLEUTE = 52;
        public const int ANGLEUTA = 53;
        public const int ANGLEETA = 54;
        public const int ANGLEATV = 55;
        public const int ADISC_ATV = 56;
        public const int TERRAGAIN = 57;
        public const int TERRAMODEL = 58;
        public const int TERRAXREF = 59;
        public const int EARTHAGAIN = 60;
        public const int EARTHAMODEL = 61;
        public const int EARTHAXREF = 62;
        public const int PROCESSED = 63;

        //-------------------------------------------------------------

        public const int INTERFERER_SZ = 2;
        public const int TERRCALL1_SZ = 10;
        public const int TERRCALL2_SZ = 10;
        public const int TERRBNDCDE_SZ = 5;
        public const int EARTHLOCATION_SZ = 11;
        public const int EARTHCALL1_SZ = 10;
        public const int EARTHBAND_SZ = 5;
        public const int TERRACODE_SZ = 13;
        public const int EARTHACODE_SZ = 13;
        public const int SATNAME_SZ = 17;
        public const int SATOPER_SZ = 3;
        public const int INTAUSE_SZ = 4;
        public const int TSOFFAXIS_SZ = 2; /* 'Y' if offaxis */
        public const int TERRAMODEL_SZ = 16;
        public const int TERRAXREF_SZ = 13;
        public const int EARTHAMODEL_SZ = 16;
        public const int EARTHAXREF_SZ = 13;

        //-------------------------------------------------------------

        /*
        interferer
        terrcall1
        terrcall2
        terrbndcde
        terranum
        earthlocation
        earthcall1
        earthband
        terracode
        earthacode
        satname
        satoper
        satlongit
        txpre
        txtro
        rxpre
        rxtro
        sarc1
        sarc2
        intause
        mode1
        mode2
        etreport
        tereport
        etsubcaseno
        tesubcaseno
        esazim
        eselev
        teelev
        etelev
        tuelev
        utelev
        euelev
        ediscang
        tdiscang
        adisc_set
        adisc_ute
        terrht
        earthht
        tvazim
        evazim
        tvelev
        evelev
        tvdistes
        tvdisttu
        evdistes
        evdisttu
        angleutv
        anglesev
        tsoffaxis
        tstrueaz
        tstrueel
        angleute
        angleuta
        angleeta
        angleatv
        adisc_atv
        terragain;
        terramodel
        terraxref
        earthagain;
        earthamodel
        earthaxref
        processed;
        */
        //-------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int n = 0;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "interferer =      " + interferer);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terrcall1 =      " + terrcall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terrcall2 =      " + terrcall2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terrbndcde =      " + terrbndcde);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terranum =      " + terranum);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthlocation =      " + earthlocation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthcall1 =      " + earthcall1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthband =      " + earthband);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terracode =      " + terracode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthacode =      " + earthacode);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satname =      " + satname);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satoper =      " + satoper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "satlongit =      " + satlongit);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txpre =      " + txpre);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "txtro =      " + txtro);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxpre =      " + rxpre);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "rxtro =      " + rxtro);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sarc1 =      " + sarc1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "sarc2 =      " + sarc2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "intause =      " + intause);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mode1 =      " + mode1);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "mode2 =      " + mode2);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "etreport =      " + etreport);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tereport =      " + tereport);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "etsubcaseno =      " + etsubcaseno);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tesubcaseno =      " + tesubcaseno);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "esazim =      " + esazim);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "eselev =      " + eselev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "teelev =      " + teelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "etelev =      " + etelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tuelev =      " + tuelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "utelev =      " + utelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "euelev =      " + euelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ediscang =      " + ediscang);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tdiscang =      " + tdiscang);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adisc_set =      " + adisc_set);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adisc_ute =      " + adisc_ute);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terrht =      " + terrht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthht =      " + earthht);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tvazim =      " + tvazim);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "evazim =      " + evazim);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tvelev =      " + tvelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "evelev =      " + evelev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tvdistes =      " + tvdistes);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tvdisttu =      " + tvdisttu);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "evdistes =      " + evdistes);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "evdisttu =      " + evdisttu);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "angleutv =      " + angleutv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "anglesev =      " + anglesev);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tsoffaxis =      " + tsoffaxis);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tstrueaz =      " + tstrueaz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "tstrueel =      " + tstrueel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "angleute =      " + angleute);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "angleuta =      " + angleuta);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "angleeta =      " + angleeta);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "angleatv =      " + angleatv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "adisc_atv =      " + adisc_atv);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terragain =      " + terragain); ;
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terramodel =      " + terramodel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "terraxref =      " + terraxref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthagain =      " + earthagain); ;
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthamodel =      " + earthamodel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "earthaxref =      " + earthaxref);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "processed =      " + processed); ;

            return sb.ToString();
        }

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of float and/or double into native memory using P/Invoke marshalling.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtr[INTERFERER] = Marshal.StringToHGlobalAnsi(interferer);

            parameterValuePtr[TERRCALL1] = Marshal.StringToHGlobalAnsi(terrcall1);

            parameterValuePtr[TERRCALL2] = Marshal.StringToHGlobalAnsi(terrcall2);

            parameterValuePtr[TERRBNDCDE] = Marshal.StringToHGlobalAnsi(terrbndcde);

            parameterValuePtr[TERRANUM] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TERRANUM], terranum);

            parameterValuePtr[EARTHLOCATION] = Marshal.StringToHGlobalAnsi(earthlocation);

            parameterValuePtr[EARTHCALL1] = Marshal.StringToHGlobalAnsi(earthcall1);

            parameterValuePtr[EARTHBAND] = Marshal.StringToHGlobalAnsi(earthband);

            parameterValuePtr[TERRACODE] = Marshal.StringToHGlobalAnsi(terracode);

            parameterValuePtr[EARTHACODE] = Marshal.StringToHGlobalAnsi(earthacode);

            parameterValuePtr[SATNAME] = Marshal.StringToHGlobalAnsi(satname);

            parameterValuePtr[SATOPER] = Marshal.StringToHGlobalAnsi(satoper);

            parameterValuePtr[SATLONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[SATLONGIT], satlongit);

            parameterValuePtr[TXPRE] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = txpre;
            Marshal.Copy(F, 0, parameterValuePtr[TXPRE], 1);

            parameterValuePtr[TXTRO] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = txtro;
            Marshal.Copy(F, 0, parameterValuePtr[TXTRO], 1);

            parameterValuePtr[RXPRE] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rxpre;
            Marshal.Copy(F, 0, parameterValuePtr[RXPRE], 1);

            parameterValuePtr[RXTRO] = Marshal.AllocHGlobal(sizeof(float));
            F[0] = rxtro;
            Marshal.Copy(F, 0, parameterValuePtr[RXTRO], 1);

            parameterValuePtr[SARC1] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = sarc1;
            Marshal.Copy(D, 0, parameterValuePtr[SARC1], 1);

            parameterValuePtr[SARC2] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = sarc2;
            Marshal.Copy(D, 0, parameterValuePtr[SARC2], 1);

            parameterValuePtr[INTAUSE] = Marshal.StringToHGlobalAnsi(intause);

            parameterValuePtr[MODE1] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[MODE1], mode1);

            parameterValuePtr[MODE2] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[MODE2], mode2);

            parameterValuePtr[ETREPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ETREPORT], etreport);

            parameterValuePtr[TEREPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TEREPORT], tereport);

            parameterValuePtr[ETSUBCASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[ETSUBCASENO], etsubcaseno);

            parameterValuePtr[TESUBCASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[TESUBCASENO], tesubcaseno);

            parameterValuePtr[ESAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = esazim;
            Marshal.Copy(D, 0, parameterValuePtr[ESAZIM], 1); /* earth stn to satellite azimuth */

            parameterValuePtr[ESELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = eselev;
            Marshal.Copy(D, 0, parameterValuePtr[ESELEV], 1); /* earth stn to satellite elevation */

            parameterValuePtr[TEELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = teelev;
            Marshal.Copy(D, 0, parameterValuePtr[TEELEV], 1); /* terr stn to earth stn elev */

            parameterValuePtr[ETELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = etelev;
            Marshal.Copy(D, 0, parameterValuePtr[ETELEV], 1); /* earth stn to terr stn elev */

            parameterValuePtr[TUELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tuelev;
            Marshal.Copy(D, 0, parameterValuePtr[TUELEV], 1); /* terr stn to terr link stn elev */

            parameterValuePtr[UTELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = utelev;
            Marshal.Copy(D, 0, parameterValuePtr[UTELEV], 1); /* terr link stn to terr stn elev */

            parameterValuePtr[EUELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = euelev;
            Marshal.Copy(D, 0, parameterValuePtr[EUELEV], 1); /* earth stn to terr link stn elev */

            parameterValuePtr[EDISCANG] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = ediscang;
            Marshal.Copy(D, 0, parameterValuePtr[EDISCANG], 1); /* earth stn discrimination angle */

            parameterValuePtr[TDISCANG] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tdiscang;
            Marshal.Copy(D, 0, parameterValuePtr[TDISCANG], 1); /* terr stn discrimination angle */

            parameterValuePtr[ADISC_SET] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adisc_set;
            Marshal.Copy(D, 0, parameterValuePtr[ADISC_SET], 1); /* ante. discrim. at angle SET */

            parameterValuePtr[ADISC_UTE] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adisc_ute;
            Marshal.Copy(D, 0, parameterValuePtr[ADISC_UTE], 1); /* ante. discrim. at angle UTE */

            parameterValuePtr[TERRHT] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = terrht;
            Marshal.Copy(D, 0, parameterValuePtr[TERRHT], 1); /* terr stn ht amsl = ground + antenna ht */

            parameterValuePtr[EARTHHT] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = earthht;
            Marshal.Copy(D, 0, parameterValuePtr[EARTHHT], 1); /* earth stn ht amsl = ground + antenna ht */

            parameterValuePtr[TVAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tvazim;
            Marshal.Copy(D, 0, parameterValuePtr[TVAZIM], 1); /* terr stn to rain scatt. vol (rsv) azim */

            parameterValuePtr[EVAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = evazim;
            Marshal.Copy(D, 0, parameterValuePtr[EVAZIM], 1); /* earth stn to rsv azim */

            parameterValuePtr[TVELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tvelev;
            Marshal.Copy(D, 0, parameterValuePtr[TVELEV], 1); /* terr stn to rsv elev */

            parameterValuePtr[EVELEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = evelev;
            Marshal.Copy(D, 0, parameterValuePtr[EVELEV], 1); /* earth stn to rsv elev */

            parameterValuePtr[TVDISTES] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tvdistes;
            Marshal.Copy(D, 0, parameterValuePtr[TVDISTES], 1); /* terr stn to rsv distance for vol on ES vec */

            parameterValuePtr[TVDISTTU] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tvdisttu;
            Marshal.Copy(D, 0, parameterValuePtr[TVDISTTU], 1); /* terr stn to rsv distance for vol on TU vec */

            parameterValuePtr[EVDISTES] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = evdistes;
            Marshal.Copy(D, 0, parameterValuePtr[EVDISTES], 1); /* earth stn to rsv dist for vol on ES vec */

            parameterValuePtr[EVDISTTU] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = evdisttu;
            Marshal.Copy(D, 0, parameterValuePtr[EVDISTTU], 1); /* earth stn to rsv dist for vol on TU vec */

            parameterValuePtr[ANGLEUTV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = angleutv;
            Marshal.Copy(D, 0, parameterValuePtr[ANGLEUTV], 1);

            parameterValuePtr[ANGLESEV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = anglesev;
            Marshal.Copy(D, 0, parameterValuePtr[ANGLESEV], 1);/* Added for support of off-axis angles.  1108 - GJS - 2002.12 */

            parameterValuePtr[TSOFFAXIS] = Marshal.StringToHGlobalAnsi(tsoffaxis);

            parameterValuePtr[TSTRUEAZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tstrueaz;
            Marshal.Copy(D, 0, parameterValuePtr[TSTRUEAZ], 1);

            parameterValuePtr[TSTRUEEL] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tstrueel;
            Marshal.Copy(D, 0, parameterValuePtr[TSTRUEEL], 1);

            parameterValuePtr[ANGLEUTE] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = angleute;
            Marshal.Copy(D, 0, parameterValuePtr[ANGLEUTE], 1);

            parameterValuePtr[ANGLEUTA] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = angleuta;
            Marshal.Copy(D, 0, parameterValuePtr[ANGLEUTA], 1);

            parameterValuePtr[ANGLEETA] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = angleeta;
            Marshal.Copy(D, 0, parameterValuePtr[ANGLEETA], 1);

            parameterValuePtr[ANGLEATV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = angleatv;
            Marshal.Copy(D, 0, parameterValuePtr[ANGLEATV], 1);

            parameterValuePtr[ADISC_ATV] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = adisc_atv;
            Marshal.Copy(D, 0, parameterValuePtr[ADISC_ATV], 1);

            parameterValuePtr[TERRAGAIN] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = terragain;
            Marshal.Copy(D, 0, parameterValuePtr[TERRAGAIN], 1);

            parameterValuePtr[TERRAMODEL] = Marshal.StringToHGlobalAnsi(terramodel);

            parameterValuePtr[TERRAXREF] = Marshal.StringToHGlobalAnsi(terraxref);

            parameterValuePtr[EARTHAGAIN] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = earthagain;
            Marshal.Copy(D, 0, parameterValuePtr[EARTHAGAIN], 1);

            parameterValuePtr[EARTHAMODEL] = Marshal.StringToHGlobalAnsi(earthamodel);

            parameterValuePtr[EARTHAXREF] = Marshal.StringToHGlobalAnsi(earthaxref);

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

            sb.AppendLine("interferer = " + interferer);
            sb.AppendLine("terrcall1 = " + terrcall1);
            sb.AppendLine("terrcall2 = " + terrcall2);
            sb.AppendLine("terrbndcde = " + terrbndcde);
            sb.AppendLine("terranum = " + terranum);
            sb.AppendLine("earthlocation = " + earthlocation);
            sb.AppendLine("earthcall1 = " + earthcall1);
            sb.AppendLine("earthband = " + earthband);
            sb.AppendLine("terracode = " + terracode);
            sb.AppendLine("earthacode = " + earthacode);
            sb.AppendLine("satname = " + satname);
            sb.AppendLine("satoper = " + satoper);
            sb.AppendLine("satlongit = " + satlongit);
            sb.AppendLine("txpre = " + txpre);
            sb.AppendLine("txtro = " + txtro);
            sb.AppendLine("rxpre = " + rxpre);
            sb.AppendLine("rxtro = " + rxtro);
            sb.AppendLine("sarc1 = " + sarc1);
            sb.AppendLine("sarc2 = " + sarc2);
            sb.AppendLine("intause = " + intause);
            sb.AppendLine("mode1 = " + mode1);
            sb.AppendLine("mode2 = " + mode2);
            sb.AppendLine("etreport = " + etreport);
            sb.AppendLine("tereport = " + tereport);
            sb.AppendLine("etsubcaseno = " + etsubcaseno);
            sb.AppendLine("tesubcaseno = " + tesubcaseno);
            sb.AppendLine("esazim = " + esazim);
            sb.AppendLine("eselev = " + eselev);
            sb.AppendLine("teelev = " + teelev);
            sb.AppendLine("etelev = " + etelev);
            sb.AppendLine("tuelev = " + tuelev);
            sb.AppendLine("utelev = " + utelev);
            sb.AppendLine("euelev = " + euelev);
            sb.AppendLine("ediscang = " + ediscang);
            sb.AppendLine("tdiscang = " + tdiscang);
            sb.AppendLine("adisc_set = " + adisc_set);
            sb.AppendLine("adisc_ute = " + adisc_ute);
            sb.AppendLine("terrht = " + terrht);
            sb.AppendLine("earthht = " + earthht);
            sb.AppendLine("tvazim = " + tvazim);
            sb.AppendLine("evazim = " + evazim);
            sb.AppendLine("tvelev = " + tvelev);
            sb.AppendLine("evelev = " + evelev);
            sb.AppendLine("tvdistes = " + tvdistes);
            sb.AppendLine("tvdisttu = " + tvdisttu);
            sb.AppendLine("evdistes = " + evdistes);
            sb.AppendLine("evdisttu = " + evdisttu);
            sb.AppendLine("angleutv = " + angleutv);
            sb.AppendLine("anglesev = " + anglesev);
            sb.AppendLine("tsoffaxis = " + tsoffaxis);
            sb.AppendLine("tstrueaz = " + tstrueaz);
            sb.AppendLine("tstrueel = " + tstrueel);
            sb.AppendLine("angleute = " + angleute);
            sb.AppendLine("angleuta = " + angleuta);
            sb.AppendLine("angleeta = " + angleeta);
            sb.AppendLine("angleatv = " + angleatv);
            sb.AppendLine("adisc_atv = " + adisc_atv);
            sb.AppendLine("terragain = " + terragain);
            sb.AppendLine("terramodel = " + terramodel);
            sb.AppendLine("terraxref = " + terraxref);
            sb.AppendLine("earthagain = " + earthagain);
            sb.AppendLine("earthamodel = " + earthamodel);
            sb.AppendLine("earthaxref = " + earthaxref);
            sb.AppendLine("processed = " + processed);

            return sb.ToString();
        }





    }
}

```
