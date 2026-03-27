using System;
using System.IO;
using	System.Text;

		//	Plot support routines 
		//
		//	(c) copyright Frequency Coordination System Association, Ottawa, 2003
		//	Programmed by Orthogonal Endeavours Ltd., North Vancouver, BC
		//


namespace DBAccess
{
	/// <summary>
	/// OELPlot is a general class for plotting routines.  The routines are static 
	/// methods, and all the output is written to the current page.
	/// </summary>
	public class OELPlot
	{
		protected static string[] aColour = new string[6] {"blue",   "green", "red", 
		                                                   "orange", "cyan",  "magenta"};
		protected static int[] aWidth = new int[6] {4, 4, 2, 2, 1, 1};

		//  *************************************************************************************
		public static string plotpolarback(double nWidth, double nMax, string cTitle, 
		                                   string cNote)
		{
			double nRadius = nMax * 1.1;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			sw.WriteLine("<svg:svg width='{0}' height='{1}' viewBox='{2} {3} {4} {5}'>", 
			             Math.Round(nWidth),
			             Math.Round(nWidth),
			             Math.Round(-nRadius), 
									 Math.Round(-nRadius),
									 Math.Round(2 * nRadius), 
									 Math.Round(2 * nRadius));
			sw.WriteLine("<svg:title>{0}</svg:title>", cTitle);
			sw.WriteLine("<svg:desc>{0}</svg:desc>", cNote);
			// Plot the outer circle and the coords
			sw.WriteLine("<svg:circle cx='0' cy='0' r='{0}' " + 
			             "style='stroke:gray;fill:none;stroke-width:0.25' />",
			             nMax);
			//	Now the coordinate lines
			double nStartx;
			double nStarty;
			double nEndx;
			double nEndy;
			double nTorad = Math.PI / 180.0;

			double nMarkSize = nMax * .05;

			for (double nAng = 0; nAng < 180; nAng += 30){
				nStartx = nMax * Math.Sin(nAng * nTorad);
				nStarty = -nMax * Math.Cos(nAng * nTorad);
				nEndx = nMax * Math.Sin((nAng + 180.0) * nTorad);
				nEndy = -nMax * Math.Cos((nAng + 180.0) * nTorad);
				sw.WriteLine("<svg:line x1='{0}' y1='{1}' x2='{2}' y2='{3}' />",
				             nStartx,
				             nStarty, 
				             nEndx,
				             nEndy);
				//	Mark the angles
				sw.WriteLine("<svg:text x='{0}' y='{1}' style='font-size: {2}'>{3}</svg:text>",
				             nStartx,
				             (nAng <= 90 ? nStarty : nStarty + nMarkSize),
				             nMarkSize,
				             nAng);
				sw.WriteLine("<svg:text x='{0}' y='{1}' text-anchor='end' style='font-size: {2}'>" +
				             "{3}</svg:text>",
				             nEndx,
										 (nAng <= 90 ? nEndy + nMarkSize : nEndy),
										 nMarkSize,
										 (nAng + 180));
			}
			
			//	Now the title
			double nFontSize = nMax * 0.07;
			
			sw.WriteLine("<svg:text x='{0}' y='{1}' text-anchor='end' style='font-size:{2}; font-weight:bold'>" +
			             "{3}</svg:text>",
			             nMax,
			             (-nMax),
									 nFontSize,
									 cTitle);
			nFontSize = nMax * 0.06;
			sw.WriteLine("<svg:text x='{0}' y='{1}' text-anchor='end' style='font-size:{2}'>" +
			             "{3}</svg:text>",
									 nMax,
									 (-nMax * 0.92),
									 nFontSize,
									 OELSupport.standarddate());
			sw.Close();
			return sb.ToString(); 
		}


		//  *************************************************************************************
		//	Plot the curve.  aX is the angle and aY the magnitude
		public static string plotpolarcurve(double[] aX, double[] aY, string cColour)
		{
			double	nLen = aX.Length;
			int			nInd;
			double	nX;
			double	nY;
			double	toRad = Math.PI / 180;
			StringBuilder	cPath = new StringBuilder("<svg:path d='M");
			
			if (aY.Length < nLen){
				nLen = aY.Length;
			}
			
			for (nInd = 0; nInd < nLen; nInd++){
				nX = aY[nInd] * Math.Sin(aX[nInd] * toRad);
				nY = -aY[nInd] * Math.Cos(aX[nInd] * toRad);
				cPath.Append(nX.ToString() + "," + nY.ToString() + " ");
			}
			
			cPath.Append("Z' style='stroke:" + cColour + "; fill:none; stroke-width:0.1' />");
			return cPath.ToString();
		}


