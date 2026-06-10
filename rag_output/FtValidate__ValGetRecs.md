# Documented File: ValGetRecs.cs
**Repository Path:** `FtValidate\ValGetRecs.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
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
    /// Provides methods to get site, antenna and channel records from the database.
    /// </summary>
    public class ValGetRecs
    {
        /// <summary>
        /// This method gets a channel record for given key.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call1"> - call1 of channel record.</param>
        /// <param name="call2"> - call2 of channel record.</param>
        /// <param name="bndcde"> - bndcde of channel record.</param>
        /// <param name="chid"> - chid of channel record.</param>
        /// <param name="chan"> - channel object populated with data.</param>
        /// <param name="nulls"> - array of ODBC nullInds.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int GetChan(string pdfName, string call1, string call2, string bndcde, string chid, out FtChan chan, out SQLLEN[] nulls)
        {
            //...Log2.v("\n\nValGetRecs.GetChan(): Entry");

            MtChan mtChan;
            SQLLEN[] nullInd;   /* null ind for chan record */
            string selClause;        /* selection criteria */
            string intTableNm;    /* internal pdf table name */
            int chanHandle;         /* handle for PDF chan table */
            int nRet;

            // Satisfy 'out' requirement.
            chan = null;
            nulls = null;

            /* Convert PDF name to internal Chan table name */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out intTableNm);

            /* Search the PDF for the chan first */
            selClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                                            call1, call2, bndcde, chid);

            chanHandle = DynChannel.FtSelectChannel(intTableNm, selClause, "");
            if (chanHandle < 0)
            {
                /* Some bad problem with the PDF site table */
                //...Log2.v("\n\nValGetRecs.GetChan(): Exit A");
                return (Constant.FAILURE);
            }

            if (DynChannel.FtFetchChannel(chanHandle, out chan, out nulls) >= 0)
            {
                /* Got the record */
                DynChannel.FtCloseChannel(chanHandle);
                //...Log2.v("\n\nValGetRecs.GetChan(): Exit B");
                return (Constant.SUCCESS);
            }

            DynChannel.FtCloseChannel(chanHandle);

            /* -- Come here if chan record not in PDF -- */

            chanHandle = DynMdbChannel.MtSelectChannel(selClause, "");
            nRet = DynMdbChannel.MtFetchChannel(chanHandle, out mtChan, out nullInd);
            DynMdbChannel.MtCloseChannel(chanHandle);
            if (nRet != 0)
            {
                //...Log2.v("\n\nValGetRecs.GetChan(): Exit C");
                return (Constant.FAILURE);
            }

            FtValCopy.FtCopyChan(ref chan, mtChan, ref nulls, nullInd);
            chan.cmd = "N";
            chan.recstat = "C";
            nulls[FtAnte.CMD] = Constant.DB_NOT_NULL;
            nulls[FtAnte.RECSTAT] = Constant.DB_NOT_NULL;

            //...Log2.v("\n\nValGetRecs.GetChan(): Exit");
            return (Constant.SUCCESS);

        }   /* ----- End of getChan ----- */

        /// <summary>
        /// This method gets an antenna record for given key.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call1"> - call1 of channel record.</param>
        /// <param name="call2"> - call2 of channel record.</param>
        /// <param name="bndcde"> - bndcde of channel record.</param>
        /// <param name="anum"> - anum of channel record.</param>
        /// <param name="ante"> - antenna object populated with data.</param>
        /// <param name="nulls"> - array of ODBC nullInds.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int GetAnte(
                                    string pdfName,   /* short form of pdf name */
                                    string call1,     /* call1 of antenna record */
                                    string call2,     /* call2 of antenna record */
                                    string bndcde,    /* bndcde of antenna record */
                                    short anum,		/* anum of antenna record */
                                    out FtAnte ante,    /* Struct. to return info in */
                                    out SQLLEN[] nulls /* Null array to return */
                                 )
        {
            //...Log2.v("\n\nValGetRecs.GetAnte(): Entry: " + pdfName + "   " + call1 + "   " + call2 + "   " + bndcde + "   " + anum);

            MtAnte mtAnte;
            SQLLEN[] nullInd = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);   /* null ind for ante record */
            string selClause;    /* selection criteria */
            string intTableNm;   /* internal pdf table name */
            int anteHandle;         /* handle for PDF ante table */
            int nRet;

            // Satisfy 'out' parameter requirements.
            ante = null;
            nulls = null;

            /* Convert PDF name to internal Ante table name */
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out intTableNm);

            /* Search the PDF for the ante first */
            selClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and anum={3}",
                                        call1, call2, bndcde, anum);

            anteHandle = DynAntenna.FtSelectAntenna(intTableNm, selClause, "");
            if (anteHandle < 0)
            {
                /* Some bad problem with the PDF site table */
                //...Log2.v("\n\nValGetRecs.GetAnte(): Exit A");
                return (Constant.FAILURE);
            }

            if (DynAntenna.FtFetchAntenna(anteHandle, out ante, out nulls) == Constant.SUCCESS)
            {
                /* Got the record */
                DynAntenna.FtCloseAntenna(anteHandle);
                //...Log2.v("\n\nValGetRecs.GetAnte(): Exit B");
                return (Constant.SUCCESS);
            }


            DynAntenna.FtCloseAntenna(anteHandle);

            anteHandle = DynMdbAntenna.MtSelectAntenna(selClause, "");
            nRet = DynMdbAntenna.MtFetchAntenna(anteHandle, out mtAnte, out nullInd);

            DynMdbAntenna.MtCloseAntenna(anteHandle);
            if (nRet != 0)
            {
                //...Log2.v("\n\nValGetRecs.GetAnte(): Exit C");
                return (Constant.FAILURE);
            }


            FtValCopy.FtCopyAnte(ref ante, mtAnte, ref nulls, nullInd);

            ante.cmd = "N";
            ante.recstat = "C";
            nulls[FtAnte.CMD] = Constant.DB_NOT_NULL;
            nulls[FtAnte.RECSTAT] = Constant.DB_NOT_NULL;

            //...Log2.v("\n\nValGetRecs.GetAnte(): Exit");
            return (Constant.SUCCESS);

        }	/* ----- End of getAnte ----- */

        /// <summary>
        /// This method gets a site record for a prescribed call1.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call1"> - call1 for a site.</param>
        /// <param name="site"> - site object populated with data.</param>
        /// <param name="nulls"> - array of ODBC nullInds.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int GetSite(string pdfName,     /* short form of pdf name */
                                     string call1,		/* call sign of site */
                                     out FtSite site,
                                     out SQLLEN[] nulls)
        {
            //...Log2.v("\n\nValGetRecs.GetSite(): Entry");

            MtSite mtSite;
            SQLLEN[] nullInd = NullHelper.CreateArrayOfNullInd(MtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);   /* null ind for site record */
            string selClause;       /* selection criteria */
            string intTableNm;      /* internal pdf table name */
            int siteHandle;         /* site handle for PDF site table */
            int nRet;

            // Satisfy 'out' parameter requirement.
            site = null;
            nulls = null;

            /* Convert PDF name to internal Site table name */
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out intTableNm);

            /* Search the PDF for the site */
            selClause = String.Format(" call1='{0}'", call1);
            siteHandle = DynSite.FtSelectSite(intTableNm, selClause, "");
            if (siteHandle < 0)
            {
                /* Some bad problem with the PDF site table */
                //...Log2.v("\n\nValGetRecs.GetSite(): Exit A");
                return (Constant.FAILURE);
            }

            if (DynSite.FtFetchSite(siteHandle, out site, out nulls) >= 0)
            {
                /* Got the record */
                DynSite.FtCloseSite(siteHandle);
                //...Log2.v("\n\nValGetRecs.GetSite(): Exit B");
                return (Constant.SUCCESS);
            }

            /* Come here if record not in PDF */
            DynSite.FtCloseSite(siteHandle);

            //exec sql select *
            //           into :mtSite:nullInd
            //					 from mt_site
            //					where :selClause;
            //if ( sqlca.sqlcode != 0 ) {
            siteHandle = DynMdbSite.MtSelectSite(selClause, "");

            nRet = DynMdbSite.MtFetchSite(siteHandle, out mtSite, out nullInd);

            DynMdbSite.MtCloseSite(siteHandle);
            if (nRet != 0)
            {
                //...Log2.v("\n\nValGetRecs.GetSite(): Exit C");
                return (Constant.FAILURE);
            }

            FtValCopy.FtCopySite(ref site, mtSite, ref nulls, nullInd);

            site.cmd = "N";
            site.recstat = "C";
            nulls[FtSite.CMD] = Constant.DB_NOT_NULL;
            nulls[FtSite.RECSTAT] = Constant.DB_NOT_NULL;

            //...Log2.v("\n\nValGetRecs.GetSite(): Exit");
            return (Constant.SUCCESS);

        }	/* ----- End of getSite ----- */

        /// <summary>
        /// This method gets a channel record for a given selection clause.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="selCriteria"> - channel selection criteria clause.</param>
        /// <param name="chan"> - channel object populated with data.</param>
        /// <param name="nulls"> - array of ODBC nullInds.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int GetChanByClause(string pdfName, string selCriteria, out FtChan chan, out SQLLEN[] nulls)
        {
            //...Log2.v("\n\nValPassive.GetChanByClause(): Entry");

            MtChan mtChan;
            SQLLEN[] nullInd;   /* null ind for chan record */
            string selClause;        /* selection criteria */

            /* Local variables */
            string intTableNm;   /* internal pdf table name */
            int chanHandle;         /* handle for PDF chan table */
            int nRet;

            // Satisfy 'out' requirement.
            chan = null;
            nulls = null;

            /* Get selection criteria where MS SQL Server can see it */
            selClause = selCriteria;

            /* Convert PDF name to internal Chan table name */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out intTableNm);

            /* Search the PDF for the chan first */
            chanHandle = DynChannel.FtSelectChannel(intTableNm, selClause, "");
            if (chanHandle < 0)
            {
                /* Some bad problem with the PDF site table */
                //...Log2.v("\n\nValPassive.GetChanByClause(): Exit A: bad problem with the PDF site table");
                return (Constant.FAILURE);
            }

            if (DynChannel.FtFetchChannel(chanHandle, out chan, out nulls) >= 0)
            {
                /* Got the record */
                DynChannel.FtCloseChannel(chanHandle);
                //...Log2.v("\n\nValPassive.GetChanByClause(): Exit B");
                return (Constant.SUCCESS);
            }


            DynChannel.FtCloseChannel(chanHandle);

            // We reach here if chan record not in PDF.

            chanHandle = DynMdbChannel.MtSelectChannel(selClause, "");
            nRet = DynMdbChannel.MtFetchChannel(chanHandle, out mtChan, out nullInd);

            DynMdbChannel.MtCloseChannel(chanHandle);
            if (nRet != 0)
            {
                //...Log2.v("\n\nValPassive.GetChanByClause(): Exit C");
                return (Constant.FAILURE);
            }

            FtValCopy.FtCopyChan(ref chan, mtChan, ref nulls, nullInd);

            chan.cmd = "N";
            chan.recstat = "C";

            nulls[FtAnte.CMD] = Constant.DB_NOT_NULL;
            nulls[FtAnte.RECSTAT] = Constant.DB_NOT_NULL;

            //...Log2.v("\n\nValPassive.GetChanByClause(): Exit");
            return (Constant.SUCCESS);

        }	/* ----- End of getChanByClause ----- */

    }
}

```
