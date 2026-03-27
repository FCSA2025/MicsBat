using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using SQLRETURN = Int16;
    public class BillingUtil
    {
        /// <summary>
        /// This method returns the operator code of the FCSA member 'responsible' for
        /// monitoring the PCN messages of the enterprise identified by the prescribed 
        /// operator code; w.r.t the SDB table 'sd_oper' if the record has a non-NULL 'cooper'
        /// field then this is the member operation code that is returned; if 'cooper' is 
        /// NULL then the content of the 'oper' field is returned.
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="membersOpCode"></param>
        /// <returns></returns>
        public static int UtGetMemberOpCode(string opCode, out string membersOpCode)
        {
            // 'out' requirement.
            membersOpCode = "";

            int nRet;
            SuOper tOper;

            nRet = Suutils.SuGetOper(opCode, out tOper);

            if (nRet != 0)
            {
                if (nRet == 1)
                {
                    /* if the operator code could not be found in sd_oper */
                    return (Error.NOMEMOPCODE);
                }
                /* some unexpected error */
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            if (String.IsNullOrWhiteSpace(tOper.cooper))
            {
                /* the cooper field is blank so the member op code is itself */
                membersOpCode = opCode;
            }
            else
            {
                membersOpCode = tOper.cooper;
            }

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method provides the ultrix userid of the FCSA member 'responsible' for 
        /// monitoring eMail PCN messages relating to a prescribed operator code; if the 
        /// member registered with the prescribed operator code has authorized another 
        /// member to 'act on their behalf' for monitoring their PCN messages then the ultrix 
        /// userid provided is that of the 'acting' member; a status character is
        /// also provided that indicates whether the returned ultrix userid is that of
        /// that of the original member (status = 'M') or an 'acting' member (status = 'A').
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int UtGetUserId(string opCode, out string userId, out char status)
        {
            // 'out' requirements.
            userId = "";
            status = '\0';

            string membersOpCode = "";
            int rc;


            if ((rc = UtGetMemberOpCode(opCode, out membersOpCode)) != Constant.SUCCESS)
            {
                /* problems getting members operator code */
                if (rc == Error.NOMEMOPCODE)
                {
                    GenUtil.SetErr("utGetUserId: Could not get member's opcode: %s", opCode);
                }
                else
                {
                    GenUtil.SetErr("utGetUserId: Error getting opcode: %s", opCode);
                }
                return (rc);
            }

            if (opCode.Trim().Equals(membersOpCode.Trim()))
            {
                status = Constant.MICS_MEMBER;
            }
            else
            {
                status = Constant.ACTING_MEMBER;
            }

            SuOpCode tOpCode;

            rc = Suutils.GetSuOpCode(membersOpCode, out tOpCode);

            if (rc != 0)
            {
                /* problems */
                if (rc == 1)
                {
                    /* could not find a match in the mics_user table */
                    return (Error.NOUSERID);
                }
                /* unexpected error */
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            userId = tOpCode.ultrixid.Trim();

            return (0);
        }

        /// <summary>
        /// This method inserts a prescribed user ID string in the first unused
        /// element of an array of strings.
        /// </summary>
        /// <param name="userIdList"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int UtAddUserId(string[] userIdList, string userId)
        {
            int i;
            int rc;

            rc = Constant.SUCCESS;
            i = 0;

            /* find the end of the list */
            while (!String.IsNullOrWhiteSpace(userIdList[i]))
            {
                i++;
            }

            /* add it to the list */
            userIdList[i++] = userId;

            /* is the list full */
            if (i == Constant.MAX_OPERCODE_LIST)
            {
                /* the list is full */
                i--;
                rc = Error.USERLISTFULL;
            }

            /* null terminate the list */
            userIdList[i] = null;

            return (rc);
        }

        /// <summary>
        /// This method returns true if a prescribed user ID is already present as an
        /// element of a prescribed string array; note that the string comparison
        /// employed by this method is case-sensitive.
        /// </summary>
        /// <param name="userIdList"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool UtUserIdInList(string[] userIdList, string userId)
        {
            int i;

            i = 0;
            /* check each item in the list */
            while (!String.IsNullOrWhiteSpace(userIdList[i]))
            {
                if (userId.Equals(userIdList[i++]))
                {
                    /* FOUND */
                    return true;
                }
            }

            /* NOT FOUND */
            return false;
        }

        /// <summary>
        /// This method returns true if a prescribed user ID is already present as an
        /// element of a prescribed string array; note that the string comparison
        /// employed by this method is case-sensitive.
        /// </summary>
        /// <param name="userIdList"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool UtUserIdInList(List<string> userIdList, string userId)
        {
            foreach (string userIdListElement in userIdList)
            {
                if (userId.Equals(userIdListElement))
                {
                    /* FOUND */
                    return true;
                }
            }

            /* NOT FOUND */
            return false;
        }

        /// <summary>
        /// This method returns an integer value that indicates whether the operator
        /// with a prescribed opCode is an FCSA member (returns 0), or is not an FCSA
        /// member (returns -1); any other returned value indicates an error condition.
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public static int UtMemberOpCode(string opCode)
        {
            SuOper tOper;
            int nRet;

            nRet = Suutils.SuGetOper(opCode, out tOper);

            if (nRet != 0)
            {
                /* some unexpected ingres error */
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* check to see if it is an FCSA member */
            if (tOper.opnote[0] == 'F')
            {
                return (Constant.SUCCESS);
            }

            return (Constant.FAILURE);
        }


    }
}