		//  *************************************************************************************
		public static string plotpolarlegend(string[] aLegend, double nEndPoint)
		{
			int			nCount = aLegend.Length;
			double	nLineHt = nEndPoint * 0.05;
			double	nLineLn = nEndPoint * 0.25;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			//	Allow half a line for the box
			sw.WriteLine("<svg:rect x='{0}' y='{1}' width='{2}' height='{3}' " + 
			             "style='stroke:black;fill:none;stroke-width:0.1' />",
			             (nEndPoint - (nLineLn + nLineHt)),
			             (nEndPoint - (nCount * nLineHt + nLineHt)),
			             (nLineLn + nLineHt),
			             (nCount * nLineHt + nLineHt));
			for (int nInd = 0, nStart = (int) (nEndPoint - (nCount * nLineHt)); 
					nInd < nCount; 
					nInd++, nStart += (int) nLineHt){
				sw.WriteLine("<svg:text x='{0}' y='{1}' style='font-size:{2};fill:{3}>{4}</svg:text>",
				             (nEndPoint - nLineLn),
				             nStart,
				             (nLineHt * .9),
				             aColour[nInd],
				             aLegend[nInd]);
			}
			sw.Close();
			
			return sb.ToString();        
		}


		//  *************************************************************************************
		//	Plot a polar plot from arrays fed in
		public static string plotpolar(double nWidth, string cTitle, double[] ax,
		                               params object[] aYin)
		{
			int nArgs = aYin.Length;
			int nArrayLen = ax.Length;
			StringBuilder sb = new StringBuilder();
			
			//	Get the max y.
			double nMax = 0;
			int nInd = 0;
			int nInd1;
			double[] aY;
			
			while (nInd < nArgs){
				aY = (double []) aYin[nInd];
				for (nInd1 = 0; nInd1 < aY.Length; nInd1++){
					nMax = Math.Max(nMax, aY[nInd1]);
				}
				
				nInd += 2;
			}
			
			//	Plot the background
			sb.Append(plotpolarback(nWidth, nMax, cTitle, "Polar Plot").ToString());
			
			//	Plot the curves
			nInd = 0;
			nInd1 = 0;
			while (nInd < nArgs){
				aY = (double []) aYin[nInd];
				sb.Append(plotpolarcurve(ax, aY, aColour[nInd1++]).ToString());

				nInd += 2;
			}
			
			string[] aLegend = new string[nArgs / 2];
			for (nInd = 1, nInd1= 0; nInd < nArgs; nInd += 2, nInd1++){
				aLegend[nInd1] = (string) aYin[nInd];
			}
			
			sb.Append(plotpolarlegend(aLegend, nMax).ToString() + "\n</svg:svg>");
			
			return sb.ToString();
		}


		//***************************************************************************************
		//	
		//		Rectangular plot support
		//
		//	Plot rectangular background
		private static double nPct = -1.0;
		private static double nHeight1;
		private static double nWidth1;
		private static double nRatio;
		private static double nBottom;
		private static double nMaxX;
		private static double nMinY;
		private static double nMaxY;
		private static double nIntY;
		private static double[] aScale;
		private static double[] aXscale;
		private static double nXstart = 70;
		private static double nXwidth = 910;

