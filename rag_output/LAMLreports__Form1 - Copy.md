# Documented File: Form1 - Copy.cs
**Repository Path:** `LAMLreports\Form1 - Copy.cs`
**Primary Layer:** `LAMLreports`
**Namespace:** `ExcelApp`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using Microsoft.VisualBasic;
using _Utillib;
using _NewLib;
using _Configuration;
using _DataStructures;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace ExcelApp
{
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;

    public partial class Form1 : Form
    {
        private const string EXCEL_TEMPLATE_FILE_PATH = @"D:\users\ahulme\Missing and Phantoms\Report Template.xlsx";
        private const string TARGET_DIR = @"D:\users\ahulme\Missing and Phantoms\";
        private const string DATE_TAG = "20230526";
        private const string TAFL_LINK_ANALYSIS_TABLE_NAME = "hulme.TaflLinkAnalysis";
        private const string MDB_LINK_ANALYSIS_TABLE_NAME = "hulme.mdbLinkLicenseAnalysisReport";

        private static List<string> mOperNames = new List<string>()
        {
            /*"ABCCOM",
            "ALIANT",
            "BCHY",
            "BELL",
            "BMCE",
            "BRAGG",
            "DND",
            "GLW",
            "HYONE",
            "HYQU",
            "MTS",
            "NAVI",
            "NTTEL",
            "NWT",
            "ONT",*/
            "RCTL",
            "SHAW",
            "STEL",
            "TBAY",
            "TEKSAV",
            "TERAGO",
            "TLUSAB",
            "TLUSBC",
            "TLUSMC",
            "TLUSQC",
            "VDTR",
            "WIREIE",
            "XCI",
            "ZAYO" 
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, System.EventArgs e)
        {
            Excel.Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheetT2M;
            Excel._Worksheet oSheetM2T;

            SQLLEN[] nullInds = new SQLLEN[TaflMdb.NUM_COLUMNS];

            double progressPC = 0.0;
            int numRecords = 0;
            string sqlTableName = "";

            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                GetEnvVariablesForUtConnect();

                // Prescribe the database name.
                Info.DbName = "micsdev";

                // Establish an FCSA user session with the database.
                int rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    Console.Write("TBD -- Can't connect to database {0}.\r\n", Info.DbName);

                    _NewLib.Application.ExitQuietly(11);
                }

                //Start Excel and get an Application object.
                oXL = new Excel.Application();
                oXL.Visible = true;

                myTextBox.AppendText("UtConnect() succeeded ...");

                progressBar1.Visible = true;
                progressBar1.Minimum = 1;
                progressBar1.Value = 1;
                progressBar1.Step = 1;

                foreach (string operName in mOperNames)
                {
                    myTextBox.AppendText(String.Format("\r\n\r\n=========================== {0}", operName));
                    myTextBox.AppendText("\r\nOpening Excel template workbook ...");   // \r\n is required to force a new line.
                    tProgress.Text = "";
                    progressBar1.Maximum = numRecords;

                    //oWB = (Excel._Workbook)oXL.Workbooks.Open(@"D:\users\ahulme\ISED TAFL data\ColumnTitles.xlsx");
                    oWB = (Excel._Workbook)oXL.Workbooks.Open(EXCEL_TEMPLATE_FILE_PATH);

                    //==================================================
                    // Populate the Excel sheet named "Missing-links".
                    //==================================================

                    myTextBox.AppendText("\r\nStarting population of Missing-Links sheet ...");

                    //oSheet = (Excel._Worksheet)oWB.ActiveSheet;
                    //oSheet = (Excel._Worksheet)oWB.Sheets["Licensed-links"];
                    oSheetT2M = (Excel._Worksheet)oWB.Sheets["Missing-links"];

                    //MessageBox.Show(oSheetT2M.Name, "Hello");

                    //Add table headers going cell by cell.
                    //oSheetT2M.Cells[10, 1] = "First Name";
                    //oSheetT2M.Cells[10, 2] = "Last Name";
                    //oSheetT2M.Cells[10, 3] = "Full Name";
                    //oSheetT2M.Cells[10, 4] = "Salary";

                    //int count = Ssutil.DbCountRows("hulme.IsedTafl_20230227", "");
                    //MessageBox.Show("DynTaflMdb.Select(): count = " + count, "hello");

                    // Construct the SQL query 'WHERE' clause for the operator SELECT.
                    string whereClause = String.Format("{0} = '{1}'", "licenseeName_oper", operName);

                    // Get the SQL table record count.
                    numRecords = Ssutil.DbCountRows(TAFL_LINK_ANALYSIS_TABLE_NAME, whereClause);
                    progressBar1.Maximum = numRecords;

                    // Do the SELECT for this operator.
                    int handle = DynTaflMdb.Select("hulme.TaflLinkAnalysis", whereClause, "taflKeyField");

                    int offset = 0;
                    int START_ROW = 10;

                    int nRet;
                    TaflMdb taflMdb = null;

                    while ((nRet = DynTaflMdb.Fetch(handle, out taflMdb, out nullInds)) == Constant.SUCCESS)
                    {
                        oSheetT2M.Cells[START_ROW + offset, 2] = (nullInds[TaflMdb.OPER] == Constant.DB_NULL) ? "NULL" : taflMdb.oper;
                        oSheetT2M.Cells[START_ROW + offset, 3] = (nullInds[TaflMdb.PROV] == Constant.DB_NULL) ? "NULL" : taflMdb.prov;
                        oSheetT2M.Cells[START_ROW + offset, 4] = (nullInds[TaflMdb.CALL1] == Constant.DB_NULL) ? "NULL" : taflMdb.call1.Replace("=", "'=");
                        oSheetT2M.Cells[START_ROW + offset, 5] = (nullInds[TaflMdb.CALL2] == Constant.DB_NULL) ? "NULL" : taflMdb.call2.Replace("=", "'=");
                        oSheetT2M.Cells[START_ROW + offset, 6] = (nullInds[TaflMdb.BNDCDE] == Constant.DB_NULL) ? "NULL" : taflMdb.bndcde;
                        oSheetT2M.Cells[START_ROW + offset, 7] = (nullInds[TaflMdb.CHID] == Constant.DB_NULL) ? "NULL" : taflMdb.chid;
                        oSheetT2M.Cells[START_ROW + offset, 8] = (nullInds[TaflMdb.ANUM] == Constant.DB_NULL) ? "NULL" : taflMdb.anum.ToString();
                        oSheetT2M.Cells[START_ROW + offset, 9] = (nullInds[TaflMdb.MDBLAT] == Constant.DB_NULL) ? "NULL" : taflMdb.mdbLat.ToString("F6");
                        oSheetT2M.Cells[START_ROW + offset, 10] = (nullInds[TaflMdb.MDBLNG] == Constant.DB_NULL) ? "NULL" : taflMdb.mdbLng.ToString("F6");
                        oSheetT2M.Cells[START_ROW + offset, 11] = (nullInds[TaflMdb.AZMTH] == Constant.DB_NULL) ? "NULL" : taflMdb.azmth.ToString("F1");
                        oSheetT2M.Cells[START_ROW + offset, 12] = (nullInds[TaflMdb.FREQTX] == Constant.DB_NULL) ? "NULL" : taflMdb.freqtx.ToString();
                        oSheetT2M.Cells[START_ROW + offset, 13] = (nullInds[TaflMdb.FREQRX] == Constant.DB_NULL) ? "NULL" : taflMdb.freqrx.ToString();
                        oSheetT2M.Cells[START_ROW + offset, 14] = (nullInds[TaflMdb.CONFIDENCE] == Constant.DB_NULL) ? "NULL" : taflMdb.confidence;
                        oSheetT2M.Cells[START_ROW + offset, 15] = (nullInds[TaflMdb.TXRX_TAFL] == Constant.DB_NULL) ? "NULL" : taflMdb.txrx_tafl;
                        oSheetT2M.Cells[START_ROW + offset, 16] = (nullInds[TaflMdb.AUTHORIZATIONNUMBER] == Constant.DB_NULL) ? "NULL" : taflMdb.authorizationNumber;
                        oSheetT2M.Cells[START_ROW + offset, 17] = (nullInds[TaflMdb.CALLSIGN] == Constant.DB_NULL) ? "NULL" : taflMdb.callsign;
                        oSheetT2M.Cells[START_ROW + offset, 18] = (nullInds[TaflMdb.INSERVICEDATE] == Constant.DB_NULL) ? "NULL" : taflMdb.inserviceDate;
                        oSheetT2M.Cells[START_ROW + offset, 19] = (nullInds[TaflMdb.ACCOUNTNUMBER] == Constant.DB_NULL) ? "NULL" : taflMdb.accountNumber;
                        oSheetT2M.Cells[START_ROW + offset, 20] = (nullInds[TaflMdb.LICENSEENAME] == Constant.DB_NULL) ? "NULL" : taflMdb.licenseeName;
                        oSheetT2M.Cells[START_ROW + offset, 21] = (nullInds[TaflMdb.LICENSEENAME_OPER] == Constant.DB_NULL) ? "NULL" : taflMdb.licenseeName_oper;
                        oSheetT2M.Cells[START_ROW + offset, 22] = (nullInds[TaflMdb.LAT_TAFL] == Constant.DB_NULL) ? "NULL" : taflMdb.lat_tafl.ToString("F6");
                        oSheetT2M.Cells[START_ROW + offset, 23] = (nullInds[TaflMdb.LNG_TAFL] == Constant.DB_NULL) ? "NULL" : taflMdb.lng_tafl.ToString("F6");
                        oSheetT2M.Cells[START_ROW + offset, 24] = (nullInds[TaflMdb.AZMTH_TAFL] == Constant.DB_NULL) ? "NULL" : taflMdb.azmth_tafl.ToString("F1");
                        oSheetT2M.Cells[START_ROW + offset, 25] = (nullInds[TaflMdb.FREQTXRX_TAFL] == Constant.DB_NULL) ? "NULL" : taflMdb.freqtxrx_tafl.ToString();
                        oSheetT2M.Cells[START_ROW + offset, 26] = (nullInds[TaflMdb.D_METERS] == Constant.DB_NULL) ? "NULL" : taflMdb.d_meters.ToString("F0");
                        oSheetT2M.Cells[START_ROW + offset, 27] = (nullInds[TaflMdb.D_DEGREES] == Constant.DB_NULL) ? "NULL" : taflMdb.d_degrees.ToString("F1");
                        oSheetT2M.Cells[START_ROW + offset, 28] = (nullInds[TaflMdb.D_MHZ] == Constant.DB_NULL) ? "NULL" : taflMdb.d_MHz.ToString();
                        oSheetT2M.Cells[START_ROW + offset, 29] = (nullInds[TaflMdb.FOM] == Constant.DB_NULL) ? "NULL" : taflMdb.fom.ToString();
                        oSheetT2M.Cells[START_ROW + offset, 30] = (nullInds[TaflMdb.LAF] == Constant.DB_NULL) ? "NULL" : taflMdb.laf;
                        oSheetT2M.Cells[START_ROW + offset, 31] = (nullInds[TaflMdb.TAFLKEYFIELD] == Constant.DB_NULL) ? "NULL" : taflMdb.taflKeyField.ToString();

                        offset++;

                        progressPC = (100.0 * offset) / numRecords;

                        tProgress.Text = String.Format("{0:F1} %", progressPC);

                        progressBar1.PerformStep();
                    }

                    DynTaflMdb.Close(handle);

                    //==================================================
                    // Populate the Excel sheet named "Licensed-links".
                    //==================================================

                    myTextBox.AppendText("\r\nStarting population of Licensed-links sheet ...");

                    oSheetM2T = (Excel._Worksheet)oWB.Sheets["Licensed-links"];

                    whereClause = String.Format("{0} = '{1}'", "oper", operName);
                    handle = DynMdbTafl.Select("hulme.mdbLinkLicenseAnalysisReport", whereClause, "taflKeyField");

                    offset = 0;
                    START_ROW = 10;

                    MdbTafl mdbTafl = null;

                    while ((nRet = DynMdbTafl.Fetch(handle, out mdbTafl, out nullInds)) == Constant.SUCCESS)
                    {
                        oSheetM2T.Cells[START_ROW + offset, 2] = (nullInds[MdbTafl.OPER] == Constant.DB_NULL) ? "NULL" : mdbTafl.oper;
                        oSheetM2T.Cells[START_ROW + offset, 3] = (nullInds[MdbTafl.PROV] == Constant.DB_NULL) ? "NULL" : mdbTafl.prov;
                        oSheetM2T.Cells[START_ROW + offset, 4] = (nullInds[MdbTafl.CALL1] == Constant.DB_NULL) ? "NULL" : mdbTafl.call1.Replace("=", "'=");
                        oSheetM2T.Cells[START_ROW + offset, 5] = (nullInds[MdbTafl.CALL2] == Constant.DB_NULL) ? "NULL" : mdbTafl.call2.Replace("=", "'=");
                        oSheetM2T.Cells[START_ROW + offset, 6] = (nullInds[MdbTafl.BNDCDE] == Constant.DB_NULL) ? "NULL" : mdbTafl.bndcde;
                        oSheetM2T.Cells[START_ROW + offset, 7] = (nullInds[MdbTafl.CHID] == Constant.DB_NULL) ? "NULL" : mdbTafl.chid;
                        oSheetM2T.Cells[START_ROW + offset, 8] = (nullInds[MdbTafl.ANUM] == Constant.DB_NULL) ? "NULL" : mdbTafl.anum.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 9] = (nullInds[MdbTafl.LATSTR] == Constant.DB_NULL) ? "NULL" : mdbTafl.latStr.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 10] = (nullInds[MdbTafl.LNGSTR] == Constant.DB_NULL) ? "NULL" : mdbTafl.lngStr.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 11] = (nullInds[MdbTafl.AZMTHSTR] == Constant.DB_NULL) ? "NULL" : mdbTafl.azmthStr.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 12] = (nullInds[MdbTafl.FREQTX] == Constant.DB_NULL) ? "NULL" : mdbTafl.freqtx.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 13] = (nullInds[MdbTafl.FREQRX] == Constant.DB_NULL) ? "NULL" : mdbTafl.freqrx.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 14] = (nullInds[MdbTafl.CONFIDENCE] == Constant.DB_NULL) ? "NULL" : mdbTafl.confidence;
                        oSheetM2T.Cells[START_ROW + offset, 15] = (nullInds[MdbTafl.TXRX_TAFL] == Constant.DB_NULL) ? "NULL" : mdbTafl.txrx_tafl;
                        oSheetM2T.Cells[START_ROW + offset, 16] = (nullInds[MdbTafl.AUTHORIZATIONNUMBER] == Constant.DB_NULL) ? "NULL" : mdbTafl.authorizationNumber;
                        oSheetM2T.Cells[START_ROW + offset, 17] = (nullInds[MdbTafl.CALLSIGN] == Constant.DB_NULL) ? "NULL" : mdbTafl.callsign;
                        oSheetM2T.Cells[START_ROW + offset, 18] = (nullInds[MdbTafl.INSERVICEDATE] == Constant.DB_NULL) ? "NULL" : mdbTafl.inserviceDate;
                        oSheetM2T.Cells[START_ROW + offset, 19] = (nullInds[MdbTafl.ACCOUNTNUMBER] == Constant.DB_NULL) ? "NULL" : mdbTafl.accountNumber;
                        oSheetM2T.Cells[START_ROW + offset, 20] = (nullInds[MdbTafl.LICENSEENAME] == Constant.DB_NULL) ? "NULL" : mdbTafl.licenseeName;
                        oSheetM2T.Cells[START_ROW + offset, 21] = (nullInds[MdbTafl.D_METERS] == Constant.DB_NULL) ? "NULL" : mdbTafl.d_meters.ToString("F0");
                        oSheetM2T.Cells[START_ROW + offset, 22] = (nullInds[MdbTafl.D_DEGREES] == Constant.DB_NULL) ? "NULL" : mdbTafl.d_degrees.ToString("F1");
                        oSheetM2T.Cells[START_ROW + offset, 23] = (nullInds[MdbTafl.D_MHZ] == Constant.DB_NULL) ? "NULL" : mdbTafl.d_MHz.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 24] = (nullInds[MdbTafl.FOM] == Constant.DB_NULL) ? "NULL" : mdbTafl.fom.ToString();
                        oSheetM2T.Cells[START_ROW + offset, 25] = (nullInds[MdbTafl.LAF] == Constant.DB_NULL) ? "NULL" : mdbTafl.laf;
                        oSheetM2T.Cells[START_ROW + offset, 26] = (nullInds[MdbTafl.TAFLKEYFIELD] == Constant.DB_NULL) ? "NULL" : mdbTafl.taflKeyField.ToString();

                        offset++;
                    }



                    // Save the Excel workbook to a prescribed file path.
                    string targetFilePath = String.Format("{0}{1}_{2}.xlsx", TARGET_DIR, operName, DATE_TAG);

                    myTextBox.AppendText("\r\nSaveAs Excel file to " + targetFilePath + " ...");

                    // Delete any existing file to stop Excel popping up a 'what do I do?" dialog.
                    File.Delete(targetFilePath);

                    oWB.SaveAs(targetFilePath, 51);

                    oSheetM2T.Delete();
                    oSheetT2M.Delete();

                    oWB.Close(false);

                    DynTaflMdb.Close(handle);

                } // end of loop over operator names.

                oXL.Quit();

                // Terminate the FCSA user's DB session.
                Ssutil.UtDisconnect(1);

                myTextBox.AppendText("\r\n\r\nUtDisconnect() succeeded ...");

                timer.Stop();
                TimeSpan timeTaken = timer.Elapsed;
                string elapsedTime = timeTaken.ToString(@"dd\.hh\:mm\:ss");

                myTextBox.AppendText("\r\n\r\nElapsed time = " + elapsedTime);

                bExit.Enabled = true;

                /*

                */
                //int index = 0;

                //Excel.Range NamedRange1 =
                //    Globals.Sheet1.Controls.AddNamedRange(
                //    Globals.Sheet1.Range["A1"], "NamedRange1");

                //foreach (Excel.Worksheet displayWorksheet in Globals.ThisWorkbook.Worksheets)
                //{
                //    NamedRange1.Offset[index, 0].Value2 = displayWorksheet.Name;
                //    index++;
                //}

