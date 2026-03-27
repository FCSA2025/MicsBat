using System;

namespace DBAccess
{
	/// <summary>
	/// Summary description for GeoSearch.
	/// </summary>
	public class GeoSearch
	{
		protected int			m_LatcSecs;				//	Latitude of the centre in centiseconds North
		protected int			m_LongcSecs;			//	Longitude of the centre in centiseconds West
		protected double	m_RadiusKm;				//	Radius of the circle around the centre in Km.
		protected int			m_BoxNorthcSecs;	//	North latitude of the box around the site.
		protected int			m_BoxSouthcSecs;	//	South
		protected int			m_BoxEastcSecs;		//	East Longitude
		protected int			m_BoxWestcSecs;		//	West Longitude
		
		public int			LatcSecs{
			get{return m_LatcSecs;}
			// set{m_LatcSecs = value;}
		}

		public int			LongcSecs{
			get{return m_LongcSecs;}
			// set{m_LongcSecs = value;}
		}

		public double			RadiusKm{
			get{return m_RadiusKm;}
			// set{m_RadiusKm = value;}
		}
		public int			BoxNorthcSecs{
			get{return m_BoxNorthcSecs;}
			// set{m_BoxNorthcSecs = value;}
		}
		public int			BoxSouthcSecs{
			get{return m_BoxSouthcSecs;}
			// set{m_BoxSouthcSecs = value;}
		}
		public int			BoxEastcSecs{
			get{return m_BoxEastcSecs;}
			// set{m_BoxEastcSecs = value;}
		}
		public int			BoxWestcSecs{
			get{return m_BoxWestcSecs;}
			// set{m_BoxWestcSecs = value;}
		}
		
		/// <summary>
		/// Construct a Geosearch object and calculate the box around it.
		/// </summary>
		/// <param name="LatcSecs">The input latitude in hundredths of a second</param>
		/// <param name="LongcSecs">The input longitude in hundredths of a second</param>
		/// <param name="RadiusKm">The Radius of the search in double km.</param>
		public GeoSearch(int LatcSecs, int LongcSecs, double RadiusKm)
		{
			m_LatcSecs = LatcSecs;
			m_LongcSecs = LongcSecs;
			m_RadiusKm = RadiusKm;
			
			//	Now calculate the box coordinates.
			const double EARTHRADIUSKM = 6374.815;
			double dDiffLat = (RadiusKm / EARTHRADIUSKM) * 57.2957795131 * 360000.0;
			int diffLat = (int) (dDiffLat + 0.5);
			m_BoxNorthcSecs = m_LatcSecs + diffLat;
			m_BoxSouthcSecs = m_LatcSecs - diffLat;
			//	To get the difference in longitude, divide by the cos of the centre latitude
			int diffLong = (int)(dDiffLat / 
			                     Math.Cos(((double)m_LatcSecs / 360000.0) * (Math.PI / 180.0)) +
			                     0.5);
			m_BoxEastcSecs = m_LongcSecs - diffLong;
			m_BoxWestcSecs = m_LongcSecs + diffLong;

			return;
		}

