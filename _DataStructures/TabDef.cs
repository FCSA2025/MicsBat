using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This method encapsulates the dataset for a table type, table description and
    /// modification description for transformation into a binary-tree format. The method
    /// has a static member Table, of type TabDef[], that builds and serves the definitions 
    /// used to create and modify tables that are dynamically created by MICS on a per 
    /// request basis. These items include ts & es pdf's, TSIP temp tables, sdf's, <i>et. al.</i>
    /// </summary>
    public class TabDef
    {
        public int tabType;            // table type (see tabdefs.h) 
        public string tabDesc;          // ptr to table decription 
        public string modDesc;         // ptr to modification desc for transformation into btree format 

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TabDef()
        {
            this.tabType = 0;
            this.tabDesc = null;
            this.modDesc = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TabDef(int tabType, string tabDesc, string modDesc)
        {
            this.tabType = tabType;
            this.tabDesc = tabDesc;
            this.modDesc = modDesc;
        }

        public static TabDef[] Table = new TabDef[]
        {
            new TabDef(Constant.FT_SHRL, shrlDesc, null),
            new TabDef(Constant.FT_TITL, titlDesc, null),
            new TabDef(Constant.FT_SITE, ftSite, ftSiteMod),
            new TabDef(Constant.FT_ANTE, ftAnte, ftAnteMod),
            new TabDef(Constant.FT_CHAN, ftChan, ftChanMod),
            new TabDef(Constant.FT_CHNG_CALL, ftChngCall, null),
            new TabDef(Constant.FE_TITL, titlDesc, null),
            new TabDef(Constant.FE_SITE, feSite, feSiteMod),
            new TabDef(Constant.FE_SHRL, shrlDesc, null),
            new TabDef(Constant.FE_AZIM, feAzim, feAzimMod),
            new TabDef(Constant.FE_ANTE, feAnte, feAnteMod),
            new TabDef(Constant.FE_CHAN, feChan, feChanMod),
            new TabDef(Constant.FE_CLOC, feChngLoc, null),
            new TabDef(Constant.FE_CCAL, feChngCall, null),
            new TabDef(Constant.FW_CULL, fwCull, null),
            new TabDef(Constant.CT_SITE, ctSite, null),
            new TabDef(Constant.CT_ANTE, ctAnte, null),
            new TabDef(Constant.CT_CHAN, ctChan, null),
            new TabDef(Constant.CT_TEMP, ctTemp, null),
            new TabDef(Constant.CT_RSLT, ctRslt, null),
            new TabDef(Constant.CE_SITE, ceSite, null),
            new TabDef(Constant.CE_ANTE, ceAnte, null),
            new TabDef(Constant.CE_CHAN, ceChan, null),
            new TabDef(Constant.CE_RSLT, ceRslt, null),
            new TabDef(Constant.SC_BAND, sCull, null),
            new TabDef(Constant.SC_ANTE, sCull, null),
            new TabDef(Constant.SC_CTX, sCull, null),
            new TabDef(Constant.SC_EQPT, sCull, null),
            new TabDef(Constant.SC_NOTE, sCull, null),
            new TabDef(Constant.SC_OCOO, sCull, null),
            new TabDef(Constant.SC_OPER, sCull, null),
            new TabDef(Constant.SC_PLAN, sCull, null),
            new TabDef(Constant.SC_ROUT, sCull, null),
            new TabDef(Constant.SC_TOWR, sCull, null),
            new TabDef(Constant.SC_TOWN, sCull, null),
            new TabDef(Constant.SC_TRAF, sCull, null),
            new TabDef(Constant.SU_BAND, sBand, null),
            new TabDef(Constant.SU_ANTE, sAnte, null),
            new TabDef(Constant.SU_ANTD, sAnte1, null),
            new TabDef(Constant.SU_CTX, sCtx, null),
            new TabDef(Constant.SU_CTXD, sCtx1, null),
            new TabDef(Constant.SU_EQPT, sEquip, null),
            new TabDef(Constant.SU_NOTE, sNote, null),
            new TabDef(Constant.SU_OCOO, sCord, null),
            new TabDef(Constant.SU_OPER, sOper, null),
            new TabDef(Constant.SU_PLAN, sPlan, null),
            new TabDef(Constant.SU_PLND, sPlan1, null),
            new TabDef(Constant.SU_ROUT, sRoute, null),
            new TabDef(Constant.SU_TOWR, sTower, null),
            new TabDef(Constant.SU_TOWN, sTowNot, null),
            new TabDef(Constant.SU_TRAF, sTraffic, null),
            new TabDef(Constant.TP_PARM, tpParm, null),
            new TabDef(Constant.PP_PARM, tpParm, null),
            new TabDef(Constant.TT_PARM, tpParm, null),
            new TabDef(Constant.TT_SITE, ttSite, null),
            new TabDef(Constant.TT_ANTE, ttAnte, null),
            new TabDef(Constant.TT_CHAN, ttChan, null),
            new TabDef(Constant.TE_PARM, tpParm, null),
            new TabDef(Constant.TE_SITE, teSite, null),
            new TabDef(Constant.TE_ANTE, teAnte, null),
            new TabDef(Constant.TE_CHAN, teChan, null),
            new TabDef(Constant.TP_SU_ANTE, sAnte, null),
            new TabDef(Constant.TP_SU_ANTD, sAnte1, null),
            new TabDef(Constant.TP_SU_CTX, sCtx, null),
            new TabDef(Constant.TP_SU_CTXD, sCtx1, null),
            new TabDef(Constant.TP_SU_PLAN, sPlan, null),
            new TabDef(Constant.TP_SU_PLND, sPlan1, null),
            new TabDef(Constant.TP_SU_EQPT, sEquip, null),
            new TabDef(Constant.TT_TEMP1, ttTemp1, null),
            new TabDef(Constant.TE_TEMP1, teTemp1, null),
            new TabDef(Constant.TT_TEMP2, ttTemp2, null),
            new TabDef(Constant.BI_USAGE, biUsage, null),
            new TabDef(Constant.SE_PERM_REQ, sePermReq, null),
            new TabDef(Constant.SE_PERM_USER, sePermUser, null),
            new TabDef(Constant.RM_PARAM, rmParam, null),
            new TabDef(Constant.RS_PARAM, rsParam, null),
            new TabDef(Constant.RS_TEMP_PARM, rsTempParm, null),
            new TabDef(Constant.AT_TAB, atTable, null),
            new TabDef(Constant.UT_TABLE_LIST, utTableList, null),
            new TabDef(Constant.RP_TS_FEE_DETAIL, rpTsFeeDetail, null),
            new TabDef(Constant.RP_ES_FEE_DETAIL, rpEsFeeDetail, null),
            new TabDef(Constant.RP_USERDEF, null, null),
            new TabDef(Constant.BI_MICS_MONTH, biMicsTable, null),
            new TabDef(Constant.BI_ULTRIX_MONTH, biUltrixTable, null),
            new TabDef(Constant.BI_MICS_YEAR, biMicsTable, null),
            new TabDef(Constant.BI_ULTRIX_YEAR, biUltrixTable, null),
            new TabDef(Constant.TS_ALPHA_SITE_TABLE, tsAlphaSiteTable, null),
            new TabDef(Constant.TS_ALPHA_ANTE_TABLE, tsAlphaAnteTable, null),
            new TabDef(Constant.TS_ALPHA_CHAN_TABLE, tsAlphaChanTable, null),
            new TabDef(Constant.ES_ALPHA_SITE_TABLE, esAlphaSiteTable, null),
            new TabDef(Constant.ES_ALPHA_ANTE_TABLE, esAlphaAnteTable, null),
            new TabDef(Constant.ES_ALPHA_CHAN_TABLE, esAlphaChanTable, null),
            new TabDef(Constant.TS_ALPHA_TOWN_TABLE, tsAlphaTownTable, null),
            new TabDef(0, null, null)
        };

        //*************************************************************************
        // The following are definitions used by functions in this file to maintain
        // tables which are dynamically created by MICS on a per request basis.
        // These items include ts & es pdf's, tsip temp tables, and sdf's, among
        // others.
        //************************************************************************

        // Shared link approval table 
        const string shrlDesc = "create table {0} (userid char(8) null, mdate char(10) null, mtime char(8) null)";
        // Title table - ES and TS 
        const string titlDesc = "create table {0} (validated char(1) null, namef char(16) null, source char(6) null, descr char(40) null, mdate char(10) null, mtime char(8) null)";
        // TS Site Table 
        const string ftSite = "create table {0} (cmd char(1) collate database_default null, recstat char(1) collate database_default null, call1 char(9) collate database_default not null, name char(32) collate database_default null, prov char(2) collate database_default null, oper char(6) collate database_default null, latit integer null, longit integer null, grnd real null, stats char(1) collate database_default null, sdate char(10) collate database_default null, loc char(25) collate database_default null, icaccount char(12) collate database_default null, reg char(2) collate database_default null, spoint char(4) collate database_default null, nots char(4) collate database_default null, oprtyp char(2) collate database_default null, snumb char(4) collate database_default null, notwr tinyint null, bandwd1 integer null, bandwd2 integer null, bandwd3 integer null, bandwd4 integer null, bandwd5 integer null, bandwd6 integer null, bandwd7 integer null, bandwd8 integer null, mdate char(10) collate database_default null, mtime char(8) collate database_default null,  PRIMARY KEY CLUSTERED (call1))";
        const string ftSiteMod = "modify {0} to btree unique on call1 with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // TS Antenna Table 
        const string ftAnte = "create table {0} (cmd char(1) null, recstat char(1) null, call1 char(9) not null, call2 char(9) not null, bndcde char(4) not null, anum smallint not null, ause char(3) null, acode char(12) null, aht real null, azmth real null, elvtn real null, dist real null, offazm char(1) null, tazmth real null, telvtn real null, tgain real null, txfdlnth char(2) null, txfdlnlh real null, txfdlntv char(2) null, txfdlnlv real null, rxfdlnth char(2) null, rxfdlnlh real null, rxfdlntv char(2) null, rxfdlnlv real null, txpadpam real null, rxpadlna real null, txcompl real null, rxcompl real null, obsloss real null, kvalue real null, atwrno tinyint null, nota char(4) null, apoint char(4) null, sdate char(10) null, licence char(13) NULL, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (call1, call2, bndcde, anum))";
        // TS Ante Modification Table 
        const string ftAnteMod = "modify {0} to btree unique on call1,  call2,  bndcde,  anum with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // TS Channel Table 
        const string ftChan = "create table {0} (cmd char(1) null, recstat char(1) null, call1 char(9) not null, call2 char(9) not null, bndcde char(4) not null, splan char(4) null, hl tinyint null, vh tinyint null, chid char(4) not null, freqtx double precision null, poltx char(1) null, antnumbtx1 tinyint null, antnumbtx2 tinyint null, eqpttx char(8) null, eqptutx char(1) null, pwrtx real null, atpccde real null, afsltx1 real null, afsltx2 real null, traftx char(6) null, srvctx char(6) null, stattx char(1) null, freqrx double precision null, polrx char(1) null, antnumbrx1 tinyint null, antnumbrx2 tinyint null, antnumbrx3 tinyint null, eqptrx char(8) null, eqpturx char(1) null, afslrx1 real null, afslrx2 real null, afslrx3 real null, pwrrx1 real null, pwrrx2 real null, pwrrx3 real null, trafrx char(6) null, esint real null, tsint real null, srvcrx char(6) null, statrx char(1) null, routnumb char(8) null, stnnumb tinyint null, hopnumb tinyint null, sdate char(10) null, notetx char(4) null, noterx char(4) null, notegnl char(4) null, cpoint char(4) null, feetx char(2) null, feerx char(2) null, mdate char(10) null, mtime char(8) null, PRIMARY KEY CLUSTERED(call1, call2, bndcde, chid))";
        // TS Chan Modification Table 
        const string ftChanMod = "modify {0} to btree unique on call1,  call2,  bndcde,  chid with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // TS Change of Call Sign Table 
        const string ftChngCall = "create table {0}(newcall1 char(9) not null, oldcall1 char(9) not null, name char(32) null,  PRIMARY KEY CLUSTERED (newcall1,  oldcall1))";
        // ES Site table 
        const string feSite = "create table {0}(cmd char(1) null, recstat char(1) null, location char(10) not null, name char(16) null, prov char(2) null, oper char(6) null, latit integer null, longit integer null, grnd real null, radio char(2) null, rain smallint null, sdate char(10) null, stats char(1) null, nots char(4) null, oprtyp char(2) null, reg char(2) null, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (location))";
        // ES Site Modification Table 
        const string feSiteMod = "modify {0} to btree unique on location with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // ES Azimuth table 
        const string feAzim = "create table {0}(cmd char(1) null, recstat char(1) null, deleteall char(1) null, location char(10) not null, call1 char(9) not null, azim real not null, elev real null, dist real null, loss real null, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (location,  call1,  azim))";
        // ES Azim Modification Table 
        const string feAzimMod = "modify {0} to btree unique on location,  call1,  azim with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // ES Antenna table 
        const string feAnte = "create table {0}(cmd char(1) null, recstat char(1) null, location char(10) not null, call1 char(9) not null, txband char(4) null, rxband char(4) null, acodetx char(12) null, acoderx char(12) null, g_t real null, lnat real null, aht real null, afslt real null, afslr real null, txhgmax real null, rxhgmax real null, satlongit integer null, satlong real null, satlongs char(1) null, az real null, el real null, sarc1 real null, sarc2 real null, rxpre real null, txpre real null, rxtro real null, txtro real null, licence char(13) null, satname char(16) null, stata char(1) null, nota char(4) null, op2 char(2) null, antref integer null, orbit char(2) null, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (location,  call1))";
        // ES Ante Modification Table 
        const string feAnteMod = "modify {0} to btree unique on location,  call1 with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // ES Channel table 
        const string feChan = "create table {0}(cmd char(1) null, recstat char(1) null, location char(10) not null, call1 char(9) not null, chid char(4) not null, freqtx double precision null, poltx char(1) null, maxtxpower real null, pwrtx real null, p4khz real null, eqpttx char(8) null, traftx char(6) null, stattx char(1) null, feetx char(2) null, freqrx double precision null, polrx char(1) null, pwrrx real null, eqptrx char(8) null, trafrx char(6) null, statrx char(1) null, i20 real null, it01 real null, ip01 real null, feerx char(2) null, notc char(4) null, srvctx char(6) null, srvcrx char(6) null, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (location,  call1,  chid))";
        // ES Chan Modification Table 
        const string feChanMod = "modify {0} to btree unique on location,  call1,  chid with nonleaffill = 80, leaffill = 70, fillfactor = 80";
        // ES Change of location table 
        const string feChngLoc = "create table {0} (newlocation char(10) not null, oldlocation char(10) not null, name char(16) null,  PRIMARY KEY CLUSTERED (newlocation))";
        // ES Change of callsign table 
        const string feChngCall = "create table {0}(newcallsign char(9) null, oldcallsign char(9) null)";
        // Create PDF from MDB temporary site Table - TS 
        const string ctSite = "create table {0}(call1 char(9) null,  procdflag char(1) null)";
        // Create PDF from MDB temporary antenna Table - TS 
        const string ctAnte = "create table {0}(call1 char(9) null,  call2 char(9) null,  bndcde char(4) null,  anum smallint null,  procdflag char(1) null)";
        // Create PDF from MDB temporary channel Table - TS 
        const string ctChan = "create table {0}(call1 char(9) null,  call2 char(9) null,  bndcde char(4) null,  chid char(4) null,  antnumbtx1 tinyint null,  antnumbtx2 tinyint null,  antnumbrx1 tinyint null,  antnumbrx2 tinyint null,  antnumbrx3 tinyint null,  procdflag char(1) null)";
        // Create PDF from MDB temporary route Table - TS 
        const string ctTemp = "create table {0}(call1 char(9) null,  call2 char(9) null,  bndcde char(4) null,  chid char(4) null,  antnumbtx1 tinyint null,  antnumbtx2 tinyint null,  antnumbrx1 tinyint null,  antnumbrx2 tinyint null,  antnumbrx3 tinyint null,  procdflag char(1) null)";
        // Create PDF from MDB temporary results Table - TS 
        const string ctRslt = "create table {0}(call1 char(9) null,  call2 char(9) null,  bndcde char(4) null,  chid char(4) null,  antnumbtx1 tinyint null,  antnumbtx2 tinyint null,  antnumbrx1 tinyint null,  antnumbrx2 tinyint null,  antnumbrx3 tinyint null,  procdflag char(1) null)";
        // Create PDF from MDB temporary site Table - ES 
        const string ceSite = "create table {0}(location char(10) null,  procdflag char(1) null)";
        // Create PDF from MDB temporary antenna Table - ES 
        const string ceAnte = "create table {0}(location char(10) null,  call1 char(9) null,  procdflag char(1) null)";
        // Create PDF from MDB temporary channel Table - ES 
        const string ceChan = "create table {0}(location char(10) null,  call1 char(9) null,  chid char(4) null,  procdflag char(1) null)";
        // Create PDF from MDB temporary results Table - ES 
        const string ceRslt = "create table {0}(location char(10) null,  call1 char(9) null,  chid char(4) null,  procdflag char(1) null)";
        // PDF Cull table - used for both ES and TS 
        const string fwCull = "create table {0}(filename char(16) null, filewtype char(2) null, culltype char(4) null, cullcode char(12) null, cullvalue char(280) null)";
        // SDF Cull parameter table used for all subsidiary tables 
        const string sCull = "create table {0}(filename char(16) null, cullvalue char(280) null)";
        // Subsidary BAND table 
        const string sBand = "create table {0}(cmd char(1) null, recstat char(1) null, bndcde char(4) not null, bandbitpos smallint null, blo double precision null, bmidf double precision null, bhi double precision null, badj char(99) null, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (bndcde))";
        // Subsidary ANTENNA table - modified 02/05/99 GJS to reflect database 
        const string sAnte = "create table {0}(cmd char(1) null, recstat char(1) null, acode char(12) not null, axtype integer null, axref char(12) null, again real null, abw real null, arms tinyint null, aband char(10) null, amanu char(10) null, apattern char(12) null, amodel char(15) null, anip smallint null, ax0 real null, adesc char(20) null, antype char(8) null, aftbr real null, lofreq double precision null, hifreq double precision null, bandcodes char(99) null, mdate char(10) null, mtime char(8) null,  PRIMARY KEY CLUSTERED (acode))";
        // Subsidary ANTENNA 1 table 
        const string sAnte1 = " create table {0}(cmd char(1) null,  acode char(12) not null,  antang real not null,  dcov real null,  dxpv real null,  dcoh real null,  dxph real null,  dtilt real null,  interpstat integer null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (acode,  antang))";
        // Subsidary CTX table 
        const string sCtx = " create table {0}(cmd char(1) null,  recstat char(1) null,  tfcr char(6) not null,  tfci char(6) not null,  rxeqp char(8) not null,  rqco real null,  rqcull real null,  rqwrst real null,  ctxndp smallint null,  ctxdesc char(40) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (tfcr,  tfci,  rxeqp))";
        // Subsidary CTX 1 table 
        const string sCtx1 = " create table {0}(cmd char(1) null,  recstat char(1) null,  tfcr char(6) not null,  tfci char(6) not null,  rxeqp char(8) not null,  fsep double precision not null,  rq real null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (tfcr,  tfci,  rxeqp,  fsep))";
        // Subsidary EQUIPMENT table 
        const string sEquip = " create table {0}(cmd char(1) null,  recstat char(1) null,  ecode char(8) not null,  estab real null,  exref char(8) null,  emanu char(10) null,  emodel char(20) null,  edesc char(32) null,  etype char(2) null,  etraf char(6) null,  emission char(10) null,  e1stif real null,  e2ndif real null,  thhold real null,  ebndcde char(4) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (ecode))";
        // Subsidary NOTES table 
        const string sNote = " create table {0} (cmd char(1) null,  recstat char(1) null,  oper char(6) not null,  nonum char(4) not null,  note char(60) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (oper,  nonum))";
        // Subsidary OPERATING COMPANY CORDINATION table 
        const string sCord = " create table {0}(cmd char(1) null,  recstat char(1) null,  cooper char(6) not null,  cocomp char(40) null,  addr char(50) null,  city char(15) null,  prstat char(2) null,  zippc char(10) null,  dept char(40) null,  namep char(40) null,  phonep char(16) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (cooper))";
        // Subsidary OPERATOR table 
        const string sOper = " create table {0}(cmd char(1) null,  recstat char(1) null,  oper char(6) not null,  nameop char(40) null,  cooper char(6) null,  mdbm char(6) null,  addr char(50) null,  city char(15) null,  prstat char(2) null,  zippc char(10) null,  dept char(40) null,  namep char(40) null,  phonep char(16) null,  faxnum char(16) null,  telecom char(1) null,  opnote char(2) null,  admin char(12) null,  email char(50) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (oper))";
        // Subsidary PLAN table 
        const string sPlan = " create table {0}(cmd char(1) null,  recstat char(1) null,  sband char(4) not null,  splan char(4) not null,  srsp char(10) null,  srspiss char(2) null,  conform char(1) null,  uscan char(1) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (sband,  splan))";
        // Subsidary PLAN 1 table 
        const string sPlan1 = " create table {0}(cmd char(1) null,  recstat char(1) null,  sband char(4) not null,  splan char(4) not null,  spno tinyint not null,  set1 double precision null,  s1chid char(4) null,  set2 double precision null,  s2chid char(4) null,  set3 double precision null,  s3chid char(4) null,  set4 double precision null,  s4chid char(4) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (sband,  splan,  spno))";
        // Subsidary ROUTE table 
        const string sRoute = " create table {0}(cmd char(1) null,  recstat char(1) null,  rcomp char(6) not null,  routnumb char(8) not null, ";
        // Subsidary TOWER table 
        const string sTower = " create table {0}(cmd char(1) null,  recstat char(1) null,  twcode char(4) not null,  twdesc char(60) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (twcode))";
        // Subsidary TOWER NOTES table 
        const string sTowNot = " create table {0}(cmd char(1) null,  recstat char(1) null,  call1 char(9) not null,  oper char(6) not null,  twcode char(4) not null,  twht real null,  atwrno tinyint not null,  twli char(1) null,  twpa char(1) null,  nott char(4) null,  tpoint char(4) null,  adate char(10) null,  sdate char(10) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (call1,  oper,  twcode,  atwrno))";
        // Subsidary TRAFFIC table 
        const string sTraffic = " create table {0}(cmd char(1) null,  recstat char(1) null,  trafcode char(6) not null,  ecode char(8) not null,  xreftrcde char(6) null,  xrefeqcde char(8) null,  trdesc char(30) null,  mdate char(10) null,  mtime char(8) null,  PRIMARY KEY CLUSTERED (trafcode,  ecode))";
        // TSIP TS temporary 1 dimensional table 
        const string ttTemp1 = "create table {0} (call1 char (9) not null,  latit int null,  longit int null,  oper char (6) null )";
        // TSIP ES temporary 1 dimensional table 
        const string teTemp1 = "create table {0}(location char (10) not null ,  latit int null,  longit int null,  oper char (6) null )";
        // TSIP temporary TS 2 dimensional table 
        const string ttTemp2 = "create table {0}(call1 char (9) not null,  call2 char (9) not null)";
        // TSIP TITL table 
        const string tpParm = "create table {0}(protype char (1) null, envtype char (8) null, proname char (16) null, envname char (16) null, tsorbout char (1) null, spherecalc char (1) null, fsep double precision null, coordist double precision null, analopt char (4) null, margin double precision null, numchan smallint null, chancodes char (19) null, tempant char (15) null, tempctx char (15) null, tempplan char (15) null, tempequip char (15) null, country char (3) null, selsites char (15) null, numcodes smallint null, codes char (164) null, runname char (5) null, reports integer null, numcases integer null, numtecases integer null, parmparm char(50) null, mdate char(10) null, mtime char (8) null)";
        // TSIP TS SITE table 
        const string ttSite = "create table {0}(interferer char (1) null, intcall1 char (9) null, intcall2 char (9) null, viccall1 char (9) null, viccall2 char (9) null, caseno integer null, subcases integer null, intname1 char (32) null, intname2 char (32) null, vicname1 char (32) null, vicname2 char (32) null, intoper char (6) null, intoper2 char (6) null, vicoper char (6) null, vicoper2 char (6) null, intlatit integer null, intlongit integer null, intgrnd double precision null, viclatit integer null, viclongit integer null, vicgrnd double precision null, report smallint null, int1int2dist double precision null, vic1vic2dist double precision null, int1vic1dist double precision null, distadv double precision null, intoffax double precision null, vicoffax double precision null, intvicaz double precision null, vicintaz double precision null, processed integer null)";
        // TSIP ES SITE table 
        const string teSite = "create table {0}(terrcall1 char (9) null, terrcall2 char (9) null, earthlocation char (10) null, terrname1 char (32) null, terrname2 char (32) null, earthname char (16) null, terroper char (6) null, terroper2 char (6) null, earthoper char (6) null, terrlatit integer null, terrlongit integer null, terrgrnd double precision null, earthlatit integer null, earthlongit integer null, earthgrnd double precision null, radiozone char (2) null, rainzone smallint null, etreport smallint null, tereport smallint null, etcaseno integer null, tecaseno integer null, etsubcases integer null, tesubcases integer null, intreq char (4) null, etdist double precision null, etazim double precision null, teazim double precision null, tudist double precision null, tuazim double precision null, utazim double precision null, eudist double precision null, euazim double precision null, ueazim double precision null, processed integer null)";
        // TSIP TS ANTE table 
        const string ttAnte = "create table {0}(interferer char (1) null, intcall1 char (9) null, intcall2 char (9) null, intbndcde char (4) null, intanum smallint null, viccall1 char (9) null, viccall2 char (9) null, vicbndcde char (4) null, caseno integer null, vicanum smallint null, intacode char (12) null, vicacode char (12) null, report smallint null, subcaseno integer null, adiscctxh double precision null, adiscctxv double precision null, adisccrxh double precision null, adisccrxv double precision null, adiscxtxh double precision null, adiscxtxv double precision null, adiscxrxh double precision null, adiscxrxv double precision null, processed integer null, intause char (4) null, vicause char (4) null, intoffaxa double precision null, vicoffaxa double precision null, intgain double precision null, vicgain double precision null, intaxref char (12) null, intamodel char (16) null, vicaxref char (12) null, vicamodel char (16) null, intaoffax char (1) null, inthopaz double precision null, intantaz double precision null, intoffantax double precision null, vicaoffax char (1) null, vichopaz double precision null, vicantaz double precision null, vicoffantax double precision null, intaht double precision null, vicaht double precision null, intvicel double precision null, vicintel double precision null, intelev double precision null, vicelev double precision null )";
        // TSIP ES ANTE table 
        const string teAnte = "create table {0}(interferer char (1) null, terrcall1 char (9) null, terrcall2 char (9) null, terrbndcde char (4) null, terranum smallint null, earthlocation char (10) null, earthcall1 char (9) null, earthband char (4) null, terracode char (12) null, earthacode char (12) null, satname char (16) null, satoper char (3) null, satlongit integer null, txpre real null, txtro real null, rxpre real null, rxtro real null, sarc1 double precision null, sarc2 double precision null, mode1 smallint null, mode2 smallint null, intause char (4) null, etreport smallint null, tereport smallint null, etsubcaseno integer null, tesubcaseno integer null, esazim double precision null, eselev double precision null, teelev double precision null, etelev double precision null, tuelev double precision null, utelev double precision null, euelev double precision null, ediscang double precision null, tdiscang double precision null, adisc_set double precision null, adisc_ute double precision null, terrht double precision null, earthht double precision null, tvazim double precision null, evazim double precision null, tvelev double precision null, evelev double precision null, tvdistes double precision null, tvdisttu double precision null, evdistes double precision null, evdisttu double precision null, angleutv double precision null, anglesev double precision null, tsoffaxis char(1) null, tstrueaz double precision null, tstrueel double precision null, angleute double precision null, angleuta double precision null, angleeta double precision null, angleatv double precision null, adisc_atv double precision null, terragain double precision null, terramodel char(15) null, terraxref char(12) null, earthagain double precision null, earthamodel char(15) null, earthaxref char(12) null, processed integer null)";
        // TSIP TS CHAN table 
        const string ttChan = "create table {0}(interferer char (1) null, intcall1 char (9) null, intcall2 char (9) null, intbndcde char (4) null, intanum smallint null, intchid char (4) null, viccall1 char (9) null, viccall2 char (9) null, vicbndcde char (4) null, vicanum smallint null, caseno integer null, vicchid char (4) null, intpolar char (1) null, vicpolar char (1) null, intstattx char (1) null, vicstatrx char (1) null, inttraftx char (6) null, victrafrx char (6) null, inteqpttx char (8) null, viceqptrx char (8) null, intfreqtx double precision null, vicfreqrx double precision null, vicpwrrx double precision null, intpwrtx double precision null, intafsltx double precision null, vicafslrx double precision null, rxant smallint null, txant smallint null, ctxinttraftx char (6) null, ctxvictrafrx char (6) null, ctxeqpt char (8) null, calctype char (3) null, report smallint null, totantdisc double precision null, freqsep double precision null, reqdcalc double precision null, patloss double precision null, calcico double precision null, calcixp double precision null, resti double precision null, eirpadv double precision null, tiltdisc double precision null, pathloss80 double precision null, calcico80 double precision null, calcixp80 double precision null, reqd80 double precision null, resti80 double precision null, pathloss99 double precision null, calcico99 double precision null, calcixp99 double precision null, reqd99 double precision null, resti99 double precision null, ohresult smallint null, rqco double precision null, processed integer null, ctxinteqpt char (8) null, inteqtype char (1) null, viceqtype char (1) null, intbwchans double precision null, vicbwchans double precision null)";
        // TSIP ES CHAN table 
        const string teChan = "create table {0}(interferer char (1) null, terrcall1 char (9) null, terrcall2 char (9) null, terrbndcde char (4) null, terranum smallint null, terrchid char (4) null, earthlocation char (10) null, earthcall1 char (9) null, earthchid char (4) null, inttraftx char (6) null, victrafrx char (6) null, inteqpttx char (8) null, viceqptrx char (8) null, intfreqtx double precision null, inttxpwr double precision null, inttxpwr2 double precision null, inttxafls double precision null, inttxafls2 double precision null, vicrxafls double precision null, vicfreqrx double precision null, vicpwrrx double precision null, stattx char(1) null, statrx char(1) null, energy double precision null, etreport smallint null, tereport smallint null, ctxinttraftx char (6) null, ctxvictrafrx char (6) null, ctxeqpt char (8) null, calctype char (3) null, earthmdsc double precision null, terrmdsc double precision null, eartheirp double precision null, terreirp double precision null, freqsep double precision null, scang double precision null, loss20mode1 double precision null, calci20mode1 double precision null, loss01mode1 double precision null, calci01mode1 double precision null, loss01mode2 double precision null, calci01mode2 double precision null, reqd20mode1 double precision null, reqd01mode1 double precision null, reqd01mode2 double precision null, marg20mode1 double precision null, marg01mode1 double precision null, marg01mode2 double precision null, remterracode char(12) null, remterragain double precision null, processed integer null, terrant smallint null)";
        // MDB Report Parameters 
        const string rmParam = "create table {0}( psort char (1) null,  reptype char (10) null,  cullcodes char (11) null,  cullvalues char (280) null)";
        // SDB Report Parameters 
        const string rsParam = "create table {0}( filename char (16) null,  projcode char (12) null,  reptype char (8) null,  cullcodes char (280) null)";
        // SDB Temp Report Parameters 
        const string rsTempParm = "create table {0}(name char (32) null, call1 char (9) null, prov char (2) null, strlatit char (9) null, strlatits char (1) null, strlongit char (10) null, strlongits char (1) null, grnd double precision null, nots char (4) null, nota char (4) null, dist double precision null, tazmth double precision null, ause char (3) null, aht double precision null, acode char (12) null, anum smallint null, freqtx double precision null, poltx char (1) null, freqrx double precision null, polrx char (1) null, stattx char (1) null, statrx char (1) null, eqpttx char (8) null, eqptrx char (8) null, srvctx char (6) null, pwrtx double precision null, notetx char (4) null, noterx char (4) null, routnumb char(8) null, stnnumb tinyint null, rtname char (48) null, twcode char (4) null, twht double precision null, direction integer null)";
        // Usage Reports working table 
        const string biUsage = "create table {0}( oper char (6) null,  year integer null,  month integer null,  function char (10) null,  data_value_1 integer null,  data_value_2 double precision null,  data_value_3 integer null,  data_value_4 double precision null,  data_value_5 double precision null,  data_value_6 integer null)";
        // security reports - need two structures 
        const string sePermReq = "create table {0}( screenid char (20) null,  function char (25) null,  perm_text char (30) null)";
        const string sePermUser = "create table {0}( ultrixid char (8) null,  micsid char (10) null,  perm_text char (30) null)";
        // Fee ts call table 
        const string rpTsFeeDetail = " create table {0}( call1 char (9) null,  licence char (13) null,  call2 char (9) null,  freqtx double precision null,  traftx char (6) null,  feetx char (2) null,  feeValtx real null,  freqrx double precision null,  trafrx char (6) null,  feerx char (2) null,  feeValrx real null)";
        // Fee es call table 
        const string rpEsFeeDetail = " create table {0}( location char (10) null,  call1 char (9) null,  licence char (13) null,  satname char (16) null,  freqtx double precision null,  traftx char (6) null,  feetx char (2) null,  feeValtx real null,  freqrx double precision null,  trafrx char (6) null,  feerx char (2) null,  feeValrx real null)";
        // Audit Trail table 
        const string atTable = "create table {0}( ultrixid char (8) null,  micsid char (10) null,  tabletype char (4) null,  filename char (16) null,  mdate char(10) null,  mtime char (8) null)";
        // Table list 
        const string utTableList = "create table {0}( type i4 null,  tname c20 null,  status c2 null)";
        // Billing Mics Month Table 
        const string biMicsTable = "create table {0}( month i4 null,  year i4 null,  oper c6 null,  pcode c10 null,  ctime i4 null,  ptime i4 null,  privatestorage f4 null,  iocount i4 null,  date char(10) null,  time c5 null)";
        // Billing Ultrix Month Table 
        const string biUltrixTable = "create table {0}(month i4 null,  year i4 null,  oper c6 null,  logincount i4 null,  connecttime i4 null,  processtime f4 null,  printcount f4 null,  mdbstorage f4 null,  mdbfeevalue f4 null,  mdbfratio f4 null,  date char(10) null,  time c5 null)";
        // TS Alpha Report Temporary Site table 
        const string tsAlphaSiteTable = "create table {0}(call1 c9 null)";
        // TS Alpha Report Temporary Ante table 
        const string tsAlphaAnteTable = " create table {0} (call1 c9 null,  call2 c9 null,  bndcde c4 null,  anum i2 null)";
        // TS Alpha Report Temporary Chan table 
        const string tsAlphaChanTable = " create table {0} (call1 c9 null,  call2 c9 null,  bndcde c4 null,  chid c4 null)";
        // TS Alpha Report Temporary Town table 
        const string tsAlphaTownTable = " create table {0} (call1 c9 null,  twcode c5 null,  twht f4 null)";
        // ES Alpha Report Temporary Site table 
        const string esAlphaSiteTable = "create table {0}(location c10 null)";
        // ES Alpha Report Temporary Ante table 
        const string esAlphaAnteTable = " create table {0} (location c10 null,  call1 c9 null)";
        // ES Alpha Report Temporary Chan table 
        const string esAlphaChanTable = " create table {0} (location c10 null,  call1 c9 null,  chid c4 null)";

        // *************** End of table definitions *************** 






    }
}
