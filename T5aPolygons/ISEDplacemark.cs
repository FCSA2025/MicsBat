// Disable the compiler warning about unreachable code.
#pragma warning disable 0162

using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace T5aPolygons
{
    /// <summary>
    /// This class provides data and methods that encapsulate the notion
    /// of a KML 'placemark' XML hierarchy as used in the Tier 5 Area geomatic data
    /// provided by ISED.
    /// </summary>
    [Serializable]
    public class ISEDplacemark
    {
        private string mName = "";
        private string mDescription = "";
        private string mTier = "";
        // A single GeoArea can have multiple GeoBoundaries.
        private List<GeoBoundary> mBoundaries = new List<GeoBoundary>();
        public static NameComparer Comparer = new NameComparer();

        public class NameComparer : IComparer<ISEDplacemark>
        {
            /// <summary>
            /// This method compares the name strings of two prescribed ISEDplacemark
            /// objects and returns the values -1, 0 or +1 signifying their lexical order.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(ISEDplacemark x, ISEDplacemark y)
            {
                return String.Compare(x.Name, y.Name);
            }
        }

        public string Name { get { return mName; } set { mName = value; } }

        public string Description { get { return mDescription; } set { mDescription = value; } }

        public string Tier { get { return mTier; } set { mTier = value; } }

        public List<GeoBoundary> Boundaries { get { return mBoundaries; } set { mBoundaries = value; } }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string
        /// that provides all the values of the object's private member variables.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(MetaDataToString());

            int i = 0;
            foreach (GeoBoundary geoBoundary in mBoundaries)
            {
                i++;
                sb.Append(String.Format("\nBoundary: {0,5};  {1}", i, geoBoundary.ToString()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a formatted, annotated string comprising this 
        /// placemark object's name, description and tier.
        /// </summary>
        /// <returns></returns>
        public string MetaDataToString()
        {
            return String.Format("name:           {0}\ndescription:    {1}\ntier:           {2}",
                                    mName, mDescription, mTier);
        }

        /// <summary>
        /// This method reads text from a single file parses its KML XML tree and outputs
        /// a list of ISEDplacemark objects.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="geoRegions"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool ImportFromKMLfile(string path, out List<ISEDplacemark> geoRegions, out string errMsg)
        {
            // 'out' requirement.
            geoRegions = new List<ISEDplacemark>();
            errMsg = null;

            bool isValid = true;
            XmlDocument doc = new XmlDocument();
            string str;

            try
            {
                // Try to load the XML document.
                // This will catch any file path errors.
                try
                {
                    doc.Load(path);
                }
                catch (Exception e)
                {
                    errMsg = String.Format("ERROR: doc.Load(path) failed for path: {0}\nReason:\n", path, e.Message);
                    return false;
                }

                string filename = Path.GetFileName(path);

                // Check that the file has XML content.
                if (!doc.HasChildNodes)
                {
                    errMsg = String.Format("ERROR: the file does not contain any top-level XML nodes: {0}", filename);
                    return false;
                }

                // Check that the file begins with an XML prolog element.
                if (doc.FirstChild.Name != "xml")
                {
                    errMsg = String.Format("ERROR: the file does not contain an initial <?xml> prolog element: {0}", filename);
                    return false;
                }

                XmlNodeList nodes = doc.ChildNodes;

                // Ensure that it has exactly one root element.
                if (nodes.Count == 1)
                {
                    errMsg = String.Format("ERROR: the file does not contain an XML root element: {0}", filename);
                    return false;
                }
                if (nodes.Count > 2)
                {
                    errMsg = String.Format("ERROR: the file contains multiple XML root elements: {0}", filename);
                    return false;
                }

                // Check that the root element is named 'kml'..
                if (nodes[1].Name != "kml")
                {
                    errMsg = String.Format("ERROR: the root element is not <kml> : {0}", filename);
                    return false;
                }

                XmlElement root = (XmlElement)nodes[1];


                // So we now have the top-level 'kml' root element.
                // Attempt to get the 'Document' child.
                if (!root.HasChildNodes)
                {
                    errMsg = String.Format("ERROR: the <kml> root element has no child nodes : {0}", filename);
                    return false;
                }

                XmlNodeList rootChildNodes = root.ChildNodes;

                if (rootChildNodes.Count > 1)
                {
                    errMsg = String.Format("ERROR: the <kml> root element has multiple child nodes : {0}", filename);
                    return false;
                }

                XmlElement documentNode = (XmlElement)root.FirstChild;

                // Check that the Document tag is correct.
                if (documentNode.Name != "Document")
                {
                    errMsg = String.Format("ERROR: the <kml> root element's child node is not <Document>' : {0}", filename);
                    return false;
                }

                if (!documentNode.HasChildNodes)
                {
                    errMsg = String.Format("ERROR: the kml/Document element has no child nodes : {0}", filename);
                    return false;
                }

                XmlNodeList folderNodeList = documentNode.GetElementsByTagName("Folder");

                // Check that kml/document contains exactly one Folder node.
                if (folderNodeList.Count == 0)
                {
                    errMsg = String.Format("ERROR: the kml/Document element has no <Folder> child node : {0}", filename);
                    return false;
                }
                if (folderNodeList.Count > 1)
                {
                    errMsg = String.Format("ERROR: the kml/Document element has multiple <Folder> child nodes : {0}", filename);
                    return false;
                }

                XmlElement folderNode = (XmlElement)folderNodeList[0];

                if (!folderNode.HasChildNodes)
                {
                    errMsg = String.Format("ERROR: the kml/Document/Folder element has no child nodes : {0}", filename);
                    return false;
                }

                XmlNodeList placemarkNodes = folderNode.GetElementsByTagName("Placemark");

                // Check that the kml/Document/Folder node contains one or more Placemark nodes.
                if (placemarkNodes.Count == 0)
                {
                    errMsg = String.Format("ERROR: the kml/Document/Folder element has no <Placemark> child nodes : {0}", filename);
                    return false;
                }

                // Parse in all the Placemark nodes.
                foreach (XmlNode placemarkNode in placemarkNodes)
                {
                    // Check that this kml/Document/Folder/Placemark has child nodes.
                    if (!placemarkNode.HasChildNodes)
                    {
                        errMsg = String.Format("ERROR: the kml/Document/Folder element has a <Placemark> with no child nodes : {0}", filename);
                        return false;
                    }

                    XmlNodeList nameNodes = ((XmlElement)placemarkNode).GetElementsByTagName("name");

                    // Check that this Placemark has exactly one child named 'name'.
                    if (nameNodes.Count == 0)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark element has no <name> child : {0}", filename);
                        return false;
                    }
                    else if (nameNodes.Count > 1)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark element has multiple <name> children : {0}", filename);
                        return false;
                    }

                    // Create and attempt to populate a new GeoRegion object.
                    ISEDplacemark geoRegion = new ISEDplacemark();

                    XmlElement nameNode = (XmlElement)nameNodes[0];

                    // Check that the Placemark's 'name' element has valid text.
                    if (String.IsNullOrWhiteSpace(nameNode.InnerText))
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark element has a blank name element: {0}", filename);
                        return false;
                    }

                    geoRegion.Name = nameNodes[0].InnerText.Trim();

                    // Check that the kml/Document/Folder/Placemark element has exactly one 'ExtendedData' element.
                    XmlNodeList ExtendedDataNodes = ((XmlElement)placemarkNode).GetElementsByTagName("ExtendedData");

                    // Check that this Placemark has exactly one child named 'name'.
                    if (ExtendedDataNodes.Count == 0)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark element has no <ExtendedData> child : {0}", filename);
                        return false;
                    }
                    else if (ExtendedDataNodes.Count > 1)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark element has multiple <ExtendedData> children : {0}", filename);
                        return false;
                    }

                    XmlElement extendedDataNode = (XmlElement)ExtendedDataNodes[0];

                    // OK, we have just one <ExtendedData> element.
                    // Check that it has exactly one <SchemaData> child element.
                    XmlNodeList SchemaDataNodes = extendedDataNode.GetElementsByTagName("SchemaData");

                    if (SchemaDataNodes.Count == 0)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData element has no <SchemaData> child : {0}", filename);
                        return false;
                    }
                    else if (SchemaDataNodes.Count > 1)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData element has multiple <SchemaData> children : {0}", filename);
                        return false;
                    }

                    // We have exactly one 'SchemaData' node.
                    XmlElement schemaDataNode = (XmlElement)SchemaDataNodes[0];

                    // Get the SchemaData node's children.
                    if (!schemaDataNode.HasChildNodes)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData element has no child nodes : {0}", filename);
                        return false;
                    }

                    // Check that this SchemaData node has exactly two SimpleData children.
                    XmlNodeList SimpleDataNodes = schemaDataNode.GetElementsByTagName("SimpleData");

                    if (SimpleDataNodes.Count == 0)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData element has no child nodes: {0}", filename);
                        return false;
                    }
                    else if (SimpleDataNodes.Count == 1)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData element has only one 'SimpleData' child node : {0}", filename);
                        return false;
                    }
                    else if (SimpleDataNodes.Count > 2)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData element has more than two 'SimpleData' child nodes : {0}", filename);
                        return false;
                    }

                    // So we have two SimpleData elements. Harvest their inner text.
                    bool hasDescription = false;
                    bool hasTier = false;

                    foreach (XmlNode node in SimpleDataNodes)
                    {
                        XmlElement simpleDataNode = (XmlElement)node;

                        if (simpleDataNode.HasAttribute("name"))
                        {
                            string attrValue = simpleDataNode.GetAttribute("name");

                            switch (attrValue)
                            {
                                case "Description":
                                case "Category":
                                    // The inner text could contain both English and French words.
                                    // Remove the French.
                                    str = simpleDataNode.InnerText.Trim();
                                    int index = str.IndexOf('/');
                                    if (index != -1) str = str.Substring(0, index).Trim();
                                    geoRegion.Description = str;
                                    hasDescription = true;
                                    break;
                                case "Tier / niveau":
                                case "ProperTier5number":
                                    geoRegion.Tier = simpleDataNode.InnerText.Trim();
                                    hasTier = true;
                                    break;
                                default:
                                    errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData/SimpleData element has an unexpected name attribute value: {0}\n in file: {1}",
                                                            attrValue, filename);
                                    return false;
                                    break;
                            }
                        }

                    } // foreach (XmlNode node in SimpleDataNodes)

                    // Check that both Description and Tier name attribute were found.
                    if (!hasDescription)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData/SimpleData element was missing the 'Description' name attribute : {0}", filename);
                        return false;
                    }
                    if (!hasTier)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/ExtendedData/SchemaData/SimpleData element was missing the 'Tier' name attribute : {0}", filename);
                        return false;
                    }

                    // Now parse in the actual (lat, lng) points defining the region's boundary.

                    // In KML, the 'Polygon' element prescribes a single closed boundary comprising 
                    // piecewise-linear boundary segements.

                    // The ISED KML datafiles contain one instance where a Placemark element
                    // contains two 'Polygon' elements (embedded inside a 'MultiGeometry' element
                    // that is a child of Placemark).

                    // The Placemark element could have multiple 'Polygon' elements as a child nodes.
                    // To get here, the Placemark has already been verified to have child nodes of some kind.
                    // Just grab all subordinate nodes of Placemark that are 'Polygon' nodes.
                    XmlNodeList PolygonNodes = ((XmlElement)placemarkNode).GetElementsByTagName("Polygon");

                    // Check that there is at least one 'Polygon' node.
                    if (PolygonNodes.Count == 0)
                    {
                        errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark element has no <Polygon> sub-nodes : {0}", filename);
                        return false;
                    }

                    // The coordinates node inner text represents one or more coordinate tuples, 
                    // with each tuple consisting of decimal values for geodetic longitude, 
                    // geodetic latitude, and altitude.
                    // e.g. -75.73333301373849,46.43333299041548,0 -75.74999998124847,46.43333299041548,0 etc
                    // The altitude component is optional.
                    // The coordinate separator is a comma and the tuple separator is a whitespace.
                    // Longitude and latitude values are expressed in decimal degrees only.

                    foreach (XmlNode node in PolygonNodes)
                    {
                        // A Polygon node provides the (lat, lng) point for a GeoBoundary object.
                        GeoBoundary geoBoundary = new GeoBoundary();

                        XmlElement polygonNode = (XmlElement)node;

                        // The actual (lng, lat alt) 3 - tuples are concatonated as a long string as the inner text of
                        // a deeply embedded <coordinates> element. We will not verify all the intermediate
                        // element layes and just pluck out the <coordinates> element.
                        XmlNodeList coordinatesNodes = ((XmlElement)polygonNode).GetElementsByTagName("coordinates");

                        // Check that this Polygon node has exactly one child named 'coordinates'.
                        if (coordinatesNodes.Count == 0)
                        {
                            errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/Polygon element has no <coordinate> child : {0}", filename);
                            return false;
                        }
                        else if (coordinatesNodes.Count > 1)
                        {
                            errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/Polygon element has multiple <coordinate> children : {0}", filename);
                            return false;
                        }

                        XmlElement coordinatesNode = (XmlElement)coordinatesNodes[0];

                        // We have now found the single deeply-embedded 'coordinate' element with the (lat,lng alt) sequence.
                        // Get and then parse the inner text.

                        // We need to Trim() the inner text because there could be sneaky whitespace after 
                        // the final altitude and before the <LF>.
                        string innerText = coordinatesNode.InnerText.Trim();

                        char[] delimeters = new char[2] { ',', ' ' };

                        string[] fields = innerText.Split(delimeters);

                        // Check that the number of numeric fields is a multiple of 3..
                        if ((fields.Length / 3) * 3 != fields.Length)
                        {
                            errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/Polygon ... /coordinates element has an inner text that cannot be parsed as 3-tuples : {0}\nname = {1}\nCSVfields = {2}",
                                                    filename, geoRegion.Name, fields.Length);
                            return false;
                        }

                        // Pair the numeric fields up into (lat, lng) points and accumulate in the list.
                        for (int i = 0; i < fields.Length / 3; i++)
                        {
                            int startIndex = 3 * i;

                            // Convert text field to double.
                            try
                            {
                                GeoPoint geoPoint = new GeoPoint();

                                // Remember that the tuple order is (lng, lat alt).
                                geoPoint.Lng = Convert.ToDouble(fields[startIndex]);
                                geoPoint.Lat = Convert.ToDouble(fields[startIndex + 1]);

                                geoBoundary.Points.Add(geoPoint);
                            }
                            catch (Exception e)
                            {
                                errMsg = String.Format("ERROR: a kml/Document/Folder/Placemark/Polygon ... /coordinates element: conversion to double failed : {0}\nlat = {1}\nlng = {2}\n{3}",
                                                        filename, fields[startIndex], fields[startIndex + 1], e.Message);
                                return false;
                            }

                        }

                        // Add the boundary to the list.
                        geoRegion.Boundaries.Add(geoBoundary);

                    } // foreach PolygonNode


                    // Add the new GeoRegion object to the list.
                    geoRegions.Add(geoRegion);

                } // foreach placemarkNode


            }
            catch (Exception e)
            {
                Console.Error.Write("\nERROR: Exception: {0}\n{1}", e.Message, e.StackTrace);
                isValid = false;
            }

            return isValid;
        }



        /// <summary>
        /// This method returns true if this ISEDplacemark object encloses a prescribed 
        /// GeoPoint object; otherwise false.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool EnclosesPoint(GeoPoint p)
        {
            bool isInside = false;

            foreach (GeoBoundary boundary in mBoundaries)
            {
                isInside |= boundary.Encloses(p);
            }

            return isInside;
        }

        /// <summary>
        /// This method returns the ISEDplacemark object whose name matches a prescribed string
        /// from a prescribed list of ISEDplacemark objects.
        /// </summary>
        /// <param name="geoAreas"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ISEDplacemark GetAreaByName(List<ISEDplacemark> geoAreas, string name)
        {
            ISEDplacemark geoArea = null;

            if (geoAreas != null)
            {
                foreach (ISEDplacemark region in geoAreas)
                {
                    if (region.Name.Equals(name))
                    {
                        geoArea = region;
                        break;
                    }
                }
            }

            return geoArea;
        }


    }
}

