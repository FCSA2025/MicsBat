//using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CheckMicsConfig
{
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;
    using SQLUINTEGER = UInt32;


    /// <summary>
    /// This class is to encapsulates a set of methods that established an ODBC/SQL connection
    /// with a prescribed data, using UtConnect() etc, that are independent of the legacy methods
    /// provided by the dll _Ssutil; the motivation for creating this class is to enable _NewLib 
    /// classes and methods to independently access the database regardless of whether the main application
    /// has openned a connection with the database, or not; _Utillib 'depends on' (uses classes from) 
    /// _NewLib and so attempting to call _Utillib methods from _NewLib would set up a circular build
    /// dependence and Visual Studio does not allow this to happen.
    /// </summary>
    public class ODBCconnect
    {
        private static bool mIsFirstConnect = true;
        private static SQLHDBC mStaticConnHandle = SQLHDBC.Zero;
        private static SQLLENPTR mNullIndPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
        private static SQLPOINTER mTargetValuePtr = Marshal.AllocHGlobal(Constant.MAX_TABLE_COLUMN_BYTES);
        private static int glbNextColumnForGet = 0;
        private static SQLHANDLE hODBCEnvironment = SQLHANDLE.Zero;
        private static SQLPOINTER ptrToglb_Schema;
        private static SQLLENPTR ptrToglb_Schema_sz;

        /// <summary>
        /// A static constructor is used to initialize any static data, or to perform a particular action 
        /// that needs to be performed once only. It is called automatically before the first instance is 
        /// created or any static members are referenced.
        /// </summary>
        static ODBCconnect()
        {
            //Allocate buffers in unmanaged memory for SQL parameter binding.
            ptrToglb_Schema = Marshal.AllocHGlobal(Constant.GLB_SCHEMA_SZ + 100);
            ptrToglb_Schema_sz = Marshal.AllocHGlobal(sizeof(SQLLEN) + 100);

            //Initialize contents of these buffers.
            //Marshal.WriteByte(ptrToglb_Schema, 0); //Effectively makes the buffer an empty string.
            Marshal.WriteInt64(ptrToglb_Schema_sz, Constant.GLB_SCHEMA_SZ);
            for (int i = 0; i < Constant.GLB_SCHEMA_SZ; i++)
            {
                Marshal.WriteByte(ptrToglb_Schema, i, 0);  //Set each byte to zero.
            }
        }

        /// <summary>
        /// This method should be invoked before calling any ODBC routines; it acquires a 
        /// handle to the ODBC environment and establishes a single 'static' ODBC connection to the database; 
        /// mMicsUserName and Info.Password <b>must be set</b> prior to calling this method.
        /// </summary>
        /// <remarks>
        /// This method acquires and maintains the ODBC environment that the single static connection will use. 
        /// It sets up ODBC connection pooling within the environment. It also retrieves the 
        /// user's schema. 
        /// </remarks>
        /// <param name="dbNameIn"> - name of the database.</param>
        /// <returns> - Constant.SUCCESS indicates successful outcome; non-zero indicates failure.</returns>
        public static bool UtConnect(string dbNameIn, ref int passCount, ref int failCount)
        {
            int rc;

            string testType = "Connect to database using ODBC?";
            string testReportLine = "";
            string reason = "";

            SQLHENV hEnv = SQLHENV.Zero;
            SQLRETURN sqlRet;
            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLHDBC hConn = SQLHDBC.Zero;

            // Validate the dbNameIn string.
            if (String.IsNullOrWhiteSpace(dbNameIn))
            {
                reason = "(Prescribed database name is NULL or whitespace)";
                testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                Console.Write("\n{0}", testReportLine);
                failCount++;
                return false;
            }

            // Check if a connection has already been established; if so, return with a fail code.
            if (mStaticConnHandle != SQLHANDLE.Zero)
            {
                reason = "(An ODBC connection to the database already exists)";
                testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                Console.Write("\n{0}", testReportLine);
                failCount++;
                return false;
            }

            // We need to get or allocate an environment.
            rc = DbGetEnv(out hEnv);
            if (rc != 0)
            {
                reason = "(Could not get handle to ODBC DB environment)";
                testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                Console.Write("\n{0}", testReportLine);
                failCount++;
                return false;
            }

            try
            {
                //	Get the user's SQL Server schema.  To do this we need to establish a connection to the
                //	designated database.  We do this without using the connection routines 
                //	because they assume the environment is already established.
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_DBC, hEnv, out hConn);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(Call to ODBC.SQLAllocHandle() failed)";
                    testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                    Console.Write("\n{0}", testReportLine);
                    failCount++;
                    return false;
                }

                //	Get the userid and password for the connection.  These are stored in 
                //	the User's Windows environment.  If they are not stored, we have an error. 
                //  We only need to do this once.
                rc = 0;
                if (mIsFirstConnect)
                {
                    if (String.IsNullOrWhiteSpace(CheckMicsConfig.mMicsUserName))
                    {
                        reason = "(Windows environment variable MICSUSER is not set)";
                        testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                        Console.Write("\n{0}", testReportLine);
                        failCount++;
                        return false;
                    }

                    if (String.IsNullOrWhiteSpace(CheckMicsConfig.mPassword))
                    {
                        reason = "(Windows environment variable PASSWORD is not set)";
                        testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                        Console.Write("\n{0}", testReportLine);
                        failCount++;
                        return false;
                    }

                    mIsFirstConnect = false;
                }

                // The ODBC connection to the database is via an existing Data Source Name (DSN)
                // as present in the 'System DSN' tab of the application 'odbcad32.exe'.
                // The legacy DSN names defined via 'odbcad32.exe' are 'fcsa', 'test' and
                // 'Regression' which are the names of actually databases on the SQL Server -
                // however, they could also have been called "tom', 'dick' and 'harry' and this
                // SQLConnect() argument would have to refer to 'tom', 'dick' or 'harry'.
                // Note: on CloudMICS the database names (and DSNs) are micsprod, micsdev and micstest.
                string dataSourceName = dbNameIn;

                sqlRet = ODBC.SQLConnect(hConn,
                                            dataSourceName,
                                            (SQLSMALLINT)dataSourceName.Length,
                                            CheckMicsConfig.mMicsUserName,
                                            (SQLSMALLINT)CheckMicsConfig.mMicsUserName.Length,
                                            CheckMicsConfig.mPassword,
                                            (SQLSMALLINT)CheckMicsConfig.mPassword.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(Call to ODBC.SQLConnect() failed)";
                    testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                    Console.Write("\n{0}", testReportLine);
                    failCount++;
                    return false;
                }

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    reason = "(Call to ODBC.SQLAllocHandle() failed)";
                    testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                    Console.Write("\n{0}", testReportLine);
                    failCount++;
                    return false;
                }

                // Determine what SQL schema is to be used.
                // Existing FCSA practice is that a user's SQL schema and their UltrixID are one and the same.
                // We will fetch the user's Ultrix ID from the DB table adm.account_details.
                // The result is also written to Info.GlobalSchema and Info.UltrixID
                if (String.IsNullOrWhiteSpace(CheckMicsConfig.mGlobalSchema))
                {
                    // The following method call returns the Ultrix ID (aka 'oper') of 'this' user (micsid);
                    // it assumes that mMicsUserName has already been set and uses it to lookup
                    // the corresponding Ultrix ID (oper) from the DB table adm.account_details.
                    //
                    // Previous instances of this code called the method GetFCSASchema_A() which in turn
                    // executed the SQL stored function dbo.user_schema2022('{0}'). This SQL function
                    // performed a lookup of micsid ('name') to oper ('default_schema_name') using the table
                    // sys.database_principals. This system table is populated automatically by the SQL Server
                    // to 'match' the Window login accounts managed by Active Directory. For FCSA staff,
                    // who have multiple SQL and Windows roles, this automatic population of their entries in
                    // sys.database_principals stopped giving the desired results in 2022. Given that the
                    // required micsid to oper lookup can be done using table adm.account_details, which is 
                    // totally under FCSA control, it was decided to stop using sys.database_principals for
                    // this lookup.

                    //string schema = GetFCSASchema_A(mMicsUserName);
                    string schema = "";
                    GetUltrixID(out schema);

                    // Check that the query to get the schema was successful.
                    if (String.IsNullOrWhiteSpace(schema))
                    {
                        reason = "(Unable to get user's SCHEMA name)";
                        testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                        Console.Write("\n{0}", testReportLine);
                        failCount++;
                        return false;
                    }

                    // Save the schema to the Info static structure member.
                    CheckMicsConfig.mGlobalSchema = schema;

                    // Establish the static (reusable) ODBC connection.
                    mStaticConnHandle = NewConn();
                }
            }
            catch (Exception e)
            {
                Console.Error.Write("\nODBCconnect.UtConnect(): Error: Exception caught: {0}\n{1}", e.Message, e.StackTrace);
                reason = "(Fatal exception thrown and caught)";
                testReportLine = String.Format(CheckMicsConfig.FORMAT_FAIL, testType, dbNameIn, "", reason);
                Console.Write("\n{0}", testReportLine);
                failCount++;
                return false;
            }
            finally
            {
                //Free handle hStmt.
                SQLRETURN sqlReturn = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                if (ODBC.IsOK(sqlReturn)) hStmt = SQLHANDLE.Zero; //Mark the handle as not in use.

                //Disconnect.
                sqlReturn = ODBC.SQLDisconnect(hConn);

                //Free handle hConn.
                sqlReturn = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_DBC, hConn);
                if (ODBC.IsOK(sqlReturn)) hConn = SQLHDBC.Zero; //Mark the handle as not in use.
            }

            reason = "";
            testReportLine = String.Format(CheckMicsConfig.FORMAT_PASS, testType, dbNameIn, "", reason);
            Console.Write("\n{0}", testReportLine);
            passCount++;

            return true;
        }

        /// <summary>
        /// Retrieves the current ODBC environment handle.  This will allocate it if it is NULL.
        /// The return value is 0 if all went well, negative on error.
        /// </summary>
        /// <remarks>
        /// An ODBC environment is a global context in which to access data; associated 
        /// with an environment is any information that is global in nature, such as:
        /// * The environment's state
        /// * The current environment-level diagnostics
        /// * The handles of connections currently allocated on the environment
        /// * The current settings of each environment attribute
        /// Within a piece of code that implements ODBC (the Driver Manager or a driver), an 
        /// environment handle identifies a structure to contain this information.
        /// Environment handles are not frequently used in ODBC applications. They are always 
        /// used in calls to SQLDataSources and SQLDrivers and sometimes used in calls to 
        /// SQLAllocHandle, SQLEndTran, SQLFreeHandle, SQLGetDiagField, and SQLGetDiagRec.
        /// </remarks>
        /// <param name="hOut"> - an ODBC handle to the ODBC environment.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - attempt to get a handle to the environment succeeded.</para>
        /// <para> - Any other value - attempt failed.</para>
        public static int DbGetEnv(out SQLHANDLE hOut)
        {
            // 'out' requirement.
            hOut = SQLHANDLE.Zero;

            // We only need to do this once.
            // Check if the static member hODBCEnvironment has already been set - if so return with that value.
            if (hODBCEnvironment != SQLHANDLE.Zero)
            {
                hOut = hODBCEnvironment;
                return Constant.SUCCESS;
            }

            SQLRETURN nRet = 0;

            /*	Before allocating the environment, turn on connection pooling. */
            nRet = ODBC.SQLSetEnvAttr(
                                        SQLHENV.Zero,
                                        ODBC.SQL_ATTR_CONNECTION_POOLING,
                                        (SQLPOINTER)ODBC.SQL_CP_ONE_PER_HENV,
                                        0);

            if (!ODBC.IsOK(nRet))
            {
                //...Console.Error.Write("\nODBCconnect.DbGetEnv(): Error: call to ODBC.SQLSetEnvAttr() failed.");
                hOut = SQLHANDLE.Zero;
                return -10;
            }

            nRet = ODBC.SQLAllocHandle(
                                        ODBC.SQL_HANDLE_ENV,
                                        (SQLHANDLE)ODBC.SQL_NULL_HANDLE,
                                        out hODBCEnvironment);

            if (!ODBC.IsOK(nRet))
            {
                //...Console.Error.Write("\nODBCconnect.DbGetEnv(): Error: call to ODBC.SQLAllocHandle() failed: unable to get environment handle.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            /*	Set the attributes of this connection.  Assume it succeeds */
            nRet = ODBC.SQLSetEnvAttr(
                                        hODBCEnvironment,
                                        ODBC.SQL_ATTR_ODBC_VERSION,
                                        (SQLPOINTER)ODBC.SQL_OV_ODBC3,
                                        0);
            if (!ODBC.IsOK(nRet))
            {
                //...Console.Error.Write("\nODBCconnect.DbGetEnv(): Error: call to ODBC.SQLSetEnvAttr() failed.");
                return Error.CANNOTSETODBCENVIRONMENTATTRIBUTES;
            }

            hOut = hODBCEnvironment;

            return Constant.SUCCESS;
        }

#if true
        /// <summary>
        /// This method returns the Ultrix ID of 'this' user; it assumes that 
        /// mMicsUserName has already been set and uses it to lookup the corresponding 
        /// Ultrix ID from the DB table adm.account_details.
        /// The result is also written to Info.UltrixID
        /// </summary>
        /// <param name="ultrixID"></param>
        /// <returns></returns>
        public static int GetUltrixID(out string ultrixID)
        {
            // 'out' requirement.
            ultrixID = "";

            // First check whether we already have 'this' user's Ultrix ID in Info.
            // If so, return it.
            if (!String.IsNullOrWhiteSpace(CheckMicsConfig.mUltrixID))
            {
                ultrixID = CheckMicsConfig.mUltrixID;
                return Constant.SUCCESS;
            }

            // The following method call assumes that mMicsUserName
            // has already been set.
            int rc = GetSystemId(out ultrixID, Constant.ULTRIXID_SZ);

            if (rc != Constant.SUCCESS)
            {
                // Something went seriously wrong with the DB retrieval.
                //...Console.Error.Write("\nODBCconnect.GetUltrixID(): ERROR: call to GetSystemId() failed.");
                return Error.BADULTRIXID;
            }

            if (String.IsNullOrWhiteSpace(ultrixID))
            {
                // Something strange just happened.
                //...Console.Error.Write("\nODBCconnect.GetUltrixID(): ERROR: call to GetSystemId() returned null, empty or whitespace string.");
                return Error.BADULTRIXID;
            }

            // If we get here the call has succeeded: 'this' user's MICS username is returned as ultrixID.
            // Set the field in the static object Info that carries the ultrixID.
            CheckMicsConfig.mUltrixID = ultrixID;

            return Constant.SUCCESS;
        }

#endif
        /// <summary>
        /// This method returns the Ultrix ID of 'this' user; it assumes that 
        /// mMicsUserName has already been set and uses it to lookup the corresponding 
        /// Ultrix ID from the DB table adm.account_details.
        /// </summary>
        /// <param name="cSystemId"> - the FCSA System ID</param>
        /// <param name="nLen"></param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the retrieval attempt succeeded.</para>
        /// <para> - Anything else - attempt failed.</para>
        public static int GetSystemId(out string cSystemId, SQLULEN nLen)
        {
            // Satisfy 'out' requirement.
            cSystemId = null;

            string cSQL = "Not Set.";
            SQLRETURN sqlRet = -666;

            if (String.IsNullOrWhiteSpace(CheckMicsConfig.mMicsUserName))
            {
                //...Console.Error.Write("\nODBCconnect.GetSystemId(): Error: mMicsUserName not set.");
                return Constant.FAILURE;
            }

            //	Get the system Id for this mics user
            SQLHANDLE hStmt;
            SQLHDBC hConn = NewConn();
            SQLLEN nNull;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                //...Console.Error.Write("\nODBCconnect.GetSystemId(): call to ODBC.SQLAllocHandle() failed.");
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            cSQL = String.Format("select ultrixid from adm.account_details where micsid='{0}'", CheckMicsConfig.mMicsUserName);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                //...Console.Error.Write("\nODBCconnect.GetSystemId(): call to ODBC.SQLExecDirect() failed.");
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                //...Console.Error.Write("\nODBCconnect.GetSystemId(): call to ODBC.SQLFetch() failed.");
                return Error.ODBC_FETCH_FAILED;
            }

            DbGetString(hStmt, 1, "ultrixid", out cSystemId, (int)nLen, out nNull);
            if (String.IsNullOrWhiteSpace(cSystemId))
            {
                //	Somehow we have a null field
                //...Console.Error.Write("\nODBCconnect.GetSystemId(): call to DbGetString() failed, returned NULL, empty or whitspace string value for cSystemId.");
                return -666;
            }

            //If we get here we have a valid value for the returned value cSystemId.
            // Clean up.
            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            DisConn(hConn);

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Returns an ODBC handle to a new connection in the current environment; the
        /// following fields MUST already be set: Info.DbName, mMicsUserName, Info.Password.
        /// </summary>
        /// <returns>ODBC handle to a new connection</returns>
        public static SQLHDBC NewConn()
        {

            // If we already have an open ODBC connection then use it.
            if (mStaticConnHandle != SQLHDBC.Zero)
            {
                return mStaticConnHandle;
            }

            // If we reach here there isn't an existing active reusable 
            // ODBC connection so we need to create one.

            SQLRETURN rc;
            SQLRETURN retCode;
            SQLHDBC hConn = SQLHDBC.Zero;
            SQLHANDLE hUse;

            // Retrieve the handle to the ODBC environment that should have
            // already been instantiated by a previous call to UtConnect().
            DbGetEnv(out hODBCEnvironment);

            // Request an ODBC connection handle for the environment.
            rc = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_DBC, hODBCEnvironment, out hConn);

            if (!ODBC.IsOK(rc))
            {
                //...Console.Error.Write("\nODBCConnect.NewConn(): ERROR A: call to SQLAllocHandle() failed.");
                return SQLHDBC.Zero;
            }

            // Enabe MARS capability.
            // The default ODBC behaviour is that a connection can have only one
            // active statement. To allow multiple active statements on a connection
            // we have to enable Multiple Active Result Sets (MARS) capability.
            ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_COPT_SS_MARS_ENABLED, (SQLPOINTER)ODBC.SQL_MARS_ENABLED_YES, ODBC.SQL_IS_UINTEGER);

            // Now attempt to establish an ODBC/SQL connection to the database.
            retCode = ODBC.SQLConnect(hConn,
                                        (SQLCHARPTR)CheckMicsConfig.mDbName,
                                        (SQLSMALLINT)CheckMicsConfig.mDbName.Length,
                                        (SQLCHARPTR)CheckMicsConfig.mMicsUserName,
                                        (SQLSMALLINT)CheckMicsConfig.mMicsUserName.Length,
                                        (SQLCHARPTR)CheckMicsConfig.mPassword,
                                        (SQLSMALLINT)CheckMicsConfig.mPassword.Length);

            if (!ODBC.IsOK(rc))
            {
                //...Console.Error.Write("\nODBCConnect.NewConn(): ERROR: call to SQLConnect() failed.");
                return SQLHDBC.Zero;
            }

            //	Now we are connected to the SQL Server Engine.
            //	We need to set the context to the actual database the user has requested. 
            retCode = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUse);

            if (!ODBC.IsOK(retCode))
            {
                //...Console.Error.Write("\nODBCConnect.NewConn(): ERROR B: call to SQLAllocHandle() failed.");
                return SQLHDBC.Zero;
            }

            string cUseStr = "USE " + CheckMicsConfig.mDbName;

            retCode = ODBC.SQLExecDirect(hUse, cUseStr, (SQLINTEGER)cUseStr.Length);
            if (!ODBC.IsOK((SQLRETURN)retCode))
            {
                //...Console.Error.Write("\nODBCConnect.NewConn(): ERROR: call to ODBC.SQLExecDirect() failed.");
                return SQLHDBC.Zero;
            }

            // If we get here then we are connected.

            // Set the transaction isolation level to default (read_uncommitted)
            DbSetIsoLevel(hConn, ODBC.SQL_TXN_READ_UNCOMMITTED);

            // Save the connection handle as a global static so that it can be reused anywhere
            // in the application code.
            mStaticConnHandle = hConn;

            return hConn;
        }

        /// <summary>
        /// This is a legacy method that no longer does anything because we establish
        /// a single ODBC connection and maintain it throughout the UtConnect() session.
        /// </summary>
        /// <param name="hConn"> - ODBC connection handle.</param>
        /// <returns> - SQL_SUCCESS, SQL_ERROR, or SQL_INVALID_HANDLE. </returns>
        public static int DisConn(SQLHDBC hConn)
        {
            return ODBC.SQL_SUCCESS;
        }

        /// <summary>
        /// Disconnects from an ODBC database session and releases the ODBC resources.
        /// </summary>
        /// <returns>- ODBC result code for SQLFreeHandle(SQL_HANDLE_ENV, hODBCEnvironment).</returns>
        public static int UtDisconnect()
        {
            /* Local variables */
            SQLRETURN sqlRet;
            int retVal = Constant.SUCCESS;

            //	The sessionId is no longer used.  Instead we release the ODBC environment.
            try
            {
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);

                if (sqlRet != ODBC.SQL_SUCCESS)
                {
                    // At this point the calling program TpRunTsip.Main() has almost
                    // completed so just carry on even though this error should not be 
                    // happening.
                    retVal = Constant.FAILURE;
                    //...Console.Error.Write("\nODBCconnect.UtDisconnect(): ERROR A: call to ODBC.SQLFreeHandle() failed.");
                }

                hODBCEnvironment = SQLHANDLE.Zero;

                // Disconnect from ODBC.
                sqlRet = ODBC.SQLDisconnect(mStaticConnHandle);
                if (!ODBC.IsOK(sqlRet))
                {
                    retVal = Constant.FAILURE;
                    //...Console.Error.Write("\nODBCconnect.UtDisconnect(): ERROR: call to ODBC.SQLDisconnect() failed.");
                }

                // Free the connection handle.
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_DBC, mStaticConnHandle);
                if (!ODBC.IsOK(sqlRet))
                {
                    retVal = Constant.FAILURE;
                    //...Console.Error.Write("\nODBCconnect.UtDisconnect(): ERROR B: call to ODBC.SQLFreeHandle() failed.");
                }

            }
            catch (Exception e)
            {
                Console.Error.Write("\r\nODBCconnect.UtDisconnect(): ERROR: exception caught:\n{0}\n{1}", e.Message, e.StackTrace);
                retVal = Constant.FAILURE;
            }

            return (retVal);
        }

        /// <summary>
        /// Get a string value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch(); SQL column numbers start at 1 and can be prescribed by the user; 
        /// alternatively, the calling program can set the call argument 'nColNum' to zero and successive calls to this method 
        /// will auto-increment the column number, again starting at 1.
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="cString"> - the retrieved string value.</param>
        /// <param name="nLen"> - the maximum length of string to retrieve.</param>
        /// <param name="nullInd"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetString(SQLHANDLE hStmt, int nColNum, string cName, out string cString, int nLen, out SQLLEN nullInd)
        {
            //This call could throw an exception; we should catch it at the next level up.
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_CHAR, mTargetValuePtr, nLen + 2, out nullInd);

            if (nullInd != Constant.DB_NULL)
            {
                cString = Marshal.PtrToStringAnsi(mTargetValuePtr);

                // All strings gotten from the DB are trimmed 'on arrival'.
                cString = cString.Trim();
            }
            else
            {
                cString = "";  // Note: this prevents issues with ODBC parameter binding; using null causes problems.
            }

            return nRet;
        }

        /// <summary>
        /// Get an int (int32) value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetValue"> - the retrieved int value.</param>
        /// <param name="nNull"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetInt(SQLHANDLE hStmt, int nColNum, string cName, out int targetValue, out SQLLEN nNull)
        {
            targetValue = 0;

            //This call could throw an exception; we should catch it at the next level up.
            //DbGetInt() returns a C# 'int'. The corresponding 'C' types are 'int' and 'long'. 
            //The only SQL / 'C' Data Type that corresponds to C# 'int' is SQL_C_SLONG.
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_SLONG, mTargetValuePtr, Constant.SIZEOF_INT, out nNull);

            if (nNull != Constant.DB_NULL)
            {
                targetValue = Marshal.ReadInt32(mTargetValuePtr);
            }

            //                                               $
            //...Log2.v("\n\nSsutil.DbGetInt():    Returned: " + targetValue);
            return nRet;
        }

        /// <summary>
        /// Sets the transaction Isolation level for an ODBC Connection.
        /// </summary>
        /// <param name="hConn"></param>
        /// <param name="nLevel"></param>
        /// <returns> - SQL_SUCCESS, SQL_SUCCESS_WITH_INFO, SQL_ERROR, SQL_INVALID_HANDLE, or SQL_STILL_EXECUTING. </returns>
        public static SQLRETURN DbSetIsoLevel(SQLHDBC hConn, SQLLEN nLevel)
        {
            SQLPOINTER nLevelPtr = Marshal.AllocHGlobal(sizeof(SQLULEN));
            Marshal.WriteInt64(nLevelPtr, nLevel);

            // The nLevel value is passed via the 3rd call argument 'SQLPOINTER ValuePtr'
            // If the value being passed is a string then ValuePtr would be its start address.
            // In our case, the value being passed is an just an integer (nLevel).
            // Reading the ODBC documention very careful reveals that nLevel can be passed
            // 'directly' rather than 'indirectly' (via a pointer). This seems to be inconsistent
            // with the usual ODBC parameter-passing regime.

            SQLRETURN sqlRet = ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_TXN_ISOLATION, (IntPtr)nLevel, 0);

            if (!ODBC.IsOK(sqlRet))
            {
                //...Console.Error.Write("\nODBCconnect.DbSetIsoLevel(): ERROR: call to ODBC.SQLSetConnectAttr() failed.");
            }

            return sqlRet;
        }

        /// <summary>
        /// This method provides a generic 'wrapper' around an ODBC.SQLGetData() operation for a variety of 'target' value types (e.g. string, short, int etc);
        /// it is called by the type-specific get methods such as DbGetString() etc; SQL column numbers start at 1 and can be prescribed by the user; 
        /// alternatively, the calling program can set the call argument 'nColNum' to zero and successive calls to this method will auto-increment
        /// the column number again starting at 1.
        /// </summary>
        /// <remarks>
        /// <b>targetType</b>:
        /// 
        /// <para>string:    ODBC.SQL_C_CHAR</para>
        /// <para>byte:      ODBC.SQL_C_UTINYINT</para>
        /// <para>short:     ODBC.SQL_C_SSHORT</para>
        /// <para>int:       ODBC.SQL_C_SLONG</para>
        /// <para>float:     ODBC.SQL_C_FLOAT</para>
        /// <para>double:    ODBC.SQL_C_DOUBLE</para>
        /// </remarks>
        /// <param name="hStmt"> - current ODBC statement handle.</param>
        /// <param name="nColNum"> - prescribed column number of DB table to be read (starts at column 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetType"> - see remarks, above.</param>
        /// <param name="targetValuePtr"> - IntPtr to a buffer where the 'get' result will be written.</param>
        /// <param name="nLen"> - length of the allocated buffer.</param>
        /// <param name="nullInd"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbUniversalGet(SQLHANDLE hStmt, int nColNum, string cName, SQLSMALLINT targetType, SQLPOINTER targetValuePtr, SQLLEN nLen, out SQLLEN nullInd)
        {
            //IMPORTANT: it is assumed that targetValuePtr points to an already-allocated block of nLen bytes in global memory.

            if (nColNum == 0)
            {
                nColNum = ++glbNextColumnForGet;
            }
            else
            {
                glbNextColumnForGet = (short)nColNum;
            }

            SQLRETURN sqlRet = ODBC.SQLGetData(hStmt, (SQLUSMALLINT)nColNum, targetType, targetValuePtr, nLen, mNullIndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                // This is a serious error that should cause the application to terminate.
                // We will raise an exception so that the 'what next?' can be decided by the
                // calling program.
                //...Console.Error.Write("\nODBCconnection.DbUniversalGet(): ERROR: call to ODBC.SQLGetData() failed.");
                throw new Exception(cName);
            }

            nullInd = Marshal.ReadInt64(mNullIndPtr);

            if (nullInd == ODBC.SQL_NULL_DATA)
            {
                nullInd = Constant.DB_NULL;
            }
            else
            {
                nullInd = Constant.DB_NOT_NULL;
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method resets the static counter used for auto-column-indexing during DB 'get' operations;
        /// If the application uses auto-column-indexing this reset method MUST be called prior to 'getting' any
        /// records from a database table.
        /// </summary>
        /// <param name=""></param>
        public static void DbStartGets()
        {
            glbNextColumnForGet = 0;
        }

    }
}