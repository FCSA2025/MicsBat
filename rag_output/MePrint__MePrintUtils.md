# Documented File: MePrintUtils.cs
**Repository Path:** `MePrint\MePrintUtils.cs`
**Primary Layer:** `MePrint`
**Namespace:** `MePrint`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MePrint
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides utility methods that support the MePrint application.
    /// </summary>
    public class MePrintUtils
    {

        private static bool isInitialized = false;       // initialization flag 

        public const string COMMENT_LINE = "*---------------------------------------------------------------------------\r\n";

        private static MeSite meSite;             // storage for site record 
        private static SQLLEN[] meSiteNulls;
        private static MeAnte meAnte;             // Storage for antenna 
        private static SQLLEN[] meAnteNulls;
        private static MeAzim meAzim;             // azimuth record 
        private static SQLLEN[] meAzimNulls;
        private static MeChan meChan;             // storage for channel 
        private static SQLLEN[] meChanNulls;

        private static bool siteEof = false;     // end of site flag 
        private static bool anteEof = false;     // end of antenna flag 
        private static bool azimEof = false;     // end of azimuth flag 
        private static bool chanEof = false;     // end of channel flag 

        private static int meAnteHandle;          // storage for antenna handle 
        private static int meChanHandle;          // storage for channel handle 
        private static int meSiteHandle;          // storage for site handle 
        private static int meAzimHandle;          // storage for azimuth handle 

        /// <summary>
        /// This method provides much of the functionality of MePrint; it is 
        /// called repeatedly by the the Main() method until all of the data
        /// has been fetched from the database and written to Console.Out .
        /// </summary>
        /// <param name="siteLocation"></param>
        /// <param name="printLine"></param>
        /// <returns></returns>
        public static int MePrintFw(string siteLocation, ref string printLine)
        {
            string siteTableName;
            string anteTableName;
            string azimTableName;
            string chanTableName;

            int rc;                 /* return code */

            /* If argument is null then reset everything */
            if (printLine == null)
            {
                /* Initialize printFw */
                isInitialized = false;
                DynMeSite.MeCloseSite(meSiteHandle);
                DynMeAnte.MeCloseAnte(meAnteHandle);
                DynMeAzim.MeCloseAzim(meAzimHandle);
                DynMeChan.MeCloseChan(meChanHandle);
                return (Constant.SUCCESS);
            }

            /* Perform initialization. Read one record of each type */
            if (!isInitialized)
            {
                siteTableName = String.Format("{0}.me_site", Info.GlobalSchema);
                anteTableName = String.Format("{0}.me_ante", Info.GlobalSchema);
                azimTableName = String.Format("{0}.me_azim", Info.GlobalSchema);
                chanTableName = String.Format("{0}.me_chan", Info.GlobalSchema);

                //...Log2.v("\nMePrint.MePrintFw(): " + siteTableName);
                //...Log2.v("\nMePrint.MePrintFw(): " + anteTableName);
                //...Log2.v("\nMePrint.MePrintFw(): " + azimTableName);
                //...Log2.v("\nMePrint.MePrintFw(): " + chanTableName);

                string where = String.Format(" location = '{0}' ", siteLocation); ;

                if ((meSiteHandle = DynMeSite.MeSelectSite(where, "")) < 0)
                {
                    return (meSiteHandle);
                }
                if ((meAnteHandle = DynMeAnte.MeSelectAnte(where, "call1")) < 0)
                {
                    return (meAnteHandle);
                }
                if ((meAzimHandle = DynMeAzim.MeSelectAzim(where, "call1,azim")) < 0)
                {
                    return (meAzimHandle);
                }
                if ((meChanHandle = DynMeChan.MeSelectChan(where, "call1,chid")) < 0)
                {
                    return (meChanHandle);
                }

                siteEof = false;
                anteEof = false;
                azimEof = false;
                chanEof = false;

                if ((rc = DynMeSite.MeFetchSite(meSiteHandle, out meSite, out meSiteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of file encountered */
                    meSite.location = Constant.LAST_KEY;
                    siteEof = true;
                }

                //...Log2.v("\n\n" + meSite.ToStringWN(meSiteNulls));

                if ((rc = DynMeAnte.MeFetchAnte(meAnteHandle, out meAnte, out meAnteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of Antenna records */
                    meAnte.location = Constant.LAST_KEY;
                    meAnte.call1 = Constant.LAST_KEY;
                    anteEof = true;
                }

                //...Log2.v("\n\n" + meAnte.ToStringWN(meAnteNulls));

                if ((rc = DynMeAzim.MeFetchAzim(meAzimHandle, out meAzim, out meAzimNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of Azimuth records */
                    meAzim.location = Constant.LAST_KEY;
                    meAzim.call1 = Constant.LAST_KEY;
                    azimEof = true;
                }

                //...Log2.v("\n\n" + meAzim.ToStringWN(meAzimNulls));

                if ((rc = DynMeChan.MeFetchChan(meChanHandle, out meChan, out meChanNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of channel records encountered */
                    meChan.location = Constant.LAST_KEY;
                    meChan.call1 = Constant.LAST_KEY;
                    chanEof = true;
                }

                //...Log2.v("\n\n" + meChan.ToStringWN(meChanNulls));

                MePrintTitl(siteLocation, ref printLine);

                isInitialized = true;    /* Flag as initialized */

                return 0;

            } // if(isInitialized == false)

            // At this point initialization is complete.

            /* If all rec types processed */
            if (siteEof == true &&
                  anteEof == true &&
                    azimEof == true &&
                        chanEof == true)
            {
                return (Constant.NOMORERECS);
            }

            /* Determine which of the Site, Antenna, Azimuth and Channel records have
            * the lowest "key".
            */
            if (CmpKey(meChan.location, meChan.call1, meAnte.location, meAnte.call1) < 0 &&
                    CmpKey(meChan.location, meChan.call1, meAzim.location, meAzim.call1) < 0 &&
                    CmpPair(meChan.location, meSite.location) < 0)
            {
                /* Channel is the lowest key */
                MePrintChan(meChan, meChanNulls, ref printLine);
                if ((rc = DynMeChan.MeFetchChan(meChanHandle, out meChan, out meChanNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    meChan.location = Constant.LAST_KEY;
                    meChan.call1 = Constant.LAST_KEY;
                    chanEof = true;
                }
            }
            else if (CmpKey(meAzim.location, meAzim.call1, meAnte.location, meAnte.call1) < 0 &&
                       CmpPair(meAzim.location, meSite.location) < 0)
            {
                /* Azimuth is the lowest key */
                MePrintAzim(meAzim, meAzimNulls, ref printLine);
                if ((rc = DynMeAzim.MeFetchAzim(meAzimHandle, out meAzim, out meAzimNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    meAzim.location = Constant.LAST_KEY;
                    meAzim.call1 = Constant.LAST_KEY;
                    azimEof = true;
                }
            }
            else if (CmpPair(meAnte.location, meSite.location) < 0)
            {
                /* Antenna has lowest key */
                MePrintAnte(meAnte, meAnteNulls, ref printLine);
                if ((rc = DynMeAnte.MeFetchAnte(meAnteHandle, out meAnte, out meAnteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of Antenna records */
                    meAnte.location = Constant.LAST_KEY;
                    meAnte.call1 = Constant.LAST_KEY;
                    anteEof = true;
                }
            }
            else
            {
                /* Site has lowest key */
                MePrintSite(meSite, meSiteNulls, ref printLine);

                meSite.location = Constant.LAST_KEY;
                siteEof = true;
#if false
                if ((rc = DynMeSite.MeFetchSite(siteHandle, out site, out siteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }

                    site.location = Constant.LAST_KEY;
                    siteEof = true;
                }
#endif
            }
            return (Constant.SUCCESS);
        }


        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_titl</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="siteLocation"></param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int MePrintTitl(string siteLocation, ref string printLine)
        {
            FeTitl feTitl = new FeTitl();
            SQLLEN[] feTitlNulls = NullHelper.CreateArrayOfNullInd(FeTitl.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Set the FeTitl object's members.
            feTitl.validated = "N";
            feTitlNulls[FeTitl.VALIDATED] = Constant.DB_NOT_NULL;

            feTitl.namef = "";
            feTitlNulls[FeTitl.NAMEF] = Constant.DB_NOT_NULL;

            feTitl.source = "";
            feTitlNulls[FeTitl.SOURCE] = Constant.DB_NOT_NULL;

            feTitl.descr = "Location: " + siteLocation;
            feTitlNulls[FeTitl.DESCR] = Constant.DB_NOT_NULL;

            feTitl.mdate = "";
            feTitlNulls[FeTitl.MDATE] = Constant.DB_NOT_NULL;

            feTitl.mtime = "";
            feTitlNulls[FeTitl.MTIME] = Constant.DB_NOT_NULL;

            FePrint.FePrintUtils.PrintTitle(feTitl, feTitlNulls, ref printLine);

            return Constant.SUCCESS;
#if false
            string printItem;

            printLine += "TE,";

            if (titlNulls[MeTitl.VALIDATED] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.validated.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[MeTitl.NAMEF] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.namef.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[MeTitl.SOURCE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.source.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[MeTitl.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[MeTitl.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", title.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nTD,";
            if (titlNulls[MeTitl.DESCR] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", title.descr.Trim());
                printLine += printItem;
            }

            return 0;
#endif
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_chan</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="meChan"> - instance of an MeChan object.</param>
        /// <param name="meChanNulls"> - ODBC nullInds associated with chan.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        private static int MePrintChan(MeChan meChan, SQLLEN[] meChanNulls, ref string printLine)
        {
            FeChan feChan = new FeChan();
            SQLLEN[] feChanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            FeValCopy.FeCopyChan(ref feChan, meChan, ref feChanNulls, meChanNulls);

            // Handle unique FeChan members.
            feChan.cmd = "A";
            feChanNulls[FeChan.CMD] = Constant.DB_NOT_NULL;

            feChan.recstat = " ";
            feChanNulls[FeChan.RECSTAT] = Constant.DB_NOT_NULL;

            FePrint.FePrintUtils.FePrintChan(feChan, feChanNulls, ref printLine);

            return Constant.SUCCESS;
#if false
            string printItem;

            printLine = COMMENT_LINE;

            printLine += "CK,";
#if false
            if (chanNulls[MeChan.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
#endif
            if (chanNulls[MeChan.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.CHID] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.chid.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.NOTC] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.notc.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", chan.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nCT,";
            if (chanNulls[MeChan.FREQTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.freqtx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.POLTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.poltx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.MAXTXPOWER] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.maxtxpower);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.PWRTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.pwrtx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.P4KHZ] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.p4khz);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.EQPTTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.eqpttx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.TRAFTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.traftx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.STATTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.stattx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.SRVCTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.srvctx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.FEETX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", chan.feetx.Trim());
                printLine += printItem;
            }

            printLine += "\r\nCR,";
            if (chanNulls[MeChan.FREQRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.freqrx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.POLRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.polrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.PWRRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.pwrrx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.EQPTRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.eqptrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.TRAFRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.trafrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.I20] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.i20);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.IT01] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.it01);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.IP01] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.ip01);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.STATRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.statrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.SRVCRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.srvcrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[MeChan.FEERX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", chan.feerx.Trim());
                printLine += printItem;
            }

            return 0;
#endif
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_azim</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="meAzim"> - instance of an MeAzim object.</param>
        /// <param name="meAzimNulls"> - ODBC nullInds associated with azim.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        private static int MePrintAzim(MeAzim meAzim, SQLLEN[] meAzimNulls, ref string printLine)
        {
            FeAzim feAzim = new FeAzim();
            SQLLEN[] feAzimNulls = NullHelper.CreateArrayOfNullInd(FeAzim.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            FeValCopy.FeCopyAzim(ref feAzim, meAzim, ref feAzimNulls, meAzimNulls);

            // Handle unique FeAzim members.
            feAzim.cmd = "A";
            feAzimNulls[FeAzim.CMD] = Constant.DB_NOT_NULL;

            feAzim.recstat = " ";
            feAzimNulls[FeAzim.RECSTAT] = Constant.DB_NOT_NULL;

            feAzim.deleteall = "A";
            feAzimNulls[FeAzim.DELETEALL] = Constant.DB_NOT_NULL;

            FePrint.FePrintUtils.PrintAzim(feAzim, feAzimNulls, ref printLine);

            return Constant.SUCCESS;
#if false
            string printItem = "";

            if (isFirstAzim == true)
            {
                printLine = COMMENT_LINE;

                printLine += "ZK,";
                isFirstAzim = false;
            }
            else
            {
                printLine = "ZK,";
            }
#if false
            if (azimNulls[MeAzim.DELETEALL] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.deleteall.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }

            if (azimNulls[MeAzim.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }

            if (azimNulls[MeAzim.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
#endif
            if (azimNulls[MeAzim.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.AZIM] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.azim);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.ELEV] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.elev);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.DIST] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.dist);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.LOSS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.loss);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[MeAzim.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", azim.mtime.Trim());
                printLine += printItem;
            }

            return 0;
#endif
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_ante</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="mtAnte"> - instance of an MeAnte object.</param>
        /// <param name="mtAnteNulls"> - ODBC nullInds associated with ante.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        private static int MePrintAnte(MeAnte mtAnte, SQLLEN[] mtAnteNulls, ref string printLine)
        {
            FeAnte feAnte = new FeAnte();
            SQLLEN[] feAnteNulls = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            FeValCopy.FeCopyAnte(ref feAnte, meAnte, ref feAnteNulls, meAnteNulls);

            // Handle unique FeAnte members.
            feAnte.cmd = "A";
            feAnteNulls[FeAnte.CMD] = Constant.DB_NOT_NULL;

            feAnte.recstat = " ";
            feAnteNulls[FeAnte.RECSTAT] = Constant.DB_NOT_NULL;

            FePrint.FePrintUtils.FePrintAnte(feAnte, feAnteNulls, ref printLine);

            return Constant.SUCCESS;
#if false
            string printItem;

            isFirstAzim = true;

            printLine = COMMENT_LINE;

            printLine += "AK,";
#if false
            if (anteNulls[MeAnte.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
#endif
            if (anteNulls[MeAnte.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.LICENCE] != Constant.DB_NULL)
            {
                if (!ante.licence.Trim().Equals("\0"))
                {
                    printItem = String.Format("{0},", ante.licence.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += " ,";
                }
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.STATA] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.stata.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.NOTA] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.nota.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.ANTREF] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.antref);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ante.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nAT,";
            if (anteNulls[MeAnte.TXBAND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.txband.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.ACODETX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.acodetx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.AFSLT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.afslt);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.TXHGMAX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.txhgmax);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.TXTRO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.txtro);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.TXPRE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.txpre);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.AHT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.aht);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.AZ] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.az);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.EL] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2}", ante.el);
                printLine += printItem;
            }

            printLine += "\r\nAR,";
            if (anteNulls[MeAnte.RXBAND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.rxband.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.ACODERX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.acoderx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.AFSLR] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.afslr);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.RXHGMAX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.rxhgmax);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.RXTRO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.rxtro);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.RXPRE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.rxpre);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.G_T] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.g_t);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.LNAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1}", ante.lnat);
                printLine += printItem;
            }

            printLine += "\r\nAS,";
            if (anteNulls[MeAnte.SATNAME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.satname.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.OP2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.op2.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.ORBIT] != Constant.DB_NULL)
            {
                if (!ante.orbit.Trim().Equals("\0"))
                {
                    printItem = String.Format("{0},", ante.orbit.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += " ,";
                }
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.SATLONG] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2}", ante.satlong);
                printLine += printItem;
            }
            if (anteNulls[MeAnte.SATLONGS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0,1},", ante.satlongs);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.SARC1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.sarc1);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[MeAnte.SARC2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2}", ante.sarc2);
                printLine += printItem;
            }

            return 0;
#endif
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_site</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="meSite"> - instance of an MeSite object.</param>
        /// <param name="meSiteNulls"> - ODBC nullInds associated with site.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        private static int MePrintSite(MeSite meSite, SQLLEN[] meSiteNulls, ref string printLine)
        {
            FeSite feSite = new FeSite();
            SQLLEN[] feSiteNulls = NullHelper.CreateArrayOfNullInd(FeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            FeValCopy.FeCopySite(ref feSite, meSite, ref feSiteNulls, meSiteNulls);

            // Handle unique FeSite members.
            feSite.cmd = "A";
            feSiteNulls[FeSite.CMD] = Constant.DB_NOT_NULL;

            feSite.recstat = " ";
            feSiteNulls[FeSite.RECSTAT] = Constant.DB_NOT_NULL;

            FePrint.FePrintUtils.FePrintSite(feSite, feSiteNulls, ref printLine);

            return Constant.SUCCESS;
#if false
            string strLongit;
            string strLatit;
            string strLongitOrient;
            string strLatitOrient;
            string printItem;

            printLine = COMMENT_LINE;

            printLine += "SK,";
#if false
            if (siteNulls[MeSite.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
#endif
            if (siteNulls[MeSite.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.NAME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.name.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.PROV] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.prov.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.OPER] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.oper.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.OPRTYP] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.oprtyp.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.mtime.Trim());
                printLine += printItem;
            }

            if (siteNulls[MeSite.USERID] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", site.userid.Trim());
                printLine += printItem;
            }

            printLine += "\r\nSD,";
            if (siteNulls[MeSite.LATIT] != Constant.DB_NULL)
            {
                GenUtil.UtLatConvStr(site.latit, out strLatit, out strLatitOrient);
                printItem = String.Format("{0}{1},", strLatit, strLatitOrient);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.LONGIT] != Constant.DB_NULL)
            {
                GenUtil.UtLongConvStr(site.longit, out strLongit, out strLongitOrient);
                printItem = String.Format("{0}{1},", strLongit, strLongitOrient);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.GRND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", site.grnd);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.RADIO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.radio.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.RAIN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.rain);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.STATS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.stats.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.NOTS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.nots.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.REG] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.reg.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[MeSite.SDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", site.sdate.Trim());
                printLine += printItem;
            }

            return 0;
#endif
        }

        /// <summary>
        /// This method returns an integer that encodes the 'order' of a prescribed 
        /// {location, call1} key-pair w.r.t. a second prescribed key-pair; a negative
        /// return value signifies 'keyPair #1 is before keyPair #2', zero indicates that
        /// the two key-pairs have identical order and a positive return value indicates
        /// that 'keyPair #1 is after keyPair #2'.
        /// </summary>
        /// <param name="key1Loc"> - location value of 1st prescribed key-pair.</param>
        /// <param name="key1Call"> - call1 value of 1st prescribed key-pair.</param>
        /// <param name="key2Loc"> - location value of 2nd prescribed key-pair.</param>
        /// <param name="key2Call"> - call1 value of 2nd prescribed key-pair.</param>
        /// <returns></returns>
        private static int CmpKey(string key1Loc, string key1Call, string key2Loc, string key2Call)
        {
            int rc;
            if ((rc = CmpPair(key1Loc, key2Loc)) != 0)
            {
                return (rc);
            }

            // If we reach here the location is the same; now check the call sign.
            rc = CmpPair(key1Call, key2Call);
            return (rc);
        }

        /// <summary>
        /// This method return an integer that encodes the relative 'order' of two
        /// prescribed location strings; the order is determined by a character-by-character
        /// comparison using ASCII ordinal values with the exception that the <b>character '=' 
        /// is deemed to have ordinal less than that of 0 (zero)</b>.
        /// </summary>
        /// <param name="location1"></param>
        /// <param name="location2"></param>
        /// <returns></returns>
        private static int CmpPair(string location1, string location2)
        {
            // In the ASCII code table, the numbers 0-9 occur BEFORE the character
            // '=' and the letters A-Z and a-z are AFTER the '='.

            // We need to ensure that any leading '=' characters have a sort order
            // that is less than any alphanumeric character. We achieve this by replacing any
            // '=' with the '!' character which occurs before the character '0' (zero) in the
            // ASCII table order.
            location1 = location1.Replace("=", "!");
            location2 = location2.Replace("=", "!");

            int rc;
            if ((rc = Strings.StrCmp(location1, location2)) != 0)
            {
                return (rc);
            }

            return (rc);
        }






    }
}


```
