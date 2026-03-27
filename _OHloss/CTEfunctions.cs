using _Configuration;
using _DataStructures;
using _NewLib;
using _OHloss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// _OHloss.dll is a C# library that encapsulates the 'over the horizon' telemetry calculation
/// subroutine library provided by Contract Telecommunication Engineering Ltd. (CTE) 
/// </summary>
/// <remarks>
/// CTE provides FCSA with their 3rd-party
/// library of functions that perform a repertoir of
/// microwave transmission calculations. This CTE library
/// was provided to FCSA as a static library compiled from
/// 'C' code using Visual Studio (VS) 2010.
/// 
/// Significant technical finesse was needed to successfully use CTE's
/// static library. MICS code developers/maintainers should read and 
/// understand the detailed remarks provided below before tinkering 
/// with the C# code.
/// 
/// The first issue is that 'C' source code is not available for
/// CTE's 'C' code static library and that it was built using
/// VS2010.
/// 
/// VS2015 uses a newer version of the linker
/// than VS2010 and C/C++ applications built with VS2015
/// cannot link successfully to static libraries built with 
/// VS2010; the VS link stage returns no errors but the application
/// will crash at run-time.
///
/// In any event, C# code cannot be linked to native code 
/// static (C/C++) libraries. 
///
/// The solution to this dilemma is to use VS2010 to build 
/// a dydnamic link library (dll) from the CTE-supplied static
/// library. Managed (C#) applications can use dlls created
/// from unmanaged (C/C++) code; furthermore it does not matter
/// what version of Visual Studio was used to build the dll.
///
/// Microsoft's P/Invoke functionality is used to [In, Out] pass 
/// C# method arguments to CTE's 'C/C++' native code functions.
/// 
/// The CTE native code function calc_ohloss() is called with
/// a single argument, the 'C' structure called ohloss_xfer
/// defined in the CTE-supplied ohloss.h include file. By
/// default, 'C' structures are passed by value, i.e. the whole
/// structure is pushed onto and popped off the stack.
/// Fortunately, CTE designed their function calc_ohloss() to
/// pass only the address of the struct ohloss_xfer, i.e. call
/// by reference.
///
/// The CTE-defined 'C' structure ohloss_xfer has five fields that
/// are arrays whose length is explicitely declared. An obscure
/// feature of 'C' is that a structure field declared as a 
/// constant length array has its storage 'inlined' inside the
/// structure rather than the field being stored as a pointer
/// to an array somewhere else in heap memory.
///
/// The implicit 'C' ohloss_xfer structure storage inlining 
/// of fixed-length array fields described above means that
/// some finesse is required to develop a C# structure that
/// can be P/Invoke marshalled [In, Out] with the native
/// function calc_ohloss().
///
/// C# does provide the 'fixed' keyword that performs the same
/// (implicit) inlining of structure fields that are explicitely 
/// declared as arrays of constant length. The use of 'fixed' in
/// this way is valid only in C# 'struct' definitions *not* 'class'
/// definitions. (Hence the type OhLoss_Xfer used for the P/Invoke
/// is a 'struct' not a class.)
///
/// Just to add further complication, when the C# qualifier 'fixed'
/// is applied to fields of the structure thereafter those fields 
/// become 'unsafe' and have to be accessed as such. Also, the
/// project _OHloss has to be compiled with the 'allow unsafe code'
/// swtich turned on.
/// 
/// A C# application calls the method Calc_OhLoss() using an
/// instance of the class OhLossXfer that is a 'clean and simple'
/// equivalent of the 'C' struct ohloss_xfer with all the 'inlined'
/// arrays replaced by conventional C# string and double[] fields.
/// </remarks>
namespace _OHloss
{
    /// <summary>
    /// Provides methods that call subroutines in CTE's
    /// OhLoss native code library and pass the results data back to
    /// the application; see <b>more</b> for a listing of 
    /// all qty 38 'C' function declarations.
    /// </summary>
    /// <remarks>
    /// All qty. 38 'C' functions are provided in CTE's OH loss library (and hence
    /// CTE.dll):
    /// <code>
    /// 
    /// void      add_erf(double, struct ohloss_record *);
    /// double    ap_uvp(double, double, double, double, double, double);
    /// double    av0(double);
    /// void      brem_sincos(double *, double *, double *, double);
    /// void      calc_ohloss(struct ohloss_xfer *);
    /// void      calc_scatter(struct ohloss_xfer *, struct ohloss_record *);
    /// void      classify_path(struct ohloss_record *, struct ohloss_xfer *);
    /// void      combined_loss(struct ohloss_xfer *);
    /// void      create_250kpath_profile(struct profile_xfer *);
    /// int       create_hztable(struct ohloss_record *, struct ohloss_xfer *);
    /// void      create_path_profile(struct profile_xfer *);
    /// void      eff_anthts(struct ohloss_record *, struct ohloss_xfer *);
    /// void      extrap_data(double, double, int, double *, double *, struct ohloss_record *);
    /// void      forward(double, double, double *, double, double, double *, double *);
    /// double    free_space_loss(double, double);
    /// int       fresnel_clear(int, int, double *, struct ohloss_record *, struct ohloss_xfer *);
    /// double    gen101_funct(int, int, double);
    /// void      geteffective_dist(struct ohloss_record *, struct ohloss_xfer *);
    /// int       get_header_len(void);
    /// double    htgain(double, int, struct ohloss_record *, struct ohloss_xfer *);
    /// void      init_directories(char *, char *);
    /// void      inverse(double *, double *, double *, double, double, double, double);
    /// void      LatLng_to_UTM(struct geo_utm_xfer *);
    /// void      nsma_diffract(struct ohloss_record *, struct ohloss_xfer *);
    /// void      nsma_multike(struct ohloss_record *, struct ohloss_xfer *);
    /// void      nsma_single(struct ohloss_record *, struct ohloss_xfer *);
    /// double    radius_3xy(double, double, double, double, double, double);
    /// void      red(struct ohloss_record *, struct ohloss_xfer *);
    /// void      set_earthparam(double *, double *, struct ohloss_record *, struct ohloss_xfer *);
    /// int       set_elevation(double, double, double *, char *);
    /// void      set_intermed(double *, double *, double *, double);
    /// void      set_T1_T4(double *, double);
    /// void      set_T1_T8(double *, double);
    /// void      set_T3_T8(double *, double);
    /// double    sign(double, double);
    /// void      time_variab(struct ohloss_xfer *);
    /// void      UTM_to_LatLng(struct geo_utm_xfer *);
    /// void      vertical_angles(struct ohloss_xfer *, struct ohloss_record *);
    /// </code>
    /// </remarks>
    public class CTEfunctions
    {

