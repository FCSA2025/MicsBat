# Documented File: ISEDkmlFiles.cs
**Repository Path:** `T5aPolygons\ISEDkmlFiles.cs`
**Primary Layer:** `T5aPolygons`
**Namespace:** `T5aPolygons`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace T5aPolygons
{
    /// <summary>
    /// This class provides methods that read KML-formatted text files and
    /// output a list of Tier5Area objects.
    /// </summary>
    public class ISEDkmlFiles
    {

        /// <summary>
        /// This method returns the list of Tier5Area objects that can be  parsed from one or more
        /// prescribed KML-formatted text files.
        /// </summary>
        /// <param name="KMLfilePaths"></param>
        /// <param name="allTier5Areas"></param>
        /// <returns></returns>
        public static bool GetTier5Areas(string[] KMLfilePaths, out List<Tier5Area> allTier5Areas)
        {
            // 'out' requirement.
            allTier5Areas = new List<Tier5Area>();

            List<ISEDplacemark> geoAreasInKMLfile = new List<ISEDplacemark>();
            List<ISEDplacemark> allGeoAreas = new List<ISEDplacemark>();

            Tier5Area tier5Area = null;
            string errMsg;

            foreach (string path in KMLfilePaths)
            {
                bool success = ISEDplacemark.ImportFromKMLfile(path, out geoAreasInKMLfile, out errMsg);

                if (!success)
                {
                    Console.Error.Write("\nERROR: call to GeoAreasFromFile() failed: \n{0}", errMsg);
                    return false;
                }
                else
                {
                    // Accumulate geoRegions in the list.
                    allGeoAreas.AddRange(geoAreasInKMLfile);
                }

            } // foreach path

            // Sort allGeoAreas list w.r.t. name.
            allGeoAreas.Sort(ISEDplacemark.Comparer);

            // Transform GeoAreas into Tier5Areas.
            foreach (ISEDplacemark geoArea in allGeoAreas)
            {
                string iD = geoArea.Name.Trim();

                BaseRateCode baseRateCode = BaseRateCode.UNKNOWN;
                switch (geoArea.Description.ToUpper())
                {
                    case "REMOTE":
                        baseRateCode = BaseRateCode.REMOTE;
                        break;
                    case "RURAL":
                        baseRateCode = BaseRateCode.RURAL;
                        break;
                    case "METRO":
                    case "URBAN":
                        baseRateCode = BaseRateCode.URBAN;
                        break;
                }

                if (baseRateCode == BaseRateCode.UNKNOWN)
                {
                    Console.Error.Write("\nERROR: converting geoArea.Description to BaseRateCode: " + geoArea.Description);
                    return false;
                }

                List<GeoBoundary> geoBoundaries = geoArea.Boundaries;

                if (geoBoundaries == null)
                {
                    Console.Error.Write("\nERROR: geoArea.Boundaries is NULL for GeoArea: " + geoArea.Name);
                    return false;
                }
                foreach (GeoBoundary geoBoundary in geoBoundaries)
                {
                    if (geoBoundary == null || geoBoundary.Points.Count == 0)
                    {
                        Console.Error.Write("\nERROR: (geoBoundary == null || geoBoundary.Points.Count == 0) for GeoArea: " + geoArea.Name);
                        return false;
                    }
                }

                // Everything is good.
                tier5Area = new Tier5Area(iD, baseRateCode, geoBoundaries);
                allTier5Areas.Add(tier5Area);
            }

            return true;
        }









    }
}


```
