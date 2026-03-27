using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;

namespace DBAccess
{
    public class memship
    {
        protected string m_applicationid;
        protected string m_username;
        protected string m_password;
		protected bool m_isapproved;
		protected bool m_islockedout;
		protected string m_ipaddress;


        public string logserial
        {
            get { return m_applicationid; }
            set { m_applicationid = value; }
        }
        
        public string username
        {
            get { return m_username; }
            set { m_username = value; }
        }

        public string password
        {
            get { return m_password; }
            set { m_password = value; }
        }

        public bool isapproved
        {
			get{return m_isapproved;}
			set{m_isapproved = value;}
		}

		public bool islockedout
        {
			get{return m_islockedout;}
			set{m_islockedout = value;}
		}

		public string ipaddress
        {
			get{return m_ipaddress;}
			set{m_ipaddress = value;}
		}

        public memship (string applicationid, string username, string ipaddress)
		{
            HttpContext ctx = HttpContext.Current;

			//	This is the constructor for a dblogger object.  It must be called with the new
			//	operator.
            m_applicationid = applicationid;
            m_username = username;
            m_ipaddress = ipaddress;
			m_password = "";
			m_isapproved = false;
			m_islockedout = false;
		}

        public int Insert(memship currtry)
        {
            HttpContext ctx = HttpContext.Current;

            string strSql;
            int retval = 0;

            strSql = "INSERT INTO dbo.InvalidCredentialsLog " +
                "(ApplicationName, UserName, Password, IsApproved, IsLockedOut, IPAddress, LoginAttemptDate) " +
                " VALUES ('" +
                m_applicationid + "','" +
                m_username + "','" +
                m_password + "','" + m_isapproved + "','" + m_islockedout +
                "','" + m_ipaddress + "', GETDATE())";

            // define log file
            // user was eliminated from filename as we may not know it at this stage
            //string logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\" + ctx.Session["s_user"].ToString() + "currtry.txt";  // continuous log file
            string logfile = ctx.Application["web_drive"].ToString() + "\\extractlogs\\currtry.txt";  // continuous log file
            // open logfile
            StreamWriter swsapr = new StreamWriter(logfile,true);

            DateTime curTime = DateTime.Now;
            string log_time = curTime.ToString("yyyy/MM/dd HH:mm:ss");

            // write info to log file
            swsapr.WriteLine(log_time);
            swsapr.WriteLine(strSql);
            swsapr.Close();

            // get odbc link from config file
            string odbc = ctx.Application["ODBC_DSN"].ToString();

            // create connection string for priviliged uesr
            string cnstr1 = "DSN=" + odbc + ";UID=fcsamics;PWD=Venn.#30";

            OdbcConnection cn = new OdbcConnection(cnstr1);

            try
            {
                cn.Open();
            }
            catch (Exception)
            {
                return (-1);
            }

            //	Execute the insertion
            OdbcCommand oCommand;
            try
            {
                oCommand = new OdbcCommand(strSql, cn);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                //	We had an error trying to insert the record.
                cn.Close();
                retval = -2;
            }
            cn.Close();

            return (retval);
        }
    }
}