		//  *************************************************************************************
		public static string plotrectback(double nWidth, double nHeight, string cTitle)
		{
			nPct = .90;		//	90%
			nHeight1 = nHeight * nPct;
			nWidth1 = nWidth * nPct;
			nRatio = nHeight1 / nWidth1;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			sw.WriteLine("<svg:svg height='{0}' width='{1}' viewBox='0 0 1000.0 {2}'>",
			             nHeight1,
			             nWidth1,
			             (nRatio * 1000.0));
			sw.WriteLine("<svg:title>{0}</svg:title>", cTitle);
			sw.WriteLine("<svg:desc>Rectangular plot of {0}</svg:desc>", cTitle);
			sw.WriteLine("<svg:rect x='0' y='0' height='{0}' width='1000.0' " +
			             "style='stroke:black;fill:none;stroke-width=2' />",
			             (nRatio * 1000));
			sw.WriteLine("<svg:text x='500' y='20' text-anchor='middle' style='font-weight:bold'>" +
										cTitle + "</svg:text>");
			sw.WriteLine("<svg:text x='500' y='35' text-anchor='middle' style='font-size:12'>" +
										OELSupport.standarddate() + "</svg:text>");
			string cLogo = OELSupport.urlhead() + "images/Image4.gif";
			sw.WriteLine("<svg:image xlink:href='" + cLogo + "' x='2' y='2' width='100' height='100' />");
			sw.WriteLine("<svg:text x='120' y='25' style='font-family:sans-serif;font-weight:bold'>" +
									 "Frequency</svg:text>");
			sw.WriteLine("<svg:text x='120' y='50' style='font-family:sans-serif;font-weight:bold'>" +
									 "Coordination</svg:text>");
			sw.WriteLine("\n<svg:text x='120' y='75' style='font-family:sans-serif;font-weight:bold'>" +
									 "System</svg:text>");
			sw.WriteLine("\n<svg:text x='120' y='100' style='font-family:sans-serif;font-weight:bold'>" +
									 "Association</svg:text>");
			sw.Close();
			
			return sb.ToString();
		}


		//  *************************************************************************************
		//	These two routines put a point in the graph rectangle.  Note that the size and location of
		//	the plot rectangle is hard-coded into them.
		public static double ploty(double nY, double nMinY, double nMaxY)
		{
			double nYval = (120 + (0.75 * nBottom)) - (((nY - nMinY) / (nMaxY - nMinY)) * (0.75 * nBottom));
			
			return nYval;
		}

		//  *************************************************************************************
		public static double plotx(double nX, double nStart, double nEnd)
		{
			double nXval = nXstart + ((nX - nStart) / (nEnd - nStart)) * nXwidth;
			
			return nXval;
		}


		//  *************************************************************************************
		//	Calculate the vertical scale.  Returns array(low, high, inc).
		public static double[] getscale(double nLow, double nHigh)
		{
			//	Right now we ignore low value
			double nSpread = nHigh - nLow;
			double nLogInterval = Math.Log10(nSpread);  // Get log base 10
			double nLogBase = Math.Floor(nLogInterval);
			double nLogMantissa = nLogInterval - nLogBase;
			double nNormal = Math.Pow(10.0, nLogMantissa);
			double nScaleFact = Math.Pow(10.0, nLogBase);
//		Response.Write("\n<!-- nHigh: " + nHigh +
//									" nLow: " + nLow +
//									" nLogInterval: " + nLogInterval + 
//									" nLogBase: " + nLogBase +
//									" nLogMantissa: " + nLogMantissa +
//									" nNormal: " + nNormal +
//									" nScaleFact: " + nScaleFact +
//									" -->");	
			
			double nInterval;
			if (nNormal < 2.5){
				nInterval = 0.25;
			} else if (nNormal < 5){
				nInterval = 0.5;
			} else {
				nInterval = 1.0;
			}
			nInterval *= nScaleFact;
			
			double nNumLowInterval;
			double nNumHighInterval;
			
			if (nLow <= 0){
				nNumLowInterval = -Math.Ceiling(-nLow / nInterval);
			} else {
				nNumLowInterval = Math.Floor(nLow / nInterval);
			}
			
			if (nHigh <= 0){
				nNumHighInterval = -Math.Floor(-nHigh / nInterval);
			} else {
				nNumHighInterval = Math.Ceiling(nHigh / nInterval);
			}
			
			return new double[3]{nNumLowInterval * nInterval, nNumHighInterval * nInterval, nInterval};
		}


		//	*************************************************************************************
		//	This function is needed enough to put it here
		public static double log10(double x)
		{	
			return Math.Log10(x);
		}


		//  *************************************************************************************
		//	Calculate a logarithmic scale.  Returns array(Number of log sections, value of first).
		public static double[] getlogscale(double nLow, double nHigh)
		{
			double nSpread = Math.Ceiling(log10(nHigh)) - Math.Floor(log10(nLow));
			double nLogLow = Math.Pow(10.0, Math.Floor(log10(nLow)));
//		Response.Write("\n<!-- nHigh: " + nHigh +
//									" nLow: " + nLow +
//									" nSpread: " + nSpread + 
//									" nLogLow: " + nLogLow +
//									" -->");	
			return new double[2]{nSpread, nLogLow};
		}



