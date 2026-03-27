using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComsearchToTAFL
{
    using _Configuration;
    using _NewLib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides a more useful encapsulation of the Comsearch data fields 
    /// than is provided by a RawRecord; a NiceRecord comprises two LinkEnd objects 
    /// ('local' and 'remote') and a set of Comsearch fields that are common to both.
    /// </summary>
    public class NiceRecord
    {
        // Comsearch-provided scalar (one per-link) link properties.
        public int pathID = 0;
        public string regID = "";
        public string pathStat = "";
        public string regdate = "";
        public string conDate = "";
        public string callsign = "";
        public string compName = "";
        public double pathLen = 0;
        // Added: use this to keep track of records.
        public int uniqueID = 0;
        // Added: to save having to check a link's connectivity (one-way or two-way) all the time, 
        // check it once and save the result.
        public Connectivity connectivity = Connectivity.UNKNOWN;

        // Link-end data.
        public LinkEnd[] linkEnd = new LinkEnd[2];

        // Null indicators.
        public SQLLEN[] nullInds;

        public const int NUM_COLUMNS = 10;

        public const int PATHID = 0;
        public const int REGID = 1;
        public const int PATHSTAT = 2;
        public const int REGDATE = 3;
        public const int CONDATE = 4;
        public const int CALLSIGN = 5;
        public const int COMPNAME = 6;
        public const int PATHLEN = 7;
        public const int UNIQUEID = 8;
        public const int CONNECTIVITY = 9;

        /// <summary>
        /// The primitive public constructor.
        /// </summary>
        public NiceRecord()
        {
            linkEnd[0] = new LinkEnd();
            linkEnd[1] = new LinkEnd();

            nullInds = new SQLLEN[NUM_COLUMNS];

            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                nullInds[i] = Constant.DB_NULL;
            }

            nullInds[CONNECTIVITY] = Constant.DB_NOT_NULL;
        }

        /// <summary>
        /// A constructor that inputs a Comsearch RawRecord object and creates a NiceRecord object.
        /// </summary>
        /// <param name="raw"></param>
        public NiceRecord(RawRecord raw) : this()  // <== Note the call to the primitive constructor to create the NullInd arrays.
        {
            // First, convert the scalar properties of the link.
            pathID = raw.pathID;
            nullInds[PATHID] = raw.nullInds[RawRecord.PATHID];

            regID = raw.regID;
            nullInds[REGID] = raw.nullInds[RawRecord.REGID];

            pathStat = raw.pathStat;
            nullInds[PATHSTAT] = raw.nullInds[RawRecord.PATHSTAT];

            regdate = Conversions.ComsearchDateToTAFL(raw.regdate);
            nullInds[REGDATE] = raw.nullInds[RawRecord.REGDATE];

            conDate = Conversions.ComsearchDateToTAFL(raw.conDate);
            nullInds[CONDATE] = raw.nullInds[RawRecord.CONDATE];

            callsign = raw.callsign;
            nullInds[CALLSIGN] = raw.nullInds[RawRecord.CALLSIGN];

            compName = raw.compName;
            nullInds[COMPNAME] = raw.nullInds[RawRecord.COMPNAME];

            pathLen = raw.pathLen;
            nullInds[PATHLEN] = raw.nullInds[RawRecord.PATHLEN];

            uniqueID = raw.uniqueID;
            nullInds[UNIQUEID] = raw.nullInds[RawRecord.UNIQUEID];

            // Now convert the LinkEnd parameters for the 'local' (L) end.
            linkEnd[(int)End.L].site = raw.site1;
            linkEnd[(int)End.L].nullInds[LinkEnd.SITE] = raw.nullInds[RawRecord.SITE1];

            linkEnd[(int)End.L].state = raw.state1;
            linkEnd[(int)End.L].nullInds[LinkEnd.STATE] = raw.nullInds[RawRecord.STATE1];

            linkEnd[(int)End.L].lat = raw.lat1;
            linkEnd[(int)End.L].nullInds[LinkEnd.LAT] = raw.nullInds[RawRecord.LAT1];

            linkEnd[(int)End.L].lng = raw.lng1;
            linkEnd[(int)End.L].nullInds[LinkEnd.LNG] = raw.nullInds[RawRecord.LNG1];

            linkEnd[(int)End.L].grndElev = raw.grndElev1;
            linkEnd[(int)End.L].nullInds[LinkEnd.GRNDELEV] = raw.nullInds[RawRecord.GRNDELEV1];

            linkEnd[(int)End.L].antMfr = raw.antMfr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ANTMFR] = raw.nullInds[RawRecord.ANTMFR1];

            linkEnd[(int)End.L].antMod = raw.antMod1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ANTMOD] = raw.nullInds[RawRecord.ANTMOD1];

            linkEnd[(int)End.L].gain = raw.gain1;
            linkEnd[(int)End.L].nullInds[LinkEnd.GAIN] = raw.nullInds[RawRecord.GAIN1];

            linkEnd[(int)End.L].bw = raw.bw1;
            linkEnd[(int)End.L].nullInds[LinkEnd.BW] = raw.nullInds[RawRecord.BW1];

            linkEnd[(int)End.L].rcagl = raw.rcagl1;
            linkEnd[(int)End.L].nullInds[LinkEnd.RCAGL] = raw.nullInds[RawRecord.RCAGL1];

            linkEnd[(int)End.L].eqptMfr = raw.eqptMfr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.EQPTMFR] = raw.nullInds[RawRecord.EQPTMFR1];

            linkEnd[(int)End.L].eqptMod = raw.eqptMod1;
            linkEnd[(int)End.L].nullInds[LinkEnd.EQPTMOD] = raw.nullInds[RawRecord.EQPTMOD1];

            linkEnd[(int)End.L].eqptModDesc = raw.eqptModDesc1;
            linkEnd[(int)End.L].nullInds[LinkEnd.EQPTMODDESC] = raw.nullInds[RawRecord.EQPTMODDESC1];

            linkEnd[(int)End.L].emDes = raw.emDes1;
            linkEnd[(int)End.L].nullInds[LinkEnd.EMDES] = raw.nullInds[RawRecord.EMDES1];

            linkEnd[(int)End.L].mod = raw.mod1;
            linkEnd[(int)End.L].nullInds[LinkEnd.MOD] = raw.nullInds[RawRecord.MOD1];

            linkEnd[(int)End.L].acmMinMod = raw.acmMinMod1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ACMMINMOD] = raw.nullInds[RawRecord.ACMMINMOD1];

            linkEnd[(int)End.L].acmMaxMod = raw.acmMaxMod1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ACMMAXMOD] = raw.nullInds[RawRecord.ACMMAXMOD1];

            linkEnd[(int)End.L].dataRate = raw.dataRate1;
            linkEnd[(int)End.L].nullInds[LinkEnd.DATARATE] = raw.nullInds[RawRecord.DATARATE1];

            linkEnd[(int)End.L].atpcNomPwr = raw.atpcNomPwr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ATPCNOMPWR] = raw.nullInds[RawRecord.ATPCNOMPWR1];

            linkEnd[(int)End.L].pwr = raw.pwr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.PWR] = raw.nullInds[RawRecord.PWR1];

            linkEnd[(int)End.L].atpcMaxPwr = raw.atpcMaxPwr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ATPCMAXPWR] = raw.nullInds[RawRecord.ATPCMAXPWR1];

            linkEnd[(int)End.L].acmMinModPwr = raw.acmMinModPwr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ACMMINMODPWR] = raw.nullInds[RawRecord.ACMMINMODPWR1];

            linkEnd[(int)End.L].acmMaxModPwr = raw.acmMaxModPwr1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ACMMAXMODPWR] = raw.nullInds[RawRecord.ACMMAXMODPWR1];

            linkEnd[(int)End.L].txLoss = raw.txLoss1;
            linkEnd[(int)End.L].nullInds[LinkEnd.TXLOSS] = raw.nullInds[RawRecord.TXLOSS1];

            linkEnd[(int)End.L].rxLoss = raw.rxLoss1;
            linkEnd[(int)End.L].nullInds[LinkEnd.RXLOSS] = raw.nullInds[RawRecord.RXLOSS1];

            linkEnd[(int)End.L].cmnLoss = raw.cmnLoss1;
            linkEnd[(int)End.L].nullInds[LinkEnd.CMNLOSS] = raw.nullInds[RawRecord.CMNLOSS1];

            linkEnd[(int)End.L].atpcNomRSL = raw.atpcNomRSL1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ATPCNOMRSL] = raw.nullInds[RawRecord.ATPCNOMRSL1];

            linkEnd[(int)End.L].selectRSL = raw.selectRSL1;
            linkEnd[(int)End.L].nullInds[LinkEnd.SELECTRSL] = raw.nullInds[RawRecord.SELECTRSL1];

            linkEnd[(int)End.L].atpcMaxRSL = raw.atpcMaxRSL1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ATPCMAXRSL] = raw.nullInds[RawRecord.ATPCMAXRSL1];

            linkEnd[(int)End.L].acmMinModRSL = raw.acmMinModRSL1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ACMMINMODRSL] = raw.nullInds[RawRecord.ACMMINMODRSL1];

            linkEnd[(int)End.L].acmMaxModRSL = raw.acmMaxModRSL1;
            linkEnd[(int)End.L].nullInds[LinkEnd.ACMMAXMODRSL] = raw.nullInds[RawRecord.ACMMAXMODRSL1];

            linkEnd[(int)End.L].centerFreq = raw.centerFreq1;
            linkEnd[(int)End.L].nullInds[LinkEnd.CENTERFREQ] = raw.nullInds[RawRecord.CENTERFREQ1];

            linkEnd[(int)End.L].polar = raw.polar1;
            linkEnd[(int)End.L].nullInds[LinkEnd.POLAR] = raw.nullInds[RawRecord.POLAR1];

            // Now convert the LinkEnd parameters for the 'remote' (R) end.
            linkEnd[(int)End.R].site = raw.site2;
            linkEnd[(int)End.R].nullInds[LinkEnd.SITE] = raw.nullInds[RawRecord.SITE2];

            linkEnd[(int)End.R].state = raw.state2;
            linkEnd[(int)End.R].nullInds[LinkEnd.STATE] = raw.nullInds[RawRecord.STATE2];

            linkEnd[(int)End.R].lat = raw.lat2;
            linkEnd[(int)End.R].nullInds[LinkEnd.LAT] = raw.nullInds[RawRecord.LAT2];

            linkEnd[(int)End.R].lng = raw.lng2;
            linkEnd[(int)End.R].nullInds[LinkEnd.LNG] = raw.nullInds[RawRecord.LNG2];

            linkEnd[(int)End.R].grndElev = raw.grndElev2;
            linkEnd[(int)End.R].nullInds[LinkEnd.GRNDELEV] = raw.nullInds[RawRecord.GRNDELEV2];

            linkEnd[(int)End.R].antMfr = raw.antMfr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ANTMFR] = raw.nullInds[RawRecord.ANTMFR2];

            linkEnd[(int)End.R].antMod = raw.antMod2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ANTMOD] = raw.nullInds[RawRecord.ANTMOD2];

            linkEnd[(int)End.R].gain = raw.gain2;
            linkEnd[(int)End.R].nullInds[LinkEnd.GAIN] = raw.nullInds[RawRecord.GAIN2];

            linkEnd[(int)End.R].bw = raw.bw2;
            linkEnd[(int)End.R].nullInds[LinkEnd.BW] = raw.nullInds[RawRecord.BW2];

            linkEnd[(int)End.R].rcagl = raw.rcagl2;
            linkEnd[(int)End.R].nullInds[LinkEnd.RCAGL] = raw.nullInds[RawRecord.RCAGL2];

            linkEnd[(int)End.R].eqptMfr = raw.eqptMfr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.EQPTMFR] = raw.nullInds[RawRecord.EQPTMFR2];

            linkEnd[(int)End.R].eqptMod = raw.eqptMod2;
            linkEnd[(int)End.R].nullInds[LinkEnd.EQPTMOD] = raw.nullInds[RawRecord.EQPTMOD2];

            linkEnd[(int)End.R].eqptModDesc = raw.eqptModDesc2;
            linkEnd[(int)End.R].nullInds[LinkEnd.EQPTMODDESC] = raw.nullInds[RawRecord.EQPTMODDESC2];

            linkEnd[(int)End.R].emDes = raw.emDes2;
            linkEnd[(int)End.R].nullInds[LinkEnd.EMDES] = raw.nullInds[RawRecord.EMDES2];

            linkEnd[(int)End.R].mod = raw.mod2;
            linkEnd[(int)End.R].nullInds[LinkEnd.MOD] = raw.nullInds[RawRecord.MOD2];

            linkEnd[(int)End.R].acmMinMod = raw.acmMinMod2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ACMMINMOD] = raw.nullInds[RawRecord.ACMMINMOD2];

            linkEnd[(int)End.R].acmMaxMod = raw.acmMaxMod2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ACMMAXMOD] = raw.nullInds[RawRecord.ACMMAXMOD2];

            linkEnd[(int)End.R].dataRate = raw.dataRate2;
            linkEnd[(int)End.R].nullInds[LinkEnd.DATARATE] = raw.nullInds[RawRecord.DATARATE2];

            linkEnd[(int)End.R].atpcNomPwr = raw.atpcNomPwr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ATPCNOMPWR] = raw.nullInds[RawRecord.ATPCNOMPWR2];

            linkEnd[(int)End.R].pwr = raw.pwr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.PWR] = raw.nullInds[RawRecord.PWR2];

            linkEnd[(int)End.R].atpcMaxPwr = raw.atpcMaxPwr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ATPCMAXPWR] = raw.nullInds[RawRecord.ATPCMAXPWR2];

            linkEnd[(int)End.R].acmMinModPwr = raw.acmMinModPwr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ACMMINMODPWR] = raw.nullInds[RawRecord.ACMMINMODPWR2];

            linkEnd[(int)End.R].acmMaxModPwr = raw.acmMaxModPwr2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ACMMAXMODPWR] = raw.nullInds[RawRecord.ACMMAXMODPWR2];

            linkEnd[(int)End.R].txLoss = raw.txLoss2;
            linkEnd[(int)End.R].nullInds[LinkEnd.TXLOSS] = raw.nullInds[RawRecord.TXLOSS2];

            linkEnd[(int)End.R].rxLoss = raw.rxLoss2;
            linkEnd[(int)End.R].nullInds[LinkEnd.RXLOSS] = raw.nullInds[RawRecord.RXLOSS2];

            linkEnd[(int)End.R].cmnLoss = raw.cmnLoss2;
            linkEnd[(int)End.R].nullInds[LinkEnd.CMNLOSS] = raw.nullInds[RawRecord.CMNLOSS2];

            linkEnd[(int)End.R].atpcNomRSL = raw.atpcNomRSL2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ATPCNOMRSL] = raw.nullInds[RawRecord.ATPCNOMRSL2];

            linkEnd[(int)End.R].selectRSL = raw.selectRSL2;
            linkEnd[(int)End.R].nullInds[LinkEnd.SELECTRSL] = raw.nullInds[RawRecord.SELECTRSL2];

            linkEnd[(int)End.R].atpcMaxRSL = raw.atpcMaxRSL2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ATPCMAXRSL] = raw.nullInds[RawRecord.ATPCMAXRSL2];

            linkEnd[(int)End.R].acmMinModRSL = raw.acmMinModRSL2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ACMMINMODRSL] = raw.nullInds[RawRecord.ACMMINMODRSL2];

            linkEnd[(int)End.R].acmMaxModRSL = raw.acmMaxModRSL2;
            linkEnd[(int)End.R].nullInds[LinkEnd.ACMMAXMODRSL] = raw.nullInds[RawRecord.ACMMAXMODRSL2];

            linkEnd[(int)End.R].centerFreq = raw.centerFreq2;
            linkEnd[(int)End.R].nullInds[LinkEnd.CENTERFREQ] = raw.nullInds[RawRecord.CENTERFREQ2];

            linkEnd[(int)End.R].polar = raw.polar2;
            linkEnd[(int)End.R].nullInds[LinkEnd.POLAR] = raw.nullInds[RawRecord.POLAR2];

            // Finally identify the connectivity of the Comsearch rawRecord.
            // Determine the connectivity (one-way or two-way) of the niceRecords.
            // There is always a local transmit frequency but there may not be a remote transmit frequency.
            if (linkEnd[(int)End.R].nullInds[LinkEnd.CENTERFREQ] == Constant.DB_NULL)
            {
                connectivity = Connectivity.UNI_DIRECTIONAL;
                nullInds[NiceRecord.CONNECTIVITY] = Constant.DB_NOT_NULL;
                //...Log2.v("\nNiceRecord.NiceRecord(): UNI_DIRECTIONAL: pathID = {0}", pathID);
            }
            else
            {
                connectivity = Connectivity.BI_DIRECTIONAL;
                nullInds[NiceRecord.CONNECTIVITY] = Constant.DB_NOT_NULL;
            }

        }

        /// <summary>
        /// This method returns a string that lists the field values
        /// of this instance of a NiceRecord object together with its field null
        /// indicators.
        /// </summary>
        /// <returns></returns>
        public string ToStringWN()
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();

            sb.Append("\n ========== Scalar Parameters ==========");

            sb.Append("\n" + nullInds[i++] + "      " + "pathID       = " + pathID);
            sb.Append("\n" + nullInds[i++] + "      " + "regID        = " + regID);
            sb.Append("\n" + nullInds[i++] + "      " + "pathStat     = " + pathStat);
            sb.Append("\n" + nullInds[i++] + "      " + "regdate      = " + regdate);
            sb.Append("\n" + nullInds[i++] + "      " + "conDate      = " + conDate);
            sb.Append("\n" + nullInds[i++] + "      " + "callsign     = " + callsign);
            sb.Append("\n" + nullInds[i++] + "      " + "compName     = " + compName);
            sb.Append("\n" + nullInds[i++] + "      " + "pathLen      = " + pathLen);
            sb.Append("\n" + nullInds[i++] + "      " + "uniqueID     = " + uniqueID);
            sb.Append("\n" + nullInds[i++] + "      " + "connectivity = " + connectivity);

            sb.Append("\n\n ========== Local-End Parameters ==========\n");
            sb.Append(linkEnd[(int)End.L].ToStringWN());

            sb.Append("\n\n ========== remote-End Parameters ==========\n");
            sb.Append(linkEnd[(int)End.R].ToStringWN());

            return sb.ToString();
        }

        /// <summary>
        /// This method inputs a list of Comsearch NiceRecord objects and returns a reduced list of only those
        /// whose local and/or remote ends are within a prescribed distance of the Can-USA border.
        /// </summary>
        /// <param name="niceRecords"></param>
        /// <param name="maxDistanceToBorderKm"></param>
        /// <param name="niceRecordsNearBorder"></param>
        /// <returns></returns>
        public static int SelectRecordsNearToBorder(List<NiceRecord> niceRecords, double maxDistanceToBorderKm, out List<NiceRecord> niceRecordsNearBorder)
        {
            // 'out' requirement.
            niceRecordsNearBorder = new List<NiceRecord>();

            // Reduce the list of NiceRecords to only those within maxDistanceToBorderKm of the Canadian border.
            bool kmlLoadWasOK = CanUsaBorder.LoadKmlMapData();
            if (!kmlLoadWasOK)
            {
                Log2.e("\nComsearch.Main(): ERROR: call to LoadKmlMapData() FAILED, errMsg = {0}", CanUsaBorder.GetDetailsAsString());
                Application.ExitQuietly(1);
            }

            foreach (NiceRecord niceRec in niceRecords)
            {
                LinkEnd localEnd = niceRec.linkEnd[(int)End.L];
                LinkEnd remoteEnd = niceRec.linkEnd[(int)End.R];

                LatLng localLatLng = new LatLng(localEnd.lat, localEnd.lng);
                LatLng remoteLatLng = new LatLng(remoteEnd.lat, remoteEnd.lng);

                double localEndDistToBorder;
                double remoteEndDistToBorder;
                if (localEnd.state == "AK")
                {
                    localEndDistToBorder = CanUsaBorder.GetDistanceToAlaskanBorder(localLatLng);
                    remoteEndDistToBorder = CanUsaBorder.GetDistanceToAlaskanBorder(remoteLatLng);
                }
                else
                {
                    localEndDistToBorder = CanUsaBorder.GetDistanceToSouthernBorder(localLatLng);
                    remoteEndDistToBorder = CanUsaBorder.GetDistanceToSouthernBorder(remoteLatLng);
                }

                // If either the local or remote end of the record is within the prescribed maximum distance
                // then add it to the sub-list that is returned.
                if ((localEndDistToBorder <= maxDistanceToBorderKm) || (remoteEndDistToBorder <= maxDistanceToBorderKm))
                {
                    niceRecordsNearBorder.Add(niceRec);

                    //...Log2.v("\nNiceRecord.SelectRecordsNearToBorder(): pathID = {0}", niceRec.pathID);
                }
            }

            return niceRecordsNearBorder.Count;
        }








    }
}

