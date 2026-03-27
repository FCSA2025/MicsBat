using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;
using _NewLib;

namespace _Utillib
{
    using _Configuration;

    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// Provides methods that retrieve records from the subupt tables.
    /// Despite the fact that most of the access is to the database and 
    /// therefore the SD tables, we use the SU structures (representing 
    /// the input files) as they are a superset of the fields in the database.
    /// </summary>
    public class Suutils
    {
#if PINVOKE
        [DllImport("_APItest.dll", CharSet = CharSet.Ansi)]
        private static extern int API_SizeOf_suPlnd_();
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetNote([In] string cOperCode, [In] string cNoteNum, [In, Out] SuNote sctNote);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetOper([In] string cOperCode, [Out] SuOper suOper);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetTown([In] string cCall1, [In] short nTowerNo, [In, Out] SuTown pTown);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetBand([In] string cBand, [Out] out IntPtr intPtrToSuBand);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetAnt([In] string cAntCode, [Out] out IntPtr intPtrToSuAntStruct);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetAnt([In] string cAntCode, [In, Out] ref SuAntStr suAntStr);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void freeBand([In, Out] SuBand ptBand);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void freeAnt([In, Out] SuAntStr ptBand);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int getLocAnteGain([In] string acodeIn, [Out] out double againOut);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetEqpt([In] string cEqptCode, [In, Out] SuEqpt suEqpt);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetFeeCode([In] string cFeeCode, [In, Out] SuFeeCode suFeeCode);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetTraf([In] string cTraf, [In] string cEqpt, [In, Out] SuTraf suTraf);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetRout([In] string cOper, [In] string cRouteNo, [In, Out] SuRout suRout);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suGetPlan([In] string cPlan, [In] string cBand, [Out] SuPlan pPlan, [In, Out] IntPtr intPtr, [Out] out int nNum);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int suPlndFindFreq([In] SuPlnd[] suPlnd, [In] int nNum, [In] int nSet, [In] double dFreq);

        public static int GetLocAnteGain_NATIVE(string acodeIn, out double againOut)
        {
            return getLocAnteGain(acodeIn, out againOut);
        }

        public static int SuGetAnt_NATIVE(string cAntCode, out SuAntStr suAntStr)
        {
            //...Log2.v("\n\nSuutils.SuGetAnt_NATIVE: Entry");

            int nRet = -666;

            // Get an IntPtr to enough global memory to hold an instance of SuAntStr.
            suAntStr = new SuAntStr(SuAntStr.Init.UNALLOCATED);
            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(suAntStr));

            // Call the native function:   
            // int suGetAnt(const char * cAntCode, suAntStruct** AntStrct)
            nRet = suGetAnt(cAntCode, out intPtr);

            //Reverse marshal suAntStr.
            Marshal.PtrToStructure(intPtr, suAntStr);

            //...Log2.v("\r\nSuutils.SuGetAnt_NATIVE: suAntStr:\r\n" + suAntStr.ToString());

            //...Log2.v("\n\nSuutils.SuGetAnt_NATIVE: Exit");
            return nRet;
        }

        public static void FreeBand_NATIVE(SuBand ptBand)
        {
            freeBand(ptBand);
        }

        public static void FreeAnt_NATIVE(SuAntStr ant)
        {
            freeAnt(ant);
        }

        public static int SuGetBand_NATIVE(string cBand, out SuBand suBand)
        {
            //...Log2.v("\n\nSuutils.SuGetBand_NATIVE: Entry");

            int nRet = -666;
            suBand = new SuBand();

            // Get an IntPtr to receive an SuBand object.
            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(suBand));

            // Call the native function:   
            // int suGetBand(const char* cBand, struct suBand_ ** pBand)
            nRet = suGetBand(cBand, out intPtr);

            // Reverse Marshal an instance of SuBand.
            Marshal.PtrToStructure(intPtr, suBand);

            //...Log2.v("\r\nSuutils.SuGetBand_NATIVE: suBand:\r\n" + suBand.ToString());

            //...Log2.v("\n\nSuutils.SuGetBand_NATIVE: Exit");
            return nRet;
        }

        public static int SuGetTown_NATIVE(string cCall1, short nTowerNo, out SuTown pTown)
        {
            //...Log2.v("\n\nSuutils.SuGetTown_NATIVE: Entry");

            pTown = new SuTown();
            // Native call:
            // int suGetTown(char * cCall1,	short nTowerNo, SUTOWN  *pTown);
            int rc = suGetTown(cCall1, nTowerNo, pTown);

            //...Log2.v("\r\nSuutils.SuGetTown_NATIVE: pTown:\r\n" + pTown.ToString());

            //...Log2.v("\n\nSuutils.SuGetTown_NATIVE: Exit");
            return rc;
        }

        public static int SuGetNote_NATIVE(string cOperCode, string cNoteNum, out SuNote sctNote)
        {
            //...Log2.v("\n\nSuutils.SuGetNote_NATIVE: Entry");

            sctNote = new SuNote();
            // Native code:
            // int suGetNote(const char * cOperCode, const char* cNoteNum, SUNOTE * sctNote);	
            int rc = suGetNote(cOperCode, cNoteNum, sctNote);

            //...Log2.v("\r\nSuutils.SuGetNote_NATIVE: sctNote:\r\n" + sctNote.ToString());

            //...Log2.v("\n\nSuutils.SuGetNote_NATIVE: Exit");
            return rc;
        }

        public static int SuGetOper_NATIVE(string cOperCode, out SuOper suOper)
        {
            //...Log2.v("\n\nSuutils.SuGetOper_NATIVE: Entry");

            suOper = new SuOper();

            // Native code:
            // int suGetOper(const char * cOperCode, SUOPER * sctOper)	
            int rc = suGetOper(cOperCode, suOper);

            //...Log2.v("\r\nSuutils.SuGetOper_NATIVE: suOper:\r\n" + suOper.ToString());

            //...Log2.v("\n\nSuutils.SuGetOper_NATIVE: Exit");
            return rc;
        }

        public static int SuGetEqpt_NATIVE(string cEqptCode, out SuEqpt suEqpt)
        {
            //...Log2.v("\n\nSuutils.SuGetEqpt_NATIVE: Entry");

            suEqpt = new SuEqpt();

            // Native code:
            int rc = suGetEqpt(cEqptCode, suEqpt);

            //...Log2.v("\r\nSuutils.SuGetEqpt_NATIVE: suEqpt:\r\n" + suEqpt.ToString());

            //...Log2.v("\n\nSuutils.SuGetEqpt_NATIVE: Exit");
            return rc;
        }

        public static int SuGetFeeCode_NATIVE(string cFeeCode, out SuFeeCode suFeeCode)
        {
            //...Log2.v("\n\nSuutils.SuGetFeeCode_NATIVE: Entry");

            suFeeCode = new SuFeeCode();

            // Native code:
            int rc = suGetFeeCode(cFeeCode, suFeeCode);

            //...Log2.v("\r\nSuutils.SuGetFeeCode_NATIVE: suFeeCode:\r\n" + suFeeCode.ToString());

            //...Log2.v("\n\nSuutils.SuGetFeeCode_NATIVE: Exit");
            return rc;
        }

        public static int SuGetTraf_NATIVE(string cTraf, string cEqpt, out SuTraf suTraf)
        {
            //...Log2.v("\n\nSuutils.SuGetTraf_NATIVE: Entry: " + cTraf + " : " + cEqpt);

            suTraf = new SuTraf();

            // Native code:
            int rc = suGetTraf(cTraf, cEqpt, suTraf);

            //...Log2.v("\r\nSuutils.SuGetTraf_NATIVE: suTraf:\r\n" + suTraf.ToString());

            //...Log2.v("\n\nSuutils.SuGetTraf_NATIVE: Exit");
            return rc;
        }

        public static int SuGetRout_NATIVE(string cOper, string cRouteNo, out SuRout suRout)
        {
            //...Log2.v("\n\nSuutils.Suutils.SuGetRout_NATIVE: Entry");

            suRout = new SuRout();

            // Native code:
            int rc = suGetRout(cOper, cRouteNo, suRout);

            //...Log2.v("\r\nSuutils.SuGetRout_NATIVE: suRout:\r\n" + suRout.ToString());

            //...Log2.v("\n\nSuutils.SuGetRout_NATIVE: Exit");
            return rc;
        }

        public static int SuGetPlan_NATIVE(string cPlan, string cBand, out SuPlan suPlan, bool getListPlanReqs, out SuPlnd[] suPlnd, out int nNum)
        {
            //...Log2.v("\n\nSuutils.SuGetPlan_NATIVE: Entry");

            suPlan = new SuPlan();
            IntPtr intPtr;
            if (getListPlanReqs)
            {
                intPtr = Marshal.AllocHGlobal(1);   //Just need to set the pointer to a non-zero value.
            }
            else
            {
                intPtr = IntPtr.Zero;
            }

            // Native code:
            int rc = suGetPlan(cPlan, cBand, suPlan, intPtr, out nNum);

            // Create and fill the SuPlnd[].
            suPlnd = Arrays.CreateArrayUsingDefaultElementConstructor<SuPlnd>(nNum);

            for (int i = 0; i < nNum; i++)
            {
                IntPtr offsetIntPtr = IntPtr.Add(intPtr, i * API_SizeOf_suPlnd_());
                Marshal.PtrToStructure(offsetIntPtr, suPlnd[i]);
            }

            //...Log2.v("\r\nSuutils.SuGetPlan_NATIVE: suPlan:\r\n" + suPlan.ToString());

            //...Log2.v("\n\nSuutils.SuGetPlan_NATIVE: Exit");

            return rc;
        }

        public static int SuPlndFindFreq_NATIVE(SuPlnd[] suPlnd, int nNum, int nSet, double dFreq)
        {
            //...Log2.v("\n\nSuutils.SuPlndFindFreq_NATIVE: Entry");

            int rc;
            if (suPlnd == null)
            {
                rc = -2;
            }
            else
            {
                // Native code:
                rc = suPlndFindFreq(suPlnd, nNum, nSet, dFreq);
                //...Log2.v("\r\nSuutils.SuPlndFindFreq_NATIVE: suPlan:\r\n" + suPlnd.ToString());
            }

            //...Log2.v("\n\nSuutils.SuPlndFindFreq_NATIVE: Exit: returned " + rc);
            return rc;
        }


#endif
        //---------------------------------------------------------------------------

        /// <summary>
        /// This class provide cache functionality for SuCtxStruct objects
        /// by extending the generic Cache class.
        /// </summary>
        public class Ctx_Cache : Cache<SuCtxStruct>
        {
            /// <summary>
            /// This method overrides the constructor in the base class and
            /// prescribes the maximum number of elements that can be stored
            /// in the cache.
            /// </summary>
            public Ctx_Cache(int maxSize) : base(maxSize) { }

            /// <summary>
            /// This method returns a 'safe' copy of a cache element
            /// that has been 'hit'. In this instance, it is safe to
            /// return access to the cached element itself as this is never
            /// modified by the caller.
            /// </summary>	
            /// <param name="t"> - SuCtxStruct object.</param>
            /// <returns></returns>
            public override SuCtxStruct SafeCopy(SuCtxStruct t)
            {
                return t;
            }

            /// <summary>
            /// This method overrides the base class method and returns true if
            /// we have a cache 'hit' for the keys {tfcr, tfci, rxeqp}.
            /// </summary>
            /// <param name="t1"> - SuCtxStruct object.</param>
            /// <param name="t2"> - SuCtxStruct object.</param>
            /// <returns></returns>
            public override bool Hit(SuCtxStruct t1, SuCtxStruct t2)
            {
                bool hitStatus = false;
                if (t1.CtxV.tfcr.Equals(t2.CtxV.tfcr) &&
                        t1.CtxV.tfci.Equals(t2.CtxV.tfci) &&
                        t1.CtxV.rxeqp.Equals(t2.CtxV.rxeqp))
                {
                    hitStatus = true;
                }
                return hitStatus;
            }
        }


        /// <summary>
        /// This class provide cache functionality for Ctx_Xref objects
        /// by extending the generic Cache class.
        /// </summary>
        public class Ctxx_Cache : Cache<Ctx_Xref>
        {
            /// <summary>
            /// This method overrides the constructor in the base class and
            /// prescribes the maximum number of elements that can be stored
            /// in the cache.
            /// </summary>
            /// <param name="maxSize"></param>
            public Ctxx_Cache(int maxSize) : base(maxSize) { }

            /// <summary>
            /// This method returns a 'safe' copy of a cache element
            /// that has been 'hit'. In this instance, it is safe to
            /// return access to the cached element itself as this is never
            /// modified by the caller.
            /// </summary>	
            /// <param name="t"></param>
            /// <returns></returns>
            public override Ctx_Xref SafeCopy(Ctx_Xref t)
            {
                return t;
            }

            /// <summary>
            /// This method overrides the base class method and returns true if
            /// we have a cache 'hit' for the keys {tfcr, tfci, rxeqp}.
            /// </summary>
            /// <param name="t1"></param>
            /// <param name="t2"></param>
            /// <returns></returns>
            public override bool Hit(Ctx_Xref t1, Ctx_Xref t2)
            {
                bool hitStatus = false;
                if (t1.tfcr.Equals(t2.tfcr) &&
                        t1.tfci.Equals(t2.tfci) &&
                        t1.rxeqp.Equals(t2.rxeqp))
                {
                    hitStatus = true;
                }
                return hitStatus;
            }
        }

        /// <summary>
        /// This class provide cache functionality for SuAntStr objects
        /// by extending the generic Cache class.
        /// </summary>
        public class Ante_Cache : Cache<SuAntStr>
        {
            /// <summary>
            /// This method overrides the constructor in the base class and
            /// prescribes the maximum number of elements that can be stored
            /// in the cache.
            /// </summary>
            /// <param name="maxSize"></param>
            public Ante_Cache(int maxSize) : base(maxSize) { }

            /// <summary>
            /// This method returns a 'safe' copy of a cache element
            /// that has been 'hit'. In this instance, the returned object
            /// might be modified by the caller so we need to output a 
            /// 'deep copy' of the 'hit' object to avoid data corruption.
            /// </summary>	
            /// <param name="t"></param>
            /// <returns></returns>
            public override SuAntStr SafeCopy(SuAntStr t)
            {
                SuAntStr suAntStr = null;

                if (t != null)
                {
                    suAntStr = t.DeepCopy();
                }

                return suAntStr;
            }

            /// <summary>
            /// This method overrides the base class method and returns true if
            /// we have a cache 'hit' for the key {acAntCode}.
            /// </summary>
            /// <param name="t1"></param>
            /// <param name="t2"></param>
            /// <returns></returns>
            public override bool Hit(SuAntStr t1, SuAntStr t2)
            {
                bool hitStatus = false;

                if (t1.acAntCode.Equals(t2.acAntCode))
                {
                    hitStatus = true;
                }

                return hitStatus;
            }
        }

        /// <summary>
        /// This class provide cache functionality for SuEqpt objects
        /// by extending the generic Cache class.
        /// </summary>
        public class Eqpt_Cache : Cache<SuEqpt>
        {
            /// <summary>
            /// This method overrides the constructor in the base class and
            /// prescribes the maximum number of elements that can be stored
            /// in the cache.
            /// </summary>
            /// <param name="maxSize"></param>
            public Eqpt_Cache(int maxSize) : base(maxSize) { }

            /// <summary>
            /// This method returns a 'safe' copy of a cache element
            /// that has been 'hit'. In this instance, the returned object
            /// might be modified by the caller so we need to output a 
            /// 'deep copy' of the 'hit' object to avoid data corruption.
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public override SuEqpt SafeCopy(SuEqpt t)
            {
                SuEqpt suEqpt = null;

                if (t != null)
                {
                    suEqpt = t.DeepCopy();
                }

                return suEqpt;
            }

            /// <summary>
            /// This method overrides the base class method and returns true if
            /// we have a cache 'hit' for the key {ecode}.
            /// </summary>
            /// <param name="t1"></param>
            /// <param name="t2"></param>
            /// <returns></returns>
            public override bool Hit(SuEqpt t1, SuEqpt t2)
            {
                bool hitStatus = false;

                if (t1.ecode.Equals(t2.ecode))
                {
                    hitStatus = true;
                }

                return hitStatus;
            }
        }

        // Band Cache.
        private static int mBandCount = 0;
        private static bool mIsBandLoaded = false;
        private static SuBand[] mpBandCache = null;
        private static BandBits mpBandBits = null;
        private static int mnglbMaxBand = -1;
        private static double mglbFreqSepForAdjacencyKHz = 0;
        private static int mnglbBitWords = 0;

        //-----------------------------------------------------------------------------------

        // Antenna Cache.
        private static Ante_Cache mAnteCacheX = new Ante_Cache(Constant.ANTCACHESIZE);
        //private static int mAnteCalls = 0;
        //private static int mAnteCacheHits = 0;         /*  Cache hits for the antennas */
        //private static short mAnteCacheNext = 0;     /*  The number of items in the cache */
        //private static AnteCache[] mAnteCache = new AnteCache[Constant.ANTCACHESIZE];

        // Equipment Cache.
        private static Eqpt_Cache mEquipCacheX = new Eqpt_Cache(Constant.ANTCACHESIZE);
        //private static int mEqptCacheNow = 0;
        //private static int mEqptCalls = 0;
        //private static int mEqptCacheHits = 0;         /*  Cache hits for the Eqptnnas */
        //private static short mEqptCacheNext = 0;     /*  The number of items in the cache */
        //private static SuEqpt[] mEqptCache = new SuEqpt[Constant.ECACHE_SIZE_];
        //private static int[] mEqptCacheWhen = new int[Constant.ECACHE_SIZE_];

        // Traffic Cache.
        private static int mTrafCacheNow = 0;
        private static int mTrafCalls = 0;
        private static int mTrafCacheHits = 0;         /*  Cache hits for the Trafnnas */
        private static short mTrafCacheNext = 0;     /*  The number of items in the cache */
        private static SuTraf[] mTrafCache = new SuTraf[Constant.TCACHE_SIZE_];
        private static int[] mTrafCacheWhen = new int[Constant.TCACHE_SIZE_];

        // CTX Cache.
        private static Ctx_Cache mCtxCache = new Ctx_Cache(Constant.CTXCACHE_SIZE_);
        //private static int mCTXCacheNow = 0;
        //private static int mCTXCalls = 0;
        //private static int mCTXCacheHits = 0;         /*  Cache hits for the Trafnnas */
        //private static short mCTXCacheNext = 0;     /*  The number of items in the cache */
        //private static SuTraf[] mCTXCache = new SuTraf[Constant.CTXCACHE_SIZE_];
        //private static int[] mCTXCacheWhen = new int[Constant.CTXCACHE_SIZE_];

        // CTX xRef Cache.
        private static Ctxx_Cache mCtxxCache = new Ctxx_Cache(Constant.CTXCACHE_SIZE_);
        //private static int mCTXxCacheNow = 0;
        //private static int mCTXxCalls = 0;
        //private static int mCTXxCacheHits = 0;         /*  Cache hits for the Trafnnas */
        //private static short mCTXxCacheNext = 0;     /*  The number of items in the cache */
        //private static SuTraf[] mCTXxCache = new SuTraf[Constant.CTXXCACHE_SIZE_];
        //private static int[] mCTXxCacheWhen = new int[Constant.CTXXCACHE_SIZE_];

        // Analog Cache.
        //private static Analog_Cache mAnalogCacheX = new Analog_Cache(Constant.ANALOGCACHE_SIZE_);
        //private static int mAnalogCacheNow = 0;
        //private static int mAnalogCalls = 0;
        //private static int mAnalogCacheHits = 0;         /*  Cache hits for the Trafnnas */
        //private static short mAnalogCacheNext = 0;     /*  The number of items in the cache */
        //private static SuTraf[] mAnalogCache = new SuTraf[Constant.ANALOGCACHE_SIZE_];
        //private static int[] mAnalogCacheWhen = new int[Constant.ANALOGCACHE_SIZE_];

        // Digital Cache.
        //private static int mDigitalCacheNow = 0;
        //private static int mDigitalCalls = 0;
        //private static int mDigitalCacheHits = 0;         /*  Cache hits for the Trafnnas */
        //private static short mDigitalCacheNext = 0;     /*  The number of items in the cache */
        //private static SuTraf[] mDigitalCache = new SuTraf[Constant.DIGITALCACHE_SIZE_];
        //private static int[] mDigitalCacheWhen = new int[Constant.DIGITALCACHE_SIZE_];

        //------------------------------------------------------------------------------------

