# Documented File: _DoxygenRelatedPages.cs
**Repository Path:** `_NewLib\_DoxygenRelatedPages.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `Global`

## Source Code Representation
```csharp

// This non-existent class is just a subterfuge to insert some MS XML
// comments that contain Doxygen commands to populate the 
// 'Related Pages' tab with titles and brief descriptions.

//       10        20        30        40        50        60        70        80
//345678901234567890123456789012345678901234567890123456789012345678901234567890

/// <summary>
/// <!-- The following line is a palliative to get the subsequent page
///      to display its 'brief' content correctly. -->
/// \page ruse &#32; 
/// <!--------------------------------------------------------------------->
/// \page aardvark MICS Programs: C++ Documentation
/// \brief <a href="OEL CDocumentation.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This document was written in 2016 by Greg. Shannon (OEL) and provides an 
/// introduction to the structure of the legacy C/C++ code. It is the 
/// recommended starting point for anyone who wants to gain an understanding of 
/// the MICS software even though the original code has been converted to C#.
/// </para>
/// <h3>Table of Contents</h3>
/// <para>
/// Overview <br>
/// Database <br>
/// Overview of the Structure <br>
/// Structure of the Terrestrial Database <br>
/// Structure of the Earth Station Database <br>
/// The Subsidiary Database <br>
/// Update Tables <br>
/// Programs: <br>
/// ftImport - Import Terrestrial Ascii Files <br>
/// ftValidate - Validate input file and perform power calculations. <br>
/// feValidate - Earth Station Validation and Satellite Calculations <br>
/// tpRunTsip - Run the Terrestrial and Space Interference Program <br>
/// mtUpdate - Updating the TS Database <br>
/// </para>
/// <!--------------------------------------------------------------------->
/// \page bear WebMICS Reference Guide: TS
/// \brief <a href="webmics TS Reference.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This document provides a very detailed description of the WebMICS Windows
/// graphical user-interface for the case of Terrestial Stations (TS).
/// </para>
/// <!--------------------------------------------------------------------->
/// \page cow WebMICS Reference Guide: ES
/// \brief <a href="webmics ES Reference.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This document provides a very detailed description of the WebMICS Windows
/// graphical user-interface for the case of Earth Stations (ES).
/// </para>
/// <!--------------------------------------------------------------------->
/// \page duck PDF Text File Format: ES
/// \brief <a href="ES Data File - Text File Format.pdf">Open the document.</a>
/// \brief
/// <h3>Description</h3>
/// <para>
/// This document provides a comprehensive worked example of a PDF import file
/// for Earth Stations (ES) together with a detailed description of its 
/// individual records (qualified lines) and their constituent 
/// comma-separated-value (CSV) fields.
/// </para>
/// <!--------------------------------------------------------------------->
/// \page eagle PDF Text File Format: TS
/// \brief <a href="TS Data File - Text File Format.pdf">Open the document.</a>
/// \brief
/// <h3>Description</h3>
/// <para>
/// This document provides a comprehensive worked example of a PDF import file
/// for Terrestial Stations (TS) together with a detailed description of its 
/// individual records (qualified lines) and their constituent 
/// comma-separated-value (CSV) fields.
/// </para>
/// <!--------------------------------------------------------------------->
/// \page fox Using Doxygen to create MICS# Documentation
/// \brief <a href="AH-0017 Using Doxygen.pdf">Open the document.</a>
/// \brief
/// <h3>Description</h3>
/// <para>
/// This document describes the technical details for
/// using Doxygen to create the MICS# software documentation.
/// </para>
/// <!--------------------------------------------------------------------->
/// \page goat Notes on NAD 27, NAD 83 and UTM
/// \brief <a href="AH-0018 Notes on NAD 27, 83 and UTM.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This document provides background information w.r.t. the NAD 27, NAD 83 and UTM
/// geographic coordinate systems.
/// </para>
/// <!--------------------------------------------------------------------->
/// \page horse Wpassive Intermediate Data Files
/// \brief <a href="AH-0019 Wpassive Input Data File Format.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This document describes the format of the intermediate text data files that
/// are used for data exchange between WebMICS and Wpassive..
/// </para>
/// <!--------------------------------------------------------------------->
/// \page ibex Visual Studio File Types for C# Development
/// \brief <a href="Visual Studio File Types for Csharp.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This document tabulates and describes the several different file types (by extension) that are associated with
/// Visual Studio development of C# applications.
/// </para>
/// <!--------------------------------------------------------------------->
/// \page koala MICS# programs - Exit Codes
/// \brief <a href="MICS_ErrorCodes.pdf">Open the document.</a> \brief
/// <h3>Description</h3>
/// <para>
/// This table provides a listing of the MICS# numerical ExitCodes
/// and their meaning.
/// </para>
/// <!--------------------------------------------------------------------->
/// \page jaguar Technical Notes
/// <table border="1" cellspacing="0" style="width:75%" align="center">
/// <tr BGCOLOR=#E0FFFF style="color:DarkBlue">
/// <th title="Field #1">ID#</th>
/// <th title="Field #2">Title</th>
/// </tr>
/// <tr>
/// <td>AH-0001</td>
/// <td><a href="AH-0001 MICS Code Overview.pdf">MICS Code Overview.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0002</td>
/// <td><a href="AH-0002 MICS Code Risk Assessment (final).pdf">MICS Code Risk Assessment (final).pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0003</td>
/// <td><a href="AH-0003 MICS Code Metrics.pdf">MICS Code Metrics.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0004</td>
/// <td><a href="AH-0004 P_invoke APIs Cpp and Csharp V3.pdf">P_invoke APIs C++ and C# V3.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0014</td>
/// <td><a href="AH-0014 Bug Description (sizeof).pdf">Bug Description (sizeof).pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0017</td>
/// <td><a href="AH-0017 Using Doxygen.pdf">Using Doxygen.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0019</td>
/// <td><a href="AH-0019 Wpassive Input Data File Format.pdf">Wpassive Input Data File Format.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0020</td>
/// <td><a href="AH-0020 Description of ES Import File es300km.pdf">Description of ES Import File es300km.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0021</td>
/// <td><a href="AH-0021 System Documentation Project Charter.pdf">System Documentation Project Charter.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0022</td>
/// <td><a href="AH-0022 Overview of the MICS.pdf">Overview of the MICS.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0023</td>
/// <td><a href="AH-0023 Definition of the New ISED Fee Model.pdf">Definition of the New ISED Fee Model.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0024</td>
/// <td><a href="AH-0024 Coherence between ISED and FCSA database contents.pdf">Coherence between ISED and FCSA database contents.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0025</td>
/// <td><a href="AH-0025 Listing of Missing and Phantom MDB TS Links for SHAW.pdf">Listing of Missing and Phantom MDB TS Links for SHAW.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0026</td>
/// <td><a href="AH-0026 New Fee Model - Work Products.pdf">New Fee Model - Work Products.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0027</td>
/// <td><a href="AH-0027 Overview of Regression Testing of FtValidate.pdf">Overview of Regression Testing of FtValidate.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0028</td>
/// <td><a href="AH-0028 Automation of Ftvalidate Regression Testing.pdf">Automation of Ftvalidate Regression Testing.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0029</td>
/// <td><a href="AH-0029 Ftvalidate Testing using the 19sep2019 Master Reports.pdf">Ftvalidate Testing using the 19sep2019 Master Reports.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0030</td>
/// <td><a href="AH-0030 FtValidate Master Test Report Anomaly 1.pdf">FtValidate Master Test Report Anomaly #1.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0031</td>
/// <td><a href="AH-0031 FtValidate Master Test Report Anomaly 2.pdf">FtValidate Master Test Report Anomaly #2.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0032</td>
/// <td><a href="AH-0032 FtValidate Master Test Report Anomaly 3.pdf">FtValidate Master Test Report Anomaly #3.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0034</td>
/// <td><a href="AH-0034 TsipInitiator System Context.pdf">TsipInitiator System Context.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0035</td>
/// <td><a href="AH-0035 Bug Fix b070419A.pdf">Bug Fix b070419A.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0036</td>
/// <td><a href="AH-0036 Analysis of the MICS Band Codes.pdf">Analysis of the MICS Band Codes.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0037</td>
/// <td><a href="AH-0037 Reverse-Engineering the HiLo Algorithm.pdf">Reverse-Engineering the HiLo Algorithm.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0038</td>
/// <td><a href="AH-0038 Comparison of the ISED SRSP bands and the MICS bands.pdf">Comparison of the ISED SRSP bands and the MICS bands.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0039</td>
/// <td><a href="AH-0039 New Definition of HiLo Algorithm.pdf">New Definition of HiLo Algorithm.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0040</td>
/// <td><a href="AH-0040 bndcde usage in the MDB.pdf">bndcde usage in the MDB.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0041</td>
/// <td><a href="AH-0041 Description of the MICS DB Access Queuing Mechanism.pdf">Description of the MICS DB Access Queuing Mechanism.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0042</td>
/// <td><a href="AH-0042 Description of the New HiLo Analysis Algorithm.pdf">Description of the New HiLo Analysis Algorithm.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0043</td>
/// <td><a href="AH-0043 New FCSA HiLo Analysis and Reporting.pdf">New FCSA HiLo Analysis and Reporting.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0044</td>
/// <td><a href="AH-0044 Investigation of ISED TAFL import problems.pdf">Investigation of ISED TAFL import problems.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0045</td>
/// <td><a href="AH-0045 FCSA email password change instructions.pdf">FCSA email password change instructions.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0046</td>
/// <td><a href="AH-0046 Changes to MDB mt_ante, mt_chan and sd_band Tables.pdf">Changes to MDB mt_ante, mt_chan and sd_band Tables.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0047</td>
/// <td><a href="AH-0047 TSIP Execution Performance Benchmarks June 2018.pdf">TSIP Execution Performance Benchmarks June 2018.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0048</td>
/// <td><a href="AH-0048 MDB TS Site, Antenna and Channel Pathology.pdf">MDB TS Site, Antenna and Channel Pathology.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0049</td>
/// <td><a href="AH-0049 Microwave Digital Encoding Terminology abbreviations and descriptions.pdf">Microwave Digital Encoding Terminology abbreviations and descriptions.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0050</td>
/// <td><a href="AH-0050 Comsearch file structure.pdf">Comsearch file structure.pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0051</td>
/// <td><a href="AH-0051 Notes on Unfaded Rx Signal Level and BER Threshold Levels .pdf">Notes on Unfaded Rx Signal Level and BER Threshold Levels .pdf</a></td>
/// </tr>
/// <tr>
/// <td>AH-0052</td>
/// <td><a href="AH-0052 Fee Accounting Algorithm.pdf">Fee Accounting Algorithm.pdf</a></td>
/// </tr>
/// </table>
/// <!--------------------------------------------------------------------->
/// </summary>



```
