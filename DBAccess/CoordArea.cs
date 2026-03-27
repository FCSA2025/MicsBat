using System;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using System.Web;

namespace DBAccess
{
	public struct AreaPoint
	{
		public int			AreaID;
		public double		Value1;
		public double		Value2;
		public double		Value3;
		public double		Value4;
		public DateTime mdate;


		//	Copy constructor
		public AreaPoint(AreaPoint oInAP)
		{
			AreaID = oInAP.AreaID;
			Value1 = oInAP.Value1;
			Value2 = oInAP.Value2;
			Value3 = oInAP.Value3;
			Value4 = oInAP.Value4;
			mdate = oInAP.mdate;			
		}		
	}
	

	/// <summary>
	/// This is the class containing the debugging event information.  Largely the returned 
	///	string.
	/// </summary>
	public class dbgInfo: EventArgs
	{
		public int		Mark;
		public string DebugString;
		public dbgInfo(int nMark, string cStr)
		{
			Mark = nMark;
			DebugString = cStr;
		}
	}


	public class dbgEvent
	{	
		//	Event raised by debugging. ---------------------------------
		public delegate void dbgOutputDef(object oSource, dbgInfo oDbg);
		public dbgOutputDef dbgOutput; // The delegate
		//	Raise the event
		public void dbgRaise(int nMark, string cStr)
		{
			if (dbgOutput != null){
				dbgOutput(this, new dbgInfo(nMark, cStr));
			}
		}
		//	------------------------------------------------------------
	}

	abstract public class LatLong
	{
		private int			LLVal = 0;
		
		//	These are put here for debugging purposes.
		int nPartD = 0;
		int	nPartM = 0;
		int nPartS = 0;
		int nPartH = 0;

		public string dbgout()
		{
			return LLVal.ToString() + " D:" + nPartD.ToString() + " M:" + nPartM.ToString() +
			       " S:" + nPartS.ToString() + " H:" + nPartH.ToString();
		}
		
		public int degrees{
			get {try{ return ((int) Math.Abs(LLVal) / 360000);} catch(Exception Ex){
					   throw new Exception("Invalid input: " + LLVal.ToString() + ":" + Ex.Message);}}
		}
		
		public int minutes{
			get {return ((int) (Math.Abs(LLVal) % 360000) / 6000);}
		}
		
		public int seconds{
			get {return((int) (Math.Abs(LLVal) % 6000) / 100);}
		}
		
		public int hundredths{
			get {return((int) (Math.Abs(LLVal) % 100));}
		}
		
		public string sense(string cChoice)
		{
			return (LLVal < 0 ? cChoice[0].ToString() : cChoice[1].ToString());
		}
		
		public void setsense(int nVal)
		{
			if (nVal < 0){
				LLVal = - Math.Abs(LLVal);
			} else {
				LLVal = Math.Abs(LLVal);
			}
		}
		
		
		public int setsense(string cChoice, string cChoices) {
			int nBound = cChoices.IndexOf(" ");
			int nFound = cChoices.IndexOf(cChoice);
			int nVal = 0;
			
			if (nFound >= 0){
				if (nFound > nBound){
					//	Make the value positive.
					nVal = +1;
					if (LLVal < 0){
						LLVal = -LLVal;
					}
				} else {
					//	Make it negative
					nVal = -1;
					if (LLVal >= 0){
						LLVal = -LLVal;
					}
				}	
			} else {
				throw new Exception("*Error* sense: [" + cChoice + "] not in [" + cChoices + "].");
			}
			
			return nVal;
		}
		
		
		public LatLong(string cInStr, int nLimit)
		{
			// Parse the values before the numeric
			int nInd;
			string cCh = " ";
			
			if (cInStr == null || cInStr.Trim().Length <= 0){
				LLVal = 0;
			}	else {			
				//	Get the degrees.
				for (nInd = 0; nInd < cInStr.Length; nInd++){
					if (!Char.IsDigit(cInStr, nInd)){
						break;
					}
					cCh = cInStr.Substring(nInd, 1);
					nPartD *= 10;
					nPartD += Convert.ToInt32(cCh);
				}
				
				//	Get the minutes
				if (nInd < cInStr.Length && !Char.IsLetter(cInStr, nInd)){
					for (nInd++; nInd < cInStr.Length; nInd++){
						if (!Char.IsDigit(cInStr, nInd)){
							break;
						}
						cCh = cInStr[nInd].ToString();
						nPartM *= 10;
						nPartM += Convert.ToInt32(cCh);
					}

					//	Get the seconds
					if (nInd < cInStr.Length && !Char.IsLetter(cInStr, nInd)){
						for (nInd++; nInd < cInStr.Length; nInd++){
							if (!Char.IsDigit(cInStr, nInd)){
								break;
							}
							cCh = cInStr[nInd].ToString();
							nPartS *= 10;
							nPartS += Convert.ToInt32(cCh);
						}

						//	Get hundredths of a second
						if (nInd < cInStr.Length && !Char.IsLetter(cInStr, nInd)){
							for (nInd++; nInd < cInStr.Length; nInd++){
								if (!Char.IsDigit(cInStr, nInd)){
									break;
								}
								cCh = cInStr[nInd].ToString();
								nPartH *= 10;
								nPartH += Convert.ToInt32(cCh);
							}
						}
					}
				}

				LLVal = nPartD * 360000 + nPartM * 6000 + nPartS * 100 + nPartH;
				
				if (LLVal > nLimit * 360000){
					throw new Exception("LatLong: Value too large: " + cInStr);
				}
				if (LLVal < -nLimit * 360000){
					throw new Exception("LatLong: Value too negative: " + cInStr);
				}
			}
		}
		
		
		public LatLong(int nVal, int nLimit)
		{
			LLVal = nVal;
			if (LLVal > nLimit * 360000){
				throw new Exception("LatLong: Value too large: " + nVal.ToString());
			}
		}
		
		
		public LatLong(double dVal, int nLimit) // input in degrees.
		{
			LLVal = (int) (dVal * 360000.0 + 0.5);
			if (LLVal > nLimit * 360000){
				throw new Exception("LatLong: Value too large: " + dVal.ToString());
			}
		}
		

