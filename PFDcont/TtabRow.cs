using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFDcont
{
    /// <summary>
    /// This class encapsulates the several scalar values that comprise a (per azimuth) 'row' 
    /// of results data. 
    /// </summary>
    public class TtabRow
    {
        public double dAz;
        public double dGain;
        public double dmindist;
        public string cminlat;
        public double dminlat;
        public string cminlong;
        public double dminlong;
        public double dmaxdist;
        public string cmaxlat;
        public double dmaxlat;
        public string cmaxlong;
        public double dmaxlong;

        //--------------------------------------------------------------------

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public TtabRow()
        {
            const double DMIN = Double.MinValue;
            dAz = DMIN;
            dGain = DMIN;
            dmindist = DMIN;
            cminlat = "";
            dminlat = DMIN;
            cminlong = "";
            dminlong = DMIN;
            dmaxdist = DMIN;
            cmaxlat = "";
            dmaxlat = DMIN;
            cmaxlong = "";
            dmaxlong = DMIN;
        }

        //--------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\ndAz      = " + dAz);
            sb.Append("\ndGain    = " + dGain);
            sb.Append("\ndmindist = " + dmindist);
            sb.Append("\ncminlat  = " + cminlat);
            sb.Append("\ndminlat  = " + dminlat);
            sb.Append("\ncminlong = " + cminlong);
            sb.Append("\ndminlong = " + dminlong);
            sb.Append("\ndmaxdist = " + dmaxdist);
            sb.Append("\ncmaxlat  = " + cmaxlat);
            sb.Append("\ndmaxlat  = " + dmaxlat);
            sb.Append("\ncmaxlong = " + cmaxlong);
            sb.Append("\ndmaxlong = " + dmaxlong);

            return sb.ToString();
        }




    }
}

