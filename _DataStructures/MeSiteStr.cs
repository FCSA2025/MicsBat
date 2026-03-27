using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates an MeSite object and its associated MeAnte[],
    /// MeAzim[] and MtChan[] arrays.
    /// </summary>
    public class MeSiteStr
    {
        public MeSite stSite;      //	Storage for the site information.
        public MeAnte[] stAntsPtr;    //	Array of antenna information.
        public MeChan[] stChanPtr;    //	Array of channel information.
        public MeAzim[] stAzimPtr;    //  Array of azimuth information.
        public int nNumChans;      //  Number of Channels.
        public int nNumAnts;       //  Number of Antennas.
        public int nNumAzim;       //  Number of Azimuth points.
        public int nDepth;          //	Depth user requested.		

        //-----------------------------------------------------------------

        //The total number of fields.
        public const int NUM_COLUMNS = 8;

        //------------------------------------------------------------------

        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public MeSiteStr(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                stSite = null;

            }
            else if (initMode == Init.ALLOCATED)
            {
                stSite = new MeSite();
            }

            stAntsPtr = null;  // We don't know anything about the array dimensions.
            stChanPtr = null;  // We don't know anything about the array dimensions.
            stAzimPtr = null;  // We don't know anything about the array dimensions.
            nNumChans = 0;
            nNumAnts = 0;
            nNumAzim = 0;
            nDepth = -666;  //This is deliberately set to an invalid value;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n");
            sb.Append("\r\n===== MeSiteStr (Start) =====");
            sb.Append("\r\nnNumChans = " + nNumChans);
            sb.Append("\r\nnNumAnts = " + nNumAnts);
            sb.Append("\r\nnNumAzim = " + nNumAzim);
            sb.Append("\r\nnDepth = " + nDepth);

            sb.Append("\r\n");
            sb.Append("\r\n===== MeSite =====");
            sb.Append(stSite);

            for (int index = 0; index < nNumAnts; index++)
            {
                sb.Append("\r\n===== MeAnte [" + index + "] Key Only =====");
                sb.Append(stAntsPtr[index].KeysToString());
            }

            for (int index = 0; index < nNumChans; index++)
            {
                sb.Append("\r\n===== MeChan [" + index + "] Key Only =====");
                sb.Append(stChanPtr[index].KeysToString());
            }

            for (int index = 0; index < nNumChans; index++)
            {
                sb.Append("\r\n===== MeAzim [" + index + "] Key Only =====");
                sb.Append(stAzimPtr[index].KeysToString());
            }

            sb.Append("\r\n===== MtSiteStr (End) =====");
            sb.Append("\r\n");
            return sb.ToString();
        }



    }
}