		public LatLong(int nDeg, int nMin, int nSec, int nHun)
		{
			LLVal = Math.Abs(nDeg) * 360000 + nMin * 6000 + nSec * 100 + nHun;
		}
		

		protected string render()
		{
			string cOut = "";
			try {
			 cOut = this.degrees.ToString("##0") + "-" +
							this.minutes.ToString("00") + "-" +
							this.seconds.ToString("00") + "." +
							this.hundredths.ToString("00");
			} catch {
				cOut = "*Invalid: " + this.LLVal.ToString() + "*";
			}
			return cOut;
		}		
		
		public int ToInt()
		{
			return LLVal;
		}
		
		
		public double ToDouble()
		{
			return (double) LLVal / 360000.0;
		}
		
		
		public static double secstodeg(int nHsecs)
		{
			return nHsecs / 360000.0;
		}
		
	}
	


	//************** Latitude 
	public class latitude: LatLong
	{
		
		public latitude(int nVal):base(nVal, 90)
		{
		}


		public latitude(string cVal):base(cVal, 90)
		{
			string cValt = cVal.Trim();
			int	nLen = cValt.Length;
			if (nLen > 0){
				Char cSense = cValt[nLen - 1];
				if (Char.IsLetter(cSense) && "sS".IndexOf(cSense) >= 0){
					this.setsense(-1);
				}
			}
		}


		public latitude(double dVal):base(dVal, 90)
		{
		}


		public override string ToString()
		{
			return (base.render() + base.sense("SN")); 
		}
	}



	//	****************** Longitude
	public class longitude: LatLong
	{
		
		public longitude(int nVal):base(nVal, 180)
		{
		}


		public longitude(string cVal):base(cVal, 180)
		{
			string cValt = cVal.Trim();
			int	nLen = cValt.Length;
			if (nLen > 0){
				Char cSense = cValt[nLen - 1];
				if (Char.IsLetter(cSense) && "eE".IndexOf(cSense) >= 0){
					this.setsense(-1);
				}
			}
		}


		public longitude(double dVal):base(dVal, 180)
		{
		}


		public override string ToString()
		{
			return (base.render() + base.sense("EW")); //	This is our standard, not others.
		}
	}
	
	/// <summary>
	/// Summary description for CoordArea.
	/// </summary>
	public class CoordArea
	{
		public int			AreaID;
		public string		AreaLic = "";
		public string		AreaName = "";
		public string		AreaOper = "";
		public string		AreaProv = "";
		public string		AreaType = "";
		public string		AreaPerim = "";
		public string		AreaFile = "";
		public string		AreaNote = "";
		public latitude	AreaLatit;
		public longitude AreaLongit;
		public double		AreaGrnd = 0;
		public string		AreaAcode = "";
		public double		AreaAht = 0;
		public double		AreaAzmth = 0;
		public double		AreaDist = 0;
		public double		AreaFslTx = 0;
		public double		AreaFslRx = 0;
		public string		AreaEqptTx = "";
		public string		AreaEqptRx = "";
		public string		AreaSrvcTx = "";
		public double		AreaPwrTx = 0;
		public string		AreaTrafTx = "";
		public string		AreaTrafRx = "";
		public string		AreaBand1 = "";
		public string		AreaBand2 = "";
		public double		AreaFreq1Lo = 0.0;
		public double		AreaFreq1Hi = 0.0;
		public double		AreaFreq2Lo = 0.0;
		public double		AreaFreq2Hi	= 0.0;
		public double		ChannelBW = 0;
		public string		FirstChan = "";
		public string		AreaPolTx = "";
		public string		AreaPolRx = "";
		public int			RemoteNum = 0;
		public double		RemoteTxPwr = 0;
		public string		RemoteAcode = "";
		public string		RemoteEqptTx = "";
		public string		RemoteEqptRx = "";
		public string		RemoteSrvcTx = "";
		public double		RemoteFslTx = 0;
		public double		RemoteFslRx = 0;
		public double		RemoteAht = 0;
		public DateTime	mdate;
		
		public AreaPoint[] aPerimeter;
		