        [DllImport("CTE.dll", CharSet = CharSet.Ansi)]
        private extern static void init_directories([In] string dir250k, [In] string dir50k);

        [DllImport("CTE.dll", CharSet = CharSet.Ansi)]
        private extern static void calc_ohloss([In, Out] ref OhLoss_Xfer s);

        [DllImport("CTE.dll", CharSet = CharSet.Ansi)]
        private extern static void create_path_profile([In, Out] ref Profile_Xfer s);

        [DllImport("CTE.dll", CharSet = CharSet.Ansi)]
        private extern static void LatLng_to_UTM([In, Out] ref Geo_UTM_Xfer s);

        [DllImport("CTE.dll", CharSet = CharSet.Ansi)]
        private extern static void UTM_to_LatLng([In, Out] ref Geo_UTM_Xfer s);

        //---------------------------------------------------------------------------
        /// <summary>
        /// This method recieves the paths of the 50k and 250k OhLoss data libraries
        /// and initializes the CTE library's internal state to be ready for subsequent
        /// subroutine calls.
        /// </summary>
        /// <param name="dir250k"></param>
        /// <param name="dir50k"></param>
        public static void Init_Directories(string dir250k, string dir50k)
        {
            bool dir50kExists = Directory.Exists(dir50k);
            if (!dir50kExists)
            {
                Log2.e("\nOhLossXfer.Init_Directories(): ERROR: 50K directory not found: " + dir50k);
            }

            bool dir250kExists = Directory.Exists(dir250k);
            if (!dir250kExists)
            {
                Log2.e("\nOhLossXfer.Init_Directories(): ERROR: 250K directory not found: " + dir250k);
            }

            //...Log2.v("\nOver Horizon Loss Calculation Directories:");
            //...Log2.v(String.Format("\n    1:250K - {0}\n    1:50K  - {1}\n", dir250k, dir50k));

            init_directories(dir250k, dir50k);

            //...Log2.v("\nOhLossXfer.Init_Directories(): called native CTE.lib function:  init_directories(dir250k, dir50k)");
        }

