using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Data.Odbc;


namespace DBAccess
{
    public class FileStorage
    {
        public string oper;
        public string micsid;
        public string file_name;
        public string storage_date;
        public int total_space;
        public DateTime create_date;
        public string project_code;

        public FileStorage()
        {
            oper = "";
            micsid = "";
            file_name = "";
            storage_date = "";
            total_space = 0;
            project_code = "";
        }

       
        public static bool ClearFileStorageTemp()
        {
            dbconnect oCn = new dbconnect();
            string cSQL = "DELETE FROM bi_filestorage_temp";

            OdbcCommand truncate = new OdbcCommand(cSQL, oCn.Connection);

            try
            {
                truncate.ExecuteNonQuery();
                oCn.dbdisconnect();
                return true;
            }
            catch (Exception)
            {
                oCn.dbdisconnect();
                return false;
            }
        }

        public static int CheckFileStorageDate(string effective_date)
        {
            int existcount = 0;

            dbconnect oCn = new dbconnect();
            string strSql = "SELECT COUNT(*) FROM acct.daily_storage WHERE storage_date = '" + effective_date + "'";
            OdbcCommand odbc1 = new OdbcCommand(strSql, oCn.Connection);

            try
            {
                existcount = (int)odbc1.ExecuteScalar();
                oCn.dbdisconnect();
            }
            catch (Exception)
            {
                oCn.dbdisconnect();
                return 1;
            }

            if (existcount > 0)
            {
                return 2;
            }
            else
            {
                return 0;
            }

        }
        public static string InsertFileStorageTemp(FileStorage fileInfo)
        {
            dbconnect oCn = new dbconnect();

            string cSQL = "SET DateFormat YMD; INSERT INTO acct.filestorage_temp " +
                          "VALUES('" + 
                          fileInfo.oper + "','" +
                          fileInfo.micsid + "','" +
                          fileInfo.file_name + "','" + 
                          fileInfo.storage_date + "'," +
                          fileInfo.total_space + ",GETDATE(),'" +
                          fileInfo.project_code + "')";

            OdbcCommand oCommand = new OdbcCommand(cSQL, oCn.Connection);

            try
            {
                oCommand.ExecuteNonQuery();
                oCn.dbdisconnect();
                return "OK";
            }
            catch( Exception ex)
            {
                oCn.dbdisconnect();
                return "ERROR" + cSQL + ex.Message;
            }
        }
    }

    public class SessionLog
    {
        public string sessionid;
        public string company;
        public string micsid;
        public string logintime;

        SessionLog()
        {
            sessionid = "";
            company = "";
            micsid = "";
        }

        public static DataTable SessionLogs()
        {
            // get currently active sessions
            HttpApplication locApp = new HttpApplication();
            locApp = (HttpApplication)HttpContext.Current.ApplicationInstance;

            char[] delimiter = ",".ToCharArray();
            string[] loc_session_array = locApp.Application["sessions"].ToString().Split(delimiter);

            string where_list = "WHERE sessionid NOT IN (";
            string comma = "";

            for (int i = 0 ; i < loc_session_array.Length ; i++)
			{
			    where_list += comma + "'" + loc_session_array[i] + "'";
				comma = ",";
            }
                
            where_list += ")";

            dbconnect oCn = new dbconnect();
            //SQL:TABLE:web.mthly_connect
            string cSQL = "SELECT sessionid AS SessionID, company AS Company, micsid AS MicsUser, logintime AS LoginTime FROM web.mthly_connect " +
                          where_list + " ORDER BY logintime ";

            DataTable oDT = null;
            try
            {
                oDT = oCn.retrieve(cSQL);
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return oDT;

        }
    
        public static bool DeleteCullsES(string schema, string sSessionid)
        {
            dbconnect oCn = new dbconnect();

            // clear any cull temp info
            if (!ClearInfo(schema + ".cull_temp1_es", sSessionid)) { return false; }
            if (!ClearInfo(schema + ".cull_temp2_es", sSessionid)) { return false; }
            if (!ClearInfo(schema + ".cull_temp3_es", sSessionid)) { return false; }

            return true;
        }
        public static bool DeleteCullsTS(string schema, string sSessionid)
        {
            dbconnect oCn = new dbconnect();

            // clear any cull temp info
            if (!ClearInfo(schema + ".cull_temp1", sSessionid)) { return false; }
            if (!ClearInfo(schema + ".cull_temp2", sSessionid)) { return false; }
            if (!ClearInfo(schema + ".cull_temp3", sSessionid)) { return false; }

            return true;
        }
        public static bool DeleteSessionLog(string sSessionid)
        {
            dbconnect oCn = new dbconnect();

            // clear entry from sessionlog
            if (!ClearInfo("web.mthly_connect", sSessionid.Trim())) { return false; }

            return true;
        }
        public static bool ClearInfo(string stablename, string sSessionid)
        {
            bool retval = false;

            dbconnect oCn = new dbconnect();

            // clear info from specified cull_temp table for specified session

            string strSql;

            strSql = "DELETE from " + stablename + " WHERE sessionid = '" + sSessionid + "'";

            OdbcCommand delete = new OdbcCommand(strSql, oCn.Connection);

            try
            {
                delete.ExecuteNonQuery();
                retval = true;
            }
            finally
            {
                oCn.dbdisconnect();
            }
            return retval;

        }
    }
}
