using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;
using _NewLib;
using _Configuration;

namespace _Utillib
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
    /// Provides methods to retrieve user information and to update, insert 
    /// or delete operations for records in the 'central table' <b>web.user_tables</b>.
    /// </summary>
    public class UserInfo
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utGetUserInfo([In, Out] UserInfoData uInfo);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utUpdateCentralTable([In] string cmd, [In] string fname, [In] int tabType, [In] string vstat, [In]string mark);

        public static int UtUpdateCentralTable_NATIVE(string cmd, string fname, int tabType, string vstat, string mark)
        {
            return utUpdateCentralTable(cmd, fname, tabType, vstat, mark);
        }
#endif
        //------------------------------------------------------------------------------------------------------------
        private static string cSystemId = null;
        //------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates and populates a UserInfoData object that provides information on process ID,
        /// MICS user name, operator name, and project code.
        /// </summary>
        /// <param name="userInfo"> - populated UserInfoData object.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int UtGetUserInfo(out UserInfoData userInfo)
        {
            //...Log2.v("\nUserInfo.UtGetUserInfo(): Entry");

            userInfo = new UserInfoData();
            bool isOK;
            string cUserName;
            int nRet = 0;

            isOK = Ssutil.GetMicsID(out cUserName);
            if (isOK)
            {
                userInfo.micsUser = new MicsUsers();
                userInfo.micsUser.micsid = cUserName;
            }
            else
            {
                Console.Write("\r\nutGetUserInfo: Could not get the current user's name.\r\n");
                Console.Out.Flush();
                Log2.e("\r\nUserInfo.UtGetUserInfo(): ERROR: GetMicsID(): FAILED");
                return -1;
            }

            //	Get the company name from the database
            string cSchemaName = Info.GlobalSchema;  // Ssutil.GetFCSASchema();
            isOK = !String.IsNullOrWhiteSpace(cSchemaName);
            if (isOK)
            {
                userInfo.oper = cSchemaName;
            }
            else
            {
                string usermess = GenUtil.GetUserMess();
                Console.Write("\r\nUserInfo.UtGetUserInfo(): Could not get the current user's schema:-\r\n" + usermess + "\r\n");
                Console.Out.Flush();
                Log2.e("\r\nUserInfo.UtGetUserInfo(): ERROR: utGetUserInfo(): getFCSASchema(): FAILED: \r\n" + usermess);
                return -2;
            }

            //    safecopy(userInfo->pcode, getprojectcode(NULL), sizeof(userInfo->pcode));
            string pCode;
            GenUtil.GetProjectCode(out pCode);
            userInfo.pCode = pCode;

            //	Get the SystemId (old ultrixid) from the micsUser.
            //	Currently we use the account_users table to link the two.
            //	if (strlen(cSystemId) == 0){
            if (String.IsNullOrWhiteSpace(cSystemId))
            {
                //		nRet = getSystemId(cSystemId, sizeof(cSystemId));
                nRet = Ssutil.GetSystemId(out cSystemId, Constant.ID_SZ);
                if (nRet != 0)
                {
                    //sprintf_s(cSystemId, sizeof(cSystemId), "%s:%d", cUserName, nRet);
                    //cSystemId = cUserName + ":" + nRet;
                    Log2.e("\n\nUserInfo.UtGetUserInfo(): ERROR: call to Ssutil.GetSystemId() FAILED, nRet = " + nRet);
                    Application.ExitQuietly(77701);
                }
            }
            //    safecopy(userInfo->micsUser.ultrixid, cSystemId, sizeof(userInfo->micsUser.ultrixid));
            userInfo.micsUser.ultrixid = cSystemId;

            //...Log2.v("\r\nuserInfo structure field values:");
            //...Log2.v("\n" + userInfo.ToString());

            //...Log2.v("\nUserInfo.UtGetUserInfo(): Exit");
            return nRet;
        }


        //public static int UtUpdateCentralTable(string cmd, string pdfName, int tabType, string vstat)
        //{
        //    return UpdateCentralTable(cmd, tabType, pdfName, vstat);
        //}

        /// <summary>
        /// Performs update, insert or delete operations for records in the 'central table' <b>web.user_tables</b>.
        /// </summary>
        /// <param name="cCmd"> - operation command letter: 'A', 'U' or 'D'.</param>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="nTableType"> - table ID number (see the code for Cvt.ConstructCvtList).</param>
        /// <param name="cValidStat"> - validation status letter, one of 'N', 'P' or 'U'.</param>
        /// <param name="cMark"> - not used.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int UtUpdateCentralTable(string cCmd, string pdfName, int nTableType, string cValidStat, string cMark)
        {
            Log2.v("\nUserInfo.UtUpdateCentralTable(): Entry:");
            Log2.v(String.Format("\nUserInfo.UtUpdateCentralTable(): |{0}|{1}|{2}|{3}|{4}|", cCmd, pdfName, nTableType, cValidStat, cMark));

            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;

            bool IsAlreadyThere;
            string cSQL;
            int nRet;
            UserInfoData tInfo;

            //	Who am i?
            nRet = UtGetUserInfo(out tInfo);
            if (nRet < 0)
            {
                Log2.e("\nUserInfo.UtUpdateCentralTable(): ERROR: call to UtGetUserInfo() failed, nRet = " + nRet);
                GenUtil.SetErr("updateCentralTable: Could not get user info: %d", nRet.ToString());
                return -1;
            }

            //	First check to see if the record already exists.
            cSQL = String.Format("tabletype={0} and file_name='{1}' and operator='{2}' ", nTableType, pdfName, Info.GlobalSchema);

            IsAlreadyThere = Ssutil.DbCountRows("web.user_tables", cSQL) > 0;

            char cmdChar = 'z';
            if (cCmd != null && cCmd.Length > 0)
            {
                cmdChar = cCmd[0];
            }

            switch (cmdChar)
            {
                case 'A':
                    if (IsAlreadyThere)
                    {
                        Log2.e("\nUserInfo.UtUpdateCentralTable(): ERROR: 'A': web.user_tables: cSQL = " + cSQL + ".  IsAlreadyThere = " + IsAlreadyThere);
                        nRet = -2;
                    }
                    else
                    {
                        cSQL = String.Format("INSERT INTO web.user_tables (operator, tabletype, file_name, micsid, project_code, validstat, create_date) VALUES ('{0}', {1}, '{2}', '{3}', '{4}', '{5}', CURRENT_TIMESTAMP)",
                                                Info.GlobalSchema, nTableType, pdfName, tInfo.micsUser.micsid, tInfo.pCode, cValidStat);
                        nRet = 0;
                    }
                    break;

                case 'U':
                    //	You can only update the valid status.
                    if (IsAlreadyThere)
                    {
                        cSQL = String.Format("UPDATE web.user_tables set validstat = '{0}' where tabletype={1} and file_name='{2}' and operator='{3}' ",
                                             cValidStat, nTableType, pdfName, Info.GlobalSchema);
                        nRet = 0;
                    }
                    else
                    {
                        Log2.e("\nUserInfo.UtUpdateCentralTable(): ERROR: 'U': web.user_tables : cSQL = " + cSQL + ".  IsAlreadyThere = " + IsAlreadyThere);
                        nRet = Constant.NOMORERECS;
                    }
                    break;

                case 'D':
                    if (IsAlreadyThere)
                    {
                        cSQL = String.Format("DELETE from web.user_tables where tabletype={0} and file_name='{1}' and operator='{2}' ",
                                          nTableType, pdfName, Info.GlobalSchema);
                        nRet = 0;
                    }
                    else
                    {
                        Log2.e("\nUserInfo.UtUpdateCentralTable(): ERROR: 'D': web.user_tables: cSQL = " + cSQL + ".  IsAlreadyThere = " + IsAlreadyThere);
                        nRet = Constant.NOMORERECS;
                    }
                    break;

                default:
                    Log2.e("\nUserInfo.UtUpdateCentralTable(): ERROR: invalid command letter, cmdChar = " + cmdChar);
                    GenUtil.SetErr("updateCentralTable: Invalid Command: %s", cCmd);
                    nRet = -3;
                    break;
            }

            if (nRet == 0)
            {
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nUserInfo.UtUpdateCentralTable(): ERROR: call to SQLExecDirect() failed, cSQL = {0}", cSQL);
                    Ssutil.DbGetDiagStmt(hStmt, "updateCentralTable: Execute failure on:\r\n" + cSQL);
                    nRet = -4;
                }
                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            }

            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nUserInfo.UtUpdateCentralTable(): Exit: nRet = " + nRet);
            return nRet;
        }
    }
}
