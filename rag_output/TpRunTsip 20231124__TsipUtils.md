# Documented File: TsipUtils.cs
**Repository Path:** `TpRunTsip 20231124\TsipUtils.cs`
**Primary Layer:** `TpRunTsip 20231124`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// Provides methods that perform 'utility' functions that are
    /// unique to TpRunTsip.
    /// </summary>
    public class TsipUtils
    {

        /// <summary>
        /// This method inserts a populated TpParm object into a parameter table in the DB.  
        /// </summary>
        /// <param name="tableName"> - full SQL Server DB table name.</param>
        /// <param name="parmStruct"> - populated TpParm object.</param>
        /// <param name="parmNulls"> - ODBC nullInds associated with parmStruct.</param>
        /// <returns></returns>
        public static int UtInsertParmRecord(string tableName, TpParm parmStruct, SQLLEN[] parmNulls)
        {

            int parmHandle;
            int rc;

            /* insert TSIP parm record information */
            if ((parmHandle = TpDynParm.TpSelectParm(tableName, "", "")) < 0)
            {
                return (parmHandle);
            }

            rc = TpDynParm.TpInsertParm(parmHandle, parmStruct, parmNulls);


            TpDynParm.TpCloseParm(parmHandle);
            return (rc);
        }

        /// <summary>
        /// This method constructs the operator code or call sign selection criteria 
        /// from the specified codes; this can then be used as part of the 'WHERE' 
        /// clause of an SQL query.  
        /// </summary>
        /// <param name="parmStruct"> - TpParm object.</param>
        /// <param name="selection"> - part of the 'WHERE' clause of an SQL query </param>
        public static void UtOpCodesCallSigns(TpParm parmStruct, out string selection)
        {
            //...Log2.v("\nTsipUtils.UtOpCodesCallSigns(): Entry");

            string codeList;
            string oneSelection;
            string aCode;
            int rc;
            int i;

            selection = "";      //	Empty the string.

            codeList = parmStruct.codes;

            if ((parmStruct.selsites.Equals("CALL SIGN")) &&
                    (!parmStruct.envtype.Equals("MDB_ES")))
            {
                i = 0;
                // Initialize the GenUtil.UtGetInputString buffer and return first poke.
                rc = GenUtil.UtGetInputString(codeList, out aCode, 9);
                while (rc >= 0)
                {
                    if (i == 0)
                    {
                        oneSelection = String.Format(" and (call1 = '{0}'", aCode.PadRight(9));
                        i++;
                    }
                    else
                    {
                        oneSelection = String.Format(" or call1 = '{0}'", aCode.PadRight(9));
                    }
                    selection += oneSelection;
                    // Subsequent pokes from the GenUtil.UtGetInputString buffer.
                    rc = GenUtil.UtGetInputString(null, out aCode, 9);
                }
                if (i > 0)
                {
                    selection += ")";
                }
            }

            if (parmStruct.selsites.Equals("OPERATOR CODE"))
            {
                i = 0;
                // First poke; non-null first argument.
                rc = GenUtil.UtGetInputString(codeList, out aCode, 6);
                while (rc >= 0)
                {
                    if (i == 0)
                    {
                        oneSelection = String.Format(" and (oper = '{0}'", aCode.PadRight(6));
                        i++;
                    }
                    else
                    {
                        oneSelection = String.Format(" or oper = '{0}'", aCode.PadRight(6));
                    }
                    selection += oneSelection;
                    // Subsequent pokes; null first arguments.
                    rc = GenUtil.UtGetInputString(null, out aCode, 6);
                }
                if (i > 0)
                {
                    selection += ")";
                }
            }

            //...Log2.v("\nTsipUtils.UtOpCodesCallSigns(): Exit: selection = " + selection);
        }

        /// <summary>
        /// This determines whether the interferer's band, or its adjacent bands, are 
        /// used by the victim.  
        /// </summary>
        /// <param name="intBand"> - interferer's band code ID.</param>
        /// <param name="vicBand"> - victim's band code ID.</param>
        /// <returns></returns>
        public static int UtAnteAdjBands(string intBand, string vicBand)
        {
            string adjBands;
            SdBand curBand;
            string cIntBand;
            string cVicBand;

            cIntBand = intBand.Trim();
            cVicBand = vicBand.Trim();

            /* check to see if its the same band */
            if (cIntBand.Equals(cVicBand))
            {
                return (Constant.SUCCESS);
            }

            /* get the interferer's adjacent bands */
            if (UtGetBand(cIntBand, out curBand) != Constant.SUCCESS)
            {
                /* no such band code exists */
                ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, intBand);
                return (Constant.FAILURE);
            }
            adjBands = curBand.badj;

            /* check each of the interferer's adjacent bands */
            string[] tokens = adjBands.Split(Constant.ADJ_BAND_DELIM_STR[0]);

            foreach (string token in tokens)
            {
                if (token.Trim().Equals(cVicBand))
                {
                    return (Constant.SUCCESS);
                }
            }

            return (Constant.FAILURE);
        }

        /// <summary>
        /// This method returns an SdBand object populated with column values from the DB table
        /// "main.sd_band" for the record with a prescribed bndcde. The method returns 0 if the
        /// record was found; a non-zero return value indicates failure.  
        /// </summary>
        /// <remarks>
        /// This method simply makes a pass-through call to Suutils.SdGetBand(). Consequently, 
        /// the need for this method in TsipUtils could be eliminated.
        /// </remarks>
        /// <param name="bndCde"> - prescribed bndcde.</param>
        /// <param name="curBand"> - populated SdBand object.</param>
        /// <returns></returns>
        public static int UtGetBand(string bndCde, out SdBand curBand)
        {
            return Suutils.SdGetBand(bndCde, out curBand);
        }

        /// <summary>
        /// Creates a populated SdBand object that corresponds to a precribed band bit.
        /// </summary>
        /// <remarks>
        /// This method simply makes a pass-through call to Suutils.SdGetBandfromBit(). Consequently, 
        /// the need for this method in TsipUtils could be eliminated.
        /// </remarks>
        /// <param name="bndBitPos"> - prescribed bit position.</param>
        /// <param name="curBand"> - populated SdBand object.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int UtGetBandBit(short bndBitPos, out SdBand curBand)
        {
            return Suutils.SdGetBandfromBit(bndBitPos, out curBand);
        }

    }
}

```