		//  **************************************************************************************
		//	Plot the legend 
		public static string plotrectlegend(string[] aDesc)
		{
			int			nInd;
			double	nY;
			double	nTop;
			double	xLeft = 800;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			nY = nTop = 10;
			
			for (nInd = 0; nInd < aDesc.Length; nInd++){
				nY += 16;
				sw.WriteLine("<svg:line x1='{0}' y1='{1}' x2='{2}' y2='{3}' " +
				             "style='stroke:{4};stroke-width:{5}' />",
										 (xLeft + 20),
										 nY,
										 (xLeft + 50),
										 nY,
										 aColour[nInd],
										 (aWidth[nInd]));
				sw.WriteLine("<svg:text x='{0}' y='{1}' style='font-size:14'>{2}</svg:text>",
											(xLeft + 70),
											(nY + 5), 
											aDesc[nInd]);
			}
			
			sw.WriteLine("<svg:rect x='{0}' y='{1}' height='{2}' width='{3}' " +
			             "style='fill:none;stroke:black' />",
									 xLeft, 
									 nTop, 
									 (nY + 4),
									 (980 - xLeft));
			sw.Close();
			
			return sb.ToString();
		}


		//  *************************************************************************************
		//	Plot all the curves.  Only the ordinate array is passed (as aX).  The rest are part of a
		//	variable length argument list consisting of ..., aY, cNote pairs.  The length of aY must be
		//	the same as aX, and cNote is the note that will be part of the legend.
		public static string plotrectcurves(double[] aX, params object[] aArgs)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			int nArgCt = aArgs.Length;
			int nInd;
			
			nBottom = nRatio * 1000;
			
			if (nPct == -1.0){
				return "<h3>Invalid calling order for plotrectcurves<h3>";
			}

			//	First get the arrays and calculate the ranges
			double[][] aY = new double[aArgs.Length / 2][];
			string[] aLegend = new string[aArgs.Length / 2];
			nMaxY = -999;
			nMaxX = aX[aX.Length - 1];
			int nInd1;
			int nYind;
			for (nInd = 0, nYind = 0; nInd < nArgCt; nInd += 2, nYind++){
				aY[nYind] = (double[]) aArgs[nInd];
				aLegend[nYind] = (string) aArgs[nInd + 1];
				
				//	Get the max Y
				for (nInd1 = 0; nInd1 < aY[nYind].Length; nInd1++){
					if (nMaxY <= aY[nYind][nInd1]){
						nMaxY = aY[nYind][nInd1];
					}
				}
			}
			aScale = OELPlot.getscale(0, nMaxY);	//	Get array (min, max, interval)
			//Response.Write("\n<!-- Scale: (" + aScale[0] + ", " + aScale[1] + ", " + aScale[2] + ") -->");
			nMaxY = aScale[1];		// Reset the max to the max of the scale
			
			//	Rectangle around the plot.
			sw.WriteLine("<svg:rect x='{0}' y='120' width='{1}' height='{2}' " +
			             "style='fill:none;stroke:black;stroke-width:1' />", 
			             nXstart, 
			             nXwidth, 
			             (0.75 * nBottom));
			sw.Close();  // Don't need it any more.
			//	x-axis scale
			sb.Append(plotrectantx());
			//	y-axis scale
			sb.Append(plotrectanty());
			
