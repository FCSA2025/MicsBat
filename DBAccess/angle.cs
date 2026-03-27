using System;

namespace DBAccess{
	public class angles{
		//--------------------------------------------------------------
		// 	Return the angle in radians given an angle in degrees.
		//--------------------------------------------------------------
		public static double torad(double angleindeg){
			return (angleindeg * 0.0174532925199);
		}


		//--------------------------------------------------------------
		// 	Return the angle in degrees given an angle in radians.
		//--------------------------------------------------------------
		public static double todeg(double angleinrad){
			return (angleinrad * 57.2957795132);
		}


		//--------------------------------------------------------------
		// 	Make an angle in degrees out of its component degrees,
		//	minutes and seconds.
		//--------------------------------------------------------------
		public static double makedeg(int degrees, int minutes, int seconds){
			if (degrees < 0){
				return(degrees - (minutes / 60) - (seconds / 3600));
			} else {
				return(degrees + (minutes / 60) + (seconds / 3600));
			}
		}


		//-----------------------------------------------------------------
		//  Return the floating point degrees from degrees minutes and secs
		//-----------------------------------------------------------------
		public static double degval(double fdeg, double fmin, double fsec)
		{
			int     nInd;
			double	deg;

			nInd = fdeg >= 0 ? 1 : -1;
			deg  = Math.Abs(fdeg) +
						fmin / 60.0 +
						fsec / 3600.0;
			return deg * nInd;
		}
	
		//------------------------------------------------------------------------
		//	Latitude and Longitude creators.
		//------------------------------------------------------------------------
		public static degrees tMakeLat(double dDegrees)
		{
			degrees oLat = new degrees(dDegrees);
			
			if (oLat.nSign < 0){
				oLat.Sense = "S";
			} else {
				oLat.Sense = "N";
			}
			
			return oLat;
		}


		//------------------------------------------
		public static degrees tMakeLong(double dDegrees)
		{
			degrees oLong = new degrees(dDegrees);
			
			if (oLong.nSign < 0){
				oLong.Sense = "E";		//	Note that FCSA is the reverse of normal
			} else {
				oLong.Sense = "W";
			}
			
			return oLong;
		}


		//--------------------------------------------------------------------------
		//	Formating the lat and long.
		//--------------------------------------------------------------------------
		public static string numtostring(int dNum, int nLength)
		{
			string outstr = dNum.ToString();
			
			if (outstr.Length < nLength){
				outstr = "0000000000".Substring(0, nLength - outstr.Length) + outstr;
			}
			
			return outstr;
		}


		//-------------------------------------------------------------------------
		//  Display a Latitude that comes in in integer seconds * 100
		//-------------------------------------------------------------------------
		public static string dispLat(int nDegrees)
		{
			degrees oDeg = tMakeLat(nDegrees / 360000.0);
			string sOutStr;
			
			sOutStr = numtostring(oDeg.Degrees, 2) + "-" +
								numtostring(oDeg.Minutes, 2) + "-" +
								numtostring(oDeg.Seconds, 2) + "." +
								numtostring(oDeg.Hundredths, 2) + " " +
								oDeg.Sense;
			return sOutStr;
		}


		//-------------------------------------------------------------------------
		//  Display a Longitude that comes in in integer seconds * 100
		//-------------------------------------------------------------------------
		public static string dispLong(int nDegrees)
		{
			degrees oDeg = tMakeLong(nDegrees / 360000.0);
			string sOutStr;
			
			sOutStr = numtostring(oDeg.Degrees, 3) + "-" +
								numtostring(oDeg.Minutes, 2) + "-" +
								numtostring(oDeg.Seconds, 2) + "." +
								numtostring(oDeg.Hundredths, 2) + " " +
								oDeg.Sense;
			return sOutStr;
		}


		//------------------------------------------------------------------------
		//	Convert float Degrees to integer seconds * 100
		//------------------------------------------------------------------------
		public static int toSec100(double dDeg)
		{
			return Convert.ToInt32(Math.Floor(dDeg * 360000 + 0.5));
		}


