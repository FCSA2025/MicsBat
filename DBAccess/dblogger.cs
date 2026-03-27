using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Data.Odbc;

namespace DBAccess
{
	/// <summary>
	/// Summary description for dblogger.
	/// </summary>
	public class dblogger
	{
        protected string m_logserial;
        protected DateTime m_logstarttime;
        protected string m_loguserid;
		protected string m_logpcode;
		protected string m_logprogram;
		protected string m_logargs;
        protected DateTime m_logfinishtime;
		protected int	 m_logreturncode;
        protected int    m_logerrorcode;
        protected string m_logerrordesc;

        public string logserial
        {
            get { return m_logserial; }
            set { m_logserial = value; }
        }
        
        public DateTime logstarttime
        {
            get { return m_logstarttime; }
            set { m_logstarttime = value; }
        }

        public string loguserid
        {
            get { return m_loguserid; }
            set { m_loguserid = value; }
        }

        public string logpcode
        {
			get{return m_logpcode;}
			set{m_logpcode = value;}
		}

		public string	logprogram{
			get{return m_logprogram;}
			set{m_logprogram = value;}
		}

		public string	logargs{
			get{return m_logargs;}
			set{m_logargs = value;}
		}
		public DateTime	logfinishtime{
            get { return m_logfinishtime;}
            set { m_logfinishtime = value; }
        }

        public int	logreturncode{
			get{return m_logreturncode;}
            set { m_logreturncode = value; }
        }

        public int logerrorcode
        {
            get { return m_logerrorcode; }
            set { m_logerrorcode = value; }
        }

        public string logerrordesc
        {
			get{return m_logerrordesc;}
            set { m_logerrordesc = value; }
        }

        public dblogger()
        {
            HttpContext ctx = HttpContext.Current;

            //	This is the constructor for a dblogger object for password reset only.  It must be called with the new
            //	operator.
            m_logserial = "9999999-" + OELSupport.nextit();
            //m_logstarttime = Now();
            m_loguserid = "resetpwd";
            m_logpcode = "RESETPWD";
            m_logprogram = "";
            m_logargs = "";
            //m_logfinishtime = null;
            m_logreturncode = 0;
            m_logerrorcode = 0;
            m_logerrordesc = "";
        }
        public dblogger(string prog, string args)
		{
            HttpContext ctx = HttpContext.Current;

			//	This is the constructor for a dblogger object.  It must be called with the new
			//	operator.
            m_logserial = ctx.Session["FCSASESS"].ToString() + "-" + OELSupport.nextit();
            //m_logstarttime = Now();
            m_loguserid = ctx.Session["s_user"].ToString();
			m_logpcode = ctx.Session["defProject"].ToString();
			m_logprogram = prog;
			m_logargs = args;
			//m_logfinishtime = null;
			m_logreturncode = 0;
            m_logerrorcode = 0;
			m_logerrordesc = "";			
		}
		
		
		public dblogger(string prog):this (prog, ""){}
		
		//	This will add a record to the web.dblogger table
		public int Start()
		{
			string	strSql;
            int retval = 0;
            dbconnect cn;

            //SQL:VIEW:web.dblogger_view
            strSql = "INSERT INTO web.dblogger_view " +
                "( logserial, logstarttime, loguserid, logpcode, logprogram, logargs) " +
			    " VALUES ('" +
                m_logserial + "'," +
			    "GETDATE(),'" +
			    m_loguserid + "','" + m_logpcode + "','" + m_logprogram +
                "','" + m_logargs + "')";
  
		    try{
				cn = new dbconnect();
			} catch (Exception ex){
                m_logerrorcode = -97;
				m_logerrordesc = "Could not connect to database: " + ex.Message;
				HttpContext.Current.Response.Write("Connection error.<br />" + ex.Message);
				HttpContext.Current.Response.Flush();
                return (-1);
			}
			
			//	Execute the insertion
			OdbcCommand oCommand;
			try {
				oCommand = new OdbcCommand(strSql, cn.Connection);
				oCommand.ExecuteNonQuery();
			} catch (Exception ex) {
				//	We had an error trying to insert the record.
                m_logerrorcode = -96;
                m_logerrordesc = "*ERROR Logger* [" + ex.Message + "]<br />SQL: [" + strSql + "]";
                HttpContext.Current.Response.Write("Error inserting dblogger record.<br />" + ex.Message);
                HttpContext.Current.Response.Flush();
                retval = -1;
            }
			cn.dbdisconnect();
			
			return (retval);
		}

