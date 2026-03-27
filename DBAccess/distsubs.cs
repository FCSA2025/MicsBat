using System;

namespace DBAccess {

	public class distobj{
		public double DistanceKm;
		public double Bearing12;
		public double Bearing21;
		
		//--------------------------------------------------------------
		//	Object Constructor for the distance object
		//--------------------------------------------------------------
		public distobj(double dDist, double dBearing12, double dBearing21){
			DistanceKm = dDist;
			Bearing12 = dBearing12;
			Bearing21 = dBearing21;
		}


		public static double torad(double degrees)
		{
			return degrees * Math.PI / 180.0;
		}
		
		
		public static double todeg(double radians)
		{
			return radians * 180.0 / Math.PI;
		}
		
		
		//--------------------------------------------------------------
		//	Return the distance in Km and bearing in degrees between
		//	two lats and longs in degrees
		//--------------------------------------------------------------
		public distobj(	double lat1deg, double long1deg, 
										double lat2deg, double long2deg){
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
			double		bearing12;
			double		bearing21;

			rlat1 = torad(lat1deg);
			rlong1 = torad(long1deg);
			rlat2 = torad(lat2deg);
			rlong2 = torad(long2deg);

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
			try {
				DistanceKm = (distancekm < 0.0) ? -distancekm : distancekm;
				Bearing12 = distobj.todeg(bearing12);
				Bearing21 = distobj.todeg(bearing21);
			} catch {
				DistanceKm = -1;
				Bearing12 = -1;
				Bearing21 = -1;
			}
		}
		
		
		public override string ToString()
		{
			return DistanceKm.ToString("#0.00") + "km, A->B: " +
			       Bearing12.ToString("#0.0") + "deg, B-A: " +
			       Bearing21.ToString("#0.0") + "deg";
		}


		//------------------------------------------------------------------
		//  Elevation angles between two antennas.  Heights and distances
		//  all in km.  Returns a two element double array with the first
		//	element the 1->2 elevation and the second the 2->1 elevation in
		//	degrees.	
		//------------------------------------------------------------------
		public static double[] elev(double anthtkm1, double anthtkm2, double distkm)
		{
			double     temp1;
			double     temp2;

			//  temp1 = D/2RK, K = 4/3
			temp1 = distkm / 16999.50667;
			temp2 = Math.Atan((anthtkm2 - anthtkm1) /
												((16999.50667 + anthtkm1 + anthtkm2) * Math.Tan(temp1)));
			return (new double[] {todeg(temp2 - temp1), todeg(-temp2 - temp1)});
		}
	}
}