using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _NewLib;

namespace _Utillib
{
    using _DataStructures;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
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

    /// <summary>
    /// Provides a large number of utility methods to interface with the MS SQL Server
    /// via ODBC.
    /// </summary>
    public class Ssutil
    {
        //AH: 
        //We need to have an SQLLENPTR available to DbBindStringInput() so that
        //it can pass the string length into SQLBindParameter() via strLen_or_IndPtr.
        //This pointer needs to have persistence hence it is defined here as a static member.
        private static SQLLENPTR staticStringLengthPtr = SQLLENPTR.Zero;

        // Statics used for performance optimization.
        private static SQLLENPTR mNullIndPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
        private static SQLPOINTER mTargetValuePtr = Marshal.AllocHGlobal(Constant.MAX_TABLE_COLUMN_BYTES);
        private static float[] mFloatArray = new float[1];
        private static double[] mDoubleArray = new double[1];

        private static int glbTotalConn = 0;		//	Keeps count of the number of currently open connections.
        private static SQLHANDLE hODBCEnvironment = SQLHANDLE.Zero;
        private static bool IsFirstConnect = true;
        private static int glbNextColumnForBind = 0;
        private static int glbNextColumnForGet = 0;
        private static SQLPOINTER ptrToglb_Schema;
        private static SQLLENPTR ptrToglb_Schema_sz;
        private static string glb_Schema = null;
        private static SQLLEN glb_sqlRowCount = -2;		//	Row count from the execute

        private static Enums.DbGetForNullMode dbGetMode = Enums.DbGetForNullMode.SET_NULLS_AS_ZERO;

        private static string mGlbRootBuff;

        private static string mMicsBinDirPath = null;

        private static string mIntName = null;
        private static string mIntName1 = null;
        private static string mIntName2 = null;
        private static string mIntName3 = null;
        private static string mIntName4 = null;
        private static string mIntName5 = null;
        private static string mIntName6 = null;
        private static string mIntName7 = null;

        // Reuse a static ODBC connection handle.
        private static SQLHDBC mStaticConnHandle = SQLHDBC.Zero;
        //----------------------------------------------------------------
        public static string Glb_Schema
        {
            get { return glb_Schema; }
            set { glb_Schema = value; }
        }
        //----------------------------------------------------------------

        /// <summary>
        /// A static constructor is used to initialize any static data, or to perform a particular action 
        /// that needs to be performed once only. It is called automatically before the first instance is 
        /// created or any static members are referenced.
        /// </summary>
        static Ssutil()
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
        /// <b>Important!</b> - sets the dbGetMode to one of SET_NULLS_AS_MAX_FOR_TYPE or SET_NULLS_AS_ZERO.
        /// See the detailed remarks below.
        /// </summary>
        /// <remarks>
        /// In the C/C++ code, numeric 'gets' from the DB are set to zero if ODBC 
        /// returns a Null for that column - this is not good practice because
        /// it allows 'if (field > 0)' to be used instead of actually carrying around
        /// and testing the associated nullInd value.
        /// <para>
        /// In the C# code, for Suutil.DbGet<type>() calls, NullInd for numeric types
        /// are also defaulted to zero, to maintain algorithmic compatability with the
        /// original C/C++ code.</para>
        /// <para>
        /// The use of MaxValue 'preserves' the NullInd information even though the 
        /// actual SQLLEN nullInd value may have been discard higher-up the
        /// call-tree.</para>
        /// </remarks>
        /// <param name="mode"> - SET_NULLS_AS_MAX_FOR_TYPE or SET_NULLS_AS_ZERO.</param>
        public static void SetDbGetMode(Enums.DbGetForNullMode mode)
        {
            dbGetMode = mode;
        }

        /// <summary>
        /// Returns the number of rows found in a prescribed database table using
        /// a prescribed SQL 'WHERE' condition.
        /// </summary>
        /// <param name="cTable"> - table to be searched.</param>
        /// <param name="cCondition"> - SQL 'WHERE' clause.</param>
        /// <returns> - number of rows found.</returns>
        public static int DbCountRows(string cTable, string cCondition)
        {
            Console.Write("\n\nSsutil.DbCountRows(): Entry ctable:" + cTable + " cCondition:" + cCondition);

            //char cBuf[1000];
            int nCount = 0;

            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLRETURN sqlRet = 0;
            SQLHDBC hConn = SQLHDBC.Zero;

            //if (cTable == NULL || strlen(cTable) == 0)
            if (String.IsNullOrWhiteSpace(cTable))
            {
                GenUtil.SetError(9920, "DbCountRows - Null table entered.");
                return -1;
            }

            //sprintf_s(cBuf, sizeof(cBuf), "select count(*) from %s ", cTable);
            StringBuilder sb = new StringBuilder();
            sb.Append("select count(*) from ");
            sb.Append(cTable);

            if (!String.IsNullOrWhiteSpace(cCondition))
            {
                sb.Append(" where ");
                sb.Append(cCondition);
            }

            SQLCHARPTR cBuf = sb.ToString();

            Console.WriteLine("\r\nSsutil.DbCountRows(): sb = " + sb);

            try
            {
                hConn = NewConn();

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLAllocHandle() failed, sqlRet =  " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                sqlRet = ODBC.SQLExecDirect(hStmt, cBuf, cBuf.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLExecDirect() failed, sqlRet =  " + sqlRet);
                    Log2.e("\r\nSsutil.DbCountRows(): query =  " + cBuf);
                    Log2.e("\n" + ODBC.GetDiagnostics(hStmt, cBuf));
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLFetch() failed, sqlRet =  " + sqlRet);
                    return Error.ODBC_FETCH_FAILED;
                }

