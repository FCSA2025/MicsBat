using System;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using System.Diagnostics;
using AcctngUtilities;
using DBAccess;


namespace sdfValidate
{
    class Program
    {
        static StreamWriter sw;
        static StreamWriter swrep;
        static string sesSchema;
        static int ErrorCount;
        static int WarningCount;
        static string dbase;
        static string key_text;
        static string cn_str;
        static int diagflag;        // flag to determine if debug info is to be written to log file (0 indicates NO, > 0 indicates YES)
        static string ProgName;     // program name ("sdfValidate" in this case)
        static string userid;       // current user id
        static string logfile;      // only set if diagflag is > 0, default is "" 



        static string EQPTANAL = "A ";
        static string EQPTDIGI = "D ";
        static int ANTEAXTYPE_LO = 0;
        static int ANTEAXTYPE_HI = 10;
        static float ANTEABW_LO = 0.0f;
        static float ANTEABW_HI = 99.91f;
        static int ANTEARMS_LO = 0;
        static int ANTEARMS_HI = 9;
        static float ANTEAFTBR_LO = 0.0f;
        static float ANTEAFTBR_HI = 99.91f;
        static float ANTEAX0_LO = 0.0f;
        static float ANTEAX0_HI = 99.91f;
        static float ANTEAGAIN_LO = 0.0f;
        static float ANTEAGAIN_HI = 99.91f;
        static float ANTEANTANG_LO = 0.0f;
        static float ANTEANTANG_HI = 360.0f;
        static float ANTEDISCR_LO = 0.0f;
        static float ANTEDISCR_HI = 99.91f;
        static float ANTEDTILT_LO = 0.0f;
        static float ANTEDTILT_HI = 99.99f;
        static int MAX_ADJ_BANDS = 20;
        static double BANDFREQ_LO = 700;
        static double BANDFREQ_HI = 100000;
        static int BANDBITPOS_LO = 1;
        static int BANDBITPOS_HI = 128;
        static float CTXRQCO_LO = -200.0f;
        static float CTXRQCO_HI = 200.0f;
        static float CTXRQCULL_LO = -200.0f;
        static float CTXRQCULL_HI = 200.0f;
        static float CTXRQWRST_LO = -200.0f;
        static float CTXRQWRST_HI = 200.0f;
        static float CTXFSEP_LO = 0.0f;
        static float CTXFSEP_HI = 400000.00f;
        static float CTXRQ_LO = -200.0f;
        static float CTXRQ_HI = 200.0f;
        static float ESTAB_LO = 0.0f;
        static float ESTAB_HI = 1.0f;
        static float E1STIF_LO = 0.0f;
        static float E1STIF_HI = 1000.0f;
        static float E2NDIF_LO = 0.0f;
        static float E2NDIF_HI = 1000.0f;
        static int MAXPLANPTS = 20;
        static float THHOLD_LO = -200.0f;
        static float THHOLD_HI = 0.0f;
        //static int ROUTNUMB_LO = 0;
        //static int ROUTNUMB_HI = 9999;
        static int ATWRNO_LO = 0;
        static int ATWRNO_HI = 9;

