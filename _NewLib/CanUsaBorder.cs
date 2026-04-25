using _Configuration;
using _NewLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _NewLib
{
    /// <summary>
    /// This class provides methods that calculate the distance of a prescribed (lat, lng) 
    /// location from the Canada-USA border (including Alaska).
    /// </summary>
    public class CanUsaBorder
    {
        private const string PATH_TO_KML_FILE = @"D:\develbat\DataFiles\Canada_and_US_Border(Modified).kml";

        private static List<LatLng> mSouthernBorderVertices = new List<LatLng>();
        private static List<LatLng> mAlaskanBorderVertices = new List<LatLng>();

        // Define the radius of the earth as its Volumetric Radius.
        // The Volumetric Radius is the radius of a spherical Earth that 
        // has the same volume as the IAU 1968 Ellipsoid.
        const double EARTH_RADIUS_KM = 6371.0008; // kms.

        // Define the maximum distance for the local cartesian approximation.
        const double MAX_FLAT_DIST_KM = 200.0;

        // Define the maximum distance between interpolated vertices.
        const double MAX_VERTEX_SEPARATION = 10.0;

        public enum Border { SOUTHERN, ALASKAN, UNKNOWN }

        private static Details mDetails;

        public static Details Detail { get { return mDetails; } }

        public class Details
        {
            public string mPathToKmlFile = "";
            public bool mKmlFileIsOK = false;
            public string mLoadKmlFileErrMsg = "";
            public int mNumSouthernBorderVertices = Int32.MinValue;
            public int mNumAlaskanBorderVertices = Int32.MinValue;
            public LatLng mPrescribedLatLngPoint = new LatLng();
            public Border mBorder = Border.UNKNOWN;
            public LatLng mNearestVertex = new LatLng();
            public int mIndexOfNearestVertex = Int32.MinValue;
            public bool mLocallyFlat = false;
            public LatLng mOneAboveNearestVertex = new LatLng();
            public LatLng mOnebelowNearestVertex = new LatLng();
            public int mIndexOneAboveNearestVertex = Int32.MinValue;
            public int mIndexOneBelowNearestVertex = Int32.MinValue;
            public double mDistanceToNearestVertex = double.MaxValue;
            public Vector2D mV0 = new Vector2D();
            public Vector2D mV1 = new Vector2D();
            public Vector2D mV2above = new Vector2D();
            public Vector2D mV2below = new Vector2D();
            public double mRabove = double.MaxValue;
            public double mRbelow = double.MaxValue;
            public double mDistanceFotpAbove = double.MaxValue;
            public double mDistanceFotpBelow = double.MaxValue;
            public LatLng mFotpAboveLatLng = new LatLng();
            public LatLng mFotpBelowLatLng = new LatLng();
            public double mFinalDistanceToBorder = double.MaxValue;

            /// <summary>
            /// This method restores the values of the internal intermediate distance
            /// calculation variables to their initial state.
            /// </summary>
            public void ResetCalculationData()
            {
                mPrescribedLatLngPoint = new LatLng();
                mBorder = Border.UNKNOWN;
                mNearestVertex = new LatLng();
                mIndexOfNearestVertex = Int32.MinValue;
                mLocallyFlat = false;
                mOneAboveNearestVertex = new LatLng();
                mOnebelowNearestVertex = new LatLng();
                mIndexOneAboveNearestVertex = Int32.MinValue;
                mIndexOneBelowNearestVertex = Int32.MinValue;
                mDistanceToNearestVertex = double.MaxValue;
                mV0 = new Vector2D();
                mV1 = new Vector2D();
                mV2above = new Vector2D();
                mV2below = new Vector2D();
                mRabove = double.MaxValue;
                mRbelow = double.MaxValue;
                mDistanceFotpAbove = double.MaxValue;
                mDistanceFotpBelow = double.MaxValue;
                mFotpAboveLatLng = new LatLng();
                mFotpBelowLatLng = new LatLng();
                mFinalDistanceToBorder = double.MaxValue;
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
                sb.Append("\n ===== Details ===== ");

                sb.Append("\nmPathToKmlFile              = " + mPathToKmlFile);
                sb.Append("\nmKmlFileIsOK                = " + mKmlFileIsOK);
                sb.Append("\nmLoadKmlFileErrMsg          = " + mLoadKmlFileErrMsg);
                sb.Append("\nmNumSouthernBorderVertices  = " + mNumSouthernBorderVertices);
                sb.Append("\nmNumAlaskanBorderVertices   = " + mNumAlaskanBorderVertices);
                sb.Append("\nmPrescribedLocation         = " + mPrescribedLatLngPoint);
                sb.Append("\nmBorder                     = " + mBorder);
                sb.Append("\nmNearestVertex              = " + mNearestVertex);
                sb.Append("\nmIndexOfNearestVertex       = " + mIndexOfNearestVertex);
                sb.Append("\nmLocallyFlat                = " + mLocallyFlat);
                sb.Append("\nmOneAboveNearestVertex      = " + mOneAboveNearestVertex);
                sb.Append("\nmOnebelowNearestVertex      = " + mOnebelowNearestVertex);
                sb.Append("\nmIndexOneAboveNearestVertex = " + mIndexOneAboveNearestVertex);
                sb.Append("\nmIndexOneBelowNearestVertex = " + mIndexOneBelowNearestVertex);
                sb.Append("\nmDistanceToNearestVertex    = " + mDistanceToNearestVertex);
                sb.Append("\nmV0                         = " + mV0);
                sb.Append("\nmV1                         = " + mV1);
                sb.Append("\nmV2above                    = " + mV2above);
                sb.Append("\nmV2below                    = " + mV2below);
                sb.Append("\nmRabove                     = " + mRabove);
                sb.Append("\nmRbelow                     = " + mRbelow);
                sb.Append("\nmDistanceFotpAbove          = " + mDistanceFotpAbove);
                sb.Append("\nmDistanceFotpBelow          = " + mDistanceFotpBelow);
                sb.Append("\nmFotpAboveLatLng            = " + mFotpAboveLatLng);
                sb.Append("\nmFotpBelowLatLng            = " + mFotpBelowLatLng);
                sb.Append("\nmFinalDistanceToBorder      = " + mFinalDistanceToBorder);

                return sb.ToString();
            }
        }

        /// <summary>
        /// This class encapsulates the details of a KML Placemarck object. 
        /// </summary>
        public class Placemark
        {
            public string mFID = "";
            public int mSectionNum = int.MinValue;
            public string mSectionEng = "";
            public string mMaxScale = "";
            public string mGlobalID = "";
            public double mSHAPE_Length = double.MinValue;

            public List<LatLng> mLatLngPoints = new List<LatLng>();

            /// <summary>
            /// This method returns a formatted, multi-line string that shows the current
            /// values of this object's member variables.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("FID          = " + mFID);
                sb.Append("\nSectionNum   = " + mSectionNum);
                sb.Append("\nSectionEng   = " + mSectionEng);
                sb.Append("\nMaxScale     = " + mMaxScale);
                sb.Append("\nGlobalID     = " + mGlobalID);
                sb.Append("\nSHAPE_Length = " + mSHAPE_Length);
                sb.Append("\n");

                foreach (LatLng latLngPoint in mLatLngPoints)
                {
                    sb.Append(latLngPoint.ToString());
                    if (!latLngPoint.Equals(mLatLngPoints.Last())) sb.Append(" ");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// This method reads the contents of a KML file and creates two lists of (lat, lng) points,
        /// one list for the southern (48th parallel) border and the other list for the Alaskan border;
        /// in addition, the distance between sequential (lat, lng) points is calculated and, if required,
        /// synthetic 'interpolated' points are inserted to ensure the fidelity of the distance-to-border
        /// calculations.
        /// </summary>
        /// <returns></returns>
        public static bool LoadKmlMapData()
        {
            mDetails = new Details();

            mDetails.mPathToKmlFile = PATH_TO_KML_FILE;

            List<XmlNode> kmlPlacemarkList;
            List<Placemark> canUsaBorderPlacemarks = null;

            GetKmlPlacemarkList(PATH_TO_KML_FILE, out kmlPlacemarkList, out mDetails.mLoadKmlFileErrMsg);

            if (!String.IsNullOrWhiteSpace(mDetails.mLoadKmlFileErrMsg))
            {
                mDetails.mKmlFileIsOK = false;
                return false;
            }

            //...Log2.v("\nCanUSABorder.LoadKmlMapDataFromFile(): the KML/Document/Folder node contains qty. {0} <Placemark> nodes.", kmlPlacemarkList.Count);

            GetPlacemarks(kmlPlacemarkList, out canUsaBorderPlacemarks);

            bool ok = VerifyPlacemarkContinuity(canUsaBorderPlacemarks);
            if (!ok)
            {
                mDetails.mLoadKmlFileErrMsg = String.Format("\n\nCanUsaBorder.Go(): ERROR: call to VerifyPlacemarkContinuity() returned FALSE.");
                mDetails.mKmlFileIsOK = false;
                return false;
            }

            PopulateSouthernAlaskanLists(canUsaBorderPlacemarks, out mSouthernBorderVertices, out mAlaskanBorderVertices);

            mDetails.mNumSouthernBorderVertices = mSouthernBorderVertices.Count;
            mDetails.mNumAlaskanBorderVertices = mAlaskanBorderVertices.Count;

            //...Log2.v("\nSouthern: {0}, Alaskan: {1}", mSouthernBorderVertices.Count, mAlaskanBorderVertices.Count);

            // Now perform an analysis of vertex separartion and, if it is too large, interpolate
            // between vertices to achieve the desired maximum vertex separation.
            InterpolateBetweenVertices(mSouthernBorderVertices, MAX_VERTEX_SEPARATION, out mSouthernBorderVertices);
            InterpolateBetweenVertices(mAlaskanBorderVertices, MAX_VERTEX_SEPARATION, out mAlaskanBorderVertices);

            //WriteVerticesToFile(@"d:\MicsBatchLogs\interpolatedVertices.csv");

            mDetails.mKmlFileIsOK = true;
            return true;
        }

        /// <summary>
        /// This method parses a list of XmlNodes and creates a list of application Placemark objects.
        /// </summary>
        /// <param name="kmlPlacemarkList"></param>
        /// <param name="canUsaBorderPlacemarks"></param>
        /// <returns></returns>
        public static int GetPlacemarks(List<XmlNode> kmlPlacemarkList, out List<Placemark> canUsaBorderPlacemarks)
        {
            // 'out' requirement
            canUsaBorderPlacemarks = new List<Placemark>();

            int retVal = 0;

            foreach (XmlNode kmlPlacemark in kmlPlacemarkList)
            {
                XmlElement placemarkElement = (XmlElement)kmlPlacemark;
                XmlElement extendedData = (XmlElement)placemarkElement.GetElementsByTagName("ExtendedData")[0];
                XmlNodeList xmlNodeList = extendedData.GetElementsByTagName("SimpleData");

                Placemark canUsaPlacemark = new Placemark();

                IEnumerator ienum = xmlNodeList.GetEnumerator();
                while (ienum.MoveNext())
                {
                    XmlNode simpleData = (XmlNode)ienum.Current;

                    switch (simpleData.Attributes["name"].Value.Trim())
                    {
                        case "FID":
                            canUsaPlacemark.mFID = simpleData.InnerText;
                            break;
                        case "SectionNum":
                            canUsaPlacemark.mSectionNum = Convert.ToInt32(simpleData.InnerText);
                            break;
                        case "SectionEng":
                            canUsaPlacemark.mSectionEng = simpleData.InnerText;
                            break;
                        case "MaxScale":
                            canUsaPlacemark.mMaxScale = simpleData.InnerText;
                            break;
                        case "GlobalID":
                            canUsaPlacemark.mGlobalID = simpleData.InnerText;
                            break;
                        case "SHAPE_Length":
                            canUsaPlacemark.mSHAPE_Length = Convert.ToDouble(simpleData.InnerText);
                            break;
                        default:
                            break;
                    }
                }

                // Get the <coordinates> element of the current KmlPlacemark.
                XmlElement multiGoemetryElement = (XmlElement)placemarkElement.GetElementsByTagName("MultiGeometry")[0];
                XmlElement LineStringElement = (XmlElement)multiGoemetryElement.GetElementsByTagName("LineString")[0];
                XmlElement coordinatesElement = (XmlElement)multiGoemetryElement.GetElementsByTagName("coordinates")[0];

                // Extract the (Long, Lat) pairs of the current KmlPlacemark.
                string[] lngLatPairs = coordinatesElement.InnerText.Trim().Split(' ');
                foreach (string lngLatPair in lngLatPairs)
                {
                    string[] values = lngLatPair.Split(',');

                    double lat = Convert.ToDouble(values[1]);
                    double lng = Convert.ToDouble(values[0]);
                    LatLng latLngPoint = new LatLng(lat, lng);

                    canUsaPlacemark.mLatLngPoints.Add(latLngPoint);
                }

                canUsaBorderPlacemarks.Add(canUsaPlacemark);
            }

            return retVal;
        }

        /// <summary>
        /// This method serves the same role as a Main() method for an application - it is
        /// provided to integrate with the Tools multi-program application.
        /// </summary>
        /// <param name="args"></param>
        public static void Go(string[] args)
        {
            try
            {
                Details mDetails = new Details();

                bool kmlLoadWasOK = LoadKmlMapData();

                if (!kmlLoadWasOK)
                {
                    Log2.e("\nCanUsaBorder.Go(): ERROR: call to LoadKmlMapDataFromFile() FAILED, errMsg = {0}", mDetails.mLoadKmlFileErrMsg);
                    Application.ExitQuietly(1);
                }

                LatLng ottawa = new LatLng(45.41479, -75.7113);

                double distanceToBorderKms = GetDistanceToSouthernBorder(ottawa);
                //double distanceToBorderKms = GetDistanceToBorder(mSouthernBorderVertices, mAlaskanBorderVertices, ottawa, out border, out indexNearestVertex, ref mDetails);

                Console.Write("\n\n{0}", mDetails);

                while (true)
                {
                    Console.Write("\n\nEnter a (lat, lng) position: ");
                    Console.Out.Flush();
                    string inputStr = Console.ReadLine();
                    if (inputStr.Length == 0) break;

                    LatLng latLngPoint = new LatLng(inputStr);

                    distanceToBorderKms = GetDistanceToSouthernBorder(latLngPoint);
                    //distanceToBorderKms = GetDistanceToBorder(mSouthernBorderVertices, mAlaskanBorderVertices, latLngPoint, out border, out indexNearestVertex);

                    Console.Write("\n\n{0}", mDetails);
                }

            }
            catch (Exception e)
            {
                Console.Error.Write("\n\nCanUsaBorderKML.Go(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
                Application.ExitQuietly(666);
            }



            Application.ExitQuietly(0);

        }

        /// <summary>
        /// This method reverses the order of a hard-coded list of (lat, lng) pairs.
        /// </summary>
        public static void ReOrderLngLatPairs()
        {
            string longLatList = @"-66.902554774,44.776569932 -66.91069,44.75004 -66.98257,44.69609 -67.2788883219999,44.1866757820001 -67.282045948,44.178020114 -67.28520265,44.1693643460001 -67.28835843,44.1607084770001 -67.291513287,44.1520525080001 -67.2946672209999,44.1433964390001 -67.297820234,44.1347402700001 -67.300972326,44.126084 -67.304123497,44.1174276300001 -67.3072737489999,44.1087711600001 -67.31042308,44.10011459 -67.313571494,44.09145792 -67.31671899,44.0828011500001 -67.319865566,44.074144281 -67.3230112259999,44.065487311 -67.3261559699999,44.056830242 -67.3292997949999,44.048173073 -67.332442708,44.0395158050001 -67.3355847029999,44.0308584370001 -67.338725785,44.022200969 -67.341865952,44.013543402 -67.3450052059999,44.0048857350001 -67.348143546,43.9962279700001 -67.351280975,43.987570104 -67.3544174909999,43.978912139 -67.3575530959999,43.9702540750001 -67.36068779,43.9615959130001 -67.363821574,43.9529376510001 -67.3669544479999,43.9442792890001 -67.370086413,43.9356208290001 -67.3732174689999,43.92696227 -67.376347617,43.9183036110001 -67.3794768559999,43.9096448540001 -67.382605188,43.9009859980001 -67.3857326149999,43.8923270440001 -67.388859135,43.8836679890001 -67.391984749,43.875008838 -67.395109459,43.8663495870001 -67.398233263,43.857690237 -67.4013561639999,43.849030789 -67.404478162,43.840371243 -67.407599256,43.831711598 -67.410719447,43.823051854 -67.413838738,43.814392013 -67.4169571269999,43.8057320730001 -67.420074614,43.797072036 -67.423191201,43.788411899 -67.426306889,43.7797516640001 -67.429421678,43.771091332 -67.4325355669999,43.7624309010001 -67.435648558,43.7537703730001 -67.438760652,43.745109746 -67.441871847,43.736449022 -67.444982147,43.7277882000001 -67.4480915519999,43.7191272800001 -67.451200058,43.7104662620001 -67.454307672,43.701805146 -67.4574143899999,43.693143934 -67.4605202129999,43.6844826230001 -67.4636251439999,43.6758212140001 -67.4667291819999,43.667159709 -67.469832326,43.658498105 -67.472934578,43.649836405 -67.47603594,43.641174607 -67.479136411,43.632512713 -67.4822359899999,43.6238507200001 -67.485334681,43.61518863 -67.488432481,43.6065264440001 -67.491529392,43.59786416 -67.494625415,43.5892017800001 -67.4977205499999,43.580539301 -67.500814798,43.5718767270001 -67.50390816,43.5632140540001 -67.507000633,43.554551287 -67.510092223,43.5458884220001 -67.513182926,43.5372254590001 -67.516272745,43.528562401 -67.519361679,43.519899245 -67.522449729,43.5112359930001 -67.525536898,43.5025726440001 -67.5286231819999,43.4939092000001 -67.531708583,43.4852456580001 -67.534793104,43.476582019 -67.537876744,43.467918286 -67.540959502,43.4592544540001 -67.5440413809999,43.4505905280001 -67.547122379,43.441926504 -67.550202498,43.4332623860001 -67.55328174,43.4245981710001 -67.556360102,43.4159338580001 -67.5594375869999,43.407269451 -67.562514194,43.3986049480001 -67.565589926,43.389940349 -67.5686647799999,43.381275653 -67.57173876,43.372610862 -67.574811864,43.363945975 -67.577884094,43.3552809930001 -67.58095545,43.346615915 -67.584025931,43.337950741 -67.58709554,43.329285472 -67.5901642749999,43.320620106 -67.593232139,43.3119546450001 -67.596299131,43.3032890900001 -67.599365253,43.294623438 -67.6024305029999,43.2859576920001 -67.6054948829999,43.27729185 -67.6085583939999,43.2686259120001 -67.611621036,43.259959881 -67.614682809,43.251293753 -67.617743714,43.242627531 -67.620803751,43.233961213 -67.6238629219999,43.2252948 -67.6269212249999,43.216628293 -67.629978664,43.2079616890001 -67.633035235,43.199294992 -67.636090942,43.1906282 -67.6391457839999,43.181961313 -67.642199763,43.1732943310001 -67.645252877,43.164627255 -67.6483051289999,43.1559600840001 -67.651356517,43.147292818 -67.654407045,43.138625458 -67.65745671,43.1299580040001 -67.660505514,43.1212904540001 -67.663553457,43.1126228100001 -67.666600541,43.1039550720001 -67.6696467639999,43.0952872410001 -67.6726921279999,43.0866193150001 -67.675736634,43.0779512930001 -67.6787802819999,43.0692831790001 -67.681823072,43.0606149700001 -67.684865004,43.051946668 -67.6879060809999,43.0432782710001 -67.6909463009999,43.034609779 -67.693985665,43.025941195 -67.697024174,43.017272516 -67.7000618289999,43.008603744 -67.703098629,42.999934878 -67.706134575,42.9912659170001 -67.7091696689999,42.9825968640001 -67.71220391,42.973927716 -67.715237297,42.965258476 -67.718269833,42.956589142 -67.721301519,42.947919713 -67.724332353,42.939250192 -67.727362337,42.9305805770001 -67.73039147,42.921910869 -67.7334197559999,42.913241068 -67.736447191,42.9045711730001 -67.73947378,42.8959011860001 -67.74249952,42.8872311040001 -67.736614478,42.8794013720001 -67.7307309239999,42.871571326 -67.724848857,42.863740969 -67.718968277,42.855910296 -67.713089183,42.848079313 -67.707211574,42.8402480170001 -67.70133545,42.832416408 -67.695460808,42.824584486 -67.68958765,42.8167522540001 -67.683715975,42.8089197080001 -67.677845781,42.801086852 -67.6719770669999,42.793253684 -67.666109833,42.7854202030001 -67.66024408,42.7775864130001 -67.654379804,42.7697523100001 -67.648517007,42.761917897 -67.642655686,42.754083173 -67.636795843,42.746248139 -67.6309374739999,42.7384127930001 -67.625080581,42.7305771370001 -67.619225163,42.722741172 -67.613371217,42.7149048960001 -67.6075187449999,42.7070683110001 -67.601667745,42.699231415 -67.5958182159999,42.6913942100001 -67.589970158,42.683556695 -67.58412357,42.6757188720001 -67.578278451,42.66788074 -67.572434801,42.6600422980001 -67.566592618,42.652203549 -67.560751903,42.6443644890001 -67.554912655,42.6365251220001 -67.549074871,42.6286854460001 -67.543238553,42.620845462 -67.5374037,42.6130051710001 -67.5315703099999,42.6051645700001 -67.525738382,42.5973236630001 -67.519907917,42.5894824480001 -67.5140789129999,42.581640925 -67.50825137,42.5737990960001 -67.5024252869999,42.5659569600001 -67.496600663,42.5581145150001 -67.4907774979999,42.550271766 -67.4849557909999,42.542428709 -67.4791355409999,42.534585345 -67.473316747,42.526741675 -67.467499409,42.5188977 -67.4608125629999,42.511376528 -67.4541273199999,42.5038549560001 -67.447443681,42.496332985 -67.440761643,42.488810614 -67.434081205,42.4812878440001 -67.42740237,42.473764675 -67.420725132,42.466241107 -67.414049495,42.45871714 -67.407375455,42.451192775 -67.400703014,42.4436680100001 -67.394032168,42.436142848 -67.3873629189999,42.4286172870001 -67.380695265,42.421091329 -67.3740292059999,42.4135649740001 -67.367364741,42.40603822 -67.360701869,42.3985110690001 -67.354040589,42.3909835220001 -67.3473809,42.3834555780001 -67.3407228029999,42.375927237 -67.3340662969999,42.368398498 -67.3274113799999,42.360869365 -67.320758052,42.353339835 -67.314106312,42.345809909 -67.30745616,42.338279587 -67.300807595,42.330748871 -67.294160614,42.3232177580001 -67.2875152199999,42.3156862500001 -67.28087141,42.3081543480001 -67.274229185,42.3006220510001 -67.267588541,42.2930893590001 -67.2609494819999,42.2855562720001 -67.2543120029999,42.278022793 -67.247676106,42.2704889180001 -67.241041788,42.26295465 -67.2344090509999,42.255419988 -67.2277778929999,42.247884932 -67.221148312,42.2403494850001 -67.214520309,42.232813644 -67.2078938829999,42.2252774090001 -67.201269032,42.2177407820001 -67.194645758,42.210203763 -67.1880240579999,42.2026663510001 -67.18140393,42.1951285480001 -67.174785379,42.187590352 -67.168168397,42.180051765 -67.161552989,42.1725127870001 -67.154939151,42.1649734160001 -67.148326884,42.1574336560001 -67.141716185,42.149893503 -67.135107057,42.14235296 -67.128499496,42.1348120270001 -67.121893504,42.1272707040001 -67.1152890769999,42.1197289900001 -67.108686216,42.1121868860001 -67.102084922,42.1046443920001 -67.0954851929999,42.097101508 -67.0888870269999,42.0895582360001 -67.082290424,42.082014574 -67.075695384,42.0744705230001 -67.069101906,42.0669260830001 -67.062509989,42.0593812550001 -67.055919633,42.051836038 -67.049330836,42.044290432 -67.0427435989999,42.0367444390001 -67.0361579189999,42.0291980580001 -67.0295737979999,42.021651289 -67.022991233,42.0141041320001 -67.016410225,42.0065565880001 -67.009830772,41.999008657 -67.003252875,41.9914603390001 -66.9966765309999,41.983911635 -66.9901017409999,41.976362544 -66.9835285029999,41.9688130660001 -66.976956817,41.9612632020001 -66.9703866839999,41.953712952 -66.9638181,41.946162316 -66.9572510679999,41.9386112940001 -66.9506855829999,41.931059888 -66.944121647,41.923508095 -66.9375592599999,41.9159559180001 -66.93099842,41.9084033560001 -66.924439126,41.900850409 -66.917881377,41.893297078 -66.911325174,41.885743362 -66.904770516,41.878189261 -66.898217401,41.8706347770001 -66.891665828,41.8630799100001 -66.8851157989999,41.855524658 -66.878567312,41.8479690230001 -66.872020364,41.8404130060001 -66.865474956,41.8328566050001 -66.858931089,41.8252998200001 -66.8523887599999,41.817742654 -66.845847969,41.810185104 -66.8393087169999,41.8026271730001 -66.832771,41.795068859 -66.82623482,41.787510164 -66.819700175,41.779951087 -66.813167065,41.772391627 -66.806635489,41.764831788 -66.800105445,41.757271567 -66.793576935,41.749710963 -66.787049957,41.7421499800001 -66.780524509,41.7345886160001 -66.774000594,41.7270268720001 -66.767478207,41.7194647470001 -66.760957348,41.7119022420001 -66.754438019,41.704339357 -66.7479202169999,41.6967760920001 -66.741403942,41.6892124490001 -66.7348891949999,41.681648426 -66.728375972,41.674084023 -66.721864275,41.6665192410001 -66.7153541019999,41.658954081 -66.708845453,41.6513885420001 -66.702338326,41.6438226240001 -66.695832722,41.636256329 -66.68932864,41.6286896550001 -66.682826077,41.6211226030001 -66.676325036,41.6135551740001 -66.669825515,41.6059873670001 -66.663327511,41.598419183 -66.656831027,41.5908506210001 -66.650336059,41.5832816830001 -66.6438426079999,41.5757123670001 -66.6373506739999,41.568142676 -66.630860254,41.560572607 -66.6243713499999,41.553002162 -66.61788396,41.545431341 -66.611398083,41.5378601460001 -66.604913719,41.5302885730001 -66.5984308669999,41.522716626 -66.591949525,41.5151443020001 -66.585469695,41.5075716040001 -66.5789913739999,41.49999853 -66.572514563,41.4924250820001 -66.566039261,41.484851259 -66.559565467,41.477277062 -66.553093179,41.4697024910001 -66.5466223989999,41.462127545 -66.540153124,41.454552226 -66.5336853539999,41.4469765330001 -66.527219089,41.4394004660001 -66.520754327,41.4318240260001 -66.51429107,41.4242472120001 -66.507829315,41.4166700260001 -66.5013690599999,41.4090924670001 -66.494910308,41.401514535 -66.488453056,41.3939362310001 -66.481997303,41.386357554 -66.475543051,41.3787785050001 -66.4690902959999,41.3711990850001 -66.462639038,41.3636192930001 -66.4561892789999,41.3560391280001 -66.449741016,41.348458593 -66.443294248,41.3408776860001 -66.436848975,41.3332964090001 -66.430405196,41.32571476 -66.423962912,41.3181327410001 -66.4175221199999,41.3105503520001 -66.411082821,41.3029675920001 -66.4046450129999,41.295384461 -66.398208698,41.2878009620001 -66.3917738709999,41.280217092 -66.385340535,41.2726328520001 -66.378908687,41.2650482430001 -66.3724783279999,41.2574632660001 -66.366049458,41.249877918 -66.359622074,41.242292202 -66.3531961749999,41.2347061180001 -66.3467717629999,41.227119664 -66.340348835,41.219532842 -66.333927392,41.2119456520001 -66.327507434,41.204358095 -66.321088957,41.196770169 -66.314671963,41.189181875 -66.308256451,41.1815932150001 -66.30184242,41.174004186 -66.295429869,41.1664147910001 -66.289018797,41.1588250290001 -66.282609205,41.1512349000001 -66.27620109,41.143644404 -66.269794454,41.1360535420001 -66.263389294,41.128462314 -66.2569856109999,41.12087072 -66.2505834039999,41.113278759 -66.244182671,41.1056864330001 -66.237783414,41.0980937410001 -66.231385629,41.0905006850001 -66.2249893169999,41.082907264 -66.2185944789999,41.075313475 -66.212201111,41.0677193240001 -66.2058092139999,41.0601248070001 -66.1994187879999,41.052529926 -66.1930298309999,41.044934681 -66.1866423439999,41.0373390710001 -66.180256325,41.029743098 -66.173871774,41.022146761 -66.16748869,41.01455006 -66.1611070709999,41.0069529960001 -66.15472692,40.9993555680001 -66.1483482319999,40.991757777 -66.141971009,40.984159623 -66.135595251,40.9765611070001 -66.1292209539999,40.9689622290001 -66.1228481219999,40.9613629880001 -66.1164767499999,40.9537633840001 -66.110106838,40.946163418 -66.103738388,40.938563091 -66.097371398,40.930962402 -66.091005867,40.923361351 -66.0846417929999,40.9157599390001 -66.0782791789999,40.9081581670001 -66.071918021,40.9005560320001 -66.0655583189999,40.892953537 -66.0592000739999,40.8853506820001 -66.0528432839999,40.877747465 -66.0464879479999,40.8701438890001 -66.040134066,40.8625399520001 -66.0337816369999,40.8549356560001 -66.027430661,40.8473310000001 -66.021081137,40.839725984 -66.014733065,40.8321206090001 -66.008386442,40.824514874 -66.002041269,40.8169087800001 -65.995697546,40.8093023280001 -65.989355272,40.8016955160001 -65.983014446,40.7940883460001 -65.976675066,40.786480818 -65.970337135,40.7788729310001 -65.964000647,40.7712646860001 -65.957665606,40.763656084 -65.951332011,40.756047123 -65.944999858,40.748437806 -65.938669149,40.7408281300001 -65.932339884,40.7332180970001 -65.9260120599999,40.725607709 -65.919685678,40.717996961 -65.913360736,40.7103858580001 -65.907037235,40.702774398 -65.9007151719999,40.6951625830001 -65.89439455,40.68755041 -65.888075366,40.679937883 -65.881757619,40.6723249980001 -65.8754413089999,40.6647117590001 -65.8691264349999,40.6570981630001 -65.8628129969999,40.6494842130001 -65.856500994,40.6418699070001 -65.8501904249999,40.6342552460001 -65.84388129,40.6266402310001 -65.8375735879999,40.6190248610001 -65.831267318,40.6114091360001 -65.8249624809999,40.6037930570001 -65.818659075,40.5961766240001 -65.8123570979999,40.588559837 -65.806056552,40.580942695 -65.7997574349999,40.573325201 -65.7934597459999,40.5657073530001 -65.787163485,40.558089152 -65.780868652,40.5504705970001 -65.774575246,40.5428516900001 -65.768283265,40.5352324300001 -65.76199271,40.527612817 -65.7557035789999,40.519992851 -65.749415873,40.5123725330001 -65.74312959,40.5047518640001 -65.736844729,40.497130843 -65.730561291,40.489509469 -65.724279275,40.4818877440001 -65.717998678,40.474265668 -65.711719504,40.4666432400001 -65.705441747,40.4590204610001 -65.69916541,40.4513973310001";

            string[] longLatPairs = longLatList.Split(' ');

            string[] reorderedPairs = new string[longLatPairs.Length];

            for (int i = 0; i < longLatPairs.Length; i++)
            {
                int j = longLatPairs.Length - 1 - i;

                reorderedPairs[j] = longLatPairs[i];

                Console.Write("\nreorderedPairs[{0}] = {1}", j, reorderedPairs[j]);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < reorderedPairs.Length; i++)
            {
                sb.Append(reorderedPairs[i]);

                if (i != reorderedPairs.Length - 1) sb.Append(" ");
            }

            File.WriteAllText(@"d:\MicsBatchLogs\clickbait.txt", sb.ToString());
            return;
        }

        /// <summary>
        /// This method verifies the continuity of a list of Placemark objects by testing 
        /// whether the final (lat, lng) pair in one placemark object is identical to the
        /// first (lat, lng) pair in the next placemark in the list.
        /// </summary>
        /// <param name="placemarks"></param>
        /// <returns></returns>
        public static bool VerifyPlacemarkContinuity(List<Placemark> placemarks)
        {
            bool isOK = false;

            // Check that the final LatLng pair in this placemark is identical to the
            // first LatLng pair in the next placemark.
            int discontinuityCount = 0;
            for (int i = 0; i < placemarks.Count - 1; i++)
            {
                LatLng lastThisPlacemark = placemarks[i].mLatLngPoints.Last();
                LatLng firstNextPlacemark = placemarks[i + 1].mLatLngPoints.First();

                bool isContinuous = (lastThisPlacemark.Lat == firstNextPlacemark.Lat) && (lastThisPlacemark.Lng == firstNextPlacemark.Lng);

                if (!isContinuous) discontinuityCount++;

                string alert = (isContinuous) ? "" : "<<<";
                //...Log2.v("\n[{0,2}]: {1,-40}  [{2,2}]: {3, -40}  {4} {5}", i, lastThisPlacemark, i + 1, firstNextPlacemark, isContinuous, alert);
            }

            isOK = (discontinuityCount == 1) ? true : false;

            if (!isOK)
            {
                Log2.e("\n\nCanUsaBorder.VerifyPlacemarkContinuity(): ERROR: discontinuityCount = {0}", discontinuityCount);
            }

            // Perform an accounting of LatLng points in each placemark.
            for (int i = 0; i < placemarks.Count; i++)
            {
                //...Log2.v("\nCanUsaBorder.VerifyPlacemarkContinuity(): , {0}, {1}, ", i, placemarks[i].mLatLngPoints.Count);
            }

            return isOK;
        }

        /// <summary>
        /// This method partitions a list of all CAN-US border placemark objects into a list of placemarks
        /// defining the southern border and another list of placemarks defining the Alaskan border. 
        /// </summary>
        /// <param name="allCUsaBorderPlacemarks"></param>
        /// <param name="southernBorderLatLngPoints"></param>
        /// <param name="alaskanBorderLatLngPoints"></param>
        public static void PopulateSouthernAlaskanLists(List<Placemark> allCUsaBorderPlacemarks, out List<LatLng> southernBorderLatLngPoints, out List<LatLng> alaskanBorderLatLngPoints)
        {
            // 'out' requirement.
            southernBorderLatLngPoints = new List<LatLng>();
            alaskanBorderLatLngPoints = new List<LatLng>();

            List<LatLng> latLngs;

            foreach (Placemark canUsaBorderPlacemark in allCUsaBorderPlacemarks)
            {
                latLngs = canUsaBorderPlacemark.mLatLngPoints;

                if (canUsaBorderPlacemark.mSectionNum <= 26)
                {
                    // Add all the LatLng points to the accumulating list except the final one.
                    southernBorderLatLngPoints.AddRange(latLngs.GetRange(0, latLngs.Count - 1));

                    // If this is the final placemark for a border fragment then add the final point.
                    if (canUsaBorderPlacemark.mSectionNum == 26) southernBorderLatLngPoints.Add(latLngs.Last());
                }
                else
                {
                    // Add all the LatLng points to the accumulating list except the final one.
                    alaskanBorderLatLngPoints.AddRange(latLngs.GetRange(0, latLngs.Count - 1));

                    // If this is the final placemark for a border fragment then add the final point.
                    if (canUsaBorderPlacemark.mSectionNum == 28) alaskanBorderLatLngPoints.Add(latLngs.Last());
                }
            }

            //...Log2.v("\nCanUsaBorder.PopulateSouthernAlaskanLists(): southernBorderLatLngPoints.Count = {0}", southernBorderLatLngPoints.Count);
            //...Log2.v("\nCanUsaBorder.PopulateSouthernAlaskanLists(): alaskanBorderLatLngPoints.Count = {0}", alaskanBorderLatLngPoints.Count);
        }


        /// <summary>
        /// This method returns true if a prescribed (lat, lng) point is within a prescribed distance (km)
        /// of the southern CAN-US border.
        /// </summary>
        /// <param name="prescribedLatLngPoint"></param>
        /// <param name="prescribedDistance"></param>
        /// <returns></returns>
        public static bool IsWithinXkmOfSouthernBorder(LatLng prescribedLatLngPoint, double prescribedDistance)
        {
            mDetails.ResetCalculationData();

            return IsWithinXkmOfBorder(mSouthernBorderVertices, prescribedLatLngPoint, prescribedDistance);
        }

        /// <summary>
        /// This method returns true if a prescribed (lat, lng) point is within a prescribed distance (km)
        /// of the Alaskan CAN-US border.
        /// </summary>
        /// <param name="prescribedLatLngPoint"></param>
        /// <param name="prescribedDistance"></param>
        /// <returns></returns>
        public static bool IsWithinXkmOfAlaskanBorder(LatLng prescribedLatLngPoint, double prescribedDistance)
        {
            mDetails.ResetCalculationData();

            return IsWithinXkmOfBorder(mAlaskanBorderVertices, prescribedLatLngPoint, prescribedDistance);
        }

        /// <summary>
        /// This method returns the distance (km) between a prescribed (lat, lng) point and
        /// the southern CAN-US border.
        /// </summary>
        /// <param name="prescribedLatLngPoint"></param>
        /// <returns></returns>
        public static double GetDistanceToSouthernBorder(LatLng prescribedLatLngPoint)
        {
            mDetails.ResetCalculationData();

            double southernBorderDistance;

            southernBorderDistance = GetDistanceToBorder(mSouthernBorderVertices, prescribedLatLngPoint);

            mDetails.mPrescribedLatLngPoint = prescribedLatLngPoint;
            mDetails.mFinalDistanceToBorder = southernBorderDistance;
            mDetails.mBorder = Border.SOUTHERN;

            return southernBorderDistance;
        }

        /// <summary>
        /// This method returns the distance (km) between a prescribed (lat, lng) point and
        /// the Alaskan CAN-US border.
        /// </summary>
        /// <param name="prescribedLatLngPoint"></param>
        /// <returns></returns>
        public static double GetDistanceToAlaskanBorder(LatLng prescribedLatLngPoint)
        {
            mDetails.ResetCalculationData();

            double alaskanBorderDistance;

            alaskanBorderDistance = GetDistanceToBorder(mAlaskanBorderVertices, prescribedLatLngPoint);

            mDetails.mPrescribedLatLngPoint = prescribedLatLngPoint;
            mDetails.mFinalDistanceToBorder = alaskanBorderDistance;
            mDetails.mBorder = Border.ALASKAN;


            return alaskanBorderDistance;
        }


        /// <summary>
        /// This method returns a precise estimate (to within 100 m) of the distance of a prescribed (lat, lng) point 
        /// to a border defined as a list of (lat, lng) vertices; the algorithm first identifies the closest
        /// border segment (defined by a pair of points) and then calculates the position of the 'foot of the perpendicular'
        /// from the prescribed point to the closest straight line border segment.
        /// </summary>
        /// <param name="borderLatLngPoints"></param>
        /// <param name="prescribedLatLngPoint"></param>
        /// <returns></returns>
        public static double GetDistanceToBorder(List<LatLng> borderLatLngPoints, LatLng prescribedLatLngPoint)
        {
            int indexNearestVertex = 0;

            LatLng nearestVertex = new LatLng();

            double minVertexDistance = double.MaxValue;

            double lat1 = prescribedLatLngPoint.Lat;
            double lng1 = prescribedLatLngPoint.Lng;

            for (int i = 0; i < borderLatLngPoints.Count; i++)
            {
                LatLng thisVertex = borderLatLngPoints[i];
                double lat2 = thisVertex.Lat;
                double lng2 = thisVertex.Lng;

                double distance = _NewLib.LatLng.Vincenty(lat1, lng1, lat2, lng2);

                if (distance < minVertexDistance)
                {
                    minVertexDistance = distance;
                    indexNearestVertex = i;
                    nearestVertex = thisVertex;
                }
            }

            double refinedDistance = RefinedDistanceToBorder(borderLatLngPoints, prescribedLatLngPoint, indexNearestVertex, minVertexDistance);

            mDetails.mNearestVertex = nearestVertex;
            mDetails.mIndexOfNearestVertex = indexNearestVertex;
            mDetails.mDistanceToNearestVertex = minVertexDistance;
            mDetails.mOnebelowNearestVertex = (indexNearestVertex >= 1) ? borderLatLngPoints[indexNearestVertex - 1] : new LatLng(0, 0);
            mDetails.mOneAboveNearestVertex = (indexNearestVertex <= borderLatLngPoints.Count - 2) ? borderLatLngPoints[indexNearestVertex + 1] : new LatLng(0, 0);
            mDetails.mFinalDistanceToBorder = refinedDistance;

            return refinedDistance;
        }

        /// <summary>
        /// This method returns true if a prescribed (lat, lng) point is within a prescribed distance (km) of a border
        /// defined by a list of (lat, lng) vertices.
        /// </summary>
        /// <param name="borderLatLngPoints"></param>
        /// <param name="prescribedLatLngPoint"></param>
        /// <param name="prescribedDistance"></param>
        /// <returns></returns>
        public static bool IsWithinXkmOfBorder(List<LatLng> borderLatLngPoints, LatLng prescribedLatLngPoint, double prescribedDistance)
        {
            const double MARGIN = 100.0;

            int indexNearestVertex = 0;

            LatLng nearestVertex = new LatLng();

            double minVertexDistance = double.MaxValue;

            double lat1 = prescribedLatLngPoint.Lat;
            double lng1 = prescribedLatLngPoint.Lng;

            for (int i = 0; i < borderLatLngPoints.Count; i++)
            {
                LatLng thisVertex = borderLatLngPoints[i];
                double lat2 = thisVertex.Lat;
                double lng2 = thisVertex.Lng;

                // Try for quick rejection.
                double deltaLatDist = Math.Abs((lat1 - lat2) * Maths.DEG_TO_RAD * EARTH_RADIUS_KM);
                if (deltaLatDist > prescribedDistance + MARGIN)
                {
                    continue;
                }

                // Try for quick rejection.
                double deltaLngDist = Math.Abs(Maths.CosD(lat1) * (lng1 - lng2) * Maths.DEG_TO_RAD * EARTH_RADIUS_KM);
                if (deltaLngDist > prescribedDistance + MARGIN)
                {
                    continue;
                }

                double distance = _NewLib.LatLng.Vincenty(lat1, lng1, lat2, lng2);

                if (distance < minVertexDistance)
                {
                    minVertexDistance = distance;
                    indexNearestVertex = i;
                    nearestVertex = thisVertex;
                }
            }

            // Try for quick rejection.
            //if (minVertexDistance > prescribedDistance * MARGIN) return false;

            double refinedDistance = RefinedDistanceToBorder(borderLatLngPoints, prescribedLatLngPoint, indexNearestVertex, minVertexDistance);

            if (refinedDistance > prescribedDistance) return false;

            mDetails.mNearestVertex = nearestVertex;
            mDetails.mIndexOfNearestVertex = indexNearestVertex;
            mDetails.mDistanceToNearestVertex = minVertexDistance;
            mDetails.mOnebelowNearestVertex = (indexNearestVertex >= 1) ? borderLatLngPoints[indexNearestVertex - 1] : new LatLng(0, 0);
            mDetails.mOneAboveNearestVertex = (indexNearestVertex <= borderLatLngPoints.Count - 2) ? borderLatLngPoints[indexNearestVertex + 1] : new LatLng(0, 0);
            mDetails.mFinalDistanceToBorder = refinedDistance;

            return true;
        }

        /// <summary>
        /// This method performs the calculation of the position of the 'foot of the perpendicular' from
        /// a prescribed (lat, lng) point and the closest border straight line segment.
        /// </summary>
        /// <param name="borderLatLngPoints"></param>
        /// <param name="prescribedLatLngPoint"></param>
        /// <param name="indexNearestVertex"></param>
        /// <param name="distanceNearestVertex"></param>
        /// <returns></returns>
        public static double RefinedDistanceToBorder(List<LatLng> borderLatLngPoints, LatLng prescribedLatLngPoint, int indexNearestVertex, double distanceNearestVertex)
        {
            // Get the distance to the nearest vertex.
            double lat1 = prescribedLatLngPoint.Lat;
            double lng1 = prescribedLatLngPoint.Lng;
            double lat2 = borderLatLngPoints[indexNearestVertex].Lat;
            double lng2 = borderLatLngPoints[indexNearestVertex].Lng;
            //double distanceNearestVertex = _NewLib.LatLng.Vincenty(lat1, lng1, lat2, lng2);

            // Define P0.
            double lat0 = prescribedLatLngPoint.Lat;
            double lng0 = prescribedLatLngPoint.Lng;

            // Define P1.
            lat1 = borderLatLngPoints[indexNearestVertex].Lat;
            lng1 = borderLatLngPoints[indexNearestVertex].Lng;

            // Use P0 as the origin of local cartesian coordinates (x, y).
            double x0 = 0.0;
            double y0 = 0.0;
            double x1 = EARTH_RADIUS_KM * (lng1 - lng0) * Maths.DEG_TO_RAD * Maths.CosD(lat0);
            double y1 = EARTH_RADIUS_KM * (lat1 - lat0) * Maths.DEG_TO_RAD;
            double x2;
            double y2;

            // Instanciate 2-D vector objects vo and v1.
            Vector2D v0 = new Vector2D(x0, y0);
            Vector2D v1 = new Vector2D(x1, y1);
            Vector2D v2;

            mDetails.mV0 = v0;
            mDetails.mV1 = v1;

            // If distance between P0 and P1 exceeds the MAX_FLAT_DIST then return
            // just the distance to the nearest vertex, P1.
            mDetails.mLocallyFlat = (v1.Mod() <= MAX_FLAT_DIST_KM);
            if (!mDetails.mLocallyFlat)
            {
                return distanceNearestVertex;
            }

            double R;
            Vector2D vp;
            double perpDistBelow = double.MaxValue;
            double perpDistAbove = double.MaxValue;

            // Try foot-of-the-perpendicular (FOTP) analysis on indexNearestVertex and 1 below.
            if (indexNearestVertex >= 1)
            {
                // Define P2.
                lat2 = borderLatLngPoints[indexNearestVertex - 1].Lat;
                lng2 = borderLatLngPoints[indexNearestVertex - 1].Lng;

                x2 = EARTH_RADIUS_KM * (lng2 - lng0) * Maths.DEG_TO_RAD * Maths.CosD(lat0);
                y2 = EARTH_RADIUS_KM * (lat2 - lat0) * Maths.DEG_TO_RAD;

                v2 = new Vector2D(x2, y2);

                // If distance between P0 and P2 exceeds the MAX_FLAT_DIST then return
                // just the distance to the nearest vertex, P1.
                mDetails.mLocallyFlat = (v2.Mod() <= MAX_FLAT_DIST_KM);
                if (!mDetails.mLocallyFlat)
                {
                    return distanceNearestVertex;
                }

                Vector2D.FOTP(v0, v1, v2, out R, out vp, out perpDistBelow);

                // If the FOTP from P0 lies does not lie between P1 and P2 then change its value
                // to double.MaxValue; otherwise estimate the LatLng of the FOTP.
                if (Maths.InRange(0, 1, R))
                {
                    // Calculate the delta latitide and logitude w.r.t. P1.
                    double deltaLatDeg = Maths.RAD_TO_DEG * ((vp.Y - y1) / EARTH_RADIUS_KM);
                    double deltaLngDeg = Maths.RAD_TO_DEG * ((vp.X - x1) / (EARTH_RADIUS_KM * Maths.CosD(lat1)));

                    mDetails.mFotpBelowLatLng = new LatLng(lat1 + deltaLatDeg, lng1 + deltaLngDeg);
                }
                else
                {
                    perpDistBelow = double.MaxValue;
                }

                mDetails.mV2below = v2;
                mDetails.mRbelow = R;
                mDetails.mOnebelowNearestVertex = borderLatLngPoints[indexNearestVertex - 1];
                mDetails.mIndexOneBelowNearestVertex = indexNearestVertex - 1;
                mDetails.mDistanceFotpBelow = perpDistBelow;
            }

            // Try foot-of-the-perpendicular (FOTP) analysis on indexNearestVertex and 1 above.
            if (indexNearestVertex <= borderLatLngPoints.Count - 2)
            {
                lat2 = borderLatLngPoints[indexNearestVertex + 1].Lat;
                lng2 = borderLatLngPoints[indexNearestVertex + 1].Lng;

                //Console.Write("\n\nP0 = {0}, P1 = {1}, P2 = {2}", prescribedLatLngPoint, borderLatLngPoints[indexNearestVertex], borderLatLngPoints[indexNearestVertex + 1]);

                x2 = EARTH_RADIUS_KM * (lng2 - lng0) * Maths.DEG_TO_RAD * Maths.CosD(lat0);
                y2 = EARTH_RADIUS_KM * (lat2 - lat0) * Maths.DEG_TO_RAD;

                v2 = new Vector2D(x2, y2);

                // If distance between P0 and P2 exceeds the MAX_FLAT_DIST then return
                // just the distance to the nearest vertex, P1.
                // If distance between P0 and P2 exceeds the MAX_FLAT_DIST then return
                // just the distance to the nearest vertex, P1.
                mDetails.mLocallyFlat = (v2.Mod() <= MAX_FLAT_DIST_KM);
                if (!mDetails.mLocallyFlat)
                {
                    return distanceNearestVertex;
                }

                Vector2D.FOTP(v0, v1, v2, out R, out vp, out perpDistAbove);

                // If the FOTP from P0 lies does not lie between P1 and P2 then change its value
                // to double.MaxValue; otherwise estimate the LatLng of the FOTP.
                if (Maths.InRange(0, 1, R))
                {
                    // Calculate the delta latitide and logitude w.r.t. P1.
                    double deltaLatDeg = Maths.RAD_TO_DEG * ((vp.Y - y1) / EARTH_RADIUS_KM);
                    double deltaLngDeg = Maths.RAD_TO_DEG * ((vp.X - x1) / (EARTH_RADIUS_KM * Maths.CosD(lat1)));

                    mDetails.mFotpAboveLatLng = new LatLng(lat1 + deltaLatDeg, lng1 + deltaLngDeg);
                }
                else
                {
                    perpDistAbove = double.MaxValue;
                }

                mDetails.mV2above = v2;
                mDetails.mRabove = R;
                mDetails.mOneAboveNearestVertex = borderLatLngPoints[indexNearestVertex + 1];
                mDetails.mIndexOneAboveNearestVertex = indexNearestVertex + 1;
                mDetails.mDistanceFotpAbove = perpDistAbove;
            }

            double minPerpDist = Math.Min(perpDistBelow, perpDistAbove);
            return Math.Min(distanceNearestVertex, minPerpDist);
        }

        /// <summary>
        /// This method uses system XML capabilities to read the contents of a prescribed KML file and 
        /// return a list of parsed XmlNode objects.
        /// </summary>
        /// <param name="pathToKmlFile"></param>
        /// <param name="kmlPlacemarkList"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool GetKmlPlacemarkList(string pathToKmlFile, out List<XmlNode> kmlPlacemarkList, out string errMsg)
        {
            // 'out' requirement;
            kmlPlacemarkList = new List<XmlNode>();
            errMsg = "";

            // Instantiate a XmlDocument object.
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = false;

            // Attempt to load the KML file into the XmlDocument.
            try { xmlDocument.Load(pathToKmlFile); }
            catch
            {
                errMsg = String.Format("ERROR: xmlDocument.Load() failed for path: {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            // Check that the file has XML content.
            if (!xmlDocument.HasChildNodes)
            {
                errMsg = String.Format("ERROR: the file does not contain any top-level XML nodes: {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            // Check that the file begins with an XML prolog element.
            if (xmlDocument.FirstChild.Name != "xml")
            {
                errMsg = String.Format("ERROR: the file does not contain an initial <?xml> prolog element: {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            // A properly formatted KML file should have exactly qty. 2 top-level children.
            XmlNodeList nodes = xmlDocument.ChildNodes;

            if (nodes.Count == 1)
            {
                errMsg = String.Format("ERROR: the file does not have a <kml> top-level node: {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }
            if (nodes.Count > 2)
            {
                errMsg = String.Format("ERROR: the file has too many top-level nodes: {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            // The first child is like  : <?xml version="1.0" encoding="utf-8" ?>
            // The second child is like : <kml xmlns="http://www.opengis.net/kml/2.2"> ... </kml>
            // Check that the second top-level child is named 'kml'.
            if (nodes[1].Name != "kml")
            {
                errMsg = String.Format("ERROR: the second top-level node is not <kml> : {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            XmlElement kmlRoot = (XmlElement)nodes[1];

            // So we now have the top-level 'kml' root element.
            // Verify that it has exactly one child - a <Document> node.
            if (kmlRoot.ChildNodes.Count != 1)
            {
                errMsg = String.Format("ERROR: the <kml> root node must contain exactly one child : {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            XmlNodeList kmlDocumentNodeList = kmlRoot.GetElementsByTagName("Document");

            if (kmlDocumentNodeList.Count != 1)
            {
                errMsg = String.Format("ERROR: the <kml> root element has no <Document> node : {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            // The kml/document node must contain a single <Folder> node.
            XmlElement kmlDocumentNode = (XmlElement)kmlDocumentNodeList[0];

            XmlNodeList folderNodeList = kmlDocumentNode.GetElementsByTagName("Folder");

            if (folderNodeList.Count == 0)
            {
                errMsg = String.Format("ERROR: the kml/Document node has no <Folder> child node : {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }
            if (folderNodeList.Count > 1)
            {
                errMsg = String.Format("ERROR: the kml/Document node has multiple <Folder> child nodes : {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            // Verify that the single <Folder> has child nodes called <Placemark>.
            // Check that the kml/Document/Folder node contains one or more Placemark nodes.
            XmlElement folderNode = (XmlElement)folderNodeList[0];

            XmlNodeList xmlNodeList = folderNode.GetElementsByTagName("Placemark");

            kmlPlacemarkList = new List<XmlNode>(xmlNodeList.Cast<XmlNode>());

            if (kmlPlacemarkList.Count == 0)
            {
                errMsg = String.Format("ERROR: the kml/Document/Folder element has no <Placemark> child nodes : {0}", pathToKmlFile);
                Log2.e("\n\nCanUsaBorder.GetKmlPlacemarkList(): {0}", errMsg);
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method writes the (lat, lng) vertices that define the southern and Alaskan parts
        /// of the CAN-US border to a prescribed file path.
        /// </summary>
        /// <param name="pathToFile"></param>
        public static void WriteVerticesToFile(string pathToFile)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < mSouthernBorderVertices.Count - 1; i++)
            {
                LatLng thisVertexLatLng = mSouthernBorderVertices[i];
                LatLng nextVertexLatLng = mSouthernBorderVertices[i + 1];

                double greatCircleDist = LatLng.Haversine(thisVertexLatLng, nextVertexLatLng);

                sb.Append(String.Format("\nSOUTHERN,{0},{1},{2},{3}", i, thisVertexLatLng.ToString(), nextVertexLatLng.ToString(), greatCircleDist));
            }

            for (int i = 0; i < mAlaskanBorderVertices.Count - 1; i++)
            {
                LatLng thisVertexLatLng = mAlaskanBorderVertices[i];
                LatLng nextVertexLatLng = mAlaskanBorderVertices[i + 1];

                double greatCircleDist = LatLng.Haversine(thisVertexLatLng, nextVertexLatLng);

                sb.Append(String.Format("\nALASKAN,{0},{1},{2},{3}", i, thisVertexLatLng.ToString(), nextVertexLatLng.ToString(), greatCircleDist));
            }

            File.WriteAllText(pathToFile, sb.ToString());
        }

        /// <summary>
        /// This method inputs a list of (lat, lng) vertices defining a border, calculates the length of each straight line
        /// segments and, where required, inserts synthetic 'interpolated' (lat, lng) points to ensure that the distance
        /// between any neighbouring pair of vertices is less than a prescribed minimum distance.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="minDistanceKm"></param>
        /// <param name="interpolatedVertices"></param>
        public static void InterpolateBetweenVertices(List<LatLng> vertices, double minDistanceKm, out List<LatLng> interpolatedVertices)
        {
            // 'out' requirement.
            interpolatedVertices = new List<LatLng>();

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                LatLng thisVertex = vertices[i];
                LatLng nextVertex = vertices[i + 1];

                // Accumulate this vertex.
                interpolatedVertices.Add(thisVertex);

                // Check whether interpolation is required.
                double greatCircleDist = LatLng.Haversine(thisVertex, nextVertex);

                if (greatCircleDist > minDistanceKm)
                {
                    // First calculate the number of interpolation points.
                    int numInterpolationPoints = (int)(greatCircleDist / minDistanceKm) + 1;

                    for (int j = 1; j < numInterpolationPoints; j++)
                    {
                        double t = ((double)j) / ((double)numInterpolationPoints);

                        double latInterp = (1 - t) * thisVertex.Lat + t * nextVertex.Lat;
                        double lngInterp = (1 - t) * thisVertex.Lng + t * nextVertex.Lng;

                        // Accumulate the interpolated points.
                        interpolatedVertices.Add(new LatLng(latInterp, lngInterp));
                    }
                }

            }

            // Remember to append the last vertex.
            interpolatedVertices.Add(vertices.Last());
        }

        public static string GetDetailsAsString() { return mDetails.ToString(); }

    }
}