			//	Plot each line - first from -180 to 0, then from 0 to 180
			//	We do this in two passes.
			bool		Is180 = aX[aX.Length - 1] < 270;
			double	nXval;
			int			nXinc;
			int			nXfirst;
			int			nXlast;
			for (nInd = 0; nInd < aY.Length; nInd++){
				if (Is180){
					nXinc = -1;
					nXfirst = aX.Length - 1;
					nXlast = 0;
				} else {
					nXinc = +1;
					//	Find the point >= 180
					for (nInd1 = 0; nInd1 < aX.Length; nInd1++){
						if (aX[nInd1] >= 180){
							break;
						}
					}
					nXfirst = nInd1;
					nXlast = aX.Length - 1;
				}
				
				//	Plot the first half of the line. -180 to 0 degrees.  Backwards from the end
				//	in 180, and from 180 to the end in 360.
				sb.Append("<svg:path d='M");
				for (int nXind = nXfirst; 
						(Is180) ? nXind >= nXlast : nXind <= nXlast; 
						nXind += nXinc){
					if (Is180){
						nXval = -aX[nXind];
					} else {
						nXval = aX[nXind] - 360;
					}
					sb.AppendFormat("{0},{1} ", 
					                OELPlot.plotx(nXval, -180, +180), 
					                ploty(nMaxY - aY[nInd][nXind], 0, nMaxY));
				}
				
				//	Plot the second half of the line.  0 to 180 degrees ascending in both.
				if (Is180){
					nXlast = aX.Length - 1;
				} else {
					if (aX[nXfirst] == 180){
						nXlast = nXfirst;
					} else {
						nXlast = nXfirst - 1;
					}
				}
				for (nInd1 = 0; nInd1 <= nXlast; nInd1++){
					sb.AppendFormat("{0},{1} ",
					                plotx(aX[nInd1], -180, +180),
					                ploty(nMaxY - aY[nInd][nInd1], 0, nMaxY));
				}
				sb.AppendFormat("' style='fill:none;stroke: {0};stroke-width: {1}' />",
				                aColour[nInd],
				                aWidth[nInd]);
			}

			sb.Append(plotrectlegend(aLegend));
				
			return sb.ToString();
		}


		//  *************************************************************************************
		//	Draw the vertical line for the x coord
		public static string scalex(double nX, double nStart, double nEnd)
		{
			return	"\n<svg:line x1='" + plotx(nX, nStart, nEnd).ToString() + 
							"' y1='" + ploty(0, 0, nMaxY).ToString() +
							"' x2='" + plotx(nX, nStart, nEnd).ToString() +
							"' y2='" + ploty(nMaxY, 0, nMaxY).ToString() +
										"' style='stroke:gray;stroke-width:0.5' />";
		}

		//  *************************************************************************************
		//	Plot the antenna x-axis scale
		public static string plotrectantx()
		{
			int			nInc = 30;
			double	nScaley = (0.94 * nBottom);
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			for (double nX = -180; nX <= 180; nX += nInc){
				//	Don't draw the vertical for first and last
				if (nX > -180 && nX < +180){
					sw.WriteLine(scalex(nX, -180, +180));
				}
				sw.WriteLine("\n<svg:text x='{0}' y='{1}' " + 
				             "text-anchor='middle' style='font-size:14'>{2}</svg:text>", 
				             plotx(nX, -180, +180),
				             nScaley, 
				             nX);
			}
			sw.WriteLine("\n<svg:text x='500' y='{0}' text-anchor='middle' " +
										" style='font-size:16'>Degrees off-axis</svg:text>",
										(nScaley + 20));
			sw.Close();
			
			return sb.ToString();
		}



		//  **************************************************************************************
		//	Draw the horizontal lines for the y coord
		public static string scaley(double nY, double nMin, double nMax)
		{
			string cWidth;
			
			if (nY == 0){
				cWidth = "1";
			} else {
				cWidth = "0.5";
			}
			
			return	"\n<svg:line x1='" + plotx(0, 0, 1).ToString() +
							"' y1='" + ploty(nY, nMin, nMax).ToString() + 
							"' x2='" + plotx(1, 0, 1).ToString() +
							"' y2='" + ploty(nY, nMin, nMax).ToString() +
							"' style='stroke:gray;stroke-width:" + cWidth + "' />";
		}


		//  *************************************************************************************
		//	Plot the horizontal lines for the antenna curves.  Must have already calculated the maxes
		public static string plotrectanty()
		{
			nIntY = aScale[2];
			nMaxY = aScale[1];
			nMinY = aScale[0];
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			for (double nY = nMinY; nY <= nMaxY; nY += nIntY){
				if (nY > nMinY && nY < nMaxY){
					sw.WriteLine(scaley(nY, nMinY, nMaxY));
				}
				sw.WriteLine("<svg:text x='{0}' y='{1}' text-anchor='end' style='font-size:14'>{2}" +
										 "</svg:text>",
										 (nXstart - 3),
										 ploty(nY, 0, nMaxY),
										 (nMaxY - nY));
			}
			sw.WriteLine("<svg:text x='15' y='{0}' transform='rotate(90,15,{1})' " +
			             "text-anchor='middle' style='font-size:16' >" +
									 "Discrimination (dB)</svg:text>",
									 ploty(nMaxY / 2, 0, nMaxY),
									 ploty(nMaxY / 2, 0, nMaxY));
			sw.Close();
			
			return sb.ToString();
		}


