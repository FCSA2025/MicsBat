# Documented File: Form1.cs
**Repository Path:** `LAMLreports\Form1.cs`
**Primary Layer:** `LAMLreports`
**Namespace:** `LAMLreports`

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
using System.Runtime.CompilerServices;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace LAMLreports
{
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    public partial class Form1 : Form
    {
        // Get the ISED extract date from the environment variable ISEDDATE (yyyymmdd).
        //Info.IsedDate = Environment.GetEnvironmentVariable("ISEDDATE");     // REQUIRED.
        //    if (String.IsNullOrWhiteSpace(Info.IsedDate))
        //    {
        //        Log2.e("\r\nExcelApp.GetEnvVariables(): ERROR: Windows environment variable IsedDate is not set.");
        //        _NewLib.Application.ExitQuietly(_Configuration.Error.ENVVARMICSUSERNOTSET);
        //    }

    private const string EXCEL_TEMPLATE_FILE_PATH = @"D:\users\ahulme\Missing and Phantoms\Report Template B.xlsx";
        //private const string TARGET_DIR = @"D:\users\ahulme\Missing and Phantoms\";  CHANGED BY BA 2025/4/16
        private const string TARGET_DIR = @"D:\Reports\LicensedAndMissingLinks\";
        private const string DATE_TAG = "2025-03-27";
        private const string TAFL_LINK_ANALYSIS_TABLE_NAME = "hulme.LinkMatch_TaflToMdb_20250327"; 
        private const string MDB_LINK_ANALYSIS_TABLE_NAME = "hulme.LinkMatch_MdbToTafl_20250327";

        private const UInt32 DARK_RED = (UInt32)(192 | (0 << 8) | (0 << 16));
        private const UInt32 DARK_GREEN = (UInt32)(0 | (128<< 8) | (0 << 16));
        private const UInt32 GREY_3 = (UInt32)(191 | (191 << 8) | (191 << 16));
        private const UInt32 PURPLE = (UInt32)(112 | (48 << 8) | (160 << 16));
        private const UInt32 TANGERINE = (UInt32)(226 | (107 << 8) | (10 << 16));

        private static List<string> mOperNames = new List<string>()
        {
            "ABCCOM",
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
            "ONT", 
            "RCTL",
            "SHAW",
            "STEL",
            "TBAY",
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
            bStart.Enabled = false;

            Task.Run(() => CreateWorkbooksAsync());
        }
#if false
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
                Log2.e("\r\nExcelApp.GetEnvVariables(): ERROR: Windows environment variable MicsUser is not set.");
                _NewLib.Application.ExitQuietly(_Configuration.Error.ENVVARMICSUSERNOTSET);
            }

            // Get the user's password from the environment.
            Info.Password = Environment.GetEnvironmentVariable("PASSWORD");         // REQUIRED (but can be anything).
            if (String.IsNullOrWhiteSpace(Info.Password))
            {
                // Password just has to be set to something; its value is never used.
                Info.Password = "Bananarama";
            }
        }
