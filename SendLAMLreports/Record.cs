using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SendLAMLreports
{
    
    /// <summary>
    /// This class encapsulates the concept of a 'user-record' that prescribes the email address
    /// salutation (first line of email) and the report file to be attached; email addresses and
    /// salutations appear in the record as TextBoxes rather than just strings - this greatly
    /// simplifies the handling of GUI events.
    /// </summary>
    public class Record
    {
        private string mMember;
        private CheckBox mSendEmailYesNo;
        private TextBox mEmailAddress;
        private TextBox mFirstLine;
        private DateTime mDateTimeLastSent;
        private string mPathToReportLastSent;

        public string Member { get => mMember; set => mMember = value; }
        public CheckBox SendEmailYesNo { get => mSendEmailYesNo; set => mSendEmailYesNo = value; }
        public TextBox EmailAddress { get => mEmailAddress; set => mEmailAddress = value; }
        public TextBox FirstLine { get => mFirstLine; set => mFirstLine = value; }
        public DateTime DateTimeLastSent { get => mDateTimeLastSent; set => mDateTimeLastSent = value; }
        public string PathToReportLastSent { get => mPathToReportLastSent; set => mPathToReportLastSent = value; }

        /// <summary>
        /// Makes the default constructor out-of-reach.
        /// </summary>
        private Record()
        {
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="sendEmailYesNo"></param>
        /// <param name="emailAddress"></param>
        /// <param name="firstLine"></param>
        /// <param name="dateTimeLastSent"></param>
        /// <param name="pathToReportLastSent"></param>
        public Record(string member, CheckBox sendEmailYesNo, TextBox emailAddress, TextBox firstLine, DateTime dateTimeLastSent, string pathToReportLastSent)
        {
            mMember = member;
            mSendEmailYesNo = sendEmailYesNo;
            mEmailAddress = emailAddress;
            mFirstLine = firstLine;
            mDateTimeLastSent = dateTimeLastSent;
            PathToReportLastSent = pathToReportLastSent;
        }

        /// <summary>
        /// This method returns a CSV string that provides the current values of
        /// this object's member variables.
        /// </summary>
        /// <returns></returns>
        public string RecordToStringAsCSV()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("\"{0}\" ,", mMember.Trim()));
            sb.Append(String.Format("\"{0}\" ,", mSendEmailYesNo.Checked));
            sb.Append(String.Format("\"{0}\" ,", mEmailAddress.Text.Trim()));
            sb.Append(String.Format("\"{0}\" ,", mFirstLine.Text.Trim()));
            sb.Append(String.Format("\"{0}\" ,", mDateTimeLastSent.ToString("s").Trim()));
            sb.Append(String.Format("\"{0}\"", mPathToReportLastSent.Trim()));

            return sb.ToString();
        }



    }
}