		//  *************************************************************************************
		//	Plot the horizontal lines for the ctx curves.  Must have already calculated the maxes
		public static string plotrectctxy(double nRqco)
		{
			string cLabel = "Required C/I (db)";
			if (nRqco < 0){
				cLabel = "Maximum Interference Level (dBm)";		
			}
			return plothorizctx(nRqco, cLabel);
		}


		//	************************************************************************************
		//	plot horizontal lines for any ctx curve
		public static string plothorizctx(double nRqco, string cInLabel)
		{
			string cLabel = cInLabel;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			
			for (double nY = nMinY; (nRqco >= 0) ? nY <= nMaxY : nY >= nMaxY; nY += nIntY){
				if (nY != nMinY && nY != nMaxY){
					sw.WriteLine(scaley(nY, nMinY, nMaxY));
				}
				sw.WriteLine("<svg:text x='{0}' y='{1}' " +
				             "text-anchor='end' style='font-size:14'>{2}</svg:text>", 
				             (nXstart - 3),
				             ploty(nY, nMinY, nMaxY), 
				             nY);
			}
			
			sw.WriteLine("<svg:text x='15' y='{0}' transform='rotate(90,15,{1})' " +
			             "text-anchor='middle' style='font-size:16' >{2}</svg:text>",
			             ploty(nMaxY / 2, 0, nMaxY),
			             ploty(nMaxY / 2, 0, nMaxY),
			             cLabel);
			sw.Close();
			
			return sb.ToString();
		}

		//  *************************************************************************************
		//	Plot the ctx x-axis scale.  This is logarithmic.
		public static string plotrectctxx()
		{
			int nInd;
			
			double nScaley = (0.94 * nBottom);
			double nLow = aXscale[1];			//	Starting from
			double nNumber = aXscale[0];	//	Number of log intervals.
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);

			double[] aLineMults = new double[10];
			double[] aLineMarks = new double[10];
			int[] aLinePrint = new int[10];
			//	Initialize the lines in a log interval
			aLineMults[0] = 0;
			aLineMarks[0] = 1;
			aLinePrint[0] = 1;
			
			aLineMarks[1] = 1.5;
			aLineMults[1] = log10(aLineMarks[1]);	
			aLinePrint[1] = 1;
			
			aLineMarks[2] = 2.0;
			aLineMults[2] = log10(aLineMarks[2]);
			aLinePrint[2] = -1;
			
			aLineMarks[3] = 3.0;
			aLineMults[3] = log10(aLineMarks[3]);
			aLinePrint[3] = 1;
			
			aLineMarks[4] = 4.0;
			aLineMults[4] = log10(aLineMarks[4]);
			aLinePrint[4] = 1;
			
			aLineMarks[5] = 5.0;
			aLineMults[5] = log10(aLineMarks[5]);
			aLinePrint[5] = -1;
			
			aLineMarks[6] = 6.0;
			aLineMults[6] = log10(aLineMarks[6]);
			aLinePrint[6] = 0;
			
			aLineMarks[7] = 7.0;
			aLineMults[7] = log10(aLineMarks[7]);
			aLinePrint[7] = 1;
			
			aLineMarks[8] = 8.0;
			aLineMults[8] = log10(aLineMarks[8]);
			aLinePrint[8] = 0;
			
			aLineMarks[9] = 9.0;
			aLineMults[9] = log10(aLineMarks[9]);
			aLinePrint[9] = 0;
			
			for (double nX = 0, nXval = nLow; nX <= nNumber; nX++, nXval *= 10){
				//	Put up a log series of coordinates
				for (nInd = 0; nInd < aLineMults.Length; nInd++){
					sw.WriteLine(scalex(nX + aLineMults[nInd], 0, nNumber));
					if (Math.Abs(nXval - 1000) < .1 || (nX > 0 && nInd == 0) || aLinePrint[nInd] < 0){
						if (aLinePrint[nInd] != 0){
							sw.WriteLine("<svg:text x='{0}' y='{1}' " +
							             "text-anchor='middle' style='font-size:14'>{2}</svg:text>",
							             plotx(nX + aLineMults[nInd], 0, nNumber),
							             nScaley,
							             nXval * aLineMarks[nInd] / 1000);
						}
					}
				}
			}
			
