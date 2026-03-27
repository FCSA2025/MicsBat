using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace _Auxlib
{
    /// <summary>
    /// This class provides methods that perform (Lat, Long) conversions between the 
    /// NAD 27 and NAD 83 coordinate systems i.a.w. the National Transformation 
    /// Version 2 (NTv2) grid-shift standard.
    /// </summary>
    /// <remarks>
    /// <para>
    /// NTv2 is a format/method transform coordinates from one coordinate reference
    /// system to another with only a small error: typical precision is a few 
    /// centimeters (or less).
    /// </para><para>
    /// The NTv2 method make use of binary files (.gsb) called 'grids' and this 
    /// format is fully supported by most popular GIS software tools (e.g. PROJ, 
    /// GDAL/OGR etc).
    /// </para><para>
    /// The NTv2 format was developed by Natural Resources Canada's Geodetic 
    /// Survey Division for conversions between NAD27 and NAD83 and it has been 
    /// adapted to several other countries, e.g. Australia, Belgium, Brazil, 
    /// France, Germany, New Zealand, Portugal, South Africa, Spain, Switzerland, 
    /// Venezuela and the United Kingdom.
    /// </para><para>
    /// The method AxOpenGrid must be called before any conversions are performed.
    /// </para><para>
    /// The method AxCloseGrid() should be called to 'tidy up' and release 
    /// resources when finished. 
    /// </para><para>
    /// All conversions are performed by calling the method AxTransCoord() with
    /// prescribed latitude and longitude values measured in centiseconds (0.01s) 
    /// and the required 'direction' of the conversion: 
    /// 1 or TRUE for NAD 27 to 83;
    /// 0; or FALSE for NAD 83 to 27. 
    /// </para><para>
    /// The converted latitude and longitude values are returned in the 'out' 
    /// call arguments newLat and newLong.
    /// </para>
    /// </remarks>
    public class AxNTv2Lib
    {
        private static GridStruct mGridind;
        private static FileStream mFileStream;
        private static BinaryReader mBinfile;
        private static ScanFormatted mSF = new ScanFormatted();

        private struct GridPoint
        {
            public float lat_shift;
            public float long_shift;
        };

        /// <summary>
        /// This method initializes the NTv2 conversion by reading the grid information 
        /// into the GridInd structure and opening the binary file so as to speed-up 
        /// the subsequent conversions. 
        /// </summary>
        /// <remarks>
        /// NTv2 initialization is achieved by calling the method AxTransCoord with 
        /// the first argument set to -1 and the second to 0.
        /// </remarks>
        /// <returns></returns>
        public static int AxOpenGrid()
        {
            int dummy1 = 0;
            int dummy2 = 0;

            return AxTransCoord(-1, 0, out dummy1, out dummy2, Enums.NTv2Dir.NAD27to83);
        }

        /// <summary>
        /// This method releases any resources used to perform the NTv2 conversions
        /// and closes the binary file.
        /// </summary>
        /// <remarks>
        /// NTv2 'close-down' is achieved by calling the method AxTransCoord with 
        /// the first argument set to -1 and the second to -1.
        /// </remarks>
        /// <returns></returns>
        public static int AxCloseGrid()
        {

            int dummy1 = 0;
            int dummy2 = 0;

            return AxTransCoord(-1, -1, out dummy1, out dummy2, Enums.NTv2Dir.NAD27to83);
        }

        /// <summary>
        /// This method converts a (Lat, Long) position in one coordinate system 
        /// (NAD 27 or NAD 83) into its equivalent in the other coordinate system;
        /// it is also used to initialize and (finally) release resources used for the 
        /// conversion.
        /// </summary>
        /// <remarks>
        /// Normally, when this method is called, the conversion file index has already been
        /// read in, and we already have the binary file open. The method converts the
        /// input values from integer centiseconds into decimal seconds, and call readGrid
        /// to look it up. The direction is important here. Going from NAD 83 to 27
        /// requires an iterative method where four iterations are made to derive the
        /// offset. Otherwise, the offset is just added to the input to produce the
        /// output.
        /// <para>
        /// The caller prescribes the 'direction' of the conversion via the argument 'dir': 
        /// <list type="bulllet">
        /// <item>1 or TRUE for NAD 27 to 83;</item>
        /// <item>0; or FALSE for NAD 83 to 27.</item>
        /// </list>
        /// </para><para>
        /// NTv2 initialization is achieved by calling the method AxTransCoord with 
        /// the first argument set to -1 and the second to 0.
        /// </para><para>
        /// NTv2 'close-down' is achieved by calling the method AxTransCoord with 
        /// the first argument set to -1 and the second to -1.
        /// </para>
        /// </remarks>
        /// <param name="latit"> - prescribed latitude value to be converted.</param>
        /// <param name="longit"> - prescribed longitude value to be converted.</param>
        /// <param name="latot"> - converted latitude value.</param>
        /// <param name="longot"> - converted longitude value.</param>
        /// <param name="dir"> - prescribes the direction of the conversion.</param>
        /// <returns></returns>
        public static int AxTransCoord(int latit, int longit, out int latot, out int longot, Enums.NTv2Dir dir)
        {

            // 'out' requirements.
            latot = 0;
            longot = 0;

            float latin, longin;
            float difflat, difflong;
            int i;
            int rc;
            string indName;
            string binName;
            int nRet = 0;

            if (latit < 0)
            {
                /*
                 * This is an initialization condition -- either we are initializing
                 * or we are at the end of the segment of code where the translation
                 * is being done. The next line determines which.
                 */
                if (longit >= 0)
                {
                    /*
                     * We are initializing. Open the index and binary files, and get the
                     * index tree.
                     */
                    string cMicsNtv2Env = Environment.GetEnvironmentVariable("MICS_NAD_FILE");

                    if (String.IsNullOrWhiteSpace(cMicsNtv2Env))
                    {
                        indName = Constant.NTV2_SOURCE_FILE + ".ind";
                        binName = Constant.NTV2_SOURCE_FILE + ".bin";
                    }
                    else
                    {
                        indName = cMicsNtv2Env + ".ind";
                        binName = cMicsNtv2Env + ".bin";
                    }

                    mGridind = OpenIndex(indName);

                    if (mGridind == null)
                    {
                        Console.Write("Could not open Nat. Trans. index file {0}\r\n", indName);
                        Log2.e("\nAxNTv2Lib.AxTransCoord(): ERROR: call to OpenIndex() failed.");
                        return 0;
                    }

                    try
                    {
                        mFileStream = File.Open(binName, FileMode.Open, FileAccess.Read);
                        mBinfile = new BinaryReader(mFileStream);
                    }
                    catch (Exception e)
                    {
                        Console.Write("Could not open Nat. Trans. binary file {0}\r\n", binName);
                        Log2.e("\nAxNTv2Lib.AxTransCoord(): ERROR: exception caught: " + e.Message);
                        return 0;
                    }
                }
                else
                {
                    /*
                     * We are closing down. Close the binary file, and deallocate
                     * the grid index.
                     */
                    mBinfile.Close();
                    mFileStream.Close();
                    mGridind = null;
                }

                /* Return true if all went well	*/
                return 1;
            }
            /*
             * Otherwise, we are translating coordinates. Convert the coordinates into
             * seconds from hundredths of seconds, and call readGrid.
             */
            latin = (float)(latit / 100.0);
            longin = (float)(longit / 100.0);
            difflat = difflong = 0.0f;

            rc = ReadGrid(latin, longin, ref difflat, ref difflong, mGridind, mBinfile, dir);
            /*
             * If the direction is NAD 27 to 83, we are done. Otherwise, we must perform
             * an iterative adjustment to get a more accurate value. Remember that the
             * values in the binary file are actually for one direction only.
             */
            if (rc == 1)
            {
                if (dir == Enums.NTv2Dir.NAD83to27)
                {
                    for (i = 1; (rc == 1) && i < 4; i++)
                    {
                        latin = latin + difflat;
                        longin = longin + difflong;
                        rc = ReadGrid(latin, longin, ref difflat, ref difflong, mGridind, mBinfile, dir);
                    }
                }
                nRet = 1;
            }
            else
            {
                nRet = 0;
            }

            difflat = (float)(difflat * 100.0);
            difflong = (float)(difflong * 100.0);

            latot = (int)((double)latit + (double)difflat + 0.5);

            longot = (int)((double)longit + (double)difflong + 0.5);
            return (nRet);
        }

        /// <summary>
        /// This method sets up the first call to ReadIndex, to create the grid index tree.
        /// </summary>
        /// <param name="indexFilePath"> - prescribed path to index file.</param>
        /// <returns>It returns the grid index tree object if successful; otherwise null</returns>
        public static GridStruct OpenIndex(string indexFilePath)
        {
            //...Log2.v("\nAxNTv2Lib.OpenIndex(): ping: indexName = " + indexName);

            TextReader indfile;
            string firstLine;
            GridStruct gridIndex = null;

            try
            {
                indfile = File.OpenText(indexFilePath);

                //...Log2.v("\nAxNTv2Lib.OpenIndex(): A");

                /*
                 * Dispose of first line (for now --we may use this information in
                 * some future version.)
                 */
                firstLine = indfile.ReadLine();

                //...Log2.v("\nAxNTv2Lib.OpenIndex(): B");

                if (String.IsNullOrWhiteSpace(firstLine))
                {
                    Console.Write("\r\nERROR: OpenIndex(): index file has empty first line or EOF occured.");
                    return null;
                }

                //...Log2.v("\nAxNTv2Lib.OpenIndex(): B-1");

                gridIndex = ReadIndex(indfile, 1);

                //...Log2.v(gridIndex.ToStringAsLines());

                //...Log2.v("\nAxNTv2Lib.OpenIndex(): C");

                indfile.Close();

                if (gridIndex == null)
                {
                    Console.Write("\r\nERROR: OpenIndex(): call to ReadIndex() returned null.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Log2.e("\nAxNTv2Lib.OpenIndex(): ERROR: file I/O exception: " + e.Message);

                return null;
            }

            return gridIndex;
        }

        /// <summary>
        /// This method reads in the index for each subfile in the grid; it works
        /// recursively, calling itself for each subgrid after the root grid is
        /// processed.
        /// </summary>
        /// <param name="indFile"> - TextReader object openned on the index file.</param>
        /// <param name="gridCount"> - the number of sequential grid indices to be read in.</param>
        /// <returns>It returns the grid index tree object if successful; otherwise null</returns>
        public static GridStruct ReadIndex(TextReader indFile, int gridCount)
        {
            //...Log2.v("\nAxNTv2Lib.ReadIndex(): ping");

            GridStruct gridind = null;

            try
            {
                int i;

                string line;
                int numFields;
                List<object> results;

                if (gridCount < 1) return null;

                gridind = new GridStruct();

                for (i = 0; i < gridCount; i++)
                {
                    //...Log2.v("\nAxNTv2Lib.ReadIndex(): gridCount = " + gridCount + "  :  " + i);

                    line = indFile.ReadLine();

                    //!!//...Log2.v("\nAxNTv2Lib.ReadIndex(): B");

                    // Check for EOF.
                    if (line == null)
                    {
                        return null;
                    }

                    numFields = mSF.Parse(line, "%s %s %f %f %f %f %f %f %d %d %d");

                    //...Log2.v("\nAxNTv2Lib.ReadIndex(): C");

                    if (numFields != 11)
                    {
                        Console.Write("\r\nERROR: failed to parse line# {0} in Transformation Index file: {1}", i + 1, line);
                        return null;
                    }

                    //...Log2.v("\nAxNTv2Lib.ReadIndex(): D");

                    results = mSF.Results;

                    if (results == null)
                    {
                        Console.Write("\r\nERROR: premature EOF in Transformation Index file.");
                        return null;
                    }

                    gridind.subname = (string)results[0];
                    gridind.parent = (string)results[1];
                    gridind.latmin = (float)results[2];
                    gridind.latmax = (float)results[3];
                    gridind.latinterval = (float)results[4];
                    gridind.longmin = (float)results[5];
                    gridind.longmax = (float)results[6];
                    gridind.longinterval = (float)results[7];
                    gridind.offset = (int)results[8];
                    gridind.pointcount = (int)results[9];
                    gridind.gridcount = (int)results[10];

                    //...Log2.v("\nAxNTv2Lib.ReadIndex(): " + gridind.ToStringAsLines());

                    // Handle the case where this grid has sub-grids.
                    if (gridind.gridcount > 0)
                    {
                        // Instantiate the subgrid array.
                        GridStruct[] subGridArray = new GridStruct[gridind.gridcount];

                        for (int j = 0; j < gridind.gridcount; j++)
                        {
                            // Beware: recursive call!
                            subGridArray[j] = ReadIndex(indFile, 1);
                        }

                        // Finally, set subgrids to this array object.
                        gridind.subgrids = subGridArray;
                    }
                    else
                    {
                        // No sub-grids.
                        gridind.subgrids = null;
                    }

                } //loop over i.

            }
            catch (Exception e)
            {
                Log2.e("\nAxNTv2Lib.ReadIndex(): ERROR: exception: " + e.Message);
                Log2.e("\n" + e.StackTrace);
            }


            return gridind;
        }

        /// <summary>
        /// This method does a recursive search through the grid index tree,
        /// searching for the point specified by lat and longit; if the point is
        /// within the current grid, the offset is calculated in order to
        /// perform the translation - this offset is used to read the binary file.
        /// </summary>
        /// <remarks>
        /// The binary file consists of records arranged by grid, then by points of
        /// latitude within the grid, and then by points of longitude with a given
        /// latitude. To find a specific record, we must find the offset, calculated
        /// according to the latitude and longitude of the point. So in a given grid
        /// the southeast corner is the first point, and we proceed west along a
        /// given row until we reach the maximum longitude, and then move one step
        /// north for the next row.
        /// \verbatim
        /// The offset is the offset of current grid,
        /// 		plus the offset of the current row
        ///		    plus the offset of the current column
        /// or,
        /// 		gridind[i]->offset +
        ///		        ((int)(floor((lat - latmin)/latinterval))) * rowwidth +
        ///		        ((int)(floor((longit - longmin)/longinterval))) * RECWIDTH
        /// where
        ///		rowwidth = ((int)((longmax - longmin)/longinterval) + 1) /// RECWIDTH
        ///
        /// Usually, four points have to be retrieved, corresponding to the four
        /// points surrounding the point we are interested in. The southeast point
        /// is offsetA, the southwest point is offsetB, the northeast point is
        /// offset C, and the northwest point is offsetD. Then we use bilinear
        /// interpolation to calculate the shift at the point.
        ///
        ///    D     f        C        x = (latP - latA)/latinterval
        ///     +----+-------+         y = (longP - longA)/longinterval
        ///     |    |       | P2
        ///     |    |P      o      If G is the gridshift (lat or longit), then
        ///   h +----o-------+ g
        ///     |  { |       |         Ge = Ga + (Gb - Ga)y
        ///     |  x |~~~y~~~|         Gf = Gc + (Gd - Gc)y
        ///     |  { |       |      and
        ///     +----+---o---+         Gp = Ge + (Gf - Ge)x
        ///    B     e    P3  A
        ///
        /// Note that if the point occurs on the edge of the grid, it appears where
        /// the latitude equals latmin and/or the longitude equal longmin. This ensures
        /// that this formula will always work, even if the point happened to be at A
        /// above.
        /// \endverbatim 
        /// </remarks>
        /// <param name="lat"> - the input latitude.</param>
        /// <param name="longit"> - the input longitude.</param>
        /// <param name="difflat"> - the output latitude difference.</param>
        /// <param name="difflong"> - the output longitude difference.</param>
        /// <param name="gridind"> - fully populated GridStruct object.</param>
        /// <param name="gridfile"> - BinaryReader object openned on the grid file.</param>
        /// <param name="dir"> - prescribed direction of conversion, either NAD27to83 or NAD83to27.</param>
        /// <returns></returns>
        public static int ReadGrid(float lat, float longit, ref float difflat, ref float difflong,
                                   GridStruct gridind, BinaryReader gridfile, Enums.NTv2Dir dir)
        {
            //...Log2.v("\nAxNTv2Lib.ReadGrid(): gridind = " + gridind.ToStringAsLines());

            GridStruct[] gridindArrayOfOne = new GridStruct[1];

            gridindArrayOfOne[0] = gridind;

            int result = ReadGrid(lat, longit, ref difflat, ref difflong, gridindArrayOfOne, 1, gridfile, dir);

            //gridind = gridindArrayOfOne[0];

            return result;
        }

        /// <summary>
        /// This method does a recursive search through the grid index tree,
        /// searching for the point specified by lat and longit; if the point is
        /// within the current grid, the offset is calculated in order to
        /// perform the translation - this offset is used to read the binary file.
        /// </summary>
        /// <remarks>
        /// The binary file consists of records arranged by grid, then by points of
        /// latitude within the grid, and then by points of longitude with a given
        /// latitude. To find a specific record, we must find the offset, calculated
        /// according to the latitude and longitude of the point. So in a given grid
        /// the southeast corner is the first point, and we proceed west along a
        /// given row until we reach the maximum longitude, and then move one step
        /// north for the next row.
        /// \verbatim
        /// The offset is the offset of current grid,
        /// 		plus the offset of the current row
        ///		    plus the offset of the current column
        /// or,
        /// 		gridind[i]->offset +
        ///		        ((int)(floor((lat - latmin)/latinterval))) * rowwidth +
        ///		        ((int)(floor((longit - longmin)/longinterval))) * RECWIDTH
        /// where
        ///		rowwidth = ((int)((longmax - longmin)/longinterval) + 1) /// RECWIDTH
        ///
        /// Usually, four points have to be retrieved, corresponding to the four
        /// points surrounding the point we are interested in. The southeast point
        /// is offsetA, the southwest point is offsetB, the northeast point is
        /// offset C, and the northwest point is offsetD. Then we use bilinear
        /// interpolation to calculate the shift at the point.
        ///
        ///    D     f        C        x = (latP - latA)/latinterval
        ///     +----+-------+         y = (longP - longA)/longinterval
        ///     |    |       | P2
        ///     |    |P      o      If G is the gridshift (lat or longit), then
        ///   h +----o-------+ g
        ///     |  { |       |         Ge = Ga + (Gb - Ga)y
        ///     |  x |~~~y~~~|         Gf = Gc + (Gd - Gc)y
        ///     |  { |       |      and
        ///     +----+---o---+         Gp = Ge + (Gf - Ge)x
        ///    B     e    P3  A
        ///
        /// Note that if the point occurs on the edge of the grid, it appears where
        /// the latitude equals latmin and/or the longitude equal longmin. This ensures
        /// that this formula will always work, even if the point happened to be at A
        /// above.
        /// \endverbatim 
        /// </remarks>
        /// <param name="lat"> - the input latitude.</param>
        /// <param name="longit"> - the input longitude.</param>
        /// <param name="difflat"> - the output latitude difference.</param>
        /// <param name="difflong"> - the output longitude difference.</param>
        /// <param name="gridIndArray"> - an array of fully populated GridStruct objects.</param>
        /// <param name="gridcount"> - grid count.</param>
        /// <param name="gridfile"> - BinaryReader object openned on the grid file.</param>
        /// <param name="dir"> - prescribed direction of conversion, either NAD27to83 or NAD83to27.</param>
        /// <returns></returns>
        public static int ReadGrid(float lat, float longit, ref float difflat, ref float difflong,
                                    GridStruct[] gridIndArray, int gridcount, BinaryReader gridfile, Enums.NTv2Dir dir)
        {
            //...Log2.v("\nAxNTv2Lib.ReadGrid(): gridind[0].ToString(): " + gridind[0].ToStringAsLines());

            // 'out' requirements.
            //difflat = 0.0f;
            //difflong = 0.0f;

            int rc = 0;

            long offsetA, offsetC;       /* File offset for point A and C			*/
            int latintA, longintA;  /* # of intervals for lat and long		*/
            int rowwidth; /* Width in bytes of rows	*/

            const int recwidth = Constant.SIZEOF_NTV2_GRID_POINT; /* Width in bytes of records	*/

            int recnumber;
            int i;

            GridPoint gridpntA;
            GridPoint gridpntB;
            GridPoint gridpntC;
            GridPoint gridpntD; /* Gridshifts at points A to D			*/

            double x, y, Ge, Gf, Gp;        /* Calculation variables				*/
            double Glat;

            for (i = 0; (i < gridcount) && (rc == 0); i++)
            {

                if ((lat >= gridIndArray[i].latmin && lat < gridIndArray[i].latmax) &&
                     (longit >= gridIndArray[i].longmin && longit < gridIndArray[i].longmax))
                {

                    if (gridIndArray[i].gridcount > 0)
                    {
                        rc = ReadGrid(lat, longit, ref difflat, ref difflong, gridIndArray[i].subgrids,
                                                    gridIndArray[i].gridcount, gridfile, dir);
                    }

                    if ((gridIndArray[i].gridcount == 0 || (rc == 0)) &&
                       (gridIndArray[i].longinterval != 0 &&
                            gridIndArray[i].latinterval != 0))
                    {

                        rowwidth = ((int)((gridIndArray[i].longmax - gridIndArray[i].longmin)
                                                            / gridIndArray[i].longinterval) + 1) * recwidth;
                        latintA = (int)(Math.Floor((lat - gridIndArray[i].latmin) / gridIndArray[i].latinterval));
                        longintA = (int)(Math.Floor((longit - gridIndArray[i].longmin) /
                                                                     gridIndArray[i].longinterval));
                        recnumber = (((int)((gridIndArray[i].longmax - gridIndArray[i].longmin) /
                                                                gridIndArray[i].longinterval) + 1) * latintA) + longintA;
                        offsetA = gridIndArray[i].offset + (latintA * rowwidth) + (longintA * recwidth);
                        offsetC = rowwidth - (recwidth * 2);

                        // ReadSingle() reads a 4-byte floating point value from the current stream and 
                        // advances the current position of the stream by four bytes.

                        // Set offset relative to beginning of the file. 
                        gridfile.BaseStream.Seek(offsetA, SeekOrigin.Begin);

                        gridpntA.lat_shift = gridfile.ReadSingle();
                        gridpntA.long_shift = gridfile.ReadSingle();

                        gridpntB.lat_shift = gridfile.ReadSingle();
                        gridpntB.long_shift = gridfile.ReadSingle();

                        // Set offset realtive to current position in the file.
                        gridfile.BaseStream.Seek(offsetC, SeekOrigin.Current);

                        gridpntC.lat_shift = gridfile.ReadSingle();
                        gridpntC.long_shift = gridfile.ReadSingle();

                        gridpntD.lat_shift = gridfile.ReadSingle();
                        gridpntD.long_shift = gridfile.ReadSingle();

                        x = (lat - ((latintA * gridIndArray[i].latinterval) + gridIndArray[i].latmin)) /
                                        gridIndArray[i].latinterval;
                        y = (longit - ((longintA * gridIndArray[i].longinterval) + gridIndArray[i].longmin)) /
                              gridIndArray[i].longinterval;

                        Ge = gridpntA.lat_shift + ((gridpntB.lat_shift - gridpntA.lat_shift) * y);
                        Gf = gridpntC.lat_shift + ((gridpntD.lat_shift -
                                                                                gridpntC.lat_shift) * y);
                        Gp = Ge + ((Gf - Ge) * x);
                        Glat = Gp;

                        if (dir == Enums.NTv2Dir.NAD27to83)
                        {
                            difflat = (float)Gp;
                        }
                        else
                        {
                            difflat = (float)((-1.0) * Gp);
                        }

                        Ge = gridpntA.long_shift + ((gridpntB.long_shift - gridpntA.long_shift) * y);
                        Gf = gridpntC.long_shift + ((gridpntD.long_shift - gridpntC.long_shift) * y);
                        Gp = Ge + ((Gf - Ge) * x);

                        if (dir == Enums.NTv2Dir.NAD27to83)
                        {
                            difflong = (float)Gp;
                        }
                        else
                        {
                            difflong = (float)((-1.0) * Gp);
                        }

                        rc = 1;

                    } // if

                } // if

            } // Loop over i.
            return rc;
        }


    }
}
