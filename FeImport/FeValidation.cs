using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _NewLib;
using static _NewLib.Enums;
using static _NewLib.Enums.FeImpQual;
using static _NewLib.Enums.FeImpQualGroup;
using static _NewLib.Enums.FeImpMand;
using _Configuration;
using _Utillib;
using System.Text.RegularExpressions;

namespace FeImport
{
    /// <summary>
    /// This class provides utility methods for the FeImport application.
    /// </summary>
    public class FeValidation
    {
        public const int TE_NUM_FIELDS = 6;
        public const int TD_NUM_FIELDS = 2;
        public const int LK_NUM_FIELDS = 4;
        public const int GK_NUM_FIELDS = 3;
        public const int SK_NUM_FIELDS = 10;
        public const int SD_NUM_FIELDS = 10;
        public const int AK_NUM_FIELDS = 11;
        public const int AT_NUM_FIELDS = 10;
        public const int AR_NUM_FIELDS = 9;
        public const int AS_NUM_FIELDS = 7;
        public const int ZK_NUM_FIELDS = 12;
        public const int CK_NUM_FIELDS = 9;
        public const int CT_NUM_FIELDS = 11;
        public const int CR_NUM_FIELDS = 12;

        private static FeImpQual[] mTE_ValidNextQuals = new FeImpQual[] { TD };
        private static FeImpQual[] mTD_ValidNextQuals = new FeImpQual[] { LK, GK, SK, AK, ZK, CK };
        private static FeImpQual[] mLK_ValidNextQuals = new FeImpQual[] { LK, GK, SK, AK, ZK, CK };
        private static FeImpQual[] mGK_ValidNextQuals = new FeImpQual[] { LK, GK, SK, AK, ZK, CK };
        private static FeImpQual[] mSK_ValidNextQuals = new FeImpQual[] { SD };
        private static FeImpQual[] mSD_ValidNextQuals = new FeImpQual[] { AK };
        private static FeImpQual[] mAK_ValidNextQuals = new FeImpQual[] { AT };
        private static FeImpQual[] mAT_ValidNextQuals = new FeImpQual[] { AR };
        private static FeImpQual[] mAR_ValidNextQuals = new FeImpQual[] { AS };
        private static FeImpQual[] mAS_ValidNextQuals = new FeImpQual[] { AK, ZK, CK };
        private static FeImpQual[] mZK_ValidNextQuals = new FeImpQual[] { ZK, CK };
        private static FeImpQual[] mCK_ValidNextQuals = new FeImpQual[] { CT };
        private static FeImpQual[] mCT_ValidNextQuals = new FeImpQual[] { CR };
        private static FeImpQual[] mCR_ValidNextQuals = new FeImpQual[] { AK, CK, SK };

        // 1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0
        // O,O,O,O,O,O,O,O,O,O,O,O,O,O,O,O,O,O,O,O,
        private static FeImpMand[] mTE_Mand = new FeImpMand[TE_NUM_FIELDS] { M, O, O, O, O, O };
        private static FeImpMand[] mTD_Mand = new FeImpMand[TD_NUM_FIELDS] { M, O };
        private static FeImpMand[] mLK_Mand = new FeImpMand[LK_NUM_FIELDS] { M, M, M, M };
        private static FeImpMand[] mGK_Mand = new FeImpMand[GK_NUM_FIELDS] { M, M, M };
        private static FeImpMand[] mSK_Mand = new FeImpMand[SK_NUM_FIELDS] { M, M, O, M, A, A, A, O, O, O };
        private static FeImpMand[] mSD_Mand = new FeImpMand[SD_NUM_FIELDS] { M, A, A, A, A, A, A, O, O, O };
        private static FeImpMand[] mAK_Mand = new FeImpMand[AK_NUM_FIELDS] { M, M, O, M, M, O, A, O, O, O, O };

        private static FeImpMand[] mAT_Mand = new FeImpMand[AT_NUM_FIELDS] { M, A, A, A, O, A, A, A, O, O };
        private static FeImpMand[] mAR_Mand = new FeImpMand[AR_NUM_FIELDS] { M, A, A, A, O, A, A, O, O };

