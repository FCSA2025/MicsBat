# Documented File: FtPrintFw.cs
**Repository Path:** `FtPrint\FtPrintFw.cs`
**Primary Layer:** `FtPrint`
**Namespace:** `FtPrint`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtPrint
{
    using _Configuration;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that convert populated FtTitl, FtChng, 
    /// FtSite, FtAnte and FtChan objects into annotated and formatted lines of text
    /// ready for writing to Console.Out.
    /// </summary>
    public class FtPrintFw
    {
        private const string SPACER = " ,";

        /// <summary>
        /// This method converts the string and numerical field values of a FtTitl 
        /// object into annotated and formatted lines of text ready for writing to 
        /// to Console.Out.
        /// </summary>
        /// <param name="ftTitl"> - prescribed FtTitle object.</param>
        /// <param name="printLine"> - returns a string of multi-line text.</param>
        /// <param name="printFull"> - if 'true' then a comment is inserted into the output text that
        /// describes the syntax of the 'TT' record, i.e. <br>"*TT, validated, namef, source, descr, mdate, mtime"</param>
        public static void PrintTitl(FtTitl ftTitl, out string printLine, bool printFull)
        {
            string printItem;

            if (printFull)
            {
                printLine = "*TT, validated, namef, source, descr, mdate, mtime\r\nTT,";
            }
            else
            {
                printLine = "TT,";
            }

            printItem = String.Format("{0},", ftTitl.validated.Trim());
            printLine += printItem;

            printItem = String.Format("{0},", ftTitl.namef.Trim());
            printLine += printItem;

            printItem = String.Format("{0},", ftTitl.source.Trim());
            printLine += printItem;

            printItem = String.Format("{0},", ftTitl.descr.Trim());
            printLine += printItem;

            printItem = String.Format("{0},", ftTitl.mdate.Trim());
            printLine += printItem;

            printItem = String.Format("{0}", ftTitl.mtime.Trim());
            printLine += printItem;
        }

        /// <summary>
        /// This method converts the string field values of a FtChng 
        /// object into annotated and formatted lines of text ready for writing
        /// to Console.Out.
        /// </summary>
        /// <param name="ftChng"> - prescribed FtChng object.</param>
        /// <param name="printLine"> - returns a string of multi-line text.</param>
        public static void PrintChnge(FtChng ftChng, out string printLine)
        {
            string printItem;

            printLine = "GK,";

            printItem = String.Format("{0},", ftChng.oldcall1.Trim());
            printLine += printItem;

            printItem = String.Format("{0},", ftChng.newcall1.Trim());
            printLine += printItem;

            printItem = String.Format("{0}", ftChng.name.Trim());
            printLine += printItem;
        }

        /// <summary>
        /// This method converts the string and numerical field values of a FtSiteStr 
        /// object, comprising FtSite, FtAnte[] and FtChan[] data, into annotated and 
        /// formatted lines of text and writes them to Console.Out.
        /// </summary>
        /// <param name="ftSiteStr"> - prescribed FtSiteStr object.</param>
        /// <param name="ftSiteStrNulls"> - prescribed FtSiteStrNulls object comprising arrays of ODBC nullInds associated with ftSiteStr.</param>
        /// <param name="printLong"> - if 'true' then all of the fields are converted to text; 
        /// if 'false' then the optional fields are excluded (i.e. those in the AO and CO record CSV lines).</param>
        /// <param name="printFull"> - if 'true' then a comment is inserted into the output text that
        /// describes the syntax of the next record type.</param>
        /// <param name="lines"> - output string providing the created text.</param>
        public static void FtPrintSite(FtSiteStr ftSiteStr, FtSiteStrNulls ftSiteStrNulls, bool printLong, bool printFull, out string lines)
        {
            // 'out' requirement.
            lines = "";

            StringBuilder sb = new StringBuilder();

            string cLine;
            string cOldCall2;
            string cOldBand;
            int nInd;
            int nIndChan;

            PrintSite(ftSiteStr.stSite, ftSiteStrNulls.anSiteNull, out cLine, printFull);
            sb.Append(String.Format("{0}\r\n", cLine));

            //	Go through the antennas, grouping by call1, call2, band (a link)
            cOldCall2 = "";
            cOldBand = "";

            for (nInd = 0; nInd < ftSiteStr.nNumAnts; nInd++)
            {
                FtAnte pAnte = ftSiteStr.stAntsPtr[nInd];

                if (!pAnte.call2.Equals(cOldCall2) ||
                    !pAnte.bndcde.Equals(cOldBand))
                {
                    //	Different antenna.  If not the first, then output the channels.
                    if (cOldCall2.Length != 0)
                    {
                        nIndChan = -1;
                        while (true)
                        {
                            nIndChan = FtUtils.FtNextChanLink(ftSiteStr.stChanPtr, cOldCall2, cOldBand, nIndChan);
                            if (nIndChan < 0)
                            {
                                break;
                            }
                            //	We found a channel for this link.  Print it out.
                            PrintChan(printLong,
                                ftSiteStr.stChanPtr[nIndChan],
                                ftSiteStrNulls.anChanNullPtr[nIndChan],
                                out cLine,
                                printFull);
                            sb.Append(String.Format("{0}\r\n", cLine));
                        }
                    }
                    cOldCall2 = pAnte.call2;
                    cOldBand = pAnte.bndcde;
                }

                //	Output the antenna.
                PrintAnt(printLong, pAnte, ftSiteStrNulls.anAntsNullPtr[nInd], out cLine, printFull, nInd + 1);
                sb.Append(String.Format("{0}\r\n", cLine));
            }

            //	Finish off the channels
            nIndChan = -1;
            nInd = 0;
            while (true)
            {
                nIndChan = FtUtils.FtNextChanLink(ftSiteStr.stChanPtr, cOldCall2, cOldBand, nIndChan);
                if (nIndChan < 0)
                {
                    break;
                }
                //	We found a channel for this link.  Print it out.
                PrintChan(printLong, ftSiteStr.stChanPtr[nIndChan], ftSiteStrNulls.anChanNullPtr[nIndChan], out cLine, printFull);
                sb.Append(String.Format("{0}\r\n", cLine));
                nInd++;
            }

            lines = sb.ToString();
        }

        /// <summary>
        /// This method converts the string and numerical field values of a FtSite 
        /// object into annotated and formatted lines of text ready for writing
        /// to Console.Out.
        /// </summary>
        /// <param name="ftSite"> - prescribed FtSite object.</param>
        /// <param name="siteNulls"> - array of ODBC nullInds associated with ftSite.</param>
        /// <param name="printLine"> - returns a string of multi-line text.</param>
        /// <param name="printFull"> - if 'true' then comments are inserted into the output text that
        /// describe the syntax of the SK and SD record types.</param>
        public static void PrintSite(FtSite ftSite, SQLLEN[] siteNulls, out string printLine, bool printFull)
        {
            string strLongit;
            string strLatit;
            string strLongitOrient;
            string strLatitOrient;
            string printItem;

            printLine = "*##############################################################################";

            if (printFull)
            {
                printLine += "\r\n*SK, cmd, recstat, call1, name, latit, longit, grnd, mdate, mtime:\r\nSK,";
            }
            else
            {
                printLine += "\r\nSK,";
            }

            if (siteNulls[FtSite.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.cmd = ftSite.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.recstat = ftSite.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.call1 = ftSite.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.NAME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.name = ftSite.name.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.LATIT] != Constant.DB_NULL)
            {
                GenUtil.UtLatConvStr(ftSite.latit, out strLatit, out strLatitOrient);
                printItem = String.Format("{0}{1},", strLatit, strLatitOrient);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.LONGIT] != Constant.DB_NULL)
            {
                GenUtil.UtLongConvStr(ftSite.longit, out strLongit, out strLongitOrient);
                printItem = String.Format("{0}{1},", strLongit, strLongitOrient);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.GRND] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftSite.grnd);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.mdate = ftSite.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftSite.mtime = ftSite.mtime.Trim());
                printLine += printItem;
            }

            if (printFull)
            {
                printLine += "\r\n*SD, prov, oper, stats, loc, icaccount, reg, snumb, notwr, nots, spoint, sdate, oprtyp:\r\nSD,";
            }
            else
            {
                printLine += "\r\nSD,";
            }
            if (siteNulls[FtSite.PROV] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.prov = ftSite.prov.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.OPER] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.oper = ftSite.oper.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.STATS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.stats = ftSite.stats.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.LOC] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.loc = ftSite.loc.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.ICACCOUNT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.icaccount = ftSite.icaccount.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.REG] != Constant.DB_NULL)
            {
                if (ftSite.reg != null)
                {
                    printItem = String.Format("{0},", ftSite.reg = ftSite.reg.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.SNUMB] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.snumb = ftSite.snumb.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.NOTWR] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.notwr);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.NOTS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.nots = ftSite.nots.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.SPOINT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.spoint = ftSite.spoint.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.SDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftSite.sdate = ftSite.sdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (siteNulls[FtSite.OPRTYP] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftSite.oprtyp = ftSite.oprtyp.Trim());
                printLine += printItem;
            }
        }

        /// <summary>
        /// This method converts the string and numerical field values of a FtChan 
        /// object into annotated and formatted lines of text ready for writing
        /// to Console.Out.
        /// </summary>
        /// <param name="printLong"> - if 'true' then all of the fields are converted to text; 
        /// if 'false' then the optional fields are excluded (i.e. those in the CO record CSV lines).</param>
        /// <param name="ftChan"> - prescribed FtChan object.</param>
        /// <param name="chanNulls"> - array of ODBC nullInds associated with ftChan.</param>
        /// <param name="printLine"> - returns a string of multi-line text.</param>
        /// <param name="printFull"> - if 'true' then comments are inserted into the output text that
        /// describe the syntax of the CK, CT, CR, CQ and CO CSV record types.</param>
        public static void PrintChan(bool printLong, FtChan ftChan, SQLLEN[] chanNulls,
                                        out string printLine, bool printFull)
        {
            string printItem;
            short loop;
            string cFreqLoss;
            string cBuff;
            int nLen;
            string printchar;

            /*  First create the frequency loss comment for the end of the comment line */
            cFreqLoss = " Freq Loss: ";
            if (ftChan.freqtx != 0.0)
            {
                cBuff = String.Format("Tx={0:F2}", GenUtil.FreeSpaceFreqLoss(ftChan.freqtx / 1000.0));
            }
            else
            {
                cBuff = "";
            }
            cFreqLoss += cBuff;
            if (ftChan.freqrx != 0.0)
            {
                if (ftChan.freqtx != 0.0)
                {
                    cFreqLoss += ", ";
                }
                cBuff = String.Format("Rx={0:F2}", GenUtil.FreeSpaceFreqLoss(ftChan.freqrx / 1000.0));
                cFreqLoss += cBuff;
            }
            nLen = (int)cFreqLoss.Length;

            /*  Now the rest of the channel  */
            printLine = "*";
            printchar = "+";

            for (loop = 0; loop < (75 - nLen); loop++)
            {
                printLine += printchar;
            }
            printLine += cFreqLoss;

            if (printFull)
            {
                printLine += "\r\n*CK, cmd, recstat, call1, call2, bndcde, chid, mdate, mtime:\r\nCK,";
            }
            else
            {
                printLine += "\r\nCK,";
            }

            if (chanNulls[FtChan.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.cmd = ftChan.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.recstat = ftChan.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.call1 = ftChan.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.CALL2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.call2 = ftChan.call2.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.BNDCDE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.bndcde = ftChan.bndcde.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.CHID] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.chid = ftChan.chid.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.mdate = ftChan.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftChan.mtime = ftChan.mtime.Trim());
                printLine += printItem;
            }

            if (printFull)
            {
                printLine += "\r\n*CT, plan, hl, vh, freqtx, poltx, antnumbtx1, afsltx1, antnumbtx2, afsltx2, eqpttx, eqptutx, pwrtx, atpccde, traftx, srvctx, stattx, feetx:\r\nCT, ";
            }
            else
            {
                printLine += "\r\nCT,";
            }
            if (chanNulls[FtChan.SPLAN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.splan = ftChan.splan.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.HL] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.hl);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.VH] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.vh);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.FREQTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftChan.freqtx);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.POLTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.poltx = ftChan.poltx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ANTNUMBTX1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.antnumbtx1);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.AFSLTX1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.afsltx1);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ANTNUMBTX2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.antnumbtx2);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.AFSLTX2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.afsltx2);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.EQPTTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.eqpttx = ftChan.eqpttx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.EQPTUTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.eqptutx = ftChan.eqptutx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.PWRTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.pwrtx);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ATPCCDE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.atpccde);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.TRAFTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.traftx = ftChan.traftx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.SRVCTX] != Constant.DB_NULL)
            {
                if (ftChan.srvctx != null)
                {
                    printItem = String.Format("{0},", ftChan.srvctx = ftChan.srvctx.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.STATTX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.stattx = ftChan.stattx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.FEETX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftChan.feetx = ftChan.feetx.Trim());
                printLine += printItem;
            }

            if (printFull)
            {
                printLine += "\r\n*CR, freqrx, polrx, antnumbrx1, antnumbrx2, antnumbrx3, eqptrx, eqpturx, trafrx, srvcrx, statrx, feerx:\r\nCR, ";
            }
            else
            {
                printLine += "\r\nCR,";
            }

            if (chanNulls[FtChan.FREQRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftChan.freqrx);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.POLRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.polrx = ftChan.polrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ANTNUMBRX1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.antnumbrx1);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ANTNUMBRX2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.antnumbrx2);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ANTNUMBRX3] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.antnumbrx3);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.EQPTRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.eqptrx = ftChan.eqptrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.EQPTURX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.eqpturx = ftChan.eqpturx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.TRAFRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.trafrx = ftChan.trafrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.SRVCRX] != Constant.DB_NULL)
            {
                if (ftChan.srvcrx != null)
                {
                    printItem = String.Format("{0},", ftChan.srvcrx = ftChan.srvcrx.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.STATRX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftChan.statrx = ftChan.statrx.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.FEERX] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftChan.feerx = ftChan.feerx.Trim());
                printLine += printItem;
            }

            if (printFull)
            {
                printLine += "\r\n*CQ, afslrx1, pwrrx1, afslrx2, pwrrx2, afslrx3, pwrrx3, esint, tsint:\r\nCQ, ";
            }
            else
            {
                printLine += "\r\nCQ,";
            }

            if (chanNulls[FtChan.AFSLRX1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.afslrx1);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.PWRRX1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.pwrrx1);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.AFSLRX2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.afslrx2);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.PWRRX2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.pwrrx2);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.AFSLRX3] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.afslrx3);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.PWRRX3] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.pwrrx3);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.ESINT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftChan.esint);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (chanNulls[FtChan.TSINT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1}", ftChan.tsint);
                printLine += printItem;
            }

            if (printLong)
            {
                if (printFull)
                {
                    printLine += "\r\n*CO, routnumb, stnnumb, hopnumb, notetx, noterx, noteegn, cpoint, sdate:\r\nCO, ";
                }
                else
                {
                    printLine += "\r\nCO,";
                }
                if (chanNulls[FtChan.ROUTNUMB] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.routnumb);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.STNNUMB] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.stnnumb);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.HOPNUMB] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.hopnumb);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.NOTETX] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.notetx = ftChan.notetx.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.NOTERX] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.noterx = ftChan.noterx.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.NOTEGNL] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.notegnl = ftChan.notegnl.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.CPOINT] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftChan.cpoint = ftChan.cpoint.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (chanNulls[FtChan.SDATE] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0}", ftChan.sdate = ftChan.sdate.Trim());
                    printLine += printItem;
                }
            }
        }

        /// <summary>
        /// This method converts the string and numerical field values of a FtAnte 
        /// object into annotated and formatted lines of text ready for writing
        /// to Console.Out.
        /// </summary>
        /// <param name="printLong"> - if 'true' then all of the fields are converted to text; 
        /// if 'false' then the optional fields are excluded (i.e. those in the AO CSV record lines).</param>
        /// <param name="ftAnte"> - prescribed FtAnte object.</param>
        /// <param name="anteNulls"> - array of ODBC nullInds associated with ftAnte.</param>
        /// <param name="printLine"> - returns a string of multi-line text.</param>
        /// <param name="printFull"> - if 'true' then comments are inserted into the output text that
        /// describe the syntax of the AK, AQ and AO CSV record types.</param>
        /// <param name="nAno"> - prescribes the order of appearance of the FtAnte record within the FtSiteStr's FtAnte[] array; the first appearance is index = 1.</param>
        public static void PrintAnt(bool printLong, FtAnte ftAnte, SQLLEN[] anteNulls, out string printLine, bool printFull, int nAno)
        {
            string printItem;
            short namesize, loop;
            string call2name;
            string call2;
            SuAntStr pAnt;
            MtSiteStr pOtherEnd;
            int nRet;
            string cGain = "(Not Found)";
            string cDist = "(No Dist.)";
            string cAtten = "(No Atten.)";
            string printchar;
            int nLen;
            double dMidBandKHz;
            double dAtten;

            call2 = ftAnte.call2;
            call2name = "------ Not Found -----";
            namesize = (short)call2name.Length;

            /*  Get the antenna gain  */
            if ((nRet = Suutils.SuGetAnt(ftAnte.acode, out pAnt)) == 0)
            {
                cGain = String.Format("{0:F1}", (double)pAnt.acAnt.again);
            }

            //	Get the mid-band frequency
            SuBand pBand;
            nRet = Suutils.SuGetBand(ftAnte.bndcde, out pBand);
            if (nRet == 0)
            {
                dMidBandKHz = pBand.bmidf;
            }
            else
            {
                dMidBandKHz = 0.0;
            }

            /*  Get the distance losses  */
            if (ftAnte.dist > 0.0)
            {
                cDist = String.Format("{0:F2}", GenUtil.FreeSpaceDistLoss(ftAnte.dist));

                //	Display the attenuation losses
                if (dMidBandKHz != 0.0)
                {
                    dAtten = GenUtil.AtmosphericAtten(dMidBandKHz, ftAnte.dist);
                    cAtten = String.Format("{0:F2}", dAtten);
                }
            }

            if (nAno == 1)
            {
                printchar = "=";
            }
            else
            {
                printchar = "-";
            }
            printLine = "*";
            for (loop = 0; loop < 4; loop++)
            {
                printLine += printchar;
            }

            printLine += "  ";
            nRet = MtUtils.MtGetSite(ftAnte.call2, out pOtherEnd, 1); // Just haul in the site info.
            if (nRet == 0)
            {
                call2name = pOtherEnd.stSite.name.Trim();
            }
            printLine += call2name;
            printLine += "  ";

            nLen = (int)printLine.Length;
            for (loop = (short)nLen; loop < 25; loop++)
            {
                printLine += "=";
            }
            printLine += " Ant Gain = ";
            printLine += cGain;
            printLine += ", Dist Loss = ";
            printLine += cDist;
            printLine += ", At. Loss = ";
            printLine += cAtten;
            if (printFull)
            {
                printLine += "\r\n*AK, cmd, recstat, call1, call2, bndcde, anum, mdate, mtime:\r\nAK, ";
            }
            else
            {
                printLine += "\r\nAK,";
            }

            if (anteNulls[FtAnte.CMD] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.cmd = ftAnte.cmd.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.RECSTAT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.recstat = ftAnte.recstat.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.CALL1] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.call1 = ftAnte.call1.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.CALL2] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.call2 = ftAnte.call2.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.BNDCDE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.bndcde = ftAnte.bndcde.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.ANUM] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.anum);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.MDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.mdate = ftAnte.mdate.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.MTIME] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftAnte.mtime = ftAnte.mtime.Trim());
                printLine += printItem;
            }

            if (printFull)
            {
                printLine += "\r\n*AQ, ause, acode, aht, azmth, elvtn, dist, offazm, tazmth, telvtn, tgain, obsloss, kvalue, atwrno, sdate:\r\nAQ,";
            }
            else
            {
                printLine += "\r\nAQ,";
            }
            if (anteNulls[FtAnte.AUSE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.ause = ftAnte.ause.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.ACODE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.acode = ftAnte.acode.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.AHT] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftAnte.aht);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.AZMTH] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftAnte.azmth);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.ELVTN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftAnte.elvtn);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.DIST] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftAnte.dist);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.OFFAZM] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.offazm = ftAnte.offazm.Trim());
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.TAZMTH] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftAnte.tazmth);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.TELVTN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftAnte.telvtn);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.TGAIN] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftAnte.tgain);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.OBSLOSS] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F1},", ftAnte.obsloss);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.KVALUE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0:F2},", ftAnte.kvalue);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.ATWRNO] != Constant.DB_NULL)
            {
                printItem = String.Format("{0},", ftAnte.atwrno);
                printLine += printItem;
            }
            else
            {
                printLine += SPACER;
            }
            if (anteNulls[FtAnte.SDATE] != Constant.DB_NULL)
            {
                printItem = String.Format("{0}", ftAnte.sdate = ftAnte.sdate.Trim());
                printLine += printItem;
            }

            if (printLong == true)
            {
                if (printFull)
                {
                    printLine += "\r\n*AO, txfdlnth, txfdlnlh, txfdlntv, txfdlnlv, txpadpam, txcompl, rxfdlnth, rxfdlnlh, rxfdlntv, rxfdlnlv, rxpadlna, rxcompl, nota, apoint, licence:\r\nAO,";
                }
                else
                {
                    printLine += "\r\nAO,";
                }
                if (anteNulls[FtAnte.TXFDLNTH] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftAnte.txfdlnth = ftAnte.txfdlnth.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.TXFDLNLH] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.txfdlnlh);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.TXFDLNTV] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftAnte.txfdlntv = ftAnte.txfdlntv.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.TXFDLNLV] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.txfdlnlv);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.TXPADPAM] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.txpadpam);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.TXCOMPL] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.txcompl);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.RXFDLNTH] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftAnte.rxfdlnth = ftAnte.rxfdlnth.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.RXFDLNLH] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.rxfdlnlh);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.RXFDLNTV] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftAnte.rxfdlntv = ftAnte.rxfdlntv.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.RXFDLNLV] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.rxfdlnlv);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.RXPADLNA] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.rxpadlna);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.RXCOMPL] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0:F1},", ftAnte.rxcompl);
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.NOTA] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftAnte.nota = ftAnte.nota.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.APOINT] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0},", ftAnte.apoint = ftAnte.apoint.Trim());
                    printLine += printItem;
                }
                else
                {
                    printLine += SPACER;
                }
                if (anteNulls[FtAnte.LICENCE] != Constant.DB_NULL)
                {
                    printItem = String.Format("{0}", ftAnte.licence = ftAnte.licence.Trim());
                    printLine += printItem;
                }
            }
        }










    }
}

```
