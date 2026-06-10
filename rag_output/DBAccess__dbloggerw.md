# Documented File: dbloggerw.cs
**Repository Path:** `DBAccess\dbloggerw.cs`
**Primary Layer:** `DBAccess`
**Namespace:** `DBAccess`

## Source Code Representation
```csharp
using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Data.Odbc;

namespace DBAccess
{
	/// <summary>
	/// Summary description for dbloggerw.
	/// </summary>
	public class dbloggerw
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

        public dbloggerw (string prog, string args)
		{
            HttpContext ctx = HttpContext.Current;

			//	This is the constructor for a dbloggerw object.  It must be called with the new
			//	operator.
            ////m_logserial = ctx.Session["FCSASESS"].ToString() + "-" + OELSupport.nextit();
            m_logserial = "1-1";
            //m_logstarttime = Now();
            ////m_loguserid = ctx.Session["s_user"].ToString();
            m_loguserid = "venn1";
			////m_logpcode = ctx.Session["defProject"].ToString();
			m_logpcode = "FCDS603";
			m_logprogram = prog;
			m_logargs = args;
			//m_logfinishtime = null;
			m_logreturncode = 0;
            m_logerrorcode = 0;
			m_logerrordesc = "";			
		}
		
		
		public dbloggerw(string prog):this (prog, ""){}
		
		//	This will add a record to the web.dblogger table
		public int Start()
		{
            return 0;
			/*
             * string	strSql;
            int retval = 0;
            ////dbconnect cn;

            string sessConnStr = "DSN=fcsa;DATABASE=fcsa;trustedconnection=true";

            // open connection
            OdbcConnection oConnection = new OdbcConnection(sessConnStr);
            oConnection.Open();

            // for test purposes only, delete prior logger record
            strSql = "DELETE web.dblogger where logserial='1-1'";
            ////cn = new dbconnect();

            //	Execute the deletion
            OdbcCommand oCommand;
            oCommand = new OdbcCommand(strSql, oConnection);
            oCommand.ExecuteNonQuery();

            strSql = "INSERT INTO web.dblogger " +
                "( logserial, logstarttime, loguserid, logpcode, logprogram, logargs) " +
			    " VALUES ('" +
                m_logserial + "'," +
			    "GETDATE(),'" +
			    m_loguserid + "','" + m_logpcode + "','" + m_logprogram +
                "','" + m_logargs + "')";
  

	        //	Execute the insertion
			////try {
				oCommand = new OdbcCommand(strSql, oConnection);
				oCommand.ExecuteNonQuery();
			////} catch (Exception ex) {
			////	//	We had an error trying to insert the record.
            ////    m_logerrorcode = -96;
            ////    m_logerrordesc = "*ERROR Logger* [" + ex.Message + "]<br />SQL: [" + strSql + "]";
            ////    HttpContext.Current.Response.Write("Error inserting dblogger record.<br />" + ex.Message);
            ////    HttpContext.Current.Response.Flush();
            ////    retval = -1;
            ////}
            oConnection.Close();
			
			return (retval);
*/
        }


        //	This will update a record in the web.dblogger table to log the program finish
        public int Finish()
        {
            return 0;
            /*
             * string strSql;
            int retval = 0;
            dbconnect cn;

            // truncate desc if necessary (just for database - leave full string for display)
            string dbdesc = m_logerrordesc;
            int desclen = dbdesc.Length;
            if (desclen > 255) dbdesc = dbdesc.Substring(1, 255);

            if (m_logreturncode == 0 && m_logerrorcode == 0)
            {
                dbdesc = "Completed successfully";
            }

            strSql = "UPDATE web.dblogger " +
                "SET logfinishtime = GETDATE()," +
                "logreturncode = " + m_logreturncode + "," +
                "logerrorcode = " + m_logerrorcode + "," +
                "logerrordesc = '" + dbdesc + "' " + 
                "WHERE logserial = '" + m_logserial + "'";

            ////cn = new dbconnect();
            string sessConnStr = "DSN=fcsa;DATABASE=fcsa;trustedconnection=true";

            // open connection
            OdbcConnection oConnection = new OdbcConnection(sessConnStr);
            oConnection.Open();
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
                return (-1);
            }
           

            //	Execute the update
            OdbcCommand oCommand;
            try
            {
                oCommand = new OdbcCommand(strSql, oConnection);
                oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //	We had an error trying to update the record.
                m_logerrordesc = "*ERROR Logger* [" + ex.Message + "]<br />SQL: [" + strSql + "]";
                m_logerrorcode = -95;
                retval = -1;
            }
            ////cn.dbdisconnect();
            oConnection.Close();

            return (retval);
            */
        }
	}
}

```