        //	This will update a record in the web.dblogger table to log the program finish
        public int Finish()
        {
            string strSql;
            int retval = 0;
            dbconnect cn;

            // swsapr commented our 2023/06/03
            //StreamWriter swsapr = new StreamWriter("D:\\extractlogs\\DBFinish.txt", true);
            //DateTime curTime = DateTime.Now;
            //string log_time = curTime.ToString("yyyy/MM/dd HH:mm:ss");
            //swsapr.WriteLine(log_time);

            // truncate desc if necessary (just for database - leave full string for display)
            string dbdesc = m_logerrordesc;
            int desclen = dbdesc.Length;
            if (desclen > 255) dbdesc = dbdesc.Substring(1, 255);

            if (m_logreturncode == 0 && m_logerrorcode == 0)
            {
                dbdesc = "Completed successfully";
            }

            //SQL:VIEW:web.dblogger_view
            strSql = "UPDATE web.dblogger_view " +
                "SET logfinishtime = GETDATE()," +
                "logreturncode = " + m_logreturncode + "," +
                "logerrorcode = " + m_logerrorcode + "," +
                "logerrordesc = '" + dbdesc + "' " + 
                "WHERE logserial = '" + m_logserial + "'";
            //swsapr.WriteLine(strSql);

            try
            {
                cn = new dbconnect();
            }
            catch (Exception ex)
            {
                m_logerrorcode = -97;
                m_logerrordesc = "Could not Connect to database: " + ex.Message;
                HttpContext.Current.Response.Write("Connection error.<br />" + ex.Message);
                HttpContext.Current.Response.Flush();
                //swsapr.WriteLine("Could not Connect to database: " + ex.Message);
                //swsapr.Close();
                return (-1);
            }

            //	Execute the update
            OdbcCommand oCommand;
            OdbcCommand oCommand1;
            try
            {
                oCommand = new OdbcCommand(strSql, cn.Connection);
                int reccount = oCommand.ExecuteNonQuery();

                if(reccount == 0)
                {
                    strSql = "select USER";
                    oCommand1 = new OdbcCommand(strSql, cn.Connection);
                    string sqluser = (string)oCommand1.ExecuteScalar();
                    //swsapr.WriteLine("USER = " + sqluser);
                    //swsapr.Close();
                    //return (-1);
                }
            }
            catch (Exception ex)
            {
                //	We had an error trying to update the record.
                m_logerrordesc = "*ERROR Logger* [" + ex.Message + "]<br />SQL: [" + strSql + "]";
                m_logerrorcode = -95;
                //swsapr.WriteLine("Could not update dblogger: " + ex.Message);
                //swsapr.Close();
                retval = -1;
            }
            cn.dbdisconnect();
            //swsapr.WriteLine("Dblogger updated");
            //swsapr.Close();
            return (retval);
        }
        //	This is the function to retrieve the return values from a function into an
		//	array.  The program running on the Alpha is expected to write its return
		//	values into the user's returnvalues table keyed on the designated key. 
		//	This function returns the values for that key into an array and returns the
		//	array.
		//
		//	The first call (getretvals) reads the values into an internal array and
		//	returns the count of the number read as the length of the array
		//	
		public static ArrayList getretvals(string cKey)
		{
			int							nInd;
			dbconnect				cn;
			OdbcDataReader	rs;
			ArrayList oRetVal = new ArrayList();
			
			try{
				cn = new dbconnect();
			} catch {
				return null;
			}

			rs = cn.odbcquery("Select * from returnvalues " +
								"where retkey='" + cKey + "' " +
								"order by retind");
			nInd = -1;
			while (rs.Read()){
				nInd++;
				oRetVal.Add(rs["retval"].ToString().Trim());
			}
			
			cn.dbdisconnect();
			
			return (oRetVal);		
		}
	}
}
