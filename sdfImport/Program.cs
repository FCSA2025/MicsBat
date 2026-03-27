using System;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using ImpExpstructs;
using AcctngUtilities;

namespace sdfImport
{
    class Program
    {
        static string inline;
        static StreamWriter sw;
        static char[] delimiter;
        static string[] lineparts;
        static int linenum;
        static int ErrorLevel;
        static int diagnostics;
        static string sesSchema;
        static string newname;
        static OdbcConnection cn;
        static OdbcTransaction otr;
        static structSDFante SDFante;
        static structSDFantd SDFantd;
        static structSDFband SDFband;
        static structSDFctx SDFctx;
        static structSDFctxd SDFctxd;
        static structSDFeqpt SDFeqpt;
        static structSDFnote SDFnote;
        static structSDFoper SDFoper;
        static structSDFplan SDFplan;
        static structSDFplnd SDFplnd;
        static structSDFrout SDFrout;
        static structSDFtown SDFtown;
        static structSDFtowr SDFtowr;
        static structSDFtraf SDFtraf;

        static int Main(string[] args)
        {
            delimiter = ",".ToCharArray();

            SDFante = new structSDFante();
            SDFantd  = new structSDFantd();
            SDFband = new structSDFband();
            SDFctx = new structSDFctx();
            SDFctxd = new structSDFctxd();
            SDFeqpt = new structSDFeqpt();
            SDFnote = new structSDFnote();
            SDFoper = new structSDFoper();
            SDFplan = new structSDFplan();
            SDFplnd = new structSDFplnd();
            SDFrout = new structSDFrout();
            SDFtown = new structSDFtown();
            SDFtowr = new structSDFtowr();
            SDFtraf = new structSDFtraf();

            diagnostics = 2;
            ErrorLevel = 0; // default is no errors

            // return codes are:
            // 0 - success
            // 1 - import warnings
            // 2 - import errors 

            // get process info
            Process thisProc = Process.GetCurrentProcess();

            string userid = Environment.GetEnvironmentVariable("MicsUser");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");

            // get argument values
            string dbase = args[0];         // database
            string filetype = args[1];      // subsidiary type
            newname = args[2];              // new PDF name
            string infile = args[3];        // data file to import
            string projectCode = args[4];   // project code

            string dbgfile = webdrive + "\\MicsBatchLogs\\" + userid + "sdfImport-" + filetype + "1.txt";

            StreamWriter swd = new StreamWriter(dbgfile, false);
            swd.WriteLine(args[0] + " " + args[1] + " " + args[2] + " " + args[3] + " " + args[4]);
            swd.Flush();

            //dbconnect dbinfo = new dbconnect();
            //string cn_str = dbinfo.ConnectString;

            string cn_str = "DSN=" + dbase + ";trustedconnection=true";
            
            cn = new OdbcConnection(cn_str);

            // try to open sql connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                swd.WriteLine(e1.Message);
                swd.Close();
                return 2;  // could not open connection
            }
            swd.WriteLine("Connection opened");
            swd.Flush();

            // get default schema
            OdbcCommand getschema = new OdbcCommand("SELECT RTrim(dbo.user_schema2022('" + userid + "'))", cn);
            getschema.CommandType = CommandType.Text;

            sesSchema = (string) getschema.ExecuteScalar();
            swd.WriteLine("New schema:" + sesSchema + ":");
            swd.Flush();
            swd.Close();

            cn.Close();  // will re-open with transaction control

            dbgfile = webdrive + "\\extractlogs\\" + userid + "sdfImport-" + filetype + "2.txt";

            swd = new StreamWriter(dbgfile, false);

            string outfile = infile.Replace(".tmp", ".txt");
            swd.WriteLine("infile:" + infile);
            swd.WriteLine("outfile:" + outfile);
            swd.Flush();

            // check that input file exists
            if (!File.Exists(infile))
            {
                swd.WriteLine("Could not find input file: " + infile);
                swd.Close();
                return 2;
            }
            swd.WriteLine("input file found");
            swd.Flush();

            // delete error file if present
            if (File.Exists(outfile))
            {
                File.Delete(outfile);
            }
            swd.WriteLine("Output file cleared");


