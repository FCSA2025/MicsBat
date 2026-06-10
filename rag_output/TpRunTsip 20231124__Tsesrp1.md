# Documented File: Tsesrp1.cs
**Repository Path:** `TpRunTsip 20231124\Tsesrp1.cs`
**Primary Layer:** `TpRunTsip 20231124`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using _DataStructures;
using _Utillib;
using _NewLib;

namespace TpRunTsip
{
    /// <summary>
    /// Provides methods used to generate the Study Report (.STUDY) for Ts-Es.
    /// </summary>
    public class Tsesrp1
    {
        /// <summary>
        /// This method generates the Study Report fragment  for a single case of Ts-Es interference.
        /// </summary>
        /// <param name="tw"> - the TextWriter object assigned to the .STUDY report.</param>
        /// <param name="tpParm"> - TpParm object containing user-defined parameters.</param>
        /// <param name="cTableName"> - the unique ID substring of the Tt DB table names.</param>
        /// <param name="p"> - see the source code for context.</param>
        /// <param name="q"> - see the source code for context.</param>
        /// <param name="r"> - see the source code for context.</param>
        /// <param name="s"> - see the source code for context.</param>
        /// <param name="t"> - see the source code for context.</param>
        /// <param name="u"> - see the source code for context.</param>
        /// <param name="v"> - see the source code for context.</param>
        /// <param name="w"> - see the source code for context.</param>
        /// <param name="x"> - see the source code for context.</param>
        /// <param name="y"> - see the source code for context.</param>
        /// <returns></returns>
        public static int TsEsRp1(TextWriter tw,
                        TpParm tpParm,
                        string cTableName,
                        string p,
                        string q,
                        string r,
                        string s,
                        string t,
                        string u,
                        string v,
                        string w,
                        string x,
                        string y)
        {
            //...Log2.v("\nTsesrp1.TsEsRp1(): Entry: cTableName = " + cTableName);

            PrintLine pl = new PrintLine();
            pl.OutFile(tw);

            int nPage = 1;
            string cDate;
            string cTime;

            pl.Output();
            pl.LeftAt(5, "FREQUENCY COORDINATION SYSTEM ASSOCIATION");
            pl.IntAt(70, "PAGE: {0,3:D}", nPage); //<--
            pl.Output();

            GenUtil.UtGetDateTime(out cDate, out cTime);
            pl.LeftAt(5, "MASTER DATA BASE -- TS-ES Interference Study Summary ");
            pl.LeftAt(74, cTime);
            pl.Output();

            pl.LeftAt(10, "Project Number            : ");
            pl.LeftAt(-1, p);
            pl.Output();
            pl.LeftAt(10, "PDF File Name             : ");
            pl.LeftAt(-1, tpParm.proname);
            pl.Output();
            pl.LeftAt(10, "Run Name                  : ");
            pl.LeftAt(-1, tpParm.runname);
            pl.Output();
            pl.LeftAt(10, "PDF Type                  : ");
            pl.LeftAt(-1, tpParm.protype);
            pl.Output();
            pl.LeftAt(10, "Environment Type          : ");
            pl.LeftAt(-1, tpParm.envtype);
            pl.Output();
            pl.LeftAt(10, "Environment Name          : ");
            pl.LeftAt(-1, tpParm.envname);
            pl.Output();
            pl.LeftAt(10, "Country (CAN/USA/ALL)     : ");
            pl.LeftAt(-1, tpParm.country);
            pl.Output();
            pl.LeftAt(10, "Selected Environment Sites: ");
            pl.LeftAt(-1, tpParm.selsites);
            pl.Output();
            if (tpParm.selsites.Equals("CALL SIGN"))
            {
                pl.LeftAt(10, "Operator Codes            : ALL");
                pl.Output();
                pl.LeftAt(10, "Call Signs                : ");
                pl.LeftAt(-1, tpParm.codes);
                pl.Output();
            }
            else if (tpParm.selsites.Equals("OPERATOR CODE"))
            {
                pl.LeftAt(10, "Call Sign Codes           : ALL");
                pl.Output();
                pl.LeftAt(10, "Operator Codes            : ");
                pl.LeftAt(-1, tpParm.codes);
                pl.Output();
            }
            else
            {
                pl.LeftAt(10, "Operator Codes            : ALL");
                pl.Output();
                pl.LeftAt(10, "Call Sign Codes           : ALL");
                pl.Output();
            }

            pl.Output();
            pl.LeftAt(10, "Study Date                : ");
            pl.LeftAt(-1, tpParm.mdate);
            pl.Output();
            pl.LeftAt(10, "Study Time                : ");
            pl.LeftAt(-1, tpParm.mtime);
            pl.Output();
            pl.LeftAt(10, "CPU Time                  : ");
            pl.LeftAt(-1, q);
            pl.Output();
            pl.LeftAt(10, "Elapsed Time              : ");
            pl.LeftAt(-1, r);
            pl.Output();
            pl.Output();
            pl.LeftAt(10, "Geostationary Orbit Study    : ");
            pl.LeftAt(-1, tpParm.tsorbout);
            pl.Output();
            pl.LeftAt(10, "Propagation Loss Model       : ");
            switch (tpParm.spherecalc[0])
            {
                case '1':
                    pl.LeftAt(-1, "TSIP CCIR-SJM");
                    break;

                case '2':
                    pl.LeftAt(-1, "Spherical Earth");
                    break;

                default:
                    break;
            }
            pl.Output();

            pl.LeftAt(10, "Maximum Frequency Separation : ");
            pl.DoubleAt(-1, "{0,5:F0}", tpParm.fsep);
            pl.Output();
            pl.LeftAt(10, "Coordination Distance (km)   : ");
            pl.DoubleAt(-1, "{0,5:F0}", tpParm.coordist);
            pl.Output();
            pl.LeftAt(10, "Analysis type                :  ");
            pl.LeftAt(-1, tpParm.analopt);
            pl.Output();
            pl.LeftAt(10, "Margin (dB)                  : ");
            pl.DoubleAt(-1, "{0,5:F1}", tpParm.margin);
            pl.Output();
            pl.LeftAt(10, "Channel Status Codes (0 - 9) : ");
            pl.LeftAt(-1, tpParm.chancodes);
            pl.Output();
            pl.Output();

            pl.LeftAt(10, "Reports run:");
            pl.Output();
            pl.LeftAt(10, "Execution Report             : ");
            pl.LeftAt(-1, t);
            pl.Output();
            pl.LeftAt(10, "Study Summary Report         : ");
            pl.LeftAt(-1, u);
            pl.Output();
            pl.LeftAt(10, "Stations Analyzed Report     : ");
            pl.LeftAt(-1, v);
            pl.Output();
            pl.LeftAt(10, "Case Detail Report           : ");
            pl.LeftAt(-1, w);
            pl.Output();
            pl.LeftAt(10, "Case Summary Report          : ");
            pl.LeftAt(-1, x);
            pl.Output();

            if (tpParm.tsorbout.Equals("Y"))
            {
                pl.LeftAt(10, "Orbit Report             : Y");
                pl.Output();
            }
            pl.Output();

            pl.LeftAt(10, "Number of TS-ES Stations Passed : ");
            pl.RightAt(-1, s);
            pl.Output();
            pl.LeftAt(10, "Number of ES->TSStations Passed : ");
            pl.RightAt(-1, y);
            pl.Output();
            pl.LeftAt(10, "Number of TS Cases        : ");
            pl.IntAt(37, "{0,5}", tpParm.numcases);
            pl.Output();
            pl.LeftAt(10, "Number of ES Cases        : ");
            pl.IntAt(37, "{0,5}", tpParm.numtecases);
            pl.Output();

            pl.LeftAt(10, "     Interference Tables:-");
            pl.Output();
            pl.LeftAt(10, "          tt_{0}_parm Table", cTableName);
            pl.Output();
            pl.LeftAt(10, "          tt_{0}_erro Table", cTableName);
            pl.Output();
            pl.LeftAt(10, "          tt_{0}_site Table", cTableName);
            pl.Output();
            pl.LeftAt(10, "          tt_{0}_ante Table", cTableName);
            pl.Output();
            pl.LeftAt(10, "          tt_{0}_chan Table", cTableName);
            pl.Output();

            //...Log2.v("\nTsesrp1.TsEsRp1(): Exit");
            return 0;
        }






    }
}

```
