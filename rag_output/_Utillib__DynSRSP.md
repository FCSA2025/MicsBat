# Documented File: DynSRSP.cs
**Repository Path:** `_Utillib\DynSRSP.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
    using SQLINTEGERPTR = IntPtr;
    using SQLLEN = Int64;
    using SQLLENPTR = IntPtr;
    using SQLPOINTER = IntPtr;
    using SQLRETURN = Int16;
    using SQLSETPOSIROW = UInt64;
    using SQLSMALLINT = Int16;
    using SQLSMALLINTPTR = IntPtr;
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// Provides methods to select and fetch SRSP records from a prescribed database table.
    /// <b>main.mt_site</b>.
    /// </summary>
    public class DynSRSP
    {
        private static List<SRSP> mSRSPList = new List<SRSP>();
        private static string mTableName = "";

        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //===================================================================================================================================

        /// <summary>
        /// This method returns the integer index of the next free (available) cursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para>- integer index of the next free cursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor()
        {
            //Get next free cursor area.
            int curHandle = -1;
            if (nNextFreeCursor >= Constant.NUM_CURSORS_FEW)
            {
                //	Search for a free cursors in the list
                for (int nInd = 0; nInd < Constant.NUM_CURSORS_FEW; nInd++)
                {
                    if (!cursors[nInd].cursorOpen)
                    {
                        curHandle = nInd;
                        break;
                    }
                }
                if (curHandle == -1)
                {
                    //There are no more open cursors
                    GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    return Error.NO_CURSOR_AVAILABLE;
                    // Application.Exit("\r\nDynAntenna.GetNextFreeCursor(): ERROR: No available cursors.");
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            cursors[curHandle].cursorOpen = true;

            return curHandle;
        }

        /// <summary>
        /// This method returns the integer index of the next free (available) FtCursor object
        /// for a prescribed table name.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free FtCursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor(string tableName)
        {
            int nextFreeCursor = GetNextFreeCursor();

            cursors[nextFreeCursor].tableName = tableName;

            return nextFreeCursor;
        }

        //----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Selects SRSP records from a prescribed database table using
        /// prescribed SQL search criteria and ordering clauses; the 
        /// method returns an index to a cursor object that can be used by subsequent
        /// calls to FetchSRSP().
        /// </summary>
        /// <param name="table"></param>
        /// <param name="searchCriteria"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public static int SelectSRSP(string table, string searchCriteria, string orderBy)
        {
            int curHandle;                  /* cursor handle */

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            StringBuilder sb = new StringBuilder();
            sb.Append(" select ");
            sb.Append(SRSP.AllColumnsForSqlSelect);
            sb.Append(" from ");
            sb.Append(table);
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                sb.Append("where ");
                sb.Append(searchCriteria);
            }
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                sb.Append(" order by ");
                sb.Append(orderBy);
            }
            string cSQL = sb.ToString();

            //	Get next free cursor area
            curHandle = GetNextFreeCursor();

            //	allocate and open the handle
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            //	Inititialize the cursor handle structure
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].tableName = table;

            return curHandle;
        }

        /// <summary>
        /// Retrieves a single row of SRSP data from a prescribed database table using 
        /// the cursor object created by a previous call to SelectSRSP().
        /// </summary>
        /// <param name="nCursor"></param>
        /// <param name="srsp"></param>
        /// <param name="nullInd"></param>
        /// <returns></returns>
        public static int FetchSRSP(int nCursor,		        //handle returned by 'ftSelectTitle'
                                        out SRSP srsp,          // caller's struct for row of data
                                        out SQLLEN[] nullInd)   // caller's array of null indicators
        {
            // Satisfy 'out' requirement.
            srsp = new SRSP();
            nullInd = NullHelper.CreateArrayOfNullInd(SRSP.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[nCursor].hStmt;

            sqlRet = ODBC.SQLFetch(hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    return Constant.FAILURE;
                }
            }

            //	Now read in the fields
            try
            {
                Ssutil.DbGetString(hStmt, 1, "srsp", out srsp.srspID, SRSP.SRSP_SZ, out nullInd[SRSP.SRSPID]);
                Ssutil.DbGetInt(hStmt, 2, "span", out srsp.span, out nullInd[SRSP.SPAN]);
                Ssutil.DbGetInt(hStmt, 3, "of", out srsp._of, out nullInd[SRSP._OF]);
                Ssutil.DbGetInt(hStmt, 4, "freqLoKHz", out srsp.freqLoKHz, out nullInd[SRSP.FREQLOKHZ]);
                Ssutil.DbGetInt(hStmt, 5, "freqHiKHz", out srsp.freqHiKHz, out nullInd[SRSP.FREQHIKHZ]);
                Ssutil.DbGetString(hStmt, 6, "title", out srsp.title, SRSP.TITLE_SZ, out nullInd[SRSP.TITLE]);
                Ssutil.DbGetBit(hStmt, 7, "applicMICS", out srsp.applicMICS, out nullInd[SRSP.APPLICMICS]);
                Ssutil.DbGetString(hStmt, 8, "pubDate", out srsp.pubDate, SRSP.PUBDATE_SZ, out nullInd[SRSP.PUBDATE]);
                Ssutil.DbGetString(hStmt, 9, "html_url", out srsp.html_url, SRSP.HTML_URL_SZ, out nullInd[SRSP.HTML_URL]);
                Ssutil.DbGetString(hStmt, 10, "pdf_url", out srsp.pdf_url, SRSP.PDF_URL_SZ, out nullInd[SRSP.PDF_URL]);
            }
            catch (Exception e)
            {
                GenUtil.SetErr("dynSite02: Input error on field: " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Closes (releases) a cursor that was previously created by a call to SelectSRSP(); 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"></param>
        /// <returns></returns>
        public static int CloseSRSP(int curHandle)
        {
            if (curHandle >= Constant.NUM_CURSORS_FEW || curHandle < 0)
            {
                /* Bad handle */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // Close the cursor.

            // First close and release the statement handle.
            SQLHANDLE hStmt = cursors[curHandle].hStmt;
            //!!ODBC.SQLCloseCursor(hStmt);
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            // Now disconnect from ODBC and free the handle.
            // Use Ssutil.DisConn to close the connection because it maintains a count
            // of the number of open connections.
            Ssutil.DisConn(cursors[curHandle].hConn);

            // Modify the cursor accordingly.
            cursors[curHandle].hConn = SQLHDBC.Zero;
            cursors[curHandle].cursorOpen = false;

            return 0;
        }

        /// <summary>
        /// This method outputs a list of all SRSP objects corresponding to all of the records
        /// in the SDB table main.sd_band.
        /// </summary>
        /// <param name="table"> - the fully-qualified name of the SQL lookup table.</param>
        /// <param name="srspList"> - output list of all SRSP objects.</param>
        /// <returns></returns>
        public static int GetSRSPlist(string table, out List<SRSP> srspList)
        {
            // "out" requiremment.
            srspList = new List<SRSP>();

            int nRet = Constant.SUCCESS;

            // Check if we already created it.
            if ((mTableName.Equals(table)) && (mSRSPList != null) && (mSRSPList.Count > 0))
            {
                srspList = mSRSPList;
            }
            else
            {
                // We need to create it.

                SRSP srsp;
                SQLLEN[] nullInds;

                int handle = SelectSRSP(table, "", "");
                if (handle < 0) return Constant.FAILURE;

                while (FetchSRSP(handle, out srsp, out nullInds) == Constant.SUCCESS)
                {
                    srspList.Add(srsp);

                    //...Log2.v("\nDynSdbBand.GetSdbBandList(): " + sdBand.ToStringAsCSV(nullInds));
                }

                CloseSRSP(handle);

                // Save a private copy to speed up subsequent calls.
                mSRSPList = srspList;
                mTableName = table;
            }

            return nRet;
        }

        /// <summary>
        /// This method returns the SRSP object with the prescribed bndcde.
        /// </summary>
        /// <param name="table"> - the fully-qualified name of the SQL lookup table.</param>
        /// <param name="freqKHz"> - the prescribed frequency (KHz).</param>
        /// <param name="srspFound"> - the SRSP object that is found.</param>
        /// <returns></returns>
        public static SRSP GetSRSP(string table, double freqKHz, out SRSP srspFound)
        {
            srspFound = null;

            List<SRSP> srspList;

            GetSRSPlist(table, out srspList);

            foreach (SRSP srsp in srspList)
            {
                if (Maths.InRange(srsp.freqLoKHz, srsp.freqHiKHz, Math.Round(freqKHz)))
                {
                    srspFound = srsp;
                    break;
                }
            }

            return srspFound;
        }





    }
}

```
