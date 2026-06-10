# Documented File: Ax12.cs
**Repository Path:** `_Auxlib\Ax12.cs`
**Primary Layer:** `_Auxlib`
**Namespace:** `_Auxlib`

## Source Code Representation
```csharp
﻿using _Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace _Auxlib
{
    public class Ax12
    {
        private static double[] discTable = new double[Constant.PAT_ROWS] {
                                                        13.5, 17.9, 20.8, 23.0, 24.8, 26.2,
                                                        27.4, 28.5, 29.5, 30.4, 31.2, 31.9,
                                                        32.5, 33.2, 33.7, 34.3, 34.8, 35.3,
                                                        35.7, 36.2, 36.6, 37.0, 37.4, 37.7,
                                                        38.1, 38.4, 38.7, 39.0, 39.3, 39.6,
                                                        39.9, 40.2, 40.4, 40.7, 40.9, 41.2,
                                                        41.4, 41.7, 41.9, 42.1, 0, 0
        };

        /// <summary>
        /// This method calculates the passive pattern envelopes.
        /// </summary>
        /// <param name="width">  - width of the passive</param>
        /// <param name="incAng">  - included angle</param>
        /// <param name="freqMHz"> - frequency in MHz</param>
        /// <param name="gain"> - far-field gain</param>
        /// <param name="patVals"> - table of pattern values</param>
        /// <param name="beamWidth"> - beam width</param>
        /// <returns></returns>
        public static int AxPasPat(double width,            /* input  - width of the passive */
                                    double incAng,          /* input  - included angle */
                                    double freqMHz,         /* input  - frequency in MHz */
                                    double gain,            /* input  - far-field gain */
                                    out double[,] patVals,  /* output - table of pattern values */
                                    out double beamWidth)   /* output - beam width */
        {
            // 'out' requirements.
            beamWidth = 0.0;
            patVals = new double[Constant.PAT_ROWS, 2];

            int i;      /* loop control variable */
            bool greater90 = false;  /* value greater than 90 flag */
            bool discGtDiscrMax = false; /* value flag */
            double varA;        /* work variable A */
            double efwlog;     /* log (base 10) or variable A */
            double freqLog;    /* log (base 10) of frequency  */
            double varAFreq;   /* product of VarA * frequency */
            double discrim;    /* discrimination value	*/
            double discrMax;   /* maximum discrimination */
            double holdVar1;   /* work variable */
            double holdVar2;   /* work variable */
            double temp;       /* work variable; 2 times i */


            varA = width * Cos(incAng * Constant.MAGICNUM1);
            if (varA <= 0.0)
            {
                varA = 1.0e-10;
            }

            efwlog = Log10(varA);
            freqLog = Log10(freqMHz);
            varAFreq = varA * freqMHz;
            discrim = (20.0 * efwlog) + (20.0 * freqLog) - Constant.MAGICNUM2;

            /* discrMax is the lesser of discrimination and gain/2  */
            discrMax = (discrim < (gain / 2.0)) ? discrim : gain / 2.0;

            holdVar1 = Constant.MAGICNUM3 / varAFreq;
            holdVar2 = Constant.MAGICNUM4 / varAFreq;
            if (Abs((int)holdVar2) > 1)
            {
                /* Area or Frequency too small for Pattern */
                return (Constant.PAT_AF_FAIL);
            }

            beamWidth = 2.0 * Constant.RAD_TO_DEG * Asin(holdVar1);

            for (i = 0; i < Constant.DISCLIMIT; i++)
            {
                temp = 2.0 * (i + 1.0);
                patVals[i, 0] = Constant.RAD_TO_DEG * Asin((temp + 1.0) * Constant.MAGICNUM5 /
                         varAFreq);
                if (patVals[i, 0] > 90.0)
                {
                    greater90 = true;
                    break;
                }
                discrim = discTable[i];
                if (discrim > discrMax)
                {
                    discGtDiscrMax = true;
                    break;
                }
                else
                {
                    patVals[i, 1] = discrim;
                }
            }

            /*  do the maximum discrimination angle if less than 90 degrees  */
            /*  and the for loop executes the maximum number of times	*/
            if ((greater90 == false) && (discGtDiscrMax == false))
            {
                patVals[i, 0] = 2.5 * (discrMax - discrim) + patVals[i - 1, 0];
                patVals[i, 1] = discrMax;
                i++;
            }

            /*if discrimination is > maximum discrimination, set the discrimin-   */
            /*ation to the maximum and output the angle and the discrimination.   */
            if (discGtDiscrMax == true)
            {
                patVals[i, 1] = discrMax;
                i++;
            }

            /*if off axis angle is > 90 , set it to 90 and recalculate the dis-   */
            /*crimination angle.						  */
            if (greater90 == true)
            {
                patVals[i, 0] = 90.0;
                patVals[i, 1] = discrMax;
                i++;
            }

            /* Do the work for 180 degrees as the last off axis angle  */
            patVals[i, 0] = 180.0;
            patVals[i, 1] = discrMax;

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method formats and writes a text report of the results provided 
        /// by a previous call to Ax12.AxPasPat().
        /// </summary>
        /// <param name="tW"> - TextWriter object for output file</param>
        /// <param name="pasName"> - passive name</param>
        /// <param name="width"> - passive width</param>
        /// <param name="freqMHz"> - frequency in MHz</param>
        /// <param name="beamWidth"> - beam width</param>
        /// <param name="gain"> - far-field gain</param>
        /// <param name="incAng"> - included angle</param>
        /// <param name="patVals"> - table of pattern values</param>
        /// <param name="err"> - return value of previous call to Ax12.AxPasPat()</param>
        public static void AxRptPat(TextWriter tW,      /* input - TextWriter object for output file */
                                    string pasName,     /* input - passive name */
                                    double width,       /* input - passive width */
                                    double freqMHz,     /* input - frequency in MHz */
                                    double beamWidth,   /* input - beam width */
                                    double gain,        /* input - far-field gain */
                                    double incAng,      /* input - included angle */
                                    double[,] patVals,  /* input - table of pattern values */
                                    int err)            /* input - message number for report */
        {
            int i;    /* loop control variable */


            tW.Write("\r\n\r\n\r\n                       PATTERN FOR PASSIVE AT {0}\r\n\r\n", pasName);
            tW.Write("Width:         {0,8:F2} M\r\n", width);
            tW.Write("Frequency:     {0,8:F2} MHz\r\n", freqMHz);
            tW.Write("3 dB Beamwidth {0,8:F2} Deg\r\n", beamWidth);
            tW.Write("Far-Field Gain {0,8:F2} dBi\r\n", gain);
            tW.Write("Included Angle {0,8:F2} Deg\r\n\r\n", incAng);
            if (err == Constant.PAT_AF_FAIL)
            {
                tW.Write("   Error calculating Far-Field Pattern\n");
                tW.Write("      Area or Frequency too small for Pattern\r\n");
            }
            else
            {
                tW.Write("              Far-Field Pattern:\r\n");
                tW.Write("   Off-Axis Angle            Discrimination\r\n");
                tW.Write("       in Deg                     in dB\r\n");
                for (i = 0; i < Constant.PAT_ROWS; i++)
                {
                    tW.Write("     {0,6:F1}                     {0,6:F1}\r\n",
                        patVals[i, 0], patVals[i, 1]);
                }
            }
        }




    }
}

```
