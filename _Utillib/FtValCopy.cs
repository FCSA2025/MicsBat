using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _NewLib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;  //Invented to mimic (char *)
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
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
    /// Provides methods to copy data from MtSite, MtAnte and MtChan objects to 
    /// FtSite, FtAnte and FtChan objects, respectively.
    /// </summary>
    public class FtValCopy
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void ftCopySite([In, Out] FtSite ftSite, [In] MtSite mtSite, [In, Out] SQLLEN[] ftNulls, [In] SQLLEN[] mtNulls);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void ftCopyAnte([In, Out] FtAnte ftAnte, [In] MtAnte mtAnte, [In, Out] SQLLEN[] ftNulls, [In] SQLLEN[] mtNulls);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern void ftCopyChan([In, Out] FtChan ftChan, [In] MtChan mtChan, [In, Out] SQLLEN[] ftNulls, [In] SQLLEN[] mtNulls);

        public static void FtCopySite_NATIVE(FtSite ftSite, MtSite mtSite, SQLLEN[] ftNulls, SQLLEN[] mtNulls)
        {
            ftCopySite(ftSite, mtSite, ftNulls, mtNulls);
        }

        public static void FtCopyAnte_NATIVE(ref FtAnte ftAnte, MtAnte mtAnte, ref SQLLEN[] ftNulls, SQLLEN[] mtNulls)
        {
            ftCopyAnte(ftAnte, mtAnte, ftNulls, mtNulls);
        }

        public static void FtCopyChan_NATIVE(ref FtChan ftChan, MtChan mtChan, ref SQLLEN[] ftNulls, SQLLEN[] mtNulls)
        {
            ftCopyChan(ftChan, mtChan, ftNulls, mtNulls);
        }
