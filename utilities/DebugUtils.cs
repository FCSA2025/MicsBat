using System.Data.Odbc;
using System.IO;
using DBUtilities;

namespace DebugUtilities
{
    public class DebugUtils
    {
        public static string SetDebugOutput(string cn_str)
        {
            // this routine determines the directory to which to write debug files
            //
            // if the table web.debugdirectory does not exist in the database,
            //   it assumes output is to be written to <webdrive>:\extractlogs
            // if the table web.debuglogs exists in the database read its contents to determine the
            //   debug directory
            // there will only be at most one recoord in this filen

            // check debug directory value for this site

            //SQL:TABLE:web.debugdirectory
            string strSql = "SELECT debug_directory FROM web.debugdirectory";

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {
                cn.Open();

                OdbcCommand select = new OdbcCommand(strSql, cn);
                OdbcDataReader dr1;

                try
                {
                    dr1 = select.ExecuteReader();
                }
                catch
                {
                    // select will fail if table does not exist
                    return "D:\\\\extractlogs";
                }

                if (dr1.HasRows)
                {
                    dr1.Read();
                    // return value of debug_directory
                    //return dr1.GetString(0);
                    return DBUtils.GetDBString(dr1, 0);
                }
                else
                {
                    // record not found 
                    return "D:\\\\extractlogs";
                }
            }
        }
        public static int SetDebugFlag(string ModuleName, string micsid, string cn_str)
        {
            // this routine checks if debug info is to be written to file determined by previous call to SetDebugOutput()
            //
            // if the table web.debuglogs does not exist in the database, it assumes no output is to be written
            // if the table web.debuglogs exists in the database for this module and user, and the value of debugflag is 0, no output is to be written
            // if the table web.debuglogs exists in the database for this module and user, and the value of debugflag is > 0, output is to be written

            // check debug value for this module and user
            //SQL:TABLE:web.debuglogs
            string strSql = " SELECT debugflag FROM web.debuglogs WHERE debugmodule ='" + ModuleName + "' AND micsid = '" + micsid + "'";

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {
                cn.Open();

                OdbcCommand select = new OdbcCommand(strSql, cn);
                OdbcDataReader dr1;

                try
                {
                    dr1 = select.ExecuteReader();
                }
                catch
                {
                    // select will fail if table does not exist
                    return 0;
                }

                if (dr1.HasRows)
                {
                    dr1.Read();
                        // return value of debugflag
                        return dr1.GetInt32(0);
                }
                else
                {
                    // record not found for this user/debug module
                    return 0;
                }
            }
        }
        public static void WriteDebug(int diagflag, StreamWriter sw, string instring)
        {

            if (diagflag > 0)
            {
                sw.WriteLine(instring);
            }
        }
        public static void WriteDebugFlush(int diagflag, StreamWriter sw, string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
                sw.Flush();
            }
        }
        public static void WriteDebugClose(int diagflag, StreamWriter sw, string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
                sw.Close();
            }
        }
    }

}