            StreamReader sr = new StreamReader(infile);
            sw = new StreamWriter(outfile);
            sw.WriteLine("Processing - " + filetype + ": " + newname);
            swd.WriteLine("Processing - " + filetype + ": " + newname);
            swd.Close();

            linenum = 0;
            string retval = "";

            using (cn = new OdbcConnection(cn_str))
            {
                otr = null; // create null transaction
                cn.Open();  // open connection
                otr = cn.BeginTransaction();    // open transaction on connection
                //sw.WriteLine("Transaction opened");
                //sw.Flush();

                switch (filetype)
                {
                    case "band":
                        if ((retval = bandParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING BAND INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "ante":
                        if ((retval = anteParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING ANTE INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            cn.Close();
                            return 2;
                        }
                        break;
                    case "ctx":
                       if ((retval = ctxParse(cn_str, sr)) != "OK")
                       {
                           sw.WriteLine("ERROR INSERTING CTX INFO: " + retval);
                           sw.Flush();
                           sw.Close();
                           return 2;
                       }
                       break;
                    case "eqpt":
                        if ((retval = eqptParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING EQPT INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "note":
                        if ((retval = noteParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING NOTE INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "oper":
                        if ((retval = operParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING OPER INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "plan":
                        if ((retval = planParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING PLAN INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "rout":
                        if ((retval = routParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING ROUT INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "town":
                        if ((retval = townParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING TOWN INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;
                    case "towr":
                        if ((retval = towrParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING TOWR INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;

                    case "traf":
                        if ((retval = trafParse(cn_str, sr)) != "OK")
                        {
                            sw.WriteLine("ERROR INSERTING TRAF INFO: " + retval);
                            sw.Flush();
                            sw.Close();
                            return 2;
                        }
                        break;

                    default:
                        break;
                }
 
                int intret = 0;
                if (ErrorLevel < 2) // passed possibly with only warnings
                {
                    intret = AcctngUtils.log_billing2(thisProc, cn, otr, sesSchema, userid, projectCode, "SD_IMPORT", newname);
                }
                if (intret != 0)    // billing update failed
                {
                    ErrLevel(2);
                }

                if (diagnostics > 0)
                {
                    sw.WriteLine("ERRLEVEL:" + ErrorLevel);
                }

                if (ErrorLevel <= 1)    // possible warnings - no errors
                {
                    try
                    {
                        otr.Commit();
                        if (diagnostics > 0)
                        {
                            sw.WriteLine("Committed trans");
                            sw.Flush();
                        }
                    }
                    catch (OdbcException oe1)
                    {
                        sw.WriteLine("ERROR:COMMIT: " + oe1.Message);
                        sw.Flush();
                        ErrLevel(2);
                    }
                }
                else  // errors in import data
                {
                    if (diagnostics > 0)
                    {
                        sw.WriteLine("Rolling back trans");
                        sw.Flush();
                    }

                    try
                    {
                        otr.Rollback();
                        if (diagnostics > 0)
                        {
                            sw.WriteLine("Rolled back trans");
                            sw.Flush();
                        }
                    }
                    catch (OdbcException oe2)
                    {
                        sw.WriteLine("ERROR:ROLLBACK: " + oe2.Message);
                        sw.Flush();
                        ErrLevel(2);
                    }
                }
            }
            sw.Close();
            //cn.Close(); closed automatically by exit of using block

            return ErrorLevel;
        }
        static private string bandParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processBand();
                    }
                }
            }
            return "OK";
        }
        static private void processBand()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in band:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFbandClear(out SDFband);

            lineparts = inline.Split(delimiter);
            
            if (lineparts.Length < 10)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFband.cmd = chkstr("cmd", lineparts[0], 1);
            SDFband.recstat = chkstr("recstat", lineparts[1], 1);
            SDFband.bndcde = chkstr("bndcde", lineparts[2], 4);
            SDFband.bandbitpos = chkint("bandbitpos",lineparts[3]);
            SDFband.blo = chknum("blo",lineparts[4],2);
            SDFband.bmidf = chknum("bmidf",lineparts[5],2);
            SDFband.bhi = chknum("bhi",lineparts[6],2);
            SDFband.badj = chkstr("badj", lineparts[7], 99);
            SDFband.mdate = chkdat("mdate", lineparts[8]);
            SDFband.mtime = chktim("mtime", lineparts[9]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFbandInsert(sesSchema, cn, otr, newname, SDFband);
                if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                {
                    sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    ErrLevel(2);
                }
                else
                {
                    sw.WriteLine(retval);
                    sw.Flush();
                }
            }
        }

        static private string anteParse(string cn_str, StreamReader sr)
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in anteParse");
                sw.Flush();
            }

            int onecount = 0;
            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        if (inline.Substring(0, 1) == "1")
                        {
                            onecount++;
                            switch (onecount)
                            {
                                case 1:
                                    processAnte11();
                                    break;
                                case 2:
                                    processAnte12();
                                    break;
                                case 3:
                                    processAnte13();
                                    onecount = 0;
                                    break;
                            }
                        }
                        if (inline.Substring(0, 1) == "2") processAnte2();
                    }
                }
            }
            return "OK";
        }
        static private void processAnte11()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in ante11:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFanteClear(out SDFante);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 12)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFante.cmd = chkstr("cmd", lineparts[1], 1);
            SDFante.recstat = chkstr("recstat", lineparts[2], 1);
            SDFante.acode = chkstr("acode", lineparts[3], 12);
            SDFante.axtype = chkint("axtype",lineparts[4]);
            SDFante.axref = chkstr("axref", lineparts[5], 12);
            SDFante.again = chknum("again",lineparts[6],1);
            SDFante.abw = chknum("abw",lineparts[7],1);
            SDFante.arms = chkint("arms",lineparts[8]);
            SDFante.aband = chkstr("aband", lineparts[9], 10);
            SDFante.amanu = chkstr("amanu", lineparts[10], 10);
            SDFante.apattern = chkstr("apattern", lineparts[11], 12);
            SDFante.amodel = chkstr("amodel", lineparts[12], 15);

        }
        static private void processAnte12()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in ante12:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 9)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFante.cmd = chkstr("cmd", lineparts[1], 1);
            SDFante.anip = chkint("anip",lineparts[2]);
            SDFante.ax0 = chknum("ax0",lineparts[3],1);
            SDFante.adesc = chkstr("adesc",lineparts[4],20);
            SDFante.antype = chkstr("antype", lineparts[5], 8);
            SDFante.aftbr = chknum("aftbr",lineparts[6],1);
            SDFante.lofreq = chknum("lofreq",lineparts[7],0);
            SDFante.hifreq = chknum("hifreq",lineparts[8],0);

        }
        static private void processAnte13()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in ante13:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 5)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFante.cmd = chkstr("cmd", lineparts[1], 1);
            SDFante.bandcodes = chkstr("bandcodes", lineparts[2], 99);
            SDFante.mdate = chkdat("mdate", lineparts[3]);
            SDFante.mtime = chktim("mtime", lineparts[4]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFanteInsert(sesSchema, cn, otr, newname, SDFante);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }
        static private void processAnte2()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in ante2:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFantdClear(out SDFantd);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 12)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFantd.cmd = chkstr("cmd", lineparts[1], 1);
            SDFantd.acode = chkstr("acode", lineparts[2], 12);
            SDFantd.antang = chkantang("antang",lineparts[3],2);
            SDFantd.dcoh = chknum("dcoh",lineparts[4],1);
            SDFantd.dxph = chknum("dxph",lineparts[5],1);
            SDFantd.dcov = chknum("dcov", lineparts[6], 1);
            SDFantd.dxpv = chknum("dxpv", lineparts[7], 1);
            SDFantd.dtilt = chknum("dtilt", lineparts[8], 1);
            SDFantd.interpstat = chklong("interpstat", lineparts[9]);
            SDFantd.mdate = chkdat("mdate", lineparts[10].Trim());
            SDFantd.mtime = chktim("mtime", lineparts[11].Trim());