		/// <summary>
		/// Method to return the distance between the centre of the geosearch and a
		/// given point.
		/// </summary>
		/// <param name="LatcSecs">The latitude of the point in centiSecs</param>
		/// <param name="LongcSecs">The longitude (W) of the point in centiSecs</param>
		/// <returns>Distance in Km. (double)</returns>
		public double distance(int LatcSecs, int LongcSecs)
		{
			double		rlat1;
			double		rlong1;
			double		rlat2;
			double		rlong2;
			double		latdiff;
			double		longdiff;
			double		latavr;
			double		c1;
			double		sinlatavr;
			double		am;
			double		bearingmid;
			double		bearingdiff;

			double		distancekm;
			double 		bearing12;
			double		bearing21;

			rlat1 = (LatcSecs / 360000.0) * (Math.PI / 180.0);	//	centiSecs to Radians
			rlong1 = (LongcSecs / 360000.0) * (Math.PI / 180.0);
			rlat2 = (m_LatcSecs / 360000.0) * (Math.PI / 180.0);
			rlong2 = (m_LongcSecs / 360000.0) * (Math.PI / 180.0);

			latdiff = rlat1 - rlat2;
			if (latdiff == 0.0){
				latdiff = 1e-8;
			}
			longdiff = rlong1 - rlong2;
			if (longdiff == 0.0){
				longdiff = 1e-8;
			}
			latavr = (rlat1 + rlat2) / 2;
			sinlatavr = Math.Sin(latavr);

			//	1 - eccentricity**2 * sin(avlat)**2
			c1 = 1 - (0.00669454 * sinlatavr * sinlatavr);
			am = Math.Sqrt(c1) / 30.9221917;
			bearingmid = Math.Atan(longdiff * Math.Cos(latavr) * c1 /
														(0.99330546 * latdiff));
			bearingmid = bearingmid < 0.0 ? -bearingmid : bearingmid;
			bearingdiff = longdiff * Math.Sin(latavr);

			bearing12 = bearingmid - bearingdiff / 2;
			if (latdiff >= 0.0){
				if (longdiff < 0.0){
					bearing12 += Math.PI;
				} else {
					bearing12 = Math.PI - bearing12 - bearingdiff;
				}
			} else if (longdiff <= 0.0){
				bearing12 = 2.0 * Math.PI - bearing12 - bearingdiff;
			}

			if (bearing12 > 2.0 * Math.PI){
				bearing12 -= 2.0 * Math.PI;
			}

			bearing21 = bearing12 + bearingdiff + Math.PI;
			if (bearing21 > 2.0 * Math.PI){
				bearing21 -= 2.0 * Math.PI;
			}

			distancekm = (longdiff * Math.Cos(latavr)) /
								(am * Math.Sin(bearingmid) * 0.0048481368);
			distancekm = (distancekm < 0.0) ? -distancekm : distancekm;
			
			return distancekm;
		}
		
		
		/// <summary>
		/// Method to return the distance between the centre of the geosearch and a
		/// given point.
		/// </summary>
		/// <param name="LatcSecs">The latitude of the point in centiSecs</param>
		/// <param name="LongcSecs">The longitude (W) of the point in centiSecs</param>
		/// <returns>Distance in Km. (double)</returns>
		public static double distance(int LatSecsA, int LongSecsA, int LatSecsB, int LongSecsB)
		{
			double		rlat1;
			double		rlong1;
			double		rlat2;
			double		rlong2;
			double		latdiff;
			double		longdiff;
			double		latavr;
			double		c1;
			double		sinlatavr;
			double		am;
			double		bearingmid;
			double		bearingdiff;

			double		distancekm;
			double 		bearing12;
			double		bearing21;

			rlat1 = (LatSecsA / 360000.0) * (Math.PI / 180.0);	//	centiSecs to Radians
			rlong1 = (LongSecsA / 360000.0) * (Math.PI / 180.0);
			rlat2 = (LatSecsB / 360000.0) * (Math.PI / 180.0);
			rlong2 = (LongSecsB / 360000.0) * (Math.PI / 180.0);

			latdiff = rlat1 - rlat2;
			if (latdiff == 0.0){
				latdiff = 1e-8;
			}
			longdiff = rlong1 - rlong2;
			if (longdiff == 0.0){
				longdiff = 1e-8;
			}
			latavr = (rlat1 + rlat2) / 2;
			sinlatavr = Math.Sin(latavr);

			//	1 - eccentricity**2 * sin(avlat)**2
			c1 = 1 - (0.00669454 * sinlatavr * sinlatavr);
			am = Math.Sqrt(c1) / 30.9221917;
			bearingmid = Math.Atan(longdiff * Math.Cos(latavr) * c1 /
														(0.99330546 * latdiff));
			bearingmid = bearingmid < 0.0 ? -bearingmid : bearingmid;
			bearingdiff = longdiff * Math.Sin(latavr);

			bearing12 = bearingmid - bearingdiff / 2;
			if (latdiff >= 0.0){
				if (longdiff < 0.0){
					bearing12 += Math.PI;
				} else {
					bearing12 = Math.PI - bearing12 - bearingdiff;
				}
			} else if (longdiff <= 0.0){
				bearing12 = 2.0 * Math.PI - bearing12 - bearingdiff;
			}

			if (bearing12 > 2.0 * Math.PI){
				bearing12 -= 2.0 * Math.PI;
			}

			bearing21 = bearing12 + bearingdiff + Math.PI;
			if (bearing21 > 2.0 * Math.PI){
				bearing21 -= 2.0 * Math.PI;
			}

			distancekm = (longdiff * Math.Cos(latavr)) /
								(am * Math.Sin(bearingmid) * 0.0048481368);
			distancekm = (distancekm < 0.0) ? -distancekm : distancekm;
			
			return distancekm;
		}
		
		
		/// <summary>
		/// This static method will convert a lat or long in centiSecs to a string representation
		/// </summary>
		/// <param name="LLcSec">Lat or long in centisecs (integer)</param>
		/// <returns>string with the representation as (d)dd-mm-ss.hh uses absolute value of input
		///	</returns>
		public static string ToDegStr(int LLcSec)
		{
			string	cOut = "";
			int			nIncSec = Math.Abs(LLcSec);
			bool		IsNegative = LLcSec < 0;
			int			nSec = nIncSec / 100;
			int			nHund = nIncSec - (nSec * 100);
			int			nMin = nSec / 60;
			int			nDeg = nSec / 3600;
			
			nSec -=	(nMin * 60);
			nMin -= (nDeg * 60);
			
			cOut = nDeg.ToString("#00") + "-" +
			       nMin.ToString("00") + "-" +
			       nSec.ToString("00") + "." +
			       nHund.ToString("00");
			return cOut;
		}
		
		
		/// <summary>
		/// Given a string in the form (d)dd-mm-ss.hh this will return its value in
		///	hundredths of a second
		/// </summary>
		/// <param name="cDegStr">string with ddd-mm-ss.hh representation</param>
		/// <returns>int with value in hundredths of a degree.</returns>
		public static int ParseDegStr(string cDegStr)
		{
			int nInd = 0;
			int nDeg = 0;
			int nMin = 0;
			int	nSec = 0;
			int nHun = 0;
			int nDenom = 1;
			
			try{
				//	Get the degrees
				cDegStr = cDegStr.Trim();
				while (Char.IsDigit(cDegStr, nInd)){
					nDeg *= 10;
					nDeg += (int) Char.GetNumericValue(cDegStr, nInd);
					nInd++;
				}
				//	Assume we have -, and skip over it.
				nInd++;
				while (Char.IsDigit(cDegStr, nInd)){
					nMin *= 10;
					nMin += (int) Char.GetNumericValue(cDegStr, nInd);
					nInd++;
				}
				//	Assume another - 
				nInd++;
				while(Char.IsDigit(cDegStr, nInd)){
					nSec *= 10;
					nSec += (int) Char.GetNumericValue(cDegStr, nInd);
					nInd++;
				}
				//	Assume a .
				nInd++;
				while(Char.IsDigit(cDegStr, nInd)){
					nHun *= 10;
					nHun += (int) Char.GetNumericValue(cDegStr, nInd);
					nDenom *= 10;
					nInd++;
				}
			} catch {
				//	We expect to come here if we have run out of string
			}
			
			//	Now test the values:
			nHun = (nHun * 100) / nDenom;		//	Normalize the fraction
			if (nSec > 59){
				throw new Exception("Invalid Input String: seconds");
			}
			if (nMin > 59){
				throw new Exception("Invalid Input String: minutes");
			}
			
			return ((((nDeg * 60) + nMin) * 60) + nSec ) * 100 + nHun;
		}