#endif
        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// Populates an FtSite object with all the field values of a prescribed MtSite object 
        /// (except the fields'cmd' and 'recstat'). The corresponding NullInds are also copied.
        /// </summary>
        /// <param name="ftSite"> - FtSite object to be populated with data.</param>
        /// <param name="mtSite"> - MtSite object prescribed as the data source.</param>
        /// <param name="ftNulls"> - array of ODBC nullInds to be populated for ftSite.</param>
        /// <param name="mtNulls"> - array of ODBC nullInds prescribed for mtSite.</param>
        public static void FtCopySite(ref FtSite ftSite, MtSite mtSite, ref SQLLEN[] ftNulls, SQLLEN[] mtNulls)
        {
            // First copy all the columns values except 'cmd' and 'recstat'.
            ftSite.call1 = mtSite.call1;
            ftSite.name = mtSite.name;
            ftSite.prov = mtSite.prov;
            ftSite.oper = mtSite.oper;
            ftSite.latit = mtSite.latit;
            ftSite.longit = mtSite.longit;
            ftSite.grnd = mtSite.grnd;
            ftSite.stats = mtSite.stats;
            ftSite.sdate = mtSite.sdate;
            ftSite.loc = mtSite.loc;
            ftSite.icaccount = mtSite.icaccount;
            ftSite.reg = mtSite.reg;
            ftSite.spoint = mtSite.spoint;
            ftSite.nots = mtSite.nots;
            ftSite.oprtyp = mtSite.oprtyp;
            ftSite.snumb = mtSite.snumb;
            ftSite.notwr = mtSite.notwr;
            ftSite.bandwd1 = mtSite.bandwd1;
            ftSite.bandwd2 = mtSite.bandwd2;
            ftSite.bandwd3 = mtSite.bandwd3;
            ftSite.bandwd4 = mtSite.bandwd4;
            ftSite.bandwd5 = mtSite.bandwd5;
            ftSite.bandwd6 = mtSite.bandwd6;
            ftSite.bandwd7 = mtSite.bandwd7;
            ftSite.bandwd8 = mtSite.bandwd8;
            ftSite.mdate = mtSite.mdate;
            ftSite.mtime = mtSite.mtime;

            // Finally, copy all the nullInds except those for 'cmd' and 'recstat'.
            ftNulls[FtSite.CALL1] = mtNulls[MtSite.CALL1];
            ftNulls[FtSite.NAME] = mtNulls[MtSite.NAME];
            ftNulls[FtSite.PROV] = mtNulls[MtSite.PROV];
            ftNulls[FtSite.OPER] = mtNulls[MtSite.OPER];
            ftNulls[FtSite.LATIT] = mtNulls[MtSite.LATIT];
            ftNulls[FtSite.LONGIT] = mtNulls[MtSite.LONGIT];
            for (int i = FtSite.GRND; i < FtSite.NUM_COLUMNS; i++)
            {
                ftNulls[i] = mtNulls[i + 2];
            }
        }

        /// <summary>
        /// Populates an FtAnte object with all the field values of a prescribed MtAnte object 
        /// (except the fields'cmd' and 'recstat'). The corresponding NullInds are also copied.
        /// </summary>
        /// <param name="ftAnte"> - FtAnte object to be populated with data.</param>
        /// <param name="mtAnte"> - MtAnte object prescribed as the data source.</param>
        /// <param name="ftNulls"> - array of ODBC nullInds to be populated for ftAnte.</param>
        /// <param name="mtNulls"> - array of ODBC nullInds prescribed for mtAnte.</param>
        public static void FtCopyAnte(ref FtAnte ftAnte, MtAnte mtAnte, ref SQLLEN[] ftNulls, SQLLEN[] mtNulls)
        {
            if (ftAnte == null) Log2.e("\n\nFtValCopy.FtCopyAnte(): ERROR: ftAnte is null.");
            if (mtAnte == null) Log2.e("\n\nFtValCopy.FtCopyAnte(): ERROR: mtAnte is null.");

            // First copy all the columns values except 'cmd' and 'recstat'.
            ftAnte.call1 = mtAnte.call1;
            ftAnte.call2 = mtAnte.call2;
            ftAnte.bndcde = mtAnte.bndcde;
            ftAnte.anum = mtAnte.anum;
            ftAnte.ause = mtAnte.ause;
            ftAnte.acode = mtAnte.acode;
            ftAnte.aht = mtAnte.aht;
            ftAnte.azmth = mtAnte.azmth;
            ftAnte.elvtn = mtAnte.elvtn;
            ftAnte.dist = mtAnte.dist;
            ftAnte.offazm = mtAnte.offazm;
            ftAnte.tazmth = mtAnte.tazmth;
            ftAnte.telvtn = mtAnte.telvtn;
            ftAnte.tgain = mtAnte.tgain;
            ftAnte.txfdlnth = mtAnte.txfdlnth;
            ftAnte.txfdlnlh = mtAnte.txfdlnlh;
            ftAnte.txfdlntv = mtAnte.txfdlntv;
            ftAnte.txfdlnlv = mtAnte.txfdlnlv;
            ftAnte.rxfdlnth = mtAnte.rxfdlnth;
            ftAnte.rxfdlnlh = mtAnte.rxfdlnlh;
            ftAnte.rxfdlntv = mtAnte.rxfdlntv;
            ftAnte.rxfdlnlv = mtAnte.rxfdlnlv;
            ftAnte.txpadpam = mtAnte.txpadpam;
            ftAnte.rxpadlna = mtAnte.rxpadlna;
            ftAnte.txcompl = mtAnte.txcompl;
            ftAnte.rxcompl = mtAnte.rxcompl;
            ftAnte.obsloss = mtAnte.obsloss;
            ftAnte.kvalue = mtAnte.kvalue;
            ftAnte.atwrno = mtAnte.atwrno;
            ftAnte.nota = mtAnte.nota;
            ftAnte.apoint = mtAnte.apoint;
            ftAnte.sdate = mtAnte.sdate;
            ftAnte.licence = mtAnte.licence;
            ftAnte.mdate = mtAnte.mdate;
            ftAnte.mtime = mtAnte.mtime;

            // Finally, copy all the nullInds except those for 'cmd' and 'recstat'.
            // The FtAnte member values 2 to 33 are the same as MtAnte 0 to 31.
            for (int i = FtAnte.CALL1; i < FtAnte.LICENCE; i++)
            {
                ftNulls[i] = mtNulls[i - FtAnte.CALL1];
            }

            // Now deal with the license, mdate and mtime members.
            ftNulls[FtAnte.LICENCE] = mtNulls[MtAnte.LICENCE];
            ftNulls[FtAnte.MDATE] = mtNulls[MtAnte.MDATE];
            ftNulls[FtAnte.MTIME] = mtNulls[MtAnte.MTIME];

        }

        /// <summary>
        /// Populates an FtChan object with all the field values of a prescribed MtChan object 
        /// (except the fields'cmd' and 'recstat'). The corresponding NullInds are also copied.
        /// </summary>
        /// <param name="ftChan"> - FtChan object to be populated with data.</param>
        /// <param name="mtChan"> - MtChan object prescribed as the data source.</param>
        /// <param name="ftNulls"> - array of ODBC nullInds to be populated for ftChan.</param>
        /// <param name="mtNulls"> - array of ODBC nullInds prescribed for mtChan.</param>
        public static void FtCopyChan(ref FtChan ftChan, MtChan mtChan, ref SQLLEN[] ftNulls, SQLLEN[] mtNulls)
        {
            // First copy all the columns values except 'cmd' and 'recstat'.
            ftChan.call1 = mtChan.call1;
            ftChan.call2 = mtChan.call2;
            ftChan.bndcde = mtChan.bndcde;
            ftChan.splan = mtChan.splan;
            ftChan.hl = mtChan.hl;
            ftChan.vh = mtChan.vh;
            ftChan.chid = mtChan.chid;
            ftChan.freqtx = mtChan.freqtx;
            ftChan.poltx = mtChan.poltx;
            ftChan.antnumbtx1 = mtChan.antnumbtx1;
            ftChan.antnumbtx2 = mtChan.antnumbtx2;
            ftChan.eqpttx = mtChan.eqpttx;
            ftChan.eqptutx = mtChan.eqptutx;
            ftChan.pwrtx = mtChan.pwrtx;
            ftChan.atpccde = mtChan.atpccde;
            ftChan.afsltx1 = mtChan.afsltx1;
            ftChan.afsltx2 = mtChan.afsltx2;
            ftChan.traftx = mtChan.traftx;
            ftChan.srvctx = mtChan.srvctx;
            ftChan.stattx = mtChan.stattx;
            ftChan.freqrx = mtChan.freqrx;
            ftChan.polrx = mtChan.polrx;
            ftChan.antnumbrx1 = mtChan.antnumbrx1;
            ftChan.antnumbrx2 = mtChan.antnumbrx2;
            ftChan.antnumbrx3 = mtChan.antnumbrx3;
            ftChan.eqptrx = mtChan.eqptrx;
            ftChan.eqpturx = mtChan.eqpturx;
            ftChan.afslrx1 = mtChan.afslrx1;
            ftChan.afslrx2 = mtChan.afslrx2;
            ftChan.afslrx3 = mtChan.afslrx3;
            ftChan.pwrrx1 = mtChan.pwrrx1;
            ftChan.pwrrx2 = mtChan.pwrrx2;
            ftChan.pwrrx3 = mtChan.pwrrx3;
            ftChan.trafrx = mtChan.trafrx;
            ftChan.esint = mtChan.esint;
            ftChan.tsint = mtChan.tsint;
            ftChan.srvcrx = mtChan.srvcrx;
            ftChan.statrx = mtChan.statrx;
            ftChan.routnumb = mtChan.routnumb;
            ftChan.stnnumb = mtChan.stnnumb;
            ftChan.hopnumb = mtChan.hopnumb;
            ftChan.sdate = mtChan.sdate;
            ftChan.notetx = mtChan.notetx;
            ftChan.noterx = mtChan.noterx;
            ftChan.notegnl = mtChan.notegnl;
            ftChan.cpoint = mtChan.cpoint;
            ftChan.feetx = mtChan.feetx;
            ftChan.feerx = mtChan.feerx;
            ftChan.mdate = mtChan.mdate;
            ftChan.mtime = mtChan.mtime;


            // Finally, copy all the nullInds except those for 'cmd' and 'recstat'.
            for (int i = FtChan.CALL1; i < FtChan.NUM_COLUMNS; i++)
            {
                ftNulls[i] = mtNulls[i - FtChan.CALL1];
            }

        }



    }
}