			sw.WriteLine("<svg:text x='500' y='{0}' text-anchor='middle' " +
									 " style='font-size:16'>Frequency Separation (MHz)</svg:text>",
									 (nScaley + 20));
			sw.Close();
			
			return sb.ToString();
		}



		//  *************************************************************************************
		public static string plotrectend()
		{
			return "\n</svg:svg><br />";
		}

		//  *************************************************************************************
		//	Plot a tick on the Y axis in the next colour
		public static string plotytick(double nY, int nColour)
		{
			double nRadius = 4;
			
			return "\n<svg:circle cx='" + plotx(0, 0, 1).ToString() + 
						 "' cy='" + ploty(nY, nMinY, nMaxY).ToString() +
						 "' r='" + nRadius.ToString() +
						 "' style='stroke:" + aColour[nColour] + 
						 ";fill:" + aColour[nColour] + "'/>";
		}


		//  *************************************************************************************
		public static string plotctxcurves(double[] aFsep, double[] aRQ, string cRQ, string cLabel)
		{
			int nInd;
			double nLogStart;
			double	nRqco = 0.0;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);

			try {
					
				nBottom = nRatio * 1000;
				
				//	Rectangle around the plot.
				sw.WriteLine("<svg:rect x='{0}' y='120' width='{1}' height='{2}' " +
										"style='fill:none;stroke:black;stroke-width:1' />",
										nXstart,
										nXwidth,
										(0.75 * nBottom));

				//	First get the arrays and calculate the ranges
				string[] aLegend = new string[2] {cRQ, ""};
				nMaxY = -99999;
				nMinY = 99999;
				double nMaxX = aFsep[aFsep.Length - 1];
				//	Get the max/min Y
				for (nInd = 0; nInd < aRQ.Length; nInd++){
					if (nMaxY < aRQ[nInd]){
						nMaxY = aRQ[nInd];
					}
					if (nMinY > aRQ[nInd]){
						nMinY = aRQ[nInd];
					}
				}
				aScale = getscale(nMinY, nMaxY);	//	Get array (min, max, interval)
				//Response.Write("\n<!--plotctxcurves Y Scale: (" + aScale[0] + ", " + aScale[1] + ", " + aScale[2] + ") -->");
				nIntY = aScale[2];
				nMaxY = aScale[1];
				nMinY = aScale[0];
	//			if (nRqco < 0){
	//				double nTemp = nMaxY;
	//				nMaxY = nMinY;
	//				nMinY = nTemp;
	//				nIntY = -nIntY;
	//			}
				//	y-axis scale
				sw.WriteLine(plothorizctx(+1.0, cLabel));
				
				//	Get the log scale for the x coords.
				double[] aLogX = new double[aFsep.Length];
				//	There is a chance that the first element could be zero.
				int nStartEl = 0;
				if (aFsep[0] <= 0){
					nStartEl = 1;
					nRqco = aRQ[0];
				}
				
				for (nInd = nStartEl; nInd < aFsep.Length; nInd++){
					aLogX[nInd] = log10(aFsep[nInd]);		//	Get the log base 10.
				}
				
				//	Get the x scale (log)
				aXscale = getlogscale((aFsep[0] == 0) ? aFsep[1] : aFsep[0], 
															aFsep[aFsep.Length - 1]); // get array (number of log scales, starting at)
				//Response.Write("<!--plotctxcurves Log Scale:- number: " + aXscale[0] + ", starting at: " + aXscale[1] + " -->");
				//	x-axis scale
				sw.WriteLine(plotrectctxx());
				
				nLogStart = log10(aXscale[1]);		//	This is the zero abscissa
				string cLine = "\n<svg:path d='M" + 
											plotx(aLogX[nStartEl] - nLogStart, 0, aXscale[0]).ToString() + "," +
											ploty(aRQ[nStartEl], nMinY, nMaxY).ToString();
				for (nInd = nStartEl + 1; nInd < aLogX.Length; nInd++){
					cLine += " " + plotx(aLogX[nInd] - nLogStart, 0, aXscale[0]).ToString() + "," + 
									ploty(aRQ[nInd], nMinY, nMaxY).ToString();
				}
				cLine += "' style='fill:none;stroke:" + aColour[0] + 
								";stroke-width:" + (aWidth[0] / 2).ToString() + "' />";
				sw.WriteLine(cLine);
				
				if (nStartEl > 0){
					//	Put the rqco mark in with the next colour
					sw.WriteLine(plotytick(nRqco, 1/* aLegend.Length*/));
					aLegend[/* aLegend.Length*/ 1]	= "CoFrequency";
				}
				
				sw.WriteLine(plotrectlegend(aLegend));
				
			}	catch (Exception Ex){
				return "Serious Error: " + Ex.Message + " at\n" + Ex.StackTrace;
			} finally {
				sw.Close();
			}

			return sb.ToString();
		}
		
		

		//  *************************************************************************************
		public static string plotctxcurves(double[] aFsep, double[] aRQ, string cRQ, double nRqco)
		{
			int nInd;
			double nLogStart;
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
				
			nBottom = nRatio * 1000;
			
			//	Rectangle around the plot.
			sw.WriteLine("<svg:rect x='{0}' y='120' width='{1}' height='{2}' " +
									 "style='fill:none;stroke:black;stroke-width:1' />",
									 nXstart,
									 nXwidth,
									 (0.75 * nBottom));

			//	First get the arrays and calculate the ranges
			string[] aLegend = new string[2] {cRQ, ""};
			nMaxY = -99999;
			nMinY = 99999;
			double nMaxX = aFsep[aFsep.Length - 1];
			//	Get the max/min Y
			for (nInd = 0; nInd < aRQ.Length; nInd++){
				if (nMaxY < aRQ[nInd]){
					nMaxY = aRQ[nInd];
				}
				if (nMinY > aRQ[nInd]){
					nMinY = aRQ[nInd];
				}
			}
			aScale = getscale(nMinY, nMaxY);	//	Get array (min, max, interval)
			//Response.Write("\n<!--plotctxcurves Y Scale: (" + aScale[0] + ", " + aScale[1] + ", " + aScale[2] + ") -->");
			nIntY = aScale[2];
			nMaxY = aScale[1];
			nMinY = aScale[0];
			if (nRqco < 0){
				double nTemp = nMaxY;
				nMaxY = nMinY;
				nMinY = nTemp;
				nIntY = -nIntY;
			}
			//	y-axis scale
			sw.WriteLine(plotrectctxy(nRqco));
			
			//	Get the log scale for the x coords.
			double[] aLogX = new double[aFsep.Length];
			//	There is a chance that the first element could be zero.
			int nStartEl = (aFsep[0] <= 0) ? 1 : 0;
			
			for (nInd = nStartEl; nInd < aFsep.Length; nInd++){
				aLogX[nInd] = log10(aFsep[nInd]);		//	Get the log base 10.
			}
			
			//	Get the x scale (log)
			aXscale = getlogscale((aFsep[0] == 0) ? aFsep[1] : aFsep[0], 
														aFsep[aFsep.Length - 1]); // get array (number of log scales, starting at)
			//Response.Write("<!--plotctxcurves Log Scale:- number: " + aXscale[0] + ", starting at: " + aXscale[1] + " -->");
			//	x-axis scale
			sw.WriteLine(plotrectctxx());
			
			nLogStart = log10(aXscale[1]);		//	This is the zero abscissa
			string cLine = "\n<svg:path d='M" + 
			               plotx(aLogX[nStartEl] - nLogStart, 0, aXscale[0]).ToString() + "," +
									   ploty(aRQ[nStartEl], nMinY, nMaxY).ToString();
			for (nInd = nStartEl + 1; nInd < aLogX.Length; nInd++){
				cLine += " " + plotx(aLogX[nInd] - nLogStart, 0, aXscale[0]).ToString() + "," + 
								 ploty(aRQ[nInd], nMinY, nMaxY).ToString();
			}
			cLine += "' style='fill:none;stroke:" + aColour[0] + 
			         ";stroke-width:" + (aWidth[0] / 2).ToString() + "' />";
			sw.WriteLine(cLine);
			
			if (Math.Abs(nRqco) != 1.0){
				//	Put the rqco mark in with the next colour
				sw.WriteLine(plotytick(nRqco, 1/* aLegend.Length*/));
				aLegend[/* aLegend.Length*/ 1]	= "CoFreq. Req.";
			}
			sw.WriteLine(plotrectlegend(aLegend));
				
			sw.Close();
			return sb.ToString();
		}

	}
}