        static int Main(string[] args)
        {
            string retval;
            string valid = "";
            int file_ntype = -1;
            ErrorCount = 0;
            WarningCount = 0;
            key_text = "";

            ProgName = "sdfValidate";
            userid = Environment.GetEnvironmentVariable("MicsUser");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");
            string odbc = Environment.GetEnvironmentVariable("odbc");

            // get arguments passed in
            dbase = args[0];         // database
            string out_dir = args[1];       // output directory
            string filetype = args[2];      // sdf type
            string filename = args[3];      // PDF name
            string projectCode = args[4];   // project code

            // define DB connection string
            cn_str = "DSN=" + odbc + ";DATABASE=" + dbase + ";Trusted_Connection = True";

            // this function sets the diagnostic flag for output to D:\extractlogs
            // see notes in CheckDebugSetting() below for details
            diagflag = CheckDebugSetting(); // 0 means no diagnostics, >0 means write diagnostics

            logfile = "";

            if (diagflag > 0)
            {
                logfile = webdrive + "\\extractlogs\\" + dbase + "_" + userid + "sdfValidate-" + filetype + ".txt";
                sw = new StreamWriter(logfile, false);
                WriteDebug("ARGS:" + dbase + " " + out_dir + " " + filetype + " " + filename + " " + projectCode);
                WriteDebug("DIAGFLAG:" + diagflag);
                WriteDebug(userid + " " + userid);
                WriteDebugFlush("ENVIRONMENT");

                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                {
                    if (de.Key.ToString() != "Password") WriteDebug(de.Key + ":" + de.Value);
                }
                WriteDebug(" ");

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
                    WriteDebugClose(e1.Message);
                    return 2;  // could not open connection
                }

                try
                {
                    // get default schema
                    OdbcCommand getschema = new OdbcCommand("SELECT RTrim(dbo.user_schema2022('" + userid + "'))", cn);
                    getschema.CommandType = CommandType.Text;

                    sesSchema = (string)getschema.ExecuteScalar();
                    WriteDebugFlush("New schema:" + sesSchema + ":");
                    cn.Close();
                }
                catch (Exception ex)
                {
                    WriteDebugClose(ex.Message);
                    cn.Close();
                    return 18;
                }

                // delete output file if present
                string outfile = out_dir + filename + ".txt";
                WriteDebugFlush(outfile);
                try
                {
                    if (File.Exists(outfile))
                    {
                        File.Delete(outfile);
                    }
                }
                catch (Exception ex)
                {
                    WriteDebugClose(ex.Message);
                    cn.Close();
                    return 19;
                }

                swrep = new StreamWriter(outfile, false);

                switch (filetype)
                {
                    case "band":
                        file_ntype = 300;
                        write_header("Band", filename);

                        swrep.Flush();
                        if ((retval = SDFbandValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            return 2;
                        }
                        break;
                    case "ante":
                        file_ntype = 301;
                        write_header("Antenna", filename);

                        if ((retval = SDFanteValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "ctx":
                        file_ntype = 303;
                        write_header("Ctx", filename);

                        if ((retval = SDFctxValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "eqpt":
                        file_ntype = 305;
                        write_header("Equipment", filename);

                        if ((retval = SDFeqptValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "note":
                        file_ntype = 306;
                        write_header("Note", filename);

                        if ((retval = SDFnoteValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "oper":
                        file_ntype = 308;
                        write_header("Operator", filename);

                        if ((retval = SDFoperValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "plan":
                        file_ntype = 310;
                        write_header("Plan", filename);

                        if ((retval = SDFplanValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "rout":
                        file_ntype = 309;
                        write_header("Route", filename);

                        if ((retval = SDFroutValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "town":
                        file_ntype = 313;
                        write_header("Tower Note", filename);

                        if ((retval = SDFtownValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "towr":
                        file_ntype = 312;
                        write_header("Tower", filename);

                        if ((retval = SDFtowrValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;

                    case "traf":
                        file_ntype = 314;
                        write_header("Traffic", filename);

                        if ((retval = SDFtrafValid(sesSchema, filename)) != "OK")
                        {
                            WriteDebugClose("Error - no records found in SDF " + filename + " - program terminating");
                            sw.Close();
                            swrep.Close();
                            return 2;
                        }
                        break;
                    default:
                        break;
                }

                WriteDebug("");
                WriteDebugFlush("USER TABLE: " + sesSchema + "-" + file_ntype.ToString() + '-' + filename);

                // update validstat flag on web.user_tables
                //SQL:VIEW:web.user_tables_view
                valid = "Y";
                if (ErrorCount > 0)
                {
                    valid = "N";
                }
                UserTable uTable = new UserTable(sesSchema, file_ntype, filename);
                if (uTable.m_operator == "")
                {
                    WriteDebug("Cannot find entry in web.user_tables_view");
                }
                else
                {
                    WriteDebug(uTable.m_operator + "-" + uTable.tabletype.ToString() + "-" + uTable.file_name);

                    bool xx = true;
                    try
                    {
                        xx = UserTable.SetUserValidFlag(uTable, valid);
                    }
                    catch (Exception ex)
                    {
                        WriteDebug(ex.Message);
                    }

                    if (!xx)
                    {
                        WriteDebug("Error updating valid status");
                    }
                    else
                    {
                        WriteDebug("Success updating valid status");
                    }
                }

                Process thisProc = Process.GetCurrentProcess();
                int intret = AcctngUtils.log_billing1(thisProc, cn_str, sesSchema, userid, projectCode, filetype + "SD_VAL", filename);
                if (intret != 0)
                {
                    WriteDebugClose("Error writing billing info" + intret.ToString());
                    swrep.Close();
                    return 2;
                }
                else
                {
                    WriteDebug("OK writing billing info" + intret.ToString());
                    swrep.Close();
                    return 0;
                }
            } 
        }

        private static string SDFbandValid(string schema, string filename)
        {
            string retval = "";
            string bandTable = schema + ".su_" + filename + "_band";

            try
            {
                DataTable oDT = Band.BandKeys(bandTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "BAND KEY: " + oDR["bndcde"].ToString();

                        retval = ValidateBand(oDR["bndcde"].ToString(), bandTable);
                        WriteDebug(oDR["bndcde"].ToString() + ":" + retval);
                    }
                    write_status("Band", filename);

                }
                else
                {
                    write_msg("E", "- There are no bands in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateBand(string sbndcde, string bandTable)
        {
            string retval = "OK";

            Band testBand = new Band(bandTable, sbndcde);
            switch (testBand.cmd)
            {
                case "A":  // add new record
                    retval = ValidateBandA(testBand, bandTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateBandD(testBand, bandTable);
                    break;

                case "U": // update existing record
                    retval = ValidateBandU(testBand, bandTable);
                    break;

                case "N": // no change
                    retval = ValidateBandN(testBand, bandTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testBand.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateBandA(Band testBand, string bandTable)
        {
            string retval = "OK";

            Band sdBand = new Band("", testBand.bndcde);

            if (sdBand.bndcde == testBand.bndcde)
            {
                write_msg("E", "- Record with this band code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateBandFields(testBand, bandTable);

            return retval;
        }
        private static string ValidateBandD(Band testBand, string bandTable)
        {
            string retval = "OK";

            Band sdBand = new Band("", testBand.bndcde);

            if (sdBand.bndcde == "")
            {
                write_msg("E", "- Record with this band code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testBand.mdate.Trim() != sdBand.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testBand.mdate.Trim() + ") differs from date of original record (" + sdBand.mdate.Trim() + ")");
                retval = "ERROR";
            }
 
            if (testBand.mtime.Trim() != sdBand.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testBand.mtime.Trim() + ") differs from time of original record (" + sdBand.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateBandN(Band testBand, string bandTable)
        {
            string retval = "OK";
            string retval2 = "";
            Band sdBand = new Band("", testBand.bndcde);

            if (sdBand.bndcde == "")
            {
                write_msg("E", "- Record with this band code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdBand.cmd = "N";
                sdBand.recstat = "U";
                if ((retval2 = Band.BandUpdate(sdBand, bandTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }
            return retval;
        }
        private static string ValidateBandU(Band testBand, string bandTable)
        {
            string retval = "OK";

            Band sdBand = new Band("", testBand.bndcde);
            swrep.Flush();

            if (sdBand.bndcde == "")
            {
                write_msg("E", "- Record with this band code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testBand.mdate.Trim() != sdBand.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testBand.mdate.Trim() + ") differs from date of original record (" + sdBand.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testBand.mtime.Trim() != sdBand.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testBand.mtime.Trim() + ") differs from time of original record (" + sdBand.mtime.Trim() + ")");
                retval = "ERROR";
            }

            // check if band bit position matches record to be updated
            if (testBand.bandbitpos != sdBand.bandbitpos)
            {
                write_msg("E", "- Band Bit Pos (" + testBand.bandbitpos + ") differs from value of original record (" + sdBand.bandbitpos + ")");
                retval = "ERROR";
            }

            swrep.Flush();

            retval = ValidateBandFields(testBand, bandTable);

            if (retval == "OK")
            {
                //testBand.bandbitpos = sdBand.bandbitpos;  // load value from SDB
                //testBand.blo = update_double(testBand.blo,sdBand.blo);
                //testBand.bmidf = update_double(testBand.bmidf,sdBand.bmidf);
                //testBand.bhi = update_double(testBand.bhi,sdBand.bhi);
                //testBand.badj = update_string(testBand.badj,sdBand.badj);
                //testBand.mdate = update_string(testBand.mdate,sdBand.mdate);
                //testBand.mtime = update_string(testBand.mtime,sdBand.mtime);
            }
            return retval;
        }
        private static string ValidateBandFields(Band testBand, string bandTable)
        {
            string retval = "OK";

            int missing = 0;

            // check for empty fields
            if (testBand.cmd == "")
            {
                write_msg("E", "- Missing value for SDB Operation (cmd)");
                missing ++;
            }

            if(!testBand.v_blo.HasValue)
            {
                write_msg("E", "- Missing value for Low Edge Frequency (blo)");
                missing ++;
            }

            if (!testBand.v_bmidf.HasValue)
            {
                write_msg("E", "- Missing value for Midband Frequency (bmidf)");
                missing ++;
            }

            if (!testBand.v_bhi.HasValue)
            {
                write_msg("E", "- Missing value for High Edge Frequency (bhi)");
                missing ++;
            }

            if(testBand.badj == "")
            {
                write_msg("E", "- Missing value for Adjacent Bands (badj)");
                missing ++;
            }

            if(missing > 0)
            {
                write_msg("E", "- All required fields not entered");
                return "ERROR";
            }

            //strcpy(titleLine, "\nINTERNAL VALIDATION");

            // check bandbitpos
            if (testBand.v_bandbitpos.HasValue)
            {
                if (testBand.v_bandbitpos.Value < BANDBITPOS_LO || testBand.v_bandbitpos.Value > BANDBITPOS_HI)
                {
                    write_msg("E", "- Band position number (" + testBand.v_bandbitpos.Value + ") must be between " + BANDBITPOS_LO + " and " + BANDBITPOS_HI);
                    retval = "ERROR";
                }
            }

            /*  low frequency for band */
            if ((testBand.v_blo.Value / 1000 < BANDFREQ_LO) || (testBand.v_blo.Value / 1000 > BANDFREQ_HI))
            {
                write_msg("E", "- BLO (" + testBand.v_blo.Value / 1000 + ") must be between " + BANDFREQ_LO + " and " + BANDFREQ_HI + " MHz");
                retval = "ERROR";
            }
   
            /* mid range frequency for band */
            if ((testBand.v_bmidf.Value / 1000 < BANDFREQ_LO) || (testBand.v_bmidf.Value / 1000 > BANDFREQ_HI))
            {
                write_msg("E", "- BMIDF (" + testBand.v_bmidf.Value / 1000 + ") must be between " + BANDFREQ_LO + " and " + BANDFREQ_HI + " MHz");
                retval = "ERROR";
            }
     
            /* high frequency for band */
            if ((testBand.v_bhi.Value / 1000 < BANDFREQ_LO) || (testBand.v_bhi.Value / 1000 > BANDFREQ_HI))
            {
                write_msg("E", "- BHI (" + testBand.v_bhi.Value / 1000 + ") must be between " + BANDFREQ_LO + " and " + BANDFREQ_HI + " MHz");
                retval = "ERROR";
            }
        
            // check low vs mid frequencies
            if (testBand.v_blo.Value >= testBand.v_bmidf.Value)
            {
                write_msg("E", "- BLO (" + testBand.v_blo.Value + ") must be less than BMIDF (" + testBand.v_bmidf.Value + ")");
                retval = "ERROR";
            }

            // check mid vs high frequencies
            if (testBand.v_bmidf.Value >= testBand.v_bhi.Value)
            {
                write_msg("E", "- BMIDF (" + testBand.v_bmidf.Value + ") must be less than BHI (" + testBand.v_bhi.Value + ")");
                swrep.Flush();
                retval = "ERROR";
            }

            // check adjacent bands
            char[] delimiter = ";".ToCharArray();
            string[] adj_bands = testBand.badj.Trim().Split(delimiter);

            if(adj_bands.Length > MAX_ADJ_BANDS)
            {
                write_msg("E", "- " + adj_bands.Length.ToString() + " adjacent bands exceed max of " + MAX_ADJ_BANDS.ToString() + " allowed");
                swrep.Flush();
                retval = "ERROR";
            }

            for (int i = 0; i < adj_bands.Length; i++)
            {
                if (!Band.UnionBandChk(adj_bands[i], bandTable))
                {
                    write_msg("E", "- Invalid adjacent band (" + adj_bands[i] + ")");
                     retval = "ERROR";
                }
            }
            return retval;
        }

        private static string SDFanteValid(string schema, string filename)
        {
            string retval = "";
            string anteTable = schema + ".su_" + filename + "_ante";
            string sacode = "";

            try
            {
                DataTable oDT = Antenna.AntennaKeys(anteTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "ANTENNA KEY: " + oDR["acode"].ToString();
                        sacode = oDR["acode"].ToString();

                        retval = ValidateAnte(oDR["acode"].ToString(), anteTable);
                     }
                     write_status("Antenna", filename);
                }
                else
                {
                    write_msg("E", "- There are no antennas in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                // this exception is tripped if there was a system error in AntennaKeys 
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateAnte(string sacode, string anteTable)
        {
            string retval = "OK";

            Antenna testAnte = new Antenna(anteTable, sacode);
            switch (testAnte.cmd)
            {
                case "A":  // add new record
                    retval = ValidateAnteA(testAnte, anteTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateAnteD(testAnte, anteTable);
                    break;

                case "U": // update existing record
                    retval = ValidateAnteU(testAnte, anteTable);
                    break;

                case "N": // no change
                    retval = ValidateAnteN(testAnte, anteTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testAnte.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateAnteA(Antenna testAnte, string anteTable)
        {
            string retval = "OK";

            Antenna sdAnte = new Antenna("", testAnte.acode);
            if (sdAnte.acode == testAnte.acode)
            {
                write_msg("E", "- Record with this antenna code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateAnteFields(testAnte, anteTable);
            if (retval == "ERROR")
            {
                return retval;
            }

            // validate related antd records
            retval = SDFantdValid(anteTable, testAnte.acode, testAnte.anip);
            return retval;
        }
        private static string ValidateAnteD(Antenna testAnte, string anteTable)
        {
            string retval = "OK";

            Antenna sdAnte = new Antenna("", testAnte.acode);
            if (sdAnte.acode == "")
            {
                write_msg("E", "- Record with this antenna code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted
 
            if (testAnte.mdate.Trim() != sdAnte.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testAnte.mdate.Trim() + ") differs from date of original record (" + sdAnte.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testAnte.mtime.Trim() != sdAnte.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testAnte.mtime.Trim() + ") differs from time of original record (" + sdAnte.mtime.Trim() + ")");
                retval = "ERROR";
            }

            if (retval == "ERROR")
            {
                return retval;
            }

            // check related antd records
            retval = SDFantdValid(anteTable, testAnte.acode, testAnte.anip);
            
            return retval;
        }
        private static string ValidateAnteN(Antenna testAnte, string anteTable)
        {
            string retval = "OK";
            string retval2 = "";

            Antenna sdAnte = new Antenna("", testAnte.acode);
            if (sdAnte.acode == "")
            {
                write_msg("E", "- Record with this antenna code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdAnte.cmd = "N";
                sdAnte.recstat = "U";
                if ((retval2 = Antenna.AnteUpdate(sdAnte, anteTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            if (retval == "ERROR")
            {
                return retval;
            }

            // check related antd records
            retval = SDFantdValid(anteTable, testAnte.acode, testAnte.anip);

            return retval;
        }
        private static string ValidateAnteU(Antenna testAnte, string anteTable)
        {
            string retval = "OK";

            Antenna sdAnte = new Antenna("", testAnte.acode);
            if (sdAnte.acode == "")
            {
                write_msg("E", "- Record with this antenna code does not exist in SDB");
                return "ERROR";
            }

            retval = ValidateAnteFields(testAnte, anteTable);
            if (retval == "ERROR")
            {
                return retval;
            }

            // check if date and time match record to be updated

            if (testAnte.mdate.Trim() != sdAnte.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testAnte.mdate.Trim() + ") differs from date of original record (" + sdAnte.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testAnte.mtime.Trim() != sdAnte.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testAnte.mtime.Trim() + ") differs from time of original record (" + sdAnte.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = SDFantdValid(anteTable, testAnte.acode, testAnte.anip);
            
            return retval;
        }
        private static string ValidateAnteFields(Antenna testAnte, string anteTable)
        {
            string retval = "OK";

            int missing = 0;

            // check for empty fields
            if (testAnte.cmd == "")
            {
                write_msg("E", "- Missing value for SDB Operation (cmd)");
                missing++;
            }

            if (!testAnte.v_again.HasValue)
            {
                write_msg("E", "- Missing value for Antenna Gain (again)");
                missing++;
            }

            if (!testAnte.v_abw.HasValue)
            {
                write_msg("E", "- Missing value for Beamwidth (abw)");
                missing++;
            }

            if (!testAnte.v_arms.HasValue)
            {
                write_msg("E", "- Missing value for Peak RMS Factor (bhi)");
                missing++;
            }

            if (testAnte.aband == "")
            {
                write_msg("E", "- Missing value for Frequency Band (aband)");
                missing++;
            }

            if (testAnte.amanu == "")
            {
                write_msg("E", "- Missing value for Manufacturer (amanu)");
                missing++;
            }

            if (testAnte.apattern == "")
            {
                write_msg("E", "- Missing value for Pattern Number (apattern)");
                missing++;
            }

            if (testAnte.adesc == "")
            {
                write_msg("E", "- Missing value for Antenna Description (adesc)");
                missing++;
            }

            if (testAnte.antype == "")
            {
                write_msg("E", "- Missing value for Antenna Type (antype)");
                missing++;
            }

            if (!testAnte.v_aftbr.HasValue)
            {
                write_msg("E", "- Missing value for Front to Back Ratio (aftbr)");
                missing++;
            }

            if (testAnte.amodel == "")
            {
                write_msg("E", "- Missing value for Antenna Model (amodel)");
                missing++;
            }

            if (!testAnte.v_ax0.HasValue)
            {
                write_msg("E", "- Missing value for AX0 (ax0)");
                missing++;
            }

            if (missing > 0)
            {
                write_msg("E", "- All required fields not entered");
                return "ERROR";
            }

	        // check if temp antenna
            if(testAnte.acode.IndexOf("$") == 0)
            {
                write_msg("E", "- Temporary antenna codes not allowed");
                retval = "ERROR";
            }

            if (testAnte.v_axtype.HasValue)
            {
                if (testAnte.axtype < ANTEAXTYPE_LO || testAnte.axtype > ANTEAXTYPE_HI)
                {
                    write_msg("E", "- Cross reference type (" + testAnte.axtype + ") must be between " + ANTEAXTYPE_LO + " and " + ANTEAXTYPE_HI);
                    retval = "ERROR";
                }
            }

            if (testAnte.v_again.Value < ANTEAGAIN_LO || testAnte.v_again.Value > ANTEAGAIN_HI) 
            {
                write_msg("E", "- Gain (" + testAnte.v_again.Value + ") must be in the range of " + ANTEAGAIN_LO + " to " + ANTEAGAIN_HI);
                retval = "ERROR";
            }

            if(testAnte.axref.IndexOf("$") == 0)
            {
                write_msg("E", "- Temporary cross reference antenna codes not allowed");
                retval = "ERROR";
            }

            // check if axref in SDB or current file
            if (testAnte.axref != "")
            {
                if (!Antenna.UnionAnteChk(testAnte.axref, anteTable))
                {
                    write_msg("E", "- Cross Reference antenna (" + testAnte.axref.Trim() + ") not found in the SDB or the SDF");
                    retval = "ERROR";
                }
            } 
            
            if (testAnte.v_abw.Value < ANTEABW_LO || testAnte.v_abw.Value > ANTEABW_HI) 
            {
                write_msg("E", "- Band width (" + testAnte.v_abw.Value + ") must be in the range of " + ANTEABW_LO + " to " + ANTEABW_HI);
                retval = "ERROR";
            }

			if (testAnte.v_arms.Value < ANTEARMS_LO || testAnte.v_arms.Value > ANTEARMS_HI) 
            {
				//printf("%s\t%s\t:%d: - Peak RMS factor must be in the range of %d to %d\n", keyLine, "E", anteTransRec.arms, ANTEARMS_LO, ANTEARMS_HI);
                write_msg("E", "- Peak RMS factor (" + testAnte.v_arms.Value + ") must be in the range of " + ANTEARMS_LO + " to " + ANTEARMS_HI);
                retval = "ERROR";
            }

            if (testAnte.v_aftbr.Value < ANTEAFTBR_LO || testAnte.v_aftbr.Value > ANTEAFTBR_HI) 
            {
				//printf("%s\t%s\t:%.2f: - Front to back ratio must be in the range of %.1f to %.1f\n", keyLine, "E", anteTransRec.aftbr, ANTEAFTBR_LO, ANTEAFTBR_HI);
                write_msg("E", "- Front to back ratio (" + testAnte.v_aftbr.Value + ") must be in the range of " + ANTEAFTBR_LO + " to " + ANTEAFTBR_HI);
                retval = "ERROR";
            }

            if (testAnte.v_ax0.Value < ANTEAX0_LO || testAnte.v_ax0.Value > ANTEAX0_HI) 
            {
				//printf("%s\t%s\t:%.2f: - AX0 must be in the range of %.1f to %.1f\n", keyLine, "E", anteTransRec.ax0, ANTEAX0_LO, ANTEAX0_HI);
                write_msg("E", "- AX0 (" + testAnte.v_ax0.Value + ") must be in the range of " + ANTEAX0_LO + " to " + ANTEAX0_HI);
                retval = "ERROR";
            }

           return retval;
        }

        private static string SDFantdValid(string anteTable, string acode, int ianip)
        {
            string retval = "";
            //replace ante at end of filename with antd
            string antdTable = anteTable.Substring(0, anteTable.Length - 4) + "antd";

            // first adjust anip to exclude points marked for deletion
            try
            {
                DataTable oDT = AntennaPoint.AntennaPointKeysnonD(antdTable, acode);

                if (oDT.Rows.Count > 0)
                {
                    if (oDT.Rows.Count != ianip)
                    {
                        string stacode = oDT.Rows[0]["acode"].ToString();
                        retval = Antenna.AntUpdateAnipNonD(stacode, oDT.Rows.Count, anteTable);
                        if (retval == "OK")
                        {

                            write_msg("W", "- Number of antenna points adjusted to exclude deletions");
                            write_msg("W", " Current anip " + ianip.ToString() + " reset to " + oDT.Rows.Count.ToString());
                        }
                        else
                        {
                            write_msg("E", "ERROR: failed updating anip");
                            return "ERROR";
                        }

                    }

                }
                else
                {
                    write_msg("E", "- There are no antenna points in file");
                    return "ERROR";
                }
            }
            catch (Exception ex)
            {
                // this exception is tripped if there was a system error in AntennaPointKeys 
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            try
            {
                DataTable oDT = AntennaPoint.AntennaPointKeys(antdTable, acode);

                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "ANTENNA POINT KEY: " + oDR["acode"].ToString() + "-" + oDR["antang"].ToString();
                        retval = ValidateAntd(oDR["acode"].ToString(), Convert.ToDouble(oDR["antang"]), antdTable);
                    }
                }
                else
                {
                    write_msg("W", "- There are no antenna points in file");
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                // this exception is tripped if there was a system error in AntennaPointKeys 
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateAntd(string sacode, double dantang, string antdTable)
        {
            string retval = "OK";
            AntennaPoint testAntd = new AntennaPoint(antdTable, sacode, dantang);
            switch (testAntd.cmd)
            {
                case "A":  // add new record
                    retval = ValidateAntdA(testAntd, antdTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateAntdD(testAntd, antdTable);
                    break;

                case "U": // update existing record
                    retval = ValidateAntdU(testAntd, antdTable);
                    break;

                case "N": // no change
                    retval = ValidateAntdN(testAntd, antdTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testAntd.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateAntdA(AntennaPoint testAntd, string antdTable)
        {
            string retval = "OK";

            AntennaPoint sdAntd = new AntennaPoint("", testAntd.acode, testAntd.antang.Value);
            if (sdAntd.acode == testAntd.acode)
            {
                write_msg("E", "- Record with this key already exists in SDB");
                return "ERROR";
            }

            retval = ValidateAntdFields(testAntd, antdTable);

            return retval;
        }
        private static string ValidateAntdD(AntennaPoint testAntd, string antdTable)
        {
            string retval = "OK";

            AntennaPoint sdAntd = new AntennaPoint("", testAntd.acode, testAntd.antang.Value);
            if (sdAntd.acode == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testAntd.mdate.Trim() != sdAntd.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testAntd.mdate.Trim() + ") differs from date of original record (" + sdAntd.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testAntd.mtime.Trim() != sdAntd.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testAntd.mtime.Trim() + ") differs from time of original record (" + sdAntd.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateAntdN(AntennaPoint testAntd, string antdTable)
        {
            string retval = "OK";
            string retval2 = "";

            AntennaPoint sdAntd = new AntennaPoint("", testAntd.acode, testAntd.antang.Value);
            if (sdAntd.acode == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdAntd.cmd = "N";
                sdAntd.recstat = "U";
                if ((retval2 = AntennaPoint.AntdUpdate(sdAntd, antdTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateAntdU(AntennaPoint testAntd, string antdTable)
        {
            string retval = "OK";

            AntennaPoint sdAntd = new AntennaPoint("", testAntd.acode, testAntd.antang.Value);
            if (sdAntd.acode == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testAntd.mdate.Trim() != sdAntd.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testAntd.mdate.Trim() + ") differs from date of original record (" + sdAntd.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testAntd.mtime.Trim() != sdAntd.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testAntd.mtime.Trim() + ") differs from time of original record (" + sdAntd.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateAntdFields(testAntd, antdTable);

            return retval;
        }
        private static string ValidateAntdFields(AntennaPoint testAntd, string antdTable)
        {
            string retval = "OK";

			if (testAntd.antang < ANTEANTANG_LO || (testAntd.antang > ANTEANTANG_HI)) 
            {
			    write_msg("E", "- Angle (" + testAntd.antang + ") must be between " + ANTEANTANG_LO + " to " + ANTEANTANG_HI);
                retval = "ERROR";
            }

            if (!testAntd.dcov.HasValue)
            {
                testAntd.dcov = 0.0;
            }
            if (testAntd.dcov < ANTEDISCR_LO || testAntd.dcov > ANTEDISCR_HI) 
            {
				//printf ("%s\t\t%s\t:%.2f: - Co-polar discrimination(vertical) must be between %.1f and %.1f\n",keyLineDet, "E", antdTransRec.dcov,ANTEDISCR_LO, ANTEDISCR_HI);
                write_msg("E", "- Co-polar discrimination(vertical) (" + testAntd.dcov + ") must be between " + ANTEDISCR_LO + " to " + ANTEDISCR_HI);
                retval = "ERROR";
            }

            if (testAntd.dcoh.HasValue)
            {
                if (testAntd.dcoh < ANTEDISCR_LO || testAntd.dcoh > ANTEDISCR_HI)
                {
                    //printf ("%s\t\t%s\t:%.2f: - Co-polar discrimination(horizontal) must be between %.1f and %.1f\n",keyLineDet, "E", antdTransRec.dcoh,	ANTEDISCR_LO, ANTEDISCR_HI);
                    write_msg("E", "- Co-polar discrimination(horizontal) (" + testAntd.dcoh + ") must be between " + ANTEDISCR_LO + " to " + ANTEDISCR_HI);
                    retval = "ERROR";
                }
            }

            if (!testAntd.dxpv.HasValue)
            {
                if (testAntd.dxpv < ANTEDISCR_LO || testAntd.dxpv > ANTEDISCR_HI)
                {
                    //printf ("%s\t\t%s\t:%.2f: - Cross-polar discrimination(vertical) must be between %.1f and %.1f\n",keyLineDet, "E", antdTransRec.dxpv,	ANTEDISCR_LO, ANTEDISCR_HI);
                    write_msg("E", "- Cross-polar discrimination(vertical) (" + testAntd.dxpv + ") must be between " + ANTEDISCR_LO + " to " + ANTEDISCR_HI);
                    retval = "ERROR";
                }
            }

            if (!testAntd.dxph.HasValue)
            {
                if (testAntd.dxph < ANTEDISCR_LO || testAntd.dxph > ANTEDISCR_HI)
                {
                    //printf ("%s\t\t%s\t:%.2f: - Cross-polar discrimination(horizontal) must be between %.1f and %.1f\n",keyLineDet, "E", antdTransRec.dxph,ANTEDISCR_LO, ANTEDISCR_HI);
                    write_msg("E", "- Cross-polar discrimination(horizontal) (" + testAntd.dxph + ") must be between " + ANTEDISCR_LO + " to " + ANTEDISCR_HI);
                    retval = "ERROR";
                }
            }

            if (!testAntd.dtilt.HasValue)
            {
                if (testAntd.dtilt < ANTEDTILT_LO || testAntd.dtilt > ANTEDTILT_HI)
                {
                    //printf ("%s\t\t%s\t:%.2f: - Antenna tilt discrimination must be between %.1f and %.1f\n",	keyLineDet, "E", antdTransRec.dtilt,ANTEDTILT_LO, ANTEDTILT_HI);
                    write_msg("E", "- Antenna tilt discrimination (" + testAntd.dtilt + ") must be between " + ANTEDTILT_LO + " to " + ANTEDTILT_HI);
                    retval = "ERROR";
                }
            }
            return retval;
        }

        private static string SDFctxValid(string schema, string filename)
        {
            string retval = "";
            string ctxTable = schema + ".su_" + filename + "_ctx_";

            try
            {
                DataTable oDT = Ctx.CtxKeys(ctxTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "CTX KEY: " + oDR["tfcr"].ToString() + "-" + oDR["tfci"].ToString() + "-" + oDR["rxeqp"].ToString();

                        retval = ValidateCtx(oDR["tfcr"].ToString(), oDR["tfci"].ToString(), oDR["rxeqp"].ToString(), ctxTable);
                    }
                    write_status("Ctx", filename);
                }
                else
                {
                    write_msg("E", "- There are no ctx records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                // this exception is tripped if there was a system error in CtxKeys 
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateCtx(string stfcr, string stfci, string srxeqp, string ctxTable)
        {
            string retval = "OK";

            Ctx testCtx = new Ctx(ctxTable, stfcr, stfci, srxeqp);

            switch (testCtx.cmd)
            {
                case "A":  // add new record
                    retval = ValidateCtxA(testCtx, ctxTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateCtxD(testCtx, ctxTable);
                    break;

                case "U": // update existing record
                    retval = ValidateCtxU(testCtx, ctxTable);
                    break;

                case "N": // no change
                    retval = ValidateCtxN(testCtx, ctxTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testCtx.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateCtxA(Ctx testCtx, string ctxTable)
        {
            string retval = "OK";

            Ctx sdCtx = new Ctx("", testCtx.tfcr, testCtx.tfci, testCtx.rxeqp);

            if (sdCtx.tfcr == testCtx.tfcr)
            {
                write_msg("E", "- Record with this ctx code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateCtxFields(testCtx, ctxTable);

            if (retval == "ERROR")
            {
                return retval;
            }

            retval = SDFctxdValid(sesSchema, ctxTable, testCtx.tfcr, testCtx.tfci, testCtx.rxeqp, testCtx.v_ctxndp.Value);
            return retval;
        }
        private static string ValidateCtxD(Ctx testCtx, string ctxTable)
        {
            string retval = "OK";

            Ctx sdCtx = new Ctx();

            sdCtx = new Ctx("", testCtx.tfcr, testCtx.tfci, testCtx.rxeqp);

            if (sdCtx.tfcr == "")
            {
                write_msg("E", "- Record with this ctx code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testCtx.mdate.Trim() != sdCtx.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testCtx.mdate.Trim() + ") differs from date of original record (" + sdCtx.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testCtx.mtime.Trim() != sdCtx.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testCtx.mtime.Trim() + ") differs from time of original record (" + sdCtx.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = SDFctxdValid(sesSchema, ctxTable, testCtx.tfcr, testCtx.tfci, testCtx.rxeqp, testCtx.v_ctxndp.Value);

            return retval;
        }
        private static string ValidateCtxN(Ctx testCtx, string ctxTable)
        {
            string retval = "OK";
            string retval2 = "";

            Ctx sdCtx = new Ctx("", testCtx.tfcr, testCtx.tfci, testCtx.rxeqp);

            if (sdCtx.tfcr == "")
            {
                write_msg("E", "- Record with this ctx code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdCtx.cmd = "N";
                sdCtx.recstat = "U";
                if ((retval2 = Ctx.CtxUpdate(sdCtx, ctxTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            if (retval == "ERROR")
            {
                return retval;
            }

            retval = SDFctxdValid(sesSchema, ctxTable, testCtx.tfcr, testCtx.tfci, testCtx.rxeqp, testCtx.v_ctxndp.Value);
            return retval;
        }
        private static string ValidateCtxU(Ctx testCtx, string ctxTable)
        {
            string retval = "OK";

            Ctx sdCtx = new Ctx("", testCtx.tfcr, testCtx.tfci, testCtx.rxeqp);

            if (sdCtx.tfcr == "")
            {
                write_msg("E", "- Record with this ctx code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testCtx.mdate.Trim() != sdCtx.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testCtx.mdate.Trim() + ") differs from date of original record (" + sdCtx.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testCtx.mtime.Trim() != sdCtx.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testCtx.mtime.Trim() + ") differs from time of original record (" + sdCtx.mtime.Trim() + ")");
                retval = "ERROR";
            }
 
            retval = ValidateCtxFields(testCtx, ctxTable);

            if (retval == "ERROR")
            {
                return retval;
            }

            retval = SDFctxdValid(sesSchema, ctxTable, testCtx.tfcr, testCtx.tfci, testCtx.rxeqp, testCtx.v_ctxndp.Value);

            return retval;

        }
        private static string ValidateCtxFields(Ctx testCtx, string ctxTable)
        {
            string retval = "OK";
            
            // check if temp CTX
            if (testCtx.tfcr.IndexOf("$") == 0 || testCtx.tfci.IndexOf("$") == 0 || testCtx.rxeqp.IndexOf("$") == 0)
            {
                write_msg("E", "- Temporary CTX codes not allowed");
                retval = "ERROR";
            }

            if (!testCtx.v_rqco.HasValue)
            {
                testCtx.v_rqco = 0.0f;
            }
            if (testCtx.v_rqco.Value < CTXRQCO_LO || testCtx.v_rqco.Value > CTXRQCO_HI)
            {
                write_msg("E", "- Co-channel objective (Rqco:" + testCtx.v_rqco.Value + ") must be between " + CTXRQCO_LO + " to " + CTXRQCO_HI);
                retval = "ERROR";
            }
 
            if (!testCtx.v_rqcull.HasValue)
            {
                testCtx.v_rqcull = 0.0f;
            }

            if (testCtx.v_rqcull.Value < CTXRQCULL_LO || testCtx.v_rqcull.Value > CTXRQCULL_HI)
            {
                write_msg("E", "- Best objective (Rqcull:" + testCtx.v_rqcull.Value + ") must be between " + CTXRQCULL_LO + " to " + CTXRQCULL_HI);
                retval = "ERROR";
            }

            if (!testCtx.v_rqwrst.HasValue)
            {
                testCtx.v_rqwrst = 0.0f;
            }

            if (testCtx.v_rqwrst.Value < CTXRQWRST_LO || testCtx.v_rqwrst.Value > CTXRQWRST_HI)
            {
                write_msg("E", "- Worst objective (Rqwrst:" + testCtx.v_rqwrst.Value + ") must be between " + CTXRQWRST_LO + " to " + CTXRQWRST_HI);
                retval = "ERROR";
            }
            return retval;
        }

        private static string SDFctxdValid(string schema, string ctxTable, string stfcr, string stfci, string srxeqp, int ictxndp)
        {
            string retval = "";
            //replace ctx_ at end of filename with ctxd
            string ctxdTable = ctxTable.Substring(0, ctxTable.Length - 4) + "ctxd";

            // first adjust ctxndp to exclude points marked for deletion
            try
            {
                DataTable oDT = Ctxd.CtxdKeysNonD(ctxdTable, stfcr, stfci, srxeqp);

                if (oDT.Rows.Count > 0)
                {
                    if (oDT.Rows.Count != ictxndp)
                    {
                        retval = Ctx.CtxUpdateCtxndpNonD(ctxTable, stfcr, stfci, srxeqp, oDT.Rows.Count);

                        if (retval == "OK")
                        {
                            write_msg("W", "- Number of ctx points adjusted to exclude deletions");
                            write_msg("W", " Current ctxndp " + ictxndp.ToString() + " reset to " + oDT.Rows.Count.ToString());
                        }
                        else
                        {
                            write_msg("E", "ERROR: failed updating ctxndp");
                            //return "ERROR";
                            return retval;
                        }
                    }
                }
                else
                {
                    write_msg("E", "- There are no ctx points in file");
                    return "ERROR";
                }
            }
            catch (Exception ex)
            {
                // this exception is tripped if there was a system error in CtxdKeysNonD 
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }       
    
            // now validate contents of ctxd records
            try
            {
                DataTable oDT = Ctxd.CtxdKeys(ctxdTable, stfcr, stfci, srxeqp);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        retval = "OK";
                        Single sepang = Convert.ToSingle(oDR["fsep"]);
                        key_text = "CTXD KEY: " + oDR["tfcr"].ToString() + "-" + oDR["tfci"].ToString() + "-" + oDR["rxeqp"].ToString() + "-" + sepang;
                        retval = ValidateCtxd(oDR["tfcr"].ToString().Trim(), oDR["tfci"].ToString().Trim(), oDR["rxeqp"].ToString().Trim(), sepang, ctxdTable);
                        //write_msg("W", "After ValidateCtxd"); // debug
                    }
                }
                else
                {
                    write_msg("E", "- There are no ctxd records in file: " + ctxdTable);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                // this exception is tripped if there was a system error in CtxdKeys 
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateCtxd(string stfcr, string stfci, string srxeqp, float dfsep, string ctxdTable)
        {
            string retval = "OK";
            //write_msg("E", "ValidateCtxd Values- " + stfcr + " " + stfci + " " + srxeqp + " " + dfsep.ToString());

            Ctxd testCtxd = new Ctxd(ctxdTable, stfcr, stfci, srxeqp, dfsep);

            //write_msg("E", testCtxd.cmd.Trim());

            switch (testCtxd.cmd.Trim())
            {
                case "A":  // add new record
                    retval = ValidateCtxdA(testCtxd, ctxdTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateCtxdD(testCtxd, ctxdTable);
                    break;

                case "U": // update existing record
                    retval = ValidateCtxdU(testCtxd, ctxdTable);
                    break;

                case "N": // no change
                    retval = ValidateCtxdN(testCtxd, ctxdTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testCtxd.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateCtxdA(Ctxd testCtxd, string ctxdTable)
        {
            string retval = "OK";

            Ctxd sdCtxd = new Ctxd("", testCtxd.tfcr, testCtxd.tfci, testCtxd.rxeqp, testCtxd.v_fsep.Value);
            //write_msg("W", "ValidateCtxdA:" + testCtxd.tfcr + " " + testCtxd.tfci + " " + testCtxd.rxeqp + " " + testCtxd.v_fsep.Value.ToString());

            if (sdCtxd.tfcr == testCtxd.tfcr)
            {
                write_msg("E", "- Record with this key already exists in SDB");
                return "ERROR";
            }

            retval = ValidateCtxdFields(testCtxd, ctxdTable);

            return retval;
        }
        private static string ValidateCtxdD(Ctxd testCtxd, string ctxdTable)
        {
            string retval = "OK";

            Ctxd sdCtxd = new Ctxd("", testCtxd.tfcr, testCtxd.tfci, testCtxd.rxeqp, testCtxd.v_fsep.Value);
            //write_msg("W", "ValidateCtxdD:" + testCtxd.tfcr + " " + testCtxd.tfci + " " + testCtxd.rxeqp + " " + testCtxd.v_fsep.Value.ToString());
            
            if (sdCtxd.tfcr == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testCtxd.mdate.Trim() != sdCtxd.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testCtxd.mdate.Trim() + ") differs from date of original record (" + sdCtxd.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testCtxd.mtime.Trim() != sdCtxd.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testCtxd.mtime.Trim() + ") differs from time of original record (" + sdCtxd.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateCtxdN(Ctxd testCtxd, string ctxdTable)
        {
            string retval = "OK";
            string retval2 = "";

            Ctxd sdCtxd = new Ctxd("", testCtxd.tfcr, testCtxd.tfci, testCtxd.rxeqp, testCtxd.v_fsep.Value);
            //write_msg("W", "ValidateCtxdN:" + testCtxd.tfcr + " " + testCtxd.tfci + " " + testCtxd.rxeqp + " " + testCtxd.v_fsep.Value.ToString());

            if (sdCtxd.tfcr == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdCtxd.cmd = "N";
                sdCtxd.recstat = "U";
                if ((retval2 = Ctxd.CtxdUpdate(testCtxd, ctxdTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateCtxdU(Ctxd testCtxd, string ctxdTable)
        {
            string retval = "OK";

            Ctxd sdCtxd = new Ctxd("", testCtxd.tfcr, testCtxd.tfci, testCtxd.rxeqp, testCtxd.v_fsep.Value);
            //write_msg("W", "ValidateCtxdU:" + testCtxd.tfcr + " " + testCtxd.tfci + " " + testCtxd.rxeqp + " " + testCtxd.v_fsep.Value.ToString());

            if (sdCtxd.tfcr == "")
            {
                write_msg("E", "- Record with this band code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated
            if (testCtxd.mdate.Trim() != sdCtxd.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testCtxd.mdate.Trim() + ") differs from date of original record (" + sdCtxd.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testCtxd.mtime.Trim() != sdCtxd.mtime.Trim())
            {
                 write_msg("E", "- Time of update record (" + testCtxd.mtime.Trim() + ") differs from time of original record (" + sdCtxd.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateCtxdFields(testCtxd, ctxdTable);

            return retval;
        }
        private static string ValidateCtxdFields(Ctxd testCtxd, string ctxddTable)
        {
            string retval = "OK";

            if (testCtxd.v_fsep.HasValue)
            {
                if (testCtxd.v_fsep.Value < CTXFSEP_LO || testCtxd.v_fsep.Value > CTXFSEP_HI)
                {
                    write_msg("E", "- Frequency separation (" + testCtxd.v_fsep.Value + ") must be between " + CTXFSEP_LO + " to " + CTXFSEP_HI);
                    retval = "ERROR";
                }
            } 
            else
            {
                write_msg("E", "- Frequency separation must be entered");
                retval = "ERROR";
            }

            if (testCtxd.v_rq.HasValue)
            {
                //testCtxd.rq = 0.0f;
                if (testCtxd.v_rq.Value < CTXRQ_LO || testCtxd.v_rq.Value > CTXRQ_HI)
                {
                    write_msg("E", "- C/I or -I (" + testCtxd.v_rq.Value + ") must be between " + CTXRQ_LO + " to " + CTXRQ_HI);
                    retval = "ERROR";
                }
            }
            return retval;
        }

        private static string SDFeqptValid(string schema, string filename)
        {
            string retval = "";
            string eqptTable = schema + ".su_" + filename + "_eqpt";

            try
            {
                DataTable oDT = Equipment.EqptKeys(eqptTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "EQUIPMENT KEY: " + oDR["ecode"].ToString();
                        retval = ValidateEqpt(oDR["ecode"].ToString(), eqptTable);
                    }
                    write_status("Equipment", filename);
                }
                else
                {
                    write_msg("E", "- There are no equipment records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateEqpt(string secode, string eqptTable)
        {
            string retval = "OK";

            Equipment testEqpt = new Equipment(eqptTable, secode);
            switch (testEqpt.cmd)
            {
                case "A":  // add new record
                    retval = ValidateEqptA(testEqpt, eqptTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateEqptD(testEqpt, eqptTable);
                    break;

                case "U": // update existing record
                    retval = ValidateEqptU(testEqpt, eqptTable);
                    break;

                case "N": // no change
                    retval = ValidateEqptN(testEqpt, eqptTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testEqpt.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateEqptA(Equipment testEqpt, string eqptTable)
        {
            string retval = "OK";

            Equipment sdEqpt = new Equipment(testEqpt.ecode);
            if (sdEqpt.ecode == testEqpt.ecode)
            {
                write_msg("E", "- Record with this equipment code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateEqptFields(testEqpt, eqptTable);

            return retval;
        }
        private static string ValidateEqptD(Equipment testEqpt, string eqptTable)
        {
            string retval = "OK";

            Equipment sdEqpt = new Equipment(testEqpt.ecode);
            if (sdEqpt.ecode == "")
            {
                write_msg("E", "- Record with this equipment code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testEqpt.mdate.Trim() != sdEqpt.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testEqpt.mdate.Trim() + ") differs from date of original record (" + sdEqpt.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testEqpt.mtime.Trim() != sdEqpt.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testEqpt.mtime.Trim() + ") differs from time of original record (" + sdEqpt.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateEqptN(Equipment testEqpt, string eqptTable)
        {
            string retval = "OK";
            string retval2 = "";

            Equipment sdEqpt = new Equipment(testEqpt.ecode);
            if (sdEqpt.ecode == "")
            {
                write_msg("E", "- Record with this equipment code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdEqpt.cmd = "N";
                sdEqpt.recstat = "U";
                if ((retval2 = Equipment.EqptUpdate(testEqpt, eqptTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateEqptU(Equipment testEqpt, string eqptTable)
        {
            string retval = "OK";

            Equipment sdEqpt = new Equipment(testEqpt.ecode);
            if (sdEqpt.ecode == "")
            {
                write_msg("E", "- Record with this equipment code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testEqpt.mdate.Trim() != sdEqpt.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testEqpt.mdate.Trim() + ") differs from date of original record (" + sdEqpt.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testEqpt.mtime.Trim() != sdEqpt.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testEqpt.mtime.Trim() + ") differs from time of original record (" + sdEqpt.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateEqptFields(testEqpt, eqptTable);

            return retval;
        }
        private static string ValidateEqptFields(Equipment testEqpt, string eqptTable)
        {
            string retval = "OK";

            int missing = 0;

            // check for empty fields
            if (!testEqpt.v_estab.HasValue)
            {
                write_msg("E", "- Missing value for Stability (estab)");
                missing++;
            }

            if (testEqpt.emanu == "")
            {
                write_msg("E", "- Missing value for Manufacturer (emanu)");
                missing++;
            }

            if (testEqpt.edesc == "")
            {
                write_msg("E", "- Missing value for Description (edesc)");
                missing++;
            }

            if (testEqpt.etype == "")
            {
                write_msg("E", "- Missing value for Equipment Type (etype)");
                missing++;
            }

            if (missing > 0)
            {
                write_msg("E", "- All required fields not entered");
                return "ERROR";
            }

            // check if temp equip
            if (testEqpt.ecode.IndexOf("$") == 0)
            {
                write_msg("E", "- Temporary equipment codes not allowed");
                retval = "ERROR";
            }

            // check if temp equip xref
            if (testEqpt.exref.IndexOf("$") == 0)
            {
                write_msg("E", "- Temporary equipment cross reference codes not allowed");
                retval = "ERROR";
            }

            if (testEqpt.v_estab.Value < ESTAB_LO || testEqpt.v_estab.Value > ESTAB_HI)
            {
                write_msg("E", "- Frequency stability (" + testEqpt.v_estab.Value + ") must be between " + ESTAB_LO + " and " + ESTAB_HI);
                retval = "ERROR";
            }
           
            if (testEqpt.etype != EQPTANAL && testEqpt.etype != EQPTDIGI)
            {
                write_msg("E", "- Equipment type (" + testEqpt.etype.Trim() + ") must be (A)nalogue or (D)igital");
                retval = "ERROR";
            }

            if (testEqpt.v_thhold.HasValue)
            {
                if (testEqpt.v_thhold.Value < THHOLD_LO || testEqpt.v_thhold.Value > THHOLD_HI)
                {
                    write_msg("E", "- Receive threshhold (" + testEqpt.v_thhold.Value + ") must be between " + THHOLD_LO + " and " + THHOLD_HI);
                    retval = "ERROR";
                }
            }

            if (testEqpt.v_e1stif.HasValue)
            {
                if (testEqpt.v_e1stif.Value < E1STIF_LO || testEqpt.v_e1stif.Value > E1STIF_HI)
                {
                    write_msg("E", "- First inter. frequency (" + testEqpt.v_e1stif.Value + ") must be between " + E1STIF_LO + " and " + E1STIF_HI);
                    retval = "ERROR";
                }
            }

            if (testEqpt.v_e2ndif.HasValue)
            {
                if (testEqpt.v_e2ndif.Value < E2NDIF_LO || testEqpt.v_e2ndif.Value > E2NDIF_HI)
                {
                    write_msg("E", "- Second inter. frequency (" + testEqpt.v_e2ndif.Value + ") must be between " + E2NDIF_LO + " and " + E2NDIF_HI);
                    retval = "ERROR";
                }
            }

            // check if exref in SDB or current file
            if (testEqpt.exref != "")
            {
                if (!Equipment.UnionEqptChk(testEqpt.exref, eqptTable))
                {
                    write_msg("E", "- Reference equipment (" + testEqpt.exref.Trim() + ") not found in the SDB or the SDF");
                    retval = "ERROR";
                }
            }
            return retval;
        }

        private static string SDFnoteValid(string schema, string filename)
        {
            string retval = "";
            string noteTable = schema + ".su_" + filename + "_note";

            try
            {
                DataTable oDT = Note.NoteKeys(noteTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "NOTE KEY: " + oDR["oper"].ToString() + "-" + oDR["nonum"].ToString();
                        retval = ValidateNote(oDR["oper"].ToString(), oDR["nonum"].ToString(), noteTable);
                    }
                    write_status("Note", filename);
                }
                else
                {
                    write_msg("E", "- There are no note records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateNote(string soper, string snonum, string noteTable)
        {
            string retval = "OK";
            Note testNote = new Note(noteTable, soper, snonum);
            
            switch (testNote.cmd)
            {
                case "A":  // add new record
                    retval = ValidateNoteA(testNote, noteTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateNoteD(testNote, noteTable);
                    break;

                case "U": // update existing record
                    retval = ValidateNoteU(testNote, noteTable);
                    break;

                case "N": // no change
                    retval = ValidateNoteN(testNote, noteTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testNote.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateNoteA(Note testNote, string noteTable)
        {
            string retval = "OK";

            Note sdNote = new Note("", testNote.oper, testNote.nonum);
            if (sdNote.oper == testNote.oper)
            {
                write_msg("E", "- Record with this note code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateNoteFields(testNote, noteTable);

            return retval;
        }
        private static string ValidateNoteD(Note testNote, string noteTable)
        {
            string retval = "OK";

            Note sdNote = new Note("", testNote.oper, testNote.nonum);
            if (sdNote.oper == "")
            {
                write_msg("E", "- Record with this note code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted
            if (testNote.mdate.Trim() != sdNote.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testNote.mdate.Trim() + ") differs from date of original record (" + sdNote.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testNote.mtime.Trim() != sdNote.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testNote.mtime.Trim() + ") differs from time of original record (" + sdNote.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateNoteN(Note testNote, string noteTable)
        {
            string retval = "OK";
            string retval2 = "";

            Note sdNote = new Note("", testNote.oper, testNote.nonum);
            if (sdNote.oper == "")
            {
                write_msg("E", "- Record with this note code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                sdNote.cmd = "N";
                sdNote.recstat = "U";
                if ((retval2 = Note.NoteUpdate(testNote, noteTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateNoteU(Note testNote, string noteTable)
        {
            string retval = "OK";

            Note sdNote = new Note("", testNote.oper, testNote.nonum);
            if (sdNote.oper == "")
            {
                write_msg("E", "- Record with this note code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testNote.mdate.Trim() != sdNote.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testNote.mdate.Trim() + ") differs from date of original record (" + sdNote.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testNote.mtime.Trim() != sdNote.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testNote.mtime.Trim() + ") differs from time of original record (" + sdNote.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateNoteFields(testNote, noteTable);

            return retval;
        }
        private static string ValidateNoteFields(Note testNote, string noteTable)
        {
            string retval = "OK";

            Operator checkOper = new Operator("", testNote.oper);

            if(checkOper.oper == "")
            {
                write_msg("E", "- Operator code (" + testNote.oper + ") not found in the SDB operator table");
                retval = "ERROR";
            }

            return retval;
        }

        private static string SDFoperValid(string schema, string filename)
        {
            string retval = "";
            string operTable = schema + ".su_" + filename + "_oper";

            try
            {
                DataTable oDT = Operator.OperKeys(operTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "OPERATOR KEY: " + oDR["oper"].ToString();
                        retval = ValidateOper(oDR["oper"].ToString(), operTable);
                    }
                    write_status("Operator", filename);
                }
                else
                {
                    write_msg("E", "- There are no operator records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateOper(string soper, string operTable)
        {
            string retval = "OK";
            Operator testOper = new Operator(operTable, soper);

            switch (testOper.cmd)
            {
                case "A":  // add new record
                    retval = ValidateOperA(testOper, operTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateOperD(testOper, operTable);
                    break;

                case "U": // update existing record
                    retval = ValidateOperU(testOper, operTable);
                    break;

                case "N": // no change
                    retval = ValidateOperN(testOper, operTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testOper.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateOperA(Operator testOper, string operTable)
        {
            string retval = "OK";

            Operator sdOper = new Operator("", testOper.oper);
            if (sdOper.oper == testOper.oper)
            {
                write_msg("E", "- Record with this operator code already exists in SDB");
                return "ERROR";
            }

            // maintenance operator
            if (testOper.mdbm != "")
            {
                Operator checkOper = new Operator("", testOper.mdbm);
                if (checkOper.oper == "")
                {
                    write_msg("E", "- MDB Maintenance Company (" + testOper.mdbm + ") not found in the SDB operator table");
                    retval = "ERROR";
                }
            }
            else
            {
                write_msg("E", "- Missing value for MDB Maintenance Company (mdbm)");
                retval = "ERROR";
            }
            
            retval = ValidateOperFields(testOper, operTable);

            return retval;
        }
        private static string ValidateOperD(Operator testOper, string operTable)
        {
            string retval = "OK";

            Operator sdOper = new Operator("", testOper.oper);
            if (sdOper.oper == "")
            {
                write_msg("E", "- Record with this operator code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testOper.mdate.Trim() != sdOper.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testOper.mdate.Trim() + ") differs from date of original record (" + sdOper.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testOper.mtime.Trim() != sdOper.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testOper.mtime.Trim() + ") differs from time of original record (" + sdOper.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateOperN(Operator testOper, string operTable)
        {
            string retval = "OK";
            string retval2 = "";

            Operator sdOper = new Operator("", testOper.oper);
            if (sdOper.oper == "")
            {
                write_msg("E", "- Record with this operator code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = Operator.OperUpdate(testOper, operTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateOperU(Operator testOper, string operTable)
        {
            string retval = "OK";

            Operator sdOper = new Operator("", testOper.oper);
            if (sdOper.oper == "")
            {
                write_msg("E", "- Record with this operator code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testOper.mdate.Trim() != sdOper.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testOper.mdate.Trim() + ") differs from date of original record (" + sdOper.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testOper.mtime.Trim() != sdOper.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testOper.mtime.Trim() + ") differs from time of original record (" + sdOper.mtime.Trim() + ")");
                retval = "ERROR";
            }

            // maintenance operator
            if (testOper.mdbm != "")
            {
                Operator checkOper = new Operator("", testOper.mdbm);
                if (checkOper.oper == "")
                {
                    write_msg("E", "- MDB Maintenance Company (" + testOper.mdbm + ") not found in the SDB operator table");
                    retval = "ERROR";
                }
            }
            else
            {
                write_msg("E", "- Missing value for MDB Maintenance Company (mdbm)");
                retval = "ERROR";
            }
            
            retval = ValidateOperFields(testOper, operTable);

            return retval;
        }
        private static string ValidateOperFields(Operator testOper, string operTable)
        {
            string retval = "OK";

            // check prov/state if not blank
            if (testOper.prstat != "")
            {
                if (!Operator.ProvExists(testOper.prstat))
                {
                    write_msg("E", "- Province/State code (" + testOper.prstat + ") not valid");
                    retval = "ERROR";
                }
            }

            // operator classification
            if (testOper.opnote != "")
            {
                if (!Operator.OpnoteExists(testOper.opnote))
                {
                    write_msg("E", "- Operator classification (" + testOper.opnote + ") must be FA, FC, FF, CC, UC, or AA");
                    retval = "ERROR";
                }
            }
            else
            {
                write_msg("E", "- Missing value for Operator Classification (opnote)");
                retval = "ERROR";
            }

            return retval;
        }

        private static string SDFplanValid(string schema, string filename)
        {
            string retval = "";
            string planTable = schema + ".su_" + filename + "_plan";

            try
            {
                DataTable oDT = Plan.PlanKeys(planTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "PLAN KEY: " + oDR["sband"].ToString() + "-" + oDR["splan"].ToString();
                        retval = ValidatePlan(oDR["sband"].ToString(), oDR["splan"].ToString(), planTable);
                    }
                    write_status("Plan", filename);
                }
                else
                {
                    write_msg("E", "- There are no plan records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidatePlan(string ssband, string ssplan, string planTable)
        {
            string retval = "OK";
            Plan testPlan = new Plan(planTable, ssband, ssplan);

            switch (testPlan.cmd)
            {
                case "A":  // add new record
                    retval = ValidatePlanA(testPlan, planTable);
                    break;

                case "D": // delete existing record
                    retval = ValidatePlanD(testPlan, planTable);
                    break;

                case "U": // update existing record
                    retval = ValidatePlanU(testPlan, planTable);
                    break;

                case "N": // no change
                    retval = ValidatePlanN(testPlan, planTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testPlan.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidatePlanA(Plan testPlan, string planTable)
        {
            string retval = "OK";

            Plan sdPlan = new Plan("", testPlan.sband, testPlan.splan);

            if (sdPlan.sband == testPlan.sband)
            {
                write_msg("E", "- Record with this plan code already exists in SDB");
                return "ERROR";
            }

            retval = ValidatePlanFields(testPlan, planTable);
            if (retval == "ERROR")
            {
                return retval;
            }

            retval = SDFplndValid(sesSchema, planTable, testPlan.sband, testPlan.splan);
            return retval;

        }
        private static string ValidatePlanD(Plan testPlan, string planTable)
        {
            string retval = "OK";

            Plan sdPlan = new Plan("", testPlan.sband, testPlan.splan);
            
            if (sdPlan.sband == "")
            {
                write_msg("E", "- Record with this plan code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testPlan.mdate.Trim() != sdPlan.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testPlan.mdate.Trim() + ") differs from date of original record (" + sdPlan.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testPlan.mtime.Trim() != sdPlan.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testPlan.mtime.Trim() + ") differs from time of original record (" + sdPlan.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = SDFplndValid(sesSchema, planTable, testPlan.sband, testPlan.splan);

            return retval;
        }
        private static string ValidatePlanN(Plan testPlan, string planTable)
        {
            string retval = "OK";
            string retval2 = "";

            Plan sdPlan = new Plan("", testPlan.sband, testPlan.splan);

            if (sdPlan.sband == "")
            {
                write_msg("E", "- Record with this plan code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = Plan.PlanUpdate(testPlan, planTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            if (retval == "ERROR")
            {
                return retval;
            }

            retval = SDFplndValid(sesSchema, planTable, testPlan.sband, testPlan.splan);
            return retval;
        }
        private static string ValidatePlanU(Plan testPlan, string planTable)
        {
            string retval = "OK";

            Plan sdPlan = new Plan("", testPlan.sband, testPlan.splan);

            if (sdPlan.sband == "")
            {
                write_msg("E", "- Record with this plan code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testPlan.mdate.Trim() != sdPlan.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testPlan.mdate.Trim() + ") differs from date of original record (" + sdPlan.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testPlan.mtime.Trim() != sdPlan.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testPlan.mtime.Trim() + ") differs from time of original record (" + sdPlan.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidatePlanFields(testPlan, planTable);

            if (retval == "ERROR")
            {
                return retval;
            }

            retval = SDFplndValid(sesSchema, planTable, testPlan.sband, testPlan.splan);

            return retval;
        }
        private static string ValidatePlanFields(Plan testPlan, string planTable)
        {
            string retval = "OK";

            // check if temp splan
            if (testPlan.splan.IndexOf("$") == 0)
            {
                write_msg("E", "- Temporary plan codes not allowed");
                retval = "ERROR";
            }

            if(testPlan.conform.Trim() != "Y" && testPlan.conform.Trim() != "N")
            {
                write_msg("E", "- Conforming Plan must be 'Y' or 'N'");
                retval = "ERROR";
            }

            if (testPlan.uscan != "C" && testPlan.uscan.Trim() != "U" && testPlan.uscan.Trim() != "B")
            {
                write_msg("E", "- Canadian/US Plan must be 'C', 'U' or 'B'");
                retval = "ERROR";
            }
            return retval;
        }

        private static string SDFplndValid(string schema, string planTable, string ssband, string ssplan)
        {
            string retval = "";
            //replace plan at end of filename with plnd
            string plndTable = planTable.Substring(0, planTable.Length - 4) + "plnd";

            try
            {
                DataTable oDT = Plnd.PlndKeys(plndTable, ssband, ssplan);
                if (oDT.Rows.Count > 0)
                {
                    if (oDT.Rows.Count > MAXPLANPTS)
                    {
                        write_msg("E", "- Plan number (" + oDT.Rows.Count + ") is greater than the maximum of " + MAXPLANPTS + " allowed");
                        retval = "ERROR";
                    }

                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "PLND KEY: " + oDR["sband"].ToString() + "-" + oDR["splan"].ToString() + "-" + Convert.ToInt16(oDR["spno"]);
                        retval = ValidatePlnd(oDR["sband"].ToString(), oDR["splan"].ToString(), Convert.ToInt16(oDR["spno"]), plndTable);
                    }
                }
                else
                {
                    write_msg("E", "- There are no plnd records in file: " + planTable);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidatePlnd(string ssband, string ssplan, short sspno, string plndTable)
        {
            string retval = "OK";
            Plnd testPlnd = new Plnd(plndTable, ssband, ssplan, sspno);

            switch (testPlnd.cmd)
            {
                case "A":  // add new record
                    retval = ValidatePlndA(testPlnd, plndTable);
                    break;

                case "D": // delete existing record
                    retval = ValidatePlndD(testPlnd, plndTable);
                    break;

                case "U": // update existing record
                    retval = ValidatePlndU(testPlnd, plndTable);
                    break;

                case "N": // no change
                    retval = ValidatePlndN(testPlnd, plndTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testPlnd.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidatePlndA(Plnd testPlnd, string plndTable)
        {
            string retval = "OK";

            Plnd sdPlnd = new Plnd("", testPlnd.sband, testPlnd.splan, testPlnd.v_spno.Value);

            if (sdPlnd.sband == testPlnd.sband)
            {
                write_msg("E", "- Record with this key already exists in SDB");
                return "ERROR";
            }

            retval = ValidatePlndFields(testPlnd, plndTable);

            return retval;
        }
        private static string ValidatePlndD(Plnd testPlnd, string plndTable)
        {
            string retval = "OK";

            Plnd sdPlnd = new Plnd("", testPlnd.sband, testPlnd.splan, testPlnd.v_spno.Value);
            if (sdPlnd.sband == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testPlnd.mdate.Trim() != sdPlnd.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testPlnd.mdate.Trim() + ") differs from date of original record (" + sdPlnd.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testPlnd.mtime.Trim() != sdPlnd.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testPlnd.mtime.Trim() + ") differs from time of original record (" + sdPlnd.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidatePlndN(Plnd testPlnd, string plndTable)
        {
            string retval = "OK";
            string retval2 = "";
            Plnd sdPlnd = new Plnd("", testPlnd.sband, testPlnd.splan, testPlnd.v_spno.Value);
            if (sdPlnd.sband == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = Plnd.PlndUpdate(testPlnd, plndTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidatePlndU(Plnd testPlnd, string plndTable)
        {
            string retval = "OK";

            Plnd sdPlnd = new Plnd("", testPlnd.sband, testPlnd.splan, testPlnd.v_spno.Value);
            if (sdPlnd.sband == "")
            {
                write_msg("E", "- Record with this key does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testPlnd.mdate.Trim() != sdPlnd.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testPlnd.mdate.Trim() + ") differs from date of original record (" + sdPlnd.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testPlnd.mtime.Trim() != sdPlnd.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testPlnd.mtime.Trim() + ") differs from time of original record (" + sdPlnd.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidatePlndFields(testPlnd, plndTable);

            return retval;
        }
        private static string ValidatePlndFields(Plnd testPlnd, string plndTable)
        {
            string retval = "OK";

            return retval;
        }

        private static string SDFroutValid(string schema, string filename)
        {
            string retval = "";
            string routTable = schema + ".su_" + filename + "_rout";

            //swrep.WriteLine("in SDFroutValid");
            //swrep.Flush();
            try
            {
                DataTable oDT = Route.RoutKeys(routTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "ROUTE KEY: " + oDR["rcomp"].ToString() + "-" + oDR["routnumb"].ToString();
                        retval = ValidateRout(oDR["rcomp"].ToString(), oDR["routnumb"].ToString(), routTable);
                    }
                    write_status("Route", filename);
                }
                else
                {
                    write_msg("E", "- There are no route records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateRout(string srcomp, string sroutnumb, string routTable)
        {
            //swrep.WriteLine("in ValidateRout:" + srcomp + ":" + sroutnumb);
            //swrep.Flush();
            string retval = "OK";
            Route testRout = new Route(routTable, srcomp, sroutnumb);

            switch (testRout.cmd)
            {
                case "A":  // add new record
                    retval = ValidateRoutA(testRout, routTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateRoutD(testRout, routTable);
                    break;

                case "U": // update existing record
                    retval = ValidateRoutU(testRout, routTable);
                    break;

                case "N": // no change
                    retval = ValidateRoutN(testRout, routTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testRout.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }      
        private static string ValidateRoutA(Route testRout, string routTable)
        {
            //swrep.WriteLine("in ValidateRoutA");
            //swrep.Flush();
            string retval = "OK";

            Route sdRout = new Route("", testRout.rcomp, testRout.routnumb);
            if (sdRout.rcomp == testRout.rcomp)
            {
                write_msg("E", "- Record with this route code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateRoutFields(testRout, routTable);

            return retval;
        }
        private static string ValidateRoutD(Route testRout, string routTable)
        {
            //swrep.WriteLine("in ValidateRoutD");
            //swrep.Flush(); 
            string retval = "OK";

            Route sdRout = new Route("", testRout.rcomp, testRout.routnumb);
            if (sdRout.rcomp == "")
            {
                write_msg("E", "- Record with this route code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testRout.mdate.Trim() != sdRout.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testRout.mdate.Trim() + ") differs from date of original record (" + sdRout.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testRout.mtime.Trim() != sdRout.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testRout.mtime.Trim() + ") differs from time of original record (" + sdRout.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateRoutN(Route testRout, string routTable)
        {
            //swrep.WriteLine("in ValidateRoutN");
            //swrep.Flush(); 
            string retval = "OK";
            string retval2 = "";

            Route sdRout = new Route("", testRout.rcomp, testRout.routnumb);
            if (sdRout.rcomp == "")
            {
                write_msg("E", "- Record with this route code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = Route.RoutUpdate(testRout, routTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateRoutU(Route testRout, string routTable)
        {
            //swrep.WriteLine("in ValidateRoutU");
            //swrep.Flush(); 
            string retval = "OK";

            Route sdRout = new Route("", testRout.rcomp, testRout.routnumb);
            if (sdRout.rcomp == "")
            {
                write_msg("E", "- Record with this route code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testRout.mdate.Trim() != sdRout.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testRout.mdate.Trim() + ") differs from date of original record (" + sdRout.mdate.Trim() + ")");
                retval = "ERROR";
            }
            if (testRout.mtime.Trim() != sdRout.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testRout.mtime.Trim() + ") differs from time of original record (" + sdRout.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateRoutFields(testRout, routTable);

            return retval;
        }
        private static string ValidateRoutFields(Route testRout, string routTable)
        {
            //swrep.WriteLine("in ValidateRoutFields");
            //swrep.Flush(); 
            string retval = "OK";

            //if (testRout.routnumb < ROUTNUMB_LO || testRout.routnumb > ROUTNUMB_HI)
            ///{
            //    write_msg("E", "- The route number (" + testRout.routnumb + ") must be between " + ROUTNUMB_LO + " to " + ROUTNUMB_HI);
            //    retval = "ERROR";
            //}

            if (!Operator.ProvExists(testRout.rtprov))
            {
                write_msg("E", "- Province/State code (" + testRout.rtprov + ") not valid");
                retval = "ERROR";
            }

            return retval;
        }

        private static string SDFtownValid(string schema, string filename)
        {
            string retval = "";
            string townTable = schema + ".su_" + filename + "_town";

            try
            {
                DataTable oDT = TowerNote.TownKeys(townTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "TOWER NOTE KEY: " + oDR["call1"].ToString() + "-" + Convert.ToInt16(oDR["atwrno"]);
                        retval = ValidateTown(oDR["call1"].ToString(), Convert.ToInt16(oDR["atwrno"]), townTable);
                    }
                    write_status("Tower note", filename);
                }
                else
                {
                    write_msg("E", "- There are no tower note records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateTown(string scall1, Int16 iatwrno, string townTable)
        {
            string retval = "OK";
            TowerNote testTown = new TowerNote(townTable, scall1, iatwrno);

            switch (testTown.cmd)
            {
                case "A":  // add new record
                    retval = ValidateTownA(testTown, townTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateTownD(testTown, townTable);
                    break;

                case "U": // update existing record
                    retval = ValidateTownU(testTown, townTable);
                    break;

                case "N": // no change
                    retval = ValidateTownN(testTown, townTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testTown.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateTownA(TowerNote testTown, string townTable)
        {
            string retval = "OK";

            TowerNote sdTown = new TowerNote("", testTown.call1, testTown.atwrno);
            if (sdTown.call1 == testTown.call1)
            {
                write_msg("E", "- Record with this tower note already exists in SDB");
                return "ERROR";
            }

            retval = ValidateTownFields(testTown, townTable);

            return retval;
        }
        private static string ValidateTownD(TowerNote testTown, string townTable)
        {
            string retval = "OK";

            TowerNote sdTown = new TowerNote("", testTown.call1, testTown.atwrno);
            if (sdTown.call1 == "")
            {
                write_msg("E", "- Record with this tower note code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testTown.mdate.Trim() != sdTown.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testTown.mdate.Trim() + ") differs from date of original record (" + sdTown.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testTown.mtime.Trim() != sdTown.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testTown.mtime.Trim() + ") differs from time of original record (" + sdTown.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateTownN(TowerNote testTown, string townTable)
        {
            string retval = "OK";
            string retval2 = "";

            TowerNote sdTown = new TowerNote("", testTown.call1, testTown.atwrno);
            if (sdTown.call1 == "")
            {
                write_msg("E", "- Record with this tower note code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = TowerNote.TownUpdate(testTown, townTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateTownU(TowerNote testTown, string townTable)
        {
            string retval = "OK";

            TowerNote sdTown = new TowerNote("", testTown.call1, testTown.atwrno);
            if (sdTown.call1 == "")
            {
                write_msg("E", "- Record with this tower note code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testTown.mdate.Trim() != sdTown.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testTown.mdate.Trim() + ") differs from date of original record (" + sdTown.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testTown.mtime.Trim() != sdTown.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testTown.mtime.Trim() + ") differs from time of original record (" + sdTown.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateTownFields(testTown, townTable);

            return retval;
        }
        private static string ValidateTownFields(TowerNote testTown, string townTable)
        {
            string retval = "OK";

            int missing = 0;

            // check for empty fields
            if (testTown.oper == "")
            {
                write_msg("E", "- Missing value for Operator (oper)");
                missing++;
            }

            if (missing > 0)
            {
                write_msg("E", "- All required fields not entered");
                return "ERROR";
            }

            // check that operator exists
            if (!TowerNote.OperExists(testTown.oper))
            {
                write_msg("E", "- Operator " + testTown.oper + " does not exist in subsidiary operator table");
                return ("ERROR");
            }

            //if (testTown.v_twht.HasValue)
            //{
            //    if (testTown.v_twht.Value < TWHT_LO || testTown.v_twht.Value > TWHT_HI)
            //    {
            //        write_msg("E", "- The tower height (" + testTown.v_twht.Value + ") must be between " + TWHT_LO + " and " + TWHT_HI);
            //        retval = "ERROR";
            //    }
            //}

            if (testTown.atwrno < ATWRNO_LO || testTown.atwrno > ATWRNO_HI)
            {
                write_msg("E", "- The tower number (" + testTown.atwrno + ") must be between " + ATWRNO_LO + " and " + ATWRNO_HI);
                retval = "ERROR";
            }

            return retval;
        }

        private static string SDFtowrValid(string schema, string filename)
        {
            string retval = "";
            string towrTable = schema + ".su_" + filename + "_towr";

            try
            {
                DataTable oDT = Tower.TowrKeys(towrTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "TOWER KEY: " + oDR["twcode"].ToString();
                        retval = ValidateTowr(oDR["twcode"].ToString(), towrTable);
                    }
                    write_status("Tower", filename);
                }
                else
                {
                    write_msg("E", "- There are no tower records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateTowr(string stwcode, string towrTable)
        {
            string retval = "OK";
            Tower testTowr = new Tower(towrTable, stwcode);

            switch (testTowr.cmd)
            {
                case "A":  // add new record
                    retval = ValidateTowrA(testTowr, towrTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateTowrD(testTowr, towrTable);
                    break;

                case "U": // update existing record
                    retval = ValidateTowrU(testTowr, towrTable);
                    break;

                case "N": // no change
                    retval = ValidateTowrN(testTowr, towrTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testTowr.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;

        }
        private static string ValidateTowrA(Tower testTowr, string towrTable)
        {
            string retval = "OK";

            Tower sdTowr = new Tower("", testTowr.twcode);
            if (sdTowr.twcode == testTowr.twcode)
            {
                write_msg("E", "- Record with this tower code already exists in SDB");
                return "ERROR";
            }

            retval = ValidateTowrFields(testTowr, towrTable);

            return retval;
        }
        private static string ValidateTowrD(Tower testTowr, string towrTable)
        {
            string retval = "OK";

            Tower sdTowr = new Tower("", testTowr.twcode);
            if (sdTowr.twcode == "")
            {
                write_msg("E", "- Record with this tower code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be deleted

            if (testTowr.mdate.Trim() != sdTowr.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testTowr.mdate.Trim() + ") differs from date of original record (" + sdTowr.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testTowr.mtime.Trim() != sdTowr.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testTowr.mtime.Trim() + ") differs from time of original record (" + sdTowr.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateTowrN(Tower testTowr, string towrTable)
        {
            string retval = "OK";
            string retval2 = "";

            Tower sdTowr = new Tower("", testTowr.twcode);
            if (sdTowr.twcode == "")
            {
                write_msg("E", "- Record with this tower code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = Tower.TowrUpdate(testTowr, towrTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }

            return retval;
        }
        private static string ValidateTowrU(Tower testTowr, string towrTable)
        {
            string retval = "OK";

            Tower sdTowr = new Tower("", testTowr.twcode);
            if (sdTowr.twcode == "")
            {
                write_msg("E", "- Record with this tower code does not exist in SDB");
                return "ERROR";
            }

            // check if date and time match record to be updated

            if (testTowr.mdate.Trim() != sdTowr.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testTowr.mdate.Trim() + ") differs from date of original record (" + sdTowr.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testTowr.mtime.Trim() != sdTowr.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testTowr.mtime.Trim() + ") differs from time of original record (" + sdTowr.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateTowrFields(testTowr, towrTable);

            return retval;
        }
        private static string ValidateTowrFields(Tower testTowr, string towrTable)
        {
            string retval = "OK";

            return retval;
        }

        private static string SDFtrafValid(string schema, string filename)
        {
            string retval = "";
            string trafTable = schema + ".su_" + filename + "_traf";

            try
            {
                DataTable oDT = Traffic.TrafKeys(trafTable);
                if (oDT.Rows.Count > 0)
                {
                    // records found; loop through and validate each one
                    foreach (DataRow oDR in oDT.Rows)
                    {
                        key_text = "TRAFFIC KEY: " + oDR["trafcode"].ToString() + "-" + oDR["ecode"].ToString();
                        retval = ValidateTraf(oDR["trafcode"].ToString(), oDR["ecode"].ToString(), trafTable);
                    }
                    write_status("Traffic", filename);
                }
                else
                {
                    write_msg("E", "- There are no traffic records in file: " + filename);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                write_msg("E", "ERROR: failed retrieving data: " + ex.Message);
                return "ERROR";
            }

            return "OK";
        }
        private static string ValidateTraf(string strafcode, string secode, string trafTable)
        {
            string retval = "OK";
            Traffic testTraf = new Traffic(trafTable, strafcode, secode);

            switch (testTraf.cmd)
            {
                case "A":  // add new record
                    retval = ValidateTrafA(testTraf, trafTable);
                    break;

                case "D": // delete existing record
                    retval = ValidateTrafD(testTraf, trafTable);
                    break;

                case "U": // update existing record
                    retval = ValidateTrafU(testTraf, trafTable);
                    break;

                case "N": // no change
                    retval = ValidateTrafN(testTraf, trafTable);
                    break;

                default:
                    write_msg("E", "- CMD (" + testTraf.cmd + ") is invalid - must be A, D, N, or U");
                    return "ERROR";
            }
            return retval;
        }
        private static string ValidateTrafA(Traffic testTraf, string trafTable)
        {
            string retval = "OK";

            Traffic sdTraf = new Traffic("", testTraf.trafcode, testTraf.ecode);

            if (sdTraf.trafcode == testTraf.trafcode)
            {
                write_msg("E", "- Record with this traffic code / equipment code already exists in SDB");
                return "ERROR";
            }

            // for digital traffic, check that trafcode is unique
            if (testTraf.trafcode.IndexOf("D") == 0)
            {
                if (!Traffic.UnionTrafCodeChk(testTraf.trafcode, trafTable, 1))
                {
                    write_msg("E", "- This traffic code (" + testTraf.trafcode + ") is duplicated in this SDF");
                    return "ERROR";
                }
            }

            retval = ValidateTrafFields(testTraf, trafTable);

            return retval;
        }
        private static string ValidateTrafD(Traffic testTraf, string trafTable)
        {
            string retval = "OK";

            Traffic sdTraf = new Traffic("", testTraf.trafcode, testTraf.ecode);
            if (sdTraf.trafcode == "")
            {
                write_msg("E", "- Record with this traffic code / equipment code does not exist in SDB");
                return "ERROR";
            }

            // for digital traffic, check that trafcode is unique
            if (testTraf.trafcode.IndexOf("D") == 0)
            {
                if (!Traffic.UnionTrafCodeChk(testTraf.trafcode, trafTable, 2))
                {
                    write_msg("E", "- This traffic code (" + testTraf.trafcode + ") is duplicated in this SDF");
                    return "ERROR";
                }
            }

            // check if date and time match record to be deleted

            if (testTraf.mdate.Trim() != sdTraf.mdate.Trim())
            {
                write_msg("E", "- Date of delete record (" + testTraf.mdate.Trim() + ") differs from date of original record (" + sdTraf.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testTraf.mtime.Trim() != sdTraf.mtime.Trim())
            {
                write_msg("E", "- Time of delete record (" + testTraf.mtime.Trim() + ") differs from time of original record (" + sdTraf.mtime.Trim() + ")");
                retval = "ERROR";
            }

            return retval;
        }
        private static string ValidateTrafN(Traffic testTraf, string trafTable)
        {
            string retval = "OK";
            string retval2 = "";

            Traffic sdTraf = new Traffic("", testTraf.trafcode, testTraf.ecode);
            if (sdTraf.trafcode == "")
            {
                write_msg("E", "- Record with this traffic code / equipment code does not exist in SDB");
                retval = "ERROR";
            }
            else
            {
                if ((retval2 = Traffic.TrafUpdate(testTraf, trafTable)) != "OK")
                {
                    write_msg("E", "- Error updating PDF with MDB values");
                    write_msg("E", retval2);
                    retval = "ERROR";
                }
            }
            
            return retval;
        }
        private static string ValidateTrafU(Traffic testTraf, string trafTable)
        {
            string retval = "OK";

            Traffic sdTraf = new Traffic("", testTraf.trafcode, testTraf.ecode);
            if (sdTraf.trafcode == "")
            {
                write_msg("E", "- Record with this traffic code does not exist in SDB");
                return "ERROR";
            }

            // for digital traffic, check that trafcode is unique
            if (testTraf.trafcode.IndexOf("D") == 0)
            {
                if (!Traffic.UnionTrafCodeChk(testTraf.trafcode, trafTable, 2))
                {
                    write_msg("E", "- This traffic code (" + testTraf.trafcode + ") is duplicated in this SDF");
                    return "ERROR";
                }
            }

            // check if date and time match record to be updated

            if (testTraf.mdate.Trim() != sdTraf.mdate.Trim())
            {
                write_msg("E", "- Date of update record (" + testTraf.mdate.Trim() + ") differs from date of original record (" + sdTraf.mdate.Trim() + ")");
                retval = "ERROR";
            }

            if (testTraf.mtime.Trim() != sdTraf.mtime.Trim())
            {
                write_msg("E", "- Time of update record (" + testTraf.mtime.Trim() + ") differs from time of original record (" + sdTraf.mtime.Trim() + ")");
                retval = "ERROR";
            }

            retval = ValidateTrafFields(testTraf, trafTable);

            return retval;
        }
        private static string ValidateTrafFields(Traffic testTraf, string trafTable)
        {
            string retval = "OK";

            // check against equipment for digital traffic
            if (testTraf.trafcode.IndexOf("D") == 0)  // digital trafcode starts with D
            {
                Equipment checkEqpt1 = new Equipment(testTraf.ecode);
                if (checkEqpt1.ecode == "")
                {
                    write_msg("E", "- The Equipment Code  (" + testTraf.ecode + ") is not in the SDB equipment table");
                    retval = "ERROR";
                }
                else
                {
                    if (testTraf.trafcode != checkEqpt1.etraf)
                    {
                        write_msg("E", "- The Traffic Code (" + testTraf.trafcode + ") does not match value (" + checkEqpt1.etraf + ") on the SDB equipment record");
                        retval = "ERROR";
                    }
                }
            }
            else  // analog
            {
                if (testTraf.ecode != "UNKNOWN")
                {
                    Equipment checkEqpt1a = new Equipment(testTraf.ecode);
                    if (checkEqpt1a.ecode == "")
                    {
                        write_msg("E", "- The Equipment Code  (" + testTraf.ecode + ") is not in the SDB equipment table");
                        retval = "ERROR";
                    }
                }
            }


            if (testTraf.xreftrcde.IndexOf("D") == 0)  // xref digital trafcode starts with D
            {
                if (testTraf.xrefeqcde != "")
                {
                    Equipment checkEqpt2 = new Equipment(testTraf.xrefeqcde);
                    if (checkEqpt2.ecode == "")
                    {
                        write_msg("E", "- The Equipment Code  (" + testTraf.xrefeqcde + ") is not in the SDB equipment table");
                        retval = "ERROR";
                    }
                    else
                    {
                        if (testTraf.xreftrcde != checkEqpt2.etraf)
                        {
                            write_msg("E", "- The Cross Reference Traffic Code (" + testTraf.xreftrcde + ") does not match value (" + checkEqpt2.etraf + ") on the SDB equipment record");
                            retval = "ERROR";
                        }
                    }
                }
                else
                {
                    write_msg("E", "- The Cross Reference Equipment Code is mandatory if the Cross Reference Traffic Code starts with 'D'");
                    retval = "ERROR";
                }
            }
            else  // analog
            {
                if (testTraf.xrefeqcde != "" && testTraf.xrefeqcde != "UNKNOWN")
                {
                    Equipment checkEqpt2a = new Equipment(testTraf.xrefeqcde);
                    if (checkEqpt2a.ecode == "")
                    {
                        write_msg("E", "- The Equipment Code  (" + testTraf.xrefeqcde + ") is not in the SDB equipment table");
                        retval = "ERROR";
                    }
                }
            }


            return retval;
        }

        private static void write_msg(string stype, string smsg_text)
        {
            int inLev = 0;

            if (key_text == "") 
            {
                // write line if key already written
                swrep.WriteLine("\t{0}\t{1}", stype, smsg_text);
                swrep.Flush();
            }
            else
            {
                // write key and then error/warning
                swrep.WriteLine();
                swrep.WriteLine(key_text);
                key_text = "";
                swrep.WriteLine("\t{0}\t{1}", stype, smsg_text);
                swrep.Flush();
            }

            // update error/warning status and counters
            switch (stype)
            {
                case "E":
                    inLev = 2;
                    ErrorCount++;
                    break;
                case "W":
                    inLev = 1;
                    WarningCount++;
                    break;
                default:
                    swrep.WriteLine("Invalid message type {0} for error{1}", stype, smsg_text);
                    return;
            }

        }
        static private void write_status(string sfiletype, string sfilename)
        {
            swrep.WriteLine();
            swrep.WriteLine("\nThere were a total of {0} errors and {1} warnings", ErrorCount, WarningCount);
            swrep.WriteLine();
            if (ErrorCount > 0)
            {
                swrep.WriteLine("\nThe {0} SDF {1} is invalid", sfiletype, sfilename);
            }
            else
            {
                swrep.WriteLine("\nThe {0} SDF {1} is valid", sfiletype, sfilename);
            }

            swrep.WriteLine();
            swrep.WriteLine("SDF Validation completed - " + DateTime.Now);

        }
        static void write_header(string sfiletype, string sfilename)
        {
            swrep.WriteLine("\t\tSDF VALIDATION REPORT " + DateTime.Now);
            swrep.WriteLine();
            swrep.WriteLine("\n{0} SDF: {1}", sfiletype, sfilename);
            swrep.WriteLine();
        }
        static double? update_double(double? su_val, double? sd_val)
        {

            if (su_val.HasValue)
            {
                return su_val;
            }
            else
            {
                if (sd_val.HasValue)
                {
                    return sd_val;
                }
                return null;
            }
        }
        static string update_string(string su_val, string sd_val)
        {
            if (su_val == "" || su_val == null)
            {
                return sd_val;
            }
            else
            {
                return su_val;
            }
        }
        private static int CheckDebugSetting()
        {
            // this routine checks if debug info is to be written to file webdrive + "\\extractlogs\\" + userid + "<ProgName>.txt" 
            //
            // if the table web.debuglogs does not exist in the database, it assumes no output is to be written
            // if the table web.debuglogs exists in the database for this module and user, and the value of debugflag is 0, no output is to be written
            // if the table web.debuglogs exists in the database for this module and user, and the value of debugflag is > 0, output is to be written

            // check debug value for this module and user
            string strSql = " SELECT debugflag FROM web.debuglogs WHERE debugmodule ='" + ProgName + "' AND micsid = '" + userid + "'";

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {
                cn.Open();
                OdbcCommand select = new OdbcCommand(strSql, cn);
                OdbcDataReader dr1;

                try
                {
                    dr1 = select.ExecuteReader();
                }
                catch
                {
                    // select will fail if table does not exist
                    return 0;
                }

                if (dr1.HasRows)
                {
                    dr1.Read();
                    // return value of debugflag
                    return dr1.GetInt32(0);
                }
                else
                {
                    // record not found for this user/debug module
                    return 0;
                }
            }
        }
        private static void WriteDebug(string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
            }
        }
        private static void WriteDebugFlush(string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
                sw.Flush();
            }
        }
        private static void WriteDebugClose(string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
                sw.Close();
            }
        }
    }
}
