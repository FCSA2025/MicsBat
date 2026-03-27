using DBUtilities;
using LongLatUtilities;
using System;
using System.Data.Odbc;
using System.IO;

namespace ImpExpstructs
{
    public struct structEStitl
    {
        public string validated;
        public string namef;
        public string source;
        public string descr;
        public string mdate;
        public string mtime;

        public structEStitl(structEStitl xx)
        {
            validated = xx.validated;
            namef = xx.namef;
            source = xx.source;
            descr = xx.descr;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structESante
    {
        public string cmd;
        public string recstat;
        public string location;
        public string call1;
        public string txband;
        public string rxband;
        public string acodetx;
        public string acoderx;
        public string g_t;
        public string lnat;
        public string aht;
        public string afslt;
        public string afslr;
        public string txhgmax;
        public string rxhgmax;
        public string satlongit;
        public string satlong;
        public string satlongs;
        public string az;
        public string el;
        public string sarc1;
        public string sarc2;
        public string rxpre;
        public string txpre;
        public string rxtro;
        public string txtro;
        public string licence;
        public string satname;
        public string stata;
        public string nota;
        public string op2;
        public string antref;
        public string orbit;
        public string mdate;
        public string mtime;

        public structESante(structESante xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            location = xx.location;
            call1 = xx.call1;
            txband = xx.txband;
            rxband = xx.rxband;
            acodetx = xx.acodetx;
            acoderx = xx.acoderx;
            g_t = xx.g_t;
            lnat = xx.lnat;
            aht = xx.aht;
            afslt = xx.afslt;
            afslr = xx.afslr;
            txhgmax = xx.txhgmax;
            rxhgmax = xx.rxhgmax;
            satlongit = xx.satlongit;
            satlong = xx.satlong;
            satlongs = xx.satlongs;
            az = xx.az;
            el = xx.el;
            sarc1 = xx.sarc1;
            sarc2 = xx.sarc2;
            rxpre = xx.rxpre;
            txpre = xx.txpre;
            rxtro = xx.rxtro;
            txtro = xx.txtro;
            licence = xx.licence;
            satname = xx.satname;
            stata = xx.stata;
            nota = xx.nota;
            op2 = xx.op2;
            antref = xx.antref;
            orbit = xx.orbit;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structESazim
    {
        public string cmd;
        public string recstat;
        public string deleteall;
        public string location;
        public string call1;
        public string azim;
        public string elev;
        public string dist;
        public string loss;
        public string mdate;
        public string mtime;

        public structESazim(structESazim xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            deleteall = xx.deleteall;
            location = xx.location;
            call1 = xx.call1;
            azim = xx.azim;
            elev = xx.elev;
            dist = xx.dist;
            loss = xx.loss;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structEScloc
    {
        public string oldlocation;
        public string newlocation;
        public string name;

        public structEScloc(structEScloc xx)
        {
            oldlocation = xx.oldlocation;
            newlocation = xx.newlocation;
            name = xx.name;
        }
    }
    public struct structESccal
    {
        public string newcallsign;
        public string oldcallsign;

        public structESccal(structESccal xx)
        {
            newcallsign = xx.newcallsign;
            oldcallsign = xx.oldcallsign;
        }
    }
    public struct structESchan
    {
        public string cmd;
        public string recstat;
        public string location;
        public string call1;
        public string chid;
        public string freqtx;
        public string poltx;
        public string maxtxpower;
        public string pwrtx;
        public string p4khz;
        public string eqpttx;
        public string traftx;
        public string stattx;
        public string feetx;
        public string freqrx;
        public string polrx;
        public string pwrrx;
        public string eqptrx;
        public string trafrx;
        public string statrx;
        public string i20;
        public string it01;
        public string ip01;
        public string feerx;
        public string notc;
        public string srvctx;
        public string srvcrx;
        public string mdate;
        public string mtime;

        public structESchan(structESchan xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            location = xx.location;
            call1 = xx.call1;
            chid = xx.chid;
            freqtx = xx.freqtx;
            poltx = xx.poltx;
            maxtxpower = xx.maxtxpower;
            pwrtx = xx.pwrtx;
            p4khz = xx.p4khz;
            eqpttx = xx.eqpttx;
            traftx = xx.traftx;
            stattx = xx.stattx;
            feetx = xx.feetx;
            freqrx = xx.freqrx;
            polrx = xx.polrx;
            pwrrx = xx.pwrrx;
            eqptrx = xx.eqptrx;
            trafrx = xx.trafrx;
            statrx = xx.statrx;
            i20 = xx.i20;
            it01 = xx.it01;
            ip01 = xx.ip01;
            feerx = xx.feerx;
            notc = xx.notc;
            srvctx = xx.srvctx;
            srvcrx = xx.srvcrx;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structESsite
    {
        public string cmd;
        public string recstat;
        public string location;
        public string name;
        public string prov;
        public string oper;
        public string latit;
        public string longit;
        public string grnd;
        public string stats;
        public string radio;
        public string rain;
        public string reg;
        public string nots;
        public string oprtyp;
        public string mdate;
        public string mtime;
        public string sdate;

        public structESsite(structESsite xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            location = xx.location;
            name = xx.name;
            prov = xx.prov;
            oper = xx.oper;
            latit = xx.latit;
            longit = xx.longit;
            grnd = xx.grnd;
            stats = xx.stats;
            radio = xx.radio;
            rain = xx.rain;
            reg = xx.reg;
            nots = xx.nots;
            oprtyp = xx.oprtyp;
            mdate = xx.mdate;
            mtime = xx.mtime;
            sdate = xx.sdate;
        }
    }
    public struct structSDFantd
    {
        public string cmd;
        public string acode;
        public string antang;
        public string dcov;
        public string dxpv;
        public string dcoh;
        public string dxph;
        public string dtilt;
        public string interpstat;
        public string mdate;
        public string mtime;

        public structSDFantd(structSDFantd xx)
        {
            cmd = xx.cmd;
            acode = xx.acode;
            antang = xx.antang;
            dcov = xx.dcov;
            dxpv = xx.dxpv;
            dcoh = xx.dcoh;
            dxph = xx.dxph;
            dtilt = xx.dtilt;
            interpstat = xx.interpstat;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFante
    {
        public string cmd;
        public string recstat;
        public string acode;
        public string axtype;
        public string axref;
        public string again;
        public string abw;
        public string arms;
        public string aband;
        public string amanu;
        public string apattern;
        public string amodel;
        public string anip;
        public string ax0;
        public string adesc;
        public string antype;
        public string aftbr;
        public string lofreq;
        public string hifreq;
        public string bandcodes;
        public string mdate;
        public string mtime;

        public structSDFante(structSDFante xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            acode = xx.acode;
            axtype = xx.axtype;
            axref = xx.axref;
            again = xx.again;
            abw = xx.abw;
            arms = xx.arms;
            aband = xx.aband;
            amanu = xx.amanu;
            apattern = xx.apattern;
            amodel = xx.amodel;
            anip = xx.anip;
            ax0 = xx.ax0;
            adesc = xx.adesc;
            antype = xx.antype;
            aftbr = xx.aftbr;
            lofreq = xx.lofreq;
            hifreq = xx.hifreq;
            bandcodes = xx.bandcodes;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFband
    {
        public string cmd;
        public string recstat;
        public string bndcde;
        public string bandbitpos;
        public string blo;
        public string bmidf;
        public string bhi;
        public string badj;
        public string mdate;
        public string mtime;

        public structSDFband(structSDFband xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            bndcde = xx.bndcde;
            bandbitpos = xx.bandbitpos;
            blo = xx.blo;
            bmidf = xx.bmidf;
            bhi = xx.bhi;
            badj = xx.badj;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFctx
    {
        public string cmd;
        public string recstat;
        public string tfcr;
        public string tfci;
        public string rxeqp;
        public string rqco;
        public string rqcull;
        public string rqwrst;
        public string ctxndp;
        public string ctxdesc;
        public string mdate;
        public string mtime;

        public structSDFctx(structSDFctx xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            tfcr = xx.tfcr;
            tfci = xx.tfci;
            rxeqp = xx.rxeqp;
            rqco = xx.rqco;
            rqcull = xx.rqcull;
            rqwrst = xx.rqwrst;
            ctxndp = xx.ctxndp;
            ctxdesc = xx.ctxdesc;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFctxd
    {
        public string cmd;
        public string recstat;
        public string tfcr;
        public string tfci;
        public string rxeqp;
        public string fsep;
        public string rq;
        public string mdate;
        public string mtime;

        public structSDFctxd(structSDFctxd xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            tfcr = xx.tfcr;
            tfci = xx.tfci;
            rxeqp = xx.rxeqp;
            fsep = xx.fsep;
            rq = xx.rq;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFeqpt
    {
        public string cmd;
        public string recstat;
        public string ecode;
        public string estab;
        public string exref;
        public string emanu;
        public string emodel;
        public string edesc;
        public string etype;
        public string etraf;
        public string emission;
        public string e1stif;
        public string e2ndif;
        public string thhold;
        public string ebndcde;
        public string mdate;
        public string mtime;

        public structSDFeqpt(structSDFeqpt xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            ecode = xx.ecode;
            estab = xx.estab;
            exref = xx.exref;
            emanu = xx.emanu;
            emodel = xx.emodel;
            edesc = xx.edesc;
            etype = xx.etype;
            etraf = xx.etraf;
            emission = xx.emission;
            e1stif = xx.e1stif;
            e2ndif = xx.e2ndif;
            thhold = xx.thhold;
            ebndcde = xx.ebndcde;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }

    }
    public struct structSDFnote
    {
        public string cmd;
        public string recstat;
        public string oper;
        public string nonum;
        public string note;
        public string mdate;
        public string mtime;

        public structSDFnote(structSDFnote xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            oper = xx.oper;
            nonum = xx.nonum;
            note = xx.note;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }

    }
    public struct structSDFoper
    {
        public string cmd;
        public string recstat;
        public string oper;
        public string nameop;
        public string cooper;
        public string mdbm;
        public string addr;
        public string city;
        public string prstat;
        public string zippc;
        public string dept;
        public string namep;
        public string phonep;
        public string faxnum;
        public string telecom;
        public string opnote;
        public string admin;
        public string email;
        public string mdate;
        public string mtime;

        public structSDFoper(structSDFoper xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            oper = xx.oper;
            nameop = xx.nameop;
            cooper = xx.cooper;
            mdbm = xx.mdbm;
            addr = xx.addr;
            city = xx.city;
            prstat = xx.prstat;
            zippc = xx.zippc;
            dept = xx.dept;
            namep = xx.namep;
            phonep = xx.phonep;
            faxnum = xx.faxnum;
            telecom = xx.telecom;
            opnote = xx.opnote;
            admin = xx.admin;
            email = xx.email;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFplan
    {
        public string cmd;
        public string recstat;
        public string sband;
        public string splan;
        public string srsp;
        public string srspiss;
        public string conform;
        public string uscan;
        public string mdate;
        public string mtime;

        public structSDFplan(structSDFplan xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            sband = xx.sband;
            splan = xx.splan;
            srsp = xx.srsp;
            srspiss = xx.srspiss;
            conform = xx.conform;
            uscan = xx.uscan;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFplnd
    {
        public string cmd;
        public string recstat;
        public string sband;
        public string splan;
        public string spno;
        public string set1;
        public string s1chid;
        public string set2;
        public string s2chid;
        public string set3;
        public string s3chid;
        public string set4;
        public string s4chid;
        public string mdate;
        public string mtime;

        public structSDFplnd(structSDFplnd xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            sband = xx.sband;
            splan = xx.splan;
            spno = xx.spno;
            set1 = xx.set1;
            s1chid = xx.s1chid;
            set2 = xx.set2;
            s2chid = xx.s2chid;
            set3 = xx.set3;
            s3chid = xx.s3chid;
            set4 = xx.set4;
            s4chid = xx.s4chid;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFrout
    {
        public string cmd;
        public string recstat;
        public string rcomp;
        public string routnumb;
        public string rtprov;
        public string rtcall;
        public string rtname;
        public string mdate;
        public string mtime;

        public structSDFrout(structSDFrout xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            rcomp = xx.rcomp;
            routnumb = xx.routnumb;
            rtprov = xx.rtprov;
            rtcall = xx.rtcall;
            rtname = xx.rtname;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFtown
    {
        public string cmd;
        public string recstat;
        public string call1;
        public string oper;
        public string twcode;
        public string twht;
        public string atwrno;
        public string twli;
        public string twpa;
        public string nott;
        public string tpoint;
        public string adate;
        public string sdate;
        public string mdate;
        public string mtime;

        public structSDFtown(structSDFtown xx)
        {
            cmd = xx.cmd;
            recstat = xx.cmd;
            call1 = xx.call1;
            oper = xx.oper;
            twcode = xx.twcode;
            twht = xx.twht;
            atwrno = xx.atwrno;
            twli = xx.twli;
            twpa = xx.twpa;
            nott = xx.nott;
            tpoint = xx.tpoint;
            adate = xx.adate;
            sdate = xx.sdate;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFtowr
    {
        public string cmd;
        public string recstat;
        public string twcode;
        public string twdesc;
        public string mdate;
        public string mtime;

        public structSDFtowr(structSDFtowr xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            twcode = xx.twcode;
            twdesc = xx.twdesc;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }
    public struct structSDFtraf
    {
        public string cmd;
        public string recstat;
        public string trafcode;
        public string ecode;
        public string xreftrcde;
        public string xrefeqcde;
        public string trdesc;
        public string mdate;
        public string mtime;

        public structSDFtraf(structSDFtraf xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            trafcode = xx.trafcode;
            ecode = xx.ecode;
            xreftrcde = xx.xreftrcde;
            xrefeqcde = xx.xrefeqcde;
            trdesc = xx.trdesc;
            mdate = xx.mdate;
            mtime = xx.mtime;
        }
    }

    public class ImpExp
    {
        public static void EStitlClear(out structEStitl EStitl)
        {
            EStitl.validated = "";
            EStitl.namef = "";
            EStitl.source = "";
            EStitl.descr = "";
            EStitl.mdate = "";
            EStitl.mtime = "";
        }
        public static string EStitlPrint(StreamWriter swrep, string schema, string cn_str, string pdfName, bool itan_dates)
        {
            string strSql;

            string titlTable = schema + ".fe_" + pdfName + "_titl";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structEStitl EStitl = new structEStitl();

            strSql = "SELECT validated, namef, source, descr, mdate, mtime " +
                     " FROM " + titlTable;

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }


            if (dr4.HasRows)
            {
                EStitlClear(out EStitl);

                dr4.Read();
                EStitl.validated = DBUtils.GetDBString(dr4, 0);
                EStitl.namef = DBUtils.GetDBString(dr4, 1);
                EStitl.source = DBUtils.GetDBString(dr4, 2);
                EStitl.descr = DBUtils.GetDBString(dr4, 3);
                EStitl.mdate = DBUtils.GetDBString(dr4, 4);
                EStitl.mtime = DBUtils.GetDBString(dr4, 5);

                // optionally convert dates to itanium format
                if (itan_dates)
                {
                    EStitl.mdate = itan_date(EStitl.mdate);
                }

                swrep.WriteLine("TE," +
                                prt_field(EStitl.validated) +
                                prt_field(EStitl.namef) +
                                prt_field(EStitl.source) +
                                prt_field(EStitl.mdate) +
                                EStitl.mtime);
                swrep.WriteLine("TD," + EStitl.descr);
                swrep.Flush();
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                return "OK";
            }

        }
        public static string EStitlUpdate(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structEStitl EStitl)
        {
            string strSql;
            string titlTable = schema + ".fe_" + pdfName + "_titl";

            //Build our SQL string to INSERT the record based on the form values
            strSql = "UPDATE " + titlTable + " SET " +
                     "validated=" + DBUtils.chNull(EStitl.validated.ToUpper()) + ", " +
                     "namef=" + DBUtils.chNull(EStitl.namef.ToUpper()) + ", " +
                     "source=" + DBUtils.chNull(EStitl.source.ToUpper()) + ", " +
                     "descr=" + DBUtils.chNull(EStitl.descr.ToUpper()) + ", " +
                     "mdate=" + DBUtils.chNull(EStitl.mdate.ToUpper()) + ", " +
                     "mtime=" + DBUtils.chNull(EStitl.mtime.ToUpper());

            OdbcCommand update1 = new OdbcCommand(strSql, cn);
            update1.Transaction = otr;

            try
            {
                update1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQL:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }

        public static void ESazimClear(out structESazim ESazim)
        {
            ESazim.cmd = "";
            ESazim.recstat = "";
            ESazim.deleteall = "";
            ESazim.location = "";
            ESazim.call1 = "";
            ESazim.azim = "";
            ESazim.elev = "";
            ESazim.dist = "";
            ESazim.loss = "";
            ESazim.mdate = "";
            ESazim.mtime = "";
        }
        public static string ESazimInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structESazim ESazim)
        {
            string strSql;
            string azimTable = schema + ".fe_" + pdfName + "_azim";

            //Build our SQL string to INSERT the record based on the form values
            strSql = "INSERT INTO " + azimTable +
                     " (cmd,recstat,deleteall,location,call1,azim,elev,dist,loss,mdate, mtime)" +
                     " VALUES " +
                     "(" +
                     DBUtils.chNull(ESazim.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(ESazim.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(ESazim.deleteall.ToUpper()) + ", " +
                     DBUtils.chNull(ESazim.location.ToUpper()) + ", " +
                     DBUtils.chNull(ESazim.call1.ToUpper()) + ", " +
                     DBUtils.numNull(ESazim.azim) + ", " +
                     DBUtils.numNull(ESazim.elev) + ", " +
                     DBUtils.numNull(ESazim.dist) + ", " +
                     DBUtils.numNull(ESazim.loss) + ", " +
                     DBUtils.chNull(ESazim.mdate) + ", " +
                     DBUtils.chNull(ESazim.mtime) +
                     ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string ESazimPrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string location, string call1, bool itan_dates)
        {
            string strSql;

            string azimTable = schema + ".fe_" + pdfName + "_azim";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structESazim ESazim = new structESazim();

            strSql = "SELECT cmd, recstat, deleteall, location, call1, azim, elev, dist, loss, mdate, mtime " +
                     " FROM " + azimTable +
                     " WHERE location ='" + location + "' and call1 = '" + call1 + "' ORDER by azim";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }

            if (dr4.HasRows)
            {
                swrep.WriteLine("*---------------------------------------------------------------------------");

                while (dr4.Read())
                {
                    ESazimClear(out ESazim);

                    ESazim.cmd = DBUtils.GetDBString(dr4, 0);
                    ESazim.recstat = DBUtils.GetDBString(dr4, 1);
                    ESazim.deleteall = DBUtils.GetDBString(dr4, 2);
                    ESazim.location = DBUtils.GetDBString(dr4, 3);
                    ESazim.call1 = DBUtils.GetDBString(dr4, 4);
                    ESazim.azim = DBUtils.GetDBFloat(dr4, 5, 2);
                    ESazim.elev = DBUtils.GetDBFloat(dr4, 6, 2);
                    ESazim.dist = DBUtils.GetDBFloat(dr4, 7, 2);
                    ESazim.loss = DBUtils.GetDBFloat(dr4, 8, 2);
                    ESazim.mdate = DBUtils.GetDBString(dr4, 9);
                    ESazim.mtime = DBUtils.GetDBString(dr4, 10);

                    // optionally convert dates to itanium format
                    if (itan_dates)
                    {
                        ESazim.mdate = itan_date(ESazim.mdate);
                    }

                    swrep.WriteLine("ZK," +
                        prt_field(ESazim.deleteall) +
                        prt_field(ESazim.cmd) +
                        prt_field(ESazim.recstat) +
                        prt_field(ESazim.location) +
                        prt_field(ESazim.call1) +
                        prt_field(ESazim.azim) +
                        prt_field(ESazim.elev) +
                        prt_field(ESazim.dist) +
                        prt_field(ESazim.loss) +
                        prt_field(ESazim.mdate) +
                        ESazim.mtime);
                    swrep.Flush();
                }
            }
            dr4.Close();
            return "OK";

        }
        public static void ESclocClear(out structEScloc EScloc)
        {
            EScloc.oldlocation = "";
            EScloc.newlocation = "";
            EScloc.name = "";
        }
        public static string ESclocInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structEScloc EScloc)
        {
            string strSql;
            string clocTable = schema + ".fe_" + pdfName + "_cloc";

            //Build our SQL string to INSERT the record based on the form values
            strSql = "INSERT INTO " + clocTable +
                     " (oldlocation, newlocation, name)" +
                     " VALUES " +
                     "(" +
                     DBUtils.chNull(EScloc.oldlocation.ToUpper()) + ", " +
                     DBUtils.chNull(EScloc.newlocation.ToUpper()) + ", " +
                     DBUtils.chNull(EScloc.name.ToUpper()) +
                     ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string ESclocPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;

            string clocTable = schema + ".fe_" + pdfName + "_cloc";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structEScloc EScloc = new structEScloc();


            strSql = "SELECT oldlocation, newlocation, name " +
                     " FROM " + clocTable;

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }


            if (dr4.HasRows)
            {
                while (dr4.Read())
                {
                    ESclocClear(out EScloc);

                    EScloc.oldlocation = DBUtils.GetDBString(dr4, 0);
                    EScloc.newlocation = DBUtils.GetDBString(dr4, 1);
                    EScloc.name = DBUtils.GetDBString(dr4, 2);

                    swrep.WriteLine("LK," +
                                    prt_field(EScloc.oldlocation) +
                                    prt_field(EScloc.newlocation) +
                                    EScloc.name);
                    swrep.Flush();

                }
            }
            dr4.Close();
            return "OK";

        }
        public static void ESccalClear(out structESccal ESccal)
        {
            ESccal.newcallsign = "";
            ESccal.oldcallsign = "";
        }
        public static string ESccalInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structESccal ESccal)
        {
            string strSql;
            string ccalTable = schema + ".fe_" + pdfName + "_ccal";

            //Build our SQL string to INSERT the record based on the form values
            strSql = "INSERT INTO " + ccalTable +
                     " (newcallsign, oldcallsign)" +
                     " VALUES " +
                     "(" +
                     DBUtils.chNull(ESccal.newcallsign.ToUpper()) + ", " +
                     DBUtils.chNull(ESccal.oldcallsign.ToUpper()) +
                     ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string ESccalPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;

            string ccalTable = schema + ".fe_" + pdfName + "_ccal";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structESccal ESccal = new structESccal();

            strSql = "SELECT newcallsign, oldcallsign " +
                     " FROM " + ccalTable;

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }


            if (dr4.HasRows)
            {
                while (dr4.Read())
                {
                    ESccalClear(out ESccal);

                    ESccal.newcallsign = DBUtils.GetDBString(dr4, 0);
                    ESccal.oldcallsign = DBUtils.GetDBString(dr4, 1);

                    swrep.WriteLine("GK," +
                                    prt_field(ESccal.oldcallsign) +
                                    ESccal.newcallsign);
                    swrep.Flush();

                }
            }
            dr4.Close();
            return "OK";

        }
        public static void ESanteClear(out structESante ESante)
        {
            ESante.cmd = "";
            ESante.recstat = "";
            ESante.location = "";
            ESante.call1 = "";
            ESante.txband = "";
            ESante.rxband = "";
            ESante.acodetx = "";
            ESante.acoderx = "";
            ESante.g_t = "";
            ESante.lnat = "";
            ESante.aht = "";
            ESante.afslt = "";
            ESante.afslr = "";
            ESante.txhgmax = "";
            ESante.rxhgmax = "";
            ESante.satlongit = "";
            ESante.satlong = "";
            ESante.satlongs = "";
            ESante.az = "";
            ESante.el = "";
            ESante.sarc1 = "";
            ESante.sarc2 = "";
            ESante.rxpre = "";
            ESante.txpre = "";
            ESante.rxtro = "";
            ESante.txtro = "";
            ESante.licence = "";
            ESante.satname = "";
            ESante.stata = "";
            ESante.nota = "";
            ESante.op2 = "";
            ESante.antref = "";
            ESante.orbit = "";
            ESante.mdate = "";
            ESante.mtime = "";
        }
        public static string ESanteInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structESante ESante)
        {
            string strSql;
            string anteTable = schema + ".fe_" + pdfName + "_ante";

            // calculate hidden field satlongit from satlong
            if (ESante.satlongs.ToUpper() == "W")
            {
                if (ESante.satlong.ToString() == "")
                {
                    ESante.satlongit = "0";
                }
                else
                {
                    double dbllongit = System.Convert.ToDouble(ESante.satlong.ToString()) * 360000.0;
                    int intlongit = System.Convert.ToInt32(dbllongit);
                    ESante.satlongit = intlongit.ToString();
                }
            }
            else
            {
                if (ESante.satlong.ToString() == "")
                {
                    ESante.satlongit = "0";
                }
                else
                {
                    double dbllongit = System.Convert.ToDouble(ESante.satlong.ToString()) * -360000.0;
                    int intlongit = System.Convert.ToInt32(dbllongit);
                    ESante.satlongit = intlongit.ToString();
                }
            }

            //Build our SQL string to INSERT the record based on the form values
            strSql = "INSERT INTO " + anteTable +
                     " (cmd, recstat, location, call1, txband, rxband, acodetx, acoderx, " +
                     "g_t, lnat, aht, afslt, afslr, txhgmax, rxhgmax, satlongit, satlong, satlongs, az, " +
                     "el, sarc1, sarc2, rxpre, txpre, rxtro, txtro, licence, satname, " +
                     "stata, nota, op2, antref, orbit, mdate, mtime)" +
                     " VALUES " +
                     "(" +
                     DBUtils.chNull(ESante.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.location.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.call1.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.txband.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.rxband.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.acodetx.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.acoderx.ToUpper()) + ", " +
                     DBUtils.numNull(ESante.g_t) + ", " +
                     DBUtils.numNull(ESante.lnat) + ", " +
                     DBUtils.numNull(ESante.aht) + ", " +
                     DBUtils.numNull(ESante.afslt) + ", " +
                     DBUtils.numNull(ESante.afslr) + ", " +
                     DBUtils.numNull(ESante.txhgmax) + ", " +
                     DBUtils.numNull(ESante.rxhgmax) + ", " +
                     DBUtils.numNull(ESante.satlongit) + ", " +
                     DBUtils.numNull(ESante.satlong) + ", " +
                     DBUtils.chNull(ESante.satlongs.ToUpper()) + ", " +
                     DBUtils.numNull(ESante.az) + ", " +
                     DBUtils.numNull(ESante.el) + ", " +
                     DBUtils.numNull(ESante.sarc1) + ", " +
                     DBUtils.numNull(ESante.sarc2) + ", " +
                     DBUtils.numNull(ESante.rxpre) + ", " +
                     DBUtils.numNull(ESante.txpre) + ", " +
                     DBUtils.numNull(ESante.rxtro) + ", " +
                     DBUtils.numNull(ESante.txtro) + ", " +
                     DBUtils.chNull(ESante.licence.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.satname.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.stata.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.nota.ToUpper()) + "," +
                     DBUtils.chNull(ESante.op2.ToUpper()) + "," +
                     DBUtils.numNull(ESante.antref) + ", " +
                     DBUtils.chNull(ESante.orbit.ToUpper()) + ", " +
                     DBUtils.chNull(ESante.mdate) + ", " +
                     DBUtils.chNull(ESante.mtime) +
                     ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string ESantePrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string location, string call1, bool itan_dates)
        {
            string strSql;

            string anteTable = schema + ".fe_" + pdfName + "_ante";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structESante ESante = new structESante();

            strSql = "SELECT cmd, recstat, location, call1, txband, rxband, acodetx, acoderx, " +
                     "g_t, lnat, aht, afslt, afslr, txhgmax, rxhgmax, satlongit, satlong, satlongs, az, " +
                     "el, sarc1, sarc2, rxpre, txpre, rxtro, txtro, licence, satname, " +
                     "stata, nota, op2, antref, orbit, mdate, mtime " +
                     " FROM " + anteTable +
                     " WHERE location='" + location + "'" +
                     " AND call1='" + call1 + "'";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }


            if (dr4.HasRows)
            {
                swrep.WriteLine("*---------------------------------------------------------------------------");

                ESanteClear(out ESante);

                dr4.Read();
                ESante.cmd = DBUtils.GetDBString(dr4, 0);   //  0 cmd
                ESante.recstat = DBUtils.GetDBString(dr4, 1);   //  1 recstat
                ESante.location = DBUtils.GetDBString(dr4, 2);  //  2 location
                ESante.call1 = DBUtils.GetDBString(dr4, 3); //  3 call1
                ESante.txband = DBUtils.GetDBString(dr4, 4); // 4 txband
                ESante.rxband = DBUtils.GetDBString(dr4, 5); // 5 rxband
                ESante.acodetx = DBUtils.GetDBString(dr4, 6); // 6 acodetx
                ESante.acoderx = DBUtils.GetDBString(dr4, 7); // 7 acoderx
                ESante.g_t = DBUtils.GetDBFloat(dr4, 8, 1); // 8 g_t
                ESante.lnat = DBUtils.GetDBFloat(dr4, 9, 1); // 9 lnat
                ESante.aht = DBUtils.GetDBFloat(dr4, 10, 1); // 10 aht
                ESante.afslt = DBUtils.GetDBFloat(dr4, 11, 1); // 11 afslt
                ESante.afslr = DBUtils.GetDBFloat(dr4, 12, 1); // 12 afsl2
                ESante.txhgmax = DBUtils.GetDBFloat(dr4, 13, 1); // 13 txhgmax
                ESante.rxhgmax = DBUtils.GetDBFloat(dr4, 14, 1); // 14 rxhgmax
                ESante.satlongit = DBUtils.GetDBInt32(dr4, 15); // 15 satlongit
                ESante.satlong = DBUtils.GetDBFloat(dr4, 16, 2); // 16 satlong
                ESante.satlongs = DBUtils.GetDBString(dr4, 17); // 17 satlongs
                ESante.az = DBUtils.GetDBFloat(dr4, 18, 2); // 18 az
                ESante.el = DBUtils.GetDBFloat(dr4, 19, 2); // 19 el
                ESante.sarc1 = DBUtils.GetDBFloat(dr4, 20, 2);// 20 sarc1
                ESante.sarc2 = DBUtils.GetDBFloat(dr4, 21, 2);// 21 sarc2
                ESante.rxpre = DBUtils.GetDBFloat(dr4, 22, 2);// 22 rxpre
                ESante.txpre = DBUtils.GetDBFloat(dr4, 23, 2);// 23 txpre
                ESante.rxtro = DBUtils.GetDBFloat(dr4, 24, 2);// 24 rxtro
                ESante.txtro = DBUtils.GetDBFloat(dr4, 25, 2);// 25 txtro
                ESante.licence = DBUtils.GetDBString(dr4, 26); // 26 licence;
                ESante.satname = DBUtils.GetDBString(dr4, 27);// 27 satname
                ESante.stata = DBUtils.GetDBString(dr4, 28);// 28 stata
                ESante.nota = DBUtils.GetDBString(dr4, 29); // 20 nota
                ESante.op2 = DBUtils.GetDBString(dr4, 30); // 30 op2
                ESante.antref = DBUtils.GetDBInt32(dr4, 31);// 31 antref
                ESante.orbit = DBUtils.GetDBString(dr4, 32);// 32 orbit
                ESante.mdate = DBUtils.GetDBString(dr4, 33); // mdate
                ESante.mtime = DBUtils.GetDBString(dr4, 34); // mtime

                // optionally convert dates to itanium format
                if (itan_dates)
                {
                    ESante.mdate = itan_date(ESante.mdate);
                }

                swrep.WriteLine("AK," +
                                prt_field(ESante.cmd) +
                                prt_field(ESante.recstat) +
                                prt_field(ESante.location) +
                                prt_field(ESante.call1) +
                                prt_field(ESante.licence) +
                                prt_field(ESante.stata) +
                                prt_field(ESante.nota) +
                                prt_field(ESante.antref) +
                                prt_field(ESante.mdate) +
                                ESante.mtime);
                swrep.WriteLine("AT," +
                                prt_field(ESante.txband) +
                                prt_field(ESante.acodetx) +
                                prt_field(ESante.afslt) +
                                prt_field(ESante.txhgmax) +
                                prt_field(ESante.txtro) +
                                prt_field(ESante.txpre) +
                                prt_field(ESante.aht) +
                                prt_field(ESante.az) +
                                ESante.el);
                swrep.WriteLine("AR," +
                                prt_field(ESante.rxband) +
                                prt_field(ESante.acoderx) +
                                prt_field(ESante.afslr) +
                                prt_field(ESante.rxhgmax) +
                                prt_field(ESante.rxtro) +
                                prt_field(ESante.rxpre) +
                                prt_field(ESante.g_t) +
                                ESante.lnat);
                swrep.WriteLine("AS," +
                                prt_field(ESante.satname) +
                                prt_field(ESante.op2) +
                                prt_field(ESante.orbit) +
                                ESante.satlong + prt_field(ESante.satlongs) +
                                prt_field(ESante.sarc1) +
                                ESante.sarc2);
                swrep.Flush();

            }
            dr4.Close();
            return "OK";
        }
        public static void ESchanClear(out structESchan ESchan)
        {
            ESchan.cmd = "";
            ESchan.recstat = "";
            ESchan.location = "";
            ESchan.call1 = "";
            ESchan.chid = "";
            ESchan.freqtx = "";
            ESchan.poltx = "";
            ESchan.maxtxpower = "";
            ESchan.pwrtx = "";
            ESchan.p4khz = "";
            ESchan.eqpttx = "";
            ESchan.traftx = "";
            ESchan.stattx = "";
            ESchan.feetx = "";
            ESchan.freqrx = "";
            ESchan.polrx = "";
            ESchan.pwrrx = "";
            ESchan.eqptrx = "";
            ESchan.trafrx = "";
            ESchan.statrx = "";
            ESchan.i20 = "";
            ESchan.it01 = "";
            ESchan.ip01 = "";
            ESchan.feerx = "";
            ESchan.notc = "";
            ESchan.srvctx = "";
            ESchan.srvcrx = "";
            ESchan.mdate = "";
            ESchan.mtime = "";
        }
        public static string ESchanInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structESchan ESchan)
        {
            string strSql;

            string chanTable = schema + ".fe_" + pdfName + "_chan";

            //Build our SQL string to INSERT the record based on the form values
            strSql = "INSERT INTO " + chanTable +
                     " (cmd, recstat, location, call1, chid, freqtx, poltx, maxtxpower,  " +
                     "pwrtx, p4khz, eqpttx, traftx, stattx, feetx, freqrx, polrx, pwrrx, " +
                     "eqptrx, trafrx, statrx, i20, it01, ip01, feerx, notc, srvctx, srvcrx, mdate, mtime)" +
                     " VALUES " +
                     "(" +
                     DBUtils.chNull(ESchan.cmd.ToUpper()) + ", " +
                     "'U', " +
                     DBUtils.chNull(ESchan.location.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.call1.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.chid.ToUpper()) + ", " +
                     DBUtils.numNull(ESchan.freqtx) + ", " +
                     DBUtils.chNull(ESchan.poltx.ToUpper()) + ", " +
                     DBUtils.numNull(ESchan.maxtxpower) + ", " +
                     DBUtils.numNull(ESchan.pwrtx) + ", " +
                     DBUtils.numNull(ESchan.p4khz) + ", " +
                     DBUtils.chNull(ESchan.eqpttx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.traftx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.stattx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.feetx.ToUpper()) + ", " +
                     DBUtils.numNull(ESchan.freqrx) + ", " +
                     DBUtils.chNull(ESchan.polrx.ToUpper()) + ", " +
                     DBUtils.numNull(ESchan.pwrrx) + ", " +
                     DBUtils.chNull(ESchan.eqptrx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.trafrx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.statrx.ToUpper()) + ", " +
                     DBUtils.numNull(ESchan.i20) + ", " +
                     DBUtils.numNull(ESchan.it01) + ", " +
                     DBUtils.numNull(ESchan.ip01) + ", " +
                     DBUtils.chNull(ESchan.feerx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.notc.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.srvctx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.srvcrx.ToUpper()) + ", " +
                     DBUtils.chNull(ESchan.mdate) + ", " +
                     DBUtils.chNull(ESchan.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string ESchanPrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string location, string call1, bool itan_dates)
        {
            string strSql;

            string chanTable = schema + ".fe_" + pdfName + "_chan";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structESchan ESchan = new structESchan();

            // get channel info
            strSql = "SELECT cmd, recstat, location, call1, chid, freqtx, poltx, maxtxpower,  " +
                     "pwrtx, p4khz, eqpttx, traftx, stattx, feetx, freqrx, polrx, pwrrx, " +
                     "eqptrx, trafrx, statrx, i20, it01, ip01, feerx, notc, srvctx, srvcrx, mdate, mtime " +
                     " FROM " + chanTable +
                     " WHERE location='" + location + "'" +
                     " AND call1='" + call1 + "'" +
                     " ORDER BY chid";

            OdbcCommand select3 = new OdbcCommand(strSql);
            select3.Connection = cn;
            OdbcDataReader dr3;

            try
            {
                dr3 = select3.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            if (dr3.HasRows)
            {
                while (dr3.Read())
                {
                    swrep.WriteLine("*---------------------------------------------------------------------------");

                    ESchanClear(out ESchan);

                    ESchan.cmd = DBUtils.GetDBString(dr3, 0);// 0 cmd
                    ESchan.recstat = DBUtils.GetDBString(dr3, 1);// 1 recstat
                    ESchan.location = DBUtils.GetDBString(dr3, 2);// 2 location
                    ESchan.call1 = DBUtils.GetDBString(dr3, 3);// 3 cal1
                    ESchan.chid = DBUtils.GetDBString(dr3, 4);// 4 chid
                    ESchan.freqtx = DBUtils.GetDBDouble(dr3, 5, 2);// 5 freqtx
                    ESchan.poltx = DBUtils.GetDBString(dr3, 6);// 6 poltx
                    ESchan.maxtxpower = DBUtils.GetDBFloat(dr3, 7, 1);// 7 maxtxpower
                    ESchan.pwrtx = DBUtils.GetDBFloat(dr3, 8, 2);// 8 pwrtx
                    ESchan.p4khz = DBUtils.GetDBFloat(dr3, 9, 1);// 9 p4khz
                    ESchan.eqpttx = DBUtils.GetDBString(dr3, 10);// 10 eqpttx
                    ESchan.traftx = DBUtils.GetDBString(dr3, 11);// 11 traftx
                    ESchan.stattx = DBUtils.GetDBString(dr3, 12);// 12 stattx
                    ESchan.feetx = DBUtils.GetDBString(dr3, 13);// 13 feetx
                    ESchan.freqrx = DBUtils.GetDBDouble(dr3, 14, 2);// 14 freqrx
                    ESchan.polrx = DBUtils.GetDBString(dr3, 15);// 15 polrx
                    ESchan.pwrrx = DBUtils.GetDBFloat(dr3, 16, 2);// 16 pwrrx
                    ESchan.eqptrx = DBUtils.GetDBString(dr3, 17);// 17 eqptrx
                    ESchan.trafrx = DBUtils.GetDBString(dr3, 18);// 18 trafrx
                    ESchan.statrx = DBUtils.GetDBString(dr3, 19);// 19 statrx
                    ESchan.i20 = DBUtils.GetDBFloat(dr3, 20, 1);// 20 i20
                    ESchan.it01 = DBUtils.GetDBFloat(dr3, 21, 1);// 21 it01
                    ESchan.ip01 = DBUtils.GetDBFloat(dr3, 22, 1);// 22 ip01
                    ESchan.feerx = DBUtils.GetDBString(dr3, 23);// 23 feerx
                    ESchan.notc = DBUtils.GetDBString(dr3, 24);// 24 notc
                    ESchan.srvctx = DBUtils.GetDBString(dr3, 25);// 25 srvctx
                    ESchan.srvcrx = DBUtils.GetDBString(dr3, 26);// 26 srvcrx
                    ESchan.mdate = DBUtils.GetDBString(dr3, 27); // 27 mdate
                    ESchan.mtime = DBUtils.GetDBString(dr3, 28); // 28 mtime

                    // optionally convert dates to itanium format
                    if (itan_dates)
                    {
                        ESchan.mdate = itan_date(ESchan.mdate);
                    }

                    swrep.WriteLine("CK," +
                                    prt_field(ESchan.cmd) +
                                    prt_field(ESchan.recstat) +
                                    prt_field(ESchan.location) +
                                    prt_field(ESchan.call1) +
                                    prt_field(ESchan.chid) +
                                    prt_field(ESchan.notc) +
                                    prt_field(ESchan.mdate) +
                                    ESchan.mtime);
                    swrep.WriteLine("CT," +
                                    prt_field(ESchan.freqtx) +
                                    prt_field(ESchan.poltx) +
                                    prt_field(ESchan.maxtxpower) +
                                    prt_field(ESchan.pwrtx) +
                                    prt_field(ESchan.p4khz) +
                                    prt_field(ESchan.eqpttx) +
                                    prt_field(ESchan.traftx) +
                                    prt_field(ESchan.stattx) +
                                    prt_field(ESchan.srvctx) +
                                    ESchan.feetx);
                    swrep.WriteLine("CR," +
                                    prt_field(ESchan.freqrx) +
                                    prt_field(ESchan.polrx) +
                                    prt_field(ESchan.pwrrx) +
                                    prt_field(ESchan.eqptrx) +
                                    prt_field(ESchan.trafrx) +
                                    prt_field(ESchan.i20) +
                                    prt_field(ESchan.it01) +
                                    prt_field(ESchan.ip01) +
                                    prt_field(ESchan.statrx) +
                                    prt_field(ESchan.srvcrx) +
                                    ESchan.feerx);
                    swrep.Flush();
                }
            }
            else
            {
                dr3.Close();
                return "TESTSQL:" + strSql;
            }
            dr3.Close();

            return "OK";

        }
        public static void ESsiteClear(out structESsite ESsite)
        {
            ESsite.cmd = "";
            ESsite.recstat = "";
            ESsite.location = "";
            ESsite.name = "";
            ESsite.prov = "";
            ESsite.oper = "";
            ESsite.latit = "";
            ESsite.longit = "";
            ESsite.grnd = "";
            ESsite.stats = "";
            ESsite.radio = "";
            ESsite.rain = "";
            ESsite.reg = "";
            ESsite.nots = "";
            ESsite.oprtyp = "";
            ESsite.mdate = "";
            ESsite.mtime = "";
            ESsite.sdate = "";
            return;
        }
        public static string ESsiteInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structESsite ESsite)
        {
            string strSql;

            string siteTable = schema + ".fe_" + pdfName + "_site";

            //Build our SQL string to INSERT the record based on the form values
            strSql = "INSERT INTO " + siteTable +
                     " (cmd, recstat, location, name, prov, oper, latit, longit, grnd, stats, " +
                     "radio, rain, reg, nots, oprtyp, mdate, mtime, sdate)" +
                     " VALUES " +
                     "(" +
                     DBUtils.chNull(ESsite.cmd.ToUpper()) + ", " +
                     "'U', " +
                     DBUtils.chNull(ESsite.location.ToUpper()) + ", " +
                     DBUtils.chNull(dblqte(ESsite.name.ToUpper())) + ", " +
                     DBUtils.chNull(ESsite.prov.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.oper.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.latit.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.longit.ToUpper()) + "," +
                     DBUtils.numNull(ESsite.grnd) + ", " +
                     DBUtils.chNull(ESsite.stats.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.radio.ToUpper()) + ", " +
                     DBUtils.numNull(ESsite.rain) + ", " +
                     DBUtils.chNull(ESsite.reg.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.nots.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.oprtyp.ToUpper()) + ", " +
                     DBUtils.chNull(ESsite.mdate) + ", " +
                     DBUtils.chNull(ESsite.mtime) + ", " +
                     DBUtils.chNull(ESsite.sdate) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e3)
            {
                //cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message;
            }

            return "OK";
        }
        public static string ESsitePrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string location, bool itan_dates)
        {
            string strSql;

            string siteTable = schema + ".fe_" + pdfName + "_site";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structESsite ESsite = new structESsite();


            // get site info
            strSql = "SELECT cmd, recstat, location, name, prov, oper, latit, longit, grnd, stats, " +
                      "radio, rain, reg, nots, oprtyp, " +
                      "mdate, mtime, sdate " +
                      " FROM " + siteTable +
                      " WHERE location='" + location + "'";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e2)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr4.HasRows)
            {
                swrep.WriteLine("*---------------------------------------------------------------------------");

                dr4.Read();
                ESsite.cmd = DBUtils.GetDBString(dr4, 0);// 0 cmd
                ESsite.recstat = DBUtils.GetDBString(dr4, 1);// 1 recstat
                ESsite.location = DBUtils.GetDBString(dr4, 2);// 2 location
                ESsite.name = DBUtils.GetDBString(dr4, 3);// 3 name
                ESsite.prov = DBUtils.GetDBString(dr4, 4);// 4 prov
                ESsite.oper = DBUtils.GetDBString(dr4, 5);// 5 oper
                ESsite.latit = dr4.GetValue(6).ToString(); // 6 latit
                ESsite.longit = dr4.GetValue(7).ToString(); // 7 longit
                ESsite.grnd = DBUtils.GetDBFloat(dr4, 8, 1);// 8 grnd
                ESsite.stats = DBUtils.GetDBString(dr4, 9);// 9 stats
                ESsite.radio = DBUtils.GetDBString(dr4, 10);// 10 radio
                ESsite.rain = dr4.GetValue(11).ToString();// 11 rain
                ESsite.reg = DBUtils.GetDBString(dr4, 12);// 12 reg
                ESsite.nots = DBUtils.GetDBString(dr4, 13);// 13 nots
                ESsite.oprtyp = DBUtils.GetDBString(dr4, 14);// 14 oprtyp
                ESsite.mdate = DBUtils.GetDBString(dr4, 15);// 15 mdate
                ESsite.mtime = DBUtils.GetDBString(dr4, 16);// 16 mtime
                ESsite.sdate = DBUtils.GetDBString(dr4, 17);// 17 sdate

                // optionally convert dates to itanium format
                if (itan_dates)
                {
                    ESsite.mdate = itan_date(ESsite.mdate);
                    ESsite.sdate = itan_date(ESsite.sdate);
                }

                // process latitude info
                LongLatUtils.SplitLat(ESsite.latit);
                string strlat = LongLatUtils.latitDD + "-" + LongLatUtils.latitMM + "-" +
                                LongLatUtils.latitSS + "." + LongLatUtils.latit00 + LongLatUtils.latitDir;

                // process longitude info
                LongLatUtils.SplitLong(ESsite.longit);
                string strlong = LongLatUtils.longitDD + "-" + LongLatUtils.longitMM + "-" +
                                 LongLatUtils.longitSS + "." + LongLatUtils.longit00 + LongLatUtils.longitDir;

                swrep.WriteLine("SK," +
                                prt_field(ESsite.cmd) +
                                prt_field(ESsite.recstat) +
                                prt_field(ESsite.location) +
                                prt_field(ESsite.name) +
                                prt_field(ESsite.prov) +
                                prt_field(ESsite.oper) +
                                prt_field(ESsite.oprtyp) +
                                prt_field(ESsite.mdate) +
                                ESsite.mtime);
                swrep.WriteLine("SD," +
                                prt_field(strlat) +
                                prt_field(strlong) +
                                prt_field(ESsite.grnd) +
                                prt_field(ESsite.radio) +
                                prt_field(ESsite.rain) +
                                prt_field(ESsite.stats) +
                                prt_field(ESsite.nots) +
                                prt_field(ESsite.reg) +
                                ESsite.sdate);
                swrep.Flush();
            }
            dr4.Close();

            return "OK";

        }
        public static void SDFantdClear(out structSDFantd SDFantd)
        {
            SDFantd.cmd = "";
            SDFantd.acode = "";
            SDFantd.antang = "";
            SDFantd.dcov = "";
            SDFantd.dxpv = "";
            SDFantd.dcoh = "";
            SDFantd.dxph = "";
            SDFantd.dtilt = "";
            SDFantd.interpstat = "";
            SDFantd.mdate = "";
            SDFantd.mtime = "";
        }
        public static string SDFantdInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFantd SDFantd)
        {
            string strSql;
            string antdTable = schema + ".su_" + PDFname + "_antd";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + antdTable +
                     " (cmd, acode, antang, dcov, dxpv, dcoh, dxph, dtilt, interpstat, mdate, mtime)" +
                     " VALUES " +
                     "(" +
                    DBUtils.chNull(SDFantd.cmd.ToUpper()) + ", " +
                    DBUtils.chNull(SDFantd.acode.ToUpper()) + ", " +
                    DBUtils.numNull(SDFantd.antang) + ", " +
                    DBUtils.numNull(SDFantd.dcov) + ", " +
                    DBUtils.numNull(SDFantd.dxpv) + ", " +
                    DBUtils.numNull(SDFantd.dcoh) + ", " +
                    DBUtils.numNull(SDFantd.dxph) + ", " +
                    DBUtils.numNull(SDFantd.dtilt) + ", " +
                    DBUtils.numNull(SDFantd.interpstat) + ", " +
                    DBUtils.chNull(SDFantd.mdate) + ", " +
                    DBUtils.chNull(SDFantd.mtime) + ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string SDFantdPrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string acode)
        {
            string strSql;

            string antdTable = schema + ".su_" + pdfName + "_antd";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, acode, antang, dcov, dxpv, dcoh, dxph, dtilt, interpstat," +
                     " mdate, mtime " +
                     "FROM " + antdTable + " WHERE acode = '" + acode + "' ORDER BY antang";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }

            structSDFantd SDFantd = new structSDFantd();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*=======================================================================");
                swrep.Flush();
                while (dr4.Read())
                {
                    SDFantdClear(out SDFantd);

                    SDFantd.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFantd.acode = DBUtils.GetDBString(dr4, 1);
                    SDFantd.antang = DBUtils.GetDBFloat(dr4, 2, 2);
                    SDFantd.dcov = DBUtils.GetDBFloat(dr4, 3, 2);
                    SDFantd.dxpv = DBUtils.GetDBFloat(dr4, 4, 2);
                    SDFantd.dcoh = DBUtils.GetDBFloat(dr4, 5, 2);
                    SDFantd.dxph = DBUtils.GetDBFloat(dr4, 6, 2);
                    SDFantd.dtilt = DBUtils.GetDBFloat(dr4, 7, 2);
                    SDFantd.interpstat = DBUtils.GetDBInt32(dr4, 8);
                    SDFantd.mdate = DBUtils.GetDBString(dr4, 9);
                    SDFantd.mtime = DBUtils.GetDBString(dr4, 10);

                    swrep.WriteLine("2," +
                                    prt_field(SDFantd.cmd) +
                                    prt_field(SDFantd.acode) +
                                    prt_field(SDFantd.antang) +
                                    prt_field(SDFantd.dcoh) +
                                    prt_field(SDFantd.dxph) +
                                    prt_field(SDFantd.dcov) +
                                    prt_field(SDFantd.dxpv) +
                                    prt_field(SDFantd.dtilt) +
                                    prt_field(SDFantd.interpstat) +
                                    prt_field(SDFantd.mdate) +
                                    SDFantd.mtime);
                }

            }
            dr4.Close();
            return "OK";

        }
        public static void SDFanteClear(out structSDFante SDFante)
        {
            SDFante.cmd = "";
            SDFante.recstat = "";
            SDFante.acode = "";
            SDFante.axtype = "";
            SDFante.axref = "";
            SDFante.again = "";
            SDFante.abw = "";
            SDFante.arms = "";
            SDFante.aband = "";
            SDFante.amanu = "";
            SDFante.apattern = "";
            SDFante.amodel = "";
            SDFante.anip = "";
            SDFante.ax0 = "";
            SDFante.adesc = "";
            SDFante.antype = "";
            SDFante.aftbr = "";
            SDFante.lofreq = "";
            SDFante.hifreq = "";
            SDFante.bandcodes = "";
            SDFante.mdate = "";
            SDFante.mtime = "";
        }
        public static bool SDFanteExists(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFantd SDFantd)
        {
            string strSql;
            string anteTable = schema + ".su_" + PDFname + "_ante";
            bool retval = false;

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "SELECT COUNT(*) FROM " + anteTable +
                     " WHERE acode = '" + SDFantd.acode + "'";

            OdbcCommand count1 = new OdbcCommand(strSql, cn);
            count1.Transaction = otr;

            try
            {
                int nCount = Convert.ToInt32(count1.ExecuteScalar());
                if (nCount > 0)
                {
                    retval = true;
                }
            }
            finally
            {
            }
            return retval;
        }
        public static string SDFanteInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFante SDFante)
        {
            string strSql;
            string anteTable = schema + ".su_" + PDFname + "_ante";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + anteTable +
                     " (cmd, recstat, acode, axtype, axref, again, abw, arms, aband, " +
                     " amanu, apattern, amodel, anip, ax0, adesc, antype, aftbr, lofreq, " +
                     " hifreq, bandcodes, mdate, mtime) " +
                     " VALUES " +
                     "(" +
                    DBUtils.chNull(SDFante.cmd.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.recstat.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.acode.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.axtype) + ", " +
                    DBUtils.chNull(SDFante.axref.ToUpper()) + ", " +
                    DBUtils.numNull(SDFante.again) + ", " +
                    DBUtils.numNull(SDFante.abw) + ", " +
                    DBUtils.numNull(SDFante.arms) + ", " +
                    DBUtils.chNull(SDFante.aband.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.amanu.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.apattern.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.amodel.ToUpper()) + ", " +
                    DBUtils.numNull(SDFante.anip) + ", " +
                    DBUtils.numNull(SDFante.ax0) + ", " +
                    DBUtils.chNull(DBUtils.charQuote(SDFante.adesc.ToUpper())) + ", " +
                    DBUtils.chNull(SDFante.antype.ToUpper()) + ", " +
                    DBUtils.numNull(SDFante.aftbr) + ", " +
                    DBUtils.numNull(SDFante.lofreq) + ", " +
                    DBUtils.numNull(SDFante.hifreq) + ", " +
                    DBUtils.chNull(SDFante.bandcodes.ToUpper()) + ", " +
                    DBUtils.chNull(SDFante.mdate) + ", " +
                    DBUtils.chNull(SDFante.mtime) + ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFantePrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string retval;

            string anteTable = schema + ".su_" + pdfName + "_ante";
            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, acode, axtype, axref, again, abw, arms, aband, amanu, " +
                     "apattern, amodel, anip, ax0, adesc, antype, aftbr, lofreq, hifreq, bandcodes, " +
                     "mdate, mtime " +
                     "FROM " + anteTable + " ORDER BY acode";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }

            structSDFante SDFante = new structSDFante();

            bool first = true;

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* ANTENNA SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                while (dr4.Read())
                {
                    SDFanteClear(out SDFante);

                    SDFante.cmd = DBUtils.GetDBString(dr4, 0);	//   cmd
                    SDFante.recstat = DBUtils.GetDBString(dr4, 1);	//   recstat
                    SDFante.acode = DBUtils.GetDBString(dr4, 2);	//   acode
                    SDFante.axtype = DBUtils.GetDBInt32(dr4, 3); // axtype
                    SDFante.axref = DBUtils.GetDBString(dr4, 4);	// axref
                    SDFante.again = DBUtils.GetDBFloat(dr4, 5, 1);	// again
                    SDFante.abw = DBUtils.GetDBFloat(dr4, 6, 1);// // abw
                    SDFante.arms = DBUtils.GetDBInt8(dr4, 7);	// arms
                    SDFante.aband = DBUtils.GetDBString(dr4, 8);	// aband
                    SDFante.amanu = DBUtils.GetDBString(dr4, 9);	// amanu
                    SDFante.apattern = DBUtils.GetDBString(dr4, 10);	// apattern
                    SDFante.amodel = DBUtils.GetDBString(dr4, 11);	//  amodel
                    SDFante.anip = DBUtils.GetDBInt16(dr4, 12);//  anip
                    SDFante.ax0 = DBUtils.GetDBFloat(dr4, 13, 2);//  ax0
                    SDFante.adesc = DBUtils.GetDBString(dr4, 14);	// //  adesc
                    SDFante.antype = DBUtils.GetDBString(dr4, 15);	//  antype
                    SDFante.aftbr = DBUtils.GetDBFloat(dr4, 16, 1);	//  aftbr
                    SDFante.lofreq = DBUtils.GetDBDouble(dr4, 17, 2);//  lofreq
                    SDFante.hifreq = DBUtils.GetDBDouble(dr4, 18, 2);//  hifreq
                    SDFante.bandcodes = DBUtils.GetDBString(dr4, 19);//  bandcodes
                    SDFante.mdate = DBUtils.GetDBString(dr4, 20);
                    SDFante.mtime = DBUtils.GetDBString(dr4, 21);

                    if (!first)
                    {
                        swrep.WriteLine("*=======================================================================");
                    }

                    swrep.WriteLine("1," +
                                    prt_field(SDFante.cmd) +
                                    prt_field(SDFante.recstat) +
                                    prt_field(SDFante.acode) +
                                    prt_field(SDFante.axtype) +
                                    prt_field(SDFante.axref) +
                                    prt_field(SDFante.again) +
                                    prt_field(SDFante.abw) +
                                    prt_field(SDFante.arms) +
                                    prt_field(SDFante.aband) +
                                    prt_field(SDFante.amanu) +
                                    prt_field(SDFante.apattern) +
                                    prt_field(SDFante.amodel));
                    swrep.WriteLine("1," +
                                    prt_field(SDFante.cmd) +
                                    prt_field(SDFante.anip) +
                                    prt_field(SDFante.ax0) +
                                    prt_field(SDFante.adesc) +
                                    prt_field(SDFante.antype) +
                                    prt_field(SDFante.aftbr) +
                                    prt_field(SDFante.lofreq) +
                                    SDFante.hifreq);
                    swrep.WriteLine("1," +
                                    prt_field(SDFante.cmd) +
                                    prt_field(SDFante.bandcodes) +
                                    prt_field(SDFante.mdate) +
                                    SDFante.mtime);

                    // print antd details
                    if ((retval = SDFantdPrint(swrep, schema, cn_str, pdfName, SDFante.acode)) != "OK")
                    {
                        dr4.Close();
                        cn.Close();
                        return "ERROR getting antd info" + retval;
                    }
                    first = false;
                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no antenna data in file " + pdfName);
                return "OK";
            }

        }

        public static void SDFbandClear(out structSDFband SDFband)
        {
            SDFband.cmd = "";
            SDFband.recstat = "";
            SDFband.bndcde = "";
            SDFband.bandbitpos = "";
            SDFband.blo = "";
            SDFband.bmidf = "";
            SDFband.bhi = "";
            SDFband.badj = "";
            SDFband.mdate = "";
            SDFband.mtime = "";
        }
        public static string SDFbandInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFband SDFband)
        {
            string strSql;
            string bandTable = schema + ".su_" + PDFname + "_band";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + bandTable +
                     "(cmd, recstat, bndcde, bandbitpos, blo, bmidf, bhi, badj, mdate, mtime) " +
                     "VALUES " +
                     "('" + SDFband.cmd + "','" +
                     SDFband.recstat + "', " +
                     DBUtils.chNull(SDFband.bndcde.ToUpper()) + ", " +
                     DBUtils.numNull(SDFband.bandbitpos) + ", " +
                     DBUtils.numNull(SDFband.blo) + ", " +
                     DBUtils.numNull(SDFband.bmidf) + ", " +
                     DBUtils.numNull(SDFband.bhi) + ", " +
                     DBUtils.chNull(SDFband.badj.ToUpper()) + ", " +
                     DBUtils.chNull(SDFband.mdate) + ", " +
                     DBUtils.chNull(SDFband.mtime) + ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string SDFbandPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;

            string bandTable = schema + ".su_" + pdfName + "_band";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            structSDFband SDFband = new structSDFband();

            // get band info
            strSql = "SELECT cmd, recstat, bndcde, bandbitpos, blo, bmidf, bhi, " +
                     "badj, mdate, mtime " +
                     " FROM " + bandTable +
                     " ORDER BY bndcde";

            OdbcCommand select3 = new OdbcCommand(strSql);
            select3.Connection = cn;
            OdbcDataReader dr3;

            try
            {
                dr3 = select3.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            if (dr3.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* BAND SDF NAME: " + pdfName);
                swrep.WriteLine("*");

                while (dr3.Read())
                {

                    SDFbandClear(out SDFband);

                    SDFband.cmd = DBUtils.GetDBString(dr3, 0);// 0 cmd
                    SDFband.recstat = DBUtils.GetDBString(dr3, 1);// 1 recstat
                    SDFband.bndcde = DBUtils.GetDBString(dr3, 2);// 2 bndcde
                    SDFband.bandbitpos = DBUtils.GetDBInt16(dr3, 3);
                    SDFband.blo = DBUtils.GetDBDouble(dr3, 4, 2);
                    SDFband.bmidf = DBUtils.GetDBDouble(dr3, 5, 2);
                    SDFband.bhi = DBUtils.GetDBDouble(dr3, 6, 2);
                    SDFband.badj = DBUtils.GetDBString(dr3, 7);
                    SDFband.mdate = DBUtils.GetDBString(dr3, 8);
                    SDFband.mtime = DBUtils.GetDBString(dr3, 9);

                    swrep.WriteLine(prt_field(SDFband.cmd) +
                                    prt_field(SDFband.recstat) +
                                    prt_field(SDFband.bndcde) +
                                    prt_field(SDFband.bandbitpos) +
                                    prt_field(SDFband.blo) +
                                    prt_field(SDFband.bmidf) +
                                    prt_field(SDFband.bhi) +
                                    prt_field(SDFband.badj) +
                                    prt_field(SDFband.mdate) +
                                    SDFband.mtime);

                    swrep.Flush();
                }
                dr3.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr3.Close();
                cn.Close();
                swrep.WriteLine("There is no band data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFctxClear(out structSDFctx SDFctx)
        {
            SDFctx.cmd = "";
            SDFctx.recstat = "";
            SDFctx.tfcr = "";
            SDFctx.tfci = "";
            SDFctx.rxeqp = "";
            SDFctx.rqco = "";
            SDFctx.rqcull = "";
            SDFctx.rqwrst = "";
            SDFctx.ctxndp = "";
            SDFctx.ctxdesc = "";
            SDFctx.mdate = "";
            SDFctx.mtime = "";
        }
        public static bool SDFctxExists(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFctxd SDFctxd)
        {
            string strSql;
            string ctxTable = schema + ".su_" + PDFname + "_ctx_";
            bool retval = false;

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "SELECT COUNT(*) FROM " + ctxTable +
                     " WHERE tfcr = '" + SDFctxd.tfcr + "'" +
                     " AND tfci = '" + SDFctxd.tfci + "'" +
                     " AND rxeqp = '" + SDFctxd.rxeqp + "'";

            OdbcCommand count1 = new OdbcCommand(strSql, cn);
            count1.Transaction = otr;

            try
            {
                int nCount = Convert.ToInt32(count1.ExecuteScalar());
                if (nCount > 0)
                {
                    retval = true;
                }
            }
            finally
            {
            }
            return retval;
        }
        public static string SDFctxInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFctx SDFctx)
        {
            string strSql;
            string ctxTable = schema + ".su_" + PDFname + "_ctx_";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + ctxTable +
                     " (cmd, recstat, tfcr, tfci, rxeqp, rqco, rqcull, rqwrst, " +
                     " ctxndp, ctxdesc, mdate, mtime) " +
                     " VALUES " +
                     "(" +
                    DBUtils.chNull(SDFctx.cmd.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctx.recstat.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctx.tfcr.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctx.tfci.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctx.rxeqp.ToUpper()) + ", " +
                    DBUtils.numNull(SDFctx.rqco) + ", " +
                    DBUtils.numNull(SDFctx.rqcull) + ", " +
                    DBUtils.numNull(SDFctx.rqwrst) + ", " +
                    DBUtils.numNull(SDFctx.ctxndp) + ", " +
                    DBUtils.chNull(DBUtils.charQuote(SDFctx.ctxdesc.ToUpper())) + ", " +
                    DBUtils.chNull(SDFctx.mdate) + ", " +
                    DBUtils.chNull(SDFctx.mtime) + ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string SDFctxPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string retval;

            string ctx_Table = schema + ".su_" + pdfName + "_ctx_";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, tfcr, tfci, rxeqp, rqco, rqcull, rqwrst, " +
                     "ctxndp, ctxdesc, mdate, mtime " +
                     "FROM " + ctx_Table + " ORDER BY tfcr, tfci, rxeqp";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }

            structSDFctx SDFctx = new structSDFctx();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* CTX SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                swrep.Flush();

                while (dr4.Read())
                {
                    SDFctxClear(out SDFctx);

                    SDFctx.cmd = DBUtils.GetDBString(dr4, 0);	//   cmd
                    SDFctx.recstat = DBUtils.GetDBString(dr4, 1);	//   recstat
                    SDFctx.tfcr = DBUtils.GetDBString(dr4, 2);	//  tfcr
                    SDFctx.tfci = DBUtils.GetDBString(dr4, 3); // tfci
                    SDFctx.rxeqp = DBUtils.GetDBString(dr4, 4);	// rxeqp
                    SDFctx.rqco = DBUtils.GetDBFloat(dr4, 5, 1);	//  rqco
                    SDFctx.rqcull = DBUtils.GetDBFloat(dr4, 6, 1);// //  rqcull
                    SDFctx.rqwrst = DBUtils.GetDBFloat(dr4, 7, 1);	//  rqwrst
                    SDFctx.ctxndp = DBUtils.GetDBInt16(dr4, 8);	//  ctxndp
                    SDFctx.ctxdesc = DBUtils.GetDBString(dr4, 9);	//  ctxdesc
                    SDFctx.mdate = DBUtils.GetDBString(dr4, 10);
                    SDFctx.mtime = DBUtils.GetDBString(dr4, 11);

                    swrep.WriteLine("1," +
                                    prt_field(SDFctx.cmd) +
                                    prt_field(SDFctx.recstat) +
                                    prt_field(SDFctx.tfcr) +
                                    prt_field(SDFctx.tfci) +
                                    prt_field(SDFctx.rxeqp) +
                                    prt_field(SDFctx.rqco) +
                                    prt_field(SDFctx.rqcull) +
                                    prt_field(SDFctx.rqwrst) +
                                    prt_field(SDFctx.ctxndp) +
                                    prt_field(SDFctx.ctxdesc) +
                                    prt_field(SDFctx.mdate) +
                                    SDFctx.mtime);
                    swrep.Flush();

                    // print ctxd details
                    if ((retval = SDFctxdPrint(swrep, schema, cn_str, pdfName, SDFctx.tfcr, SDFctx.tfci, SDFctx.rxeqp)) != "OK")
                    {
                        dr4.Close();
                        cn.Close();
                        return "ERROR getting ctxd info" + retval;
                    }
                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no ctx data in file " + pdfName);
                return "OK";
            }

        }

        public static void SDFctxdClear(out structSDFctxd SDFctxd)
        {
            SDFctxd.cmd = "";
            SDFctxd.recstat = "";
            SDFctxd.tfcr = "";
            SDFctxd.tfci = "";
            SDFctxd.rxeqp = "";
            SDFctxd.fsep = "";
            SDFctxd.rq = "";
            SDFctxd.mdate = "";
            SDFctxd.mtime = "";
        }
        public static string SDFctxdInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFctxd SDFctxd)
        {
            string strSql;
            string ctxdTable = schema + ".su_" + PDFname + "_ctxd";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + ctxdTable +
                     " (cmd, recstat, tfcr, tfci, rxeqp, fsep, rq, mdate, mtime) " +
                     " VALUES " +
                     "(" +
                    DBUtils.chNull(SDFctxd.cmd.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctxd.recstat.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctxd.tfcr.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctxd.tfci.ToUpper()) + ", " +
                    DBUtils.chNull(SDFctxd.rxeqp.ToUpper()) + ", " +
                    DBUtils.numNull(SDFctxd.fsep) + ", " +
                    DBUtils.numNull(SDFctxd.rq) + ", " +
                    DBUtils.chNull(SDFctxd.mdate) + ", " +
                    DBUtils.chNull(SDFctxd.mtime) + ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string SDFctxdPrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string tfcr, string tfci, string rxeqp)
        {
            string strSql;

            string ctxdTable = schema + ".su_" + pdfName + "_ctxd";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, tfcr, tfci, rxeqp, fsep, rq, mdate, mtime " +
                     "FROM " + ctxdTable +
                     " WHERE tfcr = '" + tfcr +
                     "' AND tfci = '" + tfci +
                     "' AND rxeqp = '" + rxeqp + "'" +
                     " ORDER BY fsep";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }

            structSDFctxd SDFctxd = new structSDFctxd();

            if (dr4.HasRows)
            {
                while (dr4.Read())
                {
                    SDFctxdClear(out SDFctxd);

                    SDFctxd.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFctxd.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFctxd.tfcr = DBUtils.GetDBString(dr4, 2);
                    SDFctxd.tfci = DBUtils.GetDBString(dr4, 3);
                    SDFctxd.rxeqp = DBUtils.GetDBString(dr4, 4);
                    SDFctxd.fsep = DBUtils.GetDBFloat(dr4, 5, 2);
                    SDFctxd.rq = DBUtils.GetDBFloat(dr4, 6, 1);
                    SDFctxd.mdate = DBUtils.GetDBString(dr4, 7);
                    SDFctxd.mtime = DBUtils.GetDBString(dr4, 8);

                    swrep.WriteLine("2," +
                                    prt_field(SDFctxd.cmd) +
                                    prt_field(SDFctxd.recstat) +
                                    prt_field(SDFctxd.tfcr) +
                                    prt_field(SDFctxd.tfci) +
                                    prt_field(SDFctxd.rxeqp) +
                                    prt_field(SDFctxd.fsep) +
                                    prt_field(SDFctxd.rq) +
                                    prt_field(SDFctxd.mdate) +
                                    SDFctxd.mtime);
                    swrep.Flush();
                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                //swrep.WriteLine("There is no ctxd data in file " + pdfName);  // this message might mess up file to be used for import
                return "OK";
            }
        }

        public static void SDFeqptClear(out structSDFeqpt SDFeqpt)
        {
            SDFeqpt.cmd = "";
            SDFeqpt.recstat = "";
            SDFeqpt.ecode = "";
            SDFeqpt.estab = "";
            SDFeqpt.exref = "";
            SDFeqpt.emanu = "";
            SDFeqpt.emodel = "";
            SDFeqpt.edesc = "";
            SDFeqpt.etype = "";
            SDFeqpt.etraf = "";
            SDFeqpt.emission = "";
            SDFeqpt.e1stif = "";
            SDFeqpt.e2ndif = "";
            SDFeqpt.thhold = "";
            SDFeqpt.ebndcde = "";
            SDFeqpt.mdate = "";
            SDFeqpt.mtime = "";
        }
        public static string SDFeqptInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFeqpt SDFeqpt)
        {
            string strSql;
            string eqptTable = schema + ".su_" + pdfName + "_eqpt";

            strSql = "INSERT INTO " + eqptTable +
                     "(cmd, recstat, ecode, etraf, estab, emission, exref, ebndcde, etype, " +
                     "emanu, emodel, edesc, e1stif, e2ndif, thhold, " +
                     "mdate, mtime)" +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFeqpt.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.ecode.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.etraf.ToUpper()) + ", " +
                     DBUtils.numNull(SDFeqpt.estab) + ", " +
                     DBUtils.chNull(SDFeqpt.emission.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.exref.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.ebndcde.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.etype.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.emanu.ToUpper()) + ", " +
                     DBUtils.chNull(SDFeqpt.emodel.ToUpper()) + ", " +
                     DBUtils.chNull(DBUtils.charQuote(SDFeqpt.edesc.ToUpper())) + ", " +
                     DBUtils.numNull(SDFeqpt.e1stif) + ", " +
                     DBUtils.numNull(SDFeqpt.e2ndif) + ", " +
                     DBUtils.chNull(SDFeqpt.thhold) + ", " +
                     DBUtils.chNull(SDFeqpt.mdate) + ", " +
                     DBUtils.chNull(SDFeqpt.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFeqptPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string eqptTable = schema + ".su_" + pdfName + "_eqpt";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, ecode, etraf, estab, emission, exref, ebndcde, etype, " +
                    "emanu, emodel, edesc, e1stif, e2ndif, thhold, " +
                    "mdate, mtime " +
                    "FROM " + eqptTable +
                    " ORDER BY ecode";


            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFeqpt SDFeqpt = new structSDFeqpt();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* EQPT SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                while (dr4.Read())
                {
                    SDFeqptClear(out SDFeqpt);

                    SDFeqpt.cmd = DBUtils.GetDBString(dr4, 0);	//   cmd
                    SDFeqpt.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFeqpt.ecode = DBUtils.GetDBString(dr4, 2);
                    SDFeqpt.etraf = DBUtils.GetDBString(dr4, 3);	//  etraf
                    SDFeqpt.estab = DBUtils.GetDBFloat(dr4, 4, 6); //  estab
                    SDFeqpt.emission = DBUtils.GetDBString(dr4, 5); //  emission
                    SDFeqpt.exref = DBUtils.GetDBString(dr4, 6); //  exref
                    SDFeqpt.ebndcde = DBUtils.GetDBString(dr4, 7); //  ebndcde
                    SDFeqpt.etype = DBUtils.GetDBString(dr4, 8); //  etype
                    SDFeqpt.emanu = DBUtils.GetDBString(dr4, 9); //  emanu
                    SDFeqpt.emodel = DBUtils.GetDBString(dr4, 10); //  emodel
                    SDFeqpt.edesc = DBUtils.GetDBString(dr4, 11); //  edesc
                    SDFeqpt.e1stif = DBUtils.GetDBFloat(dr4, 12, 1); //  e1stif
                    SDFeqpt.e2ndif = DBUtils.GetDBFloat(dr4, 13, 1); //  e2ndif
                    SDFeqpt.thhold = DBUtils.GetDBFloat(dr4, 14, 1); //  thhold
                    SDFeqpt.mdate = DBUtils.GetDBString(dr4, 15);
                    SDFeqpt.mtime = DBUtils.GetDBString(dr4, 16);

                    swrep.WriteLine(prt_field(SDFeqpt.cmd) +
                                    prt_field(SDFeqpt.recstat) +
                                    prt_field(SDFeqpt.ecode) +
                                    prt_field(SDFeqpt.estab) +
                                    prt_field(SDFeqpt.exref) +
                                    prt_field(SDFeqpt.emanu) +
                                    prt_field(SDFeqpt.emodel) +
                                    prt_field(SDFeqpt.edesc) +
                                    prt_field(SDFeqpt.etype) +
                                    prt_field(SDFeqpt.etraf) +
                                    prt_field(SDFeqpt.emission) +
                                    prt_field(SDFeqpt.e1stif) +
                                    prt_field(SDFeqpt.e2ndif) +
                                    prt_field(SDFeqpt.thhold) +
                                    prt_field(SDFeqpt.ebndcde) +
                                    prt_field(SDFeqpt.mdate) +
                                    SDFeqpt.mtime);

                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no equipment data in file " + pdfName);
                return "OK";
            }

        }

        public static void SDFnoteClear(out structSDFnote SDFnote)
        {
            SDFnote.cmd = "";
            SDFnote.recstat = "";
            SDFnote.oper = "";
            SDFnote.nonum = "";
            SDFnote.note = "";
            SDFnote.mdate = "";
            SDFnote.mtime = "";
        }
        public static string SDFnoteInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFnote SDFnote)
        {
            string strSql;
            string noteTable = schema + ".su_" + pdfName + "_note";

            strSql = "INSERT INTO " + noteTable +
                     "(cmd, recstat, oper, nonum, note, mdate, mtime) " +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFnote.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFnote.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFnote.oper.ToUpper()) + ", " +
                     DBUtils.chNull(SDFnote.nonum.ToUpper()) + ", " +
                     DBUtils.chNull(DBUtils.charQuote(SDFnote.note.ToUpper())) + ", " +
                     DBUtils.chNull(SDFnote.mdate.ToUpper()) + ", " +
                     DBUtils.chNull(SDFnote.mtime.ToUpper()) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQL:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFnotePrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string noteTable = schema + ".su_" + pdfName + "_note";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, oper, nonum, note, " +
                     "mdate, mtime " +
                     " FROM " + noteTable +
                     " ORDER BY oper, nonum";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFnote SDFnote = new structSDFnote();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* NOTE SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                while (dr4.Read())
                {
                    SDFnoteClear(out SDFnote);

                    SDFnote.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFnote.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFnote.oper = DBUtils.GetDBString(dr4, 2);
                    SDFnote.nonum = DBUtils.GetDBString(dr4, 3);
                    SDFnote.note = DBUtils.GetDBString(dr4, 4);
                    SDFnote.mdate = DBUtils.GetDBString(dr4, 5);
                    SDFnote.mtime = DBUtils.GetDBString(dr4, 6);

                    swrep.WriteLine(prt_field(SDFnote.cmd) +
                                    prt_field(SDFnote.recstat) +
                                    prt_field(SDFnote.oper) +
                                    prt_field(SDFnote.nonum) +
                                    prt_field(SDFnote.note) +
                                    prt_field(SDFnote.mdate) +
                                    SDFnote.mtime);

                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no note data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFoperClear(out structSDFoper SDFoper)
        {
            SDFoper.cmd = "";
            SDFoper.recstat = "";
            SDFoper.oper = "";
            SDFoper.nameop = "";
            SDFoper.cooper = "";
            SDFoper.mdbm = "";
            SDFoper.addr = "";
            SDFoper.city = "";
            SDFoper.prstat = "";
            SDFoper.zippc = "";
            SDFoper.dept = "";
            SDFoper.namep = "";
            SDFoper.phonep = "";
            SDFoper.faxnum = "";
            SDFoper.telecom = "";
            SDFoper.opnote = "";
            SDFoper.admin = "";
            SDFoper.email = "";
            SDFoper.mdate = "";
            SDFoper.mtime = "";
        }
        public static string SDFoperInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFoper SDFoper)
        {
            string strSql;
            string operTable = schema + ".su_" + pdfName + "_oper";

            strSql = "INSERT INTO " + operTable +
                     " (cmd, recstat, oper, nameop, cooper, mdbm, addr, city, prstat, " +
                     "zippc, dept, namep, phonep, faxnum, telecom, opnote, admin, email, mdate ,mtime) " +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFoper.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFoper.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFoper.oper.ToUpper()) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.nameop.ToUpper())) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.cooper.ToUpper())) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.mdbm.ToUpper())) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.addr.ToUpper())) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.city.ToUpper())) + ", " +
                     DBUtils.chNull(SDFoper.prstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFoper.zippc.ToUpper()) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.dept.ToUpper())) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.namep.ToUpper())) + ", " +
                     DBUtils.chNull(SDFoper.phonep) + ", " +
                     DBUtils.chNull(SDFoper.faxnum) + ", " +
                     DBUtils.chNull(SDFoper.telecom) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.opnote)) + ", " +
                     DBUtils.chNull(dblqte(SDFoper.admin.ToUpper())) + ", " +
                     DBUtils.chNull(SDFoper.email) + ", " +
                     DBUtils.chNull(SDFoper.mdate) + ", " +
                     DBUtils.chNull(SDFoper.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQL:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFoperPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string operTable = schema + ".su_" + pdfName + "_oper";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, oper, nameop, cooper, mdbm, addr, city, prstat, " +
                     "zippc, dept, namep, phonep, faxnum, telecom, opnote, admin, email, mdate, mtime " +
                     " FROM " +
                     operTable +
                     " ORDER BY oper";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFoper SDFoper = new structSDFoper();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* OPER SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                while (dr4.Read())
                {
                    SDFoperClear(out SDFoper);

                    SDFoper.cmd = DBUtils.GetDBString(dr4, 0);	//  0 cmd
                    SDFoper.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFoper.oper = DBUtils.GetDBString(dr4, 2);
                    SDFoper.nameop = DBUtils.GetDBString(dr4, 3); //  nameop
                    SDFoper.cooper = DBUtils.GetDBString(dr4, 4); //  cooper
                    SDFoper.mdbm = DBUtils.GetDBString(dr4, 5); //  mdbm
                    SDFoper.addr = DBUtils.GetDBString(dr4, 6); //  addr
                    SDFoper.city = DBUtils.GetDBString(dr4, 7); //  city
                    SDFoper.prstat = DBUtils.GetDBString(dr4, 8); //  prstat
                    SDFoper.zippc = DBUtils.GetDBString(dr4, 9); //  zippc
                    SDFoper.dept = DBUtils.GetDBString(dr4, 10); //  dept
                    SDFoper.namep = DBUtils.GetDBString(dr4, 11); //  namep
                    SDFoper.phonep = DBUtils.GetDBString(dr4, 12); //  phonep
                    SDFoper.faxnum = DBUtils.GetDBString(dr4, 13); //  faxnum
                    SDFoper.telecom = DBUtils.GetDBString(dr4, 14); //  telecom
                    SDFoper.opnote = DBUtils.GetDBString(dr4, 15); //  opnote
                    SDFoper.admin = DBUtils.GetDBString(dr4, 16); //  admin
                    SDFoper.email = DBUtils.GetDBString(dr4, 17); //  email
                    SDFoper.mdate = DBUtils.GetDBString(dr4, 18);
                    SDFoper.mtime = DBUtils.GetDBString(dr4, 19);

                    swrep.WriteLine(prt_field(SDFoper.cmd) +
                                    prt_field(SDFoper.recstat) +
                                    prt_field(SDFoper.oper) +
                                    prt_field(SDFoper.nameop) +
                                    prt_field(SDFoper.cooper) +
                                    prt_field(SDFoper.mdbm) +
                                    prt_field(SDFoper.addr) +
                                    prt_field(SDFoper.city) +
                                    prt_field(SDFoper.prstat) +
                                    prt_field(SDFoper.zippc) +
                                    prt_field(SDFoper.dept) +
                                    prt_field(SDFoper.namep) +
                                    prt_field(SDFoper.phonep) +
                                    prt_field(SDFoper.faxnum) +
                                    prt_field(SDFoper.telecom) +
                                    prt_field(SDFoper.opnote) +
                                    prt_field(SDFoper.admin) +
                                    prt_field(SDFoper.email) +
                                    prt_field(SDFoper.mdate) +
                                    SDFoper.mtime);

                }

                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no operator data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFplanClear(out structSDFplan SDFplan)
        {
            SDFplan.cmd = "";
            SDFplan.recstat = "";
            SDFplan.sband = "";
            SDFplan.splan = "";
            SDFplan.srsp = "";
            SDFplan.srspiss = "";
            SDFplan.conform = "";
            SDFplan.uscan = "";
            SDFplan.mdate = "";
            SDFplan.mtime = "";
        }
        public static bool SDFplanExists(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFplnd SDFplnd)
        {
            string strSql;
            string planTable = schema + ".su_" + PDFname + "_plan";
            bool retval = false;

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "SELECT COUNT(*) FROM " + planTable +
                     " WHERE sband = '" + SDFplnd.sband + "'" +
                     " AND splan = '" + SDFplnd.splan + "'";

            OdbcCommand count1 = new OdbcCommand(strSql, cn);
            count1.Transaction = otr;

            try
            {
                int nCount = Convert.ToInt32(count1.ExecuteScalar());
                if (nCount > 0)
                {
                    retval = true;
                }
            }
            finally
            {
            }
            return retval;
        }
        public static string SDFplanInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFplan SDFplan)
        {
            string strSql;
            string planTable = schema + ".su_" + PDFname + "_plan";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + planTable +
                     " (cmd, recstat, sband, splan, srsp, srspiss, conform, uscan, mdate, mtime) " +
                     " VALUES " +
                     "(" +
                    DBUtils.chNull(SDFplan.cmd.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.recstat.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.sband.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.splan.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.srsp.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.srspiss.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.conform.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.uscan.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplan.mdate) + ", " +
                    DBUtils.chNull(SDFplan.mtime) +
                    ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;

            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string SDFplanPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string retval;
            string planTable = schema + ".su_" + pdfName + "_plan";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, sband, splan, srsp, srspiss, conform, uscan, mdate, mtime " +
                     " FROM " +
                     planTable +
                     " ORDER BY sband, splan";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFplan SDFplan = new structSDFplan();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* PLAN SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                while (dr4.Read())
                {
                    SDFplanClear(out SDFplan);

                    SDFplan.cmd = DBUtils.GetDBString(dr4, 0);	//  0 cmd
                    SDFplan.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFplan.sband = DBUtils.GetDBString(dr4, 2);
                    SDFplan.splan = DBUtils.GetDBString(dr4, 3);
                    SDFplan.srsp = DBUtils.GetDBString(dr4, 4);
                    SDFplan.srspiss = DBUtils.GetDBString(dr4, 5);
                    SDFplan.conform = DBUtils.GetDBString(dr4, 6);
                    SDFplan.uscan = DBUtils.GetDBString(dr4, 7);
                    SDFplan.mdate = DBUtils.GetDBString(dr4, 8);
                    SDFplan.mtime = DBUtils.GetDBString(dr4, 9);

                    swrep.WriteLine("1," +
                                    prt_field(SDFplan.cmd) +
                                    prt_field(SDFplan.recstat) +
                                    prt_field(SDFplan.sband) +
                                    prt_field(SDFplan.splan) +
                                    prt_field(SDFplan.srsp) +
                                    prt_field(SDFplan.srspiss) +
                                    prt_field(SDFplan.conform) +
                                    prt_field(SDFplan.uscan) +
                                    prt_field(SDFplan.mdate) +
                                    SDFplan.mtime);

                    // print plnd details
                    if ((retval = SDFplndPrint(swrep, schema, cn_str, pdfName, SDFplan.sband, SDFplan.splan)) != "OK")
                    {
                        dr4.Close();
                        cn.Close();
                        return "ERROR getting plnd info" + retval;
                    }

                }

                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no plan data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFplndClear(out structSDFplnd SDFplnd)
        {
            SDFplnd.cmd = "";
            SDFplnd.recstat = "";
            SDFplnd.sband = "";
            SDFplnd.splan = "";
            SDFplnd.spno = "";
            SDFplnd.set1 = "";
            SDFplnd.s1chid = "";
            SDFplnd.set2 = "";
            SDFplnd.s2chid = "";
            SDFplnd.set3 = "";
            SDFplnd.s3chid = "";
            SDFplnd.set4 = "";
            SDFplnd.s4chid = "";
            SDFplnd.mdate = "";
            SDFplnd.mtime = "";
        }
        public static string SDFplndInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string PDFname, structSDFplnd SDFplnd)
        {
            string strSql;
            string plndTable = schema + ".su_" + PDFname + "_plnd";

            //Build our SQL string to INSERT the record based on the struct values
            strSql = "INSERT INTO " + plndTable +
                     " (cmd, recstat, sband, splan, spno, set1, s1chid, set2, s2chid, " +
                     " set3, s3chid, set4, s4chid, mdate, mtime) " +
                     " VALUES " +
                     "(" +
                    DBUtils.chNull(SDFplnd.cmd.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplnd.recstat.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplnd.sband.ToUpper()) + ", " +
                    DBUtils.chNull(SDFplnd.splan.ToUpper()) + ", " +
                    DBUtils.numNull(SDFplnd.spno) + ", " +
                    DBUtils.numNull(SDFplnd.set1) + ", " +
                    DBUtils.chNull(DBUtils.charQuote(SDFplnd.s1chid.ToUpper())) + ", " +
                    DBUtils.numNull(SDFplnd.set2) + ", " +
                    DBUtils.chNull(DBUtils.charQuote(SDFplnd.s2chid.ToUpper())) + ", " +
                    DBUtils.numNull(SDFplnd.set3) + ", " +
                    DBUtils.chNull(DBUtils.charQuote(SDFplnd.s3chid.ToUpper())) + ", " +
                    DBUtils.numNull(SDFplnd.set4) + ", " +
                    DBUtils.chNull(DBUtils.charQuote(SDFplnd.s4chid.ToUpper())) + ", " +
                    DBUtils.chNull(SDFplnd.mdate) + ", " +
                    DBUtils.chNull(SDFplnd.mtime) +
                    ")";

            OdbcCommand insert3 = new OdbcCommand(strSql, cn);
            insert3.Transaction = otr;
            try
            {
                insert3.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";

        }
        public static string SDFplndPrint(StreamWriter swrep, string schema, string cn_str, string pdfName, string sband, string splan)
        {
            string strSql;

            string plndTable = schema + ".su_" + pdfName + "_plnd";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, sband, splan, spno, " +
                     "set1, s1chid, set2, s2chid, set3, s3chid, set4, s4chid, " +
                     " mdate, mtime " +
                     "FROM " + plndTable +
                     " WHERE sband = '" + sband + "' AND splan = '" + splan + "' " +
                     " ORDER BY spno";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e3)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e3.Message; ;
            }

            structSDFplnd SDFplnd = new structSDFplnd();

            if (dr4.HasRows)
            {
                while (dr4.Read())
                {
                    SDFplndClear(out SDFplnd);

                    SDFplnd.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFplnd.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFplnd.sband = DBUtils.GetDBString(dr4, 2);
                    SDFplnd.splan = DBUtils.GetDBString(dr4, 3);
                    SDFplnd.spno = DBUtils.GetDBInt8(dr4, 4);
                    SDFplnd.set1 = DBUtils.GetDBDouble(dr4, 5, 2);
                    SDFplnd.s1chid = DBUtils.GetDBString(dr4, 6);
                    SDFplnd.set2 = DBUtils.GetDBDouble(dr4, 7, 2);
                    SDFplnd.s2chid = DBUtils.GetDBString(dr4, 8);
                    SDFplnd.set3 = DBUtils.GetDBDouble(dr4, 9, 2);
                    SDFplnd.s3chid = DBUtils.GetDBString(dr4, 10);
                    SDFplnd.set4 = DBUtils.GetDBDouble(dr4, 11, 2);
                    SDFplnd.s4chid = DBUtils.GetDBString(dr4, 12);
                    SDFplnd.mdate = DBUtils.GetDBString(dr4, 13);
                    SDFplnd.mtime = DBUtils.GetDBString(dr4, 14);

                    swrep.WriteLine("2," +
                                    prt_field(SDFplnd.cmd) +
                                    prt_field(SDFplnd.recstat) +
                                    prt_field(SDFplnd.sband) +
                                    prt_field(SDFplnd.splan) +
                                    prt_field(SDFplnd.spno) +
                                    prt_field(SDFplnd.set1) +
                                    prt_field(SDFplnd.s1chid) +
                                    prt_field(SDFplnd.set2) +
                                    prt_field(SDFplnd.s2chid) +
                                    prt_field(SDFplnd.set3) +
                                    prt_field(SDFplnd.s3chid) +
                                    prt_field(SDFplnd.set4) +
                                    prt_field(SDFplnd.s4chid) +
                                    prt_field(SDFplnd.mdate) +
                                    SDFplnd.mtime);
                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                //swrep.WriteLine("There is no plnd data in file " + pdfName); // this message might mess up file to be used for import
                return "OK";
            }

        }

        public static void SDFroutClear(out structSDFrout SDFrout)
        {
            SDFrout.cmd = "";
            SDFrout.recstat = "";
            SDFrout.rcomp = "";
            SDFrout.routnumb = "";
            SDFrout.rtprov = "";
            SDFrout.rtcall = "";
            SDFrout.rtname = "";
            SDFrout.mdate = "";
            SDFrout.mtime = "";
        }
        public static string SDFroutInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFrout SDFrout)
        {
            string strSql;
            string routTable = schema + ".su_" + pdfName + "_rout";

            strSql = "INSERT INTO " + routTable +
                     " (cmd, recstat, rcomp, routnumb, rtprov, rtcall, rtname, mdate, mtime) " +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFrout.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFrout.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFrout.rcomp.ToUpper()) + ", " +
                     DBUtils.chNull(SDFrout.routnumb.ToUpper()) + ", " +
                     DBUtils.chNull(SDFrout.rtprov.ToUpper()) + ", " +
                     DBUtils.chNull(SDFrout.rtcall.ToUpper()) + ", " +
                     DBUtils.chNull(DBUtils.charQuote(SDFrout.rtname.ToUpper())) + ", " +
                     DBUtils.chNull(SDFrout.mdate) + ", " +
                     DBUtils.chNull(SDFrout.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQL:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFroutPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string routTable = schema + ".su_" + pdfName + "_rout";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, rcomp, routnumb, rtprov, rtcall, rtname, " +
                     "mdate, mtime " +
                     " FROM " + routTable +
                     " ORDER BY rcomp,routnumb";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFrout SDFrout = new structSDFrout();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* ROUTE SDF NAME: " + pdfName);
                swrep.WriteLine("*");
                swrep.Flush();
                while (dr4.Read())
                {
                    SDFroutClear(out SDFrout);

                    SDFrout.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFrout.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFrout.rcomp = DBUtils.GetDBString(dr4, 2);
                    SDFrout.routnumb = DBUtils.GetDBString(dr4, 3);
                    SDFrout.rtprov = DBUtils.GetDBString(dr4, 4);
                    SDFrout.rtcall = DBUtils.GetDBString(dr4, 5);
                    SDFrout.rtname = DBUtils.GetDBString(dr4, 6);
                    SDFrout.mdate = DBUtils.GetDBString(dr4, 7);
                    SDFrout.mtime = DBUtils.GetDBString(dr4, 8);

                    swrep.WriteLine(prt_field(SDFrout.cmd) +
                                    prt_field(SDFrout.recstat) +
                                    prt_field(SDFrout.rcomp) +
                                    prt_field(SDFrout.routnumb) +
                                    prt_field(SDFrout.rtprov) +
                                    prt_field(SDFrout.rtcall) +
                                    prt_field(SDFrout.rtname) +
                                    prt_field(SDFrout.mdate) +
                                    SDFrout.mtime);
                    swrep.Flush();

                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no route data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFtownClear(out structSDFtown SDFtown)
        {
            SDFtown.cmd = "";
            SDFtown.recstat = "";
            SDFtown.call1 = "";
            SDFtown.oper = "";
            SDFtown.twcode = "";
            SDFtown.twht = "";
            SDFtown.atwrno = "";
            SDFtown.twli = "";
            SDFtown.twpa = "";
            SDFtown.nott = "";
            SDFtown.tpoint = "";
            SDFtown.adate = "";
            SDFtown.sdate = "";
            SDFtown.mdate = "";
            SDFtown.mtime = "";
        }
        public static string SDFtownInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFtown SDFtown)
        {
            string strSql;
            string townTable = schema + ".su_" + pdfName + "_town";

            strSql = "INSERT INTO " + townTable +
                     " (cmd, recstat, call1, oper, twcode, twht, atwrno, twli, twpa, " +
                     "nott, tpoint, adate, sdate, mdate, mtime) " +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFtown.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.call1.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.oper.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.twcode.ToUpper()) + ", " +
                     DBUtils.numNull(SDFtown.twht) + ", " +
                     DBUtils.chNull(SDFtown.atwrno.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.twli.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.twpa.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.nott.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.tpoint.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtown.adate) + " , " +
                     DBUtils.chNull(SDFtown.sdate) + " , " +
                     DBUtils.chNull(SDFtown.mdate) + " , " +
                     DBUtils.chNull(SDFtown.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERROR:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFtownPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string townTable = schema + ".su_" + pdfName + "_town";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, call1, oper, twcode, twht, atwrno, twli, twpa, " +
                     "nott, tpoint, " +
                     "adate, sdate, mdate, mtime " +
                     " FROM " + townTable +
                     " ORDER BY call1, atwrno";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFtown SDFtown = new structSDFtown();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* TOWN SDF NAME: " + pdfName);
                swrep.WriteLine("*");

                while (dr4.Read())
                {
                    SDFtownClear(out SDFtown);

                    SDFtown.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFtown.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFtown.call1 = DBUtils.GetDBString(dr4, 2);
                    SDFtown.oper = DBUtils.GetDBString(dr4, 3);
                    SDFtown.twcode = DBUtils.GetDBString(dr4, 4);
                    SDFtown.twht = DBUtils.GetDBFloat(dr4, 5, 2);
                    SDFtown.atwrno = DBUtils.GetDBInt8(dr4, 6);
                    SDFtown.twli = DBUtils.GetDBString(dr4, 7);
                    SDFtown.twpa = DBUtils.GetDBString(dr4, 8);
                    SDFtown.nott = DBUtils.GetDBString(dr4, 9);
                    SDFtown.tpoint = DBUtils.GetDBString(dr4, 10);
                    SDFtown.adate = DBUtils.GetDBString(dr4, 11);
                    SDFtown.sdate = DBUtils.GetDBString(dr4, 12);
                    SDFtown.mdate = DBUtils.GetDBString(dr4, 13);
                    SDFtown.mtime = DBUtils.GetDBString(dr4, 14);

                    //swrep.WriteLine("sdate:" + DBUtils.GetDBString(dr4, 12));
                    //swrep.WriteLine("MMdate:" + DBUtils.GetDBString(dr4, 13));
                    swrep.WriteLine(prt_field(SDFtown.cmd) +
                                    prt_field(SDFtown.recstat) +
                                    prt_field(SDFtown.call1) +
                                    prt_field(SDFtown.oper) +
                                    prt_field(SDFtown.twcode) +
                                    prt_field(SDFtown.twht) +
                                    prt_field(SDFtown.atwrno) +
                                    prt_field(SDFtown.twli) +
                                    prt_field(SDFtown.twpa) +
                                    prt_field(SDFtown.nott) +
                                    prt_field(SDFtown.tpoint) +
                                    prt_field(SDFtown.adate) +
                                    prt_field(SDFtown.sdate) +
                                    prt_field(SDFtown.mdate) +
                                    SDFtown.mtime);
                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no tower note data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFtowrClear(out structSDFtowr SDFtowr)
        {
            SDFtowr.cmd = "";
            SDFtowr.recstat = "";
            SDFtowr.twcode = "";
            SDFtowr.twdesc = "";
            SDFtowr.mdate = "";
            SDFtowr.mtime = "";
        }
        public static string SDFtowrInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFtowr SDFtowr)
        {
            string strSql;
            string towrTable = schema + ".su_" + pdfName + "_towr";

            strSql = "INSERT INTO " + towrTable +
                     "(cmd, recstat, twcode, twdesc, mdate, mtime) " +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFtowr.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtowr.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtowr.twcode.ToUpper()) + ", " +
                     DBUtils.chNull(dblqte(SDFtowr.twdesc.ToUpper())) + ", " +
                     DBUtils.chNull(SDFtowr.mdate) + ", " +
                     DBUtils.chNull(SDFtowr.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFtowrPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string towrTable = schema + ".su_" + pdfName + "_towr";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, twcode, twdesc, mdate, mtime " +
                     " FROM " + towrTable +
                     " ORDER BY twcode";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFtowr SDFtowr = new structSDFtowr();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* TOWR SDF NAME: " + pdfName);
                swrep.WriteLine("*");

                while (dr4.Read())
                {
                    SDFtowrClear(out SDFtowr);

                    SDFtowr.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFtowr.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFtowr.twcode = DBUtils.GetDBString(dr4, 2);
                    SDFtowr.twdesc = DBUtils.GetDBString(dr4, 3);
                    SDFtowr.mdate = DBUtils.GetDBString(dr4, 4);
                    SDFtowr.mtime = DBUtils.GetDBString(dr4, 5);

                    swrep.WriteLine(prt_field(SDFtowr.cmd) +
                                    prt_field(SDFtowr.recstat) +
                                    prt_field(SDFtowr.twcode) +
                                    prt_field(SDFtowr.twdesc) +
                                    prt_field(SDFtowr.mdate) +
                                    SDFtowr.mtime);
                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no tower data in file " + pdfName);
                return "OK";
            }
        }

        public static void SDFtrafClear(out structSDFtraf SDFtraf)
        {
            SDFtraf.cmd = "";
            SDFtraf.recstat = "";
            SDFtraf.trafcode = "";
            SDFtraf.ecode = "";
            SDFtraf.xreftrcde = "";
            SDFtraf.xrefeqcde = "";
            SDFtraf.trdesc = "";
            SDFtraf.mdate = "";
            SDFtraf.mtime = "";
        }
        public static string SDFtrafInsert(string schema, OdbcConnection cn, OdbcTransaction otr, string pdfName, structSDFtraf SDFtraf)
        {
            string strSql;
            string trafTable = schema + ".su_" + pdfName + "_traf";

            strSql = "INSERT INTO " + trafTable +
                     "(cmd, recstat, trafcode, ecode, xreftrcde, xrefeqcde, trdesc, mdate, mtime) " +
                     "VALUES " +
                     "(" +
                     DBUtils.chNull(SDFtraf.cmd.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtraf.recstat.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtraf.trafcode.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtraf.ecode.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtraf.xreftrcde.ToUpper()) + ", " +
                     DBUtils.chNull(SDFtraf.xrefeqcde.ToUpper()) + ", " +
                     DBUtils.chNull(DBUtils.charQuote(SDFtraf.trdesc.ToUpper())) + ", " +
                     DBUtils.chNull(SDFtraf.mdate) + ", " +
                     DBUtils.chNull(SDFtraf.mtime) +
                     ")";

            OdbcCommand insert1 = new OdbcCommand(strSql, cn);
            insert1.Transaction = otr;

            try
            {
                insert1.ExecuteNonQuery();
            }
            catch (Exception e5)
            {
                //cn.Close();
                return "ERRORSQLa:" + strSql + ":" + e5.Message;
            }

            return "OK";
        }
        public static string SDFtrafPrint(StreamWriter swrep, string schema, string cn_str, string pdfName)
        {
            string strSql;
            string trafTable = schema + ".su_" + pdfName + "_traf";

            OdbcConnection cn = new OdbcConnection(cn_str);

            // try to open odbc connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swrep.WriteLine(e1.Message);
                swrep.Close();
                return "ERROR: Could not open database connection";  // could not open connection
            }

            strSql = "SELECT cmd, recstat, trafcode, ecode, xreftrcde, xrefeqcde, trdesc, " +
                     "mdate, mtime " +
                     " FROM " + trafTable +
                     " ORDER BY trafcode, ecode";

            OdbcCommand select4 = new OdbcCommand(strSql);
            select4.Connection = cn;
            OdbcDataReader dr4;

            try
            {
                dr4 = select4.ExecuteReader();
            }
            catch (Exception e4)
            {
                cn.Close();
                return "ERRORSQL:" + strSql + ":" + e4.Message;
            }

            structSDFtraf SDFtraf = new structSDFtraf();

            if (dr4.HasRows)
            {
                swrep.WriteLine("*");
                swrep.WriteLine("* TRAF SDF NAME: " + pdfName);
                swrep.WriteLine("*");

                while (dr4.Read())
                {
                    SDFtrafClear(out SDFtraf);

                    SDFtraf.cmd = DBUtils.GetDBString(dr4, 0);
                    SDFtraf.recstat = DBUtils.GetDBString(dr4, 1);
                    SDFtraf.trafcode = DBUtils.GetDBString(dr4, 2);
                    SDFtraf.ecode = DBUtils.GetDBString(dr4, 3);
                    SDFtraf.xreftrcde = DBUtils.GetDBString(dr4, 4);
                    SDFtraf.xrefeqcde = DBUtils.GetDBString(dr4, 5);
                    SDFtraf.trdesc = DBUtils.GetDBString(dr4, 6);
                    SDFtraf.mdate = DBUtils.GetDBString(dr4, 7);
                    SDFtraf.mtime = DBUtils.GetDBString(dr4, 8);

                    swrep.WriteLine(prt_field(SDFtraf.cmd) +
                                    prt_field(SDFtraf.recstat) +
                                    prt_field(SDFtraf.trafcode) +
                                    prt_field(SDFtraf.ecode) +
                                    prt_field(SDFtraf.xreftrcde) +
                                    prt_field(SDFtraf.xrefeqcde) +
                                    prt_field(SDFtraf.trdesc) +
                                    prt_field(SDFtraf.mdate) +
                                    SDFtraf.mtime);

                }
                dr4.Close();
                cn.Close();
                return "OK";
            }
            else
            {
                dr4.Close();
                cn.Close();
                swrep.WriteLine("There is no traffic data in file " + pdfName);
                return "OK";
            }
        }

        private static string dblqte(string instring)
        {
            int i;
            string outstring = "";
            char tchar;
            char qchar = '\'';

            for (i = 0; i < instring.Length; i++)
            {
                tchar = instring[i];
                if (tchar == qchar)
                {
                    outstring = outstring + "'";
                }
                outstring = outstring + tchar;
            }
            return outstring;
        }
        private void parsedate(string indate, out string outyear, out string outmonth, out string outday)
        {
            if (indate.Length == 10 && indate != "          ")
            {
                char[] datedelimiter = ".".ToCharArray();
                string[] date_parts = indate.Split(datedelimiter);
                outday = date_parts[2]; //  mDay 
                outmonth = DBUtils.txtMonth(date_parts[1]);// mMonth
                outyear = date_parts[0]; //  mYear
            }
            else
            {
                outday = ""; //  mDay 
                outmonth = "";// mMonth
                outyear = ""; //  mYear
            }
        }
        private static string prt_field(string instring)
        {
            instring = instring.Trim();

            if (instring == "")
            {
                return " ,";
            }
            else
            {
                return instring + ",";
            }
        }
        private static string itan_date(string indate)
        {
            // convert windows date (yyyy.mm.dd) to itanium date (dd MMM yyyy)
            string[] cMonthNames = new string[] { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };

            indate = indate.Trim();
            if (indate == "")
            {
                return "";
            }
            else
            {
                // split indate string into parts
                char[] delimiter = ".".ToCharArray();
                string[] dateparts = indate.Split(delimiter);
                int nMonth = Convert.ToInt32(dateparts[1]);

                return dateparts[2] + "-" + cMonthNames[nMonth - 1] + "-" + dateparts[0];
            }
        }
    }
}