		public CoordArea()
		{
			AreaID = -1;		//	Show it doesn't exist.
			aPerimeter = null;
			AreaLatit = new latitude(0);
			AreaLongit = new longitude(0);
			AreaLic = "";
			mdate = DateTime.Now;
		}
		
		
		/// <summary>
		/// Calculate the AEIRP for an area.
		/// </summary>
		/// <param name="dGain">The gain used in the equation.</param>
		/// <param name="dPwr">The tx power using in the equation.</param>
		/// <returns>The AEIRP appropriate for the area.  
		///	</returns>
		public double AEIRP(double dGain, double dPwr)
		{
			
			if (RemoteNum <= 0){
				//	We need some remotes or this won't work
				throw new Exception("*ERROR* Number of Remote sites is zero.");
			}
			 
			return dPwr + 
			       1.061 * Math.Pow(Math.Log10((double) RemoteNum), 2.0) +
			       (-0.1164 * dGain + 6.103) * Math.Log10((double) RemoteNum) +
			       0.9428 * dGain - 
			       2.62;
		}
		
		
		//	Produce a central point in a polygon to run triangles from.  We just
		//	average the lats longs and alts.
		private void polycentre(AreaPoint[] aPerimeter, out AreaPoint oCentre)
		{
			double dLen = aPerimeter.Length;
			oCentre = new AreaPoint();
			
			for (int nInd = 0; nInd < aPerimeter.Length; nInd++){
				oCentre.Value2 += aPerimeter[nInd].Value2;
				oCentre.Value3 += aPerimeter[nInd].Value3;
				oCentre.Value4 += aPerimeter[nInd].Value4;
			}
			oCentre.Value2 /= dLen;
			oCentre.Value3 /= dLen;
			oCentre.Value4 /= dLen;
		}
		
		
		private	void addin(double[] aCentroid, double dArea, AreaPoint oThis)
		{
			aCentroid[0] += oThis.Value2 * dArea;
			aCentroid[1] += oThis.Value3 * dArea;
			aCentroid[2] += oThis.Value4 * dArea;
		}									
		
		
		//	This takes the triangle between the nInd point on the perimeter, the previous 
		//	point, and an arbitrary centre and calculates its area and its centroid.
		private void triangle(AreaPoint[]		aPerimeter, 
		                      AreaPoint			oCentre, 
		                      int						nInd,
		                      out double		dThisArea,
		                      out AreaPoint oThisCentre)
		{
			//	The centroid of a triangle is just the average of the points.
			int nPrev = (nInd == 0) ? aPerimeter.Length - 1 : nInd - 1;
			oThisCentre = new AreaPoint();
			oThisCentre.Value2 = (aPerimeter[nInd].Value2 + aPerimeter[nPrev].Value2 + 
														oCentre.Value2) / 3.0;
			oThisCentre.Value3 = (aPerimeter[nInd].Value3 + aPerimeter[nPrev].Value3 + 
														oCentre.Value3) / 3.0;
			oThisCentre.Value4 = (aPerimeter[nInd].Value4 + aPerimeter[nPrev].Value4 + 
														oCentre.Value4) / 3.0;
			
			//	Calculate the length of the sides...
			double dA = GeoSearch.distance((int) aPerimeter[nInd].Value2, 
			                               (int) aPerimeter[nInd].Value3,
			                               (int) aPerimeter[nPrev].Value2,
			                               (int) aPerimeter[nPrev].Value3);
			double dB = GeoSearch.distance((int) aPerimeter[nPrev].Value2, 
			                               (int) aPerimeter[nPrev].Value3,
			                               (int) oCentre.Value2,
			                               (int) oCentre.Value3);
			double dC = GeoSearch.distance((int) oCentre.Value2,
															       (int) oCentre.Value3,
			                               (int) aPerimeter[nInd].Value2, 
			                               (int) aPerimeter[nInd].Value3);

			double dS = (dA + dB + dC) / 2.0;
			dThisArea = Math.Sqrt(dS * (dS - dA) * (dS - dB) * (dS - dC));
			if (Double.IsNaN(dThisArea) || dThisArea == 0.0){
				dThisArea = 0.0;
			}
		}
		
		
		//	Calculate the centroid of the perimeter in aPerimeter
		public void centroid(out latitude outLat, out longitude outLong, out double outAlt)
		{
			if (aPerimeter == null || aPerimeter.Length <= 0){
				//	Error no perimeter
				outLat = new latitude(0);
				outLong = new longitude(0);
				outAlt = -1.0;
				return;
			}
			
			
			switch (AreaPerim){
				case "P":		//	Polygon
					//	The centroid of a polygon is the area weighted averages of all the centroids
					//	of the component triangles.
					AreaPoint oCentre;
					//AreaPoint oCentroid = new AreaPoint();
					AreaPoint oThisCentroid;
					double		dThisArea;
					double		dArea = 0;
					double[]	adCentroid = new double[3]{0.0, 0.0, 0.0}; //	The lat/long/alt array
					
					//	First we calculate the average lat/long of the polygon.  This becomes the 
					//	point we use to create the component triangles.
					polycentre(aPerimeter, out oCentre);
					
					//	Now we get the first triangle in the polygon.
					triangle(aPerimeter, oCentre, 1, out dThisArea, out oThisCentroid);
					dArea = dThisArea;
					addin(adCentroid, dThisArea, oThisCentroid);
					
					//	Go through the rest of the polygon, using these points to create and 
					//	average in the component triangles.
					for (int nInd = 2; nInd < aPerimeter.Length; nInd++){
						triangle(aPerimeter, oCentre, nInd, out dThisArea, out oThisCentroid);
						addin(adCentroid, dThisArea, oThisCentroid);
						dArea += dThisArea;
					}
					//	Complete the polygon, by getting the triangle from the last point to the first.
					triangle(aPerimeter, oCentre, 0, out dThisArea, out oThisCentroid);
					addin(adCentroid, dThisArea, oThisCentroid);
					dArea += dThisArea;
					
					//	The area is in dArea, and the weighted sum of the centroids is in adCentroid
					//	Calculate the centroid.  Check that the points are not co-linear and the area is
					//	zero.
					if (dArea > (Double.Epsilon * 1000.0)){
						adCentroid[0] /= dArea;
						adCentroid[1] /= dArea;
						adCentroid[2] /= dArea;
					} else {
						adCentroid[0] = 0;
						adCentroid[1] = 0;
						adCentroid[2] = 0;
					}
					
					outLat = new latitude((int) (adCentroid[0] + 0.5));
					outLong = new longitude((int) (adCentroid[1] + 0.5));
					outAlt = adCentroid[2];
					break;
					
				case "C":		//	Circle
					//	For a circle the centroid is the centre
					//	Interpret the values as ints to get the right constructor.
					outLat = new latitude((int) aPerimeter[0].Value2);  
					outLong = new longitude((int) aPerimeter[0].Value3);
					outAlt = aPerimeter[0].Value4;
					break;
					
				case "S":		//	Sector
					double	dRmax = aPerimeter[0].Value3;
					double	dRmin = aPerimeter[0].Value2;
					double	dTheta = aPerimeter[0].Value4 * Math.PI / 180.0;	// theta in radians
					double	dSectAz = aPerimeter[0].Value1 * Math.PI / 180.0;	//	Azimuth of the sector in radians.
					
					//	Calculate the location of the centroid from the origin along the X axis, in Km.
					double	dXc = 2.0 * Math.Sin(dTheta) * 
					              (Math.Pow(dRmax, 3) - Math.Pow(dRmin, 3)) /
					              (3.0 * dTheta * (dRmax * dRmax - dRmin * dRmin));
					
					//	Convert the distance from the hub to the centroid from Km to spherical angle
					double dDr = dXc / 6371.0;		// get distance in radians
					double dLr = (90.0 - AreaLatit.ToDouble()) * Math.PI / 180.0; // Complement of lat in radians
					
					//	Get the complement of Latitude of the centroid of the sector in radians
					double dLCr = Math.Acos(Math.Cos(dDr) * Math.Cos(dLr) + 
					                        Math.Sin(dDr) * Math.Sin(dLr) * Math.Cos(dSectAz));
					//	Get the difference in Longitude at the pole:-
					double dDeltaLong = Math.Asin(Math.Sin(dSectAz) / Math.Sin(dLCr) * Math.Sin(dDr));
					
					//	Now create the output parameters, first convert to degrees
					dLCr *= 180 / Math.PI;
					dDeltaLong *= 180 / Math.PI;
					
					outLat = new latitude(90.0 - dLCr);
					outLong = new longitude(AreaLongit.ToDouble() - dDeltaLong); // Long in degrees
					outAlt = AreaGrnd;
					break;
					
				default:
					//	No Perimeter type.
					outLat = new latitude(0);
					outLong = new longitude(0);
					outAlt = -2.0;
					break;
			}
		}
		
	}
	
	
	public class CoordAreaIO
	{
		dbconnect cn;
        string tblAreaParam;
        string tblAreaPerim;
		public dbgEvent dbgE = new dbgEvent();
		
