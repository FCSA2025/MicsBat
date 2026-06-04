using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SqlMail
{
    public struct structSqlMsg
    {
        public string emailFrom;
        public string emailTo;
        public string emailCC;
        public string emailBCC;
        public string emailSubject;
        public string emailBody;
        public string emailAttach;

        public structSqlMsg(structSqlMsg xx)
        {
            emailFrom = xx.emailFrom;
            emailTo = xx.emailTo;
            emailCC = xx.emailCC;
            emailBCC = xx.emailBCC;
            emailSubject = xx.emailSubject;
            emailBody = xx.emailBody;
            emailAttach = xx.emailAttach;
        }
    }

}