#if false
                //Start Excel and get Application object.
                oXL = new Excel.Application();
                oXL.Visible = true;

                //Get a new workbook.
                oWB = (Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
                oSheet = (Excel._Worksheet)oWB.ActiveSheet;

                //Add table headers going cell by cell.
                oSheet.Cells[1, 1] = "First Name";
                oSheet.Cells[1, 2] = "Last Name";
                oSheet.Cells[1, 3] = "Full Name";
                oSheet.Cells[1, 4] = "Salary";

                //Format A1:D1 as bold, vertical alignment = center.
                oSheet.get_Range("A1", "D1").Font.Bold = true;
                oSheet.get_Range("A1", "D1").VerticalAlignment =
                Excel.XlVAlign.xlVAlignCenter;

                // Create an array to multiple values at once.
                string[,] saNames = new string[5, 2];

                saNames[0, 0] = "John";
                saNames[0, 1] = "Smith";
                saNames[1, 0] = "Tom";
                saNames[1, 1] = "Brown";
                saNames[2, 0] = "Sue";
                saNames[2, 1] = "Thomas";
                saNames[3, 0] = "Jane";
                saNames[3, 1] = "Jones";
                saNames[4, 0] = "Adam";
                saNames[4, 1] = "Johnson";

                //Fill A2:B6 with an array of values (First and Last Names).
                oSheet.get_Range("A2", "B6").Value2 = saNames;

                //Fill C2:C6 with a relative formula (=A2 & " " & B2).
                oRng = oSheet.get_Range("C2", "C6");
                oRng.Formula = "=A2 & \" \" & B2";

                //Fill D2:D6 with a formula(=RAND()*100000) and apply format.
                oRng = oSheet.get_Range("D2", "D6");
                oRng.Formula = "=RAND()*100000";
                oRng.NumberFormat = "$0.00";

                //AutoFit columns A:D.
                oRng = oSheet.get_Range("A1", "D1");
                oRng.EntireColumn.AutoFit();

                //Manipulate a variable number of columns for Quarterly Sales Data.
                DisplayQuarterlySales(oSheet);

                //Make sure Excel is visible and give the user control
                //of Microsoft Excel's lifetime.
                oXL.Visible = true;
                oXL.UserControl = true;

#endif
            }
            catch (Exception exception)
            {
                String errMsg;
                errMsg = "Error: ";
                errMsg += String.Format("\n{0}", exception.Message);
                errMsg += String.Format("\n{0}", exception.StackTrace);
                Log2.e(errMsg);

                MessageBox.Show(errMsg, "Error");
            }
        }

        private void DisplayQuarterlySales(Excel._Worksheet oWS)
        {
            Excel._Workbook oWB;
            Excel.Series oSeries;
            Excel.Range oResizeRange;
            Excel._Chart oChart;
            String sMsg;
            int iNumQtrs;

            //Determine how many quarters to display data for.
            for (iNumQtrs = 4; iNumQtrs >= 2; iNumQtrs--)
            {
                sMsg = "Enter sales data for ";
                sMsg = String.Concat(sMsg, iNumQtrs);
                sMsg = String.Concat(sMsg, " quarter(s)?");

                DialogResult iRet = MessageBox.Show(sMsg, "Quarterly Sales?",
                MessageBoxButtons.YesNo);
                if (iRet == DialogResult.Yes)
                    break;
            }

            sMsg = "Displaying data for ";
            sMsg = String.Concat(sMsg, iNumQtrs);
            sMsg = String.Concat(sMsg, " quarter(s).");

            MessageBox.Show(sMsg, "Quarterly Sales");

            //Starting at E1, fill headers for the number of columns selected.
            oResizeRange = oWS.get_Range("E1", "E1").get_Resize(Missing.Value, iNumQtrs);
            oResizeRange.Formula = "=\"Q\" & COLUMN()-4 & CHAR(10) & \"Sales\"";

            //Change the Orientation and WrapText properties for the headers.
            oResizeRange.Orientation = 38;
            oResizeRange.WrapText = true;

            //Fill the interior color of the headers.
            oResizeRange.Interior.ColorIndex = 36;

            //Fill the columns with a formula and apply a number format.
            oResizeRange = oWS.get_Range("E2", "E6").get_Resize(Missing.Value, iNumQtrs);
            oResizeRange.Formula = "=RAND()*100";
            oResizeRange.NumberFormat = "$0.00";

            //Apply borders to the Sales data and headers.
            oResizeRange = oWS.get_Range("E1", "E6").get_Resize(Missing.Value, iNumQtrs);
            oResizeRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

            //Add a Totals formula for the sales data and apply a border.
            oResizeRange = oWS.get_Range("E8", "E8").get_Resize(Missing.Value, iNumQtrs);
            oResizeRange.Formula = "=SUM(E2:E6)";
            oResizeRange.Borders.get_Item(Excel.XlBordersIndex.xlEdgeBottom).LineStyle
            = Excel.XlLineStyle.xlDouble;
            oResizeRange.Borders.get_Item(Excel.XlBordersIndex.xlEdgeBottom).Weight
            = Excel.XlBorderWeight.xlThick;

            //Add a Chart for the selected data.
            oWB = (Excel._Workbook)oWS.Parent;
            oChart = (Excel._Chart)oWB.Charts.Add(Missing.Value, Missing.Value,
            Missing.Value, Missing.Value);

            //Use the ChartWizard to create a new chart from the selected data.
            oResizeRange = oWS.get_Range("E2:E6", Missing.Value).get_Resize(
            Missing.Value, iNumQtrs);
            oChart.ChartWizard(oResizeRange, Excel.XlChartType.xl3DColumn, Missing.Value,
            Excel.XlRowCol.xlColumns, Missing.Value, Missing.Value, Missing.Value,
            Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            oSeries = (Excel.Series)oChart.SeriesCollection(1);
            oSeries.XValues = oWS.get_Range("A2", "A6");
            for (int iRet = 1; iRet <= iNumQtrs; iRet++)
            {
                oSeries = (Excel.Series)oChart.SeriesCollection(iRet);
                String seriesName;
                seriesName = "=\"Q";
                seriesName = String.Concat(seriesName, iRet);
                seriesName = String.Concat(seriesName, "\"");
                oSeries.Name = seriesName;
            }

            oChart.Location(Excel.XlChartLocation.xlLocationAsObject, oWS.Name);

            //Move the chart so as not to cover your data.
            oResizeRange = (Excel.Range)oWS.Rows.get_Item(10, Missing.Value);
            oWS.Shapes.Item("Chart 1").Top = (float)(double)oResizeRange.Top;
            oResizeRange = (Excel.Range)oWS.Columns.get_Item(2, Missing.Value);
            oWS.Shapes.Item("Chart 1").Left = (float)(double)oResizeRange.Left;
        }

        /// <summary>
        /// This methods gets the values of Windows environment variables that
        /// are required to execute GetCoords; specifically these are 'MICSUSER'
        /// and 'PASSWORD' that are required for a successful call to UtConnect().
        /// </summary>
        public static void GetEnvVariablesForUtConnect()
        {
            // Get the user's MICS ID from the environment.
            Info.MicsUserName = Environment.GetEnvironmentVariable("MICSUSER");     // REQUIRED.
            if (String.IsNullOrWhiteSpace(Info.MicsUserName))
            {
                Log2.e("\r\nFeImport.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                _NewLib.Application.ExitQuietly(Error.ENVVARMICSUSERNOTSET);
            }

            // Get the user's password from the environment.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
            if (String.IsNullOrWhiteSpace(Info.Password))
            {
                // Password just has to be set to something; its value is never used.
                Info.Password = "Bananarama";
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

```