        /// <summary>
        /// This method returns a dataset that models 'over-the-horizon' propagation loss.
        /// </summary>
        /// <param name="ohLossXfer"> - struct used to pass values [In, Out].</param>
        public static void Calc_OhLoss(ref OhLossXfer ohLossXfer)
        {
            // Create an instance of the struct OhLoss_Xfer that we will use
            // to P/Invoke marshal [In, Out] call the native code function
            // calc_ohloss().
            OhLoss_Xfer s = new OhLoss_Xfer();

            // We only need to prescribe the following 'input' fields:
            s.lat_1 = ohLossXfer.lat_1;
            s.lng_1 = ohLossXfer.lng_1;
            s.lat_2 = ohLossXfer.lat_2;
            s.lng_2 = ohLossXfer.lng_2;
            s.s1_anthght = ohLossXfer.s1_anthght;
            s.s2_anthght = ohLossXfer.s2_anthght;
            s.freq = ohLossXfer.freq;
            s.polarization = ohLossXfer.polarization;
            s.clim_region = ohLossXfer.clim_region;
            s.K_median = ohLossXfer.K_median;

            //...Log2.v("\n\n===== Before calc_ohloss() =====");
            //...Log2.v(s.ToString());

            // Call the native function using the struct that works with P/Invoke.
            calc_ohloss(ref s);

            //...Log2.v("\n\n===== After calc_ohloss() =====");
            ////...Log2.v(s.ToString());

            // We only need to fill the 'output' fields of the object ohLossXfer.
            // First, the 'easy' fields.
            ohLossXfer.dist_horiz1 = s.dist_horiz1;
            ohLossXfer.dist_horiz2 = s.dist_horiz2;
            ohLossXfer.elev_horiz1 = s.elev_horiz1;
            ohLossXfer.elev_horiz2 = s.elev_horiz2;
            ohLossXfer.angl_horiz1 = s.angl_horiz1;
            ohLossXfer.angl_horiz2 = s.angl_horiz2;
            ohLossXfer.s1_effhght = s.s1_effhght;
            ohLossXfer.s2_effhght = s.s2_effhght;
            ohLossXfer.eff_dist = s.eff_dist;
            ohLossXfer.horiz_xover = s.horiz_xover;
            ohLossXfer.refdiff_loss = s.refdiff_loss;
            ohLossXfer.refscatt_loss = s.refscatt_loss;
            ohLossXfer.refcomb_loss = s.refcomb_loss;
            ohLossXfer.median_loss = s.median_loss;
            ohLossXfer.calc_type = s.calc_type;
            ohLossXfer.error_status = (Enums.PRF)s.error_status;
            ohLossXfer.cded50_files_used = s.cded50_files_used == 0 ? false : true;

            // Next, fill the qty. 3 string fields that were marshalled as
            // 'fixed' byte arrays.
            unsafe
            {
                StringBuilder sb = new StringBuilder();
                byte b;
                for (int i = 0; i < Constant.TSIP_MAX_PATH; i++)
                {
                    b = s.path_record[i];
                    if (b == 0) break;
                    sb.Append(Convert.ToChar(b));
                }
                ohLossXfer.path_record = sb.ToString();

                sb.Clear();
                for (int i = 0; i < Constant.MAP_NAME_SIZE; i++)
                {
                    b = s.map_name[i];
                    if (b == 0) break;
                    sb.Append(Convert.ToChar(b));
                }
                ohLossXfer.map_name = sb.ToString();

                sb.Clear();
                for (int i = 0; i < Constant.MAX_PATH; i++)
                {
                    b = s.cded50_map_name[i];
                    if (b == 0) break;
                    sb.Append(Convert.ToChar(b));
                }
                ohLossXfer.cded50_map_name = sb.ToString();
            }

            // Finally, fill the double[] fields that were marshalled
            // as 'fixed' arrays.
            unsafe
            {
                ohLossXfer.ohloss_50 = new double[Constant.OHLOSS_ARRAY_SZ];
                for (int i = 0; i < Constant.OHLOSS_ARRAY_SZ; i++)
                {
                    ohLossXfer.ohloss_50[i] = s.ohloss_50[i];
                }

                ohLossXfer.ohloss_95 = new double[Constant.OHLOSS_ARRAY_SZ];
                for (int i = 0; i < Constant.OHLOSS_ARRAY_SZ; i++)
                {
                    ohLossXfer.ohloss_95[i] = s.ohloss_95[i];
                }
            }

            //...Log2.v(ohLossXfer.ToString());
        }

