
namespace sdfPrint
{
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
        public string mDay;
        public string mMonth;
        public string mYear;
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
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
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
        public string mDay;
        public string mMonth;
        public string mYear;
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
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
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
        public string mDay;
        public string mMonth;
        public string mYear;
        public string mtime;

        public structSDFnote(structSDFnote xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            oper = xx.oper;
            nonum = xx.nonum;
            note = xx.note;
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
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
        public string mDay;
        public string mMonth;
        public string mYear;
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
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
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
        public string mDay;
        public string mMonth;
        public string mYear;
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
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
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
        public string aDay;
        public string aMonth;
        public string aYear;
        public string sDay;
        public string sMonth;
        public string sYear;
        public string mDay;
        public string mMonth;
        public string mYear;
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
            aDay = xx.aDay;
            aMonth = xx.aMonth;
            aYear = xx.aYear;
            sDay = xx.sDay;
            sMonth = xx.sMonth;
            sYear = xx.sYear;
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
            mtime = xx.mtime;
        }
    }
    public struct structSDFtowr
    {
        public string cmd;
        public string recstat;
        public string twcode;
        public string twdesc;
        public string mDay;
        public string mMonth;
        public string mYear;
        public string mtime;

        public structSDFtowr(structSDFtowr xx)
        {
            cmd = xx.cmd;
            recstat = xx.recstat;
            twcode = xx.twcode;
            twdesc = xx.twdesc;
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
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
        public string mDay;
        public string mMonth;
        public string mYear;
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
            mDay = xx.mDay;
            mMonth = xx.mMonth;
            mYear = xx.mYear;
            mtime = xx.mtime;

        }

    }
 
}
