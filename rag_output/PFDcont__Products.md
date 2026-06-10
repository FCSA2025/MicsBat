# Documented File: Products.cs
**Repository Path:** `PFDcont\Products.cs`
**Primary Layer:** `PFDcont`
**Namespace:** `PFDcont`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace PFDcont
{
    /// <summary>
    /// This class provides methods that collate and write the qty.4 types
    /// of result/output text files for a PFDcont run: comprehensive plain 
    /// text report (.REP), a CSV-formated data file (.CSV), and both types
    /// of MapInfo Data Interchange Format files (.MID and .MIF); all
    /// data files created by PFDcont are sent to the user as email attachments.
    /// </summary>
    public class Products
    {
        private const string KML_TEMPLATE = @"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<kml xmlns = ""http://www.opengis.net/kml/2.2"" >
<Document id=""root_doc"">
<Schema name = ""_SchemaName"" id=""_SchemaID"">
	<SimpleField name = ""Name"" type=""string""></SimpleField>
</Schema>
<Folder><name>_FolderName</name>
  <Placemark>
	<name>_PlacemarkNameMin</name>
	<description>Minimum Power Flux Density</description>
	<Style><LineStyle><color>FF000000</color><width>1</width></LineStyle><PolyStyle><fill>0</fill></PolyStyle></Style>
      <LineString><coordinates>_MinimaXY</coordinates></LineString>
  </Placemark>
  <Placemark>
	<name>_PlacemarkNameMax</name>
	<description>Maximum Power Flux Density</description>
	<Style><LineStyle><color>FF000000</color><width>1</width></LineStyle><PolyStyle><fill>0</fill></PolyStyle></Style>
      <LineString><coordinates>_MaximaXY</coordinates></LineString>
  </Placemark>
</Folder>
</Document></kml>";


        /// <summary>
        /// This is the top-level method that collates and writes the qty.4 types
        /// of result/output text files for a PFDcont run: comprehensive plain 
        /// text report (.REP), a CSV-formated data file (.CSV), and both types
        /// of MapInfo Data Interchange Format files (.MID and .MIF); all
        /// data files created by PFDcont are sent to the user as email attachments.
        /// </summary>
        /// <param name="mPFD"></param>
        /// <param name="mtSiteStr"></param>
        /// <param name="suAntStr"></param>
        /// <param name="mApTable"></param>
        /// <param name="cEmailAddr"></param>
        public static void WriteFilesSendEmail(PFD mPFD,
                                                MtSiteStr mtSiteStr,
                                                SuAntStr suAntStr,
                                                List<TtabRow> mApTable,
                                                string cEmailAddr)
        {
            string mPathWithFileExtn;
            int retCode;
            int numRows = 0;

            string faxref = mPFD.AnteCodeOfXref;

            // Check for any nonsense.
            if (mApTable == null)
            {
                Log2.e("\n\nProducts.WriteFilesSendEmail(): ERROR: List<TtabRow> object is NULL.");
                Application.ExitQuietly(10000);
            }
            else if (String.IsNullOrWhiteSpace(cEmailAddr))
            {
                Log2.e("\n\nProducts.WriteFilesSendEmail(): ERROR: email address is null, empty or whitespace.");
                Application.ExitQuietly(10001);
            }
            else if (String.IsNullOrWhiteSpace(mPFD.BaseReportPath))
            {
                Console.Write("\r\n*** Report path prescribed in command line is null, empty or whitespace\r\n");
                Application.ExitQuietly(10002);
            }
            else if ((mPFD.BaseReportPath.Length + 4) > Constant.TSIP_MAX_PATH)
            {
                Console.Write("\r\n*** Report path prescribed in command line is too long.  Max length is {0}\r\n",
                                        Constant.TSIP_MAX_PATH - 4);
                Application.ExitQuietly(10003);
            }

            numRows = mApTable.Count;

            // Create and write the .REP report  and send email.
            if (mPFD.IsReport && mPFD.IsCalculated)
            {
                mPathWithFileExtn = mPFD.BaseReportPath + ".rep";

                // Write to the .REP file.
                WriteToFile_REP(mPathWithFileExtn, mPFD, faxref, suAntStr, mApTable);

                // Send email with .REP file as attachment.
                string cTitle = String.Format("PFDCont Report: {0}.rep", mPFD.BaseFileName);

                retCode = SendEmail(cEmailAddr, cTitle, "--", mPathWithFileExtn);

                if (retCode != Constant.SUCCESS)
                {
                    Console.Write("\r\n*** Error ({0}) emailing report.\r\n", retCode);
                }
                else
                {
                    Console.Write("\r\nReport emailed with subject: PFDCont Report: {0}.rep", mPFD.BaseFileName);
                }
            }

            // Create and write the .CSV report and send email.
            if (mPFD.IsCSV && mPFD.IsCalculated)
            {
                mPathWithFileExtn = mPFD.BaseReportPath + ".csv";

                // Write to the .CSV file.
                WriteToFile_CSV(mPathWithFileExtn, mPFD, mtSiteStr, faxref, suAntStr, mApTable/*, latitude, longitude*/);

                // Send email with .CSV file as attachment.
                string cTitle = String.Format("PFDCont CSV File: {0}.csv", mPFD.BaseFileName);

                retCode = SendEmail(cEmailAddr, cTitle, "--", mPathWithFileExtn);

                if (retCode != Constant.SUCCESS)
                {
                    Console.Write("\r\n*** Error ({0}) emailing CSV.\r\n", retCode);
                }
                else
                {
                    Console.Write("\r\nReport emailed with subject: PFDCont CSV File: {0}.csv", mPFD.BaseFileName);
                }
            }

            // Create and write the MapInfo MID and MIF data files  and send emails.
            if (mPFD.IsMapInfo && mPFD.IsCalculated)
            {
                // Write to the .MID and .MIF files.
                WriteToFiles_MID_MIF(mPFD, mtSiteStr, faxref, suAntStr, mApTable/*, mPFD.Latitude, mPFD.Longitude*/);

                // Send email with .MID file as attachment.
                // Send email with .MIF file as attachment.
                string cTitle = String.Format("PFDCont MapInfo File: {0}.mif", mPFD.BaseFileName);

                retCode = SendEmail(cEmailAddr, cTitle, "--", mPFD.BaseReportPath + ".mif");

                if (retCode != Constant.SUCCESS)
                {
                    Console.Write("\r\n*** Error ({0}) emailing MapInfo MIF file.\r\n", retCode);
                }
                else
                {
                    Console.Write("\r\nReport emailed with subject: PFDCont MapInfo File: {0}.mif", mPFD.BaseFileName);
                }

                cTitle = String.Format("PFDCont MapInfo File: {0}.mid", mPFD.BaseFileName);

                retCode = SendEmail(cEmailAddr, cTitle, "--", mPFD.BaseReportPath + ".mid");

                if (retCode != Constant.SUCCESS)
                {
                    Console.Write("\r\n*** Error ({0}) emailing MapInfo MID file.\r\n", retCode);
                }
                else
                {
                    Console.Write("\r\nReport emailed with subject: PFDCont MapInfo File: {0}.mid", mPFD.BaseFileName);
                }

            }

            // Create and write the KML file and send email.
            if (mPFD.IsMapInfo && mPFD.IsCalculated)
            {
                // Write to the KML file.
                WriteToFile_KML(mPFD, mtSiteStr, faxref, suAntStr, mApTable/*, mPFD.Latitude, mPFD.Longitude*/);

                // Send email with .KML file as attachment.
                string cTitle = String.Format("PFDCont KML File: {0}.kml", mPFD.BaseFileName);

                retCode = SendEmail(cEmailAddr, cTitle, "--", mPFD.BaseReportPath + ".kml");

                if (retCode != Constant.SUCCESS)
                {
                    Console.Write("\r\n*** Error ({0}) emailing KML file.\r\n", retCode);
                }
                else
                {
                    Console.Write("\r\nReport emailed with subject: PFDCont KML File: {0}.kml", mPFD.BaseFileName);
                }
            }
        }

        /// <summary>
        /// This method collates and writes the comprehensive plain text report (.REP) and
        /// sends it to the user as an email attachment.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mPFD"></param>
        /// <param name="faxref"></param>
        /// <param name="suAntStr"></param>
        /// <param name="mApTable"></param>
        private static void WriteToFile_REP(string filePath, PFD mPFD, string faxref,
                                                SuAntStr suAntStr, List<TtabRow> mApTable)
        {

            string mPathWithFileExtn = filePath;


            if (mPathWithFileExtn.Length > Constant.TSIP_MAX_PATH)
            {
                Console.Write("\r\n*** Report path prescribed in command line is too long.  Max length is {0}\r\n",
                                        Constant.TSIP_MAX_PATH - 4);
                Application.ExitQuietly(97);
            }

            //	Open the file as a TextWriter.
            TextWriter fReport = null;
            try
            {
                fReport = new StreamWriter(mPathWithFileExtn);
            }
            catch (Exception e)
            {
                Log2.e("\n\nPFDcont.Main(): ERROR: new StreamWriter(): Exception: " + e.Message);
                Console.Write("\r\n*** Can't open {0}.\r\n", mPathWithFileExtn);
                Application.ExitQuietly(96);
            }

            /*	Write the header  */
            fReport.Write("Frequency Coordination System Association");
            if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
            {
                fReport.Write("           Power Flux Density Contours");
            }
            else
            {
                fReport.Write("                     Coverage Contours");
            }

            fReport.Write("\r\nMicrowave Interference Calculation System                     Build {0}\r\n",
                             Info.BuildMetaData);

            fReport.Write("Filename: {0}\r\n", mPFD.BaseFileName);

            if (!String.IsNullOrWhiteSpace(mPFD.Location))
            {
                /*	There is a site location: show it */
                fReport.Write("\r\nSite: {0} {1}.", mPFD.Call1, mPFD.Location);
            }

            fReport.Write("\r\nLatitude: {0}, Longitude: {1}", mPFD.LatStr, mPFD.LongStr);

            fReport.Write("\r\nAntenna: {0,-12} XRef: {1,-12} Azimuth: {2,-5:F1}  Gain: {3,-5:F1}",
                            mPFD.AnteCode, faxref, mPFD.TxAzim, suAntStr.acAnt.again);
            fReport.Write("\r\nAFSL: {0,-5:F1}  Bandwidth: {1,-6:F1}  Frequency: {2,-11:F3}",
                                             mPFD.Fsl, mPFD.Bandwidth, mPFD.Frequency);
            fReport.Write("\r\nTx Antenna Height: {0,-6:F1}m. Rx Height: {1,-6:F1}m  TxPower: {2,-6:F1}dBW",
                                             mPFD.TxHeight, mPFD.RxHeight, mPFD.TxPwr);
            if (mPFD.Eloss == PFDloss.SPHERICAL)
            {
                fReport.Write("\r\nUsing Spherical Earth Losses.");
            }
            else if (mPFD.Eloss == PFDloss.TERRAIN)
            {
                fReport.Write("\r\nUsing Terrain Losses with Over Horizon Calculations.");
            }
            else
            {
                /*	Unknown loss type */
                fReport.Write("\r\nUNKNOWN Loss Type");
            }
            if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
            {
                fReport.Write("\r\n          Minimum PFD Contour: {0,-6:F1}        Maximum PFD Contour: {1,-6:F1}\r\n  Az.   Gain Dist(km)   Latitude     Longitude Dist(km) Latitude     Longitude\r\n\r\n",
                                mPFD.MinPFDlevel, mPFD.MaxPFDlevel);
            }
            else
            {
                /*	Coverage contours */
                fReport.Write("\r\n          Minimum Coverage Contour power: {0,-6:F1}", mPFD.MinRxPwr);
                fReport.Write(" (PFD Contour: {0,-6:F1})", mPFD.MinPFDlevel);
                fReport.Write("\r\n   Az.  Gain Dist(km)   Latitude     Longitude");
                if (mPFD.Eloss == PFDloss.TERRAIN)
                {
                    fReport.Write("  Dist(km) Latitude     Longitude");
                }
                fReport.Write("\r\n\r\n");
            }

            /*	Unload the table into the report */
            //for (nInd = 0, pTable = apTable;
            //     nInd <= nCurrRow;
            //         nInd++, pTable++)
            int nInd = 0;
            foreach (TtabRow pTable in mApTable)
            {
                /*	Print a line to the report */
                if (nInd % 40 == 0 && nInd > 0)
                {
                    fReport.Write("\r\n\fPage {0}\r\n\r\n", ((int)(nInd / 40) + 1));
                }

                fReport.Write("\r\n{0,5:F1} {1,6:F1} {2,6:F1} {3,12} {4,13}",
                                       pTable.dAz, pTable.dGain, pTable.dmindist,
                                                 pTable.cminlat, pTable.cminlong);
                if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR || mPFD.Eloss == PFDloss.TERRAIN)
                {
                    fReport.Write("{0,7:F1} {1,12} {2,13} ", pTable.dmaxdist, pTable.cmaxlat, pTable.cmaxlong);
                }

                nInd++;
            }

            /*	Close out the file */
            fReport.Write("\r\n\f");
            fReport.Close();
        }

        /// <summary>
        /// This method collates and writes a text report using 'comma-separated values' format (.CSV) and
        /// sends it to the user as an email attachment.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mPFD"></param>
        /// <param name="mtSiteStr"></param>
        /// <param name="faxref"></param>
        /// <param name="suAntStr"></param>
        /// <param name="mApTable"></param>
        private static void WriteToFile_CSV(string filePath, PFD mPFD, MtSiteStr mtSiteStr, string faxref,
                                            SuAntStr suAntStr, List<TtabRow> mApTable)
        {
            string cLosses;
            string cCalc;

            /*	There are rows on the form.  This means that the variables have
            *		valid values.  Get the file name */
            string mPathWithFileExtn = filePath;

            //	Open the file as a TextWriter.
            TextWriter fReport = null;
            try { fReport = new StreamWriter(mPathWithFileExtn); }
            catch (Exception e)
            {
                Log2.e("\r\n\r\nPFDcont.Main(): ERROR: new StreamWriter(): Exception: " + e.Message);
                Console.Write("\r\n*** Can't open {0}.\r\n", mPathWithFileExtn);
                Application.ExitQuietly(96);
            }

            if (mPFD.Eloss == PFDloss.SPHERICAL)
            {
                cLosses = "Spherical";
            }
            else if (mPFD.Eloss == PFDloss.TERRAIN)
            {
                cLosses = "Terrain";
            }
            else
            {
                /*	Unknown loss type */
                cLosses = "Unknown";
            }

            /*	Convert the calculation enumeration to strings */
            if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
            {
                cCalc = "PFDContour";
            }
            else
            {
                cCalc = "Coverage";
            }

            /*	Write the header  */
            fReport.Write("\"call1\",\"location\",\"latitude\",\"longitude\",\"grnd\",\"TxHt\",\"RxHt\"\r\n\"{0}\",\"{1}\",{2,10:F6},{3,11:F6},{4,6:F1},{5,5:F1},{6,5:F1}\r\n",
                            mPFD.Call1, mPFD.Location, mPFD.Latitude, mPFD.Longitude, mPFD.Grnd, mPFD.TxHeight, mPFD.RxHeight);

            fReport.Write("\"antenna\",\"antenna xref\",\"azimuth\",\"fsl\",\"tx power\",\"Bandwidth\",\"Frequency\",\"Loss Type\", \"Calc Type\"\r\n\"{0}\",\"{1}\",{2,6:F1},{3,6:F2},{4,7:F2},{5,7:F2},{6,11:F3},\"{7}\",\"{8}\"\r\n",
                            mPFD.AnteCode, faxref, mPFD.TxAzim, mPFD.Fsl, mPFD.TxPwr, mPFD.Bandwidth, mPFD.Frequency, cLosses, cCalc);

            if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
            {
                /*	Print the headings */
                fReport.Write("\"Azimuth\", \"Min Dist\", \"Min Latitude\", \"Min Longitude\", \"Max Dist\", \"Max Latitude\", \"Max Longitude\"");
            }
            else
            {
                fReport.Write("\"Azimuth\", \"50% Dist\", \"50% Latitude\", \"50% Longitude\"");

                if (mPFD.Eloss == PFDloss.TERRAIN)
                {
                    fReport.Write(",\"10% Dist\", \"10% Latitude\", \"10% Longitude\"");
                }
            }

            /*	Unload the table into the report */

            foreach (TtabRow pTable in mApTable)
            {
                {
                    /*	Print a line to the report */
                    fReport.Write("\r\n{0,6:F2},{1,6:F1},{2,11:F6},{3,12:F6}",
                                      pTable.dAz, pTable.dmindist, pTable.dminlat, pTable.dminlong);

                    if ((mPFD.CalcMode == PFDcalcMode.PFDCONTOUR) || (mPFD.Eloss == PFDloss.TERRAIN))
                    {
                        fReport.Write(",{0,6:F1},{1,11:F6},{2,12:F6}",
                                         pTable.dmaxdist, pTable.dmaxlat, pTable.dmaxlong);
                    }
                }
            }

            /*	Close out the file */
            fReport.Write("\r\n");
            fReport.Close();
        }

        /// <summary>
        /// This method collates and writes the qty. 2 types of MapInfo Data Interchange Format files (.MID and .MIF); both
        /// data files are sent to the user as (individual) email attachments.
        /// </summary>
        /// <param name="mPFD"></param>
        /// <param name="mtSiteStr"></param>
        /// <param name="faxref"></param>
        /// <param name="suAntStr"></param>
        /// <param name="mApTable"></param>
        private static void WriteToFiles_MID_MIF(PFD mPFD, MtSiteStr mtSiteStr, string faxref,
                                                SuAntStr suAntStr, List<TtabRow> mApTable)
        {
            int nCols;              /*	Number of columns in the output */
            string mPathWithFileExtn;

            int numRows = mApTable.Count;

            /*	There are rows on the form.  This means that the variables have
                    valid values.  Get the file name.
                    The first point is also the last */

            /*	Calculate the number of columns we use.  This will be 2 except for
            *		the case of coverage contours using a spherical earth */
            if (Calc.IsCoverage(mPFD.CalcMode) && mPFD.Eloss == PFDloss.SPHERICAL)
            {
                nCols = 1;
            }
            else
            {
                nCols = 2;
            }

            mPathWithFileExtn = mPFD.BaseReportPath + ".mif";

            //	Open the .mif file as a TextWriter.
            TextWriter fReportMIF = null;

            try { fReportMIF = new StreamWriter(mPathWithFileExtn); }
            catch (Exception e)
            {
                Log2.e("\n\nPFDcont.Main(): ERROR: new StreamWriter(): Exception: " + e.Message);
                Console.Write("\r\n*** Can't open .mif file {0}.\r\n", mPathWithFileExtn);
                Application.ExitQuietly(96);
            }

            //	Open the .mid file as a TextWriter.
            mPathWithFileExtn = mPFD.BaseReportPath + ".mid";

            TextWriter fReportMID = null;

            try { fReportMID = new StreamWriter(mPathWithFileExtn); }
            catch (Exception e)
            {
                Log2.e("\n\nPFDcont.Main(): ERROR: new StreamWriter(): Exception: " + e.Message);
                Console.Write("\r\n*** Can't open .mid file {0}.\r\n", mPathWithFileExtn);
                Application.ExitQuietly(96);
            }

            /*	Write the header for the .MIF file */
            fReportMIF.Write("VERSION 300\r\nCharset \"WindowsLatin1\"\r\nDelimiter \",\"\r\nCOLUMNS {0}\r\nDescription   char(30)\r\nPowerLevel    float\r\n\r\nDATA", nCols);
            /*	We now go through the table twice, once for the maximum pfd level
            *		and the second for the minimum.  In each pass we write a record
            *		to the .MID file and the region, as a line to the .MIF file.
            *
            *		The header depends on the calcmode.
            */
            if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
            {
                fReportMID.Write("\"Minimum Power Flux Density\",{0:F6}", mPFD.MinPFDlevel);
            }
            else
            {
                if (mPFD.Eloss == PFDloss.SPHERICAL)
                {
                    fReportMID.Write("\"Spherical Coverage\", {0:F6}", mPFD.MinRxPwr);
                }
                else
                {
                    fReportMID.Write("\"50% Losses\", {0:F6}", mPFD.MinRxPwr);
                }
            }

            fReportMIF.Write("\r\nPLINE\r\n {0}", numRows + 1); // + 1 for the repeat of first row.


            bool IsFirst = true;

            foreach (TtabRow pTable in mApTable)
            {
                /*	Print a line to the file */
                if (IsFirst)
                {
                    mPFD.Latitude = pTable.dminlat;
                    mPFD.Longitude = pTable.dminlong;
                    IsFirst = false;
                }
                fReportMIF.Write("\r\n{0:F6} {1:F6}", -(pTable.dminlong), pTable.dminlat);
            }

            fReportMIF.Write("\r\n{0:F6} {1:F6}", -mPFD.Longitude, mPFD.Latitude); /* Last pts same as first */

            if (nCols > 1)
            {
                /*	Now do the maximum power flux density contour. */
                if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
                {
                    fReportMID.Write("\r\n\"Maximum Power Flux Density\",{0:F6}", mPFD.MaxPFDlevel);
                }
                else
                {
                    fReportMID.Write("\r\n\"10% Losses\", {0:F6}", mPFD.MinRxPwr);
                }
                fReportMIF.Write("\r\nPLINE\r\n {0}", numRows + 1); // + 1 for the repeat of first row.

                IsFirst = true;

                foreach (TtabRow pTable in mApTable)
                {
                    /*	Print a line to the file */
                    if (IsFirst)
                    {
                        mPFD.Latitude = pTable.dmaxlat;
                        mPFD.Longitude = pTable.dmaxlong;
                        IsFirst = false;
                    }

                    fReportMIF.Write("\r\n{0:F6} {1:F6}", -(pTable.dmaxlong), pTable.dmaxlat);
                }

                fReportMIF.Write("\r\n{0:F6} {1:F6}", -mPFD.Longitude, mPFD.Latitude); /* Last pts same as first */
            }

            /*	Close out the files */
            fReportMIF.Write("\r\n");
            fReportMIF.Close();

            fReportMID.Write("\r\n");
            fReportMID.Close();

        }

        /// <summary>
        /// This method collates and writes a text report in the form of a XML-based KML file and
        /// sends it to the user as an email attachment; KML files can easily be imported in Google
        /// Maps and Google Earth.
        /// </summary>
        /// <param name="mPFD"></param>
        /// <param name="mtSiteStr"></param>
        /// <param name="faxref"></param>
        /// <param name="suAntStr"></param>
        /// <param name="mApTable"></param>
        private static void WriteToFile_KML(PFD mPFD, MtSiteStr mtSiteStr, string faxref,
                                                SuAntStr suAntStr, List<TtabRow> mApTable)
        {
            string mPathWithFileExtn;

            int numRows = mApTable.Count;

            /*	There are rows on the form.  This means that the variables have
                    valid values.  Get the file name.
                    The first point is also the last */

            mPathWithFileExtn = mPFD.BaseReportPath + ".kml";

            //	Open the .mif file as a TextWriter.
            TextWriter fReportKML = null;

            try { fReportKML = new StreamWriter(mPathWithFileExtn); }
            catch (Exception e)
            {
                Log2.e("\n\nPFDcont.WriteToFile_KML(): ERROR: new StreamWriter(): Exception: " + e.Message);
                Console.Write("\r\n*** Can't open .kml file {0}.\r\n", mPathWithFileExtn);
                Application.ExitQuietly(96);
            }

            // The KML_TEMPLATE string begins with a blank line that does not comform to the
            // KML file specification. We need to remove the initial "\r\n" characters.
            string xml = KML_TEMPLATE.Substring(2, KML_TEMPLATE.Length - 2);

            // Relace _SchemaName.
            xml = xml.Replace("_SchemaName", mPFD.BaseFileName);

            // Relace _SchemaID.
            xml = xml.Replace("_SchemaID", mPFD.BaseFileName);

            // Relace _FolderName.
            xml = xml.Replace("_FolderName", mPFD.BaseFileName);

            // Relace _PlacemarkNameMin.
            xml = xml.Replace("_PlacemarkNameMin", mPFD.MinPFDlevel.ToString());

            // Relace _PlacemarkNameMax.
            xml = xml.Replace("_PlacemarkNameMax", mPFD.MaxPFDlevel.ToString());

            // Minimum Power Flux Density Long,Lat dyads.
            StringBuilder sb = new StringBuilder();

            /*	Calculate the number of columns we use.  This will be 2 except for
             *  the case of coverage contours using a spherical earth */
            int nCols = 0;
            if (Calc.IsCoverage(mPFD.CalcMode) && mPFD.Eloss == PFDloss.SPHERICAL)
            {
                nCols = 1;
            }
            else
            {
                nCols = 2;
            }

            if (nCols > 1)
            {
                /*	Now do the minimum power flux density contour. */
                bool IsFirst = true;

                foreach (TtabRow pTable in mApTable)
                {
                    if (IsFirst)
                    {
                        mPFD.Latitude = pTable.dminlat;
                        mPFD.Longitude = pTable.dminlong;
                        IsFirst = false;
                    }

                    sb.Append(String.Format("{0:F6},{1:F6} ", -pTable.dminlong, pTable.dminlat));
                }

                // Last dyad must be same as the first.
                sb.Append(String.Format("{0:F6},{1:F6} ", -mPFD.Longitude, mPFD.Latitude));
            }

            xml = xml.Replace("_MinimaXY", sb.ToString());

            // Maximum Power Flux Density Long,Lat dyads.
            sb = new StringBuilder();

            /*	Calculate the number of columns we use.  This will be 2 except for
             *  the case of coverage contours using a spherical earth */
            if (nCols > 1)
            {
                /*	Now do the maximum power flux density contour. */
                bool IsFirst = true;

                foreach (TtabRow pTable in mApTable)
                {
                    if (IsFirst)
                    {
                        mPFD.Latitude = pTable.dmaxlat;
                        mPFD.Longitude = pTable.dmaxlong;
                        IsFirst = false;
                    }

                    sb.Append(String.Format("{0:F6},{1:F6} ", -pTable.dmaxlong, pTable.dmaxlat));
                }

                // Last dyad must be same as the first.
                sb.Append(String.Format("{0:F6},{1:F6} ", -mPFD.Longitude, mPFD.Latitude));
            }

            xml = xml.Replace("_MaximaXY", sb.ToString());

            // Write the xml string to the KML file.
            fReportKML.Write(xml);

#if false

            /*	Write the header for the .MIF file */
            fReportKML.Write("VERSION 300\r\nCharset \"WindowsLatin1\"\r\nDelimiter \",\"\r\nCOLUMNS {0}\r\nDescription   char(30)\r\nPowerLevel    float\r\n\r\nDATA", nCols);
            /*	We now go through the table twice, once for the maximum pfd level
            *		and the second for the minimum.  In each pass we write a record
            *		to the .MID file and the region, as a line to the .MIF file.
            *
            *		The header depends on the calcmode.
            */
            if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
            {
                fReportMID.Write("\"Minimum Power Flux Density\",{0:F6}", mPFD.MinPFDlevel);
            }
            else
            {
                if (mPFD.Eloss == PFDloss.SPHERICAL)
                {
                    fReportMID.Write("\"Spherical Coverage\", {0:F6}", mPFD.MinRxPwr);
                }
                else
                {
                    fReportMID.Write("\"50% Losses\", {0:F6}", mPFD.MinRxPwr);
                }
            }

            fReportKML.Write("\r\nPLINE\r\n {0}", numRows + 1); // + 1 for the repeat of first row.


            bool IsFirst = true;

            foreach (TtabRow pTable in mApTable)
            {
                /*	Print a line to the file */
                if (IsFirst)
                {
                    mPFD.Latitude = pTable.dminlat;
                    mPFD.Longitude = pTable.dminlong;
                    IsFirst = false;
                }
                fReportKML.Write("\r\n{0:F6} {1:F6}", -(pTable.dminlong), pTable.dminlat);
            }

            fReportKML.Write("\r\n{0:F6} {1:F6}", -mPFD.Longitude, mPFD.Latitude); /* Last pts same as first */

            if (nCols > 1)
            {
                /*	Now do the maximum power flux density contour. */
                if (mPFD.CalcMode == PFDcalcMode.PFDCONTOUR)
                {
                    fReportMID.Write("\r\n\"Maximum Power Flux Density\",{0:F6}", mPFD.MaxPFDlevel);
                }
                else
                {
                    fReportMID.Write("\r\n\"10% Losses\", {0:F6}", mPFD.MinRxPwr);
                }
                fReportKML.Write("\r\nPLINE\r\n {0}", numRows + 1); // + 1 for the repeat of first row.

                IsFirst = true;

                foreach (TtabRow pTable in mApTable)
                {
                    /*	Print a line to the file */
                    if (IsFirst)
                    {
                        mPFD.Latitude = pTable.dmaxlat;
                        mPFD.Longitude = pTable.dmaxlong;
                        IsFirst = false;
                    }

                    fReportKML.Write("\r\n{0:F6} {1:F6}", -(pTable.dmaxlong), pTable.dmaxlat);
                }

                fReportKML.Write("\r\n{0:F6} {1:F6}", -mPFD.Longitude, mPFD.Latitude); /* Last pts same as first */
            }
#endif
            /*	Close out the files */
            fReportKML.Write("\r\n");
            fReportKML.Close();
        }

        /// <summary>
        /// This method causes an email to be sent that comprises body text (read from a prescribed text file) and a path
        /// to a file to be attached to the email; it calls the method MicsEmail.SendSql() to actually perform the SMTP send().
        /// </summary>
        /// <param name="emailAddress"> - prescribed email address to be sent to.</param>
        /// <param name="subject"> - prescribed subject line of email.</param>
        /// <param name="pathToBodyFile"> - path to a file containing the email's body text.</param>
        /// <param name="pathToAttachedFile"> - path to a file to be attached to the email.</param>
        /// <returns></returns>
        public static int SendEmail(string emailAddress,        /*	Email address */
                                    string subject,             /*	Subject Line 	*/
                                    string pathToBodyFile,      /*	Body */
                                    string pathToAttachedFile)  /*	Attached file */
        {
            //...Log2.v("\nProducts.SendEmail(): Entry");
            //...Log2.v("\nemailAddress       = " + emailAddress);
            //...Log2.v("\nsubject            = " + subject);
            //...Log2.v("\npathToBodyFile     = " + pathToBodyFile);
            //...Log2.v("\npathToAttachedFile = " + pathToAttachedFile);

            int nRet = Constant.SUCCESS;
            string errMsg;
            string bodyText = "See Attachement ...";

            if (!pathToBodyFile.Equals("--"))
            {
                try
                {
                    bodyText = File.ReadAllText(pathToBodyFile);
                }
                catch (Exception e)
                {
                    errMsg = "ERROR: call to File.ReadAllText() threw an exception for pathToBodyFile = " + pathToBodyFile;
                    Log2.e("\n\nProducts.SendEmail(): ERROR: " + e.Message);
                    Log2.e("\n" + e.StackTrace);
                    return Constant.FAILURE;
                }
            }

            nRet = MicsEmail.SendSql(emailAddress, subject, bodyText, pathToAttachedFile, out errMsg);

            if (nRet != Constant.SUCCESS)
            {
                Log2.e("\n\nProducts.SendEmail(): ERROR: call to MicsEmail.SendSql() FAILED, retVal = " + nRet);
                TsipQ.WriteToTsipLog("\nProducts.SendEmail(): ERROR: call to MicsEmail.SendSql() FAILED, retVal = " + nRet);
            }

            //...Log2.v("\nProducts.SendEmail(): Exit, return code = " + nRet);
            return (nRet);
        }


    }
}

```
