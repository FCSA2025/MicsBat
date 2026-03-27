using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    using _Configuration;
    using _NewLib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class encapsulates all the ODBC nullInd values for a populated MeSiteStr object.
    /// </summary>
    public class MeSiteStrNulls
    {
        public SQLLEN[] anSiteNull;
        public SQLLEN[][] anAntsNullPtr;
        public SQLLEN[][] anChanNullPtr;
        public SQLLEN[][] anAzimNullPtr;

        //------------------------------------------------------------------------------

        //The total number of fields.
        public const int NUM_COLUMNS = 4;

        //------------------------------------------------------------------------------

        public enum Init { ALLOCATED, UNALLOCATED }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private MeSiteStrNulls() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initMode"></param>
        /// <returns></returns>
        public MeSiteStrNulls(Init initMode)
        {
            if (initMode == Init.UNALLOCATED)
            {
                anSiteNull = null;
            }
            else if (initMode == Init.ALLOCATED)
            {
                anSiteNull = NullHelper.CreateArrayOfNullInd(MeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            }

            anAntsNullPtr = null;  // We don't know anything about the array dimensions.
            anChanNullPtr = null;  // We don't know anything about the array dimensions.
            anAzimNullPtr = null;  // We don't know anything about the array dimensions.
        }








    }
}