		public CoordAreaIO()
		{
			try{
				cn = new dbconnect();
			} catch (Exception ex){
				throw new Exception("*CoordAreaIO* Could not connect to database: " +
				                    ex.Message);
			}

            tblAreaParam = HttpContext.Current.Session["s_schema"].ToString() + ".AreaParam";
            tblAreaPerim = HttpContext.Current.Session["s_schema"].ToString() + ".AreaPerim";

			//	First check to see that the tables exist.  Create them if necessary.
            if (!cn.tableexists(tblAreaParam))
            {
				//	Create the table.
				string cCreateParam = "create table " + tblAreaParam + "(" +
															"AreaID	      int," +
															"AreaLic	  char(11)," +
															"AreaName     char(16)," +
															"AreaOper     char(6)," +
															"AreaProv     char(2)," +
															"AreaType     char(1)," +
															"AreaPerim    char(1)," +
															"AreaFile     char(16)," +
															"AreaNote     char(300)," +
															"AreaLatit	  int," +
															"AreaLongit	  int," +
															"AreaGrnd	    float(24)," +
															"AreaAcode    char(12)," +
															"AreaAht	    float(24)," +
															"AreaAzmth	  float(24)," +
															"AreaDist	    float(24)," +
															"AreaFslTx	  float(24)," +
															"AreaFslRx	  float(24)," +
															"AreaEqptTx   char(8)," +
															"AreaEqptRx   char(8)," +
															"AreaSrvcTx	  char(6)," +
															"AreaPwrTx	  float(24)," +
															"AreaTrafTx   char(6)," +
															"AreaTrafRx   char(6)," +
															"AreaBand1    char(4)," +
															"AreaBand2    char(4)," +
															"AreaFreq1Lo	float(24)," +
															"AreaFreq1Hi	float(24)," +
															"AreaFreq2Lo	float(24)," +
															"AreaFreq2Hi	float(24)," +
															"ChannelBW	  float(24)," +
															"FirstChan    char(1)," +
															"AreaPolTx    char(1)," +
															"AreaPolRx    char(1)," +
															"RemoteNum	  int," +
															"RemoteTxPwr  float(24)," +
															"RemoteAcode  char(12)," +
															"RemoteEqptTx char(8)," +
															"RemoteEqptRx char(8)," +
															"RemoteSrvcTx char(6)," +
															"RemoteFslTx	float(24)," +
															"RemoteFslRx	float(24)," +
															"RemoteAht		float(24)," +
															"MDate				date)";
				try{
					cn.nonquery(cCreateParam);
				} catch (Exception Ex){
					throw new Exception("Could not create AreaParm table: " + Ex.Message);
				}
				
				string cCreatePerim = "create table " + tblAreaPerim  + "(" +
				                      "AreaID int," +
				                      "Value1 float(53)," + 
				                      "Value2 float(53)," +
				                      "Value3 float(53)," +
				                      "Value4 float(53)," +
				                      "MDate  date)";
				try{
					cn.nonquery(cCreatePerim);
				} catch (Exception Ex){
					throw new Exception("Could not create AreaPerim table: " + Ex.Message);
				}
			}
		}
		

