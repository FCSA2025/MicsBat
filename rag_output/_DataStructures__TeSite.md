# Documented File: TeSite.cs
**Repository Path:** `_DataStructures\TeSite.cs`
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
    using SQLLEN = Int64;

    /// <summary>
    /// This class has fields that are isomorphic with DB tables of the type: 
    /// <b>&lt;userID&gt;.te_&lt;pdfName&gt;_site</b>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TeSite
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRCALL1_SZ)]
        public string terrcall1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRCALL2_SZ)]
        public string terrcall2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHLOCATION_SZ)]
        public string earthlocation;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRNAME1_SZ)]
        public string terrname1;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERRNAME2_SZ)]
        public string terrname2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHNAME_SZ)]
        public string earthname;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERROPER_SZ)]
        public string terroper;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = TERROPER2_SZ)]
        public string terroper2;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = EARTHOPER_SZ)]
        public string earthoper;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int terrlatit;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int terrlongit;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double terrgrnd;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int earthlatit;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int earthlongit;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double earthgrnd;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = RADIOZONE_SZ)]
        public string radiozone;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short rainzone;



        /* calculated fields */
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short etreport;
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short tereport;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int etcaseno;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int tecaseno;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int etsubcases;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int tesubcases;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = INTREQ_SZ)]
        public string intreq;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double etdist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double etazim;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double teazim;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tudist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double tuazim;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double utazim;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double eudist;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double euazim;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double ueazim;
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int processed;

        //--------------------------------------------------------------------------

        public const int NUM_COLUMNS = 35;  // Includes 'missing' column 9.

        //--------------------------------------------------------------------------

        // Member variable vs index (starting at zero).
        public const int TERRCALL1 = 0;
        public const int TERRCALL2 = 1;
        public const int EARTHLOCATION = 2;
        public const int TERRNAME1 = 3;
        public const int TERRNAME2 = 4;
        public const int EARTHNAME = 5;
        public const int TERROPER = 6;
        public const int TERROPER2 = 7;
        public const int EARTHOPER = 8;
        // There is no index [9] !
        public const int TERRLATIT = 10;
        public const int TERRLONGIT = 11;
        public const int TERRGRND = 12;
        public const int EARTHLATIT = 13;
        public const int EARTHLONGIT = 14;
        public const int EARTHGRND = 15;
        public const int RADIOZONE = 16;
        public const int RAINZONE = 17;
        public const int ETREPORT = 18;
        public const int TEREPORT = 19;
        public const int ETCASENO = 20;
        public const int TECASENO = 21;
        public const int ETSUBCASES = 22;
        public const int TESUBCASES = 23;
        public const int INTREQ = 24;
        public const int ETDIST = 25;
        public const int ETAZIM = 26;
        public const int TEAZIM = 27;
        public const int TUDIST = 28;
        public const int TUAZIM = 29;
        public const int UTAZIM = 30;
        public const int EUDIST = 31;
        public const int EUAZIM = 32;
        public const int UEAZIM = 33;
        public const int PROCESSED = 34;
        public const int SIZE_ = 35;       // What is this for?

        // Maximum string sizes for fetch/insert/update to/from the DB.
        public const int TERRCALL1_SZ = 10;
        public const int TERRCALL2_SZ = 10;
        public const int EARTHLOCATION_SZ = 11;
        public const int TERRNAME1_SZ = 33;
        public const int TERRNAME2_SZ = 33;
        public const int EARTHNAME_SZ = 17;
        public const int TERROPER_SZ = 7;
        public const int TERROPER2_SZ = 7;
        public const int EARTHOPER_SZ = 7;
        public const int RADIOZONE_SZ = 3;
        public const int INTREQ_SZ = 5;

        //----------------------------------------------------------------------------

        /// <summary>
        /// For each field of this object, the method allocates a type-specific amount
        /// of global (heap) memory, copies the value of the field into it, and returns
        /// an IntPtr[] containing all of the start addresses.
        /// </summary>
        /// <returns></returns>
        public SQLPOINTER[] CopyToArrayOfSQLPOINTERs()
        {
            //Can only copy arrays of double into native memory using Marshal method.
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtr = new SQLPOINTER[NUM_COLUMNS];

            // Write member values to unmanaged memory, one by one.

            parameterValuePtr[TERRCALL1] = Marshal.StringToHGlobalAnsi(terrcall1);

            parameterValuePtr[TERRCALL2] = Marshal.StringToHGlobalAnsi(terrcall2);

            parameterValuePtr[EARTHLOCATION] = Marshal.StringToHGlobalAnsi(earthlocation);

            parameterValuePtr[TERRNAME1] = Marshal.StringToHGlobalAnsi(terrname1);

            parameterValuePtr[TERRNAME2] = Marshal.StringToHGlobalAnsi(terrname2);

            parameterValuePtr[EARTHNAME] = Marshal.StringToHGlobalAnsi(earthname);

            parameterValuePtr[TERROPER] = Marshal.StringToHGlobalAnsi(terroper);

            parameterValuePtr[TERROPER2] = Marshal.StringToHGlobalAnsi(terroper2);

            parameterValuePtr[EARTHOPER] = Marshal.StringToHGlobalAnsi(earthoper);

            parameterValuePtr[TERRLATIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[TERRLATIT], terrlatit);

            parameterValuePtr[TERRLONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[TERRLONGIT], terrlongit);

            parameterValuePtr[TERRGRND] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = terrgrnd;
            Marshal.Copy(D, 0, parameterValuePtr[TERRGRND], 1);

            parameterValuePtr[EARTHLATIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[EARTHLATIT], earthlatit);

            parameterValuePtr[EARTHLONGIT] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[EARTHLONGIT], earthlongit);

            parameterValuePtr[EARTHGRND] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = earthgrnd;
            Marshal.Copy(D, 0, parameterValuePtr[EARTHGRND], 1);

            parameterValuePtr[RADIOZONE] = Marshal.StringToHGlobalAnsi(radiozone);

            parameterValuePtr[RAINZONE] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[RAINZONE], rainzone);

            parameterValuePtr[ETREPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[ETREPORT], etreport);

            parameterValuePtr[TEREPORT] = Marshal.AllocHGlobal(sizeof(short));
            Marshal.WriteInt16(parameterValuePtr[TEREPORT], tereport);

            parameterValuePtr[ETCASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[ETCASENO], etcaseno);

            parameterValuePtr[TECASENO] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[TECASENO], tecaseno);

            parameterValuePtr[ETSUBCASES] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[ETSUBCASES], etsubcases);

            parameterValuePtr[TESUBCASES] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[TESUBCASES], tesubcases);

            parameterValuePtr[INTREQ] = Marshal.StringToHGlobalAnsi(intreq);

            parameterValuePtr[ETDIST] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = etdist;
            Marshal.Copy(D, 0, parameterValuePtr[ETDIST], 1);

            parameterValuePtr[ETAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = etazim;
            Marshal.Copy(D, 0, parameterValuePtr[ETAZIM], 1);

            parameterValuePtr[TEAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = teazim;
            Marshal.Copy(D, 0, parameterValuePtr[TEAZIM], 1);

            parameterValuePtr[TUDIST] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tudist;
            Marshal.Copy(D, 0, parameterValuePtr[TUDIST], 1);

            parameterValuePtr[TUAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = tuazim;
            Marshal.Copy(D, 0, parameterValuePtr[TUAZIM], 1);

            parameterValuePtr[UTAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = utazim;
            Marshal.Copy(D, 0, parameterValuePtr[UTAZIM], 1);

            parameterValuePtr[EUDIST] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = eudist;
            Marshal.Copy(D, 0, parameterValuePtr[EUDIST], 1);

            parameterValuePtr[EUAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = euazim;
            Marshal.Copy(D, 0, parameterValuePtr[EUAZIM], 1);

            parameterValuePtr[UEAZIM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = ueazim;
            Marshal.Copy(D, 0, parameterValuePtr[UEAZIM], 1);

            parameterValuePtr[PROCESSED] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtr[PROCESSED], processed);

            return parameterValuePtr;
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

            sb.Append("\nterrcall1:   " + nullInds[TERRCALL1] + "  :  " + terrcall1);
            sb.Append("\nterrcall2:   " + nullInds[TERRCALL2] + "  :  " + terrcall2);
            sb.Append("\nearthlocation:   " + nullInds[EARTHLOCATION] + "  :  " + earthlocation);
            sb.Append("\nterrname1:   " + nullInds[TERRNAME1] + "  :  " + terrname1);
            sb.Append("\nterrname2:   " + nullInds[TERRNAME2] + "  :  " + terrname2);
            sb.Append("\nearthname:   " + nullInds[EARTHNAME] + "  :  " + earthname);
            sb.Append("\nterroper:   " + nullInds[TERROPER] + "  :  " + terroper);
            sb.Append("\nterroper2:   " + nullInds[TERROPER2] + "  :  " + terroper2);
            sb.Append("\nearthoper:   " + nullInds[EARTHOPER] + "  :  " + earthoper);
            sb.Append("\nterrlatit:   " + nullInds[TERRLATIT] + "  :  " + terrlatit);
            sb.Append("\nterrlongit:   " + nullInds[TERRLONGIT] + "  :  " + terrlongit);
            sb.Append("\nterrgrnd:   " + nullInds[TERRGRND] + "  :  " + terrgrnd);
            sb.Append("\nearthlatit:   " + nullInds[EARTHLATIT] + "  :  " + earthlatit);
            sb.Append("\nearthlongit:   " + nullInds[EARTHLONGIT] + "  :  " + earthlongit);
            sb.Append("\nearthgrnd:   " + nullInds[EARTHGRND] + "  :  " + earthgrnd);
            sb.Append("\nradiozone:   " + nullInds[RADIOZONE] + "  :  " + radiozone);
            sb.Append("\nrainzone:   " + nullInds[RAINZONE] + "  :  " + rainzone);
            sb.Append("\netreport:   " + nullInds[ETREPORT] + "  :  " + etreport);
            sb.Append("\ntereport:   " + nullInds[TEREPORT] + "  :  " + tereport);
            sb.Append("\netcaseno:   " + nullInds[ETCASENO] + "  :  " + etcaseno);
            sb.Append("\ntecaseno:   " + nullInds[TECASENO] + "  :  " + tecaseno);
            sb.Append("\netsubcases:   " + nullInds[ETSUBCASES] + "  :  " + etsubcases);
            sb.Append("\ntesubcases:   " + nullInds[TESUBCASES] + "  :  " + tesubcases);
            sb.Append("\nintreq:   " + nullInds[INTREQ] + "  :  " + intreq);
            sb.Append("\netdist:   " + nullInds[ETDIST] + "  :  " + etdist);
            sb.Append("\netazim:   " + nullInds[ETAZIM] + "  :  " + etazim);
            sb.Append("\nteazim:   " + nullInds[TEAZIM] + "  :  " + teazim);
            sb.Append("\ntudist:   " + nullInds[TUDIST] + "  :  " + tudist);
            sb.Append("\ntuazim:   " + nullInds[TUAZIM] + "  :  " + tuazim);
            sb.Append("\nutazim:   " + nullInds[UTAZIM] + "  :  " + utazim);
            sb.Append("\neudist:   " + nullInds[EUDIST] + "  :  " + eudist);
            sb.Append("\neuazim:   " + nullInds[EUAZIM] + "  :  " + euazim);
            sb.Append("\nueazim:   " + nullInds[UEAZIM] + "  :  " + ueazim);
            sb.Append("\nprocessed:   " + nullInds[PROCESSED] + "  :  " + processed);

            return sb.ToString();
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

            sb.Append("\nterrcall1 = " + terrcall1);
            sb.Append("\nterrcall2 = " + terrcall2);
            sb.Append("\nearthlocation = " + earthlocation);
            sb.Append("\nterrname1 = " + terrname1);
            sb.Append("\nterrname2 = " + terrname2);
            sb.Append("\nearthname = " + earthname);
            sb.Append("\nterroper = " + terroper);
            sb.Append("\nterroper2 = " + terroper2);
            sb.Append("\nearthoper = " + earthoper);
            sb.Append("\nterrlatit = " + terrlatit);
            sb.Append("\nterrlongit = " + terrlongit);
            sb.Append("\nterrgrnd = " + terrgrnd);
            sb.Append("\nearthlatit = " + earthlatit);
            sb.Append("\nearthlongit = " + earthlongit);
            sb.Append("\nearthgrnd = " + earthgrnd);
            sb.Append("\nradiozone = " + radiozone);
            sb.Append("\nrainzone = " + rainzone);
            sb.Append("\netreport = " + etreport);
            sb.Append("\ntereport = " + tereport);
            sb.Append("\netcaseno = " + etcaseno);
            sb.Append("\ntecaseno = " + tecaseno);
            sb.Append("\netsubcases = " + etsubcases);
            sb.Append("\ntesubcases = " + tesubcases);
            sb.Append("\nintreq = " + intreq);
            sb.Append("\netdist = " + etdist);
            sb.Append("\netazim = " + etazim);
            sb.Append("\nteazim = " + teazim);
            sb.Append("\ntudist = " + tudist);
            sb.Append("\ntuazim = " + tuazim);
            sb.Append("\nutazim = " + utazim);
            sb.Append("\neudist = " + eudist);
            sb.Append("\neuazim = " + euazim);
            sb.Append("\nueazim = " + ueazim);
            sb.Append("\nprocessed = " + processed);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a multi-line formatted string that presents the values
        /// of this TeSite object's terrcall1, terrcall2, earthlocation, etcaseno and
        /// tecaseno members.
        /// </summary>
        /// <returns></returns>
        public string ToStringA()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nterrcall1     = " + terrcall1);
            sb.Append("; terrcall2     = " + terrcall2);
            sb.Append("; earthlocation = " + earthlocation);
            sb.Append("; etcaseno      = " + etcaseno);
            sb.Append("; tecaseno      = " + tecaseno);

            return sb.ToString();
        }


    }
}

```
