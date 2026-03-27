using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;
using System.Security.Principal;

using Telerik.Web.UI;
using DBUtilities;
using ErrorUtilities;

namespace Tbulkprint
{
    /// <summary>
    /// Summary description for TwsBulkTree
    /// </summary>
        [WebService(Namespace = "https://mics.fcsa.ca/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class TwsBulkTree : System.Web.Services.WebService
    {
            public TwsBulkTree()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion


        private string cnstr = "";

        private bool getconnstring()
        {
            try
            {
                cnstr = Session["s_cnString"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                cnstr = "timeout" + ex.Message;
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        [WebMethod(EnableSession = true)]
        public RadTreeNodeData[] expandNodeTSBulkInit(RadTreeNodeData node, object context)
        {
            // This function is called only by TSPrintTree to load initial pdf list
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return null;
            }

            string callingvalue = node.Value;
            string callingtext = node.Text;

            char[] delimiter = ".".ToCharArray();
            string[] keyparts = callingvalue.Split(delimiter);

            List<RadTreeNodeData> result = new List<RadTreeNodeData>();

            switch (keyparts[0])
            {
                case "root":    // add boilerplate nodes
                    RadTreeNodeData hnodeData = new RadTreeNodeData();
                    hnodeData.Text = "HELP";
                    hnodeData.Value = "Help";
                    result.Add(hnodeData);

                    get_TSpdf_nodes(result);
                    return result.ToArray();
            }
            return null;
        }
        [WebMethod(EnableSession = true)]
        public RadTreeNodeData[] expandNodeTSBulkMain(RadTreeNodeData node, object context)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return null;
            }

            string callingvalue = node.Value;
            string callingtext = node.Text;

            char[] delimiter = ".".ToCharArray();
            string[] keyparts = callingvalue.Split(delimiter);

            List<RadTreeNodeData> result = new List<RadTreeNodeData>();

            switch (keyparts[0])
            {
                case "e":
                    RadTreeNodeData tnodeData = new RadTreeNodeData();
                    tnodeData.Text = "Title";
                    tnodeData.Value = "t." + keyparts[1];
                    result.Add(tnodeData);

                    RadTreeNodeData gnodeData = new RadTreeNodeData();
                    gnodeData.Text = "<b>Change of Call Sign</b>";
                    gnodeData.Value = "l." + keyparts[1];
                    gnodeData.ExpandMode = TreeNodeExpandMode.WebService;
                    result.Add(gnodeData);

                    RadTreeNodeData inodeData = new RadTreeNodeData();
                    inodeData.Text = "<b>Sites</b>";
                    inodeData.Value = "i." + keyparts[1];
                    inodeData.ExpandMode = TreeNodeExpandMode.WebService;
                    result.Add(inodeData);

                    return result.ToArray();

                case "l":   // change of call signs
                    get_TSchangecall_nodes(node, result);
                    return result.ToArray();

                case "i":   // sites top node
                    fix_missing_sites(keyparts[1]);
                    get_TSsite_headers(node, result);
                    return result.ToArray();

                case "s":   // specific site
                    get_TSsite_details(node, result);
                    return result.ToArray();

                case "k":   // link
                    get_TSlink_details(node, result);
                    return result.ToArray();

                case "b":   // antennas
                    get_TSante_details(node, result);
                    return result.ToArray();

                case "h":   // channels
                    get_TSchan_details(node, result);
                    return result.ToArray();
            }
            return null;
        }
        private void get_TSpdf_nodes(List<RadTreeNodeData> result)
        {
            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    string tablename;
                    string filename;

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();

                        string schema = Session["s_schema"].ToString();
                        string strSql = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = '" +
                                    schema + "' AND table_name LIKE 'ft\\_%\\_titl%' ESCAPE '\\' order by table_name";

                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr = select1.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        tablename = DBUtils.GetDBString(dr, 0);
                                        filename = tablename.Substring(3, tablename.Length - 8); // strip ft_ and _titl
                                        RadTreeNodeData nodeData = new RadTreeNodeData();
                                        nodeData.Text = filename;
                                        nodeData.Value = "e." + filename;
                                        //nodeData.ExpandMode = TreeNodeExpandMode.WebService;
                                        result.Add(nodeData);
                                    }
                                }
                                else
                                {
                                    //return null
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSpdf_nodes");
                //return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_TSlink_details(RadTreeNodeData node, List<RadTreeNodeData> result)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {           // split key into parts 
                    char[] delimiter = ".".ToCharArray();
                    string[] keyparts = node.Value.Split(delimiter);
                    string pdfName = keyparts[1];

                    // add Antennas top node
                    RadTreeNodeData bnodeData = new RadTreeNodeData();
                    bnodeData.Text = "<b>Antennas</b>";
                    bnodeData.Value = "b." + pdfName + "." + keyparts[2].ToUpper() + "." + keyparts[3].ToUpper() + "." + keyparts[4].ToUpper();
                    bnodeData.ExpandMode = TreeNodeExpandMode.WebService;
                    result.Add(bnodeData);

                    // add Channels top node
                    RadTreeNodeData hnodeData = new RadTreeNodeData();
                    hnodeData.Text = "<b>Channels</b>";
                    hnodeData.Value = "h." + pdfName + "." + keyparts[2].ToUpper() + "." + keyparts[3].ToUpper() + "." + keyparts[4].ToUpper();
                    hnodeData.ExpandMode = TreeNodeExpandMode.WebService;
                    result.Add(hnodeData);

                    return "OK";
                }

            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSlink_details");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_TSchangecall_nodes(RadTreeNodeData node, List<RadTreeNodeData> result)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // split key into parts 
            char[] delimiter = ".".ToCharArray();
            string[] keyparts = node.Value.Split(delimiter);
            string pdfName = keyparts[1];

            //Table Names
            string schema = Session["s_schema"].ToString();
            string chngTable = schema + ".ft_" + pdfName + "_chng";

            //Retrieve Change of Call Signs
            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        string strSql = "SELECT newcall1, oldcall1 FROM " + chngTable;

                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {
                                        RadTreeNodeData nodeData = new RadTreeNodeData();
                                        nodeData.Text = DBUtils.GetDBString(dr2, 1) + " Changed To " + DBUtils.GetDBString(dr2, 0);
                                        nodeData.Value = "g." + pdfName + "." + DBUtils.GetDBString(dr2, 1).ToUpper() + "." + DBUtils.GetDBString(dr2, 0).ToUpper();
                                        //nodeData.ExpandMode = TreeNodeExpandMode.WebService;
                                        result.Add(nodeData);
                                    }
                                }
                            }
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSchangecall_nodes");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_TSsite_headers(RadTreeNodeData node, List<RadTreeNodeData> result)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // split key into parts 
            char[] delimiter = ".".ToCharArray();
            string[] keyparts = node.Value.Split(delimiter);
            string pdfName = keyparts[1];

