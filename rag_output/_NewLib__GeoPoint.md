# Documented File: GeoPoint.cs
**Repository Path:** `_NewLib\GeoPoint.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates the private data and methods that model the
    /// notion of a point in 2-dimensional space (x, y); the prefix 'geo' relates to
    /// both geometric points (x, y) and geographic coordinates (lat, long).
    /// </summary>
    [Serializable]
    public class GeoPoint
    {
        private double mLat = 0;
        private double mLng = 0;

        public double Lat { get { return mLat; } set { mLat = value; } }
        //public double x { get { return mLat; } set { mLat = value; } }
        public double Lng { get { return mLng; } set { mLng = value; } }
        //public double y { get { return mLng; } set { mLng = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GeoPoint()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public GeoPoint(double lat, double lng)
        {
            this.mLat = lat;
            this.mLng = lng;
        }

        /// <summary>
        /// This method returns an annotated, formatted string that
        /// provides the current values of this object's internal field values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1})", mLat, mLng);
        }

        /// <summary>
        /// This method return true if this object is positioned to the left
        /// of the line drawn between two prescribed GeoPoint objects; otherwise false.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool IsLeftOf(GeoPoint p1, GeoPoint p2)
        {
            double testVal = (p2.Lng - p1.Lng) * (mLat - p1.Lat) - (mLng - p1.Lng) * (p2.Lat - p1.Lat);

            return (testVal > 0 ? true : false);
        }

        /// <summary>
        /// This method return true if this object is positioned to the right
        /// of the line drawn between two prescribed GeoPoint objects; otherwise false.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool IsRightOf(GeoPoint p1, GeoPoint p2)
        {
            return !IsLeftOf(p1, p2);
        }

        /// <summary>
        /// This method returns true if this GeoPoint object is enclosed within a 
        /// prescribed GeoBoundary object.
        /// </summary>
        /// <param name="geoBoundary"></param>
        /// <returns></returns>
        public bool IsInside(GeoBoundary geoBoundary)
        {
            return geoBoundary.Encloses(this);
        }



    }
}

```
