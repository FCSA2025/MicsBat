using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _NewLib;
using static _NewLib.Enums;
using static _NewLib.Enums.FeImpQual;
using static _NewLib.Enums.FeImpQualGroup;
using _Configuration;
using _Utillib;

namespace FeImport
{
    /// <summary>
    /// This class provides data and methods that encapsulate the
    /// concept of a 'qualified' line in an ES import text file; a 
    /// qualified line is one that has a recognized initial CSV field 
    /// (AK, AR, AT, AS, CK, CR, CT, GK, LK, SK, SR, ST, TD, TE and/or ZK) 
    /// and the correct number of CSV fields for its qualifier type.
    /// </summary>
    public class QualLine
    {
        private int mLineNum;
        private string mLineText;
        private FeImpQual mQualifier;
        private string[] mFields;

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

        public FeImpQual Qualifier
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
        /// This method is the default constructor, declared as 'private'
        /// so that it is inaccessible to external calls.
        /// </summary>
        private QualLine()
        {
            mLineNum = 0;
            mLineText = "";
            mQualifier = FeImpQual.UNKNOWN;
            mFields = new string[0];
        }

        /// <summary>
        /// The principal constructor.
        /// </summary>
        /// <param name="lineNum"></param>
        /// <param name="lineText"></param>
        /// <param name="qualifier"></param>
        /// <param name="fields"></param>
        public QualLine(int lineNum, string lineText, FeImpQual qualifier, string[] fields)
        {
            mLineNum = lineNum;
            mLineText = lineText;
            mQualifier = qualifier;
            mFields = fields;
        }


        /// <summary>
        /// This method returns the 'group' that 'this' QualLine object
        /// belongs to, i.e. one of { UNKNOWN, TITLE, CHANGE_OF_LOCATION, CHANGE_OF_CALLSIGN, SITE, ANTENNA, AZIMUTH, CHANNEL }.
        /// </summary>
        /// <returns>An FeImpQualGroup enumeration item.</returns>
        public FeImpQualGroup GetQualGroup()
        {
            return GetQualGroup(mQualifier);
        }

        /// <summary>
        /// This method returns the 'group' that the prescribed qualifier 
        /// belongs to, i.e. one of { UNKNOWN, TITLE, CHANGE_OF_LOCATION, CHANGE_OF_CALLSIGN, SITE, ANTENNA, AZIMUTH, CHANNEL }.
        /// </summary>
        /// <returns>An FeImpQualGroup enumeration item.</returns>
        public static FeImpQualGroup GetQualGroup(FeImpQual qualifier)
        {
            FeImpQualGroup result = TITLE;

            switch (qualifier)
            {
                case TE:
                case TD:
                    result = TITLE;
                    break;
                case LK:
                    result = CHANGE_OF_LOCATION;
                    break;
                case GK:
                    result = CHANGE_OF_CALLSIGN;
                    break;
                case SK:
                case SD:
                    result = SITE;
                    break;
                case AK:
                case AT:
                case AR:
                case AS:
                    result = ANTENNA;
                    break;
                case ZK:
                    result = AZIMUTH;
                    break;
                case CK:
                case CT:
                case CR:
                    result = CHANNEL;
                    break;
                default:
                    result = FeImpQualGroup.UNKNOWN;
                    break;
            }

            return result;
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

            for (int i = 0; i < mFields.Length; i++)
            {
                sb.Append(String.Format("\r\n          mFields[{0}] = {1}", i, mFields[i]));
            }

            return sb.ToString();
        }




    }
}