            //Table Names
            string schema = Session["s_schema"].ToString();
            string siteTable = schema + ".ft_" + pdfName + "_site";

            //Retrieve Site Info
            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        string strSql = "SELECT call1, name FROM " + siteTable;

                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {
                                        RadTreeNodeData nodeData = new RadTreeNodeData();
                                        nodeData.Text = "<b>Site(" + DBUtils.GetDBString(dr2, 0) + ", " + DBUtils.GetDBString(dr2, 1) + ")</b>";
                                        nodeData.Value = "s." + pdfName + "." + DBUtils.GetDBString(dr2, 0).ToUpper();
                                        nodeData.ExpandMode = TreeNodeExpandMode.WebService;
                                        result.Add(nodeData);
                                    }
                                }
                            }
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSsite_headers");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_TSsite_details(RadTreeNodeData node, List<RadTreeNodeData> result)
        {
            // Node is: Site Existing Header				*	s.pdfid.call1
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // split key into parts 
            char[] delimiter = ".".ToCharArray();
            string[] keyparts = node.Value.Split(delimiter);
            string pdfName = keyparts[1];

            //Table Names
            string schema = Session["s_schema"].ToString();
            string siteTable = schema + ".ft_" + pdfName + "_site";
            string anteTable = schema + ".ft_" + pdfName + "_ante";
            string chanTable = schema + ".ft_" + pdfName + "_chan";

            //Retrieve Site Info
            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    // add site data node
                    RadTreeNodeData snodeData = new RadTreeNodeData();
                    snodeData.Text = "Site Record";
                    snodeData.Value = "d." + pdfName + "." + keyparts[2].ToUpper();
                    result.Add(snodeData);

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        string strSql = "SELECT DISTINCT c.call1, c.call2, c.bndcde, s.name " +
                                        " FROM " + chanTable + " c " +
                                        " LEFT OUTER JOIN " + siteTable + " s ON s.call1 = c.call2 " +
                                        " WHERE c.call1 = '" + keyparts[2] + "'" +
                                        " UNION " +
                                        " SELECT DISTINCT a.call1, a.call2, a.bndcde, s.name " +
                                        " FROM " + anteTable + " a " +
                                        " LEFT OUTER JOIN " + siteTable + " s ON s.call1 = a.call2 " +
                                        " WHERE a.call1 = '" + keyparts[2] + "'" +
                                        " ORDER BY call1, call2, bndcde";

                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {
                                        RadTreeNodeData nodeData = new RadTreeNodeData();
                                        var remotename = DBUtils.GetDBString(dr2, 3);
                                        if (remotename == null) { remotename = get_TSsite_name(DBUtils.GetDBString(dr2, 1)); }
                                        nodeData.Text = "<b>Link to(" + DBUtils.GetDBString(dr2, 1) + ", " + DBUtils.GetDBString(dr2, 2) + ", " + DBUtils.GetDBString(dr2, 3) + ")</b>";
                                        nodeData.Value = "k." + pdfName + "." + DBUtils.GetDBString(dr2, 0).ToUpper() + "." + DBUtils.GetDBString(dr2, 1).ToUpper() + "." + DBUtils.GetDBString(dr2, 2).ToUpper();
                                        nodeData.ExpandMode = TreeNodeExpandMode.WebService;
                                        result.Add(nodeData);
                                    }
                                }
                            }
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSsite_details");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_TSante_details(RadTreeNodeData node, List<RadTreeNodeData> result)
        {
            // Node is: Antenna Top Node			b.pdfid.location.call1

            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // split key into parts 
            char[] delimiter = ".".ToCharArray();
            string[] keyparts = node.Value.Split(delimiter);
            string pdfName = keyparts[1];

            //Table Names
            string schema = Session["s_schema"].ToString();
            string anteTable = schema + ".ft_" + pdfName + "_ante";

            //Retrieve Antenna Info
            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        string strSql = "SELECT  call1, call2, bndcde, anum, acode " +
                                    " FROM " + anteTable + " " +
                                    " WHERE call1='" + keyparts[2] + "'" +
                                    " AND call2='" + keyparts[3] + "'" +
                                    " AND bndcde='" + keyparts[4] + "'" +
                                    " ORDER BY call1, call2, bndcde, anum";

                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {

                                        RadTreeNodeData nodeData = new RadTreeNodeData();// channel data node
                                        nodeData.Text = DBUtils.GetDBInt16(dr2, 3) + "," + DBUtils.GetDBString(dr2, 4);
                                        nodeData.Value = "a." + pdfName + "." +
                                                            DBUtils.GetDBString(dr2, 0).ToUpper() + "." +
                                                            DBUtils.GetDBString(dr2, 1).ToUpper() + "." +
                                                            DBUtils.GetDBString(dr2, 2).ToUpper() + "." +
                                                            DBUtils.GetDBInt16(dr2, 3).ToUpper();
                                        result.Add(nodeData);
                                    }
                                }
                            }
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSante_details");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_TSchan_details(RadTreeNodeData node, List<RadTreeNodeData> result)
        {
            // Node is: Channel Top Node			h.pdfid.call2.call2,bndcde

            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // split key into parts 
            char[] delimiter = ".".ToCharArray();
            string[] keyparts = node.Value.Split(delimiter);
            string pdfName = keyparts[1];

            //Table Names
            string schema = Session["s_schema"].ToString();
            string chanTable = schema + ".ft_" + pdfName + "_chan";

            //Retrieve Channel Info
            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        string strSql = "SELECT  call1, call2, bndcde, chid, antnumbtx1, antnumbtx2," +
                                    " antnumbrx1, antnumbrx2, antnumbrx3 " +
                                    " FROM " + chanTable + " " +
                                    " WHERE call1='" + keyparts[2] + "'" +
                                    " AND call2='" + keyparts[3] + "'" +
                                    " AND bndcde='" + keyparts[4] + "'" +
                                    " ORDER BY call1, call2, bndcde, chid";

                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {
                                        RadTreeNodeData nodeData = new RadTreeNodeData();// channel data node
                                        nodeData.Text = " TX-" + DBUtils.GetDBInt8(dr2, 4) + "," + DBUtils.GetDBInt8(dr2, 5) +
                                                        " RX-" + DBUtils.GetDBInt8(dr2, 6) + "," + DBUtils.GetDBInt8(dr2, 7) + "," + DBUtils.GetDBInt8(dr2, 8);
                                        nodeData.Value = "c." + pdfName + "." + DBUtils.GetDBString(dr2, 0).ToUpper() + "." + DBUtils.GetDBString(dr2, 1).ToUpper() + "." + DBUtils.GetDBString(dr2, 2).ToUpper() + "." + DBUtils.GetDBString(dr2, 3).ToUpper();
                                        result.Add(nodeData);
                                    }
                                }
                            }
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSchan_details");
                return "ERRORSYS:" + ee.Message;
            }
        }

 
        [WebMethod(EnableSession = true)]
        public string fix_missing_sites(string pdfid)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // Table names
            string schema = Session["s_schema"].ToString();
            string anteTable = schema + ".ft_" + pdfid + "_ante";
            string chanTable = schema + ".ft_" + pdfid + "_chan";
            string siteTable = schema + ".ft_" + pdfid + "_site";

            string strSql = "";
            string selcall;
            StringBuilder rsTree = new StringBuilder("", 1000);
            StringBuilder fullString = new StringBuilder("", 1000);

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    // populate site level of TS tree


                    // this section was added Sep 29/2000 to handle case of PDF's with antenna
                    // or channel records with no corresponding call1 site in the PDF
                    // the process is to check for any such 'orphan' antenna or channel records
                    // and if any are found, to add the corresponding site record(s)

                    strSql = "SELECT DISTINCT call1 FROM " + anteTable +
                            " WHERE call1 not in (SELECT call1 FROM " + siteTable + ")";
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {

                        ucn.Open();
                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {

                            using (OdbcDataReader dr1 = select1.ExecuteReader())
                            {
                                if (dr1.HasRows)
                                {
                                    while (dr1.Read())
                                    {
                                        selcall = DBUtils.GetDBString(dr1, 0);
                                        strSql = "INSERT INTO " + siteTable +
                                            " (cmd,recstat,call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime) " +
                                            "SELECT " +
                                            "'N','U',call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime " +
                                            "FROM main.mt_site WHERE call1 = '" + selcall + "'";

                                        using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                        {
                                            ucnn.Open();
                                            using (OdbcCommand update1 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update1.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    strSql = "SELECT DISTINCT call1 FROM " + chanTable +
                             " WHERE call1 not in (SELECT call1 FROM " + siteTable + ")";
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {
                                        selcall = DBUtils.GetDBString(dr2, 0);
                                        strSql = "INSERT INTO " + siteTable +
                                            " (cmd,recstat,call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime) " +
                                            "SELECT " +
                                            "'N','U',call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime " +
                                            "FROM main.mt_site WHERE call1 = '" + selcall + "'";

                                        using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                        {
                                            ucnn.Open();
                                            using (OdbcCommand update2 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update2.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "fix_missing_sites");
                return "ERRORSYS:" + ee.Message;
            }
            return "OK";
        }
 
        private string get_TSsite_name(string call1)
        {
            string strSql;
            string retval = "-";

            try
            {
                using (OdbcConnection lcn = new OdbcConnection(cnstr))
                {
                    lcn.Open();
                    strSql = "SELECT name from main.mt_site where call1 = '" + call1 + "'";
                    using (OdbcCommand select1 = new OdbcCommand(strSql, lcn))
                    {
                        using (OdbcDataReader dr1 = select1.ExecuteReader())
                        {
                            if (dr1.HasRows)
                            {
                                dr1.Read();
                                retval = DBUtils.GetDBString(dr1, 0);  // call1 found in main.mt_site
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSsite_name");
                return "ERRORSYS:" + ee.Message;
            }
            return retval;
        }
        [WebMethod(EnableSession = true)]
        public string CountTSPdfNodes(string pdfid)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            // Table names
            string schema = Session["s_schema"].ToString();
            string anteTable = schema + ".ft_" + pdfid + "_ante";
            string chanTable = schema + ".ft_" + pdfid + "_chan";
            string siteTable = schema + ".ft_" + pdfid + "_site";

            string strSql = "";

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();

                        // get count of sites
                        strSql = "SELECT COUNT(*) FROM " +  siteTable;

                        OdbcCommand cmd1 = new OdbcCommand(strSql, ucn);

                        int SiteCount = Convert.ToInt32(cmd1.ExecuteScalar());

                        // get count of distinct links
                        strSql = "SELECT  COUNT(*) FROM (" +
                                "SELECT call1, call2, bndcde from venn.ft_ccc2_ante " +
                                "UNION " +
                                "SELECT call1, call2, bndcde from venn.ft_ccc2_chan) as tem";
                        
                        OdbcCommand cmd2 = new OdbcCommand(strSql, ucn);

                        int LinkCount = Convert.ToInt32(cmd2.ExecuteScalar());

                        // get count on antennae
                        strSql = "SELECT COUNT(*) FROM " +  anteTable;

                        OdbcCommand cmd3 = new OdbcCommand(strSql, ucn);

                        int AnteCount = Convert.ToInt32(cmd3.ExecuteScalar());

                        strSql = "SELECT COUNT(*) FROM " +  chanTable;
                        OdbcCommand cmd4 = new OdbcCommand(strSql, ucn);

                        int ChanCount = Convert.ToInt32(cmd4.ExecuteScalar());

                        // initial 3 nodes title/change of call signs/ sites
                        // each site generates 2 nodes
                        // each link generates 3 nodes
                        // each antenna and each channel generate 1 node

                        return (3 + SiteCount * 2 + LinkCount * 3 + AnteCount + ChanCount).ToString();

                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_TSsite_name");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string populateChange(OdbcConnection ucn, string siteName)
        {

            StringBuilder rslist = new StringBuilder("", 1000);
            rslist.Append("t." + siteName + "*Title Record;");
            rslist.Append("u." + siteName + "*Change of Location Code;o." + siteName + "*New Change of Location Code;");

            //Table Names
            string schema = Session["s_schema"].ToString();
            string clocTable = schema + ".fe_" + siteName + "_cloc";
            string ccalTable = schema + ".fe_" + siteName + "_ccal";

            //Retrieve Change of Location Codes;

            string strSql = "SELECT newlocation, oldlocation, name FROM " + clocTable;

            using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
            {

                using (OdbcDataReader dr1 = select1.ExecuteReader())
                {

                    if (dr1.HasRows)
                    {
                        while (dr1.Read())
                        {
                            rslist.Append("q." + siteName + "." + DBUtils.GetDBString(dr1, 1) + "." + DBUtils.GetDBString(dr1, 0) + "*" + DBUtils.GetDBString(dr1, 1) + "(" + DBUtils.GetDBString(dr1, 2) + ") Changed To " + DBUtils.GetDBString(dr1, 0) + ";");
                        }
                    }
                }
            }

            //Retrieve Change of Call Signs
            rslist.Append("c." + siteName + "*Change of Call Signs;");
            rslist.Append("j." + siteName + "*New Change of Call Sign;");
            rslist.Append("i." + siteName + "*Sites;w." + siteName + "*New Site;");

            strSql = "SELECT newcallsign, oldcallsign FROM " + ccalTable;

            using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
            {
                using (OdbcDataReader dr2 = select2.ExecuteReader())
                {
                    if (dr2.HasRows)
                    {
                        while (dr2.Read())
                        {
                            rslist.Append("g." + siteName + "." + DBUtils.GetDBString(dr2, 1) + "." + DBUtils.GetDBString(dr2, 0) + "*" + DBUtils.GetDBString(dr2, 1) + " Changed To " + DBUtils.GetDBString(dr2, 0) + ";");
                        }
                    }
                }
            }
            return rslist.ToString();
        }
        [WebMethod(EnableSession = true)]
        public string sites(string pdfName)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();

                        StringBuilder fullString = new StringBuilder("", 1000);

                        //Table Names
                        string schema = Session["s_schema"].ToString();
                        string anteTable = schema + ".fe_" + pdfName + "_ante";
                        string chanTable = schema + ".fe_" + pdfName + "_chan";
                        string siteTable = schema + ".fe_" + pdfName + "_site";

                        string strSql = "";

                        // this section was added July 10/2001 to handle case of PDF's with antenna
                        // or channel records with no corresponding site or antenna in the PDF

                        // the first step is to check for channel records with no corresponding site record
                        // and if any are found, to add the corresponding site record(s)

                        strSql = "SELECT DISTINCT location FROM " + chanTable +
                                 " WHERE location not in (SELECT location from " + siteTable + ")";

                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr1 = select1.ExecuteReader())
                            {
                                if (dr1.HasRows)
                                {
                                    using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                    {
                                        ucnn.Open();
                                        while (dr1.Read())
                                        {
                                            string sellocation = DBUtils.GetDBString(dr1, 0);
                                            strSql = "INSERT INTO " + siteTable +
                                                 " (cmd,recstat,location,name,prov,oper,latit,longit," +
                                                 "grnd,stats,radio,sdate,nots," +
                                                 "rain,reg)" +
                                                 "SELECT " +
                                                 "'N','U',location,name,prov,oper,latit,longit," +
                                                 "grnd,stats,radio,sdate,nots," +
                                                 "rain,reg " +
                                                 "FROM main.me_site WHERE location = '" + sellocation + "'";

                                            using (OdbcCommand update1 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update1.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // the second step is to check for antenna records with no corresponding site record
                        // and if any are found, to add the corresponding site record(s)

                        strSql = "SELECT DISTINCT location FROM " + anteTable +
                                 " WHERE location not in (SELECT location from " + siteTable + ")";

                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {

                                if (dr2.HasRows)
                                {
                                    using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                    {
                                        ucnn.Open();
                                        while (dr2.Read())
                                        {
                                            string sellocation = DBUtils.GetDBString(dr2, 0);
                                            strSql = "INSERT INTO " + siteTable +
                                                 " (cmd,recstat,location,name,prov,oper,latit,longit," +
                                                 "grnd,stats,radio,sdate,nots," +
                                                 "rain,reg)" +
                                                 "SELECT " +
                                                 "'N','U',location,name,prov,oper,latit,longit," +
                                                 "grnd,stats,radio,sdate,nots," +
                                                 "rain,reg " +
                                                 "FROM main.me_site WHERE location = '" + sellocation + "'";

                                            using (OdbcCommand update2 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update2.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // the third step is to check for channel records with no corresponding antenna record
                        // and if any are found, to add the corresponding antenna record(s)

                        strSql = "SELECT DISTINCT location,call1 FROM " + chanTable +
                                 " WHERE location + call1 not in (SELECT location + call1 from " + anteTable + ")";

                        using (OdbcCommand select3 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr3 = select3.ExecuteReader())
                            {
                                if (dr3.HasRows)
                                {
                                    using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                    {
                                        ucnn.Open();
                                        while (dr3.Read())
                                        {
                                            string sellocation = DBUtils.GetDBString(dr3, 0);
                                            string selcall = DBUtils.GetDBString(dr3, 1);
                                            strSql = "INSERT INTO " + anteTable +
                                                 " (cmd,recstat,location,call1,txband, rxband, acodetx, acoderx," +
                                                 "g_t, lnat, aht, afslt, afslr, txhgmax, rxhgmax, satlong, satlongit, satlongs, az, " +
                                                 "el, sarc1, sarc2, rxpre, txpre, rxtro, txtro, icaccount, satname, " +
                                                 "stata, nota, op2, antref, orbit)" +
                                                 "SELECT " +
                                                 "'N','U',location,call1,txband, rxband, acodetx, acoderx," +
                                                 "g_t, lnat, aht, afslt, afslr, txhgmax, rxhgmax, satlong, satlongit, satlongs, az, " +
                                                 "el, sarc1, sarc2, rxpre, txpre, rxtro, txtro, icaccount, satname, " +
                                                 "stata, nota, op2, antref, orbit " +
                                                 "FROM main.me_ante WHERE location = '" + sellocation + "'" +
                                                 " AND call1 = '" + selcall + "'";

                                            using (OdbcCommand update3 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update3.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // missing record insertion complete - start building tree
                        // load title, change of location and change of call sign info

                        fullString.Append(populateChange(ucn, pdfName));

                        //return fullString;

                        strSql = "SELECT '0' as type,location,name,' ' as call1, ' ' as txband," +
                                 "' ' as rxband, ' ' as acodetx,' ' as acoderx, ' ' as chid " +
                                 "FROM " + siteTable + " " +
                                 "UNION SELECT '1',location, ' ', call1, txband, rxband, acodetx," +
                                 "acoderx, ' ' " +
                                 "FROM " + anteTable + " " +
                                 "UNION SELECT '2',location,' ',call1,' ',' ',' ',' ',chid " +
                                 "FROM " + chanTable + " " +
                                 "ORDER BY location,call1,type,chid";

                        using (OdbcCommand select4 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr4 = select4.ExecuteReader())
                            {
                                if (!dr4.HasRows)
                                {
                                    return fullString.ToString();
                                }
                                else // rows found
                                {
                                    // process sites, antennae, channels and azimiths
                                    //Initialize start values

                                    string type = "0";
                                    string oldtype = "0";
                                    string location = "";
                                    string oldloc = DBUtils.GetDBString(dr4, 1); // location
                                    string call1 = "";
                                    string oldcall1 = DBUtils.GetDBString(dr4, 3); // call1
                                    string chid = "";
                                    string oldchid = DBUtils.GetDBString(dr4, 8); // chid

                                    while (dr4.Read())
                                    {
                                        type = DBUtils.GetDBString(dr4, 0); // type
                                        call1 = DBUtils.GetDBString(dr4, 3); // call1
                                        location = DBUtils.GetDBString(dr4, 1); // location
                                        chid = DBUtils.GetDBString(dr4, 8); // chid

                                        // for debug only
                                        //fullString.Append(":"+type+":"+location+":"+call1+":"+chid+":**");

                                        if (type == "0") // site record
                                        {
                                            if (oldtype == "1") // previous was antenna
                                            {
                                            }
                                            if (oldtype == "2") // previous was channel
                                            {
                                            }

                                            fullString.Append("s." + pdfName + "." + location + "*" + "Site(" + location + ", " + DBUtils.GetDBString(dr4, 2) + ");");
                                            oldloc = location;
                                            oldcall1 = "";
                                            oldchid = "";
                                            oldtype = "0";
                                        }

                                        if (type == "1") // antenna record
                                        {
                                            if (oldtype == "0") // previous was site
                                            {
                                            }
                                            if (oldtype == "1") // previous was antenna
                                            {
                                            }
                                            if (oldtype == "2") // previous was channel
                                            {
                                            }

                                            fullString.Append("n." + pdfName + "." + location + "." + call1 + "*Antenna(" + call1 + ", " + DBUtils.GetDBString(dr4, 4) + ", " + DBUtils.GetDBString(dr4, 5) + ");");
                                            fullString.Append("a." + pdfName + "." + location + "." + call1 + "*Antenna " + DBUtils.GetDBString(dr4, 6) + " " + DBUtils.GetDBString(dr4, 7) + ";");
                                            fullString.Append("z." + pdfName + "." + location + "." + call1 + "*Azimuth;");
                                            fullString.Append("p." + pdfName + "." + location + "." + call1 + "*" + "New Channel;");

                                            oldchid = "";
                                            oldcall1 = call1;
                                            oldtype = "1";
                                        }

                                        if (type == "2") // channel record
                                        {
                                            call1 = DBUtils.GetDBString(dr4, 3);

                                            if (oldtype == "0") // previous was site
                                            {
                                            }

                                            if (oldtype == "1") // previous was antenna
                                            {
                                            }

                                            if (oldtype == "2") // previous was channel
                                            {
                                            }

                                            fullString.Append("h." + pdfName + "." + location + "." + call1 + "." + DBUtils.GetDBString(dr4, 8) + "*" + "Channel " + DBUtils.GetDBString(dr4, 8) + ";");

                                            oldchid = chid;
                                            oldtype = "2";
                                        }
                                    }

                                    // add azimuth option if last entry in pdf was channel

                                    if (oldtype == "2")
                                    {
                                        //fullString = fullString + "z." + pdfName + "." + oldloc + "." + oldcall1 + "*Azimuth;"; 
                                    }

                                    // add azimuth option if last entry in pdf was antenna

                                    if (oldtype == "1")
                                    {
                                        //fullString = fullString + "z." + pdfName + "." + oldloc + "." + oldcall1 + "*Azimuth;"; 
                                    }

                                    //close the connection
                                }
                            }
                        }

                        return fullString.ToString();
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "wsESTree");
                return "ERRORSYS:" + ee.Message;
            }
        }
        [WebMethod(EnableSession = true)]
        public string esTree()
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            StringBuilder rsTree = new StringBuilder("", 1000);

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    string tablename;
                    string filename;
                    string colon = "";

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();

                        string schema = Session["s_schema"].ToString();
                        string strSql = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = '" +
                                    schema + "' AND table_name LIKE 'fe\\_%\\_titl%' ESCAPE '\\' order by table_name";

                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr = select1.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        tablename = DBUtils.GetDBString(dr, 0);
                                        filename = tablename.Substring(3, tablename.Length - 8); // strip fe_ and _titl
                                        rsTree.Append(colon + filename);
                                        colon = ":";
                                    }
                                }
                                else
                                {
                                    rsTree.Append("NONE");
                                }
                            }
                        }
                    }
                    return rsTree.ToString();
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "wsESTree");
                return "ERRORSYS:" + ee.Message;
            }
        }
        [WebMethod(EnableSession = true)]
        public string TSpopulateTree()
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            StringBuilder rsTree = new StringBuilder("", 1000);

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    string tablename = "";
                    string filename = "";
                    string colon = "";

                    // populate pdf level of TS tree

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();

                        string schema = Session["s_schema"].ToString();
                        string strSql = "SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = '" +
                                    schema + "' AND table_name LIKE 'ft\\_%\\_titl%' ESCAPE '\\' order by table_name";

                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr = select1.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        tablename = DBUtils.GetDBString(dr, 0);
                                        filename = tablename.Substring(3, tablename.Length - 8); // strip ft_ and _titl
                                        rsTree.Append(colon + filename);
                                        colon = ":";
                                    }
                                }
                                else
                                {
                                    rsTree.Append("NONE");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "TSpopulatetree");
                return "ERRORSYS:" + ee.Message;
            }

            //TSXMLPdf(rsTree.ToString());
            return rsTree.ToString();
        }
        [WebMethod(EnableSession = true)]
        public string TSpopulatesite(string pdfid)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            string tmpCall1 = "";
            string tmpName1 = "";
            string chng = "";

            // Table names
            string schema = Session["s_schema"].ToString();
            string anteTable = schema + ".ft_" + pdfid + "_ante";
            string chanTable = schema + ".ft_" + pdfid + "_chan";
            string siteTable = schema + ".ft_" + pdfid + "_site";

            string strSql = "";
            string selcall;
            StringBuilder rsTree = new StringBuilder("", 1000);
            StringBuilder fullString = new StringBuilder("", 1000);

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    // populate site level of TS tree


                    // this section was added Sep 29/2000 to handle case of PDF's with antenna
                    // or channel records with no corresponding call1 site in the PDF
                    // the process is to check for any such 'orphan' antenna or channel records
                    // and if any are found, to add the corresponding site record(s)

                    strSql = "SELECT DISTINCT call1 FROM " + anteTable +
                            " WHERE call1 not in (SELECT call1 FROM " + siteTable + ")";
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {

                        ucn.Open();
                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {

                            using (OdbcDataReader dr1 = select1.ExecuteReader())
                            {
                                if (dr1.HasRows)
                                {
                                    while (dr1.Read())
                                    {
                                        selcall = DBUtils.GetDBString(dr1, 0);
                                        strSql = "INSERT INTO " + siteTable +
                                            " (cmd,recstat,call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime) " +
                                            "SELECT " +
                                            "'N','U',call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime " +
                                            "FROM main.mt_site WHERE call1 = '" + selcall + "'";

                                        using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                        {
                                            ucnn.Open();
                                            using (OdbcCommand update1 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update1.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    strSql = "SELECT DISTINCT call1 FROM " + chanTable +
                             " WHERE call1 not in (SELECT call1 FROM " + siteTable + ")";
                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        using (OdbcCommand select2 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr2 = select2.ExecuteReader())
                            {
                                if (dr2.HasRows)
                                {
                                    while (dr2.Read())
                                    {
                                        selcall = DBUtils.GetDBString(dr2, 0);
                                        strSql = "INSERT INTO " + siteTable +
                                            " (cmd,recstat,call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime) " +
                                            "SELECT " +
                                            "'N','U',call1,name,prov,oper,latit,longit," +
                                            "grnd,stats,sdate,loc,icaccount,reg,spoint,nots," +
                                            "oprtyp,snumb,notwr,bandwd1,bandwd2,bandwd3,bandwd4," +
                                            "bandwd5,bandwd6,bandwd7,bandwd8,mdate,mtime " +
                                            "FROM main.mt_site WHERE call1 = '" + selcall + "'";

                                        using (OdbcConnection ucnn = new OdbcConnection(cnstr))
                                        {
                                            ucnn.Open();
                                            using (OdbcCommand update2 = new OdbcCommand(strSql, ucnn))
                                            {
                                                update2.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // build string for change of call sign records
                    chng = TSpopulateChange(pdfid);
                    if (chng.Substring(0, 5) == "ERROR")
                    {
                        return chng;
                    }

                    //Get site records
                    strSql = "SELECT call1,name " +
                             "FROM " + siteTable +
                             " ORDER BY call1";

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();
                        using (OdbcCommand select3 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr3 = select3.ExecuteReader())
                            {
                                if (dr3.HasRows)
                                {
                                    while (dr3.Read())
                                    {
                                        tmpCall1 = DBUtils.GetDBTString(dr3, 0);
                                        tmpName1 = DBUtils.GetDBTString(dr3, 1);
                                        fullString.Append("s." + tmpCall1 + "*" + "Site(" + tmpCall1 + ", " + tmpName1 + ")" + ":");
                                        //fullString.Append("d." + tmpCall1 + ":");
                                        //fullString.Append("n." + tmpCall1 + ":");
                                    }
                                }
                            }
                        }  // end connection
                    } // end impersonation
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "TSpopulatesite");
                return "ERRORSYS:" + ee.Message;
            }
            //TSXMLSites(pdfid, chng + fullString.ToString());
            return chng + fullString.ToString();
        }

        //Selects the records for the Change of Call Sign node	
        private string TSpopulateChange(string pdfid)
        {
            string chngMsg = "";
            string newChng = "";

            string schema = Session["s_schema"].ToString();
            string chngTable = schema + ".ft_" + pdfid + "_chng";
            string strSql;

            try
            {
                strSql = "SELECT ALL oldcall1, name, newcall1 FROM " + chngTable;
                using (OdbcConnection ucn = new OdbcConnection(cnstr))
                {
                    ucn.Open();
                    using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                    {
                        using (OdbcDataReader dr1 = select1.ExecuteReader())
                        {
                            if (dr1.HasRows)
                            {
                                while (dr1.Read())
                                {
                                    chngMsg = chngMsg + "g." + DBUtils.GetDBString(dr1, 0) + "." + DBUtils.GetDBString(dr1, 2) + "*" + DBUtils.GetDBString(dr1, 0) + "(" + DBUtils.GetDBString(dr1, 1) + ") changed to " + DBUtils.GetDBString(dr1, 2) + ":";
                                }
                                newChng = "t:j:";
                                newChng = newChng + chngMsg + "i:w:";
                            }
                            else
                            {
                                newChng = "t:j:i:w:";
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "TSpopulatechange");
                return "ERRORSYS:" + ee.Message;
            }
            return newChng;
        }

        [WebMethod(EnableSession = true)]
        public string TSpopulateantechan(string pdfid, string siteid)
        {
            // this if clause forces return if session has timed out
            if (!getconnstring())
            {
                return cnstr;
            }

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)Session["principalw"];

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    // this function expands antennas and channels for specified site

                    string tmpCall1 = "";
                    string tmpName1 = "";
                    string tmpCall2 = "";
                    string tmpName2 = "";
                    string tmpBndcde = "";
                    StringBuilder fullString = new StringBuilder("", 1000);

                    string schema = Session["s_schema"].ToString();

                    // Table names
                    string anteTable = schema + ".ft_" + pdfid + "_ante";
                    string chanTable = schema + ".ft_" + pdfid + "_chan";
                    string siteTable = schema + ".ft_" + pdfid + "_site";

                    string strSql;

                    // field  0 - type
                    // field  1 - call1
                    // field  2 - call2
                    // field  3 - bndcde
                    // field  4 - anum
                    // field  5 - acode
                    // field  6 - chid
                    // field  7 - antnumbtx1
                    // field  8 - antnumbtx2
                    // field  9 - antnumbrx1
                    // field 10 - antnumbrx2
                    // field 11 - antnumbrx3
                    // field 12 - name

                    strSql = "SELECT 0 as type,call1, ' ' as call2,' ' as bndcde,0 as anum,' ' as acode, " +
                            " ' ' as chid,0 as antnumbtx1,0 as antnumbtx2," +
                            " 0 as antnumbrx1,0 as antnumbrx2,0 as antnumbrx3,name" +
                            " FROM " + siteTable + " where call1 = '" + siteid + "'" +
                            " UNION SELECT DISTINCT 1,a.call1,a.call2,a.bndcde,a.anum,a.acode,' ', " +
                            " 0,0,0,0,0,s.name " +
                            " FROM " + anteTable + " a left join " + siteTable + " s on a.call2 = s.call1" +
                            " where a.call1 = '" + siteid + "'" +
                            " UNION SELECT DISTINCT 2,c.call1,c.call2,c.bndcde,0,' ',c.chid, " +
                            " c.antnumbtx1,c.antnumbtx2,c.antnumbrx1,c.antnumbrx2,c.antnumbrx3,s.name " +
                            " FROM " + chanTable + " c left join " + siteTable + " s on c.call2 = s.call1" +
                            " where c.call1 = '" + siteid + "'" +
                            " ORDER BY call1,call2,bndcde,type,anum,chid ";

                    // 0 type
                    // 1 call1
                    // 2 call2
                    // 3 bndcde
                    // 4 anum
                    // 5 acode
                    // 6 chid
                    // 7 antnumbtx1
                    // 8 antnumbtx2
                    // 9 antnumbrx1
                    // 10 antnumbrx2
                    // 11 antnumbrx3
                    // 12 name

                    using (OdbcConnection ucn = new OdbcConnection(cnstr))
                    {
                        ucn.Open();

                        //Get site/antenna/channel records via union query
                        using (OdbcCommand select1 = new OdbcCommand(strSql, ucn))
                        {
                            using (OdbcDataReader dr1 = select1.ExecuteReader())
                            {
                                if (dr1.HasRows)
                                {
                                    fullString.Append("y." + siteid + ":");
                                    //Initialize start values

                                    int oldtype = 0;
                                    string oldcall1 = DBUtils.GetDBString(dr1, 1);  // call1
                                    string oldcall2 = DBUtils.GetDBString(dr1, 2);  // call2
                                    string oldbndcde = DBUtils.GetDBString(dr1, 3); // bndcde
                                    int type = 0;
                                    while (dr1.Read())
                                    {
                                        type = dr1.GetInt32(0);// type
                                        tmpCall1 = DBUtils.GetDBString(dr1, 1); // call1
                                        tmpCall2 = DBUtils.GetDBString(dr1, 2); // call2
                                        tmpBndcde = DBUtils.GetDBString(dr1, 3);// bndcde

                                        if (type == 0) // site record
                                        {
                                            tmpName1 = DBUtils.GetDBString(dr1, 12);
                                            fullString.Append("d." + tmpCall1 + ":");
                                            fullString.Append("n." + tmpCall1 + ":");

                                            oldcall1 = tmpCall1;
                                            oldcall2 = "";
                                            oldbndcde = "";
                                            oldtype = 0;
                                        }

                                        if (type == 1) // antenna record
                                        {
                                            if (!dr1.IsDBNull(12))
                                            {
                                                tmpName2 = DBUtils.GetDBString(dr1, 12);
                                            }
                                            else
                                            {
                                                tmpName2 = "null";
                                            }

                                            if (oldtype == 0) // previous was site
                                            {
                                                // add link info
                                                if (tmpName2 == "null")
                                                {
                                                    tmpName2 = get_site_name(tmpCall2);
                                                }
                                                fullString.Append("k." + tmpCall1 + "." + tmpCall2);
                                                fullString.Append("." + tmpBndcde + "*");
                                                fullString.Append("Link To(" + tmpCall2 + ", " + tmpName2 + ")");
                                                fullString.Append(tmpBndcde + ":");
                                                fullString.Append("x." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                            }
                                            if (oldtype == 1) // previous was antenna
                                            {
                                                if (oldcall2 == tmpCall2 && oldbndcde == tmpBndcde)  // on old link
                                                {
                                                }
                                                else // on new link
                                                {
                                                    // add new channel option on old link
                                                    fullString.Append("f." + tmpCall1 + "." + oldcall2 + "." + oldbndcde + ":");
                                                    if (tmpName2 == "null")
                                                    {
                                                        tmpName2 = get_site_name(tmpCall2);
                                                    }
                                                    fullString.Append("k." + tmpCall1 + "." + tmpCall2);
                                                    fullString.Append("." + tmpBndcde + "*");
                                                    fullString.Append("Link To(" + tmpCall2 + ", " + tmpName2 + ")");
                                                    fullString.Append(tmpBndcde + ":");
                                                    fullString.Append("x." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                }
                                            }
                                            if (oldtype == 2) // previous was channel
                                            {
                                                if (oldcall2 == tmpCall2 && oldbndcde == tmpBndcde)  // on same link
                                                {
                                                }
                                                else // on new link
                                                {
                                                    if (tmpName2 == "null")
                                                    {
                                                        tmpName2 = get_site_name(tmpCall2);
                                                    }
                                                    fullString.Append("k." + tmpCall1 + "." + tmpCall2);
                                                    fullString.Append("." + tmpBndcde + "*");
                                                    fullString.Append("Link To(" + tmpCall2 + ", " + tmpName2 + ")");
                                                    fullString.Append(tmpBndcde + ":");
                                                    fullString.Append("x." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                }
                                            }
                                            fullString.Append("a." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + "." + dr1.GetValue(4).ToString() + "*" + dr1.GetValue(4).ToString() + ", " + dr1.GetValue(5).ToString() + ":");

                                            oldbndcde = tmpBndcde;
                                            oldcall2 = tmpCall2;
                                            oldtype = 1;
                                        }

                                        if (type == 2) // channel record
                                        {
                                            if (!dr1.IsDBNull(12))
                                            {
                                                tmpName2 = DBUtils.GetDBString(dr1, 12);
                                            }
                                            else
                                            {
                                                tmpName2 = "null";
                                            }

                                            if (oldtype == 0) // previous was site
                                            {
                                                // add link info
                                                if (tmpName2 == "null")
                                                {
                                                    tmpName2 = get_site_name(tmpCall2);
                                                }

                                                fullString.Append("k." + tmpCall1 + "." + tmpCall2);
                                                fullString.Append("." + tmpBndcde + "*");
                                                fullString.Append("Link To(" + tmpCall2 + ", " + tmpName2 + ")");
                                                fullString.Append(tmpBndcde + ":");
                                                fullString.Append("x." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                fullString.Append("f." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                            }
                                            if (oldtype == 1) // previous was antenna
                                            {

                                                if (oldcall2 == tmpCall2 && oldbndcde == tmpBndcde)  // on same link
                                                {
                                                    fullString.Append("f." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                }
                                                else // on new link
                                                {
                                                    if (tmpName2 == "null")
                                                    {
                                                        tmpName2 = get_site_name(tmpCall2);
                                                    }
                                                    fullString.Append("k." + tmpCall1 + "." + tmpCall2);
                                                    fullString.Append("." + tmpBndcde + "*");
                                                    fullString.Append("Link To(" + tmpCall2 + ", " + tmpName2 + ")");
                                                    fullString.Append(tmpBndcde + ":");
                                                    fullString.Append("x." + tmpCall2 + "." + tmpBndcde + ":");
                                                    fullString.Append("f." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                }
                                            }
                                            if (oldtype == 2) // previous was channel
                                            {
                                                if (oldcall2 == tmpCall2 && oldbndcde == tmpBndcde) // on same link
                                                {
                                                }
                                                else // on new link
                                                {
                                                    if (tmpName2 == "null")
                                                    {
                                                        tmpName2 = get_site_name(tmpCall2);
                                                    }
                                                    fullString.Append("k." + tmpCall1 + "." + tmpCall2);
                                                    fullString.Append("." + tmpBndcde + "*");
                                                    fullString.Append("Link To(" + tmpCall2 + ", " + tmpName2 + ")");
                                                    fullString.Append(tmpBndcde + ":");
                                                    fullString.Append("x." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                    fullString.Append("f." + tmpCall1 + "." + tmpCall2 + "." + tmpBndcde + ":");
                                                }
                                            }
                                            fullString.Append("c." + tmpCall1 + "." + tmpCall2 + ".");
                                            fullString.Append(tmpBndcde + ".");
                                            fullString.Append(DBUtils.GetDBString(dr1, 6) + "*");
                                            fullString.Append(DBUtils.GetDBString(dr1, 6));
                                            fullString.Append(" TX-");
                                            if (!dr1.IsDBNull(7))
                                            {
                                                fullString.Append(dr1.GetInt32(7).ToString());
                                            }
                                            fullString.Append(",");
                                            if (!dr1.IsDBNull(8))
                                            {
                                                fullString.Append(dr1.GetInt32(8).ToString());
                                            }
                                            fullString.Append(" RX-");
                                            if (!dr1.IsDBNull(9))
                                            {
                                                fullString.Append(dr1.GetInt32(9).ToString());
                                            }
                                            fullString.Append(",");
                                            if (!dr1.IsDBNull(10))
                                            {
                                                fullString.Append(dr1.GetInt32(10).ToString());
                                            }
                                            fullString.Append(",");
                                            if (!dr1.IsDBNull(11))
                                            {
                                                fullString.Append(dr1.GetInt32(11).ToString());
                                            }
                                            fullString.Append(":");
                                            oldbndcde = tmpBndcde;
                                            oldcall2 = tmpCall2;
                                            oldtype = 2;
                                        }
                                    }

                                    //close the connection
                                    dr1.Close();
                                    dr1.Dispose();
                                    //TSXMLantechan(pdfid, siteid, fullString.ToString());
                                    return fullString.ToString();

                                }
                                else // no data
                                {
                                    return "ERROR: TABLES EMPTY-" + strSql;
                                }
                            }  // dr1 close
                        }   //odbccommand close
                    } // ucn close
                }  // impersonation close
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "TSpopulateantechan");
                return "ERRORSYS:" + ee.Message;
            }
        }
        private string get_site_name(string call1)
        {
            string strSql;
            string retval = "-";

            try
            {
                using (OdbcConnection lcn = new OdbcConnection(cnstr))
                {
                    lcn.Open();
                    strSql = "SELECT name from main.mt_site where call1 = '" + call1 + "'";
                    using (OdbcCommand select1 = new OdbcCommand(strSql, lcn))
                    {
                        using (OdbcDataReader dr1 = select1.ExecuteReader())
                        {
                            if (dr1.HasRows)
                            {
                                dr1.Read();
                                retval = DBUtils.GetDBString(dr1, 0);  // call1 found in main.mt_site
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "get_site_name");
                return "ERRORSYS:" + ee.Message;
            }
            return retval;
        }
    } 
}
