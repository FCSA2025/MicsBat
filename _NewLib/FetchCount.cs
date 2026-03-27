using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{

    /// <summary>
    /// Provides methods that count the number of fetches
    /// from the main database tables <b>main.me_*</b>, <b>main.mt_*</b> 
    /// and <b>main.sd_*</b>. The primary use of these methods is to identify
    /// opportunities for optimization of DB access.
    /// </summary>
    public class FetchCount
    {
        private static int mMeAnte = 0;
        private static int mMeAzim = 0;
        private static int mMeChan = 0;
        private static int mMeSite = 0;
        private static int mMtAnte = 0;
        private static int mMtChan = 0;
        private static int mMtSite = 0;
        private static int mSdAntd = 0;
        private static int mSdAnte = 0;
        private static int mSdBand = 0;
        private static int mSdCtx = 0;
        private static int mSdCtxd = 0;
        private static int mSdEqpt = 0;
        private static int mSdNote = 0;
        private static int mSdOper = 0;
        private static int mSdPlan = 0;
        private static int mSdPlnd = 0;
        private static int mSdRout = 0;
        private static int mSdTown = 0;
        private static int mSdTowr = 0;
        private static int mSdTraf = 0;

        //-------------------------------------------------------------------------------------------

        //public static int MeAnte { get { return mMeAnte; } set { mMeAnte = value; } }
        //public static int MeAzim { get { return mMeAzim; } set { mMeAzim = value; } }
        //public static int MeChan { get { return mMeChan; } set { mMeChan = value; } }
        //public static int MeSite { get { return mMeSite; } set { mMeSite = value; } }
        //public static int MtAnte { get { return mMtAnte; } set { mMtAnte = value; } }
        //public static int MtChan { get { return mMtChan; } set { mMtChan = value; } }
        //public static int MtSite { get { return mMtSite; } set { mMtSite = value; } }
        //public static int SdAntd { get { return mSdAntd; } set { mSdAntd = value; } }
        //public static int SdAnte { get { return mSdAnte; } set { mSdAnte = value; } }
        //public static int SdBand { get { return mSdBand; } set { mSdBand = value; } }
        //public static int SdCtx { get { return mSdCtx; } set { mSdCtx = value; } }
        //public static int SdCtxd { get { return mSdCtxd; } set { mSdCtxd = value; } }
        //public static int SdEqpt { get { return mSdEqpt; } set { mSdEqpt = value; } }
        //public static int SdNote { get { return mSdNote; } set { mSdNote = value; } }
        //public static int SdOper { get { return mSdOper; } set { mSdOper = value; } }
        //public static int SdPlan { get { return mSdPlan; } set { mSdPlan = value; } }
        //public static int SdPlnd { get { return mSdPlnd; } set { mSdPlnd = value; } }
        //public static int SdRout { get { return mSdRout; } set { mSdRout = value; } }
        //public static int SdTown { get { return mSdTown; } set { mSdTown = value; } }
        //public static int SdTowr { get { return mSdTowr; } set { mSdTowr = value; } }
        //public static int SdTraf { get { return mSdTraf; } set { mSdTraf = value; } }

        //----------------------------------------------------------------------------------------

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MeAnte object with data from the main table main.me_ante.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementMeAnte() { mMeAnte++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MeAzim object with data from the main table main.me_azim.
        /// </summary>
        /// <param name=""></param>
        /// 
        public static void IncrementMeAzim() { mMeAzim++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MeChan object with data from the main table main.me_chan.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementMeChan() { mMeChan++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MeSite object with data from the main table main.me_site.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementMeSite() { mMeSite++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MtAnte object with data from the main table main.mt_ante.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementMtAnte() { mMtAnte++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MtChan object with data from the main table main.mt_chan.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementMtChan() { mMtChan++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an MtSite object with data from the main table main.mt_site.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementMtSite() { mMtSite++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuAntd object with data from the supplemental main table main.sd_antd.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdAntd() { mSdAntd++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuAnte object with data from the supplemental main table main.sd_ante.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdAnte() { mSdAnte++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuBand object with data from the supplemental main table main.sd_band.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdBand() { mSdBand++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuCtx object with data from the supplemental main table main.sd_ctx.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdCtx() { mSdCtx++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuCtxd object with data from the supplemental main table main.sd_ctxd.
        /// </summary>
        /// <param name=""></param>
        /// <summary>
        public static void IncrementSdCtxd() { mSdCtxd++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuEqpt object with data from the supplemental main table main.sd_eqpt.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdEqpt() { mSdEqpt++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuNote object with data from the supplemental main table main.sd_note.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdNote() { mSdNote++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuOper object with data from the supplemental main table main.sd_oper.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdOper() { mSdOper++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuPlan object with data from the supplemental main table main.sd_plan.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdPlan() { mSdPlan++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuPlnd object with data from the supplemental main table main.sd_plnd.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdPlnd() { mSdPlnd++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuRout object with data from the supplemental main table main.sd_rout.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdRout() { mSdRout++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuTown object with data from the supplemental main table main.sd_town.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdTown() { mSdTown++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuTowr object with data from the supplemental main table main.sd_towr.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdTowr() { mSdTowr++; }

        /// <summary>
        /// This method counts the number of ODBC fetch requests to populate
        /// an SuTraf object with data from the supplemental main table main.sd_traf.
        /// </summary>
        /// <param name=""></param>
        public static void IncrementSdTraf() { mSdTraf++; }

        //----------------------------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that provides
        /// the current values of all of FetchCount's internal (private) counters.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public new static string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nFetchCount.MeAnte = " + mMeAnte.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.MeAzim = " + mMeAzim.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.MeChan = " + mMeChan.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.MeSite = " + mMeSite.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.MtAnte = " + mMtAnte.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.MtChan = " + mMtChan.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.MtSite = " + mMtSite.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdAntd = " + mSdAntd.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdAnte = " + mSdAnte.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdBand = " + mSdBand.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdCtx  = " + mSdCtx.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdCtxd = " + mSdCtxd.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdEqpt = " + mSdEqpt.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdNote = " + mSdNote.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdOper = " + mSdOper.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdPlan = " + mSdPlan.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdPlnd = " + mSdPlnd.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdRout = " + mSdRout.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdTown = " + mSdTown.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdTowr = " + mSdTowr.ToString("N0").PadLeft(8));
            sb.Append("\nFetchCount.SdTraf = " + mSdTraf.ToString("N0").PadLeft(8));

            return sb.ToString();
        }

    }
}