#if false
        /// <summary>
        /// Encapsulates the data structure of an antenna cache element.
        /// </summary>
        public class AnteCacheItem
        {
            public string cchAcode;     /*  The antenna code in this slot */
            public int cchAntMRU;        /*  The Most Recently Used (MRU) count. */
            public SuAntStr cchAnt;     /*  Pointer to the antenna structure read in */

            public AnteCacheItem()
            {
                cchAcode = "";
                cchAntMRU = 0;
                cchAnt = null;
            }
        }
#endif
        /// <summary>
        /// Retrieves the gain for a prescribed antenna.
        /// </summary>
        /// <param name="acodeIn"> - antenna code.</param>
        /// <param name="againOut"> - gain.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int GetLocAnteGain(string acodeIn, out double againOut)
        {
            //...Log2.v(String.Format("\nSuUtils.GetLocAnteGain(): Entry: acodeIn = |{0}|", acodeIn));

            // Satisfy 'out' requirement.
            againOut = 0.0;

            string acode;       /* MS SQL Server aware acode */
                                //double again = 0.0;			/* MS SQL Server aware again */
                                //short againNull = 0;			/* MS SQL Server null value */

            string charPtr = null;      /* Pointer for string extraction */
            int retCode = Constant.SUCCESS;     /* Function return code */
            int tempAgain = 0;      /* Temp space for again calc's */
            int rc = 0;         /* Function call return value */


            /* Get MS SQL Server aware variable to point to antenna code */
            acode = acodeIn.Trim();

            /* If antenna code begins with '$' then is ficticious and 'again' is
             * in last three (3) digits */
            if (acode.StartsWith("$"))
            {
                /* Set charPtr to start of again value */
                charPtr = acode.Substring(acode.Length - 3);

                /* Make sure we only have digits here */
                if (!GenUtil.UtIsAllDigits(charPtr))
                {
                    retCode = -1;
                }
                else
                {
                    /* Scan again value into temp var. */
                    tempAgain = Convert.ToInt32(charPtr);
                    /* Convert int value to double */
                    againOut = (double)tempAgain / 10.0;
                }
            }
            else    /* Not a fictitious antenna code */
            {
                /* Get again from SDB */
                SuAntStr suAntStr;
                rc = SuGetAnt(acodeIn, out suAntStr);
                if (rc != 0)
                {
                    /* Antenna doesn't exist */
                    //...Log2.v(String.Format("\nSuUtils.GetLocAnteGain(): BRAZIL: Antenna doesn't exist: acodeIn = {0}", acodeIn));
                    retCode = -3;
                }
                else
                {
                    /* Return gain value to user */
                    againOut = suAntStr.acAnt.again;
                    retCode = 0;
                }
            }

            //...Log2.v(String.Format("\nSuUtils.GetLocAnteGain(): Exit: retCode = {0}", retCode));
            return (retCode);

        }

        /// <summary>
        /// Retrieves the antenna table information from the subupt table
        /// for a prescribed antenna code.
        /// </summary>
        /// If the antenna is a cross-reference
        /// then the cross-reference information is returned and the
        /// the cross reference code is placed in cAntXRef.  In all cases
        /// the antenna code returned in cAntXref is the antenna returned.
        /// </remarks>
        /// <param name="cAntCode"> - prescribed antenna code.</param>
        /// <param name="suAntStr"> - a SuAntStr object populated with information.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetAnt(string cAntCode, out SuAntStr suAntStr)
        {
            //...Log2.v("\n\nSuutils.SuGetAnt(): Entry");
            //&&Console.Error.Write("\nSuGetAnt(): cAntCode = " + cAntCode);

            // Satisfy 'out' requirement.
            suAntStr = null;

            int nCount;
            int nRetCode;
            double dGain = 0.0;

            string sAcode;         //	Search antenna code
            string cXRef = "";     //	First xref antenna
            string cModel = "";    //	First antenna model

            if (String.IsNullOrWhiteSpace(cAntCode))
            {
                Log2.e("\n\nSuutils.SuGetAnt(): ERROR: cAntCode is invalid: " + Strings.AddBars(cAntCode));
                GenUtil.SetError(5100, "suGetAnt - Invalid arguments");
                return -4;
            }

            sAcode = cAntCode.Trim();

            nCount = 0;     /* This is a counter to avoid inf loops */

            while (++nCount < 5)
            {
                /*	First follow through the antenna xref sequence */
                /*  Initialize the cross reference */

                nRetCode = SeekAnt(sAcode, out suAntStr);

                if (nRetCode != 0)
                {
                    if (nRetCode > 0)
                    {
                        //&&Console.Error.Write("\nSuutils.SuGetAnt(): A");
                        /*  Antenna not found */
                        suAntStr = null;
                        //...Log2.v("\n\nSuutils.SuGetAnt(): A: No antenna found.");
                        return 1;
                    }
                    else
                    {
                        //&&Console.Error.Write("\nSuutils.SuGetAnt(): B");
                        /*  There was an error */
                        Log2.e("\n\nSuutils.SuGetAnt(): ERROR: B");
                        return -2;
                    }
                }
                else
                {
                    //&&Console.Error.Write("\nSuutils.SuGetAnt(): C");
                    /*	Got the antenna, check to see if it is a cross reference. */
                    sAcode = suAntStr.acAnt.axref.Trim();
                    if (suAntStr.acAnt.anip > 0 || sAcode.Length == 0)
                    {
                        //&&Console.Error.Write("\nSuutils.SuGetAnt(): D");
                        /*	We have the antenna we want */
                        break;
                    }
                    else
                    {
                        //&&Console.Error.Write("\nSuutils.SuGetAnt(): E");
                        /*  We need to get the cross reference, but we save the first non-zero
                        *   antenna gain we encounter, and use that as the final antenna gain.*/
                        if (suAntStr.acAnt.again != 0.0 && dGain == 0.0)
                        {
                            dGain = suAntStr.acAnt.again;
                        }
                        /* First time around, we also save the cross reference and model */
                        if (nCount <= 1)
                        {
                            cXRef = suAntStr.acAnt.axref;
                            cModel = suAntStr.acAnt.amodel;
                        }
                    }

                }  // if ((nRetCode = SeekAnt(sAcode, out suAntStr)) != 0)

            } // End of while-loop

            //&&Console.Error.Write("\nSuutils.SuGetAnt(): F");
            if (nCount < 5)
            {
                if (dGain != 0.0)
                {
                    /*  We found an intermediate gain for this antenna.  Make this the      *\
                    \*  antenna gain. */
                    suAntStr.acAnt.again = (float)dGain;
                }
                if (nCount > 1)
                {
                    //	We want to return the information from the first antenna - that referred to.
                    suAntStr.acAnt.axref = cXRef;
                    suAntStr.acAnt.amodel = cModel;
                }
                //...Log2.v("\n\nSuutils.SuGetAnt(): Exit: returned 0");
                //&&Console.Error.Write("\nSuutils.SuGetAnt(): G");
                return 0;
            }
            else
            {
                //&&Console.Error.Write("\nSuutils.SuGetAnt(): H");
                suAntStr = null;
                Log2.e("\n\nSuutils.SuGetAnt(): ERROR: C");
                return (-3);            /*	Cross reference loop */
            }

        }

        /// <summary>
        /// Retrieves information for a prescribed band as a populated SuBand object.
        /// </summary>
        /// <param name="cBand"> - prescribed band.</param>
        /// <param name="suBand"> - populated SuBand object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetBand(string cBand, out SuBand suBand)
        {
            // Satisft 'out' requirement.
            suBand = null;

            short nInd;
            short nRet;
            string cTrimBand;
            SuBand pBandEl = null; /*  Pointer to a band element */

            /*  check to see if the band is loaded, if not load */
            if (!mIsBandLoaded)
            {
                SuLoadBands();
            }

            /*	Trim the band to search the cache */
            cTrimBand = cBand.Trim();

            /*  Search through the bands for a band code of cBand */
            for (nInd = 0; nInd < mBandCount; nInd++)
            {
                pBandEl = mpBandCache[nInd];

                if (cTrimBand.Equals(pBandEl.bndcde))
                {
                    break;
                }
            }

            if (nInd >= mBandCount)
            {
                /*  The band code was not found */
                nRet = 1;
            }
            else
            {
                /*  Band found, allocate space and copy the band back */
                // Cache can be overwritten so we need to make a deep copy.
                suBand = pBandEl.DeepCopy();
                nRet = 0;
            }

            //...Log2.v("\n\nNATIVE: suGetBand(): Exit, pBand = %s", pBand[0].bndcde);
            return nRet;

        }

        /// <summary>
        /// Retrieves tower note information for a prescribed call sign and tower number as a populated SuTown object.
        /// </summary>
        /// <param name="cCall1"> - call sign.</param>
        /// <param name="nTowerNo"> - tower number.</param>
        /// <param name="suTown"> - populated SuTown object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetTown(string cCall1, short nTowerNo, out SuTown suTown)
        {
            // Satisfy 'out' requirement.
            suTown = null;

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select call1, oper, twcode, twht, atwrno, twli, twpa, nott, tpoint, adate, sdate, mdate, mtime from main.sd_town where call1='{0}' and atwrno={1}", cCall1, nTowerNo);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return (-1);
            }
            else
            {
                FetchCount.IncrementSdTown();

                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No tower note */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                try
                {
                    /*	Fill in the fields */
                    suTown = new SuTown();

                    Ssutil.DbGetString(hStmt, 1, "call1", out suTown.call1, Constant.CALLSIGN_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "oper", out suTown.oper, Constant.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 3, "twcode", out suTown.twcode, Constant.SUTOWN_TWCODE_SZ, out sIsNull);
                    Ssutil.DbGetFloat(hStmt, 4, "twht", out suTown.twht, out sIsNull);
                    Ssutil.DbGetByte(hStmt, 5, "atwrno", out suTown.atwrno, out sIsNull);
                    Ssutil.DbGetString(hStmt, 6, "twli", out suTown.twli, Constant.SUTOWN_TWLI_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 7, "twpa", out suTown.twpa, Constant.SUTOWN_TWPA_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 8, "nott", out suTown.nott, Constant.SUTOWN_NOTT_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 9, "tpoint", out suTown.tpoint, Constant.SUTOWN_TPOINT_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 10, "adate", out suTown.adate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 11, "sdate", out suTown.sdate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 12, "mdate", out suTown.mdate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 13, "mtime", out suTown.mtime, Constant.TIME_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    GenUtil.SetErr("\r\nsuGetTown04 -  could not retrieve field %s for Call %s, Tower %d",
                        e.Message, cCall1, nTowerNo.ToString());
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// Retrieves note information for a prescribed operator code and note number as a populated SuNote object.
        /// </summary>
        /// <param name="cOperCode"> - prescribed operator code.</param>
        /// <param name="cNoteNum"> - prescribed note number.</param>
        /// <param name="suNote"> - populated SuNote object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetNote(string cOperCode, string cNoteNum, out SuNote suNote)
        {
            // Satisfy 'out' requirement.
            suNote = null;

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select oper, nonum, note, mdate, mtime from main.sd_note where oper='{0}' and nonum='{1}'", cOperCode, cNoteNum);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return (-1);
            }
            else
            {
                FetchCount.IncrementSdNote();

                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No Note */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                try
                {
                    /*	Fill in the fields */
                    suNote = new SuNote();

                    Ssutil.DbGetString(hStmt, 1, "oper", out suNote.oper, Constant.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "nonum", out suNote.nonum, Constant.SUNOTE_NONUM_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 3, "note", out suNote.note, Constant.SUNOTE_NOTE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 4, "mdate", out suNote.mdate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 5, "mtime", out suNote.mtime, Constant.DATE_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    GenUtil.SetErr("\r\nsuGetNote04 -  could not retrieve field %s for Operator %s, note %s",
                        e.Message, cOperCode, cNoteNum);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// Retrieves operator information for a prescribed operator code as a populated SuOper object.
        /// </summary>
        /// <param name="cOperCode"> - prescribed operator code.</param>
        /// <param name="suOper"> - populated SuOper object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetOper(string cOperCode, out SuOper suOper)
        {
            // Satisfy 'out' requirement.
            suOper = null;

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlret = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlret = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string schema = Info.SpoofModeIsOff ? "main" : Info.GlobalSchema;

            cSQL = String.Format("select oper, nameop, cooper, mdbm, addr, city, prstat, zippc, dept, namep, phonep, faxnum, telecom, opnote, admin, email, mdate, mtime from {0}.sd_oper where oper='{1}'", schema, cOperCode);

            //...Log2.v("\n\nSuutils.SuGetOper(): cSQL = {0}", cSQL);

            sqlret = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlret))
            {
                Log2.e("\n\nSuutils.SuGetOper(): ERROR: A: ODBC.SQLExecDirect() failed for query:\n" + cSQL);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);

                return (-1);
            }
            else
            {
                FetchCount.IncrementSdOper();

                sqlret = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlret))
                {
                    /*	No Operator */
                    Log2.e("\n\nSuutils.SuGetOper(): ERROR: B: ODBC.SQLFetch() failed for query:\n" + cSQL);
                    Log2.e("\n{0}", Environment.StackTrace);

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                suOper = new SuOper();

                try
                {
                    /*	Fill in the fields */
                    Ssutil.DbGetString(hStmt, 1, "oper", out suOper.oper, Constant.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "nameop", out suOper.nameop, Constant.NAMEOP_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 3, "cooper", out suOper.cooper, Constant.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 4, "mdbm", out suOper.mdbm, Constant.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 5, "addr", out suOper.addr, Constant.ADDR_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 6, "city", out suOper.city, Constant.CITY_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 7, "prstat", out suOper.prstat, Constant.PRSTAT_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 8, "zippc", out suOper.zippc, Constant.ZIPPC_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 9, "dept", out suOper.dept, Constant.DEPT_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 10, "namep", out suOper.namep, Constant.NAMEP_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 11, "phonep", out suOper.phonep, Constant.PHONEP_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 12, "faxnum", out suOper.faxnum, Constant.FAXNUM_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 13, "telecom", out suOper.telecom, Constant.TELECOM_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 14, "opnote", out suOper.opnote, Constant.OPNOTE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 15, "admin", out suOper.admin, Constant.ADMIN_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 16, "email", out suOper.email, Constant.EMAIL_SIZE_, out sIsNull);
                    Ssutil.DbGetString(hStmt, 17, "mdate", out suOper.mdate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 18, "mtime", out suOper.mtime, Constant.TIME_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    Ssutil.DbGetDiagStmt(hStmt, "Field retrieval error");
                    GenUtil.SetErr("\r\nsuGetOper04 -  could not retrieve field %s for Operator %s",
                        e.Message, cOperCode);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// Retrieves equipment information for a prescribed equipment code as a populated SuEqpt object.
        /// </summary>
        /// <param name="cEqptCode"> - prescribed equipment code.</param>
        /// <param name="suEqpt"> - populated SuEqpt object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetEqpt(string cEqptCode, out SuEqpt suEqpt)
        {
            int nRet = 0;

            if ((nRet = EqptGetCache(cEqptCode, out suEqpt)) != 0)
            {
                string cSQL;
                SQLLEN sIsNull = 0;

                SQLRETURN sqlret = 0;
                SQLHANDLE hStmt;
                SQLHDBC hConn = Ssutil.NewConn();

                sqlret = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                string schema = Info.SpoofModeIsOff ? "main" : Info.GlobalSchema;

                cSQL = String.Format("select ecode, estab, exref, emanu, emodel, edesc, etype, etraf, emission, e1stif, e2ndif, thhold, ebndcde, mdate, mtime from {0}.sd_eqpt where ecode='{1}'", schema, cEqptCode);
                sqlret = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlret))
                {
                    Ssutil.DbGetDiagStmt(hStmt, "suGesuEqpt01 - Could not retrieve: " + cEqptCode);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -1;
                }
                else
                {
                    FetchCount.IncrementSdEqpt();

                    sqlret = ODBC.SQLFetch(hStmt);

                    if (!ODBC.IsOK(sqlret))
                    {
                        /*	No equipment */
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return Constant.NOMORERECS;
                    }

                    try
                    {
                        /*	Fill in the fields */
                        suEqpt = new SuEqpt();

                        Ssutil.DbGetString(hStmt, 1, "ecode", out suEqpt.ecode, Constant.ECODE_SZ, out sIsNull);
                        Ssutil.DbGetFloat(hStmt, 2, "estab", out suEqpt.estab, out sIsNull);
                        Ssutil.DbGetString(hStmt, 3, "exref", out suEqpt.exref, Constant.ECODE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 4, "emanu", out suEqpt.emanu, Constant.SU_EQPT_EMANU, out sIsNull);
                        Ssutil.DbGetString(hStmt, 5, "emodel", out suEqpt.emodel, Constant.SU_EQPT_EMODEL, out sIsNull);
                        Ssutil.DbGetString(hStmt, 6, "edesc", out suEqpt.edesc, Constant.SU_EQPT_EDESC, out sIsNull);
                        Ssutil.DbGetString(hStmt, 7, "etype", out suEqpt.etype, Constant.SU_EQPT_ETYPE, out sIsNull);
                        Ssutil.DbGetString(hStmt, 8, "etraf", out suEqpt.etraf, Constant.SU_EQPT_ETRAF, out sIsNull);
                        Ssutil.DbGetString(hStmt, 9, "emission", out suEqpt.emission, Constant.SU_EQPT_EMISSION, out sIsNull);
                        Ssutil.DbGetFloat(hStmt, 10, "e1stif", out suEqpt.e1stif, out sIsNull);
                        Ssutil.DbGetFloat(hStmt, 11, "e2stif", out suEqpt.e2ndif, out sIsNull);
                        Ssutil.DbGetFloat(hStmt, 12, "thhold", out suEqpt.thhold, out sIsNull);
                        Ssutil.DbGetString(hStmt, 13, "ebndcde", out suEqpt.ebndcde, Constant.BNDCDE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 14, "mdate", out suEqpt.mdate, Constant.DATE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 15, "mtime", out suEqpt.mtime, Constant.TIME_SZ, out sIsNull);
                    }
                    catch (Exception e)
                    {
                        GenUtil.SetErr("\r\nsuGesuEqpt04 -  could not retrieve field %s for Equipment %s",
                            e.Message, cEqptCode);
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return -4;
                    }
                }
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);

                EqptPutCache(suEqpt);
                nRet = 0;
            }

            return (nRet);
        }

        /// <summary>
        /// Retrieves fee code information for a prescribed fee code as a populated SuFeeCode object.
        /// </summary>
        /// <param name="cFeeCode"> - prescribed fee code.</param>
        /// <param name="suFeeCode"> - populated SuFeeCode object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetFeeCode(string cFeeCode, out SuFeeCode suFeeCode)
        {
            // Satisfy 'out' requirement.
            suFeeCode = null;

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select code, num_chans, fee from techdef.fee_codes where code='{0}'", cFeeCode);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSuutils.SuGetFeeCode(): ERROR: call to ODBC.SQLExecDirect() failed for query:\n" + cSQL);
                Ssutil.DbGetDiagStmt(hStmt, "suGetFeeCode01: Error getting fee code information for: " + cFeeCode);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return (-1);
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No Fee Code */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                try
                {
                    /*	Fill in the fields */
                    suFeeCode = new SuFeeCode();

                    Ssutil.DbGetString(hStmt, 1, "code", out suFeeCode.code, Constant.SU_FEE_CODE_CODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "num_chans", out suFeeCode.num_chans, Constant.SU_FEE_CODE_NUM_CHANS_SZ, out sIsNull);
                    Ssutil.DbGetFloat(hStmt, 3, "fee", out suFeeCode.fee, out sIsNull);
                }
                catch (Exception e)
                {
                    GenUtil.SetErr("\r\nsuGetFeeCode04 -  could not retrieve field %s for code %s",
                        e.Message, cFeeCode);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// Retrieves traffic/equipment xref information for a prescribed traffic and equipment codes as a populated SuTraf object.
        /// </summary>
        /// <param name="cTraf"> - prescribed traffic code.</param>
        /// <param name="cEqpt"> - prescribed equipment code.</param>
        /// <param name="suTraf"> - populated SuTraf object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetTraf(string cTraf, string cEqpt, out SuTraf suTraf)
        {
            // Satisfy 'out' requirement.
            suTraf = null;
            int nRet;

            mTrafCalls++;          /*  Count the number of calls for Antennas */
            if ((nRet = TrafGetCache(cTraf, cEqpt, out suTraf)) != 0)
            {
                /*	Did not find it in the cache, go to the database */
                string cSQL;
                SQLLEN sIsNull = 0;

                SQLRETURN sqlRet = 0;
                SQLHANDLE hStmt;
                SQLHDBC hConn = Ssutil.NewConn();

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                string schema = Info.SpoofModeIsOff ? "main" : Info.GlobalSchema;

                cSQL = String.Format("select trafcode, ecode, xreftrcde, xrefeqcde, trdesc, mdate, mtime from {0}.sd_traf where trafcode='{1}'", schema, cTraf);

                if (cEqpt != null)
                {
                    //	if the equipment is not null, search for that too.
                    cSQL += String.Format(" and ecode='{0}'", cEqpt);
                }
                else
                {
                    cSQL += String.Format(" order by trafcode, ecode ");
                }
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return (-1);
                }
                else
                {
                    FetchCount.IncrementSdTraf();

                    sqlRet = ODBC.SQLFetch(hStmt);

                    if (!ODBC.IsOK(sqlRet))
                    {
                        /*	No traffic */
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return 1;
                    }

                    try
                    {
                        /*	Fill in the fields */
                        suTraf = new SuTraf();

                        Ssutil.DbGetString(hStmt, 1, "trafcode", out suTraf.trafcode, Constant.TRAFCODE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 2, "ecode", out suTraf.ecode, Constant.ECODE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 3, "xreftrcde", out suTraf.xreftrcde, Constant.TRAFCODE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 4, "xrefeqcde", out suTraf.xrefeqcde, Constant.ECODE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 5, "trdesc", out suTraf.trdesc, Constant.SU_TRAF_TRDESC, out sIsNull);
                        Ssutil.DbGetString(hStmt, 6, "mdate", out suTraf.mdate, Constant.DATE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 7, "mtime", out suTraf.mtime, Constant.TIME_SZ, out sIsNull);
                    }
                    catch (Exception e)
                    {
                        GenUtil.SetErr("\r\nsuGetTraf04 -  could not retrieve field %s for Traffic %s, Equipment %s.",
                            e.Message, cTraf, cEqpt);
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return -4;
                    }
                    nRet = TrafPutCache(suTraf);
                    nRet = 0;
                }
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
            }

            return (nRet);
        }

        /// <summary>
        /// Retrieves route information for a prescribed operator and route number as a populated SuRoute object.
        /// </summary>
        /// <param name="cOper"> - prescribed operator code.</param>
        /// <param name="cRouteNo"> - prescribed route number.</param>
        /// <param name="suRoute"> - populated SuRoute object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetRout(string cOper, string cRouteNo, out SuRout suRoute)
        {
            // Satisfy 'out' requirement.
            suRoute = null;

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select rcomp, routnumb, rtprov, rtcall, rtname, mdate, mtime from main.sd_rout where rcomp='{0}' and routnumb='{1}' ", cOper, cRouteNo);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return (-1);
            }
            else
            {
                FetchCount.IncrementSdRout();

                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No Route */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                try
                {
                    /*	Fill in the fields */
                    suRoute = new SuRout();

                    Ssutil.DbGetString(hStmt, 1, "rcomp", out suRoute.rcomp, Constant.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "routnumb", out suRoute.routnumb, Constant.ROUTE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 3, "rtprov", out suRoute.rtprov, Constant.PROV_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 4, "rtcall", out suRoute.rtcall, Constant.CALLSIGN_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 5, "rtname", out suRoute.rtname, Constant.RTNAME_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 6, "mdate", out suRoute.mdate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 7, "mtime", out suRoute.mtime, Constant.TIME_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    GenUtil.SetErr("\r\nsuGetRout04 -  could not retrieve field %s for Oper %s, Route %s",
                        e.Message, cOper, cRouteNo);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// Retrieves plam information for a prescribed plan code and 
        /// band code as a populated SuPlan object. If the caller wants 
        /// the list of plan frequencies they are provided as an array of 
        /// populated SuPlnd objects.
        /// </summary>
        /// <param name="cPlan"> - prescribed plan code.</param>
        /// <param name="cBand"> - prescribed band code.</param>
        /// <param name="pPlan"> - populated SuPlan object.</param>
        /// <param name="getListPlanReqs"> - true to request plan frequency information.</param>
        /// <param name="ppPlnd"> - array of populated SuPlnd objects.</param>
        /// <param name="nNum"> - the number of SuPlnd objects populated.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuGetPlan(string cPlan, string cBand, out SuPlan pPlan, bool getListPlanReqs, out SuPlnd[] ppPlnd, out int nNum)
        {
            //...Log2.v("\n\nSuutils.SuGetPlan(): Entry");

            //Satisfy 'out' requirements.
            pPlan = new SuPlan();
            ppPlnd = null;
            nNum = 0;

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select sband, splan, srsp, srspiss, conform, uscan, mdate, mtime from main.sd_plan where splan='{0}' and sband='{1}'",
                                    cPlan, cBand);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                //...Log2.v("\r\nSuutils.SuGetPlan(): returned -1");
                return (-1);
            }
            else
            {
                FetchCount.IncrementSdPlan();

                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No plan Code */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    //...Log2.v("\r\nSuutils.SuGetPlan(): returned 1");
                    return 1;
                }

                //...Log2.v("\r\nSuutils.SuGetPlan(): get pPlan: try");
                try
                {
                    /*	Fill in the fields */
                    Ssutil.DbGetString(hStmt, 1, "sband", out pPlan.sband, Constant.BNDCDE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "splan", out pPlan.splan, Constant.PLAN_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 3, "srsp", out pPlan.srsp, Constant.SRSP_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 4, "srspiss", out pPlan.srspiss, Constant.SRSPISS_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 5, "conform", out pPlan.conform, Constant.CONFORM_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 6, "uscan", out pPlan.uscan, Constant.USCAN_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 7, "mdate", out pPlan.mdate, Constant.DATE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 8, "mtime", out pPlan.mtime, Constant.TIME_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    GenUtil.SetErr("\r\nsuGetPlan04 -  could not retrieve field %s for plan %s, band %s",
                                        e.Message, cPlan, cBand);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    Log2.e("\r\nSuutils.SuGetPlan(): exception caught: returned -4");
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            //...Log2.v("\r\nSuutils.SuGetPlan(): get pPlan: succeeded");

            //	If the user wants the list of plan frequencies, load them in.
            if (getListPlanReqs)
            {
                //...Log2.v("\r\nSuutils.SuGetPlan(): get ppPlan[]: attempting");

                int nCount;

                cSQL = String.Format("splan='{0}' and sband='{1}' ", cPlan, cBand);

                nCount = Ssutil.DbCountRows("main.sd_plnd", cSQL);

                ppPlnd = new SuPlnd[nCount];

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                cSQL = String.Format("select sband, splan, spno, set1, s1chid, set2, s2chid, set3, s3chid, set4, s4chid, mdate, mtime from main.sd_plnd where splan='{0}' and sband='{1}'",
                                        cPlan, cBand);

                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    Log2.e("\r\nSuutils.SuGetPlan(): ERROR: SQLExecDirect() failed: returned -5");
                    return (-5);
                }

                for (int nInd = 0; nInd < nCount; nInd++)
                {
                    FetchCount.IncrementSdPlnd();

                    sqlRet = ODBC.SQLFetch(hStmt);

                    if (ODBC.IsOK(sqlRet))
                    {
                        ppPlnd[nInd] = new SuPlnd();
                        try
                        {
                            Ssutil.DbGetString(hStmt, 1, "sband", out ppPlnd[nInd].sband, Constant.BNDCDE_SZ, out sIsNull);
                            Ssutil.DbGetString(hStmt, 2, "splan", out ppPlnd[nInd].splan, Constant.PLAN_SZ, out sIsNull);
                            Ssutil.DbGetShort(hStmt, 3, "spno", out ppPlnd[nInd].spno, out sIsNull);
                            Ssutil.DbGetDouble(hStmt, 4, "set1", out ppPlnd[nInd].set1, out sIsNull);
                            Ssutil.DbGetString(hStmt, 5, "s1chid", out ppPlnd[nInd].s1chid, Constant.CHID_SZ, out sIsNull);
                            Ssutil.DbGetDouble(hStmt, 6, "set2", out ppPlnd[nInd].set2, out sIsNull);
                            Ssutil.DbGetString(hStmt, 7, "s2chid", out ppPlnd[nInd].s2chid, Constant.CHID_SZ, out sIsNull);
                            Ssutil.DbGetDouble(hStmt, 8, "set3", out ppPlnd[nInd].set3, out sIsNull);
                            Ssutil.DbGetString(hStmt, 9, "s3chid", out ppPlnd[nInd].s3chid, Constant.CHID_SZ, out sIsNull);
                            Ssutil.DbGetDouble(hStmt, 10, "set4", out ppPlnd[nInd].set4, out sIsNull);
                            Ssutil.DbGetString(hStmt, 11, "s4chid", out ppPlnd[nInd].s4chid, Constant.CHID_SZ, out sIsNull);
                            Ssutil.DbGetString(hStmt, 12, "mdate", out ppPlnd[nInd].mdate, Constant.DATE_SZ, out sIsNull);
                            Ssutil.DbGetString(hStmt, 13, "mtime", out ppPlnd[nInd].mtime, Constant.TIME_SZ, out sIsNull);
                        }

                        catch (Exception e)
                        {
                            GenUtil.SetErr("\r\nsuGetPlan06 -  could not retrieve field %s for plan %s, band %s, element %d",
                                                        e.Message, cPlan, cBand, nInd.ToString());
                            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                            Ssutil.DisConn(hConn);
                            Log2.e("\r\nSuutils.SuGetPlan(): ERROR: exception caught: returned -6");
                            return -6;
                        }
                    }
                }

                nNum = nCount;        //	Return the number found.

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
            }

            //...Log2.v("\n\nSuutils.SuGetPlan(): Exit: successful, returns 0");
            return 0;
        }

        /// <summary>
        /// Determine whether a prescribed band code has sub-bands, or not.
        /// </summary>
        /// <param name="bndcde"> - prescribed band code.</param>
        /// <returns>
        /// <para> - true if has sub-bands.</para>
        /// <para> - false if no sub-bands.</para>
        /// </returns>
        public static bool HasSubBands(string bndcde)
        {
            //...Log2.v("\n\nSuutils.HasSubBands(): Entry: bndcde = " + bndcde);

            bool IsRet = false;

            //	There is only one band like this for now, These should be added here as
            //	they are added to the system.
            if (bndcde.Equals("7B"))
            {
                IsRet = true;
            }

            //...Log2.v("\nSuutils.HasSubBands(): Exit: IsRet = " + IsRet + "\n");
            return IsRet;
        }

        /// <summary>
        /// Determines whether a frequency is cited in an array of SuPlnd objects, or not. 
        /// </summary>
        /// <param name="pPlnd"></param>
        /// <param name="nNum"></param>
        /// <param name="nSet"></param>
        /// <param name="dFreq"></param>
        /// <returns>
        /// <para> - non-negative value - the index of an array object that cites the frequency.</para>
        /// <para> - Constant.FAILURE -  frequency not found.</para>
        /// <para> - (-2) and below - method struck an error.</para>
        /// </returns>
        public static int SuPlndFindFreq(SuPlnd[] pPlnd, int nNum, int nSet, double dFreq)
        {
            //...Log2.v("\n\nSuutils.SuPlndFindFreq(): Entry");
            //...Log2.v("\r\nSuutils.SuPlndFindFreq(): pPlnd.Length = " + pPlnd.Length);
            //...Log2.v("\r\nSuutils.SuPlndFindFreq(): nNum = " + nNum);
            //...Log2.v("\r\nSuutils.SuPlndFindFreq(): nSet = " + nSet);
            //...Log2.v("\r\nSuutils.SuPlndFindFreq(): dFreq = " + dFreq);

            for (int i = 0; i < pPlnd.Length; i++)
            {
                //...Log2.v("\r\n------------- pPlnd[" + i + "] ----------------");
                //...Log2.v("\n\n" + pPlnd[i].ToString());
            }


            int nRet = -3;

            if (pPlnd == null)
            {
                nRet = -2;
            }
            else
            {
                switch (nSet)
                {
                    case 1:

                        for (int nInd = 0; nInd < nNum; nInd++)
                        {
                            if (Within(dFreq, pPlnd[nInd].set1, .001))
                            {
                                nRet = nInd;
                                break;
                                //return nInd;
                            }
                        }
                        break;

                    case 2:

                        for (int nInd = 0; nInd < nNum; nInd++)
                        {
                            if (Within(dFreq, pPlnd[nInd].set2, .001))
                            {
                                nRet = nInd;
                                break;
                                //return nInd;
                            }
                        }
                        break;

                    case 3:

                        for (int nInd = 0; nInd < nNum; nInd++)
                        {
                            if (Within(dFreq, pPlnd[nInd].set3, .001))
                            {
                                nRet = nInd;
                                break;
                                //return nInd;
                            }
                        }
                        break;

                    case 4:

                        for (int nInd = 0; nInd < nNum; nInd++)
                        {
                            if (Within(dFreq, pPlnd[nInd].set4, .001))
                            {
                                nRet = nInd;
                                break;
                                //return nInd;
                            }
                        }
                        break;

                    default:
                        break;
                } // switch()
            } // if-else

            //...Log2.v("\n\nSuutils.SuPlndFindFreq(): Exit: returned " + nRet);
            return nRet;
        }

        /// <summary>
        /// Determines whether a prescribed value lies in a prescribed interval or not.
        /// </summary>
        /// <param name="value"> - the value to be tested.</param>
        /// <param name="comparand"> - the reference value.</param>
        /// <param name="eps"> - the prescribed +/- proximity bound.</param>
        /// <returns> - (Math.Abs(value - comparand) <= eps)</returns>
        public static bool Within(double value, double comparand, double eps)
        {
            return (Math.Abs(value - comparand) <= eps);
        }

        /// <summary>
        /// This class maintains a cache of SuBand objects populated by a 'one time'
        /// retrieval all records in the DB table "main.sd_band". This method returns
        /// the number of SuBand objects in this cache.
        /// </summary>
        /// <returns> - number of SuBand objects in the cache.</returns>
        public static int SuNumberOfBands()
        {
            /*  check to see if the band is loaded, if not load */
            if (!mIsBandLoaded)
            {
                SuLoadBands();
            }

            return mBandCount;
        }

        /// <summary>
        /// Creates a cache of SuBand objects populated by retrieving all records 
        /// in the DB table "main.sd_band".
        /// </summary>
        public static void SuLoadBands()
        {
            short nInd;
            SuBand sctBand;
            SQLLEN sIsNull;

            string cSQL;
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();

            mBandCount = Ssutil.DbCountRows("main.sd_band", null);

            /*  Allocate the band cache */
            mpBandCache = new SuBand[mBandCount];

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select bndcde, bandbitpos, blo, bmidf, bhi, badj, mdate, mtime from main.sd_band ");

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            for (nInd = 0; nInd < mBandCount; nInd++)
            {
                sctBand = new SuBand();

                FetchCount.IncrementSdBand();

                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*  This method does not need errors */
                    GenUtil.SetErr("_suLoadBands: Error while loading Band Cache on band %d", nInd.ToString());
                    break;
                }
                else
                {
                    try
                    {
                        Ssutil.DbGetString(hStmt, 1, "bndcde", out sctBand.bndcde, Constant.BNDCDE_SZ, out sIsNull);
                        Ssutil.DbGetShort(hStmt, 2, "bandbitpos", out sctBand.bandbitpos, out sIsNull);
                        Ssutil.DbGetDouble(hStmt, 3, "blo", out sctBand.blo, out sIsNull);
                        Ssutil.DbGetDouble(hStmt, 4, "bmidf", out sctBand.bmidf, out sIsNull);
                        Ssutil.DbGetDouble(hStmt, 5, "bhi", out sctBand.bhi, out sIsNull);
                        Ssutil.DbGetString(hStmt, 6, "badj", out sctBand.badj, Constant.BADJ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 7, "mdate", out sctBand.mdate, Constant.DATE_SZ, out sIsNull);
                        Ssutil.DbGetString(hStmt, 8, "mtime", out sctBand.mtime, Constant.TIME_SZ, out sIsNull);
                    }
                    catch (Exception e)
                    {
                        GenUtil.SetErr("_suLoadBands02 - Error reading field: &s", e.Message);
                        break;
                    }
                }

                mpBandCache[nInd] = sctBand;
                //Trim the band code in the cache for faster retrieval.
                mpBandCache[nInd].bndcde.Trim();

                if (sctBand.bandbitpos > mnglbMaxBand)
                {
                    mnglbMaxBand = sctBand.bandbitpos;
                }
            }

            //	Handle the dynamic band adjacency support.  Set the band adjacency field
            //	according to the frequency separation.
            SuSetBandAdjFields();

            /*	Indicate that the bands are loaded.  We do this here because we will
            *		be calling suGetBand to retrieve the band adjacency information and
            *		we don't want it to try to load the bands again. */
            mIsBandLoaded = true;

            mnglbBitWords = (mnglbMaxBand / 32) + 1;

            /*	Now create the useable adjacency arrays.  Each band will store the
            *		band adjacency values in an 8 word array (to mimic the bandwds stored
            *		in the site table */

            for (nInd = 0; nInd < mBandCount; nInd++)
            {
                if (SuAdjBands(mpBandCache[nInd].bndcde, out mpBandBits) != 0)
                {
                    Console.WriteLine("*ERROR* Could not load band adjacency array. ");
                    return;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return;
        }

        /// <summary>
        /// Sets the band adjacency field; iterates through the bands
        /// and sets the band adjacency field if the the value of 
        /// mglbFreqSepForAdjacency is non-zero.
        /// </summary>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int SuSetBandAdjFields()
        {
            int nRet = 0;
            int nInd;
            int nInd1;
            double dMinFreq;
            double dMaxFreq;
            SuBand pThisBand;
            SuBand pThatBand;

            if (mglbFreqSepForAdjacencyKHz > 0)
            {
                //	The global frequency separation has been set.
                for (nInd = 0; nInd < mBandCount; nInd++)
                {
                    pThisBand = mpBandCache[nInd];

                    dMinFreq = pThisBand.blo - mglbFreqSepForAdjacencyKHz;
                    dMaxFreq = pThisBand.bhi + mglbFreqSepForAdjacencyKHz;

                    for (nInd1 = 0; nInd1 < mBandCount; nInd1++)
                    {
                        pThatBand = mpBandCache[nInd1];

                        if (nInd1 == nInd)
                        {
                            continue; //	don't do this one.
                        }
                        if ((pThatBand.bhi >= dMinFreq) &&
                            (pThatBand.blo <= dMaxFreq))
                        {
                            //	Add this band, if not present.
                            SuAddAdjacentBand(ref pThisBand.badj, pThatBand.bndcde);
                        }
                    }
                }
            }

            return nRet;
        }

        /// <summary>
        /// Appends a string representing a band to a list of semi-colon separated
        /// and trimmed bandcodes, first checking that it is not already in the list.
        /// </summary>
        /// <param name="badjlist"> - prescribed list of band codes.</param>
        /// <param name="bndcde"> - band code to be appended.</param>
        static void SuAddAdjacentBand(ref string badjlist, string bndcde)
        {
            string cBand;
            int nLen;
            bool IsFound = false;

            //	Trim and condition all the inputs.
            badjlist = badjlist.Trim();
            nLen = badjlist.Length;

            if (badjlist[nLen - 1] != ';')
            {
                // For scanning, we need the input list to end in a semicolon.  
                badjlist += ";";
            }

            cBand = bndcde.Trim();
            cBand += ";"; //	End the band code with a semicolon, which is what we scan for.
            nLen = cBand.Length;

            IsFound = badjlist.Contains(cBand);

            if (!IsFound)
            {
                badjlist += cBand;
            }

            return;
        }

        /// <summary>
        /// Sets a bit in bitmap at a position determined by each bandcode in the 
        /// adjacent band list, as well as for the band itself.
        /// </summary>
        /// <param name="bandCode"> - prescribed band code.</param>
        /// <param name="adjacentBands"> - BandBit object (a bitmap encoding band adjacency).</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SuAdjBands(string bandCode, out BandBits adjacentBands)
        {
            string[] tokens;
            char[] separators = new char[3] { ' ', ',', ';' };
            //int   							bcode = 0;
            SuBand ptBand;
            SuBand ptAdjBand;
            int nRet;
            int nAdjRet;

            // Zero all the bits by setting all the elements to zero.
            // This is the default instantiation.
            adjacentBands = new BandBits();

            if ((nRet = SuGetBand(bandCode, out ptBand)) == 0)
            {
                /*	Set the bit for this band.  This may be redundant */
                //	The bit array in the site table is 1 based, the operations are zero based, so decrement 1.
                GenUtil.UtSetBit(ref adjacentBands.bitArray, ptBand.bandbitpos - 1);

                /*	Loop through the band adjacency string and retrieve and set those bands */

                //token = strtok(ptBand.badj, " ,;");
                tokens = ptBand.badj.Split(separators);

                foreach (string token in tokens)
                {
                    /*	Each string broken out, will be a band code, we must retrieve this band
                    *		record to find the bit position. */
                    if ((nAdjRet = SuGetBand(token, out ptAdjBand)) == 0)
                    {
                        /*	Adjacent band retrieved, get the bit position and set it in the
                        *		adjacent band array. */

                        //...Log2.v("\nSuUtils.SuAdjBands(): ptAdjBand.bandbitpos = " + ptAdjBand.bandbitpos);

                        GenUtil.UtSetBit(ref adjacentBands.bitArray, ptAdjBand.bandbitpos - 1);
                    }
                }
            }

            //...Log2.v(String.Format("\nSuUtils.SuAdjBands(): Exit: adjacentBands = \n{0}\n{1}", BandBits.Ruler64Bits(), adjacentBands.First64BitsToString()));

            return nRet;
        }

        /// <summary>
        /// Creates a populated SdBand object that corresponds to a prescribed band bit.
        /// </summary>
        /// <param name="bndBitPos"> - prescribed bit position.</param>
        /// <param name="curBand"> - populated SdBand object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SdGetBandfromBit(int bndBitPos, out SdBand curBand)
        {
            // Satisfy 'out' requirement.
            curBand = null;

            int nRet;
            int nInd = 0;
            SuBand pBandEl = null; /*  Pointer to a band element */

            /*  check to see if the band is loaded, if not load */
            if (!mIsBandLoaded)
            {

                SuLoadBands();
            }

            for (nInd = 0; nInd < mBandCount; nInd++)
            {
                pBandEl = mpBandCache[nInd];
                if (bndBitPos == pBandEl.bandbitpos)
                {
                    // Found it.
                    break;
                }
            }

            if (nInd >= mBandCount)
            {
                /*  The band code was not found */
                nRet = 1;
            }
            else
            {
                /*  Band found, allocate space and copy the band back */
                SuBandToSdBand(out curBand, pBandEl);
                nRet = 0;
            }

            return nRet;
        }

        /// <summary>
        /// Performs a bitwise 'OR' operation on two prescribed BandBit objects' bitmaps.
        /// </summary>
        /// <param name="pBands1"> - first BandBits object; on exit conatins result of the bitwise 'OR' operation.</param>
        /// <param name="pBands2"> - second BandBits object.</param>
        public static void SuBandBitsOR(ref BandBits pBands1, BandBits pBands2)
        {
            int nInd;

            for (nInd = 0; nInd < BandBits.LENGTH; nInd++)
            {
                pBands1.bitArray[nInd] |= pBands2.bitArray[nInd];
            }

            return;
        }

        /// <summary>
        /// Creates a SdBand object populated with values from a prescribed SuBand object.
        /// </summary>
        /// <param name="psdBand"> - populated SdBand object.</param>
        /// <param name="psuBand"> - prescribed SuBand object.</param>
        /// <returns> - Always returns Constant.SUCCESS</returns>
        public static int SuBandToSdBand(out SdBand psdBand, SuBand psuBand)
        {
            psdBand = new SdBand();

            psdBand.bndcde = psuBand.bndcde;
            psdBand.bandbitpos = psuBand.bandbitpos;
            psdBand.blo = psuBand.blo;
            psdBand.bmidf = psuBand.bmidf;
            psdBand.bhi = psuBand.bhi;
            psdBand.badj = psuBand.badj;
            psdBand.mdate = psuBand.mdate;
            psdBand.mtime = psuBand.mtime;

            return 0;
        }

        /// <summary>
        /// Creats a SuAntStr object for a prescribed antenna code. This class
        /// maintains a cache of antenna information. This method first searches this
        /// cache for the prescribed antenna code. If the antenna code is not found
        /// in the cache then a full antenna record is read from the DB table 
        /// <b>main.sd_ante</b>; this information is then added to the antenna cache.
        /// </summary>
        /// <param name="cAcode"> - prescribed antenna code.</param>
        /// <param name="suAntStr"> - populated SuAntStr object.</param>
        /// <returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        /// </returns>
        public static int SeekAnt(string cAcode, out SuAntStr suAntStr)     /*  The antenna structure */
        {
            //&&Console.Error.Write("\nSeekAnt(): aCode = " + cAcode);
            //...Log2.v("\nSuutils.SeekAnt(): Entry: " + cAcode);

            // Satisfy 'out' requirement.
            suAntStr = null;

            string cSQL;
            SQLLEN sIsNull = 0;
            SQLRETURN sqlret = 0;
            SuAntd pDisc;
#if false
            mAnteCalls++;          /*  Count the number of calls for Antennas */

            ///*  First check the cache */
            if ((nCacheInd = FindAntInCache(cAcode)) >= 0)
            {
                /*  Found in cache, just copy to the structure */
                if (AntCacheCopyOut(nCacheInd, out suAntStr) < 0)
                {
                    /*	Error in the copy out. */
                    Log2.e("Suutils.SeekAnt(): ERROR: AntCacheCopyOut() failed.");
                    return (-2);
                }
            }
#endif
            // Check the cache.
            // First, make a search key.
            SuAntStr key = new SuAntStr();
            key.acAntCode = cAcode;

            // Now query the cache.
            suAntStr = mAnteCacheX.Get(key);

            if (suAntStr != null)
            {
                // We found a 'hit' in the antenna cache so we are done.
                //...Log2.v("\nSuutils.SeekAnt(): Exit: cache hit for: " + cAcode);
                //&&Console.Error.Write("\nSeekAnt(): Found in cache");
                //&&Console.Error.Write("\nSeekAnt(): successful exit");
                return Constant.SUCCESS;
            }

            //...Log2.v("\nSuutils.SeekAnt(): A");

            /*  Not found in the cache, go to the database */
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlret = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            string schema = Info.SpoofModeIsOff ? "main" : Info.GlobalSchema;

            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append(SuAnte.SubsetOfColumnsForSqlSelect);
            sb.Append(String.Format(" from {0}.sd_ante where acode = '{1}'", schema, cAcode));
            cSQL = sb.ToString();

            //&&Console.Error.Write("\n" + cSQL);

            sqlret = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlret))
            {
                // Something is wrong with the SQLExecDirect query.
                //&&Console.Error.Write("\nSeekAnt(): return A");
                Log2.e("Suutils.SeekAnt(): ERROR: A: SQLExecDirect() failed: \n" + cSQL);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //...Log2.v("\nSuutils.SeekAnt(): B");

            // If we reach here we have antenna to fetch from the MDB.
            FetchCount.IncrementSdAnte();

            sqlret = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlret))
            {
                //&&Console.Error.Write("\nSeekAnt(): return B");
                //int nRet;

                if (sqlret == ODBC.SQL_NO_DATA)
                {
                    // There is no matching antenna in the MDB.
                    // Return the current null value for suAntStr.
                    //...Log2.v("Suutils.SeekAnt(): A: SQLExecDirect() returned ODBC.SQL_NO_DATA");
                    //...Log2.v("\nSuutils.SeekAnt(): A: SQLFetch() returned SQL_NO_DATA.");
                }
                else
                {
                    // Something bad happened.
                    Log2.e("\nSuutils.SeekAnt(): A: SQLFetch() returned " + sqlret);
                }

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                //return nRet;
                return 2;
            }

            // Get the antenna column members from the fetched row.
            suAntStr = new SuAntStr(SuAntStr.Init.UNALLOCATED);
            suAntStr.acAnt = new SuAnte();
            suAntStr.acDscPtr = null;

            try
            {
                Ssutil.DbGetString(hStmt, 1, "acode", out suAntStr.acAnt.acode, Constant.ACODE_SZ, out sIsNull);

                Ssutil.DbGetInt(hStmt, 2, "axtype", out suAntStr.acAnt.axtype, out sIsNull);

                Ssutil.DbGetString(hStmt, 3, "axref", out suAntStr.acAnt.axref, Constant.ACODE_SZ, out sIsNull);

                Ssutil.DbGetFloat(hStmt, 4, "again", out suAntStr.acAnt.again, out sIsNull);

                Ssutil.DbGetFloat(hStmt, 5, "abw", out suAntStr.acAnt.abw, out sIsNull);

                Ssutil.DbGetShort(hStmt, 6, "arms", out suAntStr.acAnt.arms, out sIsNull);

                Ssutil.DbGetString(hStmt, 7, "aband", out suAntStr.acAnt.aband, Constant.ABAND_SZ, out sIsNull);

                Ssutil.DbGetString(hStmt, 8, "amanu", out suAntStr.acAnt.amanu, Constant.AMANU_SZ, out sIsNull);

                Ssutil.DbGetString(hStmt, 9, "apattern", out suAntStr.acAnt.apattern, Constant.ACODE_SZ, out sIsNull);

                Ssutil.DbGetString(hStmt, 10, "amodel", out suAntStr.acAnt.amodel, Constant.ANTE_MODEL_SZ, out sIsNull);

                Ssutil.DbGetShort(hStmt, 11, "anip", out suAntStr.acAnt.anip, out sIsNull);

                Ssutil.DbGetFloat(hStmt, 12, "ax0", out suAntStr.acAnt.ax0, out sIsNull);

                Ssutil.DbGetString(hStmt, 13, "adesc", out suAntStr.acAnt.adesc, Constant.ADESC_SZ, out sIsNull);

                Ssutil.DbGetString(hStmt, 14, "antype", out suAntStr.acAnt.antype, Constant.ANTYPE_SZ, out sIsNull);

                Ssutil.DbGetFloat(hStmt, 15, "aftbr", out suAntStr.acAnt.aftbr, out sIsNull);

                Ssutil.DbGetDouble(hStmt, 16, "lofreq", out suAntStr.acAnt.lofreq, out sIsNull);

                Ssutil.DbGetDouble(hStmt, 17, "hifreq", out suAntStr.acAnt.hifreq, out sIsNull);

                Ssutil.DbGetString(hStmt, 18, "bandcodes", out suAntStr.acAnt.bandcodes, Constant.BANDCODES_SZ, out sIsNull);

                Ssutil.DbGetString(hStmt, 19, "mdate", out suAntStr.acAnt.mdate, Constant.DATE_SZ, out sIsNull);

                Ssutil.DbGetString(hStmt, 20, "mtime", out suAntStr.acAnt.mtime, Constant.TIME_SZ, out sIsNull);
            }
            catch (Exception e)
            {
                Log2.e("\nSuutils.SeekAnt(): ERROR: C: ODBC 'Get' request failed: " + e.Message);
                GenUtil.SetErr("\r\nSD01 - SeekAnt could not retrieve field %s for antenna %s", e.Message, cAcode);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                //&&Console.Error.Write("\nSeekAnt(): return C");
                return Error.ODBC_GET_FAILED;
            }

            //&&Console.Error.Write("\nSeekAnt(): fetch-gets succeeded");
            //...Log2.v("\nSuutils.SeekAnt(): C");

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            suAntStr.acAntCode = suAntStr.acAnt.acode;

            /*	Now allocate the discrimination array.  Some antennas that are not
            *   crossreferenced (such as passives) will not have a dicrimination
            *   array, and it should be set to NULL */

            // Sanity check anip.
            if (suAntStr.acAnt.anip < 0)
            {
                Log2.e("\nSuutils.SeekAnt(): ERROR: F: suAntStr.acAnt.anip is invalid: " + suAntStr.acAnt.anip);
                Ssutil.DisConn(hConn);
                return Error.INVALID_DATA;
            }
            else if (suAntStr.acAnt.anip == 0)
            {
                //...Log2.v("\nSuutils.SeekAnt(): D");
                // Many records in the DB table main.sd_ante have a zero value for anip.
                // Instantiate suAntStr.acDscPtr as a non-null, empty array of SuAntd[];
                suAntStr.acDscPtr = new SuAntd[0];
                //...Log2.v("\nSuutils.SeekAnt(): call to Put() for " + suAntStr.acAntCode);
                // Put SuAntStr in the antennae cache and the we're done.
                mAnteCacheX.Put(suAntStr);
                return Constant.SUCCESS;
            }

            sqlret = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sb = new StringBuilder();
            sb.Append("select ");
            sb.Append(SuAntd.SubsetOfColumnsForSqlSelect);
            sb.Append(String.Format(" from main.sd_antd where acode = '{0}'", cAcode));
            sb.Append(" order by antang ");
            cSQL = sb.ToString();

            sqlret = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlret))
            {
                Log2.e("\nSuutils.SeekAnt(): ERROR: D: call to SQLExecDirect() failed: \n" + cSQL);
                /* Execute failure */
                Ssutil.DbGetDiagStmt(hStmt, "SD02 - Could not retrieve antenna curve.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                //&&Console.Error.Write("\nSeekAnt(): return D");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            // Instantiate a list<SuAntd> to accumulate the fetched data.
            List<SuAntd> suAntdList = new List<SuAntd>();

            // So we now expect one or more rows to be fetch-able; see how many we get.
            int nRows = 0;
            while (true)
            {
                //...Log2.v("\nSuutils.SeekAnt(): E");

                pDisc = new SuAntd();

                FetchCount.IncrementSdAntd();

                sqlret = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlret))
                {
                    if (sqlret == ODBC.SQL_NO_DATA)
                    {
                        // No more rows to fetch.
                        break;
                    }
                    else
                    {
                        Log2.e("\nSuutils.SeekAnt(): ERROR: E: call to SQLFetch() failed: \n");
                        return Error.ODBC_FETCH_FAILED;
                    }
                }

                // If we reach here we have a successful fetch; get the row values.
                try
                {
                    Ssutil.DbGetString(hStmt, 1, "acode", out pDisc.acode, Constant.ACODE_SZ, out sIsNull);

                    Ssutil.DbGetFloat(hStmt, 2, "antang", out pDisc.antang, out sIsNull);

                    Ssutil.DbGetFloat(hStmt, 3, "dcov", out pDisc.dcov, out sIsNull);

                    Ssutil.DbGetFloat(hStmt, 4, "dxpv", out pDisc.dxpv, out sIsNull);

                    Ssutil.DbGetFloat(hStmt, 5, "dcoh", out pDisc.dcoh, out sIsNull);

                    Ssutil.DbGetFloat(hStmt, 6, "dxph", out pDisc.dxph, out sIsNull);

                    Ssutil.DbGetFloat(hStmt, 7, "dtilt", out pDisc.dtilt, out sIsNull);

                    Ssutil.DbGetInt(hStmt, 8, "interpstat", out pDisc.interpstat, out sIsNull);

                    Ssutil.DbGetString(hStmt, 9, "mdate", out pDisc.mdate, Constant.DATE_SZ, out sIsNull);

                    Ssutil.DbGetString(hStmt, 10, "mtime", out pDisc.mtime, Constant.TIME_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    Log2.e("\nSuutils.SeekAnt(): ERROR: E: ODBC 'Get' attempt failed: " + e.Message);
                    return Error.ODBC_GET_FAILED;
                }

                // The 'Gets' were successful; increment the row count.
                nRows++;

                // Check for too many rows fetched.
                if (nRows > suAntStr.acAnt.anip)
                {
                    /*	This is a serious error.  more points than anip says. */
                    Log2.e("\nSuutils.SeekAnt(): ERROR: F: More points in antenna curve than anip count.");
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    GenUtil.SetErr("SD04 - More points in antenna curve than anip count.");
                    return Error.INVALID_DATA;
                }

                //...Log2.v("\nSuutils.SeekAnt(): F");
                // If we get here we have fetched a valid SuAntd object; accumulate it.
                suAntdList.Add(pDisc);

            } // End of while-loop.

            //...Log2.v("\nSuutils.SeekAnt(): G");

            // Check to see that we have fetched the correct number of records.
            if (suAntdList.Count != suAntStr.acAnt.anip)
            {
                /*	Error:  There are less points than anip indicates */
                Log2.e("\nSuutils.SeekAnt(): ERROR: G: Fewer points in antenna curve than anip indicates.");
                GenUtil.SetErr("SD05 - Fewer points in antenna curve than anip indicates.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.INVALID_DATA;
            }

            // Populate the  'out' parameter's acDscPtr[] array.
            suAntStr.acDscPtr = suAntdList.ToArray();

            // Free resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            /*  Add to the antenna cache */
#if false
            if (AntCacheCopyIn(suAntStr) < 0)
            {
                /*	Error allocating the cache.  */
                Log2.e("Suutils.SeekAnt(): ERROR: H: AntCacheCopyIn() failed.");
                return Error.CACHE_WRITE_FAILED;
            }
#endif
            // Put this SuAntStr into the antennae cache.

            //...Log2.v("\nSuutils.SeekAnt(): call to Put() for " + suAntStr.acAntCode);
            mAnteCacheX.Put(suAntStr);

            //...Log2.v("\nSuutils.SeekAnt(): Exit: returned 0");
            //&&Console.Error.Write("\nSeekAnt(): successful exit");
            return 0;
        }

        /// <summary>
        /// Retrieves information on the effectiveness of the antenna cache:
        /// the maximum size of the cache, the number of 'slots' currently used, 
        /// the total number of attempted cache reads and the total number of attempted cache reads.
        /// </summary>
        /// <param name="nCalls"> - total number of attempted cache reads</param>
        /// <param name="nHits"> - total number of successful cache 'hits'.</param>
        /// <param name="nSize"> - maximum size (number of 'slots') of the cache.</param>
        /// <param name="nUsed"> - number of slots currently in use.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int GetAntCacheInfo(out int nCalls,       /*  Calls to the routine */
                                            out int nHits,        /*  Number found in cache */
                                            out int nSize,   /*  Size of cache.  Max storable */
                                            out int nUsed)     /*  Number of these used */
        {
            //nCalls = mAnteCalls;
            //nHits = mAnteCacheHits;
            //nCacheSize = mAnteCache.Length;
            //nNumUsed = mAnteCacheNext;

            mAnteCacheX.GetStats(out nCalls, out nHits, out nSize, out nUsed);

            return 0;
        }

        /// <summary>
        /// Retrieves information on the effectiveness of the equipment cache:
        /// the maximum size of the cache, the number of 'slots' currently used, 
        /// the total number of attempted cache reads and the total number of attempted cache reads.
        /// </summary>
        /// <param name="nCalls"> - total number of attempted cache reads</param>
        /// <param name="nHits"> - total number of successful cache 'hits'.</param>
        /// <param name="nSize"> - maximum size (number of 'slots') of the cache.</param>
        /// <param name="nUsed"> - number of slots currently in use.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int GetEqptCacheInfo(out int nCalls,       /*  Calls to the routine */
                                            out int nHits,        /*  Number found in cache */
                                            out int nSize,   /*  Size of cache.  Max storable */
                                            out int nUsed)     /*  Number of these used */
        {
            mEquipCacheX.GetStats(out nCalls, out nHits, out nSize, out nUsed);

            return 0;
        }

        /// <summary>
        /// Retrieves information on the effectiveness of the CTX cache:
        /// the maximum size of the cache, the number of 'slots' currently used, 
        /// the total number of attempted cache reads and the total number of attempted cache reads.
        /// </summary>
        /// <param name="nCalls"> - total number of attempted cache reads</param>
        /// <param name="nHits"> - total number of successful cache 'hits'.</param>
        /// <param name="nSize"> - maximum size (number of 'slots') of the cache.</param>
        /// <param name="nUsed"> - number of slots currently in use.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int GetCTXCacheInfo(out int nCalls,       /*  Calls to the routine */
                                            out int nHits,        /*  Number found in cache */
                                            out int nSize,   /*  Size of cache.  Max storable */
                                            out int nUsed)     /*  Number of these used */
        {
            //nCalls = mCTXCalls;
            //nHits = mCTXCacheHits;
            //nCacheSize = mCTXCache.Length;
            //nNumUsed = mCTXCacheNext;

            mCtxCache.GetStats(out nCalls, out nHits, out nSize, out nUsed);

            return 0;
        }

        /// <summary>
        /// Retrieves information on the effectiveness of the CTX xRef cache:
        /// the maximum size of the cache, the number of 'slots' currently used, 
        /// the total number of attempted cache reads and the total number of attempted cache reads.
        /// </summary>
        /// <param name="nCalls"> - total number of attempted cache reads</param>
        /// <param name="nHits"> - total number of successful cache 'hits'.</param>
        /// <param name="nSize"> - maximum size (number of 'slots') of the cache.</param>
        /// <param name="nUsed"> - number of slots currently in use.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int GetCTXxCacheInfo(out int nCalls,       /*  Calls to the routine */
                                            out int nHits,        /*  Number found in cache */
                                            out int nSize,   /*  Size of cache.  Max storable */
                                            out int nUsed)     /*  Number of these used */
        {
            mCtxxCache.GetStats(out nCalls, out nHits, out nSize, out nUsed);

            return 0;
        }

        /// <summary>
        /// Retrieves a SuEqpt object from the cache of equipment information maintained by this class.
        /// </summary>
        /// <param name="cEqpt"> - the equipment code to search for.</param>
        /// <param name="suEqpt"> - populated SuEqpt object.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int EqptGetCache(string cEqpt, out SuEqpt suEqpt)
        {
            // Satisfy 'out' requirement.
            suEqpt = null;
            int nRet = Constant.FAILURE;

            // Search the Equipment cache.
            // First, make the search key.
            SuEqpt key = new SuEqpt();
            key.ecode = cEqpt;

            // Try to 'get' from the cache.
            suEqpt = mEquipCacheX.Get(key);

            if (suEqpt != null)
            {
                // We found the equipment in the cache.
                nRet = Constant.SUCCESS;
            }

            return (nRet);
        }

        /// <summary>
        /// Adds a SuEqpt object to the equipment cache maintained by this class.
        /// </summary>
        /// <param name="suEqpt"> - the SuEqpt object to be added.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        private static int EqptPutCache(SuEqpt suEqpt)
        {
            mEquipCacheX.Put(suEqpt);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Retrieves a SuTraf object from the cache of traffic information maintained by this class.
        /// </summary>
        /// <param name="cTraf"> - prescribed traffic code.</param>
        /// <param name="cEcode"> - prescribed equipment code.</param>
        /// <param name="suTraf"> - populated SuTraf object.</param>
        /// <returns></returns>
        public static int TrafGetCache(string cTraf, string cEcode, out SuTraf suTraf)
        {
            // Satisfy 'out' requirement.
            suTraf = null;
            int nInd;
            int nRet = -1;

            for (nInd = 0; nInd < mTrafCacheNext; nInd++)
            {
                if (cTraf.Equals(mTrafCache[nInd].trafcode) && (cEcode == null || cEcode.Equals(mTrafCache[nInd].ecode)))
                {
                    /*	Found it, update the marking */
                    mTrafCacheWhen[nInd] = mTrafCacheNow++;

                    suTraf = mTrafCache[nInd].DeepCopy();

                    mTrafCacheHits++;
                    nRet = 0;
                    break;
                }
            }
            return (nRet);
        }

        /// <summary>
        /// Adds a SuTraf object to the cache of traffic information maintained by this class.
        /// </summary>
        /// <param name="suTraf"> - SuTraf object to be added.</param>
        /// <returns> - always return Constant.SUCCESS</returns>
        private static int TrafPutCache(SuTraf suTraf)
        {
            int nInd;
            int nRet = -1;
            int nLRU = mTrafCacheWhen[0];
            int nLRUInd = 0;

            /*	Check for enough room in the cache */
            if (mTrafCacheNext >= mTrafCache.Length)
            {
                /*	Too many.  We will have to replace the Least Recently Used one */
                for (nInd = 0; nInd < mTrafCacheNext; nInd++)
                {
                    if (nLRU > mTrafCacheWhen[nInd])
                    {
                        nLRU = mTrafCacheWhen[nInd];
                        nLRUInd = nInd;
                    }
                }
            }
            else
            {
                /*	Just add it to the end */
                nLRUInd = mTrafCacheNext++;
            }

            mTrafCache[nLRUInd] = suTraf;
            mTrafCacheWhen[nLRUInd] = mTrafCacheNow++;

            nRet = 0;

            return (nRet);
        }

        /// <summary>
        /// Retrieve a plan from the database in database format. We use the normal 
        /// routine to get the database in update format, and use the conversion 
        /// routines to convert them to database format.  
        /// </summary>
        /// <param name="cPlan"></param>
        /// <param name="cBand"></param>
        /// <param name="sdPlan"></param>
        /// <param name="sdPlndArray"></param>
        /// <param name="nNum"></param>
        /// <returns></returns>
        public static int SdGetPlan(string cPlan, string cBand, out SdPlan sdPlan, out SdPlnd[] sdPlndArray, out int nNum)
        {
            SuPlan suPlan;
            SuPlnd[] suPlndArray;
            int nRet;
            int nInd;

            nRet = SuGetPlan(cPlan, cBand, out suPlan, true, out suPlndArray, out nNum);

            //	Now convert the returned structures
            SuPlanToSdPlan(suPlan, out sdPlan);

            sdPlndArray = new SdPlnd[nNum];

            for (nInd = 0; nInd < nNum; nInd++)
            {
                SuPlndToSdPlnd(suPlndArray[nInd], out sdPlndArray[nInd]);
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Convert an suPlan to an sdPlan.  
        /// </summary>
        /// <param name="suPlan"></param>
        /// <param name="sdPlan"></param>
        public static void SuPlanToSdPlan(SuPlan suPlan, out SdPlan sdPlan)
        {
            // Note: SuPlan = SdPlan + 'cmd' + 'recstat'

            sdPlan = new SdPlan();

            sdPlan.sband = suPlan.sband;
            sdPlan.splan = suPlan.splan;
            sdPlan.srsp = suPlan.srsp;
            sdPlan.srspiss = suPlan.srspiss;
            sdPlan.conform = suPlan.conform;
            sdPlan.uscan = suPlan.uscan;
            sdPlan.mdate = suPlan.mdate;
            sdPlan.mtime = suPlan.mtime;

            return;
        }
        /// <summary>
        /// Convert a plan frequency point from update format to database format.  
        /// </summary>
        /// <param name="suPlnd"></param>
        /// <param name="sdPlnd"></param>
        public static void SuPlndToSdPlnd(SuPlnd suPlnd, out SdPlnd sdPlnd)
        {
            // Note: SuPlnd = SdPlnd + 'cmd' + 'recstat'

            sdPlnd = new SdPlnd();

            sdPlnd.sband = suPlnd.sband;
            sdPlnd.splan = suPlnd.splan;
            sdPlnd.spno = suPlnd.spno;
            sdPlnd.set1 = suPlnd.set1;
            sdPlnd.s1chid = suPlnd.s1chid;
            sdPlnd.set2 = suPlnd.set2;
            sdPlnd.s2chid = suPlnd.s2chid;
            sdPlnd.set3 = suPlnd.set3;
            sdPlnd.s3chid = suPlnd.s3chid;
            sdPlnd.set4 = suPlnd.set4;
            sdPlnd.s4chid = suPlnd.s4chid;
            sdPlnd.mdate = suPlnd.mdate;
            sdPlnd.mtime = suPlnd.mtime;

            return;
        }

        /// <summary>
        /// Set the band adjacency limit value. If this is zero, then no changes are 
        /// made to the band table. If it is non-zero, then the code will go through 
        /// the table and calculate which bands could be adjacent to any specific band. 
        /// It will then update the band adjacency column in the table.  
        /// </summary>
        /// <param name="dFreqMHz"></param>
        public static void SuSetAdjFreq(double dFreqMHz)
        {
            if (dFreqMHz > Constant.MAX_FREQ_SEP_MHZ)
            {
                Console.Write("\n*WARNING* Requested Frequency Separation ({0:#.}) larger than {1:#.}%.0f maximum. Set to maximum.\n",
                                dFreqMHz, Constant.MAX_FREQ_SEP_MHZ);

                dFreqMHz = Constant.MAX_FREQ_SEP_MHZ;
            }

            mglbFreqSepForAdjacencyKHz = dFreqMHz * 1000.0;  // Input in MHz, used in KHz.

            ResetBands();       //	Make them reload.
        }

        /// <summary>
        /// Reset the band cache.  
        /// </summary>
        /// <param name=""></param>
        public static void ResetBands()
        {
            mIsBandLoaded = false;
        }

        /// <summary>
        /// This method returns an SdBand object populated with column values from the DB table
        /// "main.sd_band" for the record with a prescribed bndcde. The method returns 0 if the
        /// record was found; a non-zero return value indicates failure.  
        /// </summary>
        /// <param name="bndcde"> - prescribed bndcde.</param>
        /// <param name="pBand"> - populated SdBand object.</param>
        /// <returns></returns>
        public static int SdGetBand(string bndcde, out SdBand pBand)
        {
            // 'out' requirement.
            pBand = null;

            SuBand psuBand;
            int nRet;

            nRet = SuGetBand(bndcde, out psuBand);
            if (nRet == 0)
            {
                SuBandtoSdBand(out pBand, psuBand);
            }

            return nRet;
        }

        /// <summary>
        /// Get an SDBAND structure from an SUBAND.  
        /// </summary>
        /// <param name="psdBand"></param>
        /// <param name="psuBand"></param>
        /// <returns></returns>
        public static int SuBandtoSdBand(out SdBand psdBand, SuBand psuBand)
        {
            psdBand = new SdBand();

            psdBand.bndcde = psuBand.bndcde;
            psdBand.bandbitpos = psuBand.bandbitpos;
            psdBand.blo = psuBand.blo;
            psdBand.bmidf = psuBand.bmidf;
            psdBand.bhi = psuBand.bhi;
            psdBand.badj = psuBand.badj;
            psdBand.mdate = psuBand.mdate;
            psdBand.mtime = psuBand.mtime;

            return 0;
        }

        /// <summary>
        /// Interpolate the antenna discrimination curves. The antenna discrimination 
        /// curves are fed in in an array of suAntd_ structures, and a suAntd_ 
        /// structure is returned, filled in with the interpolated values for the 
        /// angle.  
        /// </summary>
        /// <param name="aADisc"></param>
        /// <param name="fAngle"></param>
        /// <param name="iPoints"></param>
        /// <param name="aOutDisc"></param>
        /// <returns></returns>
        public static int InterpADisc(SuAntd[] aADisc, float fAngle, int iPoints, out SuAntd aOutDisc)
        {
            //...Log2.v("\nSuutils.InterpADisc(): iPoints = " + iPoints);

            // 'out' requirement.
            aOutDisc = null;

            SuAntd pLastPoint;
            SuAntd pCurPoint;
            int iRetCode;
            int nInd;

            /* assert (aADisc != NULL); */
            if (aADisc == null)
            {
                // We can't perform an interpolation.
                Log2.e("\nSuutils.InterpADisc(): ERROR: aADisc == null");
                return -1;
            }

            if (iPoints <= 0)
            {
                Log2.e("\nSuutils.InterpADisc(): ERROR: iPoints = " + iPoints);
                GenUtil.SetError(5110, "interpADisc - Invalid arguments.");
                return -2;
            }

            if (-360.0 > fAngle || fAngle > +360.0)
            {
                Log2.e("\nSuutils.InterpADisc(): ERROR: - Invalid arguments: fAngle = " + fAngle);
                GenUtil.SetError(5110, "interpADisc - Invalid arguments.");
                return -2;
            }

            if (fAngle < 0)
            {
                fAngle += 360.0f;
            }

            /*	Determine if this off-axis angle will need to use pattern symmetry
                  and if so determine if this is a symmetrical pattern (i.e. up to 180)
                  or if this is an asymmetrical pattern to 360.0 */
            if (fAngle > 180.0)
            {
                /*	 We need to use symmetry if it is present, otherwise okay */
                if ((aADisc[iPoints - 1]).antang < 181.0)
                {
                    /*	It is symmetric.  Change the angle to 0 - 180 */
                    if (fAngle > 180.0)
                    {
                        fAngle = (float)360.0 - fAngle;
                    }
                }
            }

            aOutDisc = new SuAntd();

            /*	Go through the points to find the one that brackets the angle */
            int i = 0;
            pCurPoint = aADisc[i];
            if (fAngle < pCurPoint.antang)
            {
                /* The desired angle is before the first in the list. */
                aOutDisc = pCurPoint;
                iRetCode = 1;
            }
            else
            {
                /*	Go through the array finding the one we want */
                pLastPoint = pCurPoint;
                nInd = 0;
                while (++nInd <= iPoints && pCurPoint.antang < fAngle)
                {
                    pLastPoint = pCurPoint;
                    i++;
                    pCurPoint = aADisc[i];
                }

                /*	Past the end of the points, or the next point is too high */
                if (nInd > iPoints)
                {
                    /*	Past the end of the points, use the last */
                    aOutDisc = pLastPoint;
                    iRetCode = 2;
                }
                else
                {
                    /*	Found two points that bracket this angle */
                    aOutDisc.acode = pLastPoint.acode;


                    GenUtil.Interp(fAngle, pLastPoint.antang, pCurPoint.antang,
                        pLastPoint.dcov, pCurPoint.dcov, out aOutDisc.dcov);

                    GenUtil.Interp(fAngle, pLastPoint.antang, pCurPoint.antang,
                        pLastPoint.dxpv, pCurPoint.dxpv, out aOutDisc.dxpv);

                    GenUtil.Interp(fAngle, pLastPoint.antang, pCurPoint.antang,
                        pLastPoint.dcoh, pCurPoint.dcoh, out aOutDisc.dcoh);

                    GenUtil.Interp(fAngle, pLastPoint.antang, pCurPoint.antang,
                        pLastPoint.dxph, pCurPoint.dxph, out aOutDisc.dxph);


                    GenUtil.Interp(fAngle, pLastPoint.antang, pCurPoint.antang,
                        pLastPoint.dtilt, pCurPoint.dtilt, out aOutDisc.dtilt);

                    iRetCode = 0;
                }
            }

            aOutDisc.cmd = null;
            aOutDisc.antang = fAngle;
            aOutDisc.interpstat = 0;
            aOutDisc.mdate = null;
            aOutDisc.mtime = null;

            return (iRetCode);
        }

        /// <summary>
        /// CTX retrieval routines.  
        /// </summary>
        /// <param name="tfcr"></param>
        /// <param name="tfci"></param>
        /// <param name="rxeqp"></param>
        /// <param name="iLevel"> - 1 for just main data, 2 for curve too</param>
        /// <param name="CtxStr"></param>
        /// <returns></returns>
        public static int SuGetCtx(string tfcr, string tfci, string rxeqp, int
                            iLevel,       /* 1 for just main data, 2 for curve too */
                            out SuCtxStruct CtxStr)
        {
            // 'out' requirement.
            CtxStr = null;

            SQLLEN nIsNull;                 /* General Null column indicator  */

            /*	Declare all the columns */
            string ktfcr = "";
            string ktfci = "";
            string krxeqp = "";

            string cSQLBuff;

            int nRet;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            //...Log2.v("\nSuutils.suGetCtx(): Entry");

            //if (String.IsNullOrWhiteSpace(tfcr) ||
            //String.IsNullOrWhiteSpace(tfci) ||
            //String.IsNullOrWhiteSpace(rxeqp))
            if (tfcr == null || tfci == null || rxeqp == null)
            {
                Log2.e("\nSuutils.SuGetCtx(): invalid call parameters.");
                //...Log2.v("\nSuutils.suGetCtx(): A");
                GenUtil.SetError(5120, "suGetCtx - Invalid Arguments");
                Ssutil.DisConn(hConn);
                return Error.INVALID_ARGUMENTS;
            }

            /*	First go to the xref table and get the cross reference */
            nRet = SuGetCtxx(tfcr, tfci, rxeqp, out ktfcr, out ktfci, out krxeqp);

            if (nRet != 0)
            {
                //...Log2.v("\nSuutils.suGetCtx(): B");
                /*	No cross reference.  We report an error. NOTE that we could
                **	try and continue with the input units, but this means that
                **	something is amiss anyway */
                CtxStr = null;     /* Set the returned structure to null */
                nRet = Constant.FAILURE;          /*	No cross reference 					*/
            }
            else
            {
                //...Log2.v("\nSuutils.suGetCtx(): C");
                /*	We have the cross reference, now pull in the ctx info. */

                if ((nRet = CtxGetCache(tfcr, tfci, rxeqp, out CtxStr)) != 0)
                {
                    //...Log2.v("\nSuutils.suGetCtx(): E");
                    /*	Not in the cache.  Pull it in from the database */
                    ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                    cSQLBuff = String.Format("SELECT rqco, rqcull, rqwrst, ctxndp, ctxdesc, mdate, mtime from main.sd_ctx WHERE tfcr = '{0}' and tfci = '{1}' and rxeqp = '{2}'",
                        ktfcr.Trim(), ktfci.Trim(), krxeqp.Trim());

                    sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

                    if (!ODBC.IsOK(sqlRet))
                    {
                        Log2.e("\nSuutils.SuGetCtx(): ERROR: SQLEXECDIRECT() failed, sqlRet = " + sqlRet);
                        return Error.ODBC_EXECDIRECT_FAILED;
                    }

                    FetchCount.IncrementSdCtx();

                    sqlRet = ODBC.SQLFetch(hStmt);

                    if (!ODBC.IsOK(sqlRet))
                    {
                        //...Log2.v("\nSuutils.suGetCtx(): F");
                        /*	Some sort of error.  */
                        if (sqlRet == ODBC.SQL_NO_DATA)
                        {
                            nRet = Constant.NOMORERECS;
                        }
                        else
                        {
                            // Something bad happened.
                            Log2.e("\nSuutils.SuGetCtx(): ERROR: SQLFetch() failed, sqlRet = " + sqlRet);
                            nRet = Error.ODBC_FETCH_FAILED;
                        }
                    }
                    else
                    {
                        //...Log2.v("\nSuutils.suGetCtx(): G");

                        // Instantiate a CtxStr object and populate the fields we already have.
                        CtxStr = new SuCtxStruct();

                        CtxStr.CtxV = new SuCtx();

                        CtxStr.nDepth = 1;

                        CtxStr.CtxV.tfcr = ktfcr;
                        CtxStr.CtxV.tfci = ktfci;
                        CtxStr.CtxV.rxeqp = krxeqp;

                        nRet = 0;
                        Ssutil.DbStartGets();
                        try
                        {
                            Ssutil.DbGetFloat(hStmt, 0, "rqco", out CtxStr.CtxV.rqco, out nIsNull);
                            Ssutil.DbGetFloat(hStmt, 0, "rqcull", out CtxStr.CtxV.rqcull, out nIsNull);
                            Ssutil.DbGetFloat(hStmt, 0, "rqwrst", out CtxStr.CtxV.rqwrst, out nIsNull);
                            Ssutil.DbGetShort(hStmt, 0, "ctxndp", out CtxStr.CtxV.ctxndp, out nIsNull);
                            Ssutil.DbGetString(hStmt, 0, "ctxdesc", out CtxStr.CtxV.ctxdesc, SuCtx.CTXDESC_SZ, out nIsNull);
                            Ssutil.DbGetString(hStmt, 0, "mdate", out CtxStr.CtxV.mdate, SuCtx.MDATE_SZ, out nIsNull);
                            Ssutil.DbGetString(hStmt, 0, "mtime", out CtxStr.CtxV.mtime, SuCtx.MTIME_SZ, out nIsNull);

                            CtxStr.nDepth = 1;
                        }
                        catch (Exception e)
                        {
                            //...Log2.v("\nSuutils.suGetCtx(): H");
                            Log2.e("\nSuutils.SuGetCtx(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                            String str = String.Format("suGetCtx04: Error reading field {0} for {0}-{1}-{2}", e.Message, ktfcr, ktfci, krxeqp);
                            GenUtil.SetErr(str);
                            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                            Ssutil.DisConn(hConn);
                            return Error.ODBC_FETCH_FAILED;
                        }

                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);


                        /*******************************************************
                        *	At this point we can retreive the data points from
                        *	suCtxd.  Since we will be cachine the returned values, we
                        *	get the whole curve every time.
                        *******************************************************/
                        {
                            /*	Get the curve and return it. */
                            cSQLBuff = String.Format("tfcr='{0}' and tfci='{1}' and rxeqp='{2}'",
                                tfcr, tfci, rxeqp);

                            int numRecords = Ssutil.DbCountRows("main.sd_ctxd", cSQLBuff);

                            if (numRecords == 0)
                            {
                                CtxStr.pCtxD = null;
                            }
                            else
                            {
                                //...Log2.v("\nSuutils.suGetCtx(): I");
                                /*	Allocate the space for the array */
                                CtxStr.pCtxD = Arrays.CreateArrayUsingDefaultElementConstructor<SuCtxD>(numRecords);

                                {
                                    //...Log2.v("\nSuutils.suGetCtx(): J");

                                    cSQLBuff = String.Format("select fsep, rq, mdate, mtime from main.sd_ctxd where	tfcr = '{0}' and tfci = '{1}' and rxeqp = '{2}' order by fsep",
                                        tfcr, tfci, rxeqp);

                                    sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                                    sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

                                    if (!ODBC.IsOK(sqlRet))
                                    {
                                        Log2.e("\nSuutils.SuGetCtx(): ERROR: call to SQLExecDirect() failed: \n" + cSQLBuff);
                                        Ssutil.DbGetDiagStmt(hStmt, "suGetCtx01: Error reading sd_ctxd table for " + cSQLBuff);
                                        CtxStr.pCtxD = null;
                                        CtxStr.nDepth = 1;
                                    }
                                    else
                                    {
                                        int nFetched = 0;
                                        while (true)
                                        {
                                            FetchCount.IncrementSdCtxd();

                                            sqlRet = ODBC.SQLFetch(hStmt);
                                            if (!ODBC.IsOK(sqlRet))
                                            {
                                                if (sqlRet == ODBC.SQL_NO_DATA)
                                                {
                                                    /* Normal eof */
                                                    break;
                                                }
                                                else
                                                {
                                                    CtxStr.pCtxD = null;
                                                    return (-7);
                                                }
                                            }

                                            /*	Normal read */
                                            SuCtxD pCtxEl = CtxStr.pCtxD[nFetched];

                                            Ssutil.DbStartGets();
                                            try
                                            {
                                                Ssutil.DbGetDouble(hStmt, 0, "fsep", out pCtxEl.fsep, out nIsNull);
                                                Ssutil.DbGetFloat(hStmt, 0, "rq", out pCtxEl.rq, out nIsNull);
                                                Ssutil.DbGetString(hStmt, 0, "mdate", out pCtxEl.mdate, SuCtxD.MDATE_SZ, out nIsNull);
                                                Ssutil.DbGetString(hStmt, 0, "mtime", out pCtxEl.mtime, SuCtxD.MTIME_SZ, out nIsNull);
                                            }
                                            catch (Exception e)
                                            {
                                                //...Log2.v("\nSuutils.suGetCtx(): K");
                                                Log2.e("\nSuutils.SuGetCtx(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                                                String str = String.Format("suGetCtx08: Error reading field {0} for {0}-{1}-{2}", e.Message, ktfcr, ktfci, krxeqp);
                                                GenUtil.SetErr(str);
                                                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                                                Ssutil.DisConn(hConn);
                                                return Error.ODBC_GET_FAILED;
                                            }

                                            pCtxEl.tfcr = tfcr;
                                            pCtxEl.tfci = tfci;
                                            pCtxEl.rxeqp = rxeqp;

                                            nFetched++;
                                        }

                                        if (nFetched != numRecords)
                                        {
                                            Log2.e("\nSuutils.SuGetCtx(): ERROR: nFetched != numRecords");
                                        }
                                    }
                                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                                }
                            }
                            CtxStr.nDepth = 2;      /*	Got the array of points */
                        }

                        /*	Put it in the cache. */
                        CtxPutCache(CtxStr);

                        nRet = Constant.SUCCESS;
                    }
                }
            }

            Ssutil.DisConn(hConn);

            //...Log2.v("\nSuutils.suGetCtx(): Exit, final");

            return (nRet);
        }

        /// <summary>
        /// Get the CTX cross reference. There is only a get here, as the maintenance 
        /// of the table is done manually, using QBF or some such.  
        /// </summary>
        /// <param name="tfcr"></param>
        /// <param name="tfci"></param>
        /// <param name="rxeqp"></param>
        /// <param name="xref_tfcr"></param>
        /// <param name="xref_tfci"></param>
        /// <param name="xref_rxeqp"></param>
        /// <returns></returns>
        public static int SuGetCtxx(string tfcr, string tfci, string rxeqp,
                                    out string xref_tfcr, out string xref_tfci, out string xref_rxeqp)
        {
            // 'out' requirements.
            xref_tfcr = "";
            xref_tfci = "";
            xref_rxeqp = "";

            int nRet;

            string cSQL;
            SQLLEN nNullInd;

            Ctx_Xref tCtxx; // used for size and to return the keys.

            // Check whether the information requested is in the cache.
            if ((nRet = CtxxGetCache(tfcr, tfci, rxeqp, out xref_tfcr, out xref_tfci, out xref_rxeqp)) == Constant.SUCCESS)
            {
                // We found it in the cache.
                return Constant.SUCCESS;
            }

            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLRETURN sqlRet;

            /*	Not found in the cache.  Pull in from the database */
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select tfcr, tfci, rxeqp, xref_tfcr, xref_tfci, xref_rxeqp from tsip.ctx_xref where tfcr = '{0}' and tfci = '{1}' and rxeqp = '{2}'",
                tfcr, tfci, rxeqp);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSuutils.SuGetCtxx(): ERROR: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == Constant.NOMORERECS)
                {
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return Constant.NOMORERECS;
                }
                else
                {
                    Log2.e("\nSuutils.SuGetCtxx(): ERROR: call to SQLFetch() failed, sqlRet = " + sqlRet);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return Error.ODBC_FETCH_FAILED;
                }
            }

            nRet = 0;
            tCtxx = new Ctx_Xref();

            try
            {
                Ssutil.DbGetString(hStmt, 1, "tfcr", out tCtxx.tfcr, Ctx_Xref.TFCR_SZ, out nNullInd);

                Ssutil.DbGetString(hStmt, 2, "tfci", out tCtxx.tfci, Ctx_Xref.TFCI_SZ, out nNullInd);

                Ssutil.DbGetString(hStmt, 3, "rxeqp", out tCtxx.rxeqp, Ctx_Xref.RXEQP_SZ, out nNullInd);

                Ssutil.DbGetString(hStmt, 4, "xref_tfcr", out tCtxx.xref_tfcr, Ctx_Xref.XREF_TFCR_SZ, out nNullInd);

                Ssutil.DbGetString(hStmt, 5, "xref_tfci", out tCtxx.xref_tfci, Ctx_Xref.XREF_TFCI_SZ, out nNullInd);

                Ssutil.DbGetString(hStmt, 6, "xref_rxeqp", out tCtxx.xref_rxeqp, Ctx_Xref.XREF_RXEQP_SZ, out nNullInd);
            }
            catch (Exception e)
            {
                Log2.e("\nSuutils.SuGetCtxx(): ERROR: ODBC 'Get' failed: " + e.Message);
                GenUtil.SetErr("suGetCtxx01: Error reading field: " + e.Message);
                nRet = -1;
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_GET_FAILED;
            }

            xref_tfcr = tCtxx.xref_tfcr;
            xref_tfci = tCtxx.xref_tfci;
            xref_rxeqp = tCtxx.xref_rxeqp;

            CtxxPutCache(tfcr, tfci, rxeqp, xref_tfcr, xref_tfci, xref_rxeqp);

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// CTX xref retrieval and caching.  
        /// </summary>
        /// <param name="tfcr"></param>
        /// <param name="tfci"></param>
        /// <param name="rxeqp"></param>
        /// <param name="xref_tfcr"></param>
        /// <param name="xref_tfci"></param>
        /// <param name="xref_rxeqp"></param>
        /// <returns></returns>
        public static int CtxxGetCache(string tfcr, string tfci, string rxeqp,
                                        out string xref_tfcr, out string xref_tfci, out string xref_rxeqp)
        {
            // 'out' requirements.
            xref_tfcr = "";
            xref_tfci = "";
            xref_rxeqp = "";

            int nRet = Constant.FAILURE;

            // Instanciate a 'key' to search the cache.
            Ctx_Xref key = new Ctx_Xref();
            key.tfcr = tfcr;
            key.tfci = tfci;
            key.rxeqp = rxeqp;

            Ctx_Xref xRef = mCtxxCache.Get(key);  // cached object matching the key.
            if (xRef != null)
            {
                nRet = Constant.SUCCESS;

                xref_tfcr = xRef.tfcr;
                xref_tfci = xRef.tfci;
                xref_rxeqp = xRef.rxeqp;
            }

            return (nRet);
        }

        /// <summary>
        /// Put a ctx cross reference into the cache. It will already have been sought, 
        /// and not found.  
        /// </summary>
        /// <param name="tfcr"></param>
        /// <param name="tfci"></param>
        /// <param name="rxeqp"></param>
        /// <param name="xref_tfcr"></param>
        /// <param name="xref_tfci"></param>
        /// <param name="xref_rxeqp"></param>
        /// <returns></returns>
        public static int CtxxPutCache(string tfcr, string tfci, string rxeqp, string xref_tfcr, string xref_tfci, string xref_rxeqp)
        {
            Ctx_Xref t = new Ctx_Xref();

            t.tfcr = tfcr;
            t.tfci = tfci;
            t.rxeqp = rxeqp;
            t.xref_tfcr = xref_tfcr;
            t.xref_tfci = xref_tfci;
            t.xref_rxeqp = xref_rxeqp;

            mCtxxCache.Put(t);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// Retrieve from the cache if it is present.  
        /// </summary>
        /// <param name="ctfcr"></param>
        /// <param name="ctfci"></param>
        /// <param name="ceqpr"></param>
        /// <param name="tCtx"></param>
        /// <returns></returns>
        public static int CtxGetCache(string ctfcr, string ctfci, string ceqpr, out SuCtxStruct tCtx)
        {
            // 'out' requirement.
            tCtx = null;

            int nRet = Constant.FAILURE;

            // Create the search key.
            SuCtxStruct key = new SuCtxStruct();
            key.CtxV = new SuCtx();
            key.CtxV.tfcr = ctfcr;
            key.CtxV.tfci = ctfci;
            key.CtxV.rxeqp = ceqpr;

            tCtx = mCtxCache.Get(key);

            if (tCtx != null)
            {
                nRet = Constant.SUCCESS;
            }

            return (nRet);
        }

        /// <summary>
        /// Put the ctx curve in the cache. Note that the equipment will not have been 
        /// found in the cache for this to have been called.  
        /// </summary>
        /// <param name="CtxStr"></param>
        public static void CtxPutCache(SuCtxStruct CtxStr)
        {
            mCtxCache.Put(CtxStr);
        }


        /// <summary>
        /// Returns true if bandcode1 is adjacent or equal to bandCode2.  
        /// </summary>
        /// <param name="inbandCode1"></param>
        /// <param name="inbandCode2"></param>
        /// <returns></returns>
        public static bool SuIsBandAdjacent(string inbandCode1, string inbandCode2)
        {

            string[] tokens;
            char[] separators = new char[3] { ' ', ',', ';' };
            SuBand ptBand;
            int nRet;
            bool IsRet = false;

            string bandCode1 = inbandCode1.Trim();
            string bandCode2 = inbandCode2.Trim();

            if (bandCode1.Equals(bandCode2))
            {
                IsRet = true;
            }
            else
            {
                if ((nRet = SuGetBand(bandCode2, out ptBand)) == 0)
                {
                    /*	Loop through the band adjacency string and retrieve and set those bands */
                    tokens = ptBand.badj.Split(separators);

                    foreach (string token in tokens)
                    {
                        /*	Each string broken out, will be a band code, we must retrieve this band
                        *		record to find the bit position. */
                        if (bandCode1.Equals(token))
                        {
                            //	They are adjacent.
                            IsRet = true;
                            break;
                        }

                    }
                }
            }

            string str = String.Format("\nSsutils.SuIsBandAdjacent(): {0}  {1}  {2}", inbandCode1, inbandCode2, IsRet);
            //...Log2.v(str);
            return IsRet;
        }

        /// <summary>
        /// This method returns the operator_name for a prescribed opercode, as found
        /// in the DB table techdef.satellite_operators.
        /// </summary>
        /// <param name="cOperCode"></param>
        /// <param name="sctOper"></param>
        /// <returns> - returns 0 if successful, -1 if the opercode does not exist in the table.</returns>
        public static int SuGetSatOper(string cOperCode,        /*  Operator's Code */
                                        out SuSatOper sctOper)  /*  Returned operator structure */
        {
            // 'out' requirement.
            sctOper = new SuSatOper();

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlret = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            //nOperCalls++;          /*  Count the number of calls for Antennas */

            sqlret = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select opercode, operator_name, date_added from techdef.satellite_operators where opercode='{0}' ", cOperCode);

            sqlret = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlret))
            {
                Log2.e("\nSuutils.SuGetSatOper(): ERROR: call to SQLExecDirect() failed for query: " + cSQL);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);

                return (-1);
            }
            else
            {
                sqlret = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlret))
                {
                    if (sqlret == ODBC.SQL_NO_DATA)
                    {
                        /*	No Operator */
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return 1;
                    }
                    else
                    {
                        // Something bad just happened.
                        Log2.e("\nSuutils.SuGetSatOper(): ERROR: call to SQLFetch() returned: " + sqlret);
                        Application.Exit(Error.ODBC_FETCH_FAILED);
                    }
                }

                try
                {
                    /*	Fill in the fields */
                    Ssutil.DbGetString(hStmt, 1, "opercode", out sctOper.opercode, SuSatOper.OPERCODE_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "operator_name", out sctOper.operator_name, SuSatOper.OPERATOR_NAME_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 3, "date_added", out sctOper.date_added, SuSatOper.DATE_ADDED_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    Log2.e("\nSuutils.SuGetSatOper(): ERROR: ODBC Get() attempt failed: " + e.Message);
                    Ssutil.DbGetDiagStmt(hStmt, "Field retrieval error");
                    GenUtil.SetErr("\nsuSatGetOper04 -  could not retrieve field %s for Satellite Operator %s",
                        e.Message, cOperCode);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// This method returns a populated SuOpCode object that has the prescribed 
        /// ultrix user ID.
        /// </summary>
        /// <param name="cId"></param>
        /// <param name="pOpCode"></param>
        /// <returns></returns>
        public static int SuGetOpId(string cId, out SuOpCode pOpCode)
        {
            // 'out' requirement.
            pOpCode = new SuOpCode();

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("SELECT oper, ultrixid FROM main.mics_opcodes WHERE ultrixid='{0}'", cId);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return (-1);
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No Op Code */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                try
                {
                    /*	Fill in the fields */
                    Ssutil.DbGetString(hStmt, 1, "oper", out pOpCode.oper, SuOpCode.OPER_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "ultrixid", out pOpCode.ultrixid, SuOpCode.OPER_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    Log2.e("\nSuutils.SuGetOpId(): ERROR: Ssutil.DbGetString() failed: " + e.Message);
                    GenUtil.SetErr("\nsuGetOpCode04 -  could not retrieve field %s for code %s", e.Message, cId);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// This method returns a populated SuOpCode object that has the prescribed
        /// operator code.
        /// </summary>
        /// <param name="cOper"></param>
        /// <param name="pOpCode"></param>
        /// <returns></returns>
        public static int GetSuOpCode(string cOper, out SuOpCode pOpCode)
        {
            // 'out' requirement.
            pOpCode = new SuOpCode();

            string cSQL;
            SQLLEN sIsNull = 0;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("SELECT oper, ultrixid FROM adm.account_ids WHERE oper='{0}'", cOper);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSuutils.SuGetOpCode(): ERROR: call to ODBC.SQLExecDirect() failed for query: " + cSQL);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return (-1);
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    /*	No Op Code */
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }

                try
                {
                    /*	Fill in the fields */
                    Ssutil.DbGetString(hStmt, 1, "oper", out pOpCode.oper, SuOpCode.OPER_SZ, out sIsNull);
                    Ssutil.DbGetString(hStmt, 2, "ultrixid", out pOpCode.ultrixid, SuOpCode.ULTRIXID_SZ, out sIsNull);
                }
                catch (Exception e)
                {
                    Log2.e("\nSuutils.SuGetOpCode(): ERROR: call to ODBC.DbGetString() failed: " + e.Message);
                    GenUtil.SetErr("\nsuGetOpCode04 -  could not retrieve field %s for code %s", e.Message, cOper);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return -4;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return 0;
        }

        /// <summary>
        /// This method returns the Ultrix ID of 'this' user; it assumes that 
        /// Info.MicsUserName has already been set and uses it to lookup the corresponding 
        /// Ultrix ID from the DB table adm.account_details.
        /// The result is also written to Info.UltrixID
        /// </summary>
        /// <param name="ultrixID"></param>
        /// <returns></returns>
        public static int GetUltrixID(out string ultrixID)
        {
            // 'out' requirement.
            ultrixID = "";

            // First check whether we already have 'this' user's Ultrix ID in Info.
            // If so, return it.
            if (!String.IsNullOrWhiteSpace(Info.UltrixID))
            {
                ultrixID = Info.UltrixID;
                return Constant.SUCCESS;
            }


            // The following method call assumes that Info.MicsUserName
            // has already been set.
            int rc = Ssutil.GetSystemId(out ultrixID, Constant.ULTRIXID_SZ);

            if (rc != Constant.SUCCESS)
            {
                // Something went seriously wrong with the DB retrieval.
                Log2.e("\n\nSuutils.GetUltrixID(): ERROR: call to Ssutil.GetSystemId() failed, rc = " + rc);
                Application.Exit("ERROR: call to Ssutil.GetSystemId() failed", 667);
            }
            else if (String.IsNullOrWhiteSpace(ultrixID))
            {
                // Something strange just happened.
                Log2.e("\n\nSuutils.GetUltrixID(): ERROR: call to Ssutil.GetSystemId() returned null or empty string.");
                Application.Exit("ERROR: call to Ssutil.GetSystemId() failed", 668);
            }
            else
            {
                // It worked: we have the UltrixID for 'this' user's MICS username.
                Info.UltrixID = ultrixID;
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method returns a string providing a CSV list of band codes (bndcde) 
        /// that can be used in an SQL 'WHERE' clause to select only adjacent bands, 
        /// as prescribed in a BandBits object.
        /// </summary>
        /// <param name="tBands"></param>
        /// <param name="cSearch"></param>
        public static void FtBandBitsToList(BandBits tBands, out string cSearch)
        {
            // 'out' requirement.
            cSearch = "";

            int nPos;
            int nNumBands = SuNumberOfBands();
            string cComma = "";
            SdBand tBand;

            //...Log2.v("\nSuutils.FtBandBitsToList(): nNumBands = " + nNumBands);

            for (nPos = 0; nPos < nNumBands; nPos++)
            {
                if (GenUtil.UtTestBit(tBands.bitArray, nPos) == Enums.BIT.SET)
                {
                    //...Log2.v("\nSuutils.FtBandBitsToList(): bit set at position: " + nPos);

                    //	Get the band structure for this bit position.
                    if (SdGetBandfromBit((short)nPos, out tBand) == 0)
                    {
                        cSearch += cComma;
                        cSearch += "'";    //	Assume this is for SQL and surround with quotes.
                        cSearch += tBand.bndcde;
                        cSearch += "'";
                        cComma = ",";
                    }
                }
            }

            return;
        }

        /// <summary>
        /// This method converts a prescribed BandBits object into a List of strings
        /// corresponding to the bndcde for each set bit.
        /// </summary>
        /// <param name="bandBits"></param>
        /// <returns></returns>
        public static List<string> BandBitsToBndcdeList(BandBits bandBits)
        {
            List<string> bndcdeList = new List<string>();

            /*  check to see if the main.sd_band table has already been loaded - if not load */
            if (!mIsBandLoaded)
            {
                SuLoadBands();
            }

            for (int pos = 1; pos <= BandBits.MAXNUMBITS; pos++)
            {
                if (bandBits.CheckBit(pos - 1))
                {
                    bndcdeList.Add(BndcdeFromBitPos(pos));
                }
            }

            return bndcdeList;
        }

        /// <summary>
        /// This method converts a prescribed BandBits object into a string providing 
        /// a CSV-formatted list of bndcde for each set bit.
        /// </summary>
        /// <param name="bandBits"></param>
        /// <returns></returns>
        public static string BandBitsToBndcdeCSV(BandBits bandBits)
        {
            return Strings.ListOfStringsToCSV(BandBitsToBndcdeList(bandBits));
        }

        /// <summary>
        /// This method returns the bndcde corresponding to a prescribed bandbitpos by look-up
        /// from the MDB table main.sd_band; note that bandbitpos starts at one in the table.
        /// </summary>
        /// <param name="bandBitPos"></param>
        /// <returns></returns>
        public static string BndcdeFromBitPos(int bandBitPos)
        {
            string retval = "";

            /*  check to see if the main.sd_band table has already been loaded - if not load */
            if (!mIsBandLoaded)
            {
                SuLoadBands();
            }

            // Note that the mpBandCache[] array is ordered by bndcde not bitPos.
            for (int i = 0; i < mBandCount; i++)
            {
                if (mpBandCache[i].bandbitpos == bandBitPos)
                {
                    retval = mpBandCache[i].bndcde;
                    break;
                }
            }

            return retval;
        }



#if false
        public static int GetUserTablesView(int nFileType, string cFileName, USERTABLES* pView)
        {
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            int nRet;
            string cSQL;
            SQLLEN nNull;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, &hStmt);

            cSQL = String.Format("select operator, tabletype, file_name, micsid, project_code, validstat, create_date from web.user_tables where tabletype={0} and file_name='{1}'",
                nFileType, cFileName);
            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, TBD.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt,
                    "getUserTablesView: Error selecting data for %s.",
                    cFileName);
                nRet = -1;
            }

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == SQL_NO_DATA)
                {
                    nRet = 1;
                }
                else
                {
                    Ssutil.DbGetDiagStmt(hStmt,
                        "getUserTablesView: Error fetching data for %s.",
                        cFileName);
                    nRet = -1;
                }
            }
            else
            {
                memset(pView, 0, sizeof(*pView));
                try
                {
                    Ssutil.DbStartGets();
                    Ssutil.DbGetString(hStmt, 0, "operator", out pView.oper, sizeof(pView.oper), out nNull);
                    Ssutil.DbGetInt(hStmt, 0, "tabletype", &pView.tabletype, &nNull);
                    Ssutil.DbGetString(hStmt, 0, "file_name", out pView.file_name, sizeof(pView.file_name), out nNull);
                    Ssutil.DbGetString(hStmt, 0, "micsid", out pView.micsid, sizeof(pView.micsid), out nNull);
                    Ssutil.DbGetString(hStmt, 0, "project_code", out pView.project_code, sizeof(pView.project_code), out nNull);
                    Ssutil.DbGetString(hStmt, 0, "validstat", out pView.validstat, sizeof(pView.validstat), out nNull);
                    dbGetTimestamp(hStmt, 0, "create_date", &pView.create_date, &nNull);
                    nRet = 0;
                }
                catch (string cName)
                {
                    Ssutil.DbGetDiagStmt(hStmt, "getUserTablesView: Error Retrieving column %s for table %s",
                        cName, cFileName);
                    nRet = -2;
                }
            }

            DisConnStmt(hConn, hStmt);

            return nRet;
        }
#endif








    }
}