		//------------------------------------------------------------------------
		//	Convert degrees minutes and seconds to integer seconds * 100
		//------------------------------------------------------------------------
		public static int makeSec100(int dDeg, int dMin, int dSec)
		{
			return toSec100(makedeg(dDeg, dMin, dSec));
		}


		//------------------------------------------------------------------------
		//	Convert seconds * 100 to floating point degrees.
		//------------------------------------------------------------------------
		public static double Sec100toDeg(int nDegSec100)
		{
			return (nDegSec100 / 360000.0);
		}


		//------------------------------------------------------------------------
		//	Converts seconds * 100 to a Degree structure
		//------------------------------------------------------------------------
		public static degrees makeDegfromSec100(int nDegSec100)
		{
			return new degrees(Sec100toDeg(nDegSec100));
		}


		//------------------------------------------------------------------------
		//	Cosine in degrees
		//------------------------------------------------------------------------
		const double kConv = Math.PI / 180.0;
		public static double cosd(double nDegrees)
		{
			return Math.Cos(nDegrees * kConv);
		}


		//------------------------------------------------------------------------
		//	Parse an input string in the form: dd[d]-mm-ss.hhS into float degrees
		//	with either S or E being negative.
		//------------------------------------------------------------------------
		public static double toFloatDeg(string cAngle)
		{
			string[]	aComp;
			int				nLen;
			int				dDeg;
			int				nMin;
			double		dSec;
			string		cSense;
			int				nSign;
			
			//	Get the sign first
			cAngle = cAngle.Trim();
			cSense = cAngle[cAngle.Length - 1].ToString();		//	Last char
			cSense = cSense.ToUpper();
			if (cSense == "N" || cSense == "W"){
				nSign = +1;
				cAngle = cAngle.Remove(cAngle.Length - 1, 1);		//	Omit the last character
			} else if (cSense == "S" || cSense == "E"){
				nSign = -1;    
				cAngle = cAngle.Remove(cAngle.Length - 1, 1);
			} else {
				nSign = +1;
			}
			
			aComp = cAngle.Split('-');
			nLen = aComp.Length;
			
			//	There should be 3 components.  Note that some components can start with
			//	a zero (i.e. xxx-08-09).  We specify the radix so this is not taken as octal.
			if (nLen >= 1){
				dDeg = Convert.ToInt32(aComp[0], 10);
			} else {
				return(0.0);
			}
			
			if (nLen >= 2){
				nMin = Convert.ToInt32(aComp[1], 10);
			} else {
				return(nSign * dDeg);
			}
			
			if (nLen >= 3){
				//	Now break out the seconds and hundredths
				dSec = Convert.ToDouble(aComp[2]);
			} else {
				return(nSign * (dDeg + (nMin / 60.0)));
			}
			
			return (nSign * (dDeg + (nMin / 60.0) + (dSec / 3600.0)));
		}
	}
	
	
	public class degrees{
		public int	Degrees;
		public int  Minutes;
		public int  Seconds;
		public int	Hundredths;
		public string Sense;
		public int	nSign;
		
		public double fullvalue;
		
		//------------------------------------------------------------------------
		//	Break floating point degrees into degrees, minutes, seconds and sense.
		//	This is a constructor and must be called with the 'new' keyword.
		//------------------------------------------------------------------------
		public degrees(double dDegrees)
		{
			nSign = (dDegrees < 0) ? -1 : +1;
			
			fullvalue = dDegrees;
			dDegrees *= nSign;		//	Take the absolute value.
			//	Add half a hundredth of a second for rounding.
			dDegrees += 0.00000138889;
			Degrees = Convert.ToInt32(Math.Floor(dDegrees));
			dDegrees = (dDegrees - Degrees) * 60.0;
			Minutes = Convert.ToInt32(Math.Floor(dDegrees));
			dDegrees = (dDegrees - Minutes) * 6000.0;
			Seconds = Convert.ToInt32(Math.Floor(dDegrees / 100));
			Hundredths = Convert.ToInt32(Math.Floor(((dDegrees / 100) - Seconds) * 100));
		}
	}


}