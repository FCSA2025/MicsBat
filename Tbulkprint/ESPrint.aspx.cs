using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Threading;
using System.Web.UI.HtmlControls;
using DBUtilities;
using System.Runtime.InteropServices;
using LongLatUtilities;

namespace Tbulkprint
{
	/// <summary>
	/// Summary description for ESPrint.
	/// </summary>
	public partial class ESPrint : System.Web.UI.Page
	{
        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = true)]
        private extern static uint GetCurrentThreadId();

		private StringBuilder prt;
		private string cnstr;
		private OdbcConnection cn;
		private int form_count;

		protected void Page_Load(object sender, System.EventArgs e)
		{
            try
            {
                cnstr = Session["s_cnString"].ToString();
            }
            catch (Exception)
            {
                txtErrorMsg.Value = "Timeout";
                return;
            }
            
            txtErrorMsg.Value = "";
			form_count = 0;

            if (Page.IsPostBack == false)  // initial load
            {
                return; // this fires Jscript load function to get list of keys
                // for printing from the calling screen
            }
            else
            {
                prt = new StringBuilder("", 1000);
                cnstr = Session["s_cnString"].ToString();
                cn = new OdbcConnection(cnstr);
                cn.Open();

                string prtkeys = txtPrtList.Value;
                char[] delimiter1 = ":".ToCharArray();
                string[] keys = prtkeys.Split(delimiter1);

                char[] delimiter2 = ".".ToCharArray();
                string[] keyparts = keys[0].Split(delimiter2);

                string pdfid = keyparts[1];
                string schema = Session["s_schema"].ToString();

                string titlTable = schema + ".fe_" + pdfid + "_titl";
                string clocTable = schema + ".fe_" + pdfid + "_cloc";
                string chngTable = schema + ".fe_" + pdfid + "_chng";
                string siteTable = schema + ".fe_" + pdfid + "_site";
                string anteTable = schema + ".fe_" + pdfid + "_ante";
                string azimTable = schema + ".fe_" + pdfid + "_azim";
                string chanTable = schema + ".fe_" + pdfid + "_chan";

                for (int i = 0; i < keys.Length; i++)
                {
                    keyparts = keys[i].Split(delimiter2);

                    switch (keyparts[0])
                    {
                        case "t":  // title
                            if (!Print_Title(titlTable))
                            {
                                return;
                            }
                            break;

                        case "q":  // change of location
                            if (!Print_ChangeLoc(clocTable, keyparts[2], keyparts[3]))
                            {
                                return;
                            }
                            break;

                        case "g":  // change of call signs
                            if (!Print_ChangeCall(chngTable, keyparts[2], keyparts[3]))
                            {
                                return;
                            }
                            break;

                        case "d":  // site
                            if (!Print_Site(siteTable, keyparts[2]))
                            {
                                return;
                            }
                            break;

                        case "a":  // antenna
                            if (!Print_Ante(siteTable, anteTable, keyparts[2], keyparts[3]))
                            {
                                return;
                            }
                            break;

                        case "z":  // azimuth
                            if (!Print_Azim(siteTable, anteTable, azimTable, keyparts[2], keyparts[3]))
                            {
                                return;
                            }
                            break;

                        case "h":  // channel
                            if (!Print_Chan(siteTable, anteTable, chanTable, keyparts[2], keyparts[3], keyparts[4]))
                            {
                                return;
                            }
                            break;

                        default:
                            break;
                    }
                }

                cn.Close();

                data.InnerHtml = prt.ToString();

            }
		}
		private bool Print_Title(string titlTable)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            char[] datedelimiter = ".".ToCharArray();

            form_count++;

			string strSql = "SELECT validated, namef, source, descr, mdate" +
			                " FROM " + titlTable;
			
			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;
			
			OdbcCommand select4 = new OdbcCommand(strSql);
			select4.Connection = cn;
			OdbcDataReader dr4;
			
			try
			{
				dr4 = select4.ExecuteReader();
			}
			catch (Exception e1)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e1.Message;
				cn.Close();
				return false;
			}

			if(dr4.HasRows)
			{
				dr4.Read();

                // process mdate
                loc_mdate = DBUtils.GetDBString(dr4,4); // mdate
                if (loc_mdate.Length == 10)
                {
                    string[] mdate_parts = loc_mdate.Split(datedelimiter);
                    mDay = mdate_parts[2]; //  mDay 
                    mMonth = DBUtils.txtMonth(mdate_parts[1]);// mMonth
                    mYear = mdate_parts[0]; //  mYear
                }
                else
                {
                    mDay = ""; //  mDay 
                    mMonth = "";// mMonth
                    mYear = ""; //  mYear
                }

				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Earth Station Title Record<br/><br/></h3>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Filename</td>");
				prt.Append("	<td class='by' nowrap='nowrap'><input name='txtFileName' type='text' size='20' value='" + DBUtils.GetDBString(dr4,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Modify Date</td>");
				prt.Append("	<td class='by' nowrap='nowrap'>");
				prt.Append("		<input name='txtMdateDD' type='text' size='2' readonly='readonly' value='" + mDay + "'>");
				prt.Append("		<input name='txtMdateMM' type='text' size='4' readonly='readonly' value='" + mMonth + "'>");
				prt.Append("		<input name='txtMdateYY' type='text' size='8' readonly='readonly' value='" + mYear + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Source</td>");
				prt.Append("	<td class='by' nowrap='nowrap'>");
				prt.Append("		<input name='txtOperator' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr4,2) + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Description</td>");
				prt.Append("	<td class='by' nowrap='nowrap'>");
				prt.Append("		<TextArea name='txtDescription' readonly='readonly' rows='2' cols='20'>" + DBUtils.GetDBString(dr4,3) + "</TextArea>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Validated</td>");
				prt.Append("	<td class='by' nowrap='nowrap'>");
				prt.Append("		<input name='txtValidated' type='text' size='2' readonly='readonly' value='" + DBUtils.GetDBString(dr4,0) + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");
			}
			else
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS NOT FOUND ";
			}

			dr4.Close();
			
			return true;
		}
		private bool Print_ChangeLoc(string clocTable, string oldloc, string newloc)
		{
			form_count++;

			// get name associated with old location
			string strSql = "SELECT name FROM " + clocTable + " WHERE oldlocation='" + oldloc + "' AND newlocation='" + newloc + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;
			
			OdbcCommand select1 = new OdbcCommand(strSql);
			select1.Connection = cn;
			OdbcDataReader dr1;
			
			try
			{
				dr1 = select1.ExecuteReader();
			}
			catch (Exception e1)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e1.Message;
				cn.Close();
				return false;
			}

			if(dr1.HasRows)
			{
				dr1.Read();
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Earth Station Change Of Location</h3>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Current Location</td>");
				prt.Append("	<td nowrap='nowrap'>");
				prt.Append("		<input name='txtOldLocation' type='text' maxlength='10' readonly='readonly' value='" + oldloc + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Station Name</td>");
				prt.Append("	<td nowrap='nowrap'>");
				prt.Append("		<input name='txtName' type='text' readonly='readonly' value='" + DBUtils.GetDBString(dr1,0) + "'>");
				prt.Append("	</td>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>New Location</td>");
				prt.Append("	<td nowrap='nowrap'>");
				prt.Append("		<input name='txtNewLocation' readonly='readonly' type='text' value='" + newloc + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");
			}
			else
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS NOT FOUND ";
			}

			dr1.Close();

			return true;
		}
		private bool Print_ChangeCall(string chngTable, string oldcall, string newcall)
		{
			form_count++;

			prt.Append("<form name='form" + form_count.ToString() + "'>");
			prt.Append("<form name='form" + form_count.ToString() + "'>");
			prt.Append("<h3 align='center'>FCSA MICS Earth Station Change of Call Sign<br/><br/></h3>");
			prt.Append("<table align='center'>");
			prt.Append("<tr>");
			prt.Append("<td nowrap='nowrap'>Current Call Sign</td>");
			prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtOldCallSign' value='" + oldcall + "'></td>");
			prt.Append("</tr>");
			prt.Append("<tr>");
			prt.Append("	<td class='o' nowrap='nowrap'>New Call Sign</td>");
			prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNewCallSign' value='" + newcall + "'></td>");
			prt.Append("</tr>");
			prt.Append("</table>");
			prt.Append("<P CLASS='breakhere'>");
			prt.Append("</form>");

			return true;
		}
		private bool Print_Site(string siteTable, string loc)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            string loc_sdate = "";
            string sDay;
            string sMonth;
            string sYear;
            string txtLatDD;
			string txtLatMM;
			string txtLatSS;
			string txtLat00;
			string txtLatDir;
			string txtLongDD;
			string txtLongMM;
			string txtLongSS;
			string txtLong00;
			string txtLongDir;
            char[] datedelimiter = ".".ToCharArray();

			form_count++;
			
			// build SQL command to get site info
			string strSql = "SELECT cmd, recstat, location, name, prov, oper, latit, longit, grnd, stats, " +
			                "radio, rain, reg, nots, mdate, sdate " +
			                " FROM " + siteTable +
			                " WHERE location='" + loc + "'";

			// run query
			OdbcCommand select1 = new OdbcCommand(strSql);
			select1.Connection = cn;
			OdbcDataReader dr1;
			
			try
			{
				dr1 = select1.ExecuteReader();
			}
			catch (Exception e1)
			{
				txtErrorMsg.Value = "ERRORSQLd:" + strSql + ":" + e1.Message;
				cn.Close();
				return false;
			}

			if(dr1.HasRows)
			{
				// Record found - read data
				dr1.Read();
				
                // process mdate info
                loc_mdate = dr1.GetValue(14).ToString(); // mdate
                if (loc_mdate.Length == 10)
                {
                    string[] mdate_parts = loc_mdate.Split(datedelimiter);
                    mDay = mdate_parts[2]; //  mDay 
                    mMonth = DBUtils.txtMonth(mdate_parts[1]);// mMonth
                    mYear = mdate_parts[0]; //  mYear
                }
                else
                {
                    mDay = ""; //  mDay 
                    mMonth = "";// mMonth
                    mYear = ""; //  mYear
                }

                // process sdate info
                loc_sdate = dr1.GetValue(15).ToString(); // mdate
                if (loc_sdate.Length == 10)
                {
                    string[] sdate_parts = loc_sdate.Split(datedelimiter);
                    sDay = sdate_parts[2]; //  mDay 
                    sMonth = DBUtils.txtMonth(sdate_parts[1]);// mMonth
                    sYear = sdate_parts[0]; //  mYear
                }
                else
                {
                    sDay = ""; //  mDay 
                    sMonth = "";// mMonth
                    sYear = ""; //  mYear
                }
                // process latitude info
				LongLatUtils.SplitLat(dr1.GetValue(6).ToString());// 6 latit
				//txtErrorMsg.Value = "LAT:" + LongLatUtils.tmplat;
				txtLatDD = LongLatUtils.latitDD;
				txtLatMM = LongLatUtils.latitMM;
				txtLatSS = LongLatUtils.latitSS;
				txtLat00 = LongLatUtils.latit00;
				txtLatDir = LongLatUtils.latitDir;

				// process longitude info
				LongLatUtils.SplitLong(dr1.GetValue(7).ToString());// 7 longit
				//txtErrorMsg.Value = "LONG:" + LongLatUtils.tmplong;
				txtLongDD = LongLatUtils.longitDD;
				txtLongMM = LongLatUtils.longitMM;
				txtLongSS = LongLatUtils.longitSS;
				txtLong00 = LongLatUtils.longit00;
				txtLongDir = LongLatUtils.longitDir;

				// build HTML for this site form 
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Earth Station Site<br/><br/></h3>");
				prt.Append("<table borderColor='red' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<td class='o'>MDB Operation</td>");
				prt.Append("<td class='by' nowrap='nowrap' colSpan='3'>");
				prt.Append("<input type='text' size='2' name='txtMdbOperation' value='" + DBUtils.GetDBString(dr1,0) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Location Code</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtLocation' value='" + DBUtils.GetDBString(dr1,2) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Name</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly'  type='text' name='txtName' value='" + DBUtils.GetDBString(dr1,3) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Province</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly'  type='text' name='txtProvince' value='" + DBUtils.GetDBString(dr1,4) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Operator</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtOperator' value='" + DBUtils.GetDBString(dr1,5) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Latitude</td>");
                prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='3' name='txtLatDD' value='" + LongLatUtils.latitDD + "'>");
				prt.Append("-<input readonly='readonly' type='text' size='2' name='txtLatMM' value='" + LongLatUtils.latitMM + "'>");
				prt.Append("-<input readonly='readonly' type='text' size='2' name='txtLatSS' value='" + LongLatUtils.latitSS + "'>");
				prt.Append(".<input readonly='readonly' type='text' size='2' name='txtLat00' value='" + LongLatUtils.latit00 + "'>");
				prt.Append(" <input readonly='readonly' type='text' size='1' name='txtLatDir' value='" + LongLatUtils.latitDir + "'>");
				prt.Append("</td>");
				prt.Append("<td class='o' nowrap='nowrap'>Longitude</td>");
                prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtLongDD' value='" + LongLatUtils.longitDD + "'>");
				prt.Append("-<input readonly='readonly' type='text' size='2' name='txtLongMM' value='" + LongLatUtils.longitMM + "'>");
				prt.Append("-<input readonly='readonly' type='text' size='2' name='txtLongSS' value='" + LongLatUtils.longitSS + "'>");
				prt.Append(".<input readonly='readonly' type='text' size='2' name='txtLong00' value='" + LongLatUtils.longit00 + "'>");
				prt.Append(" <input readonly='readonly' type='text' size='1' name='txtLongDir' value='" + LongLatUtils.longitDir + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Ground Height</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtGroundHeight' value='" + DBUtils.GetDBFloat(dr1,8,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Site Status</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtSiteStatus' value='" + DBUtils.GetDBString(dr1,9) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Radio</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtRadio' value='" + DBUtils.GetDBString(dr1,10) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Rain</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtRain' value='" + dr1.GetValue(11).ToString() + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Region</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtRegion' value='" + DBUtils.GetDBString(dr1,12) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Notes</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNotes' value='" + DBUtils.GetDBString(dr1,13) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Service Date</td>");
				prt.Append("<td nowrap='nowrap'>");
				prt.Append("<input readonly='readonly' type='text' size='2' name='txtSdateDD' value='" + sDay + "'>");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtSdateMM' value='" + sMonth + "'>");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtSdateYY' value='" + sYear + "'>");
				prt.Append("</td>");
				prt.Append("<td class='o' nowrap='nowrap'>Modify Date</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtMdateDD' value='" + mDay + "'>");
				prt.Append("	<input readonly='readonly' type='text' size='4' name='txtMdateMM' value='" + mMonth + "'>");
				prt.Append("	<input readonly='readonly' type='text' size='4' name='txtMdateYY' value='" + mYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");

			}
			// close reader
			dr1.Close();

			return true;
		}
		private bool Print_Ante(string siteTable, string anteTable, string loc, string call1)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            char[] datedelimiter = ".".ToCharArray();
            string txtSiteName;  // name
			string txtOperator;  // operator
			string txtProvince; // province

			form_count++;
			
			//Get site related info for display (local)
			string strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE location='" + loc.ToUpper() + "'";
			OdbcCommand select2 = new OdbcCommand(strSql);
			select2.Connection = cn;
			OdbcDataReader dr2;
		
			try
			{
				dr2 = select2.ExecuteReader();
			}
			catch (Exception e2)
			{
				txtErrorMsg.Value = "ERRORSQLd:" + strSql + ":" + e2.Message;
				cn.Close();
				return false;
			}
			if(dr2.HasRows)
			{
				dr2.Read();
				txtSiteName = DBUtils.GetDBString(dr2,0);  // name
				txtOperator = DBUtils.GetDBString(dr2,1);  // operator
				txtProvince = DBUtils.GetDBString(dr2,2); // province
			}
			else
			{
				txtSiteName = "";  // name
				txtOperator = "";  // operator
				txtProvince = ""; // province
			}
			dr2.Close();

			// first two fields are dummies to align field numbers with edit screens
			strSql = "SELECT cmd, '1', location, call1, txband, rxband, acodetx, acoderx, " +
			         "g_t, lnat, aht, afslt, afslr, txhgmax, rxhgmax, satlong, satlongs, az, " +
			         "el, sarc1, sarc2, rxpre, txpre, rxtro, txtro, licence, satname, " +
			         "stata, nota, op2, antref, orbit, " +
			         "mdate " +
			         " FROM " + anteTable +
			         " WHERE location='" + loc + "'" +
			         " AND call1='" +call1 + "'";
		
			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;

			OdbcCommand select1 = new OdbcCommand(strSql);
			select1.Connection = cn;
			OdbcDataReader dr1;
			
			try
			{
				dr1 = select1.ExecuteReader();
			}
			catch (Exception e4)
			{
				//txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e4.Message;
				cn.Close();
				return false;
			}

			if(dr1.HasRows)
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS FOUND ";

				dr1.Read();

                // process mdate
                loc_mdate = dr1.GetValue(32).ToString(); // mdate
                if (loc_mdate.Length == 10)
                {
                    string[] mdate_parts = loc_mdate.Split(datedelimiter);
                    mDay = mdate_parts[2]; //  mDay 
                    mMonth = DBUtils.txtMonth(mdate_parts[1]);// mMonth
                    mYear = mdate_parts[0]; //  mYear
                }
                else
                {
                    mDay = ""; //  mDay 
                    mMonth = "";// mMonth
                    mYear = ""; //  mYear
                }

				// build HTML for this antenna form 
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Earth Station Antenna</h3>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Site</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='11' name='txtSiteLocation' value='" + loc + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='17' name='txtSiteName' value='" + txtSiteName + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='3' name='txtProvince' value='" + txtProvince + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtOperator' value='" + txtOperator + "'></td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table borderColor='red' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o'>MDB Operation</td>");
				prt.Append("<td class='by' nowrap='nowrap' colSpan='3'>");
				prt.Append("<input type='text' size='2' name='txtMdbOperation' value='" + DBUtils.GetDBString(dr1,0) + "'></td>");
				prt.Append("</tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Location Code</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtLocation' value='" + loc + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Call Sign</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtCall1' value='" + call1 + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Licence</td>");
				prt.Append("	<td nowrap='nowrap' colSpan='3'>");
                prt.Append("			<input readonly='readonly' type='text' size='13' name='txtLicenceNo' value='" + DBUtils.GetDBString(dr1, 25) + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Antenna Height</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtHeight' value='" + DBUtils.GetDBFloat(dr1,10,1) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Azimuth</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtAz' value='" + DBUtils.GetDBFloat(dr1,17,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Elevation Angle</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtEl' value='" + DBUtils.GetDBFloat(dr1,18,2) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Antenna<br/>Reference</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtAntref' value='" + DBUtils.GetDBInt32(dr1,30) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Gain/Temperature</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtg_t' value='" + DBUtils.GetDBFloat(dr1,8,1) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Noise<br/>Temperature</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtLnat' value='" + DBUtils.GetDBFloat(dr1,9,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Satellite Oper.</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtOp2' value='" + DBUtils.GetDBString(dr1,29) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Orbit</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtOrbit' value='" + DBUtils.GetDBString(dr1,31) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Satellite Name</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtSatname' value='" + DBUtils.GetDBString(dr1,26) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Satellite Longitude</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtSatlong' value='" + DBUtils.GetDBFloat(dr1,15,2) + "'>");
				prt.Append("		<input readonly='readonly' type='text' size='1' name='txtSatlongs' value='" + DBUtils.GetDBString(dr1,16) + "'> ");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Arc Orbit Center</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtSarc1' value='" + DBUtils.GetDBFloat(dr1,19,2) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Arc Half Width</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtSarc2' value='" + DBUtils.GetDBFloat(dr1,20,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Notes</td>");
				prt.Append("	<td nowrap='nowrap' colSpan='3'><input readonly='readonly' type='text' size='16' name='txtNotes' value='" + DBUtils.GetDBString(dr1,28) + "'>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				//prt.Append("	<td nowrap='nowrap' colSpan='3'>");
				//prt.Append("<table borderColor='navy' cellSpacing='0' cellPadding='0' border='0'>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<table align='center' borderColor='navy' cellSpacing='0' cellPadding='0' border='0'>");
				prt.Append("<tr>");
				prt.Append("	<td nowrap='nowrap'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>TRANSMIT</td>");
				prt.Append("	<td class='o' nowrap='nowrap'>RECEIVE</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Antenna Feed<br/>System Loss</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtAfslt' value='" + DBUtils.GetDBFloat(dr1,11,1) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtAfslr' value='" + DBUtils.GetDBFloat(dr1,12,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Band<br/> </td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtTxBand' value='" + DBUtils.GetDBString(dr1,4) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtRxBand' value='" + DBUtils.GetDBString(dr1,5) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Antenna Code</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtAcodetx' value='" + DBUtils.GetDBString(dr1,6) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtAcoderx' value='" + DBUtils.GetDBString(dr1,7) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Maximum<br/>Antenna Gain</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtTxhgmax' value='" + DBUtils.GetDBFloat(dr1,13,1) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtRxhgmax' value='" + DBUtils.GetDBFloat(dr1,14,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Precipitation<br/>Scatter Distance</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtTxpre' value='" + DBUtils.GetDBFloat(dr1,22,2) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtRxpre' value='" + DBUtils.GetDBFloat(dr1,21,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Tropospheric<br/>Scatter Distance</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtTxtro' value='" + DBUtils.GetDBFloat(dr1,24,2) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtRxtro' value='" + DBUtils.GetDBFloat(dr1,23,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Modify Date</td>");
				prt.Append("	<td nowrap='nowrap' colSpan='3'>");
				prt.Append("		<input readonly='readonly' type='text' size='2' name='txtMdateDD' value='" + mDay + "'>");
				prt.Append("		<input readonly='readonly' type='text' size='4' name='txtMdateMM' value='" + mMonth + "'>");
				prt.Append("		<input readonly='readonly' type='text' size='4' name='txtMdateYY' value='" + mYear + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Status</td>");
				prt.Append("	<td nowrap='nowrap' colSpan='3'><input readonly='readonly' type='text' size='16' name='txtAnteStatus' value='" + DBUtils.GetDBString(dr1,27) + "'></td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");
			}
			else
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS NOT FOUND ";
			}
			dr1.Close();
			
			return true;
		}
		private bool Print_Azim(string siteTable, string anteTable, string azimTable, string loc, string call1)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            char[] datedelimiter = ".".ToCharArray();
            string txtSiteName;    // name
			string txtOperator;    // operator
			string txtProvince;    // province
			string txtAnteCall1;   // 0 call1
			string txtAnteTxband = "";  // 1 txband
			string txtAnteRxband = "";  // 2 rxband
			string txtAnteAcodeTx = ""; // 3 acodetx
			string txtAnteAcodeRx = ""; // 4 acoderx

			form_count++;

			//Get site related info for display (local)
			string strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE location='" + loc + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;
			
			OdbcCommand select2 = new OdbcCommand(strSql);
			select2.Connection = cn;
			OdbcDataReader dr2;
		
			try
			{
				dr2 = select2.ExecuteReader();
			}
			catch (Exception e2)
			{
				txtErrorMsg.Value = "ERRORSQLd:" + strSql + ":" + e2.Message;
				cn.Close();
				return false;
			}
			if(dr2.HasRows)
			{
				dr2.Read();
				txtSiteName = DBUtils.GetDBString(dr2,0);  // name
				txtOperator = DBUtils.GetDBString(dr2,1);  // operator
				txtProvince = DBUtils.GetDBString(dr2,2);  // province
			}
			else
			{
				txtSiteName = "";  // name
				txtOperator = "";  // operator
				txtProvince = ""; // province
			}
			dr2.Close();

			// get info from antenna
			strSql = "SELECT call1, txband, rxband, acodetx, acoderx FROM " + anteTable + " WHERE location='" + loc + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;

			OdbcCommand select3 = new OdbcCommand(strSql);
			select3.Connection = cn;
			OdbcDataReader dr3;
			
			try
			{
				dr3 = select3.ExecuteReader();
			}
			catch (Exception e3)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e3.Message;
				cn.Close();
				return false;
			}

			if(dr3.HasRows)
			{
				dr3.Read();
				txtAnteCall1 = DBUtils.GetDBString(dr3,0);// 0 call1
				txtAnteTxband = DBUtils.GetDBString(dr3,1);// 1 txband
				txtAnteRxband = DBUtils.GetDBString(dr3,2);// 2 rxband
				txtAnteAcodeTx = DBUtils.GetDBString(dr3,3);// 3 acodetx
				txtAnteAcodeRx = DBUtils.GetDBString(dr3,4);// 4 acoderx
			}
			dr3.Close();

			// select azimuth data

			strSql = "SELECT cmd, azim, elev, dist, loss, " +
			         "mdate " +
			         " FROM " + azimTable +
			         " WHERE location='" + loc + "'" +
			         " AND call1='" + call1 + "'";
				
			OdbcCommand select1 = new OdbcCommand(strSql);
			select1.Connection = cn;
			OdbcDataReader dr1;
			
			try
			{
				dr1 = select1.ExecuteReader();
			}
			catch (Exception e4)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e4.Message;
				cn.Close();
				return false;
			}
				
			string azim;
			string mDate;

			if(dr1.HasRows)
			{
				// write table headers
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Earth Station Azimuth</h3>");
				prt.Append("<table align='center' bordercolor='navy' cellspacing='0' cellpadding='2' border='2'>");
				prt.Append("<tr>");
				prt.Append("<td class='h' nowrap='nowrap'>&nbsp;Cmd&nbsp;</td>");
				prt.Append("<td class='h' nowrap='nowrap'>&nbsp;Azimuth&nbsp;</td>");
				prt.Append("<td class='h' nowrap='nowrap'>&nbsp;Elevation&nbsp;</td>");
				prt.Append("<td class='h' nowrap='nowrap'>&nbsp;Distance&nbsp;</td>");
				prt.Append("<td class='h' nowrap='nowrap'>&nbsp;Loss&nbsp;</td>");
				prt.Append("<td class='h' nowrap='nowrap'>&nbsp;Modify Date&nbsp;</td>");

				while(dr1.Read())
				{
                    // process mdate
                    loc_mdate = dr1.GetValue(5).ToString(); // mdate
                    if (loc_mdate.Length == 10)
                    {
                        string[] mdate_parts = loc_mdate.Split(datedelimiter);
                        mDay = mdate_parts[2]; //  mDay 
                        mMonth = DBUtils.txtMonth(mdate_parts[1]);// mMonth
                        mYear = mdate_parts[0]; //  mYear
                    }
                    else
                    {
                        mDay = ""; //  mDay 
                        mMonth = "";// mMonth
                        mYear = ""; //  mYear
                    }


					mDate = mDay + "-" +  mMonth + "-" + mYear;
					azim = DBUtils.GetDBFloat(dr1,1,2);
					prt.Append("<tr>");
					prt.Append("<td class='az' nowrap='nowrap'>" + DBUtils.GetDBString(dr1,0) + "</td>");
					prt.Append("<td class='az' nowrap='nowrap'>" + azim + "</td>");
					prt.Append("<td class='az' nowrap='nowrap'>" + blankcell(DBUtils.GetDBFloat(dr1,2,2)) + "</TD>");
					prt.Append("<TD class='az' nowrap='nowrap'>" + blankcell(DBUtils.GetDBFloat(dr1,3,2)) + "</TD>");
					prt.Append("<TD class='az' nowrap='nowrap'>" + blankcell(DBUtils.GetDBFloat(dr1,4,2)) + "</td>");
					prt.Append("<td class='az' nowrap='nowrap'>" + blankcell(mDate )+ "</td>"); 
				}
			
				dr1.Close();

				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");

			}
			else
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS NOT FOUND ";
			}
			dr1.Close();

			return true;
		}
		private bool Print_Chan(string siteTable, string anteTable, string chanTable, string loc, string call1, string chid)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            char[] datedelimiter = ".".ToCharArray();
            string txtSiteName;    // name
			string txtOperator;    // operator
			string txtProvince;    // province
			string txtAnteCall1;   // 0 call1
			string txtAnteTxband = "";  // 1 txband
			string txtAnteRxband = "";  // 2 rxband
			string txtAnteAcodeTx = ""; // 3 acodetx
			string txtAnteAcodeRx = ""; // 4 acoderx

			form_count++;

			//Get site related info for display (local)
			string strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE location='" + loc + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;
			
			OdbcCommand select2 = new OdbcCommand(strSql);
			select2.Connection = cn;
			OdbcDataReader dr2;
		
			try
			{
				dr2 = select2.ExecuteReader();
			}
			catch (Exception e2)
			{
				txtErrorMsg.Value = "ERRORSQLd:" + strSql + ":" + e2.Message;
				cn.Close();
				return false;
			}
			if(dr2.HasRows)
			{
				dr2.Read();
				txtSiteName = DBUtils.GetDBString(dr2,0);  // name
				txtOperator = DBUtils.GetDBString(dr2,1);  // operator
				txtProvince = DBUtils.GetDBString(dr2,2);  // province
			}
			else
			{
				txtSiteName = "";  // name
				txtOperator = "";  // operator
				txtProvince = ""; // province
			}
			dr2.Close();

			// get info from antenna
			strSql = "SELECT call1, txband, rxband, acodetx, acoderx FROM " + anteTable + " WHERE location='" + loc + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;

			OdbcCommand select3 = new OdbcCommand(strSql);
			select3.Connection = cn;
			OdbcDataReader dr3;
			
			try
			{
				dr3 = select3.ExecuteReader();
			}
			catch (Exception e3)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e3.Message;
				cn.Close();
				return false;
			}

			if(dr3.HasRows)
			{
				dr3.Read();
				txtAnteCall1 = DBUtils.GetDBString(dr3,0);// 0 call1
				txtAnteTxband = DBUtils.GetDBString(dr3,1);// 1 txband
				txtAnteRxband = DBUtils.GetDBString(dr3,2);// 2 rxband
				txtAnteAcodeTx = DBUtils.GetDBString(dr3,3);// 3 acodetx
				txtAnteAcodeRx = DBUtils.GetDBString(dr3,4);// 4 acoderx
			}
			dr3.Close();


			// first two fields are dummies to align field numbers with edit screens
			strSql = "SELECT cmd, '1', location, call1, chid, freqtx, poltx, maxtxpower,  " +
			         "pwrtx, p4khz, eqpttx, traftx, stattx, feetx, freqrx, polrx, pwrrx, " +
			         "eqptrx, trafrx, statrx, i20, it01, ip01, feerx, notc, srvctx, srvcrx, " +
			         "mdate " +
			         " FROM " + chanTable +
			         " WHERE location='" + loc + "'" +
			         " AND call1='" +call1 + "'" +
			         " AND chid='" + chid + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;

			OdbcCommand select1 = new OdbcCommand(strSql);
			select1.Connection = cn;
			OdbcDataReader dr1;
			
			try
			{
				dr1 = select1.ExecuteReader();
			}
			catch (Exception e1)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e1.Message;
				cn.Close();
				return false;
			}

			if(dr1.HasRows)
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS FOUND ";
				dr1.Read();
			
                // process mdate
                loc_mdate = dr1.GetValue(27).ToString(); // mdate
                if (loc_mdate.Length == 10)
                {
                    string[] mdate_parts = loc_mdate.Split(datedelimiter);
                    mDay = mdate_parts[2]; //  mDay 
                    mMonth = DBUtils.txtMonth(mdate_parts[1]);// mMonth
                    mYear = mdate_parts[0]; //  mYear
                }
                else
                {
                    mDay = ""; //  mDay 
                    mMonth = "";// mMonth
                    mYear = ""; //  mYear
                }


				// build HTML for this channel form 
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Earth Station Channel</h3>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Site</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='11' name='txtSiteLocation' value='" + loc + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='17' name='txtSiteName' value='" + txtSiteName + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='3' name='txtProvince' value='" + txtProvince + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='19' name='txtOperator' value='" + txtOperator + "'>");
				prt.Append("	</td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Antenna</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='11' name='txtAnteCall1' value='" + call1 + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAnteTxband' value='" + txtAnteTxband + "'></TD>");
				prt.Append("	<TD nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAnteRxband' value='" + txtAnteRxband + "'></TD>");
				prt.Append("	<TD nowrap='nowrap'><input readonly='readonly' type='text' size='13' name='txtAnteAcodeTx' value='" + txtAnteAcodeTx + "'></TD>");
				prt.Append("	<TD nowrap='nowrap'><input readonly='readonly' type='text' size='13' name='txtAnteAcodeRx' value='" + txtAnteAcodeRx + "'></TD>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table borderColor='red' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<TD class='o'>MDB Operation</TD>");
				prt.Append("<TD class='by' nowrap='nowrap' colSpan='3'>");
				prt.Append("<input type='text' size='2' name='txtMdbOperation' value='" + DBUtils.GetDBString(dr1,0) + "'></TD>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<TD class='o' nowrap='nowrap'>Location Code</TD>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtLocation' value='" + loc + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Call Sign</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtCall1' value='" + call1 + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Channel Id</td>");
				prt.Append("	<td nowrap='nowrap' colSpan='3'><input readonly='readonly' type='text' size='16' name='txtChid' value='" + chid + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>LT<br/>Interference Obj.</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtI20' value='" + DBUtils.GetDBFloat(dr1,20,1) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>ST Prec.<br>Interference Obj.</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'  type='text' size='16' name='txtIp01' value='" + DBUtils.GetDBFloat(dr1,22,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Maximum<br>Transmit Power</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='16' name='txtMaxtxpower' value='" + DBUtils.GetDBFloat(dr1,7,1) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>ST Trop.<br>Interference Obj.</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='16' name='txtIt01' value='" + DBUtils.GetDBFloat(dr1,21,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Energy Dispersal</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly'   type='text' size='16' name='txtP4khz' value='" + DBUtils.GetDBFloat(dr1,9,1) + "'></td>");
				prt.Append("	<td class='o' nowrap='nowrap'>Notes</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtNotes' value='" + DBUtils.GetDBString(dr1,24) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td nowrap='nowrap' colSpan='4'>");
				prt.Append("</tr>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("	<td nowrap='nowrap'></td>");
				prt.Append("	<td class='b' nowrap='nowrap'>TRANSMIT</td>");
				prt.Append("	<td class='b' nowrap='nowrap'>RECEIVE</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Frequency</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtFreqtx' value='" + DBUtils.GetDBDouble(dr1,5,2) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtFreqrx' value='" + DBUtils.GetDBDouble(dr1,14,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Polarization</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtPoltx' value='" + DBUtils.GetDBString(dr1,6) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtPolrx' value='" + DBUtils.GetDBString(dr1,15) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Equipment</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtEqpttx' value='" + DBUtils.GetDBString(dr1,10) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtEqptrx' value='" + DBUtils.GetDBString(dr1,17) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Default/Normal Power</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtPwrtx' value='" + DBUtils.GetDBFloat(dr1,8,2) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtPwrrx' value='" + DBUtils.GetDBFloat(dr1,16,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Traffic Code</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtTraftx' value='" + DBUtils.GetDBString(dr1,11) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtTrafrx' value='" + DBUtils.GetDBString(dr1,18) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Status</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtStattx' value='" + DBUtils.GetDBString(dr1,12) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtStatrx' value='" + DBUtils.GetDBString(dr1,19) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Fee Code");
				prt.Append("	</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtFeetx' value='" + DBUtils.GetDBString(dr1,13) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtFeerx' value='" + DBUtils.GetDBString(dr1,23) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("	<td class='o' nowrap='nowrap'>Service Code</td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtSrvctx' value='" + DBUtils.GetDBString(dr1,25) + "'></td>");
				prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' size='16' name='txtSrvcrx' value='" + DBUtils.GetDBString(dr1,26) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' align='right'>");
				prt.Append("Modify Date</td>");
				prt.Append("<td  class='ro' nowrap='nowrap' colSpan='3'>");
				prt.Append("	<input readonly='readonly' type='text' size='2' name='txtMdateDD' value='" + mDay + "'>");
				prt.Append("	<input readonly='readonly' type='text' size='4' name='txtMdateMM' value='" + mMonth + "'>");
				prt.Append("	<input readonly='readonly' type='text' size='4' name='txtMdateYY' value='" + mYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");

			}
			else
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS NOT FOUND ";
			}
			dr1.Close();

			return true;
		}
		private string blankcell(string instr)
		{
			if(instr == "")
			{
				return "&nbsp;";
			}
			else
			{
				return instr;
			}
		}
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
