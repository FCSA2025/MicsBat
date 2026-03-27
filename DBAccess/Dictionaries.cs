
using System;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.SessionState;
using System.Xml;

namespace DBAccess
{
    public class UserTable
    {
        public string m_operator;
        public int tabletype;
        public string file_name;
        public string micsid;
        public string project_code;
        public string validstat;
        public DateTime create_date;
 
        public UserTable()
        {
            m_operator = "";
            tabletype = -99;
            file_name = "";
            micsid = "";
            project_code = "";
            validstat = "";
            //create_date = null;
        }

        public UserTable(string soperator, int itabletype, string sfile_name)
        {
            string cSQL;
            //SQL:VIEW:web.user_tables_view
            cSQL = "SELECT operator, tabletype, file_name, micsid, project_code, validstat, create_date " +
                   "FROM web.user_tables_view " +
                   "WHERE operator = '" + soperator +
                   "' AND tabletype = " + itabletype +
                   " AND file_name = '" + sfile_name + "'";

            dbconnect oCn = new dbconnect();
            try
            {
                DataTable oDT = oCn.retrieve(cSQL);
                if (oDT.Rows.Count > 0)
                {
                    //	Take the first row 
                    DataRow oDR = oDT.Rows[0];
                    m_operator = Convert.ToString(oDR["operator"]).Trim();
                    tabletype = Convert.ToInt32(oDR["tabletype"]);
                    file_name = Convert.ToString(oDR["file_name"]);
                    micsid = Convert.ToString(oDR["micsid"]);
                    project_code = Convert.ToString(oDR["project_code"]);
                    validstat = Convert.ToString(oDR["validstat"]);
                    create_date = Convert.ToDateTime(oDR["create_date"]);
                }
                else
                {
                    //	Indicate that the mics_users_table record not found.
                    m_operator = "";
                }
            }
            finally
            {
                oCn.dbdisconnect();
            }

        }

        public static string GetUserValidFlag(string soperator, int itabletype, string sfile_name)
        {
            string validt = "";
            string validu = "";
            string pdftitl = "";
            bool tses = false;
            string cSQL;

            // set table name for TS or ES type
            switch (itabletype)
            {
                case 0: // TS
                    pdftitl = "ft_" + sfile_name + "_titl";
                    tses = true;
                    break;

                case 5: // ES
                    pdftitl = "fe_" + sfile_name + "_titl";
                    tses = true;
                    break;

                default:
                    break;
            }

            dbconnect oCn = new dbconnect();

            if (tses) // ts or es file - get validated value from titl record
            {
                cSQL = "SELECT validated FROM " + soperator + "." + pdftitl;
                DataTable oDT1 = oCn.retrieve(cSQL);
                if (oDT1.Rows.Count > 0)
                {
                    //	Take the first row 
                    DataRow oDR1 = oDT1.Rows[0];
                    validt = Convert.ToString(oDR1["validated"]);
                }
            }

            // get validstat from web.user_tables_view
            //SQL:VIEW:web.user_tables_view
            cSQL = "SELECT validstat FROM web.user_tables_view " +
                   " WHERE operator = '" + soperator.Trim() +
                   "' AND tabletype = " + itabletype +
                   " AND file_name = '" + sfile_name.Trim() + "'";


            DataTable oDT2 = oCn.retrieve(cSQL);
            if (oDT2.Rows.Count > 0)
            {
                //	Take the first row 
                DataRow oDR2 = oDT2.Rows[0];
                validu = Convert.ToString(oDR2["validstat"]);
            }

            // if we got a value from a titl record, then check if it matches user_tables value
            // this should not be necessary, but running a TS or ES validate from the command
            // prompt will not update the user_tables value
            if (validt != "")
            {
                if (validt != validu) // if not, update user_tables to match
                {
                    if (SetUserValidFlag(soperator, itabletype, sfile_name, validt))
                    {
                    }
                }
                oCn.dbdisconnect();
                return validt;
            }
            else
            {
                oCn.dbdisconnect();
                return validu;
            }
        }
        public static bool SetUserValidFlag(UserTable uTable, string cvstat)
        {
            bool retval = true;
         
            string cSQL;
            //SQL:VIEW:web.user_tables_view
            cSQL = "UPDATE web.user_tables_view " +
                   "SET validstat = '" + cvstat + 
                   "' WHERE operator = '" + uTable.m_operator.Trim() +
                   "' AND tabletype = " + uTable.tabletype +
                   " AND file_name = '" + uTable.file_name.Trim() + "'";

            dbconnect oCn = new dbconnect();
            
            try
            {
                OdbcCommand oUpdate = new OdbcCommand(cSQL,oCn.Connection);
                oUpdate.ExecuteNonQuery();
            }
            catch (Exception)
            {
                retval = false;
            }

            oCn.dbdisconnect();
            return retval;
        }
        public static bool SetUserValidFlag(string soperator, int itabletype, string sfile_name, string cvstat)
        {
            bool retval = true;

            string cSQL;
            //SQL:VIEW:web.user_tables_view
            cSQL = "UPDATE web.user_tables_view " +
                   "SET validstat = '" + cvstat +
                   "' WHERE operator = '" + soperator.Trim() +
                   "' AND tabletype = " + itabletype +
                   " AND file_name = '" + sfile_name.Trim() + "'";

            dbconnect oCn = new dbconnect();

            try
            {
                OdbcCommand oUpdate = new OdbcCommand(cSQL, oCn.Connection);
                oUpdate.ExecuteNonQuery();
            }
            catch (Exception)
            {
                retval = false;
            }

            oCn.dbdisconnect();
            return retval;
        }
        public bool insert_user_table(UserTable uTable)
        {
            bool retval = true;

            string cSQL;
            //SQL:VIEW:web.user_tables_view
            cSQL = "INSERT INTO web.user_tables_view " +
                   "(operator, tabletype, file_name, micsid, project_code, validstat, create_date) " +
                   "VALUES ('" + uTable.m_operator + "'," + uTable.tabletype + ",'" + uTable.file_name + "','" + uTable.micsid + "','" +
                   uTable.project_code + "','N',getdate())";

            dbconnect oCn = new dbconnect();

            try
            {
                OdbcCommand oInsert = new OdbcCommand(cSQL);
                oInsert.ExecuteNonQuery();
            }
            catch (Exception)
            {
                retval = false;
            }

            oCn.dbdisconnect();
            return retval;
        }
    }
}
