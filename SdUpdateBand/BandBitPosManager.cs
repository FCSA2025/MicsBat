using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;

namespace SdUpdateBand
{
    using System.Collections.Generic;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that manage the assignment of bandbitpos values
    /// for new records that are to be inserted into the SDB table main.sd_band; bandbitpos
    /// values must be unique and in the range [1, 256].
    /// </summary>
    public class BandBitPosManager
    {
        private static Dictionary<int, bool> mBandBitPosDict;
        private static int mNextAvailable;

        /// <summary>
        /// The static constructor for this class; instantiates the bandbitpos dictionary
        /// that is private to this class.
        /// </summary>
        static BandBitPosManager()
        {
            mBandBitPosDict = new Dictionary<int, bool>();
            mNextAvailable = 0;
        }


        /// <summary>
        /// This method should be called first; it creates a private Dictionary<int, bool> and 
        /// populates it with 'keys' 1 to 128 representing candidate bandbitpos values; the
        /// associated 'value' is set to true if that bandbitpos value is already used in the SDB
        /// table main.sd_band otherwise it is set to false; this data structure effectively
        /// maps the space of 'used' and 'unused/available' bandbitpos values.
        /// </summary>
        /// <returns></returns>
        public static int Initialize()
        {
            int rc;
            int i;
            int nCursor;
            SdBand sdBand;
            SQLLEN[] nullInd; //[SD_BAND_SIZE_];

            // Instantiate the bandbitpos dictionary.
            for (i = 1; i <= Constant.MAX_BAND_CODES; i++)
            {
                mBandBitPosDict.Add(i, false);  // false = unused.
            }

            //	Pull in the bands from the main band table ordered by bandbitpos.
            nCursor = DynSdbBand.SdSelectBand("", "bandbitpos");

            while ((rc = DynSdbBand.SdFetchBand(nCursor, out sdBand, out nullInd)) == Constant.SUCCESS)
            {
                bool inRange = (sdBand.bandbitpos >= 1) && (sdBand.bandbitpos <= Constant.MAX_BAND_CODES);

                if (inRange)
                {
                    // Set the dictionary 'value' for this 'key' as true, meaning
                    // that bandbitpos value is currently used in main.sd_band and
                    // so is not available to new records.
                    mBandBitPosDict[sdBand.bandbitpos] = true;
                }
                else
                {
                    // Something extraordinary just happened.
                    Log2.e("\nBandBitPosManager.Initialize(): ERROR: bandbitpos read from main.sd_band record is out of range ?!: " + sdBand.bandbitpos);
                    Console.Write("\r\nERROR: bandbitpos {0} read from main.sd_band record is out of range ?!", sdBand.bandbitpos);
                    Application.ExitQuietly(666);
                }

            } // end of fetch-loop

            // Check that the while-fetch-loop terminated because it read all the available records.
            if (rc != ODBC.SQL_NO_DATA)
            {
                // Something extraordinary just happened.
                Log2.e("\nBandBitPosManager.Initialize(): ERROR: while-fetch-loop terminated with rc = " + rc);
                Console.Write("\r\nERROR: BandBitPosManagerGetNextBitPos(): while-fetch-loop terminated with rc = " + rc);
                Application.ExitQuietly(666);
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method searches the bandbitpos Dictionary looking for the first element whose
        /// 'value' is false (i.e. 'available'); the element's 'key' is returned as the next available
        /// bandbitpos that can be assigned to a new main.sd_band record; this assignment is
        /// only provisional until the method Commit() is called.
        /// </summary>
        /// <returns></returns>
        public static int GetNextAvailable()
        {
            int i;
            bool foundNextAvailable = false;

            // Search through the Dictionary to identify the next, lowest available 
            // bandbitpos value.
            for (i = 1; i <= Constant.MAX_BAND_CODES; i++)
            {
                if (!mBandBitPosDict[i])
                {
                    // We have found a dictionary element whose 'value' is false (unused).
                    mNextAvailable = i;
                    foundNextAvailable = true;
                    break;
                }
            }

            // Check for unhappy ending.
            if (!foundNextAvailable)
            {
                Log2.e("\n\nBandBitPosManager.GetNextAvailable(): ERROR: no available bandbitpos.");
                Console.Write("\r\nERROR: no available bandbitpos - all 128 are in use in main.sd_band.");
                Application.ExitQuietly(666);
            }

            return mNextAvailable;
        }

        /// <summary>
        /// This method marks the bandbitpos returned by the previous call to GetNextAvailable()
        /// as being 'used' (its Dictionary element 'value' is set to true);
        /// </summary>
        /// <returns></returns>
        public static int Commit()
        {
            mBandBitPosDict[mNextAvailable] = true;  // true = used.

            return (Constant.SUCCESS);
        }







    }
}
