using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// Commonly used enumeration constants.
    /// </summary>
    public class Enums
    {
        public enum BuildConfig { RELEASE, DEBUG };

        public enum RecType { UNKNOWN, INFO, ACTIVE, PASSIVE };

        public enum COMMIT { AUTO, MANUAL };

        public enum AnteUse { eUNKNOWN, eTX, eRX, eTR };

        public enum SF { SUCCESS, FAIL };

        public enum BIT { SET, CLEAR, FAIL };

        public enum MDB { WILL_NOT_CHANGE, WILL_CHANGE };

        public enum DB { FT, MDB };

        public enum HiLo { Hi, Lo, NotSet, Violation };

        //public enum HiLoSense { Hi, Lo, NotSet };

        //public enum HiLoGroupState { OK, Violation}

        public enum DbGetForNullMode { SET_NULLS_AS_MAX_FOR_TYPE, SET_NULLS_AS_ZERO };

        public enum MapUsed { e250K, e50K };

        public enum Ecalctype { eMINUSI, eCOVERI };

        public enum Ereqdtype { eTABLE, eCALC };

        public enum PRF { OK, NOMAP, READERR, NOMEM, BADHEADER, NODATA };

        public enum FeImpQual { UNKNOWN, TE, TD, LK, GK, SK, SD, AK, AT, AR, AS, ZK, CK, CT, CR }

        public enum FeImpQualGroup { UNKNOWN, TITLE, CHANGE_OF_LOCATION, CHANGE_OF_CALLSIGN, SITE, ANTENNA, AZIMUTH, CHANNEL }

        public enum FeImpMand { M, A, O }   // M = Mandatory; 
                                            // A = Mandatory if Add;
                                            // O = Optional.
        public enum FtImpQual { EOF, UNKNOWN, TT, GK, SK, SD, AK, AQ, AO, CK, CT, CR, CQ, CO }

        public enum FtImpQualGroup { UNKNOWN, TITLE, CHANGE_OF_CALLSIGN, SITE, ANTENNA, CHANNEL }

        public enum FtImpMand { M, A, O }   // M = Mandatory; 
                                            // A = Mandatory if Add;
                                            // O = Optional.

        public enum NTv2Dir { NAD27to83, NAD83to27 }

        public enum Datum { NAD_83_DATUM, NAD_27_DATUM };

        public enum eRepType { eHTML, eNOHTML, eCSV, ePL3 };

        public enum OHLerrStat { PRF_OK, PRF_NOMAP, PRF_READERR, PRF_NOMEM, PRF_BADHEADER, PRF_NODATA };

        public enum OHLcalcType { OHL_LOS, OHL_SKE, OHL_ISOL, OHL_DKE, OHL_IRT };

        public enum PFDcalcMode { PFDCONTOUR, COVERAGE50, COVERAGE90 };

        public enum PFDloss { SPHERICAL, TERRAIN };

        public enum PFDclimate { CONTINENTAL, MARITIME };

        public enum PercentTime { FIFTY, EIGHTY, NINETY };

        public enum BaseRateCode {REMOTE, RURAL, URBAN, UNKNOWN };

    };
}