		/// <summary>
		/// Convert an angle in hundredths of a second to degrees
		/// </summary>
		/// <param name="nLLcSec">Angle (Lat/Long) in hundredths of second</param>
		/// <returns>Angle in degrees (double)</returns>
		public static double ToDeg(int nLLcSec)
		{
			return nLLcSec / 360000.0;
		}
		
		
		/// <summary>
		/// Convert an angle in degrees to hundredths of a second
		/// </summary>
		/// <param name="dDeg">angle in double degrees</param>
		/// <returns>integer number of centiseconds</returns>
		public static int TocSecs(double dDeg)
		{
			return (int) (dDeg * 360000.0);
		}
		
	}
	
	
	public class GeoTriangle
	{
		//	All are angles and all are in degrees.
		double m_SideA;	//	The side from the North pole to the input point.
		double m_SideB;	//	The side opposite the input point.
		double m_SideC;	//	The side opposite the North Pole.
		double m_AngleB; //	The angle opposite side b.  This is the azimuth of side C.
		double m_AngleC; //	This is the delta longitude
		
		double m_Long;		//	This is the input longitude for output purposes.
		
		public GeoTriangle(double dLat, double dLong)
		{
			m_SideA = 90.0 - dLat;
			m_Long = dLong;
		}
		
		public double SideKm
		{
			get{return (m_SideC * 111.12);}
			set{m_SideC = value / 111.12;}
		}
		
		public double Az
		{
			get{return m_AngleB;}
			set{m_AngleB = value;}
		}
		
		//	opplat and opplong must be called in order.
		public double opplat
		{
			get{m_SideB = acos(cos(m_SideC)*cos(m_SideA) + 
			              sin(m_SideC) * sin(m_SideA) * cos(m_AngleB));
			    return(90.0 - m_SideB);}
		}
		
		
		public double opplong
		{
			get{
					if (m_SideB == 0.0){
						return 0.0;
					} else {
						m_AngleC = asin(sin(m_SideC) * sin(m_AngleB) / sin(m_SideB)); 
						return(m_Long - m_AngleC);
					}
			 }
		}
		
		
		//	The following are  the trig functions in degrees.
		public double cos(double ang)
		{
			return(Math.Cos(ang * Math.PI / 180.0));
		}


		public double sin(double ang)
		{
			return(Math.Sin(ang * Math.PI / 180.0));
		}


		public double acos(double val)
		{
			return(Math.Acos(val) * 180.0 / Math.PI);
		}


		public double asin(double val)
		{
			return(Math.Asin(val) * 180.0 / Math.PI);
		}
	}
		
}