		public CoordArea Read(int nAreaID)
		{
			CoordArea oCA = null;
			// dbconnect ocn = new dbconnect();
			DataTable oDT = null;
			string sDate;
			
			string cSQL = "SELECT AreaID,AreaLic,AreaName,AreaOper,AreaProv,AreaType,AreaPerim," +
			                     "AreaFile,AreaNote,AreaLatit,AreaLongit,AreaGrnd,AreaAcode,AreaAht," +
			                     "AreaAzmth,AreaDist,AreaFslTx,AreaFslRx,AreaEqptTx,AreaEqptRx," +
			                     "AreaSrvcTx,AreaPwrTx,AreaTrafTx,AreaTrafRx," +
			                     "AreaBand1,AreaBand2,AreaFreq1Lo,AreaFreq1Hi,AreaFreq2Lo,AreaFreq2Hi," +
			                     "ChannelBW,FirstChan,AreaPolTx,AreaPolRx,RemoteNum,RemoteTxPwr," +
			                     "RemoteAcode,RemoteEqptTx,RemoteEqptRx,RemoteSrvcTx,RemoteFslTx," +
			                     "RemoteFslRx,RemoteAht,mdate " +
								 "FROM " + tblAreaParam +
								 " WHERE AreaID=" + nAreaID.ToString();
			
			try {
				oDT = cn.retrieve(cSQL);
			} catch (Exception Ex){
				throw new Exception("*Error* Reading CoordArea: " + Ex.Message + "\n SQL: " + cSQL);
			}
			
			if (oDT.Rows.Count > 0){
				oCA = new CoordArea();
				//	Assume one row.
				DataRow oRow = oDT.Rows[0];
				
				oCA.AreaID = Convert.ToInt32(oRow["AreaID"]);
				oCA.AreaLic = oRow["AreaLic"].ToString();
				oCA.AreaName = oRow["AreaName"].ToString();
				oCA.AreaOper = oRow["AreaOper"].ToString();
				oCA.AreaProv = oRow["AreaProv"].ToString();
				oCA.AreaType = oRow["AreaType"].ToString();
				oCA.AreaPerim = oRow["AreaPerim"].ToString();
				oCA.AreaFile = oRow["AreaFile"].ToString();
				oCA.AreaNote = oRow["AreaNote"].ToString();
				oCA.AreaLatit = new latitude(Convert.ToInt32(oRow["AreaLatit"].ToString()));
				oCA.AreaLongit = new longitude(Convert.ToInt32(oRow["AreaLongit"].ToString()));
				oCA.AreaGrnd = Convert.ToDouble(oRow["AreaGrnd"]);
				oCA.AreaAcode = oRow["AreaAcode"].ToString();
				oCA.AreaAht = Convert.ToDouble(oRow["AreaAht"]);
				oCA.AreaAzmth = Convert.ToDouble(oRow["AreaAzmth"]);
				oCA.AreaDist = Convert.ToDouble(oRow["AreaDist"]);
				oCA.AreaFslTx = Convert.ToDouble(oRow["AreaFslTx"]);
				oCA.AreaFslRx = Convert.ToDouble(oRow["AreaFslRx"]);
				oCA.AreaEqptTx = oRow["AreaEqptTx"].ToString();
				oCA.AreaEqptRx = oRow["AreaEqptRx"].ToString();
				oCA.AreaSrvcTx = oRow["AreaSrvcTx"].ToString();
				oCA.AreaPwrTx = Convert.ToDouble(oRow["AreaPwrTx"]);
				oCA.AreaTrafTx = oRow["AreaTrafTx"].ToString();
				oCA.AreaTrafRx = oRow["AreaTrafRx"].ToString();
				oCA.AreaBand1 = oRow["AreaBand1"].ToString();
				oCA.AreaBand2 = oRow["AreaBand2"].ToString();
				try{oCA.AreaFreq1Lo = Convert.ToDouble(oRow["AreaFreq1Lo"]);} catch{oCA.AreaFreq1Lo = 0.0;}
				try{oCA.AreaFreq1Hi = Convert.ToDouble(oRow["AreaFreq1Hi"]);} catch{oCA.AreaFreq1Hi = 0.0;}
				try{oCA.AreaFreq2Lo = Convert.ToDouble(oRow["AreaFreq2Lo"]);} catch{oCA.AreaFreq2Lo = 0.0;}
				try{oCA.AreaFreq2Hi = Convert.ToDouble(oRow["AreaFreq2Hi"]);} catch{oCA.AreaFreq2Hi = 0.0;}
				oCA.ChannelBW = Convert.ToDouble(oRow["ChannelBW"]);
				oCA.FirstChan = oRow["FirstChan"].ToString();
				oCA.AreaPolTx = oRow["AreaPolTx"].ToString();
				oCA.AreaPolRx = oRow["AreaPolRx"].ToString();
				oCA.RemoteNum = Convert.ToInt32(oRow["RemoteNum"]);
				oCA.RemoteTxPwr = Convert.ToDouble(oRow["RemoteTxPwr"]);
				oCA.RemoteAcode = oRow["RemoteAcode"].ToString();
				oCA.RemoteEqptTx = oRow["RemoteEqptTx"].ToString();
				oCA.RemoteEqptRx = oRow["RemoteEqptRx"].ToString();
				oCA.RemoteSrvcTx = oRow["RemoteSrvcTx"].ToString();
				oCA.RemoteFslTx = Convert.ToDouble(oRow["RemoteFslTx"]);
				oCA.RemoteFslRx = Convert.ToDouble(oRow["RemoteFslRx"]);
				oCA.RemoteAht = Convert.ToDouble(oRow["RemoteAht"]);
				try {
					sDate	= Convert.ToDateTime(oRow["mdate"]).ToString("yyyy.MM.dd HH:mm:ss");
					oCA.mdate	= DateTime.Parse(sDate);
				} catch {}
			}

			//	Now read in the points on the perimeter
			ArrayList alPerim = new ArrayList();
			string cSQLP = "SELECT AreaID,Value1,Value2,Value3,Value4,MDate " +
			                 "FROM " + tblAreaPerim +
			                " WHERE AreaID=" + nAreaID.ToString();
			try {
				oDT = cn.retrieve(cSQLP);
			} catch (Exception Ex){
				throw new Exception("*Error* Could not retrieve perimeter for " + 
				                    nAreaID.ToString() + "\n" + Ex.Message);
			}
			
			for (int nInd = 0; nInd < oDT.Rows.Count; nInd++){
				AreaPoint oAP = new AreaPoint();
				DataRow oRow = oDT.Rows[nInd];
				
				oAP.AreaID = Convert.ToInt32(oRow["AreaID"]);
				oAP.Value1 = Convert.ToDouble(oRow["Value1"]);
				oAP.Value2 = Convert.ToDouble(oRow["Value2"]);
				oAP.Value3 = Convert.ToDouble(oRow["Value3"]);
				oAP.Value4 = Convert.ToDouble(oRow["Value4"]);
				try {
					sDate	= Convert.ToDateTime(oRow["mdate"]).ToString("yyyy.MM.dd HH:mm:ss");
					oAP.mdate	= DateTime.Parse(sDate);
				} catch {}
				
				alPerim.Add(oAP);
			}
			//	Now add the perimeter points to the array
			oCA.aPerimeter = new AreaPoint[alPerim.Count];
			for (int nInd = 0; nInd < alPerim.Count; nInd++){
				oCA.aPerimeter[nInd] = (AreaPoint) alPerim[nInd];
			}
						
			// ocn.dbdisconnect();
			return oCA;
		}
		

