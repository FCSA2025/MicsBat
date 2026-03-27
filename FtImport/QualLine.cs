using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _NewLib;
using static _NewLib.Enums;
using static _NewLib.Enums.FtImpQual;
using static _NewLib.Enums.FtImpQualGroup;
using _Configuration;
using _Utillib;

namespace FtImport
{
    /// <summary>
    /// This class provides data and methods that encapsulate the
    /// concept of a 'qualified' line in an TS import text file; a 
    /// qualified line is one that has a recognized initial CSV field 
    /// (AK, AQ, AO, CK, CT, CR, CQ, CO, GK, SK, SD, TD, TE and/or TT) 
    /// and the correct number of CSV fields for its qualifier type.
    /// </summary>
    public class QualLine
    {
        private int mLineNum;
        private string mLineText;
        private FtImpQual mQualifier;
        private string[] mFields;

        /// <summary>
        /// This is the default constructor for the FtImport class
        /// and is declared as 'private' to prevent it from being called
        /// external to the class.
        /// </summary>
        private QualLine()
        {
            mLineNum = 0;
            mLineText = "";
            mQualifier = FtImpQual.UNKNOWN;
            mFields = new string[0];
        }

        public int LineNum
        {
            get { return mLineNum; }
            set { mLineNum = value; }
        }

        public string LineText
        {
            get { return mLineText; }
            set { mLineText = value; }
        }

        public FtImpQual Qualifier
        {
            get { return mQualifier; }
            set { mQualifier = value; }
        }

        public string[] Fields
        {
            get { return mFields; }
            set { mFields = value; }
        }


        /// <summary>
        /// The principal constructor.
        /// </summary>
        /// <param name="lineNum"></param>
        /// <param name="lineText"></param>
        /// <param name="qualifier"></param>
        /// <param name="fields"></param>
        public QualLine(int lineNum, string lineText, FtImpQual qualifier, string[] fields)
        {
            mLineNum = lineNum;
            mLineText = lineText;
            mQualifier = qualifier;
            mFields = fields;
        }

        /// <summary>
        /// This method returns a single annotated, line-formatted string that
        /// presents the member values of 'this' QualLine object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\r\n");
            sb.Append("\r\nmLineNum = " + mLineNum);
            sb.Append("\r\nmLineText = " + mLineText);
            sb.Append("\r\nmQualifier = " + mQualifier);
            sb.Append("\r\nmFields:");

            if (mFields == null)
            {
                sb.Append(" <null>");
            }
            else if (mFields.Length == 0)
            {
                sb.Append(" <zero-length array>");
            }
            else
            {
                for (int i = 0; i < mFields.Length; i++)
                {
                    sb.Append(String.Format("\r\n          mFields[{0}] = {1}", i, mFields[i]));
                }
            }

            return sb.ToString();
        }




    }
}
