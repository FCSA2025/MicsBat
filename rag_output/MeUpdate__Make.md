# Documented File: Make.cs
**Repository Path:** `MeUpdate\Make.cs`
**Primary Layer:** `MeUpdate`
**Namespace:** `MeUpdate`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeUpdate
{
    using _Configuration;
    using _NewLib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that copy member values from FeXxxx objects to
    /// MeXxxx objects, e.g. populate an MeSite object using values in a FeSite object.
    /// </summary>
    public class Make
    {

        /// <summary>
        /// This method instantiates an MeChan object that is populated with member values
        /// from a prescribed FeChan object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="feChan"> - input FeChan object.</param>
        /// <param name="feChanNullInds"> - input array of feChan null indicators.</param>
        /// <param name="meChan"> - output populated MeChan object.</param>
        /// <param name="meChanNullInds"> - output array of MeChan null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MeChanFromFeChan(FeChan feChan, SQLLEN[] feChanNullInds, out MeChan meChan, out SQLLEN[] meChanNullInds)
        {
            // 'out' requirement.
            meChan = null;
            meChanNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: feChan and feChanNullInds.
            if (feChan == null)
            {
                errorCode = -1;
            }
            else if (feChanNullInds == null)
            {
                errorCode = -2;
            }
            else if (feChanNullInds.Length != FeChan.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MeChanFromFeChan(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            meChan = new MeChan();
            meChanNullInds = NullHelper.CreateArrayOfNullInd(MeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.

            meChan.location = feChan.location;
            meChanNullInds[MeChan.LOCATION] = feChanNullInds[FeChan.LOCATION];

            meChan.call1 = feChan.call1;
            meChanNullInds[MeChan.CALL1] = feChanNullInds[FeChan.CALL1];

            meChan.chid = feChan.chid;
            meChanNullInds[MeChan.CHID] = feChanNullInds[FeChan.CHID];

            meChan.freqtx = feChan.freqtx;
            meChanNullInds[MeChan.FREQTX] = feChanNullInds[FeChan.FREQTX];

            meChan.poltx = feChan.poltx;
            meChanNullInds[MeChan.POLTX] = feChanNullInds[FeChan.POLTX];

            meChan.maxtxpower = feChan.maxtxpower;
            meChanNullInds[MeChan.MAXTXPOWER] = feChanNullInds[FeChan.MAXTXPOWER];

            meChan.pwrtx = feChan.pwrtx;
            meChanNullInds[MeChan.PWRTX] = feChanNullInds[FeChan.PWRTX];

            meChan.p4khz = feChan.p4khz;
            meChanNullInds[MeChan.P4KHZ] = feChanNullInds[FeChan.P4KHZ];

            meChan.eqpttx = feChan.eqpttx;
            meChanNullInds[MeChan.EQPTTX] = feChanNullInds[FeChan.EQPTTX];

            meChan.traftx = feChan.traftx;
            meChanNullInds[MeChan.TRAFTX] = feChanNullInds[FeChan.TRAFTX];

            meChan.stattx = feChan.stattx;
            meChanNullInds[MeChan.STATTX] = feChanNullInds[FeChan.STATTX];

            meChan.feetx = feChan.feetx;
            meChanNullInds[MeChan.FEETX] = feChanNullInds[FeChan.FEETX];

            meChan.freqrx = feChan.freqrx;
            meChanNullInds[MeChan.FREQRX] = feChanNullInds[FeChan.FREQRX];

            meChan.polrx = feChan.polrx;
            meChanNullInds[MeChan.POLRX] = feChanNullInds[FeChan.POLRX];

            meChan.pwrrx = feChan.pwrrx;
            meChanNullInds[MeChan.PWRRX] = feChanNullInds[FeChan.PWRRX];

            meChan.eqptrx = feChan.eqptrx;
            meChanNullInds[MeChan.EQPTRX] = feChanNullInds[FeChan.EQPTRX];

            meChan.trafrx = feChan.trafrx;
            meChanNullInds[MeChan.TRAFRX] = feChanNullInds[FeChan.TRAFRX];

            meChan.statrx = feChan.statrx;
            meChanNullInds[MeChan.STATRX] = feChanNullInds[FeChan.STATRX];

            meChan.i20 = feChan.i20;
            meChanNullInds[MeChan.I20] = feChanNullInds[FeChan.I20];

            meChan.it01 = feChan.it01;
            meChanNullInds[MeChan.IT01] = feChanNullInds[FeChan.IT01];

            meChan.ip01 = feChan.ip01;
            meChanNullInds[MeChan.IP01] = feChanNullInds[FeChan.IP01];

            meChan.feerx = feChan.feerx;
            meChanNullInds[MeChan.FEERX] = feChanNullInds[FeChan.FEERX];

            meChan.notc = feChan.notc;
            meChanNullInds[MeChan.NOTC] = feChanNullInds[FeChan.NOTC];

            meChan.srvctx = feChan.srvctx;
            meChanNullInds[MeChan.SRVCTX] = feChanNullInds[FeChan.SRVCTX];

            meChan.srvcrx = feChan.srvcrx;
            meChanNullInds[MeChan.SRVCRX] = feChanNullInds[FeChan.SRVCRX];

            meChan.mdate = feChan.mdate;
            meChanNullInds[MeChan.MDATE] = feChanNullInds[FeChan.MDATE];

            meChan.mtime = feChan.mtime;
            meChanNullInds[MeChan.MTIME] = feChanNullInds[FeChan.MTIME];

            // Handle the special cases.
            meChan.userid = "";
            meChanNullInds[MeChan.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method instantiates an MeAnte object that is populated with member values
        /// from a prescribed FeAnte object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="feAnte"> - input FeAnte object.</param>
        /// <param name="feAnteNullInds"> - input array of feAnte null indicators.</param>
        /// <param name="meAnte"> - output populated MeAnte object.</param>
        /// <param name="meAnteNullInds"> - output array of MeAnte null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MeAnteFromFeAnte(FeAnte feAnte, SQLLEN[] feAnteNullInds, out MeAnte meAnte, out SQLLEN[] meAnteNullInds)
        {
            // 'out' requirement.
            meAnte = null;
            meAnteNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: feAnte and feAnteNullInds.
            if (feAnte == null)
            {
                errorCode = -1;
            }
            else if (feAnteNullInds == null)
            {
                errorCode = -2;
            }
            else if (feAnteNullInds.Length != FeAnte.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MeAnteFromFeAnte(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            meAnte = new MeAnte();
            meAnteNullInds = NullHelper.CreateArrayOfNullInd(MeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.

            meAnte.location = feAnte.location;
            meAnteNullInds[MeAnte.LOCATION] = feAnteNullInds[FeAnte.LOCATION];

            meAnte.call1 = feAnte.call1;
            meAnteNullInds[MeAnte.CALL1] = feAnteNullInds[FeAnte.CALL1];

            meAnte.txband = feAnte.txband;
            meAnteNullInds[MeAnte.TXBAND] = feAnteNullInds[FeAnte.TXBAND];

            meAnte.rxband = feAnte.rxband;
            meAnteNullInds[MeAnte.RXBAND] = feAnteNullInds[FeAnte.RXBAND];

            meAnte.acodetx = feAnte.acodetx;
            meAnteNullInds[MeAnte.ACODETX] = feAnteNullInds[FeAnte.ACODETX];

            meAnte.acoderx = feAnte.acoderx;
            meAnteNullInds[MeAnte.ACODERX] = feAnteNullInds[FeAnte.ACODERX];

            meAnte.g_t = feAnte.g_t;
            meAnteNullInds[MeAnte.G_T] = feAnteNullInds[FeAnte.G_T];

            meAnte.lnat = feAnte.lnat;
            meAnteNullInds[MeAnte.LNAT] = feAnteNullInds[FeAnte.LNAT];

            meAnte.aht = feAnte.aht;
            meAnteNullInds[MeAnte.AHT] = feAnteNullInds[FeAnte.AHT];

            meAnte.afslt = feAnte.afslt;
            meAnteNullInds[MeAnte.AFSLT] = feAnteNullInds[FeAnte.AFSLT];

            meAnte.afslr = feAnte.afslr;
            meAnteNullInds[MeAnte.AFSLR] = feAnteNullInds[FeAnte.AFSLR];

            meAnte.txhgmax = feAnte.txhgmax;
            meAnteNullInds[MeAnte.TXHGMAX] = feAnteNullInds[FeAnte.TXHGMAX];

            meAnte.rxhgmax = feAnte.rxhgmax;
            meAnteNullInds[MeAnte.RXHGMAX] = feAnteNullInds[FeAnte.RXHGMAX];

            meAnte.satlongit = feAnte.satlongit;
            meAnteNullInds[MeAnte.SATLONGIT] = feAnteNullInds[FeAnte.SATLONGIT];

            meAnte.satlong = feAnte.satlong;
            meAnteNullInds[MeAnte.SATLONG] = feAnteNullInds[FeAnte.SATLONG];

            meAnte.satlongs = feAnte.satlongs;
            meAnteNullInds[MeAnte.SATLONGS] = feAnteNullInds[FeAnte.SATLONGS];

            meAnte.az = feAnte.az;
            meAnteNullInds[MeAnte.AZ] = feAnteNullInds[FeAnte.AZ];

            meAnte.el = feAnte.el;
            meAnteNullInds[MeAnte.EL] = feAnteNullInds[FeAnte.EL];

            meAnte.sarc1 = feAnte.sarc1;
            meAnteNullInds[MeAnte.SARC1] = feAnteNullInds[FeAnte.SARC1];

            meAnte.sarc2 = feAnte.sarc2;
            meAnteNullInds[MeAnte.SARC2] = feAnteNullInds[FeAnte.SARC2];

            meAnte.rxpre = feAnte.rxpre;
            meAnteNullInds[MeAnte.RXPRE] = feAnteNullInds[FeAnte.RXPRE];

            meAnte.txpre = feAnte.txpre;
            meAnteNullInds[MeAnte.TXPRE] = feAnteNullInds[FeAnte.TXPRE];

            meAnte.rxtro = feAnte.rxtro;
            meAnteNullInds[MeAnte.RXTRO] = feAnteNullInds[FeAnte.RXTRO];

            meAnte.txtro = feAnte.txtro;
            meAnteNullInds[MeAnte.TXTRO] = feAnteNullInds[FeAnte.TXTRO];

            meAnte.licence = feAnte.licence;
            meAnteNullInds[MeAnte.LICENCE] = feAnteNullInds[FeAnte.LICENCE];

            meAnte.satname = feAnte.satname;
            meAnteNullInds[MeAnte.SATNAME] = feAnteNullInds[FeAnte.SATNAME];

            meAnte.stata = feAnte.stata;
            meAnteNullInds[MeAnte.STATA] = feAnteNullInds[FeAnte.STATA];

            meAnte.nota = feAnte.nota;
            meAnteNullInds[MeAnte.NOTA] = feAnteNullInds[FeAnte.NOTA];

            meAnte.op2 = feAnte.op2;
            meAnteNullInds[MeAnte.OP2] = feAnteNullInds[FeAnte.OP2];

            meAnte.antref = feAnte.antref;
            meAnteNullInds[MeAnte.ANTREF] = feAnteNullInds[FeAnte.ANTREF];

            meAnte.orbit = feAnte.orbit;
            meAnteNullInds[MeAnte.ORBIT] = feAnteNullInds[FeAnte.ORBIT];

            meAnte.mdate = feAnte.mdate;
            meAnteNullInds[MeAnte.MDATE] = feAnteNullInds[FeAnte.MDATE];

            meAnte.mtime = feAnte.mtime;
            meAnteNullInds[MeAnte.MTIME] = feAnteNullInds[FeAnte.MTIME];

            // Handle the special cases.
            meAnte.userid = "";
            meAnteNullInds[MeAnte.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method instantiates an MeSite object that is populated with member values
        /// from a prescribed FeSite object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="feSite"> - input FeSite object.</param>
        /// <param name="feSiteNullInds"> - input array of feSite null indicators.</param>
        /// <param name="meSite"> - output populated MeSite object.</param>
        /// <param name="meSiteNullInds"> - output array of MeSite null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MeSiteFromFeSite(FeSite feSite, SQLLEN[] feSiteNullInds, out MeSite meSite, out SQLLEN[] meSiteNullInds)
        {
            // 'out' requirement.
            meSite = null;
            meSiteNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: feSite and feSiteNullInds.
            if (feSite == null)
            {
                errorCode = -1;
            }
            else if (feSiteNullInds == null)
            {
                errorCode = -2;
            }
            else if (feSiteNullInds.Length != FeSite.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MeSiteFromFeSite(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            meSite = new MeSite();
            meSiteNullInds = NullHelper.CreateArrayOfNullInd(MeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.

            meSite.location = feSite.location;
            meSiteNullInds[MeSite.LOCATION] = feSiteNullInds[FeSite.LOCATION];

            meSite.name = feSite.name;
            meSiteNullInds[MeSite.NAME] = feSiteNullInds[FeSite.NAME];

            meSite.prov = feSite.prov;
            meSiteNullInds[MeSite.PROV] = feSiteNullInds[FeSite.PROV];

            meSite.oper = feSite.oper;
            meSiteNullInds[MeSite.OPER] = feSiteNullInds[FeSite.OPER];

            meSite.latit = feSite.latit;
            meSiteNullInds[MeSite.LATIT] = feSiteNullInds[FeSite.LATIT];

            meSite.strlatit = "";
            meSiteNullInds[MeSite.STRLATIT] = Constant.DB_NULL;

            meSite.strlatits = "";
            meSiteNullInds[MeSite.STRLATITS] = Constant.DB_NULL;

            meSite.longit = feSite.longit;
            meSiteNullInds[MeSite.LONGIT] = feSiteNullInds[FeSite.LONGIT];

            meSite.strlongit = "";
            meSiteNullInds[MeSite.STRLONGIT] = Constant.DB_NULL;

            meSite.strlongits = "";
            meSiteNullInds[MeSite.STRLONGITS] = Constant.DB_NULL;

            meSite.grnd = feSite.grnd;
            meSiteNullInds[MeSite.GRND] = feSiteNullInds[FeSite.GRND];

            meSite.radio = feSite.radio;
            meSiteNullInds[MeSite.RADIO] = feSiteNullInds[FeSite.RADIO];

            meSite.rain = feSite.rain;
            meSiteNullInds[MeSite.RAIN] = feSiteNullInds[FeSite.RAIN];

            meSite.sdate = feSite.sdate;
            meSiteNullInds[MeSite.SDATE] = feSiteNullInds[FeSite.SDATE];

            meSite.stats = feSite.stats;
            meSiteNullInds[MeSite.STATS] = feSiteNullInds[FeSite.STATS];

            meSite.nots = feSite.nots;
            meSiteNullInds[MeSite.NOTS] = feSiteNullInds[FeSite.NOTS];

            meSite.oprtyp = feSite.oprtyp;
            meSiteNullInds[MeSite.OPRTYP] = feSiteNullInds[FeSite.OPRTYP];

            meSite.reg = feSite.reg;
            meSiteNullInds[MeSite.REG] = feSiteNullInds[FeSite.REG];

            meSite.mdate = feSite.mdate;
            meSiteNullInds[MeSite.MDATE] = feSiteNullInds[FeSite.MDATE];

            meSite.mtime = feSite.mtime;
            meSiteNullInds[MeSite.MTIME] = feSiteNullInds[FeSite.MTIME];

            meSite.userid = "";
            meSiteNullInds[MeSite.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method instantiates an MeAzim object that is populated with member values
        /// from a prescribed FeAzim object; similarly for the ODBC null indicators; the
        /// member 'userid" is set to DB_NULL.
        /// </summary>
        /// <param name="feAzim"> - input FeAzim object.</param>
        /// <param name="feAzimNullInds"> - input array of feAzim null indicators.</param>
        /// <param name="meAzim"> - output populated MeAzim object.</param>
        /// <param name="meAzimNullInds"> - output array of MeAzim null indicators.</param>
        /// <returns>0 if successful, otherwise negative.</returns>
        public static int MeAzimFromFeAzim(FeAzim feAzim, SQLLEN[] feAzimNullInds, out MeAzim meAzim, out SQLLEN[] meAzimNullInds)
        {
            // 'out' requirement.
            meAzim = null;
            meAzimNullInds = null;

            int errorCode = Constant.SUCCESS;

            // Sanity check the input: feAzim and feAzimNullInds.
            if (feAzim == null)
            {
                errorCode = -1;
            }
            else if (feAzimNullInds == null)
            {
                errorCode = -2;
            }
            else if (feAzimNullInds.Length != FeAzim.NUM_COLUMNS)
            {
                errorCode = -3;
            }

            // If this is a nonsense call then return the status code.
            if (errorCode != Constant.SUCCESS)
            {
                Log2.e("\nMake.MeAzimFromFeAzim(): ERROR: problem with input objects, errorCode = " + errorCode);
                return errorCode;
            }

            // Instantiate the 'out' objects.
            meAzim = new MeAzim();
            meAzimNullInds = NullHelper.CreateArrayOfNullInd(MeAzim.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Copy values, member-by-member.
            meAzim.location = feAzim.location;
            meAzimNullInds[MeAzim.LOCATION] = feAzimNullInds[FeAzim.LOCATION];

            meAzim.call1 = feAzim.call1;
            meAzimNullInds[MeAzim.CALL1] = feAzimNullInds[FeAzim.CALL1];

            meAzim.azim = feAzim.azim;
            meAzimNullInds[MeAzim.AZIM] = feAzimNullInds[FeAzim.AZIM];

            meAzim.elev = feAzim.elev;
            meAzimNullInds[MeAzim.ELEV] = feAzimNullInds[FeAzim.ELEV];

            meAzim.dist = feAzim.dist;
            meAzimNullInds[MeAzim.DIST] = feAzimNullInds[FeAzim.DIST];

            meAzim.loss = feAzim.loss;
            meAzimNullInds[MeAzim.LOSS] = feAzimNullInds[FeAzim.LOSS];

            meAzim.mdate = feAzim.mdate;
            meAzimNullInds[MeAzim.MDATE] = feAzimNullInds[FeAzim.MDATE];

            meAzim.mtime = feAzim.mtime;
            meAzimNullInds[MeAzim.MTIME] = feAzimNullInds[FeAzim.MTIME];


            // Handle the special cases.
            meAzim.userid = "";
            meAzimNullInds[MeAzim.USERID] = Constant.DB_NULL;

            return Constant.SUCCESS;
        }



    }
}

```
