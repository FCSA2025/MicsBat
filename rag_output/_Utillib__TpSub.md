# Documented File: TpSub.cs
**Repository Path:** `_Utillib\TpSub.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Utillib
{
    public class TpSub
    {
        /// <summary>
        /// This method takes latitude in seconds and calulates degrees,.  
        /// </summary>
        /// <param name="latit"></param>
        /// <param name="latSens"></param>
        /// <param name="latDeg"></param>
        /// <param name="latMin"></param>
        /// <param name="latSec"></param>
        public static void TpLoadLat(int latit,
           out string latSens,
           out int latDeg,
           out int latMin,
           out int latSec)
        {
            int tmpLat, deg, rest, xmin, sec;

            if (latit > 0)
            {
                latSens = Constant.NORTH;
            }
            else
            {
                latSens = Constant.SOUTH;
            }

            tmpLat = latit / 100;
            deg = tmpLat / 3600;
            rest = tmpLat - (deg * 3600);
            xmin = rest / 60;
            sec = rest - (xmin * 60);

            latDeg = (int)(deg);
            latMin = (int)(xmin);
            latSec = (int)(sec);
        }

        /// <summary>
        /// This method takes longitude in seconds and calulates degrees, minutes, 
        /// seconds, and sense.  
        /// </summary>
        /// <param name="longit"></param>
        /// <param name="longSens"></param>
        /// <param name="longDeg"></param>
        /// <param name="longMin"></param>
        /// <param name="longSec"></param>
        public static void TpLoadLong(int longit,
                                        out string longSens,
                                        out int longDeg,
                                        out int longMin,
                                        out int longSec)
        {
            int tmpLong, deg, rest, xmin, sec;

            if (longit > 0)
            {
                longSens = Constant.WEST;
            }
            else
            {
                longSens = Constant.EAST;
            }

            tmpLong = longit / 100;
            deg = tmpLong / 3600;
            rest = tmpLong - (deg * 3600);
            xmin = rest / 60;
            sec = rest - (xmin * 60);

            longDeg = (int)(deg);
            longMin = (int)(xmin);
            longSec = (int)(sec);
        }

        /// <summary>
        /// Performs a binary search for a given value in given array.  
        /// </summary>
        /// <remarks>
        /// NOTE: the array must be sorted in ascending order. Notes: If the given 
        /// value itself is found in the array its position in the array is returned in 
        /// both hi and lo. Otherwise hi and lo are set to the positions between which 
        /// the value would be inserted in order to maintain an ascending sorted list. 
        /// If the search value is less than the first in the sorted list, hi and lo 
        /// are set to 0, the first element. If the value is greater than the last in 
        /// the sorted list, hi and lo are set to the last element.  
        /// </remarks>
        /// <param name="pattern"> - patern aray as described above</param>
        /// <param name="numPts"> - number of rows in array</param>
        /// <param name="angle"> - angle to find values at</param>
        /// <param name="cov"> - calcd value of vertCopolar</param>
        /// <param name="csv"> - calcd value of vertXpolar</param>
        /// <param name="coh"> - calcd value of horzCopolar</param>
        /// <param name="csh"> - calcd value of horzXpolar</param>
        public static void TpFindDisc(float[,] pattern, /* input  - patern aray as described above	*/
                                int numPts,             /* input  - number of rows in array					*/
                                float angle,                /* input  - angle to find values at					*/
                                out float cov,                 /* output - calcd value of vertCopolar			*/
                                out float csv,                 /* output - calcd value of vertXpolar				*/
                                out float coh,                 /* output - calcd value of horzCopolar			*/
                                out float csh)                 /* output - calcd value of horzXpolar				*/
        {
            int i, hi, lo;
            float[] angs = new float[Constant.MAX_ANT_PTS];


            /* load array angs with angles from pattern array for
             *	binary search routine
             */
            for (i = 0; i < numPts; i++)
            {
                angs[i] = pattern[i, 0];
            }

            /* perform binary search to find place in array */
            TpGetDat.BinSearch(angs, numPts, angle, out hi, out lo);

            /* interpolate values to calc vertical copolar value */
            GenUtil.Interp(angle, pattern[lo, 0], pattern[hi, 0], pattern[lo, 1],
                pattern[hi, 1], out cov);

            /* interpolate values to calc vertical crosspolar value */
            GenUtil.Interp(angle, pattern[lo, 0], pattern[hi, 0], pattern[lo, 2],
                pattern[hi, 2], out csv);

            /* interpolate values to calc horizontal copolar value */
            GenUtil.Interp(angle, pattern[lo, 0], pattern[hi, 0], pattern[lo, 3],
                pattern[hi, 3], out coh);

            /* interpolate values to calc horizontal crosspolar value */
            GenUtil.Interp(angle, pattern[lo, 0], pattern[hi, 0], pattern[lo, 4],
                pattern[hi, 4], out csh);

            return;
        }
        /// <summary>
        /// This method calulates R, ab, and lr values based on rainZone and both 
        /// frequencies.  
        /// </summary>
        /// <param name="rainzone"></param>
        /// <param name="freq1Mhz"></param>
        /// <param name="freq2Mhz"></param>
        /// <param name="R"></param>
        /// <param name="ab"></param>
        /// <param name="lr"></param>
        /// <returns></returns>
        public static int TePropValues(short rainzone,
          double freq1Mhz,
          double freq2Mhz,
          out double R,
          out double ab,
          out double lr)
        {
            double[] constA = new double[9] { 1.78, 2.33, 2.72, 3.12, 3.58, 4.27, 5.11, 5.79, 7.01 };
            double[] raRate = new double[9] { 8.0, 12.0, 15.0, 19.0, 22.0, 28.0, 30.0, 43.0, 63.0 };
            double A, freq1Ghz, freq2Ghz;
            int nRet = 0;

            if (rainzone < 1 || rainzone > 9)
            {
                /*  Rainzone is out of range.  Assume it is 9 */
                rainzone = 9;
                nRet = -1;
            }
            freq1Ghz = freq1Mhz / 1000.0;
            freq2Ghz = freq2Mhz / 1000.0;
            R = raRate[rainzone - 1];
            A = constA[rainzone - 1];

            if (freq1Ghz < 10.0)
            {
                ab = 0.0;
                if (freq1Ghz < 1.0)
                {
                    lr = 0.0;
                }
                else
                {
                    lr = A * Pow(10.0, -3.0) * Pow(freq2Ghz, 3.2);
                }
            }
            else
            {
                ab = 0.005 * Pow(R, 0.4) * Pow(freq2Ghz - 10.0, 1.7);
                if (freq1Ghz == 10.0)
                {
                    lr = A * Pow(10.0, -3.0) * Pow(freq2Ghz, 3.2);
                }
                else
                {
                    lr = A * Pow(10.0, -2.0) * Pow(freq2Ghz, 2.2);
                }
            }
            return nRet;
        }

        /// <summary>
        /// This is the module that creates the interferer and victim tableNames based 
        /// on the tsip parameter file name and the table type.  
        /// </summary>
        /// <param name="interferer"></param>
        /// <param name="parmStruct"></param>
        /// <param name="intTableName"></param>
        /// <param name="vicTableName"></param>
        /// <param name="isMDB"></param>
        public static void TtTableName(string interferer,
                                        TpParm parmStruct,
                                        out string intTableName,
                                        out string vicTableName,
                                        out bool isMDB)
        {
            // 'out' requirement.
            isMDB = false;
            intTableName = "";
            vicTableName = "";

            parmStruct.envtype.Trim();
            if (Strings.FirstCharIs(interferer, 'P'))
            {
                intTableName = parmStruct.proname;

                if (parmStruct.envtype.Equals("MDB_TS"))
                {
                    isMDB = true;
                    vicTableName = "mt_";
                }
                else if (parmStruct.envtype.Equals("PDF_TS"))
                {
                    isMDB = false;
                    //GenUtil.UtCvtName(Constant.tabType, parmStruct.envname, out vicTableName);
                    vicTableName = parmStruct.envname;
                }
                else
                {
                    isMDB = false;
                    //GenUtil.UtCvtName(Constant.tabType, parmStruct.proname, out vicTableName);
                    vicTableName = parmStruct.proname;
                }
            }
            else
            {
                vicTableName = parmStruct.proname;

                if (parmStruct.envtype.Equals("MDB_TS"))
                {
                    isMDB = true;
                    intTableName = "mt_";
                }
                else if (parmStruct.envtype.Equals("PDF_TS"))
                {
                    isMDB = false;
                    //GenUtil.UtCvtName(Constant.tabType, parmStruct.envname, out intTableName);
                    intTableName = parmStruct.envname;
                }
                else
                {
                    isMDB = false;
                    //GenUtil.UtCvtName(Constant.tabType, parmStruct.proname, out intTableName);
                    intTableName = parmStruct.proname;
                }
            }
        }

    }
}

```
