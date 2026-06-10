# Documented File: OhLossXfer.cs
**Repository Path:** `_DataStructures\OhLossXfer.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class has fields that are isomorphic to the legacy C/C++ struct ohloss_xfer; is used when calling
    /// methods in the CTE library.
    /// </summary>
    public class OhLossXfer
    {
        // input parameters
        public double lat_1;           // 1st site latitude - positive north - NAD 83
        public double lng_1;           // 1st site longitude - positive west - NAD 83
        public double lat_2;           // 2nd site latitude - positive north - NAD 83
        public double lng_2;           // 2nd site longitude - positive west - NAD 83
        public double s1_anthght;      // 1st site antenna height - meters AGL
        public double s2_anthght;      // 2nd site antenna height - meters AGL
        public double freq;            // frequency - MHz say 2000.
        public short polarization;     // 0 - horizontal, 1 - vertical
        public short clim_region;      // 0 - continental temperate
                                       // 1 - maritime temperate overland
                                       // 2 - maritime temperate oversea
        public double K_median;        // median value of K - close to 4/3  say 1.3333

        // output parameters
        public double dist_horiz1;     // 1st site horizon distance (km)
        public double dist_horiz2;     // 2nd site horizon distance (km)
        public double elev_horiz1;     // 1st site horizon elevation (m)
        public double elev_horiz2;     // 2nd site horizon elevation (m)
        public double angl_horiz1;     // 1st site horizon angle (deg)
        public double angl_horiz2;     // 2nd site horizon angle (deg)
        public double s1_effhght;      // 1st effective antenna height
        public double s2_effhght;      // 2nd effective antenna height
        public double eff_dist;        // effective distance (km)
        public double horiz_xover;     // intersection of horizon rays (km)
        public double refdiff_loss;    // reference diffraction loss (dB)
        public double refscatt_loss;   // reference scatter loss (dB)
        public double refcomb_loss;    // reference combined loss (dB)
        public double median_loss;     // median loss L(0.5) (dB)
        public string path_record;     // Description of the Path 
        public double[] ohloss_50;     // OHLOSS array for 50 & 95% confidence factors
        public double[] ohloss_95;     // corresponding to the following time percentages
                                       // 50.0, 80.0, 90.0, 99.0, 99.90,
                                       // 99.990, 99.9950, 99.9975
                                       // classification of the path
        public int calc_type;          // OHL_LOS, OHL_SKE, OHL_ISOL, OHL_DKE, OHL_IRT 
        public Enums.PRF error_status;
        public string map_name;
        public string cded50_map_name;
        public bool cded50_files_used;

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 32;

        //---------------------------------------------------------------------------

        /// <summary>
        /// Constructor; creates an object whose fields are to type-specific default values.
        /// </summary>
        public OhLossXfer()
        {
        }

        /// <summary>
        /// Sets the fields of an object to type-specific default values
        /// </summary>
        public void Initialize()
        {
            const double INIT_DOUBLE = 0.0;
            const short INIT_SHORT = 0;
            const int INIT_INT = 0;
            const string INIT_STRING = null;
            const bool INIT_BOOL = false;

            lat_1 = INIT_DOUBLE;
            lng_1 = INIT_DOUBLE;
            lat_2 = INIT_DOUBLE;
            lng_2 = INIT_DOUBLE;
            s1_anthght = INIT_DOUBLE;
            s2_anthght = INIT_DOUBLE;
            freq = INIT_DOUBLE;
            polarization = INIT_SHORT;
            clim_region = INIT_SHORT;
            K_median = INIT_DOUBLE;
            dist_horiz1 = INIT_DOUBLE;
            dist_horiz2 = INIT_DOUBLE;
            elev_horiz1 = INIT_DOUBLE;
            elev_horiz2 = INIT_DOUBLE;
            angl_horiz1 = INIT_DOUBLE;
            angl_horiz2 = INIT_DOUBLE;
            s1_effhght = INIT_DOUBLE;
            s2_effhght = INIT_DOUBLE;
            eff_dist = INIT_DOUBLE;
            horiz_xover = INIT_DOUBLE;
            refdiff_loss = INIT_DOUBLE;
            refscatt_loss = INIT_DOUBLE;
            refcomb_loss = INIT_DOUBLE;
            median_loss = INIT_DOUBLE;
            path_record = INIT_STRING;

            ohloss_50 = new double[Constant.OHLOSS_ARRAY_SZ];
            for (int i = 0; i < ohloss_50.Length; i++)
            {
                ohloss_50[i] = INIT_DOUBLE;
            }

            ohloss_95 = new double[Constant.OHLOSS_ARRAY_SZ];
            for (int i = 0; i < ohloss_95.Length; i++)
            {
                ohloss_95[i] = INIT_DOUBLE;
            }

            calc_type = INIT_INT;
            error_status = INIT_INT;
            map_name = INIT_STRING;
            cded50_map_name = INIT_STRING;
            cded50_files_used = INIT_BOOL;

        }

        /// <summary>
        /// Returns a string that comprises all the field names and their current
        /// values.
        /// </summary>
        /// <returns> - string comprising all field names and their values.</returns>
        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nlat_1 = " + lat_1);
            sb.Append("\nlng_1 = " + lng_1);
            sb.Append("\nlat_2 = " + lat_2);
            sb.Append("\nlng_2 = " + lng_2);
            sb.Append("\ns1_anthght = " + s1_anthght);
            sb.Append("\ns2_anthght = " + s2_anthght);
            sb.Append("\nfreq = " + freq);
            sb.Append("\npolarization = " + polarization);
            sb.Append("\nclim_region = " + clim_region);
            sb.Append("\nK_median = " + K_median);
            sb.Append("\ndist_horiz1 = " + dist_horiz1);
            sb.Append("\ndist_horiz2 = " + dist_horiz2);
            sb.Append("\nelev_horiz1 = " + elev_horiz1);
            sb.Append("\nelev_horiz2 = " + elev_horiz2);
            sb.Append("\nangl_horiz1 = " + angl_horiz1);
            sb.Append("\nangl_horiz2 = " + angl_horiz2);
            sb.Append("\ns1_effhght = " + s1_effhght);
            sb.Append("\ns2_effhght = " + s2_effhght);
            sb.Append("\neff_dist = " + eff_dist);
            sb.Append("\nhoriz_xover = " + horiz_xover);
            sb.Append("\nrefdiff_loss = " + refdiff_loss);
            sb.Append("\nrefscatt_loss = " + refscatt_loss);
            sb.Append("\nrefcomb_loss = " + refcomb_loss);
            sb.Append("\nmedian_loss = " + median_loss);
            sb.Append("\npath_record = " + path_record);

            for (int i = 0; i < ohloss_50.Length; i++)
            {
                sb.Append("\nohloss_50[" + i + "] = " + ohloss_50[i]);
            }

            for (int i = 0; i < ohloss_95.Length; i++)
            {
                sb.Append("\nohloss_95[" + i + "] = " + ohloss_95[i]);
            }

            sb.Append("\ncalc_type = " + calc_type);
            sb.Append("\nerror_status = " + error_status);
            sb.Append("\nmap_name = " + map_name);
            sb.Append("\ncded50_map_name = " + cded50_map_name);
            sb.Append("\ncded50_files_used = " + cded50_files_used);

            return sb.ToString();
        }

        //-------------------------------------------------------------------------



        /*
        lat_1
        lng_1
        lat_2
        lng_2
        s1_anthght
        s2_anthght
        freq
        polarization
        clim_region
        K_median
        dist_horiz1
        dist_horiz2
        elev_horiz1
        elev_horiz2
        angl_horiz1
        angl_horiz2
        s1_effhght
        s2_effhght
        eff_dist
        horiz_xover
        refdiff_loss
        refscatt_loss
        refcomb_loss
        median_loss
        path_record
        ohloss_50
        ohloss_95
        calc_type
        error_status
        map_name
        cded50_map_name
        */






    }
}

```
