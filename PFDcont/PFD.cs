using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace PFDcont
{
    /// <summary>
    /// This class encapsulates the majority of the user-defined
    /// parameters and intermediate values used in the PFDcont calculations; this
    /// serves to significantly reduce the number arguments in the higher-level method calls.
    /// </summary>
    public class PFD
    {
        // Values prescribed on the command line.
        private PFDcalcMode mCalcMode = PFDcalcMode.PFDCONTOUR;
        private PFDloss mLoss = PFDloss.SPHERICAL;
        private PFDclimate mClimate = PFDclimate.CONTINENTAL;
        private bool mIsReport = false;
        private bool mIsCSV = false;
        private bool mIsMapInfo = false;
        private string mReportPath = "";
        private string mCall1 = "";     // call sign
        private string mLatStr = "";    // latitude
        private string mLongStr = "";   // longitude
        private string mAcode = "";     // antenna code
        private double mFtxheight;      // antenna height
        private double mFAz;            // azimuth
        private double mFptx;           // transmit power
        private double mFsl;            // antenna feed system loss
        private double mGrnd;           // ground height
        private double mFrxheight;      // Rx antenna height
        private double mBandwidth;      // bandwidth
        private double mFrequency;      // frequency
        private double mDLa;            // atmospheric Attenuation
        private double mMinpfdlevel;    // minimum pfd level
        private double mMaxpfdlevel;    // maximum pfd level
        private double mMinrxpwr;       // minimum Rx power for coverage

        // Derived values.
        private bool mIsCalculated = false;
        private string mBaseFileName = "";
        private string mLocation = "";
        private double mLatitude;
        private double mLongitude;
        private string mFaxref;         // antenna code of Xref antenna.
        private double mdAntGain;

        public double AnteGain { get { return mdAntGain; } set { mdAntGain = value; } }
        public string AnteCodeOfXref { get { return mFaxref; } set { mFaxref = value; } }
        public double Latitude { get { return mLatitude; } set { mLatitude = value; } }
        public double Longitude { get { return mLongitude; } set { mLongitude = value; } }
        public string Location { get { return mLocation; } set { mLocation = value; } }
        public string BaseFileName { get { return mBaseFileName; } set { mBaseFileName = value; } }
        public string BaseReportPath { get { return mReportPath; } set { mReportPath = value; } }

        public bool IsReport { get { return mIsReport; } set { mIsReport = value; } }
        public bool IsCalculated { get { return mIsCalculated; } set { mIsCalculated = value; } }
        public bool IsCSV { get { return mIsCSV; } set { mIsCSV = value; } }
        public bool IsMapInfo { get { return mIsMapInfo; } set { mIsMapInfo = value; } }
        public PFDclimate Climate { get { return mClimate; } set { mClimate = value; } }
        public double TxHeight { get { return mFtxheight; } set { mFtxheight = value; } }
        public double TxAzim { get { return mFAz; } set { mFAz = value; } }
        public double TxPwr { get { return mFptx; } set { mFptx = value; } }
        public double Fsl { get { return mFsl; } set { mFsl = value; } }
        public double Grnd { get { return mGrnd; } set { mGrnd = value; } }
        public double RxHeight { get { return mFrxheight; } set { mFrxheight = value; } }
        public double Bandwidth { get { return mBandwidth; } set { mBandwidth = value; } }
        public double Frequency { get { return mFrequency; } set { mFrequency = value; } }
        public double AtmosAtten { get { return mDLa; } set { mDLa = value; } }
        public double MinPFDlevel { get { return mMinpfdlevel; } set { mMinpfdlevel = value; } }
        public double MaxPFDlevel { get { return mMaxpfdlevel; } set { mMaxpfdlevel = value; } }
        public double MinRxPwr { get { return mMinrxpwr; } set { mMinrxpwr = value; } }
        public string AnteCode { get { return mAcode; } set { mAcode = value; } }

        public string LatStr
        {
            get { return mLatStr; }
            set { mLatStr = value; }
        }

        public string LongStr
        {
            get { return mLongStr; }
            set { mLongStr = value; }
        }

        public PFDcalcMode CalcMode
        {
            get { return mCalcMode; }
            set { mCalcMode = value; }
        }

        public PFDloss Eloss
        {
            get { return mLoss; }
            set { mLoss = value; }
        }

        public string Call1
        {
            get { return mCall1; }
            set { mCall1 = value; }
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n 0.(database name)");
            sb.Append("\n 1. mCalcMode     = " + mCalcMode);
            sb.Append("\n 2. mLoss         = " + mLoss);
            sb.Append("\n 3. mCall1        = " + mCall1);
            sb.Append("\n 4. mLatStr       = " + mLatStr);
            sb.Append("\n 5. mLongStr      = " + mLongStr);
            sb.Append("\n 6. mAcode        = " + mAcode);
            sb.Append("\n 7. mFtxheight    = " + mFtxheight);
            sb.Append("\n 8. mFAz          = " + mFAz);
            sb.Append("\n 9. mFptx         = " + mFptx);
            sb.Append("\n10. mFsl          = " + mFsl);
            sb.Append("\n11. mGrnd         = " + mGrnd);
            sb.Append("\n12. mFrxheight    = " + mFrxheight);
            sb.Append("\n13. mBandwidth    = " + mBandwidth);
            sb.Append("\n14. mFrequency    = " + mFrequency);
            sb.Append("\n15. mDLa          = " + mDLa);
            sb.Append("\n16. mMinpfdlevel  = " + mMinpfdlevel);
            sb.Append("\n17. mMaxpfdlevel  = " + mMaxpfdlevel);
            sb.Append("\n16. mMinrxpwr     = " + mMinrxpwr);
            sb.Append("\n17. mClimate      = " + mClimate);
            sb.Append("\n18. mIsReport     = " + mIsReport);
            sb.Append("\n18. mIsCSV        = " + mIsCSV);
            sb.Append("\n18. mIsMapInfo    = " + mIsMapInfo);
            sb.Append("\n19. mReportPath   = " + mReportPath);
            sb.Append("\n(D) mIsCalculated = " + mIsCalculated);
            sb.Append("\n(D) mBaseFileName = " + mBaseFileName);
            sb.Append("\n(D) mLocation     = " + mLocation);
            sb.Append("\n(D) mLatitude     = " + mLatitude);
            sb.Append("\n(D) mLongitude    = " + mLongitude);
            sb.Append("\n(D) mFaxref       = " + mFaxref);
            sb.Append("\n(D) mdAntGain     = " + mdAntGain);

            return sb.ToString();
        }





    }
}
