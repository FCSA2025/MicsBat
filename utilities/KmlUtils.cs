using DBAccess;
using DBUtilities;
using LongLatUtilities;
using System;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Text;
using System.Web;

namespace KmlUtilities
{
    public class KmlUtils
    {
        private static string cnstr;       // connection string
        private static string schema;
        private static string ftsitetable;
        private static string ftantetable;
        private static string ftchantable;
        private static string mtsitetable;
        private static string reportlist;
        private static string kmlrepname;
        private static string format6 = "F6";
        private static string format2 = "F2";
        private static string statusinfo;
        private static string pdfname;
        private static string reptype;

        private static StreamWriter sw;
        private static StreamWriter swd;

        private static int diagnostics;
        private static bool KmlOK;
        static HttpContext ctx;

        public static bool build_kml(string inpdfname, string inreptype, out string lstatusinfo, out string replist)
        {

            pdfname = inpdfname;
            reptype = inreptype;


            //get the current HTTP context
            ctx = HttpContext.Current;
            cnstr = ctx.Session["s_cnString"].ToString();

            //Table Names
            schema = ctx.Session["s_schema"].ToString();
            ftsitetable = schema + ".ft_" + pdfname + "_site";
            ftantetable = schema + ".ft_" + pdfname + "_ante";
            ftchantable = schema + ".ft_" + pdfname + "_chan";
            mtsitetable = "main.mt_site";
            statusinfo = "";
            lstatusinfo = "";
            reportlist = "";
            replist = "";
            string strSql = "";

            // open database connection
            dbconnect oCn = new dbconnect();

            // load datatable with site info
            // check if multiple band codes

            strSql = "SELECT DISTINCT bndcde FROM " + ftchantable;

            DataTable oDTbndcde = null;
            try
            {
                oDTbndcde = oCn.retrieve(strSql);
                oCn.dbdisconnect();
            }
            catch (Exception)
            {
                oCn.dbdisconnect();
                lstatusinfo += "System error selecting bandcodes\n";
                //close_form();
                return false;
            }

            // no records found
            if (oDTbndcde.Rows.Count == 0)
            {
                lstatusinfo += "ERROR: There are no channel records in this PDF\n";
                return false;
            }

            switch (reptype)
            {

                case "V":
                    diagnostics = 1;

                    kmlrepname = "ts_" + pdfname + ".kml";
                    // write KML headers
                    WriteKmlHeader("Vertical", kmlrepname);
                    // write kml detail
                    if (!do_kmlV())
                    {
                        swd.WriteLine("KML FAILED-statusinfo:" + statusinfo + " reportlist:" + reportlist);
                        KmlOK = false;
                        sw.WriteLine("</Document>");
                        sw.WriteLine("</kml>");
                        lstatusinfo += statusinfo;
                        return false;
                    }
                    // write kml trailers
                    swd.WriteLine("KML OK-statusinfo:" + statusinfo + " reportlist:" + reportlist);
                    KmlOK = true;
                    sw.WriteLine("</Document>");
                    sw.WriteLine("</kml>");
                    sw.Close();
                    break;

                case "H": // horizontal by band - built but not selected by user
                    diagnostics = 0;

                    kmlrepname = "ts_" + pdfname + "horizontal.kml";
                    // write KML headers
                    WriteKmlHeader("Horizontal", kmlrepname);
                    // write kml detail
                    if (!do_kmlH())
                    {
                        //close_form();
                        KmlOK = false;
                        sw.WriteLine("</Document>");
                        sw.WriteLine("</kml>");
                        lstatusinfo += statusinfo;
                        return false;
                    }
                    // write kml trailers
                    KmlOK = true;
                    sw.WriteLine("</Document>");
                    sw.WriteLine("</kml>");
                    sw.Close();
                    break;
            }

            if (reportlist != "") // file is not empty
            {
                replist = reportlist;
            }

            swd.Close();

            return KmlOK;
        }


