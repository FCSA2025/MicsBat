using System;

namespace Interpol
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHDBC = IntPtr;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides the methods that perform the interpolation of
    /// 'sparse' values for the vertical, horizontal,  copolar and 
    /// cross-polar antenna discriminations.
    /// </summary>
    public class Transforms
    {
        private static uint[] mFlags = new uint[1] { 0 };

        /// <summary>
        /// This is the top-level method that performs the interpolation of
        /// 'sparse' values for the vertical, horizontal,  copolar and 
        /// cross-polar antenna discriminations.
        /// </summary>
        /// <param name="fullTableName"></param>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public static int PopulatePolarDiscrims(string fullTableName, string aCode)
        {
            /*  It is a normal interpolate request. */
            //...Log2.v("\nTransforms.PopulatePolarDiscrims(): interpolate --->");

            uint[] tempinterp = new uint[1];
            int ctr = 0;
            int nRet;
            int cursorHandle;
            int numRecords = 0;

            SuAntd[] suAntds = new SuAntd[Constant.MAXINFLECPTS];

            SQLLEN[][] nullInds = new SQLLEN[Constant.MAXINFLECPTS][];
            for (int i = 0; i < Constant.MAXINFLECPTS; i++)
            {
                nullInds[i] = NullHelper.CreateArrayOfNullInd(SuAntd.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            }

            string whereClause = String.Format("acode = '{0}' AND cmd <> 'D'", aCode);
            string orderByClause = "antang;";

            cursorHandle = SuDynAntd.SuSelectAntd(fullTableName, whereClause, orderByClause);

            if (cursorHandle < 0)
            {
                Log2.e("\nTransforms.PopulatePolarDiscrims(): ERROR: call to SuDynAntd.SuSelectAntd() failed.");
                return (Error.ODBC_SELECT_FAILED);
            }

            ctr = 0;
            while (true)
            {
                //...Log2.v("\nTransforms.PopulatePolarDiscrims(): ctr = " + ctr);

                if (ctr >= suAntds.Length)
                {
                    Log2.e("\nTransforms.PopulatePolarDiscrims(): ERROR: number of records fetched exceeds length of suAntds[].");
                    nRet = 111;
                    break;
                }

                //sqlRet = ODBC.SQLFetch(hStmt);
                nRet = SuDynAntd.SuFetchAntd(cursorHandle, out suAntds[ctr], out nullInds[ctr]);

                if (nRet == Constant.NOMORERECS)
                {
                    // We have read all the selected records.
                    //...Log2.v("\nTransforms.PopulatePolarDiscrims(): nRet == Constant.NOMORERECS");
                    numRecords = ctr;
                    nRet = 0;
                    break;
                }
                else if (nRet != Constant.SUCCESS)
                {
                    Log2.e("\nTransforms.PopulatePolarDiscrims(): ERROR: call to SuFetchAntd() failed, nRet = " + nRet);
                    nRet = 110;
                    break;
                }

                // If the fetched uAntds[ctr].interpstat has a NULL indicator
                // default its value to zero.
                if (nullInds[ctr][SuAntd.INTERPSTAT] == Constant.DB_NULL)
                {
                    suAntds[ctr].interpstat = 0;
                    nullInds[ctr][SuAntd.INTERPSTAT] = Constant.DB_NOT_NULL;
                }

                // To use the GenUtil bit methods the flags have to be int[].
                mFlags[0] = (uint)suAntds[ctr].interpstat;

                // A value is interpolated if it is null on input, or if the interpstat
                // bit is already set on input.
                // We go through the four polarization components vv vh hh hv.
                if (nullInds[ctr][SuAntd.DCOV] == Constant.DB_NULL)
                {
                    GenUtil.UtSetBit(ref mFlags, Constant.DCOVBIT);
                }

                if (nullInds[ctr][SuAntd.DXPV] == Constant.DB_NULL)
                {
                    GenUtil.UtSetBit(ref mFlags, Constant.DXPVBIT);
                }

                if (nullInds[ctr][SuAntd.DCOH] == Constant.DB_NULL)
                {
                    GenUtil.UtSetBit(ref mFlags, Constant.DCOHBIT);
                }


                if (nullInds[ctr][SuAntd.DXPH] == Constant.DB_NULL)
                {
                    GenUtil.UtSetBit(ref mFlags, Constant.DXPHBIT);
                }

                // Update the interpstat bit flags.
                suAntds[ctr].interpstat = (int)mFlags[0];

                // To mimic the behaviour on WebMICS, if the date or time is NULL
                // replace it with an empty string.
                if (nullInds[ctr][SuAntd.MDATE] == Constant.DB_NULL)
                {
                    suAntds[ctr].mdate = "";
                    nullInds[ctr][SuAntd.MDATE] = Constant.DB_NOT_NULL;
                }

                if (nullInds[ctr][SuAntd.MTIME] == Constant.DB_NULL)
                {
                    suAntds[ctr].mtime = "";
                    nullInds[ctr][SuAntd.MTIME] = Constant.DB_NOT_NULL;
                }

                // Increment the fetch-loop counter.
                ctr++;

            } // fetching records loop.

            //...Log2.v("\nTransforms.PopulatePolarDiscrims(): numRecords = " + numRecords);

            // Return to caller if any problems occurred in the fetch loop.
            if (nRet != Constant.SUCCESS)
            {
                Log2.e("\nTransforms.PopulatePolarDiscrims(): ERROR: on break from fetch-loop, nRet = " + nRet);
                return nRet;
            }

            /* perform the interpolation */
            nRet = Transforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(ref suAntds, ref nullInds, numRecords);

            // Catch any problems.
            if (nRet != Constant.SUCCESS)
            {
                Log2.e("\nTransforms.PopulatePolarDiscrims(): ERROR: call to Populate_dcov_dxpv_dcoh_dxph_interpstat() returned nRet = " + nRet);
                GenUtil.SetErr(String.Format("interpol05: Interpolation error ({0})", nRet));
                return 122;
            }


            // Now update all the records in the subsidiary anteena table that we previously fetched.
            for (int i = 0; i < numRecords; i++)
            {
                //...Log2.v(String.Format("\ni = {0}\n{1}\n", i, suAntds[i].ToStringWN(nullInds[i])));

                nRet = SuDynAntd.SuUpdateAntd(cursorHandle, suAntds[i], nullInds[i]);

                if (nRet != Constant.SUCCESS)
                {
                    Log2.e("\nTransforms.PopulatePolarDiscrims()(): ERROR: call to SuUpdateAntd() failed, returned nRet = " + nRet);
                }
            } // Records update loop.


            SuDynAntd.SuCloseAntd(cursorHandle);

            return (nRet);

        } // normal interpolate request.      

        /// <summary>
        /// This worker method encapsulated the detailed checks and calculations required
        /// to interpolate between sparse dcov, dxpv, dcoh, dxph values.
        /// </summary>
        /// <param name="suAntds"></param>
        /// <param name="nullInds"></param>
        /// <param name="numPts"></param>
        /// <returns></returns>
        private static int Populate_dcov_dxpv_dcoh_dxph_interpstat(ref SuAntd[] suAntds, ref SQLLEN[][] nullInds, int numPts)
        {
            //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Entry");

            int retcode = Constant.SUCCESS;
            bool error = false;
            int ctr = 0;
            int lowctr = 0;
            int highctr = 0;

            /* contents of array of struct should be sorted first */
            /*	sortAntePts(antePt, numPts);  *//* Assume they are ordered */

            /* check to ensure that the 0 and 180 (or 359.9) points are filled in */
            if (numPts < 2)
            {
                /* at least 0 and 180 (or 0 and 359.9) MUST be given */
                retcode = Error.TOOFEWANGLES;
                error = true;
            }
            else
            {
                /* sufficient number of points, make sure ends are there */
                if (CheckLastPt(suAntds[numPts - 1], ref nullInds[numPts - 1]) != Constant.SUCCESS)
                {
                    error = true;
                    retcode = Error.NOLASTANGLE;

                    if (suAntds[numPts - 1].antang > 180.0)
                    {
                        retcode = Error.NOLASTANGLE359;
                    }
                }
                else
                {
                    if ((CheckFirstPt(suAntds[0], ref nullInds[0])) != Constant.SUCCESS)
                    {
                        retcode = Error.NOFIRSTANGLE;
                        error = true;
                    }
                }
            }

            /* if all is well, perform calculations for a column */
            if (error == false)
            {
                /* perform calculations for Vertical Copolar column first */
                /* Rem: antePt[0] MUST be antang 0.0 and MUST be given */

                /* for each row in this column except first and last */
                for (ctr = 1; ctr < numPts - 1; ctr++)
                {
                    mFlags[0] = (uint)suAntds[ctr].interpstat;

                    if (GenUtil.UtTestBit(mFlags, Constant.DCOVBIT) == Enums.BIT.CLEAR)
                    {
                        /* interp flag not set so dont interpolate */
                        continue;
                    }

                    /* find next 'lower' point given by user */
                    for (lowctr = ctr - 1; lowctr >= 0; lowctr--)
                    {
                        mFlags[0] = (uint)suAntds[lowctr].interpstat;
                        if (GenUtil.UtTestBit(mFlags, Constant.DCOVBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next lower user given value */
                            break;
                        }
                    }
                    if (lowctr < 0)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: A: returned " + Error.NOFIRSTANGLE);
                        return (Error.NOFIRSTANGLE);
                    }

                    /* find next 'higher' point given by user */
                    for (highctr = ctr + 1; highctr < numPts; highctr++)
                    {
                        mFlags[0] = (uint)suAntds[highctr].interpstat;
                        if (GenUtil.UtTestBit(mFlags, Constant.DCOVBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next higher user given value */
                            break;
                        }
                    }
                    if (highctr > numPts)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: B: returned " + Error.NOLASTANGLE);
                        return (Error.NOLASTANGLE);
                    }

                    // Calculate the point required.
                    Interpolate(suAntds[ctr].antang, suAntds[lowctr].antang,
                        suAntds[highctr].antang, suAntds[lowctr].dcov,
                        suAntds[highctr].dcov, out suAntds[ctr].dcov);

                    // Set the nullInd.
                    nullInds[ctr][SuAntd.DCOV] = Constant.DB_NOT_NULL;
                }


                /* perform calculations for Vertical Crosspolar column */
                /* Rem: antePt[0] MUST be antang 0.0 and MUST be given */

                /* for each row in this column except first and last */
                for (ctr = 1; ctr < numPts - 1; ctr++)
                {
                    mFlags[0] = (uint)suAntds[ctr].interpstat;

                    if (GenUtil.UtTestBit(mFlags, Constant.DXPVBIT) == Enums.BIT.CLEAR)
                    {
                        /* interp flag not set so dont interpolate */
                        continue;
                    }

                    /* find next 'lower' point given by user */
                    for (lowctr = ctr - 1; lowctr >= 0; lowctr--)
                    {
                        mFlags[0] = (uint)suAntds[lowctr].interpstat;

                        if (GenUtil.UtTestBit(mFlags, Constant.DXPVBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next lower user given value */
                            break;
                        }
                    }
                    if (lowctr < 0)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: C: returned " + Error.NOFIRSTANGLE);
                        return (Error.NOFIRSTANGLE);
                    }


                    /* find next 'higher' point given by user */
                    for (highctr = ctr + 1; highctr < numPts; highctr++)
                    {
                        mFlags[0] = (uint)suAntds[highctr].interpstat;

                        if (GenUtil.UtTestBit(mFlags, Constant.DXPVBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next higher user given value */
                            break;
                        }
                    }
                    if (highctr > numPts)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: D: returned " + Error.NOLASTANGLE);
                        return (Error.NOLASTANGLE);
                    }

                    /* calculate the point required */
                    Interpolate(suAntds[ctr].antang, suAntds[lowctr].antang,
                        suAntds[highctr].antang, suAntds[lowctr].dxpv,
                        suAntds[highctr].dxpv, out suAntds[ctr].dxpv);

                    // Set the nullInd.
                    nullInds[ctr][SuAntd.DXPV] = Constant.DB_NOT_NULL;
                }


                /* perform calculations for Horizontal Copolar column */
                /* Rem: antePt[0] MUST be antang 0.0 and MUST be given */

                /* for each row in this column except first and last */
                for (ctr = 1; ctr < numPts - 1; ctr++)
                {
                    mFlags[0] = (uint)suAntds[ctr].interpstat;

                    if (GenUtil.UtTestBit(mFlags, Constant.DCOHBIT) == Enums.BIT.CLEAR)
                    {
                        /* interp flag not set so dont interpolate */
                        continue;
                    }

                    /* find next 'lower' point given by user */
                    for (lowctr = ctr - 1; lowctr >= 0; lowctr--)
                    {
                        mFlags[0] = (uint)suAntds[lowctr].interpstat;
                        if (GenUtil.UtTestBit(mFlags, Constant.DCOHBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next lower user given value */
                            break;
                        }
                    }
                    if (lowctr < 0)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: E: returned " + Error.NOFIRSTANGLE);
                        return Error.NOFIRSTANGLE;
                    }

                    /* find next 'higher' point given by user */
                    for (highctr = ctr + 1; highctr < numPts; highctr++)
                    {
                        mFlags[0] = (uint)suAntds[highctr].interpstat;

                        if (GenUtil.UtTestBit(mFlags, Constant.DCOHBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next higher user given value */
                            break;
                        }
                    }
                    if (highctr > numPts)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: F: returned " + Error.NOLASTANGLE);
                        return (Error.NOLASTANGLE);
                    }

                    /* calculate the point required */
                    Interpolate(suAntds[ctr].antang, suAntds[lowctr].antang,
                        suAntds[highctr].antang, suAntds[lowctr].dcoh,
                        suAntds[highctr].dcoh, out suAntds[ctr].dcoh);

                    // Set the nullInd.
                    nullInds[ctr][SuAntd.DCOH] = Constant.DB_NOT_NULL;
                }


                /* perform calculations for Horizontal Crosspolar column */
                /* Rem: antePt[0] MUST be antang 0.0 and MUST be given */

                /* for each row in this column except first and last */
                for (ctr = 1; ctr < numPts - 1; ctr++)
                {
                    mFlags[0] = (uint)suAntds[ctr].interpstat;

                    if (GenUtil.UtTestBit(mFlags, Constant.DXPHBIT) == Enums.BIT.CLEAR)
                    {
                        /* interp flag not set so dont interpolate */
                        continue;
                    }

                    /* find next 'lower' point given by user */
                    for (lowctr = ctr - 1; lowctr >= 0; lowctr--)
                    {
                        mFlags[0] = (uint)suAntds[lowctr].interpstat;

                        if (GenUtil.UtTestBit(mFlags, Constant.DXPHBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next lower user given value */
                            break;
                        }
                    }
                    if (lowctr < 0)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: G: returned " + Error.NOFIRSTANGLE);
                        return (Error.NOFIRSTANGLE);
                    }

                    /* find next 'higher' point given by user */
                    for (highctr = ctr + 1; highctr < numPts; highctr++)
                    {
                        mFlags[0] = (uint)suAntds[highctr].interpstat;

                        if (GenUtil.UtTestBit(mFlags, Constant.DXPHBIT) == Enums.BIT.CLEAR)
                        {
                            /* found next higher user given value */
                            break;
                        }
                    }
                    if (highctr > numPts)
                    {
                        //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit: H: returned " + Error.NOLASTANGLE);
                        return (Error.NOLASTANGLE);
                    }

                    /* calculate the point required */
                    Interpolate(suAntds[ctr].antang, suAntds[lowctr].antang,
                        suAntds[highctr].antang, suAntds[lowctr].dxph,
                        suAntds[highctr].dxph, out suAntds[ctr].dxph);

                    // Set the nullInd.
                    nullInds[ctr][SuAntd.DXPH] = Constant.DB_NOT_NULL;
                }
            }

            //...Log2.v("\nTransforms.Populate_dcov_dxpv_dcoh_dxph_interpstat(): Exit, retcode = " + retcode);
            return (retcode);
        }

        /// <summary>
        /// This method checks the validity of the angle (antang) prescribed in the
        /// final subsidiary antenna detail record, sorted by antang.
        /// </summary>
        /// <param name="suAntd"></param>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        private static int CheckLastPt(SuAntd suAntd, ref SQLLEN[] nullInds)
        {
            //...Log2.v("\nTransforms.CheckLastPt(): Entry, suAntd.antang = " + suAntd.antang);

            //...Log2.v("\nTransforms.CheckLastPt(): suAntd = " + suAntd.ToStringWN(nullInds));

            int rc = Constant.SUCCESS;

            if (suAntd.antang > 180.0)
            {
                if (!(((suAntd.antang - 359.9) < 0.15) &&
                    ((suAntd.antang - 359.9) > -0.15)))
                {
                    //...Log2.v("\nTransforms.CheckLastPt(): A");
                    rc = Constant.FAILURE;
                }
            }
            else
            {
                if (suAntd.antang != 180.0)
                {
                    //...Log2.v("\nTransforms.CheckLastPt(): B");
                    rc = Constant.FAILURE;
                }
            }

            if ((suAntd.interpstat != 0) ||
                (nullInds[SuAntd.DCOV] == Constant.DB_NULL) ||
                (nullInds[SuAntd.DXPV] == Constant.DB_NULL) ||
                (nullInds[SuAntd.DCOH] == Constant.DB_NULL) ||
                (nullInds[SuAntd.DXPH] == Constant.DB_NULL))
            {
                //...Log2.v("\nTransforms.CheckLastPt(): C");
                rc = Constant.FAILURE;
            }

            //...Log2.v("\nTransforms.CheckLastPt(): Exit: rc = " + rc);
            return (rc);

        }   /* ----- End of checkLastPt ----- */

        /// <summary>
        /// This method checks the validity of the angle (antang) prescribed in the
        /// first subsidiary antenna detail record, sorted by antang.
        /// </summary>
        /// <param name="suAntd"></param>
        /// <param name="nullInds"></param>
        /// <returns></returns>
        private static int CheckFirstPt(SuAntd suAntd, ref SQLLEN[] nullInds)
        {

            int rc = Constant.SUCCESS;

            if (suAntd.antang != 0.0)
            {
                rc = Constant.FAILURE;
            }

            if ((suAntd.interpstat != 0)
                || (nullInds[SuAntd.DCOV] == Constant.DB_NULL)
                || (nullInds[SuAntd.DXPV] == Constant.DB_NULL)
                || (nullInds[SuAntd.DCOH] == Constant.DB_NULL)
                || (nullInds[SuAntd.DXPH] == Constant.DB_NULL))
            {
                rc = Constant.FAILURE;
            }
            return (rc);

        }   /* ----- End of checkFirstPt ----- */

        /// <summary>
        /// This method returns the linearly-interpolated antenna discrimination for 
        /// a given angle between two prescribed (angle, discrimination) points.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="minAng"></param>
        /// <param name="maxAng"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <param name="result"></param>
        private static void Interpolate(float angle,    /* input  - angle at which to find unknown val*/
                                        float minAng,   /* input  - closest known angle < 'angle' */
                                        float maxAng,   /* input  - closest known angle > 'angle' */
                                        float minVal,   /* input  - value at minAng */
                                        float maxVal,   /* input  - value at maxAng */
                                    out float result)   /* output - computed value at 'angle' */
        {

            if (angle >= maxAng)
            {
                /* if angle is greater than max - set result to maxVal */
                result = maxVal;
            }
            else if (angle <= minAng)
            {
                /* if angle is less than min - set result to minVal */
                result = minVal;
            }
            else
            {
                /* angle is between max and min so do interpolation */
                if (minVal == maxVal)
                {
                    /* max and min values are same, straigt line
                     * interpolated value is same value
                     */
                    result = minVal;
                }
                else
                {
                    /* do actual interpolation */
                    result = ((maxVal - minVal) * (angle - minAng)) / (maxAng - minAng) + minVal;
                }
            }
        }

        /// <summary>
        /// This is the top-level method that performs the 'uninterpolate' transformation
        /// of a previously interpolated subsidiary antenna detail record.
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="fullTableName"></param>
        /// <param name="aCode"></param>
        /// <returns></returns>
        public static int RevertToSparseDiscrims(string dbName, string fullTableName, string aCode)
        {
            int nRet = 0;
            int cursorHandle;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHDBC hUpConn = Ssutil.NewConn();

            SuAntd suAntd;
            SQLLEN[] nullInds;

            /*HEADER START*****************************************************************
            *
            *   Go through the records, and for any with interpolation set, set the
            *   values to null.  Zero the interpstat bits.
            *
            ******************************************************************HEADER END*/

            string whereClause = String.Format(" acode = '{0}' and interpstat <> 0 ", aCode);

            cursorHandle = SuDynAntd.SuSelectAntd(fullTableName, whereClause, "");

            if (cursorHandle < 0)
            {
                Log2.e("\nTransforms.RevertToSparseDiscrims(): ERROR: call to SuDynAntd.SuSelectAntd() failed.");
                return (Error.ODBC_SELECT_FAILED);
            }

            while (true)
            {
                //sqlRet = ODBC.SQLFetch(hStmt);
                nRet = SuDynAntd.SuFetchAntd(cursorHandle, out suAntd, out nullInds);

                if (nRet == Constant.NOMORERECS)
                {
                    // We have read all the selected records.
                    nRet = 0;
                    break;
                }
                else if (nRet != Constant.SUCCESS)
                {
                    Log2.e("\nTransforms.RevertToSparseDiscrims(): ERROR: call to SuFetchAntd() failed, nRet = " + nRet);
                    nRet = 110;
                    break;
                }

                // To use the GenUtil bit methods the flags have to be int[].
                mFlags[0] = (uint)suAntd.interpstat;

                if (GenUtil.UtTestBit(mFlags, Constant.DCOVBIT) == Enums.BIT.SET)
                {
                    nullInds[SuAntd.DCOV] = Constant.DB_NULL;
                }

                if (GenUtil.UtTestBit(mFlags, Constant.DXPVBIT) == Enums.BIT.SET)
                {
                    nullInds[SuAntd.DXPV] = Constant.DB_NULL;
                }

                if (GenUtil.UtTestBit(mFlags, Constant.DCOHBIT) == Enums.BIT.SET)
                {
                    nullInds[SuAntd.DCOH] = Constant.DB_NULL;
                }

                if (GenUtil.UtTestBit(mFlags, Constant.DXPHBIT) == Enums.BIT.SET)
                {
                    nullInds[SuAntd.DXPH] = Constant.DB_NULL;
                }

                // zero out the interpolation bit-indicators.
                suAntd.interpstat = 0;

                // Update all columns in the current record with new values.
                nRet = SuDynAntd.SuUpdateAntd(cursorHandle, suAntd, nullInds);

                if (nRet != Constant.SUCCESS)
                {
                    Log2.e("\nTransforms.RevertToSparseDiscrims(): ERROR: call to SuUpdateAntd() failed, returned nRet = " + nRet);
                }

            }

            SuDynAntd.SuCloseAntd(cursorHandle);

            return (nRet);
        }




    }
}
