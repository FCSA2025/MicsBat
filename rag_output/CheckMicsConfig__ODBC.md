# Documented File: ODBC.cs
**Repository Path:** `CheckMicsConfig\ODBC.cs`
**Primary Layer:** `CheckMicsConfig`
**Namespace:** `CheckMicsConfig`

## Source Code Representation
```csharp
﻿using System;
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
    //using _Configuration;
    /// <summary>
    /// Provides methods that allow C# to call ODBC 'native code' subroutines for accessing
    /// the Microsoft SQL Server.
    /// </summary>
    /// <remarks>
    /// \image html ODBC.png ""
    /// Open Database Connectivity (ODBC) is a standard application programming interface (API) 
    /// for accessing database management systems (DBMS). The designers of ODBC aimed to make it
    /// independent of database systems and operating systems. An application written using ODBC
    /// can be ported to other platforms, both on the client and server side, with few changes 
    /// to the data access code.
    /// 
    /// ODBC accomplishes DBMS independence by using an ODBC driver as a translation layer 
    /// between the application and the DBMS.The application uses ODBC functions through an 
    /// ODBC driver manager with which it is linked, and the driver passes the query to the DBMS.
    /// 
    /// An ODBC driver can be thought of as analogous to a printer driver or other driver, 
    /// providing a standard set of functions for the application to use, and implementing 
    /// DBMS-specific functionality.An application that can use ODBC is referred to as 
    /// "ODBC-compliant". Any ODBC-compliant application can access any DBMS for which a driver 
    /// is installed. 
    /// 
    /// Microsoft SQL Server supports ODBC, via the <b>SQL Server Native Client</b> ODBC driver. 
    /// 
    /// ODBC drivers exist for all major DBMSs, many other data sources like address book 
    /// systems and Microsoft Excel, and even for text or comma-separated values (CSV) files.
    /// 
    /// ODBC was originally developed by Microsoft and Simba Technologies during the early 
    /// 1990s, and became the basis for the Call Level Interface(CLI) standardized by 
    /// SQL Access Group in the Unix and mainframe field. ODBC retained several features 
    /// that were removed as part of the CLI effort. Full ODBC was later ported back to those 
    /// platforms, and became a de facto standard considerably better known than CLI.The 
    /// CLI remains similar to ODBC, and applications can be ported from one platform to the 
    /// other with few changes.
    /// </remarks>
    public class ODBC
    {
        public const string ODBC_DLL_PATHNAME = @"C:\Windows\System32\odbc32.dll";

        public const SQLSMALLINT SQL_HANDLE_ENV = 1;
        public const SQLSMALLINT SQL_HANDLE_DBC = 2;
        public const SQLSMALLINT SQL_HANDLE_STMT = 3;
        public const SQLSMALLINT SQL_HANDLE_DESC = 4;
        //-----------------------------------------------------------------------------------
        public const SQLRETURN SQL_INVALID_HANDLE = -2;
        public const SQLRETURN SQL_ERROR = -1;
        public const SQLRETURN SQL_SUCCESS = 0;
        public const SQLRETURN SQL_SUCCESS_WITH_INFO = 1;
        public const SQLRETURN SQL_STILL_EXECUTING = 2;
        public const SQLRETURN SQL_NEED_DATA = 99;
        public const SQLRETURN SQL_NO_DATA = 100;
        public const SQLRETURN SQL_PARAM_DATA_AVAILABLE = 101;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_CONCUR_VALUES = 4;
        public const SQLINTEGER SQL_CONCURRENCY = 7;
        public const SQLINTEGER SQL_CURSOR_TYPE = 6;
        public const SQLULEN SQL_CURSOR_DYNAMIC = 2;
        public const SQLINTEGER SQL_NULL_DATA = -1;
        public const SQLINTEGER SQL_NTS = -3;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_CLOSE = 0;
        public const SQLINTEGER SQL_DROP = 1;
        public const SQLINTEGER SSQL_UNBIND = 2;
        public const SQLINTEGER SQL_RESET_PARAMS = 3;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_ACCESS_MODE = 101;
        public const SQLINTEGER SQL_AUTOCOMMIT = 102;
        public const SQLINTEGER SQL_LOGIN_TIMEOUT = 103;
        public const SQLINTEGER SQL_OPT_TRACE = 104;
        public const SQLINTEGER SQL_OPT_TRACEFILE = 105;
        public const SQLINTEGER SQL_TRANSLATE_DLL = 106;
        public const SQLINTEGER SQL_TRANSLATE_OPTION = 107;
        public const SQLINTEGER SQL_TXN_ISOLATION = 108;
        public const SQLINTEGER SQL_CURRENT_QUALIFIER = 109;
        public const SQLINTEGER SQL_ODBC_CURSORS = 110;
        public const SQLINTEGER SQL_QUIET_MODE = 111;
        public const SQLINTEGER SQL_PACKET_SIZE = 112;
        //-----------------------------------------------------------------------------------
        public const SQLLEN SQL_TXN_READ_UNCOMMITTED = 0x00000001;
        //-----------------------------------------------------------------------------------
        public const SQLSMALLINT SQL_PARAM_TYPE_UNKNOWN = 0;
        public const SQLSMALLINT SQL_PARAM_INPUT = 1;
        public const SQLSMALLINT SQL_PARAM_INPUT_OUTPUT = 2;
        public const SQLSMALLINT SQL_RESULT_COL = 3;
        public const SQLSMALLINT SQL_PARAM_OUTPUT = 4;
        public const SQLSMALLINT SQL_RETURN_VALUE = 5;
        public const SQLSMALLINT SQL_PARAM_INPUT_OUTPUT_STREAM = 8;
        public const SQLSMALLINT SQL_PARAM_OUTPUT_STREAM = 16;
        //-----------------------------------------------------------------------------------
        public const SQLSMALLINT SQL_UNKNOWN_TYPE = 0;
        public const SQLSMALLINT SQL_CHAR = 1;
        public const SQLSMALLINT SQL_NUMERIC = 2;
        public const SQLSMALLINT SQL_DECIMAL = 3;
        public const SQLSMALLINT SQL_INTEGER = 4;
        public const SQLSMALLINT SQL_SMALLINT = 5;
        public const SQLSMALLINT SQL_FLOAT = 6;
        public const SQLSMALLINT SQL_REAL = 7;
        public const SQLSMALLINT SQL_DOUBLE = 8;
        public const SQLSMALLINT SQL_DATETIME = 9;
        public const SQLSMALLINT SQL_VARCHAR = 12;
        public const SQLSMALLINT SQL_BIGINT = -5;
        public const SQLSMALLINT SQL_TINYINT = -6;
        public const SQLSMALLINT SQL_BIT = -7;
        //-----------------------------------------------------------------------------------
        public const SQLSMALLINT SQL_SIGNED_OFFSET = -20;
        public const SQLSMALLINT SQL_UNSIGNED_OFFSET = -22;
        public const SQLSMALLINT SQL_C_DEFAULT = 99;
        public const SQLSMALLINT SQL_C_CHAR = SQL_CHAR;
        public const SQLSMALLINT SQL_C_UTINYINT = SQL_TINYINT + SQL_UNSIGNED_OFFSET;
        public const SQLSMALLINT SQL_C_SHORT = SQL_SMALLINT;
        public const SQLSMALLINT SQL_C_SSHORT = SQL_C_SHORT + SQL_SIGNED_OFFSET;
        public const SQLSMALLINT SQL_C_SBIGINT = SQL_BIGINT + SQL_SIGNED_OFFSET;
        public const SQLSMALLINT SQL_C_LONG = SQL_INTEGER;
        public const SQLSMALLINT SQL_C_SLONG = SQL_C_LONG + SQL_SIGNED_OFFSET;
        public const SQLSMALLINT SQL_C_FLOAT = SQL_REAL;
        public const SQLSMALLINT SQL_C_DOUBLE = SQL_DOUBLE;
        public const SQLSMALLINT SQL_C_BIT = SQL_BIT;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_ATTR_CONCURRENCY = SQL_CONCURRENCY;
        public const SQLINTEGER SQL_ATTR_CONNECTION_POOLING = 201;
        public const SQLINTEGER SQL_ATTR_CURSOR_TYPE = SQL_CURSOR_TYPE;
        public const SQLINTEGER SQL_ATTR_ODBC_VERSION = 200;
        public const SQLINTEGER SQL_ATTR_AUTOCOMMIT = SQL_AUTOCOMMIT;
        //-----------------------------------------------------------------------------------
        public const SQLSMALLINT SQL_COMMIT = 0;
        public const SQLSMALLINT SQL_ROLLBACK = 1;
        //-----------------------------------------------------------------------------------
        public const SQLUINTEGER SQL_AUTOCOMMIT_OFF = 0U;
        public const SQLUINTEGER SQL_AUTOCOMMIT_ON = 1U;
        public const SQLUINTEGER SQL_AUTOCOMMIT_DEFAULT = SQL_AUTOCOMMIT_ON;
        //-----------------------------------------------------------------------------------
        public const SQLULEN SQL_NULL_HANDLE = 0L;
        public const SQLULEN SQL_CP_ONE_PER_HENV = 2UL;
        public const SQLULEN SQL_OV_ODBC3 = 3UL;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_IS_POINTER = -4;
        public const SQLSMALLINT SQL_IS_UINTEGER = -5;
        public const SQLSMALLINT SQL_IS_INTEGER = -6;
        public const SQLSMALLINT SQL_IS_USMALLINT = -7;
        public const SQLSMALLINT SQL_IS_SMALLINT = -8;
        //-----------------------------------------------------------------------------------
        public const SQLSMALLINT SQL_DIAG_RETURNCODE = 1;
        public const SQLSMALLINT SQL_DIAG_NUMBER = 2;
        public const SQLSMALLINT SQL_DIAG_SQLSTATE = 4;
        public const SQLSMALLINT SQL_DIAG_MESSAGE_TEXT = 6;
        //-----------------------------------------------------------------------------------
        public const SQLLEN SQL_DIAG_CURSOR_ROW_COUNT = -1249;
        public const SQLLEN SQL_DIAG_ROW_NUMBER = -1248;
        public const SQLINTEGER SQL_DIAG_COLUMN_NUMBER = -1247;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_NO_COLUMN_NUMBER = -1;
        public const SQLINTEGER SQL_COLUMN_NUMBER_UNKNOWN = -2;
        public const SQLINTEGER SQL_NO_ROW_NUMBER = -1;
        public const SQLINTEGER SQL_ROW_NUMBER_UNKNOWN = -2;
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_ATTR_TRACE = SQL_OPT_TRACE;
        public const SQLINTEGER SQL_ATTR_TRACEFILE = SQL_OPT_TRACEFILE;
        public const SQLULEN SQL_OPT_TRACE_OFF = 0UL;
        public const SQLULEN SQL_OPT_TRACE_ON = 1UL;
        //-----------------------------------------------------------------------------------
        // Multiple Active Result Sets (MARS) enables multiple statements on a single connection.
        //-----------------------------------------------------------------------------------
        public const SQLINTEGER SQL_COPT_SS_BASE = 1200;
        public const SQLINTEGER SQL_COPT_SS_MARS_ENABLED = (SQL_COPT_SS_BASE + 24);
        public const SQLINTEGER SQL_MARS_ENABLED_YES = 1;
        //-----------------------------------------------------------------------------------
        public const SQLSMALLINT SQL_TYPE_TIMESTAMP = 93;
        public const SQLSMALLINT SQL_C_TYPE_TIMESTAMP = SQL_TYPE_TIMESTAMP;

        /// <summary>
        /// This subsidiary class encapsulates the individual field values that correspond
        /// to the SQL Server datatype <b>datetime</b>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class TIMESTAMP_STRUCT
        {
            public SQLSMALLINT year;
            public SQLUSMALLINT month;
            public SQLUSMALLINT day;
            public SQLUSMALLINT hour;
            public SQLUSMALLINT minute;
            public SQLUSMALLINT second;
            public SQLUINTEGER fraction;  // nanoseconds


            //----------------------------------------------------------------------------

            /// <summary>
            /// The default public constructor for objects of this class.
            /// </summary>
            public TIMESTAMP_STRUCT()
            {
                year = 1900;
                month = 1;
                day = 1;
                hour = 0;
                minute = 0;
                second = 0;
                fraction = 0;
            }

            /// <summary>
            /// This constructor instantiates a new object whose member values
            /// are set i.a.w. a prescribed SQL Server <b>datetime</b> object.
            /// </summary>
            /// <param name="dateTime"></param>
            public TIMESTAMP_STRUCT(DateTime dateTime)
            {
                year = (SQLSMALLINT)dateTime.Year;
                month = (SQLUSMALLINT)dateTime.Month;
                day = (SQLUSMALLINT)dateTime.Day;
                hour = (SQLUSMALLINT)dateTime.Hour;
                minute = (SQLUSMALLINT)dateTime.Minute;
                second = (SQLUSMALLINT)dateTime.Second;
                fraction = (SQLUINTEGER)dateTime.Millisecond * 1000000;  // nanoseconds
            }

            /// <summary>
            /// This method sets the member values of 'this' object i.a.w. the
            /// current system date and time using .NET's DateTime.Now() method.
            /// </summary>
            public void SetDateTimeNow()
            {
                DateTime dateTime = DateTime.Now;

                year = (SQLSMALLINT)dateTime.Year;
                month = (SQLUSMALLINT)dateTime.Month;
                day = (SQLUSMALLINT)dateTime.Day;
                hour = (SQLUSMALLINT)dateTime.Hour;
                minute = (SQLUSMALLINT)dateTime.Minute;
                second = (SQLUSMALLINT)dateTime.Second;
                fraction = (SQLUINTEGER)dateTime.Millisecond * 1000000;  // nanoseconds
            }

            /// <summary>
            /// This method returns an annotated, formatted, multi-line string that
            /// provides the current values of the internal field values of this
            /// TIMESTAMP_STRUCT object.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                // Example:   2019-05-01 11:07:51.510

                // The SQL Server measures 'fraction' in nanoseconds, so we need to convert.
                int milliseconds = (int)fraction / 1000000;

                return String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3}",
                                        year, month, day, hour, minute, second, milliseconds);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public DateTime ToDateTime()
            {
                int milliseconds = (int)fraction / 1000000;

                DateTime dateTime = new DateTime(year, month, day, hour, minute, second, milliseconds);

                return dateTime;
            }





        }
        //-----------------------------------------------------------------------------------



        /*
        SQLRETURN SQLAllocHandle(   SQLSMALLINT   HandleType,  
                                    SQLHANDLE     InputHandle,  
                                    SQLHANDLE *   OutputHandlePtr); 
        */
        /// <summary>
        /// Allocates an environment, connection, statement, or descriptor handle.
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlallochandle-function
        /// </summary>
        /// <param name="HandleType"></param>
        /// <param name="InputHandle"></param>
        /// <param name="OutputHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLAllocHandle([In] SQLSMALLINT HandleType,
                                            [In] SQLHANDLE InputHandle,
                                            [Out] out SQLHANDLE OutputHandle);

        //-------------------------------------------------------------------------------------
        /*
        SQLRETURN SQLBindCol(   SQLHSTMT        StatementHandle,
                                SQLUSMALLINT    ColumnNumber,
                                SQLSMALLINT     TargetType,
                                SQLPOINTER      TargetValuePtr,
                                SQLLEN          BufferLength,
                                SQLLEN*         StrLen_or_Ind);
        */

        /// <summary>
        /// Binds application data buffers to columns in a result set.
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="ColumnNumber"></param>
        /// <param name="TargetType"></param>
        /// <param name="TargetValuePtr"></param>
        /// <param name="BufferLength"></param>
        /// <param name="StrLen_or_Ind"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLBindCol(
                                                    [In] SQLHSTMT StatementHandle,
                                                    [In] SQLUSMALLINT ColumnNumber,
                                                    [In] SQLSMALLINT TargetType,
                                                    [In] SQLPOINTER TargetValuePtr, /* Deferred Buffer */
                                                    [In] SQLLEN BufferLength,
                                                    [In] SQLLENPTR StrLen_or_Ind);  /* Deferred Buffer */

        //-------------------------------------------------------------------------------------
        /*
        SQLRETURN SQLBindParameter( SQLHSTMT        StatementHandle,  
                                    SQLUSMALLINT    ParameterNumber,  
                                    SQLSMALLINT     InputOutputType,  
                                    SQLSMALLINT     ValueType,  
                                    SQLSMALLINT     ParameterType,  
                                    SQLULEN         ColumnSize,  
                                    SQLSMALLINT     DecimalDigits,  
                                    SQLPOINTER      ParameterValuePtr,  
                                    SQLLEN          BufferLength,  
                                    SQLLEN *        StrLen_or_IndPtr);
        */

        /// <summary>
        /// Binds a buffer to a parameter marker in an SQL statement.
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlbindparameter-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="ParameterNumber"></param>
        /// <param name="InputOutputType"></param>
        /// <param name="ValueType"></param>
        /// <param name="ParameterType"></param>
        /// <param name="ColumnSize"></param>
        /// <param name="DecimalDigits"></param>
        /// <param name="ParameterValuePtr"></param>
        /// <param name="BufferLength"></param>
        /// <param name="StrLen_or_IndPtr"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLBindParameter(
                                                [In] SQLHSTMT StatementHandle,
                                                [In] SQLUSMALLINT ParameterNumber,
                                                [In] SQLSMALLINT InputOutputType,
                                                [In] SQLSMALLINT ValueType,
                                                [In] SQLSMALLINT ParameterType,
                                                [In] SQLULEN ColumnSize,
                                                [In] SQLSMALLINT DecimalDigits,
                                                [In] SQLPOINTER ParameterValuePtr,  //Deferred buffer
                                                [In, Out] SQLLEN BufferLength,
                                                [In] SQLLENPTR StrLen_or_IndPtr);  //Deferred buffer.

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLCloseCursor( SQLHSTMT     StatementHandle);
        */

        /// <summary>
        /// Closes a cursor that has been opened on a statement and discards pending results.
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlclosecursor-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLCloseCursor(IntPtr StatementHandle);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLConnect(   SQLHDBC        ConnectionHandle,  
                                SQLCHAR *      ServerName,  
                                SQLSMALLINT    NameLength1,  
                                SQLCHAR *      UserName,  
                                SQLSMALLINT    NameLength2,  
                                SQLCHAR *      Authentication,  
                                SQLSMALLINT    NameLength3);
        */

        /// <summary>
        /// Establishes connections to a driver and a data source. The connection handle 
        /// references storage of all information about the connection to the data source, 
        /// including status, transaction state, and error information.
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlconnect-function
        /// </summary>
        /// <param name="ConnectionHandle"></param>
        /// <param name="ServerName"></param>
        /// <param name="NameLength1"></param>
        /// <param name="UserName"></param>
        /// <param name="NameLength2"></param>
        /// <param name="Authentication"></param>
        /// <param name="NameLength3"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLConnect([In] SQLHDBC ConnectionHandle,
                                        [In] SQLCHARPTR ServerName,
                                        [In] SQLSMALLINT NameLength1,
                                        [In] SQLCHARPTR UserName,
                                        [In] SQLSMALLINT NameLength2,
                                        [In] SQLCHARPTR Authentication,
                                        [In] SQLSMALLINT NameLength3);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLDisconnect(  SQLHDBC     ConnectionHandle );
        */

        /// <summary>
        /// Closes the connection associated with a specific connection handle. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqldisconnect-function
        /// </summary>
        /// <param name="ConnectionHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLDisconnect([In] IntPtr ConnectionHandle);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLEndTran(   SQLSMALLINT   HandleType,  
                                SQLHANDLE     Handle,  
                                SQLSMALLINT   CompletionType);
        */

        /// <summary>
        /// Requests a commit or rollback operation for all active operations on all 
        /// statements associated with a connection. SQLEndTran can also request that 
        /// a commit or rollback operation be performed for all connections associated 
        /// with an environment. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlendtran-function
        /// </summary>
        /// <param name="HandleType"></param>
        /// <param name="Handle"></param>
        /// <param name="CompletionType"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLEndTran(short HandleType,
                                        IntPtr Handle,
                                        short CompletionType);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLExecDirect(    SQLHSTMT     StatementHandle,  
                                    SQLCHAR *    StatementText,  
                                    SQLINTEGER   TextLength);
        */

        /// <summary>
        /// Executes a preparable statement, using the current values of the parameter marker 
        /// variables if any parameters exist in the statement. SQLExecDirect is the fastest 
        /// way to submit an SQL statement for one-time execution. 
        /// See: https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlexecdirect-function 
        /// </summary>
        /// <param name="statementHandle"></param>
        /// <param name="statementText"></param>
        /// <param name="textLength"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLExecDirect([In] SQLHSTMT statementHandle,
                                          [In] SQLCHARPTR statementText,
                                          [In] SQLINTEGER textLength);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLExecute( SQLHSTMT     StatementHandle);
        */

        /// <summary>
        /// Executes a prepared statement, using the current values of the parameter marker 
        /// variables if any parameter markers exist in the statement. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlexecute-function
        /// </summary>
        /// <param name="statementHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLExecute([In] IntPtr statementHandle);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLFetch( SQLHSTMT     StatementHandle);
        */

        /// <summary>
        /// Fetches the next rowset of data from the result set and returns data for all 
        /// bound columns. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlfetch-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLFetch([In] SQLHSTMT StatementHandle);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLFreeHandle(    SQLSMALLINT   HandleType,  
                                    SQLHANDLE     Handle);
        */

        /// <summary>
        /// Frees resources associated with a specific environment, connection, statement, 
        /// or descriptor handle. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlfreehandle-function
        /// </summary>
        /// <param name="HandleType"></param>
        /// <param name="InputHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLFreeHandle([In] SQLSMALLINT HandleType,
                                            [In] SQLHANDLE InputHandle);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLFreeStmt(  
                                    SQLHSTMT       StatementHandle,  
                                    SQLUSMALLINT   Option);  
        */

        /// <summary>
        /// Stops processing associated with a specific statement, closes any open cursors 
        /// associated with the statement, discards pending results, or, optionally, frees 
        /// all resources associated with the statement handle. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlfreestmt-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="Option"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLFreeStmt(
                                                SQLHSTMT StatementHandle,
                                                SQLUSMALLINT Option);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLGetConnectAttr(    SQLHDBC        ConnectionHandle,  
                                        SQLINTEGER     Attribute,  
                                        SQLPOINTER     ValuePtr,  
                                        SQLINTEGER     BufferLength,  
                                        SQLINTEGER *   StringLengthPtr);
        */

        /// <summary>
        /// Returns the current setting of a connection attribute. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlgetconnectattr-function
        /// </summary>
        /// <param name="ConnectionHandle"></param>
        /// <param name="Attribute"></param>
        /// <param name="ValuePtr"></param>
        /// <param name="BufferLength"></param>
        /// <param name="StringLengthPtr"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLGetConnectAttr([In] IntPtr ConnectionHandle,
                                                [In] long Attribute,
                                                [Out] IntPtr ValuePtr,
                                                [In] long BufferLength,
                                                [Out] IntPtr StringLengthPtr);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLGetCursorName( SQLHSTMT        StatementHandle,  
                                    SQLCHAR *       CursorName,  
                                    SQLSMALLINT     BufferLength,  
                                    SQLSMALLINT *   NameLengthPtr);
        */

        /// <summary>
        /// Returns the cursor name associated with a specified statement. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlgetcursorname-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="CursorName"></param>
        /// <param name="BufferLength"></param>
        /// <param name="NameLengthPtr"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLGetCursorName(
                                                        [In] SQLHSTMT StatementHandle,
                                                        [In, Out] SQLCHARPTRINOUT CursorName,  //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.
                                                        [In] SQLSMALLINT BufferLength,
                                                        [Out] out SQLSMALLINTPTR NameLengthPtr);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLGetData(   SQLHSTMT        StatementHandle,
                                SQLUSMALLINT    Col_or_Param_Num,
                                SQLSMALLINT     TargetType,
                                SQLPOINTER      TargetValuePtr,
                                SQLLEN          BufferLength,
                                SQLLEN*         StrLen_or_IndPtr);
        */

        /// <summary>
        /// Retrieves data for a single column in the result set or for a single parameter 
        /// after SQLParamData returns SQL_PARAM_DATA_AVAILABLE. It can be called multiple 
        /// times to retrieve variable-length data in parts. 
        /// See: https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlgetdata-function 
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="ColumnNumber"></param>
        /// <param name="TargetType"></param>
        /// <param name="TargetValuePtr"></param>
        /// <param name="BufferLength"></param>
        /// <param name="StrLen_or_Ind"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLGetData(
                                                [In] SQLHSTMT StatementHandle,
                                                [In] SQLUSMALLINT ColumnNumber,
                                                [In] SQLSMALLINT TargetType,
                                                [In, Out] SQLPOINTER TargetValuePtr,   //This must have global memory allocated to it prior to entry for the routine to write its output to.
                                                [In] SQLLEN BufferLength,
                                                [In] SQLLENPTR StrLen_or_Ind); //This must have global memory allocated to it prior to entry for the routine to write its output to.

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLGetDiagField(  SQLSMALLINT     HandleType,  
                                    SQLHANDLE       Handle,  
                                    SQLSMALLINT     RecNumber,  
                                    SQLSMALLINT     DiagIdentifier,  
                                    SQLPOINTER      DiagInfoPtr,  
                                    SQLSMALLINT     BufferLength,  
                                    SQLSMALLINT *   StringLengthPtr);
        */

        /// <summary>
        /// Returns the current value of a field of a record of the diagnostic data structure 
        /// (associated with a specified handle) that contains error, warning, and status 
        /// information. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlgetdiagfield-function
        /// </summary>
        /// <param name="HandleType"></param>
        /// <param name="Handle"></param>
        /// <param name="RecNumber"></param>
        /// <param name="DiagIdentifier"></param>
        /// <param name="DiagInfoPtr"></param>
        /// <param name="BufferLength"></param>
        /// <param name="StringLengthPtr"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLGetDiagField(
                                                    [In] SQLSMALLINT HandleType,
                                                    [In] SQLHANDLE Handle,
                                                    [In] SQLSMALLINT RecNumber,
                                                    [In] SQLSMALLINT DiagIdentifier,
                                                    [In] SQLPOINTER DiagInfoPtr,          //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.
                                                    [In] SQLSMALLINT BufferLength,
                                                    [In] SQLSMALLINTPTR StringLengthPtr); //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLGetDiagRec(    SQLSMALLINT     HandleType,  
                                    SQLHANDLE       Handle,  
                                    SQLSMALLINT     RecNumber,  
                                    SQLCHAR *       SQLState,  
                                    SQLINTEGER *    NativeErrorPtr,  
                                    SQLCHAR *       MessageText,  
                                    SQLSMALLINT     BufferLength,  
                                    SQLSMALLINT *   TextLengthPtr);
        */

        /// <summary>
        /// Returns the current values of multiple fields of a diagnostic record that contains 
        /// error, warning, and status information. Unlike SQLGetDiagField, which returns one 
        /// diagnostic field per call, SQLGetDiagRec returns several commonly used fields of a 
        /// diagnostic record, including the SQLSTATE, the native error code, and the diagnostic
        /// message text. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlgetdiagrec-function
        /// </summary>
        /// <param name="HandleType"></param>
        /// <param name="Handle"></param>
        /// <param name="RecNumber"></param>
        /// <param name="SQLStatePtr"></param>
        /// <param name="NativeErrorPtr"></param>
        /// <param name="MessageTextPtr"></param>
        /// <param name="BufferLength"></param>
        /// <param name="TextLengthPtr"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLGetDiagRec(
                                                    [In] SQLSMALLINT HandleType,
                                                    [In] SQLHANDLE Handle,
                                                    [In] SQLSMALLINT RecNumber,
                                                    [In] SQLCHARPTRINOUT SQLStatePtr,    //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.
                                                    [In] SQLINTEGERPTR NativeErrorPtr,   //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.
                                                    [In] SQLCHARPTRINOUT MessageTextPtr, //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.
                                                    [In] SQLSMALLINT BufferLength,
                                                    [In] SQLSMALLINTPTR TextLengthPtr);  //This pointer must have global memory allocated to it prior to entry for the routine to write its output to.

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLMoreResults( SQLHSTMT     StatementHandle);
        */

        /// <summary>
        /// Determines whether more results are available on a statement containing SELECT, 
        /// UPDATE, INSERT, or DELETE statements and, if so, initializes processing for those 
        /// results. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlmoreresults-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLMoreResults([In] SQLHSTMT StatementHandle);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLPrepare(   SQLHSTMT      StatementHandle,  
                                SQLCHAR *     StatementText,  
                                SQLINTEGER    TextLength);
        */

        /// <summary>
        /// Prepares an SQL string for execution. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlprepare-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="StatementText"></param>
        /// <param name="TextLength"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLPrepare(
                                                [In] SQLHSTMT StatementHandle,
                                                [In] SQLCHARPTR StatementText,
                                                [In] SQLINTEGER TextLength);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLRowCount(  SQLHSTMT   StatementHandle,  
                                SQLLEN *   RowCountPtr);
         */

        /// <summary>
        /// Returns the number of rows affected by an UPDATE, INSERT, or DELETE statement.
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlrowcount-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="RowCountPtr"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLRowCount([In] IntPtr StatementHandle,
                                            [Out] IntPtr RowCountPtr);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLSetConnectAttr(    SQLHDBC       ConnectionHandle,  
                                        SQLINTEGER    Attribute,  
                                        SQLPOINTER    ValuePtr,  
                                        SQLINTEGER    StringLength);
        */

        /// <summary>
        /// Sets attributes that govern aspects of connections; note that ValuePtr
        /// can be used to pass variable-length objects by reference (e.g. a string) 
        /// in which case StringLength should be set to the length of the object
        /// or to pass fixed-length numbers by value; despite its name, ValuePtr can
        /// also be used to pass fixed-length UInt32 and UInt64 integers by value and 
        /// StringLength is either set to SQL_IS_INTEGER or SQL_IS_UINTEGER, as appropriate.
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlsetconnectattr-function
        /// </summary>
        /// <param name="ConnectionHandle"></param>
        /// <param name="Attribute"></param>
        /// <param name="ValuePtr"></param>
        /// <param name="StringLength"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLSetConnectAttr([In] SQLHDBC ConnectionHandle,
                                                [In] SQLINTEGER Attribute,
                                                [In] SQLPOINTER ValuePtr,
                                                [In] SQLINTEGER StringLength);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLSetEnvAttr(    SQLHENV      EnvironmentHandle,  
                                    SQLINTEGER   Attribute,  
                                    SQLPOINTER   ValuePtr,  
                                    SQLINTEGER   StringLength);
        */

        /// <summary>
        /// Sets attributes that govern aspects of environments. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlsetenvattr-function
        /// </summary>
        /// <param name="EnvironmentHandle"></param>
        /// <param name="Attribute"></param>
        /// <param name="ValuePtr"></param>
        /// <param name="StringLength"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLSetEnvAttr(
                                            [In] SQLHENV EnvironmentHandle,
                                            [In] SQLINTEGER Attribute,
                                            [In] SQLPOINTER ValuePtr,
                                            [In] SQLINTEGER StringLength);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLSetPos(    SQLHSTMT        StatementHandle,  
                                SQLSETPOSIROW   RowNumber,  
                                SQLUSMALLINT    Operation,  
                                SQLUSMALLINT    LockType);
        */

        /// <summary>
        /// Sets the cursor position in a rowset and allows an application to refresh data 
        /// in the rowset or to update or delete data in the result set. 
        /// See:  https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlsetpos-function
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="RowNumber"></param>
        /// <param name="Operation"></param>
        /// <param name="LockType"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLSetPos([In] IntPtr StatementHandle,
                                        [In] UInt64 RowNumber,
                                        [In] ushort Operation,
                                        [In] ushort LockType);

        //-------------------------------------------------------------------------------------

        /*
        SQLRETURN SQLSetStmtAttr(   SQLHSTMT      StatementHandle,  
                                    SQLINTEGER    Attribute,  
                                    SQLPOINTER    ValuePtr,  
                                    SQLINTEGER    StringLength);
        */

        /// <summary>
        /// Sets attributes related to a statement. 
        /// See: https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlsetstmtattr-function 
        /// </summary>
        /// <param name="StatementHandle"></param>
        /// <param name="Attribute"></param>
        /// <param name="ValuePtr"></param>
        /// <param name="StringLength"></param>
        /// <returns></returns>
        [DllImport(ODBC_DLL_PATHNAME, CharSet = CharSet.Ansi)]
        public static extern SQLRETURN SQLSetStmtAttr(
                                                    [In] SQLHSTMT StatementHandle,
                                                    [In] SQLINTEGER Attribute,
                                                    [In] SQLPOINTER ValuePtr,
                                                    [In] SQLINTEGER StringLength);

        //------------------------------------------------------------------------------------------

        /// <summary>
        /// Tests whether a previously returned value of an ODBC call equals 
        /// SQL_SUCCESS (0) or SQL_SUCCESS_WITH_INFO (1).
        /// </summary>
        /// <param name="retVal"> - returned value from a previous ODBC call.</param>
        /// <returns>true or false</returns>
        public static bool IsOK(SQLRETURN retVal)
        {
            bool isOK = false;
            switch (retVal)
            {
                case (SQL_SUCCESS):
                case (SQL_SUCCESS_WITH_INFO):
                    isOK = true;
                    break;
                default:
                    break;
            }
            return isOK;
        }

        /// <summary>
        /// Tests whether a previously returned value of an ODBC call equals SQL_NO_DATA.
        /// </summary>
        /// <param name="retVal"> - returned value from a previous ODBC call.</param>
        /// <returns>true or false</returns>
        public static bool IsNoData(SQLRETURN retVal)
        {
            return retVal == SQL_NO_DATA;
        }




        /// <summary>
        /// Tests whether a previously returned value of an ODBC call equals 
        /// SQL_SUCCESS (0) or SQL_SUCCESS_WITH_INFO (1) or SQL_NO_DATA (100).
        /// </summary>
        /// <param name="retVal"> - returned value from a previous ODBC call.</param>
        /// <returns></returns>
        public static bool IsOKorNoData(SQLRETURN retVal)
        {
            bool isOK = false;
            switch (retVal)
            {
                case (SQL_SUCCESS):
                case (SQL_SUCCESS_WITH_INFO):
                case (SQL_NO_DATA):
                    isOK = true;
                    break;
                default:
                    break;
            }
            return isOK;
        }

        /// <summary>
        /// This method returns a string containing diagnostic information for the previous ODBC query.
        /// </summary>
        /// <param name="hHnd"> - an ODBC statement handle.</param>
        /// <param name="query"> - the SQL query string that was executed.</param>
        /// <returns> - always returns Constant.SUCCESS.</returns>
        public static string GetDiagnostics(SQLHANDLE hHnd, string query)
        {
            //...Log2.v("\n\nODBC.GetDiagnostics(): Entry");

            SQLRETURN sqlRet;
            SQLRETURN returnCode;
            SQLSMALLINT nRec = 1;
            SQLCHARPTRINOUT SQLStatePtr = Marshal.AllocHGlobal(5 + 1);
            SQLINTEGERPTR NativeErrorPtr = Marshal.AllocHGlobal(sizeof(SQLINTEGER));
            SQLCHARPTRINOUT MessageTextPtr = Marshal.AllocHGlobal(Constant.GETDIAGS_BUFFER_SZ);
            SQLSMALLINTPTR TextLengthPtr = Marshal.AllocHGlobal(sizeof(SQLSMALLINT)); ;
            SQLINTEGER nColNo = SQLINTEGER.MinValue;
            SQLLEN nRowNo = SQLINTEGER.MinValue;
            int numRecords;
            SQLPOINTER diagInfoPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Marshal.WriteInt64(diagInfoPtr, 0);  //As per Microsoft's instructions.
            SQLSMALLINTPTR stringLengthPtr = Marshal.AllocHGlobal(sizeof(SQLSMALLINT)); //Not actually used for case of SQL_DIAG_COLUMN_NUMBER.
            StringBuilder sb = new StringBuilder();

            // RecNumber = 0 is the header record of the diagnostic data structure.
            // The fields in the header record contain general information about a 
            // function's execution, including the return code, row count, number 
            // of status records, and type of statement executed. The header record 
            // is always created unless the function returns SQL_INVALID_HANDLE.

            sqlRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, 0, SQL_DIAG_RETURNCODE, diagInfoPtr, ODBC.SQL_IS_INTEGER, stringLengthPtr);
            returnCode = Marshal.ReadInt16(diagInfoPtr);

            string str;
            str = String.Format("\n\nRETURN CODE: {0}", ReturnCodeToString(returnCode));

            //...Log2.v("\r\n" + returnCode);

            // If the SQL query completed successfully then provide its returnCode and
            // then return.
            if (ODBC.IsOK(returnCode))
            {
                sb.Append("\n\nSQL QUERY:\n\n     " + query);
                sb.Append(str);
                return sb.ToString();
            }

            // For actual failure of an SQL query we construct a 
            // comprehensive diagnostics message.
            sb.Append("\n\n========== Start of SQL DIAGNOSTICS ==========");
            sb.Append("\n\nSQL QUERY:\n\n     " + query);

            sb.Append(str);

            // Get the number of diagnostic records from the header.
            sqlRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, 0, ODBC.SQL_DIAG_NUMBER, diagInfoPtr, SQL_C_SLONG, IntPtr.Zero);
            if (!IsOK(sqlRet))
            {
                Console.Error.Write("\nODBC.GetDiagnostics(): ERROR: call to SQLGetDiagField() failed, sqlRet = " + sqlRet);
                return "ERROR: call to SQLGetDiagField() failed.";
            }

            numRecords = Marshal.ReadInt32(diagInfoPtr);
            sb.Append("\n\nNumber of diagnostic records available: " + numRecords);

            //...Log2.v("\nODBC.GetDiagnostics(): numRecords = " + numRecords);

            //Read all the diagnostic records that are available.
            for (nRec = 1; nRec <= numRecords; nRec++)
            {
                //...Log2.v("\r\nODBC.GetDiagnostics(): SQLGetDiagRec() nRec = " + nRec);

                //Get the diagnostic record whose index is nRec.
                //Get the diagnostic results.
                string SQLState = Marshal.PtrToStringAnsi(SQLStatePtr);
                string messageText = Marshal.PtrToStringAnsi(MessageTextPtr);

                //...Log2.v("\r\nODBC.GetDiagnostics(): message = \n" + messageText);

                //	Get more information about a field
                //  nRet = SQLGetDiagField(SQL_HANDLE_STMT, hHnd, nRec,
                //                         SQL_DIAG_COLUMN_NUMBER, &nColNo, SQL_IS_INTEGER, &nRetLen);
                Marshal.WriteInt64(diagInfoPtr, 0);  //As per Microsoft's instructions.

                sqlRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, nRec, (SQLSMALLINT)SQL_DIAG_COLUMN_NUMBER, diagInfoPtr, ODBC.SQL_IS_INTEGER, stringLengthPtr);
                nColNo = Marshal.ReadInt32(diagInfoPtr);
                //...Log2.v("\r\n" + nColNo);

                sqlRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, nRec, (SQLSMALLINT)SQL_DIAG_ROW_NUMBER, diagInfoPtr, ODBC.SQL_IS_INTEGER, stringLengthPtr);
                nRowNo = Marshal.ReadInt64(diagInfoPtr);
                //...Log2.v("\r\n" + nRowNo);

                sqlRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, nRec, SQL_DIAG_SQLSTATE, MessageTextPtr, Constant.GETDIAGS_BUFFER_SZ, stringLengthPtr);
                string sqlState = Marshal.PtrToStringAnsi(MessageTextPtr);
                //...Log2.v("\r\n" + sqlState);

                sqlRet = ODBC.SQLGetDiagField(ODBC.SQL_HANDLE_STMT, hHnd, nRec, SQL_DIAG_MESSAGE_TEXT, MessageTextPtr, Constant.GETDIAGS_BUFFER_SZ, stringLengthPtr);
                //...Log2.v("\r\nnRet = " + sqlRet);
                string mess = Marshal.PtrToStringAnsi(MessageTextPtr);
                //...Log2.v("\r\n" + mess);

                str = String.Format("\n\nRec # {0}: MESSAGE  : {1}", nRec, mess);
                sb.Append(str);
                str = String.Format("\nRec # {0}: SQLSTATE : {1}", nRec, sqlState);
                sb.Append(str);
                str = String.Format("\nRec # {0}: COLUMN # : {1}", nRec, RowColNumToString(nColNo));
                sb.Append(str);
                str = String.Format("\nRec # {0}: ROW #    : {1}", nRec, RowColNumToString(nRowNo));
                sb.Append(str);

            } // for-loop over all available diagnostic records.

            sb.Append("\n\n============ End of SQL DIAGNOSTICS ==========\n");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string that indicates the type of ODBC
        /// return code for a previously executed SQL query, e.g. "SQL_SUCCESS".
        /// </summary>
        /// <param name="sqlRet"></param>
        /// <returns></returns>
        public static string ReturnCodeToString(SQLRETURN sqlRet)
        {
            string result = "";

            switch (sqlRet)
            {
                case SQL_SUCCESS:
                    result = "SQL_SUCCESS";
                    break;
                case SQL_SUCCESS_WITH_INFO:
                    result = "SQL_SUCCESS_WITH_INFO";
                    break;
                case SQL_ERROR:
                    result = "SQL_ERROR";
                    break;
                case SQL_INVALID_HANDLE:
                    result = "SQL_INVALID_HANDLE";
                    break;
                case SQL_NO_DATA:
                    result = "SQL_NO_DATA";
                    break;
                case SQL_NEED_DATA:
                    result = "SQL_NEED_DATA";
                    break;
                case SQL_STILL_EXECUTING:
                    result = "SQL_STILL_EXECUTING";
                    break;
                default:
                    result = "ERROR: invalid return code.";
                    break;
            }

            return result;
        }

        /// <summary>
        /// This method returns a string that provides the numeric value of an ODBC
        /// row-column indicator or "not available" or "indeterminate", as appropriate.
        /// </summary>
        /// <param name="numRowCol"></param>
        /// <returns></returns>
        public static string RowColNumToString(SQLLEN numRowCol)
        {
            string result = "";

            switch (numRowCol)
            {
                case SQL_NO_COLUMN_NUMBER:
                    result = "not available";
                    break;
                case SQL_COLUMN_NUMBER_UNKNOWN:
                    result = "indeterminate";
                    break;
                default:
                    result = Convert.ToString(numRowCol);
                    break;
            }

            return result;
        }



    } //class
} //namespace


```