        private static FeImpMand[] mAS_Mand = new FeImpMand[AS_NUM_FIELDS] { M, O, A, O, A, A, A };
        private static FeImpMand[] mZK_Mand = new FeImpMand[ZK_NUM_FIELDS] { M, M, M, O, M, M, M, A, A, O, O, O };
        private static FeImpMand[] mCK_Mand = new FeImpMand[CK_NUM_FIELDS] { M, M, O, M, M, M, O, O, O };

        // The following lines for 'CT and 'CR' were used in the original version
        // of C# FeImport that was accepted by Claudia; however there is a problem
        // - see below.
        //private static FeImpMand[] mCT_Mand = new FeImpMand[CT_NUM_FIELDS] { M, A, A, A, A, A, A, A, A, O, A };
        //private static FeImpMand[] mCR_Mand = new FeImpMand[CR_NUM_FIELDS] { M, A, A, A, A, A, A, A, A, A, O, A };

        // The 'Mandatories if ADD' for the qualified line types 'CT' and 'CR'
        // are difficult to implement because, as a minimum, only one of 
        // 'CT' or 'CX' needs all of its 'A' CSVs to be populated.
        // A comprehensive validation of the 'CT' and 'CR' lines is performed in
        // FeImport so we can afford to be overly-permissive here in FeImport.
        // This 'permissive' behaviour is in line with that of the C/C++ version
        // of feImport.
        private static FeImpMand[] mCT_Mand = new FeImpMand[CT_NUM_FIELDS] { M, O, O, O, O, O, O, O, O, O, O };
        private static FeImpMand[] mCR_Mand = new FeImpMand[CR_NUM_FIELDS] { M, O, O, O, O, O, O, O, O, O, O, O };

        // enum FeImpRecGroups { TITLE, CHANGE_LOCATION, CHANGE_CALLSIGN, SITE, ANTENNA, AZIMUTH, CHANNEL }
        //private FeImpQual[][] QualGroups = new FeImpQual[8][]
        //{
        //    new FeImpQual[] { FeImpQual.UNKNOWN},   // 0: UNKNOWN
        //    new FeImpQual[] { TE, TD },             // 1: TITLE
        //    new FeImpQual[] { LK },                 // 2: CHANGE_LOCATION
        //    new FeImpQual[] { GK },                 // 3: CHANGE_CALLSIGN
        //    new FeImpQual[] { SK, SD },             // 4: SITE
        //    new FeImpQual[] { AK, AT, AR, AS },     // 5: ANTENNA
        //    new FeImpQual[] { ZK },                 // 6: AZIMUTH
        //    new FeImpQual[] { CK, CT, CR },         // 7: CHANNEL
        //};

        /// <summary>
        /// This method verifies that the contents of an ES import data file
        /// provides all of the CSV fields that are mandated by the  <a href="ES Data File - Text File Format.pdf">ES data
        /// file specification</a>.
        /// </summary>
        /// <param name="qualLines"></param>
        /// <returns>Zero if no errors are found; otherwise, negative.</returns>
        public static short CheckMandatedFields(QualLine[] qualLines)
        {
            short importOK = Constant.SUCCESS;
            string msgBuf;

            foreach (QualLine qualLine in qualLines)
            {
                FeImpMand[] mandatories = GetMandatories(qualLine.Qualifier);

                for (int i = 0; i < qualLine.Fields.Length; i++)
                {
                    string field = qualLine.Fields[i];

                    bool isMandatory = mandatories[i] == M;

                    if (String.IsNullOrWhiteSpace(field) && isMandatory)
                    {
                        importOK = Error.MANDATEDFIELDHASNOVALUE;
                        msgBuf = String.Format("Error - line #{0}: Field #{1} is mandatory; a value must be provided.\r\n",
                                                qualLine.LineNum, i + 1);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);

                        Log2.v("\n\nValidation.CheckMandatedFields(): " + msgBuf);
                    }

                } // i

            } // qualLine

            return importOK;
        }