#endif
        private void Exit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public Task CreateWorkbooksAsync()
        {
            Excel.Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheetT2M;
            Excel._Worksheet oSheetM2T;

            SQLLEN[] nullInds = new SQLLEN[LinkMatch.NUM_COLUMNS];
            int numRecords = 0;
            int badRecords = 0;
            int numCells = 0;

            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                myTextBox.AppendText(String.Format("\r\nExcel template workbook :   {0}", EXCEL_TEMPLATE_FILE_PATH));           // \r\n is required to force a new line.
                myTextBox.AppendText(String.Format("\r\nMDB  to TAFL SQL table  :   {0}", MDB_LINK_ANALYSIS_TABLE_NAME));       // \r\n is required to force a new line.
                myTextBox.AppendText(String.Format("\r\nTAFL to  MDB SQL table  :   {0}", TAFL_LINK_ANALYSIS_TABLE_NAME));      // \r\n is required to force a new line.

                // UtConnect() expects to receive the user's MICSUSER and PASSWORD
                // environment variables via the static class Info. Get the values 
                // of these two environmental variables and set Info.MicsUserName
                // and Info.Password.
                // GetEnvVariablesForUtConnect();
                Info.MicsUserName = "hulme1";
                Info.Password = "bananarama";

                // Prescribe the database name.
                Info.DbName = "micsdev";

                // Establish an FCSA user session with the database.
                int rc = Ssutil.UtConnect(Info.DbName, 1);
                if (rc != 0)
                {
                    /* Can't connect to database */
                    string msg = String.Format("ERROR: Cannot connect to database {0}.\r\n", Info.DbName);
                    Log2.e("\n\nForm1.CreateWorkbooksAsync(): " + msg);
                    Console.Write("\n" + msg);
                    _NewLib.Application.ExitQuietly(11);
                }

                myTextBox.AppendText("\r\nUtConnect() succeeded ...");

                progressBar1.Visible = true;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 999;
                progressBar1.Value = 0;
                progressBar1.Step = 1;

                foreach (string operName in mOperNames)
                {
                    //if (operName != "BRAGG") continue;

                    //Start Excel and get an Application object.
                    oXL = new Excel.Application();
                    oXL.Visible = false;

                    myTextBox.AppendText(String.Format("\r\n\r\n=========================== {0}", operName));
                    myTextBox.AppendText("\r\nOpening Excel template workbook ...");   // \r\n is required to force a new line.
                    tProgress.Text = "";
                    //progressBar1.Maximum = numRecords;

                    //oWB = (Excel._Workbook)oXL.Workbooks.Open(@"D:\users\ahulme\ISED TAFL data\ColumnTitles.xlsx");
                    oWB = (Excel._Workbook)oXL.Workbooks.Open(EXCEL_TEMPLATE_FILE_PATH);

                    //==================================================
                    // Matching TAFL records to MDB links.
                    // Populate the Excel sheet named "Missing-links".
                    //==================================================

                    myTextBox.AppendText("\r\nStarting population of Missing-Links sheet ...");

                    // Construct the SQL query 'WHERE' clause for the operator SELECT.
                    string whereClause = String.Format("{0} = '{1}'", "t_LicenseeName_oper", operName);

                    // Get the SQL table record count.
                    numRecords = Ssutil.DbCountRows(TAFL_LINK_ANALYSIS_TABLE_NAME, whereClause);
                    badRecords = Ssutil.DbCountRows(TAFL_LINK_ANALYSIS_TABLE_NAME, whereClause + " AND x_LAF != 'LAF' ");

                    int offset = 0;
                    int START_ROW = 10;
                    numCells = 35;

                    int nRet;
                    LinkMatch linkMatch = null;

                    oSheetT2M = (Excel._Worksheet)oWB.Sheets["Missing-links"];

                    progressBar1.Value = progressBar1.Minimum;
                    progressBar1.Maximum = numRecords;
                    tProgress.Text = "Missing-Links Sheet";

                    oSheetT2M.Cells[5, 3].Value = DATE_TAG;
                    oSheetT2M.Cells[5, 15].Value = numRecords.ToString();
                    oSheetT2M.Cells[5, 22].Value = badRecords.ToString();



                    // Do the SELECT for this operator.
                    int handle = DynLinkMatch.Select(TAFL_LINK_ANALYSIS_TABLE_NAME, whereClause, "x_LAF, m_OrdinalKey, m_Bndcde, m_Chid, t_FreqTxRx");

                    while ((nRet = DynLinkMatch.Fetch(handle, out linkMatch, out nullInds)) == Constant.SUCCESS)
                    {
                        object[] cellValues = new object[numCells];

                        cellValues[1] = (nullInds[LinkMatch.M_OPER] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Oper;
                        cellValues[2] = (nullInds[LinkMatch.M_SITENAME] == Constant.DB_NULL) ? "NULL" : linkMatch.m_SiteName;
                        cellValues[3] = (nullInds[LinkMatch.M_REGION] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Region;
                        cellValues[4] = (nullInds[LinkMatch.M_PROV] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Prov;
                        cellValues[5] = (nullInds[LinkMatch.M_CALL1] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Call1.Replace("=", "'=");
                        cellValues[6] = (nullInds[LinkMatch.M_CALL2] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Call2.Replace("=", "'=");
                        cellValues[7] = (nullInds[LinkMatch.M_BNDCDE] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Bndcde;
                        cellValues[8] = (nullInds[LinkMatch.M_CHID] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Chid;
                        cellValues[9] = (nullInds[LinkMatch.M_ANUM] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Anum.ToString();
                        cellValues[10] = (nullInds[LinkMatch.M_LAT] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Lat.ToString("0.000000");
                        cellValues[11] = (nullInds[LinkMatch.M_LNG] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Lng.ToString("0.000000");
                        cellValues[12] = (nullInds[LinkMatch.M_AZMTH] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Azmth.ToString("0.0");
                        cellValues[13] = (nullInds[LinkMatch.M_FREQTX] == Constant.DB_NULL) ? "NULL" : linkMatch.m_FreqTx.ToString();
                        cellValues[14] = (nullInds[LinkMatch.M_FREQRX] == Constant.DB_NULL) ? "NULL" : linkMatch.m_FreqRx.ToString();
                        cellValues[15] = (nullInds[LinkMatch.M_STATTXRX] == Constant.DB_NULL) ? "NULL" : linkMatch.m_StatTxRx;
                        cellValues[16] = (nullInds[LinkMatch.X_CONFIDENCE] == Constant.DB_NULL) ? "NULL" : linkMatch.x_Confidence;
                        cellValues[17] = (nullInds[LinkMatch.T_TXRX] == Constant.DB_NULL) ? "NULL" : linkMatch.t_TxRx;
                        cellValues[18] = (nullInds[LinkMatch.T_AUTHORIZATIONNUMBER] == Constant.DB_NULL) ? "NULL" : linkMatch.t_AuthorizationNumber;
                        cellValues[19] = (nullInds[LinkMatch.T_CALLSIGN] == Constant.DB_NULL) ? "NULL" : linkMatch.t_Callsign;
                        cellValues[20] = (nullInds[LinkMatch.T_INSERVICEDATE] == Constant.DB_NULL) ? "NULL" : linkMatch.t_InserviceDate;
                        cellValues[21] = (nullInds[LinkMatch.T_ACCOUNTNUMBER] == Constant.DB_NULL) ? "NULL" : linkMatch.t_AccountNumber;
                        cellValues[22] = (nullInds[LinkMatch.T_LICENSEENAME] == Constant.DB_NULL) ? "NULL" : linkMatch.t_LicenseeName;
                        cellValues[23] = (nullInds[LinkMatch.T_LICENSEENAME_OPER] == Constant.DB_NULL) ? "NULL" : linkMatch.t_LicenseeName_oper;
                        cellValues[24] = (nullInds[LinkMatch.T_LAT] == Constant.DB_NULL) ? "NULL" : linkMatch.t_Lat.ToString("0.000000");
                        cellValues[25] = (nullInds[LinkMatch.T_LNG] == Constant.DB_NULL) ? "NULL" : linkMatch.t_Lng.ToString("0.000000");
                        cellValues[26] = (nullInds[LinkMatch.T_AZMTH] == Constant.DB_NULL) ? "NULL" : linkMatch.t_Azmth.ToString("0.0");
                        cellValues[27] = (nullInds[LinkMatch.T_FREQTXRX] == Constant.DB_NULL) ? "NULL" : linkMatch.t_FreqTxRx.ToString();
                        cellValues[28] = (nullInds[LinkMatch.T_AUTHORIZATIONSTATUS] == Constant.DB_NULL) ? "NULL" : linkMatch.t_AuthorizationStatus;
                        cellValues[29] = (nullInds[LinkMatch.D_METERS] == Constant.DB_NULL) ? "NULL" : linkMatch.d_Meters.ToString("F0");
                        cellValues[30] = (nullInds[LinkMatch.D_DEGREES] == Constant.DB_NULL) ? "NULL" : linkMatch.d_Degrees.ToString("0.0");
                        cellValues[31] = (nullInds[LinkMatch.D_MHZ] == Constant.DB_NULL) ? "NULL" : linkMatch.d_MHz.ToString();
                        cellValues[32] = (nullInds[LinkMatch.X_FOM] == Constant.DB_NULL) ? "NULL" : linkMatch.x_FOM.ToString();
                        cellValues[33] = (nullInds[LinkMatch.X_LAF] == Constant.DB_NULL) ? "NULL" : linkMatch.x_LAF;
                        cellValues[34] = (nullInds[LinkMatch.T_KEYFIELD] == Constant.DB_NULL) ? "NULL" : linkMatch.t_KeyField.ToString();

                        Range startCell = oSheetT2M.Cells[START_ROW + offset, 1];
                        Range endCell = oSheetT2M.Cells[START_ROW + offset, numCells];
                        var range = oSheetT2M.Range[startCell, endCell];

                        switch (linkMatch.x_LAF)
                        {
                            case "LAF":
                                cellValues[0] = "TAFL record has match in MDB.";
                                range.Font.Color = DARK_GREEN;
                                break;
                            case "LA-":
                                string msg = (linkMatch.t_TxRx.ToUpper() == "TX") ? "incorrect Tx freq?" : "incorrect Rx freq?";
                                cellValues[0] = "No-match: " + msg;
                                range.Font.Color = TANGERINE;
                                break;
                            case "L-F":
                                cellValues[0] = "No-match: incorrect azimuth?";
                                range.Font.Color = PURPLE;
                                break;
                            case "---":
                                cellValues[0] = "TAFL record has no match in MDB.";
                                range.Font.Color = DARK_RED;
                                break;
                        }

                        range.Value2 = cellValues;

                        offset++;

                        progressBar1.PerformStep();
                    }

                    DynLinkMatch.Close(handle);

                    //==================================================
                    // Matching MDB links to TAFL records.
                    // Populate the Excel sheet named "Licensed-links".
                    //==================================================

                    myTextBox.AppendText("\r\nStarting population of Licensed-links sheet ...");

                    // Construct the 'WHERE' clause for the SELECT.
                    whereClause = String.Format("{0} = '{1}'", "m_Oper", operName);

                    // Get the SQL table record count.
                    numRecords = Ssutil.DbCountRows(MDB_LINK_ANALYSIS_TABLE_NAME, whereClause);
                    badRecords = Ssutil.DbCountRows(MDB_LINK_ANALYSIS_TABLE_NAME, whereClause + " AND x_LAF != 'LAF' ");

                    offset = 0;
                    START_ROW = 10;
                    numCells = 30;

                    oSheetM2T = (Excel._Worksheet)oWB.Sheets["Licensed-links"];

                    oSheetM2T.Cells[5, 3].Value = DATE_TAG;

                    progressBar1.Value = progressBar1.Minimum;
                    progressBar1.Maximum = numRecords;
                    tProgress.Text = "Licensed-Links Sheet";

                    oSheetM2T.Cells[5, 15].Value = numRecords.ToString();
                    oSheetM2T.Cells[5, 22].Value = badRecords.ToString();

                    // Do the SELECT for this operator.
                    handle = DynLinkMatch.Select(MDB_LINK_ANALYSIS_TABLE_NAME, whereClause, "x_LAF, m_OrdinalKey, m_Bndcde, m_Chid, t_FreqTxRx");

                    while ((nRet = DynLinkMatch.Fetch(handle, out linkMatch, out nullInds)) == Constant.SUCCESS)
                    {
                        object[] cellValues = new object[numCells];

                        cellValues[1] = (nullInds[LinkMatch.M_OPER] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Oper;
                        cellValues[2] = (nullInds[LinkMatch.M_SITENAME] == Constant.DB_NULL) ? "NULL" : linkMatch.m_SiteName;
                        cellValues[3] = (nullInds[LinkMatch.M_REGION] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Region;
                        cellValues[4] = (nullInds[LinkMatch.M_PROV] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Prov;
                        cellValues[5] = (nullInds[LinkMatch.M_CALL1] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Call1.Replace("=", "'=");
                        cellValues[6] = (nullInds[LinkMatch.M_CALL2] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Call2.Replace("=", "'=");
                        cellValues[7] = (nullInds[LinkMatch.M_BNDCDE] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Bndcde;
                        cellValues[8] = (nullInds[LinkMatch.M_CHID] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Chid;
                        cellValues[9] = (nullInds[LinkMatch.M_ANUM] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Anum.ToString();
                        cellValues[10] = (nullInds[LinkMatch.M_LAT] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Lat.ToString("0.000000");
                        cellValues[11] = (nullInds[LinkMatch.M_LNG] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Lng.ToString("0.000000");
                        cellValues[12] = (nullInds[LinkMatch.M_AZMTH] == Constant.DB_NULL) ? "NULL" : linkMatch.m_Azmth.ToString("0.0");
                        cellValues[13] = (nullInds[LinkMatch.M_FREQTX] == Constant.DB_NULL) ? "NULL" : linkMatch.m_FreqTx.ToString();
                        cellValues[14] = (nullInds[LinkMatch.M_FREQRX] == Constant.DB_NULL) ? "NULL" : linkMatch.m_FreqRx.ToString();
                        cellValues[15] = (nullInds[LinkMatch.M_STATTXRX] == Constant.DB_NULL) ? "NULL" : linkMatch.m_StatTxRx;
                        cellValues[16] = (nullInds[LinkMatch.X_CONFIDENCE] == Constant.DB_NULL) ? "NULL" : linkMatch.x_Confidence;
                        cellValues[17] = (nullInds[LinkMatch.T_TXRX] == Constant.DB_NULL) ? "NULL" : linkMatch.t_TxRx;
                        cellValues[18] = (nullInds[LinkMatch.T_AUTHORIZATIONNUMBER] == Constant.DB_NULL) ? "NULL" : linkMatch.t_AuthorizationNumber;
                        cellValues[19] = (nullInds[LinkMatch.T_CALLSIGN] == Constant.DB_NULL) ? "NULL" : linkMatch.t_Callsign;
                        cellValues[20] = (nullInds[LinkMatch.T_INSERVICEDATE] == Constant.DB_NULL) ? "NULL" : linkMatch.t_InserviceDate;
                        cellValues[21] = (nullInds[LinkMatch.T_ACCOUNTNUMBER] == Constant.DB_NULL) ? "NULL" : linkMatch.t_AccountNumber;
                        cellValues[22] = (nullInds[LinkMatch.T_LICENSEENAME] == Constant.DB_NULL) ? "NULL" : linkMatch.t_LicenseeName;
                        cellValues[23] = (nullInds[LinkMatch.T_AUTHORIZATIONSTATUS] == Constant.DB_NULL) ? "NULL" : linkMatch.t_AuthorizationStatus;
                        cellValues[24] = (nullInds[LinkMatch.D_METERS] == Constant.DB_NULL) ? "NULL" : linkMatch.d_Meters.ToString("F0");
                        cellValues[25] = (nullInds[LinkMatch.D_DEGREES] == Constant.DB_NULL) ? "NULL" : linkMatch.d_Degrees.ToString("F1");
                        cellValues[26] = (nullInds[LinkMatch.D_MHZ] == Constant.DB_NULL) ? "NULL" : linkMatch.d_MHz.ToString();
                        cellValues[27] = (nullInds[LinkMatch.X_FOM] == Constant.DB_NULL) ? "NULL" : linkMatch.x_FOM.ToString();
                        cellValues[28] = (nullInds[LinkMatch.X_LAF] == Constant.DB_NULL) ? "NULL" : linkMatch.x_LAF;
                        cellValues[29] = (nullInds[LinkMatch.T_KEYFIELD] == Constant.DB_NULL) ? "NULL" : linkMatch.t_KeyField.ToString();

                        Range startCell = oSheetM2T.Cells[START_ROW + offset, 1];
                        Range endCell = oSheetM2T.Cells[START_ROW + offset, numCells];
                        var range = oSheetM2T.Range[startCell, endCell];

                        switch (linkMatch.x_LAF)
                        {
                            case "LAF":
                                cellValues[0] = "MDB link matches TAFL record.";
                                range.Font.Color = DARK_GREEN;
                                break;
                            case "LA-":
                                string msg = (linkMatch.m_TxRx.ToUpper() == "TX") ? "incorrect Tx freq?" : "incorrect Rx freq?";
                                cellValues[0] = "No-match: " + msg;
                                range.Font.Color = TANGERINE;
                                break;
                            case "L-F":
                                cellValues[0] = "No-match: incorrect azimuth?";
                                range.Font.Color = PURPLE;
                                break;
                            case "---":
                                cellValues[0] = "MDB link has no match in TAFL.";
                                range.Font.Color = DARK_RED;
                                break;
                        }

                        range.Value2 = cellValues;

                        offset++;

                        progressBar1.PerformStep();
                    }

                    DynLinkMatch.Close(handle);

                    // Save the Excel workbook to a prescribed file path.
                    string targetFilePath = String.Format("{0}{1}_{2}.xlsx", TARGET_DIR, operName, DATE_TAG);

                    myTextBox.AppendText("\r\nSaveAs Excel file to " + targetFilePath + " ...");

                    // Delete any existing file to stop Excel popping up a 'what do I do?" dialog.
                    File.Delete(targetFilePath);

                    oWB.SaveAs(targetFilePath, 51);

                    oWB.Close(false);
                    oXL.Quit();



                    // Manual disposal because of COM
                    while (Marshal.ReleaseComObject(oXL) != 0) { }
                    while (Marshal.ReleaseComObject(oWB) != 0) { }
                    while (Marshal.ReleaseComObject(oSheetT2M) != 0) { }
                    while (Marshal.ReleaseComObject(oSheetM2T) != 0) { }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();


                } // end of loop over operator names.

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

            return Task.CompletedTask;
        }

        /// <summary>
        /// This method encapsulates the submission of a query to the ODBC / SQL Server 
        /// without having to specify the length of the query string.
        /// </summary>
        /// <param name="hStmt"> - an open ODBC statement handle</param>
        /// <param name="query"> - SQL query to be submitted.</param>
        /// <returns></returns>
        public static int DoQuery(SQLHANDLE hStmt, string query)
        {
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n" + ODBC.GetDiagnostics(hStmt, query));
            }

            return sqlRet;
        }

    }
}

```
