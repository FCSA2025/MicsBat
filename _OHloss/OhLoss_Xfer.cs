using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _OHloss
{
    /// <summary>
    /// This class has fields that are isomorphic to the legacy C/C++ struct ohloss_xfer and
    /// is used when calling methods in the CTE library.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct OhLoss_Xfer
    {
        // IMPORTANT!
        // =========
        // The following qty. 32 member fields correspond to the fields
        // of the legacy C/C++ struct ohloss_xfer.
        // The order of appearance of these qty. 32 'column' members MUST be
        // as indicated below in the inline comment. The reason is that this
        // allows the use of computationally efficient 'blitting' when using
        // 'P/Invoke' constructs to pass structures into and out of calls to
        // native code.
        //
        // DO NOT ADD ANY NON-STATIC MEMBERS!
        // =================================
        // ... this will cause 'managed memory access violation' errors.

        // input parameters
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lat_1;          // 1st site latitude - positive north - NAD 83
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lng_1;          // 1st site longitude - positive west - NAD 83
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lat_2;          // 2nd site latitude - positive north - NAD 83
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double lng_2;          // 2nd site longitude - positive west - NAD 83
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double s1_anthght;     // 1st site antenna height - meters AGL
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double s2_anthght;     // 2nd site antenna height - meters AGL
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double freq;           // frequency - MHz say 2000.
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short polarization;    // 0 - horizontal, 1 - vertical
        [MarshalAsAttribute(UnmanagedType.I2)]
        public short clim_region;    // 0 - continental temperate
                                     // 1 - maritime temperate overland
                                     // 2 - maritime temperate oversea
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double K_median;       // median value of K - close to 4/3  say 1.3333

        // output parameters
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double dist_horiz1;    // 1st site horizon distance (km)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double dist_horiz2;    // 2nd site horizon distance (km)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double elev_horiz1;    // 1st site horizon elevation (m)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double elev_horiz2;    // 2nd site horizon elevation (m)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angl_horiz1;    // 1st site horizon angle (deg)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double angl_horiz2;    // 2nd site horizon angle (deg)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double s1_effhght;     // effective antenna heights
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double s2_effhght;
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double eff_dist;       // effective distance (km)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double horiz_xover;    // intersection of horizon rays (km)

        [MarshalAsAttribute(UnmanagedType.R8)]
        public double refdiff_loss;    // reference diffraction loss (dB)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double refscatt_loss;   // reference scatter loss (dB)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double refcomb_loss;    // reference combined loss (dB)
        [MarshalAsAttribute(UnmanagedType.R8)]
        public double median_loss;     // median loss L(0.5) (dB)

        public fixed byte path_record[Constant.TSIP_MAX_PATH];     // Description of the Path 

        public fixed double ohloss_50[Constant.OHLOSS_ARRAY_SZ];
        public fixed double ohloss_95[Constant.OHLOSS_ARRAY_SZ];

        [MarshalAsAttribute(UnmanagedType.I4)]
        public int calc_type;          // OHL_LOS, OHL_SKE, OHL_ISOL, OHL_DKE, OHL_IRT 
        [MarshalAsAttribute(UnmanagedType.I4)]
        public int error_status;

        public fixed byte map_name[Constant.MAP_NAME_SIZE];
        public fixed byte cded50_map_name[Constant.MAX_PATH];

        [MarshalAsAttribute(UnmanagedType.U1)]
        public byte cded50_files_used;

        //---------------------------------------------------------------------------

        public const int NUM_COLUMNS = 32;

        //---------------------------------------------------------------------------



        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields.
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

            fixed (byte* b = path_record)
            {
                //sb.Append("\npath_record = " + b[0]);
                sb.Append("\npath_record = ");
                for (int i = 0; i < Constant.TSIP_MAX_PATH; i++)
                {
                    if (b[i] == 0) break;
                    sb.Append(Convert.ToChar(b[i]));
                }
            }

            fixed (double* d = ohloss_50)
            {
                for (int i = 0; i < Constant.OHLOSS_ARRAY_SZ; i++)
                {
                    sb.Append("\nohloss_50[" + i + "] = " + d[i]);
                }
            }

            fixed (double* d = ohloss_95)
            {
                for (int i = 0; i < Constant.OHLOSS_ARRAY_SZ; i++)
                {
                    sb.Append("\nohloss_95[" + i + "] = " + d[i]);
                }
            }

            sb.Append("\ncalc_type = " + calc_type);
            sb.Append("\nerror_status = " + error_status);

            fixed (byte* b = map_name)
            {
                sb.Append("\nmap_name = ");
                for (int i = 0; i < Constant.MAP_NAME_SIZE; i++)
                {
                    if (b[i] == 0) break;
                    sb.Append(Convert.ToChar(b[i]));
                }
            }

            fixed (byte* b = cded50_map_name)
            {
                sb.Append("\ncded50_map_name = ");
                for (int i = 0; i < Constant.MAX_PATH; i++)
                {
                    if (b[i] == 0) break;
                    sb.Append(Convert.ToChar(b[i]));
                }
            }

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