        /// <summary>
        /// This method verifies that the contents of an ES import data file
        /// provides all of the CSV fields that are mandated by the  
        /// <a href="ES Data File - Text File Format.pdf">ES data file specification</a> 
        /// when the 'ADD' (A) operation is prescribed for qualified lines of type
        /// AK, CK, SK and/or ZK.
        /// </summary>
        /// <param name="qualLines"> - list of qualified lines.</param>
        /// <returns>Zero if no errors are found; otherwise, negative.</returns>
        public static short CheckMandatedIfAddFields(QualLine[] qualLines)
        {
            short importOK = Constant.SUCCESS;
            string msgBuf;
            bool isAddSite = false;
            bool isAddAnte = false;
            bool isAddAzim = false;
            bool isAddChan = false;

            bool hasDollarCallSign = false;

            foreach (QualLine qualLine in qualLines)
            {
                // Set the state of the 'isAddXXXX' flags, as required.
                switch (qualLine.Qualifier)
                {
                    case SK:
                        isAddSite = qualLine.Fields[1].Trim() == "A";
                        break;
                    case AK:
                        isAddAnte = qualLine.Fields[1].Trim() == "A";
                        // Remember if the current call sign starts with '$'.
                        hasDollarCallSign = Strings.FirstCharIs(qualLine.Fields[4].Trim(), '$');
                        break;
                    case ZK:
                        isAddAzim = qualLine.Fields[2].Trim() == "A";
                        break;
                    case CK:
                        isAddChan = qualLine.Fields[1].Trim() == "A";
                        break;
                }

                if (qualLine.GetQualGroup() == SITE && isAddSite
                    || qualLine.GetQualGroup() == ANTENNA && isAddAnte
                    || qualLine.GetQualGroup() == AZIMUTH && isAddAzim
                    || qualLine.GetQualGroup() == CHANNEL && isAddChan)
                {

                    // Handle the case where we have an AT or AR record in which the bndcde field is absent.
                    if ((qualLine.Qualifier == AT) && (String.IsNullOrWhiteSpace(qualLine.Fields[1]))) continue; // skip to the next qualLine.
                    if ((qualLine.Qualifier == AR) && (String.IsNullOrWhiteSpace(qualLine.Fields[1]))) continue; // skip to the next qualLine.

                    FeImpMand[] mandatories = GetMandatories(qualLine.Qualifier);

                    for (int i = 0; i < qualLine.Fields.Length; i++)
                    {
                        string field = qualLine.Fields[i];

                        bool isMandatory = mandatories[i] == A;

                        // Handle the special case for AT records.
                        if ((qualLine.Qualifier == FeImpQual.AT) && hasDollarCallSign)
                        {
                            switch (i)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 5:
                                case 6:
                                case 7:
                                    isMandatory = false;
                                    break;  
                                default:
                                    break;
                            }
                        }

                        if (String.IsNullOrWhiteSpace(field) && isMandatory)
                        {
                            importOK = Error.MANDATEDIFADDFIELDHASNOVALUE;
                            msgBuf = String.Format("Error - line #{0}: Field #{1} is mandatory for 'Add' operations; a value must be provided.\r\n",
                                                    qualLine.LineNum, i + 1);
                            ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);

                            //...Log2.v("\n\nValidation.CheckMandatedIfAddFields(): " + msgBuf);
                        }

                    } // i
                }

