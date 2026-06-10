# Documented File: Bind.cs
**Repository Path:** `_NewLib\Bind.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
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

    /// <summary>
    /// Provides methods that bind a buffer containing a value 
    /// of a specific type (string, short, int, float or double) to a particular
    /// column index in a MS SQL Server DB table using the ODBC driver (client).
    /// </summary>
    /// <remarks>
    /// See: https://docs.microsoft.com/en-us/sql/odbc/reference/syntax/sqlbindcol-function?view=sql-server-2017
    /// <para>
    /// ODBC is a native-code library meaning that it uses heap ('unmanaged') memory. In contrast,
    /// C# programs compile to produce 'managed code' applications in which the programs
    /// use and access to memory is structly regulated.</para>
    /// <para>
    /// In the following description, <b>the term buffer means a block of contiguous memory
    /// in the heap sufficient to store a value of a particular type.</b> A C# program can
    /// only interact with heap memory by using Microsoft's <b>P/Invoke</b> functionality that
    /// provides for inter-operability between managed and unmanaged code.</para>
    /// <para>
    /// ODBC's SQLBindCol is used to associate, or bind, columns in the result set
    /// to data buffers and length/indicator buffers in the application. When the 
    /// application calls SQLFetch, SQLFetchScroll, or SQLSetPos to fetch data, 
    /// the driver returns the data for the bound columns in the specified buffers</para>
    /// <para>
    /// Columns do not have to be bound to retrieve data from them. An application can 
    /// also call SQLGetData to retrieve data from columns. Although it is possible to
    /// bind some columns in a row and call SQLGetData for others, this is subject to 
    /// some restrictions.</para>
    /// <para>
    /// Data can be retrieved from the database using either the SQLBindCol function or
    /// the SQLGetData function. When  SQLBindCol  is  called, it associates, or binds,
    /// a variable  to a  column  in  the result  set. Nothing  is  sent to  the database.
    /// SQLBindCol tells the driver to remember the addresses of the variables, which the 
    /// driver will use to store the data when it is actually retrieved. When SQLFetch 
    /// is executed, the driver places the data into the addresses of the variables specified
    /// bySQLBindCol. In  contrast,SQLGetData returns  data  directly  into  variables.</para>  
    /// <para><b>
    /// Retrieving data using the SQLBindCol function instead of using the SQLGetData function 
    /// reduces the number of ODBC calls, and ultimately the number of network round trips, 
    /// and can significantly improve real-time performance.</b></para>
    /// <para>
    /// A column can be bound, unbound, or rebound at any time, even after data has 
    /// been fetched from the result set. The new binding takes effect the next time 
    /// that a function that uses bindings is called. For example, suppose an application 
    /// binds the columns in a result set and calls SQLFetch. The driver returns the 
    /// data in the bound buffers. Now suppose the application binds the columns to a 
    /// different set of buffers.The driver does not put the data for the just-fetched 
    /// row in the newly bound buffers. Instead, it waits until SQLFetch is called again
    /// and then places the data for the next row in the newly bound buffers.</para>
    /// <para>
    /// To bind a column, an application calls SQLBindCol and passes the column number, 
    /// type, address, and length of a data buffer, and the address of a length/indicator 
    /// buffer.</para> 
    /// <para>
    /// The use of these buffers is <b>deferred</b>; that is, the application binds 
    /// them in SQLBindCol but the driver accesses them from other functions � namely,
    /// SQLFetch, SQLFetchScroll, or SQLSetPos. It is the application's responsibility
    /// to make sure that the pointers specified in SQLBindCol remain valid as long as 
    /// the binding remains in effect. If the application allows these pointers to become
    /// invalid � for example, it frees a buffer � and then calls a function that expects
    /// them to be valid, the consequences are undefined.</para>
    /// <para>
    /// The binding remains in effect until it is replaced by a new binding, the column 
    /// is unbound, or the ODBC statement is freed.</para>
    /// <para>
    /// To unbind a single column, an application calls SQLBindCol with ColumnNumber set
    /// to the number of that column and TargetValuePtr set to a null pointer. If ColumnNumber
    /// refers to an unbound column, SQLBindCol still returns SQL_SUCCESS.</para>
    /// <para></para>
    /// To unbind all columns, an application calls SQLFreeStmt with fOption set to 
    /// SQL_UNBIND.This can also be accomplished by setting the SQL_DESC_COUNT field of 
    /// the ARD to zero.
    /// </remarks>
    public class Bind
    {
        /// <summary>
        /// This method binds a buffer in unmanaged memory to a particular column
        /// position in an ODBC result set, for the transfer of an ASCII string. Note that
        /// result set column positions start at 1, not 0.
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="colNum"> - result set column positions</param>
        /// <param name="targetValuePtr"> - the buffer's start address (via an IntPtr).</param>
        /// <param name="bufferLength"> - maximum number of characters that can be stored in the buffer.</param>
        /// <param name="nullIndPtr"> - the start address (via IntPtr) of another buffer used to store an ODBC null indicator.</param>
        public static void BindBufferToString(SQLHANDLE hStmt, int colNum, SQLPOINTER targetValuePtr, SQLLEN bufferLength, SQLPOINTER nullIndPtr)
        {
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLBindCol(
                                        hStmt,
                                        (ushort)colNum,
                                        ODBC.SQL_C_CHAR,
                                        targetValuePtr,
                                        bufferLength,
                                        nullIndPtr);
            if (ODBC.IsOK(sqlRet))
            {
                return;
            }
            else
            {
                Log2.e("\nSsutil.BindBuffersToString(): ERROR: call to SQLBindCol() returned " + sqlRet);
                Log2.e("\nStackTrace: {0}", Environment.StackTrace);
                Application.Exit(666);
            }
        }

        /// <summary>
        /// This method binds a buffer in unmanaged memory to a particular column
        /// position in an ODBC result set, for the transfer of a short (Int16) integer. Note that
        /// result set column positions start at 1, not 0.
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="colNum"> - result set column positions</param>
        /// <param name="targetValuePtr"> - the buffer's start address (via an IntPtr).</param>
        /// <param name="nullIndPtr"> - the start address (via IntPtr) of another buffer used to store an ODBC null indicator.</param>
        public static void BindBufferToShort(SQLHANDLE hStmt, int colNum, SQLPOINTER targetValuePtr, SQLPOINTER nullIndPtr)
        {
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLBindCol(
                                        hStmt,
                                        (ushort)colNum,
                                        ODBC.SQL_C_SSHORT,
                                        targetValuePtr,
                                        sizeof(short),
                                        nullIndPtr);
            if (ODBC.IsOK(sqlRet))
            {
                return;
            }
            else
            {
                Log2.e("\nSsutil.BindBuffersToShort(): ERROR: call to SQLBindCol() returned " + sqlRet);
                Application.Exit(666);
            }
        }

        /// <summary>
        /// This method binds a buffer in unmanaged memory to a particular column
        /// position in an ODBC result set, for the transfer of an integer (Int32). Note that
        /// result set column positions start at 1, not 0.
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="colNum"> - result set column positions</param>
        /// <param name="targetValuePtr"> - the buffer's start address (via an IntPtr).</param>
        /// <param name="nullIndPtr"> - the start address (via IntPtr) of another buffer used to store an ODBC null indicator.</param>
        public static void BindBufferToInt(SQLHANDLE hStmt, int colNum, SQLPOINTER targetValuePtr, SQLPOINTER nullIndPtr)
        {
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLBindCol(
                                        hStmt,
                                        (ushort)colNum,
                                        ODBC.SQL_C_SLONG,  // C 'long' is only 32-bits!
                                        targetValuePtr,
                                        sizeof(int),
                                        nullIndPtr);
            if (ODBC.IsOK(sqlRet))
            {
                return;
            }
            else
            {
                Log2.e("\nSsutil.BindBuffersToInt(): ERROR: call to SQLBindCol() returned " + sqlRet);
                Application.Exit(666);
            }
        }

        /// <summary>
        /// This method binds a buffer in unmanaged memory to a particular column
        /// position in an ODBC result set, for the transfer of a float (32-bit real). Note that
        /// result set column positions start at 1, not 0.
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="colNum"> - result set column positions</param>
        /// <param name="targetValuePtr"> - the buffer's start address (via an IntPtr).</param>
        /// <param name="nullIndPtr"> - the start address (via IntPtr) of another buffer used to store an ODBC null indicator.</param>
        public static void BindBufferToFloat(SQLHANDLE hStmt, int colNum, SQLPOINTER targetValuePtr, SQLPOINTER nullIndPtr)
        {
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLBindCol(
                                        hStmt,
                                        (ushort)colNum,
                                        ODBC.SQL_C_FLOAT,
                                        targetValuePtr,
                                        sizeof(int),
                                        nullIndPtr);
            if (ODBC.IsOK(sqlRet))
            {
                return;
            }
            else
            {
                Log2.e("\nSsutil.BindBuffersToFloat(): ERROR: call to SQLBindCol() returned " + sqlRet);
                Application.Exit(666);
            }
        }

        /// <summary>
        /// This method binds a buffer in unmanaged memory to a particular column
        /// position in an ODBC result set, for the transfer of a double (64-bit real). Note that
        /// result set column positions start at 1, not 0.
        /// </summary>
        /// <param name="hStmt"> - ODBC statement handle.</param>
        /// <param name="colNum"> - result set column positions</param>
        /// <param name="targetValuePtr"> - the buffer's start address (via an IntPtr).</param>
        /// <param name="nullIndPtr"> - the start address (via IntPtr) of another buffer used to store an ODBC null indicator.</param>
        public static void BindBufferToDouble(SQLHANDLE hStmt, int colNum, SQLPOINTER targetValuePtr, SQLPOINTER nullIndPtr)
        {
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLBindCol(
                                        hStmt,
                                        (ushort)colNum,
                                        ODBC.SQL_C_DOUBLE,
                                        targetValuePtr,
                                        sizeof(int),
                                        nullIndPtr);
            if (ODBC.IsOK(sqlRet))
            {
                return;
            }
            else
            {
                Log2.e("\nSsutil.BindBuffersToDouble(): ERROR: call to SQLBindCol() returned " + sqlRet);
                Application.Exit(666);
            }
        }







    }
}

```
