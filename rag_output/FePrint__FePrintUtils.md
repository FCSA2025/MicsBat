# Documented File: FePrintUtils.cs
**Repository Path:** `FePrint\FePrintUtils.cs`
**Primary Layer:** `FePrint`
**Namespace:** `FePrint`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FePrint
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides utility methods that support the FePrint application.
    /// </summary>
    public class FePrintUtils
    {

        private static bool isInitialized = false;       // initialization flag 
        private static bool isFirstAzim = true;

        public const string COMMENT_LINE = "*---------------------------------------------------------------------------\r\n";

        private static FeSite site;             // storage for site record 
        private static SQLLEN[] siteNulls;
        private static FeAnte ante;             // Storage for antenna 
        private static SQLLEN[] anteNulls;
        private static FeAzim azim;             // azimuth record 
        private static SQLLEN[] azimNulls;
        private static FeChan chan;             // storage for channel 
        private static SQLLEN[] chanNulls;
        private static FeCLoc cLoc;             // storage for change 
        private static SQLLEN[] cLocNulls;
        private static FeCCal cCal;             // storage for change 
        private static SQLLEN[] cCalNulls;
        private static FeTitl titl;             // storage for title 
        private static SQLLEN[] titlNulls;

        private static bool siteEof = false;     // end of site flag 
        private static bool anteEof = false;     // end of antenna flag 
        private static bool azimEof = false;     // end of azimuth flag 
        private static bool chanEof = false;     // end of channel flag 
        private static bool cLocEof = false;     // end of change flag 
        private static bool cCalEof = false;     // end of change flag 

        private static int anteHandle;          // storage for antenna handle 
        private static int chanHandle;          // storage for channel handle 
        private static int cLocHandle;          // storage for change handle 
        private static int cCalHandle;          // storage for change handle 
        private static int siteHandle;          // storage for site handle 
        private static int titlHandle;          // storage for title handle 
        private static int azimHandle;          // storage for azimuth handle 

        /// <summary>
        /// This method provides much of the functionality of FePrint; it is 
        /// called repeatedly by the the Main() method until all of the data
        /// has been fetched from the database and written to Console.Out .
        /// </summary>
        /// <param name="dispName"></param>
        /// <param name="printLine"></param>
        /// <returns></returns>
        public static int FePrintFw(string dispName, ref string printLine)
        {
            string titlTableName;
            string siteTableName;
            string anteTableName;
            string azimTableName;
            string chanTableName;
            string cLocTableName;
            string cCalTableName;
            int rc;                 /* return code */

            /* If argument is null then reset everything */
            if (printLine == null)
            {
                /* Initialize printFw */
                isInitialized = false;
                DynFeSite.FeCloseSite(siteHandle);
                DynFeAnte.FeCloseAnte(anteHandle);
                DynFeAzim.FeCloseAzim(azimHandle);
                DynFeChan.FeCloseChan(chanHandle);
                DynFeCLoc.FeCloseCLoc(cLocHandle);
                DynFeCCal.FeCloseCCal(cCalHandle);
                DynFeTitl.FeCloseTitl(titlHandle);
                return (Constant.SUCCESS);
            }

            /* Perform initialization. Read one record of each type */
            if (!isInitialized)
            {

                GenUtil.UtCvtName(Constant.FE_TITL, dispName, out titlTableName);
                GenUtil.UtCvtName(Constant.FE_SITE, dispName, out siteTableName);
                GenUtil.UtCvtName(Constant.FE_ANTE, dispName, out anteTableName);
                GenUtil.UtCvtName(Constant.FE_AZIM, dispName, out azimTableName);
                GenUtil.UtCvtName(Constant.FE_CHAN, dispName, out chanTableName);
                GenUtil.UtCvtName(Constant.FE_CLOC, dispName, out cLocTableName);
                GenUtil.UtCvtName(Constant.FE_CCAL, dispName, out cCalTableName);

                if ((titlHandle = DynFeTitl.FeSelectTitl(titlTableName, "", "")) < 0)
                {
                    return (titlHandle);
                }
                if ((siteHandle = DynFeSite.FeSelectSite(siteTableName, "", "location")) < 0)
                {
                    return (siteHandle);
                }
                if ((anteHandle = DynFeAnte.FeSelectAnte(anteTableName, "", "location,call1")) < 0)
                {
                    return (anteHandle);
                }
                if ((azimHandle = DynFeAzim.FeSelectAzim(azimTableName, "", "location,call1,azim")) < 0)
                {
                    return (azimHandle);
                }
                if ((chanHandle = DynFeChan.FeSelectChan(chanTableName, "", "location,call1,chid")) < 0)
                {
                    return (chanHandle);
                }
                if ((cLocHandle = DynFeCLoc.FeSelectCLoc(cLocTableName, "", "newlocation")) < 0)
                {
                    return (cLocHandle);
                }

                if ((cCalHandle = DynFeCCal.FeSelectCCal(cCalTableName, "", "newcallsign")) < 0)
                {
                    return (cCalHandle);
                }

                siteEof = false;
                anteEof = false;
                azimEof = false;
                chanEof = false;
                cCalEof = false;
                cLocEof = false;

                if ((rc = DynFeSite.FeFetchSite(siteHandle, out site, out siteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of file encountered */
                    site.location = Constant.LAST_KEY;
                    siteEof = true;
                }

                if ((rc = DynFeAnte.FeFetchAnte(anteHandle, out ante, out anteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of Antenna records */
                    ante.location = Constant.LAST_KEY;
                    ante.call1 = Constant.LAST_KEY;
                    anteEof = true;
                }

                if ((rc = DynFeAzim.FeFetchAzim(azimHandle, out azim, out azimNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of Azimuth records */
                    azim.location = Constant.LAST_KEY;
                    azim.call1 = Constant.LAST_KEY;
                    azimEof = true;
                }

                if ((rc = DynFeChan.FeFetchChan(chanHandle, out chan, out chanNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of channel records encountered */
                    chan.location = Constant.LAST_KEY;
                    chan.call1 = Constant.LAST_KEY;
                    chanEof = true;
                }

                isInitialized = true;    /* Flag as initialized */

                /* print one title record */
                if ((rc = DynFeTitl.FeFetchTitl(titlHandle, out titl, out titlNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                }
                else
                {
                    PrintTitle(titl, titlNulls, ref printLine);
                    return (Constant.SUCCESS);
                }

            }               /* end  if(init == false) */

            /* if all change of location recs
            haven't been processed
            */
            if (!cLocEof)
            {
                if ((rc = DynFeCLoc.FeFetchCLoc(cLocHandle, out cLoc, out cLocNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    cLocEof = true;
                }
                else
                {
                    PrintChangeLoc(cLoc, cLocNulls, ref printLine);
                    return (Constant.SUCCESS);
                }
            }


            /* if all change of callsign recs
            haven't been processed
            */
            if (!cCalEof)
            {
                if ((rc = DynFeCCal.FeFetchCCal(cCalHandle, out cCal, out cCalNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    cCalEof = true;
                }
                else
                {
                    PrintChangeCall(cCal, cCalNulls, ref printLine);
                    return (Constant.SUCCESS);
                }
            }

            /* If all rec types processed */
            if (siteEof == true &&
                  anteEof == true &&
                    azimEof == true &&
                        chanEof == true &&
                            cLocEof == true &&
                                cCalEof == true)
            {
                return (Constant.NOMORERECS);
            }

            /* Determine which of the Site, Antenna, Azimuth and Channel records have
            * the lowest "key".
            */
            if (CmpKey(chan.location, chan.call1, ante.location, ante.call1) < 0 &&
                    CmpKey(chan.location, chan.call1, azim.location, azim.call1) < 0 &&
                    CmpPair(chan.location, site.location) < 0)
            {
                /* Channel is the lowest key */
                FePrintChan(chan, chanNulls, ref printLine);
                if ((rc = DynFeChan.FeFetchChan(chanHandle, out chan, out chanNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    chan.location = Constant.LAST_KEY;
                    chan.call1 = Constant.LAST_KEY;
                    chanEof = true;
                }
            }
            else if (CmpKey(azim.location, azim.call1, ante.location, ante.call1) < 0 &&
                       CmpPair(azim.location, site.location) < 0)
            {
                /* Azimuth is the lowest key */
                PrintAzim(azim, azimNulls, ref printLine);
                if ((rc = DynFeAzim.FeFetchAzim(azimHandle, out azim, out azimNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    azim.location = Constant.LAST_KEY;
                    azim.call1 = Constant.LAST_KEY;
                    azimEof = true;
                }
            }
            else if (CmpPair(ante.location, site.location) < 0)
            {
                /* Antenna has lowest key */
                FePrintAnte(ante, anteNulls, ref printLine);
                if ((rc = DynFeAnte.FeFetchAnte(anteHandle, out ante, out anteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    /* End of Antenna records */
                    ante.location = Constant.LAST_KEY;
                    ante.call1 = Constant.LAST_KEY;
                    anteEof = true;
                }
            }
            else
            {
                /* Site has lowest key */
                FePrintSite(site, siteNulls, ref printLine);
                if ((rc = DynFeSite.FeFetchSite(siteHandle, out site, out siteNulls)) != Constant.SUCCESS)
                {
                    if (rc != Constant.NOMORERECS)
                    {
                        return (rc);
                    }
                    site.location = Constant.LAST_KEY;
                    siteEof = true;
                }
            }
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_titl</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="title"> - instance of an FeTitl object.</param>
        /// <param name="titlNulls"> - ODBC nullInds associated with title.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int PrintTitle(FeTitl title, SQLLEN[] titlNulls, ref string printLine)
        {
            string printItem;

            printLine += "TE,";

            if (titlNulls[FeTitl.VALIDATED] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.validated.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[FeTitl.NAMEF] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.namef.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[FeTitl.SOURCE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.source.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[FeTitl.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", title.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (titlNulls[FeTitl.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", title.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nTD,";
            if (titlNulls[FeTitl.DESCR] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", title.descr.Trim());
                printLine += printItem;
            }

            return 0;
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_cloc</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="cLoc"> - instance of an FeCLoc object.</param>
        /// <param name="cLocNulls"> - ODBC nullInds associated with cLoc.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int PrintChangeLoc(FeCLoc cLoc, SQLLEN[] cLocNulls, ref string printLine)
        {
            string printItem = "";

            printLine = "LK,";

            if (cLocNulls[FeCLoc.OLDLOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", cLoc.oldlocation.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (cLocNulls[FeCLoc.NEWLOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", cLoc.newlocation.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (cLocNulls[FeCLoc.NAME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", cLoc.name.Trim());
                printLine += printItem;
            }

            return 0;
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_ccal</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="cCal">- instance of an FeCCal object.</param>
        /// <param name="cCalNulls"> - ODBC nullInds associated with cCal.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        private static int PrintChangeCall(FeCCal cCal, SQLLEN[] cCalNulls, ref string printLine)
        {
            string printItem;

            printLine = "GK,";

            if (cCalNulls[FeCCal.OLDCALLSIGN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", cCal.oldcallsign.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (cCalNulls[FeCCal.NEWCALLSIGN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", cCal.newcallsign.Trim());
                printLine += printItem;
            }

            return 0;
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_chan</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="chan"> - instance of an FeChan object.</param>
        /// <param name="chanNulls"> - ODBC nullInds associated with chan.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int FePrintChan(FeChan chan, SQLLEN[] chanNulls, ref string printLine)
        {
            string printItem;

            printLine = COMMENT_LINE;

            printLine += "CK,";

            if (chanNulls[FeChan.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.CHID] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.chid.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.NOTC] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.notc.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", chan.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nCT,";
            if (chanNulls[FeChan.FREQTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.freqtx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.POLTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.poltx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.MAXTXPOWER] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.maxtxpower);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.PWRTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.pwrtx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.P4KHZ] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.p4khz);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.EQPTTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.eqpttx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.TRAFTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.traftx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.STATTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.stattx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.SRVCTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.srvctx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.FEETX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", chan.feetx.Trim());
                printLine += printItem;
            }

            printLine += "\r\nCR,";
            if (chanNulls[FeChan.FREQRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.freqrx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.POLRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.polrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.PWRRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", chan.pwrrx);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.EQPTRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.eqptrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.TRAFRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.trafrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.I20] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.i20);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.IT01] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.it01);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.IP01] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", chan.ip01);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.STATRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.statrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.SRVCRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", chan.srvcrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (chanNulls[FeChan.FEERX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", chan.feerx.Trim());
                printLine += printItem;
            }

            return 0;
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_azim</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="azim"> - instance of an FeAzim object.</param>
        /// <param name="azimNulls"> - ODBC nullInds associated with azim.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int PrintAzim(FeAzim azim, SQLLEN[] azimNulls, ref string printLine)
        {
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

            if (azimNulls[FeAzim.DELETEALL] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.deleteall.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.AZIM] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.azim);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.ELEV] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.elev);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.DIST] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.dist);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.LOSS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", azim.loss);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", azim.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (azimNulls[FeAzim.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", azim.mtime.Trim());
                printLine += printItem;
            }

            return 0;
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_ante</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="ante"> - instance of an FeAnte object.</param>
        /// <param name="anteNulls"> - ODBC nullInds associated with ante.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int FePrintAnte(FeAnte ante, SQLLEN[] anteNulls, ref string printLine)
        {
            string printItem;

            isFirstAzim = true;

            printLine = COMMENT_LINE;

            printLine += "AK,";

            if (anteNulls[FeAnte.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.LICENCE] != Constant.DB_NULL)
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
            if (anteNulls[FeAnte.STATA] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.stata.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.NOTA] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.nota.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.ANTREF] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.antref);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ante.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nAT,";
            if (anteNulls[FeAnte.TXBAND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.txband.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.ACODETX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.acodetx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.AFSLT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.afslt);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.TXHGMAX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.txhgmax);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.TXTRO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.txtro);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.TXPRE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.txpre);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.AHT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.aht);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.AZ] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.az);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.EL] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2}", ante.el);
                printLine += printItem;
            }

            printLine += "\r\nAR,";
            if (anteNulls[FeAnte.RXBAND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.rxband.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.ACODERX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.acoderx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.AFSLR] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.afslr);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.RXHGMAX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.rxhgmax);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.RXTRO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.rxtro);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.RXPRE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.rxpre);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.G_T] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ante.g_t);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.LNAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1}", ante.lnat);
                printLine += printItem;
            }

            printLine += "\r\nAS,";
            if (anteNulls[FeAnte.SATNAME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.satname.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.OP2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ante.op2.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.ORBIT] != Constant.DB_NULL)
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
            if (anteNulls[FeAnte.SATLONG] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2}", ante.satlong);
                printLine += printItem;
            }
            if (anteNulls[FeAnte.SATLONGS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0,1},", ante.satlongs);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.SARC1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ante.sarc1);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (anteNulls[FeAnte.SARC2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2}", ante.sarc2);
                printLine += printItem;
            }

            return 0;
        }

        /// <summary>
        /// This method takes data previously fetched from the table <b>fe_esMyPDF_site</b>
        /// and returns a CSV-formatted string comprising all its field values.
        /// </summary>
        /// <param name="site"> - instance of an FeSite object.</param>
        /// <param name="siteNulls"> - ODBC nullInds associated with site.</param>
        /// <param name="printLine"> - outputs a CSV-formatted string.</param>
        /// <returns></returns>
        public static int FePrintSite(FeSite site, SQLLEN[] siteNulls, ref string printLine)
        {
            string strLongit;
            string strLatit;
            string strLongitOrient;
            string strLatitOrient;
            string printItem;

            printLine = COMMENT_LINE;

            printLine += "SK,";

            if (siteNulls[FeSite.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.LOCATION] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.location.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.NAME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.name.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.PROV] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.prov.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.OPER] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.oper.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.OPRTYP] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.oprtyp.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", site.mtime.Trim());
                printLine += printItem;
            }

            printLine += "\r\nSD,";
            if (siteNulls[FeSite.LATIT] != Constant.DB_NULL)
            {
                GenUtil.UtLatConvStr(site.latit, out strLatit, out strLatitOrient);
                printItem = String.Format("{0}{1},", strLatit, strLatitOrient);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.LONGIT] != Constant.DB_NULL)
            {
                GenUtil.UtLongConvStr(site.longit, out strLongit, out strLongitOrient);
                printItem = String.Format("{0}{1},", strLongit, strLongitOrient);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.GRND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", site.grnd);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.RADIO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.radio.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.RAIN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.rain);
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.STATS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.stats.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.NOTS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.nots.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.REG] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", site.reg.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += " ,";
            }
            if (siteNulls[FeSite.SDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", site.sdate.Trim());
                printLine += printItem;
            }

            return 0;
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
