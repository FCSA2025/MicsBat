# Documented File: Reports.cs
**Repository Path:** `GetProfRep\Reports.cs`
**Primary Layer:** `GetProfRep`
**Namespace:** `GetProfRep`

## Source Code Representation
```csharp
﻿using _Auxlib;
using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetProfRep
{
    /// <summary>
    /// This class provides methods that write GetProfrep program output 'reports' to
    /// Console.Out; the supported report format types are PLAIN (simple text), HTML and CSV.
    /// </summary>
    public class Reports
    {
        /*	Arrays for the ohloss values */
        private static string[] aPol = new string[2] { "Horizontal", "Vertical" };

        private static string[] aClimate = new string[3] { "Continental Temperate", "Maritime Temperate Over Land", "Maritime Temperate Over Sea" };

        private static double[] dTimePct = new double[8] { 50.0, 80.0, 90.0, 99.0, 99.9, 99.99, 99.995, 99.9975 };


        /// <summary>
        /// This method manages the production of the PLAIN (simple text) and HTML
        /// output report format types.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="sctProf"></param>
        public static void WritePlainOrHTML(Parameters p, ProfileXfer sctProf)
        {
            bool isHTML = (p.eRep == Enums.eRepType.eHTML);

            if (isHTML)
            {
                Console.Write("<html><head><title>getprofrep output report</title>\r\n</head><body><h1>");
            }

            Console.Write("Frequency Coordination System Association");

            if (isHTML)
            {
                Console.Write("<br>");
            }

            Console.Write("\r\nTerrain Profile Report -- Build {0}", Info.BuildMetaData);

            if (isHTML)
            {
                Console.Write("</h1><h3>");
            }

            Console.Write("\r\nMicrowave Interference Calculation System\r\n");

            if (isHTML)
            {
                Console.Write("</h3>");
                Console.Write("\r\n<table border=0 width=\"80%\"><tr align=left><th>From:</th><th></th><th>To:</th><th></th></tr>\r\n");
            }
            else
            {
                Console.Write("\r\nFrom:                                  To:\r\n");
            }

            if (p.mSiteCallSignA.Length != 0 || p.mSiteCallSignB.Length != 0)
            {
                /*	Display the station */
                if (isHTML)
                {
                    Console.Write("\r\n<tr>");
                }

                if (p.mSiteCallSignA.Length != 0)
                {
                    if (isHTML)
                    {
                        Console.Write("\r\n<td>{0}</td><td>{1}</td>", p.mSiteCallSignA, p.mSiteNameA);
                    }
                    else
                    {
                        Console.Write("{0,-12}({1,-20})     ", p.mSiteCallSignA, p.mSiteNameA);
                    }
                }
                else
                {
                    if (isHTML)
                    {
                        Console.Write("\r\n<td></td><td></td>");
                    }
                    else
                    {
                        Console.Write("{0,40}", p.mSiteCallSignA);
                    }
                }

                if (p.mSiteCallSignB.Length != 0)
                {
                    if (isHTML)
                    {
                        Console.Write("\r\n<td>{0}</td><td>{1}</td>", p.mSiteCallSignB, p.mSiteNameB);
                    }
                    else
                    {
                        Console.Write("{0,-12}({1,-20})", p.mSiteCallSignB, p.mSiteNameB);
                    }
                }

                if (isHTML)
                {
                    Console.Write("\r\n</tr>");
                }
            }

            if (isHTML)
            {
                Console.Write("\r\n<tr><td>WGS 84 {0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                               p.mLat83A, p.mLong83A, p.mLat83B, p.mLong83B);
                Console.Write("</table><p><bold>");
            }
            else
            {
                Console.Write("\r\n\r\nNAD 83 {0,-15} {1,-15} {2,-15} {3,-15}",
                                p.mLat83A, p.mLong83A, p.mLat83B, p.mLong83B);
            }

            if (p.fdist > 0.00)
            {
                Console.Write("\r\n\r\nDistance:{0:F2}km  Bearing A->B {1:F1} Bearing B->A {2:F1} Increment: {3:F2}",
                                 p.fdist, p.fbearinga, p.fbearingb, p.fdistinc);
            }

            if (isHTML)
            {
                Console.Write("\r\n</bold></p><table border=0 width=\"80%\"><tr align=right><th>Dist from A<br>(km)</th><th>Altitude<br>(m)</th><th>Dist from B<br>(km)</th></tr>");
            }

            int nPage = 1;
            for (int nIndex = 0; nIndex < sctProf.num_points; nIndex++)
            {
                if (nIndex % 40 == 0 && !(isHTML))
                {
                    if (nIndex > 1)
                    {
                        Console.Write("\f\r\nPage {0}\r\n", ++nPage);
                    }

                    Console.Write("\r\nDist from A     Altitude     Dist from B");
                    Console.Write("\r\n       (km)          (m)            (km)");
                    Console.Write("\r\n-----------     --------     -----------\r\n");
                }

                if (isHTML)
                {
                    Console.Write("\r\n<tr align=right><td>{0,11:F2}</td><td>{1,8:F1}</td><td>{2,11:F2}</td></tr>",
                                     sctProf.dist_array[nIndex],
                                     sctProf.elev_array[nIndex],
                                     p.fdist - sctProf.dist_array[nIndex]);
                }
                else
                {
                    Console.Write("\r\n{0,11:F2}     {1,8:F1}     {2,11:F2}",
                                     sctProf.dist_array[nIndex],
                                     sctProf.elev_array[nIndex],
                                     p.fdist - sctProf.dist_array[nIndex]);
                }
            }

            if (isHTML)
            {
                Console.Write("\r\n</table>");
                Console.Write("\r\n<p><strong>{0} maps used.</strong></p>", p.mMapsUsed);
                Console.Write("\r\n<h3>*WARNING* Use for coordination only.  Not for path design.</h3>");
                Console.Write("\r\n</body></html>\r\n");
            }
            else
            {
                Console.Write("\r\n\r\n {0} maps used.\r\n", p.mMapsUsed);
                Console.Write("\r\n*WARNING* Use for coordination only. Not for path design.");
                Console.Write("\r\n");
            }
        }

        /// <summary>
        /// This method manages the production of Comma-Separated Values (CSV)
        /// output report format type.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sctProf"></param>
        public static void WriteCSV(Parameters param, ProfileXfer sctProf)
        {
            Console.Write("\"{0}\",\"{1}\"", param.mSiteCallSignA, param.mSiteNameA);
            Console.Write(",\"{0}\",\"{1}\"", param.mSiteCallSignB, param.mSiteNameB);

            Console.Write("\r\n\"WGS 84\",\"{0}\",\"{1}\",\"{2}\",\"{3}\"",
                                param.mLat83A, param.mLong83A,
                                param.mLat83B, param.mLong83B);

            Console.Write("\r\n{0:F2},{1:F1},{2:F1},{3:F2},\"{4}\"",
                            param.fdist, param.fbearinga, param.fbearingb,
                            param.fdistinc, param.mMapsUsed);

            for (int nIndex = 0; nIndex < sctProf.num_points; nIndex++)
            {
                Console.Write("\r\n{0,11:F3},{1,9:F3},{2,11:F3}",
                                 sctProf.dist_array[nIndex],
                                 sctProf.elev_array[nIndex],
                                 param.fdist - sctProf.dist_array[nIndex]);
            }
            Console.Write("\r\n");
        }

        /// <summary>
        /// This method manages the production of the PL3
        /// output report format type.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sctProf"></param>
        public static void WritePL3(Parameters param, ProfileXfer sctProf)
        {
            /*	Write the header  */
            Console.Write("PL-3.0");

            /*	Display the station */
            Console.Write("\r\nSite1:{0}", param.mSiteCallSignA);
            Console.Write("\r\nSite2:{0}", param.mSiteCallSignB);

            Console.Write("\r\n{0,-11:F6}\r\n{1,-11:F6}\r\n{2,-11:F6}\r\n{3,-11:F6}",
                         sctProf.lat_1, sctProf.lng_1, sctProf.lat_2, sctProf.lng_2);

            Console.Write("\r\n{{Site 1 Antenna Height}} {{Reference Ellipsoid}}\r\nX\r\n{{Site 2 Antenna Height}} {{Polarization 0-H,1-V}}\r\nX\r\n{{Frequency (MHz)}}\r\n{0} {1} {2}",
                                sctProf.num_points, 0, 1);

            for (int nIndex = 0; nIndex < sctProf.num_points; nIndex++)
            {
                Console.Write("\r\n{0,11:F3} {1,9:F3} {2,11:F3} {3} {4}",
                                sctProf.dist_array[nIndex],
                                sctProf.elev_array[nIndex],
                                param.fdist - sctProf.dist_array[nIndex], 0, 0);
            }

            Console.Write("\r\n{0}\r\n{1}\r\n{2}\r\nProduced by MICS getprofrep build {3}", "FCSA",
                            "Drawing:", "INI", Info.BuildMetaData);

            Console.Write("\r\n0\r\nX X\r\nX\r\nX\r\n1.333333\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX\r\nX");
            Console.Write("\r\n");

        }



    }
}


```