        private static bool do_kmlV()
        {
            // original design called for possibly multiple output files - this was dropped, but option was kept open here
            // append output file name to list
            reportlist += kmlrepname + ";";

            // write site info
            //if (!WriteSiteInfo(pdfname, ftchantable)) return false;
            if (!WriteSiteInfo()) return false;
            // Write lines for all related links
            if (!WriteLinkInfoV(pdfname)) return false; ;

            return true;

        }
        private static bool do_kmlH()
        {
            // original design called for possibly multiple output files - this was dropped, but option was kept open here
            // append output file name to list
            reportlist += kmlrepname + ";";

            // write site info
            //WriteSiteInfo(pdfname, chantable);
            if (!WriteSiteInfo()) return false;
            // Write lines for all related links
            if (!WriteLinkInfoH(pdfname)) return false; ;

            return true;
        }
        private static void WriteKmlHeader(string pdfname, string inname)
        {
            sw = new StreamWriter(ctx.Session["user_dir"].ToString() + kmlrepname, false);

            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sw.WriteLine("<kml xmlns=\"http://earth.google.com/kml/2.2\">");
            sw.WriteLine("<Document>");
            //sw.WriteLine("<name align='center'>TS " + pdfname + "</name>");
            sw.WriteLine("<name align='center'>TS " + inname + "</name>");

            // set styles for red and green lines/pins
            sw.WriteLine("<Style id='red'>");
            sw.WriteLine("  <IconStyle>");
            sw.WriteLine("    <color>7f0000ff</color>");
            sw.WriteLine("  </IconStyle>");
            sw.WriteLine("  <LineStyle>");
            sw.WriteLine("    <color>7f0000ff</color>");
            sw.WriteLine("    <width>3</width>");
            sw.WriteLine("  </LineStyle>");
            sw.WriteLine("</Style>");
            sw.WriteLine("");
            sw.WriteLine("<Style id='green'>");
            sw.WriteLine("  <IconStyle>");
            sw.WriteLine("    <color>7f00ff00</color>");
            sw.WriteLine("  </IconStyle>");
            sw.WriteLine("  <LineStyle>");
            sw.WriteLine("    <color>7f00ff00</color>");
            sw.WriteLine("    <width>3</width>");
            sw.WriteLine("  </LineStyle>");
            sw.WriteLine("</Style>");
        }
        private static bool WriteSitePoint(string call1, string name, double sdeclong, double sdeclat, double salt, string oper)
        {
            string sitedescstr = null;
            if (!SiteDesc(call1, name, sdeclong, sdeclat, salt, oper, out sitedescstr)) return false;

            // this routine draws and labels a point representing a site
            sw.WriteLine("<Placemark>");
            sw.WriteLine("<name align='center'>" + call1.TrimEnd() + "</name>");
            //sw.WriteLine("<description>" + SiteDesc(call1, name, sdeclong, sdeclat, salt, oper) + "</description>");
            sw.WriteLine("<description>" + sitedescstr + "</description>");
            sw.WriteLine("<Point>");
            sw.WriteLine("<altitudeMode>absolute</altitudeMode>");
            sw.WriteLine("<coordinates>" + sdeclong.ToString(format6) + "," + sdeclat.ToString(format6) + "," + salt.ToString(format2) + "</coordinates>");
            sw.WriteLine("</Point>");
            sw.WriteLine("</Placemark>");
            sw.Flush();
            return true;
        }
        private static bool SiteDesc(string call1, string name, double sdeclong, double sdeclat, double salt, string oper, out string sitedescstr)
        {
            sitedescstr = null;

            try
            {
                // write html for site description
                StringBuilder htmldes = new StringBuilder("<![CDATA[", 1000);
                htmldes.Append("<table border='1' align='center'>");
                htmldes.Append("<tr>");
                htmldes.Append("<td>Call1</td><td>" + call1 + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("<tr>");
                htmldes.Append("<td>Name</td><td>" + name + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("<tr>");
                htmldes.Append("<td>Latitude</td><td>" + sdeclat.ToString(format6) + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("<tr>");
                htmldes.Append("<td>Longitude</td><td>" + sdeclong.ToString(format6) + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("<td>Ground (m)</td><td>" + Convert.ToDouble(salt.ToString(format2)) + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("</tr>");
                htmldes.Append("<td>Operator</td><td>" + Convert.ToString(oper.ToString()) + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("</table>");
                htmldes.Append("]]>");

                sitedescstr = htmldes.ToString();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error building site description\n";
                swd.WriteLine("System error building site description:" + ex.Message);
                return false;
            }
        }
        private static string LinkDescH(DataRow oDRchan)
        {
            // local site info
            string lcall = Convert.ToString(oDRchan["call1"]);
            string lname = Convert.ToString(oDRchan["lname"]);
            double ldlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llatit"])));    // s.intlatit
            double ldlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llongit"])));   // s.intlongit
            double ldsalt = Convert.ToDouble(oDRchan["lgrnd"]);
            string loper = Convert.ToString(oDRchan["loper"]);

            // remote site info
            string rcall = Convert.ToString(oDRchan["call2"]);
            string rname = Convert.ToString(oDRchan["rname"]);
            double rdlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlatit"])));    // s.intlatit
            double rdlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlongit"])));   // s.intlongit
            double rdsalt = Convert.ToDouble(oDRchan["rgrnd"]);
            string roper = Convert.ToString(oDRchan["roper"]);

            // bndcde info
            string bndcde = Convert.ToString(oDRchan["bndcde"]);

            // write html for site description
            StringBuilder htmldes = new StringBuilder("<![CDATA[", 1000);
            htmldes.Append("<table border='1' align='center'>");
            htmldes.Append("<tr>");
            htmldes.Append("<td colspan = '7' align='center'>Sites</td>");
            htmldes.Append("</tr>");

            htmldes.Append("<tr>");
            htmldes.Append("<td></td><td>Call1</td>");
            htmldes.Append("<td>Name</td>");
            htmldes.Append("<td>Latitude</td>");
            htmldes.Append("<td>Longitude</td>");
            htmldes.Append("<td>Ground (m)</td>");
            htmldes.Append("<td>Operator</td>");
            htmldes.Append("</tr>");

            htmldes.Append("<tr>");
            htmldes.Append("<td>Local</td>");
            htmldes.Append("<td>" + lcall + "</td>");
            htmldes.Append("<td>" + lname + "</td>");
            htmldes.Append("<td>" + ldlat.ToString(format6) + "</td>");
            htmldes.Append("<td>" + ldlong.ToString(format6) + "</td>");
            htmldes.Append("<td>" + ldsalt.ToString(format2) + "</td>");
            htmldes.Append("<td>" + loper + "</td>");
            htmldes.Append("</tr>");

            htmldes.Append("<tr>");
            htmldes.Append("<td>Remote</td>");
            htmldes.Append("<td>" + rcall + "</td>");
            htmldes.Append("<td>" + rname + "</td>");
            htmldes.Append("<td>" + rdlat.ToString(format6) + "</td>");
            htmldes.Append("<td>" + rdlong.ToString(format6) + "</td>");
            htmldes.Append("<td>" + rdsalt.ToString(format2) + "</td>");
            htmldes.Append("<td>" + roper + "</td>");
            htmldes.Append("</tr>");
            htmldes.Append("</table>");

            string htmldescante = null;
            GetAnteInfoH(lcall, rcall, bndcde, out htmldescante);
            htmldes.Append(htmldescante);

            string htmldescchan = null;
            GetChanInfoH(lcall, rcall, bndcde, out htmldescchan);
            htmldes.Append(htmldescchan);

            htmldes.Append("]]>");
            return htmldes.ToString();
        }
        private static bool LinkDescV(DataRow oDRchan, out string htmldescstr)
        {
            htmldescstr = null;
            string lcall;
            string lname;
            double ldlat;    // s.intlatit
            double ldlong;   // s.intlongit
            double ldsalt;
            string loper;
            string rcall;
            string rname;
            double rdlat;    // s.intlatit
            double rdlong;   // s.intlongit
            double rdsalt;
            string roper;
            string bndcde;
            StringBuilder htmldes = new StringBuilder("<![CDATA[", 1000);

            try
            {
                // local site info
                lcall = Convert.ToString(oDRchan["call1"]);
                lname = Convert.ToString(oDRchan["lname"]);
                ldlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llatit"])));    // s.intlatit
                ldlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llongit"])));   // s.intlongit
                ldsalt = Convert.ToDouble(oDRchan["lgrnd"]);
                loper = Convert.ToString(oDRchan["loper"]);

                // remote site info
                rcall = Convert.ToString(oDRchan["call2"]);
                rname = Convert.ToString(oDRchan["rname"]);
                rdlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlatit"])));    // s.intlatit
                rdlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlongit"])));   // s.intlongit
                rdsalt = Convert.ToDouble(oDRchan["rgrnd"]);
                roper = Convert.ToString(oDRchan["roper"]);

                // bndcde info
                bndcde = Convert.ToString(oDRchan["bndcde"]);

                // write html for site description

                htmldes.Append("<table border='1' align='center'>");
                htmldes.Append("<tr>");
                htmldes.Append("<td colspan = '7' align='center'>Sites</td>");
                htmldes.Append("</tr>");

                htmldes.Append("<tr>");
                htmldes.Append("<td></td><td>Call1</td>");
                htmldes.Append("<td>Name</td>");
                htmldes.Append("<td>Latitude</td>");
                htmldes.Append("<td>Longitude</td>");
                htmldes.Append("<td>Ground (m)</td>");
                htmldes.Append("<td>Operator</td>");
                htmldes.Append("</tr>");

                htmldes.Append("<tr>");
                htmldes.Append("<td>Local</td>");
                htmldes.Append("<td>" + lcall + "</td>");
                htmldes.Append("<td>" + lname + "</td>");
                htmldes.Append("<td>" + ldlat.ToString(format6) + "</td>");
                htmldes.Append("<td>" + ldlong.ToString(format6) + "</td>");
                htmldes.Append("<td>" + ldsalt.ToString(format2) + "</td>");
                htmldes.Append("<td>" + loper + "</td>");
                htmldes.Append("</tr>");

                htmldes.Append("<tr>");
                htmldes.Append("<td>Remote</td>");
                htmldes.Append("<td>" + rcall + "</td>");
                htmldes.Append("<td>" + rname + "</td>");
                htmldes.Append("<td>" + rdlat.ToString(format6) + "</td>");
                htmldes.Append("<td>" + rdlong.ToString(format6) + "</td>");
                htmldes.Append("<td>" + rdsalt.ToString(format2) + "</td>");
                htmldes.Append("<td>" + roper + "</td>");
                htmldes.Append("</tr>");
                htmldes.Append("</table>");
            }
            catch (Exception ex)
            {
                statusinfo += "System error building link description\n";
                swd.WriteLine("ERROR building link description:" + ex.Message);
                return false;
            }

            if (diagnostics == 1) swd.WriteLine("before GetAnteInfoV: " + lcall + " " + rcall + " " + bndcde);
            if (diagnostics == 1) swd.Flush();

            string htmldescante;
            if (!GetAnteInfoV(lcall, rcall, bndcde, out htmldescante)) return false;
            htmldes.Append(htmldescante);

            if (diagnostics == 1) swd.WriteLine("before GetChanInfoV: " + lcall + " " + rcall + " " + bndcde);
            if (diagnostics == 1) swd.Flush();

            string htmldescchan;
            if (!GetChanInfoV(lcall, rcall, bndcde, out htmldescchan)) return false;
            htmldes.Append(htmldescchan);

            htmldes.Append("]]>");

            htmldescstr = htmldes.ToString();
            return true;
        }
        private static bool GetAnteInfoH(string lcall, string rcall, string bndcde, out string htmldescante)
        {

            htmldescante = null;
            if (diagnostics == 1) swd.WriteLine("In GetAnteInfoH: " + lcall + " " + rcall + " " + bndcde);
            if (diagnostics == 1) swd.Flush();

            string strSql = "SELECT RTRIM(call1), RTRIM(call2), bndcde, anum, acode, aht, azmth, elvtn, dist, licence " +
                        " FROM " + ftantetable + " WHERE call1 = '" + lcall + "' AND call2 = '" + rcall + "' AND bndcde = '" + bndcde + "'" +
                        " ORDER BY call1, call2, bndcde, anum";

            dbconnect oCn = new dbconnect();

            DataTable oDTantes = null;
            try
            {
                oDTantes = oCn.retrieve(strSql);
                oCn.dbdisconnect();
            }
            catch (Exception ex)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting antenna info\n";
                swd.WriteLine("ERROR selecting antenna info:" + ex.Message);
                return false;
            }

            try
            {
                StringBuilder htmldes = new StringBuilder("", 1000);
                htmldes.Append("<br />");
                htmldes.Append("<table border='1' align='center'>");
                htmldes.Append("<tr>");
                htmldes.Append("<td colspan = '10' align='center'>Antennas</td>");
                htmldes.Append("</tr>");

                htmldes.Append("<tr>");
                htmldes.Append("<td>Call1</td>");
                htmldes.Append("<td>Call2</td>");
                htmldes.Append("<td>Band</td>");
                htmldes.Append("<td>Anum</td>");
                htmldes.Append("<td>Acode</td>");
                htmldes.Append("<td>Aht</td>");
                htmldes.Append("<td>Azimuth</td>");
                htmldes.Append("<td>Elevation</td>");
                htmldes.Append("<td>Distance</td>");
                htmldes.Append("<td>Licence</td>");
                htmldes.Append("</tr>");

                // get ante info
                using (OdbcConnection ucn1 = new OdbcConnection(cnstr))
                {
                    ucn1.Open();

                    // retrieve data into datareader
                    foreach (DataRow oDRante in oDTantes.Rows)
                    {
                        if (diagnostics == 1) swd.WriteLine("In GetAnteInfoH:foreach");
                        if (diagnostics == 1) swd.Flush();

                        htmldes.Append("<tr>");
                        htmldes.Append("<td>" + lcall + "</td>");
                        htmldes.Append("<td>" + rcall + "</td>");
                        htmldes.Append("<td>" + bndcde + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRante["anum"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRante["acode"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRante["aht"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRante["azmth"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRante["elvtn"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToDouble(oDRante["dist"]).ToString(format2) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRante["licence"]) + "</td>");
                        htmldes.Append("</tr>");
                    }
                }
                htmldes.Append("</table>");
                htmldescante = htmldescante.ToString();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error selecting antenna info\n";
                swd.WriteLine("ERROR selecting antenna info:" + ex.Message);
                return false;
            }

        }
        private static bool GetAnteInfoV(string lcall, string rcall, string bndcde, out string htmldescante)
        {
            htmldescante = null;

            if (diagnostics == 1) swd.WriteLine("In GetAnteInfoV: " + lcall + " " + rcall + " " + bndcde);
            if (diagnostics == 1) swd.Flush();


            string strSql = @"SELECT 1 as rectype, call1, call2, bndcde, anum, acode, aht, azmth, elvtn, dist, licence  " +
                     " FROM " + ftantetable +
                " WHERE call1 = '" + lcall + "' AND call2 = '" + rcall + "' AND bndcde = '" + bndcde + "' " +
                " UNION " +
                " SELECT 2,call1, call2, bndcde, anum, acode, aht, azmth, elvtn, dist, licence  " +
                " FROM " + ftantetable +
                " WHERE call1 = '" + rcall + "' AND call2 = '" + lcall + "' AND bndcde = '" + bndcde + "' " +
                " ORDER BY bndcde, rectype";
            if (diagnostics == 1) swd.WriteLine(strSql);

            dbconnect oCn = new dbconnect();

            DataTable oDTantev = null;
            try
            {
                oDTantev = oCn.retrieve(strSql);
                if (oDTantev.Rows.Count == 0)
                {
                    statusinfo += "ERROR: No antennas found for: " +
                                            lcall.Trim() + "-" + rcall.Trim() + "-" + bndcde.Trim() + "\n";
                    oCn.dbdisconnect();
                    return false;
                }
                if (oDTantev.Rows.Count % 2 != 0)  // odd number of rows retrieved
                {
                    statusinfo += "WARNING: At least 1 one-way antenna selected for: " +
                        lcall.Trim() + "-" + rcall.Trim() + "-" + bndcde.Trim() + "\n";
                }
                oCn.dbdisconnect();
            }
            catch (Exception e3)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting antenna info\n";
                swd.WriteLine("ERRORSQL:" + strSql + ":" + e3.Message);
                return false;
            }

            int rowwidth = oDTantev.Rows.Count + 1;

            StringBuilder htmldesv = new StringBuilder("", 1000);

            try
            {
                htmldesv.Append("<br />");
                htmldesv.Append("<table border='1' align='center'>");
                htmldesv.Append("<tr>");
                htmldesv.Append("<td colspan = '" + rowwidth.ToString() + "' align='center'>Antennas</td>");
                htmldesv.Append("</tr>");

                if (diagnostics == 1) swd.WriteLine("after headers");
                if (diagnostics == 1) swd.Flush();

                htmldesv.Append("<tr><td>Call1</td>");
                AddVRowstr(1, oDTantev, htmldesv);
                htmldesv.Append("<tr><td>Call2</td>");
                AddVRowstr(2, oDTantev, htmldesv);
                htmldesv.Append("<tr><td>Band</td>");
                AddVRowstr(3, oDTantev, htmldesv);
                htmldesv.Append("<tr><td>Anum</td>");
                AddVRowstr(4, oDTantev, htmldesv);
                htmldesv.Append("<tr><td>Acode</td>");
                AddVRowstr(5, oDTantev, htmldesv);
                htmldesv.Append("<tr><td>Height</td>");
                AddVRowdbl(6, oDTantev, htmldesv, 1);
                htmldesv.Append("<tr><td>Azmth</td>");
                AddVRowdbl(7, oDTantev, htmldesv, 2);
                htmldesv.Append("<tr><td>Elevation</td>");
                AddVRowdbl(8, oDTantev, htmldesv, 2);
                htmldesv.Append("<tr><td>Distance</td>");
                AddVRowdbl(9, oDTantev, htmldesv, 2);
                htmldesv.Append("<tr><td>Licence</td>");
                AddVRowstr(10, oDTantev, htmldesv);

                htmldesv.Append("</table>");
                htmldescante = htmldesv.ToString();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error selecting antenna info\n";
                swd.WriteLine("ERROR selecting antenna info:" + ex.Message);
                return false;
            }
        }
        private static bool GetChanInfoV(string lcall, string rcall, string bndcde, out string htmldesvstr)
        {
            htmldesvstr = null;
            if (diagnostics == 1) swd.WriteLine("In GetChanInfoV: " + lcall + " " + rcall + " " + bndcde);
            if (diagnostics == 1) swd.Flush();

            string strSql = @"SELECT 1 as rectype, call1, call2, bndcde, chid, freqtx, poltx, pwrtx, atpccde, traftx, eqpttx, antnumbtx1, afsltx1, antnumbtx2, afsltx2, freqrx, polrx,  " +
                " antnumbrx1, afslrx1, pwrrx1, antnumbrx2, afslrx2, pwrrx2 " +
                " FROM " + ftchantable +
                " WHERE call1 = '" + lcall + "' AND call2 = '" + rcall + "' AND bndcde = '" + bndcde + "' " +
                " UNION " +
                " SELECT 2,call1, call2, bndcde, chid, freqtx, poltx, pwrtx, atpccde, traftx, eqpttx, antnumbtx1, afsltx1, antnumbtx2, afsltx2, freqrx, polrx,  " +
                " antnumbrx1, afslrx1, pwrrx1, antnumbrx2, afslrx2, pwrrx2 " +
                " FROM " + ftchantable +
                " WHERE call1 = '" + rcall + "' AND call2 = '" + lcall + "' AND bndcde = '" + bndcde + "' " +
                " ORDER BY bndcde, chid, rectype";
            if (diagnostics == 1) swd.WriteLine(strSql);

            dbconnect oCn = new dbconnect();

            DataTable oDTchanv = null;
            try
            {
                oDTchanv = oCn.retrieve(strSql);
                if (oDTchanv.Rows.Count == 0)
                {
                    statusinfo += "ERROR: No channels found for: " +
                                            lcall.Trim() + "-" + rcall.Trim() + "-" + bndcde.Trim() + "\n";
                    oCn.dbdisconnect();
                    return false;
                }
                if (oDTchanv.Rows.Count % 2 != 0)  // odd number of rows retrieved
                {
                    statusinfo += "WARNING: At least 1 one-way channel selected for: " + lcall.Trim() + "-" + rcall.Trim() + "-" + bndcde.Trim() + "\n";
                }
                oCn.dbdisconnect();
            }
            catch (Exception e3)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting channel info\n";
                swd.WriteLine("ERRORSQL:" + strSql + ":" + e3.Message);
                //ErrorUtils.NotifySystemOps(e3, "GetChanInfoV");
                return false;
            }

            int rowwidth = oDTchanv.Rows.Count + 1;
            StringBuilder htmldesv = new StringBuilder("", 1000);

            try
            {
                htmldesv.Append("<br />");
                htmldesv.Append("<table border='1' align='center'>");
                htmldesv.Append("<tr>");
                htmldesv.Append("<td colspan = '" + rowwidth.ToString() + "' align='center'>Channels</td>");
                htmldesv.Append("</tr>");

                if (diagnostics == 1) swd.WriteLine("after headers");
                if (diagnostics == 1) swd.Flush();

                htmldesv.Append("<tr><td>Call1</td>");
                AddVRowstr(1, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Call2</td>");
                AddVRowstr(2, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Band</td>");
                AddVRowstr(3, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Chid</td>");
                AddVRowstr(4, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Freqtx</td>");
                AddVRowstr(5, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Poltx</td>");
                AddVRowstr(6, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Pwrtx</td>");
                AddVRowdbl(7, oDTchanv, htmldesv, 1);
                htmldesv.Append("<tr><td>Atpccde</td>");
                AddVRowstr(8, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Traftx</td>");
                AddVRowstr(9, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Eqpttx</td>");
                AddVRowstr(10, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Antnumbtx1</td>");
                AddVRowstr(11, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Afsltx1</td>");
                AddVRowdbl(12, oDTchanv, htmldesv, 1);
                htmldesv.Append("<tr><td>Antnumbtx2</td>");
                AddVRowstr(13, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Afsltx2</td>");
                AddVRowdbl(14, oDTchanv, htmldesv, 1);
                htmldesv.Append("<tr><td>Freqrx</td>");
                AddVRowstr(15, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Polrx</td>");
                AddVRowstr(16, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Antnumbrx1</td>");
                AddVRowstr(17, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Afslrx1</td>");
                AddVRowdbl(18, oDTchanv, htmldesv, 1);
                htmldesv.Append("<tr><td>Pwrrx1</td>");
                AddVRowdbl(19, oDTchanv, htmldesv, 1);
                htmldesv.Append("<tr><td>Antnumbrx2</td>");
                AddVRowstr(20, oDTchanv, htmldesv);
                htmldesv.Append("<tr><td>Afslrx2</td>");
                AddVRowdbl(21, oDTchanv, htmldesv, 1);
                htmldesv.Append("<tr><td>Pwrrx2</td>");
                AddVRowdbl(22, oDTchanv, htmldesv, 1);

                htmldesv.Append("</table>");
                htmldesvstr = htmldesv.ToString();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error writing channel info";
                swd.WriteLine("System error writing channel info:" + strSql + ":" + ex.Message);
                return false;
            }
        }
        private static void AddVRowstr(int index, DataTable oDTchanv, StringBuilder htmldesv)
        {
            foreach (DataRow oDRchanv in oDTchanv.Rows)
            {
                string var;
                if (oDRchanv[index] == DBNull.Value)
                {
                    var = "";
                }
                else
                {
                    var = Convert.ToString(oDRchanv[index]);
                }
                htmldesv.Append("<td>" + var + "</td>");
            }
            htmldesv.Append("</tr>");
        }
        private static void AddVRowdbl(int index, DataTable oDTchanv, StringBuilder htmldesv, int decimals)
        {
            foreach (DataRow oDRchanv in oDTchanv.Rows)
            {
                if (oDRchanv[index] != DBNull.Value)
                {
                    double var = Convert.ToDouble(oDRchanv[index]);
                    string format = "F" + decimals.ToString();
                    htmldesv.Append("<td>" + var.ToString(format) + "</td>");
                }
                else
                {
                    htmldesv.Append("<td></td>");
                }
            }
            htmldesv.Append("</tr>");
        }
        private static bool GetChanInfoH(string lcall, string rcall, string bndcde, out string htmldesstr)
        {
            htmldesstr = null;

            if (diagnostics == 1) swd.WriteLine("In GetChanInfoH: " + lcall + " " + rcall + " " + bndcde);
            if (diagnostics == 1) swd.Flush();

            string strSql = "SELECT RTRIM(call1), RTRIM(call2), bndcde, chid, freqtx, poltx, pwrtx, atpccde, traftx, eqpttx, " +
                            " antnumbtx1, afsltx1, antnumbtx2, afsltx2, freqrx, polrx, " +
                            " antnumbrx1, afslrx1, pwrrx1, antnumbrx2, afslrx2, pwrrx2 " +
                            " FROM " + ftchantable + " WHERE call1 = '" + lcall + "' AND call2 = '" + rcall + "' AND bndcde = '" + bndcde + "'" +
                            " ORDER BY call1, call2, bndcde, chid";

            dbconnect oCn = new dbconnect();

            DataTable oDTchans = null;
            try
            {
                oDTchans = oCn.retrieve(strSql);
                oCn.dbdisconnect();
            }
            catch (Exception e3)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting channel info\n";
                swd.WriteLine("ERRORSQL:" + strSql + ":" + e3.Message);
                return false;
            }

            StringBuilder htmldes = new StringBuilder("", 1000);

            try
            {
                htmldes.Append("<br />");
                htmldes.Append("<table border='1' align='center'>");
                htmldes.Append("<tr>");
                htmldes.Append("<td colspan = '22' align='center'>Channels</td>");
                htmldes.Append("</tr>");

                htmldes.Append("<tr>");
                htmldes.Append("<td>Call1</td>");
                htmldes.Append("<td>Call2</td>");
                htmldes.Append("<td>Band</td>");
                htmldes.Append("<td>Chid</td>");
                htmldes.Append("<td>Freqtx</td>");
                htmldes.Append("<td>Poltx</td>");
                htmldes.Append("<td>Pwrtx</td>");
                htmldes.Append("<td>Atpccde</td>");
                htmldes.Append("<td>Traftx</td>");
                htmldes.Append("<td>Eqpttx</td>");
                htmldes.Append("<td>Antnumbtx1</td>");
                htmldes.Append("<td>Afsltx1</td>");
                htmldes.Append("<td>Antnumbtx2</td>");
                htmldes.Append("<td>Afsltx2</td>");
                htmldes.Append("<td>Freqrx</td>");
                htmldes.Append("<td>Polrx</td>");
                htmldes.Append("<td>Antnumbrx1</td>");
                htmldes.Append("<td>Afslrx1</td>");
                htmldes.Append("<td>Pwrrx1</td>");
                htmldes.Append("<td>Antnumbrx2</td>");
                htmldes.Append("<td>Afslrx2</td>");
                htmldes.Append("<td>Pwrrx2</td>");
                htmldes.Append("</tr>");

                // get chan info
                using (OdbcConnection ucn1 = new OdbcConnection(cnstr))
                {
                    ucn1.Open();

                    // retrieve data into datareader
                    foreach (DataRow oDRchan in oDTchans.Rows)
                    {
                        if (diagnostics == 1) swd.WriteLine("In GetChanInfoH:foreach");
                        if (diagnostics == 1) swd.Flush();

                        htmldes.Append("<tr>");
                        htmldes.Append("<td>" + lcall + "</td>");
                        htmldes.Append("<td>" + rcall + "</td>");
                        htmldes.Append("<td>" + bndcde + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["chid"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["freqtx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["poltx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["pwrtx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["atpccde"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["traftx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["eqpttx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["antnumbtx1"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["afsltx1"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["antnumbtx2"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["afsltx2"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["freqrx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["polrx"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["antnumbrx1"]) + " </td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["afslrx1"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["pwrrx1"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["antnumbrx2"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["afslrx2"]) + "</td>");
                        htmldes.Append("<td>" + Convert.ToString(oDRchan["pwrrx2"]) + "</td>");
                        htmldes.Append("</tr>");
                    }
                }
                htmldes.Append("</table>");

                htmldesstr = htmldes.ToString();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error writing channel info\n";
                swd.WriteLine("Error writing channel info:" + ex.Message);
                return false;
            }
        }
        private static bool WriteGELinkH(DataRow oDRchan)
        {
            try
            {
                string lcall1 = Convert.ToString(oDRchan["call1"]);
                string rcall1 = Convert.ToString(oDRchan["call2"]);
                string bndcde = Convert.ToString(oDRchan["bndcde"]);

                //sw1.WriteLine("Processing link:" + lcall1 + "#" + lanum + "-" + rcall1);
                //sw1.WriteLine("Local call sign:" + lcall1);
                //sw1.Flush();

                string lname = Convert.ToString(oDRchan["lname"]);
                double ldlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llatit"])));    // s.intlatit
                double ldlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llongit"])));   // s.intlongit
                double ldsalt = Convert.ToDouble(oDRchan["lgrnd"]);

                string rname = Convert.ToString(oDRchan["rname"]);
                double rdlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlatit"])));    // s.intlatit
                double rdlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlongit"])));   // s.intlongit
                double rdsalt = Convert.ToDouble(oDRchan["rgrnd"]);

                string linkname = lcall1.TrimEnd() + "-" + rcall1.TrimEnd() + "-" + bndcde;
                // this draws a green line indicating link 
                double ptlong = ldlong + (rdlong - ldlong) / 5.0;
                double ptlat = ldlat + (rdlat - ldlat) / 5.0;
                double ptalt = ldsalt + (rdsalt - ldsalt) / 5.0;

                sw.WriteLine("<Placemark>");
                sw.WriteLine("<name align='center'>" + linkname + "</name>");
                sw.WriteLine("<description>" + LinkDescH(oDRchan) + "</description>");
                sw.WriteLine("<styleUrl>#green</styleUrl>");
                sw.WriteLine("<MultiGeometry>");
                sw.WriteLine("<Point>");
                sw.WriteLine("<altitudeMode>absolute</altitudeMode>");
                sw.WriteLine("<coordinates>" + ptlong.ToString(format6) + "," + ptlat.ToString(format6) + "," + ptalt.ToString(format2) + "</coordinates>");
                sw.WriteLine("</Point>");
                sw.WriteLine("<LineString>");
                sw.WriteLine("<altitudeMode>absolute</altitudeMode>");
                sw.WriteLine("<coordinates>");
                sw.WriteLine(ldlong.ToString(format6) + "," + ldlat.ToString(format6) + "," + ldsalt.ToString(format2));
                sw.WriteLine(rdlong.ToString(format6) + "," + rdlat.ToString(format6) + "," + rdsalt.ToString(format2));
                sw.WriteLine("</coordinates>");
                sw.WriteLine("</LineString>");
                sw.WriteLine("</MultiGeometry>");
                sw.WriteLine("</Placemark>");
                sw.Flush();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error writing link info\n";
                swd.WriteLine("Error writing link info:" + ex.Message);
                return false;
            }
        }
        private static bool WriteGELinkV(DataRow oDRchan)
        {
            try
            {
                string lcall1 = Convert.ToString(oDRchan["call1"]);
                string rcall1 = Convert.ToString(oDRchan["call2"]);
                string bndcde = Convert.ToString(oDRchan["bndcde"]);

                //sw1.WriteLine("Processing link:" + lcall1 + "#" + lanum + "-" + rcall1);
                //sw1.WriteLine("Local call sign:" + lcall1);
                //sw1.Flush();

                string lname = Convert.ToString(oDRchan["lname"]);
                double ldlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llatit"])));    // s.intlatit
                double ldlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["llongit"])));   // s.intlongit
                double ldsalt = Convert.ToDouble(oDRchan["lgrnd"]);

                string rname = Convert.ToString(oDRchan["rname"]);
                double rdlat = Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlatit"])));    // s.intlatit
                double rdlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(Convert.ToString(oDRchan["rlongit"])));   // s.intlongit
                double rdsalt = Convert.ToDouble(oDRchan["rgrnd"]);

                string linkname = lcall1.TrimEnd() + "-" + rcall1.TrimEnd() + "-" + bndcde;
                // this draws a green line indicating link 
                double ptlong = ldlong + (rdlong - ldlong) / 2.0;
                double ptlat = ldlat + (rdlat - ldlat) / 2.0;
                double ptalt = ldsalt + (rdsalt - ldsalt) / 2.0;

                string linkdescV = null;
                if (!LinkDescV(oDRchan, out linkdescV)) return false;

                sw.WriteLine("<Placemark>");
                sw.WriteLine("<name align='center'>" + linkname + "</name>");
                sw.WriteLine("<description>" + linkdescV + "</description>");
                sw.WriteLine("<styleUrl>#green</styleUrl>");
                sw.WriteLine("<MultiGeometry>");
                sw.WriteLine("<Point>");
                sw.WriteLine("<altitudeMode>absolute</altitudeMode>");
                sw.WriteLine("<coordinates>" + ptlong.ToString(format6) + "," + ptlat.ToString(format6) + "," + ptalt.ToString(format2) + "</coordinates>");
                sw.WriteLine("</Point>");
                sw.WriteLine("<LineString>");
                sw.WriteLine("<altitudeMode>absolute</altitudeMode>");
                sw.WriteLine("<coordinates>");
                sw.WriteLine(ldlong.ToString(format6) + "," + ldlat.ToString(format6) + "," + ldsalt.ToString(format2));
                sw.WriteLine(rdlong.ToString(format6) + "," + rdlat.ToString(format6) + "," + rdsalt.ToString(format2));
                sw.WriteLine("</coordinates>");
                sw.WriteLine("</LineString>");
                sw.WriteLine("</MultiGeometry>");
                sw.WriteLine("</Placemark>");
                sw.Flush();
                return true;
            }
            catch (Exception ex)
            {
                statusinfo += "System error writing link info\n";
                swd.WriteLine("Error writing link info:" + ex.Message);
                return false;
            }
        }
        //private static bool WriteSiteInfo(string pdfname, string ftchantable)
        private static bool WriteSiteInfo()
        {
            // get all call signs involved in links 
            // and create GE points

            schema = ctx.Session["s_schema"].ToString();

            dbconnect oCn = new dbconnect();

            string strSql = "";
            //if (bndcde == "all")  // include all data in PDF
            //{
            strSql = @"SELECT DISTINCT call1 FROM " + ftchantable +
                      " UNION " +
                      "SELECT DISTINCT call2 FROM " + ftchantable;
            //}
            //else // split data by bndcde (dropped from specs)
            ///{
            //    strSql = @"SELECT DISTINCT call1 FROM " + ftchantable + " WHERE bndcde = '" + bndcde + "'" +
            //              " UNION " +
            //              "SELECT call2 FROM " + bndcde + "' WHERE bncde = '" + bndcde + "'";
            //}


            DataTable oDTSites = null;
            try
            {
                oDTSites = oCn.retrieve(strSql);
                if (oDTSites.Rows.Count == 0)  // no rows retrieved
                {
                    statusinfo += "ERROR: This PDF contains no channels\n";
                    oCn.dbdisconnect();
                    //swd.WriteLine("ERROR:This PDF contains no channels");
                    return false;
                }

                oCn.dbdisconnect();
            }
            catch (Exception e3)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting channel call1s: " + e3.Message + ":" + strSql + ":\n";
                //swd.WriteLine("ERRORSQL:" + strSql + ":" + e3.Message);
                return false;
            }

            // get site info
            try
            {
                using (OdbcConnection ucn1 = new OdbcConnection(cnstr))
                {
                    ucn1.Open();

                    // retrieve data into datareader
                    foreach (DataRow oDRSite in oDTSites.Rows)
                    {
                        string lcall1 = Convert.ToString(oDRSite["call1"]);

                        // get name, lat, long, elevation from pdf site record
                        strSql = @"SELECT RTRIM(name) as name, latit, longit, grnd, oper FROM " + ftsitetable +
                            " WHERE call1 = '" + lcall1 + "'" +
                            " UNION " +
                            "SELECT RTRIM(name), latit, longit, grnd, oper FROM " + mtsitetable +
                            " WHERE call1 = '" + lcall1 + "'";

                        using (OdbcCommand select3 = new OdbcCommand(strSql, ucn1))
                        {
                            using (OdbcDataReader dr3 = select3.ExecuteReader())
                            {
                                if (dr3.HasRows)
                                {
                                    // there will only be one record
                                    dr3.Read();
                                    double dlat = Convert.ToDouble(LongLatUtils.decdeg(DBUtils.GetDBInt32(dr3, 1)));    // s.intlatit
                                    double dlong = -1.0 * Convert.ToDouble(LongLatUtils.decdeg(DBUtils.GetDBInt32(dr3, 2)));   // s.intlongit
                                    double dalt = Convert.ToDouble(DBUtils.GetDBFloat(dr3, 3, 1));
                                    if (!WriteSitePoint(lcall1, DBUtils.GetDBString(dr3, 0), dlong, dlat, dalt, DBUtils.GetDBString(dr3, 4))) return false;
                                }
                                else
                                {
                                    statusinfo += "ERROR: No site information for " + lcall1.Trim() + "\n";
                                    return false;
                                }
                                //dr3.Close();
                            } // end of reader
                        }  // end of command
                    } // end of foreach
                }  // end of connect
            }
            catch (Exception ex)
            {
                statusinfo += "System error selecting site info: " + ex.Message + ":" + strSql + ":\n";
                //swd.WriteLine("Error selecting site info:" + strSql + ":" + ex.Message);
                //swd.WriteLine("Error selecting site info:" + ":" + ex.Message);
                return false;
            }
            return true;
        }
        private static bool WriteLinkInfoH(string pdfname)
        {
            // this draws a green line representing the channel link

            string strSql = "";

            schema = ctx.Session["s_schema"].ToString();

            swd = new StreamWriter("D:\\extractlogs\\" + ctx.Session["s_user"].ToString() + "pdfkmldebugh.txt", false);

            dbconnect oCn = new dbconnect();

            // this step gets each channel in PDF linked to the relevant site 

            strSql = @"SELECT DISTINCT c.call1, c.call2, c.bndcde, " +
               " sl.name as lname, sl.latit as llatit, sl.longit as llongit, sl.grnd as lgrnd, sl.oper as loper,  " +
               " sr.name as rname, sr.latit as rlatit, sr.longit as rlongit, sr.grnd as rgrnd, sr.oper as roper " +
               " FROM " + ftchantable + " c " +
               " INNER JOIN " + ftsitetable + " sl ON c.call1 = sl.call1  " +
               " INNER JOIN " + ftsitetable + " sr ON c.call2 = sr.call1 " +
              "  ORDER BY c.call1, c.call2, c.bndcde ";


            DataTable oDTchan = null;
            try
            {
                oDTchan = oCn.retrieve(strSql);
                oCn.dbdisconnect();
            }
            catch (Exception e3)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting link info\n";
                swd.WriteLine("Error selecting link info:" + strSql + ":" + e3.Message);
                return false;
            }

            // no records found
            if (oDTchan.Rows.Count == 0)
            {
                return false;
            }

            // set up variables for previous keys
            string scall1 = "";
            string scall2 = "";
            string sbndcde = "";

            // set up variables for current keys
            string nscall1 = "";
            string nscall2 = "";
            string nsbndcde = "";

            int codechange = 0;

            // open placemark
            foreach (DataRow oDRchan in oDTchan.Rows)
            {
                // load current keys
                nscall1 = Convert.ToString(oDRchan["call1"]);
                nscall2 = Convert.ToString(oDRchan["call2"]);
                nsbndcde = Convert.ToString(oDRchan["bndcde"]);
                if (diagnostics == 1) swd.WriteLine("Data:" + scall1 + "/" + nscall1 + " " + scall2 + "/" + nscall2 + " " + sbndcde + "/" + nsbndcde);

                // compare current to prior keys
                codechange = 0;
                if (scall1 != nscall1) codechange += 4;
                if (scall2 != nscall2) codechange += 2;
                if (sbndcde != nsbndcde) codechange += 1;

                if (diagnostics == 1) swd.WriteLine("codechange:" + codechange.ToString());

                if (codechange == 0)
                {
                    statusinfo += "Duplicate channel selected:" + nscall1.Trim() + " " + nscall2.Trim() + " " + nsbndcde.Trim() + "\n";
                    swd.WriteLine("ERROR Duplicate Channel:" + nscall1.Trim() + " " + nscall2.Trim() + " " + nsbndcde.Trim());
                    return false;
                }

                switch (codechange)
                {
                    case 1:     // bndcde
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;
                    case 2:     // call2
                        // close placemark
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new call2:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;
                    case 3:     // call2, bndcde
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new call2, bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;
                    case 4:     // call1 
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new call1:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;
                    case 5:    // call1, bndcde
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new call1, bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;
                    case 6:    // call1, call2
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new call1, call2:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;
                    case 7:    // call1, call2, bndcde
                        if (!WriteLinkLineH(oDRchan)) return false;
                        if (diagnostics == 1) swd.WriteLine("new call1, call2, bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                        break;

                }
                // reset previous keys
                scall1 = nscall1;
                scall2 = nscall2;
                sbndcde = nsbndcde;

            }
            return true;
            //swd.Close();

        }
        private static bool WriteLinkInfoV(string pdfname)
        {
            // this draws a green line representing the channel link

            string strSql = "";

            schema = ctx.Session["s_schema"].ToString();

            swd = new StreamWriter("D:\\extractlogs\\" + ctx.Session["s_user"].ToString() + "pdfkmldebugv.txt", false);

            if (diagnostics == 1) swd.WriteLine("before GetUniqueLinks");
            if (diagnostics == 1) swd.Flush();

            DataTable oDTulinks = null;
            if (!GetUniqueLinks(out oDTulinks)) return false;

            //foreach (DataRow uDR in oDTulinks.Rows)
            //{
            //    if(diagnostics == 1)swd.WriteLine("ULV: " + Convert.ToString(uDR["call1"]) + Convert.ToString(uDR["call2"]));
            //}
            dbconnect oCn = new dbconnect();
            //if(diagnostics == 1)swd.Flush();

            if (diagnostics == 1) swd.WriteLine("after GetUniqueLinks");
            if (diagnostics == 1) swd.Flush();

            foreach (DataRow oDRulink in oDTulinks.Rows)
            {
                if (diagnostics == 1) swd.WriteLine("Processing: " + Convert.ToString(oDRulink["call1"]) + "-" + Convert.ToString(oDRulink["call2"]));
                if (diagnostics == 1) swd.Flush();
                // this step gets each channel in PDF linked to the relevant site 

                strSql = @"SELECT DISTINCT c.call1, c.call2, c.bndcde, " +
                   " sl.name as lname, sl.latit as llatit, sl.longit as llongit, sl.grnd as lgrnd, sl.oper as loper,  " +
                   " sr.name as rname, sr.latit as rlatit, sr.longit as rlongit, sr.grnd as rgrnd, sr.oper as roper " +
                   " FROM " + ftchantable + " c " +
                   " INNER JOIN " + ftsitetable + " sl ON c.call1 = sl.call1  " +
                   " INNER JOIN " + ftsitetable + " sr ON c.call2 = sr.call1 " +
                   " WHERE c.call1 = '" + Convert.ToString(oDRulink["call1"]).Trim() +
                   "' AND c.call2  = '" + Convert.ToString(oDRulink["call2"]).Trim() + "' ";

                if (diagnostics == 1) swd.WriteLine(strSql);
                if (diagnostics == 1) swd.Flush();

                DataTable oDTchan = null;
                try
                {
                    oDTchan = oCn.retrieve(strSql);
                    if (diagnostics == 1) swd.WriteLine("Data retrieved");
                    if (diagnostics == 1) swd.Flush();
                    //oCn.dbdisconnect();
                }
                catch (Exception e3)
                {
                    oCn.dbdisconnect();
                    statusinfo += "System error selecting link info\n";
                    swd.WriteLine("Error selecting link info:" + strSql + ":" + e3.Message);
                    swd.Flush();
                    return false;
                }

                // no records found
                if (oDTchan.Rows.Count == 0)
                {
                    if (diagnostics == 1) swd.WriteLine("NO LINK FOUND FOR:" + Convert.ToString(oDRulink["call1"]).Trim() + "-" + Convert.ToString(oDRulink["call2"]).Trim());
                    if (diagnostics == 1) swd.Flush();
                    sw.WriteLine("NO LINK FOUND FOR:" + Convert.ToString(oDRulink["call1"]).Trim() + "-" + Convert.ToString(oDRulink["call2"]).Trim());
                    //if(diagnostics == 1)swd.Close();
                    //return false;
                }
                else
                {
                    if (diagnostics == 1) swd.WriteLine("Processing link");
                    if (diagnostics == 1) swd.Flush();

                    // set up variables for previous keys
                    string scall1 = "";
                    string scall2 = "";
                    string sbndcde = "";

                    // set up variables for current keys
                    string nscall1 = "";
                    string nscall2 = "";
                    string nsbndcde = "";

                    int codechange = 0;

                    // open placemark
                    foreach (DataRow oDRchan in oDTchan.Rows)
                    {
                        if (diagnostics == 1) swd.WriteLine("Reading oDRchan");
                        if (diagnostics == 1) swd.Flush();

                        // load current keys
                        nscall1 = Convert.ToString(oDRchan["call1"]);
                        nscall2 = Convert.ToString(oDRchan["call2"]);
                        nsbndcde = Convert.ToString(oDRchan["bndcde"]);
                        if (diagnostics == 1) swd.WriteLine("Data:" + scall1 + "/" + nscall1 + " " + scall2 + "/" + nscall2 + " " + sbndcde + "/" + nsbndcde);

                        // compare current to prior keys
                        codechange = 0;
                        if (scall1 != nscall1) codechange += 4;
                        if (scall2 != nscall2) codechange += 2;
                        if (sbndcde != nsbndcde) codechange += 1;

                        if (diagnostics == 1) swd.WriteLine("codechange:" + codechange.ToString());

                        if (codechange == 0)
                        {
                            statusinfo += "Duplicate channel selected:" + nscall1.Trim() + " " + nscall2.Trim() + " " + nsbndcde.Trim() + "\n";
                            swd.WriteLine("ERROR Duplicate Channel:" + nscall1.Trim() + " " + nscall2.Trim() + " " + nsbndcde.Trim());
                            return false;
                        }

                        switch (codechange)
                        {
                            case 1:     // bndcde
                                if (diagnostics == 1) swd.WriteLine("new bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;
                            case 2:     // call2
                                if (diagnostics == 1) swd.WriteLine("new call2:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;
                            case 3:     // call2, bndcde
                                if (diagnostics == 1) swd.WriteLine("new call2, bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;
                            case 4:     // call1 
                                if (diagnostics == 1) swd.WriteLine("new call1:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;
                            case 5:    // call1, bndcde
                                if (diagnostics == 1) swd.WriteLine("new call1, bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;
                            case 6:    // call1, call2
                                if (diagnostics == 1) swd.WriteLine("new call1, call2:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;
                            case 7:    // call1, call2, bndcde
                                if (diagnostics == 1) swd.WriteLine("new call1, call2, bndcde:" + nscall1 + " " + nscall2 + " " + nsbndcde);
                                if (!WriteLinkLineV(oDRchan)) return false;
                                break;

                        }
                        // reset previous keys
                        scall1 = nscall1;
                        scall2 = nscall2;
                        sbndcde = nsbndcde;

                    }
                }
            }
            oCn.dbdisconnect();
            //swd.Close();
            return true;

        }
        private static bool GetUniqueLinks(out DataTable DTulinks)
        {
            string strSql = "SELECT DISTINCT call1, call2 FROM " + ftchantable +
                            " UNION " +
                            " SELECT DISTINCT call2, call1 FROM " + ftchantable +
                            " ORDER BY call1, call2";

            dbconnect oCn = new dbconnect();

            DTulinks = null;
            try
            {
                DTulinks = oCn.retrieve(strSql);
                oCn.dbdisconnect();
                return true;
            }
            catch (Exception e3)
            {
                oCn.dbdisconnect();
                statusinfo += "System error selecting distinct link info\n";
                swd.WriteLine("Error selecting distinct link info:" + strSql + ":" + e3.Message);
                return false;
            }

            /*
             * bool first = true;

            object[] keylookup = new object[2];

            foreach (DataRow DRlink in DTlinks.Rows)
            {
                if(diagnostics == 1)swu.WriteLine("Read: " + Convert.ToString(DRlink["call1"] + " " + Convert.ToString(DRlink["call2"])));
                if (first)  // load first record to DTulinks
                {
                    DataRow dr = DTulinks.NewRow();
                    dr["call1"] = Convert.ToString(DRlink["call1"]);
                    dr["call2"] = Convert.ToString(DRlink["call2"]);
                    DTulinks.Rows.Add(dr);
                    first = false;
                }
                else  // check against DTulinks
                {
                    // check if call1/call2 found in DTulinks
                    keylookup[0] = Convert.ToString(DRlink["call1"]);
                    keylookup[1] = Convert.ToString(DRlink["call2"]);
                    DataRow foundRow1 = DTulinks.Rows.Find(keylookup);
                    if (foundRow1 == null) // call1/call2 not found -check if call2/call1 found in DTulinks 
                    {
                        if(diagnostics == 1)swu.WriteLine("NOT FOUND: " + Convert.ToString(DRlink["call1"] + " " + Convert.ToString(DRlink["call2"])));
                        keylookup[0] = Convert.ToString(DRlink["call2"]);
                        keylookup[1] = Convert.ToString(DRlink["call1"]);
                        DataRow foundRow2 = DTulinks.Rows.Find(keylookup);
                        if (foundRow2 == null)  // call2/call1  not found - add record to DTulinks
                        {
                            if(diagnostics == 1)swu.WriteLine("NOT FOUND: " + Convert.ToString(DRlink["call2"] + " " + Convert.ToString(DRlink["call1"])));
                            DataRow drnew = DTulinks.NewRow();
                            drnew["call1"] = Convert.ToString(DRlink["call1"]);
                            drnew["call2"] = Convert.ToString(DRlink["call2"]);
                            DTulinks.Rows.Add(drnew);
                            if(diagnostics == 1)swu.WriteLine("ADDED: " + Convert.ToString(DRlink["call1"]) + " " + Convert.ToString(DRlink["call2"]));

                        }
                    }
                }
            }

            if(diagnostics == 1)swu.WriteLine(" ");
            foreach(DataRow uDR in DTulinks.Rows)
            {
                if(diagnostics == 1)swu.WriteLine("UL: " +  Convert.ToString(uDR["call1"]) + Convert.ToString(uDR["call2"]));
            }
            if(diagnostics == 1)swu.Close();
            return DTulinks;
           */
        }
        private static bool WriteLinkLineH(DataRow oDRchan)
        {
            if (!WriteGELinkH(oDRchan)) return false;

            return true;
        }
        private static bool WriteLinkLineV(DataRow oDRchan)
        {
            if (!WriteGELinkV(oDRchan)) return false;

            return true;
        }

    }
}
