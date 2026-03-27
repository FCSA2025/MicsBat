using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class has fields that are isomorphic to the legacy C/C++ struct profile_xfer; is used when calling
    /// methods in the CTE library.
    /// </summary>
    public class ProfileXfer
    {
        // input variables 

        public double lat_1;          // 1st site latitude - positive north - NAD 83 
        public double lng_1;          // 1st site longitude - positive west - NAD 83
        public double lat_2;          // 2nd site latitude - positive north - NAD 83 
        public double lng_2;          // 2nd site longitude - positive west - NAD 83

        public double dist_inc;       // distance between points - meters 

        // output variables 

        public int num_points;     // number of points in the distance-elevation arrays
        public Enums.OHLerrStat error_code;

        public double[] dist_array;
        public double[] elev_array;

        public double last_dist;   // the path length-distance of the last point read in the case of an error
        public string map_name;    // the DTED file name on which the error occurred

        public string cded50_map_name;

        public bool cded50_files_used;

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 13;

        //---------------------------------------------------------------------------

        /// <summary>
        /// Constructor; creates an object whose fields are to type-specific default values.
        /// </summary>
        public ProfileXfer()
        {
            Initialize();
        }

        /// <summary>
        /// Sets the fields of an object to type-specific default values
        /// </summary>
        public void Initialize()
        {
            const double INIT_DOUBLE = 0.0;
            const int INIT_INT = 0;
            const string INIT_STRING = null;
            const bool INIT_BOOL = false;

            lat_1 = INIT_DOUBLE;
            lng_1 = INIT_DOUBLE;
            lat_2 = INIT_DOUBLE;
            lng_2 = INIT_DOUBLE;
            dist_inc = INIT_DOUBLE;
            num_points = INIT_INT;
            error_code = INIT_INT;
            dist_array = new double[num_points];
            elev_array = new double[num_points];
            last_dist = INIT_DOUBLE;
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
            sb.Append("\ndist_inc = " + dist_inc);
            sb.Append("\nnum_points = " + num_points);
            sb.Append("\nerror_code = " + error_code);

            if (num_points > 0)
            {
                for (int i = 0; i < num_points; i++)
                {
                    sb.Append("\ndist_array[" + i + "] = " + dist_array[i]);
                }
            }
            else
            {
                sb.Append("\ndist_array = null or has zero elements.");
            }

            if (num_points > 0)
            {
                for (int i = 0; i < num_points; i++)
                {
                    sb.Append("\nelev_array[" + i + "] = " + elev_array[i]);
                }
            }
            else
            {
                sb.Append("\nelev_array = null or has zero elements.");
            }

            sb.Append("\nlast_dist = " + last_dist);
            sb.Append("\nmap_name = " + map_name);
            sb.Append("\ncded50_map_name = " + cded50_map_name);
            sb.Append("\ncded50_files_used = " + cded50_files_used);

            return sb.ToString();
        }



        /*
                lat_1
                lng_1
                lat_2
                lng_2
                dist_inc
                num_points
                error_code
                dist_array
                elev_array
                last_dist
                map_name
                cded50_map_name
                cded50_files_used 
        */



    }
}
