# Documented File: FeValCopy.cs
**Repository Path:** `_Utillib\FeValCopy.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using SQLLEN = Int64;

    public class FeValCopy
    {

        /// <summary>
        /// To copy a site record from the MDB to the PDF.  
        /// </summary>
        /// <remarks>
        /// This is a fairly straight forward routine in that if the field is not null 
        /// in the MDB then it is copied to the PDF information record.  
        /// </remarks>
        /// <param name="feSite"> - pdf site information record</param>
        /// <param name="meSite"> - mdb site information record</param>
        /// <param name="feNulls"> - array of null flags for pdf record</param>
        /// <param name="meNulls"> - array of null flags for mdb record</param>
        public static void FeCopySite(ref FeSite feSite, /* pdf site information record */
                                        MeSite meSite, /* mdb site information record */
                                        ref SQLLEN[] feNulls,        /* array of null flags for pdf record */
                                        SQLLEN[] meNulls)        /* array of null flags for mdb record */
        {
            feSite.location = meSite.location;
            feNulls[FeSite.LOCATION] = meNulls[MeSite.LOCATION];

            feSite.name = meSite.name;
            feNulls[FeSite.NAME] = meNulls[MeSite.NAME];

            feSite.prov = meSite.prov;
            feNulls[FeSite.PROV] = meNulls[MeSite.PROV];

            feSite.oper = meSite.oper;
            feNulls[FeSite.OPER] = meNulls[MeSite.OPER];

            feSite.latit = meSite.latit;
            feNulls[FeSite.LATIT] = meNulls[MeSite.LATIT];

            feSite.longit = meSite.longit;
            feNulls[FeSite.LONGIT] = meNulls[MeSite.LONGIT];

            feSite.grnd = meSite.grnd;
            feNulls[FeSite.GRND] = meNulls[MeSite.GRND];

            feSite.radio = meSite.radio;
            feNulls[FeSite.RADIO] = meNulls[MeSite.RADIO];

            feSite.rain = meSite.rain;
            feNulls[FeSite.RAIN] = meNulls[MeSite.RAIN];

            feSite.sdate = meSite.sdate;
            feNulls[FeSite.SDATE] = meNulls[MeSite.SDATE];

            feSite.stats = meSite.stats;
            feNulls[FeSite.STATS] = meNulls[MeSite.STATS];

            feSite.nots = meSite.nots;
            feNulls[FeSite.NOTS] = meNulls[MeSite.NOTS];

            feSite.oprtyp = meSite.oprtyp;
            feNulls[FeSite.OPRTYP] = meNulls[MeSite.OPRTYP];

            feSite.reg = meSite.reg;
            feNulls[FeSite.REG] = meNulls[MeSite.REG];

            feSite.mdate = meSite.mdate;
            feNulls[FeSite.MDATE] = meNulls[MeSite.MDATE];

            feSite.mtime = meSite.mtime;
            feNulls[FeSite.MTIME] = meNulls[MeSite.MTIME];
        }

        /// <summary>
        /// To copy a antenna record from the MDB to the PDF.  
        /// </summary>
        /// <remarks>
        /// This is a fairly straight forward routine in that if the field is not null 
        /// in the MDB then it is copied to the PDF information record.  
        /// </remarks>
        /// <param name="feAnte"> - pdf ante information record</param>
        /// <param name="meAnte"> - mdb ante information record</param>
        /// <param name="feNulls"> - array of null flags for pdf record</param>
        /// <param name="meNulls"> - array of null flags for mdb record</param>
        public static void FeCopyAnte(ref FeAnte feAnte, /* pdf ante information record */
                                        MeAnte meAnte, /* mdb ante information record */
                                        ref SQLLEN[] feNulls, /* array of null flags for pdf record */
                                        SQLLEN[] meNulls) /* array of null flags for mdb record */
        {
            feAnte.location = meAnte.location;
            feNulls[FeAnte.LOCATION] = meNulls[MeAnte.LOCATION];

            feAnte.call1 = meAnte.call1;
            feNulls[FeAnte.CALL1] = meNulls[MeAnte.CALL1];

            feAnte.txband = meAnte.txband;
            feNulls[FeAnte.TXBAND] = meNulls[MeAnte.TXBAND];

            feAnte.rxband = meAnte.rxband;
            feNulls[FeAnte.RXBAND] = meNulls[MeAnte.RXBAND];

            feAnte.acodetx = meAnte.acodetx;
            feNulls[FeAnte.ACODETX] = meNulls[MeAnte.ACODETX];

            feAnte.acoderx = meAnte.acoderx;
            feNulls[FeAnte.ACODERX] = meNulls[MeAnte.ACODERX];

            feAnte.g_t = meAnte.g_t;
            feNulls[FeAnte.G_T] = meNulls[MeAnte.G_T];

            feAnte.lnat = meAnte.lnat;
            feNulls[FeAnte.LNAT] = meNulls[MeAnte.LNAT];

            feAnte.aht = meAnte.aht;
            feNulls[FeAnte.AHT] = meNulls[MeAnte.AHT];

            feAnte.afslt = meAnte.afslt;
            feNulls[FeAnte.AFSLT] = meNulls[MeAnte.AFSLT];

            feAnte.afslr = meAnte.afslr;
            feNulls[FeAnte.AFSLR] = meNulls[MeAnte.AFSLR];

            feAnte.txhgmax = meAnte.txhgmax;
            feNulls[FeAnte.TXHGMAX] = meNulls[MeAnte.TXHGMAX];

            feAnte.rxhgmax = meAnte.rxhgmax;
            feNulls[FeAnte.RXHGMAX] = meNulls[MeAnte.RXHGMAX];

            feAnte.satlongit = meAnte.satlongit;
            feNulls[FeAnte.SATLONGIT] = meNulls[MeAnte.SATLONGIT];

            feAnte.satlong = meAnte.satlong;
            feNulls[FeAnte.SATLONG] = meNulls[MeAnte.SATLONG];

            feAnte.satlongs = meAnte.satlongs;
            feNulls[FeAnte.SATLONGS] = meNulls[MeAnte.SATLONGS];

            feAnte.az = meAnte.az;
            feNulls[FeAnte.AZ] = meNulls[MeAnte.AZ];

            feAnte.el = meAnte.el;
            feNulls[FeAnte.EL] = meNulls[MeAnte.EL];

            feAnte.sarc1 = meAnte.sarc1;
            feNulls[FeAnte.SARC1] = meNulls[MeAnte.SARC1];

            feAnte.sarc2 = meAnte.sarc2;
            feNulls[FeAnte.SARC2] = meNulls[MeAnte.SARC2];

            feAnte.rxpre = meAnte.rxpre;
            feNulls[FeAnte.RXPRE] = meNulls[MeAnte.RXPRE];

            feAnte.txpre = meAnte.txpre;
            feNulls[FeAnte.TXPRE] = meNulls[MeAnte.TXPRE];

            feAnte.rxtro = meAnte.rxtro;
            feNulls[FeAnte.RXTRO] = meNulls[MeAnte.RXTRO];

            feAnte.txtro = meAnte.txtro;
            feNulls[FeAnte.TXTRO] = meNulls[MeAnte.TXTRO];

            feAnte.licence = meAnte.licence;
            feNulls[FeAnte.LICENCE] = meNulls[MeAnte.LICENCE];

            feAnte.satname = meAnte.satname;
            feNulls[FeAnte.SATNAME] = meNulls[MeAnte.SATNAME];

            feAnte.stata = meAnte.stata;
            feNulls[FeAnte.STATA] = meNulls[MeAnte.STATA];

            feAnte.nota = meAnte.nota;
            feNulls[FeAnte.NOTA] = meNulls[MeAnte.NOTA];

            feAnte.op2 = meAnte.op2;
            feNulls[FeAnte.OP2] = meNulls[MeAnte.OP2];

            feAnte.antref = meAnte.antref;
            feNulls[FeAnte.ANTREF] = meNulls[MeAnte.ANTREF];

            feAnte.orbit = meAnte.orbit;
            feNulls[FeAnte.ORBIT] = meNulls[MeAnte.ORBIT];

            feAnte.mdate = meAnte.mdate;
            feNulls[FeAnte.MDATE] = meNulls[MeAnte.MDATE];

            feAnte.mtime = meAnte.mtime;
            feNulls[FeAnte.MTIME] = meNulls[MeAnte.MTIME];

        }

        /// <summary>
        /// To copy a channel record from the MDB to the PDF.  
        /// </summary>
        /// <remarks>
        /// This is a fairly straight forward routine in that if the field is not null 
        /// in the MDB then it is copied to the PDF information record.  
        /// </remarks>
        /// <param name="feChan"> - pdf chan information record</param>
        /// <param name="meChan"> - mdb chan information record</param>
        /// <param name="feNulls"> - array of null flags for pdf record</param>
        /// <param name="meNulls"> - array of null flags for mdb record</param>
        public static void FeCopyChan(ref FeChan feChan, MeChan meChan, ref SQLLEN[] feNulls, SQLLEN[] meNulls)
        {
            feChan.location = meChan.location;
            feNulls[FeChan.LOCATION] = meNulls[MeChan.LOCATION];

            feChan.call1 = meChan.call1;
            feNulls[FeChan.CALL1] = meNulls[MeChan.CALL1];

            feChan.chid = meChan.chid;
            feNulls[FeChan.CHID] = meNulls[MeChan.CHID];

            feChan.freqtx = meChan.freqtx;
            feNulls[FeChan.FREQTX] = meNulls[MeChan.FREQTX];

            feChan.poltx = meChan.poltx;
            feNulls[FeChan.POLTX] = meNulls[MeChan.POLTX];

            feChan.maxtxpower = meChan.maxtxpower;
            feNulls[FeChan.MAXTXPOWER] = meNulls[MeChan.MAXTXPOWER];

            feChan.pwrtx = meChan.pwrtx;
            feNulls[FeChan.PWRTX] = meNulls[MeChan.PWRTX];

            feChan.p4khz = meChan.p4khz;
            feNulls[FeChan.P4KHZ] = meNulls[MeChan.P4KHZ];

            feChan.eqpttx = meChan.eqpttx;
            feNulls[FeChan.EQPTTX] = meNulls[MeChan.EQPTTX];

            feChan.traftx = meChan.traftx;
            feNulls[FeChan.TRAFTX] = meNulls[MeChan.TRAFTX];

            feChan.stattx = meChan.stattx;
            feNulls[FeChan.STATTX] = meNulls[MeChan.STATTX];

            feChan.feetx = meChan.feetx;
            feNulls[FeChan.FEETX] = meNulls[MeChan.FEETX];

            feChan.freqrx = meChan.freqrx;
            feNulls[FeChan.FREQRX] = meNulls[MeChan.FREQRX];

            feChan.polrx = meChan.polrx;
            feNulls[FeChan.POLRX] = meNulls[MeChan.POLRX];

            feChan.pwrrx = meChan.pwrrx;
            feNulls[FeChan.PWRRX] = meNulls[MeChan.PWRRX];

            feChan.eqptrx = meChan.eqptrx;
            feNulls[FeChan.EQPTRX] = meNulls[MeChan.EQPTRX];

            feChan.trafrx = meChan.trafrx;
            feNulls[FeChan.TRAFRX] = meNulls[MeChan.TRAFRX];

            feChan.statrx = meChan.statrx;
            feNulls[FeChan.STATRX] = meNulls[MeChan.STATRX];

            feChan.i20 = meChan.i20;
            feNulls[FeChan.I20] = meNulls[MeChan.I20];

            feChan.it01 = meChan.it01;
            feNulls[FeChan.IT01] = meNulls[MeChan.IT01];

            feChan.ip01 = meChan.ip01;
            feNulls[FeChan.IP01] = meNulls[MeChan.IP01];

            feChan.feerx = meChan.feerx;
            feNulls[FeChan.FEERX] = meNulls[MeChan.FEERX];

            feChan.notc = meChan.notc;
            feNulls[FeChan.NOTC] = meNulls[MeChan.NOTC];

            feChan.srvctx = meChan.srvctx;
            feNulls[FeChan.SRVCTX] = meNulls[MeChan.SRVCTX];

            feChan.srvcrx = meChan.srvcrx;
            feNulls[FeChan.SRVCRX] = meNulls[MeChan.SRVCRX];

            feChan.mdate = meChan.mdate;
            feNulls[FeChan.MDATE] = meNulls[MeChan.MDATE];

            feChan.mtime = meChan.mtime;
            feNulls[FeChan.MTIME] = meNulls[MeChan.MTIME];


            return;
        }

        /// <summary>
        /// This method copies the member values of an MeAzim object into the same members
        /// of a FeAzim object.
        /// </summary>
        /// <param name="feAzim"></param>
        /// <param name="meAzim"></param>
        /// <param name="feNulls"></param>
        /// <param name="meNulls"></param>
        public static void FeCopyAzim(ref FeAzim feAzim,           /* pdf site information record */
                                        MeAzim meAzim,         /* mdb site information record */
                                        ref SQLLEN[] feNulls,        /* array of null flags for pdf record */
                                        SQLLEN[] meNulls)        /* array of null flags for mdb record */
        {
            feAzim.location = meAzim.location;
            feNulls[FeAzim.LOCATION] = meNulls[MeAzim.LOCATION];
            feAzim.call1 = meAzim.call1;
            feNulls[FeAzim.CALL1] = meNulls[MeAzim.CALL1];
            feAzim.azim = meAzim.azim;
            feNulls[FeAzim.AZIM] = meNulls[MeAzim.AZIM];
            feAzim.dist = meAzim.dist;
            feNulls[FeAzim.DIST] = meNulls[MeAzim.DIST];
            feAzim.elev = meAzim.elev;
            feNulls[FeAzim.ELEV] = meNulls[MeAzim.ELEV];
            feAzim.loss = meAzim.loss;
            feNulls[FeAzim.LOSS] = meNulls[MeAzim.LOSS];
            feAzim.mdate = meAzim.mdate;
            feNulls[FeAzim.MDATE] = meNulls[MeAzim.MDATE];
            feAzim.mtime = meAzim.mtime;
            feNulls[FeAzim.MTIME] = meNulls[MeAzim.MTIME];
        }


    }
}

```
