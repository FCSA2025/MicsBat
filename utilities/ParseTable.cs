using System.Text.RegularExpressions;
using System.Web;

namespace ParseTable
{
    // this class parses out the table name used in a SQL query
    // it assumes there are only 5 possible sql statements which 
    // will be used:
    // DROP TABLE
    // DELETE FROM xxx
    // CREATE TABLE xxx
    // UPDATE xxx
    // INSERT INTO xxx
    // SELECT .. INTO xxx
    public class ParseSql
    {
        static public string SqlParse(string strSql)
        {
            string tablename = "";
            // eliminate extra whitespaces
            strSql = Regex.Replace(strSql, "\\s+", " ");

            if (strSql.ToUpper().IndexOf("DELETE FROM") > 0) // distinct from select from
            {
                tablename = ParseCmd(strSql, "FROM");
            }
            if (strSql.ToUpper().IndexOf("TABLE") > 0)  // drop or delete
            {
                tablename = ParseCmd(strSql, "TABLE");
            }
            if (strSql.ToUpper().IndexOf("UPDATE") >= 0)
            {
                tablename = ParseCmd(strSql, "UPDATE");
            }
            if (strSql.ToUpper().IndexOf("INTO") > 0)  // insert or select
            {
                tablename = ParseCmd(strSql, "INTO");
            }

            // check if schema/table name is valid
            return CheckTableName(tablename);
        }
        static string ParseCmd(string strSql, string comtype)
        {
            // split sql command into separate words
            char[] delimiter = " ".ToCharArray();
            string[] sql_words = strSql.Split(delimiter);

            // loop looking for match on comtype - next word should be table name
            for (int i = 0; i < sql_words.Length; i++)
            {
                if (sql_words[i].ToUpper() == comtype)
                {
                    if (i == sql_words.Length - 1)
                    {
                        return "";
                    }
                    else
                    {
                        return sql_words[i + 1];
                    }
                }
            }
            return "";
        }
        static string CheckTableName(string tablename)
        {
            // this function checks table name and optionally schema (if specified)
            // if tablename is blank or OK for update if returns "0"
            // if tablename is in restricted list it returns "1:<tablename>
            // if schema does not match users' it returns "2:<schema>

            HttpContext ctx = HttpContext.Current;

            if (tablename == "")
            {
                return "0";
            }

            // split table name on '.' in case schema specified
            char[] delimiter = ".".ToCharArray();
            string[] tableParts = tablename.Split(delimiter);

            // if schema specified, check if it matches users'
            if (tableParts.Length == 2) // input is <schema>.<table>
            {
                if (tableParts[0].ToLower() != ctx.Session["s_schema"].ToString())
                {
                    return "2:" + tableParts[0].ToLower();
                }
            }

            // check that tablename not in restricted list
            string tabname = tableParts[tableParts.Length - 1].ToLower();

            bool retval = true;
            if (tabname.IndexOf("cull_") == 0) retval = false;
            if (tabname.IndexOf("ft_") == 0) retval = false;
            if (tabname.IndexOf("fe_") == 0) retval = false;
            if (tabname.IndexOf("su_") == 0) retval = false;
            if (tabname.IndexOf("te_") == 0) retval = false;
            if (tabname.IndexOf("tp_") == 0) retval = false;
            if (tabname.IndexOf("tsip_") == 0) retval = false;
            if (tabname.IndexOf("tt_") == 0) retval = false;
            if (tabname.IndexOf("ptrtable") == 0) retval = false;
            if (tabname.IndexOf("returnvalues") == 0) retval = false;
            if (tabname.IndexOf("savedquery") == 0) retval = false;

            if (retval)
            {
                return "0";
            }
            else
            {
                return "1:" + tablename;
            }
        }

    }
}
