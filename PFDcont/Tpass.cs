using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFDcont
{
    /// <summary>
    /// This is a convenience class that encapsulates the following triad of scalar values: 
    /// frequency, distance to horizon, and absorption coefficient.
    /// </summary>
    public class Tpass
    {
        public double mHorDist;
        public double mFreqMHz;
        public double mLa;

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public Tpass()
        {
            mHorDist = Double.MinValue;
            mFreqMHz = Double.MinValue;
            mLa = Double.MinValue;
        }


    }
}