        /// <summary>
        /// This method converts NAD 83 coordinates to UTM coordinates.
        /// </summary>
        /// <param name="geo_UTM_Xfer"></param>
        public static void LatLngToUTM(ref Geo_UTM_Xfer geo_UTM_Xfer)
        {
            LatLng_to_UTM(ref geo_UTM_Xfer);
        }

        /// <summary>
        /// This method converts UTM coordinates to NAD 83 coordinates.
        /// </summary>
        /// <param name="geo_UTM_Xfer"></param>
        public static void UTMtoLatLng(ref Geo_UTM_Xfer geo_UTM_Xfer)
        {
            UTM_to_LatLng(ref geo_UTM_Xfer);
        }

        /// <summary>
        /// This method inputs data for two TS stations (A and B) and outputs a report
        /// providing ground elevation above the GRS 80 datum ellipsoid at uniformly
        /// spaced points along the geodesic between A and B; GetProfRep is called by
        /// the WebMICS Auxiliary Engineering tool for 'Terrain Profile' calculations.
        /// </summary>
        /// <param name="sctProf"></param>
        public static void Create_Path_Profile(ref ProfileXfer sctProf)
        {
            //...Log2.v("\n\nCTEfunctions.Create_Path_Profile(): Entry");
            // Create an instance of the struct Prof_Xfer that we will use
            // to P/Invoke marshal [In, Out] call the CTE native code function
            // create_path_profile().
            Profile_Xfer s = new Profile_Xfer();

            // We only need to prescribe the following 'input' fields:
            s.lat_1 = sctProf.lat_1;
            s.lng_1 = sctProf.lng_1;
            s.lat_2 = sctProf.lat_2;
            s.lng_2 = sctProf.lng_2;
            s.dist_inc = sctProf.dist_inc;

            //...Log2.v("\n\n===== Before create_path_profile() =====");
            //...Log2.v(s.ToString());

            // Call the native function using the struct that works with P/Invoke.
            create_path_profile(ref s);

            //...Log2.v("\n\n===== After create_path_profile() =====");
            ////...Log2.v(s.ToString());

            // We only need to fill the 'output' fields of the object sctProf.
            // First, the 'easy' fields.
            sctProf.num_points = s.num_points;
            sctProf.error_code = (Enums.OHLerrStat)s.error_code;
            sctProf.last_dist = s.last_dist;
            sctProf.cded50_files_used = s.cded50_files_used == 0 ? false : true;

            // Next, fill the qty. 2 string fields that were marshalled as
            // 'fixed' byte arrays.
            unsafe
            {
                StringBuilder sb = new StringBuilder();
                byte b;
                for (int i = 0; i < Constant.MAX_PATH; i++)
                {
                    b = s.map_name[i];
                    if (b == 0) break;
                    sb.Append(Convert.ToChar(b));
                }
                sctProf.map_name = sb.ToString();

                sb.Clear();
                for (int i = 0; i < Constant.MAX_PATH; i++)
                {
                    b = s.cded50_map_name[i];
                    if (b == 0) break;
                    sb.Append(Convert.ToChar(b));
                }
                sctProf.map_name = sb.ToString();
            }

            // Finally, fill the double[] fields that were marshalled
            // as IntPtr.
            unsafe
            {
                sctProf.dist_array = new double[s.num_points];
                Marshal.Copy(s.dist_array, sctProf.dist_array, 0, s.num_points);

                sctProf.elev_array = new double[s.num_points];
                Marshal.Copy(s.elev_array, sctProf.elev_array, 0, s.num_points);
            }

            //...Log2.v(sctProf.ToString());
            //...Log2.v("\nCTEfunctions.Create_Path_Profile(): Exit");
        }

    }
}