		public CoordArea[] Read()
		{
			ArrayList alCA = new ArrayList();
			// dbconnect ocn = new dbconnect();
			DataTable oDT = null;
			CoordArea oCA = null;
			
			string sDate;
			DataTable oDTP = null;
			string cSQLP = null;
			string cSQL = "SELECT AreaID,AreaLic,AreaName,AreaOper,AreaProv,AreaType,AreaPerim," +
			                     "AreaFile,AreaNote,AreaLatit,AreaLongit,AreaGrnd,AreaAcode,AreaAht," +
			                     "AreaAzmth,AreaDist,AreaFslTx,AreaFslRx,AreaEqptTx,AreaEqptRx," +
			                     "AreaSrvcTx,AreaPwrTx,AreaTrafTx,AreaTrafRx," +
			                     "AreaBand1,AreaBand2,AreaFreq1Lo,AreaFreq1Hi,AreaFreq2Lo,AreaFreq2Hi," +
			                     "ChannelBW,FirstChan,AreaPolTx,AreaPolRx,RemoteNum,RemoteTxPwr," +
			                     "RemoteAcode,RemoteEqptTx,RemoteEqptRx,RemoteSrvcTx,RemoteFslTx," +
			                     "RemoteFslRx,RemoteAht,mdate " +
								 "FROM " + tblAreaParam  +
								 " ORDER BY AreaID";
			
			try {
				oDT = cn.retrieve(cSQL);
			} catch (Exception Ex){
				throw new Exception("*Error* Reading CoordArea: " + Ex.Message + "\n SQL: " + cSQL);
			}
			
			if (oDT.Rows.Count > 0){
				foreach (DataRow oRow in oDT.Rows){
					oCA = new CoordArea();
				
					oCA.AreaID = Convert.ToInt32(oRow["AreaID"]);
					oCA.AreaLic = oRow["AreaLic"].ToString();
					oCA.AreaName = oRow["AreaName"].ToString();
					oCA.AreaOper = oRow["AreaOper"].ToString();
					oCA.AreaProv = oRow["AreaProv"].ToString();
					oCA.AreaType = oRow["AreaType"].ToString();
					oCA.AreaPerim = oRow["AreaPerim"].ToString();
					oCA.AreaFile = oRow["AreaFile"].ToString();
					oCA.AreaNote = oRow["AreaNote"].ToString();
					oCA.AreaLatit = new latitude(Convert.ToInt32(oRow["AreaLatit"].ToString()));
					oCA.AreaLongit = new longitude(Convert.ToInt32(oRow["AreaLongit"].ToString()));
					oCA.AreaGrnd = Convert.ToDouble(oRow["AreaGrnd"]);
					oCA.AreaAcode = oRow["AreaAcode"].ToString();
					oCA.AreaAht = Convert.ToDouble(oRow["AreaAht"]);
					oCA.AreaAzmth = Convert.ToDouble(oRow["AreaAzmth"]);
					oCA.AreaDist = Convert.ToDouble(oRow["AreaDist"]);
					oCA.AreaFslTx = Convert.ToDouble(oRow["AreaFslTx"]);
					oCA.AreaFslRx = Convert.ToDouble(oRow["AreaFslRx"]);
					oCA.AreaEqptTx = oRow["AreaEqptTx"].ToString();
					oCA.AreaEqptRx = oRow["AreaEqptRx"].ToString();
					oCA.AreaSrvcTx = oRow["AreaSrvcTx"].ToString();
					oCA.AreaPwrTx = Convert.ToDouble(oRow["AreaPwrTx"]);
					oCA.AreaTrafTx = oRow["AreaTrafTx"].ToString();
					oCA.AreaTrafRx = oRow["AreaTrafRx"].ToString();
					oCA.AreaBand1 = oRow["AreaBand1"].ToString();
					oCA.AreaBand2 = oRow["AreaBand2"].ToString();
					try{oCA.AreaFreq1Lo = Convert.ToDouble(oRow["AreaFreq1Lo"]);} catch{oCA.AreaFreq1Lo = 0.0;}
					try{oCA.AreaFreq1Hi = Convert.ToDouble(oRow["AreaFreq1Hi"]);} catch{oCA.AreaFreq1Hi = 0.0;}
					try{oCA.AreaFreq2Lo = Convert.ToDouble(oRow["AreaFreq2Lo"]);} catch{oCA.AreaFreq2Lo = 0.0;}
					try{oCA.AreaFreq2Hi = Convert.ToDouble(oRow["AreaFreq2Hi"]);} catch{oCA.AreaFreq2Hi = 0.0;}
					oCA.ChannelBW = Convert.ToDouble(oRow["ChannelBW"]);
					oCA.FirstChan = oRow["FirstChan"].ToString();
					oCA.AreaPolTx = oRow["AreaPolTx"].ToString();
					oCA.AreaPolRx = oRow["AreaPolRx"].ToString();
					oCA.RemoteNum = Convert.ToInt32(oRow["RemoteNum"]);
					oCA.RemoteTxPwr = Convert.ToDouble(oRow["RemoteTxPwr"]);
					oCA.RemoteAcode = oRow["RemoteAcode"].ToString();
					oCA.RemoteEqptTx = oRow["RemoteEqptTx"].ToString();
					oCA.RemoteEqptRx = oRow["RemoteEqptRx"].ToString();
					oCA.RemoteSrvcTx = oRow["RemoteSrvcTx"].ToString();
					oCA.RemoteFslTx = Convert.ToDouble(oRow["RemoteFslTx"]);
					oCA.RemoteFslRx = Convert.ToDouble(oRow["RemoteFslRx"]);
					oCA.RemoteAht = Convert.ToDouble(oRow["RemoteAht"]);
					try {
						sDate	= Convert.ToDateTime(oRow["mdate"]).ToString("yyyy.MM.dd HH:mm:ss");
						oCA.mdate	= DateTime.Parse(sDate);
					} catch {}
					
					//	Now read in the points on the perimeter
					ArrayList alPerim = new ArrayList();
					cSQLP = "SELECT AreaID,Value1,Value2,Value3,Value4,MDate " +
							"FROM " + tblAreaPerim +
							" WHERE AreaID=" + oCA.AreaID.ToString();
					try {
						oDTP = cn.retrieve(cSQLP);
					} catch (Exception Ex){
						throw new Exception("*Error* Could not retrieve perimeter for " + 
																oCA.AreaID.ToString() + "\n" + Ex.Message);
					}
			
					//	Retrieve the area points for this 
					for (int nInd = 0; nInd < oDTP.Rows.Count; nInd++){
						AreaPoint oAP = new AreaPoint();
						DataRow oRowp = oDTP.Rows[nInd];
						
						oAP.AreaID = Convert.ToInt32(oRowp["AreaID"]);
						oAP.Value1 = Convert.ToDouble(oRowp["Value1"]);
						oAP.Value2 = Convert.ToDouble(oRowp["Value2"]);
						oAP.Value3 = Convert.ToDouble(oRowp["Value3"]);
						oAP.Value4 = Convert.ToDouble(oRowp["Value4"]);
						try {
							sDate	= Convert.ToDateTime(oRowp["mdate"]).ToString("yyyy.MM.dd HH:mm:ss");
							oAP.mdate	= DateTime.Parse(sDate);
						} catch {}
						
						alPerim.Add(oAP);
					}
					//	Now add the perimeter points to the array
					oCA.aPerimeter = new AreaPoint[alPerim.Count];
					for (int nInd = 0; nInd < alPerim.Count; nInd++){
						oCA.aPerimeter[nInd] = (AreaPoint) alPerim[nInd];
					}
								
					alCA.Add(oCA);	//	Add it to the Arraylist.
				}
			}
			
			// ocn.dbdisconnect();
			
			CoordArea[] aCA = new CoordArea[alCA.Count];
			for (int nInd = 0; nInd < alCA.Count; nInd++){
				aCA[nInd] = (CoordArea) alCA[nInd];
			}
			return aCA;
		}
		

