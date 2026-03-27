using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.SessionState;
using System.Text;
using System.Xml;


namespace DBAccess
{
    public enum eTsType { ft, mt };
    ////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is the support for the antenna and channels key arrays.
    /// </summary>
    public class TsAntennaKey
    {
        string m_call2;
        string m_bndcde;

        public string call2
        {
            get { return (m_call2); }
            set { m_call2 = value; }
        }

        public string bndcde
        {
            get { return (m_bndcde); }
            set { m_bndcde = value; }
        }

        public TsAntennaKey(string cCall2, string cBand)
        {
            m_call2 = cCall2;
            m_bndcde = cBand;
        }
    }


    public class DocFile : XmlDocument
    {
        protected string m_name; ///	Name of the file.

        /// <summary>
        /// Filename accessor
        /// </summary>
        public string filename
        {
            get { return (m_name); }
            set { m_name = value; }
        }

        /// <summary>
        /// Create a simple XML node of a given name with a given value of
        ///	general structure &lt;name>value&lt;/name>
        /// </summary>
        /// <param name="sNodeName">The name of the node</param>
        /// <param name="sNodeValue">The value, may be blank</param>
        /// <returns>XMLElement object for this node.</returns>
        public XmlElement XmlEl(string sNodeName,
                                      string sNodeValue)
        {
            XmlElement xEl = CreateElement(sNodeName);
            XmlText xText = CreateTextNode(sNodeValue);
            xEl.AppendChild(xText);

            return xEl;
        }


        public override string ToString()
        {
            return "\n" + m_name + ":-\n" + this.DocumentElement.InnerXml + "\n";
        }

    }


    /////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is the top level xml storage for the radios returned.  It creates an
    /// xml document.
    /// </summary>
    public class TsFile : DocFile
    {
        /// <summary>
        /// Create an XmlDocument that also represents a file of radios.
        ///	The radios themselves can be added manually to the DocumentElement of
        ///	this document.  The document element is called radios.
        /// </summary>
        /// <param name="inName">The name of the file to be created.</param>
        public TsFile(string inName) :
            base()
        {
            m_name = inName;
            this.AppendChild(this.CreateElement("radios"));
        }


        /// <summary>
        /// After creating or obtaining a radio element; this method is used to
        ///	add it to the XML document of all the radios.
        /// </summary>
        /// <param name="tsRadio">A ts radio</param>
        public void TsAddRadio(TsRadio oRad)
        {
            XmlNode xmlSite;

            xmlSite = this.DocumentElement.AppendChild(oRad.TsRadioXml(this));
            //	Add the antennas node:-
            XmlNode xmlAnts = xmlSite.AppendChild(CreateElement("antennas"));
            //	Add the antennas
            foreach (TsAntenna antenna in oRad.Antennas)
            {
                xmlAnts.AppendChild(antenna.TsAntennaXml(oRad.Type, this));
            }
            // Add the channel node:-
            XmlNode xmlChan = xmlSite.AppendChild(CreateElement("channels"));
            //	And now add the channels to this node
            foreach (TsChannel channel in oRad.Channels)
            {
                xmlChan.AppendChild(channel.TsChannelXml(oRad.Type, this));
            }
        }


        /// <summary>
        /// This routine will return the name of the appropriate site table 
        ///	given the type of table and the fill antenna or channel.
        /// </summary>
        /// <param name="etypein">The eTsType of the table.</param>
        /// <param name="sTable">The full table name</param>
        /// <returns>The correct site table name.</returns>
        public static string sitetable(DBAccess.eTsType etypein, string sTable)
        {
            string sTableOut;

            if (etypein == eTsType.mt)
            {
                sTableOut = "main.mt_site";
            }
            else
            {
                int nBegin = sTable.IndexOf("_") + 1;
                int nLen = sTable.LastIndexOf("_") - nBegin;
                sTableOut = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Substring(nBegin, nLen) + "_site";
            }

            return sTableOut;
        }

    }


    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is the base ts antenna information class.
    /// </summary>
    public class TsAntenna
    {
        protected internal eTsType m_eType;

        protected internal string m_cmd;
        protected internal string m_recstat;

        protected internal string m_call1;
        protected internal string m_call2;
        protected internal string m_bndcde;
        protected internal short m_anum;
        protected internal string m_ause;
        protected internal string m_acode;
        protected internal float m_aht;
        protected internal float m_azmth;
        protected internal float m_elvtn;
        protected internal float m_dist;
        protected internal string m_offazm;
        protected internal float m_tazmth;
        protected internal float m_telvtn;
        protected internal float m_tgain;
        protected internal string m_txfdlnth;
        protected internal float m_txfdlnlh;
        protected internal string m_txfdlntv;
        protected internal float m_txfdlnlv;
        protected internal string m_rxfdlnth;
        protected internal float m_rxfdlnlh;
        protected internal string m_rxfdlntv;
        protected internal float m_rxfdlnlv;
        protected internal float m_txpadpam;
        protected internal float m_rxpadlna;
        protected internal float m_txcompl;
        protected internal float m_rxcompl;
        protected internal float m_obsloss;
        protected internal float m_kvalue;
        protected internal short m_atwrno;
        protected internal string m_nota;
        protected internal string m_apoint;
        protected internal string m_sdate;
        protected internal string m_mdate;
        protected internal string m_mtime;
        protected internal string m_licence;
        protected internal string m_userid;

        protected internal string m_name2;	//	The name at the other end from OE site.
        protected internal string m_oper2;	//	The operator code at the other end.

        public DBAccess.eTsType Type
        {
            get { return m_eType; }
        }


        //	***********************************************************************
        //	Render the Insert Command to add this antenna record to the database 
        //	***********************************************************************
        protected string InsertCommand(eTsType eType, string sTable)
        {
            StringBuilder sbColumns = new StringBuilder(320);
            StringBuilder sbValues = new StringBuilder(320);
            string tableName;


            if (eType == eTsType.ft)
            {
                tableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_ante";

                sbColumns.Append("cmd,");
                sbValues.Append("'" + m_cmd + "',");

                sbColumns.Append("recstat,");
                sbValues.Append("'" + m_recstat + "',");
            }
            else
            {
                tableName = "main.mt_ante";
            }

            sbColumns.Append("call1,");
            sbValues.Append("'" + m_call1 + "',");

            sbColumns.Append("call2,");
            sbValues.Append("'" + m_call2 + "',");

            sbColumns.Append("bndcde,");
            sbValues.Append("'" + m_bndcde + "',");

            sbColumns.Append("anum,");
            sbValues.Append(m_anum.ToString() + ",");

            sbColumns.Append("ause,");
            sbValues.Append("'" + m_ause + "',");

            sbColumns.Append("acode,");
            sbValues.Append("'" + m_acode + "',");

            sbColumns.Append("aht,");
            sbValues.Append(m_aht.ToString() + ",");

            sbColumns.Append("azmth,");
            sbValues.Append(m_azmth.ToString() + ",");

            sbColumns.Append("elvtn,");
            sbValues.Append(m_elvtn.ToString() + ",");

            sbColumns.Append("dist,");
            sbValues.Append(m_dist.ToString() + ",");

            sbColumns.Append("offazm,");
            sbValues.Append("'" + m_offazm + "',");

            sbColumns.Append("tazmth,");
            sbValues.Append(m_tazmth.ToString() + ",");

            sbColumns.Append("telvtn,");
            sbValues.Append(m_telvtn.ToString() + ",");

            sbColumns.Append("tgain,");
            sbValues.Append(m_tgain.ToString() + ",");

            sbColumns.Append("txfdlnth,");
            sbValues.Append("'" + m_txfdlnth + "',");

            sbColumns.Append("txfdlnlh,");
            sbValues.Append(m_txfdlnlh.ToString() + ",");

            sbColumns.Append("txfdlntv,");
            sbValues.Append("'" + m_txfdlntv + "',");

            sbColumns.Append("txfdlnlv,");
            sbValues.Append(m_txfdlnlv.ToString() + ",");

            sbColumns.Append("rxfdlnth,");
            sbValues.Append("'" + m_rxfdlnth + "',");

            sbColumns.Append("rxfdlnlh,");
            sbValues.Append(m_rxfdlnlh.ToString() + ",");

            sbColumns.Append("rxfdlntv,");
            sbValues.Append("'" + m_rxfdlntv + "',");

            sbColumns.Append("rxfdlnlv,");
            sbValues.Append(m_rxfdlnlv.ToString() + ",");

            sbColumns.Append("txpadpam,");
            sbValues.Append(m_txpadpam.ToString() + ",");

            sbColumns.Append("rxpadlna,");
            sbValues.Append(m_rxpadlna.ToString() + ",");

            sbColumns.Append("txcompl,");
            sbValues.Append(m_txcompl.ToString() + ",");

            sbColumns.Append("rxcompl,");
            sbValues.Append(m_rxcompl.ToString() + ",");

            sbColumns.Append("obsloss,");
            sbValues.Append(m_obsloss.ToString() + ",");

            sbColumns.Append("kvalue,");
            sbValues.Append(m_kvalue.ToString() + ",");

            sbColumns.Append("atwrno,");
            sbValues.Append(m_atwrno.ToString() + ",");

            sbColumns.Append("nota,");
            sbValues.Append("'" + m_nota + "',");

            sbColumns.Append("apoint,");
            sbValues.Append("'" + m_apoint + "',");

            sbColumns.Append("sdate,");
            sbValues.Append("'" + m_sdate + "',");

            sbColumns.Append("mdate,");
            sbValues.Append("'" + m_mdate + "',");

            sbColumns.Append("mtime");
            sbValues.Append("'" + m_mtime + "'");

            sbColumns.Append("licence");
            sbValues.Append("'" + m_licence + "'");

            if (eType == eTsType.mt)
            {
                sbColumns.Append(",userid");
                sbValues.Append(",'" + m_userid + "'");
            }

            // Note: some fields that come from other tables are ignored, like name2, oper2

            return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
                    sbValues + ")");
        }


        //	***********************************************************************
        //	Just construct an uninitialized antenna record
        //	***********************************************************************
        public TsAntenna()
        {
            m_call1 = "-";			//	Uninitialized.
        }

        //	***********************************************************************
        //	Just construct an uninitialized antenna record of a specified type.
        //	***********************************************************************
        public TsAntenna(eTsType eType)
        {
            m_eType = eType;
            m_call1 = "-";
        }

