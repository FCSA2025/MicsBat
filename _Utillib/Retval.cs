using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using System.Text.RegularExpressions;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides a method to write records to the user's <b>returnvalues</b> table.
    /// This table is used to store user data that can be retreived and used by other MICS
    /// programs e.g. CTX data can be stored in the returnvalues table.
    /// </summary>
    public class Retval
    {
        /// <summary>
        /// This method writes records to the user's <b>returnvalues</b> Table.
        /// </summary>
        /// <remarks>
        ///  This method inputs a key of up to 20 characters, and an array of strings
        ///  whose elements can be up to 20 characters long. It writes each string 
        ///  to a new record in the database table with the given key and a sequential 
        ///  index as keys and the value and date as the rest of the record. The table 
        ///  is created automatically if it doesn't exist. If it does already exist 
        ///  then any records over two days old are automatically deleted. 
        /// </remarks>
        /// <param name="cKey"></param>
        /// <param name="cVals"></param>
        /// <returns></returns>
        public static int RetVal(string cKey, ref string[] cVals)
        {
            int nInd;
            string cCreateStmt;
            string cSchema;

            bool IsTable;

            // Validate the length of the key string.
            if (String.IsNullOrWhiteSpace(cKey))
            {
                Log2.e("\nRetval.RetVal(): ERROR: cKey is null, empty or white space.");
                return -666;
            }
            else if (cKey.Length > 20)
            {
                Log2.e("\nRetval.RetVal(): ERROR: cKey exceeds 20 characters.");
                return -667;
            }

            IsTable = Ssutil.DbTableExists("returnvalues", out cSchema);

            if (!IsTable)
            {
                //...Log2.v("\nRetval.RetVal(): DB table 'returnvalues' does not already exist. Attempting to create it...");
                // Table does not exist. We create it.  
                cCreateStmt = String.Format("CREATE TABLE {0}.returnvalues (retkey char({1}), retind smallint, retval char({2}), retdate date)",
                                                                        Info.GlobalSchema, Constant.RETVALKEY_SZ, Constant.RETVAL_SZ);

                if (!Ssutil.DbExecute(cCreateStmt))
                {
                    // Could not create table.
                    Log2.e("\nRetval.RetVal(): ERROR: A: call to Ssutil.DbExecute() failed for SQL query: " + cCreateStmt);
                    return (-100);
                }

                //...Log2.v("\nRetval.RetVal(): DB table 'returnvalues' successfully created.");
            }
            else
            {
                // The table exists, delete items more than 2 days old */
                //...Log2.v("\nRetval.RetVal(): DB table 'returnvalues' already exists.");

                string cSQL = String.Format("delete from {0}.returnvalues where datediff(day, retdate, CURRENT_TIMESTAMP) > 2", Info.GlobalSchema);
                if (!Ssutil.DbExecute(cSQL))
                {
                    Log2.e("\nRetval.RetVal(): ERROR: B: call to Ssutil.DbExecute() failed for SQL query: " + cSQL);
                }
                else
                {
                    //...Log2.v("\nRetval.RetVal(): DB table 'returnvalues' : deletion of records more than 2 days old succeeded.");
                }
            }

            // Insert the values in key order.
            //...Log2.v("\nRetval.RetVal(): DB table 'returnvalues' : inserting values in key order.");

            for (nInd = 0; nInd < cVals.Length; nInd++)
            {
                string value;

                // Validate the length of the value string.
                if (String.IsNullOrWhiteSpace(cVals[nInd]))
                {
                    value = "";
                    //Log2.e("\nRetval.RetVal(): ERROR: cVals[nInd] is null, empty or white space.");
                    //return -668;
                }
                else
                {
                    value = cVals[nInd];
                }

                // The value string could contain one or more apostrophes that will wreak havoc
                // when SQL attempts to parse and execute the insert query.
                // The fix is to 'double up' each occurance of an apostrophe to 'escape' it.
                value = Regex.Replace(value, "'", "''");

                string cInsertStmt = String.Format("insert into {0}.returnvalues (retkey, retind, retval, retdate) values ('{1}', {2}, '{3}', CURRENT_TIMESTAMP)",
                                    Info.GlobalSchema, cKey, nInd, value);

                if (!Ssutil.DbExecute(cInsertStmt))
                {
                    Log2.e("\nRetval.RetVal(): ERROR: C: call to Ssutil.DbExecute() failed for SQL query: " + cInsertStmt);
                    return (nInd + 1);
                }
            }

            //...Log2.v("\nRetval.RetVal(): DB table 'returnvalues' : successfully inserted " + cVals.Length + " values.");

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method deletes all rows in a user's returnvalues table that have their
        /// 'retkey' column equal to a prescribed string.
        /// </summary>
        /// <param name="retkey"></param>
        /// <returns></returns>
        public static int DeleteRowsWithKey(string retkey)
        {
            int result = Constant.FAILURE;
            string schema;

            // Check if the user's returnvalues table actually exists.
            // If it doesn't exist we can't delete rows from it.
            if (!Ssutil.DbTableExists("returnvalues", out schema))
            {
                return Constant.FAILURE;
            }

            // The return values table exists so construct an SQL query to delete rows
            // with the prescribed value of the column 'retkey'.
            string cSQL = String.Format("DELETE FROM {0}.returnvalues WHERE retkey='{1}'", Info.GlobalSchema, retkey);

            // Execute the SQL query.
            if (!Ssutil.DbExecute(cSQL))
            {
                Log2.e("\nRetval.DeleteRowsWithKey(): ERROR: call to Ssutil.DbExecute() failed for SQL query: " + cSQL);
            }
            else
            {
                // Deletion of rows succeeded.
                result = Constant.SUCCESS;
            }

            return result;
        }

        /// <summary>
        /// This method deletes all rows in a user's returnvalues table.
        /// </summary>
        /// <param name="retkey"></param>
        /// <returns></returns>
        public static int DeleteAllRows(string retkey)
        {
            int result = Constant.FAILURE;
            string schema;

            // Check if the user's returnvalues table actually exists.
            // If it doesn't exist we can't delete rows from it.
            if (!Ssutil.DbTableExists("returnvalues", out schema))
            {
                return Constant.FAILURE;
            }

            // The return values table exists so construct an SQL query to delete rows
            // with the prescribed value of the column 'retkey'.
            string cSQL = String.Format("DELETE FROM {0}.returnvalues", Info.GlobalSchema);

            // Execute the SQL query.
            if (!Ssutil.DbExecute(cSQL))
            {
                Log2.e("\nRetval.DeleteAllRows(): ERROR: call to Ssutil.DbExecute() failed for SQL query: " + cSQL);
            }
            else
            {
                // Deletion of rows succeeded.
                result = Constant.SUCCESS;
            }

            return result;
        }






    }
}
