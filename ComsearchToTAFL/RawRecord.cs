using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComsearchToTAFL
{
    using _Configuration;
    using _NewLib;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
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
    /// This class encapsulates the data fields of a Comsearch raw record and the methods
    /// required to read them from, and write them to, a CSV text file; a method is 
    /// provided that retains only those raw records within a prescribed distance of the
    /// CAN-USA border.
    /// </summary>
    public class RawRecord
    {
        // Member definitions.
        public int pathID = 0;              // column A
        public string regID = "";           // column B
        public string pathStat = "";        // column C
        public string regdate = "";         // column D
        public string conDate = "";         // column E
        public string site1 = "";           // column F
        public string state1 = "";          // column G
        public string location1 = "";       // column H
        public string site2 = "";           // column I
        public string state2 = "";          // column J
        public string location2 = "";       // column K
        public string callsign = "";        // column L
        public string compName = "";        // column M
        public double lat1 = 0;             // column N
        public double lng1 = 0;             // column O
        public double lat2 = 0;             // column P
        public double lng2 = 0;             // column Q
        public double pathLen = 0;          // column R
        public double grndElev1 = 0;        // column S
        public double grndElev2 = 0;        // column T
        public string antMfr1 = "";         // column U
        public string antMod1 = "";         // column V
        public double gain1 = 0;            // column W
        public double bw1 = 0;              // column X
        public double rcagl1 = 0;           // column Y (Radiation Centre Above Ground Level)
        public string antMfr2 = "";         // column Z
        public string antMod2 = "";         // column AA
        public double gain2 = 0;            // column AB
        public double bw2 = 0;              // column AC
        public double rcagl2 = 0;           // column AD (Radiation Centre Above Ground Level)
        public string eqptMfr1 = "";        // column AE
        public string eqptMod1 = "";        // column AF
        public string eqptModDesc1 = "";    // column AG
        public string emDes1 = "";          // column AH (Emission Designator)
        public string mod1 = "";            // column AI (Modulation)
        public string acmMinMod1 = "";      // column AJ (Adaptive Coding and Modulation)
        public string acmMaxMod1 = "";      // column AK (Adaptive Coding and Modulation)
        public double dataRate1 = 0;        // column AL
        public string eqptMfr2 = "";        // column AM
        public string eqptMod2 = "";        // column AN
        public string eqptModDesc2 = "";    // column AO
        public string emDes2 = "";          // column AP (Emission Designator)
        public string mod2 = "";            // column AQ (Modulation)
        public string acmMinMod2 = "";      // column AR (Adaptive Coding and Modulation)
        public string acmMaxMod2 = "";      // column AS (Adaptive Coding and Modulation)
        public double dataRate2 = 0;        // column AT
        public double atpcNomPwr1 = 0;      // column AU (Automatic Transmit Power Control) Units: dBm
        public double pwr1 = 0;             // column AV                                    Units: dBm
        public double atpcMaxPwr1 = 0;      // column AW                                    Units: dBm
        public double acmMinModPwr1 = 0;    // column AX                                    Units: dBm
        public double acmMaxModPwr1 = 0;    // column AY                                    Units: dBm
        public double atpcNomPwr2 = 0;      // column AZ (Automatic Transmit Power Control) Units: dBm
        public double pwr2 = 0;             // column BA                                    Units: dBm
        public double atpcMaxPwr2 = 0;      // column BB                                    Units: dBm
        public double acmMinModPwr2 = 0;    // column BC                                   Units: dBm
        public double acmMaxModPwr2 = 0;    // column BD                                    Units: dBm
        public double txLoss1 = 0;          // column BE
        public double rxLoss1 = 0;          // column BF
        public double cmnLoss1 = 0;         // column BG
        public double txLoss2 = 0;          // column BH
        public double rxLoss2 = 0;          // column BI
        public double cmnLoss2 = 0;         // column BJ
        public double atpcNomRSL1 = 0;      // column BK
        public double selectRSL1 = 0;       // column BL
        public double atpcMaxRSL1 = 0;      // column BM
        public double acmMinModRSL1 = 0;    // column BN (Received Signal level)
        public double acmMaxModRSL1 = 0;    // column BO
        public double atpcNomRSL2 = 0;      // column BP
        public double selectRSL2 = 0;       // column BQ
        public double atpcMaxRSL2 = 0;      // column BR
        public double acmMinModRSL2 = 0;    // column BS
        public double acmMaxModRSL2 = 0;    // column BT
        public double centerFreq1 = 0;      // column BU    Units: MHz
        public string polar1 = "";          // column BV
        public double centerFreq2 = 0;      // column BW    Units: MHz
        public string polar2 = "";          // column BX

        // We will also append this non-Comsearch field to give each raw record a unique integer ID..
        public int uniqueID = 0;

        // The total number of fields corresponding to Comsearch rawRecord + appended uniqueID.
        public const int NUM_COLUMNS = 77;

        // Keep null indicators for each record object.
        public SQLLEN[] nullInds = new SQLLEN[NUM_COLUMNS];

        // Array of strings providing the class-member / database-column names.
        public static string[] columnNames = new string[NUM_COLUMNS] { "pathID", "regID", "pathStat", "regdate", "conDate", "site1", "state1", "location1", "site2", "state2", "location2", "callsign", "compName", "lat1", "lng1", "lat2", "lng2", "pathLen", "grndElev1", "grndElev2", "antMfr1", "antMod1", "gain1", "bw1", "rcagl1", "antMfr2", "antMod2", "gain2", "bw2", "rcagl2", "eqptMfr1", "eqptMod1", "eqptModDesc1", "emDes1", "mod1", "acmMinMod1", "acmMaxMod1", "dataRate1", "eqptMfr2", "eqptMod2", "eqptModDesc2", "emDes2", "mod2", "acmMinMod2", "acmMaxMod2", "dataRate2", "atpcNomPwr1", "pwr1", "atpcMaxPwr1", "acmMinModPwr1", "acmMaxModPwr1", "atpcNomPwr2", "pwr2", "atpcMaxPwr2", "acmMinModPwr2", "acmMaxModPwr2", "txLoss1", "rxLoss1", "cmnLoss1", "txLoss2", "rxLoss2", "cmnLoss2", "atpcNomRSL1", "selectRSL1", "atpcMaxRSL1", "acmMinModRSL1", "acmMaxModRSL1", "atpcNomRSL2", "selectRSL2", "atpcMaxRSL2", "acmMinModRSL2", "acmMaxModRSL2", "centerFreq1", "polar1", "centerFreq2", "polar2", "uniqueID" };

        // Comsearch original column names.
        public static string ComsearchColumnNamesAsCSV = "Path ID,Registration ID, Path Status,Registration Date, Construction Date,Site 1,State 1,Location 1,Site 2,State 2,Location 2,Call Sign, Company Name,Latitude Site 1, Longitude Site 1,Latitude Site 2, Longitude Site 2,Path Length, Ground Elevation Site 1, Ground Elevation Site 2, Antenna Manufacturer 1,Antenna Model 1, Gain 1, Beamwidth 1, RCAGL Site 1,Antenna Manufacturer 2, Antenna Model 2,Gain 2,Beamwidth 2,RCAGL Site 2, Equipment Manufacturer 1,Equipment Model 1, Equipment Model Description 1, Emission Designator 1,Modulation 1,ACM Min Modulation 1,ACM Max Modulation 1,Data Rate 1, Equipment Manufacturer 2,Equipment Model 2, Equipment Model Description 2, Emission Designator 2,Modulation 2,ACM Min Modulation 2,ACM Max Modulation 2,Data Rate 2, ATPC Nom Power 1, Power Site 1,ATPC Max Power 1,ACM Min Mod Power 1, ACM Max Mod Power 1,ATPC Nom Power 2,Power Site 2, ATPC Max Power 2, ACM Min Mod Power 2,ACM Max Mod Power 2, Tx Loss 1,Rx Loss 1, Common Loss 1,Tx Loss 2, Rx Loss 2,Common Loss 2, ATPC Nom RSL 1, Selected RSL Site 1, ATPC Max RSL 1, ACM Min Mod RSL 1,ACM Max Mod RSL 1, ATPC Nom RSL 2, Selected RSL Site 2, ATPC Max RSL 2, ACM Min Mod RSL 2,ACM Max Mod RSL 2, Center Freq Site 1, Polarization Site 1,Center Freq Site 2,Polarization Site 2";

        // Zero-indexed sequence number.
        public const int PATHID = 0;
        public const int REGID = 1;
        public const int PATHSTAT = 2;
        public const int REGDATE = 3;
        public const int CONDATE = 4;
        public const int SITE1 = 5;
        public const int STATE1 = 6;
        public const int LOCATION1 = 7;
        public const int SITE2 = 8;
        public const int STATE2 = 9;
        public const int LOCATION2 = 10;
        public const int CALLSIGN = 11;
        public const int COMPNAME = 12;
        public const int LAT1 = 13;
        public const int LNG1 = 14;
        public const int LAT2 = 15;
        public const int LNG2 = 16;
        public const int PATHLEN = 17;
        public const int GRNDELEV1 = 18;
        public const int GRNDELEV2 = 19;
        public const int ANTMFR1 = 20;
        public const int ANTMOD1 = 21;
        public const int GAIN1 = 22;
        public const int BW1 = 23;
        public const int RCAGL1 = 24;
        public const int ANTMFR2 = 25;
        public const int ANTMOD2 = 26;
        public const int GAIN2 = 27;
        public const int BW2 = 28;
        public const int RCAGL2 = 29;
        public const int EQPTMFR1 = 30;
        public const int EQPTMOD1 = 31;
        public const int EQPTMODDESC1 = 32;
        public const int EMDES1 = 33;
        public const int MOD1 = 34;
        public const int ACMMINMOD1 = 35;
        public const int ACMMAXMOD1 = 36;
        public const int DATARATE1 = 37;
        public const int EQPTMFR2 = 38;
        public const int EQPTMOD2 = 39;
        public const int EQPTMODDESC2 = 40;
        public const int EMDES2 = 41;
        public const int MOD2 = 42;
        public const int ACMMINMOD2 = 43;
        public const int ACMMAXMOD2 = 44;
        public const int DATARATE2 = 45;
        public const int ATPCNOMPWR1 = 46;
        public const int PWR1 = 47;
        public const int ATPCMAXPWR1 = 48;
        public const int ACMMINMODPWR1 = 49;
        public const int ACMMAXMODPWR1 = 50;
        public const int ATPCNOMPWR2 = 51;
        public const int PWR2 = 52;
        public const int ATPCMAXPWR2 = 53;
        public const int ACMMINMODPWR2 = 54;
        public const int ACMMAXMODPWR2 = 55;
        public const int TXLOSS1 = 56;
        public const int RXLOSS1 = 57;
        public const int CMNLOSS1 = 58;
        public const int TXLOSS2 = 59;
        public const int RXLOSS2 = 60;
        public const int CMNLOSS2 = 61;
        public const int ATPCNOMRSL1 = 62;
        public const int SELECTRSL1 = 63;
        public const int ATPCMAXRSL1 = 64;
        public const int ACMMINMODRSL1 = 65;
        public const int ACMMAXMODRSL1 = 66;
        public const int ATPCNOMRSL2 = 67;
        public const int SELECTRSL2 = 68;
        public const int ATPCMAXRSL2 = 69;
        public const int ACMMINMODRSL2 = 70;
        public const int ACMMAXMODRSL2 = 71;
        public const int CENTERFREQ1 = 72;
        public const int POLAR1 = 73;
        public const int CENTERFREQ2 = 74;
        public const int POLAR2 = 75;
        public const int UNIQUEID = 76;

        // SQL text string column maximum lengths (include the terminating null character).
        public const int REGID_SZ = 16;
        public const int PATHSTAT_SZ = 41;
        public const int REGDATE_SZ = 11;
        public const int CONDATE_SZ = 11;
        public const int SITE1_SZ = 51;
        public const int STATE1_SZ = 3;
        public const int LOCATION1_SZ = 101;
        public const int SITE2_SZ = 51;
        public const int STATE2_SZ = 3;
        public const int LOCATION2_SZ = 101;
        public const int CALLSIGN_SZ = 8;
        public const int COMPNAME_SZ = 101;
        public const int ANTMFR1_SZ = 101;
        public const int ANTMOD1_SZ = 51;
        public const int ANTMFR2_SZ = 101;
        public const int ANTMOD2_SZ = 51;
        public const int EQPTMFR1_SZ = 101;
        public const int EQPTMOD1_SZ = 51;
        public const int EQPTMODDESC1_SZ = 101;
        public const int EMDES1_SZ = 8;
        public const int MOD1_SZ = 11;
        public const int ACMMINMOD1_SZ = 11;
        public const int ACMMAXMOD1_SZ = 11;
        public const int EQPTMFR2_SZ = 101;
        public const int EQPTMOD2_SZ = 51;
        public const int EQPTMODDESC2_SZ = 101;
        public const int EMDES2_SZ = 8;
        public const int MOD2_SZ = 11;
        public const int ACMMINMOD2_SZ = 11;
        public const int ACMMAXMOD2_SZ = 11;
        public const int POLAR1_SZ = 2;
        public const int POLAR2_SZ = 2;

        /// The default constructor is private.
        private RawRecord() { }

        /// <summary>
        /// A public constructor.
        /// </summary>
        /// <param name="uniqueID"></param>
        public RawRecord(int uniqueID)
        {
            // Set all the nullInds to NULL.
            for (int i = 0; i < NUM_COLUMNS; i++) { nullInds[i] = Constant.DB_NULL; }

            // Set the uniqueID to the value prescribed.
            this.uniqueID = uniqueID;
            nullInds[UNIQUEID] = Constant.DB_NOT_NULL;
        }


        /// <summary>
        /// A public constructor.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="uniqueID"></param>
        public RawRecord(string[] fields, int uniqueID)
        {
            this.uniqueID = uniqueID;

            // Check string array for correct dimension.
            if (fields == null)
            {
                Console.Write("\nERROR: RawRecord() constructor: uniqueID = {0}: fields[] is NULL", uniqueID);
                Environment.Exit(2);
            }
            else if (fields.Length != NUM_COLUMNS - 1)
            {
                Console.Write("\nERROR: RawRecord() constructor: uniqueID = {0}: numFields = {1} should be {2}", uniqueID, fields.Length, NUM_COLUMNS - 1);
                Environment.Exit(3);
            }

            // Set the nullInd for the added field 'uniqueID'.
            nullInds[UNIQUEID] = Constant.DB_NOT_NULL;

            try
            {
                CSV.ParseFieldAsInt(fields, columnNames, PATHID, int.MinValue, int.MaxValue, out pathID, out nullInds[PATHID]);
                CSV.ParseFieldAsString(fields, columnNames, REGID, out regID, out nullInds[REGID]);
                CSV.ParseFieldAsString(fields, columnNames, PATHSTAT, out pathStat, out nullInds[PATHSTAT]);
                CSV.ParseFieldAsString(fields, columnNames, REGDATE, out regdate, out nullInds[REGDATE]);
                CSV.ParseFieldAsString(fields, columnNames, CONDATE, out conDate, out nullInds[CONDATE]);
                CSV.ParseFieldAsString(fields, columnNames, SITE1, out site1, out nullInds[SITE1]);
                CSV.ParseFieldAsString(fields, columnNames, STATE1, out state1, out nullInds[STATE1]);
                CSV.ParseFieldAsString(fields, columnNames, LOCATION1, out location1, out nullInds[LOCATION1]);
                CSV.ParseFieldAsString(fields, columnNames, SITE2, out site2, out nullInds[SITE2]);
                CSV.ParseFieldAsString(fields, columnNames, STATE2, out state2, out nullInds[STATE2]);
                CSV.ParseFieldAsString(fields, columnNames, LOCATION2, out location2, out nullInds[LOCATION2]);
                CSV.ParseFieldAsString(fields, columnNames, CALLSIGN, out callsign, out nullInds[CALLSIGN]);
                CSV.ParseFieldAsString(fields, columnNames, COMPNAME, out compName, out nullInds[COMPNAME]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, LAT1, -1, out lat1, out nullInds[LAT1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, LNG1, -1, out lng1, out nullInds[LNG1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, LAT2, -1, out lat2, out nullInds[LAT2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, LNG2, -1, out lng2, out nullInds[LNG2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, PATHLEN, -1, out pathLen, out nullInds[PATHLEN]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, GRNDELEV1, -1, out grndElev1, out nullInds[GRNDELEV1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, GRNDELEV2, -1, out grndElev2, out nullInds[GRNDELEV2]);
                CSV.ParseFieldAsString(fields, columnNames, ANTMFR1, out antMfr1, out nullInds[ANTMFR1]);
                CSV.ParseFieldAsString(fields, columnNames, ANTMOD1, out antMod1, out nullInds[ANTMOD1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, GAIN1, -1, out gain1, out nullInds[GAIN1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, BW1, -1, out bw1, out nullInds[BW1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, RCAGL1, -1, out rcagl1, out nullInds[RCAGL1]);
                CSV.ParseFieldAsString(fields, columnNames, ANTMFR2, out antMfr2, out nullInds[ANTMFR2]);
                CSV.ParseFieldAsString(fields, columnNames, ANTMOD2, out antMod2, out nullInds[ANTMOD2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, GAIN2, -1, out gain2, out nullInds[GAIN2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, BW2, -1, out bw2, out nullInds[BW2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, RCAGL2, -1, out rcagl2, out nullInds[RCAGL2]);
                CSV.ParseFieldAsString(fields, columnNames, EQPTMFR1, out eqptMfr1, out nullInds[EQPTMFR1]);
                CSV.ParseFieldAsString(fields, columnNames, EQPTMOD1, out eqptMod1, out nullInds[EQPTMOD1]);
                CSV.ParseFieldAsString(fields, columnNames, EQPTMODDESC1, out eqptModDesc1, out nullInds[EQPTMODDESC1]);
                CSV.ParseFieldAsString(fields, columnNames, EMDES1, out emDes1, out nullInds[EMDES1]);
                CSV.ParseFieldAsString(fields, columnNames, MOD1, out mod1, out nullInds[MOD1]);
                CSV.ParseFieldAsString(fields, columnNames, ACMMINMOD1, out acmMinMod1, out nullInds[ACMMINMOD1]);
                CSV.ParseFieldAsString(fields, columnNames, ACMMAXMOD1, out acmMaxMod1, out nullInds[ACMMAXMOD1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, DATARATE1, -1, out dataRate1, out nullInds[DATARATE1]);
                CSV.ParseFieldAsString(fields, columnNames, EQPTMFR2, out eqptMfr2, out nullInds[EQPTMFR2]);
                CSV.ParseFieldAsString(fields, columnNames, EQPTMOD2, out eqptMod2, out nullInds[EQPTMOD2]);
                CSV.ParseFieldAsString(fields, columnNames, EQPTMODDESC2, out eqptModDesc2, out nullInds[EQPTMODDESC2]);
                CSV.ParseFieldAsString(fields, columnNames, EMDES2, out emDes2, out nullInds[EMDES2]);
                CSV.ParseFieldAsString(fields, columnNames, MOD2, out mod2, out nullInds[MOD2]);
                CSV.ParseFieldAsString(fields, columnNames, ACMMINMOD2, out acmMinMod2, out nullInds[ACMMINMOD2]);
                CSV.ParseFieldAsString(fields, columnNames, ACMMAXMOD2, out acmMaxMod2, out nullInds[ACMMAXMOD2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, DATARATE2, -1, out dataRate2, out nullInds[DATARATE2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCNOMPWR1, -1, out atpcNomPwr1, out nullInds[ATPCNOMPWR1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, PWR1, -1, out pwr1, out nullInds[PWR1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCMAXPWR1, -1, out atpcMaxPwr1, out nullInds[ATPCMAXPWR1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMINMODPWR1, -1, out acmMinModPwr1, out nullInds[ACMMINMODPWR1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMAXMODPWR1, -1, out acmMaxModPwr1, out nullInds[ACMMAXMODPWR1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCNOMPWR2, -1, out atpcNomPwr2, out nullInds[ATPCNOMPWR2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, PWR2, -1, out pwr2, out nullInds[PWR2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCMAXPWR2, -1, out atpcMaxPwr2, out nullInds[ATPCMAXPWR2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMINMODPWR2, -1, out acmMinModPwr2, out nullInds[ACMMINMODPWR2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMAXMODPWR2, -1, out acmMaxModPwr2, out nullInds[ACMMAXMODPWR2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, TXLOSS1, -1, out txLoss1, out nullInds[TXLOSS1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, RXLOSS1, -1, out rxLoss1, out nullInds[RXLOSS1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, CMNLOSS1, -1, out cmnLoss1, out nullInds[CMNLOSS1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, TXLOSS2, -1, out txLoss2, out nullInds[TXLOSS2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, RXLOSS2, -1, out rxLoss2, out nullInds[RXLOSS2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, CMNLOSS2, -1, out cmnLoss2, out nullInds[CMNLOSS2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCNOMRSL1, -1, out atpcNomRSL1, out nullInds[ATPCNOMRSL1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, SELECTRSL1, -1, out selectRSL1, out nullInds[SELECTRSL1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCMAXRSL1, -1, out atpcMaxRSL1, out nullInds[ATPCMAXRSL1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMINMODRSL1, -1, out acmMinModRSL1, out nullInds[ACMMINMODRSL1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMAXMODRSL1, -1, out acmMaxModRSL1, out nullInds[ACMMAXMODRSL1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCNOMRSL2, -1, out atpcNomRSL2, out nullInds[ATPCNOMRSL2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, SELECTRSL2, -1, out selectRSL2, out nullInds[SELECTRSL2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ATPCMAXRSL2, -1, out atpcMaxRSL2, out nullInds[ATPCMAXRSL2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMINMODRSL2, -1, out acmMinModRSL2, out nullInds[ACMMINMODRSL2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, ACMMAXMODRSL2, -1, out acmMaxModRSL2, out nullInds[ACMMAXMODRSL2]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, CENTERFREQ1, -1, out centerFreq1, out nullInds[CENTERFREQ1]);
                CSV.ParseFieldAsString(fields, columnNames, POLAR1, out polar1, out nullInds[POLAR1]);
                CSV.ParseFieldAsDoubleRound(fields, columnNames, CENTERFREQ2, -1, out centerFreq2, out nullInds[CENTERFREQ2]);
                CSV.ParseFieldAsString(fields, columnNames, POLAR2, out polar2, out nullInds[POLAR2]);

            }
            catch (Exception e)
            {
                Console.Write("\nERROR: RawRecord() constructor: uniqueID = {0}: Exception: {1}", uniqueID, e.Message);
                Environment.Exit(4);
            }
        }

        /// <summary>
        /// This method returns an annotated multi-line string that shows the current values
        /// of the member variables of this Comsearch RawRecord object.
        /// </summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== RawRecord ===== ");

            sb.Append("\npathID = " + pathID);
            sb.Append("\nregID = " + regID);
            sb.Append("\npathStat = " + pathStat);
            sb.Append("\nregdate = " + regdate);
            sb.Append("\nconDate = " + conDate);
            sb.Append("\nsite1 = " + site1);
            sb.Append("\nstate1 = " + state1);
            sb.Append("\nlocation1 = " + location1);
            sb.Append("\nsite2 = " + site2);
            sb.Append("\nstate2 = " + state2);
            sb.Append("\nlocation2 = " + location2);
            sb.Append("\ncallsign = " + callsign);
            sb.Append("\ncompName = " + compName);
            sb.Append("\nlat1 = " + lat1);
            sb.Append("\nlng1 = " + lng1);
            sb.Append("\nlat2 = " + lat2);
            sb.Append("\nlng2 = " + lng2);
            sb.Append("\npathLen = " + pathLen);
            sb.Append("\ngrndElev1 = " + grndElev1);
            sb.Append("\ngrndElev2 = " + grndElev2);
            sb.Append("\nantMfr1 = " + antMfr1);
            sb.Append("\nantMod1 = " + antMod1);
            sb.Append("\ngain1 = " + gain1);
            sb.Append("\nbw1 = " + bw1);
            sb.Append("\nrcagl1 = " + rcagl1);
            sb.Append("\nantMfr2 = " + antMfr2);
            sb.Append("\nantMod2 = " + antMod2);
            sb.Append("\ngain2 = " + gain2);
            sb.Append("\nbw2 = " + bw2);
            sb.Append("\nrcagl2 = " + rcagl2);
            sb.Append("\neqptMfr1 = " + eqptMfr1);
            sb.Append("\neqptMod1 = " + eqptMod1);
            sb.Append("\neqptModDesc1 = " + eqptModDesc1);
            sb.Append("\nemDes1 = " + emDes1);
            sb.Append("\nmod1 = " + mod1);
            sb.Append("\nacmMinMod1 = " + acmMinMod1);
            sb.Append("\nacmMaxMod1 = " + acmMaxMod1);
            sb.Append("\ndataRate1 = " + dataRate1);
            sb.Append("\neqptMfr2 = " + eqptMfr2);
            sb.Append("\neqptMod2 = " + eqptMod2);
            sb.Append("\neqptModDesc2 = " + eqptModDesc2);
            sb.Append("\nemDes2 = " + emDes2);
            sb.Append("\nmod2 = " + mod2);
            sb.Append("\nacmMinMod2 = " + acmMinMod2);
            sb.Append("\nacmMaxMod2 = " + acmMaxMod2);
            sb.Append("\ndataRate2 = " + dataRate2);
            sb.Append("\natpcNomPwr1 = " + atpcNomPwr1);
            sb.Append("\npwr1 = " + pwr1);
            sb.Append("\natpcMaxPwr1 = " + atpcMaxPwr1);
            sb.Append("\nacmMinModPwr1 = " + acmMinModPwr1);
            sb.Append("\nacmMaxModPwr1 = " + acmMaxModPwr1);
            sb.Append("\natpcNomPwr2 = " + atpcNomPwr2);
            sb.Append("\npwr2 = " + pwr2);
            sb.Append("\natpcMaxPwr2 = " + atpcMaxPwr2);
            sb.Append("\nacmMinModPwr2 = " + acmMinModPwr2);
            sb.Append("\nacmMaxModPwr2 = " + acmMaxModPwr2);
            sb.Append("\ntxLoss1 = " + txLoss1);
            sb.Append("\nrxLoss1 = " + rxLoss1);
            sb.Append("\ncmnLoss1 = " + cmnLoss1);
            sb.Append("\ntxLoss2 = " + txLoss2);
            sb.Append("\nrxLoss2 = " + rxLoss2);
            sb.Append("\ncmnLoss2 = " + cmnLoss2);
            sb.Append("\natpcNomRSL1 = " + atpcNomRSL1);
            sb.Append("\nselectRSL1 = " + selectRSL1);
            sb.Append("\natpcMaxRSL1 = " + atpcMaxRSL1);
            sb.Append("\nacmMinModRSL1 = " + acmMinModRSL1);
            sb.Append("\nacmMaxModRSL1 = " + acmMaxModRSL1);
            sb.Append("\natpcNomRSL2 = " + atpcNomRSL2);
            sb.Append("\nselectRSL2 = " + selectRSL2);
            sb.Append("\natpcMaxRSL2 = " + atpcMaxRSL2);
            sb.Append("\nacmMinModRSL2 = " + acmMinModRSL2);
            sb.Append("\nacmMaxModRSL2 = " + acmMaxModRSL2);
            sb.Append("\ncenterFreq1 = " + centerFreq1);
            sb.Append("\npolar1 = " + polar1);
            sb.Append("\ncenterFreq2 = " + centerFreq2);
            sb.Append("\npolar2 = " + polar2);
            sb.Append("\nuniqueID = " + uniqueID);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated multi-line string that shows the current values
        /// of the member variables of this Comsearch RawRecord object; null indicator values
        /// are also shown.
        /// </summary>
        /// <returns></returns>
        public string ToStringWN()
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\n===== RawRecord ===== ");

            sb.Append("\n" + nullInds[i++] + "      " + "pathID = " + pathID);
            sb.Append("\n" + nullInds[i++] + "      " + "regID = " + regID);
            sb.Append("\n" + nullInds[i++] + "      " + "pathStat = " + pathStat);
            sb.Append("\n" + nullInds[i++] + "      " + "regdate = " + regdate);
            sb.Append("\n" + nullInds[i++] + "      " + "conDate = " + conDate);
            sb.Append("\n" + nullInds[i++] + "      " + "site1 = " + site1);
            sb.Append("\n" + nullInds[i++] + "      " + "state1 = " + state1);
            sb.Append("\n" + nullInds[i++] + "      " + "location1 = " + location1);
            sb.Append("\n" + nullInds[i++] + "      " + "site2 = " + site2);
            sb.Append("\n" + nullInds[i++] + "      " + "state2 = " + state2);
            sb.Append("\n" + nullInds[i++] + "      " + "location2 = " + location2);
            sb.Append("\n" + nullInds[i++] + "      " + "callsign = " + callsign);
            sb.Append("\n" + nullInds[i++] + "      " + "compName = " + compName);
            sb.Append("\n" + nullInds[i++] + "      " + "lat1 = " + lat1);
            sb.Append("\n" + nullInds[i++] + "      " + "lng1 = " + lng1);
            sb.Append("\n" + nullInds[i++] + "      " + "lat2 = " + lat2);
            sb.Append("\n" + nullInds[i++] + "      " + "lng2 = " + lng2);
            sb.Append("\n" + nullInds[i++] + "      " + "pathLen = " + pathLen);
            sb.Append("\n" + nullInds[i++] + "      " + "grndElev1 = " + grndElev1);
            sb.Append("\n" + nullInds[i++] + "      " + "grndElev2 = " + grndElev2);
            sb.Append("\n" + nullInds[i++] + "      " + "antMfr1 = " + antMfr1);
            sb.Append("\n" + nullInds[i++] + "      " + "antMod1 = " + antMod1);
            sb.Append("\n" + nullInds[i++] + "      " + "gain1 = " + gain1);
            sb.Append("\n" + nullInds[i++] + "      " + "bw1 = " + bw1);
            sb.Append("\n" + nullInds[i++] + "      " + "rcagl1 = " + rcagl1);
            sb.Append("\n" + nullInds[i++] + "      " + "antMfr2 = " + antMfr2);
            sb.Append("\n" + nullInds[i++] + "      " + "antMod2 = " + antMod2);
            sb.Append("\n" + nullInds[i++] + "      " + "gain2 = " + gain2);
            sb.Append("\n" + nullInds[i++] + "      " + "bw2 = " + bw2);
            sb.Append("\n" + nullInds[i++] + "      " + "rcagl2 = " + rcagl2);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptMfr1 = " + eqptMfr1);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptMod1 = " + eqptMod1);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptModDesc1 = " + eqptModDesc1);
            sb.Append("\n" + nullInds[i++] + "      " + "emDes1 = " + emDes1);
            sb.Append("\n" + nullInds[i++] + "      " + "mod1 = " + mod1);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinMod1 = " + acmMinMod1);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxMod1 = " + acmMaxMod1);
            sb.Append("\n" + nullInds[i++] + "      " + "dataRate1 = " + dataRate1);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptMfr2 = " + eqptMfr2);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptMod2 = " + eqptMod2);
            sb.Append("\n" + nullInds[i++] + "      " + "eqptModDesc2 = " + eqptModDesc2);
            sb.Append("\n" + nullInds[i++] + "      " + "emDes2 = " + emDes2);
            sb.Append("\n" + nullInds[i++] + "      " + "mod2 = " + mod2);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinMod2 = " + acmMinMod2);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxMod2 = " + acmMaxMod2);
            sb.Append("\n" + nullInds[i++] + "      " + "dataRate2 = " + dataRate2);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcNomPwr1 = " + atpcNomPwr1);
            sb.Append("\n" + nullInds[i++] + "      " + "pwr1 = " + pwr1);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcMaxPwr1 = " + atpcMaxPwr1);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinModPwr1 = " + acmMinModPwr1);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxModPwr1 = " + acmMaxModPwr1);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcNomPwr2 = " + atpcNomPwr2);
            sb.Append("\n" + nullInds[i++] + "      " + "pwr2 = " + pwr2);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcMaxPwr2 = " + atpcMaxPwr2);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinModPwr2 = " + acmMinModPwr2);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxModPwr2 = " + acmMaxModPwr2);
            sb.Append("\n" + nullInds[i++] + "      " + "txLoss1 = " + txLoss1);
            sb.Append("\n" + nullInds[i++] + "      " + "rxLoss1 = " + rxLoss1);
            sb.Append("\n" + nullInds[i++] + "      " + "cmnLoss1 = " + cmnLoss1);
            sb.Append("\n" + nullInds[i++] + "      " + "txLoss2 = " + txLoss2);
            sb.Append("\n" + nullInds[i++] + "      " + "rxLoss2 = " + rxLoss2);
            sb.Append("\n" + nullInds[i++] + "      " + "cmnLoss2 = " + cmnLoss2);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcNomRSL1 = " + atpcNomRSL1);
            sb.Append("\n" + nullInds[i++] + "      " + "selectRSL1 = " + selectRSL1);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcMaxRSL1 = " + atpcMaxRSL1);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinModRSL1 = " + acmMinModRSL1);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxModRSL1 = " + acmMaxModRSL1);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcNomRSL2 = " + atpcNomRSL2);
            sb.Append("\n" + nullInds[i++] + "      " + "selectRSL2 = " + selectRSL2);
            sb.Append("\n" + nullInds[i++] + "      " + "atpcMaxRSL2 = " + atpcMaxRSL2);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMinModRSL2 = " + acmMinModRSL2);
            sb.Append("\n" + nullInds[i++] + "      " + "acmMaxModRSL2 = " + acmMaxModRSL2);
            sb.Append("\n" + nullInds[i++] + "      " + "centerFreq1 = " + centerFreq1);
            sb.Append("\n" + nullInds[i++] + "      " + "polar1 = " + polar1);
            sb.Append("\n" + nullInds[i++] + "      " + "centerFreq2 = " + centerFreq2);
            sb.Append("\n" + nullInds[i++] + "      " + "polar2 = " + polar2);
            sb.Append("\n" + nullInds[i++] + "      " + "uniqueID = " + uniqueID);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns the member values of this Comsearch RawRecord object formatted
        /// as a CSV string that can be used to provide the VALUES in an SQL INSERT query.
        /// </summary>
        /// <returns></returns>
        public string ToStringAsCSV()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[RawRecord.PATHID] == Constant.DB_NULL ? "NULL, " : "'" + pathID.ToString() + "', ");
            sb.Append(nullInds[RawRecord.REGID] == Constant.DB_NULL ? "NULL, " : "'" + regID.ToString() + "', ");
            sb.Append(nullInds[RawRecord.PATHSTAT] == Constant.DB_NULL ? "NULL, " : "'" + pathStat.ToString() + "', ");
            sb.Append(nullInds[RawRecord.REGDATE] == Constant.DB_NULL ? "NULL, " : "'" + regdate.ToString() + "', ");
            sb.Append(nullInds[RawRecord.CONDATE] == Constant.DB_NULL ? "NULL, " : "'" + conDate.ToString() + "', ");
            sb.Append(nullInds[RawRecord.SITE1] == Constant.DB_NULL ? "NULL, " : "'" + site1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.STATE1] == Constant.DB_NULL ? "NULL, " : "'" + state1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.LOCATION1] == Constant.DB_NULL ? "NULL, " : "'" + location1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.SITE2] == Constant.DB_NULL ? "NULL, " : "'" + site2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.STATE2] == Constant.DB_NULL ? "NULL, " : "'" + state2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.LOCATION2] == Constant.DB_NULL ? "NULL, " : "'" + location2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.CALLSIGN] == Constant.DB_NULL ? "NULL, " : "'" + callsign.ToString() + "', ");
            sb.Append(nullInds[RawRecord.COMPNAME] == Constant.DB_NULL ? "NULL, " : "'" + compName.ToString() + "', ");
            sb.Append(nullInds[RawRecord.LAT1] == Constant.DB_NULL ? "NULL, " : "'" + lat1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.LNG1] == Constant.DB_NULL ? "NULL, " : "'" + lng1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.LAT2] == Constant.DB_NULL ? "NULL, " : "'" + lat2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.LNG2] == Constant.DB_NULL ? "NULL, " : "'" + lng2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.PATHLEN] == Constant.DB_NULL ? "NULL, " : "'" + pathLen.ToString() + "', ");
            sb.Append(nullInds[RawRecord.GRNDELEV1] == Constant.DB_NULL ? "NULL, " : "'" + grndElev1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.GRNDELEV2] == Constant.DB_NULL ? "NULL, " : "'" + grndElev2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ANTMFR1] == Constant.DB_NULL ? "NULL, " : "'" + antMfr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ANTMOD1] == Constant.DB_NULL ? "NULL, " : "'" + antMod1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.GAIN1] == Constant.DB_NULL ? "NULL, " : "'" + gain1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.BW1] == Constant.DB_NULL ? "NULL, " : "'" + bw1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.RCAGL1] == Constant.DB_NULL ? "NULL, " : "'" + rcagl1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ANTMFR2] == Constant.DB_NULL ? "NULL, " : "'" + antMfr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ANTMOD2] == Constant.DB_NULL ? "NULL, " : "'" + antMod2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.GAIN2] == Constant.DB_NULL ? "NULL, " : "'" + gain2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.BW2] == Constant.DB_NULL ? "NULL, " : "'" + bw2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.RCAGL2] == Constant.DB_NULL ? "NULL, " : "'" + rcagl2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EQPTMFR1] == Constant.DB_NULL ? "NULL, " : "'" + eqptMfr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EQPTMOD1] == Constant.DB_NULL ? "NULL, " : "'" + eqptMod1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EQPTMODDESC1] == Constant.DB_NULL ? "NULL, " : "'" + eqptModDesc1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EMDES1] == Constant.DB_NULL ? "NULL, " : "'" + emDes1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.MOD1] == Constant.DB_NULL ? "NULL, " : "'" + mod1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMINMOD1] == Constant.DB_NULL ? "NULL, " : "'" + acmMinMod1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMAXMOD1] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxMod1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.DATARATE1] == Constant.DB_NULL ? "NULL, " : "'" + dataRate1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EQPTMFR2] == Constant.DB_NULL ? "NULL, " : "'" + eqptMfr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EQPTMOD2] == Constant.DB_NULL ? "NULL, " : "'" + eqptMod2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EQPTMODDESC2] == Constant.DB_NULL ? "NULL, " : "'" + eqptModDesc2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.EMDES2] == Constant.DB_NULL ? "NULL, " : "'" + emDes2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.MOD2] == Constant.DB_NULL ? "NULL, " : "'" + mod2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMINMOD2] == Constant.DB_NULL ? "NULL, " : "'" + acmMinMod2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMAXMOD2] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxMod2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.DATARATE2] == Constant.DB_NULL ? "NULL, " : "'" + dataRate2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCNOMPWR1] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomPwr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.PWR1] == Constant.DB_NULL ? "NULL, " : "'" + pwr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCMAXPWR1] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxPwr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMINMODPWR1] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModPwr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMAXMODPWR1] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModPwr1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCNOMPWR2] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomPwr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.PWR2] == Constant.DB_NULL ? "NULL, " : "'" + pwr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCMAXPWR2] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxPwr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMINMODPWR2] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModPwr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMAXMODPWR2] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModPwr2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.TXLOSS1] == Constant.DB_NULL ? "NULL, " : "'" + txLoss1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.RXLOSS1] == Constant.DB_NULL ? "NULL, " : "'" + rxLoss1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.CMNLOSS1] == Constant.DB_NULL ? "NULL, " : "'" + cmnLoss1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.TXLOSS2] == Constant.DB_NULL ? "NULL, " : "'" + txLoss2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.RXLOSS2] == Constant.DB_NULL ? "NULL, " : "'" + rxLoss2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.CMNLOSS2] == Constant.DB_NULL ? "NULL, " : "'" + cmnLoss2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCNOMRSL1] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomRSL1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.SELECTRSL1] == Constant.DB_NULL ? "NULL, " : "'" + selectRSL1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCMAXRSL1] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxRSL1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMINMODRSL1] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModRSL1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMAXMODRSL1] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModRSL1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCNOMRSL2] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomRSL2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.SELECTRSL2] == Constant.DB_NULL ? "NULL, " : "'" + selectRSL2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ATPCMAXRSL2] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxRSL2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMINMODRSL2] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModRSL2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.ACMMAXMODRSL2] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModRSL2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.CENTERFREQ1] == Constant.DB_NULL ? "NULL, " : "'" + centerFreq1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.POLAR1] == Constant.DB_NULL ? "NULL, " : "'" + polar1.ToString() + "', ");
            sb.Append(nullInds[RawRecord.CENTERFREQ2] == Constant.DB_NULL ? "NULL, " : "'" + centerFreq2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.POLAR2] == Constant.DB_NULL ? "NULL, " : "'" + polar2.ToString() + "', ");
            sb.Append(nullInds[RawRecord.UNIQUEID] == Constant.DB_NULL ? "NULL, " : "'" + uniqueID.ToString() + "' ");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated multi-line string that provides the current values
        /// of the member variables of this Comsearch RawRecord object with null indicators in
        /// the form of CSV-formatted strings; all CSV text fields are double-quoted (").
        /// </summary>
        /// <returns></returns>
        public string ToStringAsCSVexport()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[RawRecord.PATHID] == Constant.DB_NULL ? "\"\"," : "\"" + pathID.ToString() + "\",");
            sb.Append(nullInds[RawRecord.REGID] == Constant.DB_NULL ? "\"\"," : "\"" + regID.ToString() + "\",");
            sb.Append(nullInds[RawRecord.PATHSTAT] == Constant.DB_NULL ? "\"\"," : "\"" + pathStat.ToString() + "\",");
            sb.Append(nullInds[RawRecord.REGDATE] == Constant.DB_NULL ? "\"\"," : "\"" + regdate.ToString() + "\",");
            sb.Append(nullInds[RawRecord.CONDATE] == Constant.DB_NULL ? "\"\"," : "\"" + conDate.ToString() + "\",");
            sb.Append(nullInds[RawRecord.SITE1] == Constant.DB_NULL ? "\"\"," : "\"" + site1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.STATE1] == Constant.DB_NULL ? "\"\"," : "\"" + state1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.LOCATION1] == Constant.DB_NULL ? "\"\"," : "\"" + location1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.SITE2] == Constant.DB_NULL ? "\"\"," : "\"" + site2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.STATE2] == Constant.DB_NULL ? "\"\"," : "\"" + state2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.LOCATION2] == Constant.DB_NULL ? "\"\"," : "\"" + location2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.CALLSIGN] == Constant.DB_NULL ? "\"\"," : "\"" + callsign.ToString() + "\",");
            sb.Append(nullInds[RawRecord.COMPNAME] == Constant.DB_NULL ? "\"\"," : "\"" + compName.ToString() + "\",");
            sb.Append(nullInds[RawRecord.LAT1] == Constant.DB_NULL ? "\"\"," : "\"" + lat1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.LNG1] == Constant.DB_NULL ? "\"\"," : "\"" + lng1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.LAT2] == Constant.DB_NULL ? "\"\"," : "\"" + lat2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.LNG2] == Constant.DB_NULL ? "\"\"," : "\"" + lng2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.PATHLEN] == Constant.DB_NULL ? "\"\"," : "\"" + pathLen.ToString() + "\",");
            sb.Append(nullInds[RawRecord.GRNDELEV1] == Constant.DB_NULL ? "\"\"," : "\"" + grndElev1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.GRNDELEV2] == Constant.DB_NULL ? "\"\"," : "\"" + grndElev2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ANTMFR1] == Constant.DB_NULL ? "\"\"," : "\"" + antMfr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ANTMOD1] == Constant.DB_NULL ? "\"\"," : "\"" + antMod1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.GAIN1] == Constant.DB_NULL ? "\"\"," : "\"" + gain1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.BW1] == Constant.DB_NULL ? "\"\"," : "\"" + bw1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.RCAGL1] == Constant.DB_NULL ? "\"\"," : "\"" + rcagl1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ANTMFR2] == Constant.DB_NULL ? "\"\"," : "\"" + antMfr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ANTMOD2] == Constant.DB_NULL ? "\"\"," : "\"" + antMod2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.GAIN2] == Constant.DB_NULL ? "\"\"," : "\"" + gain2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.BW2] == Constant.DB_NULL ? "\"\"," : "\"" + bw2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.RCAGL2] == Constant.DB_NULL ? "\"\"," : "\"" + rcagl2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EQPTMFR1] == Constant.DB_NULL ? "\"\"," : "\"" + eqptMfr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EQPTMOD1] == Constant.DB_NULL ? "\"\"," : "\"" + eqptMod1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EQPTMODDESC1] == Constant.DB_NULL ? "\"\"," : "\"" + eqptModDesc1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EMDES1] == Constant.DB_NULL ? "\"\"," : "\"" + emDes1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.MOD1] == Constant.DB_NULL ? "\"\"," : "\"" + mod1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMINMOD1] == Constant.DB_NULL ? "\"\"," : "\"" + acmMinMod1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMAXMOD1] == Constant.DB_NULL ? "\"\"," : "\"" + acmMaxMod1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.DATARATE1] == Constant.DB_NULL ? "\"\"," : "\"" + dataRate1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EQPTMFR2] == Constant.DB_NULL ? "\"\"," : "\"" + eqptMfr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EQPTMOD2] == Constant.DB_NULL ? "\"\"," : "\"" + eqptMod2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EQPTMODDESC2] == Constant.DB_NULL ? "\"\"," : "\"" + eqptModDesc2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.EMDES2] == Constant.DB_NULL ? "\"\"," : "\"" + emDes2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.MOD2] == Constant.DB_NULL ? "\"\"," : "\"" + mod2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMINMOD2] == Constant.DB_NULL ? "\"\"," : "\"" + acmMinMod2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMAXMOD2] == Constant.DB_NULL ? "\"\"," : "\"" + acmMaxMod2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.DATARATE2] == Constant.DB_NULL ? "\"\"," : "\"" + dataRate2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCNOMPWR1] == Constant.DB_NULL ? "\"\"," : "\"" + atpcNomPwr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.PWR1] == Constant.DB_NULL ? "\"\"," : "\"" + pwr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCMAXPWR1] == Constant.DB_NULL ? "\"\"," : "\"" + atpcMaxPwr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMINMODPWR1] == Constant.DB_NULL ? "\"\"," : "\"" + acmMinModPwr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMAXMODPWR1] == Constant.DB_NULL ? "\"\"," : "\"" + acmMaxModPwr1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCNOMPWR2] == Constant.DB_NULL ? "\"\"," : "\"" + atpcNomPwr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.PWR2] == Constant.DB_NULL ? "\"\"," : "\"" + pwr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCMAXPWR2] == Constant.DB_NULL ? "\"\"," : "\"" + atpcMaxPwr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMINMODPWR2] == Constant.DB_NULL ? "\"\"," : "\"" + acmMinModPwr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMAXMODPWR2] == Constant.DB_NULL ? "\"\"," : "\"" + acmMaxModPwr2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.TXLOSS1] == Constant.DB_NULL ? "\"\"," : "\"" + txLoss1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.RXLOSS1] == Constant.DB_NULL ? "\"\"," : "\"" + rxLoss1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.CMNLOSS1] == Constant.DB_NULL ? "\"\"," : "\"" + cmnLoss1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.TXLOSS2] == Constant.DB_NULL ? "\"\"," : "\"" + txLoss2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.RXLOSS2] == Constant.DB_NULL ? "\"\"," : "\"" + rxLoss2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.CMNLOSS2] == Constant.DB_NULL ? "\"\"," : "\"" + cmnLoss2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCNOMRSL1] == Constant.DB_NULL ? "\"\"," : "\"" + atpcNomRSL1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.SELECTRSL1] == Constant.DB_NULL ? "\"\"," : "\"" + selectRSL1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCMAXRSL1] == Constant.DB_NULL ? "\"\"," : "\"" + atpcMaxRSL1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMINMODRSL1] == Constant.DB_NULL ? "\"\"," : "\"" + acmMinModRSL1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMAXMODRSL1] == Constant.DB_NULL ? "\"\"," : "\"" + acmMaxModRSL1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCNOMRSL2] == Constant.DB_NULL ? "\"\"," : "\"" + atpcNomRSL2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.SELECTRSL2] == Constant.DB_NULL ? "\"\"," : "\"" + selectRSL2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ATPCMAXRSL2] == Constant.DB_NULL ? "\"\"," : "\"" + atpcMaxRSL2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMINMODRSL2] == Constant.DB_NULL ? "\"\"," : "\"" + acmMinModRSL2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.ACMMAXMODRSL2] == Constant.DB_NULL ? "\"\"," : "\"" + acmMaxModRSL2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.CENTERFREQ1] == Constant.DB_NULL ? "\"\"," : "\"" + centerFreq1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.POLAR1] == Constant.DB_NULL ? "\"\"," : "\"" + polar1.ToString() + "\",");
            sb.Append(nullInds[RawRecord.CENTERFREQ2] == Constant.DB_NULL ? "\"\"," : "\"" + centerFreq2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.POLAR2] == Constant.DB_NULL ? "\"\"," : "\"" + polar2.ToString() + "\",");
            sb.Append(nullInds[RawRecord.UNIQUEID] == Constant.DB_NULL ? "\"\"," : "\"" + uniqueID.ToString() + "\"");

            return sb.ToString();
        }


        /// <summary>
        /// This method returns the member values of this Comsearch rawRecord object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query.
        /// </summary>
        /// <returns></returns>
        public string ToStringAsCSVequates()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[RawRecord.PATHID] + " = " + (nullInds[RawRecord.PATHID] == Constant.DB_NULL ? "NULL, " : "'" + pathID.ToString() + "', "));
            sb.Append(columnNames[RawRecord.REGID] + " = " + (nullInds[RawRecord.REGID] == Constant.DB_NULL ? "NULL, " : "'" + regID.ToString() + "', "));
            sb.Append(columnNames[RawRecord.PATHSTAT] + " = " + (nullInds[RawRecord.PATHSTAT] == Constant.DB_NULL ? "NULL, " : "'" + pathStat.ToString() + "', "));
            sb.Append(columnNames[RawRecord.REGDATE] + " = " + (nullInds[RawRecord.REGDATE] == Constant.DB_NULL ? "NULL, " : "'" + regdate.ToString() + "', "));
            sb.Append(columnNames[RawRecord.CONDATE] + " = " + (nullInds[RawRecord.CONDATE] == Constant.DB_NULL ? "NULL, " : "'" + conDate.ToString() + "', "));
            sb.Append(columnNames[RawRecord.SITE1] + " = " + (nullInds[RawRecord.SITE1] == Constant.DB_NULL ? "NULL, " : "'" + site1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.STATE1] + " = " + (nullInds[RawRecord.STATE1] == Constant.DB_NULL ? "NULL, " : "'" + state1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.LOCATION1] + " = " + (nullInds[RawRecord.LOCATION1] == Constant.DB_NULL ? "NULL, " : "'" + location1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.SITE2] + " = " + (nullInds[RawRecord.SITE2] == Constant.DB_NULL ? "NULL, " : "'" + site2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.STATE2] + " = " + (nullInds[RawRecord.STATE2] == Constant.DB_NULL ? "NULL, " : "'" + state2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.LOCATION2] + " = " + (nullInds[RawRecord.LOCATION2] == Constant.DB_NULL ? "NULL, " : "'" + location2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.CALLSIGN] + " = " + (nullInds[RawRecord.CALLSIGN] == Constant.DB_NULL ? "NULL, " : "'" + callsign.ToString() + "', "));
            sb.Append(columnNames[RawRecord.COMPNAME] + " = " + (nullInds[RawRecord.COMPNAME] == Constant.DB_NULL ? "NULL, " : "'" + compName.ToString() + "', "));
            sb.Append(columnNames[RawRecord.LAT1] + " = " + (nullInds[RawRecord.LAT1] == Constant.DB_NULL ? "NULL, " : "'" + lat1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.LNG1] + " = " + (nullInds[RawRecord.LNG1] == Constant.DB_NULL ? "NULL, " : "'" + lng1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.LAT2] + " = " + (nullInds[RawRecord.LAT2] == Constant.DB_NULL ? "NULL, " : "'" + lat2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.LNG2] + " = " + (nullInds[RawRecord.LNG2] == Constant.DB_NULL ? "NULL, " : "'" + lng2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.PATHLEN] + " = " + (nullInds[RawRecord.PATHLEN] == Constant.DB_NULL ? "NULL, " : "'" + pathLen.ToString() + "', "));
            sb.Append(columnNames[RawRecord.GRNDELEV1] + " = " + (nullInds[RawRecord.GRNDELEV1] == Constant.DB_NULL ? "NULL, " : "'" + grndElev1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.GRNDELEV2] + " = " + (nullInds[RawRecord.GRNDELEV2] == Constant.DB_NULL ? "NULL, " : "'" + grndElev2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ANTMFR1] + " = " + (nullInds[RawRecord.ANTMFR1] == Constant.DB_NULL ? "NULL, " : "'" + antMfr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ANTMOD1] + " = " + (nullInds[RawRecord.ANTMOD1] == Constant.DB_NULL ? "NULL, " : "'" + antMod1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.GAIN1] + " = " + (nullInds[RawRecord.GAIN1] == Constant.DB_NULL ? "NULL, " : "'" + gain1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.BW1] + " = " + (nullInds[RawRecord.BW1] == Constant.DB_NULL ? "NULL, " : "'" + bw1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.RCAGL1] + " = " + (nullInds[RawRecord.RCAGL1] == Constant.DB_NULL ? "NULL, " : "'" + rcagl1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ANTMFR2] + " = " + (nullInds[RawRecord.ANTMFR2] == Constant.DB_NULL ? "NULL, " : "'" + antMfr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ANTMOD2] + " = " + (nullInds[RawRecord.ANTMOD2] == Constant.DB_NULL ? "NULL, " : "'" + antMod2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.GAIN2] + " = " + (nullInds[RawRecord.GAIN2] == Constant.DB_NULL ? "NULL, " : "'" + gain2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.BW2] + " = " + (nullInds[RawRecord.BW2] == Constant.DB_NULL ? "NULL, " : "'" + bw2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.RCAGL2] + " = " + (nullInds[RawRecord.RCAGL2] == Constant.DB_NULL ? "NULL, " : "'" + rcagl2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EQPTMFR1] + " = " + (nullInds[RawRecord.EQPTMFR1] == Constant.DB_NULL ? "NULL, " : "'" + eqptMfr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EQPTMOD1] + " = " + (nullInds[RawRecord.EQPTMOD1] == Constant.DB_NULL ? "NULL, " : "'" + eqptMod1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EQPTMODDESC1] + " = " + (nullInds[RawRecord.EQPTMODDESC1] == Constant.DB_NULL ? "NULL, " : "'" + eqptModDesc1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EMDES1] + " = " + (nullInds[RawRecord.EMDES1] == Constant.DB_NULL ? "NULL, " : "'" + emDes1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.MOD1] + " = " + (nullInds[RawRecord.MOD1] == Constant.DB_NULL ? "NULL, " : "'" + mod1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMINMOD1] + " = " + (nullInds[RawRecord.ACMMINMOD1] == Constant.DB_NULL ? "NULL, " : "'" + acmMinMod1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMAXMOD1] + " = " + (nullInds[RawRecord.ACMMAXMOD1] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxMod1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.DATARATE1] + " = " + (nullInds[RawRecord.DATARATE1] == Constant.DB_NULL ? "NULL, " : "'" + dataRate1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EQPTMFR2] + " = " + (nullInds[RawRecord.EQPTMFR2] == Constant.DB_NULL ? "NULL, " : "'" + eqptMfr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EQPTMOD2] + " = " + (nullInds[RawRecord.EQPTMOD2] == Constant.DB_NULL ? "NULL, " : "'" + eqptMod2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EQPTMODDESC2] + " = " + (nullInds[RawRecord.EQPTMODDESC2] == Constant.DB_NULL ? "NULL, " : "'" + eqptModDesc2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.EMDES2] + " = " + (nullInds[RawRecord.EMDES2] == Constant.DB_NULL ? "NULL, " : "'" + emDes2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.MOD2] + " = " + (nullInds[RawRecord.MOD2] == Constant.DB_NULL ? "NULL, " : "'" + mod2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMINMOD2] + " = " + (nullInds[RawRecord.ACMMINMOD2] == Constant.DB_NULL ? "NULL, " : "'" + acmMinMod2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMAXMOD2] + " = " + (nullInds[RawRecord.ACMMAXMOD2] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxMod2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.DATARATE2] + " = " + (nullInds[RawRecord.DATARATE2] == Constant.DB_NULL ? "NULL, " : "'" + dataRate2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCNOMPWR1] + " = " + (nullInds[RawRecord.ATPCNOMPWR1] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomPwr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.PWR1] + " = " + (nullInds[RawRecord.PWR1] == Constant.DB_NULL ? "NULL, " : "'" + pwr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCMAXPWR1] + " = " + (nullInds[RawRecord.ATPCMAXPWR1] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxPwr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMINMODPWR1] + " = " + (nullInds[RawRecord.ACMMINMODPWR1] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModPwr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMAXMODPWR1] + " = " + (nullInds[RawRecord.ACMMAXMODPWR1] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModPwr1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCNOMPWR2] + " = " + (nullInds[RawRecord.ATPCNOMPWR2] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomPwr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.PWR2] + " = " + (nullInds[RawRecord.PWR2] == Constant.DB_NULL ? "NULL, " : "'" + pwr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCMAXPWR2] + " = " + (nullInds[RawRecord.ATPCMAXPWR2] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxPwr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMINMODPWR2] + " = " + (nullInds[RawRecord.ACMMINMODPWR2] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModPwr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMAXMODPWR2] + " = " + (nullInds[RawRecord.ACMMAXMODPWR2] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModPwr2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.TXLOSS1] + " = " + (nullInds[RawRecord.TXLOSS1] == Constant.DB_NULL ? "NULL, " : "'" + txLoss1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.RXLOSS1] + " = " + (nullInds[RawRecord.RXLOSS1] == Constant.DB_NULL ? "NULL, " : "'" + rxLoss1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.CMNLOSS1] + " = " + (nullInds[RawRecord.CMNLOSS1] == Constant.DB_NULL ? "NULL, " : "'" + cmnLoss1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.TXLOSS2] + " = " + (nullInds[RawRecord.TXLOSS2] == Constant.DB_NULL ? "NULL, " : "'" + txLoss2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.RXLOSS2] + " = " + (nullInds[RawRecord.RXLOSS2] == Constant.DB_NULL ? "NULL, " : "'" + rxLoss2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.CMNLOSS2] + " = " + (nullInds[RawRecord.CMNLOSS2] == Constant.DB_NULL ? "NULL, " : "'" + cmnLoss2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCNOMRSL1] + " = " + (nullInds[RawRecord.ATPCNOMRSL1] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomRSL1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.SELECTRSL1] + " = " + (nullInds[RawRecord.SELECTRSL1] == Constant.DB_NULL ? "NULL, " : "'" + selectRSL1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCMAXRSL1] + " = " + (nullInds[RawRecord.ATPCMAXRSL1] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxRSL1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMINMODRSL1] + " = " + (nullInds[RawRecord.ACMMINMODRSL1] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModRSL1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMAXMODRSL1] + " = " + (nullInds[RawRecord.ACMMAXMODRSL1] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModRSL1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCNOMRSL2] + " = " + (nullInds[RawRecord.ATPCNOMRSL2] == Constant.DB_NULL ? "NULL, " : "'" + atpcNomRSL2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.SELECTRSL2] + " = " + (nullInds[RawRecord.SELECTRSL2] == Constant.DB_NULL ? "NULL, " : "'" + selectRSL2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ATPCMAXRSL2] + " = " + (nullInds[RawRecord.ATPCMAXRSL2] == Constant.DB_NULL ? "NULL, " : "'" + atpcMaxRSL2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMINMODRSL2] + " = " + (nullInds[RawRecord.ACMMINMODRSL2] == Constant.DB_NULL ? "NULL, " : "'" + acmMinModRSL2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.ACMMAXMODRSL2] + " = " + (nullInds[RawRecord.ACMMAXMODRSL2] == Constant.DB_NULL ? "NULL, " : "'" + acmMaxModRSL2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.CENTERFREQ1] + " = " + (nullInds[RawRecord.CENTERFREQ1] == Constant.DB_NULL ? "NULL, " : "'" + centerFreq1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.POLAR1] + " = " + (nullInds[RawRecord.POLAR1] == Constant.DB_NULL ? "NULL, " : "'" + polar1.ToString() + "', "));
            sb.Append(columnNames[RawRecord.CENTERFREQ2] + " = " + (nullInds[RawRecord.CENTERFREQ2] == Constant.DB_NULL ? "NULL, " : "'" + centerFreq2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.POLAR2] + " = " + (nullInds[RawRecord.POLAR2] == Constant.DB_NULL ? "NULL, " : "'" + polar2.ToString() + "', "));
            sb.Append(columnNames[RawRecord.UNIQUEID] + " = " + (nullInds[RawRecord.UNIQUEID] == Constant.DB_NULL ? "NULL, " : "'" + uniqueID.ToString() + "' "));

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an array of IntPtr that point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the member variable definitions of this class.
        /// Thus, this method creates the parameter bindings prior to an SQL / ODBC 'fetch', 'update' or 
        /// 'insert' query.
        /// </summary>
        /// <param name="hStmt"></param>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[PATHID] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[PATHID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, PATHID + 1, tgtValPtrs[PATHID], nullIndPtrs[PATHID]);

            tgtValPtrs[REGID] = Marshal.AllocHGlobal(REGID_SZ + 1);
            nullIndPtrs[REGID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, REGID + 1, tgtValPtrs[REGID], RawRecord.REGID_SZ, nullIndPtrs[REGID]);

            tgtValPtrs[PATHSTAT] = Marshal.AllocHGlobal(PATHSTAT_SZ + 1);
            nullIndPtrs[PATHSTAT] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PATHSTAT + 1, tgtValPtrs[PATHSTAT], RawRecord.PATHSTAT_SZ, nullIndPtrs[PATHSTAT]);

            tgtValPtrs[REGDATE] = Marshal.AllocHGlobal(REGDATE_SZ + 1);
            nullIndPtrs[REGDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, REGDATE + 1, tgtValPtrs[REGDATE], RawRecord.REGDATE_SZ, nullIndPtrs[REGDATE]);

            tgtValPtrs[CONDATE] = Marshal.AllocHGlobal(CONDATE_SZ + 1);
            nullIndPtrs[CONDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CONDATE + 1, tgtValPtrs[CONDATE], RawRecord.CONDATE_SZ, nullIndPtrs[CONDATE]);

            tgtValPtrs[SITE1] = Marshal.AllocHGlobal(SITE1_SZ + 1);
            nullIndPtrs[SITE1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SITE1 + 1, tgtValPtrs[SITE1], RawRecord.SITE1_SZ, nullIndPtrs[SITE1]);

            tgtValPtrs[STATE1] = Marshal.AllocHGlobal(STATE1_SZ + 1);
            nullIndPtrs[STATE1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATE1 + 1, tgtValPtrs[STATE1], RawRecord.STATE1_SZ, nullIndPtrs[STATE1]);

            tgtValPtrs[LOCATION1] = Marshal.AllocHGlobal(LOCATION1_SZ + 1);
            nullIndPtrs[LOCATION1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LOCATION1 + 1, tgtValPtrs[LOCATION1], RawRecord.LOCATION1_SZ, nullIndPtrs[LOCATION1]);

            tgtValPtrs[SITE2] = Marshal.AllocHGlobal(SITE2_SZ + 1);
            nullIndPtrs[SITE2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SITE2 + 1, tgtValPtrs[SITE2], RawRecord.SITE2_SZ, nullIndPtrs[SITE2]);

            tgtValPtrs[STATE2] = Marshal.AllocHGlobal(STATE2_SZ + 1);
            nullIndPtrs[STATE2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATE2 + 1, tgtValPtrs[STATE2], RawRecord.STATE2_SZ, nullIndPtrs[STATE2]);

            tgtValPtrs[LOCATION2] = Marshal.AllocHGlobal(LOCATION2_SZ + 1);
            nullIndPtrs[LOCATION2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LOCATION2 + 1, tgtValPtrs[LOCATION2], RawRecord.LOCATION2_SZ, nullIndPtrs[LOCATION2]);

            tgtValPtrs[CALLSIGN] = Marshal.AllocHGlobal(CALLSIGN_SZ + 1);
            nullIndPtrs[CALLSIGN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALLSIGN + 1, tgtValPtrs[CALLSIGN], RawRecord.CALLSIGN_SZ, nullIndPtrs[CALLSIGN]);

            tgtValPtrs[COMPNAME] = Marshal.AllocHGlobal(COMPNAME_SZ + 1);
            nullIndPtrs[COMPNAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, COMPNAME + 1, tgtValPtrs[COMPNAME], RawRecord.COMPNAME_SZ, nullIndPtrs[COMPNAME]);

            tgtValPtrs[LAT1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LAT1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LAT1 + 1, tgtValPtrs[LAT1], nullIndPtrs[LAT1]);

            tgtValPtrs[LNG1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LNG1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LNG1 + 1, tgtValPtrs[LNG1], nullIndPtrs[LNG1]);

            tgtValPtrs[LAT2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LAT2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LAT2 + 1, tgtValPtrs[LAT2], nullIndPtrs[LAT2]);

            tgtValPtrs[LNG2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LNG2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LNG2 + 1, tgtValPtrs[LNG2], nullIndPtrs[LNG2]);

            tgtValPtrs[PATHLEN] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[PATHLEN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, PATHLEN + 1, tgtValPtrs[PATHLEN], nullIndPtrs[PATHLEN]);

            tgtValPtrs[GRNDELEV1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[GRNDELEV1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, GRNDELEV1 + 1, tgtValPtrs[GRNDELEV1], nullIndPtrs[GRNDELEV1]);

            tgtValPtrs[GRNDELEV2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[GRNDELEV2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, GRNDELEV2 + 1, tgtValPtrs[GRNDELEV2], nullIndPtrs[GRNDELEV2]);

            tgtValPtrs[ANTMFR1] = Marshal.AllocHGlobal(ANTMFR1_SZ + 1);
            nullIndPtrs[ANTMFR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANTMFR1 + 1, tgtValPtrs[ANTMFR1], RawRecord.ANTMFR1_SZ, nullIndPtrs[ANTMFR1]);

            tgtValPtrs[ANTMOD1] = Marshal.AllocHGlobal(ANTMOD1_SZ + 1);
            nullIndPtrs[ANTMOD1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANTMOD1 + 1, tgtValPtrs[ANTMOD1], RawRecord.ANTMOD1_SZ, nullIndPtrs[ANTMOD1]);

            tgtValPtrs[GAIN1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[GAIN1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, GAIN1 + 1, tgtValPtrs[GAIN1], nullIndPtrs[GAIN1]);

            tgtValPtrs[BW1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[BW1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, BW1 + 1, tgtValPtrs[BW1], nullIndPtrs[BW1]);

            tgtValPtrs[RCAGL1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RCAGL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RCAGL1 + 1, tgtValPtrs[RCAGL1], nullIndPtrs[RCAGL1]);

            tgtValPtrs[ANTMFR2] = Marshal.AllocHGlobal(ANTMFR2_SZ + 1);
            nullIndPtrs[ANTMFR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANTMFR2 + 1, tgtValPtrs[ANTMFR2], RawRecord.ANTMFR2_SZ, nullIndPtrs[ANTMFR2]);

            tgtValPtrs[ANTMOD2] = Marshal.AllocHGlobal(ANTMOD2_SZ + 1);
            nullIndPtrs[ANTMOD2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANTMOD2 + 1, tgtValPtrs[ANTMOD2], RawRecord.ANTMOD2_SZ, nullIndPtrs[ANTMOD2]);

            tgtValPtrs[GAIN2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[GAIN2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, GAIN2 + 1, tgtValPtrs[GAIN2], nullIndPtrs[GAIN2]);

            tgtValPtrs[BW2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[BW2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, BW2 + 1, tgtValPtrs[BW2], nullIndPtrs[BW2]);

            tgtValPtrs[RCAGL2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RCAGL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RCAGL2 + 1, tgtValPtrs[RCAGL2], nullIndPtrs[RCAGL2]);

            tgtValPtrs[EQPTMFR1] = Marshal.AllocHGlobal(EQPTMFR1_SZ + 1);
            nullIndPtrs[EQPTMFR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTMFR1 + 1, tgtValPtrs[EQPTMFR1], RawRecord.EQPTMFR1_SZ, nullIndPtrs[EQPTMFR1]);

            tgtValPtrs[EQPTMOD1] = Marshal.AllocHGlobal(EQPTMOD1_SZ + 1);
            nullIndPtrs[EQPTMOD1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTMOD1 + 1, tgtValPtrs[EQPTMOD1], RawRecord.EQPTMOD1_SZ, nullIndPtrs[EQPTMOD1]);

            tgtValPtrs[EQPTMODDESC1] = Marshal.AllocHGlobal(EQPTMODDESC1_SZ + 1);
            nullIndPtrs[EQPTMODDESC1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTMODDESC1 + 1, tgtValPtrs[EQPTMODDESC1], RawRecord.EQPTMODDESC1_SZ, nullIndPtrs[EQPTMODDESC1]);

            tgtValPtrs[EMDES1] = Marshal.AllocHGlobal(EMDES1_SZ + 1);
            nullIndPtrs[EMDES1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EMDES1 + 1, tgtValPtrs[EMDES1], RawRecord.EMDES1_SZ, nullIndPtrs[EMDES1]);

            tgtValPtrs[MOD1] = Marshal.AllocHGlobal(MOD1_SZ + 1);
            nullIndPtrs[MOD1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MOD1 + 1, tgtValPtrs[MOD1], RawRecord.MOD1_SZ, nullIndPtrs[MOD1]);

            tgtValPtrs[ACMMINMOD1] = Marshal.AllocHGlobal(ACMMINMOD1_SZ + 1);
            nullIndPtrs[ACMMINMOD1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACMMINMOD1 + 1, tgtValPtrs[ACMMINMOD1], RawRecord.ACMMINMOD1_SZ, nullIndPtrs[ACMMINMOD1]);

            tgtValPtrs[ACMMAXMOD1] = Marshal.AllocHGlobal(ACMMAXMOD1_SZ + 1);
            nullIndPtrs[ACMMAXMOD1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACMMAXMOD1 + 1, tgtValPtrs[ACMMAXMOD1], RawRecord.ACMMAXMOD1_SZ, nullIndPtrs[ACMMAXMOD1]);

            tgtValPtrs[DATARATE1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[DATARATE1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, DATARATE1 + 1, tgtValPtrs[DATARATE1], nullIndPtrs[DATARATE1]);

            tgtValPtrs[EQPTMFR2] = Marshal.AllocHGlobal(EQPTMFR2_SZ + 1);
            nullIndPtrs[EQPTMFR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTMFR2 + 1, tgtValPtrs[EQPTMFR2], RawRecord.EQPTMFR2_SZ, nullIndPtrs[EQPTMFR2]);

            tgtValPtrs[EQPTMOD2] = Marshal.AllocHGlobal(EQPTMOD2_SZ + 1);
            nullIndPtrs[EQPTMOD2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTMOD2 + 1, tgtValPtrs[EQPTMOD2], RawRecord.EQPTMOD2_SZ, nullIndPtrs[EQPTMOD2]);

            tgtValPtrs[EQPTMODDESC2] = Marshal.AllocHGlobal(EQPTMODDESC2_SZ + 1);
            nullIndPtrs[EQPTMODDESC2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EQPTMODDESC2 + 1, tgtValPtrs[EQPTMODDESC2], RawRecord.EQPTMODDESC2_SZ, nullIndPtrs[EQPTMODDESC2]);

            tgtValPtrs[EMDES2] = Marshal.AllocHGlobal(EMDES2_SZ + 1);
            nullIndPtrs[EMDES2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, EMDES2 + 1, tgtValPtrs[EMDES2], RawRecord.EMDES2_SZ, nullIndPtrs[EMDES2]);

            tgtValPtrs[MOD2] = Marshal.AllocHGlobal(MOD2_SZ + 1);
            nullIndPtrs[MOD2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MOD2 + 1, tgtValPtrs[MOD2], RawRecord.MOD2_SZ, nullIndPtrs[MOD2]);

            tgtValPtrs[ACMMINMOD2] = Marshal.AllocHGlobal(ACMMINMOD2_SZ + 1);
            nullIndPtrs[ACMMINMOD2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACMMINMOD2 + 1, tgtValPtrs[ACMMINMOD2], RawRecord.ACMMINMOD2_SZ, nullIndPtrs[ACMMINMOD2]);

            tgtValPtrs[ACMMAXMOD2] = Marshal.AllocHGlobal(ACMMAXMOD2_SZ + 1);
            nullIndPtrs[ACMMAXMOD2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACMMAXMOD2 + 1, tgtValPtrs[ACMMAXMOD2], RawRecord.ACMMAXMOD2_SZ, nullIndPtrs[ACMMAXMOD2]);

            tgtValPtrs[DATARATE2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[DATARATE2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, DATARATE2 + 1, tgtValPtrs[DATARATE2], nullIndPtrs[DATARATE2]);

            tgtValPtrs[ATPCNOMPWR1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCNOMPWR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCNOMPWR1 + 1, tgtValPtrs[ATPCNOMPWR1], nullIndPtrs[ATPCNOMPWR1]);

            tgtValPtrs[PWR1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[PWR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, PWR1 + 1, tgtValPtrs[PWR1], nullIndPtrs[PWR1]);

            tgtValPtrs[ATPCMAXPWR1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCMAXPWR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCMAXPWR1 + 1, tgtValPtrs[ATPCMAXPWR1], nullIndPtrs[ATPCMAXPWR1]);

            tgtValPtrs[ACMMINMODPWR1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMINMODPWR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMINMODPWR1 + 1, tgtValPtrs[ACMMINMODPWR1], nullIndPtrs[ACMMINMODPWR1]);

            tgtValPtrs[ACMMAXMODPWR1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMAXMODPWR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMAXMODPWR1 + 1, tgtValPtrs[ACMMAXMODPWR1], nullIndPtrs[ACMMAXMODPWR1]);

            tgtValPtrs[ATPCNOMPWR2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCNOMPWR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCNOMPWR2 + 1, tgtValPtrs[ATPCNOMPWR2], nullIndPtrs[ATPCNOMPWR2]);

            tgtValPtrs[PWR2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[PWR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, PWR2 + 1, tgtValPtrs[PWR2], nullIndPtrs[PWR2]);

            tgtValPtrs[ATPCMAXPWR2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCMAXPWR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCMAXPWR2 + 1, tgtValPtrs[ATPCMAXPWR2], nullIndPtrs[ATPCMAXPWR2]);

            tgtValPtrs[ACMMINMODPWR2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMINMODPWR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMINMODPWR2 + 1, tgtValPtrs[ACMMINMODPWR2], nullIndPtrs[ACMMINMODPWR2]);

            tgtValPtrs[ACMMAXMODPWR2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMAXMODPWR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMAXMODPWR2 + 1, tgtValPtrs[ACMMAXMODPWR2], nullIndPtrs[ACMMAXMODPWR2]);

            tgtValPtrs[TXLOSS1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[TXLOSS1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, TXLOSS1 + 1, tgtValPtrs[TXLOSS1], nullIndPtrs[TXLOSS1]);

            tgtValPtrs[RXLOSS1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RXLOSS1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RXLOSS1 + 1, tgtValPtrs[RXLOSS1], nullIndPtrs[RXLOSS1]);

            tgtValPtrs[CMNLOSS1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[CMNLOSS1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, CMNLOSS1 + 1, tgtValPtrs[CMNLOSS1], nullIndPtrs[CMNLOSS1]);

            tgtValPtrs[TXLOSS2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[TXLOSS2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, TXLOSS2 + 1, tgtValPtrs[TXLOSS2], nullIndPtrs[TXLOSS2]);

            tgtValPtrs[RXLOSS2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RXLOSS2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RXLOSS2 + 1, tgtValPtrs[RXLOSS2], nullIndPtrs[RXLOSS2]);

            tgtValPtrs[CMNLOSS2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[CMNLOSS2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, CMNLOSS2 + 1, tgtValPtrs[CMNLOSS2], nullIndPtrs[CMNLOSS2]);

            tgtValPtrs[ATPCNOMRSL1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCNOMRSL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCNOMRSL1 + 1, tgtValPtrs[ATPCNOMRSL1], nullIndPtrs[ATPCNOMRSL1]);

            tgtValPtrs[SELECTRSL1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[SELECTRSL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, SELECTRSL1 + 1, tgtValPtrs[SELECTRSL1], nullIndPtrs[SELECTRSL1]);

            tgtValPtrs[ATPCMAXRSL1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCMAXRSL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCMAXRSL1 + 1, tgtValPtrs[ATPCMAXRSL1], nullIndPtrs[ATPCMAXRSL1]);

            tgtValPtrs[ACMMINMODRSL1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMINMODRSL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMINMODRSL1 + 1, tgtValPtrs[ACMMINMODRSL1], nullIndPtrs[ACMMINMODRSL1]);

            tgtValPtrs[ACMMAXMODRSL1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMAXMODRSL1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMAXMODRSL1 + 1, tgtValPtrs[ACMMAXMODRSL1], nullIndPtrs[ACMMAXMODRSL1]);

            tgtValPtrs[ATPCNOMRSL2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCNOMRSL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCNOMRSL2 + 1, tgtValPtrs[ATPCNOMRSL2], nullIndPtrs[ATPCNOMRSL2]);

            tgtValPtrs[SELECTRSL2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[SELECTRSL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, SELECTRSL2 + 1, tgtValPtrs[SELECTRSL2], nullIndPtrs[SELECTRSL2]);

            tgtValPtrs[ATPCMAXRSL2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ATPCMAXRSL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ATPCMAXRSL2 + 1, tgtValPtrs[ATPCMAXRSL2], nullIndPtrs[ATPCMAXRSL2]);

            tgtValPtrs[ACMMINMODRSL2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMINMODRSL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMINMODRSL2 + 1, tgtValPtrs[ACMMINMODRSL2], nullIndPtrs[ACMMINMODRSL2]);

            tgtValPtrs[ACMMAXMODRSL2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ACMMAXMODRSL2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ACMMAXMODRSL2 + 1, tgtValPtrs[ACMMAXMODRSL2], nullIndPtrs[ACMMAXMODRSL2]);

            tgtValPtrs[CENTERFREQ1] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[CENTERFREQ1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, CENTERFREQ1 + 1, tgtValPtrs[CENTERFREQ1], nullIndPtrs[CENTERFREQ1]);

            tgtValPtrs[POLAR1] = Marshal.AllocHGlobal(POLAR1_SZ + 1);
            nullIndPtrs[POLAR1] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, POLAR1 + 1, tgtValPtrs[POLAR1], RawRecord.POLAR1_SZ, nullIndPtrs[POLAR1]);

            tgtValPtrs[CENTERFREQ2] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[CENTERFREQ2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, CENTERFREQ2 + 1, tgtValPtrs[CENTERFREQ2], nullIndPtrs[CENTERFREQ2]);

            tgtValPtrs[POLAR2] = Marshal.AllocHGlobal(POLAR2_SZ + 1);
            nullIndPtrs[POLAR2] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, POLAR2 + 1, tgtValPtrs[POLAR2], RawRecord.POLAR2_SZ, nullIndPtrs[POLAR2]);

            tgtValPtrs[UNIQUEID] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[UNIQUEID] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, UNIQUEID + 1, tgtValPtrs[UNIQUEID], nullIndPtrs[UNIQUEID]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned SdAnte object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="RawRecord"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out RawRecord RawRecord, out SQLLEN[] nullInds)
        {
            RawRecord = new RawRecord();
            nullInds = NullHelper.CreateArrayOfNullInd(RawRecord.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[PATHID]);
            if (nullInd != Constant.DB_NULL)
            {
                RawRecord.pathID = Marshal.ReadInt32(tgtValPtrs[PATHID]);
                nullInds[PATHID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[REGID]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.regID = "";
                nullInds[REGID] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.regID = Marshal.PtrToStringAnsi(tgtValPtrs[REGID]).Trim();
                nullInds[REGID] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PATHSTAT]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.pathStat = "";
                nullInds[PATHSTAT] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.pathStat = Marshal.PtrToStringAnsi(tgtValPtrs[PATHSTAT]).Trim();
                nullInds[PATHSTAT] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[REGDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.regdate = "";
                nullInds[REGDATE] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.regdate = Marshal.PtrToStringAnsi(tgtValPtrs[REGDATE]).Trim();
                nullInds[REGDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CONDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.conDate = "";
                nullInds[CONDATE] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.conDate = Marshal.PtrToStringAnsi(tgtValPtrs[CONDATE]).Trim();
                nullInds[CONDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SITE1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.site1 = "";
                nullInds[SITE1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.site1 = Marshal.PtrToStringAnsi(tgtValPtrs[SITE1]).Trim();
                nullInds[SITE1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATE1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.state1 = "";
                nullInds[STATE1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.state1 = Marshal.PtrToStringAnsi(tgtValPtrs[STATE1]).Trim();
                nullInds[STATE1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LOCATION1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.location1 = "";
                nullInds[LOCATION1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.location1 = Marshal.PtrToStringAnsi(tgtValPtrs[LOCATION1]).Trim();
                nullInds[LOCATION1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SITE2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.site2 = "";
                nullInds[SITE2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.site2 = Marshal.PtrToStringAnsi(tgtValPtrs[SITE2]).Trim();
                nullInds[SITE2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATE2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.state2 = "";
                nullInds[STATE2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.state2 = Marshal.PtrToStringAnsi(tgtValPtrs[STATE2]).Trim();
                nullInds[STATE2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LOCATION2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.location2 = "";
                nullInds[LOCATION2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.location2 = Marshal.PtrToStringAnsi(tgtValPtrs[LOCATION2]).Trim();
                nullInds[LOCATION2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALLSIGN]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.callsign = "";
                nullInds[CALLSIGN] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.callsign = Marshal.PtrToStringAnsi(tgtValPtrs[CALLSIGN]).Trim();
                nullInds[CALLSIGN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[COMPNAME]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.compName = "";
                nullInds[COMPNAME] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.compName = Marshal.PtrToStringAnsi(tgtValPtrs[COMPNAME]).Trim();
                nullInds[COMPNAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LAT1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LAT1], D, 0, 1);
                RawRecord.lat1 = D[0];
                nullInds[LAT1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LNG1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LNG1], D, 0, 1);
                RawRecord.lng1 = D[0];
                nullInds[LNG1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LAT2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LAT2], D, 0, 1);
                RawRecord.lat2 = D[0];
                nullInds[LAT2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LNG2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LNG2], D, 0, 1);
                RawRecord.lng2 = D[0];
                nullInds[LNG2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PATHLEN]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PATHLEN], D, 0, 1);
                RawRecord.pathLen = D[0];
                nullInds[PATHLEN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[GRNDELEV1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[GRNDELEV1], D, 0, 1);
                RawRecord.grndElev1 = D[0];
                nullInds[GRNDELEV1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[GRNDELEV2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[GRNDELEV2], D, 0, 1);
                RawRecord.grndElev2 = D[0];
                nullInds[GRNDELEV2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTMFR1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.antMfr1 = "";
                nullInds[ANTMFR1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.antMfr1 = Marshal.PtrToStringAnsi(tgtValPtrs[ANTMFR1]).Trim();
                nullInds[ANTMFR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTMOD1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.antMod1 = "";
                nullInds[ANTMOD1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.antMod1 = Marshal.PtrToStringAnsi(tgtValPtrs[ANTMOD1]).Trim();
                nullInds[ANTMOD1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[GAIN1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[GAIN1], D, 0, 1);
                RawRecord.gain1 = D[0];
                nullInds[GAIN1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BW1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[BW1], D, 0, 1);
                RawRecord.bw1 = D[0];
                nullInds[BW1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RCAGL1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RCAGL1], D, 0, 1);
                RawRecord.rcagl1 = D[0];
                nullInds[RCAGL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTMFR2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.antMfr2 = "";
                nullInds[ANTMFR2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.antMfr2 = Marshal.PtrToStringAnsi(tgtValPtrs[ANTMFR2]).Trim();
                nullInds[ANTMFR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTMOD2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.antMod2 = "";
                nullInds[ANTMOD2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.antMod2 = Marshal.PtrToStringAnsi(tgtValPtrs[ANTMOD2]).Trim();
                nullInds[ANTMOD2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[GAIN2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[GAIN2], D, 0, 1);
                RawRecord.gain2 = D[0];
                nullInds[GAIN2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[BW2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[BW2], D, 0, 1);
                RawRecord.bw2 = D[0];
                nullInds[BW2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RCAGL2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RCAGL2], D, 0, 1);
                RawRecord.rcagl2 = D[0];
                nullInds[RCAGL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTMFR1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.eqptMfr1 = "";
                nullInds[EQPTMFR1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.eqptMfr1 = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTMFR1]).Trim();
                nullInds[EQPTMFR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTMOD1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.eqptMod1 = "";
                nullInds[EQPTMOD1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.eqptMod1 = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTMOD1]).Trim();
                nullInds[EQPTMOD1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTMODDESC1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.eqptModDesc1 = "";
                nullInds[EQPTMODDESC1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.eqptModDesc1 = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTMODDESC1]).Trim();
                nullInds[EQPTMODDESC1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EMDES1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.emDes1 = "";
                nullInds[EMDES1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.emDes1 = Marshal.PtrToStringAnsi(tgtValPtrs[EMDES1]).Trim();
                nullInds[EMDES1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MOD1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.mod1 = "";
                nullInds[MOD1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.mod1 = Marshal.PtrToStringAnsi(tgtValPtrs[MOD1]).Trim();
                nullInds[MOD1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMINMOD1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.acmMinMod1 = "";
                nullInds[ACMMINMOD1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.acmMinMod1 = Marshal.PtrToStringAnsi(tgtValPtrs[ACMMINMOD1]).Trim();
                nullInds[ACMMINMOD1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMAXMOD1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.acmMaxMod1 = "";
                nullInds[ACMMAXMOD1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.acmMaxMod1 = Marshal.PtrToStringAnsi(tgtValPtrs[ACMMAXMOD1]).Trim();
                nullInds[ACMMAXMOD1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DATARATE1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DATARATE1], D, 0, 1);
                RawRecord.dataRate1 = D[0];
                nullInds[DATARATE1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTMFR2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.eqptMfr2 = "";
                nullInds[EQPTMFR2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.eqptMfr2 = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTMFR2]).Trim();
                nullInds[EQPTMFR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTMOD2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.eqptMod2 = "";
                nullInds[EQPTMOD2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.eqptMod2 = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTMOD2]).Trim();
                nullInds[EQPTMOD2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EQPTMODDESC2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.eqptModDesc2 = "";
                nullInds[EQPTMODDESC2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.eqptModDesc2 = Marshal.PtrToStringAnsi(tgtValPtrs[EQPTMODDESC2]).Trim();
                nullInds[EQPTMODDESC2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[EMDES2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.emDes2 = "";
                nullInds[EMDES2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.emDes2 = Marshal.PtrToStringAnsi(tgtValPtrs[EMDES2]).Trim();
                nullInds[EMDES2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MOD2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.mod2 = "";
                nullInds[MOD2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.mod2 = Marshal.PtrToStringAnsi(tgtValPtrs[MOD2]).Trim();
                nullInds[MOD2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMINMOD2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.acmMinMod2 = "";
                nullInds[ACMMINMOD2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.acmMinMod2 = Marshal.PtrToStringAnsi(tgtValPtrs[ACMMINMOD2]).Trim();
                nullInds[ACMMINMOD2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMAXMOD2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.acmMaxMod2 = "";
                nullInds[ACMMAXMOD2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.acmMaxMod2 = Marshal.PtrToStringAnsi(tgtValPtrs[ACMMAXMOD2]).Trim();
                nullInds[ACMMAXMOD2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DATARATE2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DATARATE2], D, 0, 1);
                RawRecord.dataRate2 = D[0];
                nullInds[DATARATE2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCNOMPWR1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCNOMPWR1], D, 0, 1);
                RawRecord.atpcNomPwr1 = D[0];
                nullInds[ATPCNOMPWR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PWR1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PWR1], D, 0, 1);
                RawRecord.pwr1 = D[0];
                nullInds[PWR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCMAXPWR1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCMAXPWR1], D, 0, 1);
                RawRecord.atpcMaxPwr1 = D[0];
                nullInds[ATPCMAXPWR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMINMODPWR1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMINMODPWR1], D, 0, 1);
                RawRecord.acmMinModPwr1 = D[0];
                nullInds[ACMMINMODPWR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMAXMODPWR1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMAXMODPWR1], D, 0, 1);
                RawRecord.acmMaxModPwr1 = D[0];
                nullInds[ACMMAXMODPWR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCNOMPWR2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCNOMPWR2], D, 0, 1);
                RawRecord.atpcNomPwr2 = D[0];
                nullInds[ATPCNOMPWR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PWR2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[PWR2], D, 0, 1);
                RawRecord.pwr2 = D[0];
                nullInds[PWR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCMAXPWR2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCMAXPWR2], D, 0, 1);
                RawRecord.atpcMaxPwr2 = D[0];
                nullInds[ATPCMAXPWR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMINMODPWR2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMINMODPWR2], D, 0, 1);
                RawRecord.acmMinModPwr2 = D[0];
                nullInds[ACMMINMODPWR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMAXMODPWR2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMAXMODPWR2], D, 0, 1);
                RawRecord.acmMaxModPwr2 = D[0];
                nullInds[ACMMAXMODPWR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXLOSS1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXLOSS1], D, 0, 1);
                RawRecord.txLoss1 = D[0];
                nullInds[TXLOSS1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXLOSS1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXLOSS1], D, 0, 1);
                RawRecord.rxLoss1 = D[0];
                nullInds[RXLOSS1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CMNLOSS1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[CMNLOSS1], D, 0, 1);
                RawRecord.cmnLoss1 = D[0];
                nullInds[CMNLOSS1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXLOSS2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXLOSS2], D, 0, 1);
                RawRecord.txLoss2 = D[0];
                nullInds[TXLOSS2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXLOSS2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXLOSS2], D, 0, 1);
                RawRecord.rxLoss2 = D[0];
                nullInds[RXLOSS2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CMNLOSS2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[CMNLOSS2], D, 0, 1);
                RawRecord.cmnLoss2 = D[0];
                nullInds[CMNLOSS2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCNOMRSL1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCNOMRSL1], D, 0, 1);
                RawRecord.atpcNomRSL1 = D[0];
                nullInds[ATPCNOMRSL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SELECTRSL1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[SELECTRSL1], D, 0, 1);
                RawRecord.selectRSL1 = D[0];
                nullInds[SELECTRSL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCMAXRSL1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCMAXRSL1], D, 0, 1);
                RawRecord.atpcMaxRSL1 = D[0];
                nullInds[ATPCMAXRSL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMINMODRSL1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMINMODRSL1], D, 0, 1);
                RawRecord.acmMinModRSL1 = D[0];
                nullInds[ACMMINMODRSL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMAXMODRSL1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMAXMODRSL1], D, 0, 1);
                RawRecord.acmMaxModRSL1 = D[0];
                nullInds[ACMMAXMODRSL1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCNOMRSL2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCNOMRSL2], D, 0, 1);
                RawRecord.atpcNomRSL2 = D[0];
                nullInds[ATPCNOMRSL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SELECTRSL2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[SELECTRSL2], D, 0, 1);
                RawRecord.selectRSL2 = D[0];
                nullInds[SELECTRSL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ATPCMAXRSL2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ATPCMAXRSL2], D, 0, 1);
                RawRecord.atpcMaxRSL2 = D[0];
                nullInds[ATPCMAXRSL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMINMODRSL2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMINMODRSL2], D, 0, 1);
                RawRecord.acmMinModRSL2 = D[0];
                nullInds[ACMMINMODRSL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACMMAXMODRSL2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ACMMAXMODRSL2], D, 0, 1);
                RawRecord.acmMaxModRSL2 = D[0];
                nullInds[ACMMAXMODRSL2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CENTERFREQ1]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[CENTERFREQ1], D, 0, 1);
                RawRecord.centerFreq1 = D[0];
                nullInds[CENTERFREQ1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[POLAR1]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.polar1 = "";
                nullInds[POLAR1] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.polar1 = Marshal.PtrToStringAnsi(tgtValPtrs[POLAR1]).Trim();
                nullInds[POLAR1] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CENTERFREQ2]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[CENTERFREQ2], D, 0, 1);
                RawRecord.centerFreq2 = D[0];
                nullInds[CENTERFREQ2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[POLAR2]);
            if (nullInd == Constant.DB_NULL)
            {
                RawRecord.polar2 = "";
                nullInds[POLAR2] = Constant.DB_NULL;
            }
            else
            {
                RawRecord.polar2 = Marshal.PtrToStringAnsi(tgtValPtrs[POLAR2]).Trim();
                nullInds[POLAR2] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[UNIQUEID]);
            if (nullInd != Constant.DB_NULL)
            {
                RawRecord.uniqueID = Marshal.ReadInt32(tgtValPtrs[UNIQUEID]);
                nullInds[UNIQUEID] = Constant.DB_NOT_NULL;
            }

        }

        /// <summary>
        /// This method inputs a list of Comsearch RawRecords and returns a (reduced) list
        /// of those records that are within a prescribed distance of the CAN-USA border.
        /// </summary>
        /// <param name="rawRecords"></param>
        /// <param name="prescribedDistanceKm"></param>
        /// <param name="rawRecordsNearBorder"></param>
        /// <returns></returns>
        public static int SelectRecordsNearToBorder(List<RawRecord> rawRecords, double prescribedDistanceKm, out List<RawRecord> rawRecordsNearBorder)
        {
            // 'out' requirement.
            rawRecordsNearBorder = new List<RawRecord>();

            string msg = String.Format("\n\nDiscarding Comsearch records more than {0:F1} km from the border: STARTED ...   ", prescribedDistanceKm);
            //...Log2.v(msg);
            Console.Write(msg);

            // Reduce the list of RawRecords to only those within maxDistanceToBorderKm of the Canadian border.
            bool kmlLoadWasOK = CanUsaBorder.LoadKmlMapData();
            if (!kmlLoadWasOK)
            {
                Log2.e("\nRawRecord.SelectRecordsNearToBorder(): ERROR: call to LoadKmlMapData() FAILED, errMsg = {0}", CanUsaBorder.GetDetailsAsString());
                Application.ExitQuietly(1);
            }

            foreach (RawRecord rawRecord in rawRecords)
            {
                LatLng localLatLng = new LatLng(rawRecord.lat1, rawRecord.lng1);
                LatLng remoteLatLng = new LatLng(rawRecord.lat2, rawRecord.lng2);

                bool localEndIsWithinXkm;
                bool remoteEndIsWithinXkm;
                double localEndDist;
                double remoteEndDist;
                if (rawRecord.state1 == "AK")
                {
                    localEndIsWithinXkm = CanUsaBorder.IsWithinXkmOfAlaskanBorder(localLatLng, prescribedDistanceKm);
                    localEndDist = CanUsaBorder.Detail.mFinalDistanceToBorder;

                    remoteEndIsWithinXkm = CanUsaBorder.IsWithinXkmOfAlaskanBorder(remoteLatLng, prescribedDistanceKm);
                    remoteEndDist = CanUsaBorder.Detail.mFinalDistanceToBorder;
                }
                else
                {
                    localEndIsWithinXkm = CanUsaBorder.IsWithinXkmOfSouthernBorder(localLatLng, prescribedDistanceKm);
                    localEndDist = CanUsaBorder.Detail.mFinalDistanceToBorder;

                    remoteEndIsWithinXkm = CanUsaBorder.IsWithinXkmOfSouthernBorder(remoteLatLng, prescribedDistanceKm);
                    remoteEndDist = CanUsaBorder.Detail.mFinalDistanceToBorder;
                }

                // If either the local or remote end of the record is within the prescribed maximum distance
                // then add it to the sub-list that is returned.
                if (localEndIsWithinXkm || remoteEndIsWithinXkm)
                {
                    rawRecordsNearBorder.Add(rawRecord);

                    if (localEndDist == double.MaxValue) localEndDist = prescribedDistanceKm + 1;
                    if (remoteEndDist == double.MaxValue) remoteEndDist = prescribedDistanceKm + 1;

                    //....Log2.v("\nRawRecord.SelectRecordsNearToBorder(): {0}, {1}, {2}, {3}, {4}, {5}, {6}, ", rawRecord.pathID, rawRecord.lat1, rawRecord.lng1, localEndDist, rawRecord.lat2, rawRecord.lng2, remoteEndDist);
                }
            }

            //...Log2.v("\nRawRecord.SelectRecordsNearToBorder(): FINISHED: {1} sites were retained.", prescribedDistanceKm, rawRecordsNearBorder.Count);
            Console.Write("FINISHED.\nQty. {0} Comsearch sites were retained.", rawRecordsNearBorder.Count);
            return rawRecordsNearBorder.Count;
        }


        /// <summary>
        /// This method exports a list of Comsearch RawRecords to a prescribed CSV text file.
        /// </summary>
        /// <param name="rawRecords"></param>
        /// <param name="pathToCsvTextFile"></param>
        public static void ExportListToCsvTextFile(List<RawRecord> rawRecords, string pathToCsvTextFile)
        {
            Console.Write("\nExporting raw records to CSV file: {0}", pathToCsvTextFile);

            StringBuilder sb = new StringBuilder();

            // Insert an initial CSV record line containing the column names.
            sb.Append(ComsearchColumnNamesAsCSV);

            foreach (RawRecord rawRecord in rawRecords)
            {
                string recordAsCSV = rawRecord.ToStringAsCSVexport();

                // Delete the final field (uniqueID) because it is non-original.
                recordAsCSV = Regex.Replace(recordAsCSV, @",[^,]*$", "");

                // Accumulate the CSV record.
                sb.Append("\n" + recordAsCSV);
            }

            File.WriteAllText(pathToCsvTextFile, sb.ToString());

            Console.Write("\rExporting raw records to CSV file: FINISHED: {0}", pathToCsvTextFile);
        }


        public const string CREATE_TABLE = "" +
"IF OBJECT_ID('{0}.{1}') IS NOT NULL DROP TABLE {0}.{1}; " +
"CREATE TABLE {0}.{1}" +
"(" +
    "[pathID] [int] NULL, " +
    "[regID] [varchar](MAX) NULL, " +
    "[pathStat] [varchar](MAX) NULL, " +
    "[regdate] [varchar](MAX) NULL, " +
    "[conDate] [varchar](MAX) NULL, " +
    "[site1] [varchar](MAX) NULL, " +
    "[state1] [varchar](MAX) NULL, " +
    "[location1] [varchar](MAX) NULL, " +
    "[site2] [varchar](MAX) NULL, " +
    "[state2] [varchar](MAX) NULL, " +
    "[location2] [varchar](MAX) NULL, " +
    "[callsign] [varchar](MAX) NULL, " +
    "[compName] [varchar](MAX) NULL, " +
    "[lat1] [float] NULL, " +
    "[lng1] [float] NULL, " +
    "[lat2] [float] NULL, " +
    "[lng2] [float] NULL, " +
    "[pathLen] [float] NULL, " +
    "[grndElev1] [float] NULL, " +
    "[grndElev2] [float] NULL, " +
    "[antMfr1] [varchar](MAX) NULL, " +
    "[antMod1] [varchar](MAX) NULL, " +
    "[gain1] [float] NULL, " +
    "[bw1] [float] NULL, " +
    "[rcagl1] [float] NULL, " +
    "[antMfr2] [varchar](MAX) NULL, " +
    "[antMod2] [varchar](MAX) NULL, " +
    "[gain2] [float] NULL, " +
    "[bw2] [float] NULL, " +
    "[rcagl2] [float] NULL, " +
    "[eqptMfr1] [varchar](MAX) NULL, " +
    "[eqptMod1] [varchar](MAX) NULL, " +
    "[eqptModDesc1] [varchar](MAX) NULL, " +
    "[emDes1] [varchar](MAX) NULL, " +
    "[mod1] [varchar](MAX) NULL, " +
    "[acmMinMod1] [varchar](MAX) NULL, " +
    "[acmMaxMod1] [varchar](MAX) NULL, " +
    "[dataRate1] [float] NULL, " +
    "[eqptMfr2] [varchar](MAX) NULL, " +
    "[eqptMod2] [varchar](MAX) NULL, " +
    "[eqptModDesc2] [varchar](MAX) NULL, " +
    "[emDes2] [varchar](MAX) NULL, " +
    "[mod2] [varchar](MAX) NULL, " +
    "[acmMinMod2] [varchar](MAX) NULL, " +
    "[acmMaxMod2] [varchar](MAX) NULL, " +
    "[dataRate2] [float] NULL, " +
    "[atpcNomPwr1] [float] NULL, " +
    "[pwr1] [float] NULL, " +
    "[atpcMaxPwr1] [float] NULL, " +
    "[acmMinModPwr1] [float] NULL, " +
    "[acmMaxModPwr1] [float] NULL, " +
    "[atpcNomPwr2] [float] NULL, " +
    "[pwr2] [float] NULL, " +
    "[atpcMaxPwr2] [float] NULL, " +
    "[acmMinModPwr2] [float] NULL, " +
    "[acmMaxModPwr2] [float] NULL, " +
    "[txLoss1] [float] NULL, " +
    "[rxLoss1] [float] NULL, " +
    "[cmnLoss1] [float] NULL, " +
    "[txLoss2] [float] NULL, " +
    "[rxLoss2] [float] NULL, " +
    "[cmnLoss2] [float] NULL, " +
    "[atpcNomRSL1] [float] NULL, " +
    "[selectRSL1] [float] NULL, " +
    "[atpcMaxRSL1] [float] NULL, " +
    "[acmMinModRSL1] [float] NULL, " +
    "[acmMaxModRSL1] [float] NULL, " +
    "[atpcNomRSL2] [float] NULL, " +
    "[selectRSL2] [float] NULL, " +
    "[atpcMaxRSL2] [float] NULL, " +
    "[acmMinModRSL2] [float] NULL, " +
    "[acmMaxModRSL2] [float] NULL, " +
    "[centerFreq1] [float] NULL, " +
    "[polar1] [varchar](MAX) NULL, " +
    "[centerFreq2] [float] NULL, " +
    "[polar2] [varchar](MAX) NULL, " +
    "[uniqueID] [int] NOT NULL, " +
    "CONSTRAINT[PK_COMSEARCH_NICE_RECORDS] PRIMARY KEY CLUSTERED " +
    "(" +
        "[uniqueID] ASC" +
    ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON[PRIMARY]" +
") ON[PRIMARY]";




















    }
}
