using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace T5aPolygons
{
    /// <summary>
    /// This class encapsulates an ISED Tier 5 Area geomatic data set and associated methods.
    /// </summary>
    [Serializable]
    public class Tier5Area
    {
        private string mID;
        private BaseRateCode mBaseRateCode;
        private List<GeoBoundary> mGeoBoundaries;


        public string ID { get { return mID; } set { mID = value; } }
        public BaseRateCode BaseRateCode { get { return mBaseRateCode; } set { mBaseRateCode = value; } }
        public List<GeoBoundary> GeoBoundaries { get { return mGeoBoundaries; } set { GeoBoundaries = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Tier5Area()
        {
            mID = "";
            mBaseRateCode = BaseRateCode.UNKNOWN;
            mGeoBoundaries = new List<GeoBoundary>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="baseRateCode"></param>
        /// <param name="geoBoundaries"></param>
        public Tier5Area(string iD, BaseRateCode baseRateCode, List<GeoBoundary> geoBoundaries)
        {
            mID = iD;
            mBaseRateCode = baseRateCode;
            mGeoBoundaries = geoBoundaries;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string
        /// that provides all the values of the object's private member variables.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MetaDataToString());

            int nBoundary = 0;
            foreach (GeoBoundary geoBoundary in mGeoBoundaries)
            {
                sb.Append("\n  Boundary #" + nBoundary++);
                int i = 0;
                foreach (GeoPoint geoPoint in geoBoundary.Points)
                {
                    i++;
                    sb.Append(String.Format("\n     Point : {0,5};  {1}", i, geoPoint.ToString()));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method returns an annotated, formatted, string
        /// that provides the object's Tier 5 Area ID and ISED base rate code (URBAN, RURAL or REMOTE).
        /// </summary>
        /// <returns></returns>
        public string MetaDataToString()
        {
            //                      123456789012345     123456789012345
            return String.Format("\nAreaName:      {0}\nBaseRateCode:  {1}", mID, mBaseRateCode);
        }

        /// <summary>
        /// This method returns true if the prescribed GeoPoint object is enclosed by
        /// any of this object's list of GeoBoundary objects; otherwise false.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Encloses(GeoPoint p)
        {
            bool doesEnclose = false;

            foreach (GeoBoundary geoBoundary in mGeoBoundaries)
            {
                doesEnclose |= geoBoundary.Encloses(p);
            }

            return doesEnclose;
        }

        /// <summary>
        /// This method returns the first Tier5Area object that encloses a prescribed GeoPoint object
        /// given an input list of Tier5Area objects; otherwise null.
        /// </summary>
        /// <param name="tier5Areas"></param>
        /// <param name="p"></param>
        /// <param name="tier5Area"></param>
        /// <returns></returns>
        public static bool Encloses(List<Tier5Area> tier5Areas, GeoPoint p, out Tier5Area tier5Area)
        {
            // 'out' requirement.
            tier5Area = null;

            bool doesEnclose = false;

            foreach (Tier5Area t5A in tier5Areas)
            {
                if (t5A.Encloses(p))
                {
                    doesEnclose = true;
                    tier5Area = t5A;
                    break;
                }
            }

            return doesEnclose;
        }

        /// <summary>
        /// This method exports (writes) a list of Tier5Area objects to a prescribed file path in SERIALIZED format.
        /// </summary>
        /// <param name="tier5Areas"></param>
        /// <param name="exportFilePath"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool ExportAsSerialized(List<Tier5Area> tier5Areas, string exportFilePath, out string errMsg)
        {
            // 'out' requirement;
            errMsg = "";

            bool isSuccess = true;

            // Check that the prescribed file path can actually be written to.
            // Note: this will delete an existing file's content.
            if (!ImportExport.CanWriteToFile(exportFilePath, out errMsg))
            {
                isSuccess = false;
            }
            else
            {
                try
                {
                    Stream stream = File.Open(exportFilePath, FileMode.Create);
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tier5Areas);
                    stream.Close();
                }
                catch (Exception e)
                {
                    isSuccess = false;
                    errMsg = e.Message;
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// This method imports (reads) a list of Tier5Area objects from a prescribed file path in SERIALIZED format.
        /// </summary>
        /// <param name="tier5Areas"></param>
        /// <param name="importFilePath"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool ImportAsSerialized(out List<Tier5Area> tier5Areas, string importFilePath, out string errMsg)
        {
            // 'out' requirement;
            tier5Areas = new List<Tier5Area>();
            errMsg = "";

            bool isSuccess = true;

            try
            {
                Stream stream = File.Open(importFilePath, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                tier5Areas = (List<Tier5Area>)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
                isSuccess = false;
                errMsg = e.Message;
            }

            return isSuccess;
        }

        /// <summary>
        /// This method returns the Tier5Area object from a list of Tier5Area objects whose
        /// Area ID matches a prescribed string.
        /// </summary>
        /// <param name="tier5Areas"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Tier5Area GetAreaByID(List<Tier5Area> tier5Areas, string ID)
        {
            Tier5Area tier5Area = null;

            if (tier5Areas != null)
            {
                foreach (Tier5Area t5A in tier5Areas)
                {
                    if (t5A.ID == ID)
                    {
                        tier5Area = t5A;
                        break;
                    }
                }
            }

            return tier5Area;
        }

        /// <summary>
        /// This method returns the ISED rate, in $ per MHz, pertaining to this object for a
        /// prescribed transmit frequency in KHz.
        /// </summary>
        /// <param name="freqKHz"></param>
        /// <returns></returns>
        public double DollarsPerMHz(double freqKHz)
        {
            return Tier5Area.DollarsPerMHz(this.BaseRateCode, freqKHz);
        }

        /// <summary>
        /// This method returns the ISED rate, in $ per MHz, for a prescribed ISED 
        /// base rate code (RURAL, URBAN, REMOTE) and a prescribed transmit frequency in KHz.
        /// </summary>
        /// <param name="baseRateCode"></param>
        /// <param name="freqKHz"></param>
        /// <returns></returns>
        public static double DollarsPerMHz(BaseRateCode baseRateCode, double freqKHz)
        {
            const double KILO = 1E3;
            const double MEGA = 1E6;
            const double GIGA = 1E9;

            double value = Double.MinValue;
            double freqHz = freqKHz * KILO;

            switch (baseRateCode)
            {
                case BaseRateCode.URBAN:
                    value = 2750;
                    break;
                case BaseRateCode.RURAL:
                    value = 2200;
                    break;
                case BaseRateCode.REMOTE:
                    value = 1375; ;
                    break;
            }

            if (freqHz <= 890 * MEGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 2750;
                        break;
                    case BaseRateCode.RURAL:
                        value = 2200;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 1375; ;
                        break;
                }
            }
            else if (freqHz > 890 * MEGA && freqHz <= 960 * MEGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 138;
                        break;
                    case BaseRateCode.RURAL:
                        value = 110.4;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 69; ;
                        break;
                }
            }
            else if (freqHz > 960 * MEGA && freqHz <= 4200 * MEGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 45;
                        break;
                    case BaseRateCode.RURAL:
                        value = 36;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 22.5; ;
                        break;
                }
            }
            else if (freqHz > 4.2 * GIGA && freqHz <= 8.5 * GIGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 34;
                        break;
                    case BaseRateCode.RURAL:
                        value = 27.2;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 17;
                        break;
                }
            }
            else if (freqHz > 8.5 * GIGA && freqHz <= 15.35 * GIGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 24;
                        break;
                    case BaseRateCode.RURAL:
                        value = 19.2;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 12;
                        break;
                }
            }
            else if (freqHz > 15.35 * GIGA && freqHz <= 24.25 * GIGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 16;
                        break;
                    case BaseRateCode.RURAL:
                        value = 12.8;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 8;
                        break;
                }
            }
            else if (freqHz > 24.25 * GIGA && freqHz <= 52.6 * GIGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 10;
                        break;
                    case BaseRateCode.RURAL:
                        value = 8;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 5;
                        break;
                }
            }
            else if (freqHz > 52.6 * GIGA && freqHz <= 92 * GIGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 0.5;
                        break;
                    case BaseRateCode.RURAL:
                        value = 0.4;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 0.25;
                        break;
                }
            }
            else if (freqHz > 92 * GIGA)
            {
                switch (baseRateCode)
                {
                    case BaseRateCode.URBAN:
                        value = 0.5;
                        break;
                    case BaseRateCode.RURAL:
                        value = 0.4;
                        break;
                    case BaseRateCode.REMOTE:
                        value = 0.25;
                        break;
                }
            }

            return value;
        }

        /// <summary>
        /// This method returns the ID of the Tier5Area object that encloses a prescribed Geopoint 
        /// object given an input list of Tier5Area objects.
        /// </summary>
        /// <param name="tier5Areas"></param>
        /// <param name="geoPoint"></param>
        /// <returns></returns>
        public static string LookUpTier5AreaID(List<Tier5Area> tier5Areas, GeoPoint geoPoint)
        {
            string id = "";

            if (tier5Areas == null || tier5Areas.Count == 0)
            {
                    Log2.e("\n\nTier5Area.LookUpTier5AreaID(): ERROR: List<Tier5Area> tier5Areas is NULL or empty.");
                    return "";
            }

            foreach (Tier5Area tier5Area in tier5Areas)
            {
                if (tier5Area.Encloses(geoPoint))
                {
                    id = tier5Area.ID;
                    break;
                }
            }

            return id;
        }

    }
}

