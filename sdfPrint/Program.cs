using System;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Collections;
using ImpExpstructs;
using AcctngUtilities;
using DebugUtilities;

namespace sdfPrint
{
    class Program
    {
        static StreamWriter swrep;  // export file
        static string sesSchema;    // current users' schema
        static string cn_str;       // database connection string
        static string userid;       // current user id

        static string debuglogfile;      // only set if diagflag is > 0, default is "" 
        static int diagflag;        // flag to determine if debug info is to be written to log file (0 indicates NO, > 0 indicates YES)
        static StreamWriter sw;     // debugging info file


        static int Main(string[] args)
        {
            string retval;

            userid = Environment.GetEnvironmentVariable("MicsUser");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");
            string odbc = Environment.GetEnvironmentVariable("odbc");

            // get arguments passed in
            string dbase = args[0];         // database
            string out_dir = args[1];       // output directory
            string filetype = args[2];      // sdf type
            string filename = args[3];      // PDF name
            string projectCode = args[4];   // project code

            // define DB connection string
            cn_str = "DSN=" + odbc + ";DATABASE=" + dbase + ";Trusted_Connection = True";

            // this function sets the diagnostic flag for output to D:\extractlogs
            // see notes in CheckDebugSetting() below for details
            //diagflag = DebugUtils.SetDebugFlag("sdfPrint", userid, cn_str); // 0 means no diagnostics, >0 means write diagnostics
            diagflag = 1;
            debuglogfile = "";

            if (diagflag > 0)
            {
                debuglogfile = webdrive + "\\extractlogs\\" + dbase + "_" + userid + "sdfPrint-" + filetype + ".txt";
                //debuglogfile = DebugUtils.SetDebugOutput(cn_str);

                sw = new StreamWriter(debuglogfile, false);
                DebugUtils.WriteDebug(diagflag, sw, DateTime.Now.ToString());
                DebugUtils.WriteDebug(diagflag, sw, "DIAGFLAG:" + diagflag);
                DebugUtils.WriteDebug(diagflag, sw, "ENVIRONMENT");
                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                {
                    // suppress listing of password
                    if (de.Key.ToString() != "Password") DebugUtils.WriteDebug(diagflag, sw, de.Key + ":" + de.Value);
                }

                DebugUtils.WriteDebugFlush(diagflag, sw, cn_str);
                DebugUtils.WriteDebugFlush(diagflag, sw, dbase + " " + out_dir + " " + filetype + " " + filename + " " + projectCode);
            }

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    DebugUtils.WriteDebugClose(diagflag, sw, e1.Message);
                    return 2;  // could not open connection
                }
                // get default schema
                OdbcCommand getschema = new OdbcCommand("SELECT RTrim(dbo.user_schema2022('" + userid + "'))", cn);
                getschema.CommandType = CommandType.Text;

                sesSchema = (string)getschema.ExecuteScalar();
                DebugUtils.WriteDebugFlush(diagflag, sw, "New schema:" + sesSchema + ":");
            }

            // delete output file if present

            string outfile = out_dir + filename + ".txt";
            DebugUtils.WriteDebugFlush(diagflag, sw, outfile);
            try
            {
                if (File.Exists(outfile))
                {
                    File.Delete(outfile);
                }
            }
            catch (Exception ex)
            {
                DebugUtils.WriteDebugClose(diagflag, sw, ex.Message);
                return 2;
            }

            try
            {
                swrep = new StreamWriter(outfile, false);
                DebugUtils.WriteDebugFlush(diagflag, sw, outfile + " created");
            }
            catch (Exception ex)
            {
                DebugUtils.WriteDebugFlush(diagflag, sw, "failed to create " + outfile);
                DebugUtils.WriteDebugFlush(diagflag, sw, ex.Message);
            }

            try
            {
                switch (filetype)
                {
                    case "band":
                        if ((retval = ImpExp.SDFbandPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING BAND INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "ante":
                        if ((retval = ImpExp.SDFantePrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING ANTE INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "ctx":
                        if ((retval = ImpExp.SDFctxPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING CTX INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "eqpt":
                        if ((retval = ImpExp.SDFeqptPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING EQPT INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "note":
                        if ((retval = ImpExp.SDFnotePrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING NOTE INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "oper":
                        if ((retval = ImpExp.SDFoperPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING OPER INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "plan":
                        if ((retval = ImpExp.SDFplanPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING PLAN INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "rout":
                        if ((retval = ImpExp.SDFroutPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING ROUT INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "town":
                        if ((retval = ImpExp.SDFtownPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING TOWN INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "towr":
                        if ((retval = ImpExp.SDFtowrPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING TOWR INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    case "traf":
                        if ((retval = ImpExp.SDFtrafPrint(swrep, sesSchema, cn_str, filename)) != "OK")
                        {
                            DebugUtils.WriteDebugClose(diagflag, sw, "ERROR SELECTING TRAF INFO: " + retval);
                            swrep.Close();
                            return 2;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DebugUtils.WriteDebugClose(diagflag, sw, ex.Message);
                swrep.WriteLine(ex.Message);
                swrep.Close();
                return 2;
            }

            Process thisProc = Process.GetCurrentProcess();

            int intret = AcctngUtils.log_billing1(thisProc, cn_str, sesSchema, userid, projectCode, "SD_EXPORT", filename);
            if (intret != 0)
            {
                DebugUtils.WriteDebugClose(diagflag, sw, " ");
                swrep.Close();
                return 2;
            }

            DebugUtils.WriteDebugClose(diagflag, sw, " ");
            swrep.Close();
            return 0;
 
        }
       

    }
}
