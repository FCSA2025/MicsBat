# Documented File: Make.cs
**Repository Path:** `MtUpdate\Make.cs`
**Primary Layer:** `MtUpdate`
**Namespace:** `MtUpdate`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtUpdate
{
    using _Configuration;
    using _NewLib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that copy member values from FtXxxx objects to
    /// MtXxxx objects, e.g. populate an MtSite object using values in a FtSite object.
    /// </summary>
    public class Make
    {

        /// <summary>
        /// This method instantiates an MtChan object that is populated with member values
        /// from a prescribed FtChan object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="ftChan"> - input FtChan object.</param>
        /// <param name="ftChanNullInds"> - input array of ftChan null indicators.</param>
        /// <param name="mtChan"> - output populated MtChan object.</param>
        /// <param name="mtChanNullInds"> - output array of MtChan null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MtChanFromFtChan(FtChan ftChan, SQLLEN[] ftChanNullInds, out MtChan mtChan, out SQLLEN[] mtChanNullInds)
        {
            // 'out' requirement.
            mtChan = null;
            mtChanNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: ftChan and ftChanNullInds.
            if (ftChan == null)
            {
                errorCode = -1;
            }
            else if (ftChanNullInds == null)
            {
                errorCode = -2;
            }
            else if (ftChanNullInds.Length != FtChan.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MtChanFromFtChan(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            mtChan = new MtChan();
            mtChanNullInds = NullHelper.CreateArrayOfNullInd(MtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.
            mtChan.call1 = ftChan.call1;
            mtChanNullInds[MtChan.CALL1] = ftChanNullInds[FtChan.CALL1];

            mtChan.call2 = ftChan.call2;
            mtChanNullInds[MtChan.CALL2] = ftChanNullInds[FtChan.CALL2];

            mtChan.bndcde = ftChan.bndcde;
            mtChanNullInds[MtChan.BNDCDE] = ftChanNullInds[FtChan.BNDCDE];

            mtChan.splan = ftChan.splan;
            mtChanNullInds[MtChan.SPLAN] = ftChanNullInds[FtChan.SPLAN];

            mtChan.hl = ftChan.hl;
            mtChanNullInds[MtChan.HL] = ftChanNullInds[FtChan.HL];

            mtChan.vh = ftChan.vh;
            mtChanNullInds[MtChan.VH] = ftChanNullInds[FtChan.VH];

            mtChan.chid = ftChan.chid;
            mtChanNullInds[MtChan.CHID] = ftChanNullInds[FtChan.CHID];

            mtChan.freqtx = ftChan.freqtx;
            mtChanNullInds[MtChan.FREQTX] = ftChanNullInds[FtChan.FREQTX];

            mtChan.poltx = ftChan.poltx;
            mtChanNullInds[MtChan.POLTX] = ftChanNullInds[FtChan.POLTX];

            mtChan.antnumbtx1 = ftChan.antnumbtx1;
            mtChanNullInds[MtChan.ANTNUMBTX1] = ftChanNullInds[FtChan.ANTNUMBTX1];

            mtChan.antnumbtx2 = ftChan.antnumbtx2;
            mtChanNullInds[MtChan.ANTNUMBTX2] = ftChanNullInds[FtChan.ANTNUMBTX2];

            mtChan.eqpttx = ftChan.eqpttx;
            mtChanNullInds[MtChan.EQPTTX] = ftChanNullInds[FtChan.EQPTTX];

            mtChan.eqptutx = ftChan.eqptutx;
            mtChanNullInds[MtChan.EQPTUTX] = ftChanNullInds[FtChan.EQPTUTX];

            mtChan.pwrtx = ftChan.pwrtx;
            mtChanNullInds[MtChan.PWRTX] = ftChanNullInds[FtChan.PWRTX];

            mtChan.atpccde = ftChan.atpccde;
            mtChanNullInds[MtChan.ATPCCDE] = ftChanNullInds[FtChan.ATPCCDE];

            mtChan.afsltx1 = ftChan.afsltx1;
            mtChanNullInds[MtChan.AFSLTX1] = ftChanNullInds[FtChan.AFSLTX1];

            mtChan.afsltx2 = ftChan.afsltx2;
            mtChanNullInds[MtChan.AFSLTX2] = ftChanNullInds[FtChan.AFSLTX2];

            mtChan.traftx = ftChan.traftx;
            mtChanNullInds[MtChan.TRAFTX] = ftChanNullInds[FtChan.TRAFTX];

            mtChan.srvctx = ftChan.srvctx;
            mtChanNullInds[MtChan.SRVCTX] = ftChanNullInds[FtChan.SRVCTX];

            mtChan.stattx = ftChan.stattx;
            mtChanNullInds[MtChan.STATTX] = ftChanNullInds[FtChan.STATTX];

            mtChan.freqrx = ftChan.freqrx;
            mtChanNullInds[MtChan.FREQRX] = ftChanNullInds[FtChan.FREQRX];

            mtChan.polrx = ftChan.polrx;
            mtChanNullInds[MtChan.POLRX] = ftChanNullInds[FtChan.POLRX];

            mtChan.antnumbrx1 = ftChan.antnumbrx1;
            mtChanNullInds[MtChan.ANTNUMBRX1] = ftChanNullInds[FtChan.ANTNUMBRX1];

            mtChan.antnumbrx2 = ftChan.antnumbrx2;
            mtChanNullInds[MtChan.ANTNUMBRX2] = ftChanNullInds[FtChan.ANTNUMBRX2];

            mtChan.antnumbrx3 = ftChan.antnumbrx3;
            mtChanNullInds[MtChan.ANTNUMBRX3] = ftChanNullInds[FtChan.ANTNUMBRX3];

            mtChan.eqptrx = ftChan.eqptrx;
            mtChanNullInds[MtChan.EQPTRX] = ftChanNullInds[FtChan.EQPTRX];

            mtChan.eqpturx = ftChan.eqpturx;
            mtChanNullInds[MtChan.EQPTURX] = ftChanNullInds[FtChan.EQPTURX];

            mtChan.afslrx1 = ftChan.afslrx1;
            mtChanNullInds[MtChan.AFSLRX1] = ftChanNullInds[FtChan.AFSLRX1];

            mtChan.afslrx2 = ftChan.afslrx2;
            mtChanNullInds[MtChan.AFSLRX2] = ftChanNullInds[FtChan.AFSLRX2];

            mtChan.afslrx3 = ftChan.afslrx3;
            mtChanNullInds[MtChan.AFSLRX3] = ftChanNullInds[FtChan.AFSLRX3];

            mtChan.pwrrx1 = ftChan.pwrrx1;
            mtChanNullInds[MtChan.PWRRX1] = ftChanNullInds[FtChan.PWRRX1];

            mtChan.pwrrx2 = ftChan.pwrrx2;
            mtChanNullInds[MtChan.PWRRX2] = ftChanNullInds[FtChan.PWRRX2];

            mtChan.pwrrx3 = ftChan.pwrrx3;
            mtChanNullInds[MtChan.PWRRX3] = ftChanNullInds[FtChan.PWRRX3];

            mtChan.trafrx = ftChan.trafrx;
            mtChanNullInds[MtChan.TRAFRX] = ftChanNullInds[FtChan.TRAFRX];

            mtChan.esint = ftChan.esint;
            mtChanNullInds[MtChan.ESINT] = ftChanNullInds[FtChan.ESINT];

            mtChan.tsint = ftChan.tsint;
            mtChanNullInds[MtChan.TSINT] = ftChanNullInds[FtChan.TSINT];

            mtChan.srvcrx = ftChan.srvcrx;
            mtChanNullInds[MtChan.SRVCRX] = ftChanNullInds[FtChan.SRVCRX];

            mtChan.statrx = ftChan.statrx;
            mtChanNullInds[MtChan.STATRX] = ftChanNullInds[FtChan.STATRX];

            mtChan.routnumb = ftChan.routnumb;
            mtChanNullInds[MtChan.ROUTNUMB] = ftChanNullInds[FtChan.ROUTNUMB];

            mtChan.stnnumb = ftChan.stnnumb;
            mtChanNullInds[MtChan.STNNUMB] = ftChanNullInds[FtChan.STNNUMB];

            mtChan.hopnumb = ftChan.hopnumb;
            mtChanNullInds[MtChan.HOPNUMB] = ftChanNullInds[FtChan.HOPNUMB];

            mtChan.sdate = ftChan.sdate;
            mtChanNullInds[MtChan.SDATE] = ftChanNullInds[FtChan.SDATE];

            mtChan.notetx = ftChan.notetx;
            mtChanNullInds[MtChan.NOTETX] = ftChanNullInds[FtChan.NOTETX];

            mtChan.noterx = ftChan.noterx;
            mtChanNullInds[MtChan.NOTERX] = ftChanNullInds[FtChan.NOTERX];

            mtChan.notegnl = ftChan.notegnl;
            mtChanNullInds[MtChan.NOTEGNL] = ftChanNullInds[FtChan.NOTEGNL];

            mtChan.cpoint = ftChan.cpoint;
            mtChanNullInds[MtChan.CPOINT] = ftChanNullInds[FtChan.CPOINT];

            mtChan.feetx = ftChan.feetx;
            mtChanNullInds[MtChan.FEETX] = ftChanNullInds[FtChan.FEETX];

            mtChan.feerx = ftChan.feerx;
            mtChanNullInds[MtChan.FEERX] = ftChanNullInds[FtChan.FEERX];

            mtChan.mdate = ftChan.mdate;
            mtChanNullInds[MtChan.MDATE] = ftChanNullInds[FtChan.MDATE];

            mtChan.mtime = ftChan.mtime;
            mtChanNullInds[MtChan.MTIME] = ftChanNullInds[FtChan.MTIME];

            // Handle the special cases.
            mtChan.userid = "";
            mtChanNullInds[MtChan.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method instantiates an MtAnte object that is populated with member values
        /// from a prescribed FtAnte object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="ftAnte"> - input FtAnte object.</param>
        /// <param name="ftAnteNullInds"> - input array of ftAnte null indicators.</param>
        /// <param name="mtAnte"> - output populated MtAnte object.</param>
        /// <param name="mtAnteNullInds"> - output array of MtAnte null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MtAnteFromFtAnte(FtAnte ftAnte, SQLLEN[] ftAnteNullInds, out MtAnte mtAnte, out SQLLEN[] mtAnteNullInds)
        {
            // 'out' requirement.
            mtAnte = null;
            mtAnteNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: ftAnte and ftAnteNullInds.
            if (ftAnte == null)
            {
                errorCode = -1;
            }
            else if (ftAnteNullInds == null)
            {
                errorCode = -2;
            }
            else if (ftAnteNullInds.Length != FtAnte.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MtAnteFromFtAnte(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            mtAnte = new MtAnte();
            mtAnteNullInds = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.

            mtAnte.call1 = ftAnte.call1;
            mtAnteNullInds[MtAnte.CALL1] = ftAnteNullInds[FtAnte.CALL1];

            mtAnte.call2 = ftAnte.call2;
            mtAnteNullInds[MtAnte.CALL2] = ftAnteNullInds[FtAnte.CALL2];

            mtAnte.bndcde = ftAnte.bndcde;
            mtAnteNullInds[MtAnte.BNDCDE] = ftAnteNullInds[FtAnte.BNDCDE];

            mtAnte.anum = ftAnte.anum;
            mtAnteNullInds[MtAnte.ANUM] = ftAnteNullInds[FtAnte.ANUM];

            mtAnte.ause = ftAnte.ause;
            mtAnteNullInds[MtAnte.AUSE] = ftAnteNullInds[FtAnte.AUSE];

            mtAnte.acode = ftAnte.acode;
            mtAnteNullInds[MtAnte.ACODE] = ftAnteNullInds[FtAnte.ACODE];

            mtAnte.aht = ftAnte.aht;
            mtAnteNullInds[MtAnte.AHT] = ftAnteNullInds[FtAnte.AHT];

            mtAnte.azmth = ftAnte.azmth;
            mtAnteNullInds[MtAnte.AZMTH] = ftAnteNullInds[FtAnte.AZMTH];

            mtAnte.elvtn = ftAnte.elvtn;
            mtAnteNullInds[MtAnte.ELVTN] = ftAnteNullInds[FtAnte.ELVTN];

            mtAnte.dist = ftAnte.dist;
            mtAnteNullInds[MtAnte.DIST] = ftAnteNullInds[FtAnte.DIST];

            mtAnte.offazm = ftAnte.offazm;
            mtAnteNullInds[MtAnte.OFFAZM] = ftAnteNullInds[FtAnte.OFFAZM];

            mtAnte.tazmth = ftAnte.tazmth;
            mtAnteNullInds[MtAnte.TAZMTH] = ftAnteNullInds[FtAnte.TAZMTH];

            mtAnte.telvtn = ftAnte.telvtn;
            mtAnteNullInds[MtAnte.TELVTN] = ftAnteNullInds[FtAnte.TELVTN];

            mtAnte.tgain = ftAnte.tgain;
            mtAnteNullInds[MtAnte.TGAIN] = ftAnteNullInds[FtAnte.TGAIN];

            mtAnte.txfdlnth = ftAnte.txfdlnth;
            mtAnteNullInds[MtAnte.TXFDLNTH] = ftAnteNullInds[FtAnte.TXFDLNTH];

            mtAnte.txfdlnlh = ftAnte.txfdlnlh;
            mtAnteNullInds[MtAnte.TXFDLNLH] = ftAnteNullInds[FtAnte.TXFDLNLH];

            mtAnte.txfdlntv = ftAnte.txfdlntv;
            mtAnteNullInds[MtAnte.TXFDLNTV] = ftAnteNullInds[FtAnte.TXFDLNTV];

            mtAnte.txfdlnlv = ftAnte.txfdlnlv;
            mtAnteNullInds[MtAnte.TXFDLNLV] = ftAnteNullInds[FtAnte.TXFDLNLV];

            mtAnte.rxfdlnth = ftAnte.rxfdlnth;
            mtAnteNullInds[MtAnte.RXFDLNTH] = ftAnteNullInds[FtAnte.RXFDLNTH];

            mtAnte.rxfdlnlh = ftAnte.rxfdlnlh;
            mtAnteNullInds[MtAnte.RXFDLNLH] = ftAnteNullInds[FtAnte.RXFDLNLH];

            mtAnte.rxfdlntv = ftAnte.rxfdlntv;
            mtAnteNullInds[MtAnte.RXFDLNTV] = ftAnteNullInds[FtAnte.RXFDLNTV];

            mtAnte.rxfdlnlv = ftAnte.rxfdlnlv;
            mtAnteNullInds[MtAnte.RXFDLNLV] = ftAnteNullInds[FtAnte.RXFDLNLV];

            mtAnte.txpadpam = ftAnte.txpadpam;
            mtAnteNullInds[MtAnte.TXPADPAM] = ftAnteNullInds[FtAnte.TXPADPAM];

            mtAnte.rxpadlna = ftAnte.rxpadlna;
            mtAnteNullInds[MtAnte.RXPADLNA] = ftAnteNullInds[FtAnte.RXPADLNA];

            mtAnte.txcompl = ftAnte.txcompl;
            mtAnteNullInds[MtAnte.TXCOMPL] = ftAnteNullInds[FtAnte.TXCOMPL];

            mtAnte.rxcompl = ftAnte.rxcompl;
            mtAnteNullInds[MtAnte.RXCOMPL] = ftAnteNullInds[FtAnte.RXCOMPL];

            mtAnte.obsloss = ftAnte.obsloss;
            mtAnteNullInds[MtAnte.OBSLOSS] = ftAnteNullInds[FtAnte.OBSLOSS];

            mtAnte.kvalue = ftAnte.kvalue;
            mtAnteNullInds[MtAnte.KVALUE] = ftAnteNullInds[FtAnte.KVALUE];

            mtAnte.atwrno = ftAnte.atwrno;
            mtAnteNullInds[MtAnte.ATWRNO] = ftAnteNullInds[FtAnte.ATWRNO];

            mtAnte.nota = ftAnte.nota;
            mtAnteNullInds[MtAnte.NOTA] = ftAnteNullInds[FtAnte.NOTA];

            mtAnte.apoint = ftAnte.apoint;
            mtAnteNullInds[MtAnte.APOINT] = ftAnteNullInds[FtAnte.APOINT];

            mtAnte.sdate = ftAnte.sdate;
            mtAnteNullInds[MtAnte.SDATE] = ftAnteNullInds[FtAnte.SDATE];

            mtAnte.licence = ftAnte.licence;
            mtAnteNullInds[MtAnte.LICENCE] = ftAnteNullInds[FtAnte.LICENCE];

            mtAnte.mdate = ftAnte.mdate;
            mtAnteNullInds[MtAnte.MDATE] = ftAnteNullInds[FtAnte.MDATE];

            mtAnte.mtime = ftAnte.mtime;
            mtAnteNullInds[MtAnte.MTIME] = ftAnteNullInds[FtAnte.MTIME];

            // Handle the special cases.
            mtAnte.userid = "";
            mtAnteNullInds[MtAnte.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method instantiates an MtSite object that is populated with member values
        /// from a prescribed FtSite object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="ftSite"> - input FtSite object.</param>
        /// <param name="ftSiteNullInds"> - input array of ftSite null indicators.</param>
        /// <param name="mtSite"> - output populated MtSite object.</param>
        /// <param name="mtSiteNullInds"> - output array of MtSite null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MtSiteFromFtSite(FtSite ftSite, SQLLEN[] ftSiteNullInds, out MtSite mtSite, out SQLLEN[] mtSiteNullInds)
        {
            // 'out' requirement.
            mtSite = null;
            mtSiteNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: ftSite and ftSiteNullInds.
            if (ftSite == null)
            {
                errorCode = -1;
            }
            else if (ftSiteNullInds == null)
            {
                errorCode = -2;
            }
            else if (ftSiteNullInds.Length != FtSite.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MtSiteFromFtSite(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            mtSite = new MtSite();
            mtSiteNullInds = NullHelper.CreateArrayOfNullInd(MtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.

            mtSite.call1 = ftSite.call1;
            mtSiteNullInds[MtSite.CALL1] = ftSiteNullInds[FtSite.CALL1];

            mtSite.name = ftSite.name;
            mtSiteNullInds[MtSite.NAME] = ftSiteNullInds[FtSite.NAME];

            mtSite.prov = ftSite.prov;
            mtSiteNullInds[MtSite.PROV] = ftSiteNullInds[FtSite.PROV];

            mtSite.oper = ftSite.oper;
            mtSiteNullInds[MtSite.OPER] = ftSiteNullInds[FtSite.OPER];

            mtSite.latit = ftSite.latit;
            mtSiteNullInds[MtSite.LATIT] = ftSiteNullInds[FtSite.LATIT];


            mtSite.longit = ftSite.longit;
            mtSiteNullInds[MtSite.LONGIT] = ftSiteNullInds[FtSite.LONGIT];

            mtSite.grnd = ftSite.grnd;
            mtSiteNullInds[MtSite.GRND] = ftSiteNullInds[FtSite.GRND];

            mtSite.stats = ftSite.stats;
            mtSiteNullInds[MtSite.STATS] = ftSiteNullInds[FtSite.STATS];

            mtSite.sdate = ftSite.sdate;
            mtSiteNullInds[MtSite.SDATE] = ftSiteNullInds[FtSite.SDATE];

            mtSite.loc = ftSite.loc;
            mtSiteNullInds[MtSite.LOC] = ftSiteNullInds[FtSite.LOC];

            mtSite.icaccount = ftSite.icaccount;
            mtSiteNullInds[MtSite.ICACCOUNT] = ftSiteNullInds[FtSite.ICACCOUNT];

            mtSite.reg = ftSite.reg;
            mtSiteNullInds[MtSite.REG] = ftSiteNullInds[FtSite.REG];

            mtSite.spoint = ftSite.spoint;
            mtSiteNullInds[MtSite.SPOINT] = ftSiteNullInds[FtSite.SPOINT];

            mtSite.nots = ftSite.nots;
            mtSiteNullInds[MtSite.NOTS] = ftSiteNullInds[FtSite.NOTS];

            mtSite.oprtyp = ftSite.oprtyp;
            mtSiteNullInds[MtSite.OPRTYP] = ftSiteNullInds[FtSite.OPRTYP];

            mtSite.snumb = ftSite.snumb;
            mtSiteNullInds[MtSite.SNUMB] = ftSiteNullInds[FtSite.SNUMB];

            mtSite.notwr = ftSite.notwr;
            mtSiteNullInds[MtSite.NOTWR] = ftSiteNullInds[FtSite.NOTWR];

            mtSite.bandwd1 = ftSite.bandwd1;
            mtSiteNullInds[MtSite.BANDWD1] = ftSiteNullInds[FtSite.BANDWD1];

            mtSite.bandwd2 = ftSite.bandwd2;
            mtSiteNullInds[MtSite.BANDWD2] = ftSiteNullInds[FtSite.BANDWD2];

            mtSite.bandwd3 = ftSite.bandwd3;
            mtSiteNullInds[MtSite.BANDWD3] = ftSiteNullInds[FtSite.BANDWD3];

            mtSite.bandwd4 = ftSite.bandwd4;
            mtSiteNullInds[MtSite.BANDWD4] = ftSiteNullInds[FtSite.BANDWD4];

            mtSite.bandwd5 = ftSite.bandwd5;
            mtSiteNullInds[MtSite.BANDWD5] = ftSiteNullInds[FtSite.BANDWD5];

            mtSite.bandwd6 = ftSite.bandwd6;
            mtSiteNullInds[MtSite.BANDWD6] = ftSiteNullInds[FtSite.BANDWD6];

            mtSite.bandwd7 = ftSite.bandwd7;
            mtSiteNullInds[MtSite.BANDWD7] = ftSiteNullInds[FtSite.BANDWD7];

            mtSite.bandwd8 = ftSite.bandwd8;
            mtSiteNullInds[MtSite.BANDWD8] = ftSiteNullInds[FtSite.BANDWD8];

            mtSite.mdate = ftSite.mdate;
            mtSiteNullInds[MtSite.MDATE] = ftSiteNullInds[FtSite.MDATE];

            mtSite.mtime = ftSite.mtime;
            mtSiteNullInds[MtSite.MTIME] = ftSiteNullInds[FtSite.MTIME];

            // Handle the special cases.

            mtSite.strlatit = "";
            mtSiteNullInds[MtSite.STRLATIT] = Constant.DB_NULL;

            mtSite.strlatits = "";
            mtSiteNullInds[MtSite.STRLATITS] = Constant.DB_NULL;

            mtSite.strlongit = "";
            mtSiteNullInds[MtSite.STRLONGIT] = Constant.DB_NULL;

            mtSite.strlongits = "";
            mtSiteNullInds[MtSite.STRLONGITS] = Constant.DB_NULL;

            mtSite.userid = "";
            mtSiteNullInds[MtSite.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }





    }
}


```