                // Reset the state of the 'isAddxxxx' flags as required.
                switch (qualLine.Qualifier)
                {
                    case SD:
                        isAddSite = false;
                        break;
                    case AS:
                        isAddAnte = false;
                        break;
                    case ZK:
                        isAddAzim = false;
                        break;
                    case CR:
                        isAddChan = false;
                        break;
                }

            } // qualLine

            return importOK;
        }

        /// <summary>
        /// This method returns an array that encodes whether a field is mandatory (M), 
        /// mandatory if 'ADD' (A) or optional (O) for any prescribed qualifier type.
        /// </summary>
        /// <param name="qualifier"> - prescribes the qualifier type.</param>
        /// <returns>Zero if no errors are found; otherwise, negative.</returns>
        public static FeImpMand[] GetMandatories(FeImpQual qualifier)
        {
            FeImpMand[] result = null;

            switch (qualifier)
            {
                case TE:
                    result = mTE_Mand;
                    break;
                case TD:
                    result = mTD_Mand;
                    break;
                case LK:
                    result = mLK_Mand;
                    break;
                case GK:
                    result = mGK_Mand;
                    break;
                case SK:
                    result = mSK_Mand;
                    break;
                case SD:
                    result = mSD_Mand;
                    break;
                case AK:
                    result = mAK_Mand;
                    break;
                case AT:
                    result = mAT_Mand;
                    break;
                case AR:
                    result = mAR_Mand;
                    break;
                case AS:
                    result = mAS_Mand;
                    break;
                case ZK:
                    result = mZK_Mand;
                    break;
                case CK:
                    result = mCK_Mand;
                    break;
                case CT:
                    result = mCT_Mand;
                    break;
                case CR:
                    result = mCR_Mand;
                    break;
                default:
                    result = null;
                    break;
            }

            return result;
        }

        /// <summary>
        /// This methods inputs an array of strings corresponding to lines in the ES
        /// import text file and parses them to identify, and accumulate, only 'qualified' lines
        /// , i.e. have the prefix AK, AR, AT, AS, CK, CR, CT, GK, 
        /// LK, SK, SR, ST, TD, TE and/or ZK, and that have the correct number of
        /// comma-separated-values (CSV) for that qualifier type. 
        /// See <a href="ES Data File - Text File Format.pdf">ES data file specification</a>.
        /// </summary>
        /// <param name="rawLines"> - array of lines from the ES import text file.</param>
        /// <param name="qualLines"> - output array of qualified lines.</param>
        /// <returns>Zero if no errors are found; otherwise, negative.</returns>
        public static short ParseForQualLines(string[] rawLines, out QualLine[] qualLines)
        {
            // 'out' requirement.
            qualLines = new QualLine[0];

            short importOK = Constant.FAILURE;
            string msgBuf;

            List<QualLine> qualLineList = new List<QualLine>();

            int lineNum = 0;

            foreach (string rawLine in rawLines)
            {
                lineNum++;

                string lineText = rawLine.Trim().ToUpper();

                // Skip over any blank lines.
                if (String.IsNullOrWhiteSpace(lineText))
                {
                    continue;
                }

                // Skip over any comment lines.
                if (Strings.FirstCharIs(lineText, Constant.COMMENT_CHAR))
                {
                    continue;
                }

                // If we reach here the rawLine must contain some text.
                // Split it into CSV fields.
                string[] fields = rawLine.Split(',');

                // The first CSV field is the line 'qualifier' token.
                string firstToken = fields[0];
                FeImpQual qualifier = StringToFeImpQual(firstToken);

                // Check if we have an unknown FeImport text file line qualifer.
                if (qualifier == FeImpQual.UNKNOWN)
                {
                    msgBuf = String.Format("Error - line #{0}: unknown ES record type: '{1}'\r\n", lineNum, firstToken);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    Log2.e("\n{0}", msgBuf);
                    importOK = Error.FEIMPORTFILEUNKNOWNLINEQUALIFIER;
                    //...Log2.v("\nQualLine.ParseForQualLines(): A: importOK = Constant.FAILURE");
                    return importOK;
                }

                // If we reach here we have a valid qualifier.
                // Now test that we have the correct number of fields.
                int numReqdFields = GetReqdNumFields(qualifier);
                if (fields.Length != numReqdFields)
                {
                    string shim = "                 - ";
                    msgBuf = String.Format("Error - line #{0}: incorrect number of CSV fields: \r\n{5}found {1} fields\r\n{5}{2} records must have {3} fields (i.e. {4} commas)\r\n",
                        lineNum, fields.Length, qualifier, numReqdFields, numReqdFields - 1, shim);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Error.FEIMPORTFILEQUALIFIERINCORRECTNUMBEROFFIELDS;
                    //...Log2.v("\n\nQualLine.ParseForQualLines(): B: importOK = Constant.FAILURE");
                    return importOK;
                }

                // If we reach here we have a line with a known qualifier and the 
                // correct number of CSV fields for that qualifier. 
                // Add it to the list.
                QualLine qualLine = new QualLine(lineNum, lineText, qualifier, fields);
                ////...Log2.v(qualLine.ToString());
                qualLineList.Add(qualLine);
            }

            // Check that we have actually found some qualified lines.
            if (qualLineList.Count == 0)
            {
                importOK = Error.IMPORTFILEHASNOPARSABLECONTENT;
                ErrMsg.UtPrintMessage(importOK, "");
                //...Log2.v("\n\nQualLine.ParseForQualLines(): B: importOK = Constant.FAILURE");
                return importOK;
            }

            foreach (QualLine qualLine in qualLineList)
            {
                //...Log2.v(qualLines.ToString());
            }

            // Check that the first qualified line has type 'TE'.
            QualLine firstQualLine = qualLineList.First();
            if (firstQualLine.Qualifier != TE)
            {
                msgBuf = String.Format("Error - line #{0}: the first record type must be 'TE'", firstQualLine.LineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.FEIMPORTINVALIDFIRSTQUALIFIER;
                //...Log2.v("\n\nQualLine.ParseForQualLines(): C: importOK = Constant.FAILURE");
                return importOK;
            }

            // Check that the sequence of line qualifiers is valid.
            QualLine previousQualLine = null;
            QualLine currentQualLine = null;
            bool sequenceIsOk = true;
            foreach (QualLine qualLine in qualLineList)
            {
                currentQualLine = qualLine;
                if (previousQualLine != null)
                {
                    if (!IsValidNextLine(previousQualLine, currentQualLine))
                    {
                        sequenceIsOk = false;
                        break;
                    }
                }
                previousQualLine = currentQualLine;
            }

            if (!sequenceIsOk)
            {
                msgBuf = String.Format("Error - line #{0}: invalid sequence: a line of type '{1}' cannot follow a line of type '{2}'.",
                    currentQualLine.LineNum, currentQualLine.Qualifier, previousQualLine.Qualifier);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Error.FEIMPORTFILEQUALIFIERSINVALIDSEQUENCE;
                //...Log2.v("\n\nQualLine.ParseForQualLines(): D: importOK = Constant.FAILURE");
                return importOK;
            }

            // Check that the last qualified line is a valid type.
            QualLine lastQualLine = qualLineList.Last();
            FeImpQualGroup qualGroup = lastQualLine.GetQualGroup();
            switch (qualGroup)
            {
                case SITE:
                    if (lastQualLine.Qualifier != SD)
                    {
                        msgBuf = String.Format("Error - line #{0}: incomplete Site record: missing a line of type 'SD'.\r\n",
                            lastQualLine.LineNum);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                        importOK = Error.FEIMPORTFILEINCOMPLETESITERECORD;
                        //...Log2.v("\n\nQualLine.ParseForQualLines(): E: importOK = Constant.FAILURE");
                        return importOK;
                    }
                    break;
                case ANTENNA:
                    if (lastQualLine.Qualifier != AS)
                    {
                        msgBuf = String.Format("Error - line #{0}: incomplete Antenna record: missing a line beginning with 'AS'.\r\n",
                            lastQualLine.LineNum);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                        importOK = Error.FEIMPORTFILEINCOMPLETEANTENNARECORD;
                        //...Log2.v("\n\nQualLine.ParseForQualLines(): F: importOK = Constant.FAILURE");
                        return importOK;
                    }
                    break;
                case CHANNEL:
                    if (lastQualLine.Qualifier != CR)
                    {
                        msgBuf = String.Format("Error - line #{0}: incomplete Channel record: missing a line of type 'CR'.\r\n",
                            lastQualLine.LineNum);
                        ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                        importOK = Error.FEIMPORTFILEINCOMPLETECHANNELRECORD;
                        //...Log2.v("\n\nQualLine.ParseForQualLines(): G: importOK = Constant.FAILURE");
                        return importOK;
                    }
                    break;
            }

            // Convert the list to an array that is provided as output.
            qualLines = qualLineList.ToArray();

            importOK = Constant.SUCCESS;
            return importOK;
        }

        /// <summary>
        /// This method converts a string representation of a line qualifier
        /// to its FeImpQual enumeration value, e.g. input of "TE" gives and 
        /// output of FeImpQual.TE
        /// </summary>
        /// <param name="qualifierString"> - string representation of a qualifier, e.g. "TE".</param>
        /// <returns>FeImpQual enumeration value.</returns>
        public static FeImpQual StringToFeImpQual(string qualifierString)
        {
            if (String.IsNullOrWhiteSpace(qualifierString))
            {
                return FeImpQual.UNKNOWN;
            }

            FeImpQual result;

            switch (qualifierString.Trim())
            {
                case "TE":
                    result = TE;
                    break;
                case "TD":
                    result = TD;
                    break;
                case "LK":
                    result = LK;
                    break;
                case "GK":
                    result = GK;
                    break;
                case "SK":
                    result = SK;
                    break;
                case "SD":
                    result = SD;
                    break;
                case "AK":
                    result = AK;
                    break;
                case "AT":
                    result = AT;
                    break;
                case "AR":
                    result = AR;
                    break;
                case "AS":
                    result = AS;
                    break;
                case "ZK":
                    result = ZK;
                    break;
                case "CK":
                    result = CK;
                    break;
                case "CT":
                    result = CT;
                    break;
                case "CR":
                    result = CR;
                    break;
                default:
                    result = FeImpQual.UNKNOWN;
                    break;
            }

            return result;
        }

        /// <summary>
        /// This method returns the required number of comma-separated-values
        /// (CSV) for a prescribed line qualifier type i.a.w. the <a href="ES Data File - Text File Format.pdf">ES data file specification</a>. 
        /// </summary>
        /// <param name="feImpQual"> - prescribed qualifier type.</param>
        /// <returns>The number of required CSV fields.</returns>
        public static int GetReqdNumFields(FeImpQual feImpQual)
        {
            if (feImpQual == FeImpQual.UNKNOWN)
            {
                return Constant.FAILURE;
            }

            int result = Constant.FAILURE;

            switch (feImpQual)
            {
                case TE:
                    result = TE_NUM_FIELDS;
                    break;
                case TD:
                    result = TD_NUM_FIELDS;
                    break;
                case LK:
                    result = LK_NUM_FIELDS;
                    break;
                case GK:
                    result = GK_NUM_FIELDS;
                    break;
                case SK:
                    result = SK_NUM_FIELDS;
                    break;
                case SD:
                    result = SD_NUM_FIELDS;
                    break;
                case AK:
                    result = AK_NUM_FIELDS;
                    break;
                case AT:
                    result = AT_NUM_FIELDS;
                    break;
                case AR:
                    result = AR_NUM_FIELDS;
                    break;
                case AS:
                    result = AS_NUM_FIELDS;
                    break;
                case ZK:
                    result = ZK_NUM_FIELDS;
                    break;
                case CK:
                    result = CK_NUM_FIELDS;
                    break;
                case CT:
                    result = CT_NUM_FIELDS;
                    break;
                case CR:
                    result = CR_NUM_FIELDS;
                    break;
            }

            return result;
        }

        /// <summary>
        /// This method determines whether a consecutive pair of qualified
        /// lines occur in a valid sequence, or not, as defined in the 
        /// <a href="ES Data File - Text File Format.pdf">ES data file specification</a>.
        /// </summary>
        /// <param name="thisLine"> - the current qualified line.</param>
        /// <param name="candidateNextLine"> - the next qualified line.</param>
        /// <returns>True or false.</returns>
        public static bool IsValidNextLine(QualLine thisLine, QualLine candidateNextLine)
        {
            FeImpQual[] validNextQuals = ValidNextQuals(thisLine);

            bool isValid = false;
            foreach (FeImpQual validQual in validNextQuals)
            {
                if (candidateNextLine.Qualifier == validQual)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        /// <summary>
        /// This method returns an array of line qualifiers corresponding to 
        /// valid 'next in sequence' qualified lines for any prescribed
        /// current qualified line type.
        /// </summary>
        /// <param name="qualLine"> - qualified line type.</param>
        /// <returns>Array of valid line qualifiers.</returns>
        public static FeImpQual[] ValidNextQuals(QualLine qualLine)
        {
            FeImpQual[] result = null;

            switch (qualLine.Qualifier)
            {
                case TE:
                    result = mTE_ValidNextQuals;
                    break;
                case TD:
                    result = mTD_ValidNextQuals;
                    break;
                case LK:
                    result = mLK_ValidNextQuals;
                    break;
                case GK:
                    result = mGK_ValidNextQuals;
                    break;
                case SK:
                    result = mSK_ValidNextQuals;
                    break;
                case SD:
                    result = mSD_ValidNextQuals;
                    break;
                case AK:
                    result = mAK_ValidNextQuals;
                    break;
                case AT:
                    result = mAT_ValidNextQuals;
                    break;
                case AR:
                    result = mAR_ValidNextQuals;
                    break;
                case AS:
                    result = mAS_ValidNextQuals;
                    break;
                case ZK:
                    result = mZK_ValidNextQuals;
                    break;
                case CK:
                    result = mCK_ValidNextQuals;
                    break;
                case CT:
                    result = mCT_ValidNextQuals;
                    break;
                case CR:
                    result = mCR_ValidNextQuals;
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to be a string with a prescribed maximum length.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="token"> - output string.</param>
        /// <param name="maxStringLength"> - prescribed maximum string length.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsString(QualLine qualLine, int fieldNum, out string token, int maxStringLength)
        {
            // 'out' requirement.
            token = "";

            int size;
            int result = -666;
            string field = qualLine.Fields[fieldNum];

            /* if the end of string is encountered */
            if (field == null)
            {
                /* end of string */
                result = Constant.UT_EOLN;
            }
            else
            {
                token = field.ToUpper().Trim();

                /* if length greater than requested length */
                size = token.Length;
                if (size <= maxStringLength)
                {
                    result = size;
                }
                else
                {
                    /* too long */
                    //...Log2.v("\nValidation.ParseFieldAsString(): too long");
                    result = Constant.UT_INV_CONV;
                }
            }

            string str = String.Format("\r\nValidation.ParseFieldAsString(): {0,2}  {1}  {2}", fieldNum, Strings.AddBars(token), result);
            //...Log2.v(str);
            return result;
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a date that has a prescribed maximum length; valid date 
        /// formats are:  yyyy.mm.dd  and/or  dd-mmm-yyyy where (mmm = jan, feb, mar ...).
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="dateString"> - string encoding the data, converted to yyyy.mm.dd .</param>
        /// <param name="maxStringLength"> - prescribed maximum string length.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDate(QualLine qualLine, int fieldNum, out string dateString, int maxStringLength)
        {
            // 'out' requirement.
            dateString = "";

            string field;
            int size;

            /* get next available token */
            field = qualLine.Fields[fieldNum];

            /* if the end of string is encountered */
            if (field == null)
            {
                /* end of string */
                return (Error.BADDATE);
            }

            /* remove leading and trailing blanks */
            field = field.Trim().ToUpper();

            /* if length greater than requested length */
            size = field.Length;
            if (size == 0)
            {
                return Constant.SUCCESS;   //	No field, no problem.
            }
            else if (size > maxStringLength)
            {
                /* too long */
                return (Error.BADDATE);
            }
            else
            {
                // Valid formats are (yyyy.mm.dd) or (dd-mmm-yyyy).
                const string A = "[a-zA-Z]";
                const string N = "[0-9]";

                // Regex pattern for yyyy.mm.dd
                string pattern_yyyyOmmOdd = N + N + N + N + @"\." + N + N + @"\." + N + N;
                // Check for dd-mmm-yyyy
                string pattern_dd_mmm_yyyy = N + N + "-" + A + A + A + "-" + N + N + N + N;

                // Get the Regex matches.
                Match match_yyyyOmmOdd = Regex.Match(field, pattern_yyyyOmmOdd);
                Match match_dd_mmm_yyyy = Regex.Match(field, pattern_dd_mmm_yyyy);

                // Act on the match success flags.
                if (match_yyyyOmmOdd.Success)
                {
                    // We're good with this; it is the preferred 'internal' format.
                }
                else if (match_dd_mmm_yyyy.Success)
                {
                    // Convert the date field to the preferred 'internal' format.
                    if (!GenUtil.UtConvertDate(ref field))
                    {
                        return Error.BADDATE;
                    }
                }
                else // neither date patterns match the field.
                {
                    return Error.BADDATE;
                }

                dateString = field;

                return field.Length;
            }

        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a float that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="fNumber"> - string encoding a floating-point number</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsFloatRound(QualLine qualLine, int fieldNum, out float fNumber, int nDecPlaces)
        {
            // 'out' requirement.
            fNumber = 0.0f;

            string istring;
            float fInval;
            double dShift;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to float */
            try
            {
                fInval = Convert.ToSingle(istring);
            }
            catch
            {
                /* invalid conversion */
                //...Log2.e("\nGenUtil.UtGetInputFloatRound(): ERROR: invalid string for conversion to float: " + istring);
                return (Error.INVALIDNAMEORVALUE);
            }

            /*	We round the value to the specified number of decimal places.  This is
            *	so that values will have the same value when exported and imported again. */
            dShift = Pow(10.0, (double)nDecPlaces);

            fNumber = fInval * (float)dShift;

            if (fNumber < 0.0)
            {
                fNumber = (float)Ceiling(fNumber - 0.5);
            }
            else
            {
                fNumber = (float)Floor(fNumber + 0.5);
            }
            fNumber = (float)((double)fNumber / dShift);

            /*	Check if rounding did occur and indicate it to the user */
            if (Abs(fNumber - fInval) > 0.1 / dShift)
            {
                return (-3);        /*	Rounding did occur */
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a short integer.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="number"> - string encoding a floating-point number</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsShort(QualLine qualLine, int fieldNum, out short number)
        {
            // 'out' requirement.
            number = 0;

            string istring;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert to short */
            try
            {
                number = Convert.ToInt16(istring);
            }
            catch
            {
                /* invalid conversion */
                //...Log2.e("\nGenUtil.UtGetInputSNum(): ERROR: invalid string for conversion to short: " + istring);
                return (Error.INVALIDNAMEORVALUE);
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode an integer value.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="number"> - string encoding a floating-point number</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsInt(QualLine qualLine, int fieldNum, out int number)
        {
            // 'out' requirement.
            number = 0;

            string istring;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to int */
            try
            {
                number = Convert.ToInt32(istring);
            }
            catch
            {
                /* invalid conversion */
                //...Log2.e("\nGenUtil.UtGetInputLNum(): ERROR: invalid conversion: string to int: " + istring);
                return (Error.INVALIDNAMEORVALUE);
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method parses, validates and returns a prescribed CSV field that is
        /// expected to encode a double that has a prescribed number of decimal places.
        /// </summary>
        /// <param name="qualLine"> - prescribed qualified line.</param>
        /// <param name="fieldNum"> - prescribed CSV field number (starting at zero).</param>
        /// <param name="dNumber"> - string encoding a double-precision floating-point number</param>
        /// <param name="nDecPlaces"> - prescribed maximum number of decimal places.</param>
        /// <returns>The actual length of the string returned or negative if an error occurred.</returns>
        public static int ParseFieldAsDoubleRound(QualLine qualLine, int fieldNum, out double dNumber, int nDecPlaces)
        {
            // 'out' requirement.
            dNumber = 0.0;

            string istring;
            double fInval;
            double dShift;

            /* get next token */
            istring = qualLine.Fields[fieldNum];

            if (istring == null)
            {
                /* no more tokens */
                return (-1);
            }

            istring = istring.Trim();

            if (istring == "")
            {
                return (0); /* string length of zero, value is 0 */
            }

            /* convert string to double */
            try
            {
                fInval = Convert.ToDouble(istring);
            }
            catch
            {
                /* invalid conversion */
                //...Log2.e("\nGenUtil.UtGetInputDoubleRound(): ERROR: invalid string for conversion to double: " + istring);
                return (Error.INVALIDNAMEORVALUE);
            }

            /*	We round the value to the specified number of decimal places.  This is
            *	so that values will have the same value when exported and imported again. */
            dShift = Pow(10.0, nDecPlaces);

            dNumber = fInval * dShift;

            if (dNumber < 0.0)
            {
                dNumber = Ceiling(dNumber - 0.5);
            }
            else
            {
                dNumber = Floor(dNumber + 0.5);
            }
            dNumber = dNumber / dShift;

            /*	Check if rounding did occur and indicate it to the user */
            if (Abs(dNumber - fInval) > 0.1 / dShift)
            {
                return (-3);        /*	Rounding did occur */
            }

            return ((int)istring.Length);
        }

        /// <summary>
        /// This method determines whether a prescribed string has a 
        /// valid time format hh:mm  ; no numerical bounds checking
        /// is performed.
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public static bool IsValidTime(string candidate)
        {
            bool result = false;

            if (!String.IsNullOrWhiteSpace(candidate))
            {
                string pattern = @"\d\d:\d\d";
                Match match = Regex.Match(candidate, pattern);
                result = match.Success;
            }

            return result;
        }





    }
}
