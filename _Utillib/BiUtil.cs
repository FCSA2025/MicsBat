using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Data.Odbc;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// This is the biggest and oldest of the MICS libraries, providing a 
/// very large number of classes and methods to support MICS 
/// application programs. Generally, a class is included in Utillib <b>only if it 
/// is used by more than one MICS application (VS Project).</b>.
/// </summary>
namespace _Utillib
{
    using System.Text;
    using SQLCHAR = Byte;
    using SQLCHARPOINTER = String;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// Provides methods to initialize, calculate and write out billing information.
    /// </summary>
    public class BiUtil
    {
        private static string function = null;
        private static bool hasBeenInitialized = false;
        static Int64 uTime = 0;
        static Int64 cTime = 0;
        private static Int64 tStartTime = 0;
        static int nUserTime = 0;
        static string cInfo = null;

        /// <summary>
        /// Calculates billing information, and writes a billing recording the web.daily_usage_view table. 
        /// On the initial call, information is gathered but no record is written. On all subsequent calls the 
        /// billing information is gathered and a billing record is inserted into the database.  
        /// </summary>
        /// <remarks>The record contains billing information pertaining to the preceeding, or completed billing segment. 
        /// Therefore, the last call to BiBillingRec writes the final billing record, and the"func" parameter is not used. 
        /// The values stored by this method are the processor-time and elapsed-time and io counts between calls.
        /// </remarks>
        /// <param name="func"> - a description of the MICS program run by the user, e.g. "FTVALIDATE".</param>
        /// <param name="cFile"> - string to be written to the 'info' column of the billing record.</param>
        /// <returns></returns>
        /// <para>Constant.SUCCESS             - successfull call.</para>
        /// <para>ErrorMessages.NOCONSM        - could not connect to SHARED MEM</para>
        /// <para>ErrorMessages.NOSMENTRY      - no SHARED MEMORY entry for current pid or parent pid</para>
        /// <para>ErrorMessages.DYN_MS_SQL_SERVER_ERR - error writing billing record.</para>
        public static int BiBillingRec(string func, string cFile)
        {
            //            char opCode[OPERCODE_SZ];   /* ultrix userid */
            string opCode;
            //            char micsId[32];    /* mics userid */
            //            char pCode[PCODE_SZ];       /* project code */
            //            int pTime;      /* processing time in msecs */
            int pTime;
            //            char curDate[DATE_SZ];  /* current date - format yyyy.mm.dd */
            string curDate = new string(new char[15]);
            //            char curTime[TIME_SZ];	/* current time - format hh:mm:ss */
            string curTime = new string(new char[10]);
            //struct userInfo_ userInfo;		/* user information struct */
            UserInfo userInfo = new UserInfo();		/* user information struct */
            //        FILETIME tCreateTime;
            System.Runtime.InteropServices.ComTypes.FILETIME tCreateTime;
            //        FILETIME tExitTime;
            System.Runtime.InteropServices.ComTypes.FILETIME tExitTime;
            //        FILETIME tKernelTime;
            System.Runtime.InteropServices.ComTypes.FILETIME tKernelTime;
            //        FILETIME tUserTime;
            System.Runtime.InteropServices.ComTypes.FILETIME tUserTime;
            //	      ULARGE_INTEGER uTimeAcc1;  //This is a legacy datastructure that uses two 32-bit ints to represents one 64-bit integer.
            _DataStructures.LargeInteger uTimeAcc1;
            //	      ULARGE_INTEGER uTimeAcc1;  //This is a legacy datastructure that uses two 32-bit ints to represents one 64-bit integer.
            _DataStructures.LargeInteger uTimeAcc2;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            opCode = Info.GlobalSchema;

            Kernel32.Time(ref uTime);

            cTime = uTime;

            // if this is function has been initialized, calculate real and cpu
            //times and write a record to the billing table */
            if (hasBeenInitialized)
            {
                //        /* get current date and time */
                //        utGetDateTime(curDate, curTime);
                GenUtil.UtGetDateTime(out curDate, out curTime);
                //...Log2.v("\n");
                //...Log2.v("\nBiUtil.cs:  biBillingRec(): curDate = " + curDate);
                //...Log2.v("\nBiUtil.cs:  biBillingRec(): curTime = " + curTime);
                //...Log2.v("\nBiUtil.cs:  biBillingRec(): cTime = " + cTime.ToString());
                //...Log2.v("\nBiUtil.cs:  biBillingRec(): tStartTime = " + tStartTime.ToString());

                //    cTime -= tStartTime;		//	Count how long we have been going.
                cTime -= tStartTime;

                //if (!GetProcessTimes(GetCurrentProcess(), &tCreateTime, &tExitTime,
                //                       &tKernelTime, &tUserTime))
                IntPtr thisProcessHandle = Process.GetCurrentProcess().Handle;
                bool getProcessTimeSuccessful = Kernel32.GetProcessTimes(thisProcessHandle, out tCreateTime, out tExitTime,
                                                                            out tKernelTime, out tUserTime);
                if (!getProcessTimeSuccessful)
                {
                    GenUtil.SetErr("biBillingRec01: Problem getting process times.");
                    return (Constant.FAILURE);
                }


                uTimeAcc1.QuadPart = 0;    //Stops the compiler from complaining that Quadpart may not be assigned.
                uTimeAcc2.QuadPart = 0;    //Stops the compiler from complaining that Quadpart may not be assigned.
                uTimeAcc1.HighPart = tKernelTime.dwHighDateTime;
                uTimeAcc1.LowPart = (UInt32)tKernelTime.dwLowDateTime;
                uTimeAcc2.HighPart = tUserTime.dwHighDateTime;
                uTimeAcc2.LowPart = (UInt32)tUserTime.dwLowDateTime;
                nUserTime = (int)(uTimeAcc2.QuadPart / 10000);
                uTimeAcc1.QuadPart += uTimeAcc2.QuadPart;       //	Add the Kernel and user times.
                pTime = (int)(uTimeAcc1.QuadPart / 10000); // time is in 0.1 microseconds convert to ms.

                /* write record to billing table */
                //hConn = newConn();
                SQLHDBC hConn = Ssutil.NewConn();

                //ODBC.
                //sqlRet = SQLAllocHandle(SQL_HANDLE_STMT, hConn, &hStmt);
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                StringBuilder sb = new StringBuilder(Constant.SQLCOMMANDSTRING_SZ);
                sb.Append("insert into web.daily_usage ");
                sb.Append("(operator, micsid, project_code, mics_function, user_cputime, total_cputime, connect_time, create_date, info) ");
                sb.Append("values ");
                sb.Append("( ");
                sb.Append("'" + opCode + "', ");
                sb.Append("'" + Info.MicsUserName + "', ");
                sb.Append("'" + Info.ProjectCode + "', ");
                sb.Append("'" + function + "', ");
                sb.Append(nUserTime + ", ");
                sb.Append(pTime + ", ");
                sb.Append(cTime + ", ");
                sb.Append("CURRENT_TIMESTAMP" + ", ");
                sb.Append("'" + cInfo + "' ");
                sb.Append(") ");
                string cSQL = sb.ToString();

                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nBiUtil.BiBillingRec(): ERROR: call to SQLExecDirect() failed for: " + cSQL);
                    Ssutil.DbGetDiagStmt(hStmt, "biBillingRec03: Problem inserting billing record:\r\n");
                    sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                    Ssutil.DisConn(hConn);
                    return -3;
                }

                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                Ssutil.DisConn(hConn);

            }
            else
            {
                /* first time through, set to true */
                hasBeenInitialized = true;

                //        safecopy(function, func, sizeof(function));
                GenUtil.SafeCopy(ref function, func, 11);
                //        safecopy(cInfo, cFile, sizeof(cInfo));
                GenUtil.SafeCopy(ref cInfo, cFile, Constant.DU_INFO_SZ);
                //		tStartTime = cTime;
                tStartTime = cTime;
                //...Log2.v("\r\nBiUtil.cs:  biBillingRec(): tStartTime = " + tStartTime.ToString());
            }

            //...Log2.v("\n\nBiUtil.cs:  biBillingRec(): succeeded");
            return (Constant.SUCCESS);
        }



    }
}
