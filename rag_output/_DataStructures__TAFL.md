# Documented File: TAFL.cs
**Repository Path:** `_DataStructures\TAFL.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
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
    using SQLUINTEGER = UInt32;
    using _Configuration;
    
    /// <summary>
    /// This class encapsulates the column fields in an ISED-provided TS TAFL CSV data file.
    /// </summary>
    public class TAFL
    {
        //==================================================================================
        // Non-static members.
        //==================================================================================
        public string TxRx = "";
        public double FrequencyMhz = 0;
        public string Frequencyrecordidentifier = "";
        public string Regulatoryservice = "";
        public string CommunicationType = "";
        public string Conformitytofrequencyplan = "";
        public string Frequencyallocationname = "";
        public string Channel = "";
        public string Internationalcoordinationnumber = "";
        public string Analogdigital = "";
        public double OccupiedbandwidthkHz = 0;
        public string Designationofemission = "";
        public string Modulationtype = "";
        public string Filtrationinstalled = "";
        public double TxeffectiveradiatedpowerERPdBW = 0;
        public double TxtransmitterpowerW = 0;
        public double TotallossesdB = 0;
        public double AnalogcapacityChannels = 0;
        public double DigitalcapacityMbits = 0;
        public double RxunfadedreceivedsignalleveldBW = 0;
        public double RxthresholdsignallevelforBER10e3dBW = 0;
        public string Manufacturer = "";
        public string Modelnumber = "";
        public double AntennagaindBi = 0;
        public string Antennapattern = "";
        public double Halfpower3dBbeamwidthdeg = 0;
        public double FronttobackratiodB = 0;
        public string Polarization = "";
        public double Heightabovegroundlevelm = 0;
        public double Azimuthofmainlobedeg = 0;
        public double Verticalelevationangledeg = 0;
        public string Stationlocation = "";
        public string Licenseestationreference = "";
        public string Callsign = "";
        public string Typeofstation = "";
        public string ITUclassofstation = "";
        public string Stationcostcategory = "";
        public int Numberofidenticalstations = 0;
        public string Referenceidentifier = "";
        public string Provinces = "";
        public double LatitudeWGS84 = 0;
        public double LongitudeWGS84 = 0;
        public double Groundelevationabovemeansealevelm = 0;
        public double Antennastructureheightabovegroundlevelm = 0;
        public string Congestionzone = "";
        public double Radiusofoperationkm = 0;
        public string Satellitename = "";
        public string Authorizationnumber = "";
        public string MWService = "";
        public string Subservice = "";
        public string Licencetype = "";
        public string Authorizationstatus = "";
        public string Inservicedate = "";
        public string Accountnumber = "";
        public string Licenseename = "";
        public string Licenseeaddress = "";
        public string Operationalstatus = "";
        public string Stationclass = "";
        public double HorizontalpowerW = 0;
        public double VerticalpowerW = 0;
        public string Standbytransmitterinformation = "";
        public string MICSoper = "";
        public string MICSopnote = "";
        public string MICSoprtyp = "";
        public string MICScompany = "";
        public string RecordAction = "";
        public int Keyfield = 0;

        // For conveience, include an array of null-indicators.
        public SQLLEN[] mNullInds;

        //==================================================================================
        // The total number of fields corresponding to database columns.
        //==================================================================================
        public const int NUM_COLUMNS = 67;

        public const int NUM_ISED_TAFL_COLUMNS = 61;

        //==================================================================================
        // Array of strings providing the class-member / database-column names.
        //
        // Note: 
        //       - the member name 'TxRx' corresponds to the SQL table column name 'TXRX';
        //       - the member name 'Keyfield' corresponds to the SQL table column name 'keyfield';
        //       - all the other member names are identical to their SQL table column name.
        //==================================================================================
        private static string[] columnNames = new string[NUM_COLUMNS] {
                "TXRX",
                "FrequencyMhz",
                "Frequencyrecordidentifier",
                "Regulatoryservice",
                "CommunicationType",
                "Conformitytofrequencyplan",
                "Frequencyallocationname",
                "Channel",
                "Internationalcoordinationnumber",
                "Analogdigital",
                "OccupiedbandwidthkHz",
                "Designationofemission",
                "Modulationtype",
                "Filtrationinstalled",
                "TxeffectiveradiatedpowerERPdBW",
                "TxtransmitterpowerW",
                "TotallossesdB",
                "AnalogcapacityChannels",
                "DigitalcapacityMbits",
                "RxunfadedreceivedsignalleveldBW",
                "RxthresholdsignallevelforBER10e3dBW",
                "Manufacturer",
                "Modelnumber",
                "AntennagaindBi",
                "Antennapattern",
                "Halfpower3dBbeamwidthdeg",
                "FronttobackratiodB",
                "Polarization",
                "Heightabovegroundlevelm",
                "Azimuthofmainlobedeg",
                "Verticalelevationangledeg",
                "Stationlocation",
                "Licenseestationreference",
                "Callsign",
                "Typeofstation",
                "ITUclassofstation",
                "Stationcostcategory",
                "Numberofidenticalstations",
                "Referenceidentifier",
                "Provinces",
                "LatitudeWGS84",
                "LongitudeWGS84",
                "Groundelevationabovemeansealevelm",
                "Antennastructureheightabovegroundlevelm",
                "Congestionzone",
                "Radiusofoperationkm",
                "Satellitename",
                "Authorizationnumber",
                "MWService",
                "Subservice",
                "Licencetype",
                "Authorizationstatus",
                "Inservicedate",
                "Accountnumber",
                "Licenseename",
                "Licenseeaddress",
                "Operationalstatus",
                "Stationclass",
                "HorizontalpowerW",
                "VerticalpowerW",
                "Standbytransmitterinformation",
                "MICSoper",
                "MICSopnote",
                "MICSoprtyp",
                "MICScompany",
                "RecordAction",
                "keyfield"
            };

        public static string[] mNumberedColumnNames = new string[NUM_COLUMNS] {"#1 TXRX","#2 Frequency (Mhz)","#3 Frequency record identifier","#4 Regulatory service","#5 Communication Type","#6 Conformity to frequency plan","#7 Frequency allocation name","#8 Channel","#9 International coordination number","#10 Analog or digital","#11 Occupied bandwidth (kHz)","#12 Designation of emission","#13 Modulation type","#14 Filtration installed","#15 Tx effective radiated power ERP (dBW)","#16 Tx transmitter power (W)","#17 Total losses (dB)","#18 Analog capacity Channels","#19 Digital capacity (Mbits)","#20 Rx unfaded received signal level (dBW)","#21 Rx threshold signal level for BER10e3 (dBW)","#22 Manufacturer","#23 Model number","#24 Antenna gain (dBi)","#25 Antenna pattern","#26 Half-power, 3dB, beamwidth (deg)","#27 Front to back ratio (dB)","#28 Polarization","#29 Height above ground level (m)","#30 Azimuth of main lobe (deg)","#31 Vertical elevation angle (deg)","#32 Station location","#33 Licensee station reference","#34 Call sign","#35 Type of station","#36 ITU class of station","#37 Station cost category","#38 Number of identical stations","#39 Reference identifier","#40 Provinces","#41 Latitude WGS84 (deg)","#42 Longitude WGS84 (deg)","#43 Ground elevation above mean sea level (m)","#44 Antenna structure height above ground level (m)","#45 Congestion zone","#46 Radius of operation (km)","#47 Satellite name","#48 Authorization number","#49 MW Service","#50 Sub-service","#51 Licence type","#52 Authorization status","#53 In-service date","#54 Account number","#55 Licensee name","#56 Licensee address","#57 Operational status","#58 Station class","#59 Horizontal power (W)","#60 Vertical power (W)","#61 Standby transmitter information","MICS oper","MICS opnote", "MICS oprtype", "MICS company","Record Action","keyfield"};

        //List of all column names for SQL 'select' command, i.e. " cmd, recstat, call1, call2, bndcde, anum, ... , mtime " 
        //Provide read-only access to this 'constant'.
        private static string mAllFieldsAsCSV;
        public static string AllColumnsForSqlSelect
        {
            get { return mAllFieldsAsCSV; }
        }

        //List of all column names with bindings for SQL 'update' command, i.e. " cmd=?, recstat=?, call1=?, call2=?, bndcde=?, anum=?, ... , mtime=? "
        //Provide read-only access to this 'constant'.
        private static string mAllBindingsAsCSV;
        public static string AllColumnsForSqlUpdateAsBindings
        {
            get { return mAllBindingsAsCSV; }
        }

        //----------------------------------------------------------------

        /// <summary>
        /// The static constructor is automatically called once, before any
        /// instance constructor is invoked or member is accessed.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        static TAFL()
        {
            mAllFieldsAsCSV = ListOfAllColumnNamesForSQLSelect();
            mAllBindingsAsCSV = ListOfAllColumnNamesForSQLUpdate();

            SQLPOINTER[] ParameterValuePtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLPOINTER>(NUM_COLUMNS);
            SQLLENPTR[] StrLen_or_IndPtr = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLENPTR>(NUM_COLUMNS);
        }

        /// <summary>
        /// Default object constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TAFL()
        {
        }


        //==================================================================================
        // Zero-based index.
        //==================================================================================

        // These are the fields defined in ISED's document:
        // "Spectrum Management System (SMS) Authorization Data Extract – Field Descriptions".
        // Qty. 61 ISED-defined fields.
        public const int TXRX = 0;
        public const int FREQUENCYMHZ = 1;
        public const int FREQUENCYRECORDIDENTIFIER = 2;
        public const int REGULATORYSERVICE = 3;
        public const int COMMUNICATIONTYPE = 4;
        public const int CONFORMITYTOFREQUENCYPLAN = 5;
        public const int FREQUENCYALLOCATIONNAME = 6;
        public const int CHANNEL = 7;
        public const int INTERNATIONALCOORDINATIONNUMBER = 8;
        public const int ANALOGDIGITAL = 9;
        public const int OCCUPIEDBANDWIDTHKHZ = 10;
        public const int DESIGNATIONOFEMISSION = 11;
        public const int MODULATIONTYPE = 12;
        public const int FILTRATIONINSTALLED = 13;
        public const int TXEFFECTIVERADIATEDPOWERERPDBW = 14;
        public const int TXTRANSMITTERPOWERW = 15;
        public const int TOTALLOSSESDB = 16;
        public const int ANALOGCAPACITYCHANNELS = 17;
        public const int DIGITALCAPACITYMBITS = 18;
        public const int RXUNFADEDRECEIVEDSIGNALLEVELDBW = 19;
        public const int RXTHRESHOLDSIGNALLEVELFORBER10E3DBW = 20;
        public const int MANUFACTURER = 21;
        public const int MODELNUMBER = 22;
        public const int ANTENNAGAINDBI = 23;
        public const int ANTENNAPATTERN = 24;
        public const int HALFPOWER3DBBEAMWIDTHDEG = 25;
        public const int FRONTTOBACKRATIODB = 26;
        public const int POLARIZATION = 27;
        public const int HEIGHTABOVEGROUNDLEVELM = 28;
        public const int AZIMUTHOFMAINLOBEDEG = 29;
        public const int VERTICALELEVATIONANGLEDEG = 30;
        public const int STATIONLOCATION = 31;
        public const int LICENSEESTATIONREFERENCE = 32;
        public const int CALLSIGN = 33;
        public const int TYPEOFSTATION = 34;
        public const int ITUCLASSOFSTATION = 35;
        public const int STATIONCOSTCATEGORY = 36;
        public const int NUMBEROFIDENTICALSTATIONS = 37;
        public const int REFERENCEIDENTIFIER = 38;
        public const int PROVINCES = 39;
        public const int LATITUDEWGS84 = 40;
        public const int LONGITUDEWGS84 = 41;
        public const int GROUNDELEVATIONABOVEMEANSEALEVELM = 42;
        public const int ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM = 43;
        public const int CONGESTIONZONE = 44;
        public const int RADIUSOFOPERATIONKM = 45;
        public const int SATELLITENAME = 46;
        public const int AUTHORIZATIONNUMBER = 47;
        public const int MWSERVICE = 48;
        public const int SUBSERVICE = 49;
        public const int LICENCETYPE = 50;
        public const int AUTHORIZATIONSTATUS = 51;
        public const int INSERVICEDATE = 52;
        public const int ACCOUNTNUMBER = 53;
        public const int LICENSEENAME = 54;
        public const int LICENSEEADDRESS = 55;
        public const int OPERATIONALSTATUS = 56;
        public const int STATIONCLASS = 57;
        public const int HORIZONTALPOWERW = 58;
        public const int VERTICALPOWERW = 59;
        public const int STANDBYTRANSMITTERINFORMATION = 60;

        // These are fields that Bill appends to the ISED-defined fields to facilitate
        // FCSA import of the ISED/TAFL data.
        public const int MICSOPER = 61;
        public const int MICSOPNOTE = 62;
        public const int MICSOPRTYP = 63;
        public const int MICSCOMPANY = 64;
        public const int RECORDACTION = 65;
        public const int KEYFIELD = 66;

        //==================================================================================
        // String sizes.
        //==================================================================================
        public const int TXRX_SZ = 3;
        public const int FREQUENCYRECORDIDENTIFIER_SZ = 256;
        public const int REGULATORYSERVICE_SZ = 256;
        public const int COMMUNICATIONTYPE_SZ = 256;
        public const int CONFORMITYTOFREQUENCYPLAN_SZ = 256;
        public const int FREQUENCYALLOCATIONNAME_SZ = 256;
        public const int CHANNEL_SZ = 256;
        public const int INTERNATIONALCOORDINATIONNUMBER_SZ = 256;
        public const int ANALOGDIGITAL_SZ = 256;
        public const int DESIGNATIONOFEMISSION_SZ = 256;
        public const int MODULATIONTYPE_SZ = 256;
        public const int FILTRATIONINSTALLED_SZ = 256;
        public const int MANUFACTURER_SZ = 256;
        public const int MODELNUMBER_SZ = 256;
        public const int ANTENNAPATTERN_SZ = 256;
        public const int POLARIZATION_SZ = 256;
        public const int STATIONLOCATION_SZ = 256;
        public const int LICENSEESTATIONREFERENCE_SZ = 256;
        public const int CALLSIGN_SZ = 256;
        public const int TYPEOFSTATION_SZ = 256;
        public const int ITUCLASSOFSTATION_SZ = 256;
        public const int STATIONCOSTCATEGORY_SZ = 256;
        public const int REFERENCEIDENTIFIER_SZ = 256;
        public const int PROVINCES_SZ = 256;
        public const int CONGESTIONZONE_SZ = 256;
        public const int SATELLITENAME_SZ = 256;
        public const int AUTHORIZATIONNUMBER_SZ = 256;
        public const int MWSERVICE_SZ = 256;
        public const int SUBSERVICE_SZ = 5;
        public const int LICENCETYPE_SZ = 256;
        public const int AUTHORIZATIONSTATUS_SZ = 256;
        public const int INSERVICEDATE_SZ = 256;
        public const int ACCOUNTNUMBER_SZ = 256;
        public const int LICENSEENAME_SZ = 256;
        public const int LICENSEEADDRESS_SZ = 256;
        public const int OPERATIONALSTATUS_SZ = 256;
        public const int STATIONCLASS_SZ = 256;
        public const int STANDBYTRANSMITTERINFORMATION_SZ = 256;
        public const int MICSOPER_SZ = 256;
        public const int MICSOPNOTE_SZ = 3;
        public const int MICSOPRTYP_SZ = 3;
        public const int MICSCOMPANY_SZ = 256;
        public const int RECORDACTION_SZ = 3;

        //==================================================================================
        // Methods.
        //==================================================================================

        /// <summary>
        /// This method returns an array of IntPtr that point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// The binding order is that of the columns in the table main.mt_ante.
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

            tgtValPtrs[TXRX] = Marshal.AllocHGlobal(TXRX_SZ + 1);
            nullIndPtrs[TXRX] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TXRX + 1, tgtValPtrs[TXRX], TAFL.TXRX_SZ, nullIndPtrs[TXRX]);

            tgtValPtrs[FREQUENCYMHZ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[FREQUENCYMHZ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, FREQUENCYMHZ + 1, tgtValPtrs[FREQUENCYMHZ], nullIndPtrs[FREQUENCYMHZ]);

            tgtValPtrs[FREQUENCYRECORDIDENTIFIER] = Marshal.AllocHGlobal(FREQUENCYRECORDIDENTIFIER_SZ + 1);
            nullIndPtrs[FREQUENCYRECORDIDENTIFIER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FREQUENCYRECORDIDENTIFIER + 1, tgtValPtrs[FREQUENCYRECORDIDENTIFIER], TAFL.FREQUENCYRECORDIDENTIFIER_SZ, nullIndPtrs[FREQUENCYRECORDIDENTIFIER]);

            tgtValPtrs[REGULATORYSERVICE] = Marshal.AllocHGlobal(REGULATORYSERVICE_SZ + 1);
            nullIndPtrs[REGULATORYSERVICE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, REGULATORYSERVICE + 1, tgtValPtrs[REGULATORYSERVICE], TAFL.REGULATORYSERVICE_SZ, nullIndPtrs[REGULATORYSERVICE]);

            tgtValPtrs[COMMUNICATIONTYPE] = Marshal.AllocHGlobal(COMMUNICATIONTYPE_SZ + 1);
            nullIndPtrs[COMMUNICATIONTYPE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, COMMUNICATIONTYPE + 1, tgtValPtrs[COMMUNICATIONTYPE], TAFL.COMMUNICATIONTYPE_SZ, nullIndPtrs[COMMUNICATIONTYPE]);

            tgtValPtrs[CONFORMITYTOFREQUENCYPLAN] = Marshal.AllocHGlobal(CONFORMITYTOFREQUENCYPLAN_SZ + 1);
            nullIndPtrs[CONFORMITYTOFREQUENCYPLAN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CONFORMITYTOFREQUENCYPLAN + 1, tgtValPtrs[CONFORMITYTOFREQUENCYPLAN], TAFL.CONFORMITYTOFREQUENCYPLAN_SZ, nullIndPtrs[CONFORMITYTOFREQUENCYPLAN]);

            tgtValPtrs[FREQUENCYALLOCATIONNAME] = Marshal.AllocHGlobal(FREQUENCYALLOCATIONNAME_SZ + 1);
            nullIndPtrs[FREQUENCYALLOCATIONNAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FREQUENCYALLOCATIONNAME + 1, tgtValPtrs[FREQUENCYALLOCATIONNAME], TAFL.FREQUENCYALLOCATIONNAME_SZ, nullIndPtrs[FREQUENCYALLOCATIONNAME]);

            tgtValPtrs[CHANNEL] = Marshal.AllocHGlobal(CHANNEL_SZ + 1);
            nullIndPtrs[CHANNEL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CHANNEL + 1, tgtValPtrs[CHANNEL], TAFL.CHANNEL_SZ, nullIndPtrs[CHANNEL]);

            tgtValPtrs[INTERNATIONALCOORDINATIONNUMBER] = Marshal.AllocHGlobal(INTERNATIONALCOORDINATIONNUMBER_SZ + 1);
            nullIndPtrs[INTERNATIONALCOORDINATIONNUMBER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, INTERNATIONALCOORDINATIONNUMBER + 1, tgtValPtrs[INTERNATIONALCOORDINATIONNUMBER], TAFL.INTERNATIONALCOORDINATIONNUMBER_SZ, nullIndPtrs[INTERNATIONALCOORDINATIONNUMBER]);

            tgtValPtrs[ANALOGDIGITAL] = Marshal.AllocHGlobal(ANALOGDIGITAL_SZ + 1);
            nullIndPtrs[ANALOGDIGITAL] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANALOGDIGITAL + 1, tgtValPtrs[ANALOGDIGITAL], TAFL.ANALOGDIGITAL_SZ, nullIndPtrs[ANALOGDIGITAL]);

            tgtValPtrs[OCCUPIEDBANDWIDTHKHZ] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[OCCUPIEDBANDWIDTHKHZ] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, OCCUPIEDBANDWIDTHKHZ + 1, tgtValPtrs[OCCUPIEDBANDWIDTHKHZ], nullIndPtrs[OCCUPIEDBANDWIDTHKHZ]);

            tgtValPtrs[DESIGNATIONOFEMISSION] = Marshal.AllocHGlobal(DESIGNATIONOFEMISSION_SZ + 1);
            nullIndPtrs[DESIGNATIONOFEMISSION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, DESIGNATIONOFEMISSION + 1, tgtValPtrs[DESIGNATIONOFEMISSION], TAFL.DESIGNATIONOFEMISSION_SZ, nullIndPtrs[DESIGNATIONOFEMISSION]);

            tgtValPtrs[MODULATIONTYPE] = Marshal.AllocHGlobal(MODULATIONTYPE_SZ + 1);
            nullIndPtrs[MODULATIONTYPE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MODULATIONTYPE + 1, tgtValPtrs[MODULATIONTYPE], TAFL.MODULATIONTYPE_SZ, nullIndPtrs[MODULATIONTYPE]);

            tgtValPtrs[FILTRATIONINSTALLED] = Marshal.AllocHGlobal(FILTRATIONINSTALLED_SZ + 1);
            nullIndPtrs[FILTRATIONINSTALLED] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FILTRATIONINSTALLED + 1, tgtValPtrs[FILTRATIONINSTALLED], TAFL.FILTRATIONINSTALLED_SZ, nullIndPtrs[FILTRATIONINSTALLED]);

            tgtValPtrs[TXEFFECTIVERADIATEDPOWERERPDBW] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[TXEFFECTIVERADIATEDPOWERERPDBW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, TXEFFECTIVERADIATEDPOWERERPDBW + 1, tgtValPtrs[TXEFFECTIVERADIATEDPOWERERPDBW], nullIndPtrs[TXEFFECTIVERADIATEDPOWERERPDBW]);

            tgtValPtrs[TXTRANSMITTERPOWERW] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[TXTRANSMITTERPOWERW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, TXTRANSMITTERPOWERW + 1, tgtValPtrs[TXTRANSMITTERPOWERW], nullIndPtrs[TXTRANSMITTERPOWERW]);

            tgtValPtrs[TOTALLOSSESDB] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[TOTALLOSSESDB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, TOTALLOSSESDB + 1, tgtValPtrs[TOTALLOSSESDB], nullIndPtrs[TOTALLOSSESDB]);

            tgtValPtrs[ANALOGCAPACITYCHANNELS] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ANALOGCAPACITYCHANNELS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ANALOGCAPACITYCHANNELS + 1, tgtValPtrs[ANALOGCAPACITYCHANNELS], nullIndPtrs[ANALOGCAPACITYCHANNELS]);

            tgtValPtrs[DIGITALCAPACITYMBITS] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[DIGITALCAPACITYMBITS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, DIGITALCAPACITYMBITS + 1, tgtValPtrs[DIGITALCAPACITYMBITS], nullIndPtrs[DIGITALCAPACITYMBITS]);

            tgtValPtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RXUNFADEDRECEIVEDSIGNALLEVELDBW + 1, tgtValPtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW], nullIndPtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW]);

            tgtValPtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RXTHRESHOLDSIGNALLEVELFORBER10E3DBW + 1, tgtValPtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW], nullIndPtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW]);

            tgtValPtrs[MANUFACTURER] = Marshal.AllocHGlobal(MANUFACTURER_SZ + 1);
            nullIndPtrs[MANUFACTURER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MANUFACTURER + 1, tgtValPtrs[MANUFACTURER], TAFL.MANUFACTURER_SZ, nullIndPtrs[MANUFACTURER]);

            tgtValPtrs[MODELNUMBER] = Marshal.AllocHGlobal(MODELNUMBER_SZ + 1);
            nullIndPtrs[MODELNUMBER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MODELNUMBER + 1, tgtValPtrs[MODELNUMBER], TAFL.MODELNUMBER_SZ, nullIndPtrs[MODELNUMBER]);

            tgtValPtrs[ANTENNAGAINDBI] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ANTENNAGAINDBI] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ANTENNAGAINDBI + 1, tgtValPtrs[ANTENNAGAINDBI], nullIndPtrs[ANTENNAGAINDBI]);

            tgtValPtrs[ANTENNAPATTERN] = Marshal.AllocHGlobal(ANTENNAPATTERN_SZ + 1);
            nullIndPtrs[ANTENNAPATTERN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ANTENNAPATTERN + 1, tgtValPtrs[ANTENNAPATTERN], TAFL.ANTENNAPATTERN_SZ, nullIndPtrs[ANTENNAPATTERN]);

            tgtValPtrs[HALFPOWER3DBBEAMWIDTHDEG] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[HALFPOWER3DBBEAMWIDTHDEG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, HALFPOWER3DBBEAMWIDTHDEG + 1, tgtValPtrs[HALFPOWER3DBBEAMWIDTHDEG], nullIndPtrs[HALFPOWER3DBBEAMWIDTHDEG]);

            tgtValPtrs[FRONTTOBACKRATIODB] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[FRONTTOBACKRATIODB] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, FRONTTOBACKRATIODB + 1, tgtValPtrs[FRONTTOBACKRATIODB], nullIndPtrs[FRONTTOBACKRATIODB]);

            tgtValPtrs[POLARIZATION] = Marshal.AllocHGlobal(POLARIZATION_SZ + 1);
            nullIndPtrs[POLARIZATION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, POLARIZATION + 1, tgtValPtrs[POLARIZATION], TAFL.POLARIZATION_SZ, nullIndPtrs[POLARIZATION]);

            tgtValPtrs[HEIGHTABOVEGROUNDLEVELM] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[HEIGHTABOVEGROUNDLEVELM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, HEIGHTABOVEGROUNDLEVELM + 1, tgtValPtrs[HEIGHTABOVEGROUNDLEVELM], nullIndPtrs[HEIGHTABOVEGROUNDLEVELM]);

            tgtValPtrs[AZIMUTHOFMAINLOBEDEG] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[AZIMUTHOFMAINLOBEDEG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, AZIMUTHOFMAINLOBEDEG + 1, tgtValPtrs[AZIMUTHOFMAINLOBEDEG], nullIndPtrs[AZIMUTHOFMAINLOBEDEG]);

            tgtValPtrs[VERTICALELEVATIONANGLEDEG] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[VERTICALELEVATIONANGLEDEG] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, VERTICALELEVATIONANGLEDEG + 1, tgtValPtrs[VERTICALELEVATIONANGLEDEG], nullIndPtrs[VERTICALELEVATIONANGLEDEG]);

            tgtValPtrs[STATIONLOCATION] = Marshal.AllocHGlobal(STATIONLOCATION_SZ + 1);
            nullIndPtrs[STATIONLOCATION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATIONLOCATION + 1, tgtValPtrs[STATIONLOCATION], TAFL.STATIONLOCATION_SZ, nullIndPtrs[STATIONLOCATION]);

            tgtValPtrs[LICENSEESTATIONREFERENCE] = Marshal.AllocHGlobal(LICENSEESTATIONREFERENCE_SZ + 1);
            nullIndPtrs[LICENSEESTATIONREFERENCE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LICENSEESTATIONREFERENCE + 1, tgtValPtrs[LICENSEESTATIONREFERENCE], TAFL.LICENSEESTATIONREFERENCE_SZ, nullIndPtrs[LICENSEESTATIONREFERENCE]);

            tgtValPtrs[CALLSIGN] = Marshal.AllocHGlobal(CALLSIGN_SZ + 1);
            nullIndPtrs[CALLSIGN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CALLSIGN + 1, tgtValPtrs[CALLSIGN], TAFL.CALLSIGN_SZ, nullIndPtrs[CALLSIGN]);

            tgtValPtrs[TYPEOFSTATION] = Marshal.AllocHGlobal(TYPEOFSTATION_SZ + 1);
            nullIndPtrs[TYPEOFSTATION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, TYPEOFSTATION + 1, tgtValPtrs[TYPEOFSTATION], TAFL.TYPEOFSTATION_SZ, nullIndPtrs[TYPEOFSTATION]);

            tgtValPtrs[ITUCLASSOFSTATION] = Marshal.AllocHGlobal(ITUCLASSOFSTATION_SZ + 1);
            nullIndPtrs[ITUCLASSOFSTATION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ITUCLASSOFSTATION + 1, tgtValPtrs[ITUCLASSOFSTATION], TAFL.ITUCLASSOFSTATION_SZ, nullIndPtrs[ITUCLASSOFSTATION]);

            tgtValPtrs[STATIONCOSTCATEGORY] = Marshal.AllocHGlobal(STATIONCOSTCATEGORY_SZ + 1);
            nullIndPtrs[STATIONCOSTCATEGORY] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATIONCOSTCATEGORY + 1, tgtValPtrs[STATIONCOSTCATEGORY], TAFL.STATIONCOSTCATEGORY_SZ, nullIndPtrs[STATIONCOSTCATEGORY]);

            tgtValPtrs[NUMBEROFIDENTICALSTATIONS] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[NUMBEROFIDENTICALSTATIONS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, NUMBEROFIDENTICALSTATIONS + 1, tgtValPtrs[NUMBEROFIDENTICALSTATIONS], nullIndPtrs[NUMBEROFIDENTICALSTATIONS]);

            tgtValPtrs[REFERENCEIDENTIFIER] = Marshal.AllocHGlobal(REFERENCEIDENTIFIER_SZ + 1);
            nullIndPtrs[REFERENCEIDENTIFIER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, REFERENCEIDENTIFIER + 1, tgtValPtrs[REFERENCEIDENTIFIER], TAFL.REFERENCEIDENTIFIER_SZ, nullIndPtrs[REFERENCEIDENTIFIER]);

            tgtValPtrs[PROVINCES] = Marshal.AllocHGlobal(PROVINCES_SZ + 1);
            nullIndPtrs[PROVINCES] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, PROVINCES + 1, tgtValPtrs[PROVINCES], TAFL.PROVINCES_SZ, nullIndPtrs[PROVINCES]);

            tgtValPtrs[LATITUDEWGS84] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LATITUDEWGS84] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LATITUDEWGS84 + 1, tgtValPtrs[LATITUDEWGS84], nullIndPtrs[LATITUDEWGS84]);

            tgtValPtrs[LONGITUDEWGS84] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[LONGITUDEWGS84] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, LONGITUDEWGS84 + 1, tgtValPtrs[LONGITUDEWGS84], nullIndPtrs[LONGITUDEWGS84]);

            tgtValPtrs[GROUNDELEVATIONABOVEMEANSEALEVELM] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[GROUNDELEVATIONABOVEMEANSEALEVELM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, GROUNDELEVATIONABOVEMEANSEALEVELM + 1, tgtValPtrs[GROUNDELEVATIONABOVEMEANSEALEVELM], nullIndPtrs[GROUNDELEVATIONABOVEMEANSEALEVELM]);

            tgtValPtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM + 1, tgtValPtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM], nullIndPtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM]);

            tgtValPtrs[CONGESTIONZONE] = Marshal.AllocHGlobal(CONGESTIONZONE_SZ + 1);
            nullIndPtrs[CONGESTIONZONE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CONGESTIONZONE + 1, tgtValPtrs[CONGESTIONZONE], TAFL.CONGESTIONZONE_SZ, nullIndPtrs[CONGESTIONZONE]);

            tgtValPtrs[RADIUSOFOPERATIONKM] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[RADIUSOFOPERATIONKM] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, RADIUSOFOPERATIONKM + 1, tgtValPtrs[RADIUSOFOPERATIONKM], nullIndPtrs[RADIUSOFOPERATIONKM]);

            tgtValPtrs[SATELLITENAME] = Marshal.AllocHGlobal(SATELLITENAME_SZ + 1);
            nullIndPtrs[SATELLITENAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SATELLITENAME + 1, tgtValPtrs[SATELLITENAME], TAFL.SATELLITENAME_SZ, nullIndPtrs[SATELLITENAME]);

            tgtValPtrs[AUTHORIZATIONNUMBER] = Marshal.AllocHGlobal(AUTHORIZATIONNUMBER_SZ + 1);
            nullIndPtrs[AUTHORIZATIONNUMBER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, AUTHORIZATIONNUMBER + 1, tgtValPtrs[AUTHORIZATIONNUMBER], TAFL.AUTHORIZATIONNUMBER_SZ, nullIndPtrs[AUTHORIZATIONNUMBER]);

            tgtValPtrs[MWSERVICE] = Marshal.AllocHGlobal(MWSERVICE_SZ + 1);
            nullIndPtrs[MWSERVICE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MWSERVICE + 1, tgtValPtrs[MWSERVICE], TAFL.MWSERVICE_SZ, nullIndPtrs[MWSERVICE]);

            tgtValPtrs[SUBSERVICE] = Marshal.AllocHGlobal(SUBSERVICE_SZ + 1);
            nullIndPtrs[SUBSERVICE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SUBSERVICE + 1, tgtValPtrs[SUBSERVICE], TAFL.SUBSERVICE_SZ, nullIndPtrs[SUBSERVICE]);

            tgtValPtrs[LICENCETYPE] = Marshal.AllocHGlobal(LICENCETYPE_SZ + 1);
            nullIndPtrs[LICENCETYPE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LICENCETYPE + 1, tgtValPtrs[LICENCETYPE], TAFL.LICENCETYPE_SZ, nullIndPtrs[LICENCETYPE]);

            tgtValPtrs[AUTHORIZATIONSTATUS] = Marshal.AllocHGlobal(AUTHORIZATIONSTATUS_SZ + 1);
            nullIndPtrs[AUTHORIZATIONSTATUS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, AUTHORIZATIONSTATUS + 1, tgtValPtrs[AUTHORIZATIONSTATUS], TAFL.AUTHORIZATIONSTATUS_SZ, nullIndPtrs[AUTHORIZATIONSTATUS]);

            tgtValPtrs[INSERVICEDATE] = Marshal.AllocHGlobal(INSERVICEDATE_SZ + 1);
            nullIndPtrs[INSERVICEDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, INSERVICEDATE + 1, tgtValPtrs[INSERVICEDATE], TAFL.INSERVICEDATE_SZ, nullIndPtrs[INSERVICEDATE]);

            tgtValPtrs[ACCOUNTNUMBER] = Marshal.AllocHGlobal(ACCOUNTNUMBER_SZ + 1);
            nullIndPtrs[ACCOUNTNUMBER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, ACCOUNTNUMBER + 1, tgtValPtrs[ACCOUNTNUMBER], TAFL.ACCOUNTNUMBER_SZ, nullIndPtrs[ACCOUNTNUMBER]);

            tgtValPtrs[LICENSEENAME] = Marshal.AllocHGlobal(LICENSEENAME_SZ + 1);
            nullIndPtrs[LICENSEENAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LICENSEENAME + 1, tgtValPtrs[LICENSEENAME], TAFL.LICENSEENAME_SZ, nullIndPtrs[LICENSEENAME]);

            tgtValPtrs[LICENSEEADDRESS] = Marshal.AllocHGlobal(LICENSEEADDRESS_SZ + 1);
            nullIndPtrs[LICENSEEADDRESS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LICENSEEADDRESS + 1, tgtValPtrs[LICENSEEADDRESS], TAFL.LICENSEEADDRESS_SZ, nullIndPtrs[LICENSEEADDRESS]);

            tgtValPtrs[OPERATIONALSTATUS] = Marshal.AllocHGlobal(OPERATIONALSTATUS_SZ + 1);
            nullIndPtrs[OPERATIONALSTATUS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, OPERATIONALSTATUS + 1, tgtValPtrs[OPERATIONALSTATUS], TAFL.OPERATIONALSTATUS_SZ, nullIndPtrs[OPERATIONALSTATUS]);

            tgtValPtrs[STATIONCLASS] = Marshal.AllocHGlobal(STATIONCLASS_SZ + 1);
            nullIndPtrs[STATIONCLASS] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STATIONCLASS + 1, tgtValPtrs[STATIONCLASS], TAFL.STATIONCLASS_SZ, nullIndPtrs[STATIONCLASS]);

            tgtValPtrs[HORIZONTALPOWERW] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[HORIZONTALPOWERW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, HORIZONTALPOWERW + 1, tgtValPtrs[HORIZONTALPOWERW], nullIndPtrs[HORIZONTALPOWERW]);

            tgtValPtrs[VERTICALPOWERW] = Marshal.AllocHGlobal(sizeof(double));
            nullIndPtrs[VERTICALPOWERW] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToDouble(hStmt, VERTICALPOWERW + 1, tgtValPtrs[VERTICALPOWERW], nullIndPtrs[VERTICALPOWERW]);

            tgtValPtrs[STANDBYTRANSMITTERINFORMATION] = Marshal.AllocHGlobal(STANDBYTRANSMITTERINFORMATION_SZ + 1);
            nullIndPtrs[STANDBYTRANSMITTERINFORMATION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, STANDBYTRANSMITTERINFORMATION + 1, tgtValPtrs[STANDBYTRANSMITTERINFORMATION], TAFL.STANDBYTRANSMITTERINFORMATION_SZ, nullIndPtrs[STANDBYTRANSMITTERINFORMATION]);

            tgtValPtrs[MICSOPER] = Marshal.AllocHGlobal(MICSOPER_SZ + 1);
            nullIndPtrs[MICSOPER] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MICSOPER + 1, tgtValPtrs[MICSOPER], TAFL.MICSOPER_SZ, nullIndPtrs[MICSOPER]);

            tgtValPtrs[MICSOPNOTE] = Marshal.AllocHGlobal(MICSOPNOTE_SZ + 1);
            nullIndPtrs[MICSOPNOTE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MICSOPNOTE + 1, tgtValPtrs[MICSOPNOTE], TAFL.MICSOPNOTE_SZ, nullIndPtrs[MICSOPNOTE]);

            tgtValPtrs[MICSOPRTYP] = Marshal.AllocHGlobal(MICSOPRTYP_SZ + 1);
            nullIndPtrs[MICSOPRTYP] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MICSOPRTYP + 1, tgtValPtrs[MICSOPRTYP], TAFL.MICSOPRTYP_SZ, nullIndPtrs[MICSOPRTYP]);

            tgtValPtrs[MICSCOMPANY] = Marshal.AllocHGlobal(MICSCOMPANY_SZ + 1);
            nullIndPtrs[MICSCOMPANY] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MICSCOMPANY + 1, tgtValPtrs[MICSCOMPANY], TAFL.MICSCOMPANY_SZ, nullIndPtrs[MICSCOMPANY]);

            tgtValPtrs[RECORDACTION] = Marshal.AllocHGlobal(RECORDACTION_SZ + 1);
            nullIndPtrs[RECORDACTION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, RECORDACTION + 1, tgtValPtrs[RECORDACTION], TAFL.RECORDACTION_SZ, nullIndPtrs[RECORDACTION]);

            tgtValPtrs[KEYFIELD] = Marshal.AllocHGlobal(sizeof(int));
            nullIndPtrs[KEYFIELD] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToInt(hStmt, KEYFIELD + 1, tgtValPtrs[KEYFIELD], nullIndPtrs[KEYFIELD]);
        }


        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned MtAnte object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="tAFL"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out TAFL tAFL, out SQLLEN[] nullInds)
        {
            tAFL = new TAFL();
            nullInds = NullHelper.CreateArrayOfNullInd(TAFL.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXRX]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.TxRx = "";
                nullInds[TXRX] = Constant.DB_NULL;
            }
            else
            {
                tAFL.TxRx = Marshal.PtrToStringAnsi(tgtValPtrs[TXRX]).Trim();
                nullInds[TXRX] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FREQUENCYMHZ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[FREQUENCYMHZ], D, 0, 1);
                tAFL.FrequencyMhz = D[0];
                nullInds[FREQUENCYMHZ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FREQUENCYRECORDIDENTIFIER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Frequencyrecordidentifier = "";
                nullInds[FREQUENCYRECORDIDENTIFIER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Frequencyrecordidentifier = Marshal.PtrToStringAnsi(tgtValPtrs[FREQUENCYRECORDIDENTIFIER]).Trim();
                nullInds[FREQUENCYRECORDIDENTIFIER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[REGULATORYSERVICE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Regulatoryservice = "";
                nullInds[REGULATORYSERVICE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Regulatoryservice = Marshal.PtrToStringAnsi(tgtValPtrs[REGULATORYSERVICE]).Trim();
                nullInds[REGULATORYSERVICE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[COMMUNICATIONTYPE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.CommunicationType = "";
                nullInds[COMMUNICATIONTYPE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.CommunicationType = Marshal.PtrToStringAnsi(tgtValPtrs[COMMUNICATIONTYPE]).Trim();
                nullInds[COMMUNICATIONTYPE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CONFORMITYTOFREQUENCYPLAN]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Conformitytofrequencyplan = "";
                nullInds[CONFORMITYTOFREQUENCYPLAN] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Conformitytofrequencyplan = Marshal.PtrToStringAnsi(tgtValPtrs[CONFORMITYTOFREQUENCYPLAN]).Trim();
                nullInds[CONFORMITYTOFREQUENCYPLAN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FREQUENCYALLOCATIONNAME]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Frequencyallocationname = "";
                nullInds[FREQUENCYALLOCATIONNAME] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Frequencyallocationname = Marshal.PtrToStringAnsi(tgtValPtrs[FREQUENCYALLOCATIONNAME]).Trim();
                nullInds[FREQUENCYALLOCATIONNAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CHANNEL]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Channel = "";
                nullInds[CHANNEL] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Channel = Marshal.PtrToStringAnsi(tgtValPtrs[CHANNEL]).Trim();
                nullInds[CHANNEL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[INTERNATIONALCOORDINATIONNUMBER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Internationalcoordinationnumber = "";
                nullInds[INTERNATIONALCOORDINATIONNUMBER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Internationalcoordinationnumber = Marshal.PtrToStringAnsi(tgtValPtrs[INTERNATIONALCOORDINATIONNUMBER]).Trim();
                nullInds[INTERNATIONALCOORDINATIONNUMBER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANALOGDIGITAL]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Analogdigital = "";
                nullInds[ANALOGDIGITAL] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Analogdigital = Marshal.PtrToStringAnsi(tgtValPtrs[ANALOGDIGITAL]).Trim();
                nullInds[ANALOGDIGITAL] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OCCUPIEDBANDWIDTHKHZ]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[OCCUPIEDBANDWIDTHKHZ], D, 0, 1);
                tAFL.OccupiedbandwidthkHz = D[0];
                nullInds[OCCUPIEDBANDWIDTHKHZ] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DESIGNATIONOFEMISSION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Designationofemission = "";
                nullInds[DESIGNATIONOFEMISSION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Designationofemission = Marshal.PtrToStringAnsi(tgtValPtrs[DESIGNATIONOFEMISSION]).Trim();
                nullInds[DESIGNATIONOFEMISSION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MODULATIONTYPE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Modulationtype = "";
                nullInds[MODULATIONTYPE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Modulationtype = Marshal.PtrToStringAnsi(tgtValPtrs[MODULATIONTYPE]).Trim();
                nullInds[MODULATIONTYPE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FILTRATIONINSTALLED]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Filtrationinstalled = "";
                nullInds[FILTRATIONINSTALLED] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Filtrationinstalled = Marshal.PtrToStringAnsi(tgtValPtrs[FILTRATIONINSTALLED]).Trim();
                nullInds[FILTRATIONINSTALLED] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXEFFECTIVERADIATEDPOWERERPDBW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXEFFECTIVERADIATEDPOWERERPDBW], D, 0, 1);
                tAFL.TxeffectiveradiatedpowerERPdBW = D[0];
                nullInds[TXEFFECTIVERADIATEDPOWERERPDBW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TXTRANSMITTERPOWERW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TXTRANSMITTERPOWERW], D, 0, 1);
                tAFL.TxtransmitterpowerW = D[0];
                nullInds[TXTRANSMITTERPOWERW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TOTALLOSSESDB]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[TOTALLOSSESDB], D, 0, 1);
                tAFL.TotallossesdB = D[0];
                nullInds[TOTALLOSSESDB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANALOGCAPACITYCHANNELS]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ANALOGCAPACITYCHANNELS], D, 0, 1);
                tAFL.AnalogcapacityChannels = D[0];
                nullInds[ANALOGCAPACITYCHANNELS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DIGITALCAPACITYMBITS]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[DIGITALCAPACITYMBITS], D, 0, 1);
                tAFL.DigitalcapacityMbits = D[0];
                nullInds[DIGITALCAPACITYMBITS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW], D, 0, 1);
                tAFL.RxunfadedreceivedsignalleveldBW = D[0];
                nullInds[RXUNFADEDRECEIVEDSIGNALLEVELDBW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW], D, 0, 1);
                tAFL.RxthresholdsignallevelforBER10e3dBW = D[0];
                nullInds[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MANUFACTURER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Manufacturer = "";
                nullInds[MANUFACTURER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Manufacturer = Marshal.PtrToStringAnsi(tgtValPtrs[MANUFACTURER]).Trim();
                nullInds[MANUFACTURER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MODELNUMBER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Modelnumber = "";
                nullInds[MODELNUMBER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Modelnumber = Marshal.PtrToStringAnsi(tgtValPtrs[MODELNUMBER]).Trim();
                nullInds[MODELNUMBER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTENNAGAINDBI]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ANTENNAGAINDBI], D, 0, 1);
                tAFL.AntennagaindBi = D[0];
                nullInds[ANTENNAGAINDBI] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTENNAPATTERN]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Antennapattern = "";
                nullInds[ANTENNAPATTERN] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Antennapattern = Marshal.PtrToStringAnsi(tgtValPtrs[ANTENNAPATTERN]).Trim();
                nullInds[ANTENNAPATTERN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[HALFPOWER3DBBEAMWIDTHDEG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[HALFPOWER3DBBEAMWIDTHDEG], D, 0, 1);
                tAFL.Halfpower3dBbeamwidthdeg = D[0];
                nullInds[HALFPOWER3DBBEAMWIDTHDEG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[FRONTTOBACKRATIODB]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[FRONTTOBACKRATIODB], D, 0, 1);
                tAFL.FronttobackratiodB = D[0];
                nullInds[FRONTTOBACKRATIODB] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[POLARIZATION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Polarization = "";
                nullInds[POLARIZATION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Polarization = Marshal.PtrToStringAnsi(tgtValPtrs[POLARIZATION]).Trim();
                nullInds[POLARIZATION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[HEIGHTABOVEGROUNDLEVELM]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[HEIGHTABOVEGROUNDLEVELM], D, 0, 1);
                tAFL.Heightabovegroundlevelm = D[0];
                nullInds[HEIGHTABOVEGROUNDLEVELM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AZIMUTHOFMAINLOBEDEG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[AZIMUTHOFMAINLOBEDEG], D, 0, 1);
                tAFL.Azimuthofmainlobedeg = D[0];
                nullInds[AZIMUTHOFMAINLOBEDEG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[VERTICALELEVATIONANGLEDEG]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[VERTICALELEVATIONANGLEDEG], D, 0, 1);
                tAFL.Verticalelevationangledeg = D[0];
                nullInds[VERTICALELEVATIONANGLEDEG] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATIONLOCATION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Stationlocation = "";
                nullInds[STATIONLOCATION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Stationlocation = Marshal.PtrToStringAnsi(tgtValPtrs[STATIONLOCATION]).Trim();
                nullInds[STATIONLOCATION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LICENSEESTATIONREFERENCE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Licenseestationreference = "";
                nullInds[LICENSEESTATIONREFERENCE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Licenseestationreference = Marshal.PtrToStringAnsi(tgtValPtrs[LICENSEESTATIONREFERENCE]).Trim();
                nullInds[LICENSEESTATIONREFERENCE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CALLSIGN]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Callsign = "";
                nullInds[CALLSIGN] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Callsign = Marshal.PtrToStringAnsi(tgtValPtrs[CALLSIGN]).Trim();
                nullInds[CALLSIGN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[TYPEOFSTATION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Typeofstation = "";
                nullInds[TYPEOFSTATION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Typeofstation = Marshal.PtrToStringAnsi(tgtValPtrs[TYPEOFSTATION]).Trim();
                nullInds[TYPEOFSTATION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ITUCLASSOFSTATION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.ITUclassofstation = "";
                nullInds[ITUCLASSOFSTATION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.ITUclassofstation = Marshal.PtrToStringAnsi(tgtValPtrs[ITUCLASSOFSTATION]).Trim();
                nullInds[ITUCLASSOFSTATION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATIONCOSTCATEGORY]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Stationcostcategory = "";
                nullInds[STATIONCOSTCATEGORY] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Stationcostcategory = Marshal.PtrToStringAnsi(tgtValPtrs[STATIONCOSTCATEGORY]).Trim();
                nullInds[STATIONCOSTCATEGORY] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[NUMBEROFIDENTICALSTATIONS]);
            if (nullInd != Constant.DB_NULL)
            {
                tAFL.Numberofidenticalstations = Marshal.ReadInt32(tgtValPtrs[NUMBEROFIDENTICALSTATIONS]);
                nullInds[NUMBEROFIDENTICALSTATIONS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[REFERENCEIDENTIFIER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Referenceidentifier = "";
                nullInds[REFERENCEIDENTIFIER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Referenceidentifier = Marshal.PtrToStringAnsi(tgtValPtrs[REFERENCEIDENTIFIER]).Trim();
                nullInds[REFERENCEIDENTIFIER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[PROVINCES]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Provinces = "";
                nullInds[PROVINCES] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Provinces = Marshal.PtrToStringAnsi(tgtValPtrs[PROVINCES]).Trim();
                nullInds[PROVINCES] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LATITUDEWGS84]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LATITUDEWGS84], D, 0, 1);
                tAFL.LatitudeWGS84 = D[0];
                nullInds[LATITUDEWGS84] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LONGITUDEWGS84]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[LONGITUDEWGS84], D, 0, 1);
                tAFL.LongitudeWGS84 = D[0];
                nullInds[LONGITUDEWGS84] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[GROUNDELEVATIONABOVEMEANSEALEVELM]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[GROUNDELEVATIONABOVEMEANSEALEVELM], D, 0, 1);
                tAFL.Groundelevationabovemeansealevelm = D[0];
                nullInds[GROUNDELEVATIONABOVEMEANSEALEVELM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM], D, 0, 1);
                tAFL.Antennastructureheightabovegroundlevelm = D[0];
                nullInds[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CONGESTIONZONE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Congestionzone = "";
                nullInds[CONGESTIONZONE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Congestionzone = Marshal.PtrToStringAnsi(tgtValPtrs[CONGESTIONZONE]).Trim();
                nullInds[CONGESTIONZONE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RADIUSOFOPERATIONKM]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[RADIUSOFOPERATIONKM], D, 0, 1);
                tAFL.Radiusofoperationkm = D[0];
                nullInds[RADIUSOFOPERATIONKM] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SATELLITENAME]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Satellitename = "";
                nullInds[SATELLITENAME] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Satellitename = Marshal.PtrToStringAnsi(tgtValPtrs[SATELLITENAME]).Trim();
                nullInds[SATELLITENAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AUTHORIZATIONNUMBER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Authorizationnumber = "";
                nullInds[AUTHORIZATIONNUMBER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Authorizationnumber = Marshal.PtrToStringAnsi(tgtValPtrs[AUTHORIZATIONNUMBER]).Trim();
                nullInds[AUTHORIZATIONNUMBER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MWSERVICE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.MWService = "";
                nullInds[MWSERVICE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.MWService = Marshal.PtrToStringAnsi(tgtValPtrs[MWSERVICE]).Trim();
                nullInds[MWSERVICE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SUBSERVICE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Subservice = "";
                nullInds[SUBSERVICE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Subservice = Marshal.PtrToStringAnsi(tgtValPtrs[SUBSERVICE]).Trim();
                nullInds[SUBSERVICE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LICENCETYPE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Licencetype = "";
                nullInds[LICENCETYPE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Licencetype = Marshal.PtrToStringAnsi(tgtValPtrs[LICENCETYPE]).Trim();
                nullInds[LICENCETYPE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[AUTHORIZATIONSTATUS]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Authorizationstatus = "";
                nullInds[AUTHORIZATIONSTATUS] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Authorizationstatus = Marshal.PtrToStringAnsi(tgtValPtrs[AUTHORIZATIONSTATUS]).Trim();
                nullInds[AUTHORIZATIONSTATUS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[INSERVICEDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Inservicedate = "";
                nullInds[INSERVICEDATE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Inservicedate = Marshal.PtrToStringAnsi(tgtValPtrs[INSERVICEDATE]).Trim();
                nullInds[INSERVICEDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[ACCOUNTNUMBER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Accountnumber = "";
                nullInds[ACCOUNTNUMBER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Accountnumber = Marshal.PtrToStringAnsi(tgtValPtrs[ACCOUNTNUMBER]).Trim();
                nullInds[ACCOUNTNUMBER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LICENSEENAME]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Licenseename = "";
                nullInds[LICENSEENAME] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Licenseename = Marshal.PtrToStringAnsi(tgtValPtrs[LICENSEENAME]).Trim();
                nullInds[LICENSEENAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LICENSEEADDRESS]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Licenseeaddress = "";
                nullInds[LICENSEEADDRESS] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Licenseeaddress = Marshal.PtrToStringAnsi(tgtValPtrs[LICENSEEADDRESS]).Trim();
                nullInds[LICENSEEADDRESS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[OPERATIONALSTATUS]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Operationalstatus = "";
                nullInds[OPERATIONALSTATUS] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Operationalstatus = Marshal.PtrToStringAnsi(tgtValPtrs[OPERATIONALSTATUS]).Trim();
                nullInds[OPERATIONALSTATUS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STATIONCLASS]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Stationclass = "";
                nullInds[STATIONCLASS] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Stationclass = Marshal.PtrToStringAnsi(tgtValPtrs[STATIONCLASS]).Trim();
                nullInds[STATIONCLASS] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[HORIZONTALPOWERW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[HORIZONTALPOWERW], D, 0, 1);
                tAFL.HorizontalpowerW = D[0];
                nullInds[HORIZONTALPOWERW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[VERTICALPOWERW]);
            if (nullInd != Constant.DB_NULL)
            {
                Marshal.Copy(tgtValPtrs[VERTICALPOWERW], D, 0, 1);
                tAFL.VerticalpowerW = D[0];
                nullInds[VERTICALPOWERW] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[STANDBYTRANSMITTERINFORMATION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.Standbytransmitterinformation = "";
                nullInds[STANDBYTRANSMITTERINFORMATION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.Standbytransmitterinformation = Marshal.PtrToStringAnsi(tgtValPtrs[STANDBYTRANSMITTERINFORMATION]).Trim();
                nullInds[STANDBYTRANSMITTERINFORMATION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MICSOPER]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.MICSoper = "";
                nullInds[MICSOPER] = Constant.DB_NULL;
            }
            else
            {
                tAFL.MICSoper = Marshal.PtrToStringAnsi(tgtValPtrs[MICSOPER]).Trim();
                nullInds[MICSOPER] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MICSOPNOTE]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.MICSopnote = "";
                nullInds[MICSOPNOTE] = Constant.DB_NULL;
            }
            else
            {
                tAFL.MICSopnote = Marshal.PtrToStringAnsi(tgtValPtrs[MICSOPNOTE]).Trim();
                nullInds[MICSOPNOTE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MICSOPRTYP]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.MICSoprtyp = "";
                nullInds[MICSOPRTYP] = Constant.DB_NULL;
            }
            else
            {
                tAFL.MICSoprtyp = Marshal.PtrToStringAnsi(tgtValPtrs[MICSOPRTYP]).Trim();
                nullInds[MICSOPRTYP] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MICSCOMPANY]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.MICScompany = "";
                nullInds[MICSCOMPANY] = Constant.DB_NULL;
            }
            else
            {
                tAFL.MICScompany = Marshal.PtrToStringAnsi(tgtValPtrs[MICSCOMPANY]).Trim();
                nullInds[MICSCOMPANY] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[RECORDACTION]);
            if (nullInd == Constant.DB_NULL)
            {
                tAFL.RecordAction = "";
                nullInds[RECORDACTION] = Constant.DB_NULL;
            }
            else
            {
                tAFL.RecordAction = Marshal.PtrToStringAnsi(tgtValPtrs[RECORDACTION]).Trim();
                nullInds[RECORDACTION] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[KEYFIELD]);
            if (nullInd != Constant.DB_NULL)
            {
                tAFL.Keyfield = Marshal.ReadInt32(tgtValPtrs[KEYFIELD]);
                nullInds[KEYFIELD] = Constant.DB_NOT_NULL;
            }

        }

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all column names for use in a SQL 'select' query.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfAllColumnNamesForSQLSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append(columnNames[i]);
                if (i != NUM_COLUMNS - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" ");
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string comprising a comma-separated sequence
        /// of all column names for use in a SQL 'update' query using
        /// bound parameter values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private static string ListOfAllColumnNamesForSQLUpdate()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" ");
            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append(columnNames[i] + "=?");
                if (i != NUM_COLUMNS - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" ");
            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== TAFL =====");

            sb.Append("\nTxRx = " + TxRx);
            sb.Append("\nFrequencyMhz = " + FrequencyMhz);
            sb.Append("\nFrequencyrecordidentifier = " + Frequencyrecordidentifier);
            sb.Append("\nRegulatoryservice = " + Regulatoryservice);
            sb.Append("\nCommunicationType = " + CommunicationType);
            sb.Append("\nConformitytofrequencyplan = " + Conformitytofrequencyplan);
            sb.Append("\nFrequencyallocationname = " + Frequencyallocationname);
            sb.Append("\nChannel = " + Channel);
            sb.Append("\nInternationalcoordinationnumber = " + Internationalcoordinationnumber);
            sb.Append("\nAnalogdigital = " + Analogdigital);
            sb.Append("\nOccupiedbandwidthkHz = " + OccupiedbandwidthkHz);
            sb.Append("\nDesignationofemission = " + Designationofemission);
            sb.Append("\nModulationtype = " + Modulationtype);
            sb.Append("\nFiltrationinstalled = " + Filtrationinstalled);
            sb.Append("\nTxeffectiveradiatedpowerERPdBW = " + TxeffectiveradiatedpowerERPdBW);
            sb.Append("\nTxtransmitterpowerW = " + TxtransmitterpowerW);
            sb.Append("\nTotallossesdB = " + TotallossesdB);
            sb.Append("\nAnalogcapacityChannels = " + AnalogcapacityChannels);
            sb.Append("\nDigitalcapacityMbits = " + DigitalcapacityMbits);
            sb.Append("\nRxunfadedreceivedsignalleveldBW = " + RxunfadedreceivedsignalleveldBW);
            sb.Append("\nRxthresholdsignallevelforBER10e3dBW = " + RxthresholdsignallevelforBER10e3dBW);
            sb.Append("\nManufacturer = " + Manufacturer);
            sb.Append("\nModelnumber = " + Modelnumber);
            sb.Append("\nAntennagaindBi = " + AntennagaindBi);
            sb.Append("\nAntennapattern = " + Antennapattern);
            sb.Append("\nHalfpower3dBbeamwidthdeg = " + Halfpower3dBbeamwidthdeg);
            sb.Append("\nFronttobackratiodB = " + FronttobackratiodB);
            sb.Append("\nPolarization = " + Polarization);
            sb.Append("\nHeightabovegroundlevelm = " + Heightabovegroundlevelm);
            sb.Append("\nAzimuthofmainlobedeg = " + Azimuthofmainlobedeg);
            sb.Append("\nVerticalelevationangledeg = " + Verticalelevationangledeg);
            sb.Append("\nStationlocation = " + Stationlocation);
            sb.Append("\nLicenseestationreference = " + Licenseestationreference);
            sb.Append("\nCallsign = " + Callsign);
            sb.Append("\nTypeofstation = " + Typeofstation);
            sb.Append("\nITUclassofstation = " + ITUclassofstation);
            sb.Append("\nStationcostcategory = " + Stationcostcategory);
            sb.Append("\nNumberofidenticalstations = " + Numberofidenticalstations);
            sb.Append("\nReferenceidentifier = " + Referenceidentifier);
            sb.Append("\nProvinces = " + Provinces);
            sb.Append("\nLatitudeWGS84 = " + LatitudeWGS84);
            sb.Append("\nLongitudeWGS84 = " + LongitudeWGS84);
            sb.Append("\nGroundelevationabovemeansealevelm = " + Groundelevationabovemeansealevelm);
            sb.Append("\nAntennastructureheightabovegroundlevelm = " + Antennastructureheightabovegroundlevelm);
            sb.Append("\nCongestionzone = " + Congestionzone);
            sb.Append("\nRadiusofoperationkm = " + Radiusofoperationkm);
            sb.Append("\nSatellitename = " + Satellitename);
            sb.Append("\nAuthorizationnumber = " + Authorizationnumber);
            sb.Append("\nMWService = " + MWService);
            sb.Append("\nSubservice = " + Subservice);
            sb.Append("\nLicencetype = " + Licencetype);
            sb.Append("\nAuthorizationstatus = " + Authorizationstatus);
            sb.Append("\nInservicedate = " + Inservicedate);
            sb.Append("\nAccountnumber = " + Accountnumber);
            sb.Append("\nLicenseename = " + Licenseename);
            sb.Append("\nLicenseeaddress = " + Licenseeaddress);
            sb.Append("\nOperationalstatus = " + Operationalstatus);
            sb.Append("\nStationclass = " + Stationclass);
            sb.Append("\nHorizontalpowerW = " + HorizontalpowerW);
            sb.Append("\nVerticalpowerW = " + VerticalpowerW);
            sb.Append("\nStandbytransmitterinformation = " + Standbytransmitterinformation);
            sb.Append("\nMICSoper = " + MICSoper);
            sb.Append("\nMICSopnote = " + MICSopnote);
            sb.Append("\nMICSoprtyp = " + MICSoprtyp);
            sb.Append("\nMICScompany = " + MICScompany);
            sb.Append("\nRecordAction = " + RecordAction);
            sb.Append("\nKeyfield = " + Keyfield);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the member fields together with
        /// their associated ODBC nullInds.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringWN(SQLLEN[] nullInds)
        {
            int n = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n===== TAFL =====");

            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "TxRx =      " + TxRx);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "FrequencyMhz =      " + FrequencyMhz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Frequencyrecordidentifier =      " + Frequencyrecordidentifier);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Regulatoryservice =      " + Regulatoryservice);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "CommunicationType =      " + CommunicationType);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Conformitytofrequencyplan =      " + Conformitytofrequencyplan);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Frequencyallocationname =      " + Frequencyallocationname);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Channel =      " + Channel);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Internationalcoordinationnumber =      " + Internationalcoordinationnumber);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Analogdigital =      " + Analogdigital);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "OccupiedbandwidthkHz =      " + OccupiedbandwidthkHz);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Designationofemission =      " + Designationofemission);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Modulationtype =      " + Modulationtype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Filtrationinstalled =      " + Filtrationinstalled);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "TxeffectiveradiatedpowerERPdBW =      " + TxeffectiveradiatedpowerERPdBW);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "TxtransmitterpowerW =      " + TxtransmitterpowerW);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "TotallossesdB =      " + TotallossesdB);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "AnalogcapacityChannels =      " + AnalogcapacityChannels);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "DigitalcapacityMbits =      " + DigitalcapacityMbits);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "RxunfadedreceivedsignalleveldBW =      " + RxunfadedreceivedsignalleveldBW);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "RxthresholdsignallevelforBER10e3dBW =      " + RxthresholdsignallevelforBER10e3dBW);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Manufacturer =      " + Manufacturer);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Modelnumber =      " + Modelnumber);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "AntennagaindBi =      " + AntennagaindBi);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Antennapattern =      " + Antennapattern);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Halfpower3dBbeamwidthdeg =      " + Halfpower3dBbeamwidthdeg);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "FronttobackratiodB =      " + FronttobackratiodB);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Polarization =      " + Polarization);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Heightabovegroundlevelm =      " + Heightabovegroundlevelm);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Azimuthofmainlobedeg =      " + Azimuthofmainlobedeg);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Verticalelevationangledeg =      " + Verticalelevationangledeg);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Stationlocation =      " + Stationlocation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Licenseestationreference =      " + Licenseestationreference);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Callsign =      " + Callsign);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Typeofstation =      " + Typeofstation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "ITUclassofstation =      " + ITUclassofstation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Stationcostcategory =      " + Stationcostcategory);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Numberofidenticalstations =      " + Numberofidenticalstations);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Referenceidentifier =      " + Referenceidentifier);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Provinces =      " + Provinces);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "LatitudeWGS84 =      " + LatitudeWGS84);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "LongitudeWGS84 =      " + LongitudeWGS84);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Groundelevationabovemeansealevelm =      " + Groundelevationabovemeansealevelm);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Antennastructureheightabovegroundlevelm =      " + Antennastructureheightabovegroundlevelm);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Congestionzone =      " + Congestionzone);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Radiusofoperationkm =      " + Radiusofoperationkm);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Satellitename =      " + Satellitename);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Authorizationnumber =      " + Authorizationnumber);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "MWService =      " + MWService);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Subservice =      " + Subservice);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Licencetype =      " + Licencetype);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Authorizationstatus =      " + Authorizationstatus);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Inservicedate =      " + Inservicedate);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Accountnumber =      " + Accountnumber);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Licenseename =      " + Licenseename);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Licenseeaddress =      " + Licenseeaddress);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Operationalstatus =      " + Operationalstatus);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Stationclass =      " + Stationclass);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "HorizontalpowerW =      " + HorizontalpowerW);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "VerticalpowerW =      " + VerticalpowerW);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Standbytransmitterinformation =      " + Standbytransmitterinformation);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "MICSoper =      " + MICSoper);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "MICSopnote =      " + MICSopnote);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "MICSoprtyp =      " + MICSoprtyp);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "MICScompany =      " + MICScompany);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "RecordAction =      " + RecordAction);
            sb.AppendLine(nullInds[n++].ToString().PadRight(6) + "Keyfield =      " + Keyfield);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a CSV formatted string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <returns></returns>
        public string ToCSVstring()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TxRx);
            sb.Append("," + FrequencyMhz);
            sb.Append("," + Frequencyrecordidentifier);
            sb.Append("," + Regulatoryservice);
            sb.Append("," + CommunicationType);
            sb.Append("," + Conformitytofrequencyplan);
            sb.Append("," + Frequencyallocationname);
            sb.Append("," + Channel);
            sb.Append("," + Internationalcoordinationnumber);
            sb.Append("," + Analogdigital);
            sb.Append("," + OccupiedbandwidthkHz);
            sb.Append("," + Designationofemission);
            sb.Append("," + Modulationtype);
            sb.Append("," + Filtrationinstalled);
            sb.Append("," + TxeffectiveradiatedpowerERPdBW);
            sb.Append("," + TxtransmitterpowerW);
            sb.Append("," + TotallossesdB);
            sb.Append("," + AnalogcapacityChannels);
            sb.Append("," + DigitalcapacityMbits);
            sb.Append("," + RxunfadedreceivedsignalleveldBW);
            sb.Append("," + RxthresholdsignallevelforBER10e3dBW);
            sb.Append("," + Manufacturer);
            sb.Append("," + Modelnumber);
            sb.Append("," + AntennagaindBi);
            sb.Append("," + Antennapattern);
            sb.Append("," + Halfpower3dBbeamwidthdeg);
            sb.Append("," + FronttobackratiodB);
            sb.Append("," + Polarization);
            sb.Append("," + Heightabovegroundlevelm);
            sb.Append("," + Azimuthofmainlobedeg);
            sb.Append("," + Verticalelevationangledeg);
            sb.Append("," + Stationlocation);
            sb.Append("," + Licenseestationreference);
            sb.Append("," + Callsign);
            sb.Append("," + Typeofstation);
            sb.Append("," + ITUclassofstation);
            sb.Append("," + Stationcostcategory);
            sb.Append("," + Numberofidenticalstations);
            sb.Append("," + Referenceidentifier);
            sb.Append("," + Provinces);
            sb.Append("," + LatitudeWGS84);
            sb.Append("," + LongitudeWGS84);
            sb.Append("," + Groundelevationabovemeansealevelm);
            sb.Append("," + Antennastructureheightabovegroundlevelm);
            sb.Append("," + Congestionzone);
            sb.Append("," + Radiusofoperationkm);
            sb.Append("," + Satellitename);
            sb.Append("," + Authorizationnumber);
            sb.Append("," + MWService);
            sb.Append("," + Subservice);
            sb.Append("," + Licencetype);
            sb.Append("," + Authorizationstatus);
            sb.Append("," + Inservicedate);
            sb.Append("," + Accountnumber);
            sb.Append("," + Licenseename);
            sb.Append("," + Licenseeaddress);
            sb.Append("," + Operationalstatus);
            sb.Append("," + Stationclass);
            sb.Append("," + HorizontalpowerW);
            sb.Append("," + VerticalpowerW);
            sb.Append("," + Standbytransmitterinformation);
            sb.Append("," + MICSoper);
            sb.Append("," + MICSopnote);
            sb.Append("," + MICSoprtyp);
            sb.Append("," + MICScompany);
            sb.Append("," + RecordAction);
            sb.Append("," + Keyfield);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a CSV string identical to calling ToCSVstring()
        /// but with the current value of the field RecordAction prepended at the start.
        /// </summary>
        /// <returns></returns>
        public string ToCSVstringSpecial()
        {
            return RecordAction + "," + ToCSVstring();
        }

        /// <summary>
        /// This method returns a CSV-formatted string i.a.w. RFC 4180 comprising the 
        /// current member values of 'this' object as qualified by the prescribed array 
        /// of nullInds; a nullInd set to -1 will cause the associated CSV field to be 
        /// written as a string of zero length; all fields are delimited using the 
        /// double-quotation (") character.
        /// </summary>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        public string ToStringAsCSVexportRFC4180(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(nullInds[TAFL.TXRX] == Constant.DB_NULL ? "\"\"," : "\"" + TxRx.ToString() + "\",");
            sb.Append(nullInds[TAFL.FREQUENCYMHZ] == Constant.DB_NULL ? "\"\"," : "\"" + FrequencyMhz.ToString() + "\",");
            sb.Append(nullInds[TAFL.FREQUENCYRECORDIDENTIFIER] == Constant.DB_NULL ? "\"\"," : "\"" + Frequencyrecordidentifier.ToString() + "\",");
            sb.Append(nullInds[TAFL.REGULATORYSERVICE] == Constant.DB_NULL ? "\"\"," : "\"" + Regulatoryservice.ToString() + "\",");
            sb.Append(nullInds[TAFL.COMMUNICATIONTYPE] == Constant.DB_NULL ? "\"\"," : "\"" + CommunicationType.ToString() + "\",");
            sb.Append(nullInds[TAFL.CONFORMITYTOFREQUENCYPLAN] == Constant.DB_NULL ? "\"\"," : "\"" + Conformitytofrequencyplan.ToString() + "\",");
            sb.Append(nullInds[TAFL.FREQUENCYALLOCATIONNAME] == Constant.DB_NULL ? "\"\"," : "\"" + Frequencyallocationname.ToString() + "\",");
            sb.Append(nullInds[TAFL.CHANNEL] == Constant.DB_NULL ? "\"\"," : "\"" + Channel.ToString() + "\",");
            sb.Append(nullInds[TAFL.INTERNATIONALCOORDINATIONNUMBER] == Constant.DB_NULL ? "\"\"," : "\"" + Internationalcoordinationnumber.ToString() + "\",");
            sb.Append(nullInds[TAFL.ANALOGDIGITAL] == Constant.DB_NULL ? "\"\"," : "\"" + Analogdigital.ToString() + "\",");
            sb.Append(nullInds[TAFL.OCCUPIEDBANDWIDTHKHZ] == Constant.DB_NULL ? "\"\"," : "\"" + OccupiedbandwidthkHz.ToString() + "\",");
            sb.Append(nullInds[TAFL.DESIGNATIONOFEMISSION] == Constant.DB_NULL ? "\"\"," : "\"" + Designationofemission.ToString() + "\",");
            sb.Append(nullInds[TAFL.MODULATIONTYPE] == Constant.DB_NULL ? "\"\"," : "\"" + Modulationtype.ToString() + "\",");
            sb.Append(nullInds[TAFL.FILTRATIONINSTALLED] == Constant.DB_NULL ? "\"\"," : "\"" + Filtrationinstalled.ToString() + "\",");
            sb.Append(nullInds[TAFL.TXEFFECTIVERADIATEDPOWERERPDBW] == Constant.DB_NULL ? "\"\"," : "\"" + TxeffectiveradiatedpowerERPdBW.ToString() + "\",");
            sb.Append(nullInds[TAFL.TXTRANSMITTERPOWERW] == Constant.DB_NULL ? "\"\"," : "\"" + TxtransmitterpowerW.ToString() + "\",");
            sb.Append(nullInds[TAFL.TOTALLOSSESDB] == Constant.DB_NULL ? "\"\"," : "\"" + TotallossesdB.ToString() + "\",");
            sb.Append(nullInds[TAFL.ANALOGCAPACITYCHANNELS] == Constant.DB_NULL ? "\"\"," : "\"" + AnalogcapacityChannels.ToString() + "\",");
            sb.Append(nullInds[TAFL.DIGITALCAPACITYMBITS] == Constant.DB_NULL ? "\"\"," : "\"" + DigitalcapacityMbits.ToString() + "\",");
            sb.Append(nullInds[TAFL.RXUNFADEDRECEIVEDSIGNALLEVELDBW] == Constant.DB_NULL ? "\"\"," : "\"" + RxunfadedreceivedsignalleveldBW.ToString() + "\",");
            sb.Append(nullInds[TAFL.RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] == Constant.DB_NULL ? "\"\"," : "\"" + RxthresholdsignallevelforBER10e3dBW.ToString() + "\",");
            sb.Append(nullInds[TAFL.MANUFACTURER] == Constant.DB_NULL ? "\"\"," : "\"" + Manufacturer.ToString() + "\",");
            sb.Append(nullInds[TAFL.MODELNUMBER] == Constant.DB_NULL ? "\"\"," : "\"" + Modelnumber.ToString() + "\",");
            sb.Append(nullInds[TAFL.ANTENNAGAINDBI] == Constant.DB_NULL ? "\"\"," : "\"" + AntennagaindBi.ToString() + "\",");
            sb.Append(nullInds[TAFL.ANTENNAPATTERN] == Constant.DB_NULL ? "\"\"," : "\"" + Antennapattern.ToString() + "\",");
            sb.Append(nullInds[TAFL.HALFPOWER3DBBEAMWIDTHDEG] == Constant.DB_NULL ? "\"\"," : "\"" + Halfpower3dBbeamwidthdeg.ToString() + "\",");
            sb.Append(nullInds[TAFL.FRONTTOBACKRATIODB] == Constant.DB_NULL ? "\"\"," : "\"" + FronttobackratiodB.ToString() + "\",");
            sb.Append(nullInds[TAFL.POLARIZATION] == Constant.DB_NULL ? "\"\"," : "\"" + Polarization.ToString() + "\",");
            sb.Append(nullInds[TAFL.HEIGHTABOVEGROUNDLEVELM] == Constant.DB_NULL ? "\"\"," : "\"" + Heightabovegroundlevelm.ToString() + "\",");
            sb.Append(nullInds[TAFL.AZIMUTHOFMAINLOBEDEG] == Constant.DB_NULL ? "\"\"," : "\"" + Azimuthofmainlobedeg.ToString() + "\",");
            sb.Append(nullInds[TAFL.VERTICALELEVATIONANGLEDEG] == Constant.DB_NULL ? "\"\"," : "\"" + Verticalelevationangledeg.ToString() + "\",");
            sb.Append(nullInds[TAFL.STATIONLOCATION] == Constant.DB_NULL ? "\"\"," : "\"" + Stationlocation.ToString() + "\",");
            sb.Append(nullInds[TAFL.LICENSEESTATIONREFERENCE] == Constant.DB_NULL ? "\"\"," : "\"" + Licenseestationreference.ToString() + "\",");
            sb.Append(nullInds[TAFL.CALLSIGN] == Constant.DB_NULL ? "\"\"," : "\"" + Callsign.ToString() + "\",");
            sb.Append(nullInds[TAFL.TYPEOFSTATION] == Constant.DB_NULL ? "\"\"," : "\"" + Typeofstation.ToString() + "\",");
            sb.Append(nullInds[TAFL.ITUCLASSOFSTATION] == Constant.DB_NULL ? "\"\"," : "\"" + ITUclassofstation.ToString() + "\",");
            sb.Append(nullInds[TAFL.STATIONCOSTCATEGORY] == Constant.DB_NULL ? "\"\"," : "\"" + Stationcostcategory.ToString() + "\",");
            sb.Append(nullInds[TAFL.NUMBEROFIDENTICALSTATIONS] == Constant.DB_NULL ? "\"\"," : "\"" + Numberofidenticalstations.ToString() + "\",");
            sb.Append(nullInds[TAFL.REFERENCEIDENTIFIER] == Constant.DB_NULL ? "\"\"," : "\"" + Referenceidentifier.ToString() + "\",");
            sb.Append(nullInds[TAFL.PROVINCES] == Constant.DB_NULL ? "\"\"," : "\"" + Provinces.ToString() + "\",");
            sb.Append(nullInds[TAFL.LATITUDEWGS84] == Constant.DB_NULL ? "\"\"," : "\"" + LatitudeWGS84.ToString() + "\",");
            sb.Append(nullInds[TAFL.LONGITUDEWGS84] == Constant.DB_NULL ? "\"\"," : "\"" + LongitudeWGS84.ToString() + "\",");
            sb.Append(nullInds[TAFL.GROUNDELEVATIONABOVEMEANSEALEVELM] == Constant.DB_NULL ? "\"\"," : "\"" + Groundelevationabovemeansealevelm.ToString() + "\",");
            sb.Append(nullInds[TAFL.ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM] == Constant.DB_NULL ? "\"\"," : "\"" + Antennastructureheightabovegroundlevelm.ToString() + "\",");
            sb.Append(nullInds[TAFL.CONGESTIONZONE] == Constant.DB_NULL ? "\"\"," : "\"" + Congestionzone.ToString() + "\",");
            sb.Append(nullInds[TAFL.RADIUSOFOPERATIONKM] == Constant.DB_NULL ? "\"\"," : "\"" + Radiusofoperationkm.ToString() + "\",");
            sb.Append(nullInds[TAFL.SATELLITENAME] == Constant.DB_NULL ? "\"\"," : "\"" + Satellitename.ToString() + "\",");
            sb.Append(nullInds[TAFL.AUTHORIZATIONNUMBER] == Constant.DB_NULL ? "\"\"," : "\"" + Authorizationnumber.ToString() + "\",");
            sb.Append(nullInds[TAFL.MWSERVICE] == Constant.DB_NULL ? "\"\"," : "\"" + MWService.ToString() + "\",");
            sb.Append(nullInds[TAFL.SUBSERVICE] == Constant.DB_NULL ? "\"\"," : "\"" + Subservice.ToString() + "\",");
            sb.Append(nullInds[TAFL.LICENCETYPE] == Constant.DB_NULL ? "\"\"," : "\"" + Licencetype.ToString() + "\",");
            sb.Append(nullInds[TAFL.AUTHORIZATIONSTATUS] == Constant.DB_NULL ? "\"\"," : "\"" + Authorizationstatus.ToString() + "\",");
            sb.Append(nullInds[TAFL.INSERVICEDATE] == Constant.DB_NULL ? "\"\"," : "\"" + Inservicedate.ToString() + "\",");
            sb.Append(nullInds[TAFL.ACCOUNTNUMBER] == Constant.DB_NULL ? "\"\"," : "\"" + Accountnumber.ToString() + "\",");
            sb.Append(nullInds[TAFL.LICENSEENAME] == Constant.DB_NULL ? "\"\"," : "\"" + Licenseename.ToString() + "\",");
            sb.Append(nullInds[TAFL.LICENSEEADDRESS] == Constant.DB_NULL ? "\"\"," : "\"" + Licenseeaddress.ToString() + "\",");
            sb.Append(nullInds[TAFL.OPERATIONALSTATUS] == Constant.DB_NULL ? "\"\"," : "\"" + Operationalstatus.ToString() + "\",");
            sb.Append(nullInds[TAFL.STATIONCLASS] == Constant.DB_NULL ? "\"\"," : "\"" + Stationclass.ToString() + "\",");
            sb.Append(nullInds[TAFL.HORIZONTALPOWERW] == Constant.DB_NULL ? "\"\"," : "\"" + HorizontalpowerW.ToString() + "\",");
            sb.Append(nullInds[TAFL.VERTICALPOWERW] == Constant.DB_NULL ? "\"\"," : "\"" + VerticalpowerW.ToString() + "\",");
            sb.Append(nullInds[TAFL.STANDBYTRANSMITTERINFORMATION] == Constant.DB_NULL ? "\"\"" : "\"" + Standbytransmitterinformation.ToString() + "\"");
           
            // BE CAREFUL!: if using any of Bill's additional columns (see below)
            //              remember to insert two instances of ',' in the previous line of C# code.
            
            //sb.Append(nullInds[TAFL.MICSOPER] == Constant.DB_NULL ? "\"\"," : "\"" + MICSoper.ToString() + "\",");
            //sb.Append(nullInds[TAFL.MICSOPNOTE] == Constant.DB_NULL ? "\"\"," : "\"" + MICSopnote.ToString() + "\",");
            //sb.Append(nullInds[TAFL.MICSOPRTYP] == Constant.DB_NULL ? "\"\"," : "\"" + MICSoprtyp.ToString() + "\",");
            //sb.Append(nullInds[TAFL.MICSCOMPANY] == Constant.DB_NULL ? "\"\"," : "\"" + MICScompany.ToString() + "\",");
            //sb.Append(nullInds[TAFL.RECORDACTION] == Constant.DB_NULL ? "\"\"," : "\"" + RecordAction.ToString() + "\",");
            //sb.Append(nullInds[TAFL.KEYFIELD] == Constant.DB_NULL ? "\"\"," : "\"" + Keyfield.ToString() + "\"");

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a string identical to calling ListOfAllColumnNamesForSQLSelect()
        /// but with the string "ProblemCode," prepended to the start.
        /// </summary>
        /// <returns></returns>
        public static string ColumnNamesSpecial()
        {
            return "ProblemCode," + AllColumnsForSqlSelect;
        }

        public SQLPOINTER[] CopyToArrayOfSQLPOINTERinGlobalMemory()
        {
            //Can only copy arrays of float and double into native memory using Marshal method.
            float[] F = new float[1];
            double[] D = new double[1];

            SQLPOINTER[] parameterValuePtrs = new SQLPOINTER[NUM_COLUMNS];

            parameterValuePtrs[TXRX] = Marshal.StringToHGlobalAnsi(TxRx);

            parameterValuePtrs[FREQUENCYMHZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = FrequencyMhz;
            Marshal.Copy(D, 0, parameterValuePtrs[FREQUENCYMHZ], 1);

            parameterValuePtrs[FREQUENCYRECORDIDENTIFIER] = Marshal.StringToHGlobalAnsi(Frequencyrecordidentifier);

            parameterValuePtrs[REGULATORYSERVICE] = Marshal.StringToHGlobalAnsi(Regulatoryservice);

            parameterValuePtrs[COMMUNICATIONTYPE] = Marshal.StringToHGlobalAnsi(CommunicationType);

            parameterValuePtrs[CONFORMITYTOFREQUENCYPLAN] = Marshal.StringToHGlobalAnsi(Conformitytofrequencyplan);

            parameterValuePtrs[FREQUENCYALLOCATIONNAME] = Marshal.StringToHGlobalAnsi(Frequencyallocationname);

            parameterValuePtrs[CHANNEL] = Marshal.StringToHGlobalAnsi(Channel);

            parameterValuePtrs[INTERNATIONALCOORDINATIONNUMBER] = Marshal.StringToHGlobalAnsi(Internationalcoordinationnumber);

            parameterValuePtrs[ANALOGDIGITAL] = Marshal.StringToHGlobalAnsi(Analogdigital);

            parameterValuePtrs[OCCUPIEDBANDWIDTHKHZ] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = OccupiedbandwidthkHz;
            Marshal.Copy(D, 0, parameterValuePtrs[OCCUPIEDBANDWIDTHKHZ], 1);

            parameterValuePtrs[DESIGNATIONOFEMISSION] = Marshal.StringToHGlobalAnsi(Designationofemission);

            parameterValuePtrs[MODULATIONTYPE] = Marshal.StringToHGlobalAnsi(Modulationtype);

            parameterValuePtrs[FILTRATIONINSTALLED] = Marshal.StringToHGlobalAnsi(Filtrationinstalled);

            parameterValuePtrs[TXEFFECTIVERADIATEDPOWERERPDBW] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = TxeffectiveradiatedpowerERPdBW;
            Marshal.Copy(D, 0, parameterValuePtrs[TXEFFECTIVERADIATEDPOWERERPDBW], 1);

            parameterValuePtrs[TXTRANSMITTERPOWERW] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = TxtransmitterpowerW;
            Marshal.Copy(D, 0, parameterValuePtrs[TXTRANSMITTERPOWERW], 1);

            parameterValuePtrs[TOTALLOSSESDB] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = TotallossesdB;
            Marshal.Copy(D, 0, parameterValuePtrs[TOTALLOSSESDB], 1);

            parameterValuePtrs[ANALOGCAPACITYCHANNELS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = AnalogcapacityChannels;
            Marshal.Copy(D, 0, parameterValuePtrs[ANALOGCAPACITYCHANNELS], 1);

            parameterValuePtrs[DIGITALCAPACITYMBITS] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = DigitalcapacityMbits;
            Marshal.Copy(D, 0, parameterValuePtrs[DIGITALCAPACITYMBITS], 1);

            parameterValuePtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = RxunfadedreceivedsignalleveldBW;
            Marshal.Copy(D, 0, parameterValuePtrs[RXUNFADEDRECEIVEDSIGNALLEVELDBW], 1);

            parameterValuePtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = RxthresholdsignallevelforBER10e3dBW;
            Marshal.Copy(D, 0, parameterValuePtrs[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW], 1);

            parameterValuePtrs[MANUFACTURER] = Marshal.StringToHGlobalAnsi(Manufacturer);

            parameterValuePtrs[MODELNUMBER] = Marshal.StringToHGlobalAnsi(Modelnumber);

            parameterValuePtrs[ANTENNAGAINDBI] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = AntennagaindBi;
            Marshal.Copy(D, 0, parameterValuePtrs[ANTENNAGAINDBI], 1);

            parameterValuePtrs[ANTENNAPATTERN] = Marshal.StringToHGlobalAnsi(Antennapattern);

            parameterValuePtrs[HALFPOWER3DBBEAMWIDTHDEG] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Halfpower3dBbeamwidthdeg;
            Marshal.Copy(D, 0, parameterValuePtrs[HALFPOWER3DBBEAMWIDTHDEG], 1);

            parameterValuePtrs[FRONTTOBACKRATIODB] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = FronttobackratiodB;
            Marshal.Copy(D, 0, parameterValuePtrs[FRONTTOBACKRATIODB], 1);

            parameterValuePtrs[POLARIZATION] = Marshal.StringToHGlobalAnsi(Polarization);

            parameterValuePtrs[HEIGHTABOVEGROUNDLEVELM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Heightabovegroundlevelm;
            Marshal.Copy(D, 0, parameterValuePtrs[HEIGHTABOVEGROUNDLEVELM], 1);

            parameterValuePtrs[AZIMUTHOFMAINLOBEDEG] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Azimuthofmainlobedeg;
            Marshal.Copy(D, 0, parameterValuePtrs[AZIMUTHOFMAINLOBEDEG], 1);

            parameterValuePtrs[VERTICALELEVATIONANGLEDEG] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Verticalelevationangledeg;
            Marshal.Copy(D, 0, parameterValuePtrs[VERTICALELEVATIONANGLEDEG], 1);

            parameterValuePtrs[STATIONLOCATION] = Marshal.StringToHGlobalAnsi(Stationlocation);

            parameterValuePtrs[LICENSEESTATIONREFERENCE] = Marshal.StringToHGlobalAnsi(Licenseestationreference);

            parameterValuePtrs[CALLSIGN] = Marshal.StringToHGlobalAnsi(Callsign);

            parameterValuePtrs[TYPEOFSTATION] = Marshal.StringToHGlobalAnsi(Typeofstation);

            parameterValuePtrs[ITUCLASSOFSTATION] = Marshal.StringToHGlobalAnsi(ITUclassofstation);

            parameterValuePtrs[STATIONCOSTCATEGORY] = Marshal.StringToHGlobalAnsi(Stationcostcategory);

            parameterValuePtrs[NUMBEROFIDENTICALSTATIONS] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[NUMBEROFIDENTICALSTATIONS], Numberofidenticalstations);

            parameterValuePtrs[REFERENCEIDENTIFIER] = Marshal.StringToHGlobalAnsi(Referenceidentifier);

            parameterValuePtrs[PROVINCES] = Marshal.StringToHGlobalAnsi(Provinces);

            parameterValuePtrs[LATITUDEWGS84] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = LatitudeWGS84;
            Marshal.Copy(D, 0, parameterValuePtrs[LATITUDEWGS84], 1);

            parameterValuePtrs[LONGITUDEWGS84] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = LongitudeWGS84;
            Marshal.Copy(D, 0, parameterValuePtrs[LONGITUDEWGS84], 1);

            parameterValuePtrs[GROUNDELEVATIONABOVEMEANSEALEVELM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Groundelevationabovemeansealevelm;
            Marshal.Copy(D, 0, parameterValuePtrs[GROUNDELEVATIONABOVEMEANSEALEVELM], 1);

            parameterValuePtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Antennastructureheightabovegroundlevelm;
            Marshal.Copy(D, 0, parameterValuePtrs[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM], 1);

            parameterValuePtrs[CONGESTIONZONE] = Marshal.StringToHGlobalAnsi(Congestionzone);

            parameterValuePtrs[RADIUSOFOPERATIONKM] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = Radiusofoperationkm;
            Marshal.Copy(D, 0, parameterValuePtrs[RADIUSOFOPERATIONKM], 1);

            parameterValuePtrs[SATELLITENAME] = Marshal.StringToHGlobalAnsi(Satellitename);

            parameterValuePtrs[AUTHORIZATIONNUMBER] = Marshal.StringToHGlobalAnsi(Authorizationnumber);

            parameterValuePtrs[MWSERVICE] = Marshal.StringToHGlobalAnsi(MWService);

            parameterValuePtrs[SUBSERVICE] = Marshal.StringToHGlobalAnsi(Subservice);

            parameterValuePtrs[LICENCETYPE] = Marshal.StringToHGlobalAnsi(Licencetype);

            parameterValuePtrs[AUTHORIZATIONSTATUS] = Marshal.StringToHGlobalAnsi(Authorizationstatus);

            parameterValuePtrs[INSERVICEDATE] = Marshal.StringToHGlobalAnsi(Inservicedate);

            parameterValuePtrs[ACCOUNTNUMBER] = Marshal.StringToHGlobalAnsi(Accountnumber);

            parameterValuePtrs[LICENSEENAME] = Marshal.StringToHGlobalAnsi(Licenseename);

            parameterValuePtrs[LICENSEEADDRESS] = Marshal.StringToHGlobalAnsi(Licenseeaddress);

            parameterValuePtrs[OPERATIONALSTATUS] = Marshal.StringToHGlobalAnsi(Operationalstatus);

            parameterValuePtrs[STATIONCLASS] = Marshal.StringToHGlobalAnsi(Stationclass);

            parameterValuePtrs[HORIZONTALPOWERW] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = HorizontalpowerW;
            Marshal.Copy(D, 0, parameterValuePtrs[HORIZONTALPOWERW], 1);

            parameterValuePtrs[VERTICALPOWERW] = Marshal.AllocHGlobal(sizeof(double));
            D[0] = VerticalpowerW;
            Marshal.Copy(D, 0, parameterValuePtrs[VERTICALPOWERW], 1);

            parameterValuePtrs[STANDBYTRANSMITTERINFORMATION] = Marshal.StringToHGlobalAnsi(Standbytransmitterinformation);

            parameterValuePtrs[MICSOPER] = Marshal.StringToHGlobalAnsi(MICSoper);

            parameterValuePtrs[MICSOPNOTE] = Marshal.StringToHGlobalAnsi(MICSopnote);

            parameterValuePtrs[MICSOPRTYP] = Marshal.StringToHGlobalAnsi(MICSoprtyp);

            parameterValuePtrs[MICSCOMPANY] = Marshal.StringToHGlobalAnsi(MICScompany);

            parameterValuePtrs[RECORDACTION] = Marshal.StringToHGlobalAnsi(RecordAction);

            parameterValuePtrs[KEYFIELD] = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(parameterValuePtrs[KEYFIELD], Keyfield);

            return parameterValuePtrs;
        }

        /// <summary>
        /// This method returns a string comprising a complete SQL 'insert' query using
        /// bound parameter values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string BuildSqlInsertString(string tableName)
        {
            StringBuilder valuesSB = new StringBuilder();
            valuesSB.Append("(?");
            for (int i = 1; i < NUM_COLUMNS; i++)
            {
                valuesSB.Append(", ?");
            }
            valuesSB.Append(")");

            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ");
            sb.Append(tableName);
            sb.Append(" (");
            sb.Append(AllColumnsForSqlSelect);
            sb.Append(") values ");
            sb.Append(valuesSB);

            return sb.ToString();
        }

        /// <summary>
        /// This method is passed an array strings that prescribe the member values of the TAFL object
        /// that is created, populated and returned; an array of nullInds is also returned.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tAFL"></param>
        /// <param name="nullInds"></param>
        public static void CreateTAFLfromCsvFields(string[] fields, out TAFL tAFL, out SQLLEN[] nullInds)
        {
            // 'out'.
            tAFL = new TAFL();
            nullInds = NullHelper.CreateArrayOfNullInd(TAFL.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            CSV.ParseFieldAsString(fields, columnNames, TXRX, 0, 200, out tAFL.TxRx, out nullInds[TXRX]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, FREQUENCYMHZ, double.MinValue, double.MaxValue, -1, out tAFL.FrequencyMhz, out nullInds[FREQUENCYMHZ]);
            CSV.ParseFieldAsString(fields, columnNames, FREQUENCYRECORDIDENTIFIER, 0, 200, out tAFL.Frequencyrecordidentifier, out nullInds[FREQUENCYRECORDIDENTIFIER]);
            CSV.ParseFieldAsString(fields, columnNames, REGULATORYSERVICE, 0, 200, out tAFL.Regulatoryservice, out nullInds[REGULATORYSERVICE]);
            CSV.ParseFieldAsString(fields, columnNames, COMMUNICATIONTYPE, 0, 200, out tAFL.CommunicationType, out nullInds[COMMUNICATIONTYPE]);
            CSV.ParseFieldAsString(fields, columnNames, CONFORMITYTOFREQUENCYPLAN, 0, 200, out tAFL.Conformitytofrequencyplan, out nullInds[CONFORMITYTOFREQUENCYPLAN]);
            CSV.ParseFieldAsString(fields, columnNames, FREQUENCYALLOCATIONNAME, 0, 200, out tAFL.Frequencyallocationname, out nullInds[FREQUENCYALLOCATIONNAME]);
            CSV.ParseFieldAsString(fields, columnNames, CHANNEL, 0, 200, out tAFL.Channel, out nullInds[CHANNEL]);
            CSV.ParseFieldAsString(fields, columnNames, INTERNATIONALCOORDINATIONNUMBER, 0, 200, out tAFL.Internationalcoordinationnumber, out nullInds[INTERNATIONALCOORDINATIONNUMBER]);
            CSV.ParseFieldAsString(fields, columnNames, ANALOGDIGITAL, 0, 200, out tAFL.Analogdigital, out nullInds[ANALOGDIGITAL]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, OCCUPIEDBANDWIDTHKHZ, double.MinValue, double.MaxValue, -1, out tAFL.OccupiedbandwidthkHz, out nullInds[OCCUPIEDBANDWIDTHKHZ]);
            CSV.ParseFieldAsString(fields, columnNames, DESIGNATIONOFEMISSION, 0, 200, out tAFL.Designationofemission, out nullInds[DESIGNATIONOFEMISSION]);
            CSV.ParseFieldAsString(fields, columnNames, MODULATIONTYPE, 0, 200, out tAFL.Modulationtype, out nullInds[MODULATIONTYPE]);
            CSV.ParseFieldAsString(fields, columnNames, FILTRATIONINSTALLED, 0, 200, out tAFL.Filtrationinstalled, out nullInds[FILTRATIONINSTALLED]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, TXEFFECTIVERADIATEDPOWERERPDBW, double.MinValue, double.MaxValue, -1, out tAFL.TxeffectiveradiatedpowerERPdBW, out nullInds[TXEFFECTIVERADIATEDPOWERERPDBW]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, TXTRANSMITTERPOWERW, double.MinValue, double.MaxValue, -1, out tAFL.TxtransmitterpowerW, out nullInds[TXTRANSMITTERPOWERW]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, TOTALLOSSESDB, double.MinValue, double.MaxValue, -1, out tAFL.TotallossesdB, out nullInds[TOTALLOSSESDB]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, ANALOGCAPACITYCHANNELS, double.MinValue, double.MaxValue, -1, out tAFL.AnalogcapacityChannels, out nullInds[ANALOGCAPACITYCHANNELS]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, DIGITALCAPACITYMBITS, double.MinValue, double.MaxValue, -1, out tAFL.DigitalcapacityMbits, out nullInds[DIGITALCAPACITYMBITS]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, RXUNFADEDRECEIVEDSIGNALLEVELDBW, double.MinValue, double.MaxValue, -1, out tAFL.RxunfadedreceivedsignalleveldBW, out nullInds[RXUNFADEDRECEIVEDSIGNALLEVELDBW]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, RXTHRESHOLDSIGNALLEVELFORBER10E3DBW, double.MinValue, double.MaxValue, -1, out tAFL.RxthresholdsignallevelforBER10e3dBW, out nullInds[RXTHRESHOLDSIGNALLEVELFORBER10E3DBW]);
            CSV.ParseFieldAsString(fields, columnNames, MANUFACTURER, 0, 200, out tAFL.Manufacturer, out nullInds[MANUFACTURER]);
            CSV.ParseFieldAsString(fields, columnNames, MODELNUMBER, 0, 200, out tAFL.Modelnumber, out nullInds[MODELNUMBER]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, ANTENNAGAINDBI, double.MinValue, double.MaxValue, -1, out tAFL.AntennagaindBi, out nullInds[ANTENNAGAINDBI]);
            CSV.ParseFieldAsString(fields, columnNames, ANTENNAPATTERN, 0, 200, out tAFL.Antennapattern, out nullInds[ANTENNAPATTERN]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, HALFPOWER3DBBEAMWIDTHDEG, double.MinValue, double.MaxValue, -1, out tAFL.Halfpower3dBbeamwidthdeg, out nullInds[HALFPOWER3DBBEAMWIDTHDEG]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, FRONTTOBACKRATIODB, double.MinValue, double.MaxValue, -1, out tAFL.FronttobackratiodB, out nullInds[FRONTTOBACKRATIODB]);
            CSV.ParseFieldAsString(fields, columnNames, POLARIZATION, 0, 200, out tAFL.Polarization, out nullInds[POLARIZATION]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, HEIGHTABOVEGROUNDLEVELM, double.MinValue, double.MaxValue, -1, out tAFL.Heightabovegroundlevelm, out nullInds[HEIGHTABOVEGROUNDLEVELM]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, AZIMUTHOFMAINLOBEDEG, double.MinValue, double.MaxValue, -1, out tAFL.Azimuthofmainlobedeg, out nullInds[AZIMUTHOFMAINLOBEDEG]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, VERTICALELEVATIONANGLEDEG, double.MinValue, double.MaxValue, -1, out tAFL.Verticalelevationangledeg, out nullInds[VERTICALELEVATIONANGLEDEG]);
            CSV.ParseFieldAsString(fields, columnNames, STATIONLOCATION, 0, 200, out tAFL.Stationlocation, out nullInds[STATIONLOCATION]);
            CSV.ParseFieldAsString(fields, columnNames, LICENSEESTATIONREFERENCE, 0, 200, out tAFL.Licenseestationreference, out nullInds[LICENSEESTATIONREFERENCE]);
            CSV.ParseFieldAsString(fields, columnNames, CALLSIGN, 0, 200, out tAFL.Callsign, out nullInds[CALLSIGN]);
            CSV.ParseFieldAsString(fields, columnNames, TYPEOFSTATION, 0, 200, out tAFL.Typeofstation, out nullInds[TYPEOFSTATION]);
            CSV.ParseFieldAsString(fields, columnNames, ITUCLASSOFSTATION, 0, 200, out tAFL.ITUclassofstation, out nullInds[ITUCLASSOFSTATION]);
            CSV.ParseFieldAsString(fields, columnNames, STATIONCOSTCATEGORY, 0, 200, out tAFL.Stationcostcategory, out nullInds[STATIONCOSTCATEGORY]);
            CSV.ParseFieldAsInt(fields, columnNames, NUMBEROFIDENTICALSTATIONS, int.MinValue, int.MaxValue, out tAFL.Numberofidenticalstations, out nullInds[NUMBEROFIDENTICALSTATIONS]);
            CSV.ParseFieldAsString(fields, columnNames, REFERENCEIDENTIFIER, 0, 200, out tAFL.Referenceidentifier, out nullInds[REFERENCEIDENTIFIER]);
            CSV.ParseFieldAsString(fields, columnNames, PROVINCES, 0, 200, out tAFL.Provinces, out nullInds[PROVINCES]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, LATITUDEWGS84, double.MinValue, double.MaxValue, -1, out tAFL.LatitudeWGS84, out nullInds[LATITUDEWGS84]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, LONGITUDEWGS84, double.MinValue, double.MaxValue, -1, out tAFL.LongitudeWGS84, out nullInds[LONGITUDEWGS84]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, GROUNDELEVATIONABOVEMEANSEALEVELM, double.MinValue, double.MaxValue, -1, out tAFL.Groundelevationabovemeansealevelm, out nullInds[GROUNDELEVATIONABOVEMEANSEALEVELM]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM, double.MinValue, double.MaxValue, -1, out tAFL.Antennastructureheightabovegroundlevelm, out nullInds[ANTENNASTRUCTUREHEIGHTABOVEGROUNDLEVELM]);
            CSV.ParseFieldAsString(fields, columnNames, CONGESTIONZONE, 0, 200, out tAFL.Congestionzone, out nullInds[CONGESTIONZONE]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, RADIUSOFOPERATIONKM, double.MinValue, double.MaxValue, -1, out tAFL.Radiusofoperationkm, out nullInds[RADIUSOFOPERATIONKM]);
            CSV.ParseFieldAsString(fields, columnNames, SATELLITENAME, 0, 200, out tAFL.Satellitename, out nullInds[SATELLITENAME]);
            CSV.ParseFieldAsString(fields, columnNames, AUTHORIZATIONNUMBER, 0, 200, out tAFL.Authorizationnumber, out nullInds[AUTHORIZATIONNUMBER]);
            CSV.ParseFieldAsString(fields, columnNames, MWSERVICE, 0, 200, out tAFL.MWService, out nullInds[MWSERVICE]);
            CSV.ParseFieldAsString(fields, columnNames, SUBSERVICE, 0, 200, out tAFL.Subservice, out nullInds[SUBSERVICE]);
            CSV.ParseFieldAsString(fields, columnNames, LICENCETYPE, 0, 200, out tAFL.Licencetype, out nullInds[LICENCETYPE]);
            CSV.ParseFieldAsString(fields, columnNames, AUTHORIZATIONSTATUS, 0, 200, out tAFL.Authorizationstatus, out nullInds[AUTHORIZATIONSTATUS]);
            CSV.ParseFieldAsString(fields, columnNames, INSERVICEDATE, 0, 200, out tAFL.Inservicedate, out nullInds[INSERVICEDATE]);
            CSV.ParseFieldAsString(fields, columnNames, ACCOUNTNUMBER, 0, 200, out tAFL.Accountnumber, out nullInds[ACCOUNTNUMBER]);
            CSV.ParseFieldAsString(fields, columnNames, LICENSEENAME, 0, 200, out tAFL.Licenseename, out nullInds[LICENSEENAME]);
            CSV.ParseFieldAsString(fields, columnNames, LICENSEEADDRESS, 0, 200, out tAFL.Licenseeaddress, out nullInds[LICENSEEADDRESS]);
            CSV.ParseFieldAsString(fields, columnNames, OPERATIONALSTATUS, 0, 200, out tAFL.Operationalstatus, out nullInds[OPERATIONALSTATUS]);
            CSV.ParseFieldAsString(fields, columnNames, STATIONCLASS, 0, 200, out tAFL.Stationclass, out nullInds[STATIONCLASS]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, HORIZONTALPOWERW, double.MinValue, double.MaxValue, -1, out tAFL.HorizontalpowerW, out nullInds[HORIZONTALPOWERW]);
            CSV.ParseFieldAsDoubleRound(fields, columnNames, VERTICALPOWERW, double.MinValue, double.MaxValue, -1, out tAFL.VerticalpowerW, out nullInds[VERTICALPOWERW]);
            CSV.ParseFieldAsString(fields, columnNames, STANDBYTRANSMITTERINFORMATION, 0, 200, out tAFL.Standbytransmitterinformation, out nullInds[STANDBYTRANSMITTERINFORMATION]);
            
            // The remaining fields do not exist in the ISED/TAFL CSV file so don't attempt to access them.

            //CSV.ParseFieldAsString(fields, columnNames, MICSOPER, 0, 200, out tAFL.MICSoper, out nullInds[MICSOPER]);
            //CSV.ParseFieldAsString(fields, columnNames, MICSOPNOTE, 0, 200, out tAFL.MICSopnote, out nullInds[MICSOPNOTE]);
            //CSV.ParseFieldAsString(fields, columnNames, MICSOPRTYP, 0, 200, out tAFL.MICSoprtyp, out nullInds[MICSOPRTYP]);
            //CSV.ParseFieldAsString(fields, columnNames, MICSCOMPANY, 0, 200, out tAFL.MICScompany, out nullInds[MICSCOMPANY]);
            //CSV.ParseFieldAsString(fields, columnNames, RECORDACTION, 0, 200, out tAFL.RecordAction, out nullInds[RECORDACTION]);
            //CSV.ParseFieldAsInt(fields, columnNames, KEYFIELD, int.MinValue, int.MaxValue, out tAFL.Keyfield, out nullInds[KEYFIELD]);
        }

        /// <summary>
        /// This method returns a CSV string that provides the names of the qty. 61 columns
        /// defined by the ISED TAFL specification.
        /// </summary>
        /// <returns></returns>
        public static string IsedTaflNumberedColumnNamesAsCSV()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < NUM_ISED_TAFL_COLUMNS; i++)
            {
                sb.Append('"');
                sb.Append(mNumberedColumnNames[i]);
                sb.Append('"');

                if (i < NUM_ISED_TAFL_COLUMNS - 1) sb.Append(",");
            }
                
            return sb.ToString();
        }

        /// <summary>
        /// This method returns a CSV string that provides the names of the qty. 61 columns
        /// defined by the ISED TAFL specification together with qty. 6 additional member
        /// column names added by Bill Venn to facilitate his TAFL import.
        /// </summary>
        /// <returns></returns>
        public static string ExtendedNumberedColumnNamesAsCSV()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < NUM_COLUMNS; i++)
            {
                sb.Append('"');
                sb.Append(mNumberedColumnNames[i]);
                sb.Append('"');

                if (i < NUM_COLUMNS - 1) sb.Append(",");
            }

            return sb.ToString();
        }

        public const string CREATE_TABLE = "" +
"CREATE TABLE {0}.{1}(" +
    "[TXRX] [varchar](2) NOT NULL," +
    "[FrequencyMhz] [float] NOT NULL," +
    "[Frequencyrecordidentifier] [varchar](255) NULL," +
    "[Regulatoryservice] [varchar](255) NULL," +
    "[CommunicationType] [varchar](255) NULL," +
    "[Conformitytofrequencyplan] [varchar](255) NULL," +
    "[Frequencyallocationname] [varchar](255) NULL," +
    "[Channel] [varchar](255) NULL," +
    "[Internationalcoordinationnumber] [varchar](255) NULL," +
    "[Analogdigital] [varchar](255) NULL," +
    "[OccupiedbandwidthkHz] [float] NULL," +
    "[Designationofemission] [varchar](255) NULL," +
    "[Modulationtype] [varchar](255) NULL," +
    "[Filtrationinstalled] [varchar](255) NULL," +
    "[TxeffectiveradiatedpowerERPdBW] [float] NULL," +
    "[TxtransmitterpowerW] [float] NULL," +
    "[TotallossesdB] [float] NULL," +
    "[AnalogcapacityChannels] [float] NULL," +
    "[DigitalcapacityMbits] [float] NULL," +
    "[RxunfadedreceivedsignalleveldBW] [float] NULL," +
    "[RxthresholdsignallevelforBER10e3dBW] [float] NULL," +
    "[Manufacturer] [varchar](255) NULL," +
    "[Modelnumber] [varchar](255) NULL," +
    "[AntennagaindBi] [float] NULL," +
    "[Antennapattern] [varchar](255) NULL," +
    "[Halfpower3dBbeamwidthdeg] [float] NULL," +
    "[FronttobackratiodB] [float] NULL," +
    "[Polarization] [varchar](255) NULL," +
    "[Heightabovegroundlevelm] [float] NULL," +
    "[Azimuthofmainlobedeg] [float] NULL," +
    "[Verticalelevationangledeg] [float] NULL," +
    "[Stationlocation] [varchar](255) NULL," +
    "[Licenseestationreference] [varchar](255) NULL," +
    "[Callsign] [varchar](255) NULL," +
    "[Typeofstation] [varchar](255) NULL," +
    "[ITUclassofstation] [varchar](255) NULL," +
    "[Stationcostcategory] [varchar](255) NULL," +
    "[Numberofidenticalstations] [int] NULL," +
    "[Referenceidentifier] [varchar](255) NULL," +
    "[Provinces] [varchar](255) NULL," +
    "[LatitudeWGS84] [float] NULL," +
    "[LongitudeWGS84] [float] NULL," +
    "[Groundelevationabovemeansealevelm] [float] NULL," +
    "[Antennastructureheightabovegroundlevelm] [float] NULL," +
    "[Congestionzone] [varchar](255) NULL," +
    "[Radiusofoperationkm] [float] NULL," +
    "[Satellitename] [varchar](255) NULL," +
    "[Authorizationnumber] [varchar](255) NULL," +
    "[MWService] [varchar](255) NULL," +
    "[Subservice] [varchar](4) NULL," +
    "[Licencetype] [varchar](255) NULL," +
    "[Authorizationstatus] [varchar](255) NULL," +
    "[Inservicedate] [varchar](255) NULL," +
    "[Accountnumber] [varchar](255) NULL," +
    "[Licenseename] [varchar](255) NULL," +
    "[Licenseeaddress] [varchar](255) NULL," +
    "[Operationalstatus] [varchar](255) NULL," +
    "[Stationclass] [varchar](255) NULL," +
    "[HorizontalpowerW] [float] NULL," +
    "[VerticalpowerW] [float] NULL," +
    "[Standbytransmitterinformation] [varchar](255) NULL," +
    "[MICSoper] [varchar](255) NULL," +
    "[MICSopnote] [varchar](255) NULL," +
    "[MICSoprtyp] [varchar](255) NULL," +
    "[MICScompany] [varchar](255) NULL," +
    "[RecordAction] [varchar](2) NULL," +
    "[keyfield] [int] NOT NULL," +
 "CONSTRAINT [{2}] PRIMARY KEY CLUSTERED " +
"(" +
    "[keyfield] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" +
") ON [PRIMARY]";


    }
}

```
