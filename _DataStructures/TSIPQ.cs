using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class has fields that are isomorphic to the columns in the database table
    /// <b>web.tsip_queue</b> .
    /// </summary>
    public class TSIPQ
    {
        public int TQ_Job;

        public string TQ_Status;

        public int TQ_Finish;

        public string TQ_ArgDB;

        public string TQ_ArgPC;

        public string TQ_ArgDest;

        public string TQ_ArgFile;

        public int TQ_ProcID;

        public string TQ_EventName;

        public string TQ_MicsID;

        public TM TQ_TimeIn;

        public TM TQ_TimeStart;

        public TM TQ_TimeEnd;

        //----------------------------------------------------------------------

        public const int TQ_STATUS = 2;
        public const int TQ_ARGDB = 5;
        public const int TQ_ARGPC = Constant.PCODE_SZ;
        public const int TQ_ARGDEST = 256;
        public const int TQ_ARGFILE = 256;
        public const int TQ_EVENTNAME = 33;
        public const int TQ_MICSID = 33;

        //----------------------------------------------------------------------

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nTQ_Job       = " + TQ_Job);
            sb.Append("\nTQ_Status    = " + TQ_Status);
            sb.Append("\nTQ_Finish    = " + TQ_Finish);
            sb.Append("\nTQ_ArgDB     = " + TQ_ArgDB);
            sb.Append("\nTQ_ArgPC     = " + TQ_ArgPC);
            sb.Append("\nTQ_ArgDest   = " + TQ_ArgDest);
            sb.Append("\nTQ_ArgFile   = " + TQ_ArgFile);
            sb.Append("\nTQ_ProcID    = " + TQ_ProcID);
            sb.Append("\nTQ_EventName = " + TQ_EventName);
            sb.Append("\nTQ_MicsID    = " + TQ_MicsID);
            sb.Append("\nTQ_TimeIn    = " + TQ_TimeIn.ToString());
            sb.Append("\nTQ_TimeStart = " + TQ_TimeStart.ToString());
            sb.Append("\nTQ_TimeEnd   = " + TQ_TimeEnd.ToString());

            return sb.ToString();
        }





    }
}
