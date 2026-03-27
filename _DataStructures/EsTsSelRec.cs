using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class has fields that are isomorphic with TS PDF table TBD <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_TBD</b>
    /// </summary>
    public class EsTsSelRec
    {
        // parm
        public string protype;
        public string envtype;
        public double coordist;
        public double fsep;
        public string analopt;
        public string spherecalc;
        public double margin;

        // site
        public string terrcall1;
        public string terrcall2;
        public string earthlocation;
        public string terrname1;
        public string terrname2;
        public string earthname;
        public string terroper;
        public string terroper2;
        public string earthoper;
        public int terrlatit;
        public int terrlongit;
        public double terrgrnd;
        public int earthlatit;
        public int earthlongit;
        public double earthgrnd;
        public string radiozone;
        public short rainzone;
        public short /* a.etreport as */ siteetrep;
        public short /* a.tereport as */ siteterep;
        public int etcaseno;
        public int tecaseno;
        public int etsubcases;
        public int tesubcases;
        public string intreq;
        public double etdist;
        public double etazim;
        public double teazim;
        public double tudist;
        public double tuazim;
        public double utazim;
        public double eudist;
        public double euazim;
        public double ueazim;
        public int  /* a.processed  as */ siteproc;

        //	Antenna
        public string interferer;
        public string terrbndcde;
        public short terranum;
        public string earthcall1;
        public string earthband;
        public string terracode;
        public string earthacode;
        //ifnull(satname; '') as sname;
        public string sname;
        public string satoper;
        public int satlongit;
        public float txpre;
        public float txtro;
        public float rxpre;
        public float rxtro;
        public double sarc1;
        public double sarc2;
        public short mode1;
        public short mode2;
        public string intause;
        public short        /* b.etreport as */ anteetrep;
        public short        /* b.tereport as */ anteterep;
        public int          /* b.processed as */ anteproc;
        public int etsubcaseno;
        public int tesubcaseno;
        public double esazim;
        public double eselev;
        public double teelev;
        public double etelev;
        public double tuelev;
        public double utelev;
        public double euelev;
        public double ediscang;
        public double tdiscang;
        public double adisc_set;
        public double adisc_ute;
        public double terrht;
        public double earthht;
        public double tvazim;
        public double evazim;
        public double tvelev;
        public double evelev;
        public double tvdistes;
        public double tvdisttu;
        public double evdistes;
        public double evdisttu;
        public double angleutv;
        public double anglesev;

        //	Channels
        public string terrchid;
        public string earthchid;
        public string inttraftx;
        public string victrafrx;
        public string inteqpttx;
        public string viceqptrx;
        public double intfreqtx;
        public double vicfreqrx;
        public double freqsep;
        public double inttxpwr;
        public double inttxafls;
        public double inttxpwr2;
        public double inttxafls2;
        public double vicpwrrx;
        public double vicrxafls;
        public string stattx;
        public string statrx;
        public short        /* c.etreport as */ chanetrep;
        public short        /* c.tereport as */ chanterep;
        public string ctxinttraftx;
        public string ctxvictrafrx;
        public string ctxeqpt;
        public string calctype;
        public double earthmdsc;
        public double terrmdsc;
        public double eartheirp;
        public double terreirp;
        public double scang;
        public double loss20mode1;
        public double calci20mode1;
        public double reqd20mode1;
        public double marg20mode1;
        public double loss01mode1;
        public double calci01mode1;
        public double reqd01mode1;
        public double marg01mode1;
        public double loss01mode2;
        public double calci01mode2;
        public double reqd01mode2;
        public double marg01mode2;
        public double energy;
        public int          /* c.processed as */ chanproc;
        public short terrant;

        //	antenna again.
        public string terraxref;
        public string       /* terramodel as */ terramod;
        public double terragain;
        public string earthaxref;
        public string       /* earthamodel as */ earthamod;
        public double earthagain;
        public double   /* remterragain as */ terragain2;  // in channel

        public string tsoffaxis;
        public double tstrueaz;
        public double tstrueel;
        public double angleuta;
        public double angleeta;
        public double angleatv;
        public double adisc_atv;

        //----------------------------------------------------------------------

        // parm
        public static int PROTYPE = 2;
        public static int ENVTYPE = 9;
        public static int ANALOPT = 5;
        public static int SPHERECALC = 2;

        // site
        public static int TERRCALL1 = 10;
        public static int TERRCALL2 = 10;
        public static int EARTHLOCATION = 11;
        public static int TERRNAME1 = 33;
        public static int TERRNAME2 = 33;
        public static int EARTHNAME = 17;
        public static int TERROPER = 7;
        public static int TERROPER2 = 7;
        public static int EARTHOPER = 7;
        public static int RADIOZONE = 3;
        public static int INTREQ = 5;

        //	Antenna
        public static int INTERFERER = 2;
        public static int TERRBNDCDE = 5;
        public static int EARTHCALL1 = 10;
        public static int EARTHBAND = 5;
        public static int TERRACODE = 13;
        public static int EARTHACODE = 13;
        public static int SNAME = 17;
        public static int SATOPER = 3;
        public static int INTAUSE = 4;

        //	Channels
        public static int TERRCHID = 5;
        public static int EARTHCHID = 5;
        public static int INTTRAFTX = 7;
        public static int VICTRAFRX = 7;
        public static int INTEQPTTX = 9;
        public static int VICEQPTRX = 9;
        public static int STATTX = 2;
        public static int STATRX = 2;
        public static int CTXINTTRAFTX = 7;
        public static int CTXVICTRAFRX = 7;
        public static int CTXEQPT = 9;
        public static int CALCTYPE = 4;

        //	antenna again.
        public static int TERRAXREF = 13;
        public static int TERRAMOD = 16;
        public static int EARTHAXREF = 13;
        public static int EARTHAMOD = 16;
        public static int TSOFFAXIS = 2;








    }
}
