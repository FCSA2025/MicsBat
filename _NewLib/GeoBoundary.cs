using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates a list of GeoPoint objects that define a
    /// closed boundary; note that the initial GeoPoint and the final
    /// GeoPoint must have exactly the same (lat, lng) coordinates.
    /// </summary>
    [Serializable]
    public class GeoBoundary
    {
        private List<GeoPoint> mBoundaryPoints = new List<GeoPoint>();

        public List<GeoPoint> Points { get { return mBoundaryPoints; } set { mBoundaryPoints = value; } }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string
        /// that lists the values of all the object's GeoPoint objects.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (GeoPoint geoPoint in mBoundaryPoints)
            {
                i++;
                sb.Append(String.Format("\n     Point : {0,5};  {1}", i, geoPoint.ToString()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method returns true if the prescribed GeoPoint object is enclosed by this
        /// GeoBoundary object; the algorithm used is the computationally efficient 
        /// 'Winding Number' method as defined by http://geomalgorithms.com/a03-_inclusion.html
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Encloses(GeoPoint p)
        {
            int wn = 0;    // the  winding number counter

            int n = mBoundaryPoints.Count - 1;

            // loop through all edges of the polygon
            for (int i = 0; i < n; i++)
            {
                // edge from mBoundaryPoints[i] to  mBoundaryPoints[i+1]
                if (mBoundaryPoints[i].Lat <= p.Lat)
                {   
                    // start y <= P.Lat
                    if (mBoundaryPoints[i + 1].Lat > p.Lat)      // an upward crossing
                    {
                        if (p.IsLeftOf(mBoundaryPoints[i], mBoundaryPoints[i + 1]))  // P left of  edge
                        {
                            ++wn;            // have  a valid up intersect
                        }
                    }
                }
                else
                {   
                    // start y > P.Lat (no test needed)
                    if (mBoundaryPoints[i + 1].Lat <= p.Lat)     // a downward crossing
                    {
                        if (p.IsRightOf(mBoundaryPoints[i], mBoundaryPoints[i + 1]))  // P right of  edge
                        {
                            --wn;            // have  a valid down intersect
                        }
                    }
                }
            }

            return (wn == 0 ? false : true);
        }

    }
}