            if (ErrorLevel < 2)
            {
                if(!ImpExp.SDFanteExists(sesSchema, cn, otr, newname, SDFantd))
                {
                    sw.WriteLine("Error - line " + linenum.ToString() + ", No parent antenna record for this discrimination record:" + inline);
                    ErrLevel(2);
                    return;
                }

                string retval = ImpExp.SDFantdInsert(sesSchema, cn, otr, newname, SDFantd);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string ctxParse(string cn_str, StreamReader sr)
        {
            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                sw.WriteLine(inline); sw.Flush();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                         if (inline.Substring(0, 1) == "1") processCtx();
                         if (inline.Substring(0, 1) == "2") processCtxd();
                    }
                }
            }
            return "OK";
        }
        static private void processCtx()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in ctx:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFctxClear(out SDFctx);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 13)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFctx.cmd = chkstr("cmd", lineparts[1], 1);
            SDFctx.recstat = chkstr("recstat", lineparts[2], 1);
            SDFctx.tfcr = chkstr("tfcr", lineparts[3], 6);
            SDFctx.tfci = chkstr("tfci", lineparts[4], 6);
            SDFctx.rxeqp = chkstr("rxeqp", lineparts[5], 8);
            SDFctx.rqco = chknum("rqco",lineparts[6],1);
            SDFctx.rqcull = chknum("rqcull",lineparts[7],1);
            SDFctx.rqwrst = chknum("rqwrst",lineparts[8],1);
            SDFctx.ctxndp = chkint("ctxndp",lineparts[9]);
            SDFctx.ctxdesc = chkstr("recstat", lineparts[10], 40);
            SDFctx.mdate = chkdat("mdate", lineparts[11]);
            SDFctx.mtime = chktim("mtime", lineparts[12]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFctxInsert(sesSchema, cn, otr, newname, SDFctx);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }
        static private void processCtxd()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in ctxd:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFctxdClear(out SDFctxd);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 10)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFctxd.cmd = chkstr("cmd", lineparts[1], 1);
            SDFctxd.recstat = chkstr("recstat", lineparts[2], 1);
            SDFctxd.tfcr = chkstr("tfcr", lineparts[3], 6);
            SDFctxd.tfci = chkstr("tfci", lineparts[4], 6);
            SDFctxd.rxeqp = chkstr("rxeqp", lineparts[5], 8);
            SDFctxd.fsep = chknum("fsep",lineparts[6],2);
            SDFctxd.rq = chknum("rq",lineparts[7],1);
            SDFctxd.mdate = chkdat("mdate", lineparts[8]);
            SDFctxd.mtime = chktim("mtime", lineparts[9]);

            if (ErrorLevel < 2)
            {
                if (!ImpExp.SDFctxExists(sesSchema, cn, otr, newname, SDFctxd))
                {
                    sw.WriteLine("Error - line " + linenum.ToString() + ", No parent ctx record for this ctxd record:" + inline);
                    ErrLevel(2);
                    return;
                }

                string retval = ImpExp.SDFctxdInsert(sesSchema, cn, otr, newname, SDFctxd);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string eqptParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processEqpt();
                    }
                }
            }
            return "OK";
        }
        static private void processEqpt()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in eqpt:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFeqptClear(out SDFeqpt);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 17)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFeqpt.cmd = chkstr("cmd", lineparts[0], 1);
            SDFeqpt.recstat = chkstr("recstat", lineparts[1], 1);
            SDFeqpt.ecode = chkstr("ecode", lineparts[2], 8);
            SDFeqpt.estab = chknum("estab",lineparts[3], 6);
            SDFeqpt.exref = chkstr("exref", lineparts[4], 8);
            SDFeqpt.emanu = chkstr("emanu", lineparts[5], 10);
            SDFeqpt.emodel = chkstr("emodel", lineparts[6], 20);
            SDFeqpt.edesc = chkstr("edesc", lineparts[7], 32);
            SDFeqpt.etype = chkstr("etype", lineparts[8], 2);
            SDFeqpt.etraf = chkstr("etraf", lineparts[9], 6);
            SDFeqpt.emission = chkstr("emission", lineparts[10], 10);
            SDFeqpt.e1stif = chknum("e1stif",lineparts[11], 1);
            SDFeqpt.e2ndif = chknum("e2ndif",lineparts[12], 1);
            SDFeqpt.thhold = chknum("thhold",lineparts[13], 1);
            SDFeqpt.ebndcde = chkstr("ebndcde", lineparts[14], 4);
            SDFeqpt.mdate = chkdat("mdate",lineparts[15]);
            SDFeqpt.mtime = chktim("mtime",lineparts[16]);
            
            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFeqptInsert(sesSchema, cn, otr, newname, SDFeqpt);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string noteParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processNote();
                    }
                }
            }
            return "OK";
        }
        static private void processNote()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in note:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFnoteClear(out SDFnote);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 7)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFnote.cmd = chkstr("cmd", lineparts[0], 1);
            SDFnote.recstat = chkstr("recstat", lineparts[1], 1);
            SDFnote.oper = chkstr("oper", lineparts[2], 6);
            SDFnote.nonum = chkstr("nonum", lineparts[3], 4);
            SDFnote.note = chkstr("note", lineparts[4], 60);
            SDFnote.mdate = chkdat("mdate",lineparts[5]);
            SDFnote.mtime = chktim("mtime",lineparts[6]);
            
            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFnoteInsert(sesSchema, cn, otr, newname, SDFnote);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string operParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processOper();
                    }
                }
            }
            return "OK";
        }
        static private void processOper()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in oper:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFoperClear(out SDFoper);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 20)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFoper.cmd = chkstr("cmd", lineparts[0], 1);
            SDFoper.recstat = chkstr("recstat", lineparts[1], 1);
            SDFoper.oper = chkstr("oper", lineparts[2], 6);
            SDFoper.nameop = chkstr("nameop", lineparts[3], 40);
            SDFoper.cooper = chkstr("cooper", lineparts[4], 6);
            SDFoper.mdbm = chkstr("mdbm", lineparts[5], 6);
            SDFoper.addr = chkstr("addr", lineparts[6], 50);
            SDFoper.city = chkstr("city", lineparts[7], 15);
            SDFoper.prstat = chkstr("prstat", lineparts[8], 2);
            SDFoper.zippc = chkstr("zippc", lineparts[9], 10);
            SDFoper.dept = chkstr("dept", lineparts[10], 40);
            SDFoper.namep = chkstr("namep", lineparts[11],40);
            SDFoper.phonep = chkstr("phonep", lineparts[12],16);
            SDFoper.faxnum = chkstr("faxnum", lineparts[13], 16);
            SDFoper.telecom = chkstr("telecom", lineparts[14], 1);
            SDFoper.opnote = chkstr("opnote", lineparts[15], 2);
            SDFoper.admin = chkstr("admin", lineparts[16], 12);
            SDFoper.email = chkstr("email", lineparts[17], 50);
            SDFoper.mdate = chkdat("mdate", lineparts[18]);
            SDFoper.mtime = chktim("mtime", lineparts[19]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFoperInsert(sesSchema, cn, otr, newname, SDFoper);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string planParse(string cn_str, StreamReader sr)
        {
            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        if (inline.Substring(0, 1) == "1") processPlan();
                        if (inline.Substring(0, 1) == "2") processPlnd();
                    }
                }
            }
            return "OK";
        }
        static private void processPlan()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in plan:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFplanClear(out SDFplan);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 11)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFplan.cmd = chkstr("cmd", lineparts[1], 1);
            SDFplan.recstat = chkstr("recstat", lineparts[2], 1);
            SDFplan.sband = chkstr("sband", lineparts[3], 4);
            SDFplan.splan = chkstr("splan", lineparts[4], 4);
            SDFplan.srsp = chkstr("srsp", lineparts[5], 10);
            SDFplan.srspiss = chkstr("srspiss", lineparts[6], 2);
            SDFplan.conform = chkstr("conform", lineparts[7], 1);
            SDFplan.uscan = chkstr("uscan", lineparts[8], 1);
            SDFplan.mdate = chkdat("mdate", lineparts[9]);
            SDFplan.mtime = chktim("mtime", lineparts[10]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFplanInsert(sesSchema, cn, otr, newname, SDFplan);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }
        static private void processPlnd()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in plnd:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFplndClear(out SDFplnd);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 16)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            // check that spno is between 1 and 255 before trying to insert it into tinyint field
            int loc_int_spno = 0;
            Int32.TryParse(lineparts[5], out loc_int_spno);

            sw.WriteLine("Line " + linenum.ToString() + ", Spno=" + loc_int_spno);
            sw.Flush();

            if (loc_int_spno < 1 || loc_int_spno > 255)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Spno (" + loc_int_spno + ") must be between 1 and 255");
                ErrLevel(2);
                return;
            }

            SDFplnd.cmd = chkstr("cmd", lineparts[1], 1);
            SDFplnd.recstat = chkstr("recstat", lineparts[2], 1);
            SDFplnd.sband = chkstr("sband", lineparts[3], 4);
            SDFplnd.splan = chkstr("splan", lineparts[4], 4);
            SDFplnd.spno = chkint("spno", lineparts[5]);
            SDFplnd.set1 = chknum("set1", lineparts[6],2);
            SDFplnd.s1chid = chkstr("s1chid", lineparts[7],4);
            SDFplnd.set2 = chknum("set2", lineparts[8],2);
            SDFplnd.s2chid = chkstr("s2chid", lineparts[9], 4);
            SDFplnd.set3 = chknum("set3", lineparts[10],2);
            SDFplnd.s3chid = chkstr("s3chid", lineparts[11], 4);
            SDFplnd.set4 = chknum("set4", lineparts[12],2);
            SDFplnd.s4chid = chkstr("s4chid", lineparts[13], 4);
            SDFplnd.mdate = chkdat("mdate", lineparts[14]);
            SDFplnd.mtime = chktim("mtime", lineparts[15]);

            if (ErrorLevel < 2)
            {
                if (!ImpExp.SDFplanExists(sesSchema, cn, otr, newname, SDFplnd))
                {
                    sw.WriteLine("Error - line " + linenum.ToString() + ", No parent plan record for this plnd record:" + inline);
                    ErrLevel(2);
                    return;
                }

                string retval = ImpExp.SDFplndInsert(sesSchema, cn, otr, newname, SDFplnd);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string routParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processRout();
                    }
                }
            }
            return "OK";
        }
        static private void processRout()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in rout:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFroutClear(out SDFrout);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 9)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFrout.cmd = chkstr("cmd", lineparts[0], 1);
            SDFrout.recstat = chkstr("recstat", lineparts[1], 1);
            SDFrout.rcomp = chkstr("rcomp", lineparts[2], 6);
            SDFrout.routnumb = chkstr("routnumb", lineparts[3], 8);
            SDFrout.rtprov = chkstr("rtprov", lineparts[4], 2);
            SDFrout.rtcall = chkstr("rtcall", lineparts[5], 9);
            SDFrout.rtname = chkstr("rtname", lineparts[6], 48);
            SDFrout.mdate = chkdat("mdate", lineparts[7]);
            SDFrout.mtime = chktim("mtime", lineparts[8]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFroutInsert(sesSchema, cn, otr, newname, SDFrout);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string townParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processTown();
                    }
                }
            }
            return "OK";
        }
        static private void processTown()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in town:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFtownClear(out SDFtown);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 15)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFtown.cmd = chkstr("cmd", lineparts[0], 1);
            SDFtown.recstat = chkstr("recstat", lineparts[1], 1);
            SDFtown.call1 = chkstr("call1", lineparts[2], 9);
            SDFtown.oper = chkstr("oper", lineparts[3], 6);
            SDFtown.twcode = chkstr("twcode", lineparts[4], 4);
            SDFtown.twht = chknum("twht", lineparts[5],2);
            SDFtown.atwrno = chkint("atwrno", lineparts[6]);
            SDFtown.twli = chkstr("twli", lineparts[7], 1);
            SDFtown.twpa = chkstr("twpa", lineparts[8], 1);
            SDFtown.nott = chkstr("nott", lineparts[9], 4);
            SDFtown.tpoint = chkstr("tpoint", lineparts[10], 4);
            SDFtown.adate = chkdat("adate", lineparts[11]);
            SDFtown.sdate = chkdat("sdate", lineparts[12]);
            SDFtown.mdate = chkdat("mdate", lineparts[13]);
            SDFtown.mtime = chktim("mtime", lineparts[14]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFtownInsert(sesSchema, cn, otr, newname, SDFtown);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string towrParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processTowr();
                    }
                }
            }
            return "OK";
        }
        static private void processTowr()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in towr:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFtowrClear(out SDFtowr);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 6)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFtowr.cmd = chkstr("cmd", lineparts[0], 1);
            SDFtowr.recstat = chkstr("recstat", lineparts[1], 1);
            SDFtowr.twcode = chkstr("twcode", lineparts[2], 4);
            SDFtowr.twdesc = chkstr("twdesc", lineparts[3], 60);
            SDFtowr.mdate = chkdat("mdate", lineparts[4]);
            SDFtowr.mtime = chktim("mtime", lineparts[5]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFtowrInsert(sesSchema, cn, otr, newname, SDFtowr);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }

        static private string trafParse(string cn_str, StreamReader sr)
        {

            while (sr.Peek() >= 0)
            {
                inline = sr.ReadLine();
                linenum++;
                if (inline.Length > 0)
                {
                    if (inline.IndexOf("*") != 0)     // skip comment lines
                    {
                        processTraf();
                    }
                }
            }
            return "OK";
        }
        static private void processTraf()
        {
            if (diagnostics > 0)
            {
                sw.WriteLine("in traf:" + sesSchema + ":" + inline);
                sw.Flush();
            }

            ImpExp.SDFtrafClear(out SDFtraf);

            lineparts = inline.Split(delimiter);

            if (lineparts.Length < 9)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", Wrong number of fields:" + inline);
                ErrLevel(2);
                return;
            }

            SDFtraf.cmd = chkstr("cmd", lineparts[0], 1);
            SDFtraf.recstat = chkstr("recstat", lineparts[1], 1);
            SDFtraf.trafcode = chkstr("trafcode", lineparts[2], 6);
            SDFtraf.ecode = chkstr("ecode", lineparts[3], 8);
            SDFtraf.xreftrcde = chkstr("xreftrcde", lineparts[4], 6);
            SDFtraf.xrefeqcde = chkstr("xrefeqcde", lineparts[5], 8);
            SDFtraf.trdesc = chkstr("trdesc", lineparts[6],30);
            SDFtraf.mdate = chkdat("mdate", lineparts[7]);
            SDFtraf.mtime = chktim("mtime", lineparts[8]);

            if (ErrorLevel < 2)
            {
                string retval = ImpExp.SDFtrafInsert(sesSchema, cn, otr, newname, SDFtraf);
                if (retval != "OK")
                {
                    if (retval.IndexOf("Violation of PRIMARY KEY") > 0)
                    {
                        sw.WriteLine("Error - line " + linenum.ToString() + ", Duplicate key:" + inline);
                    }
                    else
                    {
                        sw.WriteLine(retval);
                        sw.Flush();
                    }
                    ErrLevel(2);
                }
            }
        }
        
        //********************************************************
        // this routine checks if input string is either empty
        // or can be parsed as a date. 
        // In the first case it returns an empty string 
        // In the latter case it returns the date in ISO format.
        // In the input cannot be parsed, in returns and enpty string and sets the error code to 2 (reject the file)
        static string chkdat(string infield, string inStr)
        {
            inStr = inStr.Trim();

            if (inStr == "")
            {
                return "";
            }

            try
            {
                DateTime dt = DateTime.Parse(inStr);
                return dt.ToString("yyyy.MM.dd");
            }
            catch
            {
                ErrLevel(2);    // error
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid date " + infield + " (" + inStr + ")");
                return "";
            }
        }

        // this routine checks if input string is either empty
        // or can be parsed as a time. 
        // In the first case it returms an empty string
        // In the latter case it returns the time in the format HH:mm 
        // If the time cannot be parsed, it returns an enpty string and sets he ErrorLevel = 2 (reject file)
        static string chktim(string infield, string inStr)
        {
            string[] timeparts;
            char[] ldelimiter;
            int hours = -1;
            int mins = -1;
            bool passed = true;

            inStr = inStr.Trim();

            ldelimiter = ":".ToCharArray();

            if (inStr == "")
            {
                return "";
            }

            if (inStr.Length > 5)   // format is hh:mm
            {
                ErrLevel(2); // Error
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid time " + infield + " (" + inStr + ")");
                return "";
            }

            if (inStr.IndexOf(":") < 1) // must be a colon but not in first position
            {
                ErrLevel(2); // Error
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid time " + infield + " (" + inStr + ")");
                return "";
            }

            timeparts = inStr.Split(ldelimiter);
            if (timeparts.Length != 2)
            {
                passed = false;
            }
            else
            {
                if (int.TryParse(timeparts[0], out hours) == false)
                {
                    passed = false;
                }

                if (int.TryParse(timeparts[1], out mins) == false)
                {
                    passed = false;
                }

                if (hours < 0 || hours > 23)
                {
                    passed = false;
                }

                if (mins < 0 || mins > 60)
                {
                    passed = false;
                }
            }

            if (passed)
            {
                return hours.ToString("00") + ":" + mins.ToString("00");
            }
            else
            {
                ErrLevel(2); // error
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid time " + infield + " (" + inStr + ")");
                return "";
            }
        }
        private static string chkint(string infield, string inStr)
        {
            int locint;

            inStr = inStr.Trim();

            if (inStr == "")
            {
                return inStr;
            }

            if (int.TryParse(inStr, out locint) == false)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid integer value " + infield + " (" + inStr + ")");
                ErrLevel(2);
                return "";
            }
            return inStr;
        }

        private static string chklong(string infield, string inStr)
        {
            long loclong;
            int locint;

            inStr = inStr.Trim();

            if (inStr == "")
            {
                return inStr;
            }

            if (long.TryParse(inStr, out loclong) == false)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid long value " + infield + " (" + inStr + ")");
                ErrLevel(2);
                return "";
            }
            else
            {
                // ok as long - still ok as int?
                if (int.TryParse(inStr, out locint) == false)
                {
                    sw.WriteLine("Error - line " + linenum.ToString() + ", invalid long value " + infield + " (" + inStr + ")");
                    locint = Convert.ToInt32(-1 * Convert.ToInt64(loclong));
                    return locint.ToString();
                }
            }
            return inStr;
        }
        private static string chkantang(string infield, string inStr, int decimals)
        {
            // this routine checks that antenna discrimination angle is between 0 and 359.99
            string sangle = chknum(infield, inStr, decimals);
            double dangle = Convert.ToDouble(sangle);
            if (dangle < 0.00 || dangle > 359.99)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid angle value " + infield + " (" + inStr + ")");
                ErrLevel(2);
                return "";
            }
            return sangle;
        }

        private static string chknum(string infield, string inStr, int decimals)
        {// changed 2010/10/5 to eliminate number of decimal checks 
            //string[] numparts;
            //char[] ldelimiter;
            //string format = "";
            double locdbl;
            
            inStr = inStr.Trim();

            if (inStr == "")
            {
                return inStr;
            }

            if (double.TryParse(inStr, out locdbl) == false)
            {
                sw.WriteLine("Error - line " + linenum.ToString() + ", invalid numeric value " + infield + " (" + inStr + ")");
                ErrLevel(2);
                return "";
            }

            return inStr;

            //ldelimiter = ".".ToCharArray();
            //numparts = inStr.Split(ldelimiter);
            //if(numparts.Length > 1)
            //{
            //    if (numparts[1].Length > decimals)
            //    {
            //        sw.WriteLine("Warning - line " + linenum.ToString() + ", value (" + inStr + ") for " + infield +
            //                    " will be rounded to " + decimals.ToString() + " decimal(s)");
            //        ErrLevel(1);
            //    }
            //}

            //switch (decimals)
            //{
            //    case 0:
            //        format = "#############0";
            //        break;
            //    case 1:
            //        format = "#############0.0";
            //        break;
            //    case 2:
            //        format = "#############0.00";
            //        break;
            //    case 3:
            //        format = "#############0.000";
            //        break;
            //    case 4:
            //        format = "#############0.0000";
            //        break;
            //    case 5:
            //        format = "#############0.00000";
            //        break;
            //    case 6:
            //        format = "#############0.000000";
            //        break;
            //    default:
            //        sw.WriteLine("Invalid number of decimals:" + decimals.ToString());
            //        ErrLevel(2);
            //        break;
            //}

            //return locdbl.ToString(format);

        }
        private static string chkstr(string infield, string inStr, int maxlen)
        {
            if(inStr.Trim().Length > maxlen)
            {
                sw.WriteLine("Warning - line " + linenum.ToString() + ", value (" + inStr + ") for " + infield + 
                            " will be truncated to (" + inStr.Substring(0,maxlen) + ")");
                ErrLevel(1);    // warning
                return inStr.Substring(0,maxlen);
            }
            else
            {
                return inStr.Trim();
            }
        }
        private static void ErrLevel(int inLev)
        {
            if (inLev > ErrorLevel)
            {
                ErrorLevel = inLev;
            }
        }


    }
}