        //	***********************************************************************
        //	This constructor will create an antenna from a datarow.  It makes the 
        //	assumption that the datarow is from an antenna table (either mt or ft),
        //	and it just reads the row into the new antenna object.
        //	***********************************************************************
        public TsAntenna(eTsType eType, DataRow oDR)
        {
            m_eType = eType;
            try
            {
                if (m_eType == eTsType.ft)
                {
                    m_cmd = oDR["cmd"].ToString();
                    m_recstat = oDR["recstat"].ToString();
                }

                m_call1 = oDR["call1"].ToString();
                m_call2 = oDR["call2"].ToString();
                m_bndcde = oDR["bndcde"].ToString();
                m_anum = Convert.ToInt16(oDR["anum"]);
                m_ause = oDR["ause"].ToString();
                m_acode = oDR["acode"].ToString();
                try { m_aht = Convert.ToSingle(oDR["aht"]); }
                catch { m_aht = 0.0f; }
                try { m_azmth = Convert.ToSingle(oDR["azmth"]); }
                catch { m_azmth = 0.0f; }
                try { m_elvtn = Convert.ToSingle(oDR["elvtn"]); }
                catch { m_elvtn = 0.0f; }
                try { m_dist = Convert.ToSingle(oDR["dist"]); }
                catch { m_dist = 0.0f; }
                m_offazm = oDR["offazm"].ToString();
                try { m_tazmth = Convert.ToSingle(oDR["tazmth"]); }
                catch { m_tazmth = 0.0f; }
                try { m_telvtn = Convert.ToSingle(oDR["telvtn"]); }
                catch { m_telvtn = 0.0f; }
                try { m_tgain = Convert.ToSingle(oDR["tgain"]); }
                catch { m_tgain = 0.0f; }
                m_txfdlnth = oDR["txfdlnth"].ToString();
                try { m_txfdlnlh = Convert.ToSingle(oDR["txfdlnlh"]); }
                catch { m_txfdlnlh = 0.0f; }
                m_txfdlntv = oDR["txfdlntv"].ToString();
                try { m_txfdlnlv = Convert.ToSingle(oDR["txfdlnlv"]); }
                catch { m_txfdlnlv = 0.0f; }
                m_rxfdlnth = oDR["rxfdlnth"].ToString();
                try { m_rxfdlnlh = Convert.ToSingle(oDR["rxfdlnlh"]); }
                catch { m_rxfdlnlh = 0.0f; }
                m_rxfdlntv = oDR["rxfdlntv"].ToString();
                try { m_rxfdlnlv = Convert.ToSingle(oDR["rxfdlnlv"]); }
                catch { m_rxfdlnlv = 0.0f; }
                try { m_txpadpam = Convert.ToSingle(oDR["txpadpam"]); }
                catch { m_txpadpam = 0.0f; }
                try { m_rxpadlna = Convert.ToSingle(oDR["rxpadlna"]); }
                catch { m_rxpadlna = 0.0f; }
                try { m_txcompl = Convert.ToSingle(oDR["txcompl"]); }
                catch { m_txcompl = 0.0f; }
                try { m_rxcompl = Convert.ToSingle(oDR["rxcompl"]); }
                catch { m_rxcompl = 0.0f; }
                try { m_obsloss = Convert.ToSingle(oDR["obsloss"]); }
                catch { m_obsloss = 0.0f; }
                try { m_kvalue = Convert.ToSingle(oDR["kvalue"]); }
                catch { m_kvalue = 0.0f; }
                try { m_atwrno = Convert.ToInt16(oDR["atwrno"]); }
                catch { m_atwrno = 0; }
                m_nota = oDR["nota"].ToString();
                m_apoint = oDR["apoint"].ToString();
                m_sdate = oDR["sdate"].ToString();
                m_mdate = oDR["mdate"].ToString();
                try
                {
                    m_sdate = DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                try
                {
                    m_mdate = DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_mtime = oDR["mtime"].ToString();
                m_licence = oDR["licence"].ToString();
                if (m_eType == eTsType.mt)
                {
                    m_userid = oDR["userid"].ToString();
                }
                //	Get the fields from other tables if they are present.
                try { m_name2 = oDR["name2"].ToString(); }
                catch { m_name2 = ""; }
                try { m_oper2 = oDR["oper2"].ToString(); }
                catch { m_oper2 = ""; }

            }
            catch (Exception e)
            {
                throw new Exception("Problem creating antenna:\n" + e.Message);
            }
        }

        //	************************************************************************
        //	This constructor will read in an antenna record whose key is completely
        //	known, from a table whose name is known (either main.mt_ante or an ft table).
        //	************************************************************************
        public TsAntenna(eTsType eType,
                         string cCall1,
                         string cCall2,
                         string cBand,
                         int nAnum,
                         string cTable)
        {
            //	Connect and get the antenna
            dbconnect oConnection = new dbconnect();

            string cSite = TsFile.sitetable(eType, cTable);

            string antCommand = "SELECT a.*,s.name as name2,s.oper as oper2" +
                             "FROM " + cTable + " a left outer join " +
                                   cSite + " s on (a.call2 = s.call1) " +
                                   " WHERE a.call1='" + cCall1.ToUpper() + "' " +
                                      "and a.call2='" + cCall2.ToUpper() + "' " +
                                      "and a.bndcde='" + cBand.ToUpper() + "' " +
                                      "and a.anum=" + nAnum.ToString();

            m_eType = eType;
            m_cmd = "-";
            m_recstat = "-";
            m_userid = "-";

            OdbcDataAdapter oAdapter = new OdbcDataAdapter(antCommand,
                                                             oConnection.Connection);
            DataSet oDS = new DataSet();
            oAdapter.Fill(oDS);

            // We assume that we have retrieved only one record (or zero)
            DataTable oDT = oDS.Tables[0];
            if (oDT.Rows.Count > 0)
            {
                //	Got something.  We are just interested in the first row.
                DataRow oDR = oDT.Rows[0];

                if (m_eType == eTsType.ft)
                {
                    m_cmd = oDR["cmd"].ToString();
                    m_recstat = oDR["recstat"].ToString();
                }

                m_call1 = oDR["a.call1"].ToString();
                m_call2 = oDR["call2"].ToString();
                m_bndcde = oDR["bndcde"].ToString();
                try { m_anum = Convert.ToInt16(oDR["anum"]); }
                catch { m_anum = 0; }
                m_ause = oDR["ause"].ToString();
                m_acode = oDR["acode"].ToString();
                try { m_aht = Convert.ToSingle(oDR["aht"]); }
                catch { m_aht = 0.0f; }
                try { m_azmth = Convert.ToSingle(oDR["azmth"]); }
                catch { m_azmth = 0.0f; }
                try { m_elvtn = Convert.ToSingle(oDR["elvtn"]); }
                catch { m_elvtn = 0.0f; }
                try { m_dist = Convert.ToSingle(oDR["dist"]); }
                catch { m_dist = 0.0f; }
                m_offazm = oDR["offazm"].ToString();
                try { m_tazmth = Convert.ToSingle(oDR["tazmth"]); }
                catch { m_tazmth = 0.0f; }
                try { m_telvtn = Convert.ToSingle(oDR["telvtn"]); }
                catch { m_telvtn = 0.0f; }
                try { m_tgain = Convert.ToSingle(oDR["tgain"]); }
                catch { m_tgain = 0.0f; }
                m_txfdlnth = oDR["txfdlnth"].ToString();
                try { m_txfdlnlh = Convert.ToSingle(oDR["txfdlnlh"]); }
                catch { m_txfdlnlh = 0.0f; }
                m_txfdlntv = oDR["txfdlntv"].ToString();
                try { m_txfdlnlv = Convert.ToSingle(oDR["txfdlnlv"]); }
                catch { m_txfdlnlv = 0.0f; }
                m_rxfdlnth = oDR["rxfdlnth"].ToString();
                try { m_rxfdlnlh = Convert.ToSingle(oDR["rxfdlnlh"]); }
                catch { m_rxfdlnlh = 0.0f; }
                m_rxfdlntv = oDR["rxfdlntv"].ToString();
                try { m_rxfdlnlv = Convert.ToSingle(oDR["rxfdlnlv"]); }
                catch { m_rxfdlnlv = 0.0f; }
                try { m_txpadpam = Convert.ToSingle(oDR["txpadpam"]); }
                catch { m_txpadpam = 0.0f; }
                try { m_rxpadlna = Convert.ToSingle(oDR["rxpadlna"]); }
                catch { m_rxpadlna = 0.0f; }
                try { m_txcompl = Convert.ToSingle(oDR["txcompl"]); }
                catch { m_txcompl = 0.0f; }
                try { m_rxcompl = Convert.ToSingle(oDR["rxcompl"]); }
                catch { m_rxcompl = 0.0f; }
                try { m_obsloss = Convert.ToSingle(oDR["obsloss"]); }
                catch { m_obsloss = 0.0f; }
                try { m_kvalue = Convert.ToSingle(oDR["kvalue"]); }
                catch { m_kvalue = 0.0f; }
                try { m_atwrno = Convert.ToInt16(oDR["atwrno"]); }
                catch { m_atwrno = 0; }
                m_nota = oDR["nota"].ToString();
                m_apoint = oDR["apoint"].ToString();
                m_sdate = oDR["sdate"].ToString();
                try
                {
                    m_sdate = DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_mdate = oDR["mdate"].ToString();
                try
                {
                    m_mdate = DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_mtime = oDR["mtime"].ToString();
                m_licence = oDR["licence"].ToString(); 
                if (m_eType == eTsType.mt)
                {
                    m_userid = oDR["userid"].ToString();
                }
                try { m_name2 = oDR["name2"].ToString(); }
                catch { m_name2 = ""; }
                try { m_oper2 = oDR["oper2"].ToString(); }
                catch { m_oper2 = ""; }
            }

            oConnection.dbdisconnect();
        }


        //	*********************************************************************
        //	Copy an antenna record into a newly created antenna.
        //	*********************************************************************
        protected void TsCopAnt(eTsType eType, TsAntenna tAnt)
        {
            if (eType == eTsType.ft)
            {
                m_cmd = tAnt.m_cmd;
                m_recstat = tAnt.m_recstat;
            }

            m_call1 = tAnt.m_call1;
            m_call2 = tAnt.m_call2;
            m_bndcde = tAnt.m_bndcde;
            m_anum = tAnt.m_anum;
            m_ause = tAnt.m_ause;
            m_acode = tAnt.m_acode;
            m_aht = tAnt.m_aht;
            m_azmth = tAnt.m_azmth;
            m_elvtn = tAnt.m_elvtn;
            m_dist = tAnt.m_dist;
            m_offazm = tAnt.m_offazm;
            m_tazmth = tAnt.m_tazmth;
            m_telvtn = tAnt.m_telvtn;
            m_tgain = tAnt.m_tgain;
            m_txfdlnth = tAnt.m_txfdlnth;
            m_txfdlnlh = tAnt.m_txfdlnlh;
            m_txfdlntv = tAnt.m_txfdlntv;
            m_txfdlnlv = tAnt.m_txfdlnlv;
            m_rxfdlnth = tAnt.m_rxfdlnth;
            m_rxfdlnlh = tAnt.m_rxfdlnlh;
            m_rxfdlntv = tAnt.m_rxfdlntv;
            m_rxfdlnlv = tAnt.m_rxfdlnlv;
            m_txpadpam = tAnt.m_txpadpam;
            m_rxpadlna = tAnt.m_rxpadlna;
            m_txcompl = tAnt.m_txcompl;
            m_rxcompl = tAnt.m_rxcompl;
            m_obsloss = tAnt.m_obsloss;
            m_kvalue = tAnt.m_kvalue;
            m_atwrno = tAnt.m_atwrno;
            m_nota = tAnt.m_nota;
            m_apoint = tAnt.m_apoint;
            m_sdate = tAnt.m_sdate;
            m_mdate = tAnt.m_mdate;
            m_mtime = tAnt.m_mtime;
            m_licence = tAnt.m_licence;

            if (eType == eTsType.mt)
            {
                m_userid = tAnt.m_userid;
            }

            m_name2 = tAnt.m_name2;
            m_oper2 = tAnt.m_oper2;

            return;
        }


        public XmlElement TsAntennaXml(eTsType eType,
                                       TsFile oFile)
        {
            XmlElement tAntennaEl = oFile.CreateElement("antenna");

            if (eType == eTsType.ft)
            {
                tAntennaEl.AppendChild(oFile.XmlEl("cmd", m_cmd));
                tAntennaEl.AppendChild(oFile.XmlEl("recstat", m_recstat));
            }
            tAntennaEl.AppendChild(oFile.XmlEl("call1", m_call1));
            tAntennaEl.AppendChild(oFile.XmlEl("call2", m_call2));
            tAntennaEl.AppendChild(oFile.XmlEl("bndcde", m_bndcde));
            tAntennaEl.AppendChild(oFile.XmlEl("anum", m_anum.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("ause", m_ause));
            tAntennaEl.AppendChild(oFile.XmlEl("acode", m_acode));
            tAntennaEl.AppendChild(oFile.XmlEl("aht", m_aht.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("azmth", m_azmth.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("elvtn", m_elvtn.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("dist", m_dist.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("offazm", m_offazm));
            tAntennaEl.AppendChild(oFile.XmlEl("tazmth", m_tazmth.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("telvtn", m_telvtn.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("tgain", m_tgain.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("txfdlnth", m_txfdlnth));
            tAntennaEl.AppendChild(oFile.XmlEl("txfdlnlh", m_txfdlnlh.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("txfdlntv", m_txfdlntv));
            tAntennaEl.AppendChild(oFile.XmlEl("txfdlnlv", m_txfdlnlv.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("rxfdlnth", m_rxfdlnth));
            tAntennaEl.AppendChild(oFile.XmlEl("rxfdlnlh", m_rxfdlnlh.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("rxfdlntv", m_rxfdlntv));
            tAntennaEl.AppendChild(oFile.XmlEl("rxfdlnlv", m_rxfdlnlv.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("txpadpam", m_txpadpam.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("rxpadlna", m_rxpadlna.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("txcompl", m_txcompl.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("rxcompl", m_rxcompl.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("obsloss", m_obsloss.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("kvalue", m_kvalue.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("atwrno", m_atwrno.ToString()));
            tAntennaEl.AppendChild(oFile.XmlEl("nota", m_nota));
            tAntennaEl.AppendChild(oFile.XmlEl("apoint", m_apoint));
            tAntennaEl.AppendChild(oFile.XmlEl("sdate", m_sdate));
            tAntennaEl.AppendChild(oFile.XmlEl("mdate", m_mdate));
            tAntennaEl.AppendChild(oFile.XmlEl("mtime", m_mtime));
            tAntennaEl.AppendChild(oFile.XmlEl("licence", m_licence));
            if (eType == eTsType.mt)
            {
                tAntennaEl.AppendChild(oFile.XmlEl("userid", m_userid));
            }
            tAntennaEl.AppendChild(oFile.XmlEl("name2", m_name2));
            tAntennaEl.AppendChild(oFile.XmlEl("oper2", m_oper2));

            return tAntennaEl;
        }
    }


    //********************************************************************
    /// <summary>
    /// This is the channel information for both mdb and ft channels.
    /// </summary>
    //********************************************************************
    public class TsChannel
    {

        protected eTsType m_eType;

        protected string m_cmd;
        protected string m_recstat;

        protected string m_call1;
        protected string m_call2;
        protected string m_bndcde;
        protected string m_splan;
        protected short m_hl;
        protected short m_vh;
        protected string m_chid;
        protected double m_freqtx;
        protected string m_poltx;
        protected short m_antnumbtx1;
        protected short m_antnumbtx2;
        protected string m_eqpttx;
        protected string m_eqptutx;
        protected float m_pwrtx;
        protected float m_atpccde;
        protected float m_afsltx1;
        protected float m_afsltx2;
        protected string m_traftx;
        protected string m_srvctx;
        protected string m_stattx;
        protected double m_freqrx;
        protected string m_polrx;
        protected short m_antnumbrx1;
        protected short m_antnumbrx2;
        protected short m_antnumbrx3;
        protected string m_eqptrx;
        protected string m_eqpturx;
        protected float m_afslrx1;
        protected float m_afslrx2;
        protected float m_afslrx3;
        protected float m_pwrrx1;
        protected float m_pwrrx2;
        protected float m_pwrrx3;
        protected string m_trafrx;
        protected float m_esint;
        protected float m_tsint;
        protected string m_srvcrx;
        protected string m_statrx;
        protected string m_routnumb;
        protected short m_stnnumb;
        protected short m_hopnumb;
        protected string m_sdate;
        protected string m_notetx;
        protected string m_noterx;
        protected string m_notegnl;
        protected string m_cpoint;
        protected string m_feetx;
        protected string m_feerx;
        protected string m_mdate;
        protected string m_mtime;
        protected string m_userid;


        public DBAccess.eTsType Type
        {
            get { return m_eType; }
        }

        //	***********************************************************************
        //	Render the Insert Command to add this channel record to the database 
        //	***********************************************************************
        protected string InsertCommand(eTsType eType, string sTable)
        {
            StringBuilder sbColumns = new StringBuilder(320);
            StringBuilder sbValues = new StringBuilder(320);
            string tableName;


            if (eType == eTsType.ft)
            {
                tableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_chan";

                sbColumns.Append("cmd,");
                sbValues.Append("'" + m_cmd + "',");

                sbColumns.Append("recstat,");
                sbValues.Append("'" + m_recstat + "',");
            }
            else
            {
                tableName = "main.mt_chan";
            }

            sbColumns.Append("call1,");
            sbValues.Append("'" + m_call1 + "',");

            sbColumns.Append("call2,");
            sbValues.Append("'" + m_call2 + "',");

            sbColumns.Append("bndcde,");
            sbValues.Append("'" + m_bndcde + "',");

            sbColumns.Append("splan,");
            sbValues.Append("'" + m_splan + "',");

            sbColumns.Append("hl,");
            sbValues.Append(m_hl.ToString() + ",");

            sbColumns.Append("vh,");
            sbValues.Append(m_vh.ToString() + ",");

            sbColumns.Append("chid,");
            sbValues.Append("'" + m_chid + "',");

            sbColumns.Append("freqtx,");
            sbValues.Append(m_freqtx.ToString() + ",");

            sbColumns.Append("poltx,");
            sbValues.Append("'" + m_poltx + "',");

            sbColumns.Append("antnumbtx1,");
            sbValues.Append(m_antnumbtx1.ToString() + ",");

            sbColumns.Append("antnumbtx2,");
            sbValues.Append(m_antnumbtx2.ToString() + ",");

            sbColumns.Append("eqpttx,");
            sbValues.Append("'" + m_eqpttx + "',");

            sbColumns.Append("eqptutx,");
            sbValues.Append("'" + m_eqptutx + "',");

            sbColumns.Append("pwrtx,");
            sbValues.Append(m_pwrtx.ToString() + ",");

            sbColumns.Append("atpccde,");
            sbValues.Append(m_atpccde.ToString() + ",");

            sbColumns.Append("afsltx1,");
            sbValues.Append(m_afsltx1.ToString() + ",");

            sbColumns.Append("afsltx2,");
            sbValues.Append(m_afsltx2.ToString() + ",");

            sbColumns.Append("traftx,");
            sbValues.Append("'" + m_traftx + "',");

            sbColumns.Append("srvctx,");
            sbValues.Append("'" + m_srvctx + "',");

            sbColumns.Append("stattx,");
            sbValues.Append("'" + m_stattx + "',");

            sbColumns.Append("freqrx,");
            sbValues.Append(m_freqrx.ToString() + ",");

            sbColumns.Append("polrx,");
            sbValues.Append("'" + m_polrx + "',");

            sbColumns.Append("antnumbrx1,");
            sbValues.Append(m_antnumbrx1.ToString() + ",");

            sbColumns.Append("antnumbrx2,");
            sbValues.Append(m_antnumbrx2.ToString() + ",");

            sbColumns.Append("antnumbrx3,");
            sbValues.Append(m_antnumbrx3.ToString() + ",");

            sbColumns.Append("eqptrx,");
            sbValues.Append("'" + m_eqptrx + "',");

            sbColumns.Append("eqpturx,");
            sbValues.Append("'" + m_eqpturx + "',");

            sbColumns.Append("afslrx1,");
            sbValues.Append(m_afslrx1.ToString() + ",");

            sbColumns.Append("afslrx2,");
            sbValues.Append(m_afslrx2.ToString() + ",");

            sbColumns.Append("afslrx3,");
            sbValues.Append(m_afslrx3.ToString() + ",");

            sbColumns.Append("pwrrx1,");
            sbValues.Append(m_pwrrx1.ToString() + ",");

            sbColumns.Append("pwrrx2,");
            sbValues.Append(m_pwrrx2.ToString() + ",");

            sbColumns.Append("pwrrx3,");
            sbValues.Append(m_pwrrx3.ToString() + ",");

            sbColumns.Append("trafrx,");
            sbValues.Append("'" + m_trafrx + "',");

            sbColumns.Append("esint,");
            sbValues.Append(m_esint.ToString() + ",");

            sbColumns.Append("tsint,");
            sbValues.Append(m_tsint.ToString() + ",");

            sbColumns.Append("srvcrx,");
            sbValues.Append("'" + m_srvcrx + "',");

            sbColumns.Append("statrx,");
            sbValues.Append("'" + m_statrx + "',");

            sbColumns.Append("routnumb,");
            sbValues.Append("'" + m_routnumb + "',");

            sbColumns.Append("stnnumb,");
            sbValues.Append(m_stnnumb.ToString() + ",");

            sbColumns.Append("hopnumb,");
            sbValues.Append(m_hopnumb.ToString() + ",");

            sbColumns.Append("sdate,");
            sbValues.Append("'" + m_sdate + "',");

            sbColumns.Append("notetx,");
            sbValues.Append("'" + m_notetx + "',");

            sbColumns.Append("noterx,");
            sbValues.Append("'" + m_noterx + "',");

            sbColumns.Append("notegnl,");
            sbValues.Append("'" + m_notegnl + "',");

            sbColumns.Append("cpoint,");
            sbValues.Append("'" + m_cpoint + "',");

            sbColumns.Append("feetx,");
            sbValues.Append("'" + m_feetx + "',");

            sbColumns.Append("feerx,");
            sbValues.Append("'" + m_feerx + "',");

            sbColumns.Append("mdate,");
            sbValues.Append("'" + m_mdate + "',");

            sbColumns.Append("mtime");
            sbValues.Append("'" + m_mtime + "'");

            if (eType == eTsType.mt)
            {
                sbColumns.Append(",userid");
                sbValues.Append(",'" + m_userid + "'");
            }

            return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
                sbValues + ")");
        }


        //	***********************************************************************
        //	Constructor for a blank Channel
        //	***********************************************************************

        public TsChannel()
        {
        }


        //	***********************************************************************
        //	Constructor for a channel of a designated type.
        //	***********************************************************************
        public TsChannel(eTsType eTypein)
        {
            m_eType = eTypein;
        }

        //	***********************************************************************
        //	Constructor for a channel of a designated type from a datarow.
        //	***********************************************************************
        public TsChannel(eTsType eType, DataRow oDR)
        {
            m_eType = eType;

            if (eType == eTsType.ft)
            {
                m_cmd = oDR["cmd"].ToString();
                m_recstat = oDR["recstat"].ToString();
            }

            m_call1 = oDR["call1"].ToString();
            m_call2 = oDR["call2"].ToString();
            m_bndcde = oDR["bndcde"].ToString();
            m_splan = oDR["splan"].ToString();
            try { m_hl = Convert.ToInt16(oDR["hl"]); }
            catch { m_hl = 0; }
            try { m_vh = Convert.ToInt16(oDR["vh"]); }
            catch { m_vh = 0; }
            m_chid = oDR["chid"].ToString();
            try { m_freqtx = Convert.ToDouble(oDR["freqtx"]); }
            catch { m_freqtx = 0; }
            m_poltx = oDR["poltx"].ToString();
            try { m_antnumbtx1 = Convert.ToInt16(oDR["antnumbtx1"]); }
            catch { m_antnumbtx1 = 0; }
            try { m_antnumbtx2 = Convert.ToInt16(oDR["antnumbtx2"]); }
            catch { m_antnumbtx2 = 0; }
            m_eqpttx = oDR["eqpttx"].ToString();
            m_eqptutx = oDR["eqptutx"].ToString();
            try { m_pwrtx = Convert.ToSingle(oDR["pwrtx"]); }
            catch { m_pwrtx = 0; }
            try { m_atpccde = Convert.ToSingle(oDR["atpccde"]); }
            catch { m_atpccde = 0; }
            try { m_afsltx1 = Convert.ToSingle(oDR["afsltx1"]); }
            catch { m_afsltx1 = 0; }
            try { m_afsltx2 = Convert.ToSingle(oDR["afsltx2"]); }
            catch { m_afsltx2 = 0; }
            m_traftx = oDR["traftx"].ToString();
            m_srvctx = oDR["srvctx"].ToString();
            m_stattx = oDR["stattx"].ToString();
            try { m_freqrx = Convert.ToDouble(oDR["freqrx"]); }
            catch { m_freqrx = 0; }
            m_polrx = oDR["polrx"].ToString();
            try { m_antnumbrx1 = Convert.ToInt16(oDR["antnumbrx1"]); }
            catch { m_antnumbrx1 = 0; }
            try { m_antnumbrx2 = Convert.ToInt16(oDR["antnumbrx2"]); }
            catch { m_antnumbrx2 = 0; }
            try { m_antnumbrx3 = Convert.ToInt16(oDR["antnumbrx3"]); }
            catch { m_antnumbrx3 = 0; }
            m_eqptrx = oDR["eqptrx"].ToString();
            m_eqpturx = oDR["eqpturx"].ToString();
            try { m_afslrx1 = Convert.ToSingle(oDR["afslrx1"]); }
            catch { m_afslrx1 = 0; }
            try { m_afslrx2 = Convert.ToSingle(oDR["afslrx2"]); }
            catch { m_afslrx2 = 0; }
            try { m_afslrx3 = Convert.ToSingle(oDR["afslrx3"]); }
            catch { m_afslrx3 = 0; }
            try { m_pwrrx1 = Convert.ToSingle(oDR["pwrrx1"]); }
            catch { m_pwrrx1 = 0; }
            try { m_pwrrx2 = Convert.ToSingle(oDR["pwrrx2"]); }
            catch { m_pwrrx2 = 0; }
            try { m_pwrrx3 = Convert.ToSingle(oDR["pwrrx3"]); }
            catch { m_pwrrx3 = 0; }
            m_trafrx = oDR["trafrx"].ToString();
            try { m_esint = Convert.ToSingle(oDR["esint"]); }
            catch { m_esint = 0; }
            try { m_tsint = Convert.ToSingle(oDR["tsint"]); }
            catch { m_tsint = 0; }
            m_srvcrx = oDR["srvcrx"].ToString();
            m_statrx = oDR["statrx"].ToString();
            m_routnumb = oDR["routnumb"].ToString();
            try { m_stnnumb = Convert.ToInt16(oDR["stnnumb"]); }
            catch { m_stnnumb = 0; }
            try { m_hopnumb = Convert.ToInt16(oDR["hopnumb"]); }
            catch { m_hopnumb = 0; }
            m_sdate = oDR["sdate"].ToString();
            try
            {
                m_sdate = DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
            }
            catch { }
            m_notetx = oDR["notetx"].ToString();
            m_noterx = oDR["noterx"].ToString();
            m_notegnl = oDR["notegnl"].ToString();
            m_cpoint = oDR["cpoint"].ToString();
            m_feetx = oDR["feetx"].ToString();
            m_feerx = oDR["feerx"].ToString();
            m_mdate = oDR["mdate"].ToString();
            try
            {
                m_mdate = DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
            }
            catch { }
            m_mtime = oDR["mtime"].ToString();

            if (eType == eTsType.mt)
            {
                m_userid = oDR["userid"].ToString();
            }
        }


        //	*********************************************************************
        //	Copy a channel record into a newly created channel.
        //	*********************************************************************
        protected void TsCopChn(eTsType eType, TsChannel tChn)
        {
            if (eType == eTsType.ft)
            {
                m_cmd = tChn.m_cmd;
                m_recstat = tChn.m_recstat;
            }

            m_call1 = tChn.m_call1;
            m_call2 = tChn.m_call2;
            m_bndcde = tChn.m_bndcde;
            m_splan = tChn.m_splan;
            m_hl = tChn.m_hl;
            m_vh = tChn.m_vh;
            m_chid = tChn.m_chid;
            m_freqtx = tChn.m_freqtx;
            m_poltx = tChn.m_poltx;
            m_antnumbtx1 = tChn.m_antnumbtx1;
            m_antnumbtx2 = tChn.m_antnumbtx2;
            m_eqpttx = tChn.m_eqpttx;
            m_eqptutx = tChn.m_eqptutx;
            m_pwrtx = tChn.m_pwrtx;
            m_atpccde = tChn.m_atpccde;
            m_afsltx1 = tChn.m_afsltx1;
            m_afsltx2 = tChn.m_afsltx2;
            m_traftx = tChn.m_traftx;
            m_srvctx = tChn.m_srvctx;
            m_stattx = tChn.m_stattx;
            m_freqrx = tChn.m_freqrx;
            m_polrx = tChn.m_polrx;
            m_antnumbrx1 = tChn.m_antnumbrx1;
            m_antnumbrx2 = tChn.m_antnumbrx2;
            m_antnumbrx3 = tChn.m_antnumbrx3;
            m_eqptrx = tChn.m_eqptrx;
            m_eqpturx = tChn.m_eqpturx;
            m_afslrx1 = tChn.m_afslrx1;
            m_afslrx2 = tChn.m_afslrx2;
            m_afslrx3 = tChn.m_afslrx3;
            m_pwrrx1 = tChn.m_pwrrx1;
            m_pwrrx2 = tChn.m_pwrrx2;
            m_pwrrx3 = tChn.m_pwrrx3;
            m_trafrx = tChn.m_trafrx;
            m_esint = tChn.m_esint;
            m_tsint = tChn.m_tsint;
            m_srvcrx = tChn.m_srvcrx;
            m_statrx = tChn.m_statrx;
            m_routnumb = tChn.m_routnumb;
            m_stnnumb = tChn.m_stnnumb;
            m_hopnumb = tChn.m_hopnumb;
            m_sdate = tChn.m_sdate;
            m_notetx = tChn.m_notetx;
            m_noterx = tChn.m_noterx;
            m_notegnl = tChn.m_notegnl;
            m_cpoint = tChn.m_cpoint;
            m_feetx = tChn.m_feetx;
            m_feerx = tChn.m_feerx;
            m_mdate = tChn.m_mdate;
            m_mtime = tChn.m_mtime;

            if (eType == eTsType.mt)
            {
                m_userid = tChn.m_userid;
            }

            return;
        }

        public XmlElement TsChannelXml(eTsType eType,
                                         TsFile oFile)
        {
            // XmlElement tChn = oFile.CreateElement("channel");
            XmlElement tChannelEl = oFile.CreateElement("channel");

            if (eType == eTsType.ft)
            {
                tChannelEl.AppendChild(oFile.XmlEl("cmd", m_cmd));
                tChannelEl.AppendChild(oFile.XmlEl("recstat", m_recstat));
            }
            tChannelEl.AppendChild(oFile.XmlEl("call1", m_call1));
            tChannelEl.AppendChild(oFile.XmlEl("call2", m_call2));
            tChannelEl.AppendChild(oFile.XmlEl("bndcde", m_bndcde));
            tChannelEl.AppendChild(oFile.XmlEl("splan", m_splan));
            tChannelEl.AppendChild(oFile.XmlEl("hl", m_hl.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("vh", m_vh.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("chid", m_chid));
            tChannelEl.AppendChild(oFile.XmlEl("freqtx", m_freqtx.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("poltx", m_poltx));
            tChannelEl.AppendChild(oFile.XmlEl("antnumbtx1", m_antnumbtx1.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("antnumbtx2", m_antnumbtx2.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("eqpttx", m_eqpttx));
            tChannelEl.AppendChild(oFile.XmlEl("eqptutx", m_eqptutx));
            tChannelEl.AppendChild(oFile.XmlEl("pwrtx", m_pwrtx.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("atpccde", m_atpccde.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("afsltx1", m_afsltx1.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("afsltx2", m_afsltx2.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("traftx", m_traftx));
            tChannelEl.AppendChild(oFile.XmlEl("srvctx", m_srvctx));
            tChannelEl.AppendChild(oFile.XmlEl("stattx", m_stattx));
            tChannelEl.AppendChild(oFile.XmlEl("freqrx", m_freqrx.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("polrx", m_polrx));
            tChannelEl.AppendChild(oFile.XmlEl("antnumbrx1", m_antnumbrx1.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("antnumbrx2", m_antnumbrx2.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("antnumbrx3", m_antnumbrx3.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("eqptrx", m_eqptrx));
            tChannelEl.AppendChild(oFile.XmlEl("eqpturx", m_eqpturx));
            tChannelEl.AppendChild(oFile.XmlEl("afslrx1", m_afslrx1.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("afslrx2", m_afslrx2.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("afslrx3", m_afslrx3.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("pwrrx1", m_pwrrx1.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("pwrrx2", m_pwrrx2.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("pwrrx3", m_pwrrx3.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("trafrx", m_trafrx));
            tChannelEl.AppendChild(oFile.XmlEl("esint", m_esint.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("tsint", m_tsint.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("srvcrx", m_srvcrx.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("statrx", m_statrx));
            tChannelEl.AppendChild(oFile.XmlEl("routnumb", m_routnumb));
            tChannelEl.AppendChild(oFile.XmlEl("stnnumb", m_stnnumb.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("hopnumb", m_hopnumb.ToString()));
            tChannelEl.AppendChild(oFile.XmlEl("sdate", m_sdate));
            tChannelEl.AppendChild(oFile.XmlEl("notetx", m_notetx));
            tChannelEl.AppendChild(oFile.XmlEl("noterx", m_noterx));
            tChannelEl.AppendChild(oFile.XmlEl("notegnl", m_notegnl));
            tChannelEl.AppendChild(oFile.XmlEl("cpoint", m_cpoint));
            tChannelEl.AppendChild(oFile.XmlEl("feetx", m_feetx));
            tChannelEl.AppendChild(oFile.XmlEl("feerx", m_feerx));
            tChannelEl.AppendChild(oFile.XmlEl("mdate", m_mdate));
            tChannelEl.AppendChild(oFile.XmlEl("mtime", m_mtime));

            if (eType == eTsType.mt)
            {
                tChannelEl.AppendChild(oFile.XmlEl("userid", m_userid));
            }

            return tChannelEl;
        }
    }


    //	**************************************************************************
    /// <summary>
    /// General Class to Access both MDB and PDF TS Radios.  The site information is
    /// stored in the radio class itself.
    /// </summary>
    public class TsRadio
    {
        protected eTsType m_etype;
        protected int m_nDepth;

        protected string m_cmd;
        protected string m_recstat;
        protected string m_call1;
        protected string m_name;
        protected int m_latit;
        protected string m_strlatit;
        protected string m_strlatits;
        protected int m_longit;
        protected string m_strlongit;
        protected string m_strlongits;
        protected double m_grnd;
        protected string m_mdate;
        protected string m_mtime;
        protected string m_userid;
        protected string m_prov;
        protected string m_oper;
        protected string m_stats;
        protected string m_loc;
        protected string m_icaccount;
        protected string m_reg;
        protected string m_snumb;
        protected int m_notwr;
        protected string m_nots;
        protected int m_spoint;
        protected string m_sdate;
        protected string m_oprtyp;
        protected uint m_bandwd1;
        protected uint m_bandwd2;
        protected uint m_bandwd3;
        protected uint m_bandwd4;
        protected uint m_bandwd5;
        protected uint m_bandwd6;
        protected uint m_bandwd7;
        protected uint m_bandwd8;

        //	Optional elements
        protected string m_nameop;

        //	Arrays and counters
        protected int m_numants = 0;
        protected ArrayList m_Antennas = new ArrayList();
        protected int m_numchan = 0;
        protected ArrayList m_Channels = new ArrayList();

        public int numants
        {
            get { return m_numants; }
        }

        public ArrayList Antennas
        {
            get { return m_Antennas; }
        }

        public int numchan
        {
            get { return m_numchan; }
        }

        public ArrayList Channels
        {
            get { return m_Channels; }
        }

        public DBAccess.eTsType Type
        {
            get { return m_etype; }
        }

        //	These are the switches indicating if the subsidiary tables are to be loaded.
        protected int m_numtown = 0;
        protected TowerNote[] m_aTown;
        public int numtown { get { return m_numtown; } }
        public TowerNote[] Towns { get { return m_aTown; } }

        /// <summary>
        /// The most primitive constructor.
        /// </summary>
        public TsRadio() { }

        public TsRadio(DBAccess.eTsType etypein,
                       string sCallsign,
                       string sTable,
                       int nDepth)
        {
            // 
            // Retrieve a full radio given its callsign and depth.
            //	Start by reading in the site information, and then call the 
            //	constructors for the antennas and channels if necessary.
            //
            string sTableName;
            if (etypein == eTsType.mt)
            {
                sTableName = "main.mt_site";
            }
            else
            {
                sTableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_site";
            }
            dbconnect oConnection = new dbconnect();

            const string sFieldNames_mt = "s.call1, s.name, " +
                                                                 "s.latit, s.strlatit, s.strlatits, " +
                                                                 "s.longit, s.strlongit, s.strlongits, " +
                                                                 "s.grnd, s.mdate, s.mtime, s.userid, s.prov, " +
                                                                 "s.oper, s.stats, s.loc, s.icaccount, s.reg, s.snumb, " +
                                                                 "s.notwr, s.nots, s.spoint, " +
                                                                 "s.sdate, s.oprtyp, " +
                                                                 "s.bandwd1, s.bandwd2, s.bandwd3, s.bandwd4, " +
                                                                 "s.bandwd5, s.bandwd6, s.bandwd7, s.bandwd8, " +
                                                                 "s.userid, " +
                                                                 "o.nameop ";
            const string sFieldNames_ft = "s.cmd, s.recstat, s.call1, s.name, " +
                                                                 "s.latit, s.longit, " +
                                                                 "s.grnd, s.mdate, s.mtime, s.prov, " +
                                                                 "s.oper, s.stats, s.loc, s.icaccount, s.reg, s.snumb, " +
                                                                 "s.notwr, s.nots, s.spoint, " +
                                                                 "s.sdate, s.oprtyp, " +
                                                                 "s.bandwd1, s.bandwd2, s.bandwd3, s.bandwd4, " +
                                                                 "s.bandwd5, s.bandwd6, s.bandwd7, s.bandwd8, " +
                                                                 "o.nameop ";
            string sFieldNames;
            switch ((int)etypein)
            {
                case (int)eTsType.mt:
                    sFieldNames = sFieldNames_mt;
                    break;
                case (int)eTsType.ft:
                    sFieldNames = sFieldNames_ft;
                    break;
                default:
                    throw new Exception("Invalid TS type entered, must be mt or ft.");
            }

            string mtCommand = "SELECT " + sFieldNames +
                           " FROM " + sTableName + " s left outer join main.sd_oper o " +
                                      " on (s.oper = o.oper) " +
                                  " WHERE call1='" + sCallsign.ToUpper() + "'";
            m_etype = etypein;
            m_cmd = "-";
            m_recstat = "-";
            m_strlatit = "";
            m_strlatits = "";
            m_strlongit = "";
            m_strlongits = "";
            m_userid = "";

            m_numants = 0;
            m_numchan = 0;
            m_nDepth = 0;	//	No info so far

            OdbcDataAdapter oAdapter = new OdbcDataAdapter(mtCommand,
                                                             oConnection.Connection);
            DataSet oDS = new DataSet();
            oAdapter.Fill(oDS);

            // We assume that we have retrieved only one record (or zero)
            DataTable oDT = oDS.Tables[0];
            if (oDT.Rows.Count > 0)
            {
                //	Got something.  We are just interested in the first row.
                DataRow oDR = oDT.Rows[0];

                if (m_etype == eTsType.ft)
                {
                    m_cmd = oDR["cmd"].ToString();
                    m_recstat = oDR["recstat"].ToString();
                }
                m_call1 = oDR["call1"].ToString();
                m_name = oDR["name"].ToString();
                try { m_latit = Convert.ToInt32(oDR["latit"]); }
                catch { m_latit = 0; }
                try { m_longit = Convert.ToInt32(oDR["longit"]); }
                catch { m_longit = 0; }
                if (m_etype == eTsType.mt)
                {
                    m_strlatit = oDR["strlatit"].ToString();
                    m_strlatits = oDR["strlatits"].ToString();
                    m_strlongit = oDR["strlongit"].ToString();
                    m_strlongits = oDR["strlongits"].ToString();
                    m_userid = oDR["userid"].ToString();
                }
                try { m_grnd = Convert.ToUInt32(oDR["grnd"]); }
                catch { m_grnd = 0; }
                m_mdate = oDR["mdate"].ToString();
                try
                {
                    m_mdate = DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_mtime = oDR["mtime"].ToString();
                m_prov = oDR["prov"].ToString();
                m_oper = oDR["oper"].ToString();
                m_stats = oDR["stats"].ToString();
                m_loc = oDR["loc"].ToString();
                m_icaccount = oDR["icaccount"].ToString();
                m_reg = oDR["reg"].ToString();
                m_snumb = oDR["snumb"].ToString();
                try { m_notwr = Convert.ToInt32(oDR["notwr"]); }
                catch { m_notwr = 0; }
                m_nots = oDR["nots"].ToString();
                try { m_spoint = Convert.ToInt32(oDR["spoint"]); }
                catch { m_spoint = 0; }
                m_sdate = oDR["sdate"].ToString();
                try
                {
                    m_sdate = DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_oprtyp = oDR["oprtyp"].ToString();
                try { m_bandwd1 = Convert.ToUInt32(oDR["bandwd1"]); }
                catch { m_bandwd1 = 0; }
                try { m_bandwd2 = Convert.ToUInt32(oDR["bandwd2"]); }
                catch { m_bandwd2 = 0; }
                try { m_bandwd3 = Convert.ToUInt32(oDR["bandwd3"]); }
                catch { m_bandwd3 = 0; }
                try { m_bandwd4 = Convert.ToUInt32(oDR["bandwd4"]); }
                catch { m_bandwd4 = 0; }
                try { m_bandwd5 = Convert.ToUInt32(oDR["bandwd5"]); }
                catch { m_bandwd5 = 0; }
                try { m_bandwd6 = Convert.ToUInt32(oDR["bandwd6"]); }
                catch { m_bandwd6 = 0; }
                try { m_bandwd7 = Convert.ToUInt32(oDR["bandwd7"]); }
                catch { m_bandwd7 = 0; }
                try { m_bandwd8 = Convert.ToUInt32(oDR["bandwd8"]); }
                catch { m_bandwd8 = 0; }

                //	The operator name may or may not come from the operator table 
                try { m_nameop = oDR["nameop"].ToString(); }
                catch { m_nameop = ""; }

                m_nDepth = 1;		// We at least got the site info.

                //	Now get the Antennas, if the depth requests it
                if (nDepth > 1)
                {
                    string cErrString = "TsRadio-site";
                    //	The user is asking for at least the antennas as well
                    string sAntTable;
                    try
                    {
                        if (etypein == eTsType.mt)
                        {
                            sAntTable = "main.mt_ante";
                        }
                        else
                        {
                            sAntTable = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_ante";
                        }
                        string antCommand = "SELECT a.*,s.name as name2,s.oper as oper2 " +
                                                                    "from " + sAntTable + " a left outer join " +
                                                      sTableName + " s on (a.call2 = s.call1) " +
                                                                " where call1='" + sCallsign.ToUpper() +
                                                        "' order by call2, bndcde, anum";

                        OdbcDataAdapter oAntAdapter = new OdbcDataAdapter(antCommand,
                                                                                                            oConnection.Connection);
                        DataSet oADS = new DataSet();
                        oAntAdapter.Fill(oADS);

                        DataTable oADT = oADS.Tables[0];  // One table retrieved
                        cErrString += ",Count=" + oADT.Rows.Count.ToString();
                        if (oADT.Rows.Count > 0)
                        {
                            foreach (DataRow oADR in oADT.Rows)
                            {
                                cErrString = "tsa: ";
                                if (etypein == eTsType.mt)
                                {
                                    cErrString += "mt";
                                    m_Antennas.Add(new mtAntenna(oADR));
                                }
                                else
                                {
                                    //	Handle the ft here
                                    cErrString += "ft";
                                    m_Antennas.Add(new ftAntenna(oADR));
                                }
                                m_numants++;
                            }
                        }
                        m_nDepth = 2;
                    }
                    catch (Exception e1)
                    {
                        Exception ex = new Exception(cErrString + "\n<br>" + e1.Message);
                        throw ex;
                    }

                    // Now go for the channels if they are requested.
                    if (nDepth > 2)
                    {
                        if (etypein == eTsType.mt)
                        {
                            sTableName = "main.mt_chan";
                        }
                        else
                        {
                            sTableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable + "_chan";
                        }
                        string chnCommand = "SELECT * " +
                                                                    "from " + sTableName +
                                                                " where call1='" + sCallsign.ToUpper() +
                                                        "' order by call2, bndcde, chid";
                        try
                        {
                            OdbcDataAdapter oChnAdapter = new OdbcDataAdapter(chnCommand,
                                                                              oConnection.Connection);
                            DataSet oCDS = new DataSet();
                            oChnAdapter.Fill(oCDS);

                            DataTable oCDT = oCDS.Tables[0];  // One table retrieved
                            if (oCDT.Rows.Count > 0)
                            {
                                foreach (DataRow oCDR in oCDT.Rows)
                                {
                                    if (etypein == eTsType.mt)
                                    {
                                        m_Channels.Add(new mtChannel(oCDR));
                                    }
                                    else
                                    {
                                        m_Channels.Add(new ftChannel(oCDR));
                                    }
                                    m_numchan++;
                                }
                            }
                            m_nDepth = 3;
                        }
                        catch (Exception ex)
                        {
                            Exception ex1 = new Exception("TsRadio Channels<br />\n" +
                                                            ex.Message + "(" + ex.Source +
                                                            ")");
                            throw ex1;
                        }
                    }
                }
            }
            else
            {
                //	Call sign not found.
                throw new System.Exception("100 - Call sign (" + sCallsign +
                                             ") not found in " + sTable);
            }

            oConnection.dbdisconnect();
        }

        /// <summary>
        /// This constructor will create a TS radio from the antenna keys, and then 
        ///	add on any subsidiary information that is requested by the sAdditional 
        ///	parameter.
        /// </summary>
        /// <param name="etypein">TS or ES</param>
        /// <param name="sCallsign">The station call sign</param>
        /// <param name="aAntennas">an array of antenna keys(q.v)</param>
        /// <param name="sTable">The name of the table, blank for the mdb</param>
        /// <param name="sAdditional">This is a comma separated list of names of
        ///		subsidiary information that is requested.  This information is 
        ///		found in the mdb (and only the mdb) and added to the Radio.
        ///	</param>
        public TsRadio(DBAccess.eTsType etypein,
                       string sCallsign,
                       TsAntennaKey[] aAntennas,
                       string sTable,
                       string sAdditional) :
            this(etypein, sCallsign, aAntennas, sTable)
        {
            //	We start by creating the normal radio, and checking for the requirement
            //	for other subsidiary information
            if (sAdditional.Length > 0)
            {
                this.AddSubsidData(sAdditional);
            }
        }

        /// <summary>
        /// This routine will return a partial radio.  The section returned depends on
        /// the arguments, but the site will be returned, any links that are specified,
        /// or all if the specification is blank, and all channels associated with the
        /// links.
        /// </summary>
        /// <param name="etypein">Type is either ft or mt</param>
        /// <param name="sCallsign">Call sign of the site.</param>
        /// <param name="aAntennas">Array, possibly empty, of antenna key elements</param>
        /// <param name="sTable">The base name of the table if ft</param>
        public TsRadio(DBAccess.eTsType etypein,
                                        string sCallsign,
                                        TsAntennaKey[] aAntennas,
                                        string sTable)
        {
            // 
            //  Retrieve a partial radio given an array of antenna keys.
            //	Start by reading in the site information, and then call the 
            //	constructors for the antennas and channels if necessary.
            //
            string sTableName;
            if (etypein == eTsType.mt)
            {
                sTableName = "main.mt_site";
            }
            else
            {
                sTableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_site";
            }
            dbconnect oConnection = new dbconnect();

            const string sFieldNames_mt = "s.call1, s.name, " +
                            "s.latit, s.strlatit, s.strlatits, " +
                            "s.longit, s.strlongit, s.strlongits, " +
                            "s.grnd, s.mdate, s.mtime, s.userid, s.prov, " +
                            "s.oper, s.stats, s.loc, s.icaccount, s.reg, s.snumb, " +
                            "s.notwr, s.nots, s.spoint, " +
                            "s.sdate, s.oprtyp, " +
                            "s.bandwd1, s.bandwd2, s.bandwd3, s.bandwd4, " +
                            "s.bandwd5, s.bandwd6, s.bandwd7, s.bandwd8, " +
                            "s.userid, o.nameop ";
            const string sFieldNames_ft = "s.cmd, s.recstat, s.call1, s.name, " +
                            "s.latit, s.longit, " +
                            "s.grnd, s.mdate, s.mtime, s.prov, " +
                            "s.oper, s.stats, s.loc, s.icaccount, s.reg, s.snumb, " +
                            "s.notwr, s.nots, s.spoint, " +
                            "s.sdate, s.oprtyp, " +
                            "s.bandwd1, s.bandwd2, s.bandwd3, s.bandwd4, " +
                            "s.bandwd5, s.bandwd6, s.bandwd7, s.bandwd8, o.nameop ";
            string sFieldNames;
            switch ((int)etypein)
            {
                case (int)eTsType.mt:
                    sFieldNames = sFieldNames_mt;
                    break;
                case (int)eTsType.ft:
                    sFieldNames = sFieldNames_ft;
                    break;
                default:
                    throw new Exception("Invalid TS type entered, must be mt or ft.");
            }

            string mtCommand = "SELECT " + sFieldNames +
                                    "FROM " + sTableName + " s left outer join main.sd_oper o " +
                                               "on (s.oper = o.oper) " +
                                  " WHERE call1='" + sCallsign.ToUpper() + "'";
            m_etype = etypein;
            m_cmd = "-";
            m_recstat = "-";
            m_strlatit = "";
            m_strlatits = "";
            m_strlongit = "";
            m_strlongits = "";
            m_userid = "";

            m_numants = 0;
            m_numchan = 0;
            m_nDepth = 0;	//	No info so far

            OdbcDataAdapter oAdapter = new OdbcDataAdapter(mtCommand,
                                                                                                         oConnection.Connection);
            DataSet oDS = new DataSet();
            oAdapter.Fill(oDS);

            // We assume that we have retrieved only one record (or zero)
            DataTable oDT = oDS.Tables[0];
            if (oDT.Rows.Count > 0)
            {
                //	Got something.  We are just interested in the first row.
                DataRow oDR = oDT.Rows[0];

                if (m_etype == eTsType.ft)
                {
                    m_cmd = oDR["cmd"].ToString();
                    m_recstat = oDR["recstat"].ToString();
                }
                m_call1 = oDR["call1"].ToString();
                m_name = oDR["name"].ToString();
                try { m_latit = Convert.ToInt32(oDR["latit"]); }
                catch { m_latit = 0; }
                try { m_longit = Convert.ToInt32(oDR["longit"]); }
                catch { m_longit = 0; }
                if (m_etype == eTsType.mt)
                {
                    m_strlatit = oDR["strlatit"].ToString();
                    m_strlatits = oDR["strlatits"].ToString();
                    m_strlongit = oDR["strlongit"].ToString();
                    m_strlongits = oDR["strlongits"].ToString();
                    m_userid = oDR["userid"].ToString();
                }
                try { m_grnd = Convert.ToDouble(oDR["grnd"]); }
                catch { m_grnd = 1; }
                m_mdate = oDR["mdate"].ToString();
                try
                {
                    m_mdate = DateTime.Parse(m_mdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_mtime = oDR["mtime"].ToString();
                m_prov = oDR["prov"].ToString();
                m_oper = oDR["oper"].ToString();
                m_stats = oDR["stats"].ToString();
                m_loc = oDR["loc"].ToString();
                m_icaccount = oDR["icaccount"].ToString();
                m_reg = oDR["reg"].ToString();
                m_snumb = oDR["snumb"].ToString();
                try { m_notwr = Convert.ToInt32(oDR["notwr"]); }
                catch { m_notwr = 0; }
                m_nots = oDR["nots"].ToString();
                try { m_spoint = Convert.ToInt32(oDR["spoint"]); }
                catch { m_spoint = 0; }
                m_sdate = oDR["sdate"].ToString();
                try
                {
                    m_sdate = DateTime.Parse(m_sdate).ToString("dd-MMM-yyyy");
                }
                catch { }
                m_oprtyp = oDR["oprtyp"].ToString();
                try { m_bandwd1 = Convert.ToUInt32(oDR["bandwd1"]); }
                catch { m_bandwd1 = 0; }
                try { m_bandwd2 = Convert.ToUInt32(oDR["bandwd2"]); }
                catch { m_bandwd2 = 0; }
                try { m_bandwd3 = Convert.ToUInt32(oDR["bandwd3"]); }
                catch { m_bandwd3 = 0; }
                try { m_bandwd4 = Convert.ToUInt32(oDR["bandwd4"]); }
                catch { m_bandwd4 = 0; }
                try { m_bandwd5 = Convert.ToUInt32(oDR["bandwd5"]); }
                catch { m_bandwd5 = 0; }
                try { m_bandwd6 = Convert.ToUInt32(oDR["bandwd6"]); }
                catch { m_bandwd6 = 0; }
                try { m_bandwd7 = Convert.ToUInt32(oDR["bandwd7"]); }
                catch { m_bandwd7 = 0; }
                try { m_bandwd8 = Convert.ToUInt32(oDR["bandwd8"]); }
                catch { m_bandwd8 = 0; }

                //	Get the operator name
                try { m_nameop = oDR["nameop"].ToString(); }
                catch { m_nameop = ""; }

                m_nDepth = 1;		// We at least got the site info.

                //	Now get the Antennas, if any requested.
                foreach (TsAntennaKey tAK in aAntennas)
                {
                    string cErrString = "";
                    //	The user is asking for at least the antennas as well
                    try
                    {
                        if (etypein == eTsType.mt)
                        {
                            sTableName = "main.mt_ante";
                        }
                        else
                        {
                            sTableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_ante";
                        }
                        string antCommand = "SELECT a.*,s.name as name2,s.oper as oper2 " +
                            "from " + sTableName + " a left outer join " +
                                      TsFile.sitetable(etypein, sTableName) +
                                      " s on (a.call2 = s.call1) " +
                            " where a.call1='" + sCallsign.ToUpper() +
                            "' and a.call2='" + tAK.call2 +
                            "' and a.bndcde='" + tAK.bndcde +
                            "' order by a.call2, a.bndcde, a.anum";

                        OdbcDataAdapter oAntAdapter = new OdbcDataAdapter(antCommand,
                            oConnection.Connection);
                        DataSet oADS = new DataSet();
                        oAntAdapter.Fill(oADS);

                        DataTable oADT = oADS.Tables[0];  // One table retrieved
                        if (oADT.Rows.Count > 0)
                        {
                            foreach (DataRow oADR in oADT.Rows)
                            {
                                TsAntenna tsa;
                                if (etypein == eTsType.mt)
                                {
                                    tsa = new mtAntenna(oADR);
                                }
                                else
                                {
                                    //	Handle the ft here
                                    tsa = new ftAntenna(oADR);
                                }
                                m_Antennas.Add(tsa);
                                m_numants++;
                            }
                            m_nDepth = 2;
                        }
                        else
                        {
                            //	There were no rows returned.  Either there were no antennas
                            //	requested, or there were none that met the criteria.
                            //	Depth will remain at 1. 
                        }
                    }
                    catch (Exception e1)
                    {
                        Exception ex = new Exception(cErrString + "\n<br>" + e1.Message);
                        throw ex;
                    }
                }

                // Now go for the channels if they are requested.
                foreach (TsAntennaKey tAK in aAntennas)
                {
                    if (etypein == eTsType.mt)
                    {
                        sTableName = "main.mt_chan";
                    }
                    else
                    {
                        sTableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable + "_chan";
                    }
                    string chnCommand = "SELECT * " +
                        "from " + sTableName +
                        " where call1='" + sCallsign.ToUpper() +
                        "' and call2='" + tAK.call2 +
                        "' and bndcde='" + tAK.bndcde +
                        "' order by call2, bndcde, chid";

                    OdbcDataAdapter oChnAdapter = new OdbcDataAdapter(chnCommand,
                        oConnection.Connection);
                    DataSet oCDS = new DataSet();
                    oChnAdapter.Fill(oCDS);

                    DataTable oCDT = oCDS.Tables[0];  // One table retrieved
                    if (oCDT.Rows.Count > 0)
                    {
                        foreach (DataRow oCDR in oCDT.Rows)
                        {
                            if (etypein == eTsType.mt)
                            {
                                m_Channels.Add(new mtChannel(oCDR));
                            }
                            else
                            {
                                m_Channels.Add(new ftChannel(oCDR));
                            }
                            m_numchan++;
                        }
                        m_nDepth = 3;
                    }
                    else
                    {
                        //	No channels met the requirements
                    }
                }
            }
            else
            {
                //	Call sign not found.
                throw new System.Exception("100 - Call sign (" + sCallsign +
                    ") not found in " + sTable);
            }

            oConnection.dbdisconnect();
        }


        /// <summary>
        /// Routine to determine if there a given tower note is present in a list of tower notes.
        /// </summary>
        /// <param name="al">An arraylist of towernotes</param>
        /// <param name="cCall">The call sign of the site we are seeking</param>
        /// <param name="nTowerNo">The tower number of the tower in the site</param>
        /// <returns>True if present, false otherwise</returns>
        private bool townPresent(ArrayList al, string cCall, short nTowerNo)
        {
            foreach (TowerNote tn in al)
            {
                if (tn.call1.Trim() == cCall.Trim() && tn.atwrno == nTowerNo)
                {
                    return true;
                }
            }
            return false;
        }


        protected void AddSubsidData(string inList)
        {
            string[] aList = inList.Split(',');

            if (aList.Length > 0)
            {
                foreach (string cListEl in aList)
                {
                    if (cListEl.Trim().ToLower() == "town")
                    {
                        //	This is the tower number processing.
                        m_numtown = 0;
                        ArrayList alTown = new ArrayList();
                        //	Get the tower notes as requested.
                        foreach (TsAntenna tAnt in Antennas)
                        {
                            if (tAnt.m_atwrno > 0)
                            {
                                //	there is a tower number for this antenna.  Check for the notes
                                if (!townPresent(alTown, tAnt.m_call1, tAnt.m_atwrno))
                                {
                                    //	We don't already have it.  Try to get it.
                                    TowerNote tn;
                                    try
                                    {
                                        //	If the note is not present, the constructor will throw an exception.
                                        tn = new TowerNote("", tAnt.m_call1, tAnt.m_atwrno);
                                        alTown.Add(tn);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }

                        //	Convert the arraylist to an array and store in the tower number array.
                        m_numtown = alTown.Count;
                        m_aTown = (TowerNote[])alTown.ToArray(typeof(TowerNote));
                    }
                }
            }
        }


        //	***********************************************************************
        //	Set the depth value in the site.  If this is greater than the current
        //	depth it is set, if it is less than or equal it is ignored.
        //	***********************************************************************
        protected void setdepth(int nDepth)
        {
            if (nDepth > m_nDepth)
            {
                m_nDepth = nDepth;
            }
            return;
        }


        //	***********************************************************************
        //	Render the Insert Command to add this site record to the database 
        //	***********************************************************************
        protected string InsertCommand(eTsType eType, string sTable)
        {
            StringBuilder sbColumns = new StringBuilder(320);
            StringBuilder sbValues = new StringBuilder(320);
            string tableName;


            if (eType == eTsType.ft)
            {
                tableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_site";

                sbColumns.Append("cmd,");
                sbValues.Append("'" + m_cmd + "',");

                sbColumns.Append("recstat,");
                sbValues.Append("'" + m_recstat + "',");
            }
            else
            {
                tableName = "main.mt_site";
            }

            sbColumns.Append("call1,");
            sbValues.Append("'" + m_call1 + "',");

            sbColumns.Append("name,");
            sbValues.Append("'" + m_name + "',");

            sbColumns.Append("latit,");
            sbValues.Append(m_latit.ToString() + ",");

            sbColumns.Append("longit,");
            sbValues.Append(m_longit.ToString() + ",");

            if (eType == eTsType.mt)
            {
                sbColumns.Append("strlatit,");
                sbValues.Append("'" + m_strlatit + "',");

                sbColumns.Append("strlatits,");
                sbValues.Append("'" + m_strlatits + "',");

                sbColumns.Append("strlongit,");
                sbValues.Append("'" + m_strlongit + "',");

                sbColumns.Append("strlongits,");
                sbValues.Append("'" + m_strlongits + "',");

                sbColumns.Append("userid,");
                sbValues.Append("'" + m_userid + "',");
            }

            sbColumns.Append("grnd,");
            sbValues.Append(m_grnd.ToString() + ",");

            sbColumns.Append("mdate,");
            sbValues.Append("'" + m_mdate + "',");

            sbColumns.Append("mtime,");
            sbValues.Append("'" + m_mtime + "',");

            sbColumns.Append("prov,");
            sbValues.Append("'" + m_prov + "',");

            sbColumns.Append("oper,");
            sbValues.Append("'" + m_oper + "',");

            sbColumns.Append("stats,");
            sbValues.Append("'" + m_stats + "',");

            sbColumns.Append("loc,");
            sbValues.Append("'" + m_loc + "',");

            sbColumns.Append("icaccount,");
            sbValues.Append("'" + m_icaccount + "',");

            sbColumns.Append("reg,");
            sbValues.Append("'" + m_reg + "',");

            sbColumns.Append("snumb,");
            sbValues.Append("'" + m_snumb + "',");

            sbColumns.Append("notwr,");
            sbValues.Append(m_notwr.ToString() + ",");

            sbColumns.Append("nots,");
            sbValues.Append("'" + m_nots + "',");

            sbColumns.Append("spoint,");
            sbValues.Append(m_spoint.ToString() + ",");

            sbColumns.Append("sdate,");
            sbValues.Append("'" + m_sdate + "',");

            sbColumns.Append("oprtyp,");
            sbValues.Append("'" + m_oprtyp + "',");

            sbColumns.Append("bandwd1,");
            sbValues.Append(m_bandwd1.ToString() + ",");

            sbColumns.Append("bandwd2,");
            sbValues.Append(m_bandwd2.ToString() + ",");

            sbColumns.Append("bandwd3,");
            sbValues.Append(m_bandwd3.ToString() + ",");

            sbColumns.Append("bandwd4,");
            sbValues.Append(m_bandwd4.ToString() + ",");

            sbColumns.Append("bandwd5,");
            sbValues.Append(m_bandwd5.ToString() + ",");

            sbColumns.Append("bandwd6,");
            sbValues.Append(m_bandwd6.ToString() + ",");

            sbColumns.Append("bandwd7,");
            sbValues.Append(m_bandwd7.ToString() + ",");

            sbColumns.Append("bandwd8");
            sbValues.Append(m_bandwd8.ToString());

            return ("INSERT into " + tableName + " (" + sbColumns + ") values (" +
                            sbValues + ")");
        }


        //	*********************************************************************
        //	Copy a site record into a newly created Radio.
        //	*********************************************************************
        protected void TsCopRad(eTsType eType, TsRadio tRad)
        {
            if (eType == eTsType.ft)
            {
                m_cmd = tRad.m_cmd;
                m_recstat = tRad.m_recstat;
            }

            m_call1 = tRad.m_call1;
            m_name = tRad.m_name;
            m_latit = tRad.m_latit;
            m_longit = tRad.m_longit;
            if (eType == eTsType.mt)
            {
                m_strlatit = tRad.m_strlatit;
                m_strlatits = tRad.m_strlatits;
                m_strlongit = tRad.m_strlongit;
                m_strlongits = tRad.m_strlongits;
                m_userid = tRad.m_userid;
            }
            m_grnd = tRad.m_grnd;
            m_mdate = tRad.m_mdate;
            m_mtime = tRad.m_mtime;
            m_prov = tRad.m_prov;
            m_oper = tRad.m_oper;
            m_stats = tRad.m_stats;
            m_loc = tRad.m_loc;
            m_icaccount = tRad.m_icaccount;
            m_reg = tRad.m_reg;
            m_snumb = tRad.m_snumb;
            m_notwr = tRad.m_notwr;
            m_nots = tRad.m_nots;
            m_spoint = tRad.m_spoint;
            m_sdate = tRad.m_sdate;
            m_oprtyp = tRad.m_oprtyp;
            m_bandwd1 = tRad.m_bandwd1;
            m_bandwd2 = tRad.m_bandwd2;
            m_bandwd3 = tRad.m_bandwd3;
            m_bandwd4 = tRad.m_bandwd4;
            m_bandwd5 = tRad.m_bandwd5;
            m_bandwd6 = tRad.m_bandwd6;
            m_bandwd7 = tRad.m_bandwd7;
            m_bandwd8 = tRad.m_bandwd8;

            m_nameop = tRad.m_nameop;

            return;
        }


        //************************************************************************
        /// <summary>
        /// This routine will generate the XML object from the radio.  It calls
        ///	The corresponding routines for the channels and antennas.
        ///	This routine returns the radio Element, with the site node below it,
        ///	the antennas and the channels can be appended to the returned radio
        ///	node.   This will generate a structure like the following:-
        ///		radio - site - [site properties...]
        ///		        antennas - antenna - [antenna properties...]
        ///                      antenna - [antenna properties...]
        ///		                   ...
        ///		        channels - channel - [channel properties...]
        ///	                     channel - [channel properties...]
        ///	                     ...
        /// </summary>
        ///	<param name="eType">The type of the radio mt or ft.</param>
        /// <param name="oFile">The XML file object that this radio will be added to
        ///	</param>
        /// <returns>XMLDocumentFragment corresponding to a single TS Radio without
        ///          any antennas or channels.</returns>
        public XmlElement TsRadioXml(TsFile oFile)
        {
            eTsType eType = Type;

            XmlElement tRadNode = oFile.CreateElement("radio");

            XmlElement tSiteNode = oFile.CreateElement("site");
            if (eType == eTsType.ft)
            {
                tSiteNode.AppendChild(oFile.XmlEl("cmd", m_cmd));
                tSiteNode.AppendChild(oFile.XmlEl("recstat", m_recstat));
            }
            tSiteNode.AppendChild(oFile.XmlEl("call1", m_call1));
            tSiteNode.AppendChild(oFile.XmlEl("name", m_name));
            tSiteNode.AppendChild(oFile.XmlEl("latit", m_latit.ToString()));
            if (eType == eTsType.mt)
            {
                tSiteNode.AppendChild(oFile.XmlEl("strlatit", m_strlatit));
                tSiteNode.AppendChild(oFile.XmlEl("strlatits", m_strlatits));
            }
            tSiteNode.AppendChild(oFile.XmlEl("longit", m_longit.ToString()));
            if (eType == eTsType.mt)
            {
                tSiteNode.AppendChild(oFile.XmlEl("strlongit", m_strlongit));
                tSiteNode.AppendChild(oFile.XmlEl("strlongits", m_strlongits));
            }
            tSiteNode.AppendChild(oFile.XmlEl("grnd", m_grnd.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("prov", m_prov));
            tSiteNode.AppendChild(oFile.XmlEl("oper", m_oper));
            tSiteNode.AppendChild(oFile.XmlEl("stats", m_stats));
            tSiteNode.AppendChild(oFile.XmlEl("loc", m_loc));
            tSiteNode.AppendChild(oFile.XmlEl("icaccount", m_icaccount));
            tSiteNode.AppendChild(oFile.XmlEl("reg", m_reg));
            tSiteNode.AppendChild(oFile.XmlEl("snumb", m_snumb));
            tSiteNode.AppendChild(oFile.XmlEl("notwr", m_notwr.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("nots", m_nots));
            tSiteNode.AppendChild(oFile.XmlEl("spoint", m_spoint.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("sdate", m_sdate));
            tSiteNode.AppendChild(oFile.XmlEl("oprtyp", m_oprtyp));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd1", m_bandwd1.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd2", m_bandwd2.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd3", m_bandwd3.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd4", m_bandwd4.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd5", m_bandwd5.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd6", m_bandwd6.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd7", m_bandwd7.ToString()));
            tSiteNode.AppendChild(oFile.XmlEl("bandwd8", m_bandwd8.ToString()));
            if (eType == eTsType.mt)
            {
                tSiteNode.AppendChild(oFile.XmlEl("mdate", m_mdate));
                tSiteNode.AppendChild(oFile.XmlEl("mtime", m_mtime));
                tSiteNode.AppendChild(oFile.XmlEl("userid", m_userid));
            }
            //	Optional elements.
            tSiteNode.AppendChild(oFile.XmlEl("nameop", m_nameop));

            tRadNode.AppendChild(tSiteNode);	//	Add the site to the radio.

            //	Now add the subsidiary elements
            XmlElement tSubsid = null;
            if (m_numtown > 0)
            {
                if (tSubsid == null)
                {
                    tSubsid = oFile.CreateElement("subsidiary");
                }
                XmlElement XmlTown = oFile.CreateElement("towernotes");
                foreach (TowerNote tn in m_aTown)
                {
                    XmlTown.AppendChild(tn.TowerNoteXml(oFile));
                }
                tSubsid.AppendChild(XmlTown);
            }

            if (tSubsid != null)
            {
                tRadNode.AppendChild(tSubsid);
            }
            /*			
                        // Now start on the Antennas
                        XmlElement tAntennas = oFile.CreateElement();
                        for (int nInd = 0; nInd < m_numants; nInd++){
                            tAntennas.AppendChild(m_Antennas[nInd].TsAntennaXml(eType, oFile));
                        }
                        tRadNode.AppendChild(tAntennas);
			
                        //	And now the channels
                        XmlElement tChannels = oFile.CreateElement();
                        for (int nInd = 0; nInd < m_numchan; nInd++){
                            tChannels.AppendChild(m_Channels[nInd].TsChannelXml(eType, oFile));
                        }
                        tRadNode.AppendChild(tChannels);
            */
            return tRadNode;
        }


        /// <summary>
        /// Test for the presence of a call sign in a file without reading in the entire set of
        ///	site data.
        /// </summary>
        /// <param name="eType">Either mt or ft</param>
        /// <param name="sTable">If ft this is the file name</param>
        /// <param name="cCall">The call sign we check for</param>
        /// <returns>True if present, false for anything else</returns>
        public static bool exists(eTsType eType, string sTable, string cCall)
        {
            int nCount = 0;
            string sTableName;

            if (eType == eTsType.mt)
            {
                sTableName = "main.mt_site";
            }
            else
            {
                sTableName = HttpContext.Current.Session["s_schema"].ToString() + ".ft_" + sTable.Trim() + "_site";
            }
            dbconnect oCn = new dbconnect();

            nCount = (int)oCn.getscalar("SELECT count(*) " +
                                           "FROM " + sTableName +
                                         " WHERE call1='" + cCall.Trim() + "'");

            oCn.dbdisconnect();

            return (nCount > 0);
        }
    }


    /// <summary>
    /// This is the MDB antenna information class.  It inherits from TsAntenna
    ///
    /// </summary>
    public class mtAntenna : TsAntenna
    {
        public string call1
        {
            get { return m_call1; }
            set { m_call1 = value; }
        }
        public string call2
        {
            get { return m_call2; }
            set { m_call2 = value; }
        }
        public string bndcde
        {
            get { return m_bndcde; }
            set { m_bndcde = value; }
        }
        public short anum
        {
            get { return m_anum; }
            set { m_anum = value; }
        }
        public string ause
        {
            get { return m_ause; }
            set { m_ause = value; }
        }
        public string acode
        {
            get { return m_acode; }
            set { m_acode = value; }
        }
        public float aht
        {
            get { return m_aht; }
            set { m_aht = value; }
        }
        public float azmth
        {
            get { return m_azmth; }
        }
        public float elvtn
        {
            get { return m_elvtn; }
        }
        public float dist
        {
            get { return m_dist; }
        }
        public string offazm
        {
            get { return m_offazm; }
            set { m_offazm = value; }
        }
        public float tazmth
        {
            get { return m_tazmth; }
            set { m_tazmth = value; }
        }
        public float telvtn
        {
            get { return m_telvtn; }
            set { m_telvtn = value; }
        }
        public float tgain
        {
            get { return m_tgain; }
            set { m_tgain = value; }
        }
        public string txfdlnth
        {
            get { return m_txfdlnth; }
            set { m_txfdlnth = value; }
        }
        public float txfdlnlh
        {
            get { return m_txfdlnlh; }
            set { m_txfdlnlh = value; }
        }
        public string txfdlntv
        {
            get { return m_txfdlntv; }
            set { m_txfdlntv = value; }
        }
        public float txfdlnlv
        {
            get { return m_txfdlnlv; }
            set { m_txfdlnlv = value; }
        }
        public string rxfdlnth
        {
            get { return m_rxfdlnth; }
            set { m_rxfdlnth = value; }
        }
        public float rxfdlnlh
        {
            get { return m_rxfdlnlh; }
            set { m_rxfdlnlh = value; }
        }
        public string rxfdlntv
        {
            get { return m_rxfdlntv; }
            set { m_rxfdlntv = value; }
        }
        public float rxfdlnlv
        {
            get { return m_rxfdlnlv; }
            set { m_rxfdlnlv = value; }
        }
        public float txpadpam
        {
            get { return m_txpadpam; }
            set { m_txpadpam = value; }
        }
        public float rxpadlna
        {
            get { return m_rxpadlna; }
            set { m_rxpadlna = value; }
        }
        public float txcompl
        {
            get { return m_txcompl; }
            set { m_txcompl = value; }
        }
        public float rxcompl
        {
            get { return m_rxcompl; }
            set { m_rxcompl = value; }
        }
        public float obsloss
        {
            get { return m_obsloss; }
            set { m_obsloss = value; }
        }
        public float kvalue
        {
            get { return m_kvalue; }
            set { m_kvalue = value; }
        }
        public short atwrno
        {
            get { return m_atwrno; }
            set { m_atwrno = value; }
        }
        public string nota
        {
            get { return m_nota; }
            set { m_nota = value; }
        }
        public string apoint
        {
            get { return m_apoint; }
            set { m_apoint = value; }
        }
        public string sdate
        {
            get { return m_sdate; }
            set { m_sdate = value; }
        }
        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }
        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }
        public string licence
        {
            get { return m_licence; }
            set { m_licence = value; }
        }
        public string userid
        {
            get { return m_userid; }
            set { m_userid = value; }
        }

        /// <summary>
        /// This is the default constructor.  It will create an empty mtAntenna
        ///	object.
        /// </summary>
        //	The default constructor
        public mtAntenna()
            : base(eTsType.mt)
        {
        }

        /// <summary>
        /// This constructor will create an mtAntenna from a row in an main.mt_ante
        ///	table.  The row must be passed in as the parameter.
        /// </summary>
        /// <param name="oDR">The Datarow that contains the mtAntenna information
        ///	that the user wishes to read into the current object.</param>
        public mtAntenna(DataRow oDR)
            : base(eTsType.mt, oDR)
        {
        }


        public XmlElement mtAntennaXml(TsFile oFile)
        {
            return (base.TsAntennaXml(eTsType.mt, oFile));
        }
    }


    /// <summary>
    /// This is the channel information for mt channels.
    /// </summary>
    public class mtChannel : TsChannel
    {

        public string call1
        {
            get { return m_call1; }
            set { m_call1 = value; }
        }
        public string call2
        {
            get { return m_call2; }
            set { m_call2 = value; }
        }
        public string bndcde
        {
            get { return m_bndcde; }
            set { m_bndcde = value; }
        }
        public string splan
        {
            get { return m_splan; }
            set { m_splan = value; }
        }
        public short hl
        {
            get { return m_hl; }
            set { m_hl = value; }
        }
        public short vh
        {
            get { return m_vh; }
            set { m_vh = value; }
        }
        public string chid
        {
            get { return m_chid; }
            set { m_chid = value; }
        }
        public double freqtx
        {
            get { return m_freqtx; }
            set { m_freqtx = value; }
        }
        public string poltx
        {
            get { return m_poltx; }
            set { m_poltx = value; }
        }
        public short antnumbtx1
        {
            get { return m_antnumbtx1; }
            set { m_antnumbtx1 = value; }
        }
        public short antnumbtx2
        {
            get { return m_antnumbtx2; }
            set { m_antnumbtx2 = value; }
        }
        public string eqpttx
        {
            get { return m_eqpttx; }
            set { m_eqpttx = value; }
        }
        public string eqptutx
        {
            get { return m_eqptutx; }
            set { m_eqptutx = value; }
        }
        public float pwrtx
        {
            get { return m_pwrtx; }
            set { m_pwrtx = value; }
        }
        public float atpccde
        {
            get { return m_atpccde; }
            set { m_atpccde = value; }
        }
        public float afsltx1
        {
            get { return m_afsltx1; }
            set { m_afsltx1 = value; }
        }
        public float afsltx2
        {
            get { return m_afsltx2; }
            set { m_afsltx2 = value; }
        }
        public string traftx
        {
            get { return m_traftx; }
            set { m_traftx = value; }
        }
        public string srvctx
        {
            get { return m_srvctx; }
            set { m_srvctx = value; }
        }
        public string stattx
        {
            get { return m_stattx; }
            set { m_stattx = value; }
        }
        public double freqrx
        {
            get { return m_freqrx; }
            set { m_freqrx = value; }
        }
        public string polrx
        {
            get { return m_polrx; }
            set { m_polrx = value; }
        }
        public short antnumbrx1
        {
            get { return m_antnumbrx1; }
            set { m_antnumbrx1 = value; }
        }
        public short antnumbrx2
        {
            get { return m_antnumbrx2; }
            set { m_antnumbrx2 = value; }
        }
        public short antnumbrx3
        {
            get { return m_antnumbrx3; }
            set { m_antnumbrx3 = value; }
        }
        public string eqptrx
        {
            get { return m_eqptrx; }
            set { m_eqptrx = value; }
        }
        public string eqpturx
        {
            get { return m_eqpturx; }
            set { m_eqpturx = value; }
        }
        public float afslrx1
        {
            get { return m_afslrx1; }
            set { m_afslrx1 = value; }
        }
        public float afslrx2
        {
            get { return m_afslrx2; }
            set { m_afslrx2 = value; }
        }
        public float afslrx3
        {
            get { return m_afslrx3; }
            set { m_afslrx3 = value; }
        }
        public float pwrrx1
        {
            get { return m_pwrrx1; }
            set { m_pwrrx1 = value; }
        }
        public float pwrrx2
        {
            get { return m_pwrrx2; }
            set { m_pwrrx2 = value; }
        }
        public float pwrrx3
        {
            get { return m_pwrrx3; }
            set { m_pwrrx3 = value; }
        }
        public string trafrx
        {
            get { return m_trafrx; }
            set { m_trafrx = value; }
        }
        public float esint
        {
            get { return m_esint; }
            set { m_esint = value; }
        }
        public float tsint
        {
            get { return m_tsint; }
            set { m_tsint = value; }
        }
        public string srvcrx
        {
            get { return m_srvcrx; }
            set { m_srvcrx = value; }
        }
        public string statrx
        {
            get { return m_statrx; }
            set { m_statrx = value; }
        }
        public string routnumb
        {
            get { return m_routnumb; }
            set { m_routnumb = value; }
        }
        public short stnnumb
        {
            get { return m_stnnumb; }
            set { m_stnnumb = value; }
        }
        public short hopnumb
        {
            get { return m_hopnumb; }
            set { m_hopnumb = value; }
        }
        public string sdate
        {
            get { return m_sdate; }
            set { m_sdate = value; }
        }
        public string notetx
        {
            get { return m_notetx; }
            set { m_notetx = value; }
        }
        public string noterx
        {
            get { return m_noterx; }
            set { m_noterx = value; }
        }
        public string notegnl
        {
            get { return m_notegnl; }
            set { m_notegnl = value; }
        }
        public string cpoint
        {
            get { return m_cpoint; }
            set { m_cpoint = value; }
        }
        public string feetx
        {
            get { return m_feetx; }
            set { m_feetx = value; }
        }
        public string feerx
        {
            get { return m_feerx; }
            set { m_feerx = value; }
        }
        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }
        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }

        /// <summary>
        /// This creates an empty mt Channel object.
        /// </summary>
        public mtChannel()
            : base(eTsType.mt)
        {
        }


        /// <summary>
        /// This creates a new mt Channel object and fills it in from the current
        ///	row that is passed in.
        /// </summary>
        /// <param name="oDR">a DataRow object containing the channel information
        ///	</param>
        public mtChannel(DataRow oDR)
            : base(eTsType.mt, oDR)
        {
        }


        public XmlElement mtChannelXml(TsFile oFile)
        {
            return (base.TsChannelXml(eTsType.mt, oFile));
        }
    }


    /// <summary>
    /// mtRadio is the TS radio structure that reflects the status of 
    /// radios in the MDB.  There is no insert or update capability as yet,
    /// as this is still done from the Alpha.
    /// </summary>
    public class mtRadio : TsRadio
    {
        public int depth
        {
            get { return (int)m_nDepth; } // nDepth is set when created.
        }

        public string call1
        {
            get { return m_call1; }
            set { m_call1 = value; }
        }
        public string name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        public int latit
        {
            get { return m_latit; }
            set { m_latit = value; }
        }
        public string strlatit
        {
            get { return m_strlatit; }
            set { m_strlatit = value; }
        }
        public string strlatits
        {
            get { return m_strlatits; }
            set { m_strlatits = value; }
        }
        public int longit
        {
            get { return m_longit; }
            set { m_longit = value; }
        }
        public string strlongit
        {
            get { return m_strlongit; }
            set { m_strlongit = value; }
        }
        public string strlongits
        {
            get { return m_strlongits; }
            set { m_strlongits = value; }
        }
        public double grnd
        {
            get { return m_grnd; }
            set { m_grnd = value; }
        }
        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }
        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }
        public string userid
        {
            get { return m_userid; }
            set { m_userid = value; }
        }
        public string prov
        {
            get { return m_prov; }
            set { m_prov = value; }
        }
        public string oper
        {
            get { return m_oper; }
            set { m_oper = value; }
        }
        public string stats
        {
            get { return m_stats; }
            set { m_stats = value; }
        }
        public string loc
        {
            get { return m_loc; }
            set { m_loc = value; }
        }
        public string icaccount
        {
            get { return m_icaccount; }
            set { m_icaccount = value; }
        }
        public string reg
        {
            get { return m_reg; }
            set { m_reg = value; }
        }
        public string snumb
        {
            get { return m_snumb; }
            set { m_snumb = value; }
        }
        public int notwr
        {
            get { return m_notwr; }
            set { m_notwr = value; }
        }
        public string nots
        {
            get { return m_nots; }
            set { m_nots = value; }
        }
        public int spoint
        {
            get { return m_spoint; }
            set { m_spoint = value; }
        }
        public string sdate
        {
            get { return m_sdate; }
            set { m_sdate = value; }
        }
        public string oprtyp
        {
            get { return m_oprtyp; }
            set { m_oprtyp = value; }
        }
        public uint bandwd1
        {
            get { return m_bandwd1; }
            set { m_bandwd1 = value; }
        }
        public uint bandwd2
        {
            get { return m_bandwd2; }
            set { m_bandwd2 = value; }
        }
        public uint bandwd3
        {
            get { return m_bandwd3; }
            set { m_bandwd3 = value; }
        }
        public uint bandwd4
        {
            get { return m_bandwd4; }
            set { m_bandwd4 = value; }
        }
        public uint bandwd5
        {
            get { return m_bandwd5; }
            set { m_bandwd5 = value; }
        }
        public uint bandwd6
        {
            get { return m_bandwd6; }
            set { m_bandwd6 = value; }
        }
        public uint bandwd7
        {
            get { return m_bandwd7; }
            set { m_bandwd7 = value; }
        }
        public uint bandwd8
        {
            get { return m_bandwd8; }
            set { m_bandwd8 = value; }
        }

        /// <summary>
        /// This constructs a basic empty mtRadio structure.
        /// </summary>
        public mtRadio()
        {
            m_etype = eTsType.mt;
            m_nDepth = 0;	// Indicate that nothing has been set.
            m_numants = 0;
            m_numchan = 0;
        }

        /// <summary>
        /// This will construct an mtRadio object for the specified callsign.
        ///	It will read the whole thing into the object
        /// </summary>
        /// <param name="inCall">The callsign of the radio we seek</param>
        /// <param name="nInDepth">Depth is 1 for Site information only,
        ///	2 for Site and Antenna data, and 3 for all, Site, Antenna, and Channels
        ///	</param>
        public mtRadio(string inCall, int nInDepth) :
            base(eTsType.mt, inCall, "", nInDepth)
        {
        }


        public mtRadio(string inCall, TsAntennaKey[] atKeys) :
            base(eTsType.mt, inCall, atKeys, "")
        {
        }


        /// <summary>
        /// Constructor for an mt radio with additional subsidiary information.
        /// </summary>
        /// <param name="inCall">Site Call sign</param>
        /// <param name="atKeys">an array of antennakeys</param>
        /// <param name="sAdditional">The subsidiary info requested.  This is a
        ///	string of subsidiary names separated by commas.
        ///	</param>
        public mtRadio(string inCall, TsAntennaKey[] atKeys, string sAdditional) :
            base(eTsType.mt, inCall, atKeys, "", sAdditional)
        {
        }


        public XmlElement mtRadioXml(TsFile oFile)
        {
            XmlElement tRadNode = base.TsRadioXml(oFile);
            // Now start on the Antennas
            XmlElement tAntennas = oFile.CreateElement("antennas");

            for (int nInd = 0; nInd < numants; nInd++)
            {
                tAntennas.AppendChild(((mtAntenna)Antennas[nInd]).mtAntennaXml(oFile));
            }
            tRadNode.AppendChild(tAntennas);

            //	And now the channels
            XmlElement tChannels = oFile.CreateElement("channels");
            for (int nInd = 0; nInd < numchan; nInd++)
            {
                tChannels.AppendChild(((mtChannel)Channels[nInd]).mtChannelXml(oFile));
            }
            tRadNode.AppendChild(tChannels);

            return tRadNode;
        }


        public static bool exists(string cCall)
        {
            return TsRadio.exists(eTsType.mt, "", cCall);
        }

    }


    /// <summary>
    /// This is the ft antenna information class.
    /// </summary>
    public class ftAntenna : TsAntenna
    {

        public string cmd
        {
            get { return m_cmd; }
            set { m_cmd = value; }
        }

        public string recstat
        {
            get { return m_recstat; }
            set { m_recstat = value; }
        }

        public string call1
        {
            get { return m_call1; }
            set { m_call1 = value; }
        }
        public string call2
        {
            get { return m_call2; }
            set { m_call2 = value; }
        }
        public string bndcde
        {
            get { return m_bndcde; }
            set { m_bndcde = value; }
        }
        public short anum
        {
            get { return m_anum; }
            set { m_anum = value; }
        }
        public string ause
        {
            get { return m_ause; }
            set { m_ause = value; }
        }
        public string acode
        {
            get { return m_acode; }
            set { m_acode = value; }
        }
        public float aht
        {
            get { return m_aht; }
            set { m_aht = value; }
        }
        public float azmth
        {
            get { return m_azmth; }
        }
        public float elvtn
        {
            get { return m_elvtn; }
        }
        public float dist
        {
            get { return m_dist; }
        }
        public string offazm
        {
            get { return m_offazm; }
            set { m_offazm = value; }
        }
        public float tazmth
        {
            get { return m_tazmth; }
            set { m_tazmth = value; }
        }
        public float telvtn
        {
            get { return m_telvtn; }
            set { m_telvtn = value; }
        }
        public float tgain
        {
            get { return m_tgain; }
            set { m_tgain = value; }
        }
        public string txfdlnth
        {
            get { return m_txfdlnth; }
            set { m_txfdlnth = value; }
        }
        public float txfdlnlh
        {
            get { return m_txfdlnlh; }
            set { m_txfdlnlh = value; }
        }
        public string txfdlntv
        {
            get { return m_txfdlntv; }
            set { m_txfdlntv = value; }
        }
        public float txfdlnlv
        {
            get { return m_txfdlnlv; }
            set { m_txfdlnlv = value; }
        }
        public string rxfdlnth
        {
            get { return m_rxfdlnth; }
            set { m_rxfdlnth = value; }
        }
        public float rxfdlnlh
        {
            get { return m_rxfdlnlh; }
            set { m_rxfdlnlh = value; }
        }
        public string rxfdlntv
        {
            get { return m_rxfdlntv; }
            set { m_rxfdlntv = value; }
        }
        public float rxfdlnlv
        {
            get { return m_rxfdlnlv; }
            set { m_rxfdlnlv = value; }
        }
        public float txpadpam
        {
            get { return m_txpadpam; }
            set { m_txpadpam = value; }
        }
        public float rxpadlna
        {
            get { return m_rxpadlna; }
            set { m_rxpadlna = value; }
        }
        public float txcompl
        {
            get { return m_txcompl; }
            set { m_txcompl = value; }
        }
        public float rxcompl
        {
            get { return m_rxcompl; }
            set { m_rxcompl = value; }
        }
        public float obsloss
        {
            get { return m_obsloss; }
            set { m_obsloss = value; }
        }
        public float kvalue
        {
            get { return m_kvalue; }
            set { m_kvalue = value; }
        }
        public short atwrno
        {
            get { return m_atwrno; }
            set { m_atwrno = value; }
        }
        public string nota
        {
            get { return m_nota; }
            set { m_nota = value; }
        }
        public string apoint
        {
            get { return m_apoint; }
            set { m_apoint = value; }
        }
        public string sdate
        {
            get { return m_sdate; }
            set { m_sdate = value; }
        }
        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }
        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }
        public string licence
        {
            get { return m_licence; }
            set { m_licence = value; }
        }
        /// <summary>
        /// Constructor for the empty ft Antenna structure.
        /// </summary>
        public ftAntenna()
            : base(eTsType.ft)
        {
        }

        /// <summary>
        /// Constructor for an ft antenna structure from a datarow in the antenna
        ///	table.
        /// </summary>
        /// <param name="oDR">The DataRow object to have the antenna constructed 
        ///	from.</param>
        public ftAntenna(DataRow oDR)
            : base(eTsType.ft, oDR)
        {
        }


        /// <summary>
        /// Create the SQL INSERT command to insert this antenna into the specified 
        ///	table.
        /// </summary>
        /// <param name="sTable">The base name of the antenna table.</param>
        /// <returns>A string containing the INSERT Command.</returns>
        public string InsertCommand(string sTable)
        {
            return (base.InsertCommand(eTsType.ft, sTable));
        }


        /// <summary>
        /// Copy an Antenna to the current antenna.  Both must be of the same 
        ///	type.
        /// </summary>
        /// <param name="tAnt">The antenna structure to be copied.</param>
        public void ftCopAnt(ftAntenna tAnt)
        {		//	Antenna struct source.
            base.TsCopAnt(eTsType.ft, tAnt);
            return;
        }


        public XmlElement ftAntennaXml(TsFile oFile)
        {
            return (base.TsAntennaXml(eTsType.ft, oFile));
        }
    }


    /// <summary>
    /// This is the channel information for ft channels.
    /// </summary>
    public class ftChannel : TsChannel
    {

        public string cmd
        {
            get { return m_cmd; }
            set { m_cmd = value; }
        }

        public string recstat
        {
            get { return m_recstat; }
            set { m_recstat = value; }
        }

        public string call1
        {
            get { return m_call1; }
            set { m_call1 = value; }
        }
        public string call2
        {
            get { return m_call2; }
            set { m_call2 = value; }
        }
        public string bndcde
        {
            get { return m_bndcde; }
            set { m_bndcde = value; }
        }
        public string splan
        {
            get { return m_splan; }
            set { m_splan = value; }
        }
        public short hl
        {
            get { return m_hl; }
            set { m_hl = value; }
        }
        public short vh
        {
            get { return m_vh; }
            set { m_vh = value; }
        }
        public string chid
        {
            get { return m_chid; }
            set { m_chid = value; }
        }
        public double freqtx
        {
            get { return m_freqtx; }
            set { m_freqtx = value; }
        }
        public string poltx
        {
            get { return m_poltx; }
            set { m_poltx = value; }
        }
        public short antnumbtx1
        {
            get { return m_antnumbtx1; }
            set { m_antnumbtx1 = value; }
        }
        public short antnumbtx2
        {
            get { return m_antnumbtx2; }
            set { m_antnumbtx2 = value; }
        }
        public string eqpttx
        {
            get { return m_eqpttx; }
            set { m_eqpttx = value; }
        }
        public string eqptutx
        {
            get { return m_eqptutx; }
            set { m_eqptutx = value; }
        }
        public float pwrtx
        {
            get { return m_pwrtx; }
            set { m_pwrtx = value; }
        }
        public float atpccde
        {
            get { return m_atpccde; }
            set { m_atpccde = value; }
        }
        public float afsltx1
        {
            get { return m_afsltx1; }
            set { m_afsltx1 = value; }
        }
        public float afsltx2
        {
            get { return m_afsltx2; }
            set { m_afsltx2 = value; }
        }
        public string traftx
        {
            get { return m_traftx; }
            set { m_traftx = value; }
        }
        public string srvctx
        {
            get { return m_srvctx; }
            set { m_srvctx = value; }
        }
        public string stattx
        {
            get { return m_stattx; }
            set { m_stattx = value; }
        }
        public double freqrx
        {
            get { return m_freqrx; }
            set { m_freqrx = value; }
        }
        public string polrx
        {
            get { return m_polrx; }
            set { m_polrx = value; }
        }
        public short antnumbrx1
        {
            get { return m_antnumbrx1; }
            set { m_antnumbrx1 = value; }
        }
        public short antnumbrx2
        {
            get { return m_antnumbrx2; }
            set { m_antnumbrx2 = value; }
        }
        public short antnumbrx3
        {
            get { return m_antnumbrx3; }
            set { m_antnumbrx3 = value; }
        }
        public string eqptrx
        {
            get { return m_eqptrx; }
            set { m_eqptrx = value; }
        }
        public string eqpturx
        {
            get { return m_eqpturx; }
            set { m_eqpturx = value; }
        }
        public float afslrx1
        {
            get { return m_afslrx1; }
            set { m_afslrx1 = value; }
        }
        public float afslrx2
        {
            get { return m_afslrx2; }
            set { m_afslrx2 = value; }
        }
        public float afslrx3
        {
            get { return m_afslrx3; }
            set { m_afslrx3 = value; }
        }
        public float pwrrx1
        {
            get { return m_pwrrx1; }
            set { m_pwrrx1 = value; }
        }
        public float pwrrx2
        {
            get { return m_pwrrx2; }
            set { m_pwrrx2 = value; }
        }
        public float pwrrx3
        {
            get { return m_pwrrx3; }
            set { m_pwrrx3 = value; }
        }
        public string trafrx
        {
            get { return m_trafrx; }
            set { m_trafrx = value; }
        }
        public float esint
        {
            get { return m_esint; }
            set { m_esint = value; }
        }
        public float tsint
        {
            get { return m_tsint; }
            set { m_tsint = value; }
        }
        public string srvcrx
        {
            get { return m_srvcrx; }
            set { m_srvcrx = value; }
        }
        public string statrx
        {
            get { return m_statrx; }
            set { m_statrx = value; }
        }
        public string routnumb
        {
            get { return m_routnumb; }
            set { m_routnumb = value; }
        }
        public short stnnumb
        {
            get { return m_stnnumb; }
            set { m_stnnumb = value; }
        }
        public short hopnumb
        {
            get { return m_hopnumb; }
            set { m_hopnumb = value; }
        }
        public string sdate
        {
            get { return m_sdate; }
            set { m_sdate = value; }
        }
        public string notetx
        {
            get { return m_notetx; }
            set { m_notetx = value; }
        }
        public string noterx
        {
            get { return m_noterx; }
            set { m_noterx = value; }
        }
        public string notegnl
        {
            get { return m_notegnl; }
            set { m_notegnl = value; }
        }
        public string cpoint
        {
            get { return m_cpoint; }
            set { m_cpoint = value; }
        }
        public string feetx
        {
            get { return m_feetx; }
            set { m_feetx = value; }
        }
        public string feerx
        {
            get { return m_feerx; }
            set { m_feerx = value; }
        }
        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }
        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }

        /// <summary>
        /// The basic constructor for an empty ft Channel.
        /// </summary>
        public ftChannel()
            : base(eTsType.ft)
        {
        }

        /// <summary>
        /// This constructor will create an ft Channel from a row in a channel 
        /// table.
        /// </summary>
        /// <param name="oDR">A DataRow object containing the channel information 
        ///	we want.</param>
        public ftChannel(DataRow oDR)
            : base(eTsType.ft, oDR)
        {
        }


        /// <summary>
        /// This will create the INSERT command for the current channel.  This 
        /// insert command can be used to add the channel to the table.
        /// </summary>
        /// <param name="sTable">The base name of the table.</param>
        /// <returns>A String containing the SQL INSERT command for the channel.
        /// </returns>
        public string InsertCommand(string sTable)
        {
            return (base.InsertCommand(eTsType.ft, sTable));
        }


        /// <summary>
        /// Copy the argument into the current channel.
        /// </summary>
        /// <param name="tChn">Channel object to be copied.</param>
        public void ftCopChn(ftChannel tChn)
        {		//	Channel struct source.
            base.TsCopChn(eTsType.ft, tChn);
            return;
        }


        public XmlElement ftChannelXml(TsFile oFile)
        {
            return (base.TsChannelXml(eTsType.ft, oFile));
        }

    }


    /// <summary>
    /// ftRadio is the TS radio structure that reflects the status of 
    /// radios in a pdf.  The Radio also contains the arrays of Antennas and
    ///	channels (ftAntenna(s) and ftChannel(s)).
    /// </summary>
    public class ftRadio : TsRadio
    {

        public int depth
        {
            get { return (int)m_nDepth; } // nDepth is set when created.
        }

        public string cmd
        {
            get { return m_cmd; }
            set { m_cmd = value; }
        }

        public string recstat
        {
            get { return m_recstat; }
            set { m_recstat = value; }
        }

        public string call1
        {
            get { return m_call1; }
            set { m_call1 = value; }
        }
        public string name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        public int latit
        {
            get { return m_latit; }
            set { m_latit = value; }
        }
        public int longit
        {
            get { return m_longit; }
            set { m_longit = value; }
        }
        public double grnd
        {
            get { return m_grnd; }
            set { m_grnd = value; }
        }
        public string mdate
        {
            get { return m_mdate; }
            set { m_mdate = value; }
        }
        public string mtime
        {
            get { return m_mtime; }
            set { m_mtime = value; }
        }
        public string prov
        {
            get { return m_prov; }
            set { m_prov = value; }
        }
        public string oper
        {
            get { return m_oper; }
            set { m_oper = value; }
        }
        public string stats
        {
            get { return m_stats; }
            set { m_stats = value; }
        }
        public string loc
        {
            get { return m_loc; }
            set { m_loc = value; }
        }
        public string icaccount
        {
            get { return m_icaccount; }
            set { m_icaccount = value; }
        }
        public string reg
        {
            get { return m_reg; }
            set { m_reg = value; }
        }
        public string snumb
        {
            get { return m_snumb; }
            set { m_snumb = value; }
        }
        public int notwr
        {
            get { return m_notwr; }
            set { m_notwr = value; }
        }
        public string nots
        {
            get { return m_nots; }
            set { m_nots = value; }
        }
        public int spoint
        {
            get { return m_spoint; }
            set { m_spoint = value; }
        }
        public string sdate
        {
            get { return m_sdate; }
            set { m_sdate = value; }
        }
        public string oprtyp
        {
            get { return m_oprtyp; }
            set { m_oprtyp = value; }
        }
        public uint bandwd1
        {
            get { return m_bandwd1; }
            set { m_bandwd1 = value; }
        }
        public uint bandwd2
        {
            get { return m_bandwd2; }
            set { m_bandwd2 = value; }
        }
        public uint bandwd3
        {
            get { return m_bandwd3; }
            set { m_bandwd3 = value; }
        }
        public uint bandwd4
        {
            get { return m_bandwd4; }
            set { m_bandwd4 = value; }
        }
        public uint bandwd5
        {
            get { return m_bandwd5; }
            set { m_bandwd5 = value; }
        }
        public uint bandwd6
        {
            get { return m_bandwd6; }
            set { m_bandwd6 = value; }
        }
        public uint bandwd7
        {
            get { return m_bandwd7; }
            set { m_bandwd7 = value; }
        }
        public uint bandwd8
        {
            get { return m_bandwd8; }
            set { m_bandwd8 = value; }
        }

        /// <summary>
        /// Constructor for an empty Site.
        /// </summary>
        public ftRadio()
        {
            m_etype = eTsType.mt;
            m_nDepth = 0;	// Indicate that nothing has been set.
            m_numants = 0;
            m_numchan = 0;
        }

        /// <summary>
        /// Constructor for a Site to be read in.  The callsign and the file
        ///	are given and the whole site (including channels and antennas) is 
        ///	read in and initialized.
        /// </summary>
        /// <param name="inCall">Call sign of the site.</param>
        /// <param name="cFile">Base file name of the ft file to read.</param>
        /// <param name="nInDepth">Depth is 1 for Site only, 2 for Site and 
        ///	antenna information, and 3 for Site, antenna and Channel information
        ///	</param>
        public ftRadio(string inCall, string cFile, int nInDepth) :
            base(eTsType.ft, inCall, cFile, nInDepth)
        {
        }


        /// <summary>
        /// Construct an ft Radio from a callsign and a selection of the antenna keys.
        /// </summary>
        /// <param name="inCall">Site callsign</param>
        /// <param name="cFile">The base name of the ft file</param>
        /// <param name="atKeys">An array (possibly empty) of TsAntennaKeys</param>
        public ftRadio(string inCall, string cFile, TsAntennaKey[] atKeys) :
            base(eTsType.ft, inCall, atKeys, cFile)
        {
        }


        /// <summary>
        /// Constructor for an ft radio with additional subsidiary information.
        /// </summary>
        /// <param name="inCall">Site Call sign</param>
        /// <param name="cFile">The base name of the ft file.</param>
        /// <param name="atKeys">an array of antennakeys</param>
        /// <param name="sAdditional">The subsidiary info requested.  This is a
        ///	string of subsidiary names separated by commas.
        ///	</param>
        public ftRadio(string inCall, string cFile, TsAntennaKey[] atKeys, string sAdditional) :
            base(eTsType.mt, inCall, atKeys, cFile, sAdditional)
        {
        }


        /// <summary>
        /// Shallow copy constructor for an ft Site.  Antenna and Channel arrays are
        ///	set to empty in the current object.
        /// </summary>
        /// <param name="tFtRadio">An ftRadio to be copied from.</param>
        public ftRadio(ftRadio tFtRadio)
        {
            m_etype = eTsType.ft;
            m_nDepth = 1;				//	Start at depth of 1.

            cmd = tFtRadio.cmd;
            recstat = tFtRadio.recstat;
            call1 = tFtRadio.call1;
            name = tFtRadio.name;
            latit = tFtRadio.latit;
            longit = tFtRadio.longit;
            grnd = tFtRadio.grnd;
            mdate = tFtRadio.mdate;
            mtime = tFtRadio.mtime;
            prov = tFtRadio.prov;
            oper = tFtRadio.oper;
            stats = tFtRadio.stats;
            loc = tFtRadio.loc;
            icaccount = tFtRadio.icaccount;
            reg = tFtRadio.reg;
            snumb = tFtRadio.snumb;
            notwr = tFtRadio.notwr;
            nots = tFtRadio.nots;
            spoint = tFtRadio.spoint;
            sdate = tFtRadio.sdate;
            oprtyp = tFtRadio.oprtyp;
            bandwd1 = tFtRadio.bandwd1;
            bandwd2 = tFtRadio.bandwd2;
            bandwd3 = tFtRadio.bandwd3;
            bandwd4 = tFtRadio.bandwd4;
            bandwd5 = tFtRadio.bandwd5;
            bandwd6 = tFtRadio.bandwd6;
            bandwd7 = tFtRadio.bandwd7;
            bandwd8 = tFtRadio.bandwd8;

            m_numants = 0;
            m_Antennas = null;
            m_numchan = 0;
            m_Channels = null;
        }


        //	**********************************************************************
        /// <summary>
        /// Returns the SQL INSERT command to insert this site into the site table
        /// </summary>
        /// <param name="sTable">The base name of the table.</param>
        /// <returns>String containing the INSERT statement.</returns>
        public string InsertCommand(string sTable)
        {
            return (base.InsertCommand(eTsType.ft, sTable));
        }


        //	**********************************************************************
        /// <summary>
        /// Find an antenna specified by call2, band, and antenna number in a site.
        /// </summary>
        /// <param name="cCall2">Call sign of the other end of the link.</param>
        /// <param name="cBand">Band code.</param>
        /// <param name="nAnum">The antenna number.</param>
        /// <returns>This will return the index of the antenna if found, and 
        ///	-1 if no antenna is found matching this description.</returns>
        public int ftFindAnt(string cCall2, string cBand, int nAnum)
        {
            for (int i = 0; i < m_numants && i < Antennas.Count; i++)
            {
                if (cCall2 == ((ftAntenna)Antennas[i]).call2 &&
                    cBand == ((ftAntenna)Antennas[i]).bndcde &&
                    nAnum == ((ftAntenna)Antennas[i]).anum)
                {
                    return i;	//	Found.
                }
            }
            return -1;		//	Not found
        }

        //**********************************************************************
        /// <summary>
        /// Adds a new empty antenna to the antenna array and returns its index
        /// </summary>
        /// <returns>The index of the new antenna in the antenna array</returns>
        public int ftAddAntLoc()
        {
            Antennas[m_numants] = new ftAntenna();
            base.setdepth(2);		//	Make sure that we know about antennas.
            return m_numants++;
        }


        //	*******************************************************************
        /// <summary>
        /// Given an existing antenna, this routine will either replace or 
        ///	add it to the antenna array, copying it in.
        /// </summary>
        /// <param name="tAnt">The antenna to be copied in</param>
        /// <param name="IsOverwrite">True if it is to be overwritten if it is 
        ///	already there, false, if it is not.</param>
        /// <returns>0</returns>
        public int ftAddAnt(ftAntenna tAnt, bool IsOverwrite)
        {
            int nAntNum = ftFindAnt(tAnt.call2, tAnt.bndcde, tAnt.anum);

            if (nAntNum < 0)
            {
                //	Not found
                nAntNum = ftAddAntLoc();
                IsOverwrite = true;
            }

            if (IsOverwrite)
            {
                ((ftAntenna)Antennas[nAntNum]).ftCopAnt(tAnt);
            }
            return 0;
        }


        //	********************************************************************
        /// <summary>
        /// Get the index of a specified channel in the channel array.
        /// </summary>
        /// <param name="cCall2">Callsign at the other end of the link</param>
        /// <param name="cBand">Band</param>
        /// <param name="cChid">Channel id.</param>
        /// <returns>Index of the channel or -1 if it was not found</returns>
        public int ftFindChn(string cCall2, string cBand, string cChid)
        {
            for (int i = 0; i < numchan && i < Channels.Count; i++)
            {
                if (cCall2 == ((ftChannel)Channels[i]).call2 &&
                        cBand == ((ftChannel)Channels[i]).bndcde &&
                        cChid == ((ftChannel)Channels[i]).chid)
                {
                    return i;	//	Found.
                }
            }
            return -1;		//	Not found
        }


        //	********************************************************************
        /// <summary>
        /// Adds a new empty channel to the end of the channel array.
        /// </summary>
        /// <returns>The index of the new channel.</returns>
        public int ftAddChnLoc()
        {
            m_Channels[m_numchan] = new ftChannel();
            setdepth(3);
            return m_numchan++;
        }


        //	*******************************************************************
        /// <summary>
        /// Adds a channel to the Channel array of the site.  If the channel
        ///	already exists in the site, the user has the option of overwriting 
        ///	it.
        /// </summary>
        /// <param name="tChn">The channel to be added.  It will be copied.</param>
        /// <param name="IsOverwrite">True if the channel is to be overwritten
        ///	if present.  False if it is not.</param>
        /// <returns>0</returns>
        public int ftAddChn(ftChannel tChn, bool IsOverwrite)
        {
            int nChnNum = ftFindChn(tChn.call2, tChn.bndcde, tChn.chid);

            if (nChnNum < 0)
            {
                //	Not found
                nChnNum = ftAddChnLoc();
                IsOverwrite = true;			//	Make sure that it is copied.
            }

            if (IsOverwrite)
            {
                ((ftChannel)Channels[nChnNum]).ftCopChn(tChn);
            }
            return 0;
        }


        //	********************************************************************
        /// <summary>
        /// Delete the site from the specified table.  The site record and all
        ///	the antennas and channels will be deleted.
        /// </summary>
        /// <param name="sTable">The base name of the table.</param>
        public void DeleteSite(string sTable)
        {
            dbconnect oConnect = new dbconnect();
            OdbcConnection oConn = oConnect.Connection;
            OdbcCommand oDelete = new OdbcCommand(
                                      "DELETE from " + HttpContext.Current.Session["s_cnString"].ToString() + ".ft_" + sTable.Trim() +
                                                                                         "_site " +
                                                                            "where call1='" + this.call1 + "'",
                                                                oConn);
            oDelete.ExecuteNonQuery();
            oDelete.Dispose();

            oDelete = new OdbcCommand(
                                      "DELETE from " + HttpContext.Current.Session["s_cnString"].ToString() + ".ft_" + sTable.Trim() + "_ante " +
                                            "where call1='" + this.call1 + "'", oConn);
            oDelete.ExecuteNonQuery();
            oDelete.Dispose();

            oDelete = new OdbcCommand(
                          "DELETE from " + HttpContext.Current.Session["s_cnString"].ToString() + ".ft_" + sTable.Trim() + "_chan " +
                                "where call1='" + this.call1 + "'", oConn);
            oDelete.ExecuteNonQuery();
            oDelete.Dispose();

            oConnect.dbdisconnect();
        }


        //	********************************************************************
        /// <summary>
        /// Writes out a site to a specified table.  All the antennas and channels
        /// are written as well.
        /// </summary>
        /// <param name="sTable">The base name of the table.</param>
        /// <param name="nDepth">The depth of the write: 1 - Site only, 2 - Site and
        ///	antenna, 3 - Site, antenna and channels.  This depth may not be
        ///	deeper than the depth the site contains.</param>
        public void WriteSite(string sTable, int nDepth)
        {
            dbconnect oConn = new dbconnect();
            string strCommand;

            strCommand = InsertCommand(sTable);
            //	We want to use a transaction so this insertion goes all the 
            //	way or not at all.
            OdbcTransaction oTrans = oConn.Connection.BeginTransaction();
            OdbcCommand oCommand = new OdbcCommand(strCommand, oConn.Connection);
            try
            {
                if (depth >= 1 && nDepth >= 1)
                {
                    oCommand.ExecuteNonQuery();

                    if (depth >= 2 && nDepth >= 2)
                    {
                        for (int nInd = 0; nInd < numants; nInd++)
                        {
                            oCommand.CommandText = ((ftAntenna)Antennas[nInd]).InsertCommand(sTable);
                            oCommand.ExecuteNonQuery();
                        }

                        if (depth >= 3 && nDepth >= 3)
                        {
                            for (int nInd = 0; nInd < numchan; nInd++)
                            {
                                oCommand.CommandText = ((ftChannel)Channels[nInd]).InsertCommand(sTable);
                                oCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    oTrans.Commit();
                }
            }
            catch (Exception e)
            {
                oTrans.Rollback();
                throw e;
            }

            oConn.dbdisconnect();
        }


        //	********************************************************************
        /// <summary>
        ///	This method will add the site to the table.  If the site is present
        ///	already and the IsReplace argument is true it will be deleted first, 
        ///	in either case the site will be added.
        ///
        ///	It will throw an exception if something goes wrong.
        /// </summary>
        public void Add(string sTable, bool IsReplace)
        {
            bool IsPresent;
            ftRadio tRadio = null;

            if (IsReplace)
            {
                DeleteSite(sTable);
            }

            try
            {
                tRadio = new ftRadio(this.call1, sTable, 3);
                IsPresent = true;
            }
            catch
            {
                IsPresent = false;
            }

            if (IsPresent)
            {
                //	Merge the radio found in the db with the current one.
                //	This only involves adding antennas and channels that are
                //	not present.  The site of the current radio takes precedence.
                for (int nInd = 0; nInd < tRadio.numants; nInd++)
                {
                    ftAddAnt((ftAntenna)tRadio.Antennas[nInd], false); //	Don't overwrite.
                }
                //	Now the channels
                for (int nInd = 0; nInd < tRadio.numchan; nInd++)
                {
                    ftAddChn((ftChannel)tRadio.Channels[nInd], false); //	Don't overwrite.
                }
            }

            WriteSite(sTable, 3);
        }


        public XmlElement ftRadioXml(TsFile oFile)
        {
            XmlElement tRadNode = base.TsRadioXml(oFile);
            // Now start on the Antennas
            XmlElement tAntennas = oFile.CreateElement("antennas");

            for (int nInd = 0; nInd < numants; nInd++)
            {
                tAntennas.AppendChild(((ftAntenna)Antennas[nInd]).ftAntennaXml(oFile));
            }
            tRadNode.AppendChild(tAntennas);

            //	And now the channels
            XmlElement tChannels = oFile.CreateElement("channels");
            for (int nInd = 0; nInd < numchan; nInd++)
            {
                tChannels.AppendChild(((ftChannel)Channels[nInd]).ftChannelXml(oFile));
            }
            tRadNode.AppendChild(tChannels);

            return tRadNode;
        }

    }


    // ***********************************************************************
    /// <summary>
    /// This is the general connection class for web projects.  
    /// For web projects it expects the user's connection string to be in the 
    /// session variable 's_cnString' and uses the constructor with no args.
    /// 
    /// For standalone exe's it expects the connection string in an environment
    /// variable, and uses the constructor with the database name passed as an arg
    ///	
    ///	It will throw a System.Exception called "Connection" if something goes 
    ///	wrong.
    /// </summary>
    public class dbconnect
    {
        private OdbcConnection oConnection = null;
        private string sessConnStr = "";

        public string ConnectString
        {
            get { return sessConnStr; }
        }

        public OdbcConnection Connection
        {
            get { return oConnection; }
        }

        /*  USED ONLY BY NEW TREE
        public dbconnect(string dbname)
        {
            if(dbname == "web")
            {
                try
                {
                    sessConnStr = HttpContext.Current.Session["s_cnString"].ToString();
                }
                catch (System.Exception Ex)
                {
                    throw new Exception("*Error* Invalid session string: " + Ex.Message);
                }
            }
            else
            {
                // get connection string from environment variable of the form '<dbname>'
                sessConnStr = "DSN=test;trustedconnection=true";
                //try
                //{
                //    envconn = "odbc" + dbname;
                //    sessConnStr = Environment.GetEnvironmentVariable(envconn);
                //}
                //catch (System.Exception Ex)
                //{
                //    throw new Exception("*Error* Invalid environment string: " + Ex.Message);
                //}
            }

            if (oConnection == null)
            {
                // open connection
                try
                {
                    oConnection = new OdbcConnection(sessConnStr);
                    oConnection.Open();
                }
                catch (System.Exception Ex)
                {
                    oConnection = null;
                    throw new System.Exception("Connection", Ex);
                }
            }

        }*/
        // this is called if using non-default connection string
        public dbconnect(string cnstr)
        {
            if (oConnection == null)
            {
                // open connection
                try
                {
                    oConnection = new OdbcConnection(cnstr);
                    oConnection.Open();
                }
                catch (System.Exception Ex)
                {
                    oConnection = null;
                    throw new System.Exception("*Error* Invalid connection string: " + Ex.Message);
                }
            }

        }
        public dbconnect()
        {
            ////string cline = System.Environment.CommandLine;

            if (oConnection == null)
            {
               ////string webdrive = Environment.GetEnvironmentVariable("webdrive");

                // check if we are running in IIS context
                // in this case there should be an HttpContext
                
                try
                {
                    // get current http context
                    // if there is none, this will throw exception
                    HttpContext hcc = HttpContext.Current;
                    sessConnStr = hcc.Session["s_cnString"].ToString();
                }
                catch (Exception)  // no HttpContext found - assume running standalone program
                {
                    // try for test/bin or prod/bin
                    ////try
                    ////{
                        ////if (cline.ToLower().IndexOf("test\\bin") > 0)
                        ////{
                    string dbname = Environment.GetEnvironmentVariable("DBName");
                    sessConnStr = "DSN=" + dbname + ";trustedconnection=true";
                        ////}
                        ////if (cline.ToLower().IndexOf("prod\\bin") > 0)
                        ////{
                            ////sessConnStr = "DSN=fcsa;DATABASE=fcsa;trustedconnection=true";
                        ////}
                    ////}
                    ////catch (System.Exception Ex)  // no valid command line
                    ////{
                    ////    throw new Exception("*Error* Invalid session string1: " + Ex.Message);
                    ////}
                }

                // open connection
                try
                {
                    oConnection = new OdbcConnection(sessConnStr);
                    oConnection.Open();
                }
                catch (System.Exception Ex)
                {
                    oConnection = null;
                    throw new System.Exception("Connection", Ex);
                }
            }
        }
        
         public bool IsConnected()
        {
            return oConnection != null;
        }


        /// <summary>
        /// Disconnect the connection, set it to null.
        /// </summary>
        public void dbdisconnect()
        {
            if (oConnection != null)
            {
                oConnection.Close();
                oConnection = null;
            }
        }
            /// <summary>
            /// This query will return an odbcdatareader for a query on the connection
            /// </summary>
            /// <param name="cQuery">The string containing the query</param>
            /// <returns>An odbc datareader that can be scanned</returns>
            public OdbcDataReader odbcquery(string cQuery)
            {
                ////try
                ////{
                    OdbcCommand tCmd = new OdbcCommand(cQuery, oConnection);
                    OdbcDataReader tReader = tCmd.ExecuteReader();
                    return tReader;
                ////}
                ////catch (Exception Ex)
                ////{
                   ////throw new Exception("* odbcquery Error [" + Ex.Message +
                   ////                     "] executing command:\n" + cQuery);
                ////}
            }


            public DataTable retrieve(string cSQL)
            {
                //	Assume we are connected to a database
                try
                {
                    OdbcCommand tCmd = new OdbcCommand(cSQL, oConnection);
                    OdbcDataAdapter oDA = new OdbcDataAdapter(tCmd);
                    DataSet oDS = new DataSet();
                    oDA.Fill(oDS);
                    DataTable oDT = oDS.Tables[0];
                    return oDT;
                }
                catch (Exception ex)
                {
                    throw new Exception("Retrieval Error on: " + cSQL + "<br>[" + ex.Message + "]");
                }
            }


            /// <summary>
            /// Execute an non-retrieval command on the connection.  The number of 
            ///	rows affected, if this is relevant is returned.
            /// </summary>
            /// <param name="cSQL">A string with the command.</param>
            /// <returns>The number of rows affected or -1</returns>
            public int nonquery(string cSQL)
            {
                int nRows = -2;
                //	Assume we are connected to a database
                try
                {
                    OdbcCommand tCmd = new OdbcCommand(cSQL, oConnection);
                    nRows = tCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Non-Query Error on: " + cSQL + "<br>[" + ex.Message + "]");
                }
                return nRows;
            }


            /// <summary>
            /// Retrieve a single value from a query.  This is the first column in the 
            ///	first row returned.
            /// </summary>
            /// <param name="cSQL">The sql string to return the values</param>
            /// <returns>The first column of the first row as an object.</returns>
            public object getscalar(string cSQL)
            {
                DataTable oDT = retrieve(cSQL);
                return oDT.Rows[0].ItemArray[0];
            }


            public int execintproc(string cProcCall)
            {
                int nRet = 0;

                try
                {
                    OdbcCommand getint = new OdbcCommand("{ ? = CALL " + cProcCall + " }", oConnection);
                    getint.CommandType = CommandType.StoredProcedure;

                    OdbcParameter getParm = getint.Parameters.Add("RETURN_VALUE", OdbcType.Int);
                    getParm.Direction = ParameterDirection.ReturnValue;

                    getint.ExecuteScalar();

                    nRet = Convert.ToInt32(getint.Parameters["RETURN_VALUE"].Value);
                }
                catch (Exception Ex)
                {
                    throw new Exception("*Error* Procedure call: [" + cProcCall + "]:-\n" +
                                        Ex.Message);
                    // nRet = -1;
                }

                return nRet;
            }


            /// <summary>
            /// This method will check the database to see if a user's table exists.  It does this
            ///	by checking for it in the iitables system table.  This is database dependent,
            ///	but documented.	It expects the connection to be open.
            /// </summary>
            /// <param name="cTable">The full name of the table we are checking for.</param>
            /// <returns>true if the table exists, false otherwise</returns>
            public bool tableexists(string cTable)
            {
                bool IsPresent;
                int nCount;

                string cSQL = "SELECT count(*) " +
                              "from INFORMATION_SCHEMA.TABLES " +
                              "where table_name='" + cTable.ToLower() +
                                     "' and table_schema='" + HttpContext.Current.Session["s_schema"].ToString() +
                                     "'";
                try
                {
                    OdbcCommand oCmd = new OdbcCommand(cSQL, oConnection);
                    nCount = Convert.ToInt32(oCmd.ExecuteScalar());

                }
                catch
                {
                    nCount = 0;
                }

                IsPresent = nCount > 0;

                return IsPresent;
            }
        }


        /// <summary>
        ///	***********************************************************************************
        /// This class is copied from the ASP file OELSupport.inc.  Many of the routines are no
        ///	longer necessary.
        /// </summary>
        public class OELSupport
        {

            // <meta NAME="Author" content="Greg Shannan, Orthogonal Endeavours Ltd - 2000 08">
            // <meta name="Copyright" 
            //  		content="Frequency Coordination System Association, Ottawa, 2000">

            //	Return this value to anyone.  This function may not be necessary
            public static bool IsDevel()
            {
                string cLocalDir = HttpContext.Current.Server.MapPath(".");
                //	********************************************************************************* //
                //	Note that the following line must be changed if the directory name of the 
                //	development directory is changed.                   
                //	********************************************************************************* //
                int nInd = cLocalDir.IndexOf("dev");

                return (nInd < 0 ? false : true);
            }


            /// <summary>
            /// Return the mics userid of the currently logged on user.
            /// </summary>
            /// <returns>String with mics userid</returns>
            public static string ACusername()
            {
                string sMicsid;

                if (HttpContext.Current.Session["s_user"] != null)
                {
                    sMicsid = HttpContext.Current.Session["s_user"].ToString();
                }
                else
                {
                    //	Session has timed out.
                    timeout("UserName");
                    sMicsid = "";
                }
                return (sMicsid);
            }


            /// <summary>
            /// Retrieve the schema of the currently logged on user
            /// </summary>
            /// <returns>String with schema</returns>
            public static string ACunixid()
            {
                string sSchema;

                if (HttpContext.Current.Session["s_schema"] != null)
                {
                    sSchema = HttpContext.Current.Session["s_schema"].ToString();
                }
                else
                {
                    //	Session has timed out.
                    timeout("Schema");
                    sSchema = "";
                }
                return (sSchema);
            }


        /// <summary>
        /// Returns true if the parameter, a string holding a schema name, is
        ///	an FCSA distinquished string, false otherwise.
        /// </summary>
        /// <param name="sSchema">A string holding the persons schema.</param>
        /// <returns>True if the id is FCSA</returns>
        public static bool IsFCSA(string sSchema)
            {
                string cTSchema = sSchema.Trim().ToLower();

                return cTSchema == "hulme" ||
                       cTSchema == "fmda2" ||
                       cTSchema == "foad" ||
                       cTSchema == "frse" ||
                       cTSchema == "venn";
            }


            /// <summary>
            /// Get the current database name
            /// </summary>
            /// <returns>The current database name</returns>
            public static string ACdbname()
            {
                string db;

                if ((db = HttpContext.Current.Session["db_name"].ToString()) == null)
                {
                    if (IsDevel())
                    {
                        db = "test";
                    }
                    else
                    {	//	Session has timed out.
                        timeout("DBName");
                        db = "";
                    }
                }
                return db;
            }


            /// <summary>
            /// Retrieve the current Project code
            /// </summary>
            /// <returns>The current project code</returns>
            public static string ACPcode()
            {
                string cPcode;

                if (IsDevel() || HttpContext.Current.Session["defProject"] == null)
                {
                    cPcode = "PCODE1";
                }
                else
                {
                    cPcode = HttpContext.Current.Session["defProject"].ToString();
                }

                return cPcode;
            }


            /// <summary>
            /// Return the current connection string for the odbc driver
            /// </summary>
            /// <returns>The current connection string or ""</returns>
            public static string ACconnstring()
            {
                string cConnStr;
                string cDB = ACdbname();

                if (HttpContext.Current.Session["s_cnString"] != null)
                {
                    cConnStr = HttpContext.Current.Session["s_cnString"].ToString();
                }
                else
                {
                    if (IsDevel())
                    {
                        cConnStr = "DSN=micsWeb;SERVER=FCSATEST;DATABASE=" + cDB +
                                            ";SERVERTYPE=INGRES;UID=fds3;PWD=fds3333";
                    }
                    else
                    {
                        cConnStr = "DSN=micsWeb;SERVER=FCSATEST;DATABASE=test;" +
                                            "SERVERTYPE=INGRES;UID=fds3;PWD=fds3333";
                    }
                }
                return cConnStr;
            }


            /// <summary>
            /// This method returns a session sub-iterator
            /// </summary>
            /// <returns>String with the next number in it.</returns>                       
            public static string nextit()
            {
                //	Get the next value of the session counter.
                int nCount;
                try
                {
                    if (HttpContext.Current.Session["nSessionCounter"].ToString() != "")
                    {
                        nCount = Convert.ToInt32(HttpContext.Current.Session["nSessionCounter"].ToString());
                        nCount++;
                        HttpContext.Current.Session["nSessionCounter"] = nCount.ToString();
                    }
                    else
                    {
                        nCount = 1;
                        HttpContext.Current.Session["nSessionCounter"] = 1;
                    }
                }
                catch
                {
                    nCount = 1;
                    HttpContext.Current.Session["nSessionCounter"] = 1;
                }
                return nCount.ToString();
            }



            /// <summary>
            /// This method returns the current session id, or a random number if not found
            /// </summary>
            /// <returns>String with the next number in it.</returns>                       
            public static string sessionid()
            {
                //	Get the value of the session id.
                int nCount = 1;
                try
                {
                    if (HttpContext.Current.Session["FCSASESS"].ToString() != "")
                    {
                        nCount = Convert.ToInt32(HttpContext.Current.Session["FCSASESS"].ToString());
                    }
                }
                catch
                {
                    Random oRand = new Random();		//	Use the current time.
                    HttpContext.Current.Session["FCSASESS"] =
                    nCount = oRand.Next(99999);
                }
                return nCount.ToString();
            }


            /// <summary>
            /// This performs the timeout function from the server.
            /// </summary>
            public static void timeout(string cReason)
            {
                HttpContext.Current.Response.Redirect("/mics/SessionTimeOut.aspx?r=" + cReason, true);
            }


            /// <summary>
            /// This will check if we still have a session open, and go to the timeout page if not.
            /// </summary>
            public static void checksession()
            {
                int nCount = 0;
                string cSite = "";

                try
                {
                    nCount = HttpContext.Current.Session.Count;
                    cSite = HttpContext.Current.Session["SiteName"].ToString();
                }
                catch
                {
                    //	Go to the logout screen.
                    timeout("ErrSessionCount");
                }
                if (nCount == 0 || cSite == null)
                {
                    //	Go to the logout screen.
                    timeout("NoSessionVariables");
                }
                return;
            }


            /// <summary>
            /// This will check if we still have a session open, and go to the timeout page if not.
            /// It is only called by forms that are called by window.open
            /// </summary>
            public static bool checksessionclosed()
            {
                int nCount = 0;
                string cSite = "";

                try
                {
                    nCount = HttpContext.Current.Session.Count;
                    cSite = HttpContext.Current.Session["SiteName"].ToString();
                    return false;
                }
                catch
                {
                    //	Go to the logout screen.
                    return true;
                }                
            }
            public static bool checkauthexpired()
            {
                bool test = false;
                // skip tests for standard entry points
                //if (Request.Url.AbsoluteUri.IndexOf("login.aspx") >= 0) test = false;

                if (test)
                {
                    if (HttpContext.Current.User != null)
                    {
                        if (HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            if (HttpContext.Current.User.Identity is FormsIdentity)
                            {
                                // get forms identity from current user
                                FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
                                // get forms ticket from Identity object
                                FormsAuthenticationTicket fat = id.Ticket;
                                if (fat.Expired)
                                {
                                    test = true;
                                }
                            }
                        }
                    }
                }
                return test;
            }
            public static int checktimeout()
            {
                int retval = 0;
                if (checksessionclosed())
                {
                    retval += 1;
                }
                if (checkauthexpired())
                {
                    retval += 2;
                }
                return retval;

            }
            // *****************************************************************************
            //
            //		Standard Date
            //
            // *****************************************************************************
            public static string standarddate()
            {
                DateTime dToday = DateTime.Today;
                string cOutDate = dToday.ToString("yyyy.MM.dd");

                return cOutDate;
            }


            // *****************************************************************************
            //
            //	Get the http header for the current directory
            public static string urlhead()
            {
                return baseurl() + "mics/";
            }


            // *****************************************************************************
            //
            //	Get the base url for the current server
            public static string baseurl()
            {
                string cPath = HttpContext.Current.Server.MapPath(".");
                string cURL;

                if (cPath.IndexOf("remicsdev") >= 0)
                {
                    cURL = HttpContext.Current.Application["DevUrl"].ToString();
                }
                else if (cPath.IndexOf("micstest") >= 0)
                {
                    cURL = HttpContext.Current.Application["TestUrl"].ToString();
                }
                else
                {
                    cURL = HttpContext.Current.Application["ProdUrl"].ToString();
                }

                return cURL;
            }


            // *****************************************************************************
            //
            //		Return the file extension for text files that are returned.  The user
            //		may eventually need to change the extension, because IE will first check
            //		to see if there is a program associated with it before just displaying the 
            //		file.
            public static string ACfileext()
            {
                string cExt;

                try
                {
                    cExt = HttpContext.Current.Session["s_file_ext"].ToString();
                }
                catch
                {
                    cExt = ".prn";
                }

                return cExt;
            }


            /// <summary>
            /// Return the user's directory on the Window's Server, accessible from the 
            ///	server software.
            /// </summary>
            /// <returns>string representing path</returns>
            public static string getusersdir()
            {
                //return HttpContext.Current.Application["web_drive"].ToString() +
                //       "\\Inetpub\\mics\\userdirs\\" +
                //       OELSupport.ACunixid() + "\\" +
                //       OELSupport.ACusername() + "\\";

                // above code replaced May 6, 2014 as above did not include micsdev/micstest/wwwroot option
                return HttpContext.Current.Session["user_dir"].ToString();
            }


            /// <summary>
            /// Return the web address of the user's directory on the Window's Server accessable
            ///	from the client.
            /// </summary>
            /// <returns>string representing path</returns>
            public static string getuserswebdir()
            {
                return baseurl() + "mics/userdirs/" +
                       OELSupport.ACunixid() + "/" +
                       OELSupport.ACusername() + "/";
            }


            /// <summary>
            /// Return the web address of the user's directory on the Alpha as seen from
            ///	the server.
            /// </summary>
            /// <returns>string representing path</returns>
            public static string getusersalphadir()
            {
                //return "g:\\" + 
                // above replaced by BA 2006/2/20
                return HttpContext.Current.Application["unix_drive"].ToString() + "\\" +
                       OELSupport.ACunixid() + "\\" +
                       OELSupport.ACusername() + "\\";
            }

            /// <summary>
            /// Copy a .prn from the G: drive (on the Alpha using NFS) to the user's
            ///	local directory.  We do this because at this time, we can access the Alpha
            ///	from an IIS program, but not from a web browser.
            ///	Copied from Bill Ableson's version. 2004 09 24 GJS
            /// </summary>
            /// <param name="serial">The base name of the file to be copied</param>
            /// <param name="newname">The name of the file to be copied to.</param>
            /// <returns>True if success</returns>
            public static bool copy_alpha(string cName, string newname)
            {
                //string unixprn = "g:" +
                // above replaced by BA 2006/2/20
                string unixprn = HttpContext.Current.Application["unix_drive"].ToString() +
                                                 "\\" + OELSupport.ACunixid() + "\\" + OELSupport.ACusername() +
                                 "\\" + cName;
                if (!File.Exists(unixprn))
                {
                    return false;
                }

                //checkDir("c:\\Inetpub\\userdirs\\" + OELSupport.ACunixid());
                // above replaced by BA 2006/2/20
                checkDir(HttpContext.Current.Application["web_drive"].ToString() +
                          "\\Inetpub\\mics\\userdirs\\" + OELSupport.ACunixid());
                //checkDir("c:\\Inetpub\\userdirs\\" + OELSupport.ACunixid() + "\\" + 
                // above replaced by BA 2006/2/20
                checkDir(HttpContext.Current.Application["web_drive"].ToString() +
                          "\\Inetpub\\mics\\userdirs\\" + OELSupport.ACunixid() + "\\" +
                          OELSupport.ACusername());
                string wincstxt = getusersdir() + newname;

                // delete target file if present
                if (File.Exists(wincstxt))
                {
                    File.Delete(wincstxt);
                }

                // move unix .prn to windows .txt file for browser display

                File.Copy(unixprn, wincstxt);

                return true;
            }


            /// <summary>
            /// Copy a file from the user's local directory to an html file in the same directory.
            /// The file itself is enclosed in <pre></pre>
            ///	tags.  It expects, but doesn't check that the filename ends in .htm
            ///	2011 06 14 Bill A
            /// </summary>
            /// <param name="serial">The base name of the file to be copied</param>
            /// <param name="newname">The name of the file to be copied to.</param>
            /// <returns>True if success, false if the file doesn't exist.</returns>
            public static bool copy_html(string cName, string newname)
            {
                string sourcefile = HttpContext.Current.Session["user_dir"].ToString() + cName;
                if (!File.Exists(sourcefile))
                {
                    return false;
                }

                string targetfile = HttpContext.Current.Session["user_dir"].ToString() + newname;

                // delete target file if present
                if (File.Exists(targetfile))
                {
                    File.Delete(targetfile);
                }

                // Copy file.  Because we bracket contents with html headers and trailers, 
                // we read and copy file line by line.
                StreamReader oSR = new StreamReader(sourcefile);
                StreamWriter oSW = new StreamWriter(targetfile);

                oSW.WriteLine("<html><head><title>File " + cName + "</title></head><body><pre>");
                string sLine;
                while ((sLine = oSR.ReadLine()) != null)
                {
                    oSW.WriteLine(sLine);
                }
                oSW.WriteLine("</pre></body></html>");

                oSR.Close();
                oSW.Close();


                return true;
            }


            /// <summary>
            /// Copy a file from the user's local directory on the server to their directory
            /// on the Alpha.  
            /// </summary>
            /// <param name="serial">The base name of the file to be copied</param>
            /// <param name="newname">The name of the file to be copied to.</param>
            /// <returns>True if success</returns>
            public static bool copy_server(string cName, string newname)
            {
                //string unixprn = "g:" + 
                string unixprn = HttpContext.Current.Application["unix_drive"].ToString() +
                                         "\\" + OELSupport.ACunixid() + "\\" + OELSupport.ACusername() +
                                 "\\" + cName;
                if (!File.Exists(unixprn))
                {
                    return false;
                }

                //checkDir("c:\\Inetpub\\userdirs\\" + OELSupport.ACunixid());
                // above replaced by BA 2006/2/20
                checkDir(HttpContext.Current.Application["web_drive"].ToString() +
                        "\\Inetpub\\userdirs\\" + OELSupport.ACunixid());
                //checkDir("c:\\Inetpub\\userdirs\\" + OELSupport.ACunixid() + "\\" + 
                // above replaced by BA 2006/2/20
                checkDir(HttpContext.Current.Application["web_drive"].ToString() +
                        "\\Inetpub\\userdirs\\" + OELSupport.ACunixid() + "\\" +
                        OELSupport.ACusername());
                string wincstxt = getusersdir() + newname;

                // delete target file if present
                if (File.Exists(unixprn))
                {
                    File.Delete(unixprn);
                }

                // move windows file to unix.  When doing this we must remove the Carriage Returns.
                StreamReader oSR = new StreamReader(wincstxt);
                StreamWriter oSW = new StreamWriter(unixprn);

                //	We read a line in normally (which does not return the ending characters), and
                //	then write out the line with the unix eol character.
                string sFile;
                while ((sFile = oSR.ReadLine()) != null)
                {
                    oSW.Write(sFile + '\n');
                }

                oSR.Close();
                oSW.Close();

                return true;
            }


            public static void checkDir(string dirname)
            {
                if (!Directory.Exists(dirname))
                {
                    Directory.CreateDirectory(dirname);
                }
            }


            /// <summary>
            /// Makes all single backslashes double.
            /// </summary>
            /// <param name="inPath">Input string with backslashes that must be
            /// doubled.</param>
            /// <returns>string representing the input string with backslashes doubled.</returns>
            public static string doubleslash(string inPath)
            {
                string[] sParts = inPath.Split('\\');
                StringBuilder sb = new StringBuilder();
                string sPrev = "";

                foreach (string sPart in sParts)
                {
                    sb.Append(sPrev + sPart);
                    sPrev = "\\\\";
                }

                return sb.ToString();
            }


            /// <summary>
            /// Doubles all the single quotes in a string so that it can be stored in the database.
            /// </summary>
            /// <param name="inStr">The input string with the quotes</param>
            /// <returns>The output string with the quotes doubled</returns>
            public static string doublequote(string inStr)
            {
                string cOut = "";

                for (int nInd = 0; nInd < inStr.Length; nInd++)
                {
                    switch (inStr.Substring(nInd, 1))
                    {
                        case "'":
                            cOut += "''";
                            break;

                        default:
                            cOut += inStr.Substring(nInd, 1);
                            break;
                    }
                }

                return cOut;
            }


            /// <summary>
            /// Places a backslash in front of single quotes('), double quotes("), and backslashes(\) in a
            /// string.
            /// </summary>
            /// <param name="inStr">The input string.</param>
            /// <returns>The output string with the characters escaped.</returns>
            public static string escapestr(string inStr)
            {
                string cOut = "";

                for (int nInd = 0; nInd < inStr.Length; nInd++)
                {
                    switch (inStr.Substring(nInd, 1))
                    {
                        case "\\":
                        case "'":
                        case "\"":
                            cOut += "\\" + inStr.Substring(nInd, 1);
                            break;

                        case "\n":
                            //	We replace line feeds
                            cOut += "\\n";
                            break;

                        case "\r":
                            //	Ignore Carriage returns
                            break;

                        default:
                            cOut += inStr.Substring(nInd, 1);
                            break;
                    }
                }

                return cOut;
            }


            /// <summary>
            /// This routine allows some basic attributes to be entered preceding the string
            ///	that is to be placed in the table cell.  Any attributes (and they are optional)
            ///	prefix the string and are surrounded by colons(:).  Two colons in a row are 
            ///	treated as one.
            ///
            ///	The two attributes are:- 
            ///		A for alignment, and the following character must be 
            ///			L - Left
            ///			R - Right
            ///			C	-	Centre
            ///
            ///		S for colspan, and the following is a number indicating the number of columns
            ///			to span.
            ///
            /// </summary>
            /// <param name="cEntry">The entry with the prefix if any.</param>
            /// <param name="cAlign">The output alignment of "Left", "Right",... or blank if none.</param>
            /// <param name="nSpan">An integer indicating the number of columns to span or 0 for none.</param>
            /// <returns>The entry with the prefix removed.</returns>
            private static string breakoutattr(string cEntry, out string cAlign, out int nSpan)
            {
                cAlign = "";
                nSpan = 0;

                if (cEntry.StartsWith(":"))
                {
                    //	We have some attributes entered.
                    cEntry = cEntry.Substring(1);		//	Drop the first character
                    int nInd = cEntry.IndexOf(":");
                    if (nInd > 0)
                    {
                        string cAttrs = cEntry.Substring(0, nInd);  // Get the attributes
                        cEntry = cEntry.Substring(nInd + 1);
                        while (cAttrs.Length > 0)
                        {
                            if (cAttrs.StartsWith("A"))
                            {
                                //	Alignment, needs to be followed by L, R, or C
                                switch (cAttrs.Substring(1, 1))
                                {
                                    case "L":
                                        cAlign = "Left";
                                        break;

                                    case "R":
                                        cAlign = "Right";
                                        break;

                                    case "C":
                                        cAlign = "Center";
                                        break;

                                    default:
                                        // Do nothing.
                                        break;
                                }
                                cAttrs = cAttrs.Substring(2);

                            }
                            else if (cAttrs.StartsWith("S"))
                            {
                                //	Colspan.  Needs to be followed by a numeric.
                                int nColSpan = 0;
                                int nOff = 0;
                                cAttrs = cAttrs.Substring(1);
                                while (cAttrs.Length > 0)
                                {
                                    string cChar = cAttrs.Substring(0, 1);
                                    if ((nOff = "0123456789".IndexOf(cChar)) > -1)
                                    {
                                        //	It is a digit
                                        nColSpan *= 10;
                                        nColSpan += nOff;
                                        cAttrs = cAttrs.Substring(1);		//	Go to the next.
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (nColSpan > 1)
                                {
                                    nSpan = nColSpan;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return cEntry;
            }


            public static HtmlTableRow addrow(HtmlTable oT, params string[] acEntries)
            {
                string cAlign = "";
                int nSpan = 0;

                HtmlTableRow oTR = new HtmlTableRow();

                foreach (string cEntry in acEntries)
                {
                    HtmlTableCell oTC = new HtmlTableCell();
                    string cNewEnt = breakoutattr(cEntry, out cAlign, out nSpan);
                    if (cAlign.Length > 0)
                    {
                        oTC.Align = cAlign;
                    }
                    if (nSpan > 1)
                    {
                        oTC.ColSpan = nSpan;
                    }
                    oTC.InnerHtml = cNewEnt;
                    oTR.Cells.Add(oTC);
                }
                oT.Rows.Add(oTR);
                return oTR;
            }


            // Overload
            public static TableRow addrow(Table oT, params string[] acEntries)
            {
                string cAlign = "";
                int nSpan = 0;

                TableRow oTR = new TableRow();
                foreach (string cEntry in acEntries)
                {
                    TableCell oTC = new TableCell();
                    string cNewEnt = breakoutattr(cEntry, out cAlign, out nSpan);
                    if (cAlign.Length > 0)
                    {
                        oTC.Attributes.Add("Align", cAlign);
                    }
                    if (nSpan > 1)
                    {
                        oTC.Attributes.Add("Colspan", nSpan.ToString());
                    }
                    oTC.Text = cNewEnt;
                    oTR.Cells.Add(oTC);
                }
                oT.Rows.Add(oTR);
                return oTR;
            }


            public static HtmlTableRow addtitle(HtmlTable oT, params string[] acEntries)
            {
                string cAlign = "";
                int nSpan = 0;

                HtmlTableRow oTR = new HtmlTableRow();
                foreach (string cEntry in acEntries)
                {
                    HtmlTableCell oTC = new HtmlTableCell();
                    string cNewEnt = breakoutattr(cEntry, out cAlign, out nSpan);
                    if (cAlign.Length > 0)
                    {
                        oTC.Align = cAlign;
                    }
                    if (nSpan > 1)
                    {
                        oTC.ColSpan = nSpan;
                    }
                    oTC.VAlign = "Top";
                    oTC.InnerHtml = "<b>" + cNewEnt + "</b>";
                    oTR.Cells.Add(oTC);
                }
                oT.Rows.Add(oTR);
                return oTR;
            }


            // Overload
            public static TableRow addtitle(Table oT, params string[] acEntries)
            {
                string cAlign = "";
                int nSpan = 0;

                TableRow oTR = new TableRow();
                foreach (string cEntry in acEntries)
                {
                    TableCell oTC = new TableCell();
                    string cNewEnt = breakoutattr(cEntry, out cAlign, out nSpan);
                    if (cAlign.Length > 0)
                    {
                        oTC.Attributes.Add("Align", cAlign);
                    }
                    if (nSpan > 1)
                    {
                        oTC.Attributes.Add("Colspan", nSpan.ToString());
                    }
                    oTC.VerticalAlign = VerticalAlign.Top;
                    oTC.Text = "<b>" + cNewEnt.Trim() + "</b>";
                    oTR.Cells.Add(oTC);
                }
                oT.Rows.Add(oTR);
                return oTR;
            }


            //	This just encapsulates the power calculation formula used in validate
            public static double prx(double dPtx,
                                                             double dGainTx,
                                                             double dFslTx,
                                                             double dFreqMHz,
                                                             double dDistKm,
                                                             double dGainRx,
                                                             double dFslRx)
            {
                return dPtx + dGainTx - dFslTx - 32.45 - 20.0 * Math.Log10(dFreqMHz) -
                       20.0 * Math.Log10(dDistKm) + dGainRx - dFslRx;
            }


            //	This just encapsultes the power calculation formula used in validate without the
            //	frequency component that changes from channel to channel.
            public static double prxnofreq(double dPtx,
                                                                         double dGainTx,
                                                                         double dFslTx,
                                                                   double dDistKm,
                                                                   double dGainRx,
                                                                   double dFslRx)
            {
                return dPtx + dGainTx - dFslTx - 32.45 -
                       20.0 * Math.Log10(dDistKm) + dGainRx - dFslRx;
            }

            //	This just encapsultes the power calculation formula used in validate using the
            //	precalculated rx power without the frequency component and then adds in the 
            //	frequency component.
            public static double prxfreq(double dPrxnoFreq,
                                                                     double dFreqMHz)
            {
                return dPrxnoFreq - 20.0 * Math.Log10(dFreqMHz);
            }

            /// <summary>
            /// Returns the maximum of the list of arguments.
            /// </summary>
            /// <param name="dVals">A list of doubles</param>
            /// <returns>The maximum value of the input list.</returns>
            public static double MaxOf(params double[] dVals)
            {
                double dMax = dVals[0];

                for (int nInd = 1; nInd < dVals.Length; nInd++)
                {
                    if (dVals[nInd] > dMax)
                    {
                        dMax = dVals[nInd];
                    }
                }
                return dMax;
            }


            /// <summary>
            /// Returns the minimum of the list of arguments.
            /// </summary>
            /// <param name="dVals">A list of doubles</param>
            /// <returns>The minimum value of the input list.</returns>
            public static double MinOf(params double[] dVals)
            {
                double dMax = dVals[0];

                for (int nInd = 1; nInd < dVals.Length; nInd++)
                {
                    if (dVals[nInd] < dMax)
                    {
                        dMax = dVals[nInd];
                    }
                }
                return dMax;
            }


            private static string getnext(string cLine, int nStart, out int nEnd)
            {
                int nLen = cLine.Length;
                string cComp = "";
                string cOut = "";

                nEnd = nStart;
                //	Scan over blanks
                while (nEnd < nLen && cLine.Substring(nEnd, 1) == " ")
                {
                    nEnd++;
                }

                if (nEnd >= nLen)
                {
                    return null;
                }
                else if (cLine.Substring(nEnd, 1) == "\"")
                {
                    cComp = "\"";
                    nEnd++;
                }
                else
                {
                    cComp = ",";
                }

                while (nEnd < nLen)
                {
                    if (cComp == "\"")
                    {
                        //	Only the quote presents a problem
                        if (cLine.Substring(nEnd, 1) == "\"")
                        {
                            nEnd++;  // If we hit the last quote we skip over it and exit
                            break;
                        }
                        else if (cLine.Substring(nEnd, 1) == "\\")
                        {
                            nEnd++;	//	If we have a \, then use the next character
                            if (cLine.Substring(nEnd, 1) == "n")
                            {
                                //	It's a new line.  Put one in.
                                cOut += "\n";
                            }
                            else
                            {
                                //	Anything else, just escape it.
                                cOut += cLine.Substring(nEnd, 1);
                            }
                        }
                        else
                        {
                            cOut += cLine.Substring(nEnd, 1);
                        }
                    }
                    else
                    {
                        if (cLine.Substring(nEnd, 1) == ",")
                        {
                            break; // a comma just exits.
                        }
                        else
                        {
                            cOut += cLine.Substring(nEnd, 1);
                        }
                    }
                    nEnd++;
                }

                while (nEnd < nLen && cLine.Substring(nEnd, 1) != ",")
                {
                    nEnd++;
                }

                //	nEnd now points to the ending comma.

                return cOut;
            }


            public static string[] breakoutcsv(string cLine)
            {
                ArrayList al = new ArrayList();
                string cNext = "";
                int nStart = 0;
                int nEnd = 0;

                if (cLine == null || cLine.Length == 0)
                {
                    return null;
                }

                while ((cNext = getnext(cLine, nStart, out nEnd)) != null)
                {
                    al.Add(cNext);
                    nStart = nEnd + 1;
                }

                return (string[])al.ToArray(Type.GetType("System.String"));
            }
        }


        public class logentry
        {
            public static BitArray outcond = new BitArray(32, true);
            protected static StreamWriter oSW = null;
            protected bool thiscond = false;
            protected int nCond = 0;

            public logentry(int cond)
            {
                nCond = cond;
                thiscond = outcond[nCond];

                if (thiscond)
                {
                    if (oSW == null)
                    {
                        //	First time, open the writer
                        string cFile = OELSupport.getusersalphadir() + "sesslog.txt";
                        oSW = new StreamWriter(cFile, true);
                        oSW.AutoFlush = true;
                        oSW.WriteLine("\n\nLogentry starts at " + DateTime.Now.ToString());
                    }
                }
            }


            public void write(string cOut)
            {
                if (thiscond)
                {
                    oSW.WriteLine(nCond.ToString() + ":" + cOut);
                }
            }


            public static void setcond(int nCond, bool IsValue)
            {
                outcond[nCond] = IsValue;		//	set the conditions.
            }

        }

    }

