# Documented File: TSPrint.aspx.cs
**Repository Path:** `Tbulkprint\TSPrint.aspx.cs`
**Primary Layer:** `Tbulkprint`
**Namespace:** `Tbulkprint`

## Source Code Representation
```csharp
using System;
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
using System.Web.UI.HtmlControls;
using DBUtilities;
using LongLatUtilities;

namespace Tbulkprint
{
	/// <summary>
	/// Summary description for TSPrint.
	/// </summary>
	public partial class TSPrint : System.Web.UI.Page
	{

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

			if(Page.IsPostBack == false)  // initial load
			{
				return; // this fires Jscript load function to get list of keys
						// for printing from the calling screen
			}
			else
			{
				prt = new StringBuilder("",1000);
				cnstr = Session["s_cnString"].ToString();
				cn = new OdbcConnection(cnstr);
				cn.Open();

				string prtkeys = txtPrtList.Value;
				char [] delimiter1 = ":".ToCharArray();
				string [] keys = prtkeys.Split(delimiter1);

				char [] delimiter2 = ".".ToCharArray();
				string [] keyparts = keys[0].Split(delimiter2);

                string pdfid = keyparts[1];
                string schema = Session["s_schema"].ToString();

                string titlTable = schema + ".ft_" + pdfid + "_titl";
                string chngTable = schema + ".ft_" + pdfid + "_chng";
                string siteTable = schema + ".ft_" + pdfid + "_site";
                string anteTable = schema + ".ft_" + pdfid + "_ante";
                string chanTable = schema + ".ft_" + pdfid + "_chan";

				for (int i = 0; i < keys.Length; i++)
				{
					keyparts = keys[i].Split(delimiter2);
 
					switch(keyparts[0])
					{
						case "t":  // title
							if(!Print_Title(titlTable))
							{
								return;
							}
							break;
						
						case "g":  // change of call signs
							if(!Print_ChangeCall(chngTable, keyparts[2], keyparts[3]))
							{
								return;
							}
							break;

						case "d":  // site
							if(!Print_Site(siteTable, keyparts[2]))
							{
								return;
							}
							break;

						case "a":  // antenna
							if(!Print_Ante(siteTable, anteTable, keyparts[2], keyparts[3], keyparts[4], keyparts[5]))
							{
								return;
							}
							break;

						case "c":  // channel
							if(!Print_Chan(siteTable, chanTable, keyparts[2], keyparts[3], keyparts[4], keyparts[5]))
							{
								return;
							}
							break;

						default:
							break;
					}
				}
			}

			cn.Close();

			data.InnerHtml = prt.ToString();
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
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS FOUND ";

				dr4.Read();
				/*
				txtFileName.Value = dr4.GetValue(1).ToString();// 1 namef
				txtMdateDD.Value = dr4.GetValue(4).ToString();// 4 mDay 
				txtMdateMM.Value = DBUtils.txtMonth(dr4.GetValue(5).ToString());// 5 mMonth
				txtMdateYY.Value = dr4.GetValue(6).ToString();// 6 mYear
				txtOperator.Value = dr4.GetValue(2).ToString();// 2 source
				txtDescription.Value = dr4.GetValue(3).ToString();// 3 descr
				txtValidated.Value = dr4.GetValue(0).ToString();// 0 validated
				*/

                // process mdate info
                loc_mdate = DBUtils.GetDBString(dr4, 4); // mdate
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
				prt.Append("<h3 align='center'>FCSA MICS Terrestrial Title Record<br/><br/></h3>");
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
		private bool Print_ChangeCall(string chngTable, string oldcall, string newcall)
		{
			form_count++;

			// get name associated with old call1
				string strSql = "SELECT name FROM " + chngTable + " WHERE oldcall1='" + oldcall + "' AND newcall1='" + newcall + "'";

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
					prt.Append("<form name='form" + form_count.ToString() + "'>");
					prt.Append("<h3 align='center'>FCSA MICS Terrestrial Change of Call Sign<br/><br/></h3>");
					prt.Append("<table align='center'>");
					prt.Append("<tr>");
					prt.Append("<td nowrap='nowrap'>Current Call Sign</td>");
					prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtOldCallSign' value='" + oldcall + "'></td>");
					prt.Append("</tr>");
					prt.Append("<tr>");
					prt.Append("	<td nowrap='nowrap'>Name</td>");
					prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtName' size='36' value='" + DBUtils.GetDBString(dr1,0) + "'></td>");
					prt.Append("</tr>");
					prt.Append("<tr>");
					prt.Append("	<td class='o' nowrap='nowrap'>New Call Sign</td>");
					prt.Append("	<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNewCallSign' value='" + newcall + "'></td>");
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
		private bool Print_Site(string siteTable, string call1)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            char[] datedelimiter = ".".ToCharArray();
            string loc_sdate = "";
            string sDay;
            string sMonth;
            string sYear;
            form_count++;
			
			// build SQL command
			string strSql = "SELECT call1, name, prov, oper, latit, longit, grnd, stats, notwr, " +
			                "icaccount, mdate, sdate, " +
			                "nots, snumb, spoint, reg, loc, cmd " +
			                "FROM " + siteTable + " WHERE call1 = '" + call1 + "'";

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
                loc_mdate = dr1.GetValue(10).ToString(); // mdate
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
                loc_sdate = dr1.GetValue(11).ToString(); // mdate
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
				LongLatUtils.SplitLat(dr1.GetValue(4).ToString());// 4 latit
				// process longitude info
				LongLatUtils.SplitLong(dr1.GetValue(5).ToString());// 5 longit

				// build HTML for this site form 
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Terrestrial Site Data Display</h3>");
				prt.Append("<table borderColor='navy' cellSpacing='0' cellPadding='3' width='90%' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<td class='o'>MDB Operation</td>");
				prt.Append("<td class='by' nowrap='nowrap' colSpan='3'>");
				prt.Append("<input type='text' size='2' name='txtMdbOperation' value='" + DBUtils.GetDBString(dr1,17) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Call Sign Local</td>");
				prt.Append("<td nowrap='nowrap' width='19%'><input style='WIDTH: 60%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,0) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'></td>");
				prt.Append("<td nowrap='nowrap' width='21%'>&nbsp;</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Name</td>");
				prt.Append("<td nowrap='nowrap' width='19%' colSpan='3'><input readonly='readonly' type='text' size='36' value='" + DBUtils.GetDBString(dr1,1) + "' style='WIDTH: 80%'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Province</td>");
				prt.Append("<td nowrap='nowrap' width='19%'><input style='WIDTH: 25%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,2) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17'>Operator</td>");
				prt.Append("<td nowrap='nowrap' width='21%'><input style='WIDTH: 60%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,3) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Latitude</td>");
				prt.Append("<td nowrap='nowrap' width='19%'><input style='WIDTH: 21%' readonly='readonly' type='text' value='" + LongLatUtils.latitDD + "'>");
				prt.Append("-<input style='WIDTH: 17%' readonly='readonly' type='text' value='" + LongLatUtils.latitMM + "'>");
				prt.Append("-<input style='WIDTH: 17%' readonly='readonly' type='text' value='" + LongLatUtils.latitSS + "'>");
				prt.Append(".<input style='WIDTH: 17%' readonly='readonly' type='text' value='" + LongLatUtils.latit00 + "'>");
				prt.Append(" <input style='WIDTH: 17%' readonly='readonly' type='text' value='" + LongLatUtils.latitDir + "'>");
				prt.Append("</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'>Longitude</td>");
				prt.Append("<td nowrap='nowrap' width='21%'><input style='WIDTH: 25%' readonly='readonly' type='text' value='" + LongLatUtils.longitDD + "'>");
				prt.Append("-<input style='WIDTH: 16%' readonly='readonly' type='text' value='" + LongLatUtils.longitMM + "'>");
				prt.Append("-<input style='WIDTH: 16%' readonly='readonly' type='text' value='" + LongLatUtils.longitSS + "'>");
				prt.Append(".<input style='WIDTH: 16%' readonly='readonly' type='text' value='" + LongLatUtils.longit00 + "'>");
				prt.Append(" <input style='WIDTH: 25%' readonly='readonly' type='text' value='" + LongLatUtils.longitDir + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Ground Height</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='19%'><input style='WIDTH: 50%' readonly='readonly' type='text' value='" + DBUtils.GetDBFloat(dr1,6,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'>Site Status</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='21%'><input style='WIDTH: 20%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,7) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>No. of Towers	</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='19%'><input style='WIDTH: 20%' readonly='readonly' type='text' value='" + dr1.GetValue(8).ToString() + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'>IC Account</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='21%'><input style='WIDTH: 50%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,9) + "'>");
			
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Service Date</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='19%'><input style='WIDTH: 20%' readonly='readonly' type='text' value='" + sDay + "'>");
				prt.Append("-<input style='WIDTH: 30%' readonly='readonly' type='text' value='" + sMonth + "'>");
				prt.Append("-<input style='WIDTH: 30%' readonly='readonly' type='text' value='" + sYear + "'>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'>Modify Date</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='21%'><input style='WIDTH: 20%' readonly='readonly' type='text' value='" + mDay + "'>");
				prt.Append("-<input style='WIDTH: 30%' readonly='readonly' type='text' value='" + mMonth + "'>");
				prt.Append("-<input style='WIDTH: 30%' readonly='readonly' type='text' value='" + mYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Notes</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='19%'><input style='WIDTH: 30%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,12) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'>Site Number</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='21%'><input style='WIDTH: 40%' readonly='readonly' type='text' value='" + dr1.GetValue(13).ToString() + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Pointer</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='19%'><input style='WIDTH: 44%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,14) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' width='17%'>Region</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='21%'><input style='WIDTH: 25%' readonly='readonly' type='text' value='" + DBUtils.GetDBString(dr1,15) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap' width='18%'>Location");
				prt.Append("</td>");
				prt.Append("<td class='o' nowrap='nowrap' width='19%' colSpan='3'><input readonly='readonly' type='text' size='30' value='" + DBUtils.GetDBString(dr1,16) + "'></td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<P CLASS='breakhere'>");
				prt.Append("</form>");

			}
			// close reader
			dr1.Close();

			return true;
		}
		private bool Print_Ante(string siteTable, string anteTable, string call1, string call2, string bndcde, string anum)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            char[] datedelimiter = ".".ToCharArray();
            string loc_sdate = "";
            string sDay;
            string sMonth;
            string sYear;
            string txtNameLocal;  // name
			string txtOperLocal;  // operator
			string txtProvLocal; // province
			string txtNameRemote;  // name
			string txtOperRemote;  // operator
			string txtProvRemote; // province

			form_count++;
			
			//Get site related info for display (local)
			string strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE call1='" + call1.ToUpper() + "'";
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
				txtNameLocal = DBUtils.GetDBString(dr2,0);  // name
				txtOperLocal = DBUtils.GetDBString(dr2,1);  // operator
				txtProvLocal = DBUtils.GetDBString(dr2,2); // province
			}
			else
			{
				txtNameLocal = "";  // name
				txtOperLocal = "";  // operator
				txtProvLocal = ""; // province
			}
			dr2.Close();

			//Get site related info from mt_site for display (remote)
			strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE call1='" + call2.ToUpper() + "'";
			OdbcCommand select3 = new OdbcCommand(strSql);
			select3.Connection = cn;
			OdbcDataReader dr3;
		
			try
			{
				dr3 = select3.ExecuteReader();
			}
			catch (Exception e3)
			{
				txtErrorMsg.Value = "ERRORSQLd:" + strSql + ":" + e3.Message;
				cn.Close();
				return false;
			}

			if(dr3.HasRows)
			{
				dr3.Read();
				txtNameRemote = DBUtils.GetDBString(dr3,0);  // name
				txtOperRemote = DBUtils.GetDBString(dr3,1);  // operator
				txtProvRemote = DBUtils.GetDBString(dr3,2); // province
			}
			else
			{
				txtNameRemote = "";  // name
				txtOperRemote = "";  // operator
				txtProvRemote = ""; // province
			}
			dr3.Close();

			// first two fields are dummies to align field numbers with edit screens
			strSql ="SELECT cmd,'x', call1, call2, bndcde, anum, ause, acode, " +
			        "aht, azmth, elvtn, dist, offazm, tazmth, telvtn, tgain, " +
			        "txfdlnth, txfdlnlh, txfdlntv, txfdlnlv, rxfdlnth, rxfdlnlh, " +
			        "rxfdlntv, rxfdlnlv, txpadpam, rxpadlna, txcompl, " +
			        "rxcompl, obsloss, kvalue, atwrno, nota, apoint, " +
			        " mdate, sdate, licence " +
			        "FROM " + anteTable +
			        " WHERE " +
			        "call1='" + call1 + "'" +
			        " AND call2='" + call2+ "'" +
			        " AND bndcde='" + bndcde + "'" +
			        " AND anum=" + anum;
		
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
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e4.Message;
				cn.Close();
				return false;
			}

			if(dr1.HasRows)
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS FOUND ";

				dr1.Read();
                // process mdate info
                loc_mdate = dr1.GetValue(33).ToString(); // mdate
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
                loc_sdate = dr1.GetValue(34).ToString(); // mdate
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

				// build HTML for this antenna form 
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Terrestrial Antenna Display</h3>");
				prt.Append("<table style='WIDTH: 358px; HEIGHT: 39px' align='center'>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Local</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtCallLocal' value='" + call1.ToUpper() + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNameLocal' size='40' value='" + txtNameLocal + "'></td>");
				prt.Append("<td nowrap='nowrap'><input  readonly='readonly' type='text' size='3' name='txtProvLocal' value='" + txtProvLocal + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtOperLocal' value='" + txtOperLocal + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Remote</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtCallRemote' value='" + call2.ToUpper() + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNameRemote' size='40' value='" + txtNameRemote + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='3' name='txtProvRemote' value='" + txtProvRemote + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtOperRemote' value='" + txtOperRemote + "'></td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>MDB Operation</td>");
				prt.Append("<td class='by' nowrap='nowrap'>");
				prt.Append("<input type='text' size='2' name='txtMdbOperation' value='" + DBUtils.GetDBString(dr1,0) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Antenna No");
				prt.Append("</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='1' name='txtAntennaNo' value='" + DBUtils.GetDBInt16(dr1,5) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Tower No</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='1' name='txtTowerNo' value='" + DBUtils.GetDBInt8(dr1,30) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Band Code</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='1' name='txtBandCode' value='" + DBUtils.GetDBString(dr1,4) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Antenna Code</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='12' name='txtAntennaCode' value='" + DBUtils.GetDBString(dr1,7) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Use</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='3' name='txtUse' value='" + DBUtils.GetDBString(dr1,6) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Height");
				prt.Append("</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtHeight' value='" + DBUtils.GetDBFloat(dr1,8,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Azimuth</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtAzmth' value='" + DBUtils.GetDBFloat(dr1,9,2) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Elevation</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtElevation' value='" + DBUtils.GetDBFloat(dr1,10,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Distance</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtDistance' value='" + DBUtils.GetDBFloat(dr1,11,2) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Off Azimuth</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='1' name='txtOffAzm' value='" + DBUtils.GetDBString(dr1,12) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>True Azimuth</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtTazmth' value='" + DBUtils.GetDBFloat(dr1,13,2) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>True Elevation</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtTelvtn' value='" + DBUtils.GetDBFloat(dr1,14,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>True Gain</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtTgain' value='" + DBUtils.GetDBFloat(dr1,15,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>K Value</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtKvalue' value='" + DBUtils.GetDBFloat(dr1,29,2) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Obstruction Loss</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtObsloss' value='" + DBUtils.GetDBFloat(dr1,28,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Notes</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtNotes' value='" + DBUtils.GetDBString(dr1,31) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Pointer</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtPointer' value='" + DBUtils.GetDBString(dr1,32) + "'></td>");
                prt.Append("<td class='o' nowrap='nowrap'>Licence</td>");
                prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='15' name='txtLicence' value='" + DBUtils.GetDBString(dr1, 35) + "'></td>");
                prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table align='center'>");
				prt.Append("<tr>");
				prt.Append("<td nowrap='nowrap'></td>");
				prt.Append("<td class='b' nowrap='nowrap'>TRANSMIT</td>");
				prt.Append("<td class='b' nowrap='nowrap'>RECEIVE</td>");
				prt.Append("<td nowrap='nowrap'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Component Loss(dB)</td>");
				prt.Append("<td nowrap='nowrap'><input  readonly='readonly' type='text' size='4' name='txtTxCompl' value='" + DBUtils.GetDBFloat(dr1,26,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtRxCompl' value='" + DBUtils.GetDBFloat(dr1,27,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Feed Line Horizontal Type</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtTxFdlnth' value='" + DBUtils.GetDBString(dr1,16) + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtRxFdlnth' value='" + DBUtils.GetDBString(dr1,20) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Modify Date</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Feed Line Horizontal Length(m)</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtTxFdlnlh' value='" + DBUtils.GetDBFloat(dr1,17,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtRxFdlnlh' value='" + DBUtils.GetDBFloat(dr1,21,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>");
				prt.Append("	<input readonly='readonly' type='text' size='1' name='txtMdateDD' value='" + mDay + "'>");
				prt.Append("	<input readonly='readonly' type='text' size='3' name='txtMdateMM' value='" + mMonth + "'>");
				prt.Append("	<input readonly='readonly' type='text' size='3' name='txtMdateYY' value='" + mYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Feed Line Vertical Type></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtTxfdlntv' value='" + DBUtils.GetDBString(dr1,18) + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtRxfdlntv' value='" + DBUtils.GetDBString(dr1,22) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Service Date</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Feed Line Vertical Length(m)</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtTxfdlnlv' value='" + DBUtils.GetDBFloat(dr1,19,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtRxfdlnlv' value='" + DBUtils.GetDBFloat(dr1,23,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'>");
				prt.Append("    <input readonly='readonly' type='text' size='1' name='txtSdateDD' value='" + sDay + "'>");
				prt.Append("    <input readonly='readonly' type='text' size='3' name='txtSdateMM' value='" + sMonth + "'>");
				prt.Append("    <input readonly='readonly' type='text' size='3' name='txtSdateYY' value='" + sYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Pad/Amplifier(dB)</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtTxPadpam' value='" + DBUtils.GetDBFloat(dr1,24,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='4' name='txtRxPadlna' value='" + DBUtils.GetDBFloat(dr1,25,1) + "'></td>");
				prt.Append("<td nowrap='nowrap'></td>");
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
		private bool Print_Chan(string siteTable, string chanTable, string call1, string call2, string bndcde, string chid)
		{
            string loc_mdate = "";
            string mDay;
            string mMonth;
            string mYear;
            string loc_sdate = "";
            string sDay;
            string sMonth;
            string sYear;
            char[] datedelimiter = ".".ToCharArray();
            string txtNameLocal;  // name
			string txtOperLocal;  // operator
			string txtProvLocal; // province
			string txtNameRemote;  // name
			string txtOperRemote;  // operator
			string txtProvRemote; // province

			form_count++;

			//Get site related info for display (local)
			string strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE call1='" + call1.ToUpper() + "'";
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
				txtNameLocal = DBUtils.GetDBString(dr2,0);  // name
				txtOperLocal = DBUtils.GetDBString(dr2,1);  // operator
				txtProvLocal = DBUtils.GetDBString(dr2,2); // province
			}
			else
			{
				txtNameLocal = "";  // name
				txtOperLocal = "";  // operator
				txtProvLocal = ""; // province
			}
			dr2.Close();

			//Get site related info from mt_site for display (remote)
			strSql = "SELECT name, oper, prov FROM " + siteTable + " WHERE call1='" + call2.ToUpper() + "'";
			OdbcCommand select3 = new OdbcCommand(strSql);
			select3.Connection = cn;
			OdbcDataReader dr3;
		
			try
			{
				dr3 = select3.ExecuteReader();
			}
			catch (Exception e3)
			{
				txtErrorMsg.Value = "ERRORSQLd:" + strSql + ":" + e3.Message;
				cn.Close();
				return false;
			}

			if(dr3.HasRows)
			{
				dr3.Read();
				txtNameRemote = DBUtils.GetDBString(dr3,0);  // name
				txtOperRemote = DBUtils.GetDBString(dr3,1);  // operator
				txtProvRemote = DBUtils.GetDBString(dr3,2); // province
			}
			else
			{
				txtNameRemote = "";  // name
				txtOperRemote = "";  // operator
				txtProvRemote = ""; // province
			}
			dr3.Close();

			// first two fields are dummies to align field numbers with edit screens
			strSql = "SELECT cmd, 'x', call1, call2, bndcde, splan, hl, vh, chid, freqtx, " +
			         "poltx, antnumbtx1, antnumbtx2, eqpttx, eqptutx, pwrtx, " +
			         "atpccde, afsltx1, afsltx2, traftx, srvctx, stattx, freqrx, " +
			         "polrx, antnumbrx1, antnumbrx2, antnumbrx3, " +
			         "eqptrx, eqpturx, afslrx1, afslrx2, afslrx3, pwrrx1, pwrrx2, " +
			         "pwrrx3, trafrx, esint, tsint, srvcrx, statrx, routnumb, " +
			         "stnnumb, hopnumb, " +
			         "mdate, sdate, " +
			         "notetx, noterx, notegnl, cpoint, feetx, feerx " +
			         "  FROM " + chanTable +
			         " WHERE " +
			         "call1='" + call1 + "' AND " +
			         "call2='" + call2 + "' AND " +
			         "bndcde='" + bndcde + "' AND " +
			         "chid='" + chid + "'";

			//txtErrorMsg.Value = txtErrorMsg.Value + strSql;

			OdbcCommand select4 = new OdbcCommand(strSql);
			select4.Connection = cn;
			OdbcDataReader dr1;
			
			try
			{
				dr1 = select4.ExecuteReader();
			}
			catch (Exception e4)
			{
				txtErrorMsg.Value = "ERRORSQL:" + strSql + ":" + e4.Message;
				cn.Close();
				return false;
			}

			if(dr1.HasRows)
			{
				//txtErrorMsg.Value = txtErrorMsg.Value + " ROWS FOUND ";
				dr1.Read();

                // process mdate info
                loc_mdate = dr1.GetValue(43).ToString(); // mdate
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
                loc_sdate = dr1.GetValue(44).ToString(); // mdate
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
                // build HTML for this channel form 
				prt.Append("<form name='form" + form_count.ToString() + "'>");
				prt.Append("<h3 align='center'>FCSA MICS Terrestrial Channel Display</h3>");
				prt.Append("<table borderColor='blue' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Local</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtCallLocal' value='" + call1 + "'>&nbsp;</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNameLocal' size='40' value='" + txtNameLocal + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='3' name='txtProvLocal' value='" + txtProvLocal + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtOperLocal' value='" + txtOperLocal + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Band</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtBandCode' value='" + bndcde + "'>&nbsp;</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Remote</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtCallRemote' value='" + call2 + "'>&nbsp;</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' name='txtNameRemote' size='40' value='" + txtNameRemote + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='3' name='txtProvRemote' value='" + txtProvRemote + "'></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='10' name='txtOperRemote' value='" + txtOperRemote + "'></td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table borderColor='red' align='center'>");
				prt.Append("<tr>");
				prt.Append("<td class='o'>MDB Operation</td>");
				prt.Append("<td class='by' nowrap='nowrap' colSpan='3'>");
				prt.Append("<input type='text' size='2' name='txtMdbOperation' value='" + DBUtils.GetDBString(dr1,0) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Channel ID</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtChId' value='" + chid + "'>&nbsp;</td>");
				prt.Append("<td class='o' nowrap='nowrap'>FrequencyPlan</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtPlan' value='" + DBUtils.GetDBString(dr1,5) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Polarization (VH Code)</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtVh' value='" + DBUtils.GetDBInt8(dr1,7) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>HiLo</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtHilo' value='" + DBUtils.GetDBInt8(dr1,6) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Cumulative<br>Interference ES</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtEsint' value='" + DBUtils.GetDBFloat(dr1,36,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Cumulative<br>Interference TS</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtTsint' value='" + DBUtils.GetDBFloat(dr1,37,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table borderColor='purple' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<td></td>");
				prt.Append("<td></td>");
				prt.Append("<td></td>");
				prt.Append("<td class='b' nowrap='nowrap'>TRANSMIT</td>");
				prt.Append("<td></td>");
				prt.Append("<td></td>");
				prt.Append("<td></td>");
				prt.Append("<td class='b' nowrap='nowrap'>RECEIVE</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Frequency</td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap' colSpan='4'><input readonly='readonly' type='text' size='12' name='txtFreqtx' value='" + DBUtils.GetDBDouble(dr1,9,2) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' colSpan='6'><input readonly='readonly' type='text' size='12' name='txtFreqrx' value='" + DBUtils.GetDBDouble(dr1,22,2) + "'>&nbsp;</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td nowrap='nowrap'>Polarization</td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap' colSpan='4'><input readonly='readonly' type='text' size='12' name='txtPoltx' value='" + DBUtils.GetDBString(dr1,10) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap' colSpan='6'><input readonly='readonly' type='text' size='12' name='txtPolrx' value='" + DBUtils.GetDBString(dr1,23) + "'>&nbsp;</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Antenna<br>Number</td>");
				prt.Append("<td class='' nowrap='nowrap'>Mn:</td>");
				prt.Append("<td nowrap='nowrap'><input type='text' size='7' name='txtAntnumbtx1' value='" + DBUtils.GetDBInt8(dr1,11) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Stx:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAntnumbtx2' value='" + DBUtils.GetDBInt8(dr1,12) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Mn:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAntnumbrx1' value='" + DBUtils.GetDBInt8(dr1,24) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Dv1:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAntnumbrx2' value='" + DBUtils.GetDBInt8(dr1,25) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Dv2:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAntnumbrx3' value='" + DBUtils.GetDBInt8(dr1,26) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Antenna<br>FSL</td>");
				prt.Append("<td class='o' nowrap='nowrap'>Mn:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtAfsltx1' value='" + DBUtils.GetDBFloat(dr1,17,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Stx:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAfsltx2' value='" + DBUtils.GetDBFloat(dr1,18,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Mn:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAfslrx1' value='" + DBUtils.GetDBFloat(dr1,29,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Dv1:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAfslrx2' value='" + DBUtils.GetDBFloat(dr1,30,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Dv2:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAfslrx3' value='" + DBUtils.GetDBFloat(dr1,31,1) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Coordinated<br>Power</td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='7' name='txtPwrtx' value='" + DBUtils.GetDBFloat(dr1,15,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>ATPC<br>Range</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtAtpccde' value='" + DBUtils.GetDBFloat(dr1,16,1) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Mn:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtPwrrx1' value='" + DBUtils.GetDBFloat(dr1,32,1) + "'>&nbsp;</td>");
				prt.Append("<td class='o' nowrap='nowrap'>Dv1:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtPwrrx2' value='" + DBUtils.GetDBFloat(dr1,33,1) + "'>&nbsp;</td>");
				prt.Append("<td class='o' nowrap='nowrap'>Dv2:</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtPwrrx3' value='" + DBUtils.GetDBFloat(dr1,34,1) + "'>&nbsp;</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Equipment<br>Code/<font color='black'>Use</font>/Fee</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='9' name='txtEqpttx' value='" + DBUtils.GetDBString(dr1,13) + "'></td>");
				prt.Append("<td nowrap='nowrap' colSpan='2'><input readonly='readonly' type='text' size='1' name='txtEqptutx' value='" + DBUtils.GetDBString(dr1,14) + "'>");
				prt.Append("<input readonly='readonly' type='text' size='2' name='txtFeetx' value='" + DBUtils.GetDBString(dr1,49) + "'></td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap' colSpan='6'>");
				prt.Append("<input readonly='readonly' type='text' size='9' name='txtEqptrx' value='" + DBUtils.GetDBString(dr1,27) + "'>");
				prt.Append("<input readonly='readonly' type='text' size='1' name='txtEqpturx' value='" + DBUtils.GetDBString(dr1,28) + "'>");
				prt.Append("<input readonly='readonly' type='text' size='2' name='txtFeerx' value='" + DBUtils.GetDBString(dr1,50) + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Traffic<br>Code/Status</td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='8' name='txtTraftx' value='" + DBUtils.GetDBString(dr1,19) + "'></td>");
				prt.Append("<td nowrap='nowrap' colSpan='2'><input readonly='readonly' type='text' size='2' name='txtStattx' value='" + DBUtils.GetDBString(dr1,21) + "'></td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap' colSpan='6'><input readonly='readonly' type='text' size='8' name='txtTrafrx' value='" + DBUtils.GetDBString(dr1,35) + "'>");
				prt.Append("<input readonly='readonly' type='text' size='2' name='txtStatrx' value='" + DBUtils.GetDBString(dr1,39) + "'>&nbsp;");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Service<br>Code/Notes</td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='8' name='txtSrvctx' value='" + DBUtils.GetDBString(dr1,20) + "'></td>");
				prt.Append("<td nowrap='nowrap' colSpan='2'><input readonly='readonly' type='text' size='4' name='txtNotetx' value='" + DBUtils.GetDBString(dr1,45) + "'></td>");
				prt.Append("<td></td>");
				prt.Append("<td nowrap='nowrap' colSpan='6'><input readonly='readonly' type='text' size='8' name='txtSrvcrx' value='" + DBUtils.GetDBString(dr1,38) + "'>&nbsp;");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtNoterx' value='" + DBUtils.GetDBString(dr1,46) + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("</table>");
				prt.Append("<br>");
				prt.Append("<table borderColor='black' align='center' border='0'>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Hop Number</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtHopnumb' value='" + DBUtils.GetDBInt8(dr1,42) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Route Code</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='8' name='txtRoutnumb' value='" + dr1.GetValue(40).ToString() + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Station Number</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtStnnumb' value='" + DBUtils.GetDBInt8(dr1,41) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Notes General</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtNotegnl' value='" + DBUtils.GetDBString(dr1,47) + "'></td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td class='o' nowrap='nowrap'>Pointer</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='5' name='txtCpoint' value='" + DBUtils.GetDBString(dr1,48) + "'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Modify Date</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtMdateDD' value='" + mDay + "'>");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtMdateMM' value='" + mMonth + "'>");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtMdateYY' value='" + mYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
				prt.Append("<td nowrap='nowrap' colSpan='2'></td>");
				prt.Append("<td class='o' nowrap='nowrap'>Service Date</td>");
				prt.Append("<td nowrap='nowrap'><input readonly='readonly' type='text' size='2' name='txtSdateDD' value='" + sDay + "'>");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtSdateMM' value='" + sMonth + "'>");
				prt.Append("<input readonly='readonly' type='text' size='4' name='txtSdateYY' value='" + sYear + "'>");
				prt.Append("</td>");
				prt.Append("</tr>");
				prt.Append("<tr>");
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

```
