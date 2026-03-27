using System;
using System.Data;
using System.Data.Odbc;
using System.Web;

namespace DBUtilities
{
    public class DBUtils
    {
        // this line added to test new release procedures
        public static OdbcConnection open_connection(string cnstr, int opentype, out string status)
        {
            // opentype = 0: no read/write restrictions
            // opentype = 1: open for read
            // opentype = 2: open for write

            OdbcConnection cn = new OdbcConnection(cnstr);
            status = "";

            switch (opentype)
            {
                case 0: // no restrictions
                    try
                    {
                        cn.Open();
                        status = "OK";
                    }
                    catch (Exception ex0)
                    {
                        cn = null;
                        status = "ERROR: " + ex0.Message;
                    }
                    break;

                case 1: // read
                    try
                    {
                        // close read gate
                        cn.Open();
                        status = "OK";
                    }
                    catch (Exception ex0)
                    {
                        cn = null;
                        status = "ERROR: " + ex0.Message;
                    }
                    break;

                case 2: // write
                    try
                    {
                        // close write gate
                        cn.Open();
                        status = "OK";
                    }
                    catch (Exception ex0)
                    {
                        cn = null;
                        status = "ERROR: " + ex0.Message;
                    }
                    break;

                default:
                    break;
            }
            return cn;

        }
        public static string close_connection(OdbcConnection cn, int closetype)
        {
            // closetype = 0: no read/write restrictions
            // closetype = 1: close after read
            // closetype = 2: open after write

            string status = "";

            switch (closetype)
            {
                case 0: // no restrictions
                    try
                    {
                        cn.Close();
                        status = "OK";
                    }
                    catch (Exception ex0)
                    {
                        status = "ERROR: " + ex0.Message;
                    }
                    break;

                case 1: // read
                    try
                    {
                        // open read gate
                        cn.Close();
                        status = "OK";
                    }
                    catch (Exception ex0)
                    {
                        cn = null;
                        status = "ERROR: " + ex0.Message;
                    }
                    break;

                case 2: // write
                    try
                    {
                        // open write gate
                        cn.Close();
                        status = "OK";
                    }
                    catch (Exception ex0)
                    {
                        cn = null;
                        status = "ERROR: " + ex0.Message;
                    }
                    break;

                default:
                    break;
            }
            return status;

        }
        public static string GetDBFloat(OdbcDataReader dr, int fldnum, int decimals)
        {
            string format = "f" + decimals;
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetFloat(fldnum).ToString(format);
            }
            else
            {
                return "";
            }
        }
        public static string GetDBDouble(OdbcDataReader dr, int fldnum, int decimals)
        {
            string format = "f" + decimals;
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetDouble(fldnum).ToString(format);
            }
            else
            {
                return "";
            }
        }
        public static string GetDBString(OdbcDataReader dr, int fldnum)
        {
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetString(fldnum).Trim();
            }
            else
            {
                return "";
            }

        }
        public static string GetDBTString(OdbcDataReader dr, int fldnum)
        { // used only by trees - returns '-' for null field
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetString(fldnum).Trim();
            }
            else
            {
                return "-";
            }

        }
        public static string GetDBInt8(OdbcDataReader dr, int fldnum)
        {
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetByte(fldnum).ToString();
            }
            else
            {
                return "";
            }

        }
        public static string GetDBInt16(OdbcDataReader dr, int fldnum)
        {
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetInt16(fldnum).ToString();
            }
            else
            {
                return "";
            }

        }
        public static string GetDBInt32(OdbcDataReader dr, int fldnum)
        {
            if (!dr.IsDBNull(fldnum))
            {
                return dr.GetInt32(fldnum).ToString();
            }
            else
            {
                return "";
            }

        }
        public static string txtMonth(string numMonth)
        {
            string[] monthArray = new string[] { "null", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            if (numMonth == "")
            {
                return "";
            }
            else
            {
                int ptr = Int32.Parse(numMonth);
                if (ptr > 0 && ptr < 13)
                {
                    return monthArray[ptr];
                }
                else
                {
                    return "";
                }
            }
        }
        public static string intMonth(string txtMonth)
        {
            string[] monthArray = new string[] { "null", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            if (txtMonth == "")
            {
                return "";
            }
            else
            {
                for (int ptr = 1; ptr < 13; ptr++)
                {
                    if (txtMonth == monthArray[ptr])
                    {
                        return ptr.ToString("D2");
                    }
                }
            }
            return "";
        }
        public static string chNull(string instring)
        {
            // function called by sql saves to replace empty string with null
            instring = instring.Trim();
            if (instring == "")
            {
                instring = "NULL";
            }
            else
            {
                instring = "'" + instring + "'";
            }
            return instring;
        }
        public static string numNull(string number)
        {
            number = number.Trim();
            if (number == "")
            {
                number = "null";
            }
            return number;
        }
        public static string checksDate(string sDateDD, string sDateMM, string sDateYY)
        {// check that all or none of sDate fields are/is filled in 
            string retval = "OK";
            if (sDateDD != "" || sDateMM != "" || sDateYY != "") // at least one field filled 
            {
                if (sDateDD == "")
                {
                    retval = "You must enter a day value for service date";
                }
                if (sDateMM == "")
                {
                    retval = "You must enter a month value for service date";
                }
                if (sDateYY == "")
                {
                    retval = "You must enter a year value for service date";
                }
            }
            return retval;
        }
        public static string checkaDate(string aDateDD, string aDateMM, string aDateYY)
        {// check that all or none of aDate fields are/is filled in 
            string retval = "OK";
            if (aDateDD != "" || aDateMM != "" || aDateYY != "") // at least one field filled 
            {
                if (aDateDD == "")
                {
                    retval = "You must enter a day value for approved date";
                }
                if (aDateMM == "")
                {
                    retval = "You must enter a month value for approved date";
                }
                if (aDateYY == "")
                {
                    retval = "You must enter a year value for approved date";
                }
            }
            return retval;
        }
        public static string dates(string dd, string mm, string yy)
        {
            // this routine is called when saving dates, and returns either null or
            // the date in single quotes for insertion/update of record
            string numMonth = intMonth(mm);

            string val = "";
            if (dd.ToUpper() == "DD" || mm.ToUpper() == "MMM" || yy.ToUpper() == "YYYY")
            {
                val = "null";

            }
            else if (dd == "" || mm == "" || yy == "")
            {
                val = "null";

            }
            else
            {
                val = "'" + yy + "." + numMonth + "." + dd + "'";
            }
            return val;
        }
        public static string lic(string val1, string val2)
        {
            string full;
            if (val1.Trim() == "" || val2.Trim() == "")
            {
                full = "NULL";
            }
            else
            {
                full = "'" + val1 + "-" + val2 + "'";
            }
            return full;
        }
        public static string charQuote(string instring)
        {
            //this function replaces single quote is string with a pair of single quotes
            instring = instring.Replace("'", "''");
            return instring;
        }
        public static string charQuoteHtml(string instring)
        {
            //this function replaces single quote is string with a pair of single quotes
            instring = instring.Replace("'", "&#39");
            return instring;
        }
        public static string resetamp(string instring)
        {
            // this function replaces circonflex in string with an ampersand
            // it is used in datasearches to cope with ampersands in criteria value
            instring = instring.Replace("^", "&");
            return instring;
        }
        public static string escapes(string inString)
        //	Adds escape key ("\") to % and _ where they are valid characters
        {
            string outString;
            if (inString != "") // should always be true in this context
            {
                outString = "";
                bool escape = false;

                //Loop through the inputted string to see if a wild card has been entered
                for (int i = 0; i < inString.Length; i++)
                {
                    switch (inString[i])
                    {
                        case '%'://% (37)
                            outString = outString + "\\%";
                            escape = true;
                            break;
                        case '_'://_ (95)
                            outString = outString + "\\_";
                            escape = true;
                            break;
                        default:
                            outString = outString + inString[i];
                            break;
                    }
                }
                //Add Single Quotes and/or Escape keys to our string
                if (escape == false)
                {
                    outString = "'" + outString + "%'";
                }
                else // escape == true
                {
                    outString = "'" + outString + "%' ESCAPE '\\'";
                }
            }
            else
            {
                outString = "'%'";
            }
            //Return value ready for the database
            return outString.ToUpper();
        }
        public static string padfield(string instring, int outlength)
        {
            string master = "..................................................";
            string outstring = master.Substring(0, outlength);
            string locstring = instring;
            int inlen;

            while (locstring.IndexOf("  ") >= 0)
            {
                locstring = locstring.Replace("  ", " ");
            }
            inlen = locstring.Length;

            if (inlen < outlength) // string not full
            {
                outstring = locstring + master.Substring(0, outlength - inlen);
                return outstring;
            }
            else // string already full - just return it 
            {
                return locstring;
            }
        }
        public static string padleft(string instring, int outlength)
        {
            // this function is used to assure alignment of columns in lookup screens
            // it returns a string with instring preceded by optional
            // blanks (&nbsp;) to fill a string of length outlength
            // if instring is longer than outlength, it returns a string of 
            // length outstring with asterisks

            string stars = "**************************************************";
            string outstring;
            int inlen;

            inlen = instring.Length;
            outstring = "";

            if (inlen > outlength) // string too long for display
            {
                outstring = stars.Substring(0, outlength);
                return outstring;
            }

            if (inlen < outlength) // string not full
            {
                // fill blanks on left
                for (int i = 0; i < (outlength - inlen); i++)
                {
                    outstring = outstring + "&nbsp;";
                }
                outstring = outstring + instring;
                return outstring;
            }
            else // string already full - just return it 
            {
                return instring;
            }
        }
        public static string padright(string instring, int outlength)
        {
            // this function is used to assure alignment of columns in lookup screens
            // it returns a string with instring followed by optional
            // blanks (&nbsp;) to fill a string of length outlength
            // if instring is longer than outlength, it returns a string of 
            // length outstring with asterisks

            string stars = "**************************************************";
            string outstring;
            int inlen;

            inlen = instring.Length;
            outstring = "";

            if (inlen > outlength) // string too long for display
            {
                outstring = stars.Substring(0, outlength);
                return outstring;
            }

            if (inlen < outlength) // string not full
            {
                outstring = instring;
                // fill blanks on right
                for (int i = 0; i < (outlength - inlen); i++)
                {
                    outstring = outstring + "&nbsp;";
                }
                return outstring;
            }
            else // string already full - just return it 
            {
                return instring;
            }
        }
        public static void parsedate(string indate, out string outyear, out string outmonth, out string outday)
        {
            if (indate.Length == 10 && indate != "          ")
            {
                char[] datedelimiter = ".".ToCharArray();
                string[] date_parts = indate.Split(datedelimiter);
                outday = date_parts[2]; //  mDay 
                outmonth = DBUtils.txtMonth(date_parts[1]);// mMonth
                outyear = date_parts[0]; //  mYear
            }
            else
            {
                outday = ""; //  mDay 
                outmonth = "";// mMonth
                outyear = ""; //  mYear
            }
        }
        public static void parsedateISO(string indate, out string outstring)
        {
            outstring = "";

            if (indate.Length >= 8 && !String.IsNullOrEmpty(indate))
            {
                char[] datedelimiter = "/".ToCharArray();
                string[] date_parts = indate.Split(datedelimiter);
                if (date_parts.Length != 3)
                {
                    outstring = "ERROR";
                    return;
                }
                string outday = date_parts[2]; //  strDay 
                string outmonth = date_parts[1];// strMonth
                string outyear = date_parts[0]; // strYear

                int intday = 0;
                int intmon = 0;
                int intyear = 0
                    ;
                bool valid = false;

                // check that all fields are numeric
                if (int.TryParse(outyear, out intyear))
                {
                    if (int.TryParse(outmonth, out intmon))
                    {
                        if (int.TryParse(outday, out intday))
                        {

                            valid = true;
                        }
                    }
                }

                // if all fields are numeric, check for range values
                if (intyear <= 1975 || intyear >= 2050)
                {
                    valid = false;
                }
                if (intmon < 1 || intmon > 12)
                {
                    valid = false;
                }
                if (intday < 1 || intday > 31)
                {
                    valid = false;
                }
                if (intday > 30 && !(intmon == 1 || intmon == 3 || intmon == 5 || intmon == 7 || intmon == 8 || intmon == 10 || intmon == 12))
                {
                    valid = false;
                }
                if (intday > 29 && intmon == 2)
                {
                    valid = false;
                }
                if (intday == 29 && intmon == 2 && !(intyear % 4 == 0))
                {
                    valid = false;
                }

                if (valid)
                {
                    outstring = outyear + "/" + intmon.ToString("D2") + "/" + intday.ToString("D2");
                }
                else
                {
                    outstring = "ERROR";
                }
            }
            else
            {
                outstring = "ERROR";
            }
        }
        public static string copy_RC_table(OdbcConnection cn, string cTable)
        {
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " FROM tabledef.master_" + cTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'tabledef' AND TABLE_NAME = master.'" +
                        cTable +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " ADD CONSTRAINT PK_" + cTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        /// <summary>
        /// This method will check the database to see if an fcc table exists in the current users' schema.  It does this
        ///	by checking for it in information_schema.  
        /// </summary>
        /// <param name="cTable">The full name of the table we are checking for.</param>
        /// <returns>true if the table exists, false otherwise</returns>
        public static bool tableexistsfccuser(string cTable)
        {
            // this routine is used by fcc module to determine if table exists in the users' schema
            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cTable.ToLower() +
                          "' and table_schema='" + ctx.Session["s_schema"].ToString() + "'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static bool tableexistsuser(string cTable)
        {
            // this routine is used to determine if table exists in the users' schema
            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cTable.ToLower() +
                          "' and table_schema='" + ctx.Session["s_schema"].ToString() + "'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static bool tableexistsfccfcc(string cTable)
        {
            // this routine is used by fcc module to determine if table exists in the fcc schema
            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cTable.ToLower() +
                          "' and table_schema='fcc'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static bool tableexistsisedised(string cTable)
        {
            // this routine is used by ised module to determine if table exists in the ised schema
            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cTable.ToLower() +
                          "' and table_schema='ised'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static bool tableexistsreleases(string cTable)
        {
            // this routine is used by release module to determine if table exists in the releases schema
            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cTable.ToLower() +
                          "' and table_schema='releases'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static bool tableexistsimport(string importSchema, string cTable)
        {
            // this routine is used by fcc, ised or comsearch module to determine if table exists in the <importSchema>
            // importSchema will be "fcc" or "ised"

            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cTable.ToLower() +
                          "' and table_schema='" + importSchema + "'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static bool viewexistsfccuser(string cView)
        {
            // this routine is used by fcc module to determine if view exists in the users' schema
            bool IsPresent;
            int nCount;

            HttpContext ctx = HttpContext.Current;
            string cnstr = ctx.Session["s_cnString"].ToString();
            OdbcConnection cn = new OdbcConnection(cnstr);
            cn.Open();

            string cSQL = "SELECT count(*) " +
                          "from INFORMATION_SCHEMA.TABLES " +
                          "where table_name='" + cView.ToLower() +
                          "' and table_schema='" + ctx.Session["s_schema"].ToString() + "'";
            try
            {
                OdbcCommand oCmd = new OdbcCommand(cSQL, cn);
                nCount = Convert.ToInt32(oCmd.ExecuteScalar());

            }
            catch
            {
                nCount = 0;
            }

            IsPresent = nCount > 0;

            return IsPresent;
        }
        public static string copy_table(OdbcConnection cn, string cTable)
        {
            // this routine copies table cTable to users' schema from tabledef.<cTable>def
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " FROM tabledef." + cTable + "def";

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            //THIS BLOCK REPLACED 2022/12/27 to replace reference to fcc
            // add primary key constraint if present in source table
            //strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
            //            " WHERE TABLE_SCHEMA = 'fcc' AND TABLE_NAME = '" +
            //            cTable + "def' " +
            //            "ORDER BY ORDINAL_POSITION";

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'tabledef' AND TABLE_NAME = '" +
                        cTable + "def' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script MODIFIED 2022/12/27 to replace fcc reference
                //strSql = "ALTER TABLE fcc." + cTable +
                //         " ADD CONSTRAINT PK_" + cTable + " PRIMARY KEY CLUSTERED (";

                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " ADD CONSTRAINT PK_" + cTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_fcc_table(OdbcConnection cn, string cTable)
        {
            // this routine creates a table in the user's schema based on a table on the same name in fcc with a 'def' suffix
            // i.e. fcc.<cTable>def is copied to <user>.<cTable> 
            // the fcc.<cTable>def is always an empty table serving as a template
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " FROM fcc." + cTable + "def";

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'fcc' AND TABLE_NAME = '" +
                        cTable + "def' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " ADD CONSTRAINT PK_" + cTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_import_table(OdbcConnection cn, string importSchema, string cTable)
        {
            // this routine creates a table in the user's schema based on a table on the same name in importSchema with a 'def' suffix
            // i.e. <importSchema>.<cTable>def is copied to <user>.<cTable> 
            // the <importSchema>.<cTable>def is always an empty table serving as a template
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " FROM " + importSchema + "." + cTable + "def";

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = '" + importSchema + "' AND TABLE_NAME = '" +
                        cTable + "def' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + cTable +
                         " ADD CONSTRAINT PK_" + cTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_fcc_table2(OdbcConnection cn, string targetTable, string sourceTable)
        {
            // this function copies source table from fcc schema to user's schema
            // it is used to create a user table when the source table template in fcc has a different name from the target table
            // i.e. table fcc.<sourceTable> is copied to <user>.targetTable
            // the fcc<sourceTable> is always empty and serves as a template
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " FROM fcc." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'fcc' AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_import_table2(OdbcConnection cn, string importSchema, string targetTable, string sourceTable)
        {
            // this function copies source table from <importSchema> to user's schema
            // it is used to create a user table when the source table template in <importSchema> has a different name from the target table
            // i.e. table <importSchema>.<sourceTable> is copied to <user>.<targetTable>
            // the <importSchema>.<sourceTable> is always empty and serves as a template
            // the value of <importSchema> is "fcc" or "ised" or "coms"
            // this function is also called by 2024 release process (with importSchema set to 'tabledef')
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " FROM " + importSchema + "." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = '" + importSchema + "' AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_releases_table2(OdbcConnection cn, string releasesSchema, string targetTable, string sourceTable)
        {
            // this function copies source table from <releasesSchema> to <releasesSchema>.targetTable
            // i.e. table <releasesSchema>.<sourceTable> is copied to <releasesSchema>.<targetTable>
            // the <releasesSchema>.<sourceTable> is always empty and serves as a template
            // the value of <releasesSchema> is always "releases"
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO releases." + targetTable + " FROM releases." + sourceTable;
            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'releases' AND TABLE_NAME = '" + sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE releases." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_fcc_table3(OdbcConnection cn, string targetTable, string sourceTable)
        {
            // this copies source table from user's schema to target table in user's schema
            // i.e. <user>.<sourceTable> is copied to <user>.targetTable
            // in practice the sourceTable usually contains data
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " FROM " + ctx.Session["s_schema"].ToString() + "." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = " + ctx.Session["s_schema"].ToString() + " AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_import_table3(OdbcConnection cn, string targetTable, string sourceTable)
        {
            // this copies source table from user's schema to target table in user's schema
            // i.e. <user>.<sourceTable> is copied to <user>.<targetTable>
            // in practice the sourceTable usually contains data
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " FROM " + ctx.Session["s_schema"].ToString() + "." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = " + ctx.Session["s_schema"].ToString() + " AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_fcc_table4(OdbcConnection cn, string targetTable, string sourceTable)
        {
            // this copies source table from fcc schema to target table in fcc schema
            // i.e. fcc.<sourceTable> is copied to fcc.targetTable
            // in practice the sourceTable is usually empty
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO fcc." + targetTable +
                         " FROM fcc." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'fcc' AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE fcc." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_ised_table4(OdbcConnection cn, string targetTable, string sourceTable)
        {
            // this copies source table from ised schema to target table in ised schema
            // i.e. ised.<sourceTable> is copied to ised.targetTable
            // in practice the sourceTable is usually empty
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO ised." + targetTable +
                         " FROM ised." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                         " WHERE TABLE_SCHEMA = 'ised' AND TABLE_NAME = '" +
                         sourceTable + "' " +
                         "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE ised." + targetTable +
                          " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";
        }
        public static string copy_import_table4(OdbcConnection cn, string sourceSchema, string targetTable, string sourceTable)
        {
            // this copies <sourceTable> from <sourceSchema> to <targetTable> in <sourceSchema>
            // <sourceSchema> is "fcc" or "ised"
            // i.e. <sourceSchema>.<sourceTable> is copied to <sourceSchema>.<targetTable>
            // in practice the sourceTable is usually empty
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + sourceSchema + "." + targetTable +
                         " FROM " + sourceSchema + "." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = '" + sourceSchema + "' AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + sourceSchema + "." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string copy_tafl_table2(OdbcConnection cn, string targetTable, string sourceTable)
        {
            // this function copies source table from tafl schema to user's schema
            // it is used to create a user table when the source table template in tafl has a different name from the target table
            // i.e. table tafl.<sourceTable> is copied to <user>.targetTable
            // the tafl<sourceTable> is always empty and serves as a template
            HttpContext ctx = HttpContext.Current;

            string strSql = "";

            strSql = "SELECT * INTO " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " FROM venn." + sourceTable;

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            // add primary key constraint if present in source table
            strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = 'tafl' AND TABLE_NAME = '" +
                        sourceTable + "' " +
                        "ORDER BY ORDINAL_POSITION";
            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }

            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + ctx.Session["s_schema"].ToString() + "." + targetTable +
                         " ADD CONSTRAINT PK_" + targetTable + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }

                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

        }
        public static string ISOtoSQLdate(string delimiter, string inISOdate)
        {
            char[] datedelimiter = delimiter.ToCharArray();
            string[] date_parts = inISOdate.Split(datedelimiter);
            return date_parts[1] + "/" + date_parts[2] + "/" + date_parts[0];
        }
    }
}

