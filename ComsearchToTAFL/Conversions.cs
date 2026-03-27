using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComsearchToTAFL
{
    /// <summary>
    /// This class provides methods for converting NiceRecord objects to TAFL record objects.
    /// </summary>
    public class Conversions
    {
        private enum Mode { LOCAL_TX, REMOTE_RX, REMOTE_TX, LOCAL_RX };

        /// <summary>
        /// This method inputs a single NiceRecord object and returns a list of the 
        /// TAFL record objects that it converts into.
        /// </summary>
        /// <param name="niceRecord"></param>
        /// <param name="taflList"></param>
        public static void ComsearchToIsedTafl(NiceRecord niceRecord, out List<TAFL> taflList)
        {
            // 'out'.
            taflList = new List<TAFL>();

            //LinkEnd localEnd = niceRecord.linkEnd[(int)End.L];
            //LinkEnd remoteEnd = niceRecord.linkEnd[(int)End.R];

            // Begin with the local end transmitter.
            TAFL tafl = new TAFL();
            tafl.mNullInds = NullHelper.CreateArrayOfNullInd(TAFL.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            ComsearchToTaflHelper(Mode.LOCAL_TX, niceRecord, ref taflList);

            ComsearchToTaflHelper(Mode.REMOTE_RX, niceRecord, ref taflList);

            ComsearchToTaflHelper(Mode.REMOTE_TX, niceRecord, ref taflList);

            ComsearchToTaflHelper(Mode.LOCAL_RX, niceRecord, ref taflList);
        }

        /// <summary>
        /// This method inputs a single NiceRecord object and adds a TAFL record to a prescribed list 
        /// that corresponds to the LOCAL_TX, REMOTE_RX, REMOTE_TX, or LOCAL_RX Comsearch data fields.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="niceRecord"></param>
        /// <param name="taflList"></param>
        private static void ComsearchToTaflHelper(Mode mode, NiceRecord niceRecord, ref List<TAFL> taflList)
        {
            TAFL tafl = new TAFL();
            tafl.mNullInds = NullHelper.CreateArrayOfNullInd(TAFL.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            LinkEnd localEnd = null;
            LinkEnd remoteEnd = null;
            bool isTX = true;
            string friTag = "";

            // The conventional is that, for a Comsearch record, the local end is TX and the remote end is RX.
            switch (mode)
            {
                case Mode.LOCAL_TX:
                    isTX = true;
                    friTag = "_LTX";
                    localEnd = niceRecord.linkEnd[(int)End.L];
                    remoteEnd = niceRecord.linkEnd[(int)End.R];
                    break;
                case Mode.REMOTE_RX:
                    isTX = false;
                    friTag = "_RRX";
                    localEnd = niceRecord.linkEnd[(int)End.L];
                    remoteEnd = niceRecord.linkEnd[(int)End.R];
                    break;
                case Mode.REMOTE_TX:
                    isTX = true;
                    friTag = "_RTX";
                    localEnd = niceRecord.linkEnd[(int)End.R];
                    remoteEnd = niceRecord.linkEnd[(int)End.L];
                    break;
                case Mode.LOCAL_RX:
                    isTX = false;
                    friTag = "_LRX";
                    localEnd = niceRecord.linkEnd[(int)End.R];
                    remoteEnd = niceRecord.linkEnd[(int)End.L];
                    break;
            }


            // #1: Station function.
            tafl.TxRx = (isTX) ? "TX" : "RX";
            tafl.mNullInds[TAFL.TXRX] = Constant.DB_NOT_NULL;

            // #2: Frequency [MHz].
            tafl.FrequencyMhz = (isTX) ? localEnd.centerFreq : remoteEnd.centerFreq;
            tafl.mNullInds[TAFL.FREQUENCYMHZ] = Constant.DB_NOT_NULL;

            // #3: Frequency record identifier.
            tafl.Frequencyrecordidentifier = niceRecord.pathID.ToString() + friTag;
            tafl.mNullInds[TAFL.FREQUENCYRECORDIDENTIFIER] = Constant.DB_NOT_NULL;

            // #4: Regulatory service.
            tafl.Regulatoryservice = "2";
            tafl.mNullInds[TAFL.REGULATORYSERVICE] = Constant.DB_NOT_NULL;

            // #5: Communication type.
            tafl.CommunicationType = "D";
            tafl.mNullInds[TAFL.COMMUNICATIONTYPE] = Constant.DB_NOT_NULL;

            // #6: Conformity to frequency plan.
            tafl.Conformitytofrequencyplan = "A";
            tafl.mNullInds[TAFL.CONFORMITYTOFREQUENCYPLAN] = Constant.DB_NOT_NULL;

            // #7: Frequency allocation name.

            // #8: Channel.

            // #9: International coordination number.

            // #10: Analog/digital.
            tafl.Analogdigital = "D";
            tafl.mNullInds[TAFL.ANALOGDIGITAL] = Constant.DB_NOT_NULL;

            // #11: Occupied bandwidth [kHz].
            tafl.OccupiedbandwidthkHz = EmissionDesignator.GetBandwidthKHz(localEnd.emDes);
            tafl.mNullInds[TAFL.OCCUPIEDBANDWIDTHKHZ] = Constant.DB_NOT_NULL;

            // #12: Designation of emission.
            tafl.Designationofemission = localEnd.emDes;
            tafl.mNullInds[TAFL.DESIGNATIONOFEMISSION] = Constant.DB_NOT_NULL;

            // #13: Modulation type.
            tafl.Modulationtype = localEnd.mod;
            tafl.mNullInds[TAFL.MODULATIONTYPE] = Constant.DB_NOT_NULL;

            // #14: Filtration installed.

            double txPower_dBm = Math.Max(localEnd.atpcNomPwr, Math.Max(localEnd.pwr, Math.Max(localEnd.atpcMaxPwr, Math.Max(localEnd.acmMinModPwr, localEnd.acmMaxModPwr))));
            double txPower_dBW = txPower_dBm - 30.0;

            // #15: Tx effective radiated power (ERP) [dBW].
            //      ERP = TxPwr (dBW) - Losses (dB) + AntennaGain (dBi) - 2.15
            //      TxPwr (dBW) = TxPwr (dBm) - 30
            if (isTX)
            {
                tafl.TxeffectiveradiatedpowerERPdBW = txPower_dBW - localEnd.txLoss + localEnd.gain - 2.15; ;
                tafl.mNullInds[TAFL.TXEFFECTIVERADIATEDPOWERERPDBW] = Constant.DB_NOT_NULL;
            }

            // #16: Tx transmitter power [W].
            if (isTX)
            {
                tafl.TxtransmitterpowerW = Math.Pow(10, 0.1 * txPower_dBW);
                tafl.mNullInds[TAFL.TXTRANSMITTERPOWERW] = Constant.DB_NOT_NULL;
            }

            // #17: Total losses.
            if (isTX)
            {
                tafl.TotallossesdB = localEnd.txLoss + localEnd.cmnLoss;
                tafl.mNullInds[TAFL.TOTALLOSSESDB] = Constant.DB_NOT_NULL;
            }
            else // isRX
            {
                tafl.TotallossesdB = localEnd.rxLoss + localEnd.cmnLoss;
                tafl.mNullInds[TAFL.TOTALLOSSESDB] = Constant.DB_NOT_NULL;
            }

            // #18: Analog capacity [Channels].
            //      We will just assume that an analog voice channel occupies 4 KHz bandwidth.
            tafl.AnalogcapacityChannels = tafl.OccupiedbandwidthkHz / 4.0;
            tafl.mNullInds[TAFL.ANALOGCAPACITYCHANNELS] = Constant.DB_NOT_NULL;

            // #19: Digital capacity (Mbits).
            tafl.DigitalcapacityMbits = localEnd.dataRate;
            tafl.mNullInds[TAFL.DIGITALCAPACITYMBITS] = Constant.DB_NOT_NULL;

            // #20: Rx unfaded received signal level (dBW).
            //      Effective Isotropically Radiated Power (EIRP) = TxPowerdBm + TxAntennaGain - TxLineLossesdB.
            //      Free Space Loss (FSL) = 32.4 + 20 log(f in MHz) + 20 log(D in km).
            //      Isotropic Receive Level (IRL) = EIRP - FSL.
            //      Received Signal Level (unfaded) = IRL + RxAntennaGain - RxLineLossesdB.
            if (!isTX)
            {
                double RSLdBW = RFcalcs.RxSignalLevelUnfaded_dBW(localEnd.pwr,
                                                                        localEnd.gain,
                                                                        localEnd.txLoss + localEnd.cmnLoss,
                                                                        localEnd.centerFreq,
                                                                        niceRecord.pathLen,
                                                                        remoteEnd.gain,
                                                                        remoteEnd.rxLoss + remoteEnd.cmnLoss
                                                                        );
                tafl.RxunfadedreceivedsignalleveldBW = RSLdBW;
                tafl.mNullInds[TAFL.RXUNFADEDRECEIVEDSIGNALLEVELDBW] = Constant.DB_NOT_NULL;
            }

            // #21: Rx threshold signal level for BER10e3 (dBW).
            if (!isTX)
            {
                double bandWidth_Hz = EmissionDesignator.GetBandwidthHz(remoteEnd.emDes);
                double bitRate_Hz = remoteEnd.dataRate * 1E6;
                double enbw_Hz = bandWidth_Hz;
                tafl.RxthresholdsignallevelforBER10e3dBW = RFcalcs.RxThresholdSignalLevelForBER_dBW(
                                                                            remoteEnd.mod,
                                                                            bandWidth_Hz,
                                                                            bitRate_Hz,
                                                                            enbw_Hz,
                                                                            1.0E-3
                                                                            );
                tafl.mNullInds[TAFL.RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] = Constant.DB_NOT_NULL;
            }

            // #22: Manufacturer.
            tafl.Manufacturer = localEnd.antMfr;
            tafl.mNullInds[TAFL.MANUFACTURER] = Constant.DB_NOT_NULL;

            // #23: Model number.
            tafl.Modelnumber = localEnd.antMod;
            tafl.mNullInds[TAFL.MODELNUMBER] = Constant.DB_NOT_NULL;

            // #24: Antenna gain(dBi).
            tafl.AntennagaindBi = localEnd.gain;
            tafl.mNullInds[TAFL.ANTENNAGAINDBI] = Constant.DB_NOT_NULL;

            // #25: Antenna pattern.
            //      Use "A" for Parabolic (Full: Solid).
            tafl.Antennapattern = "A";
            tafl.mNullInds[TAFL.ANTENNAPATTERN] = Constant.DB_NOT_NULL;

            // #26: Half-power, 3dB, beamwidth(deg).
            tafl.Halfpower3dBbeamwidthdeg = localEnd.bw;
            tafl.mNullInds[TAFL.HALFPOWER3DBBEAMWIDTHDEG] = Constant.DB_NOT_NULL;

            // #27: Front to back ratio(dB). (Not populated in ISED/TAFL csv.)

            // #28: Polarization.  ("A" = horizontal; "B" = vertical;)
            if (!String.IsNullOrWhiteSpace(localEnd.polar))
            {
                switch (localEnd.polar.ToUpper())
                {
                    case "H":
                        tafl.Polarization = "A";
                        break;
                    case "V":
                        tafl.Polarization = "B";
                        break;
                    default:
                        tafl.Polarization = "";
                        break;
                }
                tafl.mNullInds[TAFL.POLARIZATION] = Constant.DB_NOT_NULL;
            }

            // #29: Height above ground level(m).
            //      RCAGL = Radiation Centre Above Ground Level.
            tafl.Heightabovegroundlevelm = localEnd.rcagl;
            tafl.mNullInds[TAFL.HEIGHTABOVEGROUNDLEVELM] = Constant.DB_NOT_NULL;

            double pathDistKm;
            double azimuthDeg;
            double vertElevationDeg;
            LinkEnd.RelativeGeometry(localEnd, remoteEnd, out pathDistKm, out azimuthDeg, out vertElevationDeg);

            // #30: Azimuth of main lobe (deg).
            tafl.Azimuthofmainlobedeg = azimuthDeg;
            tafl.mNullInds[TAFL.AZIMUTHOFMAINLOBEDEG] = Constant.DB_NOT_NULL;

            // #31: Vertical elevation angle (deg).
            tafl.Verticalelevationangledeg = vertElevationDeg;
            tafl.mNullInds[TAFL.VERTICALELEVATIONANGLEDEG] = Constant.DB_NOT_NULL;

            // #32: Station location.
            tafl.Stationlocation = localEnd.site;
            tafl.mNullInds[TAFL.STATIONLOCATION] = Constant.DB_NOT_NULL;

            // #33: Licensee station reference.

            // #34: Call sign.
            tafl.Callsign = niceRecord.callsign;
            tafl.mNullInds[TAFL.CALLSIGN] = Constant.DB_NOT_NULL;

            // #35: Type of station. (ISED/TAFL code 1 = FIXED.)
            tafl.Typeofstation = "1";
            tafl.mNullInds[TAFL.TYPEOFSTATION] = Constant.DB_NOT_NULL;

            // #36: ITU class of station. (FX = fixed station.)
            tafl.ITUclassofstation = "FX";
            tafl.mNullInds[TAFL.ITUCLASSOFSTATION] = Constant.DB_NOT_NULL;

            // #37: Station cost category. (3 = foreign station.)
            tafl.Stationcostcategory = "3";
            tafl.mNullInds[TAFL.STATIONCOSTCATEGORY] = Constant.DB_NOT_NULL;

            // #38: Number of identical stations.

            // #39: Reference identifier.

            // #40: Province(s).
            tafl.Provinces = localEnd.state;
            tafl.mNullInds[TAFL.PROVINCES] = Constant.DB_NOT_NULL;

            // #41: Latitude WGS84 (deg).
            tafl.LatitudeWGS84 = localEnd.lat;
            tafl.mNullInds[TAFL.LATITUDEWGS84] = Constant.DB_NOT_NULL;

            // #42: Longitude WGS84 (deg).
            tafl.LongitudeWGS84 = localEnd.lng;
            tafl.mNullInds[TAFL.LONGITUDEWGS84] = Constant.DB_NOT_NULL;

            // #43: Ground elevation above mean sea level (m).
            tafl.Groundelevationabovemeansealevelm = localEnd.grndElev;
            tafl.mNullInds[TAFL.GROUNDELEVATIONABOVEMEANSEALEVELM] = Constant.DB_NOT_NULL;

            // #44: Antenna structure height above ground level (m).
            tafl.Antennastructureheightabovegroundlevelm = localEnd.rcagl;
            tafl.mNullInds[TAFL.ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM] = Constant.DB_NOT_NULL;

            // #45: Congestion zone.

            // #46: Radius of operation (km).

            // #47: Satellite name;

            // #48: Authorization number.
            tafl.Authorizationnumber = niceRecord.regID;
            tafl.mNullInds[TAFL.AUTHORIZATIONNUMBER] = Constant.DB_NOT_NULL;

            // #49: MW Service. (2 = fixed.)
            tafl.MWService = "2";
            tafl.mNullInds[TAFL.MWSERVICE] = Constant.DB_NOT_NULL;

            // #50: Sub-service. (200 = point-to-point.)
            tafl.Subservice = "200";
            tafl.mNullInds[TAFL.SUBSERVICE] = Constant.DB_NOT_NULL;

            // #51: Licence type. (S = non-developmental.)
            tafl.Licencetype = "S";
            tafl.mNullInds[TAFL.LICENCETYPE] = Constant.DB_NOT_NULL;

            // #52: Authorization status. (G = granted.)
            tafl.Authorizationstatus = "G";
            tafl.mNullInds[TAFL.AUTHORIZATIONSTATUS] = Constant.DB_NOT_NULL;

            // #53: In-service date.
            tafl.Inservicedate = (String.IsNullOrWhiteSpace(niceRecord.conDate)) ? niceRecord.regdate : niceRecord.conDate;
            tafl.mNullInds[TAFL.INSERVICEDATE] = Constant.DB_NOT_NULL;

            // #54: Account number.
            tafl.Accountnumber = niceRecord.regID;
            tafl.mNullInds[TAFL.ACCOUNTNUMBER] = Constant.DB_NOT_NULL;

            // #55: Licensee name.
            tafl.Licenseename = niceRecord.compName;
            tafl.mNullInds[TAFL.LICENSEENAME] = Constant.DB_NOT_NULL;

            // #56: Licensee address.

            // #57: Operational status.

            // #58: Station class.

            // #59: Horizontal power (W).
            tafl.HorizontalpowerW = 0.0;
            tafl.mNullInds[TAFL.HORIZONTALPOWERW] = Constant.DB_NOT_NULL;

            // #60:Vertical power (W).
            tafl.VerticalpowerW = 0.0;
            tafl.mNullInds[TAFL.VERTICALPOWERW] = Constant.DB_NOT_NULL;

            // #61: Standby transmitter information.

            //----------------------------------------------
            // Finally, add the new TAFL object to the list.
            //----------------------------------------------
            taflList.Add(tafl);
        }

        /// <summary>
        /// This method converts the Comsearch date-time format string to the format
        /// used in the ISED TAFL data.
        /// </summary>
        /// <param name="comsearchDate"></param>
        /// <returns></returns>
        public static string ComsearchDateToTAFL(string comsearchDate)
        {
            string fcsaDate = "";
            char[] delimeters = new char[] { '-', '\\', '/', '.' };

            try
            {
                if (!String.IsNullOrWhiteSpace(comsearchDate))
                {

                    string[] fields = comsearchDate.Trim().Split(delimeters);
                    //                                      year       month      day
                    fcsaDate = String.Format("{0}-{1}-{2}", fields[0], fields[1], fields[2]);
                }
            }
            catch (Exception e)
            {
                string msg = String.Format("\n\nConversions.ComsearchDateToFCSA(): ERROR: comsearchDate = |{0}| threw exception: {1}", comsearchDate, e.Message);
                Log2.e(msg);
                Console.Write(msg);
                Application.ExitQuietly(2);
            }

            return fcsaDate;
        }



    }
}