                SQLLEN nullInd;
                DbGetInt(hStmt, 1, "Count", out nCount, out nullInd);
            }
            catch (Exception e)
            {
                Log2.e("\r\nSsutil.DbCountRows(): try-catch: ERROR: exception: " + e.Message);
                DbGetDiagStmt(hStmt, "DbCountRows: Error.");
                nCount = -3;
            }
            finally
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                DisConn(hConn);
            }

            //...Log2.v("\nSsutil.DbCountRows(): Exit: nCount = " + nCount);
            return nCount;
        }

        /// <summary>
        /// Returns the number of rows found in a prescribed database table using
        /// a prescribed SQL 'WHERE' condition and a prescribed ODBC connection
        /// handle; this is useful when the ODBC commit mode is MANUAL.
        /// </summary>
        /// /// <param name="hConn"> - a prescribed open ODBC connection with the SQL Server.</param>
        /// <param name="cTable"> - table to be searched.</param>
        /// <param name="cCondition"> - SQL 'WHERE' clause.</param>
        /// <returns> - number of rows found.</returns>
        public static int DbCountRowsConn(SQLHDBC hConn, string cTable, string cCondition)
        {
            //...Log2.v("\n\nSsutil.DbCountRows(): Entry");

            //char cBuf[1000];
            int nCount = 0;

            SQLHANDLE hStmt = SQLHANDLE.Zero;
            SQLRETURN sqlRet = 0;

            //if (cTable == NULL || strlen(cTable) == 0)
            if (String.IsNullOrWhiteSpace(cTable))
            {
                GenUtil.SetError(9920, "DbCountRows - Null table entered.");
                return -1;
            }

            //sprintf_s(cBuf, sizeof(cBuf), "select count(*) from %s ", cTable);
            StringBuilder sb = new StringBuilder();
            sb.Append("select count(*) from ");
            sb.Append(cTable);

            if (!String.IsNullOrWhiteSpace(cCondition))
            {
                sb.Append(" where ");
                sb.Append(cCondition);
            }

            SQLCHARPTR cBuf = sb.ToString();

            //...Log2.v("\r\nSsutil.DbCountRows(): cBuf = " + cBuf);

            try
            {
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLAllocHandle() failed, sqlRet =  " + sqlRet);
                    return Error.ODBC_SQLALLOCHANDLE_FAILED;
                }

                sqlRet = ODBC.SQLExecDirect(hStmt, cBuf, cBuf.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLExecDirect() failed, sqlRet =  " + sqlRet);
                    Log2.e("\r\nSsutil.DbCountRows(): query =  " + cBuf);
                    Log2.e("\n" + ODBC.GetDiagnostics(hStmt, cBuf));
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.DbCountRows(): ERROR: call to SQLFetch() failed, sqlRet =  " + sqlRet);
                    return Error.ODBC_FETCH_FAILED;
                }

                SQLLEN nullInd;
                DbGetInt(hStmt, 1, "Count", out nCount, out nullInd);
            }
            catch (Exception e)
            {
                Log2.e("\r\nSsutil.DbCountRows(): try-catch: ERROR: exception: " + e.Message);
                DbGetDiagStmt(hStmt, "DbCountRows: Error.");
                nCount = -3;
            }
            finally
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            }

            //...Log2.v("\nSsutil.DbCountRows(): Exit: nCount = " + nCount);
            return nCount;
        }

        /// <summary>
        /// Disconnects from an ODBC database session and releases the ODBC environment handle.
        /// </summary>
        /// <param name="sessionIdIn"> - no longer used for anything.</param>
        /// <returns>- ODBC result code for SQLFreeHandle(SQL_HANDLE_ENV, hODBCEnvironment).</returns>
        public static int UtDisconnect(int sessionIdIn)
        {
            /* Local variables */
            SQLRETURN sqlRet = Constant.FAILURE;

            //	The sessionId is no longer used.  Instead we release the ODBC environment.
            try
            {
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);

                if (sqlRet != ODBC.SQL_SUCCESS)
                {
                    // At this point the calling program TpRunTsip.Main() has almost
                    // completed so just carry on even though this error should not be 
                    // happening.
                    //...Log2.v("\r\nSsutil.UtDisconnect(): WARNING: ODBC.SQLFreeHandle returned " + sqlRet);
                }

                hODBCEnvironment = SQLHANDLE.Zero;

                // Disconnect from ODBC.
                sqlRet = ODBC.SQLDisconnect(mStaticConnHandle);

                // Free the connection handle.
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_DBC, mStaticConnHandle);

                // Housekeeping.
                Info.StaticConnHandle = SQLHDBC.Zero;

            }
            catch (Exception e)
            {
                //Just carry on...
                Log2.e("\r\nSsutil.UtDisconnect(): ERROR: ODBC.SQLFreeHandle() raised an exception: " + e.Message);
            }

            //...Log2.v("\n\nSsutil.UtDisconnect(): returned " + sqlRet);
            return (sqlRet);
        }

        /// <summary>
        /// Retrieves diagnostic information for the most recent ODBC call. The diagnostic information
        /// is then accumulated in a list managed by GenUtil.
        /// </summary>
        /// <param name="nHandleType"> - ODBC handle type.</param>
        /// <param name="hHnd"> - ODBC handle used for the most recent ODBC call.</param>
        /// <returns> - always return Constant.SUCCESS.</returns>
        public static int DbGetDiag(SQLSMALLINT nHandleType, SQLHANDLE hHnd)
        {
            //...Log2.v("\n\nSsutil.DbGetDiag(): Entry");

            SQLRETURN nRet;
            SQLSMALLINT nRec = 1;
            SQLCHARPTRINOUT SQLStatePtr = Marshal.AllocHGlobal(5 + 1);
            SQLINTEGERPTR NativeErrorPtr = Marshal.AllocHGlobal(sizeof(SQLINTEGER));
            SQLCHARPTRINOUT MessageTextPtr = Marshal.AllocHGlobal(Constant.GETDIAGS_BUFFER_SZ);
            const SQLSMALLINT BufferLength = (SQLSMALLINT)Constant.GETDIAGS_BUFFER_SZ;
            SQLSMALLINTPTR TextLengthPtr = Marshal.AllocHGlobal(sizeof(SQLSMALLINT)); ;
            bool IsStillMore = true;
            //SQLINTEGER nColNo = SQLINTEGER.MinValue;
            string txt;

            //SQLCHAR SQLState[8] = "<?>";
            txt = "<?>\0";
            char[] cArray = txt.ToArray();
            Marshal.Copy(cArray, 0, SQLStatePtr, cArray.Length);

            while (IsStillMore)
            {
                nRet = ODBC.SQLGetDiagRec(nHandleType, hHnd, nRec, SQLStatePtr, NativeErrorPtr, MessageTextPtr, BufferLength, TextLengthPtr);
                string SQLState = Marshal.PtrToStringAnsi(SQLStatePtr);
                switch (nRet)
                {
                    case ODBC.SQL_NO_DATA:
                        //...Log2.v("\r\nSsutil.DbGetDiag(): SQLGetDiagRec(): case SQL_NO_DATA:");
                        IsStillMore = false;
                        break;

                    case ODBC.SQL_SUCCESS:
                    case ODBC.SQL_SUCCESS_WITH_INFO:
                        //...Log2.v("\r\nSsutil.DbGetDiag(): SQLGetDiagRec(): case SQL_SUCCESS or SQL_SUCCESS_WITH_INFO:");
                        string MessageText = Marshal.PtrToStringAnsi(MessageTextPtr);
                        if (nRec == 1)
                        {
                            txt = String.Format("ODBC Diagnostic:\r\n[{0}] {1}", SQLState, MessageText);
                            GenUtil.SetError(9902, txt);
                        }
                        else
                        {
                            txt = String.Format("{0}\r\n{1}", GenUtil.GetUserMess(), MessageText);
                            GenUtil.SetError(9902, txt);
                        }
                        break;

                    case ODBC.SQL_ERROR:
                        Log2.e("\r\nSsutil.DbGetDiag(): SQLGetDiagRec(): case SQL_ERROR:");
                        txt = String.Format("ODBC Diagnostic ERROR: [{0}] RecNumber: {1}, BuffLen: {2}", SQLState, nRec, BufferLength);
                        GenUtil.SetError(9903, txt);
                        IsStillMore = false;
                        break;

                    case ODBC.SQL_INVALID_HANDLE:
                        Log2.e("\r\nSsutil.DbGetDiag(): SQLGetDiagRec(): case SQL_INVALID_HANDLE:");
                        txt = String.Format("ODBC Diagnostic ERROR: [{0}] Invalid Handle.", SQLState);
                        GenUtil.SetError(9904, txt);
                        IsStillMore = false;
                        break;

                    default:
                        //...Log2.v("\r\nSsutil.DbGetDiag(): SQLGetDiagRec(): case default:");
                        txt = String.Format("ODBC Diagnostic unrecognized return: [{0}] {1}", SQLState, nRet);
                        GenUtil.SetError(9905, txt);
                        IsStillMore = false;
                        break;
                }

                nRec++;
            }

            //freemalloccopyptr();    // In case it was allocated.
            //C# takes care of garbage collection.

            //...Log2.v("\n\nSsutil.DbGetDiag(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Returns an ODBC handle to a new connection in the current environment; the
        /// following fields MUST already be set: Info.DbName, Info.MicsUserName, Info.Password.
        /// </summary>
        /// <returns>ODBC handle to a new connection</returns>
        public static SQLHDBC NewConn()
        {
            //...Log2.v("\n\nSsUtil.NewConn(): Entry");

            // If we already have an open ODBC connection then use it.
            if (mStaticConnHandle != SQLHDBC.Zero)
            {
                //...Log2.v("\nSsutil.NewConn(): StaticConnHandle != SQLHDBC.Zero");
                return mStaticConnHandle;
            }
            else
            {
                //...Log2.v("\nSsutil.NewConn(): StaticConnHandle == SQLHDBC.Zero");
            }

            // If we reach here there isn't an existing active reusable 
            // ODBC connection so we need to make one.

            SQLRETURN rc;
            int retCode;
            SQLHDBC hConn = SQLHDBC.Zero;
            SQLHANDLE hUse;

            // Retrieve the handle to the ODBC environment that should have
            // already been instantiated by a previous call to UtConnect().
            int nRet = DbGetEnv(out hODBCEnvironment);

            // Request an ODBC connection handle for the environment.
            rc = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_DBC, hODBCEnvironment, out hConn);

            // Enabe MARS capability.
            // The default ODBC behaviour is that a connection can have only one
            // active statement. To allow multiple active statements on a connection
            // we have to enable Multiple Active Result Sets (MARS) capability.
            ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_COPT_SS_MARS_ENABLED, (SQLPOINTER)ODBC.SQL_MARS_ENABLED_YES, ODBC.SQL_IS_UINTEGER);

            switch (rc)
            {
                case ODBC.SQL_SUCCESS:
                case ODBC.SQL_SUCCESS_WITH_INFO:
                    //	Allocated the handle okay, Now make the connection...
                    retCode = ODBC.SQLConnect(hConn,
                        (SQLCHARPTR)Info.DbName,
                        (SQLSMALLINT)Info.DbName.Length,
                        (SQLCHARPTR)Info.MicsUserName,
                        (SQLSMALLINT)Info.MicsUserName.Length,
                        (SQLCHARPTR)Info.Password,
                        (SQLSMALLINT)Info.Password.Length);

                    //...Log2.v("\r\nSsUtil.NewConn(): SQLConnect(): retCode = " + retCode.ToString());

                    switch (retCode)
                    {
                        case ODBC.SQL_SUCCESS:
                        case ODBC.SQL_SUCCESS_WITH_INFO:
                            //	Now we are connected to the SQL Server Database Engine.
                            //	Next we need to set the context to the actual database
                            //	the user has requested. 
                            //  retCode = SQLAllocHandle(SQL_HANDLE_STMT, hConn, &hUse);
                            retCode = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUse);
                            if (retCode == 0)
                            {
                                //                    char cUseStr[DBNAME_LEN + 5] = { "USE " };
                                //                    strcat_s(cUseStr, sizeof(cUseStr), glbDBName);
                                string cUseStr = "USE " + Info.DbName;
                                //                    retCode = SQLExecDirect(hUse, (SQLCHAR*)cUseStr, intlen(cUseStr));

                                //...Log2.v("\r\nSsUtil.NewConn(): SQLExecDirect():\r\n" + cUseStr);
                                retCode = ODBC.SQLExecDirect(hUse, cUseStr, (SQLINTEGER)cUseStr.Length);
                                if (!ODBC.IsOK((SQLRETURN)retCode))
                                {
                                    retCode = DbGetDiag(ODBC.SQL_HANDLE_STMT, hUse);
                                    GenUtil.SetErr("newConn - Error selecting database %s\r\n%s",
                                        Info.DbName, GenUtil.GetUserMess());
                                    retCode = -9;
                                }
                                else
                                {
                                    retCode = 0;
                                    // We are connected.  Set the transaction isolation level to default (read_uncommitted)
                                    DbSetIsoLevel(hConn, ODBC.SQL_TXN_READ_UNCOMMITTED);
                                }
                            }
                            else
                            {
                                retCode = -10;
                            }
                            break;

                        case ODBC.SQL_ERROR:
                            //	Put the errors in user mess.
                            DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                            GenUtil.SetErr("newConn: Could not connect-\r\n%s", GenUtil.GetUserMess());
                            retCode = -1;
                            break;

                        case ODBC.SQL_INVALID_HANDLE:
                            DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                            GenUtil.SetErr("newConn: Invalid Handle-\r\n%s", GenUtil.GetUserMess());
                            retCode = -2;
                            break;

                        default:
                            DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                            GenUtil.SetErr("newConn: Invalid Return from ODBC-\r\n%s", GenUtil.GetUserMess());
                            retCode = -3;
                            break;
                    }
                    break;

                case ODBC.SQL_INVALID_HANDLE:
                    DbGetDiag(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);
                    GenUtil.SetErr("newConn: Bad Environment Handle-\r\n%s", GenUtil.GetUserMess());
                    retCode = -4;
                    break;

                case ODBC.SQL_ERROR:
                    DbGetDiag(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);
                    GenUtil.SetErr("newConn: Environment SQL Error-\r\n%s", GenUtil.GetUserMess());
                    retCode = -5;
                    break;

                default:
                    DbGetDiag(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);
                    GenUtil.SetErr("newConn: Environment weird error-\r\n%s", GenUtil.GetUserMess());
                    retCode = -6;
                    break;
            }

            if (retCode == 0)
            {
                //...Log2.v("\r\nSsUtil.NewConn(): glbTotalConn++");
                glbTotalConn++;
                //incrementTotalConn();
            }
            else
            {
                hConn = IntPtr.Zero;
            }

            // Save the connection handle so that it can be reused.
            mStaticConnHandle = hConn;

            // Save the connection handle to the static class Info so that it is
            // available to _NewLib.
            Info.StaticConnHandle = mStaticConnHandle;

            //...Log2.v("\r\nSsUtil.NewConn(): glbTotalConn = " + glbTotalConn);
            //...Log2.v("\n\nSsUtil.NewConn(): Exit");
            return hConn;
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
            return ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_TXN_ISOLATION, (IntPtr)nLevel, 0);
        }

        /// <summary>
        /// Terminates an existing ODBC connection and frees the ODBC connection handle.
        /// </summary>
        /// <param name="hConn"> - ODBC connection handle.</param>
        /// <returns> - SQL_SUCCESS, SQL_ERROR, or SQL_INVALID_HANDLE. </returns>
        public static int DisConn(SQLHDBC hConn)
        {
            return ODBC.SQL_SUCCESS;
        }

        /// <summary>
        /// This method should be invoked before calling any ODBC routines; it acquires a 
        /// handle to the ODBC environment that is shared by all subsequent database connections; 
        /// Info.MicsUserName and Info.Password <b>must be set</b> prior to calling this method; 
        /// the 'sessionIdIn' parameter is a legacy item that is essentially ignored other than 
        /// for range check - it should be in the range [0, 5].
        /// </summary>
        /// <remarks>
        /// This method acquires and maintains the ODBC environment that later connections will use. 
        /// It sets up ODBC connection pooling within the environment. It also retrieves the 
        /// user's schema. Each individual SQL query will make its own ODBC connection and 
        /// disconnect as soon as possible.
        /// </remarks>
        /// <param name="dbNameIn"> - name of the database.</param>
        /// <param name="sessionIdIn"> - session ID assigned by the user; it must be greater than zero and less than or equal to Constant.ODBC_CONNECTION_SZ (currently 5).</param>
        /// <returns> - Constant.SUCCESS indicates successful outcome; non-zero indicates failure.</returns>
        public static int UtConnect(string dbNameIn, int sessionIdIn)
        {
            int rc;
            int retCode = Constant.SUCCESS;
            SQLHENV hEnv = SQLHENV.Zero;

            GenUtil.ReSetError();

            /* Check for sane input */
            bool inputParameterError = (sessionIdIn < 0) ||
                                        (sessionIdIn >= Constant.ODBC_CONNECTION_SZ) ||
                                        String.IsNullOrWhiteSpace(dbNameIn);
            if (inputParameterError)
            {
                Log2.e("\nSsutil.UtConnect(): ERROR: invalid input parameters: dbNameIn = {0}, sessIdIn = {1}", dbNameIn, sessionIdIn);
                retCode = Error.INVPARM;
            }
            else
            {
                /*	ODBC Connection.  We get or allocate an environment... */
                rc = DbGetEnv(out hEnv);
                if (rc != 0)
                {
                    DbGetDiag(ODBC.SQL_HANDLE_ENV, hEnv);
                    GenUtil.SetError(9914, "UtConnect: Could not get environment handle:-\r\n%s", GenUtil.GetUserMess());
                    Log2.e("\nSsutil.UtConnect(): ERROR: could not get environment handle.");
                    retCode = Error.ODBC_COULD_NOT_GET_ENV_HANDLE;
                }
                else
                {
                    // If we reach here we should have a valid ODBC environment handle.
                }

            }

            if (retCode == 0)
            {
                //	Get the user's SQL Server schema.  To do this we need to establish a connection to the
                //	designated database.  We do this without using the connection routines 
                //	because they assume the environment is already established.
                SQLRETURN sqlRet;
                SQLHANDLE hStmt = SQLHANDLE.Zero;
                SQLHDBC hConn = SQLHDBC.Zero;

                try
                {
                    sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_DBC, hEnv, out hConn);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        Log2.e("\nSsutil.UtConnect(): ERROR: A: call to ODBC SQLAllocHandle() returned " + sqlRet);
                        DbGetDiag(ODBC.SQL_HANDLE_ENV, hEnv);
                        GenUtil.SetErr("utConnect: Could not set schema:\r\n%s", GenUtil.GetUserMess());
                        retCode = Error.ODBC_COULD_NOT_GET_CONNECT_HANDLE;
                        //__leave;  
                        //There is no equivalent of the VC++ construct __leave in C#,
                        //use a goto instead.
                        goto endOfTryBlock;
                    }

                    //	Get the userid and password for the connection.  These are stored in 
                    //	the User's Windows environment.  If they are not stored, we have an error. 
                    //  We only need to do this once.
                    rc = 0;
                    if (IsFirstConnect)
                    {
                        if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                        {
                            Console.Error.Write("\n\nUtConnect(): ERROR: Info.MicsUserName not set (env. var. 'MicsUser')");
                            rc = Error.ENVVARMICSUSERNOTSET;
                        }

                        if (String.IsNullOrWhiteSpace(Info.Password))
                        {
                            Console.Error.Write("\n\nUtConnect(): ERROR: Info.Password not set (env. var. 'Password')");
                            rc = Error.NOPASSWORD;
                        }

                        IsFirstConnect = false;
                    }

                    if (rc < 0)
                    {
                        //AH: the 'too long' condition is not tested for in the MICS C++ code!?
                        GenUtil.SetErr("User or password is absent from Windows environment.");
                        Log2.e("\nSsutil.UtConnect(): ERROR: MicsUser and/or Password not set in Windows environment.");

                        retCode = rc;
                        //__leave;
                        goto endOfTryBlock;
                    }

                    // The ODBC connection to the database is via an existing Data Source Name (DSN)
                    // as present in the 'System DSN' tab of the application 'odbcad32.exe'.
                    // The legacy DSN names defined via 'odbcad32.exe' are 'fcsa', 'test' and
                    // 'Regression' which are the names of actually databases on the SQL Server -
                    // however, they could also have been called "tom', 'dick' and 'harry' and this
                    // SQLConnect() argument would have to refer to 'tom', 'dick' or 'harry'.
                    // Note: on CloudMICS the database names (and DSNs) are micsprod, micsdev and micstest.
                    // Note: on ReCloudMICS the database names (and DSNs) are remicsprod, remicsdev and remicstest.
                    string dataSourceName = dbNameIn;

                    sqlRet = ODBC.SQLConnect(hConn,
                                                dataSourceName,
                                                (SQLSMALLINT)dataSourceName.Length,
                                                Info.MicsUserName,
                                                (SQLSMALLINT)Info.MicsUserName.Length,
                                                Info.Password,
                                                (SQLSMALLINT)Info.Password.Length);

                    if (!ODBC.IsOK(sqlRet))
                    {
                        Log2.e("\n\nSsutil.UtConnect(): ERROR: call to ODBC.SQLConnect() FAILED, sqlRet = {0}", sqlRet);
                        Log2.e("\n\thConn = {0}", hConn);
                        Log2.e("\n\tdbNameIn = {0}", dbNameIn);
                        Log2.e("\n\tInfo.MicsUserName = {0}", Info.MicsUserName);
                        Log2.e("\n\tInfo.Password = {0}", Info.Password);
                        DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                        retCode = Error.ODBC_SQLCONNECT_FAILED;
                        //__leave;
                        goto endOfTryBlock;
                    }

                    //			checkIso(hConn);	// Primarily for debugging - check the isolation level default

                    sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        Log2.e("\nSsutil.UtConnect(): ERROR: B: call to ODBC SQLAllocHandle() returned " + sqlRet);
                        retCode = Error.ODBC_SQLALLOCHANDLE_FAILED;
                        //__leave;
                        goto endOfTryBlock;
                    }

                    // Determine what SQL schema is to be used.
                    // If Info.GlobalSchema has already been set then just use the existing value
                    // otherwise we need to query the database table [sys].[database_principals].
                    if (String.IsNullOrWhiteSpace(Info.GlobalSchema))
                    {
                        // The following method call returns the Ultrix ID (aka 'oper') of 'this' user (micsid);
                        // it assumes that Info.MicsUserName has already been set and uses it to lookup
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

                        //string schema = GetFCSASchema_A(Info.MicsUserName);
                        string schema = "";
                        Suutils.GetUltrixID(out schema);

                        // Check that the query to get the schema was successful.
                        if (String.IsNullOrWhiteSpace(schema))
                        {
                            Log2.e("\n\nSsutil.UtConnect(): ERROR: attempt to get user's default schema from SQL Server failed.");
                            retCode = Error.ATTEMPTTOGETSCHEMAFAILED;
                        }
                        else
                        {

                            // Save the schema to the Info static structure member.
                            Info.GlobalSchema = schema;

                            // Save the schema to the legacy static member of Ssutil.
                            Ssutil.Glb_Schema = schema;

                            //...Log2.v("\nInfo.GlobalSchema = {0}", Info.GlobalSchema);
                        }
                    }
                    else  // Info.GlobalSchema has already been set.
                    {
                        // Ensure that the legacy static property Glb_Schema of Ssutil is set accordingly.
                        Ssutil.Glb_Schema = Info.GlobalSchema;

                        // We need to make an initial call to NewConn() to establish the reusable
                        // environment and connection handles.
                        NewConn();
                    }

                endOfTryBlock:;
                }
                catch (Exception e)
                {
                    string str = Marshal.PtrToStringAnsi(ptrToglb_Schema);
                    SQLLEN size = Marshal.ReadInt64(ptrToglb_Schema_sz);
                    Log2.e("\r\nSsutil.UtConnect(): exception: " + e.Message);
                    Log2.e("\r\nSsutil.UtConnect(): stack trace: " + e.StackTrace);

                    Environment.Exit(Error.FATAL_EXCEPTION);
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

            } //if (retCode == 0)

            //...Log2.v("\n\nSsutil.UtConnect(): succeeded");
            return retCode;
        }

        /// <summary>
        /// Binds a buffer (for a string) in global memory to a parameter marker used as an output 
        /// in a SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - a valid ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="parameterValuePtr"> - an IntPtr object.</param>
        /// <param name="bufferLength"> - the size of the buffer allocated to ptrToBuffer.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int DbBindStringOutput(SQLHANDLE hStmt, int nParmNo, SQLPOINTER parameterValuePtr, SQLLEN bufferLength)
        {
            //...Log2.v("\n\nSsutil.DbBindStringOutput(): Entry");
            int returnValue = -666;
            SQLRETURN sqlRet = Constant.SUCCESS;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }
            SQLLENPTR nullInd = Marshal.AllocHGlobal(sizeof(SQLLEN));
            // Bind the return code to variable sParm1.
            sqlRet = ODBC.SQLBindParameter(
                        hStmt,                           //[Input] Statement handle.
                        (SQLUSMALLINT)nParmNo,           //[Input] Parameter number, starting at 1. 
                        ODBC.SQL_PARAM_OUTPUT,           //[Input] The directional type of the parameter.
                        ODBC.SQL_C_CHAR,                 //[Input] The C data type of the parameter.
                        ODBC.SQL_CHAR,                   //[Input] The SQL data type of the parameter. 
                        (SQLULEN)Constant.GLB_SCHEMA_SZ, //[Input] The column size of the parameter..
                        0,                               //[Input] Required decimal digits (irrelevant here).
                        parameterValuePtr,               //[Deferred Input] A pointer to a buffer for the parameter's data. 
                        bufferLength,                    //[Input/Output] Length of the ParameterValuePtr buffer in bytes. 
                        nullInd);                        //[Deferred Input] A pointer to a buffer for the parameter's length or an NTS marker etc

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nSsutil.DbBindStringOutput(): ERROR: call to SQLBindParameter() failed: " + sqlRet);
                Environment.Exit(666);
            }
            else
            {
                returnValue = Constant.SUCCESS;
            }

            //...Log2.v("\n\nSsutil.DbBindStringOutput(): Exit");
            return returnValue;
        }

        /// <summary>
        /// Binds a buffer (for an Int32) in global memory to a parameter marker used as an output
        /// in a SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - a valid ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="parameterValuePtr"> - an IntPtr object.</param>
        /// <param name="nullValuePtr"> - an IntPtr to the ODBC nullInd associated with parameterValuePtr.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int DbBindIntOutput(SQLHANDLE hStmt, int nParmNo, SQLPOINTER parameterValuePtr, SQLPOINTER nullValuePtr)
        {
            //...Log2.v("\n\nSsutil.DbBindIntOutput(): Entry");
            int returnValue = -666;
            SQLRETURN sqlRet;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            SQLLENPTR nullInd = Marshal.AllocHGlobal(sizeof(SQLLEN));

            sqlRet = ODBC.SQLBindParameter(
                        hStmt,                           //[Input] Statement handle.
                        (SQLUSMALLINT)nParmNo,           //[Input] Parameter number, starting at 1. 
                        ODBC.SQL_PARAM_OUTPUT,           //[Input] The directional type of the parameter.
                        ODBC.SQL_C_SLONG,                //[Input] The C data type of the parameter.
                        ODBC.SQL_INTEGER,                //[Input] The SQL data type of the parameter. 
                        0,                               //[Input] The column size of the parameter..
                        0,                               //[Input] Required decimal digits (irrelevant here).
                        parameterValuePtr,               //[Deferred Input] A pointer to a buffer for the parameter's data. 
                        0,                               //[Input/Output] Length of the ParameterValuePtr buffer in bytes. 
                        nullValuePtr);                   //[Deferred Input] A pointer to a buffer for the parameter's length or an NTS marker etc

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nSsutil.DbBindIntOutput(): ERROR: call to SQLBindParameter() failed: " + sqlRet);
                Environment.Exit(666);
            }
            else
            {
                returnValue = Constant.SUCCESS;
            }

            //...Log2.v("\n\nSsutil.DbBindIntOutput(): Exit");
            return returnValue;
        }

        /// <summary>
        /// Returns the name of the FCSA DB Schema; this is usually the same as the user name.
        /// </summary>
        /// <returns> - name of the FCSA DB Schema.</returns>
        public static string GetFCSASchema()
        {
            //...Log2.v("\n\nSsutil.GetFCSASchema(): Entry");
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn;

            string schema = "";

            hConn = NewConn();
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (DbBindStringOutput(hStmt, 1, ptrToglb_Schema, Constant.GLB_SCHEMA_SZ) != Constant.SUCCESS)
            {
                Log2.e("\r\nSsutil.UtConnect(): ERROR: DbBindOutString(): FAILED");
                DbGetDiagStmt(hStmt, "\r\nCould not bind string:\r\n");
                Environment.Exit(666);
            }

            //Execute an SQL procedure to get the glb_Schema name.
            string cmd = "{call dbo.FCSASchema (?)}";
            // AH: accommodating changed behaviour following installation of an update to Windows Server 2019.
            //string cmd = String.Format("{{call dbo.user_schema2022('{0}')}};", Info.MicsUserName);
            //Console.Error.Write("\nDOG: cmd = {0}", cmd);

            try
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cmd, (SQLINTEGER)cmd.Length);
            }
            catch (Exception)
            {
                Log2.e("\r\nSsutil.GetFCSASchema():  ERROR: SQLExecDirect(): Exception: Caught");
            }

            if (!ODBC.IsOK(sqlRet))
            {
                DbGetDiagStmt(hStmt, "Could not retrieve Schema 'dbo.FCSASchema'.");
                Log2.e("\r\nSsutil.GetFCSASchema(): ERROR: SQLExecDirect(): " + cmd + ": FAILED");
                Environment.Exit(666);
            }
            else
            {
                // The result of calling the SQL User-Defined Function dbo.FCSASchema is now
                // retrieved via the previously-bound pointer ptrToglb_Schema which now gives
                // the start address of a 31-character text sequence right-padded with spaces.
                schema = Marshal.PtrToStringAnsi(ptrToglb_Schema);
                schema = schema.Trim();

                SQLLEN size = Marshal.ReadInt64(ptrToglb_Schema_sz);
                //...Log2.v("\nSsutil.GetFCSASchema(): size = " + size);

                // Clear any result sets generated.
                while ((sqlRet = ODBC.SQLMoreResults(hStmt)) != ODBC.SQL_NO_DATA) ;
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            DisConn(hConn);

            //...Log2.v("\n\nSsutil.GetFCSASchema(): succeeded: retruned = " + schema);
            return schema;
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
            //...Log2.v("\n\nSsutil.DbGetEnv(): Entry");
            SQLRETURN nRet = 0;

            GenUtil.ReSetError();

            if (hODBCEnvironment == SQLHANDLE.Zero)
            {
                /*	Before allocating the environment, turn on connection pooling. */
                nRet = ODBC.SQLSetEnvAttr(
                                            SQLHENV.Zero,
                                            ODBC.SQL_ATTR_CONNECTION_POOLING,
                                            (SQLPOINTER)ODBC.SQL_CP_ONE_PER_HENV,
                                            0);

                if (!ODBC.IsOK(nRet))
                {
                    GenUtil.SetErr("DbGetEnv: Could not set up connection pooling.");
                    hOut = SQLHANDLE.Zero;
                    return -10;
                }

                nRet = ODBC.SQLAllocHandle(
                                            ODBC.SQL_HANDLE_ENV,
                                            (SQLHANDLE)ODBC.SQL_NULL_HANDLE,
                                            out hODBCEnvironment);
                switch (nRet)
                {
                    case ODBC.SQL_SUCCESS:
                    case ODBC.SQL_SUCCESS_WITH_INFO:
                        if (nRet == ODBC.SQL_SUCCESS_WITH_INFO)
                        {
                            nRet = (SQLRETURN)DbGetDiag(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);
                        }
                        /*	Set the attributes of this connection.  Assume it succeeds */
                        nRet = ODBC.SQLSetEnvAttr(
                                                    hODBCEnvironment,
                                                    ODBC.SQL_ATTR_ODBC_VERSION,
                                                    (SQLPOINTER)ODBC.SQL_OV_ODBC3,
                                                    0);
                        nRet = Constant.SUCCESS;
                        break;

                    case ODBC.SQL_INVALID_HANDLE:
                        GenUtil.SetError(9901, "DbGetEnv: Passed Invalid Environment Handle.");
                        hODBCEnvironment = SQLHANDLE.Zero;
                        nRet = -1;
                        break;

                    case ODBC.SQL_ERROR:
                        nRet = (SQLRETURN)DbGetDiag(ODBC.SQL_HANDLE_ENV, hODBCEnvironment);
                        hODBCEnvironment = SQLHANDLE.Zero;
                        nRet = -2;
                        break;

                    default:
                        GenUtil.SetError(9906, "DbGetEnv: Invalid return from allocation: %d", nRet.ToString());
                        hODBCEnvironment = SQLHANDLE.Zero;
                        nRet = -3;
                        break;
                }
            }

            hOut = hODBCEnvironment;

            //...Log2.v("\n\nSsutil.DbGetEnv(): returned nRet = " + nRet);
            return nRet;
        }

        /// <summary>
        /// Checks for the existence of a set of tables within a database that
        /// have the prescribed root table name.
        /// </summary>
        /// <remarks>When a user deletes a PDF/SDF, the corresponding record in the central_table 
        /// is marked 'Y' for delete. However,if the user wants to craete a PDF/SDF with the same 
        /// name before the nightly cleanup is run, the old PDF/SDF will get resurrected.  Therefore, 
        /// in the case when we are checking to see if a previously deleted PDF/SDF exists, we will 
        /// perform the actual drop to remove the PDF/SDF from the system.  In this case, the system 
        /// will return that the PDF/SDF does not exist.
        /// </remarks>
        /// <param name="tableType"> - prescribed type of table (e.g. Constant.FT).</param>
        /// <param name="tableName"> - the name of the database to be searched.</param>
        /// <returns> - true if table is found; else false.</returns>
        public static bool UtTableExist(int tableType, string tableName)
        {
            Console.Write("\n\nSsutil.UtTableExist(): Entry\n");
            Console.Write("\nSsutil.UtTableExist(): tableType = " + tableType + "\n");
            Console.Write("\nSsutil.UtTableExist(): tableName = " + tableName + "\n");

            bool rc = false;
            mIntName = null;
            mIntName1 = null;
            mIntName2 = null;
            mIntName3 = null;
            mIntName4 = null;
            mIntName5 = null;
            mIntName6 = null;
            mIntName7 = null;

            switch (tableType)
            {
                case Constant.FT:
                    Console.Write("\r\nSsutil.UtTableExist() case " + Constant.FT + "\n");
                    GenUtil.UtCvtName(Constant.FT_TITL, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.FT_SHRL, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.FT_SITE, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.FT_ANTE, tableName, out mIntName3);
                    GenUtil.UtCvtName(Constant.FT_CHAN, tableName, out mIntName4);
                    GenUtil.UtCvtName(Constant.FT_CHNG_CALL, tableName, out mIntName5);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1)
                        && IntTableExist(mIntName2)
                        && IntTableExist(mIntName3)
                        && IntTableExist(mIntName4)
                        && IntTableExist(mIntName5);
                    break;

                case Constant.FE:
                    Console.Write("\r\nSsutil.UtTableExist(): case " + Constant.FE + "\n");
                    GenUtil.UtCvtName(Constant.FE_TITL, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.FE_SHRL, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.FE_SITE, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.FE_AZIM, tableName, out mIntName3);
                    GenUtil.UtCvtName(Constant.FE_ANTE, tableName, out mIntName4);
                    GenUtil.UtCvtName(Constant.FE_CHAN, tableName, out mIntName5);
                    GenUtil.UtCvtName(Constant.FE_CLOC, tableName, out mIntName6);
                    GenUtil.UtCvtName(Constant.FE_CCAL, tableName, out mIntName7);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1)
                        && IntTableExist(mIntName2)
                        && IntTableExist(mIntName3)
                        && IntTableExist(mIntName4)
                        && IntTableExist(mIntName5)
                        && IntTableExist(mIntName6)
                        && IntTableExist(mIntName7);
                    break;

                case Constant.CT:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.CT");
                    GenUtil.UtCvtName(Constant.CT_SITE, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.CT_ANTE, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.CT_CHAN, tableName, out mIntName3);
                    GenUtil.UtCvtName(Constant.CT_RSLT, tableName, out mIntName4);
                    GenUtil.UtCvtName(Constant.CT_TEMP, tableName, out mIntName5);
                    rc = IntTableExist(mIntName1)
                        && IntTableExist(mIntName2)
                        && IntTableExist(mIntName3)
                        && IntTableExist(mIntName4)
                        && IntTableExist(mIntName5);
                    break;

                case Constant.CE:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.CE");
                    GenUtil.UtCvtName(Constant.CE_SITE, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.CE_ANTE, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.CE_CHAN, tableName, out mIntName3);
                    GenUtil.UtCvtName(Constant.CE_RSLT, tableName, out mIntName4);
                    rc = IntTableExist(mIntName1)
                        && IntTableExist(mIntName2)
                        && IntTableExist(mIntName3)
                        && IntTableExist(mIntName4);
                    break;


                case Constant.AC:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.AC");
                    /*	Area Coordination tables */
                    GenUtil.UtCvtName(Constant.AC_PARM, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.AC_PERI, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.AC_RADI, tableName, out mIntName3);
                    rc = IntTableExist(mIntName1) &&
                        IntTableExist(mIntName2) &&
                        IntTableExist(mIntName3);
                    break;

                case Constant.FW_CULL:
                case Constant.SU_BAND:
                case Constant.SU_EQPT:
                case Constant.SU_NOTE:
                case Constant.SU_OCOO:
                case Constant.SU_OPER:
                case Constant.SU_ROUT:
                case Constant.SU_TOWR:
                case Constant.SU_TOWN:
                case Constant.SU_TRAF:
                case Constant.SC_ANTE:
                case Constant.SC_BAND:
                case Constant.SC_CTX:
                case Constant.SC_EQPT:
                case Constant.SC_NOTE:
                case Constant.SC_OCOO:
                case Constant.SC_OPER:
                case Constant.SC_ROUT:
                case Constant.SC_PLAN:
                case Constant.SC_TOWR:
                case Constant.SC_TOWN:
                case Constant.SC_TRAF:
                case Constant.TP_PARM:
                case Constant.PP_PARM:
                case Constant.TP_SU_EQPT:
                case Constant.TT_TEMP1:
                case Constant.TE_TEMP1:
                case Constant.TT_TEMP2:
                case Constant.BI_USAGE:
                case Constant.SE_PERM_REQ:
                case Constant.SE_PERM_USER:
                case Constant.RM_PARAM:
                case Constant.RS_PARAM:
                case Constant.RP_TS_FEE_DETAIL:
                case Constant.RP_ES_FEE_DETAIL:
                case Constant.BI_MICS_MONTH:
                case Constant.BI_MICS_YEAR:
                case Constant.BI_ULTRIX_YEAR:
                case Constant.BI_ULTRIX_MONTH:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case XXX");
                    GenUtil.UtCvtName(tableType, tableName, out mIntName);
                    rc = IntTableExist(mIntName);
                    break;

                case Constant.SU_ANTE:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.SU_ANTE");
                    GenUtil.UtCvtName(Constant.SU_ANTE, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.SU_ANTD, tableName, out mIntName1);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1);
                    //...Log2.v("\r\nSsutil.UtTableExist(): mIntName  = " + mIntName);
                    //...Log2.v("\r\nSsutil.UtTableExist(): mIntName1 = " + mIntName1);
                    break;

                case Constant.SU_PLAN:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.SU_PLAN");
                    GenUtil.UtCvtName(Constant.SU_PLAN, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.SU_PLND, tableName, out mIntName1);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1);
                    break;

                case Constant.SU_CTX:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.SU_CTX");
                    GenUtil.UtCvtName(Constant.SU_CTX, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.SU_CTXD, tableName, out mIntName1);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1);
                    break;

                case Constant.TP_SU_ANTE:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.TP_SU_ANTE");
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, tableName, out mIntName1);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1);
                    break;

                case Constant.TP_SU_PLAN:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.TP_SU_PLAN");
                    GenUtil.UtCvtName(Constant.TP_SU_PLAN, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.TP_SU_PLND, tableName, out mIntName1);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1);
                    break;

                case Constant.TP_SU_CTX:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.TP_SU_CTX");
                    GenUtil.UtCvtName(Constant.TP_SU_CTX, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.TP_SU_CTXD, tableName, out mIntName1);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1);
                    break;

                case Constant.TT:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.TT");
                    GenUtil.UtCvtName(Constant.TT_PARM, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.TT_SITE, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.TT_ANTE, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.TT_CHAN, tableName, out mIntName3);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1)
                        && IntTableExist(mIntName2)
                        && IntTableExist(mIntName3);
                    break;

                case Constant.TE:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.TE");
                    GenUtil.UtCvtName(Constant.TE_PARM, tableName, out mIntName);
                    GenUtil.UtCvtName(Constant.TE_SITE, tableName, out mIntName1);
                    GenUtil.UtCvtName(Constant.TE_ANTE, tableName, out mIntName2);
                    GenUtil.UtCvtName(Constant.TE_CHAN, tableName, out mIntName3);
                    rc = IntTableExist(mIntName)
                        && IntTableExist(mIntName1)
                        && IntTableExist(mIntName2)
                        && IntTableExist(mIntName3);
                    break;

                case Constant.TP_VIEW:
                case Constant.AT_TAB:
                case Constant.RS_TEMP_PARM:
                case Constant.UT_TABLE_LIST:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case YYY");
                    /*	Added the cvtname call to make it like the others, both here and below.
                    *		- GJS 2002.01.15 */
                    GenUtil.UtCvtName(tableType, tableName, out mIntName);
                    rc = IntTableExist(mIntName);
                    break;

                case Constant.RP_USERDEF:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case Constant.RP_USERDEF");
                    GenUtil.UtCvtName(Constant.RP_USERDEF, tableName, out mIntName);
                    //The following native call always returns false! (See ssutil.cpp)
                    //rc = repExist(mIntName);
                    rc = false;
                    break;

                case Constant.TS_ALPHA_SITE_TABLE:
                case Constant.TS_ALPHA_ANTE_TABLE:
                case Constant.TS_ALPHA_CHAN_TABLE:
                case Constant.TS_ALPHA_TOWN_TABLE:
                case Constant.ES_ALPHA_SITE_TABLE:
                case Constant.ES_ALPHA_ANTE_TABLE:
                case Constant.ES_ALPHA_CHAN_TABLE:
                    //...Log2.v("\r\nSsutil.UtTableExist(): case ZZZ");
                    GenUtil.UtCvtName(tableType, tableName, out mIntName);
                    rc = IntTableExist(mIntName);
                    break;
            } //switch (tableType)

            Console.WriteLine("\n\nSsutil.UtTableExist(): Exit, returned " + rc + "\n");
            return (rc);
        }

        /// <summary>
        /// Checks for the existence of a set of tables within a database that
        /// have the prescribed root table name and provides a list of the tables found.
        /// </summary>
        /// <remarks>When a user deletes a PDF/SDF, the corresponding record in the central_table 
        /// is marked 'Y' for delete. However,if the user wants to craete a PDF/SDF with the same 
        /// name before the nightly cleanup is run, the old PDF/SDF will get resurrected.  Therefore, 
        /// in the case when we are checking to see if a previously deleted PDF/SDF exists, we will 
        /// perform the actual drop to remove the PDF/SDF from the system.  In this case, the system 
        /// will return that the PDF/SDF does not exist.
        /// </remarks>
        /// <param name="tableType"> - prescribed type of table (e.g. Constant.FT).</param>
        /// <param name="rootTableName"> - the root name of the tables to be searched for.</param>
        /// <param name="queriedTables"> - a list of the tables found.</param>
        /// <returns> - true if table is found; else false.</returns>
        public static bool UtTableExist(int tableType, string rootTableName, out List<string> queriedTables)
        {
            // 'out' requirement.
            queriedTables = new List<string>();

            bool result = UtTableExist(tableType, rootTableName);

            if (mIntName != null) queriedTables.Add(mIntName);
            if (mIntName1 != null) queriedTables.Add(mIntName1);
            if (mIntName2 != null) queriedTables.Add(mIntName2);
            if (mIntName3 != null) queriedTables.Add(mIntName3);
            if (mIntName4 != null) queriedTables.Add(mIntName4);
            if (mIntName5 != null) queriedTables.Add(mIntName5);
            if (mIntName6 != null) queriedTables.Add(mIntName6);
            if (mIntName7 != null) queriedTables.Add(mIntName7);

            return result;
        }

        /// <summary>
        /// Check the system catalog for a table's existence; this checks a specific
        /// table, not the generic table name. The table name can have a schema
        /// prepended to it. If it does not, then the user's current schema is
        /// retrieved and that is used.
        /// </summary>
        /// <param name="tabName"> - name of DB table.</param>
        /// <returns></returns>
        public static bool IntTableExist(string tabName)
        {
            Console.WriteLine("\nIn Ssutil.IntTableExist() : tabName = {0}", tabName);

            int nCount = 0;
            string cSQL;
            string ptr;
            string cSchema;
            bool result = false;

            if (tabName.Contains("."))
            {
                //	A schema name is present, use it and strip it out of the table name.
                string[] strArray = tabName.Split('.');
                cSchema = strArray[0];
                ptr = tabName.Substring(cSchema.Length + 1);
                cSQL = "TABLE_SCHEMA='" + cSchema + "' and TABLE_NAME='" + ptr + "'";
            }
            else
            {
                //	No schema name is present in the table name.  Use the current user schema.
                //...Log2.v("\r\nSsutil.IntTableExist():: No schema name is present in the table name.  Use the current user schema.");

                cSQL = "TABLE_SCHEMA='" + Info.GlobalSchema + "' and TABLE_NAME='" + tabName + "'";
            }

            Console.WriteLine("\nSsutil.IntTableExist(): cSQL= {0}", cSQL);

            nCount = DbCountRows("INFORMATION_SCHEMA.TABLES", cSQL);

            result = nCount > 0;

            Console.WriteLine("\n\n" + cSQL);
            Console.WriteLine(String.Format("\nSsutil.IntTableExist(): tabName = {0}, nCount = {1}, result = {2}", tabName, nCount, result));
            return result;
        }

        /// <summary>
        /// Retrieves the current MICS user name; if it is not already known this method
        /// will call Environment.GetEnvironmentVariable("MicsUser").
        /// </summary>
        /// <param name="cUserName"></param>
        /// <returns> - true if succed, false if the method failed.</returns>
        public static bool GetMicsID(out string cUserName)
        {
            bool IsOK = false;
            cUserName = null;

            // If we already have it, use it.
            if (!String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                cUserName = Info.MicsUserName;
                IsOK = true;
            }
            else  // We don't already have it. Try to get it from the Windows environment.
            {
                Info.MicsUserName = Environment.GetEnvironmentVariable("MicsUser");
                if (String.IsNullOrWhiteSpace(Info.MicsUserName))
                {
                    // The environment variable "MicsUser" is not set.
                }
                else
                {
                    IsOK = true;
                }
            }
            return IsOK;
        }

        /// <summary>
        /// Retrieves the FCSA System ID for the current MICS user name.
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

            int nRet = -666;
            string cSQL = "Not Set.";
            SQLRETURN sqlRet = -666;

            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                nRet = 2;
            }
            else
            {
                //	Get the system Id for this mics user
                SQLHANDLE hStmt;
                SQLHDBC hConn = Ssutil.NewConn();
                SQLLEN nNull;

                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                cSQL = String.Format("select ultrixid from adm.account_details where micsid='{0}'", Info.MicsUserName);

                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    nRet = -1;
                    Log2.e("\n\nSsutil.GetSystemId(): ERROR: ODBC.SQLExecDirect(): sqlRet = " + sqlRet);
                    Log2.e("\nSsutil.GetSystemId(): ERROR: ODBC.SQLExecDirect(): query = " + cSQL);
                    Application.ExitQuietly(66602);
                }
                else
                {
                    sqlRet = ODBC.SQLFetch(hStmt);

                    if (ODBC.IsOK(sqlRet))
                    {
                        Ssutil.DbGetString(hStmt, 1, "ultrixid", out cSystemId, (int)nLen, out nNull);
                        if (nNull == Constant.DB_NULL)
                        {
                            //	Somehow we have a null field
                            nRet = -2;
                        }
                        else if (cSystemId.Length <= 0)
                        {
                            //	Ultrixid is blank.
                            nRet = -3;
                        }
                        else
                        {
                            //	Worked normally
                            nRet = Constant.SUCCESS;
                        }
                    }
                    else if (ODBC.IsNoData(sqlRet))
                    {
                        nRet = 1;

                        Log2.e("\n\nSsutil.GetSystemId(): ERROR: ODBC.SQLFetch(): sqlRet = SQL_NO_DATA.");
                        Log2.e("\nSsutil.GetSystemId(): ERROR: ODBC.SQLFetch(): query = " + cSQL);
                        Application.ExitQuietly(66600);
                    }
                    else
                    {
                        nRet = 6;

                        Log2.e("\n\nSsutil.GetSystemId(): ERROR: ODBC.SQLFetch(): sqlRet = " + sqlRet);
                        Log2.e("\nSsutil.GetSystemId(): ERROR: ODBC.SQLFetch(): query = " + cSQL);
                        Application.ExitQuietly(66601);
                    }
                }

                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                Ssutil.DisConn(hConn);
            }

            //...Log2.v("\nSsutil.GetSystemId(): SQL      : " + cSQL);
            //...Log2.v("\nSsutil.GetSystemId(): sqlRet   : " + sqlRet);
            //...Log2.v("\nSsutil.GetSystemId(): cSystemId: " + cSystemId);
            //...Log2.v("\nSsutil.GetSystemId(): nRet     : " + nRet);
            return nRet;
        }

        /// <summary>
        /// Provides a generic 'wrapper' around an ODBC.SQLGetData() operation for a variety of 'target' value types (e.g. string, short, int etc);
        /// it is called by the type-specific get methods such as DbGetString() etc.
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
            //...Log2.v("\n\nSsutil.DbUniversalGet(): Entry, cName = " + cName);

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
                //...Log2.v("\r\nSsutil.DbUniversalGet(): SQLGetData() FAILED for column '" + cName + "'  , sqlRet = " + sqlRet);
                throw new Exception(cName);
            }
            else
            {
                //...Log2.v("\r\nSsutil.DbUniversalGet(): SQLGetData() SUCCEEDED");

                nullInd = Marshal.ReadInt64(mNullIndPtr);

                if (nullInd == ODBC.SQL_NULL_DATA)
                {
                    nullInd = Constant.DB_NULL;
                }
                else
                {
                    nullInd = Constant.DB_NOT_NULL;
                }

            }

            //...Log2.v("\n\nSsutil.DbUniversalGet(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Get a string value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
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
        /// Get a byte (uint8) value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetValue"> - the retrieved byte value.</param>
        /// <param name="nNull"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetByte(SQLHANDLE hStmt, int nColNum, string cName, out byte targetValue, out SQLLEN nNull)
        {
            targetValue = 0;

            //This call could throw an exception; we should catch it at the next level up.
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_UTINYINT, mTargetValuePtr, Constant.SIZEOF_BYTE, out nNull);

            if (nNull != Constant.DB_NULL)
            {
                targetValue = Marshal.ReadByte(mTargetValuePtr);
            }

            //...Log2.v("\n\nSsutil.DbGetByte():  Returned: " + targetValue);
            return nRet;
        }

        /// <summary>
        /// Get the SQL BIT value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch(); the BIT value is returned as a 
        /// boolean (targetValue) that is false if the BIT is not set (0) and true if the BIT is
        /// set to 1.
        /// </summary>
        /// <param name="hStmt"></param>
        /// <param name="nColNum"></param>
        /// <param name="cName"></param>
        /// <param name="targetValue"></param>
        /// <param name="nNull"></param>
        /// <returns></returns>
        public static int DbGetBit(SQLHANDLE hStmt, int nColNum, string cName, out bool targetValue, out SQLLEN nNull)
        {
            targetValue = false;

            //This call could throw an exception; we should catch it at the next level up.
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_BIT, mTargetValuePtr, Constant.SIZEOF_BYTE, out nNull);

            if (nNull != Constant.DB_NULL)
            {
                byte b = Marshal.ReadByte(mTargetValuePtr);

                targetValue = (b == 0) ? false : true;

                //Console.Error.Write("\nDbGetBit(): b = {0,8}", Convert.ToString(b, 2));
            }

            //...Log2.v("\n\nSsutil.DbGetBit():  Returned: " + targetValue);
            return nRet;
        }

        /// <summary>
        /// Get a short (int16) value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetValue"> - the retrieved short value.</param>
        /// <param name="nNull"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetShort(SQLHANDLE hStmt, int nColNum, string cName, out short targetValue, out SQLLEN nNull)
        {
            targetValue = 0;

            //This call could throw an exception; we should catch it at the next level up.
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_SSHORT, mTargetValuePtr, Constant.SIZEOF_SHORT, out nNull);

            if (nNull != Constant.DB_NULL)
            {
                targetValue = Marshal.ReadInt16(mTargetValuePtr);
            }

            //                                               $
            //...Log2.v("\n\nSsutil.DbGetShort():  Returned: " + targetValue);
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
        /// Get a long (int64) value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetValue"> - the retrieved long value.</param>
        /// <param name="nNull"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetLong(SQLHANDLE hStmt, int nColNum, string cName, out long targetValue, out SQLLEN nNull)
        {
            targetValue = 0;

            //This call could throw an exception; we should catch it at the next level up.

            //DbGetLong() returns a C# 'long'. The corresponding 'C' type is 'long long'. 
            //The only SQL / 'C' Data Type that corresponds to C# 'long' is SQL_C_SBIGINT.
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_SBIGINT, mTargetValuePtr, Constant.SIZEOF_LONG, out nNull);

            if (nNull != Constant.DB_NULL)
            {
                targetValue = Marshal.ReadInt64(mTargetValuePtr);
            }

            //                                               $
            //...Log2.v("\n\nSsutil.DbGetInt():    Returned: " + targetValue);
            return nRet;
        }

        /// <summary>
        /// Get a float (32-bit) value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetValue"> - the retrieved float value.</param>
        /// <param name="nNull"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetFloat(SQLHANDLE hStmt, int nColNum, string cName, out float targetValue, out SQLLEN nNull)
        {
            targetValue = 0;

            //This call could throw an exception; we should catch it at the next level up.      
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_FLOAT, mTargetValuePtr, Constant.SIZEOF_FLOAT, out nNull);

            if (nNull != Constant.DB_NULL)
            {

                Marshal.Copy(mTargetValuePtr, mFloatArray, 0, 1);
                targetValue = mFloatArray[0];
            }

            //...Log2.v("\n\nSsutil.DbGetFloat():  Returned: " + targetValue);
            return nRet;
        }

        /// <summary>
        /// Get a double (64-bit) value for a prescribed column in a DB record (row) previously read
        /// from a DB table using a call to ODBC.SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - statement handle previously used for ODBC.SQLFetch().</param>
        /// <param name="nColNum"> - the prescribed column number (starts at 1).</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="targetValue"> - the retrieved double value.</param>
        /// <param name="nNull"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbGetDouble(SQLHANDLE hStmt, int nColNum, string cName, out double targetValue, out SQLLEN nNull)
        {
            targetValue = 0;

            //This call could throw an exception; we should catch it at the next level up.      
            int nRet = DbUniversalGet(hStmt, nColNum, cName, ODBC.SQL_C_DOUBLE, mTargetValuePtr, Constant.SIZEOF_DOUBLE, out nNull);

            if (nNull != Constant.DB_NULL)
            {
                Marshal.Copy(mTargetValuePtr, mDoubleArray, 0, 1);
                targetValue = mDoubleArray[0];
            }

            //                                               $
            //...Log2.v("\n\nSsutil.DbGetDouble(): Returned: " + targetValue);
            return nRet;
        }

        /// <summary>
        /// Binds a string value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>
        /// <param name="nStrSize"> - the maximum string size.</param>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindStringInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLULEN nStrSize, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            string parameterValue = Marshal.PtrToStringAnsi(parameterValuePtr);
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);

            //string str = String.Format("\r\nSsUtil.DbBindStringInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}, nStrSize = {3}", cName, parameterValue, nNullValue, nStrSize);
            //...Log2.v(str);

            //Note: parameter #6 inputs the number of characters in the parameterValuePtr buffer.
            //      The P/Invoke Marshal.StringToHGlobalAnsi() method copies a C# string into global memory
            //      and appends a final \0 char. Hence we need to use nStrSize + 1 as the size of the
            //      parameterValuePtr buffer.

            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_CHAR,
                               ODBC.SQL_CHAR, nStrSize + 1, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                //...Log2.v("\nSsutil.DbBindStringInput(): cName = " + cName);
                string str = "\nSsutil.DbBindStringInput(): ERROR: SQLBindParameter() returned " + sqlRet;
                Exception e = new Exception(cName + str);
            }

            return 0;
        }

        /// <summary>
        /// Binds a byte (uint8) value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>m>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindByteInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            byte parameterValue = Marshal.ReadByte(parameterValuePtr);
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);
            //string str = String.Format("\r\nSsUtil.DbBindByteInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}", cName, parameterValue, nNullValue);
            //...Log2.v(str);

            //Note: SQL_TINYINT indicates SQL type 'tinyint' that is equivalent to C# unsigned byte (UInt8).
            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_UTINYINT,
                ODBC.SQL_TINYINT, 0, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSsutil.DbBindByteInput(): ERROR: SQLBindParameter() returned " + sqlRet);
                Exception e = new Exception(cName);
            }

            return 0;
        }

        /// <summary>
        /// Binds a short (int16) value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindShortInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            short parameterValue = Marshal.ReadInt16(parameterValuePtr);
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);

            //string str = String.Format("\r\nSsUtil.DbBindShortInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}", cName, parameterValue, nNullValue);
            //...Log2.v(str);

            //Note: SQL_SMALLINT indicates SQL type 'smallint' that is equivalent to C# short (Int16).
            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_SSHORT,
                ODBC.SQL_SMALLINT, 0, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSsutil.DbBindShortInput(): ERROR: SQLBindParameter() returned " + sqlRet);
                Exception e = new Exception(cName);
            }

            return 0;
        }

        /// <summary>
        /// Binds an int (int32) value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindIntInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            int parameterValue = Marshal.ReadInt32(parameterValuePtr);
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);

            //string str = String.Format("\r\nSsUtil.DbBindIntInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}", cName, parameterValue, nNullValue);
            //...Log2.v(str);

            //Note: SQL_INTEGER indicates SQL type 'integer' that is equivalent to C# int (Int32).
            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_DEFAULT,
                ODBC.SQL_INTEGER, 0, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSsutil.DbBindIntInput(): ERROR: SQLBindParameter() returned " + sqlRet);
                Exception e = new Exception(cName);
            }

            return 0;
        }

        /// <summary>
        /// Binds a long (int64) value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindLongInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            long parameterValue = Marshal.ReadInt64(parameterValuePtr);
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);

            //string str = String.Format("\r\nSsUtil.DbBindIntInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}", cName, parameterValue, nNullValue);
            //...Log2.v(str);

            //Note: SQL_BIGINT indicates SQL type 'bigint' that is equivalent to C# long (Int64).
            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_SBIGINT,
                ODBC.SQL_BIGINT, 0, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSsutil.DbBindLongInput(): ERROR: SQLBindParameter() returned " + sqlRet);
                Exception e = new Exception(cName);
            }

            return 0;
        }

        /// <summary>
        /// Binds a float (32-bit) value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindFloatInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            float[] floatArray = new float[1];
            Marshal.Copy(parameterValuePtr, floatArray, 0, 1);
            float parameterValue = floatArray[0];
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);

            //string str = String.Format("\r\nSsUtil.DbBindFloatInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}", cName, parameterValue, nNullValue);
            //...Log2.v(str);

            //Note:  SQL_REAL identifies an SQL 'type' of 'real' that is 32-bits.
            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_FLOAT,
                ODBC.SQL_REAL, 0, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSsutil.DbBindFloatInput(): ERROR: SQLBindParameter() returned " + sqlRet);
                Exception e = new Exception(cName);
            }

            return 0;
        }

        /// <summary>
        /// Binds a double (64-bit) value, previously written into a buffer in unmanaged memory, to a prescribed parameter marker used 
        /// in an SQL statement during a subsequent call to ODBC.SQLExecute() or ODBC.SQLExecDirect().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="nParmNo"> - ODBC parameter number, starting at 1.</param>
        /// <param name="cName"> - the name of the column in the table.</param>
        /// <param name="parameterValuePtr"> - IntPtr to the buffer in unmanaged memory.</param>
        /// <param name="strLen_or_IndPtr"> - DB_NULL or DB_NOT_NULL.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the 'get' attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DbBindDoubleInput(SQLHANDLE hStmt, int nParmNo, string cName, SQLPOINTER parameterValuePtr, SQLLENPTR strLen_or_IndPtr)
        {

            SQLRETURN sqlRet = 0;

            if (nParmNo == 0)
            {
                nParmNo = ++glbNextColumnForBind;
            }
            else
            {
                glbNextColumnForBind = nParmNo;
            }

            double[] doubleArray = new double[1];
            Marshal.Copy(parameterValuePtr, doubleArray, 0, 1);
            double parameterValue = doubleArray[0];
            SQLLEN nNullValue = Marshal.ReadInt64(strLen_or_IndPtr);

            //string str = String.Format("\r\nSsUtil.DbBindDoubleInput(): name = {0}, value = |{1}|, *strLen_or_IndPtr = {2}", cName, parameterValue, nNullValue);
            //...Log2.v(str);

            if (parameterValue == double.MaxValue && nNullValue != Constant.DB_NULL)
            {
                Log2.e("\r\nSsutil.DbBindDoubleInput(): ERROR: nullInd discrepency");
            }

            //Note:  SQL_FLOAT identifies an SQL 'type' of 'float' that is 64-bits.
            sqlRet = ODBC.SQLBindParameter(hStmt, (SQLUSMALLINT)nParmNo, ODBC.SQL_PARAM_INPUT, ODBC.SQL_C_DOUBLE,
                ODBC.SQL_FLOAT, 0, 0, parameterValuePtr, 0, strLen_or_IndPtr);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nSsutil.DbBindDoubleInput(): ERROR: SQLBindParameter() returned " + sqlRet);
                Exception e = new Exception(cName);
            }

            return 0;
        }

        /// <summary>
        /// Frees an ODBC statement handle, closes an ODBC connection and frees its connection handle.
        /// </summary>
        /// <param name="hConn"> - ODBC connection handle to be closed and freed.</param>
        /// <param name="hStmt"> - ODBC statement handle freed.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the attempt succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int DisConnStmt(SQLHDBC hConn, SQLHSTMT hStmt)
        {
            return ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
        }

        /// <summary>
        /// Retrieves diagnostic information for the previous ODBC call at the statement
        /// level; this information is then accumulated in a list managed by GenUtil.
        /// The caller can also provide a user-defined error message to further context
        /// to the diagnostic information.
        /// </summary>
        /// <param name="hHnd"> - an ODBC statement handle.</param>
        /// <param name="cFormat"> - an additional user-defined error message.</param>
        /// <returns> - always returns Constant.SUCCESS.</returns>
        public static int DbGetDiagStmt(SQLHANDLE hHnd, string cFormat)
        {
            //...Log2.v("\n\nSsutil.DbGetDiagStmt(): Entry");

            SQLRETURN nRet;
            SQLSMALLINT nRec = 1;
            SQLCHARPTRINOUT SQLStatePtr = Marshal.AllocHGlobal(5 + 1);
            SQLINTEGERPTR NativeErrorPtr = Marshal.AllocHGlobal(sizeof(SQLINTEGER));
            SQLCHARPTRINOUT MessageTextPtr = Marshal.AllocHGlobal(Constant.GETDIAGS_BUFFER_SZ);
            const SQLSMALLINT BufferLength = (SQLSMALLINT)Constant.GETDIAGS_BUFFER_SZ;
            SQLSMALLINTPTR TextLengthPtr = Marshal.AllocHGlobal(sizeof(SQLSMALLINT)); ;
            bool IsStillMore = true;
            SQLINTEGER nColNo = SQLINTEGER.MinValue;
            string txt;
            //string str;

            GenUtil.SetError(9910, cFormat);

            txt = "N/A";

            //Read all the diagnostic records that are available.
            while (IsStillMore)
            {
                //Get the diagnostic record whose index is nRec.
                nRet = ODBC.SQLGetDiagRec(ODBC.SQL_HANDLE_STMT, hHnd, nRec, SQLStatePtr, NativeErrorPtr,
                    MessageTextPtr, BufferLength, TextLengthPtr);

                switch (nRet)
                {
                    case ODBC.SQL_NO_DATA:
                        //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagRec(): case SQL_NO_DATA:");
                        IsStillMore = false;
                        break;

                    case ODBC.SQL_SUCCESS:
                    case ODBC.SQL_SUCCESS_WITH_INFO:
                        //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagRec(): case SQL_SUCCESS or SQL_SUCCESS_WITH_INFO:");

                        //Get the diagnostic results.
                        string SQLState = Marshal.PtrToStringAnsi(SQLStatePtr);
                        string MessageText = Marshal.PtrToStringAnsi(MessageTextPtr);

                        //GenUtil.SetError(9912, "%s\r\n(%s) %s", (char*)malloccopy(usermess), SQLState, MessageText);                    
                        txt = String.Format("{0}\r\n({1}) {2}", GenUtil.GetUserMess(), SQLState, MessageText);
                        GenUtil.SetError(9912, txt);
                        //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagRec(): " + txt);

                        //	Get more information about a field
                        //nRet = SQLGetDiagField(SQL_HANDLE_STMT, hHnd, nRec,
                        //    SQL_DIAG_COLUMN_NUMBER, &nColNo, SQL_IS_INTEGER, &nRetLen);
                        SQLPOINTER nColNoPtr = Marshal.AllocHGlobal(sizeof(SQLINTEGER));
                        Marshal.WriteInt64(nColNoPtr, 0);  //As per Microsoft's instructions.
                        SQLSMALLINTPTR stringLengthPtr = Marshal.AllocHGlobal(sizeof(SQLSMALLINT)); //Not actually used for case of SQL_DIAG_COLUMN_NUMBER.

                        nRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, nRec, ODBC.SQL_DIAG_COLUMN_NUMBER, nColNoPtr, ODBC.SQL_IS_INTEGER, stringLengthPtr);

                        //if (nRet != SQL_ERROR && nColNo >= 0)
                        if (nRet != ODBC.SQL_ERROR)
                        {
                            if (nColNoPtr != IntPtr.Zero)
                            {
                                //IMPORTANT! - Microsoft's function definition for SQLGetDiagField() states that
                                //             the returned value written to the pointer nColNoPtr is an SQLINTEGER.
                                //             For 64-bit Windows an SQLINTEGER is Int64. However, an analysis of the
                                //             contents of the nColNoPtr buffer indicates that only 4 bytes are being 
                                //             written, i.e. Int32.
                                nColNo = Marshal.ReadInt32(nColNoPtr);

                                if (nColNo >= 0)
                                {
                                    //seterror(9912, "%s\r\nColumn Number was: %d", (char*)malloccopy(usermess), nColNo);
                                    txt = String.Format("{0}\r\nColumn Number was: {1}", GenUtil.GetUserMess(), nColNo);
                                    GenUtil.SetError(9912, txt);
                                    //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagField(): " + txt);
                                }
                                else
                                {
                                    //switch (nColNo)
                                    //{
                                    //    case ODBC.SQL_NO_COLUMN_NUMBER:
                                    //        str = "  = SQL_NO_COLUMN_NUMBER ";
                                    //        break;
                                    //    case ODBC.SQL_COLUMN_NUMBER_UNKNOWN:
                                    //        str = "  = SQL_COLUMN_NUMBER_UNKNOWN ";
                                    //        break;
                                    //    default:
                                    //        str = "";
                                    //        break;

                                    //}
                                    //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagField(): Invalid column number: " + nColNo);
                                }
                            }
                            else
                            {
                                //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagField(): FAIL:  returned a NULL pointer for nColNo.");
                            }
                        }
                        else
                        {
                            Log2.e("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagField(): FAIL: returned SQL_ERROR.");
                        }

                        break;

                    case ODBC.SQL_ERROR:
                        Log2.e("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagRec(): case SQL_ERROR:");
                        //GenUtil.SetError(9913, "%sODBC Diagnostic ERROR: RecNumber: %d, BuffLen: %d", (char*)malloccopy(usermess), nRec, BufferLength);
                        txt = String.Format("{0} ODBC Diagnostic ERROR: RecNumber: {1}, BuffLen: {2}", GenUtil.GetUserMess(), nRec, BufferLength);
                        GenUtil.SetError(9913, txt);
                        IsStillMore = false;
                        break;

                    case ODBC.SQL_INVALID_HANDLE:
                        //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagRec(): case SQL_INVALID_HANDLE:");
                        //GenUtil.SetError(9914, "%sODBC Diagnostic ERROR: Invalid Handle.", (char*)malloccopy(usermess));
                        txt = String.Format("{0} ODBC Diagnostic ERROR: Invalid Handle.", GenUtil.GetUserMess());
                        GenUtil.SetError(9914, txt);
                        IsStillMore = false;
                        break;

                    default:
                        //GenUtil.SetError(9915, "%sODBC Diagnostic unrecognized return: %d", (char*)malloccopy(usermess), nRet);
                        //...Log2.v("\r\nSsutil.DbGetDiagStmt(): SQLGetDiagRec(): case default:");
                        txt = String.Format("{0} ODBC Diagnostic unrecognized return: {1}", GenUtil.GetUserMess(), nRet);
                        GenUtil.SetError(9915, txt);
                        IsStillMore = false;
                        break;
                }

                nRec++;
            }

            //freemalloccopyptr();    // In case it was allocated.
            //C# takes care of all memory release.

            //...Log2.v("\n\nSsutil.DbGetDiagStmt_WIP(): Exit");
            return 0;
        }

        /// <summary>
        /// Delete rows in a prescribed DB table using a current ODBC connection handle provided.  
        /// This method is used if the connection is special, like it is in manual commit mode.
        /// The caller may prescribe a SQL 'WHERE' condition  to add specificity.
        /// for 1283 - GJS - 2009.04
        /// </summary>
        /// <param name="hConn"> - ODBC connection handle.</param>
        /// <param name="cTable"> - prescribed name of DB table.</param>
        /// <param name="cCondition"> - parameters for an SQL 'WHERE' condition.</param>
        /// <returns></returns>
        /// <para> - a non-negative value - the number of rows deleted.</para>
        /// <para>  any negative number - the attempt failed.</para>
        public static int DbDeleteRowsConn(SQLHANDLE hConn, string cTable, string cCondition)
        {
            string cBuf;
            int nCount = 0;
            //short	sNull = 0;

            SQLHANDLE hStmt = IntPtr.Zero;
            SQLRETURN sqlRet = 0;

            if (hConn == IntPtr.Zero)
            {
                GenUtil.SetErr("dbDeleteRowsConn - Null Connection.");
                return -4;
            }

            if (String.IsNullOrWhiteSpace(cTable))
            {
                GenUtil.SetError(9925, "dbDeleteRows - Null table entered.");
                return -1;
            }

            cBuf = String.Format("delete from {0} ", cTable);

            if (!String.IsNullOrWhiteSpace(cCondition))
            {
                cBuf += " where " + cCondition;
            }

            //...Log2.v("\nSsutil.DbDeleteRows(): cBuf = " + cBuf);

            try
            {
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
                sqlRet = ODBC.SQLExecDirect(hStmt, cBuf, cBuf.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    if (sqlRet == ODBC.SQL_NO_DATA)
                    {
                        // Nothing to delete.
                    }
                    else
                    {
                        // Something really bad happened.
                        Log2.e("\nSsutil.DbDeleteRows(): ERROR: call to SQLExecDirect() failed, sqlRet = " + sqlRet);
                        nCount = -666;
                    }
                }
                else
                {
                    // Get the number of records deleted.
                    IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
                    ODBC.SQLRowCount(hStmt, intPtr);
                    nCount = (int)Marshal.ReadInt64(intPtr);
                }

            }
            catch (Exception e)
            {
                Log2.e("\nSsutil.DbDeleteRows(): ERROR: call to SQLExecDirect() failed: exception: " + e.Message);
                DbGetDiagStmt(hStmt, "dbDeleteRows: Error on " + e.Message + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                nCount = -3;
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            return nCount;
        }

        /// <summary>
        /// Delete rows in a prescribed DB table; the caller may prescribe a 
        /// SQL 'WHERE' condition to add specificity. 
        /// for 1283 - GJS - 2009.04
        /// </summary>
        /// <param name="cTable"> - prescribed name of DB table.</param>
        /// <param name="cCondition"> - parameters for an SQL 'WHERE' condition.</param>
        /// <returns></returns>
        /// <para> - a non-negative value - the number of rows deleted.</para>
        /// <para>  any negative number - the attempt failed.</para>
        public static int DbDeleteRows(string cTable, string cCondition)
        {
            int nCount = 0;
            SQLHDBC hConn = Ssutil.NewConn();

            nCount = DbDeleteRowsConn(hConn, cTable, cCondition);

            Ssutil.DisConn(hConn);

            return nCount;
        }

        /// <summary>
        /// Returns a string containing the MICS root directory (e.g. "d:\prod\").
        /// </summary>
        /// <param name="dbname"> - name of database (e.g. "fcsa").</param>
        /// <returns> - MICS root directory.</returns>
        public static string GetMicsRoot(string dbname)
        {
            string pEnvVar = Info.MicsRootDir;

            // if we already have it, use it.
            if (pEnvVar != null)
            {
                mGlbRootBuff = pEnvVar;
            }
            else if (dbname != null)
            {
                /*  We have the database name in cNameArea.  Now we decide from this
                *   what the path should be.  */
                string cdbname;

                mGlbRootBuff = GetFcsaDisk();
                cdbname = dbname.ToLower();
                if (cdbname.Equals("fcsa"))
                {
                    mGlbRootBuff += "\\prod\\";
                }
                else if (cdbname.Equals("test"))
                {
                    mGlbRootBuff += "\\test\\";
                }
                else if (cdbname.Equals("regression"))
                {
                    mGlbRootBuff += "\\regression\\";
                }
                else // anything else
                {
                    mGlbRootBuff += "\\prod\\";
                }
            }
            else
            {
                /*  We could not find the database, just call the program. */
                mGlbRootBuff = "";
            }

            //...Log2.v("\nSsutil.GetMicsRoot(): returns: {0}", mGlbRootBuff);

            return mGlbRootBuff;     /*	This just returns our root directory.  */
        }

        /// <summary>
        /// Returns the drive ID that the MICS program is running on (e.g. "d:").
        /// </summary>
        /// <returns> - drive ID.</returns>
        public static string GetFcsaDisk()
        {
            string cDisk = null;

            // If we already have it, use it.
            if (!String.IsNullOrWhiteSpace(Info.FcsaDisk))
            {
                cDisk = Info.FcsaDisk;
            }
            else // Try to get it from an environment variable.
            {
                //cDiskTemp = Environment.GetEnvironmentVariable("FCSADisk");

                //if (!String.IsNullOrWhiteSpace(cDiskTemp))
                //{
                //    // We found the environment variable and it has a value set.
                //    cDisk = cDiskTemp;
                //}
                //else
                //{
                // Use the system functions to get the root drive of the current directory.
                cDisk = Path.GetPathRoot(Process.GetCurrentProcess().MainModule.FileName);
                // We need to remove the final '\' character.
                cDisk = cDisk.Replace(@"\", "");
                //}

                // Remember it for next time.
                Info.FcsaDisk = cDisk;
            }

            //...Log2.v("\nSsutil.GetFcsaDisk(): returns: {0}", cDisk);

            return cDisk;
        }

        /// <summary>
        /// This method return the path to the MICS temporary directory, e.g. "d:\temp\"
        /// </summary>
        /// <returns></returns>
        public static string GetFcsaTemp()
        {
            return GetFcsaDisk() + @"\temp\";
        }

        /// <summary>
        /// The purpose of this function is to allow for the dropping of a specific 
        /// ingress table.  
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int UtDropTable(int tableType, string tableName)
        {
            /* Local variables */
            string intName;
            int rc = Constant.SUCCESS;

            switch (tableType)
            {
                case Constant.FT:
                    GenUtil.UtCvtName(Constant.FT_TITL, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_SHRL, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_SITE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_CHAN, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_CHNG_CALL, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        return (rc);
                    }
                    break;

                case Constant.FE:
                    GenUtil.UtCvtName(Constant.FE_TITL, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_SHRL, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_AZIM, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_SITE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CHAN, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CLOC, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CCAL, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        return (rc);
                    }
                    break;

                case Constant.CT:
                    GenUtil.UtCvtName(Constant.CT_SITE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_CHAN, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_TEMP, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_RSLT, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.CE:
                    GenUtil.UtCvtName(Constant.CE_SITE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_CHAN, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_RSLT, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.AC:
                    /*	Area Coordination tables */
                    GenUtil.UtCvtName(Constant.AC_PARM, tableName, out intName);
                    if ((rc = DropTable(intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                    }
                    GenUtil.UtCvtName(Constant.AC_PERI, tableName, out intName);
                    if ((rc = DropTable(intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                    }
                    GenUtil.UtCvtName(Constant.AC_RADI, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.SU_BAND:
                case Constant.SU_EQPT:
                case Constant.SU_NOTE:
                case Constant.SU_OCOO:
                case Constant.SU_OPER:
                case Constant.SU_ROUT:
                case Constant.SU_TOWR:
                case Constant.SU_TOWN:
                case Constant.SU_TRAF:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        return (rc);
                    }
                    break;

                case Constant.FW_CULL:
                case Constant.SC_ANTE:
                case Constant.SC_BAND:
                case Constant.SC_CTX:
                case Constant.SC_EQPT:
                case Constant.SC_NOTE:
                case Constant.SC_OCOO:
                case Constant.SC_OPER:
                case Constant.SC_ROUT:
                case Constant.SC_PLAN:
                case Constant.SC_TOWR:
                case Constant.SC_TOWN:
                case Constant.SC_TRAF:
                case Constant.TP_PARM:
                case Constant.PP_PARM:
                case Constant.TP_SU_EQPT:
                case Constant.TT_TEMP1:
                case Constant.TE_TEMP1:
                case Constant.TT_TEMP2:
                case Constant.BI_USAGE:
                case Constant.SE_PERM_REQ:
                case Constant.SE_PERM_USER:
                case Constant.RM_PARAM:
                case Constant.RS_PARAM:
                case Constant.RP_TS_FEE_DETAIL:
                case Constant.RP_ES_FEE_DETAIL:
                case Constant.BI_MICS_MONTH:
                case Constant.BI_ULTRIX_MONTH:
                case Constant.BI_MICS_YEAR:
                case Constant.BI_ULTRIX_YEAR:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.SU_ANTE:
                    GenUtil.UtCvtName(Constant.SU_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_ANTD, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        return (rc);
                    }
                    break;

                case Constant.SU_PLAN:
                    GenUtil.UtCvtName(Constant.SU_PLAN, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_PLND, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        return (rc);
                    }
                    break;

                case Constant.SU_CTX:
                    GenUtil.UtCvtName(Constant.SU_CTX, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_CTXD, tableName, out intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = DropTable(intName);
                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        return (rc);
                    }
                    break;

                case Constant.TP_SU_ANTE:
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.TP_SU_PLAN:
                    GenUtil.UtCvtName(Constant.TP_SU_PLAN, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_PLND, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.TP_SU_CTX:
                    GenUtil.UtCvtName(Constant.TP_SU_CTX, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_CTXD, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.TT:
                    GenUtil.UtCvtName(Constant.TT_PARM, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_SITE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_CHAN, tableName, out intName);
                    rc = DropTable(intName);

                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    break;

                case Constant.TE:
                    GenUtil.UtCvtName(Constant.TE_PARM, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_SITE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_ANTE, tableName, out intName);
                    rc = DropTable(intName);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_CHAN, tableName, out intName);
                    rc = DropTable(intName);

                    rc = UserInfo.UtUpdateCentralTable("D", tableName, tableType, "D", "Y");
                    break;

                case Constant.TS_ALPHA_SITE_TABLE:
                case Constant.TS_ALPHA_ANTE_TABLE:
                case Constant.TS_ALPHA_CHAN_TABLE:
                case Constant.TS_ALPHA_TOWN_TABLE:
                case Constant.ES_ALPHA_SITE_TABLE:
                case Constant.ES_ALPHA_ANTE_TABLE:
                case Constant.ES_ALPHA_CHAN_TABLE:
                    /*	In this and the two following.  added the utCvtName call and converted
                    *		the name for consistency in the calls. GJS 2002.01.15 */
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.RS_TEMP_PARM:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = DropTable(intName);
                    break;

                case Constant.RP_USERDEF:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    break;

                default:
                    rc = 100;       /* unkonwn type */
                    break;
            }

            return (rc);
        }   /* ----- End of utDropTable ----- */

        /// <summary>
        /// Drop a table and all associated records of that table from the MICS system. 
        ///  
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int DropTable(string tableName)
        {
            string buf;
            string tabName;

            SQLRETURN nRet = 0;
            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = IntPtr.Zero;
            SQLHDBC hConn = Ssutil.NewConn();

            // If the table does not already exist in the DB we have nothing to do.
            if (!IntTableExist(tableName))
            {
                return Constant.SUCCESS;
            }

            /* remove the record for this table from the project billing table */
            tabName = tableName;
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            buf = String.Format("drop table {0}", tableName);

            nRet = ODBC.SQLExecDirect(hStmt, buf, buf.Length);

            if (!ODBC.IsOK(nRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not drop " + tableName + ":-\n");
                Log2.e("\nSsutil.DropTable(): ERROR: Could not drop " + tableName + ":-\n");
            }
            else
            {
                //...Log2.v("\nSsutil.DropTable(): SQLExecDirect() succeeded: " + buf + "\n");
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return ((int)nRet);
        }

        /// <summary>
        /// This utility is similar to utDropTable except that an attempt is made to 
        /// delete all tables for the given table type regardless of wether an error 
        /// occured on a previous delete. eg. Table type Y has 4 tables associated with 
        /// it. If an error occurs while deleting the 2nd table then the 1st has been 
        /// deleted while the 2nd - 4th may still exist and the user has no way of 
        /// cleaning them up. No return code is set.  
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableName"></param>
        public static void UtCleanupTables(int tableType, string tableName)
        {
            Console.WriteLine("\nSsutil.UtCleanupTables(): Entry: " + tableType + "  " + tableName);

            string intName;

            switch (tableType)
            {
                case Constant.FT:
                    GenUtil.UtCvtName(Constant.FT_TITL, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FT_SHRL, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FT_SITE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FT_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FT_CHAN, tableName, out intName);
                    DropTable(intName);
                    //GenUtil.UtCvtName(Constant.FT_CHNG_CALL, tableName, out intName);
                    //DropTable(intName);
                    break;

                case Constant.FE:
                    GenUtil.UtCvtName(Constant.FE_TITL, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_SHRL, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_AZIM, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_SITE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_CHAN, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_CLOC, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.FE_CCAL, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.CT:
                    GenUtil.UtCvtName(Constant.CT_SITE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CT_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CT_CHAN, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CT_TEMP, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CT_RSLT, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.CE:
                    GenUtil.UtCvtName(Constant.CE_SITE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CE_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CE_CHAN, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.CE_RSLT, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.FW_CULL:
                case Constant.SU_BAND:
                case Constant.SU_EQPT:
                case Constant.SU_NOTE:
                case Constant.SU_OCOO:
                case Constant.SU_OPER:
                case Constant.SU_ROUT:
                case Constant.SU_TOWR:
                case Constant.SU_TOWN:
                case Constant.SU_TRAF:
                case Constant.SC_ANTE:
                case Constant.SC_BAND:
                case Constant.SC_CTX:
                case Constant.SC_EQPT:
                case Constant.SC_NOTE:
                case Constant.SC_OCOO:
                case Constant.SC_OPER:
                case Constant.SC_ROUT:
                case Constant.SC_PLAN:
                case Constant.SC_TOWR:
                case Constant.SC_TOWN:
                case Constant.SC_TRAF:
                case Constant.TP_PARM:
                case Constant.PP_PARM:
                case Constant.TP_SU_EQPT:
                case Constant.TT_TEMP1:
                case Constant.TE_TEMP1:
                case Constant.TT_TEMP2:
                case Constant.BI_USAGE:
                case Constant.SE_PERM_REQ:
                case Constant.SE_PERM_USER:
                case Constant.RM_PARAM:
                case Constant.RS_PARAM:
                case Constant.RP_TS_FEE_DETAIL:
                case Constant.RP_ES_FEE_DETAIL:
                case Constant.BI_MICS_MONTH:
                case Constant.BI_ULTRIX_MONTH:
                case Constant.BI_MICS_YEAR:
                case Constant.BI_ULTRIX_YEAR:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.SU_ANTE:
                    GenUtil.UtCvtName(Constant.SU_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.SU_ANTD, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.SU_PLAN:
                    GenUtil.UtCvtName(Constant.SU_PLAN, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.SU_PLND, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.SU_CTX:
                    GenUtil.UtCvtName(Constant.SU_CTX, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.SU_CTXD, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.TP_SU_ANTE:
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.TP_SU_PLAN:
                    GenUtil.UtCvtName(Constant.TP_SU_PLAN, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TP_SU_PLND, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.TP_SU_CTX:
                    GenUtil.UtCvtName(Constant.TP_SU_CTX, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TP_SU_CTXD, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.TT:
                    GenUtil.UtCvtName(Constant.TT_PARM, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TT_SITE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TT_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TT_CHAN, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.TE:
                    GenUtil.UtCvtName(Constant.TE_PARM, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TE_SITE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TE_ANTE, tableName, out intName);
                    DropTable(intName);
                    GenUtil.UtCvtName(Constant.TE_CHAN, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.TS_ALPHA_TABLE:
                    GenUtil.UtCvtName(Constant.TS_ALPHA_TABLE, tableName, out intName);
                    DropTable(intName);
                    break;

                case Constant.RP_USERDEF:
                    break;
            }
            //...Log2.v("\nSsutil.UtCleanupTables(): Exit: ");
        }

        /// <summary>
        /// Function to allow for the copying of an ingress table.  
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="sourceName"></param>
        /// <param name="destName"></param>
        /// <param name="appendFlag"></param>
        /// <returns></returns>
        public static int UtCopyTable(int tableType,
            string sourceName,
            string destName,
            bool appendFlag)
        {
            string intSourceName;
            string intDestName;
            int rc = Constant.SUCCESS;
            switch (tableType)
            {
                case Constant.FT:
                    GenUtil.UtCvtName(Constant.FT_SHRL, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FT_SHRL, destName, out intDestName);
                    rc = CopyTable(Constant.FT_SHRL, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_SITE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FT_SITE, destName, out intDestName);
                    rc = CopyTable(Constant.FT_SITE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FT_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.FT_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_CHAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FT_CHAN, destName, out intDestName);
                    rc = CopyTable(Constant.FT_CHAN, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_CHNG_CALL, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FT_CHNG_CALL, destName, out intDestName);
                    rc = CopyTable(Constant.FT_CHNG_CALL, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", destName, tableType, "N", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, destName);
                        return (rc);
                    }
                    break;

                case Constant.FE:
                    GenUtil.UtCvtName(Constant.FE_SHRL, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_SHRL, destName, out intDestName);
                    rc = CopyTable(Constant.FE_SHRL, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_AZIM, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_AZIM, destName, out intDestName);
                    rc = CopyTable(Constant.FE_AZIM, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_SITE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_SITE, destName, out intDestName);
                    rc = CopyTable(Constant.FE_SITE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.FE_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CHAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_CHAN, destName, out intDestName);
                    rc = CopyTable(Constant.FE_CHAN, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CLOC, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_CLOC, destName, out intDestName);
                    rc = CopyTable(Constant.FE_CLOC, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CCAL, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.FE_CCAL, destName, out intDestName);
                    rc = CopyTable(Constant.FE_CCAL, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", destName, tableType, "N", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, destName);
                        return (rc);
                    }
                    break;

                case Constant.CT:
                    GenUtil.UtCvtName(Constant.CT_SITE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CT_SITE, destName, out intDestName);
                    rc = CopyTable(Constant.CT_SITE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CT_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.CT_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_CHAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CT_CHAN, destName, out intDestName);
                    rc = CopyTable(Constant.CT_CHAN, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_TEMP, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CT_TEMP, destName, out intDestName);
                    rc = CopyTable(Constant.CT_TEMP, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_RSLT, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CT_RSLT, destName, out intDestName);
                    rc = CopyTable(Constant.CT_RSLT, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.CE:
                    GenUtil.UtCvtName(Constant.CE_SITE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CE_SITE, destName, out intDestName);
                    rc = CopyTable(Constant.CE_SITE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CE_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.CE_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_CHAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CE_CHAN, destName, out intDestName);
                    rc = CopyTable(Constant.CE_CHAN, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_RSLT, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.CE_RSLT, destName, out intDestName);
                    rc = CopyTable(Constant.CE_RSLT, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.AC:
                    /*	Area Coordination tables */
                    GenUtil.UtCvtName(Constant.AC_PARM, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.AC_PARM, destName, out intDestName);
                    rc = CopyTable(Constant.AC_PARM, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.AC_PERI, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.AC_PERI, destName, out intDestName);
                    rc = CopyTable(Constant.AC_PERI, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.AC_RADI, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.AC_RADI, destName, out intDestName);
                    rc = CopyTable(Constant.AC_RADI, intSourceName, intDestName, appendFlag);
                    break;


                case Constant.SU_BAND:
                case Constant.SU_EQPT:
                case Constant.SU_NOTE:
                case Constant.SU_OCOO:
                case Constant.SU_OPER:
                case Constant.SU_ROUT:
                case Constant.SU_TOWR:
                case Constant.SU_TOWN:
                case Constant.SU_TRAF:
                    GenUtil.UtCvtName(tableType, sourceName, out intSourceName);
                    GenUtil.UtCvtName(tableType, destName, out intDestName);
                    rc = CopyTable((short)tableType, intSourceName, intDestName, appendFlag);
                    rc = UserInfo.UtUpdateCentralTable("A", destName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, destName);
                        return (rc);
                    }
                    break;

                case Constant.FW_CULL:
                case Constant.SC_ANTE:
                case Constant.SC_BAND:
                case Constant.SC_CTX:
                case Constant.SC_EQPT:
                case Constant.SC_NOTE:
                case Constant.SC_OCOO:
                case Constant.SC_OPER:
                case Constant.SC_ROUT:
                case Constant.SC_PLAN:
                case Constant.SC_TOWR:
                case Constant.SC_TOWN:
                case Constant.SC_TRAF:
                case Constant.TP_SU_EQPT:
                case Constant.TT_TEMP1:
                case Constant.TE_TEMP1:
                case Constant.TT_TEMP2:
                case Constant.BI_USAGE:
                case Constant.SE_PERM_REQ:
                case Constant.SE_PERM_USER:
                case Constant.RM_PARAM:
                case Constant.RS_PARAM:
                case Constant.RP_USERDEF:
                case Constant.RP_TS_FEE_DETAIL:
                case Constant.RP_ES_FEE_DETAIL:
                case Constant.BI_MICS_MONTH:
                case Constant.BI_MICS_YEAR:
                case Constant.BI_ULTRIX_MONTH:
                case Constant.BI_ULTRIX_YEAR:
                    GenUtil.UtCvtName(tableType, sourceName, out intSourceName);
                    GenUtil.UtCvtName(tableType, destName, out intDestName);
                    rc = CopyTable((short)tableType, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.TP_PARM:
                    GenUtil.UtCvtName(tableType, sourceName, out intSourceName);
                    //		No longer needed -- If it is, change the parm file manually.
                    //		utModifyParm(intSourceName);
                    GenUtil.UtCvtName(tableType, destName, out intDestName);
                    rc = CopyTable((short)tableType, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.PP_PARM:
                    GenUtil.UtCvtName(Constant.PP_PARM, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_PARM, destName, out intDestName);
                    rc = CopyTable((short)tableType, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.SU_ANTE:
                    GenUtil.UtCvtName(Constant.SU_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.SU_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.SU_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_ANTD, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.SU_ANTD, destName, out intDestName);
                    rc = CopyTable(Constant.SU_ANTD, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", destName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, destName);
                        return (rc);
                    }
                    break;

                case Constant.SU_PLAN:
                    GenUtil.UtCvtName(Constant.SU_PLAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.SU_PLAN, destName, out intDestName);
                    rc = CopyTable(Constant.SU_PLAN, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_PLND, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.SU_PLND, destName, out intDestName);
                    rc = CopyTable(Constant.SU_PLND, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", destName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, destName);
                        return (rc);
                    }
                    break;

                case Constant.SU_CTX:
                    GenUtil.UtCvtName(Constant.SU_CTX, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.SU_CTX, destName, out intDestName);
                    rc = CopyTable(Constant.SU_CTX, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_CTXD, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.SU_CTXD, destName, out intDestName);
                    rc = CopyTable(Constant.SU_CTXD, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", destName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, destName);
                        return (rc);
                    }
                    break;

                case Constant.TP_SU_ANTE:
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.TP_SU_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, destName, out intDestName);
                    rc = CopyTable(Constant.TP_SU_ANTD, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.TP_SU_PLAN:
                    GenUtil.UtCvtName(Constant.TP_SU_PLAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_SU_PLAN, destName, out intDestName);
                    rc = CopyTable(Constant.TP_SU_PLAN, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_PLND, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_SU_PLND, destName, out intDestName);
                    rc = CopyTable(Constant.TP_SU_PLND, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.TP_SU_CTX:
                    GenUtil.UtCvtName(Constant.TP_SU_CTX, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_SU_CTX, destName, out intDestName);
                    rc = CopyTable(Constant.TP_SU_CTX, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_CTXD, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TP_SU_CTXD, destName, out intDestName);
                    rc = CopyTable(Constant.TP_SU_CTXD, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.TT:
                    GenUtil.UtCvtName(Constant.TT_PARM, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TT_PARM, destName, out intDestName);
                    rc = CopyTable(Constant.TT_PARM, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_SITE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TT_SITE, destName, out intDestName);
                    rc = CopyTable(Constant.TT_SITE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TT_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.TT_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_CHAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TT_CHAN, destName, out intDestName);
                    rc = CopyTable(Constant.TT_CHAN, intSourceName, intDestName, appendFlag);
                    break;

                case Constant.TE:
                    GenUtil.UtCvtName(Constant.TE_PARM, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TE_PARM, destName, out intDestName);
                    rc = CopyTable(Constant.TE_PARM, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_SITE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TE_SITE, destName, out intDestName);
                    rc = CopyTable(Constant.TE_SITE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_ANTE, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TE_ANTE, destName, out intDestName);
                    rc = CopyTable(Constant.TE_ANTE, intSourceName, intDestName, appendFlag);
                    if (rc != Constant.SUCCESS)
                    {
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_CHAN, sourceName, out intSourceName);
                    GenUtil.UtCvtName(Constant.TE_CHAN, destName, out intDestName);
                    rc = CopyTable(Constant.TE_CHAN, intSourceName, intDestName, appendFlag);
                    break;
            }
            //...Log2.v("\nSsutil.UtCopyTable(): returned " + rc);
            return (rc);
        }

        /// <summary>
        /// This method does nothing?!
        /// </summary>
        /// <param name="tabType"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static int CopyTable(short tabType,          /* Table type */
                                    string source,          /* source table name */
                                    string dest,            /* destination table name */
                                    bool append)           /* Append = TRUE */
        {
            return 0;
        }

        /// <summary>
        /// When copying one or several PDFs into a single PDF we must create a new 
        /// title record that consists of the destination name and 'N'ot validated. The 
        /// Title table can only have 1 record and mergeing several tables would create 
        /// several title records.  
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="titleName"></param>
        /// <returns></returns>
        public static int CreateNewTitleRec(int tableType, string titleName)
        {
            //	exec sql begin declare section;
            string intTitleName;
            string sqlCommand;
            string destName;
            //	exec sql end declare section;
            string curDate;     /* current date */
            string curTime;     /* current time */
            int rc;

            destName = titleName;
            GenUtil.UtGetDateTime(out curDate, out curTime);
            GenUtil.UtCvtName(tableType, titleName, out intTitleName);
            if (!IntTableExist(intTitleName))
            {
                /* if the title record already exists delete it.  This is just a check since this situation should not occur
                     unless there has been some sort of error that leaves half the PDF tables around */
                rc = UtDropTable(tableType, destName);
                if (rc != Constant.SUCCESS)
                {
                    Log2.w("\nSsutil.CreateNewTableRec(): call to UtDropTable() failed.");
                    return (Constant.FAILURE);
                }
            }

            // Create the new title table for this PDF.
            if (CreateTab(tableType, intTitleName) != Constant.SUCCESS)
            {
                return (Constant.FAILURE);
            }

            /* add in data to the new title table (only 1 rec allowed) */
            sqlCommand = String.Format("insert into {0} (validated, namef, mdate, mtime) values ('N', '%s', '%s', '%s')",
                intTitleName, destName, curDate, curTime);
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            rc = (int)ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);

            //AH:
            Ssutil.DisConn(hConn);

            //...Log2.v("\nSsutil.CreateNewTableRec(): returned " + rc);
            return (rc);
        }

        /// <summary>
        /// Create Table - there are a finite set of table types which can be created, 
        /// these are defined by the tabDesc structures.  
        /// </summary>
        /// <param name="tabType"> - identifier of table type</param>
        /// <param name="name"> - table name</param>
        /// <returns></returns>
        public static int CreateTab(int tabType,   /* identifier of table type */
                                    string name)     /* table name */
        {
            //...Log2.v(String.Format("\nSsutil.CreateTab(): Entry: {0}  {1}", tabType, name));

            /* Local variables */
            int ret = Constant.SUCCESS; /* return code */

            //struct tabDef *tDef;	/* Pointer to table definition */
            string buf;
            string tabName;

            /* Place table type that we are searching for into the last
             * element of the tabDefs array. This assures us of always finding
             * the table for which we search in the tabDefs array
             */
            TabDef.Table[TabDef.Table.Length - 1].tabType = tabType;

            // Search thru table for match on table type.
            TabDef foundEntry = null;
            foreach (TabDef entry in TabDef.Table)
            {
                if (entry.tabType == tabType)
                {
                    // We have found a match.
                    foundEntry = entry;
                    break;
                }
            }

            if (foundEntry.tabDesc != null)
            {
                buf = String.Format(foundEntry.tabDesc, name); /* construct create stmt */

                /* add a record for this table to the project billing table */
                tabName = name;

                if (!DbExecute(buf))
                {
                    Log2.e("\n\nSsutil.CreateTab(): ERROR: attempt to create an SQL table failed:\n" + buf);

                    ret = Constant.FAILURE;
                    Console.Write("\n" + GenUtil.GetUserMess() + "\n");
                }

            }
            else
            {
                Log2.e("\n\nSsutil.CreateTab(): ERROR: foundEntry.tabDesc is NULL for tabType = " + tabType);

                ret = Constant.FAILURE;
            }

            //...Log2.v("\nSsutil.CreateTab(): Exit: " + ret);
            return (ret);

        }   /* ----- End of createTab ----- */

        /// <summary>
        /// This method encapsulates the execution a single SQL query; returns true if the query succeeded 
        /// (no errors), otherwise false.  
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool DbExecute(string query)
        {
            SQLHANDLE hStmt = IntPtr.Zero;
            SQLRETURN sqlRet = ODBC.SQL_ERROR;
            SQLHDBC hConn = Ssutil.NewConn();
            bool outcome = false;

            glb_sqlRowCount = -1;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (ODBC.IsOK(sqlRet))
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);
                if (!ODBC.IsOKorNoData(sqlRet))
                {
                    //If it is not a success or success with info, and not end of data then it's an error.
                    Log2.e("\nSsutil.DbExecute(): call to ODBC.SQLExecDirect() failed for: " + query);
                    Log2.e("\n" + ODBC.GetDiagnostics(hStmt, query));
                }
                else
                {
                    outcome = true;
                    IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
                    ODBC.SQLRowCount(hStmt, intPtr);
                    glb_sqlRowCount = Marshal.ReadInt64(intPtr);
                }
            }
            else
            {
                Log2.e("\nSsutil.DbExecute(): call to ODBC.SQLAllocHandle() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "dbExecute Error allocating handle.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (outcome);
        }

        /// <summary>
        /// This method drops a general table; this is done without altering any of the MICS
        /// tables. (Use UtDropTable for MICS tables.)
        /// </summary>
        /// <param name="cTableName"> - name of the table to be dropped.</param>
        /// <returns> - Constant.SUCCESS if the drop attempt succeeded.</returns>
        public static int KillTable(string cTableName)
        {
            string cBuf;
            SQLHANDLE hStmt;
            SQLRETURN sqlRet = 0;

            SQLHDBC hConn = Ssutil.NewConn();

            cBuf = String.Format("drop table {0} ", cTableName);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cBuf, cBuf.Length);

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            return (int)sqlRet;
        }

        /// <summary>
        /// The parm number is the "column" sent or retrieved. If 0 is entered, then 
        /// the last + 1 column is used. So we need to be able to zero this to start.  
        /// </summary>
        /// <param name=""></param>
        public static void DbStartGets()
        {
            glbNextColumnForGet = 0;
        }

        /// <summary>
        /// Function to allow for the creation of a specfic table type by giving the 
        /// table name and type.  
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int UtCreateTable(int tableType, string tableName)
        {
            /* Local variables */
            string intName;
            int rc = Constant.FAILURE;

            Console.WriteLine("\nSsutil.UtCreateTable(): tableType = " + tableType);
            Console.WriteLine("\nSsutil.UtCreateTable(): tableName = " + tableName);

            switch (tableType)
            {
                case Constant.FT:
                    GenUtil.UtCvtName(Constant.FT_TITL, tableName, out intName);
                    if ((rc = CreateTab(Constant.FT_TITL, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: A");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_SHRL, tableName, out intName);
                    if ((rc = CreateTab(Constant.FT_SHRL, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: B");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_SITE, tableName, out intName);
                    if ((rc = CreateTab(Constant.FT_SITE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: C");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.FT_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        Console.WriteLine("\nSsutil.UtCreateTable(): return: D");
                        Log2.v("\nSsutil.UtCreateTable(): return: D");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_CHAN, tableName, out intName);
                    if ((rc = CreateTab(Constant.FT_CHAN, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        Console.WriteLine("\nSsutil.UtCreateTable(): return: E");
                        Log2.v("\nSsutil.UtCreateTable(): return: E");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FT_CHNG_CALL, tableName, out intName);
                    rc = CreateTab(Constant.FT_CHNG_CALL, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        //...Log2.v("\nSsutil.UtCreateTable(): return: F");
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, "N", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: G");
                        return (rc);
                    }
                    break;

                case Constant.FE:
                    //...Log2.v("\nSsutil.UtCreateTable(): H-0");
                    GenUtil.UtCvtName(Constant.FE_TITL, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_TITL, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: H");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_SHRL, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_SHRL, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: I");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_AZIM, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_AZIM, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: J");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_SITE, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_SITE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: K");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: L");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CHAN, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_CHAN, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: M");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CLOC, tableName, out intName);
                    if ((rc = CreateTab(Constant.FE_CLOC, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: N");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.FE_CCAL, tableName, out intName);
                    rc = CreateTab(Constant.FE_CCAL, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        //...Log2.v("\nSsutil.UtCreateTable(): return: O");
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, "N", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: P");
                        return (rc);
                    }
                    break;

                case Constant.CT:
                    GenUtil.UtCvtName(Constant.CT_SITE, tableName, out intName);
                    if ((rc = CreateTab(Constant.CT_SITE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: Q");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.CT_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: R");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_CHAN, tableName, out intName);
                    if ((rc = CreateTab(Constant.CT_CHAN, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: S");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_TEMP, tableName, out intName);
                    if ((rc = CreateTab(Constant.CT_TEMP, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: T");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CT_RSLT, tableName, out intName);
                    rc = CreateTab(Constant.CT_RSLT, intName);
                    //...Log2.v("\nSsutil.UtCreateTable(): return: U");
                    break;

                case Constant.CE:
                    GenUtil.UtCvtName(Constant.CE_SITE, tableName, out intName);
                    if ((rc = CreateTab(Constant.CE_SITE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: V");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.CE_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: W");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_CHAN, tableName, out intName);
                    if ((rc = CreateTab(Constant.CE_CHAN, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: X");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.CE_RSLT, tableName, out intName);
                    rc = CreateTab(Constant.CE_RSLT, intName);
                    break;

                case Constant.AC:
                    /*	Area Coordination tables */
                    GenUtil.UtCvtName(Constant.AC_PARM, tableName, out intName);
                    if ((rc = CreateTab(Constant.AC_PARM, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                    }
                    GenUtil.UtCvtName(Constant.AC_PERI, tableName, out intName);
                    if ((rc = CreateTab(Constant.AC_PERI, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                    }
                    GenUtil.UtCvtName(Constant.AC_RADI, tableName, out intName);
                    if ((rc = CreateTab(Constant.AC_RADI, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                    }

                    break;

                case Constant.SU_BAND:
                case Constant.SU_EQPT:
                case Constant.SU_NOTE:
                case Constant.SU_OCOO:
                case Constant.SU_OPER:
                case Constant.SU_ROUT:
                case Constant.SU_TOWR:
                case Constant.SU_TOWN:
                case Constant.SU_TRAF:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = CreateTab(tableType, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AA");
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AB");
                        return (rc);
                    }
                    break;

                case Constant.FW_CULL:
                case Constant.TP_PARM:
                case Constant.PP_PARM:
                case Constant.SC_ANTE:
                case Constant.SC_BAND:
                case Constant.SC_CTX:
                case Constant.SC_EQPT:
                case Constant.SC_NOTE:
                case Constant.SC_OCOO:
                case Constant.SC_OPER:
                case Constant.SC_ROUT:
                case Constant.SC_PLAN:
                case Constant.SC_TOWR:
                case Constant.SC_TOWN:
                case Constant.SC_TRAF:
                case Constant.TP_SU_EQPT:
                case Constant.RM_PARAM:
                case Constant.RS_PARAM:
                case Constant.BI_USAGE:
                case Constant.SE_PERM_REQ:
                case Constant.SE_PERM_USER:
                case Constant.RP_USERDEF:
                case Constant.RP_TS_FEE_DETAIL:
                case Constant.RP_ES_FEE_DETAIL:
                case Constant.BI_MICS_MONTH:
                case Constant.BI_ULTRIX_MONTH:
                case Constant.BI_MICS_YEAR:
                case Constant.BI_ULTRIX_YEAR:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = CreateTab(tableType, intName);
                    break;

                case Constant.TT_TEMP1:
                case Constant.TE_TEMP1:
                case Constant.TT_TEMP2:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = CreateTab(tableType, intName);
                    break;

                case Constant.SU_ANTE:
                    GenUtil.UtCvtName(Constant.SU_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.SU_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AC");
                        return (rc);
                    }
                    //...Log2.v("\nSsutil.UtCreateTable(): successfully created table: " + intName);

                    GenUtil.UtCvtName(Constant.SU_ANTD, tableName, out intName);
                    rc = CreateTab(Constant.SU_ANTD, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AD");
                        return (rc);
                    }
                    //...Log2.v("\nSsutil.UtCreateTable(): successfully created table: " + intName);

                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AE");
                        return (rc);
                    }
                    //...Log2.v("\nSsutil.UtCreateTable(): successfully updated: web.user_tables");

                    break;

                case Constant.SU_PLAN:
                    GenUtil.UtCvtName(Constant.SU_PLAN, tableName, out intName);
                    if ((rc = CreateTab(Constant.SU_PLAN, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AF");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_PLND, tableName, out intName);
                    rc = CreateTab(Constant.SU_PLND, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AG");
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AH");
                        return (rc);
                    }
                    break;

                case Constant.SU_CTX:
                    GenUtil.UtCvtName(Constant.SU_CTX, tableName, out intName);
                    if ((rc = CreateTab(Constant.SU_CTX, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AI");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.SU_CTXD, tableName, out intName);
                    rc = CreateTab(Constant.SU_CTXD, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AJ");
                        return (rc);
                    }
                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AK");
                        return (rc);
                    }
                    break;

                case Constant.TT:
                    GenUtil.UtCvtName(Constant.TT_PARM, tableName, out intName);
                    if ((rc = CreateTab(Constant.TT_PARM, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AL");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_SITE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TT_SITE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AM");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TT_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AN");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TT_CHAN, tableName, out intName);
                    rc = CreateTab(Constant.TT_CHAN, intName);
//////////////////////////////////////////////
                    Console.WriteLine("Before UtUpdateCentralTable-A: " + tableName + " " + tableType);
                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        Console.WriteLine("\nSsutil.UtCleanupTables(): return: AO");
                        return (rc);
                    }

                    break;

                case Constant.TE:
                    GenUtil.UtCvtName(Constant.TE_PARM, tableName, out intName);
                    if ((rc = CreateTab(Constant.TE_PARM, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AP");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_SITE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TE_SITE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AQ");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TE_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AR");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TE_CHAN, tableName, out intName);
                    rc = CreateTab(Constant.TE_CHAN, intName);

                    rc = UserInfo.UtUpdateCentralTable("A", tableName, tableType, " ", "N");
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AS");
                        return (rc);
                    }
                    break;

                case Constant.TP_SU_ANTE:
                    GenUtil.UtCvtName(Constant.TP_SU_ANTE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TP_SU_ANTE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AT");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_ANTD, tableName, out intName);
                    rc = CreateTab(Constant.TP_SU_ANTD, intName);
                    break;

                case Constant.TP_SU_CTX:
                    GenUtil.UtCvtName(Constant.TP_SU_CTX, tableName, out intName);
                    if ((rc = CreateTab(Constant.TP_SU_CTX, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AU");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_CTXD, tableName, out intName);
                    rc = CreateTab(Constant.TP_SU_CTXD, intName);
                    break;

                case Constant.TP_SU_PLAN:
                    GenUtil.UtCvtName(Constant.TP_SU_PLAN, tableName, out intName);
                    if ((rc = CreateTab(Constant.TP_SU_PLAN, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AV");
                        return (rc);
                    }
                    GenUtil.UtCvtName(Constant.TP_SU_PLND, tableName, out intName);
                    rc = CreateTab(Constant.TP_SU_PLND, intName);
                    break;

                case Constant.TS_ALPHA_SITE_TABLE:
                    GenUtil.UtCvtName(Constant.TS_ALPHA_SITE_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TS_ALPHA_SITE_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AV");
                        return (rc);
                    }
                    break;

                case Constant.TS_ALPHA_ANTE_TABLE:
                    GenUtil.UtCvtName(Constant.TS_ALPHA_ANTE_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TS_ALPHA_ANTE_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AX");
                        return (rc);
                    }
                    break;

                case Constant.TS_ALPHA_CHAN_TABLE:
                    GenUtil.UtCvtName(Constant.TS_ALPHA_CHAN_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TS_ALPHA_CHAN_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AY");
                        return (rc);
                    }
                    break;

                case Constant.TS_ALPHA_TOWN_TABLE:
                    GenUtil.UtCvtName(Constant.TS_ALPHA_TOWN_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.TS_ALPHA_TOWN_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: AZ");
                        return (rc);
                    }
                    break;

                case Constant.ES_ALPHA_SITE_TABLE:
                    GenUtil.UtCvtName(Constant.ES_ALPHA_SITE_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.ES_ALPHA_SITE_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: BA");
                        return (rc);
                    }
                    break;

                case Constant.ES_ALPHA_ANTE_TABLE:
                    GenUtil.UtCvtName(Constant.ES_ALPHA_ANTE_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.ES_ALPHA_ANTE_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: BB");
                        return (rc);
                    }
                    break;

                case Constant.ES_ALPHA_CHAN_TABLE:
                    GenUtil.UtCvtName(Constant.ES_ALPHA_CHAN_TABLE, tableName, out intName);
                    if ((rc = CreateTab(Constant.ES_ALPHA_CHAN_TABLE, intName)) != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: BC");
                        return (rc);
                    }
                    break;

                case Constant.UT_TABLE_LIST:
                case Constant.AT_TAB:
                case Constant.RS_TEMP_PARM:
                    GenUtil.UtCvtName(tableType, tableName, out intName);
                    rc = CreateTab(tableType, intName);
                    if (rc != Constant.SUCCESS)
                    {
                        UtCleanupTables(tableType, tableName);
                        //...Log2.v("\nSsutil.UtCreateTable(): return: BD");
                        return (rc);
                    }
                    break;

            }
            if (rc != Constant.SUCCESS)
            {
                UtCleanupTables(tableType, tableName);
            }

            //...Log2.v("\nSsutil.UtCreateTable(): return: ZZ, rc = " + rc);
            return (rc);

        }   /* ----- End of utCreateTable ----- */

        /// <summary>
        /// The parm number is the "column" sent or retrieved. If 0 is entered, then 
        /// the last + 1 column is used. So we need to be able to zero this to start.  
        /// </summary>
        /// <param name=""></param>
        public static void DbStartBinds()
        {
            glbNextColumnForBind = 0;
        }

        /// <summary>
        /// Show the satellites between the given longitudes from the geosats table. 
        /// 1205 - GJS - 2007. 10. 17.  
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="dStart"></param>
        /// <param name="dEnd"></param>
        public static void ShowSats(TextWriter tw, double dStart, double dEnd)
        {
            const int NORAD_ID_SZ = 9;
            const int NAME_SZ = 31;
            const int ORBIT_SZ = 9;
            const int CLONG_SZ = 9;

            double dFrom;
            double dTo;
            string NoradId;
            string Name;
            string Orbit;
            string CLong;
            SQLLEN sNullCheck;
            int nNumSats;
            string cQuery;

            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLRETURN sqlRet;

            /*	Handle the case around the date line. */
            if (dStart <= dEnd)
            {
                dFrom = dStart;
                dTo = dEnd;
            }
            else
            {
                dFrom = dEnd;
                dTo = dStart;
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cQuery = String.Format("select satNoradId, satName, satOrbit, satcLong from tsip.geosats where satLong >= {0,8:F4} and satLong <= {1,8:F4} order by satLong", dFrom, dTo);

            sqlRet = ODBC.SQLExecDirect(hStmt, cQuery, cQuery.Length);

            tw.Write("\r\n\tNorad Id Name                            Orbit    Longitude");

            nNumSats = 0;
            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    if (sqlRet == ODBC.SQL_NO_DATA)
                    {
                        // We have exhausted the list of records available to be fetched.
                        break;
                    }
                    else
                    {
                        // Something unexpected happened.
                        // Log the error and return.
                        Log2.e("\nSsutil.Showsats(): ERROR: call to SQLFetch() failed.");
                        return;
                    }
                }

                try
                {
                    Ssutil.DbStartGets();
                    Ssutil.DbGetString(hStmt, 0, "satNoradId", out NoradId, NORAD_ID_SZ, out sNullCheck);
                    Ssutil.DbGetString(hStmt, 0, "satName", out Name, NAME_SZ, out sNullCheck);
                    Ssutil.DbGetString(hStmt, 0, "satOrbit", out Orbit, ORBIT_SZ, out sNullCheck);
                    Ssutil.DbGetString(hStmt, 0, "satclong", out CLong, CLONG_SZ, out sNullCheck);
                }
                catch (Exception e)
                {
                    Log2.e("\nSsutil.Showsats(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    GenUtil.SetErr("showsats01: Error reading geosats field " + e.Message);
                    break;
                }

                tw.Write("\r\n\t{0,-8} {1,-31} {2,-8} {3,-8}", NoradId, Name, Orbit, CLong);
                nNumSats++;
            }

            tw.Write("\r\n\r\n\t{0} satellites between {1,7:F2} and {2,7:F2}\r\n",
                nNumSats, dStart, dEnd);

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return;
        }

        /// <summary>
        /// Return a double longitude with sign given the value and the sense.  
        /// </summary>
        /// <param name="dVal"></param>
        /// <param name="cSense"></param>
        /// <returns></returns>
        public static double GetLong(double dVal, string cSense)
        {
            double dRet;

            // Original 'C" below; strchr returns a pointer to the address of the 
            // first char in "nNwW" that equals the first character of string cSense.
            // Returning NULL means that no match was found.
            //if (strchr("nNwW", cSense[0]) != NULL)

            bool match = false;
            match |= Strings.FirstCharIs(cSense, 'N');
            match |= Strings.FirstCharIs(cSense, 'n');
            match |= Strings.FirstCharIs(cSense, 'W');
            match |= Strings.FirstCharIs(cSense, 'w');

            if (match)
            {
                dRet = 1.0;
            }
            else
            {
                dRet = -1.0;
            }

            return dRet * dVal;
        }

        /// <summary>
        /// This method return the full path name to the TSIP log file.
        /// </summary>
        /// <param name="dbname"> - name of database.</param>
        /// <returns></returns>
        public static String GetTsipLogFileName(string dbname)
        {
            // This returns the location and name of the tsip log file.  
            // It is used by several of the tsip execution routines.

            return GetMicsRoot(dbname) + @"files\tsiplog.log";
        }

        /// <summary>
        /// This method returns a unique TSIP job number that is used to identify this
        /// instance of TsipInitiator. The next TSIP job number is actually generated
        /// on the SQL Server via a call to the macro <b>dbo.getnextid()</b>.
        /// </summary>
        /// <param name="gateName"> - a string, typically <b>Global&#92;TSIPJOBfcsa</b> for the fcsa database.</param>
        /// <returns></returns>
        public static int GetNextNum(string gateName)
        {
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();
            int nResult = 0;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSsutil.GetNextNum(): ERROR:call to SQLAllocHandle() returned " + sqlRet);
                Ssutil.DbGetDiagStmt(hStmt, "\nCould not bind result integer:\n");
                return -1;
            }

            // We are going to call a user-defined function on the SQL Server, namely:
            // ?=call dbo.getnextid (?)
            // The first ? is an output value (the result).
            // The second ? is an input value (the gateName).
            // We need to establish bindings for both of these parameters
            // prior to executing the call on the SQL Server. 

            // Create the binding for the 1st parameter - an int output.
            SQLPOINTER param1ValPtr = Marshal.AllocHGlobal(sizeof(Int32));
            SQLPOINTER param1NullPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));

            DbBindIntOutput(hStmt, 1, param1ValPtr, param1NullPtr);

            // Create the binding for the 2nd parameter - a string input.
            SQLPOINTER param2ValPtr = Marshal.StringToHGlobalAnsi(gateName);
            SQLPOINTER param2NullPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Marshal.WriteInt64(param2NullPtr, Constant.DB_NOT_NULL);

            DbBindStringInput(hStmt, 2, "idname", param2ValPtr, (SQLULEN)gateName.Length, param2NullPtr);

            string sqlQuery = "{?=call dbo.getnextid (?)}";

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlQuery, sqlQuery.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = "Could not retrieve next number for " + gateName + ".";
                Log2.e("\nSsutil.GetNextNum(): ERROR: call to SQLExecDirect() returned " + sqlRet + ". " + str);
                Ssutil.DbGetDiagStmt(hStmt, str);
                return -2;
            }

            // Get the results.
            ODBC.SQLMoreResults(hStmt);

            nResult = Marshal.ReadInt32(param1ValPtr);

            //...Log2.v("\nSsutil.GetNextNum(): gateName = " + gateName);
            //...Log2.v("\nSsutil.GetNextNum(): nResult  = " + nResult);

            // Tidy up.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return nResult;
        }

        /// <summary>
        /// This method returns the full path name of the user's directory fom the current database;
        /// this is different between the 'land' and 'cloud' based Windows servers.
        /// </summary>
        /// <param name="dbname"> - name of database.</param>
        /// <returns></returns>
        public static string GetUserRoot(string dbname)
        {
            string cdbname;
            string glbUserRootBuff;
            UserInfoData tUser;

            // Get the network domain name on which this executable is running.
            // For the new FCSA Windows servers on the AWS cloud the domain names
            // are 'CLOUDMICSDEV' and 'CLOUDMICSPROD'; on the 'land' based servers 
            // it is 'FCSA'.
            bool isCloud = Environment.UserDomainName.ToUpper().Contains("CLOUD");

            glbUserRootBuff = GetFcsaDisk();
            glbUserRootBuff += @"\inetpub\";

            if (isCloud)
            {
                glbUserRootBuff += dbname + @"\mics";
            }
            else // Is a land-based KOZA server.
            {
                glbUserRootBuff += @"wwwroot\";

                cdbname = dbname.ToLower();

                // If the database is fcsa use mics, if anything else append it.
                if (!cdbname.Equals("fcsa"))
                {
                    glbUserRootBuff += cdbname;
                }
                else
                {
                    glbUserRootBuff += "mics";
                }
            }

            glbUserRootBuff += @"\userdirs\";

            UserInfo.UtGetUserInfo(out tUser);

            glbUserRootBuff += tUser.oper;
            glbUserRootBuff += @"\";
            glbUserRootBuff += tUser.micsUser.micsid;

            return glbUserRootBuff;     /*	This returns the user's root directory for TSIP reports etc.  */
        }

        /// <summary>
        /// This method returns the full path name of the .exe file corresponding to 
        /// a prescribed MICS program; for a WebMICS user the returned path would be
        /// like "d:\prod\bin"; for developmental purposes, the returned value of this method
        /// is the private member value mMicsBinDirPath if this was previously set to a 
        /// non-trivial value using the method SetMicsBinDirPath().
        /// </summary>
        /// <param name="micsProgramName"> - name of program, e.g. TpRunTsip.</param>
        /// <param name="dbname"> - name of the database.</param>
        /// <returns></returns>
        public static string GetBinPath(string micsProgramName, string dbname)
        {
            // If the static variable mMicsBinPath is set then re-use its value.
            // If mMicsBinDirPath is not set we have to deduce the bin 
            // directory path using GetMicsRoot().
            if (String.IsNullOrWhiteSpace(mMicsBinDirPath))
            {
                // GetMicsRoot() should return "c:\prod" or "d:\prod" depending on the 
                // public-facing FCSA server configuration. The worst case is that it
                // returns an empty string.
                string micsRootDir = GetMicsRoot(dbname);

                //...Log2.v("\nSsutil.GetBinPath(): micsRootDir =  " + micsRootDir);

                // Check that GetMicsRoot() returned something useful.
                if (String.IsNullOrWhiteSpace(micsRootDir))
                {
                    Log2.e("\n\nSsutil.GetBinPath(): ERROR: call to method GetMicsRoot() failed, returned: " + Strings.AddBars(micsRootDir));
                    Application.Exit(Error.MICSBINDIRNOTSET);
                }

                // In a real deployment, the directory containing the MICS# binaries is \bin
                // under micsRootDir.
                mMicsBinDirPath = Path.Combine(micsRootDir, "bin");

                // Check that mMicsBinDirPath is a valid directory path.
                if (!Directory.Exists(mMicsBinDirPath))
                {
                    Log2.e("\n\nSsutil.GetBinPath(): ERROR: invalid mMicsBinDirPath = " + mMicsBinDirPath);
                    Application.Exit(Error.MICSBINDIRNOTSET);
                }
            }

            // Construct the full path to the prescribed .exe file.
            string path = Path.Combine(mMicsBinDirPath, micsProgramName + ".exe");

            // Check that this file path exists.
            if (!File.Exists(path))
            {
                Log2.e("\n\nSsutil.GetBinPath(): ERROR: path to executable does not exist: " + path);
                Application.Exit(Error.MICSBINDIRNOTSET);
            }

            //...Log2.v("\nSsutil.GetBinPath(): returned: " + path);

            return path;
        }

        /// <summary>
        /// This method sets the value of the private string 'mMicsBinDirPath'; if this set value is
        /// not null, empty or all whitespace then it is the string value returned by a subsequent
        /// call to the method GetBinPath().
        /// </summary>
        /// <param name="binDirPath"></param>
        public static void SetMicsBinDirPath(string binDirPath)
        {
            if ((String.IsNullOrWhiteSpace(binDirPath) || !Directory.Exists(binDirPath)))
            {
                Log2.e("\n\nSsutil.SetMicsBinDirPath(): ERROR: method SetMicsBinDirPath(path) called with null, empty or whitespace path.");
                Application.Exit(Error.MICSBINDIRNOTSET);
            }

            mMicsBinDirPath = binDirPath;

            //...Log2.v("\nSsutil.SetMicsBinDirPath(): mMicsBinDirPath = " + mMicsBinDirPath);
        }

        /// <summary>
        /// This method return the user's email address, if there is one recorded 
        /// in the database.
        /// </summary>
        /// <param name="cOper"> - operator code.</param>
        /// <param name="cMicsId"> - user's MICS ID.</param>
        /// <param name="cEmailAddr"> - user's found email address.</param>
        /// <param name="nMaxLen"> - maximum length of an email address.</param>
        /// <param name="tsip_email"> - either "y" or "n"; yes means that the user wants to receive emails.</param>
        /// <param name="cDelMail"> - TBD.</param>
        /// <returns>0 if an email address was found; 1 if not found.</returns>
        public static int EmailAddr(string cOper,
                                    string cMicsId,
                                    out string cEmailAddr,
                                    int nMaxLen,
                                    out string tsip_email,
                                    out string cDelMail)
        {
            // 'out' requirements:
            cEmailAddr = "";
            tsip_email = "";
            cDelMail = "";

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLLEN nNull;

            int nRet = 0;
            string cSQL;

            if (cMicsId.Length == 0)
            {
                // If there is no mics id, use the alpha mailout.
                cEmailAddr = cOper;
                return 2;
            }

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSsutil.EmailAddr(): ERROR: call to ODBC.SQLAllocHandle() failed.");
                Application.Exit(666);
            }

            cSQL = String.Format("select email, tsip_email, auto_delete from adm.account_details where micsid='{0}'", cMicsId);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSsutil.EmailAddr(): ERROR: call to ODBC.SQLExecDirect() failed.");
                Application.Exit(666);
            }

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                // Can't find an email address for this MicsID.
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return 1;
                }
                else
                {
                    // Something bad happened.
                    Log2.e("\nSsutil.EmailAddr(): ERROR: call to ODBC.SQLFetch() failed.");
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    Application.Exit(666);
                }
            }

            // Try to get the fetched data.
            try
            {
                Ssutil.DbGetString(hStmt, 1, "email", out cEmailAddr, nMaxLen, out nNull);
                Ssutil.DbGetString(hStmt, 2, "tsip_email", out tsip_email, 2, out nNull);
                Ssutil.DbGetString(hStmt, 3, "auto_delete", out cDelMail, 2, out nNull);
                nRet = 0;
            }
            catch (Exception e)
            {
                Log2.e("\nSsutil.EmailAddr(): ERROR: call to Ssutil.DbGetString failed: " + e.Message);
                nRet = 3;
            }

            // Release ODBC resources.
            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (nRet);
        }

        /// <summary>
        /// This method gets an SQL-standard timestamp from a prescribed column in a table record 
        /// following a call to SQLFetch().
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle (same as used in SQLFetch()).</param>
        /// <param name="nColNum"> - column number in the table.</param>
        /// <param name="cName"> - name of the column.</param>
        /// <param name="pOutTime"> - the gotten timestamp object.</param>
        /// <param name="nullInd"> - ODBC null indicator for pOutTime.</param>
        /// <returns></returns>
        public static int DbGetTimestamp(SQLHANDLE hStmt,
                                            int nColNum,
                                            string cName,
                                            out ODBC.TIMESTAMP_STRUCT pOutTime,
                                            out SQLLEN nullInd)
        {
            // 'out' requirement.
            pOutTime = new ODBC.TIMESTAMP_STRUCT();
            nullInd = Constant.DB_NULL;

            SQLRETURN sqlRet;

            if (nColNum == 0)
            {
                nColNum = ++glbNextColumnForGet;
            }
            else
            {
                glbNextColumnForGet = (short)nColNum;
            }

            int targetValueSize = Marshal.SizeOf(pOutTime);
            IntPtr targetValuePtr = Marshal.AllocHGlobal(targetValueSize);

            IntPtr nullIndPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));

            sqlRet = ODBC.SQLGetData(hStmt, (SQLUSMALLINT)nColNum, ODBC.SQL_C_TYPE_TIMESTAMP, targetValuePtr, targetValueSize, nullIndPtr);
            if (!ODBC.IsOK(sqlRet))
            {
                throw new Exception("DbGetTimestamp");
            }
            else
            {
                pOutTime = new ODBC.TIMESTAMP_STRUCT();
                Marshal.PtrToStructure(targetValuePtr, pOutTime);

                nullInd = Marshal.ReadInt64(nullIndPtr);

                if (nullInd != Constant.DB_NULL)
                {
                    nullInd = Constant.DB_NOT_NULL;
                }
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method converts an SQL-standard timestamp object into
        /// a MICS-specific TM object.
        /// </summary>
        /// <param name="tInTS"> - SQL-standard timestamp.</param>
        /// <param name="tOutTS"> - TM object provided as output.</param>
        /// <returns></returns>
        public static int DbTimestampToTime(ODBC.TIMESTAMP_STRUCT tInTS, out TM tOutTS)
        {
            // 'out' requirement.
            tOutTS = null;

            if (tInTS == null)
            {
                return Constant.FAILURE;
            }

            tOutTS = new TM();

            tOutTS.tm_year = tInTS.year;
            tOutTS.tm_mon = tInTS.month;
            tOutTS.tm_mday = tInTS.day;
            tOutTS.tm_hour = tInTS.hour;
            tOutTS.tm_min = tInTS.minute;
            tOutTS.tm_sec = tInTS.second;
            tOutTS.tm_wday = -1;
            tOutTS.tm_yday = -1;
            tOutTS.tm_isdst = -1;

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method returns true if a table with a prescribed name
        /// and exists in the database; otherwise false.
        /// </summary>
        /// <param name="cTableName"> - table name, e.g. 'fcsa.web.dblogger'.</param>
        /// <param name="cSchema"> - database schema, e.g. 'fcsa.web'</param>
        /// <returns></returns>
        public static bool DbTableExists(string cTableName, out string cSchema)
        {
            // 'out' requirement.
            cSchema = "";

            string cTDB;
            string cTSchema;
            string cTName;

            string cWhere;
            int nCount;
            bool IsRet;

            if (String.IsNullOrWhiteSpace(cTableName))
            {
                return false;
            }

            DbParseTableName(cTableName, out cTDB, out cTSchema, out cTName);

            // Check whether a schema has been parsed out of the tableName;  
            // if not, use the current fcsa user's.
            if (String.IsNullOrWhiteSpace(cTSchema))
            {
                cTSchema = Info.GlobalSchema;
            }

            cWhere = String.Format("TABLE_NAME='{0}' and TABLE_SCHEMA='{1}'",
                                    cTName, cTSchema);

            nCount = Ssutil.DbCountRows("INFORMATION_SCHEMA.TABLES", cWhere);

            if (nCount > 0)
            {
                cSchema = cTSchema;
                IsRet = true;
            }
            else
            {
                cSchema = "";
                IsRet = false;
            }

            return IsRet;
        }

        /// <summary>
        /// This method decomposes a SQL Server tablename into its 
        /// separate component fields: [cTDB].[cTSchema].[cTName] ;
        /// an empty string is returned for any field that does not exist.
        /// </summary>
        /// <param name="cTableName"></param>
        /// <param name="cTDB"></param>
        /// <param name="cTSchema"></param>
        /// <param name="cTName"></param>
        public static void DbParseTableName(string cTableName, out string cTDB, out string cTSchema, out string cTName)
        {
            // 'out' requirements.
            cTDB = "";
            cTSchema = "";
            cTName = "";

            if (String.IsNullOrWhiteSpace(cTableName))
            {
                Log2.e("\nSsutil.DbParseTableName(): ERROR: DB tableName is null or whitespace.");
                return;
            }

            string[] tokens = cTableName.Split('.');

            if (tokens.Length == 1)
            {
                cTName = cTableName.Trim();
            }
            else if (tokens.Length == 2)
            {
                cTSchema = tokens[0].Trim();
                cTName = tokens[1].Trim();
            }
            else if (tokens.Length == 3)
            {
                cTDB = tokens[0].Trim();
                cTSchema = tokens[1].Trim();
                cTName = tokens[2].Trim();
            }
            else if (tokens.Length >= 4)
            {
                Log2.e("\nSsutil.DbParseTableName(): ERROR: DB tableName has more than 3 'dots': " + cTableName);
            }

            return;
        }

        /// <summary>
        /// This method directs the SQL Server to 'commit' all of the pending
        /// query transactions that have been accumulated; note that a 'commit'
        /// directive is specific to an ODBC connection handle.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <returns></returns>
        public static int DbCommit(SQLHANDLE hConn)
        {
            SQLRETURN sqlRet;

            sqlRet = ODBC.SQLEndTran(ODBC.SQL_HANDLE_DBC, hConn, ODBC.SQL_COMMIT);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSsutil.DbCommit(): ERROR: call to ODBC.SQLEndTran() failed, sqlRet = " + sqlRet);
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                GenUtil.SetErr("dbCommit02: Could not commit:-\n{0}", GenUtil.GetUserMess());
                return -2;
            }

            //...Log2.v("\nSsutil.DbCommit(): SQL_COMMIT");
            return Constant.SUCCESS;
        }


        /// <summary>
        /// This method directs the SQL Server to 'rollback' all of the pending
        /// query transactions that have been accumulated; note that a 'rollback'
        /// directive is specific to an ODBC connection handle.
        /// </summary>
        /// <param name="hConn"> - an open ODBC connection handle.</param>
        /// <returns></returns>
        public static int DbRollBack(SQLHANDLE hConn)
        {
            SQLRETURN sqlRet;

            sqlRet = ODBC.SQLEndTran(ODBC.SQL_HANDLE_DBC, hConn, ODBC.SQL_ROLLBACK);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nSsutil.DbRollBack(): ERROR: call to ODBC.SQLEndTran() failed, sqlRet = " + sqlRet);
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_DBC, hConn);
                GenUtil.SetErr("dbRollBack02: Could not rollback current transaction:-\n{0}", GenUtil.GetUserMess());
                return -2;
            }

            //...Log2.v("\nSsutil.DbRollBack(): SQL_ROLLBACK"); 
            return Constant.SUCCESS;
        }


        /// <summary>
        /// This method sets the ODBC/SQL 'commit' mode on the prescribed connection to either
        /// automatic or manual; in automatic mode each transaction is an individual SQL query that is
        /// immediately 'committed'; in manual mode the current transaction is the sequential accumulation
        /// of all queries submitted since the last 'commit'.
        /// </summary>
        /// <param name="hConn"> - the ODBC connection.</param>
        /// <param name="mode"> - one of Enums.COMMIT.AUTO, Enums.COMMIT.MANUAL</param>
        /// <returns></returns>
        public static int SetCommitMode(SQLHDBC hConn, Enums.COMMIT mode)
        {
            int retVal = Constant.SUCCESS;
            SQLRETURN sqlRet;

            switch (mode)
            {
                case Enums.COMMIT.AUTO:
                    // SQL_AUTOCOMMIT_ON: The driver uses autocommit mode in which each statement is 
                    // committed immediately after it is executed. This is the default. 
                    // Any open transactions on the connection are committed when SQL_ATTR_AUTOCOMMIT 
                    // is set to SQL_AUTOCOMMIT_ON to change from manual-commit mode to autocommit mode.

                    sqlRet = ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_ATTR_AUTOCOMMIT, (SQLPOINTER)ODBC.SQL_AUTOCOMMIT_ON, ODBC.SQL_IS_UINTEGER);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        //	Could not set autocommit off.
                        Log2.e("\nSsutil.CommitMode(): ERROR: could not set the database connection to AUTO COMMIT.");
                        retVal = Constant.FAILURE;
                    }
                    //...Log2.v("\nSsutil.CommitMode(): SQL_AUTOCOMMIT_ON");
                    break;

                case Enums.COMMIT.MANUAL:

                    // SQL_AUTOCOMMIT_OFF: The driver uses manual-commit mode, and the application must 
                    // explicitly commit or roll back transactions using SQLEndTran.

                    sqlRet = ODBC.SQLSetConnectAttr(hConn, ODBC.SQL_ATTR_AUTOCOMMIT, (SQLPOINTER)ODBC.SQL_AUTOCOMMIT_OFF, ODBC.SQL_IS_UINTEGER);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        //	Could not set autocommit off.
                        Log2.e("\nSsutil.CommitMode(): ERROR: could not set the database connection to MANUAL COMMIT.");
                        retVal = Constant.FAILURE;
                    }
                    //...Log2.v("\nSsutil.CommitMode(): SQL_AUTOCOMMIT_OFF");
                    break;

                default:
                    Log2.e("\nSsutil.CommitMode(): ERROR: unknown commit mode: " + mode);
                    retVal = -666;
                    break;
            }

            Ssutil.DisConn(hConn);

            return retVal;
        }


        /// <summary>
        /// This method returns the following parameters for the current ODBC / SQL Server session: SQL Serverver version,
        /// SQL Server name, SQL user name, SQL schema name, database name, and SQL authorization mode.
        /// </summary>
        /// <param name="sqlServerVersion"></param>
        /// <param name="sqlUserName"></param>
        /// <param name="sqlSchemaName"></param>
        /// <param name="sqlDbName"></param>
        /// <param name="sqlServerName"></param>
        /// <param name="sqlAuthMode"></param>
        /// <returns></returns>
        public static int GetSQLsessionParams(out string sqlServerVersion, out string sqlUserName, out string sqlSchemaName, out string sqlDbName, out string sqlServerName, out string sqlAuthMode)
        {
            // 'out'.
            sqlServerVersion = "";
            sqlUserName = "";
            sqlSchemaName = "";
            sqlDbName = "";
            sqlServerName = "";
            sqlAuthMode = "";

            // Connect to the DB and get a statement handle.
            SQLHDBC hConn = Ssutil.NewConn();

            // We will need these.
            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            // This is the SQL query we need to run.
            string query = "SELECT @@VERSION, USER_NAME(), SCHEMA_NAME(), DB_NAME(), @@SERVERNAME, CASE SERVERPROPERTY('IsIntegratedSecurityOnly') WHEN 1 THEN 'Windows Authentication' WHEN 0 THEN 'Windows and SQL Server Authentication' END ";

            // Assume a reasonable upper-limit for the length of the returned strings.
            const int STRING_SIZE = 128;

            // Declare the names of the pointers and allocate unmanaged memory to them.
            SQLPOINTER ptrToVersion = Marshal.AllocHGlobal(STRING_SIZE);
            SQLPOINTER ptrToUserName = Marshal.AllocHGlobal(STRING_SIZE);
            SQLPOINTER ptrToSchemaName = Marshal.AllocHGlobal(STRING_SIZE);
            SQLPOINTER ptrToDbName = Marshal.AllocHGlobal(STRING_SIZE);
            SQLPOINTER ptrToServerName = Marshal.AllocHGlobal(STRING_SIZE);
            SQLPOINTER ptrToAuthMode = Marshal.AllocHGlobal(STRING_SIZE);

            // The SQL query we are attempting to run returns the values of standard
            // built-in functions that should never return NULL.
            // Consequently, we dispense with the complexity of inspecting of nullInd 
            // returned values and only assign one nullInd pointer and re-use it for 
            // every column fetched.
            SQLPOINTER mNullIndPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));

            // Get an ODBC statement handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Bind the pointers to the columns of the cursor/result data set.
            Bind.BindBufferToString(hStmt, 1, ptrToVersion, STRING_SIZE, mNullIndPtr);
            Bind.BindBufferToString(hStmt, 2, ptrToUserName, STRING_SIZE, mNullIndPtr);
            Bind.BindBufferToString(hStmt, 3, ptrToSchemaName, STRING_SIZE, mNullIndPtr);
            Bind.BindBufferToString(hStmt, 4, ptrToDbName, STRING_SIZE, mNullIndPtr);
            Bind.BindBufferToString(hStmt, 5, ptrToServerName, STRING_SIZE, mNullIndPtr);
            Bind.BindBufferToString(hStmt, 6, ptrToAuthMode, STRING_SIZE, mNullIndPtr);

            // Attempt the SQL query.
            try
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, query, (SQLINTEGER)query.Length);
            }
            catch (Exception)
            {
                Log2.e("\r\nGetSQLsessionParams():  ERROR: SQLExecDirect(): Exception: query = " + query);
                return Constant.FAILURE;
            }

            // No exception was thrown but the SQL query may still have failed.
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nGetSQLsessionParams(): ERROR: SQLExecDirect(): sqlRet = " + sqlRet);
                return Constant.FAILURE;
            }

            // Fetch the cursor data set and extract the results.
            sqlRet = ODBC.SQLFetch(hStmt);

            // Check if the fetch was successful.
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nGetSQLsessionParams()(): ERROR: call to SQLFetch() failed, sqlRet =  " + sqlRet);
                return Error.ODBC_FETCH_FAILED;
            }

            // Extract the results of the query from the pointers.
            sqlServerVersion = Marshal.PtrToStringAnsi(ptrToVersion).Trim();
            sqlUserName = Marshal.PtrToStringAnsi(ptrToUserName).Trim();
            sqlSchemaName = Marshal.PtrToStringAnsi(ptrToSchemaName).Trim();
            sqlDbName = Marshal.PtrToStringAnsi(ptrToDbName).Trim();
            sqlServerName = Marshal.PtrToStringAnsi(ptrToServerName).Trim();
            sqlAuthMode = Marshal.PtrToStringAnsi(ptrToAuthMode).Trim();

            // Extract all we need from the SQL Server version string.
            int posCR = sqlServerVersion.IndexOf((char)13); // Find the position of the CR character (if any).
            int posLF = sqlServerVersion.IndexOf((char)10); // Find the position of the LF character.
            int pos = (posCR >= 0) ? posCR : posLF;
            if (pos > 0) sqlServerVersion = sqlServerVersion.Substring(0, pos);

            // Clean up - release resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            // If we get here everything should have worked.
            return Constant.SUCCESS;
        }



        /// <summary>
        /// This method returns the SQL schema name associated with a MICS user name; in a 
        /// pefect world this would match the ultrixid entry for the user in the database table
        /// [adm]:[account_ids].
        /// </summary>
        /// <param name="micsUserName"></param>
        /// <returns></returns>
        public static string GetFCSASchema_A(string micsUserName)
        {
            //...Log2.v("\n\nSsutil.GetFCSASchema()_A: Entry");

            string schema = "";

            // Declare required ODBC handles.
            SQLHANDLE hStmt;
            SQLHDBC hConn;

            // Establish a new connection with the database.
            hConn = NewConn();

            // Get an ODBC statement handle to be used to execute the query and fetch the results.
            SQLRETURN sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nSsutil.GetFCSASchema()_A: ERROR: SQLExecDirect(): sqlRet = " + sqlRet);
                return schema;
            }

            // We are going to retrieve the fetched data using 'bound columns'.
            // Declare and allocate memory to pointers to unmanaged memory (aka the heap);
            // We need a pointer to the fetched result and a pointer to the nullInd.
            SQLPOINTER ptrToResult = Marshal.AllocHGlobal(Constant.GLB_SCHEMA_SZ);
            SQLPOINTER ptrToNullInd = Marshal.AllocHGlobal(sizeof(SQLLEN));

            // Bind the result and nullInd pointers to column 1 of the fetched data set.
            Bind.BindBufferToString(hStmt, 1, ptrToResult, Constant.GLB_SCHEMA_SZ, ptrToNullInd);

            // Here is the specific query that we want to perform.
            string query = String.Format("SELECT dbo.user_schema2022('{0}');", micsUserName);
            //...Log2.v("\nSsutil.GetFCSASchema_A(): query = " + query);

            // Attempt the SQL query and fetch the result set.
            try
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, query, (SQLINTEGER)query.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.GetFCSASchema()_A: ERROR: SQLExecDirect(): sqlRet = " + sqlRet);
                    Log2.e("\r\nSsutil.GetFCSASchema()_A: ERROR: SQLExecDirect(): query  = " + query);
                    Log2.e("\n" + ODBC.GetDiagnostics(hStmt, query));
                    return schema;
                }

                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\r\nSsutil.GetFCSASchema()_A: ERROR: SQLFetch(): sqlRet = " + sqlRet);
                    Log2.e("\r\nSsutil.GetFCSASchema()_A: ERROR: SQLFetch(): query  = " + query);
                    return schema;
                }
            }
            catch (Exception)
            {
                Log2.e("\r\nSsutil.GetFCSASchema()_A:  ERROR: SQLExecDirect(): Exception: Caught");
                Environment.Exit(666);
            }


            // Retrieve the result set from unmanaged memory.
            SQLLEN nullInd = Marshal.ReadInt64(ptrToNullInd);
            if (nullInd == Constant.DB_NULL)
            {
                Log2.e("\r\nSsutil.GetFCSASchema()_A: ERROR: Marshal.ReadInt64() returned Constant.DB_NULL");
                return schema;
            }

            // We can now read the result from unmananged memory.
            schema = Marshal.PtrToStringAnsi(ptrToResult);
            schema = schema.Trim();

            //...Log2.v("\n\nSsutil.GetFCSASchema()_A: ZULU succeeded: schema  = " + schema);
            //...Log2.v("\n\nSsutil.GetFCSASchema()_A: ZULU succeeded: nullInd = " + nullInd);

            // Release resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            DisConn(hConn);

            //...Log2.v("\n\nSsutil.GetFCSASchema()_A: succeeded: schema = " + schema);
            return schema;
        }



#if false
        /// <summary>
        /// This method sends an email, comprising body text (read from a prescribed text file) and a path
        /// to a file to be attached to the email; it is only called from the MICS# program PFDcont.exe
        /// </summary>
        /// <param name="emailAddress"> - prescribed email address to be sent to.</param>
        /// <param name="subject"> - prescribed subject line of email.</param>
        /// <param name="pathToBodyFile"> - path to a file containing the email's body text.</param>
        /// <param name="pathToAttachedFile"> - path to a file to be attached to the email.</param>
        /// <returns></returns>
        public static int SendEmail(string emailAddress,        /*	Email address */
                                    string subject,             /*	Subject Line 	*/
                                    string pathToBodyFile,      /*	Body */
                                    string pathToAttachedFile)  /*	Attached file */
        {
            //...Log2.v("\nSsutil.SendEmail(): Entry");
            //...Log2.v("\nemailAddress       = " + emailAddress);
            //...Log2.v("\nsubject            = " + subject);
            //...Log2.v("\npathToBodyFile     = " + pathToBodyFile);
            //...Log2.v("\npathToAttachedFile = " + pathToAttachedFile);

            Log2.e("\n" + Environment.StackTrace);

            int nRet = Constant.SUCCESS;
            string errMsg;
            string bodyText = "See Attachement ...";

            if (!pathToBodyFile.Equals("--"))
            {
                try
                {
                    bodyText = File.ReadAllText(pathToBodyFile);
                }
                catch (Exception e)
                {
                    errMsg = "ERROR: call to File.ReadAllText() threw an exception for pathToBodyFile = " + pathToBodyFile;
                    Log2.e("\n\nSsutil.SendEmail(): ERROR: " + e.Message);
                    Log2.e("\n" + e.StackTrace);
                    return Constant.FAILURE;
                }
            }

            nRet = MicsEmail.Send(emailAddress, subject, bodyText, pathToAttachedFile, out errMsg);

            if (nRet != Constant.SUCCESS)
            {
                Log2.e("\n\nSsutil.SendEmail(): ERROR: call to MicsEmail.Send() FAILED, retVal = " + nRet);
                TsipQ.WriteToTsipLog("\nSsutil.SendEmail(): ERROR: call to MicsEmail.Send() FAILED, retVal = " + nRet);
            }

            //...Log2.v("\nSsutil.SendEmail(): Exit, return code = " + nRet);
            return (nRet);
        }
#endif





    } //class
} //namespace
