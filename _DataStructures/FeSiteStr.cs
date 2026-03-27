using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates an FeSite object and its associated FeAnte[],
    /// FeChan[] and FeAzim[] arrays.
    /// </summary>
    public class FeSiteStr
    {
        public FeSite stSite;      // Storage for the site information  
        public FeAnte[] stAnts;      // Array of antennas  
        public FeChan[] stChan;      // Array of channels   
        public FeAzim[] stAzim;      // Array of azimuth information       
        public int nNumChans;      // Number of channel element 
        public int nNumAnts;       // Number of antennae elements								
        public int nNumAzim;       // Number of azimuth element  
        public int nDepth;          // Depth user requested

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public FeSiteStr()
        {
            Initialize();
        }

        /// <summary>
        /// This method initializes the value of all member fields.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            stSite = null;
            stAnts = null;
            stChan = null;
            stAzim = null;

            nNumChans = 0;
            nNumAnts = 0;
            nNumAzim = 0;
            nDepth = 0;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="feSiteStrNulls"></param>
        /// <returns></returns>
        public string ToStringWN(FeSiteStrNulls feSiteStrNulls)
        {
            bool depthOK = true;
            bool siteOK = true;
            bool anteOK = true;
            bool chanOK = true;
            bool azimOK = true;

            string depthMessage = "N/A";
            string siteMessage = "N/A";
            string anteMessage = "N/A";
            string chanMessage = "N/A";
            string azimMessage = "N/A";

            // Pre-validate the depth information ---------------------------------------
            if (!(nDepth >= 1 && nDepth <= 3)) // not in range
            {
                depthOK = false;
                depthMessage = "FeSiteStr.ToStringWN(): ERROR: nDepth must be 1, 2 or 3.";
                goto marker;
            }

            // Validate the site information --------------------------------------------

            // Test for feSiteStrNulls being null.
            if (feSiteStrNulls == null)
            {
                siteOK = false;
                siteMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls is null.";
                goto marker;
            }

            // Test for member stSite being null.
            if (stSite == null)
            {
                siteOK = false;
                siteMessage = "FeSiteStr.ToStringWN(): ERROR: stSite is null.";
                goto marker;
            }

            // Test for feSiteStrNulls.anSiteNull being null
            if (feSiteStrNulls.anSiteNull == null)
            {
                siteOK = false;
                siteMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anSiteNull is null.";
                goto marker;
            }

            // If we reached here site information is OK.
            siteOK = true;
            siteMessage = "OK";

            // Revalidate the depth.
            if (nDepth >= 1)
            {
                depthOK = true;  // So far....
                depthMessage = "OK";
            }
            else
            {
                depthOK = false;
                depthMessage = "FeSiteStr.ToStringWN(): ERROR: stSite is OK so nDepth must be 1, 2 or 3.";
                goto marker;
            }

            // Validate the antennae information --------------------------------------------

            // Screen by nDepth.
            if (!(nDepth >= 2 && nDepth <= 3)) // not in range
            {
                anteOK = false;
                goto marker;
            }

            // Test for valid nNumAnts.
            if (nNumAnts < 0)
            {
                anteOK = false;
                anteMessage = "FeSiteStr.ToStringWN(): ERROR: nDepth > 1 so nNumAnts must be >= 0";
                goto marker;
            }

            // Test stAnts for null.
            if (stAnts == null)
            {
                anteOK = false;
                anteMessage = "FeSiteStr.ToStringWN(): ERROR: stAnts is null.";
                goto marker;
            }

            // Test feSiteStrNulls.anAntsNull for null.
            if (feSiteStrNulls.anAntsNull == null)
            {
                anteOK = false;
                anteMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anAntsNull is null.";
                goto marker;
            }

            // Test the antennae array dimensions.
            if (stAnts.Length != nNumAnts)
            {
                anteOK = false;
                anteMessage = "FeSiteStr.ToStringWN(): ERROR: stAnts.Length != nNumAnts.";
                goto marker;
            }

            // Test the antennae nulls array dimensions.
            if (feSiteStrNulls.anAntsNull.Length != nNumAnts)
            {
                anteOK = false;
                anteMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anAntsNull.Length != nNumAnts.";
                goto marker;
            }

            // If we reached here the antennae information is OK.
            anteOK = true;
            anteMessage = "OK";

            // Validate the aximuth information -------------------------------------------

            // Test for valid nNumAzim.
            if (nNumAzim < 0)
            {
                azimOK = false;
                azimMessage = "FeSiteStr.ToStringWN(): ERROR: nDepth > 1 so nNumAzim must be >= 0";
                goto marker;
            }

            // Test stAzim for null.
            if (stAzim == null)
            {
                azimOK = false;
                azimMessage = "FeSiteStr.ToStringWN(): ERROR: stAzim is null.";
                goto marker;
            }

            // Test feSiteStrNulls.anAzimNull for null.
            if (feSiteStrNulls.anAzimNull == null)
            {
                azimOK = false;
                azimMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anAzimNull is null.";
                goto marker;
            }

            // Test the azimuth array dimensions.
            if (stAzim.Length != nNumAzim)
            {
                azimOK = false;
                azimMessage = "FeSiteStr.ToStringWN(): ERROR: stAzim.Length != nNumAzim.";
                goto marker;
            }

            // Test the azimuth nulls array dimensions.
            if (feSiteStrNulls.anAzimNull.Length != nNumAzim)
            {
                azimOK = false;
                azimMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anAzimNull.Length != nNumAzim.";
                goto marker;
            }

            // If we reached here the azimuth information is OK.
            azimOK = true;
            azimMessage = "OK";

            // Validate the channel information --------------------------------------------

            // Screen by nDepth.
            if (nDepth != 3) // not in range
            {
                chanOK = false;
                goto marker;
            }

            // Test for valid nNumChan.
            if (nNumChans < 0)
            {
                chanOK = false;
                chanMessage = "FeSiteStr.ToStringWN(): ERROR: nDepth ==3  so nNumChan must be >= 0";
                goto marker;
            }

            // Test stChan for null.
            if (stChan == null)
            {
                chanOK = false;
                chanMessage = "FeSiteStr.ToStringWN(): ERROR: stChan is null.";
                goto marker;
            }

            // Test feSiteStrNulls.anChanNull for null.
            if (feSiteStrNulls.anChanNull == null)
            {
                chanOK = false;
                chanMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anChanNull is null.";
                goto marker;
            }

            // Test the channel array dimensions.
            if (stChan.Length != nNumChans)
            {
                chanOK = false;
                chanMessage = "FeSiteStr.ToStringWN(): ERROR: stChan.Length != nNumChans.";
                goto marker;
            }

            // Test the channel nulls array dimensions.
            if (feSiteStrNulls.anChanNull.Length != nNumChans)
            {
                chanOK = false;
                chanMessage = "FeSiteStr.ToStringWN(): ERROR: feSiteStrNulls.anChanNull.Length != nNumChans.";
                goto marker;
            }

            // If we reached here the channel information is OK.
            chanOK = true;
            chanMessage = "OK";

            //------------------------------------------------------------------------------

            marker: // end of qualification.

            StringBuilder sb = new StringBuilder();

            // Report top-level meta data.
            sb.AppendLine("=========== FeSiteStr.ToStringWN(): report ===========");
            sb.AppendLine("nDepth    : " + depthMessage);
            sb.AppendLine("nSite     : " + siteMessage);
            sb.AppendLine("nNumAnts  : " + anteMessage);
            sb.AppendLine("nNumAzim  : " + azimMessage);
            sb.AppendLine("nNumChans : " + chanMessage);
            sb.AppendLine("===================");
            sb.AppendLine("nDepth    : " + nDepth);
            sb.AppendLine("nNumAnts  : " + nNumAnts);
            sb.AppendLine("nNumAzim  : " + nNumAzim);
            sb.AppendLine("nNumChans : " + nNumChans);

            if (depthOK)
            {
                if (siteOK)
                {
                    sb.AppendLine(stSite.ToStringWN(feSiteStrNulls.anSiteNull));
                }

                if (anteOK)
                {
                    for (int i = 0; i < stAnts.Length; i++)
                    {
                        sb.AppendLine("stAnts[" + i + "]");
                        sb.AppendLine(stAnts[i].ToStringWN(feSiteStrNulls.anAntsNull[i]));
                    }
                }

                if (azimOK)
                {
                    for (int i = 0; i < stAzim.Length; i++)
                    {
                        sb.AppendLine("stAzim[" + i + "]");
                        sb.AppendLine(stAzim[i].ToStringWN(feSiteStrNulls.anAzimNull[i]));
                    }
                }

                if (chanOK)
                {
                    for (int i = 0; i < stChan.Length; i++)
                    {
                        sb.AppendLine("stChan[" + i + "]");
                        sb.AppendLine(stChan[i].ToStringWN(feSiteStrNulls.anChanNull[i]));
                    }
                }
            }

            return sb.ToString();
        }





    }
}