		public bool exists(CoordArea oArea)
		{
			bool		bExists = false;
			
			if (cn.tableexists("AreaParam")){
				string cSQL = "SELECT count(*) FROM " + tblAreaParam  + " WHERE AreaID=" + 
				                      oArea.AreaID.ToString();
			
				if (Convert.ToInt32(cn.getscalar(cSQL)) > 0){
					bExists = true;
				}
			}

			return bExists;
		}
		
		
		public int Write(CoordArea oArea)
		{
			string cSQL = "";
			string cOper = "";
			
			//	The lats and longs only exist for the hub in a Multipoint.
			int	nLatit = 0;
			int nLngit = 0;
			if (oArea.AreaType == "M"){
				nLatit = oArea.AreaLatit.ToInt();
				nLngit = oArea.AreaLongit.ToInt();
			}
			
			//	When we write the area, we change the date
			oArea.mdate = DateTime.Now;

			dbgE.dbgRaise(1, "Setting date to: " + oArea.mdate.ToString("yyyy.MM.dd HH:mm:ss"));
			
			if (oArea.AreaID != 0 && exists(oArea)){
				//	If the area already exists in the file, then update.
				cOper = "updating";
				cSQL = "UPDATE " + tblAreaParam +
                        " SET " +                                 
						//	oArea.AreaID.ToString() + ",'" + 
						"AreaLic='" + oArea.AreaLic + "'," +
	                    "AreaName='" + oArea.AreaName + "'," +
                        "AreaOper='" + oArea.AreaOper + "'," +
                        "AreaProv='" + oArea.AreaProv + "'," +
                      "AreaType='" + oArea.AreaType + "'," +
                      "AreaPerim='" + oArea.AreaPerim + "'," +
                      "AreaFile='" + oArea.AreaFile + "'," +
                      "AreaNote='" + OELSupport.doublequote(oArea.AreaNote) + "'," +
                      "AreaLatit=" + nLatit.ToString() + "," +
                      "AreaLongit=" + nLngit.ToString() + "," +
                      "AreaGrnd=" + oArea.AreaGrnd.ToString() + "," +
                      "AreaAcode='" + oArea.AreaAcode + "'," +
                      "AreaAht=" + oArea.AreaAht.ToString() + "," +
                      "AreaAzmth=" + oArea.AreaAzmth.ToString() + "," +
                      "AreaDist=" + oArea.AreaDist.ToString() + "," +
                      "AreaFslTx=" + oArea.AreaFslTx.ToString() + "," +
                      "AreaFslRx=" + oArea.AreaFslRx.ToString() + "," +
                      "AreaEqptTx='" + oArea.AreaEqptTx + "'," +
                      "AreaEqptRx='" + oArea.AreaEqptRx + "'," +
                      "AreaSrvcTx='" + oArea.AreaSrvcTx + "'," +
                      "AreaPwrTx=" + oArea.AreaPwrTx.ToString() + "," +
                      "AreaTrafTx='" + oArea.AreaTrafTx + "'," +
                      "AreaTrafRx='" + oArea.AreaTrafRx + "'," +
                      "AreaBand1='" + oArea.AreaBand1 + "'," +
                      "AreaBand2='" + oArea.AreaBand2 + "'," +
                      "AreaFreq1Lo=" + oArea.AreaFreq1Lo.ToString() + "," +
                      "AreaFreq1Hi=" + oArea.AreaFreq1Hi.ToString() + "," +
                      "AreaFreq2Lo=" + oArea.AreaFreq2Lo.ToString() + "," +
                      "AreaFreq2Hi=" + oArea.AreaFreq2Hi.ToString() + "," +
                      "ChannelBW=" + oArea.ChannelBW.ToString() + "," +
                      "FirstChan='" + oArea.FirstChan + "'," +
                      "AreaPolTx='" + oArea.AreaPolTx + "'," +
                      "AreaPolRx='" + oArea.AreaPolRx + "'," +
                      "RemoteNum=" + oArea.RemoteNum.ToString() + "," +
                      "RemoteTxPwr=" + oArea.RemoteTxPwr.ToString() + "," +
                      "RemoteAcode='" + oArea.RemoteAcode + "'," +
                      "RemoteEqptTx='" + oArea.RemoteEqptTx + "'," +
                      "RemoteEqptRx='" + oArea.RemoteEqptRx + "'," +
                      "RemoteSrvcTx='" + oArea.RemoteSrvcTx + "'," +
                      "RemoteFslTx=" + oArea.RemoteFslTx.ToString() + "," +
                      "RemoteFslRx=" + oArea.RemoteFslRx.ToString() + "," +
                      "RemoteAht=" + oArea.RemoteAht.ToString() + "," +
                      "mdate='" + oArea.mdate.ToString("yyyy.MM.dd HH:mm:ss") + "' " + 
                "WHERE AreaID=" + oArea.AreaID.ToString();
			} else {
				//	If the area does not exist, then insert it.
				//	First get the next AreaID.
				int		nArea = -1;
				try {
					nArea = cn.execintproc("fds3.getnextid('AREAID')");
					oArea.AreaID = nArea;
				} catch (Exception Ex){
					throw new Exception(">" + Ex.Message);
				}
			  cOper = "inserting";
				cSQL = "INSERT INTO " + tblAreaParam + "(AreaID,AreaLic,AreaName,AreaOper," +
											                "AreaProv,AreaType,AreaPerim,AreaFile," +
											                "AreaNote,AreaLatit,AreaLongit,AreaGrnd," +
											                "AreaAcode,AreaAht,AreaAzmth,AreaDist," +
											                "AreaFslTx,AreaFslRx,AreaEqptTx,AreaEqptRx," +
											                "AreaSrvcTx,AreaPwrTx,AreaTrafTx,AreaTrafRx," +
											                "AreaBand1,AreaBand2," +
											                "AreaFreq1Lo,AreaFreq1Hi,AreaFreq2Lo,AreaFreq2Hi," +
											                "ChannelBW,FirstChan," +
											                "AreaPolTx,AreaPolRx,RemoteNum,RemoteTxPwr," +
											                "RemoteAcode,RemoteEqptTx,RemoteEqptRx,RemoteSrvcTx," +
											                "RemoteFslTx,RemoteFslRx,RemoteAht,mdate)" +
											"values(" + oArea.AreaID.ToString() + ",'" + 
											            oArea.AreaLic + "','" +
	                                oArea.AreaName + "','" +
                                  oArea.AreaOper + "','" +
                                  oArea.AreaProv + "','" +
                                  oArea.AreaType + "','" +
                                  oArea.AreaPerim + "','" +
                                  oArea.AreaFile + "','" +
                                  OELSupport.doublequote(oArea.AreaNote) + "'," +
                                  nLatit.ToString() + "," +
                                  nLngit.ToString() + "," +
                                  oArea.AreaGrnd.ToString() + ",'" +
                                  oArea.AreaAcode + "'," +
                                  oArea.AreaAht.ToString() + "," +
                                  oArea.AreaAzmth.ToString() + "," +
                                  oArea.AreaDist.ToString() + "," +
                                  oArea.AreaFslTx.ToString() + "," +
                                  oArea.AreaFslRx.ToString() + ",'" +
                                  oArea.AreaEqptTx + "','" +
                                  oArea.AreaEqptRx + "','" +
                                  oArea.AreaSrvcTx + "'," +
                                  oArea.AreaPwrTx.ToString() + ",'" +
                                  oArea.AreaTrafTx + "','" +
                                  oArea.AreaTrafRx + "','" +
                                  oArea.AreaBand1 + "','" +
                                  oArea.AreaBand2 + "'," +
                                  oArea.AreaFreq1Lo + "," +
                                  oArea.AreaFreq1Hi + "," +
                                  oArea.AreaFreq2Lo + "," +
                                  oArea.AreaFreq2Hi + "," +                                  
                                  oArea.ChannelBW.ToString() + ",'" +
                                  oArea.FirstChan + "','" +
                                  oArea.AreaPolTx + "','" +
                                  oArea.AreaPolRx + "'," +
                                  oArea.RemoteNum.ToString() + "," +
                                  oArea.RemoteTxPwr.ToString() + ",'" +
                                  oArea.RemoteAcode + "','" +
                                  oArea.RemoteEqptTx + "','" +
                                  oArea.RemoteEqptRx + "','" +
                                  oArea.RemoteSrvcTx + "'," +
                                  oArea.RemoteFslTx.ToString() + "," +
                                  oArea.RemoteFslRx.ToString() + "," +
                                  oArea.RemoteAht.ToString() + ",'" +
                                  oArea.mdate.ToString("yyyy.MM.dd HH:mm:ss") + "'" + 
                                  ")";
      }
      
      dbgE.dbgRaise(2, "SQL is: " + cSQL);
                                              
      try {
				cn.nonquery(cSQL);
      } catch (Exception Ex){
				throw new Exception("*Error* " + cOper + " Area Param: " + Ex.Message + 
				                    "<br>SQL:" + cSQL);
      }
      
      //	Handle the Perimeter table.  First delete the perimeter points, and then 
      //	re-add them all.
      try {
				cSQL = "DELETE FROM " + tblAreaPerim + " WHERE AreaID=" + oArea.AreaID.ToString();
				cn.nonquery(cSQL);
      } catch (Exception Ex){
				throw new Exception("*Error* deleting old perimeter for AreaID: " + 
				                    oArea.AreaID.ToString() +
				                    "\n<br> " + Ex.Message);
      }
      
      try {
				//	Assign the areaid to the perimeter
				for (int nInd = 0; nInd < oArea.aPerimeter.Length; nInd++){
					oArea.aPerimeter[nInd].AreaID = oArea.AreaID;
				}
				
				//	Now update the database.
				int nPerimInd = 0;
				string cDate = "";
				foreach (AreaPoint oAP in oArea.aPerimeter){
					if (oAP.mdate.ToString() == ""){
						cDate = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
					} else {
						cDate = oAP.mdate.ToString("yyyy.MM.dd HH:mm:ss");
					}
					cSQL = "INSERT INTO " + tblAreaPerim + "(AreaID,Value1,Value2,Value3,Value4,mdate) " +
																"VALUES (" + oAP.AreaID.ToString() + "," +
																						oAP.Value1.ToString() + "," +
																						oAP.Value2.ToString() + "," +
																						oAP.Value3.ToString() + "," +
																						oAP.Value4.ToString() + ",'" +
																						cDate + "')";
					dbgE.dbgRaise(3, "Perim " + (nPerimInd++).ToString() + ": " + cSQL);
					cn.nonquery(cSQL);				                                   
				}
      }	catch (Exception Ex) {
				throw new Exception("*Error* saving the perimeter for " + 
				                    oArea.AreaID.ToString() +
				                    "\n<br> " + Ex.Message);
      }
      
      return 0;
    }                                   
		
		

		public int Delete(int AreaID)
		{
			string cSQL = "DELETE FROM " + tblAreaParam +
			                    " WHERE AreaID=" + AreaID.ToString();
			try{
				cn.nonquery(cSQL);
			}	catch (Exception Ex){
				throw new Exception("*Error* Could not delete from AreaParam: " + Ex.Message +
				                    "\nSQL is: " + cSQL);
			}
			
			cSQL = "DELETE FROM " + tblAreaPerim +
			             " WHERE AreaID=" + AreaID.ToString();
			try{
				cn.nonquery(cSQL);
			}	catch (Exception Ex){
				throw new Exception("*Error* Could not delete from AreaPerim: " + Ex.Message +
				                    "\nSQL is: " + cSQL);
			}

			return 0;
		}
		


		public ConnectionState checkconnection()
		{
			ConnectionState oCS = cn.Connection.State;
			if (oCS != ConnectionState.Open){
				cn = new dbconnect();
			}
			
			return oCS;
		}
		
		
		public void Close()
		{
			cn.dbdisconnect();
		}
	}
}
